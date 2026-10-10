// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Classified outcome taxonomy for captured transformations (REQ-125 Slice C).
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
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-125 Slice D: Cancelled is now triggerable via XsltCaptureOptions.CancellationToken   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Capture;

/// <summary>
/// The distinct outcome of one captured transformation. Every outcome is reported as data; the capture
/// API never throws for expected error conditions (engine errors, spec errors raised by the stylesheet,
/// bound breaches, collisions, cancellation or limit exhaustion).
/// </summary>
public enum XsltCaptureOutcome
{
    /// <summary>
    /// The transformation completed and every output it produced was captured within the configured
    /// bounds: the principal output and all secondary outputs are present with their resolved
    /// identities and exact serialization bytes, and no failure entry exists.
    /// </summary>
    Success,

    /// <summary>
    /// Outputs were captured, but the run is not complete. Either the transformation aborted after at
    /// least one output was captured (the outputs completed earlier are retained as diagnostics and the
    /// aborting condition is a classified failure entry), or the transformation completed but one or
    /// more outputs were dropped because a configured bound was exceeded. Partial capture is never
    /// reported as a successful run.
    /// </summary>
    Partial,

    /// <summary>
    /// The transformation failed before any output could be captured. The failure is classified in
    /// <see cref="XsltCaptureResult.Failures"/>; no output bytes are present.
    /// </summary>
    Failed,

    /// <summary>
    /// The run was terminated by the host-supplied cancellation token (REQ-125 Slice D) at an engine
    /// checkpoint. Outputs completed before the checkpoint are retained on the result as diagnostics
    /// together with a <see cref="XsltCaptureFailureKind.Cancelled"/> failure entry; a run that never
    /// observes the token cannot produce this outcome. Wall-clock and operation-budget limits are
    /// classified as failures (<c>XV0011</c>/<c>XV0012</c>) on a <see cref="Partial"/> or
    /// <see cref="Failed"/> outcome instead.
    /// </summary>
    Cancelled,
}
