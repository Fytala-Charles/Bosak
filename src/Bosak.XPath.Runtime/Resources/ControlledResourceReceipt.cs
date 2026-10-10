// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Immutable per-acquisition receipt recorded by the controlled resource policy (REQ-125 Slice B).
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice B)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Runtime.Resources;

/// <summary>
/// The outcome of one recorded acquisition attempt.
/// </summary>
public enum ControlledResourceReceiptOutcome
{
    /// <summary>The authority approved the acquisition; <see cref="ControlledResourceReceipt.Sha256"/> and
    /// <see cref="ControlledResourceReceipt.ByteCount"/> describe the acquired bytes.</summary>
    Approved,

    /// <summary>The authority denied the acquisition (possibly with a reason).</summary>
    Denied,

    /// <summary>The route is not supported under the controlled profile and was refused (code XV0005).</summary>
    RefusedUnsupported,

    /// <summary>The authority abstained (returned null) and the engine classified the abstention as a refusal
    /// (controlled profile: abstain = deny).</summary>
    RefusedNoApproval,

    /// <summary>The authority callback itself raised an exception; the acquisition was refused.</summary>
    AuthorityError,
}

/// <summary>
/// One immutable, observable record of a resource-acquisition attempt made under a
/// <see cref="ControlledResourcePolicy"/>. Receipts let the host (for example Bosak.Braid)
/// attach retained bytes and hashes to a run: every receipt names the requested URI, the
/// effective URI actually used, the route (purpose), the outcome, and — for successful
/// acquisitions — the byte count and SHA-256 of the exact bytes the engine consumed.
/// </summary>
/// <remarks>
/// Receipts never carry credentials: both URIs are already redacted (userinfo stripped) by
/// the policy before a receipt is created. Instances are immutable value-like objects.
/// </remarks>
public sealed class ControlledResourceReceipt
{
    /// <summary>Initializes a new acquisition receipt.</summary>
    /// <param name="route">The route (purpose) the acquisition was requested for.</param>
    /// <param name="requestedUri">The credential-stripped requested URI.</param>
    /// <param name="effectiveUri">The credential-stripped effective URI actually used; null when no acquisition occurred.</param>
    /// <param name="outcome">The outcome of the acquisition attempt.</param>
    /// <param name="byteCount">The number of acquired bytes for approved acquisitions; null otherwise.</param>
    /// <param name="sha256">The lowercase hex SHA-256 of the acquired bytes for approved acquisitions; null otherwise.</param>
    /// <param name="message">An optional human-readable detail (for example a denial reason or the refusal code).</param>
    public ControlledResourceReceipt(
        ControlledResourceRoute route,
        string requestedUri,
        string? effectiveUri,
        ControlledResourceReceiptOutcome outcome,
        long? byteCount = null,
        string? sha256 = null,
        string? message = null)
    {
        Route = route;
        RequestedUri = requestedUri ?? throw new ArgumentNullException(nameof(requestedUri));
        EffectiveUri = effectiveUri;
        Outcome = outcome;
        ByteCount = byteCount;
        Sha256 = sha256;
        Message = message;
    }

    /// <summary>Gets the route (purpose) the acquisition was requested for.</summary>
    public ControlledResourceRoute Route { get; }

    /// <summary>Gets the requested URI with embedded credentials stripped.</summary>
    public string RequestedUri { get; }

    /// <summary>
    /// Gets the effective URI actually used for the acquisition (the authority-supplied
    /// effective URI, or the requested URI when the authority supplied none), credential-stripped.
    /// Null when the acquisition did not occur (denial, refusal, error).
    /// </summary>
    public string? EffectiveUri { get; }

    /// <summary>Gets the outcome of the acquisition attempt.</summary>
    public ControlledResourceReceiptOutcome Outcome { get; }

    /// <summary>Gets the number of bytes acquired for approved acquisitions; null otherwise.</summary>
    public long? ByteCount { get; }

    /// <summary>Gets the lowercase hex SHA-256 of the acquired bytes for approved acquisitions; null otherwise.</summary>
    public string? Sha256 { get; }

    /// <summary>Gets an optional human-readable detail (for example a denial reason or refusal code).</summary>
    public string? Message { get; }

    /// <summary>Gets whether the acquisition was approved and consumed by the engine.</summary>
    public bool IsApproved => Outcome == ControlledResourceReceiptOutcome.Approved;

    /// <inheritdoc />
    public override string ToString()
        => $"{Outcome} {Route}: {RequestedUri}" + (EffectiveUri is not null && EffectiveUri != RequestedUri ? $" -> {EffectiveUri}" : "");
}
