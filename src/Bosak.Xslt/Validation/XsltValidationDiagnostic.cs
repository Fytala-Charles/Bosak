// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Immutable validation diagnostic: stable code, module identity, message, source range.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — controlled validation and preview API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice A)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.Xslt.Authoring;

namespace Bosak.Xslt.Validation;

/// <summary>
/// One immutable diagnostic produced by static validation. Diagnostics carry a stable engine/spec
/// error code, the identity of the module the diagnostic belongs to, a human-readable message and —
/// where available — a source-backed range. A missing range is explicit: <see cref="Range"/> is
/// <see langword="null"/> and <see cref="LocationAvailable"/> is <see langword="false"/>; locations
/// are never fabricated.
/// </summary>
/// <remarks>
/// Codes are either spec/engine codes surfaced by the underlying compile (for example
/// <c>XPST0003</c>, <c>XPST0008</c>, <c>XPST0081</c>, <c>XTSE0340</c>, <c>XTSE0350</c>) or Bosak
/// validation codes in the <c>XV</c> namespace: <c>XV0001</c> marks module source that could not be
/// decoded or parsed at all (structural/encoding failures relayed from the authoring layer),
/// <c>XV0002</c> marks a module-resolution refusal, and <c>XV0003</c> marks an engine failure that
/// did not carry a spec code. <c>XV</c> codes are Bosak-specific, not spec codes.
/// Instances are immutable value-like records: they hold no unmanaged resources, need no disposal and
/// are safe to share across threads.
/// </remarks>
public sealed class XsltValidationDiagnostic
{
    /// <summary>Initializes a new validation diagnostic.</summary>
    /// <param name="code">The stable engine/spec error code (for example <c>XPST0003</c>) or a Bosak <c>XV</c> code.</param>
    /// <param name="moduleUri">The absolute URI of the module the diagnostic belongs to.</param>
    /// <param name="message">The human-readable diagnostic description.</param>
    /// <param name="range">The source-backed range the diagnostic relates to, when available; <see langword="null"/> makes the missing location explicit.</param>
    /// <exception cref="ArgumentNullException"><paramref name="code"/>, <paramref name="moduleUri"/> or <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty.</exception>
    public XsltValidationDiagnostic(string code, Uri moduleUri, string message, SourceRange? range = null)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException("The diagnostic code must not be empty.", nameof(code));
        }

        Code = code;
        ModuleUri = moduleUri ?? throw new ArgumentNullException(nameof(moduleUri));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Range = range;
    }

    /// <summary>Gets the stable engine/spec error code (for example <c>XPST0003</c>) or a Bosak <c>XV</c> code.</summary>
    public string Code { get; }

    /// <summary>Gets the absolute URI of the module this diagnostic belongs to.</summary>
    public Uri ModuleUri { get; }

    /// <summary>Gets the human-readable diagnostic description.</summary>
    public string Message { get; }

    /// <summary>
    /// Gets the source-backed range this diagnostic relates to, when available. <see langword="null"/>
    /// means no source-backed location could be determined — see <see cref="LocationAvailable"/>.
    /// </summary>
    public SourceRange? Range { get; }

    /// <summary>
    /// Gets whether a source-backed location is available for this diagnostic. When
    /// <see langword="false"/>, <see cref="Range"/> is <see langword="null"/>; the engine reports the
    /// absence explicitly instead of fabricating coordinates.
    /// </summary>
    public bool LocationAvailable => Range is not null;

    /// <inheritdoc />
    public override string ToString() => $"{Code}: {Message} ({ModuleUri})";
}
