// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 oktober 2026
// PURPOSE              : Entry point for the qt4tests XPath 4.0 conformance test harness.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-10-2026     | Creation (REQ-123 4.0-Exp S2): qt4tests wired as the 4.0 conformance gate                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 09-10-2026     | Exit code driven by gated-set failures (GatedSets); per-set P/F/S stats + exploratory     |
//                      |                  |       |                | failure count printed for the baseline/gate workflow                                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Diagnostics;

namespace Bosak.XPath.Conformance;

internal class Program
{
    static int Main(string[] args)
    {
        // The fn-environment-variable / fn-available-environment-variables test sets
        // require these process variables (carried over from the QT3 harness defaults).
        Environment.SetEnvironmentVariable("QTTEST", "42");
        Environment.SetEnvironmentVariable("QTTEST2", "other");
        Environment.SetEnvironmentVariable("QTTESTEMPTY", "");

        string suitePath = args.Length > 0 ? args[0] : "tests/qt4tests";
        // Absolutize so document URIs derived from suite files are stable file:/// URIs
        // (relative paths triggered UriFormatException in XDocumentProvider.LoadFile).
        suitePath = Path.GetFullPath(suitePath);
        string? setFilter = args.Length > 1 ? args[1] : null;
        string? testFilter = args.Length > 2 ? args[2] : null;
        string catalogPath = Path.Combine(suitePath, "catalog.xml");

        if (!File.Exists(catalogPath))
        {
            Console.Error.WriteLine($"Catalog not found: {catalogPath}");
            Console.Error.WriteLine("Usage: Bosak.XPath.Conformance40 [path-to-qt4tests] [test-set-filter] [test-name-filter]");
            return 1;
        }

        Console.WriteLine($"Bosak XPath 4.0 Conformance Harness");
        Console.WriteLine($"Catalog: {catalogPath}");
        if (setFilter is not null)
            Console.WriteLine($"Test-set filter:  {setFilter}");
        if (testFilter is not null)
            Console.WriteLine($"Test-name filter: {testFilter}");
        Console.WriteLine();

        var stopwatch = Stopwatch.StartNew();
        var runner = new ConformanceRunner40(suitePath, setFilter, testFilter);
        // Run on a dedicated thread with a large stack: the recursive interpreter needs
        // deep frames for recursive user functions (same rationale as the QT3 harness).
        TestReport? report = null;
        var worker = new Thread(() => report = runner.Run(), maxStackSize: 512 * 1024 * 1024)
        {
            IsBackground = true,
            Name = "conformance40-runner"
        };
        worker.Start();
        worker.Join();
        stopwatch.Stop();

        report?.PrintSummary();
        Console.WriteLine();
        Console.WriteLine($"Gated failures (fail the run): {runner.GatedFailures}   Exploratory failures (reported only): {runner.TotalFailures - runner.GatedFailures}");
        Console.WriteLine($"Experimental-level (XPath40Experimental) passes: {runner.ExperimentalPasses}");
        Console.WriteLine($"Elapsed: {stopwatch.Elapsed.TotalSeconds:F2}s");

        // Optional per-test skip dump for skip-cluster analysis.
        var dumpSkipsPath = Environment.GetEnvironmentVariable("BOSAK_QT4_DUMP_SKIPS");
        if (!string.IsNullOrEmpty(dumpSkipsPath))
        {
            report?.DumpSkips(dumpSkipsPath);
            Console.WriteLine($"Skip details written to {dumpSkipsPath}");
        }

        return runner.GatedFailures > 0 ? 2 : 0;
    }
}
