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
