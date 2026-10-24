// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verify wide-integer identity and the supported Unix-time calendar boundary.
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

/// <summary>Wide integer identity remains separate from bounded Unix calendar conversion.</summary>
public sealed class UnixDateTimeWideIntegerBoundaryTests
{
    private static string Evaluate(string expression, XPathCompatibility profile) =>
        XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile })
            .Evaluate(new EvaluationContext()).ToString();

    /// <summary>Supply both explicit profiles without treating observed defects as required behavior.</summary>
    /// <returns>Twenty-four profile, expression and expected value/error combinations.</returns>
    public static IEnumerable<object[]> BoundaryCases()
    {
        var cases = new (string Expression, string Expected, bool IsError)[]
        {
            ("9223372036854775808 instance of xs:integer", "true", false),
            ("9223372036854775808 instance of xs:decimal", "true", false),
            ("9223372036854775808 eq 9223372036854775809", "false", false),
            ("string(9223372036854775808)", "9223372036854775808", false),
            ("unix-dateTime(9223372036854775807)", "FODT0001", true),
            ("unix-dateTime#1(9223372036854775807)", "FODT0001", true),
            ("unix-dateTime(?)(9223372036854775807)", "FODT0001", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','unix-dateTime'),1)(9223372036854775807)", "FODT0001", true),
            ("unix-dateTime(253402300799999)", "9999-12-31T23:59:59.999Z", false),
            ("unix-dateTime(253402300800000)", "FODT0001", true),
            ("unix-dateTime(xs:untypedAtomic('not-an-integer'))", "FORG0001", true),
            ("unix-dateTime(xs:double('NaN'))", "FOCA0002", true)
        };
        foreach (var profile in new[] { XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var item in cases)
                yield return new object[] { profile, item.Expression, item.Expected, item.IsError };
    }

    /// <summary>Verify exact wide literals and supported calendar controls independently of unresolved overflow.</summary>
    /// <param name="profile">The explicit XPath compatibility profile.</param>
    /// <param name="expression">The boundary expression.</param>
    /// <param name="expected">The exact value or supported error code.</param>
    /// <param name="isError">Whether evaluation must raise the indicated error.</param>
    [Theory, MemberData(nameof(BoundaryCases))]
    public void WideIdentityAndCalendarControls(XPathCompatibility profile, string expression, string expected, bool isError)
    {
        if (isError)
            Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate(expression, profile)).Message);
        else
            Assert.Equal(expected, Evaluate(expression, profile));
    }
}
