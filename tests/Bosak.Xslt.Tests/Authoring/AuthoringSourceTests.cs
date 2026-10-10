// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Tests for the retained-source envelope and its encoding detection / strict decoding.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
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
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

public class AuthoringSourceTests
{
    private static readonly Uri BaseUri = new("file:///C:/project/main.xsl");

    [Fact]
    public void TryCreate_Utf8Bom_DetectedAndBomExcludedFromText()
    {
        var bytes = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes("<r>hi</r>")).ToArray();

        var ok = AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure);

        Assert.True(ok);
        Assert.Null(failure);
        Assert.Equal("<r>hi</r>", source!.Text);
        Assert.Equal(BaseUri, source.BaseUri);
        Assert.Equal(3, source.OriginalBytes.Length - Encoding.UTF8.GetByteCount(source.Text));
    }

    [Fact]
    public void TryCreate_BomVariants_Detected()
    {
        var cases = new (byte[] Bom, int ExpectedCodePage, byte[] Payload)[]
        {
            (new byte[] { 0xFF, 0xFE }, 1200, EncodeUtf16("<r>hi</r>", bigEndian: false)),
            (new byte[] { 0xFE, 0xFF }, 1201, EncodeUtf16("<r>hi</r>", bigEndian: true)),
            (new byte[] { 0xFF, 0xFE, 0x00, 0x00 }, 12000, EncodeUtf32("<r>hi</r>", bigEndian: false)),
            (new byte[] { 0x00, 0x00, 0xFE, 0xFF }, 12001, EncodeUtf32("<r>hi</r>", bigEndian: true)),
        };

        foreach (var (bom, expectedCodePage, payload) in cases)
        {
            var bytes = bom.Concat(payload).ToArray();

            var ok = AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure);

            Assert.True(ok);
            Assert.Null(failure);
            Assert.Equal(expectedCodePage, source!.Encoding.CodePage);
            Assert.Equal("<r>hi</r>", source.Text);
        }
    }

    [Fact]
    public void TryCreate_Iso8859Declaration_Decoded()
    {
        // é as single byte 0xE9, declared via XML declaration (no BOM).
        var bytes = new byte[]
        {
            0x3C, 0x72, 0x3E, 0xE9, 0x3C, 0x2F, 0x72, 0x3E,
        };
        var withDecl = Encoding.ASCII.GetBytes("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>").Concat(bytes).ToArray();

        var ok = AuthoringSource.TryCreate(withDecl, BaseUri, out var source, out var failure);

        Assert.True(ok);
        Assert.Null(failure);
        Assert.Equal("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><r>é</r>", source!.Text);
    }

    [Fact]
    public void TryCreate_UsAsciiDeclaration_Decoded()
    {
        var bytes = Encoding.ASCII.GetBytes("<?xml version=\"1.0\" encoding=\"us-ascii\"?><r>plain</r>");

        var ok = AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure);

        Assert.True(ok);
        Assert.Null(failure);
        Assert.Equal("<?xml version=\"1.0\" encoding=\"us-ascii\"?><r>plain</r>", source!.Text);
    }

    [Fact]
    public void TryCreate_NoDeclaration_DefaultsToUtf8()
    {
        var bytes = Encoding.UTF8.GetBytes("<r>hi</r>");

        var ok = AuthoringSource.TryCreate(bytes, BaseUri, out var source, out _);

        Assert.True(ok);
        Assert.Contains("UTF-8", source!.Encoding.EncodingName, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("<r>hi</r>", source.Text);
    }

    [Fact]
    public void TryCreate_UnsupportedEncodingName_RefusedExplicitly()
    {
        var bytes = Encoding.ASCII.GetBytes("<?xml version=\"1.0\" encoding=\"x-bogus-1\"?><r/>");

        var ok = AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure);

        Assert.False(ok);
        Assert.Null(source);
        Assert.NotNull(failure);
        Assert.Equal(AuthoringFailureKind.UnsupportedEncoding, failure!.Kind);
        Assert.Contains("x-bogus-1", failure.Message, StringComparison.Ordinal);
        Assert.Equal(BaseUri, failure.ModuleUri);
    }

    [Fact]
    public void TryCreate_InvalidUtf8Bytes_RefusedAsInvalidSourceBytes()
    {
        // 0xC3 0x28 is an invalid UTF-8 sequence (never silently replaced).
        var bytes = new byte[] { 0x3C, 0x72, 0x3E, 0xC3, 0x28, 0x3C, 0x2F, 0x72, 0x3E };

        var ok = AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure);

        Assert.False(ok);
        Assert.Null(source);
        Assert.NotNull(failure);
        Assert.Equal(AuthoringFailureKind.InvalidSourceBytes, failure!.Kind);
    }

    [Fact]
    public void TryCreate_RelativeBaseUri_ThrowsArgumentException()
    {
        var bytes = Encoding.UTF8.GetBytes("<r/>");

        Assert.Throws<ArgumentException>(() =>
            AuthoringSource.TryCreate(bytes, new Uri("relative/main.xsl", UriKind.Relative), out _, out _));
    }

    [Fact]
    public void OriginalBytes_AreTheExactInputBytes()
    {
        var bytes = new byte[] { 0xEF, 0xBB, 0xBF, 0x3C, 0x72, 0x2F, 0x3E };

        var ok = AuthoringSource.TryCreate(bytes, BaseUri, out var source, out _);

        Assert.True(ok);
        Assert.True(bytes.SequenceEqual(source!.OriginalBytes.ToArray()));
    }

    private static byte[] EncodeUtf16(string text, bool bigEndian)
    {
        var encoding = new UnicodeEncoding(bigEndian, false);
        return encoding.GetBytes(text);
    }

    private static byte[] EncodeUtf32(string text, bool bigEndian)
    {
        var encoding = new UTF32Encoding(bigEndian, false);
        return encoding.GetBytes(text);
    }
}
