// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Verifies public typed exact-calendar identity, provenance and lifetime.
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
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Fixture = Bosak.XPath.Runtime.Tests.CalendarTransportSupport.Fixture;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Checks C20 through actual public compilation, reads, identity evaluation and expiry.</summary>
public sealed class ExactCalendarPublicTransportTests
{
    private static XdmValue Value(int family = 0, string zone = "Z", string fraction = "123456789")
    {
        var kind = family == 0 ? XdmExactCalendarKind.DateTime : family == 1 ? XdmExactCalendarKind.Time : XdmExactCalendarKind.DateTimeStamp;
        return XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse((family == 1 ? "" : "2000-01-02T") + "03:04:05." + fraction + zone, kind, 256, 256), "urn:customer:calendar");
    }
    /// <summary>Preserves exact components and actual/declared authority through real scalar identity.</summary>
    /// <param name="family">Calendar family.</param>
    /// <param name="zone">Explicit timezone.</param>
    [Theory] [InlineData(0,"Z")] [InlineData(0,"")] [InlineData(0,"+14:00")] [InlineData(0,"-14:00")] [InlineData(1,"Z")] [InlineData(1,"")] [InlineData(1,"+14:00")] [InlineData(2,"Z")] [InlineData(2,"-14:00")]
    public void C20_PublicScalarIdentity(int family, string zone)
    {
        using var f = new Fixture(); var plain = Value(family,zone); var c = f.Value(plain);
        Assert.False(SchemaAtomics.TryGetProvenance(plain,out _));
        var atom = f.Run(op => { var read = op.ReadTypedValue(f.Snapshot,c).Value; return op.Evaluate(new EvaluationContext().WithVariable("value",read)); });
        Assert.Equal(plain,atom); Assert.Equal(plain.ExactCalendarValue,atom.ExactCalendarValue); Assert.Equal(plain.ToString(),atom.ToString()); Assert.Equal(plain.HasTimezone,atom.HasTimezone);
        Assert.True(SchemaAtomics.TryGetProvenance(atom,out var provenance)); Assert.Same(f.Governing,provenance!.DeclaredType); Assert.Same(f.Actual,provenance.ActualType); Assert.Same(f.Origin,provenance.OriginalAssessment);
        f.Run(op=>{Assert.Equal(atom,op.Evaluate(new EvaluationContext().WithVariable("value",(XdmValue)(object)atom)));return 0;});
        f.Snapshot.Dispose(); Assert.False(SchemaAtomics.TryGetProvenance(atom,out _)); Assert.Throws<ObjectDisposedException>(()=>provenance.ActualType); Assert.Equal(plain.ToString(),atom.ToString());
    }
    /// <summary>Complete containers preserve ordering and per-atom provenance without manufacturing plain-value authority.</summary>
    [Fact] public void C20_CompleteContainerAndExpiry()
    {
        using var f=new Fixture();var plain=Value();var c=f.Value(plain,2);SchemaAtomicItem retained=default;
        f.Run(op=>{var result=op.ReadTypedContainer(f.Snapshot,c).Value;var items=result.AtomicItems.ToArray();Assert.Equal(2,items.Length);Assert.Equal(plain,items[0].Value);Assert.Same(f.Actual,items[1].TypeEvidence.ActualType);retained=items[0];return 0;});
        Assert.Equal(plain,retained.Value);Assert.False(SchemaAtomics.TryGetProvenance(retained.Value,out _));Assert.Throws<ObjectDisposedException>(()=>retained.TypeEvidence.ActualType);
    }
    /// <summary>Plain annotated copies cannot replace an issuer-admitted exact atomic value.</summary>
    [Fact] public void C20_PlainCopyCannotBorrowAuthority()
    {
        using var f=new Fixture();var plain=Value();var c=f.Value(plain);
        f.Run(op=>{var actual=op.ReadTypedValue(f.Snapshot,c).Value;Assert.Throws<ArgumentException>(()=>op.Evaluate(new EvaluationContext().WithVariable("value",plain)));Assert.Equal(actual,op.Evaluate(new EvaluationContext().WithVariable("value",actual)));return 0;});
    }
    /// <summary>Normalized stored fractional coefficient and scale obey receiving query capacities.</summary>
    /// <param name="digits">Receiving digit limit.</param>
    /// <param name="scale">Receiving scale limit.</param>
    [Theory] [InlineData(9,64)] [InlineData(64,8)]
    public void C20_RepresentationCapacityRefusalAndRecovery(int digits,int scale)
    {
        using var f=new Fixture(cache:1,digits:digits,scale:scale);var tooLarge=f.Value(Value());var small=f.Value(Value(fraction:"1"));
        f.Run(op=>{Assert.Throws<NotSupportedException>(()=>op.ReadTypedValue(f.Snapshot,tooLarge));Assert.Equal(small.Items[0].Value,op.ReadTypedValue(f.Snapshot,small).Value);return 0;});
    }
    /// <summary>Cache quota and cancellation failures leave valid exact values reusable.</summary>
    [Fact] public void C20_CancellationAndCacheRecovery()
    {
        using var f=new Fixture(cache:1);var a=f.Value(Value());var b=f.Value(Value(fraction:"123456788"));using var token=new CancellationTokenSource();token.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(()=>f.Run(op=>op.ReadTypedValue(f.Snapshot,a),token.Token));
        f.Run(op=>{Assert.Equal(a.Items[0].Value,op.ReadTypedValue(f.Snapshot,a).Value);Assert.Throws<NotSupportedException>(()=>op.ReadTypedValue(f.Snapshot,b));return 0;});
    }
    /// <summary>Session closure revokes observation and further typed identity evaluation.</summary>
    [Fact] public void C20_SessionExpiry()
    {
        var f=new Fixture();var c=f.Value(Value());var atom=f.Run(op=>op.ReadTypedValue(f.Snapshot,c).Value);Assert.True(SchemaAtomics.TryGetProvenance(atom,out var p));f.Dispose();Assert.False(SchemaAtomics.TryGetProvenance(atom,out _));Assert.Throws<ObjectDisposedException>(()=>p!.DeclaredType);
    }
    /// <summary>Removing actual type membership prevents a new typed read from creating authority.</summary>
    [Fact] public void C20_TypeMembershipRevocation()
    {
        using var f=new Fixture();var c=f.Value(Value());((CalendarTransportSupport.Authority)f.Assessor.Selected).Types.Remove(f.Actual);
        Assert.Throws<ArgumentException>(()=>f.Run(op=>op.ReadTypedValue(f.Snapshot,c)));
    }
}
