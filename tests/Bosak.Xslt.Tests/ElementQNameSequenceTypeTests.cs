// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 30 september 2026
// PURPOSE              : Unit tests for unprefixed QName resolution in xsl:variable/@as sequence types
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 30-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests that unprefixed QNames in an <c>xsl:variable</c>/<c>xsl:param</c> <c>@as</c>
/// sequence type resolve against the in-scope <c>xpath-default-namespace</c> (XSLT 3.0
/// §8.3): element names in <c>element(N)</c> kind tests accept schema-validated elements
/// that have no global element declaration (W3C import-schema-202), and unprefixed
/// user-defined type names match PSVI-typed values by identity (W3C
/// xpath-default-namespace-0701). <c>schema-element(N)</c> still requires a declaration.
/// </summary>
public class ElementQNameSequenceTypeTests
{
    private const string TestNs = "urn:eqst";

    private const string ImportSchema = "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
        "targetNamespace='urn:eqst' xmlns:p='urn:eqst' elementFormDefault='qualified' attributeFormDefault='qualified'>" +
        "<xs:element name='wrapper' type='p:wrapperType'/>" +
        "<xs:complexType name='wrapperType'><xs:sequence>" +
        "<xs:any namespace='##targetNamespace' processContents='lax'/>" +
        "</xs:sequence></xs:complexType>" +
        "<xs:simpleType name='partNumberType'><xs:restriction base='xs:string'><xs:pattern value='\\d{3}-[A-Z]{2}'/></xs:restriction></xs:simpleType>" +
        "</xs:schema></xsl:import-schema>";

    private static XmlSchemaSet CompileSchema()
    {
        // The annotation set mirrors the stylesheet's inline import-schema declaration.
        const string schemaText = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:eqst' xmlns:p='urn:eqst' elementFormDefault='qualified' attributeFormDefault='qualified'>
                <xs:element name='wrapper' type='p:wrapperType'/>
                <xs:complexType name='wrapperType'>
                    <xs:sequence>
                        <xs:any namespace='##targetNamespace' processContents='lax'/>
                    </xs:sequence>
                </xs:complexType>
                <xs:simpleType name='partNumberType'>
                    <xs:restriction base='xs:string'><xs:pattern value='\d{3}-[A-Z]{2}'/></xs:restriction>
                </xs:simpleType>
            </xs:schema>
            """;
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new StringReader(schemaText), null)!);
        set.Compile();
        return set;
    }

    private static XDocument ValidatedWrapper()
    {
        var ns = XNamespace.Get(TestNs);
        // 'base' has NO global element declaration; lax validation under xs:any
        // annotates it with xs:anyType.
        var source = new XDocument(new XElement(ns + "wrapper",
            new XElement(ns + "extension",
                new XElement(ns + "base", "Base 1")),
            new XElement(ns + "extension",
                new XElement(ns + "base", "Base 2"))));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, CompileSchema());
        return source;
    }

    private static string RunTransform(string xslBodyAndDecls, XDocument? source = null, bool schemaAware = true, string? xpathDefaultNamespace = null)
    {
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:eqst' xmlns:xs='http://www.w3.org/2001/XMLSchema'"
            + (xpathDefaultNamespace is null ? string.Empty : $" xpath-default-namespace='{xpathDefaultNamespace}'")
            + ">"
            + (schemaAware ? ImportSchema : string.Empty)
            + xslBodyAndDecls
            + "</xsl:stylesheet>";
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = schemaAware };
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var src = (IXdmNode)new XDocumentNode(source ?? ValidatedWrapper());
        return executable.TransformToString(src);
    }

    // ----- element(N) with no global element declaration (import-schema-202) -----

    [Fact]
    public void ElementName_NoGlobalDeclaration_ValidatedLax_Accepted()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='bases' as='element(base)*' select='//p:base'/>
                <out count='{count($bases)}' parents='{count($bases/parent::*)}'/>
            </xsl:template>
            """, xpathDefaultNamespace: TestNs);
        Assert.Contains("count=\"2\"", result);
        Assert.Contains("parents=\"2\"", result);
    }

    [Fact]
    public void ElementName_WrongNamespace_StillRejected()
    {
        // No xpath-default-namespace: unprefixed 'base' means the empty namespace,
        // but the validated elements live in urn:eqst.
        var ex = Assert.Throws<InvalidOperationException>(() => RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='bases' as='element(base)*' select='//p:base'/>
                <out count='{count($bases)}'/>
            </xsl:template>
            """));
        Assert.Contains("XTTE0570", ex.Message);
    }

    [Fact]
    public void SchemaElementName_NoDeclaration_StillRejected()
    {
        // 'base' has no global element declaration: schema-element(base) must fail
        // even though element(base) accepts the same nodes.
        Assert.Throws<InvalidOperationException>(() => RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='bases' as='schema-element(base)*' select='//p:base'/>
                <out count='{count($bases)}'/>
            </xsl:template>
            """, xpathDefaultNamespace: TestNs));
    }

    // ----- unprefixed user-defined type name in @as (xpath-default-namespace-0701) -----

    [Fact]
    public void UnprefixedUserType_ResolvesViaXPathDefaultNamespace_Accepted()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out xsl:xpath-default-namespace='urn:eqst'>
                    <xsl:variable name='v' select="p:partNumberType('123-AB')" as='partNumberType'/>
                    <xsl:value-of select='$v instance of p:partNumberType'/>
                </out>
            </xsl:template>
            """);
        Assert.Contains("<out", result);
        Assert.Contains("true", result);
    }

    [Fact]
    public void UnprefixedUserType_ValueOfWrongType_StillRejected()
    {
        // A plain xs:string is castable to partNumberType but is not an instance of it:
        // coercion requires the PSVI type identity (REQ-108), so XTTE0570 is raised.
        var ex = Assert.Throws<InvalidOperationException>(() => RunTransform("""
            <xsl:template match='/'>
                <out xsl:xpath-default-namespace='urn:eqst'>
                    <xsl:variable name='v' select="'123-AB'" as='partNumberType'/>
                    <xsl:value-of select='$v'/>
                </out>
            </xsl:template>
            """));
        Assert.Contains("XTTE0570", ex.Message);
    }
}
