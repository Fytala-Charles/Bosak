// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 juni 2026
// PURPOSE              : Unit tests for xsl:iterate support in the result-tree path.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 26-06-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 21-09-2026     | xsl:break inside xsl:if (XSLT 3.0 §8.4): termination/select value, for-each and not-last negative cases |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

public class IterateTests
{
    [Fact]
    public void Iterate_Basic_LiteralElements()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out>
                    <xsl:iterate select='//ITEM/TITLE'>
                        <item position='{position()}' last='{last()}'>
                            <xsl:copy-of select='.'/>
                        </item>
                    </xsl:iterate>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var sourceXml = @"<BOOKLIST><BOOKS>
            <ITEM><TITLE>Pride and Prejudice</TITLE></ITEM>
            <ITEM><TITLE>Wuthering Heights</TITLE></ITEM>
            <ITEM><TITLE>Tess of the d'Urbervilles</TITLE></ITEM>
        </BOOKS></BOOKLIST>";

        var source = XDocument.Parse(sourceXml);
        var compiler = new XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<item position=\"1\" last=\"3\">", result);
        Assert.Contains("<item position=\"3\" last=\"3\">", result);
        Assert.Contains("Pride and Prejudice", result);
    }

    [Fact]
    public void Iterate_ValueOf()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out>
                    <xsl:iterate select='//ITEM/TITLE'>
                        <xsl:value-of select='.'/>
                    </xsl:iterate>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var sourceXml = @"<BOOKLIST><BOOKS>
            <ITEM><TITLE>A</TITLE></ITEM>
            <ITEM><TITLE>B</TITLE></ITEM>
        </BOOKS></BOOKLIST>";

        var source = XDocument.Parse(sourceXml);
        var compiler = new XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("AB", result);
    }

    [Fact]
    public void Break_Inside_If_Terminates_Iteration()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out>
                    <xsl:iterate select='//ITEM/TITLE'>
                        <xsl:copy-of select='.'/>
                        <xsl:if test='position() eq 2'>
                            <xsl:break/>
                        </xsl:if>
                    </xsl:iterate>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var sourceXml = @"<BOOKLIST><BOOKS>
            <ITEM><TITLE>A</TITLE></ITEM>
            <ITEM><TITLE>B</TITLE></ITEM>
            <ITEM><TITLE>C</TITLE></ITEM>
        </BOOKS></BOOKLIST>";

        var source = XDocument.Parse(sourceXml);
        var compiler = new XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("<TITLE>A</TITLE>", result);
        Assert.Contains("<TITLE>B</TITLE>", result);
        Assert.DoesNotContain("C", result);
    }

    [Fact]
    public void Break_With_Select_Inside_If_Returns_Value()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out>
                    <xsl:iterate select='//ITEM'>
                        <xsl:if test='TITLE eq ""B""'>
                            <xsl:break select='TITLE'/>
                        </xsl:if>
                    </xsl:iterate>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var sourceXml = @"<BOOKLIST><BOOKS>
            <ITEM><TITLE>A</TITLE></ITEM>
            <ITEM><TITLE>B</TITLE></ITEM>
            <ITEM><TITLE>C</TITLE></ITEM>
        </BOOKS></BOOKLIST>";

        var source = XDocument.Parse(sourceXml);
        var compiler = new XsltCompiler();
        var executable = compiler.Compile(xsl);
        var result = executable.TransformToString(new XDocumentNode(source));

        Assert.Contains("B", result);
        Assert.DoesNotContain("C", result);
    }

    [Fact]
    public void Break_Inside_ForEach_Still_Throws()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out>
                    <xsl:iterate select='//ITEM/TITLE'>
                        <xsl:for-each select='.'>
                            <xsl:break/>
                        </xsl:for-each>
                    </xsl:iterate>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var source = XDocument.Parse("<BOOKLIST><BOOKS><ITEM><TITLE>A</TITLE></ITEM></BOOKS></BOOKLIST>");
        var compiler = new XsltCompiler();
        var executable = compiler.Compile(xsl);
        var ex = Assert.Throws<InvalidOperationException>(() => executable.TransformToString(new XDocumentNode(source)));
        Assert.StartsWith("XTSE3120", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Break_Not_Last_Inside_If_Still_Throws()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:template match='/'>
                <out>
                    <xsl:iterate select='//ITEM/TITLE'>
                        <xsl:if test='position() eq 1'>
                            <xsl:break/>
                            <xsl:value-of select='.'/>
                        </xsl:if>
                    </xsl:iterate>
                </out>
            </xsl:template>
        </xsl:stylesheet>";

        var source = XDocument.Parse("<BOOKLIST><BOOKS><ITEM><TITLE>A</TITLE></ITEM></BOOKS></BOOKLIST>");
        var compiler = new XsltCompiler();
        var executable = compiler.Compile(xsl);
        var ex = Assert.Throws<InvalidOperationException>(() => executable.TransformToString(new XDocumentNode(source)));
        Assert.StartsWith("XTSE3120", ex.Message, StringComparison.Ordinal);
    }
}
