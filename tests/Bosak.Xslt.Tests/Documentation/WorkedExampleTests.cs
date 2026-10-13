// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 October 2026
// PURPOSE              : Verifies publishing and reconciliation chapters against their published fixtures.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-10-2026     | Creation: executable publishing and reconciliation chapter evidence.                    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Capture;
using Xunit;

namespace Bosak.Xslt.Tests.Documentation;

/// <summary>Verifies publishing and reconciliation chapters against their published fixtures.</summary>
public class WorkedExampleTests
{
    private static string Folder(string chapter) => Path.Combine(AppContext.BaseDirectory,
        "Documentation", "ReferenceExamples", chapter);

    private static XsltCaptureResult Run(string chapter, string stylesheet, XDocument source)
    {
        var path = Path.Combine(Folder(chapter), stylesheet);
        return new XsltCompiler().Compile(File.ReadAllText(path), new Uri(path).AbsoluteUri)
            .CaptureTransform(new XDocumentNode(source), new XsltCaptureOptions
            {
                BaseOutputUri = "https://example.invalid/reference/principal.xml",
            });
    }

    private static XElement Parse(string text, bool mixedContent = false)
    {
        // These authored HTML fixtures intentionally contain no HTML void elements.
        text = Regex.Replace(text, @"<!DOCTYPE\s+html\s*>", "", RegexOptions.IgnoreCase);
        var root = XElement.Parse(text, mixedContent ? LoadOptions.PreserveWhitespace : LoadOptions.None);
        root.DescendantsAndSelf().Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
        return root;
    }

    private static string Text(XsltCapturedOutput output) => Encoding.UTF8.GetString(output.Bytes.ToArray());

    private static void Compare(string chapter, string expected, XsltCapturedOutput output, bool mixedContent = false)
    {
        var wanted = Parse(File.ReadAllText(Path.Combine(Folder(chapter), "fixtures", expected)), mixedContent);
        var actual = Parse(Text(output), mixedContent);
        Assert.True(XNode.DeepEquals(wanted, actual), $"Expected:\n{wanted}\nActual:\n{actual}");
    }

    /// <summary>Both manual cases preserve complete HTML structure and mixed text.</summary>
    /// <param name="input">The manual fixture.</param>
    /// <param name="expected">The expected HTML document.</param>
    [Theory]
    [InlineData("manual.xml", "expected.html")]
    [InlineData("broken-reference.xml", "expected-broken.html")]
    public void Manual_MatchesPublishedHtml(string input, string expected)
    {
        const string chapter = "manual-to-html";
        var source = XDocument.Load(Path.Combine(Folder(chapter), "fixtures", input), LoadOptions.PreserveWhitespace);
        var result = Run(chapter, "manual-to-html.xsl", source);
        Assert.True(result.IsSuccess, string.Join("; ", result.Failures.Select(f => f.Message)));
        Assert.Empty(result.SecondaryOutputs);
        Compare(chapter, expected, result.Principal!, mixedContent: true);
        Assert.StartsWith("<!DOCTYPE", Text(result.Principal!).TrimStart());
        var html = Parse(Text(result.Principal!), mixedContent: true);
        var ids = html.Descendants("section").Select(e => (string)e.Attribute("id")!).ToHashSet();
        Assert.All(html.Descendants("a"), link => Assert.Contains(((string)link.Attribute("href")!)[1..], ids));
    }

    /// <summary>Duplicate section IDs fail instead of generating ambiguous links.</summary>
    [Fact]
    public void Manual_DuplicateIdsRefusesPublication()
    {
        const string chapter = "manual-to-html";
        var source = XDocument.Load(Path.Combine(Folder(chapter), "fixtures", "manual.xml"));
        XNamespace ns = "urn:example:manual";
        source.Descendants(ns + "section").Last().SetAttributeValue("id", "safety");
        var result = Run(chapter, "manual-to-html.xsl", source);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Principal);
    }

    /// <summary>Complete and incomplete order cases match reports and exceptions.</summary>
    /// <param name="input">The order fixture.</param>
    /// <param name="report">The expected reconciliation report.</param>
    /// <param name="exceptions">The expected exception document.</param>
    [Theory]
    [InlineData("orders.xml", "expected-report.xml", "expected-exceptions.xml")]
    [InlineData("incomplete-orders.xml", "expected-incomplete-report.xml", "expected-incomplete-exceptions.xml")]
    public void Reconciliation_MatchesPublishedOutputs(string input, string report, string exceptions)
    {
        const string chapter = "order-reconciliation";
        var source = XDocument.Load(Path.Combine(Folder(chapter), "fixtures", input));
        var result = Run(chapter, "order-reconciliation.xsl", source);
        Assert.True(result.IsSuccess, string.Join("; ", result.Failures.Select(f => f.Message)));
        Compare(chapter, report, result.Principal!);
        var secondary = Assert.Single(result.SecondaryOutputs);
        Assert.Equal("https://example.invalid/reference/exceptions.xml", secondary.ResolvedUri);
        Compare(chapter, exceptions, secondary);
    }

    /// <summary>Ambiguous order identities refuse the entire reconciliation.</summary>
    [Fact]
    public void Reconciliation_DuplicateOrderIdsRefusesBatch()
    {
        const string chapter = "order-reconciliation";
        var source = XDocument.Load(Path.Combine(Folder(chapter), "fixtures", "orders.xml"));
        XNamespace ns = "urn:example:orders";
        source.Descendants(ns + "order").Last().SetAttributeValue("id", "O1");
        Assert.False(Run(chapter, "order-reconciliation.xsl", source).IsSuccess);
    }
}
