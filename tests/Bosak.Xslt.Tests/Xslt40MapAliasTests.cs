// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 13 October 2026
// PURPOSE              : Verifies map aliases, nested references and XPath 4.0 map coercion.
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

/// <summary>Verifies map aliases, nested references and XPath 4.0 map coercion.</summary>
public class Xslt40MapAliasTests
{
    private static string Wrap(string body) => """
        <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
          xmlns:t='urn:types' xmlns:f='urn:functions' xmlns:xs='http://www.w3.org/2001/XMLSchema'
          xmlns:map='http://www.w3.org/2005/xpath-functions/map' exclude-result-prefixes='#all'>
        """ + body + "</xsl:stylesheet>";
    private static string Run(string body, string source = "<root/>") => new Api.XsltCompiler().Compile(Wrap(body))
        .TransformToString(new XDocumentNode(XDocument.Parse(source)));
    private const string Aliases = "<xsl:item-type name='t:measurements' as='map(t:key,t:number)'/><xsl:item-type name='t:key' as='xs:string'/><xsl:item-type name='t:number' as='xs:double'/>";

    /// <summary>Converts measurement values and preserves insertion order.</summary>
    [Fact]
    public void MapAlias_BindingAndOrder()
    {
        var output = Run(Aliases + """
            <xsl:template match='/'><xsl:variable name='m' as='t:measurements' select="map{'z':1,'a':2}"/>
              <out><xsl:value-of separator='|' select="($m?z instance of xs:double,$m?a,map:keys($m))"/></out>
            </xsl:template>
            """);
        Assert.Equal("true|2|z|a", XElement.Parse(output).Value);
    }

    /// <summary>Uses named map types for direct and dynamic function conversion.</summary>
    [Fact]
    public void MapAlias_FunctionCalls()
        => Assert.Equal("true|3", XElement.Parse(Run(Aliases + """
            <xsl:function name='f:measure' as='t:measurements'><xsl:param name='m' as='t:measurements'/><xsl:sequence select='$m'/></xsl:function>
            <xsl:template match='/'><xsl:variable name='call' select='f:measure#1'/>
              <out><xsl:value-of separator='|' select="(f:measure(map{'x':1})?x instance of xs:double,$call(map{'y':3})?y)"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Checks concrete map types without converting their entries.</summary>
    [Fact]
    public void MapAlias_InstanceAndTreat()
        => Assert.Equal("true|false|2", XElement.Parse(Run(Aliases + """
            <xsl:template match='/'><out><xsl:value-of separator='|' select="(
              map{'x':xs:double(1)} instance of t:measurements,map{'x':1} instance of t:measurements,
              (map{'x':xs:double(2)} treat as t:measurements)?x)"/></out></xsl:template>
            """)).Value);

    /// <summary>Expands aliases in inline array/map operands and host sequence types.</summary>
    [Fact]
    public void NestedReferences_BindingsAndTypeExpressions()
        => Assert.Equal("true|true|true", XElement.Parse(Run(Aliases + """
            <xsl:template match='/'>
              <xsl:variable name='a' as='array(t:number)' select='[1]'/>
              <xsl:variable name='m' as='map(t:key,array(t:number))' select="map{'x':[2]}"/>
              <out><xsl:value-of separator='|' select="($a instance of array(t:number),
                $m instance of map(t:key,array(t:number)),($a treat as array(t:number))(1) instance of t:number)"/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Resolves map definitions against their declaration prefixes and alias references against the caller.</summary>
    [Fact]
    public void MapAlias_DefinitionAndUseNamespaces()
        => Assert.Equal("true", XElement.Parse(Run("""
            <xsl:item-type name='t:measurements' as='map(s:string,s:double)' xmlns:s='http://www.w3.org/2001/XMLSchema'/>
            <xsl:template match='/' xmlns:s='urn:other' xmlns:v='urn:types'>
              <xsl:variable name='m' as='v:measurements' select="map{'x':1}"/>
              <out><xsl:value-of select='$m instance of Q{urn:types}measurements'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Separates delimiters in quoted enumeration members, comments and EQName namespace URIs.</summary>
    [Fact]
    public void NestedReferences_QuotedMembersAndComments()
        => Assert.Equal("true|true", XElement.Parse(Run("""
            <xsl:item-type name='Q{urn:a,b}key' as='xs:string'/>
            <xsl:item-type name='t:states' as="map(Q{urn:a,b}key,enum('t:number,(:literal:)','can''t'))"/>
            <xsl:template match='/'>
              <xsl:variable name='m' as='t:states' select="map{'x':'t:number,(:literal:)'}"/>
              <xsl:variable name='n' as='map(t:key, (: outer (: inner :) :) array(t:number))' select="map{'x':[1]}"/>
              <out><xsl:value-of separator='|' select='($m instance of t:states,$n instance of map(t:key,array(t:number)))'/></out>
            </xsl:template>
            <xsl:item-type name='t:key' as='xs:string'/><xsl:item-type name='t:number' as='xs:double'/>
            """)).Value);

    /// <summary>Permits empty optional maps and repeated map items.</summary>
    [Fact]
    public void MapAlias_Occurrences()
        => Assert.Equal("true|true", XElement.Parse(Run(Aliases + """
            <xsl:template match='/'>
              <xsl:variable name='m' as='t:measurements?' select='()'/>
              <xsl:variable name='n' as='t:measurements+' select="(map{'x':1},map{'y':2})"/>
              <out><xsl:value-of separator='|' select='($m instance of t:measurements?,$n instance of t:measurements+)'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Reports host binding errors for invalid values and cardinalities.</summary>
    /// <param name="expression">The incompatible supplied value.</param>
    [Theory]
    [InlineData("map{'x':'invalid'}")]
    [InlineData("[1]")]
    [InlineData("()")]
    [InlineData("(map{'x':1},map{'y':2})")]
    public void MapAlias_InvalidBinding(string expression)
        => Assert.Contains("XTTE0570", Assert.ThrowsAny<Exception>(() => Run(Aliases + $"<xsl:template match='/'><xsl:variable name='m' as='t:measurements' select=\"{expression}\"/><xsl:sequence select='$m'/></xsl:template>")).ToString());

    /// <summary>Converts map keys under XPath 4.0 and rejects collisions after numeric narrowing.</summary>
    [Fact]
    public void MapCoercion_KeysAndCollisions()
    {
        var output = Run("""
            <xsl:item-type name='t:map' as='map(xs:double,xs:double)'/>
            <xsl:template match='/'><xsl:variable name='m' as='t:map' select='map{1:2}'/>
              <out><xsl:value-of separator='|' select='(map:keys($m) instance of xs:double,$m(1))'/></out>
            </xsl:template>
            """);
        Assert.Equal("true|2", XElement.Parse(output).Value);
        var error = Assert.ThrowsAny<Exception>(() => Run("""
            <xsl:item-type name='t:map' as='map(xs:float,xs:integer)'/>
            <xsl:template match='/'><xsl:variable name='m' as='t:map' select='map{1.2:0,1.2000001:0}'/><xsl:sequence select='$m'/></xsl:template>
            """));
        Assert.Contains("duplicate keys", error.ToString());
    }

    /// <summary>Rejects recursion through map value types during static declaration registration.</summary>
    [Fact]
    public void MapAlias_RecursiveDefinition()
        => Assert.Contains("XTSE4035", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(Wrap("<xsl:item-type name='t:m' as='map(xs:string,array(t:m))'/>"))).ToString());

    /// <summary>Retains the XPath 3.1 refusal of map value numeric promotion.</summary>
    [Fact]
    public void MapCoercion_XPath31Boundary()
    {
        var map = new Bosak.XPath.Core.Xdm.XdmMap().WithAdded(Bosak.XPath.Core.Xdm.XdmValue.FromString("x"),Bosak.XPath.Core.Xdm.XdmValue.FromInteger(1));
        Assert.Throws<InvalidOperationException>(() => Bosak.XPath.Runtime.Vm.VmEngine.ApplyFunctionConversion(
            Bosak.XPath.Core.Xdm.XdmValue.FromMap(map),"map(xs:string,xs:double)",new Bosak.XPath.Runtime.Vm.EvaluationContext()));
    }

    /// <summary>Rejects sequence-valued keys and unterminated nested comments in declarations.</summary>
    /// <param name="definition">The invalid map definition.</param>
    [Theory]
    [InlineData("map(xs:string?,xs:double)")]
    [InlineData("map(xs:string,(: unfinished xs:double)")]
    public void MapAlias_InvalidDefinition(string definition)
        => Assert.Contains("XPST0003", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(
            Wrap($"<xsl:item-type name='t:m' as='{definition}'/>"))).ToString());

    /// <summary>Converts a function result independently of parameter conversion.</summary>
    [Fact]
    public void MapAlias_FunctionResult()
        => Assert.Equal("true", XElement.Parse(Run(Aliases + """
            <xsl:function name='f:measure' as='t:measurements'><xsl:sequence select="map{'x':1}"/></xsl:function>
            <xsl:template match='/'><out><xsl:value-of select='f:measure()?x instance of t:number'/></out></xsl:template>
            """)).Value);

    /// <summary>Accepts whitespace and comments between structural type keywords and opening parentheses.</summary>
    [Fact]
    public void NestedReferences_StructuralWhitespace()
        => Assert.Equal("true", XElement.Parse(Run(Aliases + """
            <xsl:item-type name='t:nested' as='map (: note :) (t:key,array (t:number ?))'/>
            <xsl:template match='/'>
              <xsl:variable name='m' as='t:nested' select="map{'x':[1,()]}"/>
              <out><xsl:value-of select='$m instance of t:nested'/></out>
            </xsl:template>
            """)).Value);

    /// <summary>Converts sensor attribute values into a reusable map of named double readings.</summary>
    [Fact]
    public void MapAlias_SensorReferenceExample()
    {
        var output = Run("""
            <xsl:item-type name='t:readings' as='map(t:sensor,t:value)'/>
            <xsl:item-type name='t:sensor' as="enum('temperature','humidity')"/>
            <xsl:item-type name='t:value' as='xs:double'/>
            <xsl:template match='/'>
              <xsl:variable name='readings' as='t:readings' select="map{'temperature':root/@temperature,'humidity':root/@humidity}"/>
              <reading temperature='{$readings?temperature}' humidity='{$readings?humidity}' typed='{$readings instance of t:readings}'/>
            </xsl:template>
            """, "<root temperature='21' humidity='55'/>");
        Assert.Equal(new[] { "21", "55", "true" }, XElement.Parse(output).Attributes().Select(a => a.Value));
    }

    /// <summary>Retains nominative record identity when a map already satisfies its structural target.</summary>
    [Fact]
    public void MapCoercion_AlreadyMatchingRecordRetainsIdentity()
        => Assert.Equal("true", XElement.Parse(Run("""
            <xsl:record-type name='t:reading' constructor='yes'><xsl:field name='x' as='xs:double'/></xsl:record-type>
            <xsl:function name='f:map' as='map(xs:string,xs:double)'><xsl:param name='m' as='map(xs:string,xs:double)'/><xsl:sequence select='$m'/></xsl:function>
            <xsl:template match='/'><out><xsl:value-of select='f:map(t:reading(1)) instance of t:reading'/></out></xsl:template>
            """)).Value);
}
