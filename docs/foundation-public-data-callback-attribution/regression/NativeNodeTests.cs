// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Verifies public native original-node atomization, identity and snapshot lifetime.
// SPECIAL NOTES        : Isolated provider prototype; not a production or published API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-11-2026     | Creation; isolated native container reissuance prototype.                               |
//                      | Charles Korthout | 0.2   | 06-11-2026     | Guard exposed XML snapshot against mutation through provider node projections.           |
//                      | Charles Korthout | 0.3   | 06-11-2026     | Add original element facts, private XML projection and exact node ownership.              |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.Schema;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.Schema.ContainerPrototype;

/// <summary>Qualifies native original element facts using public data evaluation and exact membership.</summary>
public sealed class NativeNodeTests
{
    private sealed class Fixture : IDisposable
    {
        internal readonly NativeContainerAdapter Endpoint;
        internal readonly SchemaQuerySession Session;
        internal readonly NativeContainerAdapter.Advisor Advisor;
        internal readonly NativeContainerAdapter.Input Input;
        internal readonly SchemaQuerySnapshot Snapshot;
        internal readonly IXdmNode Root;
        internal readonly IXdmNode[] Children;
        internal Fixture(string xml = "<order><amount>1.25</amount><amount>2.50</amount></order>")
        {
            var native = SchemaServices.CreateDocumentAssessor(XDocument.Parse("""
                <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"><xs:element name="order"><xs:complexType>
                <xs:sequence><xs:element name="amount" type="xs:decimal" minOccurs="0" maxOccurs="2"/></xs:sequence>
                <xs:assert test="count(amount) le 2"/></xs:complexType></xs:element></xs:schema>
                """));
            Endpoint = new(native);
            var compiled = SchemaQuerySession.CompileXPath("data($node)", new(4, 8, 8, 128, 128), assessor: Endpoint);
            Assert.Equal(SchemaResultKind.Supported, compiled.Kind); Session = compiled.Value;
            Advisor = Assert.IsType<NativeContainerAdapter.Advisor>(Session.Advisor);
            Input = Advisor.Capture(XDocument.Parse(xml, LoadOptions.PreserveWhitespace));
            var document = ((ISchemaAssessmentInput)Input).Root;
            Root = Nodes(document.Children(XdmNodeKind.Element)).Single();
            Children = Nodes(Root.Children(XdmNodeKind.Element));
            Snapshot = Session.OpenSnapshot(Input, Input, Input.Origin).Value;
        }
        internal XdmValue Evaluate(IXdmNode node) => Session.Run([Snapshot], op => op.Evaluate(new EvaluationContext().WithVariable("node", XdmValue.FromNode(node))));
        /// <inheritdoc/>
        public void Dispose() { Snapshot.Dispose(); Session.Dispose(); Input.Dispose(); }
    }
    private static IXdmNode[] Nodes(XdmSequence sequence)
    { var nodes = new List<IXdmNode>(); foreach (var value in sequence) nodes.Add(value.NodeValue); return nodes.ToArray(); }
    /// <summary>The public engine atomizes real native decimal facts while the legacy route is poisoned.</summary>
    [Fact] public void PublicDataUsesNativeFacts()
    {
        using var f = new Fixture(); Assert.Equal(1.25m, f.Evaluate(f.Children[0]).DecimalValue); Assert.Equal(2.50m, f.Evaluate(f.Children[1]).DecimalValue);
        Assert.Throws<NotSupportedException>(() => f.Children[0].TypedValue);
        f.Session.Run([f.Snapshot], op =>
        {
            var value = op.Evaluate(new EvaluationContext().WithVariable("node", XdmValue.FromNode(f.Children[0])));
            Assert.True(SchemaAtomics.TryGetProvenance(value, out var evidence));
            Assert.Same(f.Input.Origin, evidence!.OriginalAssessment); return 0;
        });
    }
    /// <summary>Element-only native root retains no typed value and raises the required atomization error.</summary>
    [Fact] public void RootNoTypedValueRaisesFoty0012()
    {
        using var f = new Fixture(); var facts = ((ISchemaAnnotatedNode)f.Root).OriginalSchemaFacts;
        Assert.True(facts.HasNoTypedValue); Assert.False(facts.IsNilled);
        Assert.StartsWith("FOTY0012:", Assert.Throws<InvalidOperationException>(() => f.Evaluate(f.Root)).Message);
    }
    /// <summary>An absent optional child has no projected node and does not erase root absence.</summary>
    [Fact] public void OptionalAbsenceIsNotEmptyTypedValue()
    {
        using var f = new Fixture("<order/>"); Assert.Empty(f.Children);
        Assert.True(((ISchemaAnnotatedNode)f.Root).OriginalSchemaFacts.HasNoTypedValue);
        Assert.StartsWith("FOTY0012:", Assert.Throws<InvalidOperationException>(() => f.Evaluate(f.Root)).Message);
    }
    /// <summary>Repeated instances share the real declaration and type but retain distinct node identities.</summary>
    [Fact] public void RepeatedChildDeclarationAndIdentity()
    {
        using var f = new Fixture(); var first = ((ISchemaAnnotatedNode)f.Children[0]).OriginalSchemaFacts;
        var second = ((ISchemaAnnotatedNode)f.Children[1]).OriginalSchemaFacts;
        Assert.Same(first.Type, second.Type); Assert.Same(first.Declaration, second.Declaration);
        Assert.False(f.Children[0].IsSameNode(f.Children[1]));
        Assert.Same(f.Root, f.Children[0].Parent); Assert.Same(f.Children[0], Nodes(f.Root.Children(XdmNodeKind.Element))[0]);
    }
    /// <summary>Root declaration and anonymous type are actual distinct owned component handles.</summary>
    [Fact] public void OriginalAnonymousRootAuthority()
    {
        using var f = new Fixture(); var facts = ((ISchemaAnnotatedNode)f.Root).OriginalSchemaFacts; var authority = (ISchemaAuthority)f.Endpoint;
        Assert.True(authority.Owns(facts.Type)); Assert.True(authority.Owns(facts.Declaration!));
        Assert.NotSame(facts.Type, facts.Declaration);
        Assert.Same(authority.ResolveElementDeclaration(new("", "order"), default).Value, facts.Declaration);
        Assert.Equal(SchemaResultKind.NotFound, authority.ResolveType(new("", "anonymous"), default).Kind);
    }
    /// <summary>Facts belonging to a sibling cannot authorize another same-name node.</summary>
    [Fact] public void SwappedSiblingFactsRefuse()
    {
        using var f = new Fixture(); var first = Assert.IsType<NativeContainerAdapter.ProjectedNode>(f.Children[0]);
        var second = Assert.IsType<NativeContainerAdapter.ProjectedNode>(f.Children[1]);
        first.State = second.State;
        Assert.Throws<ArgumentException>(() => f.Evaluate(first));
    }
    /// <summary>Foreign snapshot nodes refuse even when the same advisor issued both inputs.</summary>
    [Fact] public void ForeignSnapshotNodeRefuses()
    {
        using var f = new Fixture(); using var other = f.Advisor.Capture(XDocument.Parse("<order><amount>1</amount></order>"));
        var root = Nodes(((ISchemaAssessmentInput)other).Root.Children(XdmNodeKind.Element)).Single();
        Assert.Throws<ArgumentException>(() => f.Evaluate(Nodes(root.Children(XdmNodeKind.Element)).Single()));
    }
    /// <summary>Equal XML from an independent endpoint cannot supply original node facts.</summary>
    [Fact] public void IndependentEndpointNodeRefuses()
    { using var f = new Fixture(); using var other = new Fixture(); Assert.Throws<ArgumentException>(() => f.Evaluate(other.Children[0])); }
    /// <summary>Advisor ownership requires exact node/facts/input/origin association.</summary>
    [Fact] public void ExactTupleMembership()
    {
        using var f = new Fixture(); var advisor = (ISchemaAssessmentAdvisor)f.Advisor;
        var facts = ((ISchemaAnnotatedNode)f.Children[0]).OriginalSchemaFacts;
        Assert.True(advisor.OwnsNodeFacts(facts)); Assert.True(advisor.OwnsNode(f.Children[0], facts, f.Input, f.Input.Origin));
        Assert.False(advisor.OwnsNode(f.Children[1], facts, f.Input, f.Input.Origin));
        using var other = f.Advisor.Capture(XDocument.Parse("<order/>"));
        Assert.False(advisor.OwnsNode(f.Children[0], facts, other, other.Origin));
    }
    /// <summary>Revocation stops node access, original facts, navigation and resumed cursor reads.</summary>
    [Fact] public void ProviderNodeAndFactsRevoke()
    {
        using var f = new Fixture(); var facts = ((ISchemaAnnotatedNode)f.Children[0]).OriginalSchemaFacts;
        var cursor = f.Root.Children().GetEnumerator(); Assert.True(cursor.MoveNext()); f.Input.Dispose();
        Assert.Throws<ObjectDisposedException>(() => f.Children[0].StringValue);
        Assert.Throws<ObjectDisposedException>(() => f.Children[0].TypedValue);
        Assert.Throws<ObjectDisposedException>(() => f.Root.SchemaTypeAnnotation);
        Assert.Throws<ObjectDisposedException>(() => facts.Type);
        Assert.Throws<ObjectDisposedException>(() => f.Root.Children());
        Assert.Throws<ObjectDisposedException>(() => cursor.MoveNext());
    }
    /// <summary>Document and text atomization remain unsupported by the bounded public data profile.</summary>
    /// <param name="document">Select the document rather than decimal text.</param>
    [Theory][InlineData(true)][InlineData(false)] public void NonElementDataRemainsUnsupported(bool document)
    {
        using var f = new Fixture(); var node = document ? ((ISchemaAssessmentInput)f.Input).Root : Nodes(f.Children[0].Children()).Single();
        Assert.Throws<NotSupportedException>(() => f.Evaluate(node));
    }
    /// <summary>Root comments and whitespace remain navigable without fabricated schema facts.</summary>
    [Fact] public void NonElementNodesHaveNoFabricatedFacts()
    {
        using var f = new Fixture("<order> <!--note--> <amount>1</amount> </order>");
        var comment = Nodes(f.Root.Children(XdmNodeKind.Comment)).Single();
        Assert.True(((ISchemaAssessmentInput)f.Input).OwnsNode(comment));
        Assert.Throws<NotSupportedException>(() => ((ISchemaAnnotatedNode)comment).OriginalSchemaFacts);
        Assert.Equal(1m, f.Evaluate(f.Children[0]).DecimalValue);
    }
    /// <summary>Axis projection and parent/document links return canonical source-owned wrappers.</summary>
    [Fact] public void NavigationPreservesNodeMembership()
    {
        using var f = new Fixture(); var child = Nodes(f.Root.Axis(XdmAxis.Child)).First(n => n.NodeKind == XdmNodeKind.Element);
        Assert.Same(f.Children[0], child); Assert.Same(((ISchemaAssessmentInput)f.Input).Root, child.Document);
        Assert.Throws<NotSupportedException>(() => f.Root.Axis(XdmAxis.Namespace));
    }
    /// <summary>Native copies receive fresh projected node/snapshot associations and retain original component objects.</summary>
    [Fact] public void NativeCopyHasFreshNodeAssociations()
    {
        using var f = new Fixture(); using var copy = f.Advisor.Reissue(f.Input.Native.Copy());
        var root = Nodes(((ISchemaAssessmentInput)copy).Root.Children(XdmNodeKind.Element)).Single();
        var child = Nodes(root.Children(XdmNodeKind.Element))[0];
        Assert.NotSame(f.Children[0], child);
        Assert.Same(((ISchemaAnnotatedNode)f.Children[0]).OriginalSchemaFacts.Type, ((ISchemaAnnotatedNode)child).OriginalSchemaFacts.Type);
        using var snapshot = f.Session.OpenSnapshot(copy, copy, copy.Origin).Value;
        f.Session.Run([snapshot], op => { Assert.Equal(1.25m, op.Evaluate(new EvaluationContext().WithVariable("node", XdmValue.FromNode(child))).DecimalValue); return 0; });
    }
}
