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
        string suitePath = args.Length > 0 ? args[0] : "tests/qt3tests";
        // Absolutize so document URIs derived from suite files are stable file:/// URIs
        // (relative paths triggered UriFormatException in XDocumentProvider.LoadXml).
        suitePath = Path.GetFullPath(suitePath);
        string? filter = args.Length > 1 ? args[1] : null;
        string catalogPath = Path.Combine(suitePath, "catalog.xml");

        if (!File.Exists(catalogPath))
        {
            Console.Error.WriteLine($"Catalog not found: {catalogPath}");
            Console.Error.WriteLine("Usage: Bosak.XPath.Conformance [path-to-qt3tests] [filter]");
            return 1;
        }

        Console.WriteLine($"Bosak XPath 3.1 Conformance Harness");
        Console.WriteLine($"Catalog: {catalogPath}");
        if (filter is not null)
            Console.WriteLine($"Filter:  {filter}");
        Console.WriteLine();

        var stopwatch = Stopwatch.StartNew();
        var runner = new ConformanceRunner(suitePath, filter);
        var report = runner.Run();
        stopwatch.Stop();

        report.PrintSummary();
        Console.WriteLine();
        Console.WriteLine($"Elapsed: {stopwatch.Elapsed.TotalSeconds:F2}s");

        return report.Failed > 0 ? 2 : 0;
    }
}
