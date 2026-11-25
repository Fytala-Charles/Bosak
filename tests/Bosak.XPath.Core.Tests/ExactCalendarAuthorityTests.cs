// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Verifies exact calendar evidence binding and issuer lifetime boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.XPath.Core.Tests;

/// <summary>Checks private authority transport contracts C13 through C19.</summary>
public sealed class ExactCalendarAuthorityTests
{
    private static XdmValue V(string seconds = "05.123456789", string zone = "Z", string? user = null) => XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse("2000-01-02T03:04:" + seconds + zone, XdmExactCalendarKind.DateTime, 256, 256), user);
    private sealed class Issuer : ISchemaEvidenceIssuer
    {
        internal readonly object Type = new(), Origin = new();
        private readonly HashSet<ISchemaIssuedScalar> _values = new(ReferenceEqualityComparer.Instance);
        internal ISchemaIssuedScalar Issue(XdmValue value) { var s = new Scalar(value, Type, Origin); _values.Add(s); return s; }
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value) => _values.Contains(value);
        bool ISchemaEvidenceIssuer.OwnsType(object type) => ReferenceEquals(type, Type);
        bool ISchemaEvidenceIssuer.OwnsAssessment(object origin) => ReferenceEquals(origin, Origin);
        bool ISchemaEvidenceIssuer.IsSameType(object a, object b) => ReferenceEquals(a,b);
        private sealed record Scalar(XdmValue Value, object DeclaredType, object OriginalAssessment) : ISchemaIssuedScalar
        { /// <inheritdoc/>
            public object ActualType => DeclaredType; }
    }
    /// <summary>Matches exact normalized components while separating legacy representation.</summary>
    [Fact] public void C13_ExactRepresentationMatch()
    {
        Assert.True(V().SchemaTransportMatches(V())); Assert.True(V("05.1234567890").SchemaTransportMatches(V()));
        Assert.False(V().SchemaTransportMatches(V("05.123456788")));
        Assert.False(V("05.123").SchemaTransportMatches(XdmValue.FromDateTime(new DateTimeOffset(2000,1,2,3,4,5,123,TimeSpan.Zero))));
    }
    /// <summary>Rejects changed submillisecond values behind a genuine existing marker.</summary>
    [Fact] public void C14_FractionMarkerRebindRefuses()
    {
        var issuer = new Issuer(); var env = new object(); var kernel = new SchemaEvidenceKernel(env,issuer,1,4);
        using var snapshot = kernel.Open(env,new object(),issuer.Origin,()=>{},default);
        var original = kernel.AttachValue(kernel.Read(snapshot,issuer.Issue(V()),default));
        var changed = XdmValue.SchemaTransportAttach(V("05.123456788"), XdmValue.SchemaTransportMarker(original)!);
        Assert.Throws<ArgumentException>(()=>kernel.ReadValue(changed)); Assert.NotNull(kernel.ReadValue(original));
    }
    /// <summary>Rejects timezone and descriptive annotation changes behind a live marker.</summary>
    /// <param name="kind">Changed representation field.</param>
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void C15_MetadataMarkerRebindRefuses(int kind)
    {
        var issuer = new Issuer(); var env = new object(); var kernel = new SchemaEvidenceKernel(env,issuer,1,4);
        using var snapshot = kernel.Open(env,new object(),issuer.Origin,()=>{},default);
        var original = kernel.AttachValue(kernel.Read(snapshot,issuer.Issue(V()),default));
        var changed = kind switch { 0=>V(zone:""),1=>V(zone:"+01:00"),2=>V(user:"urn:forged"),_=>XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse("2000-01-02T03:04:05.123456789Z",XdmExactCalendarKind.DateTimeStamp,256,256)) };
        Assert.Throws<ArgumentException>(()=>kernel.ReadValue(XdmValue.SchemaTransportAttach(changed,XdmValue.SchemaTransportMarker(original)!)));
    }
    /// <summary>Attachment is payload-transparent; detachment cannot preserve execution authority.</summary>
    [Fact] public void C16_AttachDetachIdentity()
    {
        var value=V();var marker=new object();var attached=XdmValue.SchemaTransportAttach(value,marker);
        Assert.Equal(value,attached);Assert.Equal(value.GetHashCode(),attached.GetHashCode());Assert.True(attached.IsExactCalendar);Assert.Equal(value.ExactCalendarValue,attached.ExactCalendarValue);
        var plain=XdmValue.WithoutSchemaEvidence(attached);Assert.Null(XdmValue.SchemaTransportMarker(plain));Assert.Equal(value,plain);
    }
    /// <summary>Only admitted issuers and origin/type memberships establish evidence.</summary>
    [Fact] public void C17_IssuerAdmission()
    {
        var issuer=new Issuer();var foreign=new Issuer();var env=new object();var kernel=new SchemaEvidenceKernel(env,issuer,1,4);
        using var snapshot=kernel.Open(env,new object(),issuer.Origin,()=>{},default);
        Assert.Throws<ArgumentException>(()=>kernel.Read(snapshot,foreign.Issue(V()),default));
        var e=kernel.Read(snapshot,issuer.Issue(V()),default);Assert.Same(issuer.Type,e.ActualType);Assert.Same(issuer.Origin,e.OriginalAssessment);Assert.Equal(V(),e.Value);
    }
    /// <summary>Closing the snapshot revokes attached authority without corrupting detached data.</summary>
    [Fact] public void C18_SnapshotExpiry()
    {
        var issuer=new Issuer();var env=new object();var kernel=new SchemaEvidenceKernel(env,issuer,1,4);int released=0;
        var snapshot=kernel.Open(env,new object(),issuer.Origin,()=>released++,default);var e=kernel.Read(snapshot,issuer.Issue(V()),default);var atom=kernel.AttachValue(e);
        snapshot.Dispose();Assert.Equal(1,released);Assert.Throws<ObjectDisposedException>(()=>kernel.ReadValue(atom));Assert.Equal(V().ToString(),atom.ToString());
    }
    /// <summary>Cancellation and admission quotas refuse without consuming available capacity.</summary>
    [Fact] public void C19_CancellationAndQuotas()
    {
        var issuer=new Issuer();var env=new object();var kernel=new SchemaEvidenceKernel(env,issuer,1,1);using var snapshot=kernel.Open(env,new object(),issuer.Origin,()=>{},default);
        using var cancel=new CancellationTokenSource();cancel.Cancel();Assert.ThrowsAny<OperationCanceledException>(()=>kernel.Read(snapshot,issuer.Issue(V()),cancel.Token));
        var scalar=issuer.Issue(V());Assert.Same(kernel.Read(snapshot,scalar,default),kernel.Read(snapshot,scalar,default));
        Assert.Throws<InvalidOperationException>(()=>kernel.Read(snapshot,issuer.Issue(V("05.123456788")),default));
    }
}
