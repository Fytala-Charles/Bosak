// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Internal scheme registry mapping basex/exist/marklogic URIs onto their REST endpoint shapes
// SPECIAL NOTES        : Part of the Bosak database document-loader package (REQ-120).
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-10-2026     | Creation (REQ-120 Slice 2 scheme registry)                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Net.Http;

namespace Bosak.XPath.Providers.Database;

/// <summary>
/// One registered database REST scheme: the <c>scheme://</c> prefix, its default port, and
/// the endpoint-specific URI mapping. Per-database wire quirks (path prefix, query-parameter
/// document URI, request headers) live here, not in the shared loader code — see the REQ-120
/// dossier risk note on REST fidelity.
/// </summary>
internal sealed class DatabaseScheme
{
    private DatabaseScheme(
        string scheme,
        string displayName,
        int defaultPort,
        Func<DatabaseLoaderOptions, DatabaseConnectionOptions> getConnectionOptions,
        Func<Uri, DatabaseConnectionOptions, Uri> buildRestUri,
        Action<HttpRequestMessage>? applyHeaders)
    {
        Scheme = scheme;
        DisplayName = displayName;
        DefaultPort = defaultPort;
        GetConnectionOptions = getConnectionOptions;
        BuildRestUri = buildRestUri;
        ApplyHeaders = applyHeaders;
    }

    /// <summary>
    /// The URI scheme prefix without the separator (e.g. <c>basex</c>).
    /// </summary>
    public string Scheme { get; }

    /// <summary>
    /// Human-readable database name used in error messages (e.g. <c>BaseX</c>).
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// The default HTTP port when the URI carries none.
    /// </summary>
    public int DefaultPort { get; }

    /// <summary>
    /// Selects this scheme's connection options from the loader options bag.
    /// </summary>
    public Func<DatabaseLoaderOptions, DatabaseConnectionOptions> GetConnectionOptions { get; }

    /// <summary>
    /// Maps a parsed database URI (custom scheme, authority already validated) to the REST
    /// endpoint URI, honoring <see cref="DatabaseConnectionOptions.EndpointBase"/> when set.
    /// </summary>
    public Func<Uri, DatabaseConnectionOptions, Uri> BuildRestUri { get; }

    /// <summary>
    /// Optional scheme-specific request headers (null when the scheme needs none).
    /// </summary>
    public Action<HttpRequestMessage>? ApplyHeaders { get; }

    private static readonly DatabaseScheme BaseX = new(
        scheme: DatabaseDocumentLoader.BaseXScheme,
        displayName: "BaseX",
        defaultPort: 8984,
        getConnectionOptions: options => options.BaseX,
        buildRestUri: (parsed, connection) =>
            BuildPathRestUri(parsed, connection, defaultPort: 8984, pathPrefix: "/rest"),
        applyHeaders: null);

    private static readonly DatabaseScheme Exist = new(
        scheme: DatabaseDocumentLoader.ExistScheme,
        displayName: "eXist",
        defaultPort: 8080,
        getConnectionOptions: options => options.Exist,
        buildRestUri: (parsed, connection) =>
            BuildPathRestUri(parsed, connection, defaultPort: 8080, pathPrefix: "/exist/rest"),
        applyHeaders: null);

    // MarkLogic reads a single document via GET /v1/documents?uri=<document-uri> — the
    // document URI travels as the 'uri' query parameter, not the request path (MarkLogic
    // REST API, GET /v1/documents). Accept: application/xml pins the XML representation.
    private static readonly DatabaseScheme MarkLogic = new(
        scheme: DatabaseDocumentLoader.MarkLogicScheme,
        displayName: "MarkLogic",
        defaultPort: 8000,
        getConnectionOptions: options => options.MarkLogic,
        buildRestUri: (parsed, connection) =>
        {
            const string documentsPath = "/v1/documents";
            var escapedUri = Uri.EscapeDataString(parsed.AbsolutePath);
            if (!string.IsNullOrEmpty(connection.EndpointBase))
            {
                return new Uri(
                    connection.EndpointBase.TrimEnd('/') + documentsPath + "?uri=" + escapedUri,
                    UriKind.Absolute);
            }

            return new UriBuilder("http", parsed.Host, EffectivePort(parsed, 8000), documentsPath)
            {
                Query = "uri=" + escapedUri,
            }.Uri;
        },
        applyHeaders: request => request.Headers.Accept.Add(new("application/xml")));

    /// <summary>
    /// The registered schemes, keyed by lowercase scheme name.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, DatabaseScheme> Registry =
        new Dictionary<string, DatabaseScheme>(StringComparer.OrdinalIgnoreCase)
        {
            [BaseX.Scheme] = BaseX,
            [Exist.Scheme] = Exist,
            [MarkLogic.Scheme] = MarkLogic,
        };

    /// <summary>
    /// Extracts the scheme name (lowercase) from an absolute URI, or null when the URI has
    /// no <c>://</c> separator.
    /// </summary>
    public static string? SchemeOf(string uri)
    {
        var separator = uri.IndexOf("://", StringComparison.Ordinal);
        return separator > 0 ? uri[..separator] : null;
    }

    private static Uri BuildPathRestUri(Uri parsed, DatabaseConnectionOptions connection, int defaultPort, string pathPrefix)
    {
        if (!string.IsNullOrEmpty(connection.EndpointBase))
        {
            return new Uri(connection.EndpointBase.TrimEnd('/') + parsed.AbsolutePath, UriKind.Absolute);
        }

        return new UriBuilder("http", parsed.Host, EffectivePort(parsed, defaultPort), pathPrefix + parsed.AbsolutePath).Uri;
    }

    private static int EffectivePort(Uri parsed, int defaultPort) => parsed.Port > 0 ? parsed.Port : defaultPort;
}
