// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 October 2026
// PURPOSE              : Verifies empty atomized operands in numeric binary arithmetic and ranges.
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

/// <summary>Verifies empty atomized operands in numeric binary arithmetic and ranges.</summary>
public sealed class BinaryEmptyAtomizationTests
{
    private static readonly XPathCompatibility[] Profiles =
        [XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental];

    /// <summary>Provides both empty sides of numeric binary operators and ranges.</summary>
    /// <returns>Profile, operator, left operand and right operand.</returns>
    public static IEnumerable<object[]> EmptyCases()
    {
        foreach (var profile in Profiles)
            foreach (var op in new[] { "+", "-", "*", "div", "idiv", "mod", "to" })
                foreach (var empty in new[] { "[]", "[()]", "[[],[]]", "[[],[[]]]" })
                {
                    var other = op is "idiv" or "mod" ? "0" : "1";
                    yield return new object[] { profile, op, empty, other };
                    yield return new object[] { profile, op, other, empty };
                }
    }

    /// <summary>Propagates empty before numeric conversion or division by zero.</summary>
    /// <param name="profile">The language profile.</param>
    /// <param name="op">The numeric or range operator.</param>
    /// <param name="left">The left expression.</param>
    /// <param name="right">The right expression.</param>
    [Theory, MemberData(nameof(EmptyCases))]
    public void AtomizedEmptyPropagates(XPathCompatibility profile, string op, string left, string right)
    {
        foreach (var source in new[] { "(" + left + ") " + op + " (" + right + ")", "(function($a,$b){$a " + op + " $b})(" + left + "," + right + ")" })
            Assert.Equal("true", Evaluate("empty(" + source + ")", profile));
    }

    /// <summary>Provides retained values, type errors and division errors.</summary>
    /// <returns>Profile, expression, expected output/error and error flag.</returns>
    public static IEnumerable<object[]> Controls()
    {
        var rows = new (string Source, string Expected, bool Error)[]
        {
            ("[[],2] + 1", "3", false),
            ("[[],2] * 3", "6", false),
            ("string-join((([[],2]) to 3) ! string(.), ',')", "2,3", false),
            ("count(3 to 2)", "0", false),
            ("9223372036854775808 + 1", "9223372036854775809", false),
            ("xs:date('2026-10-25') + xs:dayTimeDuration('P1D')", "2026-10-26", false),
            ("(1,2) + 1", "XPTY0004", true),
            ("[1,2] idiv 1", "XPTY0004", true),
            ("xs:untypedAtomic('bad') + 1", "FORG0001", true),
            ("1 idiv 0", "FOAR0001", true),
            ("1 mod 0", "FOAR0001", true),
            ("1.5 to 2", "XPTY0004", true)
        };
        foreach (var profile in Profiles)
            foreach (var row in rows)
                yield return new object[] { profile, row.Source, row.Expected, row.Error };
    }

    /// <summary>Preserves nonempty arithmetic, range, date dispatch and error codes.</summary>
    /// <param name="profile">The language profile.</param>
    /// <param name="source">The expression.</param>
    /// <param name="expected">The expected output or error code.</param>
    /// <param name="error">Whether an error is required.</param>
    [Theory, MemberData(nameof(Controls))]
    public void NonemptyControls(XPathCompatibility profile, string source, string expected, bool error)
    {
        if (error) Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate(source, profile)).Message);
        else Assert.Equal(expected, Evaluate(source, profile));
    }

    /// <summary>Retains backward-compatible NaN conversion in its existing dispatch path.</summary>
    /// <returns>Operator and empty-operand side.</returns>
    public static IEnumerable<object[]> BackwardsCases()
    {
        foreach (var op in new[] { "+", "-", "*", "div", "mod" })
            foreach (var leftEmpty in new[] { true, false }) yield return new object[] { op, leftEmpty };
    }

    /// <summary>Leaves the tested backward-compatible binary conversion unchanged.</summary>
    /// <param name="op">The binary operator.</param>
    /// <param name="leftEmpty">Whether the empty array is on the left.</param>
    [Theory, MemberData(nameof(BackwardsCases))]
    public void BackwardsCompatibleNaN(string op, bool leftEmpty)
    {
        var source = (leftEmpty ? "[]" : "1") + " " + op + " " + (leftEmpty ? "1" : "[]");
        var compiled = XPath31Expression.Compile(source, new CompileOptions { BackwardsCompatible = true });
        Assert.Equal("NaN", compiled.Evaluate(new EvaluationContext { BackwardsCompatible = true }).ToString());
    }

    private static string Evaluate(string source, XPathCompatibility profile)
        => XPath31Expression.Compile(source, new CompileOptions { Compatibility = profile }).Evaluate(new EvaluationContext()).ToString();
}
