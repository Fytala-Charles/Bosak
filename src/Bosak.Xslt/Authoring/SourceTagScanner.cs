// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Raw-source scanner computing full element extents and attribute value ranges.
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

namespace Bosak.Xslt.Authoring;

/// <summary>
/// One attribute occurrence scanned from raw source. All offsets are absolute character offsets into
/// the decoded module text; <see cref="FullEnd"/> is exclusive and includes the closing quote, while
/// <see cref="ValueEnd"/> is exclusive and sits between the quotes.
/// </summary>
internal sealed record ScannedAttribute(
    int NameStart,
    int NameEnd,
    int ValueStart,
    int ValueEnd,
    int FullEnd,
    char Quote);

/// <summary>
/// Scans raw decoded source text to compute extents that <see cref="System.Xml.IXmlLineInfo"/> cannot
/// provide: the end of a start tag (with <c>&gt;</c> inside quoted attribute values skipped), whether an
/// element is self-closed, the matching close tag (with depth counting that skips comments, PIs, CDATA
/// and nested elements), and per-attribute name/value/full ranges with exact raw spelling. Works purely
/// on the retained text; entity references are ordinary characters to this scanner, so raw spelling is
/// preserved.
/// </summary>
internal static class SourceTagScanner
{
    /// <summary>
    /// Finds the exclusive end offset of the element whose start tag begins at <paramref name="startOffset"/>.
    /// </summary>
    /// <param name="text">The decoded module text.</param>
    /// <param name="startOffset">Offset of the <c>&lt;</c> of the element's start tag.</param>
    /// <param name="startTagEnd">Receives the exclusive offset just past the start tag's closing <c>&gt;</c>.</param>
    /// <param name="selfClosed"><see langword="true"/> when the element uses empty-element syntax.</param>
    /// <returns>The exclusive offset just past the element's end (the <c>&gt;</c> of the close tag, or of the self-close).</returns>
    public static int FindElementEnd(string text, int startOffset, out int startTagEnd, out bool selfClosed)
    {
        var i = startOffset + 1;
        i = SkipName(text, i);
        i = ScanTagInterior(text, i, out selfClosed);
        startTagEnd = i;
        if (selfClosed)
        {
            return i;
        }

        var depth = 1;
        i++; // past the start tag's '>'
        while (i < text.Length)
        {
            if (text[i] != '<')
            {
                i++;
                continue;
            }

            if (i + 3 < text.Length && text[i + 1] == '!' && text[i + 2] == '-' && text[i + 3] == '-')
            {
                i = SkipUntil(text, i + 4, "-->");
            }
            else if (i + 8 < text.Length && text.AsSpan(i, 9).SequenceEqual("<![CDATA["))
            {
                i = SkipUntil(text, i + 9, "]]>");
            }
            else if (i + 1 < text.Length && text[i + 1] == '?')
            {
                i = SkipUntil(text, i + 2, "?>");
            }
            else if (i + 1 < text.Length && text[i + 1] == '!')
            {
                i = SkipUntil(text, i + 2, ">");
            }
            else if (i + 1 < text.Length && text[i + 1] == '/')
            {
                depth--;
                i = SkipUntil(text, i + 2, ">");
                if (depth == 0)
                {
                    return i;
                }
            }
            else
            {
                var j = SkipName(text, i + 1);
                j = ScanTagInterior(text, j, out var nestedSelfClosed);
                if (!nestedSelfClosed)
                {
                    depth++;
                }

                i = j;
            }
        }

        throw new InvalidDataException("Unterminated element in module source.");
    }

    /// <summary>
    /// Scans the attributes of the start tag between <paramref name="elementStartOffset"/> and
    /// <paramref name="startTagEnd"/> (both obtained from <see cref="FindElementEnd"/>).
    /// </summary>
    /// <param name="text">The decoded module text.</param>
    /// <param name="elementStartOffset">Offset of the <c>&lt;</c> of the element's start tag.</param>
    /// <param name="startTagEnd">Exclusive offset just past the start tag's closing <c>&gt;</c> (or self-close).</param>
    /// <returns>The scanned attributes in source order.</returns>
    public static List<ScannedAttribute> ScanAttributes(string text, int elementStartOffset, int startTagEnd)
    {
        var attributes = new List<ScannedAttribute>();
        var i = SkipName(text, elementStartOffset + 1);
        while (i < startTagEnd)
        {
            while (i < startTagEnd && char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            if (i >= startTagEnd || text[i] == '>' || (text[i] == '/' && i + 1 < startTagEnd && text[i + 1] == '>'))
            {
                break;
            }

            var nameStart = i;
            while (i < startTagEnd && !char.IsWhiteSpace(text[i]) && text[i] != '=' && text[i] != '>' && text[i] != '/')
            {
                i++;
            }

            var nameEnd = i;
            while (i < startTagEnd && char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            if (i >= startTagEnd || text[i] != '=')
            {
                // Malformed; stop scanning attributes but never throw from inspection paths.
                break;
            }

            i++;
            while (i < startTagEnd && char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            if (i >= startTagEnd || (text[i] != '"' && text[i] != '\''))
            {
                break;
            }

            var quote = text[i];
            var valueStart = i + 1;
            var valueEnd = text.IndexOf(quote, valueStart);
            if (valueEnd < 0 || valueEnd >= startTagEnd)
            {
                break;
            }

            attributes.Add(new ScannedAttribute(nameStart, nameEnd, valueStart, valueEnd, valueEnd + 1, quote));
            i = valueEnd + 1;
        }

        return attributes;
    }

    /// <summary>
    /// Reads the element name token as written in source (including any prefix), starting after the <c>&lt;</c>.
    /// </summary>
    /// <param name="text">The decoded module text.</param>
    /// <param name="startOffset">Offset of the <c>&lt;</c> of the element's start tag.</param>
    /// <returns>The element name exactly as written, e.g. <c>xsl:stylesheet</c>.</returns>
    public static string ReadElementName(string text, int startOffset)
    {
        var end = SkipName(text, startOffset + 1);
        return text.Substring(startOffset + 1, end - startOffset - 1);
    }

    private static int SkipName(string text, int i)
    {
        while (i < text.Length)
        {
            var c = text[i];
            if (char.IsWhiteSpace(c) || c == '>' || c == '/' || c == '=')
            {
                break;
            }

            i++;
        }

        return i;
    }

    // Scans from just after a tag's name to the closing '>'; quoted sections are skipped so a '>' inside
    // an attribute value is not mistaken for the tag end. Returns the offset just past the '>'.
    private static int ScanTagInterior(string text, int i, out bool selfClosed)
    {
        selfClosed = false;
        while (i < text.Length)
        {
            var c = text[i];
            if (c == '>')
            {
                return i + 1;
            }

            if (c == '"' || c == '\'')
            {
                var end = text.IndexOf(c, i + 1);
                if (end < 0)
                {
                    throw new InvalidDataException("Unterminated attribute value in module source.");
                }

                i = end + 1;
                continue;
            }

            if (c == '/' && i + 1 < text.Length && text[i + 1] == '>')
            {
                selfClosed = true;
                return i + 2;
            }

            i++;
        }

        throw new InvalidDataException("Unterminated tag in module source.");
    }

    private static int SkipUntil(string text, int i, string terminator)
    {
        var end = text.IndexOf(terminator, i, StringComparison.Ordinal);
        if (end < 0)
        {
            throw new InvalidDataException($"Unterminated markup; expected \"{terminator}\".");
        }

        return end + terminator.Length;
    }
}
