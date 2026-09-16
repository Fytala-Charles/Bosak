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
// ===========================================================================================================================================================
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
    private readonly StreamingNode _docNode;
    private readonly StreamingNode _rootNode;
    private readonly bool _rootEmpty;

    private PumpState _pumpState = PumpState.NotStarted;
    private int _nextRecordIndex;

    internal StreamingSource(XmlReader reader, StreamingLoadOptions options, bool ownsReader)
    {
        _reader = reader;
        _options = options;
        _ownsReader = ownsReader;

        // Read to the root element, capturing any DOCTYPE on the way. Comments and
        // processing instructions before the root element are not surfaced (documented
        // fidelity limitation of the streaming provider).
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
        // Register first so the shell sorts before every record in document order.
        XDocumentNode.RegisterTree(_shellDoc);

        _docNode = new StreamingNode(this, XDocumentNode.Wrap(_shellDoc), StreamingNodeRole.Document, recordIndex: -1);
        _rootNode = new StreamingNode(this, XDocumentNode.Wrap(_shellRoot), StreamingNodeRole.ShellRoot, recordIndex: -1);
    }

    internal StreamingNode DocumentNode => _docNode;

    internal StreamingNode RootNode => _rootNode;

    internal XElement ShellRoot => _shellRoot;

    internal bool RootEmpty => _rootEmpty;

    internal string BaseUri => _options.BaseUri ?? string.Empty;

    internal string DocumentUri => _options.DocumentUri ?? _options.BaseUri ?? string.Empty;

    internal bool HasDocumentType { get; }

    internal string DocumentTypeName { get; } = string.Empty;

    internal string PublicId { get; } = string.Empty;

    internal string SystemId { get; } = string.Empty;

    internal string InternalSubset { get; } = string.Empty;

    /// <summary>Wraps an engine-visible node of the streamed tree.</summary>
    /// <param name="inner">The shared <see cref="XDocumentNode"/> for the underlying object.</param>
    /// <param name="recordIndex">
    /// The arrival index of the top-level record the node belongs to (records and their
    /// descendants), or -1 for the shell document and root.
    /// </param>
    internal StreamingNode Wrap(XDocumentNode inner, int recordIndex)
        => new(this, inner, StreamingNodeRole.Record, recordIndex);

    /// <summary>
    /// Returns true when the pump is mid-flight owned by another enumerator, which means
    /// cross-record forward navigation cannot be served without corrupting the stream.
    /// </summary>
    internal bool IsPumpInFlight => _pumpState == PumpState.Pumping;

    /// <summary>Returns true when the stream has been read to the end of the root element.</summary>
    internal bool IsPumpDone => _pumpState == PumpState.Done;

    /// <summary>
    /// The number of top-level records yielded so far; equal to the total record count
    /// once <see cref="IsPumpDone"/> is true.
    /// </summary>
    internal int TotalRecords => _nextRecordIndex;

    /// <summary>
    /// The single-pass sequence of the root element's children (the streamed records),
    /// filtered by node kind. Can be enumerated at most once.
    /// </summary>
    internal XdmSequence ChildRecords(XdmNodeKind kind)
        => XdmSequence.FromSource(new StreamingSinglePassSequence(() => Pump(kind)));

    /// <summary>
    /// The single-pass sequence of the root element's descendants: every record
    /// followed by its own descendants, in document order.
    /// </summary>
    internal XdmSequence DescendantRecords(bool includeRoot)
        => XdmSequence.FromSource(new StreamingSinglePassSequence(() => PumpDescendants(includeRoot)));

    /// <summary>
    /// Consumes the remainder of the stream to compute the string value of the document
    /// or root node (the concatenation of all descendant text). This is a grounding
    /// operation: after it, the stream is exhausted.
    /// </summary>
    internal string ReadAllText()
    {
        var builder = new System.Text.StringBuilder();
        var enumerator = Pump(XdmNodeKind.All);
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
        var enumerator = Pump(XdmNodeKind.All);
        while (enumerator.MoveNext())
        {
            builder.Append(enumerator.Current.NodeValue!.ToXmlString());
        }
        return builder.ToString();
    }

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
                        _options.RecordPostProcessor?.Invoke(record);
                        XDocumentNode.RegisterTree(record);

                        var wrapper = Wrap(XDocumentNode.Wrap(record), _nextRecordIndex++);
                        if (MatchesKind(wrapper.NodeKind, kind))
                            yield return XdmValue.FromNode(wrapper);
                        break;
                    }

                case XmlNodeType.Text:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.Whitespace:
                    {
                        var wrapper = WrapNonElementRecord(new XText(_reader.Value));
                        if (MatchesKind(wrapper.NodeKind, kind))
                            yield return XdmValue.FromNode(wrapper);
                        break;
                    }

                case XmlNodeType.CDATA:
                    {
                        var wrapper = WrapNonElementRecord(new XCData(_reader.Value));
                        if (MatchesKind(wrapper.NodeKind, kind))
                            yield return XdmValue.FromNode(wrapper);
                        break;
                    }

                case XmlNodeType.Comment:
                    {
                        var wrapper = WrapNonElementRecord(new XComment(_reader.Value));
                        if (MatchesKind(wrapper.NodeKind, kind))
                            yield return XdmValue.FromNode(wrapper);
                        break;
                    }

                case XmlNodeType.ProcessingInstruction:
                    {
                        var wrapper = WrapNonElementRecord(new XProcessingInstruction(_reader.Name, _reader.Value));
                        if (MatchesKind(wrapper.NodeKind, kind))
                            yield return XdmValue.FromNode(wrapper);
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
