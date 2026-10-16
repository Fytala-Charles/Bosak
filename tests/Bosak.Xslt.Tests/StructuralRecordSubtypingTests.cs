// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies structural record subtyping in function signatures.
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
//                      | Charles Korthout | 0.2   | 16-10-2026     | Separate alias occurrences and add zero-argument atomic signature controls.               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies structural record subtyping in function signatures.</summary>
public class StructuralRecordSubtypingTests
{
    /// <summary>Checks parameter contravariance and return covariance through XPath and both XSLT profiles.</summary>
    /// <param name="subtype">The proposed narrower sequence type.</param>
    /// <param name="supertype">The proposed wider sequence type.</param>
    /// <param name="expected">Whether the subtype relationship holds.</param>
    [Theory]
    [InlineData("xs:integer", "xs:decimal", true)]
    [InlineData("xs:decimal", "xs:integer", false)]
    [InlineData("record(x as xs:integer)", "record(x as xs:decimal)", true)]
    [InlineData("record(x as xs:decimal)", "record(x as xs:integer)", false)]
    [InlineData("record(x as xs:integer)", "record(*)", true)]
    [InlineData("record(*)", "record(x as xs:integer)", false)]
    [InlineData("record()", "record(*)", true)]
    [InlineData("record(*)", "record()", false)]
    [InlineData("record()", "record()", true)]
    [InlineData("record(x as xs:integer,y as xs:NCName)", "record(y as xs:string,x as xs:decimal)", true)]
    [InlineData("record(x,y)", "record(x)", false)]
    [InlineData("record(x)", "record(x,y)", false)]
    [InlineData("record(Name as xs:string)", "record(name as xs:string)", false)]
    [InlineData("record('First Name' as xs:NCName)", "record('First Name' as xs:string)", true)]
    [InlineData("record('First Name')", "record('first name')", false)]
    [InlineData("record(child as record(value as xs:integer))", "record(child as record(value as xs:decimal))", true)]
    [InlineData("record(child as record(Name))", "record(child as record(name))", false)]
    [InlineData("array(record(Name))", "array(record(name))", false)]
    [InlineData("record(children as array(record(value as xs:integer)))", "record(children as array(record(value as xs:decimal)))", true)]
    [InlineData("record(children as map(xs:string,record(value as xs:integer)))", "record(children as map(xs:string,record(value as xs:decimal)))", true)]
    [InlineData("record(f as function(xs:decimal) as xs:integer)", "record(f as function(xs:integer) as xs:decimal)", true)]
    [InlineData("record(x as xs:integer?)", "record(x as xs:decimal*)", true)]
    [InlineData("record(x as xs:integer+)", "record(x as xs:decimal*)", true)]
    [InlineData("record(x as xs:integer*)", "record(x as xs:decimal+)", false)]
    [InlineData("record(x as xs:integer)", "record(x as xs:decimal?)", true)]
    [InlineData("record(x as xs:integer?)", "record(x as xs:decimal)", false)]
    [InlineData("record(x)", "record(x as item()*)", true)]
    [InlineData("record(x as empty-sequence())", "record(x as xs:string?)", true)]
    [InlineData("record(x as xs:string?)", "record(x as empty-sequence())", false)]
    [InlineData("record(x)?", "record(x)*", true)]
    [InlineData("record(x)*", "record(x)?", false)]
    [InlineData("record(x)+", "record(x)*", true)]
    [InlineData("record(x)", "item()", true)]
    [InlineData("item()", "record(x)", false)]
    [InlineData("record(x as xs:string)", "record(x as xs:integer)", false)]
    public void FunctionSignatureRelations(string subtype, string supertype, bool expected)
    {
        foreach (var expression in new[]
        {
            $"function($a as {supertype}) as xs:integer {{3}} instance of function({subtype}) as xs:integer",
            $"function() as {subtype} {{error()}} instance of function() as {supertype}"
        })
        {
            var value = XPath31Expression.Compile(expression, new CompileOptions { Compatibility = XPathCompatibility.XPath40 }).Evaluate(new EvaluationContext());
            Assert.Equal(expected, value.BooleanValue);
            foreach (var profile in new[] { XsltProcessorProfile.Legacy, XsltProcessorProfile.Xslt40Draft })
                Assert.Equal(expected, Evaluate(expression, profile));
        }
        var subOccurrence = subtype[^1] is '?' or '+' or '*' ? subtype[^1..] : "";
        var superOccurrence = supertype[^1] is '?' or '+' or '*' ? supertype[^1..] : "";
        foreach (var profile in new[] { XsltProcessorProfile.Legacy, XsltProcessorProfile.Xslt40Draft })
            Assert.Equal(expected, Evaluate($"function($a as t:Super{superOccurrence}) as xs:integer {{3}} instance of function(t:Sub{subOccurrence}) as xs:integer", profile,
                new XElement(Xsl + "item-type", new XAttribute("name", "t:Sub"), new XAttribute("as", subOccurrence.Length == 0 ? subtype : subtype[..^1])),
                new XElement(Xsl + "item-type", new XAttribute("name", "t:Super"), new XAttribute("as", superOccurrence.Length == 0 ? supertype : supertype[..^1]))));
    }

    /// <summary>Preserves the distinction between plain maps and annotated structural records.</summary>
    [Fact]
    public void InstanceTestingDoesNotCoerceMaps()
    {
        Assert.False(Evaluate("map{'x':1} instance of record(x as xs:integer)", XsltProcessorProfile.Xslt40Draft));
        Assert.True(Evaluate("(let $r as record(x as xs:integer) := map{'x':1} return $r) instance of record(x as xs:integer)", XsltProcessorProfile.Xslt40Draft));
    }

    private static readonly XNamespace Xsl = "http://www.w3.org/1999/XSL/Transform";
    private static bool Evaluate(string expression, XsltProcessorProfile profile, params XElement[] declarations)
    {
        var stylesheet = new XElement(Xsl + "stylesheet", new XAttribute("version", "4.0"),
            new XAttribute(XNamespace.Xmlns + "xsl", Xsl), new XAttribute(XNamespace.Xmlns + "xs", "http://www.w3.org/2001/XMLSchema"),
            new XAttribute(XNamespace.Xmlns + "t", "urn:types"), declarations,
            new XElement(Xsl + "template", new XAttribute("name", "start"), new XElement(Xsl + "sequence", new XAttribute("select", expression))));
        return new XsltCompiler { ProcessorProfile = profile }.Compile(stylesheet.ToString()).Transform(null, initialTemplate: "start", rawResult: true).BooleanValue;
    }
}
