// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Verify exact integer division truncation while retaining the existing result range.
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

/// <summary>Decimal-backed idiv must truncate the exact quotient before applying existing Int64 bounds.</summary>
public sealed class ExactIntegerDivisionTests
{
    /// <summary>Provide exact-reference cases, range refusals, scale extremes and dispatch controls.</summary>
    /// <returns>Profile, dividend, divisor, expected report or error and error flag.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Left, string Right, string Expected, bool Error)[]
        {
            ("(79228162514264337593543950334)", "(79228162514264337593543950335)", "0|true", false),
            ("(-79228162514264337593543950334)", "(79228162514264337593543950335)", "0|true", false),
            ("(79228162514264337593543950334)", "(-79228162514264337593543950335)", "0|true", false),
            ("(-79228162514264337593543950334)", "(-79228162514264337593543950335)", "0|true", false),
            ("(79228162514264337593543950333)", "(39614081257132168796771975167)", "1|true", false),
            ("(-79228162514264337593543950333)", "(39614081257132168796771975167)", "-1|true", false),
            ("(79228162514264337593543950333)", "(-39614081257132168796771975167)", "-1|true", false),
            ("(-79228162514264337593543950333)", "(-39614081257132168796771975167)", "1|true", false),
            ("(79228162514264337593543950334)", "(26409387504754779197847983445)", "2|true", false),
            ("(-79228162514264337593543950334)", "(26409387504754779197847983445)", "-2|true", false),
            ("(79228162514264337593543950334)", "(-26409387504754779197847983445)", "-2|true", false),
            ("(-79228162514264337593543950334)", "(-26409387504754779197847983445)", "2|true", false),
            ("(9223372036854775808)", "(1)", "FOAR0002", true),
            ("(-9223372036854775808)", "(1)", "-9223372036854775808|true", false),
            ("(9223372036854775808)", "(-1)", "-9223372036854775808|true", false),
            ("(-9223372036854775808)", "(-1)", "FOAR0002", true),
            ("(9223372036854775808)", "(2)", "4611686018427387904|true", false),
            ("(-9223372036854775808)", "(2)", "-4611686018427387904|true", false),
            ("(9223372036854775808)", "(-2)", "-4611686018427387904|true", false),
            ("(-9223372036854775808)", "(-2)", "4611686018427387904|true", false),
            ("xs:decimal('7.9228162514264337593543950334')", "xs:decimal('7.9228162514264337593543950335')", "0|true", false),
            ("xs:decimal('-7.9228162514264337593543950334')", "xs:decimal('7.9228162514264337593543950335')", "0|true", false),
            ("xs:decimal('7.9228162514264337593543950334')", "xs:decimal('-7.9228162514264337593543950335')", "0|true", false),
            ("xs:decimal('-7.9228162514264337593543950334')", "xs:decimal('-7.9228162514264337593543950335')", "0|true", false),
            ("xs:decimal('9223372036854775807.9')", "xs:decimal('1')", "9223372036854775807|true", false),
            ("xs:decimal('-9223372036854775807.9')", "xs:decimal('1')", "-9223372036854775807|true", false),
            ("xs:decimal('9223372036854775807.9')", "xs:decimal('-1')", "-9223372036854775807|true", false),
            ("xs:decimal('-9223372036854775807.9')", "xs:decimal('-1')", "9223372036854775807|true", false),
            ("xs:decimal('9223372036854775808.9')", "xs:decimal('1')", "FOAR0002", true),
            ("xs:decimal('-9223372036854775808.9')", "xs:decimal('1')", "-9223372036854775808|true", false),
            ("xs:decimal('9223372036854775808.9')", "xs:decimal('-1')", "-9223372036854775808|true", false),
            ("xs:decimal('-9223372036854775808.9')", "xs:decimal('-1')", "FOAR0002", true),
            ("xs:decimal('1')", "xs:decimal('0.0000000000000000000000000001')", "FOAR0002", true),
            ("xs:decimal('0.0000000000000000000000000001')", "1", "0|true", false),
            ("-xs:decimal('0.0000000000000000000000000001')", "1", "0|true", false),
            ("xs:decimal('1.0000000000000000000000000000')", "1", "1|true", false),
            ("xs:decimal('-0')", "xs:decimal('0.0000000000000000000000000001')", "0|true", false),
            ("79228162514264337593543950335", "79228162514264337593543950335", "1|true", false),
            ("xs:decimal('1')", "0", "FOAR0001", true),
            ("xs:decimal('0')", "0", "FOAR0001", true),
            ("xs:float('2.5')", "1", "2|true", false),
            ("xs:double('-2.5')", "1", "-2|true", false),
            ("xs:untypedAtomic('2.5')", "1", "2|true", false),
            ("xs:integer('-9223372036854775808')", "-1", "FOAR0002", true),
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Left, row.Right, row.Expected, row.Error };
    }

    /// <summary>Direct and runtime operands produce exact truncation with unchanged result-range and error policy.</summary>
    /// <param name="profile">The modern compatibility profile.</param>
    /// <param name="left">The dividend expression.</param>
    /// <param name="right">The divisor expression.</param>
    /// <param name="expected">The exact value/type report or required error code.</param>
    /// <param name="error">Whether an error is required.</param>
    [Theory, MemberData(nameof(Cases))]
    public void ExactQuotientWithBoundedResult(XPathCompatibility profile, string left, string right, string expected, bool error)
    {
        foreach (var expression in new[] { "(" + left + ") idiv (" + right + ")", "(function($a,$b){$a idiv $b})(" + left + "," + right + ")" })
        {
            var report = "let $v := (" + expression + ") return concat(string($v),'|',$v instance of xs:integer)";
            string Evaluate() => XPath31Expression.Compile(report, new CompileOptions { Compatibility = profile }).Evaluate(new EvaluationContext()).ToString();
            if (error)
                Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate()).Message);
            else
                Assert.Equal(expected, Evaluate());
        }
    }
}
