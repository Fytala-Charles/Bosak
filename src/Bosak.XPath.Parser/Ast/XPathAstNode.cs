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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
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
public sealed record NamedFunctionRefNode(string LocalName, int Arity, string? Prefix = null) : XPathAstNode;

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

public sealed record QuantifiedBinding(string VariableName, XPathAstNode Expression);

// ------------------------------------------------------------------
// Binary / Unary expressions
// ------------------------------------------------------------------

public sealed record BinaryExpressionNode(XPathAstNode Left, BinaryOperator Operator, XPathAstNode Right) : XPathAstNode;
public sealed record UnaryExpressionNode(UnaryOperator Operator, XPathAstNode Operand) : XPathAstNode;

// ------------------------------------------------------------------
// Type expressions
// ------------------------------------------------------------------

public sealed record CastNode(XPathAstNode Expression, string TypeName, string? Prefix = null) : XPathAstNode;
public sealed record CastableNode(XPathAstNode Expression, string TypeName, string? Prefix = null) : XPathAstNode;
public sealed record InstanceOfNode(XPathAstNode Expression, string TypeName, string? Prefix = null) : XPathAstNode;
public sealed record TreatNode(XPathAstNode Expression, string TypeName, string? Prefix = null) : XPathAstNode;

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

public sealed record NodeTest(NameTestKind Kind, string? Name = null, string? NamespaceUri = null);

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
