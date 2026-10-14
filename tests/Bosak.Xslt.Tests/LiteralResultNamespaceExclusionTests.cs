// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 14 October 2026
// PURPOSE              : Verify literal-result namespace copying and scoped namespace exclusions.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 14-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================


using System.Xml.Linq;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies namespace exclusions at their declaring element and native result copying.</summary>
public class LiteralResultNamespaceExclusionTests
{
    private static XElement Run(string body, string version = "3.0", string extra = "")
        => XElement.Parse(new XsltCompiler().Compile("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:p='urn:native' version='" + version + "' exclude-result-prefixes='#all' " + extra + ">" + body + "</xsl:stylesheet>").TransformToString(null, initialTemplate: "main"));

    /// <summary>An ancestor exclusion does not suppress a descendant's different namespace URI.</summary>
    /// <param name="version">The stylesheet version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    [InlineData("4.0")]
    public void ReboundInstructionBindingIsCopiedToNestedLiteral(string version)
    {
        var result = Run("<xsl:template name='main'><out><xsl:call-template name='child'/></out></xsl:template><xsl:template name='child' xmlns:p='urn:other'><hit>fixed</hit></xsl:template>", version);
        Assert.Null(result.GetNamespaceOfPrefix("p"));
        Assert.Equal("urn:other", result.Element("hit")!.GetNamespaceOfPrefix("p")?.NamespaceName);
    }

    /// <summary>Explicit local exclusions still suppress the locally introduced namespace.</summary>
    /// <param name="exclusion">The local exclusion value.</param>
    [Theory]
    [InlineData("#all")]
    [InlineData("p")]
    public void LocalExclusionResolvesItsOwnBindings(string exclusion)
    {
        var result = Run("<xsl:template name='main' xmlns:p='urn:other' exclude-result-prefixes='" + exclusion + "'><out><hit/></out></xsl:template>");
        Assert.Null(result.GetNamespaceOfPrefix("p"));
        Assert.Null(result.Element("hit")!.GetNamespaceOfPrefix("p"));
    }

    /// <summary>Exclusions designate URIs even when another prefix is used for the same URI.</summary>
    [Fact]
    public void AliasToExcludedUriRemainsExcluded()
    {
        var result = Run("<xsl:template name='main' xmlns:q='urn:native'><out/></xsl:template>");
        Assert.Null(result.GetNamespaceOfPrefix("q"));
    }

    /// <summary>Fixed bindings do not become native namespace nodes in the result.</summary>
    [Fact]
    public void FixedBindingsAndNativeRebindingRemainIndependent()
    {
        var result = Run("<xsl:template name='main' xmlns:p='urn:other'><out><hit/></out></xsl:template>", "4.0", "fixed-namespaces='#standard p=urn:fixed q=urn:fixed'");
        Assert.Equal("urn:other", result.GetNamespaceOfPrefix("p")?.NamespaceName);
        Assert.Null(result.GetNamespaceOfPrefix("q"));
    }

    /// <summary>A namespace alias target remains available despite an ancestor #all exclusion.</summary>
    [Fact]
    public void NamespaceAliasTargetSurvivesExclusion()
    {
        var result = Run("<xsl:namespace-alias stylesheet-prefix='p' result-prefix='q'/><xsl:template name='main'><p:out/></xsl:template>", "3.0", "xmlns:q='urn:target'");
        Assert.Equal("urn:target", result.Name.NamespaceName);
        Assert.Equal("urn:target", result.GetNamespaceOfPrefix("q")?.NamespaceName);
    }
}