// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies namespace declaration and foreign-attribute classification on xsl:copy.
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

/// <summary>Verifies namespace declaration and foreign-attribute classification on xsl:copy.</summary>
public class CopyNamespaceValidationTests
{
    private static XdmValue Run(string copy, bool function = false, string version = "4.0") => new Api.XsltCompiler().Compile(
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:functions' version='" + version + "'>" +
        "<xsl:variable name='seed' as='element()'><seed/></xsl:variable>" +
        (function ? "<xsl:function name='f:probe' as='item()*'>" + copy + "</xsl:function>" : "") +
        "<xsl:template name='start'>" + (function ? "<xsl:sequence select='f:probe()'/>" : copy) + "</xsl:template></xsl:stylesheet>")
        .Transform(null, initialTemplate: "start", rawResult: true);

    /// <summary>Accepts namespace declarations and ignores foreign metadata in both execution paths.</summary>
    /// <param name="copy">The copy instruction.</param>
    /// <param name="function">Whether copying occurs in a stylesheet function.</param>
    [Theory]
    [InlineData("<copy xmlns='http://www.w3.org/1999/XSL/Transform' select='7'/>", false)]
    [InlineData("<copy xmlns='http://www.w3.org/1999/XSL/Transform' select='7'/>", true)]
    [InlineData("<xsl:copy xmlns='' select='7'/>", false)]
    [InlineData("<xsl:copy xmlns='' select='7'/>", true)]
    [InlineData("<xsl:copy xmlns='urn:result' select='7'/>", false)]
    [InlineData("<xsl:copy xmlns='urn:result' select='7'/>", true)]
    [InlineData("<xsl:copy xmlns:copy-namespaces='urn:metadata' select='7'/>", false)]
    [InlineData("<xsl:copy xmlns:copy-namespaces='urn:metadata' select='7'/>", true)]
    [InlineData("<xsl:copy xmlns:e='urn:metadata' e:copy-namespaces='metadata' select='7'/>", false)]
    [InlineData("<xsl:copy xmlns:e='urn:metadata' e:copy-namespaces='metadata' select='7'/>", true)]
    [InlineData("<xsl:copy xmlns:e='urn:metadata' e:select='metadata' select='7'/>", false)]
    [InlineData("<xsl:copy xmlns:e='urn:metadata' e:select='metadata' select='7'/>", true)]
    public void NamespaceAndMetadata(string copy, bool function)
    {
        var value = Run(copy, function);
        Assert.Equal(XdmValueKind.Integer, value.Kind);
        Assert.Equal(7, value.IntegerValue);
    }

    /// <summary>Retains the default namespace's effect on literal child names.</summary>
    /// <param name="function">Whether copying occurs in a stylesheet function.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LiteralChildNamespace(bool function)
    {
        var value = Run("<xsl:copy xmlns='urn:result' select='$seed'><child>ok</child></xsl:copy>", function);
        Assert.Equal("seed", value.NodeValue.LocalName);
        Assert.Equal("", value.NodeValue.NamespaceUri);
        var children = new List<XdmValue>();
        foreach (var child in value.NodeValue.Axis(XdmAxis.Child)) children.Add(child);
        Assert.Single(children);
        Assert.Equal("urn:result", children[0].NodeValue.NamespaceUri);
        Assert.Equal("ok", children[0].NodeValue.StringValue);
    }

    /// <summary>Still rejects invalid real instruction attributes and option values.</summary>
    /// <param name="copy">The invalid copy instruction.</param>
    /// <param name="code">Its required static error.</param>
    [Theory]
    [InlineData("<xsl:copy select='7' bogus='bad'/>", "XTSE0090")]
    [InlineData("<xsl:copy xmlns='urn:r' select='7' bogus='bad'/>", "XTSE0090")]
    [InlineData("<xsl:copy select='7' copy-namespaces='metadata'/>", "XTSE0020")]
    [InlineData("<xsl:copy xmlns='urn:r' select='7' copy-namespaces='metadata'/>", "XTSE0020")]
    public void InvalidRealAttributes(string copy, string code) => Assert.Contains(code, Assert.ThrowsAny<Exception>(() => Run(copy)).ToString());

    /// <summary>Accepts real boolean copy options beside namespace declarations.</summary>
    /// <param name="option">The real option value.</param>
    [Theory]
    [InlineData("yes")]
    [InlineData("no")]
    public void RealBooleanOption(string option) => Assert.Equal(7, Run("<xsl:copy xmlns='urn:r' copy-namespaces='" + option + "' select='7'/>").IntegerValue);

    /// <summary>Accepts the same namespace forms in the earlier stylesheet profile.</summary>
    [Fact]
    public void EarlierVersion() => Assert.Equal(7, Run("<copy xmlns='http://www.w3.org/1999/XSL/Transform' select='7'/>", version: "3.0").IntegerValue);

    /// <summary>Classifies attributes statically even when the containing function is unused.</summary>
    /// <param name="copy">The unused copy instruction.</param>
    /// <param name="error">The expected static error, or null for valid input.</param>
    [Theory]
    [InlineData("<copy xmlns='http://www.w3.org/1999/XSL/Transform' select='7'/>", null)]
    [InlineData("<xsl:copy select='7' bogus='bad'/>", "XTSE0090")]
    public void UnusedFunction(string copy, string? error)
    {
        var source = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:functions' version='4.0'><xsl:function name='f:unused'>" + copy + "</xsl:function><xsl:template name='start'/></xsl:stylesheet>";
        if (error == null) Assert.NotNull(new Api.XsltCompiler().Compile(source));
        else Assert.Contains(error, Assert.ThrowsAny<Exception>(() => new Api.XsltCompiler().Compile(source)).ToString());
    }
}
