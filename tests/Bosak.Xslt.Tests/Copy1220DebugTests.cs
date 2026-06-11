// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 juni 2026
// PURPOSE              : Unit tests for copy-1220 namespace copying behavior.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-06-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

public class Copy1220DebugTests
{
    [Fact]
    public void Copy1220_NamespacesAreCopied()
    {
        var xsl = @"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='2.0'>
    <xsl:template match='/'>
        <xsl:variable name='fragment'>
            <wrapper xmlns:w='http://www.w.com/'>
                <a xmlns:a='http://www.a.com/' a:att='A'>
                    <aa xmlns='http://www.aa.com/'/>
                </a>
            </wrapper>
        </xsl:variable>
        <out>
            <xsl:copy-of select='$fragment/wrapper/child::node()'/>
        </out>
    </xsl:template>
</xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var source = new XDocumentNode(new XDocument(new XElement("dummy")));
        var result = executable.TransformToString(source);
        
        Assert.Contains("xmlns:a", result);
    }
    
    [Fact]
    public void Copy1220_NamespaceAxisAccessible()
    {
        var xsl = @"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' 
                xmlns:xs='http://www.w3.org/2001/XMLSchema'
                xmlns:f='f'
                version='3.0'
                expand-text='yes'
                exclude-result-prefixes='#all'>
    <xsl:template name='xsl:initial-template'>
        <xsl:variable name='fragment'>
            <wrapper xmlns:w='http://www.w.com/'>
                <a xmlns:a='http://www.a.com/' a:att='A'>
                    <aa xmlns='http://www.aa.com/'/>
                </a>
                <xsl:text>sandwich</xsl:text>
                <a xmlns='http://www.a.com/'>
                    <aa xmlns:aa='http://www.aa.com/'/>
                </a>
            </wrapper>
        </xsl:variable>
        <xsl:variable name='result'>
            <doc xmlns='http://www.out.com/'>
                <xsl:copy-of select='$fragment/wrapper/child::node()'/>
            </doc>
        </xsl:variable>
        <out>
            <m>{f:namespaces($result/*/*[1])}</m>
            <n>{f:namespaces($result/*/*[1]/*[1])}</n>
            <o>{f:namespaces($result/*/*[2])}</o>
            <p>{f:namespaces($result/*/*[2]/*[1])}</p>
        </out>
    </xsl:template>
    <xsl:function name='f:namespaces' as='xs:string'>
        <xsl:param name='node' as='element()'/>
        <xsl:sequence select='name($node) || "" : "" || string-join(sort($node/namespace::*[name() ne ""xml""]/(name()||""=""||.||""; "")))'/>
    </xsl:function>
</xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var source = new XDocumentNode(new XDocument(new XElement("dummy")));
        var result = executable.TransformToString(source, initialTemplate: "xsl:initial-template");
        
        // copy-1220 with copy-namespaces=yes
        Assert.Contains("<m>a : a=http://www.a.com/; w=http://www.w.com/; </m>", result);
        Assert.Contains("<n>aa : =http://www.aa.com/; a=http://www.a.com/; w=http://www.w.com/; </n>", result);
        Assert.Contains("<o>a : =http://www.a.com/; w=http://www.w.com/; </o>", result);
        Assert.Contains("<p>aa : =http://www.a.com/; aa=http://www.aa.com/; w=http://www.w.com/; </p>", result);
    }
}
