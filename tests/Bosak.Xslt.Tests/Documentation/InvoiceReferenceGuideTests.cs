// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 October 2026
// PURPOSE              : Verifies the invoice reference chapter against its published fixtures.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-10-2026     | Creation: executable REQ-126 invoice chapter evidence.                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Parser;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Bosak.Xslt.Capture;
using Xunit;

namespace Bosak.Xslt.Tests.Documentation;

/// <summary>Verifies the invoice reference chapter against its published fixtures.</summary>
public class InvoiceReferenceGuideTests
{
    private static readonly string Chapter = Path.Combine(AppContext.BaseDirectory, "Documentation", "InvoiceToErp");
    private static readonly XNamespace Supplier = "urn:example:supplier:invoice";
    private static readonly XNamespace Erp = "urn:example:erp:import";

    private static string Read(string name) => File.ReadAllText(Path.Combine(Chapter, name));

    private static XsltCaptureResult Capture(XDocument source)
    {
        var path = Path.Combine(Chapter, "invoice-to-erp.xsl");
        var executable = new XsltCompiler().Compile(File.ReadAllText(path), new Uri(path).AbsoluteUri);
        return executable.CaptureTransform(new XDocumentNode(source), new XsltCaptureOptions
        {
            BaseOutputUri = "https://example.invalid/invoice-preview/import.xml",
        });
    }

    private static XElement Xml(XsltCapturedOutput output)
        => XDocument.Parse(Encoding.UTF8.GetString(output.Bytes.ToArray())).Root!;

    private static void AssertXml(string expectedFile, XsltCapturedOutput output)
    {
        var expected = XDocument.Parse(Read(expectedFile)).Root!;
        var actual = Xml(output);
        // Compare expanded XML names; redundant namespace declaration placement is lexical.
        expected.DescendantsAndSelf().Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
        actual.DescendantsAndSelf().Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
        Assert.True(XNode.DeepEquals(expected, actual), $"Expected:\n{expected}\nActual:\n{actual}");
    }

    /// <summary>Published complete and incomplete cases match both expected documents.</summary>
    /// <param name="input">The source fixture.</param>
    /// <param name="principal">The expected import document.</param>
    /// <param name="exceptions">The expected exceptions document.</param>
    [Theory]
    [InlineData("invoices.xml", "expected-import.xml", "expected-exceptions.xml")]
    [InlineData("incomplete-invoices.xml", "expected-incomplete-import.xml", "expected-incomplete-exceptions.xml")]
    public void PublishedFixtures_MatchPrincipalAndExceptions(string input, string principal, string exceptions)
    {
        var result = Capture(XDocument.Parse(Read(Path.Combine("fixtures", input))));
        Assert.True(result.IsSuccess, string.Join("; ", result.Failures.Select(f => f.Message)));
        Assert.Empty(result.Failures);
        Assert.NotNull(result.Principal);
        Assert.Equal("https://example.invalid/invoice-preview/import.xml", result.Principal.ResolvedUri);
        AssertXml(Path.Combine("fixtures", principal), result.Principal);
        var secondary = Assert.Single(result.SecondaryOutputs);
        Assert.Equal("https://example.invalid/invoice-preview/exceptions.xml", secondary.ResolvedUri);
        AssertXml(Path.Combine("fixtures", exceptions), secondary);
    }

    /// <summary>Invalid invoice headers reject the entire invoice before line casts.</summary>
    /// <param name="field">The invalid header field.</param>
    /// <param name="value">Its replacement value.</param>
    /// <param name="reason">The expected rejection reason.</param>
    [Theory]
    [InlineData("issue-date", "31/02/2026", "invalid-issue-date")]
    [InlineData("currency", "XYZ", "unsupported-currency")]
    public void InvalidHeader_ProducesInvoiceException(string field, string value, string reason)
    {
        var source = XDocument.Parse(Read("fixtures/invoices.xml"));
        var invoice = source.Root!.Element(Supplier + "invoice")!;
        if (field == "currency") invoice.SetAttributeValue(field, value);
        else invoice.Element(Supplier + field)!.Value = value;
        var result = Capture(source);
        Assert.True(result.IsSuccess);
        Assert.Empty(Xml(result.Principal!).Elements());
        var rejection = Assert.Single(Xml(Assert.Single(result.SecondaryOutputs)).Elements());
        Assert.Equal("invoice", (string?)rejection.Attribute("scope"));
        Assert.Equal(reason, (string?)rejection.Attribute("reason"));
    }

    /// <summary>Tax rounds once per numeric rate group, including differently spelled rates.</summary>
    [Fact]
    public void TaxRounding_IsPerRateGroup()
    {
        var source = XDocument.Parse(Read("fixtures/invoices.xml"));
        var lines = source.Descendants(Supplier + "line").ToArray();
        lines[2].Remove();
        foreach (var line in lines.Take(2))
        {
            line.Element(Supplier + "quantity")!.Value = "1";
            line.Element(Supplier + "unit-price")!.Value = "0.03";
        }
        var result = Capture(source);
        Assert.True(result.IsSuccess);
        var totals = Assert.Single(Xml(result.Principal!).Descendants(Erp + "totals"));
        Assert.Equal("0.06", (string?)totals.Attribute("net"));
        Assert.Equal("0.01", (string?)totals.Attribute("tax"));
        Assert.Equal("0.07", (string?)totals.Attribute("gross"));
        Assert.Single(Xml(result.Principal!).Descendants(Erp + "tax"));
    }

    /// <summary>An all-rejected invoice remains review-required with zero accepted totals.</summary>
    [Fact]
    public void AllLinesRejected_RemainsReviewRequired()
    {
        var source = XDocument.Parse(Read("fixtures/incomplete-invoices.xml"));
        source.Descendants(Supplier + "line").First().Element(Supplier + "sku")!.Remove();
        var result = Capture(source);
        Assert.True(result.IsSuccess);
        var invoice = Assert.Single(Xml(result.Principal!).Elements());
        Assert.Equal("review-required", (string?)invoice.Attribute("status"));
        var totals = invoice.Element(Erp + "totals")!;
        Assert.Equal("0", (string?)totals.Attribute("accepted-lines"));
        Assert.Equal("4", (string?)totals.Attribute("rejected-lines"));
        Assert.Equal("0.00", (string?)totals.Attribute("gross"));
        Assert.Equal(4, Xml(Assert.Single(result.SecondaryOutputs)).Elements().Count());
    }

    /// <summary>The published optional 4.0 label agrees with the 3.1 form and is gated.</summary>
    [Fact]
    public void InvoiceLabel_MigrationPairMatchesAndRequiresOptIn()
    {
        var earlier = XPath31Expression.Compile(Read("invoice-label-31.xpath"));
        var later = XPath31Expression.Compile(Read("invoice-label-40.xpath"),
            new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        Assert.Equal("INV-2026-1042 / EUR", earlier.Evaluate(new EvaluationContext()).ToString());
        Assert.Equal("INV-2026-1042 / EUR", later.Evaluate(new EvaluationContext()).ToString());
        var rejected = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile(Read("invoice-label-40.xpath")));
        Assert.Contains("XPST0003", rejected.Message);
    }
}
