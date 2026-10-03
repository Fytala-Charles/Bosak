// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Scheme-dispatching document loader that fetches basex/exist/marklogic URIs over their REST APIs
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
//                      | Charles Korthout | 0.2   | 03-10-2026     | Slice 2: basex/exist/marklogic scheme registry, streaming response teardown              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 03-10-2026     | Slice 3: collection listing (LoadCollection / DispatchCollection) — per-scheme listing   |
//                      |                  |       |                | shapes behind the scheme registry; members returned as scheme:// URIs in listing order   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Providers.Xml;

namespace Bosak.XPath.Providers.Database;

/// <summary>
/// Scheme-dispatching document loader for XML database REST endpoints. Intercepts
/// <c>basex://</c>, <c>exist://</c>, and <c>marklogic://</c> URIs, fetches the document
/// over the database's REST API, and adapts it to <see cref="IXdmNode"/> via the public
/// <see cref="XDocumentProvider"/> path (in-memory) or <see cref="XmlStreamingProvider"/>
/// (forward-only streaming). All other URIs are delegated to a fallback loader, normally
/// <see cref="XDocumentProvider.LoadFile(string)"/>.
/// </summary>
/// <remarks>
/// <para>
/// Install the dispatch on the frozen <c>EvaluationContext</c> hooks so every consumer
/// (fn:doc, fn:document, xsl:source-document in both modes, xsl:merge-source, fn:transform)
/// resolves database URIs without engine changes:
/// </para>
/// <code>
/// var options = new DatabaseLoaderOptions();
/// options.BaseX.Username = "admin";
/// options.BaseX.Password = "admin";
/// ctx.DocumentLoader = DatabaseDocumentLoader.Dispatch(XDocumentProvider.LoadFile, options);
/// ctx.StreamingDocumentLoader = DatabaseDocumentLoader.DispatchStreaming(XDocumentProvider.LoadFile, options);
/// </code>
/// <para>
/// URI mapping (per-scheme connection options live on <see cref="DatabaseLoaderOptions"/>):
/// </para>
/// <list type="bullet">
/// <item><c>basex://host[:port]/db/resource</c> → <c>http://host[:port]/rest/db/resource</c> (default port 8984).</item>
/// <item><c>exist://host[:port]/db/resource</c> → <c>http://host[:port]/exist/rest/db/resource</c> (default port 8080).</item>
/// <item><c>marklogic://host[:port]/db/resource</c> → <c>http://host[:port]/v1/documents?uri=%2Fdb%2Fresource</c> (default port 8000) with <c>Accept: application/xml</c> — MarkLogic takes the document URI as the <c>uri</c> query parameter, not the request path.</item>
/// </list>
/// <para>
/// <see cref="DatabaseConnectionOptions.EndpointBase"/> overrides the default origin/path
/// prefix per scheme (documented on each per-scheme options type). URI-embedded credentials
/// are not supported; pass credentials via the per-scheme options.
/// </para>
/// <para>
/// Error contract (matches how <c>EvaluationContext.LoadDocument</c> maps loader failures):
/// unreachable endpoints, HTTP error statuses, and timeouts surface as
/// <see cref="IOException"/>; malformed XML payloads surface as <see cref="XmlException"/>;
/// both map to FODC0002 when the load runs through <c>EvaluationContext.LoadDocument</c>.
/// Unsupported URI shapes (an unregistered scheme passed to <see cref="Load(string)"/>
/// directly, or URI-embedded userinfo) surface as <see cref="ArgumentException"/>/<see cref="UriFormatException"/>
/// (FODC0005 class).
/// </para>
/// <para>
/// HTTP is performed synchronously over an async <see cref="HttpClient"/> with
/// <c>ConfigureAwait(false)</c> — the established sync idiom of the engine
/// (see fn:unparsed-text). The streaming variant reads the response with
/// <c>ResponseHeadersRead</c> and hands the live network stream (wrapped in a
/// <c>ResponseBoundStream</c> that disposes the <see cref="HttpResponseMessage"/> together
/// with the stream) to <see cref="XmlStreamingProvider.Load(Stream, StreamingLoadOptions?)"/>,
/// so records are materialized one at a time in bounded memory straight off the response
/// stream. The wrapper releases the response and connection deterministically when the
/// streaming source disposes the stream (end-of-stream and failure paths, via
/// <c>CloseInput = true</c>); abandoning a partially-read streamed document without
/// consuming it to the end still relies on finalization.
/// </para>
/// </remarks>
public static class DatabaseDocumentLoader
{
    /// <summary>
    /// The URI scheme handled by this loader for the BaseX REST endpoint.
    /// </summary>
    public const string BaseXScheme = "basex";

    /// <summary>
    /// The URI scheme handled by this loader for the eXist REST endpoint.
    /// </summary>
    public const string ExistScheme = "exist";

    /// <summary>
    /// The URI scheme handled by this loader for the MarkLogic REST endpoint.
    /// </summary>
    public const string MarkLogicScheme = "marklogic";

    private static readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Determines whether the supplied URI is handled by this loader (a registered
    /// database scheme: <c>basex://</c>, <c>exist://</c>, or <c>marklogic://</c>).
    /// </summary>
    /// <param name="uri">The absolute URI to inspect.</param>
    /// <returns><c>true</c> when <paramref name="uri"/> uses a registered database scheme.</returns>
    public static bool Handles(string uri)
        => uri is not null
           && DatabaseScheme.SchemeOf(uri) is { } scheme
           && DatabaseScheme.Registry.ContainsKey(scheme);

    /// <summary>
    /// Creates a <c>DocumentLoader</c> delegate that fetches registered database-scheme
    /// URIs over their REST APIs (fully materialized, XDocument-backed) and delegates every
    /// other URI to <paramref name="fallback"/>.
    /// </summary>
    /// <param name="fallback">The loader for non-database URIs, normally <see cref="XDocumentProvider.LoadFile(string)"/>.</param>
    /// <param name="options">Optional per-scheme connection options (endpoint base, credentials).</param>
    /// <returns>The scheme-dispatching loader delegate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is null.</exception>
    public static Func<string, IXdmNode> Dispatch(Func<string, IXdmNode> fallback, DatabaseLoaderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return uri => Handles(uri) ? Load(uri, options) : fallback(uri);
    }

    /// <summary>
    /// Creates a <c>StreamingDocumentLoader</c> delegate that fetches registered
    /// database-scheme URIs over their REST APIs as forward-only streaming documents
    /// (bounded memory, record-at-a-time off the response stream) and delegates every other
    /// URI to <paramref name="fallback"/>.
    /// </summary>
    /// <param name="fallback">The loader for non-database URIs, normally <see cref="XDocumentProvider.LoadFile(string)"/>.</param>
    /// <param name="options">Optional per-scheme connection options (endpoint base, credentials).</param>
    /// <returns>The scheme-dispatching streaming loader delegate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is null.</exception>
    public static Func<string, IXdmNode> DispatchStreaming(Func<string, IXdmNode> fallback, DatabaseLoaderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return uri => Handles(uri) ? LoadStreaming(uri, options) : fallback(uri);
    }

    /// <summary>
    /// Fetches a database-scheme URI over the database's REST API and returns the response
    /// body as a fully materialized (XDocument-backed) document node. The original
    /// database URI is recorded as the document URI.
    /// </summary>
    /// <param name="uri">The database document URI (<c>basex://</c>, <c>exist://</c>, or <c>marklogic://</c>).</param>
    /// <param name="options">Optional per-scheme connection options (endpoint base, credentials).</param>
    /// <returns>The document node for the fetched document.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null or empty.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> does not use a registered database scheme.</exception>
    /// <exception cref="UriFormatException">The URI carries embedded credentials (userinfo), which are not supported.</exception>
    /// <exception cref="IOException">The endpoint is unreachable, times out, or returns a non-success HTTP status.</exception>
    /// <exception cref="XmlException">The response body is not well-formed XML.</exception>
    public static IXdmNode Load(string uri, DatabaseLoaderOptions? options = null)
    {
        var connection = ResolveConnection(uri, options);
        using var request = CreateRequest(connection);
        using var response = Send(request, HttpCompletionOption.ResponseContentRead, connection);
        var xml = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        var node = XDocumentProvider.ParseXml(xml);
        if (node is XDocumentNode xdn)
            xdn.SetDocumentUri(uri);
        return node;
    }

    /// <summary>
    /// Fetches a database-scheme URI over the database's REST API and returns the response
    /// body as a forward-only streaming document node: records are materialized one at a
    /// time in bounded memory straight off the response stream. The original database URI is
    /// recorded as the base and document URI. The HTTP response is disposed deterministically
    /// when the stream is consumed to the end or the load fails; see the type remarks.
    /// </summary>
    /// <param name="uri">The database document URI (<c>basex://</c>, <c>exist://</c>, or <c>marklogic://</c>).</param>
    /// <param name="options">Optional per-scheme connection options (endpoint base, credentials).</param>
    /// <returns>The streaming document node for the fetched document.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null or empty.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> does not use a registered database scheme.</exception>
    /// <exception cref="UriFormatException">The URI carries embedded credentials (userinfo), which are not supported.</exception>
    /// <exception cref="IOException">The endpoint is unreachable, times out, or returns a non-success HTTP status.</exception>
    /// <exception cref="XmlException">The response body is not well-formed XML.</exception>
    public static IXdmNode LoadStreaming(string uri, DatabaseLoaderOptions? options = null)
    {
        var connection = ResolveConnection(uri, options);
        using var request = CreateRequest(connection);
        var response = Send(request, HttpCompletionOption.ResponseHeadersRead, connection);
        Stream stream;
        try
        {
            var contentStream = response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            // Bind the response to its stream: every disposal path below (streaming-source
            // end-of-stream via CloseInput, failure, and the catch here) then releases the
            // response and its connection deterministically.
            stream = new ResponseBoundStream(contentStream, response);
        }
        catch
        {
            response.Dispose();
            throw;
        }

        try
        {
            // CloseInput = true so the streaming source's end-of-stream reader disposal
            // releases the network stream (and, via the ResponseBoundStream wrapper, the
            // HTTP response). The remaining reader defaults replicate XmlStreamingProvider's
            // in-memory load path (DTDs parsed via XmlUrlResolver, whitespace/comments/PIs
            // preserved).
            var readerSettings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse,
                XmlResolver = new XmlUrlResolver(),
                IgnoreWhitespace = false,
                IgnoreComments = false,
                IgnoreProcessingInstructions = false,
                CloseInput = true,
            };
            return XmlStreamingProvider.Load(stream, new StreamingLoadOptions
            {
                BaseUri = uri,
                DocumentUri = uri,
                ReaderSettings = readerSettings,
            });
        }
        catch
        {
            // The streaming source takes over stream ownership only once Load returns.
            stream.Dispose();
            throw;
        }
    }

    internal sealed record Connection(
        DatabaseScheme Scheme,
        Uri RestUri,
        DatabaseConnectionOptions ConnectionOptions,
        AuthenticationHeaderValue? AuthHeader);

    /// <summary>
    /// Lists the member documents of a database collection over the database's REST API
    /// and returns them as database-scheme URIs (e.g. <c>basex://host/db/coll/a.xml</c>)
    /// in the listing order reported by the database. The returned URIs are ordinary
    /// database document URIs: they resolve through <see cref="Load(string, DatabaseLoaderOptions?)"/>
    /// / <see cref="Dispatch"/>, so document identity caching and the FODC0002/FODC0005
    /// error contract are unchanged.
    /// </summary>
    /// <param name="uri">The database collection URI (<c>basex://</c>, <c>exist://</c>, or <c>marklogic://</c>).</param>
    /// <param name="options">Optional per-scheme connection options (endpoint base, credentials).</param>
    /// <returns>The member document URIs in listing order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null or empty.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> does not use a registered database scheme.</exception>
    /// <exception cref="UriFormatException">The URI carries embedded credentials (userinfo), which are not supported.</exception>
    /// <exception cref="IOException">The endpoint is unreachable, times out, or returns a non-success HTTP status.</exception>
    /// <exception cref="XmlException">A listing response body is not well-formed XML.</exception>
    /// <remarks>
    /// <para>
    /// Per-scheme listing shapes (kept behind the scheme registry, not in shared code):
    /// </para>
    /// <list type="bullet">
    /// <item>BaseX: <c>GET /rest/{db/coll}</c> returns an XML listing of <c>rest:resource</c>
    /// members; nested <c>rest:directory</c> listings are read from the same response when
    /// nested, or via a follow-up request when empty.</item>
    /// <item>eXist: <c>GET /exist/rest/db/coll</c> returns an XML listing of <c>resource</c>
    /// members; each <c>subcollection</c> requires a follow-up request.</item>
    /// <item>MarkLogic: <c>GET /v1/search?directory={dir}&amp;view=uris&amp;depth=Infinity</c>
    /// returns only <c>&lt;search:uri&gt;</c> entries — one per matching document. The
    /// search API does not guarantee a stable member order, so MarkLogic collection order
    /// should not be relied on for document-order semantics.</item>
    /// </list>
    /// <para>
    /// Ordering contract: members are returned in the database's listing order. When the
    /// list feeds the engine's <c>EvaluationContext.CollectionLoader</c> hook, the engine
    /// loads members in that order and cross-tree document order follows the load order
    /// (creation-sequence model), so BaseX and eXist collection order is a meaningful
    /// document-order story.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<string> LoadCollection(string uri, DatabaseLoaderOptions? options = null)
    {
        var connection = ResolveConnection(uri, options);
        var parsed = new Uri(uri, UriKind.Absolute);
        var members = connection.Scheme.ListCollection(new CollectionLister(connection), parsed);
        var result = new List<string>(members.Count);
        foreach (var member in members)
            result.Add(RebuildSchemeUri(connection.Scheme, parsed, member));
        return result;
    }

    /// <summary>
    /// Creates a <c>CollectionLoader</c>-shaped delegate for the engine's
    /// <c>EvaluationContext.CollectionLoader</c> hook (REQ-120 Slice 3): registered
    /// database-scheme collection URIs are listed over their REST APIs and returned as
    /// member document URIs; every other URI is delegated to <paramref name="fallback"/>
    /// (return <c>null</c> there to decline, which the engine maps to FODC0002).
    /// </summary>
    /// <param name="fallback">The collection resolver for non-database URIs; return <c>null</c> to decline a URI.</param>
    /// <param name="options">Optional per-scheme connection options (endpoint base, credentials).</param>
    /// <returns>The scheme-dispatching collection-listing delegate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is null.</exception>
    /// <example>
    /// <code>
    /// var options = new DatabaseLoaderOptions();
    /// options.BaseX.Username = "admin";
    /// options.BaseX.Password = "admin";
    /// ctx.DocumentLoader = DatabaseDocumentLoader.Dispatch(XDocumentProvider.LoadFile, options);
    /// ctx.CollectionLoader = DatabaseDocumentLoader.DispatchCollection(_ => null, options);
    /// // fn:collection("basex://localhost/mydb/mycoll") now lists the collection server-side
    /// // and returns its member documents in listing order.
    /// </code>
    /// </example>
    public static Func<string, IReadOnlyList<string>?> DispatchCollection(Func<string, IReadOnlyList<string>?> fallback, DatabaseLoaderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return uri => Handles(uri) ? LoadCollection(uri, options) : fallback(uri);
    }

    // Turns a database-absolute resource path from a listing (e.g. "/db/coll/a.xml") back
    // into a scheme URI on the collection's own authority, so members funnel through the
    // document-load path (identity cache, error mapping) with the effective default port.
    // Entries that are already absolute URIs pass through unchanged.
    private static string RebuildSchemeUri(DatabaseScheme scheme, Uri parsed, string member)
    {
        if (Uri.IsWellFormedUriString(member, UriKind.Absolute))
            return member;
        var port = parsed.Port > 0 ? parsed.Port : scheme.DefaultPort;
        return new UriBuilder(scheme.Scheme, parsed.Host, port, member).Uri.AbsoluteUri;
    }

    private static Connection ResolveConnection(string uri, DatabaseLoaderOptions? options)
    {
        if (string.IsNullOrEmpty(uri))
            throw new ArgumentNullException(nameof(uri));

        var schemeName = DatabaseScheme.SchemeOf(uri);
        if (schemeName is null || !DatabaseScheme.Registry.TryGetValue(schemeName, out var scheme))
        {
            throw new ArgumentException(
                $"The database document loader only handles '{BaseXScheme}://', '{ExistScheme}://', or '{MarkLogicScheme}://' URIs: {uri}",
                nameof(uri));
        }

        // Custom schemes parse as hierarchical URIs; userinfo is rejected because
        // credentials are supported via options only (surfacing here as FODC0005 class).
        var parsed = new Uri(uri, UriKind.Absolute);
        if (!string.IsNullOrEmpty(parsed.UserInfo))
            throw new UriFormatException($"URI-embedded credentials are not supported for '{scheme.Scheme}://' URIs; use {nameof(DatabaseLoaderOptions)} instead.");

        var connection = scheme.GetConnectionOptions(options ?? new DatabaseLoaderOptions());
        var restUri = scheme.BuildRestUri(parsed, connection);

        AuthenticationHeaderValue? auth = null;
        if (!string.IsNullOrEmpty(connection.Username))
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{connection.Username}:{connection.Password ?? string.Empty}"));
            auth = new AuthenticationHeaderValue("Basic", token);
        }

        return new Connection(scheme, restUri, connection, auth);
    }

    private static HttpRequestMessage CreateRequest(Connection connection)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, connection.RestUri);
        if (connection.AuthHeader is not null)
            request.Headers.Authorization = connection.AuthHeader;
        connection.Scheme.ApplyHeaders?.Invoke(request);
        return request;
    }

    // Sync-over-async with ConfigureAwait(false) — the established engine idiom (fn:unparsed-text).
    // HttpRequestException (DNS/connect/reset) and TaskCanceledException (timeout/cancellation)
    // are normalized to IOException so EvaluationContext.LoadDocument maps them to FODC0002.
    internal static HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completion, Connection connection)
    {
        HttpResponseMessage response;
        try
        {
            response = _httpClient.SendAsync(request, completion, CancellationToken.None)
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (HttpRequestException e)
        {
            throw new IOException($"Cannot reach the {connection.Scheme.DisplayName} REST endpoint '{connection.RestUri}': {e.Message}", e);
        }
        catch (TaskCanceledException e)
        {
            throw new IOException($"Timed out reaching the {connection.Scheme.DisplayName} REST endpoint '{connection.RestUri}'.", e);
        }

        if (!response.IsSuccessStatusCode)
        {
            var status = (int)response.StatusCode;
            var reason = response.ReasonPhrase ?? "error";
            response.Dispose();
            throw new IOException($"The {connection.Scheme.DisplayName} REST endpoint '{connection.RestUri}' returned HTTP {status} ({reason}).");
        }

        return response;
    }
}
