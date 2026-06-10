using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

public class Copy4308Tests
{
    [Fact]
    public void Copy4308_ShouldThrow_WhenNoContextItem()
    {
        var xsl = @"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template name='xsl:initial-template'>
    <out>
      <xsl:call-template name='temp'/>
      <xsl:copy-of select='$var'/>
    </out>
  </xsl:template>
  <xsl:template name='temp'>
    <xsl:param name='b' select='false()'/>
    <xsl:if test='$b'>
      <xsl:copy>
        <in/>
      </xsl:copy>
    </xsl:if>
  </xsl:template>
  <xsl:variable name='var'>
    <xsl:call-template name='temp'>
      <xsl:with-param name='b' select='true()'/>
    </xsl:call-template>
  </xsl:variable>
</xsl:stylesheet>";

        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var source = new XDocumentNode(new XDocument(new XElement("a")));
        var ex = Assert.Throws<System.InvalidOperationException>(() =>
            executable.TransformToString(source));
        Assert.Contains("XTTE0945", ex.Message);
    }
}
