// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 september 2026
// PURPOSE              : Unit tests for record replay over streamed sources (xsl:fork over a streamed principal input).
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
using System.Text;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>
/// Tests for xsl:fork over a burst-mode streamed principal source: the engine must opt
/// the not-yet-started stream into record replay so every fork prong sees the full
/// record stream (si-fork-808/816).
/// </summary>
[Collection("MemorySensitive")]
public class StreamingForkReplayTests
{
    private const string Xml = """
        <transactions>
        <transaction value="-13.24"/>
        <transaction value="8.12"/>
        <transaction value="-15.00"/>
        <transaction value="6.00"/>
        </transactions>
        """;

    private static string Streamed(string xsl, string xml)
    {
        var doc = XmlStreamingProvider.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(xml)),
            new StreamingLoadOptions { BaseUri = "http://example.org/in.xml" });
        return new XsltCompiler().Compile(xsl).Transform(doc).NodeValue!.ToXmlString();
    }

    [Fact]
    public void Fork_BothProngsSeeAllRecords()
    {
        var xsl = """
            <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="xml" indent="no"/>
              <xsl:mode streamable="yes"/>
              <xsl:template match="transactions">
                <out>
                  <xsl:fork>
                    <xsl:sequence>
                      <neg><xsl:value-of select="count(transaction[number(@value) lt 0])"/></neg>
                    </xsl:sequence>
                    <xsl:sequence>
                      <pos><xsl:value-of select="count(transaction[number(@value) gt 0])"/></pos>
                    </xsl:sequence>
                  </xsl:fork>
                </out>
              </xsl:template>
            </xsl:stylesheet>
            """;

        var result = Streamed(xsl, Xml);
        Assert.Contains("<neg>2</neg>", result);
        Assert.Contains("<pos>2</pos>", result);
    }
}
