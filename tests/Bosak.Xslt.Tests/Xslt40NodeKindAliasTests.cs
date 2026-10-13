// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies node-kind aliases and nested static type diagnostics.
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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies node-kind aliases and nested static type diagnostics.</summary>
public class Xslt40NodeKindAliasTests
{
    private static Api.XsltExecutable Compile(string body) => new Api.XsltCompiler().Compile("""
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>");

    private static string Run(string body) => XElement.Parse(Compile(body).TransformToString(
        new XDocumentNode(XDocument.Parse("<root a='value'>text<!--comment--><?target value?></root>")))).Value;

    /// <summary>Matches node-kind aliases and preserves nodes at binding boundaries.</summary>
    /// <param name="type">The node-kind type definition.</param>
    /// <param name="select">An expression selecting a matching node.</param>
    [Theory]
    [InlineData("element()", "/*")]
    [InlineData("attribute()", "/*/@a")]
    [InlineData("text()", "/*/text()")]
    [InlineData("comment()", "/*/comment()")]
    [InlineData("processing-instruction()", "/*/processing-instruction()")]
    [InlineData("document-node()", ".")]
    [InlineData("namespace-node()", "/*/namespace::xml")]
    [InlineData(" element (:outer (:inner:) :) ( ) ", "/*")]
    public void Alias_NodeKind(string type, string select)
        => Assert.Equal("true|true|true|true|false", Run(
            $"<xsl:item-type name='t:n' as='{type}'/><xsl:template match='/'><xsl:variable name='n' as='t:n' select='{select}'/>" +
            "<out><xsl:value-of separator='|' select='($n instance of t:n,($n treat as t:n) is $n,() instance of t:n?,($n,$n) instance of t:n+,42 instance of t:n)'/></out></xsl:template>"));

    /// <summary>Traverses containers and function signatures containing node kinds.</summary>
    [Fact]
    public void Alias_StructuralTypes()
        => Assert.Equal("true|true|true|true", Run("""
            <xsl:item-type name='t:nodes' as='array(element())'/>
            <xsl:item-type name='t:lookup' as='map(xs:string,comment()?)'/>
            <xsl:item-type name='t:payload' as='record(body as text())'/>
            <xsl:item-type name='t:callback' as='function(element()) as text()*'/>
            <xsl:template match='/'><out><xsl:value-of separator='|' select="(
              [/*] instance of t:nodes, map{'x':/*/comment()} instance of t:lookup,
              (map{'body':/*/text()} cast as t:payload) instance of t:payload,
              function($e as element()) as text()* {$e/text()} instance of t:callback)"/></out></xsl:template>
            """));

    /// <summary>Does not let an earlier node kind hide an unknown later type name.</summary>
    /// <param name="type">A composite type containing an unknown name.</param>
    [Theory]
    [InlineData("(element() | t:missing)")]
    [InlineData("map(element(),t:missing)")]
    [InlineData("record(body as text(),missing as t:missing)")]
    [InlineData("function(comment()) as t:missing")]
    public void Declaration_UnknownComposite(string type)
        => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => Compile(
            $"<xsl:variable name='unused' as='{type}?' select='()'/>")).ToString());

    /// <summary>Rejects non-atomic choices when a node-kind alias is used as a cast target.</summary>
    /// <param name="operation">The casting operation.</param>
    [Theory]
    [InlineData("cast")]
    [InlineData("castable")]
    public void Alias_ChoiceCastRejected(string operation)
        => Assert.Contains("XPST0080", Assert.ThrowsAny<Exception>(() => Run(
            "<xsl:item-type name='t:payload' as='(element() | xs:string)'/>" +
            $"<xsl:template match='/'><out><xsl:value-of select='/* {operation} as t:payload'/></out></xsl:template>")).ToString());

    /// <summary>Refuses atomic values at a node-kind alias binding boundary.</summary>
    [Fact]
    public void Alias_BindingRejectsAtomicValue()
        => Assert.Contains("XTTE0570", Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:item-type name='t:n' as='element()'/>
            <xsl:template match='/'><xsl:variable name='n' as='t:n' select='42'/><out><xsl:sequence select='$n'/></out></xsl:template>
            """)).ToString());
}
