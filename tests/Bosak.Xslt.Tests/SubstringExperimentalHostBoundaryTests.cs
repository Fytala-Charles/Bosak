// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verifies XSLT hosts retain frozen substring argument behavior.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-10-2026     | Creation: Legacy/draft hosts preserve absent versus explicit-empty substring contracts.  |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies XSLT hosts retain frozen substring argument behavior.</summary>
public class SubstringExperimentalHostBoundaryTests
{
    private static string Sheet(string pattern) =>
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='4.0'>" +
        "<xsl:output method='text'/><xsl:template match='/'>" +
        "<xsl:value-of separator=',' select=\"index-of-substring('abc', " + pattern + ")\"/>" +
        "</xsl:template></xsl:stylesheet>";

    /// <summary>Does not implicitly opt an XSLT 4.0 stylesheet into XPath Experimental semantics.</summary>
    /// <param name="profile">The XSLT host processor profile.</param>
    [Theory]
    [InlineData(XsltProcessorProfile.Legacy)]
    [InlineData(XsltProcessorProfile.Xslt40Draft)]
    public void HostRetainsFrozenAbsentSubstringContract(XsltProcessorProfile profile)
    {
        var compiler = new XsltCompiler { ProcessorProfile = profile };
        var input = new XDocumentNode(XDocument.Parse("<root/>"));
        Assert.Equal("1,2,3,4", compiler.Compile(Sheet("''")).TransformToString(input));
        var error = Assert.ThrowsAny<Exception>(() => compiler.Compile(Sheet("()")).TransformToString(input));
        Assert.Contains("XPTY0004", error.ToString());
    }
}
