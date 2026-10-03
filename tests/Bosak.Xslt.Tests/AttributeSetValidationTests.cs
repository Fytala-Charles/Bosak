// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 september 2026
// PURPOSE              : Unit tests for XTSE0020 validation of xsl:attribute-set enumerated attributes.
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
//                      | Charles Korthout | 0.2   | 03-10-2026     | REQ-119: XTSE0730 — a streamable="yes" attribute-set may only reference attribute sets    |
//                      |                  |       |                | that also specify streamable="yes" (error-0730a)                                         |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Xunit;

namespace Bosak.Xslt.Tests;

public class AttributeSetValidationTests
{
    [Fact]
    public void AttributeSet_StreamableYesUpperCase_ThrowsXtse0020()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:attribute-set name='as-1' streamable='Yes'>
                <xsl:attribute name='x' select='1'/>
            </xsl:attribute-set>
            <xsl:template name='main'>
                <out><e xsl:use-attribute-sets='as-1'/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var ex = Assert.Throws<InvalidOperationException>(() => new Api.XsltCompiler().Compile(xsl));
        Assert.Contains("XTSE0020", ex.Message);
    }

    [Fact]
    public void AttributeSet_StreamableYesLowerCase_Compiles()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:attribute-set name='as-1' streamable='yes'>
                <xsl:attribute name='x' select='1'/>
            </xsl:attribute-set>
            <xsl:template name='main'>
                <out><e xsl:use-attribute-sets='as-1'/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var executable = new Api.XsltCompiler().Compile(xsl);
        var result = executable.TransformToString(null, initialTemplate: "main");
        Assert.Contains("x=\"1\"", result);
    }

    [Fact]
    public void AttributeSet_InvalidVisibility_ThrowsXtse0020()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:attribute-set name='as-1' visibility='Public'>
                <xsl:attribute name='x' select='1'/>
            </xsl:attribute-set>
            <xsl:template name='main'>
                <out/>
            </xsl:template>
        </xsl:stylesheet>";

        var ex = Assert.Throws<InvalidOperationException>(() => new Api.XsltCompiler().Compile(xsl));
        Assert.Contains("XTSE0020", ex.Message);
    }

    [Fact]
    public void AttributeSet_StreamableReferencesNonStreamable_ThrowsXtse0730()
    {
        // error-0730a: a streamable="yes" attribute-set references a set that does not
        // specify streamable="yes" (streamable="0" is a valid false boolean).
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:attribute-set name='a' streamable='yes' use-attribute-sets='b c'>
                <xsl:attribute name='x' select='1'/>
            </xsl:attribute-set>
            <xsl:attribute-set name='b' streamable='yes'>
                <xsl:attribute name='y' select='1'/>
            </xsl:attribute-set>
            <xsl:attribute-set name='c' streamable='0'>
                <xsl:attribute name='z' select='1'/>
            </xsl:attribute-set>
            <xsl:template name='main'>
                <out/>
            </xsl:template>
        </xsl:stylesheet>";

        var ex = Assert.Throws<InvalidOperationException>(() => new Api.XsltCompiler().Compile(xsl));
        Assert.Contains("XTSE0730", ex.Message);
    }

    [Fact]
    public void AttributeSet_StreamableReferencesStreamable_Compiles()
    {
        var xsl = @"<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
            <xsl:attribute-set name='a' streamable='yes' use-attribute-sets='b'>
                <xsl:attribute name='x' select='1'/>
            </xsl:attribute-set>
            <xsl:attribute-set name='b' streamable='yes'>
                <xsl:attribute name='y' select='2'/>
            </xsl:attribute-set>
            <xsl:template name='main'>
                <out><e xsl:use-attribute-sets='a'/></out>
            </xsl:template>
        </xsl:stylesheet>";

        var executable = new Api.XsltCompiler().Compile(xsl);
        var result = executable.TransformToString(null, initialTemplate: "main");
        Assert.Contains("x=\"1\"", result);
        Assert.Contains("y=\"2\"", result);
    }
}
