// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 12 October 2026
// PURPOSE              : Verifies XSLT select body evaluation, result coercion and content validation.
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

/// <summary>Verifies XSLT select body evaluation, result coercion and content validation.</summary>
public class Xslt40SelectTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
          xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:f="urn:functions"
          exclude-result-prefixes="xs f">
        """ + body + "</xsl:stylesheet>";

    private static string Run(string body)
    {
        var executable = new Api.XsltCompiler().Compile(Wrap(body), "file:///select-test.xsl");
        return executable.TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
    }

    /// <summary>Preserves literal newlines and map braces even when text expansion is enabled.</summary>
    [Fact]
    public void Select_CDataMap_PreservesText()
    {
        var output = XElement.Parse(Run("""
            <xsl:template match='/'>
              <xsl:variable name='v' as='map(*)'><xsl:select expand-text='yes' as='map(*)'><![CDATA[
                map {'note':'A
            B', 'ok':2 < 3}
              ]]></xsl:select></xsl:variable>
              <out length='{string-length($v?note)}' newline='{string-to-codepoints($v?note)[2]}' ok='{$v?ok}'/>
            </xsl:template>
            """));
        Assert.Equal("3", (string?)output.Attribute("length"));
        Assert.Equal("10", (string?)output.Attribute("newline"));
        Assert.Equal("true", (string?)output.Attribute("ok"));
    }

    /// <summary>Preserves item, position and size through a raw variable sequence constructor.</summary>
    [Fact]
    public void Select_CurrentFocus_PreservesWholeSequence()
    {
        var output = XElement.Parse(Run("""
            <xsl:template match='/'><out><xsl:for-each select="('A','B')">
              <xsl:variable name='v' as='item()*'><xsl:select>(.,position(),last())</xsl:select></xsl:variable>
              <row value="{string-join($v ! string(.),'|')}"/>
            </xsl:for-each></out></xsl:template>
            """));
        Assert.Equal(new[] { "A|1|2", "B|2|2" }, output.Elements().Select(e => (string?)e.Attribute("value")));
    }

    /// <summary>Uses function-local variables and ignores a fallback while joining preprocessed text.</summary>
    [Fact]
    public void Select_FunctionAndSimpleContent()
    {
        var output = XElement.Parse(Run("""
            <xsl:function name='f:add' as='xs:integer'><xsl:param name='n' as='xs:integer'/>
              <xsl:select as='xs:integer'><xsl:fallback><xsl:sequence select='error()'/></xsl:fallback>
                $n <!--removed--> + <?ignored value?> 1
              </xsl:select>
            </xsl:function>
            <xsl:template match='/'><out n='{f:add(2)}'><xsl:value-of><xsl:select>('A','B')</xsl:select></xsl:value-of></out></xsl:template>
            """));
        Assert.Equal("3", (string?)output.Attribute("n"));
        Assert.Equal("AB", output.Value);
    }

    /// <summary>Uses the instruction collation in a function and restores it for following expressions.</summary>
    [Fact]
    public void Select_FunctionCollation_IsScoped()
    {
        var output = XElement.Parse(Run("""
            <xsl:function name='f:values' as='xs:integer*'>
              <xsl:select default-collation='http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'>compare('A','a')</xsl:select>
              <xsl:select>compare('A','a')</xsl:select>
            </xsl:function>
            <xsl:template match='/'><out value="{string-join(f:values() ! string(.),'|')}"/></xsl:template>
            """));
        Assert.Equal("0|-1", (string?)output.Attribute("value"));
    }

    /// <summary>Evaluates select with the separate accumulator rule context and its value binding.</summary>
    [Fact]
    public void Select_AccumulatorRule_UsesValueBinding()
    {
        var output = XElement.Parse(Run("""
            <xsl:accumulator name='count' as='xs:integer' initial-value='0'>
              <xsl:accumulator-rule match='*'><xsl:select as='xs:integer'>$value + 1</xsl:select></xsl:accumulator-rule>
            </xsl:accumulator>
            <xsl:mode use-accumulators='count'/>
            <xsl:template match='/'><out value="{accumulator-after('count')}"/></xsl:template>
            """));
        Assert.Equal("1", (string?)output.Attribute("value"));
    }

    /// <summary>Handles empty expressions and type conversion in the instruction itself.</summary>
    /// <param name="instruction">The select instruction under test.</param>
    /// <param name="expected">The expected element value.</param>
    [Theory]
    [InlineData("<xsl:select/>", "")]
    [InlineData("<xsl:select as='xs:string?'>   </xsl:select>", "")]
    [InlineData("<xsl:select as='xs:double'>2</xsl:select>", "2")]
    public void Select_EmptyAndCoercion(string instruction, string expected)
        => Assert.Equal(expected, XElement.Parse(Run("<xsl:template match='/'><out>" + instruction + "</out></xsl:template>")).Value);

    /// <summary>Rejects invalid result types using the XPath type error code.</summary>
    /// <param name="instruction">The select instruction with an incompatible result.</param>
    [Theory]
    [InlineData("<xsl:select as='xs:string'>1</xsl:select>")]
    [InlineData("<xsl:select as='xs:integer'/>")]
    [InlineData("<xsl:select as='xs:integer'>(1,2)</xsl:select>")]
    public void Select_InvalidResult_XPTY0004(string instruction)
    {
        var error = Assert.ThrowsAny<Exception>(() => Run("<xsl:template match='/'><out>" + instruction + "</out></xsl:template>"));
        Assert.Contains("XPTY0004", error.Message);
    }

    /// <summary>Checks body expressions against the streamed-context restrictions.</summary>
    [Fact]
    public void Select_StreamedLast_XTSE3430()
    {
        var error = Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(Wrap("""
            <xsl:mode streamable='yes'/>
            <xsl:template match='/'><out><xsl:select>last()</xsl:select></out></xsl:template>
            """), "file:///select-test.xsl"));
        Assert.Contains("XTSE3430", error.Message);
    }

    /// <summary>Rejects children that violate the expression-text content model.</summary>
    /// <param name="instruction">The malformed select instruction.</param>
    [Theory]
    [InlineData("<xsl:select><xsl:value-of select='1'/></xsl:select>")]
    [InlineData("<xsl:select>1<xsl:fallback/></xsl:select>")]
    public void Select_InvalidContent_XTSE0010(string instruction)
    {
        var error = Assert.Throws<InvalidOperationException>(() => new Api.XsltCompiler().Compile(
            Wrap("<xsl:template match='/'><out>" + instruction + "</out></xsl:template>"), "file:///select-test.xsl"));
        Assert.Contains("XTSE0010", error.Message);
    }
}
