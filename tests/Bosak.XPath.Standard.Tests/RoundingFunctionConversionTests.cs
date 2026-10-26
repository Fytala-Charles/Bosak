// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 October 2026
// PURPOSE              : Verifies rounding argument atomization, cardinality, precision and function identity.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 26-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using System.Xml.Linq;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies rounding argument atomization, cardinality, precision and function identity.</summary>
public sealed class RoundingFunctionConversionTests
{
    /// <summary>Provides independent rounding argument contract expectations across public profiles.</summary>
    /// <returns>Profile, expression, expected value or error and error flag.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var numericRows = new (string Argument, string Round, string Even, bool Error)[]
        {
            ("2.5", "3", "2", false),
            ("()", "()", "()", false),
            ("[]", "()", "()", false),
            ("[()]", "()", "()", false),
            ("[[],[]]", "()", "()", false),
            ("[-2.5]", "-2", "-2", false),
            ("[[],[2.5],[]]", "3", "2", false),
            ("(2.5,3.5)", "XPTY0004", "XPTY0004", true),
            ("[2.5,3.5]", "XPTY0004", "XPTY0004", true),
            ("[[2.5,3.5]]", "XPTY0004", "XPTY0004", true),
            ("xs:untypedAtomic('2.5')", "3", "2", false),
            ("[xs:untypedAtomic('2.5')]", "3", "2", false),
            ("xs:untypedAtomic('bad')", "FORG0001", "FORG0001", true),
            ("'2.5'", "XPTY0004", "XPTY0004", true),
            ("true()", "XPTY0004", "XPTY0004", true),
            ("map{}", "FOTY0013", "FOTY0013", true),
            ("[map{}]", "FOTY0013", "FOTY0013", true),
            ("function(){1}", "FOTY0013", "FOTY0013", true),
            ("[function(){1}]", "FOTY0013", "FOTY0013", true),
        };
        var precisionRows = new (string Argument, string Round, string Even, bool Error)[]
        {
            ("0", "13", "12", false),
            ("1", "12.5", "12.5", false),
            ("-1", "10", "10", false),
            ("[1]", "12.5", "12.5", false),
            ("[[1]]", "12.5", "12.5", false),
            ("[[],1,[]]", "12.5", "12.5", false),
            ("xs:untypedAtomic('1')", "12.5", "12.5", false),
            ("[xs:untypedAtomic('1')]", "12.5", "12.5", false),
            ("'1'", "XPTY0004", "XPTY0004", true),
            ("xs:decimal('1')", "XPTY0004", "XPTY0004", true),
            ("1e0", "XPTY0004", "XPTY0004", true),
            ("()", "XPTY0004", "XPTY0004", true),
            ("[]", "XPTY0004", "XPTY0004", true),
            ("[()]", "XPTY0004", "XPTY0004", true),
            ("(0,1)", "XPTY0004", "XPTY0004", true),
            ("[0,1]", "XPTY0004", "XPTY0004", true),
            ("map{}", "FOTY0013", "FOTY0013", true),
            ("[map{}]", "FOTY0013", "FOTY0013", true),
            ("function(){1}", "FOTY0013", "FOTY0013", true),
            ("xs:untypedAtomic('bad')", "FORG0001", "FORG0001", true),
        };
        var controls = new (string Expression, string Expected, bool Error)[]
        {
            ("round([xs:integer('2')]) instance of xs:integer", "true", false),
            ("round([xs:decimal('2')]) instance of xs:decimal", "true", false),
            ("round([xs:float('2')]) instance of xs:float", "true", false),
            ("round([xs:double('2')]) instance of xs:double", "true", false),
            ("local-name-from-QName((function($f as function(xs:numeric?) as item()*){function-name($f)})(round#1))", "round", false),
            ("string(round(xs:double('NaN')))", "NaN", false),
            ("string(round(xs:double('INF')))", "INF", false),
            ("round([xs:integer('2')],[0]) instance of xs:integer", "true", false),
            ("round([xs:decimal('2')],[0]) instance of xs:decimal", "true", false),
            ("round([xs:float('2')],[0]) instance of xs:float", "true", false),
            ("round([xs:double('2')],[0]) instance of xs:double", "true", false),
            ("local-name-from-QName((function($f as function(item()*,xs:integer) as item()*){function-name($f)})(round#2))", "round", false),
            ("string(round(xs:double('NaN'),0))", "NaN", false),
            ("string(round(xs:double('INF'),0))", "INF", false),
            ("round('bad',1001)", "XPTY0004", true),
            ("round(xs:untypedAtomic('bad'),1001)", "FORG0001", true),
            ("round(xs:untypedAtomic('2.5'),1001) instance of xs:double", "true", false),
            ("round-half-to-even([xs:integer('2')]) instance of xs:integer", "true", false),
            ("round-half-to-even([xs:decimal('2')]) instance of xs:decimal", "true", false),
            ("round-half-to-even([xs:float('2')]) instance of xs:float", "true", false),
            ("round-half-to-even([xs:double('2')]) instance of xs:double", "true", false),
            ("local-name-from-QName((function($f as function(item()*) as item()*){function-name($f)})(round-half-to-even#1))", "round-half-to-even", false),
            ("string(round-half-to-even(xs:double('NaN')))", "NaN", false),
            ("string(round-half-to-even(xs:double('INF')))", "INF", false),
            ("round-half-to-even([xs:integer('2')],[0]) instance of xs:integer", "true", false),
            ("round-half-to-even([xs:decimal('2')],[0]) instance of xs:decimal", "true", false),
            ("round-half-to-even([xs:float('2')],[0]) instance of xs:float", "true", false),
            ("round-half-to-even([xs:double('2')],[0]) instance of xs:double", "true", false),
            ("local-name-from-QName((function($f as function(item()*,xs:integer) as item()*){function-name($f)})(round-half-to-even#2))", "round-half-to-even", false),
            ("string(round-half-to-even(xs:double('NaN'),0))", "NaN", false),
            ("string(round-half-to-even(xs:double('INF'),0))", "INF", false),
            ("round-half-to-even('bad',1001)", "XPTY0004", true),
            ("round-half-to-even(xs:untypedAtomic('bad'),1001)", "FORG0001", true),
            ("round-half-to-even(xs:untypedAtomic('2.5'),1001) instance of xs:double", "true", false),
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
        {
            foreach (var (name, arity) in new[] { ("round", 1), ("round", 2), ("round-half-to-even", 1), ("round-half-to-even", 2) })
                foreach (var row in numericRows)
                    foreach (var expression in CallForms(name, row.Argument, arity == 1 ? null : "0"))
                        yield return new object[] { profile, expression, name == "round" ? row.Round : row.Even, row.Error };
            foreach (var name in new[] { "round", "round-half-to-even" })
                foreach (var row in precisionRows)
                    foreach (var expression in CallForms(name, "12.5", row.Argument))
                        yield return new object[] { profile, expression, name == "round" ? row.Round : row.Even, row.Error };
            foreach (var row in controls)
                yield return new object[] { profile, row.Expression, row.Expected, row.Error };
        }
    }

    private static IEnumerable<string> CallForms(string name, string argument, string? precision)
    {
        int arity = precision is null ? 1 : 2;
        string args = precision is null ? argument : argument + "," + precision;
        string parameters = arity == 1 ? "$x" : "$x,$p";
        yield return $"{name}({args})";
        yield return $"{name}#{arity}({args})";
        yield return $"function-lookup(QName('http://www.w3.org/2005/xpath-functions','{name}'),{arity})({args})";
        yield return $"(function({parameters}){{{name}({parameters})}})({args})";
    }

    /// <summary>Checks direct and dynamic rounding calls against independently expected values and errors.</summary>
    /// <param name="profile">The compatibility profile.</param>
    /// <param name="expression">The complete function call.</param>
    /// <param name="expected">The expected value or error code.</param>
    /// <param name="error">Whether evaluation must fail.</param>
    [Theory, MemberData(nameof(Cases))]
    public void ConvertsRoundingArguments(XPathCompatibility profile, string expression, string expected, bool error)
    {
        string Evaluate() => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile })
            .Evaluate(new EvaluationContext()).ToString();
        if (error)
            Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate()).Message);
        else
            Assert.Equal(expected, Evaluate());
    }

    /// <summary>Retains supported static backwards-compatible rounding behavior.</summary>
    /// <param name="expression">The static rounding call.</param>
    /// <param name="expected">The expected value.</param>
    [Theory]
    [InlineData("round((2.5,3.5))", "3")]
    [InlineData("round((2.5,3.5),0)", "3")]
    [InlineData("round-half-to-even((2.5,3.5))", "2")]
    [InlineData("round-half-to-even((2.5,3.5),0)", "2")]
    [InlineData("round(())", "NaN")]
    [InlineData("round((),0)", "NaN")]
    [InlineData("round-half-to-even(())", "()")]
    [InlineData("round-half-to-even((),0)", "()")]
    public void PreservesBackwardsCompatibleCalls(string expression, string expected)
    {
        var options = new CompileOptions { BackwardsCompatible = true };
        var context = new EvaluationContext { BackwardsCompatible = true };
        Assert.Equal(expected, XPath31Expression.Compile(expression, options).Evaluate(context).ToString());
    }

    /// <summary>Provides node atomization controls for each rounding signature and profile.</summary>
    /// <returns>Profile, function name and arity.</returns>
    public static IEnumerable<object[]> NodeCases()
    {
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var (name, arity) in new[] { ("round", 1), ("round", 2), ("round-half-to-even", 1), ("round-half-to-even", 2) })
                yield return new object[] { profile, name, arity };
    }

    /// <summary>Atomizes untyped attributes, including array members, and rejects multiple values.</summary>
    /// <param name="profile">The compatibility profile.</param>
    /// <param name="name">The rounding function name.</param>
    /// <param name="arity">The function arity.</param>
    [Theory, MemberData(nameof(NodeCases))]
    public void ConvertsUntypedNodeArguments(XPathCompatibility profile, string name, int arity)
    {
        var doc = XDocument.Parse("<root n='2.5' p='0'><n>2.5</n><n>3.5</n></root>");
        var context = new EvaluationContext().WithFocus(XdmValue.FromNode(new XDocumentNode(doc.Root!)), 1, 1);
        string Evaluate(string source) => XPath31Expression.Compile(source, new CompileOptions { Compatibility = profile }).Evaluate(context).ToString();
        foreach (var argument in new[] { "@n", "[[],@n,[]]" })
            foreach (var source in CallForms(name, argument, arity == 1 ? null : "[[],@p,[]]"))
                Assert.Equal(name == "round" ? "3" : "2", Evaluate(source));
        foreach (var source in CallForms(name, "n", arity == 1 ? null : "@p"))
            Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Evaluate(source)).Message);
        if (arity == 2)
            foreach (var source in CallForms(name, "@n", "(@p,@p)"))
                Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Evaluate(source)).Message);
    }
}
