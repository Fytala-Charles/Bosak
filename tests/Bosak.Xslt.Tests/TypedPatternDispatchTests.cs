// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 september 2026
// PURPOSE              : Unit tests for schema-aware typed kind-test dispatch in XSLT match patterns (REQ-105)
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
/// Tests that the schema type argument of the <c>element(N, T)</c> / <c>element(*, T)</c> /
/// <c>attribute(N, T)</c> / <c>attribute(*, T)</c> kind tests is honored when XSLT match
/// patterns are compiled in a schema-aware stylesheet (REQ-105, Bosak.Schema Phase A item
/// PA-3). Before the fix the type argument was silently dropped, so typed templates matched
/// by name and kind only (W3C match-164: every attribute matched the first typed template).
/// The type argument stays a no-op for basic (non-schema-aware) processors.
/// </summary>
public class TypedPatternDispatchTests
{
    private const string TestNs = "urn:tpd";

    private const string ImportSchema = "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
        "targetNamespace='urn:tpd' xmlns:p='urn:tpd' elementFormDefault='qualified' attributeFormDefault='qualified'>" +
        "<xs:element name='order' type='p:orderType'/>" +
        "<xs:complexType name='orderType'><xs:sequence>" +
        "<xs:element name='part' type='p:partNumberType'/>" +
        "<xs:element name='count' type='xs:int'/>" +
        "<xs:element name='codes' type='xs:NMTOKENS'/>" +
        "<xs:element name='parts' type='p:partListType'/>" +
        "<xs:element name='opt' type='p:partNumberType' nillable='true'/>" +
        "</xs:sequence>" +
        "<xs:attribute name='id' type='p:partNumberType'/>" +
        "<xs:attribute name='tags' type='p:partListType'/>" +
        "<xs:attribute name='colors' type='xs:NMTOKENS'/>" +
        "<xs:attribute name='uni' type='p:partOrInteger'/>" +
        "</xs:complexType>" +
        "<xs:simpleType name='partNumberType'><xs:restriction base='xs:string'><xs:pattern value='\\d{3}-[A-Z]{2}'/></xs:restriction></xs:simpleType>" +
        "<xs:simpleType name='partListType'><xs:list itemType='p:partNumberType'/></xs:simpleType>" +
        "<xs:simpleType name='partOrInteger'><xs:union memberTypes='p:partNumberType xs:integer'/></xs:simpleType>" +
        "</xs:schema></xsl:import-schema>";

    private static XmlSchemaSet CompileSchema()
    {
        // The annotation set mirrors the stylesheet's inline import-schema declaration.
        const string schemaText = """
            <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:tpd' xmlns:p='urn:tpd' elementFormDefault='qualified' attributeFormDefault='qualified'>
                <xs:element name='order' type='p:orderType'/>
                <xs:complexType name='orderType'>
                    <xs:sequence>
                        <xs:element name='part' type='p:partNumberType'/>
                        <xs:element name='count' type='xs:int'/>
                        <xs:element name='codes' type='xs:NMTOKENS'/>
                        <xs:element name='parts' type='p:partListType'/>
                        <xs:element name='opt' type='p:partNumberType' nillable='true'/>
                    </xs:sequence>
                    <xs:attribute name='id' type='p:partNumberType'/>
                    <xs:attribute name='tags' type='p:partListType'/>
                    <xs:attribute name='colors' type='xs:NMTOKENS'/>
                    <xs:attribute name='uni' type='p:partOrInteger'/>
                </xs:complexType>
                <xs:simpleType name='partNumberType'>
                    <xs:restriction base='xs:string'><xs:pattern value='\d{3}-[A-Z]{2}'/></xs:restriction>
                </xs:simpleType>
                <xs:simpleType name='partListType'><xs:list itemType='p:partNumberType'/></xs:simpleType>
                <xs:simpleType name='partOrInteger'><xs:union memberTypes='p:partNumberType xs:integer'/></xs:simpleType>
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
        var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
        var source = new XDocument(new XElement(ns + "order",
            new XAttribute(ns + "id", "123-AB"),
            new XAttribute(ns + "tags", "111-AA 222-BB"),
            new XAttribute(ns + "colors", "red blue"),
            new XAttribute(ns + "uni", "100"),
            new XElement(ns + "part", "123-AB"),
            new XElement(ns + "count", "7"),
            new XElement(ns + "codes", "red blue"),
            new XElement(ns + "parts", "111-AA 222-BB"),
            new XElement(ns + "opt", new XAttribute(xsi + "nil", "true"))));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, CompileSchema());
        return source;
    }

    private static string RunTransform(string xslBodyAndDecls, XDocument? source = null, bool schemaAware = true, string? xpathDefaultNamespace = null)
    {
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:tpd' xmlns:xs='http://www.w3.org/2001/XMLSchema'"
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

    // ----- element(*, T) dispatch by type annotation -----

    [Fact]
    public void ElementStarType_MatchesOnlyElementsWithThatType()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/p:order/*'/></out>
            </xsl:template>
            <xsl:template match='element(*, p:partNumberType)'>TYPED[<xsl:value-of select='name()'/>]</xsl:template>
            <xsl:template match='element(*)'>PLAIN[<xsl:value-of select='name()'/>]</xsl:template>
            """);
        // part is partNumberType (TYPED), the nilled opt is not matched (see nilled tests),
        // count/codes/parts fall through to the wildcard template.
        Assert.Contains("TYPED[part]", result);
        Assert.Contains("PLAIN[count]", result);
        Assert.Contains("PLAIN[codes]", result);
        Assert.Contains("PLAIN[parts]", result);
    }

    [Fact]
    public void ElementNameType_MatchesNameAndType()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/p:order/*'/></out>
            </xsl:template>
            <xsl:template match='element(p:count, p:partNumberType)'>WRONG</xsl:template>
            <xsl:template match='element(p:count, xs:int)'>RIGHT[<xsl:value-of select='.'/>]</xsl:template>
            """);
        Assert.DoesNotContain("WRONG", result);
        Assert.Contains("RIGHT[7]", result);
    }

    // ----- attribute(*, T) dispatch by type annotation -----

    [Fact]
    public void AttributeStarType_MatchesOnlyAttributesWithThatType()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/p:order/@*'/></out>
            </xsl:template>
            <xsl:template match='attribute(*, p:partNumberType)'>PN[<xsl:value-of select='name()'/>]</xsl:template>
            <xsl:template match='attribute(*)'>OTHER[<xsl:value-of select='name()'/>]</xsl:template>
            """);
        Assert.Contains("PN[id]", result);
        Assert.Contains("OTHER[tags]", result);
        Assert.Contains("OTHER[colors]", result);
        Assert.Contains("OTHER[uni]", result);
    }

    [Fact]
    public void AttributeNameType_UnprefixedNameStaysInNoNamespace()
    {
        // match-205 shape: an unprefixed attribute name in the kind test is in no namespace,
        // so it must not match the namespace-qualified p:id even though the type matches.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/p:order/@*'/></out>
            </xsl:template>
            <xsl:template match='attribute(id, p:partNumberType)'>WRONG</xsl:template>
            <xsl:template match='attribute(*)'>OK</xsl:template>
            """);
        Assert.DoesNotContain("WRONG", result);
        Assert.Contains("OK", result);
    }

    // ----- derived-type matching -----

    [Fact]
    public void DerivedType_MatchesBaseTypeTarget()
    {
        // partNumberType restricts xs:string, so an id attribute (typed partNumberType)
        // also matches attribute(*, xs:string).
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/p:order/@*'/></out>
            </xsl:template>
            <xsl:template match='attribute(*, xs:string)'>STRINGY[<xsl:value-of select='name()'/>]</xsl:template>
            <xsl:template match='attribute(*)'>OTHER[<xsl:value-of select='name()'/>]</xsl:template>
            """);
        Assert.Contains("STRINGY[id]", result);
        Assert.Contains("OTHER[colors]", result);
    }

    [Fact]
    public void UnionType_MatchesMemberTypedAttribute()
    {
        // The uni attribute is typed by the union p:partOrInteger and matches it exactly;
        // a union member type also matches a pattern naming the union (match-164 shape).
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/p:order/@*'/></out>
            </xsl:template>
            <xsl:template match='attribute(*, p:partOrInteger)'>UNION[<xsl:value-of select='name()'/>]</xsl:template>
            <xsl:template match='attribute(*)'/>
            """);
        Assert.Contains("UNION[uni]", result);
    }

    [Fact]
    public void ListType_DoesNotMatchBuiltinListType()
    {
        // match-164 shape: a user-defined list type is not derived from xs:NMTOKENS,
        // even though both are list types.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/p:order/@*'/></out>
            </xsl:template>
            <xsl:template match='attribute(*, xs:NMTOKENS)'>TOKENS[<xsl:value-of select='name()'/>]</xsl:template>
            <xsl:template match='attribute(*, p:partListType)'>LIST[<xsl:value-of select='name()'/>]</xsl:template>
            <xsl:template match='attribute(*)'/>
            """);
        Assert.Contains("TOKENS[colors]", result);
        Assert.Contains("LIST[tags]", result);
        Assert.DoesNotContain("TOKENS[tags]", result);
        Assert.DoesNotContain("LIST[colors]", result);
    }

    // ----- nilled elements -----

    [Fact]
    public void NilledElement_DoesNotMatchWithoutNillableMarker()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/p:order/p:opt'/></out>
            </xsl:template>
            <xsl:template match='element(*, p:partNumberType)'>WRONG</xsl:template>
            <xsl:template match='element(p:opt)'>OK</xsl:template>
            """);
        Assert.DoesNotContain("WRONG", result);
        Assert.Contains("OK", result);
    }

    [Fact]
    public void NilledElement_MatchesWithNillableMarker()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/p:order/p:opt'/></out>
            </xsl:template>
            <xsl:template match='element(*, p:partNumberType?)'>NILLABLE-OK</xsl:template>
            <xsl:template match='element(*)'>WRONG</xsl:template>
            """);
        Assert.Contains("NILLABLE-OK", result);
        Assert.DoesNotContain("WRONG", result);
    }

    // ----- type-name resolution -----

    [Fact]
    public void UnprefixedTypeName_UsesXPathDefaultNamespace()
    {
        // match-165 shape: an unprefixed type name expands against the
        // xpath-default-namespace in scope on the template.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/p:order/@*'/></out>
            </xsl:template>
            <xsl:template match='attribute(*, partNumberType)' xpath-default-namespace='urn:tpd'>PN[<xsl:value-of select='name()'/>]</xsl:template>
            <xsl:template match='attribute(*)'/>
            """);
        Assert.Contains("PN[id]", result);
    }

    // ----- default priority (XSLT 3.0 §6.4) -----

    [Fact]
    public void TypedPattern_DefaultPriority_BeatsPlainQName()
    {
        // element(*, T) has default priority 0.25, a plain QName pattern has 0.0
        // (match-174 depends on this ordering).
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/p:order/p:part'/></out>
            </xsl:template>
            <xsl:template match='p:part'>PLAIN</xsl:template>
            <xsl:template match='element(*, p:partNumberType)'>TYPED</xsl:template>
            """);
        Assert.Contains("TYPED", result);
        Assert.DoesNotContain("PLAIN", result);
    }

    // ----- basic processor: type argument ignored -----

    [Fact]
    public void BasicProcessor_TypeArgumentIgnored()
    {
        // Without a schema set in scope the type argument is ignored and the kind test
        // matches by kind alone — the pre-REQ-105 behavior is bit-identical.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/order/*'/></out>
            </xsl:template>
            <xsl:template match='element(*, xs:string)'>ANY-ELEMENT[<xsl:value-of select='name()'/>]</xsl:template>
            """,
            source: new XDocument(new XElement("order", new XElement("part", "x"), new XElement("count", "7"))),
            schemaAware: false);
        Assert.Contains("ANY-ELEMENT[part]", result);
        Assert.Contains("ANY-ELEMENT[count]", result);
    }
}
