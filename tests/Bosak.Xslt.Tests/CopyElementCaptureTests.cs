// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies copied-element content under raw and typed template capture.
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

/// <summary>Verifies copied-element content under raw and typed template capture.</summary>
public class CopyElementCaptureTests
{
    private static XdmValue Run(string body, bool raw = true, bool typed = false, string declarations = "", string version = "4.0") =>
        new Api.XsltCompiler().Compile("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='" + version + "'>" + declarations +
            "<xsl:variable name='seed' as='element()'><n old='ignored'><old/></n></xsl:variable><xsl:template name='start'" + (typed ? " as='item()*'" : "") + ">" + body + "</xsl:template></xsl:stylesheet>")
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
        var value = Run("<xsl:copy select='$seed'>" + body + "</xsl:copy>", raw, typed);
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
    public void ConditionalContent(string body, string expected) => Assert.Equal(expected, Run("<xsl:copy select='$seed'>" + body + "</xsl:copy>").NodeValue.StringValue);

    /// <summary>Rejects maps and functions as copied-element children without diverting them.</summary>
    /// <param name="expression">The illegal content.</param>
    [Theory]
    [InlineData("map{}")]
    [InlineData("function(){1}")]
    public void InvalidContent(string expression) => Assert.Contains("XTDE0450", Assert.ThrowsAny<Exception>(() =>
        Run("<xsl:copy select='$seed'><xsl:sequence select='" + expression + "'/></xsl:copy>")).ToString());

    /// <summary>Restores raw item capture after an element-content error is caught.</summary>
    [Fact]
    public void ErrorRestoration()
    {
        var items = Items(Run("<xsl:try><xsl:copy select='$seed'><xsl:sequence select='map{}'/></xsl:copy><xsl:catch><xsl:sequence select='1,2'/></xsl:catch></xsl:try><xsl:sequence select='3,4'/>"));
        Assert.Equal(new long[] { 1,2,3,4 }, items.Select(v => v.IntegerValue));
    }

    /// <summary>Restores capture even when an attribute set raises before the element body.</summary>
    [Fact]
    public void AttributeSetError()
    {
        var items = Items(Run("<xsl:try><xsl:copy select='$seed' use-attribute-sets='a'/><xsl:catch><xsl:sequence select='1,2'/></xsl:catch></xsl:try><xsl:sequence select='3,4'/>", declarations:
            "<xsl:attribute-set name='a'><xsl:attribute name='a' select='error()'/></xsl:attribute-set>"));
        Assert.Equal(new long[] { 1,2,3,4 }, items.Select(v => v.IntegerValue));
    }

    /// <summary>Flattens a document child instead of retaining a synthetic document wrapper.</summary>
    [Fact]
    public void DocumentChild()
    {
        var value = Run("<xsl:variable name='doc' as='document-node()'><xsl:document><child>text</child></xsl:document></xsl:variable><xsl:copy select='$seed'><xsl:sequence select='$doc'/></xsl:copy>");
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
        var items = Items(Run("<xsl:sequence select='map{}'/><xsl:copy select='$seed'><xsl:sequence select='1,2'/></xsl:copy><xsl:sequence select='function(){3}'/>"));
        Assert.Equal(3, items.Count);
        Assert.True(items[0].IsMap);
        Assert.Equal("1 2", items[1].NodeValue.StringValue);
        Assert.True(items[2].IsFunction);
    }

    /// <summary>Ends atomic adjacency at the constructed element boundary.</summary>
    [Fact]
    public void NodeBoundary()
    {
        var value = Run("<out><xsl:sequence select='1'/><xsl:copy select='$seed'><xsl:sequence select='2,3'/></xsl:copy><xsl:sequence select='4'/></out>");
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
        var value = Run("<xsl:copy select='$namespaced' use-attribute-sets='a'><xsl:sequence select='1,2'/></xsl:copy>", declarations:
            "<xsl:variable name='namespaced' as='element()'><p:n xmlns:p='urn:n'/></xsl:variable><xsl:attribute-set name='a'><xsl:attribute name='a'>ok</xsl:attribute></xsl:attribute-set>");
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
        var items = Items(Run("<xsl:sequence select='1'/><xsl:copy select='$seed'/><xsl:sequence select='2'/>"));
        Assert.Equal(3, items.Count);
        Assert.Equal(1, items[0].IntegerValue);
        Assert.Equal(XdmNodeKind.Element, items[1].NodeValue.NodeKind);
        Assert.Equal(2, items[2].IntegerValue);
    }

    /// <summary>Retains the inherited complex-content behavior in the earlier profile.</summary>
    [Fact]
    public void EarlierVersion() => Assert.Equal("1 2", Run("<xsl:copy select='$seed'><xsl:sequence select='1,2'/></xsl:copy>", version: "3.0").NodeValue.StringValue);
    /// <summary>Performs a shallow copy without retaining source attributes or children.</summary>
    [Fact]
    public void ShallowCopy()
    {
        var value = Run("<xsl:copy select='$seed'/>");
        Assert.Equal("n", value.NodeValue.LocalName);
        Assert.Equal("", value.NodeValue.StringValue);
        var attributes = new List<XdmValue>();
        foreach (var attribute in value.NodeValue.Attributes()) attributes.Add(attribute);
        Assert.Empty(attributes);
    }

    /// <summary>Constructs copied content when the source is the context item.</summary>
    /// <param name="typed">Whether the entry template declares a return type.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ContextItemCopy(bool typed) => Assert.Equal("1 2", Run("<xsl:for-each select='$seed'><xsl:copy><xsl:sequence select='1,2'/></xsl:copy></xsl:for-each>", typed: typed).NodeValue.StringValue);

    /// <summary>Rejects illegal content on the conditional collection path.</summary>
    /// <param name="expression">The illegal child value.</param>
    [Theory]
    [InlineData("map{}")]
    [InlineData("function(){1}")]
    public void ConditionalInvalidContent(string expression) => Assert.Contains("XTDE0450", Assert.ThrowsAny<Exception>(() =>
        Run("<xsl:copy select='$seed'><xsl:sequence select='" + expression + "'/><xsl:on-empty>empty</xsl:on-empty></xsl:copy>")).ToString());

    /// <summary>Delivers the completed element to a surrounding typed variable.</summary>
    [Fact]
    public void VariableCapture() => Assert.Equal("1 2", Run("<xsl:variable name='v' as='element()'><xsl:copy select='$seed'><xsl:sequence select='1,2'/></xsl:copy></xsl:variable><xsl:sequence select='$v'/>").NodeValue.StringValue);

    /// <summary>Attaches explicitly supplied attributes to the copied element.</summary>
    [Fact]
    public void AttributeContent()
    {
        var value = Run("<xsl:copy select='$seed'><xsl:sequence select='$seed/@old'/><xsl:text>new</xsl:text></xsl:copy>");
        var attributes = new List<XdmValue>();
        foreach (var attribute in value.NodeValue.Attributes()) attributes.Add(attribute);
        Assert.Single(attributes);
        Assert.Equal("ignored", attributes[0].NodeValue.StringValue);
        Assert.Equal("new", value.NodeValue.StringValue);
    }
}
