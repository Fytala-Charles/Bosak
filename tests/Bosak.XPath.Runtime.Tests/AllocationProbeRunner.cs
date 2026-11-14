// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 November 2026
// PURPOSE              : Runs allocation regression workloads in a bounded standalone process
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
using System.Diagnostics;
using System.Text.Json;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Starts a bounded allocation worker and returns its complete measured rows.</summary>
internal static class AllocationProbeRunner
{
    internal static Dictionary<string, long> Run(string workload)
    {
        var worker = Path.Combine(AppContext.BaseDirectory, "allocation-checks", "Bosak.XPath.AllocationChecks.dll");
        Assert.True(File.Exists(worker), "Allocation worker was not copied to the test output.");
        var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        start.ArgumentList.Add(worker); start.ArgumentList.Add(workload);
        // Scoped to the worker: cap its managed heap, not the parent or machine.
        start.Environment["DOTNET_GCHeapHardLimit"] = "0x10000000";
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Allocation worker did not start.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(60000))
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
            throw new TimeoutException("Allocation worker exceeded its 60-second watchdog.");
        }
        var output = stdout.GetAwaiter().GetResult(); var error = stderr.GetAwaiter().GetResult();
        Assert.True(process.ExitCode == 0, $"Allocation worker exited {process.ExitCode}: {error}");
        using var report = JsonDocument.Parse(output);
        Assert.Equal(1, report.RootElement.GetProperty("protocol").GetInt32());
        Assert.True(report.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(workload, report.RootElement.GetProperty("workload").GetString());
        Assert.NotEqual(Environment.ProcessId, report.RootElement.GetProperty("processId").GetInt32());
        Assert.Equal(100000, report.RootElement.GetProperty("iterations").GetInt32());
        Assert.Equal(10000, report.RootElement.GetProperty("warmup").GetInt32());
        Assert.Equal(typeof(EvaluationContext).Assembly.GetName().Version!.ToString(), report.RootElement.GetProperty("engine").GetString());
        var items = report.RootElement.GetProperty("rows").EnumerateArray().ToArray();
        var expectedComparisons = new Dictionary<string, bool> { ["="] = false, ["!="] = true, ["<"] = true,
            ["<="] = true, [">"] = false, [">="] = false };
        var expectedKinds = new Dictionary<string, string> { ["integer"] = "Integer", ["string"] = "String",
            ["boolean"] = "Boolean", ["decimal"] = "Decimal", ["double"] = "Double" };
        foreach (var item in items)
        {
            var name = item.GetProperty("name").GetString()!;
            if (workload == "comparisons")
            {
                Assert.True(expectedComparisons.ContainsKey(name));
                Assert.Equal(expectedComparisons[name], item.GetProperty("boolean").GetBoolean());
            }
            else if (name.StartsWith("match/", StringComparison.Ordinal))
            {
                Assert.True(expectedKinds.ContainsKey(name[6..]));
                Assert.True(item.GetProperty("boolean").GetBoolean());
            }
            else
            {
                Assert.StartsWith("convert/", name);
                Assert.Equal(expectedKinds[name[8..]], item.GetProperty("kind").GetString());
            }
        }
        var rows = items.ToDictionary(x => x.GetProperty("name").GetString()!, x => x.GetProperty("bytes").GetInt64());
        Assert.Equal(workload == "comparisons" ? 6 : 10, rows.Count);
        return rows;
    }
}
