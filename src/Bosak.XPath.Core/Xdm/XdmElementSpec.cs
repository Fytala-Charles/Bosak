// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 July 2026
// PURPOSE              : Provider-neutral element construction data passed to the registered element-constructor hook.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-07-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 25-07-2026     | Added BaseUri for static-base-URI annotation on constructed elements                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 25-07-2026     | Added XdmContentKind.Namespace for computed namespace constructors                      |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 28-10-2026     | Expose namespace-copy policy on node content without changing constructor signatures (1.31.0). |
//                      | Charles Korthout | 0.5   | 28-10-2026     | Add compatible construction-mode and copied-attribute inputs (1.32.0). |
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>The schema annotation policy used by compiled XQuery constructors.</summary>
public enum XdmConstructionMode
{
    /// <summary>Preserve copied node metadata; newly constructed elements have type xs:anyType.</summary>
    Preserve,
    /// <summary>Strip copied metadata; elements are xs:untyped and attributes xs:untypedAtomic.</summary>
    Strip
}

/// <summary>The kind of one piece of element content produced by a constructor.</summary>
public enum XdmContentKind
{
    /// <summary>A text node.</summary>
    Text,
    /// <summary>An existing node to be deep-copied into the new element.</summary>
    Node,
    /// <summary>A comment node.</summary>
    Comment,
    /// <summary>A processing-instruction node.</summary>
    ProcessingInstruction,
    /// <summary>A namespace node.</summary>
    Namespace
}

/// <summary>
/// One piece of element content: a text node (<see cref="Text"/>), an existing node to
/// deep-copy (<see cref="Node"/>), a comment (<see cref="Text"/>), or a processing
/// instruction (<see cref="Target"/> + <see cref="Text"/>).
/// </summary>
/// <param name="Kind">The content kind.</param>
/// <param name="Text">The text/comment/PI-data payload, when applicable.</param>
/// <param name="Node">The node to deep-copy, when <paramref name="Kind"/> is <see cref="XdmContentKind.Node"/>.</param>
/// <param name="Target">The PI target, when <paramref name="Kind"/> is <see cref="XdmContentKind.ProcessingInstruction"/>.</param>
public sealed record XdmContentItem(XdmContentKind Kind, string? Text = null, XdmValue? Node = null, string? Target = null)
{
    /// <summary>Whether this item is lexical child markup rather than a copied enclosed-expression value.</summary>
    public bool IsLiteralElement { get; init; }

    /// <summary>The constructor's schema-copy policy; null retains legacy direct-hook behavior.</summary>
    public XdmConstructionMode? ConstructionMode { get; init; }

    /// <summary>Whether a copied node retains unused source namespace bindings; default true.</summary>
    public bool PreserveNamespaces { get; init; } = true;
    /// <summary>Whether a copied node inherits its constructor's namespace bindings; default true.</summary>
    public bool InheritNamespaces { get; init; } = true;
}

/// <summary>Attribute data for element construction through the element-constructor hook.</summary>
/// <param name="LocalName">The attribute's local name.</param>
/// <param name="Prefix">The namespace prefix used in the source, or null.</param>
/// <param name="NamespaceUri">The resolved namespace URI, or null for no namespace.</param>
/// <param name="Value">The computed attribute value.</param>
public sealed record XdmAttributeValue(string LocalName, string? Prefix, string? NamespaceUri, string Value)
{
    /// <summary>The original attribute when copying existing node content, or null for a new attribute.</summary>
    public IXdmNode? SourceNode { get; init; }
    /// <summary>The policy for a standalone attribute hook; null retains legacy hook behavior.</summary>
    public XdmConstructionMode? ConstructionMode { get; init; }
}

/// <summary>
/// Provider-neutral element-construction input: the tag, computed attributes, and content
/// items. The registered hook (see EvaluationContext.ElementConstructorHook) builds the
/// provider-specific node from this data.
/// </summary>
/// <param name="LocalName">The element's local name.</param>
/// <param name="Prefix">The namespace prefix used in the source, or null.</param>
/// <param name="NamespaceUri">The resolved namespace URI, or null for no namespace.</param>
/// <param name="Attributes">The computed attributes.</param>
/// <param name="Content">The content items in document order.</param>
/// <param name="BaseUri">The static base URI of the query, annotated on the constructed element.</param>
/// <param name="Xml11Mode">When true, XML 1.1 semantics apply (prefixed namespace undeclarations are accepted).</param>
public sealed record XdmElementSpec(
    string LocalName,
    string? Prefix,
    string? NamespaceUri,
    IReadOnlyList<XdmAttributeValue> Attributes,
    IReadOnlyList<XdmContentItem> Content,
    string? BaseUri = null,
    bool Xml11Mode = false)
{
    /// <summary>The schema policy for the new element and copied content; null retains legacy hook behavior.</summary>
    public XdmConstructionMode? ConstructionMode { get; init; }
}
