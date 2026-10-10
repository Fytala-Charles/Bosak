// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Regression tests for the REQ-125 Slice A consumer-review findings F1-F4.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation: review §4 regression evidence for findings F1-F4                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using Bosak.Xslt.Authoring;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Validation;

/// <summary>
/// Regression evidence for the confirmed consumer-review findings in
/// <c>docs/REQ-125-SLICE-A-CONSUMER-REVIEW.md</c> §4. Test names carry the finding id.
/// </summary>
public class XsltValidationReviewFindingTests
{
    private static readonly Uri MainUri = new("file:///review/main.xsl");
    private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

    private static XsltValidationResult Validate(string principal, IAuthoringModuleResolver? resolver = null)
    {
        var result = XsltValidation.Validate(Utf8.GetBytes(principal), MainUri, resolver);
        Assert.NotNull(result);
        return result;
    }

    private const string WrapperStart = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:e="urn:example" xmlns:fn="http://www.w3.org/2005/xpath-functions">
        """;

    private static string WrapBody(string body) => WrapperStart + body + "\n</xsl:stylesheet>\n";

    // ---------------------------------------------------------------------------------------------
    // F1: unknown instructions and representative engine-reported static XSLT errors cannot
    //     return Valid. Coverage/exclusions are documented on XsltValidation (see remarks there)
    //     and pinned through XsltValidationCoverage.StructuralChecks.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void F1_UnknownInstructionInTemplate_IsNotValid()
    {
        const string stylesheet = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/"><xsl:unknown-instruction/></xsl:template>
        </xsl:stylesheet>
        """;
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XTSE0010", diagnostic.Code);
        Assert.Contains("unknown-instruction", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void F1_UnknownInstructionNestedInChoose_IsNotValid()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:choose>
              <xsl:when test="true()"><xsl:also-unknown/></xsl:when>
            </xsl:choose>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0010");
    }

    [Fact]
    public void F1_UnknownTopLevelElementIn30Stylesheet_IsToleratedAsVendorExtension()
    {
        var result = Validate("""
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:vendor-extension/>
          <xsl:template match="/"><ok/></xsl:template>
        </xsl:stylesheet>
        """);
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void F1_UnknownTopLevelElementIn20Stylesheet_IsNotValid()
    {
        var result = Validate("""
        <xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:vendor-extension/>
          <xsl:template match="/"><ok/></xsl:template>
        </xsl:stylesheet>
        """);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0010");
    }

    [Fact]
    public void F1_UnknownElementInForwardsCompatibleStylesheet_IsTolerated()
    {
        // Effective version above the supported 3.0 ceiling: unknown elements are ignored,
        // mirroring the compiler's forwards-compatible behavior.
        var result = Validate("""
        <xsl:stylesheet version="9.9" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/"><xsl:future-instruction/></xsl:template>
        </xsl:stylesheet>
        """);
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
    }

    [Fact]
    public void F1_IfWithoutTest_IsNotValid()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:if><ok/></xsl:if>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0010" && d.Message.Contains("test", StringComparison.Ordinal));
    }

    [Fact]
    public void F1_MustBeEmptyElementWithContent_IsNotValid()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:copy-of select="."><oops/></xsl:copy-of>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0260");
    }

    [Fact]
    public void F1_StaticVariableInsideTemplate_IsNotValid()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:variable name="local" static="yes" select="1"/>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0090");
    }

    [Fact]
    public void F1_TopLevelOnlyDeclarationBelowTopLevel_IsNotValid()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:key name="k" match="e:item" use="@id"/>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0010" && d.Message.Contains("top level", StringComparison.Ordinal));
    }

    [Fact]
    public void F1_InstructionPlacedAtTopLevel_IsNotValid()
    {
        var result = Validate(WrapBody("""
          <xsl:when test="true()"/>
          <xsl:template match="/"><ok/></xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0010");
    }

    [Fact]
    public void F1_OnCompletionOutsideIterate_IsNotValid()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:for-each select="*">
              <xsl:on-completion><done/></xsl:on-completion>
              <xsl:copy-of select="."/>
            </xsl:for-each>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0010" && d.Message.Contains("on-completion", StringComparison.Ordinal));
    }

    [Fact]
    public void F1_XsltNamespacedAttributeOnXsltElement_IsNotValid()
    {
        var result = Validate("""
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/" xsl:mystery="yes"><ok/></xsl:template>
        </xsl:stylesheet>
        """);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0090");
    }

    [Fact]
    public void F1_UndefinedXsltAttributeOnLiteralResultElement_IsNotValid()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <out xsl:mystery="yes"/>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XTSE0805");
    }

    [Fact]
    public void F1_DeclaredCoverage_ListsStructuralChecks()
    {
        var result = Validate(WrapBody("""<xsl:template match="/"><ok/></xsl:template>"""));
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.Contains(XsltValidationCoverage.StructuralChecks, result.DeclaredCoverage);
    }

    // ---------------------------------------------------------------------------------------------
    // F2: declared/undeclared prefixes in match and grouping patterns are validated in the
    //     module-local static context, including pattern expressions referencing variables and
    //     functions where supported.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void F2_UndeclaredPrefixInMatchPattern_IsReported()
    {
        const string stylesheet = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/"><ok/></xsl:template>
          <xsl:template match="missing:item"><bad/></xsl:template>
        </xsl:stylesheet>
        """;
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XPST0081", diagnostic.Code);
        Assert.Contains("missing", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void F2_DeclaredPrefixInMatchPattern_IsValid()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/"><ok/></xsl:template>
          <xsl:template match="e:item"><ok/></xsl:template>
          <xsl:template match="e:item/e:child | e:other"><ok/></xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void F2_UndeclaredPrefixInGroupingStartingEndingWith_IsReported()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:for-each-group select="*" group-starting-with="missing:a">
              <xsl:copy-of select="."/>
            </xsl:for-each-group>
            <xsl:for-each-group select="*" group-ending-with="missing:b">
              <xsl:copy-of select="."/>
            </xsl:for-each-group>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.All(result.Diagnostics, d => Assert.Equal("XPST0081", d.Code));
        Assert.Equal(2, result.Diagnostics.Count);
    }

    [Fact]
    public void F2_UndeclaredPrefixInGroupByAndCountAndFrom_IsReported()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:for-each-group select="*" group-by="missing:kind">
              <xsl:copy-of select="."/>
            </xsl:for-each-group>
            <xsl:number count="missing:c" from="missing:d"/>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.All(result.Diagnostics, d => Assert.Equal("XPST0081", d.Code));
    }

    [Fact]
    public void F2_DeclaredPrefixesInAllGroupingAndNumberAttributes_AreValid()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:for-each-group select="*" group-by="e:kind" group-adjacent="e:kind">
              <xsl:copy-of select="."/>
            </xsl:for-each-group>
            <xsl:for-each-group select="*" group-starting-with="e:head" group-ending-with="e:tail">
              <xsl:copy-of select="."/>
            </xsl:for-each-group>
            <xsl:number count="e:row" from="e:table"/>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void F2_UndeclaredPrefixInPatternPredicate_IsReported()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="e:item[@missing:id]"><ok/></xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0081");
    }

    [Fact]
    public void F2_PatternPredicateWithDeclaredFunctionAndVariable_IsValid()
    {
        // Function calls and variable references inside pattern predicates are compiled in the
        // slot's static context; pattern variable references are not scope-checked (documented
        // exclusion), so a declared prefix with any syntactically valid predicate compiles.
        var result = Validate(WrapBody("""
          <xsl:variable name="v" select="1"/>
          <xsl:template match="e:item[fn:string-length(e:name) gt 0][$v]"><ok/></xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.Empty(result.Diagnostics);
    }

    // ---------------------------------------------------------------------------------------------
    // F3: excluded-element own attributes and descendants are skipped; malformed exclusion
    //     expressions fail; globals excluded by use-when declare nothing; non-literal exclusions
    //     degrade to UnsupportedCoverage instead of a complete pass.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void F3_ExcludedElementOwnMalformedAttributes_AreSkipped()
    {
        // The exact review reproducer: the excluded element's own unparseable select must not
        // be analyzed because the whole element is removed by XSLT 3.0 §3.13.
        const string stylesheet = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template match="/">
            <xsl:sequence use-when="false()" select="("/>
          </xsl:template>
        </xsl:stylesheet>
        """;
        Assert.Equal(XsltValidationOutcome.Valid, Validate(stylesheet).Outcome);
    }

    [Fact]
    public void F3_ExcludedElementDescendants_AreSkipped()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:if use-when="false()" test="(">
              <xsl:value-of select="("/>
            </xsl:if>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void F3_MalformedUseWhenExpression_Fails()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:sequence use-when="(" select="1"/>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0003");
        // The element's own slots are not analyzed beyond the malformed exclusion.
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == "XV0003");
    }

    [Fact]
    public void F3_GlobalExcludedByUseWhen_IsNotAnalyzed()
    {
        var result = Validate(WrapBody("""
          <xsl:variable name="dropped" use-when="false()" select="("/>
          <xsl:template match="/"><ok/></xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void F3_ReferenceToExcludedGlobal_IsNotDeclared()
    {
        var result = Validate(WrapBody("""
          <xsl:variable name="dropped" use-when="false()" select="1"/>
          <xsl:template match="/"><xsl:value-of select="$dropped"/></xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0008" && d.Message.Contains("dropped", StringComparison.Ordinal));
    }

    [Fact]
    public void F3_NonLiteralUseWhenOnElement_DegradesToUnsupportedCoverage()
    {
        var result = Validate(WrapBody("""
          <xsl:variable name="s" static="yes" select="true()"/>
          <xsl:template match="/" use-when="$s"><ok/></xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.UnsupportedCoverage, result.Outcome);
        Assert.Empty(result.Diagnostics);
        var gap = Assert.Single(result.UnsupportedCoverage);
        Assert.Equal("use-when", gap.Construct);
    }

    [Fact]
    public void F3_NonLiteralUseWhenOnGlobal_DegradesToUnsupportedCoverage()
    {
        var result = Validate(WrapBody("""
          <xsl:variable name="s" static="yes" select="true()"/>
          <xsl:variable name="maybe" use-when="$s" select="1"/>
          <xsl:template match="/"><ok/></xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.UnsupportedCoverage, result.Outcome);
        Assert.Empty(result.Diagnostics);
        Assert.Contains(result.UnsupportedCoverage, g => g.Construct == "use-when");
    }

    [Fact]
    public void F3_LiteralTrueUseWhen_ContinuesToBeChecked()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/" use-when="true()">
            <xsl:value-of select="("/>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0003");
    }

    // ---------------------------------------------------------------------------------------------
    // F4: parameter defaults use declaration-order scope — later/self references fail, earlier
    //     references pass, body references retain correct visibility; template, function and
    //     iterate rules covered independently, including shadowing.
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void F4_Template_LaterParameterReferenceInDefault_Fails()
    {
        // The exact review reproducer.
        const string stylesheet = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:template name="t">
            <xsl:param name="a" select="$b"/>
            <xsl:param name="b" select="1"/>
            <xsl:sequence select="$a"/>
          </xsl:template>
        </xsl:stylesheet>
        """;
        var result = Validate(stylesheet);
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("XPST0008", diagnostic.Code);
        Assert.Contains("$b", diagnostic.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void F4_Template_SelfParameterReferenceInDefault_Fails()
    {
        var result = Validate(WrapBody("""
          <xsl:template name="t">
            <xsl:param name="a" select="$a"/>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0008" && d.Message.Contains("$a", StringComparison.Ordinal));
    }

    [Fact]
    public void F4_Template_EarlierParameterReferenceInDefault_Passes()
    {
        var result = Validate(WrapBody("""
          <xsl:template name="t">
            <xsl:param name="a" select="1"/>
            <xsl:param name="b" select="$a + 1"/>
            <xsl:sequence select="$b"/>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void F4_Template_BodySeesAllParameters_RegardlessOfOrder()
    {
        var result = Validate(WrapBody("""
          <xsl:template name="t">
            <xsl:param name="a" select="1"/>
            <xsl:param name="b" select="2"/>
            <xsl:sequence select="$a + $b"/>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
    }

    [Fact]
    public void F4_Template_VariableShadowingParameter_KeepsEarlierReferenceValid()
    {
        var result = Validate(WrapBody("""
          <xsl:template name="t">
            <xsl:param name="a" select="1"/>
            <xsl:variable name="a" select="$a + 1"/>
            <xsl:sequence select="$a"/>
          </xsl:template>
        """));
        // The variable's default legitimately references the parameter it shadows (declaration
        // order), and the body sees the variable.
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
    }

    [Fact]
    public void F4_Function_ParametersVisibleAcrossBody_Passes()
    {
        var result = Validate(WrapBody("""
          <xsl:function name="e:f">
            <xsl:param name="a"/>
            <xsl:param name="b"/>
            <xsl:sequence select="$b + $a"/>
          </xsl:function>
        """));
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
    }

    [Fact]
    public void F4_Function_TemplateLocalNotVisibleInBody_Fails()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:variable name="local" select="1"/>
            <xsl:sequence select="e:f()"/>
          </xsl:template>
          <xsl:function name="e:f">
            <xsl:param name="a"/>
            <xsl:sequence select="$a + $local"/>
          </xsl:function>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0008" && d.Message.Contains("$local", StringComparison.Ordinal));
    }

    [Fact]
    public void F4_Iterate_LaterParameterReferenceInDefault_Fails()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:iterate select="*">
              <xsl:param name="a" select="$b"/>
              <xsl:param name="b" select="1"/>
              <xsl:copy-of select="."/>
            </xsl:iterate>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.Contains(result.Diagnostics, d => d.Code == "XPST0008" && d.Message.Contains("$b", StringComparison.Ordinal));
    }

    [Fact]
    public void F4_Iterate_EarlierParameterReferenceInDefault_Passes()
    {
        var result = Validate(WrapBody("""
          <xsl:template match="/">
            <xsl:iterate select="*">
              <xsl:param name="a" select="1"/>
              <xsl:param name="b" select="$a + 1"/>
              <xsl:copy-of select="."/>
              <xsl:on-completion><done/></xsl:on-completion>
            </xsl:iterate>
          </xsl:template>
        """));
        Assert.Equal(XsltValidationOutcome.Valid, result.Outcome);
    }
}
