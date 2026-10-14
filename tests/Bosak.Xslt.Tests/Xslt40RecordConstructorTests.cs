// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies named record constructors, field conversion and nominative identity.
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
//                      | Charles Korthout | 0.2   | 13-10-2026     | Reject surplus map fields now that valid maps support nominative coercion                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 14-10-2026     | Assert native namespace retention separately from record/type result attributes.            |
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies named record constructors, field conversion and nominative identity.</summary>
public class Xslt40RecordConstructorTests
{
    private static string Run(string body) => new Api.XsltCompiler().Compile("""
        <xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
          xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:t="urn:records"
          xmlns:map="http://www.w3.org/2005/xpath-functions/map" exclude-result-prefixes="#all">
        """ + body + "</xsl:stylesheet>").TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));

    /// <summary>Preserves declared argument order and retains explicitly empty fields.</summary>
    [Fact]
    public void Constructor_FieldOrderAndEmptyValue()
    {
        var output = XElement.Parse(Run("""
            <xsl:record-type name="t:line" constructor="yes">
              <xsl:field name="sku" as="xs:string"/><xsl:field name="quantity" as="xs:integer"/>
              <xsl:field name="note" as="xs:string?"/>
            </xsl:record-type>
            <xsl:template match="/">
              <xsl:variable name="r" as="t:line" select="t:line('A',2,())"/>
              <out sku="{$r?sku}" quantity="{$r?quantity}" present="{map:contains($r,'note')}"
                empty="{empty($r?note)}" typed="{$r instance of t:line}" structural="{$r instance of record(*)}"/>
            </xsl:template>
            """));
        Assert.Equal(new[] { "A", "2", "true", "true", "true", "true" }, output.Attributes().Select(a => a.Value));
    }

    /// <summary>Static fields use initial values and consume no constructor parameter.</summary>
    [Fact]
    public void Constructor_StaticFieldAndDefaultArity()
    {
        var output = XElement.Parse(Run("""
            <xsl:record-type name="t:line" constructor="yes">
              <xsl:field name="sku" as="xs:string"/>
              <xsl:field name="quantity" as="xs:integer" select="1"/>
              <xsl:field name="kind" static="yes" as="xs:string"><xsl:sequence select="'line'"/></xsl:field>
            </xsl:record-type>
            <xsl:template match="/">
              <xsl:variable name="r" select="t:line('B')"/>
              <xsl:variable name="f" select="t:line#2"/>
              <out sku="{$r?sku}" quantity="{$r?quantity}" kind="{$r?kind}" fields="{map:size($r)}" supplied="{$f('C',3)?quantity}"/>
            </xsl:template>
            """));
        Assert.Equal(new[] { "B", "1", "line", "3", "3" }, output.Attributes().Select(a => a.Value));
    }

    /// <summary>Same-shaped types and plain maps do not match another nominal identity.</summary>
    [Fact]
    public void Constructor_NominativeIdentity()
    {
        var output = XElement.Parse(Run("""
            <xsl:record-type name="t:first" constructor="yes"><xsl:field name="id" as="xs:integer"/></xsl:record-type>
            <xsl:record-type name="t:second" constructor="yes"><xsl:field name="id" as="xs:integer"/></xsl:record-type>
            <xsl:template match="/">
              <out own="{t:first(1) instance of t:first}" other="{t:second(1) instance of t:first}"
                plain="{map {'id':1} instance of t:first}" empty="{() instance of t:first?}"/>
            </xsl:template>
            """));
        Assert.Equal(new[] { "true", "false", "false", "true" }, output.Attributes().Select(a => a.Value));
    }

    /// <summary>Type names remain distinct under locally rebound namespace prefixes.</summary>
    [Fact]
    public void Constructor_ReboundNamespace()
    {
        var output = XElement.Parse(Run("""
            <xsl:record-type name="t:line" constructor="yes"><xsl:field name="id"/></xsl:record-type>
            <xsl:record-type xmlns:t="urn:other" name="t:line" constructor="yes"><xsl:field name="id"/></xsl:record-type>
            <xsl:template xmlns:u="urn:other" match="/">
              <xsl:variable name="r" as="u:line" select="u:line(2)"/>
              <out value="{$r?id}" own="{$r instance of u:line}" crossed="{$r instance of t:line}"/>
            </xsl:template>
            """));
        Assert.Equal("urn:other", output.GetNamespaceOfPrefix("u")?.NamespaceName);
        Assert.Equal(new[] { "2", "true", "false" }, output.Attributes().Where(a => !a.IsNamespaceDeclaration).Select(a => a.Value));
    }

    /// <summary>Field argument conversion performs numeric promotion.</summary>
    [Fact]
    public void Constructor_NumericPromotion()
    {
        var output = XElement.Parse(Run("""
            <xsl:record-type name="t:amount" constructor="yes"><xsl:field name="value" as="xs:double"/></xsl:record-type>
            <xsl:template match="/"><out promoted="{t:amount(2)?value instance of xs:double}"/></xsl:template>
            """));
        Assert.Equal("true", (string?)output.Attribute("promoted"));
    }

    /// <summary>Static initializers see global focus and restore the caller's focus.</summary>
    [Fact]
    public void Constructor_StaticGlobalFocusAndRestoration()
    {
        var output = XElement.Parse(Run("""
            <xsl:record-type name="t:line" constructor="yes">
              <xsl:field name="kind" static="yes" as="xs:string" select="name(/*)"/>
            </xsl:record-type>
            <xsl:template match="/"><xsl:for-each select="(1,2)">
              <xsl:if test="position()=2"><out kind="{t:line()?kind}" focus="{.}" position="{position()}" size="{last()}"/></xsl:if>
            </xsl:for-each></xsl:template>
            """));
        Assert.Equal(new[] { "root", "2", "2", "2" }, output.Attributes().Select(a => a.Value));
    }

    /// <summary>Default parameter expressions run with absent focus.</summary>
    [Fact]
    public void Constructor_DefaultHasAbsentFocus()
    {
        var error = Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:record-type name="t:line" constructor="yes"><xsl:field name="id" select="."/></xsl:record-type>
            <xsl:template match="/"><xsl:sequence select="t:line()"/></xsl:template>
            """));
        Assert.Contains("XPDY0002", error.ToString());
    }

    /// <summary>Invalid arguments, arity and nominal bindings produce typed errors.</summary>
    /// <param name="expression">The expression used in the typed variable.</param>
    /// <param name="code">The expected error code.</param>
    [Theory]
    [InlineData("t:line('bad')", "XPTY0004")]
    [InlineData("t:line()", "XPST0017")]
    [InlineData("map {'id':1,'extra':2}", "XTTE0570")]
    public void Constructor_InvalidCallOrBinding(string expression, string code)
    {
        var error = Assert.ThrowsAny<Exception>(() => Run("<xsl:record-type name='t:line' constructor='yes'><xsl:field name='id' as='xs:integer'/></xsl:record-type><xsl:template match='/'><xsl:variable name='r' as='t:line' select=\"" + expression + "\"/><xsl:sequence select='$r'/></xsl:template>"));
        Assert.Contains(code, error.ToString());
    }

    /// <summary>Rejects duplicate fields, missing static initializers and conflicting definitions.</summary>
    /// <param name="declarations">Invalid declarations preceding the template.</param>
    /// <param name="code">The expected error code.</param>
    [Theory]
    [InlineData("<xsl:record-type name='t:line'><xsl:field name='id'/><xsl:field name='id'/></xsl:record-type>", "XTSE4050")]
    [InlineData("<xsl:record-type name='t:line'><xsl:field name='kind' static='yes'/></xsl:record-type>", "XTSE0010")]
    [InlineData("<xsl:record-type name='t:line'/><xsl:item-type name='t:line' as=\"enum('ready')\"/>", "XTSE4030")]
    [InlineData("<xsl:record-type name='t:line' constructor='yes'><xsl:field name='a' select='1'/><xsl:field name='b'/></xsl:record-type>", "XTSE0010")]
    [InlineData("<xsl:record-type name='t:line'><xsl:sequence select='1'/></xsl:record-type>", "XTSE0010")]
    [InlineData("<xsl:record-type name='t:line' constructor='yes'><xsl:field name='not a name'/></xsl:record-type>", "XTSE0020")]
    public void Constructor_InvalidDeclaration(string declarations, string code)
    {
        var error = Assert.ThrowsAny<Exception>(() => Run(declarations + "<xsl:template match='/'><out/></xsl:template>"));
        Assert.Contains(code, error.ToString());
    }
}
