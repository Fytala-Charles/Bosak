// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Immutable acquisition request handed to the controlled resource authority (REQ-125 Slice B).
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
//                      | Charles Korthout | 0.2   | 10-10-2026     | B-F2: additive ConcreteRoute — the concrete operation behind a phase/purpose route       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Runtime.Resources;

/// <summary>
/// One resource-acquisition request presented to the host authority
/// (<see cref="IControlledResourceAuthority"/>) before the engine touches disk or network.
/// The request carries the resolved identity of the resource and the route (purpose) for
/// which it is being acquired.
/// </summary>
/// <remarks>
/// Instances are immutable value-like objects: they are safe to share across threads and to
/// retain beyond the acquisition call.
/// </remarks>
public sealed class ControlledResourceRequest
{
    /// <summary>Initializes a new acquisition request.</summary>
    /// <param name="route">The route (purpose) for which the resource is being acquired.</param>
    /// <param name="uri">The resolved (absolute) URI of the requested resource.</param>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is empty.</exception>
    public ControlledResourceRequest(ControlledResourceRoute route, string uri)
        : this(route, uri, null)
    {
    }

    /// <summary>
    /// Initializes a new acquisition request that names both a phase/purpose route and the
    /// concrete operation performed under it (REQ-125 consumer review B-F2): for example a
    /// static-evaluation document read is presented as route <see cref="ControlledResourceRoute.StaticEvaluation"/>
    /// with concrete route <see cref="ControlledResourceRoute.Document"/>.
    /// </summary>
    /// <param name="route">The route (purpose) for which the resource is being acquired.</param>
    /// <param name="uri">The resolved (absolute) URI of the requested resource.</param>
    /// <param name="concreteRoute">The concrete operation behind <paramref name="route"/>;
    /// null when <paramref name="route"/> is already the concrete operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is empty.</exception>
    public ControlledResourceRequest(ControlledResourceRoute route, string uri, ControlledResourceRoute? concreteRoute)
    {
        if (string.IsNullOrEmpty(uri))
            throw new ArgumentException("The requested URI must not be empty.", nameof(uri));
        Route = route;
        Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        ConcreteRoute = concreteRoute;
    }

    /// <summary>Gets the route (purpose) for which the resource is being acquired.</summary>
    public ControlledResourceRoute Route { get; }

    /// <summary>
    /// Gets the concrete operation behind <see cref="Route"/> when the request is governed by
    /// a phase/purpose route: for example <see cref="ControlledResourceRoute.Document"/> for a
    /// document read requested during static evaluation (route
    /// <see cref="ControlledResourceRoute.StaticEvaluation"/>). Null when <see cref="Route"/>
    /// is already the concrete operation. Additive (REQ-125 consumer review B-F2); the
    /// governing decision route remains <see cref="Route"/>.
    /// </summary>
    public ControlledResourceRoute? ConcreteRoute { get; }

    /// <summary>Gets the resolved (absolute) URI of the requested resource.</summary>
    public string Uri { get; }

    /// <summary>
    /// Gets <see cref="Uri"/> with any embedded credentials (the URI userinfo, for example
    /// <c>https://user:password@host/path</c>) stripped. Diagnostics and receipts always use
    /// the redacted form so credentials never reach a message surface.
    /// </summary>
    public string RedactedUri => ControlledResourcePolicy.RedactCredentials(Uri);

    /// <inheritdoc />
    public override string ToString()
        => ConcreteRoute is { } concrete ? $"{Route} ({concrete}): {RedactedUri}" : $"{Route}: {RedactedUri}";
}
