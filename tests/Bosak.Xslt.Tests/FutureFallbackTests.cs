// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies draft static errors for future instructions lacking fallback.
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
//                      | Charles Korthout | 0.2   | 16-10-2026     | Expand valid function fallbacks with raw items, sequence order and scope/error recovery.     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.Xslt.Api;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies future instruction fallback boundaries without changing the legacy lane.</summary>
[Collection("XSLT package registry")]
public class FutureFallbackTests
{
    private static string Sheet(string body, string version = "17.0", bool function = false, string declarations = "") =>
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:functions' version='" + version + "'>" + declarations +
        (function ? "<xsl:function name='f:probe'>" + body + "</xsl:function>" : "") +
        "<xsl:template name='start'>" + (function ? "<xsl:sequence select='f:probe()'/>" : body) + "</xsl:template></xsl:stylesheet>";
    private static XsltCompiler Compiler(bool draft = true) => new() { ProcessorProfile = draft ? XsltProcessorProfile.Xslt40Draft : XsltProcessorProfile.Legacy };
    private static void StaticError(string source) => Assert.Contains("XTSE0010", Assert.ThrowsAny<Exception>(() => Compiler().Compile(source)).ToString());
    private static XdmValue Run(string source, bool draft = true) => Compiler(draft).Compile(source).Transform(null, initialTemplate: "start", rawResult: true);

    /// <summary>Future unknown instructions require fallback in ordinary and function bodies.</summary>
    /// <param name="version">The future effective version.</param>
    /// <param name="function">Whether the unknown instruction is in a function.</param>
    [Theory]
    [InlineData("5.0", false)]
    [InlineData("5.0", true)]
    [InlineData("17.0", false)]
    [InlineData("17.0", true)]
    public void MissingFallbackStaticError(string version, bool function) => StaticError(Sheet("<xsl:future/>", version, function));

    /// <summary>Unused template and function bodies are still validated statically.</summary>
    /// <param name="function">Whether the unused body is a function.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnusedBodyStaticError(bool function) => StaticError(Sheet("<xsl:sequence select='7'/>", declarations:
        function ? "<xsl:function name='f:unused'><xsl:future/></xsl:function>" : "<xsl:template name='unused'><xsl:future/></xsl:template>"));

    /// <summary>Local future versions do not bypass the no-fallback static rule.</summary>
    [Fact]
    public void LocalFutureVersion() => StaticError(Sheet("<xsl:future version='17.0'/>", "4.0"));

    /// <summary>Runtime catch cannot suppress the compile-time missing-fallback error.</summary>
    [Fact]
    public void TryCannotCatchStaticError() => StaticError(Sheet("<xsl:try><xsl:future/><xsl:catch><xsl:sequence select='7'/></xsl:catch></xsl:try>"));

    /// <summary>A fallback removed by conditional inclusion is no fallback.</summary>
    [Fact]
    public void RemovedFallback() => StaticError(Sheet("<xsl:future><xsl:fallback use-when='false()'/></xsl:future>"));

    /// <summary>Future instructions with fallback continue to evaluate its result.</summary>
    /// <param name="version">The future effective version.</param>
    /// <param name="function">Whether evaluation occurs in a function.</param>
    [Theory]
    [InlineData("5.0", false)]
    [InlineData("5.0", true)]
    [InlineData("17.0", false)]
    [InlineData("17.0", true)]
    public void PresentFallback(string version, bool function) => Assert.Equal(7, Run(Sheet("<xsl:future><xsl:fallback><xsl:sequence select='7'/></xsl:fallback></xsl:future>", version, function)).IntegerValue);

    /// <summary>Multiple fallback children retain their ordered sequence.</summary>
    /// <param name="function">Whether the sequence is returned by a function.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MultipleFallbacks(bool function)
    {
        var result = Run(Sheet("<xsl:future><xsl:fallback><xsl:sequence select='7'/></xsl:fallback><xsl:fallback><xsl:sequence select='8'/></xsl:fallback></xsl:future>", function: function));
        var values = new List<long>(); foreach (var value in XdmSequence.FromSource(result.SequenceValue!)) values.Add(value.IntegerValue);
        Assert.Equal(new long[] { 7, 8 }, values);
    }

    /// <summary>Non-fallback siblings of fallback are ignored.</summary>
    /// <param name="function">Whether evaluation occurs in a function.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IgnoredSiblings(bool function) => Assert.Equal(7, Run(Sheet("<xsl:future><xsl:sequence select='undefined-function()'/><xsl:fallback><xsl:sequence select='7'/></xsl:fallback></xsl:future>", function: function)).IntegerValue);

    /// <summary>Ignored nested unknown instructions do not produce spurious static errors.</summary>
    [Fact]
    public void IgnoredNestedUnknown() => Assert.Equal(7, Run(Sheet("<xsl:future><xsl:ignored-child/><xsl:fallback><xsl:sequence select='7'/></xsl:fallback></xsl:future>")).IntegerValue);

    /// <summary>Future top-level declarations are ignored, including their contents.</summary>
    /// <param name="version">The future module version.</param>
    [Theory]
    [InlineData("5.0")]
    [InlineData("17.0")]
    public void TopLevelIgnored(string version) => Assert.Equal(7, Run(Sheet("<xsl:sequence select='7'/>", version, declarations: "<xsl:future><xsl:ignored-child/></xsl:future>")).IntegerValue);

    /// <summary>Conditionally excluded unknown instructions are absent from static validation.</summary>
    [Fact]
    public void ExcludedInstruction() => Assert.Equal(7, Run(Sheet("<xsl:future use-when='false()'/><xsl:sequence select='7'/>")).IntegerValue);

    /// <summary>Known-version unknown instructions remain errors even with fallback.</summary>
    /// <param name="version">The supported effective version.</param>
    [Theory]
    [InlineData("3.0")]
    [InlineData("4.0")]
    public void KnownVersionRejected(string version) => StaticError(Sheet("<xsl:future><xsl:fallback/></xsl:future>", version));

    /// <summary>The frozen legacy lane keeps its prior no-fallback tolerance.</summary>
    /// <param name="version">The legacy forward-compatible version.</param>
    [Theory]
    [InlineData("4.0")]
    [InlineData("5.0")]
    [InlineData("17.0")]
    public void LegacyTolerance(string version) => Assert.Equal(7, Run(Sheet("<xsl:future/><xsl:sequence select='7'/>", version), false).IntegerValue);

    /// <summary>The legacy supported version keeps its unknown-instruction error.</summary>
    [Fact]
    public void LegacyKnownVersionRejected() => Assert.Contains("XTSE0010", Assert.ThrowsAny<Exception>(() => Compiler(false).Compile(Sheet("<xsl:future/>", "3.0"))).ToString());

    /// <summary>Foreign literal elements are not mistaken for unsupported XSLT instructions.</summary>
    [Fact]
    public void ForeignLiteral()
    {
        var node = Run(Sheet("<e:future xmlns:e='urn:literal'>ok</e:future>")).NodeValue;
        Assert.Equal("future", node.LocalName); Assert.Equal("urn:literal", node.NamespaceUri); Assert.Equal("ok", node.StringValue);
    }

    /// <summary>Known instructions do not require fallback at a future effective version.</summary>
    [Fact]
    public void KnownInstruction() => Assert.Equal(7, Run(Sheet("<xsl:sequence select='7'/>")).IntegerValue);

    /// <summary>Active fallback descendants retain their own missing-fallback static errors.</summary>
    [Fact]
    public void ActiveFallbackDescendant() => StaticError(Sheet("<xsl:future><xsl:fallback><xsl:nested-future version='17.0'/></xsl:fallback></xsl:future>"));

    /// <summary>XSLT namespace identity applies independently of the lexical prefix.</summary>
    [Fact]
    public void AliasPrefixRejected() => StaticError(Sheet("<u:future xmlns:u='http://www.w3.org/1999/XSL/Transform'/>"));

    /// <summary>Imported and included unused bodies inherit the same static requirement.</summary>
    /// <param name="kind">The module relationship.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void ModuleStaticError(string kind)
    {
        var compiler = Compiler(); compiler.UriResolver = new Resolver(Sheet("<xsl:future/>").Replace("name='start'", "name='helper'"));
        Assert.Contains("XTSE0010", Assert.ThrowsAny<Exception>(() => compiler.Compile(Sheet("<xsl:sequence select='7'/>", declarations: "<xsl:" + kind + " href='module.xsl'/>"))).ToString());
    }

    /// <summary>Used package bodies receive static validation even when not invoked.</summary>
    [Fact]
    public void PackageStaticError()
    {
        XsltFunctionLibrary.RegisterPackage("urn:fallback-package", "1.0", "https://example.invalid/package.xsl");
        try
        {
            var compiler = Compiler();
            compiler.UriResolver = new Resolver("<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:functions' name='urn:fallback-package' package-version='1.0' version='17.0'><xsl:function name='f:unused' visibility='public'><xsl:future/></xsl:function></xsl:package>");
            Assert.Contains("XTSE0010", Assert.ThrowsAny<Exception>(() => compiler.Compile(Sheet("<xsl:sequence select='7'/>", declarations: "<xsl:use-package name='urn:fallback-package' package-version='1.0'/>"))).ToString());
        }
        finally { XsltFunctionLibrary.ClearPackages(); }
    }

    /// <summary>Function fallback retains maps as raw items.</summary>
    [Fact]
    public void FunctionFallbackMap()
    {
        var result = Run(Sheet("<xsl:future><xsl:fallback><xsl:sequence select='map{&quot;v&quot;:7}'/></xsl:fallback></xsl:future>", function: true));
        Assert.Equal(XdmValueKind.Map, result.Kind);
        Assert.True(result.MapValue!.TryGetValue(XdmValue.FromString("v"), out var value));
        Assert.Equal(7, value.IntegerValue);
    }

    /// <summary>Fallback variables do not leak to sibling fallback constructors or their caller.</summary>
    [Fact]
    public void FunctionFallbackVariables()
    {
        var result = Run(Sheet("<xsl:variable name='v' select='1'/><xsl:future><xsl:fallback><xsl:variable name='v' select='2'/><xsl:sequence select='$v'/></xsl:fallback><xsl:fallback><xsl:sequence select='$v'/></xsl:fallback></xsl:future><xsl:sequence select='$v'/>", function: true));
        var values = new List<long>(); foreach (var value in XdmSequence.FromSource(result.SequenceValue!)) values.Add(value.IntegerValue);
        Assert.Equal(new long[] { 2, 1, 1 }, values);
    }

    /// <summary>Fallback variable scope is restored before a surrounding runtime catch.</summary>
    [Fact]
    public void FunctionFallbackErrorRecovery() => Assert.Equal(1, Run(Sheet("<xsl:variable name='v' select='1'/><xsl:try><xsl:future><xsl:fallback><xsl:variable name='v' select='2'/><xsl:sequence select='error()'/></xsl:fallback></xsl:future><xsl:catch><xsl:sequence select='$v'/></xsl:catch></xsl:try>", function: true)).IntegerValue);

    private sealed class Resolver(string source) : IXsltUriResolver
    {
        /// <summary>Resolves the test stylesheet module.</summary>
        /// <param name="href">The requested URI.</param>
        /// <param name="baseUri">The referring module URI.</param>
        /// <returns>The parsed module.</returns>
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(source);
    }
}
