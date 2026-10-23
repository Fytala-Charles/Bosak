// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 October 2026
// PURPOSE              : Verifies default and explicit apply-templates modes inside stylesheet functions.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies default and explicit apply-templates modes inside stylesheet functions.</summary>
public class FunctionApplyTemplatesModeTests
{
    /// <summary>Resolves an omitted mode as the default while preserving explicit modes and empty selections.</summary>
    /// <param name="version">The effective stylesheet version.</param>
    /// <param name="profile">The host processor profile.</param>
    /// <param name="namedDefault">Whether the stylesheet declares a named default mode.</param>
    /// <param name="empty">Whether the function selects an empty sequence.</param>
    /// <param name="mode">The explicit mode, or null when absent.</param>
    /// <param name="expected">The required text result.</param>
    [Theory]
    [InlineData("3.0", XsltProcessorProfile.Legacy, false, false, null, "unnamed")]
    [InlineData("3.0", XsltProcessorProfile.Legacy, false, true, null, "")]
    [InlineData("3.0", XsltProcessorProfile.Legacy, true, false, null, "m")]
    [InlineData("3.0", XsltProcessorProfile.Legacy, true, true, null, "")]
    [InlineData("3.0", XsltProcessorProfile.Legacy, true, false, "#default", "m")]
    [InlineData("3.0", XsltProcessorProfile.Legacy, true, false, "#unnamed", "unnamed")]
    [InlineData("3.0", XsltProcessorProfile.Legacy, true, false, "#current", "unnamed")]
    [InlineData("3.0", XsltProcessorProfile.Legacy, true, false, "n", "n")]
    [InlineData("3.0", XsltProcessorProfile.Xslt40Draft, false, false, null, "unnamed")]
    [InlineData("3.0", XsltProcessorProfile.Xslt40Draft, false, true, null, "")]
    [InlineData("3.0", XsltProcessorProfile.Xslt40Draft, true, false, null, "m")]
    [InlineData("3.0", XsltProcessorProfile.Xslt40Draft, true, true, null, "")]
    [InlineData("3.0", XsltProcessorProfile.Xslt40Draft, true, false, "#default", "m")]
    [InlineData("3.0", XsltProcessorProfile.Xslt40Draft, true, false, "#unnamed", "unnamed")]
    [InlineData("3.0", XsltProcessorProfile.Xslt40Draft, true, false, "#current", "unnamed")]
    [InlineData("3.0", XsltProcessorProfile.Xslt40Draft, true, false, "n", "n")]
    [InlineData("4.0", XsltProcessorProfile.Legacy, false, false, null, "unnamed")]
    [InlineData("4.0", XsltProcessorProfile.Legacy, false, true, null, "")]
    [InlineData("4.0", XsltProcessorProfile.Legacy, true, false, null, "m")]
    [InlineData("4.0", XsltProcessorProfile.Legacy, true, true, null, "")]
    [InlineData("4.0", XsltProcessorProfile.Legacy, true, false, "#default", "m")]
    [InlineData("4.0", XsltProcessorProfile.Legacy, true, false, "#unnamed", "unnamed")]
    [InlineData("4.0", XsltProcessorProfile.Legacy, true, false, "#current", "unnamed")]
    [InlineData("4.0", XsltProcessorProfile.Legacy, true, false, "n", "n")]
    [InlineData("4.0", XsltProcessorProfile.Xslt40Draft, false, false, null, "unnamed")]
    [InlineData("4.0", XsltProcessorProfile.Xslt40Draft, false, true, null, "")]
    [InlineData("4.0", XsltProcessorProfile.Xslt40Draft, true, false, null, "m")]
    [InlineData("4.0", XsltProcessorProfile.Xslt40Draft, true, true, null, "")]
    [InlineData("4.0", XsltProcessorProfile.Xslt40Draft, true, false, "#default", "m")]
    [InlineData("4.0", XsltProcessorProfile.Xslt40Draft, true, false, "#unnamed", "unnamed")]
    [InlineData("4.0", XsltProcessorProfile.Xslt40Draft, true, false, "#current", "unnamed")]
    [InlineData("4.0", XsltProcessorProfile.Xslt40Draft, true, false, "n", "n")]
    public void FunctionModes(string version, XsltProcessorProfile profile, bool namedDefault, bool empty,
        string? mode, string expected)
    {
        var compiler = new XsltCompiler { ProcessorProfile = profile };
        var executable = compiler.Compile(Sheet(version, namedDefault, empty, mode));
        Assert.Equal(expected, executable.TransformToString(new XDocumentNode(XDocument.Parse("<root/>"))));
    }

    /// <summary>Retains the static error for an explicitly empty mode attribute.</summary>
    /// <param name="version">The effective stylesheet version.</param>
    /// <param name="profile">The host processor profile.</param>
    [Theory]
    [InlineData("3.0", XsltProcessorProfile.Legacy)]
    [InlineData("3.0", XsltProcessorProfile.Xslt40Draft)]
    [InlineData("4.0", XsltProcessorProfile.Legacy)]
    [InlineData("4.0", XsltProcessorProfile.Xslt40Draft)]
    public void ExplicitEmptyMode_ThrowsXtse0020(string version, XsltProcessorProfile profile)
    {
        var compiler = new XsltCompiler { ProcessorProfile = profile };
        var error = Assert.ThrowsAny<Exception>(() => compiler.Compile(Sheet(version, false, false, "")));
        Assert.Contains("XTSE0020", error.ToString());
    }

    private static string Sheet(string version, bool namedDefault, bool empty, string? mode)
    {
        var defaultAttribute = namedDefault ? " default-mode='m'" : "";
        var modeAttribute = mode is null ? "" : " mode='" + mode + "'";
        var selection = empty ? "()" : "$nodes";
        return "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:mode-probe' version='" + version + "'" + defaultAttribute + ">" +
            "<xsl:output method='text'/><xsl:mode name='m'/><xsl:mode name='n'/>" +
            "<xsl:function name='f:emit'><xsl:param name='nodes'/><xsl:apply-templates select='" + selection + "'" + modeAttribute + "/></xsl:function>" +
            "<xsl:template match='/' mode='#all'><xsl:value-of select='f:emit(/*)'/></xsl:template>" +
            "<xsl:template match='root' mode='#unnamed'>unnamed</xsl:template>" +
            "<xsl:template match='root' mode='m'>m</xsl:template><xsl:template match='root' mode='n'>n</xsl:template></xsl:stylesheet>";
    }
}
