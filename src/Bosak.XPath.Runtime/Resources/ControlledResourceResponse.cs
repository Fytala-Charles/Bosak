// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Approval/denial decision returned by the controlled resource authority (REQ-125 Slice B).
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
/// The decision of the host authority for one <see cref="ControlledResourceRequest"/>.
/// An approval hands back the approved bytes (and optionally an effective URI that differs
/// from the requested one, plus collection members for the collection route); a denial
/// refuses the acquisition. The engine never falls back to disk or network for a denied,
/// abstained or failed request.
/// </summary>
/// <remarks>
/// Use <see cref="Approve(byte[], string?)"/> for single-resource routes,
/// <see cref="ApproveCollection(IReadOnlyList{string}, string?)"/> for the collection route,
/// and <see cref="Deny(string?)"/> for a refusal. An authority that returns
/// <see langword="null"/> from <see cref="IControlledResourceAuthority.Authorize"/> abstains;
/// the engine classifies an abstention as a refusal (controlled profile: abstain = deny).
/// </remarks>
public sealed class ControlledResourceResponse
{
    private ControlledResourceResponse(
        bool approved,
        byte[]? bytes,
        IReadOnlyList<string>? collectionMembers,
        string? effectiveUri,
        string? denyReason)
    {
        IsApproved = approved;
        Bytes = bytes;
        CollectionMembers = collectionMembers;
        EffectiveUri = effectiveUri;
        DenyReason = denyReason;
    }

    /// <summary>Gets whether the acquisition is approved. When <see langword="false"/>, the request is denied.</summary>
    public bool IsApproved { get; }

    /// <summary>Gets whether the request is denied.</summary>
    public bool IsDenied => !IsApproved;

    /// <summary>
    /// Gets the approved content bytes. These bytes are authoritative: they win over any
    /// conflicting content at the requested (or effective) URI on disk or network. Null for
    /// denials and for collection approvals that only supply members (see
    /// <see cref="CollectionMembers"/>).
    /// </summary>
    public byte[]? Bytes { get; }

    /// <summary>
    /// Gets the approved member URIs of a collection (<see cref="ApproveCollection(IReadOnlyList{string}, string?)"/>).
    /// Every member is authorized again through <see cref="ControlledResourceRoute.Collection"/> before it is
    /// loaded, so the authority sees — and controls — each member acquisition.
    /// </summary>
    public IReadOnlyList<string>? CollectionMembers { get; }

    /// <summary>
    /// Gets the effective URI the approved bytes correspond to (used as base/document URI of
    /// the parsed resource). Null means the requested URI is the effective URI.
    /// </summary>
    public string? EffectiveUri { get; }

    /// <summary>Gets the human-readable denial reason, when supplied by the authority.</summary>
    public string? DenyReason { get; }

    /// <summary>
    /// Approves the acquisition with authoritative content bytes.
    /// </summary>
    /// <param name="bytes">The approved bytes; must not be null.</param>
    /// <param name="effectiveUri">
    /// Optional effective URI the bytes correspond to. When supplied, the engine parses the
    /// bytes as if they had been retrieved from this URI; no resolution of this URI against
    /// disk or network is performed.
    /// </param>
    /// <returns>An approval response.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null.</exception>
    public static ControlledResourceResponse Approve(byte[] bytes, string? effectiveUri = null)
        => new(approved: true, bytes ?? throw new ArgumentNullException(nameof(bytes)), null, effectiveUri, null);

    /// <summary>
    /// Approves a collection acquisition with the ordered member URIs. The bytes of every
    /// member are requested from the authority separately (tagged
    /// <see cref="ControlledResourceRoute.Collection"/>) before the member is loaded.
    /// </summary>
    /// <param name="memberUris">The member document URIs, in collection order.</param>
    /// <param name="effectiveUri">Optional effective URI of the collection itself.</param>
    /// <returns>An approval response.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="memberUris"/> is null.</exception>
    public static ControlledResourceResponse ApproveCollection(IReadOnlyList<string> memberUris, string? effectiveUri = null)
        => new(approved: true, null, memberUris ?? throw new ArgumentNullException(nameof(memberUris)), effectiveUri, null);

    /// <summary>
    /// Denies the acquisition. The engine surfaces a controlled refusal (code XV0004) and
    /// performs no alternate disk or network resolution.
    /// </summary>
    /// <param name="reason">An optional human-readable denial reason; credentials must already be stripped by the authority.</param>
    /// <returns>A denial response.</returns>
    public static ControlledResourceResponse Deny(string? reason = null)
        => new(approved: false, null, null, null, reason);
}
