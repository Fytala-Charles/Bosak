// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies named type contracts on inline XPath functions in XSLT.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 13-10-2026     | Creation                                                                                 |
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies named type contracts on inline XPath functions in XSLT.</summary>
public class Xslt40InlineAliasContractTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:map='http://www.w3.org/2005/xpath-functions/map' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string declarations, string expression, string attributes = "") => XElement.Parse(new Api.XsltCompiler().Compile(Wrap(
        declarations + "<xsl:template match='/' " + attributes + "><out><xsl:value-of separator='|' select=\"" + expression + "\"/></out></xsl:template>"))
        .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")))).Value;

    /// <summary>Expands structural record contracts on parameters and results.</summary>
    /// <param name="keyword">The inline function keyword.</param>
    [Theory]
    [InlineData("fn")]
    [InlineData("function")]
    public void StructuralRecordContract(string keyword)
    {
        Assert.Equal("1885-12-15|Johann", Run(
            "<xsl:item-type name='t:person' as=\"record(name as xs:string, 'date of birth' as xs:date)\"/>",
            "let $f := " + keyword + "($p as t:person) as t:person {map:put($p,'date of birth',xs:date('1885-12-15'))}, $r := $f(map{'name':'Johann','date of birth':xs:date('1951-10-11')}) return ($r?'date of birth',$r?name)"));
    }

    /// <summary>Checks alias occurrence indicators and argument conversion.</summary>
    /// <param name="occurrence">The parameter occurrence indicator.</param>
    /// <param name="argument">The supplied argument expression.</param>
    /// <param name="expected">The expected item count.</param>
    [Theory]
    [InlineData("?", "()", "0")]
    [InlineData("*", "(1,2)", "2")]
    [InlineData("+", "(1,2)", "2")]
    public void ParameterCardinality(string occurrence, string argument, string expected)
    {
        Assert.Equal(expected, Run("<xsl:item-type name='t:number' as='xs:double'/>",
            "(fn($v as t:number" + occurrence + ") as xs:integer {count($v)})(" + argument + ")"));
    }

    /// <summary>Applies expanded return contracts independently of parameter declarations.</summary>
    [Fact]
    public void ReturnOnlyContract()
    {
        Assert.Equal("true", Run("<xsl:item-type name='t:vector' as='array(xs:double)'/>",
            "(fn() as t:vector {[1,2]})() instance of array(xs:double)"));
    }

    /// <summary>Resolves nested member aliases in inline signatures.</summary>
    [Fact]
    public void NestedArrayContract()
    {
        Assert.Equal("true", Run("<xsl:item-type name='t:number' as='xs:double'/>",
            "(fn($v as array(t:number)) as array(t:number) {$v})([1]) instance of array(xs:double)"));
    }

    /// <summary>Resolves signature prefixes and defaults in their expression static context.</summary>
    /// <param name="name">The lexical or expanded type name.</param>
    /// <param name="attributes">The namespace and default namespace attributes.</param>
    [Theory]
    [InlineData("v:number", "xmlns:v='urn:types' xmlns:t='urn:other'")]
    [InlineData("number", "xpath-default-namespace='urn:types'")]
    [InlineData("Q{urn:types}number", "xmlns:t='urn:other'")]
    public void NamespaceForms(string name, string attributes)
    {
        Assert.Equal("true", Run("<xsl:item-type name='t:number' as='xs:double'/>",
            "(fn($v as " + name + ") as " + name + " {$v})(1) instance of xs:double", attributes));
    }

    /// <summary>Rejects invalid argument and result values without dropping declared contracts.</summary>
    /// <param name="expression">The function invocation to reject.</param>
    [Theory]
    [InlineData("(fn($p as t:person) {$p})(map{'name':1})")]
    [InlineData("(fn() as t:person {map{'name':1}})()")]
    [InlineData("(fn($n as t:number+) {$n})(())")]
    [InlineData("(fn() as t:number+ {()})()")]
    public void InvalidValues(string expression)
    {
        Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Run(
            "<xsl:item-type name='t:person' as='record(name as xs:string)'/><xsl:item-type name='t:number' as='xs:double'/>", expression)).ToString());
    }

    /// <summary>Rejects unknown signature names when compiling an unused function.</summary>
    /// <param name="expression">The inline function expression.</param>
    /// <param name="error">The expected static error code.</param>
    [Theory]
    [InlineData("fn($v as t:missing) {$v}", "XPST0051")]
    [InlineData("fn() as t:missing {()}", "XPST0051")]
    [InlineData("fn($v as missing:value) {$v}", "XPST0081")]
    public void StaticSignatureErrors(string expression, string error)
    {
        Assert.Contains(error, Assert.ThrowsAny<Exception>(() => Run("", "let $f := " + expression + " return 'unused'")).ToString());
    }

    /// <summary>Retains nominative record identity in inline signatures.</summary>
    [Fact]
    public void NamedRecordContract()
    {
        Assert.Equal("true|7", Run("<xsl:record-type name='t:point' constructor='yes'><xsl:field name='x' as='xs:integer'/></xsl:record-type>",
            "let $r := (fn($p as t:point) as t:point {$p})(map{'x':7}) return ($r instance of t:point,$r?x)"));
    }

    /// <summary>Retains aliases when an inline namespace prolog creates a merged static context.</summary>
    [Fact]
    public void InlineNamespaceProlog()
    {
        Assert.Equal("true", Run("<xsl:item-type name='t:number' as='xs:double'/>",
            "declare namespace v='urn:types'; (fn($v as v:number) as v:number {$v})(1) instance of xs:double"));
    }

    /// <summary>Keeps omitted annotations at their existing item-star defaults.</summary>
    [Fact]
    public void OmittedAnnotations()
        => Assert.Equal("ok", Run("", "(fn($v) {$v})('ok')"));

    /// <summary>Does not expose 4.0 aliases inside a 3.1 instruction.</summary>
    [Fact]
    public void MixedVersionRefusal()
    {
        Assert.Contains("XPTY0004", Assert.ThrowsAny<Exception>(() => Run("<xsl:item-type name='t:number' as='xs:double'/>",
            "(function($v as t:number) {$v})(1)", "version='3.0'")).ToString());
    }

    /// <summary>Expands public enumeration aliases in standalone inline function signatures.</summary>
    [Fact]
    public void PublicEnumerationAlias()
    {
        var expression = Bosak.XPath.Api.XPath31Expression.Compile("(fn($v as Q{urn:types}state) as Q{urn:types}state {$v})('ready')",
            new Bosak.XPath.Api.CompileOptions
            {
                Compatibility = Bosak.XPath.Api.XPathCompatibility.XPath40,
                EnumerationItemTypes = new Dictionary<string, string> { ["Q{urn:types}state"] = "enum('ready','done')" }
            });
        Assert.Equal("ready", expression.Evaluate(new XDocumentNode(XDocument.Parse("<root/>"))).StringValue);
    }

    /// <summary>Preserves the enumeration-only public registration boundary.</summary>
    /// <param name="definition">The invalid alias definition.</param>
    [Theory]
    [InlineData("array(xs:integer)")]
    [InlineData("enum('ready')?")]
    public void PublicEnumerationRejectsInvalidDefinitions(string definition)
    {
        Assert.Throws<ArgumentException>(() => Bosak.XPath.Api.XPath31Expression.Compile("fn($v as Q{urn:types}state) {$v}",
            new Bosak.XPath.Api.CompileOptions
            {
                Compatibility = Bosak.XPath.Api.XPathCompatibility.XPath40,
                EnumerationItemTypes = new Dictionary<string, string> { ["Q{urn:types}state"] = definition }
            }));
    }

    /// <summary>Supports aliases in inline functions inside attribute value templates.</summary>
    [Fact]
    public void AttributeValueTemplate()
    {
        var output = new Api.XsltCompiler().Compile(Wrap("<xsl:item-type name='t:number' as='xs:double'/><xsl:template match='/'><out valid='{(fn($v as t:number) {$v})(1) instance of xs:double}'/></xsl:template>"))
            .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));
        Assert.Equal("true", XElement.Parse(output).Attribute("valid")!.Value);
    }
}
