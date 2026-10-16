// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies static for-each attributes and required select validation.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies static for-each attributes and required select validation.</summary>
public class ForEachStaticValidationTests
{
    private static string Wrap(string body, string version = "3.0") =>
        "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:test'>" + body + "</xsl:stylesheet>";
    private static string Run(string body, string version = "3.0") => XElement.Parse(new Api.XsltCompiler().Compile(Wrap(body,version))
        .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;
    private static string Body(string attributes) => "<xsl:template match='/'><out><xsl:for-each " + attributes + "><xsl:value-of select='.'/></xsl:for-each></out></xsl:template>";

    /// <summary>Rejects unknown unqualified attributes even with a valid select.</summary>
    /// <param name="version">The stylesheet version.</param>
    /// <param name="attribute">The illegal attribute.</param>
    [Theory]
    [InlineData("1.0", "array")]
    [InlineData("2.0", "bind-to")]
    [InlineData("3.0", "array")]
    [InlineData("3.0", "bogus")]
    public void UnknownAttributes(string version, string attribute)
    {
        var error = Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(Wrap(Body("select='1' " + attribute + "='1'"),version)));
        Assert.Contains("XTSE0090",error.ToString());
    }

    /// <summary>Reports missing select during compilation, including unreachable instruction bodies.</summary>
    /// <param name="version">The stylesheet version.</param>
    /// <param name="unreachable">Whether the instruction is behind a false condition.</param>
    [Theory]
    [InlineData("3.0", false)]
    [InlineData("3.0", true)]
    [InlineData("4.0", false)]
    [InlineData("4.0", true)]
    public void MissingSelect(string version, bool unreachable)
    {
        var instruction = "<xsl:for-each/>";
        if (unreachable) instruction = "<xsl:if test='false()'>" + instruction + "</xsl:if>";
        var error = Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(Wrap("<xsl:template match='/'>" + instruction + "</xsl:template>",version)));
        Assert.Contains("XTSE0010",error.ToString());
    }

    /// <summary>Does not mistake a foreign select attribute for the required select.</summary>
    [Fact]
    public void ForeignSelect() => Assert.Contains("XTSE0010", Assert.ThrowsAny<Exception>(() =>
        new Api.XsltCompiler().Compile(Wrap(Body("t:select='1'")))).ToString());

    /// <summary>Reports illegal legacy attributes before the missing required select.</summary>
    [Fact]
    public void OriginalUpstreamShape() => Assert.Contains("XTSE0090", Assert.ThrowsAny<Exception>(() =>
        new Api.XsltCompiler().Compile(Wrap(Body("array='(1,(5,6),10)' bind-to='m'")))).ToString());

    /// <summary>Preserves separators and established forward-compatible attribute handling.</summary>
    /// <param name="version">The stylesheet version.</param>
    /// <param name="extra">An optional ignored future attribute.</param>
    [Theory]
    [InlineData("3.0", "")]
    [InlineData("4.0", "")]
    [InlineData("5.0", "future='x'")]
    public void Supported(string version, string extra) => Assert.Equal("1|2|3", Run(Body("select='1 to 3' separator='|' " + extra),version));

    /// <summary>Preserves standard, XML and foreign attributes plus sorted evaluation.</summary>
    [Fact]
    public void StandardAndForeign() => Assert.Equal("12",Run("""
        <xsl:template match='/'><out><xsl:for-each select='2,1' version='3.0' expand-text='yes'
          xpath-default-namespace='urn:test' default-collation='http://www.w3.org/2005/xpath-functions/collation/codepoint'
          xml:lang='en' t:hint='x'><xsl:sort select='.'/>{.}</xsl:for-each></out></xsl:template>
        """));

    /// <summary>Does not validate instructions removed by use-when.</summary>
    [Fact]
    public void Excluded() => Assert.Equal("ok", Run("<xsl:template match='/'><out><xsl:for-each use-when='false()' bogus='1'/>ok</out></xsl:template>"));

    /// <summary>Resolves shadow select attributes before required-attribute validation.</summary>
    [Fact]
    public void ShadowSelect() => Assert.Equal("12", Run(Body("_select='1 to 2'"),"4.0"));

    /// <summary>Uses an instruction's local earlier version for unknown-attribute checks.</summary>
    [Fact]
    public void LocalVersion() => Assert.Contains("XTSE0090", Assert.ThrowsAny<Exception>(() =>
        new Api.XsltCompiler().Compile(Wrap(Body("select='1' version='3.0' bogus='1'"),"4.0"))).ToString());
}
