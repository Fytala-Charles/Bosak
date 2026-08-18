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
//                      | Charles Korthout | 1.10  | 15-07-2026     | QuantifiedLoopInfo carries VariablePrefix/VariableNamespaceUri; for/some/every bindings preserve EQName namespaces |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.11  | 19-07-2026     | Fixed LowerAnd/LowerOr to not free the result register when reusing it for operands       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.12  | 19-07-2026     | Emit UnaryPlus opcode instead of Move; runtime validates operand type                    |
//                      | Charles Korthout | 1.13  | 22-07-2026     | Lower FlworExpressionNode with order by (OrderBy/TupleBind opcodes)                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.14  | 23-07-2026     | Lower XQuery FLWOR count clause via tuple-path counters                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.15  | 25-07-2026     | Lower XQuery FLWOR group by clause (GroupBy opcode, post-group order by re-keying)      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.16  | 25-07-2026     | Lower XQuery FLWOR window clause (Window opcode with start/end condition blocks)        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.17  | 25-07-2026     | Nested-rhs Return fix; positional vars in tuples; 'as' type enforcement (EnforceType)   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.18  | 25-07-2026     | Constructor lowering; FLWOR tuple vars scoped to the body For; count counter ref-eq     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.19  | 25-07-2026     | Constructor-local namespace declarations (SaveNamespaces/DeclareNamespace)              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.20  | 25-07-2026     | Lower computed constructors; window/tuple variable bindings keep prefixes and EQName namespaces |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.21  | 25-07-2026     | Lower switch/typeswitch by desugaring to let/if/eq/instance-of chains                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.22  | 26-07-2026     | Simple for-loop lowering seeds QuantifiedLoopInfo.ScopedVariableNames from top-level let names |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.23  | 27-07-2026     | TryCatchInfo carries ordered catch clauses with code patterns |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.24  | 27-07-2026     | String constructors desugar to fn:string-join(fn:data(E) ! fn:string(.)) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.25  | 27-07-2026     | DefaultEmptyOrder property applied to order-by specs without explicit empty order |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.26  | 28-07-2026     | Emit KindTestType and NamespaceTest for prefixed kind-test arguments |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.27  | 29-07-2026     | 'allowing empty' checks the empty binding against the declared type occurrence |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.28  | 29-07-2026     | Switch desugar: error-tolerant case comparisons; operand/case cardinality checks |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.29  | 03-08-2026     | Kind-test args: NamespaceTest for Q{uri}local and for unprefixed element() names      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.30  | 07-08-2026     | Cast targets: xs:anySimpleType XPST0080, xs:untyped/xs:anyType XQST0052; Q{}* sentinel |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.31  | 15-08-2026     | Revert unverified FirstStepRequiresContext helper; keep simple StepNode context check |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.32  | 18-08-2026     | Reject let clauses between group by and order by (unsupported ordering)                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.33  | 18-08-2026     | Arrow partial application: support placeholder arguments in => static calls (ArrowPostfix-108) |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Diagnostics;
using Bosak.XPath.Core;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser.Ast;

namespace Bosak.XPath.Compiler.Ir;

/// <summary>
/// Loop information stored in the literal pool for For/Some/Every opcodes.
/// </summary>
public readonly record struct QuantifiedLoopInfo(string VariableName, int RhsEntryPoint, string? PositionalVariableName = null, string? VariablePrefix = null, string? VariableNamespaceUri = null, bool AllowingEmpty = false, IReadOnlyList<string>? ScopedVariableNames = null);

/// <summary>
/// Try/catch information stored in the literal pool for the TryCatch opcode: the try block
/// entry point and the catch clauses in declaration order (first matching clause wins).
/// </summary>
public sealed record TryCatchInfo(int TryEntryPoint, IReadOnlyList<CatchClauseInfo> Clauses);

/// <summary>
/// One catch clause of a <see cref="TryCatchInfo"/>: the error-code patterns that select it
/// and its body entry point.
/// </summary>
public sealed record CatchClauseInfo(IReadOnlyList<CatchCodePattern> Patterns, int EntryPoint);

/// <summary>
/// Ordering information stored in the literal pool for the OrderBy opcode.
/// Tuple format: [valueCount variable items, then keyCount key items].
/// </summary>
public readonly record struct OrderByInfo(
    int ValueCount,
    int KeyCount,
    bool[] Descending,
    EmptyOrder[] EmptyOrder,
    string?[] CollationUri);

/// <summary>
/// Variable-binding information stored in the literal pool for the TupleBind opcode.
/// </summary>
public readonly record struct TupleBindInfo(IReadOnlyList<(string LocalName, string? Prefix, string? NamespaceUri)> Variables);

/// <summary>
/// Grouping information stored in the literal pool for the GroupBy opcode.
/// Tuple format: [variable items]; the key indices identify the grouping variables.
/// Per-spec optional declared types are enforced on each pre-grouping key value.
/// </summary>
public readonly record struct GroupByInfo(
    IReadOnlyList<int> KeyIndices,
    IReadOnlyList<string?> CollationUri,
    IReadOnlyList<string?> DeclaredTypeNames,
    IReadOnlyList<OccurrenceIndicator> DeclaredTypeOccurrences);

/// <summary>
/// Type-enforcement information stored in the literal pool for the EnforceType opcode.
/// Raises the given error code when the value is not an instance of the declared type.
/// </summary>
public readonly record struct EnforceTypeInfo(string TypeName, OccurrenceIndicator Occurrence, string ErrorCode);

/// <summary>
/// Attribute metadata for the ConstructElement opcode; the attribute's value parts are a
/// slice of the shared <see cref="ConstructElementInfo.Parts"/> list.
/// </summary>
public readonly record struct ConstructAttributeInfo(string LocalName, string? Prefix, int FirstPart, int PartCount);

/// <summary>
/// Element construction information stored in the literal pool for the ConstructElement opcode.
/// Expression parts reference registers relative to the instruction's RegisterB base
/// (packed consecutive by the lowerer).
/// </summary>
public readonly record struct ConstructElementInfo(
    string LocalName,
    string? Prefix,
    ConstructAttributeInfo[] Attributes,
    int FirstContentPart,
    int ContentPartCount,
    ConstructPartInfo[] Parts);

/// <summary>
/// One element-construction part: a literal text string (literal-pool index), an evaluated
/// value (register offset from the instruction's RegisterB base), a comment (literal-pool
/// index of its value), or a processing instruction (literal-pool indices of data and target).
/// </summary>
public readonly record struct ConstructPartInfo(ConstructPartKind Kind, int Index, int Index2 = -1);

/// <summary>The kind of one element-construction part.</summary>
public enum ConstructPartKind : byte
{
    /// <summary>Literal text; Index is the literal-pool index of the string.</summary>
    Literal,
    /// <summary>An evaluated expression value; Index is the register offset from RegisterB.</summary>
    Value,
    /// <summary>A comment node; Index is the literal-pool index of the comment value.</summary>
    Comment,
    /// <summary>A processing instruction; Index is the data pool index, Index2 the target pool index.</summary>
    ProcessingInstruction
}

/// <summary>The kind of a computed constructor (element/attribute/document/text/comment/PI/namespace).</summary>
public enum ComputedConstructorKind : byte
{
    /// <summary><c>element name { content }</c></summary>
    Element,
    /// <summary><c>attribute name { value }</c></summary>
    Attribute,
    /// <summary><c>document { content }</c></summary>
    Document,
    /// <summary><c>text { value }</c></summary>
    Text,
    /// <summary><c>comment { value }</c></summary>
    Comment,
    /// <summary><c>processing-instruction target { value }</c></summary>
    ProcessingInstruction,
    /// <summary><c>namespace prefix { uri }</c></summary>
    Namespace
}

/// <summary>
/// Construction information stored in the literal pool for the ConstructComputed opcode.
/// Static name parts when present; when HasNameExpression is true the name/target/prefix is
/// evaluated from the register in RegisterB instead.
/// </summary>
public readonly record struct ComputedConstructorInfo(
    ComputedConstructorKind Kind,
    string? LocalName,
    string? Prefix,
    string? NamespaceUri,
    bool HasNameExpression);

/// <summary>
/// Windowing information stored in the literal pool for the Window opcode.
/// Carries the window variable, the tumbling/sliding flag, the entry points of the
/// start condition, end condition, and window body blocks, and the optional
/// current/positional/previous/next variable names of both conditions.
/// </summary>
public readonly record struct WindowInfo(
    string VariableName,
    string? VariableNamespaceUri,
    bool Sliding,
    bool OnlyEnd,
    int StartEntryPoint,
    int EndEntryPoint,
    int RhsEntryPoint,
    string? StartCurrent,
    string? StartPos,
    string? StartPrev,
    string? StartNext,
    string? EndCurrent,
    string? EndPos,
    string? EndPrev,
    string? EndNext,
    string? DeclaredTypeName = null,
    OccurrenceIndicator DeclaredTypeOccurrence = OccurrenceIndicator.One);

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
    private int _nextCountCounter;
    private readonly Stack<int> _freeRegisters = new();

    /// <summary>
    /// The static context's default order for empty sequences in order-by clauses
    /// (from the prolog's <c>declare default order empty least|greatest</c>);
    /// null means <see cref="EmptyOrder.Least"/>.
    /// </summary>
    public EmptyOrder? DefaultEmptyOrder { get; set; }

    private EmptyOrder ResolveEmptyOrder(EmptyOrder? specEmptyOrder)
        => specEmptyOrder ?? DefaultEmptyOrder ?? EmptyOrder.Least;

    public IrModule Lower(XPathAstNode node)
    {
        _instructions.Clear();
        _literalPool.Clear();
        _freeRegisters.Clear();
        _nextRegister = 0;
        _nextCountCounter = 0;

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
            SwitchExpressionNode n => LowerSwitch(n, targetReg),
            TypeswitchExpressionNode n => LowerTypeswitch(n, targetReg),
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
            DirectElementConstructorNode n => LowerDirectElementConstructor(n, targetReg),
            DirectCommentNode n => LowerDirectComment(n, targetReg),
            DirectProcessingInstructionNode n => LowerDirectProcessingInstruction(n, targetReg),
            ComputedElementConstructorNode n => LowerComputedConstructor(ComputedConstructorKind.Element, n.NameExpression, n.TagName, n.TagPrefix, n.TagNamespaceUri, n.ContentExpression, targetReg),
            ComputedAttributeConstructorNode n => LowerComputedConstructor(ComputedConstructorKind.Attribute, n.NameExpression, n.Name, n.Prefix, n.NamespaceUri, n.ValueExpression, targetReg),
            ComputedDocumentConstructorNode n => LowerComputedConstructor(ComputedConstructorKind.Document, null, null, null, null, n.ContentExpression, targetReg),
            ComputedTextConstructorNode n => LowerComputedConstructor(ComputedConstructorKind.Text, null, null, null, null, n.ValueExpression, targetReg),
            ComputedCommentConstructorNode n => LowerComputedConstructor(ComputedConstructorKind.Comment, null, null, null, null, n.ValueExpression, targetReg),
            ComputedPIConstructorNode n => LowerComputedConstructor(ComputedConstructorKind.ProcessingInstruction, n.TargetExpression, n.Target, null, null, n.ValueExpression, targetReg),
            ComputedNamespaceConstructorNode n => LowerComputedConstructor(ComputedConstructorKind.Namespace, n.PrefixExpression, n.Prefix, null, null, n.UriExpression, targetReg),
            MapConstructorNode n => LowerMapConstructor(n, targetReg),
            ArrayConstructorNode n => LowerArrayConstructor(n, targetReg),
            LookupNode n => LowerLookup(n, targetReg),
            LookupWildcardNode n => LowerLookupWildcard(n, targetReg),
            ForExpressionNode n => LowerForExpression(n, targetReg),
            QuantifiedExpressionNode n => LowerQuantifiedExpression(n, targetReg),
            TryCatchNode n => LowerTryCatch(n, targetReg),
            StringConstructorNode n => LowerStringConstructor(n, targetReg),
            LetExpressionNode n => LowerLetExpression(n, targetReg),
            FlworExpressionNode n => LowerFlworExpression(n, targetReg),
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
        if (leftReg != resultReg)
            FreeRegister(leftReg);

        // If left is false, result is false
        int jumpToFalse = EmitJumpPlaceholder(IrOpCode.JumpIfFalse, (ushort)resultReg);

        // Evaluate right
        int rightReg = LowerNode(node.Right, resultReg);
        if (rightReg != resultReg)
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)rightReg);
        if (rightReg != resultReg)
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
        if (leftReg != resultReg)
            FreeRegister(leftReg);

        // If left is true, result is true
        int jumpToTrue = EmitJumpPlaceholder(IrOpCode.JumpIfTrue, (ushort)resultReg);

        // Evaluate right
        int rightReg = LowerNode(node.Right, resultReg);
        if (rightReg != resultReg)
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)rightReg);
        if (rightReg != resultReg)
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
                Emit(IrOpCode.UnaryPlus, (ushort)resultReg, (ushort)operandReg);
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

    private int _nextSyntheticVar;

    // switch (E) case V1 case V2 return R1 ... default return RD
    // Desugars to: let $__switch_N := E return
    //   if ($__switch_N eq V1 or $__switch_N eq V2) then R1 else (... else RD)
    // Case operands compare with value-comparison (eq) semantics and evaluate lazily
    // in order, so errors in later cases do not surface after a match.
    private int LowerSwitch(SwitchExpressionNode node, int? targetReg)
    {
        var tmp = $"__switch_{_nextSyntheticVar++}";
        XPathAstNode body = node.Default;
        for (int i = node.Cases.Count - 1; i >= 0; i--)
        {
            var clause = node.Cases[i];
            XPathAstNode? condition = null;
            foreach (var value in clause.Values)
            {
                var eq = new BinaryExpressionNode(new VariableReferenceNode(tmp), BinaryOperator.Eq, value);
                // XQuery 3.0 §3.12.2: for the purposes of switch comparison, NaN is
                // equal to NaN (switch-011) — detected via the self-inequality idiom.
                XPathAstNode comparison = new BinaryExpressionNode(eq, BinaryOperator.Or,
                    new BinaryExpressionNode(
                        new BinaryExpressionNode(new VariableReferenceNode(tmp), BinaryOperator.Ne, new VariableReferenceNode(tmp)),
                        BinaryOperator.And,
                        new BinaryExpressionNode(value, BinaryOperator.Ne, value)));
                // XQuery 3.0 §3.12.2: a case whose comparison raises an error does not
                // match (switch-006/007: decimal operand vs a current-time() case); but a
                // multi-item case operand is XPTY0004 before that rule applies (switch-902).
                // An empty switch operand matches an empty case operand (switch-009).
                XPathAstNode guarded = new IfExpressionNode(
                    new BinaryExpressionNode(
                        new FunctionCallNode("count", new XPathAstNode[] { value }),
                        BinaryOperator.Gt,
                        new IntegerLiteralNode(1)),
                    new FunctionCallNode("error",
                        new XPathAstNode[]
                        {
                            new FunctionCallNode("QName", new XPathAstNode[]
                            {
                                new StringLiteralNode("http://www.w3.org/2005/xqt-errors"),
                                new StringLiteralNode("err:XPTY0004")
                            })
                        }),
                    new BinaryExpressionNode(
                        new BinaryExpressionNode(
                            new FunctionCallNode("empty", new XPathAstNode[] { new VariableReferenceNode(tmp) }),
                            BinaryOperator.And,
                            new FunctionCallNode("empty", new XPathAstNode[] { value })),
                        BinaryOperator.Or,
                        new TryCatchNode(comparison,
                            new[] { new TryCatchClause(new[] { new CatchCodePattern(null, null, null) }, new BooleanLiteralNode(false)) })));
                condition = condition is null ? guarded : new BinaryExpressionNode(condition, BinaryOperator.Or, guarded);
            }
            body = new IfExpressionNode(condition!, clause.Return, body);
        }
        // The switch operand must be a single atomic value (or empty): a multi-item
        // operand is XPTY0004, raised before — and independently of — the guarded case
        // comparisons (switch-901/902).
        XPathAstNode cardinalityGuard = new IfExpressionNode(
            new BinaryExpressionNode(
                new FunctionCallNode("count", new XPathAstNode[] { new VariableReferenceNode(tmp) }),
                BinaryOperator.Gt,
                new IntegerLiteralNode(1)),
            new FunctionCallNode("error",
                new XPathAstNode[]
                {
                    new FunctionCallNode("QName", new XPathAstNode[]
                    {
                        new StringLiteralNode("http://www.w3.org/2005/xqt-errors"),
                        new StringLiteralNode("err:XPTY0004")
                    })
                }),
            body);
        return LowerNode(new LetExpressionNode(
            new[] { new QuantifiedBinding(tmp, node.Operand) }, cardinalityGuard), targetReg);
    }

    // typeswitch (E) case $v as T return R ... default ($d)? return RD
    // Desugars to: let $__typeswitch_N := E return
    //   if ($__typeswitch_N instance of T) then (let $v := $__typeswitch_N return R) else (...)
    private int LowerTypeswitch(TypeswitchExpressionNode node, int? targetReg)
    {
        var tmp = $"__typeswitch_{_nextSyntheticVar++}";

        XPathAstNode Bind(string? local, string? prefix, string? ns, XPathAstNode expr) =>
            local is null
                ? expr
                : new LetExpressionNode(
                    new[] { new QuantifiedBinding(local, new VariableReferenceNode(tmp), VariablePrefix: prefix, VariableNamespaceUri: ns) },
                    expr);

        XPathAstNode body = Bind(node.DefaultVariableName, node.DefaultVariablePrefix, node.DefaultVariableNamespaceUri, node.Default);
        for (int i = node.Cases.Count - 1; i >= 0; i--)
        {
            var clause = node.Cases[i];
            // A case matches when the operand is an instance of ANY member of the type union.
            XPathAstNode? condition = null;
            foreach (var type in clause.Types)
            {
                var instanceOf = new InstanceOfNode(new VariableReferenceNode(tmp), type.Local, type.Prefix, type.Occurrence);
                condition = condition is null ? instanceOf : new BinaryExpressionNode(condition, BinaryOperator.Or, instanceOf);
            }
            body = new IfExpressionNode(
                condition!,
                Bind(clause.VariableName, clause.VariablePrefix, clause.VariableNamespaceUri, clause.Return),
                body);
        }
        return LowerNode(new LetExpressionNode(
            new[] { new QuantifiedBinding(tmp, node.Operand) }, body), targetReg);
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

    private int LowerDirectElementConstructor(DirectElementConstructorNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        var parts = new List<ConstructPartInfo>();
        var attrs = new List<ConstructAttributeInfo>();
        var exprRegs = new List<int>();

        // Constructor-local namespace declarations are applied before any content is
        // evaluated (nested constructors see them) and restored afterwards.
        var nsDecls = node.Attributes
            .Where(a => a.Prefix == "xmlns" || (a.Name == "xmlns" && a.Prefix is null))
            .ToList();
        int nsSaveReg = AllocRegister();
        if (nsDecls.Count > 0)
        {
            Emit(IrOpCode.SaveNamespaces, (ushort)nsSaveReg);
            foreach (var decl in nsDecls)
            {
                // The parser enforces literal-only namespace URIs (XQST0022).
                var uri = NormalizeNamespaceDeclValue(string.Concat(
                    decl.ValueParts.OfType<StringLiteralNode>().Select(p => p.Value)));
                int valReg = AllocRegister();
                int valPoolIdx = AddToLiteralPool(uri);
                Emit(IrOpCode.LoadString, (ushort)valReg, operand: valPoolIdx);
                int prefixPoolIdx = AddToLiteralPool(decl.Prefix == "xmlns" ? decl.Name : "");
                Emit(IrOpCode.DeclareNamespace, 0, (ushort)valReg, 0, prefixPoolIdx);
                FreeRegister(valReg);
            }
        }

        void AddPart(XPathAstNode part)
        {
            if (part is StringLiteralNode literal)
            {
                parts.Add(new ConstructPartInfo(ConstructPartKind.Literal, AddToLiteralPool(literal.Value)));
            }
            else if (part is SignificantTextNode significant)
            {
                parts.Add(new ConstructPartInfo(ConstructPartKind.Literal, AddToLiteralPool(significant.Value)));
            }
            else if (part is DirectCommentNode comment)
            {
                parts.Add(new ConstructPartInfo(ConstructPartKind.Comment, AddToLiteralPool(comment.Value)));
            }
            else if (part is DirectProcessingInstructionNode pi)
            {
                parts.Add(new ConstructPartInfo(ConstructPartKind.ProcessingInstruction, AddToLiteralPool(pi.Value), AddToLiteralPool(pi.Target)));
            }
            else
            {
                int reg = LowerNode(part);
                exprRegs.Add(reg);
                parts.Add(new ConstructPartInfo(ConstructPartKind.Value, exprRegs.Count - 1));
            }
        }

        foreach (var attr in node.Attributes)
        {
            int firstPart = parts.Count;
            foreach (var p in attr.ValueParts)
                AddPart(p);
            attrs.Add(new ConstructAttributeInfo(attr.Name, attr.Prefix, firstPart, attr.ValueParts.Count));
        }

        int firstContentPart = parts.Count;
        foreach (var p in node.Content)
            AddPart(p);

        int baseReg = exprRegs.Count > 0 ? PackArgumentsConsecutive(exprRegs.ToArray()) : 0;
        var info = new ConstructElementInfo(node.TagName, node.Prefix, attrs.ToArray(), firstContentPart, node.Content.Count, parts.ToArray());
        int poolIdx = AddToLiteralPool(info);
        Emit(IrOpCode.ConstructElement, (ushort)resultReg, (ushort)baseReg, 0, poolIdx);

        if (nsDecls.Count > 0)
        {
            Emit(IrOpCode.RestoreNamespaces, 0, (ushort)nsSaveReg);
        }
        return resultReg;
    }

    private static string NormalizeNamespaceDeclValue(string value)
    {
        var parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }

    private int LowerComputedConstructor(
        ComputedConstructorKind kind,
        XPathAstNode? nameExpression,
        string? localName,
        string? prefix,
        string? namespaceUri,
        XPathAstNode contentExpression,
        int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        int nameReg = nameExpression is not null ? LowerNode(nameExpression) : -1;
        int contentReg = LowerNode(contentExpression);
        var info = new ComputedConstructorInfo(kind, localName, prefix, namespaceUri, nameReg >= 0);
        int poolIdx = AddToLiteralPool(info);
        Emit(IrOpCode.ConstructComputed, (ushort)resultReg, (ushort)(nameReg >= 0 ? nameReg : 0), (ushort)contentReg, poolIdx);
        return resultReg;
    }

    private int LowerDirectComment(DirectCommentNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        int poolIdx = AddToLiteralPool(new XdmContentItem(XdmContentKind.Comment, node.Value));
        Emit(IrOpCode.ConstructContentNode, (ushort)resultReg, 0, 0, poolIdx);
        return resultReg;
    }

    private int LowerDirectProcessingInstruction(DirectProcessingInstructionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();
        int poolIdx = AddToLiteralPool(new XdmContentItem(XdmContentKind.ProcessingInstruction, node.Value, null, node.Target));
        Emit(IrOpCode.ConstructContentNode, (ushort)resultReg, 0, 0, poolIdx);
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

                // Q{}* (explicitly no namespace) must not resolve to the default element
                // namespace at runtime — emit the sentinel the VM matches against the
                // empty namespace URI unconditionally (eqname-013).
                string nsOperand = node.NodeTest.NamespaceUri is { Length: 0 }
                    ? "Q{}"
                    : node.NodeTest.NamespaceUri ?? node.NodeTest.Name ?? ""; // URI or prefix
                int nsPoolIdx = AddToLiteralPool(nsOperand);
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
                    var kindArg = node.NodeTest.KindTestArgument;
                    if (kindArg.StartsWith("Q{", StringComparison.Ordinal))
                    {
                        // Q{uri}local argument: check the literal namespace URI first,
                        // then the local name via the NameTest below.
                        var closeBrace = kindArg.IndexOf('}');
                        if (closeBrace > 2)
                        {
                            int qNsPoolIdx = AddToLiteralPool(kindArg[2..closeBrace]);
                            int qNsTestReg = AllocRegister();
                            Emit(IrOpCode.NamespaceTest, (ushort)qNsTestReg, (ushort)axisReg, operand: qNsPoolIdx);
                            FreeRegister(axisReg);
                            axisReg = qNsTestReg;
                            kindArg = "*:" + kindArg[(closeBrace + 1)..];
                        }
                    }
                    else
                    {
                        // A prefixed name-test argument (prefix:local or prefix:*) also gets its
                        // namespace checked (raising XPST0081 for an unbound prefix, K2-NameTest-66/72).
                        int argColon = kindArg.IndexOf(':');
                        if (argColon > 0)
                        {
                            int argNsPoolIdx = AddToLiteralPool(kindArg[..argColon]);
                            int nsTestReg = AllocRegister();
                            Emit(IrOpCode.NamespaceTest, (ushort)nsTestReg, (ushort)axisReg, operand: argNsPoolIdx);
                            FreeRegister(axisReg);
                            axisReg = nsTestReg;
                        }
                        else if (kindArg != "*" && node.NodeTest.Name == "element")
                        {
                            // An unprefixed element() argument uses the default element
                            // namespace (empty prefix), like unprefixed path name tests
                            // (json-to-xml-escape-001: element(string) must not match a
                            // namespaced element).
                            int argNsPoolIdx = AddToLiteralPool("");
                            int nsTestReg = AllocRegister();
                            Emit(IrOpCode.NamespaceTest, (ushort)nsTestReg, (ushort)axisReg, operand: argNsPoolIdx);
                            FreeRegister(axisReg);
                            axisReg = nsTestReg;
                        }
                    }
                    int argPoolIdx = AddToLiteralPool(kindArg);
                    afterTestReg = AllocRegister();
                    Emit(IrOpCode.NameTest, (ushort)afterTestReg, (ushort)axisReg, operand: argPoolIdx);
                    FreeRegister(axisReg);
                    axisReg = afterTestReg;
                }

                // If the kind test carries a schema type name (element(foo, xs:integer)),
                // emit a KindTestType filter (validates the type name and checks type
                // compatibility: unknown type names raise XPST0008 at evaluation time).
                if (!string.IsNullOrEmpty(node.NodeTest.KindTestTypeName))
                {
                    int typePoolIdx = AddToLiteralPool(node.NodeTest.KindTestTypeName);
                    afterTestReg = AllocRegister();
                    Emit(IrOpCode.KindTestType, (ushort)afterTestReg, (ushort)axisReg, operand: typePoolIdx);
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
        // (only when the literal fits the IR int operand — larger values take the
        // general predicate path, where the positional comparison is done in double
        // without truncation; filter-limits-003: 'a'[4294967297]).
        if (predicateExpr is IntegerLiteralNode subscript && subscript.Value is >= int.MinValue and <= int.MaxValue)
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
        // XPST0080: the abstract types are not valid cast/castable targets
        // (K-SeqExprCastable-5a: xs:anySimpleType).
        if (normalized is "anyatomictype" or "anysimpletype" or "notation")
            throw new InvalidOperationException($"XPST0080: '{typeName}' is an abstract type and cannot be used in 'cast' or 'castable as' expressions.");
        // XQST0052: the target must be an atomic type; xs:untyped and xs:anyType are
        // not atomic (K-SeqExprCastable-6a: xs:untyped).
        if (normalized is "untyped" or "anytype")
            throw new InvalidOperationException($"XQST0052: '{typeName}' is not an atomic type and cannot be used in 'cast' or 'castable as' expressions.");
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
                    bool hasPlaceholders = staticCall.Arguments.Any(a => a is ArgumentPlaceholderNode);
                    if (hasPlaceholders)
                    {
                        // Partial application with the arrow source as the first argument:
                        // $x => concat(?) produces a curried function item with one placeholder.
                        var descriptor = new int[argCount];
                        descriptor[0] = sourceReg;
                        var argRegs = new List<int>();
                        for (int i = 0; i < staticCall.Arguments.Count; i++)
                        {
                            if (staticCall.Arguments[i] is ArgumentPlaceholderNode)
                            {
                                descriptor[i + 1] = -1;
                            }
                            else
                            {
                                int argReg = LowerNode(staticCall.Arguments[i]);
                                descriptor[i + 1] = argReg;
                                argRegs.Add(argReg);
                            }
                        }

                        int funcReg = AllocRegister();
                        int funcRefPoolIdx;
                        if (!string.IsNullOrEmpty(staticCall.NamespaceUri))
                        {
                            funcRefPoolIdx = AddToLiteralPool(new NamedFunctionItem(staticCall.NamespaceUri, staticCall.LocalName, argCount));
                        }
                        else
                        {
                            string refQName = string.IsNullOrEmpty(staticCall.Prefix)
                                ? staticCall.LocalName
                                : $"{staticCall.Prefix}:{staticCall.LocalName}";
                            funcRefPoolIdx = AddToLiteralPool((refQName, argCount));
                        }
                        Emit(IrOpCode.LoadFunction, (ushort)funcReg, operand: funcRefPoolIdx);
                        int descPoolIdx = AddToLiteralPool(descriptor);
                        Emit(IrOpCode.Curry, (ushort)resultReg, (ushort)funcReg, operand: descPoolIdx);
                        FreeRegister(funcReg);
                        foreach (var r in argRegs) FreeRegister(r);
                        return resultReg;
                    }

                    var argRegs2 = new int[argCount];
                    argRegs2[0] = sourceReg;
                    for (int i = 0; i < staticCall.Arguments.Count; i++)
                        argRegs2[i + 1] = LowerNode(staticCall.Arguments[i]);

                    int firstArgReg = PackArgumentsConsecutive(argRegs2);
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
        if (binding.DeclaredType is not null)
        {
            // XQuery 'as SequenceType': each bound item must be an instance of the type.
            int varReg = LoadVariable(new BoundVariable(binding.VariableName, binding.VariablePrefix, binding.VariableNamespaceUri));
            EmitEnforceTypeIfDeclared(binding, varReg, itemLevel: true);
            FreeRegister(varReg);
        }
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
        var info = new QuantifiedLoopInfo(binding.VariableName, rhsEntry, binding.PositionalVariableName, binding.VariablePrefix, binding.VariableNamespaceUri, binding.AllowingEmpty, CollectTopLevelLetNames(returnExpr));
        int poolIdx = AddToLiteralPool(info);
        PatchInstruction(forIdx, IrOpCode.For, (ushort)resultReg, (ushort)seqReg, 0, poolIdx);
        PatchInstruction(jumpIdx, IrOpCode.Jump, 0, 0, 0, afterRhs);
    }

    // Names of let variables bound by the top-level let chain of a for body's return
    // expression; their bindings are scoped to one iteration and restored after each
    // (function-declaration-005/006: a let inside a for must not accumulate).
    private static string[]? CollectTopLevelLetNames(XPathAstNode node)
    {
        List<string>? names = null;
        while (node is LetExpressionNode letExpr)
        {
            foreach (var binding in letExpr.Bindings)
                (names ??= new List<string>()).Add(binding.VariableName);
            node = letExpr.Body;
        }
        return names?.ToArray();
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

        var clauseInfos = new List<CatchClauseInfo>();
        foreach (var clause in node.Clauses)
        {
            int clauseEntry = _instructions.Count;
            int clauseReg = LowerNode(clause.Expression);
            Emit(IrOpCode.Return, (ushort)clauseReg);
            clauseInfos.Add(new CatchClauseInfo(clause.Patterns, clauseEntry));
        }

        int afterCatch = _instructions.Count;
        var info = new TryCatchInfo(tryEntry, clauseInfos);
        int poolIdx = AddToLiteralPool(info);
        PatchInstruction(tryCatchIdx, IrOpCode.TryCatch, (ushort)resultReg, 0, 0, poolIdx);
        PatchInstruction(jumpIdx, IrOpCode.Jump, 0, 0, 0, afterCatch);

        return resultReg;
    }

    // ``[ literal `{expr}` ... ]`` desugars to
    //   fn:string-join((literal, fn:string-join(fn:data(expr) ! fn:string(.), " "), ...), "")
    // Per XQuery 3.1 §3.11.2: an interpolation's value is atomized (fn:data — maps raise
    // FOTY0013), each atomic value is cast to xs:string and joined with a single space,
    // and the parts concatenate without a separator.
    private int LowerStringConstructor(StringConstructorNode node, int? targetReg)
    {
        XPathAstNode JoinPart(XPathAstNode part) =>
            part is StringLiteralNode
                ? part
                : new FunctionCallNode("string-join",
                    new XPathAstNode[]
                    {
                        new BinaryExpressionNode(
                            new FunctionCallNode("data", new[] { part }, "fn"),
                            BinaryOperator.SimpleMap,
                            new FunctionCallNode("string", new XPathAstNode[] { new ContextItemNode() }, "fn")),
                        new StringLiteralNode(" ")
                    },
                    "fn");

        var parts = node.Parts.Select(JoinPart).ToList();
        XPathAstNode desugared = parts.Count == 0
            ? new StringLiteralNode("")
            : new FunctionCallNode("string-join",
                new XPathAstNode[] { new SequenceExpressionNode(parts), new StringLiteralNode("") }, "fn");
        return LowerNode(desugared, targetReg);
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
        if (binding.DeclaredType is not null)
        {
            // XQuery 'as SequenceType': each bound item must be an instance of the type.
            int varReg = LoadVariable(new BoundVariable(binding.VariableName, binding.VariablePrefix, binding.VariableNamespaceUri));
            EmitEnforceTypeIfDeclared(binding, varReg, itemLevel: true);
            FreeRegister(varReg);
        }
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
        var info = new QuantifiedLoopInfo(binding.VariableName, rhsEntry, null, binding.VariablePrefix, binding.VariableNamespaceUri);
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
            // XQuery 'as SequenceType': the bound value must match the declared type.
            EmitEnforceTypeIfDeclared(binding, exprReg, itemLevel: false);
            // Store under the same key form used by variable references:
            // resolved (local, uri) tuple for Q{uri} names, "prefix:local" for
            // prefixed names (resolved at runtime), or the bare local name.
            int varPoolIdx = binding.VariableNamespaceUri is not null
                ? AddToLiteralPool((binding.VariableName, binding.VariableNamespaceUri))
                : binding.VariablePrefix is not null
                    ? AddToLiteralPool($"{binding.VariablePrefix}:{binding.VariableName}")
                    : AddToLiteralPool(binding.VariableName);
            Emit(IrOpCode.StoreVariable, 0, (ushort)exprReg, 0, varPoolIdx);
            FreeRegister(exprReg);
        }

        int bodyReg = LowerNode(node.Body, resultReg);
        if (bodyReg != resultReg)
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)bodyReg);

        return resultReg;
    }

    private void EmitEnforceTypeIfDeclared(QuantifiedBinding binding, int valueReg, bool itemLevel)
    {
        if (binding.DeclaredType is null)
            return;
        var typeName = binding.DeclaredType.Prefix is null
            ? binding.DeclaredType.TypeName
            : $"{binding.DeclaredType.Prefix}:{binding.DeclaredType.TypeName}";
        // For-bindings check each item (item level); let/grouping bindings check the
        // whole value against the declared sequence type. With 'allowing empty', the
        // empty-sequence binding is checked against the declared occurrence instead:
        // xs:integer? accepts (), xs:integer raises XPTY0004 (outer-012/013). A regular
        // (single-item) iteration matches any occurrence, so one instruction covers both.
        var occurrence = itemLevel && !binding.AllowingEmpty
            ? OccurrenceIndicator.One
            : binding.DeclaredType.Occurrence;
        var info = new EnforceTypeInfo(typeName, occurrence, "XPTY0004");
        int poolIdx = AddToLiteralPool(info);
        Emit(IrOpCode.EnforceType, (ushort)valueReg, 0, 0, poolIdx);
    }

    // ------------------------------------------------------------------
    // Full FLWOR (XQuery order by, etc.)
    // ------------------------------------------------------------------

    private readonly record struct BoundVariable(string Name, string? Prefix, string? NamespaceUri)
    {
        public object VariableKey => NamespaceUri is not null
            ? (Name, NamespaceUri)
            : Prefix is not null
                ? $"{Prefix}:{Name}"
                : Name;
    }

    private int LowerFlworExpression(FlworExpressionNode node, int? targetReg)
    {
        int resultReg = targetReg ?? AllocRegister();

        if (node.Clauses.Any(c => c is GroupByClauseNode))
        {
            return LowerFlworWithGrouping(node, resultReg);
        }

        if (!node.Clauses.Any(c => c is OrderByClauseNode or CountClauseNode or WindowClauseNode))
        {
            return LowerFlworWithoutOrderBy(node, resultReg);
        }

        return LowerFlworWithTuples(node, resultReg);
    }

    private int LowerFlworWithGrouping(FlworExpressionNode node, int resultReg)
    {
        // Locate the group by clause; only one per FLWOR expression is supported.
        int groupByIndex = -1;
        for (int i = 0; i < node.Clauses.Count; i++)
        {
            if (node.Clauses[i] is GroupByClauseNode)
            {
                if (groupByIndex >= 0)
                    throw new NotSupportedException("XPST0003: Multiple group by clauses in a single FLWOR expression are not supported.");
                groupByIndex = i;
            }
        }

        var groupByClause = (GroupByClauseNode)node.Clauses[groupByIndex];
        var preClauses = node.Clauses.Take(groupByIndex).ToList();
        var postClauses = node.Clauses.Skip(groupByIndex + 1).ToList();

        if (preClauses.Any(c => c is OrderByClauseNode))
            throw new NotSupportedException("XPST0003: An order by clause before group by is not supported; place order by after group by.");
        if (postClauses.Any(c => c is not OrderByClauseNode and not CountClauseNode and not WhereClauseNode and not LetClauseNode))
            throw new NotSupportedException("XPST0003: Only order by, count, where, and let clauses are supported after group by.");
        if (postClauses.Count(c => c is OrderByClauseNode) > 1)
            throw new NotSupportedException("XPST0003: Multiple order by clauses after group by are not supported.");

        // A post-grouping let that precedes order by is lowered after order-by re-keying,
        // so the order-by key cannot see the let variable. Reject only when an order-by key
        // actually references such a variable; harmless lets (e.g. used only in return/where)
        // are still allowed.
        int postOrderByIndex = postClauses.FindIndex(c => c is OrderByClauseNode);
        if (postOrderByIndex >= 0)
        {
            var postLetBoundVariables = postClauses
                .Take(postOrderByIndex)
                .OfType<LetClauseNode>()
                .SelectMany(l => l.Bindings)
                .Select(b => (b.VariableName, b.VariableNamespaceUri))
                .ToHashSet();
            if (postLetBoundVariables.Count > 0)
            {
                var orderByVars = postClauses
                    .OfType<OrderByClauseNode>()
                    .SelectMany(o => o.Specs)
                    .SelectMany(s => CollectVariableReferences(s.KeyExpression))
                    .Select(v => (v.LocalName, v.NamespaceUri));
                if (orderByVars.Any(v => postLetBoundVariables.Contains(v)))
                    throw new NotSupportedException("XPST0003: A let clause between group by and order by is not supported when the order-by key references the let variable.");
            }
        }

        // A grouping spec with ':=' behaves like a let binding evaluated per pre-grouping tuple.
        var syntheticBindings = groupByClause.Specs
            .Where(s => s.KeyExpression is not null)
            .Select(s => new QuantifiedBinding(s.VariableName, s.KeyExpression!, null, s.Prefix, s.NamespaceUri))
            .ToList();
        if (syntheticBindings.Count > 0)
        {
            preClauses = preClauses.Append(new LetClauseNode(syntheticBindings)).ToList();
        }

        var boundVariables = ComputeBoundVariables(preClauses);

        // Resolve each grouping variable to its tuple slot.
        var keyIndices = new List<int>(groupByClause.Specs.Count);
        var collations = new List<string?>(groupByClause.Specs.Count);
        var declaredTypeNames = new List<string?>(groupByClause.Specs.Count);
        var declaredTypeOccurrences = new List<OccurrenceIndicator>(groupByClause.Specs.Count);
        foreach (var spec in groupByClause.Specs)
        {
            int index = boundVariables.FindLastIndex(v => v.Name == spec.VariableName && v.NamespaceUri == spec.NamespaceUri);
            if (index < 0)
                throw new InvalidOperationException($"XPST0008: Grouping variable '${spec.VariableName}' is not bound in the FLWOR expression.");
            keyIndices.Add(index);
            collations.Add(spec.CollationUri);
            declaredTypeNames.Add(spec.DeclaredType is null
                ? null
                : spec.DeclaredType.Prefix is null ? spec.DeclaredType.TypeName : $"{spec.DeclaredType.Prefix}:{spec.DeclaredType.TypeName}");
            declaredTypeOccurrences.Add(spec.DeclaredType?.Occurrence ?? OccurrenceIndicator.One);
        }

        var countCounters = PrecomputeCountCounters(node.Clauses);
        InitializeCountCounters(countCounters);

        // Build a sequence of tuples: [var1, var2, ...]
        int tupleSeqReg = AllocRegister();
        var recursionBoundVariables = new List<BoundVariable>();
        LowerFlworTupleBuilder(preClauses, null, tupleSeqReg, recursionBoundVariables, countCounters);

        // Group the tuples.
        int groupedSeqReg = AllocRegister();
        var groupByInfo = new GroupByInfo(keyIndices, collations, declaredTypeNames, declaredTypeOccurrences);
        int groupByPoolIdx = AddToLiteralPool(groupByInfo);
        Emit(IrOpCode.GroupBy, (ushort)groupedSeqReg, (ushort)tupleSeqReg, 0, groupByPoolIdx);
        FreeRegister(tupleSeqReg);

        int iterationReg = groupedSeqReg;

        // An order by after group by is evaluated against the grouped bindings,
        // so the tuples must be re-keyed in a second pass before sorting.
        var postOrderByClause = postClauses.OfType<OrderByClauseNode>().FirstOrDefault();
        if (postOrderByClause is not null)
        {
            int rekeyedSeqReg = AllocRegister();
            LowerFlworRekeyForOrderBy(groupedSeqReg, boundVariables, postOrderByClause, rekeyedSeqReg);
            FreeRegister(groupedSeqReg);

            int sortedSeqReg = AllocRegister();
            var orderByInfo = new OrderByInfo(
                boundVariables.Count,
                postOrderByClause.Specs.Count,
                postOrderByClause.Specs.Select(s => s.Descending).ToArray(),
                postOrderByClause.Specs.Select(s => ResolveEmptyOrder(s.EmptyOrder)).ToArray(),
                postOrderByClause.Specs.Select(s => s.CollationUri).ToArray());
            int orderByPoolIdx = AddToLiteralPool(orderByInfo);
            Emit(IrOpCode.OrderBy, (ushort)sortedSeqReg, (ushort)rekeyedSeqReg, 0, orderByPoolIdx);
            FreeRegister(rekeyedSeqReg);
            iterationReg = sortedSeqReg;

            postClauses = postClauses.Where(c => c is not OrderByClauseNode).ToList();
        }

        // Iterate over (possibly sorted) grouped tuples, bind variables, handle post clauses, evaluate body.
        LowerFlworBodyIteration(iterationReg, boundVariables, postClauses, node.ReturnExpression, resultReg, countCounters);

        FreeRegister(iterationReg);
        return resultReg;
    }

    private void LowerFlworRekeyForOrderBy(
        int groupedSeqReg,
        IReadOnlyList<BoundVariable> boundVariables,
        OrderByClauseNode orderByClause,
        int resultReg)
    {
        int forIdx = _instructions.Count;
        Emit(IrOpCode.For, (ushort)resultReg, (ushort)groupedSeqReg, 0, 0);

        int jumpIdx = _instructions.Count;
        Emit(IrOpCode.Jump, 0, 0, 0, 0);

        int rhsEntry = _instructions.Count;

        // Bind the grouped tuple to the original variables so the order by keys
        // are evaluated against the grouped bindings.
        int tupleReg = AllocRegister();
        int tupleVarPoolIdx = AddToLiteralPool("__flwor_tuple");
        Emit(IrOpCode.LoadVariable, (ushort)tupleReg, 0, 0, tupleVarPoolIdx);

        var tupleBindInfo = new TupleBindInfo(boundVariables.Select(v => (v.Name, v.Prefix, v.NamespaceUri)).ToArray());
        int tupleBindPoolIdx = AddToLiteralPool(tupleBindInfo);
        Emit(IrOpCode.TupleBind, (ushort)tupleReg, 0, 0, tupleBindPoolIdx);
        FreeRegister(tupleReg);

        // Build a new tuple: [var1, var2, ..., key1, key2, ...]
        int newTupleReg = AllocRegister();
        Emit(IrOpCode.Array, (ushort)newTupleReg);

        foreach (var var in boundVariables)
        {
            int varReg = LoadVariable(var);
            Emit(IrOpCode.ArrayAdd, (ushort)newTupleReg, (ushort)varReg);
            FreeRegister(varReg);
        }

        foreach (var spec in orderByClause.Specs)
        {
            int keyReg = LowerNode(spec.KeyExpression);
            Emit(IrOpCode.ArrayAdd, (ushort)newTupleReg, (ushort)keyReg);
            FreeRegister(keyReg);
        }

        Emit(IrOpCode.Return, (ushort)newTupleReg);
        FreeRegister(newTupleReg);

        int afterRhs = _instructions.Count;
        var info = new QuantifiedLoopInfo("__flwor_tuple", rhsEntry);
        int poolIdx = AddToLiteralPool(info);
        PatchInstruction(forIdx, IrOpCode.For, (ushort)resultReg, (ushort)groupedSeqReg, 0, poolIdx);
        PatchInstruction(jumpIdx, IrOpCode.Jump, 0, 0, 0, afterRhs);
    }

    private int LowerFlworWithoutOrderBy(FlworExpressionNode node, int resultReg)
    {
        // Synthesize nested For/Let/If AST and lower it.
        XPathAstNode body = node.ReturnExpression;
        for (int i = node.Clauses.Count - 1; i >= 0; i--)
        {
            body = node.Clauses[i] switch
            {
                ForClauseNode forClause => new ForExpressionNode(forClause.Bindings, body),
                LetClauseNode letClause => new LetExpressionNode(letClause.Bindings, body),
                WhereClauseNode whereClause => new IfExpressionNode(whereClause.Condition, body, new SequenceExpressionNode(Array.Empty<XPathAstNode>())),
                _ => body
            };
        }

        int bodyReg = LowerNode(body, resultReg);
        if (bodyReg != resultReg)
            Emit(IrOpCode.Move, (ushort)resultReg, (ushort)bodyReg);

        return resultReg;
    }

    private int LowerFlworWithTuples(FlworExpressionNode node, int resultReg)
    {
        // Find the last order by clause (-1 if there is none).
        int orderByIndex = -1;
        for (int i = node.Clauses.Count - 1; i >= 0; i--)
        {
            if (node.Clauses[i] is OrderByClauseNode)
            {
                orderByIndex = i;
                break;
            }
        }

        var preClauses = orderByIndex >= 0 ? node.Clauses.Take(orderByIndex).ToList() : node.Clauses.ToList();
        var postClauses = orderByIndex >= 0 ? node.Clauses.Skip(orderByIndex + 1).ToList() : new List<FlworClauseNode>();
        var orderByClause = orderByIndex >= 0 ? (OrderByClauseNode)node.Clauses[orderByIndex] : null;

        if (preClauses.Any(c => c is OrderByClauseNode))
            throw new NotSupportedException("XPST0003: Only one order by clause is supported in a FLWOR expression.");
        if (postClauses.Any(c => c is not CountClauseNode and not WhereClauseNode and not LetClauseNode))
            throw new NotSupportedException("XPST0003: Only count, where, and let clauses are supported after an order by clause.");

        // The variables bound by all for/let/count clauses before order by must be captured
        // before the tuple builder recurses, because that builder adds and removes
        // variables as it unwinds.
        var boundVariables = ComputeBoundVariables(preClauses);

        // Each count clause needs a compiler-managed integer counter that persists
        // across tuple-builder invocations inside the For loops.
        var countCounters = PrecomputeCountCounters(node.Clauses);
        InitializeCountCounters(countCounters);

        // Build a sequence of tuples: [var1, var2, ..., key1, key2, ...]
        int tupleSeqReg = AllocRegister();
        var recursionBoundVariables = new List<BoundVariable>();
        LowerFlworTupleBuilder(preClauses, orderByClause, tupleSeqReg, recursionBoundVariables, countCounters);

        int iterationReg = tupleSeqReg;
        if (orderByClause is not null)
        {
            // Apply OrderBy.
            int sortedTupleSeqReg = AllocRegister();
            var orderByInfo = new OrderByInfo(
                boundVariables.Count,
                orderByClause.Specs.Count,
                orderByClause.Specs.Select(s => s.Descending).ToArray(),
                orderByClause.Specs.Select(s => ResolveEmptyOrder(s.EmptyOrder)).ToArray(),
                orderByClause.Specs.Select(s => s.CollationUri).ToArray());
            int orderByPoolIdx = AddToLiteralPool(orderByInfo);
            Emit(IrOpCode.OrderBy, (ushort)sortedTupleSeqReg, (ushort)tupleSeqReg, 0, orderByPoolIdx);
            FreeRegister(tupleSeqReg);
            iterationReg = sortedTupleSeqReg;
        }

        // Iterate over tuples, bind variables, handle post-order-by clauses, evaluate body.
        LowerFlworBodyIteration(iterationReg, boundVariables, postClauses, node.ReturnExpression, resultReg, countCounters);

        FreeRegister(iterationReg);
        return resultReg;
    }

    private readonly record struct CountCounterInfo(CountClauseNode Clause, string CounterName);

    private List<CountCounterInfo> PrecomputeCountCounters(IReadOnlyList<FlworClauseNode> clauses)
    {
        var result = new List<CountCounterInfo>();
        foreach (var clause in clauses)
        {
            if (clause is CountClauseNode countClause)
            {
                result.Add(new CountCounterInfo(countClause, $"__flwor_count_{_nextCountCounter++}"));
            }
        }
        return result;
    }

    private void InitializeCountCounters(List<CountCounterInfo> countCounters)
    {
        if (countCounters.Count == 0) return;

        int zeroReg = AllocRegister();
        int zeroPoolIdx = AddToLiteralPool(0L);
        Emit(IrOpCode.LoadInteger, (ushort)zeroReg, operand: zeroPoolIdx);
        foreach (var info in countCounters)
        {
            int counterPoolIdx = AddToLiteralPool(info.CounterName);
            Emit(IrOpCode.StoreVariable, 0, (ushort)zeroReg, 0, counterPoolIdx);
        }
        FreeRegister(zeroReg);
    }

    private string GetCountCounterName(CountClauseNode clause, List<CountCounterInfo> countCounters)
    {
        foreach (var info in countCounters)
        {
            // Reference equality: two count clauses may bind the same variable name and
            // are still distinct counters (records compare by value otherwise).
            if (ReferenceEquals(info.Clause, clause))
                return info.CounterName;
        }
        throw new InvalidOperationException("Count clause has no allocated counter.");
    }

    private void EmitIncrementCount(CountClauseNode countClause, List<CountCounterInfo> countCounters)
    {
        string counterName = GetCountCounterName(countClause, countCounters);
        int counterPoolIdx = AddToLiteralPool(counterName);

        int oldCounterReg = AllocRegister();
        Emit(IrOpCode.LoadVariable, (ushort)oldCounterReg, 0, 0, counterPoolIdx);

        int oneReg = AllocRegister();
        int onePoolIdx = AddToLiteralPool(1L);
        Emit(IrOpCode.LoadInteger, (ushort)oneReg, operand: onePoolIdx);

        int newCounterReg = AllocRegister();
        Emit(IrOpCode.Add, (ushort)newCounterReg, (ushort)oldCounterReg, (ushort)oneReg);
        FreeRegister(oldCounterReg);
        FreeRegister(oneReg);

        Emit(IrOpCode.StoreVariable, 0, (ushort)newCounterReg, 0, counterPoolIdx);

        var countVar = new BoundVariable(countClause.VariableName, countClause.Prefix, countClause.NamespaceUri);
        int countVarPoolIdx = AddToLiteralPool(countVar.VariableKey);
        Emit(IrOpCode.StoreVariable, 0, (ushort)newCounterReg, 0, countVarPoolIdx);
        FreeRegister(newCounterReg);
    }

    private static List<VariableReferenceNode> CollectVariableReferences(XPathAstNode node)
    {
        var result = new List<VariableReferenceNode>();
        CollectVariableReferencesCore(node, result);
        return result;
    }

    private static void CollectVariableReferencesCore(XPathAstNode node, List<VariableReferenceNode> result)
    {
        switch (node)
        {
            case VariableReferenceNode varRef:
                result.Add(varRef);
                return;
            case ParenthesizedExprNode p:
                CollectVariableReferencesCore(p.Expression, result);
                return;
            case BinaryExpressionNode b:
                CollectVariableReferencesCore(b.Left, result);
                CollectVariableReferencesCore(b.Right, result);
                return;
            case UnaryExpressionNode u:
                CollectVariableReferencesCore(u.Operand, result);
                return;
            case IfExpressionNode i:
                CollectVariableReferencesCore(i.Condition, result);
                CollectVariableReferencesCore(i.ThenBranch, result);
                CollectVariableReferencesCore(i.ElseBranch, result);
                return;
            case SequenceExpressionNode s:
                foreach (var e in s.Expressions) CollectVariableReferencesCore(e, result);
                return;
            case RangeExpressionNode r:
                CollectVariableReferencesCore(r.From, result);
                CollectVariableReferencesCore(r.To, result);
                return;
            case PathExprNode p:
                foreach (var s in p.Steps) CollectVariableReferencesCore(s, result);
                return;
            case StepNode s:
                foreach (var pred in s.Predicates) CollectVariableReferencesCore(pred, result);
                return;
            case PredicateNode p:
                CollectVariableReferencesCore(p.Expression, result);
                return;
            case FunctionCallNode f:
                foreach (var a in f.Arguments) CollectVariableReferencesCore(a, result);
                return;
            case NamedFunctionRefNode:
            case ContextItemNode:
            case ArgumentPlaceholderNode:
                return;
            case DynamicFunctionCallNode d:
                CollectVariableReferencesCore(d.Function, result);
                foreach (var a in d.Arguments) CollectVariableReferencesCore(a, result);
                return;
            case CastNode c:
                CollectVariableReferencesCore(c.Expression, result);
                return;
            case CastableNode c:
                CollectVariableReferencesCore(c.Expression, result);
                return;
            case InstanceOfNode i:
                CollectVariableReferencesCore(i.Expression, result);
                return;
            case TreatNode t:
                CollectVariableReferencesCore(t.Expression, result);
                return;
            case ArrowExprNode a:
                CollectVariableReferencesCore(a.Source, result);
                CollectVariableReferencesCore(a.Target, result);
                return;
            case LookupNode l:
                CollectVariableReferencesCore(l.Expression, result);
                CollectVariableReferencesCore(l.Key, result);
                return;
            case LookupWildcardNode l:
                CollectVariableReferencesCore(l.Expression, result);
                return;
            case MapConstructorNode m:
                foreach (var e in m.Entries)
                {
                    CollectVariableReferencesCore(e.Key, result);
                    CollectVariableReferencesCore(e.Value, result);
                }
                return;
            case ArrayConstructorNode a:
                foreach (var i in a.Items) CollectVariableReferencesCore(i, result);
                return;
            case ForExpressionNode f:
                foreach (var b in f.Bindings) CollectVariableReferencesCore(b.Expression, result);
                CollectVariableReferencesCore(f.ReturnExpression, result);
                return;
            case LetExpressionNode l:
                foreach (var b in l.Bindings) CollectVariableReferencesCore(b.Expression, result);
                CollectVariableReferencesCore(l.Body, result);
                return;
            case QuantifiedExpressionNode q:
                foreach (var b in q.Bindings) CollectVariableReferencesCore(b.Expression, result);
                CollectVariableReferencesCore(q.SatisfiesExpression, result);
                return;
            case SwitchExpressionNode s:
                CollectVariableReferencesCore(s.Operand, result);
                foreach (var c in s.Cases)
                {
                    foreach (var v in c.Values) CollectVariableReferencesCore(v, result);
                    CollectVariableReferencesCore(c.Return, result);
                }
                CollectVariableReferencesCore(s.Default, result);
                return;
            case TypeswitchExpressionNode t:
                CollectVariableReferencesCore(t.Operand, result);
                foreach (var c in t.Cases) CollectVariableReferencesCore(c.Return, result);
                CollectVariableReferencesCore(t.Default, result);
                return;
            case TryCatchNode t:
                CollectVariableReferencesCore(t.TryExpression, result);
                foreach (var c in t.Clauses) CollectVariableReferencesCore(c.Expression, result);
                return;
            case InlineFunctionNode i:
                CollectVariableReferencesCore(i.Body, result);
                return;
            case DirectElementConstructorNode e:
                foreach (var a in e.Attributes)
                {
                    foreach (var v in a.ValueParts) CollectVariableReferencesCore(v, result);
                }
                foreach (var c in e.Content) CollectVariableReferencesCore(c, result);
                return;
            case ComputedElementConstructorNode e:
                if (e.NameExpression is not null) CollectVariableReferencesCore(e.NameExpression, result);
                CollectVariableReferencesCore(e.ContentExpression, result);
                return;
            case ComputedAttributeConstructorNode a:
                if (a.NameExpression is not null) CollectVariableReferencesCore(a.NameExpression, result);
                CollectVariableReferencesCore(a.ValueExpression, result);
                return;
            case ComputedDocumentConstructorNode d:
                CollectVariableReferencesCore(d.ContentExpression, result);
                return;
            case ComputedTextConstructorNode t:
                CollectVariableReferencesCore(t.ValueExpression, result);
                return;
            case ComputedCommentConstructorNode c:
                CollectVariableReferencesCore(c.ValueExpression, result);
                return;
            case ComputedPIConstructorNode p:
                if (p.TargetExpression is not null) CollectVariableReferencesCore(p.TargetExpression, result);
                CollectVariableReferencesCore(p.ValueExpression, result);
                return;
            case ComputedNamespaceConstructorNode n:
                if (n.PrefixExpression is not null) CollectVariableReferencesCore(n.PrefixExpression, result);
                CollectVariableReferencesCore(n.UriExpression, result);
                return;
            case StringConstructorNode s:
                foreach (var p in s.Parts) CollectVariableReferencesCore(p, result);
                return;
            case PostfixPredicateNode p:
                CollectVariableReferencesCore(p.Expression, result);
                CollectVariableReferencesCore(p.Predicate, result);
                return;
            case FlworExpressionNode f:
                // Variables bound by FLWOR clauses are scoped within the expression; only
                // the return expression and any unbound clause expressions can reference
                // outer variables.
                foreach (var clause in f.Clauses)
                {
                    switch (clause)
                    {
                        case ForClauseNode fc:
                            foreach (var b in fc.Bindings) CollectVariableReferencesCore(b.Expression, result);
                            break;
                        case LetClauseNode lc:
                            foreach (var b in lc.Bindings) CollectVariableReferencesCore(b.Expression, result);
                            break;
                        case WhereClauseNode w:
                            CollectVariableReferencesCore(w.Condition, result);
                            break;
                        case OrderByClauseNode o:
                            foreach (var s in o.Specs) CollectVariableReferencesCore(s.KeyExpression, result);
                            break;
                        case GroupByClauseNode g:
                            foreach (var s in g.Specs)
                            {
                                if (s.KeyExpression is not null) CollectVariableReferencesCore(s.KeyExpression, result);
                            }
                            break;
                        case WindowClauseNode w:
                            CollectVariableReferencesCore(w.InExpression, result);
                            CollectVariableReferencesCore(w.StartCondition.WhenExpression, result);
                            if (w.EndCondition is not null) CollectVariableReferencesCore(w.EndCondition.WhenExpression, result);
                            break;
                    }
                }
                CollectVariableReferencesCore(f.ReturnExpression, result);
                return;
            default:
                return;
        }
    }

    private static List<BoundVariable> ComputeBoundVariables(IReadOnlyList<FlworClauseNode> clauses)
    {
        var result = new List<BoundVariable>();
        foreach (var clause in clauses)
        {
            if (clause is ForClauseNode forClause)
            {
                foreach (var b in forClause.Bindings)
                {
                    result.Add(new BoundVariable(b.VariableName, b.VariablePrefix, b.VariableNamespaceUri));
                    if (b.PositionalVariableName is not null)
                        result.Add(new BoundVariable(b.PositionalVariableName, null, null));
                }
            }
            else if (clause is LetClauseNode letClause)
            {
                foreach (var b in letClause.Bindings)
                    result.Add(new BoundVariable(b.VariableName, b.VariablePrefix, b.VariableNamespaceUri));
            }
            else if (clause is CountClauseNode countClause)
            {
                result.Add(new BoundVariable(countClause.VariableName, countClause.Prefix, countClause.NamespaceUri));
            }
            else if (clause is WindowClauseNode windowClause)
            {
                result.AddRange(GetWindowBoundVariables(windowClause));
            }
        }
        return result;
    }

    private static List<BoundVariable> GetWindowBoundVariables(WindowClauseNode windowClause)
    {
        var result = new List<BoundVariable>
        {
            new(windowClause.VariableName, windowClause.Prefix, windowClause.NamespaceUri)
        };
        void AddConditionVar(string? name)
        {
            if (name is not null)
            {
                // Condition variable names are stored in lexical form (prefix:local or
                // Q{uri}local); split them so both forms key consistently with the
                // variable references that resolve against declared namespaces.
                var (local, prefix, ns) = SplitVariableName(name);
                result.Add(new BoundVariable(local, prefix, ns));
            }
        }
        AddConditionVar(windowClause.StartCondition.CurrentItemVariable);
        AddConditionVar(windowClause.StartCondition.PositionalVariable);
        AddConditionVar(windowClause.StartCondition.PreviousItemVariable);
        AddConditionVar(windowClause.StartCondition.NextItemVariable);
        if (windowClause.EndCondition is not null)
        {
            AddConditionVar(windowClause.EndCondition.CurrentItemVariable);
            AddConditionVar(windowClause.EndCondition.PositionalVariable);
            AddConditionVar(windowClause.EndCondition.PreviousItemVariable);
            AddConditionVar(windowClause.EndCondition.NextItemVariable);
        }
        return result;
    }

    // Splits a lexical variable name (local, prefix:local, or Q{uri}local) into its parts.
    private static (string Local, string? Prefix, string? NamespaceUri) SplitVariableName(string name)
    {
        if (name.StartsWith("Q{", StringComparison.Ordinal))
        {
            int closeBrace = name.IndexOf('}');
            if (closeBrace > 1)
                return (name[(closeBrace + 1)..], null, name[2..closeBrace]);
        }
        int colon = name.IndexOf(':');
        return colon < 0 ? (name, null, null) : (name[(colon + 1)..], name[..colon], null);
    }

    private void LowerFlworTupleBuilder(
        IReadOnlyList<FlworClauseNode> clauses,
        OrderByClauseNode? orderByClause,
        int resultReg,
        List<BoundVariable> boundVariables,
        List<CountCounterInfo> countCounters,
        bool insideRhs = false)
    {
        if (clauses.Count == 0)
        {
            // Build a single tuple as an array and return it.
            // Arrays are not flattened by the outer For loop, preserving tuple identity.
            int tupleReg = AllocRegister();
            Emit(IrOpCode.Array, (ushort)tupleReg);

            foreach (var var in boundVariables)
            {
                int varReg = LoadVariable(var);
                Emit(IrOpCode.ArrayAdd, (ushort)tupleReg, (ushort)varReg);
                FreeRegister(varReg);
            }

            if (orderByClause is not null)
            {
                foreach (var spec in orderByClause.Specs)
                {
                    int keyReg = LowerNode(spec.KeyExpression);
                    Emit(IrOpCode.ArrayAdd, (ushort)tupleReg, (ushort)keyReg);
                    FreeRegister(keyReg);
                }
            }

            if (insideRhs)
            {
                // Inside a For/Window block: return the tuple to the enclosing iteration.
                Emit(IrOpCode.Return, (ushort)tupleReg);
            }
            else
            {
                // Top level (no enclosing iteration): wrap the single tuple into the
                // tuple-sequence register and fall through to the barrier opcodes.
                Emit(IrOpCode.SequenceStart, (ushort)resultReg);
                Emit(IrOpCode.SequenceAdd, (ushort)resultReg, (ushort)tupleReg);
                Emit(IrOpCode.SequenceEnd, (ushort)resultReg);
            }
            FreeRegister(tupleReg);
            return;
        }

        var clause = clauses[0];
        var restClauses = clauses.Skip(1).ToList();

        if (clause is ForClauseNode forClause)
        {
            LowerForClauseForTuples(forClause.Bindings, 0, restClauses, orderByClause, resultReg, boundVariables, countCounters, insideRhs);
        }
        else if (clause is LetClauseNode letClause)
        {
            foreach (var binding in letClause.Bindings)
            {
                int exprReg = LowerNode(binding.Expression);
                // XQuery 'as SequenceType': the bound value must match the declared type.
                EmitEnforceTypeIfDeclared(binding, exprReg, itemLevel: false);
                StoreVariable(binding, exprReg);
                FreeRegister(exprReg);
                boundVariables.Add(new BoundVariable(binding.VariableName, binding.VariablePrefix, binding.VariableNamespaceUri));
            }
            LowerFlworTupleBuilder(restClauses, orderByClause, resultReg, boundVariables, countCounters, insideRhs);
            foreach (var _ in letClause.Bindings)
            {
                boundVariables.RemoveAt(boundVariables.Count - 1);
            }
        }
        else if (clause is WhereClauseNode whereClause)
        {
            int condReg = LowerNode(whereClause.Condition);
            int jumpToEmpty = EmitJumpPlaceholder(IrOpCode.JumpIfFalse, (ushort)condReg);
            FreeRegister(condReg);
            LowerFlworTupleBuilder(restClauses, orderByClause, resultReg, boundVariables, countCounters, insideRhs);
            int emptyLabel = CurrentInstructionIndex;
            Emit(IrOpCode.LoadEmptySequence, (ushort)resultReg);
            Emit(IrOpCode.Return, (ushort)resultReg);
            PatchJump(jumpToEmpty, emptyLabel);
        }
        else if (clause is CountClauseNode countClause)
        {
            EmitIncrementCount(countClause, countCounters);
            boundVariables.Add(new BoundVariable(countClause.VariableName, countClause.Prefix, countClause.NamespaceUri));
            LowerFlworTupleBuilder(restClauses, orderByClause, resultReg, boundVariables, countCounters, insideRhs);
            boundVariables.RemoveAt(boundVariables.Count - 1);
        }
        else if (clause is WindowClauseNode windowClause)
        {
            LowerWindowClauseForTuples(windowClause, restClauses, orderByClause, resultReg, boundVariables, countCounters, insideRhs);
        }
    }

    private void LowerWindowClauseForTuples(
        WindowClauseNode windowClause,
        IReadOnlyList<FlworClauseNode> restClauses,
        OrderByClauseNode? orderByClause,
        int resultReg,
        List<BoundVariable> boundVariables,
        List<CountCounterInfo> countCounters,
        bool insideRhs)
    {
        int seqReg = LowerNode(windowClause.InExpression);

        int windowIdx = _instructions.Count;
        Emit(IrOpCode.Window, (ushort)resultReg, (ushort)seqReg, 0, 0);
        FreeRegister(seqReg);

        int jumpIdx = _instructions.Count;
        Emit(IrOpCode.Jump, 0, 0, 0, 0);

        // Start-condition block: the when-expression is evaluated by the VM with
        // the start WindowVars (current/positional/previous/next) bound.
        int startEntry = _instructions.Count;
        int startReg = LowerNode(windowClause.StartCondition.WhenExpression);
        Emit(IrOpCode.Return, (ushort)startReg);
        FreeRegister(startReg);

        // End-condition block (-1 when the clause has no end condition).
        int endEntry = -1;
        if (windowClause.EndCondition is not null)
        {
            endEntry = _instructions.Count;
            int endReg = LowerNode(windowClause.EndCondition.WhenExpression);
            Emit(IrOpCode.Return, (ushort)endReg);
            FreeRegister(endReg);
        }

        // Window body block: the VM binds the window variable and the start/end
        // condition variables for each produced window, then continues the tuple build.
        int rhsEntry = _instructions.Count;
        var windowVars = GetWindowBoundVariables(windowClause);
        boundVariables.AddRange(windowVars);
        LowerFlworTupleBuilder(restClauses, orderByClause, resultReg, boundVariables, countCounters, insideRhs: true);
        boundVariables.RemoveRange(boundVariables.Count - windowVars.Count, windowVars.Count);

        int afterRhs = _instructions.Count;
        if (insideRhs)
        {
            // Nested inside another iteration's rhs: after the Window completes, yield
            // its accumulated tuples to the enclosing block.
            Emit(IrOpCode.Return, (ushort)resultReg);
        }
        var info = new WindowInfo(
            // The lexical variable name (prefix:local or Q{uri}local) so the VM can
            // resolve the binding exactly the way references to the variable resolve.
            windowClause.NamespaceUri is not null
                ? $"Q{{{windowClause.NamespaceUri}}}{windowClause.VariableName}"
                : windowClause.Prefix is not null
                    ? $"{windowClause.Prefix}:{windowClause.VariableName}"
                    : windowClause.VariableName,
            windowClause.NamespaceUri,
            windowClause.Sliding,
            windowClause.OnlyEnd,
            startEntry,
            endEntry,
            rhsEntry,
            windowClause.StartCondition.CurrentItemVariable,
            windowClause.StartCondition.PositionalVariable,
            windowClause.StartCondition.PreviousItemVariable,
            windowClause.StartCondition.NextItemVariable,
            windowClause.EndCondition?.CurrentItemVariable,
            windowClause.EndCondition?.PositionalVariable,
            windowClause.EndCondition?.PreviousItemVariable,
            windowClause.EndCondition?.NextItemVariable,
            windowClause.DeclaredType is null
                ? null
                : windowClause.DeclaredType.Prefix is null
                    ? windowClause.DeclaredType.TypeName
                    : $"{windowClause.DeclaredType.Prefix}:{windowClause.DeclaredType.TypeName}",
            windowClause.DeclaredType?.Occurrence ?? OccurrenceIndicator.One);
        int poolIdx = AddToLiteralPool(info);
        PatchInstruction(windowIdx, IrOpCode.Window, (ushort)resultReg, (ushort)seqReg, 0, poolIdx);
        PatchInstruction(jumpIdx, IrOpCode.Jump, 0, 0, 0, afterRhs);
    }

    private void LowerForClauseForTuples(
        IReadOnlyList<QuantifiedBinding> bindings,
        int index,
        IReadOnlyList<FlworClauseNode> restClauses,
        OrderByClauseNode? orderByClause,
        int resultReg,
        List<BoundVariable> boundVariables,
        List<CountCounterInfo> countCounters,
        bool insideRhs)
    {
        var binding = bindings[index];
        int seqReg = LowerNode(binding.Expression);

        int forIdx = _instructions.Count;
        Emit(IrOpCode.For, (ushort)resultReg, (ushort)seqReg, 0, 0);
        FreeRegister(seqReg);

        int jumpIdx = _instructions.Count;
        Emit(IrOpCode.Jump, 0, 0, 0, 0);

        int rhsEntry = _instructions.Count;
        boundVariables.Add(new BoundVariable(binding.VariableName, binding.VariablePrefix, binding.VariableNamespaceUri));
        if (binding.PositionalVariableName is not null)
            boundVariables.Add(new BoundVariable(binding.PositionalVariableName, null, null));
        if (binding.DeclaredType is not null)
        {
            // XQuery 'as SequenceType': each bound item must be an instance of the type.
            int varReg = LoadVariable(new BoundVariable(binding.VariableName, binding.VariablePrefix, binding.VariableNamespaceUri));
            EmitEnforceTypeIfDeclared(binding, varReg, itemLevel: true);
            FreeRegister(varReg);
        }

        if (index == bindings.Count - 1)
        {
            LowerFlworTupleBuilder(restClauses, orderByClause, resultReg, boundVariables, countCounters, insideRhs: true);
        }
        else
        {
            LowerForClauseForTuples(bindings, index + 1, restClauses, orderByClause, resultReg, boundVariables, countCounters, insideRhs: true);
        }

        boundVariables.RemoveAt(boundVariables.Count - 1);
        if (binding.PositionalVariableName is not null)
            boundVariables.RemoveAt(boundVariables.Count - 1);

        int afterRhs = _instructions.Count;
        if (insideRhs)
        {
            // Nested inside another iteration's rhs: after this For completes, yield its
            // accumulated tuples to the enclosing block.
            Emit(IrOpCode.Return, (ushort)resultReg);
        }
        var info = new QuantifiedLoopInfo(binding.VariableName, rhsEntry, binding.PositionalVariableName, binding.VariablePrefix, binding.VariableNamespaceUri, binding.AllowingEmpty);
        int poolIdx = AddToLiteralPool(info);
        PatchInstruction(forIdx, IrOpCode.For, (ushort)resultReg, (ushort)seqReg, 0, poolIdx);
        PatchInstruction(jumpIdx, IrOpCode.Jump, 0, 0, 0, afterRhs);
    }

    private void LowerFlworBodyIteration(
        int tupleSeqReg,
        IReadOnlyList<BoundVariable> boundVariables,
        IReadOnlyList<FlworClauseNode> postClauses,
        XPathAstNode returnExpression,
        int resultReg,
        List<CountCounterInfo> countCounters)
    {
        int forIdx = _instructions.Count;
        Emit(IrOpCode.For, (ushort)resultReg, (ushort)tupleSeqReg, 0, 0);

        int jumpIdx = _instructions.Count;
        Emit(IrOpCode.Jump, 0, 0, 0, 0);

        int rhsEntry = _instructions.Count;

        // Bind the tuple to a temporary variable, then extract all original variables.
        int tupleReg = AllocRegister();
        int tupleVarPoolIdx = AddToLiteralPool("__flwor_tuple");
        Emit(IrOpCode.LoadVariable, (ushort)tupleReg, 0, 0, tupleVarPoolIdx);

        var tupleBindInfo = new TupleBindInfo(boundVariables.Select(v => (v.Name, v.Prefix, v.NamespaceUri)).ToArray());
        int tupleBindPoolIdx = AddToLiteralPool(tupleBindInfo);
        Emit(IrOpCode.TupleBind, (ushort)tupleReg, 0, 0, tupleBindPoolIdx);
        FreeRegister(tupleReg);

        // Handle post-order-by clauses (count, where, and let are supported).
        var whereSkipJumps = new List<int>();
        var scopedNames = new List<string>(boundVariables.Select(v => v.Name).Distinct());
        foreach (var postClause in postClauses)
        {
            if (postClause is CountClauseNode countClause)
            {
                EmitIncrementCount(countClause, countCounters);
            }
            else if (postClause is WhereClauseNode whereClause)
            {
                int condReg = LowerNode(whereClause.Condition);
                int skipJump = EmitJumpPlaceholder(IrOpCode.JumpIfFalse, (ushort)condReg);
                whereSkipJumps.Add(skipJump);
                FreeRegister(condReg);
            }
            else if (postClause is LetClauseNode letClause)
            {
                foreach (var binding in letClause.Bindings)
                {
                    int exprReg = LowerNode(binding.Expression);
                    StoreVariable(binding, exprReg);
                    FreeRegister(exprReg);
                    scopedNames.Add(binding.VariableName);
                }
            }
        }

        int bodyReg = LowerNode(returnExpression);
        Emit(IrOpCode.Return, (ushort)bodyReg);
        FreeRegister(bodyReg);

        // Where clauses that evaluate to false skip the body and return an empty sequence
        // so the enclosing For loop adds nothing for this iteration.
        foreach (int skipJump in whereSkipJumps)
        {
            int skipLabel = CurrentInstructionIndex;
            PatchJump(skipJump, skipLabel);
            int emptyReg = AllocRegister();
            Emit(IrOpCode.LoadEmptySequence, (ushort)emptyReg);
            Emit(IrOpCode.Return, (ushort)emptyReg);
            FreeRegister(emptyReg);
        }

        int afterRhs = _instructions.Count;
        // The body For saves and restores the tuple-bound variables so they do not leak
        // out of the FLWOR's lexical scope.
        var info = new QuantifiedLoopInfo("__flwor_tuple", rhsEntry, ScopedVariableNames: scopedNames.ToArray());
        int poolIdx = AddToLiteralPool(info);
        PatchInstruction(forIdx, IrOpCode.For, (ushort)resultReg, (ushort)tupleSeqReg, 0, poolIdx);
        PatchInstruction(jumpIdx, IrOpCode.Jump, 0, 0, 0, afterRhs);
    }

    private int LoadVariable(BoundVariable variable)
    {
        int reg = AllocRegister();
        int poolIdx = AddToLiteralPool(variable.VariableKey);
        Emit(IrOpCode.LoadVariable, (ushort)reg, 0, 0, poolIdx);
        return reg;
    }

    private void StoreVariable(QuantifiedBinding binding, int exprReg)
    {
        int varPoolIdx = binding.VariableNamespaceUri is not null
            ? AddToLiteralPool((binding.VariableName, binding.VariableNamespaceUri))
            : binding.VariablePrefix is not null
                ? AddToLiteralPool($"{binding.VariablePrefix}:{binding.VariableName}")
                : AddToLiteralPool(binding.VariableName);
        Emit(IrOpCode.StoreVariable, 0, (ushort)exprReg, 0, varPoolIdx);
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
