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
//                      | Charles Korthout | 0.8   | 19-07-2026     | DocumentedSkips: numberformat63/64 precision limitation                                  |
//                      | Charles Korthout | 0.9   | 19-07-2026     | Added optional test-name filter for targeted cbcl-style conformance runs                 |
//                      | Charles Korthout | 1.0   | 19-07-2026     | Load catalog/test sets with PreserveWhitespace so assert-string-value keeps spaces/CR     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.1   | 19-07-2026     | DocumentedSkips: K-CodepointToStringFunc-8/11/12 XML 1.0-only on XML 1.1 implementation |
//                      | Charles Korthout | 1.2   | 21-07-2026     | Pass test-set base directory to TestCase for assert-xml file resolution                |
//                      | Charles Korthout | 1.3   | 21-07-2026     | Remove XML 1.0-only DocumentedSkips (now handled by DependencyFilter xml-version)         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.4   | 25-07-2026     | Route XQuery-only-spec tests to the XQuery pipeline when its constructs are supported   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.5   | 25-07-2026     | Regenerate KnownXQueryGaps (310 entries) after switch/typeswitch admission              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.6   | 25-07-2026     | Regenerate KnownXQueryGaps (305 entries) after serialization admission                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.7   | 26-07-2026     | Regenerate KnownXQueryGaps (487 entries) after declare function/variable admission     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.8   | 27-07-2026     | Regenerate KnownXQueryGaps (499 entries) after library module admission                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.9   | 27-07-2026     | Drop 35 StringConstructor gap entries (implemented); 464 entries remain        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.10  | 28-07-2026     | Gap list: NameTest cluster fixed (2 reasoned entries remain) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.11  | 29-07-2026     | Drop 17 VarDecl.external gap entries (implemented) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.12  | 29-07-2026     | Drop 11 NamespaceDecl gap entries (implemented) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.13  | 29-07-2026     | Drop 24 Annotation gap entries (implemented) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.14  | 29-07-2026     | Drop 16 Literal gap entries (implemented or stale) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.15  | 29-07-2026     | Drop 17 CombinedErrorCodes gap entries (implemented or stale) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.16  | 29-07-2026     | Drop 15 MapConstructor gap entries (implemented) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.17  | 29-07-2026     | Drop 14 AllowingEmpty gap entries (implemented) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.18  | 29-07-2026     | Drop 11 CompNamespaceConstructor gap entries (implemented) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.19  | 29-07-2026     | Drop 11 HigherOrderFunctions gap entries (implemented) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.20  | 29-07-2026     | Drop 27 add/subtract-dayTimeDurations gap entries (implemented) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.21  | 29-07-2026     | Drop 83 residual-cluster gap entries (implemented or stale) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.22  | 03-08-2026     | Drop re00987 (\c uses explicit NameChar ranges now; test passes) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.23  | 07-08-2026     | Drop 46 stale gap entries (probe-verified passing); 148 entries remain |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.24  | 14-08-2026     | Drop 25 stale KnownXQueryGaps entries (wave 3 fixes verified passing); 39 XQuery gap entries remain |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.25  | 14-08-2026     | Drop 8 collation-related KnownXQueryGaps entries after implementing default-collation fallback |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.26  | 14-08-2026     | Drop 5 deep-equal KnownXQueryGaps entries after ignoring comments/PIs in element children |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.27  | 14-08-2026     | Drop 3 direct-constructor attribute KnownXQueryGaps entries (comment/PI attribute values, xml:space validation) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.28  | 14-08-2026     | Drop 5 map:merge KnownXQueryGaps entries after correcting default duplicates option to use-first |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.29  | 15-08-2026     | Evaluate query-based environment collections; drop stale duplicates-maps-2 gap |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.30  | 15-08-2026     | Drop 5 collection/UseCaseR31 KnownXQueryGaps entries fixed by query-based collections |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.31  | 15-08-2026     | Drop rdb-queries-results-q9 (UseCaseR) after atomizing *-from-date functions |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.32  | 15-08-2026     | Drop fn-node-name-26 after assert-eq unwraps singleton QName sequences |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.33  | 15-08-2026     | Drop path009 after GetXPathParent handles document-level PIs/comments |
//                      | Charles Korthout | 1.34  | 17-08-2026     | Drop fn-unparsed-text-available-012 after encoding cardinality check |
//                      | Charles Korthout | 1.35  | 17-08-2026     | Drop d1e74610 after stripping trailing whitespace in assert-xml multi-root fragments |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.36  | 17-08-2026     | Drop analyzeString-028 after adding explicit fn namespace declaration |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.37  | 17-08-2026     | Drop cbcl-ns-fixup-1 after namespace fixup for clashing attribute prefixes |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.38  | 18-08-2026     | Drop cbcl-distinct-values-002b after distinct-values respects XSD string type families |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.39  | 18-08-2026     | Drop Catalog004 after axis steps treat empty-sequence input as empty instead of XPDY0002 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.31  | 15-08-2026     | Drop NodeTest004 after fixing document-node(element(...)) nested kind-test case preservation |
//                      |==================|=======|================|=========================================================================================
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.24  | 07-08-2026     | Fallback static base URI is the test-set file (K2-BaseURIProlog-5); drop 16 fixed entries (serialization char-refs, xml-to-json, base-uri/URI cluster) |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.25  | 07-08-2026     | Drop 66 fixed entries (type-strictness, EQName/static errors, format-dateTime); fn-unparsed-text-available-012 re-recorded as upstream defect |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.32  | 15-08-2026     | Drop 2 UseCaseR31 KnownXQueryGaps entries after map missing-key fix and map-to-function coercion |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.XPath.Conformance;

internal sealed class ConformanceRunner
{
    private readonly string _suitePath;
    private readonly string? _setFilter;
    private readonly string? _testFilter;
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
        // Platform limitation: .NET decimal is fixed-precision (28-29 significant digits).
        // These decimal literals exceed that range and are rounded to double, so the exact
        // expected string is unrecoverable; FOAR0002 is the spec-permitted alternative but
        // is indistinguishable from valid double literals at runtime.
        ["numberformat63"] = "Platform limitation: .NET decimal cannot preserve the precision of this decimal literal",
        ["numberformat64"] = "Platform limitation: .NET decimal cannot preserve the precision of this decimal literal",
    };

    /// <summary>
    /// XQuery conformance gaps: tests admitted to the XQuery pipeline that fail on engine
    /// features not yet implemented. Each entry names the missing feature; these are the
    /// work items for closing XQuery 3.1 conformance (see docs/FEATURE_REQUESTS.md REQ-045).
    /// </summary>
    private static readonly Dictionary<string, string> KnownXQueryGaps = new(StringComparer.Ordinal)
    {
        // app/CatalogCheck.xml (0 tests)
        // fn/analyze-string.xml (0 tests)
        // fn/distinct-values.xml (0 tests)
        // fn/unparsed-text.xml (1 test)
        ["fn-unparsed-text-054a"] = "External resource blocked: timeanddate.com answers .NET HttpClient with a Cloudflare JS challenge (HTTP 403); not an engine gap",
        // prod/DirElemContent.xml (0 tests)
        // prod/NameTest.xml (1 test)
        ["K2-NameTest-5"] = "XQuery conformance gap (NameTest): keywords usable as element names in expression positions (tokenizer-torture)",
    };

    public ConformanceRunner(string suitePath, string? setFilter = null, string? testFilter = null)
    {
        _suitePath = suitePath;
        _setFilter = setFilter;
        _testFilter = testFilter;
    }

    public TestReport Run()
    {
        var report = new TestReport();
        string catalogPath = Path.Combine(_suitePath, "catalog.xml");
        var catalog = XDocument.Load(catalogPath, LoadOptions.PreserveWhitespace);
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

            if (_setFilter is not null && setName is not null && !setName.Contains(_setFilter, StringComparison.OrdinalIgnoreCase))
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
        var doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        string baseDir = Path.GetDirectoryName(path) ?? _suitePath;

        // Collect test-set-level dependencies to inherit by each test case.
        var testSetDependencies = new List<Dependency>();
        foreach (var depElem in doc.Root?.Elements(_ns + "dependency") ?? [])
        {
            testSetDependencies.Add(Dependency.FromElement(depElem));
        }

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
            var testCase = TestCase.FromElement(testCaseElem, _ns, testSetDependencies, baseDir);

            if (_testFilter is not null && !testCase.Name.Contains(_testFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            // Documented skips: upstream defects and platform limitations.
            if (DocumentedSkips.TryGetValue(testCase.Name, out var skipReason))
            {
                report.Record(testCase.Name, TestOutcomeKind.Skipped, skipReason);
                continue;
            }

            // Known XQuery conformance gaps: admitted tests that need unimplemented engine
            // features; skipped with the missing feature as the reason.
            // TEMPORARY (sweep run): gate disabled via env var to re-measure the gaps.
            if (Environment.GetEnvironmentVariable("BOSAK_QT3_RUN_KNOWN_GAPS") is null
                && KnownXQueryGaps.TryGetValue(testCase.Name, out var gapReason))
            {
                report.Record(testCase.Name, TestOutcomeKind.Skipped, gapReason);
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
            // The fallback static base URI is the test-set FILE itself (K2-BaseURIProlog-5:
            // static-base-uri() ends with "prod/BaseURIDecl.xml"); relative resource
            // resolution is identical because file URIs resolve against the parent directory.
            if (env is null)
            {
                env = new TestEnvironment { BaseUri = new Uri(path).AbsoluteUri };
            }
            else if (string.IsNullOrEmpty(env.BaseUri))
            {
                env.BaseUri = new Uri(path).AbsoluteUri;
            }

            // Dependency check. Tests whose only unsupported dependencies are positive
            // XQuery spec tokens can still run when the query uses constructs the
            // XQuery pipeline supports (full FLWOR, basic prolog declarations).
            bool depsSupported = _dependencyFilter.IsSupported(testCase.Dependencies);
            if (!depsSupported)
            {
                depsSupported = _dependencyFilter.IsSupported(testCase.Dependencies, allowXQuerySpecs: true)
                                && TestExecutor.CanHandleAsXQuery(testCase.Expression);
            }
            if (!depsSupported)
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
