//                      | Charles Korthout | 2.1   | 27-07-2026     | Namespace resolution traversal for multi-clause TryCatchNode |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 June 2026
// PURPOSE              : Compiles XQuery 3.1 source into an executable query plan.
// SPECIAL NOTES        : Part of the Bosak XQuery 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-06-2026     | Creation — placeholder skeleton                                                          |
//                      | Charles Korthout | 1.0   | 22-07-2026     | Wired to XPath parser, optimizer, IR lowerer, and VM                                    |
//                      | Charles Korthout | 1.1   | 22-07-2026     | Resolve function namespaces inside FlworExpressionNode clauses                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.2   | 25-07-2026     | Resolve function namespaces inside GroupByClauseNode specs                              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.3   | 25-07-2026     | Resolve function namespaces inside WindowClauseNode conditions                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.4   | 25-07-2026     | Optional window end condition                                                           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.5   | 25-07-2026     | Resolve function namespaces inside DirectElementConstructorNode parts                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.6   | 25-07-2026     | Resolve function namespaces inside computed constructors                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.7   | 25-07-2026     | Resolve function namespaces inside switch/typeswitch expressions                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.8   | 25-07-2026     | Optional XML 1.1 line-ending normalization flag threaded to the parser                  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.9   | 26-07-2026     | Compile prolog declare function/variable bodies into CompiledUserFunction/CompiledUserVariable records |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 2.0   | 27-07-2026     | Library modules: WithModule catalog, transitive import graph (XQST0059), visibility validation, per-module compilation |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Compiler.Optimizer;
using Bosak.XPath.Parser;
using Bosak.XPath.Parser.Ast;
using Bosak.XQuery.Compiler;

namespace Bosak.XQuery.Api;

/// <summary>A library module source registered with the compiler: its target namespace URI,
/// an optional location hint (matched by <c>import module ... at "..."</c>), and the source text.</summary>
public sealed record XQueryModuleSource(string Uri, string? Location, string Source);

/// <summary>
/// Compiles XQuery 3.1 source text into an <see cref="XQueryExecutable"/> that can be executed repeatedly.
/// </summary>
public sealed class XQueryCompiler
{
    private readonly List<XQueryModuleSource> _moduleSources = new();

    /// <summary>
    /// Registers a library module source that can satisfy a module import of
    /// <paramref name="uri"/> (optionally selected by a <paramref name="location"/> hint).
    /// </summary>
    /// <param name="uri">The module's target namespace URI.</param>
    /// <param name="source">The library module source text (<c>module namespace ...;</c>).</param>
    /// <param name="location">An optional location hint matched by the import's <c>at</c> clause.</param>
    /// <returns>The same compiler, for chaining.</returns>
    public XQueryCompiler WithModule(string uri, string source, string? location = null)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(source);
        _moduleSources.Add(new XQueryModuleSource(
            XQueryParser.NormalizeModuleUri(uri),
            location is null ? null : XQueryParser.NormalizeModuleUri(location),
            source));
        return this;
    }

    /// <summary>
    /// Compiles the supplied XQuery source text.
    /// </summary>
    /// <param name="query">The XQuery 3.1 source text.</param>
    /// <param name="xml11LineEndings">When true, string literals get XML 1.1 line-ending normalization.</param>
    /// <returns>An executable query plan.</returns>
    public XQueryExecutable Compile(string query, bool xml11LineEndings = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);

        // 1. Parse the XQuery module (prolog + query body).
        var parseResult = XQueryParser.Parse(query, xml11LineEndings);
        // XPST0003: a library module cannot be evaluated as a query.
        if (parseResult.IsLibraryModule)
            throw new ParseException("XPST0003: A library module ('module namespace ...') cannot be evaluated as a query.", 0);

        // 2. Load the transitive closure of imported library modules, keyed by target namespace.
        var moduleGraph = new Dictionary<string, List<XQueryParseResult>>(StringComparer.Ordinal);
        var loadedSources = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var loading = new HashSet<string>(StringComparer.Ordinal);
        foreach (var import in parseResult.StaticContext.ImportedModules)
            LoadModuleNamespace(import.NamespaceUri, import.LocationHints, moduleGraph, loadedSources, loading, xml11LineEndings);
        var moduleNamespaces = new HashSet<string>(moduleGraph.Keys, StringComparer.Ordinal);

        // 3. Resolve function-call namespaces using the static context derived from the prolog.
        var resolvedBody = ResolveFunctionNamespaces(parseResult.Body, parseResult.StaticContext);

        // 3b. Validate module-namespace references (public visibility, imports not transitive).
        var mainVisibility = BuildVisibility(parseResult.StaticContext, moduleGraph);
        ModuleVisibilityValidator.Validate(
            resolvedBody, parseResult.StaticContext, moduleNamespaces,
            mainVisibility.Functions, mainVisibility.Variables);

        // 4. Optimize the AST.
        var optimizer = new XPathOptimizer();
        var optimized = optimizer.Optimize(resolvedBody);

        // 4b. Compile user-declared function and variable bodies through the same pipeline.
        var userFunctions = new List<CompiledUserFunction>();
        var userVariables = new List<CompiledUserVariable>();
        CompileModuleDeclarations(
            parseResult.StaticContext, moduleGraph, moduleNamespaces, mainVisibility,
            optimizer, userFunctions, userVariables, moduleRuntimeContext: null);

        // 4c. Compile every loaded library module's declarations with its own static context.
        foreach (var (ns, modules) in moduleGraph)
        {
            foreach (var libModule in modules)
            {
                var libContext = libModule.StaticContext;
                var libVisibility = BuildVisibility(libContext, moduleGraph);
                var libRuntimeContext = new ModuleRuntimeContext(
                    libContext.Namespaces, libContext.BaseUri,
                    libContext.DefaultElementNamespace, libContext.DefaultCollation);
                CompileModuleDeclarations(
                    libContext, moduleGraph, moduleNamespaces, libVisibility,
                    optimizer, userFunctions, userVariables, libRuntimeContext);
            }
        }

        // 5. Lower to IR.
        var lowerer = new IrLowerer();
        var module = lowerer.Lower(optimized);

        return new XQueryExecutable(module, parseResult.StaticContext, userFunctions, userVariables);
    }

    /// <summary>The runtime static-context snapshot of a library module, applied around the
    /// execution of its function bodies and global variable initializers (null for the main module).</summary>
    internal sealed record ModuleRuntimeContext(
        IReadOnlyDictionary<string, string> Namespaces,
        string? BaseUri,
        string? DefaultElementNamespace,
        string? DefaultCollation);

    private sealed record VisibilitySet(
        HashSet<(string Ns, string Local, int Arity)> Functions,
        HashSet<(string Ns, string Local)> Variables);

    // Resolves one module import to registered sources and parses them, recursing into the
    // imports of every newly loaded module. Import cycles between modules are legal and
    // terminate through the in-progress set. Multiple modules may share one target namespace;
    // a namespace already in the graph is extended incrementally when a later import (via a
    // different route, with different location hints) names additional sources.
    private void LoadModuleNamespace(
        string ns,
        IReadOnlyList<string> locationHints,
        Dictionary<string, List<XQueryParseResult>> graph,
        Dictionary<string, HashSet<string>> loadedSources,
        HashSet<string> loading,
        bool xml11LineEndings)
    {
        if (!loading.Add(ns))
            return; // import cycle in progress; the namespace is already being loaded
        try
        {
            var candidates = _moduleSources.Where(m => m.Uri == ns).ToList();
            if (locationHints.Count > 0)
            {
                var hinted = candidates
                    .Where(m => m.Location is not null && locationHints.Contains(m.Location, StringComparer.Ordinal))
                    .ToList();
                if (hinted.Count > 0)
                    candidates = hinted;
            }

            var seen = loadedSources.TryGetValue(ns, out var existingSeen)
                ? existingSeen
                : (loadedSources[ns] = new HashSet<string>(StringComparer.Ordinal));
            var loaded = graph.TryGetValue(ns, out var existingLoaded)
                ? existingLoaded
                : (graph[ns] = new List<XQueryParseResult>());

            var added = new List<XQueryParseResult>();
            foreach (var candidate in candidates)
            {
                if (!seen.Add(candidate.Source))
                    continue;
                var parsed = XQueryParser.Parse(candidate.Source, xml11LineEndings);
                // A module whose own target namespace does not match the import is not a candidate.
                if (!parsed.IsLibraryModule || parsed.StaticContext.ModuleNamespaceUri != ns)
                {
                    seen.Remove(candidate.Source);
                    continue;
                }
                loaded.Add(parsed);
                added.Add(parsed);
            }
            // XQST0059: the import cannot be satisfied by any registered module.
            if (loaded.Count == 0)
                throw new ParseException($"XQST0059: Unable to locate a library module with target namespace '{ns}'.", 0);

            // XQST0034/XQST0049: modules sharing one target namespace must not declare the
            // same function (name+arity) or variable.
            var functionKeys = new HashSet<(string, int)>();
            var variableKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var module in loaded)
            {
                foreach (var fn in module.StaticContext.UserFunctions)
                {
                    if (!functionKeys.Add((fn.LocalName, fn.Parameters.Count)))
                        throw new ParseException($"XQST0034: Function '{fn.LocalName}' with arity {fn.Parameters.Count} is declared more than once in module namespace '{ns}'.", fn.Position);
                }
                foreach (var v in module.StaticContext.UserVariables)
                {
                    if (!variableKeys.Add(v.LocalName))
                        throw new ParseException($"XQST0049: Variable '${v.LocalName}' is declared more than once in module namespace '{ns}'.", v.Position);
                }
            }

            foreach (var module in added)
            {
                foreach (var import in module.StaticContext.ImportedModules)
                    LoadModuleNamespace(import.NamespaceUri, import.LocationHints, graph, loadedSources, loading, xml11LineEndings);
            }
        }
        finally
        {
            loading.Remove(ns);
        }
    }

    // Builds the visible declarations for one module: its own declarations (public and
    // private) plus the public declarations of all loaded modules in the namespaces it
    // directly imports. Also enforces XQST0034/XQST0049 collisions between the module's
    // own declarations and the declarations exposed by its imports.
    private static VisibilitySet BuildVisibility(
        XQueryStaticContext context,
        IReadOnlyDictionary<string, List<XQueryParseResult>> graph)
    {
        var functions = new HashSet<(string, string, int)>();
        var variables = new HashSet<(string, string)>();
        foreach (var fn in context.UserFunctions)
            functions.Add((fn.NamespaceUri, fn.LocalName, fn.Parameters.Count));
        foreach (var v in context.UserVariables)
            variables.Add((v.NamespaceUri, v.LocalName));

        foreach (var import in context.ImportedModules)
        {
            if (!graph.TryGetValue(import.NamespaceUri, out var modules))
                continue;
            foreach (var module in modules)
            {
                // A module that (transitively) imports its own target namespace must not
                // collide with itself (XQST0093a: self-import is legal).
                if (ReferenceEquals(module.StaticContext, context))
                    continue;
                foreach (var fn in module.StaticContext.UserFunctions)
                {
                    if (fn.IsPrivate)
                        continue;
                    // XQST0034: an own declaration collides with an imported one.
                    if (context.UserFunctions.Any(own =>
                            own.LocalName == fn.LocalName && own.NamespaceUri == fn.NamespaceUri
                            && own.Parameters.Count == fn.Parameters.Count))
                    {
                        throw new ParseException($"XQST0034: Function '{fn.LocalName}' with arity {fn.Parameters.Count} is declared more than once.", fn.Position);
                    }
                    functions.Add((fn.NamespaceUri, fn.LocalName, fn.Parameters.Count));
                }
                foreach (var v in module.StaticContext.UserVariables)
                {
                    if (v.IsPrivate)
                        continue;
                    // XQST0049: an own declaration collides with an imported one.
                    if (context.UserVariables.Any(own => own.LocalName == v.LocalName && own.NamespaceUri == v.NamespaceUri))
                        throw new ParseException($"XQST0049: Variable '${v.LocalName}' is declared more than once.", v.Position);
                    variables.Add((v.NamespaceUri, v.LocalName));
                }
            }
        }
        return new VisibilitySet(functions, variables);
    }

    // Validates and compiles one module's user-declared functions and variables. Bodies are
    // compiled with the declaring module's static context; library module declarations carry
    // the module's runtime context so their bodies execute with its namespaces and base URI.
    private static void CompileModuleDeclarations(
        XQueryStaticContext context,
        IReadOnlyDictionary<string, List<XQueryParseResult>> moduleGraph,
        HashSet<string> moduleNamespaces,
        VisibilitySet visibility,
        XPathOptimizer optimizer,
        List<CompiledUserFunction> userFunctions,
        List<CompiledUserVariable> userVariables,
        ModuleRuntimeContext? moduleRuntimeContext)
    {
        foreach (var fn in context.UserFunctions)
        {
            var fnBodyAst = ResolveFunctionNamespaces(fn.Body, context);
            ModuleVisibilityValidator.Validate(
                fnBodyAst, context, moduleNamespaces, visibility.Functions, visibility.Variables,
                fn.Parameters.Select(p => p.Name));
            var fnModule = new IrLowerer().Lower(optimizer.Optimize(fnBodyAst));
            userFunctions.Add(new CompiledUserFunction(
                fn.LocalName,
                fn.NamespaceUri,
                fn.Parameters.Select(p => p.Name).ToList(),
                fn.Parameters.Select(p => p.TypeName).ToList(),
                fn.ReturnType,
                fnModule,
                moduleRuntimeContext?.Namespaces,
                moduleRuntimeContext?.BaseUri,
                moduleRuntimeContext?.DefaultElementNamespace,
                moduleRuntimeContext?.DefaultCollation));
        }
        foreach (var v in context.UserVariables)
        {
            IrModule? varModule = null;
            if (v.Body is not null)
            {
                var varBodyAst = ResolveFunctionNamespaces(v.Body, context);
                ModuleVisibilityValidator.Validate(
                    varBodyAst, context, moduleNamespaces, visibility.Functions, visibility.Variables);
                varModule = new IrLowerer().Lower(optimizer.Optimize(varBodyAst));
            }
            userVariables.Add(new CompiledUserVariable(
                v.LocalName, v.NamespaceUri, v.TypeName, varModule, v.IsExternal,
                moduleRuntimeContext?.Namespaces,
                moduleRuntimeContext?.BaseUri,
                moduleRuntimeContext?.DefaultElementNamespace,
                moduleRuntimeContext?.DefaultCollation));
        }
    }

    private const string DefaultFunctionNamespace = "http://www.w3.org/2005/xpath-functions";

    private static string? ResolvePrefix(string? prefix, XQueryStaticContext context)
    {
        if (string.IsNullOrEmpty(prefix))
            return context.DefaultFunctionNamespace ?? DefaultFunctionNamespace;

        if (context.Namespaces.TryGetValue(prefix, out var nsUri))
            return nsUri;

        return null;
    }

    private static XPathAstNode ResolveFunctionNamespaces(XPathAstNode node, XQueryStaticContext context)
    {
        return node switch
        {
            FunctionCallNode fc => ResolveFunctionCall(fc, context),
            NamedFunctionRefNode nf => ResolveNamedFunctionRef(nf, context),
            ParenthesizedExprNode p => p with { Expression = ResolveFunctionNamespaces(p.Expression, context) },
            PredicateNode pred => pred with { Expression = ResolveFunctionNamespaces(pred.Expression, context) },
            StepNode step => step with { Predicates = step.Predicates.Select(p => ResolveFunctionNamespaces(p, context)).ToList() },
            PathExprNode path => path with { Steps = path.Steps.Select(s => ResolveFunctionNamespaces(s, context)).ToList() },
            SequenceExpressionNode seq => seq with { Expressions = seq.Expressions.Select(e => ResolveFunctionNamespaces(e, context)).ToList() },
            RangeExpressionNode range => range with { From = ResolveFunctionNamespaces(range.From, context), To = ResolveFunctionNamespaces(range.To, context) },
            IfExpressionNode ife => ife with { Condition = ResolveFunctionNamespaces(ife.Condition, context), ThenBranch = ResolveFunctionNamespaces(ife.ThenBranch, context), ElseBranch = ResolveFunctionNamespaces(ife.ElseBranch, context) },
            ForExpressionNode fe => fe with { Bindings = fe.Bindings.Select(b => b with { Expression = ResolveFunctionNamespaces(b.Expression, context) }).ToList(), ReturnExpression = ResolveFunctionNamespaces(fe.ReturnExpression, context) },
            LetExpressionNode le => le with { Bindings = le.Bindings.Select(b => b with { Expression = ResolveFunctionNamespaces(b.Expression, context) }).ToList(), Body = ResolveFunctionNamespaces(le.Body, context) },
            QuantifiedExpressionNode qe => qe with { Bindings = qe.Bindings.Select(b => b with { Expression = ResolveFunctionNamespaces(b.Expression, context) }).ToList(), SatisfiesExpression = ResolveFunctionNamespaces(qe.SatisfiesExpression, context) },
            BinaryExpressionNode bin => bin with { Left = ResolveFunctionNamespaces(bin.Left, context), Right = ResolveFunctionNamespaces(bin.Right, context) },
            FlworExpressionNode flwor => flwor with
            {
                Clauses = flwor.Clauses.Select(c => ResolveFlworClause(c, context)).ToList(),
                ReturnExpression = ResolveFunctionNamespaces(flwor.ReturnExpression, context)
            },
            UnaryExpressionNode un => un with { Operand = ResolveFunctionNamespaces(un.Operand, context) },
            CastNode cast => cast with { Expression = ResolveFunctionNamespaces(cast.Expression, context) },
            CastableNode castable => castable with { Expression = ResolveFunctionNamespaces(castable.Expression, context) },
            InstanceOfNode io => io with { Expression = ResolveFunctionNamespaces(io.Expression, context) },
            TreatNode treat => treat with { Expression = ResolveFunctionNamespaces(treat.Expression, context) },
            ArrowExprNode arrow => arrow with { Source = ResolveFunctionNamespaces(arrow.Source, context), Target = ResolveFunctionNamespaces(arrow.Target, context) },
            TryCatchNode tc => tc with
            {
                TryExpression = ResolveFunctionNamespaces(tc.TryExpression, context),
                Clauses = tc.Clauses.Select(c => c with { Expression = ResolveFunctionNamespaces(c.Expression, context) }).ToList()
            },
            LookupNode lookup => lookup with { Expression = ResolveFunctionNamespaces(lookup.Expression, context), Key = ResolveFunctionNamespaces(lookup.Key, context) },
            LookupWildcardNode lw => lw with { Expression = ResolveFunctionNamespaces(lw.Expression, context) },
            InlineFunctionNode inf => inf with { Body = ResolveFunctionNamespaces(inf.Body, context) },
            DirectElementConstructorNode elem => elem with
            {
                Attributes = elem.Attributes.Select(a => a with
                {
                    ValueParts = a.ValueParts.Select(p => ResolveFunctionNamespaces(p, context)).ToList()
                }).ToList(),
                Content = elem.Content.Select(p => ResolveFunctionNamespaces(p, context)).ToList()
            },
            ComputedElementConstructorNode n => n with
            {
                NameExpression = n.NameExpression is null ? null : ResolveFunctionNamespaces(n.NameExpression, context),
                ContentExpression = ResolveFunctionNamespaces(n.ContentExpression, context)
            },
            ComputedAttributeConstructorNode n => n with
            {
                NameExpression = n.NameExpression is null ? null : ResolveFunctionNamespaces(n.NameExpression, context),
                ValueExpression = ResolveFunctionNamespaces(n.ValueExpression, context)
            },
            ComputedDocumentConstructorNode n => n with { ContentExpression = ResolveFunctionNamespaces(n.ContentExpression, context) },
            ComputedTextConstructorNode n => n with { ValueExpression = ResolveFunctionNamespaces(n.ValueExpression, context) },
            ComputedCommentConstructorNode n => n with { ValueExpression = ResolveFunctionNamespaces(n.ValueExpression, context) },
            ComputedPIConstructorNode n => n with
            {
                TargetExpression = n.TargetExpression is null ? null : ResolveFunctionNamespaces(n.TargetExpression, context),
                ValueExpression = ResolveFunctionNamespaces(n.ValueExpression, context)
            },
            ComputedNamespaceConstructorNode n => n with
            {
                PrefixExpression = n.PrefixExpression is null ? null : ResolveFunctionNamespaces(n.PrefixExpression, context),
                UriExpression = ResolveFunctionNamespaces(n.UriExpression, context)
            },
            SwitchExpressionNode sw => sw with
            {
                Operand = ResolveFunctionNamespaces(sw.Operand, context),
                Cases = sw.Cases.Select(c => c with
                {
                    Values = c.Values.Select(v => ResolveFunctionNamespaces(v, context)).ToList(),
                    Return = ResolveFunctionNamespaces(c.Return, context)
                }).ToList(),
                Default = ResolveFunctionNamespaces(sw.Default, context)
            },
            TypeswitchExpressionNode ts => ts with
            {
                Operand = ResolveFunctionNamespaces(ts.Operand, context),
                Cases = ts.Cases.Select(c => c with { Return = ResolveFunctionNamespaces(c.Return, context) }).ToList(),
                Default = ResolveFunctionNamespaces(ts.Default, context)
            },
            MapConstructorNode mc => mc with { Entries = mc.Entries.Select(e => e with { Key = ResolveFunctionNamespaces(e.Key, context), Value = ResolveFunctionNamespaces(e.Value, context) }).ToList() },
            ArrayConstructorNode ac => ac with { Items = ac.Items.Select(i => ResolveFunctionNamespaces(i, context)).ToList() },
            PostfixPredicateNode pp => pp with { Expression = ResolveFunctionNamespaces(pp.Expression, context), Predicate = ResolveFunctionNamespaces(pp.Predicate, context) },
            DynamicFunctionCallNode dfc => dfc with { Function = ResolveFunctionNamespaces(dfc.Function, context), Arguments = dfc.Arguments.Select(a => ResolveFunctionNamespaces(a, context)).ToList() },
            _ => node
        };
    }

    private static FunctionCallNode ResolveFunctionCall(FunctionCallNode node, XQueryStaticContext context)
    {
        var nsUri = string.IsNullOrEmpty(node.NamespaceUri)
            ? ResolvePrefix(node.Prefix, context)
            : node.NamespaceUri;
        return node with
        {
            Arguments = node.Arguments.Select(a => ResolveFunctionNamespaces(a, context)).ToList(),
            NamespaceUri = nsUri
        };
    }

    private static FlworClauseNode ResolveFlworClause(FlworClauseNode clause, XQueryStaticContext context)
    {
        return clause switch
        {
            ForClauseNode forClause => forClause with
            {
                Bindings = forClause.Bindings.Select(b => b with
                {
                    Expression = ResolveFunctionNamespaces(b.Expression, context)
                }).ToList()
            },
            LetClauseNode letClause => letClause with
            {
                Bindings = letClause.Bindings.Select(b => b with
                {
                    Expression = ResolveFunctionNamespaces(b.Expression, context)
                }).ToList()
            },
            WhereClauseNode whereClause => whereClause with
            {
                Condition = ResolveFunctionNamespaces(whereClause.Condition, context)
            },
            OrderByClauseNode orderClause => orderClause with
            {
                Specs = orderClause.Specs.Select(s => s with
                {
                    KeyExpression = ResolveFunctionNamespaces(s.KeyExpression, context)
                }).ToList()
            },
            GroupByClauseNode groupClause => groupClause with
            {
                Specs = groupClause.Specs.Select(s => s with
                {
                    KeyExpression = s.KeyExpression is null ? null : ResolveFunctionNamespaces(s.KeyExpression, context)
                }).ToList()
            },
            WindowClauseNode windowClause => windowClause with
            {
                InExpression = ResolveFunctionNamespaces(windowClause.InExpression, context),
                StartCondition = windowClause.StartCondition with
                {
                    WhenExpression = ResolveFunctionNamespaces(windowClause.StartCondition.WhenExpression, context)
                },
                EndCondition = windowClause.EndCondition is null
                    ? null
                    : windowClause.EndCondition with
                    {
                        WhenExpression = ResolveFunctionNamespaces(windowClause.EndCondition.WhenExpression, context)
                    }
            },
            _ => clause
        };
    }

    private static NamedFunctionRefNode ResolveNamedFunctionRef(NamedFunctionRefNode node, XQueryStaticContext context)
    {
        var nsUri = string.IsNullOrEmpty(node.NamespaceUri)
            ? ResolvePrefix(node.Prefix, context)
            : node.NamespaceUri;
        return node with { NamespaceUri = nsUri };
    }
}
