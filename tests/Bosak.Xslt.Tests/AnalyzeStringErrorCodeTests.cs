// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 November 2026
// PURPOSE              : Tests scoped XSLT analyze-string codes and preserved XPath errors.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Checks instruction-specific regex reporting and adjacent XPath error boundaries.</summary>
public class AnalyzeStringErrorCodeTests
{
    private static readonly XNamespace Xsl = "http://www.w3.org/1999/XSL/Transform";
    private const string ErrorNamespace = "http://www.w3.org/2005/xqt-errors";

    /// <summary>Pattern validation and native regex construction report the XSLT QName.</summary>
    /// <param name="pattern">The effective regular expression.</param>
    /// <param name="flags">The effective flags.</param>
    /// <param name="expectedCode">The required instruction-specific error.</param>
    [Theory]
    [InlineData("[A-Z", "", "XTDE1140")]
    [InlineData("**", "", "XTDE1140")]
    [InlineData("a{2147483648}", "", "XTDE1140")]
    [InlineData("[A-Z]", "12345", "XTDE1145")]
    public void InvalidEffectiveRegexIsCaughtBySpecificXsltQName(string pattern, string flags, string expectedCode)
    {
        var content = Variable("pattern", pattern) + Variable("flags", flags)
            + Analyze("'abc'", "{$pattern}", "{$flags}");
        Assert.Equal(ErrorNamespace + "|" + expectedCode, Render(content, expectedCode));
    }

    /// <summary>Ordinary matching and non-matching segments retain their output.</summary>
    [Fact]
    public void ValidRegexRetainsSegmentOrder()
    {
        Assert.Equal("NMNM", Render(Analyze("'a1b2'", "[0-9]", "")));
    }

    /// <summary>The quote flag keeps otherwise invalid pattern syntax literal.</summary>
    [Fact]
    public void QuoteFlagRetainsLiteralPattern()
    {
        Assert.Equal("M", Render(Analyze("'['", "[", "q")));
    }

    /// <summary>Errors evaluating either AVT remain XPath errors, before regex preparation.</summary>
    /// <param name="attribute">The AVT that evaluates the failing XPath call.</param>
    /// <param name="code">The original XPath error code.</param>
    [Theory]
    [InlineData("regex", "FORX0002")]
    [InlineData("flags", "FORX0002")]
    [InlineData("regex", "FORX0001")]
    [InlineData("flags", "FORX0001")]
    public void AttributeExpressionErrorsRemainXPathErrors(string attribute, string code)
    {
        string expression = code == "FORX0002"
            ? "{matches($input, $badPattern)}"
            : "{matches($input, 'x', $badFlags)}";
        string content = Variable("input", "x") + Variable("badPattern", "[") + Variable("badFlags", "z")
            + Analyze("$input", attribute == "regex" ? expression : "[a-z]", attribute == "flags" ? expression : "");
        Assert.Equal(ErrorNamespace + "|" + code, Render(content, code));
    }

    /// <summary>Both child callbacks retain XPath errors in tree and typed-sequence evaluation.</summary>
    /// <param name="matching">Whether the matching or non-matching child is evaluated.</param>
    /// <param name="typed">Whether the template builds a typed raw sequence.</param>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public void ChildExpressionErrorsRemainXPathErrors(bool matching, bool typed)
    {
        string code = matching ? "FORX0002" : "FORX0001";
        string expression = matching ? "matches($input, $badPattern)" : "matches($input, 'x', $badFlags)";
        string content = Variable("input", "x") + Variable("badPattern", "[") + Variable("badFlags", "z")
            + Analyze("$input", matching ? "[a-z]" : "[0-9]", "", matching ? expression : null, matching ? null : expression);
        Assert.Equal(ErrorNamespace + "|" + code, Render(content, code, typed));
    }

    private static string Variable(string name, string value) => new XElement(Xsl + "variable",
        new XAttribute("name", name), new XAttribute("select", "'" + value.Replace("'", "''") + "'")).ToString(SaveOptions.DisableFormatting);

    private static string Analyze(string input, string pattern, string flags, string? matchingExpression = null, string? nonMatchingExpression = null)
    {
        XElement Child(string name, string? expression, string text) => new(Xsl + name,
            expression == null ? new XElement(Xsl + "text", text) : new XElement(Xsl + "value-of", new XAttribute("select", expression)));
        return new XElement(Xsl + "analyze-string", new XAttribute("select", input), new XAttribute("regex", pattern), new XAttribute("flags", flags),
            Child("matching-substring", matchingExpression, "M"), Child("non-matching-substring", nonMatchingExpression, "N")).ToString(SaveOptions.DisableFormatting);
    }

    private static string Render(string content, string? caughtCode = null, bool typed = false)
    {
        var nodes = XElement.Parse("<body xmlns:xsl='" + Xsl + "' xmlns:err='" + ErrorNamespace + "'>" + content + "</body>").Nodes().ToArray();
        var template = new XElement(Xsl + "template", new XAttribute("name", "main"));
        if (typed) template.Add(new XAttribute("as", "item()*"));
        if (caughtCode == null) template.Add(nodes);
        else template.Add(new XElement(Xsl + "try", nodes,
            new XElement(Xsl + "catch", new XAttribute("errors", "err:" + caughtCode),
                new XElement(Xsl + "value-of", new XAttribute("select", "concat(namespace-uri-from-QName($err:code), '|', local-name-from-QName($err:code))")))));
        var stylesheet = new XElement(Xsl + "stylesheet", new XAttribute("version", "3.0"),
            new XAttribute(XNamespace.Xmlns + "xsl", Xsl.NamespaceName), new XAttribute(XNamespace.Xmlns + "err", ErrorNamespace),
            new XElement(Xsl + "output", new XAttribute("method", "text")), template);
        return new XsltCompiler().Compile(new XDocument(stylesheet)).TransformToString(null, initialTemplate: "main");
    }
}
