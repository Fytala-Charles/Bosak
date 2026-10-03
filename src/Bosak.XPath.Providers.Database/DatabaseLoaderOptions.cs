// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Per-scheme connection options for the database document loaders (basex/exist/marklogic)
// SPECIAL NOTES        : Part of the Bosak database document-loader package (REQ-120).
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 03-10-2026     | Slice 2: shared DatabaseConnectionOptions base + Exist/MarkLogic per-scheme types        |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Providers.Database;

/// <summary>
/// Options for the scheme-dispatching database document loaders
/// (<see cref="DatabaseDocumentLoader"/>).
/// </summary>
/// <remarks>
/// Options are organized per database scheme: <c>basex://</c> (BaseX REST),
/// <c>exist://</c> (eXist REST), and <c>marklogic://</c> (MarkLogic REST). The options
/// object is consulted on every load, so hosts may mutate it between loads.
/// URI-embedded credentials (userinfo such as <c>basex://user:pass@host/db</c>) are
/// deliberately NOT supported; supply credentials here instead.
/// </remarks>
public sealed class DatabaseLoaderOptions
{
    /// <summary>
    /// Gets the connection options for the <c>basex://</c> scheme (BaseX REST endpoint).
    /// </summary>
    public BaseXConnectionOptions BaseX { get; } = new();

    /// <summary>
    /// Gets the connection options for the <c>exist://</c> scheme (eXist REST endpoint).
    /// </summary>
    public ExistConnectionOptions Exist { get; } = new();

    /// <summary>
    /// Gets the connection options for the <c>marklogic://</c> scheme (MarkLogic REST endpoint).
    /// </summary>
    public MarkLogicConnectionOptions MarkLogic { get; } = new();
}

/// <summary>
/// Connection options shared by every database REST scheme. Concrete per-scheme types
/// (<see cref="BaseXConnectionOptions"/>, <see cref="ExistConnectionOptions"/>,
/// <see cref="MarkLogicConnectionOptions"/>) document how <see cref="EndpointBase"/> maps
/// onto their endpoint's path shape.
/// </summary>
public abstract class DatabaseConnectionOptions
{
    /// <summary>
    /// Gets or sets the REST endpoint base override. The exact shape is scheme-specific
    /// (documented on the derived type); when set, the authority of the database URI is
    /// ignored and the URI path is appended to this base. When null (the default), the
    /// scheme's default <c>http://host:port</c> mapping applies.
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

/// <summary>
/// Connection options for the BaseX REST scheme (<c>basex://host[:port]/db/resource</c>).
/// </summary>
/// <remarks>
/// Default mapping: <c>basex://host[:port]/db/x.xml</c> →
/// <c>http://host:port/rest/db/x.xml</c> (default port 8984). <see cref="DatabaseConnectionOptions.EndpointBase"/>
/// replaces the whole <c>http://host:port/rest</c> base, for example
/// <c>http://db.internal:8984/rest</c> or a reverse-proxied
/// <c>https://example.org/basex/rest</c>.
/// </remarks>
public sealed class BaseXConnectionOptions : DatabaseConnectionOptions
{
}

/// <summary>
/// Connection options for the eXist REST scheme (<c>exist://host[:port]/db/resource</c>).
/// </summary>
/// <remarks>
/// Default mapping: <c>exist://host[:port]/db/x.xml</c> →
/// <c>http://host:port/exist/rest/db/x.xml</c> (default port 8080).
/// <see cref="DatabaseConnectionOptions.EndpointBase"/> replaces the whole
/// <c>http://host:port/exist/rest</c> base, for example
/// <c>http://db.internal:8080/exist/rest</c> or a reverse-proxied
/// <c>https://example.org/exist/rest</c>.
/// </remarks>
public sealed class ExistConnectionOptions : DatabaseConnectionOptions
{
}

/// <summary>
/// Connection options for the MarkLogic REST scheme
/// (<c>marklogic://host[:port]/db/resource</c>).
/// </summary>
/// <remarks>
/// Default mapping: <c>marklogic://host[:port]/db/x.xml</c> →
/// <c>http://host:port/v1/documents?uri=%2Fdb%2Fx.xml</c> (default port 8000) — MarkLogic
/// takes the document URI as the <c>uri</c> query parameter rather than the request path,
/// and requests are sent with <c>Accept: application/xml</c> so the document is returned
/// as XML. <see cref="DatabaseConnectionOptions.EndpointBase"/> replaces the
/// <c>http://host:port</c> origin only (the <c>/v1/documents?uri=…</c> suffix is still
/// appended), for example <c>https://example.org</c> for a reverse-proxied cluster.
/// </remarks>
public sealed class MarkLogicConnectionOptions : DatabaseConnectionOptions
{
}
