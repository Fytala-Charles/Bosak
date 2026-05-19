// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Performs bottom-up optimizations on the XPath AST before lowering to IR
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Added OptimizeLookupWildcard                                                           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Parser.Ast;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Compiler.Optimizer;

/// <summary>
/// Performs bottom-up optimizations on the XPath AST before lowering to IR.
/// </summary>
public sealed class XPathOptimizer
{
    /// <summary>
    /// Optimizes an AST node. Runs multiple passes until no further changes are made.
    /// </summary>
    public XPathAstNode Optimize(XPathAstNode node)
    {
        bool changed;
        do
        {
            changed = false;
            node = OptimizeNode(node, ref changed);
        } while (changed);
        return node;
    }

    private XPathAstNode OptimizeNode(XPathAstNode node, ref bool changed)
    {
        return node switch
        {
            BinaryExpressionNode bin => OptimizeBinary(bin, ref changed),
            UnaryExpressionNode unary => OptimizeUnary(unary, ref changed),
            IfExpressionNode ifExpr => OptimizeIf(ifExpr, ref changed),
            SequenceExpressionNode seq => OptimizeSequence(seq, ref changed),
            RangeExpressionNode range => OptimizeRange(range, ref changed),
            PathExprNode path => OptimizePath(path, ref changed),
            StepNode step => OptimizeStep(step, ref changed),
            PostfixPredicateNode postfix => OptimizePostfix(postfix, ref changed),
            ParenthesizedExprNode paren => OptimizeParen(paren, ref changed),
            ForExpressionNode forExpr => OptimizeFor(forExpr, ref changed),
            QuantifiedExpressionNode quant => OptimizeQuantified(quant, ref changed),
            ArrowExprNode arrow => OptimizeArrow(arrow, ref changed),
            LookupNode lookup => OptimizeLookup(lookup, ref changed),
            LookupWildcardNode lookup => OptimizeLookupWildcard(lookup, ref changed),
            CastNode cast => OptimizeCast(cast, ref changed),
            CastableNode castable => OptimizeCastable(castable, ref changed),
            InstanceOfNode inst => OptimizeInstanceOf(inst, ref changed),
            TreatNode treat => OptimizeTreat(treat, ref changed),
            MapConstructorNode map => OptimizeMap(map, ref changed),
            ArrayConstructorNode arr => OptimizeArray(arr, ref changed),
            InlineFunctionNode inline => OptimizeInline(inline, ref changed),
            FunctionCallNode call => OptimizeFunctionCall(call, ref changed),
            LetExpressionNode let => OptimizeLet(let, ref changed),
            DynamicFunctionCallNode dyn => OptimizeDynamicFunctionCall(dyn, ref changed),
            _ => node
        };
    }

    // ------------------------------------------------------------------
    // Binary expressions
    // ------------------------------------------------------------------

    private XPathAstNode OptimizeBinary(BinaryExpressionNode node, ref bool changed)
    {
        var left = OptimizeNode(node.Left, ref changed);
        var right = OptimizeNode(node.Right, ref changed);

        if (left != node.Left || right != node.Right)
        {
            changed = true;
            node = node with { Left = left, Right = right };
        }

        // Constant folding
        if (left is IntegerLiteralNode li && right is IntegerLiteralNode ri)
        {
            XPathAstNode? result = node.Operator switch
            {
                BinaryOperator.Plus => new IntegerLiteralNode(li.Value + ri.Value),
                BinaryOperator.Minus => new IntegerLiteralNode(li.Value - ri.Value),
                BinaryOperator.Multiply => new IntegerLiteralNode(li.Value * ri.Value),
                BinaryOperator.Divide => new IntegerLiteralNode(li.Value / ri.Value),
                BinaryOperator.Idiv => new IntegerLiteralNode(li.Value / ri.Value),
                BinaryOperator.Mod => new IntegerLiteralNode(li.Value % ri.Value),
                BinaryOperator.Eq => new BooleanLiteralNode(li.Value == ri.Value),
                BinaryOperator.Ne => new BooleanLiteralNode(li.Value != ri.Value),
                BinaryOperator.Lt => new BooleanLiteralNode(li.Value < ri.Value),
                BinaryOperator.Le => new BooleanLiteralNode(li.Value <= ri.Value),
                BinaryOperator.Gt => new BooleanLiteralNode(li.Value > ri.Value),
                BinaryOperator.Ge => new BooleanLiteralNode(li.Value >= ri.Value),
                BinaryOperator.Equal => new BooleanLiteralNode(li.Value == ri.Value),
                BinaryOperator.NotEqual => new BooleanLiteralNode(li.Value != ri.Value),
                BinaryOperator.LessThan => new BooleanLiteralNode(li.Value < ri.Value),
                BinaryOperator.LessThanOrEqual => new BooleanLiteralNode(li.Value <= ri.Value),
                BinaryOperator.GreaterThan => new BooleanLiteralNode(li.Value > ri.Value),
                BinaryOperator.GreaterThanOrEqual => new BooleanLiteralNode(li.Value >= ri.Value),
                _ => null
            };
            if (result is not null) { changed = true; return result; }
        }

        if (left is DoubleLiteralNode ld && right is DoubleLiteralNode rd)
        {
            var result = node.Operator switch
            {
                BinaryOperator.Plus => new DoubleLiteralNode(ld.Value + rd.Value),
                BinaryOperator.Minus => new DoubleLiteralNode(ld.Value - rd.Value),
                BinaryOperator.Multiply => new DoubleLiteralNode(ld.Value * rd.Value),
                BinaryOperator.Divide => new DoubleLiteralNode(ld.Value / rd.Value),
                _ => null
            };
            if (result is not null) { changed = true; return result; }
        }

        if (left is DecimalLiteralNode lc && right is DecimalLiteralNode rc)
        {
            var result = node.Operator switch
            {
                BinaryOperator.Plus => new DecimalLiteralNode(lc.Value + rc.Value),
                BinaryOperator.Minus => new DecimalLiteralNode(lc.Value - rc.Value),
                BinaryOperator.Multiply => new DecimalLiteralNode(lc.Value * rc.Value),
                BinaryOperator.Divide => new DecimalLiteralNode(lc.Value / rc.Value),
                _ => null
            };
            if (result is not null) { changed = true; return result; }
        }

        if (left is StringLiteralNode ls && right is StringLiteralNode rs && node.Operator == BinaryOperator.StringConcat)
        {
            changed = true;
            return new StringLiteralNode(ls.Value + rs.Value);
        }

        // Boolean simplification
        if (node.Operator is BinaryOperator.And or BinaryOperator.Or)
        {
            var simplified = SimplifyBoolean(node.Operator, left, right);
            if (simplified is not null) { changed = true; return simplified; }
        }

        return node;
    }

    private static XPathAstNode? SimplifyBoolean(BinaryOperator op, XPathAstNode left, XPathAstNode right)
    {
        bool? leftBool = AsBoolean(left);
        bool? rightBool = AsBoolean(right);

        if (op == BinaryOperator.And)
        {
            if (leftBool == false || rightBool == false) return FalseLiteral();
            if (leftBool == true) return right;
            if (rightBool == true) return left;
        }
        else if (op == BinaryOperator.Or)
        {
            if (leftBool == true || rightBool == true) return TrueLiteral();
            if (leftBool == false) return right;
            if (rightBool == false) return left;
        }

        return null;
    }

    // ------------------------------------------------------------------
    // Unary expressions
    // ------------------------------------------------------------------

    private XPathAstNode OptimizeUnary(UnaryExpressionNode node, ref bool changed)
    {
        var operand = OptimizeNode(node.Operand, ref changed);
        if (operand != node.Operand)
        {
            changed = true;
            node = node with { Operand = operand };
        }

        // not(not(x)) => x
        if (node.Operator == UnaryOperator.Minus &&
            operand is UnaryExpressionNode inner &&
            inner.Operator == UnaryOperator.Minus)
        {
            changed = true;
            return inner.Operand;
        }

        // -(IntegerLiteral) => negated literal
        if (node.Operator == UnaryOperator.Minus && operand is IntegerLiteralNode i)
        {
            changed = true;
            return new IntegerLiteralNode(-i.Value);
        }

        // -(DoubleLiteral) => negated literal
        if (node.Operator == UnaryOperator.Minus && operand is DoubleLiteralNode d)
        {
            changed = true;
            return new DoubleLiteralNode(-d.Value);
        }

        // -(DecimalLiteral) => negated literal
        if (node.Operator == UnaryOperator.Minus && operand is DecimalLiteralNode dec)
        {
            changed = true;
            return new DecimalLiteralNode(-dec.Value);
        }

        // +x => x
        if (node.Operator == UnaryOperator.Plus)
        {
            changed = true;
            return operand;
        }

        return node;
    }

    // ------------------------------------------------------------------
    // If expressions
    // ------------------------------------------------------------------

    private XPathAstNode OptimizeIf(IfExpressionNode node, ref bool changed)
    {
        var cond = OptimizeNode(node.Condition, ref changed);
        var thenBranch = OptimizeNode(node.ThenBranch, ref changed);
        var elseBranch = OptimizeNode(node.ElseBranch, ref changed);

        if (cond != node.Condition || thenBranch != node.ThenBranch || elseBranch != node.ElseBranch)
        {
            changed = true;
            node = node with { Condition = cond, ThenBranch = thenBranch, ElseBranch = elseBranch };
        }

        // Dead code elimination
        if (AsBoolean(cond) == true) { changed = true; return thenBranch; }
        if (AsBoolean(cond) == false) { changed = true; return elseBranch; }

        return node;
    }

    // ------------------------------------------------------------------
    // Sequence expressions
    // ------------------------------------------------------------------

    private XPathAstNode OptimizeSequence(SequenceExpressionNode node, ref bool changed)
    {
        var optimized = new List<XPathAstNode>();
        foreach (var expr in node.Expressions)
        {
            var opt = OptimizeNode(expr, ref changed);
            optimized.Add(opt);
        }

        if (optimized.Count != node.Expressions.Count || !optimized.SequenceEqual(node.Expressions))
        {
            changed = true;
            return optimized.Count == 1 ? optimized[0] : node with { Expressions = optimized };
        }

        return node;
    }

    // ------------------------------------------------------------------
    // Range expressions
    // ------------------------------------------------------------------

    private XPathAstNode OptimizeRange(RangeExpressionNode node, ref bool changed)
    {
        var from = OptimizeNode(node.From, ref changed);
        var to = OptimizeNode(node.To, ref changed);

        if (from != node.From || to != node.To)
        {
            changed = true;
            node = node with { From = from, To = to };
        }

        // Constant range expansion (only for small ranges to avoid blow-up)
        if (from is IntegerLiteralNode fi && to is IntegerLiteralNode ti)
        {
            long count = ti.Value - fi.Value + 1;
            if (count >= 0 && count <= 16)
            {
                changed = true;
                var items = new List<XPathAstNode>();
                for (long v = fi.Value; v <= ti.Value; v++)
                    items.Add(new IntegerLiteralNode(v));
                return new SequenceExpressionNode(items);
            }
        }

        return node;
    }

    // ------------------------------------------------------------------
    // Path expressions
    // ------------------------------------------------------------------

    private XPathAstNode OptimizePath(PathExprNode node, ref bool changed)
    {
        var optimized = new List<XPathAstNode>();
        foreach (var step in node.Steps)
        {
            optimized.Add(OptimizeNode(step, ref changed));
        }

        if (!optimized.SequenceEqual(node.Steps))
        {
            changed = true;
            return node with { Steps = optimized };
        }

        return node;
    }

    private XPathAstNode OptimizeStep(StepNode node, ref bool changed)
    {
        var preds = new List<XPathAstNode>();
        foreach (var p in node.Predicates)
        {
            preds.Add(OptimizeNode(p, ref changed));
        }

        if (!preds.SequenceEqual(node.Predicates))
        {
            changed = true;
            return node with { Predicates = preds };
        }

        return node;
    }

    private XPathAstNode OptimizePostfix(PostfixPredicateNode node, ref bool changed)
    {
        var expr = OptimizeNode(node.Expression, ref changed);
        var pred = OptimizeNode(node.Predicate, ref changed);

        if (expr != node.Expression || pred != node.Predicate)
        {
            changed = true;
            return node with { Expression = expr, Predicate = pred };
        }

        return node;
    }

    private XPathAstNode OptimizeParen(ParenthesizedExprNode node, ref bool changed)
    {
        var inner = OptimizeNode(node.Expression, ref changed);
        changed = true;
        // Unwrap single-expression parentheses
        return inner;
    }

    // ------------------------------------------------------------------
    // FLWOR
    // ------------------------------------------------------------------

    private XPathAstNode OptimizeFor(ForExpressionNode node, ref bool changed)
    {
        var bindings = new List<QuantifiedBinding>();
        foreach (var b in node.Bindings)
        {
            var expr = OptimizeNode(b.Expression, ref changed);
            bindings.Add(expr == b.Expression ? b : new QuantifiedBinding(b.VariableName, expr));
        }

        var body = OptimizeNode(node.ReturnExpression, ref changed);

        if (!bindings.SequenceEqual(node.Bindings) || body != node.ReturnExpression)
        {
            changed = true;
            return node with { Bindings = bindings, ReturnExpression = body };
        }

        return node;
    }

    private XPathAstNode OptimizeQuantified(QuantifiedExpressionNode node, ref bool changed)
    {
        var bindings = new List<QuantifiedBinding>();
        foreach (var b in node.Bindings)
        {
            var expr = OptimizeNode(b.Expression, ref changed);
            bindings.Add(expr == b.Expression ? b : new QuantifiedBinding(b.VariableName, expr));
        }

        var body = OptimizeNode(node.SatisfiesExpression, ref changed);

        if (!bindings.SequenceEqual(node.Bindings) || body != node.SatisfiesExpression)
        {
            changed = true;
            return node with { Bindings = bindings, SatisfiesExpression = body };
        }

        return node;
    }

    // ------------------------------------------------------------------
    // Arrow / Lookup
    // ------------------------------------------------------------------

    private XPathAstNode OptimizeArrow(ArrowExprNode node, ref bool changed)
    {
        var source = OptimizeNode(node.Source, ref changed);
        var target = OptimizeNode(node.Target, ref changed);

        if (source != node.Source || target != node.Target)
        {
            changed = true;
            return node with { Source = source, Target = target };
        }

        return node;
    }

    private XPathAstNode OptimizeLookup(LookupNode node, ref bool changed)
    {
        var expr = OptimizeNode(node.Expression, ref changed);
        var key = OptimizeNode(node.Key, ref changed);

        if (expr != node.Expression || key != node.Key)
        {
            changed = true;
            return node with { Expression = expr, Key = key };
        }

        return node;
    }

    private XPathAstNode OptimizeLookupWildcard(LookupWildcardNode node, ref bool changed)
    {
        var expr = OptimizeNode(node.Expression, ref changed);

        if (expr != node.Expression)
        {
            changed = true;
            return node with { Expression = expr };
        }

        return node;
    }

    // ------------------------------------------------------------------
    // Type expressions
    // ------------------------------------------------------------------

    private XPathAstNode OptimizeCast(CastNode node, ref bool changed)
    {
        var expr = OptimizeNode(node.Expression, ref changed);
        if (expr != node.Expression)
        {
            changed = true;
            node = node with { Expression = expr };
        }

        // Fold string-to-integer cast
        if (expr is StringLiteralNode s && node.TypeName is "integer" or "int" && node.Prefix == "xs")
        {
            if (int.TryParse(s.Value, out var val))
            {
                changed = true;
                return new IntegerLiteralNode(val);
            }
        }

        return node;
    }

    private XPathAstNode OptimizeCastable(CastableNode node, ref bool changed)
    {
        var expr = OptimizeNode(node.Expression, ref changed);
        if (expr != node.Expression) { changed = true; return node with { Expression = expr }; }
        return node;
    }

    private XPathAstNode OptimizeInstanceOf(InstanceOfNode node, ref bool changed)
    {
        var expr = OptimizeNode(node.Expression, ref changed);
        if (expr != node.Expression) { changed = true; return node with { Expression = expr }; }
        return node;
    }

    private XPathAstNode OptimizeTreat(TreatNode node, ref bool changed)
    {
        var expr = OptimizeNode(node.Expression, ref changed);
        if (expr != node.Expression) { changed = true; return node with { Expression = expr }; }
        return node;
    }

    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    private XPathAstNode OptimizeMap(MapConstructorNode node, ref bool changed)
    {
        var entries = new List<MapEntryNode>();
        foreach (var e in node.Entries)
        {
            var key = OptimizeNode(e.Key, ref changed);
            var value = OptimizeNode(e.Value, ref changed);
            entries.Add(key == e.Key && value == e.Value ? e : new MapEntryNode(key, value));
        }

        if (!entries.SequenceEqual(node.Entries))
        {
            changed = true;
            return node with { Entries = entries };
        }

        return node;
    }

    private XPathAstNode OptimizeArray(ArrayConstructorNode node, ref bool changed)
    {
        var items = new List<XPathAstNode>();
        foreach (var item in node.Items)
        {
            items.Add(OptimizeNode(item, ref changed));
        }

        if (!items.SequenceEqual(node.Items))
        {
            changed = true;
            return node with { Items = items };
        }

        return node;
    }

    private XPathAstNode OptimizeInline(InlineFunctionNode node, ref bool changed)
    {
        var body = OptimizeNode(node.Body, ref changed);
        if (body != node.Body)
        {
            changed = true;
            return node with { Body = body };
        }
        return node;
    }

    private XPathAstNode OptimizeLet(LetExpressionNode node, ref bool changed)
    {
        bool bindingsChanged = false;
        var newBindings = new List<QuantifiedBinding>(node.Bindings.Count);
        foreach (var binding in node.Bindings)
        {
            var optExpr = OptimizeNode(binding.Expression, ref changed);
            newBindings.Add(new QuantifiedBinding(binding.VariableName, optExpr));
            if (optExpr != binding.Expression) bindingsChanged = true;
        }
        var body = OptimizeNode(node.Body, ref changed);
        if (bindingsChanged || body != node.Body)
        {
            changed = true;
            return node with { Bindings = newBindings, Body = body };
        }
        return node;
    }

    private XPathAstNode OptimizeDynamicFunctionCall(DynamicFunctionCallNode node, ref bool changed)
    {
        var func = OptimizeNode(node.Function, ref changed);
        bool argsChanged = false;
        var newArgs = new List<XPathAstNode>(node.Arguments.Count);
        foreach (var arg in node.Arguments)
        {
            var optimized = OptimizeNode(arg, ref changed);
            newArgs.Add(optimized);
            if (optimized != arg) argsChanged = true;
        }
        if (func != node.Function || argsChanged)
        {
            changed = true;
            return node with { Function = func, Arguments = newArgs };
        }
        return node;
    }

    private XPathAstNode OptimizeFunctionCall(FunctionCallNode node, ref bool changed)
    {
        // Optimize arguments
        var args = node.Arguments;
        bool argsChanged = false;
        var newArgs = new List<XPathAstNode>(args.Count);
        foreach (var arg in args)
        {
            var optimized = OptimizeNode(arg, ref changed);
            newArgs.Add(optimized);
            if (optimized != arg) argsChanged = true;
        }

        if (argsChanged)
        {
            changed = true;
            node = node with { Arguments = newArgs };
        }

        // Fold common functions
        if (string.IsNullOrEmpty(node.Prefix) && node.LocalName == "true" && args.Count == 0)
        {
            changed = true;
            return new BooleanLiteralNode(true) { Span = node.Span };
        }

        if (string.IsNullOrEmpty(node.Prefix) && node.LocalName == "false" && args.Count == 0)
        {
            changed = true;
            return new BooleanLiteralNode(false) { Span = node.Span };
        }

        if (string.IsNullOrEmpty(node.Prefix) && node.LocalName == "not" && args.Count == 1 && args[0] is BooleanLiteralNode b)
        {
            changed = true;
            return new BooleanLiteralNode(!b.Value) { Span = node.Span };
        }

        return node;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static bool? AsBoolean(XPathAstNode node)
    {
        return node switch
        {
            BooleanLiteralNode b => b.Value,
            IntegerLiteralNode i => i.Value != 0,
            DoubleLiteralNode d => d.Value != 0.0 && !double.IsNaN(d.Value),
            DecimalLiteralNode dec => dec.Value != 0m,
            StringLiteralNode s => !string.IsNullOrEmpty(s.Value),
            _ => null
        };
    }

    private static XPathAstNode TrueLiteral() => new BooleanLiteralNode(true);
    private static XPathAstNode FalseLiteral() => new BooleanLiteralNode(false);
}
