// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 October 2026
// PURPOSE              : Verify readable insertion layout, exact edit replay and source provenance.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 17-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>Public API acceptance fixtures for REQ-129 readable insertion.</summary>
public class AuthoringReadableInsertionTests
{
    private static readonly Uri Uri = new("file:///C:/project/main.xsl");
    private static string Sheet(string body) => "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'><xsl:output omit-xml-declaration='yes' indent='no'/><xsl:template match='/'>" + body + "</xsl:template></xsl:stylesheet>";
    private static AuthoringSnapshot Inspect(string text, Encoding? encoding = null)
    {
        encoding ??= new UTF8Encoding(false, true);
        Assert.True(AuthoringSource.TryCreate(encoding.GetPreamble().Concat(encoding.GetBytes(text)).ToArray(), Uri, out var source, out var failure), failure?.ToString());
        var result = XsltAuthoring.Inspect(source!, options: new AuthoringInspectionOptions { AttemptCompilation = false }); Assert.True(result.IsSuccess, result.Failure?.ToString()); return result.Snapshot!;
    }
    private static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor node) => new[] { node }.Concat(node.Children.SelectMany(Nodes));
    private static AuthoringValueOfInsertionTarget Target(AuthoringSnapshot snapshot)
    {
        var result = snapshot.GetValueOfInsertionTarget(Nodes(snapshot.PrincipalModule.Root).Single(n => n.ElementName?.LocalName == "out")); Assert.True(result.IsSuccess, result.Failure?.ToString()); return result.Target!;
    }
    private static AuthoringValueOfInsertionCandidate Insert(AuthoringSnapshot snapshot, string expression = "'A'", AuthoringInsertionFormattingMode mode = AuthoringInsertionFormattingMode.Readable)
    {
        var result = snapshot.ProposeValueOfInsertion(new(Target(snapshot), expression, mode)); Assert.True(result.IsSuccess, result.Failure?.ToString()); return result.Candidate!;
    }
    private static string Text(AuthoringValueOfInsertionCandidate c, Encoding? encoding = null) => (encoding ?? Encoding.UTF8).GetString(c.ExportSource()).TrimStart('\uFEFF');
    private static string Transform(AuthoringValueOfInsertionCandidate c) => new XsltCompiler().Compile(Text(c), Uri.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse("<input/>")));
    private static void Verify(AuthoringValueOfInsertionCandidate c)
    {
        var old = c.InputSnapshot.ExportOriginal(Uri); var output = c.ExportSource(); using var replay = new MemoryStream(); int oi = 0, ci = 0;
        foreach (var e in c.SourceEdits)
        {
            var start = checked((int)e.OriginalRange.StartByteOffset); var next = checked((int)e.ReplacementRange.StartByteOffset);
            Assert.True(start >= oi && next >= ci); Assert.Equal(old[oi..start], output[ci..next]); replay.Write(old.AsSpan(oi, start - oi)); replay.Write(output.AsSpan(next, (int)e.ReplacementRange.ByteLength)); oi = start + (int)e.OriginalRange.ByteLength; ci = next + (int)e.ReplacementRange.ByteLength;
        }
        Assert.Equal(old[oi..], output[ci..]); replay.Write(old.AsSpan(oi)); Assert.Equal(output, replay.ToArray());
        var originals = Nodes(c.InputSnapshot.PrincipalModule.Root).ToArray(); Assert.Equal(originals.Length, c.NodeCorrespondence.Count); Assert.Equal(originals.Length, c.NodeCorrespondence.Values.Distinct().Count());
        foreach (var n in originals) { var survivor = Nodes(c.Snapshot.PrincipalModule.Root).Single(x => x.Id == c.NodeCorrespondence[n.Id]); Assert.Equal(n.Kind, survivor.Kind); Assert.Equal(n.ElementName, survivor.ElementName); if (n.Kind != AuthoringNodeKind.Text) Assert.Equal(n.Text, survivor.Text); }
        Assert.Equal(c.InsertedNode.Range, c.InsertedRange); Assert.Equal(0, c.OriginalRange.ByteLength); Assert.Equal(AuthoringAttributeSlotKind.Expression, c.SelectSlot.SlotKind); Assert.Equal(Uri, c.ModuleUri);
    }
    /// <summary>Compact direct children receive two-space indentation and exact replayable edits.</summary>
    /// <param name="children">The fixture children.</param>
    [Theory]
    [InlineData("")]
    [InlineData("<a/>")]
    [InlineData("<a/><b/>")]
    [InlineData("<!-- </out> --><?note raw?><a/>")]
    [InlineData(" \t<a/> ")]
    public void Compact_FormatsOnlyDirectGaps(string children)
    {
        var snapshot = Inspect(Sheet("<out>" + children + "</out>")); var before = snapshot.ExportOriginal(Uri); var c = Insert(snapshot); Verify(c);
        Assert.Contains("\n  <xsl:value-of select=\"'A'\"/>\n</out>", Text(c)); Assert.DoesNotContain("<out><", Text(c)); Assert.Equal(before, snapshot.ExportOriginal(Uri)); Assert.Equal(Transform(Insert(snapshot, mode: AuthoringInsertionFormattingMode.PreserveSource)), Transform(c));
    }
    /// <summary>Existing indentation and newline conventions survive repeat insertion and retain child bytes.</summary>
    /// <param name="nl">The fixture nl.</param>
    /// <param name="indent">The fixture indent.</param>
    [Theory]
    [InlineData("\n", "  ")]
    [InlineData("\r\n", "    ")]
    [InlineData("\r", "\t")]
    public void Indented_RepeatPreservesLayout(string nl, string indent)
    {
        var child = "<a data='&#65;'><nested/></a>"; var snapshot = Inspect(Sheet("<out>" + nl + indent + child + nl + "</out>")); var first = Insert(snapshot); Verify(first); Assert.Contains(child + nl + indent + "<xsl:value-of", Text(first));
        var second = Insert(first.Snapshot, "'B'"); Verify(second); Assert.Contains("select=\"'A'\"/>" + nl + indent + "<xsl:value-of select=\"'B'\"/>", Text(second)); Assert.Equal(Transform(Insert(first.Snapshot, "'B'", AuthoringInsertionFormattingMode.PreserveSource)), Transform(second));
    }
    /// <summary>Empty multiline parents can infer a homogeneous indentation unit from an ancestor.</summary>
    [Fact]
    public void EmptyIndented_InfersAncestorUnit()
    {
        var text = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>\n    <xsl:template match='/'>\n        <out>\n        </out>\n    </xsl:template>\n</xsl:stylesheet>"; var c = Insert(Inspect(text)); Verify(c); Assert.Contains("<out>\n            <xsl:value-of", Text(c));
    }
    /// <summary>Ambiguous layout refuses atomically while legacy insertion keeps working.</summary>
    /// <param name="body">The fixture body.</param>
    [Theory]
    [InlineData("<out>\n  <a/>\r\n</out>")]
    [InlineData("<out>\n  <a/>\n    <b/>\n</out>")]
    [InlineData("<out><a/>\n  <b/>\n</out>")]
    [InlineData("<out>\n \t<a/>\n</out>")]
    [InlineData("<out>&#32;<a/></out>")]
    public void Ambiguous_RefusesWithoutDowngrade(string body)
    {
        var snapshot = Inspect(Sheet(body)); var before = snapshot.ExportOriginal(Uri); var result = snapshot.ProposeValueOfInsertion(new(Target(snapshot), "'A'", AuthoringInsertionFormattingMode.Readable)); Assert.Null(result.Candidate); Assert.Equal(AuthoringInsertionFailureKind.UnsupportedFormatting, result.Failure!.Kind); Assert.Equal(before, snapshot.ExportOriginal(Uri)); Verify(Insert(snapshot, mode: AuthoringInsertionFormattingMode.PreserveSource));
    }
    /// <summary>Original constructor and explicit default preserve the historical splice and invalid enum values throw.</summary>
    [Fact]
    public void Default_IsCompatible()
    {
        Assert.True(AuthoringCapabilities.SupportsReadableValueOfInsertion); var s = Inspect(Sheet("<out>\n<a/>\r\n</out>")); var target = Target(s); var old = s.ProposeValueOfInsertion(new(target, "'A'")).Candidate!; var explicitMode = Insert(s, mode: AuthoringInsertionFormattingMode.PreserveSource); Assert.Equal(old.ExportSource(), explicitMode.ExportSource()); Assert.Single(old.SourceEdits); Assert.Equal(AuthoringInsertionFormattingMode.PreserveSource, old.FormattingMode); Verify(old); Assert.Throws<ArgumentOutOfRangeException>(() => new AuthoringValueOfInsertionProposal(target, "'A'", (AuthoringInsertionFormattingMode)42));
    }
    /// <summary>Encodings, declarations, BOMs, Unicode and escaped expression text retain exact byte evidence.</summary>
    /// <param name="variant">The fixture variant.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Encodings_AreExact(int variant)
    {
        Encoding encoding = variant switch { 0 => new UTF8Encoding(false, true), 1 => new UTF8Encoding(true, true), 2 => new UnicodeEncoding(false, true, true), _ => new UnicodeEncoding(true, true, true) }; var declaration = variant < 2 ? "utf-8" : variant == 2 ? "utf-16le" : "utf-16be"; var s = Inspect("<?xml version='1.0' encoding='" + declaration + "'?>" + Sheet("<out note='😀é'><a/></out>"), encoding); var c = Insert(s, "'&<>\"😀é'"); Verify(c); Assert.Equal(encoding.GetPreamble(), c.ExportSource()[..encoding.GetPreamble().Length]); Assert.StartsWith("<?xml version='1.0' encoding='" + declaration + "'?>", Text(c, encoding)); Assert.Equal("'&<>\"😀é'", c.SelectSlot.ExpandedValue);
    }
    /// <summary>Candidate bytes and edit/correspondence collections cannot mutate engine state.</summary>
    [Fact]
    public void Candidate_IsDefensive()
    {
        var c = Insert(Inspect(Sheet("<out> <a/> </out>"))); Verify(c); Assert.Throws<NotSupportedException>(() => ((IList<AuthoringInsertionSourceEdit>)c.SourceEdits).Clear()); Assert.Throws<NotSupportedException>(() => ((IDictionary<int, int>)c.NodeCorrespondence).Clear()); var bytes = c.ExportSource(); bytes[0] ^= 1; Assert.NotEqual(bytes, c.ExportSource());
    }
    /// <summary>Existing mixed-content and explicit-close restrictions remain enforced.</summary>
    /// <param name="body">The fixture body.</param>
    /// <param name="kind">The fixture kind.</param>
    [Theory]
    [InlineData("<out/>", AuthoringInsertionFailureKind.SelfClosingParent)]
    [InlineData("<out>text</out>", AuthoringInsertionFailureKind.MixedContent)]
    [InlineData("<out><![CDATA[ ]]></out>", AuthoringInsertionFailureKind.MixedContent)]
    [InlineData("<out xml:space='preserve'> </out>", AuthoringInsertionFailureKind.MixedContent)]
    public void ExistingRefusals_Remain(string body, AuthoringInsertionFailureKind kind)
    {
        var s = Inspect(Sheet(body)); var result = s.GetValueOfInsertionTarget(Nodes(s.PrincipalModule.Root).Single(n => n.ElementName?.LocalName == "out")); Assert.Equal(kind, result.Failure!.Kind);
    }
    /// <summary>Invalid expressions, cancellation and foreign targets return no candidate or input mutation.</summary>
    [Fact]
    public void InvalidCancelledForeign_RefuseAtomically()
    {
        var s = Inspect(Sheet("<out><a/></out>")); var target = Target(s); var before = s.ExportOriginal(Uri); var invalid = s.ProposeValueOfInsertion(new(target, "$missing", AuthoringInsertionFormattingMode.Readable)); Assert.Equal(AuthoringInsertionFailureKind.InvalidExpression, invalid.Failure!.Kind); using var cts = new CancellationTokenSource(); cts.Cancel(); var cancelled = s.ProposeValueOfInsertion(new(target, "'A'", AuthoringInsertionFormattingMode.Readable), cts.Token); Assert.Equal(AuthoringInsertionFailureKind.Cancelled, cancelled.Failure!.Kind); Assert.Null(cancelled.Candidate); Assert.Equal(before, s.ExportOriginal(Uri)); var foreign = Inspect(Sheet("<out></out>")); Assert.Equal(AuthoringInsertionFailureKind.ForeignSnapshot, foreign.ProposeValueOfInsertion(new(target, "'A'", AuthoringInsertionFormattingMode.Readable)).Failure!.Kind);
    }
    /// <summary>Compact formatting yields an exact golden candidate.</summary>
    /// <param name="version">The effective stylesheet version.</param>
    [Theory]
    [InlineData("3.0")]
    [InlineData("4.0")]
    public void Compact_ExactGolden(string version)
    {
        var c = Insert(Inspect(Sheet("<out><a/><b/></out>").Replace("version='3.0'", "version='" + version + "'")));
        Assert.Equal(Sheet("<out>\n  <a/>\n  <b/>\n  <xsl:value-of select=\"'A'\"/>\n</out>").Replace("version='3.0'", "version='" + version + "'"), Text(c)); Verify(c);
    }
    /// <summary>Namespace aliases, default-only bindings and prefix collisions preserve instruction identity.</summary>
    /// <param name="prefix">The fixture prefix.</param>
    /// <param name="collision">The fixture collision.</param>
    [Theory]
    [InlineData("alt", false)]
    [InlineData("", false)]
    [InlineData("", true)]
    public void NamespaceVariants_ArePreserved(string prefix, bool collision)
    {
        var s = Sheet("<out xmlns=''><a/></out>").Replace("xmlns:xsl=", prefix.Length == 0 ? "xmlns=" : "xmlns:" + prefix + "=").Replace("xsl:", prefix.Length == 0 ? "" : prefix + ":");
        if (collision) s = s.Replace("<out xmlns=''", "<out xmlns='' xmlns:xsl='urn:collision'");
        var c = Insert(Inspect(s)); Verify(c); Assert.Equal("http://www.w3.org/1999/XSL/Transform", c.InsertedNode.ElementName!.NamespaceUri);
    }
    /// <summary>Formatting-induced overflow refuses while a legacy candidate exactly fits the byte limit.</summary>
    [Fact]
    public void LayoutOverLimit_Refuses()
    {
        const string instruction = "<xsl:value-of select=\"'A'\"/>";
        var filler = AuthoringCapabilities.MaxValueOfInsertionSourceBytes - Encoding.UTF8.GetByteCount(Sheet("<out><!----></out>")) - instruction.Length;
        var s = Inspect(Sheet("<out><!--" + new string('a', filler) + "--></out>")); var target = Target(s);
        var result = s.ProposeValueOfInsertion(new(target, "'A'", AuthoringInsertionFormattingMode.Readable)); Assert.Null(result.Candidate); Assert.Equal(AuthoringInsertionFailureKind.SizeLimit, result.Failure!.Kind);
        var legacy = s.ProposeValueOfInsertion(new(target, "'A'")); Assert.True(legacy.IsSuccess, legacy.Failure?.ToString()); Assert.Equal(AuthoringCapabilities.MaxValueOfInsertionSourceBytes, legacy.Candidate!.ExportSource().Length);
    }
    /// <summary>Final byte counting permits safe whitespace shrinkage when a legacy append would exceed the limit.</summary>
    [Fact]
    public void Shrink_UsesFinalSize()
    {
        var filler = AuthoringCapabilities.MaxValueOfInsertionSourceBytes - Encoding.UTF8.GetByteCount(Sheet("<out><!---->" + new string(' ', 128) + "</out>"));
        var s = Inspect(Sheet("<out><!--" + new string('a', filler) + "-->" + new string(' ', 128) + "</out>")); var c = Insert(s); Verify(c); Assert.True(c.ExportSource().Length < AuthoringCapabilities.MaxValueOfInsertionSourceBytes);
        Assert.Equal(AuthoringInsertionFailureKind.SizeLimit, s.ProposeValueOfInsertion(new(Target(s), "'A'")).Failure!.Kind);
    }

    /// <summary>Ancestor preserved whitespace refuses formatting even after a nearer default reset.</summary>
    /// <param name="reset">Whether the target explicitly resets xml:space.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EffectiveXmlSpace_IsRespected(bool reset)
    {
        var body="<outer xml:space='preserve'><out"+(reset?" xml:space='default'":"")+"><a/></out></outer>";
        var snapshot=Inspect(Sheet(body));var node=Nodes(snapshot.PrincipalModule.Root).Single(n=>n.ElementName?.LocalName=="out");
        var target=snapshot.GetValueOfInsertionTarget(node);
        if(!reset){Assert.Equal(AuthoringInsertionFailureKind.MixedContent,target.Failure!.Kind);return;}
        Assert.True(target.IsSuccess,target.Failure?.ToString());
        var readable=snapshot.ProposeValueOfInsertion(new(target.Target!,"'A'",AuthoringInsertionFormattingMode.Readable));
        Assert.Null(readable.Candidate);Assert.Equal(AuthoringInsertionFailureKind.UnsupportedFormatting,readable.Failure!.Kind);
        Verify(Insert(snapshot,mode:AuthoringInsertionFormattingMode.PreserveSource));
    }

    /// <summary>Formatting does not move the new expression outside a preceding variable's static scope.</summary>
    [Fact]
    public void VariableScope_IsPreserved()
    {
        var s=Inspect(Sheet("<out><xsl:variable name='v' select=\"'A'\"/></out>"));
        var readable=Insert(s,"$v");Verify(readable);
        Assert.Equal(Transform(Insert(s,"$v",AuthoringInsertionFormattingMode.PreserveSource)),Transform(readable));
        Assert.Equal("$v",readable.SelectSlot.ExpandedValue);
    }

}
