// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Unit tests for the compile-time streamability analyzer (XTSE3430)
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-09-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 21-09-2026     | su-filter/su-unclassified batch: boolean-typed variable predicate, positional predicate on striding step, unclassified atomic-param atomization in any position |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for the XSLT 3.0 §19 streamability analyzer: streamable constructs that violate
/// the streaming rules raise static error XTSE3430 at compile time; guaranteed-streamable
/// constructs keep compiling. Calibration shapes derive from the W3C xslt30Test strm corpus.
/// </summary>
public class StreamabilityAnalysisTests
{
    private const string XslNs = "http://www.w3.org/1999/XSL/Transform";

    /// <summary>Wraps a sequence constructor in a named template with a streamable source document.</summary>
    private static string Streamed(string constructor) =>
        $"""<xsl:stylesheet version="3.0" xmlns:xsl="{XslNs}"><xsl:template name="main">{constructor}</xsl:template></xsl:stylesheet>""";

    /// <summary>Wraps a template rule in a streamable default mode.</summary>
    private static string Templated(string templateBody, string match = "/*") =>
        $"""<xsl:stylesheet version="3.0" xmlns:xsl="{XslNs}"><xsl:mode name="m" streamable="yes"/><xsl:template match="{match}" mode="m">{templateBody}</xsl:template></xsl:stylesheet>""";

    private static void AssertXtse3430(string xsl)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new XsltCompiler().Compile(xsl));
        Assert.StartsWith("XTSE3430", ex.Message, StringComparison.Ordinal);
    }

    private static void AssertCompiles(string xsl) => new XsltCompiler().Compile(xsl);

    // ---------------- one consuming use per instruction (R3) ----------------

    [Fact]
    public void TwoConsumingPathsInOneInstruction_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="count(*) + count(*/*)"/></in></xsl:source-document>"""));

    [Fact]
    public void TwoAtomizationsInOneInstruction_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="TITLE || PRICE"/></in></xsl:source-document>"""));

    [Fact]
    public void TwoAvtsInOneLiteralResultElement_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in a="{count(*)}" b="{count(*/*)}"/></xsl:source-document>"""));

    [Fact]
    public void SingleConsumingPath_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="sum(./A/B/C)"/></in></xsl:source-document>"""));

    [Fact]
    public void SiblingInstructionsEachConsume_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="name"/><xsl:value-of select="price"/></in></xsl:source-document>"""));

    [Fact]
    public void MappingOperatorIsSinglePass_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="A/B ! xs:decimal(.)"/></in></xsl:source-document>"""));

    // ---------------- variables must not bind streamed nodes ----------------

    [Fact]
    public void VariableBoundToStreamedContext_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:variable name="current" select="."/><xsl:for-each select="1 to 5"><in><xsl:value-of select="count($current/*[current()])"/></in></xsl:for-each></xsl:source-document>"""));

    [Fact]
    public void VariableBoundToCopyOfStreamedNode_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:variable name="current" select="copy-of(.)"/><xsl:for-each select="1 to 5"><in><xsl:value-of select="count($current/*)"/></in></xsl:for-each></xsl:source-document>"""));

    [Fact]
    public void IterateNextIterationWithStreamedBinding_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:iterate select="*"><xsl:param name="seen" as="xs:integer" select="0"/><xsl:next-iteration><xsl:with-param name="seen" select="."/></xsl:next-iteration></xsl:iterate></xsl:source-document>"""));

    // ---------------- xsl:iterate ----------------

    [Fact]
    public void IterateBodyWithTwoConsumingUses_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:iterate select="child::node()"><in><xsl:value-of select="count(*) + count(*/*)"/></in></xsl:iterate></xsl:source-document>"""));

    [Fact]
    public void IterateBodyMotionless_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:iterate select="child::node()"><in><xsl:value-of select="name(.)"/></in></xsl:iterate></xsl:source-document>"""));

    // ---------------- for-each / for-each-group ----------------

    [Fact]
    public void ForEachWithCrawlingSelectAndConsumingBody_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:for-each select="//ITEM/TITLE"><title><xsl:value-of select="."/></title></xsl:for-each></xsl:source-document>"""));

    [Fact]
    public void ForEachWithCrawlingSelectAndMotionlessBody_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:for-each select="//*"><title><xsl:value-of select="name()"/></title></xsl:for-each></xsl:source-document>"""));

    [Fact]
    public void ForEachGroupWithConsumingGroupBy_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:for-each-group select="*" group-by="string(.)"><in><xsl:value-of select="current-grouping-key()"/></in></xsl:for-each-group></xsl:source-document>"""));

    [Fact]
    public void ForEachGroupWithMotionlessGroupAdjacent_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:for-each-group select="*" group-adjacent="@cat"><in><xsl:value-of select="current-grouping-key()"/></in></xsl:for-each-group></xsl:source-document>"""));

    [Fact]
    public void CurrentGroupInNestedSourceDocument_Throws()
        => AssertXtse3430(Templated("""<xsl:for-each-group select="Order" group-adjacent="@number"><xsl:source-document streamable="yes" href="other.xml"><xsl:for-each select="//transaction[@date = current-group()[1]/Date]"><v><xsl:value-of select="@value"/></v></xsl:for-each></xsl:source-document></xsl:for-each-group>"""));

    [Fact]
    public void CurrentGroupInCopySelectContent_Throws()
        => AssertXtse3430(Templated("""<xsl:variable name="root" as="element()"><xsl:copy-of select="."/></xsl:variable><xsl:for-each-group select="product" group-adjacent="position()"><xsl:copy select="$root"><xsl:copy-of select="current-group()"/></xsl:copy></xsl:for-each-group>"""));

    [Fact]
    public void CurrentGroupDirectlyInGroupBody_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:for-each-group select="*" group-adjacent="@cat"><in><xsl:value-of select="count(current-group())"/></in></xsl:for-each-group></xsl:source-document>"""));

    // ---------------- positional predicates and filters ----------------

    [Fact]
    public void NumericPredicateInScanningPath_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="//section/head[1]"/></in></xsl:source-document>"""));

    [Fact]
    public void PositionalFilterOnCrawlingSequence_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:for-each-group group-adjacent="position()" select="(//*)[position() = 1 to 6]"><in><xsl:value-of select="name()"/></in></xsl:for-each-group></xsl:source-document>"""));

    [Fact]
    public void NonBooleanLoneVariablePredicate_Throws()
        => AssertXtse3430("""<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:f="urn:f" xmlns:xs="http://www.w3.org/2001/XMLSchema"><xsl:function name="f:enum" as="xs:string*" streamability="absorbing"><xsl:param name="element" as="node()*"/><xsl:sequence select="for $i in 1 to 3 return name($element[$i])"/></xsl:function></xsl:stylesheet>""");

    [Fact]
    public void MotionlessFilterPredicate_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="sum(./A/B[@cat = 'x']/C)"/></in></xsl:source-document>"""));

    // ---------------- last() ----------------

    [Fact]
    public void LastOverStreamedSequence_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="/*/*[last()]"/></in></xsl:source-document>"""));

    // ---------------- xsl:sequence ----------------

    [Fact]
    public void SequenceReturningStreamedNodes_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:sequence select="/A/B"/></xsl:source-document>"""));

    [Fact]
    public void SequenceReturningCopyOf_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:sequence select="copy-of(/A/B)"/></xsl:source-document>"""));

    // ---------------- xsl:map ----------------

    [Fact]
    public void MapEntryWithStreamedValue_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:variable name="m" as="map(*)"><xsl:map><xsl:map-entry key="'authors'" select="//AUTHOR"/></xsl:map></xsl:variable><in/></xsl:source-document>"""));

    // ---------------- declared-streamable functions ----------------

    [Fact]
    public void UndeclaredFunctionCalledWithStreamedNode_Throws()
        => AssertXtse3430("""<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:f="urn:f" xmlns:xs="http://www.w3.org/2001/XMLSchema"><xsl:function name="f:probe" as="xs:string"><xsl:param name="n" as="node()"/><xsl:sequence select="name($n)"/></xsl:function><xsl:template name="main"><xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="f:probe(*)"/></in></xsl:source-document></xsl:template></xsl:stylesheet>""");

    [Fact]
    public void InspectionFunctionWithConsumingBody_Throws()
        => AssertXtse3430("""<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:f="urn:f" xmlns:xs="http://www.w3.org/2001/XMLSchema"><xsl:function name="f:bad" streamability="inspection" as="xs:string?"><xsl:param name="element" as="node()?"/><xsl:sequence select="string($element)"/></xsl:function></xsl:stylesheet>""");

    [Fact]
    public void InspectionFunctionMotionlessBody_Compiles()
        => AssertCompiles("""<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:f="urn:f" xmlns:xs="http://www.w3.org/2001/XMLSchema"><xsl:function name="f:ok" streamability="inspection" as="xs:string?"><xsl:param name="element" as="node()?"/><xsl:sequence select="name($element)"/></xsl:function></xsl:stylesheet>""");

    [Fact]
    public void AbsorbingFunctionReferencingArgumentTwice_Throws()
        => AssertXtse3430("""<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:f="urn:f" xmlns:xs="http://www.w3.org/2001/XMLSchema"><xsl:function name="f:bad" streamability="absorbing" as="xs:integer"><xsl:param name="element" as="node()*"/><xsl:sequence select="count($element) + count($element/*)"/></xsl:function></xsl:stylesheet>""");

    // ---------------- apply-templates / climbing ----------------

    [Fact]
    public void ApplyTemplatesWithCrawlingSelect_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:apply-templates select="//ITEM"/></xsl:source-document>"""));

    [Fact]
    public void ApplyTemplatesWithClimbingSelect_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><xsl:apply-templates select="*/ancestor::*"/></xsl:source-document>"""));

    // ---------------- for expressions ----------------

    [Fact]
    public void ForExpressionBindingStreamedSequence_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="count(for $x in * return name($x))"/></in></xsl:source-document>"""));

    [Fact]
    public void ForExpressionOverGroundedBinding_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="string-join(for $i in 1 to 3 return name(ancestor::x[$i]), ',')"/></in></xsl:source-document>"""));

    // ---------------- grounded (non-streamable) stylesheets are untouched ----------------

    [Fact]
    public void NonStreamableStylesheetWithArbitraryXPath_Compiles()
        => AssertCompiles("""<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"><xsl:template match="/"><in><xsl:value-of select="count(//a/b[last()]) + count(//c)"/></in></xsl:template></xsl:stylesheet>""");

    [Fact]
    public void SourceDocumentWithoutStreamable_Compiles()
        => AssertCompiles("""<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"><xsl:template name="main"><xsl:source-document href="x.xml"><in><xsl:value-of select="count(*) + count(*/*)"/></in></xsl:source-document></xsl:template></xsl:stylesheet>""");

    // ---------------- su-filter / su-unclassified batch (analyzer 0.6) ----------------

    [Fact]
    public void FilterFunctionWithBooleanVariablePredicate_Compiles()
        => AssertCompiles("""<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:f="urn:f" xmlns:xs="http://www.w3.org/2001/XMLSchema"><xsl:function name="f:keep" streamability="filter" as="node()?"><xsl:param name="input" as="node()"/><xsl:param name="test" as="xs:boolean"/><xsl:sequence select="$input[$test]"/></xsl:function></xsl:stylesheet>""");

    [Fact]
    public void FilterFunctionWithClimbingPredicate_Compiles()
        => AssertCompiles("""<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:f="urn:f"><xsl:function name="f:keep" streamability="filter" as="node()"><xsl:param name="input" as="node()"/><xsl:sequence select="$input[parent::BOOKS]"/></xsl:function></xsl:stylesheet>""");

    [Fact]
    public void PositionalPredicateOnStridingStep_Compiles()
        => AssertCompiles(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:copy-of select="/A/B/C[position() ne 42]"/></in></xsl:source-document>"""));

    [Fact]
    public void PositionalPredicateUsingLastOnStridingStep_Throws()
        => AssertXtse3430(Streamed("""<xsl:source-document streamable="yes" href="x.xml"><in><xsl:copy-of select="/A/B/C[position() ne last()]"/></in></xsl:source-document>"""));

    [Fact]
    public void UnclassifiedFunctionAtomizingStreamedArgInSecondPosition_Compiles()
        => AssertCompiles("""<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:f="urn:f" xmlns:xs="http://www.w3.org/2001/XMLSchema"><xsl:function name="f:avg2" streamability="unclassified" as="xs:decimal?"><xsl:param name="two" as="xs:decimal"/><xsl:param name="in" as="xs:decimal*"/><xsl:sequence select="round(avg($in), 2) + $two"/></xsl:function><xsl:template name="main"><xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="f:avg2(2, /A/B/C)"/></in></xsl:source-document></xsl:template></xsl:stylesheet>""");

    [Fact]
    public void UnclassifiedFunctionStreamedNodeToNodeParamInSecondPosition_Throws()
        => AssertXtse3430("""<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:f="urn:f"><xsl:function name="f:probe" streamability="unclassified" as="xs:string"><xsl:param name="two" as="xs:decimal"/><xsl:param name="n" as="node()*"/><xsl:sequence select="name($n[1])"/></xsl:function><xsl:template name="main"><xsl:source-document streamable="yes" href="x.xml"><in><xsl:value-of select="f:probe(2, /A/B/C)"/></in></xsl:source-document></xsl:template></xsl:stylesheet>""");
}
