// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verify fixed namespace resolution for named template declarations and calls.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>Tests applicable namespaces on named templates and call sites.</summary>
public class Xslt40TemplateQNameTests
{
    private static string Sheet(string body, string bindings) => "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' fixed-namespaces='" + bindings + "'>" + body + "</xsl:stylesheet>";
    private static XsltValidationResult Validate(string text) => XsltValidation.Validate(Encoding.UTF8.GetBytes(text), new Uri("file:///templates/main.xsl"));

    /// <summary>Fixed-only and rebound names resolve to the same template identity.</summary>
    /// <param name="declaration">The declared name.</param>
    /// <param name="call">The called name.</param>
    [Theory]
    [InlineData("q:target", "q:target")]
    [InlineData("p:target", "q:target")]
    [InlineData("Q{urn:fixed}target", "q:target")]
    [InlineData("q:target", "Q{urn:fixed}target")]
    [InlineData("target", "Q{}target")]
    public void FixedNamesExecute(string declaration, string call)
    {
        var text = Sheet("<xsl:template name='" + declaration + "'>7</xsl:template><xsl:template match='/'><out><xsl:call-template name='" + call + "'/></out></xsl:template>", "#standard p=urn:fixed q=urn:fixed");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        var result = new Api.XsltCompiler().Compile(text).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
        Assert.Equal("7", XElement.Parse(result).Value);
    }

    /// <summary>Excluded or malformed template names fail at their source location.</summary>
    /// <param name="body">The declaration or call.</param>
    /// <param name="code">The expected static error.</param>
    [Theory]
    [InlineData("<xsl:template name='p:target'/>", "XTSE0280")]
    [InlineData("<xsl:template match='/'><xsl:call-template name='p:target'/></xsl:template>", "XTSE0280")]
    [InlineData("<xsl:template name='a:b:c'/>", "XTSE0020")]
    [InlineData("<xsl:template match='/'><xsl:call-template name='a:b:c'/></xsl:template>", "XTSE0020")]
    public void InvalidNamesAreRejected(string body, string code)
    {
        var text = Sheet(body, "#standard");
        Assert.Contains(Validate(text).Diagnostics, d => d.Code == code);
        Assert.Contains(code, Assert.Throws<InvalidOperationException>(() => new Api.XsltCompiler().Compile(text)).Message);
    }

    /// <summary>Reserved namespaces are checked after applying fixed bindings.</summary>
    [Fact]
    public void FixedReservedNamespaceIsRejected()
    {
        var text = Sheet("<xsl:template name='p:target'/>", "#standard p=http://www.w3.org/2005/xpath-functions");
        Assert.Contains("XTSE0080", Assert.Throws<InvalidOperationException>(() => new Api.XsltCompiler().Compile(text)).Message);
        Assert.Contains(Validate(text).Diagnostics, d => d.Code == "XTSE0080");
    }
    /// <summary>Earlier host profiles preserve native component namespace bindings.</summary>
    /// <param name="version">The stylesheet version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierProfilesRemainNative(string version)
    {
        var text = Sheet("<xsl:template name='p:target'>7</xsl:template><xsl:template match='/'><out><xsl:call-template name='Q{urn:native}target'/></out></xsl:template>", "#standard").Replace("version='4.0'", "version='" + version + "'").Replace(" fixed-namespaces='#standard'", "");
        var result = new Api.XsltCompiler().Compile(text).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
        Assert.Equal("7", XElement.Parse(result).Value);
    }

    /// <summary>The standard initial template remains legal under fixed namespaces.</summary>
    [Fact]
    public void InitialTemplateRemainsLegal()
    {
        var text = Sheet("<xsl:template name='xsl:initial-template'><out>7</out></xsl:template>", "#standard");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        _ = new Api.XsltCompiler().Compile(text);
    }

    /// <summary>Literal static exclusion keeps invalid names outside validation.</summary>
    [Fact]
    public void ExcludedTemplateIsIgnored()
    {
        var text = Sheet("<xsl:template name='p:target' use-when='false()'/><xsl:template match='/'><out/></xsl:template>", "#standard");
        Assert.Equal(XsltValidationOutcome.Valid, Validate(text).Outcome);
        _ = new Api.XsltCompiler().Compile(text);
    }

}
