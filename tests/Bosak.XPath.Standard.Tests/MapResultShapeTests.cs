// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 October 2026
// PURPOSE              : Verifies map entry sequences and concatenation of map item values.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-10-2026     | Creation: map result sequence shapes and compatibility boundaries.                       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies map result sequence shapes, ordering and compatibility boundaries.</summary>
public class MapResultShapeTests
{
    private static XdmValue Evaluate(string expression) => XPath31Expression.Compile(expression,
        new CompileOptions { Compatibility = XPathCompatibility.XPath40 }).Evaluate(new EvaluationContext());

    /// <summary>Checks cardinality, item types and order through direct and dynamic calls.</summary>
    /// <param name="expression">The complete expression to evaluate.</param>
    /// <param name="expected">The exact projected result.</param>
    [Theory]
    [InlineData("empty(map:entries(map{}))", "true")]
    [InlineData("map:entries(map{'a':()}) instance of map(*)", "true")]
    [InlineData("every $entry in map:entries(map{'a':(1,2),'b':()}) satisfies map:size($entry) eq 1", "true")]
    [InlineData("count(map:entries(map{'a':(1,2),'b':()}))", "2")]
    [InlineData("string-join(map:entries(map{'b':2,'a':1}) ! string(map:keys(.)), ',')", "b,a")]
    [InlineData("count(function-lookup(xs:QName('map:entries'),1)(map{'a':1,'b':2}))", "2")]
    [InlineData("map:entries#1(map{'a':1}) instance of map(*)", "true")]
    [InlineData("empty(map:items(map{}))", "true")]
    [InlineData("empty(map:items(map{'a':(),'b':()}))", "true")]
    [InlineData("map:items(map{'a':7}) instance of xs:integer", "true")]
    [InlineData("string-join(map:items(map{'b':(2,3),'a':1,'c':()}) ! string(.), ',')", "2,3,1")]
    [InlineData("count(function-lookup(xs:QName('map:items'),1)(map{'a':(1,2),'b':3}))", "3")]
    [InlineData("map:items(map{'a':[1,2],'b':map{'nested':3}})[1] instance of array(*)", "true")]
    [InlineData("map:items(map{'a':[1,2],'b':map{'nested':3}})[2] instance of map(*)", "true")]
    [InlineData("deep-equal(map:items(map{'a':(1,2),'b':(),'c':3}), map:for-each(map{'a':(1,2),'b':(),'c':3}, function($k,$v){$v}))", "true")]
    [InlineData("deep-equal(map:entries(map{'a':1,'b':2}), map:for-each(map{'a':1,'b':2}, function($k,$v){map:entry($k,$v)}))", "true")]
    public void CorrectedResult_MatchesSequenceContract(string expression, string expected)
        => Assert.Equal(expected, Evaluate(expression).ToString());

    /// <summary>Rejects direct 4.0 map functions in the default XPath31 profile.</summary>
    /// <param name="name">The map function local name.</param>
    [Theory]
    [InlineData("entries")]
    [InlineData("items")]
    public void DefaultProfile_RejectsDirectCalls(string name)
    {
        var failure = Assert.ThrowsAny<Exception>(() => XPath31Expression.Compile($"map:{name}(map{{'a':1}})"));
        Assert.Contains("XPST0017", failure.Message);
    }

    /// <summary>Hides frozen map functions from dynamic lookup at XPath31.</summary>
    /// <param name="name">The map function local name.</param>
    [Theory]
    [InlineData("entries")]
    [InlineData("items")]
    public void DefaultProfile_HidesDynamicLookup(string name)
        => Assert.Equal("true", XPath31Expression.Compile($"empty(function-lookup(xs:QName('map:{name}'),1))")
            .Evaluate(new EvaluationContext()).ToString());

    /// <summary>Preserves map value sequences inside entries while expanding them in map:items.</summary>
    [Fact]
    public void EntryValue_RemainsWholeWhileItemsAreConcatenated()
    {
        Assert.Equal("2", Evaluate("count(map:entries(map{'a':(1,2)})?a)").ToString());
        Assert.Equal("3", Evaluate("count(map:items(map{'a':(1,2),'b':[3,4]}))").ToString());
        Assert.Equal("true", Evaluate("map:items(map{'a':(1,2),'b':[3,4]})[3] instance of array(*)").ToString());
    }
}
