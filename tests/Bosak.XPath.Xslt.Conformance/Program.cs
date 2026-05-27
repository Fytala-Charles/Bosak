using System.Xml.Linq;
using System.Xml;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;

namespace Bosak.XPath.Xslt.Conformance;

class Program
{
    static int Passed = 0;
    static int Failed = 0;
    static int Skipped = 0;

    static readonly HashSet<string> SupportedSpecs = new(StringComparer.OrdinalIgnoreCase)
    {
        "XSLT20+", "XSLT20", "XSLT30+", "XSLT30", "XSLT"
    };

    static readonly HashSet<string> SkipFeatures = new(StringComparer.OrdinalIgnoreCase)
    {
        "serialization",
        "schema-awareness",
        "schema-import",
        "streaming",
        "packages",
        "dynamic-evaluation",
        "higher-order-functions",
        "xslt-3.0-snapshot"
    };

    static void Main(string[] args)
    {
        string catalogPath = args.Length > 0 ? args[0] : "tests/xslt30-test/catalog.xml";
        string? filter = args.Length > 1 ? args[1] : null;

        if (!File.Exists(catalogPath))
        {
            Console.WriteLine($"Catalog not found: {catalogPath}");
            Environment.Exit(1);
        }

        var catalogDir = Path.GetDirectoryName(Path.GetFullPath(catalogPath))!;
        var catalog = XDocument.Load(catalogPath);
        XNamespace ns = "http://www.w3.org/2012/10/xslt-test-catalog";

        var testSets = catalog.Root!.Elements(ns + "test-set").ToList();
        Console.WriteLine($"Bosak XSLT Conformance Harness");
        Console.WriteLine($"Catalog: {catalogPath}");
        Console.WriteLine($"Test sets: {testSets.Count}");
        if (filter != null) Console.WriteLine($"Filter: {filter}");
        Console.WriteLine();

        foreach (var testSetElem in testSets)
        {
            var testSetName = testSetElem.Attribute("name")?.Value ?? "";
            var testSetFile = testSetElem.Attribute("file")?.Value ?? "";

            if (filter != null && !testSetName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                continue;

            var testSetPath = Path.Combine(catalogDir, testSetFile);
            if (!File.Exists(testSetPath))
            {
                Console.WriteLine($"  Skip: test-set file not found: {testSetPath}");
                continue;
            }

            RunTestSet(testSetPath, testSetName, catalogDir);
        }

        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine("XSLT Conformance Test Results");
        Console.WriteLine("============================================================");
        Console.WriteLine($"Total:   {Passed + Failed + Skipped}");
        Console.WriteLine($"Passed:  {Passed}");
        Console.WriteLine($"Failed:  {Failed}");
        Console.WriteLine($"Skipped: {Skipped}");
        Console.WriteLine();
        Console.WriteLine($"Pass rate: {(Passed * 100.0 / Math.Max(1, Passed + Failed)):F1}%");
    }

    static void RunTestSet(string testSetPath, string testSetName, string catalogDir)
    {
        var doc = XDocument.Load(testSetPath);
        XNamespace ns = "http://www.w3.org/2012/10/xslt-test-catalog";
        var testSetDir = Path.GetDirectoryName(testSetPath)!;

        var environments = new Dictionary<string, XElement>();
        foreach (var env in doc.Root!.Elements(ns + "environment"))
        {
            var name = env.Attribute("name")?.Value;
            if (name != null) environments[name] = env;
        }

        var testCases = doc.Root.Elements(ns + "test-case").ToList();
        if (testCases.Count == 0) return;

        Console.WriteLine($"  Starting: {testSetName} ...");
        int setPassed = 0, setFailed = 0, setSkipped = 0;

        foreach (var testCase in testCases)
        {
            var result = RunTestCase(testCase, environments, testSetDir, catalogDir, ns);
            if (result == TestResult.Pass) setPassed++;
            else if (result == TestResult.Fail) setFailed++;
            else setSkipped++;
        }

        Passed += setPassed;
        Failed += setFailed;
        Skipped += setSkipped;
        Console.WriteLine($"  Done: {testSetName} ({testCases.Count} tests, {setPassed} passed, {setFailed} failed, {setSkipped} skipped)");
    }

    enum TestResult { Pass, Fail, Skip }

    static TestResult RunTestCase(XElement testCase, Dictionary<string, XElement> environments, string testSetDir, string catalogDir, XNamespace ns)
    {
        var name = testCase.Attribute("name")?.Value ?? "unknown";

        try
        {
            // Check dependencies
            var deps = testCase.Element(ns + "dependencies");
            if (deps != null)
            {
                foreach (var spec in deps.Elements(ns + "spec"))
                {
                    var val = spec.Attribute("value")?.Value ?? "";
                    if (!IsSpecSupported(val))
                        return TestResult.Skip;
                }
                foreach (var feature in deps.Elements(ns + "feature"))
                {
                    var val = feature.Attribute("value")?.Value ?? "";
                    if (SkipFeatures.Contains(val))
                        return TestResult.Skip;
                }
            }

            // Load environment (source XML)
            IXdmNode? sourceNode = null;
            var envRef = testCase.Element(ns + "environment")?.Attribute("ref")?.Value;
            if (envRef == null)
                envRef = testCase.Attribute("ref")?.Value;

            if (envRef != null && environments.TryGetValue(envRef, out var envElem))
            {
                sourceNode = LoadEnvironment(envElem, testSetDir, catalogDir, ns);
            }

            // Load test (stylesheet(s))
            var testElem = testCase.Element(ns + "test");
            if (testElem == null) return TestResult.Skip;

            var stylesheetElem = testElem.Element(ns + "stylesheet");
            if (stylesheetElem == null) return TestResult.Skip;

            var mainStylesheetFile = stylesheetElem.Attribute("file")?.Value;
            if (mainStylesheetFile == null) return TestResult.Skip;

            var mainStylesheetPath = Path.Combine(testSetDir, mainStylesheetFile);
            if (!File.Exists(mainStylesheetPath))
            {
                // Try relative to catalog dir
                mainStylesheetPath = Path.Combine(catalogDir, mainStylesheetFile);
                if (!File.Exists(mainStylesheetPath))
                    return TestResult.Skip;
            }

            // Build resolver for secondary stylesheets
            var resolver = new TestUriResolver(testSetDir, catalogDir);
            foreach (var ss in testElem.Elements(ns + "stylesheet"))
            {
                var file = ss.Attribute("file")?.Value;
                var role = ss.Attribute("role")?.Value;
                if (file != null && role == "secondary")
                {
                    var path = Path.Combine(testSetDir, file);
                    if (!File.Exists(path)) path = Path.Combine(catalogDir, file);
                    if (File.Exists(path))
                    {
                        var uri = new Uri(path).AbsoluteUri;
                        resolver.Register(uri, path);
                    }
                }
            }

            // Compile and run
            var xslText = File.ReadAllText(mainStylesheetPath);
            var compiler = new Bosak.XPath.Xslt.Api.XsltCompiler { UriResolver = resolver };
            var baseUri = new Uri(mainStylesheetPath).AbsoluteUri;
            var executable = compiler.Compile(xslText, baseUri);

            // Set up document loader that handles document('') by returning the stylesheet
            var xslDoc = XDocument.Parse(xslText, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            var evalContext = new Bosak.XPath.Runtime.Vm.EvaluationContext();
            evalContext.BaseUri = baseUri;
            evalContext.DocumentLoader = uri =>
            {
                if (string.IsNullOrEmpty(uri) || uri == baseUri)
                    return new XDocumentNode(xslDoc);
                var resolvedUri = uri;
                if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute) && !string.IsNullOrEmpty(baseUri))
                    resolvedUri = new Uri(new Uri(baseUri), uri).AbsoluteUri;
                var localPath = new Uri(resolvedUri).LocalPath;
                if (File.Exists(localPath))
                {
                    var doc = XDocument.Load(localPath);
                    return new XDocumentNode(doc);
                }
                // Try test set dir
                var testPath = Path.Combine(testSetDir, uri);
                if (File.Exists(testPath))
                {
                    var doc = XDocument.Load(testPath);
                    return new XDocumentNode(doc);
                }
                throw new FileNotFoundException($"Document not found: {uri}");
            };

            // Check for initial-template
            string? initialTemplate = null;
            var initialTemplateElem = testElem.Element(ns + "initial-template");
            if (initialTemplateElem != null)
                initialTemplate = initialTemplateElem.Attribute("name")?.Value;

            string resultXml;
            if (sourceNode != null)
            {
                resultXml = executable.TransformToString(sourceNode, evalContext, initialTemplate);
            }
            else
            {
                resultXml = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("dummy"))), evalContext, initialTemplate);
            }

            // Compare with expected result
            var resultElem = testCase.Element(ns + "result");
            if (resultElem == null) return TestResult.Skip;

            if (CompareResult(resultXml, resultElem, ns, testSetDir, catalogDir))
            {
                // Console.WriteLine($"  PASS {name}");
                return TestResult.Pass;
            }

            Console.WriteLine($"  FAIL {name}: Result mismatch");
            Console.WriteLine($"    Expected: {GetExpectedDescription(resultElem, ns, testSetDir, catalogDir)}");
            Console.WriteLine($"    Got:      {resultXml.Trim()}");
            return TestResult.Fail;
        }
        catch (Exception ex)
        {
            // Check if an error was expected
            var resultElem = testCase.Element(ns + "result");
            if (resultElem != null && resultElem.Element(ns + "error") != null)
            {
                return TestResult.Pass;
            }
            if (resultElem != null && resultElem.Element(ns + "any-of") != null)
            {
                foreach (var child in resultElem.Element(ns + "any-of")!.Elements())
                {
                    if (child.Name.LocalName == "error") return TestResult.Pass;
                }
            }

            Console.WriteLine($"  FAIL {name}: {ex.Message}");
            return TestResult.Fail;
        }
    }

    static bool IsSpecSupported(string specValue)
    {
        // XSLT20+ means XSLT 2.0 and higher; we support XSLT 2.0/3.0 basics
        if (specValue.StartsWith("XSLT", StringComparison.OrdinalIgnoreCase))
        {
            // For now, support all XSLT specs (we'll skip unsupported features via feature checks)
            return true;
        }
        return false;
    }

    static IXdmNode? LoadEnvironment(XElement envElem, string testSetDir, string catalogDir, XNamespace ns)
    {
        var source = envElem.Element(ns + "source");
        if (source == null) return null;

        var content = source.Element(ns + "content");
        if (content != null)
        {
            var cdata = content.Nodes().OfType<XCData>().FirstOrDefault();
            var xmlText = cdata?.Value ?? content.Value;
            var doc = XDocument.Parse(xmlText);
            return new XDocumentNode(doc);
        }

        var file = source.Attribute("file")?.Value;
        if (file != null)
        {
            var path = Path.Combine(testSetDir, file);
            if (!File.Exists(path)) path = Path.Combine(catalogDir, file);
            if (File.Exists(path))
            {
                var doc = XDocument.Load(path);
                return new XDocumentNode(doc);
            }
        }

        return null;
    }

    static bool CompareResult(string actual, XElement resultElem, XNamespace ns, string testSetDir, string catalogDir)
    {
        // Handle <all-of>
        var allOf = resultElem.Element(ns + "all-of");
        if (allOf != null)
        {
            foreach (var option in allOf.Elements())
            {
                if (!CompareSingleResult(actual, option, ns, testSetDir, catalogDir))
                    return false;
            }
            return true;
        }

        // Handle <any-of>
        var anyOf = resultElem.Element(ns + "any-of");
        if (anyOf != null)
        {
            foreach (var option in anyOf.Elements())
            {
                if (CompareSingleResult(actual, option, ns, testSetDir, catalogDir))
                    return true;
            }
            return false;
        }

        return CompareSingleResult(actual, resultElem, ns, testSetDir, catalogDir);
    }

    static bool CompareSingleResult(string actual, XElement resultElem, XNamespace ns, string testSetDir, string catalogDir)
    {
        // assert-xml
        var assertXml = resultElem.Element(ns + "assert-xml");
        if (assertXml != null)
        {
            var expected = assertXml.Value.Trim();
            // Load from file if specified
            var fileAttr = assertXml.Attribute("file")?.Value;
            if (string.IsNullOrEmpty(expected) && !string.IsNullOrEmpty(fileAttr))
            {
                var filePath = Path.Combine(testSetDir, fileAttr);
                if (!File.Exists(filePath)) filePath = Path.Combine(catalogDir, fileAttr);
                if (File.Exists(filePath))
                    expected = File.ReadAllText(filePath).Trim();
            }
            // Normalize whitespace for comparison
            var normActual = NormalizeXml(actual);
            var normExpected = NormalizeXml(expected);
            return normActual == normExpected || actual.Trim() == expected;
        }

        // assert-string-value
        var assertString = resultElem.Element(ns + "assert-string-value");
        if (assertString != null)
        {
            return actual.Trim() == assertString.Value;
        }

        // assert-true
        if (resultElem.Element(ns + "assert-true") != null)
        {
            return actual.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        // assert-false
        if (resultElem.Element(ns + "assert-false") != null)
        {
            return actual.Trim().Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        // assert: evaluate XPath expression against result document
        var assertExpr = resultElem.Element(ns + "assert");
        if (assertExpr != null)
        {
            return EvaluateAssert(actual, assertExpr.Value);
        }

        // assert-eq: evaluate XPath and compare atomized value
        var assertEq = resultElem.Element(ns + "assert-eq");
        if (assertEq != null)
        {
            var expected = assertEq.Attribute("expected")?.Value ?? assertEq.Value;
            var expr = assertEq.Attribute("select")?.Value ?? assertEq.Value;
            return EvaluateAssertEq(actual, expr, expected);
        }

        // serialization
        var assertSer = resultElem.Element(ns + "assert-serialization");
        if (assertSer != null)
        {
            var expected = assertSer.Value.Trim();
            return NormalizeXml(actual) == NormalizeXml(expected);
        }

        // error expected
        if (resultElem.Element(ns + "error") != null)
        {
            // If we reach here, no error was thrown - that's handled by the caller
            return false;
        }

        return false;
    }

    static IXdmNode? ParseResultDocument(string actual)
    {
        try
        {
            // If it's a well-formed document, parse directly
            var doc = XDocument.Parse(actual, LoadOptions.PreserveWhitespace);
            return new XDocumentNode(doc);
        }
        catch
        {
            // Not well-formed XML (e.g., text output or XML fragment)
            // Try wrapping in a dummy root
            try
            {
                var wrapped = $"<__root__>{actual}</__root__>";
                var doc = XDocument.Parse(wrapped, LoadOptions.PreserveWhitespace);
                return new XDocumentNode(doc);
            }
            catch
            {
                return null;
            }
        }
    }

    static bool EvaluateAssert(string actual, string xpath)
    {
        var contextNode = ParseResultDocument(actual);
        if (contextNode == null)
            return false;

        try
        {
            var compiled = XPath31Expression.Compile(xpath);
            var result = compiled.Evaluate(contextNode);
            return result.EffectiveBooleanValue();
        }
        catch
        {
            return false;
        }
    }

    static bool EvaluateAssertEq(string actual, string xpath, string expected)
    {
        var contextNode = ParseResultDocument(actual);
        if (contextNode == null)
            return false;

        try
        {
            var compiled = XPath31Expression.Compile(xpath);
            var result = compiled.Evaluate(contextNode);
            return result.ToString() == expected;
        }
        catch
        {
            return false;
        }
    }

    static string NormalizeXml(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return doc.ToString(SaveOptions.DisableFormatting).Replace(" />", "/>");
        }
        catch
        {
            return xml.Trim();
        }
    }

    static string GetExpectedDescription(XElement resultElem, XNamespace ns, string testSetDir = "", string catalogDir = "")
    {
        var assertXml = resultElem.Element(ns + "assert-xml");
        if (assertXml != null)
        {
            var val = assertXml.Value.Trim();
            var fileAttr = assertXml.Attribute("file")?.Value;
            if (string.IsNullOrEmpty(val) && !string.IsNullOrEmpty(fileAttr))
            {
                var filePath = Path.Combine(testSetDir, fileAttr);
                if (!File.Exists(filePath)) filePath = Path.Combine(catalogDir, fileAttr);
                if (File.Exists(filePath))
                    val = File.ReadAllText(filePath).Trim();
            }
            return val;
        }
        var assertString = resultElem.Element(ns + "assert-string-value");
        if (assertString != null) return assertString.Value;
        var err = resultElem.Element(ns + "error");
        if (err != null) return $"error {err.Attribute("code")?.Value}";
        return "(complex assertion)";
    }
}

public class TestUriResolver : Bosak.XPath.Xslt.Api.IXsltUriResolver
{
    private readonly Dictionary<string, string> _mappings = new();
    private readonly string _primaryDir;
    private readonly string _fallbackDir;

    public TestUriResolver(string primaryDir, string fallbackDir)
    {
        _primaryDir = primaryDir;
        _fallbackDir = fallbackDir;
    }

    public void Register(string uri, string path) => _mappings[uri] = path;

    public XDocument Resolve(string href, string? baseUri)
    {
        // Try direct mapping
        if (_mappings.TryGetValue(href, out var mappedPath) && File.Exists(mappedPath))
            return XDocument.Load(mappedPath);

        // Resolve relative to baseUri
        if (!string.IsNullOrEmpty(baseUri))
        {
            var baseUriObj = new Uri(baseUri);
            var resolved = new Uri(baseUriObj, href);
            var resolvedPath = resolved.LocalPath;
            if (File.Exists(resolvedPath))
                return XDocument.Load(resolvedPath);
        }

        // Try primary dir
        var primaryPath = Path.Combine(_primaryDir, href);
        if (File.Exists(primaryPath))
            return XDocument.Load(primaryPath);

        // Try fallback dir
        var fallbackPath = Path.Combine(_fallbackDir, href);
        if (File.Exists(fallbackPath))
            return XDocument.Load(fallbackPath);

        throw new FileNotFoundException($"Stylesheet not found: {href} (base: {baseUri})");
    }
}
