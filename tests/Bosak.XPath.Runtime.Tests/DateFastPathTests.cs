// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 November 2026
// PURPOSE              : Verifies valid ASCII date parsing and unchanged lexical fallback behavior
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Reflection;
using System.Globalization;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;
namespace Bosak.XPath.Runtime.Tests;
/// <summary>Verifies the bounded ASCII date fast path preserves existing parser and cast behavior.</summary>
public sealed class DateFastPathTests
{
    private delegate bool Parse(string? text,out XPathDateTime value,out bool timezone);
    private static readonly Type Vm=typeof(EvaluationContext).Assembly.GetType("Bosak.XPath.Runtime.Vm.VmEngine",true)!;
    private static readonly Parse Parser=Vm.GetMethod("TryParseXPathDate",BindingFlags.NonPublic|BindingFlags.Static)!.CreateDelegate<Parse>();
    private static readonly Parse Fast=Vm.GetMethod("TryParseValidAsciiDate",BindingFlags.NonPublic|BindingFlags.Static)!.CreateDelegate<Parse>();
    /// <summary>Checks valid date/calendar/timezone fields on the direct fast path.</summary>
    [Fact]public void ValidDates_PreserveFields()
    {
        foreach(var c in new[]{("2026-11-16",2026L,11,16,0,false),("0000-02-29Z",0L,2,29,0,true),("2000-02-29+14:00",2000L,2,29,840,true),("0400-02-29-14:00",400L,2,29,-840,true),("9999-12-31+05:30",9999L,12,31,330,true),("2026-11-16-00:00",2026L,11,16,0,true)})
        {
            Assert.True(Fast(c.Item1,out var v,out var tz));Assert.True(Parser(c.Item1,out var full,out var fullTz));Assert.Equal(v,full);Assert.Equal(tz,fullTz);
            Assert.Equal(c.Item2,v.Year);Assert.Equal(c.Item3,v.Month);Assert.Equal(c.Item4,v.Day);Assert.Equal(c.Item5,v.TimezoneOffsetMinutes);Assert.Equal(c.Item6,tz);Assert.Equal(tz,v.HasTimezone);
            Assert.Equal(0,v.Hour+v.Minute+v.Second+v.Millisecond);
        }
    }
    /// <summary>Checks invalid forms preserve the legacy failure and out-parameter state.</summary>
    [Fact]public void InvalidDates_PreserveOutState()
    {
        foreach(var c in new[]{("2026-11-16+14:01",true),("2026-11-16+00:60",true),("2026-13-16Z",false),("1900-02-29Z",false),("2026-11-16z",false),("+2026-11-16",false),("00000-01-01",false)})
        {Assert.False(Fast(c.Item1,out _,out _));Assert.False(Parser(c.Item1,out var v,out var tz));Assert.Equal(default(XPathDateTime),v);Assert.Equal(c.Item2,tz);}
        Assert.Throws<ArgumentNullException>(()=>Parser(null,out _,out _));
        Assert.Throws<FormatException>(()=>Parser("\u0662\u0660\u0662\u0666-11-16",out _,out _));
    }
    /// <summary>Checks signed/expanded years and legacy final-newline acceptance remain fallback-only.</summary>
    [Fact]public void FallbackOnlyDates_KeepExistingAcceptanceAndErrors()
    {
        foreach(var c in new[]{("10000-02-29",10000L),("-0004-02-29",-4L),("2026-11-16\n",2026L)})
        {Assert.False(Fast(c.Item1,out _,out _));Assert.True(Parser(c.Item1,out var v,out _));Assert.Equal(c.Item2,v.Year);}
        Assert.Equal("FODT0001: The year '2147483648' is outside the supported range.",Assert.Throws<InvalidOperationException>(()=>Parser("2147483648-01-01",out _,out _)).Message);
        Assert.Throws<OverflowException>(()=>Parser("9223372036854775808-01-01",out _,out _));
    }
    /// <summary>Checks actual public casts and castable calls, including caller whitespace normalization.</summary>
    [Fact]public void PublicCasts_PreserveAcceptanceAndErrors()
    {
        foreach(var c in new[]{("2026-11-16",true),("0000-02-29Z",true),(" 2026-11-16+05:30 ",true),("10000-01-01",true),("1900-02-29",false),("2026-11-16+14:01",false),("2026-11-16z",false)})
        {
            var ctx=new EvaluationContext().WithVariable("s",XdmValue.FromString(c.Item1));Assert.Equal(c.Item2,XPath31Expression.Compile("$s castable as xs:date").Evaluate(ctx).BooleanValue);
            if(c.Item2)Assert.Equal(XdmValueKind.Date,XPath31Expression.Compile("xs:date($s)").Evaluate(ctx).Kind);
            else Assert.Contains("FORG0001",Assert.ThrowsAny<Exception>(()=>XPath31Expression.Compile("xs:date($s)").Evaluate(ctx)).Message);
        }
    }
    /// <summary>Checks locale independence and concurrent use of the shared parser.</summary>
    [Fact]public void LocaleAndConcurrency_PreserveDateFields()
    {
        var saved=CultureInfo.CurrentCulture;
        try{foreach(var locale in new[]{"fr-FR","tr-TR","ar-SA"}){CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(locale);Assert.True(Parser("0000-02-29-14:00",out var v,out var tz));Assert.Equal(0L,v.Year);Assert.Equal(-840,v.TimezoneOffsetMinutes);Assert.True(tz);}}finally{CultureInfo.CurrentCulture=saved;}
        Parallel.For(0,256,_=>{Assert.True(Parser("2026-11-16+05:30",out var v,out var tz));Assert.Equal(330,v.TimezoneOffsetMinutes);Assert.True(tz);});
    }
    /// <summary>Checks fixed fast-path ceilings and exact baseline fallback costs in an isolated worker.</summary>
    [Fact]public void IsolatedAllocations_RespectRecordedScope()
    {
        foreach(var row in AllocationProbeRunner.Run("dates"))
        {
            if(!row.Key.StartsWith("fallback-",StringComparison.Ordinal))Assert.InRange(row.Value,0L,16L*100000);
            else Assert.Equal((row.Key=="fallback-extended"?704L:row.Key=="fallback-invalid-timezone"?736L:0L)*100000,row.Value);
        }
    }
}
