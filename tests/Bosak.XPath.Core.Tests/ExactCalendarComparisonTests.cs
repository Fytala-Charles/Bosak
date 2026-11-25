// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Verifies exact calendar comparison, partial ordering and same-key laws.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using Bosak.XPath.Core.Xdm;
using Xunit;
using C = Bosak.XPath.Core.Xdm.XdmExactCalendarComparison;
using O = Bosak.XPath.Core.Xdm.XdmExactCalendarOrder;

namespace Bosak.XPath.Core.Tests;

/// <summary>Qualifies explicit exact comparison contracts against independent vectors and an oracle.</summary>
public sealed class ExactCalendarComparisonTests
{
    private static XdmExactCalendarValue D(string text, bool stamp=false)=>XdmExactCalendarValue.Parse(text,stamp?XdmExactCalendarKind.DateTimeStamp:XdmExactCalendarKind.DateTime,256,256);
    private static XdmExactCalendarValue T(string text)=>XdmExactCalendarValue.Parse(text,XdmExactCalendarKind.Time,256,256);
    [Theory] [InlineData("2000-01-01T12:00:00Z","2000-01-01T13:00:00+01:00",0)] [InlineData("2000-01-01T12:00:00Z","2000-01-01T12:00:00.000000001Z",-1)] [InlineData("2000-01-01T13:00:00Z","2000-01-01T12:00:00Z",1)]
    /// <summary>Verifies C01 AwareComparison.</summary>
    /// <param name="a">Contract vector input.</param>
    /// <param name="b">Contract vector input.</param>
    /// <param name="order">Contract vector input.</param>
    public void C01_AwareComparison(string a,string b,int order)=>Assert.Equal(order,C.CompareXPath(D(a),D(b),0));
    [Theory] [InlineData("2000-01-01T00:00:00+14:00","1999-12-31T10:00:00Z")] [InlineData("2000-03-01T00:00:00+01:00","2000-02-29T23:00:00Z")] [InlineData("0001-01-01T00:00:00+14:00","0000-12-31T10:00:00Z")]
    /// <summary>Verifies C02 OffsetRollover.</summary>
    /// <param name="a">Contract vector input.</param>
    /// <param name="b">Contract vector input.</param>
    [InlineData("-0001-12-31T23:00:00Z","0000-01-01T00:00:00+01:00")] [InlineData("-0400-03-01T00:00:00+01:00","-0400-02-29T23:00:00Z")] [InlineData("-0100-03-01T00:00:00+01:00","-0100-02-28T23:00:00Z")]
    public void C02_OffsetRollover(string a,string b)=>Assert.Equal(0,C.CompareXPath(D(a),D(b),0));
    /// <summary>Verifies C03 ExplicitImplicitTimezone.</summary>
    [Fact] public void C03_ExplicitImplicitTimezone()
    {
        var a=D("2000-01-01T12:00:00");var b=D("2000-01-01T11:00:00Z");Assert.Equal(0,C.CompareXPath(a,b,60));Assert.Equal(1,C.CompareXPath(a,b,0));Assert.Equal(-1,C.CompareXPath(a,b,120));
        Assert.Equal(0,C.CompareXPath(D("2000-01-01T12:00:00"),D("2000-01-01T12:00:00"),-840));Assert.Equal(0,C.CompareXPath(T("12:00:00"),T("11:00:00Z"),60));
    }
    [Theory] [InlineData(-841)] [InlineData(841)] [InlineData(int.MinValue)] [InlineData(int.MaxValue)]
    /// <summary>Verifies C04 InvalidContextAlwaysRefuses.</summary>
    /// <param name="zone">Contract vector input.</param>
    public void C04_InvalidContextAlwaysRefuses(int zone)=>Assert.Throws<ArgumentOutOfRangeException>(()=>C.CompareXPath(D("2000-01-01T12:00:00Z"),D("2000-01-01T12:00:00Z"),zone));
    /// <summary>Verifies C05 StampBaseFamily.</summary>
    [Fact] public void C05_StampBaseFamily()
    {
        var a=D("2000-01-01T12:00:00Z",true);var b=D("2000-01-01T13:00:00+01:00");Assert.Equal(0,C.CompareXPath(a,b,0));Assert.Equal(O.Equal,C.CompareXsd(a,b));Assert.True(C.SameKey(a,b));Assert.NotEqual(a,b);
    }
    /// <summary>Verifies C06 InvalidAndIncompatibleOperands.</summary>
    [Fact] public void C06_InvalidAndIncompatibleOperands()
    {
        var a=D("2000-01-01T12:00:00Z");var b=T("12:00:00Z");Assert.Throws<ArgumentException>(()=>C.CompareXPath(default,a,0));Assert.Throws<ArgumentException>(()=>C.CompareXsd(a,default));Assert.Throws<ArgumentException>(()=>C.CompareXPath(a,b,0));Assert.Throws<ArgumentException>(()=>C.CompareXsd(b,a));Assert.False(C.SameKey(a,b));Assert.Throws<ArgumentException>(()=>C.SameKey(a,default));Assert.Throws<ArgumentException>(()=>C.GetSameKeyHashCode(default));
    }
    [Theory] [InlineData("23:00:00-03:00","02:00:00Z",1)] [InlineData("17:00:00-06:00","08:00:00+09:00",1)] [InlineData("21:30:00+10:30","06:00:00-05:00",0)] [InlineData("00:00:00+02:00","23:00:00+01:00",-1)]
    /// <summary>Verifies C07 TimeReferenceDay.</summary>
    /// <param name="a">Contract vector input.</param>
    /// <param name="b">Contract vector input.</param>
    /// <param name="order">Contract vector input.</param>
    public void C07_TimeReferenceDay(string a,string b,int order)=>Assert.Equal(order,C.CompareXPath(T(a),T(b),0));
    /// <summary>Verifies C08 ExactFractionPrecision.</summary>
    [Fact] public void C08_ExactFractionPrecision()
    {
        string prefix="2000-01-01T00:00:00.";var a=D(prefix+new string('0',255)+"1Z");var b=D(prefix+new string('0',255)+"2Z");Assert.Equal(-1,C.CompareXPath(a,b,0));Assert.False(C.SameKey(a,b));Assert.Equal(O.Less,C.CompareXsd(a,b));
        Assert.Equal(-1,C.CompareXPath(D("2000-01-01T00:00:05.123456788Z"),D("2000-01-01T00:00:05.123456789Z"),0));
    }
    [Theory] [InlineData("-9223372036854775807")] [InlineData("-0400")] [InlineData("0000")] [InlineData("10000")] [InlineData("9223372036854775807")]
    /// <summary>Verifies C09 WideGregorianYears.</summary>
    /// <param name="year">Contract vector input.</param>
    public void C09_WideGregorianYears(string year)
    {
        Assert.Equal(-1,C.CompareXPath(D(year+"-01-01T00:00:00Z"),D(year+"-01-02T00:00:00Z"),0));Assert.Equal(-1,C.CompareXPath(D(year+"-12-31T23:59:59+14:00"),D(year+"-12-31T23:59:59-14:00"),0));
    }
    [Theory] [InlineData("0000-02-28T24:00:00Z","0000-02-29T00:00:00Z")] [InlineData("1900-02-28T24:00:00Z","1900-03-01T00:00:00Z")] [InlineData("-0001-12-31T24:00:00Z","0000-01-01T00:00:00Z")]
    /// <summary>Verifies C10 EndOfDayAndLeapYears.</summary>
    /// <param name="a">Contract vector input.</param>
    /// <param name="b">Contract vector input.</param>
    public void C10_EndOfDayAndLeapYears(string a,string b)
    {Assert.Equal(0,C.CompareXPath(D(a),D(b),0));Assert.Equal(0,C.CompareXPath(T("24:00:00+01:00"),T("00:00:00+01:00"),0));}
    /// <summary>Verifies C11 XsdAwareOrder.</summary>
    [Fact] public void C11_XsdAwareOrder()
    {
        Assert.Equal(O.Equal,C.CompareXsd(D("2000-01-01T12:00:00Z"),D("2000-01-01T13:00:00+01:00")));Assert.Equal(O.Less,C.CompareXsd(D("2000-01-01T12:00:00Z"),D("2000-01-01T13:00:00Z")));Assert.Equal(O.Greater,C.CompareXsd(T("17:00:00-06:00"),T("08:00:00+09:00")));
    }
    /// <summary>Verifies C12 XsdAbsentOrder.</summary>
    [Fact] public void C12_XsdAbsentOrder()
    {
        Assert.Equal(O.Equal,C.CompareXsd(D("2000-01-01T12:00:00"),D("2000-01-01T12:00:00")));Assert.Equal(O.Less,C.CompareXsd(T("01:00:00"),T("02:00:00")));Assert.Equal(O.Greater,C.CompareXsd(D("0000-01-02T00:00:00"),D("0000-01-01T00:00:00")));
    }
    [Theory] [InlineData("2000-01-01T00:00:00","2000-01-01T14:00:00.000000001Z")] [InlineData("2000-01-01T00:00:00Z","2000-01-01T14:00:00.000000001")]
    /// <summary>Verifies C13 MixedDefiniteLess.</summary>
    /// <param name="a">Contract vector input.</param>
    /// <param name="b">Contract vector input.</param>
    public void C13_MixedDefiniteLess(string a,string b){Assert.Equal(O.Less,C.CompareXsd(D(a),D(b)));Assert.Equal(O.Less,C.CompareXsd(T("00:00:00Z"),T("14:00:00.000000001")));}
    [Theory] [InlineData("2000-01-01T14:00:00.000000001","2000-01-01T00:00:00Z")] [InlineData("2000-01-01T14:00:00.000000001Z","2000-01-01T00:00:00")]
    /// <summary>Verifies C14 MixedDefiniteGreater.</summary>
    /// <param name="a">Contract vector input.</param>
    /// <param name="b">Contract vector input.</param>
    public void C14_MixedDefiniteGreater(string a,string b){Assert.Equal(O.Greater,C.CompareXsd(D(a),D(b)));Assert.Equal(O.Greater,C.CompareXsd(T("14:00:00.000000001Z"),T("00:00:00")));}
    [Theory] [InlineData("2000-01-01T00:00:00","2000-01-01T14:00:00Z")] [InlineData("2000-01-01T00:00:00","1999-12-31T10:00:00Z")] [InlineData("2000-01-01T00:00:00","2000-01-01T00:00:00Z")]
    /// <summary>Verifies C15 MixedIndeterminate.</summary>
    /// <param name="a">Contract vector input.</param>
    /// <param name="b">Contract vector input.</param>
    public void C15_MixedIndeterminate(string a,string b)
    {Assert.Equal(O.Indeterminate,C.CompareXsd(D(a),D(b)));Assert.Equal(O.Indeterminate,C.CompareXsd(D(b),D(a)));Assert.Equal(O.Indeterminate,C.CompareXsd(T("00:00:00"),T("14:00:00Z")));}
    /// <summary>Verifies C16 SameKeyPresence.</summary>
    [Fact] public void C16_SameKeyPresence()
    {
        var a=D("2000-01-01T12:00:00");var b=D("2000-01-01T12:00:00Z");Assert.Equal(0,C.CompareXPath(a,b,0));Assert.False(C.SameKey(a,b));Assert.True(C.SameKey(a,D("2000-01-01T12:00:00")));
    }
    /// <summary>Verifies C17 SameKeyHashAndDictionary.</summary>
    [Fact] public void C17_SameKeyHashAndDictionary()
    {
        var a=D("2000-01-01T12:00:05.12300Z");var b=D("2000-01-01T13:00:05.123+01:00",true);Assert.True(C.SameKey(a,b));Assert.Equal(C.GetSameKeyHashCode(a),C.GetSameKeyHashCode(b));var map=new Dictionary<XdmExactCalendarValue,string>(XdmExactCalendarKeyComparer.Instance){[a]="first",[b]="second"};Assert.Single(map);Assert.Equal("second",map[a]);
    }
    /// <summary>Verifies C18 TimeKeyRules.</summary>
    [Fact] public void C18_TimeKeyRules()
    {
        var a=T("21:30:00+10:30");var b=T("06:00:00-05:00");Assert.True(C.SameKey(a,b));Assert.Equal(C.GetSameKeyHashCode(a),C.GetSameKeyHashCode(b));Assert.False(C.SameKey(T("23:00:00-03:00"),T("02:00:00Z")));Assert.False(C.SameKey(a,D("1972-12-31T21:30:00+10:30")));
    }
    /// <summary>Verifies C20 DtoOracleAndLaws.</summary>
    [Fact] public void C20_DtoOracleAndLaws()
    {
        var random=new Random(20261125);
        for(int i=0;i<500;i++)
        {
            DateTimeOffset Make()=>new DateTimeOffset(2000,1,1,0,0,0,TimeSpan.FromMinutes(random.Next(-840,841))).AddTicks(random.NextInt64(0,366L*TimeSpan.TicksPerDay));
            var a=Make();var b=Make();var c=Make();var x=XdmExactCalendarValue.FromDateTimeOffset(a);var y=XdmExactCalendarValue.FromDateTimeOffset(b);var z=XdmExactCalendarValue.FromDateTimeOffset(c);
            int ab=C.CompareXPath(x,y,0),bc=C.CompareXPath(y,z,0);Assert.Equal(Math.Sign(a.UtcTicks.CompareTo(b.UtcTicks)),ab);Assert.Equal(-ab,C.CompareXPath(y,x,0));if(ab<=0&&bc<=0)Assert.True(C.CompareXPath(x,z,0)<=0);
            DateTimeOffset Time()=>new DateTimeOffset(1972,12,31,0,0,0,TimeSpan.FromMinutes(random.Next(-840,841))).AddTicks(random.NextInt64(0,TimeSpan.TicksPerDay));
            var ta=Time();var tb=Time();Assert.Equal(Math.Sign(ta.UtcTicks.CompareTo(tb.UtcTicks)),C.CompareXPath(T(ta.ToString("HH:mm:ss.fffffffzzz",CultureInfo.InvariantCulture)),T(tb.ToString("HH:mm:ss.fffffffzzz",CultureInfo.InvariantCulture)),0));
            var shifted=XdmExactCalendarValue.FromDateTimeOffset(a.ToOffset(TimeSpan.FromMinutes(random.Next(-840,841))));Assert.True(C.SameKey(x,shifted));Assert.Equal(C.GetSameKeyHashCode(x),C.GetSameKeyHashCode(shifted));
        }
    }
}
