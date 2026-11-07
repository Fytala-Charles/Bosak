// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Verifies native decimal reissuance, input ownership and raw container lifetime.
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
//                      | Charles Korthout | 0.2   | 06-11-2026     | Keep verification on public APIs; remove private marker observation.                    |
//                      | Charles Korthout | 0.3   | 06-11-2026     | Cover exposed-node mutation, scalar execution and unsupported public assessment.          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.Schema;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.Schema.ContainerPrototype;

/// <summary>Checks actual native decimal reissuance through public engine container transport.</summary>
public sealed class NativeContainerTests
{
    private sealed class Fixture : IDisposable
    {
        internal readonly XsdDocumentAssessor Native = Compile();
        internal readonly NativeContainerAdapter Endpoint;
        internal readonly SchemaQuerySession Session;
        internal readonly NativeContainerAdapter.Advisor Advisor;
        internal readonly NativeContainerAdapter.Input Input;
        internal readonly SchemaQuerySnapshot Snapshot;
        internal Fixture(string xml = "<order><amount>1.25</amount><amount>2.50</amount></order>")
        {
            Endpoint = new(Native);
            var result = SchemaQuerySession.CompileXPath("$value", new(4, 8, 8, 128, 128), assessor: Endpoint);
            Assert.Equal(SchemaResultKind.Supported, result.Kind); Session = result.Value;
            Advisor = Assert.IsType<NativeContainerAdapter.Advisor>(Session.Advisor);
            Input = Advisor.Capture(XDocument.Parse(xml));
            Snapshot = Session.OpenSnapshot(Input, Input, Input.Origin).Value;
        }
        internal T Run<T>(Func<SchemaQueryOperation, T> callback) => Session.Run([Snapshot], callback);
        /// <inheritdoc/>
        public void Dispose() { Snapshot.Dispose(); Session.Dispose(); Input.Dispose(); }
    }
    private static XsdDocumentAssessor Compile() => SchemaServices.CreateDocumentAssessor(XDocument.Parse("""
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"><xs:element name="order"><xs:complexType>
        <xs:sequence><xs:element name="amount" type="xs:decimal" minOccurs="0" maxOccurs="2"/></xs:sequence>
        <xs:assert test="count(amount) le 2"/></xs:complexType></xs:element></xs:schema>
        """));
    /// <summary>Actual native decimals retain governing type, origin, order and plain payload.</summary>
    [Fact] public void ActualNativeCompleteContainers()
    {
        using var f = new Fixture();
        Assert.Same(f.Session.Environment, ((ISchemaAssessmentAdvisor)f.Advisor).QueryEnvironment);
        var values = f.Run(op => f.Input.Values.Select(v =>
        {
            var view = op.ReadTypedContainer(f.Snapshot, v).Value;
            Assert.False(view.IsList); var cell = Assert.Single(view.AtomicItems);
            Assert.Same(view.GoverningType, cell.TypeEvidence.DeclaredType);
            Assert.Same(view.GoverningType, cell.TypeEvidence.ActualType);
            Assert.Same(f.Input.Origin, cell.TypeEvidence.OriginalAssessment);
            Assert.Equal(XdmValueKind.Decimal, cell.Value.Kind);
            Assert.False(SchemaAtomics.TryGetProvenance(cell.Value, out _)); return cell.Value.DecimalValue;
        }).ToArray());
        Assert.Equal(new[] { 1.25m, 2.50m }, values);
    }
    /// <summary>Native absent children create no containers and root remains no-typed-value state.</summary>
    [Fact] public void NoPhantomRootOrAbsentChildContainer()
    {
        using var f = new Fixture("<order/>"); Assert.Empty(f.Input.Values);
        Assert.True(f.Input.Native.Elements[0].HasNoTypedValue);
        Assert.Null(f.Input.Native.Elements[0].TypedValue);
    }
    /// <summary>Foreign containers from another input refuse before metadata reads.</summary>
    [Fact] public void DifferentSnapshotRefusesContainer()
    {
        using var f = new Fixture(); using var other = f.Advisor.Capture(XDocument.Parse("<order><amount>1</amount></order>"));
        f.Run(op => { Assert.Throws<ArgumentException>(() => op.ReadTypedContainer(f.Snapshot, other.Values[0])); return 0; });
    }
    /// <summary>A second advisor on the same endpoint does not inherit issuance.</summary>
    [Fact] public void DifferentAdvisorRefusesOrigin()
    {
        using var f = new Fixture();
        using var second = SchemaQuerySession.CompileXPath("$value", new(4, 8, 8, 128, 128), assessor: f.Endpoint).Value;
        Assert.NotSame(f.Advisor, second.Advisor);
        Assert.Throws<ArgumentException>(() => second.OpenSnapshot(f.Input, f.Input, f.Input.Origin));
    }
    /// <summary>Independent equal schema compilation grants no native component authority.</summary>
    [Fact] public void EqualSchemaIsNotAuthority()
    {
        using var f = new Fixture(); var other = Compile(); Assert.Equal(f.Native.SchemaDigest, other.SchemaDigest);
        var native = other.Assess(XDocument.Parse("<order><amount>1</amount></order>")).Document!;
        Assert.Throws<ArgumentException>(() => f.Advisor.Reissue(native));
    }
    /// <summary>Copy reissuance keeps native components but uses distinct public source/snapshot associations.</summary>
    [Fact] public void NativeCopyReissuedExplicitly()
    {
        using var f = new Fixture(); using var copy = f.Advisor.Reissue(f.Input.Native.Copy());
        Assert.Equal(f.Input.Native.AssessmentIdentity, copy.Native.AssessmentIdentity);
        Assert.NotEqual(f.Input.Native.SnapshotIdentity, copy.Native.SnapshotIdentity);
        Assert.Same(f.Input.Native.Elements[1].Declaration, copy.Native.Elements[1].Declaration);
        Assert.NotSame(((ISchemaAssessmentInput)f.Input).Snapshot, ((ISchemaAssessmentInput)copy).Snapshot);
        Assert.NotSame(f.Input.Origin, copy.Origin);
        using var snap = f.Session.OpenSnapshot(copy, copy, copy.Origin).Value;
        f.Session.Run([snap], op => { Assert.Equal(1.25m, Assert.Single(op.ReadTypedContainer(snap, copy.Values[0]).Value.AtomicItems).Value.DecimalValue); return 0; });
    }
    /// <summary>Changing caller XML and exported copies cannot mutate retained native values.</summary>
    [Fact] public void CallerMutationIsIsolated()
    {
        using var f = new Fixture(); var xml = XDocument.Parse("<order><amount>3.75</amount></order>");
        using var captured = f.Advisor.Capture(xml); xml.Root!.Element("amount")!.Value = "999";
        var exported = captured.Native.CreateDocumentCopy(false); exported.Root!.Element("amount")!.Value = "bad";
        using var snap = f.Session.OpenSnapshot(captured, captured, captured.Origin).Value;
        f.Session.Run([snap], op => { Assert.Equal(3.75m, Assert.Single(op.ReadTypedContainer(snap, captured.Values[0]).Value.AtomicItems).Value.DecimalValue); return 0; });
    }
    /// <summary>The complete view expires at raw return while copied plain decimal survives.</summary>
    [Fact] public void RawCompleteViewExpires()
    {
        using var f = new Fixture(); ISchemaTypedValue escaped = null!; XdmValue plain = default;
        f.Run(op => { escaped = op.ReadTypedContainer(f.Snapshot, f.Input.Values[0]).Value; plain = Assert.Single(escaped.AtomicItems).Value; return 0; });
        Assert.Throws<ObjectDisposedException>(() => escaped.IsList); Assert.Equal(1.25m, plain.DecimalValue);
    }
    /// <summary>Raw provenance and enumerator cannot be read after the operation ends.</summary>
    [Fact] public void RawEvidenceAndCursorExpire()
    {
        using var f = new Fixture(); ISchemaAtomicProvenance evidence = null!; IEnumerator<SchemaAtomicItem> cursor = null!;
        f.Run(op => { var view = op.ReadTypedContainer(f.Snapshot, f.Input.Values[0]).Value; cursor = view.AtomicItems.GetEnumerator(); Assert.True(cursor.MoveNext()); evidence = cursor.Current.TypeEvidence; return 0; });
        Assert.Throws<ObjectDisposedException>(() => evidence.DeclaredType);
        Assert.Throws<ObjectDisposedException>(() => cursor.MoveNext()); cursor.Dispose();
    }
    /// <summary>Revoked input refuses before any later container transport.</summary>
    [Fact] public void InputRevocationRefuses()
    {
        using var f = new Fixture(); f.Input.Dispose(); Assert.Throws<ObjectDisposedException>(() => f.Run(op => op.ReadTypedContainer(f.Snapshot, f.Input.Values[0])));
    }
    /// <summary>Cancellation before native capture cannot publish a partial input.</summary>
    [Fact] public void CanceledCaptureRefuses()
    {
        using var f = new Fixture(); using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => f.Advisor.Capture(XDocument.Parse("<order/>"), cancellation.Token));
    }
    /// <summary>Invalid and unsupported native instances publish no provider input.</summary>
    /// <param name="xml">Invalid or unsupported native document.</param>
    [Theory][InlineData("<order><amount>bad</amount></order>")][InlineData("<order unknown='yes'/>")]
    public void NativeFailureDoesNotIssue(string xml)
    { using var f = new Fixture(); Assert.Throws<NotSupportedException>(() => f.Advisor.Capture(XDocument.Parse(xml))); }
    /// <summary>Caller-built containers cannot obtain issuance by copying valid public fields.</summary>
    [Fact] public void ForgedContainerRefusesBeforeFields()
    {
        using var f = new Fixture(); f.Run(op => { Assert.Throws<ArgumentException>(() => op.ReadTypedContainer(f.Snapshot, new PoisonContainer())); return 0; });
    }
    private sealed class PoisonContainer : ISchemaTypedValue
    {
        ISchemaComponentHandle ISchemaTypedValue.GoverningType => throw new Exception("Untrusted getter read.");
        bool ISchemaTypedValue.IsList => throw new Exception("Untrusted getter read.");
        IEnumerable<SchemaAtomicItem> ISchemaTypedValue.AtomicItems => throw new Exception("Untrusted getter read.");
    }
    /// <summary>Authority authenticates membership and never resolves an anonymous type using a fabricated name.</summary>
    [Fact] public void ExactAuthorityMembership()
    {
        using var f = new Fixture(); var authority = (ISchemaAuthority)f.Endpoint;
        var type = authority.ResolveType(new("http://www.w3.org/2001/XMLSchema", "decimal"), default).Value;
        Assert.True(authority.Owns(type)); Assert.True(authority.IsSameComponent(type, type, default).Value);
        var other = (ISchemaAuthority)new NativeContainerAdapter(Compile()); Assert.False(other.Owns(type));
        Assert.Equal(SchemaResultKind.NotFound, authority.ResolveType(new("", "anonymous"), default).Kind);
        Assert.Equal(SchemaResultKind.NotFound, authority.ResolveElementDeclaration(new("", "amount"), default).Kind);
    }
    /// <summary>Prototype owns only the exact privately issued root object, not descriptive equivalent nodes.</summary>
    [Fact] public void ExactInputRootMembership()
    {
        using var f = new Fixture(); var input = (ISchemaAssessmentInput)f.Input;
        Assert.True(input.OwnsNode(input.Root)); using var other = f.Advisor.Capture(XDocument.Parse("<order/>"));
        Assert.False(input.OwnsNode(((ISchemaAssessmentInput)other).Root));
    }
    /// <summary>Mutable underlying XML reached through the provider projection revokes the snapshot.</summary>
    [Fact] public void ExposedNodeMutationRevokesSnapshot()
    {
        using var f = new Fixture();
        var node = Assert.IsType<Bosak.XPath.Providers.Xml.XDocumentNode>(((ISchemaAssessmentInput)f.Input).Root);
        Assert.IsType<XDocument>(node.UnderlyingObject).Root!.Element("amount")!.Value = "999";
        Assert.Throws<InvalidOperationException>(() => f.Run(op => op.ReadTypedContainer(f.Snapshot, f.Input.Values[0])));
        Assert.False(((ISchemaAssessmentInput)f.Input).OwnsNode(node));
    }
    /// <summary>Native decimal scalar admission executes the public expression with live origin evidence.</summary>
    [Fact] public void NativeScalarPublicExecution()
    {
        using var f = new Fixture();
        f.Run(op =>
        {
            var value = op.ReadTypedValue(f.Snapshot, f.Input.Values[0]).Value;
            Assert.True(SchemaAtomics.TryGetProvenance(value, out var evidence));
            Assert.Same(f.Input.Origin, evidence!.OriginalAssessment);
            Assert.Equal(1.25m, op.Evaluate(new Bosak.XPath.Runtime.Vm.EvaluationContext().WithVariable("value", value)).DecimalValue);
            return 0;
        });
    }
    /// <summary>Public assessment requests remain unsupported rather than publishing a legacy document root.</summary>
    [Fact] public void PublicAssessmentIsExplicitlyUnsupported()
    {
        using var f = new Fixture();
        var request = new SchemaAssessmentRequest(f.Input, SchemaAssessmentMode.Strict, null, null, new Episode());
        var result = ((ISchemaAssessmentAdvisor)f.Advisor).Assess(request, default);
        Assert.Equal(SchemaAssessmentStatus.Unsupported, result.Status); Assert.Null(result.ValidatedRoot); Assert.Null(result.Evidence);
    }
    private sealed class Episode : ISchemaAssessmentEpisode
    {
        DateTimeOffset ISchemaAssessmentEpisode.CurrentDateTime => new(2026, 11, 6, 0, 0, 0, TimeSpan.Zero);
        int ISchemaAssessmentEpisode.ImplicitTimezoneOffsetMinutes => 0;
    }
}
