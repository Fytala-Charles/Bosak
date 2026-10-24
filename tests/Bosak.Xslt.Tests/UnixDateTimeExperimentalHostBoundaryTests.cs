// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 24 October 2026
// PURPOSE              : Verify XSLT hosts preserve the frozen negative Unix epoch boundary.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 24-10-2026     | Creation: Legacy and Draft hosts retain frozen negative-epoch refusal.  |
//                      | Charles Korthout | 0.2   | 24-10-2026     | Verify valid wide positive epochs raise calendar errors in both XSLT hosts.               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>XSLT version 4.0 and Draft host selection do not imply Experimental signed epochs.</summary>
public sealed class UnixDateTimeExperimentalHostBoundaryTests
{
    private static string Sheet(string value) =>
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='4.0'>" +
        "<xsl:output method='text'/><xsl:template match='/'>" +
        "<xsl:value-of select=\"unix-dateTime(" + value + ")\"/>" +
        "</xsl:template></xsl:stylesheet>";

    /// <summary>Both supported XSLT hosts retain frozen negative-epoch refusal and positive behavior.</summary>
    /// <param name="profile">The captured XSLT host profile.</param>
    [Theory, InlineData(XsltProcessorProfile.Legacy), InlineData(XsltProcessorProfile.Xslt40Draft)]
    public void HostRetainsFrozenNegativeEpochContract(XsltProcessorProfile profile)
    {
        var compiler = new XsltCompiler { ProcessorProfile = profile };
        var input = new XDocumentNode(XDocument.Parse("<root/>"));
        Assert.Equal("1970-01-01T00:00:00Z", compiler.Compile(XDocument.Parse(Sheet("0"))).TransformToString(input));
        var failure = Assert.ThrowsAny<Exception>(() => compiler.Compile(XDocument.Parse(Sheet("-1"))).TransformToString(input));
        Assert.Contains("FOCA0002", failure.Message);
    }
    /// <summary>Valid wide positive integers reach the supported calendar boundary in both XSLT hosts.</summary>
    /// <param name="profile">The captured XSLT host profile.</param>
    [Theory, InlineData(XsltProcessorProfile.Legacy), InlineData(XsltProcessorProfile.Xslt40Draft)]
    public void HostReportsWideIntegerCalendarOverflow(XsltProcessorProfile profile)
    {
        var compiler = new XsltCompiler { ProcessorProfile = profile };
        var input = new XDocumentNode(XDocument.Parse("<root/>"));
        var failure = Assert.ThrowsAny<Exception>(() => compiler.Compile(XDocument.Parse(Sheet("9223372036854775808"))).TransformToString(input));
        Assert.Contains("FODT0001", failure.Message);
    }
}
