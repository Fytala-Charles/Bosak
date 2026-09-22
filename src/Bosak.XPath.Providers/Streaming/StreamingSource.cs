// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Owns the XmlReader, shell document, and single-pass record pump for streaming input
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 16-09-2026     | Phase B: IStreamingNode, per-record hook with drop support, Drain/CanDrain               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 17-09-2026     | Phase D4: opt-in record retention (tee/replay) for crawling streamable shapes          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 17-09-2026     | Annotate the shell document with DTD unparsed entities from the captured DOCTYPE;        |
//                      |                  |       |                | TryGetUnparsedEntity delegates record lookups to the shell (sf-unparsed-entity-01..08)   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 17-09-2026     | EnableReplay opts a not-yet-started stream into record retention so xsl:fork prongs can  |
//                      |                  |       |                | each replay the record stream (si-fork-808/816)                                          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 21-09-2026     | Wrapper cache (ConditionalWeakTable on the shared XDocumentNode) so navigation wraps     |
//                      |                  |       |                | per node, not per access; pre-root comments/PIs captured into the shell document and     |
//                      |                  |       |                | surfaced on the document axes                                                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 21-09-2026     | API freeze stage D: StreamCompleted is an event; EnableReplay -> TryEnableReplay         |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;

namespace Bosak.XPath.Providers.Streaming;

/// <summary>
/// Shared state behind a streaming document: the <see cref="XmlReader"/> being pulled,
/// the shell document (an empty root element carrying the root tag's name, attributes,
/// and namespace declarations), the pump that materializes top-level records one at a
/// time, and the stable wrappers for the document and root nodes.
/// </summary>
internal sealed class StreamingSource
{
    /// <summary>Marker annotation identifying the throwaway holder element of a
    /// non-element top-level record (text, comment, or processing instruction).</summary>
    internal sealed class TextRecordHolder;

    private enum PumpState
    {
        NotStarted,
        Pumping,
        Done,
    }

    private static readonly XName TextHolderName = XName.Get("streamed-text-holder", "urn:bosak:streaming");

    private readonly XmlReader _reader;
    private readonly bool _ownsReader;
    private readonly StreamingLoadOptions _options;
    private readonly XDocument _shellDoc;
    private readonly XElement _shellRoot;
    private readonly List<XObject> _preRootNodes = new();
    private readonly StreamingNode _docNode;
    private readonly StreamingNode _rootNode;
    private readonly bool _rootEmpty;

    // Wrapper cache keyed on the shared XDocumentNode (itself cached per XObject by
    // XDocumentNode.Wrap). Each underlying XObject belongs to exactly one record with a
    // fixed index, so the cached wrapper is always reached with the same recordIndex.
    // ConditionalWeakTable gives automatic eviction when a released record is collected,
    // preserving the bounded-memory contract for large streams.
    private readonly ConditionalWeakTable<XDocumentNode, StreamingNode> _wrapperCache = new();

    private PumpState _pumpState = PumpState.NotStarted;
    private int _nextRecordIndex;

    // Phase D4: opt-in record retention (tee/replay). When options.RetainRecords is set,                                                                     
    // every accepted record is appended to _retainedRecords as the single shared pump
    // enumerator produces it; consumers enumerate the list by index and whichever
    // enumerator reaches the frontier advances the pump. Evaluation is single-threaded,
    // so the shared pump is never inside MoveNext for two consumers at once.
    private List<XdmValue>? _retainedRecords;
    private IEnumerator<XdmValue>? _retainedPump;

    internal StreamingSource(XmlReader reader, StreamingLoadOptions options, bool ownsReader)
    {
        _reader = reader;
        _options = options;
        _ownsReader = ownsReader;
        RecordPostProcessor = options.RecordPostProcessor;
        _retainedRecords = options.RetainRecords ? new List<XdmValue>() : null;

        // Read to the root element, capturing any DOCTYPE on the way. Comments and
        // processing instructions before the root element are materialized eagerly (they
        // are typically tiny) and added to the shell document before the root, matching
        // the in-memory provider's fidelity.
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.DocumentType:
                    HasDocumentType = true;
                    DocumentTypeName = reader.Name ?? string.Empty;
                    PublicId = reader.GetAttribute("PUBLIC") ?? string.Empty;
                    SystemId = reader.GetAttribute("SYSTEM") ?? string.Empty;
                    InternalSubset = reader.Value ?? string.Empty;
                    break;
                case XmlNodeType.Comment:
                    _preRootNodes.Add(new XComment(reader.Value ?? string.Empty));
                    break;
                case XmlNodeType.ProcessingInstruction:
                    _preRootNodes.Add(new XProcessingInstruction(reader.Name ?? string.Empty, reader.Value ?? string.Empty));
                    break;
                case XmlNodeType.Element:
                    goto FoundRoot;
            }
        }
        throw new StreamingException("Streaming: the source contains no root element.");

FoundRoot:
        // Build the shell: an empty root element carrying the root tag's name,
        // attributes, and namespace declarations. Records are materialized as
        // detached trees and are never added to the shell, so order maps stay valid.
        _shellRoot = new XElement(XName.Get(reader.LocalName, reader.NamespaceURI));
        if (reader.HasAttributes)
        {
            while (reader.MoveToNextAttribute())
            {
                if (reader.Prefix == "xmlns")
                    _shellRoot.Add(new XAttribute(XNamespace.Xmlns + reader.LocalName, reader.Value ?? string.Empty));
                else if (reader.Name == "xmlns")
                    _shellRoot.Add(new XAttribute("xmlns", reader.Value ?? string.Empty));
                else
                    _shellRoot.Add(new XAttribute(XName.Get(reader.LocalName, reader.NamespaceURI), reader.Value ?? string.Empty));
            }
            reader.MoveToElement();
        }
        _rootEmpty = reader.IsEmptyElement;

        _shellDoc = new XDocument(_shellRoot);
        // Pre-root comments/PIs are document-level children arriving before the root.
        // Add them before registration so document-order ids place them before the root.
        if (_preRootNodes.Count > 0)
            _shellDoc.AddFirst(_preRootNodes);
        // Register first so the shell sorts before every record in document order.
        XDocumentNode.RegisterTree(_shellDoc);

        // Surface DTD unparsed entity declarations (fn:unparsed-entity-uri/-public-id)
        // exactly as the in-memory load path does.
        if (HasDocumentType)
        {
            Xml.Xml11Loader.AttachUnparsedEntitiesFromDoctype(
                _shellDoc, DocumentTypeName, PublicId, SystemId, InternalSubset,
                _options.BaseUri ?? string.Empty);
        }

        _docNode = new StreamingDocumentNode(this, XDocumentNode.Wrap(_shellDoc));
        _rootNode = new StreamingNode(this, XDocumentNode.Wrap(_shellRoot), StreamingNodeRole.ShellRoot, recordIndex: -1);
    }

    internal StreamingNode DocumentNode => _docNode;

    internal StreamingNode RootNode => _rootNode;

    internal XElement ShellRoot => _shellRoot;

    /// <summary>The shell document holding the (empty) root element and the pre-root
    /// comments/PIs. Used to recognize wrappers over shell-level nodes.</summary>
    internal XDocument ShellXDocument => _shellDoc;

    /// <summary>
    /// The comments and processing instructions that preceded the root element in the
    /// source, in document order. They are document-level children and sort before the
    /// shell root in document order.
    /// </summary>
    internal IReadOnlyList<XObject> PreRootShellNodes => _preRootNodes;

    internal bool RootEmpty => _rootEmpty;

    internal string BaseUri => _options.BaseUri ?? string.Empty;

    internal string DocumentUri => _options.DocumentUri ?? _options.BaseUri ?? string.Empty;

    internal bool HasDocumentType { get; }

    internal string DocumentTypeName { get; } = string.Empty;

    internal string PublicId { get; } = string.Empty;

    internal string SystemId { get; } = string.Empty;

    internal string InternalSubset { get; } = string.Empty;

    /// <summary>
    /// Looks up an unparsed entity declared by the document's DTD. The shell document
    /// carries the entity annotation; record nodes (detached trees) delegate here.
    /// </summary>
    internal bool TryGetUnparsedEntity(string name, out string? systemId, out string? publicId)
        => XDocumentNode.Wrap(_shellDoc).TryGetUnparsedEntity(name, out systemId, out publicId);

    /// <summary>Wraps an engine-visible node of the streamed tree.</summary>
    /// <param name="inner">The shared <see cref="XDocumentNode"/> for the underlying object.</param>
    /// <param name="recordIndex">
    /// The arrival index of the top-level record the node belongs to (records and their
    /// descendants), or -1 for the shell document, the shell root, and pre-root
    /// comments/PIs.
    /// </param>
    /// <remarks>
    /// The wrapper is cached per underlying object (see the class remarks): the same
    /// inner node is always requested with the same index, so the cached instance is
    /// valid. The cache is a <see cref="ConditionalWeakTable{TKey, TValue}"/> keyed on
    /// the inner node, so wrappers of released records are collected with their record.
    /// </remarks>
    internal StreamingNode Wrap(XDocumentNode inner, int recordIndex)
        => _wrapperCache.GetValue(inner, k => new StreamingNode(this, k, StreamingNodeRole.Record, recordIndex));

    /// <summary>
    /// Returns true when the pump is mid-flight owned by another enumerator, which means
    /// cross-record forward navigation cannot be served without corrupting the stream.
    /// </summary>
    internal bool IsPumpInFlight => _pumpState == PumpState.Pumping;

    /// <summary>Returns true when the stream has been read to the end of the root element.</summary>
    internal bool IsPumpDone => _pumpState == PumpState.Done;

    /// <summary>
    /// The per-record callback; seeded from <see cref="StreamingLoadOptions.RecordPostProcessor"/>
    /// and composable by the engine (whitespace stripping, accumulator evaluation).
    /// Returning false drops the record from the stream.
    /// </summary>
    internal Func<XObject, IXdmNode, bool>? RecordPostProcessor { get; set; }

    /// <summary>Invoked once when the pump reaches the end of the root element.</summary>
    internal event Action? StreamCompleted;

    /// <summary>True when the stream can be drained (not started, already done, or retained).</summary>
    internal bool CanDrain => _retainedRecords is not null || _pumpState != PumpState.Pumping;

    /// <summary>
    /// Consumes the remainder of the stream without exposing records: every record still
    /// flows through <see cref="RecordPostProcessor"/> and is released (or, with retention
    /// on, appended to the memo). Used by the engine for grounding reads such as
    /// document-level accumulator-after values.
    /// </summary>
    internal void Drain()
    {
        if (_pumpState == PumpState.Done)
            return;
        if (_retainedRecords is not null)
        {
            // Retained: any outstanding enumeration continues from the memo, so draining
            // to the end (appending all remaining records) cannot corrupt it.
            foreach (var _ in ReplayAll())
            {
            }
            return;
        }
        if (_pumpState == PumpState.Pumping)
        {
            throw new StreamingException(
                "Streaming: the stream cannot be drained while another enumeration is reading it. " +
                "Complete the current enumeration before performing the consuming read.");
        }

        var enumerator = Pump(XdmNodeKind.All);
        while (enumerator.MoveNext())
        {
            // Records are post-processed and discarded; the stream is grounding input here.
        }
    }

    /// <summary>
    /// Enables record retention so the record stream can be replayed (see
    /// <see cref="StreamingLoadOptions.RetainRecords"/>). Only possible while the pump
    /// has not started: records yielded before retention is enabled cannot be replayed.
    /// Idempotent — returns true when retention was already enabled.
    /// </summary>
    internal bool TryEnableReplay()
    {
        if (_retainedRecords is not null)
            return true;
        if (_pumpState != PumpState.NotStarted)
            return false;
        _retainedRecords = new List<XdmValue>();
        return true;
    }

    /// <summary>
    /// The number of top-level records yielded so far; equal to the total record count
    /// once <see cref="IsPumpDone"/> is true.
    /// </summary>
    internal int TotalRecords => _nextRecordIndex;

    /// <summary>
    /// The sequence of the root element's children (the streamed records), filtered by
    /// node kind. Without retention this is a single-pass sequence that can be enumerated
    /// at most once; with retention (see <see cref="StreamingLoadOptions.RetainRecords"/>)
    /// every enumeration replays the records memoized so far and continues at the pump
    /// frontier, so any number of consumers each see the full record stream.
    /// </summary>
    internal XdmSequence ChildRecords(XdmNodeKind kind)
    {
        if (_retainedRecords is not null)
        {
            return XdmSequence.FromSource(new EnumerableXdmSequence(ReplayChildren(kind)));
        }
        return XdmSequence.FromSource(new StreamingSinglePassSequence(() => Pump(kind)));
    }

    private IEnumerable<XdmValue> ReplayChildren(XdmNodeKind kind)
    {
        foreach (var item in ReplayAll())
        {
            if (MatchesKind(item.NodeValue!.NodeKind, kind))
                yield return item;
        }
    }

    /// <summary>
    /// The sequence of the root element's descendants: every record followed by its own
    /// descendants, in document order. Single-pass without retention; replayable with
    /// retention (see <see cref="ChildRecords"/>).
    /// </summary>
    internal XdmSequence DescendantRecords(bool includeRoot)
    {
        if (_retainedRecords is not null)
        {
            return XdmSequence.FromSource(new EnumerableXdmSequence(ReplayDescendants(includeRoot)));
        }
        return XdmSequence.FromSource(new StreamingSinglePassSequence(() => PumpDescendants(includeRoot)));
    }

    private IEnumerable<XdmValue> ReplayDescendants(bool includeRoot)
    {
        if (includeRoot)
            yield return XdmValue.FromNode(_rootNode);

        foreach (var item in ReplayAll())
        {
            yield return item;
            var record = (StreamingNode)item.NodeValue!;
            foreach (var descendant in record.InnerAxis(XdmAxis.Descendant))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// The shared record source for retained streaming: replays records from the memo by
    /// index; when a consumer reaches the frontier it advances the single shared pump,
    /// which appends each accepted record to the memo before yielding it. Consumers that
    /// are behind read only memo entries, which is the tee.
    /// </summary>
    private IEnumerable<XdmValue> ReplayAll()
    {
        var records = _retainedRecords!;
        var pump = RetainedPump();
        int index = 0;
        while (true)
        {
            while (index < records.Count)
            {
                yield return records[index];
                index++;
            }
            if (_pumpState == PumpState.Done)
                yield break;
            if (!pump.MoveNext())
                yield break;
            // The pump appended at least one record before yielding; re-check the memo.
        }
    }

    /// <summary>Returns the single shared memoizing pump enumerator, creating it on first use.</summary>
    private IEnumerator<XdmValue> RetainedPump()
    {
        if (_retainedPump is not null)
            return _retainedPump;
        if (_pumpState != PumpState.NotStarted)
        {
            throw new StreamingException(
                "Streaming: the streamed record stream was consumed before retention could be established.");
        }
        _retainedPump = Pump(XdmNodeKind.All);
        return _retainedPump;
    }

    /// <summary>
    /// Consumes the remainder of the stream to compute the string value of the document
    /// or root node (the concatenation of all descendant text). This is a grounding
    /// operation: after it, the stream is exhausted.
    /// </summary>
    internal string ReadAllText()
    {
        var builder = new System.Text.StringBuilder();
        var enumerator = AllRecords();
        while (enumerator.MoveNext())
        {
            builder.Append(enumerator.Current.NodeValue!.StringValue);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Consumes the remainder of the stream to serialize the streamed content. Each
    /// record is serialized individually and concatenated.
    /// </summary>
    internal string SerializeContent(System.Text.StringBuilder builder)
    {
        var enumerator = AllRecords();
        while (enumerator.MoveNext())
        {
            builder.Append(enumerator.Current.NodeValue!.ToXmlString());
        }
        return builder.ToString();
    }

    /// <summary>
    /// Enumerator over every record from the start of the stream: a replay over the
    /// memoized records with retention on, or a fresh single-pass pump otherwise.
    /// </summary>
    private IEnumerator<XdmValue> AllRecords()
        => _retainedRecords is not null ? ReplayAll().GetEnumerator() : Pump(XdmNodeKind.All);

    private IEnumerator<XdmValue> PumpDescendants(bool includeRoot)
    {
        if (includeRoot)
            yield return XdmValue.FromNode(_rootNode);

        var enumerator = Pump(XdmNodeKind.All);
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
            var record = (StreamingNode)enumerator.Current.NodeValue!;
            foreach (var descendant in record.InnerAxis(XdmAxis.Descendant))
            {
                yield return descendant;
            }
        }
    }

    private IEnumerator<XdmValue> Pump(XdmNodeKind kind)
    {
        if (_pumpState != PumpState.NotStarted)
        {
            throw new StreamingException(
                "Streaming: the children of the streamed document root are forward-only and have already been consumed. " +
                "Restructure the transform so the streamed input is read in a single pass.");
        }
        _pumpState = PumpState.Pumping;

        if (_rootEmpty)
        {
            FinishPump();
            yield break;
        }

        bool skipRead = false;
        while (true)
        {
            if (!skipRead && !_reader.Read())
            {
                FinishPump();
                yield break;
            }
            skipRead = false;

            switch (_reader.NodeType)
            {
                case XmlNodeType.Element:
                    {
                        var record = (XElement)XNode.ReadFrom(_reader);
                        // XNode.ReadFrom may leave the reader on the record's end tag or
                        // already on the following node; only the former needs another Read().
                        if (_reader.NodeType != XmlNodeType.EndElement)
                            skipRead = true;

                        InheritRootNamespaces(record);
                        XDocumentNode.RegisterTree(record);

                        var wrapper = Wrap(XDocumentNode.Wrap(record), _nextRecordIndex++);
                        if (RecordPostProcessor?.Invoke(record, wrapper) == false)
                            break; // dropped by the post-processor (e.g. xsl:strip-space)
                        var value = XdmValue.FromNode(wrapper);
                        if (_retainedRecords is not null)
                            _retainedRecords.Add(value); // memoize before yielding so replay sees the full stream
                        if (MatchesKind(wrapper.NodeKind, kind))
                            yield return value;
                        break;
                    }

                case XmlNodeType.Text:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.Whitespace:
                    {
                        var record = new XText(_reader.Value);
                        var wrapper = WrapNonElementRecord(record);
                        if (RecordPostProcessor?.Invoke(record, wrapper) == false)
                            break;
                        var value = XdmValue.FromNode(wrapper);
                        if (_retainedRecords is not null)
                            _retainedRecords.Add(value); // memoize before yielding so replay sees the full stream
                        if (MatchesKind(wrapper.NodeKind, kind))
                            yield return value;
                        break;
                    }

                case XmlNodeType.CDATA:
                    {
                        var record = new XCData(_reader.Value);
                        var wrapper = WrapNonElementRecord(record);
                        if (RecordPostProcessor?.Invoke(record, wrapper) == false)
                            break;
                        var value = XdmValue.FromNode(wrapper);
                        if (_retainedRecords is not null)
                            _retainedRecords.Add(value); // memoize before yielding so replay sees the full stream
                        if (MatchesKind(wrapper.NodeKind, kind))
                            yield return value;
                        break;
                    }

                case XmlNodeType.Comment:
                    {
                        var record = new XComment(_reader.Value);
                        var wrapper = WrapNonElementRecord(record);
                        if (RecordPostProcessor?.Invoke(record, wrapper) == false)
                            break;
                        var value = XdmValue.FromNode(wrapper);
                        if (_retainedRecords is not null)
                            _retainedRecords.Add(value); // memoize before yielding so replay sees the full stream
                        if (MatchesKind(wrapper.NodeKind, kind))
                            yield return value;
                        break;
                    }

                case XmlNodeType.ProcessingInstruction:
                    {
                        var record = new XProcessingInstruction(_reader.Name, _reader.Value);
                        var wrapper = WrapNonElementRecord(record);
                        if (RecordPostProcessor?.Invoke(record, wrapper) == false)
                            break;
                        var value = XdmValue.FromNode(wrapper);
                        if (_retainedRecords is not null)
                            _retainedRecords.Add(value); // memoize before yielding so replay sees the full stream
                        if (MatchesKind(wrapper.NodeKind, kind))
                            yield return value;
                        break;
                    }

                case XmlNodeType.EndElement:
                    FinishPump();
                    yield break;
            }
        }
    }

    private StreamingNode WrapNonElementRecord(XNode node)
    {
        // Non-element records need a tree root for document-order computation, so the
        // node is placed in a throwaway holder element. The holder is marked so the
        // wrapper can hide it from the parent/ancestor axes.
        var holder = new XElement(TextHolderName, node);
        holder.AddAnnotation(new TextRecordHolder());
        XDocumentNode.RegisterTree(holder);
        return Wrap(XDocumentNode.Wrap(node), _nextRecordIndex++);
    }

    private void FinishPump()
    {
        _pumpState = PumpState.Done;
        StreamCompleted?.Invoke();
        if (_ownsReader)
            _reader.Dispose();
    }

    /// <summary>
    /// Copies namespace declarations from the shell root to a materialized record when
    /// the reader did not repeat them, so prefixes bound on ancestors remain bound and
    /// the namespace axis inside the record reflects the source document.
    /// </summary>
    private void InheritRootNamespaces(XElement record)
    {
        foreach (var attr in _shellRoot.Attributes())
        {
            if (!attr.IsNamespaceDeclaration)
                continue;
            if (record.Attribute(attr.Name) is null)
                record.Add(new XAttribute(attr.Name, attr.Value));
        }
    }

    private static bool MatchesKind(XdmNodeKind actual, XdmNodeKind requested)
        => requested == XdmNodeKind.All || (requested & actual) == actual;
}
