// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies candidate-node focus for forward-axis operands in XSLT 4.0 set patterns.
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

/// <summary>Verifies candidate-node focus for forward-axis operands in XSLT 4.0 set patterns.</summary>
public class Xslt40SetPatternAxisFocusTests
{
    private static string Run(string pattern, string select = "/*", string source = "<root code='7'><child id='1'/><child id='2'/><child id='3'/></root>", string version = "4.0", string priority = "-1.1") => XElement.Parse(
        new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
            "<xsl:template match='/' priority='2'><out><xsl:apply-templates select='" + select + "'/></out></xsl:template>" +
            "<xsl:template match='" + pattern + "'>M<xsl:value-of select='@id'/></xsl:template><xsl:template match='.' priority='" + priority + "'>F</xsl:template></xsl:stylesheet>")
        .TransformToString(new XDocumentNode(XDocument.Parse(source)))).Value;

    /// <summary>Matches explicit forward-axis operands against the candidate node.</summary>
    /// <param name="pattern">The set pattern.</param>
    /// <param name="expected">The expected output.</param>
    [Theory]
    [InlineData("self::root intersect self::root", "M")]
    [InlineData("self::root except self::missing", "M")]
    [InlineData("self::root except self::root", "F")]
    [InlineData("child::root intersect self::root", "M")]
    [InlineData("descendant::root intersect self::root", "M")]
    [InlineData("descendant-or-self::root except self::missing", "M")]
    [InlineData("self::root[child/..] intersect self::root", "M")]
    [InlineData("self::root[contains(&quot;::/&quot;, &quot;::&quot;)] except self::missing", "M")]
    [InlineData("self::root except self::missing intersect self::missing", "F")]
    [InlineData("(self::root except self::missing) intersect self::root", "M")]
    [InlineData("self::root intersect self::missing | self::root", "M")]
    public void AxisMembership(string pattern, string expected) => Assert.Equal(expected, Run(pattern));

    /// <summary>Matches attribute operands without evaluating their axes from the document root.</summary>
    /// <param name="pattern">The attribute pattern.</param>
    /// <param name="expected">The expected result.</param>
    [Theory]
    [InlineData("attribute::code intersect attribute::code", "M")]
    [InlineData("attribute::code except attribute::other", "M")]
    [InlineData("attribute::code except attribute::code", "F")]
    public void Attributes(string pattern, string expected) => Assert.Equal(expected, Run(pattern, "/*/@code"));

    /// <summary>Combines a real path operand with an axis operand using their respective pattern matchers.</summary>
    /// <param name="pattern">The mixed pattern.</param>
    /// <param name="expected">The expected output.</param>
    [Theory]
    [InlineData("root/child intersect self::child", "M1M2M3")]
    [InlineData("self::child intersect root/child", "M1M2M3")]
    [InlineData("root/child except self::child", "FFF")]
    [InlineData("self::child except root/child", "FFF")]
    public void MixedPaths(string pattern, string expected) => Assert.Equal(expected, Run(pattern, "/*/*"));

    /// <summary>Preserves positional selection when all operands contain actual paths.</summary>
    [Fact]
    public void AllPathPositions() => Assert.Equal("FM2F", Run("root/child[2] intersect root/child", "/*/*"));

    /// <summary>Preserves the first operand's default priority.</summary>
    [Fact]
    public void NamedAxisPriority() => Assert.Equal("F", Run("self::root except self::missing", priority: "0.1"));

    /// <summary>Ignores punctuation in EQName URIs while matching axis operands.</summary>
    [Fact]
    public void UriAxis() => Assert.Equal("M", Run("self::Q{urn::n}root intersect child::Q{urn::n}root", source: "<root xmlns='urn::n'/>"));

    /// <summary>Retains earlier-profile axis-set dispatch.</summary>
    [Fact]
    public void EarlierProfile() => Assert.Equal("F", Run("self::root intersect self::root", version: "3.0"));
}
