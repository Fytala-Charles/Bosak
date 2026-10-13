// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies fixed namespace bindings in named types and XPath expressions.
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

/// <summary>Verifies fixed namespace bindings in named types and XPath expressions.</summary>
public class Xslt40FixedTypeNamespaceTests
{
    private static string Wrap(string bindings, string body, string native = "") =>
        "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' exclude-result-prefixes='#all' fixed-namespaces='" + bindings + "' " + native + ">" + body + "</xsl:stylesheet>";
    private static string Run(string source) => XElement.Parse(new Api.XsltCompiler().Compile(source)
        .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;

    /// <summary>Resolves standard prefixes without native declarations.</summary>
    [Theory]
    [InlineData("#standard")]
    [InlineData("xs fn map array math")]
    public void StandardPrefixes(string bindings)
    {
        Assert.Equal("1951-10-11|2", Run(Wrap(bindings,
            "<xsl:item-type name='birthday' as='xs:date'/><xsl:template match='/'><out><xsl:variable name='d' as='birthday' select=\"xs:date('1951-10-11')\"/><xsl:value-of separator='|' select='($d,array:size([1,2]))'/></out></xsl:template>")));
    }

    /// <summary>Ignores descendant prefix rebinding for type and expression names.</summary>
    [Fact]
    public void ExplicitBindingsRemainFixed()
    {
        Assert.Equal("true|false", Run(Wrap("#standard t=urn:types n=urn:nodes",
            "<xsl:item-type xmlns:t='urn:wrong' xmlns:n='urn:wrong' name='t:root' as='element(n:root)'/><xsl:template match='/' xmlns:t='urn:other'><out><xsl:variable name='v'><n:root xmlns:n='urn:nodes'/></xsl:variable><xsl:value-of separator='|' select='($v/* instance of t:root,$v/* instance of element(Q{urn:wrong}root))'/></out></xsl:template>")));
    }

    /// <summary>Uses root native bindings and the final explicit token for repeated prefixes.</summary>
    [Theory]
    [InlineData("#standard t", "urn:types")]
    [InlineData("#standard t=urn:wrong t=urn:types", "urn:types")]
    public void RootBindingAndLastToken(string bindings, string expectedUri)
    {
        Assert.Equal("true", Run(Wrap(bindings,
            "<xsl:item-type name='t:value' as='xs:string'/><xsl:template match='/'><out><xsl:value-of select=\"'ok' instance of Q{" + expectedUri + "}value\"/></out></xsl:template>", "xmlns:t='urn:types'")));
    }

    /// <summary>Leaves unprefixed element names under xpath-default-namespace control.</summary>
    [Fact]
    public void UnprefixedNamesKeepTheirDefault()
    {
        Assert.Equal("true", Run(Wrap("#standard", "<xsl:item-type xpath-default-namespace='urn:nodes' name='rootType' as='element(root)'/><xsl:template match='/'><out><xsl:variable name='v'><root xmlns='urn:nodes'/></xsl:variable><xsl:value-of select='$v/* instance of Q{urn:nodes}rootType'/></out></xsl:template>")));
    }

    /// <summary>Does not admit native prefixes absent from the fixed set.</summary>
    [Fact]
    public void OmittedNativePrefixIsUnavailable()
    {
        var ex = Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(Wrap("#standard",
            "<xsl:item-type name='t:value' as='xs:string'/>", "xmlns:t='urn:types'")));
        Assert.Contains("XTSE0280", ex.ToString());
    }

    /// <summary>Rejects invalid or unavailable fixed namespace declarations.</summary>
    [Theory]
    [InlineData("xmlns=urn:bad")]
    [InlineData("xml=urn:bad")]
    [InlineData("p=http://www.w3.org/XML/1998/namespace")]
    [InlineData("p=")]
    [InlineData("=urn:bad")]
    [InlineData("bad:prefix=urn:bad")]
    [InlineData("./missing.xml")]
    public void InvalidBindings(string bindings)
    {
        Assert.Contains("XTSE0122", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(Wrap(bindings, ""))).ToString());
    }

    private sealed class Resolver : Api.IXsltUriResolver
    {
        /// <summary>Resolves namespace and include documents.</summary>
        /// <param name="href">The requested location.</param>
        /// <param name="baseUri">The referring base URI.</param>
        /// <returns>The requested document.</returns>
        public XDocument Resolve(string href, string? baseUri) => href.EndsWith("bindings.xml")
            ? XDocument.Parse("<bindings xmlns:t='urn:types' xmlns:xs='http://www.w3.org/2001/XMLSchema'/>")
            : XDocument.Parse(Wrap("xs t=urn:included", "<xsl:item-type name='t:value' as='xs:string'/>"));
    }

    /// <summary>Imports document bindings without making them native result namespaces.</summary>
    [Fact]
    public void NamespaceDocument()
    {
        var executable = new Api.XsltCompiler { UriResolver = new Resolver() }.Compile(Wrap("./bindings.xml",
            "<xsl:item-type name='t:value' as='xs:string'/><xsl:template match='/'><out><xsl:value-of select=\"'ok' instance of t:value\"/></out></xsl:template>"));
        var result = XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse("<root/>"))));
        Assert.Equal("true", result.Value);
        Assert.DoesNotContain(result.Attributes(), a => a.IsNamespaceDeclaration && a.Value == "urn:types");
    }

    /// <summary>Keeps included module bindings independent of the principal module.</summary>
    [Fact]
    public void IncludedModuleIsolation()
    {
        var executable = new Api.XsltCompiler { UriResolver = new Resolver() }.Compile(Wrap("#standard t=urn:principal",
            "<xsl:include href='included.xsl'/><xsl:item-type name='t:value' as='xs:integer'/><xsl:template match='/'><out><xsl:value-of separator='|' select=\"('ok' instance of Q{urn:included}value,1 instance of t:value)\"/></out></xsl:template>"));
        Assert.Equal("true|true", XElement.Parse(executable.TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value);
    }
}
