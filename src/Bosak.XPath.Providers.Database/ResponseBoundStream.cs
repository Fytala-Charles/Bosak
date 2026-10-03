// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 October 2026
// PURPOSE              : Stream wrapper that disposes the owning HTTP response when the stream is disposed
// SPECIAL NOTES        : Part of the Bosak database document-loader package (REQ-120).
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
using System.Net.Http;

namespace Bosak.XPath.Providers.Database;

/// <summary>
/// Binds an <see cref="HttpResponseMessage"/> to its content stream: disposing the stream
/// (which the streaming source does deterministically at end-of-stream and on failure via
/// <c>XmlReaderSettings.CloseInput</c>) also disposes the response, releasing the connection
/// and the response's unmanaged resources without waiting for finalization. Disposal is
/// idempotent.
/// </summary>
internal sealed class ResponseBoundStream : Stream
{
    private readonly Stream _inner;
    private readonly HttpResponseMessage _response;

    public ResponseBoundStream(Stream inner, HttpResponseMessage response)
    {
        _inner = inner;
        _response = response;
    }

    public override bool CanRead => _inner.CanRead;

    public override bool CanSeek => _inner.CanSeek;

    public override bool CanWrite => _inner.CanWrite;

    public override long Length => _inner.Length;

    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public override void Flush() => _inner.Flush();

    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

    public override void SetLength(long value) => _inner.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
            _response.Dispose();
        }

        base.Dispose(disposing);
    }
}
