// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 september 2026
// PURPOSE              : Unit tests for xsl:copy / xsl:copy-of copy-namespaces="no" behavior.
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

public class CopyNamespacesTests
{
    private const string CityGmlLikeSource = @"<CityModel xmlns='http://www.opengis.net/citygml/1.0'
    xmlns:bldg='http://www.opengis.net/citygml/building/1.0'
    xmlns:gml='http://www.opengis.net/gml'>
  <gml:description>Text</gml:description>
</CityModel>";

    private static string Transform(string xsl, string sourceXml)
    {
        var compiler = new Api.XsltCompiler();
        var executable = compiler.Compile(xsl, "file:///test.xsl");
        var source = XDocument.Parse(sourceXml);
        return executable.TransformToString(new XDocumentNode(source));
    }

    [Fact]
    public void CopyOf_CopyNamespacesNo_KeepsOnlyRequiredNamespaces()
    {
        var xsl = @"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'
                xmlns:gml='http://www.opengis.net/gml'>
  <xsl:template match='/'>
    <out>
      <xsl:copy-of select='/*/gml:description' copy-namespaces='no'/>
    </out>
  </xsl:template>
</xsl:stylesheet>";

        var result = Transform(xsl, CityGmlLikeSource);

        Assert.Contains("xmlns:gml=\"http://www.opengis.net/gml\"", result);
        Assert.DoesNotContain("citygml/1.0", result);
        Assert.DoesNotContain("building/1.0", result);
    }

    [Fact]
    public void CopyOf_CopyNamespacesYes_CopiesAllInScopeNamespaces()
    {
        var xsl = @"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'
                xmlns:gml='http://www.opengis.net/gml'>
  <xsl:template match='/'>
    <out>
      <xsl:copy-of select='/*/gml:description' copy-namespaces='yes'/>
    </out>
  </xsl:template>
</xsl:stylesheet>";

        var result = Transform(xsl, CityGmlLikeSource);

        Assert.Contains("xmlns:gml=\"http://www.opengis.net/gml\"", result);
        Assert.Contains("http://www.opengis.net/citygml/1.0", result);
        Assert.Contains("http://www.opengis.net/citygml/building/1.0", result);
    }

    [Fact]
    public void CopyOf_CopyNamespacesNo_DefaultNamespaceNotCopied()
    {
        // The default-namespace declaration (xmlns="...") has no namespace URI in the
        // LINQ-to-XML attribute model; it must still be treated as a namespace
        // declaration and dropped (regression: si-copy-of-020).
        var xsl = @"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <out>
      <xsl:copy-of select='/*/*' copy-namespaces='no'/>
    </out>
  </xsl:template>
</xsl:stylesheet>";

        var result = Transform(xsl, CityGmlLikeSource);

        // The copied CityModel element itself is in the default namespace, so a default
        // declaration is required; but no prefixed copies of the unused bldg binding.
        Assert.DoesNotContain("bldg", result);
    }

    [Fact]
    public void Copy_CopyNamespacesNo_KeepsOnlyRequiredNamespaces()
    {
        var xsl = @"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'
                xmlns:gml='http://www.opengis.net/gml'>
  <xsl:template match='/'>
    <out>
      <xsl:for-each select='/*/gml:description'>
        <xsl:copy copy-namespaces='no'>
          <xsl:value-of select='.'/>
        </xsl:copy>
      </xsl:for-each>
    </out>
  </xsl:template>
</xsl:stylesheet>";

        var result = Transform(xsl, CityGmlLikeSource);

        Assert.Contains("xmlns:gml=\"http://www.opengis.net/gml\"", result);
        Assert.DoesNotContain("citygml/1.0", result);
        Assert.DoesNotContain("building/1.0", result);
    }

    [Fact]
    public void Copy_CopyNamespacesYes_CopiesAllInScopeNamespaces()
    {
        var xsl = @"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'
                xmlns:gml='http://www.opengis.net/gml'>
  <xsl:template match='/'>
    <out>
      <xsl:for-each select='/*/gml:description'>
        <xsl:copy copy-namespaces='yes'>
          <xsl:value-of select='.'/>
        </xsl:copy>
      </xsl:for-each>
    </out>
  </xsl:template>
</xsl:stylesheet>";

        var result = Transform(xsl, CityGmlLikeSource);

        Assert.Contains("xmlns:gml=\"http://www.opengis.net/gml\"", result);
        Assert.Contains("http://www.opengis.net/citygml/1.0", result);
        Assert.Contains("http://www.opengis.net/citygml/building/1.0", result);
    }
}
