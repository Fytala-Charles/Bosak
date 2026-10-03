// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 september 2026
// PURPOSE              : Unit tests for schema-aware XSLT compilation (REQ-097 seam hooks H1/H2: XsltCompiler.SchemaAware, SchemaResolver, SchemaSet).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 23-09-2026     | REQ-102: inert locationless imports, namespace mismatch XTSE0220, xml:lang, include merge|
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 30-09-2026     | REQ-114 (PB-3 C9): empty xsl:import-schema is a legal no-op per XSLT 2.0 §3.14.2        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 30-09-2026     | REQ-114/PB-3 C9: XTSE0020 default-validation version gate, XTSE0770 function-vs-type   |
//                      |                  |       |                | constructor collision, shadowed host-set schema skip, semantic XTSE3070 union identity  |
//                      | Charles Korthout | 0.5   | 03-10-2026     | [Collection("XSLT package registry")] — serializes the global package-registry mutations against the other two registry test classes |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

// Serializes against StylesheetTests and PackageWhitespaceStrippingTests: all three
// mutate the global XsltFunctionLibrary package registry (ClearPackages/RegisterPackage),
// which races under xUnit's default cross-class parallelism.
[Collection("XSLT package registry")]
public class SchemaAwareCompilationTests
{
    private const string SizeSchema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:test' xmlns:t='urn:test' elementFormDefault='qualified'>
        <xs:simpleType name='size'>
            <xs:restriction base='xs:integer'>
                <xs:maxInclusive value='10'/>
            </xs:restriction>
        </xs:simpleType>
    </xs:schema>";

    private static string Run(string xsl, Xslt.Api.XsltCompiler compiler, XDocument? source = null, string? initialTemplate = "main")
    {
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var src = source != null
            ? (IXdmNode)new XDocumentNode(source)
            : (IXdmNode)new XDocumentNode(new XDocument(new XElement("dummy")));
        return executable.TransformToString(src, initialTemplate: initialTemplate);
    }

    // ----- H1: schema-aware compilation mode gates XTSE1650/XTSE1660 -----

    [Fact]
    public void ImportSchema_BasicProcessor_ThrowsXtse1650()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import-schema namespace='urn:test' schema-location='size.xsd'/>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var ex = Assert.Throws<InvalidOperationException>(() => new Xslt.Api.XsltCompiler().Compile(xsl));
        Assert.Contains("XTSE1650", ex.Message);
    }

    [Fact]
    public void ImportSchema_SchemaAware_Compiles()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import-schema namespace='urn:test'>
                " + SizeSchema + @"
            </xsl:import-schema>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var result = Run(xsl, compiler);
        Assert.Contains("<out", result);
    }

    [Fact]
    public void ValidationStrict_BasicProcessor_ThrowsXtse1660()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template name='main'>
                <xsl:copy-of select='.' validation='strict'/>
            </xsl:template>
        </xsl:stylesheet>";

        var ex = Assert.Throws<InvalidOperationException>(() => new Xslt.Api.XsltCompiler().Compile(xsl));
        Assert.Contains("XTSE1660", ex.Message);
    }

    [Fact]
    public void ValidationStrict_SchemaAware_Compiles()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template name='main'>
                <out><xsl:copy-of select='.' validation='strict'/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var result = Run(xsl, compiler);
        Assert.Contains("<out", result);
    }

    // ----- H2: schema resolution — resolver, inline, host set -----

    [Fact]
    public void ImportSchema_SchemaResolver_SuppliesSchemaAndTypeIsVisible()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:test'>
            <xsl:import-schema namespace='urn:test' schema-location='size.xsd'/>
            <xsl:template name='main'>
                <out><xsl:value-of select=""'7' cast as t:size""/></out>
            </xsl:template>
        </xsl:stylesheet>";

        string? requestedNs = null;
        IReadOnlyList<string>? requestedHints = null;
        var compiler = new Xslt.Api.XsltCompiler
        {
            SchemaAware = true,
            SchemaResolver = (ns, hints) =>
            {
                requestedNs = ns;
                requestedHints = hints;
                return new MemoryStream(Encoding.UTF8.GetBytes(SizeSchema));
            },
        };

        var result = Run(xsl, compiler);
        Assert.Contains(">7<", result);
        Assert.Equal("urn:test", requestedNs);
        Assert.Equal(new[] { "size.xsd" }, requestedHints!.ToArray());
    }

    [Fact]
    public void ImportSchema_UnresolvableLocation_ThrowsXtse0220()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import-schema namespace='urn:test' schema-location='no-such-schema-xyz.xsd'/>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl, "file:///test.xsl"));
        Assert.Contains("XTSE0220", ex.Message);
    }

    [Fact]
    public void ImportSchema_HostSchemaSet_SatisfiesNamespaceWithoutLocation()
    {
        var hostSet = new XmlSchemaSet();
        hostSet.Add(XmlSchema.Read(new MemoryStream(Encoding.UTF8.GetBytes(SizeSchema)), null)!);
        hostSet.Compile();

        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:test'>
            <xsl:import-schema namespace='urn:test'/>
            <xsl:template name='main'>
                <out><xsl:value-of select=""t:size(3)""/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true, SchemaSet = hostSet };
        var result = Run(xsl, compiler);
        Assert.Contains(">3<", result);
    }

    [Fact]
    public void ImportSchema_InlineSchema_UserTypeConstructorRegistered()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:test'>
            <xsl:import-schema namespace='urn:test'>
                " + SizeSchema + @"
            </xsl:import-schema>
            <xsl:template name='main'>
                <out><xsl:value-of select=""t:size(8)""/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var result = Run(xsl, compiler);
        Assert.Contains(">8<", result);
    }

    // ----- REQ-102: resolution/merge correctness (PA-1) -----

    [Fact]
    public void ImportSchema_NamespaceOnlyUnresolvable_IsInert()
    {
        // XSLT 3.0 §3.14.1: a locationless import of an unlocatable namespace raises no error
        // while no component from that namespace is used (import-schema-178/184 shape).
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import-schema namespace='urn:does-not-exist-anywhere'/>
            <xsl:template name='main'><out>test</out></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var result = Run(xsl, compiler);
        Assert.Contains("<out>test</out>", result);
    }

    [Fact]
    public void ImportSchema_NamespaceMismatch_ThrowsXtse0220()
    {
        // Every resolvable hint carries a different target namespace and nothing else covers
        // the namespace — import-schema-200 shape.
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import-schema namespace='urn:test' schema-location='wrong-ns.xsd'/>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler
        {
            SchemaAware = true,
            SchemaResolver = (_, _) => new MemoryStream(Encoding.UTF8.GetBytes(
                "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:other'/>")),
        };
        var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl, "file:///test.xsl"));
        Assert.Contains("XTSE0220", ex.Message);
        Assert.Contains("different target namespace", ex.Message);
    }

    [Fact]
    public void ImportSchema_MismatchedHint_HostSetFallbackSucceeds()
    {
        // The schema-location hint yields a document for a different namespace, but the host
        // set covers the declared namespace — import-schema-186 shape (hints are advisory).
        var hostSet = new XmlSchemaSet();
        hostSet.Add(XmlSchema.Read(new MemoryStream(Encoding.UTF8.GetBytes(SizeSchema)), null)!);
        hostSet.Compile();

        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:test'>
            <xsl:import-schema namespace='urn:test' schema-location='wrong-ns.xsd'/>
            <xsl:template name='main'>
                <out><xsl:value-of select=""t:size(4)""/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler
        {
            SchemaAware = true,
            SchemaSet = hostSet,
            SchemaResolver = (_, _) => new MemoryStream(Encoding.UTF8.GetBytes(
                "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:other'/>")),
        };
        var result = Run(xsl, compiler);
        Assert.Contains(">4<", result);
    }

    [Fact]
    public void ImportSchema_NamespaceOmitted_InlineTargetNamespaceImported()
    {
        // import-schema-179 (the spec example): no @namespace — the inline schema's target
        // namespace becomes the imported namespace.
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:y='urn:yes-no'>
            <xsl:import-schema>
                <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:yes-no'>
                    <xs:simpleType name='yes-no'>
                        <xs:restriction base='xs:string'>
                            <xs:enumeration value='yes'/>
                            <xs:enumeration value='no'/>
                        </xs:restriction>
                    </xs:simpleType>
                </xs:schema>
            </xsl:import-schema>
            <xsl:template name='main'>
                <out><xsl:value-of select=""'yes' cast as y:yes-no""/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var result = Run(xsl, compiler);
        Assert.Contains(">yes<", result);
    }

    [Fact]
    public void ImportSchema_BareXmlLangReference_Compiles()
    {
        // The XML namespace is implicitly available in every schema (XSD 1.0 §4.2.6.2);
        // a schema referencing xml:lang without an explicit import must compile.
        const string schemaWithXmlLang = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:test' xmlns:t='urn:test' elementFormDefault='qualified'>
            <xs:element name='doc'>
                <xs:complexType>
                    <xs:attribute ref='xml:lang' use='optional'/>
                </xs:complexType>
            </xs:element>
        </xs:schema>";
        var hostSet = new XmlSchemaSet();
        hostSet.Add(XmlSchema.Read(new MemoryStream(Encoding.UTF8.GetBytes(schemaWithXmlLang)), null)!);

        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import-schema namespace='urn:test'/>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true, SchemaSet = hostSet };
        var result = Run(xsl, compiler);
        Assert.Contains("<out", result);
    }

    [Fact]
    public void ImportSchema_HostIncludeMerge_TypesFromIncludedDocumentVisible()
    {
        // Host set with two documents for one namespace (main includes inc): declarations
        // from the included document must survive the merge (schema066/colors shape).
        var tempDir = Path.Combine(Path.GetTempPath(), "bosak-schema-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var incPath = Path.Combine(tempDir, "inc.xsd");
            var mainPath = Path.Combine(tempDir, "main.xsd");
            File.WriteAllText(incPath, @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:test' elementFormDefault='qualified'>
                <xs:simpleType name='colors'><xs:list><xs:simpleType><xs:restriction base='xs:NCName'/></xs:simpleType></xs:list></xs:simpleType>
            </xs:schema>");
            File.WriteAllText(mainPath, @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:test' elementFormDefault='qualified'>
                <xs:include schemaLocation='inc.xsd'/>
            </xs:schema>");

            var hostSet = new XmlSchemaSet();
            hostSet.Add(null, new Uri(mainPath).AbsoluteUri);
            hostSet.Add(null, new Uri(incPath).AbsoluteUri);

            var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:test'>
                <xsl:import-schema namespace='urn:test'/>
                <xsl:template name='main'>
                    <out><xsl:value-of select=""'red green' cast as t:colors""/></out>
                </xsl:template>
            </xsl:stylesheet>";

            var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true, SchemaSet = hostSet };
            var result = Run(xsl, compiler);
            Assert.Contains("red green", result);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ImportSchema_ExplicitXmlNamespaceImport_StillCompiles()
    {
        // A schema that explicitly imports the XML namespace with a resolvable xml.xsd must
        // not conflict with the predefined declarations added by the builder.
        var tempDir = Path.Combine(Path.GetTempPath(), "bosak-schema-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var xmlXsdPath = Path.Combine(tempDir, "xml.xsd");
            var mainPath = Path.Combine(tempDir, "withimport.xsd");
            File.WriteAllText(xmlXsdPath, @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='http://www.w3.org/XML/1998/namespace'>
                <xs:attribute name='lang' type='xs:string'/>
                <xs:attribute name='space'><xs:simpleType><xs:restriction base='xs:NCName'><xs:enumeration value='default'/><xs:enumeration value='preserve'/></xs:restriction></xs:simpleType></xs:attribute>
                <xs:attribute name='base' type='xs:anyURI'/>
                <xs:attribute name='id' type='xs:ID'/>
            </xs:schema>");
            File.WriteAllText(mainPath, @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:test' elementFormDefault='qualified'>
                <xs:import namespace='http://www.w3.org/XML/1998/namespace' schemaLocation='xml.xsd'/>
                <xs:element name='doc'>
                    <xs:complexType><xs:attribute ref='xml:lang' use='optional'/></xs:complexType>
                </xs:element>
            </xs:schema>");

            var hostSet = new XmlSchemaSet();
            hostSet.Add(null, new Uri(mainPath).AbsoluteUri);

            var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
                <xsl:import-schema namespace='urn:test'/>
                <xsl:template name='main'><out/></xsl:template>
            </xsl:stylesheet>";

            var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true, SchemaSet = hostSet };
            var result = Run(xsl, compiler);
            Assert.Contains("<out", result);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    // ----- declaration validation -----

    [Fact]
    public void ImportSchema_ConflictingSamePrecedence_ThrowsXtse0215()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import-schema namespace='urn:test' schema-location='a.xsd'/>
            <xsl:import-schema namespace='urn:test' schema-location='b.xsd'/>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl));
        Assert.Contains("XTSE0215", ex.Message);
    }

    [Fact]
    public void ImportSchema_EmptyDeclaration_IsLegalNoOp()
    {
        // REQ-114 (PB-3 C9): XSLT 2.0 §3.14.2 — both attributes are optional, so an
        // empty xsl:import-schema declaration is a legal no-op (import-schema-183).
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import-schema/>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var result = Run(xsl, compiler);
        Assert.Contains("<out", result);
    }

    [Fact]
    public void ImportSchema_InvalidInlineContent_ThrowsXtse0220()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import-schema namespace='urn:test'>
                <notASchema/>
            </xsl:import-schema>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl));
        Assert.Contains("XTSE0220", ex.Message);
    }

    // ----- basic-processor behavior is bit-identical (default off) -----

    [Fact]
    public void ImportSchema_DefaultCompiler_RemainsBasicProcessor()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import-schema namespace='urn:test'>
                " + SizeSchema + @"
            </xsl:import-schema>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        // Even with an inline schema available, the default (basic) compiler rejects import-schema.
        var ex = Assert.Throws<InvalidOperationException>(() => new Xslt.Api.XsltCompiler().Compile(xsl));
        Assert.Contains("XTSE1650", ex.Message);
    }

    // ----- REQ-114 (PB-3 C9): static-error and type-identity conformance fixes -----

    [Fact]
    public void DefaultValidation_Strict_BelowVersion3_ThrowsXtse0020()
    {
        // validation-0110: lax/strict default-validation is XTSE0020 in an XSLT 3.0
        // stylesheet; a stylesheet whose effective version is below 3.0 that uses them
        // is flagged on a schema-aware processor.
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' default-validation='strict'>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl));
        Assert.Contains("XTSE0020", ex.Message);
    }

    [Fact]
    public void DefaultValidation_Strict_BasicProcessor_StillThrowsXtse1660()
    {
        // validation-0104 shape: the XTSE1660 basic-processor gate wins over the
        // version gate for strict.
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' default-validation='strict'>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var ex = Assert.Throws<InvalidOperationException>(() => new Xslt.Api.XsltCompiler().Compile(xsl));
        Assert.Contains("XTSE1660", ex.Message);
    }

    [Fact]
    public void DefaultValidation_Preserve_BelowVersion3_Compiles()
    {
        var xsl = @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' default-validation='preserve'>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var result = Run(xsl, compiler);
        Assert.Contains("<out", result);
    }

    [Fact]
    public void UserFunction_NamedLikeImportedSchemaType_ThrowsXtse0770()
    {
        // type-functions-0503: a user xsl:function named like a schema simple type
        // collides with the type's constructor function (XQST0034 analogue).
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:test'>
            <xsl:import-schema namespace='urn:test'>
                " + SizeSchema + @"
            </xsl:import-schema>
            <xsl:function name='t:size' as='xs:integer' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xsl:param name='in' as='xs:integer'/>
                <xsl:sequence select='$in'/>
            </xsl:function>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl));
        Assert.Contains("XTSE0770", ex.Message);
    }

    [Fact]
    public void UserFunction_NamedLikeImportedSchemaType_DifferentArity_Compiles()
    {
        // The constructor collision is arity-specific: t:size#2 does not collide with
        // the arity-1 constructor function.
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:test'>
            <xsl:import-schema namespace='urn:test'>
                " + SizeSchema + @"
            </xsl:import-schema>
            <xsl:function name='t:size' as='xs:integer' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xsl:param name='a' as='xs:integer'/>
                <xsl:param name='b' as='xs:integer'/>
                <xsl:sequence select='$a + $b'/>
            </xsl:function>
            <xsl:template name='main'>
                <out><xsl:value-of select='t:size(1, 2)'/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var result = Run(xsl, compiler);
        Assert.Contains(">3<", result);
    }

    [Fact]
    public void ImportSchema_ShadowedLowerPrecedenceLocation_NotReaddedFromHostSet()
    {
        // import-schema-177 shape: the principal module and its import declare the same
        // namespace with different schema locations; the harness host set carries both
        // documents. Import precedence must drop the imported module's declaration, and
        // the host-set merge must not re-introduce it (the two documents declare the
        // same global element with different definitions — a duplicate-declaration
        // XTSE0220 at Compile if the shadowed document re-enters).
        var tempDir = Path.Combine(Path.GetTempPath(), "bosak-schema-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var winnerPath = Path.Combine(tempDir, "winner.xsd");
            var loserPath = Path.Combine(tempDir, "loser.xsd");
            File.WriteAllText(winnerPath, @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:test' xmlns:t='urn:test' elementFormDefault='qualified'>
                <xs:element name='item' type='t:size'/>
                <xs:simpleType name='size'>
                    <xs:restriction base='xs:integer'>
                        <xs:maxInclusive value='10'/>
                    </xs:restriction>
                </xs:simpleType>
            </xs:schema>");
            File.WriteAllText(loserPath, @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:test' elementFormDefault='qualified'>
                <xs:element name='item' type='xs:string'/>
            </xs:schema>");

            var hostSet = new XmlSchemaSet();
            hostSet.Add(null, new Uri(winnerPath).AbsoluteUri);
            hostSet.Add(null, new Uri(loserPath).AbsoluteUri);

            var mainUri = new Uri(Path.Combine(tempDir, "main.xsl")).AbsoluteUri;
            var baseUri = new Uri(Path.Combine(tempDir, "base.xsl")).AbsoluteUri;
            var resolver = new InMemoryResolver();
            resolver.Add(mainUri, @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:test'>
                <xsl:import href='base.xsl'/>
                <xsl:import-schema namespace='urn:test' schema-location='winner.xsd'/>
                <xsl:template name='main'>
                    <out><xsl:value-of select='t:size(4)'/></out>
                </xsl:template>
            </xsl:stylesheet>");
            resolver.Add(baseUri, @"<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
                <xsl:import-schema namespace='urn:test' schema-location='loser.xsd'/>
            </xsl:stylesheet>");

            var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true, SchemaSet = hostSet, UriResolver = resolver };
            var executable = compiler.Compile(resolver.Resolve(mainUri, null), mainUri);
            var src = (IXdmNode)new XDocumentNode(new XDocument(new XElement("dummy")));
            var result = executable.TransformToString(src, initialTemplate: "main");
            Assert.Contains(">4<", result);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private sealed class InMemoryResolver : Api.IXsltUriResolver
    {
        private readonly Dictionary<string, string> _documents = new();

        public void Add(string uri, string xml) => _documents[uri] = xml;

        public XDocument Resolve(string href, string? baseUri)
        {
            var key = ResolveKey(href, baseUri);
            if (!_documents.TryGetValue(key, out var xml))
                throw new FileNotFoundException($"In-memory document not found: {key}");
            return XDocument.Parse(xml, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        }

        private static string ResolveKey(string href, string? baseUri)
        {
            if (string.IsNullOrEmpty(baseUri))
                return href;
            if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
                return href;
            return new Uri(new Uri(baseUri), href).AbsoluteUri;
        }
    }

    [Fact]
    public void OverrideVariable_UnionSameMembersDifferentOrder_Compiles()
    {
        // override-v-005: union types with the same member-type set are identical
        // regardless of the union's name or the member declaration order.
        var resolver = new InMemoryResolver();
        const string packageUri = "urn:test:union-pkg";
        const string packageLocation = "file:///union-pkg.xsl";
        resolver.Add(packageLocation, @"<xsl:package name='urn:test:union-pkg' package-version='1.0' version='3.0'
            xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
            <xsl:import-schema>
                <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                    <xs:simpleType name='u1'><xs:union memberTypes='xs:date xs:time xs:dateTime'/></xs:simpleType>
                </xs:schema>
            </xsl:import-schema>
            <xsl:variable name='var' as='u1' select='current-dateTime()' visibility='public'/>
        </xsl:package>");

        Api.XsltFunctionLibrary.ClearPackages();
        Api.XsltFunctionLibrary.RegisterPackage(packageUri, "1.0", packageLocation);
        try
        {
            var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xsl:import-schema>
                    <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                        <xs:simpleType name='u2'><xs:union memberTypes='xs:time xs:dateTime xs:date'/></xs:simpleType>
                    </xs:schema>
                </xsl:import-schema>
                <xsl:use-package name='urn:test:union-pkg' package-version='1.0'>
                    <xsl:override>
                        <xsl:variable name='var' as='u2' select='current-dateTime()' visibility='public'/>
                    </xsl:override>
                </xsl:use-package>
                <xsl:template name='main'>
                    <out><xsl:value-of select='$var instance of xs:dateTime'/></out>
                </xsl:template>
            </xsl:stylesheet>";

            var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true, UriResolver = resolver };
            var result = Run(xsl, compiler);
            Assert.Contains(">true<", result);
        }
        finally
        {
            Api.XsltFunctionLibrary.ClearPackages();
        }
    }

    [Fact]
    public void OverrideVariable_UnionDifferentMembers_ThrowsXtse3070()
    {
        // override-v-006 guard: different member-type sets are not identical.
        var resolver = new InMemoryResolver();
        const string packageUri = "urn:test:union-pkg";
        const string packageLocation = "file:///union-pkg.xsl";
        resolver.Add(packageLocation, @"<xsl:package name='urn:test:union-pkg' package-version='1.0' version='3.0'
            xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
            <xsl:import-schema>
                <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                    <xs:simpleType name='u1'><xs:union memberTypes='xs:date xs:time xs:dateTime'/></xs:simpleType>
                </xs:schema>
            </xsl:import-schema>
            <xsl:variable name='var' as='u1' select='current-dateTime()' visibility='public'/>
        </xsl:package>");

        Api.XsltFunctionLibrary.ClearPackages();
        Api.XsltFunctionLibrary.RegisterPackage(packageUri, "1.0", packageLocation);
        try
        {
            var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                <xsl:import-schema>
                    <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                        <xs:simpleType name='u6'><xs:union memberTypes='xs:dateTime xs:date'/></xs:simpleType>
                    </xs:schema>
                </xsl:import-schema>
                <xsl:use-package name='urn:test:union-pkg' package-version='1.0'>
                    <xsl:override>
                        <xsl:variable name='var' as='u6' select='current-dateTime()' visibility='public'/>
                    </xsl:override>
                </xsl:use-package>
                <xsl:template name='main'><out/></xsl:template>
            </xsl:stylesheet>";

            var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true, UriResolver = resolver };
            var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl));
            Assert.Contains("XTSE3070", ex.Message);
        }
        finally
        {
            Api.XsltFunctionLibrary.ClearPackages();
        }
    }

    [Fact]
    public void OverrideFunction_UnionSameMembersDifferentOrder_Compiles()
    {
        // override-f-031 shape: an overriding function whose parameter and return types
        // name a different union with the same member-type set is signature-compatible.
        var resolver = new InMemoryResolver();
        const string packageUri = "urn:test:union-fn-pkg";
        const string packageLocation = "file:///union-fn-pkg.xsl";
        resolver.Add(packageLocation, @"<xsl:package name='urn:test:union-fn-pkg' package-version='1.0' version='3.0'
            xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:p='urn:test:union-fn'>
            <xsl:import-schema>
                <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                    <xs:simpleType name='u1'><xs:union memberTypes='xs:decimal xs:double'/></xs:simpleType>
                </xs:schema>
            </xsl:import-schema>
            <xsl:expose component='function' names='*' visibility='public'/>
            <xsl:function name='p:f' visibility='public' as='u1'>
                <xsl:param name='x' as='u1'/>
                <xsl:sequence select='$x'/>
            </xsl:function>
        </xsl:package>");

        Api.XsltFunctionLibrary.ClearPackages();
        Api.XsltFunctionLibrary.RegisterPackage(packageUri, "1.0", packageLocation);
        try
        {
            var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:p='urn:test:union-fn'>
                <xsl:import-schema>
                    <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                        <xs:simpleType name='u2'><xs:union memberTypes='xs:double xs:decimal'/></xs:simpleType>
                    </xs:schema>
                </xsl:import-schema>
                <xsl:use-package name='urn:test:union-fn-pkg' package-version='1.0'>
                    <xsl:override>
                        <xsl:function name='p:f' as='u2' visibility='public'>
                            <xsl:param name='x' as='u2'/>
                            <xsl:sequence select='xsl:original($x)'/>
                        </xsl:function>
                    </xsl:override>
                </xsl:use-package>
                <xsl:template name='main'>
                    <out><xsl:value-of select='p:f(12e0) instance of xs:double'/></out>
                </xsl:template>
            </xsl:stylesheet>";

            var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true, UriResolver = resolver };
            var result = Run(xsl, compiler);
            Assert.Contains(">true<", result);
        }
        finally
        {
            Api.XsltFunctionLibrary.ClearPackages();
        }
    }
}
