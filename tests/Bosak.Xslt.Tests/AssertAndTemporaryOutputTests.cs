// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 3 augustus 2026
// PURPOSE              : Unit tests for xsl:assert evaluation and XTDE1480 (result-document in temporary output state).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-08-2026     | Creation (assert-001..010, result-document-1131/1139/1140/1142/1144 coverage)          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

public class AssertAndTemporaryOutputTests
{
    private static string Run(string xsl)
    {
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var source = new XDocumentNode(new XDocument(new XElement("dummy")));
        return executable.TransformToString(source);
    }

    [Fact]
    public void Assert_PassingTest_ProducesOutput()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
    <xsl:template match='/'>
        <out><xsl:assert test='1 eq 1'>ignored</xsl:assert></out>
    </xsl:template>
</xsl:stylesheet>");
        Assert.Contains("<out/>", result);
    }

    [Fact]
    public void Assert_FailingTest_RaisesDefaultErrorCode()
    {
        var ex = Assert.Throws<Bosak.XPath.Runtime.Vm.XPathErrorException>(() => Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
    <xsl:template match='/'>
        <out><xsl:assert test='1 eq 2'>boom</xsl:assert></out>
    </xsl:template>
</xsl:stylesheet>"));
        Assert.Equal("XTMM9001", ex.CodeLocalName);
    }

    [Fact]
    public void Assert_FailingTest_UsesCustomErrorCode()
    {
        var ex = Assert.Throws<Bosak.XPath.Runtime.Vm.XPathErrorException>(() => Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'
    xmlns:my='http://example.com/my'>
    <xsl:template match='/'>
        <out><xsl:assert test='false()' error-code='my:ABCD9999'>boom</xsl:assert></out>
    </xsl:template>
</xsl:stylesheet>"));
        Assert.Equal("ABCD9999", ex.CodeLocalName);
        Assert.Equal("http://example.com/my", ex.CodeNamespaceUri);
    }

    [Fact]
    public void Assert_FailingTest_CaughtByTryCatch()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
    <xsl:template match='/'>
        <out>
            <xsl:try>
                <xsl:text>A</xsl:text>
                <xsl:assert test='false()'>boom</xsl:assert>
                <xsl:catch errors='*:XTMM9001'>B</xsl:catch>
            </xsl:try>
            <xsl:text>C</xsl:text>
        </out>
    </xsl:template>
</xsl:stylesheet>");
        // An error in xsl:try rolls back the try block's partial output (assert-010).
        Assert.Contains("<out>BC</out>", result);
    }

    [Fact]
    public void ResultDocument_InVariableContent_RaisesXTDE1480()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
    <xsl:template match='/'>
        <xsl:variable name='v'>
            <xsl:result-document href='out.xml'><boo/></xsl:result-document>
        </xsl:variable>
        <xsl:sequence select='$v'/>
    </xsl:template>
</xsl:stylesheet>"));
        Assert.Contains("XTDE1480", ex.Message);
    }

    [Fact]
    public void ResultDocument_InFunctionBody_RaisesXTDE1480()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
    <xsl:template match='/'>
        <xsl:sequence select='f:l()' xmlns:f='http://example.com/f'/>
    </xsl:template>
    <xsl:function name='f:l' xmlns:f='http://example.com/f'>
        <xsl:result-document href='out.xml'><boo/></xsl:result-document>
    </xsl:function>
</xsl:stylesheet>"));
        Assert.Contains("XTDE1480", ex.Message);
    }

    [Fact]
    public void ResultDocument_InTemplateBody_IsAllowed()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
    <xsl:template match='/'>
        <xsl:result-document href='secondary.xml'><boo/></xsl:result-document>
        <out/>
    </xsl:template>
</xsl:stylesheet>");
        Assert.Contains("<out/>", result);
    }

    [Fact]
    public void WherePopulated_DiscardsEmptyItemsIndividually()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
    <xsl:template match='/'>
        <Results>
            <xsl:where-populated>
                <first>one</first>
                <second one='empty'/>
                <third>three</third>
            </xsl:where-populated>
        </Results>
    </xsl:template>
</xsl:stylesheet>");
        Assert.Contains("<first>one</first>", result);
        Assert.DoesNotContain("<second", result);
        Assert.Contains("<third>three</third>", result);
    }

    [Fact]
    public void WherePopulated_KeepsAttributeWithValue_DiscardsZeroLength()
    {
        var result = Run(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
    <xsl:template match='/'>
        <Results>
            <xsl:where-populated>
                <xsl:attribute name='x' select='17'/>
                <xsl:attribute name='y' select=""''""/>
                <inner/>
            </xsl:where-populated>
        </Results>
    </xsl:template>
</xsl:stylesheet>");
        Assert.Contains("<Results x=\"17\"/>", result);
    }
}
