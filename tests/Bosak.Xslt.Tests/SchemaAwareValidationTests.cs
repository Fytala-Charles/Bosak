// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 september 2026
// PURPOSE              : Unit tests for schema-aware validation/@type runtime semantics on constructed nodes (REQ-099 seam H4)
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

using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for the REQ-099 seam H4 runtime semantics: the <c>validation</c> and
/// <c>[xsl:]type</c> attributes on node-construction instructions drive validation through
/// the public <see cref="XdmSchemaAnnotator"/> service when a schema set is in scope, with
/// the XTTE15xx error family; without one they keep the basic-processor no-op behavior.
/// </summary>
public class SchemaAwareValidationTests
{
    private const string TestNs = "urn:h4t";

    // order: element-only complex type (id integer child, required integer code attribute);
    // size: capped integer simple type; complexT: a complex type; qnameList: QName list.
    private const string H4Schema = """
        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:h4t' xmlns:t='urn:h4t' elementFormDefault='qualified'>
            <xs:element name='order' type='t:orderType'/>
            <xs:complexType name='orderType'>
                <xs:sequence>
                    <xs:element name='id' type='xs:integer'/>
                </xs:sequence>
                <xs:attribute name='code' type='xs:integer' use='required'/>
            </xs:complexType>
            <xs:simpleType name='size'>
                <xs:restriction base='xs:integer'>
                    <xs:maxInclusive value='10'/>
                </xs:restriction>
            </xs:simpleType>
            <xs:complexType name='complexT'>
                <xs:sequence>
                    <xs:element name='e'/>
                </xs:sequence>
            </xs:complexType>
            <xs:simpleType name='qnameList'>
                <xs:list itemType='xs:QName'/>
            </xs:simpleType>
            <xs:element name='nillable' nillable='true' type='xs:integer'/>
            <xs:attribute name='limit' type='xs:integer'/>
            <xs:element name='holder'>
                <xs:complexType>
                    <xs:sequence>
                        <xs:any namespace='##any' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>
                    </xs:sequence>
                </xs:complexType>
            </xs:element>
        </xs:schema>
        """;

    private const string ImportSchema = "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
        "targetNamespace='urn:h4t' xmlns:t='urn:h4t' elementFormDefault='qualified'>" +
        "<xs:element name='order' type='t:orderType'/>" +
        "<xs:complexType name='orderType'><xs:sequence><xs:element name='id' type='xs:integer'/></xs:sequence>" +
        "<xs:attribute name='code' type='xs:integer' use='required'/></xs:complexType>" +
        "<xs:simpleType name='size'><xs:restriction base='xs:integer'><xs:maxInclusive value='10'/></xs:restriction></xs:simpleType>" +
        "<xs:complexType name='complexT'><xs:sequence><xs:element name='e'/></xs:sequence></xs:complexType>" +
        "<xs:simpleType name='qnameList'><xs:list itemType='xs:QName'/></xs:simpleType>" +
        "<xs:element name='nillable' nillable='true' type='xs:integer'/>" +
        "<xs:attribute name='limit' type='xs:integer'/>" +
        "<xs:element name='holder'><xs:complexType><xs:sequence>" +
        "<xs:any namespace='##any' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>" +
        "</xs:sequence></xs:complexType></xs:element>" +
        "</xs:schema></xsl:import-schema>";

    private static XmlSchemaSet CompileH4Schema()
    {
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new StringReader(H4Schema), null)!);
        set.Compile();
        return set;
    }

    private static string RunSchemaAware(string xslBodyAndDecls, XDocument? source = null, bool schemaAware = true, bool useMainTemplate = true)
    {
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:h4t' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
            + (schemaAware ? ImportSchema : string.Empty)
            + xslBodyAndDecls
            + "</xsl:stylesheet>";
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = schemaAware };
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var src = (IXdmNode)new XDocumentNode(source ?? new XDocument(new XElement("dummy")));
        return executable.TransformToString(src, initialTemplate: useMainTemplate ? "main" : null);
    }

    // ----- element validation="strict" -----

    [Fact]
    public void Element_Strict_Valid_SucceedsAndAnnotates()
    {
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:variable name='v'>
                    <xsl:element name='t:order' validation='strict'>
                        <xsl:attribute name='code' select='5'/>
                        <xsl:element name='t:id'>7</xsl:element>
                    </xsl:element>
                </xsl:variable>
                <out><xsl:value-of select="$v/t:order/t:id instance of element(*, xs:integer)"/></out>
            </xsl:template>
            """);
        Assert.Contains(">true<", result);
    }

    [Fact]
    public void Element_Strict_InvalidContent_Xtte1510()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:element name='t:order' validation='strict'>
                    <xsl:attribute name='code' select='5'/>
                    <xsl:element name='t:id'>not-an-integer</xsl:element>
                </xsl:element>
            </xsl:template>
            """));
        Assert.Contains("XTTE1510", ex.Message);
    }

    [Fact]
    public void Element_Strict_UndeclaredElement_Xtte1512()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:element name='undeclared' validation='strict'>text</xsl:element>
            </xsl:template>
            """));
        Assert.Contains("XTTE1512", ex.Message);
    }

    [Fact]
    public void Element_Strict_ErrorIsCatchableByXslTry()
    {
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <out>
                    <xsl:try>
                        <xsl:element name='t:order' validation='strict'>
                            <xsl:attribute name='code' select='5'/>
                            <xsl:element name='t:id'>bad</xsl:element>
                        </xsl:element>
                        <xsl:catch errors='*:XTTE1510'>caught</xsl:catch>
                    </xsl:try>
                </out>
            </xsl:template>
            """);
        Assert.Contains("caught", result);
    }

    // ----- literal result elements -----

    [Fact]
    public void LiteralResultElement_XslValidationStrict_Invalid_Xtte1510()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <t:order xsl:validation='strict' code='5'><t:id>bad</t:id></t:order>
            </xsl:template>
            """));
        Assert.Contains("XTTE1510", ex.Message);
    }

    [Fact]
    public void LiteralResultElement_XslType_Valid_Succeeds()
    {
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:variable name='v'><anything xsl:type='t:size'>7</anything></xsl:variable>
                <out><xsl:value-of select="$v/anything instance of element(*, t:size)"/></out>
            </xsl:template>
            """);
        Assert.Contains(">true<", result);
    }

    // ----- element validation="lax" -----

    [Fact]
    public void Element_Lax_UndeclaredRoot_IsValid()
    {
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <out><undeclared xsl:validation='lax'>text</undeclared></out>
            </xsl:template>
            """);
        Assert.Contains("<undeclared>text</undeclared>", result);
    }

    [Fact]
    public void Element_Lax_InvalidDeclaredContent_Xtte1515()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:element name='t:order' validation='lax'>
                    <xsl:attribute name='code' select='5'/>
                    <xsl:element name='t:id'>bad</xsl:element>
                </xsl:element>
            </xsl:template>
            """));
        Assert.Contains("XTTE1515", ex.Message);
    }

    // ----- element @type -----

    [Fact]
    public void Element_Type_Valid_SucceedsAndAnnotates()
    {
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:variable name='v'><xsl:element name='anything' type='t:size'>9</xsl:element></xsl:variable>
                <out><xsl:value-of select="$v/anything instance of element(*, t:size)"/></out>
            </xsl:template>
            """);
        Assert.Contains(">true<", result);
    }

    [Fact]
    public void Element_Type_Invalid_Xtte1540()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:element name='anything' type='t:size'>11</xsl:element>
            </xsl:template>
            """));
        Assert.Contains("XTTE1540", ex.Message);
    }

    [Fact]
    public void Element_Type_UnknownType_Xtse1520()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:element name='anything' type='t:absent'>9</xsl:element>
            </xsl:template>
            """));
        Assert.Contains("XTSE1520", ex.Message);
    }

    [Fact]
    public void Element_Type_BuiltIn_WithoutImportSchema_Invalid_Xtte1540()
    {
        // No xsl:import-schema: built-in types are still in scope (error-1540a shape).
        var xsl = """
            <xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xsl:template name='main'>
                    <e xsl:type='xs:date'>2006-02-31</e>
                </xsl:template>
            </xsl:stylesheet>
            """;
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var src = (IXdmNode)new XDocumentNode(new XDocument(new XElement("dummy")));
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => executable.TransformToString(src, initialTemplate: "main"));
        Assert.Contains("XTTE1540", ex.Message);
    }

    [Fact]
    public void Element_Type_UntypedAtomic_WithElementChildren_Xtte1540()
    {
        // validation-0109 shape: xs:untypedAtomic cannot validate element content.
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <z xsl:type='xs:untypedAtomic'>abcd<a/>wxyz</z>
            </xsl:template>
            """));
        Assert.Contains("XTTE1540", ex.Message);
    }

    // ----- attribute validation -----

    [Fact]
    public void Attribute_Type_Valid_SucceedsAndAnnotates()
    {
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:variable name='v'>
                    <e xsl:validation='preserve'><xsl:attribute name='code' type='t:size' select='7'/></e>
                </xsl:variable>
                <out><xsl:value-of select="$v/e/@code instance of attribute(*, t:size)"/></out>
            </xsl:template>
            """);
        Assert.Contains(">true<", result);
    }

    [Fact]
    public void Attribute_Type_Invalid_Xtte1540()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <out><xsl:attribute name='code' type='t:size' select='11'/></out>
            </xsl:template>
            """));
        Assert.Contains("XTTE1540", ex.Message);
    }

    [Fact]
    public void Attribute_Type_ComplexType_Xtse1530()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <out><xsl:attribute name='code' type='t:complexT' select='7'/></out>
            </xsl:template>
            """));
        Assert.Contains("XTSE1530", ex.Message);
    }

    [Fact]
    public void Attribute_Type_QName_Xtte1545()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <out><xsl:attribute name='code' type='xs:QName' select="'t:x'"/></out>
            </xsl:template>
            """));
        Assert.Contains("XTTE1545", ex.Message);
    }

    [Fact]
    public void Attribute_Lax_DeclaredInvalid_Xtte1515()
    {
        // t:limit is a global attribute declaration (xs:integer); lax validation against it fails.
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <out><xsl:attribute name='t:limit' validation='lax' select="'not-an-integer'"/></out>
            </xsl:template>
            """));
        Assert.Contains("XTTE1515", ex.Message);
    }

    [Fact]
    public void Attribute_Lax_DeclaredValid_Succeeds()
    {
        var result = RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:variable name='v'><e xsl:validation='preserve'><xsl:attribute name='t:limit' validation='lax' select='42'/></e></xsl:variable>
                <out><xsl:value-of select="$v/e/@t:limit instance of attribute(*, xs:integer)"/></out>
            </xsl:template>
            """);
        Assert.Contains(">true<", result);
    }

    [Fact]
    public void Attribute_Standalone_InvalidType_Xtte1555()
    {
        // Parentless attribute (raw-item collection via json output): XTTE1555.
        var result = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:output method='json'/>
            <xsl:template name='main'>
                <xsl:attribute name='code' type='t:size' select='11'/>
            </xsl:template>
            """));
        Assert.Contains("XTTE1555", result.Message);
    }

    // ----- copy-of -----

    [Fact]
    public void CopyOf_Preserve_KeepsTypeAnnotations()
    {
        var schemas = CompileH4Schema();
        var source = new XDocument(new XElement(XNamespace.Get(TestNs) + "order",
            new XAttribute("code", "5"),
            new XElement(XNamespace.Get(TestNs) + "id", "7")));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, schemas);

        var result = RunSchemaAware("""
            <xsl:template match='/'>
                <xsl:variable name='v'>
                    <xsl:copy-of select='/*' validation='preserve'/>
                </xsl:variable>
                <out><xsl:value-of select="$v/t:order instance of element(*, t:orderType)"/></out>
            </xsl:template>
            """, source, useMainTemplate: false);
        Assert.Contains(">true<", result);
    }

    [Fact]
    public void CopyOf_Preserve_KeepsNilledProperty()
    {
        var schemas = CompileH4Schema();
        var source = new XDocument(new XElement(XNamespace.Get(TestNs) + "nillable",
            new XAttribute(XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance") + "nil", "true")));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, schemas);
        Assert.True(XDocumentNode.Wrap(source.Root!).IsNilled);

        var result = RunSchemaAware("""
            <xsl:template match='/'>
                <xsl:variable name='v'>
                    <xsl:copy-of select='/*' validation='preserve'/>
                </xsl:variable>
                <out><xsl:value-of select="nilled($v/t:nillable)"/></out>
            </xsl:template>
            """, source, useMainTemplate: false);
        Assert.Contains(">true<", result);
    }

    [Fact]
    public void CopyOf_Strip_RemovesTypeAnnotations()
    {
        var schemas = CompileH4Schema();
        var source = new XDocument(new XElement(XNamespace.Get(TestNs) + "order",
            new XAttribute("code", "5"),
            new XElement(XNamespace.Get(TestNs) + "id", "7")));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, schemas);

        var result = RunSchemaAware("""
            <xsl:template match='/'>
                <xsl:variable name='v'>
                    <xsl:copy-of select='/*' validation='strip'/>
                </xsl:variable>
                <out><xsl:value-of select="$v/t:order instance of element(*, t:orderType)"/></out>
            </xsl:template>
            """, source, useMainTemplate: false);
        Assert.Contains(">false<", result);
    }

    [Fact]
    public void CopyOf_ValidationStrict_InvalidSource_Xtte1510()
    {
        var source = new XDocument(new XElement(XNamespace.Get(TestNs) + "order",
            new XAttribute("code", "abc"), // invalid against xs:integer
            new XElement(XNamespace.Get(TestNs) + "id", "7")));
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template match='/'>
                <out><xsl:copy-of select='/*' validation='strict'/></out>
            </xsl:template>
            """, source, useMainTemplate: false));
        Assert.Contains("XTTE1510", ex.Message);
    }

    [Fact]
    public void CopyOf_Type_ComplexTypeOnAttribute_Xtte1535()
    {
        var source = new XDocument(new XElement(XNamespace.Get(TestNs) + "order",
            new XAttribute("code", "5"),
            new XElement(XNamespace.Get(TestNs) + "id", "7")));
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template match='/'>
                <out><xsl:copy-of select='//@code' type='t:complexT'/></out>
            </xsl:template>
            """, source, useMainTemplate: false));
        Assert.Contains("XTTE1535", ex.Message);
    }

    // ----- xsl:copy -----

    [Fact]
    public void Copy_ValidationStrict_Valid_Succeeds()
    {
        var source = new XDocument(new XElement(XNamespace.Get(TestNs) + "order",
            new XElement(XNamespace.Get(TestNs) + "id", "7")));
        var result = RunSchemaAware("""
            <xsl:template match='t:order'>
                <xsl:copy validation='strict'>
                    <xsl:attribute name='code' select='5'/>
                    <xsl:copy-of select='t:id'/>
                </xsl:copy>
            </xsl:template>
            <xsl:template name='main'>
                <out><xsl:apply-templates select='/t:order'/></out>
            </xsl:template>
            """, source);
        Assert.Contains("order", result);
        Assert.Contains(">7<", result);
    }

    [Fact]
    public void Copy_ValidationStrict_Invalid_Xtte1510()
    {
        var source = new XDocument(new XElement(XNamespace.Get(TestNs) + "order",
            new XElement(XNamespace.Get(TestNs) + "id", "7")));
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template match='t:order'>
                <xsl:copy validation='strict'>
                    <xsl:copy-of select='t:id'/>
                </xsl:copy>
            </xsl:template>
            <xsl:template name='main'>
                <out><xsl:apply-templates select='/t:order'/></out>
            </xsl:template>
            """, source));
        // Missing required @code attribute.
        Assert.Contains("XTTE1510", ex.Message);
    }

    // ----- documents -----

    [Fact]
    public void Document_Strict_TopLevelText_Xtte1550()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:document validation='strict'><xsl:element name='t:order'><xsl:attribute name='code' select='5'/><xsl:element name='t:id'>7</xsl:element></xsl:element><xsl:text>trailing</xsl:text></xsl:document>
            </xsl:template>
            """));
        Assert.Contains("XTTE1550", ex.Message);
    }

    [Fact]
    public void Document_Strict_InvalidContent_Xtte1510()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:document validation='strict'><xsl:element name='t:order'><xsl:attribute name='code' select='5'/><xsl:element name='t:id'>bad</xsl:element></xsl:element></xsl:document>
            </xsl:template>
            """));
        Assert.Contains("XTTE1510", ex.Message);
    }

    [Fact]
    public void ResultDocument_Strict_TwoRoots_Xtte1550()
    {
        // The validation failure fires before any file is written.
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:result-document href='h4-should-not-be-written.xml' validation='strict'><a/><b/></xsl:result-document>
            </xsl:template>
            """));
        Assert.Contains("XTTE1550", ex.Message);
        Assert.False(System.IO.File.Exists("h4-should-not-be-written.xml"));
    }

    [Fact]
    public void ResultDocument_Strict_InvalidContent_Xtte1510()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <xsl:result-document href='h4-should-not-be-written.xml' validation='strict'><t:order code='5'><t:id>bad</t:id></t:order></xsl:result-document>
            </xsl:template>
            """));
        Assert.Contains("XTTE1510", ex.Message);
    }

    // ----- default-validation inheritance -----

    [Fact]
    public void DefaultValidation_Strict_IsInheritedByConstructedElements()
    {
        // 'undeclared-x' has no global declaration. With default-validation='strict' at the
        // stylesheet root, the constructed element is strictly validated and rejected
        // (XTTE1512) — proving the inheritance (without it the element would pass as strip).
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:h4t' xmlns:xs='http://www.w3.org/2001/XMLSchema' default-validation='strict'>"
            + ImportSchema
            + """
            <xsl:template name='main'>
                <xsl:element name='t:undeclared-x'>ok</xsl:element>
            </xsl:template>
            </xsl:stylesheet>
            """;
        var executable = new Xslt.Api.XsltCompiler { SchemaAware = true }.Compile(xsl, "file:///test.xsl");
        var ex = Assert.ThrowsAny<InvalidOperationException>(() =>
            executable.TransformToString(new XDocumentNode(new XDocument(new XElement("dummy"))), initialTemplate: "main"));
        Assert.Contains("XTTE1512", ex.Message);
    }

    [Fact]
    public void DefaultValidation_Strict_InvalidContent_Xtte1510()
    {
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:h4t' xmlns:xs='http://www.w3.org/2001/XMLSchema' default-validation='strict'>"
            + ImportSchema
            + """
            <xsl:template name='main'>
                <xsl:element name='t:nillable'>not-an-integer</xsl:element>
            </xsl:template>
            </xsl:stylesheet>
            """;
        var executable = new Xslt.Api.XsltCompiler { SchemaAware = true }.Compile(xsl, "file:///test.xsl");
        var ex = Assert.ThrowsAny<InvalidOperationException>(() =>
            executable.TransformToString(new XDocumentNode(new XDocument(new XElement("dummy"))), initialTemplate: "main"));
        Assert.Contains("XTTE1510", ex.Message);
    }

    [Fact]
    public void DefaultValidation_InnerDefaultOverridesOuter()
    {
        // t:holder is globally declared with lax wildcards, so it validates strictly even
        // with an undeclared child; the child's own effective validation comes from the
        // inner xsl:default-validation='strip' and must NOT raise XTTE1512.
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:h4t' xmlns:xs='http://www.w3.org/2001/XMLSchema' default-validation='strict'>"
            + ImportSchema
            + """
            <xsl:template name='main'>
                <t:holder xsl:default-validation='strip'>
                    <xsl:element name='t:undeclared-child'>ok</xsl:element>
                </t:holder>
            </xsl:template>
            </xsl:stylesheet>
            """;
        var executable = new Xslt.Api.XsltCompiler { SchemaAware = true }.Compile(xsl, "file:///test.xsl");
        var result = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("dummy"))), initialTemplate: "main");
        Assert.Contains("undeclared-child", result);
    }

    [Fact]
    public void DefaultValidation_WithoutInnerOverride_UndeclaredChild_Xtte1512()
    {
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:h4t' xmlns:xs='http://www.w3.org/2001/XMLSchema' default-validation='strict'>"
            + ImportSchema
            + """
            <xsl:template name='main'>
                <t:holder>
                    <xsl:element name='t:undeclared-child'>ok</xsl:element>
                </t:holder>
            </xsl:template>
            </xsl:stylesheet>
            """;
        var executable = new Xslt.Api.XsltCompiler { SchemaAware = true }.Compile(xsl, "file:///test.xsl");
        var ex = Assert.ThrowsAny<InvalidOperationException>(() =>
            executable.TransformToString(new XDocumentNode(new XDocument(new XElement("dummy"))), initialTemplate: "main"));
        Assert.Contains("XTTE1512", ex.Message);
    }

    // ----- input-type-annotations -----

    [Fact]
    public void InputTypeAnnotations_Strip_RemovesSourceAnnotations()
    {
        var schemas = CompileH4Schema();
        var source = new XDocument(new XElement(XNamespace.Get(TestNs) + "order",
            new XAttribute("code", "5"),
            new XElement(XNamespace.Get(TestNs) + "id", "7")));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, schemas);

        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:h4t' xmlns:xs='http://www.w3.org/2001/XMLSchema' input-type-annotations='strip'>"
            + ImportSchema
            + """
            <xsl:template match='/'>
                <out><xsl:value-of select='/t:order/t:id instance of element(*, xs:integer)'/></out>
            </xsl:template>
            </xsl:stylesheet>
            """;
        var executable = new Xslt.Api.XsltCompiler { SchemaAware = true }.Compile(xsl, "file:///test.xsl");
        var result = executable.TransformToString(new XDocumentNode(source));
        Assert.Contains(">false<", result);
    }

    [Fact]
    public void InputTypeAnnotations_Preserve_KeepsSourceAnnotations()
    {
        var schemas = CompileH4Schema();
        var source = new XDocument(new XElement(XNamespace.Get(TestNs) + "order",
            new XAttribute("code", "5"),
            new XElement(XNamespace.Get(TestNs) + "id", "7")));
        XdmSchemaAnnotator.ValidateSubtree(source.Root!, schemas);

        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:h4t' xmlns:xs='http://www.w3.org/2001/XMLSchema' input-type-annotations='preserve'>"
            + ImportSchema
            + """
            <xsl:template match='/'>
                <out><xsl:value-of select='/t:order/t:id instance of element(*, xs:integer)'/></out>
            </xsl:template>
            </xsl:stylesheet>
            """;
        var executable = new Xslt.Api.XsltCompiler { SchemaAware = true }.Compile(xsl, "file:///test.xsl");
        var result = executable.TransformToString(new XDocumentNode(source));
        Assert.Contains(">true<", result);
    }

    // ----- bit-identity on a basic processor -----

    [Fact]
    public void BasicProcessor_LaxPreserveStrip_AreNoOps()
    {
        const string body = """
            <xsl:template name='main'>
                <out>
                    <a xsl:validation='lax'>x</a>
                    <b xsl:validation='preserve'>y</b>
                    <c xsl:validation='strip'>z</c>
                </out>
            </xsl:template>
            """;
        var basic = RunSchemaAware(body, schemaAware: false);
        Assert.Contains("<a>x</a>", basic);
        Assert.Contains("<b>y</b>", basic);
        Assert.Contains("<c>z</c>", basic);
    }

    [Fact]
    public void BasicProcessor_Strict_Xtse1660()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <out><a xsl:validation='strict'>x</a></out>
            </xsl:template>
            """, schemaAware: false));
        Assert.Contains("XTSE1660", ex.Message);
    }

    [Fact]
    public void BasicProcessor_Type_Xtse1660()
    {
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => RunSchemaAware("""
            <xsl:template name='main'>
                <out><a xsl:type='xs:string'>x</a></out>
            </xsl:template>
            """, schemaAware: false));
        Assert.Contains("XTSE1660", ex.Message);
    }

    [Fact]
    public void SchemaAware_WithoutSchemaSet_OutputMatchesBasicProcessor()
    {
        // With no schema set in scope, validation="lax"/"strip"/"preserve" are no-ops:
        // the output is identical to the basic processor's.
        const string body = """
            <xsl:template name='main'>
                <out>
                    <a xsl:validation='lax'>x</a>
                    <b xsl:validation='preserve'>y</b>
                    <c xsl:validation='strip'>z</c>
                </out>
            </xsl:template>
            """;
        var schemaAwareNoSchema = RunSchemaAware(body, schemaAware: false);
        var xsl = "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:h4t' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
            + body + "</xsl:stylesheet>";
        var executable = new Xslt.Api.XsltCompiler { SchemaAware = true }.Compile(xsl, "file:///test.xsl");
        var awareResult = executable.TransformToString(new XDocumentNode(new XDocument(new XElement("dummy"))), initialTemplate: "main");
        Assert.Equal(schemaAwareNoSchema, awareResult);
    }
}
