// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 November 2026
// PURPOSE              : Measures isolated lazy filter allocation regression workloads
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
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.AllocationChecks;

/// <summary>Measures fixed lazy filter allocation budgets in the isolated worker.</summary>
internal static class LazyFilterAllocationWorkload
{
    private static XdmValue _sink;
    internal static List<object> Run()
    {
        var vm = typeof(EvaluationContext).Assembly.GetType("Bosak.XPath.Runtime.Vm.VmEngine", true)!;
        var filter = vm.GetMethod("FilterNodesLazy", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<XdmValue, Func<IXdmNode, bool>, XdmValue>>();
        Func<IXdmNode, bool> accept = static _ => true;
        var values = new[] { XdmValue.FromNode(new XDocumentNode(new XElement("a"))), XdmValue.FromNode(new XDocumentNode(new XElement("b"))) };
        var input = XdmValue.FromSequence(XdmSequence.FromSource(new Source(values)));
        var marked = XdmValue.FromSequence(XdmSequence.FromSource(new MarkedSource(values)));
        var rows = new List<object> {
            Measure("create", () => filter(input, accept)),
            Measure("create-marked", () => filter(marked, accept)),
            Measure("once", () => XdmValue.FromInteger(Count(filter(input, accept)))) };
        var replay = filter(input, accept);
        rows.Add(Measure("replay", () => XdmValue.FromInteger(Count(replay))));
        var prepared = new XdmValue[110000];
        for (var i = 0; i < prepared.Length; i++) prepared[i] = filter(input, accept);
        int index = 0; object? cross = null; Exception? error = null;
        var thread = new Thread(() => {
            try { cross = Measure("cross-first", () => XdmValue.FromInteger(Count(prepared[index++]))); }
            catch (Exception e) { error = e; }
        });
        thread.Start();
        if (!thread.Join(30000)) throw new TimeoutException("Filter cross-thread measurement exceeded watchdog.");
        if (error is not null) throw new InvalidOperationException("Caught filter worker failure.", error);
        rows.Add(cross ?? throw new InvalidOperationException("Missing filter worker row."));
        return rows;
    }
    private static int Count(XdmValue value)
    {
        int count = 0;
        foreach (var _ in XdmSequence.FromSource(value.SequenceValue!)) count++;
        return count;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object Measure(string name, Func<XdmValue> call)
    {
        for (var i = 0; i < 10000; i++) _sink = call();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100000; i++) _sink = call();
        var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        return new { name, bytes, kind = _sink.Kind.ToString(), items = _sink.Kind == XdmValueKind.Integer ? (long?)_sink.IntegerValue : null };
    }
    private class Source : IXdmSequence
    {
        private readonly XdmValue[] _values;
        internal Source(XdmValue[] values) => _values = values;
        /// <inheritdoc/>
        public bool TryGetLength(out int length) { length = 0; return false; }
        /// <inheritdoc/>
        public IXdmSequenceEnumerator GetEnumerator() => new Enumerator(_values);
        private sealed class Enumerator : IXdmSequenceEnumerator
        {
            private readonly XdmValue[] _values; private int _index = -1;
            internal Enumerator(XdmValue[] values) => _values = values;
            /// <inheritdoc/>
            public XdmValue Current => _values[_index];
            /// <inheritdoc/>
            public bool MoveNext() => ++_index < _values.Length;
        }
    }
    private sealed class MarkedSource : Source, ISinglePassSequence
    {
        internal MarkedSource(XdmValue[] values) : base(values) { }
    }
}
