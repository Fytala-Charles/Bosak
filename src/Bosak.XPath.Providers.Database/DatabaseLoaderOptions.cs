// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Per-scheme connection options for the database document-loader spike
// SPECIAL NOTES        : Part of the Bosak database document-loader spike (REQ-120 Slice 1).
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Providers.Database;

/// <summary>
/// Options for the scheme-dispatching database document loaders
/// (<see cref="DatabaseDocumentLoader"/>).
/// </summary>
/// <remarks>
/// Options are organized per database scheme; Slice 1 ships the <c>basex://</c> scheme only.
/// The options object is consulted on every load, so hosts may mutate it between loads.
/// URI-embedded credentials (userinfo such as <c>basex://user:pass@host/db</c>) are deliberately
/// NOT supported in the spike; supply credentials here instead.
/// </remarks>
public sealed class DatabaseLoaderOptions
{
    /// <summary>
    /// Gets the connection options for the <c>basex://</c> scheme (BaseX REST endpoint).
    /// </summary>
    public BaseXConnectionOptions BaseX { get; } = new();
}

/// <summary>
/// Connection options for the BaseX REST scheme (<c>basex://host[:port]/db/resource</c>).
/// </summary>
public sealed class BaseXConnectionOptions
{
    /// <summary>
    /// Gets or sets the REST endpoint base that replaces the default
    /// <c>http://host:port/rest</c> mapping, for example <c>http://db.internal:8984/rest</c>
    /// or a reverse-proxied <c>https://example.org/basex/rest</c>. When set, the authority of
    /// the <c>basex://</c> URI is ignored and the URI path is appended to this base.
    /// When null (the default), <c>basex://host[:port]/db/x.xml</c> maps to
    /// <c>http://host:port/rest/db/x.xml</c> (default port 8984).
    /// </summary>
    public string? EndpointBase { get; set; }

    /// <summary>
    /// Gets or sets the user name for HTTP Basic authentication against the REST endpoint.
    /// When set (with <see cref="Password"/>), an <c>Authorization: Basic</c> header is sent;
    /// when null, no Authorization header is sent.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for HTTP Basic authentication against the REST endpoint.
    /// Used only when <see cref="Username"/> is set.
    /// </summary>
    public string? Password { get; set; }
}
