// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 October 2026
// PURPOSE              : Verify physical bounds, cancellation and malformed attribute correspondence.
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
// ===========================================================================================================================================================

using System.Text;
using System.Collections;
using Bosak.Xslt.Authoring;
using Xunit;

namespace Bosak.Xslt.Tests.Authoring;

/// <summary>Physical boundary, cancellation and source-only proof controls for REQ-135.</summary>
public partial class AuthoringLiteralAttributeLifecycleTests
{
    private static byte[] Sized(string body, int variant, int wanted)
    {
        var s = Sheet("<!---->" + body); var baseline = Encode(s, variant); var unit = variant < 2 ? 1 : 2;
        Assert.Equal(0, (wanted - baseline.Length) % unit);
        return Encode(s.Replace("<!---->", "<!--" + new string('p', (wanted - baseline.Length) / unit) + "-->"), variant);
    }
    /// <summary>Original/final bounds, escaping growth and NoChange are checked independently.</summary>
    /// <param name="variant">Physical encoding variant.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void PhysicalBounds(int variant)
    {
        const int limit = 8_388_608; var unit = variant < 2 ? 1 : 2;
        // UTF-8 BOM and declaration lengths can have either parity; UTF-16 is always even.
        var exact = Inspect(Sized("<out status='&#39;'/>", variant, limit));
        var ct = exact.GetLiteralAttributeCreationTarget(Owner(exact)); Assert.True(ct.IsSuccess, ct.Failure?.Message);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.SizeLimit, exact.ProposeLiteralAttributeCreation(new(ct.Target!, "new", "v")).Failure!.Kind);
        var ut = exact.GetLiteralAttributeUpdateTarget(Owner(exact), Attribute(exact)).Target!;
        Assert.Equal(AuthoringLiteralAttributeUpdateOutcome.NoChange, exact.ProposeLiteralAttributeUpdate(new(ut, "'")).Outcome);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.SizeLimit, exact.ProposeLiteralAttributeUpdate(new(ut, "&&&&")).Failure!.Kind);
        var shrunk = Update(exact, ""); Assert.True(shrunk.ExportSource().Length < limit);
        var removed = Delete(exact); Assert.True(removed.ExportSource().Length < limit);
        var over = Inspect(Sized("<out status='ok'/>", variant, limit + unit));
        Assert.Equal(AuthoringLiteralAttributeFailureKind.SizeLimit, over.GetLiteralAttributeCreationTarget(Owner(over)).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.SizeLimit, over.GetLiteralAttributeUpdateTarget(Owner(over), Attribute(over)).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.SizeLimit, over.GetLiteralAttributeDeletionTarget(Owner(over), Attribute(over)).Failure!.Kind);
        var name = "status"; var insertionLength = Encoding.UTF8.GetByteCount(" " + name + "=\"v\"") * unit;
        var fits = Inspect(Sized("<out/>", variant, limit - insertionLength));
        Assert.Equal(limit, Create(fits, "v").ExportSource().Length);
        var updateFits = Inspect(Sized("<out status=''/>", variant, limit - 6 * unit));
        Assert.Equal(limit, Update(updateFits, "'").ExportSource().Length);
    }

    /// <summary>Unicode names, exact case and XMLNS remain ordinary distinct attributes.</summary>
    /// <param name="name">Valid no-namespace name.</param>
    [Theory][InlineData("État")][InlineData("状態")][InlineData("XMLNS")][InlineData("Status")][InlineData("e\u0301")]
    public void UnicodeNames(string name)
    { var c = Create(Inspect(Sheet("<out status='old'/>")), "value", name); Assert.Equal("value", (string?)Run(c.ExportSource()).Attribute(name)); }

    /// <summary>Both source spellings of the XSLT namespace and effective versions work.</summary>
    /// <param name="defaultNamespace">Whether XSLT uses a default namespace.</param>
    /// <param name="version">Supported effective version.</param>
    [Theory][InlineData(false,"3.0")][InlineData(false,"4.0")][InlineData(true,"3.0")][InlineData(true,"4.0")]
    public void NamespaceAliases(bool defaultNamespace, string version)
    {
        var text = defaultNamespace ? $"<stylesheet xmlns='{Xsl}' version='{version}'><template match='/'><out xmlns='' keep='K'/></template></stylesheet>"
            : Sheet("<out/>", version:version).Replace("xsl:", "t:").Replace("xmlns:xsl", "xmlns:t");
        var c = Create(Inspect(text), "{literal}"); Assert.Equal("{literal}", (string?)Run(c.ExportSource()).Attribute("status"));
    }

    /// <summary>Unsupported inspection encodings do not become edit-capable.</summary>
    [Fact]
    public void UnsupportedEncoding()
    {
        var bytes = Encoding.Latin1.GetBytes("<?xml version='1.0' encoding='iso-8859-1'?>" + Sheet("<out status='ok'/>"));
        var s = Inspect(bytes); Assert.Equal(AuthoringLiteralAttributeFailureKind.UnsupportedEncoding, s.GetLiteralAttributeCreationTarget(Owner(s)).Failure!.Kind);
    }

    private static AuthoringSnapshot Wrap(AuthoringSnapshot s, Func<AuthoringNodeDescriptor, AuthoringNodeDescriptor> replace)
    {
        var module = s.PrincipalModule; var root = replace(module.Root);
        var fresh = new AuthoringModuleDescriptor(module.ModuleUri, module.IsPrincipal, module.EncodingName, module.OriginalByteLength, module.Version, module.Edges, root, module.Precedence, module.NamedTypes);
        var ids = Nodes(root).Where(n => n.BackingObject is not null).ToDictionary(n => n.BackingObject!, n => n.Id);
        return new([fresh], new() { [module.ModuleUri] = fresh }, new() { [module.ModuleUri] = s.SourceFor(module.ModuleUri) }, ids,
            s.IsCompilable, s.CompilationDiagnostics, s.ModuleResolver, s.InspectionOptions, s.FunctionSignatures);
    }
    private sealed class Traversal(IReadOnlyList<AuthoringNodeDescriptor> nodes, Action visit) : IReadOnlyList<AuthoringNodeDescriptor>
    {
        /// <inheritdoc />
        public int Count { get { visit(); return nodes.Count; } }
        /// <inheritdoc />
        public AuthoringNodeDescriptor this[int index] { get { visit(); return nodes[index]; } }
        /// <inheritdoc />
        public IEnumerator<AuthoringNodeDescriptor> GetEnumerator() { foreach (var n in nodes) { visit(); yield return n; } }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Cancellation during actual descriptor traversal refuses without partial authority.</summary>
    /// <param name="proposal">Whether discovery has already completed.</param>
    /// <param name="operation">Operation or equal-Update branch.</param>
    [Theory][InlineData(false,0)][InlineData(false,1)][InlineData(false,2)][InlineData(true,0)][InlineData(true,1)][InlineData(true,2)][InlineData(true,3)]
    public void TraversalCancellation(bool proposal, int operation)
    {
        var baseline = Inspect(Sheet("<out status='ok'/>")); using var cts = new CancellationTokenSource(); var armed = false; var seen = false;
        var s = Wrap(baseline, n => new(n.Id, n.Kind, n.ElementName, n.Text, n.Range, n.Attributes, new Traversal(n.Children, () => { if (armed) { seen = true; cts.Cancel(); } }), n.BackingObject));
        var o = Owner(s); var attr = Attribute(s);
        if (proposal)
        {
            var c = s.GetLiteralAttributeCreationTarget(o).Target!; var u = s.GetLiteralAttributeUpdateTarget(o, attr).Target!; var d = s.GetLiteralAttributeDeletionTarget(o, attr).Target!;
            armed = true;
            var f = operation switch { 0 => s.ProposeLiteralAttributeCreation(new(c,"new","v"),cts.Token).Failure, 1 => s.ProposeLiteralAttributeUpdate(new(u,"new"),cts.Token).Failure, 2 => s.ProposeLiteralAttributeDeletion(new(d),cts.Token).Failure, _ => s.ProposeLiteralAttributeUpdate(new(u,"ok"),cts.Token).Failure };
            Assert.Equal(AuthoringLiteralAttributeFailureKind.Cancelled, f!.Kind);
        }
        else
        {
            armed = true;
            var f = operation switch { 0 => s.GetLiteralAttributeCreationTarget(o,cts.Token).Failure, 1 => s.GetLiteralAttributeUpdateTarget(o,attr,cts.Token).Failure, _ => s.GetLiteralAttributeDeletionTarget(o,attr,cts.Token).Failure };
            Assert.Equal(AuthoringLiteralAttributeFailureKind.Cancelled, f!.Kind);
        }
        Assert.True(seen);
    }

    /// <summary>Malformed actual candidate attributes/graph cannot pass engine correspondence proof.</summary>
    /// <param name="fault">Metadata, ordering, range or graph corruption.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)][InlineData(10)][InlineData(11)]
    public void MalformedProof(int fault)
    {
        var s = Inspect(Sheet("<out status='old' keep='K'><child a='B'/>text</out>")); var o = Owner(s); var a = Attribute(s);
        var target = s.GetLiteralAttributeUpdateTarget(o,a).Target!;
        var candidate = Update(s,"new");
        AuthoringNodeDescriptor Clone(AuthoringNodeDescriptor n)
        {
            var attrs = n.Attributes.ToArray(); var children = n.Children.Select(Clone).ToArray(); var range = n.Range; var text = n.Text;
            if (n.ElementName?.LocalName == "out")
            {
                if (fault == 0) attrs = attrs[..1];
                if (fault == 1) attrs = attrs.Concat(new[] { attrs[1] }).ToArray();
                if (fault == 2) Array.Reverse(attrs);
                if (fault == 3) attrs[1] = a;
                if (fault == 4) { var x = attrs[1]; attrs[1] = new(x.Name,x.Prefix,x.NamespaceUri,x.LocalName,x.SlotKind,"bad",x.ExpandedValue,x.ValueRange,x.FullRange,x.SlotContext); }
                if (fault == 5) range = range with { ByteLength = range.ByteLength + 1 };
                if (fault == 6) children = children.Reverse().ToArray();
                if (fault >= 8)
                {
                    var x = attrs[0];
                    attrs[0] = new(x.Name,x.Prefix,x.NamespaceUri,x.LocalName,fault == 10 ? AuthoringAttributeSlotKind.Expression : x.SlotKind,
                        fault == 8 ? "wrong" : x.RawLiteral,fault == 9 ? "{{new}}" : x.ExpandedValue,x.ValueRange,x.FullRange,x.SlotContext);
                }
            }
            if (fault == 7 && n.Kind == AuthoringNodeKind.Text) text = "different";
            return new(fault == 11 && n.Kind == AuthoringNodeKind.Text ? candidate.CandidateOwner.Id : n.Id,n.Kind,n.ElementName,text,range,attrs,children,n.BackingObject);
        }
        var malformed = Wrap(candidate.Snapshot, Clone); var source = s.SourceFor(Module);
        var start = source.CoordinateMap.CharOffsetFromLineColumn(a.ValueRange.StartLine,a.ValueRange.StartColumn);
        var end = source.CoordinateMap.CharOffsetFromLineColumn(a.ValueRange.EndLine,a.ValueRange.EndColumn);
        Assert.Null(LiteralAttributeLifecyclePipeline.Prove(s,malformed,target.Data,AuthoringLiteralAttributeOperation.Update,"status","new",start,end,3,CancellationToken.None));
    }

    /// <summary>Returned evidence collections have no mutable backing exposure.</summary>
    [Fact]
    public void ImmutableEvidence()
    {
        var c = Create(Inspect(Sheet("<out keep='K'/>")), "literal");
        var map = Assert.IsAssignableFrom<IDictionary<AuthoringNodeDescriptor, AuthoringNodeDescriptor>>(c.NodeCorrespondence);
        Assert.Throws<NotSupportedException>(() => map.Clear());
        var attrs = Assert.IsAssignableFrom<IList<AuthoringLiteralAttributeCorrespondence>>(c.AttributeCorrespondence);
        Assert.Throws<NotSupportedException>(() => attrs.Clear());
        var edits = Assert.IsAssignableFrom<IList<AuthoringInsertionSourceEdit>>(c.SourceEdits);
        Assert.Throws<NotSupportedException>(() => edits.Clear());
        var owner = c.AttributeCorrespondence.Single(x => ReferenceEquals(x.OriginalOwner, c.OriginalOwner));
        var survivors = Assert.IsAssignableFrom<IDictionary<AuthoringAttributeDescriptor, AuthoringAttributeDescriptor>>(owner.Survivors);
        Assert.Throws<NotSupportedException>(() => survivors.Clear());
    }

    private sealed class CountingResolver : IAuthoringModuleResolver
    {
        internal int Calls;
        /// <inheritdoc />
        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Calls++;
            Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes($"<xsl:stylesheet xmlns:xsl='{Xsl}' version='3.0'/>"), absoluteUri, out var source, out _));
            return source;
        }
    }
    /// <summary>Existing inspection resolver authority is never reused by lifecycle proposals.</summary>
    [Fact]
    public void DependencyDeniedBeforeAcquisition()
    {
        var xml = Sheet("<out status='old'/>").Replace("<xsl:output", "<xsl:include href='dependency.xsl'/><xsl:output");
        Assert.True(AuthoringSource.TryCreate(Encoding.UTF8.GetBytes(xml), Module, out var source, out _));
        var resolver = new CountingResolver(); var inspected = XsltAuthoring.Inspect(source!, resolver, new() { AttemptCompilation=false });
        Assert.True(inspected.IsSuccess); Assert.True(resolver.Calls > 0); resolver.Calls=0;
        var s=inspected.Snapshot!; var o=Owner(s); var a=Attribute(s);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.UnsupportedModule,s.GetLiteralAttributeCreationTarget(o).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.UnsupportedModule,s.GetLiteralAttributeUpdateTarget(o,a).Failure!.Kind);
        Assert.Equal(AuthoringLiteralAttributeFailureKind.UnsupportedModule,s.GetLiteralAttributeDeletionTarget(o,a).Failure!.Kind);
        Assert.Equal(0,resolver.Calls);
    }
}
