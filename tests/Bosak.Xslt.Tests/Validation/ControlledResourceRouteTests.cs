// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Contract tests for REQ-125 Slice B: table-driven coverage of every declared acquisition route (AC-04, AC-05).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice B)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Resources;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>
/// AC-04/AC-05 route coverage for the controlled resource policy: table-driven tests exercise
/// every declared acquisition route — document()/doc-available(), unparsed-text(+lines/
/// available), json-doc, collection/uri-collection, xsl:source-document (including the
/// streaming path), fn:transform and xsl:evaluate nested inheritance, and static
/// (use-when) evaluation — with denial producing no disk/network fallback, and receipts
/// describing the exact effective inputs.
/// </summary>
public class ControlledResourceRouteTests
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false);

    private sealed class StubAuthority : IControlledResourceAuthority
    {
        private readonly object _gate = new();
        private readonly List<ControlledResourceRequest> _requests = new();

        public Func<ControlledResourceRequest, ControlledResourceResponse?>? Handler { get; set; }

        public IReadOnlyList<ControlledResourceRequest> Requests
        {
            get { lock (_gate) return _requests.ToArray(); }
        }

        public ControlledResourceResponse? Authorize(ControlledResourceRequest request)
        {
            lock (_gate)
                _requests.Add(request);
            return Handler?.Invoke(request);
        }
    }

    /// <summary>Approves URIs listed in <paramref name="bytes"/> and abstains otherwise.</summary>
    private static StubAuthority AuthorityApproving(IReadOnlyDictionary<string, byte[]> bytes, out ControlledResourcePolicy policy)
    {
        var authority = new StubAuthority
        {
            Handler = request => bytes.TryGetValue(request.Uri, out var b)
                ? ControlledResourceResponse.Approve(b)
                : null,
        };
        policy = new ControlledResourcePolicy(authority);
        return authority;
    }

    private static string Wrap(string body) => $$"""
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:fn="http://www.w3.org/2005/xpath-functions">
          <xsl:output method="xml" indent="no"/>
          <xsl:template match="/">
            <out>{{body}}</out>
          </xsl:template>
        </xsl:stylesheet>
        """;

    /// <summary>Compiles without a policy and runs under the supplied policy context.</summary>
    private static string Run(string xsl, ControlledResourcePolicy policy)
    {
        var executable = new XsltCompiler().Compile(xsl, "file:///policy-test/main.xsl");
        var context = new EvaluationContext { ResourcePolicy = policy };
        return executable.TransformToString(XDocumentProvider.ParseXml("<dummy/>"), context);
    }

    // ------------------------------------------------------------------
    // AC-04: table-driven route coverage (approval path + refusal path)
    // ------------------------------------------------------------------

    public static IEnumerable<object[]> ApprovalRoutes
    {
        get
        {
            var docBytes = Utf8.GetBytes("<root><child>DOC-OK</child></root>");
            var srcBytes = Utf8.GetBytes("<root><child>SRC-OK</child></root>");
            var textBytes = Utf8.GetBytes("TEXT-OK");
            var linesBytes = Utf8.GetBytes("line-1\nline-2");
            var jsonBytes = Utf8.GetBytes("""{"a":"JSON-OK"}""");
            var member1 = Utf8.GetBytes("<m>one</m>");
            var member2 = Utf8.GetBytes("<m>two</m>");

            // route name, stylesheet body, approvals (uri -> bytes), collection approvals (uri -> members), expected output fragment
            yield return new object[]
            {
                "fn:doc",
                """<xsl:value-of select="doc('urn:test:doc')/root/child"/>""",
                new Dictionary<string, byte[]> { ["urn:test:doc"] = docBytes },
                null,
                "DOC-OK",
            };
            yield return new object[]
            {
                "fn:document (XSLT)",
                """<xsl:value-of select="document('urn:test:doc')/root/child"/>""",
                new Dictionary<string, byte[]> { ["urn:test:doc"] = docBytes },
                null,
                "DOC-OK",
            };
            yield return new object[]
            {
                "fn:doc-available (approved)",
                """<xsl:value-of select="doc-available('urn:test:doc')"/>""",
                new Dictionary<string, byte[]> { ["urn:test:doc"] = docBytes },
                null,
                ">true<",
            };
            yield return new object[]
            {
                "fn:unparsed-text",
                """<xsl:value-of select="unparsed-text('urn:test:text')"/>""",
                new Dictionary<string, byte[]> { ["urn:test:text"] = textBytes },
                null,
                "TEXT-OK",
            };
            yield return new object[]
            {
                "fn:unparsed-text-lines",
                """<xsl:value-of select="unparsed-text-lines('urn:test:lines')"/>""",
                new Dictionary<string, byte[]> { ["urn:test:lines"] = linesBytes },
                null,
                "line-1 line-2",
            };
            yield return new object[]
            {
                "fn:unparsed-text-available (approved)",
                """<xsl:value-of select="unparsed-text-available('urn:test:text')"/>""",
                new Dictionary<string, byte[]> { ["urn:test:text"] = textBytes },
                null,
                ">true<",
            };
            yield return new object[]
            {
                "fn:json-doc",
                """<xsl:value-of select="json-doc('urn:test:json')?a"/>""",
                new Dictionary<string, byte[]> { ["urn:test:json"] = jsonBytes },
                null,
                "JSON-OK",
            };
            yield return new object[]
            {
                "fn:collection",
                """<xsl:value-of select="count(collection('urn:test:coll'))"/>""",
                new Dictionary<string, byte[]>
                {
                    ["urn:test:coll/one.xml"] = member1,
                    ["urn:test:coll/two.xml"] = member2,
                },
                new Dictionary<string, IReadOnlyList<string>> { ["urn:test:coll"] = new[] { "urn:test:coll/one.xml", "urn:test:coll/two.xml" } },
                ">2<",
            };
            yield return new object[]
            {
                "fn:uri-collection",
                """<xsl:value-of select="count(uri-collection('urn:test:coll'))"/>""",
                new Dictionary<string, byte[]>
                {
                    ["urn:test:coll/one.xml"] = member1,
                    ["urn:test:coll/two.xml"] = member2,
                },
                new Dictionary<string, IReadOnlyList<string>> { ["urn:test:coll"] = new[] { "urn:test:coll/one.xml", "urn:test:coll/two.xml" } },
                ">2<",
            };
            yield return new object[]
            {
                "xsl:source-document",
                """<xsl:source-document href="urn:test:src"><xsl:value-of select="root/child"/></xsl:source-document>""",
                new Dictionary<string, byte[]> { ["urn:test:src"] = srcBytes },
                null,
                "SRC-OK",
            };
            yield return new object[]
            {
                "xsl:source-document (streamable)",
                """<xsl:source-document href="urn:test:src" streamable="yes"><xsl:value-of select="root/child"/></xsl:source-document>""",
                new Dictionary<string, byte[]> { ["urn:test:src"] = srcBytes },
                null,
                "SRC-OK",
            };
        }
    }

    [Theory]
    [MemberData(nameof(ApprovalRoutes))]
    public void Route_ApprovedBytes_AcquireWithoutAnyDiskOrNetwork(
        string routeName,
        string body,
        IReadOnlyDictionary<string, byte[]> approvals,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? collectionApprovals,
        string expected)
    {
        var authority = new StubAuthority
        {
            Handler = request =>
            {
                if (approvals.TryGetValue(request.Uri, out var bytes))
                    return ControlledResourceResponse.Approve(bytes);
                if (collectionApprovals is not null && collectionApprovals.TryGetValue(request.Uri, out var members))
                    return ControlledResourceResponse.ApproveCollection(members);
                return null;
            },
        };
        var policy = new ControlledResourcePolicy(authority);

        var result = Run(Wrap(body), policy);

        Assert.True(result.Contains(expected), $"{routeName}: expected '{expected}' in '{result}'");
        Assert.NotEmpty(authority.Requests);
        Assert.All(policy.Receipts, r => Assert.Equal(ControlledResourceReceiptOutcome.Approved, r.Outcome));
    }

    public static IEnumerable<object[]> RefusalRoutes
    {
        get
        {
            var anyBytes = Utf8.GetBytes("<root><child>SHADOW</child></root>");

            // route name, stylesheet body, expected XV code
            yield return new object[]
            {
                "fn:doc",
                """<xsl:value-of select="doc('urn:test:doc')/root/child"/>""",
            };
            yield return new object[]
            {
                "fn:document (XSLT)",
                """<xsl:value-of select="document('urn:test:doc')/root/child"/>""",
            };
            yield return new object[]
            {
                "fn:unparsed-text",
                """<xsl:value-of select="unparsed-text('urn:test:text')"/>""",
            };
            yield return new object[]
            {
                "fn:unparsed-text-lines",
                """<xsl:value-of select="unparsed-text-lines('urn:test:text')"/>""",
            };
            yield return new object[]
            {
                "fn:json-doc",
                """<xsl:value-of select="json-doc('urn:test:json')?a"/>""",
            };
            yield return new object[]
            {
                "fn:collection",
                """<xsl:value-of select="count(collection('urn:test:coll'))"/>""",
            };
            yield return new object[]
            {
                "xsl:source-document",
                """<xsl:source-document href="urn:test:src"><xsl:value-of select="root/child"/></xsl:source-document>""",
            };
            yield return new object[]
            {
                "xsl:source-document (streamable)",
                """<xsl:source-document href="urn:test:src" streamable="yes"><xsl:value-of select="root/child"/></xsl:source-document>""",
            };
        }
    }

    [Theory]
    [MemberData(nameof(RefusalRoutes))]
    public void Route_DenyAll_RefusesWithXv0004(string routeName, string body)
    {
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Deny("not approved") };
        var policy = new ControlledResourcePolicy(authority);

        var ex = Assert.Throws<InvalidOperationException>(() => Run(Wrap(body), policy));

        Assert.True(ex.Message.StartsWith("XV0004", StringComparison.Ordinal), $"{routeName}: {ex.Message}");
        Assert.Equal("not approved", ex.Message.Split(": ").Last());
    }

    // ------------------------------------------------------------------
    // No disk fallback: a valid file on disk is never read after a denial
    // ------------------------------------------------------------------

    public static IEnumerable<object[]> DiskFallbackRoutes
    {
        get
        {
            yield return new object[]
            {
                "fn:doc",
                """<xsl:value-of select="doc('{0}')/root/child"/>""",
                "<root><child>DISK-SHADOW</child></root>",
            };
            yield return new object[]
            {
                "fn:unparsed-text",
                """<xsl:value-of select="unparsed-text('{0}')"/>""",
                "DISK-SHADOW",
            };
            yield return new object[]
            {
                "fn:unparsed-text-available",
                """<xsl:value-of select="unparsed-text-available('{0}')"/>""",
                "DISK-SHADOW",
            };
            yield return new object[]
            {
                "fn:json-doc",
                """<xsl:value-of select="json-doc('{0}')?a"/>""",
                """{"a":"DISK-SHADOW"}""",
            };
            yield return new object[]
            {
                "xsl:source-document",
                """<xsl:source-document href="{0}"><xsl:value-of select="root/child"/></xsl:source-document>""",
                "<root><child>DISK-SHADOW</child></root>",
            };
            yield return new object[]
            {
                "xsl:source-document (streamable)",
                """<xsl:source-document href="{0}" streamable="yes"><xsl:value-of select="root/child"/></xsl:source-document>""",
                "<root><child>DISK-SHADOW</child></root>",
            };
        }
    }

    [Theory]
    [MemberData(nameof(DiskFallbackRoutes))]
    public void Route_DenyAll_WithValidFileOnDisk_RefusedAndFileNeverRead(
        string routeName, string bodyTemplate, string diskContent)
    {
        var path = Path.Combine(Path.GetTempPath(), $"bosak-req125-{Guid.NewGuid():N}.dat");
        File.WriteAllText(path, diskContent, Utf8);
        try
        {
            var uri = new Uri(path).AbsoluteUri;
            var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Deny() };
            var policy = new ControlledResourcePolicy(authority);

            if (routeName.Contains("available"))
            {
                // Availability probes return the spec-appropriate false without any IO.
                var result = Run(Wrap(string.Format(bodyTemplate, uri)), policy);
                Assert.Contains(">false<", result);
                Assert.Contains(policy.Receipts, r => r.Outcome == ControlledResourceReceiptOutcome.Denied);
            }
            else
            {
                var ex = Assert.Throws<InvalidOperationException>(() => Run(Wrap(string.Format(bodyTemplate, uri)), policy));
                Assert.StartsWith("XV0004", ex.Message);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DocAvailable_DenyAll_WithValidFileOnDisk_ReturnsFalseWithoutReadingFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bosak-req125-{Guid.NewGuid():N}.xml");
        File.WriteAllText(path, "<root><child>DISK-SHADOW</child></root>", Utf8);
        try
        {
            var uri = new Uri(path).AbsoluteUri;
            var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Deny() };
            var policy = new ControlledResourcePolicy(authority);

            var result = Run(Wrap("""<xsl:value-of select="doc-available('{0}')"/>""".Replace("{0}", uri)), policy);

            Assert.Contains(">false<", result);
            var receipt = Assert.Single(policy.Receipts);
            Assert.Equal(ControlledResourceRoute.Document, receipt.Route);
            Assert.Equal(ControlledResourceReceiptOutcome.Denied, receipt.Outcome);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DocAvailable_Approved_ReturnsTrue()
    {
        var authority = new StubAuthority
        {
            Handler = _ => ControlledResourceResponse.Approve(Utf8.GetBytes("<root/>")),
        };
        var policy = new ControlledResourcePolicy(authority);

        var result = Run(Wrap("""<xsl:value-of select="doc-available('urn:test:doc')"/>"""), policy);

        Assert.Contains(">true<", result);
    }

    // ------------------------------------------------------------------
    // Nested-context inheritance: fn:transform and xsl:evaluate
    // ------------------------------------------------------------------

    [Fact]
    public void FnTransform_NestedTransform_InheritsPolicy_AcquisitionsAuthorized()
    {
        const string nested = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template name="go">
                <xsl:value-of select="doc('urn:test:inner')/root/child"/>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var body = """
            <xsl:value-of select="map:get(fn:transform(map{
                'stylesheet-location': 'urn:test:nested',
                'initial-template': xs:QName('go'),
                'delivery-format': 'serialized'
              }), 'output')"/>
            """;
        var authority = new StubAuthority
        {
            Handler = request => request.Uri switch
            {
                "urn:test:nested" => ControlledResourceResponse.Approve(Utf8.GetBytes(nested)),
                "urn:test:inner" => ControlledResourceResponse.Approve(Utf8.GetBytes("<root><child>INNER-OK</child></root>")),
                _ => null,
            },
        };
        var policy = new ControlledResourcePolicy(authority);

        var result = Run(Wrap(body), policy);

        Assert.Contains("INNER-OK", result);
        Assert.Contains(authority.Requests, r => r.Route == ControlledResourceRoute.Transform && r.Uri == "urn:test:nested");
        Assert.Contains(authority.Requests, r => r.Route == ControlledResourceRoute.Document && r.Uri == "urn:test:inner");
    }

    [Fact]
    public void FnTransform_NestedTransform_DenyAll_RefusesStylesheetAcquisitionWithoutFallback()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bosak-req125-{Guid.NewGuid():N}.xsl");
        File.WriteAllText(path, "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'/>", Utf8);
        try
        {
            var uri = new Uri(path).AbsoluteUri;
            var body = $$"""
                <xsl:value-of select="map:get(fn:transform(map{
                    'stylesheet-location': '{{uri}}',
                    'initial-template': xs:QName('go'),
                    'delivery-format': 'serialized'
                  }), 'output')"/>
                """;
            var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Deny() };
            var policy = new ControlledResourcePolicy(authority);

            var ex = Assert.Throws<InvalidOperationException>(() => Run(Wrap(body), policy));

            Assert.StartsWith("XV0004", ex.Message);
            Assert.Contains(authority.Requests, r => r.Route == ControlledResourceRoute.Transform);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void XslEvaluate_NestedEvaluation_InheritsPolicy()
    {
        var authority = new StubAuthority
        {
            Handler = _ => ControlledResourceResponse.Approve(Utf8.GetBytes("<root><child>EVAL-OK</child></root>")),
        };
        var policy = new ControlledResourcePolicy(authority);

        var result = Run(Wrap("""<xsl:evaluate xpath="'doc(&quot;urn:test:ev&quot;)/root/child'"/>"""), policy);

        Assert.Contains("EVAL-OK", result);
        Assert.Contains(authority.Requests, r => r.Uri == "urn:test:ev");
    }

    // ------------------------------------------------------------------
    // Static evaluation (use-when) route
    // ------------------------------------------------------------------

    [Fact]
    public void UseWhen_DocAvailable_DenyAll_ExcludesIncludeWithoutReadingDiskFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bosak-req125-{Guid.NewGuid():N}.xsl");
        File.WriteAllText(path, """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template name="inc">DISK</xsl:template>
            </xsl:stylesheet>
            """, Utf8);
        try
        {
            var uri = new Uri(path).AbsoluteUri;
            // The disk file exists and is valid: under the controlled profile the denied
            // doc-available probe must be false, the include must be elided — never read.
            var xsl = $$"""
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:include href="{{uri}}" use-when="doc-available('{{uri}}')"/>
                  <xsl:template match="/"><out>MAIN</out></xsl:template>
                </xsl:stylesheet>
                """;
            var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Deny() };
            var policy = new ControlledResourcePolicy(authority);

            var compiler = new XsltCompiler { ResourcePolicy = policy };
            var executable = compiler.Compile(xsl, "file:///policy-test/main.xsl");
            var context = new EvaluationContext { ResourcePolicy = policy };
            var result = executable.TransformToString(XDocumentProvider.ParseXml("<dummy/>"), context);

            Assert.Contains("MAIN", result);
            Assert.DoesNotContain("DISK", result);
            Assert.Contains(authority.Requests, r => r.Uri == uri);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void UseWhen_DocAvailable_Approved_IncludesModule()
    {
        const string module = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template name="inc">FROM-INCLUDE</xsl:template>
            </xsl:stylesheet>
            """;
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:include href="urn:test:mod" use-when="doc-available('urn:test:probe')"/>
              <xsl:template match="/"><out><xsl:call-template name="inc"/></out></xsl:template>
            </xsl:stylesheet>
            """;
        var authority = new StubAuthority
        {
            Handler = request => request.Uri switch
            {
                "urn:test:probe" => ControlledResourceResponse.Approve(Utf8.GetBytes("<root/>")),
                "urn:test:mod" => ControlledResourceResponse.Approve(Utf8.GetBytes(module)),
                _ => null,
            },
        };
        var policy = new ControlledResourcePolicy(authority);

        var compiler = new XsltCompiler { ResourcePolicy = policy };
        var executable = compiler.Compile(xsl, "file:///policy-test/main.xsl");
        var context = new EvaluationContext { ResourcePolicy = policy };
        var result = executable.TransformToString(XDocumentProvider.ParseXml("<dummy/>"), context);

        Assert.Contains("FROM-INCLUDE", result);
        Assert.Contains(authority.Requests, r => r.Uri == "urn:test:probe");
    }

    // ------------------------------------------------------------------
    // Package route (xsl:use-package and fn:transform package-name)
    // ------------------------------------------------------------------

    [Fact]
    public void UsePackage_DenyAll_WithValidFileOnDisk_RefusedNotReadFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bosak-req125-{Guid.NewGuid():N}.xsl");
        File.WriteAllText(path, """
            <xsl:package name="urn:test:pkg" package-version="1.0" version="3.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:variable name="v" select="1" visibility="public"/>
            </xsl:package>
            """, Utf8);
        Api.XsltFunctionLibrary.ClearPackages();
        Api.XsltFunctionLibrary.RegisterPackage("urn:test:pkg", "1.0", new Uri(path).AbsoluteUri);
        try
        {
            const string xsl = """
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:use-package name="urn:test:pkg" package-version="1.0"/>
                  <xsl:template match="/"><out/></xsl:template>
                </xsl:stylesheet>
                """;
            var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Deny() };
            var policy = new ControlledResourcePolicy(authority);

            var compiler = new XsltCompiler { ResourcePolicy = policy };
            var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl, "file:///policy-test/main.xsl"));

            Assert.StartsWith("XV0004", ex.Message);
            Assert.Contains("Package", ex.Message);
            var receipt = Assert.Single(policy.Receipts);
            Assert.Equal(ControlledResourceRoute.Package, receipt.Route);
        }
        finally
        {
            Api.XsltFunctionLibrary.ClearPackages();
            File.Delete(path);
        }
    }

    // ------------------------------------------------------------------
    // Schema route (xsl:import-schema location hints)
    // ------------------------------------------------------------------

    private const string SchemaDocument = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" targetNamespace="urn:test:schema" xmlns:t="urn:test:schema">
          <xs:element name="size" type="xs:int"/>
        </xs:schema>
        """;

    [Fact]
    public void ImportSchema_DenyAll_WithValidFileOnDisk_RefusedNotReadFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bosak-req125-{Guid.NewGuid():N}.xsd");
        File.WriteAllText(path, SchemaDocument, Utf8);
        try
        {
            var uri = new Uri(path).AbsoluteUri;
            var xsl = $$"""
                <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
                  <xsl:import-schema namespace="urn:test:schema" schema-location="{{uri}}"/>
                  <xsl:template match="/"><out/></xsl:template>
                </xsl:stylesheet>
                """;
            var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Deny() };
            var policy = new ControlledResourcePolicy(authority);

            var compiler = new XsltCompiler { SchemaAware = true, ResourcePolicy = policy };
            var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl, "file:///policy-test/main.xsl"));

            Assert.StartsWith("XV0004", ex.Message);
            Assert.Contains("Schema", ex.Message);
            var receipt = Assert.Single(policy.Receipts);
            Assert.Equal(ControlledResourceRoute.Schema, receipt.Route);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ImportSchema_ApprovedBytes_CompileFromMemory()
    {
        var authority = new StubAuthority
        {
            Handler = _ => ControlledResourceResponse.Approve(Utf8.GetBytes(SchemaDocument)),
        };
        var policy = new ControlledResourcePolicy(authority);

        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:import-schema namespace="urn:test:schema" schema-location="urn:test:schema.xsd"/>
              <xsl:template match="/"><out/></xsl:template>
            </xsl:stylesheet>
            """;
        var compiler = new XsltCompiler { SchemaAware = true, ResourcePolicy = policy };
        var executable = compiler.Compile(xsl, "file:///policy-test/main.xsl");
        var result = executable.TransformToString(XDocumentProvider.ParseXml("<dummy/>"));

        Assert.NotNull(result);
        // XmlSchemaSet re-reads a by-URI schema (add + compile); every acquisition of the
        // effective input is recorded.
        Assert.NotEmpty(policy.Receipts);
        Assert.All(policy.Receipts, r =>
        {
            Assert.Equal(ControlledResourceRoute.Schema, r.Route);
            Assert.Equal(ControlledResourceReceiptOutcome.Approved, r.Outcome);
        });
    }

    // ------------------------------------------------------------------
    // AC-05: runtime receipts across nested routes
    // ------------------------------------------------------------------

    [Fact]
    public void NestedTransform_ReceiptsDescribeEveryEffectiveInput()
    {
        const string nested = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template name="go"><xsl:value-of select="doc('urn:test:inner')/root/child"/></xsl:template>
            </xsl:stylesheet>
            """;
        var body = """
            <xsl:value-of select="map:get(fn:transform(map{
                'stylesheet-location': 'urn:test:nested',
                'initial-template': xs:QName('go'),
                'delivery-format': 'serialized'
              }), 'output')"/>
            """;
        var nestedBytes = Utf8.GetBytes(nested);
        var innerBytes = Utf8.GetBytes("<root><child>INNER-OK</child></root>");
        var authority = new StubAuthority
        {
            Handler = request => request.Uri switch
            {
                "urn:test:nested" => ControlledResourceResponse.Approve(nestedBytes),
                "urn:test:inner" => ControlledResourceResponse.Approve(innerBytes),
                _ => null,
            },
        };
        var policy = new ControlledResourcePolicy(authority);

        Run(Wrap(body), policy);

        var transformReceipt = Assert.Single(policy.Receipts, r => r.Route == ControlledResourceRoute.Transform);
        Assert.Equal("urn:test:nested", transformReceipt.RequestedUri);
        Assert.Equal(nestedBytes.Length, transformReceipt.ByteCount);
        Assert.Equal(
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(nestedBytes)).ToLowerInvariant(),
            transformReceipt.Sha256);

        var documentReceipt = Assert.Single(policy.Receipts, r => r.Route == ControlledResourceRoute.Document);
        Assert.Equal("urn:test:inner", documentReceipt.RequestedUri);
        Assert.Equal(innerBytes.Length, documentReceipt.ByteCount);
    }

    [Fact]
    public void RuntimeDeniedUriWithCredentials_MessageAndReceiptsAreRedacted()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bosak-req125-{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, "DISK-SHADOW", Utf8);
        try
        {
            var uri = new Uri(path).AbsoluteUri;
            var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Deny("denied by host") };
            var policy = new ControlledResourcePolicy(authority);

            var ex = Assert.Throws<InvalidOperationException>(
                () => Run(Wrap("""<xsl:value-of select="unparsed-text('https://user:secret@example.com/data.txt')"/>"""), policy));

            Assert.StartsWith("XV0004", ex.Message);
            Assert.DoesNotContain("secret", ex.Message);
            Assert.Contains("https://example.com/data.txt", ex.Message);

            var receipt = Assert.Single(policy.Receipts);
            Assert.Equal("https://example.com/data.txt", receipt.RequestedUri);
            Assert.Equal("denied by host", receipt.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
