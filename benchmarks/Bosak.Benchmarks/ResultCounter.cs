// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 September 2026
// PURPOSE              : Helpers that force full materialization of XDM results in benchmarks.
// SPECIAL NOTES        : Benchmark harness for the Bosak XPath 3.1 implementation; not shipped.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;

namespace Bosak.Benchmarks;

/// <summary>
/// Consumes XDM results so that benchmarked evaluations cannot leave work undone behind
/// a lazy sequence: every item is enumerated and counted.
/// </summary>
internal static class ResultCounter
{
    /// <summary>Evaluates a compiled expression and counts the resulting node sequence.</summary>
    /// <param name="expression">The compiled XPath expression.</param>
    /// <param name="contextItem">The context document node.</param>
    /// <returns>The number of items in the result sequence.</returns>
    public static int CountNodes(XPath31Expression expression, IXdmNode contextItem)
        => Count(expression.EvaluateNodes(contextItem));

    /// <summary>Counts the items of an arbitrary XDM value (singletons count as one).</summary>
    /// <param name="value">The value to count.</param>
    /// <returns>The number of items in the value.</returns>
    public static int CountItems(XdmValue value)
    {
        if (value.IsSequence && value.SequenceValue is not null)
            return Count(XdmSequence.FromSource(value.SequenceValue));
        return value.IsUndefined ? 0 : 1;
    }

    private static int Count(XdmSequence sequence)
    {
        if (sequence.TryGetLength(out var length))
            return length;
        var count = 0;
        foreach (var _ in sequence)
            count++;
        return count;
    }
}
