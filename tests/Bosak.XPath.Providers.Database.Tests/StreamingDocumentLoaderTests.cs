// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Unit tests for the streaming DatabaseDocumentLoader dispatch (forward-only path)
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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.IO;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.XPath.Providers.Database.Tests;

/// <summary>
/// Tests for <see cref="DatabaseDocumentLoader.DispatchStreaming"/>: the BaseX REST response
/// stream feeds <see cref="XmlStreamingProvider"/> so records materialize one at a time
/// (bounded memory, forward-only), and non-basex URIs fall through to the fallback.
/// </summary>
public sealed class StreamingDocumentLoaderTests : IDisposable
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
    public void DispatchStreaming_BasexUri_ReturnsForwardOnlyStreamingDocument()
    {
        var loader = DatabaseDocumentLoader.DispatchStreaming(
            _ => throw new InvalidOperationException("the fallback must not be called for basex:// URIs"));

        var uri = BaseXUri("/db/inventory.xml");
        var doc = loader(uri);

        var request = Assert.Single(_stub.Requests);
        Assert.Equal("/rest/db/inventory.xml", request.Path);

        Assert.Equal(XdmNodeKind.Document, doc.NodeKind);
        Assert.Equal(uri, doc.DocumentUri);

        var root = Assert.Single(Nodes(doc.Axis(XdmAxis.Child)));
        Assert.Equal("inventory", root.LocalName);

        // Enumerate the record path once: the top-level product records stream off the
        // response one at a time.
        var products = Nodes(root.Axis(XdmAxis.Child)).Where(p => p.NodeKind == XdmNodeKind.Element).ToList();
        Assert.Equal(3, products.Count);
        Assert.Equal(new[] { "1", "2", "3" },
            products.Select(p => Assert.Single(Nodes(p.Attributes("id"))).StringValue));
        Assert.Equal("Hammer", Nodes(products[0].Axis(XdmAxis.Child)).First(c => c.LocalName == "name").StringValue);
    }

    [Fact]
    public void DispatchStreaming_SecondEnumerationOfRecords_Throws()
    {
        var loader = DatabaseDocumentLoader.DispatchStreaming(
            _ => throw new InvalidOperationException("the fallback must not be called"));
        var doc = loader(BaseXUri("/db/inventory.xml"));
        var root = Assert.Single(Nodes(doc.Axis(XdmAxis.Child)));

        _ = Nodes(root.Axis(XdmAxis.Child));

        var ex = Assert.Throws<StreamingException>(() => Nodes(root.Axis(XdmAxis.Child)));
        Assert.Contains("forward-only", ex.Message);
    }

    [Fact]
    public void DispatchStreaming_OtherUri_DelegatesToFallback()
    {
        var loader = DatabaseDocumentLoader.DispatchStreaming(uri =>
        {
            Assert.Equal("file:///C:/Data/local.xml", uri);
            return XDocumentProvider.ParseXml("<local/>");
        });

        var node = loader("file:///C:/Data/local.xml");

        Assert.Equal(XdmNodeKind.Document, node.NodeKind);
        Assert.Empty(_stub.Requests);
    }

    [Fact]
    public void DispatchStreaming_UnreachableEndpoint_ThrowsIOException()
    {
        var port = _stub.Port; // reuse a port the stub proved free, then release it
        _stub.Dispose();
        WaitForPortRelease(port);

        var loader = DatabaseDocumentLoader.DispatchStreaming(XDocumentProvider.LoadFile);

        Assert.Throws<IOException>(() => loader($"basex://127.0.0.1:{port}/db/x.xml"));
    }

    private static void WaitForPortRelease(int port)
    {
        for (var i = 0; i < 100; i++)
        {
            try
            {
                using var client = new System.Net.Sockets.TcpClient();
                var connect = client.ConnectAsync("127.0.0.1", port);
                if (connect.Wait(TimeSpan.FromMilliseconds(200)) && client.Connected)
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
