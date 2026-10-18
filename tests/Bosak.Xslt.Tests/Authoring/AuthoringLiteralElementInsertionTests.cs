// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 October 2026
// PURPOSE              : Verify literal element source, runtime identity, composition and refusal boundaries.
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

/// <summary>Public contract evidence for bounded literal result element insertion.</summary>
public class AuthoringLiteralElementInsertionTests
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static readonly Uri ModuleUri = new("file:///C:/authoring/element.xsl");
    private static string Sheet(string body = "<out></out>", string extra = "", string version = "3.0") =>
        $"<xsl:stylesheet xmlns:xsl='{Xsl}' version='{version}' {extra}><xsl:output method='xml' omit-xml-declaration='yes'/><xsl:template match='/'>{body}</xsl:template></xsl:stylesheet>";
    private static AuthoringSnapshot Inspect(string text) => Inspect(Encoding.UTF8.GetBytes(text));
    private static AuthoringSnapshot Inspect(byte[] bytes)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, ModuleUri, out var source, out var failure), failure?.Message);
        var result = XsltAuthoring.Inspect(source!, options: new() { AttemptCompilation = false });
        Assert.True(result.IsSuccess, result.Failure?.Message); return result.Snapshot!;
    }
    private static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor root) => new[] { root }.Concat(root.Children.SelectMany(Nodes));
    private static AuthoringNodeDescriptor Find(AuthoringSnapshot s, string name) => Nodes(s.PrincipalModule.Root).First(n => n.ElementName?.LocalName == name);
    private static string Lexical(byte[] bytes, SourceRange range)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, ModuleUri, out var source, out _));
        return source!.Encoding.GetString(bytes.AsSpan((int)range.StartByteOffset, (int)range.ByteLength));
    }
    private static string Run(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return new XsltCompiler().Compile(XDocument.Load(stream, LoadOptions.PreserveWhitespace), ModuleUri.AbsoluteUri)
            .TransformToString(new XDocumentNode(XDocument.Parse("<input/>")));
    }
    private static AuthoringLiteralElementInsertionCandidate Insert(AuthoringSnapshot s, string name = "item",
        AuthoringInsertionFormattingMode mode = AuthoringInsertionFormattingMode.PreserveSource, string parent = "out")
    {
        var t = s.GetLiteralElementInsertionTarget(Find(s,parent)); Assert.True(t.IsSuccess,t.Failure?.Message);
        var result = s.ProposeLiteralElementInsertion(new(t.Target!,name,mode)); Assert.True(result.IsSuccess,result.Failure?.Message);
        Verify(result.Candidate!); return result.Candidate!;
    }
    private static void Verify(AuthoringLiteralElementInsertionCandidate c)
    {
        var old = c.InputSnapshot.ExportOriginal(c.ModuleUri); var bytes = c.ExportSource();
        var output = new List<byte>(); var cursor = 0; long lastNew = 0;
        foreach (var edit in c.SourceEdits)
        {
            Assert.True(edit.OriginalRange.StartByteOffset >= cursor); Assert.True(edit.ReplacementRange.StartByteOffset >= lastNew);
            output.AddRange(old.AsSpan(cursor,(int)edit.OriginalRange.StartByteOffset-cursor).ToArray());
            output.AddRange(bytes.AsSpan((int)edit.ReplacementRange.StartByteOffset,(int)edit.ReplacementRange.ByteLength).ToArray());
            cursor=(int)(edit.OriginalRange.StartByteOffset+edit.OriginalRange.ByteLength);
            lastNew=edit.ReplacementRange.StartByteOffset+edit.ReplacementRange.ByteLength;
        }
        output.AddRange(old.AsSpan(cursor).ToArray()); Assert.Equal(bytes,output.ToArray());
        var before=Nodes(c.InputSnapshot.PrincipalModule.Root).ToArray();var after=Nodes(c.Snapshot.PrincipalModule.Root).ToArray();
        Assert.Equal(before.Length,c.NodeCorrespondence.Count);Assert.Equal(before.Length,c.NodeCorrespondence.Values.Distinct().Count());
        Assert.Equal(after.Select(n=>n.Id).Order(),c.NodeCorrespondence.Values.Concat(c.NewNodes.Select(n=>n.Id)).Order());
        Assert.DoesNotContain(c.NewNodes,n=>c.NodeCorrespondence.Values.Contains(n.Id));
        Assert.Contains(c.InsertedNode,c.NewNodes); Assert.Contains(c.InsertedNode,c.InsertedParent.Children);
        Assert.Empty(c.InsertedNode.Children);Assert.Equal("",c.ElementName.NamespaceUri);Assert.Equal(c.LocalName,c.ElementName.LocalName);
        Assert.Equal(0,c.BodyRange.ByteLength);Assert.Equal("",Lexical(bytes,c.BodyRange));
        Assert.Same(c.InsertedNode,after.Single(n=>n.Id==c.InsertedNode.Id));
        foreach(var node in before)
        {
            var fresh=after.Single(n=>n.Id==c.NodeCorrespondence[node.Id]);
            Assert.Equal(node.Kind,fresh.Kind);Assert.Equal(node.ElementName?.NamespaceUri,fresh.ElementName?.NamespaceUri);
            Assert.Equal(node.ElementName?.LocalName,fresh.ElementName?.LocalName);
            Assert.Equal(node.Children.Select(n=>c.NodeCorrespondence[n.Id]),fresh.Children.Where(n=>c.NodeCorrespondence.Values.Contains(n.Id)).Select(n=>n.Id));
            Assert.Equal(node.Attributes.Select(a=>(a.Name,a.RawLiteral,a.ExpandedValue)),fresh.Attributes.Select(a=>(a.Name,a.RawLiteral,a.ExpandedValue)));
        }
    }

    /// <summary>Checks exact source and runtime namespace semantics across encodings and modes.</summary>
    /// <param name="encoding">The physical source encoding.</param>
    /// <param name="ns">The inherited default namespace.</param>
    /// <param name="readable">Whether layout is explicitly requested.</param>
    [Theory]
    [InlineData(0,"",false)]
    [InlineData(0,"",true)]
    [InlineData(0,"urn:result",false)]
    [InlineData(0,"urn:result",true)]
    [InlineData(0,"http://www.w3.org/1999/XSL/Transform",false)]
    [InlineData(0,"http://www.w3.org/1999/XSL/Transform",true)]
    [InlineData(1,"",false)]
    [InlineData(1,"",true)]
    [InlineData(1,"urn:result",false)]
    [InlineData(1,"urn:result",true)]
    [InlineData(1,"http://www.w3.org/1999/XSL/Transform",false)]
    [InlineData(1,"http://www.w3.org/1999/XSL/Transform",true)]
    [InlineData(2,"",false)]
    [InlineData(2,"",true)]
    [InlineData(2,"urn:result",false)]
    [InlineData(2,"urn:result",true)]
    [InlineData(2,"http://www.w3.org/1999/XSL/Transform",false)]
    [InlineData(2,"http://www.w3.org/1999/XSL/Transform",true)]
    [InlineData(3,"",false)]
    [InlineData(3,"",true)]
    [InlineData(3,"urn:result",false)]
    [InlineData(3,"urn:result",true)]
    [InlineData(3,"http://www.w3.org/1999/XSL/Transform",false)]
    [InlineData(3,"http://www.w3.org/1999/XSL/Transform",true)]
    public void NamespaceEncodingAndComposition(int encoding,string ns,bool readable)
    {
        Encoding enc=encoding switch {0=>new UTF8Encoding(false,true),1=>new UTF8Encoding(true,true),2=>new UnicodeEncoding(false,true,true),_=>new UnicodeEncoding(true,true,true)};
        var declaration=encoding<2 ? "<?xml version='1.0' encoding='utf-8'?>" : "<?xml version='1.0' encoding='utf-16'?>";
        var text=Sheet(extra:ns.Length==0 ? "" : $"xmlns='{ns}'");
        if(ns==Xsl) text=text.Replace("<out></out>","<out xmlns='urn:parent'></out>");
        // Target the template under a default XSLT namespace; its existing child is explicitly namespaced.
        var parent=ns==Xsl ? "template" : "out";
        var bytes=enc.GetPreamble().Concat(enc.GetBytes(declaration+text)).ToArray();var original=Inspect(bytes);
        var mode=readable ? AuthoringInsertionFormattingMode.Readable : AuthoringInsertionFormattingMode.PreserveSource;
        var c=Insert(original,mode:mode,parent:parent);
        var markup=Lexical(c.ExportSource(),c.InsertedRange);Assert.Equal(ns.Length==0 ? "<item></item>" : "<item xmlns=\"\"></item>",markup);
        Assert.Equal(0,c.BodyRange.ByteLength);Assert.Equal(c.InsertedRange.StartByteOffset+enc.GetByteCount(markup[..markup.IndexOf("</",StringComparison.Ordinal)]),c.BodyRange.StartByteOffset);
        Assert.Equal(enc.GetPreamble(),c.ExportSource().Take(enc.GetPreamble().Length));
        var result=XElement.Parse("<result>"+Run(c.ExportSource())+"</result>");var element=result.Descendants().Single(e=>e.Name.LocalName=="item");Assert.Equal(XName.Get("item"),element.Name);
        Assert.NotEqual(Run(bytes),Run(c.ExportSource()));
        var t=c.Snapshot.GetTextInsertionTarget(c.InsertedNode);Assert.True(t.IsSuccess,t.Failure?.Message);
        var textResult=c.Snapshot.ProposeTextInsertion(new(t.Target!,"A & {literal}"));Assert.True(textResult.IsSuccess,textResult.Failure?.Message);
        Assert.Equal("A & {literal}",XElement.Parse("<result>"+Run(textResult.Candidate!.ExportSource())+"</result>").Descendants("item").Single().Value);
        var v=c.Snapshot.GetValueOfInsertionTarget(c.InsertedNode);Assert.True(v.IsSuccess,v.Failure?.Message);
        var valueResult=c.Snapshot.ProposeValueOfInsertion(new(v.Target!,"'V'"));Assert.True(valueResult.IsSuccess,valueResult.Failure?.Message);
        Assert.Equal("V",XElement.Parse("<result>"+Run(valueResult.Candidate!.ExportSource())+"</result>").Descendants("item").Single().Value);
        Assert.Equal(bytes,original.ExportOriginal(ModuleUri));
    }

    /// <summary>Accepts exact legal names without local-name instruction or reserved-prefix guessing.</summary>
    /// <param name="name">The exact local name.</param>
    [Theory][InlineData("item")][InlineData("value-of")][InlineData("text")][InlineData("template")][InlineData("xml")][InlineData("xmlns")][InlineData("xmlThing")][InlineData("é")][InlineData("é")][InlineData("項目")]
    public void ExactNames(string name)
    {
        var c=Insert(Inspect(Sheet()),name);Assert.Equal(name,c.LocalName);Assert.Equal("<"+name+"></"+name+">",Lexical(c.ExportSource(),c.InsertedRange));
        Assert.Equal(XName.Get(name),XDocument.Parse(Run(c.ExportSource())).Root!.Elements().Single().Name);
    }
    /// <summary>Rejects name intent without repair or expression parsing.</summary>
    /// <param name="name">Invalid local-name intent.</param>
    [Theory][InlineData("")][InlineData(" a")][InlineData("a ")][InlineData("1a")][InlineData("a:b")][InlineData("xmlns:a")][InlineData("xml:a")][InlineData("<a/>")][InlineData("{u}a")][InlineData("Q{u}a")][InlineData("a b")][InlineData("a/b")][InlineData("a&b")]
    public void InvalidNames(string name)
    {
        var s=Inspect(Sheet());var t=s.GetLiteralElementInsertionTarget(Find(s,"out")).Target!;var result=s.ProposeLiteralElementInsertion(new(t,name));
        Assert.Null(result.Candidate);Assert.Equal(AuthoringInsertionFailureKind.InvalidElementName,result.Failure!.Kind);
    }
    /// <summary>Runtime-created invalid scalars bypass test-runner Unicode argument normalization.</summary>
    /// <param name="scalar">The invalid UTF-16 code unit.</param>
    [Theory][InlineData(0)][InlineData(0xD800)][InlineData(0xDC00)]
    public void InvalidNameScalars(int scalar) => InvalidNames("a"+new string((char)scalar,1));

    /// <summary>Unsupported static/module/mixed contexts never gain insertion authority.</summary>
    /// <param name="body">The constructor body.</param>
    /// <param name="extra">Extra root attributes.</param>
    /// <param name="declaration">A top-level declaration.</param>
    /// <param name="kind">The classified refusal.</param>
    [Theory]
    [InlineData("<out/>","","",AuthoringInsertionFailureKind.SelfClosingParent)]
    [InlineData("<out>significant</out>","","",AuthoringInsertionFailureKind.MixedContent)]
    [InlineData("<out><![CDATA[ ]]></out>","","",AuthoringInsertionFailureKind.MixedContent)]
    [InlineData("<out xml:space='preserve'></out>","","",AuthoringInsertionFailureKind.MixedContent)]
    [InlineData("<out></out>","","<xsl:namespace-alias stylesheet-prefix='#default' result-prefix='xsl'/>",AuthoringInsertionFailureKind.UnsupportedCoverage)]
    [InlineData("<out></out>","extension-element-prefixes='xsl'","",AuthoringInsertionFailureKind.UnsupportedCoverage)]
    [InlineData("<out></out>","","<xsl:variable name='a' static='yes' select=\"doc('https://invalid.example/')\"/>",AuthoringInsertionFailureKind.UnsupportedCoverage)]
    [InlineData("<out></out>","","<xsl:variable name='a' _select=\"doc('https://invalid.example/')\"/>",AuthoringInsertionFailureKind.UnsupportedCoverage)]
    [InlineData("<out><xsl:text disable-output-escaping='yes'>x</xsl:text></out>","","",AuthoringInsertionFailureKind.UnsupportedCoverage)]
    public void ScopeRefusals(string body,string extra,string declaration,AuthoringInsertionFailureKind kind)
    {
        var source=Sheet(body,extra).Replace("<xsl:output",declaration+"<xsl:output");var s=Inspect(source);
        var result=s.GetLiteralElementInsertionTarget(Find(s,"out"));Assert.Null(result.Target);Assert.Equal(kind,result.Failure!.Kind);
        Assert.Equal(Encoding.UTF8.GetBytes(source),s.ExportOriginal(ModuleUri));
    }

    /// <summary>Protects direct comments, PIs, payloads, attributes and uniform physical newlines.</summary>
    /// <param name="newline">The retained physical newline.</param>
    [Theory][InlineData("\n")][InlineData("\r\n")][InlineData("\r")]
    public void ReadableFidelity(string newline)
    {
        var s=Inspect(Sheet("<out>"+newline+"  <!-- keep -->"+newline+"  <?keep x?>"+newline+"  <nested a='&amp;'><xsl:text> A&#xD;&#xA;{old} </xsl:text></nested>"+newline+"</out>"));
        var p=Insert(s);var c=Insert(s,mode:AuthoringInsertionFormattingMode.Readable);
        Assert.Equal(Run(p.ExportSource()),Run(c.ExportSource()));
        Assert.Contains("a='&amp;'",Encoding.UTF8.GetString(c.ExportSource()));Assert.Contains(" A&#xD;&#xA;{old} ",Encoding.UTF8.GetString(c.ExportSource()));
        Assert.True(c.NewNodes.Count>1);
    }
    /// <summary>Default layout equals explicit PreserveSource and rejects undefined argument modes.</summary>
    [Fact]
    public void DefaultsAndArgumentContracts()
    {
        var s=Inspect(Sheet());var target=s.GetLiteralElementInsertionTarget(Find(s,"out")).Target!;
        Assert.Equal(Insert(s).ExportSource(),s.ProposeLiteralElementInsertion(new(target,"item",AuthoringInsertionFormattingMode.PreserveSource)).Candidate!.ExportSource());
        Assert.Throws<ArgumentNullException>(()=>s.GetLiteralElementInsertionTarget(null!));Assert.Throws<ArgumentNullException>(()=>s.ProposeLiteralElementInsertion(null!));
        Assert.Throws<ArgumentNullException>(()=>new AuthoringLiteralElementInsertionProposal(null!,"item"));Assert.Throws<ArgumentNullException>(()=>new AuthoringLiteralElementInsertionProposal(target,null!));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new AuthoringLiteralElementInsertionProposal(target,"item",(AuthoringInsertionFormattingMode)99));
    }
    /// <summary>Ownership is by snapshot identity, not identical bytes, and exports/collections are defensive.</summary>
    [Fact]
    public void OwnershipAndImmutability()
    {
        var a=Inspect(Sheet());var b=Inspect(Sheet());var node=Find(a,"out");var target=a.GetLiteralElementInsertionTarget(node).Target!;
        Assert.Equal(AuthoringInsertionFailureKind.ForeignSnapshot,b.GetLiteralElementInsertionTarget(node).Failure!.Kind);
        Assert.Equal(AuthoringInsertionFailureKind.ForeignSnapshot,b.ProposeLiteralElementInsertion(new(target,"item")).Failure!.Kind);
        var c=Insert(a);var bytes=c.ExportSource();bytes[0]=0;Assert.NotEqual(bytes,c.ExportSource());
        Assert.Throws<NotSupportedException>(()=>((IDictionary<int,int>)c.NodeCorrespondence).Clear());
        Assert.Throws<NotSupportedException>(()=>((IList<AuthoringNodeDescriptor>)c.NewNodes).Clear());
        Assert.Throws<NotSupportedException>(()=>((IList<AuthoringInsertionSourceEdit>)c.SourceEdits).Clear());
        Assert.True(a.ProposeLiteralElementInsertion(new(target,"again")).IsSuccess);
    }
    /// <summary>Pre-cancelled target and proposal calls return classified cancellation without a candidate.</summary>
    [Fact]
    public void Cancellation()
    {
        var s=Inspect(Sheet());var parent=Find(s,"out");var target=s.GetLiteralElementInsertionTarget(parent).Target!;
        using var cts=new CancellationTokenSource();cts.Cancel();
        Assert.Equal(AuthoringInsertionFailureKind.Cancelled,s.GetLiteralElementInsertionTarget(parent,cts.Token).Failure!.Kind);
        Assert.Equal(AuthoringInsertionFailureKind.Cancelled,s.ProposeLiteralElementInsertion(new(target,"item"),cts.Token).Failure!.Kind);
    }
    /// <summary>Repeated append and nested creation use actual fresh nodes without reusing old targets.</summary>
    [Fact]
    public void RepeatedAndNestedContainers()
    {
        var c=Insert(Inspect(Sheet()));var nested=Insert(c.Snapshot,"child",parent:"item");var again=Insert(nested.Snapshot,"sibling");
        var result=XDocument.Parse(Run(again.ExportSource()));Assert.Equal(new[]{"item","sibling"},result.Root!.Elements().Select(e=>e.Name.LocalName));Assert.Equal("child",result.Root.Element("item")!.Elements().Single().Name.LocalName);
    }
    /// <summary>Both currently bounded language versions support the same operation.</summary>
    /// <param name="version">The supported explicit version.</param>
    [Theory][InlineData("3.0")][InlineData("4.0")]
    public void SupportedVersions(string version) => Insert(Inspect(Sheet(version:version)));
    /// <summary>Ancestor preserve and ambiguous newlines refuse Readable without changing default mode.</summary>
    /// <param name="body">The otherwise eligible constructor.</param>
    /// <param name="extra">The ancestor XML space declaration.</param>
    [Theory][InlineData("<out xml:space='default'></out>","xml:space='preserve'")][InlineData("<out>\r\n<nested/>\n</out>","")]
    public void ReadableRefusal(string body,string extra)
    {
        var s=Inspect(Sheet(body,extra));var t=s.GetLiteralElementInsertionTarget(Find(s,"out"));Assert.True(t.IsSuccess,t.Failure?.Message);
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedFormatting,s.ProposeLiteralElementInsertion(new(t.Target!,"item",AuthoringInsertionFormattingMode.Readable)).Failure!.Kind);
        Assert.True(s.ProposeLiteralElementInsertion(new(t.Target!,"item")).IsSuccess);
    }
    /// <summary>Candidate bytes at the inclusive bound succeed; one additional name byte refuses.</summary>
    /// <param name="utf16">Whether bytes are UTF-16.</param>
    /// <param name="extra">Whether the complete candidate is too large.</param>
    [Theory][InlineData(false,false)][InlineData(false,true)][InlineData(true,false)][InlineData(true,true)]
    public void CandidateByteBoundary(bool utf16,bool extra)
    {
        Encoding enc=utf16 ? new UnicodeEncoding(false,true,true) : new UTF8Encoding(false,true);
        var prefix=utf16 ? "<?xml version='1.0' encoding='utf-16'?>" : "";
        var small=prefix+Sheet("<!----><out></out>");var limit=AuthoringCapabilities.MaxLiteralElementInsertionSourceBytes;
        var padding=(limit-enc.GetPreamble().Length-enc.GetByteCount(small)-enc.GetByteCount("<item></item>"))/(utf16?2:1);
        var text=prefix+Sheet("<!--"+new string('a',padding+(extra?1:0))+"--><out></out>");var s=Inspect(enc.GetPreamble().Concat(enc.GetBytes(text)).ToArray());
        var t=s.GetLiteralElementInsertionTarget(Find(s,"out"));Assert.True(t.IsSuccess,t.Failure?.Message);
        var result=s.ProposeLiteralElementInsertion(new(t.Target!,"item"));
        if(extra)Assert.Equal(AuthoringInsertionFailureKind.SizeLimit,result.Failure!.Kind);
        else {Assert.True(result.IsSuccess,result.Failure?.Message);Assert.Equal(limit,result.Candidate!.ExportSource().Length);}
    }
    /// <summary>Readable accounts for replaced physical whitespace rather than a hypothetical pure append.</summary>
    [Fact]
    public void ReadableShrinkToFitAndOriginalLimit()
    {
        var small=Sheet("<out></out>");var count=AuthoringCapabilities.MaxLiteralElementInsertionSourceBytes-Encoding.UTF8.GetByteCount(small)-2;
        var s=Inspect(Sheet("<out>"+new string(' ',count)+"</out>"));var t=s.GetLiteralElementInsertionTarget(Find(s,"out")).Target!;
        Assert.Equal(AuthoringInsertionFailureKind.SizeLimit,s.ProposeLiteralElementInsertion(new(t,"item")).Failure!.Kind);
        var c=s.ProposeLiteralElementInsertion(new(t,"item",AuthoringInsertionFormattingMode.Readable));Assert.True(c.IsSuccess,c.Failure?.Message);Verify(c.Candidate!);
        var oversized=Inspect(Sheet("<out>"+new string(' ',count+3)+"</out>"));
        Assert.Equal(AuthoringInsertionFailureKind.SizeLimit,oversized.GetLiteralElementInsertionTarget(Find(oversized,"out")).Failure!.Kind);
    }
    /// <summary>Oversized repeated names refuse before allocating candidate markup.</summary>
    [Fact]
    public void OversizedName()
    {
        var s=Inspect(Sheet());var t=s.GetLiteralElementInsertionTarget(Find(s,"out")).Target!;
        Assert.Equal(AuthoringInsertionFailureKind.SizeLimit,s.ProposeLiteralElementInsertion(new(t,new string('a',AuthoringCapabilities.MaxLiteralElementInsertionSourceBytes/2+1))).Failure!.Kind);
    }
    /// <summary>Capabilities are independently advertised with immutable bounds and encodings.</summary>
    [Fact]
    public void Capabilities()
    {
        Assert.True(AuthoringCapabilities.SupportsLiteralElementInsertion);Assert.True(AuthoringCapabilities.SupportsReadableLiteralElementInsertion);
        Assert.Equal(8_388_608,AuthoringCapabilities.MaxLiteralElementInsertionSourceBytes);
        Assert.Equal(new[]{"UTF-8","UTF-16LE","UTF-16BE"},AuthoringCapabilities.LiteralElementInsertionSupportedEncodings);
        Assert.Throws<NotSupportedException>(()=>((IList<string>)AuthoringCapabilities.LiteralElementInsertionSupportedEncodings).Clear());
        Assert.Equal(18,(int)AuthoringInsertionFailureKind.InvalidPayload);Assert.Equal(19,(int)AuthoringInsertionFailureKind.InvalidElementName);
    }
    /// <summary>Unsupported explicit language versions retain existing scope classifications.</summary>
    /// <param name="version">The excluded version.</param>
    [Theory][InlineData("1.0")][InlineData("2.0")][InlineData("5.0")]
    public void UnsupportedVersions(string version)
    {
        var s=Inspect(Sheet(version:version));Assert.Equal(AuthoringInsertionFailureKind.UnsupportedProfile,s.GetLiteralElementInsertionTarget(Find(s,"out")).Failure!.Kind);
    }
    /// <summary>Module references refuse without acquiring resources during discovery or proposal.</summary>
    [Fact]
    public void DependenciesAndResourceAuthority()
    {
        var text=Sheet().Replace("<xsl:output","<xsl:include href='https://invalid.example/other.xsl'/><xsl:output");
        var resolver=new CountingResolver();Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text),ModuleUri,out var source,out _));
        var inspection=XsltAuthoring.Inspect(source!,resolver,new(){AttemptCompilation=false});Assert.True(inspection.IsSuccess);
        var before=resolver.Calls;var result=inspection.Snapshot!.GetLiteralElementInsertionTarget(Find(inspection.Snapshot,"out"));
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedModule,result.Failure!.Kind);Assert.Equal(before,resolver.Calls);
    }
    private sealed class CountingResolver : IAuthoringModuleResolver
    {
        internal int Calls { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri,Uri referencingModuleUri){Calls++;return null;}
    }
    /// <summary>Inspection encodings outside the structural profile never trigger conversion.</summary>
    [Fact]
    public void UnsupportedStructuralEncoding()
    {
        var text="<?xml version='1.0' encoding='us-ascii'?>"+Sheet();var s=Inspect(Encoding.ASCII.GetBytes(text));
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedEncoding,s.GetLiteralElementInsertionTarget(Find(s,"out")).Failure!.Kind);
    }
    /// <summary>Empty reset containers can be nested beneath an unprefixed XSLT-default template.</summary>
    [Fact]
    public void DefaultXsltWithoutAnyNonemptyXsltPrefix()
    {
        var text=$"<stylesheet xmlns='{Xsl}' version='3.0'><output method='xml' omit-xml-declaration='yes'/><template match='/'></template></stylesheet>";
        var c=Insert(Inspect(text),parent:"template");Assert.Equal("<item xmlns=\"\"></item>",Lexical(c.ExportSource(),c.InsertedRange));
        var t=c.Snapshot.GetTextInsertionTarget(c.InsertedNode);Assert.True(t.IsSuccess,t.Failure?.Message);
        var result=c.Snapshot.ProposeTextInsertion(new(t.Target!,"T"));Assert.True(result.IsSuccess,result.Failure?.Message);Assert.Equal("T",XDocument.Parse(Run(result.Candidate!.ExportSource())).Root!.Value);
        var v=c.Snapshot.GetValueOfInsertionTarget(c.InsertedNode);Assert.True(v.IsSuccess,v.Failure?.Message);
        var value=c.Snapshot.ProposeValueOfInsertion(new(v.Target!,"'V'"));Assert.True(value.IsSuccess,value.Failure?.Message);Assert.Equal("V",XDocument.Parse(Run(value.Candidate!.ExportSource())).Root!.Value);
    }
    /// <summary>Concurrent proposals remain isolated and deterministic against one immutable owner.</summary>
    [Fact]
    public async Task ConcurrentProposals()
    {
        var s=Inspect(Sheet());var target=s.GetLiteralElementInsertionTarget(Find(s,"out")).Target!;
        var names=new[]{"a","b","é","xml"};var tasks=names.Select(name=>Task.Run(()=>s.ProposeLiteralElementInsertion(new(target,name))));
        var results=await Task.WhenAll(tasks);
        for(var i=0;i<results.Length;i++){Assert.True(results[i].IsSuccess,results[i].Failure?.Message);Verify(results[i].Candidate!);Assert.Equal(names[i],results[i].Candidate!.LocalName);}
        Assert.Equal(Encoding.UTF8.GetBytes(Sheet()),s.ExportOriginal(ModuleUri));
    }
    /// <summary>New layout restrictions do not narrow the historical value-of text controls.</summary>
    [Fact]
    public void LegacyOperationCompatibility()
    {
        var s=Inspect(Sheet("<out><xsl:text disable-output-escaping='yes'>old</xsl:text></out>"));
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedCoverage,s.GetLiteralElementInsertionTarget(Find(s,"out")).Failure!.Kind);
        var t=s.GetValueOfInsertionTarget(Find(s,"out"));Assert.True(t.IsSuccess,t.Failure?.Message);
        Assert.True(s.ProposeValueOfInsertion(new(t.Target!,"'V'")).IsSuccess);
    }

}
