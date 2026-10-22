// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies XSLT 4.0 union priorities and single-rule next-match behavior.
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
//                      | Charles Korthout | 0.2   | 22-10-2026     | Verify minimum 4.0 union priorities and retain 3.0 dispatch controls.                     |
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies XSLT 4.0 union priorities and single-rule next-match behavior.</summary>
public class Xslt40UnionPatternTests
{
    private static string Run(string body, string version = "4.0", string source = "<root/>", string extra = "") => XElement.Parse(
        new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:n='urn:n' exclude-result-prefixes='#all' " + extra + ">" + body + "</xsl:stylesheet>")
            .TransformToString(new XDocumentNode(XDocument.Parse(source)))).Value;

    /// <summary>Uses the minimum alternative priority, including alternatives that do not match.</summary>
    /// <param name="pattern">The union match pattern.</param>
    [Theory]
    [InlineData("root|missing/child")]
    [InlineData("root union missing/child")]
    [InlineData("(root|missing/child)")]
    [InlineData("((root|missing/child))")]
    [InlineData("(root|*)|missing/child")]
    [InlineData("root|(missing/child|*)")]
    [InlineData("root|missing[true()]")]
    [InlineData("element(*)|element(root)")]
    [InlineData("element(*:root)|element(missing)")]
    public void MinimumAlternativePriority(string pattern)
    {
        string competitor = pattern.Contains("missing/") || pattern.Contains("[true()]") ? "0.25" : "-0.1";
        Assert.Equal("competitor", Run("<xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>" +
            "<xsl:template match='" + pattern + "'>union</xsl:template><xsl:template match='*' priority='" + competitor + "'>competitor</xsl:template>"));
    }

    /// <summary>Applies minimum priority only in the effective 4.0 template profile.</summary>
    /// <param name="rootVersion">The stylesheet version.</param>
    /// <param name="templateVersion">The template version.</param>
    /// <param name="expected">The selected template output.</param>
    [Theory]
    [InlineData("3.0", "3.0", "union")]
    [InlineData("4.0", "4.0", "competitor")]
    [InlineData("3.0", "4.0", "competitor")]
    [InlineData("4.0", "3.0", "union")]
    public void UnmatchedLowerPriorityAlternative(string rootVersion, string templateVersion, string expected)
    {
        Assert.Equal(expected, Run("<xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>" +
            "<xsl:template version='" + templateVersion + "' match='root[true()]|missing'>union</xsl:template>" +
            "<xsl:template match='*' priority='0.25'>competitor</xsl:template>", rootVersion));
    }

    /// <summary>Preserves equal alternative priorities in both union spellings.</summary>
    /// <param name="pattern">The union pattern.</param>
    [Theory]
    [InlineData("root|missing")]
    [InlineData("root union missing")]
    public void EqualAlternativePriorities(string pattern)
    {
        Assert.Equal("union", Run("<xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>" +
            "<xsl:template match='" + pattern + "'>union</xsl:template>" +
            "<xsl:template match='*' priority='-0.1'>competitor</xsl:template>"));
    }

    /// <summary>Invokes a union rule once even when multiple alternatives match.</summary>
    /// <param name="version">The effective profile.</param>
    /// <param name="pattern">The union pattern.</param>
    /// <param name="expected">The expected dispatch chain.</param>
    [Theory]
    [InlineData("4.0", "root|*", "UT")]
    [InlineData("4.0", "root|root", "UT")]
    [InlineData("4.0", "root[true()]|root", "UT")]
    [InlineData("4.0", "(root|*)|root[true()]", "UT")]
    [InlineData("4.0", "root union *", "UT")]
    [InlineData("3.0", "root|*", "UUT")]
    [InlineData("3.0", "root|root", "UUT")]
    [InlineData("3.0", "root[true()]|root", "UUT")]
    public void NextMatchVisitsSingleRule(string version, string pattern, string expected)
    {
        Assert.Equal(expected, Run("<xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>" +
            "<xsl:template match='*' priority='-2'>T</xsl:template><xsl:template match='" + pattern + "'>U<xsl:next-match/></xsl:template>", version));
    }

    /// <summary>Retains an explicit union priority in either profile.</summary>
    /// <param name="version">The owning profile.</param>
    [Theory]
    [InlineData("3.0")]
    [InlineData("4.0")]
    public void ExplicitPriority(string version)
    {
        Assert.Equal("CT", Run("""
            <xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>
            <xsl:template match='root|missing/child' priority='-1'>T</xsl:template>
            <xsl:template match='*' priority='0'>C<xsl:next-match/></xsl:template>
            """, version));
    }

    /// <summary>Does not treat two matching alternatives as conflicting rules in a failing mode.</summary>
    [Fact]
    public void AlternativesDoNotCreateAmbiguity()
    {
        Assert.Equal("UT", Run("""
            <xsl:mode on-multiple-match='fail'/>
            <xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>
            <xsl:template match='*' priority='-2'>T</xsl:template>
            <xsl:template match='root|root'>U<xsl:next-match/></xsl:template>
            """));
    }

    /// <summary>Uses the declaring template's version override.</summary>
    /// <param name="rootVersion">The stylesheet version.</param>
    /// <param name="templateVersion">The template version.</param>
    /// <param name="expected">The expected chain.</param>
    [Theory]
    [InlineData("3.0", "4.0", "UT")]
    [InlineData("4.0", "3.0", "UUT")]
    public void TemplateProfileOverride(string rootVersion, string templateVersion, string expected)
    {
        Assert.Equal(expected, Run("<xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>" +
            "<xsl:template match='*' priority='-2'>T</xsl:template><xsl:template version='" + templateVersion + "' match='root|*'>U<xsl:next-match/></xsl:template>", rootVersion));
    }

    /// <summary>Preserves the default namespace across nested union compilation.</summary>
    [Fact]
    public void NestedUnionNamespace()
    {
        Assert.Equal("union", Run("""
            <xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>
            <xsl:template match='(root|missing)|other'>union</xsl:template>
            <xsl:template match='*' priority='-0.1'>fallback</xsl:template>
            """, source: "<root xmlns='urn:n'/>", extra: "xpath-default-namespace='urn:n'"));
    }

    /// <summary>Does not interpret union-like text inside strings or EQNames as an operator.</summary>
    [Fact]
    public void OperatorDelimiters()
    {
        Assert.Equal("union", Run("""
            <xsl:template match='/'><out><xsl:apply-templates select='/*'/></out></xsl:template>
            <xsl:template match="Q{urn:n|part}root | missing[.='x union y']">union</xsl:template>
            <xsl:template match='*' priority='0.25'>fallback</xsl:template>
            """, source: "<root xmlns='urn:n|part'/>") );
    }

    /// <summary>Rejects empty union alternatives.</summary>
    /// <param name="pattern">The malformed pattern.</param>
    [Theory]
    [InlineData("root|")]
    [InlineData("root||root")]
    [InlineData("root union")]
    [InlineData("|root")]
    public void EmptyAlternative(string pattern)
    {
        Assert.ThrowsAny<Exception>(() => Run("<xsl:template match='/'><out/></xsl:template><xsl:template match='" + pattern + "'>bad</xsl:template>"));
    }
}
