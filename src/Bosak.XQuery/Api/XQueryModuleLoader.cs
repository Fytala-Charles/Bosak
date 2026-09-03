// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 August 2026
// PURPOSE              : Implements fn:load-xquery-module — dynamic XQuery library-module loading.
// SPECIAL NOTES        : Part of the Bosak XQuery 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 01-08-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 22-08-2026     | Inherit caller schema set and compile schemas imported by loaded modules (fn:load-xquery-module schema propagation) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 02-09-2026     | XQST0059 (not FOQM0002) for an unresolvable relative module URI (xslt30 load-xquery-module-001) |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Functions;
using Bosak.XPath.Runtime.Vm;
using Bosak.XQuery.Compiler;

namespace Bosak.XQuery.Api;

/// <summary>
/// Implements <c>fn:load-xquery-module($href)</c> and <c>fn:load-xquery-module($href, $options)</c>
/// (F&amp;O 3.1 §15.3.1): resolves a library module by URI, compiles it together with its
/// transitive imports, binds external variables and the context item from the options map,
/// and returns <c>map { "variables": map { QName → value }, "functions": map { QName → map { arity → function } } }</c>
/// containing the module's public declarations only.
/// </summary>
public static class XQueryModuleLoader
{
    private const string FnNamespace = "http://www.w3.org/2005/xpath-functions";

    /// <summary>Implements <c>fn:load-xquery-module($href)</c>.</summary>
    public static XdmValue Load1(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Load(ctx, args[0], XdmValue.Undefined);

    /// <summary>Implements <c>fn:load-xquery-module($href, $options)</c>.</summary>
    public static XdmValue Load2(EvaluationContext ctx, ReadOnlySpan<XdmValue> args)
        => Load(ctx, args[0], args[1]);

    private static XdmValue Load(EvaluationContext ctx, XdmValue hrefArg, XdmValue optionsArg)
    {
        // FOQM0001: an empty module URI (fn-load-xquery-module-001/002).
        string href = AtomizedString(hrefArg).Trim();
        if (href.Length == 0)
            throw new InvalidOperationException("FOQM0001: The module URI is empty.");
        href = XQueryParser.NormalizeModuleUri(href);

        var options = ParseOptions(optionsArg);

        // FOQM0006: the requested XQuery version is not supported (fn-load-xquery-module-915).
        if (options.XQueryVersion is not null
            && options.XQueryVersion is not ("1.0" or "3.0" or "3.1"))
            throw new InvalidOperationException($"FOQM0006: XQuery version '{options.XQueryVersion}' is not supported.");

        // Resolve the target module source: registered sources first (location hints
        // select among candidates), then the filesystem fallback.
        var target = ResolveSource(ctx, href, options.LocationHints)
            ?? throw new InvalidOperationException(UnresolvedModuleError(href));

        XQueryParseResult targetParse;
        try
        {
            targetParse = XQueryParser.Parse(target.Source);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"FOQM0003: The module '{href}' is not a valid XQuery library module: {ex.Message}");
        }
        // The resolved source must be a library module for the requested URI (FOQM0003).
        if (!targetParse.IsLibraryModule || targetParse.StaticContext.ModuleNamespaceUri != href)
            throw new InvalidOperationException($"FOQM0003: The module '{href}' is not a valid XQuery library module.");

        // FOQM0005: a supplied context item that does not match the module's declared
        // context item type (fn-load-xquery-module-060).
        if (options.ContextItem is not null && targetParse.StaticContext.ContextItemTypeName is not null
            && !VmEngine.ValueMatchesType(options.ContextItem.Value, targetParse.StaticContext.ContextItemTypeName, ctx))
            throw new InvalidOperationException(
                $"FOQM0005: The supplied context item does not match the declared type '{targetParse.StaticContext.ContextItemTypeName}'.");

        // Resolve the transitive import closure through the same resolver chain.
        var resolved = new Dictionary<string, List<(string? Location, string Source)>>(StringComparer.Ordinal);
        var loading = new HashSet<string>(StringComparer.Ordinal);
        ResolveImports(targetParse, ctx, resolved, loading);
        resolved.TryAdd(href, new List<(string?, string)>());
        resolved[href].Add((target.Location, target.Source));

        // Compile the module with a synthetic importing main module so the existing
        // library-module pipeline compiles every declaration in the closure.
        var compiler = new XQueryCompiler();
        foreach (var (ns, sources) in resolved)
            foreach (var (location, source) in sources)
                compiler.WithModule(ns, source, location);
        var executable = compiler.Compile($"import module namespace __lxm = \"{href}\"; ()");

        // Bind the execution context: module sources for nested loads, the option's
        // context item, and external-variable values (external declarations only;
        // entries for other variables are silently ignored — fn-load-xquery-module-021).
        var xqCtx = new XQueryContext();
        var evalCtx = xqCtx.EvaluationContext;
        foreach (var (ns, sources) in ctx.XQueryModuleSources)
            evalCtx.XQueryModuleSources[ns] = sources;
        foreach (var (ns, sources) in resolved)
        {
            if (!evalCtx.XQueryModuleSources.TryGetValue(ns, out var existing))
                evalCtx.XQueryModuleSources[ns] = new List<(string?, string)>(sources);
            else
                existing.AddRange(sources.Where(s => !existing.Any(e => e.Source == s.Source)));
        }

        // Schema imports declared by the loaded library module (and its transitive
        // imports) must be compiled into the module's own evaluation context, and the
        // caller's schema resolver/schema set must be inherited so that schema-aware
        // operations inside the module work (fn-load-xquery-module-050/051/052/056).
        evalCtx.SchemaResolver = ctx.SchemaResolver;
        var loadedSchemas = CollectSchemaImports(targetParse, resolved);
        if (loadedSchemas.Count > 0 || ctx.SchemaSet is not null)
            evalCtx.SchemaSet = XQueryExecutable.BuildSchemaSet(loadedSchemas, evalCtx, ctx.SchemaSet);

        if (options.ContextItem is not null)
            evalCtx.WithFocus(options.ContextItem.Value, 1, 1);

        var externalDecls = CollectExternalDeclarations(targetParse, resolved);
        foreach (var (qname, value) in options.Variables)
        {
            var decl = externalDecls.FirstOrDefault(d =>
                d.LocalName == qname.LocalName && d.NamespaceUri == qname.NamespaceUri);
            if (decl is null)
                continue;
            // FOQM0005: a supplied value that does not match the declared type
            // (fn-load-xquery-module-011).
            if (decl.TypeName is not null && !MatchesDeclaredType(value, decl.TypeName, evalCtx))
                throw new InvalidOperationException(
                    $"FOQM0005: The value supplied for external variable '${decl.LocalName}' does not match the declared type '{decl.TypeName}'.");
            evalCtx.WithVariable(decl.LocalName, value, decl.NamespaceUri);
        }

        // Evaluate the wrapper (trivial body) so the library functions and the lazy
        // variable resolver are registered on the context.
        executable.Evaluate(xqCtx);

        // Collect the target module's public variables, eagerly evaluated with the
        // module's own runtime context (fn-load-xquery-module-013: initializer errors
        // propagate; -007: an unsupplied external variable is XPDY0002).
        var varsMap = new XdmMap();
        foreach (var decl in targetParse.StaticContext.UserVariables.Where(v => !v.IsPrivate))
        {
            XdmValue value;
            if (decl.IsExternal)
            {
                if (evalCtx.TryGetVariable(decl.LocalName, out var supplied, decl.NamespaceUri))
                {
                    value = supplied;
                }
                else if (decl.Body is not null)
                {
                    value = ResolveModuleVariable(evalCtx, decl);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"XPDY0002: No value was supplied for external variable '${decl.LocalName}'.");
                }
            }
            else
            {
                value = ResolveModuleVariable(evalCtx, decl);
            }
            varsMap = varsMap.WithAdded(
                XdmValue.FromQName(new XsQName(decl.LocalName, decl.NamespaceUri)), value);
        }

        // Collect the target module's public functions as (arity → function item) maps.
        // Function items invoke against the module's own evaluation context (its lazy
        // variable resolver, bound externals, and option context item), not the caller's
        // (fn-load-xquery-module-016/028: bodies referencing module variables resolve).
        var funcsMap = new XdmMap();
        foreach (var decl in targetParse.StaticContext.UserFunctions.Where(f => !f.IsPrivate))
        {
            int arity = decl.Parameters.Count;
            if (!evalCtx.TryResolveFunction(decl.NamespaceUri, decl.LocalName, arity, out var sig))
                continue;
            var implementation = sig.Implementation;
            var key = XdmValue.FromQName(new XsQName(decl.LocalName, decl.NamespaceUri));
            var arityMap = funcsMap.TryGetValue(key, out var existing) && existing.IsMap
                ? existing.MapValue
                : new XdmMap();
            arityMap = arityMap.WithAdded(
                XdmValue.FromInteger(arity),
                XdmValue.FromFunction(new DelegateFunctionItem(arity,
                    (callCtx, callArgs) => implementation(evalCtx, callArgs))));
            funcsMap = funcsMap.WithAdded(key, XdmValue.FromMap(arityMap));
        }

        return XdmValue.FromMap(new XdmMap()
            .WithAdded(XdmValue.FromString("variables"), XdmValue.FromMap(varsMap))
            .WithAdded(XdmValue.FromString("functions"), XdmValue.FromMap(funcsMap)));
    }

    /// <summary>
    /// The error for an unresolvable module URI: FOQM0002 for an absolute URI
    /// (fn-load-xquery-module-003/005), XQST0059 for a relative URI — the XSLT 3.0
    /// test load-xquery-module-001 accepts either XQST0059 or FOQM0006 for a
    /// relative href that cannot be resolved.
    /// </summary>
    private static string UnresolvedModuleError(string href)
        => Bosak.XPath.Standard.Functions.FunctionLibrary.IsAbsoluteUri(href)
            ? $"FOQM0002: Unable to resolve the module URI '{href}'."
            : $"XQST0059: Unable to resolve the module URI '{href}'.";

    /// <summary>
    /// Resolves a module variable through the lazy resolver registered by the wrapper
    /// evaluation, unwrapping the global-variable error marker so the original error
    /// (for example FOAR0001 or XPDY0002) surfaces unchanged.
    /// </summary>
    private static XdmValue ResolveModuleVariable(EvaluationContext evalCtx, UserVariableDeclaration decl)
    {
        if (evalCtx.LazyVariableResolver is null)
            throw new InvalidOperationException(
                $"XPDY0002: No value was supplied for variable '${decl.LocalName}'.");
        try
        {
            return evalCtx.LazyVariableResolver(decl.LocalName, decl.NamespaceUri) ?? XdmValue.Undefined;
        }
        catch (GlobalVariableEvaluationException gve) when (gve.InnerException is not null)
        {
            throw gve.InnerException;
        }
    }

    /// <summary>
    /// Recursively resolves the import closure of a parsed module through the resolver
    /// chain. Import cycles terminate through the in-progress set, mirroring the static
    /// compiler's import loader. An unresolvable import raises FOQM0003.
    /// </summary>
    private static void ResolveImports(
        XQueryParseResult module,
        EvaluationContext ctx,
        Dictionary<string, List<(string? Location, string Source)>> resolved,
        HashSet<string> loading)
    {
        foreach (var import in module.StaticContext.ImportedModules)
        {
            var ns = import.NamespaceUri;
            if (resolved.ContainsKey(ns) || !loading.Add(ns))
                continue;
            var source = ResolveSource(ctx, ns, import.LocationHints)
                ?? throw new InvalidOperationException($"FOQM0003: Unable to resolve the imported module URI '{ns}'.");
            XQueryParseResult importedParse;
            try
            {
                importedParse = XQueryParser.Parse(source.Source);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"FOQM0003: The imported module '{ns}' is not a valid XQuery library module: {ex.Message}");
            }
            if (!importedParse.IsLibraryModule || importedParse.StaticContext.ModuleNamespaceUri != ns)
                throw new InvalidOperationException($"FOQM0003: The imported module '{ns}' is not a valid XQuery library module.");
            resolved[ns] = new List<(string?, string)> { (source.Location, source.Source) };
            ResolveImports(importedParse, ctx, resolved, loading);
        }
    }

    /// <summary>
    /// Resolves a module URI to its source text: registered
    /// <see cref="EvaluationContext.XQueryModuleSources"/> candidates first (location
    /// hints select among them), then a filesystem fallback that treats the URI as a
    /// rooted path or as relative to the static base URI.
    /// </summary>
    private static (string Source, string? Location)? ResolveSource(
        EvaluationContext ctx, string uri, IReadOnlyList<string> locationHints)
    {
        if (ctx.XQueryModuleSources.TryGetValue(uri, out var candidates))
        {
            var filtered = candidates;
            if (locationHints.Count > 0)
            {
                var hinted = filtered
                    .Where(c => c.Location is not null && locationHints.Contains(c.Location, StringComparer.Ordinal))
                    .ToList();
                if (hinted.Count > 0)
                    filtered = hinted;
            }
            if (filtered.Count > 0)
                return (filtered[0].Source, filtered[0].Location);
        }

        string? path = null;
        try
        {
            if (Path.IsPathRooted(uri))
            {
                path = uri;
            }
            else if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute) && !string.IsNullOrEmpty(ctx.BaseUri))
            {
                var basePath = Uri.IsWellFormedUriString(ctx.BaseUri, UriKind.Absolute) && new Uri(ctx.BaseUri).IsFile
                    ? new Uri(ctx.BaseUri).LocalPath
                    : ctx.BaseUri;
                if (!string.IsNullOrEmpty(basePath))
                    path = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(basePath)) ?? "", uri);
            }
        }
        catch (Exception) { /* fall through to FOQM0002 */ }

        if (path is not null && File.Exists(path))
            return (File.ReadAllText(path), null);
        return null;
    }

    /// <summary>
    /// Collects the external variable declarations of the target module and of every
    /// module in its import closure (fn-load-xquery-module-026: values may be supplied
    /// for externals declared in transitively-imported modules).
    /// </summary>
    private static List<UserVariableDeclaration> CollectExternalDeclarations(
        XQueryParseResult targetParse,
        Dictionary<string, List<(string? Location, string Source)>> resolved)
    {
        var decls = new List<UserVariableDeclaration>();
        foreach (var v in targetParse.StaticContext.UserVariables)
        {
            if (v.IsExternal)
                decls.Add(v);
        }
        foreach (var (_, sources) in resolved)
        {
            foreach (var (_, source) in sources)
            {
                XQueryParseResult parse;
                try
                {
                    parse = XQueryParser.Parse(source);
                }
                catch (Exception)
                {
                    continue;
                }
                foreach (var v in parse.StaticContext.UserVariables)
                {
                    if (v.IsExternal)
                        decls.Add(v);
                }
            }
        }
        return decls;
    }

    /// <summary>
    /// Collects the schema-import declarations of the target module and of every module in
    /// its import closure, de-duplicated by target namespace.
    /// </summary>
    private static List<SchemaImport> CollectSchemaImports(
        XQueryParseResult targetParse,
        Dictionary<string, List<(string? Location, string Source)>> resolved)
    {
        var imports = new List<SchemaImport>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        void AddImports(IEnumerable<SchemaImport> list)
        {
            foreach (var import in list)
            {
                string ns = import.NamespaceUri ?? "";
                if (seen.Add(ns))
                    imports.Add(import);
            }
        }

        AddImports(targetParse.StaticContext.ImportedSchemas);
        foreach (var (_, sources) in resolved)
        {
            foreach (var (_, source) in sources)
            {
                XQueryParseResult parse;
                try
                {
                    parse = XQueryParser.Parse(source);
                }
                catch (Exception)
                {
                    continue;
                }
                AddImports(parse.StaticContext.ImportedSchemas);
            }
        }

        return imports;
    }

    /// <summary>
    /// Checks a supplied external-variable value against the declared sequence type:
    /// nodes are atomized for non-node-kind types (mirroring the engine's typed-external
    /// binding rules).
    /// </summary>
    private static bool MatchesDeclaredType(XdmValue value, string typeName, EvaluationContext ctx)
    {
        bool nodeKind = typeName.Contains('(');
        if (!nodeKind && value.IsNode)
            value = XdmValue.FromString(value.NodeValue.StringValue, "untypedAtomic");
        return VmEngine.ValueMatchesType(value, typeName, ctx);
    }

    private static LoadOptions ParseOptions(XdmValue optionsArg)
    {
        var options = new LoadOptions();
        if (optionsArg.IsUndefined)
            return options;
        if (!optionsArg.IsMap)
            throw new InvalidOperationException("XPTY0004: The $options argument must be a map.");

        foreach (var entry in optionsArg.MapValue.Entries)
        {
            string key = AtomizedString(entry.Key);
            switch (key)
            {
                case "variables":
                    // XPTY0004: the variables value must be a map with QName keys
                    // (fn-load-xquery-module-062/063).
                    if (!entry.Value.IsMap)
                        throw new InvalidOperationException("XPTY0004: The 'variables' option value must be a map.");
                    foreach (var varEntry in entry.Value.MapValue.Entries)
                    {
                        if (varEntry.Key.Kind != XdmValueKind.QName)
                            throw new InvalidOperationException("XPTY0004: The 'variables' option map keys must be xs:QName values.");
                        options.Variables.Add((varEntry.Key.QNameValue, varEntry.Value));
                    }
                    break;
                case "context-item":
                    options.ContextItem = entry.Value;
                    break;
                case "location-hints":
                    foreach (var hint in EnumerateItems(entry.Value))
                        options.LocationHints.Add(XQueryParser.NormalizeModuleUri(AtomizedString(hint)));
                    break;
                case "xquery-version":
                    // XPTY0004: the version must be numeric (fn-load-xquery-module-072).
                    if (entry.Value.Kind is not (XdmValueKind.Double or XdmValueKind.Decimal or XdmValueKind.Integer or XdmValueKind.Float))
                        throw new InvalidOperationException("XPTY0004: The 'xquery-version' option value must be numeric.");
                    options.XQueryVersion = AtomizedString(entry.Value);
                    break;
                case "static-parameters":
                case "vendor-options":
                    // Accepted with no processing, but the value must be a map with
                    // QName keys (fn-load-xquery-module-066/067).
                    if (!entry.Value.IsMap)
                        throw new InvalidOperationException($"XPTY0004: The '{key}' option value must be a map.");
                    foreach (var vendorEntry in entry.Value.MapValue.Entries)
                    {
                        if (vendorEntry.Key.Kind != XdmValueKind.QName)
                            throw new InvalidOperationException($"XPTY0004: The '{key}' option map keys must be xs:QName values.");
                    }
                    break;
                default:
                    // Unrecognized options are ignored (fn-load-xquery-module-070).
                    break;
            }
        }
        return options;
    }

    private static IEnumerable<XdmValue> EnumerateItems(XdmValue value)
    {
        if (value.IsUndefined)
            yield break;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                yield return item;
        }
        else
        {
            yield return value;
        }
    }

    private static string AtomizedString(XdmValue value)
    {
        if (value.IsUndefined)
            return string.Empty;
        if (value.IsNode)
            return value.NodeValue.StringValue;
        return value.ToString();
    }

    private sealed class LoadOptions
    {
        public List<(XsQName Name, XdmValue Value)> Variables { get; } = new();
        public XdmValue? ContextItem { get; set; }
        public List<string> LocationHints { get; } = new();
        public string? XQueryVersion { get; set; }
    }
}
