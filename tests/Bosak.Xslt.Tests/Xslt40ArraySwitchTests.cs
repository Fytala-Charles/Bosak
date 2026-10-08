// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 October 2026
// PURPOSE              : Unit tests for the XSLT 4.0 xsl:array / xsl:array-member and xsl:switch surfaces of REQ-118 slice 4.0-S8
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
/// Tests the XSLT 4.0 surfaces of REQ-118 slice 4.0-S8: <c>xsl:array</c> and
/// <c>xsl:array-member</c> construction (§21.1.2) and <c>xsl:switch</c> with
/// <c>select</c>-valued <c>xsl:when</c>/<c>xsl:otherwise</c> branches (§8.3).
/// </summary>
/// <remarks>
/// Per XSLT 4.0 draft §3.8.2/§3.8.3 these surfaces are not gated to version="4.0"
/// stylesheets: the tests pin that they work at version="3.0", and that a
/// version="4.0" stylesheet (processed in forwards-compatible mode, since the
/// supported-version ceiling stays 3.0) still executes them because they are
/// known XSLT elements.
/// </remarks>
public class Xslt40ArraySwitchTests
{
    private const string XslNs = "http://www.w3.org/1999/XSL/Transform";
    private const string MapNs = "http://www.w3.org/2005/xpath-functions/map";
    private const string ArrayNs = "http://www.w3.org/2005/xpath-functions/array";
    private const string HtmlCollation = "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive";

    private static string Wrap(string inner, string version, string? decls = null, string? rootAttrs = null)
        => $"<xsl:stylesheet version='{version}' xmlns:xsl='{XslNs}' xmlns:map='{MapNs}' xmlns:array='{ArrayNs}'" +
           " exclude-result-prefixes='map array'" +
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
    // xsl:array (XSLT 4.0 §21.1.2)
    // ------------------------------------------------------------------

    [Fact]
    public void Array_Select_OneSingletonMemberPerItem()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='a' as='array(*)'>
                    <xsl:array select="('a', 'b', 'c')"/>
                </xsl:variable>
                <out size='{array:size($a)}' m1='{array:get($a, 1)}' m3='{array:get($a, 3)}'/>
            </xsl:template>
            """);
        Assert.Contains("size=\"3\"", result);
        Assert.Contains("m1=\"a\"", result);
        Assert.Contains("m3=\"c\"", result);
    }

    [Fact]
    public void Array_Select_TokenizedString()
    {
        // Spec example: each item of the selected sequence becomes one member.
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='a' as='array(*)'>
                    <xsl:array select="tokenize('alpha,beta,gamma', ',')"/>
                </xsl:variable>
                <out size='{array:size($a)}' m2='{array:get($a, 2)}'/>
            </xsl:template>
            """);
        Assert.Contains("size=\"3\"", result);
        Assert.Contains("m2=\"beta\"", result);
    }

    [Fact]
    public void Array_ForEachSelect_MemberIsWholeSequence()
    {
        // With @for-each, one member is appended per focus item and the member is
        // the whole sequence obtained from @select evaluated with that focus.
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='a' as='array(*)'>
                    <xsl:array for-each='1 to 3' select='., . * 10'/>
                </xsl:variable>
                <out size='{array:size($a)}'
                     m1='{string-join(array:get($a, 1), ";")}'
                     m3='{string-join(array:get($a, 3), ";")}'/>
            </xsl:template>
            """);
        Assert.Contains("size=\"3\"", result);
        Assert.Contains("m1=\"1;10\"", result);
        Assert.Contains("m3=\"3;30\"", result);
    }

    [Fact]
    public void Array_Content_ArrayMemberSiblings()
    {
        // xsl:array-member children each contribute exactly one (possibly
        // multi-item) member; ordinary content contributes one member per item.
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='a' as='array(*)'>
                    <xsl:array>
                        <xsl:array-member select='1, 2'/>
                        <xsl:array-member select='3'/>
                        <xsl:sequence select='4, 5'/>
                    </xsl:array>
                </xsl:variable>
                <out size='{array:size($a)}'
                     m1='{string-join(array:get($a, 1), ";")}'
                     m2='{array:get($a, 2)}'
                     m3='{array:get($a, 3)}'
                     m4='{array:get($a, 4)}'/>
            </xsl:template>
            """);
        Assert.Contains("size=\"4\"", result);
        Assert.Contains("m1=\"1;2\"", result);
        Assert.Contains("m2=\"3\"", result);
        Assert.Contains("m3=\"4\"", result);
        Assert.Contains("m4=\"5\"", result);
    }

    [Fact]
    public void Array_Content_ForEachGroup_ArrayMemberSelect()
    {
        // Spec §21.1.2 grouping example (adapted to group-by; the draft uses the
        // 4.0 split-when grouping key): each current-group() becomes ONE member.
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='a' as='array(*)'>
                    <xsl:array>
                        <xsl:for-each-group select='1 to 10' group-by='(. - 1) idiv 3'>
                            <xsl:array-member select='current-group()'/>
                        </xsl:for-each-group>
                    </xsl:array>
                </xsl:variable>
                <out size='{array:size($a)}'
                     m1='{string-join(array:get($a, 1), ";")}'
                     m4='{string-join(array:get($a, 4), ";")}'/>
            </xsl:template>
            """);
        Assert.Contains("size=\"4\"", result);
        Assert.Contains("m1=\"1;2;3\"", result);
        Assert.Contains("m4=\"10\"", result);
    }

    [Fact]
    public void Array_Content_NestedArrayMemberInForEach()
    {
        // A nested xsl:array-member inside xsl:for-each content wraps its sequence
        // into one member per iteration.
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='a' as='array(*)'>
                    <xsl:array>
                        <xsl:for-each select='1 to 2'>
                            <xsl:array-member select='., . * 10'/>
                        </xsl:for-each>
                    </xsl:array>
                </xsl:variable>
                <out size='{array:size($a)}' m2='{string-join(array:get($a, 2), ";")}'/>
            </xsl:template>
            """);
        Assert.Contains("size=\"2\"", result);
        Assert.Contains("m2=\"2;20\"", result);
    }

    [Fact]
    public void Array_InFunctionBody()
    {
        var result = RunTransform("""
            <xsl:function name='f:make' as='array(*)'>
                <xsl:array select="('x', 'y')"/>
            </xsl:function>
            <xsl:template match='/'>
                <xsl:variable name='a' select='f:make()'/>
                <out size='{array:size($a)}' m2='{array:get($a, 2)}'/>
            </xsl:template>
            """, decls: null, rootAttrs: "xmlns:f='urn:f'");
        Assert.Contains("size=\"2\"", result);
        Assert.Contains("m2=\"y\"", result);
    }

    [Fact]
    public void Array_SelectPlusContent_XTSE3185()
    {
        var ex = CompileError("""
            <xsl:template match='/'>
                <xsl:variable name='a' as='array(*)'>
                    <xsl:array select='(1, 2)'><xsl:array-member select='3'/></xsl:array>
                </xsl:variable>
                <out/>
            </xsl:template>
            """);
        Assert.Contains("XTSE3185", ex.Message);
    }

    [Fact]
    public void ArrayMember_SelectPlusContent_XTSE3185()
    {
        var ex = CompileError("""
            <xsl:template match='/'>
                <xsl:variable name='a' as='array(*)'>
                    <xsl:array>
                        <xsl:array-member select='1'><xsl:sequence select='2'/></xsl:array-member>
                    </xsl:array>
                </xsl:variable>
                <out/>
            </xsl:template>
            """);
        Assert.Contains("XTSE3185", ex.Message);
    }

    [Fact]
    public void Array_AsElementChild_XTDE0450()
    {
        var ex = TransformError("""
            <xsl:template match='/'>
                <out><xsl:array select='(1, 2)'/></out>
            </xsl:template>
            """);
        Assert.Contains("XTDE0450", ex.Message);
    }

    [Fact]
    public void Array_WorksAtVersion_3_0()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='a' as='array(*)'>
                    <xsl:array select="('p', 'q')"/>
                </xsl:variable>
                <out size='{array:size($a)}'/>
            </xsl:template>
            """, version: "3.0");
        Assert.Contains("size=\"2\"", result);
    }

    [Fact]
    public void Array_WorksInForwardsCompatible_4_0()
    {
        // version="4.0" is above the supported-version ceiling, so the stylesheet is
        // forwards-compatible — but xsl:array is a KNOWN element, so it executes
        // instead of being skipped.
        var result = RunTransform("""
            <xsl:template match='/'>
                <xsl:variable name='a' as='array(*)'>
                    <xsl:array select="('u', 'v', 'w')"/>
                </xsl:variable>
                <out size='{array:size($a)}'/>
            </xsl:template>
            """, version: "4.0");
        Assert.Contains("size=\"3\"", result);
    }

    // ------------------------------------------------------------------
    // xsl:switch (XSLT 4.0 §8.3)
    // ------------------------------------------------------------------

    [Fact]
    public void Switch_LookupTable_SelectBranches()
    {
        // xsl:when / xsl:otherwise under xsl:switch may carry a select attribute
        // evaluated in place of the sequence constructor.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:switch select='2'>
                    <xsl:when test='1' select="'January'"/>
                    <xsl:when test='2' select="'February'"/>
                    <xsl:when test='3' select="'March'"/>
                    <xsl:otherwise select="'other'"/>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("<out>February</out>", result);
    }

    [Fact]
    public void Switch_TestSequences_DaysInMonth()
    {
        // Spec example: a test expression may yield a sequence of atomic items; the
        // branch matches when ANY item equals the selector under general comparison.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:switch select='/root/month'>
                    <xsl:when test='2' select='28'/>
                    <xsl:when test='(4, 6, 9, 11)' select='30'/>
                    <xsl:otherwise select='31'/>
                </xsl:switch></out>
            </xsl:template>
            """, sourceXml: "<root><month>9</month></root>");
        Assert.Contains("<out>30</out>", result);
    }

    [Fact]
    public void Switch_GeneralComparison_NumericPromotion()
    {
        // Branch matching uses XPath '=' semantics: the double 1.0 matches the
        // integer test value 1.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:switch select='1.0'>
                    <xsl:when test='1' select="'one'"/>
                    <xsl:otherwise select="'other'"/>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("<out>one</out>", result);
    }

    [Fact]
    public void Switch_UsesDefaultCollationInScope()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:switch select='"RED"'>
                    <xsl:when test='"red"' select="'matched'"/>
                    <xsl:otherwise select="'other'"/>
                </xsl:switch></out>
            </xsl:template>
            """, rootAttrs: $"default-collation='{HtmlCollation}'");
        Assert.Contains("<out>matched</out>", result);
    }

    [Fact]
    public void Switch_LaterBranchesNotEvaluated()
    {
        // Branch selection is lazy: tests and branch bodies after the match are
        // never evaluated.
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:switch select='1'>
                    <xsl:when test='1' select="'first'"/>
                    <xsl:when test='error()' select="'unreachable'"/>
                    <xsl:when test='2' select='error()'/>
                    <xsl:otherwise select='error()'/>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("<out>first</out>", result);
    }

    [Fact]
    public void Switch_NoOtherwiseNoMatch_Empty()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:switch select='99'>
                    <xsl:when test='1' select="'one'"/>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("<out/>", result);
    }

    [Fact]
    public void Switch_ContentBranches()
    {
        // Without @select, the matched branch evaluates its sequence-constructor
        // content (the xsl:choose form of branch bodies is unchanged).
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:switch select='2'>
                    <xsl:when test='1'><a/></xsl:when>
                    <xsl:when test='2'><b/></xsl:when>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("<b/>", result);
    }

    [Fact]
    public void Switch_OtherwiseSelect()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:switch select='7'>
                    <xsl:when test='1' select="'one'"/>
                    <xsl:otherwise select="'fallback'"/>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("<out>fallback</out>", result);
    }

    [Fact]
    public void Switch_SelectorNotSingleAtomic_XPTY0004()
    {
        var ex = TransformError("""
            <xsl:template match='/'>
                <out><xsl:switch select='(1, 2)'>
                    <xsl:when test='1' select="'one'"/>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Switch_WhenSelectPlusContent_XTSE3185()
    {
        var ex = CompileError("""
            <xsl:template match='/'>
                <out><xsl:switch select='1'>
                    <xsl:when test='1' select="'one'"><a/></xsl:when>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("XTSE3185", ex.Message);
    }

    [Fact]
    public void Switch_OtherwiseSelectPlusContent_XTSE3185()
    {
        var ex = CompileError("""
            <xsl:template match='/'>
                <out><xsl:switch select='9'>
                    <xsl:when test='1' select="'one'"/>
                    <xsl:otherwise select="'other'"><a/></xsl:otherwise>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("XTSE3185", ex.Message);
    }

    [Fact]
    public void Switch_RequiresSelect_XTSE0010()
    {
        var ex = CompileError("""
            <xsl:template match='/'>
                <out><xsl:switch>
                    <xsl:when test='1' select="'one'"/>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("XTSE0010", ex.Message);
    }

    [Fact]
    public void Switch_RequiresAtLeastOneWhen_XTSE0010()
    {
        var ex = CompileError("""
            <xsl:template match='/'>
                <out><xsl:switch select='1'>
                    <xsl:otherwise select="'other'"/>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("XTSE0010", ex.Message);
    }

    [Fact]
    public void Switch_RejectsNonBranchChildren_XTSE0010()
    {
        var ex = CompileError("""
            <xsl:template match='/'>
                <out><xsl:switch select='1'>
                    <xsl:when test='1' select="'one'"/>
                    <a/>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("XTSE0010", ex.Message);
    }

    [Fact]
    public void Switch_WhenAfterOtherwise_XTSE0010()
    {
        var ex = CompileError("""
            <xsl:template match='/'>
                <out><xsl:switch select='1'>
                    <xsl:when test='1' select="'one'"/>
                    <xsl:otherwise select="'other'"/>
                    <xsl:when test='2' select="'two'"/>
                </xsl:switch></out>
            </xsl:template>
            """);
        Assert.Contains("XTSE0010", ex.Message);
    }

    [Fact]
    public void Switch_ChooseWhenSelect_IsIgnoredByChoose()
    {
        // @select on xsl:when is only honored under xsl:switch: inside xsl:choose
        // the attribute is inert and the content form applies (4.0 behavior is
        // identical to 3.0 for xsl:choose).
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:choose>
                    <xsl:when test='2 > 1' select="'ignored'"><b>content</b></xsl:when>
                    <xsl:otherwise select="'other'"/>
                </xsl:choose></out>
            </xsl:template>
            """);
        Assert.Contains("<b>content</b>", result);
        Assert.DoesNotContain("ignored", result);
    }

    [Fact]
    public void Switch_InFunctionBody()
    {
        var result = RunTransform("""
            <xsl:function name='f:class' as='xs:string'>
                <xsl:param name='n' as='xs:integer'/>
                <xsl:switch select='$n'>
                    <xsl:when test='0' select="'zero'"/>
                    <xsl:when test='(2, 4, 6, 8)' select="'even'"/>
                    <xsl:otherwise select="'odd'"/>
                </xsl:switch>
            </xsl:function>
            <xsl:template match='/'>
                <out e='{f:class(4)}' o='{f:class(7)}'/>
            </xsl:template>
            """, rootAttrs: "xmlns:f='urn:f' xmlns:xs='http://www.w3.org/2001/XMLSchema'");
        Assert.Contains("e=\"even\"", result);
        Assert.Contains("o=\"odd\"", result);
    }

    [Fact]
    public void Switch_WorksAtVersion_3_0()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:switch select='"b"'>
                    <xsl:when test='"a"' select="'A'"/>
                    <xsl:when test='"b"' select="'B'"/>
                </xsl:switch></out>
            </xsl:template>
            """, version: "3.0");
        Assert.Contains("<out>B</out>", result);
    }

    [Fact]
    public void Switch_WorksInForwardsCompatible_4_0()
    {
        var result = RunTransform("""
            <xsl:template match='/'>
                <out><xsl:switch select='"y"'>
                    <xsl:when test='"x"' select="'X'"/>
                    <xsl:otherwise select="'Y'"/>
                </xsl:switch></out>
            </xsl:template>
            """, version: "4.0");
        Assert.Contains("<out>Y</out>", result);
    }
}
