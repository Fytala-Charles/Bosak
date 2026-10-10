// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 oktober 2026
// PURPOSE              : Unit tests verifying correctness of the underlying implementation.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-123 JNode cluster, F&O 4.0 §17.7): fn:jtree/fn:jkey/fn:jvalue, jnode()     |
//                      |                    |       |                | type tests, JNode navigation, coercion, serialization                                    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>
/// Tests for the XPath 4.0 §17.7 JNode model: fn:jtree/fn:jkey/fn:jvalue, the
/// jnode() type syntax, and JNode-producing path navigation (implicit fn:jtree
/// wrapping of maps/arrays, child/parent/descendant axes, key selectors).
/// Semantics pinned against qt4tests fn/jtree.xml, fn/jkey.xml, fn/jvalue.xml.
/// </summary>
public class JNodeTests
{
    private static XdmValue Eval40(string xpath)
    {
        var expr = XPath31Expression.Compile(xpath, new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        return expr.Evaluate(new EvaluationContext());
    }

    private static string[] Seq40(string xpath)
    {
        var result = Eval40(xpath);
        if (result.IsUndefined)
            return [];
        if (!result.IsSequence)
            return [result.ToString()!];
        var list = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            list.Add(item.ToString());
        return list.ToArray();
    }

    private static bool Bool40(string xpath) => Seq40(xpath)[0] == "true";

    private static string One(string xpath)
    {
        var items = Seq40(xpath);
        Assert.Single(items);
        return items[0];
    }

    // ------------------------------------------------------------------
    // fn:jtree
    // ------------------------------------------------------------------

    [Fact]
    public void JTree_Array_UnwrapsToArray()
    {
        Assert.Equal("[1,2,3]", One("fn:serialize(fn:jtree([1,2,3]) => fn:jvalue(), map{'method':'json'})"));
    }

    [Fact]
    public void JTree_Map_UnwrapsToMap()
    {
        Assert.Equal("{\"a\":1,\"b\":2}", One("fn:serialize(fn:jtree({'a':1,'b':2}) => fn:jvalue(), map{'method':'json'})"));
    }

    [Fact]
    public void JTree_EmptySequence_RootJNodeWithEmptyValue()
    {
        Assert.Empty(Seq40("fn:jtree(()) => fn:jvalue()"));
        Assert.Empty(Seq40("fn:jtree(()) => fn:jkey()"));
    }

    [Fact]
    public void JTree_JNodeInput_WrapsSecondTime()
    {
        Assert.Equal("[1,2,3]", One("fn:serialize(fn:jtree(fn:jtree([1,2,3])) => fn:jvalue() => fn:jvalue(), map{'method':'json'})"));
    }

    [Fact]
    public void JTree_Atomic_PassesThrough()
    {
        Assert.Equal("22", One("fn:jtree(22) => fn:jvalue()"));
    }

    [Fact]
    public void JTree_FunctionItem_PassesThrough()
    {
        Assert.Equal("42", One("fn:jtree(fn:abs#1) => fn:jvalue() => apply([-42])"));
    }

    // ------------------------------------------------------------------
    // jnode() type tests
    // ------------------------------------------------------------------

    [Fact]
    public void InstanceOf_TwoArg_Array()
    {
        Assert.True(Bool40("fn:jtree([1,2,3]) instance of jnode(*, array(xs:integer))"));
    }

    [Fact]
    public void InstanceOf_TwoArg_Map()
    {
        Assert.True(Bool40("fn:jtree({'a':1,'b':2}) instance of jnode(*, map(xs:string, xs:integer))"));
    }

    [Fact]
    public void InstanceOf_Bare_MatchesAnyJNode()
    {
        Assert.True(Bool40("fn:jtree([1]) instance of jnode()"));
        Assert.True(Bool40("fn:jtree({'a':1}) instance of jnode()"));
        Assert.False(Bool40("fn:jtree([1]) instance of jnode(\"z\", array(*))"));
    }

    [Fact]
    public void InstanceOf_RootForm_MatchesKeylessRoot()
    {
        Assert.True(Bool40("fn:jtree([1]) instance of jnode((), array(*))"));
        Assert.False(Bool40("fn:jtree([1]) / * instance of jnode(())"));
    }

    [Fact]
    public void InstanceOf_KeySpecifier_MatchesEntryKey()
    {
        Assert.True(Bool40("fn:jtree({'a':1}) / child::a instance of jnode('a', xs:integer)"));
        Assert.True(Bool40("fn:jtree([9]) / * instance of jnode(1, xs:integer)"));
    }

    // ------------------------------------------------------------------
    // fn:jkey
    // ------------------------------------------------------------------

    [Fact]
    public void JKey_RootIsEmpty()
    {
        Assert.Empty(Seq40("fn:jtree({'a':1,'b':2}) => fn:jkey()"));
    }

    [Fact]
    public void JKey_ArrayMember_IsPosition()
    {
        Assert.Equal("1", One("fn:jtree([12]) / child::* => fn:jkey()"));
    }

    [Fact]
    public void JKey_MapEntry_IsKey()
    {
        Assert.Equal("a", One("fn:jtree({'a':1}) / child::* => fn:jkey()"));
    }

    [Fact]
    public void JKey_QNameKey_ReturnsQName()
    {
        Assert.Equal("xml:space", One("{#xml:space:23, #xml:id:24} / child::xml:space => fn:jkey()"));
    }

    [Fact]
    public void JKey_SequenceChildren_KeyedByPosition()
    {
        Assert.Equal("1, 2", string.Join(", ", Seq40("fn:jtree((1,'a'))/* ! fn:jkey()")));
    }

    [Fact]
    public void JKey_TwoItems_Xpty0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("(fn:jtree(1), fn:jtree(2)) => fn:jkey()"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void JKey_NonJNode_Xpty0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("fn:jkey(42)"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void JKey_NoContext_Xpdy0002()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("fn() { fn:jkey() }()"));
        Assert.Contains("XPDY0002", ex.Message);
    }

    // ------------------------------------------------------------------
    // fn:jvalue
    // ------------------------------------------------------------------

    [Fact]
    public void JValue_NonJNode_Xpty0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("fn:jvalue(42)"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void JValue_EmptyInput_EmptySequence()
    {
        Assert.Empty(Seq40("fn:jvalue(())"));
    }

    // ------------------------------------------------------------------
    // Coercion: atomic/map-array-required contexts extract the jvalue
    // ------------------------------------------------------------------

    [Fact]
    public void Coerce_GeneralComparison_ExtractsJValue()
    {
        Assert.True(Bool40("fn:jtree([22]) / *[1] gt 21"));
    }

    [Fact]
    public void Coerce_Arithmetic_ExtractsJValue()
    {
        Assert.Equal("14", One("fn:jtree([12]) / * + 2"));
    }

    [Fact]
    public void Coerce_MapArrayParams_ExtractJValue()
    {
        Assert.Equal("2", One("fn:jtree({'a':1,'b':2}) => map:size()"));
        Assert.Equal("3", One("fn:jtree([1,2,3]) => array:size()"));
    }

    [Fact]
    public void Coerce_String_ExtractsJValue()
    {
        Assert.Equal("hello", One("fn:string(fn:jtree('hello'))"));
    }

    [Fact]
    public void Coerce_Data_ExtractsJValue()
    {
        Assert.Equal("42", One("fn:string(fn:data(fn:jtree(42)))"));
    }

    [Fact]
    public void Coerce_Ebv_JNodeIsTrue()
    {
        Assert.Equal("truthy", One("if (fn:jtree(1)) then 'truthy' else 'falsy'"));
    }

    // ------------------------------------------------------------------
    // Navigation
    // ------------------------------------------------------------------

    [Fact]
    public void Nav_ChildOfArray_PositionKeyedJNodes()
    {
        Assert.Equal("12", One("fn:jtree([12]) / child::* => fn:jvalue()"));
        Assert.Equal("1", One("fn:jtree([12]) / child::* => fn:jkey()"));
    }

    [Fact]
    public void Nav_ParentRoundTrip_ReturnsMapJNode()
    {
        Assert.Equal("{\"a\":1}", One("fn:serialize(fn:jtree({'a':1}) / child::* / parent::* => fn:jvalue(), map{'method':'json'})"));
    }

    [Fact]
    public void Nav_RootParent_IsEmpty()
    {
        Assert.Empty(Seq40("fn:jtree({}) / parent::* => fn:jvalue()"));
    }

    [Fact]
    public void Nav_RawMap_ImplicitWrap()
    {
        Assert.Equal("b", One("{'a':23,'b':24} / child::b => fn:jkey()"));
        Assert.Equal("24", One("{'a':23,'b':24} / child::b => fn:jvalue()"));
    }

    [Fact]
    public void Nav_DeepNameTest_FindsNestedKey()
    {
        Assert.Equal("42", One("{'x':{'y':42}} // y => fn:jvalue()"));
    }

    [Fact]
    public void Nav_SequenceValue_ChildrenKeyedByPosition()
    {
        Assert.Equal("1, 2", string.Join(", ", Seq40(
            "{'a':23,'b':([1,2,3],[4,5,6])} / b / * ! fn:serialize(array{ fn:jkey() })")));
    }

    [Fact]
    public void Nav_StringLiteralLookupStep_SelectsEntry()
    {
        Assert.Equal("24", One("{'a':23,'b':24}/\"b\" => fn:jvalue()"));
        Assert.Equal("7", One("map{'a':map{'b':7}} / \"a\" / \"b\" => fn:jvalue()"));
    }

    [Fact]
    public void Nav_IntegerLookupStep_SelectsPositionOrIntKey()
    {
        Assert.Equal("5", One("[4,5,6] / 2 => fn:jvalue()"));
        Assert.Equal("2", One("{1:'a',2:'b'} / 2 => fn:jkey()"));
    }

    [Fact]
    public void Nav_BracedKeySelector_EvaluatesKeyExpression()
    {
        Assert.Equal("2", One("[1,2,3] / child::{2} => fn:jkey()"));
        Assert.Equal("3", One("[1,2,3] / child::{1+2} => fn:jvalue()"));
    }

    [Fact]
    public void Nav_WildcardWithPredicate_WorksOnMapSteps()
    {
        Assert.Equal("20", One("[2,4,{'a':20}] / *[3] / a ! fn:jvalue()"));
    }

    [Fact]
    public void Nav_AttributeAxisOnJNode_IsEmpty()
    {
        Assert.Empty(Seq40("fn:jtree({'a':1}) /@* => fn:jvalue()"));
    }

    // ------------------------------------------------------------------
    // Serialization
    // ------------------------------------------------------------------

    [Fact]
    public void Serialize_Json_SerializesJValue()
    {
        Assert.Equal("[1,2,3]", One("fn:serialize(fn:jtree([1,2,3]), map{'method':'json'})"));
    }

    [Fact]
    public void Serialize_Adaptive_SerializesJValue()
    {
        Assert.Equal("1 2 3", One("fn:serialize(fn:jtree([1,2,3]))"));
    }

    [Fact]
    public void DeepEqual_JNodes_CompareByValue()
    {
        Assert.True(Bool40("fn:deep-equal(fn:jtree([1,2,3]), fn:jtree([1,2,3]))"));
    }
}
