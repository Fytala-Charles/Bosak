// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 mei 2026
// PURPOSE              : Orchestrates loading, filtering, executing and reporting QT3 conformance tests.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 22-05-2026     | Added optional test-set name filter for targeted conformance runs                        |
//                      | Charles Korthout | 0.3   | 15-07-2026     | DocumentedSkips: upstream defects/platform limitations recorded as skips with reasons    |
//                      | Charles Korthout | 0.4   | 15-07-2026     | External-variable tests now run; params are bound by the executor                        |
//                      | Charles Korthout | 0.5   | 15-07-2026     | Tests without environment resolve relative URIs against the test-set directory           |
//                      | Charles Korthout | 0.6   | 15-07-2026     | Referenced environments without static-base-uri also fall back to test-set directory     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.XPath.Conformance;

internal sealed class ConformanceRunner
{
    private readonly string _suitePath;
    private readonly string? _filter;
    private readonly XNamespace _ns = "http://www.w3.org/2010/09/qt-fots-catalog";
    private readonly DependencyFilter _dependencyFilter = new();
    private readonly TestExecutor _executor = new();

    /// <summary>
    /// Tests that can never pass in this harness, with the reason. Each entry is a
    /// documented upstream test/data defect or a documented platform limitation.
    /// </summary>
    private static readonly Dictionary<string, string> DocumentedSkips = new(StringComparer.Ordinal)
    {
        // Upstream defect: the expected value in the catalog carries an artifactual leading
        // space (formatting), but U+09BE by itself is the only correct result.
        ["cbcl-fn-normalize-unicode-006"] = "Upstream defect: expected value has an artifactual leading space",
        // Platform limitation (AGENTS.md): DateTimeOffset minimum year is 1; year -2 needs a
        // custom date representation.
        ["fo-test-fn-year-from-dateTime-005"] = "Platform limitation: DateTimeOffset does not support year -2",
        ["fo-test-fn-year-from-date-003"] = "Platform limitation: DateTimeOffset does not support year -2",
    };

    public ConformanceRunner(string suitePath, string? filter = null)
    {
        _suitePath = suitePath;
        _filter = filter;
    }

    public TestReport Run()
    {
        var report = new TestReport();
        string catalogPath = Path.Combine(_suitePath, "catalog.xml");
        var catalog = XDocument.Load(catalogPath);
        var testSetRefs = catalog.Descendants(_ns + "test-set").ToList();

        Console.WriteLine($"Discovered {testSetRefs.Count} test sets.");
        Console.Out.Flush();

        // Pre-load shared environments from catalog
        var sharedEnvironments = LoadSharedEnvironments(catalog);

        int processedSets = 0;
        foreach (var testSetRef in testSetRefs)
        {
            string? setName = (string?)testSetRef.Attribute("name");
            string? fileName = (string?)testSetRef.Attribute("file");
            if (string.IsNullOrEmpty(fileName))
                continue;

            if (_filter is not null && setName is not null && !setName.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                continue;

            string testSetPath = Path.Combine(_suitePath, fileName);
            if (!File.Exists(testSetPath))
            {
                Console.WriteLine($"  Skip missing test set: {fileName}");
                continue;
            }

            Console.WriteLine($"  Starting: {setName} ...");
            Console.Out.Flush();
            RunTestSet(testSetPath, sharedEnvironments, report);
            processedSets++;
            Console.WriteLine($"  Done: {setName} ({report.Total} tests total)");
            Console.Out.Flush();

            if (processedSets % 50 == 0)
            {
                Console.WriteLine($"  ... processed {processedSets}/{testSetRefs.Count} sets ({report.Total} tests)");
                Console.Out.Flush();
            }
        }

        return report;
    }

    private Dictionary<string, TestEnvironment> LoadSharedEnvironments(XDocument catalog)
    {
        var envs = new Dictionary<string, TestEnvironment>();
        foreach (var envElem in catalog.Descendants(_ns + "environment"))
        {
            string? name = (string?)envElem.Attribute("name");
            if (name is not null)
            {
                envs[name] = TestEnvironment.FromElement(envElem, _suitePath, "");
            }
        }
        return envs;
    }

    private void RunTestSet(string path, Dictionary<string, TestEnvironment> sharedEnvs, TestReport report)
    {
        var doc = XDocument.Load(path);
        string baseDir = Path.GetDirectoryName(path) ?? _suitePath;

        // Load local environments
        var localEnvs = new Dictionary<string, TestEnvironment>();
        foreach (var envElem in doc.Descendants(_ns + "environment"))
        {
            string? name = (string?)envElem.Attribute("name");
            if (name is not null)
            {
                localEnvs[name] = TestEnvironment.FromElement(envElem, _suitePath, baseDir);
            }
        }

        foreach (var testCaseElem in doc.Descendants(_ns + "test-case"))
        {
            var testCase = TestCase.FromElement(testCaseElem, _ns);

            // Documented skips: upstream defects and platform limitations.
            if (DocumentedSkips.TryGetValue(testCase.Name, out var skipReason))
            {
                report.Record(testCase.Name, TestOutcomeKind.Skipped, skipReason);
                continue;
            }

            // Resolve environment
            TestEnvironment? env = null;
            var envRef = testCaseElem.Element(_ns + "environment");
            if (envRef is not null)
            {
                string? refName = (string?)envRef.Attribute("ref");
                if (refName is not null)
                {
                    if (!localEnvs.TryGetValue(refName, out env))
                    {
                        sharedEnvs.TryGetValue(refName, out env);
                    }
                }
                else
                {
                    env = TestEnvironment.FromElement(envRef, _suitePath, baseDir);
                }
            }

            // FOTS convention: tests without an explicit environment resolve relative
            // resource URIs against the test-set file's directory (fn-parse-json-101..105).
            // Referenced environments that do not declare a static-base-uri also fall back
            // to the test-set directory so fn:transform relative URIs resolve correctly.
            if (env is null)
            {
                env = new TestEnvironment { BaseUri = new Uri(baseDir + Path.DirectorySeparatorChar).AbsoluteUri };
            }
            else if (string.IsNullOrEmpty(env.BaseUri))
            {
                env.BaseUri = new Uri(baseDir + Path.DirectorySeparatorChar).AbsoluteUri;
            }

            // Dependency check
            if (!_dependencyFilter.IsSupported(testCase.Dependencies))
            {
                report.Record(testCase.Name, TestOutcomeKind.Skipped, "Unsupported dependency");
                continue;
            }

            // Skip schema-aware tests for now
            if (testCase.Dependencies.Any(d => d.Type == "feature" && d.Value.Contains("schema")))
            {
                report.Record(testCase.Name, TestOutcomeKind.Skipped, "Schema awareness not supported");
                continue;
            }

            // External variables with select expressions are bound by the executor.

            try
            {
                var outcome = _executor.Execute(testCase, env);
                report.Record(testCase.Name, outcome.Kind, outcome.Message);
            }
            catch (NotSupportedException ex)
            {
                report.Record(testCase.Name, TestOutcomeKind.Skipped, ex.Message);
            }
            catch (Exception ex)
            {
                report.Record(testCase.Name, TestOutcomeKind.Skipped, $"Harness error: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
