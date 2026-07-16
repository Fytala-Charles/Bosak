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
//                      | Charles Korthout | 0.5   | 19-05-2026     | Lower occurrence indicators in type expressions to RegisterC                           |
//                      | Charles Korthout | 0.6   | 19-05-2026     | Nested lowering for multi-binding for/some/every expressions                           |
//                      | Charles Korthout | 0.7   | 19-05-2026     | Unwrap PredicateNode in LowerPostfixPredicate for Subscript/Last compilation           |
//                      | Charles Korthout | 0.8   | 19-05-2026     | Support filter expressions as path steps (e.g. parse-xml(...)/root/item)               |
//                      | Charles Korthout | 0.9   | 24-05-2026     | Fix * name test to filter by principal node kind (element/attribute/namespace)         |
//                      | Charles Korthout | 1.0   | 27-05-2026     | Emit DocumentRoot for absolute path expressions                                        |
//                      | Charles Korthout | 1.1   | 30-05-2026     | Emit NamespaceTest for QName node tests (prefix:localname)                             |
//                      | Charles Korthout | 1.2   | 30-05-2026     | Wrap predicated path steps in PathStepMap for per-context-item predicate evaluation   |
//                      | Charles Korthout | 1.3   | 01-06-2026     | Use SimpleMap for non-StepNode steps in path expressions (e.g. /a/b/number())        |
//                      | Charles Korthout | 1.4   | 01-06-2026     | Expanded register encoding from byte to ushort; removed 255-register limit             |
//                      | Charles Korthout | 1.5   | 25-06-2026     | Only emit LoadContextItem for path expressions that actually reference the focus       |
//                      | Charles Korthout | 1.6   | 25-06-2026     | Named node tests on element-principal axes filter to element kind first                |
//                      | Charles Korthout | 1.7   | 26-06-2026     | Lower Q{uri}* URI-qualified wildcards to NamespaceTest                                  |
//                      | Charles Korthout | 1.8   | 13-07-2026     | Partial application (placeholders) for dynamic function calls (higher-order-func-045)   |
//                      | Charles Korthout | 1.9   | 15-07-2026     | QuantifiedLoopInfo carries optional positional variable for FLWOR 'at $pos'             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Diagnostics;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser.Ast;

namespace Bosak.XPath.Compiler.Ir;

/// <summary>
/// Loop information stored in the literal pool for For/Some/Every opcodes.
/// </summary>
public readonly record struct QuantifiedLoopInfo(string VariableName, int RhsEntryPoint, string? PositionalVariableName = null);

/// <summary>
/// Try/catch information stored in the literal pool for the TryCatch opcode.
/// </summary>
public readonly record struct TryCatchInfo(int TryEntryPoint, int CatchEntryPoint);

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
    private readonly Stack<int> _freeRegisters = new();

    public IrModule Lower(XPathAstNode node)
    {
        _instructions.Clear();
        _literalPool.Clear();
        _freeRegisters.Clear();
        _nextRegister = 0;

        int resultReg = LowerNode(node);
        Emit(IrOpCode.Return, (ushort)resultReg);

        return new IrModule(_instructions.ToArray(), _literalPool.ToArray(), _nextRegister);
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
            TryCatchNode n => LowerTryCatch(n, targetReg),
            LetExpressionNode n => LowerLetExpression(n, targetReg),
            InlineFunctionNode n => LowerInlineFunction(n, targetReg),
            DynamicFunctionCallNode n => LowerDynamicFunctionCall(n, targetReg),
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
        Emit(IrOpCode.LoadBoolean, (ushort)reg, operand: node.Value ? 1 : 0);
        return reg;
    }

    private int LowerIntegerLiteral(IntegerLiteralNode node, int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        int poolIdx = AddToLiteralPool(node.Value);
        Emit(IrOpCode.LoadInteger, (ushort)reg, operand: poolIdx);
        return reg;
    }

    private int LowerDecimalLiteral(DecimalLiteralNode node, int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        int poolIdx = AddToLiteralPool(node.Value);
        Emit(IrOpCode.LoadDecimal, (ushort)reg, operand: poolIdx);
        return reg;
    }

    private int LowerDoubleLiteral(DoubleLiteralNode node, int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        int poolIdx = AddToLiteralPool(node.Value);
        Emit(IrOpCode.LoadDouble, (ushort)reg, operand: poolIdx);
        return reg;
    }

    private int LowerStringLiteral(StringLiteralNode node, int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        int poolIdx = AddToLiteralPool(node.Value);
        Emit(IrOpCode.LoadString, (ushort)reg, operand: poolIdx);
        return reg;
    }

    // ------------------------------------------------------------------
    // Variables & Context
    // ------------------------------------------------------------------

    private int LowerVariable(VariableReferenceNode node, int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        int poolIdx;
        if (!string.IsNullOrEmpty(node.NamespaceUri))
        {
            poolIdx = AddToLiteralPool((node.LocalName, node.NamespaceUri));
        }
        else
        {
            string qname = string.IsNullOrEmpty(node.Prefix)
                ? node.LocalName
                : $"{node.Prefix}:{node.LocalName}";
            poolIdx = AddToLiteralPool(qname);
        }
        Emit(IrOpCode.LoadVariable, (ushort)reg, operand: poolIdx);
        return reg;
    }

    private int LowerContextItem(int? targetReg)
    {
        int reg = targetReg ?? AllocRegister();
        Emit(IrOpCode.LoadContextItem, (ushort)reg);
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

        Emit(opcode, (ushort)resultReg, (ushort)leftReg, (ushort)rightReg);
        FreeRegister(leftReg);
        FreeRegister(rightReg);
        return resultReg;
    }

    private int LowerAnd(BinaryExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        // Evaluate left
        int leftReg = LowerNode(node.Left, resultReg);
        if (leftReg != resultReg)
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)leftReg);
        FreeRegister(leftReg);

        // If left is false, result is false
        int jumpToFalse = EmitJumpPlaceholder(IrOpCode.JumpIfFalse, (ushort)resultReg);

        // Evaluate right
        int rightReg = LowerNode(node.Right, resultReg);
        if (rightReg != resultReg)
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)rightReg);
        FreeRegister(rightReg);

        // If right is false, result is false
        int jumpToFalse2 = EmitJumpPlaceholder(IrOpCode.JumpIfFalse, (ushort)resultReg);

        // Both true: result = true
        Emit(IrOpCode.LoadBoolean, (ushort)resultReg, operand: 1);
        int jumpToEnd = EmitJumpPlaceholder(IrOpCode.Jump);

        // False path
        PatchJump(jumpToFalse, CurrentInstructionIndex);
        PatchJump(jumpToFalse2, CurrentInstructionIndex);
        Emit(IrOpCode.LoadBoolean, (ushort)resultReg, operand: 0);

        // End
        PatchJump(jumpToEnd, CurrentInstructionIndex);

        return resultReg;
    }

    private int LowerOr(BinaryExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        // Evaluate left
        int leftReg = LowerNode(node.Left, resultReg);
        if (leftReg != resultReg)
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)leftReg);
        FreeRegister(leftReg);

        // If left is true, result is true
        int jumpToTrue = EmitJumpPlaceholder(IrOpCode.JumpIfTrue, (ushort)resultReg);

        // Evaluate right
        int rightReg = LowerNode(node.Right, resultReg);
        if (rightReg != resultReg)
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)rightReg);
        FreeRegister(rightReg);

        // If right is true, result is true
        int jumpToTrue2 = EmitJumpPlaceholder(IrOpCode.JumpIfTrue, (ushort)resultReg);

        // Both false: result = false
        Emit(IrOpCode.LoadBoolean, (ushort)resultReg, operand: 0);
        int jumpToEnd = EmitJumpPlaceholder(IrOpCode.Jump);

        // True path
        PatchJump(jumpToTrue, CurrentInstructionIndex);
        PatchJump(jumpToTrue2, CurrentInstructionIndex);
        Emit(IrOpCode.LoadBoolean, (ushort)resultReg, operand: 1);

        // End
        PatchJump(jumpToEnd, CurrentInstructionIndex);

        return resultReg;
    }

    private int LowerSimpleMap(BinaryExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        int leftReg = LowerNode(node.Left);

        // Emit SimpleMap instruction with placeholder for RHS entry point
        int simpleMapInstrIdx = _instructions.Count;
        Emit(IrOpCode.SimpleMap, (ushort)resultReg, (ushort)leftReg, 0, 0); // placeholder

        // Jump over RHS code (so it doesn't execute during fall-through)
        int jumpInstrIdx = _instructions.Count;
        Emit(IrOpCode.Jump, 0, 0, 0, 0); // placeholder

        // RHS entry point
        int rhsEntry = _instructions.Count;

        // The SimpleMap instruction sets the context item before jumping here.
        int rhsReg = LowerNode(node.Right);
        Emit(IrOpCode.Return, (ushort)rhsReg);
        FreeRegister(rhsReg);

        // Patch instructions
        int afterRhs = _instructions.Count;
        PatchInstruction(simpleMapInstrIdx, IrOpCode.SimpleMap, (ushort)resultReg, (ushort)leftReg, 0, rhsEntry);
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
                Emit(IrOpCode.Move, (ushort)resultReg, (ushort)operandReg);
                break;
            case UnaryOperator.Minus:
                Emit(IrOpCode.UnaryMinus, (ushort)resultReg, (ushort)operandReg);
                break;
            default:
                throw new NotSupportedException($"Unary operator {node.Operator} is not supported.");
        }

        FreeRegister(operandReg);
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
        int jumpToElse = EmitJumpPlaceholder(IrOpCode.JumpIfFalse, (ushort)condReg);

        // Then branch
        int thenReg = LowerNode(node.ThenBranch, resultReg);
        if (thenReg != resultReg)
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)thenReg);

        // Jump over else
        int jumpToEnd = EmitJumpPlaceholder(IrOpCode.Jump);

        // Else branch
        int elseLabel = CurrentInstructionIndex;
        PatchJump(jumpToElse, elseLabel);

        int elseReg = LowerNode(node.ElseBranch, resultReg);
        if (elseReg != resultReg)
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)elseReg);

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

        int funcPoolIdx;
        if (!string.IsNullOrEmpty(node.NamespaceUri))
        {
            funcPoolIdx = AddToLiteralPool((node.LocalName, node.NamespaceUri));
        }
        else
        {
            string qname = string.IsNullOrEmpty(node.Prefix)
                ? node.LocalName
                : $"{node.Prefix}:{node.LocalName}";
            funcPoolIdx = AddToLiteralPool(qname);
        }

        int argCount = node.Arguments.Count;

        // Check for argument placeholders (partial application)
        bool hasPlaceholders = false;
        foreach (var arg in node.Arguments)
        {
            if (arg is ArgumentPlaceholderNode)
            {
                hasPlaceholders = true;
                break;
            }
        }

        if (hasPlaceholders)
        {
            // Load the named function as a function item
            int funcReg = AllocRegister();
            int funcItemPoolIdx;
            if (!string.IsNullOrEmpty(node.NamespaceUri))
            {
                funcItemPoolIdx = AddToLiteralPool(new NamedFunctionItem(node.NamespaceUri, node.LocalName, argCount));
            }
            else
            {
                string qname = string.IsNullOrEmpty(node.Prefix)
                    ? node.LocalName
                    : $"{node.Prefix}:{node.LocalName}";
                funcItemPoolIdx = AddToLiteralPool((qname, argCount));
            }
            Emit(IrOpCode.LoadFunction, (ushort)funcReg, operand: funcItemPoolIdx);

            // Evaluate non-placeholder arguments and build descriptor
            var argRegs = new int[argCount];
            var descriptor = new int[argCount];
            for (int i = 0; i < argCount; i++)
            {
                if (node.Arguments[i] is ArgumentPlaceholderNode)
                {
                    descriptor[i] = -1;
                }
                else
                {
                    argRegs[i] = LowerNode(node.Arguments[i]);
                    descriptor[i] = argRegs[i];
                }
            }

            int descPoolIdx = AddToLiteralPool(descriptor);
            Emit(IrOpCode.Curry, (ushort)resultReg, (ushort)funcReg, operand: descPoolIdx);
            FreeRegister(funcReg);
            foreach (var r in argRegs) FreeRegister(r);
            return resultReg;
        }

        int firstArgReg = 0;
        int[]? callArgRegs = null;
        bool consecutive = true;

        if (argCount > 0)
        {
            // Evaluate each argument (may allocate scratch registers internally)
            callArgRegs = new int[argCount];
            callArgRegs[0] = LowerNode(node.Arguments[0]);
            for (int i = 1; i < argCount; i++)
            {
                callArgRegs[i] = LowerNode(node.Arguments[i]);
            }

            // Check whether the argument result registers are already consecutive
            for (int i = 1; i < argCount; i++)
            {
                if (callArgRegs[i] != callArgRegs[0] + i)
                {
                    consecutive = false;
                    break;
                }
            }

            if (consecutive)
            {
                firstArgReg = callArgRegs[0];
            }
            else
            {
                // Repack arguments into a consecutive register block for the VM Call opcode
                firstArgReg = _nextRegister;
                for (int i = 0; i < argCount; i++)
                {
                    Debug.Assert(_nextRegister <= 65535, "Register overflow during argument repacking.");
                    int packedReg = _nextRegister++;
                    Emit(IrOpCode.Move, (ushort)packedReg, (ushort)callArgRegs[i]);
                }
            }
        }

        Emit(IrOpCode.Call, (ushort)resultReg, (ushort)firstArgReg, (ushort)argCount, funcPoolIdx);
        if (callArgRegs != null)
        {
            // Free argument registers after the call
            for (int i = 0; i < argCount; i++)
                FreeRegister(callArgRegs[i]);
            if (!consecutive)
            {
                // Also free the repacked block
                for (int i = 0; i < argCount; i++)
                    FreeRegister(firstArgReg + i);
            }
        }
        return resultReg;
    }

    private int LowerNamedFunctionRef(NamedFunctionRefNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        if (!string.IsNullOrEmpty(node.NamespaceUri))
        {
            int nsPoolIdx = AddToLiteralPool(new NamedFunctionItem(node.NamespaceUri, node.LocalName, node.Arity));
            Emit(IrOpCode.LoadFunction, (ushort)resultReg, operand: nsPoolIdx);
            return resultReg;
        }
        string qname = string.IsNullOrEmpty(node.Prefix)
            ? node.LocalName
            : $"{node.Prefix}:{node.LocalName}";
        var funcTuple = (qname, node.Arity);
        int poolIdx = AddToLiteralPool(funcTuple);
        Emit(IrOpCode.LoadFunction, (ushort)resultReg, operand: poolIdx);
        return resultReg;
    }

    // ------------------------------------------------------------------
    // Path expressions
    // ------------------------------------------------------------------

    private int LowerStepAsPath(StepNode node, int? targetReg)
    {
        int contextReg = AllocRegister();
        Emit(IrOpCode.LoadContextItem, (ushort)contextReg);
        int resultReg = LowerStep(node, contextReg);

        if (targetReg.HasValue && targetReg.Value != resultReg)
        {
            Emit(IrOpCode.Move, (ushort)targetReg.Value, (ushort)resultReg);
            return targetReg.Value;
        }

        return resultReg;
    }

    private int LowerPathExpr(PathExprNode node, int? targetReg)
    {
        // Only load the context item if the path expression actually uses it.
        // A relative path whose first step is a non-axis expression (e.g. $x, parse-xml(...))
        // does not need the focus; loading it would force an XPDY0002 error when the focus
        // is absent and the context item is never referenced.
        bool needsContext = node.IsAbsolute || (node.Steps.Count > 0 && node.Steps[0] is StepNode);
        int contextReg = needsContext ? AllocRegister() : -1;
        if (needsContext)
            Emit(IrOpCode.LoadContextItem, (ushort)contextReg);

        int currentReg = needsContext ? contextReg : -1;
        if (node.IsAbsolute)
        {
            int rootReg = AllocRegister();
            Emit(IrOpCode.DocumentRoot, (ushort)rootReg, (ushort)currentReg);
            FreeRegister(currentReg);
            currentReg = rootReg;
        }

        bool isFirstStep = true;
        foreach (var step in node.Steps)
        {
            if (step is StepNode stepNode)
            {
                currentReg = LowerStep(stepNode, currentReg);
            }
            else
            {
                if (isFirstStep)
                {
                    // First step: evaluate once (e.g., $x, parse-xml(...)).
                    // If the path did not need the context item, let LowerNode allocate
                    // a fresh register rather than passing the sentinel -1.
                    currentReg = LowerNode(step, currentReg == -1 ? null : (int?)currentReg);
                }
                else
                {
                    // Subsequent non-axis step: evaluate per-item using SimpleMap
                    // semantics (e.g., /a/b/number(), /a/b/(1+2))
                    int mapResultReg = AllocRegister();
                    int mapInstrIdx = _instructions.Count;
                    Emit(IrOpCode.SimpleMap, (ushort)mapResultReg, (ushort)currentReg, 1, 0); // placeholder; RegisterC=1 => enforce XPTY0018

                    int jumpInstrIdx = _instructions.Count;
                    Emit(IrOpCode.Jump, 0, 0, 0, 0); // placeholder

                    int rhsEntry = _instructions.Count;
                    int rhsReg = LowerNode(step);
                    Emit(IrOpCode.Return, (ushort)rhsReg);
                    FreeRegister(rhsReg);

                    int afterRhs = _instructions.Count;
                    PatchInstruction(mapInstrIdx, IrOpCode.SimpleMap, (ushort)mapResultReg, (ushort)currentReg, 1, rhsEntry);
                    PatchInstruction(jumpInstrIdx, IrOpCode.Jump, 0, 0, 0, afterRhs);

                    FreeRegister(currentReg);
                    currentReg = mapResultReg;
                }

                // Path expression results must be in document order.
                int normReg = AllocRegister();
                Emit(IrOpCode.Normalize, (ushort)normReg, (ushort)currentReg);
                FreeRegister(currentReg);
                currentReg = normReg;
            }
            isFirstStep = false;
        }

        if (targetReg.HasValue && targetReg.Value != currentReg)
        {
            Emit(IrOpCode.Move, (ushort)targetReg.Value, (ushort)currentReg);
            return targetReg.Value;
        }

        return currentReg;
    }

    private int LowerStep(StepNode node, int contextReg)
    {
        if (node.Predicates.Count > 0)
        {
            // Predicates on a path step must be evaluated per context item,
            // not on the flattened union of all axis results.
            // Wrap the step in PathStepMap so each input node becomes the
            // sole context item while the predicates run.
            int mapResultReg = AllocRegister();
            int mapInstrIdx = _instructions.Count;
            Emit(IrOpCode.PathStepMap, (ushort)mapResultReg, (ushort)contextReg, 0, 0); // placeholder

            int jumpInstrIdx = _instructions.Count;
            Emit(IrOpCode.Jump, 0, 0, 0, 0); // placeholder

            int blockEntry = _instructions.Count;

            // Inner block: single context item -> axis -> name test -> predicates
            int innerCtx = AllocRegister();
            Emit(IrOpCode.LoadContextItem, (ushort)innerCtx);
            int innerResult = LowerStepCore(node, innerCtx);

            foreach (var pred in node.Predicates)
            {
                var predExpr = pred is PredicateNode pn ? pn.Expression : pred;
                innerResult = EmitPredicateFilter(innerResult, predExpr);
            }

            Emit(IrOpCode.Return, (ushort)innerResult);
            FreeRegister(innerCtx);
            FreeRegister(innerResult);

            int afterBlock = _instructions.Count;
            PatchInstruction(mapInstrIdx, IrOpCode.PathStepMap, (ushort)mapResultReg, (ushort)contextReg, 0, blockEntry);
            PatchInstruction(jumpInstrIdx, IrOpCode.Jump, 0, 0, 0, afterBlock);

            // Path expression results must be in document order.
            int normReg = AllocRegister();
            Emit(IrOpCode.Normalize, (ushort)normReg, (ushort)mapResultReg);
            FreeRegister(mapResultReg);
            return normReg;
        }

        int resultReg = LowerStepCore(node, contextReg);
        // Path expression results must be in document order.
        int normReg2 = AllocRegister();
        Emit(IrOpCode.Normalize, (ushort)normReg2, (ushort)resultReg);
        FreeRegister(resultReg);
        return normReg2;
    }

    /// <summary>
    /// Emits axis + name test for a step (no predicates).
    /// </summary>
    private int LowerStepCore(StepNode node, int contextReg)
    {
        // Emit axis instruction
        int axisReg = AllocRegister();
        var axisOpcode = GetAxisOpcode(node.Axis);
        Emit(axisOpcode, (ushort)axisReg, (ushort)contextReg);

        // Emit name test if present
        int afterTestReg = axisReg;
        if (node.NodeTest.Kind != NameTestKind.AnyName)
        {
            afterTestReg = AllocRegister();
            int namePoolIdx = -1;

            if (node.NodeTest.Kind == NameTestKind.LocalName || node.NodeTest.Kind == NameTestKind.PrefixedName)
            {
                namePoolIdx = AddToLiteralPool(node.NodeTest.Name);
                // Unprefixed element names must match the default element namespace.
                // For element axes, emit NamespaceTest with empty prefix so the runtime
                // resolves it to the default element namespace (or empty if none declared).
                // Attribute and namespace axes skip this — unprefixed attribute/namespace
                // names always match no namespace.
                if (node.Axis != XdmAxis.Attribute && node.Axis != XdmAxis.Namespace)
                {
                    int nsPoolIdx = AddToLiteralPool("");
                    Emit(IrOpCode.NamespaceTest, (ushort)afterTestReg, (ushort)axisReg, operand: nsPoolIdx);
                    FreeRegister(axisReg);
                    axisReg = afterTestReg;
                }
            }
            else if (node.NodeTest.Kind == NameTestKind.QName && !string.IsNullOrEmpty(node.NodeTest.Name))
            {
                // For *:local, preserve the wildcard in the name test so the VM knows not to
                // apply the no-namespace restriction for unprefixed attribute names.
                // For any resolved prefix or URI, use the *:local form so the NameTest opcode
                // checks only the local name and lets the preceding NamespaceTest enforce the URI.
                var nameToPool = string.IsNullOrEmpty(node.NodeTest.NamespaceUri) ? node.NodeTest.Name : "*:" + node.NodeTest.Name;
                namePoolIdx = AddToLiteralPool(nameToPool);
                // If a real prefix is present (not the wildcard "*"), emit NamespaceTest
                // to filter by resolved URI.
                if (!string.IsNullOrEmpty(node.NodeTest.NamespaceUri) && node.NodeTest.NamespaceUri != "*")
                {
                    int nsPoolIdx = AddToLiteralPool(node.NodeTest.NamespaceUri); // prefix
                    Emit(IrOpCode.NamespaceTest, (ushort)afterTestReg, (ushort)axisReg, operand: nsPoolIdx);
                    FreeRegister(axisReg);
                    axisReg = afterTestReg;
                }
            }
            else if (node.NodeTest.Kind == NameTestKind.NamespaceAny)
            {
                // prefix:* — emit KindTest for principal node kind, then NamespaceTest.
                afterTestReg = AllocRegister();
                string principalKind = node.Axis switch
                {
                    XdmAxis.Attribute => "attribute",
                    XdmAxis.Namespace => "namespace",
                    _ => "element"
                };
                int kindPoolIdx = AddToLiteralPool(principalKind);
                Emit(IrOpCode.KindTest, (ushort)afterTestReg, (ushort)axisReg, operand: kindPoolIdx);
                FreeRegister(axisReg);
                axisReg = afterTestReg;

                int nsPoolIdx = AddToLiteralPool(node.NodeTest.NamespaceUri ?? node.NodeTest.Name ?? ""); // URI or prefix
                afterTestReg = AllocRegister();
                Emit(IrOpCode.NamespaceTest, (ushort)afterTestReg, (ushort)axisReg, operand: nsPoolIdx);
                FreeRegister(axisReg);
                axisReg = afterTestReg;
            }
            else if (node.NodeTest.Kind == NameTestKind.KindTest)
            {
                namePoolIdx = AddToLiteralPool(node.NodeTest.Name ?? "node");
                Emit(IrOpCode.KindTest, (ushort)afterTestReg, (ushort)axisReg, operand: namePoolIdx);
                FreeRegister(axisReg);
                axisReg = afterTestReg;

                // If the kind test has an argument (e.g. processing-instruction('name')),
                // emit a NameTest to filter by that name.
                if (!string.IsNullOrEmpty(node.NodeTest.KindTestArgument))
                {
                    int argPoolIdx = AddToLiteralPool(node.NodeTest.KindTestArgument);
                    afterTestReg = AllocRegister();
                    Emit(IrOpCode.NameTest, (ushort)afterTestReg, (ushort)axisReg, operand: argPoolIdx);
                    FreeRegister(axisReg);
                    axisReg = afterTestReg;
                }
            }

            if (node.NodeTest.Kind != NameTestKind.KindTest && node.NodeTest.Kind != NameTestKind.NamespaceAny)
            {
                // Named tests on axes whose principal node kind is element (child, descendant,
                // self, following, preceding, etc.) must only match element nodes. Without this
                // filter, a name test such as self::center-attr on an attribute node incorrectly
                // succeeds because the name matches.
                if (node.Axis != XdmAxis.Attribute && node.Axis != XdmAxis.Namespace)
                {
                    int kindReg = AllocRegister();
                    int kindPoolIdx = AddToLiteralPool("element");
                    Emit(IrOpCode.KindTest, (ushort)kindReg, (ushort)axisReg, operand: kindPoolIdx);
                    if (axisReg != afterTestReg)
                        FreeRegister(axisReg);
                    axisReg = kindReg;
                }

                Emit(IrOpCode.NameTest, (ushort)afterTestReg, (ushort)axisReg, operand: namePoolIdx);
            }
            if (axisReg != afterTestReg)
                FreeRegister(axisReg);
        }
        else
        {
            // * is a name test that matches any name of the principal node kind for the axis.
            // Filter by principal node kind since the axis returns all node kinds.
            int oldAxisReg = axisReg;
            afterTestReg = AllocRegister();
            string principalKind = node.Axis switch
            {
                XdmAxis.Attribute => "attribute",
                XdmAxis.Namespace => "namespace",
                _ => "element"
            };
            int kindPoolIdx = AddToLiteralPool(principalKind);
            Emit(IrOpCode.KindTest, (ushort)afterTestReg, (ushort)axisReg, operand: kindPoolIdx);
            FreeRegister(oldAxisReg);
        }

        return afterTestReg;
    }

    private int LowerPostfixPredicate(PostfixPredicateNode node, int? targetReg)
    {
        int baseReg = LowerNode(node.Expression);
        var predExpr = node.Predicate is PredicateNode pn ? pn.Expression : node.Predicate;
        int resultReg = EmitPredicateFilter(baseReg, predExpr);

        if (targetReg.HasValue && targetReg.Value != resultReg)
        {
            Emit(IrOpCode.Move, (ushort)targetReg.Value, (ushort)resultReg);
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
            Emit(IrOpCode.Subscript, (ushort)resultReg, (ushort)sequenceReg, operand: (int)subscript.Value);
            return resultReg;
        }

        if (predicateExpr is FunctionCallNode fc &&
            string.IsNullOrEmpty(fc.Prefix) &&
            fc.LocalName == "last" &&
            fc.Arguments.Count == 0)
        {
            Emit(IrOpCode.Last, (ushort)resultReg, (ushort)sequenceReg);
            return resultReg;
        }

        if (predicateExpr is FunctionCallNode fcPos &&
            string.IsNullOrEmpty(fcPos.Prefix) &&
            fcPos.LocalName == "position" &&
            fcPos.Arguments.Count == 0)
        {
            // position() as a predicate is always true (non-zero position)
            // This should have been optimized away, but handle it anyway
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)sequenceReg);
            return resultReg;
        }

        // General predicate: emit Filter instruction with inline predicate code
        int filterInstrIdx = _instructions.Count;
        Emit(IrOpCode.Filter, (ushort)resultReg, (ushort)sequenceReg, 0, 0); // placeholder

        // Jump over predicate code (so it doesn't execute during fall-through)
        int jumpInstrIdx = _instructions.Count;
        Emit(IrOpCode.Jump, 0, 0, 0, 0); // placeholder

        // Predicate entry point
        int predicateEntry = _instructions.Count;

        // The Filter instruction sets the context item before jumping here.
        // The predicate expression is evaluated with that context.
        int predicateReg = LowerNode(predicateExpr);
        Emit(IrOpCode.Return, (ushort)predicateReg);
        FreeRegister(predicateReg);

        // Patch the Filter instruction to point to predicate entry
        int afterPredicate = _instructions.Count;
        PatchInstruction(filterInstrIdx, IrOpCode.Filter, (ushort)resultReg, (ushort)sequenceReg, 0, predicateEntry);
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
            Emit(IrOpCode.LoadEmptySequence, (ushort)reg);
            return reg;
        }

        if (node.Expressions.Count == 1)
        {
            return LowerNode(node.Expressions[0], targetReg);
        }

        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.SequenceStart, (ushort)resultReg);

        foreach (var expr in node.Expressions)
        {
            int itemReg = LowerNode(expr);
            Emit(IrOpCode.SequenceAdd, (ushort)resultReg, (ushort)itemReg);
        }

        Emit(IrOpCode.SequenceEnd, (ushort)resultReg);
        return resultReg;
    }

    private int LowerRange(RangeExpressionNode node, int? targetReg)
    {
        int fromReg = LowerNode(node.From);
        int toReg = LowerNode(node.To);
        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.Range, (ushort)resultReg, (ushort)fromReg, (ushort)toReg);
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
        ValidateCastTarget(typeName);
        int poolIdx = AddToLiteralPool(typeName);
        Emit(IrOpCode.Cast, (ushort)resultReg, (ushort)exprReg, (ushort)node.Occurrence, poolIdx);
        return resultReg;
    }

    private int LowerCastable(CastableNode node, int? targetReg)
    {
        int exprReg = LowerNode(node.Expression);
        int resultReg = targetReg ?? AllocRegister();
        string typeName = string.IsNullOrEmpty(node.Prefix) ? node.TypeName : $"{node.Prefix}:{node.TypeName}";
        ValidateCastTarget(typeName);
        int poolIdx = AddToLiteralPool(typeName);
        Emit(IrOpCode.Castable, (ushort)resultReg, (ushort)exprReg, (ushort)node.Occurrence, poolIdx);
        return resultReg;
    }

    private static void ValidateCastTarget(string typeName)
    {
        string normalized = typeName.ToLowerInvariant().Replace("xs:", "");
        if (normalized is "anyatomictype" or "notation")
            throw new InvalidOperationException($"XPST0080: '{typeName}' is an abstract type and cannot be used in 'cast' or 'castable as' expressions.");
    }

    private int LowerInstanceOf(InstanceOfNode node, int? targetReg)
    {
        int exprReg = LowerNode(node.Expression);
        int resultReg = targetReg ?? AllocRegister();
        string typeName = string.IsNullOrEmpty(node.Prefix) ? node.TypeName : $"{node.Prefix}:{node.TypeName}";
        int poolIdx = AddToLiteralPool(typeName);
        Emit(IrOpCode.InstanceOf, (ushort)resultReg, (ushort)exprReg, (ushort)node.Occurrence, poolIdx);
        return resultReg;
    }

    private int LowerTreat(TreatNode node, int? targetReg)
    {
        int exprReg = LowerNode(node.Expression);
        int resultReg = targetReg ?? AllocRegister();
        string typeName = string.IsNullOrEmpty(node.Prefix) ? node.TypeName : $"{node.Prefix}:{node.TypeName}";
        int poolIdx = AddToLiteralPool(typeName);
        Emit(IrOpCode.TreatAs, (ushort)resultReg, (ushort)exprReg, (ushort)node.Occurrence, poolIdx);
        return resultReg;
    }

    // ------------------------------------------------------------------
    // XPath 3.1 additions
    // ------------------------------------------------------------------

    private int LowerArrow(ArrowExprNode node, int? targetReg)
    {
        // $x => fn()  desugars to  fn($x)
        int sourceReg = LowerNode(node.Source);
        int resultReg = targetReg ?? AllocRegister();

        switch (node.Target)
        {
            case FunctionCallNode staticCall:
                {
                    int funcPoolIdx;
                    if (!string.IsNullOrEmpty(staticCall.NamespaceUri))
                    {
                        funcPoolIdx = AddToLiteralPool((staticCall.LocalName, staticCall.NamespaceUri));
                    }
                    else
                    {
                        string qname = string.IsNullOrEmpty(staticCall.Prefix)
                            ? staticCall.LocalName
                            : $"{staticCall.Prefix}:{staticCall.LocalName}";
                        funcPoolIdx = AddToLiteralPool(qname);
                    }

                    int argCount = staticCall.Arguments.Count + 1;
                    var argRegs = new int[argCount];
                    argRegs[0] = sourceReg;
                    for (int i = 0; i < staticCall.Arguments.Count; i++)
                        argRegs[i + 1] = LowerNode(staticCall.Arguments[i]);

                    int firstArgReg = PackArgumentsConsecutive(argRegs);
                    Emit(IrOpCode.Call, (ushort)resultReg, (ushort)firstArgReg, (ushort)argCount, funcPoolIdx);
                    return resultReg;
                }

            case DynamicFunctionCallNode dynamicCall:
                {
                    int funcReg = LowerNode(dynamicCall.Function);
                    int argCount = dynamicCall.Arguments.Count + 1;
                    var argRegs = new int[argCount];
                    argRegs[0] = sourceReg;
                    for (int i = 0; i < dynamicCall.Arguments.Count; i++)
                        argRegs[i + 1] = LowerNode(dynamicCall.Arguments[i]);

                    int firstArgReg = PackArgumentsConsecutive(argRegs);
                    Emit(IrOpCode.Apply, (ushort)resultReg, (ushort)funcReg, (ushort)argCount, firstArgReg);
                    return resultReg;
                }

            default:
                throw new NotSupportedException($"Arrow target type {node.Target.GetType().Name} is not supported.");
        }
    }

    private int PackArgumentsConsecutive(int[] argRegs)
    {
        bool consecutive = true;
        for (int i = 1; i < argRegs.Length; i++)
        {
            if (argRegs[i] != argRegs[0] + i)
            {
                consecutive = false;
                break;
            }
        }

        if (consecutive)
            return argRegs[0];

        int firstArgReg = _nextRegister;
        for (int i = 0; i < argRegs.Length; i++)
        {
            Debug.Assert(_nextRegister <= 65535, "Register overflow during argument repacking.");
            int packedReg = _nextRegister++;
            Emit(IrOpCode.Move, (ushort)packedReg, (ushort)argRegs[i]);
        }
        return firstArgReg;
    }

    private int LowerLookup(LookupNode node, int? targetReg)
    {
        int exprReg = LowerNode(node.Expression);
        int keyReg = LowerNode(node.Key);
        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.Lookup, (ushort)resultReg, (ushort)exprReg, (ushort)keyReg);
        return resultReg;
    }

    private int LowerLookupWildcard(LookupWildcardNode node, int? targetReg)
    {
        int exprReg = LowerNode(node.Expression);
        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.LookupWildcard, (ushort)resultReg, (ushort)exprReg);
        return resultReg;
    }

    private int LowerMapConstructor(MapConstructorNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.Map, (ushort)resultReg);

        foreach (var entry in node.Entries)
        {
            int keyReg = LowerNode(entry.Key);
            int valueReg = LowerNode(entry.Value);
            Emit(IrOpCode.MapAdd, (ushort)resultReg, (ushort)keyReg, (ushort)valueReg);
        }

        return resultReg;
    }

    private int LowerArrayConstructor(ArrayConstructorNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        Emit(IrOpCode.Array, (ushort)resultReg);

        if (node.IsSquare)
        {
            foreach (var item in node.Items)
            {
                int itemReg = LowerNode(item);
                Emit(IrOpCode.ArrayAdd, (ushort)resultReg, (ushort)itemReg);
            }
        }
        else
        {
            // Curly array constructor: array { expr } — each item in the
            // sequence becomes a separate array member.
            foreach (var item in node.Items)
            {
                int itemReg = LowerNode(item);
                Emit(IrOpCode.ArrayAddAll, (ushort)resultReg, (ushort)itemReg);
            }
        }

        return resultReg;
    }

    // ------------------------------------------------------------------
    // FLWOR
    // ------------------------------------------------------------------

    private int LowerForExpression(ForExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        LowerForBindings(node.Bindings, 0, node.ReturnExpression, resultReg);
        return resultReg;
    }

    private void LowerForBindings(IReadOnlyList<QuantifiedBinding> bindings, int index, XPathAstNode returnExpr, int resultReg)
    {
        var binding = bindings[index];
        int seqReg = LowerNode(binding.Expression);

        int forIdx = _instructions.Count;
        Emit(IrOpCode.For, (ushort)resultReg, (ushort)seqReg, 0, 0);
        FreeRegister(seqReg);

        int jumpIdx = _instructions.Count;
        Emit(IrOpCode.Jump, 0, 0, 0, 0);

        int rhsEntry = _instructions.Count;
        if (index == bindings.Count - 1)
        {
            int rhsReg = LowerNode(returnExpr);
            Emit(IrOpCode.Return, (ushort)rhsReg);
            FreeRegister(rhsReg);
        }
        else
        {
            LowerForBindings(bindings, index + 1, returnExpr, resultReg);
            Emit(IrOpCode.Return, (ushort)resultReg);
        }

        int afterRhs = _instructions.Count;
        var info = new QuantifiedLoopInfo(binding.VariableName, rhsEntry, binding.PositionalVariableName);
        int poolIdx = AddToLiteralPool(info);
        PatchInstruction(forIdx, IrOpCode.For, (ushort)resultReg, (ushort)seqReg, 0, poolIdx);
        PatchInstruction(jumpIdx, IrOpCode.Jump, 0, 0, 0, afterRhs);
    }

    private int LowerTryCatch(TryCatchNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        int tryCatchIdx = _instructions.Count;
        Emit(IrOpCode.TryCatch, (ushort)resultReg, 0, 0, 0);

        int jumpIdx = _instructions.Count;
        Emit(IrOpCode.Jump, 0, 0, 0, 0);

        int tryEntry = _instructions.Count;
        int tryReg = LowerNode(node.TryExpression);
        Emit(IrOpCode.Return, (ushort)tryReg);

        int catchEntry = _instructions.Count;
        int catchReg = LowerNode(node.CatchExpression);
        Emit(IrOpCode.Return, (ushort)catchReg);

        int afterCatch = _instructions.Count;
        var info = new TryCatchInfo(tryEntry, catchEntry);
        int poolIdx = AddToLiteralPool(info);
        PatchInstruction(tryCatchIdx, IrOpCode.TryCatch, (ushort)resultReg, 0, 0, poolIdx);
        PatchInstruction(jumpIdx, IrOpCode.Jump, 0, 0, 0, afterCatch);

        return resultReg;
    }

    private int LowerQuantifiedExpression(QuantifiedExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        LowerQuantifiedBindings(node.Quantifier, node.Bindings, 0, node.SatisfiesExpression, resultReg);
        return resultReg;
    }

    private void LowerQuantifiedBindings(QuantifierKind quantifier, IReadOnlyList<QuantifiedBinding> bindings, int index, XPathAstNode satisfiesExpr, int resultReg)
    {
        var binding = bindings[index];
        int seqReg = LowerNode(binding.Expression);

        IrOpCode opCode = quantifier == QuantifierKind.Some ? IrOpCode.Some : IrOpCode.Every;

        int quantIdx = _instructions.Count;
        Emit(opCode, (ushort)resultReg, (ushort)seqReg, 0, 0);
        FreeRegister(seqReg);

        int jumpIdx = _instructions.Count;
        Emit(IrOpCode.Jump, 0, 0, 0, 0);

        int rhsEntry = _instructions.Count;
        if (index == bindings.Count - 1)
        {
            int rhsReg = LowerNode(satisfiesExpr);
            Emit(IrOpCode.Return, (ushort)rhsReg);
            FreeRegister(rhsReg);
        }
        else
        {
            LowerQuantifiedBindings(quantifier, bindings, index + 1, satisfiesExpr, resultReg);
            Emit(IrOpCode.Return, (ushort)resultReg);
        }

        int afterRhs = _instructions.Count;
        var info = new QuantifiedLoopInfo(binding.VariableName, rhsEntry);
        int poolIdx = AddToLiteralPool(info);
        PatchInstruction(quantIdx, opCode, (ushort)resultReg, (ushort)seqReg, 0, poolIdx);
        PatchInstruction(jumpIdx, IrOpCode.Jump, 0, 0, 0, afterRhs);
    }

    // ------------------------------------------------------------------
    // Let expressions
    // ------------------------------------------------------------------

    private int LowerLetExpression(LetExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        foreach (var binding in node.Bindings)
        {
            int exprReg = LowerNode(binding.Expression);
            int varPoolIdx = AddToLiteralPool(binding.VariableName);
            Emit(IrOpCode.StoreVariable, 0, (ushort)exprReg, 0, varPoolIdx);
            FreeRegister(exprReg);
        }

        int bodyReg = LowerNode(node.Body, resultReg);
        if (bodyReg != resultReg)
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)bodyReg);

        return resultReg;
    }

    // ------------------------------------------------------------------
    // Inline functions
    // ------------------------------------------------------------------

    private int LowerInlineFunction(InlineFunctionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        var subLowerer = new IrLowerer();
        int bodyReg = subLowerer.LowerNode(node.Body);
        subLowerer.Emit(IrOpCode.Return, (ushort)bodyReg);
        var module = subLowerer.Lower(node.Body);

        var paramNames = node.Parameters.Select(p => p.Name).ToList();
        var paramTypes = node.Parameters.Select(p => p.TypeName).ToList();
        var funcItem = new CompilerInlineFunction(paramNames, module, paramTypes, node.ReturnType);
        int poolIdx = AddToLiteralPool(funcItem);
        Emit(IrOpCode.LoadFunction, (ushort)resultReg, operand: poolIdx);
        return resultReg;
    }

    // ------------------------------------------------------------------
    // Dynamic function calls
    // ------------------------------------------------------------------

    private int LowerDynamicFunctionCall(DynamicFunctionCallNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        int funcReg = LowerNode(node.Function);

        int argCount = node.Arguments.Count;

        // Partial application: f(?, arg) produces a curried function item instead of
        // invoking the function.
        bool hasPlaceholders = node.Arguments.Any(a => a is ArgumentPlaceholderNode);
        if (hasPlaceholders)
        {
            var descriptor = new int[argCount];
            var argRegs = new List<int>();
            for (int i = 0; i < argCount; i++)
            {
                if (node.Arguments[i] is ArgumentPlaceholderNode)
                {
                    descriptor[i] = -1;
                }
                else
                {
                    int argReg = LowerNode(node.Arguments[i]);
                    descriptor[i] = argReg;
                    argRegs.Add(argReg);
                }
            }

            int descPoolIdx = AddToLiteralPool(descriptor);
            Emit(IrOpCode.Curry, (ushort)resultReg, (ushort)funcReg, operand: descPoolIdx);
            FreeRegister(funcReg);
            foreach (var r in argRegs) FreeRegister(r);
            return resultReg;
        }

        int firstArgReg = 0;

        if (argCount > 0)
        {
            var argRegs = new int[argCount];
            argRegs[0] = LowerNode(node.Arguments[0]);
            for (int i = 1; i < argCount; i++)
            {
                argRegs[i] = LowerNode(node.Arguments[i]);
            }

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
                firstArgReg = _nextRegister;
                for (int i = 0; i < argCount; i++)
                {
                    Debug.Assert(_nextRegister <= 65535, "Register overflow during argument repacking.");
                    int packedReg = _nextRegister++;
                    Emit(IrOpCode.Move, (ushort)packedReg, (ushort)argRegs[i]);
                }
            }
        }

        Emit(IrOpCode.Apply, (ushort)resultReg, (ushort)funcReg, (ushort)argCount, firstArgReg);
        return resultReg;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private int AllocRegister()
    {
        if (_freeRegisters.Count > 0)
            return _freeRegisters.Pop();
        Debug.Assert(_nextRegister <= 65535, "Register overflow: more than 65535 registers allocated.");
        return _nextRegister++;
    }

    private void FreeRegister(int reg)
    {
        if (reg >= 0 && reg < _nextRegister && !_freeRegisters.Contains(reg))
            _freeRegisters.Push(reg);
    }

    private int AddToLiteralPool(object? value)
    {
        int idx = _literalPool.Count;
        _literalPool.Add(value);
        return idx;
    }

    private int CurrentInstructionIndex => _instructions.Count;

    private void Emit(IrOpCode op, ushort regA = 0, ushort regB = 0, ushort regC = 0, int operand = 0)
    {
        _instructions.Add(new IrInstruction(op, regA, regB, regC, operand));
    }

    private int EmitJumpPlaceholder(IrOpCode jumpOp, ushort regA = 0)
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

    private void PatchInstruction(int index, IrOpCode op, ushort regA, ushort regB = 0, ushort regC = 0, int operand = 0)
    {
        _instructions[index] = new IrInstruction(op, regA, regB, regC, operand);
    }
}
