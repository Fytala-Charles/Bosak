// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 November 2026
// PURPOSE              : Measures isolated provider wrapper allocation regression workloads
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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;

namespace Bosak.XPath.AllocationChecks;

/// <summary>Measures warmed in-record wrapper paths through the existing provider.</summary>
internal static class ProviderWrapperAllocationWorkload
{
    private static XdmValue _sink;
    internal static List<object> Run()
    {
        using var reader = XmlReader.Create(new StringReader("<root><record><a/><b/></record></root>"));
        var document = XmlStreamingProvider.Load(reader);
        var roots = document.Children(XdmNodeKind.Element).GetEnumerator(); roots.MoveNext();
        var records = roots.Current.NodeValue.Children(XdmNodeKind.Element).GetEnumerator(); records.MoveNext();
        var record = records.Current.NodeValue; var type = record.GetType();
        var inner = (IXdmNode)type.GetField("_inner", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(record)!;
        var direct = type.GetMethod("WrapInRecord", BindingFlags.NonPublic | BindingFlags.Instance);
        Func<XdmSequence, XdmSequence> make;
        if (direct is not null) make = direct.CreateDelegate<Func<XdmSequence, XdmSequence>>(record);
        else
        {
            // Compatibility control: this same binary also executes on qualified 1.81.14.
            var wrap = type.GetMethod("WrapEach", BindingFlags.NonPublic | BindingFlags.Instance)!
                .CreateDelegate<Func<XdmSequence, IEnumerable<XdmValue>>>(record);
            var adapter = typeof(XdmSequence).Assembly.GetType("Bosak.XPath.Core.Xdm.EnumerableXdmSequence", true)!;
            var parameter = Expression.Parameter(typeof(IEnumerable<XdmValue>));
            var adapt = Expression.Lambda<Func<IEnumerable<XdmValue>, IXdmSequence>>(
                Expression.New(adapter.GetConstructor(new[] { typeof(IEnumerable<XdmValue>) })!, parameter), parameter).Compile();
            make = value => XdmSequence.FromSource(adapt(wrap(value)));
        }
        var values = new List<XdmValue>(); foreach (var item in inner.Children(XdmNodeKind.Element)) values.Add(item);
        var input = XdmSequence.FromSource(new Source(values.ToArray()));
        if (Count(make(input)) != 2) throw new InvalidOperationException("Wrapper fixture lost nodes.");
        var rows = new List<object> {
            Measure("create", () => XdmValue.FromSequence(make(input))),
            Measure("once", () => XdmValue.FromInteger(Count(make(input)))) };
        var replay = make(input); rows.Add(Measure("replay", () => XdmValue.FromInteger(Count(replay))));
        var prepared = new XdmSequence[110000]; for (int i = 0; i < prepared.Length; i++) prepared[i] = make(input);
        int index = 0; object? cross = null; Exception? error = null;
        var thread = new Thread(() => {
            try { cross = Measure("cross-first", () => XdmValue.FromInteger(Count(prepared[index++]))); }
            catch (Exception e) { error = e; }
        });
        thread.Start(); if (!thread.Join(30000)) throw new TimeoutException("Wrapper cross-thread watchdog.");
        if (error is not null) throw new InvalidOperationException("Caught wrapper worker failure.", error);
        rows.Add(cross ?? throw new InvalidOperationException("Missing wrapper allocation row."));
        return rows;
    }
    private static int Count(XdmSequence sequence) { int count = 0; foreach (var _ in sequence) count++; return count; }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static object Measure(string name, Func<XdmValue> call)
    {
        for (int i = 0; i < 10000; i++) _sink = call();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100000; i++) _sink = call();
        var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        return new { name, bytes, kind = _sink.Kind.ToString(), items = _sink.Kind == XdmValueKind.Integer ? (long?)_sink.IntegerValue : null };
    }
    private sealed class Source : IXdmSequence
    {
        private readonly XdmValue[] _items;
        internal Source(XdmValue[] items) => _items = items;
        /// <inheritdoc/>
        public bool TryGetLength(out int length) { length = 0; return false; }
        /// <inheritdoc/>
        public IXdmSequenceEnumerator GetEnumerator() => new Enumerator(_items);
        private sealed class Enumerator : IXdmSequenceEnumerator
        {
            private readonly XdmValue[] _items; private int _index = -1;
            internal Enumerator(XdmValue[] items) => _items = items;
            /// <inheritdoc/>
            public XdmValue Current => _items[_index];
            /// <inheritdoc/>
            public bool MoveNext() => ++_index < _items.Length;
        }
    }
}
