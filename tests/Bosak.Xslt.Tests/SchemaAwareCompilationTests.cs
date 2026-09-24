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
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

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
    public void ImportSchema_EmptyDeclaration_ThrowsXtse0010()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:import-schema/>
            <xsl:template name='main'><out/></xsl:template>
        </xsl:stylesheet>";

        var compiler = new Xslt.Api.XsltCompiler { SchemaAware = true };
        var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(xsl));
        Assert.Contains("XTSE0010", ex.Message);
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
}
