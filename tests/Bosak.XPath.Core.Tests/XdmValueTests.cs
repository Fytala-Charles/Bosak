// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Source file for XdmValueTests in the Development project
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
using Xunit;

namespace Bosak.XPath.Core.Tests;

public class XdmValueTests
{
    [Fact]
    public void BooleanValue_Roundtrips()
    {
        Assert.True(XdmValue.FromBoolean(true).BooleanValue);
        Assert.False(XdmValue.FromBoolean(false).BooleanValue);
    }

    [Fact]
    public void IntegerValue_Roundtrips()
    {
        Assert.Equal(42, XdmValue.FromInteger(42).IntegerValue);
        Assert.Equal(-999, XdmValue.FromInteger(-999).IntegerValue);
    }

    [Fact]
    public void DoubleValue_Roundtrips()
    {
        Assert.Equal(3.14, XdmValue.FromDouble(3.14).DoubleValue, precision: 2);
    }

    [Fact]
    public void StringValue_Roundtrips()
    {
        Assert.Equal("hello", XdmValue.FromString("hello").StringValue);
    }

    [Fact]
    public void EffectiveBooleanValue_FollowsXPathRules()
    {
        Assert.True(XdmValue.FromBoolean(true).EffectiveBooleanValue());
        Assert.False(XdmValue.FromBoolean(false).EffectiveBooleanValue());
        Assert.False(XdmValue.FromInteger(0).EffectiveBooleanValue());
        Assert.True(XdmValue.FromInteger(1).EffectiveBooleanValue());
        Assert.False(XdmValue.FromDouble(0.0).EffectiveBooleanValue());
        Assert.True(XdmValue.FromDouble(1.5).EffectiveBooleanValue());
        Assert.False(XdmValue.FromString("").EffectiveBooleanValue());
        Assert.True(XdmValue.FromString("x").EffectiveBooleanValue());
        Assert.False(XdmValue.Undefined.EffectiveBooleanValue());
    }

    [Fact]
    public void EmptySequence_HasZeroLength()
    {
        var seq = XdmSequence.Empty;
        Assert.True(seq.TryGetLength(out var len));
        Assert.Equal(0, len);

        int count = 0;
        foreach (var _ in seq)
            count++;
        Assert.Equal(0, count);
    }

    [Fact]
    public void SingletonSequence_YieldsOneItem()
    {
        var seq = XdmSequence.Singleton(XdmValue.FromInteger(99));
        Assert.True(seq.TryGetLength(out var len));
        Assert.Equal(1, len);

        var items = new List<long>();
        foreach (var v in seq)
            items.Add(v.IntegerValue);

        Assert.Single(items);
        Assert.Equal(99, items[0]);
    }

    // ------------------------------------------------------------------
    // REQ-008: Double-to-string precision tests
    // ------------------------------------------------------------------

    [Fact]
    public void DoubleToString_MathPi()
    {
        var v = XdmValue.FromDouble(Math.PI);
        Assert.Equal("3.141592653589793", v.ToString());
    }

    [Fact]
    public void DoubleToString_MathSin1()
    {
        var v = XdmValue.FromDouble(Math.Sin(1.0));
        Assert.Equal("0.8414709848078965", v.ToString());
    }

    [Fact]
    public void DoubleToString_MathCos1()
    {
        var v = XdmValue.FromDouble(Math.Cos(1.0));
        Assert.Equal("0.5403023058681398", v.ToString());
    }

    [Fact]
    public void DoubleToString_MathTan1()
    {
        var v = XdmValue.FromDouble(Math.Tan(1.0));
        Assert.Equal("1.5574077246549023", v.ToString());
    }

    [Fact]
    public void DoubleToString_MathAsin1()
    {
        var v = XdmValue.FromDouble(Math.Asin(1.0));
        Assert.Equal("1.5707963267948966", v.ToString());
    }

    [Fact]
    public void DoubleToString_MathAtan1()
    {
        var v = XdmValue.FromDouble(Math.Atan(1.0));
        Assert.Equal("0.7853981633974483", v.ToString());
    }

    [Fact]
    public void DoubleToString_MathAtan2_1_1()
    {
        var v = XdmValue.FromDouble(Math.Atan2(1.0, 1.0));
        Assert.Equal("0.7853981633974483", v.ToString());
    }

    [Fact]
    public void DoubleToString_ZeroPointOne()
    {
        var v = XdmValue.FromDouble(0.1);
        Assert.Equal("0.1", v.ToString());
    }

    [Fact]
    public void DoubleToString_ScientificNotation()
    {
        var v = XdmValue.FromDouble(1.23e6);
        Assert.Equal("1.23E6", v.ToString());
    }

    [Fact]
    public void DoubleToString_SmallScientific()
    {
        var v = XdmValue.FromDouble(1.23e-7);
        Assert.Equal("1.23E-7", v.ToString());
    }
}
