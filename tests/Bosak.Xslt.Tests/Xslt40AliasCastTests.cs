// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies named aliases in cast targets and declaration validation.
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
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies named alias casts and declaration-time named reference validation.</summary>
[Collection("XSLT package registry")]
public class Xslt40AliasCastTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string body) => new Api.XsltCompiler().Compile(Wrap(body))
        .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));

    /// <summary>Casts sensor readings through atomic and nested container aliases.</summary>
    /// <param name="type">The declared alias definition.</param>
    /// <param name="expression">The expression evaluated using the alias.</param>
    /// <param name="expected">The expected serialized value.</param>
    [Theory]
    [InlineData("xs:integer", "'42' cast as t:value", "42")]
    [InlineData("xs:integer", "'bad' castable as t:value", "false")]
    [InlineData("xs:integer", "() castable as t:value?", "true")]
    [InlineData("array(xs:integer)", "(['42'] cast as t:value)(1)", "42")]
    [InlineData("array(xs:integer)", "['bad'] castable as t:value", "false")]
    [InlineData("map(xs:string,xs:integer)", "(map{'reading':'42'} cast as t:value)?reading", "42")]
    [InlineData("record(reading as xs:integer)", "(map{'reading':'42'} cast as t:value)?reading", "42")]
    [InlineData("enum('active','idle')", "'active' cast as t:value", "active")]
    [InlineData("enum('active','idle')", "'broken' castable as t:value", "false")]
    public void Cast_Alias(string type, string expression, string expected)
        => Assert.Equal(expected, XElement.Parse(Run($"<xsl:item-type name='t:value' as=\"{type}\"/><xsl:template match='/'><out><xsl:value-of select=\"{expression}\"/></out></xsl:template>")).Value);

    /// <summary>Expands aliases inside direct container casts and rebound namespace forms.</summary>
    [Fact]
    public void Cast_NestedAndNamespaceAliases()
        => Assert.Equal("42|43|44", XElement.Parse(Run("""
            <xsl:item-type name='t:number' as='xs:integer'/>
            <xsl:template match='/' xmlns:v='urn:types' xpath-default-namespace='urn:types'>
              <out><xsl:value-of separator='|' select="((['42'] cast as array(v:number))(1),
                '43' cast as number,'44' cast as Q{urn:types}number)"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Reports unknown names even in unused declarations and nested signatures.</summary>
    /// <param name="definition">The invalid alias definition.</param>
    [Theory]
    [InlineData("t:missing")]
    [InlineData("array(t:missing)")]
    [InlineData("function(t:missing) as xs:integer")]
    public void Declaration_UnknownType(string definition)
        => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(
            Wrap($"<xsl:item-type name='t:value' as='{definition}'/>"))).ToString());

    /// <summary>Rejects function and generic item aliases as casting targets.</summary>
    /// <param name="definition">The ineligible alias definition.</param>
    /// <param name="operation">The casting operation.</param>
    [Theory]
    [InlineData("function(*)", "cast")]
    [InlineData("function(xs:integer) as xs:integer", "castable")]
    [InlineData("item()", "cast")]
    [InlineData("node()", "castable")]
    public void Cast_InvalidTarget(string definition, string operation)
        => Assert.Contains("XPST0080", Assert.ThrowsAny<Exception>(() => Run(
            $"<xsl:item-type name='t:value' as='{definition}'/><xsl:template match='/'><xsl:sequence select=\"() {operation} as t:value\"/></xsl:template>")).ToString());

    /// <summary>Preserves nominative identity when an alias refers to a declared record type.</summary>
    [Fact]
    public void Cast_NamedRecordAlias()
        => Assert.Equal("42|true", XElement.Parse(Run("""
            <xsl:record-type name='t:sensor'><xsl:field name='reading' as='xs:integer'/></xsl:record-type>
            <xsl:item-type name='t:value' as='t:sensor'/>
            <xsl:template match='/'><xsl:variable name='s' select="map{'reading':'42'} cast as t:value"/>
              <out><xsl:value-of separator='|' select='($s?reading,$s instance of t:sensor)'/></out>
            </xsl:template>
            """)).Value);
}
