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
//                      | Charles Korthout | 1.4   | 25-07-2026     | Optional window end condition; FlworTypeDeclaration for 'as SequenceType' bindings      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.5   | 25-07-2026     | Added direct constructor AST nodes (element/attribute/comment/PI)                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.6   | 25-07-2026     | Added SignificantTextNode; AllowingEmpty on QuantifiedBinding                           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.7   | 25-07-2026     | Added computed constructor AST nodes (element/attribute/document/text/comment/PI/ns)    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.8   | 25-07-2026     | Added SwitchExpressionNode/TypeswitchExpressionNode AST for XQuery switch and typeswitch|
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.9   | 27-07-2026     | TryCatchNode holds multiple catch clauses with error-code name-test patterns |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.10  | 27-07-2026     | StringConstructorNode for XQuery string constructors |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.11  | 27-07-2026     | OrderSpec.EmptyOrder nullable (unspecified uses the prolog default) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.12  | 28-07-2026     | NodeTest.KindTestTypeName for schema type names in kind tests |
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

/// <summary>An XQuery switch expression: <c>switch (E) (case V)+ return R ... default return RD</c>.</summary>
public sealed record SwitchExpressionNode(
    XPathAstNode Operand,
    IReadOnlyList<SwitchCaseClause> Cases,
    XPathAstNode Default) : XPathAstNode;

/// <summary>One case clause of a switch expression: operand values compared with <c>eq</c> semantics; first match wins.</summary>
public sealed record SwitchCaseClause(IReadOnlyList<XPathAstNode> Values, XPathAstNode Return);

/// <summary>An XQuery typeswitch expression: <c>typeswitch (E) (case ($v as)? T return R)+ default ($d)? return RD</c>.</summary>
public sealed record TypeswitchExpressionNode(
    XPathAstNode Operand,
    IReadOnlyList<TypeswitchCaseClause> Cases,
    XPathAstNode Default,
    string? DefaultVariableName = null,
    string? DefaultVariablePrefix = null,
    string? DefaultVariableNamespaceUri = null) : XPathAstNode;

/// <summary>One case clause of a typeswitch expression: an optional bound variable, the sequence-type union to match, and the return expression.</summary>
public sealed record TypeswitchCaseClause(
    IReadOnlyList<TypeswitchCaseType> Types,
    XPathAstNode Return,
    string? VariableName = null,
    string? VariablePrefix = null,
    string? VariableNamespaceUri = null);

/// <summary>One member type of a typeswitch case sequence-type union (<c>xs:integer | xs:string</c>).</summary>
public sealed record TypeswitchCaseType(string? Prefix, string Local, OccurrenceIndicator Occurrence);

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

/// <summary>A single ordering specification inside an order by clause. When
/// <see cref="EmptyOrder"/> is null the static context's default order for empty
/// sequences applies (itself defaulting to <see cref="EmptyOrder.Least"/>).</summary>
public sealed record OrderSpec(
    XPathAstNode KeyExpression,
    bool Descending = false,
    EmptyOrder? EmptyOrder = null,
    string? CollationUri = null);

/// <summary>How to order empty sequences in an order by clause.</summary>
public enum EmptyOrder
{
    Least,
    Greatest
}

/// <summary>A group by clause: <c>group by $var (:= expr)? (collation 'uri')?, ...</c>.</summary>
public sealed record GroupByClauseNode(IReadOnlyList<GroupingSpec> Specs) : FlworClauseNode;

/// <summary>A window clause: <c>for tumbling|sliding window $var (as SequenceType)? in expr start ... when ... ((only)? end ... when ...)?</c>.</summary>
public sealed record WindowClauseNode(
    bool Sliding,
    string VariableName,
    XPathAstNode InExpression,
    WindowCondition StartCondition,
    WindowCondition? EndCondition,
    bool OnlyEnd = false,
    string? Prefix = null,
    string? NamespaceUri = null,
    FlworTypeDeclaration? DeclaredType = null) : FlworClauseNode;

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
    string? NamespaceUri = null,
    FlworTypeDeclaration? DeclaredType = null);

/// <summary>An optional type declaration on a FLWOR variable binding: <c>as SequenceType</c>.</summary>
public sealed record FlworTypeDeclaration(string TypeName, string? Prefix, OccurrenceIndicator Occurrence);

// ------------------------------------------------------------------
// XQuery direct constructors
// ------------------------------------------------------------------

/// <summary>
/// A direct element constructor: <c>&lt;name a="v"&gt;content {expr}&lt;/name&gt;</c>.
/// Content and attribute values are lists of parts: <see cref="StringLiteralNode"/> for
/// literal text, expression nodes for enclosed expressions, and nested
/// <see cref="DirectElementConstructorNode"/> for nested elements.
/// </summary>
public sealed record DirectElementConstructorNode(
    string TagName,
    string? Prefix,
    IReadOnlyList<DirectAttributeNode> Attributes,
    IReadOnlyList<XPathAstNode> Content) : XPathAstNode;

/// <summary>A direct attribute constructor: <c>name="literal {expr} literal"</c>.</summary>
public sealed record DirectAttributeNode(
    string Name,
    string? Prefix,
    IReadOnlyList<XPathAstNode> ValueParts);

/// <summary>A comment constructor inside direct element content: <c>&lt;!-- ... --&gt;</c>.</summary>
public sealed record DirectCommentNode(string Value) : XPathAstNode;

/// <summary>A processing-instruction constructor inside direct element content: <c>&lt;?target data?&gt;</c>.</summary>
public sealed record DirectProcessingInstructionNode(string Target, string Value) : XPathAstNode;

/// <summary>Literal text in element content that contains a character/entity reference and is therefore never boundary whitespace.</summary>
public sealed record SignificantTextNode(string Value) : XPathAstNode;

// ------------------------------------------------------------------
// XQuery computed constructors
// ------------------------------------------------------------------

/// <summary>A computed element constructor: <c>element (QName | "{" Expr "}") "{" Expr "}"</c>.</summary>
public sealed record ComputedElementConstructorNode(
    XPathAstNode? NameExpression,
    string? TagName,
    string? TagPrefix,
    string? TagNamespaceUri,
    XPathAstNode ContentExpression) : XPathAstNode;

/// <summary>A computed attribute constructor: <c>attribute (QName | "{" Expr "}") "{" Expr "}"</c>.</summary>
public sealed record ComputedAttributeConstructorNode(
    XPathAstNode? NameExpression,
    string? Name,
    string? Prefix,
    string? NamespaceUri,
    XPathAstNode ValueExpression) : XPathAstNode;

/// <summary>A computed document constructor: <c>document "{" Expr "}"</c>.</summary>
public sealed record ComputedDocumentConstructorNode(XPathAstNode ContentExpression) : XPathAstNode;

/// <summary>A computed text constructor: <c>text "{" Expr "}"</c>.</summary>
public sealed record ComputedTextConstructorNode(XPathAstNode ValueExpression) : XPathAstNode;

/// <summary>A computed comment constructor: <c>comment "{" Expr "}"</c>.</summary>
public sealed record ComputedCommentConstructorNode(XPathAstNode ValueExpression) : XPathAstNode;

/// <summary>A computed processing-instruction constructor: <c>processing-instruction (NCName | "{" Expr "}") "{" Expr "}"</c>.</summary>
public sealed record ComputedPIConstructorNode(
    XPathAstNode? TargetExpression,
    string? Target,
    XPathAstNode ValueExpression) : XPathAstNode;

/// <summary>A computed namespace constructor: <c>namespace (NCName | "{" Expr "}") "{" Expr "}"</c>.</summary>
public sealed record ComputedNamespaceConstructorNode(
    XPathAstNode? PrefixExpression,
    string? Prefix,
    XPathAstNode UriExpression) : XPathAstNode;

public sealed record QuantifiedBinding(string VariableName, XPathAstNode Expression, string? PositionalVariableName = null, string? VariablePrefix = null, string? VariableNamespaceUri = null, FlworTypeDeclaration? DeclaredType = null, bool AllowingEmpty = false);

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

/// <summary>Try/catch expression: <c>try { A } catch CodePatternList { B } (catch CodePatternList { C })*</c></summary>
public sealed record TryCatchNode(XPathAstNode TryExpression, IReadOnlyList<TryCatchClause> Clauses) : XPathAstNode;

/// <summary>One catch clause of a try/catch expression: <c>catch PatternList { Expr }</c>; first matching clause wins.</summary>
public sealed record TryCatchClause(IReadOnlyList<CatchCodePattern> Patterns, XPathAstNode Expression);

/// <summary>
/// One error-code pattern of a catch clause (an XPath NameTest over error codes):
/// <c>*</c> matches everything; <c>prefix:local</c>/<c>prefix:*</c> resolve the prefix at
/// runtime; <c>*:local</c> matches any namespace (<see cref="Prefix"/> is "*");
/// <c>Q{uri}local</c>/<c>Q{uri}*</c> carry the namespace in <see cref="NamespaceUri"/>;
/// an unprefixed name matches the empty namespace (<see cref="NamespaceUri"/> is "").
/// A null <see cref="LocalName"/> is a namespace-local wildcard.
/// </summary>
public sealed record CatchCodePattern(string? Prefix, string? LocalName, string? NamespaceUri);

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

/// <summary>
/// An XQuery string constructor: <c>``[literal `{expr}` literal]``</c>. Parts are literal
/// text runs (<see cref="StringLiteralNode"/>) and interpolation expressions; the result is
/// their concatenation, each interpolation's atomized items joined with single spaces.
/// </summary>
public sealed record StringConstructorNode(IReadOnlyList<XPathAstNode> Parts) : XPathAstNode;

/// <summary>Array constructor: <c>[1, 2, 3]</c> or <c>array { $seq }</c></summary>
public sealed record ArrayConstructorNode(IReadOnlyList<XPathAstNode> Items, bool IsSquare = true) : XPathAstNode;

// ------------------------------------------------------------------
// Node tests
// ------------------------------------------------------------------

public sealed record NodeTest(NameTestKind Kind, string? Name = null, string? NamespaceUri = null, string? KindTestArgument = null, string? KindTestTypeName = null);

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
