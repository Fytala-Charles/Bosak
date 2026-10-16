// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies array patterns, member sequences, predicate focus and priorities.
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
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies array patterns, member sequences, predicate focus and priorities.</summary>
public class Xslt40ArrayPatternTests
{
    private static string Run(string pattern, string value, string declarations = "", string version = "4.0")
    {
        XNamespace xsl = "http://www.w3.org/1999/XSL/Transform";
        var body = declarations + new XElement(xsl + "template", new XAttribute("match", "/"), new XAttribute("priority", "2"), new XElement("out",
            new XElement(xsl + "apply-templates", new XAttribute("select", value)))) +
            new XElement(xsl + "template", new XAttribute("match", pattern), "match") +
            "<xsl:template match='.' priority='-2'>fallback</xsl:template>";
        return XElement.Parse(new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version +
            "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:array='http://www.w3.org/2005/xpath-functions/array' xmlns:t='urn:types' exclude-result-prefixes='#all'>" + body + "</xsl:stylesheet>")
            .TransformToString(new XDocumentNode(XDocument.Parse("<root><child/></root>")))).Value;
    }

    /// <summary>Checks every member as a sequence, including empty arrays and empty members.</summary>
    /// <param name="pattern">The pattern.</param>
    /// <param name="value">The selected value.</param>
    /// <param name="expected">The selected rule.</param>
    [Theory]
    [InlineData("array(atomic(xs:integer))", "[1,2]", "match")]
    [InlineData("array(atomic(xs:integer))", "[]", "match")]
    [InlineData("array(atomic(xs:integer))", "[1,'2']", "fallback")]
    [InlineData("array(atomic(xs:integer))", "[(1,2)]", "fallback")]
    [InlineData("array(atomic(xs:integer))", "[()]", "fallback")]
    [InlineData("array(atomic(xs:integer)*)", "[(),(1,2)]", "match")]
    [InlineData("array(atomic(xs:integer)+)", "[(),(1,2)]", "fallback")]
    [InlineData("array(atomic(xs:integer)+)", "[(1,2),3]", "match")]
    [InlineData("array(atomic(xs:integer)?)", "[(),1]", "match")]
    [InlineData("array(atomic(xs:integer)?)", "[(1,2)]", "fallback")]
    [InlineData("array(empty-sequence())", "[(),()]", "match")]
    [InlineData("array(empty-sequence())", "[]", "match")]
    [InlineData("array(empty-sequence ( ))", "[1]", "fallback")]
    [InlineData("array(empty-sequence(:gap:)())", "[()]", "match")]
    [InlineData("array(.)", "[(),1]", "fallback")]
    [InlineData("array(.)", "[1,map{},[]]", "match")]
    [InlineData("array(.)", "map{}", "fallback")]
    [InlineData("array(.)", "1", "fallback")]
    [InlineData("array(.)", "/*", "fallback")]
    [InlineData("array(*)", "[/*,/*/child]", "match")]
    [InlineData("array(*)", "[1]", "fallback")]
    [InlineData("array(root/*+)", "[/*/child]", "match")]
    [InlineData("array(**)", "[(),/*]", "match")]
    [InlineData("array(array(atomic(xs:integer)))", "[[1,2],[]]", "match")]
    [InlineData("array(array(atomic(xs:integer)))", "[[1,'2']]", "fallback")]
    [InlineData("array({'x':atomic(xs:integer)})", "[map{'x':1}]", "match")]
    [InlineData("array({'x':atomic(xs:integer)})", "[map{'x':'1'}]", "fallback")]
    [InlineData("{'v':array(atomic(xs:integer))+}", "map{'v':([1],[2])}", "match")]
    [InlineData("array(.[position()=1 and last()=1 and current()=.]+)", "[(1,2),3]", "match")]
    [InlineData("array(.)[position()=1 and last()=1 and array:size(current())=2]", "[1,2]", "match")]
    [InlineData("array(.)[2]", "[1]", "fallback")]
    [InlineData("array(.)[error()]", "[1]", "fallback")]
    [InlineData("array(.[error()])", "[]", "match")]
    [InlineData("array(.[error()])", "[1]", "fallback")]
    [InlineData("(array(.))", "[1]", "fallback")]
    public void Matching(string pattern, string value, string expected) => Assert.Equal(expected, Run(pattern, value));

    /// <summary>Uses recursive member priorities and the array predicate override.</summary>
    /// <param name="pattern">The array pattern.</param>
    /// <param name="value">A matching array.</param>
    /// <param name="below">A priority below its default.</param>
    /// <param name="above">A priority above its default.</param>
    [Theory]
    [InlineData("array(.)", "[1]", "-1.01", "-0.99")]
    [InlineData("array(atomic(xs:integer)+)", "[(1,2)]", "0.24", "0.26")]
    [InlineData("array(array(atomic(xs:integer)))", "[[1]]", "0.24", "0.26")]
    [InlineData("array(atomic(xs:integer | xs:string))", "[1]", "-0.01", "0.01")]
    [InlineData("array(empty-sequence())", "[()]", "0.49", "0.51")]
    [InlineData("array(.[true()])", "[1]", "0.49", "0.51")]
    [InlineData("array({'x':.[true()]})", "[map{'x':1}]", "0.49", "0.51")]
    [InlineData("array(array(.)[true()])", "[[1]]", "0.49", "0.51")]
    [InlineData("array({'[key]'})", "[map{'[key]':1}]", "-0.01", "0.01")]
    [InlineData("array(.)[true()]", "[1]", "0.49", "0.51")]
    public void Priorities(string pattern, string value, string below, string above)
    {
        string Other(string priority) => "<xsl:template match='.[. instance of array(*)]' priority='" + priority + "'>other</xsl:template>";
        Assert.Equal("match", Run(pattern, value, Other(below)));
        Assert.Equal("other", Run(pattern, value, Other(above)));
    }

    /// <summary>Composes record aliases and named type bindings in member patterns.</summary>
    [Fact]
    public void NamedRecords() => Assert.Equal("match", Run("array(record(t:point))", "[t:point(1),t:point(2)]", """
        <xsl:record-type name='t:point' constructor='yes'><xsl:field name='x' as='xs:integer'/></xsl:record-type>
        """));

    /// <summary>Does not interpret brackets inside a type EQName URI as predicates.</summary>
    [Fact]
    public void EqNamePriority()
    {
        const string types = """
            <xsl:record-type name='t:point' constructor='yes'><xsl:field name='x' as='xs:integer'/></xsl:record-type>
            <xsl:item-type name='Q{urn:a[b],)}shape' as='record(x as xs:integer)'/>
            """;
        const string pattern = "array(record(Q{urn:a[b],)}shape))";
        Assert.Equal("match", Run(pattern, "[t:point(1)]", types +
            "<xsl:template match='.[. instance of array(*)]' priority='-0.01'>other</xsl:template>"));
        Assert.Equal("other", Run(pattern, "[t:point(1)]", types +
            "<xsl:template match='.[. instance of array(*)]' priority='0.01'>other</xsl:template>"));
    }

    /// <summary>Rejects malformed patterns before matching even an empty array.</summary>
    /// <param name="pattern">The invalid pattern.</param>
    [Theory]
    [InlineData("array()")]
    [InlineData("array(atomic(xs:unknown))")]
    [InlineData("array(empty-sequence()?)")]
    [InlineData("array(atomic(xs:integer)??)")]
    [InlineData("array(atomic(xs:integer),.)")]
    [InlineData("array(.??)")]
    [InlineData("array(***)")]
    [InlineData("array(.)?")]
    [InlineData("array(empty-sequence()*)")]
    [InlineData("array(.)/x")]
    [InlineData("array(.)[true()")]
    [InlineData("array(.[(true()])")]
    [InlineData("array(.) | x")]
    [InlineData("x intersect array(.)")]
    public void Invalid(string pattern) => Assert.ThrowsAny<Exception>(() => Run(pattern, "[]"));

    /// <summary>Requires the 4.0 profile.</summary>
    [Fact]
    public void Profile() => Assert.ThrowsAny<Exception>(() => Run("array(.)", "()", version: "3.0"));
}
