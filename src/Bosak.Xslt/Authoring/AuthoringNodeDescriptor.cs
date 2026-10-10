// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Ordered source-backed descriptor tree nodes for one inspected module.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — source-preserving authoring API for visual designers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.Xslt.Authoring;

/// <summary>
/// The kind of one node in an authoring descriptor tree.
/// </summary>
public enum AuthoringNodeKind
{
    /// <summary>The document node.</summary>
    Document,

    /// <summary>An element.</summary>
    Element,

    /// <summary>A text node (including CDATA sections).</summary>
    Text,

    /// <summary>A comment.</summary>
    Comment,

    /// <summary>A processing instruction.</summary>
    ProcessingInstruction,
}

/// <summary>
/// An immutable, ordered node in the source-backed descriptor tree of one module. Every node carries
/// its full source extent; the original bytes behind a range are always recoverable byte-for-byte from
/// the owning snapshot. Node <see cref="Id"/> values are stable within one snapshot only. Instances
/// never re-serialize the source — descriptors point into the retained envelope.
/// </summary>
public sealed class AuthoringNodeDescriptor
{
    private readonly IReadOnlyList<AuthoringNodeDescriptor> _children;

    internal AuthoringNodeDescriptor(
        int id,
        AuthoringNodeKind kind,
        AuthoringQName? elementName,
        string? text,
        SourceRange range,
        IReadOnlyList<AuthoringAttributeDescriptor> attributes,
        IReadOnlyList<AuthoringNodeDescriptor> children,
        XObject? backingObject)
    {
        Id = id;
        Kind = kind;
        ElementName = elementName;
        Text = text;
        Range = range;
        Attributes = attributes;
        _children = children;
        BackingObject = backingObject;
    }

    /// <summary>Gets the snapshot-scoped identity of this node. Reliable only within the owning snapshot.</summary>
    public int Id { get; }

    /// <summary>Gets the kind of this node.</summary>
    public AuthoringNodeKind Kind { get; }

    /// <summary>
    /// Gets the qualified name of the element: namespace, local name and prefix exactly as written.
    /// <see langword="null"/> for non-element nodes.
    /// </summary>
    public AuthoringQName? ElementName { get; }

    /// <summary>
    /// Gets the DOM value of a text, comment or processing-instruction node. This is lossy with respect
    /// to source spelling (entities are expanded, line endings normalized); use the owning snapshot's
    /// original bytes plus <see cref="Range"/> for exact source. <see langword="null"/> for elements and
    /// the document node.
    /// </summary>
    public string? Text { get; }

    /// <summary>
    /// Gets the full half-open source extent of this node. For elements this covers from the <c>&lt;</c>
    /// of the start tag through the <c>&gt;</c> of the end tag (or the self-close <c>/&gt;</c>).
    /// </summary>
    public SourceRange Range { get; }

    /// <summary>Gets the attributes of an element in source order. Empty for non-element nodes.</summary>
    public IReadOnlyList<AuthoringAttributeDescriptor> Attributes { get; }

    /// <summary>Gets the ordered child nodes.</summary>
    public IReadOnlyList<AuthoringNodeDescriptor> Children => _children;

    /// <summary>Gets the derived <see cref="XObject"/> this descriptor was built from, or <see langword="null"/> for the document node.</summary>
    internal XObject? BackingObject { get; }

    /// <summary>
    /// Finds the first descendant (depth-first, including this node) matching a predicate.
    /// </summary>
    /// <param name="predicate">The match predicate.</param>
    /// <returns>The matching node, or <see langword="null"/>.</returns>
    public AuthoringNodeDescriptor? FindDescendant(Func<AuthoringNodeDescriptor, bool> predicate)
    {
        if (predicate(this))
        {
            return this;
        }

        foreach (var child in _children)
        {
            var found = child.FindDescendant(predicate);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }
}
