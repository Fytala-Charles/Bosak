// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Represents a compiled XPath 3.1 expression that can be evaluated repeatedly against different inp...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
//                      |                  |       |                | fallback, XPST0081) in the namespace resolution pass                                   |
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
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.15  | 08-10-2026     | REQ-118 4.0-S4: ResolveFunctionNamespaces for PipelineExprNode
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.16  | 09-10-2026     | REQ-123 4.0-Exp S1: stamp IsXPath40Experimental; static XPST0017 for                        |
//                      |                  |       |                | IsXPath40ExperimentalOnly functions (call + named-function-ref forms)                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.17  | 10-10-2026     | REQ-123: version gates made arity-aware (xs:T#0 focus constructors, PR661, share        |
//                      |                  |       |                | names with 3.1 arity-1 constructors)                                                    |
//                      | Charles Korthout | 0.18  | 10-10-2026     | REQ-123 compare-tail slice: QName literal prefix resolution (xml/fn/map/array/math     |
//                      |                  |       |                | fallback, XPST0081) in the namespace resolution pass                                   |
//                      |==================|=======|================|=========================================================================================
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.19  | 11-10-2026     | Preserve and enforce frozen profile boundaries in mixed-version XSLT execution.        |
//                      | Charles Korthout | 0.20  | 12-10-2026     | Resolve enumeration aliases in instance-of and treat AST nodes                           |
//                      | Charles Korthout | 0.21  | 13-10-2026     | Apply expression namespace context and default wildcard tests
//                      | Charles Korthout | 0.22 | 13-10-2026     | Expand host atomic and array aliases in XPath instance-of and treat expressions
//                      | Charles Korthout | 0.23 | 13-10-2026     | Expand nested named types inside XPath array and map instance-of/treat operands
//                      | Charles Korthout | 0.24 | 13-10-2026     | Expand aliases in cast/castable targets and reject ineligible alias targets
//                      | Charles Korthout | 0.25 | 13-10-2026     | Validate direct and expanded choice casting targets statically in XPath 4.0
//                      | Charles Korthout | 0.26 | 13-10-2026     | Validate host type names and retain nominative namespace identity in expression targets
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
    /// <param name="expression">The XPath source text.</param>
    /// <returns>The compiled expression.</returns>
    public static XPath31Expression Compile(string expression)
        => Compile(expression, CompileOptions.Default);

    /// <summary>
    /// Parses and compiles an XPath expression with the specified options.
    /// </summary>
    /// <param name="expression">The XPath source text, including optional XPath 4.0 namespace declarations.</param>
    /// <param name="options">The compatibility profile and host static context.</param>
    /// <returns>The compiled expression with its expression-local namespace context.</returns>
    /// <exception cref="XPathParseException">The expression or namespace declaration grammar is invalid.</exception>
    public static XPath31Expression Compile(string expression, CompileOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(expression))
            throw new XPathParseException("Empty expression is not a valid XPath expression", 0);

        var declarations = XPathParser.ExtractNamespaceDeclarations(expression, options.Compatibility >= XPathCompatibility.XPath40);
        if (declarations.Namespaces.Count > 0 || declarations.DefaultNamespace is not null)
        {
            var namespaces = options.Namespaces is null
                ? new Dictionary<string, string>(StringComparer.Ordinal)
                : new Dictionary<string, string>(options.Namespaces, StringComparer.Ordinal);
            foreach (var (prefix, uri) in declarations.Namespaces)
                namespaces[prefix] = uri;
            options = new CompileOptions
            {
                Compatibility = options.Compatibility, Namespaces = namespaces,
                DefaultElementNamespace = declarations.DefaultNamespace ?? options.DefaultElementNamespace,
                DefiningElementDefaultNamespace = options.DefiningElementDefaultNamespace,
                EnumerationItemTypes = options.EnumerationItemTypes, ItemTypeAliases = options.ItemTypeAliases, NamedRecordTypeNames = options.NamedRecordTypeNames, SchemaSet = options.SchemaSet,
                Xml11LineEndings = options.Xml11LineEndings, BaseUri = options.BaseUri,
                BackwardsCompatible = options.BackwardsCompatible, StaticVariableTypes = options.StaticVariableTypes,
                StrictStaticTyping = options.StrictStaticTyping, EnableJit = options.EnableJit
            };
            expression = declarations.Expression;
        }

        // 1. Lex + Parse -> AST
        var ast = XPathParser.Parse(expression, xml11LineEndings: options.Xml11LineEndings, schemaAware: options.SchemaSet is not null, xpath40: options.Compatibility >= XPathCompatibility.XPath40);

        // 2. Resolve function-call namespaces using the supplied static context and
        // report static errors for functions that have been removed from the spec.
        ast = ResolveFunctionNamespaces(ast, options);
        var defaultElementNamespace = options.Compatibility >= XPathCompatibility.XPath40
            && options.DefaultElementNamespace == "##any" ? string.Empty : options.DefaultElementNamespace;

        // 2b. Static name-test validation against the in-scope namespaces (XPST0081 for
        // undeclared prefixes) and schema-aware kind tests (XPST0008): without a schema
        // set there is no schema awareness; with one (REQ-104) the kind-test name argument
        // must resolve to a global declaration in the set. Skipped when no namespace
        // context is supplied: XSLT patterns and runtime-namespace compilations keep the
        // runtime NamespaceTest resolution (the runtime kind tests raise XPST0008 for
        // undeclared schema names).
        if (options.Namespaces is not null)
            Compiler.StaticNameTestValidator.Validate(ast, prefix =>
                options.Namespaces.TryGetValue(prefix, out var nsUri) && nsUri.Length > 0 ? nsUri : null,
                options.SchemaSet, defaultElementNamespace);

        // 3. Optimize AST
        var optimizer = new XPathOptimizer();
        var optimized = optimizer.Optimize(ast, options.BackwardsCompatible);

        // 4. Lower to IR
        var lowerer = new IrLowerer();
        var module = lowerer.Lower(optimized);

        return new XPath31Expression(module, options.Namespaces, defaultElementNamespace, options.DefiningElementDefaultNamespace, options.BaseUri, options.Compatibility);
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
        {
            if (nsUri.Length == 0)
                throw new InvalidOperationException($"XPST0081: Namespace prefix '{prefix}' was undeclared.");
            return nsUri;
        }

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
    private static void ThrowIfXPath40OnlyFunction(string? nsUri, string localName, int arity, string? prefix, CompileOptions options)
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
                "xs" => "http://www.w3.org/2001/XMLSchema",
                _ => null,
            };
        }
        // Arity-aware: the 4.0 focus constructors (xs:T#0, PR661) share their name with
        // the 3.1 arity-1 constructors, so the name set is only a pre-filter; the actual
        // decision comes from the resolved signature's IsXPath40Only flag.
        if (!string.IsNullOrEmpty(nsUri)
            && FunctionLibrary.XPath40OnlyFunctionNames.Contains((nsUri, localName))
            && FunctionLibrary.TryGetFunction(nsUri, localName, arity, out var gated)
            && gated.IsXPath40Only)
            throw new InvalidOperationException(
                $"XPST0017: Function {{{nsUri}}}{localName} is defined in XPath 4.0 and is not available when targeting XPath {(int)options.Compatibility}; set CompileOptions.Compatibility to XPathCompatibility.XPath40.");
    }

    // REQ-123 experimental tier (4.0-Exp S1): functions marked IsXPath40ExperimentalOnly
    // are rejected at compile time unless the compilation targets XPath40Experimental.
    // They are NOT IsXPath40Only, so the frozen-level check above does not see them;
    // this is a separate check mirroring ThrowIfXPath40OnlyFunction, with the same
    // canonical-URI fallback for unresolved predefined prefixes.
    private static void ThrowIfXPath40ExperimentalOnlyFunction(string? nsUri, string localName, int arity, string? prefix, CompileOptions options)
    {
        if (options.Compatibility >= XPathCompatibility.XPath40Experimental)
            return;
        if (string.IsNullOrEmpty(nsUri))
        {
            nsUri = prefix switch
            {
                "fn" => DefaultFunctionNamespace,
                "map" => "http://www.w3.org/2005/xpath-functions/map",
                "array" => "http://www.w3.org/2005/xpath-functions/array",
                "math" => "http://www.w3.org/2005/xpath-functions/math",
                "xs" => "http://www.w3.org/2001/XMLSchema",
                _ => null,
            };
        }
        // Arity-aware for the same reason as ThrowIfXPath40OnlyFunction (xs:T#0 focus
        // constructors share names with 3.1 arity-1 constructors).
        if (!string.IsNullOrEmpty(nsUri)
            && FunctionLibrary.XPath40ExperimentalOnlyFunctionNames.Contains((nsUri, localName))
            && FunctionLibrary.TryGetFunction(nsUri, localName, arity, out var gated)
            && gated.IsXPath40ExperimentalOnly)
            throw new InvalidOperationException(
                $"XPST0017: Function {{{nsUri}}}{localName} is a not-yet-stabilized XPath 4.0 addition and is not available when targeting XPath {(int)options.Compatibility}; set CompileOptions.Compatibility to XPathCompatibility.XPath40Experimental.");
    }

    private static XPathAstNode ResolveFunctionNamespaces(XPathAstNode node, CompileOptions options)
    {
        return node switch
        {
            VariableReferenceNode variable when variable.NamespaceUri is null && variable.Prefix is not null
                && options.Namespaces is not null && options.Namespaces.TryGetValue(variable.Prefix, out var variableUri) && variableUri.Length == 0
                => throw new InvalidOperationException($"XPST0008: Variable '${variable.Prefix}:{variable.LocalName}' uses an undeclared namespace prefix."),
            FunctionCallNode fc => ResolveFunctionCall(fc, options),
            NamedFunctionRefNode nf => ResolveNamedFunctionRef(nf, options),
            ParenthesizedExprNode p => p with { Expression = ResolveFunctionNamespaces(p.Expression, options) },
            PredicateNode pred => pred with { Expression = ResolveFunctionNamespaces(pred.Expression, options) },
            StepNode step => step with { NodeTest = ResolveDefaultNamespaceTest(step, options), Predicates = step.Predicates.Select(p => ResolveFunctionNamespaces(p, options)).ToList() },
            PathExprNode path => path with { Steps = path.Steps.Select(s => ResolveFunctionNamespaces(s, options)).ToList() },
            SequenceExpressionNode seq => seq with { Expressions = seq.Expressions.Select(e => ResolveFunctionNamespaces(e, options)).ToList() },
            RangeExpressionNode range => range with { From = ResolveFunctionNamespaces(range.From, options), To = ResolveFunctionNamespaces(range.To, options) },
            IfExpressionNode ife => ife with { Condition = ResolveFunctionNamespaces(ife.Condition, options), ThenBranch = ResolveFunctionNamespaces(ife.ThenBranch, options), ElseBranch = ResolveFunctionNamespaces(ife.ElseBranch, options) },
            ForExpressionNode fe => fe with { Bindings = fe.Bindings.Select(b => b with { Expression = ResolveFunctionNamespaces(b.Expression, options) }).ToList(), ReturnExpression = ResolveFunctionNamespaces(fe.ReturnExpression, options) },
            LetExpressionNode le => le with { Bindings = le.Bindings.Select(b => b with { Expression = ResolveFunctionNamespaces(b.Expression, options) }).ToList(), Body = ResolveFunctionNamespaces(le.Body, options) },
            QuantifiedExpressionNode qe => qe with { Bindings = qe.Bindings.Select(b => b with { Expression = ResolveFunctionNamespaces(b.Expression, options) }).ToList(), SatisfiesExpression = ResolveFunctionNamespaces(qe.SatisfiesExpression, options) },
            BinaryExpressionNode bin => bin with { Left = ResolveFunctionNamespaces(bin.Left, options), Right = ResolveFunctionNamespaces(bin.Right, options) },
            UnaryExpressionNode un => un with { Operand = ResolveFunctionNamespaces(un.Operand, options) },
            CastNode cast => ResolveCastTypeAlias(cast with { Expression = ResolveFunctionNamespaces(cast.Expression, options), TypeName = ResolveDefaultItemType(cast.TypeName, cast.Prefix, options, castTarget: true) }, options),
            CastableNode castable => ResolveCastTypeAlias(castable with { Expression = ResolveFunctionNamespaces(castable.Expression, options), TypeName = ResolveDefaultItemType(castable.TypeName, castable.Prefix, options, castTarget: true) }, options),
            InstanceOfNode io => ResolveSequenceTypeAlias(io with { Expression = ResolveFunctionNamespaces(io.Expression, options), TypeName = ResolveDefaultItemType(io.TypeName, io.Prefix, options) }, options),
            TreatNode treat => ResolveSequenceTypeAlias(treat with { Expression = ResolveFunctionNamespaces(treat.Expression, options), TypeName = ResolveDefaultItemType(treat.TypeName, treat.Prefix, options) }, options),
            ArrowExprNode arrow => arrow with { Source = ResolveFunctionNamespaces(arrow.Source, options), Target = ResolveArrowTarget(arrow.Target, options) },
            PipelineExprNode pipeline => pipeline with { Source = ResolveFunctionNamespaces(pipeline.Source, options), Target = ResolveFunctionNamespaces(pipeline.Target, options) },
            TryCatchNode tc => tc with
            {
                TryExpression = ResolveFunctionNamespaces(tc.TryExpression, options),
                Clauses = tc.Clauses.Select(c => c with { Expression = ResolveFunctionNamespaces(c.Expression, options) }).ToList()
            },
            StringConstructorNode sc => sc with { Parts = sc.Parts.Select(p => ResolveFunctionNamespaces(p, options)).ToList() },
            QNameLiteralNode qn => ResolveQNameLiteral(qn, options),
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

    private static NodeTest ResolveDefaultNamespaceTest(StepNode step, CompileOptions options)
    {
        if (options.Compatibility >= XPathCompatibility.XPath40 && options.DefaultElementNamespace == "##any"
            && step.Axis is not (XdmAxis.Attribute or XdmAxis.Namespace) && step.NodeTest.Kind == NameTestKind.LocalName
            && step.NodeTest.NamespaceUri is null)
            return step.NodeTest with { Kind = NameTestKind.QName, NamespaceUri = "*" };
        return step.NodeTest;
    }

    private static string ResolveDefaultItemType(string name, string? prefix, CompileOptions options, bool castTarget = false)
    {
        if (options.Compatibility < XPathCompatibility.XPath40 || options.DefaultElementNamespace != "##any"
            || !string.IsNullOrEmpty(prefix) || name.Contains('(') || name.Contains(':') || name.StartsWith("Q{", StringComparison.Ordinal))
            return name;
        if (castTarget && name is "anyAtomicType" or "anySimpleType" or "NOTATION")
            throw new InvalidOperationException($"XPST0080: '{name}' is an abstract cast target.");
        if (castTarget && name is "untyped" or "anyType")
            throw new InvalidOperationException($"XQST0052: '{name}' is not an atomic cast target.");
        return "Q{http://www.w3.org/2001/XMLSchema}" + name;
    }

    // Resolves a QName literal's lexical prefix against the static context. The braced
    // form (#Q{uri}local) is already resolved by the parser; #local needs no resolution.
    // A prefixed form resolves against the supplied namespaces with fallback to the
    // canonical predefined bindings (xml/fn/map/array/math, matching EvaluationContext);
    // an unresolvable prefix is the static error XPST0081 (Literals-40-924).
    private static QNameLiteralNode ResolveQNameLiteral(QNameLiteralNode node, CompileOptions options)
    {
        if (node.NamespaceUri is not null || string.IsNullOrEmpty(node.Prefix))
            return node;

        string? nsUri = null;
        if (options.Namespaces is not null && options.Namespaces.TryGetValue(node.Prefix, out var bound))
            nsUri = bound;
        else
            nsUri = node.Prefix switch
            {
                "xml" => "http://www.w3.org/XML/1998/namespace",
                "fn" => DefaultFunctionNamespace,
                "map" => "http://www.w3.org/2005/xpath-functions/map",
                "array" => "http://www.w3.org/2005/xpath-functions/array",
                "math" => "http://www.w3.org/2005/xpath-functions/math",
                _ => null,
            };
        if (string.IsNullOrEmpty(nsUri))
            throw new InvalidOperationException($"XPST0081: No namespace declaration for prefix '{node.Prefix}' in QName literal.");
        return node with { NamespaceUri = nsUri };
    }

    private static XPathAstNode ResolveItemTypeAlias(XPathAstNode node, CompileOptions options)
    {
        if (options.Compatibility < XPathCompatibility.XPath40 || (options.EnumerationItemTypes is null && options.ItemTypeAliases is null))
            return node;
        var (prefix, name) = node switch
        {
            CastNode cast => (cast.Prefix, cast.TypeName),
            CastableNode castable => (castable.Prefix, castable.TypeName),
            InstanceOfNode instance => (instance.Prefix, instance.TypeName),
            TreatNode treat => (treat.Prefix, treat.TypeName),
            _ => (null, string.Empty)
        };
        if (options.ItemTypeAliases is not null && name.Contains('('))
        {
            var expanded = ItemTypeAliasExpansion.Expand(name, lexical =>
            {
                var expandedName = lexical;
                if (!lexical.StartsWith("Q{", StringComparison.Ordinal))
                {
                    var colon = lexical.IndexOf(':');
                    var ns = options.DefaultElementNamespace ?? string.Empty;
                    if (colon >= 0 && (options.Namespaces is null || !options.Namespaces.TryGetValue(lexical[..colon], out ns))) return lexical;
                    expandedName = $"Q{{{ns}}}{(colon < 0 ? lexical : lexical[(colon + 1)..])}";
                }
                return options.ItemTypeAliases.TryGetValue(expandedName, out var definition) ? definition : lexical;
            });
            if (expanded is not null && expanded != name)
            {
                var parsedType = XPathParser.Parse("() instance of " + expanded, xpath40: true);
                if (parsedType is InstanceOfNode nested)
                {
                    ValidateAliasCastTarget(node, nested);
                    return node switch
                    {
                        CastNode cast => cast with { Prefix = nested.Prefix, TypeName = nested.TypeName },
                        CastableNode castable => castable with { Prefix = nested.Prefix, TypeName = nested.TypeName },
                        InstanceOfNode instance => instance with { Prefix = nested.Prefix, TypeName = nested.TypeName },
                        TreatNode treat => treat with { Prefix = nested.Prefix, TypeName = nested.TypeName },
                        _ => node
                    };
                }
            }
            return node;
        }
        string key;
        if (name.StartsWith("Q{", StringComparison.Ordinal))
            key = name;
        else
        {
            var ns = options.DefaultElementNamespace ?? string.Empty;
            if (!string.IsNullOrEmpty(prefix))
            {
                if (options.Namespaces is null || !options.Namespaces.TryGetValue(prefix, out ns))
                    return node;
            }
            key = $"Q{{{ns}}}{name}";
        }
        var hostAlias = options.ItemTypeAliases is not null && options.ItemTypeAliases.TryGetValue(key, out _);
        string? definition;
        if (hostAlias) definition = options.ItemTypeAliases![key];
        else if (options.EnumerationItemTypes is null || !options.EnumerationItemTypes.TryGetValue(key, out definition)) return node;
        if (!hostAlias && !definition.TrimStart().StartsWith("enum(", StringComparison.Ordinal))
            throw new ArgumentException("Enumeration aliases must contain enum(...) item types.");
        var parsed = XPathParser.Parse("() instance of " + definition, xpath40: true);
        if (parsed is not InstanceOfNode { Occurrence: OccurrenceIndicator.One } alias)
            throw new ArgumentException("An item-type alias must denote one item type.");
        ValidateAliasCastTarget(node, alias);
        return node switch
        {
            CastNode cast => cast with { Prefix = alias.Prefix, TypeName = alias.TypeName },
            CastableNode castable => castable with { Prefix = alias.Prefix, TypeName = alias.TypeName },
            InstanceOfNode instance => instance with { Prefix = alias.Prefix, TypeName = alias.TypeName },
            TreatNode treat => treat with { Prefix = alias.Prefix, TypeName = alias.TypeName },
            _ => node
        };
    }

    private static XPathAstNode ResolveSequenceTypeAlias(XPathAstNode node, CompileOptions options)
    {
        var resolved = ResolveItemTypeAlias(node, options);
        if (options.Compatibility < XPathCompatibility.XPath40 || options.NamedRecordTypeNames is null)
            return resolved;
        var (prefix, type) = resolved switch
        {
            CastNode cast => (cast.Prefix, cast.TypeName),
            CastableNode castable => (castable.Prefix, castable.TypeName),
            InstanceOfNode instance => (instance.Prefix, instance.TypeName),
            TreatNode treat => (treat.Prefix, treat.TypeName),
            _ => (null, string.Empty)
        };
        var text = string.IsNullOrEmpty(prefix) ? type : prefix + ":" + type;
        var expanded = ItemTypeAliasExpansion.Expand(text, lexical => ResolveStaticItemTypeName(lexical, options));
        if (expanded is null || expanded == text) return resolved;
        var parsed = (InstanceOfNode)XPathParser.Parse("() instance of " + expanded, xpath40: true);
        return resolved switch
        {
            CastNode cast => cast with { Prefix = parsed.Prefix, TypeName = parsed.TypeName },
            CastableNode castable => castable with { Prefix = parsed.Prefix, TypeName = parsed.TypeName },
            InstanceOfNode instance => instance with { Prefix = parsed.Prefix, TypeName = parsed.TypeName },
            TreatNode treat => treat with { Prefix = parsed.Prefix, TypeName = parsed.TypeName },
            _ => resolved
        };
    }

    private static string ResolveStaticItemTypeName(string lexical, CompileOptions options)
    {
        const string schemaNamespace = "http://www.w3.org/2001/XMLSchema";
        string uri, local;
        if (lexical.StartsWith("Q{", StringComparison.Ordinal))
        {
            var close = lexical.IndexOf('}');
            uri = lexical[2..close]; local = lexical[(close + 1)..];
        }
        else
        {
            var colon = lexical.IndexOf(':');
            local = colon < 0 ? lexical : lexical[(colon + 1)..];
            uri = options.DefaultElementNamespace ?? string.Empty;
            if (colon >= 0)
            {
                if (options.Namespaces?.TryGetValue(lexical[..colon], out var namespaceUri) == true)
                    uri = namespaceUri;
                else if (lexical[..colon] == "xs") uri = schemaNamespace;
                else throw new InvalidOperationException($"XPST0081: Undeclared type prefix in '{lexical}'.");
            }
        }
        var key = $"Q{{{uri}}}{local}";
        if (options.ItemTypeAliases?.TryGetValue(key, out var definition) == true) return definition;
        if (options.NamedRecordTypeNames!.Contains(key)) return key;
        var qname = new System.Xml.XmlQualifiedName(local, uri);
        if (uri == schemaNamespace)
        {
            // XDM/XSD 1.1 additions absent from .NET's XSD 1.0 built-in table.
            if (local is "untypedAtomic" or "untyped" or "numeric" or "error" or "dayTimeDuration" or "yearMonthDuration" or "dateTimeStamp" ||
                System.Xml.Schema.XmlSchemaType.GetBuiltInSimpleType(qname) is not null ||
                System.Xml.Schema.XmlSchemaType.GetBuiltInComplexType(qname) is not null)
                return "xs:" + local;
        }
        else if (options.SchemaSet?.GlobalTypes[qname] is System.Xml.Schema.XmlSchemaSimpleType)
            return key;
        throw new InvalidOperationException($"XPST0051: Unknown item type '{lexical}' in the host static context.");
    }

    private static XPathAstNode ResolveCastTypeAlias(XPathAstNode node, CompileOptions options)
    {
        var resolved = ResolveSequenceTypeAlias(node, options);
        if (options.Compatibility < XPathCompatibility.XPath40) return resolved;
        var target = resolved switch
        {
            CastNode cast => cast.TypeName,
            CastableNode castable => castable.TypeName,
            _ => string.Empty
        };
        if (ItemTypeAliasExpansion.HasIneligibleCastTarget(target, isNonAtomicName: name => options.NamedRecordTypeNames?.Contains(name) == true))
            throw new InvalidOperationException("XPST0080: The target is not an eligible cast type; choice alternatives must be generalized atomic types.");
        return resolved;
    }

    private static void ValidateAliasCastTarget(XPathAstNode node, InstanceOfNode alias)
    {
        if (node is (CastNode or CastableNode) && ItemTypeAliasExpansion.HasIneligibleCastTarget(alias.TypeName))
            throw new InvalidOperationException("XPST0080: The alias is not an eligible cast type; choice alternatives must be generalized atomic types.");
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
        ThrowIfXPath40OnlyFunction(resolved.NamespaceUri, resolved.LocalName, resolved.Arguments.Count, resolved.Prefix, options);
        ThrowIfXPath40ExperimentalOnlyFunction(resolved.NamespaceUri, resolved.LocalName, resolved.Arguments.Count, resolved.Prefix, options);
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
        ThrowIfXPath40OnlyFunction(resolved.NamespaceUri, resolved.LocalName, resolved.Arity, resolved.Prefix, options);
        ThrowIfXPath40ExperimentalOnlyFunction(resolved.NamespaceUri, resolved.LocalName, resolved.Arity, resolved.Prefix, options);
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
        ctx.IsXPath40Experimental = _compatibility >= XPathCompatibility.XPath40Experimental;

        FunctionLibrary.Populate(ctx);
        return Evaluate(ctx);
    }

    /// <summary>
    /// Evaluates the compiled expression with a custom evaluation context.
    /// </summary>
    public XdmValue Evaluate(EvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        // XSLT keeps a shared registry across mixed-version instructions. Nested
        // evaluations must restore the caller's profile before its VM resumes.
        var restoreCompatibility = context.IsXsltMode && context.SkipStandardFunctionPopulation;
        var savedXPath40 = context.IsXPath40;
        var savedXPath40Experimental = context.IsXPath40Experimental;
        // REQ-118 version gate: stamp the compiled compatibility onto the context so
        // FunctionLibrary.Populate installs (or hides) XPath 4.0-only functions; the
        // experimental tier (REQ-123) is additive over the frozen 4.0 level.
        context.IsXPath40 = _compatibility >= XPathCompatibility.XPath40;
        context.IsXPath40Experimental = _compatibility >= XPathCompatibility.XPath40Experimental;
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
            if (restoreCompatibility)
            {
                context.IsXPath40 = savedXPath40;
                context.IsXPath40Experimental = savedXPath40Experimental;
            }
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
