// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Verifies plain legacy and exact calendar interoperability contracts.
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
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Xunit;
using C = Bosak.XPath.Core.Xdm.XdmCalendarInterop;
using O = Bosak.XPath.Core.Xdm.XdmExactCalendarOrder;

namespace Bosak.XPath.Core.Tests;

/// <summary>Qualifies explicit legacy/exact calendar conversion and comparison contracts.</summary>
public sealed class CalendarInteropTests
{
    private static XdmValue E(string text="2000-01-02T03:04:05.123Z", XdmExactCalendarKind kind=XdmExactCalendarKind.DateTime)
        => XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse(text,kind,256,256));
    private static XPathDateTime Raw(long year=2000,int month=1,int day=2,int hour=3,int minute=4,int second=5,int ms=123,int zone=0,bool tz=true)
        => new(year,month,day,hour,minute,second,ms,zone,tz);
    private static XdmValue L(XPathDateTime? raw=null,bool tz=true,string? label=null)
        => label is null ? XdmValue.FromDateTime(raw??Raw(),tz) : XdmValue.FromDateTime(raw??Raw(),tz,label);
    private static XdmValue T(XPathDateTime? raw=null,bool tz=true,string? label=null)
        => label is null ? XdmValue.FromTime(raw??Raw(),tz) : XdmValue.FromTime(raw??Raw(),tz,label);

    /// <summary>Checks C01 legacy dateTime projection preserves every meaningful field.</summary>
    [Fact] public void C01_DateTimeComponents()
    {
        Assert.Equal(E().ExactCalendarValue,C.Read(L()));
        Assert.Equal(E("2000-01-02T03:04:05.123+02:00").ExactCalendarValue,C.Read(L(Raw(zone:120))));
    }

    /// <summary>Checks C02 time discards incidental dates, including invalid date-only fields.</summary>
    /// <param name="year">Incidental reference year.</param>
    /// <param name="month">Incidental reference month.</param>
    /// <param name="day">Incidental reference day.</param>
    [Theory] [InlineData(2000,1,2)] [InlineData(2026,11,25)] [InlineData(long.MinValue,-1,100)] [InlineData(0,0,0)]
    public void C02_TimeDateProjection(long year,int month,int day)
    {
        var result=C.Read(T(Raw(year,month,day)));
        Assert.Equal(E("03:04:05.123Z",XdmExactCalendarKind.Time).ExactCalendarValue,result);
        Assert.Equal(0,result.Year);Assert.Equal(0,result.Month);Assert.Equal(0,result.Day);
    }

    /// <summary>Checks C03 legacy milliseconds survive; discarded DTO tick digits are not invented.</summary>
    /// <param name="ms">Stored millisecond.</param>
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(120)] [InlineData(999)]
    public void C03_StoredMilliseconds(int ms)
    {
        var value=C.Read(L(Raw(ms:ms)));Assert.Equal(5000+ms,(int)value.Second.ToExactUnits(3));
        var dto=new DateTimeOffset(2000,1,2,3,4,5,TimeSpan.Zero).AddTicks(ms*10000+1234);
        Assert.Equal(value,C.Read(XdmValue.FromDateTime(dto)));
        Assert.NotEqual(XdmExactCalendarValue.FromDateTimeOffset(dto),C.Read(XdmValue.FromDateTime(dto)));
    }

    /// <summary>Checks C04 exact payloads retain all256fraction digits.</summary>
    [Fact] public void C04_ExactPreserved()
    {
        var e=E("2000-01-02T03:04:00."+new string('0',255)+"1Z");Assert.Equal(e.ExactCalendarValue,C.Read(e));Assert.Equal(256,C.Read(e).Second.Scale);
        Assert.Equal(E("03:04:05.123456789Z",XdmExactCalendarKind.Time).ExactCalendarValue,C.Read(E("03:04:05.123456789Z",XdmExactCalendarKind.Time)));
    }

    /// <summary>Checks C05 timezone presence is independent from a zero offset.</summary>
    /// <param name="zone">Offset minutes.</param>
    [Theory] [InlineData(-840)] [InlineData(0)] [InlineData(840)]
    public void C05_Timezones(int zone)
    {
        var a=C.Read(L(Raw(zone:zone)));Assert.True(a.HasTimezone);Assert.Equal(zone,a.TimezoneOffsetMinutes);
        var absent=C.Read(L(Raw(tz:false),false));Assert.False(absent.HasTimezone);Assert.Equal(0,absent.TimezoneOffsetMinutes);
    }

    /// <summary>Checks C06 disagreement between wrapper and raw timezone presence refuses.</summary>
    /// <param name="raw">Raw presence.</param>
    [Theory] [InlineData(true)] [InlineData(false)]
    public void C06_ContradictoryPresence(bool raw)
    {
        Assert.Throws<ArgumentException>(()=>C.Read(L(Raw(tz:raw),!raw)));
        Assert.Throws<ArgumentException>(()=>C.Read(T(Raw(tz:raw),!raw)));
    }

    /// <summary>Checks C07 hidden absent offsets refuse instead of being repaired.</summary>
    /// <param name="zone">Hidden offset minutes.</param>
    [Theory] [InlineData(-60)] [InlineData(60)]
    public void C07_HiddenOffset(int zone)
    {
        Assert.Throws<ArgumentException>(()=>C.Read(L(Raw(zone:zone,tz:false),false)));
        Assert.Throws<ArgumentException>(()=>C.Read(T(Raw(zone:zone,tz:false),false)));
    }

    /// <summary>Checks C08 known built-in labels remain descriptive and user names grant nothing.</summary>
    [Fact] public void C08_Labels()
    {
        Assert.Equal(XdmExactCalendarKind.DateTime,C.Read(L(label:"dateTime")).Kind);
        Assert.Equal(XdmExactCalendarKind.DateTimeStamp,C.Read(L(label:"dateTimeStamp")).Kind);
        Assert.Equal(XdmExactCalendarKind.Time,C.Read(T(label:"time")).Kind);
        var value=XdmValue.FromDateTime(Raw(),true,"dateTime","Q{urn:example}Special");
        Assert.Equal(C.Read(L()),C.Read(value));Assert.False(SchemaAtomics.TryGetProvenance(value,out _));
        Assert.Throws<ArgumentOutOfRangeException>(()=>C.Read(L(Raw(tz:false),false,"dateTimeStamp")));
    }

    /// <summary>Checks C09 unknown and incompatible physical family labels refuse.</summary>
    /// <param name="label">Unsupported dateTime label.</param>
    [Theory] [InlineData("gYear")] [InlineData("time")] [InlineData("xs:dateTime")] [InlineData("Q{urn:x}T")] [InlineData("")]
    public void C09_UnsupportedLabels(string label)
    {
        Assert.Throws<NotSupportedException>(()=>C.Read(L(label:label)));
        Assert.Throws<NotSupportedException>(()=>C.Read(T(label:"dateTimeStamp")));
    }

    /// <summary>Checks C10 invalid seconds, milliseconds and clock fields refuse.</summary>
    /// <param name="second">Second component.</param>
    /// <param name="ms">Millisecond component.</param>
    /// <param name="hour">Hour component.</param>
    /// <param name="minute">Minute component.</param>
    [Theory] [InlineData(-1,0,0,0)] [InlineData(60,0,0,0)] [InlineData(0,-1,0,0)] [InlineData(0,1000,0,0)] [InlineData(0,0,25,0)] [InlineData(0,0,0,60)] [InlineData(0,1,24,0)]
    public void C10_InvalidClock(int second,int ms,int hour,int minute)
    {
        var raw=Raw(second:second,ms:ms,hour:hour,minute:minute);
        Assert.Throws<ArgumentOutOfRangeException>(()=>C.Read(L(raw)));Assert.Throws<ArgumentOutOfRangeException>(()=>C.Read(T(raw)));
    }

    /// <summary>Checks C11 extended Gregorian dates and invalid fields without DTO narrowing.</summary>
    /// <param name="year">Astronomical year.</param>
    [Theory] [InlineData(-400)] [InlineData(0)] [InlineData(10000)] [InlineData(long.MaxValue)]
    public void C11_ExtendedGregorian(long year)
    {
        Assert.Equal(year,C.Read(L(Raw(year))).Year);
        Assert.Throws<ArgumentOutOfRangeException>(()=>C.Read(L(Raw(1900,2,29))));
        Assert.Equal(29,C.Read(L(Raw(0,2,29))).Day);
        Assert.Throws<ArgumentOutOfRangeException>(()=>C.Read(L(Raw(long.MinValue))));
        Assert.Throws<ArgumentOutOfRangeException>(()=>C.Read(L(Raw(zone:841))));
    }

    /// <summary>Checks C12 normalized end-of-day and overflow refusal.</summary>
    [Fact] public void C12_EndOfDay()
    {
        Assert.Equal(E("2000-03-01T00:00:00Z").ExactCalendarValue,C.Read(L(Raw(2000,2,29,24,0,0,0))));
        Assert.Equal(E("00:00:00Z",XdmExactCalendarKind.Time).ExactCalendarValue,C.Read(T(Raw(hour:24,minute:0,second:0,ms:0))));
        Assert.Throws<NotSupportedException>(()=>C.Read(L(Raw(long.MaxValue,12,31,24,0,0,0))));
    }

    /// <summary>Checks C13 mixed comparisons use explicit context and exact fractional order.</summary>
    [Fact] public void C13_MixedXPath()
    {
        var legacy=L();Assert.Equal(0,C.CompareXPath(legacy,E(),0));Assert.Equal(-1,C.CompareXPath(legacy,E("2000-01-02T03:04:05.123000001Z"),0));
        var absent=L(Raw(tz:false),false);Assert.Equal(0,C.CompareXPath(absent,E("2000-01-02T02:04:05.123Z"),60));
        Assert.Equal(1,C.CompareXPath(absent,E("2000-01-02T02:04:05.123Z"),0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>C.CompareXPath(legacy,E(),841));
        Assert.Equal(0,C.CompareXPath(T(Raw(1999,1,1)),E("03:04:05.123Z",XdmExactCalendarKind.Time),0));
    }

    /// <summary>Checks C14 XSD partial order preserves mixed timezone uncertainty.</summary>
    [Fact] public void C14_MixedXsd()
    {
        Assert.Equal(O.Equal,C.CompareXsd(L(),E()));
        Assert.Equal(O.Indeterminate,C.CompareXsd(L(Raw(tz:false),false),E()));
        Assert.Equal(O.Less,C.CompareXsd(L(Raw(tz:false),false),E("2000-01-04T03:04:05.123Z")));
    }

    /// <summary>Checks C15 mixed keys obey symmetry, transitivity and equal hashes.</summary>
    [Fact] public void C15_KeyLaws()
    {
        var a=L();var b=E();var c=E("2000-01-02T04:04:05.123+01:00",XdmExactCalendarKind.DateTimeStamp);
        Assert.True(C.SameKey(a,b));Assert.True(C.SameKey(b,a));Assert.True(C.SameKey(b,c));Assert.True(C.SameKey(a,c));
        Assert.Equal(C.GetSameKeyHashCode(a),C.GetSameKeyHashCode(c));
        Assert.False(C.SameKey(a,L(Raw(tz:false),false)));Assert.False(C.SameKey(a,E("2000-01-02T03:04:05.123000001Z")));
        Assert.True(C.SameKey(T(Raw(1980,1,1)),T(Raw(2026,11,25))));
        Assert.False(C.SameKey(T(Raw(hour:23,minute:0,second:0,ms:0,zone:-180)),E("02:00:00Z",XdmExactCalendarKind.Time)));
        var random=new Random(20261125);
        for(int i=0;i<500;i++)
        {
            var dto=new DateTimeOffset(random.Next(2,9998),random.Next(1,13),random.Next(1,28),random.Next(24),random.Next(60),random.Next(60),random.Next(1000),TimeSpan.FromMinutes(random.Next(-56,57)*15));
            var legacy=XdmValue.FromDateTime(dto);var expected=XdmExactCalendarValue.FromDateTimeOffset(dto);
            var shifted=XdmValue.FromExactCalendar(XdmExactCalendarValue.FromDateTimeOffset(dto.ToOffset(TimeSpan.Zero)));
            Assert.Equal(expected,C.Read(legacy));Assert.True(C.SameKey(legacy,shifted));Assert.Equal(C.GetSameKeyHashCode(legacy),C.GetSameKeyHashCode(shifted));
            Assert.Equal(-1,C.CompareXPath(legacy,XdmValue.FromDateTime(dto.AddMilliseconds(1)),0));
        }
    }

    /// <summary>Checks C16 an explicit CLR dictionary can retrieve a mixed representation key.</summary>
    [Fact] public void C16_ExplicitDictionary()
    {
        var map=new Dictionary<XdmValue,string>(XdmCalendarInteropKeyComparer.Instance){{L(),"found"}};
        Assert.Equal("found",map[E()]);Assert.False(map.TryAdd(E(),"duplicate"));
        Assert.True(map.TryAdd(E("03:04:05.123Z",XdmExactCalendarKind.Time),"time"));Assert.Equal(2,map.Count);
        Assert.Throws<ArgumentException>(()=>map.ContainsKey(XdmValue.FromString("2000-01-02T03:04:05.123Z")));
    }

    /// <summary>Checks C17 default and wrong operands refuse and time remains a separate family.</summary>
    [Fact] public void C17_FamilyRefusal()
    {
        Assert.Throws<ArgumentException>(()=>C.Read(default));Assert.Throws<ArgumentException>(()=>C.Read(XdmValue.FromInteger(1)));
        Assert.Throws<ArgumentException>(()=>C.Read(XdmValue.FromDate(DateTimeOffset.UnixEpoch)));
        Assert.Throws<ArgumentException>(()=>C.CompareXPath(L(),T(),0));Assert.Throws<ArgumentException>(()=>C.CompareXsd(T(),L()));Assert.False(C.SameKey(L(),T()));
        Assert.Throws<ArgumentException>(()=>C.GetSameKeyHashCode(default));
    }

    /// <summary>Checks C18 existing getter and default map guard behavior remains intact.</summary>
    [Fact] public void C18_LegacyGuards()
    {
        var legacy=L();var raw=legacy.DateTimeXPathValue;_ = C.Read(legacy);Assert.Equal(raw.Millisecond,legacy.DateTimeXPathValue.Millisecond);Assert.False(legacy.IsExactCalendar);
        Assert.Throws<NotSupportedException>(()=>E().DateTimeXPathValue);Assert.Throws<NotSupportedException>(()=>XdmValueEqualityComparer.Instance.GetHashCode(E()));
        Assert.Throws<NotSupportedException>(()=>XdmValueEqualityComparer.Instance.Equals(E(),legacy));
    }

    /// <summary>Checks C19 projection and equivalent reconstructed atomics carry no authority.</summary>
    [Fact] public void C19_PlainNoAuthority()
    {
        var legacy=L();var exact=XdmValue.FromExactCalendar(C.Read(legacy),"Q{urn:x}Pretend");
        Assert.True(C.SameKey(legacy,exact));Assert.False(SchemaAtomics.TryGetProvenance(legacy,out _));Assert.False(SchemaAtomics.TryGetProvenance(exact,out _));
        Assert.False(legacy.IsExactCalendar);Assert.Equal("Q{urn:x}Pretend",exact.UserSchemaTypeName);
    }
}
