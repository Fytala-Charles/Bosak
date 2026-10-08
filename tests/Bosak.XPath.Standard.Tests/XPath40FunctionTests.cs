// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 oktober 2026
// PURPOSE              : Unit tests for the first XPath 4.0 F&O function batch (REQ-118 slice 4.0-S1).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 08-10-2026     | Creation: replicate, slice, items-at, foot, trunk, insert-separator, char, characters    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 08-10-2026     | Part 2: subsequence family, duplicate-values, all-equal/different, highest/lowest,      |
//                      |                  |       |                | sort-by/sort-with, graphemes, pad-string, trim-space, index-of-substring,               |
//                      |                  |       |                | substring-before/after-last, hash                                                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 08-10-2026     | Part 3 (REQ-118 4.0-S2): map:build/entries/filter/items, array:build/empty/items/slice, |
//                      |                  |       |                | fn:parse-uri/build-uri/decode-from-uri, fn:seconds/duration-to-seconds/build-dateTime/  |
//                      |                  |       |                | unix-dateTime/days-in-month                                                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 08-10-2026     | Part 4 (REQ-118 4.0-S5): fn:some/every/index-where/partition/take-while/drop-while/     |
//                      |                  |       |                | while-do/do-until/partial-apply/transitive-closure + keyword-argument composition       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 08-10-2026     | 4.0-S5 follow-up: fn:identity + keyword-default resolution test                          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

public class XPath40FunctionTests
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
        Assert.True(result.IsSequence);
        var list = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            list.Add(item.ToString());
        return list.ToArray();
    }

    private static InvalidOperationException Error40(string xpath)
        => Assert.Throws<InvalidOperationException>(() => Eval40(xpath));

    // ------------------------------------------------------------------
    // fn:replicate (F+O 4.0 §2.1.10)
    // ------------------------------------------------------------------

    [Fact]
    public void Replicate_SingleItem_RepeatsItem()
    {
        Assert.Equal(["0", "0", "0", "0", "0", "0"], Seq40("fn:replicate(0, 6)"));
    }

    [Fact]
    public void Replicate_MultiItem_RepeatsWholeSequence()
    {
        Assert.Equal(["A", "B", "C", "A", "B", "C", "A", "B", "C"], Seq40("fn:replicate(('A', 'B', 'C'), 3)"));
    }

    [Fact]
    public void Replicate_EmptyInput_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:replicate((), 5)").IsUndefined);
    }

    [Fact]
    public void Replicate_ZeroCount_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:replicate((1 to 10), 0)").IsUndefined);
    }

    [Fact]
    public void Replicate_NegativeCount_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:replicate('a', -3)").Message);
    }

    [Fact]
    public void Replicate_PositionalAccess_PreservesOrder()
    {
        Assert.Equal("X", Eval40("fn:replicate(('X', 'Y'), 100)[99]").ToString());
    }

    // ------------------------------------------------------------------
    // fn:slice (F+O 4.0 §2.1.12)
    // ------------------------------------------------------------------

    [Fact]
    public void Slice_StartAndEnd_IsInclusive()
    {
        Assert.Equal(["b", "c", "d"], Seq40("fn:slice(('a', 'b', 'c', 'd', 'e'), 2, 4)"));
    }

    [Fact]
    public void Slice_StartOnly_GoesToEnd()
    {
        Assert.Equal(["b", "c", "d", "e"], Seq40("fn:slice(('a', 'b', 'c', 'd', 'e'), 2)"));
    }

    [Fact]
    public void Slice_EndOnly_PositionalStart()
    {
        Assert.Equal(["a", "b"], Seq40("fn:slice(('a', 'b', 'c', 'd', 'e'), (), 2)"));
    }

    [Fact]
    public void Slice_ZeroArgs_ReturnsInput()
    {
        Assert.Equal(["a", "b", "c", "d", "e"], Seq40("fn:slice(('a', 'b', 'c', 'd', 'e'))"));
    }

    [Fact]
    public void Slice_SingleItemZeroArgs_ReturnsItem()
    {
        Assert.Equal(["a"], Seq40("fn:slice('a', 0)"));
    }

    [Fact]
    public void Slice_NegativeStart_CountsFromEnd()
    {
        Assert.Equal(["e"], Seq40("fn:slice(('a', 'b', 'c', 'd', 'e'), -1)"));
        Assert.Equal(["c", "d", "e"], Seq40("fn:slice(('a', 'b', 'c', 'd', 'e'), -3)"));
    }

    [Fact]
    public void Slice_NegativeStartPositiveEnd_ReversesRange()
    {
        Assert.Equal(["f", "e", "d", "c", "b"], Seq40("fn:slice(('a', 'b', 'c', 'd', 'e', 'f', 'g'), -2, 2)"));
    }

    [Fact]
    public void Slice_NegativeBounds()
    {
        Assert.Equal(["d", "e", "f"], Seq40("fn:slice(('a', 'b', 'c', 'd', 'e', 'f', 'g'), -4, -2)"));
    }

    [Fact]
    public void Slice_PositiveStep()
    {
        Assert.Equal(["b", "d"], Seq40("fn:slice(('a', 'b', 'c', 'd', 'e'), 2, 5, 2)"));
    }

    [Fact]
    public void Slice_NegativeStep()
    {
        Assert.Equal(["e", "c"], Seq40("fn:slice(('a', 'b', 'c', 'd', 'e'), 5, 2, -2)"));
    }

    [Fact]
    public void Slice_NegativeStepEmptyRange_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:slice(('a', 'b', 'c', 'd', 'e'), 2, 5, -2)").IsUndefined);
        Assert.True(Eval40("fn:slice(('a', 'b', 'c', 'd', 'e'), 5, 2, 2)").IsUndefined);
    }

    [Fact]
    public void Slice_NegativeStepOnly_ReversesInput()
    {
        Assert.Equal(["e", "d", "c", "b", "a"], Seq40("fn:slice(('a', 'b', 'c', 'd', 'e'), (), (), -1)"));
    }

    [Fact]
    public void Slice_EmptyInput_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:slice((), 1, 2, 3)").IsUndefined);
    }

    // ------------------------------------------------------------------
    // fn:items-at (F+O 4.0 §2.1.8)
    // ------------------------------------------------------------------

    [Fact]
    public void ItemsAt_SinglePosition()
    {
        Assert.Equal(["b"], Seq40("fn:items-at(('a', 'b', 'c'), 2)"));
    }

    [Fact]
    public void ItemsAt_MultiplePositionsInAtOrder()
    {
        Assert.Equal(["c", "a"], Seq40("fn:items-at(('a', 'b', 'c', 'd'), (3, 1))"));
    }

    [Fact]
    public void ItemsAt_OutOfRangePositionsIgnored()
    {
        Assert.Equal(["a"], Seq40("fn:items-at(('a', 'b', 'c'), (1, 99))"));
        Assert.True(Eval40("fn:items-at(('a', 'b', 'c'), -2)").IsUndefined);
    }

    [Fact]
    public void ItemsAt_DuplicatePositionsKept()
    {
        Assert.Equal(["b", "b"], Seq40("fn:items-at(('a', 'b', 'c'), (2, 2))"));
    }

    [Fact]
    public void ItemsAt_EmptyAt_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:items-at(('a', 'b'), ())").IsUndefined);
    }

    [Fact]
    public void ItemsAt_NonIntegerPosition_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:items-at(('a', 'b'), 'x')").Message);
    }

    // ------------------------------------------------------------------
    // fn:foot / fn:trunk (F+O 4.0 §2.1.3 / §2.1.15)
    // ------------------------------------------------------------------

    [Fact]
    public void Foot_SimpleSequence_ReturnsLastItem()
    {
        Assert.Equal("5", Eval40("fn:foot(1 to 5)").ToString());
    }

    [Fact]
    public void Foot_EmptySequence_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:foot(())").IsUndefined);
    }

    [Fact]
    public void Foot_Singleton_ReturnsTheItem()
    {
        Assert.Equal("42", Eval40("fn:foot(42)").ToString());
    }

    [Fact]
    public void Trunk_SimpleSequence_ReturnsAllButLast()
    {
        Assert.Equal(["1", "2", "3", "4"], Seq40("fn:trunk(1 to 5)"));
    }

    [Fact]
    public void Trunk_Singleton_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:trunk('a')").IsUndefined);
    }

    [Fact]
    public void Trunk_EmptySequence_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:trunk(())").IsUndefined);
    }

    // ------------------------------------------------------------------
    // fn:insert-separator (F+O 4.0 §2.1.7)
    // ------------------------------------------------------------------

    [Fact]
    public void InsertSeparator_InsertsBetweenAdjacentItems()
    {
        Assert.Equal(["1", "|", "2", "|", "3"], Seq40("fn:insert-separator((1, 2, 3), '|')"));
    }

    [Fact]
    public void InsertSeparator_MultiItemSeparator_InsertedAsBlock()
    {
        Assert.Equal(["1", "⅓", "⅔", "2"], Seq40("fn:insert-separator((1, 2), ('⅓', '⅔'))"));
    }

    [Fact]
    public void InsertSeparator_EmptySeparator_ReturnsInput()
    {
        Assert.Equal(["1", "2"], Seq40("fn:insert-separator((1, 2), ())"));
    }

    [Fact]
    public void InsertSeparator_SingleItem_ReturnsItem()
    {
        Assert.Equal("A", Eval40("fn:insert-separator('A', '|')").ToString());
    }

    [Fact]
    public void InsertSeparator_EmptyInput_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:insert-separator((), '|')").IsUndefined);
    }

    // ------------------------------------------------------------------
    // fn:char (F+O 4.0 §5.4.1)
    // ------------------------------------------------------------------

    [Fact]
    public void Char_Codepoint_ReturnsCharacter()
    {
        Assert.Equal("\t", Eval40("fn:char(9)").ToString());
        Assert.Equal(" ", Eval40("fn:char(32)").ToString());
    }

    [Fact]
    public void Char_Html5Entity_ReturnsCharacter()
    {
        Assert.Equal("á", Eval40("fn:char('aacute')").ToString());
        Assert.Equal("\u00A0", Eval40("fn:char('nbsp')").ToString());
        Assert.Equal("\n", Eval40("fn:char('NewLine')").ToString());
    }

    [Fact]
    public void Char_MultiCodepointEntity_ReturnsFullString()
    {
        Assert.Equal("\u2242\u0338", Eval40("fn:char('NotEqualTilde')").ToString());
    }

    [Fact]
    public void Char_BackslashEscapes_ReturnsControlCharacter()
    {
        Assert.Equal("\n", Eval40("fn:char('\\n')").ToString());
        Assert.Equal("\t", Eval40("fn:char('\\t')").ToString());
        Assert.Equal("\r", Eval40("fn:char('\\r')").ToString());
        Assert.Equal("\b", Eval40("fn:char('\\b')").ToString());
        Assert.Equal("\f", Eval40("fn:char('\\f')").ToString());
    }

    [Fact]
    public void Char_UnknownEntity_RaisesFoch0005()
    {
        Assert.Contains("FOCH0005", Error40("fn:char('Unknown')").Message);
    }

    [Fact]
    public void Char_EmptyString_RaisesFoch0005()
    {
        Assert.Contains("FOCH0005", Error40("fn:char('')").Message);
    }

    [Fact]
    public void Char_EmptySequence_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:char(())").Message);
    }

    [Fact]
    public void Char_NonPositiveInteger_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:char(0)").Message);
        Assert.Contains("XPTY0004", Error40("fn:char(-1)").Message);
    }

    [Fact]
    public void Char_SurrogateOrOutOfRange_RaisesFoch0005()
    {
        Assert.Contains("FOCH0005", Error40("fn:char(57005)").Message); // U+DEAD, unpaired surrogate
        Assert.Contains("FOCH0005", Error40("fn:char(99999999)").Message);
    }

    // ------------------------------------------------------------------
    // fn:characters (F+O 4.0 §5.4.2)
    // ------------------------------------------------------------------

    [Fact]
    public void Characters_SimpleString_SplitsIntoSingleCharacters()
    {
        Assert.Equal(["U", "P"], Seq40("fn:characters('UP')"));
    }

    [Fact]
    public void Characters_NonAsciiString_SplitsByCodepoint()
    {
        Assert.Equal(["T", "h", "é", "r", "è", "s", "e"], Seq40("fn:characters('Thérèse')"));
    }

    [Fact]
    public void Characters_EmptyString_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:characters('')").IsUndefined);
    }

    [Fact]
    public void Characters_EmptySequence_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:characters(())").IsUndefined);
    }

    [Fact]
    public void Characters_NonStringArgument_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:characters(1)").Message);
    }

    // ------------------------------------------------------------------
    // fn:contains-subsequence (F+O 4.0 §2.2.3)
    // ------------------------------------------------------------------

    [Fact]
    public void ContainsSubsequence_EmptySequences_ReturnsTrue()
    {
        Assert.Equal("true", Eval40("fn:contains-subsequence((), ())").ToString());
    }

    [Fact]
    public void ContainsSubsequence_ContiguousRange_ReturnsTrue()
    {
        Assert.Equal("true", Eval40("fn:contains-subsequence(1 to 10, 3 to 6)").ToString());
        Assert.Equal("true", Eval40("fn:contains-subsequence(1 to 10, 5)").ToString());
        Assert.Equal("true", Eval40("fn:contains-subsequence(1 to 10, 1 to 10)").ToString());
    }

    [Fact]
    public void ContainsSubsequence_NonContiguousOrLonger_ReturnsFalse()
    {
        Assert.Equal("false", Eval40("fn:contains-subsequence(1 to 10, (2, 4, 6))").ToString());
        Assert.Equal("false", Eval40("fn:contains-subsequence(1 to 3, 1 to 4)").ToString());
    }

    [Fact]
    public void ContainsSubsequence_EmptySubsequence_ReturnsTrue()
    {
        Assert.Equal("true", Eval40("fn:contains-subsequence(1 to 10, ())").ToString());
    }

    [Fact]
    public void ContainsSubsequence_WithCallback_UsesCallback()
    {
        Assert.Equal("true", Eval40(
            "fn:contains-subsequence(1 to 10, 103 to 105, function($x, $y) { $x mod 100 = $y mod 100 })").ToString());
    }

    [Fact]
    public void ContainsSubsequence_CallbackEmptyResult_TreatedAsFalse()
    {
        Assert.Equal("false", Eval40("fn:contains-subsequence(1 to 3, 1 to 3, function($x, $y) { () })").ToString());
    }

    [Fact]
    public void ContainsSubsequence_NonFunctionCallback_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:contains-subsequence(1 to 3, 1, 'x')").Message);
    }

    // ------------------------------------------------------------------
    // fn:starts-with-subsequence / fn:ends-with-subsequence (F+O 4.0 §2.2.9 / §2.2.7)
    // ------------------------------------------------------------------

    [Fact]
    public void StartsWithSubsequence_PrefixMatch()
    {
        Assert.Equal("true", Eval40("fn:starts-with-subsequence(1 to 10, 1 to 5)").ToString());
        Assert.Equal("true", Eval40("fn:starts-with-subsequence(1 to 10, 1)").ToString());
        Assert.Equal("true", Eval40("fn:starts-with-subsequence((), ())").ToString());
        Assert.Equal("true", Eval40("fn:starts-with-subsequence(1 to 10, ())").ToString());
        Assert.Equal("false", Eval40("fn:starts-with-subsequence(1 to 10, 2 to 5)").ToString());
        Assert.Equal("false", Eval40("fn:starts-with-subsequence(1 to 3, 1 to 4)").ToString());
    }

    [Fact]
    public void StartsWithSubsequence_WithCallback()
    {
        Assert.Equal("true", Eval40(
            "fn:starts-with-subsequence(1 to 10, 101 to 105, function($x, $y) { $x mod 100 = $y mod 100 })").ToString());
    }

    [Fact]
    public void EndsWithSubsequence_SuffixMatch()
    {
        Assert.Equal("true", Eval40("fn:ends-with-subsequence(1 to 10, 7 to 10)").ToString());
        Assert.Equal("true", Eval40("fn:ends-with-subsequence(1 to 10, 10)").ToString());
        Assert.Equal("true", Eval40("fn:ends-with-subsequence((), ())").ToString());
        Assert.Equal("false", Eval40("fn:ends-with-subsequence(1 to 10, 5 to 8)").ToString());
        Assert.Equal("false", Eval40("fn:ends-with-subsequence(1 to 3, 1 to 4)").ToString());
    }

    [Fact]
    public void EndsWithSubsequence_WithCallback()
    {
        Assert.Equal("true", Eval40(
            "fn:ends-with-subsequence(1 to 10, 108 to 110, function($x, $y) { $x mod 100 = $y mod 100 })").ToString());
    }

    // ------------------------------------------------------------------
    // fn:duplicate-values (F+O 4.0 §2.2.6)
    // ------------------------------------------------------------------

    [Fact]
    public void DuplicateValues_SpecExample_ReturnsSecondOccurrence()
    {
        // The duplicate set {1, 1.0, 1e0} contributes its second member, xs:decimal 1.0.
        Assert.Equal("1", Eval40("fn:count(fn:duplicate-values((1, 2, 3, 1.0, 1e0)))").ToString());
        Assert.Equal("true", Eval40("fn:duplicate-values((1, 2, 3, 1.0, 1e0)) instance of xs:decimal").ToString());
        Assert.Equal("true", Eval40("fn:deep-equal(fn:duplicate-values((1, 2, 3, 1.0, 1e0)), 1.0)").ToString());
    }

    [Fact]
    public void DuplicateValues_NoDuplicates_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:duplicate-values(1 to 100)").IsUndefined);
    }

    [Fact]
    public void DuplicateValues_UntypedAtomicEqualsString()
    {
        Assert.Equal(["1"], Seq40("fn:duplicate-values(('1', fn:parse-xml('<x>1</x>')/x, '2', 2))"));
    }

    [Fact]
    public void DuplicateValues_ResultInSecondAppearanceOrder()
    {
        Assert.Equal(["5", "1"], Seq40("fn:duplicate-values((5, 1, 5, 1))"));
    }

    [Fact]
    public void DuplicateValues_WithCollation()
    {
        Assert.Equal(["a"], Seq40(
            "fn:duplicate-values(('A', 'a'), 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive')"));
    }

    // ------------------------------------------------------------------
    // fn:all-equal / fn:all-different (F+O 4.0 §2.4.2 / §2.4.3)
    // ------------------------------------------------------------------

    [Fact]
    public void AllEqual_SpecExamples()
    {
        Assert.Equal("false", Eval40("fn:all-equal((1, 2, 3))").ToString());
        Assert.Equal("true", Eval40("fn:all-equal((1, 1.0, 1.0e0))").ToString());
        Assert.Equal("true", Eval40("fn:all-equal('one')").ToString());
        Assert.Equal("true", Eval40("fn:all-equal(())").ToString());
    }

    [Fact]
    public void AllDifferent_SpecSemantics()
    {
        Assert.Equal("true", Eval40("fn:all-different((1, 2, 3))").ToString());
        Assert.Equal("false", Eval40("fn:all-different((1, 1.0))").ToString());
        Assert.Equal("true", Eval40("fn:all-different(())").ToString());
    }

    // ------------------------------------------------------------------
    // fn:highest / fn:lowest (F+O 4.0 §2.5.10 / §2.5.12)
    // ------------------------------------------------------------------

    [Fact]
    public void Highest_UntypedValuesComparedAsNumbers()
    {
        Assert.Equal(["x"], Seq40("fn:highest(fn:parse-xml('<a x=\"10\" y=\"5\" z=\"2\"/>')/*/@*) ! name()"));
    }

    [Fact]
    public void Highest_WithStringKey_ComparesAsStrings()
    {
        Assert.Equal(["y"], Seq40("fn:highest(fn:parse-xml('<a x=\"10\" y=\"5\" z=\"2\"/>')/*/@*, (), fn:string#1) ! name()"));
    }

    [Fact]
    public void Highest_WithComputedKey()
    {
        Assert.Equal(["green"], Seq40("fn:highest(('red', 'green', 'blue'), (), fn:string-length#1)"));
    }

    [Fact]
    public void Highest_EqualBoundaryItems_AllRetainedInInputOrder()
    {
        Assert.Equal("2", Eval40("fn:count(fn:highest((1, 1.0)))").ToString());
        Assert.Equal("true", Eval40("fn:highest((1, 1.0))[2] instance of xs:decimal").ToString());
        Assert.Equal("true", Eval40("fn:highest((1, 1.0))[1] instance of xs:integer").ToString());
    }

    [Fact]
    public void Highest_EmptyInput_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:highest(())").IsUndefined);
        Assert.True(Eval40("fn:lowest(())").IsUndefined);
    }

    [Fact]
    public void Highest_UntypedKeyNotCastable_RaisesForg0001()
    {
        Assert.Contains("FORG0001", Error40("fn:highest(fn:parse-xml('<a x=\"abc\"/>')/*/@*)").Message);
    }

    [Fact]
    public void Lowest_SelectsMinimum()
    {
        Assert.Equal(["1"], Seq40("fn:lowest((3, 2, 1))"));
        Assert.Equal(["red"], Seq40("fn:lowest(('green', 'red', 'blue'), (), fn:string-length#1)"));
    }

    // ------------------------------------------------------------------
    // fn:sort-by (F+O 4.0 §2.5.18)
    // ------------------------------------------------------------------

    [Fact]
    public void SortBy_EmptyKeys_UsesDefaultKey()
    {
        Assert.Equal(["1", "3", "4", "5", "6"], Seq40("fn:sort-by((1, 4, 6, 5, 3), ())"));
    }

    [Fact]
    public void SortBy_DescendingOrder()
    {
        Assert.Equal(["6", "5", "4", "3", "1"], Seq40("fn:sort-by((1, 4, 6, 5, 3), map { 'order': 'descending' })"));
    }

    [Fact]
    public void SortBy_WithKeyFunction_StableForEqualKeys()
    {
        Assert.Equal(["1", "-2", "5", "8", "10", "-10", "10"],
            Seq40("fn:sort-by((1, -2, 5, 10, -10, 10, 8), map { 'key': fn:abs#1 })"));
    }

    [Fact]
    public void SortBy_MultipleKeyDefinitions_MajorToMinor()
    {
        Assert.Equal(["c", "a", "bb", "aa"],
            Seq40("fn:sort-by(('bb', 'a', 'aa', 'c'), (map { 'key': fn:string-length#1 }, map { 'key': fn:string#1, 'order': 'descending' }))"));
    }

    [Fact]
    public void SortBy_InvalidOrderValue_RaisesForg0001()
    {
        Assert.Contains("FORG0001", Error40("fn:sort-by((1, 2), map { 'order': 'up' })").Message);
    }

    [Fact]
    public void SortBy_NonMapKeyItem_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:sort-by((1, 2), 'not a map')").Message);
    }

    [Fact]
    public void SortBy_EmptyInput_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:sort-by((), map { 'order': 'descending' })").IsUndefined);
    }

    // ------------------------------------------------------------------
    // fn:sort-with (F+O 4.0 §2.5.20)
    // ------------------------------------------------------------------

    [Fact]
    public void SortWith_CompareComparator_SortsStrings()
    {
        Assert.Equal(["a", "b", "c"], Seq40("fn:sort-with(('c', 'a', 'b'), fn:compare#2)"));
    }

    [Fact]
    public void SortWith_InlineComparator_SortsNumbers()
    {
        Assert.Equal(["1", "2", "3"], Seq40("fn:sort-with((3, 1, 2), function($a, $b) { $a - $b })"));
    }

    [Fact]
    public void SortWith_StableWhenComparatorReturnsZero()
    {
        Assert.Equal(["2", "1", "1"], Seq40("fn:sort-with((2, 1, 1), function($a, $b) { 0 })"));
    }

    [Fact]
    public void SortWith_MultipleComparators_FirstNonZeroWins()
    {
        Assert.Equal(["a", "c", "aa", "bb"],
            Seq40("fn:sort-with(('bb', 'a', 'aa', 'c'), (function($x, $y) { fn:string-length($x) - fn:string-length($y) }, fn:compare#2))"));
    }

    [Fact]
    public void SortWith_EmptyComparators_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:sort-with((1, 2), ())").Message);
    }

    [Fact]
    public void SortWith_ComparatorEmptyResult_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:sort-with((2, 1), function($a, $b) { () })").Message);
    }

    // ------------------------------------------------------------------
    // fn:graphemes (F+O 4.0 §5.4.3)
    // ------------------------------------------------------------------

    [Fact]
    public void Graphemes_CombiningMark_AttachesToBase()
    {
        Assert.Equal(["a\u0308", "b"], Seq40("fn:graphemes('a' || fn:char(776) || 'b')"));
    }

    [Fact]
    public void Graphemes_CrLf_IsOneGrapheme()
    {
        Assert.Equal(["\r\n"], Seq40("fn:graphemes(fn:char(13) || fn:char(10))"));
    }

    [Fact]
    public void Graphemes_EmojiZwjSequence_IsOneGrapheme()
    {
        Assert.Equal(["\U0001F476\u200D\U0001F6D1"], Seq40("fn:graphemes(fn:char(128118) || fn:char(8205) || fn:char(128721))"));
    }

    [Fact]
    public void Graphemes_DevanagariAdjacentConsonants_Split()
    {
        Assert.Equal(["क", "त"], Seq40("fn:graphemes('कत')"));
    }

    [Fact]
    public void Graphemes_IndicConjunct_WithZwJ_IsOneGrapheme()
    {
        Assert.Equal(["क\u093C\u200D\u094Dत"], Seq40("fn:graphemes('क' || fn:char(2364) || fn:char(8205) || fn:char(2381) || 'त')"));
    }

    [Fact]
    public void Graphemes_EmptyInput_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:graphemes('')").IsUndefined);
        Assert.True(Eval40("fn:graphemes(())").IsUndefined);
    }

    // ------------------------------------------------------------------
    // fn:pad-string (F+O 4.0 §5.4.6)
    // ------------------------------------------------------------------

    [Fact]
    public void PadString_DefaultPadsAtEndWithSpaces()
    {
        Assert.Equal("abc   ", Eval40("fn:pad-string('abc', 6)").ToString());
    }

    [Fact]
    public void PadString_SideOption()
    {
        Assert.Equal("   abc", Eval40("fn:pad-string('abc', 6, map { 'side': 'start' })").ToString());
    }

    [Fact]
    public void PadString_BothSidesSplitsWithExtraAtEnd()
    {
        Assert.Equal("**abc**", Eval40("fn:pad-string('abc', 7, map { 'side': 'both', 'padding': '*' })").ToString());
        Assert.Equal("**abc***", Eval40("fn:pad-string('abc', 8, map { 'side': 'both', 'padding': '*' })").ToString());
    }

    [Fact]
    public void PadString_PaddingRunsArePrefixesOfRepeatedPadding()
    {
        Assert.Equal("0123x", Eval40("fn:pad-string('x', 5, map { 'padding': '0123456789', 'side': 'start' })").ToString());
        Assert.Equal("012x0123", Eval40("fn:pad-string('x', 8, map { 'padding': '0123456789', 'side': 'both' })").ToString());
    }

    [Fact]
    public void PadString_EmptyInput_PadsFully()
    {
        Assert.Equal("*****", Eval40("fn:pad-string((), 5, map { 'padding': '*' })").ToString());
    }

    [Fact]
    public void PadString_NeverTruncates()
    {
        Assert.Equal("abcdef", Eval40("fn:pad-string('abcdef', 3)").ToString());
    }

    [Fact]
    public void PadString_NonStringInput_IsCast()
    {
        Assert.Equal("..2026-07-12..", Eval40(
            "fn:pad-string(xs:date('2026-07-12'), 14, map { 'padding': '.', 'side': 'both' })").ToString());
    }

    [Fact]
    public void PadString_ZeroLengthPadding_RaisesForg0001()
    {
        Assert.Contains("FORG0001", Error40("fn:pad-string('abc', 6, map { 'padding': '' })").Message);
    }

    [Fact]
    public void PadString_InvalidSide_RaisesForg0001()
    {
        Assert.Contains("FORG0001", Error40("fn:pad-string('abc', 6, map { 'side': 'middle' })").Message);
    }

    // ------------------------------------------------------------------
    // fn:trim-space (F+O 4.0 §5.4.11)
    // ------------------------------------------------------------------

    [Fact]
    public void TrimSpace_DefaultTrimsBothEnds()
    {
        Assert.Equal("Albert  Camus", Eval40("fn:trim-space('  Albert  Camus  ')").ToString());
    }

    [Fact]
    public void TrimSpace_SideOption()
    {
        Assert.Equal("Albert  Camus  ", Eval40("fn:trim-space('  Albert  Camus  ', map { 'side': 'start' })").ToString());
        Assert.Equal("  Albert  Camus", Eval40("fn:trim-space('  Albert  Camus  ', map { 'side': 'end' })").ToString());
    }

    [Fact]
    public void TrimSpace_XmlWhitespaceOnly()
    {
        Assert.Equal("x", Eval40("fn:trim-space(' \t\r\nx \t\r\n')").ToString());
        // A non-breaking space is not a whitespace character.
        Assert.Equal("\u00A0x", Eval40("fn:trim-space(fn:char(160) || 'x ')").ToString());
    }

    [Fact]
    public void TrimSpace_EmptyInput_ReturnsEmptyString()
    {
        Assert.Equal("", Eval40("fn:trim-space(())").ToString());
        Assert.Equal("", Eval40("fn:trim-space(' \t ')").ToString());
    }

    [Fact]
    public void TrimSpace_InvalidSide_RaisesForg0001()
    {
        Assert.Contains("FORG0001", Error40("fn:trim-space('x', map { 'side': 'bothh' })").Message);
    }

    // ------------------------------------------------------------------
    // fn:index-of-substring (F+O 4.0 §5.4.8)
    // ------------------------------------------------------------------

    [Fact]
    public void IndexOfSubstring_SpecExamples()
    {
        Assert.Equal(["2", "4"], Seq40("fn:index-of-substring('banana', 'an')"));
        Assert.Equal(["2", "4", "6"], Seq40("fn:index-of-substring('banana', 'a')"));
        Assert.Equal(["2", "4"], Seq40("fn:index-of-substring('banana', 'ana')"));
        Assert.True(Eval40("fn:index-of-substring('banana', 'x')").IsUndefined);
    }

    [Fact]
    public void IndexOfSubstring_EmptySubstring_MatchesEveryPosition()
    {
        Assert.Equal(["1", "2", "3", "4"], Seq40("fn:index-of-substring('abc', '')"));
        Assert.Equal(["1"], Seq40("fn:index-of-substring('', '')"));
    }

    [Fact]
    public void IndexOfSubstring_EmptyValue_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:index-of-substring((), 'an')").IsUndefined);
    }

    [Fact]
    public void IndexOfSubstring_PositionsInCodepoints()
    {
        Assert.Equal(["1", "3"], Seq40("fn:index-of-substring('\U0001F600a\U0001F600', '\U0001F600')"));
    }

    [Fact]
    public void IndexOfSubstring_MultiItemValue_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:index-of-substring(('a', 'b'), 'a')").Message);
    }

    // ------------------------------------------------------------------
    // fn:substring-before-last / fn:substring-after-last (F+O 4.0 §5.5.6 / §5.5.7)
    // ------------------------------------------------------------------

    [Fact]
    public void SubstringBeforeLast_SpecExamples()
    {
        Assert.Equal("a/b", Eval40("fn:substring-before-last('a/b/c', '/')").ToString());
        Assert.Equal("tat", Eval40("fn:substring-before-last('tattoo', 't')").ToString());
        Assert.Equal("a", Eval40("fn:substring-before-last('aaa', 'aa')").ToString());
        Assert.Equal("", Eval40("fn:substring-before-last('tattoo', 'x')").ToString());
        Assert.Equal("tattoo", Eval40("fn:substring-before-last('tattoo', '')").ToString());
        Assert.Equal("", Eval40("fn:substring-before-last((), ())").ToString());
    }

    [Fact]
    public void SubstringAfterLast_SpecExamples()
    {
        Assert.Equal("c", Eval40("fn:substring-after-last('a/b/c', '/')").ToString());
        Assert.Equal("gz", Eval40("fn:substring-after-last('archive.tar.gz', '.')").ToString());
        Assert.Equal("", Eval40("fn:substring-after-last('aaa', 'aa')").ToString());
        Assert.Equal("", Eval40("fn:substring-after-last('tattoo', 'x')").ToString());
        Assert.Equal("", Eval40("fn:substring-after-last('tattoo', '')").ToString());
        Assert.Equal("", Eval40("fn:substring-after-last((), ())").ToString());
    }

    [Fact]
    public void SubstringBeforeLast_WithCollation()
    {
        Assert.Equal("A-b-", Eval40(
            "fn:substring-before-last('A-b-B-c', 'b', 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive')").ToString());
    }

    [Fact]
    public void SubstringAfterLast_WithCollation()
    {
        Assert.Equal("-c", Eval40(
            "fn:substring-after-last('A-b-B-c', 'b', 'http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive')").ToString());
    }

    // ------------------------------------------------------------------
    // fn:hash (F+O 4.0 §5.4.16)
    // ------------------------------------------------------------------

    [Fact]
    public void Hash_SpecVectors()
    {
        Assert.Equal("900150983CD24FB0D6963F7D28E17F72", Eval40("fn:hash('abc')").ToString());
        Assert.Equal("902FBDD2B1DF0C4F70B4A5D23525E932", Eval40("fn:hash('ABC')").ToString());
        Assert.Equal("D41D8CD98F00B204E9800998ECF8427E", Eval40("fn:hash('')").ToString());
        Assert.Equal("3C01BDBB26F358BAB27F267924AA2C9A03FCFDB8", Eval40("fn:hash('ABC', 'SHA-1')").ToString());
        Assert.Equal("B5D4045C3F466FA91FE2CC6ABE79232A1A57CDF104F7A26E716E0A1E2789DF78",
            Eval40("fn:hash('ABC', 'sha-256')").ToString());
    }

    [Fact]
    public void Hash_AlgorithmIsNormalized()
    {
        Assert.Equal("900150983CD24FB0D6963F7D28E17F72", Eval40("fn:hash('abc', '  md5 ')").ToString());
    }

    [Fact]
    public void Hash_BinaryInputs_HashOctets()
    {
        Assert.Equal("900150983CD24FB0D6963F7D28E17F72", Eval40("fn:hash(xs:hexBinary('616263'))").ToString());
        Assert.Equal("900150983CD24FB0D6963F7D28E17F72", Eval40("fn:hash(xs:base64Binary('YWJj'))").ToString());
    }

    [Fact]
    public void Hash_EmptySequence_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:hash(())").IsUndefined);
    }

    [Fact]
    public void Hash_UnsupportedAlgorithm_RaisesFoha0001()
    {
        Assert.Contains("FOHA0001", Error40("fn:hash('abc', 'BLAKE3')").Message);
        Assert.Contains("FOHA0001", Error40("fn:hash('abc', 'no-such-alg')").Message);
    }

    [Fact]
    public void Hash_NonStringBinaryInput_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:hash(123)").Message);
    }

    // ------------------------------------------------------------------
    // map:build (F+O 4.0 §14.3)
    // ------------------------------------------------------------------

    [Fact]
    public void MapBuild_SpecExample_GroupsByKey()
    {
        Assert.Equal("3,6,9", Eval40(
            "string-join(map:get(map:build(1 to 10, function($i){$i mod 3}), 0), ',')").ToString());
        Assert.Equal("1,4,7,10", Eval40(
            "string-join(map:get(map:build(1 to 10, function($i){$i mod 3}), 1), ',')").ToString());
    }

    [Fact]
    public void MapBuild_DefaultKeyAndValue_IsIdentity()
    {
        Assert.Equal("a", Eval40("map:get(map:build(('a', 'b')), 'a')").ToString());
    }

    [Fact]
    public void MapBuild_ArityOneCallback_ReceivesExtraPositionArgDropped()
    {
        // F+O 4.0 §1.8: arity-n function accepted where arity-m (m >= n) is declared.
        Assert.Equal("1,3", Eval40(
            "string-join(map:get(map:build((1, 2, 3), function($x){$x mod 2}), 1), ',')").ToString());
    }

    [Fact]
    public void MapBuild_ValueFunction_ReceivesPosition()
    {
        Assert.Equal("1,9", Eval40(
            "string-join(map:get(map:build((1, 2, 3), function($x){$x mod 2}, function($x, $p){$x * $p}), 1), ',')").ToString());
    }

    [Fact]
    public void MapBuild_KeyYieldsMultipleKeys_EachBecomesEntry()
    {
        Assert.Equal("6", Eval40(
            "map:size(map:build((1, 2, 3), function($x){($x, $x + 10)}))").ToString());
    }

    [Fact]
    public void MapBuild_KeyYieldsEmptySequence_NoEntry()
    {
        Assert.Equal("0", Eval40("map:size(map:build((1, 2, 3), function($x){()}))").ToString());
    }

    [Fact]
    public void MapBuild_DuplicatesCombine_ConcatenatesValues()
    {
        Assert.Equal("1,3", Eval40(
            "string-join(map:get(map:build((1, 2, 3, 4), function($i){$i mod 2}, function($i){$i}), 1), ',')").ToString());
    }

    [Fact]
    public void MapBuild_DuplicatesUseLast_KeepsLastValue()
    {
        Assert.Equal("3", Eval40(
            "map:get(map:build((1, 2, 3, 4), function($i){$i mod 2}, function($i){$i}, map{'duplicates':'use-last'}), 1)").ToString());
    }

    [Fact]
    public void MapBuild_DuplicatesReject_RaisesFojs0003()
    {
        Assert.Contains("FOJS0003", Error40(
            "map:build((1, 3), function($i){$i mod 2}, function($i){$i}, map{'duplicates':'reject'})").Message);
    }

    [Fact]
    public void MapBuild_DuplicatesCombinerFunction_AppliesCombiner()
    {
        Assert.Equal("6", Eval40(
            "map:get(map:build((1, 2, 3, 4), function($i){$i mod 2}, function($i){$i}, " +
            "map{'duplicates':function($a, $b){$a + $b}}), 0)").ToString());
    }

    [Fact]
    public void MapBuild_InvalidDuplicatesOption_RaisesFojs0005()
    {
        Assert.Contains("FOJS0005", Error40(
            "map:build((1, 2), function($i){$i}, function($i){$i}, map{'duplicates':'nonsense'})").Message);
    }

    // ------------------------------------------------------------------
    // map:entries (F+O 4.0 §14.2.2)
    // ------------------------------------------------------------------

    [Fact]
    public void MapEntries_ReturnsSingleEntryMapsInOrder()
    {
        Assert.Equal("2", Eval40("map:entries(map{'a':1, 'b':2})?2?b").ToString());
        Assert.Equal("3", Eval40("array:size(map:entries(map{'a':1, 'b':2, 'c':3}))").ToString());
        Assert.Equal("0", Eval40("array:size(map:entries(map{}))").ToString());
    }

    // ------------------------------------------------------------------
    // map:filter (F+O 4.0 §14.2.3)
    // ------------------------------------------------------------------

    [Fact]
    public void MapFilter_KeepsMatchingEntriesWithPosition()
    {
        Assert.Equal("3", Eval40(
            "map:filter(map{'a':1, 'b':2, 'c':3}, function($k, $v, $p){$v mod 2 eq 1})?c").ToString());
        Assert.True(Eval40(
            "map:filter(map{'a':1, 'b':2, 'c':3}, function($k, $v, $p){$v mod 2 eq 1})?b").IsUndefined);
    }

    [Fact]
    public void MapFilter_EmptyPredicateResultMeansFalse()
    {
        Assert.True(Eval40(
            "map:filter(map{'a':1}, function($k, $v, $p){()})?a").IsUndefined);
    }

    // ------------------------------------------------------------------
    // map:items (F+O 4.0 §14.4.6)
    // ------------------------------------------------------------------

    [Fact]
    public void MapItems_ReturnsValuesInEntryOrder()
    {
        Assert.Equal("x,y", Eval40(
            "string-join(map:items(map{'a':'x', 'b':'y'}), ',')").ToString());
        Assert.Equal("2", Eval40("count(map:items(map{'a':1, 'b':(2, 3)}))").ToString());
    }

    // ------------------------------------------------------------------
    // array:build (F+O 4.0 §16.2)
    // ------------------------------------------------------------------

    [Fact]
    public void ArrayBuild_DefaultAction_IsIdentityPerItem()
    {
        Assert.Equal("3", Eval40("array:size(array:build((1, 2, 3)))").ToString());
    }

    [Fact]
    public void ArrayBuild_ActionReceivesItemAndPosition()
    {
        Assert.Equal("6", Eval40("array:get(array:build((1, 2, 3), function($x, $p){$x + $p}), 3)").ToString());
    }

    [Fact]
    public void ArrayBuild_ResultSequenceBecomesSingleMember()
    {
        Assert.Equal("2", Eval40("count(array:get(array:build(1 to 3, function($x){($x, $x)}), 1))").ToString());
    }

    // ------------------------------------------------------------------
    // array:empty (F+O 4.0 §16.3.4)
    // ------------------------------------------------------------------

    [Fact]
    public void ArrayEmpty_OnlyZeroMemberArrayIsEmpty()
    {
        Assert.Equal("true", Eval40("array:empty([])").ToString());
        Assert.Equal("false", Eval40("array:empty([[]])").ToString());
        Assert.Equal("false", Eval40("array:empty([()])").ToString());
    }

    // ------------------------------------------------------------------
    // array:items (F+O 4.0 §16.3.5)
    // ------------------------------------------------------------------

    [Fact]
    public void ArrayItems_ConcatenatesMembersNonRecursively()
    {
        Assert.Equal("1,2,3,4", Eval40(
            "string-join(array:items([(1, 2), (3, 4)]), ',')").ToString());
        // Nested arrays are members, not flattened: items() yields the array itself.
        Assert.Equal("3", Eval40("count(array:items([(1, 2), [3, 4]]))").ToString());
    }

    // ------------------------------------------------------------------
    // array:slice (F+O 4.0 §16.5.2)
    // ------------------------------------------------------------------

    [Fact]
    public void ArraySlice_StartEndStep()
    {
        Assert.Equal("2,3,4", Eval40("string-join(array:slice([1, 2, 3, 4, 5], 2, 4)?*, ',')").ToString());
        Assert.Equal("1,3,5", Eval40("string-join(array:slice([1, 2, 3, 4, 5, 6], (), (), 2)?*, ',')").ToString());
        Assert.Equal("4,5", Eval40("string-join(array:slice([1, 2, 3, 4, 5], -2, -1)?*, ',')").ToString());
        Assert.Equal("5,4,3,2,1", Eval40("string-join(array:slice([1, 2, 3, 4, 5], (), (), -1)?*, ',')").ToString());
    }

    [Fact]
    public void ArraySlice_OutOfBoundsYieldsEmptyArrayNoError()
    {
        Assert.Equal("0", Eval40("array:size(array:slice([1, 2, 3], 10, 20))").ToString());
        Assert.Equal("0", Eval40("array:size(array:slice([], 1, 3))").ToString());
    }

    // ------------------------------------------------------------------
    // fn:decode-from-uri (F+O 4.0 §7.1)
    // ------------------------------------------------------------------

    [Fact]
    public void DecodeFromUri_SpecExamples()
    {
        Assert.Equal("http://example.com/", Eval40("fn:decode-from-uri('http://example.com/')").ToString());
        Assert.Equal("~bébé?a=b+c", Eval40("fn:decode-from-uri('~b%C3%A9b%C3%A9?a=b+c')").ToString());
        Assert.Equal("~bébé?a=b c", Eval40(
            "fn:decode-from-uri(translate('~b%C3%A9b%C3%A9?a=b+c', '+', ' '))").ToString());
    }

    [Fact]
    public void DecodeFromUri_InvalidEscapesAndInvalidUtf8_BecomeReplacementChar()
    {
        Assert.Equal("�-�-�A-�💡", Eval40("fn:decode-from-uri('%00-%XX-%F0%9F%92%41-%F0%F0%9F%92%A1')").ToString());
        Assert.Equal("�!", Eval40("fn:decode-from-uri('%1X!')").ToString());
    }

    [Fact]
    public void DecodeFromUri_EmptySequence_ReturnsZeroLengthString()
    {
        Assert.Equal("", Eval40("fn:decode-from-uri(())").ToString());
    }

    // ------------------------------------------------------------------
    // fn:parse-uri (F+O 4.0 §7.6.2)
    // ------------------------------------------------------------------

    [Fact]
    public void ParseUri_HierarchicalHttp_AllFields()
    {
        Assert.Equal("http|user@example.com:8080|user|example.com|8080|/p|q=1|frag",
            Eval40("let $m := fn:parse-uri('http://user@example.com:8080/p?q=1#frag') " +
                   "return string-join(($m?scheme, $m?authority, $m?userinfo, $m?host, $m?port, $m?path, $m?query, $m?fragment), '|')").ToString());
    }

    [Fact]
    public void ParseUri_AbsoluteOnlyWithoutFragment()
    {
        Assert.Equal("true", Eval40("fn:parse-uri('http://www.ietf.org/rfc/rfc2396.txt')?absolute").ToString());
        Assert.True(Eval40("fn:parse-uri('http://h/p#f')?absolute").IsUndefined);
    }

    [Fact]
    public void ParseUri_PortIsInteger()
    {
        Assert.Equal("true", Eval40(
            "fn:parse-uri('https://example.com:8080/path')?port instance of xs:integer").ToString());
    }

    [Fact]
    public void ParseUri_FileScheme_SegmentsAndFilepath()
    {
        Assert.Equal("c:/path/to/file", Eval40("fn:parse-uri('file:///c:/path/to/file')?filepath").ToString());
        Assert.Equal("|c:|path|to|file", Eval40(
            "string-join(fn:parse-uri('file:///c:/path/to/file')?path-segments, '|')").ToString());
    }

    [Fact]
    public void ParseUri_DriveLetterAcqiresFileScheme()
    {
        Assert.Equal("file", Eval40(@"fn:parse-uri('c:\path\to\file')?scheme").ToString());
        Assert.Equal("c:/path/to/file", Eval40(@"fn:parse-uri('c:\path\to\file')?filepath").ToString());
    }

    [Fact]
    public void ParseUri_FragmentOnlyOrQueryOnly_PathIsEmpty()
    {
        Assert.Equal("0|testing", Eval40(
            "string-join((count(fn:parse-uri('#testing')?path), fn:parse-uri('#testing')?fragment), '|')").ToString());
        Assert.True(Eval40("fn:parse-uri('#testing')?hierarchical").IsUndefined);
        Assert.Equal("0|q=1", Eval40(
            "string-join((count(fn:parse-uri('?q=1')?path), fn:parse-uri('?q=1')?query), '|')").ToString());
    }

    [Fact]
    public void ParseUri_Ipv6Host_FormDecodedQueryParameters()
    {
        Assert.Equal("[2001:db8::7]", Eval40(
            "fn:parse-uri('ldap://[2001:db8::7]/c=GB?objectClass?one')?host").ToString());
        // A token without '=' has key "" and the whole token as value.
        Assert.Equal("objectClass?one", Eval40(
            "fn:parse-uri('ldap://[2001:db8::7]/c=GB?objectClass?one')?query-parameters?''").ToString());
        Assert.Equal("\"hello world\"", Eval40(
            "fn:parse-uri('https://example.com:8080/path?s=%22hello world%22&sort=relevance')?query-parameters?s").ToString());
    }

    [Fact]
    public void ParseUri_QueryParametersAccumulateDuplicateKeys()
    {
        Assert.Equal("1,2", Eval40(
            "string-join(fn:parse-uri('http://h/?a=1&a=2&b=3')?query-parameters?a, ',')").ToString());
        // Plus decodes to space in query parameters only (Issue 2811).
        Assert.Equal("b c", Eval40("fn:parse-uri('http://h/?a=b+c')?query-parameters?a").ToString());
    }

    [Fact]
    public void ParseUri_UserInfoPasswordDiscardedUnlessDeprecatedAllowed()
    {
        Assert.True(Eval40("fn:parse-uri('http://user:pw@host/path')?userinfo").IsUndefined);
        Assert.Equal("user:pw", Eval40(
            "fn:parse-uri('http://user:pw@host/path', map{'allow-deprecated-features':true()})?userinfo").ToString());
    }

    [Fact]
    public void ParseUri_OmitDefaultPorts()
    {
        Assert.True(Eval40("fn:parse-uri('http://h:80/', map{'omit-default-ports':true()})?port").IsUndefined);
        Assert.Equal("8080", Eval40("fn:parse-uri('http://h:8080/', map{'omit-default-ports':true()})?port").ToString());
    }

    [Fact]
    public void ParseUri_NonHierarchicalScheme_HasNoAuthority()
    {
        Assert.Equal("mailto|false|0", Eval40(
            "string-join((fn:parse-uri('mailto:user@example.com')?scheme, " +
            "fn:parse-uri('mailto:user@example.com')?hierarchical, " +
            "count(fn:parse-uri('mailto:user@example.com')?absolute)), '|')").ToString());
    }

    [Fact]
    public void ParseUri_UnmatchedBracketInAuthority_RaisesFour0001()
    {
        Assert.Contains("FOUR0001", Error40("fn:parse-uri('http://[2001:db8::7/path')").Message);
    }

    [Fact]
    public void ParseUri_EmptySequence_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:parse-uri(())").IsUndefined);
    }

    // ------------------------------------------------------------------
    // fn:build-uri (F+O 4.0 §7.6.3)
    // ------------------------------------------------------------------

    [Fact]
    public void BuildUri_SpecExample()
    {
        Assert.Equal("https://qt4cg.org/specifications/index.html", Eval40(
            "fn:build-uri(map{'scheme':'https', 'host':'qt4cg.org', 'port':(), 'path':'/specifications/index.html'})").ToString());
    }

    [Fact]
    public void BuildUri_ParseRoundTrip_ReencodesDelimiters()
    {
        Assert.Equal("http://example.com:8080/p?s=\"hello%20world\"#frag", Eval40(
            "fn:build-uri(fn:parse-uri('http://example.com:8080/p?s=%22hello world%22#frag'))").ToString());
        Assert.Equal("https://u@h/p%20x?q=a%20b#c%20d", Eval40(
            "fn:build-uri(fn:parse-uri('https://u@h/p x?q=a b#c d'))").ToString());
    }

    [Fact]
    public void BuildUri_PathSegmentsEscapedOnlyWhenHierarchical()
    {
        // Segments are joined verbatim (a leading "" segment supplies the leading slash,
        // exactly as produced by fn:parse-uri).
        Assert.Equal("http://h/a%20b/c%2Fd", Eval40(
            "fn:build-uri(map{'scheme':'http', 'host':'h', 'path-segments':('', 'a b', 'c/d')})").ToString());
        Assert.Equal("mailto:a b/c/d", Eval40(
            "fn:build-uri(map{'scheme':'mailto', 'hierarchical':false(), 'path-segments':('a b', 'c/d')})").ToString());
    }

    [Fact]
    public void BuildUri_QueryParameters_EscapeAndEmptyKey()
    {
        Assert.Equal("http://h/?a=1&a=2&b=x%20y", Eval40(
            "fn:build-uri(map{'scheme':'http', 'host':'h', 'path':'/', " +
            "'query-parameters':map{'a':('1','2'), 'b':'x y'}})").ToString());
        Assert.Equal("http://h?v", Eval40(
            "fn:build-uri(map{'scheme':'http', 'host':'h', 'query-parameters':map{'':'v'}})").ToString());
        // A plus is escaped because parse-uri form-decodes query parameters.
        Assert.Equal("http://h?a=b%2Bc", Eval40(
            "fn:build-uri(map{'scheme':'http', 'host':'h', 'query-parameters':map{'a':'b+c'}})").ToString());
    }

    [Fact]
    public void BuildUri_NonHierarchicalScheme_UsesColonDelimiter()
    {
        Assert.Equal("mailto:user@example.com", Eval40(
            "fn:build-uri(map{'scheme':'mailto', 'path':'user@example.com'})").ToString());
        Assert.Equal("urn:example:animal:ferret:nose", Eval40(
            "fn:build-uri(map{'scheme':'urn', 'path':'example:animal:ferret:nose'})").ToString());
    }

    [Fact]
    public void BuildUri_OmitDefaultPortsOption()
    {
        Assert.Equal("http://h", Eval40(
            "fn:build-uri(map{'scheme':'http', 'host':'h', 'port':'80'}, map{'omit-default-ports':true()})").ToString());
        Assert.Equal("http://h:8080", Eval40(
            "fn:build-uri(map{'scheme':'http', 'host':'h', 'port':'8080'}, map{'omit-default-ports':true()})").ToString());
    }

    [Fact]
    public void BuildUri_UncPathOption_DoubleSlashPrefix()
    {
        Assert.Equal("file://///server/share", Eval40(
            "fn:build-uri(map{'scheme':'file', 'path-segments':('', 'server', 'share')}, map{'unc-path':true()})").ToString());
    }

    // ------------------------------------------------------------------
    // fn:seconds / fn:duration-to-seconds (F+O 4.0 §8.4)
    // ------------------------------------------------------------------

    [Fact]
    public void Seconds_SpecExamples()
    {
        Assert.Equal("PT1S", Eval40("fn:seconds(1)").ToString());
        Assert.Equal("PT0.001S", Eval40("fn:seconds(0.001)").ToString());
        Assert.Equal("PT1M", Eval40("fn:seconds(60)").ToString());
        Assert.Equal("P1D", Eval40("fn:seconds(86400)").ToString());
        Assert.Equal("-PT1H30M", Eval40("fn:seconds(-5400)").ToString());
    }

    [Fact]
    public void Seconds_EmptySequence_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:seconds(())").IsUndefined);
    }

    [Fact]
    public void DurationToSeconds_SpecExamples()
    {
        Assert.Equal("90", Eval40("fn:duration-to-seconds(xs:dayTimeDuration('PT1M30S'))").ToString());
        Assert.Equal("86400.5", Eval40("fn:duration-to-seconds(xs:dayTimeDuration('P1DT0.5S'))").ToString());
        Assert.Equal("-5400", Eval40("fn:duration-to-seconds(xs:dayTimeDuration('-PT1H30M'))").ToString());
    }

    [Fact]
    public void SecondsAndDurationToSeconds_AreInverses()
    {
        Assert.Equal("1234.5", Eval40(
            "fn:duration-to-seconds(fn:seconds(1234.5))").ToString());
        // Unix-timestamp idiom from the spec notes.
        Assert.Equal("1706702400", Eval40(
            "fn:duration-to-seconds(xs:dateTime('2024-01-31T12:00:00Z') - xs:dateTime('1970-01-01T00:00:00Z'))").ToString());
    }

    // ------------------------------------------------------------------
    // fn:build-dateTime (F+O 4.0 §9.4.2)
    // ------------------------------------------------------------------

    [Fact]
    public void BuildDateTime_SpecDateTimeStampExample()
    {
        Assert.Equal("1999-05-31T13:20:00-05:00", Eval40(
            "fn:build-dateTime(map{'year':1999, 'month':5, 'day':31, 'hours':13, 'minutes':20, " +
            "'seconds':0, 'timezone':xs:dayTimeDuration('-PT5H')})").ToString());
    }

    [Fact]
    public void BuildDateTime_AllSevenComponentsWithoutTimezone_IsDateTime()
    {
        Assert.Equal("true", Eval40(
            "fn:build-dateTime(map{'year':1999, 'month':5, 'day':31, 'hours':13, 'minutes':20, 'seconds':0}) " +
            "instance of xs:dateTime").ToString());
    }

    [Fact]
    public void BuildDateTime_TimeWithFractionalSeconds()
    {
        Assert.Equal("13:30:04.25", Eval40(
            "fn:build-dateTime(map{'hours':13, 'minutes':30, 'seconds':4.25})").ToString());
    }

    [Fact]
    public void BuildDateTime_OtherGregorianTypes()
    {
        Assert.Equal("2000-02-29", Eval40(
            "fn:build-dateTime(map{'year':2000, 'month':2, 'day':29})").ToString());
        Assert.Equal("--02-29", Eval40(
            "fn:build-dateTime(map{'day':29, 'month':2})").ToString());
        Assert.Equal("2026-04", Eval40(
            "fn:build-dateTime(map{'year':2026, 'month':4})").ToString());
    }

    [Fact]
    public void BuildDateTime_CoercesNumericComponentTypes()
    {
        Assert.Equal("1999-05-31T13:20:00", Eval40(
            "fn:build-dateTime(map{'year':xs:double(1999), 'month':5, 'day':31, " +
            "'hours':13, 'minutes':20, 'seconds':0})").ToString());
    }

    [Fact]
    public void BuildDateTime_InvalidFieldSet_RaisesFodt0005()
    {
        Assert.Contains("FODT0005", Error40(
            "fn:build-dateTime(map{'year':2000, 'hours':1})").Message);
    }

    [Fact]
    public void BuildDateTime_TimezoneOutOfRange_RaisesFodt0003()
    {
        Assert.Contains("FODT0003", Error40(
            "fn:build-dateTime(map{'year':2000, 'month':1, 'timezone':xs:dayTimeDuration('-PT15H')})").Message);
    }

    [Fact]
    public void BuildDateTime_OutOfRangeComponent_RaisesForg0001()
    {
        Assert.Contains("FORG0001", Error40(
            "fn:build-dateTime(map{'year':2001, 'month':2, 'day':29})").Message);
        Assert.Contains("FORG0001", Error40(
            "fn:build-dateTime(map{'hours':1, 'minutes':2, 'seconds':61})").Message);
    }

    [Fact]
    public void BuildDateTime_EmptySequence_ReturnsEmpty()
    {
        Assert.True(Eval40("fn:build-dateTime(())").IsUndefined);
    }

    // ------------------------------------------------------------------
    // fn:unix-dateTime (F+O 4.0 §9.4.3)
    // ------------------------------------------------------------------

    [Fact]
    public void UnixDateTime_SpecExamples()
    {
        Assert.Equal("1970-01-01T00:00:00Z", Eval40("fn:unix-dateTime()").ToString());
        Assert.Equal("1970-01-01T00:00:00.001Z", Eval40("fn:unix-dateTime(1)").ToString());
        Assert.Equal("1970-01-02T00:00:00Z", Eval40("fn:unix-dateTime(86400000)").ToString());
    }

    [Fact]
    public void UnixDateTime_LargeTimestamp()
    {
        Assert.Equal("2024-01-31T12:00:00Z", Eval40("fn:unix-dateTime(1706702400000)").ToString());
    }

    [Fact]
    public void UnixDateTime_NegativeValue_RaisesFoca0002()
    {
        Assert.Contains("FOCA0002", Error40("fn:unix-dateTime(-1)").Message);
    }

    // ------------------------------------------------------------------
    // fn:days-in-month (F+O 4.0 §9.6.11)
    // ------------------------------------------------------------------

    [Fact]
    public void DaysInMonth_SpecExamples()
    {
        Assert.Equal("29", Eval40("fn:days-in-month(xs:date('2024-02-10'))").ToString());
        Assert.Equal("28", Eval40("fn:days-in-month(xs:date('1900-02-10'))").ToString());
        Assert.Equal("29", Eval40("fn:days-in-month(xs:dateTime('2000-02-29T12:00:00Z'))").ToString());
        Assert.Equal("30", Eval40("fn:days-in-month(xs:gYearMonth('2026-04'))").ToString());
        Assert.True(Eval40("fn:days-in-month(())").IsUndefined);
    }

    [Fact]
    public void DaysInMonth_ProlepticGregorianYearZeroIsLeap()
    {
        Assert.Equal("29", Eval40("fn:days-in-month(xs:gYearMonth('0000-02'))").ToString());
    }

    // ------------------------------------------------------------------
    // fn:some (F+O 4.0 §2.5.16)
    // ------------------------------------------------------------------

    [Fact]
    public void Some_PredicateMatchingOneItem_ReturnsTrue()
    {
        Assert.Equal("true", Eval40("fn:some((1, 2, 3), function($x) { $x = 2 })").ToString());
        Assert.Equal("false", Eval40("fn:some(1 to 10, function($x) { $x gt 10 })").ToString());
    }

    [Fact]
    public void Some_UnprefixedCall_UsesDefaultFunctionNamespace()
    {
        Assert.Equal("true", Eval40("some((1, 2), function($x) { $x + $x = 4 })").ToString());
    }

    [Fact]
    public void Some_DefaultPredicate_UsesEffectiveBooleanValue()
    {
        Assert.Equal("true", Eval40("fn:some((1 = 1, 2 = 2, 3 = 4))").ToString());
        Assert.Equal("false", Eval40("fn:some(('', 0))").ToString());
        Assert.Equal("false", Eval40("fn:some(())").ToString());
    }

    [Fact]
    public void Some_EmptyPredicateArgument_UsesDefault()
    {
        // (:default-on-empty:) — an empty-sequence predicate means fn:boolean#1 (some-empty).
        Assert.Equal("true", Eval40("fn:some((true(), false()), ())").ToString());
    }

    [Fact]
    public void Some_ArityZeroPredicate_IsArityCoerced()
    {
        Assert.Equal("true", Eval40("fn:some(1 to 10, true#0)").ToString());
        Assert.Equal("false", Eval40("fn:some(1 to 10, false#0)").ToString());
    }

    [Fact]
    public void Some_PositionArgument_IsOneBased()
    {
        Assert.Equal("true", Eval40("fn:some(reverse(1 to 5), function($num, $pos) { $num = $pos })").ToString());
    }

    [Fact]
    public void Some_EmptyPredicateResultMeansFalse()
    {
        Assert.Equal("false", Eval40(
            "fn:some((1, 2, 3), function($x) { if ($x = 2) then () else false() })").ToString());
    }

    [Fact]
    public void Some_NonBooleanPredicateResult_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:some(1 to 10, function($x) { $x + 1 })").Message);
    }

    [Fact]
    public void Some_PredicateArityGreaterThanTwo_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40(
            "fn:some(1 to 10, function($x, $y, $z) { $x + 1 })").Message);
    }

    // ------------------------------------------------------------------
    // fn:every (F+O 4.0 §2.5.4)
    // ------------------------------------------------------------------

    [Fact]
    public void Every_AllItemsMatch_ReturnsTrue()
    {
        Assert.Equal("true", Eval40("fn:every((2, 4, 6), function($x) { $x mod 2 = 0 })").ToString());
        Assert.Equal("false", Eval40("fn:every((2, 3, 6), function($x) { $x mod 2 = 0 })").ToString());
    }

    [Fact]
    public void Every_EmptyInput_ReturnsTrue()
    {
        Assert.Equal("true", Eval40("fn:every(())").ToString());
        Assert.Equal("true", Eval40("fn:every((), false#0)").ToString());
    }

    [Fact]
    public void Every_DefaultPredicate_UsesEffectiveBooleanValue()
    {
        Assert.Equal("true", Eval40("fn:every((1 = 1, 2 = 2))").ToString());
        Assert.Equal("false", Eval40("fn:every((1 = 1, 2 = 5))").ToString());
    }

    [Fact]
    public void Every_EmptyPredicateResultMeansFalse()
    {
        Assert.Equal("false", Eval40(
            "fn:every((1, 2, 3), function($x) { if ($x lt 3) then true() else () })").ToString());
        Assert.Equal("false", Eval40("fn:every((1, 2), function($x) { () })").ToString());
    }

    [Fact]
    public void Every_NonBooleanPredicateResult_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:every(1 to 5, function($x) { $x + 1 })").Message);
    }

    // ------------------------------------------------------------------
    // fn:index-where (F+O 4.0 §2.5.11)
    // ------------------------------------------------------------------

    [Fact]
    public void IndexWhere_ReturnsAscendingPositions()
    {
        Assert.Equal(["30", "60", "90"], Seq40(
            "fn:index-where(1 to 100, function($x) { $x mod 30 = 0 })"));
        Assert.Equal(["2", "3"], Seq40("fn:index-where((0, 4, 9), boolean#1)"));
        Assert.Equal([], Seq40("fn:index-where(1 to 100, function($x) { $x lt 0 })"));
    }

    [Fact]
    public void IndexWhere_EmptyInput_ReturnsEmpty()
    {
        Assert.Equal([], Seq40("fn:index-where((), boolean#1)"));
    }

    [Fact]
    public void IndexWhere_PositionArgument_IsOneBased()
    {
        Assert.Equal(["6", "7"], Seq40(
            "fn:index-where(('January', 'February', 'March', 'April', 'May', 'June', 'July')," +
            " function($it, $pos) { $pos gt 5 })"));
    }

    [Fact]
    public void IndexWhere_EmptyPredicateResultMeansFalse()
    {
        Assert.Equal(["2", "4"], Seq40(
            "fn:index-where((1, 2, 3, 2), function($x) { if ($x = 2) then true() else () })"));
    }

    [Fact]
    public void IndexWhere_EmptyPredicate_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:index-where((1, 2), ())").Message);
    }

    // ------------------------------------------------------------------
    // fn:partition (F+O 4.0 §2.5.14)
    // ------------------------------------------------------------------

    [Fact]
    public void Partition_FixedSizeGroups()
    {
        Assert.Equal(["2", "2", "2"], Seq40(
            "fn:partition((1, 2, 3, 4, 5, 6), function($a, $b) { count($a) eq 2 }) ! array:size(.)"));
        Assert.Equal(["1,2", "3,4", "5,6"], Seq40(
            "fn:partition((1, 2, 3, 4, 5, 6), function($a, $b) { count($a) eq 2 })" +
            " ! string-join(?*, ',')"));
    }

    [Fact]
    public void Partition_SplitsOnAscendingRunBoundary()
    {
        Assert.Equal(["846,23,5", "8,6", "1000"], Seq40(
            "fn:partition((846, 23, 5, 8, 6, 1000), function($seq, $curr) { $curr > $seq })" +
            " ! string-join(?*, ',')"));
    }

    [Fact]
    public void Partition_SplitsOnValueChange()
    {
        Assert.Equal(["1,1", "2", "1"], Seq40(
            "fn:partition((1, 1, 2, 1), function($seq, $curr) { not($seq = $curr) })" +
            " ! string-join(?*, ',')"));
    }

    [Fact]
    public void Partition_ArityOneCallback_ReceivesPartitionOnly()
    {
        Assert.Equal(["In the", "beginning was", "the word"], Seq40(
            "fn:partition(tokenize('In the beginning was the word'), function($a) { count($a) = 2 })" +
            " ! string-join(?*, ' ')"));
    }

    [Fact]
    public void Partition_EmptyInput_ReturnsEmpty()
    {
        Assert.Equal([], Seq40("fn:partition((), function($a, $b) { true() })"));
        Assert.Equal(["1"], Seq40(
            "fn:partition(1, function($a, $b) { true() }) ! string-join(?*, ',')"));
    }

    [Fact]
    public void Partition_NeverSplits_SinglePartition()
    {
        Assert.Equal(["1000"], Seq40(
            "fn:partition(1 to 1000, function($a, $b) { false() }) ! string(array:size(.))"));
    }

    // ------------------------------------------------------------------
    // fn:take-while (F+O 4.0 §2.5.21)
    // ------------------------------------------------------------------

    [Fact]
    public void TakeWhile_StopsAtFirstNonMatch()
    {
        Assert.Equal(["1", "2", "3", "4", "5", "6", "7", "8", "9"], Seq40(
            "fn:take-while(1 to 29, function($x) { $x != 10 })"));
        Assert.Equal([], Seq40("fn:take-while(1 to 29, function($x) { false() })"));
        Assert.Equal([], Seq40("fn:take-while((), function($x) { $x = 3 })"));
    }

    [Fact]
    public void TakeWhile_AllMatch_ReturnsWholeInput()
    {
        Assert.Equal(["1", "2", "3"], Seq40("fn:take-while(1 to 3, function($x) { true() })"));
    }

    [Fact]
    public void TakeWhile_MapAsPredicate_LooksUpEachItem()
    {
        Assert.Equal(["1", "2"], Seq40(
            "fn:take-while(1 to 5, map{1:true(), 2:true(), 3:false(), 4:false(), 5:true()})"));
    }

    [Fact]
    public void TakeWhile_EmptyPredicateResultMeansFalse()
    {
        Assert.Equal(["1", "2"], Seq40(
            "fn:take-while(1 to 5, function($x) { if ($x < 3) then true() else () })"));
    }

    [Fact]
    public void TakeWhile_NonBooleanPredicateResult_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:take-while(1 to 5, function($x) { $x - 1 })").Message);
    }

    // ------------------------------------------------------------------
    // fn:drop-while (F+O 4.0 §2.5.3)
    // ------------------------------------------------------------------

    [Fact]
    public void DropWhile_DropsPrefixUpToFirstNonMatch()
    {
        Assert.Equal(["13", "14", "15", "16", "17", "18", "19", "20"], Seq40(
            "fn:drop-while(10 to 20, function($x) { $x le 12 })"));
        Assert.Equal(["5", "2", "6"], Seq40(
            "fn:drop-while((1, 5, 2, 6), function($x) { $x lt 4 })"));
    }

    [Fact]
    public void DropWhile_AllMatch_ReturnsEmpty()
    {
        Assert.Equal([], Seq40("fn:drop-while(1 to 5, function($x) { $x lt 100 })"));
        Assert.Equal([], Seq40("fn:drop-while(1 to 5, true#0)"));
        Assert.Equal([], Seq40("fn:drop-while((), boolean#1)"));
    }

    [Fact]
    public void DropWhile_EmptyPredicateResultMeansFalse()
    {
        Assert.Equal(["1", "2", "3", "4", "5"], Seq40("fn:drop-while(1 to 5, function($x) { () })"));
        Assert.Equal(["1", "2", "3", "4", "5"], Seq40("fn:drop-while(1 to 5, false#0)"));
    }

    [Fact]
    public void DropWhile_EmptyPredicate_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:drop-while(1 to 5, ())").Message);
    }

    [Fact]
    public void DropWhile_TakeWhile_ReconstructInput()
    {
        Assert.Equal(["1", "2", "3", "4", "5"], Seq40(
            "(fn:take-while(1 to 5, function($x) { $x lt 3 })," +
            " fn:drop-while(1 to 5, function($x) { $x lt 3 }))"));
    }

    // ------------------------------------------------------------------
    // fn:while-do (F+O 4.0 §2.5.23)
    // ------------------------------------------------------------------

    [Fact]
    public void WhileDo_IteratesUntilPredicateFalse()
    {
        Assert.Equal("10000", Eval40(
            "fn:while-do(1, function($x) { $x lt 10000 }, function($x) { $x + 1 })").ToString());
        Assert.Equal("65536", Eval40(
            "fn:while-do(2, function($x) { $x lt 1000 }, function($x) { $x * $x })").ToString());
    }

    [Fact]
    public void WhileDo_PredicateInitiallyFalse_NeverCallsAction()
    {
        Assert.Equal("1", Eval40("fn:while-do(1, not#1, function($_) { error() })").ToString());
        Assert.Equal(["1", "2"], Seq40("fn:while-do((1, 2), false#0, function($x) { $x })"));
    }

    [Fact]
    public void WhileDo_ActionError_Propagates()
    {
        Assert.ThrowsAny<Exception>(() => Eval40(
            "fn:while-do(1, boolean#1, function($_) { error() })"));
    }

    [Fact]
    public void WhileDo_WholeSequenceIsTheValue()
    {
        Assert.Equal(["2", "3", "4"], Seq40(
            "fn:while-do((6 to 8), function($s) { sum($s) gt 10 }, function($s) { $s ! (. - 1) })"));
    }

    [Fact]
    public void WhileDo_PositionIncrementsFromOne()
    {
        Assert.Equal("8", Eval40(
            "fn:while-do(1, function($x, $p) { $p le 3 }, function($x) { $x * 2 })").ToString());
    }

    [Fact]
    public void WhileDo_EmptyPredicateResultMeansFalse()
    {
        Assert.Equal("4", Eval40(
            "fn:while-do(1, function($x) { if ($x lt 4) then true() else () }, function($x) { $x + 1 })").ToString());
    }

    // ------------------------------------------------------------------
    // fn:do-until (F+O 4.0 §2.5.2)
    // ------------------------------------------------------------------

    [Fact]
    public void DoUntil_IteratesUntilPredicateTrue()
    {
        Assert.Equal("10000", Eval40(
            "fn:do-until(1, function($x) { $x + 1 }, function($x) { $x ge 10000 })").ToString());
        Assert.Equal("65536", Eval40(
            "fn:do-until(2, function($x) { $x * $x }, function($x) { $x ge 1000 })").ToString());
    }

    [Fact]
    public void DoUntil_ActionEvaluatedBeforePredicate()
    {
        // The action runs on the initial value even when it already satisfies the predicate.
        Assert.ThrowsAny<Exception>(() => Eval40("fn:do-until(1, error#0, boolean#1)"));
        Assert.Equal("1", Eval40("fn:do-until(1, function($x) { $x }, exists#1)").ToString());
    }

    [Fact]
    public void DoUntil_EmptyInput_StillRunsAction()
    {
        // string(()) is "" — a single (zero-length) item — so exists("") is true and the
        // result is that empty string (do-until-003).
        Assert.Equal("", Eval40("fn:do-until((), string#1, exists#1)").ToString());
    }

    [Fact]
    public void DoUntil_PositionIncrementsFromOne()
    {
        Assert.Equal("8", Eval40(
            "fn:do-until(1, function($x, $p) { $x * 2 }, function($x, $p) { if ($p = 3) then true() else () })").ToString());
    }

    // ------------------------------------------------------------------
    // fn:partial-apply (F+O 4.0 §2.5.13)
    // ------------------------------------------------------------------

    [Fact]
    public void PartialApply_SpecExamples()
    {
        Assert.Equal("1.2", Eval40("fn:partial-apply(round#2, map{2: 1})(1.23456)").ToString());
        Assert.Equal("ow", Eval40("fn:partial-apply(substring#3, map{2: 3, 3: 2})('flower')").ToString());
        Assert.Equal("12, 13, 14", Eval40(
            "fn:partial-apply(string-join#2, map{2: ', '})((12, 13, 14))").ToString());
    }

    [Fact]
    public void PartialApply_InlineFunctionBase()
    {
        Assert.Equal("99", Eval40(
            "let $f := function($a, $b, $c) { $a + $b + $c }" +
            " return fn:partial-apply($f, map{2: 22})(33, 44)").ToString());
    }

    [Fact]
    public void PartialApply_EmptyMap_ReturnsFunctionUnchanged()
    {
        Assert.Equal("true", Eval40("fn:partial-apply(true#0, map{})()").ToString());
    }

    [Fact]
    public void PartialApply_BindingAllArguments_YieldsZeroArityFunction()
    {
        Assert.Equal("ab", Eval40("fn:partial-apply(concat#2, map{1: 'a', 2: 'b'})()").ToString());
    }

    [Fact]
    public void PartialApply_KeysBeyondArity_AreIgnored()
    {
        Assert.Equal("1", Eval40("fn:partial-apply(count#1, map{1: 'x', 2: 'y'})()").ToString());
    }

    [Fact]
    public void PartialApply_ArrayAsFunction()
    {
        Assert.Equal("14", Eval40("fn:partial-apply(array{10 to 20}, map{1: 5})()").ToString());
    }

    [Fact]
    public void PartialApply_BoundValueCoercion_FailsAtCallTimeWhenNoTypeMetadata()
    {
        // ('a', 'b') cannot coerce to the xs:string parameter of fn:string-length; the
        // function's registration carries no parameter-type metadata, so the error
        // surfaces when the resulting zero-arity function is called (spec: the error
        // "may be raised" at bind or call time).
        Assert.Contains("XPTY0004", Error40("fn:partial-apply(string-length#1, map{1: ('a', 'b') })()").Message);
    }

    [Fact]
    public void PartialApply_NonIntegerKey_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:partial-apply(concat#8, map{'1': '---'})").Message);
    }

    [Fact]
    public void PartialApply_NonFunction_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:partial-apply(42, map{})").Message);
    }

    // ------------------------------------------------------------------
    // fn:transitive-closure (F+O 4.0 §2.5.22)
    // ------------------------------------------------------------------

    [Fact]
    public void TransitiveClosure_ReachesAllDescendants()
    {
        Assert.Equal("m a b n c", Eval40(
            "string-join(" +
            "  fn:transitive-closure(fn:parse-xml('<r><m><a/><b/></m><n><c/></n></r>')/r," +
            "                        function($n) { $n/* }) / name(.), ' ')").ToString());
    }

    [Fact]
    public void TransitiveClosure_ResultExcludesStartNode()
    {
        Assert.Equal("2", Eval40(
            "count(fn:transitive-closure(fn:parse-xml('<doc><a><b/><c/></a></doc>')/doc/a," +
            "                            function($n) { $n/* }))").ToString());
    }

    [Fact]
    public void TransitiveClosure_DuplicatesAndCycles_Terminate()
    {
        // A step returning duplicates still yields each node once; a parent-axis step
        // walks back to the root without looping.
        Assert.Equal("2", Eval40(
            "count(fn:transitive-closure(fn:parse-xml('<doc><a><b/><c/></a></doc>')/doc/a," +
            "                            function($n) { ($n/*, $n/*) }))").ToString());
        Assert.Equal("2", Eval40(
            "count(fn:transitive-closure(fn:parse-xml('<doc><a><b/></a></doc>')/doc/a/b," +
            "                            function($n) { $n/parent::* }))").ToString());
    }

    [Fact]
    public void TransitiveClosure_EmptyNode_ReturnsEmpty()
    {
        Assert.Equal([], Seq40("fn:transitive-closure((), function($n) { $n })"));
    }

    [Fact]
    public void TransitiveClosure_NonNode_RaisesXpty0004()
    {
        Assert.Contains("XPTY0004", Error40("fn:transitive-closure(42, fn:root#1)").Message);
    }

    // ------------------------------------------------------------------
    // Keyword-argument composition with the S3b machinery (F+O 4.0 §4.6.1)
    // ------------------------------------------------------------------

    [Fact]
    public void KeywordArguments_SomeWithDefaultPredicate()
    {
        Assert.Equal("true", Eval40("fn:some(input := (true(), false()))").ToString());
        Assert.Equal("true", Eval40(
            "fn:some(input := (1, 2, 3), predicate := function($x) { $x = 2 })").ToString());
    }

    [Fact]
    public void KeywordArguments_DropWhileAndWhileDo()
    {
        Assert.Equal(["5", "2", "6"], Seq40(
            "fn:drop-while(input := (1, 5, 2, 6), predicate := function($x) { $x lt 4 })"));
        Assert.Equal("5", Eval40(
            "fn:while-do(input := 1, predicate := function($x) { $x lt 5 }," +
            "            action := function($x) { $x + 1 })").ToString());
    }

    [Fact]
    public void KeywordArguments_PartialApply()
    {
        Assert.Equal("1.2", Eval40(
            "(fn:partial-apply(function := fn:round#2, arguments := map{2: 1}))(1.23456)").ToString());
    }

    // fn:identity (4.0-S5 follow-up — registered because map:build/array:build
    // keyword defaults reference fn:identity#1)

    [Fact]
    public void Identity_ReturnsArgumentUnchanged()
    {
        Assert.Equal(["1", "2"], Seq40("fn:identity((1, 2))"));
        Assert.Equal("a", Eval40("fn:identity('a')").ToString());
    }

    [Fact]
    public void KeywordArguments_UnfilledKeyword_ResolvesIdentityDefault()
    {
        // The keyword expansion path resolves unfilled keywords through the signature
        // default snippets (here fn:identity#1 for map:build's $key).
        Assert.Equal("a", Eval40("map:get(map:build(input := ('a', 'b'), value := fn:string#1), 'a')").ToString());
    }
}
