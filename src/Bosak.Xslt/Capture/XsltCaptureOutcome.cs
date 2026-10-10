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
// ===========================================================================================================================================================

namespace Bosak.Xslt.Capture;

/// <summary>
/// The distinct outcome of one captured transformation. Every outcome is reported as data; the capture
/// API never throws for expected error conditions (engine errors, spec errors raised by the stylesheet,
/// bound breaches, collisions). The taxonomy is shaped for the full REQ-125 controlled profile:
/// cancellation is reserved for Slice D and cannot be produced by this build.
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
    /// Reserved for REQ-125 Slice D (execution control): the transformation was cancelled through the
    /// cooperative cancellation contract. No build up to and including Slice C can produce this
    /// outcome; it exists so consumers can code against the final taxonomy now.
    /// </summary>
    Cancelled,
}
