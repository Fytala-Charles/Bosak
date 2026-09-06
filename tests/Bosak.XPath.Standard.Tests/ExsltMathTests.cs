// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 September 2026
// PURPOSE              : Unit tests for the EXSLT math extension library (http://exslt.org/math).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

public class ExsltMathTests
{
    private const string ExsltMathNs = "http://exslt.org/math";

    private static XdmValue Evaluate(string xpath)
    {
        var ctx = new EvaluationContext().WithNamespace("exslt", ExsltMathNs);
        FunctionLibrary.Populate(ctx);
        return XPath31Expression.Compile(xpath).Evaluate(ctx);
    }

    private static string EvalStr(string xpath)
        => Evaluate(xpath).ToString();

    // ------------------------------------------------------------------
    // math:constant — happy paths
    // ------------------------------------------------------------------

    [Fact]
    public void Constant_Pi_FullPrecision()
        => Assert.Equal(Math.PI, Evaluate("exslt:constant('PI')").DoubleValue, precision: 12);

    [Fact]
    public void Constant_Pi_Precision4()
        => Assert.Equal("3.1415", EvalStr("exslt:constant('PI', 4)"));

    [Fact]
    public void Constant_E_Precision12()
        => Assert.Equal(2.718281828459, Evaluate("exslt:constant('E', 12)").DoubleValue, precision: 12);

    [Fact]
    public void Constant_Sqrt1_2()
        => Assert.Equal(0.7071067811865476, Evaluate("exslt:constant('SQRT1_2')").DoubleValue, precision: 15);

    [Fact]
    public void Constant_AllSevenNames()
    {
        Assert.False(double.IsNaN(Evaluate("exslt:constant('PI')").DoubleValue));
        Assert.False(double.IsNaN(Evaluate("exslt:constant('E')").DoubleValue));
        Assert.False(double.IsNaN(Evaluate("exslt:constant('SQRRT2')").DoubleValue));
        Assert.False(double.IsNaN(Evaluate("exslt:constant('LN2')").DoubleValue));
        Assert.False(double.IsNaN(Evaluate("exslt:constant('LN10')").DoubleValue));
        Assert.False(double.IsNaN(Evaluate("exslt:constant('LOG2E')").DoubleValue));
        Assert.False(double.IsNaN(Evaluate("exslt:constant('SQRT1_2')").DoubleValue));
    }

    // ------------------------------------------------------------------
    // math:constant — argument validation (extension-functions-0201)
    // ------------------------------------------------------------------

    [Fact]
    public void Constant_UnknownName_RaisesXTDE1420()
        => Assert.Contains("XTDE1420", Assert.Throws<InvalidOperationException>(
            () => Evaluate("exslt:constant('BOGUS')")).Message);

    [Fact]
    public void Constant_PrecisionZero_RaisesXTDE1420()
        => Assert.Contains("XTDE1420", Assert.Throws<InvalidOperationException>(
            () => Evaluate("exslt:constant('PI', 0)")).Message);

    [Fact]
    public void Constant_PrecisionBeyondDigits_RaisesXTDE1420()
        => Assert.Contains("XTDE1420", Assert.Throws<InvalidOperationException>(
            () => Evaluate("exslt:constant('PI', 60)")).Message);

    [Fact]
    public void Constant_UnconvertiblePrecision_RaisesXTDE1425()
        => Assert.Contains("XTDE1425", Assert.Throws<InvalidOperationException>(
            () => Evaluate("exslt:constant('PI', 'abc')")).Message);

    [Fact]
    public void Constant_W3CExtensionFunctions0201_ArgumentsRejected()
        // The W3C extension-functions-0201 scenario: math:constant(1, 'PI') in a
        // version="1.0" stylesheet must raise XTDE1420 (unknown constant "1") or
        // XTDE1425 (precision 'PI' not convertible to number).
        => Assert.Contains("XTDE14", Assert.Throws<InvalidOperationException>(
            () => Evaluate("exslt:constant(1, 'PI')")).Message);

    // ------------------------------------------------------------------
    // Other EXSLT math functions
    // ------------------------------------------------------------------

    [Fact]
    public void Sqrt_PerfectSquare() => Assert.Equal("16", EvalStr("exslt:sqrt(256)"));

    [Fact]
    public void Power_IntegerExponent() => Assert.Equal("1024", EvalStr("exslt:power(2, 10)"));

    [Fact]
    public void Power_FractionalExponent() => Assert.Equal("2", EvalStr("exslt:power(16, 0.25)"));

    [Fact]
    public void Abs_Negative() => Assert.Equal("3.5", EvalStr("exslt:abs(-3.5)"));

    [Fact]
    public void Sin_Zero() => Assert.Equal("0", EvalStr("exslt:sin(0)"));

    [Fact]
    public void Cos_Zero() => Assert.Equal("1", EvalStr("exslt:cos(0)"));

    [Fact]
    public void Tan_Zero() => Assert.Equal("0", EvalStr("exslt:tan(0)"));

    [Fact]
    public void Log_One() => Assert.Equal("0", EvalStr("exslt:log(1)"));

    [Fact]
    public void Exp_Zero() => Assert.Equal("1", EvalStr("exslt:exp(0)"));

    [Fact]
    public void Atan2_AxisAngle() => Assert.Equal(Math.PI / 2, Evaluate("exslt:atan2(1, 0)").DoubleValue, precision: 12);

    [Fact]
    public void Max_Sequence() => Assert.Equal("5", EvalStr("exslt:max((1, 5, 3))"));

    [Fact]
    public void Min_Sequence() => Assert.Equal("1", EvalStr("exslt:min((4, 1, 3))"));

    [Fact]
    public void Max_EmptySequenceYieldsNaN()
        => Assert.Equal("NaN", EvalStr("exslt:max(())"));

    // ------------------------------------------------------------------
    // Namespace isolation from the standard math:* library
    // ------------------------------------------------------------------

    [Fact]
    public void StandardMathNamespace_Unaffected()
    {
        // The EXSLT library binds http://exslt.org/math; the XSLT 2.0+ math:*
        // functions in http://www.w3.org/2005/xpath-functions/math keep priority.
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        Assert.Equal(Math.PI, XPath31Expression.Compile("math:pi()").Evaluate(ctx).DoubleValue, precision: 12);
        Assert.True(FunctionLibrary.TryGetFunction(ExsltMathNs, "constant", 2, out _));
        Assert.True(FunctionLibrary.TryGetFunction("http://www.w3.org/2005/xpath-functions/math", "pi", 0, out _));
    }
}
