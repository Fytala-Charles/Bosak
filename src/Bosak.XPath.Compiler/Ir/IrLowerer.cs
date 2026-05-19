// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Lowers an optimized XPath AST into register-based IR instructions. Uses a simple stack-like regis...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Fixed function call argument packing into consecutive registers                        |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Added Intersect, Except, and SimpleMap lowering                                        |
//                      | Charles Korthout | 0.4   | 19-05-2026     | Added Map/Array constructor and LookupWildcard lowering                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Diagnostics;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser.Ast;

namespace Bosak.XPath.Compiler.Ir;

/// <summary>
/// Lowers an optimized XPath AST into register-based IR instructions.
/// Uses a simple stack-like register allocation model with a literal pool
/// for constants that don't fit in the instruction operand.
/// </summary>
public sealed class IrLowerer
{
    private readonly List<IrInstruction> _instructions = new();
    private readonly List<object?> _literalPool = new();
    private int _nextRegister;

    public IrModule Lower(XPathAstNode node)
    {
        _instructions.Clear();
        _literalPool.Clear();
        _nextRegister = 0;

        int resultReg = LowerNode(node);
        Emit(IrOpCode.Return, (byte)resultReg);

        return new IrModule(_instructions.ToArray(), _literalPool.ToArray());
    }

    // ------------------------------------------------------------------
    // Main dispatch
    // ------------------------------------------------------------------

    private int LowerNode(XPathAstNode node, int? targetReg = null)
    {
        return node switch
        {
            BooleanLiteralNode n => LowerBooleanLiteral(n, targetReg),
            IntegerLiteralNode n => LowerIntegerLiteral(n, targetReg),
            DecimalLiteralNode n => LowerDecimalLiteral(n, targetReg),
            DoubleLiteralNode n => LowerDoubleLiteral(n, targetReg),
            StringLiteralNode n => LowerStringLiteral(n, targetReg),
            VariableReferenceNode n => LowerVariable(n, targetReg),
            ContextItemNode => LowerContextItem(targetReg),
            ParenthesizedExprNode n => LowerNode(n.Expression, targetReg),
            BinaryExpressionNode n => LowerBinary(n, targetReg),
            UnaryExpressionNode n => LowerUnary(n, targetReg),
            IfExpressionNode n => LowerIf(n, targetReg),
            FunctionCallNode n => LowerFunctionCall(n, targetReg),
            PathExprNode n => LowerPathExpr(n, targetReg),
            StepNode n => LowerStepAsPath(n, targetReg),
            SequenceExpressionNode n => LowerSequence(n, targetReg),
            RangeExpressionNode n => LowerRange(n, targetReg),
            CastNode n => LowerCast(n, targetReg),
            CastableNode n => LowerCastable(n, targetReg),
            InstanceOfNode n => LowerInstanceOf(n, targetReg),
            TreatNode n => LowerTreat(n, targetReg),
            ArrowExprNode n => LowerArrow(n, targetReg),
            NamedFunctionRefNode n => LowerNamedFunctionRef(n, targetReg),
            MapConstructorNode n => LowerMapConstructor(n, targetReg),
            ArrayConstructorNode n => LowerArrayConstructor(n, targetReg),
            LookupNode n => LowerLookup(n, targetReg),
            LookupWildcardNode n => LowerLookupWildcard(n, targetReg),
            ForExpressionNode n => LowerForExpression(n, targetReg),
            QuantifiedExpressionNode n => LowerQuantifiedExpression(n, targetReg),
            PredicateNode n => LowerNode(n.Expression, targetReg),
            PostfixPredicateNode n => LowerPostfixPredicate(n, targetReg),
            _ => throw new NotSupportedException($"AST node type {node.GetType().Name} is not supported by the IR lowerer.")
        };
    }

    // ------------------------------------------------------------------
    // Literals
    // ------------------------------------------------------------------

    private int LowerBooleanLiteral(BooleanLiteralNode node, int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        Emit(IrOpCode.LoadBoolean, (byte)reg, operand: node.Value ? 1 : 0);
        return reg;
    }

    private int LowerIntegerLiteral(IntegerLiteralNode node, int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        int poolIdx = AddToLiteralPool(node.Value);
        Emit(IrOpCode.LoadInteger, (byte)reg, operand: poolIdx);
        return reg;
    }

    private int LowerDecimalLiteral(DecimalLiteralNode node, int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        int poolIdx = AddToLiteralPool(node.Value);
        Emit(IrOpCode.LoadDecimal, (byte)reg, operand: poolIdx);
        return reg;
    }

    private int LowerDoubleLiteral(DoubleLiteralNode node, int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        int poolIdx = AddToLiteralPool(node.Value);
        Emit(IrOpCode.LoadDouble, (byte)reg, operand: poolIdx);
        return reg;
    }

    private int LowerStringLiteral(StringLiteralNode node, int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        int poolIdx = AddToLiteralPool(node.Value);
        Emit(IrOpCode.LoadString, (byte)reg, operand: poolIdx);
        return reg;
    }

    // ------------------------------------------------------------------
    // Variables & Context
    // ------------------------------------------------------------------

    private int LowerVariable(VariableReferenceNode node, int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        string qname = string.IsNullOrEmpty(node.Prefix)
            ? node.LocalName
            : $"{node.Prefix}:{node.LocalName}";
        int poolIdx = AddToLiteralPool(qname);
        Emit(IrOpCode.LoadVariable, (byte)reg, operand: poolIdx);
        return reg;
    }

    private int LowerContextItem(int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        Emit(IrOpCode.LoadContextItem, (byte)reg);
        return reg;
    }

    // ------------------------------------------------------------------
    // Binary expressions
    // ------------------------------------------------------------------

    private int LowerBinary(BinaryExpressionNode node, int? targetReg)
    {
        // Short-circuit boolean operators need special handling
        if (node.Operator == BinaryOperator.And)
            return LowerAnd(node, targetReg);
        if (node.Operator == BinaryOperator.Or)
            return LowerOr(node, targetReg);
        if (node.Operator == BinaryOperator.SimpleMap)
            return LowerSimpleMap(node, targetReg);

        int leftReg = LowerNode(node.Left);
        int rightReg = LowerNode(node.Right);
        int resultReg = targetReg ?? AllocRegister();

        var opcode = node.Operator switch
        {
            BinaryOperator.Plus => IrOpCode.Add,
            BinaryOperator.Minus => IrOpCode.Subtract,
            BinaryOperator.Multiply => IrOpCode.Multiply,
            BinaryOperator.Divide => IrOpCode.Divide,
            BinaryOperator.Idiv => IrOpCode.IntegerDivide,
            BinaryOperator.Mod => IrOpCode.Modulo,
            BinaryOperator.StringConcat => IrOpCode.StringConcat,
            BinaryOperator.Eq => IrOpCode.Equal,
            BinaryOperator.Ne => IrOpCode.NotEqual,
            BinaryOperator.Lt => IrOpCode.LessThan,
            BinaryOperator.Le => IrOpCode.LessThanOrEqual,
            BinaryOperator.Gt => IrOpCode.GreaterThan,
            BinaryOperator.Ge => IrOpCode.GreaterThanOrEqual,
            BinaryOperator.Equal => IrOpCode.GeneralEqual,
            BinaryOperator.NotEqual => IrOpCode.GeneralNotEqual,
            BinaryOperator.LessThan => IrOpCode.GeneralLessThan,
            BinaryOperator.LessThanOrEqual => IrOpCode.GeneralLessThanOrEqual,
            BinaryOperator.GreaterThan => IrOpCode.GeneralGreaterThan,
            BinaryOperator.GreaterThanOrEqual => IrOpCode.GeneralGreaterThanOrEqual,
            BinaryOperator.Is => IrOpCode.IsSameNode,
            BinaryOperator.Precedes => IrOpCode.PrecedesNode,
            BinaryOperator.Follows => IrOpCode.FollowsNode,
            BinaryOperator.To => IrOpCode.Range,
            BinaryOperator.Union => IrOpCode.Concatenate,
            BinaryOperator.Intersect => IrOpCode.Intersect,
            BinaryOperator.Except => IrOpCode.Except,
            _ => throw new NotSupportedException($"Binary operator {node.Operator} is not supported by the IR lowerer.")
        };

        Emit(opcode, (byte)resultReg, (byte)leftReg, (byte)rightReg);
        return resultReg;
    }

    private int LowerAnd(BinaryExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        // Evaluate left
        int leftReg = LowerNode(node.Left, resultReg);
        if (leftReg != resultReg)
            Emit(IrOpCode.Move, (byte)resultReg, (byte)leftReg);

        // If left is false, skip right and return false
        int jumpToEnd = EmitJumpPlaceholder(IrOpCode.JumpIfFalse, (byte)resultReg);

        // Evaluate right
        int rightReg = LowerNode(node.Right, resultReg);
        if (rightReg != resultReg)
            Emit(IrOpCode.Move, (byte)resultReg, (byte)rightReg);

        // Patch jump to end
        PatchJump(jumpToEnd, CurrentInstructionIndex);

        return resultReg;
    }

    private int LowerOr(BinaryExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        // Evaluate left
        int leftReg = LowerNode(node.Left, resultReg);
        if (leftReg != resultReg)
            Emit(IrOpCode.Move, (byte)resultReg, (byte)leftReg);

        // If left is true, skip right and return true
        int jumpToEnd = EmitJumpPlaceholder(IrOpCode.JumpIfTrue, (byte)resultReg);

        // Evaluate right
        int rightReg = LowerNode(node.Right, resultReg);
        if (rightReg != resultReg)
            Emit(IrOpCode.Move, (byte)resultReg, (byte)rightReg);

        // Patch jump to end
        PatchJump(jumpToEnd, CurrentInstructionIndex);

        return resultReg;
    }

    private int LowerSimpleMap(BinaryExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        int leftReg = LowerNode(node.Left);

        // Emit SimpleMap instruction with placeholder for RHS entry point
        int simpleMapInstrIdx = _instructions.Count;
        Emit(IrOpCode.SimpleMap, (byte)resultReg, (byte)leftReg, 0, 0); // placeholder

        // Jump over RHS code (so it doesn't execute during fall-through)
        int jumpInstrIdx = _instructions.Count;
        Emit(IrOpCode.Jump, 0, 0, 0, 0); // placeholder

        // RHS entry point
        int rhsEntry = _instructions.Count;

        // The SimpleMap instruction sets the context item before jumping here.
        int rhsReg = LowerNode(node.Right);
        Emit(IrOpCode.Return, (byte)rhsReg);

        // Patch instructions
        int afterRhs = _instructions.Count;
        PatchInstruction(simpleMapInstrIdx, IrOpCode.SimpleMap, (byte)resultReg, (byte)leftReg, 0, rhsEntry);
        PatchInstruction(jumpInstrIdx, IrOpCode.Jump, 0, 0, 0, afterRhs);

        return resultReg;
    }

    // ------------------------------------------------------------------
    // Unary expressions
    // ------------------------------------------------------------------

    private int LowerUnary(UnaryExpressionNode node, int? targetReg)
    {
        int operandReg = LowerNode(node.Operand);
        int resultReg = targetReg ?? AllocRegister();

        switch (node.Operator)
        {
            case UnaryOperator.Plus:
                // Unary plus is a no-op; just move the value
                Emit(IrOpCode.Move, (byte)resultReg, (byte)operandReg);
                break;
            case UnaryOperator.Minus:
                Emit(IrOpCode.UnaryMinus, (byte)resultReg, (byte)operandReg);
                break;
            default:
                throw new NotSupportedException($"Unary operator {node.Operator} is not supported.");
        }

        return resultReg;
    }

    // ------------------------------------------------------------------
    // If expressions
    // ------------------------------------------------------------------

    private int LowerIf(IfExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        // Condition
        int condReg = LowerNode(node.Condition);

        // Jump to else if false
        int jumpToElse = EmitJumpPlaceholder(IrOpCode.JumpIfFalse, (byte)condReg);

        // Then branch
        int thenReg = LowerNode(node.ThenBranch, resultReg);
        if (thenReg != resultReg)
            Emit(IrOpCode.Move, (byte)resultReg, (byte)thenReg);

        // Jump over else
        int jumpToEnd = EmitJumpPlaceholder(IrOpCode.Jump);

        // Else branch
        int elseLabel = CurrentInstructionIndex;
        PatchJump(jumpToElse, elseLabel);

        int elseReg = LowerNode(node.ElseBranch, resultReg);
        if (elseReg != resultReg)
            Emit(IrOpCode.Move, (byte)resultReg, (byte)elseReg);

        // End
        int endLabel = CurrentInstructionIndex;
        PatchJump(jumpToEnd, endLabel);

        return resultReg;
    }

    // ------------------------------------------------------------------
    // Function calls
    // ------------------------------------------------------------------

    private int LowerFunctionCall(FunctionCallNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        string qname = string.IsNullOrEmpty(node.Prefix)
            ? node.LocalName
            : $"{node.Prefix}:{node.LocalName}";
        int funcPoolIdx = AddToLiteralPool(qname);

        int argCount = node.Arguments.Count;
        int firstArgReg = 0;

        if (argCount > 0)
        {
            // Evaluate each argument (may allocate scratch registers internally)
            var argRegs = new int[argCount];
            argRegs[0] = LowerNode(node.Arguments[0]);
            for (int i = 1; i < argCount; i++)
            {
                argRegs[i] = LowerNode(node.Arguments[i]);
            }

            // Check whether the argument result registers are already consecutive
            bool consecutive = true;
            for (int i = 1; i < argCount; i++)
            {
                if (argRegs[i] != argRegs[0] + i)
                {
                    consecutive = false;
                    break;
                }
            }

            if (consecutive)
            {
                firstArgReg = argRegs[0];
            }
            else
            {
                // Repack arguments into a consecutive register block for the VM Call opcode
                firstArgReg = AllocRegister();
                Emit(IrOpCode.Move, (byte)firstArgReg, (byte)argRegs[0]);
                for (int i = 1; i < argCount; i++)
                {
                    int packedReg = AllocRegister();
                    Emit(IrOpCode.Move, (byte)packedReg, (byte)argRegs[i]);
                }
            }
        }

        Emit(IrOpCode.Call, (byte)resultReg, (byte)firstArgReg, (byte)argCount, funcPoolIdx);
        return resultReg;
    }

    private int LowerNamedFunctionRef(NamedFunctionRefNode node, int? targetReg)
    {
        // Named function reference like fn:abs#1 - we treat it as loading a function value
        int resultReg = targetReg ?? AllocRegister();
        string qname = string.IsNullOrEmpty(node.Prefix)
            ? node.LocalName
            : $"{node.Prefix}:{node.LocalName}";
        // Store as a tuple in the literal pool for now
        int poolIdx = AddToLiteralPool((qname, node.Arity));
        Emit(IrOpCode.LoadVariable, (byte)resultReg, operand: poolIdx);
        return resultReg;
    }

    // ------------------------------------------------------------------
    // Path expressions
    // ------------------------------------------------------------------

    private int LowerStepAsPath(StepNode node, int? targetReg)
    {
        int contextReg = AllocRegister();
        Emit(IrOpCode.LoadContextItem, (byte)contextReg);
        int resultReg = LowerStep(node, contextReg);

        if (targetReg.HasValue && targetReg.Value != resultReg)
        {
            Emit(IrOpCode.Move, (byte)targetReg.Value, (byte)resultReg);
            return targetReg.Value;
        }

        return resultReg;
    }

    private int LowerPathExpr(PathExprNode node, int? targetReg)
    {
        int contextReg = AllocRegister();
        Emit(IrOpCode.LoadContextItem, (byte)contextReg);

        int currentReg = contextReg;
        foreach (var step in node.Steps)
        {
            currentReg = LowerStep((StepNode)step, currentReg);
        }

        if (targetReg.HasValue && targetReg.Value != currentReg)
        {
            Emit(IrOpCode.Move, (byte)targetReg.Value, (byte)currentReg);
            return targetReg.Value;
        }

        return currentReg;
    }

    private int LowerStep(StepNode node, int contextReg)
    {
        // Emit axis instruction
        int axisReg = AllocRegister();
        var axisOpcode = GetAxisOpcode(node.Axis);
        Emit(axisOpcode, (byte)axisReg, (byte)contextReg);

        // Emit name test if present
        int afterTestReg = axisReg;
        if (node.NodeTest.Kind != NameTestKind.AnyName)
        {
            afterTestReg = AllocRegister();
            int namePoolIdx = -1;

            if (node.NodeTest.Kind == NameTestKind.LocalName || node.NodeTest.Kind == NameTestKind.PrefixedName)
            {
                namePoolIdx = AddToLiteralPool(node.NodeTest.Name);
            }
            else if (node.NodeTest.Kind == NameTestKind.QName && !string.IsNullOrEmpty(node.NodeTest.Name))
            {
                namePoolIdx = AddToLiteralPool(node.NodeTest.Name);
            }
            else if (node.NodeTest.Kind == NameTestKind.KindTest)
            {
                namePoolIdx = AddToLiteralPool(node.NodeTest.Name ?? "node");
                Emit(IrOpCode.KindTest, (byte)afterTestReg, (byte)axisReg, operand: namePoolIdx);
                axisReg = afterTestReg;
            }

            if (node.NodeTest.Kind != NameTestKind.KindTest)
            {
                Emit(IrOpCode.NameTest, (byte)afterTestReg, (byte)axisReg, operand: namePoolIdx);
            }
        }

        // Emit predicates
        int resultReg = afterTestReg;
        foreach (var pred in node.Predicates)
        {
            if (pred is PredicateNode pn)
            {
                resultReg = EmitPredicateFilter(resultReg, pn.Expression);
            }
            else
            {
                resultReg = EmitPredicateFilter(resultReg, pred);
            }
        }

        return resultReg;
    }

    private int LowerPostfixPredicate(PostfixPredicateNode node, int? targetReg)
    {
        int baseReg = LowerNode(node.Expression);
        int resultReg = EmitPredicateFilter(baseReg, node.Predicate);

        if (targetReg.HasValue && targetReg.Value != resultReg)
        {
            Emit(IrOpCode.Move, (byte)targetReg.Value, (byte)resultReg);
            return targetReg.Value;
        }

        return resultReg;
    }

    private int EmitPredicateFilter(int sequenceReg, XPathAstNode predicateExpr)
    {
        int resultReg = AllocRegister();

        // Check if this is a numeric subscript like [1] or [last()]
        if (predicateExpr is IntegerLiteralNode subscript)
        {
            Emit(IrOpCode.Subscript, (byte)resultReg, (byte)sequenceReg, operand: (int)subscript.Value);
            return resultReg;
        }

        if (predicateExpr is FunctionCallNode fc &&
            string.IsNullOrEmpty(fc.Prefix) &&
            fc.LocalName == "last" &&
            fc.Arguments.Count == 0)
        {
            Emit(IrOpCode.Last, (byte)resultReg, (byte)sequenceReg);
            return resultReg;
        }

        if (predicateExpr is FunctionCallNode fcPos &&
            string.IsNullOrEmpty(fcPos.Prefix) &&
            fcPos.LocalName == "position" &&
            fcPos.Arguments.Count == 0)
        {
            // position() as a predicate is always true (non-zero position)
            // This should have been optimized away, but handle it anyway
            Emit(IrOpCode.Move, (byte)resultReg, (byte)sequenceReg);
            return resultReg;
        }

        // General predicate: emit Filter instruction with inline predicate code
        int filterInstrIdx = _instructions.Count;
        Emit(IrOpCode.Filter, (byte)resultReg, (byte)sequenceReg, 0, 0); // placeholder

        // Jump over predicate code (so it doesn't execute during fall-through)
        int jumpInstrIdx = _instructions.Count;
        Emit(IrOpCode.Jump, 0, 0, 0, 0); // placeholder

        // Predicate entry point
        int predicateEntry = _instructions.Count;

        // The Filter instruction sets the context item before jumping here.
        // The predicate expression is evaluated with that context.
        int predicateReg = LowerNode(predicateExpr);
        Emit(IrOpCode.Return, (byte)predicateReg);

        // Patch the Filter instruction to point to predicate entry
        int afterPredicate = _instructions.Count;
        PatchInstruction(filterInstrIdx, IrOpCode.Filter, (byte)resultReg, (byte)sequenceReg, 0, predicateEntry);
        PatchInstruction(jumpInstrIdx, IrOpCode.Jump, 0, 0, 0, afterPredicate);

        return resultReg;
    }

    private static IrOpCode GetAxisOpcode(XdmAxis axis)
    {
        return axis switch
        {
            XdmAxis.Child => IrOpCode.Child,
            XdmAxis.Descendant => IrOpCode.Descendant,
            XdmAxis.DescendantOrSelf => IrOpCode.DescendantOrSelf,
            XdmAxis.Ancestor => IrOpCode.Ancestor,
            XdmAxis.AncestorOrSelf => IrOpCode.AncestorOrSelf,
            XdmAxis.Attribute => IrOpCode.Attribute,
            XdmAxis.Parent => IrOpCode.Parent,
            XdmAxis.Self => IrOpCode.Self,
            XdmAxis.Following => IrOpCode.Following,
            XdmAxis.FollowingSibling => IrOpCode.FollowingSibling,
            XdmAxis.Preceding => IrOpCode.Preceding,
            XdmAxis.PrecedingSibling => IrOpCode.PrecedingSibling,
            XdmAxis.Namespace => IrOpCode.Namespace,
            _ => throw new NotSupportedException($"Axis {axis} is not supported.")
        };
    }

    // ------------------------------------------------------------------
    // Sequences
    // ------------------------------------------------------------------

    private int LowerSequence(SequenceExpressionNode node, int? targetReg)
    {
        if (node.Expressions.Count == 0)
        {
            int reg = targetReg ?? AllocRegister();
            Emit(IrOpCode.LoadEmptySequence, (byte)reg);
            return reg;
        }

        if (node.Expressions.Count == 1)
        {
            return LowerNode(node.Expressions[0], targetReg);
        }

        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.SequenceStart, (byte)resultReg);

        foreach (var expr in node.Expressions)
        {
            int itemReg = LowerNode(expr);
            Emit(IrOpCode.SequenceAdd, (byte)resultReg, (byte)itemReg);
        }

        Emit(IrOpCode.SequenceEnd, (byte)resultReg);
        return resultReg;
    }

    private int LowerRange(RangeExpressionNode node, int? targetReg)
    {
        int fromReg = LowerNode(node.From);
        int toReg = LowerNode(node.To);
        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.Range, (byte)resultReg, (byte)fromReg, (byte)toReg);
        return resultReg;
    }

    // ------------------------------------------------------------------
    // Type expressions
    // ------------------------------------------------------------------

    private int LowerCast(CastNode node, int? targetReg)
    {
        int exprReg = LowerNode(node.Expression);
        int resultReg = targetReg ?? AllocRegister();
        string typeName = string.IsNullOrEmpty(node.Prefix) ? node.TypeName : $"{node.Prefix}:{node.TypeName}";
        int poolIdx = AddToLiteralPool(typeName);
        Emit(IrOpCode.Cast, (byte)resultReg, (byte)exprReg, operand: poolIdx);
        return resultReg;
    }

    private int LowerCastable(CastableNode node, int? targetReg)
    {
        int exprReg = LowerNode(node.Expression);
        int resultReg = targetReg ?? AllocRegister();
        string typeName = string.IsNullOrEmpty(node.Prefix) ? node.TypeName : $"{node.Prefix}:{node.TypeName}";
        int poolIdx = AddToLiteralPool(typeName);
        Emit(IrOpCode.Castable, (byte)resultReg, (byte)exprReg, operand: poolIdx);
        return resultReg;
    }

    private int LowerInstanceOf(InstanceOfNode node, int? targetReg)
    {
        int exprReg = LowerNode(node.Expression);
        int resultReg = targetReg ?? AllocRegister();
        string typeName = string.IsNullOrEmpty(node.Prefix) ? node.TypeName : $"{node.Prefix}:{node.TypeName}";
        int poolIdx = AddToLiteralPool(typeName);
        Emit(IrOpCode.InstanceOf, (byte)resultReg, (byte)exprReg, operand: poolIdx);
        return resultReg;
    }

    private int LowerTreat(TreatNode node, int? targetReg)
    {
        int exprReg = LowerNode(node.Expression);
        int resultReg = targetReg ?? AllocRegister();
        string typeName = string.IsNullOrEmpty(node.Prefix) ? node.TypeName : $"{node.Prefix}:{node.TypeName}";
        int poolIdx = AddToLiteralPool(typeName);
        Emit(IrOpCode.TreatAs, (byte)resultReg, (byte)exprReg, operand: poolIdx);
        return resultReg;
    }

    // ------------------------------------------------------------------
    // XPath 3.1 additions
    // ------------------------------------------------------------------

    private int LowerArrow(ArrowExprNode node, int? targetReg)
    {
        // $x => fn()  desugars to  fn($x)
        int sourceReg = LowerNode(node.Source);
        var target = node.Target;

        // Prepend source as first argument
        var args = new List<XPathAstNode>(target.Arguments.Count + 1);
        args.Add(new ContextItemNode { Span = node.Span }); // placeholder - we'll push sourceReg instead

        // Actually, we can't easily modify the AST here. Instead, we'll emit the call manually.
        int resultReg = targetReg ?? AllocRegister();
        string qname = string.IsNullOrEmpty(target.Prefix)
            ? target.LocalName
            : $"{target.Prefix}:{target.LocalName}";
        int funcPoolIdx = AddToLiteralPool(qname);

        int argCount = target.Arguments.Count + 1;

        // Evaluate remaining arguments (may allocate scratch registers internally)
        var argRegs = new int[argCount];
        argRegs[0] = sourceReg;
        for (int i = 0; i < target.Arguments.Count; i++)
        {
            argRegs[i + 1] = LowerNode(target.Arguments[i]);
        }

        // Check whether the argument result registers are already consecutive
        bool consecutive = true;
        for (int i = 1; i < argCount; i++)
        {
            if (argRegs[i] != argRegs[0] + i)
            {
                consecutive = false;
                break;
            }
        }

        int firstArgReg;
        if (consecutive)
        {
            firstArgReg = argRegs[0];
        }
        else
        {
            // Repack arguments into a consecutive register block for the VM Call opcode
            firstArgReg = AllocRegister();
            Emit(IrOpCode.Move, (byte)firstArgReg, (byte)argRegs[0]);
            for (int i = 1; i < argCount; i++)
            {
                int packedReg = AllocRegister();
                Emit(IrOpCode.Move, (byte)packedReg, (byte)argRegs[i]);
            }
        }

        Emit(IrOpCode.Call, (byte)resultReg, (byte)firstArgReg, (byte)argCount, funcPoolIdx);
        return resultReg;
    }

    private int LowerLookup(LookupNode node, int? targetReg)
    {
        int exprReg = LowerNode(node.Expression);
        int keyReg = LowerNode(node.Key);
        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.Lookup, (byte)resultReg, (byte)exprReg, (byte)keyReg);
        return resultReg;
    }

    private int LowerLookupWildcard(LookupWildcardNode node, int? targetReg)
    {
        int exprReg = LowerNode(node.Expression);
        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.LookupWildcard, (byte)resultReg, (byte)exprReg);
        return resultReg;
    }

    private int LowerMapConstructor(MapConstructorNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.Map, (byte)resultReg);

        foreach (var entry in node.Entries)
        {
            int keyReg = LowerNode(entry.Key);
            int valueReg = LowerNode(entry.Value);
            Emit(IrOpCode.MapAdd, (byte)resultReg, (byte)keyReg, (byte)valueReg);
        }

        return resultReg;
    }

    private int LowerArrayConstructor(ArrayConstructorNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.Array, (byte)resultReg);

        foreach (var item in node.Items)
        {
            int itemReg = LowerNode(item);
            Emit(IrOpCode.ArrayAdd, (byte)resultReg, (byte)itemReg);
        }

        return resultReg;
    }

    // ------------------------------------------------------------------
    // FLWOR
    // ------------------------------------------------------------------

    private int LowerForExpression(ForExpressionNode node, int? targetReg)
    {
        // For now, lower as a sequence construction loop
        // This is a simplified implementation
        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.SequenceStart, (byte)resultReg);

        if (node.Bindings.Count == 1)
        {
            var binding = node.Bindings[0];
            int seqReg = LowerNode(binding.Expression);

            // Emit a loop: for each item in seqReg, bind to variable, evaluate return
            // This requires loop opcodes we don't have yet.
            // For now, emit a placeholder Call.
            int bindingPoolIdx = AddToLiteralPool(("__for", binding.VariableName));
            Emit(IrOpCode.Call, (byte)resultReg, (byte)seqReg, 1, bindingPoolIdx);
        }
        else
        {
            // Multiple bindings - cartesian product
            // Emit placeholder
            int poolIdx = AddToLiteralPool("__for_multi");
            Emit(IrOpCode.Call, (byte)resultReg, 0, 0, poolIdx);
        }

        Emit(IrOpCode.SequenceEnd, (byte)resultReg);
        return resultReg;
    }

    private int LowerQuantifiedExpression(QuantifiedExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        if (node.Bindings.Count == 1)
        {
            var binding = node.Bindings[0];
            int seqReg = LowerNode(binding.Expression);

            string opName = node.Quantifier == QuantifierKind.Some ? "__some" : "__every";
            int poolIdx = AddToLiteralPool((opName, binding.VariableName));
            Emit(IrOpCode.Call, (byte)resultReg, (byte)seqReg, 1, poolIdx);
        }
        else
        {
            string opName = node.Quantifier == QuantifierKind.Some ? "__some_multi" : "__every_multi";
            int poolIdx = AddToLiteralPool(opName);
            Emit(IrOpCode.Call, (byte)resultReg, 0, 0, poolIdx);
        }

        return resultReg;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private int AllocRegister()
    {
        Debug.Assert(_nextRegister <= 255, "Register overflow: more than 255 registers allocated.");
        return _nextRegister++;
    }

    private int AddToLiteralPool(object? value)
    {
        int idx = _literalPool.Count;
        _literalPool.Add(value);
        return idx;
    }

    private int CurrentInstructionIndex => _instructions.Count;

    private void Emit(IrOpCode op, byte regA = 0, byte regB = 0, byte regC = 0, int operand = 0)
    {
        _instructions.Add(new IrInstruction(op, regA, regB, regC, operand));
    }

    private int EmitJumpPlaceholder(IrOpCode jumpOp, byte regA = 0)
    {
        Debug.Assert(jumpOp is IrOpCode.Jump or IrOpCode.JumpIfTrue or IrOpCode.JumpIfFalse or IrOpCode.JumpIfEmpty);
        int idx = _instructions.Count;
        _instructions.Add(new IrInstruction(jumpOp, regA, 0, 0, 0));
        return idx;
    }

    private void PatchJump(int instructionIndex, int targetIndex)
    {
        var instr = _instructions[instructionIndex];
        _instructions[instructionIndex] = new IrInstruction(instr.OpCode, instr.RegisterA, instr.RegisterB, instr.RegisterC, targetIndex);
    }

    private void PatchInstruction(int index, IrOpCode op, byte regA, byte regB = 0, byte regC = 0, int operand = 0)
    {
        _instructions[index] = new IrInstruction(op, regA, regB, regC, operand);
    }
}
