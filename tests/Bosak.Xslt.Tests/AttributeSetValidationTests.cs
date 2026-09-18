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
}
