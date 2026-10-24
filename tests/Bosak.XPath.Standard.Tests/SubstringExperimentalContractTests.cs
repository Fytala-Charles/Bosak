// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verifies Experimental substring normalization and frozen profile isolation.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-10-2026     | Creation: absent substring opt-in and compatibility/call-path controls.                  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies Experimental substring normalization and frozen profile isolation.</summary>
public class SubstringExperimentalContractTests
{
    private static readonly XPathCompatibility[] Profiles =
        [XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental];

    private static XPath31Expression Compile(string expression, XPathCompatibility profile) =>
        XPath31Expression.Compile($"string-join(({expression}) ! string(.), ',')",
            new CompileOptions { Compatibility = profile });

    private static string Evaluate(string expression, XPathCompatibility profile) =>
        Compile(expression, profile).Evaluate(new EvaluationContext()).ToString();

    /// <summary>Normalizes an absent substring only under the explicit Experimental profile.</summary>
    /// <param name="call">The direct, reference, lookup, partial or variable-based invocation.</param>
    [Theory]
    [InlineData("index-of-substring('abc', ())")]
    [InlineData("index-of-substring#2('abc', ())")]
    [InlineData("function-lookup(QName('http://www.w3.org/2005/xpath-functions','index-of-substring'),2)('abc', ())")]
    [InlineData("index-of-substring(?, ())('abc')")]
    [InlineData("let $needle := () return index-of-substring('abc', $needle)")]
    public void AbsentSubstring_IsExperimentalOptIn(string call)
    {
        Assert.Equal("1,2,3,4", Evaluate(call, XPathCompatibility.XPath40Experimental));
        var error = Assert.ThrowsAny<Exception>(() => Evaluate(call, XPathCompatibility.XPath40));
        Assert.Contains("XPTY0004", error.Message);
    }

    /// <summary>Handles absent input and counts supplementary characters as single positions.</summary>
    /// <param name="call">The expression containing an absent substring.</param>
    /// <param name="expected">The complete sequence of positions.</param>
    [Theory]
    [InlineData("index-of-substring((), ())", "1")]
    [InlineData("index-of-substring('', ())", "1")]
    [InlineData("index-of-substring('\U0001F600a', ())", "1,2,3")]
    [InlineData("index-of-substring('abc', parse-xml('<r/>')/r/missing/text())", "1,2,3,4")]
    public void ExperimentalEmptyArguments_KeepCodepointBoundaries(string call, string expected)
    {
        Assert.Equal(expected, Evaluate(call, XPathCompatibility.XPath40Experimental));
        Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Evaluate(call, XPathCompatibility.XPath40)).Message);
    }

    /// <summary>Supplies unchanged matching and validation contracts for both explicit profiles.</summary>
    public static TheoryData<XPathCompatibility, string, string> SharedCases
    {
        get
        {
            var rows = new TheoryData<XPathCompatibility, string, string>();
            foreach (var profile in Profiles)
            {
                rows.Add(profile, "index-of-substring('abc', '')", "1,2,3,4");
                rows.Add(profile, "index-of-substring('', '')", "1");
                rows.Add(profile, "index-of-substring((), '')", "1");
                rows.Add(profile, "index-of-substring((), 'x')", "");
                rows.Add(profile, "index-of-substring('banana', 'ana')", "2,4");
                rows.Add(profile, "index-of-substring('\U0001F600a\U0001F600', '\U0001F600')", "1,3");
                rows.Add(profile, "index-of-substring('abc', 'x')", "");
                rows.Add(profile, "index-of-substring('\U0001F600a', '')", "1,2,3");
            }
            return rows;
        }
    }

    /// <summary>Retains explicit-empty, overlap, no-match and Unicode results in both profiles.</summary>
    /// <param name="profile">The explicitly selected compatibility profile.</param>
    /// <param name="call">The matching expression.</param>
    /// <param name="expected">The complete position sequence.</param>
    [Theory]
    [MemberData(nameof(SharedCases))]
    public void ExistingMatching_IsUnchanged(XPathCompatibility profile, string call, string expected) =>
        Assert.Equal(expected, Evaluate(call, profile));

    /// <summary>Supplies invalid argument types and cardinalities in both explicit profiles.</summary>
    public static TheoryData<XPathCompatibility, string> InvalidCalls
    {
        get
        {
            var rows = new TheoryData<XPathCompatibility, string>();
            foreach (var profile in Profiles)
            foreach (var call in new[]
            {
                "index-of-substring(('a','b'), 'a')",
                "index-of-substring('abc', ('a','b'))",
                "index-of-substring(123, 'a')",
                "index-of-substring('abc', 123)"
            })
                rows.Add(profile, call);
            return rows;
        }
    }

    /// <summary>Does not make Experimental normalization a general coercion or cardinality change.</summary>
    /// <param name="profile">The selected explicit profile.</param>
    /// <param name="call">The invalid argument expression.</param>
    [Theory]
    [MemberData(nameof(InvalidCalls))]
    public void InvalidArguments_StillRaiseXpty0004(XPathCompatibility profile, string call) =>
        Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Evaluate(call, profile)).Message);

    /// <summary>Retains the older-profile static refusal of this 4.0 function.</summary>
    /// <param name="profile">The pre-4.0 profile.</param>
    [Theory]
    [InlineData(XPathCompatibility.XPath20)]
    [InlineData(XPathCompatibility.XPath30)]
    [InlineData(XPathCompatibility.XPath31)]
    public void OlderProfiles_StillRefuseFunction(XPathCompatibility profile) =>
        Assert.Contains("XPST0017", Assert.ThrowsAny<Exception>(() =>
            XPath31Expression.Compile("index-of-substring('abc', ())", new CompileOptions { Compatibility = profile })).Message);

    /// <summary>Restamps the selected profile when compiled expressions share a reused context.</summary>
    [Fact]
    public void ContextReuse_DoesNotLeakExperimentalNormalization()
    {
        var experimental = Compile("index-of-substring#2('abc', ())", XPathCompatibility.XPath40Experimental);
        var frozen = Compile("index-of-substring#2('abc', ())", XPathCompatibility.XPath40);
        var context = new EvaluationContext();
        for (int repeat = 0; repeat < 3; repeat++)
        {
            Assert.Equal("1,2,3,4", experimental.Evaluate(context).ToString());
            Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => frozen.Evaluate(context)).Message);
        }
    }

    /// <summary>Keeps the 4.0 function absent from dynamic lookup under default XPath 3.1.</summary>
    [Fact]
    public void LegacyLookup_RemainsAbsent() =>
        Assert.Equal("true", XPath31Expression.Compile(
            "empty(function-lookup(QName('http://www.w3.org/2005/xpath-functions','index-of-substring'),2))")
            .Evaluate(new EvaluationContext()).ToString());
}
