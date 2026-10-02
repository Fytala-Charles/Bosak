// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 02 oktober 2026
// PURPOSE              : Unit tests for the Saxon 9.x HTMLIndenter port on the xhtml serialization path
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 02-10-2026     | Creation (validation-0201: 3-space levels, inline adjacency, newline folding)           |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for the Saxon 9.x HTMLIndenter port used by method="xhtml" indent="yes"
/// (validation-0201): three spaces per level, no indentation adjacent to inline elements
/// (span, a, br, ...), verbatim content inside formatted elements (pre, script, style,
/// textarea, xmp), end tags indented only when the element did not end on the same line,
/// and text folded at embedded newlines with the following spaces absorbed into the
/// emitted indentation.
/// </summary>
public class XhtmlIndentTests
{
    private const string OutputDecl =
        "<xsl:output method='xhtml' omit-xml-declaration='yes' indent='yes'/>";

    private static string Transform(string body)
    {
        var xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>"
            + OutputDecl
            + "<xsl:template match='/'>"
            + body
            + "</xsl:template></xsl:stylesheet>";
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var src = (IXdmNode)new XDocumentNode(new XDocument(new XElement("input")));
        return executable.TransformToString(src);
    }

    // ----- happy path: block structure with inline mixed content -----

    [Fact]
    public void BlockAndInlineElements_SaxonLayout_ExactIndentation()
    {
        const string result = "<html xmlns=\"http://www.w3.org/1999/xhtml\"><body><div>"
            + "<p>text<span>in</span> tail</p><p>second</p></div></body></html>";

        var actual = Transform(result);

        // 3 spaces per level; inline <span> gets no adjacent whitespace; a paragraph ending
        // in text stays on one line (characters() only clears SameLine at a fold), while a
        // paragraph ending on an inline element's end tag is indented (endElement clears it).
        const string expected =
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
            "   <body>\n" +
            "      <div>\n" +
            "         <p>text<span>in</span> tail\n" +
            "         </p>\n" +
            "         <p>second</p>\n" +
            "      </div>\n" +
            "   </body>\n" +
            "</html>";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AdjacentInlineElements_NoWhitespaceBetweenThem()
    {
        var actual = Transform(
            "<html xmlns='http://www.w3.org/1999/xhtml'><body><div><a href='x'>link</a><span>s</span></div></body></html>");

        // Inline elements never receive adjacent indentation; ending on an inline element
        // suppresses the end-tag indent of the enclosing div.
        const string expected =
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
            "   <body>\n" +
            "      <div><a href=\"x\">link</a><span>s</span></div>\n" +
            "   </body>\n" +
            "</html>";
        Assert.Equal(expected, actual);
    }

    // ----- text folding at embedded newlines -----

    [Fact]
    public void TextWithNewline_FoldedWithIndentAndSpaceAbsorption()
    {
        var actual = Transform(
            "<html xmlns='http://www.w3.org/1999/xhtml'><body><p><xsl:text>line1\n   indented</xsl:text></p></body></html>");

        // The fold emits newline + level*3 spaces (p's content is at level 3) and skips
        // the spaces that followed the newline in the source text (Saxon HTMLIndenter
        // characters()); the end tag indents at the decremented level (2).
        const string expected =
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
            "   <body>\n" +
            "      <p>line1\n" +
            "         indented\n" +
            "      </p>\n" +
            "   </body>\n" +
            "</html>";
        Assert.Equal(expected, actual);
    }

    // ----- formatted elements keep their content verbatim -----

    [Fact]
    public void PreElement_ContentVerbatim_AndFollowingBlockStartsOnSameLine()
    {
        var actual = Transform(
            "<html xmlns='http://www.w3.org/1999/xhtml'><body><pre>a\n   b</pre><p>after</p></body></html>");

        // Formatted tag: no indent before <pre> (inFormattedTag) and no line splitting
        // inside it; the next element abuts the end tag (AfterFormatted), and since plain
        // text leaves SameLine true its end tag stays on the same line too.
        const string expected =
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
            "   <body><pre>a\n   b</pre><p>after</p>\n" +
            "   </body>\n" +
            "</html>";
        Assert.Equal(expected, actual);
    }

    // ----- indent="no" stays byte-exact (regression guard) -----

    [Fact]
    public void IndentNo_MixedContent_NoWhitespaceAdded()
    {
        var xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>"
            + "<xsl:output method='xhtml' omit-xml-declaration='yes' indent='no'/>"
            + "<xsl:template match='/'>"
            + "<html xmlns='http://www.w3.org/1999/xhtml'><body><p>a<span>b</span></p></body></html>"
            + "</xsl:template></xsl:stylesheet>";
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl);
        var src = (IXdmNode)new XDocumentNode(new XDocument(new XElement("input")));
        var actual = executable.TransformToString(src);

        Assert.Equal(
            "<html xmlns=\"http://www.w3.org/1999/xhtml\"><body><p>a<span>b</span></p></body></html>",
            actual);
    }
}
