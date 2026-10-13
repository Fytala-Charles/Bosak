// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Classified failure kinds for captured transformations (REQ-125 Slice C).
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
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-125 Slice D: TimeLimitExceeded/XV0011, OperationLimitExceeded/XV0012, Cancelled/    |
//                      |                  |       |                | XV0013 (appended; existing members unchanged)                                            |
//                      | Charles Korthout | 0.3   | 11-10-2026     | C-F1: explicit unsupported bounded serialization profile (XV0014).                      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Capture;

/// <summary>
/// One kind of classified failure on a captured transformation. Failures are reported as data on
/// <see cref="XsltCaptureResult.Failures"/>; the capture API never throws for these expected
/// conditions. A failure entry always carries a stable code: the spec code when the underlying
/// condition is a spec error (for example <c>XTDE1490</c> for a duplicate output URI), otherwise a
/// Bosak <c>XV</c> code — see <see cref="XsltCaptureFailure.Code"/>.
/// </summary>
public enum XsltCaptureFailureKind
{
    /// <summary>
    /// Two writes targeted the same resolved output URI. The engine raises XTDE1490 and aborts the
    /// transformation; outputs completed before the colliding write are retained. Code: <c>XTDE1490</c>.
    /// </summary>
    UriCollision,

    /// <summary>
    /// A secondary output would exceed <see cref="XsltCaptureOptions.MaxSecondaryOutputCount"/>. The
    /// transformation is terminated at the offending output. Code: <c>XV0006</c>.
    /// </summary>
    OutputCountLimitExceeded,

    /// <summary>
    /// A single output's serialization bytes exceed <see cref="XsltCaptureOptions.MaxBytesPerOutput"/>.
    /// The offending output is dropped and the transformation is terminated. Code: <c>XV0007</c>.
    /// </summary>
    OutputSizeLimitExceeded,

    /// <summary>
    /// Capturing an output would raise the total captured bytes over
    /// <see cref="XsltCaptureOptions.MaxTotalBytes"/>. The offending output is dropped and the
    /// transformation is terminated. Code: <c>XV0008</c>.
    /// </summary>
    AggregateSizeLimitExceeded,

    /// <summary>
    /// The <c>xsl:message</c> count exceeded <see cref="XsltCaptureOptions.MaxMessages"/>; messages
    /// beyond the bound were discarded. The transformation itself is not terminated by this failure.
    /// Code: <c>XV0010</c>.
    /// </summary>
    MessageCountLimitExceeded,

    /// <summary>
    /// The transformation aborted with an error: an engine or spec error raised by the stylesheet (for
    /// example <c>XTMM9000</c> for <c>xsl:message terminate="yes"</c>, or <c>XTDE1500</c> for a write to
    /// a previously read URI). Outputs captured before the abort are retained. Code: the spec code when
    /// one is available, otherwise <c>XV0009</c> (uncoded engine failure).
    /// </summary>
    TransformFailure,

    /// <summary>
    /// The wall-clock budget (<see cref="XsltCaptureOptions.TimeLimit"/>) was exceeded at an engine
    /// checkpoint. Outputs captured before the checkpoint are retained. Code: <c>XV0011</c>.
    /// </summary>
    TimeLimitExceeded,

    /// <summary>
    /// The engine-operation budget (<see cref="XsltCaptureOptions.MaxEngineOperations"/>) was exhausted.
    /// Outputs captured before the checkpoint are retained; the abort point is deterministic for a
    /// given stylesheet, input and budget. Code: <c>XV0012</c>.
    /// </summary>
    OperationLimitExceeded,

    /// <summary>
    /// The run was terminated by the host-supplied cancellation token
    /// (<see cref="XsltCaptureOptions.CancellationToken"/>) at an engine checkpoint. Outputs captured
    /// before the checkpoint are retained; the outcome is
    /// <see cref="XsltCaptureOutcome.Cancelled"/>. Code: <c>XV0013</c>.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The requested byte-bounded serialization profile cannot be streamed faithfully. No
    /// serialization is attempted for this output; prior outputs are retained. Code: <c>XV0014</c>.
    /// Currently covers JSON/adaptive, text Unicode normalization, and XML provider fallbacks.
    /// </summary>
    UnsupportedSerializationProfile,
}
