// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Verifies calendar interoperability refuses live and expired authority carriers.
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
using Fixture = Bosak.XPath.Runtime.Tests.CalendarTransportSupport.Fixture;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Checks plain calendar interoperability cannot accept or recreate admitted authority.</summary>
public sealed class CalendarInteropPublicTests
{
    /// <summary>Checks C20 live and expired authority carriers refuse without consulting providers.</summary>
    /// <param name="exact">Whether the supplied carrier has an exact payload.</param>
    [Theory] [InlineData(true)] [InlineData(false)]
    public void C20_AuthorityCarriersRefuse(bool exact)
    {
        using var f = new Fixture();
        var plain = exact
            ? XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse("2000-01-02T03:04:05.123Z",XdmExactCalendarKind.DateTime,64,32))
            : XdmValue.FromDateTime(new DateTimeOffset(2000,1,2,3,4,5,123,TimeSpan.Zero));
        var container=f.Value(plain);var admitted=f.Run(op=>op.ReadTypedValue(f.Snapshot,container).Value);
        Assert.True(SchemaAtomics.TryGetProvenance(admitted,out _));
        var checks=f.Input.Checks;
        Assert.Throws<NotSupportedException>(()=>XdmCalendarInterop.Read(admitted));
        Assert.Throws<NotSupportedException>(()=>XdmCalendarInterop.CompareXPath(admitted,plain,0));
        Assert.Throws<NotSupportedException>(()=>XdmCalendarInterop.CompareXsd(plain,admitted));
        Assert.Throws<NotSupportedException>(()=>XdmCalendarInterop.SameKey(plain,admitted));
        Assert.Throws<NotSupportedException>(()=>XdmCalendarInterop.GetSameKeyHashCode(admitted));
        Assert.Equal(checks,f.Input.Checks);
        var reconstructed=XdmValue.FromExactCalendar(XdmCalendarInterop.Read(plain));
        Assert.False(SchemaAtomics.TryGetProvenance(reconstructed,out _));
        f.Run(op=>{Assert.Throws<ArgumentException>(()=>op.Evaluate(new EvaluationContext().WithVariable("value",reconstructed)));return 0;});
        f.Snapshot.Dispose();checks=f.Input.Checks;
        Assert.Throws<NotSupportedException>(()=>XdmCalendarInterop.Read(admitted));
        Assert.Throws<NotSupportedException>(()=>XdmCalendarInteropKeyComparer.Instance.GetHashCode(admitted));
        Assert.Equal(checks,f.Input.Checks);
        Assert.Equal(0,XdmCalendarInterop.CompareXPath(plain,reconstructed,0));
    }
}
