// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Represents a compiled XPath 3.1 expression that can be evaluated repeatedly against different inp...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-06-2026     | Added DefiningElementDefaultNamespace for element-available default namespace            |
//                      | Charles Korthout | 0.3   | 26-06-2026     | Compile-time namespace resolution and static errors for removed functions                |
//                      | Charles Korthout | 0.4   | 27-06-2026     | Preserve explicit braced-URI namespace URIs in function calls and named function refs    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
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

    private XPath31Expression(IrModule module, IReadOnlyDictionary<string, string>? namespaces = null, string? defaultElementNamespace = null, string? definingElementDefaultNamespace = null, string? baseUri = null)
    {
        _module = module;
        _namespaces = namespaces;
        _defaultElementNamespace = defaultElementNamespace;
        _definingElementDefaultNamespace = definingElementDefaultNamespace;
        _baseUri = baseUri;
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
        ArgumentException.ThrowIfNullOrEmpty(expression);
        ArgumentNullException.ThrowIfNull(options);

        // 1. Lex + Parse -> AST
        var ast = XPathParser.Parse(expression);

        // 2. Resolve function-call namespaces using the supplied static context and
        // report static errors for functions that have been removed from the spec.
        ast = ResolveFunctionNamespaces(ast, options);

        // 3. Optimize AST
        var optimizer = new XPathOptimizer();
        var optimized = optimizer.Optimize(ast);

        // 4. Lower to IR
        var lowerer = new IrLowerer();
        var module = lowerer.Lower(optimized);

        return new XPath31Expression(module, options.Namespaces, options.DefaultElementNamespace, options.DefiningElementDefaultNamespace, options.BaseUri);
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
            ArrowExprNode arrow => arrow with { Source = ResolveFunctionNamespaces(arrow.Source, options), Target = ResolveFunctionNamespaces(arrow.Target, options) },
            TryCatchNode tc => tc with { TryExpression = ResolveFunctionNamespaces(tc.TryExpression, options), CatchExpression = ResolveFunctionNamespaces(tc.CatchExpression, options) },
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

    private static FunctionCallNode ResolveFunctionCall(FunctionCallNode node, CompileOptions options)
    {
        var nsUri = string.IsNullOrEmpty(node.NamespaceUri)
            ? ResolvePrefix(node.Prefix, options)
            : node.NamespaceUri;
        var resolved = node with
        {
            Arguments = node.Arguments.Select(a => ResolveFunctionNamespaces(a, options)).ToList(),
            NamespaceUri = nsUri
        };
        ThrowIfRemovedFunction(resolved.NamespaceUri, resolved.LocalName);
        return resolved;
    }

    private static NamedFunctionRefNode ResolveNamedFunctionRef(NamedFunctionRefNode node, CompileOptions options)
    {
        var nsUri = string.IsNullOrEmpty(node.NamespaceUri)
            ? ResolvePrefix(node.Prefix, options)
            : node.NamespaceUri;
        var resolved = node with { NamespaceUri = nsUri };
        ThrowIfRemovedFunction(resolved.NamespaceUri, resolved.LocalName);
        return resolved;
    }

    /// <summary>
    /// Evaluates the compiled expression against the given context item (typically a document node).
    /// </summary>
    public XdmValue Evaluate(IXdmNode contextItem)
    {
        var ctx = new EvaluationContext()
            .WithFocus(XdmValue.FromNode(contextItem), 1, 1);

        FunctionLibrary.Populate(ctx);
        return Evaluate(ctx);
    }

    /// <summary>
    /// Evaluates the compiled expression with a custom evaluation context.
    /// </summary>
    public XdmValue Evaluate(EvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
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
