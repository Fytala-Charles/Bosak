// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Verifies public exact calendar atomic payloads and narrowing refusals.
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
using System.Numerics;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Core.Tests;

/// <summary>Checks public atomic transport contracts C01 through C12.</summary>
public sealed class ExactCalendarAtomicTests
{
    private static XdmValue V(string text = "2000-01-02T03:04:05.123456789Z", XdmExactCalendarKind kind = XdmExactCalendarKind.DateTime, string? user = null) => XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse(text, kind, 256, 256), user);
    [Theory] [InlineData("05.123456789")] [InlineData("05.123456788")] [InlineData("05.0000000000000000001")]
    /// <summary>Verifies C01 DateTimeFactory.</summary>
    /// <param name="seconds">Contract input.</param>
    public void C01_DateTimeFactory(string seconds)
    {
        var value = V("2000-01-02T03:04:" + seconds + "Z"); Assert.Equal(XdmValueKind.DateTime, value.Kind); Assert.True(value.IsAtomic); Assert.True(value.IsExactCalendar); Assert.Equal("dateTime", value.SchemaTypeName); Assert.Equal(seconds, value.ExactCalendarValue.Second.ToInvariantString(100));
    }
    [Theory] [InlineData("03:04:05.123456789Z")] [InlineData("24:00:00")]
    /// <summary>Verifies C02 TimeFactory.</summary>
    /// <param name="text">Contract input.</param>
    public void C02_TimeFactory(string text)
    {
        var value = V(text, XdmExactCalendarKind.Time); Assert.Equal(XdmValueKind.Time, value.Kind); Assert.Equal("time", value.SchemaTypeName); Assert.Equal(XdmExactCalendarKind.Time, value.ExactCalendarValue.Kind); Assert.Equal(0, value.ExactCalendarValue.Year);
    }
    [Fact]
    /// <summary>Verifies C03 StampFactory.</summary>
    public void C03_StampFactory()
    {
        var value = V(kind: XdmExactCalendarKind.DateTimeStamp); Assert.Equal(XdmValueKind.DateTime, value.Kind); Assert.Equal("dateTimeStamp", value.SchemaTypeName); Assert.True(value.HasTimezone);
        Assert.Throws<ArgumentOutOfRangeException>(() => V("2000-01-02T03:04:05", XdmExactCalendarKind.DateTimeStamp));
    }
    [Fact]
    /// <summary>Verifies C04 DefaultRefusesAndPlainHasNoAuthority.</summary>
    public void C04_DefaultRefusesAndPlainHasNoAuthority()
    {
        Assert.Throws<ArgumentException>(() => XdmValue.FromExactCalendar(default)); Assert.False(SchemaAtomics.TryGetProvenance(V(), out _)); Assert.False(default(XdmValue).IsExactCalendar);
    }
    [Theory] [InlineData(null)] [InlineData("urn:customer:calendar")]
    /// <summary>Verifies C05 AnnotationsAreDescriptive.</summary>
    /// <param name="annotation">Contract input.</param>
    public void C05_AnnotationsAreDescriptive(string? annotation)
    {
        var value = V(user: annotation); Assert.Equal(annotation, value.UserSchemaTypeName); Assert.Equal("dateTime", value.SchemaTypeName); Assert.False(SchemaAtomics.TryGetProvenance(value, out _));
    }
    [Fact]
    /// <summary>Verifies C06 ExplicitAccessor.</summary>
    public void C06_ExplicitAccessor()
    {
        var value = (XdmValue)(object)V(); Assert.Equal(new BigInteger(5123456789), value.ExactCalendarValue.Second.Coefficient); Assert.Equal(9, value.ExactCalendarValue.Second.Scale);
        Assert.Throws<InvalidOperationException>(() => XdmValue.FromString("not a calendar").ExactCalendarValue);
        Assert.Throws<InvalidOperationException>(() => XdmValue.FromDateTime(DateTimeOffset.Now).ExactCalendarValue);
    }
    [Theory] [InlineData("2000-01-02T03:04:05.123456789Z")] [InlineData("2000-01-02T03:04:05.0000000000000000001-14:00")]
    /// <summary>Verifies C07 ExactSerialization.</summary>
    /// <param name="text">Contract input.</param>
    public void C07_ExactSerialization(string text)
    {
        Assert.Equal(text, V(text).ToString()); Assert.NotEqual(V("2000-01-02T03:04:05.123456789Z").ToString(), V("2000-01-02T03:04:05.123456788Z").ToString());
    }
    [Theory] [InlineData("", false, 0)] [InlineData("Z", true, 0)] [InlineData("+14:00", true, 840)] [InlineData("-14:00", true, -840)]
    /// <summary>Verifies C08 TimezonePresence.</summary>
    /// <param name="suffix">Contract input.</param>
    /// <param name="present">Contract input.</param>
    /// <param name="offset">Contract input.</param>
    public void C08_TimezonePresence(string suffix, bool present, int offset)
    {
        var value = V("2000-01-02T03:04:05.123456789" + suffix); Assert.Equal(present, value.HasTimezone); Assert.Equal(offset, value.ExactCalendarValue.TimezoneOffsetMinutes);
    }
    [Theory] [InlineData("0000")] [InlineData("-0400")] [InlineData("10000")] [InlineData("9223372036854775807")]
    /// <summary>Verifies C09 ExtendedYears.</summary>
    /// <param name="year">Contract input.</param>
    public void C09_ExtendedYears(string year)
    {
        var value = V(year + "-01-02T03:04:05.123456789Z"); Assert.Equal(long.Parse(year), value.ExactCalendarValue.Year); Assert.StartsWith(year, value.ToString());
    }
    [Theory] [InlineData("05")] [InlineData("05.123")] [InlineData("05.123456789")]
    /// <summary>Verifies C10 LegacyDtoRefusesAllExactPayloads.</summary>
    /// <param name="seconds">Contract input.</param>
    public void C10_LegacyDtoRefusesAllExactPayloads(string seconds)
    {
        Assert.Throws<NotSupportedException>(() => V("2000-01-02T03:04:" + seconds + "Z").DateTimeValue); Assert.Throws<NotSupportedException>(() => V("03:04:" + seconds + "Z", XdmExactCalendarKind.Time).TimeValue);
        Assert.Equal(123, XdmValue.FromDateTime(new DateTimeOffset(2000,1,2,3,4,5,123,TimeSpan.Zero)).DateTimeValue.Millisecond);
    }
    [Fact]
    /// <summary>Verifies C11 LegacyComponentsAndMapSemanticsRefuse.</summary>
    public void C11_LegacyComponentsAndMapSemanticsRefuse()
    {
        var dt = V(); var time = V("03:04:05.123456789", XdmExactCalendarKind.Time);
        Assert.Throws<NotSupportedException>(() => dt.DateTimeXPathValue); Assert.Throws<NotSupportedException>(() => time.TimeXPathValue);
        Assert.Throws<NotSupportedException>(() => XdmValueEqualityComparer.Instance.Equals(dt, XdmValue.FromInteger(1)));
        Assert.Throws<NotSupportedException>(() => XdmValueEqualityComparer.Instance.Equals(dt, dt));
        Assert.Throws<NotSupportedException>(() => XdmValueEqualityComparer.Instance.GetHashCode(dt));
        Assert.Throws<NotSupportedException>(() => XdmValueEqualityComparer.Instance.Equals(time, V()));
    }
    [Fact]
    /// <summary>Verifies C12 OrdinaryIdentityHashAndSequence.</summary>
    public void C12_OrdinaryIdentityHashAndSequence()
    {
        var a = V(); var b = V(); Assert.Equal(a, b); Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, V("2000-01-02T03:04:05.123456788Z")); Assert.NotEqual(a, V(user:"urn:other"));
        var sequence = XdmValue.FromSequence(XdmSequence.Singleton(a)); var cursor = sequence.SequenceValue!.GetEnumerator(); Assert.True(cursor.MoveNext()); Assert.Equal(a, cursor.Current); Assert.False(cursor.MoveNext());
    }
}
