// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 juni 2026
// PURPOSE              : Unit tests verifying xsl:copy document-node behavior and on-empty handling.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-06-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 11-07-2026     | Updated expected XML empty-element tag to no-space form.                               |
//                      | Charles Korthout | 0.3   | 12-07-2026     | Expect the default XML declaration in the serialized result.                            |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

public class Copy4301Tests
{
    [Fact]
    public void Copy4301_DocumentNodeInCopyOf()
    {
        var xsl = @"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='2.0'>
    <xsl:template match='/'>
        <root>
            <xsl:variable name='a-doc'>
                <a-root/>
            </xsl:variable>
            <xsl:copy-of select='$a-doc'/>
            <xsl:apply-templates select='$a-doc' mode='test'/>
        </root>
    </xsl:template>
    <xsl:template match='/ | node()' mode='test'>
        <xsl:copy/>
    </xsl:template>
</xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var source = new XDocumentNode(new XDocument(new XElement("dummy")));
        var result = executable.TransformToString(source);
        Assert.Contains("<root><a-root/></root>", result);
        Assert.Contains("<?xml", result);
    }
}
