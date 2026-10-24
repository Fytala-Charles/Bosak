// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verify unsigned floating conversion truncates finite fractions before range facets.
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

/// <summary>Unsigned float and double conversion must agree with numeric truncation toward zero.</summary>
public sealed class UnsignedFloatingCastTests
{
    /// <summary>Provide near-zero, negative-one and nonfinite boundaries for both floating types.</summary>
    /// <returns>Sixty profile, source, expected-value and error combinations.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Source, string Expected, string Error)[]
        {
            ("xs:double('-0.5')", "0", ""),
            ("xs:float('-0.5')", "0", ""),
            ("xs:double('-0.9999999999999999')", "0", ""),
            ("xs:float('-0.99999994')", "0", ""),
            ("xs:double('-1')", "", "FORG0001"),
            ("xs:float('-1')", "", "FORG0001"),
            ("xs:double('-1.0000000000000002')", "", "FORG0001"),
            ("xs:float('-1.0000001')", "", "FORG0001"),
            ("xs:double('-0')", "0", ""),
            ("xs:float('-0')", "0", ""),
            ("xs:double('-5E-324')", "0", ""),
            ("xs:float('-1E-45')", "0", ""),
            ("xs:double('0.5')", "0", ""),
            ("xs:float('0.5')", "0", ""),
            ("xs:double('1.9')", "1", ""),
            ("xs:float('1.9')", "1", ""),
            ("xs:double('NaN')", "", "FOCA0002"),
            ("xs:double('INF')", "", "FOCA0002"),
            ("xs:float('NaN')", "", "FOCA0002"),
            ("xs:float('INF')", "", "FOCA0002")
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Source, row.Expected, row.Error };
    }

    /// <summary>Constructor, cast, reference and castability agree, preserving unsigned and integer identity.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    /// <param name="source">The floating-point source expression.</param>
    /// <param name="expected">The exact truncated value.</param>
    /// <param name="error">The error code, or empty when conversion succeeds.</param>
    [Theory, MemberData(nameof(Cases))]
    public void FloatingTruncationPrecedesFacets(XPathCompatibility profile, string source, string expected, string error)
    {
        string Evaluate(string expression) => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile }).Evaluate(new EvaluationContext()).ToString();
        foreach (var expression in new[] { $"xs:unsignedLong({source})", $"({source}) cast as xs:unsignedLong", $"xs:unsignedLong#1({source})" })
        {
            if (error.Length > 0)
                Assert.Contains(error, Assert.ThrowsAny<Exception>(() => Evaluate(expression)).Message);
            else
                Assert.Equal(expected + "|true|true", Evaluate("let $v := (" + expression + ") return concat(string($v),'|',$v instance of xs:integer,'|',$v instance of xs:unsignedLong)"));
        }
        Assert.Equal(error.Length == 0 ? "true" : "false", Evaluate($"({source}) castable as xs:unsignedLong"));
    }
}
