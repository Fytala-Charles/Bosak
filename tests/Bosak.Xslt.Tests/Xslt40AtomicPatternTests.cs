// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies atomic patterns, alternatives, type resolution and priorities.
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

/// <summary>Verifies atomic patterns, alternatives, type resolution and priorities.</summary>
public class Xslt40AtomicPatternTests
{
    private static string Run(string pattern, string value, string declarations = "", string extra = "", string version = "4.0")
    {
        XNamespace xsl = "http://www.w3.org/1999/XSL/Transform";
        var body = declarations + new XElement(xsl + "template", new XAttribute("match", "/"), new XElement("out",
            new XElement(xsl + "apply-templates", new XAttribute("select", value)))) +
            new XElement(xsl + "template", new XAttribute("match", pattern), "match") +
            "<xsl:template match='.'>fallback</xsl:template>";
        return XElement.Parse(new Api.XsltCompiler().Compile("<xsl:stylesheet version='" + version +
            "' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:t='urn:types' exclude-result-prefixes='#all' " + extra + ">" + body + "</xsl:stylesheet>")
            .TransformToString(new XDocumentNode(XDocument.Parse("<root>7</root>")))).Value;
    }

    /// <summary>Matches atomic instances without atomization or coercion.</summary>
    /// <param name="pattern">The pattern.</param>
    /// <param name="value">The selected value.</param>
    /// <param name="expected">The selected rule.</param>
    [Theory]
    [InlineData("atomic(xs:integer)", "7", "match")]
    [InlineData("atomic(xs:integer)", "xs:int(7)", "match")]
    [InlineData("atomic(xs:integer)", "7.5", "fallback")]
    [InlineData("atomic(xs:integer)", "'7'", "fallback")]
    [InlineData("atomic(xs:anyAtomicType)", "/*", "fallback")]
    [InlineData("atomic(xs:anyAtomicType)", "map{}", "fallback")]
    [InlineData("atomic(xs:anyAtomicType)", "[7]", "fallback")]
    [InlineData("atomic(xs:anyAtomicType)", "true()", "match")]
    [InlineData("atomic(xs:untypedAtomic)", "xs:untypedAtomic('7')", "match")]
    [InlineData("atomic(xs:numeric)", "xs:double(7)", "match")]
    [InlineData("atomic(xs:numeric)", "'7'", "fallback")]
    [InlineData("atomic(xs:string | xs:integer)", "7", "match")]
    [InlineData("atomic(xs:string | xs:integer)", "true()", "fallback")]
    [InlineData("atomic (:gap:) (xs:integer)[position()=1 and last()=1 and current()=7]", "7", "match")]
    [InlineData("atomic(xs:integer)[2]", "7", "fallback")]
    [InlineData("atomic(xs:integer)[error()]", "7", "fallback")]
    [InlineData("{'v':atomic(xs:integer)+}", "map{'v':(1,2)}", "match")]
    [InlineData("{'v':atomic(xs:integer)+}", "map{'v':(1,'2')}", "fallback")]
    [InlineData("{'v':atomic(xs:integer)+}", "map{'v':()}", "fallback")]
    [InlineData("{'v':atomic(xs:integer)?}", "map{'v':()}", "match")]
    [InlineData("(atomic(xs:integer))", "7", "fallback")]
    public void Matching(string pattern, string value, string expected) => Assert.Equal(expected, Run(pattern, value));

    /// <summary>Resolves aliases and element/type default namespaces.</summary>
    [Fact]
    public void NamesAndAliases()
    {
        Assert.Equal("match", Run("atomic(t:number)", "7", "<xsl:item-type name='t:number' as='(xs:integer | xs:string)'/>"));
        Assert.Equal("match", Run("atomic(integer)", "7", extra: "xpath-default-namespace='http://www.w3.org/2001/XMLSchema'"));
        Assert.Equal("match", Run("atomic(Q{http://www.w3.org/2001/XMLSchema}integer)", "7"));
    }

    /// <summary>Uses derivation depth and the minimum alternative priority.</summary>
    /// <param name="pattern">The pattern with inferred priority.</param>
    /// <param name="competitor">A priority just below the expected priority.</param>
    [Theory]
    [InlineData("atomic(xs:anyAtomicType)", "-0.51")]
    [InlineData("atomic(xs:decimal)", "-0.01")]
    [InlineData("atomic(xs:integer)", "0.24")]
    [InlineData("atomic(xs:long)", "0.37")]
    [InlineData("atomic(xs:int)", "0.43")]
    [InlineData("atomic(xs:integer | xs:string)", "-0.01")]
    [InlineData("atomic(xs:numeric)", "-0.01")]
    [InlineData("atomic(xs:integer)[true()]", "0.49")]
    public void DefaultPriority(string pattern, string competitor)
    {
        var declaration = "<xsl:template match='.[. instance of xs:anyAtomicType]' priority='" + competitor + "'>lower</xsl:template>";
        Assert.Equal("match", Run(pattern, "xs:int(7)", declaration));
        // A rule just above the expected value must win too (especially for alternatives).
        var above = (double.Parse(competitor, System.Globalization.CultureInfo.InvariantCulture) + 0.02).ToString(System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal("lower", Run(pattern, "xs:int(7)", declaration.Replace(competitor, above)));
    }

    /// <summary>Rejects malformed or non-atomic types even when no candidate is selected.</summary>
    /// <param name="pattern">The invalid pattern.</param>
    [Theory]
    [InlineData("atomic()")]
    [InlineData("atomic(xs:integer |)")]
    [InlineData("atomic(| xs:integer)")]
    [InlineData("atomic(xs:integer | | xs:string)")]
    [InlineData("atomic(xs:integer?)")]
    [InlineData("atomic(item())")]
    [InlineData("atomic(xs:anySimpleType)")]
    [InlineData("atomic(xs:IDREFS)")]
    [InlineData("atomic(xs:unknown)")]
    [InlineData("atomic(no:integer)")]
    [InlineData("atomic(xs:integer)/x")]
    [InlineData("atomic(xs:integer) | x")]
    [InlineData("x union atomic(xs:integer)")]
    [InlineData("atomic(xs:integer)[true()")]
    public void InvalidPatterns(string pattern) => Assert.ThrowsAny<Exception>(() => Run(pattern, "()"));


    /// <summary>Preserves alias namespace punctuation and validates atomic context declarations.</summary>
    [Fact]
    public void AliasNamespaceAndContext()
    {
        Assert.Equal("match", Run("atomic(Q{urn:a||b[|]}number)", "7",
            "<xsl:item-type name='Q{urn:a||b[|]}number' as='xs:integer'/>"));
        Assert.Equal("match", Run("atomic(t:number)", "7",
            "<xsl:item-type name='t:number' as='xs:integer'/>",
            "default-mode='m'"));
        var stylesheet = """
            <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
              xmlns:xs='http://www.w3.org/2001/XMLSchema'>
              <xsl:template match='/'><out><xsl:apply-templates select='7'/></out></xsl:template>
              <xsl:template match='atomic(xs:integer)'><xsl:context-item as='xs:integer'/>match</xsl:template>
            </xsl:stylesheet>
            """;
        Assert.Equal("match", XElement.Parse(new Api.XsltCompiler().Compile(stylesheet).TransformToString(
            new XDocumentNode(XDocument.Parse("<root/>")))).Value);
    }

    /// <summary>Expands pure-union aliases before computing their minimum priority.</summary>
    [Fact]
    public void AliasPriority()
    {
        var declarations = "<xsl:item-type name='t:number' as='(xs:integer | xs:string)'/>" +
            "<xsl:template match='.[. instance of xs:integer]' priority='0.1'>higher</xsl:template>";
        Assert.Equal("higher", Run("atomic(t:number)", "7", declarations));
    }

    /// <summary>Requires the 4.0 profile and refuses structural aliases.</summary>
    [Fact]
    public void ProfileAndAliasErrors()
    {
        Assert.ThrowsAny<Exception>(() => Run("atomic(xs:integer)", "()", version: "3.0"));
        Assert.ThrowsAny<Exception>(() => Run("atomic(t:bad)", "()", "<xsl:item-type name='t:bad' as='map(*)'/>"));
    }
}
