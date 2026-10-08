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
}
