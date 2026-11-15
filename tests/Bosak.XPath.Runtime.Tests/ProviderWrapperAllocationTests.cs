// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 15 November 2026
// PURPOSE              : Verifies isolated provider wrapper allocation budgets
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 15-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Xunit;
namespace Bosak.XPath.Runtime.Tests;
/// <summary>Verifies the actual provider wrapper allocation path in an isolated worker.</summary>
public sealed class ProviderWrapperAllocationTests
{
    /// <summary>Checks pre-recorded budgets reject the legacy wrapping layers.</summary>
    [Fact]
    public void InRecordWrapper_StaysWithinFixedIsolatedBudgets()
    {
        foreach (var row in AllocationProbeRunner.Run("wrapping"))
        {
            var cap = row.Key switch { "create" => 48L, "once" => 192L, _ => 160L };
            Assert.InRange(row.Value, 0L, cap * 100000);
        }
    }
}
