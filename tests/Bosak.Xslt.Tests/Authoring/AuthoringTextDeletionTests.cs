// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 October 2026
// PURPOSE              : Verify literal text deletion, merged-text source fragments and classified refusals.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>Actual API evidence for the bounded deletion and merged-text contract.</summary>
public class AuthoringTextDeletionTests
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    private const string Instruction = "<xsl:text>A</xsl:text>";
    private static readonly Uri Uri = new("file:///C:/project/deletion.xsl");
    private static string Sheet(string body, string version = "3.0") => $"<xsl:stylesheet xmlns:xsl='{Xsl}' version='{version}'><xsl:output omit-xml-declaration='yes' indent='no'/><xsl:template match='/'>{body}</xsl:template></xsl:stylesheet>";
    private static AuthoringSnapshot Inspect(byte[] bytes, bool compile = false)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, Uri, out var source, out var failure), failure?.Message);
        var result = XsltAuthoring.Inspect(source!, options: new() { AttemptCompilation = compile });
        Assert.True(result.IsSuccess, result.Failure?.Message);
        return result.Snapshot!;
    }
    private static AuthoringSnapshot Inspect(string text, bool compile = false) => Inspect(Encoding.UTF8.GetBytes(text), compile);
    private static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor root) => new[] { root }.Concat(root.Children.SelectMany(Nodes));
    private static AuthoringNodeDescriptor Find(AuthoringSnapshot s, string name) => Nodes(s.PrincipalModule.Root).First(n => n.ElementName?.LocalName == name);
    private static AuthoringTextDeletionCandidate Delete(AuthoringSnapshot s, AuthoringNodeDescriptor? node = null)
    {
        var target = s.GetTextDeletionTarget(node ?? Find(s, "text"));
        Assert.True(target.IsSuccess, target.Failure?.Message);
        var result = s.ProposeTextDeletion(new(target.Target!));
        Assert.True(result.IsSuccess, result.Failure?.Message);
        var c = result.Candidate!;
        Verify(c);
        return c;
    }
    private static void Verify(AuthoringTextDeletionCandidate c)
    {
        var old = c.InputSnapshot.ExportOriginal(c.ModuleUri); var fresh = c.ExportSource();
        var a = (int)c.OriginalRange.StartByteOffset; var b = a + (int)c.OriginalRange.ByteLength;
        Assert.Equal(old[..a].Concat(old[b..]).ToArray(), fresh);
        Assert.Equal(0, c.CandidateRange.ByteLength); Assert.Single(c.SourceEdits);
        Assert.Equal(c.OriginalRange, c.SourceEdits[0].OriginalRange);
        Assert.Equal(c.CandidateRange, c.SourceEdits[0].ReplacementRange);
        Assert.Equal(c.RemovedPayloadNode is { } payload ? new[] { c.RemovedNode, payload } : new[] { c.RemovedNode }, c.RemovedNodes);
        Assert.DoesNotContain(c.RemovedNode, c.NodeCorrespondence.Keys);
        if (c.RemovedPayloadNode is not null) Assert.DoesNotContain(c.RemovedPayloadNode, c.NodeCorrespondence.Keys);
        var inputs = c.RemovedNodes.Concat(c.NodeCorrespondence.Keys).Concat(c.TextMergeGroups.SelectMany(g => g.Fragments.Select(f => f.OriginalNode))).ToArray();
        Assert.Equal(inputs.Length, inputs.Distinct().Count());
        Assert.True(Nodes(c.InputSnapshot.PrincipalModule.Root).ToHashSet().SetEquals(inputs));
        var outputs = c.NodeCorrespondence.Values.Concat(c.TextMergeGroups.Select(g => g.CandidateNode)).ToArray();
        Assert.Equal(outputs.Length, outputs.Distinct().Count());
        Assert.True(Nodes(c.Snapshot.PrincipalModule.Root).ToHashSet().SetEquals(outputs));
        Assert.Contains(c.SurvivingParent, c.NodeCorrespondence.Values);
        foreach (var group in c.TextMergeGroups)
        {
            Assert.Equal(2, group.Fragments.Count);
            long offset = group.CandidateNode.Range.StartByteOffset;
            foreach (var f in group.Fragments)
            {
                Assert.Equal(f.OriginalNode.Range, f.OriginalRange);
                Assert.Equal(offset, f.CandidateRange.StartByteOffset);
                Assert.Equal(old.AsSpan((int)f.OriginalRange.StartByteOffset, (int)f.OriginalRange.ByteLength).ToArray(), fresh.AsSpan((int)offset, (int)f.CandidateRange.ByteLength).ToArray());
                offset += f.CandidateRange.ByteLength;
            }
            Assert.Equal(group.CandidateNode.Range.StartByteOffset + group.CandidateNode.Range.ByteLength, offset);
        }
        var mutated = c.ExportSource(); mutated[0] ^= 1; Assert.Equal(fresh, c.ExportSource());
        Assert.Equal(old, c.InputSnapshot.ExportOriginal(c.ModuleUri));
        Assert.True(c.Snapshot.IsCompilable);
    }

    /// <summary>Imported compact and indented first/middle/last/sole removals preserve the complete graph.</summary>
    /// <param name="position">The selected sibling position.</param>
    /// <param name="indent">Whether whitespace surrounds instructions.</param>
    /// <param name="version">The explicit supported profile.</param>
    [Theory]
    [InlineData(0, false, "3.0")][InlineData(1, false, "3.0")][InlineData(2, false, "3.0")]
    [InlineData(0, true, "3.0")][InlineData(1, true, "3.0")][InlineData(2, true, "3.0")]
    [InlineData(0, true, "4.0")][InlineData(1, true, "4.0")][InlineData(2, true, "4.0")]
    public void Positions(int position, bool indent, string version)
    {
        var gap = indent ? "\n  " : "";
        var text = Sheet("<out>" + gap + string.Join(gap, Enumerable.Repeat(Instruction, 3)) + gap + "</out>", version);
        var snapshot = Inspect(text);
        var node = Nodes(snapshot.PrincipalModule.Root).Where(n => n.ElementName?.LocalName == "text").ElementAt(position);
        var c = Delete(snapshot, node);
        Assert.Equal(indent ? 1 : 0, c.TextMergeGroups.Count);
        Assert.Equal(2, Nodes(c.Snapshot.PrincipalModule.Root).Count(n => n.ElementName?.LocalName == "text"));
    }

    /// <summary>Every encoding and lexical seam is verified through actual deletion and ordinary reinspection.</summary>
    /// <param name="encodingKind">UTF-8 without/with BOM or UTF-16 LE/BE.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void EncodingAndNewlineSeams(int encodingKind)
    {
        Encoding enc = encodingKind switch { 0 => new UTF8Encoding(false, true), 1 => new UTF8Encoding(true, true), 2 => new UnicodeEncoding(false, true, true), _ => new UnicodeEncoding(true, true, true) };
        foreach (var seam in new[] { ("\n  ", "\n", "\n  \n"), ("\r", "\n", "\n"), ("&#xD;", "\n", "\r\n"), ("&#xD;", "&#xA;", "\r\n") })
        {
            var text = $"<?xml version='1.0' encoding='{(encodingKind < 2 ? "utf-8" : "utf-16")}'?>" + Sheet("<out>" + seam.Item1 + Instruction + seam.Item2 + "</out>");
            var bytes = enc.GetPreamble().Concat(enc.GetBytes(text)).ToArray();
            var snapshot = Inspect(bytes);
            var c = Delete(snapshot);
            Assert.Equal(seam.Item3, Assert.Single(c.TextMergeGroups).CandidateNode.Text);
            Assert.Equal("out", c.SurvivingParent.ElementName!.LocalName);
            Assert.Equal(bytes, snapshot.ExportOriginal(Uri));
        }
    }

    /// <summary>Explicit empty spelling, comments/PIs and one-sided text retain their exact source.</summary>
    /// <param name="body">The containing constructor body.</param>
    [Theory]
    [InlineData("<xsl:text></xsl:text>")]
    [InlineData(" <!--keep--> <xsl:text/> ")]
    [InlineData(" <xsl:text/><?keep data?> ")]
    [InlineData(" <xsl:text/>")]
    [InlineData("<xsl:text/> ")]
    public void Trivia(string body) => Delete(Inspect(Sheet("<out>" + body + "</out>")));

    /// <summary>Previously inserted preserve/readable candidates and default compilation snapshots remain valid inputs.</summary>
    /// <param name="readable">Whether insertion produced readable layout.</param>
    /// <param name="compile">Whether ordinary inspection attempted compilation.</param>
    [Theory][InlineData(false, false)][InlineData(true, false)][InlineData(false, true)][InlineData(true, true)]
    public void InsertThenDelete(bool readable, bool compile)
    {
        var snapshot = Inspect(Sheet("\n  <out>\n  </out>\n"), compile);
        var target = snapshot.GetTextInsertionTarget(Find(snapshot, "out")); Assert.True(target.IsSuccess);
        var insertion = snapshot.ProposeTextInsertion(new(target.Target!, "A", readable ? AuthoringInsertionFormattingMode.Readable : AuthoringInsertionFormattingMode.PreserveSource));
        Assert.True(insertion.IsSuccess, insertion.Failure?.Message);
        Delete(insertion.Candidate!.Snapshot);
    }

    /// <summary>Namespace aliases and default XSLT namespaces identify by expanded name.</summary>
    /// <param name="alias">Whether XSLT uses a nondefault prefix.</param>
    [Theory][InlineData(true)][InlineData(false)]
    public void Namespaces(bool alias)
    {
        var text = alias ? Sheet("<out>" + Instruction + "</out>").Replace("xsl:", "s:").Replace("xmlns:xsl", "xmlns:s")
            : $"<stylesheet xmlns='{Xsl}' version='3.0'><template match='/'><out xmlns=''><text xmlns='{Xsl}'/></out></template></stylesheet>";
        Delete(Inspect(text));
    }

    /// <summary>Foreign targets and equal local identifiers cannot authorize deletion.</summary>
    [Fact]
    public void ForeignAndOldSnapshots()
    {
        var a = Inspect(Sheet("<out>" + Instruction + "</out>")); var b = Inspect(Sheet("<out>" + Instruction + "</out>"));
        Assert.Equal(AuthoringTextDeletionFailureKind.ForeignSnapshot, a.GetTextDeletionTarget(Find(b, "text")).Failure!.Kind);
        var target = a.GetTextDeletionTarget(Find(a, "text")).Target!;
        Assert.Equal(AuthoringTextDeletionFailureKind.ForeignSnapshot, b.ProposeTextDeletion(new(target)).Failure!.Kind);
        Assert.True(a.ProposeTextDeletion(new(target)).IsSuccess);
    }

    /// <summary>Body content, mixed/preserved contexts and unsupported static evaluation never yield a candidate.</summary>
    /// <param name="body">The excluded source body.</param>
    /// <param name="kind">The expected refusal category.</param>
    [Theory]
    [InlineData("<out><xsl:text><![CDATA[]]></xsl:text></out>", AuthoringTextDeletionFailureKind.UnsupportedCoverage)]
    [InlineData("<out><xsl:text><!--x--></xsl:text></out>", AuthoringTextDeletionFailureKind.UnsupportedCoverage)]
    [InlineData("<out><xsl:text><![CDATA[A]]></xsl:text></out>", AuthoringTextDeletionFailureKind.UnsupportedCoverage)]
    [InlineData("<out>text<xsl:text/></out>", AuthoringTextDeletionFailureKind.MixedContent)]
    [InlineData("<out xml:space='preserve'><xsl:text/></out>", AuthoringTextDeletionFailureKind.MixedContent)]
    [InlineData("<outer xml:space='preserve'><out xml:space='default'><xsl:text/></out></outer>", AuthoringTextDeletionFailureKind.MixedContent)]
    [InlineData("<out use-when='true()'><xsl:text/></out>", AuthoringTextDeletionFailureKind.UnsupportedCoverage)]
    [InlineData("<out><xsl:value-of select='$missing'/><xsl:text>A</xsl:text></out>", AuthoringTextDeletionFailureKind.InvalidOriginal)]
    public void Refusals(string body, AuthoringTextDeletionFailureKind kind)
    {
        if (body.Contains("use-when")) body = body.Replace("use-when", "xsl:use-when");
        var s = Inspect(Sheet(body)); var original = s.ExportOriginal(Uri);
        var result = s.GetTextDeletionTarget(Find(s, "text"));
        Assert.False(result.IsSuccess); Assert.Null(result.Target); Assert.Equal(kind, result.Failure!.Kind);
        Assert.Equal(original, s.ExportOriginal(Uri));
    }

    /// <summary>Cancellation is classified and programmer null arguments do not create partial results.</summary>
    [Fact]
    public void CancellationAndNulls()
    {
        var s = Inspect(Sheet("<out>" + Instruction + "</out>")); var n = Find(s, "text");
        var target = s.GetTextDeletionTarget(n).Target!; var token = new CancellationToken(true);
        Assert.Equal(AuthoringTextDeletionFailureKind.Cancelled, s.GetTextDeletionTarget(n, token).Failure!.Kind);
        Assert.Equal(AuthoringTextDeletionFailureKind.Cancelled, s.ProposeTextDeletion(new(target), token).Failure!.Kind);
        Assert.Throws<ArgumentNullException>(() => s.GetTextDeletionTarget(null!));
        Assert.Throws<ArgumentNullException>(() => s.ProposeTextDeletion(null!));
        Assert.Throws<ArgumentNullException>(() => new AuthoringTextDeletionProposal(null!));
    }

    /// <summary>Metadata, Unicode and surviving namespaces keep exact ranges and bytes.</summary>
    [Fact]
    public void UnicodeAndRetainedNamespaces()
    {
        var text = Sheet("<!--𝄞--><out xmlns:unused='urn:unused' data='𝄞 &amp; &lt;'>\r\n  " + Instruction.Replace("<xsl:text>", "<xsl:text xmlns:local='urn:local'>") + "\r\n</out>");
        var c = Delete(Inspect(text));
        Assert.Contains("xmlns:unused='urn:unused'", Encoding.UTF8.GetString(c.ExportSource()));
        Assert.DoesNotContain("urn:local", Encoding.UTF8.GetString(c.ExportSource()));
        Assert.Equal("\r\n  \r\n", Encoding.UTF8.GetString(c.ExportSource()).Split("data='𝄞 &amp; &lt;'>")[1].Split("</out>")[0]);
    }

    /// <summary>Oversized originals are not accepted merely because removal would make them smaller.</summary>
    /// <param name="kind">The physical encoding variant.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void OriginalByteBound(int kind)
    {
        Encoding enc = kind switch { 0 => new UTF8Encoding(false,true), 1 => new UTF8Encoding(true,true), 2 => new UnicodeEncoding(false,true,true), _ => new UnicodeEncoding(true,true,true) };
        var template = $"<?xml version='1.0' encoding='{(kind<2?"utf-8":"utf-16")}'?>" + Sheet("<!--PAD--><out>" + Instruction + "</out>");
        var overhead = enc.GetPreamble().Length + enc.GetByteCount(template.Replace("PAD", ""));
        var exact = template.Replace("PAD", new string('x', (AuthoringCapabilities.MaxTextDeletionSourceBytes - overhead) / enc.GetByteCount("x")));
        var s = Inspect(enc.GetPreamble().Concat(enc.GetBytes(exact)).ToArray()); Assert.Equal(AuthoringCapabilities.MaxTextDeletionSourceBytes, s.ExportOriginal(Uri).Length);
        Delete(s);
        var tooLarge = Inspect(enc.GetPreamble().Concat(enc.GetBytes(exact.Replace("<!--", "<!--x"))).ToArray());
        Assert.Equal(AuthoringTextDeletionFailureKind.SizeLimit, tooLarge.GetTextDeletionTarget(Find(tooLarge, "text")).Failure!.Kind);
    }

    /// <summary>Unsupported versions and originally invalid expressions cannot be repaired by deletion.</summary>
    /// <param name="version">The unsupported profile.</param>
    [Theory][InlineData("1.0")][InlineData("2.0")][InlineData("5.0")]
    public void UnsupportedProfiles(string version)
    {
        var s = Inspect(Sheet("<out>" + Instruction + "</out>", version));
        Assert.Equal(AuthoringTextDeletionFailureKind.UnsupportedProfile, s.GetTextDeletionTarget(Find(s, "text")).Failure!.Kind);
    }

    /// <summary>Non-XSLT same-name elements, non-elements and nested unsupported constructors refuse.</summary>
    [Fact]
    public void IneligibleInstructions()
    {
        var s = Inspect(Sheet("<out><text/><xsl:if test='true()'>" + Instruction + "</xsl:if></out>"));
        Assert.Equal(AuthoringTextDeletionFailureKind.IneligibleInstruction, s.GetTextDeletionTarget(s.PrincipalModule.Root).Failure!.Kind);
        Assert.Equal(AuthoringTextDeletionFailureKind.IneligibleInstruction, s.GetTextDeletionTarget(Find(s, "text")).Failure!.Kind);
        var instruction = Nodes(s.PrincipalModule.Root).Single(n => n.ElementName?.NamespaceUri == Xsl && n.ElementName.LocalName == "text");
        Assert.Equal(AuthoringTextDeletionFailureKind.IneligibleInstruction, s.GetTextDeletionTarget(instruction).Failure!.Kind);
    }

    /// <summary>Static exclusions refuse before resource access and never use the retained resolver.</summary>
    /// <param name="declaration">An excluded module/static declaration.</param>
    [Theory]
    [InlineData("<xsl:include href='https://invalid.example/other.xsl'/>")]
    [InlineData("<xsl:variable name='v' static='yes' select=\"doc('https://invalid.example/')\"/>")]
    [InlineData("<xsl:variable name='v' _select=\"doc('https://invalid.example/')\"/>")]
    public void ResourceAndPreprocessing(string declaration)
    {
        var bytes = Encoding.UTF8.GetBytes(Sheet("<out>" + Instruction + "</out>").Replace("<xsl:output", declaration + "<xsl:output"));
        Assert.True(AuthoringSource.TryCreate(bytes, Uri, out var source, out _));
        var resolver = new CountingResolver();
        var s = XsltAuthoring.Inspect(source!, resolver, new() { AttemptCompilation = false }).Snapshot!;
        var before = resolver.Calls;
        Assert.False(s.GetTextDeletionTarget(Find(s, "text")).IsSuccess);
        Assert.Equal(before, resolver.Calls);
    }

    /// <summary>Concurrent proposals against immutable input produce independently owned snapshots.</summary>
    [Fact]
    public async Task ConcurrentProposals()
    {
        var s = Inspect(Sheet("<out>\n  " + Instruction + "\n</out>"));
        var proposal = new AuthoringTextDeletionProposal(s.GetTextDeletionTarget(Find(s, "text")).Target!);
        var results = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Task.Run(() => s.ProposeTextDeletion(proposal))));
        foreach (var result in results) { Assert.True(result.IsSuccess); Verify(result.Candidate!); }
        Assert.NotSame(results[0].Candidate!.Snapshot, results[1].Candidate!.Snapshot);
        Assert.Equal(results[0].Candidate!.ExportSource(), results[1].Candidate!.ExportSource());
    }

    private sealed class CountingResolver : IAuthoringModuleResolver
    {
        internal int Calls { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) { Calls++; return null; }
    }

    /// <summary>Deletion changes output intentionally and leaves explicit empty parent spelling.</summary>
    [Fact]
    public void ChangedOutputGolden()
    {
        var original = Sheet("<out>" + Instruction + "</out>");
        Assert.Equal("<out>A</out>", new XsltCompiler().Compile(original, Uri.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse("<input/>"))));
        var c = Delete(Inspect(original));
        var output = new XsltCompiler().Compile(Encoding.UTF8.GetString(c.ExportSource()), Uri.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse("<input/>")));
        Assert.Equal("<out/>", output);
        Assert.Contains("<out></out>", Encoding.UTF8.GetString(c.ExportSource()));
    }

    /// <summary>All instruction spellings expose exact payload/body authority and change source.</summary>
    /// <param name="kind">The supported physical source encoding.</param>
    /// <param name="instruction">The original lexical instruction.</param>
    /// <param name="value">Its independently expected logical value.</param>
    [Theory]
    [InlineData(0,"<xsl:text/>","")][InlineData(1,"<xsl:text/>","")]
    [InlineData(2,"<xsl:text/>","")][InlineData(3,"<xsl:text/>","")]
    [InlineData(0,"<xsl:text></xsl:text>","")][InlineData(1,"<xsl:text></xsl:text>","")]
    [InlineData(2,"<xsl:text></xsl:text>","")][InlineData(3,"<xsl:text></xsl:text>","")]
    [InlineData(0,"<xsl:text>&lt;&amp;&#xD;&#xA;😀</xsl:text>","<&\r\n😀")]
    [InlineData(1,"<xsl:text>&lt;&amp;&#xD;&#xA;😀</xsl:text>","<&\r\n😀")]
    [InlineData(2,"<xsl:text>&lt;&amp;&#xD;&#xA;😀</xsl:text>","<&\r\n😀")]
    [InlineData(3,"<xsl:text>&lt;&amp;&#xD;&#xA;😀</xsl:text>","<&\r\n😀")]
    [InlineData(0,"<xsl:text> \t\r\n </xsl:text>"," \t\n ")]
    [InlineData(1,"<xsl:text> \t\r\n </xsl:text>"," \t\n ")]
    [InlineData(2,"<xsl:text> \t\r\n </xsl:text>"," \t\n ")]
    [InlineData(3,"<xsl:text> \t\r\n </xsl:text>"," \t\n ")]
    public void SpellingsAndPayload(int kind, string instruction, string value)
    {
        Encoding enc = kind switch { 0 => new UTF8Encoding(false,true), 1 => new UTF8Encoding(true,true), 2 => new UnicodeEncoding(false,true,true), _ => new UnicodeEncoding(true,true,true) };
        var xml=$"<?xml version='1.0' encoding='{(kind<2?"utf-8":"utf-16")}'?>"+Sheet("<!--😀--><out data='😀'>\r"+instruction+"\n</out>");
        var s=Inspect(enc.GetPreamble().Concat(enc.GetBytes(xml)).ToArray());
        var n=Find(s,"text"); var target=s.GetTextDeletionTarget(n).Target!;
        Assert.Equal(value,target.OriginalTextValue); Assert.Same(n,target.Instruction);
        Assert.Equal(instruction.EndsWith("/>"),target.IsSelfClosing);
        Assert.Equal(target.IsSelfClosing,target.OriginalBodyRange is null);
        var c=Delete(s); Assert.Equal(value,c.OriginalTextValue);
        Assert.Equal(value.Length==0?1:2,c.RemovedNodes.Count);
        Assert.Equal(target.OriginalBodyRange,c.OriginalBodyRange);
        Assert.Equal(s.ExportOriginal(Uri).Length-n.Range.ByteLength,c.ExportSource().Length);
        var expected=xml.Replace(instruction,"");
        Assert.Equal(enc.GetPreamble().Concat(enc.GetBytes(expected)),c.ExportSource());
        var seam=expected.IndexOf("\r\n</out>",StringComparison.Ordinal)+1;
        Assert.Equal(enc.GetPreamble().Length+enc.GetByteCount(expected[..seam]),c.CandidateRange.StartByteOffset);
        Assert.Equal("\n",Assert.Single(c.TextMergeGroups).CandidateNode.Text);
        Assert.Equal("<out data=\"😀\"/>",new XsltCompiler().Compile(expected,Uri.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse("<input/>"))));
    }

    /// <summary>Control exclusions apply to empty and self-closing instructions as well.</summary>
    /// <param name="attributes">Unsupported control syntax.</param>
    /// <param name="selfClosing">Whether the instruction lacks a body.</param>
    [Theory]
    [InlineData("expand-text='yes'",false)][InlineData("expand-text='yes'",true)]
    [InlineData("expand-text='true'",false)][InlineData("expand-text='1'",true)]
    [InlineData("expand-text='invalid'",false)][InlineData("expand-text='invalid'",true)]
    [InlineData("disable-output-escaping='yes'",false)][InlineData("disable-output-escaping='yes'",true)]
    [InlineData("disable-output-escaping='invalid'",false)][InlineData("disable-output-escaping='invalid'",true)]
    public void ExcludedControls(string attributes,bool selfClosing)
    {
        var s=Inspect(Sheet("<out><xsl:text "+attributes+(selfClosing?"/>":"></xsl:text>")+"</out>"));
        var result=s.GetTextDeletionTarget(Find(s,"text"));
        Assert.Null(result.Target); Assert.Equal(AuthoringTextDeletionFailureKind.UnsupportedCoverage,result.Failure!.Kind);
        Assert.Equal(Uri,result.Failure.ModuleUri); Assert.Null(result.Failure.Range);
    }

    /// <summary>Supported controls and removed local preserve do not alter surrounding source.</summary>
    /// <param name="attributes">Supported instruction metadata.</param>
    [Theory][InlineData("expand-text='no'")][InlineData("expand-text='false'")][InlineData("expand-text='0'")]
    [InlineData("disable-output-escaping='no'")][InlineData("xml:space='preserve'")][InlineData("xml:space='default'")]
    public void SupportedControls(string attributes) => Delete(Inspect(Sheet("<out> <xsl:text "+attributes+">A</xsl:text> </out>")));

    /// <summary>Local non-expansion overrides inherited expansion without widening remaining module coverage.</summary>
    [Fact]
    public void LocalExpandOverride() => Delete(Inspect(Sheet("<out><xsl:text expand-text='false'>{A}</xsl:text></out>").Replace("version='3.0'","version='3.0' expand-text='yes'")));

    /// <summary>Body markup and surrounding CDATA/preserved contexts never yield authority.</summary>
    /// <param name="body">The excluded constructor body.</param>
    /// <param name="kind">The refusal classification.</param>
    [Theory]
    [InlineData("<out><xsl:text><?pi x?></xsl:text></out>",AuthoringTextDeletionFailureKind.UnsupportedCoverage)]
    [InlineData("<out><xsl:text><child/></xsl:text></out>",AuthoringTextDeletionFailureKind.UnsupportedCoverage)]
    [InlineData("<out><xsl:text>A<!--x-->B</xsl:text></out>",AuthoringTextDeletionFailureKind.UnsupportedCoverage)]
    [InlineData("<out><![CDATA[]]><xsl:text/></out>",AuthoringTextDeletionFailureKind.MixedContent)]
    [InlineData("<out><![CDATA[ ]]><xsl:text/></out>",AuthoringTextDeletionFailureKind.MixedContent)]
    [InlineData("<out>\u00a0<xsl:text/></out>",AuthoringTextDeletionFailureKind.MixedContent)]
    public void BodyAndParentBoundaries(string body,AuthoringTextDeletionFailureKind kind)
    { var s=Inspect(Sheet(body)); Assert.Equal(kind,s.GetTextDeletionTarget(Find(s,"text")).Failure!.Kind); }

    /// <summary>Replacement accepts its existing mixed/preserved profile while deletion refuses it.</summary>
    [Fact]
    public void ReplacementCompatibility()
    {
        var s=Inspect(Sheet("<out xml:space='preserve'>L<xsl:text>A</xsl:text>R</out>"));
        Assert.Equal(AuthoringTextDeletionFailureKind.MixedContent,s.GetTextDeletionTarget(Find(s,"text")).Failure!.Kind);
        var t=s.GetTextBodyReplacementTarget(Find(s,"text")); Assert.True(t.IsSuccess,t.Failure?.Message);
        Assert.NotNull(s.ProposeTextBodyReplacement(new(t.Target!,"B")).Candidate);
    }

    /// <summary>A freshly replaced body and a legacy throwing cancellation contract remain composable.</summary>
    [Fact]
    public void ReplaceThenDeleteAndLegacyCancellation()
    {
        var s=Inspect(Sheet("<out><xsl:value-of select='1'/><xsl:text>A</xsl:text></out>"));
        var t=s.GetTextBodyReplacementTarget(Find(s,"text")).Target!;
        var replaced=s.ProposeTextBodyReplacement(new(t,"B")).Candidate!;
        Assert.Equal("B",Delete(replaced.Snapshot).OriginalTextValue);
        var v=Find(s,"value-of");var token=new CancellationToken(true);
        Assert.Throws<OperationCanceledException>(()=>s.GetValueOfDeletionTarget(v,token));
        var vt=s.GetValueOfDeletionTarget(v).Target!;
        Assert.Throws<OperationCanceledException>(()=>s.ProposeValueOfDeletion(new(vt),token));
        Assert.Equal(AuthoringDeletionFailureKind.IneligibleNode,s.GetValueOfDeletionTarget(Find(s,"text")).Failure!.Kind);
    }

    /// <summary>The additive capability, opaque constructors and returned collections preserve authority.</summary>
    [Fact]
    public void ImmutablePublicSurface()
    {
        Assert.True(AuthoringCapabilities.SupportsTextDeletion); Assert.Equal(8_388_608,AuthoringCapabilities.MaxTextDeletionSourceBytes);
        Assert.Empty(typeof(AuthoringTextDeletionTarget).GetConstructors());
        Assert.Empty(typeof(AuthoringTextDeletionCandidate).GetConstructors());
        Assert.Null(typeof(AuthoringTextDeletionResult).GetProperty("NoChange"));
        Assert.Throws<NotSupportedException>(()=>((IList<string>)AuthoringCapabilities.TextDeletionSupportedEncodings)[0]="bad");
        var c=Delete(Inspect(Sheet("<out> "+Instruction+" </out>")));
        Assert.Throws<NotSupportedException>(()=>((IList<AuthoringNodeDescriptor>)c.RemovedNodes).Clear());
        Assert.Throws<NotSupportedException>(()=>((IDictionary<AuthoringNodeDescriptor,AuthoringNodeDescriptor>)c.NodeCorrespondence).Clear());
        Assert.Throws<NotSupportedException>(()=>((IList<AuthoringTextMergeFragment>)c.TextMergeGroups[0].Fragments).Clear());
    }

    /// <summary>Inspection's broader encodings do not imply deletion encoding authority.</summary>
    /// <param name="name">A supported inspection encoding outside deletion coverage.</param>
    [Theory][InlineData("us-ascii")][InlineData("iso-8859-1")][InlineData("utf-32")][InlineData("utf-32BE")]
    public void UnsupportedDeletionEncodings(string name)
    {
        var enc = Encoding.GetEncoding(name);
        var xml = $"<?xml version='1.0' encoding='{name}'?>" + Sheet("<out>" + Instruction + "</out>");
        var snapshot = Inspect(enc.GetPreamble().Concat(enc.GetBytes(xml)).ToArray());
        var result = snapshot.GetTextDeletionTarget(Find(snapshot, "text"));
        Assert.Null(result.Target);
        Assert.Equal(AuthoringTextDeletionFailureKind.UnsupportedEncoding, result.Failure!.Kind);
    }
}
