// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies atomic choice casts and rejects non-atomic alternatives.
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
using Bosak.XPath.Api;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies atomic choice casts and static rejection of non-atomic alternatives.</summary>
[Collection("XSLT package registry")]
public class Xslt40ChoiceCastValidationTests
{
    private static string Run(string declarations, string expression) => new Api.XsltCompiler().Compile("""
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + declarations + $"<xsl:template match='/'><out><xsl:value-of select=\"{expression}\"/></out></xsl:template></xsl:stylesheet>")
        .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));

    /// <summary>Supports timestamp and sentinel choices, optional inputs and quoted separators.</summary>
    /// <param name="definition">The choice type definition.</param>
    /// <param name="expression">The alias cast expression.</param>
    /// <param name="expected">The expected serialized output.</param>
    [Theory]
    [InlineData("(xs:date | xs:dateTime)", "'2026-10-13' cast as t:when", "2026-10-13")]
    [InlineData("(xs:date | xs:dateTime)", "'2026-10-13T09:30:00' cast as t:when", "2026-10-13T09:30:00")]
    [InlineData("(xs:date | xs:dateTime)", "'invalid' castable as t:when", "false")]
    [InlineData("(xs:date | xs:dateTime)", "() castable as t:when?", "true")]
    [InlineData("(enum('unknown|pending') | xs:date)", "'unknown|pending' cast as t:when", "unknown|pending")]
    [InlineData("((enum('unknown') | xs:date) | xs:dateTime)", "'unknown' cast as t:when", "unknown")]
    [InlineData("(enum('a''|b') | xs:integer)", "\"a'|b\" cast as t:when", "a'|b")]
    public void Cast_AtomicChoice(string definition, string expression, string expected)
        => Assert.Equal(expected, XElement.Parse(Run($"<xsl:item-type name='t:when' as=\"{definition}\"/>", expression.Replace("\"", "&quot;"))).Value);

    /// <summary>Rejects choices containing container, node, function or abstract alternatives.</summary>
    /// <param name="definition">The invalid choice definition.</param>
    [Theory]
    [InlineData("(xs:integer | array(*))")]
    [InlineData("(map(*) | xs:string)")]
    [InlineData("(xs:integer | record(reading as xs:integer))")]
    [InlineData("(xs:integer | function(*))")]
    [InlineData("(xs:integer | node())")]
    [InlineData("((xs:integer | enum('unknown')) | array(*))")]
    [InlineData("(xs:integer | xs:anyAtomicType)")]
    public void Castable_NonAtomicAlias(string definition)
        => Assert.Contains("XPST0080", Assert.ThrowsAny<Exception>(() => Run(
            $"<xsl:item-type name='t:when' as=\"{definition}\"/>", "() castable as t:when?")).ToString());

    /// <summary>Checks direct targets without requiring host aliases or evaluating the operand.</summary>
    /// <param name="expression">The expression containing an invalid target.</param>
    [Theory]
    [InlineData("error() castable as (xs:integer | array(*))")]
    [InlineData("() cast as (map(*) | array(*))?")]
    [InlineData("() castable as ((xs:string | node()))")]
    [InlineData("() castable as function(*)")]
    public void Compile_DirectInvalidTarget(string expression)
        => Assert.Contains("XPST0080", Assert.ThrowsAny<Exception>(() => XPath31Expression.Compile(
            expression, new CompileOptions { Compatibility = XPathCompatibility.XPath40 })).ToString());

    /// <summary>Checks eligibility after expanding aliases nested within a choice.</summary>
    [Fact]
    public void Castable_NestedAliases()
        => Assert.Contains("XPST0080", Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:item-type name='t:number' as='xs:integer'/>
            <xsl:item-type name='t:container' as='array(t:number)'/>
            <xsl:item-type name='t:when' as='(t:number | t:container)'/>
            """, "() castable as t:when?")).ToString());

    /// <summary>Retains container-valued choices for ordinary type tests and standalone casts.</summary>
    [Fact]
    public void InstanceOf_ContainerChoiceStillAllowed()
        => Assert.Equal("true", XElement.Parse(Run("<xsl:item-type name='t:payload' as='(map(*) | array(*))'/>",
            "([1] cast as array(*)) instance of t:payload")).Value);

    /// <summary>Keeps the XPath 3.1 grammar boundary unchanged.</summary>
    [Fact]
    public void Compile_FrozenProfileRefusesChoiceGrammar()
        => Assert.ThrowsAny<Exception>(() => XPath31Expression.Compile("1 cast as (xs:integer | xs:string)",
            new CompileOptions { Compatibility = XPathCompatibility.XPath31 }));
}
