// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies static errors for unavailable prefixes in XSLT 4.0 patterns.
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

using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies static errors for unavailable prefixes in XSLT 4.0 patterns.</summary>
public class Xslt40PatternPrefixDiagnosticTests
{
    private static void Compile(string pattern) => new Api.XsltCompiler().Compile(
        "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:n='urn:native' fixed-namespaces='#standard'>" +
        "<xsl:template match='" + pattern + "'/></xsl:stylesheet>");

    /// <summary>Rejects omitted native prefixes when compiling the stylesheet.</summary>
    /// <param name="pattern">The pattern containing an unavailable prefix.</param>
    [Theory]
    [InlineData("n:root")]
    [InlineData("n:*")]
    [InlineData("@n:code")]
    [InlineData("@n:*")]
    [InlineData("self::n:root")]
    [InlineData("element(n:root)")]
    [InlineData("attribute(n:code)")]
    [InlineData("document-node(element(n:root))")]
    [InlineData("(n:root)")]
    [InlineData("root | n:root")]
    [InlineData("root intersect n:root")]
    [InlineData("root except n:root")]
    [InlineData("root/n:child")]
    [InlineData("root[@n:code]")]
    [InlineData("root[n:child]")]
    [InlineData("root[n:exists(.)]")]
    [InlineData("root[$n:value]")]
    [InlineData("~n:root")]
    [InlineData("(~n:root)")]
    [InlineData("root[. instance of n:root]")]
    [InlineData("n.part:root")]
    [InlineData("élément:root")]
    public void OmittedPrefix(string pattern)
    {
        var error = Assert.ThrowsAny<Exception>(() => Compile(pattern));
        Assert.Contains("XPST0081", error.ToString());
    }

    /// <summary>Ignores prefix-like text in literals, comments and EQName URIs.</summary>
    /// <param name="pattern">The pattern with protected lexical content.</param>
    [Theory]
    [InlineData("root[&quot;n:value&quot; = &quot;n:value&quot;]")]
    [InlineData("root (: n:value :)")]
    [InlineData("Q{urn:n:value}root")]
    [InlineData("*:root")]
    [InlineData("root[&quot;n:value&quot;&quot;n:other&quot; = &quot;n:value&quot;&quot;n:other&quot;]")]
    [InlineData("root (: outer (: n:inner :) n:outer :)")]
    public void ProtectedText(string pattern) => Compile(pattern);

    /// <summary>Retains native prefix declarations in earlier profiles and unfixed 4.0.</summary>
    /// <param name="version">The effective stylesheet version.</param>
    [Theory]
    [InlineData("2.0")]
    [InlineData("3.0")]
    [InlineData("4.0")]
    public void NativeBindings(string version) => new Api.XsltCompiler().Compile(
        "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:n='urn:native'><xsl:template match='n:root'/></xsl:stylesheet>");

    /// <summary>Rejects undeclared prefixes in unfixed 4.0 patterns too.</summary>
    [Fact]
    public void UndeclaredNativePrefix()
    {
        var error = Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(
            "<xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='missing:root'/></xsl:stylesheet>"));
        Assert.Contains("XPST0081", error.ToString());
    }
    /// <summary>Distinguishes a spaced map-entry delimiter from a lexical prefixed wildcard.</summary>
    [Fact]
    public void MapEntryDelimiter() => Patterns.PatternCompiler.ValidateNamespacePrefixes(
        "root[map{key : *}]", new Dictionary<string, string>());
}
