// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 November 2026
// PURPOSE              : Verifies captured issuer identity guards and provider callback postconditions.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-11-2026     | Creation; captured-identity getter patch regression controls (1.81.1).                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Runtime.Vm;
using Xunit;
using static Bosak.XPath.Runtime.Tests.QueryTypedContainerTests;

namespace Bosak.XPath.Runtime.Tests;

/// <summary>Qualifies captured issuer identity access without bypassing provider callback postconditions.</summary>
public sealed partial class QueryNodeFactsTests
{
    /// <summary>Successful original typed-container staging retains actual authority and payload.</summary>
    [Fact] public void CapturedIdentityKeepsOwnedTypedContainer()
    {
        using var f = new Fixture();
        f.Run(op => { var facts = f.Read(op); Assert.Same(f.Type, facts.TypedValue.GoverningType); Assert.Equal(1, facts.TypedValue.GetAtomic(0).Value.IntegerValue); return 0; });
    }
    /// <summary>Mutation during a real provider getter refuses before the next typed member is read.</summary>
    /// <param name="mutation">Authority, tuple ownership or raw session revocation.</param>
    [Theory][InlineData("authority")][InlineData("tuple")][InlineData("close")]
    public void CapturedIdentityKeepsCallbackPostconditions(string mutation)
    {
        using var f = new Fixture();
        f.Facts.Value!.OnRead = stage =>
        {
            if (stage != "governing") return;
            if (mutation == "authority") f.Issuer.Selected = new Authority();
            else if (mutation == "tuple") f.Issuer.TupleOwned = false;
            else f.Session.Dispose();
        };
        f.Run(op =>
        {
            if (mutation == "close") Assert.Throws<ObjectDisposedException>(() => f.Read(op));
            else Assert.Throws<ArgumentException>(() => f.Read(op));
            Assert.DoesNotContain("list", f.Facts.Value.Reads); return 0;
        });
    }
}
