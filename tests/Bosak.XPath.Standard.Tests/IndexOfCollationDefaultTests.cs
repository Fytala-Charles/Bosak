// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 21 October 2026
// PURPOSE              : Verifies index-of empty-collation defaults while retaining XPath 3.1 behavior.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 21-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies index-of empty-collation defaults while retaining XPath 3.1 behavior.</summary>
public class IndexOfCollationDefaultTests
{
    private const string Codepoint = "http://www.w3.org/2005/xpath-functions/collation/codepoint";
    private const string Ascii = "http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive";
    private static readonly string[] Calls =
    [
        "index-of(('A','a','A'),'A',())",
        "index-of#3(('A','a','A'),'A',())",
        "function-lookup(QName('http://www.w3.org/2005/xpath-functions','index-of'),3)(('A','a','A'),'A',())",
        "index-of(?, 'A', ()) (('A','a','A'))",
        "index-of(input := ('A','a','A'), target := 'A', collation := ())"
    ];

    private static XPath31Expression Compile(string expression, XPathCompatibility compatibility) =>
        XPath31Expression.Compile(expression, new CompileOptions { Compatibility = compatibility });

    private static string Evaluate(string expression, XPathCompatibility compatibility, string collation = Codepoint) =>
        Compile(expression, compatibility).Evaluate(new EvaluationContext { DefaultCollation = collation }).ToString();

    /// <summary>Supplies every call path in both explicit 4.0 compatibility modes.</summary>
    public static TheoryData<XPathCompatibility, string> DefaultedCalls
    {
        get
        {
            var rows = new TheoryData<XPathCompatibility, string>();
            foreach (var compatibility in new[] { XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (string call in Calls)
                rows.Add(compatibility, call);
            return rows;
        }
    }

    /// <summary>Supplies the same call paths for unchanged 3.1 refusal checks.</summary>
    public static TheoryData<string> LegacyCalls
    {
        get
        {
            var rows = new TheoryData<string>();
            foreach (string call in Calls)
                rows.Add(call);
            return rows;
        }
    }

    /// <summary>Uses each evaluation's default collation without capturing or leaking another context.</summary>
    /// <param name="compatibility">The explicit 4.0 compatibility level.</param>
    /// <param name="call">The positional, reference, lookup, partial or keyword call.</param>
    [Theory]
    [MemberData(nameof(DefaultedCalls))]
    public void EmptyCollationUsesRuntimeDefault(XPathCompatibility compatibility, string call)
    {
        var expression = Compile("string-join(" + call + ",'|')", compatibility);
        Assert.Equal("1|3", expression.Evaluate(new EvaluationContext { DefaultCollation = Codepoint }).ToString());
        Assert.Equal("1|2|3", expression.Evaluate(new EvaluationContext { DefaultCollation = Ascii }).ToString());
        Assert.Equal("1|3", expression.Evaluate(new EvaluationContext { DefaultCollation = Codepoint }).ToString());
    }

    /// <summary>Retains 3.1's required collation argument and keyword grammar refusal.</summary>
    /// <param name="call">The same call path used by the 4.0 default checks.</param>
    [Theory]
    [MemberData(nameof(LegacyCalls))]
    public void XPath31StillRefusesEmptyCollation(string call)
    {
        var error = Assert.ThrowsAny<Exception>(() => Evaluate(call, XPathCompatibility.XPath31));
        Assert.Contains(call.Contains(":=", StringComparison.Ordinal) ? "XPST0003" : "XPTY0004", error.Message);
    }

    /// <summary>Retains omitted defaults and explicit collation overrides in both language levels.</summary>
    /// <param name="compatibility">The 3.1 or frozen 4.0 compatibility level.</param>
    [Theory]
    [InlineData(XPathCompatibility.XPath31)]
    [InlineData(XPathCompatibility.XPath40)]
    public void OmittedAndExplicitCollationsRetainBehavior(XPathCompatibility compatibility)
    {
        Assert.Equal("1|2|3", Evaluate("string-join(index-of(('A','a','A'),'A'),'|')", compatibility, Ascii));
        Assert.Equal("1|3", Evaluate($"string-join(index-of(('A','a','A'),'A','{Codepoint}'),'|')", compatibility, Ascii));
        Assert.Equal("1|2|3", Evaluate($"string-join(index-of(('A','a','A'),'A','{Ascii}'),'|')", compatibility));
    }

    /// <summary>Retains argument typing and unknown-collation errors.</summary>
    /// <param name="compatibility">The explicit compatibility level.</param>
    /// <param name="argument">The invalid nonempty third argument.</param>
    /// <param name="code">The unchanged error identity.</param>
    [Theory]
    [InlineData(XPathCompatibility.XPath31, "'urn:unsupported-index-collation'", "FOCH0002")]
    [InlineData(XPathCompatibility.XPath40, "'urn:unsupported-index-collation'", "FOCH0002")]
    [InlineData(XPathCompatibility.XPath31, "('A','B')", "XPTY0004")]
    [InlineData(XPathCompatibility.XPath40, "('A','B')", "XPTY0004")]
    [InlineData(XPathCompatibility.XPath31, "42", "XPTY0004")]
    [InlineData(XPathCompatibility.XPath40, "42", "XPTY0004")]
    public void InvalidNonemptyArgumentsStillRefuse(XPathCompatibility compatibility, string argument, string code)
    {
        var error = Assert.ThrowsAny<Exception>(() => Evaluate($"index-of(('A'),'A',{argument})", compatibility));
        Assert.Contains(code, error.Message);
    }

    /// <summary>Defaults an empty-input call without weakening the required target argument.</summary>
    /// <param name="compatibility">The frozen or experimental 4.0 mode.</param>
    [Theory]
    [InlineData(XPathCompatibility.XPath40)]
    [InlineData(XPathCompatibility.XPath40Experimental)]
    public void EmptyInputDoesNotBypassTargetValidation(XPathCompatibility compatibility)
    {
        Assert.Equal("0", Evaluate("count(index-of((),'A',()))", compatibility));
        var error = Assert.ThrowsAny<Exception>(() => Evaluate("index-of((),(),())", compatibility));
        Assert.Contains("XPTY0004", error.Message);
    }

    /// <summary>Preserves value equality for NaN and the existing empty-string collation path.</summary>
    /// <param name="compatibility">The 3.1 or frozen 4.0 compatibility level.</param>
    [Theory]
    [InlineData(XPathCompatibility.XPath31)]
    [InlineData(XPathCompatibility.XPath40)]
    public void EqualityAndEmptyStringBehaviorAreUnchanged(XPathCompatibility compatibility)
    {
        Assert.Equal("0", Evaluate("count(index-of(xs:double('NaN'),xs:double('NaN')))", compatibility));
        Assert.Equal("1|3", Evaluate("string-join(index-of(('A','a','A'),'A',''),'|')", compatibility, Ascii));
    }
}
