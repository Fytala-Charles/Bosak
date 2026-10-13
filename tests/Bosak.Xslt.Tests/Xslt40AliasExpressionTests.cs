// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies structural aliases in XPath instance-of and treat expressions.
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

/// <summary>Verifies structural aliases in XPath instance-of and treat expressions.</summary>
[Collection("XSLT package registry")]
public class Xslt40AliasExpressionTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:f='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string body) => new Api.XsltCompiler().Compile(Wrap(body))
        .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
    private const string Aliases = "<xsl:item-type name='t:vector' as='array(t:number)'/><xsl:item-type name='t:number' as='xs:double'/>";

    /// <summary>Matches expanded array and atomic types without applying numeric coercion.</summary>
    [Fact]
    public void InstanceOf_ArrayAndAtomicAliases()
    {
        var output = Run(Aliases + """
            <xsl:template match='/'><out><xsl:value-of separator='|' select='(
              [xs:double(1)] instance of t:vector,[1] instance of t:vector,
              xs:double(1) instance of t:number,1 instance of t:number)'/></out></xsl:template>
            """);
        Assert.Equal("true|false|true|false", XElement.Parse(output).Value);
    }

    /// <summary>Preserves optional and repeated item cardinalities in alias tests.</summary>
    [Fact]
    public void InstanceOf_Occurrences()
    {
        var output = Run(Aliases + """
            <xsl:template match='/'><out><xsl:value-of separator='|' select='(
              () instance of t:vector?,() instance of t:vector,
              ([xs:double(1)],[xs:double(2)]) instance of t:vector+,
              ([xs:double(1)],[xs:double(2)]) instance of t:vector)'/></out></xsl:template>
            """);
        Assert.Equal("true|false|true|false", XElement.Parse(output).Value);
    }

    /// <summary>Checks valid treat expressions without changing their values.</summary>
    [Fact]
    public void Treat_ArrayAlias()
        => Assert.Equal("2", XElement.Parse(Run(Aliases + """
            <xsl:template match='/'><out><xsl:value-of select='([xs:double(2)] treat as t:vector)(1)'/></out></xsl:template>
            """)).Value);

    /// <summary>Reports treat errors for incompatible values instead of coercing them.</summary>
    /// <param name="expression">The invalid treat expression.</param>
    [Theory]
    [InlineData("[1] treat as t:vector")]
    [InlineData("() treat as t:vector")]
    [InlineData("1 treat as t:number")]
    public void Treat_IncompatibleValues(string expression)
        => Assert.Contains("XPDY0050", Assert.ThrowsAny<Exception>(() => Run(Aliases + $"<xsl:template match='/'><xsl:sequence select=\"{expression}\"/></xsl:template>")).ToString());

    /// <summary>Resolves rebound prefixes, default type namespaces and expanded names.</summary>
    [Fact]
    public void InstanceOf_NamespaceForms()
    {
        var output = Run(Aliases + """
            <xsl:template match='/' xmlns:v='urn:types' xmlns:t='urn:other' xpath-default-namespace='urn:types'>
              <out><xsl:value-of separator='|' select='([xs:double(1)] instance of v:vector,
                [xs:double(1)] instance of vector,[xs:double(1)] instance of Q{urn:types}vector)'/></out>
            </xsl:template>
            """);
        Assert.Equal("true|true|true", XElement.Parse(output).Value);
    }

    /// <summary>Retains host aliases when an XPath namespace prolog creates a merged static context.</summary>
    [Fact]
    public void InstanceOf_InlineNamespaceDeclaration()
        => Assert.Equal("true", XElement.Parse(Run(Aliases + """
            <xsl:template match='/'><out><xsl:value-of select="declare namespace v='urn:types'; [xs:double(1)] instance of v:vector"/></out></xsl:template>
            """)).Value);

    /// <summary>Supplies the structural alias context to attribute value templates.</summary>
    [Fact]
    public void InstanceOf_AttributeValueTemplate()
        => Assert.Equal("true", XElement.Parse(Run(Aliases + """
            <xsl:template match='/'><out valid='{[xs:double(1)] instance of t:vector}'/></xsl:template>
            """)).Attribute("valid")!.Value);

    /// <summary>Matches nested array member types and optional member sequences.</summary>
    [Fact]
    public void InstanceOf_NestedArrays()
        => Assert.Equal("true|false", XElement.Parse(Run("""
            <xsl:item-type name='t:matrix' as='array(array(xs:double?))'/>
            <xsl:template match='/'><out><xsl:value-of separator='|' select='(
              [[xs:double(1),()]] instance of t:matrix,[[1]] instance of t:matrix)'/></out></xsl:template>
            """)).Value);

    /// <summary>Keeps the public enumeration-only option restricted to enum definitions.</summary>
    [Fact]
    public void PublicEnumerationOption_StillRejectsStructuralDefinitions()
        => Assert.Throws<ArgumentException>(() => Bosak.XPath.Api.XPath31Expression.Compile("[1] instance of Q{urn:types}vector",
            new Bosak.XPath.Api.CompileOptions
            {
                Compatibility = Bosak.XPath.Api.XPathCompatibility.XPath40,
                EnumerationItemTypes = new Dictionary<string,string> { ["Q{urn:types}vector"] = "array(xs:integer)" }
            }));

    /// <summary>Matches arrays of enumeration members without rewriting literal strings.</summary>
    [Fact]
    public void InstanceOf_EnumerationMembers()
        => Assert.Equal("true|false", XElement.Parse(Run("""
            <xsl:item-type name='t:states' as='array(t:state)'/>
            <xsl:item-type name='t:state' as="enum('t:state','ready')"/>
            <xsl:template match='/'><out><xsl:value-of separator='|' select="(['t:state'] instance of t:states,['lost'] instance of t:states)"/></out></xsl:template>
            """)).Value);

    /// <summary>Preserves unknown-type refusal in XPath 3.1 instructions within a 4.0 stylesheet.</summary>
    [Fact]
    public void InstanceOf_MixedVersionDoesNotExposeAliases()
        => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => Run(Aliases + """
            <xsl:template match='/' version='3.0'><out><xsl:value-of select='[xs:double(1)] instance of t:vector'/></out></xsl:template>
            """)).ToString());

    private sealed class PackageResolver : Api.IXsltUriResolver
    {
        /// <summary>Resolves the package containing a private array alias.</summary>
        /// <param name="href">The requested location.</param>
        /// <param name="baseUri">The referring module base URI.</param>
        /// <returns>The package document.</returns>
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse("""
            <xsl:package name='urn:array-type-package' package-version='1.0' version='4.0'
              xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'
              xmlns:t='urn:types' xmlns:f='urn:functions'>
              <xsl:item-type name='t:vector' as='array(xs:double)'/>
              <xsl:function name='f:valid' visibility='public' as='xs:boolean'>
                <xsl:param name='v' as='t:vector'/><xsl:sequence select='$v instance of t:vector'/>
              </xsl:function>
            </xsl:package>
            """);
    }

    /// <summary>Uses package-private aliases for type tests and restores the caller's same-named alias.</summary>
    [Fact]
    public void InstanceOf_PackageIsolation()
    {
        Api.XsltFunctionLibrary.ClearPackages();
        Api.XsltFunctionLibrary.RegisterPackage("urn:array-type-package", "1.0", "urn:array-type-package:1.0");
        try
        {
            var output = new Api.XsltCompiler { UriResolver = new PackageResolver() }.Compile(Wrap("""
                <xsl:use-package name='urn:array-type-package' package-version='1.0'/>
                <xsl:item-type name='t:vector' as='array(xs:string)'/>
                <xsl:template match='/'><out><xsl:value-of separator='|' select="(
                  f:valid([1]),['caller'] instance of t:vector,[xs:double(1)] instance of t:vector)"/></out></xsl:template>
                """)).TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
            Assert.Equal("true|true|false", XElement.Parse(output).Value);
        }
        finally { Api.XsltFunctionLibrary.ClearPackages(); }
    }
}
