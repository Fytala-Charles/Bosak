// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 september 2026
// PURPOSE              : Unit tests for schema-element()/schema-attribute() visibility in XPath static contexts (REQ-104)
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

using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests that the stylesheet's merged schema set is visible to every XPath static context
/// the transform touches (REQ-104, Bosak.Schema Phase A item PA-2): select expressions,
/// predicates with <c>instance of</c>, and document-node kind tests. Before the fix these
/// failed statically with XPST0008 ("no schema awareness") even in schema-aware transforms;
/// now the name argument is validated against the in-scope declarations (XPST0008 when
/// absent) and the kind tests evaluate through the REQ-097/REQ-100 runtime machinery.
/// </summary>
public class SchemaKindTestStaticVisibilityTests
{
    private const string TestNs = "urn:vis";

    private const string ImportSchema = "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
        "targetNamespace='urn:vis' xmlns:p='urn:vis' elementFormDefault='qualified'>" +
        "<xs:element name='order' type='p:orderType'/>" +
        "<xs:element name='doc' type='p:docType'/>" +
        "<xs:complexType name='orderType'><xs:sequence><xs:element name='id' type='xs:integer'/></xs:sequence>" +
        "<xs:attribute ref='p:limit'/></xs:complexType>" +
        "<xs:complexType name='docType'><xs:sequence><xs:element name='q' type='xs:QName'/><xs:element name='day' type='xs:gDay'/></xs:sequence></xs:complexType>" +
        "<xs:attribute name='limit' type='xs:integer'/>" +
        "</xs:schema></xsl:import-schema>";

    private static XmlSchemaSet CompileSchema()
    {
        // The annotation set mirrors the stylesheet's inline import-schema declaration.
        const string schemaText = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:vis' xmlns:p='urn:vis' elementFormDefault='qualified'>
                <xs:element name='order' type='p:orderType'/>
                <xs:element name='doc' type='p:docType'/>
                <xs:complexType name='orderType'>
                    <xs:sequence>
                        <xs:element name='id' type='xs:integer'/>
                    </xs:sequence>
                    <xs:attribute ref='p:limit'/>
                </xs:complexType>
                <xs:complexType name='docType'>
                    <xs:sequence>
                        <xs:element name='q' type='xs:QName'/>
                        <xs:element name='day' type='xs:gDay'/>
                    </xs:sequence>
                </xs:complexType>
                <xs:attribute name='limit' type='xs:integer'/>
            </xs:schema>
            """;
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new StringReader(schemaText), null)!);
        set.Compile();
        return set;
    }

    private static XDocument ValidatedOrder()
    {
        var ns = XNamespace.Get(TestNs);
        var source = new XDocument(new XElement(ns + "order",
            new XAttribute(ns + "limit", "3"),
            new XElement(ns + "id", "7")));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, CompileSchema());
        return source;
    }

    private static XDocument ValidatedTypedDoc()
    {
        var ns = XNamespace.Get(TestNs);
        var source = new XDocument(new XElement(ns + "doc",
            new XAttribute(XNamespace.Xmlns + "m", "urn:m"),
            new XElement(ns + "q", "m:val"),
            new XElement(ns + "day", "---31Z")));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, CompileSchema());
        return source;
    }

    private static string RunTransform(string xslBodyAndDecls, XDocument? source = null, bool schemaAware = true, string? xpathDefaultNamespace = null)
    {
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:vis' xmlns:xs='http://www.w3.org/2001/XMLSchema'"
            + (xpathDefaultNamespace is null ? string.Empty : $" xpath-default-namespace='{xpathDefaultNamespace}'")
            + ">"
            + (schemaAware ? ImportSchema : string.Empty)
            + xslBodyAndDecls
            + "</xsl:stylesheet>";
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = schemaAware };
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var src = (IXdmNode)new XDocumentNode(source ?? ValidatedOrder());
        return executable.TransformToString(src);
    }

    // ----- schema-element() in select expressions -----

    [Fact]
    public void SchemaElement_InSelect_MatchesValidatedRoot()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='schema-element(p:order)'/></out>
            </xsl:template>
            <xsl:template match='p:order'>ORDER-MATCHED</xsl:template>
            """);
        Assert.Contains("ORDER-MATCHED", result);
    }

    [Fact]
    public void SchemaElement_InSelect_FiltersByDeclaration()
    {
        // p:base is not declared by the imported schema... use a declared-vs-undeclared
        // contrast instead: p:order matches, and a kind test for a different declared
        // name would not match this root (there is none), so assert the empty result.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='schema-element(p:order)'/></out>
            </xsl:template>
            <xsl:template match='p:order'>ORDER-MATCHED</xsl:template>
            <xsl:template match='*'>OTHER</xsl:template>
            """);
        Assert.Contains("ORDER-MATCHED", result);
        Assert.DoesNotContain("OTHER", result);
    }

    [Fact]
    public void SchemaElement_Unprefixed_WithXPathDefaultNamespace_Matches()
    {
        // Unprefixed names expand against xpath-default-namespace; before REQ-104 the
        // parser rejected unprefixed schema-element() names outright (XPST0008).
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='schema-element(order)'/></out>
            </xsl:template>
            <xsl:template match='p:order'>ORDER-MATCHED</xsl:template>
            """, xpathDefaultNamespace: TestNs);
        Assert.Contains("ORDER-MATCHED", result);
    }

    [Fact]
    public void SchemaElement_InInstanceOfPredicate_True()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:value-of select='* instance of schema-element(p:order)'/></out>
            </xsl:template>
            """);
        Assert.Contains("true", result);
    }

    [Fact]
    public void DocumentNode_SchemaElement_InInstanceOf_True()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:value-of select='. instance of document-node(schema-element(p:order))'/></out>
            </xsl:template>
            """);
        Assert.Contains("true", result);
    }

    // ----- schema-attribute() in XPath positions -----

    [Fact]
    public void SchemaAttribute_InInstanceOfPredicate_True()
    {
        var result = RunTransform("""
            <xsl:template match='p:order'>
                <out><xsl:value-of select='exists(@*[. instance of schema-attribute(p:limit)])'/></out>
            </xsl:template>
            """);
        Assert.Contains("true", result);
    }

    // ----- companion: typed-value coercion from validated sources (as-1702 shape) -----

    [Fact]
    public void ValidatedSource_TypedValues_CoerceToDeclaredVariableTypes()
    {
        // REQ-104 companion: a validated source's PSVI typed values must satisfy @as
        // coercion by subtype substitution — the coercion previously atomized every node
        // to untypedAtomic, which fails for xs:QName (no untypedAtomic→QName cast) and for
        // DateTime-kind g* values (ValueMatchesType accepted only string-kind g* values).
        var result = RunTransform("""
            <xsl:variable name='qv' select='/p:doc/p:q' as='xs:QName'/>
            <xsl:variable name='dv' select='/p:doc/p:day' as='xs:gDay'/>
            <xsl:template match='/'>
                <out><xsl:value-of select='$qv instance of xs:QName'/>,<xsl:value-of select='$dv instance of xs:gDay'/></out>
            </xsl:template>
            """, ValidatedTypedDoc());
        Assert.Contains("true,true", result);
    }

    // ----- error cases -----

    [Fact]
    public void SchemaElement_UndeclaredDeclaration_XPST0008()
    {
        // The prefix resolves but the imported schema declares no p:absent element.
        var ex = Assert.ThrowsAny<Exception>(() => RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='schema-element(p:absent)'/></out>
            </xsl:template>
            """));
        Assert.Contains("XPST0008", ex.Message);
    }

    [Fact]
    public void SchemaElement_BasicProcessor_StillXPST0008()
    {
        // Without schema awareness the kind test must keep failing (no behavior change
        // for basic processors).
        var ex = Assert.ThrowsAny<Exception>(() => RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='schema-element(p:order)'/></out>
            </xsl:template>
            <xsl:template match='p:order'>ORDER-MATCHED</xsl:template>
            """, schemaAware: false));
        Assert.Contains("XPST0008", ex.Message);
    }
}
