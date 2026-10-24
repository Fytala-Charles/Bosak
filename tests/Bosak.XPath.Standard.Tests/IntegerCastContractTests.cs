// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verify supported integer constructors, casts and castability controls.
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

/// <summary>Supported integer conversion controls complement separately retained boundary defect observations.</summary>
public sealed class IntegerCastContractTests
{
    /// <summary>Provide supported lexical, numeric, derived-type and castability cases.</summary>
    /// <returns>Seventy-two explicit profile, expression, result and error-flag combinations.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var rows = new (string Expression, string Expected, bool Error)[]
        {
            ("xs:integer('9223372036854775807')", "9223372036854775807", false),
            ("xs:integer('-9223372036854775808')", "-9223372036854775808", false),
            ("xs:integer('  +001  ')", "1", false),
            ("xs:integer(3.9)", "3", false),
            ("xs:integer(-3.9)", "-3", false),
            ("xs:integer(true())", "1", false),
            ("xs:integer(xs:untypedAtomic('42'))", "42", false),
            ("xs:unsignedLong('18446744073709551615')", "18446744073709551615", false),
            ("'127' cast as xs:byte", "127", false),
            ("12 cast as xs:integer", "12", false),
            ("xs:long('9223372036854775807')", "9223372036854775807", false),
            ("xs:int('2147483647')", "2147483647", false),
            ("xs:unsignedInt('4294967295')", "4294967295", false),
            ("xs:unsignedLong('18446744073709551615') instance of xs:integer", "true", false),
            ("xs:integer('1.0')", "FORG0001", true),
            ("xs:integer('1e3')", "FORG0001", true),
            ("xs:integer('--1')", "FORG0001", true),
            ("xs:integer('not-an-integer')", "FORG0001", true),
            ("xs:integer(xs:double('NaN'))", "FOCA0002", true),
            ("xs:integer(xs:double('INF'))", "FOCA0002", true),
            ("'42' castable as xs:integer", "true", false),
            ("'1.0' castable as xs:integer", "false", false),
            ("(1,2) castable as xs:integer", "false", false),
            ("() castable as xs:integer?", "true", false)
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in rows)
                yield return new object[] { profile, row.Expression, row.Expected, row.Error };
    }

    /// <summary>Valid supported conversions remain exact and malformed input remains distinct from range defects.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    /// <param name="expression">The constructor, cast or castability expression.</param>
    /// <param name="expected">The exact value or documented error control.</param>
    /// <param name="error">Whether evaluation must report the error code.</param>
    [Theory, MemberData(nameof(Cases))]
    public void SupportedConversionControls(XPathCompatibility profile, string expression, string expected, bool error)
    {
        string Evaluate() => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile }).Evaluate(new EvaluationContext()).ToString();
        if (error)
            Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate()).Message);
        else
            Assert.Equal(expected, Evaluate());
    }
}
