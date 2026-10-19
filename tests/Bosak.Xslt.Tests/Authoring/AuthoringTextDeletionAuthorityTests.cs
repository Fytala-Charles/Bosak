// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 October 2026
// PURPOSE              : Verify literal text deletion cancellation and rejection of inconsistent retained evidence.
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

/// <summary>Instrumented source-only authority and cancellation controls for literal text deletion.</summary>
public class AuthoringTextDeletionAuthorityTests
{
    private const string Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static readonly Uri ModuleUri = new("file:///C:/authoring/element.xsl");
    private static string Sheet(string body = "<out><xsl:text>A</xsl:text></out>", string extra = "", string version = "3.0") =>
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
    /// <summary>Cooperative cancellation during descriptor work returns no partial target or candidate.</summary>
    /// <param name="duringProposal">Whether cancellation occurs after target discovery.</param>
    /// <param name="cancel">Whether the armed traversal requests cancellation.</param>
    [Theory][InlineData(false,true)][InlineData(false,false)][InlineData(true,true)][InlineData(true,false)]
    public void DeterministicTraversalCancellation(bool duringProposal, bool cancel)
    {
        var original=Inspect(Sheet());var parent=Find(original,"text");using var cts=new CancellationTokenSource();
        var armed=!duringProposal;var seen=false;
        var snapshot=WithRootTraversal(original,()=>{if(armed&&!seen){seen=true;if(cancel)cts.Cancel();}});
        if(duringProposal)
        {
            var target=snapshot.GetTextDeletionTarget(parent);Assert.True(target.IsSuccess);armed=true;
            var result=snapshot.ProposeTextDeletion(new(target.Target!),cts.Token);Assert.True(seen);
            if(cancel){Assert.Null(result.Candidate);Assert.Equal(AuthoringTextDeletionFailureKind.Cancelled,result.Failure!.Kind);}
            else {Assert.True(result.IsSuccess);Assert.Equal("A",result.Candidate!.OriginalTextValue);}
        }
        else
        {
            var target=snapshot.GetTextDeletionTarget(parent,cts.Token);Assert.True(seen);
            if(cancel){Assert.Null(target.Target);Assert.Equal(AuthoringTextDeletionFailureKind.Cancelled,target.Failure!.Kind);}
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


    /// <summary>Source-only instrumented metadata mismatch cannot produce incomplete correspondence.</summary>
    /// <param name="attribute">Whether an old attribute value rather than comment text is inconsistent.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void InconsistentOldMetadataRefuses(bool attribute)
    {
        var original=Inspect(Sheet("<out a='old'><!-- keep --><xsl:text>A</xsl:text></out>"));
        AuthoringNodeDescriptor Clone(AuthoringNodeDescriptor n)
        {
            var attrs=n.Attributes.Select(a=>attribute && n.ElementName?.LocalName=="out"
                ? new AuthoringAttributeDescriptor(a.Name,a.Prefix,a.NamespaceUri,a.LocalName,a.SlotKind,a.RawLiteral,"tampered",a.ValueRange,a.FullRange,a.SlotContext)
                : a).ToArray();
            return new(n.Id,n.Kind,n.ElementName,!attribute && n.Kind==AuthoringNodeKind.Comment ? "tampered" : n.Text,
                n.Range,attrs,n.Children.Select(Clone).ToArray(),n.BackingObject);
        }
        var m=original.PrincipalModule;var root=Clone(m.Root);
        var module=new AuthoringModuleDescriptor(m.ModuleUri,m.IsPrincipal,m.EncodingName,m.OriginalByteLength,m.Version,m.Edges,root,m.Precedence,m.NamedTypes);
        var ids=Nodes(root).Where(n=>n.BackingObject is not null).ToDictionary(n=>n.BackingObject!,n=>n.Id);
        var s=new AuthoringSnapshot([module],new(){[m.ModuleUri]=module},new(){[m.ModuleUri]=original.SourceFor(m.ModuleUri)},ids,
            original.IsCompilable,original.CompilationDiagnostics,original.ModuleResolver,original.InspectionOptions,original.FunctionSignatures);
        var t=s.GetTextDeletionTarget(Find(s,"text"));Assert.True(t.IsSuccess,t.Failure?.Message);
        var result=s.ProposeTextDeletion(new(t.Target!));Assert.Null(result.Candidate);
        Assert.Equal(AuthoringTextDeletionFailureKind.UnsupportedCorrespondence,result.Failure!.Kind);
    }

    /// <summary>Untrusted source-only graph corruption cannot publish incomplete replacement evidence.</summary>
    /// <param name="mode">The deliberately inconsistent descriptor evidence.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)]
    public void MalformedGraphEvidence(int mode)
    {
        var original=Inspect(Sheet("<out a='old'><!--one--><xsl:text>A</xsl:text><!--two--></out>"));
        var comments=Nodes(original.PrincipalModule.Root).Where(n=>n.Kind==AuthoringNodeKind.Comment).ToArray();
        AuthoringNodeDescriptor Clone(AuthoringNodeDescriptor n)
        {
            var id=mode==0 && ReferenceEquals(n,comments[1]) ? comments[0].Id : n.Id;
            var range=mode==3 && n.Kind==AuthoringNodeKind.Text ? n.Range with { ByteLength=n.Range.ByteLength+1 } : n.Range;
            var kind=mode==4 && n.Kind==AuthoringNodeKind.Text ? AuthoringNodeKind.Comment : n.Kind;
            var attrs=n.Attributes.Select(a=>mode==2 && n.ElementName?.LocalName=="out"
                ?new AuthoringAttributeDescriptor(a.Name,a.Prefix,a.NamespaceUri,a.LocalName,a.SlotKind,"tampered",a.ExpandedValue,a.ValueRange,a.FullRange,a.SlotContext):a).ToArray();
            var children=n.Children.Select(Clone).ToArray();
            if(mode==1 && n.ElementName?.LocalName=="out") Array.Reverse(children);
            return new(id,kind,n.ElementName,n.Text,range,attrs,children,n.BackingObject);
        }
        var m=original.PrincipalModule;var root=Clone(m.Root);
        var module=new AuthoringModuleDescriptor(m.ModuleUri,m.IsPrincipal,m.EncodingName,m.OriginalByteLength,m.Version,m.Edges,root,m.Precedence,m.NamedTypes);
        var identities=Nodes(root).Where(n=>n.BackingObject is not null).ToDictionary(n=>n.BackingObject!,n=>n.Id);
        var snapshot=new AuthoringSnapshot([module],new(){[m.ModuleUri]=module},new(){[m.ModuleUri]=original.SourceFor(m.ModuleUri)},identities,
            original.IsCompilable,original.CompilationDiagnostics,original.ModuleResolver,original.InspectionOptions,original.FunctionSignatures);
        var discovery=snapshot.GetTextDeletionTarget(Find(snapshot,"text"));
        if(mode>=3) {Assert.Null(discovery.Target);Assert.Equal(AuthoringTextDeletionFailureKind.SourceAlignmentFailure,discovery.Failure!.Kind);}
        else
        {
            Assert.True(discovery.IsSuccess, discovery.Failure?.Message);
            var result=snapshot.ProposeTextDeletion(new(discovery.Target!));
            Assert.Null(result.Candidate);
            Assert.Equal(AuthoringTextDeletionFailureKind.UnsupportedCorrespondence,result.Failure!.Kind);
        }
        Assert.Equal(original.ExportOriginal(ModuleUri),snapshot.ExportOriginal(ModuleUri));
    }

    /// <summary>Cancellation at each observed traversal phase returns no partial proposal.</summary>
    [Fact]
    public void CancellationAcrossTraversalPhases()
    {
        var original = Inspect(Sheet("<out>\r<xsl:text>A</xsl:text>\n</out>"));
        var instruction = Find(original, "text");
        var count = 0;
        var observed = WithRootTraversal(original, () => count++);
        var target = observed.GetTextDeletionTarget(instruction).Target!;
        count = 0;
        Assert.True(observed.ProposeTextDeletion(new(target)).IsSuccess);
        var phases = count;
        Assert.True(phases > 2);
        for (var phase = 1; phase <= phases; phase++)
        {
            using var cts = new CancellationTokenSource();
            var armed = false;
            var seen = 0;
            var snapshot = WithRootTraversal(original, () => { if (armed && ++seen == phase) cts.Cancel(); });
            var authority = snapshot.GetTextDeletionTarget(instruction).Target!;
            armed = true;
            var result = snapshot.ProposeTextDeletion(new(authority), cts.Token);
            Assert.True(cts.IsCancellationRequested);
            Assert.Null(result.Candidate);
            Assert.Equal(AuthoringTextDeletionFailureKind.Cancelled, result.Failure!.Kind);
            Assert.Equal(original.ExportOriginal(ModuleUri), snapshot.ExportOriginal(ModuleUri));
        }
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
        var input = Inspect(Sheet("<out>\n  <xsl:text>A</xsl:text>\n</out>"));
        var bad = Inspect(Sheet(changedBody));
        var target = input.GetTextDeletionTarget(Find(input, "text")).Target!;
        var proof = new TextDeletionPipeline.Proof(input.SourceFor(ModuleUri), bad.SourceFor(ModuleUri), target, default);
        Assert.False(proof.Match(input.PrincipalModule.Root, bad.PrincipalModule.Root));
    }

}
