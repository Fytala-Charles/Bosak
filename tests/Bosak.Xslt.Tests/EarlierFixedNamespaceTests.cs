// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies earlier-version fixed bindings and isolated draft compilation contexts.
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
//                      | Charles Korthout | 0.2   | 16-10-2026     | Add XDM prefix and XML 1.1 owned-copy provenance controls; retain separate serializer probe.                              |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies fixed bindings follow processor capability with isolated module contexts.</summary>
[Collection("XSLT package registry")]
public class EarlierFixedNamespaceTests
{
    private const string Function = "<xsl:function name='p:probe'><xsl:sequence select='xs:integer(7)'/></xsl:function>";
    private static string Sheet(string root = "stylesheet", string version = "3.0", string? bindings = "#standard p=urn:fixed", string expression = "Q{urn:fixed}probe()", string? declarations = null) =>
        "<xsl:" + root + " xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' version='" + version + "' " +
        (root == "package" ? "name='urn:earlier-fixed-package' package-version='1.0' " : "") +
        (bindings is null ? "" : "fixed-namespaces='" + bindings + "'") + ">" + (declarations ?? Function) +
        "<xsl:template name='start'" + (root == "package" ? " visibility='public'" : "") + "><xsl:sequence select='" + expression + "'/></xsl:template></xsl:" + root + ">";
    private static XsltCompiler Compiler(bool draft = true) => new() { ProcessorProfile = draft ? XsltProcessorProfile.Xslt40Draft : XsltProcessorProfile.Legacy };
    private static long Run(string source, XsltCompiler? compiler = null) => (compiler ?? Compiler()).Compile(source)
        .Transform(null, initialTemplate: "start", rawResult: true).IntegerValue;

    /// <summary>Draft processor capability enables module bindings independently of declared version.</summary>
    /// <param name="root">The module root.</param>
    /// <param name="version">The module version.</param>
    [Theory]
    [InlineData("stylesheet", "1.0")]
    [InlineData("stylesheet", "2.0")]
    [InlineData("stylesheet", "3.0")]
    [InlineData("stylesheet", "4.0")]
    [InlineData("transform", "1.0")]
    [InlineData("transform", "2.0")]
    [InlineData("transform", "3.0")]
    [InlineData("transform", "4.0")]
    [InlineData("package", "1.0")]
    [InlineData("package", "2.0")]
    [InlineData("package", "3.0")]
    [InlineData("package", "4.0")]
    public void DraftEarlierBindings(string root, string version) => Assert.Equal(7, Run(Sheet(root, version)));

    /// <summary>Legacy 4.0 bindings preserve their existing behavior.</summary>
    /// <param name="root">The module root.</param>
    [Theory]
    [InlineData("stylesheet")]
    [InlineData("transform")]
    [InlineData("package")]
    public void LegacyCurrentBindings(string root) => Assert.Equal(7, Run(Sheet(root, "4.0"), Compiler(false)));

    /// <summary>Earlier legacy modules retain their unknown-attribute rejection.</summary>
    /// <param name="version">The legacy stylesheet version.</param>
    [Theory]
    [InlineData("1.0")]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void LegacyEarlierRejected(string version) => Assert.Contains("XTSE0090",
        Assert.ThrowsAny<Exception>(() => Compiler(false).Compile(Sheet(version: version, expression: "7", declarations: ""))).ToString());

    /// <summary>Earlier draft modules use the existing binding validator's errors.</summary>
    /// <param name="version">The module version.</param>
    /// <param name="bindings">The invalid binding declaration.</param>
    [Theory]
    [InlineData("1.0", "xml=urn:bad")]
    [InlineData("2.0", "xmlns=urn:bad")]
    [InlineData("3.0", "p=")]
    public void InvalidBindings(string version, string bindings) => Assert.Contains("XTSE0122",
        Assert.ThrowsAny<Exception>(() => Compiler().Compile(Sheet(version: version, bindings: bindings, declarations: "", expression: "7"))).ToString());

    /// <summary>Included and imported earlier modules retain their own binding contexts.</summary>
    /// <param name="kind">The module relationship.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void ModuleIsolation(string kind)
    {
        var compiler = Compiler();
        compiler.UriResolver = new Resolver(Sheet(bindings: "#standard p=urn:module").Replace("name='start'", "name='module-start'"));
        Assert.Equal(7, Run(Sheet(bindings: "#standard p=urn:principal", expression: "Q{urn:module}probe()", declarations: "<xsl:" + kind + " href='module.xsl'/>"), compiler));
    }

    /// <summary>Used packages inherit draft capability for their earlier module bindings.</summary>
    [Fact]
    public void UsedPackageBindings()
    {
        XsltFunctionLibrary.RegisterPackage("urn:earlier-fixed-package", "1.0", "https://example.invalid/library.xsl");
        try
        {
            var compiler = Compiler();
            compiler.UriResolver = new Resolver(Sheet("package").Replace("<xsl:function name=", "<xsl:function visibility='public' name=").Replace("name='start'", "name='library-start'"));
            Assert.Equal(7, Run(Sheet(declarations: "<xsl:use-package name='urn:earlier-fixed-package' package-version='1.0'/>"), compiler));
        }
        finally { XsltFunctionLibrary.ClearPackages(); }
    }

    /// <summary>Draft compilation owns module annotations and survives later source/profile reuse.</summary>
    [Fact]
    public void SourceAndProfileIsolation()
    {
        var document = XDocument.Parse(Sheet(declarations: Function + "<xsl:variable name='unused' use-when='false()' select='0'/>"));
        var original = document.ToString(SaveOptions.DisableFormatting);
        var compiler = Compiler();
        var executable = compiler.Compile(document);
        Assert.Equal(original, document.ToString(SaveOptions.DisableFormatting));
        document.Root!.SetAttributeValue("fixed-namespaces", "#standard p=urn:changed");
        compiler.ProcessorProfile = XsltProcessorProfile.Legacy;
        Assert.Contains("XTSE0090", Assert.ThrowsAny<Exception>(() => compiler.Compile(document)).ToString());
        Assert.Equal(7, executable.Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);
    }

    /// <summary>Owned module copies retain reader base URI metadata.</summary>
    [Fact]
    public void BaseUriPreserved()
    {
        using var reader = XmlReader.Create(new StringReader(Sheet(expression: "static-base-uri()", declarations: "")), new XmlReaderSettings(), "https://example.invalid/owned/style.xsl");
        var document = XDocument.Load(reader, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
        Assert.Equal("https://example.invalid/owned/style.xsl", Compiler().Compile(document).Transform(null, initialTemplate: "start", rawResult: true).ToString());
    }

    /// <summary>Earlier modules retain namespace-document resolution through their own resolver.</summary>
    [Fact]
    public void NamespaceDocument()
    {
        var resolver = new Resolver("");
        var compiler = Compiler(); compiler.UriResolver = resolver;
        Assert.Equal(7, compiler.Compile(Sheet(bindings: "bindings.xml"), "https://example.invalid/main.xsl")
            .Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);
        Assert.Equal(1, resolver.BindingRequests);
    }

    /// <summary>Fixed prefixes apply to earlier global variable names and references.</summary>
    [Fact]
    public void VariableBindings() => Assert.Equal(7, Run(Sheet(expression: "$Q{urn:fixed}value", declarations: "<xsl:variable name='p:value' as='xs:integer' select='7'/>")));

    /// <summary>Qualified metadata is not interpreted as a module binding option.</summary>
    [Fact]
    public void QualifiedMetadata() => Assert.Equal(7, Run(Sheet(bindings: null, expression: "7", declarations: "")
        .Replace("xmlns:p='urn:native'", "xmlns:p='urn:native' xmlns:e='urn:metadata' e:fixed-namespaces='p='")));

    /// <summary>Local earlier expressions keep the selected module's fixed bindings.</summary>
    [Fact]
    public void LocalEarlierVersion() => Assert.Equal(7, Run(Sheet().Replace("<xsl:sequence select='Q", "<xsl:sequence version='2.0' select='Q")));

    /// <summary>Owned module copies retain the XDM original prefix when URI bindings are ambiguous.</summary>
    /// <param name="version">The module version.</param>
    [Theory]
    [InlineData("3.0")]
    [InlineData("4.0")]
    public void OwnedPrefixMetadata(string version)
    {
        var source = Sheet(version: version, expression: "7", declarations: "")
            .Replace("<xsl:sequence select='7'/>", "<p:out xmlns:p='urn:output' xmlns:q='urn:output'/>");
        var input = Bosak.XPath.Providers.Xml.Xml11Loader.Parse(source, LoadOptions.PreserveWhitespace);
        var module = new Stylesheet.Stylesheet(input, null, new FileSystemUriResolver(), processorProfile: XsltProcessorProfile.Xslt40Draft);
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(module.Root.Descendants().Last());
        Assert.Equal("p", node.Prefix);
        Assert.Equal("out", node.LocalName);
    }

    /// <summary>Owned module copies retain XML 1.1 name-decoding provenance.</summary>
    [Fact]
    public void OwnedXml11Metadata()
    {
        var localName = char.ConvertFromUtf32(0x37F) + "out";
        var source = "<?xml version='1.1'?>" + Sheet(expression: "7", declarations: "")
            .Replace("<xsl:sequence select='7'/>", "<" + localName + "/>");
        var input = Bosak.XPath.Providers.Xml.Xml11Loader.Parse(source, LoadOptions.PreserveWhitespace);
        var module = new Stylesheet.Stylesheet(input, null, new FileSystemUriResolver(), processorProfile: XsltProcessorProfile.Xslt40Draft);
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(module.Root.Descendants().Last());
        Assert.Equal(localName, node.LocalName);
    }

    private sealed class Resolver(string module) : IXsltUriResolver
    {
        internal int BindingRequests { get; private set; }
        /// <summary>Resolves the isolated namespace or stylesheet document.</summary>
        /// <param name="href">The requested location.</param>
        /// <param name="baseUri">The referring module URI.</param>
        /// <returns>The parsed requested document.</returns>
        public XDocument Resolve(string href, string? baseUri)
        {
            if (!href.EndsWith("bindings.xml", StringComparison.Ordinal)) return XDocument.Parse(module);
            BindingRequests++;
            return XDocument.Parse("<bindings xmlns:p='urn:fixed' xmlns:xs='http://www.w3.org/2001/XMLSchema'/>");
        }
    }
}
