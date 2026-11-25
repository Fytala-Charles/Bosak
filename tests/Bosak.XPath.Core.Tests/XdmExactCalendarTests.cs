// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Verifies exact calendar contracts, boundaries and lossless interoperability.
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
using System.Numerics;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Core.Tests;

/// <summary>Verifies twenty exact calendar foundation contracts.</summary>
public sealed class XdmExactCalendarTests
{
    private static XdmExactSecond S(string text) => XdmExactSecond.Parse(text, 256, 256);
    private static XdmExactCalendarValue P(string text, XdmExactCalendarKind kind = XdmExactCalendarKind.DateTime) => XdmExactCalendarValue.Parse(text, kind, 256, 256);
    [Fact]
    /// <summary>Verifies C01 SuppliedCapacityBeforeNormalization.</summary>
    public void C01_SuppliedCapacityBeforeNormalization()
    {
        Assert.Equal(1, new XdmExactSecond(1000, 3, 4, 3).Coefficient);
        Assert.Throws<NotSupportedException>(() => new XdmExactSecond(1000, 3, 3, 3));
        Assert.Throws<NotSupportedException>(() => new XdmExactSecond(0, 4, 4, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => new XdmExactSecond(0, 0, 257, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => new XdmExactSecond(0, 0, 4, 257));
    }
    [Fact]
    /// <summary>Verifies C02 SecondRange.</summary>
    public void C02_SecondRange()
    {
        Assert.Equal("59.999", S("59.999").ToInvariantString(20));
        Assert.Throws<ArgumentOutOfRangeException>(() => S("60"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new XdmExactSecond(-1, 1, 2, 2));
    }
    [Fact]
    /// <summary>Verifies C03 NormalizationAndDefault.</summary>
    public void C03_NormalizationAndDefault()
    {
        Assert.Equal(new BigInteger(123), S("01.23000").Coefficient); Assert.Equal(2, S("01.23000").Scale);
        Assert.Equal(default, S("00.000")); Assert.Equal(0, S("00.000").Scale);
        Assert.Equal("00", default(XdmExactSecond).ToInvariantString(2));
    }
    [Theory]
    [InlineData("03", true)] [InlineData("03.001", true)] [InlineData("3", false)] [InlineData("03.", false)] [InlineData("+03", false)] [InlineData("03e1", false)] [InlineData("03,1", false)] [InlineData(" 03", false)] [InlineData("\u0660\u0663", false)]
    /// <summary>Verifies C04 SecondLexical.</summary>
    /// <param name="token">Contract test input.</param>
    /// <param name="valid">Contract test input.</param>
    public void C04_SecondLexical(string token, bool valid)
    {
        if (valid) Assert.True(S(token).Coefficient >= 0); else Assert.Throws<FormatException>(() => S(token));
    }
    [Fact]
    /// <summary>Verifies C05 FractionOutputBounds.</summary>
    public void C05_FractionOutputBounds()
    {
        Assert.Equal("00.0001", S("00.000100").ToInvariantString(7));
        Assert.Throws<NotSupportedException>(() => S("00.0001").ToInvariantString(6));
        Assert.Throws<NotSupportedException>(() => S("00").ToInvariantString(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => S("00").ToInvariantString(0));
    }
    [Fact]
    /// <summary>Verifies C06 FractionEqualityHash.</summary>
    public void C06_FractionEqualityHash()
    {
        Assert.Equal(S("01.23"), S("01.2300")); Assert.Equal(S("01.23").GetHashCode(), S("01.2300").GetHashCode());
        Assert.NotEqual(S("01.234567"), S("01.234568"));
        Assert.Equal(2, new HashSet<XdmExactSecond> { S("01.23"), S("01.230"), S("01.231") }.Count);
    }
    [Fact]
    /// <summary>Verifies C07 FractionOrdering.</summary>
    public void C07_FractionOrdering()
    {
        Assert.True(S("01.234567").CompareTo(S("01.234568")) < 0);
        Assert.True(S("59.9").CompareTo(S("00.0000001")) > 0);
        Assert.Equal(0, S("01.0").CompareTo(S("01")));
    }
    [Fact]
    /// <summary>Verifies C08 ExactTicks.</summary>
    public void C08_ExactTicks()
    {
        Assert.Equal(12345678, S("01.2345678").ToExactUnits(7));
        Assert.Equal(12300000, S("01.23000000").ToExactUnits(7));
        Assert.Throws<NotSupportedException>(() => S("01.23456789").ToExactUnits(7));
        Assert.Throws<ArgumentOutOfRangeException>(() => S("01").ToExactUnits(6));
    }
    [Fact]
    /// <summary>Verifies C09 ExactMilliseconds.</summary>
    public void C09_ExactMilliseconds()
    {
        Assert.Equal(59999, S("59.999").ToExactUnits(3));
        Assert.Throws<NotSupportedException>(() => S("00.0001").ToExactUnits(3));
        Assert.Equal(0, default(XdmExactSecond).ToExactUnits(3));
    }
    [Theory]
    [InlineData("0000-02-29T00:00:00", true)] [InlineData("-0400-02-29T00:00:00", true)] [InlineData("2000-02-29T00:00:00", true)] [InlineData("1900-02-29T00:00:00", false)] [InlineData("2026-04-31T00:00:00", false)] [InlineData("2026-00-01T00:00:00", false)] [InlineData("2026-01-00T00:00:00", false)]
    /// <summary>Verifies C10 GregorianCalendar.</summary>
    /// <param name="text">Contract test input.</param>
    /// <param name="valid">Contract test input.</param>
    public void C10_GregorianCalendar(string text, bool valid)
    {
        if (valid) Assert.Equal(29, P(text).Day); else Assert.Throws<ArgumentOutOfRangeException>(() => P(text));
    }
    [Fact]
    /// <summary>Verifies C11 ExtendedYearCapacity.</summary>
    public void C11_ExtendedYearCapacity()
    {
        Assert.Equal("10000-01-01T00:00:00", P("10000-01-01T00:00:00").ToInvariantString(100));
        Assert.Equal(long.MaxValue, P("9223372036854775807-01-01T00:00:00").Year);
        Assert.Throws<NotSupportedException>(() => P("9223372036854775808-01-01T00:00:00"));
        Assert.Throws<NotSupportedException>(() => P("9223372036854775807-12-31T24:00:00"));
        Assert.Throws<ArgumentOutOfRangeException>(() => P("-9223372036854775808-01-01T00:00:00"));
    }
    [Theory]
    [InlineData("23:59:59.999", true)] [InlineData("12:60:00", false)] [InlineData("12:00:60", false)] [InlineData("25:00:00", false)] [InlineData("24:00:00.001", false)]
    /// <summary>Verifies C12 TimeFields.</summary>
    /// <param name="text">Contract test input.</param>
    /// <param name="valid">Contract test input.</param>
    public void C12_TimeFields(string text, bool valid)
    {
        if (valid) Assert.Equal(23, P(text, XdmExactCalendarKind.Time).Hour); else Assert.Throws<ArgumentOutOfRangeException>(() => P(text, XdmExactCalendarKind.Time));
    }
    [Theory]
    [InlineData("Z", true)] [InlineData("+14:00", true)] [InlineData("-14:00", true)] [InlineData("+14:01", false)] [InlineData("+13:60", false)] [InlineData("+15:00", false)]
    /// <summary>Verifies C13 TimezoneBounds.</summary>
    /// <param name="zone">Contract test input.</param>
    /// <param name="valid">Contract test input.</param>
    public void C13_TimezoneBounds(string zone, bool valid)
    {
        if (valid) Assert.True(P("2000-01-01T00:00:00" + zone).HasTimezone); else Assert.Throws<FormatException>(() => P("2000-01-01T00:00:00" + zone));
        Assert.False(P("2000-01-01T00:00:00").HasTimezone);
    }
    [Theory]
    [InlineData("2000-02-28T24:00:00", "2000-02-29T00:00:00")]
    [InlineData("9999-12-31T24:00:00.000Z", "10000-01-01T00:00:00Z")]
    [InlineData("-0001-12-31T24:00:00", "0000-01-01T00:00:00")]
    /// <summary>Verifies C14 EndOfDay.</summary>
    /// <param name="input">Contract test input.</param>
    /// <param name="expected">Contract test input.</param>
    public void C14_EndOfDay(string input, string expected)
    {
        Assert.Equal(expected, P(input).ToInvariantString(100));
        Assert.Equal("00:00:00", P("24:00:00", XdmExactCalendarKind.Time).ToInvariantString(20));
        Assert.Throws<ArgumentOutOfRangeException>(() => P("2000-01-01T24:01:00"));
    }
    [Theory]
    [InlineData("\t2000-01-01T00:00:01.234567890123Z\r\n", true)] [InlineData("02000-01-01T00:00:00", false)] [InlineData("-0000-01-01T00:00:00", false)] [InlineData("+2000-01-01T00:00:00", false)] [InlineData("2000-01-01t00:00:00", false)] [InlineData("\u00a02000-01-01T00:00:00", false)] [InlineData("2000-01-01T00: 00:00", false)]
    /// <summary>Verifies C15 DateTimeLexical.</summary>
    /// <param name="text">Contract test input.</param>
    /// <param name="valid">Contract test input.</param>
    public void C15_DateTimeLexical(string text, bool valid)
    {
        if (valid) Assert.Equal(12, P(text).Second.Scale); else Assert.Throws<FormatException>(() => P(text));
    }
    [Fact]
    /// <summary>Verifies C16 TimeLexical.</summary>
    public void C16_TimeLexical()
    {
        var t = P("03:04:05.123456789+02:30", XdmExactCalendarKind.Time); Assert.Equal(0, t.Year); Assert.Equal(150, t.TimezoneOffsetMinutes);
        Assert.Throws<FormatException>(() => P("2000-01-01T03:04:05", XdmExactCalendarKind.Time));
        Assert.Throws<FormatException>(() => P("03:04:05"));
    }
    [Fact]
    /// <summary>Verifies C17 StampTimezone.</summary>
    public void C17_StampTimezone()
    {
        Assert.Equal(XdmExactCalendarKind.DateTimeStamp, P("2000-01-01T00:00:00Z", XdmExactCalendarKind.DateTimeStamp).Kind);
        Assert.Throws<ArgumentOutOfRangeException>(() => P("2000-01-01T00:00:00", XdmExactCalendarKind.DateTimeStamp));
    }
    [Fact]
    /// <summary>Verifies C18 InvariantFormattingAndCapacity.</summary>
    public void C18_InvariantFormattingAndCapacity()
    {
        var old = CultureInfo.CurrentCulture;
        try { CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR"); var v = P("2000-01-02T03:04:05.123456789-02:30"); string text = v.ToInvariantString(100); Assert.Equal("2000-01-02T03:04:05.123456789-02:30", text); Assert.Equal(v, P(text)); Assert.Throws<NotSupportedException>(() => v.ToInvariantString(text.Length - 1)); }
        finally { CultureInfo.CurrentCulture = old; }
        Assert.Throws<InvalidOperationException>(() => default(XdmExactCalendarValue).ToInvariantString(100));
        Assert.Throws<NotSupportedException>(() => P(new string(' ', 1025)));
    }
    [Fact]
    /// <summary>Verifies C19 InteropNeverTruncates.</summary>
    public void C19_InteropNeverTruncates()
    {
        var dto = new DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.FromHours(14)).AddTicks(1234567);
        Assert.Equal(dto, XdmExactCalendarValue.FromDateTimeOffset(dto).ToDateTimeOffsetExact());
        Assert.Equal(1234567, XdmExactCalendarValue.FromDateTimeOffset(dto).Second.ToExactUnits(7) % TimeSpan.TicksPerSecond);
        var ms = P("2000-01-02T03:04:05.123Z"); Assert.Equal(ms, XdmExactCalendarValue.FromXPathDateTime(ms.ToXPathDateTimeExact(), XdmExactCalendarKind.DateTime));
        Assert.Throws<NotSupportedException>(() => P("2000-01-02T03:04:05.123456789Z").ToDateTimeOffsetExact());
        Assert.Throws<NotSupportedException>(() => P("2000-01-02T03:04:05.1234567Z").ToXPathDateTimeExact());
        Assert.Throws<NotSupportedException>(() => P("2000-01-02T03:04:05").ToDateTimeOffsetExact());
        Assert.Throws<NotSupportedException>(() => P("0000-01-01T00:00:00Z").ToDateTimeOffsetExact());
        Assert.Throws<NotSupportedException>(() => P("0001-01-01T00:00:00+14:00").ToDateTimeOffsetExact());
        Assert.Throws<NotSupportedException>(() => P("00:00:00Z", XdmExactCalendarKind.Time).ToDateTimeOffsetExact());
    }
    [Fact]
    /// <summary>Verifies C20 ComponentIdentityNotInstantEquality.</summary>
    public void C20_ComponentIdentityNotInstantEquality()
    {
        var a = P("2000-01-01T00:00:01.230Z"); var b = P("2000-01-01T00:00:01.23+00:00"); Assert.Equal(a, b); Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, P("2000-01-01T00:00:01.23"));
        Assert.NotEqual(a, P("2000-01-01T01:00:01.23+01:00"));
        Assert.NotEqual(a, P("2000-01-01T00:00:01.23Z", XdmExactCalendarKind.DateTimeStamp));
    }
}
