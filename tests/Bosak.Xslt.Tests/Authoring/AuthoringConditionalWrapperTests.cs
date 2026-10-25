// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Bounded conditional wrapper lifecycle and complete source provenance.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>REQ-138 engine lifecycle, source provenance, context and refusal obligations.</summary>
public sealed class AuthoringConditionalWrapperTests
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static readonly Uri Module = new("file:///C:/authoring/conditional.xsl");
    private static string Sheet(string body, string version = "3.0", string extra = "") =>
        $"<s:stylesheet xmlns:s='{Xsl}' {extra} version='{version}'><s:output omit-xml-declaration='yes'/><s:template match='/'>{body}</s:template></s:stylesheet>";
    private static byte[] Encode(string text, int variant)
    {
        Encoding e = variant switch { 0 => new UTF8Encoding(false,true), 1 => new UTF8Encoding(true,true), 2 => new UnicodeEncoding(false,true,true), _ => new UnicodeEncoding(true,true,true) };
        return e.GetPreamble().Concat(e.GetBytes($"<?xml version='1.0' encoding='{(variant < 2 ? "utf-8" : variant == 2 ? "utf-16le" : "utf-16be")}'?>" + text)).ToArray();
    }
    private static AuthoringSnapshot Inspect(string text, int encoding = 0, IAuthoringModuleResolver? resolver = null)
    {
        Assert.True(AuthoringSource.TryCreate(Encode(text, encoding), Module, out var source, out var f), f?.Message);
        var result = XsltAuthoring.Inspect(source!, resolver, new() { AttemptCompilation = false });
        Assert.True(result.IsSuccess, result.Failure?.Message); return result.Snapshot!;
    }
    private static IEnumerable<AuthoringNodeDescriptor> Nodes(AuthoringNodeDescriptor n) => new[] { n }.Concat(n.Children.SelectMany(Nodes));
    private static AuthoringNodeDescriptor Find(AuthoringSnapshot s, string local) => Nodes(s.PrincipalModule.Root).First(n => n.ElementName?.LocalName == local);
    private static byte[] Slice(byte[] bytes, SourceRange r) => bytes.AsSpan((int)r.StartByteOffset,(int)r.ByteLength).ToArray();
    private static AuthoringConditionalCandidate Wrap(AuthoringSnapshot s, string condition = "true()")
    {
        var d = s.GetConditionalWrapTarget(Find(s,"item")); Assert.True(d.IsSuccess, d.Failure?.Message);
        var result = s.ProposeConditionalWrap(new(d.Target!,condition)); Assert.True(result.IsSuccess,result.Failure?.Message);
        Verify(result.Candidate!); return result.Candidate!;
    }
    private static AuthoringConditionalCandidate Unwrap(AuthoringSnapshot s)
    {
        var d = s.GetConditionalUnwrapTarget(Find(s,"if")); Assert.True(d.IsSuccess,d.Failure?.Message);
        var result = s.ProposeConditionalUnwrap(new(d.Target!)); Assert.True(result.IsSuccess,result.Failure?.Message);
        Verify(result.Candidate!); return result.Candidate!;
    }
    private static void Verify(AuthoringConditionalCandidate c)
    {
        var original = c.InputSnapshot.ExportOriginal(Module); var candidate = c.ExportSource();
        Assert.Equal(2,c.SourceEdits.Count);
        var replay = original.ToList();
        foreach (var edit in c.SourceEdits.Reverse())
        {
            Assert.Equal(Slice(original,edit.OriginalRange),c.ExportOriginalRange(edit.OriginalRange));
            replay.RemoveRange((int)edit.OriginalRange.StartByteOffset,(int)edit.OriginalRange.ByteLength);
            replay.InsertRange((int)edit.OriginalRange.StartByteOffset,Slice(candidate,edit.ReplacementRange));
        }
        Assert.Equal(candidate,replay.ToArray());
        var oldNodes = Nodes(c.InputSnapshot.PrincipalModule.Root).ToArray(); var fresh = Nodes(c.Snapshot.PrincipalModule.Root).ToArray();
        var fragments = c.MergedTextGroups.SelectMany(g => g.Fragments.Select(f => f.OriginalNode)).ToArray();
        Assert.Equal(oldNodes.Length,c.NodeCorrespondence.Count+c.RemovedNodes.Count+fragments.Length);
        Assert.Equal(oldNodes.ToHashSet(),c.NodeCorrespondence.Keys.Concat(c.RemovedNodes).Concat(fragments).ToHashSet());
        Assert.Equal(fresh.ToHashSet(),c.NodeCorrespondence.Values.Concat(c.CreatedNodes).Concat(c.MergedTextGroups.Select(g=>g.CandidateNode)).ToHashSet());
        foreach(var pair in c.NodeCorrespondence)
        { Assert.Equal(pair.Key.ElementName,pair.Value.ElementName); Assert.Equal(pair.Key.Text,pair.Value.Text); }
        Assert.Equal(oldNodes.Where(n=>n.Kind==AuthoringNodeKind.Element && !c.RemovedNodes.Contains(n)).Count(),c.AttributeCorrespondence.Count);
        foreach(var group in c.AttributeCorrespondence)
        {
            Assert.Same(c.NodeCorrespondence[group.OriginalOwner],group.CandidateOwner);
            Assert.Equal(group.OriginalOwner.Attributes.Count,group.Survivors.Count);
            foreach(var pair in group.Survivors)
            {
                Assert.Equal((pair.Key.Name,pair.Key.ExpandedValue,pair.Key.RawLiteral),(pair.Value.Name,pair.Value.ExpandedValue,pair.Value.RawLiteral));
                Assert.Equal(Slice(original,pair.Key.FullRange),Slice(candidate,pair.Value.FullRange));
            }
        }
        foreach(var g in c.MergedTextGroups)
        {
            Assert.Equal(2,g.Fragments.Count);
            var bytes = g.Fragments.SelectMany(f=>Slice(original,f.OriginalRange)).ToArray();
            Assert.Equal(Slice(candidate,g.CandidateNode.Range),bytes);
            foreach(var f in g.Fragments) Assert.Equal(Slice(original,f.OriginalRange),Slice(candidate,f.CandidateRange));
        }
        Assert.NotEqual(original,candidate); Assert.Equal(original,c.InputSnapshot.ExportOriginal(Module));
        candidate[0] ^= 1; Assert.NotEqual(candidate,c.ExportSource());
    }

    /// <summary>All encodings preserve populated retained content through Wrap/Edit/Unwrap.</summary>
    /// <param name="encoding">The strict encoding variant.</param>
    /// <param name="version">The effective stylesheet version.</param>
    [Theory][InlineData(0,"3.0")][InlineData(1,"3.0")][InlineData(2,"3.0")][InlineData(3,"3.0")]
    [InlineData(0,"4.0")][InlineData(1,"4.0")][InlineData(2,"4.0")][InlineData(3,"4.0")]
    public void CompleteLifecycle(int encoding,string version)
    {
        var s=Inspect(Sheet("<out> \r\n<item a='{{x}}' xmlns:p='urn:retained'><child>??? &amp; X</child><!--inside--><?p keep?><s:value-of select='1'/></item> \t<tail/></out>",version),encoding);
        var wrapped=Wrap(s,"true() and ('a\t\r\nb' = 'a\t\r\nb')");
        Assert.Equal("if",wrapped.CandidateSelection.ElementName!.LocalName); Assert.Same(wrapped.CandidateTest,Assert.Single(wrapped.CandidateSelection.Attributes));
        Assert.Single(wrapped.CreatedNodes); Assert.Single(wrapped.CreatedAttributes); Assert.Empty(wrapped.RemovedNodes);
        var edited=wrapped.Snapshot.ProposeExpressionEdit(new(wrapped.CandidateSelection.Id,"test","false()"),CancellationToken.None);
        Assert.True(edited.IsSuccess,edited.Failure?.Message); Assert.True(edited.Candidate!.IsCompilable);
        var unwrapped=Unwrap(edited.Candidate.Snapshot); Assert.Equal(s.ExportOriginal(Module),unwrapped.ExportSource());
        Assert.Equal("item",unwrapped.CandidateSelection.ElementName!.LocalName);
        // Existing operations remain independently restricted through the introduced if.
        var item=Find(wrapped.Snapshot,"item");
        Assert.False(wrapped.Snapshot.GetLiteralElementRenameTarget(item).IsSuccess);
        Assert.False(wrapped.Snapshot.GetLiteralElementDeletionTarget(item).IsSuccess);
        Assert.False(wrapped.Snapshot.GetLiteralElementInsertionTarget(item).IsSuccess);
        Assert.True(unwrapped.Snapshot.GetLiteralElementRenameTarget(unwrapped.CandidateSelection).IsSuccess);
    }

    /// <summary>Both whitespace boundaries merge with one CR/LF normalization across each seam.</summary>
    /// <param name="encoding">The strict encoding variant.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void TwoBoundaryCrLfSeams(int encoding)
    {
        var s=Inspect(Sheet("<out> \r<s:if test='true()'>\n<item>keep &#xD; &amp; bytes</item>\r</s:if>\n </out>"),encoding);
        var c=Unwrap(s); Assert.Equal(2,c.MergedTextGroups.Count);
        Assert.All(c.MergedTextGroups,g=>Assert.Equal(g.CandidateNode.Text!.Replace("\r",""),g.CandidateNode.Text));
        var expected=Inspect(Sheet("<out> \r\n<item>keep &#xD; &amp; bytes</item>\r\n </out>"),encoding);
        Assert.Equal(expected.ExportOriginal(Module),c.ExportSource());
        foreach(var g in c.MergedTextGroups)
        {
            var combined=g.Fragments.Select(f=>f.OriginalNode.Text).Aggregate("",(a,b)=>a+b);
            Assert.NotEqual(combined,g.CandidateNode.Text);
        }
    }

    /// <summary>Boundary shape variants produce exactly the required merge groups.</summary>
    /// <param name="outerLeft">Outside-left trivia.</param>
    /// <param name="innerLeft">Inside-left trivia.</param>
    /// <param name="innerRight">Inside-right trivia.</param>
    /// <param name="outerRight">Outside-right trivia.</param>
    /// <param name="groups">Expected merge count.</param>
    [Theory][InlineData("","","","",0)][InlineData(" "," ","","",1)][InlineData("",""," "," ",1)]
    [InlineData(" "," "," "," ",2)][InlineData(""," "," ","",0)][InlineData(" ","",""," ",0)]
    public void BoundaryShapes(string outerLeft,string innerLeft,string innerRight,string outerRight,int groups)
    {
        var s=Inspect(Sheet($"<out>{outerLeft}<s:if test='true()'>{innerLeft}<item/>{innerRight}</s:if>{outerRight}</out>"));
        var c=Unwrap(s); Assert.Equal(groups,c.MergedTextGroups.Count);
        Assert.Equal(Inspect(Sheet($"<out>{outerLeft}{innerLeft}<item/>{innerRight}{outerRight}</out>")).ExportOriginal(Module),c.ExportSource());
    }

    /// <summary>Unsupported Unwrap shapes never grant deletion or retention authority.</summary>
    /// <param name="body">The unsupported wrapper context.</param>
    [Theory][InlineData("<s:if test='true()'/>")][InlineData("<s:if test='true()'><item/><other/></s:if>")]
    [InlineData("<s:if test='true()' xmlns:q='urn:q'><item/></s:if>")][InlineData("<s:if test='true()' xml:lang='en'><item/></s:if>")]
    [InlineData("<s:if test='true()'><!--outside--><item/></s:if>")][InlineData("<s:if test='true()'>significant<item/></s:if>")]
    [InlineData("<s:if test='true()'>&#x20;<item/></s:if>")][InlineData("&#x20;<s:if test='true()'><item/></s:if>")]
    [InlineData("<s:if test='true()'><item/>&#xA;</s:if>")][InlineData("<s:if test='true()'><item/></s:if>&#xA;")]
    [InlineData("<s:if test='true()'><![CDATA[ ]]><item/></s:if>")][InlineData("<out xml:space='preserve'><s:if test='true()'><item/></s:if></out>")]
    [InlineData("<s:if test='true()'><s:if test='true()'><item/></s:if></s:if>")]
    public void RefusesUnsupportedUnwrap(string body)
    { var s=Inspect(Sheet(body));var result=s.GetConditionalUnwrapTarget(Find(s,"if"));Assert.False(result.IsSuccess);Assert.NotNull(result.Failure); }

    /// <summary>Candidate compilation uses P's context, not namespace declarations local to E.</summary>
    [Fact]
    public void ParentPrefixAndStaticContext()
    {
        var s=Inspect(Sheet("<item xmlns:a='urn:local' xmlns:s='urn:shadow'/>",extra:$"xmlns:z='{Xsl}' xmlns:b='{Xsl}'"));
        var c=Wrap(s); Assert.Equal("b",c.CandidateSelection.ElementName!.Prefix);
        var target=s.GetConditionalWrapTarget(Find(s,"item")).Target!;
        var invalid=s.ProposeConditionalWrap(new(target,"a:missing()"));Assert.False(invalid.IsSuccess);
        var noAlias=Inspect($"<stylesheet xmlns='{Xsl}' version='3.0'><template match='/'><item xmlns='' xmlns:q='{Xsl}'/></template></stylesheet>");
        Assert.Equal(AuthoringConditionalFailureKind.NoSafeXsltPrefix,noAlias.GetConditionalWrapTarget(Find(noAlias,"item")).Failure!.Kind);
    }

    /// <summary>Foreign snapshots, pre-cancellation, bounds and malformed conditions are typed refusals.</summary>
    [Fact]
    public void OwnershipCancellationAndConditionLimits()
    {
        var s=Inspect(Sheet("<item/>"));var foreign=Inspect(Sheet("<item/>"));
        Assert.Equal(AuthoringConditionalFailureKind.ForeignSnapshot,s.GetConditionalWrapTarget(Find(foreign,"item")).Failure!.Kind);
        var target=s.GetConditionalWrapTarget(Find(s,"item")).Target!;
        Assert.Equal(AuthoringConditionalFailureKind.ForeignSnapshot,foreign.ProposeConditionalWrap(new(target,"true()")).Failure!.Kind);
        using var cts=new CancellationTokenSource();cts.Cancel();
        Assert.Equal(AuthoringConditionalFailureKind.Cancelled,s.GetConditionalWrapTarget(Find(s,"item"),cts.Token).Failure!.Kind);
        Assert.Equal(AuthoringConditionalFailureKind.Cancelled,s.ProposeConditionalWrap(new(target,"true()"),cts.Token).Failure!.Kind);
        Assert.Equal(AuthoringConditionalFailureKind.SizeLimit,s.ProposeConditionalWrap(new(target,new string('x',65537))).Failure!.Kind);
        foreach(var condition in new[]{""," ","(","'\u0001'","'\ud800'","missing:call()"})
        { var result=s.ProposeConditionalWrap(new(target,condition));Assert.Null(result.Candidate);Assert.NotNull(result.Failure); }
        var w=Wrap(s);var u=w.Snapshot.GetConditionalUnwrapTarget(w.CandidateSelection).Target!;
        Assert.Equal(AuthoringConditionalFailureKind.ForeignSnapshot,s.ProposeConditionalUnwrap(new(u)).Failure!.Kind);
        Assert.Equal(AuthoringConditionalFailureKind.Cancelled,w.Snapshot.ProposeConditionalUnwrap(new(u),cts.Token).Failure!.Kind);
    }

    /// <summary>Bounded editor is additive, preserves logical whitespace, exact slot ownership and enum numbering.</summary>
    [Fact]
    public void BoundedExpressionCompatibilityAndEquality()
    {
        var s=Inspect(Sheet("<s:if test='true()'><item/></s:if>"));var node=Find(s,"if");
        var equal=s.ProposeExpressionEdit(new(node.Id,"test","true()"),CancellationToken.None);Assert.True(equal.IsSuccess,equal.Failure?.Message);
        Assert.Equal(s.ExportOriginal(Module),equal.Candidate!.EmittedSource.ToArray());
        var text="'a\t\r\nb' = 'a\t\r\nb'";var result=s.ProposeExpressionEdit(new(node.Id,"test",text),CancellationToken.None);
        Assert.True(result.IsSuccess,result.Failure?.Message);Assert.Equal(text,Find(result.Candidate!.Snapshot,"if").Attributes.Single().ExpandedValue);
        Assert.Equal(Nodes(s.PrincipalModule.Root).Count(),result.Candidate.NodeCorrespondence.Count);
        Assert.True(s.ProposeExpressionEdit(new(node.Id,"test","false()")).IsSuccess);
        Assert.Equal(0,(int)AuthoringEditFailureKind.UnknownNode);Assert.Equal(4,(int)AuthoringEditFailureKind.NotRepresentableInEncoding);
        using var cts=new CancellationTokenSource();cts.Cancel();Assert.Equal(AuthoringEditFailureKind.Cancelled,s.ProposeExpressionEdit(new(node.Id,"test","true()"),cts.Token).Failure!.Kind);
        Assert.Equal(AuthoringEditFailureKind.SizeLimit,s.ProposeExpressionEdit(new(node.Id,"test",new string('x',65537)),CancellationToken.None).Failure!.Kind);
    }

    /// <summary>Ambient resolvers are not retained as compilation authority; no condition is evaluated.</summary>
    [Fact]
    public void HostileResolverAndNonEvaluatedCondition()
    {
        var resolver=new HostileResolver(); var s=Inspect(Sheet("<item/>"),resolver:resolver);
        var c=Wrap(s,"doc('file:///C:/must-not-open.xml')/root");Assert.Equal(0,resolver.Calls);
        Unwrap(c.Snapshot);Assert.Equal(0,resolver.Calls);
        var edited=c.Snapshot.ProposeExpressionEdit(new(c.CandidateSelection.Id,"test","doc('https://invalid.test/never')/root"),CancellationToken.None);
        Assert.True(edited.IsSuccess,edited.Failure?.Message);Assert.Equal(0,resolver.Calls);
        var exported=edited.Candidate!.EmittedSource;
        Assert.True(System.Runtime.InteropServices.MemoryMarshal.TryGetArray(exported,out var buffer));buffer.Array![buffer.Offset]^=1;
        Assert.NotEqual(exported.ToArray(),edited.Candidate.EmittedSource.ToArray());
    }
    private sealed class HostileResolver : IAuthoringModuleResolver
    {
        internal int Calls;
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri,Uri referencingModuleUri)
        { Calls++;throw new InvalidOperationException("Ambient module acquisition must not occur."); }
    }

    /// <summary>Invalid originals, resource/static contexts and unsupported vocabulary never become eligible.</summary>
    /// <param name="body">Module content violating restricted validation.</param>
    [Theory][InlineData("<item><s:sequence select='('/></item>")][InlineData("<item><s:unknown/></item>")]
    [InlineData("<item a='{1}'/>")][InlineData("<item xsl:use-when='true()' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'/>")]
    [InlineData("<s:if test='true()'><item/></s:if>")][InlineData("<s:for-each select='1'><item/></s:for-each>")]
    [InlineData("<item xmlns='urn:qualified'/>")]
    public void RestrictedOriginalRefusals(string body)
    {var s=Inspect(Sheet(body));Assert.False(s.GetConditionalWrapTarget(Find(s,"item")).IsSuccess);}

    /// <summary>Deep candidates and oversized evidence are refused before publishing any candidate.</summary>
    [Fact]
    public void SourceDepthAndEvidenceBounds()
    {
        var deep=Inspect(Sheet(string.Concat(Enumerable.Repeat("<a>",253))+"<item/>"+string.Concat(Enumerable.Repeat("</a>",253))));
        var depth=deep.GetConditionalWrapTarget(Find(deep,"item"));Assert.False(depth.IsSuccess);Assert.Equal(AuthoringConditionalFailureKind.SizeLimit,depth.Failure!.Kind);
        var huge=Inspect(Sheet("<item>"+new string('x',8_388_608)+"</item>"));
        Assert.Equal(AuthoringConditionalFailureKind.SizeLimit,huge.GetConditionalWrapTarget(Find(huge,"item")).Failure!.Kind);
        var evidence=Inspect(Sheet("<item>"+new string('x',6_000_000)+"</item>"));var target=evidence.GetConditionalWrapTarget(Find(evidence,"item"));Assert.True(target.IsSuccess,target.Failure?.Message);
        var result=evidence.ProposeConditionalWrap(new(target.Target!,"true()"));Assert.Null(result.Candidate);Assert.Equal(AuthoringConditionalFailureKind.SizeLimit,result.Failure!.Kind);
    }

    /// <summary>Instrumented descriptor traversal cancels discovery/proposal/editor deterministically.</summary>
    /// <param name="operation">The bounded phase entry point.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void TraversalCancellation(int operation)
    {
        var original=Inspect(Sheet(operation>=2 ? "<s:if test='true()'> <item/> </s:if>" : "<item/>"));
        using var cts=new CancellationTokenSource();var armed=false;var seen=false;
        var m=original.PrincipalModule;var root=m.Root;
        var clonedRoot=new AuthoringNodeDescriptor(root.Id,root.Kind,root.ElementName,root.Text,root.Range,root.Attributes,
            new CallbackChildren(root.Children,()=>{if(armed){seen=true;cts.Cancel();}}),root.BackingObject);
        var module=new AuthoringModuleDescriptor(m.ModuleUri,m.IsPrincipal,m.EncodingName,m.OriginalByteLength,m.Version,m.Edges,clonedRoot,m.Precedence,m.NamedTypes);
        var ids=Nodes(root).Where(n=>n.BackingObject is not null).ToDictionary(n=>n.BackingObject!,n=>n.Id);
        var s=new AuthoringSnapshot([module],new(){[m.ModuleUri]=module},new(){[m.ModuleUri]=original.SourceFor(m.ModuleUri)},ids,
            original.IsCompilable,original.CompilationDiagnostics,original.ModuleResolver,original.InspectionOptions,original.FunctionSignatures);
        if(operation==0){armed=true;Assert.Equal(AuthoringConditionalFailureKind.Cancelled,s.GetConditionalWrapTarget(Find(original,"item"),cts.Token).Failure!.Kind);}
        if(operation==1){var t=s.GetConditionalWrapTarget(Find(original,"item")).Target!;armed=true;Assert.Equal(AuthoringConditionalFailureKind.Cancelled,s.ProposeConditionalWrap(new(t,"true()"),cts.Token).Failure!.Kind);}
        if(operation==2){var t=s.GetConditionalUnwrapTarget(Find(original,"if")).Target!;armed=true;Assert.Equal(AuthoringConditionalFailureKind.Cancelled,s.ProposeConditionalUnwrap(new(t),cts.Token).Failure!.Kind);}
        if(operation==3){armed=true;Assert.Equal(AuthoringEditFailureKind.Cancelled,s.ProposeExpressionEdit(new(Find(original,"if").Id,"test","false()"),cts.Token).Failure!.Kind);}
        Assert.True(seen);Assert.Equal(original.ExportOriginal(Module),s.ExportOriginal(Module));
    }
    private sealed class CallbackChildren(IReadOnlyList<AuthoringNodeDescriptor> children,Action callback) : IReadOnlyList<AuthoringNodeDescriptor>
    {
        /// <inheritdoc />
        public int Count=>children.Count;
        /// <inheritdoc />
        public AuthoringNodeDescriptor this[int index]=>children[index];
        /// <inheritdoc />
        public IEnumerator<AuthoringNodeDescriptor> GetEnumerator(){callback();return children.GetEnumerator();}
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
    }

    /// <summary>Inconsistent old metadata cannot be hidden by candidate reinspection.</summary>
    /// <param name="attribute">Whether to corrupt an old attribute or comment descriptor.</param>
    /// <param name="operation">Wrap, Unwrap or bounded expression editing.</param>
    [Theory][InlineData(false,0)][InlineData(true,0)][InlineData(false,1)][InlineData(true,1)][InlineData(false,2)][InlineData(true,2)]
    public void ForgedOriginalMetadataRefuses(bool attribute,int operation)
    {
        var original=Inspect(Sheet(operation==0 ? "<item a='old'><!--keep--></item>" : "<s:if test='true()'><item><!--keep--></item></s:if>"));
        AuthoringNodeDescriptor Clone(AuthoringNodeDescriptor n)=>new(n.Id,n.Kind,n.ElementName,!attribute&&n.Kind==AuthoringNodeKind.Comment ? "forged" : n.Text,n.Range,
            n.Attributes.Select(a=>attribute&&n.ElementName?.LocalName==(operation==0 ? "item" : "if") ? new AuthoringAttributeDescriptor(a.Name,a.Prefix,a.NamespaceUri,a.LocalName,a.SlotKind,a.RawLiteral,"forged",a.ValueRange,a.FullRange,a.SlotContext):a).ToArray(),n.Children.Select(Clone).ToArray(),n.BackingObject);
        var m=original.PrincipalModule;var root=Clone(m.Root);var module=new AuthoringModuleDescriptor(m.ModuleUri,m.IsPrincipal,m.EncodingName,m.OriginalByteLength,m.Version,m.Edges,root,m.Precedence,m.NamedTypes);
        var ids=Nodes(root).Where(n=>n.BackingObject is not null).ToDictionary(n=>n.BackingObject!,n=>n.Id);
        var s=new AuthoringSnapshot([module],new(){[m.ModuleUri]=module},new(){[m.ModuleUri]=original.SourceFor(m.ModuleUri)},ids,
            original.IsCompilable,original.CompilationDiagnostics,original.ModuleResolver,original.InspectionOptions,original.FunctionSignatures);
        if(operation==0) Assert.Equal(AuthoringConditionalFailureKind.SourceAlignmentFailure,s.GetConditionalWrapTarget(Find(s,"item")).Failure!.Kind);
        if(operation==1) Assert.Equal(AuthoringConditionalFailureKind.SourceAlignmentFailure,s.GetConditionalUnwrapTarget(Find(s,"if")).Failure!.Kind);
        if(operation==2) { var result=s.ProposeExpressionEdit(new(Find(s,"if").Id,"test","false()"),CancellationToken.None);Assert.Null(result.Candidate);Assert.NotNull(result.Failure); }
    }

    /// <summary>Conditions gate execution; Unwrap preserves content and makes it unconditional.</summary>
    [Fact]
    public void ActualExecutionLifecycle()
    {
        string Run(byte[] bytes)
        {
            using var stream=new MemoryStream(bytes);
            return new Bosak.Xslt.Api.XsltCompiler().Compile(System.Xml.Linq.XDocument.Load(stream),Module.AbsoluteUri)
                .TransformToString(new Bosak.XPath.Providers.Xml.XDocumentNode(System.Xml.Linq.XDocument.Parse("<input/>")));
        }
        var s=Inspect(Sheet("<out><item>retained</item><tail/></out>"));var wrapped=Wrap(s,"false()");
        Assert.Contains("<item>retained</item>",Run(s.ExportOriginal(Module)));Assert.DoesNotContain("<item",Run(wrapped.ExportSource()));
        var edited=wrapped.Snapshot.ProposeExpressionEdit(new(wrapped.CandidateSelection.Id,"test","true()"),CancellationToken.None);
        Assert.True(edited.IsSuccess,edited.Failure?.Message);Assert.Contains("<item>retained</item>",Run(edited.Candidate!.EmittedSource.ToArray()));
        var removed=Unwrap(wrapped.Snapshot);Assert.Equal(Run(s.ExportOriginal(Module)),Run(removed.ExportSource()));
    }

    /// <summary>Independent node and attribute budgets refuse large flat graphs before compilation.</summary>
    /// <param name="attributes">Whether to exceed attribute count or node count.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void FlatGraphLimits(bool attributes)
    {
        var body=attributes ? "<item "+string.Join(" ",Enumerable.Range(0,65_537).Select(i=>$"a{i}='x'"))+"/>"
            : "<item/>"+string.Concat(Enumerable.Repeat("<other/>",65_537));
        var s=Inspect(Sheet(body));var result=s.GetConditionalWrapTarget(Find(s,"item"));
        Assert.Null(result.Target);Assert.Equal(AuthoringConditionalFailureKind.SizeLimit,result.Failure!.Kind);
    }

    /// <summary>Wrap pre-counts its new descriptor before candidate allocation or recursive inspection.</summary>
    [Fact]
    public void CandidateGraphGrowthAtExactOriginalLimit()
    {
        var s=Inspect(Sheet("<item/>"+string.Concat(Enumerable.Repeat("<other/>",65_531))));
        Assert.Equal(65_536,Nodes(s.PrincipalModule.Root).Count());
        var result=s.GetConditionalWrapTarget(Find(s,"item"));Assert.Null(result.Target);
        Assert.Equal(AuthoringConditionalFailureKind.SizeLimit,result.Failure!.Kind);
    }
}
