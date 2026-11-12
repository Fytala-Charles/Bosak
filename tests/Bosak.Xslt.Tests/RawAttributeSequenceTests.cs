// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 12 November 2026
// PURPOSE              : Tests standalone attributes in typed/raw template sequences and content boundaries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 12-11-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 12-11-2026     | Enumerate XdmSequence explicitly without IEnumerable assumptions.                          |
//                      | Charles Korthout | 0.3   | 12-11-2026     | Preserve namespace simple content built from a nested attribute evaluator.                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;
using Bosak.Xslt.Api;
using Bosak.Xslt.Runtime;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Checks raw attribute item delivery independently of ordinary element construction.</summary>
public class RawAttributeSequenceTests
{
    /// <summary>Adaptive output accepts an attribute after map/function and atomic items.</summary>
    [Fact]
    public void AdaptiveMixedSequenceRetainsStandaloneAttribute()
    {
        var executable = new XsltCompiler().Compile("""
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
              <xsl:output method="adaptive"/>
              <xsl:template name="main">
                <xsl:map-entry key="'a'" select="1"/><xsl:sequence select="name#1"/>
                <xsl:sequence select="1 to 5"/><xsl:attribute name="att"/><xsl:comment>tail</xsl:comment>
              </xsl:template>
            </xsl:stylesheet>
            """);
        var raw = executable.Transform(null, initialTemplate: "main", rawResult: true);
        var items = Items(raw);
        Assert.Equal(9, items.Length);
        Assert.True(items[0].IsMap);
        Assert.True(items[1].IsFunction);
        Assert.Equal(XdmNodeKind.Attribute, items[7].NodeValue!.NodeKind);
        Assert.Empty(Materialize(items[7].NodeValue!.Axis(XdmAxis.Parent)));
        Assert.Equal(XdmNodeKind.Comment, items[8].NodeValue!.NodeKind);
        Assert.Contains("att=", ResultTreeSerializer.Serialize(raw, executable.OutputProperties));
    }

    /// <summary>Attribute placeholders retain source position and duplicate names in raw results.</summary>
    [Fact]
    public void RawAttributesKeepOrderAndDuplicateNames()
    {
        var executable = new XsltCompiler().Compile("""
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
              <xsl:template name="main"><xsl:text>before</xsl:text><xsl:attribute name="a">one</xsl:attribute><xsl:text>after</xsl:text><xsl:attribute name="a">two</xsl:attribute></xsl:template>
            </xsl:stylesheet>
            """);
        var items = Items(executable.Transform(null, initialTemplate: "main", rawResult: true));
        Assert.Equal(new[] { "before", "one", "after", "two" }, items.Select(v => v.NodeValue!.StringValue));
        Assert.Equal(new[] { XdmNodeKind.Text, XdmNodeKind.Attribute, XdmNodeKind.Text, XdmNodeKind.Attribute },
            items.Select(v => v.NodeValue!.NodeKind));
        Assert.All(items.Where(v => v.NodeValue!.NodeKind == XdmNodeKind.Attribute),
            v => Assert.Empty(Materialize(v.NodeValue!.Axis(XdmAxis.Parent))));
    }

    /// <summary>Declared attribute sequence types retain separate same-name attributes.</summary>
    [Fact]
    public void DeclaredAttributeSequenceRetainsBothItems()
    {
        var executable = new XsltCompiler().Compile("""
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
              <xsl:template name="main" as="attribute()*"><xsl:attribute name="a">one</xsl:attribute><xsl:attribute name="a">two</xsl:attribute></xsl:template>
            </xsl:stylesheet>
            """);
        var items = Items(executable.Transform(null, initialTemplate: "main", rawResult: true));
        Assert.Equal(2, items.Length);
        Assert.Equal(new[] { "one", "two" }, items.Select(v => v.NodeValue!.StringValue));
    }

    /// <summary>Constructing an attribute after child content in a real element still fails.</summary>
    [Fact]
    public void ElementContentStillRejectsLateAttribute()
    {
        var executable = new XsltCompiler().Compile("""
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
              <xsl:output method="adaptive"/>
              <xsl:template name="main"><out>child<xsl:attribute name="a">late</xsl:attribute></out></xsl:template>
            </xsl:stylesheet>
            """);
        var error = Assert.ThrowsAny<Exception>(() => executable.Transform(null, initialTemplate: "main", rawResult: true));
        Assert.Contains("XTDE0410", error.Message);
    }

    /// <summary>A namespace's simple-content evaluator keeps its independently materialized attribute.</summary>
    [Fact]
    public void NestedAttributeEvaluationRetainsNamespaceSimpleContent()
    {
        var executable = new XsltCompiler().Compile("""
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
              <xsl:output omit-xml-declaration="yes"/>
              <xsl:template name="main"><root><xsl:call-template name="namespaces"/></root></xsl:template>
              <xsl:template name="namespaces" as="node()*">
                <xsl:namespace name="xml"><xsl:attribute name="select">http://www.w3.org/XML/1998/namespace</xsl:attribute></xsl:namespace>
              </xsl:template>
            </xsl:stylesheet>
            """);
        Assert.Equal("<root/>", executable.TransformToString(null, initialTemplate: "main"));
    }

    private static XdmValue[] Items(XdmValue value)
    {
        Assert.True(value.IsSequence);
        return Materialize(XdmSequence.FromSource(value.SequenceValue!));
    }

    private static XdmValue[] Materialize(XdmSequence sequence)
    {
        var items = new List<XdmValue>();
        foreach (var item in sequence) items.Add(item);
        return items.ToArray();
    }
}
