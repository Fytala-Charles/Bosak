// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Unit tests for the database collection listing (LoadCollection / DispatchCollection, REQ-120 Slice 3)
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-10-2026     | Creation (REQ-120 Slice 3)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Providers.Database.Tests;

/// <summary>
/// Tests for <see cref="DatabaseDocumentLoader.LoadCollection(string, DatabaseLoaderOptions?)"/> and
/// <see cref="DatabaseDocumentLoader.DispatchCollection"/>: per-scheme listing wire shapes
/// (BaseX/eXist XML listings with nested directories, MarkLogic <c>view=uris</c> search),
/// scheme-URI reassembly, dispatch fallback delegation, and the error contract.
/// </summary>
public sealed class DatabaseCollectionLoaderTests : IDisposable
{
    private readonly DatabaseRestStub _stub = new();

    /// <inheritdoc />
    public void Dispose() => _stub.Dispose();

    private string BaseXUri(string path) => $"basex://127.0.0.1:{_stub.Port}{path}";

    private DatabaseLoaderOptions EndpointOptions() => new()
    {
        BaseX = { EndpointBase = $"{_stub.BaseUrl}/rest" },
        Exist = { EndpointBase = $"{_stub.BaseUrl}/exist/rest" },
        MarkLogic = { EndpointBase = _stub.BaseUrl },
    };

    [Fact]
    public void LoadCollection_BaseX_ReturnsMembersInListingOrder()
    {
        _stub.Responder = request => request.Path switch
        {
            "/rest/db/coll" => new(200, """
                <?xml version="1.0" encoding="UTF-8"?>
                <rest:database xmlns:rest="http://basex.org/rest" name="coll" resources="3" size="100">
                  <rest:resource name="a.xml" size="10"/>
                  <rest:directory name="sub" resources="1">
                    <rest:resource name="b.xml" size="20"/>
                  </rest:directory>
                </rest:database>
                """),
            _ => new(404, "not found"),
        };

        var options = EndpointOptions();
        var members = DatabaseDocumentLoader.LoadCollection(BaseXUri("/db/coll"), options);

        Assert.Equal(new[]
        {
            $"basex://127.0.0.1:{_stub.Port}/db/coll/a.xml",
            $"basex://127.0.0.1:{_stub.Port}/db/coll/sub/b.xml",
        }, members);

        var listing = Assert.Single(_stub.Requests);
        Assert.Equal("/rest/db/coll", listing.Path);
    }

    [Fact]
    public void LoadCollection_BaseX_EmptyDirectoryTriggersFollowUpRequest()
    {
        _stub.Responder = request => request.Path switch
        {
            "/rest/db/coll" => new(200, """
                <rest:database xmlns:rest="http://basex.org/rest" name="coll" resources="1" size="10">
                  <rest:resource name="a.xml" size="10"/>
                  <rest:directory name="empty" resources="0"/>
                </rest:database>
                """),
            "/rest/db/coll/empty" => new(200, """
                <rest:database xmlns:rest="http://basex.org/rest" name="empty" resources="0" size="0"/>
                """),
            _ => new(404, "not found"),
        };

        var members = DatabaseDocumentLoader.LoadCollection(BaseXUri("/db/coll"), EndpointOptions());

        var expected = new[] { $"basex://127.0.0.1:{_stub.Port}/db/coll/a.xml" };
        Assert.Equal(expected, members);
        Assert.Equal(new[] { "/rest/db/coll", "/rest/db/coll/empty" }, _stub.Requests.Select(r => r.Path));
    }

    [Fact]
    public void LoadCollection_Exist_RecursesIntoSubcollections()
    {
        _stub.Responder = request => request.Path switch
        {
            "/exist/rest/db/coll" => new(200, """
                <?xml version="1.0" encoding="UTF-8"?>
                <collection name="/db/coll" created="2026-01-01T00:00:00Z">
                  <resource name="a.xml" size="10" created="2026-01-01T00:00:00Z"/>
                  <subcollection name="sub" created="2026-01-01T00:00:00Z"/>
                </collection>
                """),
            "/exist/rest/db/coll/sub" => new(200, """
                <?xml version="1.0" encoding="UTF-8"?>
                <collection name="/db/coll/sub" created="2026-01-01T00:00:00Z">
                  <resource name="b.xml" size="20" created="2026-01-01T00:00:00Z"/>
                </collection>
                """),
            _ => new(404, "not found"),
        };

        var members = DatabaseDocumentLoader.LoadCollection(
            $"exist://127.0.0.1:{_stub.Port}/db/coll", EndpointOptions());

        Assert.Equal(new[]
        {
            $"exist://127.0.0.1:{_stub.Port}/db/coll/a.xml",
            $"exist://127.0.0.1:{_stub.Port}/db/coll/sub/b.xml",
        }, members);
        Assert.Equal(new[] { "/exist/rest/db/coll", "/exist/rest/db/coll/sub" }, _stub.Requests.Select(r => r.Path));
    }

    [Fact]
    public void LoadCollection_MarkLogic_UsesSearchViewUris()
    {
        _stub.Responder = request =>
        {
            if (request.Path == "/v1/search")
            {
                var query = Uri.UnescapeDataString(request.Query.TrimStart('?'));
                Assert.Contains("directory=/db/coll/", query);
                Assert.Contains("view=uris", query);
                Assert.Contains("depth=Infinity", query);
                Assert.Equal("application/xml", request.Accept);
                return new(200, """
                    <?xml version="1.0" encoding="UTF-8"?>
                    <search:response xmlns:search="http://marklogic.com/appservices/search" total="2">
                      <search:uri>/db/coll/a.xml</search:uri>
                      <search:uri>/db/coll/b.xml</search:uri>
                    </search:response>
                    """);
            }

            return new(404, "not found");
        };

        var members = DatabaseDocumentLoader.LoadCollection(
            $"marklogic://127.0.0.1:{_stub.Port}/db/coll", EndpointOptions());

        Assert.Equal(new[]
        {
            $"marklogic://127.0.0.1:{_stub.Port}/db/coll/a.xml",
            $"marklogic://127.0.0.1:{_stub.Port}/db/coll/b.xml",
        }, members);
    }

    [Fact]
    public void LoadCollection_WithoutPort_RebuildsMemberUrisWithDefaultPort()
    {
        _stub.Responder = request => request.Path switch
        {
            "/rest/db/coll" => new(200, """
                <rest:database xmlns:rest="http://basex.org/rest" name="coll" resources="1" size="10">
                  <rest:resource name="a.xml" size="10"/>
                </rest:database>
                """),
            _ => new(404, "not found"),
        };

        var members = DatabaseDocumentLoader.LoadCollection("basex://127.0.0.1/db/coll", EndpointOptions());

        Assert.Equal(new[] { "basex://127.0.0.1:8984/db/coll/a.xml" }, members);
    }

    [Fact]
    public void LoadCollection_UnregisteredScheme_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => DatabaseDocumentLoader.LoadCollection("file:///tmp/coll"));
        Assert.Contains("basex", ex.Message);
    }

    [Fact]
    public void LoadCollection_UserInfoInUri_ThrowsUriFormatException()
    {
        var ex = Assert.Throws<UriFormatException>(
            () => DatabaseDocumentLoader.LoadCollection(BaseXUri("/db/coll").Replace("://", "://user:pass@")));
        Assert.Contains("credentials", ex.Message);
    }

    [Fact]
    public void LoadCollection_EndpointError_ThrowsIOException()
    {
        _stub.Responder = _ => new(404, "not found");

        var ex = Assert.Throws<IOException>(
            () => DatabaseDocumentLoader.LoadCollection(BaseXUri("/db/coll"), EndpointOptions()));
        Assert.Contains("HTTP 404", ex.Message);
    }

    [Fact]
    public void LoadCollection_MalformedListing_ThrowsXmlException()
    {
        _stub.Responder = _ => new(200, "<not-xml");

        Assert.Throws<XmlException>(
            () => DatabaseDocumentLoader.LoadCollection(BaseXUri("/db/coll"), EndpointOptions()));
    }

    [Fact]
    public void DispatchCollection_DatabaseSchemeUri_ListsCollection()
    {
        _stub.Responder = request => request.Path switch
        {
            "/rest/db/coll" => new(200, """
                <rest:database xmlns:rest="http://basex.org/rest" name="coll" resources="1" size="10">
                  <rest:resource name="a.xml" size="10"/>
                </rest:database>
                """),
            _ => new(404, "not found"),
        };

        var resolver = DatabaseDocumentLoader.DispatchCollection(_ => null, EndpointOptions());
        var members = resolver(BaseXUri("/db/coll"));

        Assert.Equal(new[] { $"basex://127.0.0.1:{_stub.Port}/db/coll/a.xml" }, members);
    }

    [Fact]
    public void DispatchCollection_OtherSchemeUri_DelegatesToFallback()
    {
        var resolver = DatabaseDocumentLoader.DispatchCollection(
            uri =>
            {
                Assert.Equal("file:///tmp/coll", uri);
                return new[] { "file:///tmp/coll/a.xml" };
            });

        var members = resolver("file:///tmp/coll");

        Assert.Equal(new[] { "file:///tmp/coll/a.xml" }, members);
        Assert.Empty(_stub.Requests);
    }

    [Fact]
    public void CollectionLoader_SeamEndToEnd_LoadsMembersThroughDocumentLoader()
    {
        _stub.Responder = request => request.Path switch
        {
            "/rest/db/coll" => new(200, """
                <rest:database xmlns:rest="http://basex.org/rest" name="coll" resources="2" size="20">
                  <rest:resource name="a.xml" size="10"/>
                  <rest:resource name="b.xml" size="10"/>
                </rest:database>
                """),
            "/rest/db/coll/a.xml" => new(200, "<a/>"),
            "/rest/db/coll/b.xml" => new(200, "<b/>"),
            _ => new(404, "not found"),
        };

        var options = EndpointOptions();
        var ctx = new EvaluationContext
        {
            DocumentLoader = DatabaseDocumentLoader.Dispatch(XDocumentProvider.LoadFile, options),
            CollectionLoader = DatabaseDocumentLoader.DispatchCollection(_ => null, options),
        };

        var collectionUri = BaseXUri("/db/coll");
        var members = ctx.CollectionLoader(collectionUri);

        Assert.Equal(new[]
        {
            $"basex://127.0.0.1:{_stub.Port}/db/coll/a.xml",
            $"basex://127.0.0.1:{_stub.Port}/db/coll/b.xml",
        }, members);

        // Members are ordinary database document URIs: they resolve through the
        // document loader with the engine's FODC0002/FODC0005 error contract.
        Assert.NotNull(members);
        var first = ctx.LoadDocument(members[0]);
        string? rootName = null;
        foreach (var child in first.Children(XdmNodeKind.Element))
        {
            Assert.True(child.IsNode);
            rootName = child.NodeValue!.LocalName;
            break;
        }

        Assert.Equal("a", rootName);
        var ex = Assert.Throws<InvalidOperationException>(() => ctx.LoadDocument(BaseXUri("/db/coll/missing.xml")));
        Assert.Contains("FODC0002", ex.Message);
    }
}
