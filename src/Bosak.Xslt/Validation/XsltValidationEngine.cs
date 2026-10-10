// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Walks all checked slots across all snapshot modules and compiles them in their real static context.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — controlled validation and preview API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice A)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Parser;
using Bosak.XPath.Parser.Ast;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Patterns;

namespace Bosak.Xslt.Validation;

/// <summary>
/// The engine behind <see cref="XsltValidation"/>. One instance serves one validation run; it holds
/// only per-run accumulation state. The walk reuses the authoring snapshot's module descriptors,
/// slot classification (<see cref="AuthoringAttributeSlotKind"/>) and per-slot
/// <see cref="ExpressionSlotContext"/> (namespaces, xpath-default-namespace, base URI, effective
/// version), so every expression is compiled through the public
/// <see cref="XPath31Expression.Compile(string, CompileOptions)"/> entry point in the same static
/// context the stylesheet itself declares. Nothing here writes: the snapshot and its module
/// envelopes are read-only for the lifetime of the run.
/// </summary>
internal sealed class XsltValidationEngine
{
    internal const string SourceFailureCode = "XV0001";
    internal const string RefusalCode = "XV0002";
    internal const string UncodedEngineCode = "XV0003";

    private static readonly Regex ErrorCodePattern = new(
        @"\b(XPST|XQST|XPTY|XTSE|XTDE|XTTE|XTRE|XTMM|XTMO|FOER)[0-9]{4}\b",
        RegexOptions.Compiled);

    private static readonly XNamespace XsltNs = AttributeSlotClassifier.XsltNamespace;

    private readonly XsltValidationOptions _options;
    private readonly List<XsltValidationDiagnostic> _diagnostics = new();
    private readonly List<XsltValidationCoverageGap> _gaps = new();
    private readonly HashSet<string> _recordedGaps = new(StringComparer.Ordinal);
    private bool _hasSourceFailure;
    private bool _hasInvalid;
    private bool _hasRefusal;
    private IReadOnlyList<StaticVariableName> _staticGlobals = Array.Empty<StaticVariableName>();
    private VariableScope _globals = VariableScope.Empty;

    public XsltValidationEngine(XsltValidationOptions? options)
    {
        _options = options ?? new XsltValidationOptions();
    }

    /// <summary>
    /// Builds the distinct structural-outcome result for a principal module whose bytes could not
    /// even become an authoring source or snapshot (encoding, byte validity, XML structure).
    /// </summary>
    internal static XsltValidationResult SourceFailureResult(AuthoringFailure failure)
    {
        var diagnostic = new XsltValidationDiagnostic(
            SourceFailureCode,
            failure.ModuleUri,
            failure.Message,
            failure.Range);
        return new XsltValidationResult(
            XsltValidationOutcome.InvalidSource,
            new[] { diagnostic },
            Array.Empty<XsltValidationCoverageGap>());
    }

    public XsltValidationResult Validate(AuthoringSnapshot snapshot)
    {
        CollectGlobalDeclarations(snapshot);
        foreach (var module in snapshot.Modules)
        {
            ReportEdgeDiagnostics(module);
            WalkModule(snapshot, module);
        }

        var outcome = DecideOutcome();
        return new XsltValidationResult(
            outcome,
            Array.AsReadOnly(_diagnostics.ToArray()),
            Array.AsReadOnly(_gaps.ToArray()));
    }

    private XsltValidationOutcome DecideOutcome()
    {
        if (_hasSourceFailure)
        {
            return XsltValidationOutcome.InvalidSource;
        }

        if (_hasInvalid)
        {
            return XsltValidationOutcome.Invalid;
        }

        if (_hasRefusal)
        {
            return XsltValidationOutcome.Refused;
        }

        return _gaps.Count > 0 ? XsltValidationOutcome.UnsupportedCoverage : XsltValidationOutcome.Valid;
    }

    // ---------------------------------------------------------------------------------------------
    // Global declarations (stylesheet-wide, across every resolved module)
    // ---------------------------------------------------------------------------------------------

    private void CollectGlobalDeclarations(AuthoringSnapshot snapshot)
    {
        var globals = new List<(string NamespaceUri, string LocalName, bool IsStatic)>();
        foreach (var module in snapshot.Modules)
        {
            var root = StylesheetRoot(module.Root);
            if (root is null || !IsStylesheetElement(root))
            {
                continue;
            }

            foreach (var child in root.Children)
            {
                if (child.Kind != AuthoringNodeKind.Element || !IsXslt(child, "variable") && !IsXslt(child, "param"))
                {
                    continue;
                }

                if (TryReadQNameName(child, out var ns, out var local))
                {
                    globals.Add((ns, local, IsStaticDeclaration(child)));
                }
            }
        }

        var scope = VariableScope.Empty;
        var statics = new List<StaticVariableName>();
        foreach (var (ns, local, isStatic) in globals)
        {
            scope = scope.Add(ns, local);
            if (isStatic)
            {
                statics.Add(new StaticVariableName(ns, local));
            }
        }

        _globals = scope;
        _staticGlobals = statics;
    }

    private static bool IsStaticDeclaration(AuthoringNodeDescriptor element)
    {
        foreach (var attribute in element.Attributes)
        {
            if (attribute.LocalName == "static" &&
                (attribute.NamespaceUri.Length == 0 || attribute.NamespaceUri == AttributeSlotClassifier.XsltNamespace))
            {
                var value = attribute.ExpandedValue.Trim();
                return value is "yes" or "true" or "1";
            }
        }

        return false;
    }

    /// <summary>
    /// Reads the declared name of an <c>xsl:variable</c>/<c>xsl:param</c> element. The value of the
    /// <c>name</c> attribute is a QName, resolved here against the namespace declarations in scope
    /// at the element (via the derived backing node). Unresolvable or malformed names are not
    /// declarations the walk can reason about and simply return <see langword="false"/>.
    /// </summary>
    private static bool TryReadQNameName(AuthoringNodeDescriptor element, out string namespaceUri, out string localName)
    {
        namespaceUri = string.Empty;
        localName = string.Empty;
        foreach (var attribute in element.Attributes)
        {
            if (attribute.LocalName != "name" || attribute.NamespaceUri.Length != 0)
            {
                continue;
            }

            var value = attribute.ExpandedValue.Trim();
            if (value.Length == 0)
            {
                return false;
            }

            if (value.StartsWith("Q{", StringComparison.Ordinal))
            {
                var close = value.IndexOf('}');
                if (close < 2)
                {
                    return false;
                }

                namespaceUri = value[2..close];
                localName = value[(close + 1)..];
                return localName.Length != 0;
            }

            var colon = value.IndexOf(':');
            if (colon < 0)
            {
                localName = value;
                return true;
            }

            var prefix = value[..colon];
            localName = value[(colon + 1)..];
            if (localName.Length == 0)
            {
                return false;
            }

            var resolved = (element.BackingObject as XElement)?.GetNamespaceOfPrefix(prefix)?.NamespaceName;
            if (resolved is null)
            {
                return false;
            }

            namespaceUri = resolved;
            return true;
        }

        return false;
    }

    // ---------------------------------------------------------------------------------------------
    // Module walk
    // ---------------------------------------------------------------------------------------------

    private void WalkModule(AuthoringSnapshot snapshot, AuthoringModuleDescriptor module)
    {
        var source = snapshot.SourceFor(module.ModuleUri);
        var map = source.CoordinateMap;
        var root = StylesheetRoot(module.Root);

        if (module.IsPrincipal && (root is null || !IsStylesheetElement(root)))
        {
            _hasInvalid = true;
            _diagnostics.Add(new XsltValidationDiagnostic(
                "XTSE0010",
                module.ModuleUri,
                "The document element is not an XSLT stylesheet: it is neither xsl:stylesheet/xsl:transform " +
                "nor a literal result element carrying an xsl:version attribute.",
                root?.Range));
            return;
        }

        if (root is null)
        {
            return;
        }

        var state = new ModuleWalkState(module, map);
        VisitElement(state, root, _globals);
    }

    private static AuthoringNodeDescriptor? StylesheetRoot(AuthoringNodeDescriptor document)
    {
        foreach (var child in document.Children)
        {
            if (child.Kind == AuthoringNodeKind.Element)
            {
                return child;
            }
        }

        return null;
    }

    private static bool IsStylesheetElement(AuthoringNodeDescriptor element)
    {
        if (element.ElementName is null)
        {
            return false;
        }

        if (element.ElementName.NamespaceUri == AttributeSlotClassifier.XsltNamespace)
        {
            return element.ElementName.LocalName is "stylesheet" or "transform" or "package";
        }

        // Literal result element as stylesheet root: xsl:version in scope.
        foreach (var attribute in element.Attributes)
        {
            if (attribute.LocalName == "version" && attribute.NamespaceUri == AttributeSlotClassifier.XsltNamespace)
            {
                return true;
            }
        }

        return false;
    }

    private void VisitElement(ModuleWalkState state, AuthoringNodeDescriptor node, VariableScope scope)
    {
        var elementName = node.ElementName;
        if (elementName is null)
        {
            return;
        }

        ReportCoverageGapIfDeferred(state, elementName);

        // Slots on this element's start tag.
        foreach (var attribute in node.Attributes)
        {
            var context = attribute.SlotContext;
            if (context is null)
            {
                continue;
            }

            switch (attribute.SlotKind)
            {
                case AuthoringAttributeSlotKind.Expression:
                    CheckExpressionSlot(state, attribute, context, scope, isUseWhen: attribute.LocalName == "use-when");
                    break;
                case AuthoringAttributeSlotKind.Pattern:
                    CheckPatternSlot(state, attribute, context);
                    break;
                case AuthoringAttributeSlotKind.Avt:
                    CheckAvtSlot(state, attribute, context, scope);
                    break;
            }
        }

        // A literal use-when="false()" removes the subtree from the stylesheet (XSLT 3.0 §3.13);
        // mirroring engine semantics, its descendant slots are not analyzed.
        if (HasLiteralFalseUseWhen(node))
        {
            return;
        }

        // Child scope: template/function/iterate parameters are visible across the whole subtree;
        // a function body sees only globals plus its own parameters (XTSE ...: outer locals are
        // not in scope inside xsl:function).
        VariableScope childScope;
        if (IsXslt(node, "function"))
        {
            childScope = _globals;
            foreach (var parameter in ParameterNames(node))
            {
                childScope = childScope.Add(parameter.NamespaceUri, parameter.LocalName);
            }
        }
        else
        {
            childScope = scope;
            if (IsXslt(node, "template") || IsXslt(node, "iterate"))
            {
                foreach (var parameter in ParameterNames(node))
                {
                    childScope = childScope.Add(parameter.NamespaceUri, parameter.LocalName);
                }
            }
        }

        // Sibling scoping: xsl:variable/xsl:param declarations are in scope for FOLLOWING siblings
        // (and their subtrees) only.
        var siblingScope = childScope;
        foreach (var child in node.Children)
        {
            if (child.Kind != AuthoringNodeKind.Element)
            {
                continue;
            }

            VisitElement(state, child, siblingScope);
            if (IsXslt(child, "variable") || IsXslt(child, "param"))
            {
                if (TryReadQNameName(child, out var ns, out var local))
                {
                    siblingScope = siblingScope.Add(ns, local);
                }
            }
        }
    }

    private static bool HasLiteralFalseUseWhen(AuthoringNodeDescriptor node)
    {
        foreach (var attribute in node.Attributes)
        {
            if (attribute.LocalName == "use-when" && attribute.SlotKind == AuthoringAttributeSlotKind.Expression)
            {
                var value = attribute.ExpandedValue.Trim();
                if (value == "false()" || value == "false")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static IEnumerable<StaticVariableName> ParameterNames(AuthoringNodeDescriptor node)
    {
        foreach (var child in node.Children)
        {
            if (child.Kind == AuthoringNodeKind.Element && IsXslt(child, "param") &&
                TryReadQNameName(child, out var ns, out var local))
            {
                yield return new StaticVariableName(ns, local);
            }
        }
    }

    private void ReportCoverageGapIfDeferred(ModuleWalkState state, AuthoringQName elementName)
    {
        if (elementName.NamespaceUri != AttributeSlotClassifier.XsltNamespace)
        {
            return;
        }

        string? construct = elementName.LocalName switch
        {
            "import-schema" => "xsl:import-schema",
            "use-package" => "xsl:use-package",
            "package" => "xsl:package",
            _ => null,
        };
        if (construct is null)
        {
            return;
        }

        var key = state.Module.ModuleUri.AbsoluteUri + "|" + construct;
        if (!_recordedGaps.Add(key))
        {
            return;
        }

        var description = construct switch
        {
            "xsl:import-schema" =>
                "Schema imports are not statically validated: schema acquisition and schema-aware " +
                "expression analysis are outside the declared validation coverage of this engine build.",
            _ =>
                "Packages are not statically validated: package acquisition and use-package " +
                "resolution are outside the declared validation coverage of this engine build.",
        };
        _gaps.Add(new XsltValidationCoverageGap(construct, state.Module.ModuleUri, description));
    }

    // ---------------------------------------------------------------------------------------------
    // Slot checks
    // ---------------------------------------------------------------------------------------------

    private void CheckExpressionSlot(
        ModuleWalkState state,
        AuthoringAttributeDescriptor attribute,
        ExpressionSlotContext context,
        VariableScope scope,
        bool isUseWhen)
    {
        var text = attribute.ExpandedValue;
        var positionsExact = attribute.RawLiteral == text;
        var compatibility = ResolveCompatibility(context);

        try
        {
            XPath31Expression.Compile(text, BuildCompileOptions(context, compatibility, isAvt: false));
        }
        catch (Exception ex)
        {
            ReportCompileDiagnostic(state, attribute, text, segmentOffsetInValue: 0, ex, positionsExact);
        }

        CheckFreeVariables(
            state, attribute, context, text, segmentOffsetInValue: 0, compatibility,
            isUseWhen ? VariableScope.Empty : scope,
            isUseWhen ? _staticGlobals : null);
    }

    private void CheckPatternSlot(
        ModuleWalkState state,
        AuthoringAttributeDescriptor attribute,
        ExpressionSlotContext context)
    {
        var text = attribute.ExpandedValue.Trim();
        if (text.Length == 0)
        {
            return;
        }

        try
        {
            _ = new PatternCompiler().Compile(text, context.XpathDefaultNamespace);
        }
        catch (Exception ex)
        {
            _hasInvalid = true;
            _diagnostics.Add(new XsltValidationDiagnostic(
                ExtractCode(ex),
                state.Module.ModuleUri,
                ex.Message,
                attribute.ValueRange));
        }
    }

    private void CheckAvtSlot(
        ModuleWalkState state,
        AuthoringAttributeDescriptor attribute,
        ExpressionSlotContext context,
        VariableScope scope)
    {
        var text = attribute.ExpandedValue;
        var positionsExact = attribute.RawLiteral == text;
        var compatibility = ResolveCompatibility(context);
        var options = BuildCompileOptions(context, compatibility, isAvt: true);

        var i = 0;
        while (i < text.Length)
        {
            var c = text[i];
            if (c == '{' && i + 1 < text.Length && text[i + 1] == '{')
            {
                i += 2;
                continue;
            }

            if (c == '}' && i + 1 < text.Length && text[i + 1] == '}')
            {
                i += 2;
                continue;
            }

            if (c != '{')
            {
                // A lone '}' is treated as literal text, mirroring the engine's AVT evaluation.
                i++;
                continue;
            }

            var end = FindAvtExpressionEnd(text, i + 1);
            if (end < 0)
            {
                _hasInvalid = true;
                _diagnostics.Add(new XsltValidationDiagnostic(
                    "XTSE0350",
                    state.Module.ModuleUri,
                    "An unescaped left curly bracket in an attribute value template does not have a " +
                    "matching right curly bracket.",
                    RangeForValuePosition(state.Map, attribute, i, 1, positionsExact)));
                break;
            }

            var expression = text.Substring(i + 1, end - i - 1);
            if (HasSubstantiveContent(expression))
            {
                try
                {
                    XPath31Expression.Compile(expression, options);
                }
                catch (Exception ex)
                {
                    ReportCompileDiagnostic(state, attribute, expression, i + 1, ex, positionsExact);
                }

                CheckFreeVariables(state, attribute, context, expression, i + 1, compatibility, scope, null);
            }

            i = end + 1;
        }
    }

    private void CheckFreeVariables(
        ModuleWalkState state,
        AuthoringAttributeDescriptor attribute,
        ExpressionSlotContext context,
        string text,
        int segmentOffsetInValue,
        XPathCompatibility compatibility,
        VariableScope scope,
        IReadOnlyList<StaticVariableName>? useWhenStatics)
    {
        XPathAstNode ast;
        try
        {
            ast = XPathParser.Parse(text, xpath40: compatibility >= XPathCompatibility.XPath40);
        }
        catch (Exception)
        {
            // A parse failure here was already reported by the compile pass; variable analysis has
            // no tree to walk.
            return;
        }

        foreach (var reference in FreeVariableCollector.Collect(ast))
        {
            string? namespaceUri = reference.NamespaceUri;
            var prefix = reference.Prefix ?? string.Empty;
            if (namespaceUri is null)
            {
                if (prefix.Length == 0)
                {
                    namespaceUri = string.Empty;
                }
                else
                {
                    var resolved = ResolvePrefix(context, prefix);
                    if (resolved is null)
                    {
                        _hasInvalid = true;
                        _diagnostics.Add(new XsltValidationDiagnostic(
                            "XPST0081",
                            state.Module.ModuleUri,
                            $"XPST0081: No namespace declaration for prefix '{prefix}' in variable reference.",
                            RangeForValuePosition(state.Map, attribute, segmentOffsetInValue, text.Length, positionsExact: false)));
                        continue;
                    }

                    namespaceUri = resolved;
                }
            }

            var declared = useWhenStatics is not null
                ? ContainsName(useWhenStatics, namespaceUri, reference.LocalName)
                : scope.Contains(namespaceUri, reference.LocalName);
            if (!declared)
            {
                _hasInvalid = true;
                _diagnostics.Add(new XsltValidationDiagnostic(
                    "XPST0008",
                    state.Module.ModuleUri,
                    $"XPST0008: Variable '${(prefix.Length == 0 ? string.Empty : prefix + ":")}{reference.LocalName}' is not declared in scope at this location.",
                    RangeForValuePosition(state.Map, attribute, segmentOffsetInValue, text.Length, positionsExact: false)));
            }
        }
    }

    private static bool ContainsName(IReadOnlyList<StaticVariableName> names, string namespaceUri, string localName)
    {
        foreach (var name in names)
        {
            if (name.NamespaceUri == namespaceUri && name.LocalName == localName)
            {
                return true;
            }
        }

        return false;
    }

    private void ReportCompileDiagnostic(
        ModuleWalkState state,
        AuthoringAttributeDescriptor attribute,
        string text,
        int segmentOffsetInValue,
        Exception ex,
        bool positionsExact)
    {
        _hasInvalid = true;
        SourceRange? range = attribute.ValueRange;
        if (positionsExact && ex is XPathParseException parseException &&
            parseException.Position >= 0 && parseException.Position < text.Length)
        {
            range = RangeForValuePosition(
                state.Map, attribute, segmentOffsetInValue + parseException.Position, 1, positionsExact: true);
        }

        _diagnostics.Add(new XsltValidationDiagnostic(
            ExtractCode(ex),
            state.Module.ModuleUri,
            ex.Message,
            range));
    }

    private XPathCompatibility ResolveCompatibility(ExpressionSlotContext context)
    {
        if (_options.CompatibilityOverride is { } profile)
        {
            return profile;
        }

        return context.EffectiveVersion.StartsWith("4", StringComparison.Ordinal)
            ? XPathCompatibility.XPath40
            : XPathCompatibility.XPath31;
    }

    private static CompileOptions BuildCompileOptions(
        ExpressionSlotContext context, XPathCompatibility compatibility, bool isAvt)
    {
        var namespaces = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var binding in context.InScopeNamespaces)
        {
            if (binding.Prefix.Length != 0)
            {
                namespaces[binding.Prefix] = binding.Uri;
            }
        }

        return new CompileOptions
        {
            Namespaces = namespaces,
            DefaultElementNamespace = context.XpathDefaultNamespace,
            BaseUri = context.BaseUri.AbsoluteUri,
            Compatibility = compatibility,
            BackwardsCompatible = isAvt && context.EffectiveVersion.StartsWith("1", StringComparison.Ordinal),
        };
    }

    private static string? ResolvePrefix(ExpressionSlotContext context, string prefix)
    {
        // Outermost-first ordering: the nearest declaration wins, which is the last match.
        for (var i = context.InScopeNamespaces.Count - 1; i >= 0; i--)
        {
            var binding = context.InScopeNamespaces[i];
            if (binding.Prefix == prefix)
            {
                return binding.Uri;
            }
        }

        return null;
    }

    private SourceRange RangeForValuePosition(
        SourceCoordinateMap map,
        AuthoringAttributeDescriptor attribute,
        int offsetInValue,
        int length,
        bool positionsExact)
    {
        if (!positionsExact)
        {
            return attribute.ValueRange;
        }

        try
        {
            var valueStart = map.CharOffsetFromLineColumn(attribute.ValueRange.StartLine, attribute.ValueRange.StartColumn);
            var valueEnd = map.CharOffsetFromLineColumn(attribute.ValueRange.EndLine, attribute.ValueRange.EndColumn);
            var start = valueStart + offsetInValue;
            if (start >= valueEnd)
            {
                return attribute.ValueRange;
            }

            var end = Math.Min(start + Math.Max(length, 1), valueEnd);
            return map.RangeFromCharOffsets(start, end);
        }
        catch (ArgumentOutOfRangeException)
        {
            return attribute.ValueRange;
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Include/import edges
    // ---------------------------------------------------------------------------------------------

    private void ReportEdgeDiagnostics(AuthoringModuleDescriptor module)
    {
        foreach (var edge in module.Edges)
        {
            var diagnostic = edge.Diagnostic;
            if (diagnostic is null)
            {
                continue;
            }

            switch (diagnostic.Kind)
            {
                case AuthoringFailureKind.ResolverFailure:
                    _hasRefusal = true;
                    _diagnostics.Add(new XsltValidationDiagnostic(
                        RefusalCode,
                        diagnostic.ModuleUri,
                        diagnostic.Message,
                        diagnostic.Range));
                    break;
                case AuthoringFailureKind.Structure:
                case AuthoringFailureKind.UnsupportedEncoding:
                case AuthoringFailureKind.InvalidSourceBytes:
                    _hasSourceFailure = true;
                    _diagnostics.Add(new XsltValidationDiagnostic(
                        SourceFailureCode,
                        diagnostic.ModuleUri,
                        diagnostic.Message,
                        diagnostic.Range));
                    break;
            }
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Small helpers
    // ---------------------------------------------------------------------------------------------

    private static bool IsXslt(AuthoringNodeDescriptor node, string localName) =>
        node.Kind == AuthoringNodeKind.Element &&
        node.ElementName is { } name &&
        name.NamespaceUri == AttributeSlotClassifier.XsltNamespace &&
        name.LocalName == localName;

    private static string ExtractCode(Exception ex)
        => ErrorCodePattern.Match(ex.Message) is { Success: true } match ? match.Value : UncodedEngineCode;

    /// <summary>
    /// Finds the matching closing brace of an AVT expression segment, skipping string literals and
    /// nested braces (same scanning rules as the engine's runtime AVT evaluation).
    /// </summary>
    private static int FindAvtExpressionEnd(string value, int start)
    {
        char inString = '\0';
        var braceDepth = 1;
        for (var i = start; i < value.Length; i++)
        {
            var c = value[i];
            if (inString != '\0')
            {
                if (c == inString)
                {
                    if (i + 1 < value.Length && value[i + 1] == inString)
                    {
                        i++;
                    }
                    else
                    {
                        inString = '\0';
                    }
                }

                continue;
            }

            if (c is '\'' or '"')
            {
                inString = c;
                continue;
            }

            if (c == '{')
            {
                braceDepth++;
                continue;
            }

            if (c == '}')
            {
                braceDepth--;
                if (braceDepth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Whether an AVT segment carries anything worth compiling after whitespace and XPath comments
    /// are stripped.
    /// </summary>
    private static bool HasSubstantiveContent(string expression)
    {
        var text = expression;
        int previousLength;
        do
        {
            previousLength = text.Length;
            var start = text.IndexOf("(:", StringComparison.Ordinal);
            if (start < 0)
            {
                break;
            }

            var depth = 1;
            var i = start + 2;
            for (; i < text.Length && depth > 0; i++)
            {
                if (i + 1 < text.Length && text[i] == '(' && text[i + 1] == ':')
                {
                    depth++;
                    i++;
                }
                else if (i + 1 < text.Length && text[i] == ':' && text[i + 1] == ')')
                {
                    depth--;
                    i++;
                }
            }

            text = depth == 0
                ? text.Remove(start, i - start)
                : text.Remove(start);
        }
        while (text.Length != previousLength);

        return text.Trim().Length != 0;
    }

    private readonly record struct StaticVariableName(string NamespaceUri, string LocalName);

    /// <summary>Persistent (structurally shared) set of declared variable names.</summary>
    private readonly struct VariableScope
    {
        private readonly Entry? _head;

        private VariableScope(Entry? head)
        {
            _head = head;
        }

        public static VariableScope Empty => new(null);

        public VariableScope Add(string namespaceUri, string localName) => new(new Entry(namespaceUri, localName, _head));

        public bool Contains(string namespaceUri, string localName)
        {
            for (var entry = _head; entry is not null; entry = entry.Next)
            {
                if (entry.NamespaceUri == namespaceUri && entry.LocalName == localName)
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class Entry
        {
            public Entry(string namespaceUri, string localName, Entry? next)
            {
                NamespaceUri = namespaceUri;
                LocalName = localName;
                Next = next;
            }

            public string NamespaceUri { get; }

            public string LocalName { get; }

            public Entry? Next { get; }
        }
    }

    private sealed class ModuleWalkState
    {
        public ModuleWalkState(AuthoringModuleDescriptor module, SourceCoordinateMap map)
        {
            Module = module;
            Map = map;
        }

        public AuthoringModuleDescriptor Module { get; }

        public SourceCoordinateMap Map { get; }
    }
}
