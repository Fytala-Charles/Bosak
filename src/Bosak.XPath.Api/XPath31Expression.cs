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

        // 2. Optimize AST
        var optimizer = new XPathOptimizer();
        var optimized = optimizer.Optimize(ast);

        // 3. Lower to IR
        var lowerer = new IrLowerer();
        var module = lowerer.Lower(optimized);

        return new XPath31Expression(module, options.Namespaces, options.DefaultElementNamespace, options.DefiningElementDefaultNamespace, options.BaseUri);
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
