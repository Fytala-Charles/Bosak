// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Module graph edges (import/include references) and per-module descriptors.
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
/// The kind of a module-graph edge.
/// </summary>
public enum AuthoringModuleEdgeKind
{
    /// <summary>An <c>xsl:import</c> reference. Import precedence applies across modules.</summary>
    Import,

    /// <summary>An <c>xsl:include</c> reference.</summary>
    Include,
}

/// <summary>
/// An immutable record of one <c>xsl:include</c>/<c>xsl:import</c> occurrence in a module. The edge
/// captures the href exactly as written, the resolved absolute URI (when resolution succeeded), the
/// source range of the referencing element, and — when anything went wrong — a classified diagnostic.
/// A missing module never fails inspection of the referencing module.
/// </summary>
public sealed class AuthoringModuleEdge
{
    internal AuthoringModuleEdge(
        AuthoringModuleEdgeKind kind,
        string hrefLiteral,
        Uri? resolvedUri,
        SourceRange referenceRange,
        AuthoringFailure? diagnostic,
        bool reusesExistingModule)
    {
        Kind = kind;
        HrefLiteral = hrefLiteral;
        ResolvedUri = resolvedUri;
        ReferenceRange = referenceRange;
        Diagnostic = diagnostic;
        ReusesExistingModule = reusesExistingModule;
    }

    /// <summary>Gets whether this edge is an import or an include.</summary>
    public AuthoringModuleEdgeKind Kind { get; }

    /// <summary>Gets the href exactly as written in source.</summary>
    public string HrefLiteral { get; }

    /// <summary>Gets the absolute URI the href resolved to, or <see langword="null"/> when unresolved.</summary>
    public Uri? ResolvedUri { get; }

    /// <summary>Gets the full source range of the referencing include/import element.</summary>
    public SourceRange ReferenceRange { get; }

    /// <summary>Gets the classified diagnostic when resolution failed, a cycle was detected or module bytes were invalid.</summary>
    public AuthoringFailure? Diagnostic { get; }

    /// <summary>
    /// Gets whether the resolved module had already been inspected in this snapshot; the snapshot reuses
    /// the same envelope and descriptor for both references (module identity is the absolute URI).
    /// </summary>
    public bool ReusesExistingModule { get; }
}

/// <summary>
/// An immutable description of one inspected module: provenance (URI, encoding, byte length), the
/// verbatim version, its include/import edges and the ordered source-backed descriptor tree rooted at
/// the document node.
/// </summary>
public sealed class AuthoringModuleDescriptor
{
    internal AuthoringModuleDescriptor(
        Uri moduleUri,
        bool isPrincipal,
        string encodingName,
        long originalByteLength,
        string? version,
        IReadOnlyList<AuthoringModuleEdge> edges,
        AuthoringNodeDescriptor root,
        int precedence)
    {
        ModuleUri = moduleUri;
        IsPrincipal = isPrincipal;
        EncodingName = encodingName;
        OriginalByteLength = originalByteLength;
        Version = version;
        Edges = edges;
        Root = root;
        Precedence = precedence;
    }

    /// <summary>Gets the absolute URI identifying this module.</summary>
    public Uri ModuleUri { get; }

    /// <summary>Gets whether this is the principal (entry) module of the snapshot.</summary>
    public bool IsPrincipal { get; }

    /// <summary>Gets the display name of the detected encoding.</summary>
    public string EncodingName { get; }

    /// <summary>Gets the exact length of the original module bytes, including any byte-order mark.</summary>
    public long OriginalByteLength { get; }

    /// <summary>
    /// Gets the verbatim version of the stylesheet (<c>xsl:stylesheet/@version</c> or a literal-result
    /// element's <c>xsl:version</c>), or <see langword="null"/> when no version attribute is present.
    /// The string is reported exactly as written and never refused.
    /// </summary>
    public string? Version { get; }

    /// <summary>Gets the include/import edges declared by this module, in document order.</summary>
    public IReadOnlyList<AuthoringModuleEdge> Edges { get; }

    /// <summary>
    /// Gets the module processing order within the snapshot: 0 is the principal module, later numbers
    /// are modules discovered later through include/import references in document order. Because
    /// <c>xsl:import</c> must precede <c>xsl:include</c> in document order, lower numbers also imply
    /// lower import precedence.
    /// </summary>
    public int Precedence { get; }

    /// <summary>Gets the descriptor tree of this module, rooted at the document node.</summary>
    public AuthoringNodeDescriptor Root { get; }
}
