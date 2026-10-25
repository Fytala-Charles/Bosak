// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verify Experimental-only signed Unix epochs and frozen profile/host boundaries.
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
//                      | Charles Korthout | 0.2   | 24-10-2026     | Require calendar overflow for valid wide integer epochs in both profiles.                 |
//                      | Charles Korthout | 0.3   | 25-10-2026     | Require repaired minimum literal identity and existing domain/calendar error precedence.  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Signed epochs require explicit Experimental compatibility and retain bounded calendar limits.</summary>
public sealed class UnixDateTimeExperimentalTests
{
    private static XPath31Expression Compile(string expression, XPathCompatibility profile) =>
        XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile });
    private static string Evaluate(string expression, XPathCompatibility profile) => Compile(expression, profile).Evaluate(new EvaluationContext()).ToString();

    /// <summary>Explicit signed epochs reach UTC instants before 1970 only under Experimental.</summary>
    /// <param name="milliseconds">A representable negative integer epoch.</param>
    /// <param name="expected">Exact UTC stamp lexical value.</param>
    [Theory]
    [InlineData("-1", "1969-12-31T23:59:59.999Z")]
    [InlineData("-999", "1969-12-31T23:59:59.001Z")]
    [InlineData("-1000", "1969-12-31T23:59:59Z")]
    [InlineData("-1001", "1969-12-31T23:59:58.999Z")]
    [InlineData("-86400000", "1969-12-31T00:00:00Z")]
    [InlineData("-2208988800000", "1900-01-01T00:00:00Z")]
    [InlineData("-62135596799999", "0001-01-01T00:00:00.001Z")]
    [InlineData("-62135596800000", "0001-01-01T00:00:00Z")]
    public void SignedEpoch_IsExperimentalOptIn(string milliseconds, string expected)
    {
        Assert.Equal(expected, Evaluate($"unix-dateTime({milliseconds})", XPathCompatibility.XPath40Experimental));
        Assert.Contains("FOCA0002", Assert.ThrowsAny<Exception>(() => Evaluate($"unix-dateTime({milliseconds})", XPathCompatibility.XPath40)).Message);
    }

    /// <summary>Direct and indirect calls share the selected signed domain.</summary>
    /// <param name="call">The direct/reference/lookup/partial/variable/untyped invocation.</param>
    [Theory]
    [InlineData("unix-dateTime(-1)")]
    [InlineData("unix-dateTime#1(-1)")]
    [InlineData("function-lookup(QName('http://www.w3.org/2005/xpath-functions','unix-dateTime'),1)(-1)")]
    [InlineData("unix-dateTime(?)(-1)")]
    [InlineData("let $epoch := -1 return unix-dateTime($epoch)")]
    [InlineData("unix-dateTime(xs:untypedAtomic('-1'))")]
    public void InvocationPaths_UseCapturedProfile(string call)
    {
        Assert.Equal("1969-12-31T23:59:59.999Z", Evaluate(call, XPathCompatibility.XPath40Experimental));
    }

    /// <summary>Signed domain does not promise extended years or unlimited integer representation.</summary>
    /// <param name="value">The signed epoch outside the bounded calendar.</param>
    [Theory]
    [InlineData("-62135596800001")]
    [InlineData("-9223372036854775807 - 1")]
    [InlineData("xs:integer('-9223372036854775808')")]
    [InlineData("-9223372036854775808")]
    public void SignedCalendarOverflow_RaisesFodt0001(string value) =>
        Assert.Contains("FODT0001", Assert.ThrowsAny<Exception>(() => Evaluate($"unix-dateTime({value})", XPathCompatibility.XPath40Experimental)).Message);

    /// <summary>Valid wide integer epochs report supported-calendar overflow in both profiles.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    /// <param name="value">The out-of-Int64 positive magnitude.</param>
    [Theory]
    [InlineData(XPathCompatibility.XPath40, "9223372036854775808")]
    [InlineData(XPathCompatibility.XPath40, "9223372036854775809")]
    [InlineData(XPathCompatibility.XPath40Experimental, "9223372036854775808")]
    [InlineData(XPathCompatibility.XPath40Experimental, "9223372036854775809")]
    public void WideIntegerEpoch_RaisesCalendarOverflow(XPathCompatibility profile, string value) =>
        Assert.Contains("FODT0001", Assert.ThrowsAny<Exception>(() => Evaluate($"unix-dateTime({value})", profile)).Message);

    /// <summary>The corrected minimum literal reaches the existing domain and calendar policies.</summary>
    [Fact]
    public void MinimumLiteral_PreservesIntegerIdentityAndEpochPolicy()
    {
        Assert.Equal("true", Evaluate("(-9223372036854775808) instance of xs:integer", XPathCompatibility.XPath40Experimental));
        Assert.Contains("FOCA0002", Assert.ThrowsAny<Exception>(() => Evaluate("unix-dateTime(-9223372036854775808)", XPathCompatibility.XPath40)).Message);
        Assert.Contains("FODT0001", Assert.ThrowsAny<Exception>(() => Evaluate("unix-dateTime(-9223372036854775808)", XPathCompatibility.XPath40Experimental)).Message);
    }

    /// <summary>Reusing an evaluation context cannot leak Experimental signed support into frozen evaluation.</summary>
    [Fact]
    public void ContextReuse_RestampsTheEpochPolicy()
    {
        var experimental = Compile("unix-dateTime#1(-1)", XPathCompatibility.XPath40Experimental);
        var frozen = Compile("unix-dateTime#1(-1)", XPathCompatibility.XPath40);
        var context = new EvaluationContext { ImplicitTimezoneOffsetMinutes = 330 };
        for (var repeat = 0; repeat < 3; repeat++)
        {
            Assert.Equal("1969-12-31T23:59:59.999Z", experimental.Evaluate(context).ToString());
            Assert.Contains("FOCA0002", Assert.ThrowsAny<Exception>(() => frozen.Evaluate(context)).Message);
        }
    }

    /// <summary>Negative results retain the UTC stamp subtype independently of ambient timezone.</summary>
    [Fact]
    public void SignedResult_HasUtcStampType()
    {
        var expression = Compile("let $v := unix-dateTime(-1) return concat($v instance of xs:dateTimeStamp, '|', timezone-from-dateTime($v), '|', $v)", XPathCompatibility.XPath40Experimental);
        foreach (var minutes in new[] { -840, 0, 330, 840 })
            Assert.Equal("true|PT0S|1969-12-31T23:59:59.999Z", expression.Evaluate(new EvaluationContext { ImplicitTimezoneOffsetMinutes = minutes }).ToString());
    }
}
