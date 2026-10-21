// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 October 2026
// PURPOSE              : Verify public literal attribute lifecycle, exact bytes and runtime values.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 20-10-2026     | Complete asynchronous test return documentation.                                         |
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>Public lifecycle checks for the complete reviewed REQ-135 feature.</summary>
public partial class AuthoringLiteralAttributeLifecycleTests
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static readonly Uri Module = new("file:///C:/authoring/attributes.xsl");
    private static string Sheet(string body, string extra = "", string version = "3.0") =>
        $"<xsl:stylesheet xmlns:xsl='{Xsl}' version='{version}' {extra}><xsl:output omit-xml-declaration='yes'/><xsl:template match='/'>{body}</xsl:template></xsl:stylesheet>";
    private static byte[] Encode(string text, int variant)
    {
        Encoding encoding = variant switch { 0 => new UTF8Encoding(false, true), 1 => new UTF8Encoding(true, true), 2 => new UnicodeEncoding(false, true, true), _ => new UnicodeEncoding(true, true, true) };
        var declaration = variant < 2 ? "utf-8" : variant == 2 ? "utf-16le" : "utf-16be";
        return encoding.GetPreamble().Concat(encoding.GetBytes($"<?xml version='1.0' encoding='{declaration}'?>" + text)).ToArray();
    }
    private static AuthoringSnapshot Inspect(byte[] bytes)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, Module, out var source, out var failure), failure?.Message);
        var inspected = XsltAuthoring.Inspect(source!, options: new() { AttemptCompilation = false });
        Assert.True(inspected.IsSuccess, inspected.Failure?.Message); return inspected.Snapshot!;
    }
    private static AuthoringSnapshot Inspect(string text) => Inspect(Encoding.UTF8.GetBytes(text));
    private static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor n) => new[] { n }.Concat(n.Children.SelectMany(Nodes));
    private static AuthoringNodeDescriptor Owner(AuthoringSnapshot s) => Nodes(s.PrincipalModule.Root).First(n => n.ElementName?.LocalName == "out");
    private static AuthoringAttributeDescriptor Attribute(AuthoringSnapshot s, string name = "status") => Owner(s).Attributes.Single(a => a.Name == name);
    private static AuthoringLiteralAttributeCandidate Create(AuthoringSnapshot s, string value, string name = "status")
    {
        var target = s.GetLiteralAttributeCreationTarget(Owner(s)); Assert.True(target.IsSuccess, target.Failure?.Message);
        var r = s.ProposeLiteralAttributeCreation(new(target.Target!, name, value)); Assert.True(r.IsSuccess, r.Failure?.Message); Verify(r.Candidate!); return r.Candidate!;
    }
    private static AuthoringLiteralAttributeCandidate Update(AuthoringSnapshot s, string value)
    {
        var t = s.GetLiteralAttributeUpdateTarget(Owner(s), Attribute(s)); Assert.True(t.IsSuccess, t.Failure?.Message);
        var r = s.ProposeLiteralAttributeUpdate(new(t.Target!, value)); Assert.Equal(AuthoringLiteralAttributeUpdateOutcome.Changed, r.Outcome); Assert.Null(r.NoChange); Verify(r.Candidate!); return r.Candidate!;
    }
    private static AuthoringLiteralAttributeCandidate Delete(AuthoringSnapshot s)
    {
        var t = s.GetLiteralAttributeDeletionTarget(Owner(s), Attribute(s)); Assert.True(t.IsSuccess, t.Failure?.Message);
        var r = s.ProposeLiteralAttributeDeletion(new(t.Target!)); Assert.True(r.IsSuccess, r.Failure?.Message); Verify(r.Candidate!); return r.Candidate!;
    }
    private static XElement Run(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        var transform = new XsltCompiler().Compile(XDocument.Load(stream, LoadOptions.PreserveWhitespace), Module.AbsoluteUri);
        return XElement.Parse(transform.TransformToString(new XDocumentNode(XDocument.Parse("<input/>"))), LoadOptions.PreserveWhitespace);
    }
    private static void Verify(AuthoringLiteralAttributeCandidate c)
    {
        var old = c.InputSnapshot.ExportOriginal(c.ModuleUri); var fresh = c.ExportSource(); var e = Assert.Single(c.SourceEdits);
        int a = (int)e.OriginalRange.StartByteOffset, b = a + (int)e.OriginalRange.ByteLength;
        int x = (int)e.ReplacementRange.StartByteOffset, y = x + (int)e.ReplacementRange.ByteLength;
        Assert.Equal(a, x); Assert.Equal(old[..a], fresh[..x]); Assert.Equal(old[b..], fresh[y..]);
        Assert.Equal(fresh, old[..a].Concat(fresh[x..y]).Concat(old[b..]).ToArray());
        var olds = Nodes(c.InputSnapshot.PrincipalModule.Root).ToArray(); var news = Nodes(c.Snapshot.PrincipalModule.Root).ToArray();
        Assert.Equal(olds.Length, c.NodeCorrespondence.Count); Assert.Equal(news.Length, c.NodeCorrespondence.Values.Distinct().Count());
        Assert.All(olds, n => Assert.Contains(n, c.NodeCorrespondence.Keys)); Assert.All(news, n => Assert.Contains(n, c.NodeCorrespondence.Values));
        Assert.Same(c.CandidateOwner, c.NodeCorrespondence[c.OriginalOwner]);
        foreach (var pair in c.NodeCorrespondence)
        {
            Assert.Equal(pair.Key.Kind, pair.Value.Kind); Assert.Equal(pair.Key.ElementName, pair.Value.ElementName); Assert.Equal(pair.Key.Text, pair.Value.Text);
            Assert.Equal(pair.Key.Children.Select(n => c.NodeCorrespondence[n]), pair.Value.Children);
        }
        Assert.Equal(olds.Count(n => n.Kind == AuthoringNodeKind.Element), c.AttributeCorrespondence.Count);
        foreach (var owner in c.AttributeCorrespondence)
        {
            Assert.Same(c.NodeCorrespondence[owner.OriginalOwner], owner.CandidateOwner);
            var originalAddressed = ReferenceEquals(owner.OriginalOwner, c.OriginalOwner) ? c.OriginalAttribute : null;
            var candidateAddressed = ReferenceEquals(owner.CandidateOwner, c.CandidateOwner) ? c.CandidateAttribute : null;
            Assert.Equal(owner.OriginalOwner.Attributes.Where(a => !ReferenceEquals(a, originalAddressed)), owner.Survivors.Keys);
            Assert.Equal(owner.CandidateOwner.Attributes.Where(a => !ReferenceEquals(a, candidateAddressed)), owner.Survivors.Values);
            foreach (var pair in owner.Survivors) { Assert.Equal(pair.Key.RawLiteral, pair.Value.RawLiteral); Assert.Equal(pair.Key.ExpandedValue, pair.Value.ExpandedValue); Assert.Equal(pair.Key.SlotKind, pair.Value.SlotKind); }
        }
        var exported = c.ExportSource(); exported[0] ^= 1; Assert.Equal(fresh, c.ExportSource());
        Assert.Equal("", c.AddressedName.NamespaceUri); Assert.Equal("", c.AddressedName.Prefix);
    }
    /// <summary>Representative exact literal values cross all physical encodings and all operations.</summary>
    public static IEnumerable<object[]> LifecycleCases => from variant in Enumerable.Range(0, 4)
        from value in new[] { "", "ready", "{name}", "a\tb\rc\nd\r\ne", "'\"&<>&amp;", "😀e\u0301", "{{}}", "   " }
        from paired in new[] { false, true } select new object[] { variant, value, paired };
    /// <summary>Every operation preserves complete source and graph evidence and actual output.</summary>
    /// <param name="variant">Physical encoding variant.</param>
    /// <param name="value">Exact logical value.</param>
    /// <param name="paired">Whether the owner is paired.</param>
    [Theory][MemberData(nameof(LifecycleCases))]
    public void FullLifecycle(int variant, string value, bool paired)
    {
        var syntax = paired ? "<out  keep = 'K' xml:space='preserve'>😀<!--C--><?p v?><child/>tail</out>" : "<out  keep = 'K' />";
        var s = Inspect(Encode(Sheet(syntax), variant)); var original = s.ExportOriginal(Module);
        var c = Create(s, value); Assert.Null(c.OriginalAttribute); Assert.Equal(value, (string?)Run(c.ExportSource()).Attribute("status"));
        Assert.Equal("status", c.CandidateOwner.Attributes[0].Name); Assert.Equal(0, c.SourceEdits[0].OriginalRange.ByteLength);
        var t = c.Snapshot.GetLiteralAttributeUpdateTarget(c.CandidateOwner, c.CandidateAttribute!); Assert.True(t.IsSuccess);
        var equal = c.Snapshot.ProposeLiteralAttributeUpdate(new(t.Target!, value)); Assert.Equal(AuthoringLiteralAttributeUpdateOutcome.NoChange, equal.Outcome);
        Assert.Null(equal.Candidate); Assert.Null(equal.Failure); Assert.Same(c.Snapshot, equal.NoChange!.InputSnapshot);
        var u = Update(c.Snapshot, value + "changed"); Assert.Equal(value + "changed", (string?)Run(u.ExportSource()).Attribute("status"));
        var d = Delete(u.Snapshot); Assert.Null(d.CandidateAttribute); Assert.Null(Run(d.ExportSource()).Attribute("status")); Assert.Equal(0, d.SourceEdits[0].ReplacementRange.ByteLength);
        Assert.Equal("K", (string?)Run(d.ExportSource()).Attribute("keep"));
        var again = Create(d.Snapshot, "again"); Assert.Equal("again", (string?)Run(again.ExportSource()).Attribute("status"));
        Assert.Equal(original, s.ExportOriginal(Module));
    }
    /// <summary>Logical equality preserves entity, quote and escaped-brace spelling.</summary>
    /// <param name="raw">Retained source value.</param>
    /// <param name="logical">Proven logical value.</param>
    [Theory][InlineData("&#123;&#123;name&#125;&#125;", "{name}")][InlineData("{{name}}", "{name}")][InlineData("&#x9;&#xD;&#xA;&amp;&apos;", "\t\r\n&'")][InlineData("", "")]
    public void NoChangeRetainsSpelling(string raw, string logical)
    {
        var s = Inspect(Sheet("<out status = '" + raw + "'/>", version: "4.0")); var bytes = s.ExportOriginal(Module);
        var t = s.GetLiteralAttributeUpdateTarget(Owner(s), Attribute(s)); Assert.True(t.IsSuccess, t.Failure?.Message); Assert.Equal(logical, t.Target!.LogicalValue);
        var r = s.ProposeLiteralAttributeUpdate(new(t.Target, logical)); Assert.Equal(AuthoringLiteralAttributeUpdateOutcome.NoChange, r.Outcome); Assert.Null(r.Candidate); Assert.Equal(bytes, s.ExportOriginal(Module));
        var c = Update(s, "{other}'\"\t\r\n"); Assert.Equal("{other}'\"\t\r\n", (string?)Run(c.ExportSource()).Attribute("status"));
        Assert.Contains("status = '", Encoding.UTF8.GetString(c.ExportSource()));
    }
    /// <summary>Unsupported or invalid contexts cannot acquire any operation authority.</summary>
    /// <param name="body">Original body.</param>
    /// <param name="extra">Module metadata.</param>
    [Theory]
    [InlineData("<out status='{name}'/>", "")][InlineData("<out status='&#123;name&#125;'/>", "")]
    [InlineData("<out status='}'/>", "")][InlineData("<out status='ok'/><other a='{name}'/>", "")]
    [InlineData("<out xmlns='urn:q' status='ok'/>", "")][InlineData("<out status='ok' xmlns:p='urn:q' p:a='x'/>", "")]
    [InlineData("<out status='ok'/>", "use-when='true()'")][InlineData("<out status='ok'/>", "extension-element-prefixes='p' xmlns:p='urn:p'")]
    [InlineData("<out status='ok'><![CDATA[abc]]></out>", "")][InlineData("<out status='ok'/>", "expand-text='yes'")]
    [InlineData("<out status='ok' xsl:use-attribute-sets='s'/>", "")][InlineData("<out status='ok'/><xsl:value-of select='('/>", "")]
    public void OriginalCoverageCannotBeRepaired(string body, string extra)
    {
        var s = Inspect(Sheet(body, extra)); var owner = Owner(s); var attr = Attribute(s);
        Assert.False(s.GetLiteralAttributeCreationTarget(owner).IsSuccess);
        Assert.False(s.GetLiteralAttributeUpdateTarget(owner, attr).IsSuccess);
        Assert.False(s.GetLiteralAttributeDeletionTarget(owner, attr).IsSuccess);
    }
    /// <summary>Names follow exact XML NCName rules and duplicate creation never overwrites.</summary>
    /// <param name="name">Invalid or reserved name.</param>
    [Theory][InlineData("")][InlineData("xmlns")][InlineData("a:b")][InlineData("1x")][InlineData("a b")]
    public void InvalidNames(string name)
    {
        var s = Inspect(Sheet("<out/>")); var t = s.GetLiteralAttributeCreationTarget(Owner(s)).Target!;
        var r = s.ProposeLiteralAttributeCreation(new(t, name, "x")); Assert.Null(r.Candidate); Assert.Equal(AuthoringLiteralAttributeFailureKind.InvalidName, r.Failure!.Kind);
    }
    /// <summary>Invalid XML logical values never enter candidates or NoChange.</summary>
    /// <param name="code">Invalid UTF-16 code unit constructed after theory serialization.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(0xd800)][InlineData(0xdc00)][InlineData(0xfffe)]
    public void InvalidValues(int code)
    {
        var value = new string((char)code, 1);
        var s = Inspect(Sheet("<out status='ok'/>")); var t = s.GetLiteralAttributeCreationTarget(Owner(s)).Target!;
        Assert.Equal(AuthoringLiteralAttributeFailureKind.InvalidValue, s.ProposeLiteralAttributeCreation(new(t, "new", value)).Failure!.Kind);
        var u = s.GetLiteralAttributeUpdateTarget(Owner(s), Attribute(s)).Target!;
        Assert.Equal(AuthoringLiteralAttributeFailureKind.InvalidValue, s.ProposeLiteralAttributeUpdate(new(u, value)).Failure!.Kind);
    }
    /// <summary>Duplicate, foreign, reserved and stale attribute authority cannot mutate another source.</summary>
    [Fact]
    public void AuthorityAndDuplicates()
    {
        var s = Inspect(Sheet("<out status='ok' xmlns:p='urn:p'/>")); var foreign = Inspect(Sheet("<out status='ok'/>")); var owner = Owner(s);
        var t = s.GetLiteralAttributeCreationTarget(owner).Target!;
        Assert.Equal(AuthoringLiteralAttributeFailureKind.DuplicateAttribute, s.ProposeLiteralAttributeCreation(new(t, "status", "other")).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.ForeignSnapshot, foreign.ProposeLiteralAttributeCreation(new(t, "new", "v")).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.ForeignSnapshot, s.GetLiteralAttributeUpdateTarget(owner, Attribute(foreign)).Failure!.Kind);
        var xmlns = owner.Attributes.Single(a => a.Name == "xmlns:p"); Assert.Equal(AuthoringLiteralAttributeFailureKind.IneligibleAttribute, s.GetLiteralAttributeDeletionTarget(owner, xmlns).Failure!.Kind);
        var ut = s.GetLiteralAttributeUpdateTarget(owner, Attribute(s)).Target!; var dt = s.GetLiteralAttributeDeletionTarget(owner, Attribute(s)).Target!;
        Assert.Equal(AuthoringLiteralAttributeFailureKind.ForeignSnapshot, foreign.ProposeLiteralAttributeUpdate(new(ut, "x")).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.ForeignSnapshot, foreign.ProposeLiteralAttributeDeletion(new(dt)).Failure!.Kind);
        var c = Create(s, "new", "NEW"); Assert.Equal(AuthoringLiteralAttributeFailureKind.ForeignSnapshot, c.Snapshot.GetLiteralAttributeDeletionTarget(c.CandidateOwner, Attribute(s)).Failure!.Kind);
    }
    /// <summary>Pre-cancelled discovery and all operations return classified cancellation exclusively.</summary>
    [Fact]
    public void CancellationAndNulls()
    {
        var s = Inspect(Sheet("<out status='ok'/>")); var o = Owner(s); var a = Attribute(s); var token = new CancellationToken(true);
        var c = s.GetLiteralAttributeCreationTarget(o).Target!; var u = s.GetLiteralAttributeUpdateTarget(o, a).Target!; var d = s.GetLiteralAttributeDeletionTarget(o, a).Target!;
        Assert.Equal(AuthoringLiteralAttributeFailureKind.Cancelled, s.GetLiteralAttributeCreationTarget(o, token).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.Cancelled, s.GetLiteralAttributeUpdateTarget(o, a, token).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.Cancelled, s.GetLiteralAttributeDeletionTarget(o, a, token).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.Cancelled, s.ProposeLiteralAttributeCreation(new(c, "new", "v"), token).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.Cancelled, s.ProposeLiteralAttributeUpdate(new(u, "ok"), token).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.Cancelled, s.ProposeLiteralAttributeDeletion(new(d), token).Failure!.Kind);
        Assert.Throws<ArgumentNullException>(() => s.GetLiteralAttributeCreationTarget(null!));
        Assert.Throws<ArgumentNullException>(() => s.GetLiteralAttributeUpdateTarget(o, null!));
        Assert.Throws<ArgumentNullException>(() => s.ProposeLiteralAttributeCreation(null!));
        Assert.Throws<ArgumentNullException>(() => new AuthoringLiteralAttributeUpdateProposal(u, null!));
    }
    /// <summary>Exact token deletion retains unrelated layout, children and paired syntax.</summary>
    /// <param name="tag">Original opening tag and body.</param>
    /// <param name="expected">Exact remaining tag and body.</param>
    [Theory][InlineData("<out status='x'  keep='K'/>", "<out   keep='K'/>")][InlineData("<out keep='K'\tstatus = \"x\" >text</out>", "<out keep='K'\t >text</out>")][InlineData("<out status=''></out>", "<out ></out>")]
    public void DeletionFootprint(string tag, string expected)
    { var c = Delete(Inspect(Sheet(tag))); Assert.Equal(Sheet(expected), Encoding.UTF8.GetString(c.ExportSource())); }
    /// <summary>Empty original values expand with correct boundary affinity for all unchanged metadata.</summary>
    [Fact]
    public void EmptyValueAndNormalizationDistinctUpdate()
    {
        var c = Update(Inspect(Sheet("<out status='' keep='K'/>")), "e\u0301"); Assert.Equal(0, c.SourceEdits[0].OriginalRange.ByteLength);
        var d = Update(c.Snapshot, "é"); Assert.Equal("é", d.CandidateLogicalValue); Assert.Equal("e\u0301", d.OriginalLogicalValue);
        Assert.True(AuthoringCapabilities.SupportsLiteralAttributeLifecycle); Assert.False(AuthoringCapabilities.SupportsAvtSlotReplacement);
        Assert.Equal(8_388_608, AuthoringCapabilities.MaxLiteralAttributeSourceBytes);
    }
    /// <summary>Independent concurrent proposals do not mutate shared owners.</summary>
    /// <returns>A task completing after all independent proposals are verified.</returns>
    [Fact]
    public async Task ConcurrentProposals()
    {
        var s = Inspect(Sheet("<out status='x'/>")); var before = s.ExportOriginal(Module);
        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(i => Task.Run(() => Create(s, i.ToString(), "new"))));
        Assert.Equal(before, s.ExportOriginal(Module)); Assert.Equal(8, results.Select(c => c.CandidateLogicalValue).Distinct().Count());
    }
    /// <summary>Unicode before the tag, physical CR/LF seams and legacy text edits compose exactly.</summary>
    /// <param name="variant">Physical encoding variant.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void PhysicalCoordinatesAndLegacyComposition(int variant)
    {
        var s = Inspect(Encode(Sheet("<!--😀e\u0301-->\r\n<out \rstatus='old'\n keep='K&#xA;Z'><xsl:text>A</xsl:text></out>"), variant));
        var created = Create(s, "{😀}\r\n", "new");
        var updated = Update(created.Snapshot, "'\"😀\r\n");
        Assert.Equal("'\"😀\r\n", (string?)Run(updated.ExportSource()).Attribute("status"));
        // Deletion joins the physical CR preceding the token with the LF after it.
        var deleted = Delete(updated.Snapshot);
        Assert.Equal(updated.CandidateOpeningTagRange.EndLine - 1, deleted.CandidateOpeningTagRange.EndLine);
        Assert.Equal("K\nZ", (string?)Run(deleted.ExportSource()).Attribute("keep"));
        var textNode = Nodes(deleted.Snapshot.PrincipalModule.Root).Single(n => n.ElementName?.NamespaceUri == Xsl && n.ElementName.LocalName == "text");
        var target = deleted.Snapshot.GetTextBodyReplacementTarget(textNode); Assert.True(target.IsSuccess, target.Failure?.Message);
        var replaced = deleted.Snapshot.ProposeTextBodyReplacement(new(target.Target!, "B")); Assert.NotNull(replaced.Candidate);
        Assert.Equal("B", Run(replaced.Candidate!.ExportSource()).Value);
        Assert.Equal("{😀}\r\n", (string?)Run(replaced.Candidate.ExportSource()).Attribute("new"));
    }
}
