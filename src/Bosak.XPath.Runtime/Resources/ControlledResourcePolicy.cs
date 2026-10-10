// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Opt-in controlled resource policy: one authority, no disk/network fallback, observable receipts (REQ-125 Slice B).
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

using System.Security.Cryptography;

namespace Bosak.XPath.Runtime.Resources;

/// <summary>
/// One controlled resource policy: a single host authority (<see cref="IControlledResourceAuthority"/>)
/// consulted by every engine acquisition path, with denial, abstention and callback errors all
/// refusing the acquisition — never falling back to disk or network. The policy is strictly
/// opt-in: when no policy is attached to an <c>EvaluationContext</c> (or XSLT compiler), every
/// code path behaves exactly as before the policy existed.
/// </summary>
/// <remarks>
/// <para><b>Attachment.</b> Set <see cref="Bosak.XPath.Runtime.Vm.EvaluationContext.ResourcePolicy"/>
/// to cover runtime acquisition (fn:doc, document(), unparsed-text and variants, json-doc,
/// collections, xsl:source-document including the streaming path, and — through nested-context
/// inheritance — fn:transform, xsl:evaluate and streaming pipelines). Set
/// <c>Bosak.Xslt.Api.XsltCompiler.ResourcePolicy</c> to cover compile-time acquisition
/// (xsl:include, xsl:import, xsl:use-package packages, xsl:import-schema location hints,
/// compile-time parameter documents and static (use-when) evaluation).</para>
/// <para><b>Route support.</b> Routes the host does not implement are declared unsupported at
/// construction; the engine refuses acquisitions on them with code XV0005 — never silently
/// runnable without controls. <see cref="ControlledResourceRoute.ExtensionFunction"/> is
/// unsupported by default: the engine cannot intercept host-registered functions, so
/// policy-aware hosts must not register IO-capable extension functions under the controlled
/// profile.</para>
/// <para><b>Receipts.</b> Every acquisition attempt appends one immutable
/// <see cref="ControlledResourceReceipt"/> (thread-safe), carrying the credential-stripped
/// requested and effective URIs, the route, the outcome and — for approvals — the byte count
/// and SHA-256 of the exact bytes consumed.</para>
/// <para><b>XML safety.</b> Approved XML bytes are parsed with DTD processing prohibited and
/// no external resolver, so the DTD/entity policy and limits apply to every XML acquisition
/// path, not only the principal stylesheet.</para>
/// </remarks>
public sealed class ControlledResourcePolicy
{
    /// <summary>Stable engine code surfaced when an acquisition is refused (denied, abstained, or the authority failed).</summary>
    public const string RefusedCode = "XV0004";

    /// <summary>Stable engine code surfaced when a route is not supported under the controlled profile.</summary>
    public const string UnsupportedCode = "XV0005";

    private readonly IControlledResourceAuthority _authority;
    private readonly HashSet<ControlledResourceRoute> _unsupportedRoutes;
    private readonly List<ControlledResourceReceipt> _receipts = new();
    private readonly object _receiptLock = new();

    /// <summary>
    /// Initializes a new controlled resource policy.
    /// </summary>
    /// <param name="authority">The host authority deciding every acquisition request. Must not be null.</param>
    /// <param name="unsupportedRoutes">
    /// Additional routes the host declares unsupported; acquisitions on them are refused with
    /// code XV0005. <see cref="ControlledResourceRoute.ExtensionFunction"/> is always treated as
    /// unsupported and needs not be listed.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="authority"/> is null.</exception>
    public ControlledResourcePolicy(
        IControlledResourceAuthority authority,
        IEnumerable<ControlledResourceRoute>? unsupportedRoutes = null)
    {
        _authority = authority ?? throw new ArgumentNullException(nameof(authority));
        _unsupportedRoutes = new HashSet<ControlledResourceRoute> { ControlledResourceRoute.ExtensionFunction };
        if (unsupportedRoutes is not null)
        {
            foreach (var route in unsupportedRoutes)
                _unsupportedRoutes.Add(route);
        }
    }

    /// <summary>Gets the host authority consulted for every acquisition request.</summary>
    public IControlledResourceAuthority Authority => _authority;

    /// <summary>
    /// Gets whether the supplied route is supported under this policy. Unsupported routes are
    /// refused explicitly (code XV0005) — never silently runnable without controls.
    /// </summary>
    /// <param name="route">The route to query.</param>
    /// <returns><c>true</c> when the route is supported; <c>false</c> when acquisitions on it are refused.</returns>
    public bool IsRouteSupported(ControlledResourceRoute route) => !_unsupportedRoutes.Contains(route);

    /// <summary>
    /// Gets the receipts recorded so far, in acquisition order. The returned list is a
    /// snapshot copy; receipt recording is thread-safe, so parallel or nested evaluations do
    /// not corrupt the record (but interleaving order of concurrent acquisitions is not
    /// guaranteed).
    /// </summary>
    public IReadOnlyList<ControlledResourceReceipt> Receipts
    {
        get
        {
            lock (_receiptLock)
                return _receipts.ToArray();
        }
    }

    /// <summary>
    /// Strips embedded credentials (the URI userinfo, for example <c>user:password@</c>) from
    /// an absolute URI so it is safe for diagnostics, receipts and messages. URIs without
    /// userinfo — and anything that does not parse as an absolute URI — are returned unchanged.
    /// </summary>
    /// <param name="uri">The URI to redact.</param>
    /// <returns>The URI with userinfo removed.</returns>
    public static string RedactCredentials(string uri)
    {
        if (string.IsNullOrEmpty(uri))
            return uri;
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            return uri;
        if (string.IsNullOrEmpty(parsed.UserInfo))
            return uri;
        var builder = new UriBuilder(parsed)
        {
            UserName = string.Empty,
            Password = string.Empty,
        };
        return builder.Uri.AbsoluteUri;
    }

    /// <summary>
    /// Acquires the approved bytes for one resource, recording a receipt. Engine-facing: throws
    /// an <see cref="InvalidOperationException"/> carrying code XV0004 (refused: denied,
    /// abstained, approval malformed, or authority error) or XV0005 (route unsupported) when
    /// the acquisition cannot proceed. The caller must not fall back to disk or network.
    /// </summary>
    /// <param name="requestedUri">The resolved (absolute) requested URI.</param>
    /// <param name="route">The route (purpose) of the acquisition.</param>
    /// <param name="effectiveUri">Receives the credential-stripped effective URI to parse the bytes as.</param>
    /// <returns>The approved, authoritative bytes.</returns>
    internal byte[] AcquireBytes(string requestedUri, ControlledResourceRoute route, out string effectiveUri)
    {
        var approval = Authorize(requestedUri, route);
        if (approval.Bytes is not { } bytes)
            throw Refusal(route, requestedUri, approval.EffectiveUri, "approval did not supply resource bytes");
        effectiveUri = RedactCredentials(approval.EffectiveUri ?? requestedUri);
        Record(route, requestedUri, effectiveUri, ControlledResourceReceiptOutcome.Approved,
            byteCount: bytes.Length, sha256: ToSha256Hex(bytes), message: null);
        return bytes;
    }

    /// <summary>
    /// Acquires the approved member URIs of a collection, recording a receipt. Every member is
    /// authorized separately (tagged <see cref="ControlledResourceRoute.Collection"/>) before it
    /// is loaded. Engine-facing: throws with code XV0004/XV0005 on refusal; the caller must not
    /// fall back to directory enumeration or host hooks.
    /// </summary>
    /// <param name="requestedUri">The resolved (absolute) collection URI (empty for the default collection).</param>
    /// <param name="effectiveUri">Receives the credential-stripped effective collection URI.</param>
    /// <returns>The approved member URIs, in collection order.</returns>
    internal IReadOnlyList<string> AcquireCollectionMembers(string requestedUri, out string effectiveUri)
    {
        var approval = Authorize(requestedUri, ControlledResourceRoute.Collection);
        if (approval.CollectionMembers is not { } members)
            throw Refusal(ControlledResourceRoute.Collection, requestedUri, approval.EffectiveUri,
                "approval did not supply collection members");
        effectiveUri = RedactCredentials(approval.EffectiveUri ?? requestedUri);
        Record(ControlledResourceRoute.Collection, requestedUri, effectiveUri,
            ControlledResourceReceiptOutcome.Approved, byteCount: null, sha256: null, message: null);
        return members;
    }

    /// <summary>
    /// Consults the authority for one request, classifying abstention and callback errors as
    /// refusals (controlled profile: abstain = deny). Records the corresponding receipt.
    /// </summary>
    private ControlledResourceResponse Authorize(string requestedUri, ControlledResourceRoute route)
    {
        if (_unsupportedRoutes.Contains(route))
        {
            Record(route, requestedUri, null, ControlledResourceReceiptOutcome.RefusedUnsupported,
                message: UnsupportedCode);
            throw new InvalidOperationException(
                $"{UnsupportedCode}: Controlled resource route '{route}' is not supported under the controlled profile: {RedactCredentials(requestedUri)}");
        }

        var request = new ControlledResourceRequest(route, requestedUri);
        ControlledResourceResponse? response;
        try
        {
            response = _authority.Authorize(request);
        }
        catch (Exception ex)
        {
            Record(route, requestedUri, null, ControlledResourceReceiptOutcome.AuthorityError, message: ex.GetType().Name);
            throw new InvalidOperationException(
                $"{RefusedCode}: Controlled resource authority failed for '{route}' ({request.RedactedUri}): {ex.GetType().Name}",
                ex);
        }

        if (response is null)
        {
            Record(route, requestedUri, null, ControlledResourceReceiptOutcome.RefusedNoApproval, message: RefusedCode);
            throw new InvalidOperationException(
                $"{RefusedCode}: Controlled resource refused for '{route}' (no authority approval): {request.RedactedUri}");
        }

        if (response.IsDenied)
        {
            Record(route, requestedUri, null, ControlledResourceReceiptOutcome.Denied, message: response.DenyReason);
            var reason = string.IsNullOrEmpty(response.DenyReason) ? string.Empty : $": {response.DenyReason}";
            throw new InvalidOperationException(
                $"{RefusedCode}: Controlled resource denied for '{route}': {request.RedactedUri}{reason}");
        }

        return response;
    }

    private InvalidOperationException Refusal(ControlledResourceRoute route, string requestedUri, string? effectiveUri, string detail)
    {
        var redacted = RedactCredentials(effectiveUri ?? requestedUri);
        Record(route, requestedUri, redacted, ControlledResourceReceiptOutcome.Denied, message: detail);
        return new InvalidOperationException(
            $"{RefusedCode}: Controlled resource refused for '{route}' ({detail}): {redacted}");
    }

    private void Record(
        ControlledResourceRoute route,
        string requestedUri,
        string? effectiveUri,
        ControlledResourceReceiptOutcome outcome,
        long? byteCount = null,
        string? sha256 = null,
        string? message = null)
    {
        var receipt = new ControlledResourceReceipt(
            route,
            RedactCredentials(requestedUri),
            effectiveUri is null ? null : RedactCredentials(effectiveUri),
            outcome,
            byteCount,
            sha256,
            message);
        lock (_receiptLock)
            _receipts.Add(receipt);
    }

    private static string ToSha256Hex(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
