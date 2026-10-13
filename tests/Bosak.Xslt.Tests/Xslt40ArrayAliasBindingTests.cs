// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies atomic and array aliases at XSLT declaration boundaries.
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

/// <summary>Verifies atomic and array aliases at XSLT declaration boundaries.</summary>
public class Xslt40ArrayAliasBindingTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:f='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string body, string source = "<root/>") => new Api.XsltCompiler().Compile(Wrap(body))
        .TransformToString(new XDocumentNode(XDocument.Parse(source)));
    private const string Alias = "<xsl:item-type name='t:vector' as='array(t:number)'/><xsl:item-type name='t:number' as='xs:double'/>";

    /// <summary>Expands forward member aliases and promotes array members to the declared atomic type.</summary>
    [Fact]
    public void ArrayAlias_FunctionAndVariableConversion()
    {
        var output = Run(Alias + """
            <xsl:function name='f:vector' as='t:vector'><xsl:param name='v' as='t:vector'/><xsl:sequence select='$v'/></xsl:function>
            <xsl:template match='/'>
              <xsl:variable name='v' as='t:vector' select='f:vector([1,2])'/>
              <out><xsl:value-of select='($v(1) instance of xs:double,$v(2))' separator='|'/></out>
            </xsl:template>
            """);
        Assert.Equal("true|2", XElement.Parse(output).Value);
    }

    /// <summary>Uses declaration namespace bindings even when the consuming instruction rebinds that prefix.</summary>
    [Fact]
    public void ArrayAlias_DefinitionNamespaceAndNestedArrays()
    {
        var output = Run("""
            <xsl:item-type name='t:matrix' as='array(array(s:double?))' xmlns:s='http://www.w3.org/2001/XMLSchema'/>
            <xsl:template match='/' xmlns:s='urn:other' xmlns:v='urn:types'>
              <xsl:variable name='a' as='v:matrix' select='[[1,()]]'/>
              <out><xsl:value-of select='($a(1)(1) instance of xs:double,empty($a(1)(2)))' separator='|'/></out>
            </xsl:template>
            """);
        Assert.Equal("true|true", XElement.Parse(output).Value);
    }

    /// <summary>Preserves occurrence indicators and supports expanded alias names.</summary>
    [Fact]
    public void ArrayAlias_OptionalAndRepeatedBindings()
    {
        var output = Run(Alias + """
            <xsl:template match='/'>
              <xsl:variable name='absent' as='t:vector?' select='()'/>
              <xsl:variable name='values' as='Q{urn:types}vector+' select='([1],[2])'/>
              <out><xsl:value-of select='(empty($absent),count($values))' separator='|'/></out>
            </xsl:template>
            """);
        Assert.Equal("true|2", XElement.Parse(output).Value);
    }

    /// <summary>Applies expanded types to named function references and lookup calls.</summary>
    [Fact]
    public void ArrayAlias_DynamicFunctionConversion()
    {
        var output = Run(Alias + """
            <xsl:function name='f:vector' as='t:vector'><xsl:param name='v' as='t:vector'/><xsl:sequence select='$v'/></xsl:function>
            <xsl:template match='/'><xsl:variable name='f' select='f:vector#1'/>
              <xsl:variable name='g' select="function-lookup(QName('urn:functions','f:vector'),1)"/>
              <out><xsl:value-of select='($f([1])(1) instance of xs:double,$g([2])(1))' separator='|'/></out>
            </xsl:template>
            """);
        Assert.Equal("true|2", XElement.Parse(output).Value);
    }

    /// <summary>Reports variable conversion errors for invalid members and cardinalities.</summary>
    /// <param name="expression">The invalid value expression.</param>
    [Theory]
    [InlineData("['invalid']")]
    [InlineData("1")]
    [InlineData("()")]
    [InlineData("([1],[2])")]
    public void ArrayAlias_InvalidVariable(string expression)
    {
        var error = Assert.ThrowsAny<Exception>(() => Run(Alias + $"<xsl:template match='/'><xsl:variable name='v' as='t:vector' select=\"{expression}\"/><xsl:sequence select='$v'/></xsl:template>"));
        Assert.Contains("XTTE0570", error.ToString());
    }

    /// <summary>Rejects direct and indirect recursive definitions during compilation.</summary>
    /// <param name="declarations">The circular alias declarations.</param>
    [Theory]
    [InlineData("<xsl:item-type name='t:a' as='array(t:a)'/>")]
    [InlineData("<xsl:item-type name='t:a' as='t:b'/><xsl:item-type name='t:b' as='array(t:a)'/>")]
    public void ArrayAlias_CyclesAreStaticErrors(string declarations)
        => Assert.Contains("XTSE4035", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(Wrap(declarations))).ToString());

    /// <summary>Rejects occurrence indicators on an item-type declaration.</summary>
    [Fact]
    public void ArrayAlias_DefinitionMustBeAnItemType()
        => Assert.Contains("XTSE0020", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(Wrap("<xsl:item-type name='t:a' as='array(xs:double)?'/>"))).ToString());

    /// <summary>Converts atomic aliases independently and enumeration members inside an array.</summary>
    [Fact]
    public void Aliases_AtomicAndEnumerationMembers()
    {
        var output = Run("""
            <xsl:item-type name='t:number' as='xs:double'/>
            <xsl:item-type name='t:states' as='array(t:state)'/>
            <xsl:item-type name='t:state' as="enum('ready','queued')"/>
            <xsl:template match='/'>
              <xsl:variable name='n' as='t:number' select='1'/>
              <xsl:variable name='states' as='t:states' select="['ready','queued']"/>
              <out><xsl:value-of select='($n instance of xs:double,$states(2))' separator='|'/></out>
            </xsl:template>
            """);
        Assert.Equal("true|queued", XElement.Parse(output).Value);
    }

    /// <summary>Keeps member numeric promotion gated to the XPath 4.0 conversion profile.</summary>
    [Fact]
    public void ArrayCoercion_PreservesXPath31Behavior()
    {
        var array = Bosak.XPath.Core.Xdm.XdmValue.FromArray(new Bosak.XPath.Core.Xdm.XdmArray(
            new[] { Bosak.XPath.Core.Xdm.XdmValue.FromInteger(1) }));
        Assert.Throws<InvalidOperationException>(() => Bosak.XPath.Runtime.Vm.VmEngine.ApplyFunctionConversion(
            array, "array(xs:double)", new Bosak.XPath.Runtime.Vm.EvaluationContext()));
    }

    /// <summary>Converts XML coordinate attributes to a typed vector before calculating distance.</summary>
    [Fact]
    public void ArrayAlias_CoordinateReferenceExample()
    {
        var output = Run("""
            <xsl:item-type name='t:position' as='array(xs:double)'/>
            <xsl:template match='/' xmlns:math='http://www.w3.org/2005/xpath-functions/math'>
              <xsl:variable name='point' as='t:position' select='[root/@x,root/@y]'/>
              <distance><xsl:value-of select='math:sqrt($point(1)*$point(1)+$point(2)*$point(2))'/></distance>
            </xsl:template>
            """, "<root x='3' y='4'/>");
        Assert.Equal("5", XElement.Parse(output).Value);
    }
}
