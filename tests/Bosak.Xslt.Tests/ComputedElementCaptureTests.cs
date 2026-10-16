// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies computed-element content under raw and typed template capture.
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

using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies computed-element content under raw and typed template capture.</summary>
public class ComputedElementCaptureTests
{
    private static XdmValue Run(string body, bool raw = true, bool typed = false, string declarations = "", string version = "4.0") =>
        new Api.XsltCompiler().Compile("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='" + version + "'>" + declarations +
            "<xsl:template name='start'" + (typed ? " as='item()*'" : "") + ">" + body + "</xsl:template></xsl:stylesheet>")
            .Transform(null, initialTemplate: "start", rawResult: raw);
    private static List<XdmValue> Items(XdmValue value)
    {
        if (!value.IsSequence) return [value];
        var result = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) result.Add(item);
        return result;
    }

    /// <summary>Constructs atomic text with spaces independently of delivery and template return type.</summary>
    /// <param name="selectBody">Whether to use xsl:select.</param>
    /// <param name="raw">Whether to collect a raw result.</param>
    /// <param name="typed">Whether the template declares a return type.</param>
    [Theory]
    [InlineData(true, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, false, true)]
    public void AtomicSpacing(bool selectBody, bool raw, bool typed)
    {
        var body = selectBody ? "<xsl:select>1,2</xsl:select>" : "<xsl:sequence select='1,2'/>";
        var value = Run("<xsl:element name='n'>" + body + "</xsl:element>", raw, typed);
        if (raw) Assert.Equal(XdmNodeKind.Element, value.NodeValue.NodeKind);
        Assert.Equal("1 2", value.NodeValue.StringValue);
    }

    /// <summary>Uses complex-content rules on the conditional item-collection path too.</summary>
    /// <param name="body">The conditional body.</param>
    /// <param name="expected">The expected text.</param>
    [Theory]
    [InlineData("<xsl:sequence select='1,2'/><xsl:on-empty>empty</xsl:on-empty>", "1 2")]
    [InlineData("<xsl:on-empty><xsl:sequence select='1,2'/></xsl:on-empty>", "1 2")]
    [InlineData("<xsl:sequence select='1,2'/><xsl:on-non-empty><xsl:sequence select='3,4'/></xsl:on-non-empty>", "1 2 3 4")]
    public void ConditionalContent(string body, string expected) => Assert.Equal(expected, Run("<xsl:element name='n'>" + body + "</xsl:element>").NodeValue.StringValue);

    /// <summary>Rejects maps and functions as computed-element children without diverting them.</summary>
    /// <param name="expression">The illegal content.</param>
    [Theory]
    [InlineData("map{}")]
    [InlineData("function(){1}")]
    public void InvalidContent(string expression) => Assert.Contains("XTDE0450", Assert.ThrowsAny<Exception>(() =>
        Run("<xsl:element name='n'><xsl:sequence select='" + expression + "'/></xsl:element>")).ToString());

    /// <summary>Restores raw item capture after an element-content error is caught.</summary>
    [Fact]
    public void ErrorRestoration()
    {
        var items = Items(Run("<xsl:try><xsl:element name='n'><xsl:sequence select='map{}'/></xsl:element><xsl:catch><xsl:sequence select='1,2'/></xsl:catch></xsl:try><xsl:sequence select='3,4'/>"));
        Assert.Equal(new long[] { 1,2,3,4 }, items.Select(v => v.IntegerValue));
    }

    /// <summary>Restores capture even when an attribute set raises before the element body.</summary>
    [Fact]
    public void AttributeSetError()
    {
        var items = Items(Run("<xsl:try><xsl:element name='n' use-attribute-sets='a'/><xsl:catch><xsl:sequence select='1,2'/></xsl:catch></xsl:try><xsl:sequence select='3,4'/>", declarations:
            "<xsl:attribute-set name='a'><xsl:attribute name='a' select='error()'/></xsl:attribute-set>"));
        Assert.Equal(new long[] { 1,2,3,4 }, items.Select(v => v.IntegerValue));
    }

    /// <summary>Flattens a document child instead of retaining a synthetic document wrapper.</summary>
    [Fact]
    public void DocumentChild()
    {
        var value = Run("<xsl:variable name='doc' as='document-node()'><xsl:document><child>text</child></xsl:document></xsl:variable><xsl:element name='n'><xsl:sequence select='$doc'/></xsl:element>");
        var children = new List<XdmValue>();
        foreach (var child in value.NodeValue.Axis(XdmAxis.Child)) children.Add(child);
        Assert.Single(children);
        Assert.Equal("child", children[0].NodeValue.LocalName);
        Assert.Equal("text", children[0].NodeValue.StringValue);
    }

    /// <summary>Retains surrounding raw maps/functions and the constructed element's text.</summary>
    [Fact]
    public void OuterFunctionItems()
    {
        var items = Items(Run("<xsl:sequence select='map{}'/><xsl:element name='n'><xsl:sequence select='1,2'/></xsl:element><xsl:sequence select='function(){3}'/>"));
        Assert.Equal(3, items.Count);
        Assert.True(items[0].IsMap);
        Assert.Equal("1 2", items[1].NodeValue.StringValue);
        Assert.True(items[2].IsFunction);
    }

    /// <summary>Ends atomic adjacency at the constructed element boundary.</summary>
    [Fact]
    public void NodeBoundary()
    {
        var value = Run("<out><xsl:sequence select='1'/><xsl:element name='n'><xsl:sequence select='2,3'/></xsl:element><xsl:sequence select='4'/></out>");
        var children = new List<XdmValue>();
        foreach (var child in value.NodeValue.Axis(XdmAxis.Child)) children.Add(child);
        Assert.Equal(3, children.Count);
        Assert.Equal("1", children[0].NodeValue.StringValue);
        Assert.Equal("2 3", children[1].NodeValue.StringValue);
        Assert.Equal("4", children[2].NodeValue.StringValue);
    }

    /// <summary>Preserves element namespace and attribute-set application.</summary>
    [Fact]
    public void NamespaceAndAttributes()
    {
        var value = Run("<xsl:element name='p:n' namespace='urn:n' use-attribute-sets='a'><xsl:sequence select='1,2'/></xsl:element>", declarations:
            "<xsl:attribute-set name='a'><xsl:attribute name='a'>ok</xsl:attribute></xsl:attribute-set>");
        Assert.Equal("urn:n", value.NodeValue.NamespaceUri);
        var attributes = new List<XdmValue>();
        foreach (var attribute in value.NodeValue.Axis(XdmAxis.Attribute)) attributes.Add(attribute);
        Assert.Single(attributes);
        Assert.Equal("ok", attributes[0].NodeValue.StringValue);
        Assert.Equal("1 2", value.NodeValue.StringValue);
    }

    /// <summary>Restores separate atomic items after constructing an empty element.</summary>
    [Fact]
    public void EmptyElement()
    {
        var items = Items(Run("<xsl:sequence select='1'/><xsl:element name='n'/><xsl:sequence select='2'/>"));
        Assert.Equal(3, items.Count);
        Assert.Equal(1, items[0].IntegerValue);
        Assert.Equal(XdmNodeKind.Element, items[1].NodeValue.NodeKind);
        Assert.Equal(2, items[2].IntegerValue);
    }

    /// <summary>Retains the inherited complex-content behavior in the earlier profile.</summary>
    [Fact]
    public void EarlierVersion() => Assert.Equal("1 2", Run("<xsl:element name='n'><xsl:sequence select='1,2'/></xsl:element>", version: "3.0").NodeValue.StringValue);
}
