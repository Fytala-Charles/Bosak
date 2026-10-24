// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verify exact sibling exchange, total provenance, fresh selection and bounded refusals.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Collections;
using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>Engine exchange obligations; consumer Apply/history and native acceptance remain separate.</summary>
public sealed class AuthoringLiteralElementReorderingTests
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static readonly Uri Module = new("file:///C:/authoring/reorder.xsl");
    private static string Sheet(string body, string version = "3.0") => $"<s:stylesheet xmlns:s='{Xsl}' version='{version}'><s:output omit-xml-declaration='yes'/><s:template match='/'>{body}</s:template></s:stylesheet>";
    private static byte[] Encode(string text, int variant = 0)
    {
        Encoding encoding = variant switch { 0 => new UTF8Encoding(false, true), 1 => new UTF8Encoding(true, true), 2 => new UnicodeEncoding(false, true, true), _ => new UnicodeEncoding(true, true, true) };
        var declared = variant < 2 ? "utf-8" : variant == 2 ? "utf-16le" : "utf-16be";
        return encoding.GetPreamble().Concat(encoding.GetBytes($"<?xml version='1.0' encoding='{declared}'?>" + text)).ToArray();
    }
    private static AuthoringSnapshot Inspect(byte[] bytes, IAuthoringModuleResolver? resolver = null)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, Module, out var source, out var f), f?.Message);
        var result = XsltAuthoring.Inspect(source!, resolver, new() { AttemptCompilation = false });
        Assert.True(result.IsSuccess, result.Failure?.Message); return result.Snapshot!;
    }
    private static AuthoringSnapshot Inspect(string text) => Inspect(Encode(text));
    private static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor root) => new[] { root }.Concat(root.Children.SelectMany(Nodes));
    private static AuthoringNodeDescriptor Find(AuthoringSnapshot s, string name) => Nodes(s.PrincipalModule.Root).First(n => n.ElementName?.NamespaceUri == "" && n.ElementName.LocalName == name);
    private static AuthoringLiteralElementReorderCandidate Move(AuthoringSnapshot s, AuthoringNodeDescriptor node, AuthoringLiteralElementMoveDirection direction)
    {
        var target = s.GetLiteralElementReorderTarget(node, direction); Assert.Null(target.Failure); Assert.NotNull(target.Target);
        var result = s.ProposeLiteralElementReorder(new(target.Target!)); Assert.Null(result.Failure); Assert.Null(result.NoChange);
        Assert.Equal(AuthoringLiteralElementReorderOutcome.Changed, result.Outcome); Assert.NotNull(result.Candidate);
        Verify(result.Candidate!); return result.Candidate!;
    }
    private static byte[] Slice(byte[] bytes, SourceRange range) => bytes.AsSpan(checked((int)range.StartByteOffset), checked((int)range.ByteLength)).ToArray();
    private static void Verify(AuthoringLiteralElementReorderCandidate c)
    {
        var original = c.InputSnapshot.ExportOriginal(Module); var candidate = c.ExportSource();
        var edit = Assert.Single(c.SourceEdits);
        Assert.Equal(edit.OriginalRange.StartByteOffset, edit.ReplacementRange.StartByteOffset);
        Assert.Equal(edit.OriginalRange.ByteLength, edit.ReplacementRange.ByteLength);
        var replay = original.ToList(); replay.RemoveRange((int)edit.OriginalRange.StartByteOffset, (int)edit.OriginalRange.ByteLength);
        replay.InsertRange((int)edit.OriginalRange.StartByteOffset, Slice(candidate, edit.ReplacementRange)); Assert.Equal(candidate, replay.ToArray());
        var oldNodes = Nodes(c.InputSnapshot.PrincipalModule.Root).ToArray(); var freshNodes = Nodes(c.Snapshot.PrincipalModule.Root).ToArray();
        Assert.Equal(oldNodes.Length, c.NodeCorrespondence.Count); Assert.Equal(freshNodes.Length, c.NodeCorrespondence.Values.Distinct().Count());
        Assert.All(oldNodes, n => Assert.Contains(n, c.NodeCorrespondence.Keys)); Assert.All(freshNodes, n => Assert.Contains(n, c.NodeCorrespondence.Values));
        var exchange = c.Exchange;
        var originalParent = exchange.OriginalParent; var newParent = exchange.CandidateParent;
        var expected = originalParent.Children.Select(n => ReferenceEquals(n, exchange.OriginalEarlier) ? c.NodeCorrespondence[exchange.OriginalLater]
            : ReferenceEquals(n, exchange.OriginalLater) ? c.NodeCorrespondence[exchange.OriginalEarlier] : c.NodeCorrespondence[n]).ToArray();
        Assert.Equal(expected, newParent.Children);
        Assert.Same(c.CandidateSelection, c.NodeCorrespondence[c.OriginalNode]); Assert.Same(c.CandidateNeighbor, c.NodeCorrespondence[c.OriginalNeighbor]);
        foreach (var old in oldNodes)
        {
            var fresh = c.NodeCorrespondence[old]; Assert.Equal(old.Kind, fresh.Kind); Assert.Equal(old.ElementName, fresh.ElementName); Assert.Equal(old.Text, fresh.Text);
            if (!ReferenceEquals(old, originalParent)) Assert.Equal(old.Children.Select(n => c.NodeCorrespondence[n]), fresh.Children);
            var withinA = old.Range.StartByteOffset >= exchange.OriginalEarlier.Range.StartByteOffset && old.Range.StartByteOffset + old.Range.ByteLength <= exchange.OriginalEarlier.Range.StartByteOffset + exchange.OriginalEarlier.Range.ByteLength;
            var withinB = old.Range.StartByteOffset >= exchange.OriginalLater.Range.StartByteOffset && old.Range.StartByteOffset + old.Range.ByteLength <= exchange.OriginalLater.Range.StartByteOffset + exchange.OriginalLater.Range.ByteLength;
            long delta = withinA ? exchange.OriginalLater.Range.ByteLength + (exchange.OriginalSeparator?.Range.ByteLength ?? 0)
                : withinB ? -(exchange.OriginalEarlier.Range.ByteLength + (exchange.OriginalSeparator?.Range.ByteLength ?? 0))
                : ReferenceEquals(old, exchange.OriginalSeparator) ? exchange.OriginalLater.Range.ByteLength - exchange.OriginalEarlier.Range.ByteLength : 0;
            Assert.Equal(old.Range.StartByteOffset + delta, fresh.Range.StartByteOffset); Assert.Equal(old.Range.ByteLength, fresh.Range.ByteLength);
            var contains = old.Range.StartByteOffset <= exchange.OriginalEarlier.Range.StartByteOffset && old.Range.StartByteOffset + old.Range.ByteLength >= exchange.OriginalLater.Range.StartByteOffset + exchange.OriginalLater.Range.ByteLength;
            if (!contains) Assert.Equal(Slice(original, old.Range), Slice(candidate, fresh.Range));
            Assert.Equal(old.Attributes.Count, fresh.Attributes.Count);
        }
        Assert.Equal(oldNodes.Count(n => n.Kind == AuthoringNodeKind.Element), c.AttributeCorrespondence.Count);
        foreach (var group in c.AttributeCorrespondence)
        {
            Assert.Same(c.NodeCorrespondence[group.OriginalOwner], group.CandidateOwner);
            Assert.Equal(group.OriginalOwner.Attributes.Count, group.Survivors.Count);
            Assert.Equal(group.CandidateOwner.Attributes.Count, group.Survivors.Values.Distinct().Count());
            for (var i = 0; i < group.OriginalOwner.Attributes.Count; i++)
            {
                var a = group.OriginalOwner.Attributes[i]; var b = group.CandidateOwner.Attributes[i]; Assert.Same(b, group.Survivors[a]);
                Assert.Equal((a.Name,a.RawLiteral,a.ExpandedValue,a.SlotKind),(b.Name,b.RawLiteral,b.ExpandedValue,b.SlotKind));
                Assert.Equal(Slice(original,a.FullRange),Slice(candidate,b.FullRange)); Assert.Equal(Slice(original,a.ValueRange),Slice(candidate,b.ValueRange));
            }
        }
        Assert.Equal(original, c.InputSnapshot.ExportOriginal(Module)); Assert.NotEqual(original, candidate);
    }

    /// <summary>Both directions exchange full populated unequal subtrees under all strict encodings and separator shapes.</summary>
    /// <returns>The encoding/direction/separator matrix.</returns>
    public static IEnumerable<object[]> ExchangeCases()
    {
        for (var encoding = 0; encoding < 4; encoding++) foreach (var up in new[] { false, true }) foreach (var w in new[] { "", " \r\n\t ", "&#xD;\r\n&#xA; " }) yield return new object[] { encoding, up, w };
    }
    /// <summary>Proves exact complete exchange, changed order, byte translations and inverse movement.</summary>
    /// <param name="encoding">Strict encoding variant.</param>
    /// <param name="up">Whether to select the later element and move up.</param>
    /// <param name="separator">Exact retained separator source.</param>
    [Theory, MemberData(nameof(ExchangeCases))]
    public void Exact_exchange_preserves_every_owned_slice_and_selects_moved_element(int encoding, bool up, string separator)
    {
        const string a = "<item id='a' xmlns:q='urn:q'>é😀&amp;\r\n<!--keep--><?pi keep?><item/><s:value-of select='1+1'/><q:local/></item>";
        const string b = "<item id=\"b\"><s:text>B</s:text></item>";
        var text = Sheet("<parent before='yes'>\r\n" + a + separator + b + "\r\n<tail/></parent>", "4.0");
        var bytes = Encode(text, encoding); var s = Inspect(bytes); var parent = Find(s, "parent");
        var items = parent.Children.Where(n => n.ElementName?.LocalName == "item").ToArray();
        var c = Move(s, items[up ? 1 : 0], up ? AuthoringLiteralElementMoveDirection.Up : AuthoringLiteralElementMoveDirection.Down);
        Assert.Equal(Encode(text.Replace(a + separator + b, b + separator + a), encoding), c.ExportSource());
        Assert.Equal(up ? "b" : "a", c.CandidateSelection.Attributes.Single(x => x.Name == "id").ExpandedValue);
        var back = Move(c.Snapshot, c.CandidateSelection, up ? AuthoringLiteralElementMoveDirection.Down : AuthoringLiteralElementMoveDirection.Up);
        Assert.Equal(bytes, back.ExportSource());
    }

    /// <summary>Identical byte slices produce no candidate and preserve actual original selection.</summary>
    /// <param name="up">Captured direction.</param>
    /// <param name="encoding">Strict encoding variant.</param>
    [Theory, InlineData(false,0), InlineData(true,0), InlineData(false,1), InlineData(true,1), InlineData(false,2), InlineData(true,2), InlineData(false,3), InlineData(true,3)]
    public void Identical_siblings_return_original_selection_NoChange(bool up, int encoding)
    {
        var s = Inspect(Encode(Sheet("<parent><same><child a='v'/></same>\r\n<same><child a='v'/></same></parent>"), encoding));
        var items = Find(s,"parent").Children.Where(n => n.Kind == AuthoringNodeKind.Element).ToArray(); var node = items[up ? 1 : 0];
        var t = s.GetLiteralElementReorderTarget(node, up ? AuthoringLiteralElementMoveDirection.Up : AuthoringLiteralElementMoveDirection.Down).Target!;
        var r = s.ProposeLiteralElementReorder(new(t)); Assert.Equal(AuthoringLiteralElementReorderOutcome.NoChange,r.Outcome);
        Assert.Null(r.Candidate); Assert.Null(r.Failure); Assert.Same(t,r.NoChange!.Target); Assert.Same(node,r.NoChange.Target.Node);
    }

    /// <summary>Significant or intervening content is never skipped; unsupported contexts refuse.</summary>
    /// <param name="body">Template body.</param>
    [Theory]
    [InlineData("<parent><a/><!--stop--><b/></parent>")]
    [InlineData("<parent><a/><?stop now?><b/></parent>")]
    [InlineData("<parent><a/><s:text> </s:text><b/></parent>")]
    [InlineData("<parent><a/>meaningful<b/></parent>")]
    [InlineData("<parent><a/><foreign xmlns='urn:foreign'/><b/></parent>")]
    [InlineData("<parent><a/><![CDATA[ ]]><b/></parent>")]
    [InlineData("<parent xml:space='preserve'><a/> <b/></parent>")]
    [InlineData("<parent><a bad='{1}'/><b/></parent>")]
    [InlineData("<parent><a/><b><s:value-of select='('/></b></parent>")]
    [InlineData("<parent><a/><b s:use-when='false()'/></parent>")]
    [InlineData("<parent><a/><b s:expand-text='yes'/></parent>")]
    [InlineData("<parent><a/><b><s:evaluate xpath='.' /></b></parent>")]
    [InlineData("<parent><a/><b><s:result-document href='out.xml'/></b></parent>")]
    [InlineData("<parent><a/><b p:q='v' xmlns:p='urn:p'/></parent>")]
    [InlineData("<s:if test='true()'><parent><a/><b/></parent></s:if>")]
    [InlineData("<a/><b/>")]
    [InlineData("<parent xmlns='urn:p'><a xmlns=''/><b xmlns=''/></parent>")]
    public void Unsupported_shapes_never_return_partial_authority(string body)
    {
        var s=Inspect(Sheet(body)); var bytes=s.ExportOriginal(Module);
        var r=s.GetLiteralElementReorderTarget(Find(s,"a"),AuthoringLiteralElementMoveDirection.Down);
        Assert.Null(r.Target); Assert.False(r.IsSuccess); Assert.NotNull(r.Failure); Assert.Equal(bytes,s.ExportOriginal(Module));
    }

    /// <summary>True boundaries do not wrap, and empty whitespace at a boundary supplies no neighbor.</summary>
    /// <param name="up">Whether to test the first rather than last boundary.</param>
    [Theory, InlineData(true), InlineData(false)]
    public void Boundary_does_not_wrap(bool up)
    {
        var s=Inspect(Sheet("<parent>\n<a/>\n<b/>\n</parent>"));
        var r=s.GetLiteralElementReorderTarget(Find(s,up?"a":"b"),up?AuthoringLiteralElementMoveDirection.Up:AuthoringLiteralElementMoveDirection.Down);
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.UnavailableNeighbor,r.Failure!.Kind); Assert.Null(r.Target);
    }

    /// <summary>Foreign equal-byte descriptors/targets and stale descriptors cannot authorize movement.</summary>
    [Fact]
    public void Authority_uses_actual_snapshot_owned_descriptor_and_direction()
    {
        var s=Inspect(Sheet("<parent><a/><b/></parent>")); var foreign=Inspect(s.ExportOriginal(Module)); var node=Find(s,"a");
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.ForeignSnapshot,s.GetLiteralElementReorderTarget(Find(foreign,"a"),AuthoringLiteralElementMoveDirection.Down).Failure!.Kind);
        var target=s.GetLiteralElementReorderTarget(node,AuthoringLiteralElementMoveDirection.Down).Target!;
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.ForeignSnapshot,foreign.ProposeLiteralElementReorder(new(target)).Failure!.Kind);
        var c=Move(s,node,AuthoringLiteralElementMoveDirection.Down);
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.ForeignSnapshot,c.Snapshot.ProposeLiteralElementReorder(new(target)).Failure!.Kind);
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.ForeignSnapshot,c.Snapshot.GetLiteralElementReorderTarget(node,AuthoringLiteralElementMoveDirection.Up).Failure!.Kind);
        Assert.Equal(AuthoringLiteralElementMoveDirection.Down,c.Direction);
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.InvalidDirection,s.GetLiteralElementReorderTarget(node,(AuthoringLiteralElementMoveDirection)999).Failure!.Kind);
        Assert.Empty(typeof(AuthoringLiteralElementReorderTarget).GetConstructors()); Assert.Empty(typeof(AuthoringLiteralElementReorderCandidate).GetConstructors());
        Assert.Single(typeof(AuthoringLiteralElementReorderProposal).GetConstructors().Single().GetParameters());
    }

    /// <summary>Cancellation suppresses discovery, changed and NoChange authority.</summary>
    /// <param name="same">Whether byte equality would otherwise occur.</param>
    [Theory, InlineData(true), InlineData(false)]
    public void Cancellation_precedes_both_success_outcomes(bool same)
    {
        var s=Inspect(Sheet("<parent><a/>"+(same?"<a/>":"<b/>")+"</parent>")); var n=Find(s,"a");
        var target=s.GetLiteralElementReorderTarget(n,AuthoringLiteralElementMoveDirection.Down).Target!;
        var cancelled=new CancellationToken(true);
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.Cancelled,s.GetLiteralElementReorderTarget(n,AuthoringLiteralElementMoveDirection.Down,cancelled).Failure!.Kind);
        var result=s.ProposeLiteralElementReorder(new(target),cancelled); Assert.Null(result.Candidate); Assert.Null(result.NoChange);
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.Cancelled,result.Failure!.Kind);
    }

    /// <summary>Concurrent directions remain isolated and all public byte/collection exports are defensive.</summary>
    /// <returns>Completion of independent proposals.</returns>
    [Fact]
    public async Task Concurrent_proposals_and_exports_are_isolated()
    {
        var s=Inspect(Sheet("<parent><a/><b/><c/></parent>")); var bytes=s.ExportOriginal(Module); var n=Find(s,"b");
        var results=await Task.WhenAll(Task.Run(()=>Move(s,n,AuthoringLiteralElementMoveDirection.Up)),Task.Run(()=>Move(s,n,AuthoringLiteralElementMoveDirection.Down)));
        Assert.NotSame(results[0].Snapshot,results[1].Snapshot); Assert.Equal(bytes,s.ExportOriginal(Module));
        var c=results[0]; var expected=c.ExportSource(); var exported=c.ExportSource(); exported[0]^=0xff; Assert.Equal(expected,c.ExportSource());
        var range=c.SourceEdits[0].OriginalRange; var old=c.ExportOriginalRange(range); old[0]^=0xff; Assert.Equal(Slice(bytes,range),c.ExportOriginalRange(range));
        Assert.Throws<NotSupportedException>(()=>((IDictionary<AuthoringNodeDescriptor,AuthoringNodeDescriptor>)c.NodeCorrespondence).Clear());
        Assert.Throws<NotSupportedException>(()=>((IList)c.SourceEdits).Clear());
        Assert.Throws<NotSupportedException>(()=>((IList)c.AttributeCorrespondence).Clear());
        var attrs=c.AttributeCorrespondence.First(g=>g.Survivors.Count>0);
        Assert.Throws<NotSupportedException>(()=>((IDictionary<AuthoringAttributeDescriptor,AuthoringAttributeDescriptor>)attrs.Survivors).Clear());
        Assert.Throws<ArgumentOutOfRangeException>(()=>c.ExportOriginalRange(range with { ByteLength=long.MaxValue }));
    }

    /// <summary>Changed source is not NoChange merely because the current transform output is equal.</summary>
    [Fact]
    public void Equal_sample_output_is_still_Changed()
    {
        var text=Sheet("<visible/>").Replace("</s:stylesheet>","<s:template match='never'><parent><a/><b/></parent></s:template></s:stylesheet>");
        var s=Inspect(text); var c=Move(s,Find(s,"a"),AuthoringLiteralElementMoveDirection.Down);
        Assert.Equal(Run(s.ExportOriginal(Module)),Run(c.ExportSource()));
    }
    private static string Run(byte[] bytes)
    { using var stream=new MemoryStream(bytes); return new XsltCompiler().Compile(XDocument.Load(stream),Module.AbsoluteUri).TransformToString(new XDocumentNode(XDocument.Parse("<input/>"))); }

    /// <summary>Namespace rebinding/default resets use expanded identity and retain local declarations byte-exactly.</summary>
    [Fact]
    public void Namespace_alias_rebinding_and_default_reset_are_preserved()
    {
        var s=Inspect(Sheet("<outer xmlns='urn:outer'><parent xmlns=''><a xmlns:s='urn:literal'><s:child/></a>\n<b xmlns:q='urn:q'><q:child/></b></parent></outer>"));
        var c=Move(s,Find(s,"a"),AuthoringLiteralElementMoveDirection.Down); Assert.Equal("",c.CandidateSelection.ElementName!.NamespaceUri);
        Assert.Contains("<b xmlns:q='urn:q'><q:child/></b>\n<a xmlns:s='urn:literal'><s:child/></a>",Encoding.UTF8.GetString(c.ExportSource()));
    }

    /// <summary>Imported and engine-authored subtrees compose with both moves and existing lifecycle operations.</summary>
    [Fact]
    public void Create_populate_move_rename_move_delete_reopens_and_executes()
    {
        var s=Inspect(Sheet("<parent><existing/></parent>"));
        s=s.ProposeLiteralElementInsertion(new(s.GetLiteralElementInsertionTarget(Find(s,"parent")).Target!,"new")).Candidate!.Snapshot;
        s=s.ProposeLiteralAttributeCreation(new(s.GetLiteralAttributeCreationTarget(Find(s,"new")).Target!,"status","ready")).Candidate!.Snapshot;
        s=s.ProposeTextInsertion(new(s.GetTextInsertionTarget(Find(s,"new")).Target!,"body")).Candidate!.Snapshot;
        s=s.ProposeValueOfInsertion(new(s.GetValueOfInsertionTarget(Find(s,"new")).Target!,"1+1")).Candidate!.Snapshot;
        var moved=Move(s,Find(s,"new"),AuthoringLiteralElementMoveDirection.Up); var reopened=Inspect(moved.ExportSource());
        Assert.Equal("new",XDocument.Parse(Run(reopened.ExportOriginal(Module))).Root!.Elements().First().Name.LocalName);
        Assert.Equal("body2",XDocument.Parse(Run(moved.ExportSource())).Root!.Element("new")!.Value);
        var renamed=moved.Snapshot.ProposeLiteralElementRename(new(moved.Snapshot.GetLiteralElementRenameTarget(moved.CandidateSelection).Target!,"renamed")).Candidate!;
        var back=Move(renamed.Snapshot,renamed.CandidateSelection,AuthoringLiteralElementMoveDirection.Down);
        var deleted=back.Snapshot.ProposeLiteralElementDeletion(new(back.Snapshot.GetLiteralElementDeletionTarget(back.CandidateSelection).Target!)).Candidate!;
        Assert.Null(XDocument.Parse(Run(deleted.ExportSource())).Root!.Element("renamed"));
        Assert.Equal("existing",XDocument.Parse(Run(deleted.ExportSource())).Root!.Elements().Single().Name.LocalName);
    }

    /// <summary>Traversal, physical source and copied-evidence bounds refuse without partial authority.</summary>
    [Fact]
    public void Independent_work_and_evidence_limits_refuse()
    {
        var large=Inspect(Sheet("<parent><a>"+new string('x',3_000_000)+"</a><b/></parent>"));
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.SizeLimit,large.GetLiteralElementReorderTarget(Find(large,"a"),AuthoringLiteralElementMoveDirection.Down).Failure!.Kind);
        var oversized=Inspect(Sheet("<parent><a/><b/></parent><!--"+new string('x',8_388_608)+"-->"));
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.SizeLimit,oversized.GetLiteralElementReorderTarget(Find(oversized,"a"),AuthoringLiteralElementMoveDirection.Down).Failure!.Kind);
        var deep=Inspect(Sheet(string.Concat(Enumerable.Repeat("<n>",260))+"<parent><a/><b/></parent>"+string.Concat(Enumerable.Repeat("</n>",260))));
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.SizeLimit,deep.GetLiteralElementReorderTarget(Find(deep,"a"),AuthoringLiteralElementMoveDirection.Down).Failure!.Kind);
        var many=Inspect(Sheet("<parent><a/><b/>"+string.Concat(Enumerable.Repeat("<n/>",65_536))+"</parent>"));
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.SizeLimit,many.GetLiteralElementReorderTarget(Find(many,"a"),AuthoringLiteralElementMoveDirection.Down).Failure!.Kind);
    }

    /// <summary>Original invalidity is not repaired by moving, including byte-identical proposals.</summary>
    [Fact]
    public void Invalid_original_and_unsupported_versions_never_gain_authority()
    {
        var invalid=Inspect(Sheet("<parent><a/><a/></parent><s:sequence select='('/>"));
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.InvalidOriginal,invalid.GetLiteralElementReorderTarget(Find(invalid,"a"),AuthoringLiteralElementMoveDirection.Down).Failure!.Kind);
        foreach(var version in new[]{"1.0","2.0","5.0"})
        {
            var s=Inspect(Sheet("<parent><a/><b/></parent>",version)); Assert.Null(s.GetLiteralElementReorderTarget(Find(s,"a"),AuthoringLiteralElementMoveDirection.Down).Target);
        }
    }

    /// <summary>Discovery/proposal executes no resource-bearing expression and does not invoke the captured resolver.</summary>
    [Fact]
    public void Discovery_and_proposal_do_not_acquire_ambient_resources()
    {
        var resolver=new CountingResolver(); var bytes=Encode(Sheet("<parent><a><s:value-of select=\"doc('file:///C:/must-not-read.xml')\"/></a><b/></parent>"));
        var s=Inspect(bytes,resolver); var before=resolver.Calls; Move(s,Find(s,"a"),AuthoringLiteralElementMoveDirection.Down); Assert.Equal(before,resolver.Calls);
        var denied=Inspect(Encode(Sheet("<parent><a/><b s:use-when=\"doc('file:///C:/must-not-read.xml')\"/></parent>")),resolver);
        before=resolver.Calls; Assert.Null(denied.GetLiteralElementReorderTarget(Find(denied,"a"),AuthoringLiteralElementMoveDirection.Down).Target); Assert.Equal(before,resolver.Calls);
    }
    private sealed class CountingResolver : IAuthoringModuleResolver
    {
        internal int Calls { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri) { Calls++; throw new InvalidOperationException("Unexpected resource acquisition."); }
    }

    /// <summary>Capabilities are separate and null arguments follow established public conventions.</summary>
    [Fact]
    public void Public_capabilities_and_null_contracts_are_explicit()
    {
        Assert.True(AuthoringCapabilities.SupportsLiteralElementReordering); Assert.Equal("LiteralSiblingExchangeV1",AuthoringCapabilities.LiteralElementReorderProfile);
        Assert.Equal(8_388_608,AuthoringCapabilities.MaxLiteralElementReorderSourceBytes); Assert.Equal(8_388_608,AuthoringCapabilities.MaxLiteralElementReorderExchangeBytes);
        Assert.Equal(16_777_216,AuthoringCapabilities.MaxLiteralElementReorderEvidenceBytes); Assert.Equal(65_536,AuthoringCapabilities.MaxLiteralElementReorderNodes);
        Assert.Equal(65_536,AuthoringCapabilities.MaxLiteralElementReorderAttributes); Assert.Equal(256,AuthoringCapabilities.MaxLiteralElementReorderDepth);
        Assert.Throws<NotSupportedException>(()=>((IList)AuthoringCapabilities.LiteralElementReorderSupportedEncodings).Clear());
        var s=Inspect(Sheet("<parent><a/><b/></parent>"));
        Assert.Throws<ArgumentNullException>(()=>s.GetLiteralElementReorderTarget(null!,AuthoringLiteralElementMoveDirection.Down));
        Assert.Throws<ArgumentNullException>(()=>s.ProposeLiteralElementReorder(null!)); Assert.Throws<ArgumentNullException>(()=>new AuthoringLiteralElementReorderProposal(null!));
    }

    /// <summary>Equal logical subtrees with different lexical forms remain a Changed exchange.</summary>
    [Fact]
    public void Paired_and_self_closing_equal_output_are_Changed()
    {
        var s=Inspect(Sheet("<parent><same/><same></same></parent>"));
        var c=Move(s,Find(s,"same"),AuthoringLiteralElementMoveDirection.Down);
        Assert.Contains("<parent><same></same><same/></parent>",Encoding.UTF8.GetString(c.ExportSource()));
        Assert.Equal(Run(s.ExportOriginal(Module)),Run(c.ExportSource()));
    }

    /// <summary>Broader inspection encodings do not grant mutation eligibility.</summary>
    /// <param name="variant">Unsupported inspection encoding variant.</param>
    [Theory, InlineData(0), InlineData(1), InlineData(2)]
    public void Inspection_only_encodings_refuse_reordering(int variant)
    {
        Encoding e=variant==0?Encoding.ASCII:variant==1?Encoding.Latin1:new UTF32Encoding(false,true,true);
        var declaration=variant==0?"us-ascii":variant==1?"iso-8859-1":"utf-32";
        var bytes=e.GetPreamble().Concat(e.GetBytes($"<?xml version='1.0' encoding='{declaration}'?>"+Sheet("<parent><a/><b/></parent>"))).ToArray();
        var s=Inspect(bytes); var r=s.GetLiteralElementReorderTarget(Find(s,"a"),AuthoringLiteralElementMoveDirection.Down);
        Assert.Equal(AuthoringLiteralElementReorderFailureKind.UnsupportedEncoding,r.Failure!.Kind);
    }

    /// <summary>The proof refuses wrong order, missing nodes, extra nodes and attribute/value changes.</summary>
    /// <param name="candidateBody">Intentionally invalid exchange result.</param>
    [Theory]
    [InlineData("<parent><a k='1'/><b/></parent>")]
    [InlineData("<parent><b/></parent>")]
    [InlineData("<parent><b/><a k='1'/><extra/></parent>")]
    [InlineData("<parent><b/><a k='2'/></parent>")]
    [InlineData("<parent><b/><a/></parent>")]
    public void Internal_proof_rejects_incomplete_or_modified_exchange(string candidateBody)
    {
        var original=Inspect(Sheet("<parent><a k='1'/><b/></parent>")); var fresh=Inspect(Sheet(candidateBody));
        var target=original.GetLiteralElementReorderTarget(Find(original,"a"),AuthoringLiteralElementMoveDirection.Down).Target!;
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        var data=typeof(AuthoringLiteralElementReorderTarget).GetProperty("Data",flags)!.GetValue(target);
        var type=typeof(AuthoringSnapshot).Assembly.GetType("Bosak.Xslt.Authoring.LiteralElementReorderPipeline+Proof")!;
        Assert.True(AuthoringSource.TryCreate(original.ExportOriginal(Module),Module,out var oldSource,out _));
        Assert.True(AuthoringSource.TryCreate(fresh.ExportOriginal(Module),Module,out var newSource,out _));
        var proof=Activator.CreateInstance(type,flags|System.Reflection.BindingFlags.Public,null,new object?[]{oldSource,newSource,data,CancellationToken.None},null)!;
        Assert.False((bool)type.GetMethod("Match",flags)!.Invoke(proof,new object[]{original.PrincipalModule.Root,fresh.PrincipalModule.Root})!);
    }
}
