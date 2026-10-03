// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Scheme-dispatching document loader that fetches basex:// URIs over the BaseX REST API
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
/// <c>basex://</c> URIs, fetches the document over the BaseX REST API (GET on the REST
/// endpoint, response body = the XML document), and adapts it to <see cref="IXdmNode"/> via
/// the public <see cref="XDocumentProvider"/> path (in-memory) or
/// <see cref="XmlStreamingProvider"/> (forward-only streaming). All other URIs are delegated
/// to a fallback loader, normally <see cref="XDocumentProvider.LoadFile(string)"/>.
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
/// URI mapping: <c>basex://host[:port]/db/resource/path.xml</c> →
/// <c>http://host[:port]/rest/db/resource/path.xml</c> (default port 8984);
/// <see cref="BaseXConnectionOptions.EndpointBase"/> overrides the
/// <c>http://host:port/rest</c> base entirely. URI-embedded credentials are not supported;
/// pass credentials via <see cref="DatabaseLoaderOptions.BaseX"/>.
/// </para>
/// <para>
/// Error contract (matches how <c>EvaluationContext.LoadDocument</c> maps loader failures):
/// unreachable endpoints, HTTP error statuses, and timeouts surface as
/// <see cref="IOException"/>; malformed XML payloads surface as <see cref="XmlException"/>;
/// both map to FODC0002 when the load runs through <c>EvaluationContext.LoadDocument</c>.
/// Unsupported URI shapes (non-basex scheme passed to <see cref="Load(string)"/> directly,
/// or URI-embedded userinfo) surface as <see cref="ArgumentException"/>/<see cref="UriFormatException"/>
/// (FODC0005 class).
/// </para>
/// <para>
/// HTTP is performed synchronously over an async <see cref="HttpClient"/> with
/// <c>ConfigureAwait(false)</c> — the established sync idiom of the engine
/// (see fn:unparsed-text). The streaming variant reads the response with
/// <c>ResponseHeadersRead</c> and hands the live network stream to
/// <see cref="XmlStreamingProvider.Load(Stream, StreamingLoadOptions?)"/>, so records are
/// materialized one at a time in bounded memory straight off the response stream. The stream
/// is closed when the streaming source finishes or fails; abandoning a partially-read
/// streamed document relies on finalization to release the connection.
/// </para>
/// </remarks>
public static class DatabaseDocumentLoader
{
    /// <summary>
    /// The URI scheme handled by this loader.
    /// </summary>
    public const string BaseXScheme = "basex";

    private static readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Determines whether the supplied URI is handled by this loader (the
    /// <c>basex://</c> scheme).
    /// </summary>
    /// <param name="uri">The absolute URI to inspect.</param>
    /// <returns><c>true</c> when <paramref name="uri"/> uses the <c>basex://</c> scheme.</returns>
    public static bool Handles(string uri)
        => uri.StartsWith(BaseXScheme + "://", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a <c>DocumentLoader</c> delegate that fetches <c>basex://</c> URIs over the
    /// BaseX REST API (fully materialized, XDocument-backed) and delegates every other URI to
    /// <paramref name="fallback"/>.
    /// </summary>
    /// <param name="fallback">The loader for non-basex URIs, normally <see cref="XDocumentProvider.LoadFile(string)"/>.</param>
    /// <param name="options">Optional per-scheme connection options (endpoint base, credentials).</param>
    /// <returns>The scheme-dispatching loader delegate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is null.</exception>
    public static Func<string, IXdmNode> Dispatch(Func<string, IXdmNode> fallback, DatabaseLoaderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return uri => Handles(uri) ? Load(uri, options) : fallback(uri);
    }

    /// <summary>
    /// Creates a <c>StreamingDocumentLoader</c> delegate that fetches <c>basex://</c> URIs
    /// over the BaseX REST API as forward-only streaming documents (bounded memory,
    /// record-at-a-time off the response stream) and delegates every other URI to
    /// <paramref name="fallback"/>.
    /// </summary>
    /// <param name="fallback">The loader for non-basex URIs, normally <see cref="XDocumentProvider.LoadFile(string)"/>.</param>
    /// <param name="options">Optional per-scheme connection options (endpoint base, credentials).</param>
    /// <returns>The scheme-dispatching streaming loader delegate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is null.</exception>
    public static Func<string, IXdmNode> DispatchStreaming(Func<string, IXdmNode> fallback, DatabaseLoaderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return uri => Handles(uri) ? LoadStreaming(uri, options) : fallback(uri);
    }

    /// <summary>
    /// Fetches a <c>basex://</c> URI over the BaseX REST API and returns the response body
    /// as a fully materialized (XDocument-backed) document node. The original
    /// <c>basex://</c> URI is recorded as the document URI.
    /// </summary>
    /// <param name="uri">The <c>basex://</c> document URI.</param>
    /// <param name="options">Optional per-scheme connection options (endpoint base, credentials).</param>
    /// <returns>The document node for the fetched document.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null or empty.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is not a basex URI.</exception>
    /// <exception cref="UriFormatException">The URI carries embedded credentials (userinfo), which the spike does not support.</exception>
    /// <exception cref="IOException">The endpoint is unreachable, times out, or returns a non-success HTTP status.</exception>
    /// <exception cref="XmlException">The response body is not well-formed XML.</exception>
    public static IXdmNode Load(string uri, DatabaseLoaderOptions? options = null)
    {
        var connection = ResolveConnection(uri, options);
        using var request = CreateRequest(connection.RestUri, connection.AuthHeader);
        using var response = Send(request, HttpCompletionOption.ResponseContentRead, connection.RestUri);
        var xml = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        var node = XDocumentProvider.ParseXml(xml);
        if (node is XDocumentNode xdn)
            xdn.SetDocumentUri(uri);
        return node;
    }

    /// <summary>
    /// Fetches a <c>basex://</c> URI over the BaseX REST API and returns the response body
    /// as a forward-only streaming document node: records are materialized one at a time in
    /// bounded memory straight off the response stream. The original <c>basex://</c> URI is
    /// recorded as the base and document URI.
    /// </summary>
    /// <param name="uri">The <c>basex://</c> document URI.</param>
    /// <param name="options">Optional per-scheme connection options (endpoint base, credentials).</param>
    /// <returns>The streaming document node for the fetched document.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null or empty.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is not a basex URI.</exception>
    /// <exception cref="UriFormatException">The URI carries embedded credentials (userinfo), which the spike does not support.</exception>
    /// <exception cref="IOException">The endpoint is unreachable, times out, or returns a non-success HTTP status.</exception>
    /// <exception cref="XmlException">The response body is not well-formed XML.</exception>
    public static IXdmNode LoadStreaming(string uri, DatabaseLoaderOptions? options = null)
    {
        var connection = ResolveConnection(uri, options);
        using var request = CreateRequest(connection.RestUri, connection.AuthHeader);
        var response = Send(request, HttpCompletionOption.ResponseHeadersRead, connection.RestUri);
        Stream stream;
        try
        {
            stream = response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch
        {
            response.Dispose();
            throw;
        }

        try
        {
            // CloseInput = true so the streaming source's end-of-stream reader disposal
            // releases the network stream (and with it the HTTP connection). The remaining
            // reader defaults replicate XmlStreamingProvider's in-memory load path (DTDs
            // parsed via XmlUrlResolver, whitespace/comments/PIs preserved).
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
            response.Dispose();
            throw;
        }
    }

    private sealed record Connection(Uri RestUri, AuthenticationHeaderValue? AuthHeader);

    private static Connection ResolveConnection(string uri, DatabaseLoaderOptions? options)
    {
        if (string.IsNullOrEmpty(uri))
            throw new ArgumentNullException(nameof(uri));
        if (!Handles(uri))
            throw new ArgumentException($"The database document loader only handles '{BaseXScheme}://' URIs: {uri}", nameof(uri));

        // Custom schemes parse as hierarchical URIs; userinfo is rejected because the spike
        // supports credentials via options only (surfacing here as FODC0005 class).
        var parsed = new Uri(uri, UriKind.Absolute);
        if (!string.IsNullOrEmpty(parsed.UserInfo))
            throw new UriFormatException($"URI-embedded credentials are not supported for '{BaseXScheme}://' URIs; use {nameof(DatabaseLoaderOptions)} instead.");

        var baseX = options?.BaseX ?? new BaseXConnectionOptions();
        Uri restUri;
        if (!string.IsNullOrEmpty(baseX.EndpointBase))
        {
            restUri = new Uri(baseX.EndpointBase.TrimEnd('/') + parsed.AbsolutePath, UriKind.Absolute);
        }
        else
        {
            var builder = new UriBuilder("http", parsed.Host, parsed.Port > 0 ? parsed.Port : 8984, "/rest" + parsed.AbsolutePath);
            restUri = builder.Uri;
        }

        AuthenticationHeaderValue? auth = null;
        if (!string.IsNullOrEmpty(baseX.Username))
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{baseX.Username}:{baseX.Password ?? string.Empty}"));
            auth = new AuthenticationHeaderValue("Basic", token);
        }

        return new Connection(restUri, auth);
    }

    private static HttpRequestMessage CreateRequest(Uri restUri, AuthenticationHeaderValue? auth)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, restUri);
        if (auth is not null)
            request.Headers.Authorization = auth;
        return request;
    }

    // Sync-over-async with ConfigureAwait(false) — the established engine idiom (fn:unparsed-text).
    // HttpRequestException (DNS/connect/reset) and TaskCanceledException (timeout/cancellation)
    // are normalized to IOException so EvaluationContext.LoadDocument maps them to FODC0002.
    private static HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completion, Uri restUri)
    {
        HttpResponseMessage response;
        try
        {
            response = _httpClient.SendAsync(request, completion, CancellationToken.None)
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (HttpRequestException e)
        {
            throw new IOException($"Cannot reach the BaseX REST endpoint '{restUri}': {e.Message}", e);
        }
        catch (TaskCanceledException e)
        {
            throw new IOException($"Timed out reaching the BaseX REST endpoint '{restUri}'.", e);
        }

        if (!response.IsSuccessStatusCode)
        {
            var status = (int)response.StatusCode;
            var reason = response.ReasonPhrase ?? "error";
            response.Dispose();
            throw new IOException($"The BaseX REST endpoint '{restUri}' returned HTTP {status} ({reason}).");
        }

        return response;
    }
}
