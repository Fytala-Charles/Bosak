// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 October 2026
// PURPOSE              : Preserves XML serialization lexical conventions while forwarding bounded chunks.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-10-2026     | Creation: C-F1 incremental XML lexical filter and decoding bridge.                       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;
using System.Text;

namespace Bosak.Xslt.Runtime;

/// <summary>
/// Streaming equivalents of the legacy XML lexical fixups. Entity lookahead is capped at 32
/// characters; empty-tag lookahead retains two characters. No output-sized string is built.
/// </summary>
internal sealed class CaptureMarkupWriter(TextWriter target, bool xmlWriterFixes,
    Encoding? writerEncoding = null, bool convertHex = true) : TextWriter
{
    private readonly StringBuilder _entity = new(32);
    private string? _highEntity;
    private int _highCodepoint;
    private readonly StringBuilder _tail = new(3);
    private bool _inTag;
    private bool _afterEquals;
    private char _quote;

    /// <summary>Gets the destination encoding.</summary>
    public override Encoding Encoding => writerEncoding ?? target.Encoding;

    /// <summary>Writes a string through the incremental filter.</summary>
    /// <param name="value">The string.</param>
    public override void Write(string? value) => Write(value.AsSpan());

    /// <summary>Writes a character array segment.</summary>
    /// <param name="buffer">The source array.</param>
    /// <param name="index">The segment start.</param>
    /// <param name="count">The segment length.</param>
    public override void Write(char[] buffer, int index, int count) => Write(buffer.AsSpan(index, count));

    /// <summary>Writes a character span.</summary>
    /// <param name="buffer">The characters.</param>
    public override void Write(ReadOnlySpan<char> buffer)
    {
        foreach (char c in buffer) Write(c);
    }

    /// <summary>Writes one character with bounded lookahead.</summary>
    /// <param name="value">The character.</param>
    public override void Write(char value)
    {
        if (_entity.Length > 0)
        {
            _entity.Append(value);
            if (value == ';') FinishEntity();
            else if (_entity.Length == 32 || value == '&')
            {
                FlushHighEntity();
                string literal = _entity.ToString();
                _entity.Clear();
                if (value == '&')
                {
                    Emit(literal[..^1]);
                    _entity.Append('&');
                }
                else Emit(literal);
            }
            return;
        }
        if (value == '&')
        {
            _entity.Append(value);
            return;
        }
        FlushHighEntity();
        Emit(value);
    }

    private void FinishEntity()
    {
        string entity = _entity.ToString();
        _entity.Clear();
        bool hex = xmlWriterFixes && convertHex && entity.StartsWith("&#x", StringComparison.Ordinal);
        int prefix = hex ? 3 : 2;
        bool numeric = entity.StartsWith("&#", StringComparison.Ordinal)
            && int.TryParse(entity.AsSpan(prefix, entity.Length - prefix - 1),
                hex ? NumberStyles.HexNumber : NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        if (!numeric)
        {
            FlushHighEntity();
            Emit(entity);
            return;
        }
        int cp = int.Parse(entity.AsSpan(prefix, entity.Length - prefix - 1),
            hex ? NumberStyles.HexNumber : NumberStyles.Integer, CultureInfo.InvariantCulture);
        string normalized = hex ? $"&#{cp};" : entity;
        if (_highEntity is not null && cp is >= 0xDC00 and <= 0xDFFF)
        {
            int scalar = char.ConvertToUtf32((char)_highCodepoint, (char)cp);
            _highEntity = null;
            Emit($"&#{scalar};");
        }
        else
        {
            FlushHighEntity();
            if (cp is >= 0xD800 and <= 0xDBFF)
            {
                _highEntity = normalized;
                _highCodepoint = cp;
            }
            else Emit(normalized);
        }
    }

    private void FlushHighEntity()
    {
        if (_highEntity is { } entity)
        {
            _highEntity = null;
            Emit(entity);
        }
    }

    private void Emit(string value)
    {
        foreach (char c in value) Emit(c);
    }

    private void Emit(char value)
    {
        if (!xmlWriterFixes)
        {
            target.Write(value);
            return;
        }
        // Preserve the existing EscapeAttributeWhitespace conventions.
        if (_quote != '\0')
        {
            if (value == _quote) _quote = '\0';
            else if (value is '\t' or '\n' or '\r')
            {
                foreach (char c in value switch { '\t' => "&#9;", '\n' => "&#10;", _ => "&#13;" }) EmitTail(c);
                return;
            }
        }
        else if (_inTag)
        {
            if (_afterEquals && value is '\"' or '\'') { _quote = value; _afterEquals = false; }
            else if (value == '=') _afterEquals = true;
            else if (!char.IsWhiteSpace(value)) _afterEquals = false;
            if (value == '>') _inTag = false;
        }
        else if (value == '<') _inTag = true;
        EmitTail(value);
    }

    private void EmitTail(char value)
    {
        _tail.Append(value);
        if (_tail.Length == 3)
        {
            if (_tail[0] == ' ' && _tail[1] == '/' && _tail[2] == '>')
            {
                target.Write("/>");
                _tail.Clear();
            }
            else
            {
                target.Write(_tail[0]);
                _tail.Remove(0, 1);
            }
        }
    }

    internal void Complete()
    {
        FlushHighEntity();
        if (_entity.Length > 0) { Emit(_entity.ToString()); _entity.Clear(); }
        target.Write(_tail.ToString());
        _tail.Clear();
    }
}

/// <summary>Decodes XmlWriter's fixed-size byte chunks directly into a capture text writer.</summary>
internal sealed class CaptureDecodingStream(Encoding encoding, TextWriter target) : Stream
{
    private readonly Decoder _decoder = encoding.GetDecoder();
    private readonly char[] _chars = new char[1024];
    /// <summary>Gets whether reading is supported.</summary>
    public override bool CanRead => false;
    /// <summary>Gets whether seeking is supported.</summary>
    public override bool CanSeek => false;
    /// <summary>Gets whether writing is supported.</summary>
    public override bool CanWrite => true;
    /// <summary>Length is unsupported.</summary>
    public override long Length => throw new NotSupportedException();
    /// <summary>Position is unsupported.</summary>
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    /// <summary>Flushes the destination.</summary>
    public override void Flush() => target.Flush();
    /// <summary>Reading is unsupported.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="count">The count.</param>
    /// <returns>No value; throws.</returns>
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    /// <summary>Seeking is unsupported.</summary>
    /// <param name="offset">The offset.</param>
    /// <param name="origin">The origin.</param>
    /// <returns>No value; throws.</returns>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    /// <summary>Changing length is unsupported.</summary>
    /// <param name="value">The length.</param>
    public override void SetLength(long value) => throw new NotSupportedException();
    /// <summary>Decodes and forwards a byte segment.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="count">The count.</param>
    public override void Write(byte[] buffer, int offset, int count) => Decode(buffer.AsSpan(offset, count), false);
    /// <summary>Decodes and forwards a byte span.</summary>
    /// <param name="buffer">The bytes.</param>
    public override void Write(ReadOnlySpan<byte> buffer) => Decode(buffer, false);
    private void Decode(ReadOnlySpan<byte> bytes, bool flush)
    {
        bool completed;
        do
        {
            _decoder.Convert(bytes, _chars, flush, out int used, out int produced, out completed);
            target.Write(_chars.AsSpan(0, produced));
            bytes = bytes[used..];
        } while (!completed);
    }
    internal void Complete() => Decode(ReadOnlySpan<byte>.Empty, true);
}
