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
}
