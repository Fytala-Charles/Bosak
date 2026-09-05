// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 mei 2026
// PURPOSE              : Entry point for the W3C QT3 XPath 3.1 conformance test harness.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 22-05-2026     | Added optional test-set name filter argument                                             |
//                      | Charles Korthout | 0.3   | 15-07-2026     | Absolutize the suite path so relative invocations yield file:/// document URIs           |
//                      | Charles Korthout | 0.4   | 19-07-2026     | Added optional test-name filter argument for targeted cbcl-style runs                    |
//                      | Charles Korthout | 0.5   | 26-07-2026     | Run the conformance suite on a dedicated 512MB-stack thread for deep recursion (function-declaration-007, numberformat121) |
//                      | Charles Korthout | 0.6   | 07-08-2026     | Set QTTEST/QTTEST2/QTTESTEMPTY process variables for the fn-environment-variable test sets |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 18-08-2026     | BOSAK_QT3_DUMP_SKIPS env var writes per-test skip details grouped by reason           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 03-09-2026     | Warning-free build: CS8602 null-conditional report access                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Diagnostics;
using System.Xml.Linq;

namespace Bosak.XPath.Conformance;

internal class Program
{
    private static readonly XNamespace CatalogNs = "http://www.w3.org/2010/09/qt-fots-catalog";

    static int Main(string[] args)
    {
        // The fn-environment-variable / fn-available-environment-variables test sets
        // require these process variables (their catalog comments specify exactly these
        // values); set them as the documented `QTTEST="42" QTTEST2="other" QTTESTEMPTY=
        // ./test-harness` invocation would.
        Environment.SetEnvironmentVariable("QTTEST", "42");
        Environment.SetEnvironmentVariable("QTTEST2", "other");
        Environment.SetEnvironmentVariable("QTTESTEMPTY", "");

        string suitePath = args.Length > 0 ? args[0] : "tests/qt3tests";
        // Absolutize so document URIs derived from suite files are stable file:/// URIs
        // (relative paths triggered UriFormatException in XDocumentProvider.LoadXml).
        suitePath = Path.GetFullPath(suitePath);
        string? setFilter = args.Length > 1 ? args[1] : null;
        string? testFilter = args.Length > 2 ? args[2] : null;
        string catalogPath = Path.Combine(suitePath, "catalog.xml");

        if (!File.Exists(catalogPath))
        {
            Console.Error.WriteLine($"Catalog not found: {catalogPath}");
            Console.Error.WriteLine("Usage: Bosak.XPath.Conformance [path-to-qt3tests] [test-set-filter] [test-name-filter]");
            return 1;
        }

        Console.WriteLine($"Bosak XPath 3.1 Conformance Harness");
        Console.WriteLine($"Catalog: {catalogPath}");
        if (setFilter is not null)
            Console.WriteLine($"Test-set filter:  {setFilter}");
        if (testFilter is not null)
            Console.WriteLine($"Test-name filter: {testFilter}");
        Console.WriteLine();

        var stopwatch = Stopwatch.StartNew();
        var runner = new ConformanceRunner(suitePath, setFilter, testFilter);
        // Run on a dedicated thread with a large stack: the recursive interpreter needs
        // deep frames for recursive user functions (function-declaration-007 recurses 100+;
        // fn-format-number numberformat121/122 recurse 5,000 deep through the user-function
        // dispatch, which costs several interpreter frames per level).
        TestReport? report = null;
        var worker = new Thread(() => report = runner.Run(), maxStackSize: 512 * 1024 * 1024)
        {
            IsBackground = true,
            Name = "conformance-runner"
        };
        worker.Start();
        worker.Join();
        stopwatch.Stop();

        report?.PrintSummary();
        Console.WriteLine();
        Console.WriteLine($"Elapsed: {stopwatch.Elapsed.TotalSeconds:F2}s");

        // Optional per-test skip dump for skip-cluster analysis.
        var dumpSkipsPath = Environment.GetEnvironmentVariable("BOSAK_QT3_DUMP_SKIPS");
        if (!string.IsNullOrEmpty(dumpSkipsPath))
        {
            report?.DumpSkips(dumpSkipsPath);
            Console.WriteLine($"Skip details written to {dumpSkipsPath}");
        }

        return report is { Failed: > 0 } ? 2 : 0;
    }
}
