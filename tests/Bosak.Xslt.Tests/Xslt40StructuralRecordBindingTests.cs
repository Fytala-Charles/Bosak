// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies coercion and cardinality of direct structural record bindings.
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

/// <summary>Verifies coercion and cardinality of direct structural record bindings.</summary>
public class Xslt40StructuralRecordBindingTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:t='urn:types' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string body) => XElement.Parse(new Api.XsltCompiler().Compile(Wrap(body))
        .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;

    /// <summary>Coerces direct local bindings and retains their record annotation.</summary>
    [Fact]
    public void LocalRecordBinding()
    {
        Assert.Equal("true|true|7", Run("""
            <xsl:template match='/'><out>
              <xsl:variable name='r' as='record(x as xs:double)' select="map{'x':7}"/>
              <xsl:value-of separator='|' select='($r instance of record(*),$r?x instance of xs:double,$r?x)'/>
            </out></xsl:template>
            """));
    }

    /// <summary>Applies coercion to global bindings and template parameter defaults.</summary>
    [Fact]
    public void GlobalAndParameterBindings()
    {
        Assert.Equal("true|true", Run("""
            <xsl:variable name='global' as='record(x as xs:double)' select="map{'x':1}"/>
            <xsl:template match='/'><xsl:param name='p' as='record(x as xs:double)' select="map{'x':2}"/>
              <out><xsl:value-of separator='|' select='($global instance of record(*),$p?x instance of xs:double)'/></out>
            </xsl:template>
            """));
    }

    /// <summary>Converts every member while preserving allowed cardinalities.</summary>
    /// <param name="occurrence">The allowed occurrence indicator.</param>
    /// <param name="value">The supplied sequence expression.</param>
    /// <param name="expected">The expected converted item count.</param>
    [Theory]
    [InlineData("*", "()", "0")]
    [InlineData("?", "()", "0")]
    [InlineData("*", "(map{'x':1},map{'x':2})", "2")]
    [InlineData("+", "(map{'x':1},map{'x':2})", "2")]
    public void SequenceCardinality(string occurrence, string value, string expected)
    {
        Assert.Equal(expected, Run("<xsl:template match='/'><out><xsl:variable name='r' as='record(x as xs:double)" + occurrence + "' select=\"" + value + "\"/><xsl:value-of select='count($r[. instance of record(*)])'/></out></xsl:template>"));
    }

    /// <summary>Rejects missing required records, extra fields and incompatible field values.</summary>
    /// <param name="occurrence">The required occurrence indicator.</param>
    /// <param name="value">The invalid value expression.</param>
    [Theory]
    [InlineData("", "()")]
    [InlineData("+", "()")]
    [InlineData("", "(map{'x':1},map{'x':2})")]
    [InlineData("?", "(map{'x':1},map{'x':2})")]
    [InlineData("", "map{'x':'invalid'}")]
    [InlineData("", "map{'x':1,'extra':2}")]
    [InlineData("", "map{}")]
    public void InvalidVariableBindings(string occurrence, string value)
    {
        Assert.Contains("XTTE0570", Assert.ThrowsAny<Exception>(() => Run("<xsl:template match='/'><out><xsl:variable name='r' as='record(x as xs:double)" + occurrence + "' select=\"" + value + "\"/><xsl:value-of select='count($r)'/></out></xsl:template>")).ToString());
    }

    /// <summary>Preserves the parameter-specific diagnostic on failed conversion.</summary>
    [Fact]
    public void InvalidParameterBinding()
    {
        Assert.Contains("XTTE0590", Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:template match='/'><out><xsl:call-template name='target'><xsl:with-param name='p' select="map{'x':'invalid'}"/></xsl:call-template></out></xsl:template>
            <xsl:template name='target'><xsl:param name='p' as='record(x as xs:double)'/><xsl:value-of select='$p?x'/></xsl:template>
            """)).ToString());
    }

    /// <summary>Resolves field aliases and node contracts in the declaration namespace.</summary>
    [Fact]
    public void FieldNamespaceContext()
    {
        Assert.Equal("true|true", Run("""
            <xsl:item-type name='t:number' as='xs:double'/>
            <xsl:template match='/' xmlns:n='urn:nodes'><out>
              <xsl:variable name='node'><n:root/></xsl:variable>
              <xsl:variable name='r' as='record(x as t:number, root as element(n:root))' select="map{'x':1,'root':$node/*}"/>
              <xsl:value-of separator='|' select='($r?x instance of xs:double,$r?root instance of element(n:root))'/>
            </out></xsl:template>
            """));
    }

    /// <summary>Allows record-star to accept annotated records and rejects plain maps.</summary>
    [Fact]
    public void AnyRecordRequiresAnnotation()
    {
        Assert.Equal("true", Run("""
            <xsl:template match='/'><out><xsl:variable name='r' as='record()' select='map{}'/>
              <xsl:variable name='any' as='record(*)' select='$r'/><xsl:value-of select='$any instance of record(*)'/>
            </out></xsl:template>
            """));
        Assert.Contains("XTTE0570", Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:template match='/'><out><xsl:variable name='r' as='record(*)' select='map{}'/><xsl:value-of select='count($r)'/></out></xsl:template>
            """)).ToString());
    }

    /// <summary>Rejects empty values for direct required inline function record contracts.</summary>
    [Fact]
    public void RequiredInlineRecordIsNotOptional()
    {
        Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:template match='/'><out><xsl:value-of select='(fn() as record(x) {()})()'/></out></xsl:template>
            """)).ToString());
    }
}
