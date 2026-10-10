// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Internal control-flow exception terminating a captured run at a configured bound.
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
// ===========================================================================================================================================================

namespace Bosak.Xslt.Capture;

/// <summary>
/// Internal control-flow exception used to terminate a captured transformation at the first output
/// that breaches a configured capture bound (count, per-output or aggregate bytes). The breach is
/// recorded as a classified <see cref="XsltCaptureFailure"/> before this exception is thrown; the
/// public capture API catches it and reports the already-recorded failure — it never escapes to the
/// caller. When a stylesheet's <c>xsl:try</c> intercepts it, the recorded failure still downgrades
/// the outcome to <see cref="XsltCaptureOutcome.Partial"/>.
/// </summary>
internal sealed class XsltCaptureLimitException : Exception
{
    internal XsltCaptureLimitException(XsltCaptureFailureKind kind, string? resolvedUri)
        : base($"Capture bound exceeded ({kind}) at output '{resolvedUri ?? "(none)"}'.")
    {
        Kind = kind;
        ResolvedUri = resolvedUri;
    }

    internal XsltCaptureFailureKind Kind { get; }

    internal string? ResolvedUri { get; }
}
