// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Base type for all nodes in the XPath Abstract Syntax Tree. The AST is immutable and produced by t...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Added LookupWildcardNode                                                               |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Added OccurrenceIndicator to type expression nodes                                       |
//                      | Charles Korthout | 0.4   | 15-07-2026     | Added PositionalVariableName to QuantifiedBinding (FLWOR 'at $pos')                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 15-07-2026     | Added VariablePrefix/VariableNamespaceUri to QuantifiedBinding (EQName variables)       |
//                      | Charles Korthout | 1.0   | 22-07-2026     | Added FlworExpressionNode and clause nodes for full XQuery FLWOR (order by)            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.1   | 23-07-2026     | Added CountClauseNode for XQuery FLWOR count clause                                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.2   | 25-07-2026     | Added GroupByClauseNode and GroupingSpec for XQuery FLWOR group by                      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.3   | 25-07-2026     | Added WindowClauseNode and WindowCondition for XQuery FLWOR window clause               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Parser.Ast;

/// <summary>
/// Base type for all nodes in the XPath Abstract Syntax Tree.
/// The AST is immutable and produced by the parser.
/// </summary>
public abstract record XPathAstNode
{
    /// <summary>Source span for diagnostic reporting.</summary>
    public TextSpan Span { get; init; }
}

/// <summary>Represents a region in the source XPath text.</summary>
public readonly record struct TextSpan(int Start, int Length)
{
    public int End => Start + Length;
}

// ------------------------------------------------------------------
// Literals & Variables
// ------------------------------------------------------------------

public sealed record BooleanLiteralNode(bool Value) : XPathAstNode;
public sealed record IntegerLiteralNode(long Value) : XPathAstNode;
public sealed record DecimalLiteralNode(decimal Value) : XPathAstNode;
public sealed record DoubleLiteralNode(double Value) : XPathAstNode;
public sealed record StringLiteralNode(string Value) : XPathAstNode;

public sealed record VariableReferenceNode(string LocalName, string? Prefix = null, string? NamespaceUri = null) : XPathAstNode;

// ------------------------------------------------------------------
// Path & Steps
// ------------------------------------------------------------------

/// <summary>A single step in a path expression.</summary>
public sealed record StepNode(XdmAxis Axis, NodeTest NodeTest, IReadOnlyList<XPathAstNode> Predicates) : XPathAstNode;

/// <summary>A path expression (relative or absolute).</summary>
public sealed record PathExprNode(bool IsAbsolute, IReadOnlyList<XPathAstNode> Steps) : XPathAstNode;

/// <summary>The context item expression: <c>.</c></summary>
public sealed record ContextItemNode : XPathAstNode;

/// <summary>Parenthesized expression: <c>(expr)</c></summary>
public sealed record ParenthesizedExprNode(XPathAstNode Expression) : XPathAstNode;

// ------------------------------------------------------------------
// Predicates
// ------------------------------------------------------------------

public sealed record PredicateNode(XPathAstNode Expression) : XPathAstNode;

// ------------------------------------------------------------------
// Function calls
// ------------------------------------------------------------------

public sealed record FunctionCallNode(string LocalName, IReadOnlyList<XPathAstNode> Arguments, string? Prefix = null, string? NamespaceUri = null) : XPathAstNode;

/// <summary>Named function reference: <c>fn:abs#1</c></summary>
public sealed record NamedFunctionRefNode(string LocalName, int Arity, string? Prefix = null, string? NamespaceUri = null) : XPathAstNode;

// ------------------------------------------------------------------
// Sequence / Range
// ------------------------------------------------------------------

public sealed record SequenceExpressionNode(IReadOnlyList<XPathAstNode> Expressions) : XPathAstNode;
public sealed record RangeExpressionNode(XPathAstNode From, XPathAstNode To) : XPathAstNode;

// ------------------------------------------------------------------
// Conditional & FLWOR
// ------------------------------------------------------------------

public sealed record IfExpressionNode(XPathAstNode Condition, XPathAstNode ThenBranch, XPathAstNode ElseBranch) : XPathAstNode;
public sealed record ForExpressionNode(IReadOnlyList<QuantifiedBinding> Bindings, XPathAstNode ReturnExpression) : XPathAstNode;
public sealed record LetExpressionNode(IReadOnlyList<QuantifiedBinding> Bindings, XPathAstNode Body) : XPathAstNode;
public sealed record QuantifiedExpressionNode(QuantifierKind Quantifier, IReadOnlyList<QuantifiedBinding> Bindings, XPathAstNode SatisfiesExpression) : XPathAstNode;

/// <summary>Full XQuery FLWOR expression with clauses and return expression (replaces nested For/Let/Where for full XQuery FLWOR).</summary>
public sealed record FlworExpressionNode(IReadOnlyList<FlworClauseNode> Clauses, XPathAstNode ReturnExpression) : XPathAstNode;

/// <summary>Base type for a FLWOR clause.</summary>
public abstract record FlworClauseNode : XPathAstNode;

/// <summary>A for clause: <c>for $var in expr</c> (possibly with multiple bindings).</summary>
public sealed record ForClauseNode(IReadOnlyList<QuantifiedBinding> Bindings) : FlworClauseNode;

/// <summary>A let clause: <c>let $var := expr</c> (possibly with multiple bindings).</summary>
public sealed record LetClauseNode(IReadOnlyList<QuantifiedBinding> Bindings) : FlworClauseNode;

/// <summary>A where clause: <c>where expr</c>.</summary>
public sealed record WhereClauseNode(XPathAstNode Condition) : FlworClauseNode;

/// <summary>A count clause: <c>count $var</c>.</summary>
public sealed record CountClauseNode(string VariableName, string? Prefix = null, string? NamespaceUri = null) : FlworClauseNode;

/// <summary>An order by clause: <c>order by key [ascending|descending] [empty least|greatest] [collation 'uri']</c>.</summary>
public sealed record OrderByClauseNode(IReadOnlyList<OrderSpec> Specs) : FlworClauseNode;

/// <summary>A single ordering specification inside an order by clause.</summary>
public sealed record OrderSpec(
    XPathAstNode KeyExpression,
    bool Descending = false,
    EmptyOrder EmptyOrder = EmptyOrder.Least,
    string? CollationUri = null);

/// <summary>How to order empty sequences in an order by clause.</summary>
public enum EmptyOrder
{
    Least,
    Greatest
}

/// <summary>A group by clause: <c>group by $var (:= expr)? (collation 'uri')?, ...</c>.</summary>
public sealed record GroupByClauseNode(IReadOnlyList<GroupingSpec> Specs) : FlworClauseNode;

/// <summary>A window clause: <c>for tumbling|sliding window $var in expr start ... when ... (only)? end ... when ...</c>.</summary>
public sealed record WindowClauseNode(
    bool Sliding,
    string VariableName,
    XPathAstNode InExpression,
    WindowCondition StartCondition,
    WindowCondition EndCondition,
    bool OnlyEnd = false,
    string? Prefix = null,
    string? NamespaceUri = null) : FlworClauseNode;

/// <summary>A start or end condition of a window clause: <c>($cur)? (at $pos)? (previous $p)? (next $n)? when expr</c>.</summary>
public sealed record WindowCondition(
    XPathAstNode WhenExpression,
    string? CurrentItemVariable = null,
    string? PositionalVariable = null,
    string? PreviousItemVariable = null,
    string? NextItemVariable = null);

/// <summary>A single grouping specification inside a group by clause.</summary>
public sealed record GroupingSpec(
    string VariableName,
    XPathAstNode? KeyExpression = null,
    string? CollationUri = null,
    string? Prefix = null,
    string? NamespaceUri = null);

public sealed record QuantifiedBinding(string VariableName, XPathAstNode Expression, string? PositionalVariableName = null, string? VariablePrefix = null, string? VariableNamespaceUri = null);

// ------------------------------------------------------------------
// Binary / Unary expressions
// ------------------------------------------------------------------

public sealed record BinaryExpressionNode(XPathAstNode Left, BinaryOperator Operator, XPathAstNode Right) : XPathAstNode;
public sealed record UnaryExpressionNode(UnaryOperator Operator, XPathAstNode Operand) : XPathAstNode;

// ------------------------------------------------------------------
// Type expressions
// ------------------------------------------------------------------

public sealed record CastNode(XPathAstNode Expression, string TypeName, string? Prefix = null, OccurrenceIndicator Occurrence = OccurrenceIndicator.One) : XPathAstNode;
public sealed record CastableNode(XPathAstNode Expression, string TypeName, string? Prefix = null, OccurrenceIndicator Occurrence = OccurrenceIndicator.One) : XPathAstNode;
public sealed record InstanceOfNode(XPathAstNode Expression, string TypeName, string? Prefix = null, OccurrenceIndicator Occurrence = OccurrenceIndicator.One) : XPathAstNode;
public sealed record TreatNode(XPathAstNode Expression, string TypeName, string? Prefix = null, OccurrenceIndicator Occurrence = OccurrenceIndicator.One) : XPathAstNode;

// ------------------------------------------------------------------
// XPath 3.1 additions
// ------------------------------------------------------------------

/// <summary>Arrow expression: <c>$x => upper-case()</c></summary>
public sealed record ArrowExprNode(XPathAstNode Source, XPathAstNode Target) : XPathAstNode;

/// <summary>Try/catch expression: <c>try { A } catch * { B }</c></summary>
public sealed record TryCatchNode(XPathAstNode TryExpression, XPathAstNode CatchExpression) : XPathAstNode;

/// <summary>Lookup (postfix): <c>$map?key</c> or <c>$array?1</c></summary>
public sealed record LookupNode(XPathAstNode Expression, XPathAstNode Key) : XPathAstNode;

/// <summary>Lookup wildcard (postfix): <c>$map?*</c> or <c>$array?*</c></summary>
public sealed record LookupWildcardNode(XPathAstNode Expression) : XPathAstNode;

/// <summary>Inline function: <c>function($x as xs:int) as xs:int { $x + 1 }</c></summary>
public sealed record InlineFunctionNode(IReadOnlyList<ParamNode> Parameters, XPathAstNode Body, string? ReturnType = null) : XPathAstNode;
public sealed record ParamNode(string Name, string? TypeName = null);

/// <summary>Map constructor: <c>map { "a": 1, "b": 2 }</c></summary>
public sealed record MapConstructorNode(IReadOnlyList<MapEntryNode> Entries) : XPathAstNode;
public sealed record MapEntryNode(XPathAstNode Key, XPathAstNode Value) : XPathAstNode;

/// <summary>Array constructor: <c>[1, 2, 3]</c> or <c>array { $seq }</c></summary>
public sealed record ArrayConstructorNode(IReadOnlyList<XPathAstNode> Items, bool IsSquare = true) : XPathAstNode;

// ------------------------------------------------------------------
// Node tests
// ------------------------------------------------------------------

public sealed record NodeTest(NameTestKind Kind, string? Name = null, string? NamespaceUri = null, string? KindTestArgument = null);

// ------------------------------------------------------------------
// Enums
// ------------------------------------------------------------------

public enum BinaryOperator
{
    Or, And,
    Eq, Ne, Lt, Le, Gt, Ge,
    Equal, NotEqual, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual,
    Is, Precedes, Follows,
    To,
    Plus, Minus, Multiply, Divide, Idiv, Mod,
    Union, Intersect, Except,
    StringConcat,   // ||
    SimpleMap,      // !
    Range,
    InstanceOf, TreatAs, CastableAs, CastAs,
    Arrow,
    Assignable  // XQuery update
}

public enum UnaryOperator
{
    Plus, Minus
}

public enum NameTestKind
{
    AnyName,        // *
    PrefixedName,   // prefix:local
    LocalName,      // local (in default element namespace)
    NamespaceAny,   // namespace:*
    QName,          // full name with URI resolved
    KindTest        // node(), text(), element(), etc.
}

public sealed record PostfixPredicateNode(XPathAstNode Expression, XPathAstNode Predicate) : XPathAstNode;
public sealed record DynamicFunctionCallNode(XPathAstNode Function, IReadOnlyList<XPathAstNode> Arguments) : XPathAstNode;
public sealed record ArgumentPlaceholderNode : XPathAstNode;

public enum QuantifierKind
{
    Some, Every
}
