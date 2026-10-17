// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 October 2026
// PURPOSE              : Verify literal text insertion, payload fidelity, layout, ownership and full source provenance.
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

/// <summary>Actual API evidence for REQ-131 literal-text insertion and mixed authoring workflows.</summary>
public class AuthoringTextInsertionTests
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static readonly Uri ModuleUri = new("file:///C:/authoring/text.xsl");
    private static string Sheet(string body = "<out></out>", string extra = "", string version = "3.0") =>
        $"<xsl:stylesheet xmlns:xsl='{Xsl}' version='{version}' {extra}><xsl:output method='text'/><xsl:template match='/'>{body}</xsl:template></xsl:stylesheet>";
    private static AuthoringSnapshot Inspect(string text, bool compile = false) => Inspect(Encoding.UTF8.GetBytes(text), compile);
    private static AuthoringSnapshot Inspect(byte[] bytes, bool compile = false)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, ModuleUri, out var source, out var failure), failure?.Message);
        var result = XsltAuthoring.Inspect(source!, options: new() { AttemptCompilation = compile });
        Assert.True(result.IsSuccess, result.Failure?.Message);
        return result.Snapshot!;
    }
    private static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor node) => new[] { node }.Concat(node.Children.SelectMany(Nodes));
    private static AuthoringNodeDescriptor Find(AuthoringSnapshot s, string name) => Nodes(s.PrincipalModule.Root).First(n => n.ElementName?.LocalName == name);
    private static AuthoringTextInsertionCandidate Insert(AuthoringSnapshot snapshot, string value,
        AuthoringInsertionFormattingMode mode = AuthoringInsertionFormattingMode.PreserveSource, string parent = "out")
    {
        var target = snapshot.GetTextInsertionTarget(Find(snapshot, parent)); Assert.True(target.IsSuccess, target.Failure?.Message);
        var result = snapshot.ProposeTextInsertion(new(target.Target!, value, mode)); Assert.True(result.IsSuccess, result.Failure?.Message);
        Verify(result.Candidate!); return result.Candidate!;
    }
    private static string Run(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return new XsltCompiler().Compile(XDocument.Load(stream, LoadOptions.PreserveWhitespace), ModuleUri.AbsoluteUri)
            .TransformToString(new XDocumentNode(XDocument.Parse("<input/>")));
    }
    private static string Lexical(byte[] bytes, SourceRange range)
    {
        Assert.True(AuthoringSource.TryCreate(bytes, ModuleUri, out var source, out _));
        return source!.Encoding.GetString(bytes.AsSpan((int)range.StartByteOffset, (int)range.ByteLength));
    }
    private static void Verify(AuthoringTextInsertionCandidate c)
    {
        var original = c.InputSnapshot.ExportOriginal(c.ModuleUri); var fresh = c.ExportSource();
        var output = new List<byte>(); var cursor = 0;
        foreach (var e in c.SourceEdits)
        {
            Assert.InRange(e.OriginalRange.StartByteOffset, cursor, original.Length);
            output.AddRange(original[cursor..(int)e.OriginalRange.StartByteOffset]);
            Assert.Equal(output.Count, e.ReplacementRange.StartByteOffset);
            output.AddRange(fresh.AsSpan((int)e.ReplacementRange.StartByteOffset, (int)e.ReplacementRange.ByteLength).ToArray());
            cursor = (int)(e.OriginalRange.StartByteOffset + e.OriginalRange.ByteLength);
        }
        output.AddRange(original[cursor..]); Assert.Equal(fresh, output.ToArray());
        Assert.Equal(0, c.OriginalRange.ByteLength); Assert.Equal(c.InsertedNode.Range, c.InsertedRange);
        Assert.Equal("text", c.InsertedNode.ElementName!.LocalName); Assert.Equal(Xsl, c.InsertedNode.ElementName.NamespaceUri);
        Assert.Contains(c.InsertedNode.Attributes, a => a.Name == "expand-text" && a.ExpandedValue == "no");
        Assert.DoesNotContain(c.InsertedNode.Attributes, a => a.Name == "select" || a.Name == "disable-output-escaping");
        var olds = Nodes(c.InputSnapshot.PrincipalModule.Root).ToArray(); var news = Nodes(c.Snapshot.PrincipalModule.Root).ToDictionary(n => n.Id);
        Assert.Equal(olds.Length, c.NodeCorrespondence.Count); Assert.Equal(olds.Length, c.NodeCorrespondence.Values.Distinct().Count());
        var inserted = Nodes(c.InsertedNode).Select(n => n.Id).ToHashSet();
        foreach (var old in olds)
        {
            Assert.True(c.NodeCorrespondence.TryGetValue(old.Id, out var id)); Assert.DoesNotContain(id, inserted);
            var current = news[id]; Assert.Equal(old.Kind, current.Kind); Assert.Equal(old.ElementName, current.ElementName);
            Assert.Equal(old.Attributes.Select(a => (a.Name, a.RawLiteral, a.ExpandedValue)), current.Attributes.Select(a => (a.Name, a.RawLiteral, a.ExpandedValue)));
            var affectedText = old.Kind == AuthoringNodeKind.Text && c.SourceEdits.Any(e => e.OriginalRange.ByteLength > 0
                && old.Range.StartByteOffset >= e.OriginalRange.StartByteOffset && old.Range.StartByteOffset + old.Range.ByteLength <= e.OriginalRange.StartByteOffset + e.OriginalRange.ByteLength);
            if (!affectedText) Assert.Equal(old.Text, current.Text);
            var order = old.Children.Select(child => current.Children.ToList().FindIndex(n => n.Id == c.NodeCorrespondence[child.Id])).ToArray();
            Assert.All(order, i => Assert.True(i >= 0)); Assert.Equal(order.Order().ToArray(), order);
        }
        foreach (var node in news.Values.Where(n => !inserted.Contains(n.Id) && !c.NodeCorrespondence.Values.Contains(n.Id)))
        {
            Assert.Equal(AuthoringNodeKind.Text, node.Kind); Assert.True(node.Text!.All(ch => ch is ' ' or '\r' or '\n' or '\t'));
            Assert.Contains(c.SourceEdits, e => node.Range.StartByteOffset >= e.ReplacementRange.StartByteOffset && node.Range.StartByteOffset + node.Range.ByteLength <= e.ReplacementRange.StartByteOffset + e.ReplacementRange.ByteLength);
        }
        if (c.TextValue.Length == 0) { Assert.Null(c.PayloadNode); Assert.Equal(0, c.TextPayloadRange.ByteLength); Assert.Empty(c.InsertedNode.Children); }
        else { Assert.Same(Assert.Single(c.InsertedNode.Children), c.PayloadNode); Assert.Equal(c.TextValue, c.PayloadNode!.Text); Assert.Equal(c.TextPayloadRange, c.PayloadNode.Range); }
        var mutated = c.ExportSource(); mutated[0] ^= 1; Assert.Equal(fresh, c.ExportSource()); Assert.Equal(original, c.InputSnapshot.ExportOriginal(c.ModuleUri));
        Assert.Throws<NotSupportedException>(() => ((IList<AuthoringInsertionSourceEdit>)c.SourceEdits).Clear());
        Assert.Throws<NotSupportedException>(() => ((IDictionary<int, int>)c.NodeCorrespondence).Clear());
        Assert.True(c.Snapshot.IsCompilable);
    }

    /// <summary>Independent logical/source golden payloads, including literal controls and hostile-looking strings.</summary>
    /// <returns>Pairs of exact logical text and expected encoded source content.</returns>
    public static IEnumerable<object[]> Payloads()
    {
        yield return new object[] { "Label: ", "Label: " };
        yield return new object[] { "", "" };
        yield return new object[] { " \t\r\n", " &#x9;&#xD;&#xA;" };
        yield return new object[] { "&<>", "&amp;&lt;&gt;" };
        yield return new object[] { "'\"", "'\"" };
        yield return new object[] { "]]>", "]]&gt;" };
        yield return new object[] { "&amp;", "&amp;amp;" };
        yield return new object[] { "</xsl:text><evil/>", "&lt;/xsl:text&gt;&lt;evil/&gt;" };
        yield return new object[] { "{customer} {{braces}} $missing {1 div 0}", "{customer} {{braces}} $missing {1 div 0}" };
        yield return new object[] { "{doc('https://invalid.example/')}", "{doc('https://invalid.example/')}" };
        yield return new object[] { "𝄞 e\u0301 é 中文 \uFEFF", "𝄞 e\u0301 é 中文 \uFEFF" };
    }

    /// <summary>Literal payloads preserve exact code points, encoded ranges and output under both profiles.</summary>
    /// <param name="payload">Logical text.</param>
    /// <param name="encoded">Independent expected lexical content.</param>
    [Theory][MemberData(nameof(Payloads))]
    public void LiteralPayloadGolden(string payload, string encoded)
    {
        foreach (var version in new[] { "3.0", "4.0" })
        {
            var snapshot = Inspect(Sheet(version: version)); var c = Insert(snapshot, payload);
            Assert.Equal(payload, Run(c.ExportSource())); Assert.Equal(encoded, Lexical(c.ExportSource(), c.TextPayloadRange));
            Assert.Equal("<xsl:text expand-text=\"no\">" + encoded + "</xsl:text>", Lexical(c.ExportSource(), c.InsertedRange));
            Assert.Equal(c.TextPayloadRange.StartLine, c.TextPayloadRange.EndLine);
            Assert.Equal(encoded.Length, c.TextPayloadRange.EndColumn - c.TextPayloadRange.StartColumn);
        }
    }

    /// <summary>Inherited expansion never interprets inserted text as a value template or XPath.</summary>
    /// <param name="expand">The inherited setting.</param>
    [Theory][InlineData("yes")][InlineData("true")][InlineData("1")][InlineData("no")][InlineData("false")][InlineData("0")]
    public void ExplicitLiteralOverride(string expand)
    {
        var payload = "{unknown:missing()} {doc('https://invalid.example/')}";
        var c = Insert(Inspect(Sheet(extra: "expand-text='" + expand + "'")), payload);
        Assert.Equal(payload, Run(c.ExportSource()));
    }

    /// <summary>Four source envelopes and both formatting modes preserve exact payload and byte evidence.</summary>
    /// <param name="kind">The encoding envelope.</param>
    /// <param name="readable">Whether Readable layout is requested.</param>
    [Theory][InlineData(0,false)][InlineData(1,false)][InlineData(2,false)][InlineData(3,false)]
    [InlineData(0,true)][InlineData(1,true)][InlineData(2,true)][InlineData(3,true)]
    public void Encodings(int kind, bool readable)
    {
        Encoding encoding = kind switch { 0 => new UTF8Encoding(false,true), 1 => new UTF8Encoding(true,true), 2 => new UnicodeEncoding(false,true,true), _ => new UnicodeEncoding(true,true,true) };
        var text = $"<?xml version='1.0' encoding='{(kind < 2 ? "utf-8" : "utf-16")}'?>" + Sheet("\r\n  <out>\r\n  </out>\r\n");
        var bytes = encoding.GetPreamble().Concat(encoding.GetBytes(text)).ToArray();
        var c = Insert(Inspect(bytes), " 𝄞 &\r\n\t{literal} ", readable ? AuthoringInsertionFormattingMode.Readable : AuthoringInsertionFormattingMode.PreserveSource);
        Assert.Equal(" 𝄞 &\r\n\t{literal} ", Run(c.ExportSource()));
        Assert.Equal(encoding.GetPreamble(), c.ExportSource()[..encoding.GetPreamble().Length]);
        Assert.Equal(" 𝄞 &amp;&#xD;&#xA;&#x9;{literal} ", Lexical(c.ExportSource(), c.TextPayloadRange));
    }

    /// <summary>Invalid XML scalars and isolated surrogates are not replaced or interpreted as expressions.</summary>
    [Fact]
    public void InvalidPayloads()
    {
        var s = Inspect(Sheet()); var target = s.GetTextInsertionTarget(Find(s,"out")).Target!;
        foreach (var payload in new[] { "\0", "\u0001", "\uFFFE", "\uFFFF", "\uD800", "\uDC00", "A\uD800B" })
        {
            var result = s.ProposeTextInsertion(new(target, payload)); Assert.False(result.IsSuccess); Assert.Null(result.Candidate);
            Assert.Equal(AuthoringInsertionFailureKind.InvalidPayload, result.Failure!.Kind);
        }
        Assert.Throws<ArgumentNullException>(() => new AuthoringTextInsertionProposal(target,null!));
        Assert.Throws<ArgumentNullException>(() => new AuthoringTextInsertionProposal(null!,""));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AuthoringTextInsertionProposal(target,"",(AuthoringInsertionFormattingMode)99));
        Assert.Throws<ArgumentNullException>(() => s.GetTextInsertionTarget(null!));
        Assert.Throws<ArgumentNullException>(() => s.ProposeTextInsertion(null!));
        Assert.Equal((int)AuthoringInsertionFailureKind.UnsupportedFormatting + 1, (int)AuthoringInsertionFailureKind.InvalidPayload);
    }

    /// <summary>Repeated text/value-of insertion and value-of deletion retain typed source ownership.</summary>
    /// <param name="readable">Whether layout is readable.</param>
    /// <param name="compile">Whether initial inspection compiled its derived tree.</param>
    [Theory][InlineData(false,false)][InlineData(true,false)][InlineData(false,true)][InlineData(true,true)]
    public void MixedAuthoringLifecycle(bool readable, bool compile)
    {
        var mode = readable ? AuthoringInsertionFormattingMode.Readable : AuthoringInsertionFormattingMode.PreserveSource;
        var c1 = Insert(Inspect(Sheet("\n  <out>\n  </out>\n"),compile), "A ",mode);
        var s = c1.Snapshot; var valueTarget = s.GetValueOfInsertionTarget(Find(s,"out")); Assert.True(valueTarget.IsSuccess,valueTarget.Failure?.Message);
        var value = s.ProposeValueOfInsertion(new(valueTarget.Target!,"'B'",mode)); Assert.True(value.IsSuccess,value.Failure?.Message);
        var c2 = Insert(value.Candidate!.Snapshot," C",mode); Assert.Equal("A B C", Run(c2.ExportSource()));
        var instruction = Find(c2.Snapshot,"value-of"); var deletionTarget = c2.Snapshot.GetValueOfDeletionTarget(instruction); Assert.True(deletionTarget.IsSuccess,deletionTarget.Failure?.Message);
        var deleted = c2.Snapshot.ProposeValueOfDeletion(new(deletionTarget.Target!)); Assert.True(deleted.IsSuccess,deleted.Failure?.Message);
        var c3 = Insert(deleted.Candidate!.Snapshot,"D",mode); Assert.Equal("A  CD",Run(c3.ExportSource()));
    }

    /// <summary>New text coverage is narrow without tightening previously valid legacy value-of cases.</summary>
    /// <param name="existing">Existing literal, expanding or output-escaping text.</param>
    /// <param name="textSupported">Whether the new literal operation accepts it.</param>
    [Theory]
    [InlineData("<xsl:text> X </xsl:text>",true)]
    [InlineData("<xsl:text expand-text='no'>{x}</xsl:text>",true)]
    [InlineData("<xsl:text disable-output-escaping='no'> X </xsl:text>",true)]
    [InlineData("<xsl:text disable-output-escaping='yes'>&lt;b&gt;</xsl:text>",false)]
    [InlineData("<xsl:text expand-text='yes'> </xsl:text>",false)]
    public void ExistingTextCompatibility(string existing, bool textSupported)
    {
        var s = Inspect(Sheet("<out>" + existing + "</out>"));
        var legacy = s.GetValueOfInsertionTarget(Find(s,"out")); Assert.True(legacy.IsSuccess,legacy.Failure?.Message);
        Assert.True(s.ProposeValueOfInsertion(new(legacy.Target!,"'A'")).IsSuccess);
        var target = s.GetTextInsertionTarget(Find(s,"out")); Assert.Equal(textSupported,target.IsSuccess);
        if (textSupported) { var c=Insert(s,"Y"); Assert.Contains(existing, Encoding.UTF8.GetString(c.ExportSource())); }
        else Assert.Equal(AuthoringInsertionFailureKind.UnsupportedCoverage,target.Failure!.Kind);
    }

    /// <summary>Default-only and colliding bindings require a local declaration without changing old markup.</summary>
    /// <param name="collision">Whether xsl and xsl1 are already bound to unrelated namespaces.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void PrefixChoice(bool collision)
    {
        var text = $"<stylesheet xmlns='{Xsl}' version='3.0'><output method='text'/><template match='/'><out xmlns='' {(collision ? "xmlns:xsl='urn:other' xmlns:xsl1='urn:taken'" : "")}></out></template></stylesheet>";
        var c = Insert(Inspect(text),"X");
        Assert.Equal(collision ? "xsl2" : "xsl", c.InsertedNode.ElementName!.Prefix);
        Assert.Contains("xmlns:" + c.InsertedNode.ElementName.Prefix + "=\"" + Xsl + "\"", Lexical(c.ExportSource(),c.InsertedRange));
        Assert.Equal("X",Run(c.ExportSource()));
    }

    /// <summary>Scope exclusions and ambiguous formatting return no candidate.</summary>
    /// <param name="body">The unsupported constructor source.</param>
    [Theory]
    [InlineData("<out/>")][InlineData("<out>mixed</out>")][InlineData("<out><![CDATA[]]></out>")]
    [InlineData("<out xml:space='preserve'> </out>")]
    [InlineData("<out><xsl:text expand-text='yes'>{unknown:missing()}</xsl:text></out>")]
    public void RefusedContexts(string body)
    {
        var s=Inspect(Sheet(body)); var original=s.ExportOriginal(ModuleUri);var target=s.GetTextInsertionTarget(Find(s,"out"));
        Assert.False(target.IsSuccess);Assert.Null(target.Target);Assert.Equal(original,s.ExportOriginal(ModuleUri));
    }

    /// <summary>Readable preserves its conservative ancestor guard and never silently falls back.</summary>
    [Fact]
    public void FormattingRefusals()
    {
        var s=Inspect(Sheet("<outer xml:space='preserve'><out xml:space='default'></out></outer>"));
        var target=s.GetTextInsertionTarget(Find(s,"out"));Assert.True(target.IsSuccess,target.Failure?.Message);
        var result=s.ProposeTextInsertion(new(target.Target!,"X",AuthoringInsertionFormattingMode.Readable));
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedFormatting,result.Failure!.Kind);Assert.Null(result.Candidate);
        var mixed=Inspect(Sheet("\r\n<out>\n</out>"));var t=mixed.GetTextInsertionTarget(Find(mixed,"out")).Target!;
        Assert.Equal(AuthoringInsertionFailureKind.UnsupportedFormatting,mixed.ProposeTextInsertion(new(t,"X",AuthoringInsertionFormattingMode.Readable)).Failure!.Kind);
    }

    /// <summary>Reference ownership, old-snapshot reuse, cancellation and concurrency remain independent.</summary>
    /// <returns>Completion of the concurrent proposal checks.</returns>
    [Fact]
    public async Task OwnershipCancellationConcurrency()
    {
        var a=Inspect(Sheet());var b=Inspect(Sheet());var parent=Find(a,"out");
        Assert.Equal(AuthoringInsertionFailureKind.ForeignSnapshot,a.GetTextInsertionTarget(Find(b,"out")).Failure!.Kind);
        var target=a.GetTextInsertionTarget(parent).Target!;var proposal=new AuthoringTextInsertionProposal(target,"X");
        Assert.Equal(AuthoringInsertionFailureKind.ForeignSnapshot,b.ProposeTextInsertion(proposal).Failure!.Kind);
        var token=new CancellationToken(true);
        Assert.Equal(AuthoringInsertionFailureKind.Cancelled,a.GetTextInsertionTarget(parent,token).Failure!.Kind);
        Assert.Equal(AuthoringInsertionFailureKind.Cancelled,a.ProposeTextInsertion(proposal,token).Failure!.Kind);
        var results=await Task.WhenAll(Enumerable.Range(0,4).Select(_=>Task.Run(()=>a.ProposeTextInsertion(proposal))));
        foreach(var result in results){Assert.True(result.IsSuccess,result.Failure?.Message);Verify(result.Candidate!);}
        Assert.NotSame(results[0].Candidate!.Snapshot,results[1].Candidate!.Snapshot);
        Assert.True(a.ProposeTextInsertion(proposal).IsSuccess);
    }
    /// <summary>Instruction and escaped-payload amplification respect the inclusive candidate byte bound.</summary>
    /// <param name="utf16">Whether physical UTF-16 bytes determine the bound.</param>
    /// <param name="extra">Whether one more payload scalar must refuse.</param>
    [Theory][InlineData(false,false)][InlineData(false,true)][InlineData(true,false)][InlineData(true,true)]
    public void CandidateByteBoundary(bool utf16, bool extra)
    {
        Encoding enc = utf16 ? new UnicodeEncoding(false,true,true) : new UTF8Encoding(false,true);
        var text = (utf16 ? "<?xml version='1.0' encoding='utf-16'?>" : "") + Sheet();
        var bytes=enc.GetPreamble().Concat(enc.GetBytes(text)).ToArray();var snapshot=Inspect(bytes);
        var empty=Insert(snapshot,"");var remaining=AuthoringCapabilities.MaxTextInsertionSourceBytes-empty.ExportSource().Length;
        var scalarBytes=utf16 ? 10 : 5;var ampersands=remaining/scalarBytes;var rest=(remaining-ampersands*scalarBytes)/(utf16 ? 2 : 1);
        var payload=new string('&',ampersands)+new string('a',rest)+(extra ? "A" : "");
        var target=snapshot.GetTextInsertionTarget(Find(snapshot,"out")).Target!;var result=snapshot.ProposeTextInsertion(new(target,payload));
        if(extra){Assert.Null(result.Candidate);Assert.Equal(AuthoringInsertionFailureKind.SizeLimit,result.Failure!.Kind);}
        else { Assert.True(result.IsSuccess,result.Failure?.Message);Assert.Equal(AuthoringCapabilities.MaxTextInsertionSourceBytes,result.Candidate!.ExportSource().Length); }
    }

    /// <summary>Original source is bounded even when it contains only otherwise harmless metadata padding.</summary>
    [Fact]
    public void OriginalByteLimit()
    {
        var small=Sheet("<!----><out></out>");var padding=AuthoringCapabilities.MaxTextInsertionSourceBytes-Encoding.UTF8.GetByteCount(small)+1;
        var snapshot=Inspect(Sheet("<!--"+new string('x',padding)+"--><out></out>"));
        Assert.Equal(AuthoringInsertionFailureKind.SizeLimit,snapshot.GetTextInsertionTarget(Find(snapshot,"out")).Failure!.Kind);
    }

    /// <summary>Readable whitespace shrinkage may fit where a pure append exceeds the bound.</summary>
    [Fact]
    public void ReadableFootprintBudget()
    {
        var small=Sheet("<out>"+new string(' ',1000)+"</out>");
        var prefix=AuthoringCapabilities.MaxTextInsertionSourceBytes-Encoding.UTF8.GetByteCount(small)-2;
        var snapshot=Inspect(Sheet("<out>"+new string(' ',1000+prefix)+"</out>"));
        var target=snapshot.GetTextInsertionTarget(Find(snapshot,"out")).Target!;
        Assert.Equal(AuthoringInsertionFailureKind.SizeLimit,snapshot.ProposeTextInsertion(new(target,"X")).Failure!.Kind);
        var formatted=snapshot.ProposeTextInsertion(new(target,"X",AuthoringInsertionFormattingMode.Readable));
        Assert.True(formatted.IsSuccess,formatted.Failure?.Message);Verify(formatted.Candidate!);Assert.Equal("X",Run(formatted.Candidate!.ExportSource()));
    }

    /// <summary>Direct child layout changes protect comments, PIs, descendants and literal body bytes.</summary>
    /// <param name="newline">The established physical source newline.</param>
    [Theory][InlineData("\n")][InlineData("\r\n")][InlineData("\r")]
    public void ProtectedChildrenAndOutputEquivalence(string newline)
    {
        var body="<out>"+newline+"  <!-- keep -->"+newline+"  <?keep a?>"+newline+"  <xsl:text>  old&#xD;&#xA;{old} </xsl:text>"+newline+"  <nested><xsl:value-of select=\"'N'\"/></nested>"+newline+"</out>";
        var snapshot=Inspect(Sheet(body));var payload=" new\r\n{literal} ";
        var preserve=Insert(snapshot,payload);var readable=Insert(snapshot,payload,AuthoringInsertionFormattingMode.Readable);
        Assert.Equal("  old\r\n{old} N"+payload,Run(readable.ExportSource()));Assert.Equal(Run(preserve.ExportSource()),Run(readable.ExportSource()));
        Assert.Contains("<xsl:text>  old&#xD;&#xA;{old} </xsl:text>",Encoding.UTF8.GetString(readable.ExportSource()));
    }

    /// <summary>The compiler retains supported alias bindings while the new text target remains typed.</summary>
    [Fact]
    public void AliasedPrefixAndTemplateTarget()
    {
        var sheet=Sheet("<out></out>").Replace("xsl:","s:").Replace("xmlns:xsl","xmlns:s");
        var c=Insert(Inspect(sheet),"T",parent:"template");Assert.Equal("s",c.InsertedNode.ElementName!.Prefix);Assert.Equal("T",Run(c.ExportSource()));
    }

    /// <summary>Unsupported profiles and encoding declarations retain classified scope refusal.</summary>
    /// <param name="version">An excluded profile.</param>
    [Theory][InlineData("1.0")][InlineData("2.0")][InlineData("5.0")]
    public void UnsupportedVersions(string version)
    {
        var snapshot=Inspect(Sheet(version:version));Assert.Equal(AuthoringInsertionFailureKind.UnsupportedProfile,snapshot.GetTextInsertionTarget(Find(snapshot,"out")).Failure!.Kind);
    }

    /// <summary>Static evaluation, dependencies and unsupported text bodies do not gain authority.</summary>
    /// <param name="extra">An excluded declaration.</param>
    [Theory]
    [InlineData("<xsl:include href='https://invalid.example/other.xsl'/>")]
    [InlineData("<xsl:variable name='v' static='yes' select=\"doc('https://invalid.example/')\"/>")]
    [InlineData("<xsl:variable name='v' _select=\"doc('https://invalid.example/')\"/>")]
    public void NoResourceFallback(string extra)
    {
        var text=Sheet().Replace("<xsl:output",extra+"<xsl:output");Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(text),ModuleUri,out var source,out _));
        var resolver=new CountingResolver();var snapshot=XsltAuthoring.Inspect(source!,resolver,new(){AttemptCompilation=false}).Snapshot!;
        var calls=resolver.Calls;var target=snapshot.GetTextInsertionTarget(Find(snapshot,"out"));Assert.False(target.IsSuccess);Assert.Equal(calls,resolver.Calls);
    }

    /// <summary>Previously captured mutable inspection options cannot weaken candidate restrictions.</summary>
    [Fact]
    public void InspectionOptionsAreNotReused()
    {
        var options=new AuthoringInspectionOptions{AttemptCompilation=false};Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(Sheet()),ModuleUri,out var source,out _));
        var snapshot=XsltAuthoring.Inspect(source!,options:options).Snapshot!;var target=snapshot.GetTextInsertionTarget(Find(snapshot,"out")).Target!;
        options.AttemptCompilation=true;options.MaxModuleCount=0;var result=snapshot.ProposeTextInsertion(new(target,"X"));Assert.True(result.IsSuccess,result.Failure?.Message);Verify(result.Candidate!);
    }

    private sealed class CountingResolver : IAuthoringModuleResolver
    {
        internal int Calls { get; private set; }
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri){Calls++;return null;}
    }

    /// <summary>Cooperative cancellation during descriptor work returns no partial target or candidate.</summary>
    /// <param name="duringProposal">Whether cancellation occurs after target discovery.</param>
    /// <param name="cancel">Whether the armed traversal requests cancellation.</param>
    [Theory][InlineData(false,true)][InlineData(false,false)][InlineData(true,true)][InlineData(true,false)]
    public void DeterministicTraversalCancellation(bool duringProposal, bool cancel)
    {
        var original=Inspect(Sheet());var parent=Find(original,"out");using var cts=new CancellationTokenSource();
        var armed=!duringProposal;var seen=false;
        var snapshot=WithRootTraversal(original,()=>{if(armed&&!seen){seen=true;if(cancel)cts.Cancel();}});
        if(duringProposal)
        {
            var target=snapshot.GetTextInsertionTarget(parent);Assert.True(target.IsSuccess);armed=true;
            var result=snapshot.ProposeTextInsertion(new(target.Target!,"X"),cts.Token);Assert.True(seen);
            if(cancel){Assert.Null(result.Candidate);Assert.Equal(AuthoringInsertionFailureKind.Cancelled,result.Failure!.Kind);}
            else {Assert.True(result.IsSuccess,result.Failure?.Message);Verify(result.Candidate!);}
        }
        else
        {
            var target=snapshot.GetTextInsertionTarget(parent,cts.Token);Assert.True(seen);
            if(cancel){Assert.Null(target.Target);Assert.Equal(AuthoringInsertionFailureKind.Cancelled,target.Failure!.Kind);}
            else Assert.True(target.IsSuccess,target.Failure?.Message);
        }
        Assert.Equal(original.ExportOriginal(ModuleUri),snapshot.ExportOriginal(ModuleUri));
    }

    private static AuthoringSnapshot WithRootTraversal(AuthoringSnapshot snapshot, Action onTraversal)
    {
        var module = snapshot.PrincipalModule;
        var root = module.Root;
        var wrappedRoot = new AuthoringNodeDescriptor(root.Id, root.Kind, root.ElementName, root.Text,
            root.Range, root.Attributes, new TraversalList(root.Children, onTraversal), root.BackingObject);
        var wrappedModule = new AuthoringModuleDescriptor(module.ModuleUri, module.IsPrincipal,
            module.EncodingName, module.OriginalByteLength, module.Version, module.Edges,
            wrappedRoot, module.Precedence, module.NamedTypes);
        var identities = Nodes(root).Where(n => n.BackingObject is not null)
            .ToDictionary(n => n.BackingObject!, n => n.Id);
        return new AuthoringSnapshot([wrappedModule], new() { [module.ModuleUri] = wrappedModule },
            new() { [module.ModuleUri] = snapshot.SourceFor(module.ModuleUri) }, identities,
            snapshot.IsCompilable, snapshot.CompilationDiagnostics, snapshot.ModuleResolver,
            snapshot.InspectionOptions, snapshot.FunctionSignatures);
    }

    private sealed class TraversalList(IReadOnlyList<AuthoringNodeDescriptor> children, Action onTraversal)
        : IReadOnlyList<AuthoringNodeDescriptor>
    {

        /// <summary>Gets the number of unchanged child descriptors.</summary>
        public int Count => children.Count;

        /// <summary>Gets the unchanged child at the requested index.</summary>
        /// <param name="index">The zero-based child index.</param>
        /// <returns>The original child descriptor.</returns>
        public AuthoringNodeDescriptor this[int index] => children[index];

        /// <summary>Invokes the test callback once, then traverses the original children.</summary>
        /// <returns>The original child enumerator.</returns>
        public IEnumerator<AuthoringNodeDescriptor> GetEnumerator()
        {
            onTraversal();
            return children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }


}
