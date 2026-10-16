// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 October 2026
// PURPOSE              : Verifies host processor profiles across validation, XPath and module loading.
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
//                      | Charles Korthout | 0.2   | 16-10-2026     | Inspect sequence items and expand module/profile boundary controls.                         |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.Xslt.Api;
using Bosak.XPath.Core.Xdm;
using Xunit;

namespace Bosak.Xslt.Tests;

/// <summary>Verifies processor profiles independently of stylesheet versions.</summary>
[Collection("XSLT package registry")]
public class ProcessorProfileTests
{
    private static string Wrap(string body, string version = "4.0", string declarations = "") =>
        "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='" + version + "'>" +
        declarations + "<xsl:template name='start'>" + body + "</xsl:template></xsl:stylesheet>";

    private static XsltCompiler Compiler(bool draft = true) => new() { ProcessorProfile = draft ? XsltProcessorProfile.Xslt40Draft : XsltProcessorProfile.Legacy };

    /// <summary>Retains the legacy default and rejects unknown host profile values.</summary>
    [Fact]
    public void DefaultAndInvalidProfile()
    {
        Assert.Equal(XsltProcessorProfile.Legacy, new XsltCompiler().ProcessorProfile);
        var compiler = new XsltCompiler { ProcessorProfile = (XsltProcessorProfile)99 };
        Assert.Throws<ArgumentOutOfRangeException>(() => compiler.Compile(Wrap("")));
        Assert.Throws<ArgumentOutOfRangeException>(() => compiler.Compile(XDocument.Parse(Wrap(""))));
    }

    /// <summary>Checks unknown unqualified attributes against the selected processor ceiling.</summary>
    /// <param name="instruction">The instruction being checked.</param>
    /// <param name="version">The effective version.</param>
    /// <param name="draft">Whether the host opted into draft 4.0.</param>
    /// <param name="accepted">Whether compilation must succeed.</param>
    [Theory]
    [InlineData("copy", "3.0", true, false)]
    [InlineData("copy", "4.0", true, false)]
    [InlineData("copy", "5.0", true, true)]
    [InlineData("copy", "17.0", true, true)]
    [InlineData("apply-templates", "3.0", true, false)]
    [InlineData("apply-templates", "4.0", true, false)]
    [InlineData("apply-templates", "5.0", true, true)]
    [InlineData("apply-templates", "17.0", true, true)]
    [InlineData("sequence", "3.0", true, false)]
    [InlineData("sequence", "4.0", true, false)]
    [InlineData("sequence", "5.0", true, true)]
    [InlineData("sequence", "17.0", true, true)]
    [InlineData("copy", "3.0", false, false)]
    [InlineData("copy", "4.0", false, false)]
    [InlineData("copy", "5.0", false, false)]
    [InlineData("copy", "17.0", false, false)]
    [InlineData("apply-templates", "3.0", false, false)]
    [InlineData("apply-templates", "4.0", false, true)]
    [InlineData("apply-templates", "5.0", false, true)]
    [InlineData("apply-templates", "17.0", false, true)]
    [InlineData("sequence", "3.0", false, true)]
    [InlineData("sequence", "4.0", false, true)]
    [InlineData("sequence", "5.0", false, true)]
    [InlineData("sequence", "17.0", false, true)]
    public void UnknownAttributeMatrix(string instruction, string version, bool draft, bool accepted)
    {
        var error = Record.Exception(() => Compiler(draft).Compile(Wrap("<xsl:" + instruction + " bogus='ignored'/>", version)));
        if (accepted) Assert.Null(error);
        else Assert.Contains("XTSE0090", Assert.IsAssignableFrom<Exception>(error).ToString());
    }

    /// <summary>Uses local effective versions for suppression without changing processor capability.</summary>
    /// <param name="version">The local instruction version.</param>
    /// <param name="accepted">Whether the unknown attribute is ignored.</param>
    [Theory]
    [InlineData("3.0", false)]
    [InlineData("3.3", false)]
    [InlineData("4.0", false)]
    [InlineData("4.1", true)]
    public void LocalVersion(string version, bool accepted)
    {
        var error = Record.Exception(() => Compiler().Compile(Wrap("<xsl:sequence version='" + version + "' bogus='ignored'/>", "17.0")));
        if (accepted) Assert.Null(error);
        else Assert.Contains("XTSE0090", Assert.IsAssignableFrom<Exception>(error).ToString());
    }

    /// <summary>Retains checks for invalid known options even alongside ignored future attributes.</summary>
    /// <param name="attribute">The invalid known copy option.</param>
    /// <param name="errorCode">The required error.</param>
    [Theory]
    [InlineData("copy-namespaces='bad'", "XTSE0020")]
    [InlineData("use-attribute-sets='missing'", "XTSE0710")]
    public void KnownOptionsStillChecked(string attribute, string errorCode) =>
        Assert.Contains(errorCode, Assert.ThrowsAny<Exception>(() => Compiler().Compile(Wrap("<xsl:copy bogus='ignored' " + attribute + "/>", "17.0"))).ToString());

    /// <summary>Preserves standard attributes and qualified metadata in strict draft validation.</summary>
    [Fact]
    public void StandardAndForeignAttributes()
    {
        var executable = Compiler().Compile(Wrap("<xsl:sequence xmlns:e='urn:meta' e:bogus='ok' expand-text='yes' select='7'/><xsl:copy select='8' xmlns='urn:data'/>"));
        var values = new List<long>();
        foreach (var value in XdmSequence.FromSource(executable.Transform(null, initialTemplate: "start", rawResult: true).SequenceValue!))
            values.Add(value.IntegerValue);
        Assert.Equal(new long[] { 7, 8 }, values);
    }

    /// <summary>Uses frozen XPath 4.0 at effective versions 3.0 and 4.0 in the draft processor.</summary>
    /// <param name="version">The stylesheet version.</param>
    [Theory]
    [InlineData("3.0")]
    [InlineData("4.0")]
    public void DraftXPathIndependentOfVersion(string version)
    {
        var compiler = Compiler();
        var result = compiler.Compile(Wrap("<xsl:sequence select='() ?? 7'/>", version)).Transform(null, initialTemplate: "start", rawResult: true);
        Assert.Equal(7, result.IntegerValue);
        Assert.Contains("XPST0017", Assert.ThrowsAny<Exception>(() => compiler.Compile(Wrap("<xsl:sequence select='scan((1,2), 0, function($a,$b){$a+$b})'/>", version)).Transform(null, initialTemplate: "start")).ToString());
    }

    /// <summary>Static expression paths share the host profile with runtime expressions.</summary>
    [Fact]
    public void StaticXPathProfile()
    {
        var result = Compiler().Compile(Wrap("<xsl:sequence use-when='exists(() ?? 7)' _select='{&quot;() ?? 8&quot;}'/>", "3.0"))
            .Transform(null, initialTemplate: "start", rawResult: true);
        Assert.Equal(8, result.IntegerValue);
    }

    /// <summary>The compiled executable retains its profile after the compiler is reused.</summary>
    [Fact]
    public void CompilationCapturesProfile()
    {
        var compiler = Compiler();
        var executable = compiler.Compile(Wrap("<xsl:sequence select='() ?? 7'/>", "3.0"));
        compiler.ProcessorProfile = XsltProcessorProfile.Legacy;
        Assert.Equal(7, executable.Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);
        Assert.Contains("XPST0003", Assert.ThrowsAny<Exception>(() => compiler.Compile(Wrap("<xsl:sequence select='() ?? 7'/>", "3.0")).Transform(null, initialTemplate: "start")).ToString());
    }

    /// <summary>Included and imported modules inherit the compilation profile.</summary>
    /// <param name="kind">The module relationship.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void ModulesInheritProfile(string kind)
    {
        var compiler = Compiler();
        compiler.UriResolver = new ModuleResolver(Wrap("<xsl:sequence select='() ?? 9'/>", "3.0"));
        var source = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='4.0'><xsl:" + kind + " href='module.xsl'/></xsl:stylesheet>";
        Assert.Equal(9, compiler.Compile(source, "https://example.invalid/main.xsl").Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);
    }

    /// <summary>An unversioned fallback resets the effective version to the supported draft version.</summary>
    /// <param name="attributes">Explicit fallback attributes.</param>
    /// <param name="accepted">Whether a descendant's unknown attribute is ignored.</param>
    [Theory]
    [InlineData("", false)]
    [InlineData("version='17.0'", true)]
    public void FallbackVersionReset(string attributes, bool accepted)
    {
        var body = "<xsl:future><xsl:fallback " + attributes + "><xsl:sequence bogus='ignored' select='7'/></xsl:fallback></xsl:future>";
        var error = Record.Exception(() => Compiler().Compile(Wrap(body, "17.0")).Transform(null, initialTemplate: "start", rawResult: true));
        if (accepted) Assert.Null(error);
        else Assert.Contains("XTSE0090", Assert.IsAssignableFrom<Exception>(error).ToString());
    }

    /// <summary>Future instructions execute fallback using the same ceiling as static validation.</summary>
    [Fact]
    public void FutureInstructionRuntimeFallback() => Assert.Equal(7,
        Compiler().Compile(Wrap("<xsl:future><xsl:fallback><xsl:sequence select='7'/></xsl:fallback></xsl:future>", "17.0"))
            .Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);

    /// <summary>Module validation inherits the ceiling as well as the expression grammar.</summary>
    /// <param name="kind">The module relationship.</param>
    [Theory]
    [InlineData("include")]
    [InlineData("import")]
    public void ModuleValidationInheritsProfile(string kind)
    {
        var compiler = Compiler();
        compiler.UriResolver = new ModuleResolver(Wrap("<xsl:apply-templates bogus='bad'/>", "4.0"));
        var source = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='17.0'><xsl:" + kind + " href='module.xsl'/></xsl:stylesheet>";
        Assert.Contains("XTSE0090", Assert.ThrowsAny<Exception>(() => compiler.Compile(source, "https://example.invalid/main.xsl")).ToString());
    }

    /// <summary>Used packages inherit the host profile for grammar and validation.</summary>
    /// <param name="invalid">Whether the package contains an unknown attribute at version 4.0.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UsedPackageInheritsProfile(bool invalid)
    {
        const string name = "urn:processor-profile-package";
        XsltFunctionLibrary.RegisterPackage(name, "1.0", "https://example.invalid/library.xsl");
        try
        {
            var module = "<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='urn:profile' version='" + (invalid ? "4.0" : "3.0") + "' name='" + name + "' package-version='1.0'><xsl:function name='f:probe' visibility='public'><xsl:sequence " + (invalid ? "bogus='bad' " : "") + "select='() ?? 9'/></xsl:function></xsl:package>";
            var compiler = Compiler();
            compiler.UriResolver = new ModuleResolver(module);
            var source = Wrap("<xsl:sequence xmlns:f='urn:profile' select='f:probe()'/>", "3.0", "<xsl:use-package name='" + name + "' package-version='1.0'/>");
            if (invalid) Assert.Contains("XTSE0090", Assert.ThrowsAny<Exception>(() => compiler.Compile(source)).ToString());
            else Assert.Equal(9, compiler.Compile(source).Transform(null, initialTemplate: "start", rawResult: true).IntegerValue);
        }
        finally { XsltFunctionLibrary.ClearPackages(); }
    }

    /// <summary>Known processor versions reject unknown instructions even when fallback is supplied.</summary>
    /// <param name="version">The stylesheet version.</param>
    [Theory]
    [InlineData("3.0")]
    [InlineData("4.0")]
    public void KnownVersionsRejectUnknownInstructions(string version) => Assert.Contains("XTSE0010",
        Assert.ThrowsAny<Exception>(() => Compiler().Compile(Wrap("<xsl:future><xsl:fallback/></xsl:future>", version))).ToString());

    /// <summary>Unversioned fallback attributes are validated at the processor's supported version.</summary>
    [Fact]
    public void FallbackOwnAttributes() => Assert.Contains("XTSE0090",
        Assert.ThrowsAny<Exception>(() => Compiler().Compile(Wrap("<xsl:future><xsl:fallback select='7'/></xsl:future>", "17.0"))).ToString());

    /// <summary>Literal AVTs and function bodies at version 3.0 use the selected draft grammar.</summary>
    [Fact]
    public void LiteralAndFunctionXPath()
    {
        var source = Wrap("<out value='{() ?? 8}'><xsl:value-of xmlns:f='urn:profile' select='f:probe()'/></out>", "3.0",
            "<xsl:function xmlns:f='urn:profile' name='f:probe'><xsl:sequence select='() ?? 9'/></xsl:function>");
        var result = Compiler().Compile(XDocument.Parse(source)).Transform(null, initialTemplate: "start", rawResult: true).NodeValue;
        Assert.Equal("9", result.StringValue);
        var attributes = new List<string>();
        foreach (var attribute in result.Attributes("value", "")) attributes.Add(attribute.NodeValue.StringValue);
        Assert.Equal(new[] { "8" }, attributes);
    }

    private sealed class ModuleResolver(string source) : IXsltUriResolver
    {
        /// <summary>Returns the test module.</summary>
        /// <param name="href">The requested module.</param>
        /// <param name="baseUri">The importing module URI.</param>
        /// <returns>The parsed test module.</returns>
        public XDocument Resolve(string href, string? baseUri) => XDocument.Parse(source);
    }
}
