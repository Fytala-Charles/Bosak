// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Verifies repeated unary operators preserve conversion, atomization and errors.
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

/// <summary>Verifies repeated unary operators preserve conversion, atomization and errors.</summary>
public sealed class UnaryCoercionTests
{
    /// <summary>Supplies values and errors shared by three modern profiles.</summary>
    /// <returns>Profile, operand, expected report or error code, and error flag.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Operand, string Expected, bool Error)[]
        {
            ("xs:untypedAtomic('2')", "2|double", false),
            ("xs:untypedAtomic('-2.5')", "-2.5|double", false),
            ("parse-xml('<n>3</n>')/n", "3|double", false),
            ("parse-xml('<n a=\"4\"/>')/n/@a", "4|double", false),
            ("()", "empty", false),
            ("7", "7|integer", false),
            ("9223372036854775808", "9223372036854775808|integer", false),
            ("xs:integer('-9223372036854775808')", "-9223372036854775808|integer", false),
            ("xs:decimal('2.5')", "2.5|decimal", false),
            ("xs:float('2.5')", "2.5|float", false),
            ("xs:double('2.5')", "2.5|double", false),
            ("xs:double('NaN')", "NaN|double", false),
            ("xs:double('INF')", "INF|double", false),
            ("xs:double('-INF')", "-INF|double", false),
            ("xs:unsignedLong('18446744073709551615')", "18446744073709551615|integer", false),
            ("[xs:untypedAtomic('5')]", "5|double", false),
            ("'2'", "XPTY0004", true),
            ("true()", "XPTY0004", true),
            ("xs:anyURI('2')", "XPTY0004", true),
            ("xs:date('2026-10-25')", "XPTY0004", true),
            ("(1,2)", "XPTY0004", true),
            ("[1,2]", "XPTY0004", true),
            ("parse-xml('<r><n>1</n><n>2</n></r>')/r/n", "XPTY0004", true),
            ("xs:untypedAtomic('not-a-number')", "FORG0001", true),
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Operand, row.Expected, row.Error };
    }

    /// <summary>Double negation agrees with two separately invoked unary operations.</summary>
    /// <param name="profile">The modern language profile.</param>
    /// <param name="operand">The input expression.</param>
    /// <param name="expected">The independently specified value/type or error code.</param>
    /// <param name="error">Whether evaluation must fail.</param>
    [Theory, MemberData(nameof(Cases))]
    public void DoubleNegationPreservesSemantics(XPathCompatibility profile, string operand, string expected, bool error)
    {
        var expressions = new[]
        {
            "-(-(" + operand + "))",
            "(function($x){-(-$x)})(" + operand + ")",
            "(function($x){-$x})((function($x){-$x})(" + operand + "))"
        };
        foreach (var expression in expressions)
        {
            if (error)
            {
                var exception = Assert.ThrowsAny<Exception>(() => Evaluate(expression, profile));
                Assert.Contains(expected, exception.Message);
            }
            else
            {
                var report = "let $v := (" + expression + ") return if (empty($v)) then 'empty' else concat(string($v),'|',if ($v instance of xs:integer) then 'integer' else if ($v instance of xs:double) then 'double' else if ($v instance of xs:float) then 'float' else if ($v instance of xs:decimal) then 'decimal' else 'other')";
                Assert.Equal(expected, Evaluate(report, profile));
            }
        }
    }

    /// <summary>Preserves numeric base-type normalization and signed floating zero.</summary>
    /// <param name="profile">The modern language profile.</param>
    [Theory]
    [InlineData(XPathCompatibility.XPath31)]
    [InlineData(XPathCompatibility.XPath40)]
    [InlineData(XPathCompatibility.XPath40Experimental)]
    public void DerivedTypesAndSignedZero(XPathCompatibility profile)
    {
        Assert.Equal("false", Evaluate("-(-xs:short('7')) instance of xs:short", profile));
        Assert.Equal("-INF", Evaluate("1 div (-(-xs:double('-0')))", profile));
        Assert.Equal("INF", Evaluate("1 div (-(-xs:double('0')))", profile));
    }

    /// <summary>Backward-compatible negation still performs its required numeric conversion.</summary>
    /// <param name="operand">The string, boolean or empty operand.</param>
    /// <param name="expected">The converted numeric text.</param>
    [Theory]
    [InlineData("'2'", "2")]
    [InlineData("true()", "1")]
    [InlineData("()", "NaN")]
    [InlineData("'bad'", "NaN")]
    public void BackwardsCompatibleConversion(string operand, string expected)
    {
        var options = new CompileOptions { BackwardsCompatible = true };
        var context = new EvaluationContext { BackwardsCompatible = true };
        var value = XPath31Expression.Compile("-(-(" + operand + "))", options).Evaluate(context);
        Assert.Equal(expected, value.ToString());
        Assert.Equal(Bosak.XPath.Core.Xdm.XdmValueKind.Double, value.Kind);
    }

    private static string Evaluate(string expression, XPathCompatibility profile)
        => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile })
            .Evaluate(new EvaluationContext()).ToString();
}
