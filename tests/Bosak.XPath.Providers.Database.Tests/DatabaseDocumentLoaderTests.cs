// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Unit tests for the scheme-dispatching DatabaseDocumentLoader (in-memory path)
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
//                      | Charles Korthout | 0.2   | 03-10-2026     | WaitForPortRelease: tolerate a faulted connect task (AggregateException on Linux CI)     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using System.Xml;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Providers.Database.Tests;

/// <summary>
/// Tests for <see cref="DatabaseDocumentLoader"/>: scheme dispatch, REST fetch →
/// <see cref="IXdmNode"/>, fallback delegation, endpoint/auth options, and the error
/// contract (FODC0002/FODC0005 class) as mapped by <see cref="EvaluationContext.LoadDocument"/>.
/// </summary>
public sealed class DatabaseDocumentLoaderTests : IDisposable
{
    private readonly BaseXRestStub _stub = new();

    /// <inheritdoc />
    public void Dispose() => _stub.Dispose();

    private string BaseXUri(string path) => $"basex://127.0.0.1:{_stub.Port}{path}";

    private static List<IXdmNode> Nodes(XdmSequence sequence)
    {
        var list = new List<IXdmNode>();
        foreach (var item in sequence)
            if (item.IsNode) list.Add(item.NodeValue!);
        return list;
    }

    [Fact]
    public void Dispatch_BasexUri_FetchesViaRestAndWrapsDocument()
    {
        var loader = DatabaseDocumentLoader.Dispatch(
            _ => throw new InvalidOperationException("the fallback must not be called for basex:// URIs"));

        var uri = BaseXUri("/db/inventory.xml");
        var node = loader(uri);

        var request = Assert.Single(_stub.Requests);
        Assert.Equal("GET", request.Method);
        Assert.Equal("/rest/db/inventory.xml", request.Path);

        Assert.Equal(XdmNodeKind.Document, node.NodeKind);
        Assert.Equal(uri, node.DocumentUri);

        var root = Assert.Single(Nodes(node.Axis(XdmAxis.Child)));
        Assert.Equal(XdmNodeKind.Element, root.NodeKind);
        Assert.Equal("inventory", root.LocalName);
        Assert.Equal("3", Assert.Single(Nodes(root.Attributes("count"))).StringValue);

        var products = Nodes(root.Axis(XdmAxis.Child)).Where(p => p.NodeKind == XdmNodeKind.Element).ToList();
        Assert.Equal(3, products.Count);
        Assert.Equal(new[] { "Hammer", "Saw", "Pliers" },
            products.Select(p => Nodes(p.Axis(XdmAxis.Child)).First(c => c.LocalName == "name").StringValue));
    }

    [Fact]
    public void Dispatch_OtherUri_DelegatesToFallback()
    {
        IXdmNode? fallbackResult = null;
        var loader = DatabaseDocumentLoader.Dispatch(uri =>
        {
            Assert.Equal("file:///C:/Data/local.xml", uri);
            fallbackResult = XDocumentProvider.ParseXml("<local/>");
            return fallbackResult;
        });

        var node = loader("file:///C:/Data/local.xml");

        Assert.Same(fallbackResult, node);
        Assert.Empty(_stub.Requests);
    }

    [Fact]
    public void Load_UnreachableEndpoint_ThrowsIOException()
    {
        var port = _stub.Port; // reuse a port the stub proved free, then release it
        _stub.Dispose();
        WaitForPortRelease(port);

        var ex = Assert.Throws<IOException>(() => DatabaseDocumentLoader.Load($"basex://127.0.0.1:{port}/db/x.xml"));
        Assert.Contains("127.0.0.1", ex.Message);
    }

    [Fact]
    public void Load_HttpErrorStatus_ThrowsIOException()
    {
        _stub.Responder = _ => new BaseXRestStub.Response(404, "not found", "text/plain");

        var ex = Assert.Throws<IOException>(() => DatabaseDocumentLoader.Load(BaseXUri("/db/missing.xml")));
        Assert.Contains("404", ex.Message);
    }

    [Fact]
    public void LoadDocument_ThroughEvaluationContext_MapsLoaderFailureToFODC0002()
    {
        var port = _stub.Port;
        _stub.Dispose();
        WaitForPortRelease(port);

        var context = new EvaluationContext
        {
            DocumentLoader = DatabaseDocumentLoader.Dispatch(XDocumentProvider.LoadFile),
        };

        var ex = Assert.Throws<InvalidOperationException>(
            () => context.LoadDocument($"basex://127.0.0.1:{port}/db/x.xml"));
        Assert.Contains("FODC0002", ex.Message);
    }

    [Fact]
    public void Load_MalformedResponseBody_ThrowsXmlException()
    {
        _stub.Responder = _ => new BaseXRestStub.Response(200, "this is not xml");

        Assert.Throws<XmlException>(() => DatabaseDocumentLoader.Load(BaseXUri("/db/broken.xml")));
    }

    [Fact]
    public void Dispatch_WithCredentials_SendsBasicAuthorizationHeader()
    {
        var options = new DatabaseLoaderOptions();
        options.BaseX.Username = "admin";
        options.BaseX.Password = "s3cret";
        var loader = DatabaseDocumentLoader.Dispatch(
            _ => throw new InvalidOperationException("the fallback must not be called"), options);

        loader(BaseXUri("/db/inventory.xml"));

        var request = Assert.Single(_stub.Requests);
        var expected = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:s3cret"));
        Assert.Equal(expected, request.Authorization);
    }

    [Fact]
    public void Dispatch_WithEndpointBaseOverride_UsesOverrideAndIgnoresAuthority()
    {
        var options = new DatabaseLoaderOptions();
        options.BaseX.EndpointBase = _stub.BaseUrl + "/proxied";
        var loader = DatabaseDocumentLoader.Dispatch(
            _ => throw new InvalidOperationException("the fallback must not be called"), options);

        loader("basex://db-internal.example/db/inventory.xml");

        var request = Assert.Single(_stub.Requests);
        Assert.Equal("/proxied/db/inventory.xml", request.Path);
    }

    [Fact]
    public void Load_NonBasexUri_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DatabaseDocumentLoader.Load("file:///C:/Data/local.xml"));
    }

    [Fact]
    public void Load_UriEmbeddedCredentials_ThrowsUriFormatException()
    {
        var ex = Assert.Throws<UriFormatException>(
            () => DatabaseDocumentLoader.Load($"basex://user:pass@127.0.0.1:{_stub.Port}/db/x.xml"));
        Assert.Contains("credentials", ex.Message);
    }

    private static void WaitForPortRelease(int port)
    {
        // HttpListener releases its URL registration asynchronously after Close(); briefly
        // retry a raw TCP connect until the port refuses connections (connection refused
        // proves the stub is gone and the loader will see a genuinely unreachable endpoint).
        for (var i = 0; i < 100; i++)
        {
            try
            {
                using var client = new System.Net.Sockets.TcpClient();
                var connect = client.ConnectAsync("127.0.0.1", port);
                bool connected;
                try
                {
                    // A faulted connect task (connection refused) proves the stub is gone.
                    // Task.Wait rethrows it as AggregateException — the common case on Linux CI.
                    connected = connect.Wait(TimeSpan.FromMilliseconds(200)) && client.Connected;
                }
                catch (AggregateException)
                {
                    return; // connection refused: the stub is gone
                }

                if (connected)
                {
                    client.Close();
                    System.Threading.Thread.Sleep(50);
                    continue;
                }

                return; // connect timed out: nothing is accepting
            }
            catch (System.Net.Sockets.SocketException)
            {
                return; // connection refused: the stub is gone
            }
        }
    }
}
