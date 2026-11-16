// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 November 2026
// PURPOSE              : Measures valid date parsing allocations and unchanged fallback controls
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
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.XPath.AllocationChecks;
internal static class DateParsingAllocationWorkload
{
    private delegate bool Parse(string text,out XPathDateTime value,out bool timezone);
    private static XPathDateTime _sink;private static bool _parsed,_timezone;
    internal static object Run()
    {
        var parse=typeof(EvaluationContext).Assembly.GetType("Bosak.XPath.Runtime.Vm.VmEngine",true)!.GetMethod("TryParseXPathDate",BindingFlags.NonPublic|BindingFlags.Static)!.CreateDelegate<Parse>();
        var rows=new List<object>();
        foreach(var c in new[]{("plain","2026-11-16"),("utc","2026-11-16Z"),("offset","2026-11-16+05:30"),("fallback-extended","10000-01-01"),("fallback-invalid-timezone","2026-11-16+14:01"),("fallback-lowercase-z","2026-11-16z")})
        {
            for(int i=0;i<10000;i++)_parsed=parse(c.Item2,out _sink,out _timezone);
            long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100000;i++)_parsed=parse(c.Item2,out _sink,out _timezone);long bytes=GC.GetAllocatedBytesForCurrentThread()-start;
            rows.Add(new{name=c.Item1,bytes,parsed=_parsed,year=_sink.Year,timezone=_timezone,offsetMinutes=_sink.TimezoneOffsetMinutes});
        }
        GC.KeepAlive(_sink);
        return new{protocol=1,success=true,workload="dates",processId=Environment.ProcessId,framework=Environment.Version.ToString(),engine=typeof(EvaluationContext).Assembly.GetName().Version!.ToString(),iterations=100000,warmup=10000,rows};
    }
}
