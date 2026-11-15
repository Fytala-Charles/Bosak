// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 November 2026
// PURPOSE              : Verifies lazy filter evaluation, errors, identity and isolated allocation budgets
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 15-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Reflection;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Verifies lazy filter evaluation, errors, identity and isolated allocation budgets.</summary>
public sealed class LazyFilterSequenceTests
{
    private static readonly Func<XdmValue, Func<IXdmNode, bool>, XdmValue> Filter = typeof(EvaluationContext).Assembly
        .GetType("Bosak.XPath.Runtime.Vm.VmEngine", true)!.GetMethod("FilterNodesLazy", BindingFlags.NonPublic | BindingFlags.Static)!
        .CreateDelegate<Func<XdmValue, Func<IXdmNode, bool>, XdmValue>>();
    private static XdmValue Node(string name) => XdmValue.FromNode(new XDocumentNode(new XElement(name)));
    private static XdmValue Value(IXdmSequence source) => XdmValue.FromSequence(XdmSequence.FromSource(source));
    private static int Count(XdmValue value) { int n = 0; foreach (var _ in XdmSequence.FromSource(value.SequenceValue!)) n++; return n; }

    /// <summary>Checks deferred acquisition, mixed-item filtering and retained Current values.</summary>
    [Fact]
    public void Filter_DefersAcquisitionAndPreservesNodeIdentityAndCurrent()
    {
        var a = Node("a"); var b = Node("b");
        var source = new Source(new() { XdmValue.FromString("skip"), a, XdmValue.FromBoolean(true), b });
        int calls = 0; var value = Filter(Value(source), _ => { calls++; return true; });
        Assert.Equal(0, source.Gets); Assert.Equal(0, calls);
        Assert.False(value.SequenceValue!.TryGetLength(out var length)); Assert.Equal(0, length);
        var e = value.SequenceValue.GetEnumerator(); Assert.True(e.Current.IsUndefined);
        Assert.Equal(0, source.Gets); Assert.Equal(0, calls);
        Assert.True(e.MoveNext()); Assert.Same(a.NodeValue, e.Current.NodeValue);
        Assert.True(e.MoveNext()); Assert.Same(b.NodeValue, e.Current.NodeValue); Assert.Equal(2, calls);
        Assert.False(e.MoveNext()); var moves = source.Moves;
        Assert.Same(b.NodeValue, e.Current.NodeValue); Assert.False(e.MoveNext()); Assert.Equal(moves, source.Moves);
        Assert.Equal(0, source.Disposals);
    }

    /// <summary>Checks error timing, retained Current and terminal state after failure.</summary>
    /// <param name="mode">The source or predicate failure location.</param>
    [Theory]
    [InlineData("acquire")]
    [InlineData("move")]
    [InlineData("current")]
    [InlineData("predicate")]
    public void Filter_DefersErrorsAndStopsPullingAfterFailure(string mode)
    {
        var a = Node("a"); var source = new Source(new() { a, Node("b") }, mode);
        var value = Filter(Value(source), n => mode == "predicate" && n.LocalName == "b"
            ? throw new InvalidOperationException("predicate-marker") : true);
        var e = value.SequenceValue!.GetEnumerator(); Assert.Equal(0, source.Gets);
        if (mode != "acquire") Assert.True(e.MoveNext());
        Assert.Equal(mode + "-marker", Assert.Throws<InvalidOperationException>(() => e.MoveNext()).Message);
        if (mode == "acquire") Assert.True(e.Current.IsUndefined); else Assert.Same(a.NodeValue, e.Current.NodeValue);
        var gets = source.Gets; var moves = source.Moves;
        Assert.False(e.MoveNext()); Assert.Equal(gets, source.Gets); Assert.Equal(moves, source.Moves);
        Assert.Equal(0, source.Disposals);
    }

    /// <summary>Checks source and XML mutations before first use and replay.</summary>
    [Fact]
    public void OrdinaryFilter_ObservesMutationAndRejectedPredicates()
    {
        var element = new XElement("a", "before"); var a = XdmValue.FromNode(new XDocumentNode(element));
        var items = new List<XdmValue> { a }; var value = Filter(Value(new Source(items)), static n => n.StringValue == "after");
        element.Value = "after"; Assert.Equal(1, Count(value));
        items.Add(XdmValue.FromNode(new XDocumentNode(new XElement("b", "after")))); Assert.Equal(2, Count(value));
        items.Clear(); Assert.Equal(0, Count(value));
        var rejected = Filter(Value(new Source(new() { a })), static _ => false).SequenceValue!.GetEnumerator();
        Assert.False(rejected.MoveNext()); Assert.True(rejected.Current.IsUndefined);
    }

    /// <summary>Checks marker propagation, refusal and atomic simultaneous acquisition.</summary>
    [Fact]
    public void MarkedFilter_EnforcesAtomicSinglePassAcquisition()
    {
        foreach (bool pull in new[] { false, true })
        {
            var seq = Filter(Value(new MarkedSource(new() { Node("a") })), static _ => true).SequenceValue!;
            Assert.IsAssignableFrom<ISinglePassSequence>(seq); var first = seq.GetEnumerator();
            if (pull) Assert.True(first.MoveNext());
            Assert.Contains("already been consumed", Assert.Throws<StreamingException>(() => seq.GetEnumerator()).Message);
        }
        var input = new MarkedSource(new() { Node("a") }); var output = Filter(Value(input), static _ => true).SequenceValue!;
        using var ready = new CountdownEvent(2); using var start = new ManualResetEventSlim();
        int accepted = 0, refused = 0; Exception? unexpected = null;
        var threads = Enumerable.Range(0, 2).Select(_ => new Thread(() => {
            try { ready.Signal(); start.Wait(); output.GetEnumerator(); Interlocked.Increment(ref accepted); }
            catch (StreamingException) { Interlocked.Increment(ref refused); }
            catch (Exception error) { unexpected = error; }
        })).ToArray();
        foreach (var t in threads) t.Start();
        var allReady = ready.Wait(10000); start.Set();
        foreach (var t in threads) Assert.True(t.Join(10000));
        Assert.True(allReady); Assert.Null(unexpected); Assert.Equal(1, accepted); Assert.Equal(1, refused); Assert.Equal(0, input.Gets);
    }

    /// <summary>Checks scalar, atomic and empty input evaluation timing.</summary>
    [Fact]
    public void ScalarAndEmptyInputs_KeepExistingEvaluationTiming()
    {
        foreach (var input in new[] { default(XdmValue), XdmValue.FromInteger(1), XdmValue.FromSequence(default) })
            Assert.Equal(0, Count(Filter(input, static _ => throw new InvalidOperationException("unexpected"))));
        int calls = 0; var a = Node("a"); var value = Filter(a, _ => { calls++; return true; });
        Assert.Equal(1, calls); Assert.Equal(1, Count(value)); Assert.Equal(0, Count(Filter(a, static _ => false)));
        Assert.Equal(0, Count(Filter(Value(new Source(new())), static _ => false)));
    }

    /// <summary>Checks null source enumerators complete without repeated acquisition.</summary>
    [Fact]
    public void NullSourceEnumerator_CompletesWithoutRepeatedAcquisition()
    {
        var source = new Source(new(), "null"); var e = Filter(Value(source), static _ => true).SequenceValue!.GetEnumerator();
        Assert.False(e.MoveNext()); Assert.False(e.MoveNext()); Assert.Equal(1, source.Gets); Assert.True(e.Current.IsUndefined);
    }

    /// <summary>Checks fixed budgets reject legacy filter wrapper allocation costs.</summary>
    [Fact]
    public void Allocations_StayWithinPreRecordedIsolatedBudgets()
    {
        foreach (var row in AllocationProbeRunner.Run("filtering"))
        {
            var cap = row.Key switch { "create" => 48L, "create-marked" => 56L, "once" => 192L, _ => 160L };
            Assert.InRange(row.Value, 0L, cap * 100000);
        }
    }

    private class Source : IXdmSequence
    {
        private readonly List<XdmValue> _items; private readonly string _fault;
        internal int Gets, Moves, Disposals;
        internal Source(List<XdmValue> items, string fault = "none") { _items = items; _fault = fault; }
        /// <inheritdoc/>
        public bool TryGetLength(out int length) { length = 0; return false; }
        /// <inheritdoc/>
        public virtual IXdmSequenceEnumerator GetEnumerator()
        {
            Gets++; if (_fault == "acquire") throw new InvalidOperationException("acquire-marker");
            return _fault == "null" ? null! : new Enumerator(this);
        }
        private sealed class Enumerator : IXdmSequenceEnumerator, IDisposable
        {
            private readonly Source _owner; private int _index = -1;
            internal Enumerator(Source owner) => _owner = owner;
            /// <inheritdoc/>
            public XdmValue Current => _owner._fault == "current" && _index == 1
                ? throw new InvalidOperationException("current-marker") : _owner._items[_index];
            /// <inheritdoc/>
            public bool MoveNext()
            {
                _owner.Moves++; _index++;
                if (_owner._fault == "move" && _index == 1) throw new InvalidOperationException("move-marker");
                return _index < _owner._items.Count;
            }
            /// <inheritdoc/>
            public void Dispose() => _owner.Disposals++;
        }
    }
    private sealed class MarkedSource : Source, ISinglePassSequence
    {
        private int _taken;
        internal MarkedSource(List<XdmValue> items) : base(items) { }
        /// <inheritdoc/>
        public override IXdmSequenceEnumerator GetEnumerator()
        {
            if (Interlocked.Exchange(ref _taken, 1) != 0) throw new StreamingException("Test source consumed");
            return base.GetEnumerator();
        }
    }
}
