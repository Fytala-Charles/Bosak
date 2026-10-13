// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies function and choice aliases across XSLT and XPath boundaries.
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

/// <summary>Verifies function and choice aliases across XSLT and XPath boundaries.</summary>
public class Xslt40FunctionChoiceAliasTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:f='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string body, string source = "<root/>") => new Api.XsltCompiler().Compile(Wrap(body))
        .TransformToString(new XDocumentNode(XDocument.Parse(source)));
    private const string Function = "<xsl:item-type name='t:callback' as='function(xs:integer) as t:number'/><xsl:item-type name='t:number' as='xs:double'/>";
    private const string Choice = "<xsl:item-type name='t:value' as='(t:number|xs:string)'/><xsl:item-type name='t:number' as='xs:double'/>";

    /// <summary>Coerces a callback's integer return value to the alias's double return type.</summary>
    [Fact]
    public void FunctionAlias_CallbackConversion()
        => Assert.Equal("true|3", XElement.Parse(Run(Function + """
            <xsl:template match='/'>
              <xsl:variable name='f' as='t:callback' select='fn($x as xs:integer) as xs:integer {$x+1}'/>
              <out><xsl:value-of separator='|' select='($f(2) instance of t:number,$f(2))'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Passes callback aliases through direct and named-reference function calls.</summary>
    [Fact]
    public void FunctionAlias_FunctionBoundaries()
        => Assert.Equal("true|true", XElement.Parse(Run(Function + """
            <xsl:function name='f:callback' as='t:callback'><xsl:param name='f' as='t:callback'/><xsl:sequence select='$f'/></xsl:function>
            <xsl:template match='/'>
              <xsl:variable name='f' select='fn($x as xs:integer) as xs:integer {$x}'/>
              <xsl:variable name='call' select='f:callback#1'/>
              <out><xsl:value-of separator='|' select='(f:callback($f)(2) instance of t:number,$call($f)(3) instance of t:number)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Checks typed function aliases in instance-of and treat operands.</summary>
    [Fact]
    public void FunctionAlias_TypeExpressions()
        => Assert.Equal("true|false|2", XElement.Parse(Run(Function + """
            <xsl:template match='/'>
              <xsl:variable name='f' select='fn($x as xs:integer) as xs:double {xs:double($x)}'/>
              <out><xsl:value-of separator='|' select='($f instance of t:callback,
                1 instance of t:callback,($f treat as t:callback)(2))'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Keeps a function's optional return type distinct from an optional function item.</summary>
    [Fact]
    public void FunctionAlias_ReturnAndOuterOccurrences()
        => Assert.Equal("true|true", XElement.Parse(Run("""
            <xsl:item-type name='t:callback' as='function() as xs:double?'/>
            <xsl:template match='/'>
              <xsl:variable name='f' as='t:callback' select='fn() as xs:double? {()}'/>
              <xsl:variable name='missing' as='t:callback?' select='()'/>
              <out><xsl:value-of separator='|' select='(empty($f()),$missing instance of t:callback?)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Preserves optional parameters and repeated function return sequences.</summary>
    [Fact]
    public void FunctionAlias_ParameterAndResultSequences()
        => Assert.Equal("2|true", XElement.Parse(Run("""
            <xsl:item-type name='t:callback' as='function(xs:integer?) as xs:double*'/>
            <xsl:template match='/'>
              <xsl:variable name='f' as='t:callback' select='fn($x as xs:integer?) as xs:integer* {($x,$x)}'/>
              <out><xsl:value-of separator='|' select='(count($f(1)),$f(1) instance of xs:double*)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Expands nested function aliases in array members and inline function type operands.</summary>
    [Fact]
    public void FunctionAlias_NestedReferences()
        => Assert.Equal("true|true", XElement.Parse(Run(Function + """
            <xsl:template match='/'>
              <xsl:variable name='functions' as='array(t:callback)' select='[fn($x as xs:integer) as xs:double {xs:double($x)}]'/>
              <out><xsl:value-of separator='|' select='($functions instance of array(t:callback),
                $functions(1) instance of function(xs:integer) as t:number)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Reports incompatible callback arity and non-function binding values.</summary>
    /// <param name="expression">The invalid callback expression.</param>
    [Theory]
    [InlineData("1")]
    [InlineData("fn() as xs:integer {1}")]
    public void FunctionAlias_InvalidBinding(string expression)
        => Assert.Contains("XTTE0570", Assert.ThrowsAny<Exception>(() => Run(Function + $"<xsl:template match='/'><xsl:variable name='f' as='t:callback' select=\"{expression}\"/><xsl:sequence select='$f'/></xsl:template>")).ToString());

    /// <summary>Expands ordered alternatives at declaration and XPath type-test boundaries.</summary>
    [Fact]
    public void ChoiceAlias_BindingAndTypeExpressions()
        => Assert.Equal("true|true|false", XElement.Parse(Run(Choice + """
            <xsl:template match='/'>
              <xsl:variable name='value' as='t:value' select='1'/>
              <out><xsl:value-of separator='|' select="($value instance of t:value,
                'ready' instance of t:value,true() instance of t:value)"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Preserves the declared order when an untyped value can convert to either alternative.</summary>
    [Fact]
    public void ChoiceAlias_AlternativeOrder()
        => Assert.Equal("true|true", XElement.Parse(Run("""
            <xsl:item-type name='t:numeric' as='(xs:double|xs:string)'/>
            <xsl:item-type name='t:textual' as='(xs:string|xs:double)'/>
            <xsl:template match='/'>
              <xsl:variable name='n' as='t:numeric' select='root/@value'/>
              <xsl:variable name='s' as='t:textual' select='root/@value'/>
              <out><xsl:value-of separator='|' select='($n instance of xs:double,$s instance of xs:string)'/></out>
            </xsl:template>
            """, "<root value='12'/>")).Value);

    /// <summary>Resolves choice aliases inside arrays and parenthesized type operands.</summary>
    [Fact]
    public void ChoiceAlias_NestedReferencesAndOccurrences()
        => Assert.Equal("true|true|true", XElement.Parse(Run(Choice + """
            <xsl:template match='/'>
              <xsl:variable name='values' as='array(t:value)' select="[1,'ready']"/>
              <out><xsl:value-of separator='|' select="($values instance of array(t:value),
                'ready' instance of (t:number|xs:string),() instance of t:value?)"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Reports treat errors without performing choice conversion.</summary>
    [Fact]
    public void ChoiceAlias_InvalidTreat()
        => Assert.Contains("XPDY0050", Assert.ThrowsAny<Exception>(() => Run(Choice + """
            <xsl:template match='/'><xsl:sequence select='true() treat as t:value'/></xsl:template>
            """)).ToString());

    /// <summary>Rejects alias cycles through function signatures or choice alternatives.</summary>
    /// <param name="definition">The recursive item type.</param>
    [Theory]
    [InlineData("function(t:self) as xs:string")]
    [InlineData("(xs:string|t:self)")]
    public void Alias_RecursiveDefinitions(string definition)
        => Assert.Contains("XTSE4035", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(Wrap($"<xsl:item-type name='t:self' as='{definition}'/>"))).ToString());

    /// <summary>Rejects outer occurrences on item-type declarations while accepting return cardinalities.</summary>
    [Fact]
    public void FunctionAlias_InvalidOuterOccurrence()
        => Assert.Contains("XTSE0020", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(
            Wrap("<xsl:item-type name='t:f' as='(function() as xs:double?)?'/>"))).ToString());

    /// <summary>Preserves delimiters inside enumeration literals when splitting choice alternatives.</summary>
    [Fact]
    public void ChoiceAlias_LiteralDelimitersAndComments()
        => Assert.Equal("true", XElement.Parse(Run("""
            <xsl:item-type name='t:value' as="(enum('left|right','can''t') | (: note :) xs:double)"/>
            <xsl:template match='/'><out valid="{'left|right' instance of t:value}"/></xsl:template>
            """)).Attribute("valid")!.Value);

    /// <summary>Uses a reusable callback type to calibrate an XML sensor reading.</summary>
    [Fact]
    public void FunctionAlias_CalibrationReferenceExample()
        => Assert.Equal("23.1", XElement.Parse(Run("""
            <xsl:item-type name='t:calibration' as='function(t:value) as t:value'/>
            <xsl:item-type name='t:value' as='xs:double'/>
            <xsl:template match='/'>
              <xsl:variable name='calibrate' as='t:calibration' select='fn($v as xs:double) as xs:double {$v*1.1}'/>
              <reading><xsl:value-of select='$calibrate(xs:double(root/@value))'/></reading>
            </xsl:template>
            """, "<root value='21'/>")).Value);

    /// <summary>Applies outer cardinalities to sequences of function items.</summary>
    [Fact]
    public void FunctionAlias_ItemSequenceOccurrences()
        => Assert.Equal("true|false|true", XElement.Parse(Run(Function + """
            <xsl:template match='/'>
              <xsl:variable name='f' select='fn($x as xs:integer) as xs:double {xs:double($x)}'/>
              <out><xsl:value-of separator='|' select='(($f,$f) instance of t:callback+,
                ($f,$f) instance of t:callback,$f instance of t:callback?)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Preserves unknown alias refusal within a mixed XPath 3.1 instruction.</summary>
    [Fact]
    public void ChoiceAlias_MixedVersionBoundary()
        => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => Run(Choice + """
            <xsl:template match='/' version='3.0'><xsl:sequence select='1 instance of t:value'/></xsl:template>
            """)).ToString());
}
