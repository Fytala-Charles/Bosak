// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies document construction inside raw and typed template results.
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

/// <summary>Verifies document construction inside raw and typed template results.</summary>
public class DocumentConstructionCaptureTests
{
    private static XdmValue Run(string body, bool raw = true, bool typed = false, string version = "4.0") => new Api.XsltCompiler().Compile(
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='" + version + "'><xsl:template name='start'" +
        (typed ? " as='document-node()'" : "") + ">" + body + "</xsl:template></xsl:stylesheet>")
        .Transform(null, initialTemplate: "start", rawResult: raw);
    private static List<XdmValue> Items(XdmValue value)
    {
        if (!value.IsSequence) return [value];
        var result = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) result.Add(item);
        return result;
    }

    /// <summary>Applies complex-content atomic spacing independently of outer delivery and return type.</summary>
    /// <param name="selectBody">Whether the expression is supplied using xsl:select.</param>
    /// <param name="raw">Whether delivery collects raw items.</param>
    /// <param name="typed">Whether the template declares a document result.</param>
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
        var content = selectBody ? "<xsl:select>1,2</xsl:select>" : "<xsl:sequence select='1,2'/>";
        var value = Run("<xsl:document>" + content + "</xsl:document>", raw, typed);
        if (raw) Assert.Equal(XdmNodeKind.Document, value.NodeValue.NodeKind);
        Assert.Equal("1 2", value.NodeValue.StringValue);
    }

    /// <summary>Constructs explicit and implicit temporary documents correctly inside a raw entry.</summary>
    /// <param name="body">The variable body.</param>
    [Theory]
    [InlineData("<xsl:document><xsl:select>1,2</xsl:select></xsl:document>")]
    [InlineData("<xsl:sequence select='1,2'/>")]
    public void TemporaryDocuments(string body) => Assert.Equal("1 2", Run("<xsl:variable name='v'>" + body + "</xsl:variable><xsl:sequence select='$v'/>").NodeValue.StringValue);

    /// <summary>Flattens arrays and retains node/text boundaries during document construction.</summary>
    /// <param name="content">The constructor body.</param>
    /// <param name="expected">The expected string value.</param>
    [Theory]
    [InlineData("<xsl:select>[1,(),(2,3)]</xsl:select>", "1 2 3")]
    [InlineData("<xsl:sequence select='[1,(),(2,3)]'/>", "1 2 3")]
    [InlineData("<xsl:sequence select='1,2'/><xsl:text>T</xsl:text><xsl:sequence select='3,4'/>", "1 2T3 4")]
    [InlineData("<xsl:sequence select='1'/><n/><xsl:sequence select='2'/>", "12")]
    public void ComplexContent(string content, string expected) => Assert.Equal(expected, Run("<xsl:document>" + content + "</xsl:document>").NodeValue.StringValue);

    /// <summary>Restores raw item kinds and count before and after a constructed document.</summary>
    [Fact]
    public void OuterRawSequence()
    {
        var items = Items(Run("<xsl:sequence select='1,2'/><xsl:document><xsl:sequence select='3,4'/></xsl:document><xsl:sequence select='5,6'/>"));
        Assert.Equal(5, items.Count);
        Assert.Equal(new long[] { 1,2,5,6 }, new[] { items[0].IntegerValue,items[1].IntegerValue,items[3].IntegerValue,items[4].IntegerValue });
        Assert.Equal(XdmNodeKind.Document, items[2].NodeValue.NodeKind);
        Assert.Equal("3 4", items[2].NodeValue.StringValue);
    }

    /// <summary>Does not divert illegal document maps/functions into the enclosing raw result.</summary>
    /// <param name="expression">The illegal document content.</param>
    [Theory]
    [InlineData("map{}")]
    [InlineData("function(){1}")]
    public void InvalidDocumentContent(string expression) => Assert.Contains("XTDE0450", Assert.ThrowsAny<Exception>(() =>
        Run("<xsl:document><xsl:sequence select='" + expression + "'/></xsl:document>")).ToString());

    /// <summary>Restores the raw collector after a caught document-construction error.</summary>
    [Fact]
    public void ErrorRestoration()
    {
        var items = Items(Run("<xsl:try><xsl:document><xsl:sequence select='map{}'/></xsl:document><xsl:catch><xsl:sequence select='1,2'/></xsl:catch></xsl:try><xsl:sequence select='3,4'/>"));
        Assert.Equal(new long[] { 1,2,3,4 }, items.Select(v => v.IntegerValue));
    }

    /// <summary>Flattens nested documents into the constructed document without a synthetic wrapper.</summary>
    [Fact]
    public void NestedDocument()
    {
        var value = Run("<xsl:variable name='inner' as='document-node()'><xsl:document><n>text</n></xsl:document></xsl:variable><xsl:document><xsl:sequence select='$inner'/></xsl:document>");
        var children = new List<IXdmNode>();
        foreach (var child in value.NodeValue.Axis(XdmAxis.Child)) if (child.IsNode) children.Add(child.NodeValue);
        Assert.Single(children);
        Assert.Equal("n", children[0].LocalName);
        Assert.Equal("text", children[0].StringValue);
    }

    /// <summary>Preserves maps and functions in the enclosing raw sequence after document construction.</summary>
    [Fact]
    public void OuterFunctionItems()
    {
        var items = Items(Run("<xsl:sequence select='map{}'/><xsl:document><xsl:sequence select='1,2'/></xsl:document><xsl:sequence select='function(){3}'/>"));
        Assert.Equal(3, items.Count);
        Assert.True(items[0].IsMap);
        Assert.Equal("1 2", items[1].NodeValue.StringValue);
        Assert.True(items[2].IsFunction);
    }

    /// <summary>Corrects the inherited document-construction behavior in the XSLT3.0 profile too.</summary>
    [Fact]
    public void EarlierVersion() => Assert.Equal("1 2", Run("<xsl:document><xsl:sequence select='1,2'/></xsl:document>", version: "3.0").NodeValue.StringValue);
}
