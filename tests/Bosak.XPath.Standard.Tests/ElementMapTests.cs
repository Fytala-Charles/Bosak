// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 oktober 2026
// PURPOSE              : Unit tests for the REQ-123 XPath 4.0 element-to-map slice (F&O 4.0 §17.6).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-10-2026     | Creation: fn:element-to-map (layouts, attribute/content keys, plan typing, numeric       |
//                      |                  |       |                | promotion), fn:map-to-element, fn:element-to-map-plan, fn:jvalue, and PR2688 map path    |
//                      |                  |       |                | navigation (E/"key", E//"key", E//QName)                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

public class ElementMapTests
{
    private static XdmValue Eval40(string xpath)
    {
        var expr = XPath31Expression.Compile(xpath, new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        return expr.Evaluate(new EvaluationContext());
    }

    // ------------------------------------------------------------------
    // fn:element-to-map — happy paths
    // ------------------------------------------------------------------

    [Fact]
    public void ElementToMap_SimpleElement_ContentIsValue()
    {
        Assert.Equal("x", Eval40("element-to-map(parse-xml('<a>x</a>')/a)?a").ToString());
    }

    [Fact]
    public void ElementToMap_AttributeAndContent_KeysUseAtAndHashPrefixes()
    {
        Assert.Equal("@id,#content",
            Eval40("string-join(map:keys(element-to-map(parse-xml('<a id=\"7\">x</a>')/a)?a), ',')").ToString());
        Assert.Equal("7",
            Eval40("element-to-map(parse-xml('<a id=\"7\">x</a>')/a)?a?\"@id\"").ToString());
        Assert.Equal("x",
            Eval40("element-to-map(parse-xml('<a id=\"7\">x</a>')/a)?a?\"#content\"").ToString());
    }

    [Fact]
    public void ElementToMap_NestedElements_RecordLayoutValuesDirectly()
    {
        // <a><b>1</b><c>2</c></a> — record layout: each child name maps directly
        // to the child's converted value.
        Assert.Equal("1",
            Eval40("element-to-map(parse-xml('<a><b>1</b><c>2</c></a>')/a)?a?b").ToString());
        Assert.Equal("2",
            Eval40("element-to-map(parse-xml('<a><b>1</b><c>2</c></a>')/a)?a?c").ToString());
    }

    // ------------------------------------------------------------------
    // fn:element-to-map — PR2688 plan-driven typing
    // ------------------------------------------------------------------

    [Fact]
    public void ElementToMap_PlanInfersIntegerAttribute()
    {
        Assert.Equal("true,true",
            Eval40("""
                let $in := parse-xml('<a><b id="23"/><b id="-4"/></a>')
                return string-join(
                    for $x in element-to-map($in, {'plan': element-to-map-plan($in)})//"@id"
                    return string(jvalue($x) instance of xs:integer), ',')
                """).ToString());
    }

    [Fact]
    public void ElementToMap_PlanPromotesIntegerDecimalMixToDecimal()
    {
        // Numeric subtype promotion: a mix of integers and decimals infers xs:decimal
        // (element-to-map-552 ground truth).
        Assert.Equal("true,true",
            Eval40("""
                let $in := parse-xml('<a><b id="23.1"/><b id="-4"/></a>')
                return string-join(
                    for $x in element-to-map($in, {'plan': element-to-map-plan($in)})//"@id"
                    return string(jvalue($x) instance of xs:decimal), ',')
                """).ToString());
    }

    [Fact]
    public void ElementToMap_PlanPromotesExponentFormsToDouble()
    {
        Assert.Equal("true,true",
            Eval40("""
                let $in := parse-xml('<a><b id="1e0"/><b id="-4"/></a>')
                return string-join(
                    for $x in element-to-map($in, {'plan': element-to-map-plan($in)})//"@id"
                    return string(jvalue($x) instance of xs:double), ',')
                """).ToString());
    }

    [Fact]
    public void ElementToMap_PlanLeadingZeroAttributeStaysUntyped()
    {
        // "023" is not integer-lexical, so the union type falls back to untypedAtomic.
        Assert.Equal("false",
            Eval40("""
                let $in := parse-xml('<a><b id="023"/><b id="4"/></a>')
                let $m := element-to-map($in, {'plan': element-to-map-plan($in)})
                let $ids := $m//"@id"
                return string(jvalue($ids[1]) instance of xs:integer)
                """).ToString());
    }

    // ------------------------------------------------------------------
    // fn:element-to-map — edge cases / errors
    // ------------------------------------------------------------------

    [Fact]
    public void ElementToMap_EmptySequence_ReturnsEmpty()
    {
        var result = Eval40("element-to-map(())");
        Assert.True(result.IsUndefined);
    }

    [Fact]
    public void ElementToMap_AtomicInput_Xpty0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("element-to-map(42)"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void ElementToMap_InconsistentLayoutAgainstPlan_Fojs0008()
    {
        // Plan declares <b> as a simple (text-only) layout; a child element violates it.
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("""
            element-to-map(
                parse-xml('<a><b>1<c/></b></a>')/a,
                {'plan': map{'a': map{'layout': 'empty', 'child': 'b'},
                             'b': map{'layout': 'simple'}}})
            """));
        Assert.Contains("FOJS0008", ex.Message);
    }

    // ------------------------------------------------------------------
    // fn:map-to-element
    // ------------------------------------------------------------------

    [Fact]
    public void MapToElement_SingleEntryMap_BuildsElement()
    {
        Assert.Equal("<a id=\"7\"/>",
            Eval40("serialize(map-to-element(map{'a': map{'@id': '7'}}))").ToString());
    }

    [Fact]
    public void MapToElement_EmptySequence_ReturnsEmpty()
    {
        var result = Eval40("map-to-element(())");
        Assert.True(result.IsUndefined);
    }

    [Fact]
    public void MapToElement_NonMapInput_Xpty0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("map-to-element(42)"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    // ------------------------------------------------------------------
    // fn:element-to-map-plan
    // ------------------------------------------------------------------

    [Fact]
    public void ElementToMapPlan_RepeatedSameNameChildren_InfersListLayout()
    {
        // <a> holds two <b> children → list; <b> itself is text-only → simple.
        Assert.Equal("list",
            Eval40("element-to-map-plan(parse-xml('<a><b>1</b><b>2</b></a>')/a)?a?layout").ToString());
        Assert.Equal("simple",
            Eval40("element-to-map-plan(parse-xml('<a><b>1</b><b>2</b></a>')/a)?b?layout").ToString());
    }

    [Fact]
    public void ElementToMapPlan_UnionInfersChildName()
    {
        Assert.Equal("b",
            Eval40("element-to-map-plan(parse-xml('<a><b>1</b><b>2</b></a>')/a)?a?child").ToString());
    }

    // ------------------------------------------------------------------
    // PR2688 map path navigation (E/"key", E//"key", E//QName)
    // ------------------------------------------------------------------

    [Fact]
    public void MapNavigation_DescendantLookupStep()
    {
        Assert.Equal("3,4",
            Eval40("""let $m := map{'a': map{'@id': 3}, 'b': map{'@id': 4}} return string-join($m//"@id", ',')""").ToString());
    }

    [Fact]
    public void MapNavigation_ChildLookupStep()
    {
        Assert.Equal("1",
            Eval40("""let $m := map{'a': 1, 'b': 2} return string($m/"a")""").ToString());
    }

    [Fact]
    public void MapNavigation_MissingKey_YieldsEmpty()
    {
        var result = Eval40("""let $m := map{'a': 1} return $m/"z" """);
        Assert.True(result.IsUndefined);
    }

    [Fact]
    public void MapNavigation_LookupDescendsThroughArrays()
    {
        Assert.Equal("1,2",
            Eval40("""let $m := map{'a': [map{'@id': 1}, map{'@id': 2}]} return string-join($m//"@id", ',')""").ToString());
    }

    [Fact]
    public void MapNavigation_DescendantQNameStep_LooksUpKey()
    {
        Assert.Equal("3,4",
            Eval40("""let $m := map{'a': map{'id': 3}, 'b': map{'id': 4}} return string-join($m//id, ',')""").ToString());
    }

    // ------------------------------------------------------------------
    // Parser disambiguation guards: a string literal in FIRST step position
    // remains a primary expression (the PR2688 lookup step only applies after
    // "/" or "//"); function-argument strings must not become lookup steps.
    // ------------------------------------------------------------------

    [Fact]
    public void StringLiteral_FirstStepPosition_RemainsPrimaryExpression()
    {
        Assert.Equal("hello", Eval40("'hello'").ToString());
        Assert.Equal("3", Eval40("string-length('abc')").ToString());
    }

    [Fact]
    public void PathExpression_FunctionArgumentString_NotAlookupStep()
    {
        // Regression guard: parse-xml's argument is a plain string, not a key lookup;
        // the wrapped path must select the <b> element.
        Assert.Equal("1",
            Eval40("count((parse-xml('<a><b/></a>'))/a/b)").ToString());
    }
}
