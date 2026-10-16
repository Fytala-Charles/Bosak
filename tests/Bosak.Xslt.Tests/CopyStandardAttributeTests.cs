// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies supported standard XSLT attributes on copy instructions.
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

using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies supported standard XSLT attributes on copy instructions.</summary>
public class CopyStandardAttributeTests
{
    private static XdmValue Run(string copy, bool function = false, string declarations = "") => new Api.XsltCompiler().Compile(
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:functions' xmlns:e='urn:extension' version='4.0'>" + declarations +
        "<xsl:variable name='seed' as='element()'><seed><child/></seed></xsl:variable>" +
        (function ? "<xsl:function name='f:probe' as='item()*'>" + copy + "</xsl:function>" : "") +
        "<xsl:template name='start'>" + (function ? "<xsl:sequence select='f:probe()'/>" : copy) + "</xsl:template></xsl:stylesheet>")
        .Transform(null, initialTemplate: "start", rawResult: true);

    /// <summary>Accepts supported standard attributes alongside copy-specific attributes.</summary>
    /// <param name="attribute">The standard attribute.</param>
    /// <param name="function">Whether copying occurs in a stylesheet function.</param>
    [Theory]
    [InlineData("version='3.0'", false)]
    [InlineData("version='3.0'", true)]
    [InlineData("expand-text='yes'", false)]
    [InlineData("expand-text='yes'", true)]
    [InlineData("default-mode='#unnamed'", false)]
    [InlineData("default-mode='#unnamed'", true)]
    [InlineData("default-collation='http://www.w3.org/2005/xpath-functions/collation/codepoint'", false)]
    [InlineData("default-collation='http://www.w3.org/2005/xpath-functions/collation/codepoint'", true)]
    [InlineData("xpath-default-namespace='urn:data'", false)]
    [InlineData("xpath-default-namespace='urn:data'", true)]
    [InlineData("exclude-result-prefixes='f'", false)]
    [InlineData("exclude-result-prefixes='f'", true)]
    [InlineData("extension-element-prefixes='e'", false)]
    [InlineData("extension-element-prefixes='e'", true)]
    [InlineData("default-validation='strip'", false)]
    [InlineData("default-validation='strip'", true)]
    public void SupportedAttribute(string attribute, bool function) => Assert.Equal(7, Run("<xsl:copy select='7' " + attribute + "/>", function).IntegerValue);

    /// <summary>Expands copied literal text according to the local standard attribute.</summary>
    /// <param name="function">Whether copying occurs in a stylesheet function.</param>
    /// <param name="expand">Whether text expansion is enabled.</param>
    /// <param name="expected">The expected text.</param>
    [Theory]
    [InlineData(false, "yes", "value 2")]
    [InlineData(true, "yes", "value 2")]
    [InlineData(false, "no", "value {1+1}")]
    [InlineData(true, "no", "value {1+1}")]
    public void ExpandText(bool function, string expand, string expected) => Assert.Equal(expected,
        Run("<xsl:copy select='$seed' expand-text='" + expand + "'>value {1+1}</xsl:copy>", function).NodeValue.StringValue);

    /// <summary>Applies a local XPath default namespace while evaluating copy content.</summary>
    /// <param name="function">Whether copying occurs in a stylesheet function.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void XPathNamespace(bool function) => Assert.Equal("1", Run("<xsl:copy select='$namespaced' xpath-default-namespace='urn:data'><xsl:value-of select='count(child)'/></xsl:copy>", function,
        "<xsl:variable name='namespaced' as='element()'><seed xmlns='urn:data'><child/></seed></xsl:variable>").NodeValue.StringValue);

    /// <summary>Uses the local default mode for ordinary nested template applications.</summary>
    [Fact]
    public void DefaultMode() => Assert.Equal("mode", Run("<xsl:copy select='$seed' default-mode='m'><xsl:apply-templates select='child'/></xsl:copy>", declarations:
        "<xsl:mode name='m'/><xsl:template match='child' mode='m'>mode</xsl:template>").NodeValue.StringValue);

    /// <summary>Rejects unknown or module-only attributes and invalid real copy options.</summary>
    /// <param name="attributes">The invalid attributes.</param>
    /// <param name="error">The required static error.</param>
    [Theory]
    [InlineData("bogus='bad'", "XTSE0090")]
    [InlineData("input-type-annotations='strip'", "XTSE0090")]
    [InlineData("expand-text='yes' bogus='bad'", "XTSE0090")]
    [InlineData("expand-text='yes' copy-namespaces='bad'", "XTSE0020")]
    public void InvalidAttributes(string attributes, string error) => Assert.Contains(error, Assert.ThrowsAny<Exception>(() => Run("<xsl:copy select='7' " + attributes + "/>")).ToString());

    /// <summary>Applies the local standard collation in copy content.</summary>
    /// <param name="function">Whether copying occurs in a stylesheet function.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LocalCollation(bool function) => Assert.Equal("0", Run("<xsl:copy select='$seed' default-collation='http://www.w3.org/2005/xpath-functions/collation/html-ascii-case-insensitive'><xsl:value-of select='compare(&quot;A&quot;,&quot;a&quot;)'/></xsl:copy>", function).NodeValue.StringValue);

    /// <summary>Validates supported and module-only attributes in unused functions.</summary>
    /// <param name="attribute">The attribute on the unused copy.</param>
    /// <param name="valid">Whether the attribute is allowed.</param>
    [Theory]
    [InlineData("expand-text='yes'", true)]
    [InlineData("input-type-annotations='strip'", false)]
    public void UnusedFunction(string attribute, bool valid)
    {
        var source = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:functions' version='4.0'><xsl:function name='f:unused'><xsl:copy select='7' " + attribute + "/></xsl:function><xsl:template name='start'/></xsl:stylesheet>";
        if (valid) Assert.NotNull(new Api.XsltCompiler().Compile(source));
        else Assert.Contains("XTSE0090", Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(source)).ToString());
    }
}
