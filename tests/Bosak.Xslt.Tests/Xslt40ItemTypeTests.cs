// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 12 October 2026
// PURPOSE              : Verifies enumeration aliases in XSLT XPath type expressions.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 12-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies enumeration aliases in XSLT XPath type expressions.</summary>
public class Xslt40ItemTypeTests
{
    private static string Run(string body) => new Api.XsltCompiler().Compile("""
        <xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
          xmlns:t="urn:status" exclude-result-prefixes="#all">
        """ + body + "</xsl:stylesheet>").TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));

    /// <summary>Matches only string members and preserves occurrence indicators.</summary>
    [Fact]
    public void Enumeration_MembersAndCardinality()
    {
        var output = XElement.Parse(Run("""
            <xsl:item-type name="t:status" as="enum('ready','queued')"/>
            <xsl:template match="/">
              <out good="{'ready' instance of t:status}" bad="{'lost' instance of t:status}"
                number="{1 instance of t:status}" empty="{() instance of t:status?}"
                many="{('ready','queued') instance of t:status+}" single="{('ready','queued') instance of t:status}"/>
            </xsl:template>
            """));
        Assert.Equal(new[] { "true", "false", "false", "true", "true", "false" }, output.Attributes().Select(a => a.Value));
    }

    /// <summary>Resolves locally rebound prefixes and braced URI names by expanded identity.</summary>
    [Fact]
    public void Enumeration_NamespaceRebinding()
    {
        var output = XElement.Parse(Run("""
            <xsl:item-type name="t:status" as="enum('ready')"/>
            <xsl:item-type xmlns:t="urn:other" name="t:status" as="enum('queued')"/>
            <xsl:template xmlns:u="urn:other" match="/">
              <out primary="{'ready' instance of t:status}" other="{'queued' instance of u:status}"
                crossed="{'ready' instance of u:status}" braced="{'queued' instance of Q{urn:other}status}"/>
            </xsl:template>
            """));
        Assert.Equal(new[] { "true", "true", "false", "true" }, output.Attributes().Select(a => a.Value));
    }

    /// <summary>Uses the XPath default type namespace and leaves string contents unchanged.</summary>
    [Fact]
    public void Enumeration_DefaultNamespaceAndTreat()
    {
        var output = XElement.Parse(Run("""
            <xsl:item-type xpath-default-namespace="urn:status" name="status" as="enum('t:status','ready')"/>
            <xsl:template match="/" xpath-default-namespace="urn:status">
              <out value="{'ready' treat as status}" literal="{'t:status' instance of status}"/>
            </xsl:template>
            """));
        Assert.Equal("ready", (string?)output.Attribute("value"));
        Assert.Equal("true", (string?)output.Attribute("literal"));
    }

    /// <summary>Rejects values outside the declared enumeration in a treat expression.</summary>
    [Fact]
    public void Enumeration_TreatRejectsUnknownMember()
    {
        var error = Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:item-type name="t:status" as="enum('ready')"/>
            <xsl:template match="/"><xsl:sequence select="'lost' treat as t:status"/></xsl:template>
            """));
        Assert.Contains("XPDY0050", error.ToString());
    }

    /// <summary>Rejects invalid declarations and same-precedence duplicates.</summary>
    /// <param name="declarations">Invalid declarations preceding the template.</param>
    /// <param name="code">Expected static error code.</param>
    [Theory]
    [InlineData("<xsl:item-type name='t:status'/>", "XTSE0010")]
    [InlineData("<xsl:item-type name='t:status' as=\"enum('ready')\"><xsl:text>x</xsl:text></xsl:item-type>", "XTSE0260")]
    [InlineData("<xsl:item-type name='t:status' as=\"enum('ready')\"/><xsl:item-type name='t:status' as=\"enum('queued')\"/>", "XTSE4030")]
    public void Enumeration_InvalidDeclarations(string declarations, string code)
    {
        var error = Assert.ThrowsAny<Exception>(() => Run(declarations + "<xsl:template match='/'><out ok=\"{'ready' instance of t:status}\"/></xsl:template>"));
        Assert.Contains(code, error.ToString());
    }
}
