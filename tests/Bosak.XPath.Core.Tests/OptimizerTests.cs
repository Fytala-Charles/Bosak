// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Source file for OptimizerTests in the Development project
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
using Bosak.XPath.Compiler.Optimizer;
using Bosak.XPath.Parser;
using Bosak.XPath.Parser.Ast;
using Xunit;

namespace Bosak.XPath.Core.Tests;

public class OptimizerTests
{
    private static XPathAstNode ParseAndOptimize(string xpath)
    {
        var ast = XPathParser.Parse(xpath);
        var optimizer = new XPathOptimizer();
        return optimizer.Optimize(ast);
    }

    private static T AssertOptimized<T>(string xpath) where T : XPathAstNode
    {
        var node = ParseAndOptimize(xpath);
        Assert.IsType<T>(node);
        return (T)node;
    }

    // ------------------------------------------------------------------
    // Constant folding: integers
    // ------------------------------------------------------------------

    [Fact]
    public void FoldInteger_Addition()
    {
        var node = AssertOptimized<IntegerLiteralNode>("1 + 2");
        Assert.Equal(3, node.Value);
    }

    [Fact]
    public void FoldInteger_Subtraction()
    {
        var node = AssertOptimized<IntegerLiteralNode>("10 - 3");
        Assert.Equal(7, node.Value);
    }

    [Fact]
    public void FoldInteger_Multiplication()
    {
        var node = AssertOptimized<IntegerLiteralNode>("4 * 5");
        Assert.Equal(20, node.Value);
    }

    [Fact]
    public void FoldInteger_Division()
    {
        var node = AssertOptimized<IntegerLiteralNode>("20 div 4");
        Assert.Equal(5, node.Value);
    }

    [Fact]
    public void FoldInteger_Idiv()
    {
        var node = AssertOptimized<IntegerLiteralNode>("7 idiv 2");
        Assert.Equal(3, node.Value);
    }

    [Fact]
    public void FoldInteger_Modulo()
    {
        var node = AssertOptimized<IntegerLiteralNode>("10 mod 3");
        Assert.Equal(1, node.Value);
    }

    // ------------------------------------------------------------------
    // Constant folding: comparisons
    // ------------------------------------------------------------------

    [Fact]
    public void FoldInteger_Equal_True()
    {
        var node = AssertOptimized<BooleanLiteralNode>("1 = 1");
        Assert.True(node.Value);
    }

    [Fact]
    public void FoldInteger_Equal_False()
    {
        var node = AssertOptimized<BooleanLiteralNode>("1 = 2");
        Assert.False(node.Value);
    }

    [Fact]
    public void FoldInteger_LessThan()
    {
        var node = AssertOptimized<BooleanLiteralNode>("1 < 2");
        Assert.True(node.Value);
    }

    [Fact]
    public void FoldInteger_GreaterThanOrEqual()
    {
        var node = AssertOptimized<BooleanLiteralNode>("5 >= 5");
        Assert.True(node.Value);
    }

    // ------------------------------------------------------------------
    // Constant folding: string concat
    // ------------------------------------------------------------------

    [Fact]
    public void FoldStringConcat()
    {
        var node = AssertOptimized<StringLiteralNode>("'hello' || ' ' || 'world'");
        Assert.Equal("hello world", node.Value);
    }

    // ------------------------------------------------------------------
    // Constant folding: doubles
    // ------------------------------------------------------------------

    [Fact]
    public void FoldDecimal_Addition()
    {
        var node = AssertOptimized<DecimalLiteralNode>("1.5 + 2.5");
        Assert.Equal(4.0m, node.Value);
    }

    // ------------------------------------------------------------------
    // Constant folding: decimals
    // ------------------------------------------------------------------

    [Fact]
    public void FoldDecimal_Addition_Large()
    {
        var node = AssertOptimized<DecimalLiteralNode>("1.1 + 2.2");
        Assert.Equal(3.3m, node.Value);
    }

    // ------------------------------------------------------------------
    // Boolean simplification
    // ------------------------------------------------------------------

    [Fact]
    public void SimplifyTrueAndX()
    {
        var node = AssertOptimized<VariableReferenceNode>("1 and $x");
        Assert.Equal("x", node.LocalName);
    }

    [Fact]
    public void SimplifyFalseAndX()
    {
        var node = AssertOptimized<BooleanLiteralNode>("0 and $x");
        Assert.False(node.Value);
    }

    [Fact]
    public void SimplifyTrueOrX()
    {
        var node = AssertOptimized<BooleanLiteralNode>("1 or $x");
        Assert.True(node.Value);
    }

    [Fact]
    public void SimplifyFalseOrX()
    {
        var node = AssertOptimized<VariableReferenceNode>("0 or $x");
        Assert.Equal("x", node.LocalName);
    }

    // ------------------------------------------------------------------
    // Unary simplification
    // ------------------------------------------------------------------

    [Fact]
    public void EliminateDoubleNegation()
    {
        var node = AssertOptimized<IntegerLiteralNode>("--5");
        Assert.Equal(5, node.Value);
    }

    [Fact]
    public void EliminateUnaryPlus()
    {
        var node = AssertOptimized<IntegerLiteralNode>("+42");
        Assert.Equal(42, node.Value);
    }

    [Fact]
    public void FoldUnaryMinus_Integer()
    {
        var node = AssertOptimized<IntegerLiteralNode>("-7");
        Assert.Equal(-7, node.Value);
    }

    [Fact]
    public void FoldUnaryMinus_Decimal()
    {
        var node = AssertOptimized<DecimalLiteralNode>("-3.14");
        Assert.Equal(-3.14m, node.Value);
    }

    // ------------------------------------------------------------------
    // Dead code elimination
    // ------------------------------------------------------------------

    [Fact]
    public void EliminateIfTrue()
    {
        var node = AssertOptimized<IntegerLiteralNode>("if (1) then 42 else 99");
        Assert.Equal(42, node.Value);
    }

    [Fact]
    public void EliminateIfFalse()
    {
        var node = AssertOptimized<IntegerLiteralNode>("if (0) then 42 else 99");
        Assert.Equal(99, node.Value);
    }

    [Fact]
    public void EliminateIfConstantComparison()
    {
        var node = AssertOptimized<IntegerLiteralNode>("if (1 = 1) then 1 else 0");
        Assert.Equal(1, node.Value);
    }

    // ------------------------------------------------------------------
    // Parentheses
    // ------------------------------------------------------------------

    [Fact]
    public void UnwrapParentheses()
    {
        var node = AssertOptimized<IntegerLiteralNode>("(42)");
        Assert.Equal(42, node.Value);
    }

    [Fact]
    public void UnwrapNestedParentheses()
    {
        var node = AssertOptimized<IntegerLiteralNode>("(((1 + 2)))");
        Assert.Equal(3, node.Value);
    }

    // ------------------------------------------------------------------
    // Range expansion
    // ------------------------------------------------------------------

    [Fact]
    public void ExpandSmallConstantRange()
    {
        var node = AssertOptimized<SequenceExpressionNode>("1 to 3");
        Assert.Equal(3, node.Expressions.Count);
        Assert.Equal(1, Assert.IsType<IntegerLiteralNode>(node.Expressions[0]).Value);
        Assert.Equal(2, Assert.IsType<IntegerLiteralNode>(node.Expressions[1]).Value);
        Assert.Equal(3, Assert.IsType<IntegerLiteralNode>(node.Expressions[2]).Value);
    }

    [Fact]
    public void DoNotExpandLargeRange()
    {
        // Range 1 to 100 should NOT be expanded (would blow up AST)
        var node = AssertOptimized<RangeExpressionNode>("1 to 100");
        Assert.IsType<IntegerLiteralNode>(node.From);
        Assert.IsType<IntegerLiteralNode>(node.To);
    }

    // ------------------------------------------------------------------
    // Multi-pass folding
    // ------------------------------------------------------------------

    [Fact]
    public void MultiPass_LeftAssociative()
    {
        var node = AssertOptimized<IntegerLiteralNode>("1 + 2 + 3");
        Assert.Equal(6, node.Value);
    }

    [Fact]
    public void MultiPass_MixedOperators()
    {
        var node = AssertOptimized<IntegerLiteralNode>("(1 + 2) * (4 - 1)");
        Assert.Equal(9, node.Value);
    }

    [Fact]
    public void MultiPass_BooleanChain()
    {
        var node = AssertOptimized<BooleanLiteralNode>("1 = 1 and 2 = 2 and 3 = 3");
        Assert.True(node.Value);
    }

    // ------------------------------------------------------------------
    // No-op on variables
    // ------------------------------------------------------------------

    [Fact]
    public void NoFold_VariableArithmetic()
    {
        var node = AssertOptimized<BinaryExpressionNode>("$x + 1");
        Assert.Equal(BinaryOperator.Plus, node.Operator);
    }

    [Fact]
    public void NoFold_VariableComparison()
    {
        var node = AssertOptimized<BinaryExpressionNode>("$x = 1");
        Assert.Equal(BinaryOperator.Equal, node.Operator);
    }

    // ------------------------------------------------------------------
    // Complex expression
    // ------------------------------------------------------------------

    [Fact]
    public void Complex_FoldAllConstants()
    {
        // (2 + 3) * (10 div 2) = 5 * 5 = 25
        var node = AssertOptimized<IntegerLiteralNode>("(2 + 3) * (10 div 2)");
        Assert.Equal(25, node.Value);
    }
}
