// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Unit tests for the XPath recursive-descent parser AST shapes.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
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
    public void Parse_IntegerLiteral() => AssertParse<IntegerLiteralNode>("42");

    [Fact]
    public void Parse_DecimalLiteral() => AssertParse<DecimalLiteralNode>("3.14");

    [Fact]
    public void Parse_DoubleLiteral() => AssertParse<DoubleLiteralNode>("1e3");

    [Fact]
    public void Parse_StringLiteral_SingleQuote() => AssertParse<StringLiteralNode>("'hello'");

    [Fact]
    public void Parse_StringLiteral_DoubleQuote() => AssertParse<StringLiteralNode>("\"hello\"");

    // ------------------------------------------------------------------
    // Variables and context
    // ------------------------------------------------------------------

    [Fact]
    public void Parse_VariableReference()
    {
        var node = AssertParse<VariableReferenceNode>("$name");
        Assert.Equal("name", node.LocalName);
    }

    [Fact]
    public void Parse_ContextItem() => AssertParse<ContextItemNode>(".");

    [Fact]
    public void Parse_ParentNode() => AssertParse<StepNode>("..");

    // ------------------------------------------------------------------
    // Binary expressions
    // ------------------------------------------------------------------

    [Fact]
    public void Parse_Addition()
    {
        var node = AssertParse<BinaryExpressionNode>("1 + 2");
        Assert.Equal(BinaryOperator.Plus, node.Operator);
    }

    [Fact]
    public void Parse_Subtraction()
    {
        var node = AssertParse<BinaryExpressionNode>("1 - 2");
        Assert.Equal(BinaryOperator.Minus, node.Operator);
    }

    [Fact]
    public void Parse_Multiplication()
    {
        var node = AssertParse<BinaryExpressionNode>("2 * 3");
        Assert.Equal(BinaryOperator.Multiply, node.Operator);
    }

    [Fact]
    public void Parse_Division()
    {
        var node = AssertParse<BinaryExpressionNode>("4 div 2");
        Assert.Equal(BinaryOperator.Divide, node.Operator);
    }

    [Fact]
    public void Parse_IntegerDivision()
    {
        var node = AssertParse<BinaryExpressionNode>("5 idiv 2");
        Assert.Equal(BinaryOperator.Idiv, node.Operator);
    }

    [Fact]
    public void Parse_Modulo()
    {
        var node = AssertParse<BinaryExpressionNode>("5 mod 2");
        Assert.Equal(BinaryOperator.Mod, node.Operator);
    }

    [Fact]
    public void Parse_StringConcat()
    {
        var node = AssertParse<BinaryExpressionNode>("'a' || 'b'");
        Assert.Equal(BinaryOperator.StringConcat, node.Operator);
    }

    [Fact]
    public void Parse_Range()
    {
        var node = AssertParse<RangeExpressionNode>("1 to 5");
        Assert.NotNull(node.From);
        Assert.NotNull(node.To);
    }

    [Fact]
    public void Parse_Union()
    {
        var node = AssertParse<BinaryExpressionNode>("//a | //b");
        Assert.Equal(BinaryOperator.Union, node.Operator);
    }

    [Fact]
    public void Parse_Intersect()
    {
        var node = AssertParse<BinaryExpressionNode>("//a intersect //b");
        Assert.Equal(BinaryOperator.Intersect, node.Operator);
    }

    [Fact]
    public void Parse_Except()
    {
        var node = AssertParse<BinaryExpressionNode>("//a except //b");
        Assert.Equal(BinaryOperator.Except, node.Operator);
    }

    [Fact]
    public void Parse_SimpleMap()
    {
        var node = AssertParse<BinaryExpressionNode>("(1, 2, 3) ! fn:string(.)");
        Assert.Equal(BinaryOperator.SimpleMap, node.Operator);
    }

    // ------------------------------------------------------------------
    // Comparisons
    // ------------------------------------------------------------------

    [Fact]
    public void Parse_GeneralEqual()
    {
        var node = AssertParse<BinaryExpressionNode>("1 = 1");
        Assert.Equal(BinaryOperator.Equal, node.Operator);
    }

    [Fact]
    public void Parse_ValueEqual()
    {
        var node = AssertParse<BinaryExpressionNode>("1 eq 1");
        Assert.Equal(BinaryOperator.Eq, node.Operator);
    }

    [Fact]
    public void Parse_NodeIs()
    {
        var node = AssertParse<BinaryExpressionNode>(". is .");
        Assert.Equal(BinaryOperator.Is, node.Operator);
    }

    [Fact]
    public void Parse_NodePrecedes()
    {
        var node = AssertParse<BinaryExpressionNode>(". << .");
        Assert.Equal(BinaryOperator.Precedes, node.Operator);
    }

    [Fact]
    public void Parse_NodeFollows()
    {
        var node = AssertParse<BinaryExpressionNode>(". >> .");
        Assert.Equal(BinaryOperator.Follows, node.Operator);
    }

    // ------------------------------------------------------------------
    // Path expressions
    // ------------------------------------------------------------------

    [Fact]
    public void Parse_AbsolutePath() => AssertParse<PathExprNode>("/a/b");

    [Fact]
    public void Parse_RelativePath() => AssertParse<PathExprNode>("a/b/c");

    [Fact]
    public void Parse_DescendantOrSelf() => AssertParse<PathExprNode>("//a");

    [Fact]
    public void Parse_StepWithPredicate()
    {
        var node = AssertParse<StepNode>("a[1]");
        Assert.Single(node.Predicates);
    }

    [Fact]
    public void Parse_AxisStep()
    {
        var node = AssertParse<StepNode>("child::a");
        Assert.Equal(XdmAxis.Child, node.Axis);
    }

    [Fact]
    public void Parse_AttributeAxis()
    {
        var node = AssertParse<StepNode>("@attr");
        Assert.Equal(XdmAxis.Attribute, node.Axis);
    }

    // ------------------------------------------------------------------
    // Function calls
    // ------------------------------------------------------------------

    [Fact]
    public void Parse_FunctionCall()
    {
        var node = AssertParse<FunctionCallNode>("fn:string(42)");
        Assert.Equal("string", node.LocalName);
        Assert.Equal("fn", node.Prefix);
        Assert.Single(node.Arguments);
    }

    [Fact]
    public void Parse_FunctionCall_NoArgs()
    {
        var node = AssertParse<FunctionCallNode>("position()");
        Assert.Equal("position", node.LocalName);
        Assert.Empty(node.Arguments);
    }

    // ------------------------------------------------------------------
    // Maps and Arrays
    // ------------------------------------------------------------------

    [Fact]
    public void Parse_MapConstructor()
    {
        var node = AssertParse<MapConstructorNode>("map { 'a': 1, 'b': 2 }");
        Assert.Equal(2, node.Entries.Count);
    }

    [Fact]
    public void Parse_SquareArrayConstructor()
    {
        var node = AssertParse<ArrayConstructorNode>("[1, 2, 3]");
        Assert.Equal(3, node.Items.Count);
        Assert.True(node.IsSquare);
    }

    [Fact]
    public void Parse_CurlyArrayConstructor()
    {
        var node = AssertParse<ArrayConstructorNode>("array { 1, 2, 3 }");
        Assert.Single(node.Items);
        Assert.False(node.IsSquare);
    }

    [Fact]
    public void Parse_Lookup()
    {
        var node = AssertParse<LookupNode>("$m?key");
        Assert.IsType<VariableReferenceNode>(node.Expression);
    }

    [Fact]
    public void Parse_LookupWildcard()
    {
        var node = AssertParse<LookupWildcardNode>("$m?*");
        Assert.IsType<VariableReferenceNode>(node.Expression);
    }

    [Fact]
    public void Parse_Lookup_IntegerKey()
    {
        var node = AssertParse<LookupNode>("$a?2");
        Assert.IsType<VariableReferenceNode>(node.Expression);
    }

    // ------------------------------------------------------------------
    // FLWOR
    // ------------------------------------------------------------------

    [Fact]
    public void Parse_ForExpression()
    {
        var node = AssertParse<ForExpressionNode>("for $x in (1,2,3) return $x * 2");
        Assert.Single(node.Bindings);
        Assert.NotNull(node.ReturnExpression);
    }

    [Fact]
    public void Parse_SomeExpression()
    {
        var node = AssertParse<QuantifiedExpressionNode>("some $x in (1,2,3) satisfies $x > 1");
        Assert.Equal(QuantifierKind.Some, node.Quantifier);
    }

    [Fact]
    public void Parse_EveryExpression()
    {
        var node = AssertParse<QuantifiedExpressionNode>("every $x in (1,2,3) satisfies $x > 0");
        Assert.Equal(QuantifierKind.Every, node.Quantifier);
    }

    // ------------------------------------------------------------------
    // If expressions
    // ------------------------------------------------------------------

    [Fact]
    public void Parse_IfExpression()
    {
        var node = AssertParse<IfExpressionNode>("if (1 = 1) then 2 else 3");
        Assert.NotNull(node.Condition);
        Assert.NotNull(node.ThenBranch);
        Assert.NotNull(node.ElseBranch);
    }

    // ------------------------------------------------------------------
    // Type expressions
    // ------------------------------------------------------------------

    [Fact]
    public void Parse_InstanceOf() => AssertParse<InstanceOfNode>("1 instance of xs:integer");

    [Fact]
    public void Parse_Cast() => AssertParse<CastNode>("1 cast as xs:string");

    [Fact]
    public void Parse_Castable() => AssertParse<CastableNode>("1 castable as xs:integer");

    [Fact]
    public void Parse_Treat() => AssertParse<TreatNode>("1 treat as xs:integer");

    // ------------------------------------------------------------------
    // Arrow expressions
    // ------------------------------------------------------------------

    [Fact]
    public void Parse_ArrowExpression()
    {
        var node = AssertParse<ArrowExprNode>("'hello' => upper-case()");
        Assert.NotNull(node.Source);
        Assert.NotNull(node.Target);
    }

    // ------------------------------------------------------------------
    // Error cases
    // ------------------------------------------------------------------

    [Fact]
    public void ParseError_UnexpectedToken()
    {
        Assert.Throws<ParseException>(() => XPathParser.Parse("1 + ) 2"));
    }

    [Fact]
    public void ParseError_MissingClosingParen()
    {
        Assert.Throws<ParseException>(() => XPathParser.Parse("fn:string(42"));
    }

    [Fact]
    public void ParseError_EmptyInput()
    {
        Assert.Throws<ParseException>(() => XPathParser.Parse(""));
    }
}
