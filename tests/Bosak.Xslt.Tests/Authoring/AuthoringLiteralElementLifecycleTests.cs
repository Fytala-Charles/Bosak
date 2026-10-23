// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 October 2026
// PURPOSE              : Verify literal element lifecycle bytes, complete identity partitions and refusals.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>REQ-136 engine obligations; consumer history and native acceptance remain separate.</summary>
public sealed class AuthoringLiteralElementLifecycleTests
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static readonly Uri Module = new("file:///C:/authoring/elements.xsl");
    private static string Sheet(string body, string version = "3.0") => $"<s:stylesheet xmlns:s='{Xsl}' version='{version}'><s:output omit-xml-declaration='yes'/><s:template match='/'>{body}</s:template></s:stylesheet>";
    private static Encoding EncodingFor(int variant) => variant switch { 0 => new UTF8Encoding(false, true), 1 => new UTF8Encoding(true, true), 2 => new UnicodeEncoding(false, true, true), _ => new UnicodeEncoding(true, true, true) };
    private static byte[] Encode(string text, int variant)
    {
        var encoding = EncodingFor(variant); var declared = variant < 2 ? "utf-8" : variant == 2 ? "utf-16le" : "utf-16be";
        return encoding.GetPreamble().Concat(encoding.GetBytes($"<?xml version='1.0' encoding='{declared}'?>" + text)).ToArray();
    }
    private static AuthoringSnapshot Inspect(byte[] bytes, IAuthoringModuleResolver? resolver = null)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, Module, out var source, out var f), f?.Message);
        var r = XsltAuthoring.Inspect(source!, resolver, new() { AttemptCompilation = false });
        Assert.True(r.IsSuccess, r.Failure?.Message); return r.Snapshot!;
    }
    private static AuthoringSnapshot Inspect(string text) => Inspect(Encoding.UTF8.GetBytes(text));
    private static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor n) => new[] { n }.Concat(n.Children.SelectMany(Nodes));
    private static AuthoringNodeDescriptor Find(AuthoringSnapshot s, string local = "out") => Nodes(s.PrincipalModule.Root).First(n => n.ElementName?.LocalName == local && n.ElementName.NamespaceUri.Length == 0);
    private static AuthoringLiteralElementCandidate Rename(AuthoringSnapshot s, string name)
    {
        var target = s.GetLiteralElementRenameTarget(Find(s)); Assert.Null(target.Failure); Assert.NotNull(target.Target);
        var r = s.ProposeLiteralElementRename(new(target.Target!, name)); Assert.Null(r.Failure); Assert.Equal(AuthoringLiteralElementRenameOutcome.Changed, r.Outcome);
        Verify(r.Candidate!); return r.Candidate!;
    }
    private static AuthoringLiteralElementCandidate Delete(AuthoringSnapshot s)
    {
        var target = s.GetLiteralElementDeletionTarget(Find(s)); Assert.Null(target.Failure); Assert.NotNull(target.Target);
        var r = s.ProposeLiteralElementDeletion(new(target.Target!)); Assert.Null(r.Failure); Assert.NotNull(r.Candidate);
        Verify(r.Candidate!); return r.Candidate!;
    }
    private static void Verify(AuthoringLiteralElementCandidate c)
    {
        var original = c.InputSnapshot.ExportOriginal(c.ModuleUri); var candidate = c.ExportSource();
        var replay = original.ToList(); long delta = 0; long previousEnd = 0;
        foreach (var e in c.SourceEdits)
        {
            Assert.True(e.OriginalRange.StartByteOffset >= previousEnd);
            Assert.Equal(e.OriginalRange.StartByteOffset + delta, e.ReplacementRange.StartByteOffset);
            delta += e.ReplacementRange.ByteLength - e.OriginalRange.ByteLength;
            previousEnd = e.OriginalRange.StartByteOffset + e.OriginalRange.ByteLength;
        }
        foreach (var e in c.SourceEdits.Reverse())
        {
            var oldAt = checked((int)e.OriginalRange.StartByteOffset); var newAt = checked((int)e.ReplacementRange.StartByteOffset);
            replay.RemoveRange(oldAt, checked((int)e.OriginalRange.ByteLength));
            replay.InsertRange(oldAt, candidate.AsSpan(newAt, checked((int)e.ReplacementRange.ByteLength)).ToArray());
        }
        Assert.Equal(candidate, replay.ToArray());
        var olds = Nodes(c.InputSnapshot.PrincipalModule.Root).ToArray(); var news = Nodes(c.Snapshot.PrincipalModule.Root).ToArray();
        var contributors = c.MergedTextGroups.SelectMany(g => g.Fragments.Select(f => f.OriginalNode)).ToArray();
        var originalPartition = c.NodeCorrespondence.Keys.Concat(c.RemovedNodes).Concat(contributors).ToArray();
        var candidatePartition = c.NodeCorrespondence.Values.Concat(c.MergedTextGroups.Select(g => g.CandidateNode)).ToArray();
        Assert.Equal(olds.Length, originalPartition.Length); Assert.Equal(olds.Length, originalPartition.Distinct().Count());
        Assert.All(olds, n => Assert.Contains(n, originalPartition));
        Assert.Equal(news.Length, candidatePartition.Length); Assert.Equal(news.Length, candidatePartition.Distinct().Count());
        Assert.All(news, n => Assert.Contains(n, candidatePartition));
        Assert.Equal(c.RemovedNodes.SelectMany(n => n.Attributes), c.RemovedAttributes);
        Assert.Equal(c.NodeCorrespondence.Keys.Count(n => n.Kind == AuthoringNodeKind.Element), c.AttributeCorrespondence.Count);
        foreach (var a in c.AttributeCorrespondence)
        {
            Assert.Same(c.NodeCorrespondence[a.OriginalOwner], a.CandidateOwner);
            Assert.Equal(a.OriginalOwner.Attributes, a.Survivors.Keys); Assert.Equal(a.CandidateOwner.Attributes, a.Survivors.Values);
        }
        foreach (var g in c.MergedTextGroups)
        {
            Assert.Equal(2, g.Fragments.Count); long end = g.CandidateNode.Range.StartByteOffset;
            foreach (var f in g.Fragments)
            {
                Assert.Equal(end, f.CandidateRange.StartByteOffset); end += f.CandidateRange.ByteLength;
                Assert.Equal(c.ExportOriginalRange(f.OriginalRange), candidate.AsSpan((int)f.CandidateRange.StartByteOffset, (int)f.CandidateRange.ByteLength).ToArray());
            }
            Assert.Equal(g.CandidateNode.Range.StartByteOffset + g.CandidateNode.Range.ByteLength, end);
        }
        var copy = c.ExportSource(); copy[0] ^= 1; Assert.Equal(candidate, c.ExportSource());
        Assert.Throws<NotSupportedException>(() => ((IDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>)c.NodeCorrespondence).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<AuthoringNodeDescriptor>)c.RemovedNodes).Clear());
    }

    /// <summary>Exercises lexical tokens, encodings and populated removal under both retained versions.</summary>
    /// <returns>Encoding, effective version and lexical form.</returns>
    public static IEnumerable<object[]> Matrix()
    {
        for (var encoding = 0; encoding < 4; encoding++) foreach (var version in new[] { "3.0", "4.0" }) foreach (var self in new[] { false, true }) yield return new object[] { encoding, version, self };
    }
    /// <summary>Only addressed name tokens change across imported lexical forms.</summary>
    /// <param name="encoding">Retained encoding variant.</param>
    /// <param name="version">Explicit stylesheet version.</param>
    /// <param name="self">Whether the imported target is self-closing.</param>
    [Theory, MemberData(nameof(Matrix))]
    public void Rename_replays_exact_name_tokens(int encoding, string version, bool self)
    {
        var element = self ? "<out a = 'out &amp; {{value}}' />" : "<out a = 'out &amp; {{value}}'><out/><!-- </out> --><?p out?><s:text>out</s:text></out  >";
        var input = Encode(Sheet("<wrapper>" + element + "<after/></wrapper>", version), encoding);
        var s = Inspect(input); var c = Rename(s, "Élément");
        Assert.Equal(self ? 1 : 2, c.SourceEdits.Count); Assert.Empty(c.RemovedNodes); Assert.Empty(c.MergedTextGroups);
        Assert.Equal("Élément", c.CandidateSelection.ElementName!.LocalName);
        var expected = element.Replace("<out a", "<Élément a").Replace("</out  >", "</Élément  >");
        Assert.Equal(Encode(Sheet("<wrapper>" + expected + "<after/></wrapper>", version), encoding), c.ExportSource());
        Assert.Equal(input, s.ExportOriginal(Module));
    }
    /// <summary>Deletes the entire owned subtree, preserving outside whitespace and paired parents.</summary>
    /// <param name="encoding">Retained encoding variant.</param>
    /// <param name="version">Explicit stylesheet version.</param>
    /// <param name="self">Whether the selected element is self-closing.</param>
    [Theory, MemberData(nameof(Matrix))]
    public void Populated_delete_partitions_every_descriptor_and_attribute(int encoding, string version, bool self)
    {
        var element = self ? "<out a='x'/>" : "<out a='x'><nested b='2'><s:value-of select='1+1'/></nested>text<!--c--><?p v?><s:text>x</s:text></out>";
        var s = Inspect(Encode(Sheet("<wrapper>\r\n " + element + "\r\n <after/></wrapper>", version), encoding));
        var c = Delete(s); Assert.Single(c.SourceEdits); Assert.Single(c.MergedTextGroups);
        Assert.Equal(Nodes(Find(s)).ToArray(), c.RemovedNodes);
        Assert.Equal("wrapper", c.CandidateSelection.ElementName!.LocalName);
        Assert.Equal(Encode(Sheet("<wrapper>\r\n \r\n <after/></wrapper>", version), encoding), c.ExportSource());
    }
    /// <summary>NCNames are exact ordinal Unicode intent, including combining marks.</summary>
    /// <param name="name">A valid local name.</param>
    [Theory]
    [InlineData("Out"), InlineData("outé"), InlineData("e\u0301"), InlineData("text"), InlineData("value-of"), InlineData("名")]
    public void Rename_does_not_normalize_or_misclassify_literal_names(string name)
    { var c = Rename(Inspect(Sheet("<out/>")), name); Assert.Equal(name, c.CandidateSelection.ElementName!.LocalName); }
    /// <summary>Illegal, normalized or prefixed substitutes are never accepted.</summary>
    /// <param name="name">Invalid exact intent.</param>
    [Theory]
    [InlineData(""), InlineData(" out"), InlineData("out "), InlineData("p:out"), InlineData("1out"), InlineData("a/b"), InlineData("<x>"), InlineData("a\0")]
    public void Invalid_names_refuse_without_candidate(string name)
    {
        var s = Inspect(Sheet("<out/>")); var target = s.GetLiteralElementRenameTarget(Find(s)).Target!;
        var r = s.ProposeLiteralElementRename(new(target, name)); Assert.Null(r.Candidate); Assert.Null(r.NoChange);
        Assert.Equal(AuthoringLiteralElementFailureKind.InvalidName, r.Failure!.Kind);
    }
    /// <summary>Equal names have no candidate or changes after eligibility has been proven.</summary>
    [Fact]
    public void NoChange_is_exclusive_and_cancellable()
    {
        var s = Inspect(Sheet("<out/>", "4.0")); var target = s.GetLiteralElementRenameTarget(Find(s)).Target!;
        var r = s.ProposeLiteralElementRename(new(target, "out")); Assert.Equal(AuthoringLiteralElementRenameOutcome.NoChange, r.Outcome);
        Assert.NotNull(r.NoChange); Assert.Null(r.Candidate); Assert.Null(r.Failure);
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        var stopped = s.ProposeLiteralElementRename(new(target, "out"), cancel.Token);
        Assert.Null(stopped.NoChange); Assert.Equal(AuthoringLiteralElementFailureKind.Cancelled, stopped.Failure!.Kind);
    }
    /// <summary>Equal-byte snapshots and equal numeric IDs confer no authority.</summary>
    [Fact]
    public void Foreign_authority_refuses_both_operations()
    {
        var s = Inspect(Sheet("<out/>")); var other = Inspect(Sheet("<out/>"));
        Assert.Equal(Find(s).Id, Find(other).Id);
        Assert.Equal(AuthoringLiteralElementFailureKind.ForeignSnapshot, s.GetLiteralElementRenameTarget(Find(other)).Failure!.Kind);
        Assert.Equal(AuthoringLiteralElementFailureKind.ForeignSnapshot, s.GetLiteralElementDeletionTarget(Find(other)).Failure!.Kind);
        Assert.Equal(AuthoringLiteralElementFailureKind.ForeignSnapshot, other.ProposeLiteralElementRename(new(s.GetLiteralElementRenameTarget(Find(s)).Target!, "new")).Failure!.Kind);
        Assert.Equal(AuthoringLiteralElementFailureKind.ForeignSnapshot, other.ProposeLiteralElementDeletion(new(s.GetLiteralElementDeletionTarget(Find(s)).Target!)).Failure!.Kind);
    }
    /// <summary>Whole-module exclusions refuse before original repair or equal-name success.</summary>
    /// <param name="body">A well-formed module body outside the reviewed profile.</param>
    [Theory]
    [InlineData("<out a='{1}'/>"), InlineData("<out xml:space='preserve'/>"), InlineData("<out><![CDATA[text]]></out>"), InlineData("<s:if test='true()'><out/></s:if>"), InlineData("<out/><s:result-document href='file:///C:/must-not-write.xml'><x/></s:result-document>"), InlineData("<out/><s:evaluate xpath='1'/>"), InlineData("<out/><s:value-of select='missing()'/>"), InlineData("<out/><s:text disable-output-escaping='yes'>x</s:text>"), InlineData("<out s:expand-text='yes'/>")]
    public void Unsupported_originals_cannot_be_repaired_by_deletion(string body)
    {
        var s = Inspect(Sheet(body)); var n = Find(s);
        Assert.Null(s.GetLiteralElementRenameTarget(n).Target); Assert.Null(s.GetLiteralElementDeletionTarget(n).Target);
    }
    /// <summary>Only ordinary whitespace boundary merges are permitted.</summary>
    /// <param name="left">Left adjacent text.</param>
    /// <param name="right">Right adjacent text.</param>
    [Theory]
    [InlineData("a", "b"), InlineData(" ", "b"), InlineData("a", " ")]
    public void Nonwhitespace_boundary_merges_refuse(string left, string right)
    {
        var s = Inspect(Sheet("<wrapper>" + left + "<out><child/></out>" + right + "</wrapper>"));
        var t = s.GetLiteralElementDeletionTarget(Find(s)); Assert.NotNull(t.Target);
        var r = s.ProposeLiteralElementDeletion(new(t.Target!)); Assert.Null(r.Candidate); Assert.Equal(AuthoringLiteralElementFailureKind.UnsupportedCorrespondence, r.Failure!.Kind);
    }
    /// <summary>Physical name bytes, UTF-16 units and graph depth have independent bounds.</summary>
    [Fact]
    public void Independent_name_and_depth_limits_refuse()
    {
        var s = Inspect(Encode(Sheet("<out/>"), 2)); var t = s.GetLiteralElementRenameTarget(Find(s)).Target!;
        Assert.Equal(AuthoringLiteralElementFailureKind.SizeLimit, s.ProposeLiteralElementRename(new(t, new string('a', 1025))).Failure!.Kind);
        Assert.Equal(AuthoringLiteralElementFailureKind.SizeLimit, s.ProposeLiteralElementRename(new(t, new string('é', 513))).Failure!.Kind);
        var deep = Inspect(Sheet(string.Concat(Enumerable.Repeat("<n>", 256)) + "<out/>" + string.Concat(Enumerable.Repeat("</n>", 256))));
        Assert.Equal(AuthoringLiteralElementFailureKind.SizeLimit, deep.GetLiteralElementDeletionTarget(Find(deep)).Failure!.Kind);
    }
    /// <summary>Malformed Unicode and physical source/count/evidence limits refuse independently.</summary>
    [Fact]
    public void Malformed_unicode_and_independent_source_graph_evidence_bounds()
    {
        var s = Inspect(Sheet("<out/>")); var t = s.GetLiteralElementRenameTarget(Find(s)).Target!;
        Assert.Equal(AuthoringLiteralElementFailureKind.InvalidName, s.ProposeLiteralElementRename(new(t, new string((char)0xd800, 1))).Failure!.Kind);
        var oversized = Inspect(Sheet("<out/>" + new string(' ', 8_388_608)));
        Assert.Equal(AuthoringLiteralElementFailureKind.SizeLimit, oversized.GetLiteralElementRenameTarget(Find(oversized)).Failure!.Kind);
        var many = Inspect(Sheet("<out/>" + string.Concat(Enumerable.Repeat("<n/>", 65_536))));
        Assert.Equal(AuthoringLiteralElementFailureKind.SizeLimit, many.GetLiteralElementDeletionTarget(Find(many)).Failure!.Kind);
        var large = Inspect(Sheet("<out>" + new string('x', 4_300_000) + "</out>"));
        var lt = large.GetLiteralElementRenameTarget(Find(large)); Assert.NotNull(lt.Target);
        Assert.Equal(AuthoringLiteralElementFailureKind.SizeLimit, large.ProposeLiteralElementRename(new(lt.Target!, "new")).Failure!.Kind);
    }
    /// <summary>Removing the only populated child preserves the explicit paired parent form.</summary>
    [Fact]
    public void Removing_only_child_retains_paired_parent()
    {
        var s = Inspect(Sheet("<wrapper><out><s:if test='false()'><hidden/></s:if></out></wrapper>"));
        var c = Delete(s); Assert.Empty(c.MergedTextGroups);
        Assert.Contains("<wrapper></wrapper>", Encoding.UTF8.GetString(c.ExportSource()));
        // Both source mutations remain explicit Changed operations.
        var renamed = Rename(s, "renamed"); Assert.NotEqual(s.ExportOriginal(Module), renamed.ExportSource());
    }
    /// <summary>Changed intent is independent of output equivalence for the current input.</summary>
    /// <param name="rename">Whether to rename rather than delete the unused constructor.</param>
    [Theory, InlineData(true), InlineData(false)]
    public void Unchanged_sample_output_does_not_turn_source_mutation_into_NoChange(bool rename)
    {
        var sheet = Sheet("<visible/>").Replace("</s:stylesheet>", "<s:template match='never'><out><s:text>unused</s:text></out></s:template></s:stylesheet>");
        var s = Inspect(sheet); var c = rename ? Rename(s, "renamed") : Delete(s);
        string Run(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            return new XsltCompiler().Compile(XDocument.Load(stream), Module.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse("<input/>")));
        }
        Assert.Equal(Run(s.ExportOriginal(Module)), Run(c.ExportSource()));
        Assert.NotEqual(s.ExportOriginal(Module), c.ExportSource());
        Assert.Equal(rename ? AuthoringLiteralElementOperation.Rename : AuthoringLiteralElementOperation.Delete, c.Operation);
    }
    /// <summary>Zero acquisition is observable through the captured module resolver for refused and supported work.</summary>
    [Fact]
    public void Module_resolver_is_not_called_by_discovery_or_proposals()
    {
        var resolver = new CountingResolver(); var s = Inspect(Encoding.UTF8.GetBytes(Sheet("<out><s:value-of select=\"doc('file:///C:/must-not-read.xml')\"/></out>")), resolver);
        var before = resolver.Calls;
        var renamed = Rename(s, "renamed"); Delete(s);
        Assert.Equal(before, resolver.Calls);
        var unsafeSource = Inspect(Encoding.UTF8.GetBytes(Sheet("<out s:use-when=\"doc('file:///C:/must-not-read.xml')\"/>")), resolver);
        before = resolver.Calls; Assert.Null(unsafeSource.GetLiteralElementRenameTarget(Find(unsafeSource)).Target); Assert.Null(unsafeSource.GetLiteralElementDeletionTarget(Find(unsafeSource)).Target);
        Assert.Equal(before, resolver.Calls);
    }
    private sealed class CountingResolver : IAuthoringModuleResolver
    {
        internal int Calls { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) { Calls++; throw new InvalidOperationException("Unexpected ambient module acquisition."); }
    }
    /// <summary>Whitespace entity spelling is retained while ordinary XML inspection owns merged values.</summary>
    /// <param name="encoding">Retained encoding variant.</param>
    [Theory, InlineData(0), InlineData(1), InlineData(2), InlineData(3)]
    public void Entity_and_crlf_boundary_fragments_are_not_logically_concatenated(int encoding)
    {
        var s = Inspect(Encode(Sheet("<wrapper>&#xD;\r\n<out><nested/></out>&#xA; </wrapper>"), encoding));
        var c = Delete(s); Assert.Single(c.MergedTextGroups);
        var g = c.MergedTextGroups[0]; Assert.Equal("\r\n\n ", g.CandidateNode.Text);
        Assert.Equal(Encode(Sheet("<wrapper>&#xD;\r\n&#xA; </wrapper>"), encoding), c.ExportSource());
    }
    /// <summary>Namespace aliases and default resets use expanded identity.</summary>
    [Fact]
    public void Namespace_rebinding_and_default_reset_preserve_exact_bytes()
    {
        var s = Inspect(Sheet("<parent xmlns='urn:result'><out xmlns='' xmlns:s='urn:literal'><s:child a='constant'/></out></parent>"));
        var c = Rename(s, "renamed"); Assert.Equal("", c.CandidateSelection.ElementName!.NamespaceUri);
        var deleted = Delete(s); Assert.Contains("<parent xmlns='urn:result'></parent>", Encoding.UTF8.GetString(deleted.ExportSource()));
    }
    /// <summary>Concurrent proposals neither mutate the snapshot nor share caller-owned arrays.</summary>
    /// <returns>Completion of both proposals.</returns>
    [Fact]
    public async Task Concurrent_proposals_are_independent()
    {
        var s = Inspect(Sheet("<out><child/></out>")); var original = s.ExportOriginal(Module);
        var results = await Task.WhenAll(Task.Run(() => Rename(s, "first")), Task.Run(() => Rename(s, "second")), Task.Run(() => Delete(s)));
        Assert.Equal(original, s.ExportOriginal(Module)); Assert.NotSame(results[0].Snapshot, results[1].Snapshot);
        Assert.Equal("first", results[0].CandidateSelection.ElementName!.LocalName); Assert.Equal("second", results[1].CandidateSelection.ElementName!.LocalName);
    }
    /// <summary>Engine-created elements retain authority through attribute, text, Rename and Delete revisions.</summary>
    [Fact]
    public void Create_populate_rename_delete_create_uses_fresh_descriptors()
    {
        var original = Inspect(Sheet("<wrapper></wrapper>")); var parent = Find(original, "wrapper");
        var created = original.ProposeLiteralElementInsertion(new(original.GetLiteralElementInsertionTarget(parent).Target!, "out")).Candidate!;
        var s = created.Snapshot;
        var attr = s.ProposeLiteralAttributeCreation(new(s.GetLiteralAttributeCreationTarget(Find(s)).Target!, "status", "ready")).Candidate!;
        s = attr.Snapshot;
        var text = s.ProposeTextInsertion(new(s.GetTextInsertionTarget(Find(s)).Target!, "body")).Candidate!;
        s = text.Snapshot;
        var value = s.ProposeValueOfInsertion(new(s.GetValueOfInsertionTarget(Find(s)).Target!, "1+1")).Candidate!;
        s = value.Snapshot; var renamed = Rename(s, "renamed");
        var selected = renamed.CandidateSelection;
        var deleted = renamed.Snapshot.ProposeLiteralElementDeletion(new(renamed.Snapshot.GetLiteralElementDeletionTarget(selected).Target!)).Candidate!;
        Verify(deleted);
        Assert.Equal("wrapper", deleted.CandidateSelection.ElementName!.LocalName);
        var again = deleted.Snapshot.ProposeLiteralElementInsertion(new(deleted.Snapshot.GetLiteralElementInsertionTarget(deleted.CandidateSelection).Target!, "out"));
        Assert.True(again.IsSuccess, again.Failure?.Message);
        using var stream = new MemoryStream(renamed.ExportSource());
        var executable = new XsltCompiler().Compile(XDocument.Load(stream), Module.AbsoluteUri);
        var output = executable.TransformToString(new XDocumentNode(XDocument.Parse("<input/>")));
        Assert.Equal("body2", XDocument.Parse(output).Root!.Element("renamed")!.Value);
    }
}
