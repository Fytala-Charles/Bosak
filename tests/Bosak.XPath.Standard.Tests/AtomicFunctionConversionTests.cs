// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Verifies atomic conversion and cardinality for numeric standard-function arguments.
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

/// <summary>Verifies atomic conversion and cardinality for numeric standard-function arguments.</summary>
public sealed class AtomicFunctionConversionTests
{
    /// <summary>Provides independently expected abs argument conversion cases.</summary>
    /// <returns>Profile, expression, expected value or error and error flag.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Argument, string Expected, bool Error)[]
        {
            ("2", "2", false),
            ("()", "()", false),
            ("[]", "()", false),
            ("[()]", "()", false),
            ("[[],[]]", "()", false),
            ("[2]", "2", false),
            ("[[2]]", "2", false),
            ("[[],2,[]]", "2", false),
            ("[[[]],[-2],[]]", "2", false),
            ("(1,2)", "XPTY0004", true),
            ("[1,2]", "XPTY0004", true),
            ("[[1,2]]", "XPTY0004", true),
            ("[[],[1,2],[]]", "XPTY0004", true),
            ("xs:untypedAtomic('-2')", "2", false),
            ("[xs:untypedAtomic('-2')]", "2", false),
            ("xs:untypedAtomic('bad')", "FORG0001", true),
            ("[xs:untypedAtomic('bad')]", "FORG0001", true),
            ("'2'", "XPTY0004", true),
            ("['2']", "XPTY0004", true),
            ("true()", "XPTY0004", true),
            ("[true()]", "XPTY0004", true),
            ("map{}", "FOTY0013", true),
            ("[map{}]", "FOTY0013", true),
            ("function(){1}", "FOTY0013", true),
            ("[function(){1}]", "FOTY0013", true),
        };
        string[] forms = ["abs({0})", "abs#1({0})", "function-lookup(QName('http://www.w3.org/2005/xpath-functions','abs'),1)({0})", "(function($x){abs($x)})({0})"];
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                foreach (var form in forms)
                    yield return new object[] { profile, form.Replace("{0}", row.Argument), row.Expected, row.Error };
    }

    /// <summary>Checks values and required errors through static and dynamic public calls.</summary>
    /// <param name="profile">The compatibility profile.</param>
    /// <param name="expression">The complete function call.</param>
    /// <param name="expected">The expected value or error code.</param>
    /// <param name="error">Whether evaluation must fail.</param>
    [Theory, MemberData(nameof(Cases))]
    public void ConvertsAtomicArgument(XPathCompatibility profile, string expression, string expected, bool error)
    {
        string Evaluate() => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile })
            .Evaluate(new EvaluationContext()).ToString();
        if (error)
            Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate()).Message);
        else
            Assert.Equal(expected, Evaluate());
    }

    /// <summary>Preserves numeric result types and shared numeric conversion errors.</summary>
    /// <returns>Profile, expression and expected boolean result or error.</returns>
    public static IEnumerable<object[]> Controls()
    {
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
        {
            foreach (var type in new[] { "integer", "decimal", "float", "double" })
                yield return new object[] { profile, $"abs([xs:{type}('-2')]) instance of xs:{type}", "true", false };
            yield return new object[] { profile, "abs([xs:untypedAtomic('-2')]) instance of xs:double", "true", false };
            yield return new object[] { profile, "string(abs(xs:double('NaN')))", "NaN", false };
            yield return new object[] { profile, "string(abs(xs:double('-INF')))", "INF", false };
            yield return new object[] { profile, "(1 div abs(xs:double('-0'))) eq xs:double('INF')", "true", false };
            yield return new object[] { profile, "local-name-from-QName((function($f as function(item()*) as item()*){function-name($f)})(abs#1))", "abs", false };
            foreach (var name in new[] { "floor", "ceiling", "round" })
            {
                yield return new object[] { profile, $"{name}([[],2,[]])", "2", false };
                yield return new object[] { profile, $"{name}((1,2))", "XPTY0004", true };
                yield return new object[] { profile, $"{name}([map{{}}])", "FOTY0013", true };
                yield return new object[] { profile, $"{name}#1([function(){{1}}])", "FOTY0013", true };
            }
        }
    }

    /// <summary>Checks numeric identity and neighboring numeric functions.</summary>
    /// <param name="profile">The compatibility profile.</param>
    /// <param name="expression">The control expression.</param>
    /// <param name="expected">The expected value or error.</param>
    /// <param name="error">Whether evaluation must fail.</param>
    [Theory, MemberData(nameof(Controls))]
    public void PreservesNumericControls(XPathCompatibility profile, string expression, string expected, bool error)
        => ConvertsAtomicArgument(profile, expression, expected, error);

    /// <summary>Preserves first-item numeric behavior in backwards-compatible evaluation.</summary>
    /// <param name="expression">The backwards-compatible call.</param>
    /// <param name="expected">The expected result.</param>
    [Theory]
    [InlineData("abs((1,2))", "1")]
    [InlineData("abs#1((1,2))", "1")]
    [InlineData("abs(())", "()")]
    public void PreservesBackwardsCompatibleCalls(string expression, string expected)
    {
        var options = new CompileOptions { BackwardsCompatible = true };
        var context = new EvaluationContext { BackwardsCompatible = true };
        Assert.Equal(expected, XPath31Expression.Compile(expression, options).Evaluate(context).ToString());
    }
}
