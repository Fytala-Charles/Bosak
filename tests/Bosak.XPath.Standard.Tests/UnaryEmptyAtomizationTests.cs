// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Verifies unary operators handle empty results after array atomization.
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

/// <summary>Verifies unary operators handle empty results after array atomization.</summary>
public sealed class UnaryEmptyAtomizationTests
{
    private static readonly XPathCompatibility[] Profiles =
        [XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental];

    /// <summary>Provides empty sequences and arrays whose atomized value is empty.</summary>
    /// <returns>The language profile and input expression.</returns>
    public static IEnumerable<object[]> EmptyCases()
    {
        foreach (var profile in Profiles)
            foreach (var operand in new[] { "()", "[]", "[()]", "[[]]", "[[()]]", "array { () }", "[(),()]", "[[[]]]" })
                yield return new object[] { profile, operand };
    }

    /// <summary>Preserves empty results for direct, runtime and composed unary operators.</summary>
    /// <param name="profile">The modern language profile.</param>
    /// <param name="operand">An input whose atomized value is empty.</param>
    [Theory, MemberData(nameof(EmptyCases))]
    public void EmptyAfterAtomizationRemainsEmpty(XPathCompatibility profile, string operand)
    {
        foreach (var signs in new[] { "-", "+", "--", "+-", "-+" })
            foreach (var expression in new[] { signs + "(" + operand + ")", "(function($x){" + signs + "$x})(" + operand + ")" })
                Assert.Equal("true", Evaluate("empty(" + expression + ")", profile));
    }

    /// <summary>Provides singleton numeric/atomized controls with both signs.</summary>
    /// <returns>Profile, operand, positive report and negative report.</returns>
    public static IEnumerable<object[]> ValueCases()
    {
        var rows = new (string Operand, string Positive, string Negative)[]
        {
            ("[2]", "2|integer", "-2|integer"),
            ("[[2]]", "2|integer", "-2|integer"),
            ("[xs:untypedAtomic('2.5')]", "2.5|double", "-2.5|double"),
            ("parse-xml('<n>3</n>')/n", "3|double", "-3|double"),
            ("9223372036854775808", "9223372036854775808|integer", "-9223372036854775808|integer"),
            ("xs:decimal('2.5')", "2.5|decimal", "-2.5|decimal")
        };
        foreach (var profile in Profiles)
            foreach (var row in rows)
                yield return new object[] { profile, row.Operand, row.Positive, row.Negative };
    }

    /// <summary>Retains nonempty unary magnitude and type through direct and runtime operands.</summary>
    /// <param name="profile">The modern language profile.</param>
    /// <param name="operand">The singleton input.</param>
    /// <param name="positive">The positive value/type report.</param>
    /// <param name="negative">The negated value/type report.</param>
    [Theory, MemberData(nameof(ValueCases))]
    public void NonemptyValuesRemainNumeric(XPathCompatibility profile, string operand, string positive, string negative)
    {
        foreach (var (sign, expected) in new[] { ("+", positive), ("-", negative) })
            foreach (var expression in new[] { sign + "(" + operand + ")", "(function($x){" + sign + "$x})(" + operand + ")" })
            {
                var report = "let $v := (" + expression + ") return concat(string($v),'|',if ($v instance of xs:integer) then 'integer' else if ($v instance of xs:double) then 'double' else 'decimal')";
                Assert.Equal(expected, Evaluate(report, profile));
            }
    }

    /// <summary>Provides unchanged cardinality, type, atomization and lexical errors.</summary>
    /// <returns>Profile, operand and required error code.</returns>
    public static IEnumerable<object[]> ErrorCases()
    {
        var rows = new (string Operand, string Code)[]
        {
            ("'2'", "XPTY0004"), ("true()", "XPTY0004"), ("[1,2]", "XPTY0004"),
            ("map{}", "FOTY0013"), ("function(){1}", "FOTY0013"),
            ("[xs:untypedAtomic('bad')]", "FORG0001")
        };
        foreach (var profile in Profiles)
            foreach (var row in rows)
                yield return new object[] { profile, row.Operand, row.Code };
    }

    /// <summary>Empty handling must not bypass errors on nonempty inputs.</summary>
    /// <param name="profile">The modern language profile.</param>
    /// <param name="operand">The invalid input.</param>
    /// <param name="code">The expected error code.</param>
    [Theory, MemberData(nameof(ErrorCases))]
    public void NonemptyErrorsRemainErrors(XPathCompatibility profile, string operand, string code)
    {
        foreach (var sign in new[] { "+", "-" })
            foreach (var expression in new[] { sign + "(" + operand + ")", "(function($x){" + sign + "$x})(" + operand + ")" })
                Assert.Contains(code, Assert.ThrowsAny<Exception>(() => Evaluate(expression, profile)).Message);
    }

    /// <summary>Backward-compatible empty conversion remains NaN.</summary>
    /// <param name="operand">An input with an empty atomized value.</param>
    [Theory]
    [InlineData("()")]
    [InlineData("[]")]
    [InlineData("[()]")]
    [InlineData("[[]]")]
    public void BackwardsCompatibleEmptyRemainsNaN(string operand)
    {
        foreach (var sign in new[] { "+", "-" })
        {
            var compiled = XPath31Expression.Compile(sign + "(" + operand + ")", new CompileOptions { BackwardsCompatible = true });
            Assert.Equal("NaN", compiled.Evaluate(new EvaluationContext { BackwardsCompatible = true }).ToString());
        }
    }

    private static string Evaluate(string source, XPathCompatibility profile)
        => XPath31Expression.Compile(source, new CompileOptions { Compatibility = profile })
            .Evaluate(new EvaluationContext()).ToString();
}
