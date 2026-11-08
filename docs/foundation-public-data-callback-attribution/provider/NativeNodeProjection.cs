// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Projects native original element state with exact guarded node and facts membership.
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
//                      | Charles Korthout | 0.4   | 06-11-2026     | Use retained owner fields without duplicate primary-constructor capture.                  |
//                      | Charles Korthout | 0.5   | 07-11-2026     | Count node metadata reads without altering facts or callback guards.                      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;

namespace Bosak.Schema.ContainerPrototype;

/// <summary>Projects privately retained native element state into authenticated public node facts.</summary>
internal sealed partial class NativeContainerAdapter
{
    internal sealed partial class Input
    {
        private readonly Dictionary<XObject, ProjectedNode> _projected = new(ReferenceEqualityComparer.Instance);
        internal ProjectedNode Wrap(XDocumentNode node)
        {
            Check(default);
            if (!_projected.TryGetValue(node.UnderlyingObject, out var result))
            {
                result = new ProjectedNode(this, node);
                _projected.Add(node.UnderlyingObject, result); _nodes.Add(result);
            }
            return result;
        }
        internal XdmSequence Project(XdmSequence source)
        {
            Check(default); var values = new List<XdmValue>();
            foreach (var item in source)
            {
                Check(default);
                if (item.NodeValue is not XDocumentNode node) throw new NotSupportedException("Unsupported provider projection.");
                values.Add(XdmValue.FromNode(Wrap(node)));
            }
            return XdmSequence.FromSource(new NodeSequence(this, values.ToArray()));
        }
    }
    internal sealed class Facts(Input input, ProjectedNode node, AssessedElement native, ISchemaTypedValue? value) : ISchemaNodeFacts
    {
        internal readonly Input Input = input;
        internal readonly ProjectedNode Node = node;
        ISchemaComponentHandle ISchemaNodeFacts.Type { get { ProviderDiagnostics.Hit(12); Input.Check(default); return Input.Owner.Type(native); } }
        ISchemaComponentHandle ISchemaNodeFacts.Declaration { get { ProviderDiagnostics.Hit(13); Input.Check(default); return Input.Owner.Declaration(native); } }
        ISchemaAssessmentOrigin ISchemaNodeFacts.Origin { get { ProviderDiagnostics.Hit(14); Input.Check(default); return Input.Origin; } }
        bool ISchemaNodeFacts.HasNoTypedValue { get { ProviderDiagnostics.Hit(15); Input.Check(default); return native.HasNoTypedValue; } }
        bool ISchemaNodeFacts.IsNilled { get { ProviderDiagnostics.Hit(16); Input.Check(default); return native.IsNilled; } }
        SchemaResult<ISchemaTypedValue> ISchemaNodeFacts.ReadTypedValue(CancellationToken token)
        {
            ProviderDiagnostics.Hit(17); Input.Check(token);
            if (native.HasNoTypedValue) throw new InvalidOperationException("FOTY0012: Native element has no typed value.");
            return value is null ? Outcome<ISchemaTypedValue>.Unsupported() : Outcome<ISchemaTypedValue>.Supported(value);
        }
    }
    internal sealed class ProjectedNode(Input input, XDocumentNode inner) : ISchemaAnnotatedNode
    {
        internal readonly Input Input = input;
        internal Facts? State;
        private T Read<T>(Func<T> read) { Input.Check(default); return read(); }
        ISchemaNodeFacts ISchemaAnnotatedNode.OriginalSchemaFacts => Read(() => State ?? throw new NotSupportedException("No assessed element facts for this node kind."));
        XdmNodeKind IXdmNode.NodeKind => Read(() => inner.NodeKind);
        string IXdmNode.LocalName => Read(() => inner.LocalName);
        string IXdmNode.NamespaceUri => Read(() => inner.NamespaceUri);
        string IXdmNode.Prefix => Read(() => inner.Prefix);
        string IXdmNode.StringValue => Read(() => inner.StringValue);
        // Poison the legacy typed route: public data must obtain the issued facts.
        XdmValue IXdmNode.TypedValue => Read<XdmValue>(() => throw new NotSupportedException("Use authenticated original node facts."));
        bool IXdmNode.HasNoTypedValue => Read<bool>(() => throw new NotSupportedException("Use authenticated original node facts."));
        bool IXdmNode.IsComplexType => Read<bool>(() => throw new NotSupportedException("Legacy schema metadata is outside this projection."));
        bool IXdmNode.IsNilled => Read<bool>(() => throw new NotSupportedException("Use authenticated original node facts."));
        (string NamespaceUri, string LocalName)? IXdmNode.SchemaTypeAnnotation => Read<(string, string)?>(() => throw new NotSupportedException("Use authenticated original node facts."));
        (string NamespaceUri, string LocalName)? IXdmNode.SchemaElementDeclaration => Read<(string, string)?>(() => throw new NotSupportedException("Use authenticated original node facts."));
        (string NamespaceUri, string LocalName)? IXdmNode.SchemaAttributeDeclaration => Read<(string, string)?>(() => throw new NotSupportedException("Attribute schema projection is unsupported."));
        IXdmNode? IXdmNode.Parent => Read(() => inner.Parent is XDocumentNode node ? Input.Wrap(node) : null);
        IXdmNode? IXdmNode.Document => Read(() => inner.Document is XDocumentNode node ? Input.Wrap(node) : null);
        XdmSequence IXdmNode.Children(XdmNodeKind kind) => Read(() => Input.Project(inner.Children(kind)));
        XdmSequence IXdmNode.Attributes(string? localName, string? namespaceUri) => Read(() => Input.Project(inner.Attributes(localName, namespaceUri)));
        XdmSequence IXdmNode.Axis(XdmAxis axis) => Read(() => axis == XdmAxis.Namespace
            ? throw new NotSupportedException("Namespace projection is outside this prototype.") : Input.Project(inner.Axis(axis)));
        bool IXdmNode.IsSameNode(IXdmNode other) => Read(() => ReferenceEquals(this, other));
        long IXdmNode.DocumentOrder => Read(() => inner.DocumentOrder);
        string IXdmNode.BaseUri => Read(() => inner.BaseUri);
        string IXdmNode.DocumentUri => Read(() => inner.DocumentUri);
        string IXdmNode.ToXmlString() => Read(() => inner.ToXmlString());
    }
    private sealed class NodeSequence(Input input, XdmValue[] values) : IXdmSequence
    {
        bool IXdmSequence.TryGetLength(out int length) { input.Check(default); length = values.Length; return true; }
        IXdmSequenceEnumerator IXdmSequence.GetEnumerator() { input.Check(default); return new Cursor(input, values); }
        private sealed class Cursor(Input owner, XdmValue[] items) : IXdmSequenceEnumerator
        {
            private int _position = -1;
            XdmValue IXdmSequenceEnumerator.Current
            {
                get { owner.Check(default); return _position >= 0 && _position < items.Length ? items[_position] : throw new InvalidOperationException("Cursor is not on a node."); }
            }
            bool IXdmSequenceEnumerator.MoveNext() { owner.Check(default); return ++_position < items.Length; }
        }
    }
}
