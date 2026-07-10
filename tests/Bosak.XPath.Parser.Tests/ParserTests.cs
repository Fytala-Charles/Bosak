// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Source file for ParserTests in the Development project
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 26-06-2026     | Added URI-qualified wildcard node-test parsing tests                                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser;
using Bosak.XPath.Parser.Ast;
using Xunit;

namespace Bosak.XPath.Parser.Tests;

public class ParserTests
{
    private static T AssertParse<T>(string xpath) where T : XPathAstNode
    {
        var node = XPathParser.Parse(xpath);
        Assert.IsType<T>(node);
        return (T)node;
    }

    // ------------------------------------------------------------------
    // Literals
    // ------------------------------------------------------------------

    [Fact]
    public void IntegerLiteral()
    {
        var node = AssertParse<IntegerLiteralNode>("42");
        Assert.Equal(42, node.Value);
    }

    [Fact]
    public void DecimalLiteral()
    {
        var node = AssertParse<DecimalLiteralNode>("3.14");
        Assert.Equal(3.14m, node.Value);
    }

    [Fact]
    public void DoubleLiteral()
    {
        var node = AssertParse<DoubleLiteralNode>("1.5e10");
        Assert.Equal(1.5e10, node.Value);
    }

    [Fact]
    public void StringLiteral_DoubleQuotes()
    {
        var node = AssertParse<StringLiteralNode>("\"hello\"");
        Assert.Equal("hello", node.Value);
    }

    [Fact]
    public void StringLiteral_SingleQuotes()
    {
        var node = AssertParse<StringLiteralNode>("'world'");
        Assert.Equal("world", node.Value);
    }

    // ------------------------------------------------------------------
    // Variables & Context
    // ------------------------------------------------------------------

    [Fact]
    public void VariableReference()
    {
        var node = AssertParse<VariableReferenceNode>("$x");
        Assert.Equal("x", node.LocalName);
        Assert.Null(node.Prefix);
    }

    [Fact]
    public void VariableReference_QName()
    {
        var node = AssertParse<VariableReferenceNode>("$ns:var");
        Assert.Equal("var", node.LocalName);
        Assert.Equal("ns", node.Prefix);
    }

    [Theory]
    [InlineData("mod")]
    [InlineData("div")]
    [InlineData("and")]
    [InlineData("or")]
    [InlineData("union")]
    [InlineData("intersect")]
    [InlineData("except")]
    [InlineData("to")]
    [InlineData("instance")]
    [InlineData("treat")]
    [InlineData("castable")]
    [InlineData("cast")]
    [InlineData("if")]
    [InlineData("then")]
    [InlineData("else")]
    [InlineData("for")]
    [InlineData("let")]
    [InlineData("in")]
    [InlineData("return")]
    [InlineData("some")]
    [InlineData("every")]
    [InlineData("satisfies")]
    [InlineData("function")]
    [InlineData("map")]
    [InlineData("array")]
    [InlineData("try")]
    [InlineData("catch")]
    [InlineData("eq")]
    [InlineData("ne")]
    [InlineData("lt")]
    [InlineData("le")]
    [InlineData("gt")]
    [InlineData("ge")]
    [InlineData("is")]
    public void VariableReference_KeywordName(string keyword)
    {
        var node = AssertParse<VariableReferenceNode>($"${keyword}");
        Assert.Equal(keyword, node.LocalName);
        Assert.Null(node.Prefix);
    }

    [Fact]
    public void ContextItem()
    {
        AssertParse<ContextItemNode>(".");
    }

    [Fact]
    public void ParenthesizedExpr()
    {
        var node = AssertParse<ParenthesizedExprNode>("(42)");
        Assert.IsType<IntegerLiteralNode>(node.Expression);
    }

    // ------------------------------------------------------------------
    // Path expressions
    // ------------------------------------------------------------------

    [Fact]
    public void AbsolutePath_RootOnly()
    {
        var node = AssertParse<PathExprNode>("/");
        Assert.True(node.IsAbsolute);
        Assert.Empty(node.Steps);
    }

    [Fact]
    public void AbsolutePath_SingleStep()
    {
        var node = AssertParse<PathExprNode>("/foo");
        Assert.True(node.IsAbsolute);
        Assert.Single(node.Steps);
        var step = Assert.IsType<StepNode>(node.Steps[0]);
        Assert.Equal(XdmAxis.Child, step.Axis);
    }

    [Fact]
    public void RelativePath_TwoSteps()
    {
        var node = AssertParse<PathExprNode>("foo/bar");
        Assert.False(node.IsAbsolute);
        Assert.Equal(2, node.Steps.Count);
    }

    [Fact]
    public void DescendantOrSelf_DoubleSlash()
    {
        var node = AssertParse<PathExprNode>("//foo");
        Assert.True(node.IsAbsolute);
        Assert.Equal(2, node.Steps.Count);
        var first = Assert.IsType<StepNode>(node.Steps[0]);
        Assert.Equal(XdmAxis.DescendantOrSelf, first.Axis);
    }

    [Fact]
    public void AbbreviatedParent()
    {
        var step = AssertParse<StepNode>("..");
        Assert.Equal(XdmAxis.Parent, step.Axis);
    }

    [Fact]
    public void AbbreviatedAttribute()
    {
        var step = AssertParse<StepNode>("@href");
        Assert.Equal(XdmAxis.Attribute, step.Axis);
    }

    [Fact]
    public void ExplicitAxis()
    {
        var step = AssertParse<StepNode>("child::foo");
        Assert.Equal(XdmAxis.Child, step.Axis);
    }

    [Fact]
    public void AxisStep_WithPredicate()
    {
        var step = AssertParse<StepNode>("foo[1]");
        Assert.Single(step.Predicates);
    }

    [Fact]
    public void Predicate_OnParent()
    {
        var step = AssertParse<StepNode>("..[1]");
        Assert.Single(step.Predicates);
    }

    [Fact]
    public void KindTest_Attribute_DefaultAxis()
    {
        // XPath 2.0 §3.2.1.1: attribute() defaults to attribute axis
        var step = AssertParse<StepNode>("attribute()");
        Assert.Equal(XdmAxis.Attribute, step.Axis);
        Assert.Equal(NameTestKind.KindTest, step.NodeTest.Kind);
        Assert.Equal("attribute", step.NodeTest.Name);
    }

    [Fact]
    public void KindTest_NamespaceNode_DefaultAxis()
    {
        // XPath 2.0 §3.2.1.1: namespace-node() defaults to namespace axis
        var step = AssertParse<StepNode>("namespace-node()");
        Assert.Equal(XdmAxis.Namespace, step.Axis);
        Assert.Equal(NameTestKind.KindTest, step.NodeTest.Kind);
        Assert.Equal("namespace-node", step.NodeTest.Name);
    }

    [Fact]
    public void KindTest_Text_DefaultAxisIsChild()
    {
        // text() still defaults to child axis
        var step = AssertParse<StepNode>("text()");
        Assert.Equal(XdmAxis.Child, step.Axis);
        Assert.Equal(NameTestKind.KindTest, step.NodeTest.Kind);
        Assert.Equal("text", step.NodeTest.Name);
    }

    [Fact]
    public void KindTest_Attribute_ExplicitAxis()
    {
        // Explicit child::attribute() keeps child axis
        var step = AssertParse<StepNode>("child::attribute()");
        Assert.Equal(XdmAxis.Child, step.Axis);
    }

    // ------------------------------------------------------------------
    // Function calls
    // ------------------------------------------------------------------

    [Fact]
    public void FunctionCall_NoArgs()
    {
        var node = AssertParse<FunctionCallNode>("true()");
        Assert.Equal("true", node.LocalName);
        Assert.Empty(node.Arguments);
    }

    [Fact]
    public void FunctionCall_WithArgs()
    {
        var node = AssertParse<FunctionCallNode>("concat('a', 'b')");
        Assert.Equal("concat", node.LocalName);
        Assert.Equal(2, node.Arguments.Count);
    }

    [Fact]
    public void FunctionCall_QName()
    {
        var node = AssertParse<FunctionCallNode>("fn:string(1)");
        Assert.Equal("string", node.LocalName);
        Assert.Equal("fn", node.Prefix);
    }

    [Fact]
    public void NamedFunctionRef()
    {
        var node = AssertParse<NamedFunctionRefNode>("abs#1");
        Assert.Equal("abs", node.LocalName);
        Assert.Equal(1, node.Arity);
    }

    // ------------------------------------------------------------------
    // Binary operators
    // ------------------------------------------------------------------

    [Fact]
    public void AdditiveExpr()
    {
        var node = AssertParse<BinaryExpressionNode>("1 + 2");
        Assert.Equal(BinaryOperator.Plus, node.Operator);
    }

    [Fact]
    public void MultiplicativeExpr()
    {
        var node = AssertParse<BinaryExpressionNode>("3 * 4");
        Assert.Equal(BinaryOperator.Multiply, node.Operator);
    }

    [Fact]
    public void ComparisonExpr_General()
    {
        var node = AssertParse<BinaryExpressionNode>("1 = 2");
        Assert.Equal(BinaryOperator.Equal, node.Operator);
    }

    [Fact]
    public void ComparisonExpr_Value()
    {
        var node = AssertParse<BinaryExpressionNode>("1 eq 2");
        Assert.Equal(BinaryOperator.Eq, node.Operator);
    }

    [Fact]
    public void AndExpr()
    {
        var node = AssertParse<BinaryExpressionNode>("true() and false()");
        Assert.Equal(BinaryOperator.And, node.Operator);
    }

    [Fact]
    public void OrExpr()
    {
        var node = AssertParse<BinaryExpressionNode>("true() or false()");
        Assert.Equal(BinaryOperator.Or, node.Operator);
    }

    [Fact]
    public void UnionExpr()
    {
        var node = AssertParse<BinaryExpressionNode>("$a | $b");
        Assert.Equal(BinaryOperator.Union, node.Operator);
    }

    [Fact]
    public void IntersectExpr()
    {
        var node = AssertParse<BinaryExpressionNode>("$a intersect $b");
        Assert.Equal(BinaryOperator.Intersect, node.Operator);
    }

    [Fact]
    public void RangeExpr()
    {
        var node = AssertParse<RangeExpressionNode>("1 to 10");
        Assert.IsType<IntegerLiteralNode>(node.From);
        Assert.IsType<IntegerLiteralNode>(node.To);
    }

    [Fact]
    public void StringConcatExpr()
    {
        var node = AssertParse<BinaryExpressionNode>("'a' || 'b'");
        Assert.Equal(BinaryOperator.StringConcat, node.Operator);
    }

    [Fact]
    public void ArrowExpr()
    {
        var node = AssertParse<ArrowExprNode>("$x => upper-case()");
        Assert.IsType<VariableReferenceNode>(node.Source);
        Assert.IsType<FunctionCallNode>(node.Target);
    }

    // ------------------------------------------------------------------
    // Unary operators
    // ------------------------------------------------------------------

    [Fact]
    public void UnaryMinus()
    {
        var node = AssertParse<UnaryExpressionNode>("-5");
        Assert.Equal(UnaryOperator.Minus, node.Operator);
    }

    [Fact]
    public void UnaryPlus()
    {
        var node = AssertParse<UnaryExpressionNode>("+3");
        Assert.Equal(UnaryOperator.Plus, node.Operator);
    }

    // ------------------------------------------------------------------
    // Sequence
    // ------------------------------------------------------------------

    [Fact]
    public void SequenceExpr()
    {
        var node = AssertParse<SequenceExpressionNode>("1, 2, 3");
        Assert.Equal(3, node.Expressions.Count);
    }

    // ------------------------------------------------------------------
    // Conditional
    // ------------------------------------------------------------------

    [Fact]
    public void IfExpr()
    {
        var node = AssertParse<IfExpressionNode>("if ($x) then 1 else 0");
        Assert.IsType<VariableReferenceNode>(node.Condition);
        Assert.IsType<IntegerLiteralNode>(node.ThenBranch);
        Assert.IsType<IntegerLiteralNode>(node.ElseBranch);
    }

    // ------------------------------------------------------------------
    // FLWOR
    // ------------------------------------------------------------------

    [Fact]
    public void ForExpr()
    {
        var node = AssertParse<ForExpressionNode>("for $i in 1 to 10 return $i");
        Assert.Single(node.Bindings);
        Assert.IsType<RangeExpressionNode>(node.Bindings[0].Expression);
    }

    [Fact]
    public void QuantifiedExpr_Some()
    {
        var node = AssertParse<QuantifiedExpressionNode>("some $x in (1,2,3) satisfies $x gt 0");
        Assert.Equal(QuantifierKind.Some, node.Quantifier);
    }

    [Fact]
    public void QuantifiedExpr_Every()
    {
        var node = AssertParse<QuantifiedExpressionNode>("every $x in (1,2,3) satisfies $x gt 0");
        Assert.Equal(QuantifierKind.Every, node.Quantifier);
    }

    // ------------------------------------------------------------------
    // Type expressions
    // ------------------------------------------------------------------

    [Fact]
    public void CastExpr()
    {
        var node = AssertParse<CastNode>("$x cast as xs:string");
        Assert.IsType<VariableReferenceNode>(node.Expression);
        Assert.Equal("string", node.TypeName);
        Assert.Equal("xs", node.Prefix);
    }

    [Fact]
    public void InstanceOfExpr()
    {
        var node = AssertParse<InstanceOfNode>("$x instance of xs:integer");
        Assert.IsType<VariableReferenceNode>(node.Expression);
    }

    // ------------------------------------------------------------------
    // XPath 3.1 constructors
    // ------------------------------------------------------------------

    [Fact]
    public void MapConstructor()
    {
        var node = AssertParse<MapConstructorNode>("map { 'a': 1, 'b': 2 }");
        Assert.Equal(2, node.Entries.Count);
    }

    [Fact]
    public void SquareArrayConstructor()
    {
        var node = AssertParse<ArrayConstructorNode>("[1, 2, 3]");
        Assert.Equal(3, node.Items.Count);
        Assert.True(node.IsSquare);
    }

    [Fact]
    public void CurlyArrayConstructor()
    {
        var node = AssertParse<ArrayConstructorNode>("array { $seq }");
        Assert.Single(node.Items);
        Assert.False(node.IsSquare);
    }

    // ------------------------------------------------------------------
    // Complex expressions
    // ------------------------------------------------------------------

    [Fact]
    public void ComplexPath_WithPredicates()
    {
        var node = AssertParse<PathExprNode>("//book[price gt 10]/title");
        Assert.True(node.IsAbsolute);
        Assert.Equal(3, node.Steps.Count);
    }

    [Fact]
    public void ComplexArithmetic_WithPath()
    {
        var node = AssertParse<BinaryExpressionNode>("count(//item) + 1");
        Assert.Equal(BinaryOperator.Plus, node.Operator);
    }

    [Fact]
    public void ComplexConditional_InPath()
    {
        var node = AssertParse<PostfixPredicateNode>("$x[if (@y) then @y else 'default']");
        Assert.IsType<VariableReferenceNode>(node.Expression);
    }

    [Fact]
    public void UriQualifiedNameWildcard_EmptyUri()
    {
        var step = AssertParse<StepNode>("@Q{}*");
        Assert.Equal(XdmAxis.Attribute, step.Axis);
        Assert.Equal(NameTestKind.NamespaceAny, step.NodeTest.Kind);
        Assert.Equal("", step.NodeTest.NamespaceUri);
    }

    [Fact]
    public void UriQualifiedNameWildcard_NonEmptyUri()
    {
        var step = AssertParse<StepNode>("Q{http://example.com}*");
        Assert.Equal(XdmAxis.Child, step.Axis);
        Assert.Equal(NameTestKind.NamespaceAny, step.NodeTest.Kind);
        Assert.Equal("http://example.com", step.NodeTest.NamespaceUri);
    }
}
