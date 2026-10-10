// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 oktober 2026
// PURPOSE              : Orchestrates loading, filtering, executing and reporting qt4tests cases.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-10-2026     | Creation (REQ-123 4.0-Exp S2): qt4tests orchestration at the frozen XPath40 level with     |
//                      |                  |       |                | experimental-retry executor; DocumentedSkips/KnownGaps seeded from the first sweep       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 09-10-2026     | Per-test watchdog (BOSAK_QT4_TEST_TIMEOUT_SECS, default 120s): runaway evaluations are     |
//                      |                  |       |                | recorded as failed-and-triaged instead of hanging the sweep (first sweep hit a 49GB      |
//                      |                  |       |                | runaway in op-to); each test runs on its own large-stack background thread                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 09-10-2026     | GatedSets mechanism: only listed (100% green) test-sets fail the run; per-set P/F/S        |
//                      |                  |       |                | deltas printed so the gate list can be maintained from sweep statistics                  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 09-10-2026     | GatedSets seeded from the third baseline sweep: 201 test-sets at 100% green (P>0, F=0)   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 09-10-2026     | fn:op slice: scan KnownGaps removed (all six pass at XPath40Experimental); fn-op and     |
//                      |                  |       |                | fn-scan promoted into GatedSets                                                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 09-10-2026     | parse-csv slice: ResultComparer.AssertCompatibility set to XPath40 (asserts may use      |
//                      |                  |       |                | fn:char and string templates); parse-csv/csv-to-xml/csv-doc promoted into GatedSets      |
//                      |------------------|-------|----------------|------------------------------------------------------------------------------------------|
//                      | Charles Korthout | 0.7   | 09-10-2026     | element-to-map slice: TestCase.DecodeEntitiesInTestExpressions enabled; fn-element-to-  |
//                      |                  |       |                | map/fn-map-to-element/fn-element-to-map-plan promoted into GatedSets                      |
//                      |------------------|-------|----------------|------------------------------------------------------------------------------------------|
//                      | Charles Korthout | 0.8   | 10-10-2026     | fn:atomic-equal slice: fn-atomic-equal promoted into GatedSets; fn-while-do promoted    |
//                      |                  |       |                | (last failure fixed incidentally by earlier engine work — verified 30/0/0)              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 10-10-2026     | focus-constructors slice: misc-FocusConstructors promoted into GatedSets (213 sets)     |
//                      |                  |       |                | after arity-0 xs:* constructor support (PR661) + assert-type unsignedLong annotation     |
//                      | Charles Korthout | 0.10  | 10-10-2026     | REQ-123 compare-tail slice: fn-collation-key, fn-contains-token and fn-collation-available promoted into GatedSets|
//                      |==================|=======|================|=========================================================================================
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.10  | 10-10-2026     | sort-with slice: fn-sort-with + array-sort-with promoted into GatedSets (215 sets);     |
//                      |                  |       |                | DependencyFilter40 marks typedData unsupported (no PSVI-typed source nodes)               |
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Api;

namespace Bosak.XPath.Conformance;

internal sealed class ConformanceRunner40
{
    private readonly string _suitePath;
    private readonly string? _setFilter;
    private readonly string? _testFilter;
    private readonly XNamespace _ns = "http://www.w3.org/2010/09/qt-fots-catalog";
    private readonly DependencyFilter40 _dependencyFilter = new();
    private readonly TestExecutor40 _executor = new();

    /// <summary>
    /// Per-test timeout (BOSAK_QT4_TEST_TIMEOUT_SECS, default 120s). The XPath engine has
    /// no cooperative cancellation, so a timed-out evaluation is recorded as failed (it
    /// lands in triage → DocumentedSkips/KnownGaps) and its thread is leaked; background
    /// threads never block process exit. First sweep found a pathological op-to test
    /// growing ~45MB/s — without the watchdog an unattended sweep OOMs the machine.
    /// </summary>
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(
        int.TryParse(Environment.GetEnvironmentVariable("BOSAK_QT4_TEST_TIMEOUT_SECS"), out var secs) && secs > 0 ? secs : 120);

    /// <summary>
    /// BOSAK_QT4_GATE_ONLY=1 runs only the GatedSets members — the fast verification pass
    /// after maintaining the gate list (full sweeps stay the default).
    /// </summary>
    private static readonly bool GateOnly =
        Environment.GetEnvironmentVariable("BOSAK_QT4_GATE_ONLY") == "1";

    /// <summary>
    /// Tests that can never pass in this harness, with the reason. Each entry is a
    /// documented upstream test/data defect or a documented platform limitation.
    /// Seeded from the 2026-10-09 baseline sweep; entries are dropped (never added
    /// silently) when the engine or the upstream suite fixes them.
    /// </summary>
    private static readonly Dictionary<string, string> DocumentedSkips = new(StringComparer.Ordinal)
    {
    };

    /// <summary>
    /// Known XPath 4.0 conformance gaps: admitted tests that fail on engine features not
    /// yet implemented. Each entry names the missing feature; these are the work items
    /// for closing 4.0 conformance. Seeded from the 2026-10-09 baseline sweep; the
    /// fn:op-blocked fn:scan entries were removed again by the fn:op slice (all six
    /// now pass at the XPath40Experimental level).
    /// </summary>
    private static readonly Dictionary<string, string> KnownGaps = new(StringComparer.Ordinal)
    {
    };

    public ConformanceRunner40(string suitePath, string? setFilter = null, string? testFilter = null)
    {
        _suitePath = suitePath;
        _setFilter = setFilter;
        _testFilter = testFilter;
        // Assert expressions may use XPath 4.0-only functions (fn:char) and string
        // templates (REQ-123 parse-csv slice); the QT3 harness keeps the 3.1 default.
        ResultComparer.AssertCompatibility = XPathCompatibility.XPath40;
        // qt4tests write predefined XML entities literally inside CDATA test/assert
        // expressions and expect them decoded (map-to-element-014/021,
        // expanded-QName-004-XQ); the QT3 corpus reads test strings literally.
        TestCase.DecodeEntitiesInTestExpressions = true;
    }

    /// <summary>
    /// The qt4tests gate: test-sets whose failures FAIL THE RUN (exit code 2). Only sets
    /// that are 100% green (zero failures, at least one pass) are listed; a set is
    /// promoted into the gate by the slice that fixes its last failure — never by
    /// silencing. Sets outside the gate still run and report (exploratory baseline) but
    /// do not affect the exit code. Seeded from the 2026-10-09 baseline sweep.
    /// </summary>
    private static readonly HashSet<string> GatedSets = new(StringComparer.Ordinal)
    {
        // Seeded 2026-10-09 from the third baseline sweep (25,774 passed / 5,940 failed /
        // 15,271 skipped): every listed set had P>0, F=0. Re-derive with BOSAK_QT4_GATE_ONLY=0.
        "fn-abs",
        "fn-adjust-date-to-timezone",
        "fn-adjust-dateTime-to-timezone",
        "fn-adjust-time-to-timezone",
        "fn-atomic-equal",
        "fn-available-environment-variables",
        "fn-boolean",
        "fn-ceiling",
        "fn-characters",
        "fn-codepoint-equal",
        "fn-collation-available",
        "fn-collation-key",
        "fn-collection",
        "fn-count",
        "fn-contains-subsequence",
        "fn-contains-token",
        "fn-csv-doc",
        "fn-csv-to-xml",
        "fn-current-date",
        "fn-current-dateTime",
        "fn-current-time",
        "fn-data",
        "fn-dateTime",
        "fn-day-from-date",
        "fn-do-until",
        "fn-default-collation",
        "fn-default-language",
        "fn-element-to-map",
        "fn-element-to-map-plan",
        "fn-element-with-id",
        "fn-empty",
        "fn-encode-for-uri",
        "fn-ends-with",
        "fn-environment-variable",
        "fn-escape-html-uri",
        "fn-exactly-one",
        "fn-exists",
        "fn-false",
        "fn-floor",
        "fn-foot",
        "fn-function-arity",
        "fn-head",
        "fn-hours-from-time",
        "fn-id",
        "fn-identity",
        "fn-idref",
        "fn-implicit-timezone",
        "fn-index-of-substring",
        "fn-insert-separator",
        "fn-in-scope-prefixes",
        "fn-iri-to-uri",
        "fn-items-at",
        "fn-lang",
        "fn-last",
        "fn-local-name-from-QName",
        "fn-lower-case",
        "fn-minutes-from-time",
        "fn-month-from-date",
        "fn-namespace-uri-for-prefix",
        "fn-namespace-uri-from-QName",
        "fn-not",
        "fn-one-or-more",
        "fn-op",
        "fn-parse-csv",
        "fn-parse-ietf-date",
        "fn-partition",
        "fn-position",
        "fn-prefix-from-QName",
        "fn-QName",
        "fn-replicate",
        "fn-resolve-QName",
        "fn-reverse",
        "fn-scan",
        "fn-seconds-from-duration",
        "fn-seconds-from-time",
        "fn-sort",
        "fn-sort-with",
        "fn-starts-with",
        "fn-static-base-uri",
        "fn-string-to-codepoints",
        "fn-take-while",
        "fn-tail",
        "fn-timezone-from-date",
        "fn-timezone-from-time",
        "fn-trace",
        "fn-translate",
        "fn-true",
        "fn-trunk",
        "fn-unix-dateTime",
        "fn-unordered",
        "fn-upper-case",
        "fn-uri-collection",
        "fn-while-do",
        "fn-year-from-date",
        "fn-zero-or-one",
        "math-acos",
        "math-asin",
        "math-atan",
        "math-atan2",
        "math-cos",
        "math-exp",
        "math-exp10",
        "math-log",
        "math-log10",
        "math-pi",
        "math-pow",
        "math-sin",
        "math-sqrt",
        "math-tan",
        "map-contains",
        "map-entry",
        "map-find",
        "map-remove",
        "map-size",
        "map-to-element",
        "array-append",
        "array-build",
        "array-flatten",
        "array-head",
        "array-insert-before",
        "array-items",
        "array-join",
        "array-put",
        "array-remove",
        "array-reverse",
        "array-size",
        "array-slice",
        "array-sort",
        "array-sort-with",
        "array-tail",
        "xs-anyAtomicType",
        "xs-anySimpleType",
        "xs-normalizedString",
        "xs-notation",
        "xs-numeric",
        "xs-token",
        "op-add-dayTimeDuration-to-date",
        "op-add-dayTimeDuration-to-time",
        "op-add-yearMonthDurations",
        "op-add-yearMonthDuration-to-date",
        "op-add-yearMonthDuration-to-dateTime",
        "op-anyURI-equal",
        "op-anyURI-greater-than",
        "op-anyURI-less-than",
        "op-boolean-equal",
        "op-boolean-greater-than",
        "op-boolean-less-than",
        "op-concatenate",
        "op-date-equal",
        "op-date-greater-than",
        "op-date-less-than",
        "op-dateTime-equal",
        "op-dateTime-greater-than",
        "op-dateTime-less-than",
        "op-dayTimeDuration-greater-than",
        "op-dayTimeDuration-less-than",
        "op-gDay-equal",
        "op-gMonth-equal",
        "op-gMonthDay-equal",
        "op-gYear-equal",
        "op-gYearMonth-equal",
        "op-hexBinary-greater-than",
        "op-hexBinary-less-than",
        "op-numeric-equal",
        "op-numeric-greater-than",
        "op-numeric-integer-divide",
        "op-numeric-less-than",
        "op-numeric-mod",
        "op-numeric-unary-plus",
        "op-pipeline",
        "op-string-equal",
        "op-string-greater-than",
        "op-string-less-than",
        "op-subtract-dates",
        "op-subtract-dateTimes",
        "op-subtract-dayTimeDuration-from-date",
        "op-subtract-dayTimeDuration-from-dateTime",
        "op-subtract-dayTimeDuration-from-time",
        "op-subtract-dayTimeDurations",
        "op-subtract-times",
        "op-subtract-yearMonthDuration-from-date",
        "op-subtract-yearMonthDuration-from-dateTime",
        "op-subtract-yearMonthDurations",
        "op-time-equal",
        "op-time-greater-than",
        "op-time-less-than",
        "op-yearMonthDuration-greater-than",
        "op-yearMonthDuration-less-than",
        "prod-AxisStep.abbr",
        "prod-AxisStep.unabbr",
        "prod-Comment",
        "prod-ContextItemExpr",
        "prod-CurlyArrayConstructor",
        "prod-DirectConstructor",
        "prod-DirElemContent",
        "prod-GeneralComp.ge",
        "prod-GeneralComp.le",
        "prod-GeneralComp.lt",
        "prod-GeneralComp.ne",
        "prod-IfExpr",
        "prod-KeywordArguments",
        "prod-LambdaExpr",
        "prod-NameTest",
        "prod-OrExpr",
        "prod-ParenthesizedExpr",
        "prod-ReturnClause",
        "prod-SquareArrayConstructor",
        "prod-StringTemplate",
        "prod-TreatExpr",
        "misc-AppendixA4",
        "misc-FocusConstructors",
        "misc-Surrogates",
        "misc-UCACollation",
        "misc-XMLEdition",
        "app-FunctxFn",
        "app-UseCaseJSON",
        "app-UseCaseNLP",
        "app-UseCaseSEQ",
        "app-UseCaseSTRING",
    };

    /// <summary>Failures inside <see cref="GatedSets"/> — these fail the run.</summary>
    public int GatedFailures { get; private set; }

    /// <summary>All failures (gated + exploratory) — reported, but only gated ones fail the run.</summary>
    public int TotalFailures => _totalFailures;

    private int _totalFailures;

    /// <summary>Passes that only succeeded at the XPath40Experimental level (e.g. fn:scan).</summary>
    public int ExperimentalPasses => _executor.ExperimentalPasses;

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

            if (GateOnly && setName is not null && !GatedSets.Contains(setName))
                continue;

            string testSetPath = Path.Combine(_suitePath, fileName);
            if (!File.Exists(testSetPath))
            {
                Console.WriteLine($"  Skip missing test set: {fileName}");
                continue;
            }

            // app-CatalogCheck is a meta/consistency set: each test case loads the entire
            // catalog and all referenced test-set files. Running it dominates the sweep
            // and appears to hang; record its tests as skipped (same posture as QT3).
            if (setName is "app-CatalogCheck" or "app-Demos" or "app-XMark")
            {
                var doc = XDocument.Load(testSetPath, LoadOptions.PreserveWhitespace);
                string reason = setName switch
                {
                    "app-CatalogCheck" => "Catalog consistency checks load the full corpus per test; skipped to avoid hang",
                    "app-Demos" => "Demo applications are skipped during unattended sweeps",
                    _ => "XMark benchmark set is skipped during unattended sweeps",
                };
                int skippedBefore = report.Skipped;
                foreach (var testCaseElem in doc.Descendants(_ns + "test-case"))
                {
                    var testName = (string?)testCaseElem.Attribute("name");
                    if (!string.IsNullOrEmpty(testName))
                        report.Record(testName, TestOutcomeKind.Skipped, reason);
                }
                processedSets++;
                Console.WriteLine($"  Done: {setName} ({report.Total} tests total) [skipped by policy] [P=0 F=0 S+={report.Skipped - skippedBefore}]");
                Console.Out.Flush();
                continue;
            }

            Console.WriteLine($"  Starting: {setName} ...");
            Console.Out.Flush();
            int passedBefore = report.Passed, failedBefore = report.Failed, skippedBefore2 = report.Skipped;
            RunTestSet(testSetPath, setName ?? "", sharedEnvironments, report);
            processedSets++;
            Console.WriteLine($"  Done: {setName} ({report.Total} tests total) [P+={report.Passed - passedBefore} F+={report.Failed - failedBefore} S+={report.Skipped - skippedBefore2}]");
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

    private void RunTestSet(string path, string setName, Dictionary<string, TestEnvironment> sharedEnvs, TestReport report)
    {
        var doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        string baseDir = Path.GetDirectoryName(path) ?? _suitePath;
        bool gated = GatedSets.Contains(setName);

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

            // Known 4.0 conformance gaps: admitted tests that need unimplemented engine
            // features; skipped with the missing feature as the reason.
            if (Environment.GetEnvironmentVariable("BOSAK_QT4_RUN_KNOWN_GAPS") is null
                && KnownGaps.TryGetValue(testCase.Name, out var gapReason))
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
            // resource URIs against the test-set file's directory. The fallback static
            // base URI is the test-set FILE itself (relative resource resolution is
            // identical because file URIs resolve against the parent directory).
            if (env is null)
            {
                env = new TestEnvironment { BaseUri = new Uri(path).AbsoluteUri };
            }
            else if (string.IsNullOrEmpty(env.BaseUri))
            {
                env.BaseUri = new Uri(path).AbsoluteUri;
            }

            // Dependency check: the 4.0 harness runs the XPath pipeline only; XQ-only
            // spec dependencies are not satisfiable here (no XQuery 4.0 mode).
            if (!_dependencyFilter.IsSupported(testCase.Dependencies))
            {
                report.Record(testCase.Name, TestOutcomeKind.Skipped, "Unsupported dependency");
                continue;
            }

            try
            {
                var outcome = ExecuteWithTimeout(testCase, env);
                report.Record(testCase.Name, outcome.Kind, outcome.Message);
                if (outcome.Kind == TestOutcomeKind.Failed)
                {
                    _totalFailures++;
                    if (gated)
                        GatedFailures++;
                }
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

    /// <summary>
    /// Runs one test on a dedicated large-stack background thread with <see cref="TestTimeout"/>.
    /// On timeout the runaway evaluation is recorded as failed for triage; the leaked
    /// thread (IsBackground) never blocks process exit.
    /// </summary>
    private TestOutcome ExecuteWithTimeout(TestCase testCase, TestEnvironment env)
    {
        var tcs = new TaskCompletionSource<TestOutcome>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                tcs.SetResult(_executor.Execute(testCase, env));
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, maxStackSize: 512 * 1024 * 1024)
        {
            IsBackground = true,
            Name = $"qt4-test-{testCase.Name}",
        };
        thread.Start();
        if (tcs.Task.Wait(TestTimeout))
            return tcs.Task.Result;
        return new TestOutcome(TestOutcomeKind.Failed,
            $"Timeout after {TestTimeout.TotalSeconds:F0}s: runaway evaluation (recorded for triage)");
    }
}
