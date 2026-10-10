// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Optional controls for static validation, including the language profile override.
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
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-125 Slice D: CancellationToken — cooperative, element-boundary checkpoints,         |
//                      |                  |       |                | classified Cancelled outcome                                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;

namespace Bosak.Xslt.Validation;

/// <summary>
/// Optional controls for <see cref="XsltValidation.Validate(Authoring.AuthoringSnapshot, XsltValidationOptions?)"/>
/// and its overloads. All settings are opt-in; the defaults follow the declared module content
/// exactly (effective version per slot, detected encodings, supplied resolver).
/// </summary>
public sealed class XsltValidationOptions
{
    /// <summary>
    /// Gets or sets a profile override applied to every expression, pattern and AVT segment
    /// compiled during validation, regardless of the slot's effective version. <see langword="null"/>
    /// (the default) validates each slot at the XPath version implied by its module's effective
    /// version (XPath 4.0 when the effective version starts with <c>4</c>, XPath 3.1 otherwise).
    /// Forcing <see cref="XPathCompatibility.XPath31"/> reports XPath 4.0-only constructs (XPST0017)
    /// even in a version 4.0 stylesheet; forcing <see cref="XPathCompatibility.XPath40"/> accepts
    /// them.
    /// </summary>
    public XPathCompatibility? CompatibilityOverride { get; set; }

    /// <summary>
    /// Gets or sets the cooperative cancellation token for the validation walk. The token is
    /// observed at element boundaries of every walked module; triggering it stops the walk and
    /// returns a result with <see cref="XsltValidationOutcome.Cancelled"/>, retaining the
    /// diagnostics collected before the boundary that observed the trigger. <see langword="null"/>
    /// (the default) disables cancellation. Cancellation never surfaces as
    /// <see cref="OperationCanceledException"/> through the validation API, and the validated
    /// snapshot and module envelopes remain unchanged.
    /// </summary>
    public CancellationToken? CancellationToken { get; set; }
}
