// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 September 2026
// PURPOSE              : Unit tests for instance-of and treat-as over single-pass (streamed) sequences
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

namespace Bosak.XPath.Runtime.Tests;

public class SinglePassInstanceOfTests
{
    private static bool EvaluateBoolean(string expression, params XdmValue[] items)
    {
        var compiled = XPath31Expression.Compile(expression);
        var value = XdmValue.FromSequence(XdmSequence.FromSource(new FakeSinglePassSequence(items)));
        var ctx = new EvaluationContext().WithVariable("p", value);
        return compiled.Evaluate(ctx).BooleanValue;
    }

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
    // occurrence *
    [InlineData("$p instance of xs:integer*", new object[] { 1, 2, 3 }, true)]
    [InlineData("$p instance of xs:integer*", new object[] { }, true)]
    [InlineData("$p instance of xs:integer*", new object[] { 1, "x" }, false)]
    // occurrence +
    [InlineData("$p instance of xs:integer+", new object[] { }, false)]
    [InlineData("$p instance of xs:integer+", new object[] { 1 }, true)]
    [InlineData("$p instance of xs:integer+", new object[] { 1, 2 }, true)]
    [InlineData("$p instance of xs:integer+", new object[] { 1, "x" }, false)]
    // occurrence ?
    [InlineData("$p instance of xs:integer?", new object[] { }, true)]
    [InlineData("$p instance of xs:integer?", new object[] { 1 }, true)]
    [InlineData("$p instance of xs:integer?", new object[] { 1, 2 }, false)]
    // occurrence 1
    [InlineData("$p instance of xs:integer", new object[] { }, false)]
    [InlineData("$p instance of xs:integer", new object[] { 1 }, true)]
    [InlineData("$p instance of xs:integer", new object[] { 1, 2 }, false)]
    [InlineData("$p instance of xs:integer", new object[] { "x" }, false)]
    public void InstanceOf_SinglePass_DecidesInOneEnumeration(string expression, object[] rawItems, bool expected)
    {
        var items = rawItems.Select(ToValue).ToArray();
        Assert.Equal(expected, EvaluateBoolean(expression, items));
    }

    [Fact]
    public void InstanceOf_SinglePassNodes_NodeKindTest()
    {
        Assert.True(EvaluateBoolean("$p instance of node()*", NodeItem(), NodeItem()));
        Assert.False(EvaluateBoolean("$p instance of node()*", NodeItem(), XdmValue.FromInteger(1)));
    }

    [Fact]
    public void TreatAs_SinglePassValid_PassesThroughLazily()
    {
        var result = Evaluate("count($p treat as xs:integer*)", XdmValue.FromInteger(1), XdmValue.FromInteger(2));
        Assert.Equal(2, result.IntegerValue);
    }

    [Fact]
    public void TreatAs_SinglePassWrongItemType_RaisesXpdy0050OnEnumeration()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("count($p treat as xs:integer*)", XdmValue.FromInteger(1), XdmValue.FromString("x")));
        Assert.Contains("XPDY0050", ex.Message);
    }

    [Fact]
    public void TreatAs_SinglePassWrongCardinality_RaisesXpdy0050OnEnumeration()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("count($p treat as xs:integer)", XdmValue.FromInteger(1), XdmValue.FromInteger(2)));
        Assert.Contains("XPDY0050", ex.Message);
    }

    [Fact]
    public void TreatAs_SinglePassEmptyForPlus_RaisesXpdy0050OnEnumeration()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("count($p treat as xs:integer+)", new XdmValue[0]));
        Assert.Contains("XPDY0050", ex.Message);
    }

    [Fact]
    public void TreatAs_SinglePassEmptyForOptional_Passes()
    {
        var result = Evaluate("count($p treat as xs:integer?)", new XdmValue[0]);
        Assert.Equal(0, result.IntegerValue);
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
