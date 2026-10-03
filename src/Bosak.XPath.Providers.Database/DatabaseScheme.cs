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
//                      | Charles Korthout | 0.2   | 03-10-2026     | Slice 3: per-scheme collection-listing entry points (BaseX/eXist XML listings,           |
//                      |                  |       |                | MarkLogic search view=uris) behind the registry — wire quirks stay out of shared code    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Net.Http;
using System.Xml.Linq;

namespace Bosak.XPath.Providers.Database;

/// <summary>
/// One registered database REST scheme: the <c>scheme://</c> prefix, its default port, and
/// the endpoint-specific URI mapping. Per-database wire quirks (path prefix, query-parameter
/// document URI, request headers, collection-listing shape) live here, not in the shared
/// loader code — see the REQ-120 dossier risk note on REST fidelity.
/// </summary>
internal sealed class DatabaseScheme
{
    private DatabaseScheme(
        string scheme,
        string displayName,
        int defaultPort,
        Func<DatabaseLoaderOptions, DatabaseConnectionOptions> getConnectionOptions,
        Func<Uri, DatabaseConnectionOptions, Uri> buildRestUri,
        Action<HttpRequestMessage>? applyHeaders,
        Func<CollectionLister, Uri, IReadOnlyList<string>> listCollection)
    {
        Scheme = scheme;
        DisplayName = displayName;
        DefaultPort = defaultPort;
        GetConnectionOptions = getConnectionOptions;
        BuildRestUri = buildRestUri;
        ApplyHeaders = applyHeaders;
        ListCollection = listCollection;
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

    /// <summary>
    /// Lists the member document resource paths (database-absolute, e.g. <c>/db/coll/a.xml</c>)
    /// of the collection identified by the supplied parsed database URI. Owns the whole
    /// listing exchange, including follow-up requests for nested directories, so per-database
    /// listing wire shapes stay behind the registry entry.
    /// </summary>
    public Func<CollectionLister, Uri, IReadOnlyList<string>> ListCollection { get; }

    private static readonly DatabaseScheme BaseX = new(
        scheme: DatabaseDocumentLoader.BaseXScheme,
        displayName: "BaseX",
        defaultPort: 8984,
        getConnectionOptions: options => options.BaseX,
        buildRestUri: (parsed, connection) =>
            BuildPathRestUri(parsed, connection, defaultPort: 8984, pathPrefix: "/rest"),
        applyHeaders: null,
        listCollection: ListBaseXCollection);

    private static readonly DatabaseScheme Exist = new(
        scheme: DatabaseDocumentLoader.ExistScheme,
        displayName: "eXist",
        defaultPort: 8080,
        getConnectionOptions: options => options.Exist,
        buildRestUri: (parsed, connection) =>
            BuildPathRestUri(parsed, connection, defaultPort: 8080, pathPrefix: "/exist/rest"),
        applyHeaders: null,
        listCollection: ListExistCollection);

    // MarkLogic reads a single document via GET /v1/documents?uri=<document-uri> — the
    // document URI travels as the 'uri' query parameter, not the request path (MarkLogic
    // REST API, GET /v1/documents). Accept: application/xml pins the XML representation.
    // Collections list via the search API: GET /v1/search?directory=<dir>&view=uris&
    // depth=Infinity returns only <search:uri> entries (one per matching document), which
    // is the most economical listing shape the REST API offers.
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
        applyHeaders: request => request.Headers.Accept.Add(new("application/xml")),
        listCollection: ListMarkLogicCollection);

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
        => BuildPathRestUri(parsed.Host, EffectivePort(parsed, defaultPort), parsed.AbsolutePath, connection, pathPrefix);

    // Host/port/path form so collection listings can issue follow-up requests for nested
    // directories that were not part of the original collection URI.
    private static Uri BuildPathRestUri(string host, int port, string absolutePath, DatabaseConnectionOptions connection, string pathPrefix)
    {
        if (!string.IsNullOrEmpty(connection.EndpointBase))
        {
            return new Uri(connection.EndpointBase.TrimEnd('/') + absolutePath, UriKind.Absolute);
        }

        return new UriBuilder("http", host, port, pathPrefix + absolutePath).Uri;
    }

    private static int EffectivePort(Uri parsed, int defaultPort) => parsed.Port > 0 ? parsed.Port : defaultPort;

    // ------------------------------------------------------------------
    // Collection listings (one implementation per registered scheme)
    // ------------------------------------------------------------------

    /// <summary>
    /// BaseX collection listing: <c>GET /rest/{db/coll}</c> answers an XML listing whose
    /// <c>rest:resource</c> entries name the direct members and <c>rest:directory</c> entries
    /// either nest the subdirectory listing in the same response or are empty, in which case
    /// a follow-up <c>GET /rest/{db/coll/sub}</c> is issued. Returns database-absolute
    /// resource paths in listing order.
    /// </summary>
    private static IReadOnlyList<string> ListBaseXCollection(CollectionLister lister, Uri parsed)
    {
        var acc = new List<string>();
        CollectBaseX(lister, parsed, parsed.AbsolutePath, lister.ConnectionOptions, acc);
        return acc;
    }

    private static void CollectBaseX(CollectionLister lister, Uri parsed, string path, DatabaseConnectionOptions connection, List<string> acc)
    {
        XNamespace rest = "http://basex.org/rest";
        var listingUri = BuildPathRestUri(parsed.Host, EffectivePort(parsed, 8984), path, connection, "/rest");
        var listing = XDocument.Parse(lister.GetString(listingUri));
        Walk(listing.Root, path);

        void Walk(XElement? container, string containerPath)
        {
            if (container is null)
                return;

            foreach (var child in container.Elements())
            {
                if (child.Name == rest + "resource")
                {
                    if ((string?)child.Attribute("name") is { } name)
                        acc.Add(containerPath.TrimEnd('/') + "/" + name);
                }
                else if (child.Name == rest + "directory")
                {
                    if ((string?)child.Attribute("name") is not { } directoryName)
                        continue;
                    var directoryPath = containerPath.TrimEnd('/') + "/" + directoryName;
                    // BaseX nests subdirectory listings in the parent response; an empty
                    // directory entry requires its own request.
                    if (child.HasElements)
                        Walk(child, directoryPath);
                    else
                        CollectBaseX(lister, parsed, directoryPath, connection, acc);
                }
            }
        }
    }

    /// <summary>
    /// eXist collection listing: <c>GET /exist/rest/db/coll</c> answers an XML listing whose
    /// <c>resource</c> entries name the direct members and whose <c>subcollection</c> entries
    /// require a follow-up request per nested collection. Returns database-absolute resource
    /// paths in listing order.
    /// </summary>
    private static IReadOnlyList<string> ListExistCollection(CollectionLister lister, Uri parsed)
    {
        var acc = new List<string>();
        CollectExist(lister, parsed, parsed.AbsolutePath, lister.ConnectionOptions, acc);
        return acc;
    }

    private static void CollectExist(CollectionLister lister, Uri parsed, string path, DatabaseConnectionOptions connection, List<string> acc)
    {
        var listingUri = BuildPathRestUri(parsed.Host, EffectivePort(parsed, 8080), path, connection, "/exist/rest");
        var listing = XDocument.Parse(lister.GetString(listingUri));
        if (listing.Root is null)
            return;

        foreach (var child in listing.Root.Elements())
        {
            if ((string?)child.Attribute("name") is not { } name)
                continue;

            switch (child.Name.LocalName)
            {
                case "resource":
                    acc.Add(path.TrimEnd('/') + "/" + name);
                    break;
                case "subcollection":
                    CollectExist(lister, parsed, path.TrimEnd('/') + "/" + name, connection, acc);
                    break;
            }
        }
    }

    /// <summary>
    /// MarkLogic collection listing: <c>GET /v1/search?directory={dir}&amp;view=uris&amp;depth=Infinity</c>
    /// (sent with <c>Accept: application/xml</c>) returns only <c>&lt;search:uri&gt;</c> entries —
    /// one per matching document, already database-absolute, in search-result order (which
    /// MarkLogic does not guarantee to be stable, so member order should not be relied on
    /// for document-order semantics).
    /// </summary>
    private static IReadOnlyList<string> ListMarkLogicCollection(CollectionLister lister, Uri parsed)
    {
        const string searchNamespace = "http://marklogic.com/appservices/search";
        var connection = lister.ConnectionOptions;
        var directory = parsed.AbsolutePath;
        if (!directory.EndsWith('/'))
            directory += "/";

        const string searchPath = "/v1/search";
        var query = "directory=" + Uri.EscapeDataString(directory) + "&view=uris&depth=Infinity";
        Uri searchUri;
        if (!string.IsNullOrEmpty(connection.EndpointBase))
        {
            searchUri = new Uri(connection.EndpointBase.TrimEnd('/') + searchPath + "?" + query, UriKind.Absolute);
        }
        else
        {
            searchUri = new UriBuilder("http", parsed.Host, EffectivePort(parsed, 8000), searchPath)
            {
                Query = query,
            }.Uri;
        }

        var listing = XDocument.Parse(lister.GetString(searchUri));
        XNamespace search = searchNamespace;
        var uris = new List<string>();
        foreach (var uri in listing.Descendants(search + "uri"))
            uris.Add(uri.Value);
        return uris;
    }
}

/// <summary>
/// The exchange helpers handed to a scheme's collection-listing delegate: the resolved
/// connection (scheme, credentials, connection options) plus an authorized GET that maps
/// endpoint/timeout failures to <see cref="IOException"/> exactly like document loads.
/// </summary>
internal sealed class CollectionLister
{
    private readonly DatabaseDocumentLoader.Connection _connection;

    /// <summary>
    /// Initializes a new lister over the resolved connection of the collection request.
    /// </summary>
    /// <param name="connection">The resolved connection (scheme, REST URI of the collection itself, credentials).</param>
    internal CollectionLister(DatabaseDocumentLoader.Connection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// The per-scheme connection options (endpoint base, credentials) of the collection request.
    /// </summary>
    internal DatabaseConnectionOptions ConnectionOptions => _connection.ConnectionOptions;

    /// <summary>
    /// Performs an authorized <c>GET</c> against the supplied listing URI and returns the
    /// response body text.
    /// </summary>
    /// <param name="restUri">The listing endpoint URI.</param>
    /// <returns>The response body.</returns>
    /// <exception cref="IOException">The endpoint is unreachable, times out, or returns a non-success HTTP status.</exception>
    internal string GetString(Uri restUri)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, restUri);
        if (_connection.AuthHeader is not null)
            request.Headers.Authorization = _connection.AuthHeader;
        _connection.Scheme.ApplyHeaders?.Invoke(request);
        using var response = DatabaseDocumentLoader.Send(request, HttpCompletionOption.ResponseContentRead, _connection);
        return response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
