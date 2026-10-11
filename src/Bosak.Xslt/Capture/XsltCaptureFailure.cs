// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Immutable classified failure entry of a captured transformation (REQ-125 Slice C).
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — controlled validation and preview API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice C)                                                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-125 Slice D: XV0011/XV0012/XV0013 documented                                        |
//                      | Charles Korthout | 0.3   | 11-10-2026     | C-F1: document explicit unsupported serialization profile code XV0014.                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Capture;

/// <summary>
/// One immutable, classified failure of a captured transformation. Failures carry a stable code — the
/// spec code when the underlying condition is a spec error (for example <c>XTDE1490</c>), otherwise a
/// Bosak <c>XV</c> code (<c>XV0006</c>–<c>XV0014</c>) — the resolved identity of the output the failure
/// relates to when one exists, and a human-readable message. The capture API reports these conditions
/// as data; it never throws for them.
/// </summary>
/// <remarks>
/// <c>XV</c> codes are Bosak-specific, not spec codes. <c>XV0006</c> marks a secondary-output count
/// bound breach, <c>XV0007</c> a per-output byte bound breach, <c>XV0008</c> an aggregate byte bound
/// breach, <c>XV0009</c> a transform failure that carried no spec code, <c>XV0010</c> a message bound
/// breach, <c>XV0011</c> a wall-clock limit breach, <c>XV0012</c> an engine-operation budget breach
/// and <c>XV0013</c> host cancellation. Cancellation is additionally reflected in
/// <see cref="XsltCaptureOutcome.Cancelled"/>. Instances are immutable value-like records: they hold
/// no unmanaged resources, need no disposal and are safe to share across threads.
/// </remarks>
public sealed class XsltCaptureFailure
{
    /// <summary>Initializes a new capture failure entry.</summary>
    /// <param name="kind">The classified failure kind.</param>
    /// <param name="code">The stable spec code (for example <c>XTDE1490</c>) or Bosak <c>XV</c> code.</param>
    /// <param name="resolvedUri">The resolved identity of the output this failure relates to, when one exists; <see langword="null"/> when the failure is not attributable to a single output (for example a global transform failure before any output).</param>
    /// <param name="message">The human-readable failure description.</param>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> or <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty.</exception>
    public XsltCaptureFailure(XsltCaptureFailureKind kind, string code, string? resolvedUri, string message)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("The failure code must not be empty.", nameof(code));

        Kind = kind;
        Code = code;
        ResolvedUri = resolvedUri;
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>Gets the classified failure kind.</summary>
    public XsltCaptureFailureKind Kind { get; }

    /// <summary>
    /// Gets the stable spec code (for example <c>XTDE1490</c>, <c>XTDE1500</c>, <c>XTMM9000</c>) or
    /// Bosak <c>XV</c> code for this failure.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the resolved identity of the output this failure relates to, when one exists.
    /// <see langword="null"/> means the failure is not attributable to a single output — for example a
    /// global transform failure raised before any output was produced. For the principal output the
    /// identity is the base output URI (which may be the empty string when the host supplied none).
    /// </summary>
    public string? ResolvedUri { get; }

    /// <summary>Gets the human-readable failure description.</summary>
    public string Message { get; }

    /// <inheritdoc />
    public override string ToString() => $"{Code}: {Message}";
}
