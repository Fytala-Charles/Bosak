// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 June 2026
// PURPOSE              : Represents a compiled XQuery ready for execution.
// SPECIAL NOTES        : Part of the Bosak XQuery 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-06-2026     | Creation — placeholder skeleton                                                          |
//                      | Charles Korthout | 1.0   | 22-07-2026     | Execute via XPath VM using static context                                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Bosak.XQuery.Compiler;

namespace Bosak.XQuery.Api;

/// <summary>
/// A compiled, thread-safe XQuery that can be evaluated against a context document.
/// </summary>
public sealed class XQueryExecutable
{
    private readonly IrModule _module;
    private readonly XQueryStaticContext _staticContext;

    internal XQueryExecutable(IrModule module, XQueryStaticContext staticContext)
    {
        _module = module;
        _staticContext = staticContext;
    }

    /// <summary>
    /// Executes the compiled query.
    /// </summary>
    /// <param name="context">The evaluation context (variables, context item, namespaces).</param>
    /// <returns>The result of the query as an <see cref="XdmValue"/>.</returns>
    public XdmValue Evaluate(XQueryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var evaluationContext = context.EvaluationContext;
        var snapshot = evaluationContext.SnapshotNamespaces();
        var savedDefaultNs = evaluationContext.DefaultElementNamespace;
        var savedBaseUri = evaluationContext.BaseUri;
        var savedCollation = evaluationContext.DefaultCollation;

        try
        {
            ApplyStaticContext(evaluationContext);
            return VmEngine.Execute(_module, evaluationContext);
        }
        finally
        {
            evaluationContext.RestoreNamespaces(snapshot);
            evaluationContext.DefaultElementNamespace = savedDefaultNs;
            evaluationContext.BaseUri = savedBaseUri;
            evaluationContext.DefaultCollation = savedCollation;
        }
    }

    private void ApplyStaticContext(EvaluationContext ctx)
    {
        // Populate the standard function library once per execution context.
        if (!ctx.SkipStandardFunctionPopulation)
            FunctionLibrary.Populate(ctx);

        if (!string.IsNullOrEmpty(_staticContext.DefaultElementNamespace))
            ctx.DefaultElementNamespace = _staticContext.DefaultElementNamespace;

        if (!string.IsNullOrEmpty(_staticContext.BaseUri))
            ctx.BaseUri = _staticContext.BaseUri;

        if (!string.IsNullOrEmpty(_staticContext.DefaultCollation))
            ctx.DefaultCollation = _staticContext.DefaultCollation;

        foreach (var (prefix, nsUri) in _staticContext.Namespaces)
        {
            if (!string.IsNullOrEmpty(prefix))
                ctx.WithNamespace(prefix, nsUri);
        }

        foreach (var ((localName, nsUri), value) in _staticContext.Variables)
        {
            ctx.WithVariable(localName, value, nsUri);
        }
    }
}
