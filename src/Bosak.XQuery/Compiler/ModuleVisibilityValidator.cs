// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 July 2026
// PURPOSE              : Statically validates that module-namespace function and variable references are visible (public) in the referencing module.
// SPECIAL NOTES        : Part of the Bosak XQuery 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 27-07-2026     | Creation — module visibility validation (XPST0017/XPST0008 across module boundaries)     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 27-07-2026     | Traversal for multi-clause TryCatchNode |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 27-07-2026     | Traversal for StringConstructorNode |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 29-07-2026     | excludeVariable parameter for initializer self-reference checks |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 07-08-2026     | Collect statically unresolvable names for the evaluation-time check (XPST0008/XPST0081/XPST0017); catch clauses bind the err:* variables |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 22-08-2026     | Traversal for ValidateExpressionNode |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Parser;
using Bosak.XPath.Parser.Ast;

namespace Bosak.XQuery.Compiler;

/// <summary>
/// Validates function and variable references against the declarations visible inside one
/// module: its own declarations plus the public declarations of the modules it directly
/// imports. References into a module namespace that name a declaration not visible to the
/// referencing module raise XPST0017 (functions) or XPST0008 (variables). References that
/// cannot be resolved to a module namespace are left to the dynamically scoped runtime.
/// </summary>
internal static class ModuleVisibilityValidator
{
    /// <summary>
    /// Names referenced by a validated body that could not be resolved statically:
    /// variables that are neither lexically bound nor declared globals, variable names
    /// with an undeclared prefix, and calls into the local-functions namespace that match
    /// no declared user function. Because the host may still supply external variables
    /// (and functions) through the evaluation context, these candidates are re-checked
    /// against the fully populated context at evaluation time — only then is XPST0008
    /// (undefined variable), XPST0081 (undeclared prefix), or XPST0017 (unknown function)
    /// raised. This keeps static errors static even for code paths that are never executed
    /// (dead branches, uncalled function bodies) without rejecting externally supplied names.
    /// </summary>
    internal sealed class UnresolvedNameReferences
    {
        /// <summary>Variable names whose prefix does not resolve in the prolog: (prefix, local).</summary>
        public HashSet<(string Prefix, string Local)> PrefixedVariables { get; } = new();

        /// <summary>Unresolvable variable references: (local, namespaceUri, display form).</summary>
        public HashSet<(string Local, string NamespaceUri, string Display)> Variables { get; } = new();

        /// <summary>Static calls/references into the local-functions namespace: (local, arity).</summary>
        public HashSet<(string Local, int Arity)> LocalFunctions { get; } = new();

        /// <summary>True when no unresolved names were collected.</summary>
        public bool IsEmpty => PrefixedVariables.Count == 0 && Variables.Count == 0 && LocalFunctions.Count == 0;

        /// <summary>Merges another body's collected references into this set.</summary>
        public void Merge(UnresolvedNameReferences other)
        {
            foreach (var v in other.PrefixedVariables) PrefixedVariables.Add(v);
            foreach (var v in other.Variables) Variables.Add(v);
            foreach (var f in other.LocalFunctions) LocalFunctions.Add(f);
        }
    }

    /// <summary>
    /// Validates one compiled body (the main query body, a user function body, or a global
    /// variable initializer) against the visibility sets of its declaring module.
    /// </summary>
    /// <param name="body">The body AST with function-call namespaces already resolved.</param>
    /// <param name="moduleContext">The static context of the declaring module (prefix resolution).</param>
    /// <param name="moduleNamespaces">The target namespaces of all loaded library modules.</param>
    /// <param name="visibleFunctions">Functions visible in the declaring module, as (ns, local, arity).</param>
    /// <param name="visibleVariables">Variables visible in the declaring module, as (ns, local).</param>
    /// <param name="parameterNames">Lexical parameter names of the enclosing function declaration, if any.</param>
    /// <param name="excludeVariable">
    /// When set (the global variable whose initializer is being compiled), a reference to
    /// it that is not shadowed by a lexical binding raises XPST0008 — a variable is not in
    /// scope within its own initializer (K-InternalVariablesWith-15b).
    /// </param>
    /// <returns>The names that could not be resolved statically, for the deferred
    /// evaluation-time check against externally supplied bindings.</returns>
    public static UnresolvedNameReferences Validate(
        XPathAstNode body,
        XQueryStaticContext moduleContext,
        IReadOnlyCollection<string> moduleNamespaces,
        IReadOnlySet<(string Ns, string Local, int Arity)> visibleFunctions,
        IReadOnlySet<(string Ns, string Local)> visibleVariables,
        IEnumerable<string>? parameterNames = null,
        (string Local, string Ns)? excludeVariable = null)
    {
        var walker = new Walker(moduleContext, moduleNamespaces, visibleFunctions, visibleVariables, excludeVariable);
        if (parameterNames is not null)
        {
            foreach (var name in parameterNames)
                walker.BindLexical(name);
        }
        walker.Walk(body);
        return walker.Report;
    }

    private sealed class Walker
    {
        private const string LocalFunctionsNamespace = "http://www.w3.org/2005/xquery-local-functions";
        private const string ErrorsNamespace = "http://www.w3.org/2005/xqt-errors";

        private readonly XQueryStaticContext _moduleContext;
        private readonly IReadOnlyCollection<string> _moduleNamespaces;
        private readonly IReadOnlySet<(string Ns, string Local, int Arity)> _visibleFunctions;
        private readonly IReadOnlySet<(string Ns, string Local)> _visibleVariables;
        private readonly (string Local, string Ns)? _excludeVariable;
        private readonly List<HashSet<(string Prefix, string Local)>> _boundLexical = new();
        private readonly List<HashSet<(string Ns, string Local)>> _boundResolved = new();
        private readonly List<Dictionary<string, string>> _namespaceScopes = new();
        private readonly UnresolvedNameReferences _report = new();

        public Walker(
            XQueryStaticContext moduleContext,
            IReadOnlyCollection<string> moduleNamespaces,
            IReadOnlySet<(string Ns, string Local, int Arity)> visibleFunctions,
            IReadOnlySet<(string Ns, string Local)> visibleVariables,
            (string Local, string Ns)? excludeVariable = null)
        {
            _moduleContext = moduleContext;
            _moduleNamespaces = moduleNamespaces;
            _visibleFunctions = visibleFunctions;
            _visibleVariables = visibleVariables;
            _excludeVariable = excludeVariable;
        }

        /// <summary>The unresolved names collected while walking the body.</summary>
        public UnresolvedNameReferences Report => _report;

        // ------------------------------------------------------------------
        // Bound-name tracking (lexical scoping approximation; the engine itself
        // is dynamically scoped, so when in doubt a name counts as bound).
        // ------------------------------------------------------------------

        private void PushScope()
        {
            _boundLexical.Add(new HashSet<(string, string)>());
            _boundResolved.Add(new HashSet<(string, string)>());
        }

        private void PopScope()
        {
            _boundLexical.RemoveAt(_boundLexical.Count - 1);
            _boundResolved.RemoveAt(_boundResolved.Count - 1);
        }

        /// <summary>Binds a lexical variable name ('local', 'prefix:local', or 'Q{uri}local').</summary>
        public void BindLexical(string lexicalName)
        {
            EnsureScope();
            if (lexicalName.StartsWith("Q{", StringComparison.Ordinal))
            {
                int close = lexicalName.IndexOf('}');
                if (close > 1)
                {
                    var ns = lexicalName[2..close];
                    var local = lexicalName[(close + 1)..];
                    _boundLexical[^1].Add(("", local));
                    _boundResolved[^1].Add((ns, local));
                }
                return;
            }
            int colon = lexicalName.IndexOf(':');
            if (colon > 0)
            {
                var prefix = lexicalName[..colon];
                var local = lexicalName[(colon + 1)..];
                _boundLexical[^1].Add((prefix, local));
                if (ResolvePrefix(prefix) is { } resolvedNs)
                    _boundResolved[^1].Add((resolvedNs, local));
                else
                    _report.PrefixedVariables.Add((prefix, local)); // XPST0081 candidate (binding site)
            }
            else
            {
                _boundLexical[^1].Add(("", lexicalName));
            }
        }

        private void Bind(string? prefix, string? local)
            => Bind(prefix, local, null);

        // Binds a variable; <paramref name="resolvedNs"/> carries the namespace of the
        // Q{uri}local binding form (already expanded by the parser).
        private void Bind(string? prefix, string? local, string? resolvedNs)
        {
            if (string.IsNullOrEmpty(local))
                return;
            EnsureScope();
            _boundLexical[^1].Add((prefix ?? "", local));
            if (!string.IsNullOrEmpty(resolvedNs))
            {
                _boundResolved[^1].Add((resolvedNs, local));
                return;
            }
            if (!string.IsNullOrEmpty(prefix))
            {
                if (ResolvePrefix(prefix) is { } ns)
                    _boundResolved[^1].Add((ns, local));
                else
                    _report.PrefixedVariables.Add((prefix, local)); // XPST0081 candidate (binding site)
            }
        }

        // Binds a variable by its already-resolved namespace (catch-clause error variables).
        private void BindResolved(string ns, string local)
        {
            EnsureScope();
            _boundResolved[^1].Add((ns, local));
        }

        // Resolves a namespace prefix: innermost constructor-local declarations first
        // (a direct element constructor's xmlns:p bindings are in scope for its enclosed
        // expressions — K2-DirectConElemNamespace-33), then the prolog bindings.
        private string? ResolvePrefix(string prefix)
        {
            for (int i = _namespaceScopes.Count - 1; i >= 0; i--)
            {
                if (_namespaceScopes[i].TryGetValue(prefix, out var ns))
                    return ns;
            }
            return _moduleContext.Namespaces.TryGetValue(prefix, out var moduleNs) ? moduleNs : null;
        }

        private void EnsureScope()
        {
            if (_boundLexical.Count == 0)
                PushScope();
        }

        private bool IsBound(VariableReferenceNode node, string resolvedNs)
        {
            var lexicalKey = (node.Prefix ?? "", node.LocalName);
            foreach (var scope in _boundLexical)
            {
                if (scope.Contains(lexicalKey))
                    return true;
            }
            foreach (var scope in _boundResolved)
            {
                if (scope.Contains((resolvedNs, node.LocalName)))
                    return true;
            }
            return false;
        }

        private bool IsLexicallyBound(string prefix, string local)
        {
            var key = (prefix, local);
            foreach (var scope in _boundLexical)
            {
                if (scope.Contains(key))
                    return true;
            }
            return false;
        }

        // ------------------------------------------------------------------
        // Visibility checks
        // ------------------------------------------------------------------

        private void CheckFunction(string? ns, string local, int arity)
        {
            if (string.IsNullOrEmpty(ns))
                return;
            if (_moduleNamespaces.Contains(ns))
            {
                if (!_visibleFunctions.Contains((ns, local, arity)))
                    throw new ParseException($"XPST0017: Function {{{ns}}}{local}#{arity} is not visible in this module.", 0);
                return;
            }
            // A static call into the local-functions namespace must name a declared user
            // function (XPST0017, K2-FunctionProlog-38). Deferred to evaluation time, when
            // the registered function signatures are known.
            if (ns == LocalFunctionsNamespace && !_visibleFunctions.Contains((ns, local, arity)))
                _report.LocalFunctions.Add((local, arity));
        }

        private void CheckVariable(VariableReferenceNode node)
        {
            string? ns = node.NamespaceUri;
            bool prefixUnresolved = false;
            if (string.IsNullOrEmpty(ns) && !string.IsNullOrEmpty(node.Prefix))
            {
                if (ResolvePrefix(node.Prefix) is { } resolved)
                    ns = resolved;
                else
                    prefixUnresolved = true;
            }

            if (prefixUnresolved)
            {
                // The prefix does not resolve in the prolog: XPST0081. When the name is
                // lexically bound under the same lexical form, the binding site has
                // already reported the undeclared prefix.
                if (!IsLexicallyBound(node.Prefix!, node.LocalName))
                    _report.PrefixedVariables.Add((node.Prefix!, node.LocalName));
                return;
            }

            ns ??= string.Empty;
            if (!IsBound(node, ns) && _excludeVariable is { } excluded
                && excluded.Local == node.LocalName && excluded.Ns == ns)
            {
                throw new ParseException(
                    $"XPST0008: Variable ${(node.Prefix is null ? "" : node.Prefix + ":")}{node.LocalName} is not defined in the initializer of the variable being declared.", 0);
            }
            if (IsBound(node, ns))
                return;
            if (_moduleNamespaces.Contains(ns))
            {
                if (!_visibleVariables.Contains((ns, node.LocalName)))
                {
                    throw new ParseException(
                        $"XPST0008: Variable ${(node.Prefix is null ? "" : node.Prefix + ":")}{node.LocalName} is not visible in this module.", 0);
                }
                return;
            }
            // Neither lexically bound nor a declared global: a static error (XPST0008)
            // unless the host binds the variable externally. Collected and re-checked
            // against the evaluation context (K-FunctionProlog-37/38, K-LetExprWithout-1).
            if (!_visibleVariables.Contains((ns, node.LocalName)))
            {
                var display = node.Prefix is not null
                    ? node.Prefix + ":" + node.LocalName
                    : ns.Length == 0 ? node.LocalName : $"Q{{{ns}}}{node.LocalName}";
                _report.Variables.Add((node.LocalName, ns, display));
            }
        }

        // ------------------------------------------------------------------
        // Traversal
        // ------------------------------------------------------------------

        public void Walk(XPathAstNode node)
        {
            switch (node)
            {
                case FunctionCallNode fc:
                    CheckFunction(fc.NamespaceUri, fc.LocalName, fc.Arguments.Count);
                    foreach (var arg in fc.Arguments)
                        Walk(arg);
                    break;
                case NamedFunctionRefNode nf:
                    CheckFunction(nf.NamespaceUri, nf.LocalName, nf.Arity);
                    break;
                case VariableReferenceNode vr:
                    CheckVariable(vr);
                    break;
                case ParenthesizedExprNode p:
                    Walk(p.Expression);
                    break;
                case PredicateNode pred:
                    Walk(pred.Expression);
                    break;
                case StepNode step:
                    foreach (var p in step.Predicates)
                        Walk(p);
                    break;
                case PathExprNode path:
                    foreach (var s in path.Steps)
                        Walk(s);
                    break;
                case SequenceExpressionNode seq:
                    foreach (var e in seq.Expressions)
                        Walk(e);
                    break;
                case RangeExpressionNode range:
                    Walk(range.From);
                    Walk(range.To);
                    break;
                case IfExpressionNode ife:
                    Walk(ife.Condition);
                    Walk(ife.ThenBranch);
                    Walk(ife.ElseBranch);
                    break;
                case ForExpressionNode fe:
                    PushScope();
                    foreach (var b in fe.Bindings)
                    {
                        Walk(b.Expression);
                        Bind(b.VariablePrefix, b.VariableName, b.VariableNamespaceUri);
                        if (b.PositionalVariableName is not null)
                            BindLexical(b.PositionalVariableName);
                    }
                    Walk(fe.ReturnExpression);
                    PopScope();
                    break;
                case LetExpressionNode le:
                    PushScope();
                    foreach (var b in le.Bindings)
                    {
                        Walk(b.Expression);
                        Bind(b.VariablePrefix, b.VariableName, b.VariableNamespaceUri);
                    }
                    Walk(le.Body);
                    PopScope();
                    break;
                case QuantifiedExpressionNode qe:
                    PushScope();
                    foreach (var b in qe.Bindings)
                    {
                        Walk(b.Expression);
                        Bind(b.VariablePrefix, b.VariableName, b.VariableNamespaceUri);
                        if (b.PositionalVariableName is not null)
                            BindLexical(b.PositionalVariableName);
                    }
                    Walk(qe.SatisfiesExpression);
                    PopScope();
                    break;
                case BinaryExpressionNode bin:
                    Walk(bin.Left);
                    Walk(bin.Right);
                    break;
                case FlworExpressionNode flwor:
                    PushScope();
                    foreach (var clause in flwor.Clauses)
                        WalkClause(clause);
                    Walk(flwor.ReturnExpression);
                    PopScope();
                    break;
                case UnaryExpressionNode un:
                    Walk(un.Operand);
                    break;
                case CastNode cast:
                    Walk(cast.Expression);
                    break;
                case CastableNode castable:
                    Walk(castable.Expression);
                    break;
                case InstanceOfNode io:
                    Walk(io.Expression);
                    break;
                case TreatNode treat:
                    Walk(treat.Expression);
                    break;
                case ArrowExprNode arrow:
                    Walk(arrow.Source);
                    // An arrow target call gains the source as its implicit first
                    // argument: $x => local:f($a) calls local:f#2 (numberformat122).
                    if (arrow.Target is FunctionCallNode arrowCall)
                    {
                        CheckFunction(arrowCall.NamespaceUri, arrowCall.LocalName, arrowCall.Arguments.Count + 1);
                        foreach (var arg in arrowCall.Arguments)
                            Walk(arg);
                    }
                    else
                    {
                        Walk(arrow.Target);
                    }
                    break;
                case TryCatchNode tc:
                    Walk(tc.TryExpression);
                    foreach (var c in tc.Clauses)
                    {
                        // The catch clause implicitly binds the error record variables
                        // ($err:code, $err:description, ...) in the errors namespace.
                        PushScope();
                        BindResolved(ErrorsNamespace, "code");
                        BindResolved(ErrorsNamespace, "description");
                        BindResolved(ErrorsNamespace, "value");
                        BindResolved(ErrorsNamespace, "module");
                        BindResolved(ErrorsNamespace, "line-number");
                        BindResolved(ErrorsNamespace, "column-number");
                        BindResolved(ErrorsNamespace, "additional");
                        Walk(c.Expression);
                        PopScope();
                    }
                    break;
                case ValidateExpressionNode v:
                    Walk(v.Expression);
                    break;
                case StringConstructorNode sc:
                    foreach (var p in sc.Parts)
                        Walk(p);
                    break;
                case LookupNode lookup:
                    Walk(lookup.Expression);
                    Walk(lookup.Key);
                    break;
                case LookupWildcardNode lw:
                    Walk(lw.Expression);
                    break;
                case InlineFunctionNode inf:
                    PushScope();
                    foreach (var p in inf.Parameters)
                        BindLexical(p.Name);
                    Walk(inf.Body);
                    PopScope();
                    break;
                case DirectElementConstructorNode elem:
                    {
                        // A direct element constructor's xmlns:p declarations are in scope
                        // for all its enclosed expressions, regardless of attribute order
                        // (K2-DirectConElemNamespace-33).
                        Dictionary<string, string>? ctorNamespaces = null;
                        foreach (var a in elem.Attributes)
                        {
                            if (a.Prefix == "xmlns" && a.ValueParts.Count == 1
                                && a.ValueParts[0] is StringLiteralNode nsLiteral)
                            {
                                ctorNamespaces ??= new Dictionary<string, string>(StringComparer.Ordinal);
                                ctorNamespaces[a.Name] = nsLiteral.Value;
                            }
                        }
                        if (ctorNamespaces is not null)
                            _namespaceScopes.Add(ctorNamespaces);
                        foreach (var a in elem.Attributes)
                            foreach (var p in a.ValueParts)
                                Walk(p);
                        foreach (var p in elem.Content)
                            Walk(p);
                        if (ctorNamespaces is not null)
                            _namespaceScopes.RemoveAt(_namespaceScopes.Count - 1);
                        break;
                    }
                case ComputedElementConstructorNode n:
                    if (n.NameExpression is not null) Walk(n.NameExpression);
                    Walk(n.ContentExpression);
                    break;
                case ComputedAttributeConstructorNode n:
                    if (n.NameExpression is not null) Walk(n.NameExpression);
                    Walk(n.ValueExpression);
                    break;
                case ComputedDocumentConstructorNode n:
                    Walk(n.ContentExpression);
                    break;
                case ComputedTextConstructorNode n:
                    Walk(n.ValueExpression);
                    break;
                case ComputedCommentConstructorNode n:
                    Walk(n.ValueExpression);
                    break;
                case ComputedPIConstructorNode n:
                    if (n.TargetExpression is not null) Walk(n.TargetExpression);
                    Walk(n.ValueExpression);
                    break;
                case ComputedNamespaceConstructorNode n:
                    if (n.PrefixExpression is not null) Walk(n.PrefixExpression);
                    Walk(n.UriExpression);
                    break;
                case SwitchExpressionNode sw:
                    Walk(sw.Operand);
                    foreach (var c in sw.Cases)
                    {
                        foreach (var v in c.Values)
                            Walk(v);
                        Walk(c.Return);
                    }
                    Walk(sw.Default);
                    break;
                case TypeswitchExpressionNode ts:
                    Walk(ts.Operand);
                    foreach (var c in ts.Cases)
                    {
                        PushScope();
                        Bind(c.VariablePrefix, c.VariableName, c.VariableNamespaceUri);
                        Walk(c.Return);
                        PopScope();
                    }
                    PushScope();
                    Bind(ts.DefaultVariablePrefix, ts.DefaultVariableName, ts.DefaultVariableNamespaceUri);
                    Walk(ts.Default);
                    PopScope();
                    break;
                case MapConstructorNode mc:
                    foreach (var e in mc.Entries)
                    {
                        Walk(e.Key);
                        Walk(e.Value);
                    }
                    break;
                case ArrayConstructorNode ac:
                    foreach (var i in ac.Items)
                        Walk(i);
                    break;
                case PostfixPredicateNode pp:
                    Walk(pp.Expression);
                    Walk(pp.Predicate);
                    break;
                case DynamicFunctionCallNode dfc:
                    Walk(dfc.Function);
                    foreach (var a in dfc.Arguments)
                        Walk(a);
                    break;
            }
        }

        private void WalkClause(FlworClauseNode clause)
        {
            switch (clause)
            {
                case ForClauseNode forClause:
                    foreach (var b in forClause.Bindings)
                    {
                        Walk(b.Expression);
                        Bind(b.VariablePrefix, b.VariableName, b.VariableNamespaceUri);
                        if (b.PositionalVariableName is not null)
                            BindLexical(b.PositionalVariableName);
                    }
                    break;
                case LetClauseNode letClause:
                    foreach (var b in letClause.Bindings)
                    {
                        Walk(b.Expression);
                        Bind(b.VariablePrefix, b.VariableName, b.VariableNamespaceUri);
                    }
                    break;
                case WhereClauseNode whereClause:
                    Walk(whereClause.Condition);
                    break;
                case OrderByClauseNode orderClause:
                    foreach (var s in orderClause.Specs)
                        Walk(s.KeyExpression);
                    break;
                case GroupByClauseNode groupClause:
                    foreach (var s in groupClause.Specs)
                    {
                        if (s.KeyExpression is not null)
                        {
                            // A grouping spec with ':= expr' introduces a new variable
                            // binding (in scope for subsequent clauses and the return);
                            // without it the spec re-groups an existing, already bound
                            // variable. All pre-grouping variables stay bound either way.
                            Walk(s.KeyExpression);
                            Bind(s.Prefix, s.VariableName, s.NamespaceUri);
                        }
                    }
                    break;
                case CountClauseNode countClause:
                    Bind(countClause.Prefix, countClause.VariableName, countClause.NamespaceUri);
                    break;
                case WindowClauseNode windowClause:
                    Walk(windowClause.InExpression);
                    Bind(windowClause.Prefix, windowClause.VariableName, windowClause.NamespaceUri);
                    // Window condition variables stay in scope for the end condition's
                    // when expression and for the rest of the FLWOR (TumblingWindowExpr530:
                    // return ($w, $s, $x, $sp, $sn, $e, $y, $ep, $en)).
                    BindWindowConditionVariables(windowClause.StartCondition);
                    Walk(windowClause.StartCondition.WhenExpression);
                    if (windowClause.EndCondition is not null)
                    {
                        BindWindowConditionVariables(windowClause.EndCondition);
                        Walk(windowClause.EndCondition.WhenExpression);
                    }
                    break;
            }
        }

        private void BindWindowConditionVariables(WindowCondition condition)
        {
            // The parser keeps the lexical name forms (plain, prefix:local, Q{uri}local).
            if (condition.CurrentItemVariable is not null)
                BindLexical(condition.CurrentItemVariable);
            if (condition.PositionalVariable is not null)
                BindLexical(condition.PositionalVariable);
            if (condition.PreviousItemVariable is not null)
                BindLexical(condition.PreviousItemVariable);
            if (condition.NextItemVariable is not null)
                BindLexical(condition.NextItemVariable);
        }
    }
}
