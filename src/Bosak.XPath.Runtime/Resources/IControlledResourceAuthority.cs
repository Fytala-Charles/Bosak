// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Host authority contract for controlled resource acquisition (REQ-125 Slice B).
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
/// The explicit host authority for every resource acquisition under the controlled profile
/// (see <see cref="ControlledResourcePolicy"/>). The engine consults the authority before it
/// touches disk or network on any wired acquisition route; denial, abstention and callback
/// errors all cause a controlled refusal — never an alternate resolution.
/// </summary>
/// <remarks>
/// Implementations must be thread-safe: a single policy (and thus a single authority) can
/// serve acquisitions from nested and parallel evaluation contexts. Implementations should
/// not perform blocking IO of their own on the calling thread beyond what the host accepts;
/// the engine invokes <see cref="Authorize"/> synchronously.
/// <para>
/// The authority decides per request; whether a whole route is supported is declared on the
/// policy (see <see cref="ControlledResourcePolicy.IsRouteSupported"/>), so routes the host
/// does not implement are refused explicitly (code XV0005) instead of being silently
/// runnable without controls.
/// </para>
/// </remarks>
public interface IControlledResourceAuthority
{
    /// <summary>
    /// Decides one acquisition request.
    /// </summary>
    /// <param name="request">The resolved request (route and absolute URI).</param>
    /// <returns>
    /// An approval (see <see cref="ControlledResourceResponse.Approve(byte[], string?)"/> or
    /// <see cref="ControlledResourceResponse.ApproveCollection(IReadOnlyList{string}, string?)"/>),
    /// a denial (see <see cref="ControlledResourceResponse.Deny"/>), or <see langword="null"/>
    /// to abstain. An abstention is classified by the engine as a refusal (controlled
    /// profile: abstain = deny); the engine never falls back to disk or network.
    /// </returns>
    /// <exception cref="Exception">
    /// Any exception thrown by the authority is treated as a refusal (receipt outcome
    /// <see cref="ControlledResourceReceiptOutcome.AuthorityError"/>); it never enables a
    /// fallback resolution.
    /// </exception>
    ControlledResourceResponse? Authorize(ControlledResourceRequest request);
}
