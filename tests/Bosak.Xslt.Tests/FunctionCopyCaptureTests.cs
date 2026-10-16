// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies function-body copy construction and caller capture restoration.
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

/// <summary>Verifies function-body copy construction and caller capture restoration.</summary>
public class FunctionCopyCaptureTests
{
    private static XdmValue Run(string content, string body = "<xsl:sequence select='f:make($seed)'/>", bool document = false, bool raw = true, bool typed = false, string declarations = "", string version = "4.0") =>
        new Api.XsltCompiler().Compile("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:functions' version='" + version + "'>" + declarations +
            "<xsl:variable name='seed' as='node()'>" + (document ? "<xsl:document><old/></xsl:document>" : "<n old='ignored'><old/></n>") + "</xsl:variable>" +
            "<xsl:function name='f:make' as='item()*'><xsl:param name='seed' as='node()'/>" + content + "</xsl:function>" +
            "<xsl:template name='start'" + (typed ? " as='item()*'" : "") + ">" + body + "</xsl:template></xsl:stylesheet>")
            .Transform(null, initialTemplate: "start", rawResult: raw);
    private static List<XdmValue> Items(XdmValue value)
    {
        if (!value.IsSequence) return [value];
        var result = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) result.Add(item);
        return result;
    }

    /// <summary>Uses complex-content spacing independently of node kind and result capture.</summary>
    /// <param name="document">Whether the source is a document.</param>
    /// <param name="raw">Whether the entry result is raw.</param>
    /// <param name="typed">Whether the entry declares its return type.</param>
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void AtomicSpacing(bool document, bool raw, bool typed)
    {
        var value = Run("<xsl:copy select='$seed'><xsl:sequence select='1,2'/></xsl:copy>", document: document, raw: raw, typed: typed);
        if (raw) Assert.Equal(document ? XdmNodeKind.Document : XdmNodeKind.Element, value.NodeValue.NodeKind);
        Assert.Equal("1 2", value.NodeValue.StringValue);
    }

    /// <summary>Constructs text and multiple roots in document copies.</summary>
    /// <param name="content">The copied content.</param>
    /// <param name="expected">The expected text.</param>
    /// <param name="count">The expected child count.</param>
    [Theory]
    [InlineData("<xsl:select>1,2</xsl:select>", "1 2", 1)]
    [InlineData("<a/><b/>", "", 2)]
    [InlineData("", "", 0)]
    public void DocumentContent(string content, string expected, int count)
    {
        var value = Run("<xsl:copy select='$seed'>" + content + "</xsl:copy>", document: true);
        Assert.Equal(XdmNodeKind.Document, value.NodeValue.NodeKind);
        Assert.Equal(expected, value.NodeValue.StringValue);
        var children = new List<XdmValue>();
        foreach (var child in value.NodeValue.Axis(XdmAxis.Child)) children.Add(child);
        Assert.Equal(count, children.Count);
    }

    /// <summary>Rejects illegal map/function children rather than leaking them into the caller.</summary>
    /// <param name="document">Whether a document is copied.</param>
    /// <param name="expression">The illegal child expression.</param>
    [Theory]
    [InlineData(false, "map{}")]
    [InlineData(false, "function(){1}")]
    [InlineData(true, "map{}")]
    [InlineData(true, "function(){1}")]
    public void InvalidContent(bool document, string expression) => Assert.Contains("XTDE0450", Assert.ThrowsAny<Exception>(() =>
        Run("<xsl:copy select='$seed'><xsl:sequence select='" + expression + "'/></xsl:copy>", document: document)).ToString());

    /// <summary>Restores the caller collector after a function construction error is caught.</summary>
    /// <param name="document">Whether a document is copied.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ErrorRestoration(bool document)
    {
        var items = Items(Run("<xsl:copy select='$seed'><xsl:sequence select='map{}'/></xsl:copy>", "<xsl:try><xsl:sequence select='f:make($seed)'/><xsl:catch><xsl:sequence select='1,2'/></xsl:catch></xsl:try><xsl:sequence select='3,4'/>", document: document));
        Assert.Equal(new long[] { 1,2,3,4 }, items.Select(v => v.IntegerValue));
    }

    /// <summary>Retains ordinary stylesheet text and conditional constructor content.</summary>
    /// <param name="content">The copied content.</param>
    /// <param name="expected">The expected text.</param>
    [Theory]
    [InlineData("literal", "literal")]
    [InlineData("<xsl:on-empty>empty</xsl:on-empty>", "empty")]
    [InlineData("<xsl:sequence select='1,2'/><xsl:on-non-empty><xsl:sequence select='3,4'/></xsl:on-non-empty>", "1 2 3 4")]
    public void ElementContent(string content, string expected) => Assert.Equal(expected, Run("<xsl:copy select='$seed'>" + content + "</xsl:copy>").NodeValue.StringValue);

    /// <summary>Retains outer atomic adjacency when only the string value of a function copy is used.</summary>
    /// <param name="document">Whether a document is copied.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CallerAtomicAdjacency(bool document) => Assert.Equal("1 2 3", Run("<xsl:copy select='$seed'><xsl:sequence select='2'/></xsl:copy>", "<out><xsl:sequence select='1'/><xsl:sequence select='string(f:make($seed))'/><xsl:sequence select='3'/></out>", document: document).NodeValue.StringValue);

    /// <summary>Applies attribute sets while omitting source attributes and children.</summary>
    [Fact]
    public void AttributesAndShallowCopy()
    {
        var value = Run("<xsl:copy select='$seed' use-attribute-sets='a'/>", declarations: "<xsl:attribute-set name='a'><xsl:attribute name='new'>ok</xsl:attribute></xsl:attribute-set>");
        var attributes = new List<XdmValue>();
        foreach (var attribute in value.NodeValue.Axis(XdmAxis.Attribute)) attributes.Add(attribute);
        Assert.Single(attributes);
        Assert.Equal("new", attributes[0].NodeValue.LocalName);
        Assert.Equal("ok", attributes[0].NodeValue.StringValue);
        Assert.Equal("", value.NodeValue.StringValue);
    }

    /// <summary>Honors namespace copy options on the reused element path.</summary>
    /// <param name="option">The copy-namespaces option.</param>
    /// <param name="expected">Whether the unused function namespace remains.</param>
    [Theory]
    [InlineData("yes", true)]
    [InlineData("no", false)]
    public void NamespaceOption(string option, bool expected)
    {
        var value = Run("<xsl:copy select='$seed' copy-namespaces='" + option + "'/>");
        var namespaces = new List<string>();
        foreach (var ns in value.NodeValue.Axis(XdmAxis.Namespace)) namespaces.Add(ns.NodeValue.StringValue);
        Assert.Equal(expected, namespaces.Contains("urn:functions"));
    }

    /// <summary>Rejects namespace children in function document copies.</summary>
    /// <param name="conditional">Whether the conditional collection path is used.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DocumentNamespaceError(bool conditional) => Assert.Contains("XTDE0420", Assert.ThrowsAny<Exception>(() =>
        Run("<xsl:copy select='$seed'><xsl:namespace name='p'>urn:p</xsl:namespace>" + (conditional ? "<xsl:on-empty>empty</xsl:on-empty>" : "") + "</xsl:copy>", document: true)).ToString());

    /// <summary>Flattens nested document children in either copied node kind.</summary>
    /// <param name="document">Whether the source is a document.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NestedDocument(bool document)
    {
        var value = Run("<xsl:copy select='$seed'><xsl:sequence select='$inner'/></xsl:copy>", document: document, declarations:
            "<xsl:variable name='inner' as='document-node()'><xsl:document><child>text</child></xsl:document></xsl:variable>");
        var children = new List<XdmValue>();
        foreach (var child in value.NodeValue.Axis(XdmAxis.Child)) children.Add(child);
        Assert.Single(children);
        Assert.Equal("child", children[0].NodeValue.LocalName);
        Assert.Equal("text", children[0].NodeValue.StringValue);
    }

    /// <summary>Restores the caller when a copied element's attribute set raises.</summary>
    [Fact]
    public void AttributeSetError()
    {
        var items = Items(Run("<xsl:copy select='$seed' use-attribute-sets='a'/>", "<xsl:try><xsl:sequence select='f:make($seed)'/><xsl:catch><xsl:sequence select='1,2'/></xsl:catch></xsl:try><xsl:sequence select='3,4'/>", declarations:
            "<xsl:attribute-set name='a'><xsl:attribute name='a' select='error()'/></xsl:attribute-set>"));
        Assert.Equal(new long[] {1,2,3,4}, items.Select(v => v.IntegerValue));
    }

    /// <summary>Retains inherited behavior in the earlier stylesheet profile.</summary>
    [Fact]
    public void EarlierVersion() => Assert.Equal("1 2", Run("<xsl:copy select='$seed'><xsl:sequence select='1,2'/></xsl:copy>", version: "3.0").NodeValue.StringValue);
}
