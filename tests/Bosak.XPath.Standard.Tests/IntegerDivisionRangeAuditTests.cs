// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Verify the existing idiv result range separately from quotient precision diagnostics.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Integer division keeps its existing Int64 range while precision is investigated separately.</summary>
public sealed class IntegerDivisionRangeAuditTests
{
    /// <summary>Provide range endpoints, truncation and exact floating thresholds.</summary>
    /// <returns>Profile, expression, expected value/type or error code and error flag.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Expression, string Expected, bool Error)[]
        {
            ("xs:decimal('9223372036854775807') idiv 1", "9223372036854775807|true", false),
            ("xs:decimal('-9223372036854775808') idiv 1", "-9223372036854775808|true", false),
            ("xs:decimal('9223372036854775807.9') idiv 1", "9223372036854775807|true", false),
            ("xs:decimal('-9223372036854775808.9') idiv 1", "-9223372036854775808|true", false),
            ("xs:decimal('9223372036854775808') idiv 1", "FOAR0002", true),
            ("xs:decimal('-9223372036854775809') idiv 1", "FOAR0002", true),
            ("xs:decimal('9223372036854775807') idiv -1", "-9223372036854775807|true", false),
            ("xs:decimal('-9223372036854775808') idiv -1", "FOAR0002", true),
            ("9223372036854775808 idiv 2", "4611686018427387904|true", false),
            ("xs:decimal('0') idiv xs:decimal('0.0000000000000000000000000001')", "0|true", false),
            ("xs:double('9223372036854775808') idiv 1", "FOAR0002", true),
            ("xs:float('9223372036854775808') idiv 1", "FOAR0002", true),
            ("xs:double('-9223372036854775808') idiv 1", "-9223372036854775808|true", false),
            ("xs:float('-9223372036854775808') idiv 1", "-9223372036854775808|true", false),
            ("xs:double('9223372036854774784') idiv 1", "9223372036854774784|true", false),
            ("xs:float('9223371487098961920') idiv 1", "9223371487098961920|true", false)
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Expression, row.Expected, row.Error };
    }

    /// <summary>Keep supported result-range decisions explicit without asserting precision discrepancies as correct.</summary>
    /// <param name="profile">The modern compatibility profile.</param>
    /// <param name="expression">The division expression.</param>
    /// <param name="expected">The exact value/type report or required error code.</param>
    /// <param name="error">Whether an error is required.</param>
    [Theory, MemberData(nameof(Cases))]
    public void ExistingRangeControls(XPathCompatibility profile, string expression, string expected, bool error)
    {
        var report = "let $v := (" + expression + ") return concat(string($v),'|',$v instance of xs:integer)";
        string Evaluate() => XPath31Expression.Compile(report, new CompileOptions { Compatibility = profile }).Evaluate(new EvaluationContext()).ToString();
        if (error)
            Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate()).Message);
        else
            Assert.Equal(expected, Evaluate());
    }
}
