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
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Compiler.Optimizer;
using Bosak.XPath.Parser.Ast;
using Bosak.XQuery.Compiler;

namespace Bosak.XQuery.Api;

/// <summary>
/// Compiles XQuery 3.1 source text into an <see cref="XQueryExecutable"/> that can be executed repeatedly.
/// </summary>
public sealed class XQueryCompiler
{
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

        // 2. Resolve function-call namespaces using the static context derived from the prolog.
        var resolvedBody = ResolveFunctionNamespaces(parseResult.Body, parseResult.StaticContext);

        // 3. Optimize the AST.
        var optimizer = new XPathOptimizer();
        var optimized = optimizer.Optimize(resolvedBody);

        // 3b. Compile user-declared function and variable bodies through the same pipeline.
        var userFunctions = new List<CompiledUserFunction>();
        foreach (var fn in parseResult.StaticContext.UserFunctions)
        {
            var fnBodyAst = ResolveFunctionNamespaces(fn.Body, parseResult.StaticContext);
            var fnModule = new IrLowerer().Lower(optimizer.Optimize(fnBodyAst));
            userFunctions.Add(new CompiledUserFunction(
                fn.LocalName,
                fn.NamespaceUri,
                fn.Parameters.Select(p => p.Name).ToList(),
                fn.Parameters.Select(p => p.TypeName).ToList(),
                fn.ReturnType,
                fnModule));
        }
        var userVariables = new List<CompiledUserVariable>();
        foreach (var v in parseResult.StaticContext.UserVariables)
        {
            IrModule? varModule = null;
            if (v.Body is not null)
            {
                var varBodyAst = ResolveFunctionNamespaces(v.Body, parseResult.StaticContext);
                varModule = new IrLowerer().Lower(optimizer.Optimize(varBodyAst));
            }
            userVariables.Add(new CompiledUserVariable(v.LocalName, v.NamespaceUri, v.TypeName, varModule, v.IsExternal));
        }

        // 4. Lower to IR.
        var lowerer = new IrLowerer();
        var module = lowerer.Lower(optimized);

        return new XQueryExecutable(module, parseResult.StaticContext, userFunctions, userVariables);
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
            TryCatchNode tc => tc with { TryExpression = ResolveFunctionNamespaces(tc.TryExpression, context), CatchExpression = ResolveFunctionNamespaces(tc.CatchExpression, context) },
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
