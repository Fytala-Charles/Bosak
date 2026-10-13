// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies nominative record coercion across XSLT and XPath boundaries.
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

/// <summary>Verifies nominative record coercion across XSLT and XPath boundaries.</summary>
public class Xslt40NominativeRecordCoercionTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:f='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string body, string source = "<root/>") => new Api.XsltCompiler().Compile(Wrap(body))
        .TransformToString(new XDocumentNode(XDocument.Parse(source)));
    private const string Types = "<xsl:record-type name='t:reading'><xsl:field name='value' as='xs:double'/><xsl:field name='label' as='xs:string' select=\"'sensor'\"/><xsl:field name='kind' static='yes' as='xs:string' select=\"'reading'\"/></xsl:record-type>";

    /// <summary>Applies defaults and static fields while giving the map named identity.</summary>
    [Fact]
    public void NominalCoercion_SensorExample()
        => Assert.Equal("21|sensor|reading|true", XElement.Parse(Run(Types + """
            <xsl:template match='/'>
              <xsl:variable name='r' as='t:reading' select="map{'value':root/@value}"/>
              <out><xsl:value-of separator='|' select='($r?value,$r?label,$r?kind,$r instance of t:reading)'/></out>
            </xsl:template>
            """, "<root value='21'/>")).Value);

    /// <summary>Coerces function arguments and returns, including named function references.</summary>
    [Fact]
    public void NominalCoercion_FunctionBoundaries()
        => Assert.Equal("true|true", XElement.Parse(Run(Types + """
            <xsl:function name='f:normalize' as='t:reading'><xsl:param name='r' as='t:reading'/><xsl:sequence select="$r"/></xsl:function>
            <xsl:function name='f:create' as='t:reading'><xsl:sequence select="map{'value':22}"/></xsl:function>
            <xsl:template match='/'>
              <out><xsl:value-of separator='|' select="(f:normalize#1(map{'value':21}) instance of t:reading,f:create() instance of t:reading)"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Rejects surplus keys, supplied static fields, missing required fields and non-maps.</summary>
    /// <param name="expression">The invalid supplied expression.</param>
    [Theory]
    [InlineData("map{'value':1,'extra':2}")]
    [InlineData("map{'value':1,'kind':'reading'}")]
    [InlineData("map{}")]
    [InlineData("1")]
    [InlineData("()")]
    public void NominalCoercion_InvalidBinding(string expression)
        => Assert.Contains("XTTE0570", Assert.ThrowsAny<Exception>(() => Run(Types +
            "<xsl:template match='/'><xsl:variable name='r' as='t:reading' select=\"" + expression +
            "\"/><xsl:sequence select='$r'/></xsl:template>")).ToString());

    /// <summary>Preserves matching identity and permits optional and multiple record items.</summary>
    [Fact]
    public void NominalCoercion_OccurrencesAndExistingIdentity()
        => Assert.Equal("true|true|true", XElement.Parse(Run(Types + """
            <xsl:template match='/'>
              <xsl:variable name='r' as='t:reading' select="map{'value':21}"/>
              <xsl:variable name='same' as='t:reading' select='$r'/>
              <xsl:variable name='many' as='t:reading+' select="($r,map{'value':22})"/>
              <xsl:variable name='absent' as='t:reading?' select='()'/>
              <out><xsl:value-of separator='|' select='($same instance of t:reading,$many instance of t:reading+,$absent instance of t:reading?)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Type checks retain the difference between plain maps and distinct declarations.</summary>
    [Fact]
    public void NominalCoercion_IdentityChecks()
        => Assert.Equal("false|false|true", XElement.Parse(Run("""
            <xsl:record-type name='t:a'><xsl:field name='id' as='xs:integer'/></xsl:record-type>
            <xsl:record-type name='t:b'><xsl:field name='id' as='xs:integer'/></xsl:record-type>
            <xsl:template match='/'>
              <xsl:variable name='a' as='t:a' select="map{'id':1}"/>
              <xsl:variable name='b' as='t:b' select='$a'/>
              <out><xsl:value-of separator='|' select="(map{'id':1} instance of t:a,$a instance of t:b,$b instance of t:b)"/></out>
            </xsl:template>
            """)).Value);
}
