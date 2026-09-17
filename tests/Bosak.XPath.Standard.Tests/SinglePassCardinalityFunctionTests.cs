// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 September 2026
// PURPOSE              : Unit tests for cardinality functions over single-pass (streamed) sequences
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
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

public class SinglePassCardinalityFunctionTests
{
    private static XdmValue Evaluate(string expression, params XdmValue[] items)
    {
        var compiled = XPath31Expression.Compile(expression);
        var value = XdmValue.FromSequence(XdmSequence.FromSource(new FakeSinglePassSequence(items)));
        var ctx = new EvaluationContext().WithVariable("p", value);
        return compiled.Evaluate(ctx);
    }

    private static XdmValue NodeItem(string xml = "<a/>")
        => XdmValue.FromNode(new XDocumentNode(System.Xml.Linq.XElement.Parse(xml)));

    [Theory]
    [InlineData(new object[] { 1, 2 }, 2)]
    [InlineData(new object[] { 1 }, 1)]
    public void OneOrMore_SinglePass_ReturnsAllItems(object[] rawItems, long expectedCount)
    {
        var items = rawItems.Select(ToValue).ToArray();
        Assert.Equal(expectedCount, Evaluate("count(fn:one-or-more($p))", items).IntegerValue);
    }

    [Fact]
    public void OneOrMore_SinglePassEmpty_RaisesForg0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:one-or-more($p)"));
        Assert.Contains("FORG0004", ex.Message);
    }

    [Fact]
    public void ZeroOrOne_SinglePassEmpty_ReturnsEmpty()
    {
        Assert.Equal(0, Evaluate("count(fn:zero-or-one($p))").IntegerValue);
    }

    [Fact]
    public void ZeroOrOne_SinglePassSingleItem_ReturnsTheItem()
    {
        Assert.True(Evaluate("fn:zero-or-one($p) eq 42", XdmValue.FromInteger(42)).BooleanValue);
    }

    [Fact]
    public void ZeroOrOne_SinglePassMultipleItems_RaisesForg0003()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("fn:zero-or-one($p)", XdmValue.FromInteger(1), XdmValue.FromInteger(2)));
        Assert.Contains("FORG0003", ex.Message);
    }

    [Fact]
    public void ExactlyOne_SinglePassSingleItem_ReturnsTheItem()
    {
        Assert.True(Evaluate("fn:exactly-one($p) eq 7", XdmValue.FromInteger(7)).BooleanValue);
    }

    [Fact]
    public void ExactlyOne_SinglePassEmpty_RaisesForg0005()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:exactly-one($p)"));
        Assert.Contains("FORG0005", ex.Message);
    }

    [Fact]
    public void ExactlyOne_SinglePassMultipleItems_RaisesForg0005()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("fn:exactly-one($p)", XdmValue.FromInteger(1), XdmValue.FromInteger(2)));
        Assert.Contains("FORG0005", ex.Message);
    }

    [Theory]
    [InlineData(new object[] { }, false)]
    [InlineData(new object[] { 1 }, true)]
    [InlineData(new object[] { 0 }, false)]
    public void Boolean_SinglePass_DecidesInOneEnumeration(object[] rawItems, bool expected)
    {
        var items = rawItems.Select(ToValue).ToArray();
        Assert.Equal(expected, Evaluate("fn:boolean($p)", items).BooleanValue);
    }

    [Fact]
    public void Boolean_SinglePassNode_IsTrue()
    {
        Assert.True(Evaluate("fn:boolean($p)", NodeItem()).BooleanValue);
    }

    [Fact]
    public void Boolean_SinglePassTwoAtomics_RaisesForg0006()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("fn:boolean($p)", XdmValue.FromInteger(1), XdmValue.FromInteger(2)));
        Assert.Contains("FORG0006", ex.Message);
    }

    private static XdmValue ToValue(object raw)
        => raw switch
        {
            int i => XdmValue.FromInteger(i),
            string s => XdmValue.FromString(s),
            _ => throw new ArgumentException($"Unsupported test item: {raw}", nameof(raw)),
        };

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
