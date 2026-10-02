// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 02 oktober 2026
// PURPOSE              : Unit tests for XSLT 3.0 §11.9 construction/result-validation schema scoping (validation-0201)
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 02-10-2026     | Creation (validation-0201: stylesheet-import vs secondary host schema scoping)           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Schema;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for the validation-0201 schema-scope split: host <c>stylesheet-import</c> schemas
/// (XsltCompiler.SchemaSet) are part of the stylesheet's in-scope schema definitions and
/// annotate lax-validated constructed trees (default attributes appear), while host
/// <c>secondary</c> schemas (XsltCompiler.EnvironmentSchemaSet) are source-validation
/// context only and must never annotate constructed/result trees or satisfy xsl:type.
/// </summary>
public class ValidationSchemaScopingTests
{
    // item: element-only complex type with a default-valued attribute; code: a named
    // simple type used through xsl:type.
    private const string ScopeSchema = """
        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:scope' xmlns:s='urn:scope' elementFormDefault='qualified'>
            <xs:element name='item' type='s:itemType'/>
            <xs:complexType name='itemType'>
                <xs:sequence>
                    <xs:element name='id' type='xs:integer'/>
                </xs:sequence>
                <xs:attribute name='origin' type='xs:string' default='host'/>
            </xs:complexType>
            <xs:simpleType name='code'>
                <xs:restriction base='xs:string'/>
            </xs:simpleType>
        </xs:schema>
        """;

    private const string LaxItemStylesheet = """
        <xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:s='urn:scope'>
            <xsl:template name='main'>
                <s:item xsl:validation='lax'>
                    <s:id>1</s:id>
                </s:item>
            </xsl:template>
        </xsl:stylesheet>
        """;

    private const string TypedThingStylesheet = """
        <xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:s='urn:scope'>
            <xsl:template name='main'>
                <s:thing xsl:type='s:code'>abc</s:thing>
            </xsl:template>
        </xsl:stylesheet>
        """;

    private static XmlSchemaSet CompileScopeSchema()
    {
        var set = new XmlSchemaSet();
        set.Add(XmlSchema.Read(new StringReader(ScopeSchema), null)!);
        set.Compile();
        return set;
    }

    private static string Run(string xsl, Xslt.Api.XsltCompiler compiler)
    {
        var executable = compiler.Compile(xsl, "file:///scope-test.xsl");
        var src = (IXdmNode)new XDocumentNode(new XDocument(new XElement("dummy")));
        return executable.TransformToString(src, initialTemplate: "main");
    }

    // ----- happy path: stylesheet-import host schema IS construction-validation scope -----

    [Fact]
    public void LaxValidation_HostStylesheetImportSchema_DefaultAttributeAdded()
    {
        var compiler = new Xslt.Api.XsltCompiler
        {
            SchemaAware = true,
            SchemaSet = CompileScopeSchema(),
        };

        var result = Run(LaxItemStylesheet, compiler);

        // The element declaration is in scope for construction validation: lax validity
        // assessment applies its default attribute (import-schema-186 family semantics).
        Assert.Contains("origin=\"host\"", result);
    }

    [Fact]
    public void XslType_HostStylesheetImportSchema_ResolvesAndValidates()
    {
        var compiler = new Xslt.Api.XsltCompiler
        {
            SchemaAware = true,
            SchemaSet = CompileScopeSchema(),
        };

        var result = Run(TypedThingStylesheet, compiler);

        Assert.Contains("abc", result);
    }

    // ----- edge/negative path: secondary host schema is NOT construction-validation scope -----

    [Fact]
    public void LaxValidation_HostSecondarySchema_NoDefaultAttributeAdded()
    {
        var compiler = new Xslt.Api.XsltCompiler
        {
            SchemaAware = true,
            EnvironmentSchemaSet = CompileScopeSchema(),
        };

        var result = Run(LaxItemStylesheet, compiler);

        // validation-0201: the secondary schema is source-validation context only; the
        // lax-validated constructed element must not pick up its default attribute.
        Assert.DoesNotContain("origin=", result);
        Assert.Contains("<s:item", result);
        Assert.Contains("<s:id>1</s:id>", result);
    }

    [Fact]
    public void XslType_HostSecondarySchema_XTSE1520()
    {
        var compiler = new Xslt.Api.XsltCompiler
        {
            SchemaAware = true,
            EnvironmentSchemaSet = CompileScopeSchema(),
        };

        var ex = Assert.ThrowsAny<InvalidOperationException>(() => Run(TypedThingStylesheet, compiler));
        Assert.Contains("XTSE1520", ex.Message);
    }

    [Fact]
    public void Strict_XmlLangAttribute_LocationlessXmlNamespaceImport_SynthesizedDeclarationsInScope()
    {
        // attribute-1501/1502/1503: a locationless import of the XML namespace adds no
        // document, but the synthesized xml.xsd declarations (xml:lang/xml:space/...) must
        // still be in the construction-validation scope.
        const string xsl = """
            <xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
                <xsl:import-schema namespace='http://www.w3.org/XML/1998/namespace'/>
                <xsl:template name='main'>
                    <out>
                        <e>
                            <xsl:attribute name='xml:lang' select="'en-US'" validation='strict'/>
                        </e>
                    </out>
                </xsl:template>
            </xsl:stylesheet>
            """;
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };

        var result = Run(xsl, compiler);

        Assert.Contains("xml:lang=\"en-US\"", result);
    }

    [Fact]
    public void Strict_XmlLangAttribute_InvalidValue_XTTE1510()
    {
        const string xsl = """
            <xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
                <xsl:import-schema namespace='http://www.w3.org/XML/1998/namespace'/>
                <xsl:template name='main'>
                    <out>
                        <e>
                            <xsl:attribute name='xml:lang' select="'!@$%^*'" validation='strict'/>
                        </e>
                    </out>
                </xsl:template>
            </xsl:stylesheet>
            """;
        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };

        var ex = Assert.ThrowsAny<InvalidOperationException>(() => Run(xsl, compiler));
        Assert.Contains("XTTE1510", ex.Message);
    }

    [Fact]
    public void StrictValidation_HostSecondarySchema_XTTE1512()
    {
        const string strictXsl = """
            <xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:s='urn:scope'>
                <xsl:template name='main'>
                    <s:item xsl:validation='strict'>
                        <s:id>1</s:id>
                    </s:item>
                </xsl:template>
            </xsl:stylesheet>
            """;
        var compiler = new Xslt.Api.XsltCompiler
        {
            SchemaAware = true,
            EnvironmentSchemaSet = CompileScopeSchema(),
        };

        // Strict validation requires a top-level declaration in the imported component set;
        // the secondary schema's declaration does not count.
        var ex = Assert.ThrowsAny<InvalidOperationException>(() => Run(strictXsl, compiler));
        Assert.Contains("XTTE1512", ex.Message);
    }
}
