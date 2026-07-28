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
    /// Validates one compiled body (the main query body, a user function body, or a global
    /// variable initializer) against the visibility sets of its declaring module.
    /// </summary>
    /// <param name="body">The body AST with function-call namespaces already resolved.</param>
    /// <param name="moduleContext">The static context of the declaring module (prefix resolution).</param>
    /// <param name="moduleNamespaces">The target namespaces of all loaded library modules.</param>
    /// <param name="visibleFunctions">Functions visible in the declaring module, as (ns, local, arity).</param>
    /// <param name="visibleVariables">Variables visible in the declaring module, as (ns, local).</param>
    /// <param name="parameterNames">Lexical parameter names of the enclosing function declaration, if any.</param>
    public static void Validate(
        XPathAstNode body,
        XQueryStaticContext moduleContext,
        IReadOnlyCollection<string> moduleNamespaces,
        IReadOnlySet<(string Ns, string Local, int Arity)> visibleFunctions,
        IReadOnlySet<(string Ns, string Local)> visibleVariables,
        IEnumerable<string>? parameterNames = null)
    {
        var walker = new Walker(moduleContext, moduleNamespaces, visibleFunctions, visibleVariables);
        if (parameterNames is not null)
        {
            foreach (var name in parameterNames)
                walker.BindLexical(name);
        }
        walker.Walk(body);
    }

    private sealed class Walker
    {
        private readonly XQueryStaticContext _moduleContext;
        private readonly IReadOnlyCollection<string> _moduleNamespaces;
        private readonly IReadOnlySet<(string Ns, string Local, int Arity)> _visibleFunctions;
        private readonly IReadOnlySet<(string Ns, string Local)> _visibleVariables;
        private readonly List<HashSet<(string Prefix, string Local)>> _boundLexical = new();
        private readonly List<HashSet<(string Ns, string Local)>> _boundResolved = new();

        public Walker(
            XQueryStaticContext moduleContext,
            IReadOnlyCollection<string> moduleNamespaces,
            IReadOnlySet<(string Ns, string Local, int Arity)> visibleFunctions,
            IReadOnlySet<(string Ns, string Local)> visibleVariables)
        {
            _moduleContext = moduleContext;
            _moduleNamespaces = moduleNamespaces;
            _visibleFunctions = visibleFunctions;
            _visibleVariables = visibleVariables;
        }

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
                if (_moduleContext.Namespaces.TryGetValue(prefix, out var resolvedNs))
                    _boundResolved[^1].Add((resolvedNs, local));
            }
            else
            {
                _boundLexical[^1].Add(("", lexicalName));
            }
        }

        private void Bind(string? prefix, string? local)
        {
            if (string.IsNullOrEmpty(local))
                return;
            EnsureScope();
            _boundLexical[^1].Add((prefix ?? "", local));
            if (!string.IsNullOrEmpty(prefix) && _moduleContext.Namespaces.TryGetValue(prefix, out var ns))
                _boundResolved[^1].Add((ns, local));
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

        // ------------------------------------------------------------------
        // Visibility checks
        // ------------------------------------------------------------------

        private void CheckFunction(string? ns, string local, int arity)
        {
            if (string.IsNullOrEmpty(ns) || !_moduleNamespaces.Contains(ns))
                return;
            if (!_visibleFunctions.Contains((ns, local, arity)))
                throw new ParseException($"XPST0017: Function {{{ns}}}{local}#{arity} is not visible in this module.", 0);
        }

        private void CheckVariable(VariableReferenceNode node)
        {
            string? ns = node.NamespaceUri;
            if (string.IsNullOrEmpty(ns) && !string.IsNullOrEmpty(node.Prefix))
                _moduleContext.Namespaces.TryGetValue(node.Prefix, out ns);
            ns ??= string.Empty;
            if (!_moduleNamespaces.Contains(ns) || IsBound(node, ns))
                return;
            if (!_visibleVariables.Contains((ns, node.LocalName)))
            {
                throw new ParseException(
                    $"XPST0008: Variable ${(node.Prefix is null ? "" : node.Prefix + ":")}{node.LocalName} is not visible in this module.", 0);
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
                        Bind(b.VariablePrefix, b.VariableName);
                        Bind(null, b.PositionalVariableName);
                    }
                    Walk(fe.ReturnExpression);
                    PopScope();
                    break;
                case LetExpressionNode le:
                    PushScope();
                    foreach (var b in le.Bindings)
                    {
                        Walk(b.Expression);
                        Bind(b.VariablePrefix, b.VariableName);
                    }
                    Walk(le.Body);
                    PopScope();
                    break;
                case QuantifiedExpressionNode qe:
                    PushScope();
                    foreach (var b in qe.Bindings)
                    {
                        Walk(b.Expression);
                        Bind(b.VariablePrefix, b.VariableName);
                        Bind(null, b.PositionalVariableName);
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
                    Walk(arrow.Target);
                    break;
                case TryCatchNode tc:
                    Walk(tc.TryExpression);
                    Walk(tc.CatchExpression);
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
                    foreach (var a in elem.Attributes)
                        foreach (var p in a.ValueParts)
                            Walk(p);
                    foreach (var p in elem.Content)
                        Walk(p);
                    break;
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
                        Bind(c.VariablePrefix, c.VariableName);
                        Walk(c.Return);
                        PopScope();
                    }
                    PushScope();
                    Bind(ts.DefaultVariablePrefix, ts.DefaultVariableName);
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
                        Bind(b.VariablePrefix, b.VariableName);
                        Bind(null, b.PositionalVariableName);
                    }
                    break;
                case LetClauseNode letClause:
                    foreach (var b in letClause.Bindings)
                    {
                        Walk(b.Expression);
                        Bind(b.VariablePrefix, b.VariableName);
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
                            Walk(s.KeyExpression);
                    }
                    // All pre-grouping variables stay bound after the grouping.
                    break;
                case CountClauseNode countClause:
                    Bind(countClause.Prefix, countClause.VariableName);
                    break;
                case WindowClauseNode windowClause:
                    Walk(windowClause.InExpression);
                    Bind(windowClause.Prefix, windowClause.VariableName);
                    WalkWindowCondition(windowClause.StartCondition);
                    if (windowClause.EndCondition is not null)
                        WalkWindowCondition(windowClause.EndCondition);
                    break;
            }
        }

        private void WalkWindowCondition(WindowCondition condition)
        {
            PushScope();
            Bind(null, condition.CurrentItemVariable);
            Bind(null, condition.PositionalVariable);
            Bind(null, condition.PreviousItemVariable);
            Bind(null, condition.NextItemVariable);
            Walk(condition.WhenExpression);
            PopScope();
        }
    }
}
