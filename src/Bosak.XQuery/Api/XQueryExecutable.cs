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
//                      | Charles Korthout | 1.4   | 26-07-2026     | Register user functions via FunctionSignature dispatch; lazy user variables with XQST0054 cycle detection |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.5   | 27-07-2026     | Per-library-module runtime context (namespaces, base URI) around user function/variable body execution |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.6   | 27-07-2026     | Global-variable initializer errors marked to bypass try/catch (try-006/007) |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Bosak.XQuery.Compiler;

namespace Bosak.XQuery.Api;

/// <summary>A user function declaration compiled to an executable body module. The optional
/// module fields carry the declaring library module's runtime static context (null for the
/// main module) so the body executes with that module's namespaces and base URI.</summary>
public sealed record CompiledUserFunction(
    string LocalName,
    string NamespaceUri,
    IReadOnlyList<string> Parameters,
    IReadOnlyList<string?> ParameterTypes,
    string? ReturnType,
    IrModule Body,
    IReadOnlyDictionary<string, string>? ModuleNamespaces = null,
    string? ModuleBaseUri = null,
    string? ModuleDefaultElementNamespace = null,
    string? ModuleDefaultCollation = null);

/// <summary>A user variable declaration compiled to an executable body module (null for external).
/// The optional module fields mirror <see cref="CompiledUserFunction"/>.</summary>
public sealed record CompiledUserVariable(
    string LocalName,
    string NamespaceUri,
    string? TypeName,
    IrModule? Body,
    bool IsExternal,
    IReadOnlyDictionary<string, string>? ModuleNamespaces = null,
    string? ModuleBaseUri = null,
    string? ModuleDefaultElementNamespace = null,
    string? ModuleDefaultCollation = null);

/// <summary>
/// A compiled, thread-safe XQuery that can be evaluated against a context document.
/// </summary>
public sealed class XQueryExecutable
{
    private readonly IrModule _module;
    private readonly XQueryStaticContext _staticContext;
    private readonly IReadOnlyList<CompiledUserFunction> _userFunctions;
    private readonly IReadOnlyList<CompiledUserVariable> _userVariables;

    internal XQueryExecutable(
        IrModule module,
        XQueryStaticContext staticContext,
        IReadOnlyList<CompiledUserFunction>? userFunctions = null,
        IReadOnlyList<CompiledUserVariable>? userVariables = null)
    {
        _module = module;
        _staticContext = staticContext;
        _userFunctions = userFunctions ?? [];
        _userVariables = userVariables ?? [];
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
        // Unwrap global-variable error markers so callers see the original error.
        catch (GlobalVariableEvaluationException gve) when (gve.InnerException is not null)
        {
            throw gve.InnerException;
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

        // User-declared functions dispatch through a compiled InlineFunctionItem body.
        foreach (var fn in _userFunctions)
        {
            var captured = fn;
            ctx.RegisterFunction(new FunctionSignature
            {
                NamespaceUri = captured.NamespaceUri,
                LocalName = captured.LocalName,
                Arity = captured.Parameters.Count,
                ParameterTypes = captured.Parameters.Select(_ => XdmValueKind.External).ToList(),
                ReturnType = XdmValueKind.External,
                ParameterTypeNames = captured.ParameterTypes,
                ReturnTypeName = captured.ReturnType,
                Implementation = (callCtx, args) => InvokeWithModuleContext(captured, callCtx, args)
            });
        }

        // User-declared variables evaluate lazily on first reference (globals).
        if (_userVariables.Count > 0)
        {
            var previousResolver = ctx.LazyVariableResolver;
            var inFlight = new HashSet<(string, string)>();
            // XQuery: a global variable's initializing expression is evaluated with the
            // module's initial dynamic context — including the initial context item —
            // regardless of where the variable is first referenced (function-declaration-026).
            var initialItem = ctx.ContextItem;
            var initialPosition = ctx.ContextPosition;
            var initialSize = ctx.ContextSize;
            ctx.LazyVariableResolver = (local, ns) =>
            {
                foreach (var v in _userVariables)
                {
                    if (v.LocalName == local && v.NamespaceUri == ns && v.Body is not null)
                    {
                        // XQST0054: circular variable dependency.
                        if (!inFlight.Add((local, ns)))
                            throw new InvalidOperationException($"XQST0054: Circular variable dependency for variable '${local}'.");
                        var savedItem = ctx.ContextItem;
                        var savedPosition = ctx.ContextPosition;
                        var savedSize = ctx.ContextSize;
                        try
                        {
                            ctx.WithFocus(initialItem, initialPosition, initialSize);
                            return EvaluateWithModuleContext(v, ctx);
                        }
                        // Errors raised while evaluating a global variable initializer are
                        // not caught by try/catch expressions (XQuery try-006/007); mark
                        // them so the VM's TryCatch opcode lets them propagate.
                        catch (Exception ex) when (ex is not GlobalVariableEvaluationException)
                        {
                            throw new GlobalVariableEvaluationException(ex);
                        }
                        finally
                        {
                            ctx.WithFocus(savedItem, savedPosition, savedSize);
                            inFlight.Remove((local, ns));
                        }
                    }
                }
                return previousResolver?.Invoke(local, ns);
            };
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

    // Invokes a user function body, applying the declaring library module's runtime static
    // context (namespaces, base URI, default element namespace, default collation) when the
    // function was declared in a library module (cbcl-module-002: module-local base URIs).
    private static XdmValue InvokeWithModuleContext(CompiledUserFunction function, EvaluationContext callCtx, ReadOnlySpan<XdmValue> args)
    {
        if (function.ModuleNamespaces is null)
        {
            return VmEngine.InvokeFunctionItem(
                new InlineFunctionItem(function.Parameters, function.Body, function.ParameterTypes, function.ReturnType),
                callCtx, args);
        }
        var snapshot = callCtx.SnapshotNamespaces();
        var savedDefaultNs = callCtx.DefaultElementNamespace;
        var savedBaseUri = callCtx.BaseUri;
        var savedCollation = callCtx.DefaultCollation;
        try
        {
            ApplyModuleContext(callCtx, function.ModuleNamespaces, function.ModuleBaseUri,
                function.ModuleDefaultElementNamespace, function.ModuleDefaultCollation);
            return VmEngine.InvokeFunctionItem(
                new InlineFunctionItem(function.Parameters, function.Body, function.ParameterTypes, function.ReturnType),
                callCtx, args);
        }
        finally
        {
            callCtx.RestoreNamespaces(snapshot);
            callCtx.DefaultElementNamespace = savedDefaultNs;
            callCtx.BaseUri = savedBaseUri;
            callCtx.DefaultCollation = savedCollation;
        }
    }

    // Evaluates a global variable initializer with the declaring module's runtime context.
    private static XdmValue EvaluateWithModuleContext(CompiledUserVariable variable, EvaluationContext ctx)
    {
        if (variable.ModuleNamespaces is null)
            return VmEngine.Execute(variable.Body!, ctx);
        var snapshot = ctx.SnapshotNamespaces();
        var savedDefaultNs = ctx.DefaultElementNamespace;
        var savedBaseUri = ctx.BaseUri;
        var savedCollation = ctx.DefaultCollation;
        try
        {
            ApplyModuleContext(ctx, variable.ModuleNamespaces, variable.ModuleBaseUri,
                variable.ModuleDefaultElementNamespace, variable.ModuleDefaultCollation);
            return VmEngine.Execute(variable.Body!, ctx);
        }
        finally
        {
            ctx.RestoreNamespaces(snapshot);
            ctx.DefaultElementNamespace = savedDefaultNs;
            ctx.BaseUri = savedBaseUri;
            ctx.DefaultCollation = savedCollation;
        }
    }

    private static void ApplyModuleContext(
        EvaluationContext ctx,
        IReadOnlyDictionary<string, string> namespaces,
        string? baseUri,
        string? defaultElementNamespace,
        string? defaultCollation)
    {
        foreach (var (prefix, nsUri) in namespaces)
        {
            if (!string.IsNullOrEmpty(prefix))
                ctx.WithNamespace(prefix, nsUri);
        }
        if (!string.IsNullOrEmpty(baseUri))
            ctx.BaseUri = baseUri;
        if (!string.IsNullOrEmpty(defaultElementNamespace))
            ctx.DefaultElementNamespace = defaultElementNamespace;
        if (!string.IsNullOrEmpty(defaultCollation))
            ctx.DefaultCollation = defaultCollation;
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
