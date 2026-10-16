// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies JNode atomization during XSLT simple-content construction.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies JNode atomization during XSLT simple-content construction.</summary>
public class JNodeTextAtomizationTests
{
    private static readonly string[] Surfaces = ["avt", "tvt", "value-of", "attribute-select", "attribute-content"];
    private static string Render(string surface, string expression, string declarations = "", string separator = " ", bool elementOnlySchema = false)
    {
        const string ns = "http://www.w3.org/1999/XSL/Transform";
        var content = surface switch
        {
            "avt" => "<out value='{$j}'/>",
            "tvt" => "<out>{$j}</out>",
            "value-of" => "<out>" + new XElement(XName.Get("value-of", ns), new XAttribute("select", "$j"), new XAttribute("separator", separator)) + "</out>",
            "attribute-select" => "<out>" + new XElement(XName.Get("attribute", ns), new XAttribute("name", "value"), new XAttribute("select", "$j"), new XAttribute("separator", separator)) + "</out>",
            _ => "<out><xsl:attribute name='value' separator=' '><xsl:sequence select='$j'/></xsl:attribute></out>"
        };
        var stylesheet = "<xsl:stylesheet xmlns:xsl='" + ns + "' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:t='urn:types' xmlns:array='http://www.w3.org/2005/xpath-functions/array' version='4.0' expand-text='yes' exclude-result-prefixes='#all'>" +
            "<xsl:template name='start'>" + declarations + new XElement(XName.Get("variable", ns), new XAttribute("name", "j"), new XAttribute("select", expression)) + content + "</xsl:template></xsl:stylesheet>";
        var compiler = new Api.XsltCompiler { SchemaAware = true };
        if (elementOnlySchema)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("urn:types", XmlReader.Create(new StringReader("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:types'><xs:complexType name='elementOnly'><xs:sequence><xs:element name='child' type='xs:string'/></xs:sequence></xs:complexType></xs:schema>")));
            compiler.SchemaSet = schemas;
        }
        var result = XElement.Parse(compiler.Compile(stylesheet).TransformToString(null, initialTemplate: "start"));
        return surface is "avt" or "attribute-select" or "attribute-content" ? result.Attribute("value")!.Value : result.Value;
    }

    /// <summary>Provides single, empty, multi-item, nested-array and nested-JNode payloads.</summary>
    public static IEnumerable<object[]> AtomicCases
    {
        get
        {
            foreach (var surface in Surfaces)
            foreach (var pair in new[] { ("jtree(42)", "42"), ("jtree(())", ""), ("jtree((1,2))", "1 2"), ("jtree([(),1,(2,3)])", "1 2 3"), ("jtree((jtree((1,2)),jtree(()),jtree(3)))", "1 2 3"), ("jtree(('',1,'',2,''))", " 1  2 ") })
                yield return [surface, pair.Item1, pair.Item2];
        }
    }

    /// <summary>Preserves complete atomized payloads across each simple-content surface.</summary>
    /// <param name="surface">The text construction path.</param>
    /// <param name="expression">The JNode expression.</param>
    /// <param name="expected">The atomized text.</param>
    [Theory]
    [MemberData(nameof(AtomicCases))]
    public void Payloads(string surface, string expression, string expected) => Assert.Equal(expected, Render(surface, expression));

    /// <summary>Provides payloads that cannot be completely atomized.</summary>
    public static IEnumerable<object[]> ErrorCases
    {
        get
        {
            foreach (var surface in Surfaces)
            foreach (var expression in new[] { "jtree(map{})", "jtree(function(){1})", "jtree((1,[2,function(){3}]))" })
                yield return [surface, expression];
        }
    }

    /// <summary>Raises FOTY0013 for maps/functions even after earlier valid payload items.</summary>
    /// <param name="surface">The text construction path.</param>
    /// <param name="expression">The invalid payload.</param>
    [Theory]
    [MemberData(nameof(ErrorCases))]
    public void AtomizationErrors(string surface, string expression) => Assert.Contains("FOTY0013", Assert.ThrowsAny<Exception>(() => Render(surface, expression)).ToString());

    /// <summary>Retains explicit separators when atomization produces several or empty-string values.</summary>
    /// <param name="surface">The selected construction.</param>
    [Theory]
    [InlineData("value-of")]
    [InlineData("attribute-select")]
    public void Separators(string surface) => Assert.Equal("1||2", Render(surface, "jtree((1,'',2))", separator: "|"));

    /// <summary>Atomizes wrapped schema-typed nodes and recursively nested arrays of nodes.</summary>
    /// <param name="surface">The construction path.</param>
    [Theory]
    [InlineData("avt")]
    [InlineData("tvt")]
    [InlineData("value-of")]
    public void TypedNodes(string surface) => Assert.Equal("23 23", Render(surface, "jtree(($n,[$n]))", "<xsl:variable name='n' as='element()'><xsl:element name='n' type='xs:integer'>0023</xsl:element></xsl:variable>"));

    /// <summary>Preserves FOTY0012 for a wrapped element with no typed value.</summary>
    [Fact]
    public void NoTypedValue() => Assert.Contains("FOTY0012", Assert.ThrowsAny<Exception>(() => Render("tvt", "jtree($n)", "<xsl:variable name='n' as='element()'><xsl:element name='n' type='t:elementOnly'><child/></xsl:element></xsl:variable>", elementOnlySchema: true)).ToString());

    /// <summary>Renders city-name children as atomized text so result-node predicates match.</summary>
    [Fact]
    public void CityFilter()
    {
        const string stylesheet = """
            <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:array='http://www.w3.org/2005/xpath-functions/array' version='4.0' expand-text='yes' exclude-result-prefixes='#all'>
            <xsl:template name='start'><xsl:variable name='cities' as='element(city)*'>
            <xsl:for-each select="array:members([map{'name':'Moskva','country':'RU'},map{'name':'Moskva','country':'TJ'},map{'name':'Other','country':'X'}])">
            <city country='{./country}'><name>{./name}</name></city>
            </xsl:for-each></xsl:variable><out>{$cities[name='Moskva']/@country}</out></xsl:template></xsl:stylesheet>
            """;
        Assert.Equal("RU TJ", XElement.Parse(new Api.XsltCompiler().Compile(stylesheet).TransformToString(null, initialTemplate: "start")).Value);
    }

    /// <summary>Retains the empty default separator for attributes constructed without select.</summary>
    [Fact]
    public void DefaultAttributeContentSeparator()
    {
        const string stylesheet = """
            <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='4.0'>
            <xsl:template name='start'><out><xsl:attribute name='value'><xsl:sequence select='jtree((1,2))'/></xsl:attribute></out></xsl:template></xsl:stylesheet>
            """;
        Assert.Equal("12", XElement.Parse(new Api.XsltCompiler().Compile(stylesheet).TransformToString(null, initialTemplate: "start")).Attribute("value")!.Value);
    }

    /// <summary>Retains the lexical string of an untyped node inside a JNode payload.</summary>
    [Fact]
    public void UntypedNode() => Assert.Equal("0023", Render("avt", "jtree($n)", "<xsl:variable name='n' as='element()'><n>0023</n></xsl:variable>"));

    /// <summary>Preserves the existing text-node boundaries around a multi-item JNode.</summary>
    /// <param name="surface">The construction path.</param>
    [Theory]
    [InlineData("tvt")]
    [InlineData("value-of")]
    public void MixedText(string surface) => Assert.Equal("T 1 2 T", Render(surface, "($t,jtree((1,2)),$t)", "<xsl:variable name='t' as='text()'><xsl:text>T</xsl:text></xsl:variable>"));
}
