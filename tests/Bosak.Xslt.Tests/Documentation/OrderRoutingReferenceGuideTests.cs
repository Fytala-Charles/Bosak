// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 October 2026
// PURPOSE              : Verifies order dispatch and refusal contracts against published fixtures.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-10-2026     | Creation: executable REQ-126 order-routing chapter evidence.                                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 11-10-2026     | Verify standalone XSLT40 routing against identical fixtures and identity refusals.       |
// ===========================================================================================================================================================

using System.Text;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Capture;
using Xunit;

namespace Bosak.Xslt.Tests.Documentation;

/// <summary>Verifies order dispatch and refusal contracts against published fixtures.</summary>
public class OrderRoutingReferenceGuideTests
{
    private static readonly string Chapter = Path.Combine(AppContext.BaseDirectory,
        "Documentation", "ReferenceExamples", "order-routing");

    private static XsltCaptureResult Run(string input)
    {
        var path = Path.Combine(Chapter, "order-routing.xsl");
        var source = new XDocumentNode(XDocument.Load(Path.Combine(Chapter, "fixtures", input)));
        var plan = XPath31Expression.Compile(File.ReadAllText(Path.Combine(Chapter, "routing-plan-40.xpath")),
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 })
            .Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(source), 1, 1));
        return new XsltCompiler().Compile(File.ReadAllText(path), new Uri(path).AbsoluteUri)
            .CaptureTransform(source,
                new XsltCaptureOptions { BaseOutputUri = "https://example.invalid/order-routing/dispatch.xml",
                    Context = new EvaluationContext().WithVariable("plan", plan) });
    }

    private static void Compare(string fixture, XsltCapturedOutput output)
    {
        var expected = XElement.Load(Path.Combine(Chapter, "fixtures", fixture));
        var actual = XElement.Parse(Encoding.UTF8.GetString(output.Bytes.ToArray()));
        expected.DescendantsAndSelf().Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
        actual.DescendantsAndSelf().Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
        Assert.True(XNode.DeepEquals(expected, actual), $"Expected:\n{expected}\nActual:\n{actual}");
    }

    /// <summary>Source order, capacity, rejection reasons and empty boundaries match complete outputs.</summary>
    /// <param name="input">The published input fixture.</param>
    /// <param name="principal">The expected dispatch document.</param>
    /// <param name="exceptions">The expected repair document.</param>
    [Theory]
    [InlineData("orders.xml", "expected-dispatch.xml", "expected-exceptions.xml")]
    [InlineData("invalid-orders.xml", "expected-invalid-dispatch.xml", "expected-invalid-exceptions.xml")]
    [InlineData("empty-orders.xml", "expected-empty-dispatch.xml", "expected-empty-exceptions.xml")]
    public void PublishedFixtures_MatchDispatchAndExceptions(string input, string principal, string exceptions)
    {
        var result = Run(input);
        Assert.True(result.IsSuccess, string.Join("; ", result.Failures.Select(f => f.Message)));
        Assert.Empty(result.Failures);
        Assert.NotNull(result.Principal);
        Assert.Equal("https://example.invalid/order-routing/dispatch.xml", result.Principal.ResolvedUri);
        Compare(principal, result.Principal);
        var secondary = Assert.Single(result.SecondaryOutputs);
        Assert.Equal("https://example.invalid/order-routing/exceptions.xml", secondary.ResolvedUri);
        Compare(exceptions, secondary);
    }

    /// <summary>The published planning expression refuses default XPath 3.1 compilation.</summary>
    [Fact]
    public void Planning_RequiresExplicitXPath40()
    {
        var code = File.ReadAllText(Path.Combine(Chapter, "routing-plan-40.xpath"));
        var failure = Assert.ThrowsAny<Exception>(() => XPath31Expression.Compile(code));
        Assert.Contains("XPST0017", failure.Message);
    }

    /// <summary>Ambiguous or missing identities refuse dispatch before either document is constructed.</summary>
    /// <param name="input">The invalid identity fixture.</param>
    [Theory]
    [InlineData("duplicate-orders.xml")]
    [InlineData("missing-id-orders.xml")]
    public void InvalidIdentity_RefusesBothOutputs(string input)
    {
        var failure = Assert.ThrowsAny<Exception>(() => Run(input));
        Assert.Contains("ROUTING-ID", failure.Message);
    }

    private static XsltCaptureResult RunStandalone(string input)
    {
        var path = Path.Combine(Chapter, "order-routing-40.xsl");
        return new XsltCompiler().Compile(File.ReadAllText(path), new Uri(path).AbsoluteUri)
            .CaptureTransform(new XDocumentNode(XDocument.Load(Path.Combine(Chapter, "fixtures", input))),
                new XsltCaptureOptions { BaseOutputUri = "https://example.invalid/order-routing/dispatch.xml" });
    }

    /// <summary>The self-contained 4.0 stylesheet matches both documents from the two-stage workflow.</summary>
    /// <param name="input">The source fixture.</param>
    /// <param name="principal">The expected dispatch document.</param>
    /// <param name="exceptions">The expected repair document.</param>
    [Theory]
    [InlineData("orders.xml", "expected-dispatch.xml", "expected-exceptions.xml")]
    [InlineData("invalid-orders.xml", "expected-invalid-dispatch.xml", "expected-invalid-exceptions.xml")]
    [InlineData("empty-orders.xml", "expected-empty-dispatch.xml", "expected-empty-exceptions.xml")]
    public void Standalone40_MatchesPublishedOutputs(string input, string principal, string exceptions)
    {
        var result = RunStandalone(input);
        Assert.True(result.IsSuccess, string.Join("; ", result.Failures.Select(f => f.Message)));
        Assert.NotNull(result.Principal);
        Compare(principal, result.Principal);
        var secondary = Assert.Single(result.SecondaryOutputs);
        Assert.Equal("https://example.invalid/order-routing/exceptions.xml", secondary.ResolvedUri);
        Compare(exceptions, secondary);
    }

    /// <summary>Identity refusals happen before standalone XSLT produces either output.</summary>
    /// <param name="input">The ambiguous identity fixture.</param>
    [Theory]
    [InlineData("duplicate-orders.xml")]
    [InlineData("missing-id-orders.xml")]
    public void Standalone40_InvalidIdentityRefusesBothOutputs(string input)
    {
        var result = RunStandalone(input);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Principal);
        Assert.Empty(result.SecondaryOutputs);
        Assert.Contains(result.Failures, failure => failure.Message.Contains("ROUTING-ID", StringComparison.Ordinal));
    }

}
