// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 23 October 2026
// PURPOSE              : Verifies numeric literal digit groups and compatibility boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 23-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Parser;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Api.Tests;

/// <summary>Verifies numeric literal digit groups and compatibility boundaries.</summary>
public class NumericLiteralBoundaryTests
{
    /// <summary>Rejects separators at the edges of fractional and exponent digit groups.</summary>
    /// <param name="source">The malformed numeric expression.</param>
    /// <param name="compatibility">The requested language profile.</param>
    [Theory]
    [InlineData("1._2", XPathCompatibility.XPath30)]
    [InlineData("1._2", XPathCompatibility.XPath31)]
    [InlineData("1._2", XPathCompatibility.XPath40)]
    [InlineData("1._2", XPathCompatibility.XPath40Experimental)]
    [InlineData("1.2_", XPathCompatibility.XPath30)]
    [InlineData("1.2_", XPathCompatibility.XPath31)]
    [InlineData("1.2_", XPathCompatibility.XPath40)]
    [InlineData("1.2_", XPathCompatibility.XPath40Experimental)]
    [InlineData("1e_2", XPathCompatibility.XPath30)]
    [InlineData("1e_2", XPathCompatibility.XPath31)]
    [InlineData("1e_2", XPathCompatibility.XPath40)]
    [InlineData("1e_2", XPathCompatibility.XPath40Experimental)]
    [InlineData("1e2_", XPathCompatibility.XPath30)]
    [InlineData("1e2_", XPathCompatibility.XPath31)]
    [InlineData("1e2_", XPathCompatibility.XPath40)]
    [InlineData("1e2_", XPathCompatibility.XPath40Experimental)]
    [InlineData("1e+_2", XPathCompatibility.XPath30)]
    [InlineData("1e+_2", XPathCompatibility.XPath31)]
    [InlineData("1e+_2", XPathCompatibility.XPath40)]
    [InlineData("1e+_2", XPathCompatibility.XPath40Experimental)]
    [InlineData(".1_", XPathCompatibility.XPath30)]
    [InlineData(".1_", XPathCompatibility.XPath31)]
    [InlineData(".1_", XPathCompatibility.XPath40)]
    [InlineData(".1_", XPathCompatibility.XPath40Experimental)]
    [InlineData("1.2_e3", XPathCompatibility.XPath30)]
    [InlineData("1.2_e3", XPathCompatibility.XPath31)]
    [InlineData("1.2_e3", XPathCompatibility.XPath40)]
    [InlineData("1.2_e3", XPathCompatibility.XPath40Experimental)]
    [InlineData("1.2e3_", XPathCompatibility.XPath30)]
    [InlineData("1.2e3_", XPathCompatibility.XPath31)]
    [InlineData("1.2e3_", XPathCompatibility.XPath40)]
    [InlineData("1.2e3_", XPathCompatibility.XPath40Experimental)]
    public void Compile_InvalidDigitGroup_ThrowsXpst0003(string source, XPathCompatibility compatibility)
    {
        var failure = Assert.Throws<XPathParseException>(() => XPath31Expression.Compile(source,
            new CompileOptions { Compatibility = compatibility }));
        Assert.Contains("XPST0003", failure.Message);
    }

    /// <summary>Accepts internal repeated separators while preserving ordinary 3.1 numeric syntax.</summary>
    /// <param name="compatibility">The requested 4.0 profile.</param>
    [Theory]
    [InlineData(XPathCompatibility.XPath40)]
    [InlineData(XPathCompatibility.XPath40Experimental)]
    public void Evaluate_ValidDigitGroups_ReturnExpectedValues(XPathCompatibility compatibility)
    {
        var options = new CompileOptions { Compatibility = compatibility };
        Assert.Equal("12.34", XPath31Expression.Compile("1__2.3__4", options).Evaluate(new EvaluationContext()).ToString());
        Assert.Equal("1250", XPath31Expression.Compile("1_2.5e+0__2", options).Evaluate(new EvaluationContext()).ToString());
        Assert.Equal("12", XPath31Expression.Compile("1_2.", options).Evaluate(new EvaluationContext()).ToString());
        Assert.Equal("0.12", XPath31Expression.Compile(".1__2", options).Evaluate(new EvaluationContext()).ToString());
    }
}
