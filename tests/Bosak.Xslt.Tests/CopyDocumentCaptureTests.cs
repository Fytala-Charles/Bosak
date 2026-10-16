// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies copied-document construction inside raw and typed template results.
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

/// <summary>Verifies copied-document construction inside raw and typed template results.</summary>
public class CopyDocumentCaptureTests
{
    private static XdmValue Run(string body, bool raw = true, bool typed = false, string version = "4.0", string declarations = "") => new Api.XsltCompiler().Compile(
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='" + version + "'>" + declarations + "<xsl:variable name='seed' as='document-node()'><xsl:document><old/></xsl:document></xsl:variable><xsl:template name='start'" +
        (typed ? " as='item()*'" : "") + ">" + body + "</xsl:template></xsl:stylesheet>")
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
    /// <param name="typed">Whether the template declares an item sequence result.</param>
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
        var value = Run("<xsl:copy select='$seed'>" + content + "</xsl:copy>", raw, typed);
        if (raw) Assert.Equal(XdmNodeKind.Document, value.NodeValue.NodeKind);
        Assert.Equal("1 2", value.NodeValue.StringValue);
    }

    /// <summary>Constructs explicit and implicit temporary documents correctly inside a raw entry.</summary>
    /// <param name="body">The variable body.</param>
    [Theory]
    [InlineData("<xsl:copy select='$seed'><xsl:select>1,2</xsl:select></xsl:copy>")]
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
    public void ComplexContent(string content, string expected) => Assert.Equal(expected, Run("<xsl:copy select='$seed'>" + content + "</xsl:copy>").NodeValue.StringValue);

    /// <summary>Restores raw item kinds and count before and after a constructed document.</summary>
    [Fact]
    public void OuterRawSequence()
    {
        var items = Items(Run("<xsl:sequence select='1,2'/><xsl:copy select='$seed'><xsl:sequence select='3,4'/></xsl:copy><xsl:sequence select='5,6'/>"));
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
        Run("<xsl:copy select='$seed'><xsl:sequence select='" + expression + "'/></xsl:copy>")).ToString());

    /// <summary>Restores the raw collector after a caught document-construction error.</summary>
    [Fact]
    public void ErrorRestoration()
    {
        var items = Items(Run("<xsl:try><xsl:copy select='$seed'><xsl:sequence select='map{}'/></xsl:copy><xsl:catch><xsl:sequence select='1,2'/></xsl:catch></xsl:try><xsl:sequence select='3,4'/>"));
        Assert.Equal(new long[] { 1,2,3,4 }, items.Select(v => v.IntegerValue));
    }

    /// <summary>Flattens nested documents into the constructed document without a synthetic wrapper.</summary>
    [Fact]
    public void NestedDocument()
    {
        var value = Run("<xsl:variable name='inner' as='document-node()'><xsl:copy select='$seed'><n>text</n></xsl:copy></xsl:variable><xsl:copy select='$seed'><xsl:sequence select='$inner'/></xsl:copy>");
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
        var items = Items(Run("<xsl:sequence select='map{}'/><xsl:copy select='$seed'><xsl:sequence select='1,2'/></xsl:copy><xsl:sequence select='function(){3}'/>"));
        Assert.Equal(3, items.Count);
        Assert.True(items[0].IsMap);
        Assert.Equal("1 2", items[1].NodeValue.StringValue);
        Assert.True(items[2].IsFunction);
    }

    /// <summary>Corrects the inherited document-construction behavior in the XSLT3.0 profile too.</summary>
    [Fact]
    public void EarlierVersion() => Assert.Equal("1 2", Run("<xsl:copy select='$seed'><xsl:sequence select='1,2'/></xsl:copy>", version: "3.0").NodeValue.StringValue);

    /// <summary>Applies document content rules on conditional collection paths.</summary>
    /// <param name="body">The conditional content.</param>
    /// <param name="expected">The expected text.</param>
    [Theory]
    [InlineData("<xsl:sequence select='1,2'/><xsl:on-empty>empty</xsl:on-empty>", "1 2")]
    [InlineData("<xsl:on-empty><xsl:sequence select='1,2'/></xsl:on-empty>", "1 2")]
    [InlineData("<xsl:sequence select='1,2'/><xsl:on-non-empty><xsl:sequence select='3,4'/></xsl:on-non-empty>", "1 2 3 4")]
    public void ConditionalContent(string body, string expected) => Assert.Equal(expected, Run("<xsl:copy select='$seed'>" + body + "</xsl:copy>").NodeValue.StringValue);

    /// <summary>Retains text-only, multiple-root and empty documents in a typed variable.</summary>
    /// <param name="body">The copied-document content.</param>
    /// <param name="expected">The expected text.</param>
    /// <param name="childCount">The expected number of children.</param>
    [Theory]
    [InlineData("<xsl:sequence select='1,2'/>", "1 2", 1)]
    [InlineData("<a/><b/>", "", 2)]
    [InlineData("", "", 0)]
    public void VariableDocument(string body, string expected, int childCount)
    {
        var value = Run("<xsl:variable name='v' as='document-node()'><xsl:copy select='$seed'>" + body + "</xsl:copy></xsl:variable><xsl:sequence select='$v'/>");
        Assert.Equal(XdmNodeKind.Document, value.NodeValue.NodeKind);
        Assert.Equal(expected, value.NodeValue.StringValue);
        var children = new List<XdmValue>();
        foreach (var child in value.NodeValue.Axis(XdmAxis.Child)) children.Add(child);
        Assert.Equal(childCount, children.Count);
    }

    /// <summary>Rejects attributes and namespace nodes as document children.</summary>
    /// <param name="body">The invalid document content.</param>
    [Theory]
    [InlineData("<xsl:attribute name='bad'>value</xsl:attribute>")]
    [InlineData("<xsl:namespace name='p'>urn:p</xsl:namespace>")]
    [InlineData("<xsl:attribute name='bad'>value</xsl:attribute><xsl:on-empty>empty</xsl:on-empty>")]
    [InlineData("<xsl:namespace name='p'>urn:p</xsl:namespace><xsl:on-empty>empty</xsl:on-empty>")]
    public void InvalidNodeContent(string body) => Assert.Contains("XTDE0420", Assert.ThrowsAny<Exception>(() => Run("<xsl:copy select='$seed'>" + body + "</xsl:copy>")).ToString());

    /// <summary>Preserves source document base URI in an enclosing typed variable.</summary>
    [Fact]
    public void SourceBaseUri()
    {
        var value = Run("<xsl:variable name='v' as='document-node()'><xsl:copy select='$origin'><n/></xsl:copy></xsl:variable><xsl:sequence select='string(base-uri($v))'/>", declarations:
            "<xsl:variable name='origin' as='document-node()' xml:base='https://example.test/source/'><xsl:document><old/></xsl:document></xsl:variable>");
        Assert.Equal("https://example.test/source/", value.StringValue);
    }

    /// <summary>Returns a document through a template with an exact document return type.</summary>
    [Fact]
    public void StrictDocumentResult()
    {
        var value = Run("<xsl:call-template name='strict'/>", declarations:
            "<xsl:template name='strict' as='document-node()'><xsl:copy select='$seed'><xsl:sequence select='1,2'/></xsl:copy></xsl:template>");
        Assert.Equal(XdmNodeKind.Document, value.NodeValue.NodeKind);
        Assert.Equal("1 2", value.NodeValue.StringValue);
    }
}
