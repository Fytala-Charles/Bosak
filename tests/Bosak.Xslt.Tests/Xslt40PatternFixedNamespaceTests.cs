// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies fixed namespace bindings in XSLT 4.0 template patterns.
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

/// <summary>Verifies fixed namespace bindings in XSLT 4.0 template patterns.</summary>
public class Xslt40PatternFixedNamespaceTests
{
    private static string Run(string pattern, string bindings = "#standard n=urn:nodes t=urn:types", string native = "xmlns:n='urn:wrong' xmlns:t='urn:wrong'", string select = "/*", string version = "4.0") => XElement.Parse(
        new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' fixed-namespaces='" + bindings + "'>" +
            "<xsl:item-type name='Q{urn:types}root' as='element(Q{urn:nodes}root)'/>" +
            "<xsl:template match='/' priority='2'><out><xsl:apply-templates select='" + select + "'/></out></xsl:template>" +
            "<xsl:template " + native + " match='" + pattern + "'>M</xsl:template><xsl:template match='.' priority='-2'>F</xsl:template></xsl:stylesheet>")
        .TransformToString(new XDocumentNode(XDocument.Parse("<n:root xmlns:n='urn:nodes' n:code='7'><n:child/></n:root>")))).Value;

    /// <summary>Uses fixed bindings throughout supported node and predicate patterns.</summary>
    /// <param name="pattern">The pattern to compile.</param>
    [Theory]
    [InlineData("n:root")]
    [InlineData("element(n:root)")]
    [InlineData("n:*")]
    [InlineData("self::n:root")]
    [InlineData("(n:root)")]
    [InlineData("n:root | n:missing")]
    [InlineData("n:root intersect element(n:root)")]
    [InlineData("n:root except n:missing")]
    [InlineData("n:root[@n:code = 7]")]
    [InlineData("n:root[n:child]")]
    [InlineData("n:root[. instance of t:root]")]
    [InlineData("(~t:root)")]
    [InlineData("~t:root")]
    [InlineData("n:root[fn:exists(n:child)]")]
    [InlineData("Q{urn:nodes}root[@n:code = 7]")]
    public void FixedBindings(string pattern) => Assert.Equal("M", Run(pattern));

    /// <summary>Allows fixed prefixes with no native declarations.</summary>
    [Fact]
    public void NoNativeDeclaration() => Assert.Equal("M", Run("n:root", native: ""));

    /// <summary>Resolves attribute patterns using fixed bindings.</summary>
    [Fact]
    public void AttributeName() => Assert.Equal("M", Run("@n:code", select: "/*/@*"));

    /// <summary>Preserves quoted text containing a rebound prefix.</summary>
    [Fact]
    public void QuotedPrefix() => Assert.Equal("M", Run("n:root[&quot;n:root&quot; = &quot;n:root&quot;]"));

    /// <summary>Keeps native namespace resolution when fixed bindings are absent.</summary>
    /// <param name="version">The effective stylesheet version.</param>
    [Theory]
    [InlineData("4.0")]
    [InlineData("3.0")]
    [InlineData("2.0")]
    public void NativeBindings(string version)
    {
        var source = "<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><out><xsl:apply-templates/></out></xsl:template><xsl:template xmlns:n='urn:nodes' match='n:root'>M</xsl:template></xsl:stylesheet>";
        Assert.Equal("M", XElement.Parse(new Api.XsltCompiler().Compile(source).TransformToString(new XDocumentNode(XDocument.Parse("<root xmlns='urn:nodes'/>")))).Value);
    }
}
