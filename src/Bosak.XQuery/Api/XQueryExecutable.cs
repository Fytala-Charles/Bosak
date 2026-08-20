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
//                      | Charles Korthout | 1.7   | 29-07-2026     | Typed external-variable binding check (XPTY0004); undeclared prefixes unbound at runtime |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.8   | 01-08-2026     | fn:load-xquery-module registered; module sources seeded onto the evaluation context |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.9   | 01-08-2026     | Decimal formats applied; module-local decimal formats in module runtime contexts    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.0   | 07-08-2026     | Relative/empty declared base-uri resolved against ambient static base URI (K2-BaseURIProlog-4) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.1   | 07-08-2026     | Initial context item validated against imported library modules' context item type declarations (XPTY0004, contextDecl-054) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.2   | 07-08-2026     | Evaluation-time check of statically unresolvable names (XPST0008/XPST0081/XPST0017)   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.3   | 18-08-2026     | Main-module function bodies use the main module's static default element namespace (extvardeclwithtype-23) |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Schema;
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
    string? ModuleDefaultCollation = null,
    IReadOnlyDictionary<(string LocalName, string NamespaceUri), DecimalFormat>? ModuleDecimalFormats = null,
    DecimalFormat? ModuleDefaultDecimalFormat = null);

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
    string? ModuleDefaultCollation = null,
    IReadOnlyDictionary<(string LocalName, string NamespaceUri), DecimalFormat>? ModuleDecimalFormats = null,
    DecimalFormat? ModuleDefaultDecimalFormat = null);

/// <summary>
/// A compiled, thread-safe XQuery that can be evaluated against a context document.
/// </summary>
public sealed class XQueryExecutable
{
    private readonly IrModule _module;
    private readonly XQueryStaticContext _staticContext;
    private readonly IReadOnlyList<CompiledUserFunction> _userFunctions;
    private readonly IReadOnlyList<CompiledUserVariable> _userVariables;
    private readonly IReadOnlyList<XQueryModuleSource> _moduleSources;
    private readonly IReadOnlyList<string> _libraryModuleContextItemTypes;
    private readonly ModuleVisibilityValidator.UnresolvedNameReferences? _unresolvedNames;

    internal XQueryExecutable(
        IrModule module,
        XQueryStaticContext staticContext,
        IReadOnlyList<CompiledUserFunction>? userFunctions = null,
        IReadOnlyList<CompiledUserVariable>? userVariables = null,
        IReadOnlyList<XQueryModuleSource>? moduleSources = null,
        IReadOnlyList<string>? libraryModuleContextItemTypes = null,
        ModuleVisibilityValidator.UnresolvedNameReferences? unresolvedNames = null)
    {
        _module = module;
        _staticContext = staticContext;
        _userFunctions = userFunctions ?? [];
        _userVariables = userVariables ?? [];
        _moduleSources = moduleSources ?? [];
        _libraryModuleContextItemTypes = libraryModuleContextItemTypes ?? [];
        _unresolvedNames = unresolvedNames is { IsEmpty: true } ? null : unresolvedNames;
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

            // XQuery 3.1 §4.14: the initial context item must satisfy the context item
            // type declared by every imported library module (contextDecl-054).
            if (!evaluationContext.ContextItem.IsUndefined)
            {
                foreach (var contextItemType in _libraryModuleContextItemTypes)
                {
                    if (!VmEngine.ValueMatchesType(evaluationContext.ContextItem, contextItemType, evaluationContext))
                        throw new InvalidOperationException(
                            $"XPTY0004: The initial context item does not match the context item type '{contextItemType}' declared in an imported library module.");
                }
            }

            ValidateUnresolvedNames(evaluationContext);

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

    // Names that the compiler could not resolve statically (undeclared variables, undeclared
    // variable-name prefixes, unknown local-namespace function calls) are re-checked here,
    // where externally supplied variables and registered functions are visible. Static errors
    // must surface even when the offending code path is never executed (errors-and-optimization-7,
    // K-FunctionProlog-37/38, K2-FunctionProlog-38, K-LetExprWithout-1).
    private void ValidateUnresolvedNames(EvaluationContext ctx)
    {
        if (_unresolvedNames is null)
            return;
        foreach (var (prefix, local) in _unresolvedNames.PrefixedVariables)
        {
            if (!ctx.TryResolveNamespace(prefix, out var ns))
                throw new InvalidOperationException($"XPST0081: Prefix '{prefix}' is not declared.");
            if (!ctx.TryGetBoundVariable(local, out _, ns))
                throw new InvalidOperationException($"XPST0008: Variable ${prefix}:{local} is not defined.");
        }
        foreach (var (local, ns, display) in _unresolvedNames.Variables)
        {
            if (!ctx.TryGetBoundVariable(local, out _, ns))
                throw new InvalidOperationException($"XPST0008: Variable ${display} is not defined.");
        }
        foreach (var (local, arity) in _unresolvedNames.LocalFunctions)
        {
            if (!ctx.TryResolveFunction("http://www.w3.org/2005/xquery-local-functions", local, arity, out _))
                throw new InvalidOperationException($"XPST0017: Function {{http://www.w3.org/2005/xquery-local-functions}}{local}#{arity} not found.");
        }
    }

    /// <summary>
    /// Builds a compiled <see cref="XmlSchemaSet"/> from the schema imports declared in the
    /// prolog, using the supplied resolver to fetch each schema document.
    /// </summary>
    private static XmlSchemaSet BuildSchemaSet(IReadOnlyList<SchemaImport> imports, Func<string, IReadOnlyList<string>, System.IO.Stream?>? resolver)
    {
        var schemaSet = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
        var errors = new List<string>();
        schemaSet.ValidationEventHandler += (sender, e) =>
        {
            if (e.Severity == XmlSeverityType.Error)
                errors.Add(e.Message);
        };

        foreach (var import in imports)
        {
            string targetNs = import.NamespaceUri ?? "";
            var hints = import.LocationHints;
            System.IO.Stream? stream = null;
            string? resolvedLocation = null;

            if (resolver is not null)
            {
                stream = resolver(targetNs, hints);
            }

            if (stream is null && hints.Count > 0)
            {
                foreach (var hint in hints)
                {
                    if (Uri.TryCreate(hint, UriKind.Absolute, out var uri) && uri.Scheme != "file")
                    {
                        // Only file:// hints are supported in this first iteration.
                        continue;
                    }
                    string path = uri?.LocalPath ?? hint;
                    if (System.IO.File.Exists(path))
                    {
                        stream = System.IO.File.OpenRead(path);
                        resolvedLocation = path;
                        break;
                    }
                }
            }

            if (stream is null)
            {
                throw new InvalidOperationException($"XQST0059: Unable to locate schema for namespace '{targetNs}'.");
            }

            using (stream)
            {
                using var reader = System.Xml.XmlReader.Create(stream);
                var schema = XmlSchema.Read(reader, null);
                if (schema is null)
                    throw new InvalidOperationException($"XQST0059: Unable to read schema for namespace '{targetNs}'.");
                schemaSet.Add(schema);
            }
        }

        schemaSet.Compile();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Schema compilation failed:\n{string.Join("\n", errors)}");
        }
        return schemaSet;
    }

    private void ApplyStaticContext(EvaluationContext ctx)
    {
        // Populate the standard function library once per execution context.
        if (!ctx.SkipStandardFunctionPopulation)
            FunctionLibrary.Populate(ctx);

        // fn:load-xquery-module is an XQuery-only function: it is registered here (not in
        // the shared XPath library) and resolves modules through the sources registered
        // with the compiler (XQueryCompiler.WithModule), seeded onto the context.
        ctx.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "load-xquery-module",
            Arity = 1,
            ParameterTypes = [XdmValueKind.String],
            ReturnType = XdmValueKind.Map,
            Implementation = XQueryModuleLoader.Load1
        });
        ctx.RegisterFunction(new FunctionSignature
        {
            NamespaceUri = "http://www.w3.org/2005/xpath-functions",
            LocalName = "load-xquery-module",
            Arity = 2,
            ParameterTypes = [XdmValueKind.String, XdmValueKind.Map],
            ReturnType = XdmValueKind.Map,
            Implementation = XQueryModuleLoader.Load2
        });
        foreach (var source in _moduleSources)
        {
            if (!ctx.XQueryModuleSources.TryGetValue(source.Uri, out var candidates))
                ctx.XQueryModuleSources[source.Uri] = new List<(string?, string)> { (source.Location, source.Source) };
            else if (!candidates.Any(c => c.Source == source.Source && c.Location == source.Location))
                candidates.Add((source.Location, source.Source));
        }

        if (!string.IsNullOrEmpty(_staticContext.DefaultElementNamespace))
            ctx.DefaultElementNamespace = _staticContext.DefaultElementNamespace;

        if (_staticContext.BaseUri is not null)
        {
            // XQuery 3.1 §4.5: a relative URILiteral in the base URI declaration is made
            // absolute by resolving it against the ambient static base URI (the one already
            // present on the evaluation context). An empty URILiteral therefore inherits
            // the ambient base URI (K2-BaseURIProlog-4/5).
            string declared = _staticContext.BaseUri;
            if (!FunctionLibrary.IsAbsoluteUri(declared)
                && Uri.TryCreate(ctx.BaseUri, UriKind.Absolute, out var ambientBase)
                && Uri.TryCreate(ambientBase, declared, out var resolvedBase))
            {
                ctx.BaseUri = resolvedBase.ToString();
            }
            else if (declared.Length > 0)
            {
                ctx.BaseUri = declared;
            }
        }

        if (!string.IsNullOrEmpty(_staticContext.DefaultCollation))
            ctx.DefaultCollation = _staticContext.DefaultCollation;

        foreach (var (prefix, nsUri) in _staticContext.Namespaces)
        {
            if (!string.IsNullOrEmpty(prefix))
                ctx.WithNamespace(prefix, nsUri);
        }

        // Namespace undeclarations (declare namespace p = "") must also unbind the
        // predeclared bindings of the runtime context (K2-NamespaceProlog-4/9).
        foreach (var prefix in _staticContext.UndeclaredPrefixes)
            ctx.RemoveNamespace(prefix);

        // Load schemas imported by the prolog. If the execution context already carries a
        // schema set (e.g. supplied by the test harness), use it; otherwise build one
        // from the imported schema declarations via the registered resolver.
        if (ctx.SchemaSet is null && _staticContext.ImportedSchemas.Count > 0)
        {
            ctx.SchemaSet = BuildSchemaSet(_staticContext.ImportedSchemas, ctx.SchemaResolver);
        }

        foreach (var ((localName, nsUri), value) in _staticContext.Variables)
        {
            ctx.WithVariable(localName, value, nsUri);
        }

        // User-declared functions dispatch through a compiled InlineFunctionItem body.
        // Main-module function bodies use the main module's static default element
        // namespace, not a namespace leaked from an enclosing direct element constructor
        // (extvardeclwithtype-23).
        var mainModuleDefaultElementNamespace = _staticContext.DefaultElementNamespace;
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
                Implementation = (callCtx, args) => InvokeWithModuleContext(captured, callCtx, args, mainModuleDefaultElementNamespace)
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

        // External variables with a declared type: the supplied value is checked strictly
        // (atomization plus instance-of, no casts or promotions) — XPTY0004 on mismatch
        // (extvardeclwithtype-19). Unbound externals resolve to XPST0008 on reference.
        foreach (var v in _userVariables)
        {
            if (!v.IsExternal || v.TypeName is null)
                continue;
            if (!ctx.TryGetVariable(v.LocalName, out var externalValue, v.NamespaceUri))
                continue;
            var valueToCheck = IsNodeKindTestText(v.TypeName) ? externalValue : AtomizeItemsForTypeCheck(externalValue);
            if (!VmEngine.ValueMatchesType(valueToCheck, v.TypeName, ctx))
                throw new InvalidOperationException($"XPTY0004: The value of the external variable '${v.LocalName}' does not match the declared type '{v.TypeName}'.");
        }

        // Decimal-format declarations (declare (default )?decimal-format) feed
        // fn:format-number's named and default formats.
        foreach (var ((localName, nsUri), decimalFormat) in _staticContext.DecimalFormats)
            ctx.WithDecimalFormat(localName, nsUri, decimalFormat);
        if (_staticContext.DeclaredDefaultDecimalFormat is not null)
            ctx.DefaultDecimalFormat = _staticContext.DeclaredDefaultDecimalFormat;

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
    private static XdmValue InvokeWithModuleContext(CompiledUserFunction function, EvaluationContext callCtx, ReadOnlySpan<XdmValue> args, string? mainModuleDefaultElementNamespace)
    {
        if (function.ModuleNamespaces is null)
        {
            // Main-module function: evaluate the body with the main module's static default
            // element namespace, so an enclosing direct element constructor's default
            // namespace does not leak into the function body's type tests and name
            // resolution (extvardeclwithtype-23).
            var savedMainDefaultNs = callCtx.DefaultElementNamespace;
            try
            {
                callCtx.DefaultElementNamespace = mainModuleDefaultElementNamespace;
                return VmEngine.InvokeFunctionItem(
                    new InlineFunctionItem(function.Parameters, function.Body, function.ParameterTypes, function.ReturnType),
                    callCtx, args);
            }
            finally
            {
                callCtx.DefaultElementNamespace = savedMainDefaultNs;
            }
        }
        var snapshot = callCtx.SnapshotNamespaces();
        var savedDefaultNs = callCtx.DefaultElementNamespace;
        var savedBaseUri = callCtx.BaseUri;
        var savedCollation = callCtx.DefaultCollation;
        var savedDefaultFormat = callCtx.DefaultDecimalFormat;
        var savedFormats = callCtx.SnapshotDecimalFormats();
        try
        {
            ApplyModuleContext(callCtx, function.ModuleNamespaces, function.ModuleBaseUri,
                function.ModuleDefaultElementNamespace, function.ModuleDefaultCollation,
                function.ModuleDecimalFormats, function.ModuleDefaultDecimalFormat);
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
            callCtx.DefaultDecimalFormat = savedDefaultFormat;
            callCtx.RestoreDecimalFormats(savedFormats);
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
        var savedDefaultFormat = ctx.DefaultDecimalFormat;
        var savedFormats = ctx.SnapshotDecimalFormats();
        try
        {
            ApplyModuleContext(ctx, variable.ModuleNamespaces, variable.ModuleBaseUri,
                variable.ModuleDefaultElementNamespace, variable.ModuleDefaultCollation,
                variable.ModuleDecimalFormats, variable.ModuleDefaultDecimalFormat);
            return VmEngine.Execute(variable.Body!, ctx);
        }
        finally
        {
            ctx.RestoreNamespaces(snapshot);
            ctx.DefaultElementNamespace = savedDefaultNs;
            ctx.BaseUri = savedBaseUri;
            ctx.DefaultCollation = savedCollation;
            ctx.DefaultDecimalFormat = savedDefaultFormat;
            ctx.RestoreDecimalFormats(savedFormats);
        }
    }

    private static void ApplyModuleContext(
        EvaluationContext ctx,
        IReadOnlyDictionary<string, string> namespaces,
        string? baseUri,
        string? defaultElementNamespace,
        string? defaultCollation,
        IReadOnlyDictionary<(string LocalName, string NamespaceUri), DecimalFormat>? decimalFormats = null,
        DecimalFormat? defaultDecimalFormat = null)
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
        // The declaring module's decimal-format declarations apply within its bodies
        // (decimal-format-21: a library module's format is used by its format-number calls).
        if (decimalFormats is not null)
            foreach (var ((localName, nsUri), fmt) in decimalFormats)
                ctx.WithDecimalFormat(localName, nsUri, fmt);
        if (defaultDecimalFormat is not null)
            ctx.DefaultDecimalFormat = defaultDecimalFormat;
    }

    // Atomizes node items in an external variable's value for strict type checking
    // (XQuery 3.1 §4.16: nodes become xs:untypedAtomic; atomic values pass through).
    private static XdmValue AtomizeItemsForTypeCheck(XdmValue value)
    {
        if (value.IsNode)
            return XdmValue.FromString(value.NodeValue.StringValue, "untypedAtomic");
        if (!value.IsSequence || value.SequenceValue is null)
            return value;
        var items = new List<XdmValue>();
        bool anyNode = false;
        foreach (var item in XdmSequence.FromSource(value.SequenceValue))
        {
            if (item.IsNode)
                anyNode = true;
            items.Add(item.IsNode ? XdmValue.FromString(item.NodeValue.StringValue, "untypedAtomic") : item);
        }
        return anyNode ? XdmValue.FromSequence(MaterializedSequence.FromList(items)) : value;
    }

    // True when a sequence-type text is a node kind test (matched on nodes, no atomization).
    private static bool IsNodeKindTestText(string typeName)
    {
        var t = typeName.TrimStart();
        return t.StartsWith("element(", StringComparison.Ordinal)
            || t.StartsWith("attribute(", StringComparison.Ordinal)
            || t.StartsWith("document-node(", StringComparison.Ordinal)
            || t.StartsWith("comment(", StringComparison.Ordinal)
            || t.StartsWith("text(", StringComparison.Ordinal)
            || t.StartsWith("processing-instruction(", StringComparison.Ordinal)
            || t.StartsWith("namespace-node(", StringComparison.Ordinal)
            || t.StartsWith("schema-element(", StringComparison.Ordinal)
            || t.StartsWith("schema-attribute(", StringComparison.Ordinal)
            || t.StartsWith("node(", StringComparison.Ordinal);
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
