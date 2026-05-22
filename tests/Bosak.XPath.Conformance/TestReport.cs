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
                    break;
            }
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
    }
}
