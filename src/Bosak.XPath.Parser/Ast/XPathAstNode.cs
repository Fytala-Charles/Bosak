// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Base type for all nodes in the XPath Abstract Syntax Tree. The AST is immutable and produced by t...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
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
//                      | Charles Korthout | 1.13  | 22-08-2026     | Added ValidateExpressionNode for XQuery validate expressions |
//                      | Charles Korthout | 1.14  | 23-08-2026     | ValidateExpressionNode carries optional TypeName/TypePrefix for validate type QName |
//                      | Charles Korthout | 1.15  | 09-09-2026     | DecimalLiteralNode gains IsIntegerLiteral flag                                           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.15  | 07-09-2026     | NodeTest carries KindTestInnerName for document-node(element|schema-element(...))        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.16  | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
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
/// <param name="Start">The zero-based character offset of the region in the source text.</param>
/// <param name="Length">The length of the region in characters.</param>
public readonly record struct TextSpan(int Start, int Length)
{
    /// <summary>The zero-based character offset just past the end of the region.</summary>
    public int End => Start + Length;
}

// ------------------------------------------------------------------
// Literals & Variables
// ------------------------------------------------------------------

/// <summary>A boolean literal value, produced by folding the <c>true()</c>/<c>false()</c> functions.</summary>
/// <param name="Value">The boolean value.</param>
public sealed record BooleanLiteralNode(bool Value) : XPathAstNode;
/// <summary>An integer literal: <c>42</c>.</summary>
/// <param name="Value">The integer value.</param>
public sealed record IntegerLiteralNode(long Value) : XPathAstNode;
/// <summary>A decimal literal: <c>3.14</c>.</summary>
/// <param name="Value">The decimal value.</param>
/// <param name="IsIntegerLiteral">True when the source literal was an integer that overflowed
/// <see cref="long"/> and is stored as a decimal; the value keeps xs:integer typing.</param>
public sealed record DecimalLiteralNode(decimal Value, bool IsIntegerLiteral = false) : XPathAstNode;
/// <summary>A double literal: <c>1e3</c>.</summary>
/// <param name="Value">The double value.</param>
public sealed record DoubleLiteralNode(double Value) : XPathAstNode;
/// <summary>A string literal: <c>'abc'</c>.</summary>
/// <param name="Value">The literal content with the delimiting quotes removed and escapes resolved.</param>
public sealed record StringLiteralNode(string Value) : XPathAstNode;

/// <summary>A variable reference: <c>$name</c>, <c>$prefix:name</c>, or <c>$Q{uri}name</c>.</summary>
/// <param name="LocalName">The local name of the variable.</param>
/// <param name="Prefix">The namespace prefix of the variable, or null.</param>
/// <param name="NamespaceUri">The namespace URI of an EQName variable, or null.</param>
public sealed record VariableReferenceNode(string LocalName, string? Prefix = null, string? NamespaceUri = null) : XPathAstNode;

// ------------------------------------------------------------------
// Path & Steps
// ------------------------------------------------------------------

/// <summary>A single step in a path expression.</summary>
/// <param name="Axis">The axis the step navigates.</param>
/// <param name="NodeTest">The node test applied to the axis.</param>
/// <param name="Predicates">The predicate expressions filtering the step's result.</param>
public sealed record StepNode(XdmAxis Axis, NodeTest NodeTest, IReadOnlyList<XPathAstNode> Predicates) : XPathAstNode;

/// <summary>A path expression (relative or absolute).</summary>
/// <param name="IsAbsolute">True when the path starts with <c>/</c> or <c>//</c>.</param>
/// <param name="Steps">The steps composing the path.</param>
public sealed record PathExprNode(bool IsAbsolute, IReadOnlyList<XPathAstNode> Steps) : XPathAstNode;

/// <summary>The context item expression: <c>.</c></summary>
public sealed record ContextItemNode : XPathAstNode;

/// <summary>Parenthesized expression: <c>(expr)</c></summary>
/// <param name="Expression">The parenthesized expression.</param>
public sealed record ParenthesizedExprNode(XPathAstNode Expression) : XPathAstNode;

// ------------------------------------------------------------------
// Predicates
// ------------------------------------------------------------------

/// <summary>A predicate in square brackets: <c>[expr]</c>.</summary>
/// <param name="Expression">The predicate expression.</param>
public sealed record PredicateNode(XPathAstNode Expression) : XPathAstNode;

// ------------------------------------------------------------------
// Function calls
// ------------------------------------------------------------------

/// <summary>A static function call: <c>fn:count($x)</c>.</summary>
/// <param name="LocalName">The local name of the function.</param>
/// <param name="Arguments">The argument expressions.</param>
/// <param name="Prefix">The namespace prefix of the function name, or null.</param>
/// <param name="NamespaceUri">The namespace URI of an EQName function name, or null.</param>
public sealed record FunctionCallNode(string LocalName, IReadOnlyList<XPathAstNode> Arguments, string? Prefix = null, string? NamespaceUri = null) : XPathAstNode;

/// <summary>Named function reference: <c>fn:abs#1</c></summary>
/// <param name="LocalName">The local name of the function.</param>
/// <param name="Arity">The function arity (the integer after <c>#</c>).</param>
/// <param name="Prefix">The namespace prefix of the function name, or null.</param>
/// <param name="NamespaceUri">The namespace URI of an EQName function name, or null.</param>
public sealed record NamedFunctionRefNode(string LocalName, int Arity, string? Prefix = null, string? NamespaceUri = null) : XPathAstNode;

// ------------------------------------------------------------------
// Sequence / Range
// ------------------------------------------------------------------

/// <summary>A sequence (comma) expression: <c>1, 2, 3</c>.</summary>
/// <param name="Expressions">The sub-expressions whose results are concatenated.</param>
public sealed record SequenceExpressionNode(IReadOnlyList<XPathAstNode> Expressions) : XPathAstNode;
/// <summary>A range expression: <c>1 to 10</c>.</summary>
/// <param name="From">The lower bound expression.</param>
/// <param name="To">The upper bound expression.</param>
public sealed record RangeExpressionNode(XPathAstNode From, XPathAstNode To) : XPathAstNode;

// ------------------------------------------------------------------
// Conditional & FLWOR
// ------------------------------------------------------------------

/// <summary>A conditional expression: <c>if (C) then A else B</c>.</summary>
/// <param name="Condition">The condition expression.</param>
/// <param name="ThenBranch">The expression evaluated when the condition is true.</param>
/// <param name="ElseBranch">The expression evaluated when the condition is false.</param>
public sealed record IfExpressionNode(XPathAstNode Condition, XPathAstNode ThenBranch, XPathAstNode ElseBranch) : XPathAstNode;
/// <summary>A simple for expression: <c>for $x in E return R</c>.</summary>
/// <param name="Bindings">The variable bindings of the for clause.</param>
/// <param name="ReturnExpression">The expression evaluated once per binding tuple.</param>
public sealed record ForExpressionNode(IReadOnlyList<QuantifiedBinding> Bindings, XPathAstNode ReturnExpression) : XPathAstNode;
/// <summary>A simple let expression: <c>let $x := E return R</c>.</summary>
/// <param name="Bindings">The variable bindings of the let clause.</param>
/// <param name="Body">The expression evaluated with the bindings in scope.</param>
public sealed record LetExpressionNode(IReadOnlyList<QuantifiedBinding> Bindings, XPathAstNode Body) : XPathAstNode;
/// <summary>A quantified expression: <c>some|every $x in E satisfies C</c>.</summary>
/// <param name="Quantifier">Whether this is an existential (<c>some</c>) or universal (<c>every</c>) quantification.</param>
/// <param name="Bindings">The variable bindings of the quantified expression.</param>
/// <param name="SatisfiesExpression">The test evaluated once per binding tuple.</param>
public sealed record QuantifiedExpressionNode(QuantifierKind Quantifier, IReadOnlyList<QuantifiedBinding> Bindings, XPathAstNode SatisfiesExpression) : XPathAstNode;

/// <summary>An XQuery switch expression: <c>switch (E) (case V)+ return R ... default return RD</c>.</summary>
/// <param name="Operand">The operand expression compared against the case values.</param>
/// <param name="Cases">The case clauses, tried in order; the first match wins.</param>
/// <param name="Default">The result expression when no case matches.</param>
public sealed record SwitchExpressionNode(
    XPathAstNode Operand,
    IReadOnlyList<SwitchCaseClause> Cases,
    XPathAstNode Default) : XPathAstNode;

/// <summary>One case clause of a switch expression: operand values compared with <c>eq</c> semantics; first match wins.</summary>
/// <param name="Values">The case operand values compared against the switch operand.</param>
/// <param name="Return">The result expression of this case.</param>
public sealed record SwitchCaseClause(IReadOnlyList<XPathAstNode> Values, XPathAstNode Return);

/// <summary>An XQuery typeswitch expression: <c>typeswitch (E) (case ($v as)? T return R)+ default ($d)? return RD</c>.</summary>
/// <param name="Operand">The operand expression whose type is matched.</param>
/// <param name="Cases">The case clauses, tried in order; the first type match wins.</param>
/// <param name="Default">The result expression when no case matches.</param>
/// <param name="DefaultVariableName">The local name of the variable bound to the operand in the default clause, or null.</param>
/// <param name="DefaultVariablePrefix">The namespace prefix of the default variable, or null.</param>
/// <param name="DefaultVariableNamespaceUri">The namespace URI of an EQName default variable, or null.</param>
public sealed record TypeswitchExpressionNode(
    XPathAstNode Operand,
    IReadOnlyList<TypeswitchCaseClause> Cases,
    XPathAstNode Default,
    string? DefaultVariableName = null,
    string? DefaultVariablePrefix = null,
    string? DefaultVariableNamespaceUri = null) : XPathAstNode;

/// <summary>One case clause of a typeswitch expression: an optional bound variable, the sequence-type union to match, and the return expression.</summary>
/// <param name="Types">The sequence-type union matched by this case.</param>
/// <param name="Return">The result expression of this case.</param>
/// <param name="VariableName">The local name of the variable bound to the operand when this case matches, or null.</param>
/// <param name="VariablePrefix">The namespace prefix of the case variable, or null.</param>
/// <param name="VariableNamespaceUri">The namespace URI of an EQName case variable, or null.</param>
public sealed record TypeswitchCaseClause(
    IReadOnlyList<TypeswitchCaseType> Types,
    XPathAstNode Return,
    string? VariableName = null,
    string? VariablePrefix = null,
    string? VariableNamespaceUri = null);

/// <summary>One member type of a typeswitch case sequence-type union (<c>xs:integer | xs:string</c>).</summary>
/// <param name="Prefix">The namespace prefix of the type name, or null.</param>
/// <param name="Local">The local name of the type or kind test.</param>
/// <param name="Occurrence">The occurrence indicator of this member type.</param>
public sealed record TypeswitchCaseType(string? Prefix, string Local, OccurrenceIndicator Occurrence);

/// <summary>Full XQuery FLWOR expression with clauses and return expression (replaces nested For/Let/Where for full XQuery FLWOR).</summary>
/// <param name="Clauses">The FLWOR clauses in source order.</param>
/// <param name="ReturnExpression">The return expression.</param>
public sealed record FlworExpressionNode(IReadOnlyList<FlworClauseNode> Clauses, XPathAstNode ReturnExpression) : XPathAstNode;

/// <summary>Base type for a FLWOR clause.</summary>
public abstract record FlworClauseNode : XPathAstNode;

/// <summary>A for clause: <c>for $var in expr</c> (possibly with multiple bindings).</summary>
/// <param name="Bindings">The variable bindings of the clause.</param>
public sealed record ForClauseNode(IReadOnlyList<QuantifiedBinding> Bindings) : FlworClauseNode;

/// <summary>A let clause: <c>let $var := expr</c> (possibly with multiple bindings).</summary>
/// <param name="Bindings">The variable bindings of the clause.</param>
public sealed record LetClauseNode(IReadOnlyList<QuantifiedBinding> Bindings) : FlworClauseNode;

/// <summary>A where clause: <c>where expr</c>.</summary>
/// <param name="Condition">The boolean filter expression.</param>
public sealed record WhereClauseNode(XPathAstNode Condition) : FlworClauseNode;

/// <summary>A count clause: <c>count $var</c>.</summary>
/// <param name="VariableName">The local name of the counter variable.</param>
/// <param name="Prefix">The namespace prefix of the counter variable, or null.</param>
/// <param name="NamespaceUri">The namespace URI of an EQName counter variable, or null.</param>
public sealed record CountClauseNode(string VariableName, string? Prefix = null, string? NamespaceUri = null) : FlworClauseNode;

/// <summary>An order by clause: <c>order by key [ascending|descending] [empty least|greatest] [collation 'uri']</c>.</summary>
/// <param name="Specs">The ordering specifications of the clause.</param>
public sealed record OrderByClauseNode(IReadOnlyList<OrderSpec> Specs) : FlworClauseNode;

/// <summary>A single ordering specification inside an order by clause. When
/// <see cref="EmptyOrder"/> is null the static context's default order for empty
/// sequences applies (itself defaulting to <see cref="EmptyOrder.Least"/>).</summary>
/// <param name="KeyExpression">The sort key expression.</param>
/// <param name="Descending">True for a descending sort.</param>
/// <param name="EmptyOrder">How empty sequences sort, or null to use the static context default.</param>
/// <param name="CollationUri">The collation URI of a <c>collation</c> modifier, or null.</param>
public sealed record OrderSpec(
    XPathAstNode KeyExpression,
    bool Descending = false,
    EmptyOrder? EmptyOrder = null,
    string? CollationUri = null);

/// <summary>How to order empty sequences in an order by clause.</summary>
public enum EmptyOrder
{
    /// <summary>Empty sequences sort before all other values.</summary>
    Least,
    /// <summary>Empty sequences sort after all other values.</summary>
    Greatest
}

/// <summary>A group by clause: <c>group by $var (:= expr)? (collation 'uri')?, ...</c>.</summary>
/// <param name="Specs">The grouping specifications of the clause.</param>
public sealed record GroupByClauseNode(IReadOnlyList<GroupingSpec> Specs) : FlworClauseNode;

/// <summary>A window clause: <c>for tumbling|sliding window $var (as SequenceType)? in expr start ... when ... ((only)? end ... when ...)?</c>.</summary>
/// <param name="Sliding">True for a sliding window, false for a tumbling window.</param>
/// <param name="VariableName">The local name of the window variable.</param>
/// <param name="InExpression">The expression producing the windowed sequence.</param>
/// <param name="StartCondition">The window start condition.</param>
/// <param name="EndCondition">The window end condition, or null when the clause has no end clause.</param>
/// <param name="OnlyEnd">True when the end condition uses <c>only end</c>.</param>
/// <param name="Prefix">The namespace prefix of the window variable, or null.</param>
/// <param name="NamespaceUri">The namespace URI of an EQName window variable, or null.</param>
/// <param name="DeclaredType">The optional <c>as SequenceType</c> declaration of the window variable.</param>
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
/// <param name="WhenExpression">The boolean <c>when</c> expression.</param>
/// <param name="CurrentItemVariable">The variable bound to the current item, or null.</param>
/// <param name="PositionalVariable">The variable bound to the current position (<c>at $pos</c>), or null.</param>
/// <param name="PreviousItemVariable">The variable bound to the previous item, or null.</param>
/// <param name="NextItemVariable">The variable bound to the next item, or null.</param>
public sealed record WindowCondition(
    XPathAstNode WhenExpression,
    string? CurrentItemVariable = null,
    string? PositionalVariable = null,
    string? PreviousItemVariable = null,
    string? NextItemVariable = null);

/// <summary>A single grouping specification inside a group by clause.</summary>
/// <param name="VariableName">The local name of the grouping variable.</param>
/// <param name="KeyExpression">The grouping key expression, or null when the variable's existing binding is the key.</param>
/// <param name="CollationUri">The collation URI of a <c>collation</c> modifier, or null.</param>
/// <param name="Prefix">The namespace prefix of the grouping variable, or null.</param>
/// <param name="NamespaceUri">The namespace URI of an EQName grouping variable, or null.</param>
/// <param name="DeclaredType">The optional <c>as SequenceType</c> declaration of the grouping variable.</param>
public sealed record GroupingSpec(
    string VariableName,
    XPathAstNode? KeyExpression = null,
    string? CollationUri = null,
    string? Prefix = null,
    string? NamespaceUri = null,
    FlworTypeDeclaration? DeclaredType = null);

/// <summary>An optional type declaration on a FLWOR variable binding: <c>as SequenceType</c>.</summary>
/// <param name="TypeName">The local name of the declared type.</param>
/// <param name="Prefix">The namespace prefix of the declared type, or null.</param>
/// <param name="Occurrence">The occurrence indicator of the declared type.</param>
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
/// <param name="TagName">The local name of the element tag.</param>
/// <param name="Prefix">The namespace prefix of the element tag, or null.</param>
/// <param name="Attributes">The attribute constructors of the element.</param>
/// <param name="Content">The content parts: literal text, enclosed expressions, and nested elements.</param>
public sealed record DirectElementConstructorNode(
    string TagName,
    string? Prefix,
    IReadOnlyList<DirectAttributeNode> Attributes,
    IReadOnlyList<XPathAstNode> Content) : XPathAstNode;

/// <summary>A direct attribute constructor: <c>name="literal {expr} literal"</c>.</summary>
/// <param name="Name">The local name of the attribute.</param>
/// <param name="Prefix">The namespace prefix of the attribute, or null.</param>
/// <param name="ValueParts">The value parts: literal text runs and enclosed expressions.</param>
public sealed record DirectAttributeNode(
    string Name,
    string? Prefix,
    IReadOnlyList<XPathAstNode> ValueParts);

/// <summary>A comment constructor inside direct element content: <c>&lt;!-- ... --&gt;</c>.</summary>
/// <param name="Value">The comment text.</param>
public sealed record DirectCommentNode(string Value) : XPathAstNode;

/// <summary>A processing-instruction constructor inside direct element content: <c>&lt;?target data?&gt;</c>.</summary>
/// <param name="Target">The processing-instruction target.</param>
/// <param name="Value">The processing-instruction data.</param>
public sealed record DirectProcessingInstructionNode(string Target, string Value) : XPathAstNode;

/// <summary>Literal text in element content that contains a character/entity reference and is therefore never boundary whitespace.</summary>
/// <param name="Value">The literal text.</param>
public sealed record SignificantTextNode(string Value) : XPathAstNode;

// ------------------------------------------------------------------
// XQuery computed constructors
// ------------------------------------------------------------------

/// <summary>A computed element constructor: <c>element (QName | "{" Expr "}") "{" Expr "}"</c>.</summary>
/// <param name="NameExpression">The expression computing the element name, or null when the name is static.</param>
/// <param name="TagName">The static local name of the element, or null.</param>
/// <param name="TagPrefix">The static namespace prefix of the element, or null.</param>
/// <param name="TagNamespaceUri">The static namespace URI of the element (EQName), or null.</param>
/// <param name="ContentExpression">The expression producing the element content.</param>
public sealed record ComputedElementConstructorNode(
    XPathAstNode? NameExpression,
    string? TagName,
    string? TagPrefix,
    string? TagNamespaceUri,
    XPathAstNode ContentExpression) : XPathAstNode;

/// <summary>A computed attribute constructor: <c>attribute (QName | "{" Expr "}") "{" Expr "}"</c>.</summary>
/// <param name="NameExpression">The expression computing the attribute name, or null when the name is static.</param>
/// <param name="Name">The static local name of the attribute, or null.</param>
/// <param name="Prefix">The static namespace prefix of the attribute, or null.</param>
/// <param name="NamespaceUri">The static namespace URI of the attribute (EQName), or null.</param>
/// <param name="ValueExpression">The expression producing the attribute value.</param>
public sealed record ComputedAttributeConstructorNode(
    XPathAstNode? NameExpression,
    string? Name,
    string? Prefix,
    string? NamespaceUri,
    XPathAstNode ValueExpression) : XPathAstNode;

/// <summary>A computed document constructor: <c>document "{" Expr "}"</c>.</summary>
/// <param name="ContentExpression">The expression producing the document content.</param>
public sealed record ComputedDocumentConstructorNode(XPathAstNode ContentExpression) : XPathAstNode;

/// <summary>A computed text constructor: <c>text "{" Expr "}"</c>.</summary>
/// <param name="ValueExpression">The expression producing the text value.</param>
public sealed record ComputedTextConstructorNode(XPathAstNode ValueExpression) : XPathAstNode;

/// <summary>A computed comment constructor: <c>comment "{" Expr "}"</c>.</summary>
/// <param name="ValueExpression">The expression producing the comment text.</param>
public sealed record ComputedCommentConstructorNode(XPathAstNode ValueExpression) : XPathAstNode;

/// <summary>A computed processing-instruction constructor: <c>processing-instruction (NCName | "{" Expr "}") "{" Expr "}"</c>.</summary>
/// <param name="TargetExpression">The expression computing the target, or null when the target is static.</param>
/// <param name="Target">The static target NCName, or null.</param>
/// <param name="ValueExpression">The expression producing the processing-instruction data.</param>
public sealed record ComputedPIConstructorNode(
    XPathAstNode? TargetExpression,
    string? Target,
    XPathAstNode ValueExpression) : XPathAstNode;

/// <summary>A computed namespace constructor: <c>namespace (NCName | "{" Expr "}") "{" Expr "}"</c>.</summary>
/// <param name="PrefixExpression">The expression computing the namespace prefix, or null when the prefix is static.</param>
/// <param name="Prefix">The static prefix NCName, or null.</param>
/// <param name="UriExpression">The expression producing the namespace URI.</param>
public sealed record ComputedNamespaceConstructorNode(
    XPathAstNode? PrefixExpression,
    string? Prefix,
    XPathAstNode UriExpression) : XPathAstNode;

/// <summary>One variable binding of a for, let, some, or every clause:
/// <c>$var (as SequenceType)? (allowing empty)? (at $pos)? in|:= expr</c>.</summary>
/// <param name="VariableName">The local name of the bound variable.</param>
/// <param name="Expression">The expression whose value is bound to the variable.</param>
/// <param name="PositionalVariableName">The positional variable of a for binding (<c>at $pos</c>), or null.</param>
/// <param name="VariablePrefix">The namespace prefix of the bound variable, or null.</param>
/// <param name="VariableNamespaceUri">The namespace URI of an EQName bound variable, or null.</param>
/// <param name="DeclaredType">The optional <c>as SequenceType</c> declaration of the binding.</param>
/// <param name="AllowingEmpty">True when a for binding declares <c>allowing empty</c>.</param>
public sealed record QuantifiedBinding(string VariableName, XPathAstNode Expression, string? PositionalVariableName = null, string? VariablePrefix = null, string? VariableNamespaceUri = null, FlworTypeDeclaration? DeclaredType = null, bool AllowingEmpty = false);

// ------------------------------------------------------------------
// Binary / Unary expressions
// ------------------------------------------------------------------

/// <summary>A binary operator expression.</summary>
/// <param name="Left">The left operand.</param>
/// <param name="Operator">The binary operator.</param>
/// <param name="Right">The right operand.</param>
public sealed record BinaryExpressionNode(XPathAstNode Left, BinaryOperator Operator, XPathAstNode Right) : XPathAstNode;
/// <summary>A unary plus or minus expression.</summary>
/// <param name="Operator">The unary operator.</param>
/// <param name="Operand">The operand expression.</param>
public sealed record UnaryExpressionNode(UnaryOperator Operator, XPathAstNode Operand) : XPathAstNode;

// ------------------------------------------------------------------
// Type expressions
// ------------------------------------------------------------------

/// <summary>A cast expression: <c>E cast as T</c>.</summary>
/// <param name="Expression">The expression whose value is cast.</param>
/// <param name="TypeName">The local name of the target type.</param>
/// <param name="Prefix">The namespace prefix of the target type, or null.</param>
/// <param name="Occurrence">The occurrence indicator of the target type.</param>
public sealed record CastNode(XPathAstNode Expression, string TypeName, string? Prefix = null, OccurrenceIndicator Occurrence = OccurrenceIndicator.One) : XPathAstNode;
/// <summary>A castable expression: <c>E castable as T</c>.</summary>
/// <param name="Expression">The expression whose value is tested.</param>
/// <param name="TypeName">The local name of the target type.</param>
/// <param name="Prefix">The namespace prefix of the target type, or null.</param>
/// <param name="Occurrence">The occurrence indicator of the target type.</param>
public sealed record CastableNode(XPathAstNode Expression, string TypeName, string? Prefix = null, OccurrenceIndicator Occurrence = OccurrenceIndicator.One) : XPathAstNode;
/// <summary>An instance-of expression: <c>E instance of T</c>.</summary>
/// <param name="Expression">The expression whose value is tested.</param>
/// <param name="TypeName">The local name of the tested type.</param>
/// <param name="Prefix">The namespace prefix of the tested type, or null.</param>
/// <param name="Occurrence">The occurrence indicator of the tested type.</param>
public sealed record InstanceOfNode(XPathAstNode Expression, string TypeName, string? Prefix = null, OccurrenceIndicator Occurrence = OccurrenceIndicator.One) : XPathAstNode;
/// <summary>A treat-as expression: <c>E treat as T</c>.</summary>
/// <param name="Expression">The expression whose value is asserted.</param>
/// <param name="TypeName">The local name of the asserted type.</param>
/// <param name="Prefix">The namespace prefix of the asserted type, or null.</param>
/// <param name="Occurrence">The occurrence indicator of the asserted type.</param>
public sealed record TreatNode(XPathAstNode Expression, string TypeName, string? Prefix = null, OccurrenceIndicator Occurrence = OccurrenceIndicator.One) : XPathAstNode;

// ------------------------------------------------------------------
// XPath 3.1 additions
// ------------------------------------------------------------------

/// <summary>Arrow expression: <c>$x => upper-case()</c></summary>
/// <param name="Source">The input expression, passed as the first argument to the target.</param>
/// <param name="Target">The function or inline function applied to the source value.</param>
public sealed record ArrowExprNode(XPathAstNode Source, XPathAstNode Target) : XPathAstNode;

/// <summary>Try/catch expression: <c>try { A } catch CodePatternList { B } (catch CodePatternList { C })*</c></summary>
/// <param name="TryExpression">The guarded expression.</param>
/// <param name="Clauses">The catch clauses, tried in order; the first pattern match wins.</param>
public sealed record TryCatchNode(XPathAstNode TryExpression, IReadOnlyList<TryCatchClause> Clauses) : XPathAstNode;

/// <summary>XQuery validate expression: <c>validate { Expr }</c>, <c>validate strict|lax { Expr }</c>, or <c>validate type QName { Expr }</c>.</summary>
/// <param name="Expression">The expression whose result is validated.</param>
/// <param name="Mode">The validation mode (<c>strict</c> or <c>lax</c>), or null for the default mode.</param>
/// <param name="TypeName">The local name of the target type for <c>validate type QName</c>, or null.</param>
/// <param name="TypePrefix">The namespace prefix of the target type, or null.</param>
public sealed record ValidateExpressionNode(XPathAstNode Expression, string? Mode = null, string? TypeName = null, string? TypePrefix = null) : XPathAstNode;

/// <summary>One catch clause of a try/catch expression: <c>catch PatternList { Expr }</c>; first matching clause wins.</summary>
/// <param name="Patterns">The error-code name-test patterns selecting this clause.</param>
/// <param name="Expression">The handler expression evaluated when a pattern matches.</param>
public sealed record TryCatchClause(IReadOnlyList<CatchCodePattern> Patterns, XPathAstNode Expression);

/// <summary>
/// One error-code pattern of a catch clause (an XPath NameTest over error codes):
/// <c>*</c> matches everything; <c>prefix:local</c>/<c>prefix:*</c> resolve the prefix at
/// runtime; <c>*:local</c> matches any namespace (<see cref="Prefix"/> is "*");
/// <c>Q{uri}local</c>/<c>Q{uri}*</c> carry the namespace in <see cref="NamespaceUri"/>;
/// an unprefixed name matches the empty namespace (<see cref="NamespaceUri"/> is "").
/// A null <see cref="LocalName"/> is a namespace-local wildcard.
/// </summary>
/// <param name="Prefix">The namespace prefix, <c>*</c> for an any-namespace wildcard, or null.</param>
/// <param name="LocalName">The local name of the error code, or null for a namespace-local wildcard.</param>
/// <param name="NamespaceUri">The namespace URI of an EQName pattern, the empty string for an
/// unprefixed name, or null when the prefix is resolved at runtime.</param>
public sealed record CatchCodePattern(string? Prefix, string? LocalName, string? NamespaceUri);

/// <summary>Lookup (postfix): <c>$map?key</c> or <c>$array?1</c></summary>
/// <param name="Expression">The map or array expression.</param>
/// <param name="Key">The key or index expression.</param>
public sealed record LookupNode(XPathAstNode Expression, XPathAstNode Key) : XPathAstNode;

/// <summary>Lookup wildcard (postfix): <c>$map?*</c> or <c>$array?*</c></summary>
/// <param name="Expression">The map or array expression.</param>
public sealed record LookupWildcardNode(XPathAstNode Expression) : XPathAstNode;

/// <summary>Inline function: <c>function($x as xs:int) as xs:int { $x + 1 }</c></summary>
/// <param name="Parameters">The function parameters.</param>
/// <param name="Body">The function body.</param>
/// <param name="ReturnType">The declared return type (<c>as T</c>), or null.</param>
public sealed record InlineFunctionNode(IReadOnlyList<ParamNode> Parameters, XPathAstNode Body, string? ReturnType = null) : XPathAstNode;
/// <summary>One parameter of an inline function.</summary>
/// <param name="Name">The parameter name.</param>
/// <param name="TypeName">The declared parameter type (<c>as T</c>), or null.</param>
public sealed record ParamNode(string Name, string? TypeName = null);

/// <summary>Map constructor: <c>map { "a": 1, "b": 2 }</c></summary>
/// <param name="Entries">The entries of the map.</param>
public sealed record MapConstructorNode(IReadOnlyList<MapEntryNode> Entries) : XPathAstNode;
/// <summary>One key/value pair of a map constructor.</summary>
/// <param name="Key">The key expression.</param>
/// <param name="Value">The value expression.</param>
public sealed record MapEntryNode(XPathAstNode Key, XPathAstNode Value) : XPathAstNode;

/// <summary>
/// An XQuery string constructor: <c>``[literal `{expr}` literal]``</c>. Parts are literal
/// text runs (<see cref="StringLiteralNode"/>) and interpolation expressions; the result is
/// their concatenation, each interpolation's atomized items joined with single spaces.
/// </summary>
/// <param name="Parts">The literal text runs and interpolation expressions.</param>
public sealed record StringConstructorNode(IReadOnlyList<XPathAstNode> Parts) : XPathAstNode;

/// <summary>Array constructor: <c>[1, 2, 3]</c> or <c>array { $seq }</c></summary>
/// <param name="Items">The member expressions (square constructor) or the single sequence
/// expression whose items become members (curly constructor).</param>
/// <param name="IsSquare">True for the square <c>[...]</c> constructor, false for <c>array { }</c>.</param>
public sealed record ArrayConstructorNode(IReadOnlyList<XPathAstNode> Items, bool IsSquare = true) : XPathAstNode;

// ------------------------------------------------------------------
// Node tests
// ------------------------------------------------------------------

/// <summary>
/// A node test: wildcard, name test, or kind test.
/// <paramref name="NamespaceUri"/> carries a prefix for prefixed name tests (resolved at
/// compile/evaluation time), the wildcard marker <c>"*"</c>, or an explicit namespace URI
/// for EQName and <c>xml</c> forms. For <c>document-node()</c> kind tests,
/// <paramref name="KindTestInnerName"/> records the inner test ("element" or
/// "schema-element") and <paramref name="KindTestArgument"/> its name argument.
/// </summary>
/// <param name="Kind">The kind of node test.</param>
/// <param name="Name">The local name or kind-test name, or null for the <c>*</c> wildcard.</param>
/// <param name="NamespaceUri">A prefix to resolve, the wildcard marker <c>"*"</c>, or an
/// explicit namespace URI (EQName/<c>xml</c> forms); null when not applicable.</param>
/// <param name="KindTestArgument">The name argument of a kind test (e.g. <c>element(name)</c>), or null.</param>
/// <param name="KindTestTypeName">The schema type name of a kind test (e.g. <c>element(name, type)</c>), or null.</param>
/// <param name="KindTestInnerName">The inner test name of a <c>document-node()</c> kind test
/// (<c>element</c> or <c>schema-element</c>), or null.</param>
public sealed record NodeTest(
    NameTestKind Kind,
    string? Name = null,
    string? NamespaceUri = null,
    string? KindTestArgument = null,
    string? KindTestTypeName = null,
    string? KindTestInnerName = null);

// ------------------------------------------------------------------
// Enums
// ------------------------------------------------------------------

/// <summary>The binary operators of XPath 3.1 and XQuery 3.1.</summary>
public enum BinaryOperator
{
    /// <summary>The <c>or</c> boolean operator.</summary>
    Or,
    /// <summary>The <c>and</c> boolean operator.</summary>
    And,
    /// <summary>The <c>eq</c> value comparison.</summary>
    Eq,
    /// <summary>The <c>ne</c> value comparison.</summary>
    Ne,
    /// <summary>The <c>lt</c> value comparison.</summary>
    Lt,
    /// <summary>The <c>le</c> value comparison.</summary>
    Le,
    /// <summary>The <c>gt</c> value comparison.</summary>
    Gt,
    /// <summary>The <c>ge</c> value comparison.</summary>
    Ge,
    /// <summary>The <c>=</c> general comparison.</summary>
    Equal,
    /// <summary>The <c>!=</c> general comparison.</summary>
    NotEqual,
    /// <summary>The <c>&lt;</c> general comparison.</summary>
    LessThan,
    /// <summary>The <c>&lt;=</c> general comparison.</summary>
    LessThanOrEqual,
    /// <summary>The <c>&gt;</c> general comparison.</summary>
    GreaterThan,
    /// <summary>The <c>&gt;=</c> general comparison.</summary>
    GreaterThanOrEqual,
    /// <summary>The <c>is</c> node identity comparison.</summary>
    Is,
    /// <summary>The <c>&lt;&lt;</c> node-before comparison.</summary>
    Precedes,
    /// <summary>The <c>&gt;&gt;</c> node-after comparison.</summary>
    Follows,
    /// <summary>The <c>to</c> range operator.</summary>
    To,
    /// <summary>The <c>+</c> arithmetic operator.</summary>
    Plus,
    /// <summary>The <c>-</c> arithmetic operator.</summary>
    Minus,
    /// <summary>The <c>*</c> arithmetic operator.</summary>
    Multiply,
    /// <summary>The <c>div</c> arithmetic operator.</summary>
    Divide,
    /// <summary>The <c>idiv</c> integer division operator.</summary>
    Idiv,
    /// <summary>The <c>mod</c> arithmetic operator.</summary>
    Mod,
    /// <summary>The <c>union</c> (or <c>|</c>) node set operator.</summary>
    Union,
    /// <summary>The <c>intersect</c> node set operator.</summary>
    Intersect,
    /// <summary>The <c>except</c> node set operator.</summary>
    Except,
    /// <summary>The <c>||</c> string concatenation operator.</summary>
    StringConcat,
    /// <summary>The <c>!</c> simple map operator.</summary>
    SimpleMap,
    /// <summary>The range operator (lowered from <see cref="To"/>).</summary>
    Range,
    /// <summary>The <c>instance of</c> type test.</summary>
    InstanceOf,
    /// <summary>The <c>treat as</c> type assertion.</summary>
    TreatAs,
    /// <summary>The <c>castable as</c> type test.</summary>
    CastableAs,
    /// <summary>The <c>cast as</c> type conversion.</summary>
    CastAs,
    /// <summary>The <c>=&gt;</c> arrow operator.</summary>
    Arrow,
    /// <summary>XQuery update assignability test.</summary>
    Assignable
}

/// <summary>The unary operators of XPath 3.1.</summary>
public enum UnaryOperator
{
    /// <summary>Unary plus: <c>+E</c>.</summary>
    Plus,
    /// <summary>Unary minus: <c>-E</c>.</summary>
    Minus
}

/// <summary>The kind of a <see cref="NodeTest"/>: wildcard, name test, or kind test.</summary>
public enum NameTestKind
{
    /// <summary>The <c>*</c> wildcard, matching any node of the principal node kind.</summary>
    AnyName,
    /// <summary>A prefixed name test: <c>prefix:local</c>; the prefix is resolved at compile/evaluation time.</summary>
    PrefixedName,
    /// <summary>An unprefixed name test, resolved against the default element namespace.</summary>
    LocalName,
    /// <summary>A namespace wildcard: <c>prefix:*</c> or <c>Q{uri}*</c>.</summary>
    NamespaceAny,
    /// <summary>A full name with the namespace URI already resolved (EQName or <c>xml</c> prefix forms).</summary>
    QName,
    /// <summary>A kind test: <c>node()</c>, <c>text()</c>, <c>element()</c>, etc.</summary>
    KindTest
}

/// <summary>A predicate applied to a postfix expression: <c>E[P]</c>.</summary>
/// <param name="Expression">The expression being filtered.</param>
/// <param name="Predicate">The predicate expression.</param>
public sealed record PostfixPredicateNode(XPathAstNode Expression, XPathAstNode Predicate) : XPathAstNode;
/// <summary>A call through a function item: <c>$f(1, 2)</c>.</summary>
/// <param name="Function">The expression producing the function item.</param>
/// <param name="Arguments">The argument expressions.</param>
public sealed record DynamicFunctionCallNode(XPathAstNode Function, IReadOnlyList<XPathAstNode> Arguments) : XPathAstNode;
/// <summary>A partial-application argument placeholder: <c>?</c>.</summary>
public sealed record ArgumentPlaceholderNode : XPathAstNode;

/// <summary>The quantifier of a quantified expression.</summary>
public enum QuantifierKind
{
    /// <summary>Existential quantification: <c>some</c>.</summary>
    Some,
    /// <summary>Universal quantification: <c>every</c>.</summary>
    Every
}
