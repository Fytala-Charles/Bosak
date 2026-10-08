// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Represents a compiled XPath 3.1 expression that can be evaluated repeatedly against different inp...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-06-2026     | Added DefiningElementDefaultNamespace for element-available default namespace            |
//                      | Charles Korthout | 0.3   | 26-06-2026     | Compile-time namespace resolution and static errors for removed functions                |
//                      | Charles Korthout | 0.4   | 27-06-2026     | Preserve explicit braced-URI namespace URIs in function calls and named function refs    |
//                      | Charles Korthout | 0.5   | 21-07-2026     | Empty expression reports XPST0003 via XPathParseException instead of ArgumentException         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 25-07-2026     | Xml11LineEndings option threaded to the parser                                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 27-07-2026     | Namespace resolution traversal for multi-clause TryCatchNode |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 27-07-2026     | Namespace resolution traversal for StringConstructorNode |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 07-09-2026     | Static name-test validation (XPST0081/XPST0008) against CompileOptions.Namespaces        |
//                      | Charles Korthout | 0.10  | 21-09-2026     | API freeze stage A: ParseException renamed to XPathParseException                      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.11  | 24-09-2026     | REQ-104: CompileOptions.SchemaSet threaded to the parser (schemaAware) and the         |
//                      |                  |       |                | static name-test validator (declaration-aware XPST0008)                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.12  | 08-10-2026     | REQ-118 4.0-S0: static XPST0017 for XPath 4.0-only functions in 3.1 mode;               |
//                      |                  |       |                | compiled compatibility stamped onto the EvaluationContext before Populate               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.13  | 08-10-2026     | REQ-118 4.0-S3a: Compatibility >= XPath40 opts the parser into the 4.0 grammar          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.14  | 08-10-2026     | REQ-118 4.0-S3b: keyword-argument expansion (XPST0017 rules, F&O defaults) in            |
//                      |                  |       |                | ResolveFunctionCall; StringTemplateNode namespace traversal                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Concurrent;
using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Compiler.Optimizer;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser;
using Bosak.XPath.Parser.Ast;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;

namespace Bosak.XPath.Api;

/// <summary>
/// Represents a compiled XPath 3.1 expression that can be evaluated repeatedly
/// against different input documents with high performance.
/// </summary>
public sealed class XPath31Expression
{
    private readonly IrModule _module;
    private readonly IReadOnlyDictionary<string, string>? _namespaces;
    private readonly string? _defaultElementNamespace;
    private readonly string? _definingElementDefaultNamespace;
    private readonly string? _baseUri;
    private readonly XPathCompatibility _compatibility;

    private XPath31Expression(IrModule module, IReadOnlyDictionary<string, string>? namespaces = null, string? defaultElementNamespace = null, string? definingElementDefaultNamespace = null, string? baseUri = null, XPathCompatibility compatibility = XPathCompatibility.XPath31)
    {
        _module = module;
        _namespaces = namespaces;
        _defaultElementNamespace = defaultElementNamespace;
        _definingElementDefaultNamespace = definingElementDefaultNamespace;
        _baseUri = baseUri;
        _compatibility = compatibility;
    }

    /// <summary>
    /// Parses and compiles an XPath 3.1 expression string with default options.
    /// </summary>
    public static XPath31Expression Compile(string expression)
        => Compile(expression, CompileOptions.Default);

    /// <summary>
    /// Parses and compiles an XPath expression with the specified options.
    /// </summary>
    public static XPath31Expression Compile(string expression, CompileOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(expression))
            throw new XPathParseException("Empty expression is not a valid XPath expression", 0);

        // 1. Lex + Parse -> AST
        var ast = XPathParser.Parse(expression, xml11LineEndings: options.Xml11LineEndings, schemaAware: options.SchemaSet is not null, xpath40: options.Compatibility >= XPathCompatibility.XPath40);

        // 2. Resolve function-call namespaces using the supplied static context and
        // report static errors for functions that have been removed from the spec.
        ast = ResolveFunctionNamespaces(ast, options);

        // 2b. Static name-test validation against the in-scope namespaces (XPST0081 for
        // undeclared prefixes) and schema-aware kind tests (XPST0008): without a schema
        // set there is no schema awareness; with one (REQ-104) the kind-test name argument
        // must resolve to a global declaration in the set. Skipped when no namespace
        // context is supplied: XSLT patterns and runtime-namespace compilations keep the
        // runtime NamespaceTest resolution (the runtime kind tests raise XPST0008 for
        // undeclared schema names).
        if (options.Namespaces is not null)
            Compiler.StaticNameTestValidator.Validate(ast, prefix =>
                options.Namespaces.TryGetValue(prefix, out var nsUri) ? nsUri : null,
                options.SchemaSet, options.DefaultElementNamespace);

        // 3. Optimize AST
        var optimizer = new XPathOptimizer();
        var optimized = optimizer.Optimize(ast, options.BackwardsCompatible);

        // 4. Lower to IR
        var lowerer = new IrLowerer();
        var module = lowerer.Lower(optimized);

        return new XPath31Expression(module, options.Namespaces, options.DefaultElementNamespace, options.DefiningElementDefaultNamespace, options.BaseUri, options.Compatibility);
    }

    private const string DefaultFunctionNamespace = "http://www.w3.org/2005/xpath-functions";
    private const string OldMapNamespace = "http://www.w3.org/2011/xpath-functions/map";

    private static readonly HashSet<(string NamespaceUri, string LocalName)> RemovedFunctions = new()
    {
        ("http://www.w3.org/2005/xpath-functions/map", "new"),
        ("http://www.w3.org/2005/xpath-functions/map", "for-each-entry"),
        ("http://www.w3.org/2005/xpath-functions/map", "collation"),
        ("http://www.w3.org/2005/xpath-functions", "deep-equal2"),
    };

    private static string? ResolvePrefix(string? prefix, CompileOptions options)
    {
        if (string.IsNullOrEmpty(prefix))
            return DefaultFunctionNamespace;

        if (options.Namespaces != null && options.Namespaces.TryGetValue(prefix, out var nsUri))
            return nsUri;

        return null;
    }

    private static void ThrowIfRemovedFunction(string? nsUri, string localName)
    {
        if (nsUri == OldMapNamespace)
            throw new InvalidOperationException($"XPST0017: Function in obsolete map namespace '{nsUri}' is not available");

        if (!string.IsNullOrEmpty(nsUri) && RemovedFunctions.Contains((nsUri, localName)))
            throw new InvalidOperationException($"XPST0017: Function {{{nsUri}}}{localName} has been removed");
    }

    // REQ-118 version gate (4.0-S0): XPath 4.0-only functions are rejected at compile
    // time in 3.1 mode with XPST0017, mirroring the removed-function check above. The
    // RemovedFunctions behavior above stays byte-identical; this is a separate check.
    // When the caller supplied no namespace context, prefixed names may not have
    // resolved to a URI; the predefined fn/map/array/math prefixes then fall back to
    // their canonical URIs (matching EvaluationContext's predefined bindings) so the
    // gate still applies to them.
    private static void ThrowIfXPath40OnlyFunction(string? nsUri, string localName, string? prefix, CompileOptions options)
    {
        if (options.Compatibility >= XPathCompatibility.XPath40)
            return;
        if (string.IsNullOrEmpty(nsUri))
        {
            nsUri = prefix switch
            {
                "fn" => DefaultFunctionNamespace,
                "map" => "http://www.w3.org/2005/xpath-functions/map",
                "array" => "http://www.w3.org/2005/xpath-functions/array",
                "math" => "http://www.w3.org/2005/xpath-functions/math",
                _ => null,
            };
        }
        if (!string.IsNullOrEmpty(nsUri)
            && FunctionLibrary.XPath40OnlyFunctionNames.Contains((nsUri, localName)))
            throw new InvalidOperationException(
                $"XPST0017: Function {{{nsUri}}}{localName} is defined in XPath 4.0 and is not available when targeting XPath {(int)options.Compatibility}; set CompileOptions.Compatibility to XPathCompatibility.XPath40.");
    }

    private static XPathAstNode ResolveFunctionNamespaces(XPathAstNode node, CompileOptions options)
    {
        return node switch
        {
            FunctionCallNode fc => ResolveFunctionCall(fc, options),
            NamedFunctionRefNode nf => ResolveNamedFunctionRef(nf, options),
            ParenthesizedExprNode p => p with { Expression = ResolveFunctionNamespaces(p.Expression, options) },
            PredicateNode pred => pred with { Expression = ResolveFunctionNamespaces(pred.Expression, options) },
            StepNode step => step with { Predicates = step.Predicates.Select(p => ResolveFunctionNamespaces(p, options)).ToList() },
            PathExprNode path => path with { Steps = path.Steps.Select(s => ResolveFunctionNamespaces(s, options)).ToList() },
            SequenceExpressionNode seq => seq with { Expressions = seq.Expressions.Select(e => ResolveFunctionNamespaces(e, options)).ToList() },
            RangeExpressionNode range => range with { From = ResolveFunctionNamespaces(range.From, options), To = ResolveFunctionNamespaces(range.To, options) },
            IfExpressionNode ife => ife with { Condition = ResolveFunctionNamespaces(ife.Condition, options), ThenBranch = ResolveFunctionNamespaces(ife.ThenBranch, options), ElseBranch = ResolveFunctionNamespaces(ife.ElseBranch, options) },
            ForExpressionNode fe => fe with { Bindings = fe.Bindings.Select(b => b with { Expression = ResolveFunctionNamespaces(b.Expression, options) }).ToList(), ReturnExpression = ResolveFunctionNamespaces(fe.ReturnExpression, options) },
            LetExpressionNode le => le with { Bindings = le.Bindings.Select(b => b with { Expression = ResolveFunctionNamespaces(b.Expression, options) }).ToList(), Body = ResolveFunctionNamespaces(le.Body, options) },
            QuantifiedExpressionNode qe => qe with { Bindings = qe.Bindings.Select(b => b with { Expression = ResolveFunctionNamespaces(b.Expression, options) }).ToList(), SatisfiesExpression = ResolveFunctionNamespaces(qe.SatisfiesExpression, options) },
            BinaryExpressionNode bin => bin with { Left = ResolveFunctionNamespaces(bin.Left, options), Right = ResolveFunctionNamespaces(bin.Right, options) },
            UnaryExpressionNode un => un with { Operand = ResolveFunctionNamespaces(un.Operand, options) },
            CastNode cast => cast with { Expression = ResolveFunctionNamespaces(cast.Expression, options) },
            CastableNode castable => castable with { Expression = ResolveFunctionNamespaces(castable.Expression, options) },
            InstanceOfNode io => io with { Expression = ResolveFunctionNamespaces(io.Expression, options) },
            TreatNode treat => treat with { Expression = ResolveFunctionNamespaces(treat.Expression, options) },
            ArrowExprNode arrow => arrow with { Source = ResolveFunctionNamespaces(arrow.Source, options), Target = ResolveArrowTarget(arrow.Target, options) },
            TryCatchNode tc => tc with
            {
                TryExpression = ResolveFunctionNamespaces(tc.TryExpression, options),
                Clauses = tc.Clauses.Select(c => c with { Expression = ResolveFunctionNamespaces(c.Expression, options) }).ToList()
            },
            StringConstructorNode sc => sc with { Parts = sc.Parts.Select(p => ResolveFunctionNamespaces(p, options)).ToList() },
            StringTemplateNode st => st with { Parts = st.Parts.Select(p => ResolveFunctionNamespaces(p, options)).ToList() },
            LookupNode lookup => lookup with { Expression = ResolveFunctionNamespaces(lookup.Expression, options), Key = ResolveFunctionNamespaces(lookup.Key, options) },
            LookupWildcardNode lw => lw with { Expression = ResolveFunctionNamespaces(lw.Expression, options) },
            InlineFunctionNode inf => inf with { Body = ResolveFunctionNamespaces(inf.Body, options) },
            MapConstructorNode mc => mc with { Entries = mc.Entries.Select(e => e with { Key = ResolveFunctionNamespaces(e.Key, options), Value = ResolveFunctionNamespaces(e.Value, options) }).ToList() },
            ArrayConstructorNode ac => ac with { Items = ac.Items.Select(i => ResolveFunctionNamespaces(i, options)).ToList() },
            PostfixPredicateNode pp => pp with { Expression = ResolveFunctionNamespaces(pp.Expression, options), Predicate = ResolveFunctionNamespaces(pp.Predicate, options) },
            DynamicFunctionCallNode dfc => dfc with { Function = ResolveFunctionNamespaces(dfc.Function, options), Arguments = dfc.Arguments.Select(a => ResolveFunctionNamespaces(a, options)).ToList() },
            _ => node
        };
    }

    // An arrow target that is a static function call receives the arrow source as its
    // first positional argument at lowering time, so keyword expansion must treat the
    // first declared parameter as already filled and must not include it in the
    // rewritten argument list (the lowerer prepends the source).
    private static XPathAstNode ResolveArrowTarget(XPathAstNode target, CompileOptions options)
        => target is FunctionCallNode fc
            ? ResolveFunctionCall(fc, options, arrowInsertsFirst: true)
            : ResolveFunctionNamespaces(target, options);

    private static FunctionCallNode ResolveFunctionCall(FunctionCallNode node, CompileOptions options, bool arrowInsertsFirst = false)
    {
        var nsUri = string.IsNullOrEmpty(node.NamespaceUri)
            ? ResolvePrefix(node.Prefix, options)
            : node.NamespaceUri;
        var resolvedKeywords = node.KeywordArguments?
            .Select(k => k with { Value = ResolveFunctionNamespaces(k.Value, options) })
            .ToList();
        var resolved = node with
        {
            Arguments = node.Arguments.Select(a => ResolveFunctionNamespaces(a, options)).ToList(),
            KeywordArguments = resolvedKeywords,
            NamespaceUri = nsUri
        };
        ThrowIfRemovedFunction(resolved.NamespaceUri, resolved.LocalName);
        ThrowIfXPath40OnlyFunction(resolved.NamespaceUri, resolved.LocalName, resolved.Prefix, options);
        if (resolved.KeywordArguments is { Count: > 0 })
            resolved = ExpandKeywordArguments(resolved, options, arrowInsertsFirst);
        return resolved;
    }

    // XPath 4.0 §4.6.1 keyword-argument expansion: positional arguments fill the first
    // parameters; each keyword (an EQName matched with the no-namespace rule: an
    // unprefixed keyword has no namespace) must match a distinct, not yet filled
    // parameter of the function's declared signature; unfilled optional parameters take
    // their declared F&O default. Every mismatch is a static error XPST0017. The call is
    // rewritten to a fully positional call of the fully-populated arity, which dispatches
    // to the corresponding standard registration.
    private static FunctionCallNode ExpandKeywordArguments(FunctionCallNode node, CompileOptions options, bool arrowInsertsFirst)
    {
        if (node.Arguments.Any(a => a is ArgumentPlaceholderNode))
            throw new InvalidOperationException("XPST0017: Argument placeholders cannot be combined with keyword arguments.");

        var nsUri = node.NamespaceUri;
        if (string.IsNullOrEmpty(nsUri))
        {
            // Unresolved prefixes fall back to the canonical predefined bindings (same
            // treatment as ThrowIfXPath40OnlyFunction).
            nsUri = node.Prefix switch
            {
                "fn" => DefaultFunctionNamespace,
                "map" => "http://www.w3.org/2005/xpath-functions/map",
                "array" => "http://www.w3.org/2005/xpath-functions/array",
                _ => null,
            };
        }
        if (string.IsNullOrEmpty(nsUri) ||
            !FunctionLibrary.TryGetKeywordSignature(nsUri, node.LocalName, out var sig))
            throw new InvalidOperationException(
                $"XPST0017: Function {node.LocalName} does not declare keyword parameters and cannot be called with keyword arguments.");

        var names = sig.ParameterNames;
        var defaults = sig.ParameterDefaults;
        // With 'E => f(...)', the first parameter is filled by the arrow source at
        // lowering time: it counts as positionally filled and stays out of the
        // rewritten argument list.
        int firstFillable = arrowInsertsFirst ? 1 : 0;
        if (arrowInsertsFirst && names.Count == 0)
            throw new InvalidOperationException($"XPST0017: Function {node.LocalName} does not declare keyword parameters and cannot be called with keyword arguments.");
        if (node.Arguments.Count > names.Count - firstFillable)
            throw new InvalidOperationException(
                $"XPST0017: Too many positional arguments in call of {node.LocalName}: expected at most {names.Count - firstFillable}.");

        var filled = new XPathAstNode[names.Count];
        var used = new bool[names.Count];
        for (int i = 0; i < firstFillable; i++)
            used[i] = true;
        for (int i = 0; i < node.Arguments.Count; i++)
        {
            filled[firstFillable + i] = node.Arguments[i];
            used[firstFillable + i] = true;
        }

        foreach (var kw in node.KeywordArguments!)
        {
            var (kwPrefix, kwLocal, kwNs) = SplitKeywordName(kw.Name);
            // No-namespace rule: an unprefixed keyword is in no namespace (it is NOT in
            // the default function namespace).
            string kwNamespace = kwNs ?? (string.IsNullOrEmpty(kwPrefix)
                ? string.Empty
                : (ResolvePrefix(kwPrefix, options) ?? string.Empty));
            int match = -1;
            for (int i = 0; i < names.Count; i++)
            {
                if (!used[i] && kwNamespace.Length == 0 && names[i] == kwLocal)
                {
                    match = i;
                    break;
                }
            }
            if (match < 0)
            {
                bool known = kwNamespace.Length == 0 && names.Any(n => n == kwLocal);
                throw new InvalidOperationException(known
                    ? $"XPST0017: Keyword argument '{kw.Name}' in call of {node.LocalName} matches a parameter that is already filled."
                    : $"XPST0017: Unknown keyword argument '{kw.Name}' in call of {node.LocalName}.");
            }
            filled[match] = kw.Value;
            used[match] = true;
        }

        for (int i = 0; i < names.Count; i++)
        {
            if (used[i])
                continue;
            if (defaults[i] is { } snippet)
                filled[i] = ParseDefaultExpression(snippet);
            else
                throw new InvalidOperationException(
                    $"XPST0017: Required parameter ${names[i]} is not supplied in call of {node.LocalName}.");
        }

        // The rewritten call carries every filled parameter except those supplied by
        // the arrow source (the lowerer prepends it), in declaration order.
        return node with
        {
            Arguments = filled.Skip(firstFillable).ToList(),
            KeywordArguments = null
        };
    }

    // Splits a keyword EQName into its parts (braced-URI, prefixed, or plain NCName).
    private static (string? Prefix, string Local, string? NamespaceUri) SplitKeywordName(string name)
    {
        if (name.Length > 2 && name[0] == 'Q' && name[1] == '{')
        {
            int closeBrace = name.IndexOf('}');
            if (closeBrace >= 2)
                return (null, name[(closeBrace + 1)..], name[2..closeBrace]);
        }
        int colon = name.IndexOf(':');
        return colon < 0 ? (null, name, null) : (name[..colon], name[(colon + 1)..], null);
    }

    // F&O default-value snippets are parsed once and cached; the AST is immutable
    // (Span is init-only), so the cached nodes can be shared across compilations.
    private static readonly ConcurrentDictionary<string, XPathAstNode> DefaultExpressionCache = new();

    private static XPathAstNode ParseDefaultExpression(string snippet)
        => DefaultExpressionCache.GetOrAdd(snippet, s => XPathParser.ParseExprSingle(s, xpath40: true));

    private static NamedFunctionRefNode ResolveNamedFunctionRef(NamedFunctionRefNode node, CompileOptions options)
    {
        var nsUri = string.IsNullOrEmpty(node.NamespaceUri)
            ? ResolvePrefix(node.Prefix, options)
            : node.NamespaceUri;
        var resolved = node with { NamespaceUri = nsUri };
        ThrowIfRemovedFunction(resolved.NamespaceUri, resolved.LocalName);
        ThrowIfXPath40OnlyFunction(resolved.NamespaceUri, resolved.LocalName, resolved.Prefix, options);
        return resolved;
    }

    /// <summary>
    /// Evaluates the compiled expression against the given context item (typically a document node).
    /// </summary>
    public XdmValue Evaluate(IXdmNode contextItem)
    {
        var ctx = new EvaluationContext()
            .WithFocus(XdmValue.FromNode(contextItem), 1, 1);
        ctx.IsXPath40 = _compatibility >= XPathCompatibility.XPath40;

        FunctionLibrary.Populate(ctx);
        return Evaluate(ctx);
    }

    /// <summary>
    /// Evaluates the compiled expression with a custom evaluation context.
    /// </summary>
    public XdmValue Evaluate(EvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        // REQ-118 version gate: stamp the compiled compatibility onto the context so
        // FunctionLibrary.Populate installs (or hides) XPath 4.0-only functions.
        context.IsXPath40 = _compatibility >= XPathCompatibility.XPath40;
        if (!context.SkipStandardFunctionPopulation)
            FunctionLibrary.Populate(context);

        var savedDefaultNs = context.DefaultElementNamespace;
        var savedDefiningNs = context.DefiningElementDefaultNamespace;
        var savedBaseUri = context.BaseUri;
        try
        {
            if (_defaultElementNamespace != null)
                context.DefaultElementNamespace = _defaultElementNamespace;
            if (_definingElementDefaultNamespace != null)
                context.DefiningElementDefaultNamespace = _definingElementDefaultNamespace;
            if (_baseUri != null)
                context.BaseUri = _baseUri;

            if (_namespaces != null && _namespaces.Count > 0)
            {
                var snapshot = context.SnapshotNamespaces();
                try
                {
                    foreach (var (prefix, nsUri) in _namespaces)
                    {
                        if (!string.IsNullOrEmpty(prefix))
                            context.WithNamespace(prefix, nsUri);
                    }
                    return VmEngine.Execute(_module, context);
                }
                finally
                {
                    context.RestoreNamespaces(snapshot);
                }
            }

            return VmEngine.Execute(_module, context);
        }
        finally
        {
            context.DefaultElementNamespace = savedDefaultNs;
            context.DefiningElementDefaultNamespace = savedDefiningNs;
            context.BaseUri = savedBaseUri;
        }
    }

    /// <summary>
    /// Evaluates and returns the result as a node sequence.
    /// </summary>
    public XdmSequence EvaluateNodes(IXdmNode contextItem)
    {
        var result = Evaluate(contextItem);
        if (result.IsSequence)
            return XdmSequence.FromSource(result.SequenceValue!);
        if (result.IsNode)
            return XdmSequence.Singleton(result);
        return XdmSequence.Empty;
    }

}
