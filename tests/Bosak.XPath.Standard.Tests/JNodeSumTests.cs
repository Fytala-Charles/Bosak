// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies complete atomization and empty handling for fn:sum over JNodes.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies complete atomization and empty handling for fn:sum over JNodes.</summary>
public class JNodeSumTests
{
    private static string Eval(string expression) => XPath31Expression.Compile(expression,
        new CompileOptions { Compatibility = XPathCompatibility.XPath40 }).Evaluate(new EvaluationContext()).ToString();

    /// <summary>Includes every atomized value and handles emptiness after atomization.</summary>
    /// <param name="expression">The sum expression.</param>
    /// <param name="expected">The expected value.</param>
    [Theory]
    [InlineData("sum(jtree((5,6)))", "11")]
    [InlineData("sum(jtree((5,6)),99)", "11")]
    [InlineData("sum(array:members([1,(5,6),10]))", "22")]
    [InlineData("sum((jtree(()),jtree((5,6))))", "11")]
    [InlineData("sum(jtree(()))", "0")]
    [InlineData("sum(jtree(()),99)", "99")]
    [InlineData("empty(sum(jtree(()),()))", "true")]
    [InlineData("sum(array:members([(),()]),99)", "99")]
    [InlineData("sum(jtree([1,(2,3),[4]]))", "10")]
    [InlineData("sum(jtree(jtree((5,6))))", "11")]
    [InlineData("sum(jtree((xs:untypedAtomic('5'),xs:untypedAtomic('6'))))", "11")]
    [InlineData("sum(jtree((xs:dayTimeDuration('PT1S'),xs:dayTimeDuration('PT2S'))))", "PT3S")]
    [InlineData("sum(jtree((xs:yearMonthDuration('P1M'),xs:yearMonthDuration('P2M'))))", "P3M")]
    [InlineData("sum(jtree((-1,1)),99)", "0")]
    [InlineData("sum((1,2),99)", "3")]
    [InlineData("sum([1,(2,3),[4]])", "10")]
    public void Values(string expression, string expected) => Assert.Equal(expected, Eval(expression));

    /// <summary>Does not silently discard invalid later values or non-atomizable items.</summary>
    /// <param name="expression">The invalid sum.</param>
    /// <param name="code">The expected error code.</param>
    [Theory]
    [InlineData("sum(jtree((5,'bad')))", "FORG0006")]
    [InlineData("sum(jtree((5,map{})))", "FOTY0013")]
    [InlineData("sum(jtree((5,true())))", "FORG0006")]
    [InlineData("sum(jtree((5,xs:dayTimeDuration('PT1S'))))", "FORG0006")]
    public void Errors(string expression, string code) => Assert.Contains(code, Assert.ThrowsAny<Exception>(() => Eval(expression)).ToString());
}
