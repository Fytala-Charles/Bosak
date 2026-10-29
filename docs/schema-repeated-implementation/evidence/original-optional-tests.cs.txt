// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Tests optional decimal particles, present-instance identity, assertions and engine copies.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : LICENSE.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation; optional decimal assessment, Schema 0.6/core 1.34.1 candidates                   |
//                      | Charles Korthout | 0.2   | 29-10-2026     | Enumerate constructor results through the unboxed sequence API                            |
//                      | Charles Korthout | 0.3   | 29-10-2026     | Add boundary refusal, required-middle and original-copy controls                          |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XQuery.Api;
using Xunit;

namespace Bosak.Schema.Tests;

/// <summary>Qualifies bounded optional particles without conflating absent instances and declaration identity.</summary>
public sealed class OptionalDecimalAssessmentTests
{
    private static XsdDocumentAssessor Compile(string particles, string assertion = "1 eq 1") =>
        SchemaServices.CreateDocumentAssessor(XDocument.Parse($"""
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"><xs:element name="order"><xs:complexType>
          <xs:sequence>{particles}</xs:sequence><xs:assert test="{assertion}"/>
        </xs:complexType></xs:element></xs:schema>
        """));
    private static string Element(string name, string minimum = "1", string maximum = "1") =>
        $"<xs:element name='{name}' type='xs:decimal' minOccurs='{minimum}' maxOccurs='{maximum}'/>";
    private static XdmValue Query(XDocument document, string expression) => XPath31Expression.Compile(expression)
        .Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(new XDocumentNode(document)), 1, 1));
    private static AssessedDocument Valid(XsdDocumentAssessor assessor, string children)
    {
        Assert.True(assessor.IsSupported, string.Join(";", assessor.Diagnostics));
        var result = assessor.Assess(XDocument.Parse("<order>" + children + "</order>"));
        Assert.Equal(SchemaAssessmentOutcome.Valid, result.Outcome);
        return Assert.IsType<AssessedDocument>(result.Document);
    }

    /// <summary>Optional neighbors do not make a middle required particle optional.</summary>
    /// <param name="prefix">Whether the first optional child exists.</param>
    /// <param name="suffix">Whether the last optional child exists.</param>
    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void RequiredMiddle_RetainsItsDeclaration(bool prefix, bool suffix)
    {
        var assessor = Compile(Element("a", "0") + Element("b") + Element("c", "0"));
        var assessed = Valid(assessor, (prefix ? "<a>1</a>" : "") + "<b>2</b>" + (suffix ? "<c>3</c>" : ""));
        Assert.Same(assessor.ChildDeclarations[1], assessed.Elements.Single(e => e.Declaration.Name == "b").Declaration);
    }

    /// <summary>Optional structure still validates a present value exactly.</summary>
    /// <param name="lexical">The present child's lexical value.</param>
    /// <param name="outcome">Expected lexical/representation outcome.</param>
    [Theory]
    [InlineData("bad", SchemaAssessmentOutcome.Invalid)]
    [InlineData("1e2", SchemaAssessmentOutcome.Invalid)]
    [InlineData("79228162514264337593543950336", SchemaAssessmentOutcome.Unsupported)]
    [InlineData("0.00000000000000000000000000001", SchemaAssessmentOutcome.Unsupported)]
    public void PresentOptional_EnforcesExactValues(string lexical, SchemaAssessmentOutcome outcome)
    {
        var result = Compile(Element("a", "0")).Assess(XDocument.Parse("<order><a>" + lexical + "</a></order>"));
        Assert.Equal(outcome, result.Outcome);
        Assert.Null(result.Document);
    }

    /// <summary>Structural failure wins before an empty optional assertion can be evaluated.</summary>
    [Fact]
    public void MissingRequired_IsStructuralFailureBeforeAssertion()
    {
        var result = Compile(Element("a", "0") + Element("b"), "a gt 0").Assess(XDocument.Parse("<order/>"));
        Assert.Equal(SchemaAssessmentOutcome.Invalid, result.Outcome);
        Assert.Equal(SchemaDiagnosticCodes.AssessmentInvalidInstance, Assert.Single(result.Diagnostics).Code);
    }

    /// <summary>The extension does not admit optional enclosing groups or wider assertion functions.</summary>
    [Fact]
    public void BroaderGroupAndAssertionCoverage_RemainsUnsupported()
    {
        var schema = XDocument.Parse(Compile(Element("a", "0")).SchemaXml);
        schema.Descendants(XName.Get("sequence", "http://www.w3.org/2001/XMLSchema")).Single().SetAttributeValue("minOccurs", "0");
        Assert.False(SchemaServices.CreateDocumentAssessor(schema).IsSupported);
        Assert.False(Compile(Element("a", "0"), "exists(a)").IsSupported);
    }

    /// <summary>Omitted occurrence attributes retain the original required-only profile.</summary>
    [Fact]
    public void DefaultOccurrence_RetainsV1AndRequiredValidation()
    {
        var assessor = Compile("<xs:element name='a' type='xs:decimal'/>");
        Assert.Equal("decimal-sequence-assertions-v1", assessor.Profile);
        Assert.Equal(SchemaAssessmentOutcome.Invalid, assessor.Assess(XDocument.Parse("<order/>")).Outcome);
        Assert.Equal(2, Valid(assessor, "<a>1</a>").Elements.Count);
    }

    /// <summary>Unsigned bounded zero spellings admit optional children; broader occurrence coverage refuses.</summary>
    /// <param name="minimum">Local minimum lexical value.</param>
    /// <param name="maximum">Local maximum lexical value.</param>
    /// <param name="supported">Expected finite-profile coverage.</param>
    [Theory]
    [InlineData("0", "1", true)]
    [InlineData("00", "01", true)]
    [InlineData(" 0 ", " 1 ", true)]
    [InlineData("1", "1", true)]
    [InlineData("01", "001", true)]
    [InlineData("+0", "1", false)]
    [InlineData("-0", "1", false)]
    [InlineData("", "1", false)]
    [InlineData("2", "1", false)]
    [InlineData("0", "0", false)]
    [InlineData("0", "2", false)]
    [InlineData("0", "unbounded", false)]
    public void OccurrenceCoverage_IsFinite(string minimum, string maximum, bool supported)
    {
        var assessor = Compile(Element("n", minimum, maximum));
        Assert.Equal(supported, assessor.IsSupported);
        if (!supported)
        {
            var result = assessor.Assess(XDocument.Parse("<order/>"));
            Assert.Equal(SchemaAssessmentOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Document);
        }
        else Assert.Equal(minimum.Trim(' ', '0').Length == 0 ? "decimal-sequence-assertions-v2" : "decimal-sequence-assertions-v1", assessor.Profile);
    }

    /// <summary>Optional first/middle/last particles do not shift the original declaration of later nodes.</summary>
    /// <param name="mask">Presence mask for three optional children.</param>
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void EveryOptionalPresenceCombination_RetainsOriginalState(int mask)
    {
        var assessor = Compile(Element("a", "0") + Element("b", "0") + Element("c", "0"));
        var children = string.Concat(Enumerable.Range(0, 3).Where(i => (mask & (1 << i)) != 0)
            .Select(i => $"<{(char)('a' + i)}>{i + 1}</{(char)('a' + i)}>"));
        var assessed = Valid(assessor, children);
        Assert.Equal(3, assessor.ChildDeclarations.Count);
        Assert.Equal(1 + System.Numerics.BitOperations.PopCount((uint)mask), assessed.Elements.Count);
        var xml = assessed.CreateDocumentCopy(true);
        foreach (var element in xml.Root!.Elements())
        {
            var declaration = assessor.ChildDeclarations.Single(d => d.Name == element.Name);
            var state = assessed.Elements.Single(e => e.Declaration.Name == element.Name);
            Assert.Same(declaration, state.Declaration);
            Assert.Same(state, XdmElementAssessment.Get(element)!.State);
            Assert.Equal((decimal)(element.Name.LocalName[0] - 'a' + 1), new XDocumentNode(element).TypedValue.DecimalValue);
            Assert.Equal(element.Value, assessed.CreateTypedElementCopy(element.Name).Value);
        }
        var copy = assessed.Copy();
        Assert.NotEqual(assessed.SnapshotIdentity, copy.SnapshotIdentity);
        Assert.Equal(assessed.AssessmentIdentity, copy.AssessmentIdentity);
        for (var i = 0; i < assessed.Elements.Count; i++) Assert.Same(assessed.Elements[i], copy.Elements[i]);
    }

    /// <summary>Only matching present nodes are consumed, with required particles checked before assertions.</summary>
    /// <param name="children">Instance child sequence.</param>
    /// <param name="valid">Whether the structure is admitted.</param>
    [Theory]
    [InlineData("<a>1</a><c>3</c>", true)]
    [InlineData("<a>1</a><b>2</b><c>3</c>", true)]
    [InlineData("<a>1</a>", false)]
    [InlineData("<b>2</b><c>3</c>", false)]
    [InlineData("<a>1</a><b>2</b><b>2</b><c>3</c>", false)]
    [InlineData("<a>1</a><a>1</a><c>3</c>", false)]
    [InlineData("<a>1</a><c>3</c><b>2</b>", false)]
    [InlineData("<c>3</c><a>1</a>", false)]
    [InlineData("<unknown>0</unknown><a>1</a><c>3</c>", false)]
    [InlineData("<a>1</a><c>3</c><unknown>0</unknown>", false)]
    public void OrderedMatching_DoesNotSeekAhead(string children, bool valid)
    {
        var assessor = Compile(Element("a") + Element("b", "0") + Element("c"));
        Assert.True(assessor.IsSupported);
        var result = assessor.Assess(XDocument.Parse("<order>" + children + "</order>"));
        Assert.Equal(valid ? SchemaAssessmentOutcome.Valid : SchemaAssessmentOutcome.Invalid, result.Outcome);
        if (!valid) Assert.Null(result.Document);
        else Assert.Same(assessor.ChildDeclarations[2], result.Document!.Elements.Last().Declaration);
    }

    /// <summary>An empty comparison is a false assertion, not an unsupported Boolean payload.</summary>
    /// <param name="assertion">An admitted comparison/logical expression.</param>
    /// <param name="valid">Expected assertion effective Boolean value.</param>
    [Theory]
    [InlineData("b eq a", false)] [InlineData("b ne a", false)]
    [InlineData("b lt a", false)] [InlineData("b le a", false)]
    [InlineData("b gt a", false)] [InlineData("b ge a", false)]
    [InlineData("b eq b", false)]
    [InlineData("b le a or a gt 0", true)]
    [InlineData("b le a and a gt 0", false)]
    public void AbsentComparison_UsesEffectiveBooleanValue(string assertion, bool valid)
    {
        var assessor = Compile(Element("a") + Element("b", "0"), assertion);
        Assert.True(assessor.IsSupported);
        var result = assessor.Assess(XDocument.Parse("<order><a>1</a></order>"));
        Assert.Equal(valid ? SchemaAssessmentOutcome.Valid : SchemaAssessmentOutcome.Invalid, result.Outcome);
        if (!valid)
        {
            Assert.Null(result.Document);
            Assert.Equal(SchemaDiagnosticCodes.AssertionViolated, Assert.Single(result.Diagnostics).Code);
        }
    }

    /// <summary>Absent projections refuse, while the later required child keeps its own decimal and declaration.</summary>
    [Fact]
    public void MissingProjection_RefusesWithoutShiftingLaterValue()
    {
        var assessed = Valid(Compile(Element("a") + Element("b", "0") + Element("c")), "<a>1</a><c>3</c>");
        Assert.Throws<NotSupportedException>(() => assessed.CreateTypedElementCopy("b"));
        Assert.Equal("3", assessed.CreateTypedElementCopy("c").Value);
        Assert.Equal(3m, Query(assessed.CreateDocumentCopy(true), "data(/order/c)").DecimalValue);
        Assert.True(Query(assessed.CreateDocumentCopy(false), "data(/order/c) instance of xs:untypedAtomic").BooleanValue);
    }

    /// <summary>All-absent element-only roots keep original state across ignorable whitespace removal.</summary>
    [Fact]
    public void EmptyRoot_PreservesWhitespaceSemanticsAndAtomizationError()
    {
        var assessed = Valid(Compile(Element("a", "0")), "  ");
        var xml = assessed.CreateDocumentCopy(true);
        xml.Root!.RemoveNodes();
        Assert.Same(assessed, XdmElementAssessment.Get(xml.Root)!.Owner);
        Assert.True(new XDocumentNode(xml.Root).HasNoTypedValue);
        Assert.Contains("FOTY0012", Assert.ThrowsAny<Exception>(() => Query(xml, "data(/order)")).ToString());
        xml.Root.Value = "changed";
        Assert.Throws<NotSupportedException>(() => XdmElementAssessment.Get(xml.Root));
        Assert.Single(assessed.Elements);
    }

    /// <summary>Actual XSLT copy paths retain optional-gap decimals and strip annotations, including empty roots.</summary>
    /// <param name="empty">Whether every optional child is absent.</param>
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void XsltCopies_HandleGapsAndEmptyRoots(bool empty)
    {
        var assessed = Valid(Compile(Element("a", "0") + Element("b", "0")), empty ? "  " : "<b>3</b>");
        const string style = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:err="http://www.w3.org/2005/xqt-errors">
          <xsl:template name="main">
            <xsl:variable name="p" as="element()"><xsl:copy-of select="/order" validation="preserve"/></xsl:variable>
            <xsl:variable name="s" as="element()"><xsl:copy-of select="/order" validation="strip"/></xsl:variable>
            <result typed="{every $n in $p/* satisfies data($n) instance of xs:decimal}" stripped="{$s instance of element(*,xs:untyped)}" count="{count($p/*)}">
              <xsl:try><xsl:value-of select="$p"/><xsl:catch><error code="{$err:code}"/></xsl:catch></xsl:try>
            </result>
          </xsl:template>
        </xsl:stylesheet>
        """;
        var compiler = SchemaAwareXslt.CreateCompiler();
        compiler.SchemaSet = new XmlSchemaSet();
        var result = XElement.Parse(compiler.Compile(style).TransformToString(new XDocumentNode(assessed.CreateDocumentCopy(true)), initialTemplate: "main"));
        Assert.Equal("true", (string?)result.Attribute("typed"));
        Assert.Equal("true", (string?)result.Attribute("stripped"));
        Assert.Equal(empty ? "0" : "1", (string?)result.Attribute("count"));
        Assert.Contains("FOTY0012", (string?)result.Element("error")!.Attribute("code"));
    }

    /// <summary>XQuery constructors preserve original gap-state references or strip them on private copies.</summary>
    /// <param name="preserve">Requested construction mode.</param>
    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void XQueryCopies_RetainPresentStateOnly(bool preserve)
    {
        var assessed = Valid(Compile(Element("a", "0") + Element("b", "0")), "<b>3</b>");
        var xml = assessed.CreateDocumentCopy(true);
        var text = "declare construction " + (preserve ? "preserve" : "strip") + "; declare variable $input external; <copy>{$input}</copy>/*";
        var value = new XQueryCompiler().Compile(text).Evaluate(new XQueryContext().WithVariable("input", XdmValue.FromNode(new XDocumentNode(xml.Root!))));
        IXdmNode? node = value.IsNode ? value.NodeValue : null;
        if (node is null)
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) { node = item.NodeValue; break; }
        var result = Assert.IsType<XDocumentNode>(node);
        Assert.Equal(preserve, result.HasNoTypedValue);
        if (preserve) Assert.Same(assessed.Elements[0], result.ElementAssessment!.State);
        else Assert.Null(result.ElementAssessment);
        var copiedChild = new XDocumentNode(((XElement)result.UnderlyingObject).Element("b")!);
        if (preserve) Assert.Same(assessed.Elements[1], copiedChild.ElementAssessment!.State);
        else Assert.Null(copiedChild.ElementAssessment);
        Assert.Equal(3m, new XDocumentNode(xml.Root!.Element("b")!).TypedValue.DecimalValue);
        xml.Root.Element("b")!.Value = "9";
        Assert.Throws<NotSupportedException>(() => new XDocumentNode(xml.Root.Element("b")!).TypedValue);
        Assert.Equal(3m, Query(assessed.CreateDocumentCopy(true), "data(/order/b)").DecimalValue);
    }
}
