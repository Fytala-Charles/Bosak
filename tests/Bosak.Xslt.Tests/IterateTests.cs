// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 26 juni 2026
// PURPOSE              : Unit tests for xsl:iterate support in the result-tree path.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 26-06-2026     | Creation                                                                                 |
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
}
