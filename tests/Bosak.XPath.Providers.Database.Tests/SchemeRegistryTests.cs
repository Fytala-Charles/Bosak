// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Unit tests for the basex/exist/marklogic scheme registry and per-scheme REST URI mapping
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
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
using System.Text;
using System.Xml;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.XPath.Providers.Database.Tests;

/// <summary>
/// Tests for the REQ-120 Slice 2 scheme registry: <c>exist://</c> and <c>marklogic://</c>
/// REST mapping (default/custom port, <c>EndpointBase</c> override, MarkLogic's
/// query-parameter document URI and <c>Accept</c> header), dispatch across all three
/// schemes, and per-scheme fallback delegation.
/// </summary>
public sealed class SchemeRegistryTests : IDisposable
{
    private readonly DatabaseRestStub _stub = new();

    /// <inheritdoc />
    public void Dispose() => _stub.Dispose();

    private string ExistUri(string path) => $"exist://127.0.0.1:{_stub.Port}{path}";

    private string MarkLogicUri(string path) => $"marklogic://127.0.0.1:{_stub.Port}{path}";

    private static List<IXdmNode> Nodes(XdmSequence sequence)
    {
        var list = new List<IXdmNode>();
        foreach (var item in sequence)
            if (item.IsNode) list.Add(item.NodeValue!);
        return list;
    }

    // ----- default-port mapping (pure URI building, no network) -----

    [Fact]
    public void ExistUri_WithoutPort_MapsToDefaultPort8080()
    {
        var scheme = DatabaseScheme.Registry[DatabaseDocumentLoader.ExistScheme];
        var restUri = scheme.BuildRestUri(new Uri("exist://db.example/db/x.xml"), new ExistConnectionOptions());

        Assert.Equal("http://db.example:8080/exist/rest/db/x.xml", restUri.AbsoluteUri);
    }

    [Fact]
    public void MarkLogicUri_WithoutPort_MapsToDefaultPort8000WithQueryParameter()
    {
        var scheme = DatabaseScheme.Registry[DatabaseDocumentLoader.MarkLogicScheme];
        var restUri = scheme.BuildRestUri(new Uri("marklogic://ml.example/db/x.xml"), new MarkLogicConnectionOptions());

        Assert.Equal("http://ml.example:8000", restUri.GetLeftPart(UriPartial.Authority));
        Assert.Equal("/v1/documents", restUri.AbsolutePath);
        Assert.Equal("uri=%2Fdb%2Fx.xml", restUri.Query.TrimStart('?'));
        Assert.Equal("/db/x.xml", Uri.UnescapeDataString(restUri.Query.TrimStart('?')["uri=".Length..]));
    }

    [Fact]
    public void MarkLogicUri_EndpointBaseOverride_AppendsDocumentsSuffixAndUriParameter()
    {
        var scheme = DatabaseScheme.Registry[DatabaseDocumentLoader.MarkLogicScheme];
        var options = new MarkLogicConnectionOptions { EndpointBase = "https://gateway.example/proxied/" };
        var restUri = scheme.BuildRestUri(new Uri("marklogic://ignored.example/db/x.xml"), options);

        Assert.Equal("https://gateway.example/proxied/v1/documents", restUri.GetLeftPart(UriPartial.Path));
        Assert.Equal("uri=%2Fdb%2Fx.xml", restUri.Query.TrimStart('?'));
    }

    [Fact]
    public void BaseXUri_WithoutPort_StillMapsToDefaultPort8984()
    {
        var scheme = DatabaseScheme.Registry[DatabaseDocumentLoader.BaseXScheme];
        var restUri = scheme.BuildRestUri(new Uri("basex://db.example/db/x.xml"), new BaseXConnectionOptions());

        Assert.Equal("http://db.example:8984/rest/db/x.xml", restUri.AbsoluteUri);
    }

    // ----- exist:// over the live stub -----

    [Fact]
    public void Dispatch_ExistUri_FetchesViaExistRestPath()
    {
        var loader = DatabaseDocumentLoader.Dispatch(
            _ => throw new InvalidOperationException("the fallback must not be called for exist:// URIs"));

        var uri = ExistUri("/db/inventory.xml");
        var node = loader(uri);

        var request = Assert.Single(_stub.Requests);
        Assert.Equal("GET", request.Method);
        Assert.Equal("/exist/rest/db/inventory.xml", request.Path);
        Assert.Equal(string.Empty, request.Query);

        Assert.Equal(XdmNodeKind.Document, node.NodeKind);
        Assert.Equal(uri, node.DocumentUri);
        var root = Assert.Single(Nodes(node.Axis(XdmAxis.Child)));
        Assert.Equal("inventory", root.LocalName);
    }

    [Fact]
    public void Dispatch_ExistEndpointBaseOverride_UsesOverrideAndIgnoresAuthority()
    {
        var options = new DatabaseLoaderOptions();
        options.Exist.EndpointBase = _stub.BaseUrl + "/proxied";
        var loader = DatabaseDocumentLoader.Dispatch(
            _ => throw new InvalidOperationException("the fallback must not be called"), options);

        loader("exist://db-internal.example/db/inventory.xml");

        var request = Assert.Single(_stub.Requests);
        Assert.Equal("/proxied/db/inventory.xml", request.Path);
    }

    [Fact]
    public void Dispatch_ExistWithCredentials_SendsBasicAuthorizationHeader()
    {
        var options = new DatabaseLoaderOptions();
        options.Exist.Username = "exist-user";
        options.Exist.Password = "exist-pass";
        var loader = DatabaseDocumentLoader.Dispatch(
            _ => throw new InvalidOperationException("the fallback must not be called"), options);

        loader(ExistUri("/db/inventory.xml"));

        var request = Assert.Single(_stub.Requests);
        var expected = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("exist-user:exist-pass"));
        Assert.Equal(expected, request.Authorization);
    }

    // ----- marklogic:// over the live stub -----

    [Fact]
    public void Dispatch_MarkLogicUri_UsesQueryParameterDocumentUriAndXmlAcceptHeader()
    {
        var loader = DatabaseDocumentLoader.Dispatch(
            _ => throw new InvalidOperationException("the fallback must not be called for marklogic:// URIs"));

        var uri = MarkLogicUri("/db/inventory.xml");
        var node = loader(uri);

        var request = Assert.Single(_stub.Requests);
        Assert.Equal("GET", request.Method);
        Assert.Equal("/v1/documents", request.Path);
        Assert.Equal("uri=%2Fdb%2Finventory.xml", request.Query.TrimStart('?'));
        Assert.Equal("application/xml", request.Accept);

        Assert.Equal(XdmNodeKind.Document, node.NodeKind);
        Assert.Equal(uri, node.DocumentUri);
        var root = Assert.Single(Nodes(node.Axis(XdmAxis.Child)));
        Assert.Equal("inventory", root.LocalName);
    }

    [Fact]
    public void Dispatch_MarkLogicWithCredentials_SendsBasicAuthorizationHeader()
    {
        var options = new DatabaseLoaderOptions();
        options.MarkLogic.Username = "ml-user";
        options.MarkLogic.Password = "ml-pass";
        var loader = DatabaseDocumentLoader.Dispatch(
            _ => throw new InvalidOperationException("the fallback must not be called"), options);

        loader(MarkLogicUri("/db/inventory.xml"));

        var request = Assert.Single(_stub.Requests);
        var expected = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("ml-user:ml-pass"));
        Assert.Equal(expected, request.Authorization);
    }

    // ----- registry dispatch -----

    [Fact]
    public void Handles_RecognizesAllRegisteredSchemesOnly()
    {
        Assert.True(DatabaseDocumentLoader.Handles("basex://db.example/db/x.xml"));
        Assert.True(DatabaseDocumentLoader.Handles("exist://db.example/db/x.xml"));
        Assert.True(DatabaseDocumentLoader.Handles("marklogic://db.example/db/x.xml"));
        Assert.True(DatabaseDocumentLoader.Handles("Basex://db.example/db/x.xml")); // case-insensitive
        Assert.False(DatabaseDocumentLoader.Handles("file:///C:/Data/local.xml"));
        Assert.False(DatabaseDocumentLoader.Handles("http://example.org/x.xml"));
        Assert.False(DatabaseDocumentLoader.Handles("not a uri"));
    }

    [Fact]
    public void Dispatch_AllThreeSchemes_FetchesEachViaItsOwnRestShape()
    {
        var loader = DatabaseDocumentLoader.Dispatch(
            _ => throw new InvalidOperationException("the fallback must not be called for database URIs"));

        loader($"basex://127.0.0.1:{_stub.Port}/db/a.xml");
        loader($"exist://127.0.0.1:{_stub.Port}/db/b.xml");
        loader($"marklogic://127.0.0.1:{_stub.Port}/db/c.xml");

        Assert.Equal(3, _stub.Requests.Count);
        Assert.Equal("/rest/db/a.xml", _stub.Requests[0].Path);
        Assert.Equal("/exist/rest/db/b.xml", _stub.Requests[1].Path);
        Assert.Equal("/v1/documents", _stub.Requests[2].Path);
        Assert.Equal("uri=%2Fdb%2Fc.xml", _stub.Requests[2].Query.TrimStart('?'));
    }

    [Fact]
    public void Dispatch_UnknownScheme_DelegatesToFallbackPerSchemeOptions()
    {
        var options = new DatabaseLoaderOptions();
        options.MarkLogic.Username = "ml-user";
        var loader = DatabaseDocumentLoader.Dispatch(uri =>
        {
            Assert.Equal("file:///C:/Data/local.xml", uri);
            return XDocumentProvider.ParseXml("<local/>");
        }, options);

        var node = loader("file:///C:/Data/local.xml");

        Assert.Equal(XdmNodeKind.Document, node.NodeKind);
        Assert.Empty(_stub.Requests);
    }

    [Fact]
    public void DispatchStreaming_ExistUri_ReturnsStreamingDocumentOverExistRestPath()
    {
        var loader = DatabaseDocumentLoader.DispatchStreaming(
            _ => throw new InvalidOperationException("the fallback must not be called"));

        var doc = loader(ExistUri("/db/inventory.xml"));

        var request = Assert.Single(_stub.Requests);
        Assert.Equal("/exist/rest/db/inventory.xml", request.Path);

        var root = Assert.Single(Nodes(doc.Axis(XdmAxis.Child)));
        var products = Nodes(root.Axis(XdmAxis.Child)).Where(p => p.NodeKind == XdmNodeKind.Element).ToList();
        Assert.Equal(3, products.Count);
    }

    [Fact]
    public void DispatchStreaming_MarkLogicUri_StreamsOverQueryParameterForm()
    {
        var loader = DatabaseDocumentLoader.DispatchStreaming(
            _ => throw new InvalidOperationException("the fallback must not be called"));

        var doc = loader(MarkLogicUri("/db/inventory.xml"));

        var request = Assert.Single(_stub.Requests);
        Assert.Equal("/v1/documents", request.Path);
        Assert.Equal("uri=%2Fdb%2Finventory.xml", request.Query.TrimStart('?'));
        Assert.Equal("application/xml", request.Accept);

        var root = Assert.Single(Nodes(doc.Axis(XdmAxis.Child)));
        Assert.Equal("inventory", root.LocalName);
    }

    // ----- error contract per scheme -----

    [Fact]
    public void Load_ExistHttpErrorStatus_ThrowsIOExceptionNamingExist()
    {
        _stub.Responder = _ => new DatabaseRestStub.Response(401, "unauthorized", "text/plain");

        var ex = Assert.Throws<IOException>(() => DatabaseDocumentLoader.Load(ExistUri("/db/missing.xml")));
        Assert.Contains("401", ex.Message);
        Assert.Contains("eXist", ex.Message);
    }

    [Fact]
    public void Load_MarkLogicHttpErrorStatus_ThrowsIOExceptionNamingMarkLogic()
    {
        _stub.Responder = _ => new DatabaseRestStub.Response(404, "not found", "text/plain");

        var ex = Assert.Throws<IOException>(() => DatabaseDocumentLoader.Load(MarkLogicUri("/db/missing.xml")));
        Assert.Contains("404", ex.Message);
        Assert.Contains("MarkLogic", ex.Message);
    }

    [Fact]
    public void Load_MarkLogicMalformedResponseBody_ThrowsXmlException()
    {
        _stub.Responder = _ => new DatabaseRestStub.Response(200, "not xml at all");

        Assert.Throws<XmlException>(() => DatabaseDocumentLoader.Load(MarkLogicUri("/db/broken.xml")));
    }

    [Fact]
    public void Load_ExistUriEmbeddedCredentials_ThrowsUriFormatException()
    {
        var ex = Assert.Throws<UriFormatException>(
            () => DatabaseDocumentLoader.Load($"exist://user:pass@127.0.0.1:{_stub.Port}/db/x.xml"));
        Assert.Contains("credentials", ex.Message);
    }
}
