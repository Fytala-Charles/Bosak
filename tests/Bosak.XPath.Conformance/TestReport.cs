// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 mei 2026
// PURPOSE              : Collects and prints conformance test results.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 15-07-2026     | Skip-reason tracking with grouped summary (harness-error collapse)                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 18-08-2026     | Per-test skip name recording + DumpSkips for skip-cluster analysis                      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Conformance;

internal sealed class TestReport
{
    public int Passed { get; private set; }
    public int Failed { get; private set; }
    public int Skipped { get; private set; }
    public int Total => Passed + Failed + Skipped;

    private readonly List<(string Name, TestOutcomeKind Kind, string? Reason)> _failures = new();
    private readonly Dictionary<string, int> _skipReasons = new(StringComparer.Ordinal);
    private readonly List<(string Name, string Reason)> _skipped = new();
    private readonly object _lock = new();

    public void Record(string name, TestOutcomeKind kind, string? message)
    {
        lock (_lock)
        {
            switch (kind)
            {
                case TestOutcomeKind.Passed:
                    Passed++;
                    break;
                case TestOutcomeKind.Failed:
                    Failed++;
                    _failures.Add((name, kind, message));
                    break;
                case TestOutcomeKind.Skipped:
                    Skipped++;
                    if (message is not null)
                    {
                        _skipReasons[message] = _skipReasons.TryGetValue(message, out int n) ? n + 1 : 1;
                        _skipped.Add((name, message));
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Writes every skipped test name grouped by skip reason to the given path.
    /// </summary>
    public void DumpSkips(string path)
    {
        using var writer = new StreamWriter(path);
        foreach (var group in _skipped.GroupBy(s => s.Reason).OrderByDescending(g => g.Count()))
        {
            writer.WriteLine($"=== {group.Count()} skipped: {group.Key} ===");
            foreach (var (name, _) in group.OrderBy(s => s.Name))
                writer.WriteLine(name);
            writer.WriteLine();
        }
    }

    public void PrintSummary()
    {
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine("Conformance Test Results");
        Console.WriteLine("============================================================");
        Console.WriteLine($"Total:   {Total}");
        Console.WriteLine($"Passed:  {Passed}");
        Console.WriteLine($"Failed:  {Failed}");
        Console.WriteLine($"Skipped: {Skipped}");
        Console.WriteLine();

        if (Total > 0)
        {
            double passRate = (double)Passed / Total * 100;
            Console.WriteLine($"Pass rate: {passRate:F2}%");
        }

        if (_failures.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("First 20 failures:");
            foreach (var (name, _, reason) in _failures)
            {
                Console.WriteLine($"  FAIL {name}: {reason}");
            }

        }

        if (_skipReasons.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Skip reasons (grouped):");
            var grouped = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var (reason, count) in _skipReasons)
            {
                // Collapse per-test "Harness error: TypeName: message" into the type name.
                string key = reason.StartsWith("Harness error: ", StringComparison.Ordinal)
                    ? reason[..(reason.IndexOf(':', "Harness error: ".Length) is int idx && idx > 0 ? idx : reason.Length)]
                    : reason;
                grouped[key] = grouped.TryGetValue(key, out int n) ? n + count : count;
            }
            foreach (var (reason, count) in grouped.OrderByDescending(kv => kv.Value).Take(40))
                Console.WriteLine($"  {count,5}  {reason}");
        }
    }
}
