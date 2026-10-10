// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Regression tests for REQ-125 Slice B consumer-review findings B-F1 (cache/route enforcement) and B-F2 (static-evaluation route).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice B consumer-review corrections B-F1/B-F2)                         |
//                      | Charles Korthout | 0.2   | 10-10-2026     | B-F3: digest-validated controlled cache hits — fresh parses on byte/effective-URI change |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Security.Cryptography;
using System.Text;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Resources;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>
/// Required regressions for the two acceptance blockers raised by the Bosak.Braid consumer
/// review of the REQ-125 Slice B controlled-resource-policy delivery
/// (docs/REQ-125-SLICE-B-CONSUMER-REVIEW.md):
/// B-F1 — the document cache must never bypass the requested route's unsupported-route check
/// (XV0005) or route-specific authority decision (XV0004), and cache hits under a policy still
/// record acquisition receipts; B-F2 — static-phase acquisition (use-when evaluation and static
/// expressions) must request the <see cref="ControlledResourceRoute.StaticEvaluation"/> purpose
/// route with the concrete operation (Document/TextResource/JsonResource/Collection) preserved
/// on requests and receipts, so a host's declared static restriction governs.
/// </summary>
public class ControlledResourceReviewFindingTests
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false);

    /// <summary>Records every request and answers through a swappable handler; null = abstain.</summary>
    internal sealed class StubAuthority : IControlledResourceAuthority
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

    private static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    /// <summary>Compiles and runs under the same controlled policy, mirroring the reviewer probes.</summary>
    private static string Run(string xsl, ControlledResourcePolicy policy)
    {
        var executable = new XsltCompiler { ResourcePolicy = policy }.Compile(xsl, "file:///policy-test/main.xsl");
        var context = new EvaluationContext { ResourcePolicy = policy };
        return executable.TransformToString(XDocumentProvider.ParseXml("<dummy/>"), context);
    }

    private const string DocBytesXml = "<root><child>DOC-OK</child></root>";

    // ------------------------------------------------------------------
    // B-F1: the document cache never bypasses route policy
    // ------------------------------------------------------------------

    [Fact]
    public void BF1_UnsupportedRouteAfterApprovedDocument_RefusesXv0005DespiteCache()
    {
        // Verbatim reviewer probe: the authority approves; SourceDocument is declared
        // unsupported. The first load populates the cache; the second must still refuse.
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(Utf8.GetBytes("<approved/>")) };
        var policy = new ControlledResourcePolicy(authority, new[] { ControlledResourceRoute.SourceDocument });
        var context = new EvaluationContext { ResourcePolicy = policy };

        var approved = context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);
        Assert.NotNull(approved);

        var ex = Assert.Throws<InvalidOperationException>(
            () => context.LoadDocument("urn:review:input", ControlledResourceRoute.SourceDocument));
        Assert.StartsWith("XV0005", ex.Message);

        // Receipts: one approval (Document) and one route refusal (SourceDocument). The
        // unsupported-route check fires before the authority is consulted.
        var receipts = policy.Receipts;
        Assert.Equal(2, receipts.Count);
        Assert.Equal(ControlledResourceReceiptOutcome.Approved, receipts[0].Outcome);
        Assert.Equal(ControlledResourceRoute.Document, receipts[0].Route);
        Assert.Equal(ControlledResourceReceiptOutcome.RefusedUnsupported, receipts[1].Outcome);
        Assert.Equal(ControlledResourceRoute.SourceDocument, receipts[1].Route);
        Assert.Single(authority.Requests);
    }

    [Fact]
    public void BF1_RouteSpecificDenial_RefusesXv0004EvenWhenAnotherRoutePopulatedCache()
    {
        var docBytes = Utf8.GetBytes(DocBytesXml);
        var authority = new StubAuthority
        {
            Handler = request => request.Route == ControlledResourceRoute.SourceDocument
                ? ControlledResourceResponse.Deny("source documents not approved")
                : ControlledResourceResponse.Approve(docBytes),
        };
        var policy = new ControlledResourcePolicy(authority);
        var context = new EvaluationContext { ResourcePolicy = policy };

        context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);

        var ex = Assert.Throws<InvalidOperationException>(
            () => context.LoadDocument("urn:review:input", ControlledResourceRoute.SourceDocument));
        Assert.StartsWith("XV0004", ex.Message);
        Assert.Contains("source documents not approved", ex.Message);

        var receipt = Assert.Single(policy.Receipts, r => r.Route == ControlledResourceRoute.SourceDocument);
        Assert.Equal(ControlledResourceReceiptOutcome.Denied, receipt.Outcome);
        Assert.Equal("source documents not approved", receipt.Message);
    }

    [Fact]
    public void BF1_PolicyChange_DoesNotExposeEarlierCachedContent()
    {
        var firstBytes = Utf8.GetBytes(DocBytesXml);
        var approving = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(firstBytes) };
        var policyA = new ControlledResourcePolicy(approving);
        var context = new EvaluationContext { ResourcePolicy = policyA };

        var cached = context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);
        Assert.NotNull(cached);

        // Swapping the policy must not unlock content acquired under the earlier policy:
        // the new authority decides, and its denial refuses despite the warm cache.
        var denying = new StubAuthority { Handler = _ => ControlledResourceResponse.Deny("not approved") };
        var policyB = new ControlledResourcePolicy(denying);
        context.ResourcePolicy = policyB;

        var ex = Assert.Throws<InvalidOperationException>(
            () => context.LoadDocument("urn:review:input", ControlledResourceRoute.Document));
        Assert.StartsWith("XV0004", ex.Message);
        var receipt = Assert.Single(policyB.Receipts);
        Assert.Equal(ControlledResourceReceiptOutcome.Denied, receipt.Outcome);
    }

    [Fact]
    public void BF1_SamePolicySameRoute_CacheHitReturnsIdenticalNodeAndRecordsReceipt()
    {
        var docBytes = Utf8.GetBytes(DocBytesXml);
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(docBytes) };
        var policy = new ControlledResourcePolicy(authority);
        var context = new EvaluationContext { ResourcePolicy = policy };

        var first = context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);
        var second = context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);

        Assert.Same(first, second);
        // The cache avoids re-parsing, not the policy decision: every access consults the
        // authority and records an approval receipt carrying the authoritative evidence.
        Assert.Equal(2, authority.Requests.Count);
        Assert.Equal(2, policy.Receipts.Count);
        Assert.All(policy.Receipts, r =>
        {
            Assert.Equal(ControlledResourceReceiptOutcome.Approved, r.Outcome);
            Assert.Equal(docBytes.Length, r.ByteCount);
            Assert.Equal(Sha256(docBytes), r.Sha256);
        });
    }

    [Fact]
    public void BF1_Stylesheet_DocAndSourceDocument_SameUri_DenialRefusesSourceRoute()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <out>
                  <doc><xsl:value-of select="doc('urn:review:input')/root/child"/></doc>
                  <src><xsl:source-document href="urn:review:input"><xsl:value-of select="root/child"/></xsl:source-document></src>
                </out>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var docBytes = Utf8.GetBytes(DocBytesXml);
        var authority = new StubAuthority
        {
            Handler = request => request.Route == ControlledResourceRoute.SourceDocument
                ? ControlledResourceResponse.Deny("source documents not approved")
                : ControlledResourceResponse.Approve(docBytes),
        };
        var policy = new ControlledResourcePolicy(authority);

        var ex = Assert.Throws<InvalidOperationException>(() => Run(xsl, policy));
        Assert.StartsWith("XV0004", ex.Message);
        Assert.Contains(authority.Requests, r => r.Route == ControlledResourceRoute.SourceDocument);
    }

    [Fact]
    public void BF1_Stylesheet_DocAndSourceDocument_SameUri_ApprovedRoutesShareNothingTheyDidNotApprove()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <out>
                  <doc><xsl:value-of select="doc('urn:review:input')/root/child"/></doc>
                  <src><xsl:source-document href="urn:review:input"><xsl:value-of select="root/child"/></xsl:source-document></src>
                </out>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var docBytes = Utf8.GetBytes(DocBytesXml);
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(docBytes) };
        var policy = new ControlledResourcePolicy(authority);

        var result = Run(xsl, policy);

        Assert.Contains("<doc>DOC-OK</doc>", result);
        Assert.Contains("<src>DOC-OK</src>", result);
        Assert.All(policy.Receipts, r => Assert.Equal(ControlledResourceReceiptOutcome.Approved, r.Outcome));
    }

    // ------------------------------------------------------------------
    // B-F2: static-phase acquisition requests the StaticEvaluation route
    // ------------------------------------------------------------------

    private const string UseWhenDocAvailableStylesheet = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/" use-when="doc-available('urn:review:static')">
            <ok/>
          </xsl:template>
        </xsl:stylesheet>
        """;

    [Fact]
    public void BF2_UseWhenDocAvailable_StaticEvaluationUnsupported_ElidesAndRecordsRefusal()
    {
        // Verbatim reviewer probe: an approving authority must not be able to approve through
        // a phase the host declared unsupported; the use-when availability probe is false and
        // the element is elided — no fallback IO.
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(Utf8.GetBytes(DocBytesXml)) };
        var policy = new ControlledResourcePolicy(authority, new[] { ControlledResourceRoute.StaticEvaluation });

        var result = Run(UseWhenDocAvailableStylesheet, policy);

        Assert.DoesNotContain("ok", result);
        var receipt = Assert.Single(policy.Receipts);
        Assert.Equal(ControlledResourceRoute.StaticEvaluation, receipt.Route);
        Assert.Equal(ControlledResourceRoute.Document, receipt.ConcreteRoute);
        Assert.Equal(ControlledResourceReceiptOutcome.RefusedUnsupported, receipt.Outcome);
        Assert.Equal("XV0005", receipt.Message);
        // The unsupported-route check fires before the authority is consulted.
        Assert.Empty(authority.Requests);
    }

    [Fact]
    public void BF2_StaticApproval_ReceiptAndRequestCarryPurposeAndConcreteRoute()
    {
        var staticBytes = Utf8.GetBytes(DocBytesXml);
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(staticBytes) };
        var policy = new ControlledResourcePolicy(authority);

        var result = Run(UseWhenDocAvailableStylesheet, policy);

        Assert.Contains("ok", result);
        // The use-when expression is evaluated on every compilation pass; each evaluation is
        // a separate acquisition (and receipt) under the decide-before-serve contract.
        Assert.NotEmpty(authority.Requests);
        Assert.All(authority.Requests, request =>
        {
            Assert.Equal(ControlledResourceRoute.StaticEvaluation, request.Route);
            Assert.Equal(ControlledResourceRoute.Document, request.ConcreteRoute);
        });
        Assert.NotEmpty(policy.Receipts);
        Assert.All(policy.Receipts, receipt =>
        {
            Assert.Equal(ControlledResourceRoute.StaticEvaluation, receipt.Route);
            Assert.Equal(ControlledResourceRoute.Document, receipt.ConcreteRoute);
            Assert.Equal(ControlledResourceReceiptOutcome.Approved, receipt.Outcome);
            Assert.Equal(staticBytes.Length, receipt.ByteCount);
            Assert.Equal(Sha256(staticBytes), receipt.Sha256);
        });
    }

    [Fact]
    public void BF2_StaticUnparsedTextAvailable_UnsupportedStaticRoute_ElidesWithFalse()
    {
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/" use-when="unparsed-text-available('urn:review:text')">
                <ok/>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(Utf8.GetBytes("TEXT-OK")) };
        var policy = new ControlledResourcePolicy(authority, new[] { ControlledResourceRoute.StaticEvaluation });

        var result = Run(xsl, policy);

        Assert.DoesNotContain("ok", result);
        var receipt = Assert.Single(policy.Receipts);
        Assert.Equal(ControlledResourceRoute.StaticEvaluation, receipt.Route);
        Assert.Equal(ControlledResourceRoute.TextResource, receipt.ConcreteRoute);
        Assert.Equal(ControlledResourceReceiptOutcome.RefusedUnsupported, receipt.Outcome);
    }

    [Fact]
    public void BF2_StaticUnparsedTextAvailable_Approved_IncludesWithPurposeAndConcreteRoute()
    {
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/" use-when="unparsed-text-available('urn:review:text')">
                <ok/>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(Utf8.GetBytes("TEXT-OK")) };
        var policy = new ControlledResourcePolicy(authority);

        var result = Run(xsl, policy);

        Assert.Contains("ok", result);
        // One receipt per use-when evaluation pass (see the doc-available approval test).
        Assert.NotEmpty(policy.Receipts);
        Assert.All(policy.Receipts, receipt =>
        {
            Assert.Equal(ControlledResourceRoute.StaticEvaluation, receipt.Route);
            Assert.Equal(ControlledResourceRoute.TextResource, receipt.ConcreteRoute);
            Assert.Equal(ControlledResourceReceiptOutcome.Approved, receipt.Outcome);
        });
    }

    [Fact]
    public void BF2_StaticJsonDoc_UnsupportedStaticRoute_RefusesExplicitly()
    {
        // json-doc is not an availability function: an unsupported static phase refuses
        // explicitly (XV0005) rather than approving through the forbidden phase. There is
        // no fallback IO and the receipt records the route pair.
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/" use-when="json-doc('urn:review:json')?flag = true()">
                <ok/>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var jsonBytes = Utf8.GetBytes("""{"flag":true}""");
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(jsonBytes) };
        var policy = new ControlledResourcePolicy(authority, new[] { ControlledResourceRoute.StaticEvaluation });

        var ex = Assert.Throws<InvalidOperationException>(() => Run(xsl, policy));
        Assert.StartsWith("XV0005", ex.Message);

        var receipt = Assert.Single(policy.Receipts);
        Assert.Equal(ControlledResourceRoute.StaticEvaluation, receipt.Route);
        Assert.Equal(ControlledResourceRoute.JsonResource, receipt.ConcreteRoute);
        Assert.Equal(ControlledResourceReceiptOutcome.RefusedUnsupported, receipt.Outcome);
    }

    [Fact]
    public void BF2_RuntimeDocumentAccess_UnaffectedByStaticRestriction()
    {
        // The same policy that forbids static-phase acquisition permits ordinary runtime
        // document access: doc() and doc-available() keep the Document route with no
        // concrete-route override.
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <out>
                  <a><xsl:value-of select="doc-available('urn:review:runtime')"/></a>
                  <b><xsl:value-of select="doc('urn:review:runtime')/root/child"/></b>
                </out>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var docBytes = Utf8.GetBytes(DocBytesXml);
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(docBytes) };
        var policy = new ControlledResourcePolicy(authority, new[] { ControlledResourceRoute.StaticEvaluation });

        var result = Run(xsl, policy);

        Assert.Contains(">true<", result);
        Assert.Contains("<b>DOC-OK</b>", result);
        Assert.All(policy.Receipts, r =>
        {
            Assert.Equal(ControlledResourceRoute.Document, r.Route);
            Assert.Null(r.ConcreteRoute);
            Assert.Equal(ControlledResourceReceiptOutcome.Approved, r.Outcome);
        });
    }

    [Fact]
    public void BF2_NestedCompile_UseWhenStaticRouteGoverns()
    {
        // fn:transform compiling a stylesheet inside a transform: the nested compile's
        // use-when evaluation inherits the policy AND the static phase, so the host's
        // declared StaticEvaluation restriction governs the nested static path too.
        const string nested = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template name="go">
                <r><xsl:text use-when="doc-available('urn:review:static')">STATIC-IN</xsl:text></r>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var body = """
            <xsl:value-of select="map:get(fn:transform(map{
                'stylesheet-location': 'urn:review:nested',
                'initial-template': xs:QName('go'),
                'delivery-format': 'serialized'
              }), 'output')"/>
            """;
        var outer = $$"""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:fn="http://www.w3.org/2005/xpath-functions">
              <xsl:template match="/">
                <out>{{body}}</out>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var authority = new StubAuthority
        {
            Handler = request => request.Uri switch
            {
                "urn:review:nested" => ControlledResourceResponse.Approve(Utf8.GetBytes(nested)),
                "urn:review:static" => ControlledResourceResponse.Approve(Utf8.GetBytes(DocBytesXml)),
                _ => null,
            },
        };
        var policy = new ControlledResourcePolicy(authority, new[] { ControlledResourceRoute.StaticEvaluation });

        var result = Run(outer, policy);

        Assert.DoesNotContain("STATIC-IN", result);
        Assert.Contains(authority.Requests, r => r.Route == ControlledResourceRoute.Transform && r.Uri == "urn:review:nested");
        var receipt = Assert.Single(policy.Receipts, r => r.Route == ControlledResourceRoute.StaticEvaluation);
        Assert.Equal(ControlledResourceRoute.Document, receipt.ConcreteRoute);
        Assert.Equal(ControlledResourceReceiptOutcome.RefusedUnsupported, receipt.Outcome);
    }

    [Fact]
    public void BF2_NestedCompile_StaticApproved_StaticContentIncluded()
    {
        const string nested = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template name="go">
                <r><xsl:text use-when="doc-available('urn:review:static')">STATIC-IN</xsl:text></r>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var body = """
            <xsl:value-of select="map:get(fn:transform(map{
                'stylesheet-location': 'urn:review:nested',
                'initial-template': xs:QName('go'),
                'delivery-format': 'serialized'
              }), 'output')"/>
            """;
        var outer = $$"""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:fn="http://www.w3.org/2005/xpath-functions">
              <xsl:template match="/">
                <out>{{body}}</out>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var authority = new StubAuthority
        {
            Handler = request => request.Uri switch
            {
                "urn:review:nested" => ControlledResourceResponse.Approve(Utf8.GetBytes(nested)),
                "urn:review:static" => ControlledResourceResponse.Approve(Utf8.GetBytes(DocBytesXml)),
                _ => null,
            },
        };
        var policy = new ControlledResourcePolicy(authority);

        var result = Run(outer, policy);

        Assert.Contains("STATIC-IN", result);
        var receipt = Assert.Single(policy.Receipts, r => r.Route == ControlledResourceRoute.StaticEvaluation);
        Assert.Equal(ControlledResourceRoute.Document, receipt.ConcreteRoute);
        Assert.Equal(ControlledResourceReceiptOutcome.Approved, receipt.Outcome);
    }

    // ------------------------------------------------------------------
    // B-F3: digest-validated controlled cache hits — receipts and content always agree
    // ------------------------------------------------------------------

    private const string OldApprovedBytes = "<approved>OLD</approved>";
    private const string ChangedBytes = "<changed/>";

    // Digests as published by the Bosak.Braid re-review (docs/REQ-125-SLICE-B-CONSUMER-REVIEW.md §4).
    private const string OldApprovedDigest = "7af55314edb57d23ea0ecb44ce645725c0434cc8d8ba6cec21dec39b788de943";
    private const string ChangedDigest = "a6f97c6663c45f978a3d5ca749ec1a9aacaa11851902e08203ebe0cf36485f47";

    [Fact]
    public void BF3_ChangingAuthorityContent_FreshNodeAndTruthfulReceipt()
    {
        // Verbatim reviewer probe: one policy, one context, an authority whose approved
        // content changes between loads. The second load must return content matching the
        // new digest — never the stale node with a fresh-bytes receipt.
        var current = Utf8.GetBytes(OldApprovedBytes);
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(current) };
        var policy = new ControlledResourcePolicy(authority);
        var context = new EvaluationContext { ResourcePolicy = policy };

        var first = context.LoadDocument("urn:review:changing", ControlledResourceRoute.Document);
        Assert.Equal("OLD", first.StringValue);
        var firstReceipt = Assert.Single(policy.Receipts);
        Assert.False(firstReceipt.ServedFromCache);
        Assert.Equal(OldApprovedDigest, firstReceipt.Sha256);
        Assert.Equal(ControlledResourceReceiptOutcome.Approved, firstReceipt.Outcome);

        current = Utf8.GetBytes(ChangedBytes);
        var second = context.LoadDocument("urn:review:changing", ControlledResourceRoute.Document);
        Assert.NotSame(first, second);
        Assert.Equal(string.Empty, second.StringValue);
        var secondReceipt = policy.Receipts[1];
        Assert.False(secondReceipt.ServedFromCache);
        Assert.Equal(ChangedDigest, secondReceipt.Sha256);

        // A further unchanged load is a digest-validated hit: identity with the second node.
        var third = context.LoadDocument("urn:review:changing", ControlledResourceRoute.Document);
        Assert.Same(second, third);
        var thirdReceipt = policy.Receipts[2];
        Assert.True(thirdReceipt.ServedFromCache);
        Assert.Equal(ChangedDigest, thirdReceipt.Sha256);
    }

    [Fact]
    public void BF3_SameContentRepeatLoad_PreservesIdentityAndMarksReceiptCacheHit()
    {
        var docBytes = Utf8.GetBytes(DocBytesXml);
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(docBytes) };
        var policy = new ControlledResourcePolicy(authority);
        var context = new EvaluationContext { ResourcePolicy = policy };

        var first = context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);
        var second = context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);

        Assert.Same(first, second);
        Assert.Equal(2, policy.Receipts.Count);
        Assert.False(policy.Receipts[0].ServedFromCache);
        Assert.True(policy.Receipts[1].ServedFromCache);
        Assert.Equal(policy.Receipts[0].Sha256, policy.Receipts[1].Sha256);
        Assert.Equal(Sha256(docBytes), policy.Receipts[1].Sha256);
    }

    [Fact]
    public void BF3_ChangedEffectiveUri_RefreshesNodeAndReceipt()
    {
        // The effective URI is part of a cache entry's identity: approving the same bytes
        // under a different effective URI must not serve the stale node with a receipt that
        // names the new URI.
        var docBytes = Utf8.GetBytes(DocBytesXml);
        var effective = "https://eff.example/a";
        var authority = new StubAuthority
        {
            Handler = _ => ControlledResourceResponse.Approve(docBytes, effective),
        };
        var policy = new ControlledResourcePolicy(authority);
        var context = new EvaluationContext { ResourcePolicy = policy };

        var first = context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);
        Assert.Equal("https://eff.example/a", first.BaseUri);
        var firstReceipt = Assert.Single(policy.Receipts);
        Assert.False(firstReceipt.ServedFromCache);
        Assert.Equal("https://eff.example/a", firstReceipt.EffectiveUri);

        effective = "https://eff.example/b";
        var second = context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);
        Assert.NotSame(first, second);
        Assert.Equal("https://eff.example/b", second.BaseUri);
        var secondReceipt = policy.Receipts[1];
        Assert.False(secondReceipt.ServedFromCache);
        Assert.Equal("https://eff.example/b", secondReceipt.EffectiveUri);
        Assert.Equal(Sha256(docBytes), secondReceipt.Sha256);
    }

    [Fact]
    public void BF3_DenialRemainsEnforcedOnWarmCache()
    {
        // The policy decision runs before the cache on every load: with a warm digest-valid
        // entry under the SAME policy and route, switching the authority to deny must refuse
        // XV0004 rather than serving cached content.
        var docBytes = Utf8.GetBytes(DocBytesXml);
        var approve = true;
        var authority = new StubAuthority
        {
            Handler = _ => approve
                ? ControlledResourceResponse.Approve(docBytes)
                : ControlledResourceResponse.Deny("not approved"),
        };
        var policy = new ControlledResourcePolicy(authority);
        var context = new EvaluationContext { ResourcePolicy = policy };

        var first = context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);
        var second = context.LoadDocument("urn:review:input", ControlledResourceRoute.Document);
        Assert.Same(first, second);
        Assert.True(policy.Receipts[1].ServedFromCache);

        approve = false;
        var ex = Assert.Throws<InvalidOperationException>(
            () => context.LoadDocument("urn:review:input", ControlledResourceRoute.Document));
        Assert.StartsWith("XV0004", ex.Message);
        Assert.Equal(3, policy.Receipts.Count);
        Assert.Equal(ControlledResourceReceiptOutcome.Denied, policy.Receipts[2].Outcome);
    }

    [Fact]
    public void BF3_Stylesheet_DocCalledTwice_SecondServeMarkedCacheHit()
    {
        // End-to-end: two doc() calls for the same URI inside one transform share document
        // identity, and the receipts distinguish the parse (ServedFromCache=false) from the
        // digest-validated cache serve (ServedFromCache=true).
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:template match="/">
                <out>
                  <a><xsl:value-of select="doc('urn:review:input')/root/child"/></a>
                  <b><xsl:value-of select="doc('urn:review:input')/root/child"/></b>
                  <same><xsl:value-of select="count(doc('urn:review:input') | doc('urn:review:input'))"/></same>
                </out>
              </xsl:template>
            </xsl:stylesheet>
            """;
        var docBytes = Utf8.GetBytes(DocBytesXml);
        var authority = new StubAuthority { Handler = _ => ControlledResourceResponse.Approve(docBytes) };
        var policy = new ControlledResourcePolicy(authority);

        var result = Run(xsl, policy);

        Assert.Contains("<a>DOC-OK</a>", result);
        Assert.Contains("<b>DOC-OK</b>", result);
        Assert.Contains("<same>1</same>", result);
        // Four doc() acquisitions: the first parses (ServedFromCache=false), the three
        // repeat loads are digest-validated cache serves (ServedFromCache=true) — and the
        // union count of 1 confirms they share one document identity.
        Assert.Equal(4, policy.Receipts.Count);
        Assert.False(policy.Receipts[0].ServedFromCache);
        Assert.All(policy.Receipts.Skip(1), r => Assert.True(r.ServedFromCache));
        Assert.All(policy.Receipts, r =>
        {
            Assert.Equal(ControlledResourceRoute.Document, r.Route);
            Assert.Equal(ControlledResourceReceiptOutcome.Approved, r.Outcome);
            Assert.Equal(Sha256(docBytes), r.Sha256);
        });
    }
}
