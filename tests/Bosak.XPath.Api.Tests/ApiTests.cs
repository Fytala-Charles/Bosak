// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Unit tests for the public XPath 3.1 API surface.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Added occurrence indicator tests for type expressions                                  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 19-07-2026     | Added DebugBooleanEqual regression test for and/or target-register reuse                  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 19-07-2026     | Added UnaryPlus_ValidatesOperandType test                                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 19-07-2026     | Added NumericSubtract_PromotesUntypedAtomicToDouble test                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 19-07-2026     | Added StringLessThan_UsesUnicodeCodepoints test                                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 19-07-2026     | Added Compare_RejectsNonStringArguments test                                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 19-07-2026     | Added DoubleMaxValue_RoundTripString test                                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 19-07-2026     | Added IriToUri_RejectsNonStringArguments test                                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.0   | 19-07-2026     | Added ImplicitTimezone_DivByInvalidNumber_Throws test                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.1   | 19-07-2026     | Added SubstringAfter_ResolvesRelativeCollationUri test                                   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.2   | 19-07-2026     | Added DocAvailable_RejectsNonStringArgument test                                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.3   | 20-07-2026     | Added Not_ThrowsOnMixedSequence test                                                     |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.4   | 20-07-2026     | Added Number_ThrowsWithoutContextItem test                                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.5   | 20-07-2026     | Added UpperCase_ArmenianLigatureMenXeh test                                              |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 1.6   | 20-07-2026     | Added Data_ThrowsFoty0012ForElementOnlyComplexElement test                             |
//                      | Charles Korthout | 1.7   | 20-07-2026     | Added DeepEqual_RespectsImplicitTimezone test                                            |
//                      | Charles Korthout | 1.8   | 20-07-2026     | Added Number_ReturnsNaNForNonNumericNonStringTypes test                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using System.Xml;
using System.Xml.Schema;
using Xunit;

namespace Bosak.XPath.Api.Tests;

public class ApiTests
{
    private static XdmValue Eval(string xpath)
    {
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        return XPath31Expression.Compile(xpath).Evaluate(ctx);
    }

    private static string[] EvalSequence(string xpath)
    {
        var result = Eval(xpath);
        Assert.True(result.IsSequence);
        var list = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            list.Add(item.ToString());
        return list.ToArray();
    }

    // ------------------------------------------------------------------
    // Compilation
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_SimpleExpression()
    {
        var expr = XPath31Expression.Compile("1 + 2");
        Assert.NotNull(expr);
    }

    [Fact]
    public void Compile_PathExpression()
    {
        var expr = XPath31Expression.Compile("/root/child");
        Assert.NotNull(expr);
    }

    [Fact]
    public void Compile_WithNamespaces()
    {
        var expr = XPath31Expression.Compile("xs:integer('42')");
        Assert.NotNull(expr);
    }

    [Fact]
    public void Compile_Predicate()
    {
        var expr = XPath31Expression.Compile("//a[@id='1']");
        Assert.NotNull(expr);
    }

    [Fact]
    public void Compile_ForExpression()
    {
        var expr = XPath31Expression.Compile("for $x in (1,2,3) return $x * 2");
        Assert.NotNull(expr);
    }

    [Fact]
    public void Compile_IfExpression()
    {
        var expr = XPath31Expression.Compile("if (true()) then 1 else 0");
        Assert.NotNull(expr);
    }

    [Fact]
    public void Compile_MapConstructor()
    {
        var expr = XPath31Expression.Compile("map { 'a': 1 }");
        Assert.NotNull(expr);
    }

    [Fact]
    public void Compile_ArrayConstructor()
    {
        var expr = XPath31Expression.Compile("[1, 2, 3]");
        Assert.NotNull(expr);
    }

    // ------------------------------------------------------------------
    // Atomic evaluation
    // ------------------------------------------------------------------

    [Fact]
    public void EvaluateValue_Integer()
    {
        var result = Eval("42");
        Assert.Equal("42", result.ToString());
    }

    [Fact]
    public void EvaluateValue_String()
    {
        var result = Eval("'hello'");
        Assert.Equal("hello", result.ToString());
    }

    [Fact]
    public void EvaluateValue_Arithmetic()
    {
        var result = Eval("2 + 3 * 4");
        Assert.Equal("14", result.ToString());
    }

    [Fact]
    public void EvaluateValue_Boolean()
    {
        var result = Eval("true()");
        Assert.Equal("true", result.ToString());
    }

    [Fact]
    public void EvaluateValue_BooleanFalse()
    {
        var result = Eval("false()");
        Assert.Equal("false", result.ToString());
    }

    [Fact]
    public void EvaluateValue_Comparison()
    {
        var result = Eval("1 = 1");
        Assert.Equal("true", result.ToString());
    }

    [Fact]
    public void EvaluateValue_ValueComparison()
    {
        var result = Eval("1 eq 1");
        Assert.Equal("true", result.ToString());
    }

    [Fact]
    public void EvaluateValue_Concat()
    {
        var result = Eval("'hello' || ' ' || 'world'");
        Assert.Equal("hello world", result.ToString());
    }

    [Fact]
    public void EvaluateValue_FunctionCall()
    {
        var result = Eval("fn:string(42)");
        Assert.Equal("42", result.ToString());
    }

    [Fact]
    public void EvaluateValue_NestedFunctionCall()
    {
        var result = Eval("fn:concat('a','b','c')");
        Assert.Equal("abc", result.ToString());
    }

    [Fact]
    public void EvaluateValue_IfExpression()
    {
        var result = Eval("if (1 = 1) then 'yes' else 'no'");
        Assert.Equal("yes", result.ToString());
    }

    [Fact]
    public void EvaluateValue_ForExpression()
    {
        var items = EvalSequence("for $x in (1,2,3) return $x * 2");
        Assert.Equal(new[] { "2", "4", "6" }, items);
    }

    [Fact]
    public void EvaluateValue_QuantifiedSome()
    {
        var result = Eval("some $x in (1,2,3) satisfies $x > 2");
        Assert.Equal("true", result.ToString());
    }

    [Fact]
    public void EvaluateValue_QuantifiedEvery()
    {
        var result = Eval("every $x in (1,2,3) satisfies $x > 0");
        Assert.Equal("true", result.ToString());
    }

    [Fact]
    public void EvaluateValue_SimpleMap()
    {
        var items = EvalSequence("(1, 2, 3) ! (. + 10)");
        Assert.Equal(new[] { "11", "12", "13" }, items);
    }

    [Fact]
    public void EvaluateValue_InstanceOf()
    {
        var result = Eval("1 instance of xs:integer");
        Assert.Equal("true", result.ToString());
    }

    [Fact]
    public void EvaluateValue_Cast()
    {
        var result = Eval("1 cast as xs:string");
        Assert.Equal("1", result.ToString());
    }

    [Fact]
    public void EvaluateValue_Castable()
    {
        var result = Eval("1 castable as xs:integer");
        Assert.Equal("true", result.ToString());
    }

    [Fact]
    public void EvaluateValue_Treat()
    {
        var result = Eval("1 treat as xs:integer");
        Assert.Equal("1", result.ToString());
    }

    [Theory]
    [InlineData("() instance of xs:integer?", "true")]
    [InlineData("1 instance of xs:integer?", "true")]
    [InlineData("(1, 2) instance of xs:integer?", "false")]
    [InlineData("() instance of xs:integer*", "true")]
    [InlineData("(1, 2) instance of xs:integer*", "true")]
    [InlineData("() instance of xs:integer+", "false")]
    [InlineData("(1, 2) instance of xs:integer+", "true")]
    [InlineData("(1, 'a') instance of xs:integer*", "false")]
    [InlineData("'hello' instance of xs:string*", "true")]
    public void EvaluateValue_InstanceOf_Occurrence(string xpath, string expected)
    {
        var result = Eval(xpath);
        Assert.Equal(expected, result.ToString());
    }

    [Theory]
    [InlineData("() cast as xs:integer?", "()")]
    [InlineData("1 castable as xs:integer?", "true")]
    [InlineData("() castable as xs:integer?", "true")]
    public void EvaluateValue_Cast_Occurrence(string xpath, string expected)
    {
        var result = Eval(xpath);
        Assert.Equal(expected, result.ToString());
    }

    [Theory]
    [InlineData("1 treat as xs:integer?", "1")]
    public void EvaluateValue_Treat_Occurrence(string xpath, string expected)
    {
        var result = Eval(xpath);
        Assert.Equal(expected, result.ToString());
    }

    [Fact]
    public void EvaluateValue_Treat_Occurrence_EmptySequence()
    {
        var result = Eval("() treat as xs:integer?");
        Assert.True(result.IsSequence || result.IsUndefined);
        if (result.IsSequence)
        {
            int len = 0;
            foreach (var _ in XdmSequence.FromSource(result.SequenceValue!))
                len++;
            Assert.Equal(0, len);
        }
    }

    [Fact]
    public void EvaluateValue_Treat_Occurrence_Fails()
    {
        Assert.Throws<InvalidOperationException>(() => Eval("() treat as xs:integer+"));
    }

    // ------------------------------------------------------------------
    // Sequence evaluation
    // ------------------------------------------------------------------

    [Fact]
    public void EvaluateValue_Sequence()
    {
        var items = EvalSequence("(1, 2, 3)");
        Assert.Equal(new[] { "1", "2", "3" }, items);
    }

    [Fact]
    public void EvaluateValue_EmptySequence()
    {
        var result = Eval("()");
        Assert.True(result.IsSequence);
        Assert.True(result.SequenceValue!.TryGetLength(out var len));
        Assert.Equal(0, len);
    }

    [Fact]
    public void EvaluateValue_Range()
    {
        var items = EvalSequence("1 to 3");
        Assert.Equal(new[] { "1", "2", "3" }, items);
    }

    // ------------------------------------------------------------------
    // Map / Array constructors
    // ------------------------------------------------------------------

    [Fact]
    public void EvaluateValue_MapConstructor()
    {
        var result = Eval("map { 'a': 1, 'b': 2 }");
        Assert.True(result.IsMap);
    }

    [Fact]
    public void EvaluateValue_ArrayConstructor()
    {
        var result = Eval("[10, 20, 30]");
        Assert.True(result.IsArray);
    }

    [Fact]
    public void EvaluateValue_MapLookup()
    {
        var result = Eval("map { 'key': 'value' }?key");
        Assert.Equal("value", result.ToString());
    }

    [Fact]
    public void EvaluateValue_ArrayLookup()
    {
        var result = Eval("[10, 20, 30]?2");
        Assert.Equal("20", result.ToString());
    }

    [Fact]
    public void EvaluateValue_LookupWildcard()
    {
        var items = EvalSequence("[10, 20]?*");
        Assert.Contains("10", items);
        Assert.Contains("20", items);
    }

    // ------------------------------------------------------------------
    // Error cases
    // ------------------------------------------------------------------

    [Fact]
    public void Compile_Empty_Throws()
    {
        Assert.ThrowsAny<System.Exception>(() => XPath31Expression.Compile(""));
    }

    // ------------------------------------------------------------------
    // Caching / Re-evaluation
    // ------------------------------------------------------------------

    [Fact]
    public void EvaluateValue_MultipleTimes()
    {
        var expr = XPath31Expression.Compile("2 + 3");
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        var r1 = expr.Evaluate(ctx);
        var r2 = expr.Evaluate(ctx);
        Assert.Equal(r1.ToString(), r2.ToString());
        Assert.Equal("5", r1.ToString());
    }

    [Fact]
    public void EvaluateValue_DifferentExpressions()
    {
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        var expr1 = XPath31Expression.Compile("1 + 1");
        var expr2 = XPath31Expression.Compile("2 + 2");
        Assert.Equal("2", expr1.Evaluate(ctx).ToString());
        Assert.Equal("4", expr2.Evaluate(ctx).ToString());
    }

    [Fact]
    public void DebugBooleanEqual()
    {
        Assert.Equal("true", Eval("(1 eq 1) and (2 eq 2)").ToString());
        Assert.Equal("false", Eval("(1 eq 1) and (2 eq 3)").ToString());
        Assert.Equal("false", Eval("(1 eq 1) eq (2 eq 3)").ToString());
        Assert.Equal("true", Eval("xs:boolean('true') and xs:boolean('true')").ToString());
        Assert.Equal("false", Eval("xs:boolean('false') and xs:boolean('false')").ToString());
        Assert.Equal("false", Eval("(xs:boolean('true') and xs:boolean('true')) eq (xs:boolean('false') and xs:boolean('false'))").ToString());
    }

    [Fact]
    public void UnaryPlus_ValidatesOperandType()
    {
        Assert.Equal("3", Eval("+3").ToString());
        Assert.Equal("true", Eval("(+3) eq 3").ToString());
        Assert.Throws<InvalidOperationException>(() => Eval("+\"a string\""));
    }

    [Fact]
    public void NumericSubtract_PromotesUntypedAtomicToDouble()
    {
        var result = Eval("(xs:untypedAtomic('3') - 1.1)");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        var result2 = Eval("(1.1 - xs:untypedAtomic('3'))");
        Assert.Equal(XdmValueKind.Double, result2.Kind);
    }

    [Fact]
    public void StringLessThan_UsesUnicodeCodepoints()
    {
        // K2-StringLT-1: codepoint comparison must compare Unicode scalar values,
        // not UTF-16 code units. U+EA60 (60000) < U+11170 (70000) is true.
        Assert.Equal("true", Eval("\"\u60000\" lt \"\u70000\"").ToString());
        Assert.Equal("false", Eval("\"\u70000\" lt \"\u60000\"").ToString());
        Assert.Equal("true", Eval("\"\u70000\" gt \"\u60000\"").ToString());
    }

    [Fact]
    public void Compare_RejectsNonStringArguments()
    {
        // compare-011: fn:compare requires string-typed atomized arguments.
        Assert.Throws<InvalidOperationException>(() => Eval("compare(123, 456)"));
        Assert.Equal("-1", Eval("compare('a', 'b')").ToString());
        Assert.Equal("0", Eval("compare('a', 'a')").ToString());
    }

    [Fact]
    public void DoubleMaxValue_RoundTripString()
    {
        // Double formatting must preserve all round-trip digits (G17), not just G16.
        Assert.Equal("1.7976931348623157E308", Eval("xs:double('1.7976931348623157E308')").ToString());
        Assert.Equal("-1.7976931348623157E308", Eval("xs:double('-1.7976931348623157E308')").ToString());
    }

    [Fact]
    public void IriToUri_RejectsNonStringArguments()
    {
        // fn-iri-to-uri1args-5 / K2-IRIToURIfunc-3/4: argument must be a single string.
        Assert.Throws<InvalidOperationException>(() => Eval("iri-to-uri(12)"));
        Assert.Throws<InvalidOperationException>(() => Eval("iri-to-uri(1)"));
        Assert.Throws<InvalidOperationException>(() => Eval("iri-to-uri(('a string', 'a string'))"));
        Assert.Equal("a%20string", Eval("iri-to-uri('a string')").ToString());
    }

    [Fact]
    public void ImplicitTimezone_DivByInvalidNumber_Throws()
    {
        // fn-implicit-timezone-10/11/12: dividing a dayTimeDuration by NaN or 0
        // is an error, even when the duration itself is zero.
        Assert.Throws<InvalidOperationException>(() => Eval("fn:string(fn:implicit-timezone() div (0 div 0E0))"));
        Assert.Throws<InvalidOperationException>(() => Eval("fn:string(fn:implicit-timezone() div 0)"));
        Assert.Throws<InvalidOperationException>(() => Eval("fn:string(fn:implicit-timezone() div -0)"));
    }

    [Fact]
    public void SubstringAfter_ResolvesRelativeCollationUri()
    {
        // fn-substring-after-23 / fn-substring-before-23: relative collation URIs
        // resolve against the static base URI.
        var expr = XPath31Expression.Compile("substring-after('banana', 'a', 'collation/codepoint')");
        var ctx = new EvaluationContext { BaseUri = "http://www.w3.org/2005/xpath-functions/" };
        FunctionLibrary.Populate(ctx);
        Assert.Equal("nana", expr.Evaluate(ctx).ToString());
    }

    [Fact]
    public void DocAvailable_RejectsNonStringArgument()
    {
        // fn-doc-available-2: fn:doc-available expects a string-typed URI argument.
        Assert.Throws<InvalidOperationException>(() => Eval("fn:doc-available(xs:integer(2))"));
        Assert.Equal("false", Eval("fn:doc-available(())").ToString());
    }

    [Fact]
    public void Number_ThrowsWithoutContextItem()
    {
        // fn-number-3: fn:number() with no context item raises XPDY0002.
        Assert.Throws<InvalidOperationException>(() => Eval("number()"));
        Assert.Equal("NaN", Eval("number(())").ToString());
    }

    [Fact]
    public void Not_ThrowsOnMixedSequence()
    {
        // fn-not-28: EBV of a multi-item sequence whose first item is not a node is FORG0006.
        Assert.Throws<InvalidOperationException>(() => Eval("not((23, 24))"));
        Assert.Throws<InvalidOperationException>(() => Eval("not((1, 2, 3))"));
        Assert.Equal("true", Eval("not(())").ToString());
    }

    [Fact]
    public void UpperCase_ArmenianLigatureMenXeh()
    {
        // fn-upper-case-22: Armenian small ligature men xeh (U+FB17) upper-cases to two codepoints.
        var items = EvalSequence("string-to-codepoints(upper-case(codepoints-to-string(64279)))");
        Assert.Equal(new[] { "1348", "1341" }, items);
    }

    [Fact]
    public void Data_ThrowsFoty0012ForElementOnlyComplexElement()
    {
        // K2-DataFunc-6: fn:data() on a complex element-only schema-validated element raises FOTY0012.
        var schema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:test' xmlns='urn:test' elementFormDefault='qualified'>
            <xs:element name='root' type='rootType'/>
            <xs:complexType name='rootType'>
                <xs:sequence><xs:element name='child' type='xs:string'/></xs:sequence>
            </xs:complexType>
        </xs:schema>";

        var schemaSet = new XmlSchemaSet();
        using (var reader = XmlReader.Create(new System.IO.StringReader(schema)))
            schemaSet.Add("urn:test", reader);
        schemaSet.Compile();

        var xml = "<root xmlns='urn:test'><child>text</child></root>";
        var tempPath = System.IO.Path.GetTempFileName();
        System.IO.File.WriteAllText(tempPath, xml);
        var validatedDoc = XDocumentProvider.LoadXml(tempPath, baseUri: null, schemaSet: schemaSet);

        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        ctx = ctx.WithFocus(XdmValue.FromNode(validatedDoc), 1, 1);
        Assert.Throws<InvalidOperationException>(() => XPath31Expression.Compile("/*/data()").Evaluate(ctx));
    }

    [Fact]
    public void DeepEqual_RespectsImplicitTimezone()
    {
        // K2-SeqDeepEqualFunc-40: fn:deep-equal must apply the implicit timezone when
        // comparing a dateTime without an explicit timezone to one with a timezone.
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        ctx.ImplicitTimezoneOffsetMinutes = 120; // +02:00

        var result = XPath31Expression.Compile(
            "deep-equal(xs:dateTime('2012-05-30T12:00:00'), xs:dateTime('2012-05-30T12:00:00Z') - implicit-timezone())")
            .Evaluate(ctx);
        Assert.Equal("true", result.ToString());
    }

    [Fact]
    public void Number_ReturnsNaNForNonNumericNonStringTypes()
    {
        // K-NodeNumberFunc-13/15: fn:number on non-numeric, non-string, non-boolean atomic types returns NaN.
        Assert.Equal("NaN", Eval("number(xs:anyURI('1'))").ToString());
        Assert.Equal("NaN", Eval("number(xs:gYear('2005'))").ToString());

        // Boolean, numeric and string/untypedAtomic values still convert normally.
        Assert.Equal("1", Eval("number(true())").ToString());
        Assert.Equal("0", Eval("number(false())").ToString());
        Assert.Equal("1", Eval("number('1')").ToString());
        Assert.Equal("2", Eval("number(2)").ToString());
    }
}
