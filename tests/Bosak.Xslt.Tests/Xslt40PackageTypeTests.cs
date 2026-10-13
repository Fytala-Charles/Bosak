// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies named type visibility across XSLT packages.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-10-2026     | Creation                                                                                 |
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies named type visibility across XSLT packages.</summary>
[Collection("XSLT package registry")]
public class Xslt40PackageTypeTests
{
    private sealed class Resolver(string declarations) : Api.IXsltUriResolver
    {
        /// <summary>Resolves the defining type package.</summary>
        /// <param name="href">The package location.</param>
        /// <param name="baseUri">The referring URI.</param>
        /// <returns>The package document.</returns>
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(Wrap(declarations, true));
    }

    private static string Wrap(string body, bool package = false) =>
        "<xsl:" + (package ? "package name='urn:type-package' package-version='1.0'" : "stylesheet") +
        " version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:t='urn:types' " +
        "xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>" + body +
        "</xsl:" + (package ? "package" : "stylesheet") + ">";

    private static Api.XsltExecutable Compile(string declarations, string accept, string body)
    {
        Api.XsltFunctionLibrary.ClearPackages();
        Api.XsltFunctionLibrary.RegisterPackage("urn:type-package", "1.0", "urn:type-package:1.0");
        try
        {
            return new Api.XsltCompiler { UriResolver = new Resolver(declarations) }.Compile(Wrap(
                "<xsl:use-package name='urn:type-package' package-version='1.0'>" + accept + "</xsl:use-package>" + body));
        }
        finally { Api.XsltFunctionLibrary.ClearPackages(); }
    }

    /// <summary>Accepts public and exposed aliases without exposing their private dependencies.</summary>
    [Theory]
    [InlineData(" visibility='public'", "")]
    [InlineData("", "<xsl:expose component='item-type' names='t:vector' visibility='public'/>")]
    [InlineData("", "<xsl:expose component='*' names='*:vector' visibility='public'/>")]
    public void PublicAliasPreservesPrivateDependency(string visibility, string expose)
    {
        var executable = Compile("<xsl:item-type name='t:number' as='xs:double'/><xsl:item-type name='t:vector' as='array(t:number)'" + visibility + "/>" + expose,
            "<xsl:accept component='item-type' names='t:vector' visibility='private'/>",
            "<xsl:item-type name='t:number' as='xs:string'/><xsl:template match='/'><out><xsl:value-of separator='|' select='([xs:double(1)] instance of t:vector,[1] instance of t:vector)'/></out></xsl:template>");
        Assert.Equal("true|false", XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value);
    }

    /// <summary>Resolves accept prefixes declared directly on the accept element.</summary>
    [Fact]
    public void AcceptUsesOwnNamespaceContext()
    {
        var executable = Compile("<xsl:item-type name='t:value' as='xs:string' visibility='public'/>",
            "<xsl:accept xmlns:a='urn:types' component='item-type' names='a:value' visibility='private'/>",
            "<xsl:template match='/'><xsl:value-of select=\"'ok' instance of t:value\"/></xsl:template>");
        Assert.Contains("true", executable.TransformToString(new XDocumentNode(XDocument.Parse("<root/>"))));
    }

    /// <summary>Imports the defining record metadata and its constructor.</summary>
    [Fact]
    public void PublicRecordConstructor()
    {
        var executable = Compile("<xsl:record-type name='t:point' constructor='yes'><xsl:field name='x' as='xs:integer'/></xsl:record-type><xsl:expose component='record-type' names='t:point' visibility='public'/>",
            "<xsl:accept component='record-type' names='t:point' visibility='private'/>",
            "<xsl:template match='/'><out><xsl:value-of select='t:point(7)?x'/></out></xsl:template>");
        Assert.Equal("7", XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value);
    }

    /// <summary>Rejects missing declarations and incompatible visibility.</summary>
    [Theory]
    [InlineData("item-type", "t:missing", "private", "XTSE3030")]
    [InlineData("record-type", "t:value", "private", "XTSE3030")]
    [InlineData("item-type", "t:value", "abstract", "XTSE3040")]
    public void InvalidAccept(string kind, string name, string visibility, string error)
    {
        var ex = Assert.ThrowsAny<Exception>(() => Compile("<xsl:item-type name='t:value' as='xs:string' visibility='public'/>",
            $"<xsl:accept component='{kind}' names='{name}' visibility='{visibility}'/>", ""));
        Assert.Contains(error, ex.ToString());
    }

    /// <summary>Prevents private and hidden type names from entering the caller's static context.</summary>
    [Theory]
    [InlineData("", "")]
    [InlineData(" visibility='public'", "<xsl:accept component='item-type' names='t:value' visibility='hidden'/>")]
    public void UnavailableAlias(string visibility, string accept)
    {
        var ex = Assert.ThrowsAny<Exception>(() => Compile("<xsl:item-type name='t:value' as='xs:string'" + visibility + "/>", accept,
            "<xsl:variable name='v' as='t:value' select=\"'ok'\"/>"));
        Assert.Contains("XPST0051", ex.ToString());
    }

    /// <summary>Validates exposure names and declaration visibility.</summary>
    [Theory]
    [InlineData("<xsl:expose component='item-type' names='t:missing' visibility='public'/>", "XTSE3020")]
    [InlineData("<xsl:expose component='record-type' names='t:value' visibility='public'/>", "XTSE3020")]
    [InlineData("<xsl:item-type name='t:other' as='xs:string' visibility='final'/>", "XTSE0020")]
    [InlineData("<xsl:record-type name='t:other' visibility='abstract'/>", "XTSE0020")]
    public void InvalidExposure(string declaration, string error)
    {
        var ex = Assert.ThrowsAny<Exception>(() => Compile("<xsl:item-type name='t:value' as='xs:string'/>" + declaration, "", ""));
        Assert.Contains(error, ex.ToString());
    }

    private sealed class TransitiveResolver(string visibility) : Api.IXsltUriResolver
    {
        /// <summary>Resolves the two packages in the chain.</summary>
        /// <param name="href">The package location.</param>
        /// <param name="baseUri">The referring URI.</param>
        /// <returns>The requested package document.</returns>
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(href.Contains("bridge")
            ? Wrap("<xsl:use-package name='urn:type-package' package-version='1.0'><xsl:accept component='item-type' names='t:value' visibility='" + visibility + "'/></xsl:use-package>", true).Replace("name='urn:type-package' package-version='1.0' version=", "name='urn:bridge' package-version='1.0' version=")
            : Wrap("<xsl:item-type name='t:value' as='xs:string' visibility='public'/>", true));
    }

    /// <summary>Preserves acceptance visibility through a chain of packages.</summary>
    [Theory]
    [InlineData("public", true)]
    [InlineData("private", false)]
    public void TransitiveVisibility(string visibility, bool available)
    {
        Api.XsltFunctionLibrary.ClearPackages();
        Api.XsltFunctionLibrary.RegisterPackage("urn:type-package", "1.0", "urn:type-package:1.0");
        Api.XsltFunctionLibrary.RegisterPackage("urn:bridge", "1.0", "urn:bridge:1.0");
        try
        {
            var compiler = new Api.XsltCompiler { UriResolver = new TransitiveResolver(visibility) };
            var source = Wrap("<xsl:use-package name='urn:bridge' package-version='1.0'/><xsl:variable name='v' as='t:value' select=\"'ok'\"/><xsl:template match='/'><xsl:value-of select='$v'/></xsl:template>");
            if (available)
                Assert.Contains("ok", compiler.Compile(source).TransformToString(new XDocumentNode(XDocument.Parse("<root/>"))));
            else
                Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => compiler.Compile(source)).ToString());
        }
        finally { Api.XsltFunctionLibrary.ClearPackages(); }
    }

    /// <summary>Rejects local collisions with an accepted type.</summary>
    [Fact]
    public void AcceptedTypeCannotBeOverridden()
    {
        var ex = Assert.ThrowsAny<Exception>(() => Compile("<xsl:item-type name='t:value' as='xs:string' visibility='public'/>", "",
            "<xsl:record-type name='t:value'/>") );
        Assert.Contains("XTSE3050", ex.ToString());
    }
}
