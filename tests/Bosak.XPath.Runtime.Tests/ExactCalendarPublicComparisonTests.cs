// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Verifies explicit calendar comparison preserves public typed authority boundaries.
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
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Fixture=Bosak.XPath.Runtime.Tests.CalendarTransportSupport.Fixture;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Checks C20 through independently compiled public typed reads and detached comparisons.</summary>
public sealed class ExactCalendarPublicComparisonTests
{
    /// <summary>Equal instants keep separate real type evidence, while comparison outputs grant none.</summary>
    [Fact] public void C20_PublicComparisonDoesNotTransferAuthority()
    {
        using var f=new Fixture();var a=XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse("2000-01-01T00:00:00.123456789Z",XdmExactCalendarKind.DateTime,64,32),"urn:a");
        var b=XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse("2000-01-01T01:00:00.123456789+01:00",XdmExactCalendarKind.DateTimeStamp,64,32),"urn:b");
        var ca=f.Value(a);var cb=f.Value(b);var other=new CalendarTransportSupport.Handle();((CalendarTransportSupport.Authority)f.Assessor.Selected).Types.Add(other);
        cb.Items=[new(b,new CalendarTransportSupport.Provenance(cb,f.Governing,other,f.Origin))];
        var pair=f.Run(op=>(op.ReadTypedValue(f.Snapshot,ca).Value,op.ReadTypedValue(f.Snapshot,cb).Value));
        Assert.True(SchemaAtomics.TryGetProvenance(pair.Item1,out var pa));Assert.True(SchemaAtomics.TryGetProvenance(pair.Item2,out var pb));Assert.Same(f.Actual,pa!.ActualType);Assert.Same(other,pb!.ActualType);
        Assert.Equal(0,XdmExactCalendarComparison.CompareXPath(pair.Item1.ExactCalendarValue,pair.Item2.ExactCalendarValue,0));Assert.True(XdmExactCalendarComparison.SameKey(pair.Item1.ExactCalendarValue,pair.Item2.ExactCalendarValue));Assert.NotEqual(pair.Item1,pair.Item2);
        var plain=XdmValue.FromExactCalendar(pair.Item2.ExactCalendarValue,pair.Item1.UserSchemaTypeName);Assert.False(SchemaAtomics.TryGetProvenance(plain,out _));
        f.Run(op=>{Assert.Throws<ArgumentException>(()=>op.Evaluate(new EvaluationContext().WithVariable("value",plain)));return 0;});
        Assert.Throws<NotSupportedException>(()=>XdmValueEqualityComparer.Instance.GetHashCode(pair.Item1));
        f.Snapshot.Dispose();Assert.False(SchemaAtomics.TryGetProvenance(pair.Item1,out _));Assert.Throws<ObjectDisposedException>(()=>pa.ActualType);Assert.Equal(0,XdmExactCalendarComparison.CompareXPath(pair.Item1.ExactCalendarValue,pair.Item2.ExactCalendarValue,0));
    }
}
