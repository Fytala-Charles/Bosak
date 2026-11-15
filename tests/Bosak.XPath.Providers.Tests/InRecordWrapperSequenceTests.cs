// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 November 2026
// PURPOSE              : Verifies in-record wrapper identity, mutation, typed values and lifetime
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
//                      | Charles Korthout | 0.2   | 15-11-2026     | Preserve method snapshot versus lazy axis mutation semantics.                           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.XPath.Providers.Tests.Streaming;

/// <summary>Verifies direct in-record wrapping preserves the existing provider behavior.</summary>
public sealed class InRecordWrapperSequenceTests
{
    private static List<XdmValue> Pull(XdmSequence seq) { var values = new List<XdmValue>(); foreach (var v in seq) values.Add(v); return values; }
    private static Func<XdmSequence, XdmSequence> Direct(IXdmNode owner) => owner.GetType().GetMethod("WrapInRecord", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<Func<XdmSequence, XdmSequence>>(owner);

    /// <summary>Checks deferred child lookup, mutation visibility, identity and ordinary markers.</summary>
    [Fact]
    public void Children_ObserveMutationAndReplayIdentity()
    {
        using var f = new Fixture(); var sequence = f.Record.Children(XdmNodeKind.Element); var axis = f.Record.Axis(XdmAxis.Child);
        Assert.False(sequence.IsSinglePass); Assert.False(sequence.TryGetLength(out _));
        f.Element.Add(new XElement("late")); var first = Pull(sequence); Assert.Equal(new[] { "a", "b" }, first.Select(v => v.NodeValue.LocalName));
        var second = Pull(sequence); for (int i = 0; i < first.Count; i++) { Assert.Same(first[i].NodeValue, second[i].NodeValue); Assert.True(first[i].NodeValue.Parent!.IsSameNode(f.Record)); Assert.Same(f.Document, first[i].NodeValue.Document); }
        Assert.Equal(new[] { "a", "b", "late" }, Pull(axis).Where(v => v.NodeValue.NodeKind == XdmNodeKind.Element).Select(v => v.NodeValue.LocalName));
        Assert.Equal(3, Pull(f.Record.Children(XdmNodeKind.Element)).Count);
        Assert.Contains(Pull(f.Record.Children()), v => v.NodeValue.NodeKind == XdmNodeKind.Comment);
    }

    /// <summary>Checks attribute filtering, late mutation and namespace XDM identity.</summary>
    [Fact]
    public void AttributesAndNamespaces_PreserveIdentityAndFiltering()
    {
        using var f = new Fixture(); var attributes = f.Record.Attributes(); var axis = f.Record.Axis(XdmAxis.Attribute); f.Element.SetAttributeValue("late", "new");
        var first = Pull(attributes); Assert.DoesNotContain(first, v => v.NodeValue.LocalName == "late");
        Assert.Contains(Pull(axis), v => v.NodeValue.LocalName == "late");
        Assert.Contains(Pull(f.Record.Attributes()), v => v.NodeValue.LocalName == "late"); Assert.DoesNotContain(first, v => v.NodeValue.NodeKind == XdmNodeKind.Namespace);
        var id = Assert.Single(Pull(f.Record.Attributes("id", ""))).NodeValue; Assert.Equal("one", id.StringValue);
        Assert.Same(id, Assert.Single(Pull(f.Record.Axis(XdmAxis.Attribute)).Where(v => v.NodeValue.LocalName == "id")).NodeValue);
        var ns = Pull(f.Record.Axis(XdmAxis.Namespace)); var replay = Pull(f.Record.Axis(XdmAxis.Namespace));
        Assert.Contains(ns, v => v.NodeValue.LocalName == "q" && v.NodeValue.StringValue == "urn:q");
        foreach (var v in ns) Assert.True(v.NodeValue.IsSameNode(Assert.Single(replay.Where(n => n.NodeValue.LocalName == v.NodeValue.LocalName)).NodeValue));
    }

    /// <summary>Checks held record reads, captured indices and cross-record guards after advancement.</summary>
    [Fact]
    public void HeldSequences_PreserveRecordIndexAndNavigationGuards()
    {
        using var f = new Fixture(); var old = f.Record.Children(XdmNodeKind.Element); Assert.True(f.Advance());
        var values = Pull(old); Assert.Equal(new[] { "a", "b" }, values.Select(v => v.NodeValue.LocalName));
        Assert.All(values, v => Assert.Equal(0, (int)v.NodeValue.GetType().GetField("_recordIndex", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(v.NodeValue)!));
        var second = Assert.Single(Pull(f.Current.Children(XdmNodeKind.Element))).NodeValue;
        Assert.Equal(1, (int)second.GetType().GetField("_recordIndex", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(second)!);
        Assert.Throws<StreamingException>(() => second.Axis(XdmAxis.Preceding));
        Assert.False(f.Advance()); Assert.Equal(2, Pull(old).Count);
        Assert.Throws<StreamingException>(() => Pull(values[0].NodeValue.Axis(XdmAxis.Following)));
        Assert.Same(f.Document, values[0].NodeValue.Document);
    }

    /// <summary>Checks deferred source failures, casts, retained Current and terminal behavior.</summary>
    /// <param name="fault">The injected failure point.</param>
    [Theory]
    [InlineData("acquire")]
    [InlineData("move")]
    [InlineData("current")]
    [InlineData("cast")]
    public void SourceFailures_AreDeferredAndTerminal(string fault)
    {
        using var f = new Fixture();
        // Supply actual in-record XDocumentNode values to the wrapper, not streaming adapters.
        var raw = new[] { XdmValue.FromNode(XDocumentNode.Wrap(new XElement("a"))), fault == "cast" ? XdmValue.FromNode(f.Record) : XdmValue.FromNode(XDocumentNode.Wrap(new XElement("b"))) };
        var source = new FaultSource(raw, fault); var value = Direct(f.Record)(XdmSequence.FromSource(source));
        var e = value.GetEnumerator(); Assert.True(e.Current.IsUndefined); Assert.Equal(0, source.Gets);
        if (fault != "acquire") Assert.True(e.MoveNext());
        if (fault == "cast") Assert.Throws<InvalidCastException>(() => e.MoveNext());
        else Assert.Equal(fault + "-marker", Assert.Throws<InvalidOperationException>(() => e.MoveNext()).Message);
        if (fault == "acquire") Assert.True(e.Current.IsUndefined); else Assert.Equal("a", e.Current.NodeValue.LocalName);
        int moves = source.Moves; Assert.False(e.MoveNext()); Assert.Equal(moves, source.Moves); Assert.Equal(0, source.Disposals);
    }

    /// <summary>Checks empty/null sources and no eager acquisition or marker propagation.</summary>
    [Fact]
    public void DirectProtocol_PreservesUnknownLengthAndNullEnumeratorBehavior()
    {
        using var f = new Fixture(); var make = Direct(f.Record);
        foreach (var empty in new[] { default(XdmSequence), XdmSequence.Empty }) { var seq = make(empty); Assert.False(seq.TryGetLength(out _)); Assert.False(seq.IsSinglePass); Assert.Empty(Pull(seq)); }
        var source = new FaultSource(Array.Empty<XdmValue>(), "null"); var sequence = make(XdmSequence.FromSource(source));
        var e = sequence.GetEnumerator(); Assert.Equal(0, source.Gets); Assert.False(e.MoveNext()); Assert.False(e.MoveNext()); Assert.Equal(1, source.Gets);
    }

    /// <summary>Checks negative-index document/shell paths retain their existing contracts.</summary>
    [Fact]
    public void DocumentAndShellRoutes_RetainMarkersAndParentChains()
    {
        using var f = new Fixture(); Assert.True(f.Root.Children().IsSinglePass); Assert.False(f.Document.Children().IsSinglePass);
        var children = Pull(f.Document.Children()); Assert.Equal(new[] { XdmNodeKind.Comment, XdmNodeKind.ProcessingInstruction, XdmNodeKind.Element }, children.Select(v => v.NodeValue.NodeKind));
        Assert.All(children, v => Assert.True(v.NodeValue.Parent!.IsSameNode(f.Document)));
        Assert.Empty(Pull(children[0].NodeValue.Children())); Assert.Equal("root", f.Root.LocalName);
        Assert.Equal("root-value", Assert.Single(Pull(f.Root.Attributes("root-attr", ""))).NodeValue.StringValue);
    }

    /// <summary>Checks typed child/attribute values and annotations survive streaming wrapping.</summary>
    [Fact]
    public void TypedRecords_PreservePsviThroughAllDirectPaths()
    {
        var schemas = new XmlSchemaSet(); using (var text = XmlReader.Create(new StringReader("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element name='record'><xs:complexType><xs:sequence><xs:element name='n' type='xs:integer'/></xs:sequence><xs:attribute name='id' type='xs:integer' use='required'/></xs:complexType></xs:element></xs:schema>"))) schemas.Add(null, text); schemas.Compile();
        using var reader = XmlReader.Create(new StringReader("<root><record id='007'><n>009</n></record></root>"));
        var document = XmlStreamingProvider.Load(reader, new StreamingLoadOptions { RecordPostProcessor = (obj, _) => {
            if (obj is XElement el) Assert.True(XdmSchemaAnnotator.ValidateSubtree(el, schemas).IsValid); return true; } });
        var root = Assert.Single(Pull(document.Children(XdmNodeKind.Element))).NodeValue;
        var record = Assert.Single(Pull(root.Children(XdmNodeKind.Element))).NodeValue;
        var child = Assert.Single(Pull(record.Children(XdmNodeKind.Element))).NodeValue;
        var attr = Assert.Single(Pull(record.Attributes())).NodeValue;
        Assert.Equal(XdmValueKind.Integer, child.TypedValue.Kind); Assert.Equal(9L, child.TypedValue.IntegerValue);
        Assert.Equal(XdmValueKind.Integer, attr.TypedValue.Kind); Assert.Equal(7L, attr.TypedValue.IntegerValue);
        Assert.Equal("integer", child.SchemaTypeAnnotation!.Value.LocalName); Assert.Equal("integer", attr.SchemaTypeAnnotation!.Value.LocalName);
        Assert.Same(child, Assert.Single(Pull(record.Axis(XdmAxis.Descendant)).Where(v => v.NodeValue.NodeKind == XdmNodeKind.Element)).NodeValue);
        Assert.Same(attr, Assert.Single(Pull(record.Axis(XdmAxis.Attribute))).NodeValue);
    }

    /// <summary>Checks abandoned in-record enumerators do not close the caller's reader or stream.</summary>
    [Fact]
    public void ReaderOwnership_RemainsCallerControlled()
    {
        using var f = new Fixture(); var partial = f.Record.Children(XdmNodeKind.Element).GetEnumerator(); Assert.True(partial.MoveNext());
        Assert.True(f.Advance()); f.DisposeReader(); Assert.True(f.StreamOpen);
    }

    /// <summary>Checks discarded sequences do not introduce strong cache roots for synthetic records.</summary>
    [Fact]
    public void DiscardedSequences_DoNotAddStrongCacheRoots()
    {
        using var reader = XmlReader.Create(new StringReader("<root/>")); var source = new StreamingSource(reader, new StreamingLoadOptions(), ownsReader: false);
        var weak = MakeDiscarded(source);
        for (int i = 0; i < 3; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
        Assert.False(weak.IsAlive); GC.KeepAlive(source);
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference MakeDiscarded(StreamingSource source)
    {
        var element = new XElement("record", new XElement("child")); var node = source.Wrap(XDocumentNode.Wrap(element), 5);
        var sequence = node.Children(XdmNodeKind.Element); Assert.Single(Pull(sequence)); return new WeakReference(element);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly MemoryStream _stream; private readonly XmlReader _reader; private XdmSequence.Enumerator _records;
        internal readonly IXdmNode Document, Root, Record; internal readonly XElement Element;
        internal Fixture()
        {
            _stream = new MemoryStream(Encoding.UTF8.GetBytes("<!--pre--><?pi x?><root xmlns:p='urn:p' root-attr='root-value'><record id='one' xmlns:q='urn:q'><a/><!--inside--><b/></record><record id='two'><c/></record></root>"));
            _reader = XmlReader.Create(_stream, new XmlReaderSettings { CloseInput = false }); Document = XmlStreamingProvider.Load(_reader);
            Root = Assert.Single(Pull(Document.Children(XdmNodeKind.Element))).NodeValue;
            _records = Root.Children(XdmNodeKind.Element).GetEnumerator(); Assert.True(_records.MoveNext()); Record = _records.Current.NodeValue;
            Element = (XElement)((IStreamingNode)Record).UnderlyingXObject;
        }
        internal IXdmNode Current => _records.Current.NodeValue;
        internal bool Advance() => _records.MoveNext();
        internal bool StreamOpen => _stream.CanRead;
        internal void DisposeReader() => _reader.Dispose();
        /// <inheritdoc/>
        public void Dispose() { _reader.Dispose(); _stream.Dispose(); }
    }
    private sealed class FaultSource : IXdmSequence
    {
        private readonly XdmValue[] _items; private readonly string _fault; internal int Gets, Moves, Disposals;
        internal FaultSource(XdmValue[] items, string fault) { _items = items; _fault = fault; }
        /// <inheritdoc/>
        public bool TryGetLength(out int length) { length = 0; return false; }
        /// <inheritdoc/>
        public IXdmSequenceEnumerator GetEnumerator() { Gets++; if (_fault == "acquire") throw new InvalidOperationException("acquire-marker"); return _fault == "null" ? null! : new Enumerator(this); }
        private sealed class Enumerator : IXdmSequenceEnumerator, IDisposable
        {
            private readonly FaultSource _s; private int _i = -1; internal Enumerator(FaultSource source) => _s = source;
            /// <inheritdoc/>
            public XdmValue Current => _s._fault == "current" && _i == 1 ? throw new InvalidOperationException("current-marker") : _s._items[_i];
            /// <inheritdoc/>
            public bool MoveNext() { _s.Moves++; _i++; if (_s._fault == "move" && _i == 1) throw new InvalidOperationException("move-marker"); return _i < _s._items.Length; }
            /// <inheritdoc/>
            public void Dispose() => _s.Disposals++;
        }
    }
}
