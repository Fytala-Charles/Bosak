// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 september 2026
// PURPOSE              : Unit tests for user-defined simple type identity on schema-typed XDM values (REQ-108)
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for REQ-108: schema-validated values keep their user-defined simple type
/// identity (in <c>Q{uri}local</c> form) alongside the built-in base type annotation, so
/// <c>instance of</c> for a user-defined type is answered by type identity instead of
/// castability, and <c>@as</c> coercion of an untypedAtomic converts to (and annotates
/// with) the user-defined type.
/// </summary>
public class TypeIdentityTests
{
    private const string TestNs = "urn:req108";

    // partNumberType: string restriction with a pattern facet; part: element of that type.
    private const string TestSchema = """
        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:req108' xmlns:t='urn:req108' elementFormDefault='qualified'>
            <xs:simpleType name='partNumberType'>
                <xs:restriction base='xs:string'>
                    <xs:pattern value='[A-Z]{2}[0-9]{4}'/>
                </xs:restriction>
            </xs:simpleType>
            <xs:element name='part' type='t:partNumberType'/>
        </xs:schema>
        """;

    private const string ImportSchema = "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
        "targetNamespace='urn:req108' xmlns:t='urn:req108' elementFormDefault='qualified'>" +
        "<xs:simpleType name='partNumberType'><xs:restriction base='xs:string'>" +
        "<xs:pattern value='[A-Z]{2}[0-9]{4}'/></xs:restriction></xs:simpleType>" +
        "<xs:element name='part' type='t:partNumberType'/></xs:schema></xsl:import-schema>";

    private static XmlSchemaSet CompileTestSchema()
    {
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new StringReader(TestSchema), null)!);
        set.Compile();
        return set;
    }

    private static XDocument ValidatedPartDocument()
    {
        var source = new XDocument(new XElement(XNamespace.Get(TestNs) + "part", "AB1234"));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, CompileTestSchema());
        return source;
    }

    private static string RunSchemaAware(string xslBodyAndDecls, XDocument? source = null)
    {
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:req108' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
            + ImportSchema
            + xslBodyAndDecls
            + "</xsl:stylesheet>";
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var src = (IXdmNode)new XDocumentNode(source ?? new XDocument(new XElement("dummy")));
        return executable.TransformToString(src, initialTemplate: "main");
    }

    [Fact]
    public void InstanceOf_PsviUserTypedValue_MatchesUserTypeAndBaseType()
    {
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <out>
                    <user><xsl:value-of select="data(/t:part) instance of t:partNumberType"/></user>
                    <base><xsl:value-of select="data(/t:part) instance of xs:string"/></base>
                </out>
            </xsl:template>
            """, ValidatedPartDocument());
        Assert.Contains("<user>true</user>", result);
        Assert.Contains("<base>true</base>", result);
    }

    [Fact]
    public void InstanceOf_FacetValidUntypedAtomic_DoesNotMatchUserType()
    {
        // The lexical form satisfies the type's facets, but identity (not castability)
        // decides instance-of for user-defined types (as-2002).
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <out><xsl:value-of select="xs:untypedAtomic('AB1234') instance of t:partNumberType"/></out>
            </xsl:template>
            """);
        Assert.Contains(">false<", result);
    }

    [Fact]
    public void VariableCoercion_AsUserType_ConvertsUntypedAtomicAndAnnotates()
    {
        // @as coercion of xs:untypedAtomic casts to the user-defined type and the
        // coerced value carries the user type identity (as-1806..as-1809).
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:variable name='v' as='t:partNumberType' select="xs:untypedAtomic('AB1234')"/>
                <out>
                    <lex><xsl:value-of select='$v'/></lex>
                    <ident><xsl:value-of select="$v instance of t:partNumberType"/></ident>
                </out>
            </xsl:template>
            """);
        Assert.Contains("<lex>AB1234</lex>", result);
        Assert.Contains("<ident>true</ident>", result);
    }

    [Fact]
    public void Constructor_UserDefinedType_ResultMatchesUserType()
    {
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <out><xsl:value-of select="t:partNumberType('AB1234') instance of t:partNumberType"/></out>
            </xsl:template>
            """);
        Assert.Contains(">true<", result);
    }

    [Fact]
    public void Ebv_UserTypedStringValue_DoesNotRaiseForg0006()
    {
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:variable name='v' as='t:partNumberType' select="xs:untypedAtomic('AB1234')"/>
                <out>
                    <xsl:if test='$v'>non-empty</xsl:if>
                </out>
            </xsl:template>
            """);
        Assert.Contains(">non-empty<", result);
    }

    [Fact]
    public void Equality_UserTypedValue_EqualsStringWithSameContent()
    {
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <out><xsl:value-of select="data(/t:part) = xs:string('AB1234')"/></out>
            </xsl:template>
            """, ValidatedPartDocument());
        Assert.Contains(">true<", result);
    }
}
