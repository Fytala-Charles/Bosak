// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : IXdmNode wrapper presenting a streamed record tree with parent/document chains intact
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
//                      | Charles Korthout | 0.2   | 16-09-2026     | Phase B: implements IStreamingNode (UnderlyingXObject); unsealed for the document role   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 17-09-2026     | TryGetUnparsedEntity falls back to the streaming source's shell-document DTD entities    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 21-09-2026     | Document role surfaces pre-root comments/PIs (children, child/descendant axes,           |
//                      |                  |       |                | serialization); shell-level wrappers route parent/ancestor/following-sibling to the      |
//                      |                  |       |                | document/root correctly                                                                  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;

namespace Bosak.XPath.Providers.Streaming;

internal enum StreamingNodeRole
{
    /// <summary>The synthetic document node at the top of the streamed tree.</summary>
    Document,

    /// <summary>The shell root element; its children are pulled from the stream.</summary>
    ShellRoot,

    /// <summary>A materialized record node or a node inside a record.</summary>
    Record,
}

/// <summary>
/// An <see cref="IXdmNode"/> over the streamed source. Delegates every member to an inner
/// <see cref="XDocumentNode"/> except parent/document navigation (which is re-rooted at
/// the streaming root and document nodes) and the forward axes of the shell root (which
/// pull the stream). Identity and document order delegate to the underlying
/// <see cref="XObject"/>, so wrappers created from below (via <see cref="Parent"/>) are
/// value-equal to wrappers handed out from above.
/// </summary>
internal class StreamingNode : IXdmNode, IStreamingNode
{
    private readonly StreamingSource _source;
    private readonly XDocumentNode _inner;
    private readonly StreamingNodeRole _role;
    private readonly int _recordIndex;

    internal StreamingNode(StreamingSource source, XDocumentNode inner, StreamingNodeRole role, int recordIndex)
    {
        _source = source;
        _inner = inner;
        _role = role;
        _recordIndex = recordIndex;
    }

    internal StreamingNodeRole Role => _role;

    internal StreamingSource Source => _source;

    /// <inheritdoc/>
    public XObject UnderlyingXObject => _inner.UnderlyingObject;

    /// <summary>
    /// True when this wrapper represents a pre-root comment or processing instruction
    /// of the streamed document: a node whose containing document is the shell document.
    /// Its parent chain is [document] — never the shell root — and it is not a record,
    /// so record-local navigation must not apply to it.
    /// </summary>
    private bool IsShellChild
        => _role == StreamingNodeRole.Record
           && _inner.UnderlyingObject is XNode node
           && ReferenceEquals(node.Document, _source.ShellXDocument);

    /// <summary>
    /// True when this wrapper represents a top-level record directly below the streamed
    /// root: a detached record root element, or a non-element record hidden in its
    /// throwaway holder element.
    /// </summary>
    private bool IsTopLevelRecord
    {
        get
        {
            if (_role != StreamingNodeRole.Record)
                return false;
            var parent = _inner.Parent;
            if (parent is null)
                return true;
            return parent is XDocumentNode xn
                   && xn.UnderlyingObject is XElement holder
                   && holder.Annotation<StreamingSource.TextRecordHolder>() is not null;
        }
    }

    // ------------------------------------------------------------------
    // Metadata — straight delegation
    // ------------------------------------------------------------------

    public XdmNodeKind NodeKind => _inner.NodeKind;

    public string LocalName => _inner.LocalName;

    public string EncodedLocalName => _inner.EncodedLocalName;

    public string NamespaceUri => _inner.NamespaceUri;

    public string Prefix => _inner.Prefix;

    public string EncodedPrefix => _inner.EncodedPrefix;

    public bool HasNoTypedValue => _inner.HasNoTypedValue;

    public bool IsComplexType => _inner.IsComplexType;

    public bool IsId => _inner.IsId;

    public bool IsIdref => _inner.IsIdref;

    public (string NamespaceUri, string LocalName)? SchemaTypeAnnotation => _inner.SchemaTypeAnnotation;

    public (string NamespaceUri, string LocalName)? SchemaElementDeclaration => _inner.SchemaElementDeclaration;

    public (string NamespaceUri, string LocalName)? SchemaAttributeDeclaration => _inner.SchemaAttributeDeclaration;

    public bool IsNilled => _inner.IsNilled;

    public bool IsConstructedElement => _inner.IsConstructedElement;

    public long DocumentOrder => _inner.DocumentOrder;

    // ------------------------------------------------------------------
    // Values — document/root string and typed values consume the stream
    // ------------------------------------------------------------------

    public string StringValue => _role == StreamingNodeRole.Record
        ? _inner.StringValue
        : _source.ReadAllText();

    public XdmValue TypedValue => _role == StreamingNodeRole.Record
        ? _inner.TypedValue
        : XdmValue.FromString(StringValue, "untypedAtomic");

    // ------------------------------------------------------------------
    // URIs and DTD properties
    // ------------------------------------------------------------------

    public string BaseUri => _role == StreamingNodeRole.Record && !string.IsNullOrEmpty(_inner.BaseUri)
        ? _inner.BaseUri
        : _source.BaseUri;

    public string DocumentUri => _source.DocumentUri;

    public bool HasDocumentType => _source.HasDocumentType;

    public string DocumentTypeName => _source.DocumentTypeName;

    public string PublicId => _source.PublicId;

    public string SystemId => _source.SystemId;

    public string InternalSubset => _source.InternalSubset;

    public bool TryGetUnparsedEntity(string name, out string? systemId, out string? publicId)
    {
        // Record nodes live in detached trees without the DTD annotation; entity
        // declarations come from the streamed document's DOCTYPE either way.
        if (_inner.TryGetUnparsedEntity(name, out systemId, out publicId))
            return true;
        return _source.TryGetUnparsedEntity(name, out systemId, out publicId);
    }

    // ------------------------------------------------------------------
    // Navigation — re-rooted at the streaming document
    // ------------------------------------------------------------------

    public IXdmNode? Parent => _role switch
    {
        StreamingNodeRole.Document => null,
        StreamingNodeRole.ShellRoot => _source.DocumentNode,
        _ => IsShellChild
            ? _source.DocumentNode
            : IsTopLevelRecord
                ? _source.RootNode
                : _source.Wrap((XDocumentNode)_inner.Parent!, _recordIndex),
    };

    public IXdmNode? Document => _source.DocumentNode;

    public XdmSequence Children(XdmNodeKind kind = XdmNodeKind.All) => _role switch
    {
        StreamingNodeRole.Document => DocumentChildren(kind),
        StreamingNodeRole.ShellRoot => _source.ChildRecords(kind),
        _ => XdmSequence.FromSource(new EnumerableXdmSequence(WrapEach(_inner.Children(kind)))),
    };

    /// <summary>
    /// The children of the streamed document: the pre-root comments/PIs (if any),
    /// followed by the root element. Records are children of the root, not the document.
    /// </summary>
    private XdmSequence DocumentChildren(XdmNodeKind kind)
    {
        var preRoot = _source.PreRootShellNodes;
        bool includeRoot = MatchesKind(XdmNodeKind.Element, kind);
        if (preRoot.Count == 0)
        {
            return includeRoot
                ? XdmSequence.Singleton(XdmValue.FromNode(_source.RootNode))
                : XdmSequence.Empty;
        }
        return XdmSequence.FromSource(new EnumerableXdmSequence(EnumerateDocumentChildren(kind)));
    }

    private IEnumerable<XdmValue> EnumerateDocumentChildren(XdmNodeKind kind)
    {
        foreach (var item in EnumeratePreRootNodes(kind))
            yield return item;
        if (MatchesKind(XdmNodeKind.Element, kind))
            yield return XdmValue.FromNode(_source.RootNode);
    }

    /// <summary>The pre-root comments/PIs whose kind matches <paramref name="kind"/>.</summary>
    private IEnumerable<XdmValue> EnumeratePreRootNodes(XdmNodeKind kind)
    {
        foreach (var obj in _source.PreRootShellNodes)
        {
            var node = XDocumentNode.Wrap(obj);
            if (MatchesKind(node.NodeKind, kind))
                yield return XdmValue.FromNode(_source.Wrap(node, recordIndex: -1));
        }
    }

    public XdmSequence Attributes(string? localName = null, string? namespaceUri = null) =>
        _role == StreamingNodeRole.Document
            ? XdmSequence.Empty
            : XdmSequence.FromSource(new EnumerableXdmSequence(WrapEach(_inner.Attributes(localName, namespaceUri))));

    public XdmSequence Axis(XdmAxis axis)
    {
        switch (_role)
        {
            case StreamingNodeRole.Document:
                return axis switch
                {
                    XdmAxis.Child => DocumentChildren(XdmNodeKind.All),
                    // Both forward axes pull the record pump: keep them single-pass
                    // marked so the VM's lookahead sees the same contract as before.
                    XdmAxis.Descendant => Marked(EnumerateDocumentDescendants()),
                    XdmAxis.DescendantOrSelf => Marked(EnumerateSelfThen(EnumerateDocumentDescendants())),
                    XdmAxis.Self => XdmSequence.Singleton(XdmValue.FromNode(this)),
                    _ => XdmSequence.Empty,
                };

            case StreamingNodeRole.ShellRoot:
                return axis switch
                {
                    XdmAxis.Child => _source.ChildRecords(XdmNodeKind.All),
                    XdmAxis.Descendant => _source.DescendantRecords(includeRoot: false),
                    XdmAxis.DescendantOrSelf => _source.DescendantRecords(includeRoot: true),
                    XdmAxis.Self => XdmSequence.Singleton(XdmValue.FromNode(this)),
                    XdmAxis.Attribute => XdmSequence.FromSource(new EnumerableXdmSequence(WrapEach(_inner.Axis(XdmAxis.Attribute)))),
                    XdmAxis.Namespace => XdmSequence.FromSource(new EnumerableXdmSequence(WrapEach(_inner.Axis(XdmAxis.Namespace)))),
                    XdmAxis.Parent => XdmSequence.Singleton(XdmValue.FromNode(_source.DocumentNode)),
                    XdmAxis.Ancestor => XdmSequence.Singleton(XdmValue.FromNode(_source.DocumentNode)),
                    XdmAxis.AncestorOrSelf => XdmSequence.FromSource(new EnumerableXdmSequence(EnumerateSelfAndDocument())),
                    // Nothing follows the root element (post-root nodes are not surfaced),
                    // and pre-root comments/PIs are not its siblings: they are children
                    // of the document, never of the root.
                    _ => XdmSequence.Empty,
                };

            default:
                return RecordAxis(axis);
        }
    }

    private XdmSequence RecordAxis(XdmAxis axis)
    {
        switch (axis)
        {
            case XdmAxis.Self:
                return XdmSequence.Singleton(XdmValue.FromNode(this));
            case XdmAxis.Child:
            case XdmAxis.Descendant:
            case XdmAxis.Attribute:
            case XdmAxis.Namespace:
                return XdmSequence.FromSource(new EnumerableXdmSequence(InnerAxis(axis)));
            case XdmAxis.DescendantOrSelf:
                return XdmSequence.FromSource(new EnumerableXdmSequence(EnumerateSelfThen(InnerAxis(XdmAxis.Descendant))));
            case XdmAxis.Parent:
                return Parent is { } parent
                    ? XdmSequence.Singleton(XdmValue.FromNode(parent))
                    : XdmSequence.Empty;
            case XdmAxis.Ancestor:
                return XdmSequence.FromSource(new EnumerableXdmSequence(EnumerateAncestors(includeSelf: false)));
            case XdmAxis.AncestorOrSelf:
                return XdmSequence.FromSource(new EnumerableXdmSequence(EnumerateAncestors(includeSelf: true)));
            case XdmAxis.FollowingSibling:
                if (IsShellChild)
                    return XdmSequence.Singleton(XdmValue.FromNode(_source.RootNode));
                if (IsTopLevelRecord)
                    return CrossRecordForward();
                return XdmSequence.FromSource(new EnumerableXdmSequence(InnerAxis(axis)));
            case XdmAxis.PrecedingSibling:
                if (IsShellChild)
                    return XdmSequence.Empty;
                if (IsTopLevelRecord && _recordIndex > 0)
                    throw CrossRecordBackward();
                return XdmSequence.FromSource(new EnumerableXdmSequence(InnerAxis(axis)));
            case XdmAxis.Following:
                if (IsShellChild)
                    throw CrossRecordForwardException();
                return XdmSequence.FromSource(new EnumerableXdmSequence(EnumerateFollowing()));
            case XdmAxis.Preceding:
                if (_recordIndex > 0)
                    throw CrossRecordBackward();
                return XdmSequence.FromSource(new EnumerableXdmSequence(InnerAxis(axis)));
            default:
                return XdmSequence.Empty;
        }
    }

    /// <summary>
    /// In-record following nodes, followed by a guard against forward navigation into
    /// records that were released or never materialized: only the last record of a
    /// fully-read stream has a complete, in-record-only answer.
    /// </summary>
    private IEnumerable<XdmValue> EnumerateFollowing()
    {
        foreach (var item in InnerAxis(XdmAxis.Following))
            yield return item;
        if (!IsLastRecordOfFinishedStream)
            throw CrossRecordForwardException();
    }

    private XdmSequence CrossRecordForward()
    {
        if (IsLastRecordOfFinishedStream)
            return XdmSequence.Empty;
        throw CrossRecordForwardException();
    }

    /// <summary>
    /// True when this node belongs to the final record of a stream that has been read to
    /// completion — the only case where cross-record forward axes have an empty (and
    /// therefore complete) answer.
    /// </summary>
    private bool IsLastRecordOfFinishedStream
        => _source.IsPumpDone && _recordIndex == _source.TotalRecords - 1;

    private static StreamingException CrossRecordForwardException()
        => new("Streaming: forward navigation beyond the current streamed record would interleave " +
               "with the single-pass reader. Restructure the transform to process records independently.");

    private static StreamingException CrossRecordBackward()
        => new("Streaming: backward navigation across streamed records is not possible; earlier records " +
               "have been released. Restructure the transform to keep the data it needs (for example in a variable or accumulator).");

    private IEnumerable<XdmValue> EnumerateAncestors(bool includeSelf)
    {
        if (includeSelf)
            yield return XdmValue.FromNode(this);

        if (IsShellChild)
        {
            // Pre-root comments/PIs are children of the document only; the shell root
            // is not their ancestor.
            yield return XdmValue.FromNode(_source.DocumentNode);
            yield break;
        }

        foreach (var item in _inner.Axis(XdmAxis.Ancestor))
        {
            var xn = (XDocumentNode)item.NodeValue!;
            // Hide the throwaway holder of non-element records.
            if (xn.UnderlyingObject is XElement holder
                && holder.Annotation<StreamingSource.TextRecordHolder>() is not null)
                continue;
            yield return XdmValue.FromNode(_source.Wrap(xn, _recordIndex));
        }

        yield return XdmValue.FromNode(_source.RootNode);
        yield return XdmValue.FromNode(_source.DocumentNode);
    }

    private IEnumerable<XdmValue> EnumerateSelfAndDocument()
    {
        yield return XdmValue.FromNode(this);
        yield return XdmValue.FromNode(_source.DocumentNode);
    }

    /// <summary>
    /// The document's descendants in document order: pre-root comments/PIs first, then
    /// the root element followed by all streamed records.
    /// </summary>
    private IEnumerable<XdmValue> EnumerateDocumentDescendants()
    {
        foreach (var item in EnumeratePreRootNodes(XdmNodeKind.All))
            yield return item;
        foreach (var item in _source.DescendantRecords(includeRoot: true))
            yield return item;
    }

    private IEnumerable<XdmValue> EnumerateSelfThen(XdmSequence rest)
    {
        yield return XdmValue.FromNode(this);
        foreach (var item in rest)
            yield return item;
    }

    private IEnumerable<XdmValue> EnumerateSelfThen(IEnumerable<XdmValue> rest)
    {
        yield return XdmValue.FromNode(this);
        foreach (var item in rest)
            yield return item;
    }

    /// <summary>Wraps the results of an inner (in-record) axis, preserving record index.</summary>
    internal IEnumerable<XdmValue> InnerAxis(XdmAxis axis)
        => WrapEach(_inner.Axis(axis));

    private IEnumerable<XdmValue> WrapEach(XdmSequence sequence)
    {
        foreach (var item in sequence)
        {
            if (item.IsNode)
                yield return XdmValue.FromNode(_source.Wrap((XDocumentNode)item.NodeValue!, _recordIndex));
        }
    }

    private XdmSequence Marked(IEnumerable<XdmValue> pull)
        => XdmSequence.FromSource(new StreamingSinglePassSequence(pull.GetEnumerator));

    private static bool MatchesKind(XdmNodeKind actual, XdmNodeKind requested)
        => requested == XdmNodeKind.All || (requested & actual) == actual;

    // ------------------------------------------------------------------
    // Identity — delegates to the underlying XObject so wrappers compare
    // equal regardless of which direction they were reached from
    // ------------------------------------------------------------------

    public bool IsSameNode(IXdmNode other)
    {
        if (other is StreamingNode streaming)
            return _inner.IsSameNode(streaming._inner);
        if (other is XDocumentNode xn)
            return ReferenceEquals(xn.UnderlyingObject, _inner.UnderlyingObject);
        return false;
    }

    public override bool Equals(object? obj)
        => obj is IXdmNode other && IsSameNode(other);

    public override int GetHashCode()
        => _inner.GetHashCode();

    // ------------------------------------------------------------------
    // Serialization — document/root serialization consumes the stream
    // ------------------------------------------------------------------

    public string ToXmlString()
    {
        if (_role == StreamingNodeRole.Record)
            return _inner.ToXmlString();

        var shell = _source.ShellRoot;
        var builder = new System.Text.StringBuilder();
        if (_role == StreamingNodeRole.Document)
        {
            // Pre-root comments/PIs are document-level content and serialize before
            // the root element; the shell root's serialization excludes them.
            foreach (var obj in _source.PreRootShellNodes)
            {
                switch (obj)
                {
                    case XComment comment:
                        builder.Append(comment.ToString(SaveOptions.DisableFormatting));
                        break;
                    case XProcessingInstruction pi:
                        builder.Append(pi.ToString(SaveOptions.DisableFormatting));
                        break;
                }
            }
        }
        if (_source.RootEmpty)
        {
            builder.Append(shell.ToString(SaveOptions.DisableFormatting));
            return builder.ToString();
        }

        var emptyShell = new XElement(shell.Name, shell.Attributes()).ToString(SaveOptions.DisableFormatting);
        builder.Append(emptyShell.AsSpan(0, emptyShell.Length - 2)).Append('>');
        _source.SerializeContent(builder);
        var prefix = shell.GetPrefixOfNamespace(shell.Name.Namespace) ?? string.Empty;
        builder.Append("</").Append(prefix.Length > 0 ? prefix + ":" : string.Empty)
            .Append(shell.Name.LocalName).Append('>');
        return builder.ToString();
    }
}
