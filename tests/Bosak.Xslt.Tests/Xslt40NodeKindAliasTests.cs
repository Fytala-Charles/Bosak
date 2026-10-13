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
//                      | Charles Korthout | 0.2   | 13-10-2026     | Cover named and wildcard node aliases with declaration namespaces
//                      | Charles Korthout | 0.3   | 13-10-2026     | Verify processing-instruction alias target matching and invalid target diagnostics
//                      | Charles Korthout | 0.4   | 13-10-2026     | Verify partial node wildcards and the any default namespace
//                      | Charles Korthout | 0.5 | 13-10-2026     | Verify node-name union alternatives, declaration bindings and separators
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

    private static string Run(string body, string source = "<root a='value'>text<!--comment--><?target value?></root>") => XElement.Parse(Compile(body).TransformToString(
        new XDocumentNode(XDocument.Parse(source)))).Value;

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

    /// <summary>Matches named node tests using the namespace of the alias declaration.</summary>
    /// <param name="type">The declared node type.</param>
    /// <param name="select">The node expression.</param>
    /// <param name="source">The source document.</param>
    [Theory]
    [InlineData("element(p:Root)", "/*", "<Root xmlns='urn:payload'/>")]
    [InlineData("element(Q{urn:payload}Root)", "/*", "<Root xmlns='urn:payload'/>")]
    [InlineData("attribute(p:Code)", "/*/@*", "<root xmlns:p='urn:payload' p:Code='x'/>")]
    [InlineData("element(*)", "/*", "<Root xmlns='urn:other'/>")]
    [InlineData("attribute(*)", "/*/@*", "<root Code='x'/>")]
    [InlineData("document-node(element(p:Root))", ".", "<Root xmlns='urn:payload'/>")]
    public void Alias_NamedNode(string type, string select, string source)
        => Assert.Equal("true|true", Run($"<xsl:item-type xmlns:p='urn:payload' name='t:n' as='{type}'/>" +
            $"<xsl:template match='/' xmlns:p='urn:other'><xsl:variable name='n' as='t:n' select='{select}'/>" +
            "<out><xsl:value-of separator='|' select='($n instance of t:n,($n treat as t:n) is $n)'/></out></xsl:template>", source));

    /// <summary>Distinguishes case, local names and namespaces for expanded node tests.</summary>
    /// <param name="type">The alias definition.</param>
    /// <param name="source">A node that must not match.</param>
    [Theory]
    [InlineData("element(Q{}Root)", "<root/>")]
    [InlineData("element(Q{}Root)", "<Root xmlns='urn:other'/>")]
    [InlineData("attribute(Q{}Code)", "<root code='x'/>")]
    [InlineData("attribute(Q{}Code)", "<root xmlns:p='urn:other' p:Code='x'/>")]
    [InlineData("document-node(element(Q{}Root))", "<root/>")]
    public void Alias_NamedNodeMismatch(string type, string source)
        => Assert.Equal("false", Run($"<xsl:item-type name='t:n' as='{type}'/><xsl:template match='/'><out>" +
            $"<xsl:value-of select='{(type.StartsWith("attribute") ? "/*/@*" : type.StartsWith("document") ? "." : "/*")} instance of t:n'/>" +
            "</out></xsl:template>", source));

    /// <summary>Applies the declaration default to elements while unprefixed attributes stay unqualified.</summary>
    [Fact]
    public void Alias_DefaultNamespaces()
        => Assert.Equal("true|true|true", Run("""
            <xsl:item-type name='t:e' xpath-default-namespace='urn:payload' as='element(Root)'/>
            <xsl:item-type name='t:a' xpath-default-namespace='urn:payload' as='attribute(Code)'/>
            <xsl:item-type name='t:plain' as='element(root)'/>
            <xsl:template match='/' xpath-default-namespace='urn:other'><out>
              <xsl:value-of separator='|' select='(/* instance of t:e,/*/@* instance of t:a,/*/* instance of t:plain)'/>
            </out></xsl:template>
            """, "<Root xmlns='urn:payload' Code='x'><root xmlns=''/></Root>"));

    /// <summary>Traverses named node types without confusing node names with type names.</summary>
    [Fact]
    public void Alias_NestedNamedNodes()
        => Assert.Equal("true|true", Run("""
            <xsl:item-type xmlns:p='urn:payload' name='t:arr' as='array(element(p:Root))'/>
            <xsl:record-type name='t:r'><xsl:field xmlns:p='urn:payload' name='body' as='element(p:Root)'/></xsl:record-type>
            <xsl:template match='/'><xsl:variable name='r' select="map{'body':/*} cast as t:r"/>
              <out><xsl:value-of separator='|' select='([/*] instance of t:arr,$r?body is /*)'/></out>
            </xsl:template>
            """, "<Root xmlns='urn:payload'/>") );

    /// <summary>Rejects undeclared node prefixes at alias registration.</summary>
    /// <param name="type">A node type with an unknown prefix.</param>
    [Theory]
    [InlineData("element(absent:Root)")]
    [InlineData("attribute(absent:Code)")]
    [InlineData("document-node(element(absent:Root))")]
    public void Alias_UnboundNodePrefix(string type)
        => Assert.Contains("XPST0081", Assert.ThrowsAny<Exception>(() => Compile(
            $"<xsl:item-type name='t:n' as='{type}'/>")).ToString());

    /// <summary>Validates type names beside named node tests in unused declarations.</summary>
    [Fact]
    public void Declaration_NamedNodeAndUnknownType()
        => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => Compile(
            "<xsl:variable name='v' as='(element(Root) | t:missing)?' select='()'/>")).ToString());

    /// <summary>Matches targeted processing instructions without expanding targets as QNames.</summary>
    /// <param name="type">The processing-instruction alias definition.</param>
    [Theory]
    [InlineData("processing-instruction(Target)")]
    [InlineData("processing-instruction('Target')")]
    [InlineData("processing-instruction(\"  Target  \")")]
    [InlineData("processing-instruction (:a (:b:) :) (Target)")]
    public void Alias_ProcessingInstructionTarget(string type)
        => Assert.Equal("true|false|true|false", Run(
            "<xsl:item-type xpath-default-namespace='urn:irrelevant' name='t:pi' as='" +
            type.Replace("'", "&apos;").Replace("\"", "&quot;") + "'/>" + """
            <xsl:template match='/'>
              <xsl:variable name='pi' as='t:pi' select='/*/processing-instruction(Target)'/>
              <out><xsl:value-of separator='|' select='($pi instance of t:pi,
                /*/processing-instruction(target) instance of t:pi,($pi treat as t:pi) is $pi,
                /* instance of t:pi)'/></out>
            </xsl:template>
            """, "<root><?Target x?><?target y?></root>"));

    /// <summary>Handles targeted tests inside containers and choice casts.</summary>
    [Fact]
    public void Alias_NestedProcessingInstructions()
        => Assert.Equal("true|true", Run("""
            <xsl:item-type name='t:arr' as="array(processing-instruction('target'))"/>
            <xsl:item-type name='t:pi' as="processing-instruction('target')"/>
            <xsl:template match='/'><out><xsl:value-of separator='|' select='(
              [/*/processing-instruction()] instance of t:arr,() instance of t:pi?)'/></out></xsl:template>
            """));

    /// <summary>Rejects malformed and invalid targets during alias registration.</summary>
    /// <param name="type">An invalid targeted node type.</param>
    /// <param name="code">The expected diagnostic.</param>
    [Theory]
    [InlineData("processing-instruction(absent:target)", "XPST0003")]
    [InlineData("processing-instruction(42)", "XPST0003")]
    [InlineData("processing-instruction(target,other)", "XPST0003")]
    [InlineData("processing-instruction('')", "XPTY0004")]
    [InlineData("processing-instruction('a b')", "XPTY0004")]
    [InlineData("processing-instruction('a:b')", "XPTY0004")]
    public void Alias_InvalidProcessingInstructionTarget(string type, string code)
        => Assert.Contains(code, Assert.ThrowsAny<Exception>(() => Compile(
            "<xsl:item-type name='t:pi' as='" + type.Replace("'", "&apos;") + "'/>")).ToString());

    /// <summary>Validates unknown type names beside targeted processing-instruction tests.</summary>
    [Fact]
    public void Declaration_ProcessingInstructionAndUnknownType()
        => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => Compile(
            "<xsl:variable name='v' as='(processing-instruction(target) | t:missing)?' select='()'/>")).ToString());

    /// <summary>Matches local-name and namespace wildcards while preserving declaration bindings.</summary>
    /// <param name="type">The wildcard alias definition.</param>
    /// <param name="source">A source node matching the wildcard.</param>
    [Theory]
    [InlineData("element(*:Root)", "<Root xmlns='urn:other'/>")]
    [InlineData("element(p:*)", "<Anything xmlns='urn:payload'/>")]
    [InlineData("element(Q{urn:payload}*)", "<Anything xmlns='urn:payload'/>")]
    [InlineData("element(Q{}*)", "<Anything/>")]
    [InlineData("attribute(*:Code)", "<root xmlns:p='urn:other' p:Code='x'/>")]
    [InlineData("attribute(p:*)", "<root xmlns:p='urn:payload' p:Anything='x'/>")]
    [InlineData("attribute(Q{}*)", "<root Anything='x'/>")]
    [InlineData("document-node(element(*:Root))", "<Root xmlns='urn:other'/>")]
    [InlineData("document-node(element(p:*))", "<Anything xmlns='urn:payload'/>")]
    public void Alias_PartialWildcard(string type, string source)
    {
        var select = type.StartsWith("attribute") ? "/*/@*" : type.StartsWith("document") ? "." : "/*";
        Assert.Equal("true|true", Run($"<xsl:item-type xmlns:p='urn:payload' name='t:n' as='{type}'/>" +
            $"<xsl:template match='/' xmlns:p='urn:other'><xsl:variable name='n' as='t:n' select='{select}'/>" +
            "<out><xsl:value-of separator='|' select='($n instance of t:n,($n treat as t:n) is $n)'/></out></xsl:template>", source));
    }

    /// <summary>Rejects the wrong local name or namespace rather than treating a partial wildcard as any node.</summary>
    /// <param name="type">The wildcard alias definition.</param>
    /// <param name="source">A source node that must not match.</param>
    [Theory]
    [InlineData("element(*:Root)", "<root/>")]
    [InlineData("element(p:*)", "<Root xmlns='urn:other'/>")]
    [InlineData("element(Q{}*)", "<Root xmlns='urn:other'/>")]
    [InlineData("attribute(*:Code)", "<root code='x'/>")]
    [InlineData("attribute(p:*)", "<root Code='x'/>")]
    [InlineData("attribute(Q{}*)", "<root xmlns:p='urn:other' p:Code='x'/>")]
    public void Alias_PartialWildcardMismatch(string type, string source)
    {
        var select = type.StartsWith("attribute") ? "/*/@*" : "/*";
        Assert.Equal("false", Run($"<xsl:item-type xmlns:p='urn:payload' name='t:n' as='{type}'/>" +
            $"<xsl:template match='/'><out><xsl:value-of select='{select} instance of t:n'/></out></xsl:template>", source));
    }

    /// <summary>Reports undeclared namespace wildcard prefixes during alias registration.</summary>
    /// <param name="type">The invalid wildcard type.</param>
    [Theory]
    [InlineData("element(absent:*)")]
    [InlineData("attribute(absent:*)")]
    public void Alias_UnboundWildcardPrefix(string type)
        => Assert.Contains("XPST0081", Assert.ThrowsAny<Exception>(() => Compile(
            $"<xsl:item-type name='t:n' as='{type}'/>")).ToString());

    /// <summary>Preserves wildcard meaning inside arrays and direct type tests.</summary>
    [Fact]
    public void Alias_NestedWildcard()
        => Assert.Equal("true|true|true", Run("""
            <xsl:item-type name='t:arr' as='array(element(*:Root))'/>
            <xsl:template match='/' xmlns:p='urn:payload'><out>
              <xsl:value-of separator='|' select='([/*] instance of t:arr,
                /* instance of element(p:*),/* instance of element(*:Root))'/>
            </out></xsl:template>
            """, "<Root xmlns='urn:payload'/>") );

    /// <summary>Expands the any default for element names while keeping attributes unqualified.</summary>
    [Fact]
    public void Alias_AnyDefaultNamespace()
        => Assert.Equal("true|true|true", Run("""
            <xsl:item-type name='t:e' xpath-default-namespace='##any' as='element(Root)'/>
            <xsl:item-type name='t:a' xpath-default-namespace='##any' as='attribute(Code)'/>
            <xsl:template match='/' xpath-default-namespace='##any'><out>
              <xsl:value-of separator='|' select='(/* instance of t:e,/*/@* instance of t:a,
                /* instance of element(Root))'/>
            </out></xsl:template>
            """, "<Root xmlns='urn:payload' Code='x'/>") );

    /// <summary>Does not allow wildcard node tests to hide unknown adjacent type names.</summary>
    [Fact]
    public void Declaration_WildcardAndUnknownType()
        => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => Compile(
            "<xsl:variable name='v' as='(element(*:Root) | t:missing)?' select='()'/>")).ToString());

    /// <summary>Matches every node-name union alternative without changing node identity.</summary>
    /// <param name="type">The union alias definition.</param>
    /// <param name="source">A source matching one alternative.</param>
    [Theory]
    [InlineData("element(doctor|nurse)", "<doctor/>")]
    [InlineData("element(doctor|nurse)", "<nurse/>")]
    [InlineData("attribute(price|cost)", "<root price='x'/>")]
    [InlineData("attribute(price|cost)", "<root cost='x'/>")]
    [InlineData("element(p:doctor|q:nurse)", "<nurse xmlns='urn:staff'/>")]
    [InlineData("element(p:*|*:nurse)", "<nurse xmlns='urn:other'/>")]
    [InlineData("element(p:*|*:nurse)", "<doctor xmlns='urn:payload'/>")]
    [InlineData("attribute(p:*|*:cost)", "<root xmlns:a='urn:other' a:cost='x'/>")]
    [InlineData("document-node(element(doctor|nurse))", "<nurse/>")]
    [InlineData("element(Q{urn:pipe|comma,part}doctor|nurse)", "<doctor xmlns='urn:pipe|comma,part'/>")]
    public void Alias_NodeNameUnion(string type, string source)
    {
        var select = type.StartsWith("attribute") ? "/*/@*" : type.StartsWith("document") ? "." : "/*";
        Assert.Equal("true|true", Run($"<xsl:item-type xmlns:p='urn:payload' xmlns:q='urn:staff' name='t:n' as='{type}'/>" +
            $"<xsl:template match='/' xmlns:p='urn:other' xmlns:q='urn:other'><xsl:variable name='n' as='t:n' select='{select}'/>" +
            "<out><xsl:value-of separator='|' select='($n instance of t:n,($n treat as t:n) is $n)'/></out></xsl:template>", source));
    }

    /// <summary>Rejects nodes outside the union, including case and namespace mismatches.</summary>
    /// <param name="type">The union alias definition.</param>
    /// <param name="source">A source outside both alternatives.</param>
    [Theory]
    [InlineData("element(doctor|nurse)", "<patient/>")]
    [InlineData("element(doctor|nurse)", "<Nurse/>")]
    [InlineData("element(p:doctor|q:nurse)", "<nurse xmlns='urn:other'/>")]
    [InlineData("element(p:*|*:nurse)", "<doctor xmlns='urn:other'/>")]
    [InlineData("attribute(price|cost)", "<root xmlns:p='urn:other' p:cost='x'/>")]
    public void Alias_NodeNameUnionMismatch(string type, string source)
    {
        var select = type.StartsWith("attribute") ? "/*/@*" : "/*";
        Assert.Equal("false", Run($"<xsl:item-type xmlns:p='urn:payload' xmlns:q='urn:staff' name='t:n' as='{type}'/>" +
            $"<xsl:template match='/'><out><xsl:value-of select='{select} instance of t:n'/></out></xsl:template>", source));
    }

    /// <summary>Reports empty alternatives as malformed node-name unions.</summary>
    /// <param name="type">The malformed union type.</param>
    [Theory]
    [InlineData("element(|nurse)")]
    [InlineData("attribute(price|)")]
    [InlineData("element(doctor||nurse)")]
    public void Alias_EmptyNodeNameUnionAlternative(string type)
        => Assert.Contains("XPST0003", Assert.ThrowsAny<Exception>(() => Compile(
            $"<xsl:item-type name='t:n' as='{type}'/>")).ToString());

    /// <summary>Resolves even a later union alternative during registration.</summary>
    [Fact]
    public void Alias_NodeNameUnionUnboundPrefix()
        => Assert.Contains("XPST0081", Assert.ThrowsAny<Exception>(() => Compile(
            "<xsl:item-type name='t:n' as='element(doctor|absent:nurse)'/>")).ToString());

    /// <summary>Does not hide unknown adjacent type names behind node-name unions.</summary>
    [Fact]
    public void Declaration_NodeNameUnionAndUnknownType()
        => Assert.Contains("XPST0051", Assert.ThrowsAny<Exception>(() => Compile(
            "<xsl:variable name='v' as='(element(doctor|nurse) | t:missing)?' select='()'/>")).ToString());

    /// <summary>Traverses unions inside arrays and record fields and in direct tests.</summary>
    [Fact]
    public void Alias_NestedNodeNameUnion()
        => Assert.Equal("true|true|true", Run("""
            <xsl:item-type name='t:arr' as='array(element(doctor|nurse))'/>
            <xsl:item-type name='t:record' as='record(staff as element(doctor|nurse))'/>
            <xsl:template match='/'><out><xsl:value-of separator='|' select='(
              [/*] instance of t:arr,(map{"staff":/*} cast as t:record) instance of t:record,
              /* instance of element(doctor|nurse))'/></out></xsl:template>
            """, "<nurse/>"));

    /// <summary>Applies the any default independently to each unprefixed element alternative.</summary>
    [Fact]
    public void Alias_NodeNameUnionAnyDefault()
        => Assert.Equal("true|true", Run("""
            <xsl:item-type name='t:e' xpath-default-namespace='##any' as='element(doctor|nurse)'/>
            <xsl:item-type name='t:a' xpath-default-namespace='##any' as='attribute(price|cost)'/>
            <xsl:template match='/'><out><xsl:value-of separator='|' select='(
              /* instance of t:e,/*/@* instance of t:a)'/></out></xsl:template>
            """, "<nurse xmlns='urn:staff' cost='x'/>"));
}
