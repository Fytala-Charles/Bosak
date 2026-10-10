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
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-125 review findings F1-F4: structural/static XSLT pass (unknown instructions,        |
//                      |                  |       |                | XTSE0010/0090/0260/0805 cluster), use-when exclusion decided before slot checks          |
//                      |                  |       |                | (literal subset; non-literal degrades to UnsupportedCoverage), declaration-order         |
//                      |                  |       |                | parameter scoping, pattern slots compiled with the slot's namespace bindings             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Parser;
using Bosak.XPath.Parser.Ast;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Patterns;
using XsltStylesheet = Bosak.Xslt.Stylesheet.Stylesheet;

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

                // XSLT 3.0 §3.13: a declaration excluded by a supported-literal use-when is
                // removed from the stylesheet, so it declares nothing; a non-literal use-when
                // cannot be evaluated here, which makes every check that depends on the
                // declaration's presence partial.
                if (FindUseWhenAttribute(child) is { } useWhen)
                {
                    if (ClassifyUseWhen(useWhen) == UseWhenExclusion.Excluded)
                    {
                        continue;
                    }

                    if (ClassifyUseWhen(useWhen) == UseWhenExclusion.Undetermined)
                    {
                        ReportUseWhenEvaluationGap(module.ModuleUri);
                    }
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

        // XSLT 3.0 §3.13 exclusion is decided BEFORE anything else about the element is
        // analyzed: when the supported literal subset excludes the element, its own attribute
        // slots and its whole subtree are not part of the stylesheet and must not be checked
        // (an excluded xsl:sequence may carry an unparseable select). The use-when expression
        // itself is always checked first, so a malformed exclusion still fails.
        var useWhenAttribute = FindUseWhenAttribute(node);
        if (useWhenAttribute is not null)
        {
            var useWhenContext = useWhenAttribute.SlotContext;
            var useWhenCompiled = true;
            if (useWhenContext is not null)
            {
                useWhenCompiled = CheckExpressionSlot(state, useWhenAttribute, useWhenContext, scope, isUseWhen: true);
            }

            if (!useWhenCompiled || ClassifyUseWhen(useWhenAttribute) == UseWhenExclusion.Excluded)
            {
                // Unparseable exclusion (already reported) or literal exclusion: the element
                // cannot be reasoned about / is removed from the stylesheet.
                return;
            }

            if (ClassifyUseWhen(useWhenAttribute) == UseWhenExclusion.Undetermined)
            {
                // The exclusion effect cannot be evaluated by the supported subset: the element
                // may or may not exist, so its checks are partial. That must never surface as a
                // complete pass: record the deferred evaluation as a coverage gap.
                ReportUseWhenEvaluationGap(state.Module.ModuleUri);
            }
        }

        ReportCoverageGapIfDeferred(state, elementName);
        CheckElementStructure(state, node);

        // Slots on this element's start tag (the use-when slot, when present, was checked above).
        foreach (var attribute in node.Attributes)
        {
            if (ReferenceEquals(attribute, useWhenAttribute))
            {
                continue;
            }

            var context = attribute.SlotContext;
            if (context is null)
            {
                continue;
            }

            switch (attribute.SlotKind)
            {
                case AuthoringAttributeSlotKind.Expression:
                    CheckExpressionSlot(state, attribute, context, scope, isUseWhen: false);
                    break;
                case AuthoringAttributeSlotKind.Pattern:
                    CheckPatternSlot(state, attribute, context);
                    break;
                case AuthoringAttributeSlotKind.Avt:
                    CheckAvtSlot(state, attribute, context, scope);
                    break;
            }
        }

        // Child scope: a function body sees only globals plus its own parameters (outer locals
        // are not in scope inside xsl:function). Template and iterate parameters are NOT
        // predeclared here: a parameter default may reference only parameters declared before
        // it (declaration-order scope), so each parameter enters scope through the sibling
        // walk below, immediately after its own default has been checked.
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
        }

        // Sibling scoping: xsl:variable/xsl:param declarations are in scope for FOLLOWING siblings
        // (and their subtrees) only. A declaration excluded by a supported-literal use-when is
        // removed from the stylesheet (XSLT 3.0 §3.13), so it enters no scope.
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
                if (!IsExcludedByLiteralUseWhen(child) && TryReadQNameName(child, out var ns, out var local))
                {
                    siblingScope = siblingScope.Add(ns, local);
                }
            }
        }
    }

    /// <summary>Whether the element carries a <c>use-when</c> value in the supported literal subset that excludes it.</summary>
    private static bool IsExcludedByLiteralUseWhen(AuthoringNodeDescriptor node)
    {
        return FindUseWhenAttribute(node) is { } useWhen &&
            ClassifyUseWhen(useWhen) == UseWhenExclusion.Excluded;
    }

    /// <summary>
    /// Finds the element's <c>use-when</c> attribute when it is classified as an expression slot.
    /// </summary>
    private static AuthoringAttributeDescriptor? FindUseWhenAttribute(AuthoringNodeDescriptor node)
    {
        foreach (var attribute in node.Attributes)
        {
            if (attribute.LocalName == "use-when" && attribute.SlotKind == AuthoringAttributeSlotKind.Expression)
            {
                return attribute;
            }
        }

        return null;
    }

    /// <summary>The supported <c>use-when</c> evaluation subset and its three outcomes.</summary>
    private enum UseWhenExclusion
    {
        /// <summary>A supported literal form that includes the element.</summary>
        Included,

        /// <summary>A supported literal form that excludes the element (and its subtree).</summary>
        Excluded,

        /// <summary>A form outside the supported literal subset; exclusion is undetermined.</summary>
        Undetermined,
    }

    /// <summary>
    /// Classifies a <c>use-when</c> value against the supported literal subset. Only the
    /// literals <c>true</c>, <c>true()</c>, <c>false</c> and <c>false()</c> are evaluated;
    /// anything else is undetermined rather than guessed, because evaluating arbitrary static
    /// expressions would require uncontrolled static-expression IO.
    /// </summary>
    private static UseWhenExclusion ClassifyUseWhen(AuthoringAttributeDescriptor attribute)
    {
        var value = attribute.ExpandedValue.Trim();
        return value switch
        {
            "false" or "false()" => UseWhenExclusion.Excluded,
            "true" or "true()" => UseWhenExclusion.Included,
            _ => UseWhenExclusion.Undetermined,
        };
    }

    private void ReportUseWhenEvaluationGap(Uri moduleUri)
    {
        const string construct = "use-when";
        var key = moduleUri.AbsoluteUri + "|" + construct;
        if (!_recordedGaps.Add(key))
        {
            return;
        }

        _gaps.Add(new XsltValidationCoverageGap(
            construct,
            moduleUri,
            "Non-literal use-when attributes are not statically evaluated by this validation " +
            "build (evaluating them would require uncontrolled static-expression IO). Whether the " +
            "affected elements and declarations are excluded from the stylesheet is undetermined, " +
            "so the checks on them are partial rather than a complete pass."));
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
    // Structural / static XSLT checks (review finding F1)
    //
    // The walk deliberately disables ordinary stylesheet compilation, which historically also
    // disabled the compiler's static structural validation: an unknown xsl:instruction inside a
    // template reported XTSE0010 by the compiler validated as "Valid". This pass re-checks the
    // safe, IO-free subset of the compiler's ValidateInstructionTree rules over the authoring
    // descriptors: unknown XSLT instructions, top-level placement, must-be-empty content,
    // misplaced/required attributes, static-declaration placement and XSLT-namespaced
    // attributes. It performs no expression evaluation beyond the supported use-when literal
    // subset and no resource acquisition. Rules are mirrored from Stylesheet.cs; element sets
    // are shared with the compiler so the two cannot drift.
    // ---------------------------------------------------------------------------------------------

    private void CheckElementStructure(ModuleWalkState state, AuthoringNodeDescriptor node)
    {
        var elementName = node.ElementName;
        if (elementName is null || !ShouldValidateStructure(node))
        {
            return;
        }

        var isXsltElement = elementName.NamespaceUri == AttributeSlotClassifier.XsltNamespace;
        var localName = elementName.LocalName;

        // xsl:note elements are discarded at an early stage of processing (XSLT 4.0 §3.11.2),
        // without validation of their attributes or content.
        if (isXsltElement && localName == "note")
        {
            return;
        }

        // XTSE0805 / XTSE0090: validate attributes in the XSLT namespace.
        foreach (var attribute in node.Attributes)
        {
            if (attribute.NamespaceUri != AttributeSlotClassifier.XsltNamespace)
            {
                continue;
            }

            if (isXsltElement)
            {
                // XSLT-namespaced attributes are not permitted on XSLT elements.
                ReportStructureDiagnostic(state, node, "XTSE0090",
                    $"XTSE0090: Attributes in the XSLT namespace are not permitted on xsl:{localName}.");
            }
            else
            {
                // On literal result elements only the defined XSLT attributes are allowed.
                var allowed = attribute.LocalName is "use-when" or "expand-text" or "type" or "validation"
                    or "default-mode" or "default-collation" or "default-validation"
                    or "exclude-result-prefixes" or "extension-element-prefixes"
                    or "version" or "xpath-default-namespace" or "use-attribute-sets"
                    or "inherit-namespaces";
                if (!allowed && !IsForwardsCompatibleElement(node))
                {
                    ReportStructureDiagnostic(state, node, "XTSE0805",
                        $"XTSE0805: The attribute xsl:{attribute.LocalName} is not permitted on a literal result element.");
                }
            }
        }

        if (!isXsltElement)
        {
            return;
        }

        // XTSE0260: XSLT elements that must be empty must not contain text nodes
        // or element children; comments and processing instructions are allowed.
        if (XsltStylesheet.EmptyXsltElementNames.Contains(localName))
        {
            foreach (var child in node.Children)
            {
                if (child.Kind is AuthoringNodeKind.Text or AuthoringNodeKind.Element)
                {
                    ReportStructureDiagnostic(state, node, "XTSE0260",
                        $"XTSE0260: xsl:{localName} must be empty; it must not contain text or element children.");
                    break;
                }
            }
        }

        // XTSE0090: static variables and parameters must be declared at the top level.
        if (localName is "param" or "variable" && IsStaticDeclaration(node) && !IsTopLevelChild(node))
        {
            ReportStructureDiagnostic(state, node, "XTSE0090",
                "XTSE0090: A static variable or parameter must be declared at the top level of the stylesheet.");
        }

        var parent = (node.BackingObject as XElement)?.Parent;
        if (parent is not null)
        {
            var isTopLevel = parent.Name.NamespaceName == AttributeSlotClassifier.XsltNamespace &&
                parent.Name.LocalName is "stylesheet" or "transform" or "package";

            // Unknown XSLT elements are normally a static error. They are ignored when the
            // stylesheet is in forwards-compatible mode, or when they appear at the top level
            // of an XSLT 3.0 stylesheet (where unrecognized elements are tolerated as vendor
            // extensions). In earlier XSLT versions an unrecognized top-level element is an error.
            if (!XsltStylesheet.KnownXsltElementNames.Contains(localName))
            {
                if (!IsForwardsCompatibleElement(node) &&
                    !(isTopLevel && GetEffectiveVersion(node) >= 3.0))
                {
                    ReportStructureDiagnostic(state, node, "XTSE0010",
                        $"XTSE0010: Unknown XSLT element xsl:{localName}.");
                }
            }
            else if (isTopLevel)
            {
                if (!XsltStylesheet.AllowedTopLevelDeclarations.Contains(localName) && !IsForwardsCompatibleElement(node))
                {
                    ReportStructureDiagnostic(state, node, "XTSE0010",
                        $"XTSE0010: xsl:{localName} is not permitted at the top level.");
                }
            }
            else if (XsltStylesheet.TopLevelOnlyDeclarations.Contains(localName))
            {
                var insideUsePackage = parent.Name.NamespaceName == AttributeSlotClassifier.XsltNamespace &&
                    parent.Name.LocalName == "use-package";
                var insideOverride = parent.Name.NamespaceName == AttributeSlotClassifier.XsltNamespace &&
                    parent.Name.LocalName == "override";
                if (!insideUsePackage && !insideOverride)
                {
                    ReportStructureDiagnostic(state, node, "XTSE0010",
                        $"XTSE0010: xsl:{localName} must appear at the top level.");
                }
            }

            // xsl:use-package requires a name attribute (package resolution itself is a
            // declared coverage gap, but the missing name is a plain structural error).
            if (localName == "use-package" && !HasAttribute(node, "name") && !HasAttribute(node, "_name"))
            {
                ReportStructureDiagnostic(state, node, "XTSE0010",
                    "XTSE0010: xsl:use-package requires a name attribute.");
            }

            // xsl:if requires a test attribute.
            if (localName == "if" && !HasAttribute(node, "test") && !HasAttribute(node, "_test"))
            {
                ReportStructureDiagnostic(state, node, "XTSE0010", "XTSE0010: xsl:if requires a test attribute.");
            }

            // xsl:on-completion must be a direct child of xsl:iterate.
            if (localName == "on-completion" &&
                (parent.Name.NamespaceName != AttributeSlotClassifier.XsltNamespace || parent.Name.LocalName != "iterate"))
            {
                ReportStructureDiagnostic(state, node, "XTSE0010",
                    "XTSE0010: xsl:on-completion must be a child of xsl:iterate.");
            }
        }
    }

    /// <summary>
    /// Mirrors the compiler's ShouldValidateElement: returns false when the element is inside an
    /// unknown XSLT element that is in forwards-compatible mode, unless the element is a
    /// descendant of an <c>xsl:fallback</c> child of that unknown element. Elements inside a
    /// discarded <c>xsl:note</c> subtree are not validated either (XSLT 4.0 §3.11.2). Descendants
    /// of excluded subtrees never reach this check.
    /// </summary>
    private static bool ShouldValidateStructure(AuthoringNodeDescriptor node)
    {
        if (node.BackingObject is not XElement element)
        {
            return true;
        }

        var current = element.Parent;
        while (current != null)
        {
            if (current.Name.NamespaceName == AttributeSlotClassifier.XsltNamespace &&
                current.Name.LocalName == "note")
            {
                return false;
            }

            if (current.Name.NamespaceName == AttributeSlotClassifier.XsltNamespace &&
                !XsltStylesheet.KnownXsltElementNames.Contains(current.Name.LocalName) &&
                GetEffectiveVersion(current) > 3.0)
            {
                // Walk up from the element to the unknown ancestor to find the immediate child
                // of the unknown ancestor on that path.
                var childOnPath = element;
                while (childOnPath.Parent != null && childOnPath.Parent != current)
                {
                    childOnPath = childOnPath.Parent;
                }

                return childOnPath.Name.NamespaceName == AttributeSlotClassifier.XsltNamespace &&
                    childOnPath.Name.LocalName == "fallback";
            }

            current = current.Parent;
        }

        return true;
    }

    private static bool IsTopLevelChild(AuthoringNodeDescriptor node)
    {
        var parent = (node.BackingObject as XElement)?.Parent;
        return parent != null &&
            parent.Name.NamespaceName == AttributeSlotClassifier.XsltNamespace &&
            parent.Name.LocalName is "transform" or "stylesheet" or "package";
    }

    private static bool HasAttribute(AuthoringNodeDescriptor node, string localName)
    {
        foreach (var attribute in node.Attributes)
        {
            if (attribute.LocalName == localName && attribute.NamespaceUri.Length == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The effective XSLT version of an element, mirroring <c>Stylesheet.GetEffectiveVersion</c>:
    /// the nearest <c>version</c> (XSLT elements) or <c>xsl:version</c> (literal result elements)
    /// attribute on the element or an ancestor, falling back to 3.0.
    /// </summary>
    private static double GetEffectiveVersion(AuthoringNodeDescriptor node)
        => node.BackingObject is XElement element ? GetEffectiveVersion(element) : 3.0;

    private static double GetEffectiveVersion(XElement element)
    {
        foreach (var ancestor in element.AncestorsAndSelf())
        {
            XAttribute? versionAttr = null;
            if (ancestor.Name.NamespaceName == AttributeSlotClassifier.XsltNamespace)
            {
                versionAttr = ancestor.Attribute("version");
            }

            versionAttr ??= ancestor.Attribute(XNamespace.Get(AttributeSlotClassifier.XsltNamespace) + "version");
            if (versionAttr != null)
            {
                if (double.TryParse(versionAttr.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                {
                    return v;
                }

                break;
            }
        }

        return 3.0;
    }

    /// <summary>
    /// Whether the element is in XSLT forwards-compatible mode (effective version greater than
    /// the supported version 3.0), mirroring <c>Stylesheet.IsForwardsCompatibleElement</c>.
    /// </summary>
    private static bool IsForwardsCompatibleElement(AuthoringNodeDescriptor node) => GetEffectiveVersion(node) > 3.0;

    private void ReportStructureDiagnostic(
        ModuleWalkState state, AuthoringNodeDescriptor node, string code, string message)
    {
        _hasInvalid = true;
        _diagnostics.Add(new XsltValidationDiagnostic(code, state.Module.ModuleUri, message, node.Range));
    }

    // ---------------------------------------------------------------------------------------------
    // Slot checks
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Compiles an expression slot in its static context. Returns whether compilation and free
    /// variable analysis produced no diagnostic.
    /// </summary>
    private bool CheckExpressionSlot(
        ModuleWalkState state,
        AuthoringAttributeDescriptor attribute,
        ExpressionSlotContext context,
        VariableScope scope,
        bool isUseWhen)
    {
        var text = attribute.ExpandedValue;
        var positionsExact = attribute.RawLiteral == text;
        var compatibility = ResolveCompatibility(context);
        var ok = true;

        try
        {
            XPath31Expression.Compile(text, BuildCompileOptions(context, compatibility, isAvt: false));
        }
        catch (Exception ex)
        {
            ok = false;
            ReportCompileDiagnostic(state, attribute, text, segmentOffsetInValue: 0, ex, positionsExact);
        }

        if (!CheckFreeVariables(
            state, attribute, context, text, segmentOffsetInValue: 0, compatibility,
            isUseWhen ? VariableScope.Empty : scope,
            isUseWhen ? _staticGlobals : null))
        {
            ok = false;
        }

        return ok;
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
            // Compile the pattern with the slot's real in-scope namespace bindings: pattern QNames
            // and the prefix bindings of pattern predicates must resolve in the module-local
            // static context, so an undeclared prefix is rejected (XPST0081) instead of being
            // silently read as a no-namespace name.
            _ = new PatternCompiler().Compile(
                text, context.XpathDefaultNamespace, namespaces: BuildNamespaceMap(context));
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

    /// <summary>
    /// Matches the free variable references of one already-compiled expression against the
    /// declarations in scope. Returns whether every reference resolved.
    /// </summary>
    private bool CheckFreeVariables(
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
            return true;
        }

        var ok = true;
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
                        ok = false;
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
                ok = false;
                _hasInvalid = true;
                _diagnostics.Add(new XsltValidationDiagnostic(
                    "XPST0008",
                    state.Module.ModuleUri,
                    $"XPST0008: Variable '${(prefix.Length == 0 ? string.Empty : prefix + ":")}{reference.LocalName}' is not declared in scope at this location.",
                    RangeForValuePosition(state.Map, attribute, segmentOffsetInValue, text.Length, positionsExact: false)));
            }
        }

        return ok;
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
        return new CompileOptions
        {
            Namespaces = BuildNamespaceMap(context),
            DefaultElementNamespace = context.XpathDefaultNamespace,
            BaseUri = context.BaseUri.AbsoluteUri,
            Compatibility = compatibility,
            BackwardsCompatible = isAvt && context.EffectiveVersion.StartsWith("1", StringComparison.Ordinal),
        };
    }

    /// <summary>
    /// The slot's in-scope prefix bindings as a map, nearest declaration winning (the binding
    /// list is outermost first, so later entries overwrite earlier ones).
    /// </summary>
    private static Dictionary<string, string> BuildNamespaceMap(ExpressionSlotContext context)
    {
        var namespaces = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var binding in context.InScopeNamespaces)
        {
            if (binding.Prefix.Length != 0)
            {
                namespaces[binding.Prefix] = binding.Uri;
            }
        }

        return namespaces;
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
