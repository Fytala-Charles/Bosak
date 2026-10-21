// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 18 October 2026
// PURPOSE              : Verifies explicit and absent wildcard layouts after element-to-map plan failures.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 18-10-2026   | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies wildcard fallback layout selection for element-to-map conversion.</summary>
public class ElementMapFallbackTests
{
    private static XdmValue Evaluate(string expression) => XPathExpression.Compile(expression,
        new CompileOptions { Compatibility = XPathCompatibility.XPath40 }).Evaluate(new EvaluationContext());

    /// <summary>Rejects a failed explicit layout when the wildcard entry supplies no layout.</summary>
    /// <param name="wildcard">The layout-free wildcard conversion-plan entry.</param>
    [Theory]
    [InlineData("{}")]
    [InlineData("{'type':'integer'}")]
    [InlineData("{'child':'b'}")]
    public void FailedExplicitLayout_RequiresActualFallbackLayout(string wildcard)
    {
        var error = Assert.Throws<InvalidOperationException>(() => Evaluate(
            "element-to-map(parse-xml('<a><b>7</b></a>')/*, " +
            "{'plan': {'a': {'layout':'simple'}, '*': " + wildcard + "}})"));
        Assert.Contains("FOJS0008", error.Message);
        Assert.DoesNotContain("Nullable object", error.Message);
    }

    /// <summary>Retains an explicitly requested XML fallback after the named layout fails.</summary>
    [Fact]
    public void FailedExplicitLayout_UsesXmlFallback()
    {
        Assert.Equal("<a><b>7</b></a>", Evaluate("""
            element-to-map(parse-xml('<a><b>7</b></a>')/*,
                {'plan': {'a': {'layout':'simple'}, '*': {'layout':'xml'}}})?a
            """).ToString());
    }

    /// <summary>Retains a deep-skip fallback without inventing output for the dropped element.</summary>
    [Fact]
    public void FailedExplicitLayout_UsesDeepSkipFallback()
    {
        Assert.True(Evaluate("""
            empty(element-to-map(parse-xml('<a><b>7</b></a>')/*,
                {'plan': {'a': {'layout':'simple'}, '*': {'layout':'deep-skip'}}}))
            """).BooleanValue);
    }

    /// <summary>Does not require an unused fallback when the explicit layout succeeds.</summary>
    [Fact]
    public void SuccessfulExplicitLayout_IgnoresLayoutFreeWildcard()
    {
        Assert.Equal("7", Evaluate("""
            element-to-map(parse-xml('<a>7</a>')/*,
                {'plan': {'a': {'layout':'simple'}, '*': {}}})?a
            """).ToString());
    }

    /// <summary>Infers the ordinary layout when no explicit layout has failed.</summary>
    [Fact]
    public void NoExplicitLayout_InfersLayoutAndRetainsWildcardType()
    {
        Assert.True(Evaluate("""
            element-to-map(parse-xml('<a>7</a>')/*,
                {'plan': {'*': {'type':'integer'}}})?a instance of xs:integer
            """).BooleanValue);
    }
}
