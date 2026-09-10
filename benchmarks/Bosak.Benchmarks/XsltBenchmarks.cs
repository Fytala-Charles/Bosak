// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 September 2026
// PURPOSE              : Benchmarks for XSLT 3.0 stylesheet compilation and transformation.
// SPECIAL NOTES        : Benchmark harness for the Bosak XSLT 3.0 implementation; not shipped.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using BenchmarkDotNet.Attributes;
using Bosak.XPath.Core.Xdm;
using Bosak.Xslt.Api;

namespace Bosak.Benchmarks;

/// <summary>
/// Benchmarks for the XSLT 3.0 front end: stylesheet compilation and a transformation
/// that renders the shared synthetic catalog as an HTML table (for-each, sort,
/// format-number).
/// </summary>
[MemoryDiagnoser]
public class XsltBenchmarks
{
    /// <summary>A stylesheet that renders the catalog items with price >= 100 as an HTML table.</summary>
    public const string CatalogStylesheet = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:output method="html" indent="no"/>
          <xsl:template match="/">
            <html><body><table>
              <xsl:for-each select="/catalog/item[@price >= 100]">
                <xsl:sort select="number(@price)" data-type="number" order="descending"/>
                <tr>
                  <td><xsl:value-of select="@id"/></td>
                  <td><xsl:value-of select="@name"/></td>
                  <td><xsl:value-of select="format-number(number(@price), '#,##0.00')"/></td>
                  <td><xsl:value-of select="@category"/></td>
                  <td><xsl:value-of select="rating"/></td>
                </tr>
              </xsl:for-each>
            </table></body></html>
          </xsl:template>
        </xsl:stylesheet>
        """;

    private IXdmNode _doc = null!;
    private XsltExecutable _stylesheet = null!;

    /// <summary>Generates the catalog document and compiles the stylesheet once.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _doc = CatalogData.CreateDocument();
        _stylesheet = new XsltCompiler().Compile(CatalogStylesheet);

        if (TransformCatalog().Length == 0)
            throw new InvalidOperationException("XSLT benchmark produced an empty result.");
    }

    /// <summary>Compiles the HTML-table stylesheet into an executable.</summary>
    /// <returns>The compiled stylesheet.</returns>
    [Benchmark]
    public XsltExecutable Compile_Stylesheet() => new XsltCompiler().Compile(CatalogStylesheet);

    /// <summary>Transforms the 2,000-item catalog into an HTML table and serializes it.</summary>
    /// <returns>The length of the serialized HTML output.</returns>
    [Benchmark]
    public int Transform_HtmlTable() => TransformCatalog().Length;

    private string TransformCatalog() => _stylesheet.TransformToString(_doc);
}
