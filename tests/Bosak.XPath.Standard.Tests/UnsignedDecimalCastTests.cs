// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verify unsignedLong decimal truncation precedes range facet validation.
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

/// <summary>Unsigned decimal conversion truncates toward zero before validating range facets.</summary>
public sealed class UnsignedDecimalCastTests
{
    /// <summary>Provide decimal numeric boundaries and lexical controls in modern profiles.</summary>
    /// <returns>Forty-eight profile, source, expected-value and error combinations.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Source, string Expected, string Error)[]
        {
            ("xs:decimal('0.0')", "0", ""),
            ("xs:decimal('-0.5')", "0", ""),
            ("xs:decimal('0.5')", "0", ""),
            ("xs:decimal('1.9')", "1", ""),
            ("xs:decimal('-1.1')", "", "FORG0001"),
            ("xs:decimal('9223372036854775806.9')", "9223372036854775806", ""),
            ("xs:decimal('9223372036854775807.9')", "9223372036854775807", ""),
            ("xs:decimal('9223372036854775808.9')", "9223372036854775808", ""),
            ("xs:decimal('18446744073709551614.5')", "18446744073709551614", ""),
            ("xs:decimal('18446744073709551615.5')", "18446744073709551615", ""),
            ("xs:decimal('18446744073709551616')", "", "FORG0001"),
            ("xs:decimal('79228162514264337593543950335')", "", "FORG0001"),
            ("'1.9'", "", "FORG0001"),
            ("xs:untypedAtomic('-0.5')", "", "FORG0001"),
            ("'  +001  '", "1", ""),
            ("'18446744073709551615'", "18446744073709551615", "")
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Source, row.Expected, row.Error };
    }

    /// <summary>Constructor, cast, reference and castability agree without allowing fractional lexical forms.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    /// <param name="source">The numeric decimal or lexical control.</param>
    /// <param name="expected">The exact truncated unsigned integer value.</param>
    /// <param name="error">The error code, or empty when conversion succeeds.</param>
    [Theory, MemberData(nameof(Cases))]
    public void TruncationPrecedesFacets(XPathCompatibility profile, string source, string expected, string error)
    {
        string Evaluate(string expression) => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile }).Evaluate(new EvaluationContext()).ToString();
        foreach (var expression in new[] { $"xs:unsignedLong({source})", $"({source}) cast as xs:unsignedLong", $"xs:unsignedLong#1({source})" })
        {
            if (error.Length > 0)
                Assert.Contains(error, Assert.ThrowsAny<Exception>(() => Evaluate(expression)).Message);
            else
                Assert.Equal(expected + "|true", Evaluate("let $v := (" + expression + ") return concat(string($v),'|',$v instance of xs:integer)"));
        }
        Assert.Equal(error.Length == 0 ? "true" : "false", Evaluate($"({source}) castable as xs:unsignedLong"));
    }
}
