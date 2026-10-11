// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 October 2026
// PURPOSE              : Verifies effective-version XPath profiles across stylesheet compilation and execution.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-10-2026     | Creation: effective-version XPath40 regression coverage.                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies XPath40 activation and XPath31 isolation at effective-version boundaries.</summary>
public class XPathCompatibilityRuntimeTests
{
    private static string Wrap(string body, string version = "4.0", string declarations = "")
        => $"<xsl:stylesheet version='{version}' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
           "xmlns:f='urn:test' xmlns:fn='http://www.w3.org/2005/xpath-functions' xmlns:xs='http://www.w3.org/2001/XMLSchema' exclude-result-prefixes='f xs'>" +
           "<xsl:output method='text'/>" + declarations + "<xsl:template match='/'>" + body +
           "</xsl:template></xsl:stylesheet>";

    private static string Run(string stylesheet, XsltCompiler? compiler = null)
        => (compiler ?? new XsltCompiler()).Compile(stylesheet, "https://example.invalid/main.xsl")
            .TransformToString(new XDocumentNode(XDocument.Parse("<root/>")));

    /// <summary>Instruction, literal AVT, text template and dynamic expression paths opt into frozen 4.0.</summary>
    /// <param name="body">The instruction sequence.</param>
    /// <param name="expected">Its text result.</param>
    [Theory]
    [InlineData("<xsl:value-of select='count(partition((1,2,3), function($a,$b,$p){count($a) ge 2}))'/>", "2")]
    [InlineData("<xsl:value-of select='() ?? 7'/>", "7")]
    [InlineData("<xsl:variable name='v' select='count(partition((1,2,3), function($a,$b){count($a) ge 2}))'/><xsl:value-of select='$v'/>", "2")]
    [InlineData("<xsl:for-each select='partition((1,2,3), function($a,$b){count($a) ge 2})'><xsl:value-of select='count(?*)'/></xsl:for-each>", "21")]
    [InlineData("<xsl:copy-of select='count(partition((1,2,3), function($a,$b){count($a) ge 2}))'/>", "2")]
    [InlineData("<xsl:element name='{&quot;result&quot; ?? &quot;unused&quot;}'><xsl:value-of select='7'/></xsl:element>", "7")]
    [InlineData("<result xsl:expand-text='yes'>{count(partition((1,2,3), function($a,$b){count($a) ge 2}))}</result>", "2")]
    [InlineData("<xsl:evaluate xpath='&quot;count(partition((1,2,3), function($a,$b){count($a) ge 2}))&quot;'/>", "2")]
    [InlineData("<xsl:try select='() ?? 7'><xsl:catch select='0'/></xsl:try>", "7")]
    [InlineData("<xsl:try select='error()'><xsl:catch select='() ?? 7'/></xsl:try>", "7")]
    [InlineData("<xsl:number value='() ?? 7'/>", "7")]
    public void Frozen40_CompilesAcrossRuntimePaths(string body, string expected)
        => Assert.Equal(expected, Run(Wrap(body)));

    /// <summary>Literal attribute templates produce the computed value with schema-aware compilation.</summary>
    [Fact]
    public void LiteralAttributeTemplate_PreservesSchemaAwareProfile()
    {
        var stylesheet = Wrap("<result value='{count(partition((1,2,3), function($a,$b){count($a) ge 2}))}'/>")
            .Replace("method='text'", "method='xml' omit-xml-declaration='yes'");
        var result = XElement.Parse(Run(stylesheet, new XsltCompiler { SchemaAware = true }));
        Assert.Equal("2", (string?)result.Attribute("value"));
    }

    /// <summary>Earlier stylesheet versions refuse 4.0-only calls and grammar.</summary>
    /// <param name="version">The earlier effective version.</param>
    [Theory]
    [InlineData("1.0")]
    [InlineData("2.0")]
    [InlineData("3.0")]
    public void EarlierVersions_KeepXPath31(string version)
    {
        var functionFailure = Assert.ThrowsAny<Exception>(() => Run(Wrap("<xsl:value-of select='count(partition((), function($a,$b){true()}))'/>", version)));
        Assert.Contains("XPST0017", functionFailure.Message);
        var grammarFailure = Assert.ThrowsAny<Exception>(() => Run(Wrap("<xsl:value-of select='() ?? 7'/>", version)));
        Assert.Contains("XPST0003", grammarFailure.Message);
    }

    /// <summary>Local XSLT and literal-element overrides determine expression compatibility.</summary>
    [Fact]
    public void LocalOverrides_DoNotLeakAcrossInstructionsOrCachedExpressions()
    {
        Assert.Equal("7", Run(Wrap("<xsl:value-of version='4.0' select='() ?? 7'/>", "3.0")));
        Assert.Equal("7", Run(Wrap("<result xsl:version='4.0' xsl:expand-text='yes'>{() ?? 7}</result>", "3.0")));
        var failure = Assert.ThrowsAny<Exception>(() => Run(Wrap("<xsl:value-of select='() ?? 7'/><xsl:value-of version='3.0' select='() ?? 7'/>")));
        Assert.Contains("XPST0003", failure.Message);
        var literalFailure = Assert.ThrowsAny<Exception>(() => Run(Wrap("<result xsl:version='3.0' value='{() ?? 7}'/>")));
        Assert.Contains("XPST0003", literalFailure.Message);
    }

    /// <summary>Static variables, use-when and shadow AVTs follow the declaring element.</summary>
    [Fact]
    public void StaticExpressions_UseEffectiveVersion()
    {
        const string declarations = "<xsl:variable name='v' static='yes' select='() ?? 7'/>";
        Assert.Equal("7", Run(Wrap("<xsl:value-of select='$v'/>", declarations: declarations)));
        Assert.Equal("7", Run(Wrap("<xsl:value-of use-when='every((1,2), function($i,$p){true()})' select='7'/>")));
        Assert.Equal("7", Run(Wrap("<xsl:value-of _select='{&quot;() ?? 7&quot; ?? &quot;0&quot;}'/>")));
        var failure = Assert.ThrowsAny<Exception>(() => Run(Wrap("<xsl:value-of select='$v'/>", "3.0", declarations)));
        Assert.Contains("XPST0003", failure.Message);
    }

    /// <summary>Serialization version does not control XPath compilation of shadow output attributes.</summary>
    [Fact]
    public void OutputVersion_IsNotAnExpressionVersionOverride()
    {
        const string output = "<xsl:output method='text' version='1.0' _encoding='{() ?? &quot;UTF-8&quot;}'/>";
        Assert.Equal("7", Run(Wrap("<xsl:value-of select='7'/>", declarations: output)));
        var failure = Assert.ThrowsAny<Exception>(() => Run(Wrap("<xsl:value-of select='7'/>", "3.0", output.Replace("version='1.0'", "version='4.0'"))));
        Assert.Contains("XPST0003", failure.Message);
    }

    /// <summary>Function bodies retain 4.0 grammar even when evaluated as raw sequences.</summary>
    [Fact]
    public void FunctionBody_UsesItsOwnVersion()
    {
        const string declarations = "<xsl:function name='f:value'><xsl:for-each select='partition((1,2,3), function($a,$b){count($a) ge 2})'><xsl:sequence select='count(?*)'/></xsl:for-each></xsl:function>";
        Assert.Equal("2 1", Run(Wrap("<xsl:value-of select='f:value()'/>", declarations: declarations)));
    }

    /// <summary>Future version declarations use frozen features without granting experimental scan.</summary>
    [Fact]
    public void FutureVersion_UsesFrozenProfileOnly()
    {
        Assert.Equal("7", Run(Wrap("<xsl:value-of select='() ?? 7'/>", "5.0")));
        var failure = Assert.ThrowsAny<Exception>(() => Run(Wrap("<xsl:sequence select='scan((1,2), 0, function($a,$b){$a+$b})'/>", "5.0")));
        Assert.Contains("XPST0017", failure.Message);
    }

    /// <summary>Dynamic lookup follows the expression version, including after a nested 3.0 function call.</summary>
    [Fact]
    public void DynamicLookup_IsolatedAcrossNestedAndSiblingVersions()
    {
        const string declaration = "<xsl:function name='f:earlier'><xsl:sequence version='3.0' select='empty(function-lookup(xs:QName(&quot;fn:partition&quot;), 2))'/></xsl:function>";
        Assert.Equal("true true", Run(Wrap("<xsl:value-of select='f:earlier(), exists(function-lookup(xs:QName(&quot;fn:partition&quot;), 2))'/>", declarations: declaration)));
        Assert.Equal("true", Run(Wrap("<xsl:value-of select='empty(function-lookup(xs:QName(&quot;fn:partition&quot;), 2))'/>", "3.0")));
    }

    /// <summary>Pattern predicates inherit their declaring template's version and preserve the caller profile.</summary>
    [Fact]
    public void MatchPredicate_UsesTheDeclaringVersion()
    {
        const string declarations = "<xsl:template match='root[every((1,2), boolean#1)]'><xsl:text>matched</xsl:text></xsl:template>";
        Assert.Equal("matched", Run(Wrap("<xsl:apply-templates/>", declarations: declarations)));
        var failure = Assert.ThrowsAny<Exception>(() => Run(Wrap("<xsl:apply-templates/>", "3.0", declarations)));
        Assert.Contains("XPST0017", failure.Message);
    }

    /// <summary>Grouping boundary patterns compile their predicates at the owning instruction's version.</summary>
    [Fact]
    public void GroupingPredicate_UsesInstructionVersion()
    {
        const string body = "<xsl:for-each-group select='root' group-starting-with='root[every((1,2), function($i,$p){true()})]'><xsl:value-of select='count(current-group())'/></xsl:for-each-group>";
        Assert.Equal("1", Run(Wrap(body)));
    }

    /// <summary>Validation and execution select the same frozen profile, including future declarations.</summary>
    /// <param name="version">The stylesheet version.</param>
    /// <param name="valid">Whether frozen 4.0 expressions are available.</param>
    [Theory]
    [InlineData("3.0", false)]
    [InlineData("4.0", true)]
    [InlineData("5.0", true)]
    public void Validation_AgreesWithRuntimeProfile(string version, bool valid)
    {
        var stylesheet = Wrap("<xsl:value-of select='() ?? 7'/>", version,
            "<xsl:template match='root[every((1,2), boolean#1)]'/>");
        var result = Bosak.Xslt.Validation.XsltValidation.Validate(System.Text.Encoding.UTF8.GetBytes(stylesheet),
            new Uri("https://example.invalid/main.xsl"));
        Assert.Equal(valid, result.IsValid);
    }

    /// <summary>Included modules retain their declared version in either direction.</summary>
    /// <param name="mainVersion">The including module's version.</param>
    /// <param name="moduleVersion">The included module's version.</param>
    /// <param name="succeeds">Whether the included expression should compile.</param>
    /// <param name="reference">The include or import instruction.</param>
    [Theory]
    [InlineData("3.0", "4.0", true, "include")]
    [InlineData("4.0", "3.0", false, "include")]
    [InlineData("3.0", "4.0", true, "import")]
    [InlineData("4.0", "3.0", false, "import")]
    public void IncludedModule_RetainsVersion(string mainVersion, string moduleVersion, bool succeeds, string reference)
    {
        var module = $"<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='{moduleVersion}'><xsl:template name='module'><xsl:value-of select='() ?? 7'/></xsl:template></xsl:stylesheet>";
        var compiler = new XsltCompiler { UriResolver = new ModuleResolver(module) };
        var stylesheet = Wrap("<xsl:call-template name='module'/>", mainVersion, $"<xsl:{reference} href='module.xsl'/>")
            .Replace("<xsl:output method='text'/>", "")
            .Replace($"<xsl:{reference} href='module.xsl'/>", $"<xsl:{reference} href='module.xsl'/><xsl:output method='text'/>");
        if (succeeds) Assert.Equal("7", Run(stylesheet, compiler));
        else Assert.Contains("XPST0003", Assert.ThrowsAny<Exception>(() => Run(stylesheet, compiler)).Message);
    }

    private sealed class ModuleResolver(string text) : IXsltUriResolver
    {
        /// <summary>Returns the sole in-memory module without external IO.</summary>
        /// <param name="href">The requested module reference.</param>
        /// <param name="baseUri">The importing module's base URI.</param>
        /// <returns>The module document.</returns>
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(text);
    }
}
