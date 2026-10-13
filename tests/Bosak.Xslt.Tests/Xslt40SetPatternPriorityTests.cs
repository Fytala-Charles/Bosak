// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies XSLT 4.0 set-operand and predicate-pattern default priorities.
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

/// <summary>Verifies XSLT 4.0 set-operand and predicate-pattern default priorities.</summary>
public class Xslt40SetPatternPriorityTests
{
    private static string Run(string pattern, string priority, string version = "4.0", string select = "/*", string source = "<root><child/></root>") => XElement.Parse(
        new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version + "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:n='urn:n' exclude-result-prefixes='#all'>" +
            "<xsl:template match='/' priority='2'><out><xsl:apply-templates select='" + select + "'/></out></xsl:template>" +
            "<xsl:template match='" + pattern + "'>default</xsl:template><xsl:template match='.' priority='" + priority + "'>explicit</xsl:template></xsl:stylesheet>")
        .TransformToString(new XDocumentNode(XDocument.Parse(source)))).Value;

    /// <summary>Uses the first set operand's priority rather than the complete text or later operands.</summary>
    /// <param name="pattern">The set pattern.</param>
    /// <param name="priority">The explicit competitor priority.</param>
    /// <param name="expected">The expected winning marker.</param>
    [Theory]
    [InlineData("root intersect *", "0.1", "explicit")]
    [InlineData("root intersect *", "-0.1", "default")]
    [InlineData("* intersect root", "-0.4", "explicit")]
    [InlineData("* except missing", "-0.6", "default")]
    [InlineData("root except missing[true()]", "0.1", "explicit")]
    [InlineData("root except missing/child", "0.1", "explicit")]
    [InlineData("root intersect * except missing", "0.1", "explicit")]
    [InlineData("root except missing intersect *", "0.1", "explicit")]
    [InlineData("(root intersect *)", "0.1", "explicit")]
    [InlineData("(root)intersect(*)", "0.1", "explicit")]
    [InlineData("root except(missing)", "0.1", "explicit")]
    [InlineData("(root|*) intersect *", "0.1", "explicit")]
    [InlineData("(* except missing)|root", "-0.1", "default")]
    [InlineData("root intersect * | missing/child", "0.25", "default")]
    [InlineData("root[true()] except missing", "0.25", "default")]
    [InlineData("root except (: nested (: comment :) :) missing", "0.1", "explicit")]
    public void FirstOperandPriority(string pattern, string priority, string expected) => Assert.Equal(expected, Run(pattern, priority));

    /// <summary>Distinguishes the bare-dot priority from predicate and node-pattern priorities.</summary>
    /// <param name="pattern">The predicate pattern.</param>
    /// <param name="priority">The competing priority.</param>
    /// <param name="version">The effective stylesheet profile.</param>
    /// <param name="expected">The expected winner.</param>
    [Theory]
    [InlineData(".", "-0.75", "4.0", "explicit")]
    [InlineData(".", "-1.1", "4.0", "default")]
    [InlineData(".", "-0.75", "3.0", "default")]
    [InlineData(".[true()]", "0.75", "4.0", "default")]
    [InlineData(". [child::child]", "0.75", "4.0", "default")]
    [InlineData(".[child/..]", "0.75", "4.0", "default")]
    [InlineData(".[contains(\"a/b\", \"/\")]", "0.75", "4.0", "default")]
    [InlineData("(: before :) . (: between :) [child/..]", "0.75", "4.0", "default")]
    [InlineData(".[true()][child/..]", "0.75", "4.0", "default")]
    [InlineData("root[child/..]", "0.75", "4.0", "explicit")]
    public void PredicatePriority(string pattern, string priority, string version, string expected) => Assert.Equal(expected, Run(pattern, priority, version));

    /// <summary>Matches atomic values with the bare-dot predicate pattern.</summary>
    [Fact]
    public void BareDotMatchesAtomicValues() => Assert.Equal("default", Run(".", "-1.1", select: "42"));

    /// <summary>Does not interpret operator words inside a URI or an element name as set operators.</summary>
    /// <param name="pattern">The node pattern.</param>
    /// <param name="source">The input tree.</param>
    /// <param name="expected">The expected winning marker.</param>
    [Theory]
    [InlineData("Q{urn: except intersect }root", "<root xmlns='urn: except intersect '/>", "explicit")]
    [InlineData("except", "<except/>", "explicit")]
    [InlineData("intersect", "<intersect/>", "explicit")]
    [InlineData("root[contains(\"except intersect\", \"except\")] except missing", "<root/>", "default")]
    public void OperatorTextBoundaries(string pattern, string source, string expected) => Assert.Equal(expected, Run(pattern, "0.1", source: source));

    /// <summary>Preserves set membership when the selected node is excluded.</summary>
    [Fact]
    public void ExcludedNodeDoesNotMatch() => Assert.Equal("explicit", Run("root except root", "-0.75"));

    /// <summary>Preserves predicate nonmatches after changing their priority.</summary>
    [Fact]
    public void FalsePredicateDoesNotMatch() => Assert.Equal("explicit", Run(".[false()]", "0.75"));

    /// <summary>Retains the earlier-profile set-pattern default.</summary>
    [Fact]
    public void EarlierSetPriority() => Assert.Equal("default", Run("* except missing", "0.1", "3.0"));
}
