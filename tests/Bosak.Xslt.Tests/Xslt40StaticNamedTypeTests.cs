// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies host static names in XPath type expressions and choices.
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
//                      | Charles Korthout | 0.2   | 13-10-2026     | Cover static names on unused bindings and record fields
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies host static names in XPath type expressions and choices.</summary>
[Collection("XSLT package registry")]
public class Xslt40StaticNamedTypeTests
{
    private static string Run(string declarations, string body) => new Api.XsltCompiler().Compile("""
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + declarations + "<xsl:template match='/'>" + body + "</xsl:template></xsl:stylesheet>")
        .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
    private const string Sensor = "<xsl:record-type name='t:sensor'><xsl:field name='reading' as='xs:integer'/></xsl:record-type>";

    private static void CompileDeclarations(string declarations) => new Api.XsltCompiler().Compile("""
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
        """ + declarations + "</xsl:stylesheet>");

    /// <summary>Rejects unknown supported declaration types even when never executed.</summary>
    /// <param name="declaration">An unused declaration containing an unknown type.</param>
    [Theory]
    [InlineData("<xsl:variable name='v' as='t:missing?' select='()'/>")]
    [InlineData("<xsl:param name='v' as='array(t:missing)?' select='()'/>")]
    [InlineData("<xsl:template name='unused'><xsl:variable name='v' as='map(xs:string,t:missing)?' select='()'/></xsl:template>")]
    [InlineData("<xsl:template name='unused'><xsl:param name='v' as='record(x as t:missing)?'/></xsl:template>")]
    [InlineData("<xsl:function name='t:unused' as='t:missing?'><xsl:sequence select='()'/></xsl:function>")]
    [InlineData("<xsl:function name='t:unused'><xsl:param name='v' as='function(t:missing) as xs:integer'/><xsl:sequence select='()'/></xsl:function>")]
    [InlineData("<xsl:record-type name='t:r'><xsl:field name='x' as='t:missing?'/></xsl:record-type>")]
    [InlineData("<xsl:record-type name='t:r'><xsl:field name='x' as='array(t:missing)'/></xsl:record-type>")]
    [InlineData("<xsl:variable name='v' as='(xs:integer | t:missing)?' select='()'/>")]
    [InlineData("<xsl:variable name='v' as='xs:notAType?' select='()'/>")]
    [InlineData("<xsl:variable name='v' as='Q{urn:missing}type?' select='()'/>")]
    public void Declaration_UnknownType(string declaration)
        => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => CompileDeclarations(declaration)).ToString());

    /// <summary>Reports unbound declaration prefixes using the XPath static error.</summary>
    [Fact]
    public void Declaration_UnboundPrefix()
        => Assert.Contains("XPST0081", Assert.ThrowsAny<Exception>(() => CompileDeclarations(
            "<xsl:variable name='v' as='absent:type?' select='()'/>")).ToString());

    /// <summary>Accepts forward aliases, recursive records and local namespace bindings.</summary>
    [Fact]
    public void Declaration_ForwardAndRecursiveNames()
        => CompileDeclarations("""
            <xsl:variable name='v' as='array(t:alias)?' select='()'/>
            <xsl:record-type name='t:r'><xsl:field name='next' as='t:r?'/></xsl:record-type>
            <xsl:item-type name='t:alias' as='t:r'/>
            <xsl:template name='unused' xmlns:u='urn:types'><xsl:param name='v' as='u:r?'/></xsl:template>
            <xsl:template name='default' xpath-default-namespace='urn:types'><xsl:param name='v' as='r?'/></xsl:template>
            """);

    /// <summary>Leaves QName-like field names and enumeration strings untouched.</summary>
    [Fact]
    public void Declaration_LiteralNames()
        => CompileDeclarations("""
            <xsl:variable name='v' as="record('absent:type' as enum('absent:type'))?" select='()'/>
            """);

    /// <summary>Rejects unknown names, including nested types, before operand evaluation.</summary>
    /// <param name="expression">The invalid expression.</param>
    [Theory]
    [InlineData("error() cast as t:missing")]
    [InlineData("error() castable as t:missing?")]
    [InlineData("error() instance of t:missing?")]
    [InlineData("error() treat as t:missing?")]
    [InlineData("() instance of array(t:missing)")]
    [InlineData("() instance of map(xs:string,t:missing)")]
    [InlineData("() instance of record(reading as t:missing)")]
    [InlineData("() instance of function(t:missing) as xs:integer")]
    [InlineData("() instance of (xs:integer | t:missing)")]
    [InlineData("() instance of xs:notAType")]
    [InlineData("() castable as Q{urn:missing}type?")]
    public void Expression_UnknownType(string expression)
        => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => Run("",
            $"<out><xsl:value-of select=\"{expression}\"/></out>")).ToString());

    /// <summary>Reports undeclared type prefixes through the host static namespace check.</summary>
    [Fact]
    public void Expression_UnboundTypePrefix()
        => Assert.Contains("XPST0081", Assert.ThrowsAny<Exception>(() => Run("",
            "<xsl:sequence select='() instance of absent:type'/>")).ToString());

    /// <summary>Validates type names inside attribute value templates.</summary>
    [Fact]
    public void Avt_UnknownType()
        => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => Run("", "<out valid='{() instance of t:missing}'/>")).ToString());

    /// <summary>Rejects nominative record alternatives in direct and aliased choice cast targets.</summary>
    /// <param name="expression">The casting expression.</param>
    [Theory]
    [InlineData("() castable as (xs:integer | t:sensor)?")]
    [InlineData("() cast as (xs:integer | Q{urn:types}sensor)?")]
    [InlineData("() castable as t:payload?")]
    public void Cast_NominativeChoice(string expression)
        => Assert.Contains("XPST0080", Assert.ThrowsAny<Exception>(() => Run(Sensor +
            "<xsl:item-type name='t:payload' as='(xs:integer | t:sensor)'/>",
            $"<xsl:sequence select=\"{expression}\"/>")).ToString());

    /// <summary>Resolves a declared record name in a rebound prefix without losing its identity.</summary>
    [Fact]
    public void Record_ReboundNamespace()
        => Assert.Equal("42|true", XElement.Parse(Run(Sensor, """
            <xsl:variable name='s' xmlns:v='urn:types' xmlns:t='urn:other'
              select="map{'reading':'42'} cast as v:sensor"/>
            <out><xsl:value-of separator='|' select='($s?reading,$s instance of t:sensor)'/></out>
            """)).Value);

    /// <summary>Preserves the host record registry through an expression namespace prolog.</summary>
    [Fact]
    public void Record_InlineNamespace()
        => Assert.Equal("true", XElement.Parse(Run(Sensor,
            "<out><xsl:value-of select=\"declare namespace v='urn:types'; () instance of v:sensor?\"/></out>")).Value);

    /// <summary>Supports record-valued choices for ordinary type tests.</summary>
    [Fact]
    public void InstanceOf_NominativeChoice()
        => Assert.Equal("true|true", XElement.Parse(Run(Sensor, """
            <xsl:variable name='s' select="map{'reading':'42'} cast as t:sensor"/>
            <out><xsl:value-of separator='|' select='($s instance of (xs:integer | t:sensor),42 instance of (xs:integer | t:sensor))'/></out>
            """)).Value);

    /// <summary>Recognizes XDM duration and union names absent from the XSD 1.0 type table.</summary>
    [Fact]
    public void InstanceOf_BuiltInTypes()
        => Assert.Equal("true|true|true", XElement.Parse(Run("", """
            <out><xsl:value-of separator='|' select="(xs:dayTimeDuration('PT1H') instance of xs:dayTimeDuration,
              42 instance of xs:numeric,() instance of xs:error?)"/></out>
            """)).Value);

    /// <summary>Keeps XPath 3.1 element profiles isolated from host type validation.</summary>
    [Fact]
    public void FrozenElement_RetainsProfile()
        => Assert.Equal("true", XElement.Parse(Run(Sensor,
            "<out><xsl:value-of version='3.0' select='42 instance of xs:integer'/></out>")).Value);
}
