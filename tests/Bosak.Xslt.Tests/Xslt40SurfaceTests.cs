// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 October 2026
// PURPOSE              : Unit tests for the XSLT 4.0 surfaces of REQ-118 slice 4.0-S7
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 08-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests the XSLT 4.0 "easy surfaces" of REQ-118 slice 4.0-S7: <c>xsl:note</c>
/// (discarded without validation), <c>xsl:if</c> with <c>then</c>/<c>else</c>
/// attributes, <c>separator</c> on <c>xsl:for-each</c>/<c>xsl:apply-templates</c>,
/// and <c>xsl:map</c> with <c>select</c>/<c>duplicates</c> plus first-class
/// <c>xsl:map-entry</c> usage.
/// </summary>
/// <remarks>
/// Per XSLT 4.0 draft §3.8.2/§3.8.3 no differences are defined for XSLT 3.0 behavior:
/// an XSLT 4.0 processor produces the same results whether the effective version is
/// 3.0 or 4.0, so these surfaces are NOT gated to version="4.0" stylesheets — the
/// tests below pin that they also work at version="3.0", and that version="5.0"
/// stylesheets keep forwards-compatible processing.
/// </remarks>
public class Xslt40SurfaceTests
{
    private const string XslNs = "http://www.w3.org/1999/XSL/Transform";
    private const string MapNs = "http://www.w3.org/2005/xpath-functions/map";

    private static string Wrap(string inner, string version, string? decls = null, string? rootAttrs = null)
        => $"<xsl:stylesheet version='{version}' xmlns:xsl='{XslNs}' xmlns:map='{MapNs}' exclude-result-prefixes='map'" +
           (rootAttrs is null ? string.Empty : " " + rootAttrs) + ">" +
           (decls ?? string.Empty) + inner + "</xsl:stylesheet>";

    private static string RunTransform(string inner, string version = "4.0", string? sourceXml = null, string? decls = null, string? rootAttrs = null)
    {
        var executable = new Api.XsltCompiler().Compile(Wrap(inner, version, decls, rootAttrs), "file:///test.xsl");
        var src = new XDocumentNode(XDocument.Parse(sourceXml ?? "<root/>"));
        return executable.TransformToString(src);
    }

    private static Exception CompileError(string inner, string version = "4.0", string? decls = null)
        => Assert.Throws<InvalidOperationException>(() =>
            new Api.XsltCompiler().Compile(Wrap(inner, version, decls), "file:///test.xsl"));

    private static Exception TransformError(string inner, string version = "4.0", string? sourceXml = null, string? decls = null)
    {
        var executable = new Api.XsltCompiler().Compile(Wrap(inner, version, decls), "file:///test.xsl");
        var src = new XDocumentNode(XDocument.Parse(sourceXml ?? "<root/>"));
        return Assert.ThrowsAny<Exception>(() => executable.TransformToString(src));
    }

    // ------------------------------------------------------------------
    // xsl:note (XSLT 4.0 §3.11.2)
    // ------------------------------------------------------------------

    [Fact]
    public void Note_InTemplate_IsDiscarded()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:note author='test'>any <xsl:fantasy>content</xsl:fantasy></xsl:note>
                <out>ok</out>
            </xsl:template>
            """);
        Assert.Contains("<out>ok</out>", result);
        Assert.DoesNotContain("note", result);
    }

    [Fact]
    public void Note_AtTopLevel_IsDiscarded()
    {
        // xsl:note may appear anywhere except as the outermost element, so a
        // top-level xsl:note is legal and discarded.
        var result = RunTransform("""
            <xsl:note>stylesheet documentation</xsl:note>
            <xsl:template match='/'><out>ok</out></xsl:template>
            """);
        Assert.Contains("<out>ok</out>", result);
    }

    [Fact]
    public void Note_ContentIsNotValidated()
    {
        // The processor discards xsl:note without validating attributes or content:
        // an unknown XSLT element and a bogus attribute inside xsl:note must not
        // raise XTSE0010/XTSE0090.
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:note some-unknown-attribute='x'><xsl:totally-bogus a='1'/></xsl:note>
                <out>ok</out>
            </xsl:template>
            """);
        Assert.Contains("<out>ok</out>", result);
    }

    // ------------------------------------------------------------------
    // xsl:if then/else (XSLT 4.0 §8.1)
    // ------------------------------------------------------------------

    [Fact]
    public void If_ThenElse_TrueBranch()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:if test='2 > 1' then="'yes'" else="'no'"/></out>
            </xsl:template>
            """);
        Assert.Contains("<out>yes</out>", result);
    }

    [Fact]
    public void If_ThenElse_FalseBranch()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:if test='1 > 2' then="'yes'" else="'no'"/></out>
            </xsl:template>
            """);
        Assert.Contains("<out>no</out>", result);
    }

    [Fact]
    public void If_ThenOnly_FalseIsEmpty()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:if test='false()' then="'yes'"/></out>
            </xsl:template>
            """);
        Assert.Contains("<out/>", result.Replace(" ", string.Empty));
    }

    [Fact]
    public void If_ElseWithContent_ContentIsTrueBranch()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:if test='true()' else="'fallback'">content</xsl:if></out>
            </xsl:template>
            """);
        Assert.Contains("<out>content</out>", result);

        var result2 = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:if test='false()' else="'fallback'">content</xsl:if></out>
            </xsl:template>
            """);
        Assert.Contains("<out>fallback</out>", result2);
    }

    [Fact]
    public void If_ThenWithChildren_XTSE0010()
    {
        var ex = CompileError("""
            <xsl:template match='/'>
                <xsl:if test='true()' then="'yes'"><out/></xsl:if>
            </xsl:template>
            """);
        Assert.Contains("XTSE0010", ex.Message);
    }

    [Fact]
    public void If_NoEffectiveBooleanValue_FORG0006()
    {
        var ex = TransformError("""
            <xsl:template match='/'>
                <out><xsl:if test='(1, 2)' then="'yes'" else="'no'"/></out>
            </xsl:template>
            """);
        Assert.Contains("FORG0006", ex.Message);
    }

    [Fact]
    public void If_ThenElse_InFunctionBody()
    {
        var result = RunTransform("""
            <xsl:function name='f:abs' xmlns:f='urn:f'>
                <xsl:param name='x'/>
                <xsl:if test='$x &lt; 0' then="-$x" else="$x"/>
            </xsl:function>
            <xsl:template match='/' xmlns:f='urn:f'>
                <out><xsl:value-of select='f:abs(-3), f:abs(5)'/></out>
            </xsl:template>
            """);
        Assert.Contains(">3 5</out>", result);
    }

    [Fact]
    public void If_ThenElse_WorksAtVersion_3_0()
    {
        // Spec-pinned: an XSLT 4.0 processor applies the 4.0 instruction regardless
        // of the effective version (draft §3.8.2/§3.8.3).
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:if test='true()' then="'yes'" else="'no'"/></out>
            </xsl:template>
            """, version: "3.0");
        Assert.Contains("<out>yes</out>", result);
    }

    // ------------------------------------------------------------------
    // separator on xsl:for-each (XSLT 4.0 §7.1.1)
    // ------------------------------------------------------------------

    [Fact]
    public void ForEach_Separator_BetweenItems()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:for-each select='1 to 3' separator='|'><xsl:sequence select='.'/></xsl:for-each></out>
            </xsl:template>
            """);
        Assert.Contains("<out>1|2|3</out>", result);
    }

    [Fact]
    public void ForEach_Separator_WithSort()
    {
        // Draft example: select="6, 3, 9" with xsl:sort and sequence select="., . + 1"
        // yields 3, 4, "|", 6, 7, "|", 9, 10.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:for-each select='6, 3, 9' separator='|'>
                    <xsl:sort select='.'/>
                    <xsl:sequence select='., . + 1'/>
                </xsl:for-each></out>
            </xsl:template>
            """);
        Assert.Contains("<out>3 4|6 7|9 10</out>", result);
    }

    [Fact]
    public void ForEach_Separator_EmptyAndSingleItem_NoSeparator()
    {
        var empty = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:for-each select='()' separator='|'><xsl:sequence select='.'/></xsl:for-each></out>
            </xsl:template>
            """);
        Assert.Contains("<out/>", empty.Replace(" ", string.Empty));

        var single = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:for-each select='42' separator='|'><xsl:sequence select='.'/></xsl:for-each></out>
            </xsl:template>
            """);
        Assert.Contains("<out>42</out>", single);
    }

    [Fact]
    public void ForEach_Separator_IsAnAvt()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='sep' select='"--"'/>
                <out><xsl:for-each select='1 to 3' separator='{$sep}'><xsl:sequence select='.'/></xsl:for-each></out>
            </xsl:template>
            """);
        Assert.Contains("<out>1--2--3</out>", result);
    }

    [Fact]
    public void ForEach_Separator_InRawSequenceVariable()
    {
        // Inside xsl:variable/@as content the separator is a string item of the raw
        // sequence: it breaks runs of adjacent atomics.
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='v' as='item()*'>
                    <xsl:for-each select='1 to 3' separator='|'>
                        <xsl:sequence select='., . + 10'/>
                    </xsl:for-each>
                </xsl:variable>
                <out><xsl:value-of select="string-join($v, ';')"/></out>
            </xsl:template>
            """);
        Assert.Contains("<out>1;11;|;2;12;|;3;13</out>", result);
    }

    [Fact]
    public void ForEach_Separator_WorksAtVersion_3_0()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:for-each select='1 to 3' separator='|'><xsl:sequence select='.'/></xsl:for-each></out>
            </xsl:template>
            """, version: "3.0");
        Assert.Contains("<out>1|2|3</out>", result);
    }

    // ------------------------------------------------------------------
    // separator on xsl:apply-templates (XSLT 4.0 §6.7)
    // ------------------------------------------------------------------

    [Fact]
    public void ApplyTemplates_Separator_BetweenItems()
    {
        const string source = "<root><item>a</item><item>b</item><item>c</item></root>";
        var result = RunTransform("""
            <xsl:template match='item'><xsl:value-of select='.'/></xsl:template>
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/root/item' separator=','/></out>
            </xsl:template>
            """, sourceXml: source);
        Assert.Contains("<out>a,b,c</out>", result);
    }

    [Fact]
    public void ApplyTemplates_Separator_WithSort()
    {
        const string source = "<root><n>3</n><n>1</n><n>2</n></root>";
        var result = RunTransform("""
            <xsl:template match='n'><xsl:value-of select='.'/></xsl:template>
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/root/n' separator='-'>
                    <xsl:sort select='number(.)'/>
                </xsl:apply-templates></out>
            </xsl:template>
            """, sourceXml: source);
        Assert.Contains("<out>1-2-3</out>", result);
    }

    [Fact]
    public void ApplyTemplates_Separator_WorksAtVersion_3_0()
    {
        const string source = "<root><item>a</item><item>b</item></root>";
        var result = RunTransform("""
            <xsl:template match='item'><xsl:value-of select='.'/></xsl:template>
            <xsl:template match='/'>
                <out><xsl:apply-templates select='/root/item' separator=';'/></out>
            </xsl:template>
            """, version: "3.0", sourceXml: source);
        Assert.Contains("<out>a;b</out>", result);
    }

    // ------------------------------------------------------------------
    // xsl:map select/duplicates (XSLT 4.0 §21.1.1)
    // ------------------------------------------------------------------

    [Fact]
    public void Map_Select_MergesInputMaps()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map select="(map{'a': 1}, map{'b': 2})"/>
                </xsl:variable>
                <out a='{$m("a")}' b='{$m("b")}' size='{map:size($m)}'/>
            </xsl:template>
            """);
        Assert.Contains("a=\"1\"", result);
        Assert.Contains("b=\"2\"", result);
        Assert.Contains("size=\"2\"", result);
    }

    [Fact]
    public void Map_Select_DuplicatesDefault_XTDE3365()
    {
        var ex = TransformError("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map select="(map{'a': 1}, map{'a': 2})"/>
                </xsl:variable>
                <out/>
            </xsl:template>
            """);
        Assert.Contains("XTDE3365", ex.Message);
    }

    [Fact]
    public void Map_Select_DuplicatesUseFirst()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map select="(map{'a': 1}, map{'a': 2})" duplicates="'use-first'"/>
                </xsl:variable>
                <out a='{$m("a")}'/>
            </xsl:template>
            """);
        Assert.Contains("a=\"1\"", result);
    }

    [Fact]
    public void Map_Select_DuplicatesUseLast()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map select="(map{'a': 1}, map{'a': 2})" duplicates="'use-last'"/>
                </xsl:variable>
                <out a='{$m("a")}'/>
            </xsl:template>
            """);
        Assert.Contains("a=\"2\"", result);
    }

    [Fact]
    public void Map_Select_DuplicatesCombine()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map select="(map{'a': 1}, map{'a': (2, 3)})" duplicates="'combine'"/>
                </xsl:variable>
                <out a='{string-join($m("a"), ";")}'/>
            </xsl:template>
            """);
        Assert.Contains("a=\"1;2;3\"", result);
    }

    [Fact]
    public void Map_Select_DuplicatesReject_FOJS0003()
    {
        var ex = TransformError("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map select="(map{'a': 1}, map{'a': 2})" duplicates="'reject'"/>
                </xsl:variable>
                <out/>
            </xsl:template>
            """);
        Assert.Contains("FOJS0003", ex.Message);
    }

    [Fact]
    public void Map_Select_DuplicatesInvalidString_FOJS0005()
    {
        var ex = TransformError("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map select="(map{'a': 1}, map{'a': 2})" duplicates="'whatever'"/>
                </xsl:variable>
                <out/>
            </xsl:template>
            """);
        Assert.Contains("FOJS0005", ex.Message);
    }

    [Fact]
    public void Map_Select_DuplicatesFunctionItem()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map select="(map{'a': 1}, map{'a': 2}, map{'a': 3})"
                             duplicates='function($x, $y) { $x + $y }'/>
                </xsl:variable>
                <out a='{$m("a")}'/>
            </xsl:template>
            """);
        Assert.Contains("a=\"6\"", result);
    }

    [Fact]
    public void Map_Select_NonMapInput_XTTE3375()
    {
        var ex = TransformError("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map select="(map{'a': 1}, 42)"/>
                </xsl:variable>
                <out/>
            </xsl:template>
            """);
        Assert.Contains("XTTE3375", ex.Message);
    }

    [Fact]
    public void Map_SelectPlusContent_XTSE3185()
    {
        var ex = CompileError("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map select="map{'a': 1}"><xsl:map-entry key="'b'" select='2'/></xsl:map>
                </xsl:variable>
                <out/>
            </xsl:template>
            """);
        Assert.Contains("XTSE3185", ex.Message);
    }

    [Fact]
    public void Map_DuplicatesWithContent_UseLast()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map duplicates="'use-last'">
                        <xsl:map-entry key="'a'" select='1'/>
                        <xsl:map-entry key="'a'" select='2'/>
                    </xsl:map>
                </xsl:variable>
                <out a='{$m("a")}'/>
            </xsl:template>
            """);
        Assert.Contains("a=\"2\"", result);
    }

    [Fact]
    public void Map_ContentDuplicateDefault_StillXTDE3365()
    {
        var ex = TransformError("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map>
                        <xsl:map-entry key="'a'" select='1'/>
                        <xsl:map-entry key="'a'" select='2'/>
                    </xsl:map>
                </xsl:variable>
                <out/>
            </xsl:template>
            """);
        Assert.Contains("XTDE3365", ex.Message);
    }

    [Fact]
    public void Map_Select_WorksAtVersion_3_0()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map select="(map{'a': 1}, map{'a': 2})" duplicates="'use-last'"/>
                </xsl:variable>
                <out a='{$m("a")}'/>
            </xsl:template>
            """, version: "3.0");
        Assert.Contains("a=\"2\"", result);
    }

    // ------------------------------------------------------------------
    // xsl:map-entry first-class promotion
    // ------------------------------------------------------------------

    [Fact]
    public void MapEntry_StandaloneInVariable_AsSingleEntryMap()
    {
        // xsl:map-entry is usable anywhere an instruction is expected: here it is the
        // sole content of a variable, producing a single-entry map.
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map-entry key="'k'" select="'v'"/>
                </xsl:variable>
                <out k='{$m("k")}' size='{map:size($m)}'/>
            </xsl:template>
            """);
        Assert.Contains("k=\"v\"", result);
        Assert.Contains("size=\"1\"", result);
    }

    [Fact]
    public void MapEntry_SelectPlusContent_StillXTSE3280()
    {
        // XSLT 3.0 behavior pinned by xt3 maps-008/error-3280a: kept even though the
        // 4.0 draft re-labels this XTSE3185 (conformance catalog asserts XTSE3280).
        var ex = TransformError("""
            <xsl:template match='/'>
                <xsl:variable name='m' as='map(*)'>
                    <xsl:map>
                        <xsl:map-entry key="'a'" select='1'><xsl:map-entry key="'b'" select='2'/></xsl:map-entry>
                    </xsl:map>
                </xsl:variable>
                <out/>
            </xsl:template>
            """);
        Assert.Contains("XTSE3280", ex.Message);
    }

    // ------------------------------------------------------------------
    // Version declaration behavior
    // ------------------------------------------------------------------

    [Fact]
    public void Version_4_0_IsFullySupported_UnknownInstructionErrors()
    {
        // With 4.0 as the supported version, a 4.0 stylesheet is no longer
        // forwards-compatible: an unknown XSLT instruction is XTSE0010 again
        // (but note xsl:note itself is never an error).
        var ex = CompileError("""
            <xsl:template match='/'>
                <xsl:switch-new-invented/>
            </xsl:template>
            """, version: "4.0");
        Assert.Contains("XTSE0010", ex.Message);
    }

    [Fact]
    public void Version_5_0_RemainsForwardsCompatible()
    {
        // Above the supported version, forwards-compatible processing applies:
        // unknown elements are ignored (their xsl:fallback evaluated).
        var result = RunTransform("""
            <xsl:template match='/'>
                <out>
                    <xsl:futuristic-thing>
                        <xsl:fallback>fb</xsl:fallback>
                    </xsl:futuristic-thing>
                </out>
            </xsl:template>
            """, version: "5.0");
        Assert.Contains("fb", result);
    }

    [Fact]
    public void Note_WorksInForwardsCompatible_5_0()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:note>docs</xsl:note>
                <out>ok</out>
            </xsl:template>
            """, version: "5.0");
        Assert.Contains("<out>ok</out>", result);
    }
}
