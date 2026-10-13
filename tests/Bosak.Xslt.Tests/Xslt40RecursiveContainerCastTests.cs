// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies recursive records and container casts across XSLT and XPath boundaries.
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

/// <summary>Verifies recursive records and container casts across XSLT and XPath boundaries.</summary>
public class Xslt40RecursiveContainerCastTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:f='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string body, string source = "<root/>") => new Api.XsltCompiler().Compile(Wrap(body))
        .TransformToString(new XDocumentNode(XDocument.Parse(source)));

    /// <summary>Structural record fields cast nested optional containers and sequences.</summary>
    [Fact]
    public void ContainerCast_StructuralRecordFields()
        => Assert.Equal("1|2|true", XElement.Parse(Run("""
            <xsl:template match='/'>
              <xsl:variable name='r' select="map{'values':('1','2'),'a':['3']} cast as record(values as xs:integer+,a as array(xs:integer)?)"/>
              <out><xsl:value-of separator='|' select='($r?values,$r?a instance of array(xs:integer))'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Structural castable handles required member cardinality errors.</summary>
    [Fact]
    public void ContainerCast_StructuralRecordCastable()
        => Assert.Equal("false|true", XElement.Parse(Run("""
            <xsl:template match='/'>
              <out><xsl:value-of separator='|' select="(map{'a':[()]} castable as record(a as array(xs:integer)),
                map{'a':()} castable as record(a as array(xs:integer)?))"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Empty container types accept only empty containers.</summary>
    [Fact]
    public void ContainerCast_EmptyContainerTypes()
        => Assert.Equal("true|false|true|false", XElement.Parse(Run("""
            <xsl:template match='/'>
              <out><xsl:value-of separator='|' select="([] castable as array(),[1] castable as array(),
                map{} castable as map(),map{'x':1} castable as map())"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Enforces member cardinality and refuses map/array kind mismatches.</summary>
    [Fact]
    public void ContainerCast_CardinalityAndKinds()
        => Assert.Equal("false|false|false|false", XElement.Parse(Run("""
            <xsl:template match='/'>
              <out><xsl:value-of separator='|' select="([('1','2')] castable as array(xs:integer),
                [()] castable as array(xs:integer),[] castable as map(*),map{} castable as array(*))"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Finite recursive records work through typed array casts.</summary>
    [Fact]
    public void RecursiveRecord_ArrayCast()
        => Assert.Equal("1|2|true", XElement.Parse(Run("""
            <xsl:record-type name='t:list'><xsl:field name='value' as='xs:integer'/>
              <xsl:field name='next' as='t:list?'/></xsl:record-type>
            <xsl:template match='/'>
              <xsl:variable name='a' select="[map{'value':'1','next':map{'value':'2'}}] cast as array(t:list)"/>
              <out><xsl:value-of separator='|' select='($a?1?value,$a?1?next?value,$a?1 instance of t:list)'/></out>
            </xsl:template>
            """)).Value);
    /// <summary>Casts each array member sequence without flattening the outer array.</summary>
    [Fact]
    public void ContainerCast_ArrayMembers()
        => Assert.Equal("1|2|true|true", XElement.Parse(Run("""
            <xsl:template match='/'>
              <xsl:variable name='a' select="[('1','2'),()] cast as array(xs:integer*)"/>
              <out><xsl:value-of separator='|' select='($a?1,$a?1 instance of xs:integer+,empty($a?2))'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Casts nested arrays and atomizes array-valued atomic members.</summary>
    [Fact]
    public void ContainerCast_NestedArrays()
        => Assert.Equal("1|2|true", XElement.Parse(Run("""
            <xsl:template match='/'>
              <xsl:variable name='a' select="[['1'],['2']] cast as array(array(xs:integer))"/>
              <xsl:variable name='flat' select="[['1'],['2']] cast as array(xs:integer)"/>
              <out><xsl:value-of separator='|' select='($a?1?1,$flat?2,$a instance of array(array(xs:integer)))'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Casts map keys and value sequences while preserving insertion order.</summary>
    [Fact]
    public void ContainerCast_MapEntries()
        => Assert.Equal("1|2|true", XElement.Parse(Run("""
            <xsl:template match='/'>
              <xsl:variable name='m' select="map{'1':('1','2')} cast as map(xs:integer,xs:integer+)"/>
              <out><xsl:value-of separator='|' select='($m(1),$m instance of map(xs:integer,xs:integer+))'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Detects distinct source keys that collapse during an explicit map cast.</summary>
    [Fact]
    public void ContainerCast_KeyCollision()
        => Assert.Contains("XQDY0137", Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:template match='/'><xsl:sequence select="map{'1':1,'01':2} cast as map(xs:integer,xs:integer)"/></xsl:template>
            """)).ToString());

    /// <summary>Castable reports failures for wrong containers, members and key collisions.</summary>
    [Fact]
    public void ContainerCast_CastableBoundaries()
        => Assert.Equal("false|false|false|true|true", XElement.Parse(Run("""
            <xsl:template match='/'>
              <out><xsl:value-of separator='|' select="(1 castable as array(xs:integer),
                ['bad'] castable as array(xs:integer),map{'1':1,'01':2} castable as map(xs:integer,xs:integer),
                () castable as array(xs:integer)?,map{} castable as map(xs:string,xs:integer))"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Wildcard container casts retain the values and optional casts accept empty input.</summary>
    [Fact]
    public void ContainerCast_WildcardsAndOptional()
        => Assert.Equal("true|true|true", XElement.Parse(Run("""
            <xsl:template match='/'>
              <out><xsl:value-of separator='|' select="([1] cast as array(*) instance of array(*),
                map{'x':1} cast as map(*) instance of map(*),empty(() cast as map(xs:string,xs:integer)?))"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Casts container-valued fields in a named record projection.</summary>
    [Fact]
    public void ContainerCast_NamedRecordFields()
        => Assert.Equal("1|2|true", XElement.Parse(Run("""
            <xsl:record-type name='t:batch'><xsl:field name='values' as='array(xs:integer)'/>
              <xsl:field name='counts' as='map(xs:string,xs:integer)'/></xsl:record-type>
            <xsl:template match='/'>
              <xsl:variable name='r' select="map{'values':['1'],'counts':map{'x':'2'}} cast as t:batch"/>
              <out><xsl:value-of separator='|' select='($r?values?1,$r?counts?x,$r instance of t:batch)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Creates and queries a finite linked list using a recursive named field.</summary>
    [Fact]
    public void RecursiveRecord_LinkedListBinding()
        => Assert.Equal("1|2|true", XElement.Parse(Run("""
            <xsl:record-type name='t:list'><xsl:field name='value' as='xs:integer'/>
              <xsl:field xmlns:n='urn:types' name='next' as='n:list?'/></xsl:record-type>
            <xsl:template match='/'>
              <xsl:variable name='list' as='t:list' select="map{'value':1,'next':map{'value':2}}"/>
              <out><xsl:value-of separator='|' select='($list?value,$list?next?value,$list?next instance of t:list)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Recursive casts turn textual values into integer fields and discard surplus data.</summary>
    [Fact]
    public void RecursiveRecord_LinkedListCast()
        => Assert.Equal("1|2|true", XElement.Parse(Run("""
            <xsl:record-type name='t:list'><xsl:field name='value' as='xs:integer'/>
              <xsl:field name='next' as='t:list?'/></xsl:record-type>
            <xsl:template match='/'>
              <xsl:variable name='list' select="map{'value':'1','next':map{'value':'2','extra':9}} cast as t:list"/>
              <out><xsl:value-of separator='|' select='($list?value,$list?next?value,$list?next instance of t:list)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Supports finite mutually recursive record declarations and forward references.</summary>
    [Fact]
    public void RecursiveRecord_MutualReferences()
        => Assert.Equal("true|true", XElement.Parse(Run("""
            <xsl:record-type name='t:a'><xsl:field name='b' as='t:b?'/></xsl:record-type>
            <xsl:record-type name='t:b'><xsl:field name='a' as='t:a?'/></xsl:record-type>
            <xsl:template match='/'>
              <xsl:variable name='a' as='t:a' select="map{'b':map{}}"/>
              <out><xsl:value-of separator='|' select='($a instance of t:a,$a?b instance of t:b)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Rejects invalid required values in a recursive record binding.</summary>
    [Fact]
    public void RecursiveRecord_InvalidNestedValue()
        => Assert.Contains("XTTE0570", Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:record-type name='t:list'><xsl:field name='value' as='xs:integer'/>
              <xsl:field name='next' as='t:list?'/></xsl:record-type>
            <xsl:template match='/'><xsl:variable name='r' as='t:list' select="map{'value':1,'next':map{}}"/>
              <xsl:sequence select='$r'/></xsl:template>
            """)).ToString());
}
