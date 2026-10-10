// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Tests for the line/column/byte coordinate contract (CRLF, lone CR, multiline, astral).
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
using System.Xml;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

public class SourceCoordinateMapTests
{
    private static readonly Uri BaseUri = new("file:///C:/project/coords.xsl");

    private static SourceCoordinateMap CreateMap(string text, Encoding? encoding = null, int bias = 0)
        => new(text, encoding ?? new UTF8Encoding(false, true), bias);

    [Fact]
    public void LineCounting_CrlfAndLoneCrAndLf_EachBreakOneLine()
    {
        // \r\n, lone \r and lone \n each terminate exactly one line.
        var map = CreateMap("a\r\nb\rc\nd");

        Assert.Equal(4, map.LineCount);
        Assert.Equal(0, map.CharOffsetFromLineColumn(1, 1));
        Assert.Equal(3, map.CharOffsetFromLineColumn(2, 1));
        Assert.Equal(5, map.CharOffsetFromLineColumn(3, 1));
        Assert.Equal(7, map.CharOffsetFromLineColumn(4, 1));
        Assert.Equal((3, 1), map.LineColumnFromCharOffset(5));
        Assert.Equal((4, 1), map.LineColumnFromCharOffset(7));
    }

    [Fact]
    public void LineCounting_MatchesSystemXml_OnCrlfAndLoneCr()
    {
        // System.Xml (IXmlLineInfo / XmlException) treats \r\n and lone \r as single line breaks; the
        // map must agree. An XmlException pointing at a known position cross-checks the convention.
        // Physical lines: 1 "<a>", 2 "<b>", 3 "<c</a>" (the error is the second '<' in "<c<").
        var text = "<a>\r\n<b>\r<c</a>";
        var map = CreateMap(text);

        try
        {
            System.Xml.Linq.XDocument.Parse(text, System.Xml.Linq.LoadOptions.None);
            Assert.Fail("Expected XmlException.");
        }
        catch (XmlException ex)
        {
            Assert.Equal(3, ex.LineNumber);
            var errorOffset = map.CharOffsetFromLineColumn(ex.LineNumber, ex.LinePosition);
            Assert.Equal('<', text[errorOffset]);
        }

        // '<b>' sits after a CRLF break: physical line 2; the lone CR at offset 8 still belongs to
        // line 2, and the '<' of "<c" at offset 9 opens physical line 3.
        Assert.Equal((2, 1), map.LineColumnFromCharOffset(5));
        Assert.Equal(5, map.CharOffsetFromLineColumn(2, 1));
        Assert.Equal((2, 4), map.LineColumnFromCharOffset(8));
        Assert.Equal((3, 1), map.LineColumnFromCharOffset(9));
        Assert.Equal(9, map.CharOffsetFromLineColumn(3, 1));
    }

    [Fact]
    public void LineColumn_RoundTripsForEveryOffset()
    {
        var text = "one\r\ntwo\rthree\nfour";
        var map = CreateMap(text);

        for (var i = 0; i <= text.Length; i++)
        {
            var (line, column) = map.LineColumnFromCharOffset(i);
            Assert.Equal(i, map.CharOffsetFromLineColumn(line, column));
        }
    }

    [Fact]
    public void ByteOffsets_Utf8_AstralCharactersCountFourBytesButTwoUtf16Units()
    {
        // '<r a="😀">😀x</r>' — the emoji is one scalar, two UTF-16 code units, four UTF-8 bytes.
        const string text = "<r a=\"\U0001F600\">\U0001F600x</r>";
        var map = CreateMap(text);

        // Attribute value: UTF-16 columns 7..8 (one-past-the-last = 9), bytes 6..10.
        Assert.Equal((1, 7), map.LineColumnFromCharOffset(6));
        Assert.Equal((1, 9), map.LineColumnFromCharOffset(8));
        Assert.Equal(6, map.ByteOffsetFromCharOffset(6));
        Assert.Equal(4, map.ByteOffsetFromCharOffset(8) - map.ByteOffsetFromCharOffset(6));

        // Text '😀x': UTF-16 cols 11..13 (3 units), bytes 10..15 (5 bytes).
        Assert.Equal((1, 11), map.LineColumnFromCharOffset(10));
        Assert.Equal((1, 14), map.LineColumnFromCharOffset(13));
        Assert.Equal(5, map.ByteOffsetFromCharOffset(13) - map.ByteOffsetFromCharOffset(10));
    }

    [Fact]
    public void ByteOffsets_Utf16_UnitIsTwoBytes()
    {
        var encoding = new UnicodeEncoding(false, true, true);
        var text = "aéb";
        var map = CreateMap(text, encoding);

        Assert.Equal(0, map.ByteOffsetFromCharOffset(0));
        Assert.Equal(2, map.ByteOffsetFromCharOffset(1));
        Assert.Equal(4, map.ByteOffsetFromCharOffset(2));
        Assert.Equal(6, map.ByteOffsetFromCharOffset(3));
    }

    [Fact]
    public void ByteOffsets_BomBias_IsAccountedFor()
    {
        var text = "<r>hi</r>";
        var map = CreateMap(text, new UTF8Encoding(false, true), bias: 3);

        Assert.Equal(3, map.ByteOffsetFromCharOffset(0));
        Assert.Equal(12, map.ByteOffsetFromCharOffset(text.Length));
    }

    [Fact]
    public void Inspection_AstralAttribute_AndMultilineValue_HaveExactRanges()
    {
        const string text = "<r one=\"a\r\nb\" two=\"2\"/>";
        var bytes = Encoding.UTF8.GetBytes(text);
        Assert.True(AuthoringSource.TryCreate(bytes, BaseUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!);
        Assert.True(result.IsSuccess);
        var root = result.Snapshot!.PrincipalModule.Root;
        var element = root.FindDescendant(n => n.Kind == AuthoringNodeKind.Element)!;

        // Whole element: start tag on line 1 through the self-close on line 2 (23 bytes total).
        Assert.Equal(new SourceRange(1, 1, 2, 13, 0, 23), element.Range);

        var one = element.Attributes.Single(a => a.Name == "one");
        Assert.Equal("a\r\nb", one.RawLiteral);
        Assert.Equal(new SourceRange(1, 9, 2, 2, 8, 4), one.ValueRange);
        Assert.Equal(new SourceRange(1, 4, 2, 3, 3, 10), one.FullRange);

        var two = element.Attributes.Single(a => a.Name == "two");
        Assert.Equal("2", two.RawLiteral);
        Assert.Equal(new SourceRange(2, 9, 2, 10, 19, 1), two.ValueRange);
    }
}
