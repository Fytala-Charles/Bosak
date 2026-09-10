// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 September 2026
// PURPOSE              : Entry point for the BenchmarkDotNet benchmark harness.
// SPECIAL NOTES        : Benchmark harness for the Bosak XPath 3.1 implementation; not shipped.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using BenchmarkDotNet.Running;

namespace Bosak.Benchmarks;

/// <summary>
/// Entry point for the benchmark harness. Run with
/// <c>dotnet run -c Release --project benchmarks/Bosak.Benchmarks -- --filter '*'</c>.
/// </summary>
public static class Program
{
    /// <summary>Runs the BenchmarkDotNet switcher over all benchmarks in this assembly.</summary>
    /// <param name="args">BenchmarkDotNet command-line arguments (e.g. <c>--filter</c>, <c>--job</c>).</param>
    public static void Main(string[] args)
        => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
