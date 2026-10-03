// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Unit tests for the streaming teardown — the response-bound stream disposing the HTTP response
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-10-2026     | Creation (REQ-120 Slice 2 streaming teardown)                                            |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Net;
using System.Net.Http;
using System.Text;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Providers.Database.Tests;

/// <summary>
/// Tests for the REQ-120 Slice 2 streaming teardown: the content stream handed to
/// <c>XmlStreamingProvider</c> is a <c>ResponseBoundStream</c> that disposes the owning
/// <see cref="HttpResponseMessage"/> when the stream is disposed. The streaming source
/// disposes that stream deterministically at end-of-stream (and on load failure) via
/// <c>XmlReaderSettings.CloseInput</c>, so response disposal is asserted through a tracking
/// <see cref="HttpContent"/> — content disposal only happens when the response itself is
/// disposed, not when the bare content stream is closed (the Slice 1 behavior this test
/// guards against).
/// </summary>
public sealed class StreamingTeardownTests
{
    [Fact]
    public void LoadStreaming_WithResponseBoundStream_FullEnumerationStillYieldsAllRecords()
    {
        using var stub = new DatabaseRestStub();
        var loader = DatabaseDocumentLoader.DispatchStreaming(
            _ => throw new InvalidOperationException("the fallback must not be called"));

        var doc = loader($"basex://127.0.0.1:{stub.Port}/db/inventory.xml");

        // Fully enumerate the records through the response-bound stream (the Slice 2
        // wiring): the streaming source reads to the end of the root element and disposes
        // its reader — CloseInput = true then disposes the response-bound stream. Response
        // disposal itself is asserted by the tracking-content unit tests below (it is not
        // observable through HttpListener, since a fully-read keep-alive connection is
        // pooled rather than closed).
        var root = SingleChild(doc);
        var elementCount = 0;
        foreach (var record in root.Axis(XdmAxis.Child))
            if (record.IsNode && record.NodeValue!.NodeKind == XdmNodeKind.Element)
                elementCount++;

        Assert.Equal(3, elementCount);
    }

    [Fact]
    public void ResponseBoundStream_Dispose_DisposesResponseContent()
    {
        var inner = new MemoryStream(Encoding.UTF8.GetBytes("<r/>"));
        var content = new TrackingContent(inner);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        var bound = new ResponseBoundStream(inner, response);

        bound.Dispose();

        Assert.True(content.Disposed);
    }

    [Fact]
    public void ResponseBoundStream_DoubleDispose_IsIdempotent()
    {
        var inner = new MemoryStream(Encoding.UTF8.GetBytes("<r/>"));
        var content = new TrackingContent(inner);
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        var bound = new ResponseBoundStream(inner, response);

        bound.Dispose();
        bound.Dispose();

        Assert.True(content.Disposed);
    }

    private static IXdmNode SingleChild(IXdmNode node)
    {
        IXdmNode? child = null;
        foreach (var item in node.Axis(XdmAxis.Child))
        {
            Assert.True(item.IsNode);
            Assert.Null(child);
            child = item.NodeValue;
        }

        return Assert.IsAssignableFrom<IXdmNode>(child);
    }

    private sealed class TrackingContent : HttpContent
    {
        private readonly Stream _stream;

        public TrackingContent(Stream stream) => _stream = stream;

        public bool Disposed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => Task.CompletedTask;

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disposed = true;
                _stream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
