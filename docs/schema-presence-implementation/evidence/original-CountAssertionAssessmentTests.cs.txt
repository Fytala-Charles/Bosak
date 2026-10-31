// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Tests bounded count assertion admission, name semantics, validation order and typed copies.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : LICENSE.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation; bounded count assertion Schema 0.8 candidate                                   |
//                      | Charles Korthout | 0.2   | 29-10-2026     | Cover structural profile compatibility and full syntax admission before optimization      |
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

/// <summary>Qualifies count calls separately from native validation, bare child names and larger grammar.</summary>
public sealed class CountAssertionAssessmentTests
{
    private static string Element(string name, int minimum = 0, int maximum = 2) =>
        $"<xs:element name='{name}' type='xs:decimal' minOccurs='{minimum}' maxOccurs='{maximum}'/>";
    private static XDocument Schema(string particles, params string[] assertions)
    {
        var schema = XDocument.Parse($"""
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"><xs:element name="order"><xs:complexType>
          <xs:sequence>{particles}</xs:sequence>
        </xs:complexType></xs:element></xs:schema>
        """);
        var type = schema.Descendants(XName.Get("complexType", "http://www.w3.org/2001/XMLSchema")).Single();
        foreach (var expression in assertions) type.Add(new XElement(XName.Get("assert", "http://www.w3.org/2001/XMLSchema"), new XAttribute("test", expression)));
        return schema;
    }
    private static XsdDocumentAssessor Compile(string expression, string? particles = null) =>
        SchemaServices.CreateDocumentAssessor(Schema(particles ?? Element("a"), expression));
    private static SchemaDocumentAssessment Assess(XsdDocumentAssessor assessor, string children) =>
        assessor.Assess(XDocument.Parse("<order>" + children + "</order>", LoadOptions.PreserveWhitespace));
    private static AssessedDocument Valid(XsdDocumentAssessor assessor, string children)
    {
        Assert.True(assessor.IsSupported, string.Join(";", assessor.Diagnostics));
        var result = Assess(assessor, children); Assert.Equal(SchemaAssessmentOutcome.Valid, result.Outcome);
        return Assert.IsType<AssessedDocument>(result.Document);
    }
    private static XdmValue Query(XDocument xml, string expression) => XPath31Expression.Compile(expression)
        .Evaluate(new EvaluationContext().WithFocus(XdmValue.FromNode(new XDocumentNode(xml)), 1, 1));

    /// <summary>Cardinality counts actual occurrences and ignores non-element content without atomizing the root.</summary>
    /// <param name="count">Actual occurrence count.</param>
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(16)]
    public void Cardinality_IsAnIntegerAcrossFiniteDomain(int count)
    {
        var assessor = Compile($"count(a) eq {count}", Element("a", 0, 16));
        var assessed = Valid(assessor, " <!--comment--><?count scope?> " + string.Concat(Enumerable.Repeat("<a>2</a>", count)));
        Assert.Equal("decimal-sequence-assertions-v4", assessor.Profile);
        var xml = assessed.CreateDocumentCopy(true);
        var value = Query(xml, "count(/order/a)");
        Assert.Equal(count, value.IntegerValue);
        Assert.True(Query(xml, "count(/order/a) instance of xs:integer").BooleanValue);
        Assert.True(new XDocumentNode(xml.Root!).HasNoTypedValue);
        Assert.Equal(count + 1, assessed.Elements.Count);
    }

    /// <summary>Count supplies a scalar to existing comparison/logical operations.</summary>
    /// <param name="expression">Admitted expression.</param>
    /// <param name="valid">Expected assertion value with two a and two b nodes.</param>
    [Theory]
    [InlineData("count(a) eq 2", true)] [InlineData("count(a) ne 2", false)]
    [InlineData("count(a) lt 3", true)] [InlineData("count(a) le 2", true)]
    [InlineData("count(a) gt 1", true)] [InlineData("count(a) ge 3", false)]
    [InlineData("count(a) eq +02.0", true)] [InlineData("count(a) gt -1", true)]
    [InlineData("count(a) eq 1.5", false)] [InlineData("count(a) eq count(b)", true)]
    [InlineData("count(a) eq 2 and count(b) eq 2", true)]
    [InlineData("count(a) eq 0 or count(b) eq 2", true)]
    public void ScalarComparisons_RetainNativeBooleanSemantics(string expression, bool valid)
    {
        var assessor = Compile(expression, Element("a") + Element("b"));
        Assert.True(assessor.IsSupported);
        var result = Assess(assessor, "<a>2</a><a>2</a><b>3</b><b>3</b>");
        Assert.Equal(valid ? SchemaAssessmentOutcome.Valid : SchemaAssessmentOutcome.Invalid, result.Outcome);
        if (!valid) { Assert.Null(result.Document); Assert.Equal(SchemaDiagnosticCodes.AssertionViolated, Assert.Single(result.Diagnostics).Code); }
    }

    /// <summary>An empty count is zero, whereas an uncounted empty value comparison has false EBV.</summary>
    [Fact]
    public void EmptyCount_DiffersFromEmptyValueComparison()
    {
        Assert.Equal(SchemaAssessmentOutcome.Valid, Assess(Compile("count(a) eq 0"), "").Outcome);
        Assert.Equal(SchemaAssessmentOutcome.Invalid, Assess(Compile("a eq 0"), "").Outcome);
    }

    /// <summary>Only parsed calls select v4; bare child names preserve legacy structural profiles.</summary>
    [Fact]
    public void CountChildName_IsNotAFunctionMarker()
    {
        var bare = Compile("count eq 2", Element("count", 1, 1));
        Assert.Equal("decimal-sequence-assertions-v1", bare.Profile);
        Valid(bare, "<count>2</count>");
        var call = Compile("count(count) eq 1", Element("count", 1, 1));
        Assert.Equal("decimal-sequence-assertions-v4", call.Profile);
        Valid(call, "<count>2</count>");
        var substring = Compile("discount eq 2", Element("discount", 0, 1));
        Assert.Equal("decimal-sequence-assertions-v2", substring.Profile);
        Valid(substring, "<discount>2</discount>");
        var repeated = Compile("count eq 2", Element("count"));
        Assert.Equal("decimal-sequence-assertions-v3", repeated.Profile);
        Valid(repeated, "<count>2</count>");
    }

    /// <summary>Count calls select v4 independently of the prior structural profile.</summary>
    /// <param name="minimum">Particle minimum.</param>
    /// <param name="maximum">Particle maximum.</param>
    /// <param name="count">Actual count.</param>
    /// <param name="legacy">Profile without count admission.</param>
    [Theory]
    [InlineData(1, 1, 1, "decimal-sequence-assertions-v1")]
    [InlineData(0, 1, 0, "decimal-sequence-assertions-v2")]
    [InlineData(0, 2, 2, "decimal-sequence-assertions-v3")]
    public void StructuralProfiles_AreRetainedUntilAnActualCall(int minimum, int maximum, int count, string legacy)
    {
        var particles = Element("a", minimum, maximum);
        Assert.Equal(legacy, Compile("1 eq 1", particles).Profile);
        var counted = Compile($"count(a) eq {count}", particles);
        Assert.Equal("decimal-sequence-assertions-v4", counted.Profile);
        Valid(counted, string.Concat(Enumerable.Repeat("<a>2</a>", count)));
    }

    /// <summary>Admitted features are syntactic even when the optimizer can eliminate a logical branch.</summary>
    [Fact]
    public void UnexecutedAdmittedCall_StillSelectsCountProfile()
    {
        var assessor = Compile("1 eq 1 or count(a) eq 2");
        Assert.Equal("decimal-sequence-assertions-v4", assessor.Profile);
        Valid(assessor, "");
    }

    /// <summary>XML whitespace around call delimiters remains admitted.</summary>
    /// <param name="expression">Call spelling.</param>
    [Theory]
    [InlineData("count(a) eq 2")]
    [InlineData("count ( a ) eq 2")]
    [InlineData("count\t(\na\r) eq 2")]
    public void CallWhitespace_IsBoundedXmlWhitespace(string expression) =>
        Valid(Compile(expression), "<a>1</a><a>2</a>");

    /// <summary>Malformed, prefixed or broader function arguments cannot leak a successful feature marker.</summary>
    /// <param name="expression">Refused expression.</param>
    [Theory]
    [InlineData("count() eq 0")] [InlineData("count(a,b) eq 2")]
    [InlineData("count(count(a)) eq 1")] [InlineData("count((a)) eq 1")]
    [InlineData("count(unknown) eq 0")] [InlineData("count(a[1]) eq 1")]
    [InlineData("count(*) eq 2")] [InlineData("count(a/b) eq 0")]
    [InlineData("count(1) eq 1")] [InlineData("count((a,b)) eq 2")]
    [InlineData("count($a) eq 0")] [InlineData("count(a) = 2")]
    [InlineData("count(a) + 1 eq 3")] [InlineData("fn:count(a) eq 2")]
    [InlineData("f:count(a) eq 2")] [InlineData("COUNT(a) eq 2")]
    [InlineData("sum(a) gt 0")] [InlineData("avg(a) gt 0")]
    [InlineData("exists(a)")] [InlineData("count(a)")]
    [InlineData("count(a) eq 2 trailing")] [InlineData("count(a eq 2")]
    [InlineData("1 eq 1 or fn:count(a) eq 2")]
    [InlineData("1 eq 0 and sum(a) eq 2")]
    public void WiderOrPartialGrammar_IsRefused(string expression)
    {
        var assessor = Compile(expression, Element("a") + Element("b"));
        Assert.False(assessor.IsSupported);
        Assert.NotEqual("decimal-sequence-assertions-v4", assessor.Profile);
        var result = Assess(assessor, "<a>1</a><a>2</a>");
        Assert.Equal(SchemaAssessmentOutcome.Unsupported, result.Outcome); Assert.Null(result.Document);
    }

    /// <summary>XML prefix bindings cannot rebind an unprefixed standard function or admit a prefixed call.</summary>
    /// <param name="functionNamespace">Schema XML fn prefix binding.</param>
    [Theory]
    [InlineData("http://www.w3.org/2005/xpath-functions")]
    [InlineData("urn:foreign")]
    public void PrefixBindings_DoNotBroadenFunctionAdmission(string functionNamespace)
    {
        var schema = Schema(Element("a"), "count(a) eq 2");
        schema.Root!.SetAttributeValue(XNamespace.Xmlns + "fn", functionNamespace);
        Valid(SchemaServices.CreateDocumentAssessor(schema), "<a>1</a><a>2</a>");
        schema.Descendants(XName.Get("assert", "http://www.w3.org/2001/XMLSchema")).Single().SetAttributeValue("test", "fn:count(a) eq 2");
        Assert.False(SchemaServices.CreateDocumentAssessor(schema).IsSupported);
    }

    /// <summary>A count call in any fully admitted assertion selects v4, but later refusal rejects the schema.</summary>
    [Fact]
    public void MultipleAssertions_RequireCompleteAdmission()
    {
        var source = Schema(Element("a"), "1 eq 1", "count(a) eq 2");
        var assessor = SchemaServices.CreateDocumentAssessor(source);
        Assert.Equal("decimal-sequence-assertions-v4", assessor.Profile);
        Valid(assessor, "<a>1</a><a>2</a>");
        source.Descendants(XName.Get("complexType", "http://www.w3.org/2001/XMLSchema")).Single()
            .Add(new XElement(XName.Get("assert", "http://www.w3.org/2001/XMLSchema"), new XAttribute("test", "fn:count(a) eq 2")));
        var refused = SchemaServices.CreateDocumentAssessor(source);
        Assert.False(refused.IsSupported); Assert.NotEqual("decimal-sequence-assertions-v4", refused.Profile);
    }

    /// <summary>Function-call parentheses count toward the existing depth bound without changing legacy grouping.</summary>
    /// <param name="groups">Outer Boolean grouping depth.</param>
    /// <param name="supported">Expected count-call admission.</param>
    [Theory]
    [InlineData(15, true)] [InlineData(16, false)]
    public void DepthBound_IncludesCallParentheses(int groups, bool supported)
    {
        var expression = new string('(', groups) + "count(a) eq 0" + new string(')', groups);
        Assert.Equal(supported, Compile(expression).IsSupported);
        Assert.True(Compile(new string('(', 16) + "a eq 0" + new string(')', 16)).IsSupported);
    }

    /// <summary>Token and character bounds are preserved even when short-circuit evaluation would skip work.</summary>
    [Fact]
    public void CharacterAndTokenLimits_ApplyToTheWholeExpression()
    {
        const string basic = "count(a) eq 0";
        Assert.True(Compile(new string(' ', 512 - basic.Length) + basic).IsSupported);
        Assert.False(Compile(new string(' ', 513 - basic.Length) + basic).IsSupported);
        var atTokenLimit = "count(a) eq count(a) or " + string.Join(" or ", Enumerable.Repeat(basic, 17));
        Assert.True(Compile(atTokenLimit).IsSupported); // 9 + 17*6 + 18-1 = 128 tokens.
        Assert.False(Compile(string.Join(" or ", Enumerable.Repeat(basic, 19))).IsSupported);
    }

    /// <summary>A satisfied count never bypasses native lexical and representation validation.</summary>
    /// <param name="lexical">Second child value.</param>
    /// <param name="expected">Native value classification.</param>
    [Theory]
    [InlineData("bad", SchemaAssessmentOutcome.Invalid)]
    [InlineData("79228162514264337593543950336", SchemaAssessmentOutcome.Unsupported)]
    [InlineData("0.00000000000000000000000000001", SchemaAssessmentOutcome.Unsupported)]
    public void NativeValueChecks_PrecedeCountAssertions(string lexical, SchemaAssessmentOutcome expected)
    {
        var assessor = Compile("count(a) eq 2"); Assert.True(assessor.IsSupported);
        var result = Assess(assessor, "<a>2</a><a>" + lexical + "</a>");
        Assert.Equal(expected, result.Outcome); Assert.Null(result.Document);
        Assert.DoesNotContain(result.Diagnostics, x => x.Code == SchemaDiagnosticCodes.AssertionViolated);
    }

    /// <summary>Structural failure wins before an otherwise satisfied count assertion.</summary>
    [Fact]
    public void NativeStructure_PrecedesCountAssertions()
    {
        var assessor = Compile("count(a) eq 2", Element("a") + Element("b", 1, 1));
        var result = Assess(assessor, "<a>2</a><a>3</a>");
        Assert.Equal(SchemaAssessmentOutcome.Invalid, result.Outcome);
        Assert.Equal(SchemaDiagnosticCodes.AssessmentInvalidInstance, Assert.Single(result.Diagnostics).Code);
        Assert.Null(result.Document);
    }

    /// <summary>Count is scalar but does not suppress a cardinality error on an uncounted operand.</summary>
    [Fact]
    public void UncountedMultiItemOperand_StillFailsAssertion()
    {
        var result = Assess(Compile("count(a) eq b", Element("a") + Element("b")), "<a>1</a><a>2</a><b>2</b><b>2</b>");
        Assert.Equal(SchemaAssessmentOutcome.Invalid, result.Outcome);
        Assert.Equal(SchemaDiagnosticCodes.AssertionViolated, Assert.Single(result.Diagnostics).Code);
    }

    /// <summary>A count-validated document retains real typed copies, original identity and source ownership.</summary>
    [Fact]
    public void CountValidatedDocument_PreservesTypedCopySemantics()
    {
        var assessor = Compile("count(a) eq 2");
        var assessed = Valid(assessor, "<a>2</a><a>3</a>");
        var input = assessed.CreateDocumentCopy(true);
        var children = input.Root!.Elements().ToArray();
        Assert.Same(assessed.Elements[1].Declaration, assessed.Elements[2].Declaration);
        Assert.NotSame(assessed.Elements[1], assessed.Elements[2]);
        Assert.Same(assessed.Elements[2], XdmElementAssessment.Get(children[1])!.State);
        Assert.Throws<NotSupportedException>(() => assessed.CreateTypedElementCopy("a"));
        const string style = """
        <xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema">
          <xsl:template name="main">
            <xsl:variable name="p" as="element()"><xsl:copy-of select="/order" validation="preserve"/></xsl:variable>
            <xsl:variable name="s" as="element()"><xsl:copy-of select="/order" validation="strip"/></xsl:variable>
            <result values="{string-join($p/a ! string(data(.)), ',')}" typed="{every $n in $p/a satisfies data($n) instance of xs:decimal}" stripped="{every $n in $s/a satisfies data($n) instance of xs:untypedAtomic}"/>
          </xsl:template>
        </xsl:stylesheet>
        """;
        var compiler = SchemaAwareXslt.CreateCompiler(); compiler.SchemaSet = new XmlSchemaSet();
        var output = XElement.Parse(compiler.Compile(style).TransformToString(new XDocumentNode(input), initialTemplate: "main"));
        Assert.Equal("2,3", (string?)output.Attribute("values"));
        Assert.Equal("true", (string?)output.Attribute("typed")); Assert.Equal("true", (string?)output.Attribute("stripped"));
        Assert.Contains("FOTY0012", Assert.ThrowsAny<Exception>(() => Query(input, "data(/order)")).ToString());
        children[1].Value = "99";
        Assert.Throws<NotSupportedException>(() => Query(input, "data(/order/a[2])"));
        Assert.Equal(3m, Query(assessed.CreateDocumentCopy(true), "data(/order/a[2])").DecimalValue);
    }

    /// <summary>XQuery preserves or strips actual count-validated state on destination-private nodes.</summary>
    /// <param name="preserve">Construction mode.</param>
    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void CountValidatedXQueryCopy_RetainsOrStripsOriginalState(bool preserve)
    {
        var assessed = Valid(Compile("count(a) eq 2"), "<a>2</a><a>3</a>");
        var input = assessed.CreateDocumentCopy(true);
        var query = "declare construction " + (preserve ? "preserve" : "strip") + "; declare variable $input external; <copy>{$input}</copy>/*";
        var value = new XQueryCompiler().Compile(query).Evaluate(new XQueryContext().WithVariable("input", XdmValue.FromNode(new XDocumentNode(input.Root!))));
        IXdmNode? node = value.IsNode ? value.NodeValue : null;
        if (node is null) foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) { node = item.NodeValue; break; }
        var root = (XElement)Assert.IsType<XDocumentNode>(node).UnderlyingObject;
        if (preserve) Assert.Same(assessed.Elements[2], XdmElementAssessment.Get(root.Elements().Last())!.State);
        else Assert.Null(XdmElementAssessment.Get(root.Elements().Last()));
        Assert.Same(assessed, XdmElementAssessment.Get(input.Root!)!.Owner);
    }
}
