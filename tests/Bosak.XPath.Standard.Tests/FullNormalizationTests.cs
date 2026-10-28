// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 28 October 2026
// PURPOSE              : Verify FULLY-NORMALIZED conversion, scalar prefixes and representative idempotence.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 28-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verify FULLY-NORMALIZED conversion, scalar prefixes and representative idempotence.</summary>
public sealed class FullNormalizationTests
{
    /// <summary>Supplies exact scalar results across profiles and the shared XSLT mode.</summary>
    /// <returns>Profile, XSLT mode, scalar input and exact expected output codepoints.</returns>
    public static IEnumerable<object[]> Cases()
    {
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
        foreach (bool xslt in new[] { false, true })
        foreach (var pair in new[]
        {
            ("()", ""), ("(98,108,97,104)", "98,108,97,104"),
            ("(65,775)", "550"), ("(550)", "550"),
            ("(769)", "32,769"), ("(847)", "847"), ("(8413)", "8413"),
            ("(2494)", "32,2494"), ("(2495)", "2495"), ("(2307)", "2307"),
            ("(4352)", "4352"), ("(4449)", "32,4449"), ("(4520)", "32,4520"),
            ("(4352,4449)", "44032"), ("(44032,4520)", "44033"),
            ("(2503,2494)", "2507"), ("(119141)", "32,119141"), ("(119143)", "32,119143"),
            ("(32,769)", "32,769"), ("(113,789,768)", "113,768,789"),
            ("(65,778,769)", "506"), ("(768,769)", "32,768,769")
        }) yield return [profile, xslt, pair.Item1, pair.Item2];
    }

    /// <summary>Checks exact output, a string result and representative idempotence through the API.</summary>
    /// <param name="profile">The compatibility profile.</param>
    /// <param name="xslt">Whether the shared function runs in XSLT mode.</param>
    /// <param name="input">The input scalar sequence.</param>
    /// <param name="expected">Comma-separated output scalars.</param>
    [Theory, MemberData(nameof(Cases))]
    public void ConvertsInsteadOfValidating(XPathCompatibility profile, bool xslt, string input, string expected)
    {
        var context = new EvaluationContext { IsXsltMode = xslt };
        FunctionLibrary.Populate(context);
        string query = $"let $r := normalize-unicode(codepoints-to-string({input}), 'FULLY-NORMALIZED') return concat(count($r), ':', string-join(string-to-codepoints($r) ! string(.), ','), ':', deep-equal(normalize-unicode($r,'FULLY-NORMALIZED'),$r))";
        Assert.Equal("1:" + expected + ":true", XPath31Expression.Compile(query, new CompileOptions { Compatibility = profile }).Evaluate(context).ToString());
    }

    /// <summary>Retains type and unsupported-form errors.</summary>
    /// <param name="expression">The invalid call.</param>
    /// <param name="code">The existing required error.</param>
    [Theory]
    [InlineData("normalize-unicode('x','UNKNOWN-C21')", "FOCH0003")]
    [InlineData("normalize-unicode(42,'FULLY-NORMALIZED')", "XPTY0004")]
    [InlineData("normalize-unicode('x',42)", "XPTY0004")]
    public void RetainsErrors(string expression, string code) =>
        Assert.Contains(code, Assert.ThrowsAny<Exception>(() => XPath31Expression.Compile(expression).Evaluate(new EvaluationContext())).Message);
}
