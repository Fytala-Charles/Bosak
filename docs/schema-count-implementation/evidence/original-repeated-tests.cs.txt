// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Tests finite repeated decimal particles, shared declarations, cardinality errors and copies.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : LICENSE.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation; finite repeated decimal Schema 0.7 candidate                                   |
//                      | Charles Korthout | 0.2   | 29-10-2026     | Add strict error-contract, bounds, empty-root and lifetime controls                       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XQuery.Api;
using Xunit;

namespace Bosak.Schema.Tests;

/// <summary>Qualifies bounded repetition without conflating occurrence nodes and governing declarations.</summary>
public sealed class RepeatedDecimalAssessmentTests
{
    private static string Element(string name, string minimum = "1", string maximum = "1") =>
        $"<xs:element name='{name}' type='xs:decimal' minOccurs='{minimum}' maxOccurs='{maximum}'/>";
    private static XsdDocumentAssessor Compile(string particles, string assertion = "1 eq 1") =>
        SchemaServices.CreateDocumentAssessor(XDocument.Parse($"""
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"><xs:element name="order"><xs:complexType>
          <xs:sequence>{particles}</xs:sequence><xs:assert test="{assertion}"/>
        </xs:complexType></xs:element></xs:schema>
        """));
    private static SchemaDocumentAssessment Assess(XsdDocumentAssessor assessor, string children) =>
        assessor.Assess(XDocument.Parse("<order>" + children + "</order>", LoadOptions.PreserveWhitespace));
    private static AssessedDocument Valid(XsdDocumentAssessor assessor, string children)
    {
        Assert.True(assessor.IsSupported, string.Join(";", assessor.Diagnostics));
        var result = Assess(assessor, children);
        Assert.Equal(SchemaAssessmentOutcome.Valid, result.Outcome);
        return Assert.IsType<AssessedDocument>(result.Document);
    }
    private static XdmValue Query(XDocument xml, string expression) => XPath31Expression.Compile(expression)
        .Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(new XDocumentNode(xml)), 1, 1));

    /// <summary>The assertion filter admits only the specific standard error contract and never broad CLR failures.</summary>
    /// <param name="kind">Error contract variant.</param>
    /// <param name="expected">Expected narrow recognition.</param>
    [Theory]
    [InlineData("plain", true)] [InlineData("structured", true)]
    [InlineData("foreign", false)] [InlineData("wrong-code", false)]
    [InlineData("substring", false)] [InlineData("long-code", false)]
    [InlineData("bare-code", false)] [InlineData("argument", false)]
    [InlineData("cancel", false)] [InlineData("memory", false)]
    public void ErrorRecognition_RejectsForeignOrUnrelatedFailures(string kind, bool expected)
    {
        Exception error = kind switch
        {
            "plain" => new InvalidOperationException("XPTY0004: Value comparison requires singleton operands"),
            "structured" => new XPathErrorException("http://www.w3.org/2005/xqt-errors", "XPTY0004", "err", "cardinality"),
            "foreign" => new XPathErrorException("urn:foreign", "XPTY0004", "u", "XPTY0004: misleading text"),
            "wrong-code" => new XPathErrorException("http://www.w3.org/2005/xqt-errors", "FOER0000", "err", "XPTY0004: misleading text"),
            "substring" => new InvalidOperationException("Unexpected engine failure mentioning XPTY0004: in text"),
            "long-code" => new InvalidOperationException("XPTY00040: unrelated"),
            "bare-code" => new InvalidOperationException("XPTY0004"),
            "argument" => new ArgumentException("XPTY0004: unrelated CLR error"),
            "cancel" => new OperationCanceledException(),
            _ => new OutOfMemoryException(),
        };
        Assert.Equal(expected, XsdDocumentAssessor.IsAssertionCardinalityError(error));
    }

    /// <summary>Fault-injected compiled plans distinguish an actual standard code from foreign/custom error text.</summary>
    /// <param name="errorNamespace">Structured error namespace.</param>
    /// <param name="errorCode">Structured error local name.</param>
    /// <param name="invalid">Whether it is the admitted standard cardinality error.</param>
    [Theory]
    [InlineData("http://www.w3.org/2005/xqt-errors", "XPTY0004", true)]
    [InlineData("urn:foreign", "XPTY0004", false)]
    [InlineData("http://www.w3.org/2005/xqt-errors", "FOER0000", false)]
    public void InjectedErrorPlans_DoNotTurnArbitraryFailuresIntoInvalid(string errorNamespace, string errorCode, bool invalid)
    {
        var source = Compile(Element("a", "0", "2"));
        var fault = XPath31Expression.Compile($"error(QName('{errorNamespace}', '{errorCode}'), 'XPTY0004: misleading text')",
            new CompileOptions { Compatibility = XPathCompatibility.XPath20 });
        // Internal fault injection deliberately bypasses grammar admission to exercise the exception boundary.
        var assessor = new XsdDocumentAssessor(source.SchemaXml, [], source.RootDeclaration,
            [new AssessmentParticle(source.ChildDeclarations[0], 0, 2)], [fault]);
        var result = Assess(assessor, "<a>2</a><a>3</a>");
        Assert.Equal(invalid ? SchemaAssessmentOutcome.Invalid : SchemaAssessmentOutcome.Unsupported, result.Outcome);
        Assert.Null(result.Document);
        Assert.Equal(invalid ? SchemaDiagnosticCodes.AssertionViolated : SchemaDiagnosticCodes.AssessmentUnsupported, Assert.Single(result.Diagnostics).Code);
    }

    /// <summary>The per-particle limit permits sixteen actual nodes and rejects a seventeenth structurally.</summary>
    [Fact]
    public void MaximumSixteen_EnforcesActualCount()
    {
        var assessor = Compile(Element("a", "0", "16"));
        var children = string.Concat(Enumerable.Range(1, 16).Select(i => $"<a>{i}</a>"));
        var assessed = Valid(assessor, children);
        Assert.Equal(17, assessed.Elements.Count);
        Assert.Equal(16m, Query(assessed.CreateDocumentCopy(true), "data(/order/a[16])").DecimalValue);
        var invalid = Assess(assessor, children + "<a>17</a>");
        Assert.Equal(SchemaAssessmentOutcome.Invalid, invalid.Outcome);
        Assert.Null(invalid.Document);
    }

    /// <summary>An empty repeated profile retains root atomization, whitespace and explicit stripped semantics.</summary>
    [Fact]
    public void EmptyRepeatedRoot_RetainsOriginalOwnerAndNoTypedValue()
    {
        var assessed = Valid(Compile(Element("a", "0", "2")), "  ");
        var xml = assessed.CreateDocumentCopy(true); xml.Root!.RemoveNodes();
        Assert.Same(assessed, XdmElementAssessment.Get(xml.Root)!.Owner);
        Assert.Contains("FOTY0012", Assert.ThrowsAny<Exception>(() => Query(xml, "data(/order)")).ToString());
        Assert.True(Query(assessed.CreateDocumentCopy(false), "/order instance of element(*,xs:untyped)").BooleanValue);
    }

    /// <summary>Node/lexical limits and cancellation remain explicit refusals rather than partial results.</summary>
    [Fact]
    public void BoundsAndCancellation_ExposeNoPartialDocument()
    {
        var assessor = Compile(Element("a", "0", "2"));
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        Assert.Throws<OperationCanceledException>(() => assessor.Assess(XDocument.Parse("<order><a>2</a><a>3</a></order>"), canceled.Token));
        var lexical = Assess(assessor, "<a>2</a><a>" + new string('0', 257) + "1</a>");
        Assert.Equal(SchemaAssessmentOutcome.Unsupported, lexical.Outcome); Assert.Null(lexical.Document);
        var nodes = new XDocument(new XElement("order", Enumerable.Range(0, 257).Select(_ => new XComment("x"))));
        var bound = assessor.Assess(nodes);
        Assert.Equal(SchemaAssessmentOutcome.Unsupported, bound.Outcome); Assert.Null(bound.Document);
    }

    /// <summary>Escaped repeated documents strongly retain the owning original assessment.</summary>
    [Fact]
    public void EscapedRepeatedDocument_RetainsOwnerAndDistinctStates()
    {
        var (xml, owner) = Export(); GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Assert.True(owner.IsAlive);
        var children = xml.Root!.Elements().ToArray();
        var first = XdmElementAssessment.Get(children[0])!;
        var second = XdmElementAssessment.Get(children[1])!;
        Assert.Same(first.Owner, second.Owner); Assert.NotSame(first.State, second.State);
        Assert.Same(((AssessedElement)first.State).Declaration, ((AssessedElement)second.State).Declaration);
        Assert.Equal(3m, Query(xml, "data(/order/a[2])").DecimalValue);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (XDocument Xml, WeakReference Owner) Export()
    {
        var assessed = Valid(Compile(Element("a", "0", "2")), "<a>2</a><a>3</a>");
        return (assessed.CreateDocumentCopy(true), new WeakReference(assessed));
    }

    /// <summary>Occurrence coverage stays finite and unsigned, preserving prior profile selection.</summary>
    /// <param name="minimum">Local minimum.</param>
    /// <param name="maximum">Local maximum.</param>
    /// <param name="profile">Expected profile or null after refusal.</param>
    [Theory]
    [InlineData("0", "2", "decimal-sequence-assertions-v3")]
    [InlineData("1", "2", "decimal-sequence-assertions-v3")]
    [InlineData("0", "16", "decimal-sequence-assertions-v3")]
    [InlineData(" 01 ", " 002 ", "decimal-sequence-assertions-v3")]
    [InlineData("0", "01", "decimal-sequence-assertions-v2")]
    [InlineData("1", "1", "decimal-sequence-assertions-v1")]
    [InlineData("2", "2", null)] [InlineData("0", "17", null)]
    [InlineData("0", "0", null)] [InlineData("+0", "2", null)]
    [InlineData("0", "+2", null)] [InlineData("0", "-2", null)]
    [InlineData("0", "unbounded", null)] [InlineData("0", "", null)]
    [InlineData("0", "9999999999999999999999999999999999", null)]
    public void OccurrenceBounds_SelectProfileOrRefuse(string minimum, string maximum, string? profile)
    {
        var assessor = Compile(Element("a", minimum, maximum));
        Assert.Equal(profile is not null, assessor.IsSupported);
        if (profile is not null) Assert.Equal(profile, assessor.Profile);
        else
        {
            var result = Assess(assessor, "");
            Assert.Equal(SchemaAssessmentOutcome.Unsupported, result.Outcome);
            Assert.Null(result.Document);
        }
    }

    /// <summary>The declared aggregate budget is enforced before any instance can be accepted.</summary>
    [Fact]
    public void TotalMaximumBudget_AdmitsSixteenAndRefusesSeventeen()
    {
        var atLimit = Compile(Element("a", "0", "15") + Element("b"));
        var valid = Valid(atLimit, string.Concat(Enumerable.Repeat("<a>1</a>", 15)) + "<b>2</b>");
        Assert.Equal(17, valid.Elements.Count);
        Assert.False(Compile(Element("a", "0", "16") + Element("b")).IsSupported);
    }

    /// <summary>Contiguous counts, omissions, later particles and input ordering are checked structurally.</summary>
    /// <param name="minimum">Minimum for a.</param>
    /// <param name="children">Input child sequence.</param>
    /// <param name="valid">Expected structural result.</param>
    [Theory]
    [InlineData("0", "<b>9</b>", true)]
    [InlineData("1", "<b>9</b>", false)]
    [InlineData("1", "<a>1</a><b>9</b>", true)]
    [InlineData("1", "<a>1</a><a>2</a><b>9</b>", true)]
    [InlineData("0", "<a>1</a><a>2</a><a>3</a><b>9</b>", false)]
    [InlineData("0", "<a>1</a><a>2</a>", false)]
    [InlineData("0", "<a>1</a><b>9</b><a>2</a>", false)]
    [InlineData("0", "<b>9</b><a>1</a>", false)]
    [InlineData("0", "<x>0</x><b>9</b>", false)]
    [InlineData("0", "<b>9</b><x>0</x>", false)]
    public void ContiguousMatching_EnforcesCountsAndOrder(string minimum, string children, bool valid)
    {
        var assessor = Compile(Element("a", minimum, "2") + Element("gap", "0") + Element("b"));
        Assert.True(assessor.IsSupported);
        var result = Assess(assessor, children);
        Assert.Equal(valid ? SchemaAssessmentOutcome.Valid : SchemaAssessmentOutcome.Invalid, result.Outcome);
        if (!valid)
        {
            Assert.Null(result.Document);
            Assert.Equal(SchemaDiagnosticCodes.AssessmentInvalidInstance, Assert.Single(result.Diagnostics).Code);
        }
        else Assert.Same(assessor.ChildDeclarations[2], result.Document!.Elements.Last().Declaration);
    }

    /// <summary>Adjacent repeated first/middle/last particles retain every occurrence and shared authority.</summary>
    [Fact]
    public void RepeatedStates_AreDistinctEvenWhenValuesAreEqual()
    {
        var assessor = Compile(Element("a", "0", "2") + Element("b", "1", "2") + Element("c", "0", "2"));
        var assessed = Valid(assessor, "<a>2</a><a>2</a><b>3</b><b>4</b><c>5</c><c>6</c>");
        Assert.Equal(3, assessor.ChildDeclarations.Count);
        Assert.Equal(7, assessed.Elements.Count);
        var xml = assessed.CreateDocumentCopy(true);
        var nodes = xml.Root!.Elements().ToArray();
        for (var i = 0; i < nodes.Length; i++)
        {
            Assert.Same(assessed.Elements[i + 1], XdmElementAssessment.Get(nodes[i])!.State);
            Assert.Same(assessor.ChildDeclarations[i / 2], assessed.Elements[i + 1].Declaration);
        }
        Assert.NotSame(assessed.Elements[1], assessed.Elements[2]);
        Assert.False(new XDocumentNode(nodes[0]).IsSameNode(new XDocumentNode(nodes[1])));
        var copy = assessed.Copy();
        Assert.NotEqual(assessed.SnapshotIdentity, copy.SnapshotIdentity);
        Assert.Equal(assessed.AssessmentIdentity, copy.AssessmentIdentity);
        for (var i = 0; i < assessed.Elements.Count; i++) Assert.Same(assessed.Elements[i], copy.Elements[i]);
    }

    /// <summary>Present repeated values are validated exactly, including failures after earlier valid copies.</summary>
    /// <param name="second">Second occurrence lexical value.</param>
    /// <param name="expected">Expected failure classification.</param>
    [Theory]
    [InlineData("bad", SchemaAssessmentOutcome.Invalid)]
    [InlineData("1e2", SchemaAssessmentOutcome.Invalid)]
    [InlineData("79228162514264337593543950336", SchemaAssessmentOutcome.Unsupported)]
    [InlineData("0.00000000000000000000000000001", SchemaAssessmentOutcome.Unsupported)]
    public void LaterRepeatedValue_StillRequiresExactDecimal(string second, SchemaAssessmentOutcome expected)
    {
        var result = Assess(Compile(Element("a", "0", "2")), "<a>2</a><a>" + second + "</a>");
        Assert.Equal(expected, result.Outcome);
        Assert.Null(result.Document);
    }

    /// <summary>A multi-item singleton-value comparison is a failed assertion, not unsupported coverage.</summary>
    /// <param name="expression">Existing admitted assertion.</param>
    [Theory]
    [InlineData("a eq 2")] [InlineData("a ne 2")]
    [InlineData("a lt 3")] [InlineData("a le 3")]
    [InlineData("a gt 1")] [InlineData("a ge 1")]
    [InlineData("2 eq a")] [InlineData("2 ne a")]
    [InlineData("a eq a")] [InlineData("a eq b")]
    public void MultiItemValueComparison_IsAssertionInvalid(string expression)
    {
        var assessor = Compile(Element("a", "0", "2") + Element("b", "0", "2"), expression);
        Assert.True(assessor.IsSupported);
        var result = Assess(assessor, "<a>2</a><a>3</a><b>4</b><b>5</b>");
        Assert.Equal(SchemaAssessmentOutcome.Invalid, result.Outcome);
        Assert.Null(result.Document);
        Assert.Equal(SchemaDiagnosticCodes.AssertionViolated, Assert.Single(result.Diagnostics).Code);
    }

    /// <summary>Existing Boolean/logical semantics apply without scanning unexecuted branches.</summary>
    /// <param name="expression">Bounded assertion expression.</param>
    /// <param name="children">Actual repeated instance.</param>
    /// <param name="valid">Expected outcome for this engine's evaluation.</param>
    [Theory]
    [InlineData("a eq 2", "", false)]
    [InlineData("a eq 2", "<a>2</a>", true)]
    [InlineData("a eq 2", "<a>3</a>", false)]
    [InlineData("1 eq 1", "<a>2</a><a>3</a>", true)]
    [InlineData("1 eq 1 or a eq 2", "<a>2</a><a>3</a>", true)]
    [InlineData("1 eq 0 and a eq 2", "<a>2</a><a>3</a>", false)]
    public void AssertionEvaluation_RetainsEmptyAndShortCircuitBehavior(string expression, string children, bool valid)
    {
        var result = Assess(Compile(Element("a", "0", "2"), expression), children);
        Assert.Equal(valid ? SchemaAssessmentOutcome.Valid : SchemaAssessmentOutcome.Invalid, result.Outcome);
    }

    /// <summary>Singular projection never chooses a first repetition implicitly.</summary>
    /// <param name="count">Number of actual repeated occurrences.</param>
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void SingularProjection_RequiresExactlyOnePresentMatch(int count)
    {
        var assessed = Valid(Compile(Element("a", "0", "2") + Element("b")), string.Concat(Enumerable.Repeat("<a>2</a>", count)) + "<b>9</b>");
        if (count == 1) Assert.Equal("2", assessed.CreateTypedElementCopy("a").Value);
        else Assert.Contains("BSK-XSD-016", Assert.Throws<NotSupportedException>(() => assessed.CreateTypedElementCopy("a")).Message);
        Assert.Equal("9", assessed.CreateTypedElementCopy("b").Value);
        if (count == 2) Assert.Equal(2m, Query(assessed.CreateDocumentCopy(true), "data(/order/a[2])").DecimalValue);
    }

    /// <summary>Real XSLT preserve/strip retains repeated value order with correct child typing.</summary>
    [Fact]
    public void XsltCopies_RetainBothOccurrencesInOrder()
    {
        var assessor = Compile(Element("a", "0", "2") + Element("gap", "0") + Element("b"));
        var assessed = Valid(assessor, "<a>2</a><a>3</a><b>9</b>");
        const string style = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:err="http://www.w3.org/2005/xqt-errors">
          <xsl:template name="main">
            <xsl:variable name="p" as="element()"><xsl:copy-of select="/order" validation="preserve"/></xsl:variable>
            <xsl:variable name="s" as="element()"><xsl:copy-of select="/order" validation="strip"/></xsl:variable>
            <result values="{string-join($p/a ! string(data(.)), ',')}" typed="{every $n in $p/a satisfies data($n) instance of xs:decimal}" stripped="{every $n in $s/a satisfies data($n) instance of xs:untypedAtomic}" later="{data($p/b)}">
              <xsl:try><xsl:value-of select="$p"/><xsl:catch><error code="{$err:code}"/></xsl:catch></xsl:try>
            </result>
          </xsl:template>
        </xsl:stylesheet>
        """;
        var compiler = SchemaAwareXslt.CreateCompiler(); compiler.SchemaSet = new XmlSchemaSet();
        var xml = assessed.CreateDocumentCopy(true);
        var output = XElement.Parse(compiler.Compile(style).TransformToString(new XDocumentNode(xml), initialTemplate: "main"));
        Assert.Equal("2,3", (string?)output.Attribute("values"));
        Assert.Equal("true", (string?)output.Attribute("typed"));
        Assert.Equal("true", (string?)output.Attribute("stripped"));
        Assert.Equal("9", (string?)output.Attribute("later"));
        Assert.Contains("FOTY0012", (string?)output.Element("error")!.Attribute("code"));
        Assert.Same(assessed, XdmElementAssessment.Get(xml.Root!)!.Owner);
    }

    /// <summary>XQuery copies retain distinct repeated states or strip them without modifying the input.</summary>
    /// <param name="preserve">Construction mode.</param>
    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void XQueryCopies_RetainOccurrenceStateAndOwnership(bool preserve)
    {
        var assessed = Valid(Compile(Element("a", "0", "2")), "<a>2</a><a>3</a>");
        var input = assessed.CreateDocumentCopy(true);
        var query = "declare construction " + (preserve ? "preserve" : "strip") + "; declare variable $input external; <copy>{$input}</copy>/*";
        var value = new XQueryCompiler().Compile(query).Evaluate(new XQueryContext().WithVariable("input", XdmValue.FromNode(new XDocumentNode(input.Root!))));
        IXdmNode? node = value.IsNode ? value.NodeValue : null;
        if (node is null) foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) { node = item.NodeValue; break; }
        var result = Assert.IsType<XDocumentNode>(node);
        var children = ((XElement)result.UnderlyingObject).Elements().ToArray();
        Assert.Equal(2, children.Length);
        for (var i = 0; i < children.Length; i++)
        {
            if (preserve) Assert.Same(assessed.Elements[i + 1], XdmElementAssessment.Get(children[i])!.State);
            else Assert.Null(XdmElementAssessment.Get(children[i]));
            Assert.Equal((i + 2).ToString(), children[i].Value);
        }
        input.Root!.Elements().Last().Value = "99";
        Assert.Throws<NotSupportedException>(() => Query(input, "data(/order/a[2])"));
        Assert.Equal(3m, Query(assessed.CreateDocumentCopy(true), "data(/order/a[2])").DecimalValue);
    }

    /// <summary>Broader grammar and schema constructs remain refused by the new profile.</summary>
    /// <param name="expression">Unsupported grammar request.</param>
    [Theory]
    [InlineData("sum(a) gt 0")] [InlineData("count(a) eq 2")]
    [InlineData("a = 2")] [InlineData("a[1] eq 2")]
    public void WiderAssertionGrammar_RemainsUnsupported(string expression) =>
        Assert.False(Compile(Element("a", "0", "2"), expression).IsSupported);
}
