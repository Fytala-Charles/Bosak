// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies named record casts and nested references across XSLT and XPath boundaries.
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

/// <summary>Verifies named record casts and nested references across XSLT and XPath boundaries.</summary>
public class Xslt40NamedRecordCastTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:f='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string body, string source = "<root/>") => new Api.XsltCompiler().Compile(Wrap(body))
        .TransformToString(new XDocumentNode(XDocument.Parse(source)));
    private const string Types = "<xsl:record-type name='t:reading'><xsl:field name='value' as='xs:double'/><xsl:field name='label' as='xs:string' select=\"'sensor'\"/><xsl:field name='kind' static='yes' as='xs:string' select=\"'reading'\"/></xsl:record-type>";

    /// <summary>Missing required fields produce a cast type error.</summary>
    [Fact]
    public void NamedCast_MissingRequiredField()
        => Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Run(Types + """
            <xsl:template match='/'><xsl:sequence select='map{} cast as t:reading'/></xsl:template>
            """)).ToString());

    /// <summary>A nonempty value cannot be cast to an explicitly empty field.</summary>
    [Fact]
    public void NamedCast_NonemptyEmptyField()
        => Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:record-type name='t:empty'><xsl:field name='nothing' as='empty-sequence()'/></xsl:record-type>
            <xsl:template match='/'><xsl:sequence select="map{'nothing':1} cast as t:empty"/></xsl:template>
            """)).ToString());

    /// <summary>An explicitly empty field type accepts an empty supplied value.</summary>
    [Fact]
    public void NamedCast_EmptySequenceField()
        => Assert.Equal("true", XElement.Parse(Run("""
            <xsl:record-type name='t:empty'><xsl:field name='nothing' as='empty-sequence()'/></xsl:record-type>
            <xsl:template match='/'>
              <xsl:variable name='r' select="map{'nothing':()} cast as t:empty"/>
              <out><xsl:value-of select='empty($r?nothing)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Function signatures resolve nested names using their own namespace context.</summary>
    [Fact]
    public void NamedCast_NestedFunctionSignatureNamespace()
        => Assert.Equal("true|21", XElement.Parse(Run("""
            <xsl:record-type name='t:reading'><xsl:field name='value' as='xs:double'/></xsl:record-type>
            <xsl:function xmlns:n='urn:types' name='f:normalize' as='array(n:reading)'>
              <xsl:param name='a' as='array(n:reading)'/><xsl:sequence select='$a'/>
            </xsl:function>
            <xsl:template match='/'>
              <xsl:variable name='a' select="f:normalize#1([map{'value':21}])"/>
              <out><xsl:value-of separator='|' select='($a?1 instance of t:reading,$a?1?value)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Castable rejects missing required fields and excessive input cardinality.</summary>
    [Fact]
    public void NamedCast_InvalidCardinalities()
        => Assert.Equal("false|false|false", XElement.Parse(Run(Types + """
            <xsl:template match='/'>
              <out><xsl:value-of separator='|' select="(map{} castable as t:reading,
                map{'value':(1,2)} castable as t:reading,
                (map{'value':1},map{'value':2}) castable as t:reading)"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Nested casts preserve optional fields and reject empty required sequences.</summary>
    [Fact]
    public void NamedCast_OptionalFieldSequences()
        => Assert.Equal("true|false", XElement.Parse(Run("""
            <xsl:record-type name='t:batch'><xsl:field name='values' as='xs:integer+'/>
              <xsl:field name='note' as='xs:string?'/></xsl:record-type>
            <xsl:template match='/'>
              <xsl:variable name='r' select="map{'values':('1','2'),'note':()} cast as t:batch"/>
              <out><xsl:value-of separator='|' select="(empty($r?note),map{'values':()} castable as t:batch)"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Casts textual field values and discards surplus source keys.</summary>
    [Fact]
    public void NamedCast_SensorProjection()
        => Assert.Equal("21|sensor|reading|true", XElement.Parse(Run(Types + """
            <xsl:template match='/'>
              <xsl:variable name='r' select="map{'value':'21','extra':99} cast as t:reading"/>
              <out><xsl:value-of separator='|' select='($r?value,$r?label,$r?kind,$r instance of t:reading)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Castable returns false on failed field casts and static-field injection.</summary>
    [Fact]
    public void NamedCast_Castable()
        => Assert.Equal("true|false|false|false|false|true", XElement.Parse(Run(Types + """
            <xsl:template match='/'>
              <out><xsl:value-of separator='|' select="(map{'value':'21'} castable as t:reading,
                map{'value':'bad'} castable as t:reading,map{'value':21,'kind':'reading'} castable as t:reading,
                1 castable as t:reading,() castable as t:reading,() castable as t:reading?)"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Invalid lexical values retain casting errors rather than binding errors.</summary>
    [Fact]
    public void NamedCast_InvalidLexicalValue()
        => Assert.Contains("FORG0001", Assert.ThrowsAny<Exception>(() => Run(Types + """
            <xsl:template match='/'><xsl:sequence select="map{'value':'bad'} cast as t:reading"/></xsl:template>
            """)).ToString());

    /// <summary>A cast rejects an externally supplied static field.</summary>
    [Fact]
    public void NamedCast_StaticInjection()
        => Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Run(Types + """
            <xsl:template match='/'><xsl:sequence select="map{'value':21,'kind':'reading'} cast as t:reading"/></xsl:template>
            """)).ToString());

    /// <summary>Existing records retain static values and optional casts accept empty input.</summary>
    [Fact]
    public void NamedCast_ExistingAndOptional()
        => Assert.Equal("true|reading|true", XElement.Parse(Run(Types + """
            <xsl:template match='/'>
              <xsl:variable name='a' as='t:reading' select="map{'value':21}"/>
              <xsl:variable name='b' select='$a cast as t:reading'/>
              <out><xsl:value-of separator='|' select='($b instance of t:reading,$b?kind,empty(() cast as t:reading?))'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Casts each member of a declared field sequence with cardinality checking.</summary>
    [Fact]
    public void NamedCast_SequenceFields()
        => Assert.Equal("1|2|true", XElement.Parse(Run("""
            <xsl:record-type name='t:batch'><xsl:field name='values' as='xs:integer+'/></xsl:record-type>
            <xsl:template match='/'>
              <xsl:variable name='r' select="map{'values':('1','2')} cast as t:batch"/>
              <out><xsl:value-of separator='|' select='($r?values,every $v in $r?values satisfies $v instance of xs:integer)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Nested binding references use the namespace on the declaration.</summary>
    [Fact]
    public void NamedCast_NestedBindingNamespace()
        => Assert.Equal("true|21", XElement.Parse(Run("""
            <xsl:record-type name='t:reading'><xsl:field name='value' as='xs:double'/></xsl:record-type>
            <xsl:template match='/'>
              <xsl:variable xmlns:n='urn:types' name='a' as='array(n:reading)' select="[map{'value':21}]"/>
              <out><xsl:value-of separator='|' select='($a?1 instance of t:reading,$a?1?value)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Nested named record fields use the same cast path recursively.</summary>
    [Fact]
    public void NamedCast_NestedRecordField()
        => Assert.Equal("true|21", XElement.Parse(Run("""
            <xsl:record-type name='t:reading'><xsl:field name='value' as='xs:double'/></xsl:record-type>
            <xsl:record-type name='t:envelope'><xsl:field xmlns:n='urn:types' name='reading' as='n:reading'/></xsl:record-type>
            <xsl:template match='/'>
              <xsl:variable name='r' select="map{'reading':map{'value':'21'}} cast as t:envelope"/>
              <out><xsl:value-of separator='|' select='($r?reading instance of t:reading,$r?reading?value)'/></out>
            </xsl:template>
            """)).Value);
}
