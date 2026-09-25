// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 september 2026
// PURPOSE              : Unit tests for NOTATION-derived casting, grouping/key atomization, and nested schema imports (REQ-106)
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
//                      | Charles Korthout | 0.2   | 25-09-2026     | REQ-108: no-namespace instance-of asserts identity on a cast (user-typed) value          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for the REQ-106 (Bosak.Schema PA-3) notation surface: casts to NOTATION-derived
/// user types annotate the result <c>xs:NOTATION</c> (so <c>instance of xs:NOTATION</c> and
/// <c>as="xs:NOTATION"</c> coercion succeed); the §19.3 cast matrix restricts
/// namespace-sensitive targets to the stringish source family (xs:anyURI is not castable);
/// unprefixed type names in <c>instance of</c> resolve against no-namespace schema types;
/// <c>xs:QName()</c> accepts a QName-kind (NOTATION) argument (XPath 3.0); grouping keys and
/// <c>xsl:key</c> values atomize schema-annotated nodes to their PSVI typed value; and
/// locationful nested <c>xs:import</c> targets of imported schemas are loaded eagerly
/// (XmlSchemaSet.Compile fetches nothing with a null resolver).
/// </summary>
public class NotationAndNestedImportTests
{
    private const string NotaNs = "urn:nota";

    private const string NotaSchema = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
        $"targetNamespace='{NotaNs}' xmlns:n='{NotaNs}' elementFormDefault='qualified'>" +
        "<xs:notation name='mp3' public='audio/mpeg'/>" +
        "<xs:notation name='wav' public='audio/wav'/>" +
        "<xs:simpleType name='nota'><xs:restriction base='xs:NOTATION'>" +
        "<xs:enumeration value='n:mp3'/><xs:enumeration value='n:wav'/>" +
        "</xs:restriction></xs:simpleType>" +
        "<xs:element name='root'><xs:complexType><xs:sequence>" +
        "<xs:element name='item' maxOccurs='unbounded'><xs:complexType>" +
        "<xs:attribute name='nval' type='n:nota'/><xs:attribute name='name' type='xs:string'/>" +
        "</xs:complexType></xs:element></xs:sequence></xs:complexType></xs:element>" +
        "</xs:schema>";

    private static XDocument ValidatedSource()
    {
        var ns = XNamespace.Get(NotaNs);
        var doc = new XDocument(new XElement(ns + "root",
            new XElement(ns + "item", new XAttribute("nval", "n:mp3"), new XAttribute("name", "a")),
            new XElement(ns + "item", new XAttribute("nval", "n:mp3"), new XAttribute("name", "b")),
            new XElement(ns + "item", new XAttribute("nval", "n:wav"), new XAttribute("name", "c"))));
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new StringReader(NotaSchema), null)!);
        set.Compile();
        XdmSchemaAnnotator.ValidateSubtree(doc.Root!, set);
        return doc;
    }

    private static string RunTransform(string xslBodyAndDecls, XDocument? source = null)
    {
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:n='" + NotaNs + "' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
            + "<xsl:import-schema namespace='" + NotaNs + "'><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' "
            + "targetNamespace='" + NotaNs + "' xmlns:n='" + NotaNs + "' elementFormDefault='qualified'>"
            + "<xs:notation name='mp3' public='audio/mpeg'/><xs:notation name='wav' public='audio/wav'/>"
            + "<xs:simpleType name='nota'><xs:restriction base='xs:NOTATION'>"
            + "<xs:enumeration value='n:mp3'/><xs:enumeration value='n:wav'/>"
            + "</xs:restriction></xs:simpleType>"
            + "<xs:element name='root'><xs:complexType><xs:sequence>"
            + "<xs:element name='item' maxOccurs='unbounded'><xs:complexType>"
            + "<xs:attribute name='nval' type='n:nota'/><xs:attribute name='name' type='xs:string'/>"
            + "</xs:complexType></xs:element></xs:sequence></xs:complexType></xs:element>"
            + "</xs:schema></xsl:import-schema>"
            + xslBodyAndDecls
            + "</xsl:stylesheet>";
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var src = (IXdmNode)new XDocumentNode(source ?? ValidatedSource());
        return executable.TransformToString(src);
    }

    [Fact]
    public void NotationDerivedCast_AnnotatesNotation_InstanceOfNotation()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:value-of select='n:nota("n:mp3") instance of xs:NOTATION'/></out>
            </xsl:template>
            """);
        Assert.Contains("true", result);
    }

    [Fact]
    public void NotationDerivedCast_CoercesToNotationVariableType()
    {
        // The notation-0001 shape: a NOTATION-derived constructor value must satisfy
        // as="xs:NOTATION" coercion (previously XTTE0570).
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='s' select='n:nota("n:mp3")' as='xs:NOTATION'/>
                <out><xsl:value-of select='namespace-uri-from-QName(xs:QName($s))'/></out>
            </xsl:template>
            """);
        Assert.Contains(NotaNs, result);
    }

    [Fact]
    public void NotationDerivedCast_FromAnyUri_NotCastable()
    {
        // §19.3: namespace-sensitive targets admit only the stringish source family.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:value-of select='xs:anyURI("urn:x") castable as n:nota'/></out>
            </xsl:template>
            """);
        Assert.Contains("false", result);
    }

    [Fact]
    public void GroupBy_NotationTypedAttribute_GroupsByQNameValue()
    {
        // a and b share the notation {urn:nota}mp3; c is {urn:nota}wav.
        var result = RunTransform("""
            <xsl:template match='/n:root'>
                <out><xsl:for-each-group select='n:item' group-by='@nval'><xsl:value-of select='@name'/></xsl:for-each-group></out>
            </xsl:template>
            """);
        Assert.True(result.Contains(">ac</out>"), $"got: {result}");
    }

    [Fact]
    public void KeyLookup_NotationTypedAttribute_MatchesByQNameValue()
    {
        var result = RunTransform("""
            <xsl:key name='k' match='n:item' use='@nval'/>
            <xsl:template match='/n:root'>
                <out><xsl:value-of select='key("k", data(n:item[1]/@nval))/@name'/></out>
            </xsl:template>
            """);
        Assert.Contains("a b", result.Replace("  ", " "));
    }

    [Fact]
    public void NestedImportSchemaLocation_LoadedRelativeToImportingSchema()
    {
        // The parent schema imports the child by relative schemaLocation; the child declares
        // the element the parent's content model references. Compile fetches nothing with a
        // null resolver, so the nested target must be loaded eagerly (notation-0301 family).
        var dir = Path.Combine(Path.GetTempPath(), "req106-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "child.xsd"),
                "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:child'>"
                + "<xs:element name='doc' type='xs:string'/></xs:schema>");
            File.WriteAllText(Path.Combine(dir, "parent.xsd"),
                "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:parent' xmlns:c='urn:child'>"
                + "<xs:import namespace='urn:child' schemaLocation='child.xsd'/>"
                + "<xs:element name='root'><xs:complexType><xs:sequence>"
                + "<xs:element ref='c:doc'/></xs:sequence></xs:complexType></xs:element></xs:schema>");

            var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:c='urn:child'>"
                + "<xsl:import-schema namespace='urn:parent' schema-location='parent.xsd'/>"
                + "<xsl:template match='/'><out/></xsl:template>"
                + "</xsl:stylesheet>";
            var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
            var executable = compiler.Compile(xsl, new Uri(Path.Combine(dir, "test.xsl")).AbsoluteUri);
            Assert.NotNull(executable);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void UnprefixedTypeName_NoNamespaceSchema_InstanceOf()
    {
        // notation-0101/0102 shape: with no xpath-default-namespace, an unprefixed type name
        // resolves against a no-namespace imported schema (was XPST0051).
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
            + "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
            + "<xs:notation name='mp3' public='audio/mpeg'/>"
            + "<xs:simpleType name='nota'><xs:restriction base='xs:NOTATION'>"
            + "<xs:enumeration value='mp3'/></xs:restriction></xs:simpleType></xs:schema></xsl:import-schema>"
            + "<xsl:template match='/'><out><xsl:value-of select=\"('mp3' cast as nota) instance of nota\"/></out></xsl:template>"
            + "</xsl:stylesheet>";
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var src = (IXdmNode)new XDocumentNode(new XDocument(new XElement("doc")));
        var result = executable.TransformToString(src);
        Assert.Contains("true", result);
    }
}
