// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 12 October 2026
// PURPOSE              : Verify compact source containment, exact byte ranges and isolated sibling expression edits.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 12-10-2026     | Creation — REQ-127 compact sibling inspection and edit regressions                         |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>Source fidelity regressions for REQ-127, independent of compilation and external acquisition.</summary>
public class CompactSourceInspectionTests
{
    private static readonly Uri BaseUri = new("file:///req127/compact.xsl");
    private const string Header = "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"3.0\"><xsl:template match=\"/\"><result>";
    private const string Footer = "</result></xsl:template></xsl:stylesheet>";

    /// <summary>Provides compact and indented sibling counts, separators and supported Unicode encodings.</summary>
    /// <returns>The 192 source-layout/encoding combinations.</returns>
    public static IEnumerable<object[]> Layouts()
    {
        foreach (var count in new[] { 1, 2, 80 })
        foreach (var separator in new[] { "", " ", "\n", "\r\n" })
        foreach (var interior in new[] { "", " ", "\n", "\r\n" })
        foreach (var format in new[] { "utf8", "utf8bom", "utf16le", "utf16be" })
            yield return new object[] { count, separator, interior, format };
    }

    /// <summary>Every sibling and nested element retains its own exact lexical extent and language slot.</summary>
    /// <param name="count">The number of sibling groups.</param>
    /// <param name="separator">Whitespace between sibling groups.</param>
    /// <param name="interior">Whitespace between nested tags.</param>
    /// <param name="format">The encoding and BOM policy.</param>
    [Theory]
    [MemberData(nameof(Layouts))]
    public void Siblings_InspectWithExactContainmentAndCoordinates(int count, string separator, string interior, string format)
    {
        var groups = Enumerable.Range(0, count).Select(i => Group(i, interior)).ToArray();
        var text = Declaration(format) + Header + string.Join(separator, groups) + Footer;
        var encoding = EncodingFor(format);
        var snapshot = Inspect(text, encoding, out var bytes);
        var nodes = Flatten(snapshot.PrincipalModule.Root).ToArray();
        Assert.Equal(nodes.Length, nodes.Select(n => n.Id).Distinct().Count());
        Assert.Equal(BaseUri, snapshot.PrincipalModule.ModuleUri);
        Assert.Equal(bytes, snapshot.ExportOriginal(BaseUri));
        Assert.Equal(bytes.Length, snapshot.PrincipalModule.OriginalByteLength);
        Assert.False(snapshot.IsCompilable);
        var result = nodes.Single(n => n.ElementName?.LocalName == "result");
        var descriptors = result.Children.Where(n => n.Kind == AuthoringNodeKind.Element).ToArray();
        Assert.Equal(count, descriptors.Length);
        for (var i = 0; i < count; i++)
        {
            var group = descriptors[i];
            Assert.Equal("group", group.ElementName!.LocalName);
            Assert.Equal(i.ToString(), group.Attributes.Single(a => a.Name == "id").ExpandedValue);
            AssertExtent(group.Range, groups[i], text, encoding, bytes);
            var nested = group.Children.Single(n => n.Kind == AuthoringNodeKind.Element);
            Assert.Equal("nested", nested.ElementName!.LocalName);
            AssertExtent(nested.Range, "<nested>" + interior + "<xsl:value-of select=\"/input/name\"/>" + interior + "</nested>", text, encoding, bytes, (int)group.Range.StartByteOffset);
            var valueOf = nested.Children.Single(n => n.Kind == AuthoringNodeKind.Element);
            Assert.Equal("value-of", valueOf.ElementName!.LocalName);
            AssertExtent(valueOf.Range, "<xsl:value-of select=\"/input/name\"/>", text, encoding, bytes, (int)nested.Range.StartByteOffset);
            var select = valueOf.Attributes.Single(a => a.Name == "select");
            Assert.Equal(AuthoringAttributeSlotKind.Expression, select.SlotKind);
            Assert.Equal("/input/name", select.RawLiteral);
            Assert.Equal("/input/name", select.ExpandedValue);
            AssertExtent(select.ValueRange, "/input/name", text, encoding, bytes, (int)valueOf.Range.StartByteOffset);
        }
        AssertContained(snapshot.PrincipalModule.Root);
    }

    /// <summary>Mixed immediate content, alias names and entities preserve all lexical nodes.</summary>
    /// <param name="format">The encoding and BOM policy.</param>
    [Theory]
    [InlineData("utf8")]
    [InlineData("utf8bom")]
    [InlineData("utf16le")]
    [InlineData("utf16be")]
    public void MixedContent_PreservesEntitiesCommentsPiCdataAndAliases(string format)
    {
        const string content = "<!-- <fake/> --><?review compact?><![CDATA[<not-a-tag/>]]>é &amp; text<group><nested><t:value-of select=\"'A &amp; B'\"/></nested><empty/><empty></empty></group><group><nested/></group>";
        var text = Declaration(format) + Header.Replace("xmlns:xsl=", "xmlns:t=\"http://www.w3.org/1999/XSL/Transform\" xmlns:xsl=") + content + Footer;
        var encoding = EncodingFor(format);
        var snapshot = Inspect(text, encoding, out var bytes);
        Assert.Equal(bytes, snapshot.ExportOriginal(BaseUri));
        var nodes = Flatten(snapshot.PrincipalModule.Root).ToArray();
        var result = nodes.Single(n => n.ElementName?.LocalName == "result");
        Assert.Equal(new[] { AuthoringNodeKind.Comment, AuthoringNodeKind.ProcessingInstruction, AuthoringNodeKind.Text, AuthoringNodeKind.Text, AuthoringNodeKind.Element, AuthoringNodeKind.Element }, result.Children.Select(n => n.Kind));
        AssertExtent(result.Children[0].Range, "<!-- <fake/> -->", text, encoding, bytes);
        AssertExtent(result.Children[1].Range, "<?review compact?>", text, encoding, bytes);
        AssertExtent(result.Children[2].Range, "<![CDATA[<not-a-tag/>]]>", text, encoding, bytes);
        AssertExtent(result.Children[3].Range, "é &amp; text", text, encoding, bytes);
        Assert.Equal("é & text", result.Children[3].Text);
        var value = nodes.Single(n => n.ElementName?.LocalName == "value-of");
        Assert.Equal("t", value.ElementName!.Prefix);
        Assert.Equal("http://www.w3.org/1999/XSL/Transform", value.ElementName.NamespaceUri);
        var slot = value.Attributes.Single(a => a.Name == "select");
        Assert.Equal("'A &amp; B'", slot.RawLiteral);
        Assert.Equal("'A & B'", slot.ExpandedValue);
        AssertExtent(slot.ValueRange, "'A &amp; B'", text, encoding, bytes);
        var empties = nodes.Where(n => n.ElementName?.LocalName == "empty").ToArray();
        Assert.Equal(2, empties.Length);
        AssertExtent(empties[0].Range, "<empty/>", text, encoding, bytes);
        AssertExtent(empties[1].Range, "<empty></empty>", text, encoding, bytes);
        Assert.Equal(2, nodes.Count(n => n.ElementName?.LocalName == "group"));
        Assert.Equal(nodes.Length, nodes.Select(n => n.Id).Distinct().Count());
        AssertContained(snapshot.PrincipalModule.Root);
    }

    /// <summary>A contextual edit changes only the chosen middle sibling's expression bytes.</summary>
    /// <param name="format">The encoding and BOM policy.</param>
    [Theory]
    [InlineData("utf8")]
    [InlineData("utf8bom")]
    [InlineData("utf16le")]
    [InlineData("utf16be")]
    public void MiddleSiblingExpressionEdit_IsIsolatedAndReinspected(string format)
    {
        var text = Declaration(format) + Header + string.Concat(Enumerable.Range(0, 3).Select(i => Group(i, ""))) + Footer;
        var encoding = EncodingFor(format);
        var snapshot = Inspect(text, encoding, out var original);
        var values = Flatten(snapshot.PrincipalModule.Root).Where(n => n.ElementName?.LocalName == "value-of").ToArray();
        var slot = values[1].Attributes.Single(a => a.Name == "select");
        var edit = snapshot.ProposeExpressionEdit(new AuthoringEditProposal(values[1].Id, "select", "/input/identifier"));
        Assert.True(edit.IsSuccess, edit.Failure?.ToString());
        var candidate = edit.Candidate!;
        var offset = (int)slot.ValueRange.StartByteOffset;
        var length = (int)slot.ValueRange.ByteLength;
        var expected = original[..offset].Concat(encoding.GetBytes("/input/identifier")).Concat(original[(offset + length)..]).ToArray();
        Assert.Equal(expected, candidate.EmittedSource.ToArray());
        Assert.Equal(original, snapshot.ExportOriginal(BaseUri));
        Assert.Equal(expected, candidate.Snapshot.ExportOriginal(BaseUri));
        Assert.Single(candidate.AffectedRanges);
        Assert.Equal(slot.ValueRange, candidate.AffectedRanges[0]);
        var updated = Flatten(candidate.Snapshot.PrincipalModule.Root).Where(n => n.ElementName?.LocalName == "value-of").ToArray();
        Assert.Equal(new[] { "/input/name", "/input/identifier", "/input/name" }, updated.Select(n => n.Attributes.Single(a => a.Name == "select").ExpandedValue));
        Assert.Equal(candidate.NodeCorrespondence[values[1].Id], updated[1].Id);
        AssertExtent(updated[1].Attributes.Single(a => a.Name == "select").ValueRange, "/input/identifier", text.Replace("<group id=\"1\"><nested><xsl:value-of select=\"/input/name\"", "<group id=\"1\"><nested><xsl:value-of select=\"/input/identifier\""), encoding, expected);
        AssertContained(candidate.Snapshot.PrincipalModule.Root);
        var template = Flatten(snapshot.PrincipalModule.Root).Single(n => n.ElementName?.LocalName == "template");
        Assert.False(snapshot.ProposeExpressionEdit(new AuthoringEditProposal(template.Id, "match", "node()")).IsSuccess);
    }

    /// <summary>The raw scanner must not skip immediate child markup when finding matching end tags.</summary>
    /// <param name="text">A complete lexical element.</param>
    [Theory]
    [InlineData("<group><nested><value/></nested></group>")]
    [InlineData("<group><nested/></group>")]
    [InlineData("<group><!-- <fake> --><?review x?><nested><value/></nested></group>")]
    [InlineData("<group><![CDATA[<fake>]]><nested><value/></nested></group>")]
    [InlineData("<group>é &amp; text<nested arrow='&gt;'><value/></nested></group>")]
    public void Scanner_UsesExclusiveStartTagEnd(string text)
    {
        Assert.Equal(text.Length, SourceTagScanner.FindElementEnd(text + "<next/>", 0, out var startTagEnd, out var selfClosed));
        Assert.Equal(7, startTagEnd);
        Assert.False(selfClosed);
    }

    /// <summary>Malformed compact XML retains its source and never publishes a snapshot.</summary>
    [Fact]
    public void MalformedCompactSource_RemainsClassifiedWithoutSnapshot()
    {
        var bytes = Encoding.UTF8.GetBytes(Header + "<group><nested></group>" + Footer);
        Assert.True(AuthoringSource.TryCreate(bytes, BaseUri, out var source, out _));
        var result = XsltAuthoring.Inspect(source!, options: new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.False(result.IsSuccess);
        Assert.Null(result.Snapshot);
        Assert.Equal(AuthoringFailureKind.Structure, result.Failure!.Kind);
        Assert.Equal(bytes, source!.OriginalBytes.ToArray());
        Assert.Throws<InvalidDataException>(() => SourceTagScanner.FindElementEnd("<group><nested/>", 0, out _, out _));
    }

    private static string Group(int i, string gap) => $"<group id=\"{i}\">{gap}<nested>{gap}<xsl:value-of select=\"/input/name\"/>{gap}</nested>{gap}</group>";
    private static string Declaration(string format) => format.StartsWith("utf16", StringComparison.Ordinal) ? "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n" : "";
    private static Encoding EncodingFor(string format) => format switch
    {
        "utf8" => new UTF8Encoding(false, true), "utf8bom" => new UTF8Encoding(true, true),
        "utf16le" => new UnicodeEncoding(false, true, true), _ => new UnicodeEncoding(true, true, true),
    };
    private static AuthoringSnapshot Inspect(string text, Encoding encoding, out byte[] bytes)
    {
        bytes = encoding.GetPreamble().Concat(encoding.GetBytes(text)).ToArray();
        Assert.True(AuthoringSource.TryCreate(bytes, BaseUri, out var source, out var failure), failure?.ToString());
        var inspection = XsltAuthoring.Inspect(source!, options: new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(inspection.IsSuccess, inspection.Failure?.ToString());
        return inspection.Snapshot!;
    }
    private static IEnumerable<AuthoringNodeDescriptor> Flatten(AuthoringNodeDescriptor node)
    {
        yield return node;
        foreach (var child in node.Children)
        foreach (var descendant in Flatten(child))
            yield return descendant;
    }
    private static void AssertContained(AuthoringNodeDescriptor parent)
    {
        long previousEnd = parent.Range.StartByteOffset;
        foreach (var child in parent.Children)
        {
            Assert.InRange(child.Range.StartByteOffset, previousEnd, parent.Range.StartByteOffset + parent.Range.ByteLength);
            previousEnd = child.Range.StartByteOffset + child.Range.ByteLength;
            Assert.True(previousEnd <= parent.Range.StartByteOffset + parent.Range.ByteLength);
            AssertContained(child);
        }
    }
    private static void AssertExtent(SourceRange actual, string lexical, string text, Encoding encoding, byte[] bytes, int minimumByteOffset = 0)
    {
        var start = text.IndexOf(lexical, StringComparison.Ordinal);
        while (start >= 0 && encoding.GetPreamble().Length + encoding.GetByteCount(text[..start]) < minimumByteOffset)
            start = text.IndexOf(lexical, start + 1, StringComparison.Ordinal);
        Assert.True(start >= 0, lexical);
        var end = start + lexical.Length;
        static (int Line, int Column) Position(string source, int offset)
            => (source[..offset].Count(c => c == '\n') + 1, offset - source.LastIndexOf('\n', Math.Max(0, offset - 1)));
        var begin = Position(text, start);
        var finish = Position(text, end);
        Assert.Equal(new SourceRange(begin.Line, begin.Column, finish.Line, finish.Column,
            encoding.GetPreamble().Length + encoding.GetByteCount(text[..start]), encoding.GetByteCount(lexical)), actual);
        Assert.Equal(lexical, encoding.GetString(bytes.AsSpan((int)actual.StartByteOffset, (int)actual.ByteLength)));
    }
}
