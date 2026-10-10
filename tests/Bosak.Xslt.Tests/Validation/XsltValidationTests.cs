// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Contract tests for REQ-125 Slice A: static validation outcomes (AC-01/AC-02/AC-09).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice A)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.XPath.Api;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

public class XsltValidationTests
{
    private static readonly Uri MainUri = new("file:///val/main.xsl");
    private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

    private sealed class MemoryResolver : IAuthoringModuleResolver
    {
        private readonly Dictionary<string, byte[]> _modules = new(StringComparer.OrdinalIgnoreCase);

        public List<(Uri AbsoluteUri, Uri Referencing)> Requests { get; } = new();

        public void Add(string absoluteUri, string text) => _modules[absoluteUri] = Utf8.GetBytes(text);

        public AuthoringSource? ResolveModule(Uri absoluteUri, Uri referencingModuleUri)
        {
            Requests.Add((absoluteUri, referencingModuleUri));
            return _modules.TryGetValue(absoluteUri.AbsoluteUri, out var bytes) &&
                   AuthoringSource.TryCreate(bytes, absoluteUri, out var source, out _)
                ? source
                : null;
        }
    }

    private static XsltValidationResult Validate(string principal, IAuthoringModuleResolver? resolver = null, XsltValidationOptions? options = null)
    {
        var result = XsltValidation.Validate(Utf8.GetBytes(principal), MainUri, resolver, options);
        Assert.NotNull(result);
        return result;
    }

    private static AuthoringSnapshot Inspect(string principal, IAuthoringModuleResolver? resolver = null)
    {
        Assert.True(AuthoringSource.TryCreate(Utf8.GetBytes(principal), MainUri, out var source, out var failure), failure?.ToString());
        var inspection = XsltAuthoring.Inspect(source!, resolver, new AuthoringInspectionOptions { AttemptCompilation = false });
        Assert.True(inspection.IsSuccess, inspection.Failure?.ToString());
        return inspection.Snapshot!;
    }

    private const string ValidFixture = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:e="urn:example">
  <xsl:param name="global-p" select="1"/>
  <xsl:variable name="global-v" select="$global-p + 1"/>
  <xsl:template match="/">
    <out a="{$global-v}" b="literal-{{brace}}">
      <xsl:value-of select="e:root/child"/>
    </out>
  </xsl:template>
  <xsl:template match="e:item" mode="m" use-when="false()">
    <xsl:value-of select="$global-v"/>
  </xsl:template>
  <xsl:template name="named">
    <xsl:param name="p" select="2"/>
    <xsl:variable name="local" select="$p * 2"/>
    <xsl:if test="$local gt 2">
      <xsl:copy-of select="."/>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
""";

    // ---------------------------------------------------------------------------------------------
    // AC-01: malformed XPath in an unused template reports a static failure.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Ac01_MalformedXPathInUnusedTemplate_IsReported()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/"><ok/></xsl:template>
  <xsl:template name="never-called">
    <xsl:value-of select="("/>
  </xsl:template>
</xsl:stylesheet>
""";
        var snapshot = Inspect(stylesheet);
        // Baseline evidence for the dossier: the default compilation path accepts this stylesheet
        // because the malformed expression sits in a template that is never compiled.
        var compiled = new Api.XsltCompiler().Compile(stylesheet, MainUri.AbsoluteUri);
        Assert.NotNull(compiled);

        var result = XsltValidation.Validate(snapshot);

        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XPST0003", diagnostic.Code);
        Assert.Equal(MainUri, diagnostic.ModuleUri);
        Assert.True(diagnostic.LocationAvailable, "A source range must be available.");
        var select = FindAttribute(snapshot, "value-of", "select");
        Assert.InRange(diagnostic.Range!.StartByteOffset, select.ValueRange.StartByteOffset, select.ValueRange.StartByteOffset + select.ValueRange.ByteLength);
    }

    [Fact]
    public void Ac01_ValidMultiTemplateFixture_IsValid()
    {
        var result = Validate(ValidFixture);
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.Empty(result.Diagnostics);
        Assert.Empty(result.UnsupportedCoverage);
    }

    [Fact]
    public void Ac01_DeclaredCoverage_IsPinned()
    {
        var result = Validate(ValidFixture);
        Assert.Equal(
            new[]
            {
                XsltValidationCoverage.ExpressionSlots,
                XsltValidationCoverage.PatternSlots,
                XsltValidationCoverage.AvtSlots,
                XsltValidationCoverage.StaticVariableScope,
            },
            result.DeclaredCoverage);
    }

    // ---------------------------------------------------------------------------------------------
    // Static context: in-scope variables and namespaces.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void StaticContext_InScopeVariablesValidate_Ok()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="g" select="10"/>
  <xsl:template match="/">
    <xsl:variable name="local" select="$g + 1"/>
    <xsl:value-of select="$local + count(*)"/>
  </xsl:template>
  <xsl:template name="with-params">
    <xsl:param name="p"/>
    <xsl:value-of select="$p + $g"/>
  </xsl:template>
</xsl:stylesheet>
""";
        Assert.Equal(XsltValidationOutcome.Valid, Validate(stylesheet).Outcome);
    }

    [Fact]
    public void StaticContext_UndeclaredVariable_IsReported()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <xsl:value-of select="$missing + 1"/>
  </xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XPST0008", diagnostic.Code);
        Assert.Contains("$missing", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StaticContext_VariableBoundInsideExpression_IsNotReported()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <xsl:value-of select="let $x := 1 return for $y in (1,2) return $x + $y"/>
  </xsl:template>
</xsl:stylesheet>
""";
        Assert.Equal(XsltValidationOutcome.Valid, Validate(stylesheet).Outcome);
    }

    [Fact]
    public void StaticContext_DeclaredPrefixOk_UndeclaredPrefixReported()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:e="urn:example">
  <xsl:template match="/">
    <xsl:value-of select="e:root"/>
    <xsl:value-of select="gone:root"/>
  </xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XPST0081", diagnostic.Code);
        Assert.Contains("gone", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StaticContext_VariableNotVisibleBeforeDeclaration_IsReported()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <xsl:value-of select="$later"/>
    <xsl:variable name="later" select="1"/>
  </xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0008");
    }

    [Fact]
    public void StaticContext_VariableNotVisibleInsideFunction_IsReported()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="outer" select="1"/>
  <xsl:template match="/">
    <xsl:variable name="local" select="2"/>
    <xsl:value-of select="f:test() + $local"/>
  </xsl:template>
  <xsl:function name="f:test" xmlns:f="urn:f">
    <xsl:param name="a"/>
    <xsl:sequence select="$outer + $a"/>
    <xsl:sequence select="$local"/>
  </xsl:function>
</xsl:stylesheet>
""";
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        // $outer (global) and $a (function param) are fine; the template-local $local is not
        // visible inside the function body.
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XPST0008", diagnostic.Code);
        Assert.Contains("$local", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StaticContext_UseWhenSeesOnlyStaticGlobals()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="s" static="yes" select="1"/>
  <xsl:variable name="dynamic" select="2"/>
  <xsl:template match="/" use-when="$s = 1"><ok/></xsl:template>
  <xsl:template match="/" use-when="$dynamic = 2"><bad/></xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XPST0008", diagnostic.Code);
        Assert.Contains("$dynamic", diagnostic.Message, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------------------------------------
    // Pattern and AVT coverage (declared checked set).
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Coverage_BadMatchPattern_IsReported()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/"><ok/></xsl:template>
  <xsl:template match="make()"><bad/></xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XTSE0340", diagnostic.Code);
        Assert.True(diagnostic.LocationAvailable);
    }

    [Fact]
    public void Coverage_BadAvtExpression_IsReported()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <out name="pre{(}"/>
  </xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XPST0003", diagnostic.Code);
    }

    [Fact]
    public void Coverage_UnmatchedAvtBrace_IsReported()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <xsl:element name="a{b"/>
  </xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XTSE0350", diagnostic.Code);
    }

    [Fact]
    public void Coverage_ValidAvtWithEscapes_IsValid()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <out name="a-{{literal}}-{1 + 1}-b" other="plain"/>
  </xsl:template>
</xsl:stylesheet>
""";
        Assert.Equal(XsltValidationOutcome.Valid, Validate(stylesheet).Outcome);
    }

    [Fact]
    public void Coverage_LiteralFalseUseWhenSubtree_IsNotAnalyzed()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/"><ok/></xsl:template>
  <xsl:template match="/" use-when="false()">
    <xsl:value-of select="("/>
  </xsl:template>
</xsl:stylesheet>
""";
        Assert.Equal(XsltValidationOutcome.Valid, Validate(stylesheet).Outcome);
    }

    [Fact]
    public void Coverage_BadGroupingPattern_IsReported()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <xsl:for-each-group select="*" group-starting-with="make()">
      <xsl:copy-of select="."/>
    </xsl:for-each-group>
  </xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XTSE0340", diagnostic.Code);
    }

    // ---------------------------------------------------------------------------------------------
    // AC-02: bytes and snapshots unchanged by validation.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Ac02_ModuleBytes_AreUnchangedByValidation()
    {
        const string includeText = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="inc"><inc/></xsl:template>
</xsl:stylesheet>
""";
        var resolver = new MemoryResolver();
        resolver.Add("file:///val/dep.xsl", includeText);
        const string principalText = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="dep.xsl"/>
  <xsl:template match="/"><ok/></xsl:template>
</xsl:stylesheet>
""";
        var snapshot = Inspect(principalText, resolver);
        var principalBefore = snapshot.ExportOriginal(MainUri);
        var includeBefore = snapshot.ExportOriginal(new Uri("file:///val/dep.xsl"));
        var moduleCountBefore = snapshot.Modules.Count;
        var versionBefore = snapshot.PrincipalModule.Version;

        var result = XsltValidation.Validate(snapshot);

        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.True(principalBefore.SequenceEqual(snapshot.ExportOriginal(MainUri)), "Principal bytes must be unchanged.");
        Assert.True(includeBefore.SequenceEqual(snapshot.ExportOriginal(new Uri("file:///val/dep.xsl"))), "Dependency bytes must be unchanged.");
        Assert.Equal(moduleCountBefore, snapshot.Modules.Count);
        Assert.Equal(versionBefore, snapshot.PrincipalModule.Version);
        Assert.Equal(new Uri("file:///val/dep.xsl"), snapshot.Modules[1].ModuleUri);
    }

    [Fact]
    public void Ac02_ByteOverload_DefensivelyCopies_AndPreservesBytes()
    {
        var bytes = Utf8.GetBytes(ValidFixture);
        var result = XsltValidation.Validate(bytes, MainUri);
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.True(bytes.SequenceEqual(Utf8.GetBytes(ValidFixture)), "Caller bytes must not be consumed or mutated.");
    }

    // ---------------------------------------------------------------------------------------------
    // Cross-module diagnostics.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void CrossModule_MalformedSelectInInclude_IdentifiesIncludeModule()
    {
        var includeUri = new Uri("file:///val/broken.xsl");
        var resolver = new MemoryResolver();
        resolver.Add(includeUri.AbsoluteUri, """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="broken"><xsl:value-of select="("/></xsl:template>
</xsl:stylesheet>
""");
        const string principal = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="broken.xsl"/>
  <xsl:template match="/"><ok/></xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(principal, resolver);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XPST0003", diagnostic.Code);
        Assert.Equal(includeUri, diagnostic.ModuleUri);
    }

    [Fact]
    public void CrossModule_GlobalVariableFromInclude_IsVisibleInPrincipal()
    {
        var resolver = new MemoryResolver();
        resolver.Add("file:///val/vars.xsl", """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="shared" select="42"/>
</xsl:stylesheet>
""");
        const string principal = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="vars.xsl"/>
  <xsl:template match="/"><xsl:value-of select="$shared"/></xsl:template>
</xsl:stylesheet>
""";
        Assert.Equal(XsltValidationOutcome.Valid, Validate(principal, resolver).Outcome);
    }

    // ---------------------------------------------------------------------------------------------
    // Outcome taxonomy.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Outcomes_ResolverRefusal_IsRefused()
    {
        var resolver = new MemoryResolver(); // knows nothing
        const string principal = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="missing.xsl"/>
  <xsl:template match="/"><ok/></xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(principal, resolver);
        Assert.Equal(XsltValidationOutcome.Refused, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XV0002", diagnostic.Code);
        Assert.Equal(MainUri, diagnostic.ModuleUri);
        Assert.True(diagnostic.LocationAvailable, "The include element range must be available.");
    }

    [Fact]
    public void Outcomes_MalformedPrincipalXml_IsInvalidSource()
    {
        var result = XsltValidation.Validate(Utf8.GetBytes("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>"), MainUri);
        Assert.Equal(XsltValidationOutcome.InvalidSource, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XV0001", diagnostic.Code);
        Assert.Equal(MainUri, diagnostic.ModuleUri);
    }

    [Fact]
    public void Outcomes_EmptyPrincipal_IsInvalidSource()
    {
        var result = XsltValidation.Validate(Array.Empty<byte>(), MainUri);
        Assert.Equal(XsltValidationOutcome.InvalidSource, result.Outcome);
        Assert.Single(result.Diagnostics);
    }

    [Fact]
    public void Outcomes_NonStylesheetRoot_IsInvalid()
    {
        var result = Validate("<html><body>plain document</body></html>");
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XTSE0010", diagnostic.Code);
    }

    [Fact]
    public void Outcomes_DeferredCoverage_IsExplicitlyReported()
    {
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import-schema namespace="urn:example"/>
  <xsl:template match="/"><ok/></xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.UnsupportedCoverage, result.Outcome);
        Assert.Empty(result.Diagnostics);
        var gap = Assert.Single(result.UnsupportedCoverage);
        Assert.Equal("xsl:import-schema", gap.Construct);
        Assert.Equal(MainUri, gap.ModuleUri);
    }

    [Fact]
    public void Outcomes_InvalidTakesPrecedenceOverRefusal()
    {
        var resolver = new MemoryResolver();
        const string principal = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="missing.xsl"/>
  <xsl:template match="/"><xsl:value-of select="("/></xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(principal, resolver);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0003");
        Assert.Contains(result.Diagnostics, d => d.Code == "XV0002");
    }

    // ---------------------------------------------------------------------------------------------
    // Version / profile handling.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Version_Effective40_Allows40OnlyFunction()
    {
        const string stylesheet = """
<xsl:stylesheet version="4.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/"><xsl:sequence select="fn:replicate(1, 2)"/></xsl:template>
</xsl:stylesheet>
""";
        Assert.Equal(XsltValidationOutcome.Valid, Validate(stylesheet).Outcome);

        var forced31 = Validate(stylesheet, options: new XsltValidationOptions { CompatibilityOverride = XPathCompatibility.XPath31 });
        Assert.Equal(XsltValidationOutcome.Invalid, forced31.Outcome);
        var diagnostic = Assert.Single(forced31.Diagnostics);
        Assert.Equal("XPST0017", diagnostic.Code);
    }

    // ---------------------------------------------------------------------------------------------
    // No side effects: everything resolves through the supplied resolver, never the disk.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void SideEffects_NoDiskAccess_WithMemoryResolver()
    {
        // The include target does NOT exist on disk under file:///val/; only the memory resolver
        // can supply it. A green result proves validation read nothing from the file system.
        var resolver = new MemoryResolver();
        resolver.Add("file:///val/only-in-memory.xsl", """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="mem"><m/></xsl:template>
</xsl:stylesheet>
""");
        const string principal = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:include href="only-in-memory.xsl"/>
  <xsl:template match="/"><ok/></xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(principal, resolver);
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.Single(resolver.Requests);
    }

    [Fact]
    public void SideEffects_ValidationNeverExecutes_Templates()
    {
        // A template whose expression is valid but would fail or write at run time must not run:
        // validation performs no transformation and no result-document writes.
        const string stylesheet = """
<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <xsl:result-document href="file:///would-write.xml">
      <xsl:value-of select="error(QName('urn:x', 'must-not-fire'))"/>
    </xsl:result-document>
  </xsl:template>
</xsl:stylesheet>
""";
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
    }

    // ---------------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------------

    private static AuthoringAttributeDescriptor FindAttribute(AuthoringSnapshot snapshot, string elementLocalName, string attributeName)
    {
        var element = snapshot.PrincipalModule.Root.FindDescendant(
            n => n.Kind == AuthoringNodeKind.Element && n.ElementName?.LocalName == elementLocalName);
        Assert.NotNull(element);
        return Assert.Single(element!.Attributes, a => a.Name == attributeName);
    }
}
