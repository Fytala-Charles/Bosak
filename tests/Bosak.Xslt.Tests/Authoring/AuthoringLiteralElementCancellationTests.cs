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

/// <summary>Instrumented source-only cancellation controls for literal element insertion.</summary>
public class AuthoringLiteralElementCancellationTests
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
            var target=snapshot.GetLiteralElementInsertionTarget(parent);Assert.True(target.IsSuccess);armed=true;
            var result=snapshot.ProposeLiteralElementInsertion(new(target.Target!,"item"),cts.Token);Assert.True(seen);
            if(cancel){Assert.Null(result.Candidate);Assert.Equal(AuthoringInsertionFailureKind.Cancelled,result.Failure!.Kind);}
            else {Assert.True(result.IsSuccess,result.Failure?.Message);Assert.Empty(result.Candidate!.InsertedNode.Children);}
        }
        else
        {
            var target=snapshot.GetLiteralElementInsertionTarget(parent,cts.Token);Assert.True(seen);
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


    /// <summary>Source-only instrumented metadata mismatch cannot produce incomplete correspondence.</summary>
    /// <param name="attribute">Whether an old attribute value rather than comment text is inconsistent.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void InconsistentOldMetadataRefuses(bool attribute)
    {
        var original=Inspect(Sheet("<out a='old'><!-- keep --></out>"));
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
        var t=s.GetLiteralElementInsertionTarget(Find(s,"out"));Assert.True(t.IsSuccess,t.Failure?.Message);
        var result=s.ProposeLiteralElementInsertion(new(t.Target!,"item"));Assert.Null(result.Candidate);
        Assert.Equal(AuthoringInsertionFailureKind.SourceAlignmentFailure,result.Failure!.Kind);
    }

}
