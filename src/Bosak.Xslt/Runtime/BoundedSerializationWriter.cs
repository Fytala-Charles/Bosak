// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 October 2026
// PURPOSE              : Encodes capture serialization incrementally under byte budgets.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-10-2026     | Creation: C-F1 bounded encoding sink.                                                    |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;

namespace Bosak.Xslt.Runtime;

/// <summary>Signals the first byte that cannot be produced within an output budget.</summary>
internal sealed class SerializationByteLimitException(bool perOutput, long limit) : Exception
{
    internal bool PerOutput { get; } = perOutput;
    internal long Limit { get; } = limit;
}

/// <summary>
/// Incrementally encodes characters without retaining a serialized string. The encoding scratch
/// buffer is fixed-size; at a boundary it permits one complete encoding scalar as lookahead.
/// Only accepted bytes reach the retained stream. An encoder preserves split surrogate pairs.
/// </summary>
internal sealed class BoundedSerializationWriter : TextWriter
{
    private readonly MemoryStream _bytes = new();
    private readonly Encoder _encoder;
    private readonly byte[] _scratch = new byte[1024];
    private readonly long? _perOutput;
    private readonly long? _aggregate;
    private readonly Action? _observeControls;
    private readonly Action<long>? _observeProduction;
    private bool _firstCharacter = true;
    private long _produced;

    internal BoundedSerializationWriter(Encoding encoding, bool bom, long? perOutput,
        long? aggregate, Action? observeControls, Action<long>? observeProduction)
    {
        Encoding = encoding;
        _encoder = encoding.GetEncoder();
        _perOutput = perOutput;
        _aggregate = aggregate;
        _observeControls = observeControls;
        _observeProduction = observeProduction;
        if (bom)
            Accept(encoding.GetPreamble());
    }

    /// <summary>Gets the actual output encoding.</summary>
    public override Encoding Encoding { get; }

    /// <summary>Writes one character, preserving encoder state across calls.</summary>
    /// <param name="value">The character to encode.</param>
    public override void Write(char value)
    {
        Span<char> one = stackalloc char[1];
        one[0] = value;
        Write(one);
    }

    /// <summary>Writes a string in bounded chunks.</summary>
    /// <param name="value">The string to encode.</param>
    public override void Write(string? value) => Write(value.AsSpan());

    /// <summary>Writes a character array segment in bounded chunks.</summary>
    /// <param name="buffer">The character array.</param>
    /// <param name="index">The segment start.</param>
    /// <param name="count">The segment length.</param>
    public override void Write(char[] buffer, int index, int count) => Write(buffer.AsSpan(index, count));

    /// <summary>Writes a span without allocating a copy of it.</summary>
    /// <param name="buffer">The characters to encode.</param>
    public override void Write(ReadOnlySpan<char> buffer)
    {
        if (_firstCharacter && !buffer.IsEmpty)
        {
            _firstCharacter = false;
            // Legacy serializers represent their preamble as an initial U+FEFF. The actual
            // preamble was accounted above, so consume that marker exactly once.
            if (buffer[0] == '\uFEFF')
                buffer = buffer[1..];
        }
        Encode(buffer, flush: false);
    }

    private void Encode(ReadOnlySpan<char> chars, bool flush)
    {
        bool completed;
        do
        {
            _observeControls?.Invoke();
            long remaining = Math.Min(_perOutput ?? long.MaxValue, _aggregate ?? long.MaxValue) - _bytes.Length;
            // Encoder.Convert requires room for a complete scalar. Use at least the encoding's
            // maximum scalar size, still constant scratch space, then reject before retention.
            int capacity = (int)Math.Min(_scratch.Length,
                Math.Max(Encoding.GetMaxByteCount(2), remaining >= _scratch.Length ? _scratch.Length : remaining + 1));
            _encoder.Convert(chars, _scratch.AsSpan(0, capacity), flush,
                out int charsUsed, out int bytesUsed, out completed);
            Accept(_scratch.AsSpan(0, bytesUsed));
            chars = chars[charsUsed..];
        } while (!completed);
    }

    private void Accept(ReadOnlySpan<byte> bytes)
    {
        // Count actual encoded bytes, including the constant-size rejected encoding chunk.
        // This is production evidence: no later serialization or encoding is performed.
        long limit = Math.Min(_perOutput ?? long.MaxValue, _aggregate ?? long.MaxValue);
        long remaining = limit - _bytes.Length;
        int accepted = (int)Math.Min(bytes.Length, Math.Max(0, remaining));
        if (accepted > 0)
            _bytes.Write(bytes[..accepted]);
        _produced += bytes.Length;
        _observeProduction?.Invoke(_produced);
        if (bytes.Length > accepted)
        {
            bool perOutput = _perOutput.HasValue && (!_aggregate.HasValue || _perOutput.Value <= _aggregate.Value);
            throw new SerializationByteLimitException(perOutput, limit);
        }
        _observeControls?.Invoke();
    }

    internal byte[] Complete()
    {
        Encode(ReadOnlySpan<char>.Empty, flush: true);
        _observeControls?.Invoke();
        return _bytes.ToArray();
    }

    /// <summary>Releases the retained byte stream.</summary>
    /// <param name="disposing">Whether managed resources should be released.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing) _bytes.Dispose();
        base.Dispose(disposing);
    }
}
