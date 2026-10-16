// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies copy-select focus and current-template-rule state restoration.
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

/// <summary>Verifies copy-select focus and current-template-rule state restoration.</summary>
public class CopySelectStateTests
{
    private static XdmValue Run(string body, bool function = false, string declarations = "") => new Api.XsltCompiler().Compile(
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:functions' version='4.0'>" + declarations +
        "<xsl:variable name='seed' as='element()'><seed><a/><b/></seed></xsl:variable>" +
        (function ? "<xsl:function name='f:probe' as='item()*'>" + body + "</xsl:function>" : "") +
        "<xsl:template name='start'><out>" + (function ? "<xsl:sequence select='f:probe()'/>" : body) + "</out></xsl:template></xsl:stylesheet>")
        .Transform(null, initialTemplate: "start", rawResult: true);
    private const string Focus = "concat(name(), &quot;:&quot;, position(), &quot;:&quot;, last())";

    /// <summary>Uses singleton focus inside selected copying and restores all three components afterward.</summary>
    /// <param name="function">Whether the collecting dispatcher is used.</param>
    /// <param name="select">The node-selection expression.</param>
    [Theory]
    [InlineData(false, "$seed")]
    [InlineData(true, "$seed")]
    [InlineData(false, "$seed/self::*")]
    [InlineData(true, "$seed/self::*")]
    public void SuccessFocus(bool function, string select)
    {
        var value = Run("<xsl:for-each select='$seed/*'><xsl:copy select='" + select + "'><xsl:value-of select='" + Focus + "'/></xsl:copy><xsl:text>|</xsl:text><xsl:value-of select='" + Focus + "'/></xsl:for-each>", function);
        Assert.Equal("seed:1:1|a:1:2seed:1:1|b:2:2", value.NodeValue.StringValue);
    }

    /// <summary>Restores caller focus after construction, selection and cardinality errors.</summary>
    /// <param name="function">Whether the collecting dispatcher is used.</param>
    /// <param name="select">The selection expression.</param>
    /// <param name="content">The copied content.</param>
    [Theory]
    [InlineData(false, "$seed", "<xsl:sequence select='error()'/>")]
    [InlineData(true, "$seed", "<xsl:sequence select='error()'/>")]
    [InlineData(false, "error()", "")]
    [InlineData(true, "error()", "")]
    [InlineData(false, "$seed/*", "")]
    [InlineData(true, "$seed/*", "")]
    public void ErrorFocus(bool function, string select, string content)
    {
        var value = Run("<xsl:for-each select='$seed/*'><xsl:try><xsl:copy select='" + select + "'>" + content + "</xsl:copy><xsl:catch><xsl:value-of select='" + Focus + "'/></xsl:catch></xsl:try><xsl:text>|</xsl:text><xsl:value-of select='" + Focus + "'/></xsl:for-each>", function);
        Assert.Equal("a:1:2|a:1:2b:2:2|b:2:2", value.NodeValue.StringValue);
    }

    /// <summary>Leaves the caller focus unchanged when select is absent.</summary>
    /// <param name="function">Whether the collecting dispatcher is used.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ContextCopy(bool function) => Assert.Equal("a:1:2b:2:2", Run("<xsl:for-each select='$seed/*'><xsl:copy><xsl:value-of select='" + Focus + "'/></xsl:copy></xsl:for-each>", function).NodeValue.StringValue);

    /// <summary>Restores the current template rule after a caught selected-copy error.</summary>
    /// <param name="select">The expression that raises or selects the copied node.</param>
    /// <param name="content">The copied content.</param>
    [Theory]
    [InlineData("$seed", "<xsl:sequence select='error()'/>")]
    [InlineData("error()", "")]
    [InlineData("$seed/*", "")]
    public void TemplateRuleAfterError(string select, string content)
    {
        var value = Run("<xsl:apply-templates select='$seed/a'/>", declarations:
            "<xsl:template match='a' priority='2'><xsl:try><xsl:copy select='" + select + "'>" + content + "</xsl:copy><xsl:catch><xsl:text>caught</xsl:text></xsl:catch></xsl:try><xsl:next-match/></xsl:template>" +
            "<xsl:template match='a' priority='1'><xsl:text>base</xsl:text></xsl:template>");
        Assert.Equal("caughtbase", value.NodeValue.StringValue);
    }

    /// <summary>Uses singleton document focus and restores the caller after it.</summary>
    /// <param name="function">Whether the collecting dispatcher is used.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DocumentFocus(bool function)
    {
        var value = Run("<xsl:for-each select='$seed/*'><xsl:copy select='$doc'><xsl:value-of select='" + Focus + "'/></xsl:copy><xsl:text>|</xsl:text><xsl:value-of select='" + Focus + "'/></xsl:for-each>", function, declarations:
            "<xsl:variable name='doc' as='document-node()'><xsl:document><old/></xsl:document></xsl:variable>");
        Assert.Equal(":1:1|a:1:2:1:1|b:2:2", value.NodeValue.StringValue);
    }

    /// <summary>Clears the current template rule inside selected copies on both dispatch paths.</summary>
    /// <param name="collecting">Whether a typed variable collects the copy.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SelectedCopyClearsRule(bool collecting)
    {
        var content = "<xsl:copy select='$seed'><xsl:next-match/></xsl:copy>";
        if (collecting) content = "<xsl:variable name='v' as='item()*'>" + content + "</xsl:variable><xsl:sequence select='$v'/>";
        Assert.Contains("XTDE0560", Assert.ThrowsAny<Exception>(() => Run("<xsl:apply-templates select='$seed/a'/>", declarations:
            "<xsl:template match='a' priority='2'>" + content + "</xsl:template><xsl:template match='a' priority='1'>base</xsl:template>")).ToString());
    }

    /// <summary>Retains the current template rule when select is absent.</summary>
    /// <param name="collecting">Whether a typed variable collects the copy.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ContextCopyRetainsRule(bool collecting)
    {
        var content = "<xsl:copy><xsl:next-match/></xsl:copy>";
        if (collecting) content = "<xsl:variable name='v' as='item()*'>" + content + "</xsl:variable><xsl:sequence select='$v'/>";
        Assert.Equal("base", Run("<xsl:apply-templates select='$seed/a'/>", declarations:
            "<xsl:template match='a' priority='2'>" + content + "</xsl:template><xsl:template match='a' priority='1'>base</xsl:template>").NodeValue.StringValue);
    }

    /// <summary>Does not evaluate copy content for empty or atomic selections.</summary>
    /// <param name="function">Whether the collecting dispatcher is used.</param>
    /// <param name="select">The selected empty or atomic value.</param>
    /// <param name="expected">The expected result text.</param>
    [Theory]
    [InlineData(false, "()", "a:1:2b:2:2")]
    [InlineData(true, "()", "a:1:2b:2:2")]
    [InlineData(false, "9", "9a:1:29b:2:2")]
    [InlineData(true, "9", "9a:1:29b:2:2")]
    public void IgnoredContent(bool function, string select, string expected)
    {
        var value = Run("<xsl:for-each select='$seed/*'><xsl:copy select='" + select + "'><xsl:sequence select='error()'/></xsl:copy><xsl:value-of select='" + Focus + "'/></xsl:for-each>", function);
        Assert.Equal(expected, value.NodeValue.StringValue);
        foreach (var child in value.NodeValue.Axis(XdmAxis.Child)) Assert.Equal(XdmNodeKind.Text, child.NodeValue.NodeKind);
    }
}
