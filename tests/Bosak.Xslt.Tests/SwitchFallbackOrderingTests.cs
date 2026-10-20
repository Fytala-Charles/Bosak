// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 October 2026
// PURPOSE              : Verifies switch fallback ordering across processor profiles.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies switch fallback ordering across processor profiles.</summary>
public class SwitchFallbackOrderingTests
{
    private static string Sheet(string children, string version) =>
        $"<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='{version}'>" +
        "<xsl:template name='start'><out><xsl:switch select='1'>" + children +
        "</xsl:switch></out></xsl:template></xsl:stylesheet>";

    /// <summary>Supplies legal and illegal layouts under both profiles and stylesheet versions.</summary>
    public static TheoryData<bool, string, string> InvalidLayouts
    {
        get
        {
            var data = new TheoryData<bool, string, string>();
            foreach (bool draft in new[] { false, true })
            foreach (string version in new[] { "3.0", "4.0" })
            foreach (string children in new[] {
                "<xsl:fallback/><xsl:when test='1'/>",
                "<xsl:when test='1'/><xsl:fallback/><xsl:when test='2'/>",
                "<xsl:when test='1'/><xsl:fallback/><xsl:otherwise/>" })
                data.Add(draft, version, children);
            return data;
        }
    }

    /// <summary>Rejects branch children after the first fallback even in unused templates.</summary>
    /// <param name="draft">Whether to use the draft processor profile.</param>
    /// <param name="version">The stylesheet version.</param>
    /// <param name="children">The invalid switch content.</param>
    [Theory]
    [MemberData(nameof(InvalidLayouts))]
    public void RejectsBranchAfterFallback(bool draft, string version, string children)
    {
        var compiler = new XsltCompiler { ProcessorProfile = draft ? XsltProcessorProfile.Xslt40Draft : XsltProcessorProfile.Legacy };
        Assert.Contains("XTSE0010", Assert.ThrowsAny<Exception>(() => compiler.Compile(Sheet(children, version))).Message);
    }

    /// <summary>Accepts and ignores multiple trailing fallbacks with or without otherwise.</summary>
    /// <param name="draft">Whether to use the draft processor profile.</param>
    /// <param name="version">The stylesheet version.</param>
    /// <param name="otherwise">Whether an otherwise branch is present.</param>
    [Theory]
    [InlineData(false, "3.0", false)]
    [InlineData(false, "3.0", true)]
    [InlineData(false, "4.0", false)]
    [InlineData(false, "4.0", true)]
    [InlineData(true, "3.0", false)]
    [InlineData(true, "3.0", true)]
    [InlineData(true, "4.0", false)]
    [InlineData(true, "4.0", true)]
    public void IgnoresTrailingFallbacks(bool draft, string version, bool otherwise)
    {
        string children = "<xsl:when test='1'>selected</xsl:when>" +
            (otherwise ? "<xsl:otherwise>other</xsl:otherwise>" : "") +
            "<xsl:fallback><xsl:sequence select='error()'/></xsl:fallback><xsl:fallback>ignored</xsl:fallback>";
        var compiler = new XsltCompiler { ProcessorProfile = draft ? XsltProcessorProfile.Xslt40Draft : XsltProcessorProfile.Legacy };
        string result = compiler.Compile(Sheet(children, version)).TransformToString(null, initialTemplate: "start");
        Assert.Contains("<out>selected</out>", result);
    }
}
