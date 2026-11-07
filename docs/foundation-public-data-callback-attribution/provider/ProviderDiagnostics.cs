// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 November 2026
// PURPOSE              : Counts serial provider boundary entries without suppressing existing checks.
// SPECIAL NOTES        : Isolated provider/consumer qualification; not a production or published API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-11-2026     | Creation; finite observational public consumer workload with caught diagnostic faults.                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.Schema.ContainerPrototype;

/// <summary>Fixed thread-local counters for isolated serial provider boundary attribution.</summary>
public static class ProviderDiagnostics
{
    private static readonly string[] Names = ["Assessor.Authority", "Advisor.Assessor", "Advisor.Environment", "Advisor.Availability", "Advisor.OwnsOrigin", "Advisor.OwnsFacts", "Advisor.OwnsNode", "Advisor.OwnsContainer", "Output.Availability", "Input.OwnsNode", "Authority.OwnsComponent", "Output.LeaseCheck", "Facts.Type", "Facts.Declaration", "Facts.Origin", "Facts.HasNoTypedValue", "Facts.IsNilled", "Facts.ReadTypedValue"];
    [ThreadStatic] private static long[]? _counts;
    internal static void Hit(int index) => (_counts ??= new long[Names.Length])[index]++;
    internal static T Observe<T>(int index, T value) { Hit(index); return value; }
    /// <summary>Clears counters for the calling thread before a measured diagnostic phase.</summary>
    public static void Reset() => Array.Clear(_counts ??= new long[Names.Length]);
    /// <summary>Copies the calling thread's counts; no provider resources or ownership are transferred.</summary>
    /// <returns>A detached name/count mapping.</returns>
    public static IReadOnlyDictionary<string, long> Snapshot()
    {
        var counts = _counts ??= new long[Names.Length];
        return Enumerable.Range(0, Names.Length).ToDictionary(i => Names[i], i => counts[i]);
    }
}
