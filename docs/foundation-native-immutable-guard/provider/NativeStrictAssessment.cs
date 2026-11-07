// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Adapts bounded strict document requests with transactional results and diagnostic limits.
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
//                      | Charles Korthout | 0.5   | 06-11-2026     | Recheck cancellation after capturing caller-supplied episode constants.                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Xml.Linq;
using Bosak.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;

namespace Bosak.Schema.ContainerPrototype;

/// <summary>Adapts strict provider-owned document requests without broadening the native assessment profile.</summary>
internal sealed partial class NativeContainerAdapter
{
    internal sealed partial class Advisor
    {
        internal RequestInput CaptureRequest(XDocument instance) => new(this, instance);
        private ISchemaAssessmentResult AssessStrict(SchemaAssessmentRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Input); ArgumentNullException.ThrowIfNull(request.Episode);
            if (!Enum.IsDefined(request.Mode)) throw new ArgumentOutOfRangeException(nameof(request));
            var limits = request.Limits;
            if (limits is { MaxDiagnostics: <= 0 } or { MaxDiagnosticCharacters: <= 0 }) throw new ArgumentOutOfRangeException(nameof(request));
            int maxCount = Math.Min(limits?.MaxDiagnostics ?? 8, 8);
            long maxCharacters = Math.Min(limits?.MaxDiagnosticCharacters ?? 4096, 4096);
            var capturedTime = request.Episode.CurrentDateTime;
            var capturedZone = request.Episode.ImplicitTimezoneOffsetMinutes;
            if (capturedZone is < -840 or > 840) throw new ArgumentOutOfRangeException(nameof(request));
            token.ThrowIfCancellationRequested();
            _ = capturedTime; // Native profile has no date/time functions; never consult an ambient clock.
            if (request.RequestedType is not null && !((ISchemaAuthority)owner).Owns(request.RequestedType)) throw new ArgumentException("Foreign requested type.");
            if (request.GoverningDeclaration is not null && !((ISchemaAuthority)owner).Owns(request.GoverningDeclaration)) throw new ArgumentException("Foreign declaration.");
            if (request.Mode != SchemaAssessmentMode.Strict || request.RequestedType is not null)
                return Refused(maxCount, maxCharacters);
            if (request.GoverningDeclaration is not null && !ReferenceEquals(request.GoverningDeclaration, owner._handles[owner._native.RootDeclaration!]))
                return Refused(maxCount, maxCharacters);
            // This prototype supports its actual owner capture, not arbitrary claimed host bindings.
            if (request.Input is not RequestInput input || !ReferenceEquals(input.Owner, this)) return Refused(maxCount, maxCharacters);
            using var lease = ((IQueryOperationLifetime)input).Enter(token);
            Input? output = null;
            try
            {
                var candidate = input.Copy(token);
                var native = owner._native.Assess(candidate, token);
                lease.ThrowIfUnavailable(token);
                var status = native.Outcome switch
                {
                    SchemaAssessmentOutcome.Valid => SchemaAssessmentStatus.Valid,
                    SchemaAssessmentOutcome.Invalid => SchemaAssessmentStatus.Invalid,
                    _ => SchemaAssessmentStatus.Unsupported
                };
                var diagnostics = Bound(native.Diagnostics.Select(d => new SchemaAssessmentDiagnostic(d.Code, d.Message)), maxCount, maxCharacters);
                ISchemaAssessmentOrigin origin;
                if (status == SchemaAssessmentStatus.Valid)
                {
                    if (native.Document is null) throw new InvalidOperationException("Valid native result lacks its document.");
                    output = new Input(this, native.Document, input.Source);
                    origin = output.Origin;
                }
                else origin = new AttemptOrigin(this, input.Source);
                var evidence = new AssessmentEvidence(origin, status, status == SchemaAssessmentStatus.Valid);
                var result = new AssessmentResult(status, evidence, output, diagnostics.Items, diagnostics.Truncated);
                lease.ThrowIfUnavailable(token); output?.Check(token);
                return result;
            }
            catch { output?.Dispose(); throw; }
        }
        private static AssessmentResult Refused(int maxCount, long maxCharacters)
        {
            var diagnostics = Bound([new("PROTOTYPE-SCOPE", "This request is outside the strict provider-owned native document profile.")], maxCount, maxCharacters);
            return new(SchemaAssessmentStatus.Unsupported, null, null, diagnostics.Items, diagnostics.Truncated);
        }
        private static (ImmutableArray<SchemaAssessmentDiagnostic> Items, bool Truncated) Bound(IEnumerable<SchemaAssessmentDiagnostic> source, int maxCount, long maxCharacters)
        {
            var items = ImmutableArray.CreateBuilder<SchemaAssessmentDiagnostic>(); long used = 0; bool truncated = false;
            foreach (var diagnostic in source)
            {
                long length = (long)diagnostic.Code.Length + diagnostic.Message.Length;
                if (items.Count == maxCount || length > maxCharacters - used) { truncated = true; continue; }
                items.Add(diagnostic); used += length;
            }
            return (items.ToImmutable(), truncated);
        }
    }
    internal sealed class RequestInput : ISchemaAssessmentInput, IQueryOperationLifetime, IDisposable
    {
        internal readonly Advisor Owner;
        private readonly XDocument _document;
        private readonly string _xml;
        private readonly IXdmNode _root;
        private readonly ISchemaSnapshotBinding _snapshot = new RequestSnapshot();
        internal readonly ISchemaSourceBinding Source = new RequestSource();
        private int _closed;
        internal int Releases;
        internal RequestInput(Advisor owner, XDocument instance)
        { Owner = owner; _document = new(instance); _xml = _document.ToString(SaveOptions.DisableFormatting); _root = new XDocumentNode(_document); }
        internal XDocument Copy(CancellationToken token)
        { Check(token); var result = new XDocument(_document); Check(token); return result; }
        internal void Check(CancellationToken token)
        {
            token.ThrowIfCancellationRequested(); ObjectDisposedException.ThrowIf(Volatile.Read(ref _closed) != 0, this);
            if (_document.ToString(SaveOptions.DisableFormatting) != _xml)
            { Interlocked.Exchange(ref _closed, 1); throw new InvalidOperationException("Request snapshot changed; input revoked."); }
        }
        IXdmNode ISchemaAssessmentInput.Root { get { Check(default); return _root; } }
        ISchemaSourceBinding ISchemaAssessmentInput.Source => Source;
        ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot => _snapshot;
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node) => Volatile.Read(ref _closed) == 0 && ReferenceEquals(node, _root);
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token) => Check(token);
        IQueryOperationLease IQueryOperationLifetime.Enter(CancellationToken token) { Check(token); return new RequestLease(this); }
        /// <summary>Revokes the provider-owned captured request without disposing the caller XML.</summary>
        public void Dispose() => Interlocked.Exchange(ref _closed, 1);
        private sealed class RequestSource : ISchemaSourceBinding { }
        private sealed class RequestSnapshot : ISchemaSnapshotBinding { }
        private sealed class RequestLease(RequestInput input) : IQueryOperationLease
        {
            private int _closed;
            /// <inheritdoc/>
            public void ThrowIfUnavailable(CancellationToken token) { ObjectDisposedException.ThrowIf(Volatile.Read(ref _closed) != 0, this); input.Check(token); }
            /// <inheritdoc/>
            public void Dispose() { if (Interlocked.Exchange(ref _closed, 1) == 0) Interlocked.Increment(ref input.Releases); }
        }
    }
    private sealed class AttemptOrigin(Advisor owner, ISchemaSourceBinding source) : ISchemaAssessmentOrigin
    {
        internal readonly Advisor Owner = owner;
        ISchemaSourceBinding ISchemaAssessmentOrigin.Source => source;
    }
    private sealed class AssessmentEvidence(ISchemaAssessmentOrigin origin, SchemaAssessmentStatus status, bool complete) : ISchemaAssessmentEvidence
    {
        ISchemaAssessmentOrigin ISchemaAssessmentEvidence.Origin => origin;
        SchemaAssessmentStatus ISchemaAssessmentEvidence.Status => status;
        bool ISchemaAssessmentEvidence.IsComplete => complete;
    }
    internal sealed class AssessmentResult(SchemaAssessmentStatus status, ISchemaAssessmentEvidence? evidence, Input? output, ImmutableArray<SchemaAssessmentDiagnostic> diagnostics, bool truncated) : ISchemaAssessmentResult
    {
        internal readonly Input? Output = output;
        SchemaAssessmentStatus ISchemaAssessmentResult.Status => status;
        ISchemaAssessmentEvidence? ISchemaAssessmentResult.Evidence => evidence;
        IXdmNode? ISchemaAssessmentResult.ValidatedRoot => Output is null ? null : ((ISchemaAssessmentInput)Output).Root;
        ImmutableArray<SchemaAssessmentDiagnostic> ISchemaAssessmentResult.Diagnostics => diagnostics;
        bool ISchemaAssessmentResult.DiagnosticsTruncated => truncated;
    }
}
