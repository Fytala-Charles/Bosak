// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies array member JNode decomposition and reconstruction.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies array member JNode decomposition and reconstruction.</summary>
public class ArrayMemberFunctionTests
{
    private static XdmValue Eval(string expression, XPathCompatibility profile = XPathCompatibility.XPath40) =>
        XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile }).Evaluate(new EvaluationContext());

    /// <summary>Preserves member boundaries, empty values, nested arrays and JNodes.</summary>
    /// <param name="expression">A boolean assertion.</param>
    [Theory]
    [InlineData("empty(array:members([]))")]
    [InlineData("count(array:members([(),(1,2),[3]]))=3")]
    [InlineData("deep-equal(array:members([1,2]) ! jkey(.), (1,2))")]
    [InlineData("deep-equal(array:members([(),(1,2)]) ! count(jvalue(.)), (0,2))")]
    [InlineData("deep-equal(array:of-members(()), [])")]
    [InlineData("deep-equal(array:of-members((jtree(()),jtree((1,2)))), [(),(1,2)])")]
    [InlineData("let $a := [(),(1,2),[3],map{'x':4}] return deep-equal(array:of-members(array:members($a)), $a)")]
    [InlineData("deep-equal(array:of-members(reverse(array:members([1,2,3]))), [3,2,1])")]
    [InlineData("deep-equal(array:of-members((jtree(1),jtree(1))),[1,1])")]
    [InlineData("array:of-members(jtree(jtree(7)))(1) instance of jnode()")]
    [InlineData("exists(function-lookup(QName('http://www.w3.org/2005/xpath-functions/array','members'),1))")]
    [InlineData("function-lookup(QName('http://www.w3.org/2005/xpath-functions/array','members'),1)([1]) instance of jnode()")]
    [InlineData("array:of-members#1(array:members#1([1,2])) => deep-equal([1,2])")]
    [InlineData("array:members(array := [1]) ! jvalue(.) = 1")]
    [InlineData("deep-equal(array:of-members(input := jtree((1,2))), [(1,2)])")]
    public void Values(string expression) => Assert.True(Eval(expression).BooleanValue);

    /// <summary>Gives ordered children one shared root parent wrapping the original array.</summary>
    [Fact]
    public void IdentityAndParents()
    {
        var result = Eval("array:members([(),(1,2),[3]])");
        var values = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!)) values.Add(item);
        var members = values.ToArray();
        Assert.Equal(3, members.Length);
        var parent = members[0].JNodeValue.Parent;
        Assert.NotNull(parent);
        Assert.True(parent.IsRoot);
        Assert.True(parent.Value.IsArray);
        for (int i=0; i<members.Length; i++)
        {
            Assert.True(members[i].IsJNode);
            Assert.Same(parent, members[i].JNodeValue.Parent);
            Assert.Equal(i+1, members[i].JNodeValue.Key.IntegerValue);
        }
        Assert.True(members[0].JNodeValue.Value.IsUndefined || members[0].JNodeValue.Value.IsSequence);
        Assert.True(members[2].JNodeValue.Value.IsArray);
        Assert.Same(parent.Value.ArrayValue.Get(3).ArrayValue, members[2].JNodeValue.Value.ArrayValue);
    }

    /// <summary>Rejects non-array input and every non-JNode item without coercion.</summary>
    /// <param name="expression">An invalid call.</param>
    [Theory]
    [InlineData("array:members(())")]
    [InlineData("array:members(([],[]))")]
    [InlineData("array:members(map{})")]
    [InlineData("array:of-members(1)")]
    [InlineData("array:of-members([1])")]
    [InlineData("array:of-members((jtree(1),'bad'))")]
    public void TypeErrors(string expression) => Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Eval(expression)).ToString());

    /// <summary>Hides dynamic lookup of the pair in the 3.1 profile.</summary>
    /// <param name="name">The function local name.</param>
    [Theory]
    [InlineData("members")]
    [InlineData("of-members")]
    public void LookupProfile(string name) => Assert.True(Eval("empty(function-lookup(QName('http://www.w3.org/2005/xpath-functions/array','"+name+"'),1))", XPathCompatibility.XPath31).BooleanValue);

    /// <summary>Refuses direct and referenced functions in the frozen 3.1 profile.</summary>
    /// <param name="expression">A gated call or reference.</param>
    [Theory]
    [InlineData("array:members([])")]
    [InlineData("array:members#1")]
    [InlineData("array:of-members(())")]
    [InlineData("array:of-members#1")]
    public void Profiles(string expression) => Assert.Contains("XPST0017", Assert.ThrowsAny<Exception>(() => Eval(expression, XPathCompatibility.XPath31)).ToString());
}
