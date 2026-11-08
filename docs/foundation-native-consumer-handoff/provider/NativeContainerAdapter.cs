// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Reissues native preassessed decimals through isolated public typed-container contracts.
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
//                      | Charles Korthout | 0.4   | 06-11-2026     | Add strict assessment delegation and explicit original-source output lineage.             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Xml.Linq;
using Bosak.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;

namespace Bosak.Schema.ContainerPrototype;

/// <summary>Reissues preassessed native decimal containers for an isolated public transport prototype.</summary>
internal sealed partial class NativeContainerAdapter : ISchemaAssessor, ISchemaAuthority
{
    private readonly XsdDocumentAssessor _native;
    private readonly Dictionary<object, Handle> _handles = new(ReferenceEqualityComparer.Instance);
    internal NativeContainerAdapter(XsdDocumentAssessor native)
    {
        _native = native;
        if (!native.IsSupported) throw new NotSupportedException("Native schema profile is unsupported.");
        foreach (var declaration in native.ChildDeclarations.Prepend(native.RootDeclaration!))
        {
            _handles.TryAdd(declaration, new Handle(this, declaration));
            _handles.TryAdd(declaration.Type, new Handle(this, declaration.Type));
        }
    }
    ISchemaAuthority ISchemaAssessor.Authority => this;
    SchemaLanguageVersion ISchemaAssessor.SchemaLanguage => SchemaLanguageVersion.Xsd11;
    string ISchemaAssessor.AssessmentProfile => "preassessed-native-decimal-container-prototype/" + _native.Profile;
    SchemaResult<ISchemaAssessmentAdvisor> ISchemaAssessor.Bind(IQuerySemanticEnvironment environment, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Only the engine's selected typed identity path is proposed here.
        if (environment.HostLanguage != QueryHostLanguage.XPath || environment.HostVersion != "3.1" ||
            environment.BehaviorProfile is not ("bosak-typed-identity-v1" or "bosak-original-node-data-v1") ||
            environment.XdmSpecificationRevision.AbsoluteUri != "https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/")
            return Outcome<ISchemaAssessmentAdvisor>.Unsupported();
        return Outcome<ISchemaAssessmentAdvisor>.Supported(new Advisor(this, environment));
    }
    bool ISchemaAuthority.Owns(ISchemaComponentHandle component) => component is Handle h && ReferenceEquals(h.Owner, this) &&
        _handles.TryGetValue(h.Component, out var issued) && ReferenceEquals(issued, h);
    SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveType(SchemaExpandedName name, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Find(_handles.Where(p => p.Key is AssessedSchemaType t && t.Name?.NamespaceName == name.NamespaceUri && t.Name.LocalName == name.LocalName).Select(p => p.Value));
    }
    SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveElementDeclaration(SchemaExpandedName name, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Name lookup is global-only; local declarations are obtained from native state.
        var root = _native.RootDeclaration!;
        return root.Name.NamespaceName == name.NamespaceUri && root.Name.LocalName == name.LocalName
            ? Outcome<ISchemaComponentHandle>.Supported(_handles[root]) : Outcome<ISchemaComponentHandle>.NotFound();
    }
    SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveAttributeDeclaration(SchemaExpandedName name, CancellationToken token)
    { token.ThrowIfCancellationRequested(); return Outcome<ISchemaComponentHandle>.Unsupported(); }
    SchemaResult<bool> ISchemaAuthority.IsSameComponent(ISchemaComponentHandle first, ISchemaComponentHandle second, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!((ISchemaAuthority)this).Owns(first) || !((ISchemaAuthority)this).Owns(second)) throw new ArgumentException("Foreign component.");
        return Outcome<bool>.Supported(ReferenceEquals(first, second));
    }
    SchemaResult<bool> ISchemaAuthority.IsDerivedFrom(ISchemaComponentHandle actual, ISchemaComponentHandle expected, CancellationToken token)
    { token.ThrowIfCancellationRequested(); return Outcome<bool>.Unsupported(); }
    SchemaResult<bool> ISchemaAuthority.IsAffiliatedWith(ISchemaComponentHandle member, ISchemaComponentHandle head, CancellationToken token)
    { token.ThrowIfCancellationRequested(); return Outcome<bool>.Unsupported(); }
    SchemaResult<ImmutableArray<ISchemaComponentHandle>> ISchemaAuthority.GetComponentClosure(ISchemaComponentHandle governingType, CancellationToken token)
    { token.ThrowIfCancellationRequested(); return Outcome<ImmutableArray<ISchemaComponentHandle>>.Unsupported(); }
    private static SchemaResult<ISchemaComponentHandle> Find(IEnumerable<Handle> candidates)
    {
        var values = candidates.ToArray();
        return values.Length == 1 ? Outcome<ISchemaComponentHandle>.Supported(values[0]) : Outcome<ISchemaComponentHandle>.NotFound();
    }
    private sealed class Handle(NativeContainerAdapter owner, object component) : ISchemaComponentHandle
    {
        internal readonly NativeContainerAdapter Owner = owner;
        internal readonly object Component = component;
        ISchemaAuthority ISchemaComponentHandle.Authority => Owner;
    }
    internal sealed partial class Advisor(NativeContainerAdapter owner, IQuerySemanticEnvironment environment) : ISchemaAssessmentAdvisor
    {
        internal Input Capture(XDocument instance, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var result = owner._native.Assess(instance, token);
            if (result.Outcome != SchemaAssessmentOutcome.Valid) throw new NotSupportedException("Only complete valid native results can be reissued.");
            return Reissue(result.Document!, token);
        }
        internal Input Reissue(AssessedDocument document, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(document.Schema, owner._native)) throw new ArgumentException("Foreign native assessor.");
            return new Input(this, document);
        }
        ISchemaAssessor ISchemaAssessmentAdvisor.Assessor => owner;
        IQuerySemanticEnvironment ISchemaAssessmentAdvisor.QueryEnvironment => environment;
        bool ISchemaAssessmentAdvisor.OwnsAssessment(ISchemaAssessmentOrigin origin) => origin is Origin o && ReferenceEquals(o.Input.Owner, this) || origin is AttemptOrigin attempt && ReferenceEquals(attempt.Owner, this);
        bool ISchemaAssessmentAdvisor.OwnsNodeFacts(ISchemaNodeFacts facts) => facts is Facts f && ReferenceEquals(f.Input.Owner, this) && f.Input.IsAvailable;
        bool ISchemaAssessmentAdvisor.OwnsNode(IXdmNode node, ISchemaNodeFacts facts, ISchemaAssessmentInput input, ISchemaAssessmentOrigin origin) =>
            node is ProjectedNode n && facts is Facts f && ReferenceEquals(n.State, f) && ReferenceEquals(f.Node, n) && ReferenceEquals(n.Input, input) &&
            ReferenceEquals(f.Input, input) && ReferenceEquals(f.Input.Owner, this) && ReferenceEquals(f.Input.Origin, origin) && f.Input.IsAvailable;
        bool ISchemaAssessmentAdvisor.OwnsTypedValue(ISchemaTypedValue value, ISchemaAssessmentInput input, ISchemaAssessmentOrigin? originalAssessment) =>
            value is Container c && ReferenceEquals(c.Input.Owner, this) && ReferenceEquals(c.Input, input) && ReferenceEquals(c.Input.Origin, originalAssessment) && c.Input.IsAvailable;
        void ISchemaAssessmentAdvisor.ThrowIfUnavailable(CancellationToken token) => token.ThrowIfCancellationRequested();
        ISchemaAssessmentResult ISchemaAssessmentAdvisor.Assess(SchemaAssessmentRequest request, CancellationToken token)
        { return AssessStrict(request, token); }
        SchemaResult<ISchemaTypedValue> ISchemaAssessmentAdvisor.ConstructSimpleValue(XdmValue input, ISchemaComponentHandle targetType, ISchemaAssessmentEpisode episode, CancellationToken token)
        { token.ThrowIfCancellationRequested(); return Outcome<ISchemaTypedValue>.Unsupported(); }
        internal ISchemaComponentHandle Type(AssessedElement element) => owner._handles[element.Declaration.Type];
        internal ISchemaComponentHandle Declaration(AssessedElement element) => owner._handles[element.Declaration];
    }
    internal sealed partial class Input : ISchemaAssessmentInput, IQueryOperationLifetime, IDisposable
    {
        internal readonly Advisor Owner;
        internal readonly AssessedDocument Native;
        internal readonly ISchemaAssessmentOrigin Origin;
        internal readonly ImmutableArray<ISchemaTypedValue> Values;
        private readonly IXdmNode _root;
        private readonly XDocument _document;
        private readonly string _capturedXml;
        private readonly HashSet<IXdmNode> _nodes = new(ReferenceEqualityComparer.Instance);
        private readonly ISchemaSourceBinding _source = new Source();
        private readonly ISchemaSnapshotBinding _snapshot = new Snapshot();
        private int _closed;
        internal int Releases;
        internal bool IsAvailable => Volatile.Read(ref _closed) == 0;
        internal Input(Advisor owner, AssessedDocument native, ISchemaSourceBinding? originalSource = null)
        {
            Owner = owner; Native = native; Origin = new Origin(this);
            if (originalSource is not null) _source = originalSource;
            // The retained assessment creates this private stripped copy; caller edits never reach it.
            _document = native.CreateDocumentCopy(false);
            _capturedXml = _document.ToString(SaveOptions.DisableFormatting);
            _root = Wrap(new XDocumentNode(_document));
            var values = ImmutableArray.CreateBuilder<ISchemaTypedValue>();
            foreach (var state in native.Elements.Skip(1))
            {
                if (state.HasNoTypedValue || state.IsNilled || state.TypedValue is not { Kind: XdmValueKind.Decimal })
                    throw new NotSupportedException("Only existing exact decimal singleton states are reissued.");
                values.Add(new Container(this, owner.Type(state), state.TypedValue.Value));
            }
            Values = values.ToImmutable();
            var elements = _document.Root!.DescendantsAndSelf().ToArray();
            if (elements.Length != native.Elements.Count) throw new InvalidOperationException("Native element mapping mismatch.");
            for (int i = 0; i < elements.Length; i++)
            {
                var node = Wrap(new XDocumentNode(elements[i]));
                node.State = new Facts(this, node, native.Elements[i], i == 0 ? null : Values[i - 1]);
            }
        }
        IXdmNode ISchemaAssessmentInput.Root { get { Check(default); return _root; } }
        ISchemaSourceBinding ISchemaAssessmentInput.Source => _source;
        ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot => _snapshot;
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node) => IsAvailable && _nodes.Contains(node);
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token) => Check(token);
        internal void Check(CancellationToken token)
        {
            token.ThrowIfCancellationRequested(); ObjectDisposedException.ThrowIf(!IsAvailable, this);
            if (_document.ToString(SaveOptions.DisableFormatting) != _capturedXml)
            {
                Interlocked.Exchange(ref _closed, 1);
                throw new InvalidOperationException("Captured XML snapshot changed; input revoked.");
            }
        }
        IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken token) { Check(token); return new Lease(this); }
        /// <summary>Revokes this prototype-owned input without disposing caller XML or the native assessor.</summary>
        public void Dispose() => Interlocked.Exchange(ref _closed, 1);
        private sealed class Source : ISchemaSourceBinding { }
        private sealed class Snapshot : ISchemaSnapshotBinding { }
        private sealed class Lease(Input input) : IQueryOperationLease
        {
            private int _closed;
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken token) { ObjectDisposedException.ThrowIf(Volatile.Read(ref _closed) != 0, this); input.Check(token); }
            /// <inheritdoc/>
            public void Dispose() { if (Interlocked.Exchange(ref _closed, 1) == 0) Interlocked.Increment(ref input.Releases); }
        }
    }
    private sealed class Origin(Input input) : ISchemaAssessmentOrigin
    {
        internal readonly Input Input = input;
        ISchemaSourceBinding ISchemaAssessmentOrigin.Source => ((ISchemaAssessmentInput)Input).Source;
    }
    private sealed class Container(Input input, ISchemaComponentHandle type, XdmValue plain) : ISchemaTypedValue
    {
        internal readonly Input Input = input;
        private readonly ISchemaAtomicProvenance _provenance = new Provenance(input, type);
        ISchemaComponentHandle ISchemaTypedValue.GoverningType { get { Input.Check(default); return type; } }
        bool ISchemaTypedValue.IsList { get { Input.Check(default); return false; } }
        IEnumerable<SchemaAtomicItem> ISchemaTypedValue.AtomicItems
        { get { Input.Check(default); return Enumerate(); } }
        private IEnumerable<SchemaAtomicItem> Enumerate() { Input.Check(default); yield return new(plain, _provenance); Input.Check(default); }
        bool ISchemaTypedValue.TryGetSingleAtomic(out SchemaAtomicItem item) { Input.Check(default); item = new(plain, _provenance); return true; }
    }
    private sealed class Provenance(Input input, ISchemaComponentHandle type) : ISchemaAtomicProvenance
    {
        ISchemaComponentHandle ISchemaAtomicProvenance.DeclaredType { get { input.Check(default); return type; } }
        ISchemaComponentHandle ISchemaAtomicProvenance.ActualType { get { input.Check(default); return type; } }
        ISchemaAssessmentOrigin ISchemaAtomicProvenance.OriginalAssessment { get { input.Check(default); return input.Origin; } }
    }
    private sealed class UnsupportedAssessment : ISchemaAssessmentResult
    {
        SchemaAssessmentStatus ISchemaAssessmentResult.Status => SchemaAssessmentStatus.Unsupported;
        ISchemaAssessmentEvidence? ISchemaAssessmentResult.Evidence => null;
        IXdmNode? ISchemaAssessmentResult.ValidatedRoot => null;
        ImmutableArray<SchemaAssessmentDiagnostic> ISchemaAssessmentResult.Diagnostics => [];
        bool ISchemaAssessmentResult.DiagnosticsTruncated => false;
    }
    private sealed class Outcome<T>(SchemaResultKind kind, T payload) : SchemaResult<T>
    {
        internal static Outcome<T> Supported(T value) => new(SchemaResultKind.Supported, value);
        internal static Outcome<T> Unsupported() => new(SchemaResultKind.Unsupported, default!);
        internal static Outcome<T> NotFound() => new(SchemaResultKind.NotFound, default!);
        /// <inheritdoc/>
        public override SchemaResultKind Kind => kind;
        /// <inheritdoc/>
        public override T Value => kind == SchemaResultKind.Supported ? payload : throw new InvalidOperationException("Unsupported outcome.");
        /// <inheritdoc/>
        public override string? DiagnosticCode => kind == SchemaResultKind.Unsupported ? "PROTOTYPE-SCOPE" : null;
    }
}
