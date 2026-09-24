// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 september 2026
// PURPOSE              : Unit tests for typed-template result harvesting with kind-tested @as (REQ-106 companion)
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests that a template with a kind-tested <c>@as</c> (<c>schema-attribute(N)</c> /
/// <c>attribute(N,T)</c>) accepts the single attribute its sequence constructor produces
/// (REQ-106 companion, as-1812/1813/1814): the typed-template result harvest counted the
/// namespace declaration that attribute namespace fixup adds to the temporary container as a
/// second result item, failing the cardinality check with XTTE0505.
/// </summary>
public class TemplateAsKindTestTests
{
    private const string TestNs = "urn:tak";

    private static string RunTransform(string templateBody, string asType)
    {
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='" + TestNs + "' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
            + "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' "
            + "targetNamespace='" + TestNs + "' xmlns:p='" + TestNs + "' elementFormDefault='qualified' attributeFormDefault='qualified'>"
            + "<xs:element name='root'/>"
            + "<xs:attribute name='specialPart' type='p:partNumberType'/>"
            + "<xs:simpleType name='partNumberType'><xs:restriction base='xs:string'><xs:pattern value='\\d{3}-[A-Z]{2}'/></xs:restriction></xs:simpleType>"
            + "<xs:simpleType name='specialPartNumber'><xs:restriction base='p:partNumberType'/></xs:simpleType>"
            + "</xs:schema></xsl:import-schema>"
            + "<xsl:template match='/'>"
            + "<out><xsl:call-template name='t'/></out>"
            + "</xsl:template>"
            + $"<xsl:template name='t' as='{asType}'>{templateBody}</xsl:template>"
            + "</xsl:stylesheet>";
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var src = (IXdmNode)new XDocumentNode(new System.Xml.Linq.XDocument(
            new System.Xml.Linq.XElement(System.Xml.Linq.XNamespace.Get(TestNs) + "root")));
        return executable.TransformToString(src);
    }

    [Fact]
    public void TypedTemplate_ConstructedAttribute_SchemaAttributeAs_Passes()
    {
        // as-1812 temp2 shape: one constructed, type-validated attribute satisfies
        // as="schema-attribute(my:specialPart)" — the harvested sequence must contain
        // exactly the attribute, not the fixup namespace declaration.
        var result = RunTransform(
            "<xsl:attribute name='p:specialPart' type='p:specialPartNumber'>123-AB</xsl:attribute>",
            "schema-attribute(p:specialPart)");
        Assert.Contains("123-AB", result);
    }

    [Fact]
    public void TypedTemplate_ConstructedAttribute_AttributeNameAndTypeAs_Passes()
    {
        // as-1813/1814 shape: as="attribute(name, type)" over a single constructed attribute.
        var result = RunTransform(
            "<xsl:attribute name='p:specialPart' type='p:specialPartNumber'>123-AB</xsl:attribute>",
            "attribute(p:specialPart, p:partNumberType)");
        Assert.Contains("123-AB", result);
    }
}
