// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Verify unary negation preserves integer magnitude and type at storage boundaries.
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

/// <summary>Unary negation must agree across literal folding and runtime integer promotion.</summary>
public sealed class IntegerUnaryTests
{
    /// <summary>Provide integer boundaries and genuine decimal/floating controls.</summary>
    /// <returns>Profile, literal expression, runtime expression and exact magnitude/type report.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Literal, string Runtime, string Expected)[]
        {
            ("-(-9223372036854775807 - 1)", "-xs:integer('-9223372036854775808')", "9223372036854775808|true"),
            ("-9223372036854775808", "-(xs:integer('9223372036854775807') + 1)", "-9223372036854775808|true"),
            ("-9223372036854775809", "-(xs:integer('9223372036854775807') + 2)", "-9223372036854775809|true"),
            ("-(0 - 9223372036854775809)", "-(0 - (xs:integer('9223372036854775807') + 2))", "9223372036854775809|true"),
            ("-18446744073709551615", "-xs:unsignedLong('18446744073709551615')", "-18446744073709551615|true"),
            ("-79228162514264337593543950335", "(function($a){-$a})(79228162514264337593543950335)", "-79228162514264337593543950335|true"),
            ("-(0 - 79228162514264337593543950335)", "(function($a){-$a})(0 - 79228162514264337593543950335)", "79228162514264337593543950335|true"),
            ("-9223372036854775807", "-xs:integer('9223372036854775807')", "-9223372036854775807|true"),
            ("-0", "-xs:integer('0')", "0|true"),
            ("-5", "-xs:unsignedInt('5')", "-5|true"),
            ("+9223372036854775808", "+(xs:integer('9223372036854775807') + 1)", "9223372036854775808|true"),
            ("-9223372036854775808.0", "-xs:decimal('9223372036854775808')", "-9223372036854775808|false"),
            ("-0.5", "-xs:decimal('0.5')", "-0.5|false"),
            ("-2e0", "-xs:double('2')", "-2|false"),
            ("-2e0", "-xs:float('2')", "-2|false"),
            ("-(9223372036854775807 + 1) + 1", "-(xs:integer('9223372036854775807') + 1) + 1", "-9223372036854775807|true")
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Literal, row.Runtime, row.Expected };
    }

    /// <summary>Literal and runtime negation preserve the required numeric base type and value.</summary>
    /// <param name="profile">The modern compatibility profile.</param>
    /// <param name="literal">The literal expression eligible for optimization.</param>
    /// <param name="runtime">The equivalent expression using runtime operands.</param>
    /// <param name="expected">The exact magnitude and integer identity.</param>
    [Theory, MemberData(nameof(Cases))]
    public void LiteralAndRuntimeAgree(XPathCompatibility profile, string literal, string runtime, string expected)
    {
        foreach (var expression in new[] { literal, runtime })
        {
            var report = "let $v := (" + expression + ") return concat(string($v),'|',$v instance of xs:integer)";
            var compiled = XPath31Expression.Compile(report, new CompileOptions { Compatibility = profile });
            Assert.Equal(expected, compiled.Evaluate(new EvaluationContext()).ToString());
        }
    }
}
