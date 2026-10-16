// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies fixed namespace admission on supported stylesheet module roots.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 16-10-2026     | Correct XML attribute quoting in the independent binding-semantics fixture.                  |
//                      | Charles Korthout | 0.3   | 16-10-2026     | Isolate package template name and add namespace classification and local-version controls.   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies fixed namespace admission without broadening instruction attributes.</summary>
[Collection("XSLT package registry")]
public class FixedNamespaceValidationTests
{
    private static string Sheet(string root, string attributes = "fixed-namespaces='#standard t=urn:types'", string version = "4.0", string body = "", string declarations = "") =>
        "<xsl:" + root + " xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='" + version + "' " +
        (root == "package" ? "name='urn:fixed-validation' package-version='1.0' " : "") + attributes + ">" + declarations +
        "<xsl:template name='start'" + (root == "package" ? " visibility='public'" : "") + ">" + body + "</xsl:template></xsl:" + root + ">";

    private static XsltCompiler Compiler(bool draft = true) => new() { ProcessorProfile = draft ? XsltProcessorProfile.Xslt40Draft : XsltProcessorProfile.Legacy };

    /// <summary>Admits bindings on each module root and retains their actual type-name semantics.</summary>
    /// <param name="root">The stylesheet module root.</param>
    /// <param name="draft">Whether the opt-in draft processor is selected.</param>
    [Theory]
    [InlineData("stylesheet", true)]
    [InlineData("stylesheet", false)]
    [InlineData("transform", true)]
    [InlineData("transform", false)]
    [InlineData("package", true)]
    [InlineData("package", false)]
    public void ModuleBindingSemantics(string root, bool draft)
    {
        var source = Sheet(root, body: "<xsl:sequence xmlns:t='urn:wrong' select='7 instance of t:number'/>",
            declarations: "<xsl:item-type name='t:number' as='xs:integer'/>");
        Assert.True(Compiler(draft).Compile(source).Transform(null, initialTemplate: "start", rawResult: true).BooleanValue);
    }

    /// <summary>Preserves the existing binding validator's errors instead of masking them with XTSE0090.</summary>
    /// <param name="root">The module root.</param>
    /// <param name="bindings">The invalid binding declaration.</param>
    [Theory]
    [InlineData("stylesheet", "xmlns=urn:bad")]
    [InlineData("stylesheet", "xml=urn:bad")]
    [InlineData("stylesheet", "p=")]
    [InlineData("stylesheet", "=urn:bad")]
    [InlineData("stylesheet", "bad:prefix=urn:bad")]
    [InlineData("transform", "xmlns=urn:bad")]
    [InlineData("transform", "xml=urn:bad")]
    [InlineData("transform", "p=")]
    [InlineData("transform", "=urn:bad")]
    [InlineData("transform", "bad:prefix=urn:bad")]
    [InlineData("package", "xmlns=urn:bad")]
    [InlineData("package", "xml=urn:bad")]
    [InlineData("package", "p=")]
    [InlineData("package", "=urn:bad")]
    [InlineData("package", "bad:prefix=urn:bad")]
    public void InvalidBindings(string root, string bindings) => Assert.Contains("XTSE0122",
        Assert.ThrowsAny<Exception>(() => Compiler().Compile(Sheet(root, "fixed-namespaces='" + bindings + "'"))).ToString());

    /// <summary>Unknown unqualified module attributes remain rejected next to valid bindings.</summary>
    /// <param name="root">The module root.</param>
    [Theory]
    [InlineData("stylesheet")]
    [InlineData("transform")]
    [InlineData("package")]
    public void UnknownModuleAttribute(string root) => Assert.Contains("XTSE0090",
        Assert.ThrowsAny<Exception>(() => Compiler().Compile(Sheet(root, "fixed-namespaces='#standard' bogus='bad'"))).ToString());

    /// <summary>Legacy 3.0 modules retain their existing unknown-attribute rejection.</summary>
    /// <param name="root">The module root.</param>
    [Theory]
    [InlineData("stylesheet")]
    [InlineData("transform")]
    [InlineData("package")]
    public void LegacyEarlierModule(string root) => Assert.Contains("XTSE0090",
        Assert.ThrowsAny<Exception>(() => Compiler(false).Compile(Sheet(root, version: "3.0"))).ToString());

    /// <summary>The module option does not become a standard instruction attribute.</summary>
    /// <param name="instruction">The instruction receiving the misplaced option.</param>
    [Theory]
    [InlineData("sequence")]
    [InlineData("copy")]
    [InlineData("apply-templates")]
    public void InstructionAttributeRejected(string instruction) => Assert.Contains("XTSE0090",
        Assert.ThrowsAny<Exception>(() => Compiler().Compile(Sheet("stylesheet", body: "<xsl:" + instruction + " fixed-namespaces='#standard'/>"))).ToString());

    /// <summary>Future-version tolerance is retained without bypassing known binding errors.</summary>
    [Fact]
    public void FutureModuleChecksKnownBindings()
    {
        Assert.Equal(7, Compiler().Compile(Sheet("stylesheet", "fixed-namespaces='#standard' bogus='ignored'", "17.0", "<xsl:sequence select='7'/>"))
            .Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);
        Assert.Contains("XTSE0122", Assert.ThrowsAny<Exception>(() => Compiler().Compile(Sheet("stylesheet", "fixed-namespaces='p=' bogus='ignored'", "17.0"))).ToString());
    }

    /// <summary>Imported and included modules retain their own valid fixed bindings.</summary>
    /// <param name="kind">The module relationship.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void ModuleIsolation(string kind)
    {
        var compiler = Compiler();
        compiler.UriResolver = new Resolver(Sheet("stylesheet", "fixed-namespaces='#standard t=urn:included'", declarations: "<xsl:item-type name='t:number' as='xs:integer'/>")
            .Replace("<xsl:template name='start'></xsl:template>", ""));
        var source = Sheet("stylesheet", body: "<xsl:sequence select='7 instance of Q{urn:included}number'/>", declarations: "<xsl:" + kind + " href='module.xsl'/>");
        Assert.True(compiler.Compile(source, "https://example.invalid/main.xsl").Transform(null, initialTemplate: "start", rawResult: true).BooleanValue);
    }

    /// <summary>Used packages reach their own fixed bindings and expose their function result.</summary>
    [Fact]
    public void UsedPackageBindings()
    {
        XsltFunctionLibrary.RegisterPackage("urn:fixed-validation", "1.0", "https://example.invalid/library.xsl");
        try
        {
            var library = Sheet("package", declarations: "<xsl:item-type name='t:number' as='xs:integer'/><xsl:function name='t:probe' visibility='public'><xsl:sequence select='7 instance of t:number'/></xsl:function>").Replace("name='start'", "name='library-start'");
            var compiler = Compiler();
            compiler.UriResolver = new Resolver(library);
            var source = Sheet("stylesheet", body: "<xsl:sequence select='Q{urn:types}probe()'/>", declarations: "<xsl:use-package name='urn:fixed-validation' package-version='1.0'/>");
            Assert.True(compiler.Compile(source).Transform(null, initialTemplate: "start", rawResult: true).BooleanValue);
        }
        finally { XsltFunctionLibrary.ClearPackages(); }
    }

    /// <summary>Foreign metadata with an option-like name is not a fixed-binding declaration.</summary>
    /// <param name="root">The module root.</param>
    [Theory]
    [InlineData("stylesheet")]
    [InlineData("transform")]
    [InlineData("package")]
    public void ForeignAttributeIsMetadata(string root) => Assert.Equal(7,
        Compiler().Compile(Sheet(root, "xmlns:e='urn:metadata' e:fixed-namespaces='p='", body: "<xsl:sequence select='7'/>"))
            .Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);

    /// <summary>XSLT-qualified spellings remain invalid on XSLT module roots.</summary>
    /// <param name="root">The module root.</param>
    [Theory]
    [InlineData("stylesheet")]
    [InlineData("transform")]
    [InlineData("package")]
    public void XsltQualifiedAttributeRejected(string root) => Assert.Contains("XTSE0090",
        Assert.ThrowsAny<Exception>(() => Compiler().Compile(Sheet(root, "xsl:fixed-namespaces='#standard'"))).ToString());

    /// <summary>A local earlier expression version does not remove the module's supported bindings.</summary>
    /// <param name="draft">Whether the opt-in draft processor is selected.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LocalVersionRetainsModuleBindings(bool draft) => Assert.Equal(7,
        Compiler(draft).Compile(Sheet("stylesheet", body: "<xsl:sequence version='3.0' select='xs:integer(7)'/>"))
            .Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);

    private sealed class Resolver(string source) : IXsltUriResolver
    {
        /// <summary>Returns the independent test module.</summary>
        /// <param name="href">The requested module URI.</param>
        /// <param name="baseUri">The referring base URI.</param>
        /// <returns>The parsed module.</returns>
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(source);
    }
}
