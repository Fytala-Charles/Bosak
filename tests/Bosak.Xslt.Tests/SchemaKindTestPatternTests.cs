// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 september 2026
// PURPOSE              : Unit tests for schema-element()/schema-attribute() kind tests in XSLT match patterns
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for <c>schema-element(N)</c> and <c>schema-attribute(N)</c> kind tests in match
/// patterns. The pattern compiler previously fell through to QName parsing, which dropped
/// the argument; the fixed compiler evaluates the declaration, substitution group,
/// nillability, and type-derivation semantics against the schema set in scope (mirroring
/// <c>VmEngine.MatchesSchemaElement</c>). Without a schema set the tests never match.
/// </summary>
public class SchemaKindTestPatternTests
{
    private const string TestNs = "urn:pat";

    // base/derived exercise the substitution-group walk; unrelated shares the type but
    // is not a substitution-group member; order carries the limit attribute.
    private const string PatternSchema = """
        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:pat' xmlns:p='urn:pat' elementFormDefault='qualified'>
            <xs:element name='base' type='p:baseType'/>
            <xs:element name='derived' type='p:baseType' substitutionGroup='p:base'/>
            <xs:element name='unrelated' type='p:baseType'/>
            <xs:element name='order' type='p:orderType'/>
            <xs:complexType name='baseType'>
                <xs:sequence>
                    <xs:element name='id' type='xs:integer'/>
                </xs:sequence>
            </xs:complexType>
            <xs:complexType name='orderType'>
                <xs:sequence>
                    <xs:element name='id' type='xs:integer'/>
                </xs:sequence>
                <xs:attribute ref='p:limit'/>
            </xs:complexType>
            <xs:attribute name='limit' type='xs:integer'/>
        </xs:schema>
        """;

    private const string ImportSchema = "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
        "targetNamespace='urn:pat' xmlns:p='urn:pat' elementFormDefault='qualified'>" +
        "<xs:element name='base' type='p:baseType'/>" +
        "<xs:element name='derived' type='p:baseType' substitutionGroup='p:base'/>" +
        "<xs:element name='unrelated' type='p:baseType'/>" +
        "<xs:element name='order' type='p:orderType'/>" +
        "<xs:complexType name='baseType'><xs:sequence><xs:element name='id' type='xs:integer'/></xs:sequence></xs:complexType>" +
        "<xs:complexType name='orderType'><xs:sequence><xs:element name='id' type='xs:integer'/></xs:sequence>" +
        "<xs:attribute ref='p:limit'/></xs:complexType>" +
        "<xs:attribute name='limit' type='xs:integer'/>" +
        "</xs:schema></xsl:import-schema>";

    private static XmlSchemaSet CompilePatternSchema()
    {
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new StringReader(PatternSchema), null)!);
        set.Compile();
        return set;
    }

    private static string RunPatternTransform(string xslBodyAndDecls, XDocument? source = null, bool schemaAware = true)
    {
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:pat' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
            + (schemaAware ? ImportSchema : string.Empty)
            + xslBodyAndDecls
            + "</xsl:stylesheet>";
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = schemaAware };
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var src = (IXdmNode)new XDocumentNode(source ?? new XDocument(new XElement("dummy")));
        return executable.TransformToString(src);
    }

    private static XDocument ValidatedDocument(string rootLocal)
    {
        var schemas = CompilePatternSchema();
        var ns = XNamespace.Get(TestNs);
        var source = new XDocument(new XElement(ns + rootLocal,
            new XAttribute(ns + "limit", "3"),
            new XElement(ns + "id", "7")));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, schemas);
        return source;
    }

    // ----- schema-element() in match patterns -----

    [Fact]
    public void SchemaElement_MatchesValidatedElement()
    {
        var source = ValidatedDocument("order");
        var result = RunPatternTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*'/></out>
            </xsl:template>
            <xsl:template match='schema-element(p:order)'>ORDER</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """, source);
        Assert.Contains("ORDER", result);
        Assert.DoesNotContain("OTHER", result);
    }

    [Fact]
    public void SchemaElement_SubstitutionGroupMember_MatchesHeadDeclaration()
    {
        // The source element is declared as p:derived; the pattern names the head p:base.
        var source = ValidatedDocument("derived");
        var result = RunPatternTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*'/></out>
            </xsl:template>
            <xsl:template match='schema-element(p:base)'>BASE</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """, source);
        Assert.Contains("BASE", result);
    }

    [Fact]
    public void SchemaElement_NotSubstitutionGroupMember_DoesNotMatch()
    {
        // p:unrelated shares baseType but is not in the p:base substitution group.
        var source = ValidatedDocument("unrelated");
        var result = RunPatternTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*'/></out>
            </xsl:template>
            <xsl:template match='schema-element(p:base)'>BASE</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """, source);
        Assert.Contains("OTHER", result);
        Assert.DoesNotContain("BASE", result);
    }

    [Fact]
    public void SchemaElement_UnvalidatedSource_DoesNotMatch()
    {
        // No PSVI annotations on the source node: the declaration check fails, no error.
        var ns = XNamespace.Get(TestNs);
        var plain = new XDocument(new XElement(ns + "order",
            new XAttribute("limit", "3"),
            new XElement(ns + "id", "7")));
        var result = RunPatternTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*'/></out>
            </xsl:template>
            <xsl:template match='schema-element(p:order)'>ORDER</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """, plain);
        Assert.Contains("OTHER", result);
        Assert.DoesNotContain("ORDER", result);
    }

    [Fact]
    public void SchemaElement_NoSchemaSetInScope_DoesNotMatchAndDoesNotThrow()
    {
        // Basic processor (no xsl:import-schema): the kind test silently never matches.
        var source = ValidatedDocument("order");
        var result = RunPatternTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*'/></out>
            </xsl:template>
            <xsl:template match='schema-element(p:order)'>ORDER</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """, source, schemaAware: false);
        Assert.Contains("OTHER", result);
        Assert.DoesNotContain("ORDER", result);
    }

    // ----- schema-attribute() in match patterns -----

    [Fact]
    public void SchemaAttribute_MatchesValidatedAttribute()
    {
        var source = ValidatedDocument("order");
        var result = RunPatternTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*/@p:limit'/></out>
            </xsl:template>
            <xsl:template match='@schema-attribute(p:limit)'>LIMIT=<xsl:value-of select='.'/></xsl:template>
            <xsl:template match='@*'>OTHER</xsl:template>
            """, source);
        Assert.True(result.Contains("LIMIT=3"), $"expected LIMIT=3, got: {result}");
        Assert.DoesNotContain("OTHER", result);
    }

    [Fact]
    public void SchemaAttribute_UnvalidatedSource_DoesNotMatch()
    {
        var ns = XNamespace.Get(TestNs);
        var plain = new XDocument(new XElement(ns + "order",
            new XAttribute(ns + "limit", "3"),
            new XElement(ns + "id", "7")));
        var result = RunPatternTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*/@p:limit'/></out>
            </xsl:template>
            <xsl:template match='@schema-attribute(p:limit)'>LIMIT</xsl:template>
            <xsl:template match='@*'>OTHER</xsl:template>
            """, plain);
        Assert.Contains("OTHER", result);
        Assert.DoesNotContain("LIMIT", result);
    }

    // ----- path-step position -----

    [Fact]
    public void SchemaElement_InPathStep_MatchesChild()
    {
        var schemas = CompilePatternSchema();
        var ns = XNamespace.Get(TestNs);
        var plain = new XDocument(new XElement(ns + "wrapper",
            new XElement(ns + "order",
                new XAttribute(ns + "limit", "3"),
                new XElement(ns + "id", "7"))));
        XdmSchemaAnnotator.ValidateSubtree(plain.Root!.Elements().First(), schemas);

        var result = RunPatternTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='*'/></out>
            </xsl:template>
            <xsl:template match='p:wrapper'>
                <xsl:apply-templates select='p:order'/>
            </xsl:template>
            <xsl:template match='p:wrapper/schema-element(p:order)'>NESTED</xsl:template>
            <xsl:template match='p:order'>FLAT</xsl:template>
            """, plain);
        Assert.True(result.Contains("NESTED"), $"expected NESTED, got: {result}");
        Assert.DoesNotContain("FLAT", result);
    }
}
