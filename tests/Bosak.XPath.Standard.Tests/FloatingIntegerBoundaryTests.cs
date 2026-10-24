// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verify exact floating-point integer thresholds and adjacent representable values.
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

/// <summary>Floating conversion must reject overflow and preserve accepted integer magnitudes exactly.</summary>
public sealed class FloatingIntegerBoundaryTests
{
    /// <summary>Provide exact signed and unsigned thresholds for double and float.</summary>
    /// <returns>Seventy-two profile, target, source, value and error combinations.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Target, string Source, string Expected, string Error)[]
        {
            ("integer", "xs:double('9223372036854774784')", "9223372036854774784", ""),
            ("integer", "xs:double('9223372036854775808')", "", "FOCA0003"),
            ("integer", "xs:double('9223372036854777856')", "", "FOCA0003"),
            ("integer", "xs:double('-9223372036854775808')", "-9223372036854775808", ""),
            ("integer", "xs:double('-9223372036854777856')", "", "FOCA0003"),
            ("integer", "xs:double('-9223372036854774784')", "-9223372036854774784", ""),
            ("integer", "xs:float('9223371487098961920')", "9223371487098961920", ""),
            ("integer", "xs:float('9223372036854775808')", "", "FOCA0003"),
            ("integer", "xs:float('-9223372036854775808')", "-9223372036854775808", ""),
            ("integer", "xs:float('-9223373136366403584')", "", "FOCA0003"),
            ("unsignedLong", "xs:double('0')", "0", ""),
            ("unsignedLong", "xs:double('9223372036854775808')", "9223372036854775808", ""),
            ("unsignedLong", "xs:double('18446744073709549568')", "18446744073709549568", ""),
            ("unsignedLong", "xs:double('18446744073709551616')", "", "FORG0001"),
            ("unsignedLong", "xs:double('18446744073709555712')", "", "FORG0001"),
            ("unsignedLong", "xs:double('9223372036854774784')", "9223372036854774784", ""),
            ("unsignedLong", "xs:float('18446742974197923840')", "18446742974197923840", ""),
            ("unsignedLong", "xs:float('18446744073709551616')", "", "FORG0001"),
            ("unsignedLong", "xs:float('18446746272732807168')", "", "FORG0001"),
            ("unsignedLong", "xs:float('9223372036854775808')", "9223372036854775808", ""),
            ("integer", "xs:double('NaN')", "", "FOCA0002"),
            ("integer", "xs:double('INF')", "", "FOCA0002"),
            ("unsignedLong", "xs:double('NaN')", "", "FOCA0002"),
            ("unsignedLong", "xs:double('INF')", "", "FOCA0002")
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Target, row.Source, row.Expected, row.Error };
    }

    /// <summary>Constructor, cast and function-reference paths agree with castability at each boundary.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    /// <param name="target">The integer-family target type.</param>
    /// <param name="source">The floating-point source expression.</param>
    /// <param name="expected">The exact accepted integer value.</param>
    /// <param name="error">The expected error code, or empty for success.</param>
    [Theory, MemberData(nameof(Cases))]
    public void ExactFloatingBoundaries(XPathCompatibility profile, string target, string source, string expected, string error)
    {
        string Evaluate(string expression) => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile }).Evaluate(new EvaluationContext()).ToString();
        foreach (var expression in new[] { $"xs:{target}({source})", $"({source}) cast as xs:{target}", $"xs:{target}#1({source})" })
        {
            if (error.Length > 0)
                Assert.Contains(error, Assert.ThrowsAny<Exception>(() => Evaluate(expression)).Message);
            else
                Assert.Equal(expected + "|true", Evaluate("let $v := (" + expression + ") return concat(string($v),'|',$v instance of xs:integer)"));
        }
        Assert.Equal(error.Length == 0 ? "true" : "false", Evaluate($"({source}) castable as xs:{target}"));
    }
}
