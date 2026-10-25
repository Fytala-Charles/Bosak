// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Verify integer modulo preserves remainder magnitude and type at storage boundaries.
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

/// <summary>Integer modulo must preserve exact remainders and numeric identity across backing stores.</summary>
public sealed class IntegerModuloTests
{
    /// <summary>Provide modulo boundaries, mixed numeric controls and zero-divisor errors.</summary>
    /// <returns>Profile, dividend, divisor, expected report and whether an error is required.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Left, string Right, string Expected, bool Error)[]
        {
            ("xs:integer('-9223372036854775808')", "-1", "0|true", false),
            ("-9223372036854775808", "-1", "0|true", false),
            ("xs:integer('-9223372036854775808')", "1", "0|true", false),
            ("xs:integer('-9223372036854775808')", "3", "-2|true", false),
            ("xs:integer('-9223372036854775808')", "-3", "-2|true", false),
            ("9223372036854775808", "3", "2|true", false),
            ("9223372036854775808", "-3", "2|true", false),
            ("(0 - 9223372036854775808)", "3", "-2|true", false),
            ("9223372036854775808", "9223372036854775809", "9223372036854775808|true", false),
            ("9223372036854775809", "9223372036854775808", "1|true", false),
            ("(0 - 9223372036854775809)", "9223372036854775808", "-1|true", false),
            ("xs:unsignedLong('18446744073709551615')", "2", "1|true", false),
            ("79228162514264337593543950335", "2", "1|true", false),
            ("(0 - 79228162514264337593543950335)", "2", "-1|true", false),
            ("9223372036854775808", "1", "0|true", false),
            ("9223372036854775808", "9223372036854775808", "0|true", false),
            ("5", "9223372036854775808", "5|true", false),
            ("0", "9223372036854775808", "0|true", false),
            ("xs:integer('-9223372036854775808')", "xs:integer('-9223372036854775808')", "0|true", false),
            ("xs:integer('9223372036854775807')", "xs:integer('-9223372036854775808')", "9223372036854775807|true", false),
            ("9223372036854775808", "3.0", "2|false", false),
            ("xs:decimal('-7.5')", "2", "-1.5|false", false),
            ("5e0", "2", "1|false", false),
            ("xs:float('5')", "2", "1|false", false),
            ("9223372036854775808", "0", "FOAR0001", true),
            ("xs:integer('-9223372036854775808')", "0", "FOAR0001", true),
            ("xs:decimal('5')", "0", "FOAR0001", true),
            ("xs:double('5')", "0", "NaN|false", false)
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Left, row.Right, row.Expected, row.Error };
    }

    /// <summary>Direct and inline-function paths preserve integer results without changing decimal/floating policy.</summary>
    /// <param name="profile">The explicit modern profile.</param>
    /// <param name="left">The dividend expression.</param>
    /// <param name="right">The divisor expression.</param>
    /// <param name="expected">The exact value/type report or required error code.</param>
    /// <param name="error">Whether an error is required.</param>
    [Theory, MemberData(nameof(Cases))]
    public void DirectAndRuntimeAgree(XPathCompatibility profile, string left, string right, string expected, bool error)
    {
        foreach (var expression in new[] { "(" + left + ") mod (" + right + ")", "(function($a,$b){$a mod $b})(" + left + "," + right + ")" })
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
