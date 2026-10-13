// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies XSLT 4.0 mixed intersect/except pattern associativity.
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

/// <summary>Verifies XSLT 4.0 mixed intersect/except pattern associativity.</summary>
public class Xslt40SetPatternAssociativityTests
{
    private static string Run(string pattern, string version = "4.0", string source = "<root/>") => XElement.Parse(
        new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
            "<xsl:template match='/' priority='2'><out><xsl:apply-templates select='/*'/></out></xsl:template>" +
            "<xsl:template match='" + pattern + "'>M</xsl:template><xsl:template match='.' priority='-1.1'>F</xsl:template></xsl:stylesheet>")
        .TransformToString(new XDocumentNode(XDocument.Parse(source)))).Value;

    /// <summary>Evaluates mixed operators from left to right while preserving parentheses and union precedence.</summary>
    /// <param name="pattern">The set pattern.</param>
    /// <param name="expected">The expected result marker.</param>
    [Theory]
    [InlineData("root except missing intersect missing", "F")]
    [InlineData("root except (missing intersect missing)", "M")]
    [InlineData("(root except missing) intersect missing", "F")]
    [InlineData("root except root intersect missing", "F")]
    [InlineData("root intersect root except missing intersect missing", "F")]
    [InlineData("root except missing intersect root", "M")]
    [InlineData("root intersect * except missing", "M")]
    [InlineData("root except missing except other intersect root", "M")]
    [InlineData("root except missing intersect missing | root", "M")]
    [InlineData("(root|missing) except missing intersect missing", "F")]
    [InlineData("root[contains(&quot;except intersect )]&quot;, &quot;except&quot;)] except missing intersect missing", "F")]
    [InlineData("root except missing intersect missing except other", "F")]
    [InlineData("root except (root except root) intersect root", "M")]
    [InlineData("child::root except child::missing intersect child::missing", "F")]
    [InlineData("/* except /*[false()] intersect /*[false()]", "F")]
    public void LeftAssociativeMembership(string pattern, string expected) => Assert.Equal(expected, Run(pattern));

    /// <summary>Ignores operators and delimiters in EQName namespace URIs.</summary>
    [Fact]
    public void UriOperands() => Assert.Equal("F", Run("Q{urn:except|intersect)}root except missing intersect missing", source: "<root xmlns='urn:except|intersect)'/>"));

    /// <summary>Retains earlier-profile dispatch while isolating the 4.0 correction.</summary>
    [Fact]
    public void EarlierProfile() => Assert.Equal("M", Run("root except missing intersect missing", "3.0"));

    /// <summary>Rejects missing operands in a mixed operator chain.</summary>
    [Fact]
    public void MissingOperand() => Assert.Contains("XTSE0340", Assert.ThrowsAny<Exception>(() => Run("root except intersect missing")).Message);
}
