// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Immutable retained-source envelope: original bytes, detected encoding, base URI, decoded text.
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
//                      | Charles Korthout | 0.2   | 10-10-2026     | REQ-124 Slice B: strict-decode and copy helpers for emitted candidate bytes              |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;

namespace Bosak.Xslt.Authoring;

/// <summary>
/// An immutable, engine-owned envelope around one original module. The original bytes are the only
/// emission source for untouched regions; the decoded text is derived working state. Encoding detection
/// order: a byte-order mark (UTF-8 / UTF-16 LE / UTF-16 BE / UTF-32 LE / UTF-32 BE), else the
/// <c>encoding</c> pseudo-attribute of an XML declaration in the first ~256 bytes, else UTF-8.
/// Supported encodings: the four UTF variants, ISO-8859-1, US-ASCII and UTF-8. Any other encoding name
/// is refused explicitly with <see cref="AuthoringFailureKind.UnsupportedEncoding"/>; invalid byte
/// sequences are refused with <see cref="AuthoringFailureKind.InvalidSourceBytes"/> — never silently
/// replaced.
/// </summary>
public sealed class AuthoringSource
{
    private readonly byte[] _originalBytes;
    private readonly Lazy<string> _text;
    private readonly Lazy<SourceCoordinateMap> _coordinateMap;

    private AuthoringSource(byte[] originalBytes, Encoding encoding, Uri baseUri, int byteOffsetBias)
    {
        _originalBytes = originalBytes;
        Encoding = encoding;
        BaseUri = baseUri;
        ByteOffsetBias = byteOffsetBias;
        _text = new Lazy<string>(() => DecodeStrict(originalBytes, encoding, byteOffsetBias));
        _coordinateMap = new Lazy<SourceCoordinateMap>(() => new SourceCoordinateMap(Text, encoding, byteOffsetBias));
    }

    /// <summary>Gets the exact original module bytes, including any byte-order mark.</summary>
    public ReadOnlyMemory<byte> OriginalBytes => _originalBytes;

    /// <summary>Gets the detected (or defaulted) encoding used to decode the module.</summary>
    public Encoding Encoding { get; }

    /// <summary>Gets the absolute base URI of the module.</summary>
    public Uri BaseUri { get; }

    /// <summary>Gets the length of the detected byte-order mark, in bytes; zero when there is no BOM.</summary>
    internal int ByteOffsetBias { get; }

    /// <summary>
    /// Gets the decoded module text. Decoding is strict: invalid byte sequences for the detected
    /// encoding raise <see cref="InvalidDataException"/>. Successful <see cref="TryCreate"/> validation
    /// guarantees the text decodes, so this property does not throw for sources obtained through it.
    /// </summary>
    public string Text => _text.Value;

    /// <summary>Gets the coordinate map over the decoded text for this module.</summary>
    internal SourceCoordinateMap CoordinateMap => _coordinateMap.Value;

    /// <summary>
    /// Copies emitted candidate bytes into an independent array so a candidate is not aliased to
    /// shared buffers.
    /// </summary>
    internal byte[] EmittedBytes(ReadOnlyMemory<byte> emitted) => emitted.ToArray();

    /// <summary>
    /// Decodes bytes with this module's detected encoding under strict fallback: invalid byte
    /// sequences raise <see cref="DecoderFallbackException"/> instead of being replaced. The byte
    /// offset bias (byte-order mark) of this envelope is honored.
    /// </summary>
    internal string StrictDecode(byte[] bytes) => DecodeStrict(bytes, Encoding, ByteOffsetBias);

    /// <summary>
    /// Attempts to create a retained-source envelope from original module bytes.
    /// </summary>
    /// <param name="bytes">The exact original module bytes, including any byte-order mark.</param>
    /// <param name="baseUri">The absolute base URI of the module; used for include/import resolution and xml:base chains.</param>
    /// <param name="source">The created envelope, or <see langword="null"/> on failure.</param>
    /// <param name="failure">The classified failure, or <see langword="null"/> on success.</param>
    /// <returns><see langword="true"/> when the envelope was created; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bytes"/> or <paramref name="baseUri"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="baseUri"/> is not absolute.</exception>
    public static bool TryCreate(byte[] bytes, Uri baseUri, out AuthoringSource? source, out AuthoringFailure? failure)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        ArgumentNullException.ThrowIfNull(baseUri);
        if (!baseUri.IsAbsoluteUri)
        {
            throw new ArgumentException("The module base URI must be absolute.", nameof(baseUri));
        }

        source = null;
        failure = null;

        if (!TryDetectEncoding(bytes, baseUri, out var encoding, out var byteOffsetBias, out var detectFailure))
        {
            failure = detectFailure;
            return false;
        }

        // Validate strictly up-front so failures surface here — as data — rather than from Text.
        try
        {
            _ = DecodeStrict(bytes, encoding, byteOffsetBias);
        }
        catch (Exception ex) when (ex is DecoderFallbackException or ArgumentException or InvalidDataException)
        {
            failure = new AuthoringFailure(
                AuthoringFailureKind.InvalidSourceBytes,
                $"The module bytes are not valid {encoding.EncodingName}: {ex.Message}",
                baseUri);
            return false;
        }

        source = new AuthoringSource(bytes, encoding, baseUri, byteOffsetBias);
        return true;
    }

    private static string DecodeStrict(byte[] bytes, Encoding encoding, int byteOffsetBias)
    {
        // Re-create with throwing fallbacks so direct construction paths stay strict too. Both
        // UnicodeEncoding and UTF32Encoding cover both endiannesses, so discriminate by code page.
        var strict = encoding.CodePage switch
        {
            65001 => new UTF8Encoding(false, true),
            1200 => new UnicodeEncoding(false, true, true),
            1201 => new UnicodeEncoding(true, true, true),
            12000 => new UTF32Encoding(false, true, true),
            12001 => new UTF32Encoding(true, true, true),
            _ => Encoding.GetEncoding(encoding.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback),
        };

        return strict.GetString(bytes, byteOffsetBias, bytes.Length - byteOffsetBias);
    }

    private static bool TryDetectEncoding(
        byte[] bytes,
        Uri baseUri,
        out Encoding encoding,
        out int byteOffsetBias,
        out AuthoringFailure? failure)
    {
        encoding = Encoding.UTF8;
        byteOffsetBias = 0;
        failure = null;

        if (TryDetectBom(bytes, out var bomEncoding, out var bomLength))
        {
            encoding = bomEncoding;
            byteOffsetBias = bomLength;
            return true;
        }

        var declaredName = ScanXmlDeclarationEncoding(bytes);
        if (declaredName is null)
        {
            encoding = new UTF8Encoding(false, true);
            return true;
        }

        if (!TryResolveEncodingName(declaredName, out var resolved))
        {
            failure = new AuthoringFailure(
                AuthoringFailureKind.UnsupportedEncoding,
                $"The module declares encoding \"{declaredName}\", which is not supported. " +
                "Supported encodings: UTF-8, UTF-16, UTF-32 (LE/BE), ISO-8859-1 and US-ASCII.",
                baseUri);
            return false;
        }

        encoding = resolved;
        return true;
    }

    private static bool TryDetectBom(byte[] bytes, out Encoding encoding, out int byteOffsetBias)
    {
        encoding = Encoding.UTF8;
        byteOffsetBias = 0;

        if (bytes.Length >= 4)
        {
            if (bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            {
                encoding = new UTF32Encoding(false, true, true);
                byteOffsetBias = 4;
                return true;
            }

            if (bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            {
                encoding = new UTF32Encoding(true, true, true);
                byteOffsetBias = 4;
                return true;
            }
        }

        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            encoding = new UTF8Encoding(false, true);
            byteOffsetBias = 3;
            return true;
        }

        if (bytes.Length >= 2)
        {
            if (bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                encoding = new UnicodeEncoding(false, true, true);
                byteOffsetBias = 2;
                return true;
            }

            if (bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                encoding = new UnicodeEncoding(true, true, true);
                byteOffsetBias = 2;
                return true;
            }
        }

        return false;
    }

    private static string? ScanXmlDeclarationEncoding(byte[] bytes)
    {
        const int scanLimit = 256;
        var length = Math.Min(bytes.Length, scanLimit);
        var prefix = new char[length];
        for (var i = 0; i < length; i++)
        {
            var b = bytes[i];
            prefix[i] = b < 0x80 ? (char)b : '?';
        }

        var text = new string(prefix);

        // The declaration is only honored when it is actually an XML declaration at offset 0.
        if (!text.StartsWith("<?xml", StringComparison.Ordinal))
        {
            return null;
        }

        var index = text.IndexOf("encoding", StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        index += "encoding".Length;
        while (index < text.Length && char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        if (index >= text.Length || text[index] != '=')
        {
            return null;
        }

        index++;
        while (index < text.Length && char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        if (index >= text.Length || (text[index] != '"' && text[index] != '\''))
        {
            return null;
        }

        var quote = text[index];
        var valueStart = index + 1;
        var valueEnd = text.IndexOf(quote, valueStart);
        if (valueEnd < 0)
        {
            return null;
        }

        return text[valueStart..valueEnd];
    }

    private static bool TryResolveEncodingName(string name, out Encoding encoding)
    {
        var normalized = name.Trim().ToLowerInvariant();
        encoding = normalized switch
        {
            "utf-8" or "utf8" => new UTF8Encoding(false, true),
            "utf-16" or "utf16" or "utf-16le" or "utf16le" or "unicode" => new UnicodeEncoding(false, true, true),
            "utf-16be" or "utf16be" => new UnicodeEncoding(true, true, true),
            "utf-32" or "utf32" or "utf-32le" or "utf32le" => new UTF32Encoding(false, true, true),
            "utf-32be" or "utf32be" => new UTF32Encoding(true, true, true),
            "iso-8859-1" or "iso8859-1" or "iso88591" or "latin1" or "latin-1" or "l1" => Encoding.Latin1,
            "us-ascii" or "usascii" or "ascii" or "ansi_x3.4-1968" => Encoding.GetEncoding("us-ascii", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback),
            _ => null!,
        };

        return encoding is not null;
    }
}
