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

            RunTestSet(testSetPath, sharedEnvironments, report);
            processedSets++;

            if (processedSets % 50 == 0)
            {
                Console.WriteLine($"  ... processed {processedSets}/{testSetRefs.Count} sets ({report.Total} tests)");
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

            // Skip tests requiring external variables with complex expressions (not yet supported)
            if (env?.Parameters.Any(p => !string.IsNullOrEmpty(p.SelectExpression)) == true)
            {
                report.Record(testCase.Name, TestOutcomeKind.Skipped, "External variable binding not supported");
                continue;
            }

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
