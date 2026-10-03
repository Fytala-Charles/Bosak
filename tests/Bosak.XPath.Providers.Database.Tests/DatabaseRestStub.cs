// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Loopback HttpListener stub faking the BaseX/eXist/MarkLogic REST surfaces for the loader tests
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-10-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 03-10-2026     | Slice 2: renamed to DatabaseRestStub; requests record the query string and Accept header |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Bosak.XPath.Providers.Database.Tests;

/// <summary>
/// A loopback <see cref="HttpListener"/> stub that fakes the database REST surfaces
/// (BaseX <c>GET /rest/db/resource.xml</c>, eXist <c>GET /exist/rest/db/resource.xml</c>,
/// MarkLogic <c>GET /v1/documents?uri=…</c>) on a dynamic port, so the loader tests run in
/// CI without a real database. Requests are recorded; the canned response is configurable
/// per test.
/// </summary>
internal sealed class DatabaseRestStub : IDisposable
{
    /// <summary>
    /// A canned HTTP response served by the stub.
    /// </summary>
    public sealed record Response(int StatusCode, string Body, string ContentType = "application/xml");

    /// <summary>
    /// A request the stub received, recorded for assertions.
    /// </summary>
    public sealed record Request(string Method, string Path, string Query, string? Accept, string? Authorization);

    private readonly HttpListener _listener;
    private readonly Task _loop;
    private readonly object _gate = new();
    private readonly List<Request> _requests = new();

    /// <summary>
    /// Initializes a new stub listening on <c>http://127.0.0.1:&lt;dynamic-port&gt;/</c>.
    /// </summary>
    public DatabaseRestStub()
    {
        // HttpListener has no LocalEndpoint: probe a free loopback port with a throwaway
        // TcpListener, release it, and register the HttpListener prefix on that port.
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        Port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
        _listener.Start();
        _loop = Task.Run(AcceptLoopAsync);
    }

    /// <summary>
    /// Gets the loopback port the stub listens on.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Gets the stub base URL (<c>http://127.0.0.1:port</c>).
    /// </summary>
    public string BaseUrl => $"http://127.0.0.1:{Port}";

    /// <summary>
    /// Gets or sets the responder producing the canned response for each request.
    /// Defaults to a small inventory document with a 200 status.
    /// </summary>
    public Func<Request, Response> Responder { get; set; } = _ => DefaultResponse;

    /// <summary>
    /// The default canned document served by the stub.
    /// </summary>
    public static Response DefaultResponse => new(200, """
        <?xml version="1.0" encoding="UTF-8"?>
        <inventory count="3">
          <product id="1"><name>Hammer</name><price>9.99</price></product>
          <product id="2"><name>Saw</name><price>19.99</price></product>
          <product id="3"><name>Pliers</name><price>4.99</price></product>
        </inventory>
        """);

    /// <summary>
    /// Gets the requests received so far, in arrival order.
    /// </summary>
    public IReadOnlyList<Request> Requests
    {
        get
        {
            lock (_gate)
                return _requests.ToList();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _listener.Close();
        try
        {
            _loop.Wait(TimeSpan.FromSeconds(10));
        }
        catch
        {
            // The accept loop exits via HttpListenerException/ObjectDisposedException
            // once the listener is closed; nothing actionable remains at dispose time.
        }
    }

    private async Task AcceptLoopAsync()
    {
        while (true)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (HttpListenerException)
            {
                return; // listener closed
            }
            catch (ObjectDisposedException)
            {
                return; // listener closed
            }
            catch (InvalidOperationException)
            {
                return; // listener stopped
            }

            Handle(context);
        }
    }

    private void Handle(HttpListenerContext context)
    {
        Request request;
        lock (_gate)
        {
            request = new Request(
                context.Request.HttpMethod,
                context.Request.Url?.AbsolutePath ?? string.Empty,
                context.Request.Url?.Query ?? string.Empty,
                context.Request.Headers["Accept"],
                context.Request.Headers["Authorization"]);
            _requests.Add(request);
        }

        var response = Responder(request);
        var body = Encoding.UTF8.GetBytes(response.Body);
        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = response.ContentType;
        context.Response.ContentLength64 = body.Length;
        context.Response.OutputStream.Write(body, 0, body.Length);
        context.Response.Close();
    }
}
