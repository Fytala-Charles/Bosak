// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 29 October 2026
// PURPOSE              : Tests bounded presence predicates, validation order and preserved assessment identity.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : LICENSE.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 29-10-2026     | Creation; bounded presence assertion Schema 0.9 candidate                                   |
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

/// <summary>Qualifies finite presence predicates independently of numeric values and broader grammar.</summary>
public sealed class PresenceAssertionAssessmentTests
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

    /// <summary>Presence depends on node cardinality, including zero-valued decimal children.</summary>
    /// <param name="count">Actual child count.</param>
    /// <param name="exists">Whether to test exists rather than empty.</param>
    [Theory]
    [InlineData(0, true)] [InlineData(1, true)] [InlineData(2, true)] [InlineData(16, true)]
    [InlineData(0, false)] [InlineData(1, false)] [InlineData(2, false)] [InlineData(16, false)]
    public void Presence_UsesNodesRatherThanDecimalValues(int count, bool exists)
    {
        var assessor = Compile((exists ? "exists(a)" : "empty(a)"), Element("a", 0, 16));
        Assert.True(assessor.IsSupported);
        Assert.Equal("decimal-sequence-assertions-v5", assessor.Profile);
        var result = Assess(assessor, string.Concat(Enumerable.Repeat("<a>0</a>", count)));
        Assert.Equal((exists == (count > 0)) ? SchemaAssessmentOutcome.Valid : SchemaAssessmentOutcome.Invalid, result.Outcome);
        if (result.Outcome == SchemaAssessmentOutcome.Invalid)
        {
            Assert.Null(result.Document);
            Assert.Equal(SchemaDiagnosticCodes.AssertionViolated, Assert.Single(result.Diagnostics).Code);
        }
        else
        {
            var xml = result.Document!.CreateDocumentCopy(true);
            Assert.Equal(count > 0, Query(xml, "exists(/order/a)").BooleanValue);
            Assert.Equal(count == 0, Query(xml, "empty(/order/a)").BooleanValue);
            Assert.True(Query(xml, "exists(/order/a) instance of xs:boolean").BooleanValue);
        }
    }

    /// <summary>Presence combines with the existing Boolean and scalar comparison grammar.</summary>
    /// <param name="expression">Complete assertion expression.</param>
    [Theory]
    [InlineData("exists(a) and count(a) eq 2")]
    [InlineData("empty(b) or b le a")]
    [InlineData("(exists(a) and empty(b)) or count(a) eq 0")]
    [InlineData("count(a) eq 2 and (empty(b) or exists(b))")]
    [InlineData("1 eq 1 or exists(b)")]
    [InlineData("empty ( b ) and exists ( a )")]
    public void BooleanPredicates_ComposeWithoutChangingScalarGrammar(string expression)
    {
        var assessor = Compile(expression, Element("a") + Element("b"));
        Valid(assessor, "<a>0</a><a>3</a>");
        Assert.Equal("decimal-sequence-assertions-v5", assessor.Profile);
    }

    /// <summary>All assertion plans contribute features even if an earlier expression is constant.</summary>
    [Fact]
    public void MultipleAssertions_RecordAllAdmittedFeatures()
    {
        var assessor = SchemaServices.CreateDocumentAssessor(Schema(Element("a"), "1 eq 1", "exists(a)", "count(a) eq 2"));
        Valid(assessor, "<a>0</a><a>1</a>");
        Assert.Equal("decimal-sequence-assertions-v5", assessor.Profile);
        Assert.False(SchemaServices.CreateDocumentAssessor(Schema(Element("a"), "exists(a)", "1 eq 1 or not(a)")).IsSupported);
    }

    /// <summary>Function spellings used as child names do not accidentally select presence profiles.</summary>
    [Fact]
    public void FunctionAndChildNames_AreDisambiguatedStructurally()
    {
        var particles = Element("exists", 1, 1) + Element("empty", 1, 1);
        Valid(Compile("exists(exists) and exists(empty)", particles), "<exists>0</exists><empty>2</empty>");
        Valid(Compile("empty(empty) or exists(exists)", particles), "<exists>0</exists><empty>2</empty>");
        var plain = Compile("exists eq 0 and empty eq 2", particles);
        Valid(plain, "<exists>0</exists><empty>2</empty>");
        Assert.Equal("decimal-sequence-assertions-v1", plain.Profile);
        Assert.Equal("decimal-sequence-assertions-v4", Compile("count(exists) eq 1", particles).Profile);
        Assert.Equal("decimal-sequence-assertions-v2", Compile("a eq 1", Element("a", 0, 1)).Profile);
        Assert.Equal("decimal-sequence-assertions-v3", Compile("a eq 1").Profile);
    }

    /// <summary>Complete admission rejects broader grammar before any short-circuit optimization.</summary>
    /// <param name="expression">Unsupported assertion.</param>
    [Theory]
    [InlineData("fn:exists(a)")] [InlineData("fn:empty(a)")] [InlineData("EXISTS(a)")]
    [InlineData("exists(missing)")] [InlineData("empty(missing)")]
    [InlineData("exists()")] [InlineData("empty()")]
    [InlineData("exists(a/b)")] [InlineData("empty(a[1])")]
    [InlineData("exists(count(a))")] [InlineData("count(exists(a)) eq 1")]
    [InlineData("exists(a) eq empty(a)")] [InlineData("exists(a) eq 1")]
    [InlineData("1 eq exists(a)")] [InlineData("not(exists(a))")]
    [InlineData("exists(a,a)")] [InlineData("exists(1)")]
    [InlineData("true()")] [InlineData("a")] [InlineData("count(a)")]
    [InlineData("1 eq 1 or fn:exists(a)")] [InlineData("exists(a) or unknown(a)")]
    [InlineData("exists(a) trailing")] [InlineData("exists((a))")]
    [InlineData("empty(a) + 1")]
    public void WiderGrammar_RemainsUnsupported(string expression) => Assert.False(Compile(expression).IsSupported);

    /// <summary>Character, token and parenthesis limits remain independent of runtime evaluation.</summary>
    [Fact]
    public void SyntaxBudgets_AreEnforcedBeforeCompilation()
    {
        const string atom = "exists(a)";
        Assert.True(Compile(new string(' ', 512 - atom.Length) + atom).IsSupported);
        Assert.False(Compile(new string(' ', 513 - atom.Length) + atom).IsSupported);
        Assert.True(Compile(new string('(', 15) + atom + new string(')', 15)).IsSupported);
        Assert.False(Compile(new string('(', 16) + atom + new string(')', 16)).IsSupported);
        var limit = "1 eq 1 or " + string.Join(" or ", Enumerable.Repeat(atom, 25)); // 4 + 124 = 128 tokens.
        Assert.True(Compile(limit).IsSupported);
        Assert.False(Compile(limit + " or " + atom).IsSupported);
        Assert.False(Compile("exists\u00a0(a)").IsSupported);
    }

    /// <summary>Namespace bindings do not widen the fixed unprefixed local-child function grammar.</summary>
    /// <param name="expression">Assertion spelling.</param>
    /// <param name="supported">Expected grammar admission.</param>
    [Theory]
    [InlineData("exists(a)", true)] [InlineData("empty(a)", true)]
    [InlineData("fn:exists(a)", false)] [InlineData("f:empty(a)", false)]
    public void NamespaceBindings_DoNotAdmitAliases(string expression, bool supported)
    {
        var schema = Schema(Element("a"), expression);
        schema.Root!.Add(new XAttribute(XNamespace.Xmlns + "fn", "http://www.w3.org/2005/xpath-functions"));
        schema.Root.Add(new XAttribute(XNamespace.Xmlns + "f", "http://www.w3.org/2005/xpath-functions"));
        Assert.Equal(supported, SchemaServices.CreateDocumentAssessor(schema).IsSupported);
        schema.Descendants(XName.Get("assert", "http://www.w3.org/2001/XMLSchema")).Single()
            .SetAttributeValue("xpathDefaultNamespace", "urn:outside");
        Assert.False(SchemaServices.CreateDocumentAssessor(schema).IsSupported);
    }

    /// <summary>Presence alone selects v5 for required particles and local optional structure is still validated.</summary>
    [Fact]
    public void PresenceProfile_IsIndependentOfStructuralProfile()
    {
        var required = Compile("exists(a)", Element("a", 1, 1));
        Valid(required, "<a>0</a>"); Assert.Equal("decimal-sequence-assertions-v5", required.Profile);
        Assert.Equal(SchemaAssessmentOutcome.Invalid, Assess(required, "").Outcome);
        var optional = Compile("empty(a)", Element("a", 0, 1));
        Valid(optional, ""); Assert.Equal("decimal-sequence-assertions-v5", optional.Profile);
        Assert.Equal(SchemaAssessmentOutcome.Invalid, Assess(optional, "<a>0</a>").Outcome);
    }

    /// <summary>Native decimal validity and exact representation precede a satisfied presence predicate.</summary>
    /// <param name="lexical">Input value.</param>
    /// <param name="expected">Native classification.</param>
    [Theory]
    [InlineData("bad", SchemaAssessmentOutcome.Invalid)]
    [InlineData("79228162514264337593543950336", SchemaAssessmentOutcome.Unsupported)]
    [InlineData("0.00000000000000000000000000001", SchemaAssessmentOutcome.Unsupported)]
    public void NativeValues_PrecedeAssertions(string lexical, SchemaAssessmentOutcome expected)
    {
        var assessor = Compile("exists(a)"); Assert.True(assessor.IsSupported);
        var result = Assess(assessor, "<a>" + lexical + "</a>");
        Assert.Equal(expected, result.Outcome); Assert.Null(result.Document);
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == SchemaDiagnosticCodes.AssertionViolated);
    }

    /// <summary>Structural failure and scalar cardinality errors retain their distinct diagnostics.</summary>
    [Fact]
    public void StructureAndScalarCardinality_AreNotMaskedByPresence()
    {
        var structural = Compile("exists(a)", Element("a") + Element("b", 1, 1));
        Assert.True(structural.IsSupported);
        var absent = Assess(structural, "<a>1</a>");
        Assert.Equal(SchemaDiagnosticCodes.AssessmentInvalidInstance, Assert.Single(absent.Diagnostics).Code);
        var repeated = Assess(Compile("exists(a) and a eq 1"), "<a>1</a><a>2</a>");
        Assert.Equal(SchemaAssessmentOutcome.Invalid, repeated.Outcome);
        Assert.Equal(SchemaDiagnosticCodes.AssertionViolated, Assert.Single(repeated.Diagnostics).Code);
    }
    /// <summary>A presence-validated document retains real typed copies, original identity and source ownership.</summary>
    [Fact]
    public void PresenceValidatedDocument_PreservesTypedCopySemantics()
    {
        var assessor = Compile("exists(a) and count(a) eq 2");
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

    /// <summary>XQuery preserves or strips actual presence-validated state on destination-private nodes.</summary>
    /// <param name="preserve">Construction mode.</param>
    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void PresenceValidatedXQueryCopy_RetainsOrStripsOriginalState(bool preserve)
    {
        var assessed = Valid(Compile("exists(a) and count(a) eq 2"), "<a>2</a><a>3</a>");
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
