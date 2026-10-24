// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verify folded and runtime integer arithmetic preserve exact magnitude and type.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Integer arithmetic must agree across constant folding and runtime decimal promotion.</summary>
public sealed class IntegerPromotionTests
{
    /// <summary>Provide arithmetic boundaries under all supported modern profiles.</summary>
    /// <returns>Profile, folded expression, runtime expression and expected value/type.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Folded, string Runtime, string Expected)[]
        {
            ("9223372036854775807 + 1", "xs:integer('9223372036854775807') + 1", "9223372036854775808|true"),
            ("-9223372036854775807 - 2", "xs:integer('-9223372036854775807') - 2", "-9223372036854775809|true"),
            ("9223372036854775807 * 2", "xs:integer('9223372036854775807') * 2", "18446744073709551614|true"),
            ("0 - (-9223372036854775807 - 1)", "0 - xs:integer('-9223372036854775808')", "9223372036854775808|true"),
            ("(-9223372036854775807 - 1) - (-9223372036854775807 - 1)", "xs:integer('-9223372036854775808') - xs:integer('-9223372036854775808')", "0|true"),
            ("9223372036854775808 + 9223372036854775809", "(function($a,$b){$a+$b})(9223372036854775808,9223372036854775809)", "18446744073709551617|true"),
            ("9223372036854775808 - 9223372036854775809", "(function($a,$b){$a - $b})(9223372036854775808,9223372036854775809)", "-1|true"),
            ("9223372036854775808 * 2", "(function($a){$a*2})(9223372036854775808)", "18446744073709551616|true"),
            ("(9223372036854775807 + 1) + 1", "(xs:integer('9223372036854775807') + 1) + 1", "9223372036854775809|true"),
            ("(9223372036854775807 + 1) - 1", "(xs:integer('9223372036854775807') + 1) - 1", "9223372036854775807|true"),
            ("9223372036854775808 + 0.5", "(function($a){$a+0.5})(9223372036854775808)", "9223372036854775808.5|false"),
            ("1.0 + 2.0", "xs:decimal('1.0') + xs:decimal('2.0')", "3|false"),
            ("2 + 3", "xs:integer('2') + 3", "5|true"),
            ("2 * 0", "xs:integer('2') * 0", "0|true")
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Folded, row.Runtime, row.Expected };
    }

    /// <summary>Both evaluation paths preserve integer identity while genuine decimals remain decimal.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    /// <param name="folded">An expression eligible for constant folding.</param>
    /// <param name="runtime">An equivalent expression evaluated with runtime operands.</param>
    /// <param name="expected">Exact magnitude and integer type test.</param>
    [Theory, MemberData(nameof(Cases))]
    public void FoldedAndRuntimeAgree(XPathCompatibility profile, string folded, string runtime, string expected)
    {
        foreach (var expression in new[] { folded, runtime })
        {
            var report = "let $v := (" + expression + ") return concat(string($v),'|',$v instance of xs:integer)";
            var compiled = XPath31Expression.Compile(report, new CompileOptions { Compatibility = profile });
            Assert.Equal(expected, compiled.Evaluate(new EvaluationContext()).ToString());
        }
    }
}
