// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verify Unix-time calendar errors for decimal-backed integer operands.
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

/// <summary>Calendar limits apply to valid wide integers before narrowing to Int64.</summary>
public sealed class UnixDateTimeWideRangeTests
{
    private static string Evaluate(string expression, XPathCompatibility profile) =>
        XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile }).Evaluate(new EvaluationContext()).ToString();

    /// <summary>Supply wide operands and unchanged conversion controls in both explicit profiles.</summary>
    /// <returns>Thirty profile, expression and expected-error combinations.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var ranges = new[] {
            "unix-dateTime(9223372036854775808)",
            "unix-dateTime(9223372036854775809)",
            "unix-dateTime#1(9223372036854775808)",
            "unix-dateTime(?)(9223372036854775808)",
            "function-lookup(QName('http://www.w3.org/2005/xpath-functions','unix-dateTime'),1)(9223372036854775808)",
            "let $v := 9223372036854775808 return unix-dateTime($v)",
            "unix-dateTime(9223372036854775807 + 1)",
            "unix-dateTime(xs:unsignedLong('18446744073709551615'))"
        };
        foreach (var profile in new[] { XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
        {
            foreach (var expression in ranges)
                yield return new object[] { profile, expression, "FODT0001" };
            foreach (var expression in new[] {
                "unix-dateTime(xs:untypedAtomic('not-an-integer'))",
                "unix-dateTime(xs:untypedAtomic('9223372036854775808'))",
                "unix-dateTime(xs:integer('9223372036854775808'))" })
                yield return new object[] { profile, expression, "FORG0001" };
            foreach (var expression in new[] {
                "unix-dateTime(-9223372036854775807 - 2)",
                "unix-dateTime(xs:integer('-9223372036854775807') - 2)" })
                yield return new object[] { profile, expression, profile == XPathCompatibility.XPath40 ? "FOCA0002" : "FODT0001" };
            yield return new object[] { profile, "unix-dateTime(253402300800000)", "FODT0001" };
            yield return new object[] { profile, "unix-dateTime(9223372036854775808.5)", "FORG0001" };
        }
    }

    /// <summary>Distinguish valid integer calendar overflow from separate lexical/constructor limitations.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    /// <param name="expression">The tested invocation or conversion control.</param>
    /// <param name="expected">The required error code for this bounded correction.</param>
    [Theory, MemberData(nameof(Cases))]
    public void RangeAndConversionErrorsStayDistinct(XPathCompatibility profile, string expression, string expected) =>
        Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate(expression, profile)).Message);

    /// <summary>Decimal-backed integer arithmetic within the calendar still returns exact UTC milliseconds.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    [Theory, InlineData(XPathCompatibility.XPath40), InlineData(XPathCompatibility.XPath40Experimental)]
    public void WideBackingCanReturnIntoCalendar(XPathCompatibility profile) =>
        Assert.Equal("1970-01-01T00:00:00.001Z", Evaluate("unix-dateTime((9223372036854775807 + 1) - 9223372036854775807)", profile));
}
