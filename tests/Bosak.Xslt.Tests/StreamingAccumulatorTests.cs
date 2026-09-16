// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Unit tests for xsl:accumulator over burst-mode streamed sources (Phase B)
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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for streaming accumulators (Phase B): push-style per-record evaluation over a
/// burst-mode streamed source. Every behavioral test asserts parity with the in-memory
/// transform of the same stylesheet; the error contract mirrors in-memory error codes
/// where the streaming model shares the semantics (XTDE3341/3362/3400/3350).
/// </summary>
[Collection("MemorySensitive")]
public class StreamingAccumulatorTests
{
    private const string Book = """
        <book>
        <chap><fig>A</fig><fig>B</fig></chap>
        <chap><fig>C</fig></chap>
        </book>
        """;

    private const string Transactions = """
        <transactions>
        <transaction amount="10.50"/>
        <transaction amount="20.25"/>
        <transaction amount="5.00"/>
        </transactions>
        """;

    private static string InMemory(string xsl, string xml)
        => new XsltCompiler().Compile(xsl)
            .Transform(XDocumentProvider.ParseXml(xml)).NodeValue!.ToXmlString();

    private static string Streamed(string xsl, string xml)
        => new XsltCompiler().Compile(xsl)
            .TransformStreaming(new MemoryStream(Encoding.UTF8.GetBytes(xml))).NodeValue!.ToXmlString();

    private static void AssertParity(string xsl, string xml)
        => Assert.Equal(InMemory(xsl, xml), Streamed(xsl, xml));

    [Fact]
    public void CounterAccumulator_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="figNr" initial-value="0">
                <xsl:accumulator-rule match="chap" select="0"/>
                <xsl:accumulator-rule match="fig" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="figNr"/>
              <xsl:template match="fig"><f nr="{accumulator-before('figNr')}"><xsl:value-of select="."/></f></xsl:template>
              <xsl:template match="/"><out><xsl:apply-templates/></out></xsl:template>
            </xsl:stylesheet>
            """, Book);

    [Fact]
    public void SumWithAccumulatorAfter_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="total" initial-value="0" as="xs:double" xmlns:xs="http://www.w3.org/2001/XMLSchema">
                <xsl:accumulator-rule match="transaction" select="$value + number(@amount)"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="total"/>
              <xsl:template match="/transactions"><xsl:variable name="done"><xsl:apply-templates/></xsl:variable><out total="{accumulator-after('total')}"/></xsl:template>
              <xsl:template match="/"><xsl:apply-templates/></xsl:template>
            </xsl:stylesheet>
            """, Transactions);

    [Fact]
    public void MultiAccumulatorMinMax_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="count" initial-value="0"><xsl:accumulator-rule match="transaction" select="$value + 1"/></xsl:accumulator>
              <xsl:accumulator name="sum" initial-value="0"><xsl:accumulator-rule match="transaction" select="$value + number(@amount)"/></xsl:accumulator>
              <xsl:accumulator name="min" initial-value="999999999999"><xsl:accumulator-rule match="transaction" select="if (number(@amount) &lt; $value) then number(@amount) else $value"/></xsl:accumulator>
              <xsl:accumulator name="max" initial-value="-999999999999"><xsl:accumulator-rule match="transaction" select="if (number(@amount) &gt; $value) then number(@amount) else $value"/></xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="#all"/>
              <xsl:template match="/transactions"><xsl:variable name="done"><xsl:apply-templates/></xsl:variable><out
                count="{accumulator-after('count')}" sum="{accumulator-after('sum')}"
                min="{accumulator-after('min')}" max="{accumulator-after('max')}"/></xsl:template>
              <xsl:template match="/"><xsl:apply-templates/></xsl:template>
            </xsl:stylesheet>
            """, Transactions);

    [Fact]
    public void PhaseStartEndStack_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="secNr" initial-value="()">
                <xsl:accumulator-rule match="section" phase="start" select="0, head(($value, 0)[1]) + 1, tail(($value, ()))"/>
                <xsl:accumulator-rule match="section" phase="end" select="tail(($value, ()))"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="secNr"/>
              <xsl:template match="section"><s nr="{string-join(accumulator-before('secNr') ! string(.), '.')}"/></xsl:template>
              <xsl:template match="/"><out><xsl:apply-templates/></out></xsl:template>
            </xsl:stylesheet>
            """,
            "<doc><section><title>A</title><section><title>A1</title></section></section><section><title>B</title></section></doc>");

    [Fact]
    public void TextNodeRule_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="item-cost" initial-value="0">
                <xsl:accumulator-rule match="cost/text()" select="$value + xs:integer(.)"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="item-cost"/>
              <xsl:template match="/order"><xsl:variable name="done"><xsl:apply-templates/></xsl:variable><out sum="{accumulator-after('item-cost')}"/></xsl:template>
              <xsl:template match="/"><xsl:apply-templates/></xsl:template>
            </xsl:stylesheet>
            """,
            "<order><item><cost>10</cost></item><item><cost>25</cost></item></order>");

    [Fact]
    public void RootedPathRule_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="firstCost" initial-value="()">
                <xsl:accumulator-rule match="/order/item/cost/text()" select="string(.)"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="firstCost"/>
              <xsl:template match="cost"><c first="{accumulator-before('firstCost')}"><xsl:value-of select="."/></c></xsl:template>
              <xsl:template match="/"><out><xsl:apply-templates/></out></xsl:template>
            </xsl:stylesheet>
            """,
            "<order><item><cost>10</cost></item><item><cost>25</cost></item></order>");

    [Fact]
    public void DocumentNodeEndPhaseRule_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="seen" initial-value="'no'">
                <xsl:accumulator-rule match="/" phase="end" select="'yes'"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="seen"/>
              <xsl:template match="/"><xsl:variable name="done"><xsl:apply-templates/></xsl:variable><out seen="{accumulator-after('seen')}"/></xsl:template>
            </xsl:stylesheet>
            """,
            "<doc><a/></doc>");

    [Fact]
    public void SequenceConstructorRuleBody_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="figNr" initial-value="0">
                <xsl:accumulator-rule match="fig"><xsl:sequence select="$value + 1"/></xsl:accumulator-rule>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="figNr"/>
              <xsl:template match="fig"><f nr="{accumulator-before('figNr')}"/></xsl:template>
              <xsl:template match="/"><out><xsl:apply-templates/></out></xsl:template>
            </xsl:stylesheet>
            """, Book);

    [Fact]
    public void GlobalParamInInitialValue_Parity()
    {
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:param name="start" select="100"/>
              <xsl:accumulator name="n" initial-value="$start">
                <xsl:accumulator-rule match="fig" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="n"/>
              <xsl:template match="fig"><f nr="{accumulator-before('n')}"/></xsl:template>
              <xsl:template match="/"><out><xsl:apply-templates/></out></xsl:template>
            </xsl:stylesheet>
            """;
        AssertParity(xsl, Book);
    }

    [Fact]
    public void InapplicableAccumulator_ThrowsXTDE3362()
    {
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="a1" initial-value="0">
                <xsl:accumulator-rule match="item" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip"/>
              <xsl:template match="/"><out><xsl:value-of select="accumulator-before('a1')"/></out></xsl:template>
            </xsl:stylesheet>
            """;

        var ex = Assert.ThrowsAny<Exception>(() => Streamed(xsl, "<order><item/></order>"));
        Assert.Contains("XTDE3362", ex.Message);
    }

    [Fact]
    public void UnknownAccumulator_ThrowsXTDE3341()
    {
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="a1" initial-value="0">
                <xsl:accumulator-rule match="item" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="a1"/>
              <xsl:template match="/"><out><xsl:value-of select="accumulator-before('nope')"/></out></xsl:template>
            </xsl:stylesheet>
            """;

        var ex = Assert.ThrowsAny<Exception>(() => Streamed(xsl, "<order><item/></order>"));
        Assert.Contains("XTDE3341", ex.Message);
    }

    [Fact]
    public void AccumulatorAfterAtDocument_DrainsStreamAndReturnsValue()
    {
        // Reading accumulator-after at the streamed document/root is a consuming
        // (grounding) operation: the engine drains the remaining records and returns
        // the accumulated value once nothing is mid-enumeration.
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="a1" initial-value="0">
                <xsl:accumulator-rule match="item" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="deep-skip" use-accumulators="a1"/>
              <xsl:template match="/"><out n="{accumulator-after('a1')}"/></xsl:template>
            </xsl:stylesheet>
            """;

        var result = Streamed(xsl, "<order><item/><item/><item/></order>");
        Assert.Contains("n=\"3\"", result);
    }

    [Fact]
    public void AccumulatorAfterAtDocument_WhileStreamIsConsumed_ThrowsXTDE3350()
    {
        // The same read is unavailable while the stream is mid-enumeration: the engine
        // cannot drain without stealing records from the active loop.
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="a1" initial-value="0">
                <xsl:accumulator-rule match="item" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="a1"/>
              <xsl:template match="/"><out><xsl:for-each select="/order/item"><xsl:value-of select="(/) ! accumulator-after('a1')"/></xsl:for-each></out></xsl:template>
            </xsl:stylesheet>
            """;

        var ex = Assert.ThrowsAny<Exception>(() => Streamed(xsl, "<order><item/><item/></order>"));
        Assert.Contains("XTDE3350", ex.Message);
    }

    [Fact]
    public void ForwardCrossAccumulatorReference_SurfacesAtAccess()
    {
        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="first" initial-value="0">
                <xsl:accumulator-rule match="item" select="accumulator-before('second') + 1"/>
              </xsl:accumulator>
              <xsl:accumulator name="second" initial-value="0">
                <xsl:accumulator-rule match="item" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="#all"/>
              <xsl:template match="item"><i f="{accumulator-before('first')}"/></xsl:template>
              <xsl:template match="/"><out><xsl:apply-templates/></out></xsl:template>
            </xsl:stylesheet>
            """;

        // 'first' references 'second', declared later: per spec bug 29813 the rule
        // failure is deferred and surfaces loudly when the poisoned accumulator is read.
        var ex = Assert.ThrowsAny<Exception>(() => Streamed(xsl, "<order><item/></order>"));
        Assert.Contains("declared earlier", ex.Message);
    }

    [Fact]
    public void BackwardCrossAccumulatorReference_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="count" initial-value="0">
                <xsl:accumulator-rule match="item" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:accumulator name="doubled" initial-value="0">
                <xsl:accumulator-rule match="item" select="accumulator-before('count') * 2"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="#all"/>
              <xsl:template match="item"><i d="{accumulator-before('doubled')}"/></xsl:template>
              <xsl:template match="/"><out><xsl:apply-templates/></out></xsl:template>
            </xsl:stylesheet>
            """,
            "<order><item/><item/><item/></order>");

    [Fact]
    public void CopyOfCarriesAccumulatorValues_Parity()
        => AssertParity("""
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="figNr" initial-value="0">
                <xsl:accumulator-rule match="fig" select="$value + 1"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="figNr"/>
              <xsl:template match="fig">
                <xsl:variable name="c"><xsl:copy-of select="."/></xsl:variable>
                <f nr="{accumulator-before('figNr')}" copynr="{$c/fig/accumulator-before('figNr')}"/>
              </xsl:template>
              <xsl:template match="/"><out><xsl:apply-templates/></out></xsl:template>
            </xsl:stylesheet>
            """, Book);

    [Fact]
    public void BoundedMemoryWithAccumulator()
    {
        const int recordCount = 500_000;
        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write("<records>");
            for (int i = 0; i < recordCount; i++)
                writer.Write($"<record id=\"{i}\"><value>1</value></record>");
            writer.Write("</records>");
        }
        stream.Position = 0;

        const string xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema">
              <xsl:output method="xml" indent="no"/>
              <xsl:accumulator name="total" initial-value="0" as="xs:integer">
                <xsl:accumulator-rule match="value/text()" select="$value + xs:integer(.)"/>
              </xsl:accumulator>
              <xsl:mode on-no-match="shallow-skip" use-accumulators="total"/>
              <xsl:template match="record"><xsl:if test="accumulator-before('total') mod 100000 = 0"><mark/></xsl:if></xsl:template>
              <xsl:template match="/"><out><xsl:apply-templates/></out></xsl:template>
            </xsl:stylesheet>
            """;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long before = GC.GetTotalMemory(true);

        var result = new XsltCompiler().Compile(xsl).TransformStreaming(stream);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long growth = GC.GetTotalMemory(true) - before;

        int marks = 0;
        foreach (var d in result.NodeValue!.Axis(Bosak.XPath.Core.Xdm.XdmAxis.Descendant))
            if (d.NodeValue!.LocalName == "mark")
                marks++;

        Assert.Equal(5, marks);
        // Accumulator values travel as per-record annotations and die with the record;
        // growth stays bounded well below the input tree size.
        Assert.True(growth < 100_000_000, $"live growth {growth / 1_000_000.0:F1} MB exceeds the bounded-memory budget");
    }
}
