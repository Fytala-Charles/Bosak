// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Unit tests for standard XPath / XQuery function implementations.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

public class FunctionLibraryTests
{
    private static XdmValue Evaluate(string xpath)
    {
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        return XPath31Expression.Compile(xpath).Evaluate(ctx);
    }

    private static string EvalStr(string xpath)
        => Evaluate(xpath).ToString();

    private static string[] EvalSequence(string xpath)
    {
        var result = Evaluate(xpath);
        Assert.True(result.IsSequence);
        var list = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            list.Add(item.ToString());
        return list.ToArray();
    }

    // ------------------------------------------------------------------
    // fn:string
    // ------------------------------------------------------------------

    [Fact]
    public void String_Integer() => Assert.Equal("42", EvalStr("fn:string(42)"));

    [Fact]
    public void String_Boolean() => Assert.Equal("true", EvalStr("fn:string(true())"));

    [Fact]
    public void String_EmptySequence() => Assert.Equal("", EvalStr("fn:string(())"));

    // ------------------------------------------------------------------
    // fn:concat
    // ------------------------------------------------------------------

    [Fact]
    public void Concat_TwoArgs() => Assert.Equal("ab", EvalStr("fn:concat('a','b')"));

    [Fact]
    public void Concat_ThreeArgs() => Assert.Equal("abc", EvalStr("fn:concat('a','b','c')"));

    [Fact]
    public void Concat_NumericArgs() => Assert.Equal("12", EvalStr("fn:concat(1,2)"));

    // ------------------------------------------------------------------
    // fn:string-length
    // ------------------------------------------------------------------

    [Fact]
    public void StringLength_Empty() => Assert.Equal("0", EvalStr("fn:string-length('')"));

    [Fact]
    public void StringLength_NonEmpty() => Assert.Equal("5", EvalStr("fn:string-length('hello')"));

    [Fact]
    public void StringLength_DefaultArg() => Assert.Equal("0", EvalStr("fn:string-length(())"));

    // ------------------------------------------------------------------
    // fn:substring
    // ------------------------------------------------------------------

    [Fact]
    public void Substring_Start1() => Assert.Equal("abc", EvalStr("fn:substring('abc',1)"));

    [Fact]
    public void Substring_Start2() => Assert.Equal("bc", EvalStr("fn:substring('abc',2)"));

    [Fact]
    public void Substring_WithLength() => Assert.Equal("ab", EvalStr("fn:substring('abc',1,2)"));

    [Fact]
    public void Substring_OutOfBounds() => Assert.Equal("", EvalStr("fn:substring('abc',10)"));

    // ------------------------------------------------------------------
    // fn:contains
    // ------------------------------------------------------------------

    [Fact]
    public void Contains_True() => Assert.Equal("true", EvalStr("fn:contains('hello','ell')"));

    [Fact]
    public void Contains_False() => Assert.Equal("false", EvalStr("fn:contains('hello','xyz')"));

    [Fact]
    public void Contains_EmptyNeedle() => Assert.Equal("true", EvalStr("fn:contains('hello','')"));

    // ------------------------------------------------------------------
    // fn:starts-with / fn:ends-with
    // ------------------------------------------------------------------

    [Fact]
    public void StartsWith_True() => Assert.Equal("true", EvalStr("fn:starts-with('hello','he')"));

    [Fact]
    public void StartsWith_False() => Assert.Equal("false", EvalStr("fn:starts-with('hello','lo')"));

    [Fact]
    public void EndsWith_True() => Assert.Equal("true", EvalStr("fn:ends-with('hello','lo')"));

    [Fact]
    public void EndsWith_False() => Assert.Equal("false", EvalStr("fn:ends-with('hello','he')"));

    // ------------------------------------------------------------------
    // fn:normalize-space
    // ------------------------------------------------------------------

    [Fact]
    public void NormalizeSpace_Trims() => Assert.Equal("a b", EvalStr("fn:normalize-space('  a  b  ')"));

    [Fact]
    public void NormalizeSpace_Empty() => Assert.Equal("", EvalStr("fn:normalize-space('')"));

    // ------------------------------------------------------------------
    // fn:upper-case / fn:lower-case
    // ------------------------------------------------------------------

    [Fact]
    public void UpperCase() => Assert.Equal("HELLO", EvalStr("fn:upper-case('hello')"));

    [Fact]
    public void LowerCase() => Assert.Equal("hello", EvalStr("fn:lower-case('HELLO')"));

    // ------------------------------------------------------------------
    // fn:matches
    // ------------------------------------------------------------------

    [Fact]
    public void Matches_Literal() => Assert.Equal("true", EvalStr("fn:matches('hello','ell')"));

    [Fact]
    public void Matches_False() => Assert.Equal("false", EvalStr("fn:matches('hello','xyz')"));

    // ------------------------------------------------------------------
    // fn:replace
    // ------------------------------------------------------------------

    [Fact]
    public void Replace() => Assert.Equal("hallo", EvalStr("fn:replace('hello','e','a')"));

    // ------------------------------------------------------------------
    // fn:count
    // ------------------------------------------------------------------

    [Fact]
    public void Count_Empty() => Assert.Equal("0", EvalStr("fn:count(())"));

    [Fact]
    public void Count_Sequence() => Assert.Equal("3", EvalStr("fn:count((1,2,3))"));

    [Fact]
    public void Count_Singleton() => Assert.Equal("1", EvalStr("fn:count(42)"));

    // ------------------------------------------------------------------
    // fn:sum
    // ------------------------------------------------------------------

    [Fact]
    public void Sum_Integers() => Assert.Equal("6", EvalStr("fn:sum((1,2,3))"));

    [Fact]
    public void Sum_Empty_DefaultZero() => Assert.Equal("0", EvalStr("fn:sum(())"));

    [Fact]
    public void Sum_Empty_CustomDefault() => Assert.Equal("42", EvalStr("fn:sum((),42)"));

    // ------------------------------------------------------------------
    // fn:avg
    // ------------------------------------------------------------------

    [Fact]
    public void Avg_Integers() => Assert.Equal("2", EvalStr("fn:avg((1,2,3))"));

    [Fact]
    public void Avg_Empty() => Assert.True(Evaluate("fn:avg(())").IsUndefined);

    // ------------------------------------------------------------------
    // fn:min / fn:max
    // ------------------------------------------------------------------

    [Fact]
    public void Min_Integers() => Assert.Equal("1", EvalStr("fn:min((3,1,2))"));

    [Fact]
    public void Max_Integers() => Assert.Equal("3", EvalStr("fn:max((1,3,2))"));

    [Fact]
    public void Min_Empty() => Assert.True(Evaluate("fn:min(())").IsUndefined);

    [Fact]
    public void Max_Empty() => Assert.True(Evaluate("fn:max(())").IsUndefined);

    // ------------------------------------------------------------------
    // fn:string-join
    // ------------------------------------------------------------------

    [Fact]
    public void StringJoin_CustomSeparator() => Assert.Equal("a-b-c", EvalStr("fn:string-join(('a','b','c'),'-')"));

    // ------------------------------------------------------------------
    // map:*
    // ------------------------------------------------------------------

    [Fact]
    public void MapGet() => Assert.Equal("1", EvalStr("map:get(map{'a':1},'a')"));

    [Fact]
    public void MapSize() => Assert.Equal("2", EvalStr("map:size(map{'a':1,'b':2})"));

    [Fact]
    public void MapContains_True() => Assert.Equal("true", EvalStr("map:contains(map{'a':1},'a')"));

    [Fact]
    public void MapContains_False() => Assert.Equal("false", EvalStr("map:contains(map{'a':1},'b')"));

    // ------------------------------------------------------------------
    // array:*
    // ------------------------------------------------------------------

    [Fact]
    public void ArrayGet() => Assert.Equal("2", EvalStr("array:get([1,2,3],2)"));

    [Fact]
    public void ArraySize() => Assert.Equal("3", EvalStr("array:size([1,2,3])"));

    [Fact]
    public void ArrayContains_True() => Assert.Equal("true", EvalStr("array:contains([1,2,3],2)"));

    [Fact]
    public void ArrayContains_False() => Assert.Equal("false", EvalStr("array:contains([1,2,3],99)"));

    [Fact]
    public void ArrayHead() => Assert.Equal("1", EvalStr("array:head([1,2,3])"));

    // ------------------------------------------------------------------
    // Sequence functions via materialization
    // ------------------------------------------------------------------

    [Fact]
    public void Reverse()
    {
        var items = EvalSequence("fn:reverse((1,2,3))");
        Assert.Equal(new[] { "3", "2", "1" }, items);
    }

    [Fact]
    public void Subsequence_Start()
    {
        var items = EvalSequence("fn:subsequence((1,2,3,4),2)");
        Assert.Equal(new[] { "2", "3", "4" }, items);
    }

    [Fact]
    public void DistinctValues()
    {
        var items = EvalSequence("fn:distinct-values((1,2,2,3,1))");
        Assert.Equal(3, items.Length);
        Assert.Contains("1", items);
        Assert.Contains("2", items);
        Assert.Contains("3", items);
    }

    [Fact]
    public void IndexOf_Found()
    {
        var items = EvalSequence("fn:index-of((1,2,2,3),2)");
        Assert.Equal(new[] { "2", "3" }, items);
    }

    [Fact]
    public void Remove_Middle()
    {
        var items = EvalSequence("fn:remove((1,2,3),2)");
        Assert.Equal(new[] { "1", "3" }, items);
    }

    [Fact]
    public void InsertBefore_Middle()
    {
        var items = EvalSequence("fn:insert-before((1,2,3),2,99)");
        Assert.Equal(new[] { "1", "99", "2", "3" }, items);
    }

    [Fact]
    public void Tokenize()
    {
        var items = EvalSequence("fn:tokenize('a,b,c',',')");
        Assert.Equal(new[] { "a", "b", "c" }, items);
    }

    [Fact]
    public void ArrayTail()
    {
        var result = Evaluate("array:tail([1,2,3])");
        Assert.True(result.IsArray);
    }

    [Fact]
    public void MapMerge()
    {
        var result = Evaluate("map:merge((map{'a':1},map{'b':2}))");
        Assert.True(result.IsMap);
    }

    // ------------------------------------------------------------------
    // Edge cases
    // ------------------------------------------------------------------

    [Fact]
    public void Replace_Global()
    {
        var result = EvalStr("fn:replace('a-b-c','-','_')");
        Assert.Equal("a_b_c", result);
    }
}
