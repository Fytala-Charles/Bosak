// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 september 2026
// PURPOSE              : Unit tests for XSLT 3.0 §11.7.3 content-evaluation rules (value-of/attribute separators, text coalescing, xsl:copy of atomics).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 17-09-2026     | Creation (D2: §11.7.3 separator/coalescing rules, copy-of-atomics, template @as)        |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

public class ContentEvaluationTests
{
    private static string Run(string xsl, XDocument? source = null, string? initialTemplate = null)
    {
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var src = source != null
            ? (IXdmNode)new XDocumentNode(source)
            : (IXdmNode)new XDocumentNode(new XDocument(new XElement("dummy")));
        return executable.TransformToString(src, initialTemplate: initialTemplate);
    }

    private static XDocument Source(string xml) => XDocument.Parse(xml);

    // ----- xsl:value-of select: §11.7.3 rules -----

    [Fact]
    public void ValueOf_AdjacentTextNodes_CoalesceWithoutSeparator()
    {
        // Two adjacent text nodes in the source must concatenate with NO separator
        // (si-value-of-009/047): the default separator is ignored between them.
        var source = new XDocument(new XElement("root",
            new XElement("a", new XText("1"), new XText("2")),
            new XElement("b", "3")));
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:value-of select='//text()'/></out>
  </xsl:template>
</xsl:stylesheet>", source);
        Assert.True(result.Contains("<out>123</out>"), result);
    }

    [Fact]
    public void ValueOf_AllTextNodes_IgnoresExplicitSeparator()
    {
        var source = new XDocument(new XElement("root",
            new XElement("a", new XText("1"), new XText("2")),
            new XElement("b", "3")));
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:value-of select='//text()' separator='|'/></out>
  </xsl:template>
</xsl:stylesheet>", source);
        Assert.True(result.Contains("<out>123</out>"), result);
    }

    [Fact]
    public void ValueOf_MixedAtomicsAndTextRun_JoinsEachRunWithSeparator()
    {
        // Atomics occupy individual slots; the coalesced text run occupies one slot;
        // all slots join with the separator (si-value-of-075 shape).
        var source = new XDocument(new XElement("root",
            new XElement("a", new XText("4"), new XText("5"))));
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:value-of select='(1 to 3), //text(), (6 to 7)' separator='|'/></out>
  </xsl:template>
</xsl:stylesheet>", source);
        Assert.True(result.Contains("<out>1|2|3|45|6|7</out>"), result);
    }

    [Fact]
    public void ValueOf_EmptyStringAtomics_OccupyJoinSlots()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:value-of select='""a"", """", ""b""'/></out>
  </xsl:template>
</xsl:stylesheet>");
        Assert.True(result.Contains("<out>a  b</out>"), result);
    }

    [Fact]
    public void ValueOf_OnlyEmptyStringAtomics_ProducesSeparators()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:value-of select='"""", """"' separator=','/></out>
  </xsl:template>
</xsl:stylesheet>");
        Assert.True(result.Contains("<out>,</out>"), result);
    }

    [Fact]
    public void ValueOf_ZeroLengthTextNodes_AreRemovedBeforeAllTextTest()
    {
        // An empty xsl:value-of produces a zero-length text node; removed, so the
        // remaining all-text sequence still concatenates without separators.
        var source = new XDocument(new XElement("root", new XElement("a", "12")));
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:value-of select='//text(), //text()' separator='|'/></out>
  </xsl:template>
</xsl:stylesheet>", source);
        Assert.True(result.Contains("<out>1212</out>"), result);
    }

    // ----- xsl:value-of sequence-constructor content -----

    [Fact]
    public void ValueOf_SequenceConstructor_AllText_IgnoresSeparator()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:value-of separator='|'><xsl:text>a</xsl:text><xsl:text>b</xsl:text></xsl:value-of></out>
  </xsl:template>
</xsl:stylesheet>");
        Assert.True(result.Contains("<out>ab</out>"), result);
    }

    [Fact]
    public void ValueOf_SequenceConstructor_MixedTextAndAtomic_UsesSeparator()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:value-of separator='|'><xsl:text>a</xsl:text><xsl:sequence select=""'b'""/><xsl:text>c</xsl:text></xsl:value-of></out>
  </xsl:template>
</xsl:stylesheet>");
        Assert.True(result.Contains("<out>a|b|c</out>"), result);
    }

    // ----- xsl:attribute content follows the same rules -----

    [Fact]
    public void Attribute_AllTextContent_IgnoresSeparator()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:attribute name='a' separator='|'><xsl:text>1</xsl:text><xsl:text>2</xsl:text></xsl:attribute></out>
  </xsl:template>
</xsl:stylesheet>");
        Assert.True(result.Contains("a=\"12\""), result);
    }

    [Fact]
    public void Attribute_SelectAdjacentTextNodes_Coalesces()
    {
        var source = new XDocument(new XElement("root",
            new XElement("a", new XText("1"), new XText("2")),
            new XElement("b", "3")));
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:attribute name='a' select='//text()'/></out>
  </xsl:template>
</xsl:stylesheet>", source);
        Assert.True(result.Contains("a=\"123\""), result);
    }

    [Fact]
    public void Attribute_EmptyStringAtomics_OccupyJoinSlots()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:attribute name='a' select='""x"", """", ""y""'/></out>
  </xsl:template>
</xsl:stylesheet>");
        Assert.True(result.Contains("a=\"x  y\""), result);
    }

    // ----- xsl:copy of atomic values -----

    [Fact]
    public void Copy_OfAtomicValues_SpaceJoins()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:for-each select='1 to 3'><xsl:copy/></xsl:for-each></out>
  </xsl:template>
</xsl:stylesheet>");
        Assert.True(result.Contains("<out>1 2 3</out>"), result);
    }

    [Fact]
    public void Copy_SelectAtomicValue_SpaceJoinsAcrossIterations()
    {
        var source = Source("<root><a>-1</a><a>-2</a></root>");
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:for-each select='/root/a/data(.)'><xsl:copy select='.'/></xsl:for-each></out>
  </xsl:template>
</xsl:stylesheet>", source);
        Assert.True(result.Contains("<out>-1 -2</out>"), result);
    }

    [Fact]
    public void Copy_TextNodesThenAtomics_ConcatenatesThenSpaceJoins()
    {
        // Text-node copies concatenate without separator; a following atomic copy is
        // appended without a separator (it is not adjacent to an atomic), and further
        // atomic copies are space-joined (si-copy-007 shape: "…16.47101 102").
        var source = new XDocument(new XElement("root",
            new XElement("a", new XText("4"), new XText("5")),
            new XElement("b", "6")));
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out><xsl:for-each select='//text(), 7, 8'><xsl:copy/></xsl:for-each></out>
  </xsl:template>
</xsl:stylesheet>", source);
        Assert.True(result.Contains("<out>4567 8</out>"), result);
    }

    [Fact]
    public void Copy_WithoutContextItem_StillRaisesXTTE0945()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'
                xmlns:f='http://example.com/f'>
  <xsl:template name='main'>
    <out><xsl:copy-of select='f:f()'/></out>
  </xsl:template>
  <xsl:function name='f:f'>
    <xsl:copy>
      <a/>
    </xsl:copy>
  </xsl:function>
</xsl:stylesheet>", initialTemplate: "main"));
        Assert.Contains("XTTE0945", ex.Message);
    }

    // ----- xsl:copy of a sequence of attribute nodes into a typed variable -----

    [Fact]
    public void Copy_AttributeSequenceIntoTypedVariable_KeepsAllItems()
    {
        var source = Source("<root><a x='1'/><a x='2'/><a x='3'/></root>");
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out>
      <xsl:variable name='atts' as='attribute(*)*'>
        <xsl:for-each select='/root/a/@x'><xsl:copy/></xsl:for-each>
      </xsl:variable>
      <xsl:value-of select='data($atts)'/>
    </out>
  </xsl:template>
</xsl:stylesheet>", source);
        Assert.True(result.Contains("<out>1 2 3</out>"), result);
    }

    // ----- template @as conversion preserves atomicity (si-value-of-100 shape) -----

    [Fact]
    public void Template_AsString_InsideValueOf_KeepsSeparatorSemantics()
    {
        var source = Source("<root><r><x>a</x></r><r><x>b</x></r></root>");
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'
                xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='xs'>
  <xsl:template match='/'>
    <out><xsl:value-of separator='|'><xsl:apply-templates select='/root/r'/></xsl:value-of></out>
  </xsl:template>
  <xsl:template match='r' as='xs:string'>
    <xsl:value-of select='x'/>
  </xsl:template>
</xsl:stylesheet>", source);
        Assert.True(result.Contains("<out>a|b</out>"), result);
    }

    // ----- xsl:on-empty keeps empty-string slots -----

    [Fact]
    public void OnEmpty_EmptyStringAtomics_OccupyJoinSlots()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'
                xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='xs'>
  <xsl:template match='/'>
    <out>
      <xsl:sequence select='()'/>
      <xsl:on-empty>
        <xsl:sequence select=""'23', '', xs:date('2011-01-01'), '', '0', ''""/>
      </xsl:on-empty>
    </out>
  </xsl:template>
</xsl:stylesheet>");
        Assert.True(result.Contains("<out>23  2011-01-01  0 </out>"), result);
    }
}
