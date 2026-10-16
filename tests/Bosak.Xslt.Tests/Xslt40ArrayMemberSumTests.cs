// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies sums of complete array member values during XSLT iteration.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies sums of complete array member values during XSLT iteration.</summary>
public class Xslt40ArrayMemberSumTests
{
    /// <summary>Matches the unchanged upstream scenario including a multi-item array member.</summary>
    /// <param name="sum">The sum call.</param>
    [Theory]
    [InlineData("sum(.)")]
    [InlineData("sum(.,99)")]
    public void IterateMembers(string sum)
    {
        var stylesheet = """
            <xsl:stylesheet version='4.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
              xmlns:array='http://www.w3.org/2005/xpath-functions/array' exclude-result-prefixes='#all'>
              <xsl:template match='/'><out><xsl:for-each select='array:members([1,(5,6),10])'>
                <in><xsl:value-of select='SUM'/></in>
              </xsl:for-each></out></xsl:template>
            </xsl:stylesheet>
            """;
        var output = new Api.XsltCompiler().Compile(stylesheet.Replace("SUM",sum)).TransformToString(
            new XDocumentNode(XDocument.Parse("<root/>")));
        Assert.Equal(new[] { "1", "11", "10" }, XElement.Parse(output).Elements("in").Select(e => e.Value));
    }
}
