// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 September 2026
// PURPOSE              : Unit tests for the effective boolean value of single-pass (streamed) sequences
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 17-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.XPath.Core.Tests;

public class SinglePassSequenceEbvTests
{
    private static XdmValue SinglePassValue(params XdmValue[] items)
        => XdmValue.FromSequence(XdmSequence.FromSource(new FakeSinglePassSequence(items)));

    private static XdmValue NodeItem(string xml = "<a/>")
        => XdmValue.FromNode(new XDocumentNode(System.Xml.Linq.XElement.Parse(xml)));

    [Fact]
    public void Ebv_EmptySinglePassSequence_IsFalse()
    {
        Assert.False(SinglePassValue().EffectiveBooleanValue());
    }

    [Fact]
    public void Ebv_SingleNode_IsTrue()
    {
        Assert.True(SinglePassValue(NodeItem()).EffectiveBooleanValue());
    }

    [Fact]
    public void Ebv_MultipleNodes_IsTrue()
    {
        Assert.True(SinglePassValue(NodeItem("<a/>"), NodeItem("<b/>")).EffectiveBooleanValue());
    }

    [Theory]
    [InlineData("x", true)]
    [InlineData("", false)]
    public void Ebv_SingleString_MatchesStringRules(string value, bool expected)
    {
        Assert.Equal(expected, SinglePassValue(XdmValue.FromString(value)).EffectiveBooleanValue());
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void Ebv_SingleInteger_MatchesNumericRules(long value, bool expected)
    {
        Assert.Equal(expected, SinglePassValue(XdmValue.FromInteger(value)).EffectiveBooleanValue());
    }

    [Theory]
    [InlineData(1.5, true)]
    [InlineData(0.0, false)]
    public void Ebv_SingleDouble_MatchesNumericRules(double value, bool expected)
    {
        Assert.Equal(expected, SinglePassValue(XdmValue.FromDouble(value)).EffectiveBooleanValue());
    }

    [Fact]
    public void Ebv_SingleNaN_IsFalse()
    {
        Assert.False(SinglePassValue(XdmValue.FromDouble(double.NaN)).EffectiveBooleanValue());
    }

    [Fact]
    public void Ebv_TwoAtomicItems_RaisesForg0006()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => SinglePassValue(XdmValue.FromInteger(1), XdmValue.FromInteger(2)).EffectiveBooleanValue());
        Assert.Contains("FORG0006", ex.Message);
    }

    [Fact]
    public void Ebv_NodeFollowedByAtomic_IsTrue()
    {
        // A sequence whose first item is a node has EBV true regardless of later items.
        Assert.True(SinglePassValue(NodeItem(), XdmValue.FromInteger(1)).EffectiveBooleanValue());
    }

    [Fact]
    public void SinglePassSequence_SecondEnumeration_Throws()
    {
        var sequence = new FakeSinglePassSequence(XdmValue.FromInteger(1));
        var value = XdmValue.FromSequence(XdmSequence.FromSource(sequence));
        Assert.True(value.EffectiveBooleanValue());
        Assert.Throws<StreamingException>(() => value.EffectiveBooleanValue());
    }

    /// <summary>
    /// A one-shot <see cref="ISinglePassSequence"/> test double: the first enumeration
    /// succeeds, a second <see cref="IXdmSequence.GetEnumerator"/> throws.
    /// </summary>
    private sealed class FakeSinglePassSequence : ISinglePassSequence
    {
        private readonly XdmValue[] _items;
        private int _taken;

        internal FakeSinglePassSequence(params XdmValue[] items)
            => _items = items;

        public bool TryGetLength(out int length)
        {
            length = 0;
            return false;
        }

        public IXdmSequenceEnumerator GetEnumerator()
        {
            if (Interlocked.Exchange(ref _taken, 1) != 0)
            {
                throw new StreamingException(
                    "Streaming: a streamed sequence is forward-only and has already been consumed.");
            }
            return new OneShotEnumerator(_items);
        }

        private sealed class OneShotEnumerator : IXdmSequenceEnumerator
        {
            private readonly XdmValue[] _items;
            private int _index = -1;

            internal OneShotEnumerator(XdmValue[] items)
                => _items = items;

            public XdmValue Current => _items[_index];

            public bool MoveNext() => ++_index < _items.Length;
        }
    }
}
