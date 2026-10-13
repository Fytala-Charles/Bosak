// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies structural record aliases across XSLT and XPath boundaries.
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

/// <summary>Verifies structural record aliases across XSLT and XPath boundaries.</summary>
public class Xslt40StructuralRecordAliasTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:f='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string body, string source = "<root/>") => new Api.XsltCompiler().Compile(Wrap(body))
        .TransformToString(new XDocumentNode(XDocument.Parse(source)));
    private const string Types = "<xsl:item-type name='t:reading' as='record(value as t:number,label as xs:string?)'/><xsl:item-type name='t:number' as='xs:double'/>";

    /// <summary>Resolves field aliases in the declaration namespace and function boundaries.</summary>
    [Fact]
    public void RecordAlias_DeclarationNamespacesAndFunctions()
        => Assert.Equal("true|21", XElement.Parse(Run("""
            <xsl:item-type name='t:r' xmlns:n='urn:numbers' as='record(value as n:value)'/>
            <xsl:item-type name='Q{urn:numbers}value' as='xs:double'/>
            <xsl:function name='f:normalize' as='t:r'>
              <xsl:param name='r' as='t:r'/><xsl:sequence select='$r'/>
            </xsl:function>
            <xsl:template match='/'>
              <xsl:variable name='r' select="f:normalize(map{'value':21})"/>
              <out><xsl:value-of separator='|' select='($r instance of t:r,$r?value)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Expands nested record fields and preserves untyped field defaults.</summary>
    [Fact]
    public void RecordAlias_NestedFieldsAndComments()
        => Assert.Equal("true|true", XElement.Parse(Run("""
            <xsl:item-type name='t:r' as='record (:outer:) (child as record(value as t:n), free)'/>
            <xsl:item-type name='t:n' as='xs:double'/>
            <xsl:template match='/'>
              <xsl:variable name='r' as='t:r' select="map{'child':map{'value':21},'free':(1,2)}"/>
              <out><xsl:value-of separator='|' select='($r instance of t:r,$r?child?value instance of t:n)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Converts XML sensor values to a typed structural record.</summary>
    [Fact]
    public void RecordAlias_SensorExample()
        => Assert.Equal("21|true", XElement.Parse(Run(Types + """
            <xsl:template match='/'>
              <xsl:variable name='r' as='t:reading' select="map{'value':root/@value}"/>
              <out><xsl:value-of separator='|' select='($r?value,$r instance of t:reading)'/></out>
            </xsl:template>
            """, "<root value='21'/>")).Value);

    /// <summary>Expands inline record fields and checks without coercing plain maps.</summary>
    [Fact]
    public void RecordAlias_InlineTypeChecks()
        => Assert.Equal("true|false|21", XElement.Parse(Run(Types + """
            <xsl:template match='/'>
              <xsl:variable name='r' as='record(value as t:number)' select="map{'value':21}"/>
              <out><xsl:value-of separator='|' select="($r instance of record(value as t:number),
                map{'value':21} instance of t:reading,($r treat as record(value as t:number))?value)"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Expands record aliases nested in arrays and callback signatures.</summary>
    [Fact]
    public void RecordAlias_NestedTypes()
        => Assert.Equal("true|true", XElement.Parse(Run(Types + """
            <xsl:template match='/'>
              <xsl:variable name='a' as='array(t:reading)' select="[map{'value':21}]"/>
              <xsl:variable name='f' as='function() as t:reading' select="fn(){map{'value':22}}"/>
              <out><xsl:value-of separator='|' select='($a?1 instance of t:reading,$f() instance of t:reading)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Preserves quoted punctuation and doubled quotes in field names.</summary>
    [Fact]
    public void RecordAlias_LiteralFieldNames()
        => Assert.Equal("true", XElement.Parse(Run("""
            <xsl:item-type name='t:r' as="record('a,as|b' as t:n,'can''t' as t:n,)"/>
            <xsl:item-type name='t:n' as='xs:double'/>
            <xsl:template match='/'>
              <xsl:variable name='r' as='t:r' select="map{'a,as|b':1,'can''t':2}"/>
              <out><xsl:value-of select='$r instance of t:r'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Rejects missing required fields, surplus keys and invalid field values.</summary>
    /// <param name="expression">The map expression violating the contract.</param>
    [Theory]
    [InlineData("map{}")]
    [InlineData("map{'value':1,'extra':2}")]
    [InlineData("map{'value':true()}")]
    public void RecordAlias_InvalidBinding(string expression)
        => Assert.Contains("XTTE0570", Assert.ThrowsAny<Exception>(() => Run(Types +
            "<xsl:template match='/'><xsl:variable name='r' as='t:reading' select=\"" + expression +
            "\"/><xsl:sequence select='$r'/></xsl:template>")).ToString());

    /// <summary>Detects circular aliases inside record field declarations.</summary>
    [Fact]
    public void RecordAlias_Cycle()
        => Assert.Contains("XTSE4035", Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:item-type name='t:r' as='record(value as t:r)'/>
            <xsl:template match='/'><out/></xsl:template>
            """)).ToString());

    /// <summary>Preserves empty and wildcard record forms and optional alias uses.</summary>
    [Fact]
    public void RecordAlias_EmptyWildcardAndOccurrences()
        => Assert.Equal("true|true|true", XElement.Parse(Run("""
            <xsl:item-type name='t:empty' as='record()'/>
            <xsl:item-type name='t:any' as='record(*)'/>
            <xsl:template match='/'>
              <xsl:variable name='r' as='t:empty' select='map{}'/>
              <xsl:variable name='absent' as='t:empty?' select='()'/>
              <out><xsl:value-of separator='|' select='($r instance of t:empty,$r instance of t:any,$absent instance of t:empty?)'/></out>
            </xsl:template>
            """)).Value);
}
