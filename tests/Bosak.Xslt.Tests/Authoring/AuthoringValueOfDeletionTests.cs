// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 October 2026
// PURPOSE              : Verify instruction-only deletion, merged-text source fragments and classified refusals.
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

/// <summary>Actual API evidence for the bounded deletion and merged-text contract.</summary>
public class AuthoringValueOfDeletionTests
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    private const string Instruction = "<xsl:value-of select=\"'A'\"/>";
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
    private static AuthoringValueOfDeletionCandidate Delete(AuthoringSnapshot s, AuthoringNodeDescriptor? node = null)
    {
        var target = s.GetValueOfDeletionTarget(node ?? Find(s, "value-of"));
        Assert.True(target.IsSuccess, target.Failure?.Message);
        var result = s.ProposeValueOfDeletion(new(target.Target!));
        Assert.True(result.IsSuccess, result.Failure?.Message);
        var c = result.Candidate!;
        Verify(c);
        return c;
    }
    private static void Verify(AuthoringValueOfDeletionCandidate c)
    {
        var old = c.InputSnapshot.ExportOriginal(c.ModuleUri); var fresh = c.ExportSource();
        var a = (int)c.OriginalRange.StartByteOffset; var b = a + (int)c.OriginalRange.ByteLength;
        Assert.Equal(old[..a].Concat(old[b..]).ToArray(), fresh);
        Assert.Equal(0, c.CandidateRange.ByteLength); Assert.Single(c.SourceEdits);
        Assert.Equal(c.OriginalRange, c.SourceEdits[0].OriginalRange);
        Assert.Equal(c.CandidateRange, c.SourceEdits[0].ReplacementRange);
        Assert.Equal(new[] { c.RemovedNode }, c.RemovedNodes);
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
        var node = Nodes(snapshot.PrincipalModule.Root).Where(n => n.ElementName?.LocalName == "value-of").ElementAt(position);
        var c = Delete(snapshot, node);
        Assert.Equal(indent ? 1 : 0, c.TextMergeGroups.Count);
        Assert.Equal(2, Nodes(c.Snapshot.PrincipalModule.Root).Count(n => n.ElementName?.LocalName == "value-of"));
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
    [InlineData("<xsl:value-of select='1'></xsl:value-of>")]
    [InlineData(" <!--keep--> <xsl:value-of select='1'/> ")]
    [InlineData(" <xsl:value-of select='1'/><?keep data?> ")]
    [InlineData(" <xsl:value-of select='1'/>")]
    [InlineData("<xsl:value-of select='1'/> ")]
    public void Trivia(string body) => Delete(Inspect(Sheet("<out>" + body + "</out>")));

    /// <summary>Previously inserted preserve/readable candidates and default compilation snapshots remain valid inputs.</summary>
    /// <param name="readable">Whether insertion produced readable layout.</param>
    /// <param name="compile">Whether ordinary inspection attempted compilation.</param>
    [Theory][InlineData(false, false)][InlineData(true, false)][InlineData(false, true)][InlineData(true, true)]
    public void InsertThenDelete(bool readable, bool compile)
    {
        var snapshot = Inspect(Sheet("\n  <out>\n  </out>\n"), compile);
        var target = snapshot.GetValueOfInsertionTarget(Find(snapshot, "out")); Assert.True(target.IsSuccess);
        var insertion = snapshot.ProposeValueOfInsertion(new(target.Target!, "'A'", readable ? AuthoringInsertionFormattingMode.Readable : AuthoringInsertionFormattingMode.PreserveSource));
        Assert.True(insertion.IsSuccess, insertion.Failure?.Message);
        Delete(insertion.Candidate!.Snapshot);
    }

    /// <summary>Namespace aliases and default XSLT namespaces identify by expanded name.</summary>
    /// <param name="alias">Whether XSLT uses a nondefault prefix.</param>
    [Theory][InlineData(true)][InlineData(false)]
    public void Namespaces(bool alias)
    {
        var text = alias ? Sheet("<out>" + Instruction + "</out>").Replace("xsl:", "s:").Replace("xmlns:xsl", "xmlns:s")
            : $"<stylesheet xmlns='{Xsl}' version='3.0'><template match='/'><out xmlns=''><value-of xmlns='{Xsl}' select='1'/></out></template></stylesheet>";
        Delete(Inspect(text));
    }

    /// <summary>Foreign targets and equal local identifiers cannot authorize deletion.</summary>
    [Fact]
    public void ForeignAndOldSnapshots()
    {
        var a = Inspect(Sheet("<out>" + Instruction + "</out>")); var b = Inspect(Sheet("<out>" + Instruction + "</out>"));
        Assert.Equal(AuthoringDeletionFailureKind.ForeignTarget, a.GetValueOfDeletionTarget(Find(b, "value-of")).Failure!.Kind);
        var target = a.GetValueOfDeletionTarget(Find(a, "value-of")).Target!;
        Assert.Equal(AuthoringDeletionFailureKind.ForeignTarget, b.ProposeValueOfDeletion(new(target)).Failure!.Kind);
        Assert.True(a.ProposeValueOfDeletion(new(target)).IsSuccess);
    }

    /// <summary>Body content, mixed/preserved contexts and unsupported static evaluation never yield a candidate.</summary>
    /// <param name="body">The excluded source body.</param>
    /// <param name="kind">The expected refusal category.</param>
    [Theory]
    [InlineData("<out><xsl:value-of select='1'> </xsl:value-of></out>", AuthoringDeletionFailureKind.IneligibleNode)]
    [InlineData("<out><xsl:value-of select='1'><!--x--></xsl:value-of></out>", AuthoringDeletionFailureKind.IneligibleNode)]
    [InlineData("<out><xsl:value-of select='1'><![CDATA[]]></xsl:value-of></out>", AuthoringDeletionFailureKind.IneligibleNode)]
    [InlineData("<out>text<xsl:value-of select='1'/></out>", AuthoringDeletionFailureKind.MixedContent)]
    [InlineData("<out xml:space='preserve'><xsl:value-of select='1'/></out>", AuthoringDeletionFailureKind.MixedContent)]
    [InlineData("<outer xml:space='preserve'><out xml:space='default'><xsl:value-of select='1'/></out></outer>", AuthoringDeletionFailureKind.MixedContent)]
    [InlineData("<out use-when='true()'><xsl:value-of select='1'/></out>", AuthoringDeletionFailureKind.UnsupportedCoverage)]
    [InlineData("<out><xsl:value-of select='$missing'/></out>", AuthoringDeletionFailureKind.InvalidOriginal)]
    public void Refusals(string body, AuthoringDeletionFailureKind kind)
    {
        if (body.Contains("use-when")) body = body.Replace("use-when", "xsl:use-when");
        var s = Inspect(Sheet(body)); var original = s.ExportOriginal(Uri);
        var result = s.GetValueOfDeletionTarget(Find(s, "value-of"));
        Assert.False(result.IsSuccess); Assert.Null(result.Target); Assert.Equal(kind, result.Failure!.Kind);
        Assert.Equal(original, s.ExportOriginal(Uri));
    }

    /// <summary>Cancellation throws and programmer null arguments do not create partial results.</summary>
    [Fact]
    public void CancellationAndNulls()
    {
        var s = Inspect(Sheet("<out>" + Instruction + "</out>")); var n = Find(s, "value-of");
        var target = s.GetValueOfDeletionTarget(n).Target!; var token = new CancellationToken(true);
        Assert.Throws<OperationCanceledException>(() => s.GetValueOfDeletionTarget(n, token));
        Assert.Throws<OperationCanceledException>(() => s.ProposeValueOfDeletion(new(target), token));
        Assert.Throws<ArgumentNullException>(() => s.GetValueOfDeletionTarget(null!));
        Assert.Throws<ArgumentNullException>(() => s.ProposeValueOfDeletion(null!));
        Assert.Throws<ArgumentNullException>(() => new AuthoringValueOfDeletionProposal(null!));
    }

    /// <summary>Metadata, Unicode and surviving namespaces keep exact ranges and bytes.</summary>
    [Fact]
    public void UnicodeAndRetainedNamespaces()
    {
        var text = Sheet("<!--𝄞--><out xmlns:unused='urn:unused' data='𝄞 &amp; &lt;'>\r\n  " + Instruction.Replace("/>", " xmlns:local='urn:local'/>") + "\r\n</out>");
        var c = Delete(Inspect(text));
        Assert.Contains("xmlns:unused='urn:unused'", Encoding.UTF8.GetString(c.ExportSource()));
        Assert.DoesNotContain("urn:local", Encoding.UTF8.GetString(c.ExportSource()));
        Assert.Equal("\r\n  \r\n", Encoding.UTF8.GetString(c.ExportSource()).Split("data='𝄞 &amp; &lt;'>")[1].Split("</out>")[0]);
    }

    /// <summary>Oversized originals are not accepted merely because removal would make them smaller.</summary>
    [Fact]
    public void OriginalByteBound()
    {
        var template = Sheet("<!--PAD--><out>" + Instruction + "</out>");
        var overhead = Encoding.UTF8.GetByteCount(template.Replace("PAD", ""));
        var exact = template.Replace("PAD", new string('x', AuthoringCapabilities.MaxValueOfDeletionSourceBytes - overhead));
        var s = Inspect(exact); Assert.Equal(AuthoringCapabilities.MaxValueOfDeletionSourceBytes, s.ExportOriginal(Uri).Length);
        Delete(s);
        var tooLarge = Inspect(exact.Replace("<!--", "<!--x"));
        Assert.Equal(AuthoringDeletionFailureKind.SizeLimit, tooLarge.GetValueOfDeletionTarget(Find(tooLarge, "value-of")).Failure!.Kind);
    }

    /// <summary>Unsupported versions and originally invalid expressions cannot be repaired by deletion.</summary>
    /// <param name="version">The unsupported profile.</param>
    [Theory][InlineData("1.0")][InlineData("2.0")][InlineData("5.0")]
    public void UnsupportedProfiles(string version)
    {
        var s = Inspect(Sheet("<out>" + Instruction + "</out>", version));
        Assert.Equal(AuthoringDeletionFailureKind.UnsupportedProfile, s.GetValueOfDeletionTarget(Find(s, "value-of")).Failure!.Kind);
    }

    /// <summary>Non-XSLT same-name elements, non-elements and nested unsupported constructors refuse.</summary>
    [Fact]
    public void IneligibleNodes()
    {
        var s = Inspect(Sheet("<out><value-of/><xsl:if test='true()'>" + Instruction + "</xsl:if></out>"));
        Assert.Equal(AuthoringDeletionFailureKind.IneligibleNode, s.GetValueOfDeletionTarget(s.PrincipalModule.Root).Failure!.Kind);
        Assert.Equal(AuthoringDeletionFailureKind.IneligibleNode, s.GetValueOfDeletionTarget(Find(s, "value-of")).Failure!.Kind);
        var instruction = Nodes(s.PrincipalModule.Root).Single(n => n.ElementName?.NamespaceUri == Xsl && n.ElementName.LocalName == "value-of");
        Assert.Equal(AuthoringDeletionFailureKind.IneligibleNode, s.GetValueOfDeletionTarget(instruction).Failure!.Kind);
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
        Assert.False(s.GetValueOfDeletionTarget(Find(s, "value-of")).IsSuccess);
        Assert.Equal(before, resolver.Calls);
    }

    /// <summary>Concurrent proposals against immutable input produce independently owned snapshots.</summary>
    [Fact]
    public async Task ConcurrentProposals()
    {
        var s = Inspect(Sheet("<out>\n  " + Instruction + "\n</out>"));
        var proposal = new AuthoringValueOfDeletionProposal(s.GetValueOfDeletionTarget(Find(s, "value-of")).Target!);
        var results = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Task.Run(() => s.ProposeValueOfDeletion(proposal))));
        foreach (var result in results) { Assert.True(result.IsSuccess); Verify(result.Candidate!); }
        Assert.NotSame(results[0].Candidate!.Snapshot, results[1].Candidate!.Snapshot);
        Assert.Equal(results[0].Candidate!.ExportSource(), results[1].Candidate!.ExportSource());
    }

    /// <summary>The engine proof rejects ordinary candidate graphs with changed, missing or reordered evidence.</summary>
    /// <param name="changedBody">A candidate body that is not the exact approved deletion result.</param>
    [Theory]
    [InlineData("<out>\n \n</out>")]
    [InlineData("<out>\n  <!--extra-->\n</out>")]
    [InlineData("<out>\n  <extra/>\n</out>")]
    [InlineData("<out>\n  \n</out><extra/>")]
    public void RejectMalformedCandidateProof(string changedBody)
    {
        var input = Inspect(Sheet("<out>\n  " + Instruction + "\n</out>"));
        var bad = Inspect(Sheet(changedBody));
        var target = input.GetValueOfDeletionTarget(Find(input, "value-of")).Target!;
        var proof = new ValueOfDeletionPipeline.Proof(input.SourceFor(Uri), bad.SourceFor(Uri), target, default);
        Assert.False(proof.Match(input.PrincipalModule.Root, bad.PrincipalModule.Root));
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
}
