// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Read-only capability descriptor advertising what the authoring surface supports.
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

using System.Reflection;

namespace Bosak.Xslt.Authoring;

/// <summary>
/// The fidelity modes one authoring surface can offer. Bosak authoring is lossless-only: untouched
/// regions are always produced from the retained original bytes, never re-serialized.
/// </summary>
public enum AuthoringFidelityMode
{
    /// <summary>Untouched regions export byte-for-byte from the retained envelope; only declared affected spans are spliced.</summary>
    LosslessByteExport,
}

/// <summary>
/// A read-only, exception-free description of what the Bosak authoring surface supports, so a consumer
/// (for example a visual designer) can adapt its UI without probing APIs by trial and error. All
/// members are static and side-effect free; the descriptor holds no mutable state and reflects the
/// engine build it ships in. Values here are contractual: the capabilities advertised are exactly the
/// behaviors implemented by <see cref="AuthoringSource"/>, <see cref="XsltAuthoring"/>,
/// <see cref="AuthoringInspector"/> and <see cref="AuthoringSnapshot"/>.
/// </summary>
public static class AuthoringCapabilities
{
    /// <summary>Gets whether source-preserving inspection of a stylesheet and its modules is available.</summary>
    public static bool SupportsInspection => true;

    /// <summary>
    /// Gets whether expression-slot replacement is supported: <see cref="AuthoringSnapshot.ProposeExpressionEdit"/>
    /// accepts proposals targeting attributes classified as <see cref="AuthoringAttributeSlotKind.Expression"/>.
    /// </summary>
    public static bool SupportsExpressionSlotReplacement => true;

    /// <summary>
    /// Gets whether pattern-slot replacement is supported. <see cref="AuthoringAttributeSlotKind.Pattern"/>
    /// slots (for example <c>xsl:template/@match</c>) are inspectable but not editable in this version;
    /// proposing an edit on one yields <see cref="AuthoringEditFailureKind.SlotNotEditable"/>.
    /// </summary>
    public static bool SupportsPatternSlotReplacement => false;

    /// <summary>
    /// Gets whether attribute-value-template slot replacement is supported. <see cref="AuthoringAttributeSlotKind.Avt"/>
    /// slots are inspectable but not editable in this version.
    /// </summary>
    public static bool SupportsAvtSlotReplacement => false;

    /// <summary>
    /// Gets whether qualified-name slot replacement is supported. <see cref="AuthoringAttributeSlotKind.QName"/>
    /// slots (for example <c>xsl:call-template/@name</c>) are inspectable but not editable in this version.
    /// </summary>
    public static bool SupportsQNameSlotReplacement => false;

    /// <summary>
    /// Gets the offered fidelity modes. Bosak authoring offers <see cref="AuthoringFidelityMode.LosslessByteExport"/>
    /// only: there is no re-serializing export path.
    /// </summary>
    public static IReadOnlyList<AuthoringFidelityMode> FidelityModes { get; } =
        new[] { AuthoringFidelityMode.LosslessByteExport };

    /// <summary>
    /// Gets the canonical names of the encodings authoring accepts, matching
    /// <see cref="AuthoringSource.TryCreate"/> detection exactly: a byte-order mark (UTF-8, UTF-16 LE/BE,
    /// UTF-32 LE/BE), else the <c>encoding</c> pseudo-attribute of an XML declaration (any of the names
    /// listed here), else UTF-8. Any other declared encoding name is refused with
    /// <see cref="AuthoringFailureKind.UnsupportedEncoding"/>; invalid byte sequences are refused with
    /// <see cref="AuthoringFailureKind.InvalidSourceBytes"/>. Detection notes: UTF-16 and UTF-32 are
    /// recognized via BOM; without a BOM their declarations are not byte-detectable and the bytes are
    /// treated as UTF-8 (which fails strict validation). US-ASCII input is validated strictly.
    /// </summary>
    public static IReadOnlyList<string> SupportedEncodings { get; } =
        new[]
        {
            "UTF-8",
            "UTF-16LE",
            "UTF-16BE",
            "UTF-32LE",
            "UTF-32BE",
            "ISO-8859-1",
            "US-ASCII",
        };

    /// <summary>
    /// Gets the maximum number of modules (including the principal) one snapshot inspects. References
    /// beyond the bound are recorded as unresolved edges with a diagnostic. Matches
    /// <see cref="AuthoringInspectionOptions.MaxModuleCount"/>.
    /// </summary>
    public static int MaxModuleCount => 256;

    /// <summary>
    /// Gets the human-readable documented limits a consumer should surface in diagnostics or settings UI.
    /// </summary>
    public static IReadOnlyList<string> DocumentedLimits { get; } =
        new[]
        {
            "Internal DTD subsets are refused: module parsing is strict XML 1.0 without DTD processing.",
            "XML 1.1 declarations are refused by inspection parsing; derived compilation resolves included modules through the Xml11Loader XML 1.1 compatibility path.",
            "UTF-16 and UTF-32 modules are recognized via byte-order mark only; without a BOM the encoding declaration is not detectable and the bytes decode (and fail) as UTF-8.",
            "Node identities are snapshot-scoped: a descriptor id is reliable only within the snapshot that issued it and must not be reused against a candidate or a later inspection.",
            "Edit candidates are valid only against the snapshot they were derived from; a late proposal still resolves against the snapshot it is submitted to.",
            "Attribute editability is limited to Expression slots; Pattern, AVT and QName slots are inspectable but refuse edits.",
            "Snapshots and candidates are pure managed state: they hold no unmanaged resources, need no disposal, never mutate, and are safe to share across threads.",
        };

    /// <summary>
    /// Gets the version policy: XSLT 3.0 / XPath 3.1 is the delivered surface; XPath 4.0 grammar surfaces
    /// are gated by the engine version policy and are opted into per expression slot from the module's
    /// effective version (see <c>CompileOptions.Compatibility</c>).
    /// </summary>
    public static string VersionPolicy =>
        "XSLT 3.0 / XPath 3.1 first; XPath 4.0 surfaces are gated by the engine version policy " +
        "(CompileOptions.Compatibility, driven by the module effective version).";

    /// <summary>
    /// Gets the simple name of the engine assembly the authoring surface ships in, for binding logs
    /// (for example <c>Bosak.Xslt</c>).
    /// </summary>
    public static string EngineAssemblyName => typeof(AuthoringCapabilities).Assembly.GetName().Name ?? "Bosak.Xslt";

    /// <summary>
    /// Gets the engine assembly version a consumer is bound to: the informational (file) version when
    /// available, otherwise the assembly name version. Deterministic per build; intended for binding logs.
    /// </summary>
    public static string EngineVersion
    {
        get
        {
            var assembly = typeof(AuthoringCapabilities).Assembly;
            var informational = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            return string.IsNullOrEmpty(informational)
                ? assembly.GetName().Version?.ToString() ?? "unknown"
                : informational;
        }
    }
}
