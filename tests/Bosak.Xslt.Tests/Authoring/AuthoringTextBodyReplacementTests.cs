// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 October 2026
// PURPOSE              : Verify literal body replacement outcomes, exact source evidence and refusal boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 18-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>Public API checks for the reviewed REQ-133 body replacement contract.</summary>
public class AuthoringTextBodyReplacementTests
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static readonly Uri Uri = new("file:///C:/authoring/replacement.xsl");
    private static string Sheet(string body, string extra = "", string version = "3.0") =>
        $"<xsl:stylesheet xmlns:xsl='{Xsl}' version='{version}' {extra}><xsl:output method='text'/><xsl:template match='/'>{body}</xsl:template></xsl:stylesheet>";
    private static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor n) => new[] { n }.Concat(n.Children.SelectMany(Nodes));
    private static AuthoringSnapshot Inspect(string s) => Inspect(Encoding.UTF8.GetBytes(s));
    private static AuthoringSnapshot Inspect(byte[] bytes)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, Uri, out var source, out var failure), failure?.Message);
        var result = XsltAuthoring.Inspect(source!, options: new() { AttemptCompilation = false });
        Assert.True(result.IsSuccess, result.Failure?.Message); return result.Snapshot!;
    }
    private static AuthoringNodeDescriptor Text(AuthoringSnapshot s) => Nodes(s.PrincipalModule.Root).First(n => n.ElementName?.NamespaceUri == Xsl && n.ElementName.LocalName == "text");
    private static AuthoringTextBodyReplacementTarget Target(AuthoringSnapshot s)
    {
        var result = s.GetTextBodyReplacementTarget(Text(s)); Assert.True(result.IsSuccess, result.Failure?.Message); return result.Target!;
    }
    private static AuthoringTextBodyReplacementCandidate Change(AuthoringSnapshot s, string value)
    {
        var r = s.ProposeTextBodyReplacement(new(Target(s), value));
        Assert.True(r.Outcome == AuthoringTextBodyReplacementOutcome.Changed, r.Failure?.Message);
        Assert.Null(r.NoChange); Assert.Null(r.Failure); Verify(r.Candidate!); return r.Candidate!;
    }
    private static string Run(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return new XsltCompiler().Compile(XDocument.Load(stream, LoadOptions.PreserveWhitespace), Uri.AbsoluteUri)
            .TransformToString(new XDocumentNode(XDocument.Parse("<input/>")));
    }
    private static void Verify(AuthoringTextBodyReplacementCandidate c)
    {
        var old = c.InputSnapshot.ExportOriginal(c.ModuleUri); var fresh = c.ExportSource();
        var edit = Assert.Single(c.SourceEdits);
        Assert.Equal(c.OriginalBodyRange, edit.OriginalRange); Assert.Equal(c.ReplacementBodyRange, edit.ReplacementRange);
        var a = (int)edit.OriginalRange.StartByteOffset; var b = a + (int)edit.OriginalRange.ByteLength;
        var x = (int)edit.ReplacementRange.StartByteOffset; var y = x + (int)edit.ReplacementRange.ByteLength;
        Assert.Equal(a, x); Assert.Equal(old[..a], fresh[..x]); Assert.Equal(old[b..], fresh[y..]);
        Assert.Equal(fresh, old[..a].Concat(fresh[x..y]).Concat(old[b..]).ToArray());
        var olds = Nodes(c.InputSnapshot.PrincipalModule.Root).ToDictionary(n => n.Id);
        var news = Nodes(c.Snapshot.PrincipalModule.Root).ToDictionary(n => n.Id);
        var replaced = c.ReplacedNodes.Select(n => n.Id).ToArray(); var added = c.NewNodes.Select(n => n.Id).ToArray();
        Assert.Equal(olds.Keys.Order(), c.SurvivorNodeCorrespondence.Keys.Concat(replaced).Order());
        Assert.Equal(news.Keys.Order(), c.SurvivorNodeCorrespondence.Values.Concat(added).Order());
        Assert.Equal(c.SurvivorNodeCorrespondence.Count, c.SurvivorNodeCorrespondence.Values.Distinct().Count());
        Assert.Empty(c.SurvivorNodeCorrespondence.Keys.Intersect(replaced)); Assert.Empty(c.SurvivorNodeCorrespondence.Values.Intersect(added));
        Assert.Same(c.EditedNode, news[c.SurvivorNodeCorrespondence[Text(c.InputSnapshot).Id]]);
        Assert.Contains(c.EditedNode, c.EditedParent.Children);
        foreach (var pair in c.SurvivorNodeCorrespondence)
        {
            var o = olds[pair.Key]; var n = news[pair.Value];
            Assert.Equal(o.Kind, n.Kind); Assert.Equal(o.ElementName, n.ElementName); Assert.Equal(o.Text, n.Text);
            Assert.Equal(o.Attributes.Select(a => (a.Name,a.Prefix,a.NamespaceUri,a.LocalName,a.RawLiteral,a.ExpandedValue,a.SlotKind)),
                n.Attributes.Select(a => (a.Name,a.Prefix,a.NamespaceUri,a.LocalName,a.RawLiteral,a.ExpandedValue,a.SlotKind)));
            Assert.Equal(o.Children.Where(v => !ReferenceEquals(v,c.OriginalPayloadNode)).Select(v => c.SurvivorNodeCorrespondence[v.Id]),
                n.Children.Where(v => !ReferenceEquals(v,c.PayloadNode)).Select(v => v.Id));
        }
        if (c.OriginalPayloadNode is null) Assert.Empty(c.ReplacedNodes); else Assert.Same(c.OriginalPayloadNode, Assert.Single(c.ReplacedNodes));
        if (c.TextValue.Length == 0)
        { Assert.Null(c.PayloadNode); Assert.Empty(c.NewNodes); Assert.Empty(c.EditedNode.Children); Assert.Equal(0,c.ReplacementBodyRange.ByteLength); }
        else
        { Assert.Same(c.PayloadNode,Assert.Single(c.NewNodes)); Assert.Same(c.PayloadNode,Assert.Single(c.EditedNode.Children)); Assert.Equal(c.TextValue,c.PayloadNode!.Text); Assert.Equal(c.ReplacementBodyRange,c.PayloadNode.Range); }
        var copy = c.ExportSource(); copy[0] ^= 1; Assert.Equal(fresh,c.ExportSource());
    }

    /// <summary>All changed payload-presence transitions have exact disjoint partitions.</summary>
    /// <param name="old">The old fixture input.</param>
    /// <param name="value">The value fixture input.</param>
    [Theory]
    [InlineData("", "new")]
    [InlineData("old", "")]
    [InlineData("old", "new")]
    [InlineData("  ", " ")]
    public void PayloadTransitions(string old, string value)
    {
        var c = Change(Inspect(Sheet($"<out><xsl:text>{old}</xsl:text></out>")),value);
        Assert.Equal(value,Run(c.ExportSource()));
        Assert.Equal(old,c.OriginalTextValue);
    }

    /// <summary>Encoding and Unicode coordinates preserve every byte outside the body.</summary>
    /// <param name="mode">The mode fixture input.</param>
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void EncodingsAndCoordinates(int mode)
    {
        Encoding e = mode < 2 ? new UTF8Encoding(mode == 1,true) : new UnicodeEncoding(mode == 3,true,true);
        var declaration = mode < 2 ? "utf-8" : mode == 2 ? "utf-16le" : "utf-16be";
        var text = $"<?xml version='1.0' encoding='{declaration}'?>\r\n" + Sheet("<!--???-->\r<out a='&amp;' xml:space='preserve'>before\n<xsl:text>old</xsl:text>after\r\n</out>");
        var s = Inspect(e.GetPreamble().Concat(e.GetBytes(text)).ToArray());
        var value = "???e\u0301&<>]]>{a}'\"\t\r\n&#x41;";
        var c = Change(s,value);
        Assert.Contains("before\n"+value+"after\n",Run(c.ExportSource()));
        Assert.True(AuthoringSource.TryCreate(c.ExportSource(),Uri,out var src,out _));
        var lexical = src!.Encoding.GetString(c.ExportSource().AsSpan((int)c.ReplacementBodyRange.StartByteOffset,(int)c.ReplacementBodyRange.ByteLength));
        Assert.Contains("&amp;&lt;&gt;]]&gt;",lexical); Assert.Contains("&#x9;&#xD;&#xA;&amp;#x41;",lexical);
    }

    /// <summary>Equal logical intent preserves lexical spelling and has no candidate.</summary>
    /// <param name="lexical">The lexical fixture input.</param>
    /// <param name="logical">The logical fixture input.</param>
    [Theory]
    [InlineData("&#x41;","A")]
    [InlineData("&#65;","A")]
    [InlineData("&amp;","&")]
    [InlineData("","")]
    [InlineData("\r\n","\n")]
    [InlineData("\r","\n")]
    [InlineData("&#xD;&#xA;","\r\n")]
    public void EqualIntent(string lexical,string logical)
    {
        var s = Inspect(Sheet($"<xsl:text>{lexical}</xsl:text>")); var target=Target(s); var bytes=s.ExportOriginal(Uri);
        var r=s.ProposeTextBodyReplacement(new(target,logical)); Assert.Equal(AuthoringTextBodyReplacementOutcome.NoChange,r.Outcome);
        Assert.Null(r.Candidate); Assert.Null(r.Failure); Assert.Same(s,r.NoChange!.InputSnapshot); Assert.Same(target,r.NoChange.Target);
        Assert.Equal(logical,r.NoChange.TextValue); Assert.Equal(bytes,s.ExportOriginal(Uri));
    }

    /// <summary>Different newline and normalization code units are distinct intent.</summary>
    /// <param name="old">The old fixture input.</param>
    /// <param name="value">The value fixture input.</param>
    [Theory]
    [InlineData("\r\n","\r")]
    [InlineData("\r\n","\r\n")]
    [InlineData("?","e\u0301")]
    [InlineData("A","&#x41;")]
    public void DifferentLogicalIntent(string old,string value)
    { Assert.Equal(value,Run(Change(Inspect(Sheet($"<xsl:text>{old}</xsl:text>")),value).ExportSource())); }

    /// <summary>Syntax exclusions cannot be bypassed with empty/equal intent.</summary>
    /// <param name="body">The body fixture input.</param>
    /// <param name="expected">The expected fixture input.</param>
    [Theory]
    [InlineData("<xsl:text/>",AuthoringTextBodyReplacementFailureKind.SelfClosingInstruction)]
    [InlineData("<xsl:text><![CDATA[]]></xsl:text>",AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("<xsl:text><![CDATA[A]]></xsl:text>",AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("<xsl:text><!--c--></xsl:text>",AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("<xsl:text><?p x?></xsl:text>",AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("<xsl:text>A<!--c-->B</xsl:text>",AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("<xsl:text><out/></xsl:text>",AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("<xsl:text expand-text='yes'>A</xsl:text>",AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("<xsl:text disable-output-escaping='yes'>A</xsl:text>",AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    public void ExcludedBodies(string body,AuthoringTextBodyReplacementFailureKind expected)
    { var s=Inspect(Sheet(body)); var r=s.GetTextBodyReplacementTarget(Text(s)); Assert.False(r.IsSuccess); Assert.Equal(expected,r.Failure!.Kind); }

    /// <summary>Supported inherited preservation, mixed text and local non-expansion are retained.</summary>
    /// <param name="root">The root fixture input.</param>
    /// <param name="attrs">The attrs fixture input.</param>
    [Theory]
    [InlineData("xml:space='preserve'","")]
    [InlineData("xml:space='default'","")]
    [InlineData("expand-text='yes'","expand-text='no'")]
    [InlineData("","expand-text='false'")]
    [InlineData("","expand-text='0'")]
    public void SupportedContexts(string root,string attrs)
    {
        var outside=root.Contains("expand") ? "" : "outside";
        var s=Inspect(Sheet($"<out>{outside}<xsl:text {attrs}>A</xsl:text>{outside}</out>",root));
        var c=Change(s,"B"); Assert.Equal(outside+"B"+outside,Run(c.ExportSource()));
    }

    /// <summary>Aliases, default XSLT namespace and both language versions are eligible.</summary>
    /// <param name="prefix">The prefix fixture input.</param>
    /// <param name="version">The version fixture input.</param>
    [Theory]
    [InlineData("xsl","3.0")] [InlineData("a","4.0")] [InlineData("","3.0")] [InlineData("","4.0")]
    public void ExpandedNames(string prefix,string version)
    {
        var p=prefix.Length==0 ? "" : prefix+":"; var ns=prefix.Length==0 ? "xmlns" : "xmlns:"+prefix;
        var s=Inspect($"<{p}stylesheet {ns}='{Xsl}' version='{version}'><{p}output method='text'/><{p}template match='/'><{p}text>A</{p}text></{p}template></{p}stylesheet>");
        Assert.Equal("B",Run(Change(s,"B").ExportSource()));
    }

    /// <summary>Foreign identity, wrong kinds and pre-cancellation never grant authority.</summary>
    [Fact]
    public void IdentityAndCancellation()
    {
        var a=Inspect(Sheet("<out><xsl:text>A</xsl:text></out>")); var b=Inspect(Sheet("<out><xsl:text>A</xsl:text></out>"));
        Assert.Equal(AuthoringTextBodyReplacementFailureKind.ForeignSnapshot,a.GetTextBodyReplacementTarget(Text(b)).Failure!.Kind);
        Assert.Equal(AuthoringTextBodyReplacementFailureKind.ForeignSnapshot,b.ProposeTextBodyReplacement(new(Target(a),"A")).Failure!.Kind);
        foreach(var n in Nodes(a.PrincipalModule.Root).Where(n=>!ReferenceEquals(n,Text(a))))
            Assert.Equal(AuthoringTextBodyReplacementFailureKind.IneligibleInstruction,a.GetTextBodyReplacementTarget(n).Failure!.Kind);
        using var cts=new CancellationTokenSource(); cts.Cancel();
        Assert.Equal(AuthoringTextBodyReplacementFailureKind.Cancelled,a.GetTextBodyReplacementTarget(Text(a),cts.Token).Failure!.Kind);
        var r=a.ProposeTextBodyReplacement(new(Target(a),"A"),cts.Token);
        Assert.Equal(AuthoringTextBodyReplacementOutcome.Refused,r.Outcome); Assert.Null(r.Candidate); Assert.Null(r.NoChange);
        Assert.Equal(AuthoringTextBodyReplacementFailureKind.Cancelled,r.Failure!.Kind);
    }

    /// <summary>Invalid runtime-built scalars are rejected without normalization.</summary>
    [Fact]
    public void InvalidScalars()
    {
        var s=Inspect(Sheet("<xsl:text>A</xsl:text>")); var t=Target(s);
        foreach(var value in new[]{new string((char)0xd800,1),new string((char)0xdc00,1),"a\0b","\u0001","\ufffe","\uffff"})
            Assert.Equal(AuthoringTextBodyReplacementFailureKind.InvalidPayload,s.ProposeTextBodyReplacement(new(t,value)).Failure!.Kind);
    }

    /// <summary>Invalid original and unsupported static contexts cannot return NoChange.</summary>
    /// <param name="attrs">The attrs fixture input.</param>
    /// <param name="sibling">The sibling fixture input.</param>
    /// <param name="expected">The expected fixture input.</param>
    [Theory]
    [InlineData("", "<xsl:sequence select='$missing'/>", AuthoringTextBodyReplacementFailureKind.InvalidOriginal)]
    [InlineData("", "<xsl:variable name='v' static='yes' select='1'/>", AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("", "<xsl:sequence use-when='true()' select='1'/>", AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("extension-element-prefixes='e' xmlns:e='urn:e'", "", AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("", "<xsl:sequence _select='1' select='1'/>", AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    public void OriginalCoverage(string attrs,string sibling,AuthoringTextBodyReplacementFailureKind expected)
    {
        var s=Inspect(Sheet($"<xsl:text>A</xsl:text>{sibling}",attrs)); var r=s.GetTextBodyReplacementTarget(Text(s));
        Assert.False(r.IsSuccess); Assert.Equal(expected,r.Failure!.Kind);
    }

    /// <summary>Null arguments throw while optional collections remain immutable.</summary>
    [Fact]
    public void NullAndCollections()
    {
        Assert.True(AuthoringCapabilities.SupportsTextBodyReplacement); Assert.Equal(8388608,AuthoringCapabilities.MaxTextBodyReplacementSourceBytes);
        Assert.Throws<NotSupportedException>(()=>((IList<string>)AuthoringCapabilities.TextBodyReplacementSupportedEncodings)[0]="bad");
        var s=Inspect(Sheet("<xsl:text>A</xsl:text>")); var t=Target(s);
        Assert.Throws<ArgumentNullException>(()=>s.GetTextBodyReplacementTarget(null!));
        Assert.Throws<ArgumentNullException>(()=>s.ProposeTextBodyReplacement(null!));
        Assert.Throws<ArgumentNullException>(()=>new AuthoringTextBodyReplacementProposal(null!,"x"));
        Assert.Throws<ArgumentNullException>(()=>new AuthoringTextBodyReplacementProposal(t,null!));
        var c=Change(s,"B");
        Assert.Throws<NotSupportedException>(()=>((IDictionary<int,int>)c.SurvivorNodeCorrespondence).Clear());
        Assert.Throws<NotSupportedException>(()=>((IList<AuthoringNodeDescriptor>)c.NewNodes).Clear());
        Assert.Throws<NotSupportedException>(()=>((IList<AuthoringNodeDescriptor>)c.ReplacedNodes).Clear());
        Assert.Throws<NotSupportedException>(()=>((IList<AuthoringInsertionSourceEdit>)c.SourceEdits).Clear());
        Assert.Null(typeof(AuthoringTextBodyReplacementResult).GetProperty("IsSuccess"));
    }

    /// <summary>Repeated and prior inserted instructions need fresh owned authority.</summary>
    [Fact]
    public void InsertThenReplaceAndRepeat()
    {
        var s=Inspect(Sheet("<out></out>")); var parent=Nodes(s.PrincipalModule.Root).Single(n=>n.ElementName?.LocalName=="out");
        var inserted=s.ProposeTextInsertion(new(s.GetTextInsertionTarget(parent).Target!,"first")).Candidate!;
        var c=Change(inserted.Snapshot,"second"); c=Change(c.Snapshot,""); c=Change(c.Snapshot,"third");
        Assert.Equal("third",Run(c.ExportSource()));
    }

    /// <summary>Concurrent changed, equal and refused proposals do not mutate their shared owner.</summary>
    /// <returns>A task completing after all isolated requests have been verified.</returns>
    [Fact]
    public async Task ConcurrentIsolation()
    {
        var s=Inspect(Sheet("<xsl:text>A</xsl:text>")); var t=Target(s); var bytes=s.ExportOriginal(Uri);
        var tasks=Enumerable.Range(0,12).Select(i=>Task.Run(()=>s.ProposeTextBodyReplacement(new(t,i%3==0?"A":i%3==1?"B":"\0"))));
        var results=await Task.WhenAll(tasks);
        Assert.Equal(4,results.Count(r=>r.Outcome==AuthoringTextBodyReplacementOutcome.Changed));
        Assert.Equal(4,results.Count(r=>r.Outcome==AuthoringTextBodyReplacementOutcome.NoChange));
        Assert.Equal(4,results.Count(r=>r.Outcome==AuthoringTextBodyReplacementOutcome.Refused));
        Assert.Equal(bytes,s.ExportOriginal(Uri));
    }
    /// <summary>Exact physical boundaries count escaped payload, encoding and BOM.</summary>
    /// <param name="utf16">Whether to use BOM-marked UTF-16.</param>
    /// <param name="extra">Whether the final candidate crosses the bound.</param>
    [Theory][InlineData(false,false)][InlineData(false,true)][InlineData(true,false)][InlineData(true,true)]
    public void PhysicalBoundary(bool utf16,bool extra)
    {
        Encoding enc=utf16 ? new UnicodeEncoding(false,true,true) : new UTF8Encoding(false,true);
        var prefix=utf16 ? "<?xml version='1.0' encoding='utf-16'?>" : "";
        var small=prefix+Sheet("<!----><xsl:text>A</xsl:text>");
        var width=utf16?2:1; var limit=AuthoringCapabilities.MaxTextBodyReplacementSourceBytes;
        // '&' expands to five ASCII source characters; old A contributes one.
        var padding=(limit-enc.GetPreamble().Length-enc.GetByteCount(small)-4*width)/width;
        var text=prefix+Sheet("<!--"+new string('p',padding+(extra?1:0))+"--><xsl:text>A</xsl:text>");
        var owner=Inspect(enc.GetPreamble().Concat(enc.GetBytes(text)).ToArray());
        var r=owner.ProposeTextBodyReplacement(new(Target(owner),"&"));
        if(extra) Assert.Equal(AuthoringTextBodyReplacementFailureKind.SizeLimit,r.Failure!.Kind);
        else {Assert.Equal(AuthoringTextBodyReplacementOutcome.Changed,r.Outcome);Assert.Equal(limit,r.Candidate!.ExportSource().Length);Verify(r.Candidate);}
    }

    /// <summary>Equal intent avoids hypothetical escaping growth; shrinking replaces rather than appends.</summary>
    [Fact]
    public void NoChangeBudgetAndShrinkage()
    {
        var limit=AuthoringCapabilities.MaxTextBodyReplacementSourceBytes;
        var small=Sheet("<!----><xsl:text>\n</xsl:text>");
        var padding=limit-Encoding.UTF8.GetByteCount(small);
        var s=Inspect(Sheet("<!--"+new string('p',padding)+"--><xsl:text>\n</xsl:text>"));
        var t=Target(s); Assert.Equal(limit,s.ExportOriginal(Uri).Length);
        Assert.Equal(AuthoringTextBodyReplacementOutcome.NoChange,s.ProposeTextBodyReplacement(new(t,"\n")).Outcome);
        var changed=s.ProposeTextBodyReplacement(new(t,"")); Assert.Equal(AuthoringTextBodyReplacementOutcome.Changed,changed.Outcome);
        Assert.Equal(limit-1,changed.Candidate!.ExportSource().Length); Verify(changed.Candidate);
        var over=Inspect(Sheet("<!--"+new string('p',padding+1)+"--><xsl:text>\n</xsl:text>"));
        Assert.Equal(AuthoringTextBodyReplacementFailureKind.SizeLimit,over.GetTextBodyReplacementTarget(Text(over)).Failure!.Kind);
    }

    /// <summary>Large payload replacements subtract the actual old body length.</summary>
    [Fact]
    public void ShrinkToFit()
    {
        var old=new string('a',4_200_000); var s=Inspect(Sheet("<xsl:text>"+old+"</xsl:text>"));
        var c=Change(s,new string('b',4_199_999)); Assert.Equal(s.ExportOriginal(Uri).Length-1,c.ExportSource().Length);
    }

    /// <summary>Excluded encodings and profiles refuse rather than returning equal intent.</summary>
    /// <param name="mode">The excluded profile selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void UnsupportedProfiles(int mode)
    {
        var text=Sheet("<xsl:text>A</xsl:text>",version:mode==0?"2.0":"3.0");
        var bytes=mode switch
        {
            1=>Encoding.ASCII.GetBytes("<?xml version='1.0' encoding='us-ascii'?>"+text),
            2=>new UTF32Encoding(false,true,true).GetPreamble().Concat(new UTF32Encoding(false,true,true).GetBytes(text)).ToArray(),
            3=>new UnicodeEncoding(false,true,true).GetPreamble().Concat(new UnicodeEncoding(false,true,true).GetBytes("<?xml version='1.0' encoding='utf-16be'?>"+text)).ToArray(),
            _=>Encoding.UTF8.GetBytes(text)
        };
        var s=Inspect(bytes); Assert.Equal(mode==0?AuthoringTextBodyReplacementFailureKind.UnsupportedProfile:AuthoringTextBodyReplacementFailureKind.UnsupportedEncoding,
            s.GetTextBodyReplacementTarget(Text(s)).Failure!.Kind);
    }

    /// <summary>Dependencies and preprocessing refuse before additional resolver calls.</summary>
    /// <param name="declaration">The dependency or preprocessing declaration.</param>
    /// <param name="expected">The operation-specific refusal.</param>
    [Theory]
    [InlineData("<xsl:include href='https://invalid.example/other.xsl'/>",AuthoringTextBodyReplacementFailureKind.UnsupportedModule)]
    [InlineData("<xsl:import-schema namespace='urn:s'/>",AuthoringTextBodyReplacementFailureKind.UnsupportedModule)]
    [InlineData("<xsl:use-package name='urn:p' package-version='1.0'/>",AuthoringTextBodyReplacementFailureKind.UnsupportedModule)]
    [InlineData("<xsl:variable name='v' static='yes' select=\"doc('https://invalid.example/')\"/>",AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("<xsl:variable name='v' _select=\"doc('https://invalid.example/')\" select='1'/>",AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    [InlineData("<xsl:template name='t' as='item()*'/>",AuthoringTextBodyReplacementFailureKind.UnsupportedCoverage)]
    public void DeniedContext(string declaration,AuthoringTextBodyReplacementFailureKind expected)
    {
        var text=Sheet("<xsl:text>A</xsl:text>").Replace("<xsl:output",declaration+"<xsl:output");
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text),Uri,out var source,out _));
        var resolver=new CountingResolver(); var result=XsltAuthoring.Inspect(source!,resolver,new(){AttemptCompilation=false}); Assert.True(result.IsSuccess);
        var before=resolver.Calls; var s=result.Snapshot!; var target=s.GetTextBodyReplacementTarget(Text(s));
        Assert.Equal(expected,target.Failure!.Kind); Assert.Equal(before,resolver.Calls);
    }
    private sealed class CountingResolver : IAuthoringModuleResolver
    {
        internal int Calls { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri,Uri referencingModuleUri) { Calls++;return null; }
    }

}
