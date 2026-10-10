// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Attribute descriptors with raw/expanded values, source ranges and slot classification.
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

namespace Bosak.Xslt.Authoring;

/// <summary>
/// Classifies the role of an attribute in its owning element, so consumers know whether a value is an
/// XPath expression, a pattern, an attribute value template, a qualified name, or plain data.
/// Unclassified combinations always fall back to <see cref="Plain"/>; classification never throws.
/// </summary>
public enum AuthoringAttributeSlotKind
{
    /// <summary>The value is an XPath expression (for example <c>select</c>, <c>test</c>, <c>use-when</c>).</summary>
    Expression,

    /// <summary>The value is an XSLT match pattern (for example <c>xsl:template/@match</c>).</summary>
    Pattern,

    /// <summary>The value is an attribute value template (curly-brace expansion applies).</summary>
    Avt,

    /// <summary>The value is a literal qualified name (for example <c>xsl:call-template/@name</c>).</summary>
    QName,

    /// <summary>The value carries no special role; it is plain data.</summary>
    Plain,
}

/// <summary>
/// An immutable description of one attribute occurrence, exactly as written in the retained source.
/// </summary>
public sealed class AuthoringAttributeDescriptor
{
    internal AuthoringAttributeDescriptor(
        string name,
        string prefix,
        string namespaceUri,
        string localName,
        AuthoringAttributeSlotKind slotKind,
        string rawLiteral,
        string expandedValue,
        SourceRange valueRange,
        SourceRange fullRange,
        ExpressionSlotContext? slotContext)
    {
        Name = name;
        Prefix = prefix;
        NamespaceUri = namespaceUri;
        LocalName = localName;
        SlotKind = slotKind;
        RawLiteral = rawLiteral;
        ExpandedValue = expandedValue;
        ValueRange = valueRange;
        FullRange = fullRange;
        SlotContext = slotContext;
    }

    /// <summary>Gets the attribute name as written in source (including any prefix).</summary>
    public string Name { get; }

    /// <summary>Gets the namespace prefix as written, or the empty string when there is none.</summary>
    public string Prefix { get; }

    /// <summary>Gets the resolved namespace URI of the attribute (empty for no namespace).</summary>
    public string NamespaceUri { get; }

    /// <summary>Gets the local name of the attribute.</summary>
    public string LocalName { get; }

    /// <summary>Gets the classified role of the attribute value.</summary>
    public AuthoringAttributeSlotKind SlotKind { get; }

    /// <summary>
    /// Gets the attribute value exactly as written, between the quotes: entity references and character
    /// references keep their source spelling (for example <c>a &amp;amp; b</c>), including newlines.
    /// </summary>
    public string RawLiteral { get; }

    /// <summary>
    /// Gets the DOM-expanded attribute value. This is lossy with respect to source spelling; use
    /// <see cref="RawLiteral"/> for anything source-preserving.
    /// </summary>
    public string ExpandedValue { get; }

    /// <summary>Gets the half-open source range of the value between the quotes.</summary>
    public SourceRange ValueRange { get; }

    /// <summary>Gets the half-open source range covering the whole attribute (name through closing quote).</summary>
    public SourceRange FullRange { get; }

    /// <summary>
    /// Gets the opaque editing context for <see cref="AuthoringAttributeSlotKind.Expression"/>,
    /// <see cref="AuthoringAttributeSlotKind.Pattern"/> and <see cref="AuthoringAttributeSlotKind.Avt"/>
    /// slots; <see langword="null"/> for other slots.
    /// </summary>
    public ExpressionSlotContext? SlotContext { get; }
}
