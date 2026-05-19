// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Source file for VmEngineTests in the Development project
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
using Bosak.XPath.Api;
using Bosak.XPath.Compiler.Ir;
using Bosak.XPath.Compiler.Optimizer;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Parser;
using Bosak.XPath.Parser.Ast;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

public class VmEngineTests
{
    private static XdmValue Evaluate(string xpath, EvaluationContext? context = null)
    {
        var ast = XPathParser.Parse(xpath);
        var optimizer = new XPathOptimizer();
        var optimized = optimizer.Optimize(ast);
        var lowerer = new IrLowerer();
        var module = lowerer.Lower(optimized);

        context ??= new EvaluationContext();
        FunctionLibrary.Populate(context);
        return VmEngine.Execute(module, context);
    }

    // ------------------------------------------------------------------
    // Literals
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_IntegerLiteral()
    {
        var result = Evaluate("42");
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(42, result.IntegerValue);
    }

    [Fact]
    public void Eval_DecimalLiteral()
    {
        var result = Evaluate("3.14");
        Assert.Equal(XdmValueKind.Decimal, result.Kind);
        Assert.Equal(3.14m, result.DecimalValue);
    }

    [Fact]
    public void Eval_DoubleLiteral()
    {
        var result = Evaluate("1e3");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.Equal(1000.0, result.DoubleValue);
    }

    [Fact]
    public void Eval_StringLiteral()
    {
        var result = Evaluate("'hello'");
        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("hello", result.StringValue);
    }

    [Fact]
    public void Eval_BooleanLiteral_True()
    {
        var result = Evaluate("true()");
        Assert.Equal(XdmValueKind.Boolean, result.Kind);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_BooleanLiteral_False()
    {
        var result = Evaluate("false()");
        Assert.Equal(XdmValueKind.Boolean, result.Kind);
        Assert.False(result.BooleanValue);
    }

    // ------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_VariableReference()
    {
        var ctx = new EvaluationContext()
            .WithVariable("x", XdmValue.FromInteger(99));
        FunctionLibrary.Populate(ctx);

        var result = Evaluate("$x", ctx);
        Assert.Equal(99, result.IntegerValue);
    }

    [Fact]
    public void Eval_VariableReference_QName()
    {
        var ctx = new EvaluationContext()
            .WithNamespace("ns", "http://example.com")
            .WithVariable("var", XdmValue.FromString("test"), "http://example.com");
        FunctionLibrary.Populate(ctx);

        var result = Evaluate("$ns:var", ctx);
        Assert.Equal("test", result.StringValue);
    }

    // ------------------------------------------------------------------
    // Context item
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_ContextItem()
    {
        var ctx = new EvaluationContext()
            .WithFocus(XdmValue.FromInteger(42), 1, 1);
        FunctionLibrary.Populate(ctx);

        var result = Evaluate(".", ctx);
        Assert.Equal(42, result.IntegerValue);
    }

    // ------------------------------------------------------------------
    // Arithmetic
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_Addition()
    {
        var result = Evaluate("1 + 2");
        Assert.Equal(3, result.IntegerValue);
    }

    [Fact]
    public void Eval_Subtraction()
    {
        var result = Evaluate("10 - 3");
        Assert.Equal(7, result.IntegerValue);
    }

    [Fact]
    public void Eval_Multiplication()
    {
        var result = Evaluate("4 * 5");
        Assert.Equal(20, result.IntegerValue);
    }

    [Fact]
    public void Eval_Division()
    {
        var result = Evaluate("20 div 4");
        Assert.Equal(5, result.IntegerValue);
    }

    [Fact]
    public void Eval_IntegerDivision()
    {
        var result = Evaluate("7 idiv 2");
        Assert.Equal(3, result.IntegerValue);
    }

    [Fact]
    public void Eval_Modulo()
    {
        var result = Evaluate("10 mod 3");
        Assert.Equal(1, result.IntegerValue);
    }

    [Fact]
    public void Eval_UnaryMinus()
    {
        var result = Evaluate("-5");
        Assert.Equal(-5, result.IntegerValue);
    }

    [Fact]
    public void Eval_UnaryPlus()
    {
        var result = Evaluate("+5");
        Assert.Equal(5, result.IntegerValue);
    }

    [Fact]
    public void Eval_MixedArithmetic()
    {
        var result = Evaluate("1 + 2 * 3");
        Assert.Equal(7, result.IntegerValue);
    }

    // ------------------------------------------------------------------
    // String concat
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_StringConcat()
    {
        var result = Evaluate("'hello' || ' ' || 'world'");
        Assert.Equal("hello world", result.StringValue);
    }

    // ------------------------------------------------------------------
    // Comparisons
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_Equal_True()
    {
        var result = Evaluate("1 eq 1");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_Equal_False()
    {
        var result = Evaluate("1 eq 2");
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void Eval_NotEqual()
    {
        var result = Evaluate("1 ne 2");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_LessThan()
    {
        var result = Evaluate("1 lt 2");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_LessThanOrEqual()
    {
        var result = Evaluate("2 le 2");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_GreaterThan()
    {
        var result = Evaluate("2 gt 1");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_GreaterThanOrEqual()
    {
        var result = Evaluate("2 ge 2");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_GeneralComparison()
    {
        var result = Evaluate("1 = 1");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_StringComparison()
    {
        var result = Evaluate("'a' lt 'b'");
        Assert.True(result.BooleanValue);
    }

    // ------------------------------------------------------------------
    // Boolean logic
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_And_True()
    {
        var result = Evaluate("true() and true()");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_And_False()
    {
        var result = Evaluate("true() and false()");
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void Eval_Or_True()
    {
        var result = Evaluate("false() or true()");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_Or_False()
    {
        var result = Evaluate("false() or false()");
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void Eval_Not()
    {
        var result = Evaluate("not(true())");
        Assert.False(result.BooleanValue);
    }

    // ------------------------------------------------------------------
    // Conditionals
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_If_Then()
    {
        var result = Evaluate("if (true()) then 1 else 2");
        Assert.Equal(1, result.IntegerValue);
    }

    [Fact]
    public void Eval_If_Else()
    {
        var result = Evaluate("if (false()) then 1 else 2");
        Assert.Equal(2, result.IntegerValue);
    }

    [Fact]
    public void Eval_If_VariableCondition()
    {
        var ctx = new EvaluationContext()
            .WithVariable("x", XdmValue.FromBoolean(true));
        FunctionLibrary.Populate(ctx);

        var result = Evaluate("if ($x) then 42 else 99", ctx);
        Assert.Equal(42, result.IntegerValue);
    }

    // ------------------------------------------------------------------
    // Sequences
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_SequenceLiteral()
    {
        var result = Evaluate("(1, 2, 3)");
        Assert.True(result.IsSequence);
        var items = Materialize(result);
        Assert.Equal(3, items.Length);
        Assert.Equal(1, items[0].IntegerValue);
        Assert.Equal(2, items[1].IntegerValue);
        Assert.Equal(3, items[2].IntegerValue);
    }

    [Fact]
    public void Eval_SingletonSequence()
    {
        var result = Evaluate("(42)");
        // Optimizer unwraps single-item parentheses
        Assert.Equal(42, result.IntegerValue);
    }

    [Fact]
    public void Eval_EmptySequence()
    {
        var result = Evaluate("()");
        Assert.True(result.IsSequence);
        Assert.True(result.SequenceValue!.TryGetLength(out var len));
        Assert.Equal(0, len);
    }

    [Fact]
    public void Eval_RangeExpression()
    {
        // Optimizer expands small ranges
        var result = Evaluate("1 to 3");
        Assert.True(result.IsSequence);
        var items = Materialize(result);
        Assert.Equal(3, items.Length);
        Assert.Equal(1, items[0].IntegerValue);
        Assert.Equal(2, items[1].IntegerValue);
        Assert.Equal(3, items[2].IntegerValue);
    }

    // ------------------------------------------------------------------
    // Function calls
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_FunctionCall_Count()
    {
        var result = Evaluate("count((1, 2, 3))");
        Assert.Equal(3, result.IntegerValue);
    }

    [Fact]
    public void Eval_FunctionCall_Concat()
    {
        var result = Evaluate("concat('a', 'b')");
        Assert.Equal("ab", result.StringValue);
    }

    [Fact]
    public void Eval_FunctionCall_Exists_True()
    {
        var result = Evaluate("exists((1, 2))");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_FunctionCall_Exists_False()
    {
        var result = Evaluate("exists(())");
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void Eval_FunctionCall_Empty_True()
    {
        var result = Evaluate("empty(())");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_FunctionCall_Head()
    {
        var result = Evaluate("head((1, 2, 3))");
        Assert.Equal(1, result.IntegerValue);
    }

    [Fact]
    public void Eval_FunctionCall_String()
    {
        var result = Evaluate("fn:string(42)");
        Assert.Equal("42", result.StringValue);
    }

    // ------------------------------------------------------------------
    // Cast
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_Cast_StringToInteger()
    {
        var result = Evaluate("'42' cast as xs:integer");
        Assert.Equal(42, result.IntegerValue);
    }

    [Fact]
    public void Eval_Cast_IntegerToString()
    {
        var result = Evaluate("42 cast as xs:string");
        Assert.Equal("42", result.StringValue);
    }

    // ------------------------------------------------------------------
    // Instance of
    // ------------------------------------------------------------------

    [Fact]
    public void Eval_InstanceOf_Integer()
    {
        var result = Evaluate("1 instance of xs:integer");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_InstanceOf_String()
    {
        var result = Evaluate("'hello' instance of xs:string");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Eval_InstanceOf_False()
    {
        var result = Evaluate("'hello' instance of xs:integer");
        Assert.False(result.BooleanValue);
    }

    // ------------------------------------------------------------------
    // End-to-end via XPath31Expression API
    // ------------------------------------------------------------------

    [Fact]
    public void Api_CompileAndEvaluate_Literal()
    {
        var expr = XPath31Expression.Compile("42");
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        var result = expr.Evaluate(ctx);
        Assert.Equal(42, result.IntegerValue);
    }

    [Fact]
    public void Api_CompileAndEvaluate_Arithmetic()
    {
        var expr = XPath31Expression.Compile("$x + 1");
        var ctx = new EvaluationContext()
            .WithVariable("x", XdmValue.FromInteger(5));
        FunctionLibrary.Populate(ctx);
        var result = expr.Evaluate(ctx);
        Assert.Equal(6, result.IntegerValue);
    }

    [Fact]
    public void Api_CompileAndEvaluate_Conditional()
    {
        var expr = XPath31Expression.Compile("if ($x) then 'yes' else 'no'");
        var ctx = new EvaluationContext()
            .WithVariable("x", XdmValue.FromBoolean(true));
        FunctionLibrary.Populate(ctx);
        var result = expr.Evaluate(ctx);
        Assert.Equal("yes", result.StringValue);
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static XdmValue[] Materialize(XdmValue sequence)
    {
        if (sequence.IsSequence && sequence.SequenceValue is not null)
        {
            var list = new List<XdmValue>();
            foreach (var item in XdmSequence.FromSource(sequence.SequenceValue))
                list.Add(item);
            return list.ToArray();
        }
        if (sequence.IsUndefined)
            return Array.Empty<XdmValue>();
        return new[] { sequence };
    }
}
