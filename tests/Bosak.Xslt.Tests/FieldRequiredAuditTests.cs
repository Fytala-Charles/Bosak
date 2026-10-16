// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies the current record field contract after required was retired.
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

using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies current record fields without reviving retired optional-field syntax.</summary>
public class FieldRequiredAuditTests
{
    private static string Sheet(string field, string expression = "7", string version = "4.0") =>
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:r='urn:record' version='" + version + "'>" +
        "<xsl:record-type name='r:probe' constructor='yes'>" + field + "</xsl:record-type>" +
        "<xsl:template name='start'><xsl:sequence select='" + expression + "'/></xsl:template></xsl:stylesheet>";

    private static XsltCompiler Compiler() => new() { ProcessorProfile = XsltProcessorProfile.Xslt40Draft };

    /// <summary>The retired unqualified attribute is rejected irrespective of its old boolean spelling.</summary>
    /// <param name="value">The retired attribute value.</param>
    [Theory]
    [InlineData("yes")]
    [InlineData("no")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("1")]
    [InlineData("0")]
    public void RetiredRequiredRejected(string value) => Assert.Contains("XTSE0090",
        Assert.ThrowsAny<Exception>(() => Compiler().Compile(Sheet("<xsl:field name='value' required='" + value + "'/>"))).ToString());

    /// <summary>Current optional values use empty sequences while the field remains present.</summary>
    [Fact]
    public void EmptySequenceValueHasPresentField() => Assert.True(Compiler().Compile(Sheet(
        "<xsl:field name='value' as='xs:string?' select='()'/>",
        "map:contains(r:probe(), &quot;value&quot;) and empty(r:probe()?value)".Replace("map:contains", "Q{http://www.w3.org/2005/xpath-functions/map}contains")))
        .Transform(null, initialTemplate: "start", rawResult: true).BooleanValue);

    /// <summary>Current static fields retain their initializer in generated constructors.</summary>
    [Fact]
    public void StaticFieldInitializer() => Assert.Equal(7, Compiler().Compile(Sheet(
        "<xsl:field name='value' as='xs:integer' static='yes' select='7'/>", "r:probe()?value"))
        .Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);

    /// <summary>Known static field options are validated.</summary>
    /// <param name="value">The invalid static option.</param>
    [Theory]
    [InlineData("bad")]
    [InlineData("YES")]
    public void InvalidStaticRejected(string value) => Assert.Contains("XTSE0020",
        Assert.ThrowsAny<Exception>(() => Compiler().Compile(Sheet("<xsl:field name='value' static='" + value + "' select='7'/>"))).ToString());

    /// <summary>Static fields require an initializer.</summary>
    [Fact]
    public void StaticInitializerRequired() => Assert.Contains("XTSE0010",
        Assert.ThrowsAny<Exception>(() => Compiler().Compile(Sheet("<xsl:field name='value' static='yes'/>"))).ToString());

    /// <summary>Foreign metadata does not revive retired XSLT options.</summary>
    [Fact]
    public void ForeignRequiredMetadata() => Assert.Equal(7, Compiler().Compile(Sheet(
        "<xsl:field xmlns:e='urn:metadata' name='value' e:required='no' select='7'/>", "r:probe()?value"))
        .Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);

    /// <summary>Future-version suppression still ignores attributes unknown to this processor.</summary>
    [Fact]
    public void FutureRequiredIgnored() => Assert.Equal(7, Compiler().Compile(Sheet(
        "<xsl:field name='value' required='no' select='7'/>", "r:probe()?value", "17.0"))
        .Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);
}
