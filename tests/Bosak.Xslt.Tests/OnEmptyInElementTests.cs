// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 september 2026
// PURPOSE              : Unit tests for xsl:on-empty / xsl:on-non-empty inside xsl:element content.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 17-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

public class OnEmptyInElementTests
{
    private static string Transform(string xsl, string? sourceXml = null)
    {
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        if (sourceXml == null)
            return executable.TransformToString(null, initialTemplate: "main");
        return executable.TransformToString(new XDocumentNode(XDocument.Parse(sourceXml)));
    }

    private const string FallbackXsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
  <xsl:template match='/'>
    <out>
      <xsl:element name='a'>
        <xsl:sequence select='/BOOKLIST/BOOKS/ITEM/PRICEDATA'/>
        <xsl:on-empty>There is no price data</xsl:on-empty>
      </xsl:element>
    </out>
  </xsl:template>
</xsl:stylesheet>";

    [Fact]
    public void OnEmpty_InXslElement_EmptySequence_FiresFallback()
    {
        var source = "<BOOKLIST><BOOKS><ITEM/></BOOKS></BOOKLIST>";
        var result = Transform(FallbackXsl, source);
        Assert.Contains("<a>There is no price data</a>", result);
    }

    [Fact]
    public void OnEmpty_InXslElement_NonEmptySequence_DoesNotFireFallback()
    {
        var source = "<BOOKLIST><BOOKS><ITEM><PRICEDATA>9.99</PRICEDATA></ITEM></BOOKS></BOOKLIST>";
        var result = Transform(FallbackXsl, source);
        Assert.Contains("<a><PRICEDATA>9.99</PRICEDATA></a>", result);
        Assert.DoesNotContain("There is no price data", result);
    }

    [Fact]
    public void OnNonEmpty_InXslElement_EmptySequence_DoesNotFireFallback()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
  <xsl:template match='/'>
    <out>
      <xsl:element name='a'>
        <xsl:sequence select='/BOOKLIST/BOOKS/ITEM/PRICEDATA'/>
        <xsl:on-non-empty>HAS-DATA</xsl:on-non-empty>
      </xsl:element>
    </out>
  </xsl:template>
</xsl:stylesheet>";

        var source = "<BOOKLIST><BOOKS><ITEM/></BOOKS></BOOKLIST>";
        var result = Transform(xsl, source);
        Assert.Contains("<a/>", result);
    }

    [Fact]
    public void OnNonEmpty_InXslElement_NonEmptySequence_FiresFallback()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
  <xsl:template match='/'>
    <out>
      <xsl:element name='a'>
        <xsl:sequence select='/BOOKLIST/BOOKS/ITEM/PRICEDATA'/>
        <xsl:on-non-empty>HAS-DATA</xsl:on-non-empty>
      </xsl:element>
    </out>
  </xsl:template>
</xsl:stylesheet>";

        var source = "<BOOKLIST><BOOKS><ITEM><PRICEDATA>9.99</PRICEDATA></ITEM></BOOKS></BOOKLIST>";
        var result = Transform(xsl, source);
        Assert.Contains("HAS-DATA", result);
    }
}
