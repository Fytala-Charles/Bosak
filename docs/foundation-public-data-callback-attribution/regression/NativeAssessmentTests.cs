// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Verifies public strict native assessment requests, failure isolation and result ownership.
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
//                      | Charles Korthout | 0.5   | 06-11-2026     | Preserve native Invalid/Unsupported distinction and cover count truncation/cancellation.   |
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

/// <summary>Verifies strict public requests, bounded diagnostics and transactional native result publication.</summary>
public sealed class NativeAssessmentTests
{
    private sealed class Fixture : IDisposable
    {
        internal readonly NativeContainerAdapter Endpoint;
        internal readonly SchemaQuerySession Session;
        internal readonly NativeContainerAdapter.Advisor Advisor;
        internal Fixture(bool multipleFailures = false)
        {
            var schema = XDocument.Parse("""
                <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"><xs:element name="order"><xs:complexType>
                <xs:sequence><xs:element name="amount" type="xs:decimal" minOccurs="0" maxOccurs="2"/></xs:sequence>
                <xs:assert test="count(amount) le 2"/></xs:complexType></xs:element></xs:schema>
                """);
            if (multipleFailures)
            {
                XNamespace xs = "http://www.w3.org/2001/XMLSchema";
                var type = schema.Descendants(xs + "complexType").Single();
                type.Add(new XElement(xs + "assert", new XAttribute("test", "count(amount) eq 0")));
                type.Add(new XElement(xs + "assert", new XAttribute("test", "count(amount) eq 1")));
            }
            var native = SchemaServices.CreateDocumentAssessor(schema);
            Endpoint = new(native); Session = SchemaQuerySession.CompileXPath("data($node)", new(4, 8, 8, 128, 128), assessor: Endpoint).Value;
            Advisor = Assert.IsType<NativeContainerAdapter.Advisor>(Session.Advisor);
        }
        internal ISchemaAssessmentResult Assess(ISchemaAssessmentInput input, SchemaAssessmentLimits? limits = null) =>
            ((ISchemaAssessmentAdvisor)Advisor).Assess(new(input, SchemaAssessmentMode.Strict, null, null, new Episode(), limits), default);
        /// <inheritdoc/>
        public void Dispose() => Session.Dispose();
    }
    private sealed class Episode : ISchemaAssessmentEpisode
    {
        internal int Zone; internal Action? ReadTime;
        DateTimeOffset ISchemaAssessmentEpisode.CurrentDateTime { get { ReadTime?.Invoke(); return new(2026, 11, 6, 0, 0, 0, TimeSpan.Zero); } }
        int ISchemaAssessmentEpisode.ImplicitTimezoneOffsetMinutes => Zone;
    }
    private static IXdmNode[] Nodes(XdmSequence values)
    { var nodes = new List<IXdmNode>(); foreach (var value in values) nodes.Add(value.NodeValue); return nodes.ToArray(); }
    /// <summary>A complete valid public request returns an owned root with the original source and fresh snapshot.</summary>
    [Fact] public void ValidRequestPublishesNativeRoot()
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order><amount> 01.250 </amount></order>"));
        var result = f.Assess(request); Assert.Equal(SchemaAssessmentStatus.Valid, result.Status);
        Assert.True(result.Evidence!.IsComplete); Assert.Equal(result.Status, result.Evidence.Status);
        Assert.True(((ISchemaAssessmentAdvisor)f.Advisor).OwnsAssessment(result.Evidence.Origin));
        Assert.Same(((ISchemaAssessmentInput)request).Source, result.Evidence.Origin.Source);
        using var output = Assert.IsType<NativeContainerAdapter.AssessmentResult>(result).Output!;
        Assert.Same(((ISchemaAssessmentInput)request).Source, ((ISchemaAssessmentInput)output).Source);
        Assert.NotSame(((ISchemaAssessmentInput)request).Snapshot, ((ISchemaAssessmentInput)output).Snapshot);
        Assert.Same(((ISchemaAssessmentInput)output).Root, result.ValidatedRoot); Assert.Empty(result.Diagnostics);
        Assert.False(result.DiagnosticsTruncated); Assert.Equal(1, request.Releases);
        using var snapshot = f.Session.OpenSnapshot(output, output, result.Evidence.Origin).Value;
        var root = Nodes(result.ValidatedRoot!.Children(XdmNodeKind.Element)).Single();
        var child = Nodes(root.Children(XdmNodeKind.Element)).Single();
        f.Session.Run([snapshot], op => { Assert.Equal(1.250m, op.Evaluate(new EvaluationContext().WithVariable("node", XdmValue.FromNode(child))).DecimalValue); return 0; });
    }
    /// <summary>Invalid values and unsupported instance coverage expose no validated root.</summary>
    /// <param name="xml">Candidate instance.</param><param name="status">Expected native outcome.</param>
    [Theory][InlineData("<order><amount>bad</amount></order>", SchemaAssessmentStatus.Invalid)]
    [InlineData("<order unknown='yes'/>", SchemaAssessmentStatus.Invalid)]
    [InlineData("<order><amount>0.00000000000000000000000000001</amount></order>", SchemaAssessmentStatus.Unsupported)]
    public void FailureNeverPublishesRoot(string xml, SchemaAssessmentStatus status)
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse(xml));
        var result = f.Assess(request); Assert.Equal(status, result.Status); Assert.Null(result.ValidatedRoot);
        Assert.Null(Assert.IsType<NativeContainerAdapter.AssessmentResult>(result).Output);
        Assert.False(result.Evidence!.IsComplete); Assert.Equal(status, result.Evidence.Status);
        Assert.True(((ISchemaAssessmentAdvisor)f.Advisor).OwnsAssessment(result.Evidence.Origin));
        Assert.All(result.Diagnostics, d => Assert.Null(d.Location)); Assert.NotEmpty(result.Diagnostics); Assert.Equal(1, request.Releases);
    }
    /// <summary>Diagnostic truncation is independent of invalid coverage and cannot manufacture a valid result.</summary>
    [Fact] public void TinyDiagnosticBudgetPreservesInvalidStatus()
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order><amount>bad</amount></order>"));
        var result = f.Assess(request, new(1, 1)); Assert.Empty(result.Diagnostics); Assert.True(result.DiagnosticsTruncated);
        Assert.Equal(SchemaAssessmentStatus.Invalid, result.Status); Assert.Null(result.ValidatedRoot); Assert.False(result.Evidence!.IsComplete);
    }
    /// <summary>Positive tightened diagnostic limits still allow a diagnostic-free complete success.</summary>
    [Fact] public void TinyBudgetDoesNotInvalidateValidDocument()
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order/>"));
        var result = f.Assess(request, new(1, 1)); using var output = Assert.IsType<NativeContainerAdapter.AssessmentResult>(result).Output!;
        Assert.Equal(SchemaAssessmentStatus.Valid, result.Status); Assert.True(result.Evidence!.IsComplete); Assert.False(result.DiagnosticsTruncated);
    }
    /// <summary>Zero or negative diagnostic limits fail before any input lease is acquired.</summary>
    /// <param name="count">Count limit.</param><param name="characters">Character limit.</param>
    [Theory][InlineData(0, 1)][InlineData(1, 0)][InlineData(-1, 100)][InlineData(1, -1)]
    public void InvalidLimitsRefuse(int count, long characters)
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order/>"));
        Assert.Throws<ArgumentOutOfRangeException>(() => f.Assess(request, new(count, characters))); Assert.Equal(0, request.Releases);
    }
    /// <summary>Lax and type requests remain explicitly unsupported and publish nothing.</summary>
    /// <param name="mode">Unimplemented request mode.</param>
    [Theory][InlineData(SchemaAssessmentMode.Lax)][InlineData(SchemaAssessmentMode.Type)]
    public void OtherModesRefuse(SchemaAssessmentMode mode)
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order/>"));
        var result = ((ISchemaAssessmentAdvisor)f.Advisor).Assess(new(request, mode, null, null, new Episode()), default);
        Assert.Equal(SchemaAssessmentStatus.Unsupported, result.Status); Assert.Null(result.ValidatedRoot); Assert.Null(result.Evidence); Assert.Equal(0, request.Releases);
    }
    /// <summary>An owned explicit root declaration selects the actual native root authority.</summary>
    [Fact] public void ExplicitOwnedRootDeclaration()
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order/>"));
        var declaration = ((ISchemaAuthority)f.Endpoint).ResolveElementDeclaration(new("", "order"), default).Value;
        var result = ((ISchemaAssessmentAdvisor)f.Advisor).Assess(new(request, SchemaAssessmentMode.Strict, null, declaration, new Episode()), default);
        using var output = Assert.IsType<NativeContainerAdapter.AssessmentResult>(result).Output!; Assert.Equal(SchemaAssessmentStatus.Valid, result.Status);
    }
    /// <summary>Foreign declarations cannot authorize strict assessment even with an equal schema name.</summary>
    [Fact] public void ForeignDeclarationRefuses()
    {
        using var f = new Fixture(); using var other = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order/>"));
        var declaration = ((ISchemaAuthority)other.Endpoint).ResolveElementDeclaration(new("", "order"), default).Value;
        Assert.Throws<ArgumentException>(() => ((ISchemaAssessmentAdvisor)f.Advisor).Assess(new(request, SchemaAssessmentMode.Strict, null, declaration, new Episode()), default));
        Assert.Equal(0, request.Releases);
    }
    /// <summary>A request captured by another advisor is unsupported without borrowing its resource.</summary>
    [Fact] public void ForeignAdvisorInputRefuses()
    {
        using var f = new Fixture(); using var other = new Fixture(); using var request = other.Advisor.CaptureRequest(XDocument.Parse("<order/>"));
        var result = f.Assess(request); Assert.Equal(SchemaAssessmentStatus.Unsupported, result.Status); Assert.Null(result.ValidatedRoot); Assert.Equal(0, request.Releases);
    }
    /// <summary>Unknown claimed input bindings are refused before their getters execute.</summary>
    [Fact] public void UnknownInputRefusesBeforeGetters()
    { using var f = new Fixture(); Assert.Equal(SchemaAssessmentStatus.Unsupported, f.Assess(new PoisonInput()).Status); }
    private sealed class PoisonInput : ISchemaAssessmentInput
    {
        IXdmNode ISchemaAssessmentInput.Root => throw new Exception("Untrusted getter.");
        ISchemaSourceBinding ISchemaAssessmentInput.Source => throw new Exception("Untrusted getter.");
        ISchemaSnapshotBinding ISchemaAssessmentInput.Snapshot => throw new Exception("Untrusted getter.");
        bool ISchemaAssessmentInput.OwnsNode(IXdmNode node) => throw new Exception("Untrusted callback.");
        void ISchemaAssessmentInput.ThrowIfUnavailable(CancellationToken token) => throw new Exception("Untrusted callback.");
    }
    /// <summary>Cancellation before entry publishes nothing and acquires no request lease.</summary>
    [Fact] public void CanceledRequestRefuses()
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order/>")); using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => ((ISchemaAssessmentAdvisor)f.Advisor).Assess(new(request, SchemaAssessmentMode.Strict, null, null, new Episode()), cancellation.Token));
        Assert.Equal(0, request.Releases);
    }
    /// <summary>Cancellation during episode capture is checked before native work or an unsupported return.</summary>
    /// <param name="mode">Supported or unsupported request mode.</param>
    [Theory][InlineData(SchemaAssessmentMode.Strict)][InlineData(SchemaAssessmentMode.Lax)]
    public void CancellationAfterEpisodeCaptureRefuses(SchemaAssessmentMode mode)
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order/>")); using var cancellation = new CancellationTokenSource();
        var episode = new Episode { ReadTime = cancellation.Cancel };
        Assert.Throws<OperationCanceledException>(() => ((ISchemaAssessmentAdvisor)f.Advisor).Assess(new(request, mode, null, null, episode), cancellation.Token));
        Assert.Equal(0, request.Releases);
    }
    /// <summary>Out-of-bounds episode timezone fails before an input lease is acquired.</summary>
    [Fact] public void InvalidEpisodeRefuses()
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order/>"));
        Assert.Throws<ArgumentOutOfRangeException>(() => ((ISchemaAssessmentAdvisor)f.Advisor).Assess(new(request, SchemaAssessmentMode.Strict, null, null, new Episode { Zone = 841 }), default));
        Assert.Equal(0, request.Releases);
    }
    /// <summary>Edits to an exposed request snapshot revoke the request before native assessment.</summary>
    [Fact] public void MutatedRequestSnapshotRefuses()
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order/>"));
        var root = (Bosak.XPath.Providers.Xml.XDocumentNode)((ISchemaAssessmentInput)request).Root;
        ((XDocument)root.UnderlyingObject).Root!.Name = "other";
        Assert.Throws<InvalidOperationException>(() => f.Assess(request)); Assert.Equal(0, request.Releases);
    }
    /// <summary>Revoked requests cannot be revalidated.</summary>
    [Fact] public void RevokedRequestRefuses()
    { using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order/>")); request.Dispose(); Assert.Throws<ObjectDisposedException>(() => f.Assess(request)); }
    /// <summary>Request and caller XML lifetimes do not own the successful output snapshot.</summary>
    [Fact] public void OutputSurvivesRequestDisposal()
    {
        using var f = new Fixture(); var caller = XDocument.Parse("<order><amount>5</amount></order>");
        using var request = f.Advisor.CaptureRequest(caller); caller.Root!.Value = "mutated caller";
        var result = f.Assess(request); using var output = Assert.IsType<NativeContainerAdapter.AssessmentResult>(result).Output!; request.Dispose();
        using var snapshot = f.Session.OpenSnapshot(output, output, result.Evidence!.Origin).Value;
        var child = Nodes(Nodes(result.ValidatedRoot!.Children(XdmNodeKind.Element)).Single().Children(XdmNodeKind.Element)).Single();
        f.Session.Run([snapshot], op => { Assert.Equal(5m, op.Evaluate(new EvaluationContext().WithVariable("node", XdmValue.FromNode(child))).DecimalValue); return 0; });
    }
    /// <summary>Repeated assessments retain original source identity but publish independent current snapshots.</summary>
    [Fact] public void RepeatedRequestHasFreshOutputSnapshot()
    {
        using var f = new Fixture(); using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order/>"));
        var first = f.Assess(request); var second = f.Assess(request);
        using var a = Assert.IsType<NativeContainerAdapter.AssessmentResult>(first).Output!; using var b = Assert.IsType<NativeContainerAdapter.AssessmentResult>(second).Output!;
        Assert.Same(((ISchemaAssessmentInput)a).Source, ((ISchemaAssessmentInput)b).Source);
        Assert.NotSame(((ISchemaAssessmentInput)a).Snapshot, ((ISchemaAssessmentInput)b).Snapshot);
        Assert.NotSame(first.Evidence!.Origin, second.Evidence!.Origin); Assert.Equal(2, request.Releases);
    }
    /// <summary>Diagnostic count tightening preserves original provider codes and flags omitted findings.</summary>
    [Fact] public void CountBudgetTruncatesWithoutRewritingFindings()
    {
        using var f = new Fixture(multipleFailures: true);
        using var request = f.Advisor.CaptureRequest(XDocument.Parse("<order><amount>1</amount><amount>2</amount></order>"));
        var complete = f.Assess(request); Assert.Equal(2, complete.Diagnostics.Length); Assert.False(complete.DiagnosticsTruncated);
        var limited = f.Assess(request, new(1, long.MaxValue));
        Assert.Equal(SchemaAssessmentStatus.Invalid, limited.Status); Assert.Null(limited.ValidatedRoot);
        Assert.Equal(complete.Diagnostics[0], Assert.Single(limited.Diagnostics)); Assert.True(limited.DiagnosticsTruncated);
    }
}
