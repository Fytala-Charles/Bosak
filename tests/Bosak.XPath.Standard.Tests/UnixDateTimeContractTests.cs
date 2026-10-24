// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verify frozen Unix-time units, defaults, range boundaries and profile isolation.
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

/// <summary>Bounded existing Unix-time contracts; current upstream adoption is separate.</summary>
public sealed class UnixDateTimeContractTests
{
    private static readonly XPathCompatibility[] Profiles = [XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental];
    private static XPath31Expression Compile(string expression, XPathCompatibility profile) =>
        XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile });
    private static string Evaluate(string expression, XPathCompatibility profile) => Compile(expression, profile).Evaluate(new EvaluationContext()).ToString();

    /// <summary>Supplies exact millisecond boundaries in both available 4.0 profiles.</summary>
    /// <returns>Profile, epoch milliseconds and exact UTC lexical value.</returns>
    public static IEnumerable<object[]> MillisecondCases()
    {
        foreach (var profile in Profiles)
        foreach (var pair in new[]
        {
            ("0", "1970-01-01T00:00:00Z"),
            ("1", "1970-01-01T00:00:00.001Z"),
            ("999", "1970-01-01T00:00:00.999Z"),
            ("1000", "1970-01-01T00:00:01Z"),
            ("1001", "1970-01-01T00:00:01.001Z"),
            ("86400000", "1970-01-02T00:00:00Z"),
            ("2147483647", "1970-01-25T20:31:23.647Z"),
            ("2147483648", "1970-01-25T20:31:23.648Z"),
            ("2147483647000", "2038-01-19T03:14:07Z"),
            ("2147483648000", "2038-01-19T03:14:08Z"),
            ("253402300799999", "9999-12-31T23:59:59.999Z")
        }) yield return [profile, pair.Item1, pair.Item2];
    }

    /// <summary>Distinguishes milliseconds from seconds and preserves supported precision.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    /// <param name="milliseconds">Exact nonnegative integer input.</param>
    /// <param name="expected">Exact UTC lexical result.</param>
    [Theory, MemberData(nameof(MillisecondCases))]
    public void SupportedMilliseconds_AreExact(XPathCompatibility profile, string milliseconds, string expected) =>
        Assert.Equal(expected, Evaluate($"unix-dateTime({milliseconds})", profile));

    /// <summary>Supplies omitted/empty argument call paths without relying on current time.</summary>
    /// <returns>Profile and epoch-default invocation.</returns>
    public static IEnumerable<object[]> DefaultCalls()
    {
        foreach (var profile in Profiles)
        foreach (var call in new[]
        {
            "unix-dateTime()", "unix-dateTime(())", "unix-dateTime#0()", "unix-dateTime#1(())",
            "function-lookup(QName('http://www.w3.org/2005/xpath-functions','unix-dateTime'),0)()",
            "function-lookup(QName('http://www.w3.org/2005/xpath-functions','unix-dateTime'),1)(())",
            "unix-dateTime(?)(())", "let $epoch := () return unix-dateTime($epoch)"
        }) yield return [profile, call];
    }

    /// <summary>Omission and empty input mean epoch zero on every tested call path.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    /// <param name="call">The direct or indirect invocation.</param>
    [Theory, MemberData(nameof(DefaultCalls))]
    public void Default_IsEpochRatherThanNow(XPathCompatibility profile, string call) =>
        Assert.Equal("1970-01-01T00:00:00Z", Evaluate(call, profile));

    /// <summary>The Experimental profile has not adopted the newer negative-epoch contract.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    [Theory, InlineData(XPathCompatibility.XPath40), InlineData(XPathCompatibility.XPath40Experimental)]
    public void ExistingNegativeContract_RemainsFoca0002(XPathCompatibility profile)
    {
        // Construct Int64.MinValue inside integer arithmetic; the direct lexical magnitude
        // crosses Bosak's integer-literal representation boundary and is a separate diagnostic.
        foreach (var value in new[] { "-1", "-86400000", "-9223372036854775807 - 1" })
            Assert.Contains("FOCA0002", Assert.ThrowsAny<Exception>(() => Evaluate($"unix-dateTime({value})", profile)).Message);
    }

    /// <summary>Existing negative-input refusal also holds through references, lookup and partial application.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    [Theory, InlineData(XPathCompatibility.XPath40), InlineData(XPathCompatibility.XPath40Experimental)]
    public void NegativeRefusal_IsStableAcrossCallPaths(XPathCompatibility profile)
    {
        foreach (var call in new[]
        {
            "unix-dateTime#1(-1)",
            "function-lookup(QName('http://www.w3.org/2005/xpath-functions','unix-dateTime'),1)(-1)",
            "unix-dateTime(?)(-1)", "let $epoch := -1 return unix-dateTime($epoch)"
        }) Assert.Contains("FOCA0002", Assert.ThrowsAny<Exception>(() => Evaluate(call, profile)).Message);
    }

    /// <summary>Representable integer inputs beyond the .NET calendar limit use the date-range error.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    [Theory, InlineData(XPathCompatibility.XPath40), InlineData(XPathCompatibility.XPath40Experimental)]
    public void CalendarOverflow_IsFodt0001(XPathCompatibility profile)
    {
        foreach (var value in new[] { "253402300800000", "9223372036854775807" })
            Assert.Contains("FODT0001", Assert.ThrowsAny<Exception>(() => Evaluate($"unix-dateTime({value})", profile)).Message);
    }

    /// <summary>The result remains a UTC stamp regardless of the reused context's implicit timezone.</summary>
    /// <param name="profile">The explicit compatibility profile.</param>
    [Theory, InlineData(XPathCompatibility.XPath40), InlineData(XPathCompatibility.XPath40Experimental)]
    public void ResultTypeAndUtc_AreIndependentOfImplicitTimezone(XPathCompatibility profile)
    {
        var expression = Compile("let $stamp := unix-dateTime(1) return concat($stamp instance of xs:dateTimeStamp, '|', timezone-from-dateTime($stamp), '|', $stamp)", profile);
        var context = new EvaluationContext();
        foreach (var minutes in new[] { -840, 0, 330, 840 })
        {
            context.ImplicitTimezoneOffsetMinutes = minutes;
            Assert.Equal("true|PT0S|1970-01-01T00:00:00.001Z", expression.Evaluate(context).ToString());
        }
    }

    /// <summary>Produces a complete supplied-input deadline report across a 2038 millisecond boundary.</summary>
    /// <param name="elapsedMilliseconds">Supplied elapsed measurement; no clock read is involved.</param>
    /// <param name="remaining">Expected signed remaining milliseconds.</param>
    /// <param name="onTime">Expected deadline comparison.</param>
    [Theory, InlineData(1, "1", "true"), InlineData(2, "0", "true"), InlineData(3, "-1", "false")]
    public void DeadlineReport_MatchesXPath31Baseline(int elapsedMilliseconds, string remaining, string onTime)
    {
        var tail = $"let $deadline := $received + seconds(0.002) return let $finished := $received + seconds({elapsedMilliseconds} div 1000) return concat('received=', $received, '; deadline=', $deadline, '; elapsedMs=', duration-to-seconds($finished - $received) * 1000, '; remainingMs=', duration-to-seconds($deadline - $finished) * 1000, '; onTime=', $finished le $deadline)";
        var baseline = "let $received := xs:dateTime('1970-01-01T00:00:00Z') + xs:dayTimeDuration('PT0.001S') * 2147483647999 return " + tail;
        baseline = baseline.Replace("seconds(0.002)", "xs:dayTimeDuration('PT0.002S')")
            .Replace($"seconds({elapsedMilliseconds} div 1000)", $"xs:dayTimeDuration('PT0.001S') * {elapsedMilliseconds}")
            .Replace("duration-to-seconds($finished - $received)", "(($finished - $received) div xs:dayTimeDuration('PT1S'))")
            .Replace("duration-to-seconds($deadline - $finished)", "(($deadline - $finished) div xs:dayTimeDuration('PT1S'))");
        var expected = $"received=2038-01-19T03:14:07.999Z; deadline=2038-01-19T03:14:08.001Z; elapsedMs={elapsedMilliseconds}; remainingMs={remaining}; onTime={onTime}";
        Assert.Equal(expected, Evaluate(baseline, XPathCompatibility.XPath31));
        foreach (var profile in Profiles)
            Assert.Equal(expected, Evaluate("let $received := unix-dateTime(2147483647999) return " + tail, profile));
    }

    /// <summary>Older profiles retain static refusal and absent dynamic lookup.</summary>
    /// <param name="profile">The pre-4.0 profile.</param>
    [Theory, InlineData(XPathCompatibility.XPath20), InlineData(XPathCompatibility.XPath30), InlineData(XPathCompatibility.XPath31)]
    public void OlderProfiles_RemainUnavailable(XPathCompatibility profile)
    {
        foreach (var call in new[] { "unix-dateTime()", "unix-dateTime(0)" })
            Assert.Contains("XPST0017", Assert.ThrowsAny<Exception>(() => Compile(call, profile)).Message);
        Assert.Equal("true", Evaluate("empty(function-lookup(QName('http://www.w3.org/2005/xpath-functions','unix-dateTime'),0)) and empty(function-lookup(QName('http://www.w3.org/2005/xpath-functions','unix-dateTime'),1))", profile));
    }
}
