// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : QName value type and the opaque editing context attached to expression/pattern/AVT slots.
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
/// A qualified name as written and resolved in source: the namespace prefix exactly as written, the
/// resolved namespace URI, and the local name.
/// </summary>
/// <param name="Prefix">The namespace prefix as written in source (empty for the default namespace).</param>
/// <param name="NamespaceUri">The resolved namespace URI.</param>
/// <param name="LocalName">The local name.</param>
public sealed record AuthoringQName(string Prefix, string NamespaceUri, string LocalName)
{
    /// <summary>Gets the lexical form as written: <c>prefix:local</c>, or just <c>local</c>.</summary>
    public string LexicalForm => Prefix.Length == 0 ? LocalName : Prefix + ":" + LocalName;

    /// <inheritdoc />
    public override string ToString() => LexicalForm;
}

/// <summary>
/// One in-scope namespace binding: the prefix (empty for the default namespace) and its URI.
/// </summary>
/// <param name="Prefix">The namespace prefix; empty for the default namespace.</param>
/// <param name="Uri">The namespace URI.</param>
public sealed record AuthoringNamespaceBinding(string Prefix, string Uri);

/// <summary>
/// An opaque, engine-owned editing context attached to every <see cref="AuthoringAttributeSlotKind.Expression"/>,
/// <see cref="AuthoringAttributeSlotKind.Pattern"/> and <see cref="AuthoringAttributeSlotKind.Avt"/> slot
/// descriptor. It captures the real static context of the slot — owning element, in-scope namespaces,
/// default XPath namespace, resolved base URI and effective XSLT version — so a consumer can edit an
/// expression in its true context instead of as a naked XPath string. Instances are immutable.
/// </summary>
public sealed class ExpressionSlotContext
{
    internal ExpressionSlotContext(
        AuthoringQName owningElement,
        string attributeName,
        IReadOnlyList<AuthoringNamespaceBinding> inScopeNamespaces,
        string? xpathDefaultNamespace,
        Uri baseUri,
        string effectiveVersion)
    {
        OwningElement = owningElement;
        AttributeName = attributeName;
        InScopeNamespaces = inScopeNamespaces;
        XpathDefaultNamespace = xpathDefaultNamespace;
        BaseUri = baseUri;
        EffectiveVersion = effectiveVersion;
    }

    /// <summary>Gets the qualified name of the element owning the slot.</summary>
    public AuthoringQName OwningElement { get; }

    /// <summary>Gets the attribute name (as written) of the slot.</summary>
    public string AttributeName { get; }

    /// <summary>
    /// Gets the in-scope namespace bindings: all <c>xmlns</c>/<c>xmlns:prefix</c> attributes on the
    /// owning element and its ancestors, outermost first, with the predefined <c>xml</c> binding present.
    /// When several declarations bind the same prefix, the nearest one wins (appears last).
    /// </summary>
    public IReadOnlyList<AuthoringNamespaceBinding> InScopeNamespaces { get; }

    /// <summary>Gets the in-effect <c>xpath-default-namespace</c> for the slot, or <see langword="null"/> when none is in scope.</summary>
    public string? XpathDefaultNamespace { get; }

    /// <summary>Gets the base URI resolved through the xml:base chain over the module base URI.</summary>
    public Uri BaseUri { get; }

    /// <summary>Gets the effective XSLT version in scope (a stylesheet <c>version</c> or literal-result-element <c>xsl:version</c>), verbatim.</summary>
    public string EffectiveVersion { get; }
}
