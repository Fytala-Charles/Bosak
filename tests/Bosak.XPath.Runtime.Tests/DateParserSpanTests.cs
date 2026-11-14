// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 November 2026
// PURPOSE              : Verifies date parsing values and failures after span allocation changes.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 14-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Reflection;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Verifies date parsing values and failures after span allocation changes.</summary>
public sealed class DateParserSpanTests
{
    private delegate bool Parse(string text, out XPathDateTime value, out bool timezone);
    private static readonly Parse Parser = typeof(EvaluationContext).Assembly
        .GetType("Bosak.XPath.Runtime.Vm.VmEngine", true)!
        .GetMethod("TryParseXPathDate", BindingFlags.NonPublic | BindingFlags.Static)!.CreateDelegate<Parse>();

    /// <summary>Checks parsed values across calendar and timezone boundaries.</summary>
    [Fact]
    public void DateValues_PreserveCalendarAndTimezoneFields()
    {
        foreach (var c in new (string text, long year, int month, int day, int offset, bool zone)[] {
            ("2026-11-14",2026,11,14,0,false), ("0000-02-29Z",0,2,29,0,true),
            ("10000-02-29+14:00",10000,2,29,840,true), ("-0004-02-29-14:00",-4,2,29,-840,true),
            ("2000-02-29+05:30",2000,2,29,330,true), ("2026-11-14-00:00",2026,11,14,0,true) })
        {
            Assert.True(Parser(c.text,out var v,out var tz));
            Assert.Equal(c.year,v.Year); Assert.Equal(c.month,v.Month); Assert.Equal(c.day,v.Day);
            Assert.Equal(c.offset,v.TimezoneOffsetMinutes); Assert.Equal(c.zone,tz); Assert.Equal(tz,v.HasTimezone);
            Assert.Equal(0,v.Hour); Assert.Equal(0,v.Minute); Assert.Equal(0,v.Second); Assert.Equal(0,v.Millisecond);
        }
    }

    /// <summary>Checks invalid lexical, calendar and timezone forms.</summary>
    [Fact]
    public void InvalidDates_KeepExistingRefusalBehavior()
    {
        foreach(var text in new[]{"not-a-date","2026-11-14T12:00:00Z","+2026-11-14","00000-01-01",
            "2026-13-14","1900-02-29","2026-04-31","2026-11-14+14:01","2026-11-14+00:60","2026-11-14z"})
            Assert.False(Parser(text,out _,out _));
    }

    /// <summary>Checks bounded-year diagnostics and numeric overflow remain distinct.</summary>
    [Fact]
    public void YearOverflow_PreservesExceptionTypeAndDiagnostic()
    {
        foreach(var year in new[]{"2147483648","-2147483649"})
            Assert.Equal($"FODT0001: The year '{year}' is outside the supported range.",
                Assert.Throws<InvalidOperationException>(()=>Parser(year+"-01-01",out _,out _)).Message);
        Assert.Throws<OverflowException>(()=>Parser("999999999999999999999-01-01",out _,out _));
    }
}
