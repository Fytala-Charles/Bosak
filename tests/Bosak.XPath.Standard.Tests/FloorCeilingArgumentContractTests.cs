// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 October 2026
// PURPOSE              : Verifies floor and ceiling numeric argument contracts.
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


using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>Verifies floor and ceiling numeric argument contracts.</summary>
public sealed class FloorCeilingArgumentContractTests
{
    /// <summary>Provides predeclared argument conversion, node and identity controls.</summary>
    /// <returns>Profile, expression, expected value or error, and error flag.</returns>
    public static IEnumerable<object[]> Cases()
    {
        var cases = new (string Expression, string Expected, bool Error)[]
        {
            ("floor(-2.5)", "-3", false),
            ("floor#1(-2.5)", "-3", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(-2.5)", "-3", false),
            ("(function($x){floor($x)})(-2.5)", "-3", false),
            ("floor(())", "()", false),
            ("floor#1(())", "()", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(())", "()", false),
            ("(function($x){floor($x)})(())", "()", false),
            ("floor([])", "()", false),
            ("floor#1([])", "()", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([])", "()", false),
            ("(function($x){floor($x)})([])", "()", false),
            ("floor([()])", "()", false),
            ("floor#1([()])", "()", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([()])", "()", false),
            ("(function($x){floor($x)})([()])", "()", false),
            ("floor([[],[]])", "()", false),
            ("floor#1([[],[]])", "()", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([[],[]])", "()", false),
            ("(function($x){floor($x)})([[],[]])", "()", false),
            ("floor([-2.5])", "-3", false),
            ("floor#1([-2.5])", "-3", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([-2.5])", "-3", false),
            ("(function($x){floor($x)})([-2.5])", "-3", false),
            ("floor([[[]],[-2.5],[]])", "-3", false),
            ("floor#1([[[]],[-2.5],[]])", "-3", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([[[]],[-2.5],[]])", "-3", false),
            ("(function($x){floor($x)})([[[]],[-2.5],[]])", "-3", false),
            ("floor([[],[-2.5],[]])", "-3", false),
            ("floor#1([[],[-2.5],[]])", "-3", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([[],[-2.5],[]])", "-3", false),
            ("(function($x){floor($x)})([[],[-2.5],[]])", "-3", false),
            ("floor(xs:untypedAtomic('-2.5'))", "-3", false),
            ("floor#1(xs:untypedAtomic('-2.5'))", "-3", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(xs:untypedAtomic('-2.5'))", "-3", false),
            ("(function($x){floor($x)})(xs:untypedAtomic('-2.5'))", "-3", false),
            ("floor([xs:untypedAtomic('-2.5')])", "-3", false),
            ("floor#1([xs:untypedAtomic('-2.5')])", "-3", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([xs:untypedAtomic('-2.5')])", "-3", false),
            ("(function($x){floor($x)})([xs:untypedAtomic('-2.5')])", "-3", false),
            ("floor(xs:untypedAtomic('bad'))", "FORG0001", true),
            ("floor#1(xs:untypedAtomic('bad'))", "FORG0001", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(xs:untypedAtomic('bad'))", "FORG0001", true),
            ("(function($x){floor($x)})(xs:untypedAtomic('bad'))", "FORG0001", true),
            ("floor([xs:untypedAtomic('bad')])", "FORG0001", true),
            ("floor#1([xs:untypedAtomic('bad')])", "FORG0001", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([xs:untypedAtomic('bad')])", "FORG0001", true),
            ("(function($x){floor($x)})([xs:untypedAtomic('bad')])", "FORG0001", true),
            ("floor('-2.5')", "XPTY0004", true),
            ("floor#1('-2.5')", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)('-2.5')", "XPTY0004", true),
            ("(function($x){floor($x)})('-2.5')", "XPTY0004", true),
            ("floor(['-2.5'])", "XPTY0004", true),
            ("floor#1(['-2.5'])", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(['-2.5'])", "XPTY0004", true),
            ("(function($x){floor($x)})(['-2.5'])", "XPTY0004", true),
            ("floor(true())", "XPTY0004", true),
            ("floor#1(true())", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(true())", "XPTY0004", true),
            ("(function($x){floor($x)})(true())", "XPTY0004", true),
            ("floor([true()])", "XPTY0004", true),
            ("floor#1([true()])", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([true()])", "XPTY0004", true),
            ("(function($x){floor($x)})([true()])", "XPTY0004", true),
            ("floor((-2.5,-3.5))", "XPTY0004", true),
            ("floor#1((-2.5,-3.5))", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)((-2.5,-3.5))", "XPTY0004", true),
            ("(function($x){floor($x)})((-2.5,-3.5))", "XPTY0004", true),
            ("floor([-2.5,-3.5])", "XPTY0004", true),
            ("floor#1([-2.5,-3.5])", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([-2.5,-3.5])", "XPTY0004", true),
            ("(function($x){floor($x)})([-2.5,-3.5])", "XPTY0004", true),
            ("floor([[[],[-2.5,-3.5]],[]])", "XPTY0004", true),
            ("floor#1([[[],[-2.5,-3.5]],[]])", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([[[],[-2.5,-3.5]],[]])", "XPTY0004", true),
            ("(function($x){floor($x)})([[[],[-2.5,-3.5]],[]])", "XPTY0004", true),
            ("floor(map{})", "FOTY0013", true),
            ("floor#1(map{})", "FOTY0013", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(map{})", "FOTY0013", true),
            ("(function($x){floor($x)})(map{})", "FOTY0013", true),
            ("floor([map{}])", "FOTY0013", true),
            ("floor#1([map{}])", "FOTY0013", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([map{}])", "FOTY0013", true),
            ("(function($x){floor($x)})([map{}])", "FOTY0013", true),
            ("floor(function(){1})", "FOTY0013", true),
            ("floor#1(function(){1})", "FOTY0013", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(function(){1})", "FOTY0013", true),
            ("(function($x){floor($x)})(function(){1})", "FOTY0013", true),
            ("floor([function(){1}])", "FOTY0013", true),
            ("floor#1([function(){1}])", "FOTY0013", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([function(){1}])", "FOTY0013", true),
            ("(function($x){floor($x)})([function(){1}])", "FOTY0013", true),
            ("floor(@n)", "-3", false),
            ("floor#1(@n)", "-3", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(@n)", "-3", false),
            ("(function($x){floor($x)})(@n)", "-3", false),
            ("floor([[],@n,[]])", "-3", false),
            ("floor#1([[],@n,[]])", "-3", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([[],@n,[]])", "-3", false),
            ("(function($x){floor($x)})([[],@n,[]])", "-3", false),
            ("floor(n)", "XPTY0004", true),
            ("floor#1(n)", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(n)", "XPTY0004", true),
            ("(function($x){floor($x)})(n)", "XPTY0004", true),
            ("floor([n])", "XPTY0004", true),
            ("floor#1([n])", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([n])", "XPTY0004", true),
            ("(function($x){floor($x)})([n])", "XPTY0004", true),
            ("floor(@bad)", "FORG0001", true),
            ("floor#1(@bad)", "FORG0001", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(@bad)", "FORG0001", true),
            ("(function($x){floor($x)})(@bad)", "FORG0001", true),
            ("floor(missing)", "()", false),
            ("floor#1(missing)", "()", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)(missing)", "()", false),
            ("(function($x){floor($x)})(missing)", "()", false),
            ("floor([xs:integer('-2')]) instance of xs:integer", "true", false),
            ("floor#1([xs:integer('-2')]) instance of xs:integer", "true", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([xs:integer('-2')]) instance of xs:integer", "true", false),
            ("(function($x){floor($x)})([xs:integer('-2')]) instance of xs:integer", "true", false),
            ("floor([xs:decimal('-2')]) instance of xs:decimal", "true", false),
            ("floor#1([xs:decimal('-2')]) instance of xs:decimal", "true", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([xs:decimal('-2')]) instance of xs:decimal", "true", false),
            ("(function($x){floor($x)})([xs:decimal('-2')]) instance of xs:decimal", "true", false),
            ("floor([xs:float('-2')]) instance of xs:float", "true", false),
            ("floor#1([xs:float('-2')]) instance of xs:float", "true", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([xs:float('-2')]) instance of xs:float", "true", false),
            ("(function($x){floor($x)})([xs:float('-2')]) instance of xs:float", "true", false),
            ("floor([xs:double('-2')]) instance of xs:double", "true", false),
            ("floor#1([xs:double('-2')]) instance of xs:double", "true", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','floor'),1)([xs:double('-2')]) instance of xs:double", "true", false),
            ("(function($x){floor($x)})([xs:double('-2')]) instance of xs:double", "true", false),
            ("(function($f as function(xs:numeric?) as xs:numeric?){local-name-from-QName(function-name($f))})(floor#1)", "floor", false),
            ("ceiling(-2.5)", "-2", false),
            ("ceiling#1(-2.5)", "-2", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(-2.5)", "-2", false),
            ("(function($x){ceiling($x)})(-2.5)", "-2", false),
            ("ceiling(())", "()", false),
            ("ceiling#1(())", "()", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(())", "()", false),
            ("(function($x){ceiling($x)})(())", "()", false),
            ("ceiling([])", "()", false),
            ("ceiling#1([])", "()", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([])", "()", false),
            ("(function($x){ceiling($x)})([])", "()", false),
            ("ceiling([()])", "()", false),
            ("ceiling#1([()])", "()", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([()])", "()", false),
            ("(function($x){ceiling($x)})([()])", "()", false),
            ("ceiling([[],[]])", "()", false),
            ("ceiling#1([[],[]])", "()", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([[],[]])", "()", false),
            ("(function($x){ceiling($x)})([[],[]])", "()", false),
            ("ceiling([-2.5])", "-2", false),
            ("ceiling#1([-2.5])", "-2", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([-2.5])", "-2", false),
            ("(function($x){ceiling($x)})([-2.5])", "-2", false),
            ("ceiling([[[]],[-2.5],[]])", "-2", false),
            ("ceiling#1([[[]],[-2.5],[]])", "-2", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([[[]],[-2.5],[]])", "-2", false),
            ("(function($x){ceiling($x)})([[[]],[-2.5],[]])", "-2", false),
            ("ceiling([[],[-2.5],[]])", "-2", false),
            ("ceiling#1([[],[-2.5],[]])", "-2", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([[],[-2.5],[]])", "-2", false),
            ("(function($x){ceiling($x)})([[],[-2.5],[]])", "-2", false),
            ("ceiling(xs:untypedAtomic('-2.5'))", "-2", false),
            ("ceiling#1(xs:untypedAtomic('-2.5'))", "-2", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(xs:untypedAtomic('-2.5'))", "-2", false),
            ("(function($x){ceiling($x)})(xs:untypedAtomic('-2.5'))", "-2", false),
            ("ceiling([xs:untypedAtomic('-2.5')])", "-2", false),
            ("ceiling#1([xs:untypedAtomic('-2.5')])", "-2", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([xs:untypedAtomic('-2.5')])", "-2", false),
            ("(function($x){ceiling($x)})([xs:untypedAtomic('-2.5')])", "-2", false),
            ("ceiling(xs:untypedAtomic('bad'))", "FORG0001", true),
            ("ceiling#1(xs:untypedAtomic('bad'))", "FORG0001", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(xs:untypedAtomic('bad'))", "FORG0001", true),
            ("(function($x){ceiling($x)})(xs:untypedAtomic('bad'))", "FORG0001", true),
            ("ceiling([xs:untypedAtomic('bad')])", "FORG0001", true),
            ("ceiling#1([xs:untypedAtomic('bad')])", "FORG0001", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([xs:untypedAtomic('bad')])", "FORG0001", true),
            ("(function($x){ceiling($x)})([xs:untypedAtomic('bad')])", "FORG0001", true),
            ("ceiling('-2.5')", "XPTY0004", true),
            ("ceiling#1('-2.5')", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)('-2.5')", "XPTY0004", true),
            ("(function($x){ceiling($x)})('-2.5')", "XPTY0004", true),
            ("ceiling(['-2.5'])", "XPTY0004", true),
            ("ceiling#1(['-2.5'])", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(['-2.5'])", "XPTY0004", true),
            ("(function($x){ceiling($x)})(['-2.5'])", "XPTY0004", true),
            ("ceiling(true())", "XPTY0004", true),
            ("ceiling#1(true())", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(true())", "XPTY0004", true),
            ("(function($x){ceiling($x)})(true())", "XPTY0004", true),
            ("ceiling([true()])", "XPTY0004", true),
            ("ceiling#1([true()])", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([true()])", "XPTY0004", true),
            ("(function($x){ceiling($x)})([true()])", "XPTY0004", true),
            ("ceiling((-2.5,-3.5))", "XPTY0004", true),
            ("ceiling#1((-2.5,-3.5))", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)((-2.5,-3.5))", "XPTY0004", true),
            ("(function($x){ceiling($x)})((-2.5,-3.5))", "XPTY0004", true),
            ("ceiling([-2.5,-3.5])", "XPTY0004", true),
            ("ceiling#1([-2.5,-3.5])", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([-2.5,-3.5])", "XPTY0004", true),
            ("(function($x){ceiling($x)})([-2.5,-3.5])", "XPTY0004", true),
            ("ceiling([[[],[-2.5,-3.5]],[]])", "XPTY0004", true),
            ("ceiling#1([[[],[-2.5,-3.5]],[]])", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([[[],[-2.5,-3.5]],[]])", "XPTY0004", true),
            ("(function($x){ceiling($x)})([[[],[-2.5,-3.5]],[]])", "XPTY0004", true),
            ("ceiling(map{})", "FOTY0013", true),
            ("ceiling#1(map{})", "FOTY0013", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(map{})", "FOTY0013", true),
            ("(function($x){ceiling($x)})(map{})", "FOTY0013", true),
            ("ceiling([map{}])", "FOTY0013", true),
            ("ceiling#1([map{}])", "FOTY0013", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([map{}])", "FOTY0013", true),
            ("(function($x){ceiling($x)})([map{}])", "FOTY0013", true),
            ("ceiling(function(){1})", "FOTY0013", true),
            ("ceiling#1(function(){1})", "FOTY0013", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(function(){1})", "FOTY0013", true),
            ("(function($x){ceiling($x)})(function(){1})", "FOTY0013", true),
            ("ceiling([function(){1}])", "FOTY0013", true),
            ("ceiling#1([function(){1}])", "FOTY0013", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([function(){1}])", "FOTY0013", true),
            ("(function($x){ceiling($x)})([function(){1}])", "FOTY0013", true),
            ("ceiling(@n)", "-2", false),
            ("ceiling#1(@n)", "-2", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(@n)", "-2", false),
            ("(function($x){ceiling($x)})(@n)", "-2", false),
            ("ceiling([[],@n,[]])", "-2", false),
            ("ceiling#1([[],@n,[]])", "-2", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([[],@n,[]])", "-2", false),
            ("(function($x){ceiling($x)})([[],@n,[]])", "-2", false),
            ("ceiling(n)", "XPTY0004", true),
            ("ceiling#1(n)", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(n)", "XPTY0004", true),
            ("(function($x){ceiling($x)})(n)", "XPTY0004", true),
            ("ceiling([n])", "XPTY0004", true),
            ("ceiling#1([n])", "XPTY0004", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([n])", "XPTY0004", true),
            ("(function($x){ceiling($x)})([n])", "XPTY0004", true),
            ("ceiling(@bad)", "FORG0001", true),
            ("ceiling#1(@bad)", "FORG0001", true),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(@bad)", "FORG0001", true),
            ("(function($x){ceiling($x)})(@bad)", "FORG0001", true),
            ("ceiling(missing)", "()", false),
            ("ceiling#1(missing)", "()", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)(missing)", "()", false),
            ("(function($x){ceiling($x)})(missing)", "()", false),
            ("ceiling([xs:integer('-2')]) instance of xs:integer", "true", false),
            ("ceiling#1([xs:integer('-2')]) instance of xs:integer", "true", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([xs:integer('-2')]) instance of xs:integer", "true", false),
            ("(function($x){ceiling($x)})([xs:integer('-2')]) instance of xs:integer", "true", false),
            ("ceiling([xs:decimal('-2')]) instance of xs:decimal", "true", false),
            ("ceiling#1([xs:decimal('-2')]) instance of xs:decimal", "true", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([xs:decimal('-2')]) instance of xs:decimal", "true", false),
            ("(function($x){ceiling($x)})([xs:decimal('-2')]) instance of xs:decimal", "true", false),
            ("ceiling([xs:float('-2')]) instance of xs:float", "true", false),
            ("ceiling#1([xs:float('-2')]) instance of xs:float", "true", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([xs:float('-2')]) instance of xs:float", "true", false),
            ("(function($x){ceiling($x)})([xs:float('-2')]) instance of xs:float", "true", false),
            ("ceiling([xs:double('-2')]) instance of xs:double", "true", false),
            ("ceiling#1([xs:double('-2')]) instance of xs:double", "true", false),
            ("function-lookup(QName('http://www.w3.org/2005/xpath-functions','ceiling'),1)([xs:double('-2')]) instance of xs:double", "true", false),
            ("(function($x){ceiling($x)})([xs:double('-2')]) instance of xs:double", "true", false),
            ("(function($f as function(xs:numeric?) as xs:numeric?){local-name-from-QName(function-name($f))})(ceiling#1)", "ceiling", false),
        };
        foreach (var profile in new[] { XPathCompatibility.XPath31, XPathCompatibility.XPath40, XPathCompatibility.XPath40Experimental })
            foreach (var row in cases)
                yield return new object[] { profile, row.Expression, row.Expected, row.Error };
    }

    /// <summary>Checks public-call values, primitive types, signs, and required errors.</summary>
    /// <param name="profile">The XPath compatibility profile.</param>
    /// <param name="expression">The complete predicate or error expression.</param>
    /// <param name="expected">The expected value or error code.</param>
    /// <param name="error">Whether evaluation must fail.</param>
    [Theory, MemberData(nameof(Cases))]
    public void PreservesIntegerResults(XPathCompatibility profile, string expression, string expected, bool error)
    {
        var document = XDocument.Parse("<root n='-2.5' bad='bad'><n>-2.5</n><n>-3.5</n></root>");
        var context = new EvaluationContext().WithFocus(XdmValue.FromNode(new XDocumentNode(document.Root!)), 1, 1);
        string Evaluate() => XPath31Expression.Compile(expression, new CompileOptions { Compatibility = profile }).Evaluate(context).ToString();
        if (error) Assert.Contains(expected, Assert.ThrowsAny<Exception>(() => Evaluate()).Message);
        else Assert.Equal(expected, Evaluate());
    }

    /// <summary>Retains supported static backwards-compatible first-item and empty behavior.</summary>
    /// <param name="expression">The static function call.</param>
    /// <param name="expected">The expected value.</param>
    [Theory]
    [InlineData("floor((-2.5,-3.5))", "-3")]
    [InlineData("ceiling((-2.5,-3.5))", "-2")]
    [InlineData("floor(())", "NaN")]
    [InlineData("ceiling(())", "NaN")]
    public void BackwardsCompatibleControls(string expression, string expected)
    {
        Assert.Equal(expected, XPath31Expression.Compile(expression, new CompileOptions { BackwardsCompatible = true }).Evaluate(new EvaluationContext { BackwardsCompatible = true }).ToString());
    }
}
