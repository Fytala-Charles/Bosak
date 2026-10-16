// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies structural record-to-map and generic function subtype relationships.
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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies structural record-to-map and generic function subtype relationships.</summary>
public class RecordMapSubtypingTests
{
    /// <summary>Checks map value covariance and key constraints without changing typed callable-record contracts.</summary>
    /// <param name="subtype">The proposed narrower sequence type.</param>
    /// <param name="supertype">The proposed wider sequence type.</param>
    /// <param name="expected">Whether the subtype relationship holds.</param>
    [Theory]
    [InlineData("record(x as xs:integer)", "map(xs:string,xs:decimal)", true)]
    [InlineData("record(x as xs:integer,y as xs:string)", "map(xs:string,xs:anyAtomicType)", true)]
    [InlineData("record()", "map(xs:string,xs:integer)", true)]
    [InlineData("record(*)", "map(xs:string,item()*)", true)]
    [InlineData("record(*)", "map(xs:anyAtomicType,item()*)", true)]
    [InlineData("record(*)", "map(*)", true)]
    [InlineData("record(*)", "function(*)", true)]
    [InlineData("record(x)", "function(*)", true)]
    [InlineData("record(x)", "map(xs:string,xs:integer)", false)]
    [InlineData("record(*)", "map(xs:string,xs:integer*)", false)]
    [InlineData("record(x)", "map(xs:integer,item()*)", false)]
    [InlineData("record(x)", "map(xs:normalizedString,item()*)", false)]
    [InlineData("record(x)", "array(*)", false)]
    [InlineData("map(xs:string,xs:integer)", "record(x as xs:integer)", false)]
    [InlineData("map(*)", "record(*)", false)]
    [InlineData("record(x as xs:integer?)", "map(xs:string,xs:decimal?)", true)]
    [InlineData("record(x as xs:integer+)", "map(xs:string,xs:decimal)", false)]
    [InlineData("record(x as xs:integer+)", "map(xs:string,xs:decimal*)", true)]
    [InlineData("record(x as xs:integer?)", "map(xs:string,xs:decimal)", false)]
    [InlineData("record(x as xs:integer)", "map(xs:anyAtomicType,xs:decimal)", true)]
    [InlineData("record(x as xs:string)", "map(xs:string,xs:decimal)", false)]
    [InlineData("record(x)", "map(xs:QName,item()*)", false)]
    [InlineData("record(child as record(value as xs:integer))", "map(xs:string,record(value as xs:decimal))", true)]
    [InlineData("record(child as record(Name))", "map(xs:string,record(name))", false)]
    [InlineData("record(*)", "map(xs:string,item())", false)]
    [InlineData("function(*)", "record(*)", false)]
    [InlineData("record(Name as xs:integer,name as xs:integer)", "map(xs:string,xs:integer)", true)]
    [InlineData("record(x)?", "map(*)?", true)]
    [InlineData("record(*)?", "map(*)?", true)]
    [InlineData("record(x)*", "map(*)?", false)]
    [InlineData("record(x)+", "map(*)*", true)]
    [InlineData("record(x as empty-sequence())", "map(xs:string,xs:integer?)", true)]
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
