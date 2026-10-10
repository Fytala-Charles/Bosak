// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Source range coordinates (line/column primary, byte offsets derived) and the coordinate map.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — source-preserving authoring API for visual designers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;

namespace Bosak.Xslt.Authoring;

/// <summary>
/// Identifies a half-open source range within one module. Line and column are 1-based and counted in
/// UTF-16 code units, matching <see cref="System.Xml.IXmlLineInfo"/>. The end position is one-past-the-last
/// character. Byte offsets are derived deterministically through the retained source bytes and are
/// 0-based; <see cref="ByteLength"/> counts bytes, not characters.
/// </summary>
/// <param name="StartLine">1-based line of the first character.</param>
/// <param name="StartColumn">1-based UTF-16 column of the first character.</param>
/// <param name="EndLine">1-based line one-past-the-last character.</param>
/// <param name="EndColumn">1-based UTF-16 column one-past-the-last character.</param>
/// <param name="StartByteOffset">0-based byte offset of the first character in the original module bytes.</param>
/// <param name="ByteLength">Number of bytes covered by the range.</param>
public sealed record SourceRange(
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn,
    long StartByteOffset,
    long ByteLength)
{
    /// <inheritdoc />
    public override string ToString() =>
        $"({StartLine},{StartColumn})-({EndLine},{EndColumn}) bytes {StartByteOffset}+{ByteLength}";
}

/// <summary>
/// Maps between line/column positions (1-based, UTF-16 code units, matching
/// <see cref="System.Xml.IXmlLineInfo"/>), character offsets, and byte offsets for one decoded module.
/// Line breaks are counted exactly like System.Xml: <c>\r\n</c>, a lone <c>\r</c>, and a lone <c>\n</c>
/// each terminate exactly one line.
/// </summary>
public sealed class SourceCoordinateMap
{
    private readonly string _text;
    private readonly Encoding _encoding;
    private readonly int _byteOffsetBias;
    private readonly int[] _lineStarts;

    internal SourceCoordinateMap(string text, Encoding encoding, int byteOffsetBias)
    {
        _text = text;
        _encoding = encoding;
        _byteOffsetBias = byteOffsetBias;

        var lineStarts = new List<int> { 0 };
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '\r')
            {
                if (i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                lineStarts.Add(i + 1);
            }
            else if (c == '\n')
            {
                lineStarts.Add(i + 1);
            }
        }

        _lineStarts = lineStarts.ToArray();
    }

    /// <summary>Gets the total number of lines (including a final line without a trailing break).</summary>
    public int LineCount => _lineStarts.Length;

    /// <summary>Gets the decoded module text this map was built from.</summary>
    public string Text => _text;

    /// <summary>
    /// Converts a 1-based line/column position to an absolute character offset into the decoded text.
    /// </summary>
    /// <param name="line">1-based line number.</param>
    /// <param name="column">1-based UTF-16 column.</param>
    /// <returns>The absolute character offset (0-based).</returns>
    /// <exception cref="ArgumentOutOfRangeException">The line or column does not exist in the text.</exception>
    public int CharOffsetFromLineColumn(int line, int column)
    {
        if (line < 1 || line > _lineStarts.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(line), line, "Line is outside the module text.");
        }

        var offset = _lineStarts[line - 1] + (column - 1);
        if (column < 1 || offset > _text.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(column), column, "Column is outside the module text.");
        }

        return offset;
    }

    /// <summary>
    /// Converts an absolute character offset to a 1-based line/column position.
    /// </summary>
    /// <param name="charOffset">0-based absolute character offset.</param>
    /// <returns>The 1-based line and column (the column of the character at the offset).</returns>
    public (int Line, int Column) LineColumnFromCharOffset(int charOffset)
    {
        if (charOffset < 0 || charOffset > _text.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(charOffset), charOffset, "Offset is outside the module text.");
        }

        var lineIndex = Array.BinarySearch(_lineStarts, charOffset);
        if (lineIndex < 0)
        {
            lineIndex = ~lineIndex - 1;
        }

        return (lineIndex + 1, charOffset - _lineStarts[lineIndex] + 1);
    }

    /// <summary>
    /// Converts an absolute character offset to a byte offset in the original module bytes, through the
    /// module encoding. A leading byte-order mark, when present in the original bytes, is accounted for.
    /// </summary>
    /// <param name="charOffset">0-based absolute character offset into the decoded text.</param>
    /// <returns>The 0-based byte offset in the original module bytes.</returns>
    public long ByteOffsetFromCharOffset(int charOffset)
    {
        if (charOffset < 0 || charOffset > _text.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(charOffset), charOffset, "Offset is outside the module text.");
        }

        return _byteOffsetBias + _encoding.GetByteCount(_text.AsSpan(0, charOffset));
    }

    /// <summary>
    /// Builds a half-open <see cref="SourceRange"/> from inclusive character start and exclusive character end offsets.
    /// </summary>
    /// <param name="startCharOffset">0-based offset of the first character.</param>
    /// <param name="endCharOffsetExclusive">0-based offset one-past-the-last character.</param>
    /// <returns>The source range with line/column and byte coordinates.</returns>
    public SourceRange RangeFromCharOffsets(int startCharOffset, int endCharOffsetExclusive)
    {
        if (endCharOffsetExclusive < startCharOffset)
        {
            throw new ArgumentOutOfRangeException(nameof(endCharOffsetExclusive));
        }

        var (startLine, startColumn) = LineColumnFromCharOffset(startCharOffset);
        var (endLine, endColumn) = LineColumnFromCharOffset(endCharOffsetExclusive);
        var startByte = ByteOffsetFromCharOffset(startCharOffset);
        var endByte = ByteOffsetFromCharOffset(endCharOffsetExclusive);
        return new SourceRange(startLine, startColumn, endLine, endColumn, startByte, endByte - startByte);
    }
}
