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
//                      | Charles Korthout | 1.1   | 25-07-2026     | Register element/content-node constructor hooks for XQuery constructors                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.2   | 25-07-2026     | Register attribute and document constructor hooks                                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.3   | 25-07-2026     | Seed static output parameters; expand QName lists with the default element namespace    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
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

        // XQuery element constructors need a node-building provider; default to XDocument.
        evaluationContext.ElementConstructorHook ??= XDocumentProvider.ConstructElement;
        evaluationContext.ContentNodeConstructorHook ??= XDocumentProvider.ConstructContentNode;
        evaluationContext.AttributeConstructorHook ??= XDocumentProvider.ConstructAttribute;
        evaluationContext.DocumentConstructorHook ??= XDocumentProvider.ConstructDocument;

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

        // Output declarations (declare option output:* "...") become the static
        // serialization parameters consumed by fn:serialize.
        var outputOptions = _staticContext.Options
            .Where(o => o.NamespaceUri == "http://www.w3.org/2010/xslt-xquery-serialization")
            .ToList();
        if (outputOptions.Count > 0)
        {
            var parameters = new Dictionary<(string, string), string>();
            foreach (var (local, ns, value) in outputOptions)
            {
                parameters[(ns, local)] = local is "cdata-section-elements" or "suppress-indentation"
                    ? ExpandQNameList(value)
                    : value;
            }
            ctx.StaticOutputParameters = parameters;
        }
    }

    // Expands QName tokens in a whitespace-separated list to '{uri}local' form using the
    // prolog's namespace bindings ('{uri}local' forms pass through; unprefixed names stay
    // in no namespace).
    private string ExpandQNameList(string value)
    {
        var tokens = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            if (token.StartsWith("Q{", StringComparison.Ordinal))
                token = token[1..]; // Q{uri}local → {uri}local
            if (token.StartsWith('{'))
            {
                tokens[i] = token;
                continue;
            }
            int colon = token.IndexOf(':');
            if (colon <= 0)
            {
                // Unprefixed names in these lists are in the default element namespace.
                tokens[i] = string.IsNullOrEmpty(_staticContext.DefaultElementNamespace)
                    ? token
                    : $"{{{_staticContext.DefaultElementNamespace}}}{token}";
                continue;
            }
            if (!_staticContext.Namespaces.TryGetValue(token[..colon], out var prefixNs))
                throw new InvalidOperationException($"XPST0081: Prefix '{token[..colon]}' is not declared.");
            tokens[i] = $"{{{prefixNs}}}{token[(colon + 1)..]}";
        }
        return string.Join(' ', tokens);
    }
}
