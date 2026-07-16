// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Unit tests for standard XPath / XQuery function implementations.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Added numeric and node-name accessor tests                                               |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Added date/time and fn:node-name tests                                                   |
//                      | Charles Korthout | 0.4   | 19-05-2026     | Added arrow inline-function and multi-binding FLWOR tests                                |
//                      | Charles Korthout | 0.5   | 19-05-2026     | Added fn:doc and fn:collection tests                                                     |
//                      | Charles Korthout | 0.6   | 19-05-2026     | Added substring-before, substring-after, codepoints, parse-xml tests                     |
//                      | Charles Korthout | 0.7   | 19-05-2026     | Added predicate indexing optimization tests                                              |
//                      | Charles Korthout | 0.8   | 19-05-2026     | Fixed predicate indexing tests for single-item results                                   |
//                      | Charles Korthout | 0.9   | 19-05-2026     | Added fn:analyze-string tests                                                          |
//                      | Charles Korthout | 1.0   | 19-05-2026     | Added document-node path traversal tests                                               |
//                      | Charles Korthout | 1.1   | 19-05-2026     | Added fn:serialize tests                                                               |
//                      | Charles Korthout | 1.2   | 19-05-2026     | Added fn:trace, fn:boolean, cardinality, fn:base-uri, fn:document-uri tests            |
//                      | Charles Korthout | 1.3   | 27-05-2026     | Added JSON function tests (parse-json, json-to-xml, xml-to-json, round-trip)            |
//                      | Charles Korthout | 1.4   | 27-05-2026     | Updated tokenize tests for spec-correct leading/trailing empty string preservation       |
//                      | Charles Korthout | 1.5   | 13-07-2026     | Allow current-time test day 1 or 2 when positive offset underflows DateTimeOffset.       |
//                      | Charles Korthout | 1.6   | 11-07-2026     | Added XSD char-class (Unicode 9.0), translate and codepoints-to-string astral tests      |
//                      | Charles Korthout | 1.7   | 14-07-2026     | Regression tests: LF in complement classes (NonBacktracking bug), fn:concat arity 16     |
//                      | Charles Korthout | 1.8   | 14-07-2026     | codepoints-to-string accepts XML 1.1 C0 controls (xml-to-json regression)                |
//                      | Charles Korthout | 1.9   | 15-07-2026     | QT3 regex quick wins: dot-vs-CR, \S, x flag, backref/empty-class FORX0002, tokenize captures/NBSP, translate XPTY0004
//                      | Charles Korthout | 2.0   | 15-07-2026     | ResourceUriMapper tests (doc/json-doc/unparsed-text) + FOJS0001 JSON parse error wrapping
//                      | Charles Korthout | 2.1   | 15-07-2026     | map:find tests (flat, nested maps/arrays, no-match, empty input)                          |
//                      | Charles Korthout | 2.2   | 15-07-2026     | xml-to-json F+O §32.2.2 tests: number reformat, FOJS0006 validation, escaped strings      |
//                      | Charles Korthout | 2.3   | 15-07-2026     | fn:min/fn:max tests: untypedAtomic→double, FORG0001/FORG0006, NaN propagation, duration   |
//                      | Charles Korthout | 2.4   | 15-07-2026     | fn:parse-json tests: empty input, duplicates modes, escape semantics, fallback rules      |
//                      | Charles Korthout | 2.5   | 15-07-2026     | json-to-xml tests: retain/use-first duplicates, escape attrs, surrogates, () input, eager option validation; parse-json quote decoding
//                      | Charles Korthout | 2.6   | 15-07-2026     | fn:serialize tests: xml declaration/standalone, item-separator, char maps, CDATA, element form, json/adaptive methods, SENR0001/SERE002x/SEPM001x/XQDY0137
//                      | Charles Korthout | 2.7   | 15-07-2026     | Tier-2i: map:merge options, strict keys, numeric/duration key equality, array bounds, FORG0006 EBV, map(K,V)/array(T)/function-type tests, deep-equal map collation |
//                      | Charles Korthout | 2.8   | 15-07-2026     | Tier-2j: FLWOR 'at $pos'/'where'/mixed chains, strict arithmetic/EBV/atomization, fn:sum/avg/numeric-fn strictness (40 tests) |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

public class FunctionLibraryTests
{
    private static XdmValue Evaluate(string xpath)
    {
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        return XPath31Expression.Compile(xpath).Evaluate(ctx);
    }

    private static string EvalStr(string xpath)
        => Evaluate(xpath).ToString();

    private static string[] EvalSequence(string xpath)
    {
        var result = Evaluate(xpath);
        Assert.True(result.IsSequence);
        var list = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            list.Add(item.ToString());
        return list.ToArray();
    }

    // ------------------------------------------------------------------
    // fn:string
    // ------------------------------------------------------------------

    [Fact]
    public void String_Integer() => Assert.Equal("42", EvalStr("fn:string(42)"));

    [Fact]
    public void String_Boolean() => Assert.Equal("true", EvalStr("fn:string(true())"));

    [Fact]
    public void String_EmptySequence() => Assert.Equal("", EvalStr("fn:string(())"));

    // ------------------------------------------------------------------
    // fn:concat
    // ------------------------------------------------------------------

    [Fact]
    public void Concat_TwoArgs() => Assert.Equal("ab", EvalStr("fn:concat('a','b')"));

    [Fact]
    public void Concat_ThreeArgs() => Assert.Equal("abc", EvalStr("fn:concat('a','b','c')"));

    [Fact]
    public void Concat_NumericArgs() => Assert.Equal("12", EvalStr("fn:concat(1,2)"));

    // ------------------------------------------------------------------
    // fn:string-length
    // ------------------------------------------------------------------

    [Fact]
    public void StringLength_Empty() => Assert.Equal("0", EvalStr("fn:string-length('')"));

    [Fact]
    public void StringLength_NonEmpty() => Assert.Equal("5", EvalStr("fn:string-length('hello')"));

    [Fact]
    public void StringLength_DefaultArg() => Assert.Equal("0", EvalStr("fn:string-length(())"));

    // ------------------------------------------------------------------
    // fn:substring
    // ------------------------------------------------------------------

    [Fact]
    public void Substring_Start1() => Assert.Equal("abc", EvalStr("fn:substring('abc',1)"));

    [Fact]
    public void Substring_Start2() => Assert.Equal("bc", EvalStr("fn:substring('abc',2)"));

    [Fact]
    public void Substring_WithLength() => Assert.Equal("ab", EvalStr("fn:substring('abc',1,2)"));

    [Fact]
    public void Substring_OutOfBounds() => Assert.Equal("", EvalStr("fn:substring('abc',10)"));

    // ------------------------------------------------------------------
    // fn:contains
    // ------------------------------------------------------------------

    [Fact]
    public void Contains_True() => Assert.Equal("true", EvalStr("fn:contains('hello','ell')"));

    [Fact]
    public void Contains_False() => Assert.Equal("false", EvalStr("fn:contains('hello','xyz')"));

    [Fact]
    public void Contains_EmptyNeedle() => Assert.Equal("true", EvalStr("fn:contains('hello','')"));

    // ------------------------------------------------------------------
    // fn:starts-with / fn:ends-with
    // ------------------------------------------------------------------

    [Fact]
    public void StartsWith_True() => Assert.Equal("true", EvalStr("fn:starts-with('hello','he')"));

    [Fact]
    public void StartsWith_False() => Assert.Equal("false", EvalStr("fn:starts-with('hello','lo')"));

    [Fact]
    public void EndsWith_True() => Assert.Equal("true", EvalStr("fn:ends-with('hello','lo')"));

    [Fact]
    public void EndsWith_False() => Assert.Equal("false", EvalStr("fn:ends-with('hello','he')"));

    // ------------------------------------------------------------------
    // fn:normalize-space
    // ------------------------------------------------------------------

    [Fact]
    public void NormalizeSpace_Trims() => Assert.Equal("a b", EvalStr("fn:normalize-space('  a  b  ')"));

    [Fact]
    public void NormalizeSpace_Empty() => Assert.Equal("", EvalStr("fn:normalize-space('')"));

    // ------------------------------------------------------------------
    // fn:upper-case / fn:lower-case
    // ------------------------------------------------------------------

    [Fact]
    public void UpperCase() => Assert.Equal("HELLO", EvalStr("fn:upper-case('hello')"));

    [Fact]
    public void LowerCase() => Assert.Equal("hello", EvalStr("fn:lower-case('HELLO')"));

    // ------------------------------------------------------------------
    // fn:matches
    // ------------------------------------------------------------------

    [Fact]
    public void Matches_Literal() => Assert.Equal("true", EvalStr("fn:matches('hello','ell')"));

    [Fact]
    public void Matches_False() => Assert.Equal("false", EvalStr("fn:matches('hello','xyz')"));

    // ------------------------------------------------------------------
    // fn:replace
    // ------------------------------------------------------------------

    [Fact]
    public void Replace() => Assert.Equal("hallo", EvalStr("fn:replace('hello','e','a')"));

    // ------------------------------------------------------------------
    // fn:count
    // ------------------------------------------------------------------

    [Fact]
    public void Count_Empty() => Assert.Equal("0", EvalStr("fn:count(())"));

    [Fact]
    public void Count_Sequence() => Assert.Equal("3", EvalStr("fn:count((1,2,3))"));

    [Fact]
    public void Count_Singleton() => Assert.Equal("1", EvalStr("fn:count(42)"));

    // ------------------------------------------------------------------
    // fn:sum
    // ------------------------------------------------------------------

    [Fact]
    public void Sum_Integers() => Assert.Equal("6", EvalStr("fn:sum((1,2,3))"));

    [Fact]
    public void Sum_Empty_DefaultZero() => Assert.Equal("0", EvalStr("fn:sum(())"));

    [Fact]
    public void Sum_Empty_CustomDefault() => Assert.Equal("42", EvalStr("fn:sum((),42)"));

    // ------------------------------------------------------------------
    // fn:avg
    // ------------------------------------------------------------------

    [Fact]
    public void Avg_Integers() => Assert.Equal("2", EvalStr("fn:avg((1,2,3))"));

    [Fact]
    public void Avg_Empty() => Assert.True(Evaluate("fn:avg(())").IsUndefined);

    // ------------------------------------------------------------------
    // fn:min / fn:max
    // ------------------------------------------------------------------

    [Fact]
    public void Min_Integers() => Assert.Equal("1", EvalStr("fn:min((3,1,2))"));

    [Fact]
    public void Max_Integers() => Assert.Equal("3", EvalStr("fn:max((1,3,2))"));

    [Fact]
    public void Min_Empty() => Assert.True(Evaluate("fn:min(())").IsUndefined);

    [Fact]
    public void Max_Empty() => Assert.True(Evaluate("fn:max(())").IsUndefined);

    [Fact]
    public void Max_UntypedAtomic_ReturnsDouble()
    {
        var result = Evaluate("fn:max(xs:untypedAtomic(\"3\"))");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.Equal(3.0, result.DoubleValue);
    }

    [Fact]
    public void Min_UntypedAtomicMixed_ReturnsDouble()
    {
        var result = Evaluate("fn:min((xs:untypedAtomic(\"3\"), 4, 5))");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.Equal(3.0, result.DoubleValue);
    }

    [Fact]
    public void Max_UntypedAtomicUncastable_RaisesFORG0001()
        => Assert.Contains("FORG0001", Assert.Throws<InvalidOperationException>(() => Evaluate("fn:max(xs:untypedAtomic(\"three\"))")).Message);

    [Fact]
    public void Max_StringMixedWithNumber_RaisesFORG0006()
        => Assert.Contains("FORG0006", Assert.Throws<InvalidOperationException>(() => Evaluate("fn:max((3, 4, \"Zero\"))")).Message);

    [Fact]
    public void Max_NaNWins()
        => Assert.Equal("NaN", EvalStr("fn:string(fn:max((3, xs:double(\"NaN\"))))"));

    [Fact]
    public void Min_NaNWins()
        => Assert.Equal("NaN", EvalStr("fn:string(fn:min((xs:float(\"NaN\"), 1, 2)))"));

    [Fact]
    public void Max_GenericDuration_RaisesFORG0006()
        => Assert.Contains("FORG0006", Assert.Throws<InvalidOperationException>(() => Evaluate("fn:max(xs:duration(\"P1Y1M1D\"))")).Message);

    [Fact]
    public void Max_DayTimeDurations_Orderable()
        => Assert.Equal("P2D", EvalStr("fn:max((xs:dayTimeDuration(\"P1D\"), xs:dayTimeDuration(\"P2D\")))"));

    // ------------------------------------------------------------------
    // fn:string-join
    // ------------------------------------------------------------------

    [Fact]
    public void StringJoin_CustomSeparator() => Assert.Equal("a-b-c", EvalStr("fn:string-join(('a','b','c'),'-')"));

    // ------------------------------------------------------------------
    // map:*
    // ------------------------------------------------------------------

    [Fact]
    public void MapGet() => Assert.Equal("1", EvalStr("map:get(map{'a':1},'a')"));

    [Fact]
    public void MapSize() => Assert.Equal("2", EvalStr("map:size(map{'a':1,'b':2})"));

    [Fact]
    public void MapContains_True() => Assert.Equal("true", EvalStr("map:contains(map{'a':1},'a')"));

    [Fact]
    public void MapContains_False() => Assert.Equal("false", EvalStr("map:contains(map{'a':1},'b')"));

    [Fact]
    public void MapFind_Flat() => Assert.Equal("1", EvalStr("array:get(map:find(map{'a':1,'b':2},'a'), 1)"));

    [Fact]
    public void MapFind_NestedMapsAndArrays()
    {
        // Searches maps and arrays at any depth; collects every value under the key.
        Assert.Equal("3", EvalStr("array:size(map:find((map{'a':1,'b':map{'a':2}}, [map{'a':3}, 42]), 'a'))"));
        Assert.Equal("2", EvalStr("array:get(map:find(map{'a':1,'b':map{'a':2}},'a'), 2)"));
    }

    [Fact]
    public void MapFind_NoMatch_ReturnsEmptyArray() => Assert.Equal("0", EvalStr("array:size(map:find(map{'a':1},'zz'))"));

    [Fact]
    public void MapFind_EmptyInput_ReturnsEmptyArray() => Assert.Equal("0", EvalStr("array:size(map:find((), 17))"));

    // ------------------------------------------------------------------
    // array:*
    // ------------------------------------------------------------------

    [Fact]
    public void ArrayGet() => Assert.Equal("2", EvalStr("array:get([1,2,3],2)"));

    [Fact]
    public void ArraySize() => Assert.Equal("3", EvalStr("array:size([1,2,3])"));

    [Fact]
    public void ArrayContains_True() => Assert.Equal("true", EvalStr("array:contains([1,2,3],2)"));

    [Fact]
    public void ArrayContains_False() => Assert.Equal("false", EvalStr("array:contains([1,2,3],99)"));

    [Fact]
    public void ArrayHead() => Assert.Equal("1", EvalStr("array:head([1,2,3])"));

    // ------------------------------------------------------------------
    // Sequence functions via materialization
    // ------------------------------------------------------------------

    [Fact]
    public void Reverse()
    {
        var items = EvalSequence("fn:reverse((1,2,3))");
        Assert.Equal(new[] { "3", "2", "1" }, items);
    }

    [Fact]
    public void Subsequence_Start()
    {
        var items = EvalSequence("fn:subsequence((1,2,3,4),2)");
        Assert.Equal(new[] { "2", "3", "4" }, items);
    }

    [Fact]
    public void DistinctValues()
    {
        var items = EvalSequence("fn:distinct-values((1,2,2,3,1))");
        Assert.Equal(3, items.Length);
        Assert.Contains("1", items);
        Assert.Contains("2", items);
        Assert.Contains("3", items);
    }

    [Fact]
    public void IndexOf_Found()
    {
        var items = EvalSequence("fn:index-of((1,2,2,3),2)");
        Assert.Equal(new[] { "2", "3" }, items);
    }

    [Fact]
    public void Remove_Middle()
    {
        var items = EvalSequence("fn:remove((1,2,3),2)");
        Assert.Equal(new[] { "1", "3" }, items);
    }

    [Fact]
    public void InsertBefore_Middle()
    {
        var items = EvalSequence("fn:insert-before((1,2,3),2,99)");
        Assert.Equal(new[] { "1", "99", "2", "3" }, items);
    }

    [Fact]
    public void Tokenize()
    {
        var items = EvalSequence("fn:tokenize('a,b,c',',')");
        Assert.Equal(new[] { "a", "b", "c" }, items);
    }

    [Fact]
    public void ArrayTail()
    {
        var result = Evaluate("array:tail([1,2,3])");
        Assert.True(result.IsArray);
    }

    [Fact]
    public void MapMerge()
    {
        var result = Evaluate("map:merge((map{'a':1},map{'b':2}))");
        Assert.True(result.IsMap);
    }

    // ------------------------------------------------------------------
    // Edge cases
    // ------------------------------------------------------------------

    [Fact]
    public void Replace_Global()
    {
        var result = EvalStr("fn:replace('a-b-c','-','_')");
        Assert.Equal("a_b_c", result);
    }

    // ------------------------------------------------------------------
    // fn:abs
    // ------------------------------------------------------------------

    [Fact]
    public void Abs_Integer() => Assert.Equal("42", EvalStr("fn:abs(-42)"));

    [Fact]
    public void Abs_Decimal()
    {
        var result = Evaluate("fn:abs(-3.14)");
        Assert.Equal(XdmValueKind.Decimal, result.Kind);
        Assert.Equal(3.14m, result.DecimalValue);
    }

    [Fact]
    public void Abs_Double()
    {
        var result = Evaluate("fn:abs(-2.5e0)");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.Equal(2.5, result.DoubleValue);
    }

    [Fact]
    public void Abs_Zero() => Assert.Equal("0", EvalStr("fn:abs(0)"));

    // ------------------------------------------------------------------
    // fn:floor
    // ------------------------------------------------------------------

    [Fact]
    public void Floor_Integer() => Assert.Equal("42", EvalStr("fn:floor(42)"));

    [Fact]
    public void Floor_Decimal() => Assert.Equal("3", EvalStr("fn:floor(3.14)"));

    [Fact]
    public void Floor_DecimalNegative() => Assert.Equal("-4", EvalStr("fn:floor(-3.14)"));

    [Fact]
    public void Floor_Double()
    {
        var result = Evaluate("fn:floor(2.5e0)");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.Equal(2.0, result.DoubleValue);
    }

    // ------------------------------------------------------------------
    // fn:ceiling
    // ------------------------------------------------------------------

    [Fact]
    public void Ceiling_Integer() => Assert.Equal("42", EvalStr("fn:ceiling(42)"));

    [Fact]
    public void Ceiling_Decimal() => Assert.Equal("4", EvalStr("fn:ceiling(3.14)"));

    [Fact]
    public void Ceiling_DecimalNegative() => Assert.Equal("-3", EvalStr("fn:ceiling(-3.14)"));

    [Fact]
    public void Ceiling_Double()
    {
        var result = Evaluate("fn:ceiling(2.5e0)");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.Equal(3.0, result.DoubleValue);
    }

    // ------------------------------------------------------------------
    // fn:round
    // ------------------------------------------------------------------

    [Fact]
    public void Round_Integer() => Assert.Equal("42", EvalStr("fn:round(42)"));

    [Fact]
    public void Round_Decimal() => Assert.Equal("3", EvalStr("fn:round(3.14)"));

    [Fact]
    public void Round_DecimalHalf() => Assert.Equal("4", EvalStr("fn:round(3.5)"));

    [Fact]
    public void Round_Double()
    {
        var result = Evaluate("fn:round(2.5e0)");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.Equal(3.0, result.DoubleValue); // round half away from zero
    }

    [Fact]
    public void Round_Precision()
    {
        var result = Evaluate("fn:round(3.1415, 2)");
        Assert.Equal(XdmValueKind.Decimal, result.Kind);
        Assert.Equal(3.14m, result.DecimalValue);
    }

    [Fact]
    public void Round_NegativePrecision() => Assert.Equal("0", EvalStr("fn:round(42, -2)"));

    // ------------------------------------------------------------------
    // fn:round-half-to-even
    // ------------------------------------------------------------------

    [Fact]
    public void RoundHalfToEven_Integer() => Assert.Equal("42", EvalStr("fn:round-half-to-even(42)"));

    [Fact]
    public void RoundHalfToEven_DecimalHalf() => Assert.Equal("2", EvalStr("fn:round-half-to-even(2.5)"));

    [Fact]
    public void RoundHalfToEven_DecimalOneAndHalf() => Assert.Equal("2", EvalStr("fn:round-half-to-even(1.5)"));

    [Fact]
    public void RoundHalfToEven_Precision()
    {
        var result = Evaluate("fn:round-half-to-even(3.1415, 2)");
        Assert.Equal(XdmValueKind.Decimal, result.Kind);
        Assert.Equal(3.14m, result.DecimalValue);
    }

    // ------------------------------------------------------------------
    // fn:local-name / fn:namespace-uri / fn:name
    // ------------------------------------------------------------------

    [Fact]
    public void LocalName_ContextItem()
    {
        var doc = new System.Xml.Linq.XElement("{http://example.com}root");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc);
        var result = XPath31Expression.Compile("fn:local-name()").Evaluate(node);
        Assert.Equal("root", result.ToString());
    }

    [Fact]
    public void LocalName_Argument()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><child/></root>");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("fn:local-name(child)").Evaluate(node);
        Assert.Equal("child", result.ToString());
    }

    [Fact]
    public void LocalName_EmptySequence() => Assert.Equal("", EvalStr("fn:local-name(())"));

    [Fact]
    public void NamespaceUri_ContextItem()
    {
        var doc = new System.Xml.Linq.XElement("{http://example.com}root");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc);
        var result = XPath31Expression.Compile("fn:namespace-uri()").Evaluate(node);
        Assert.Equal("http://example.com", result.ToString());
    }

    [Fact]
    public void NamespaceUri_NoNamespace()
    {
        var doc = new System.Xml.Linq.XElement("root");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc);
        var result = XPath31Expression.Compile("fn:namespace-uri()").Evaluate(node);
        Assert.Equal("", result.ToString());
    }

    [Fact]
    public void Name_ContextItem()
    {
        var doc = new System.Xml.Linq.XElement("{http://example.com}root");
        // XDocument nodes don't have prefixes set on elements created this way
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc);
        var result = XPath31Expression.Compile("fn:name()").Evaluate(node);
        Assert.Equal("root", result.ToString());
    }

    [Fact]
    public void Name_WithPrefix()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<ns:root xmlns:ns='http://example.com'><ns:child/></ns:root>");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var options = new Bosak.XPath.Api.CompileOptions
        {
            Namespaces = new Dictionary<string, string> { ["ns"] = "http://example.com" }
        };
        var ctx = new Bosak.XPath.Runtime.Vm.EvaluationContext()
            .WithFocus(Bosak.XPath.Core.Xdm.XdmValue.FromNode(node), 1, 1)
            .WithNamespace("ns", "http://example.com");
        var result = XPath31Expression.Compile("fn:name(ns:child)", options).Evaluate(ctx);
        Assert.Equal("ns:child", result.ToString());
    }

    [Fact]
    public void Name_DefaultNamespace()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root xmlns='http://default.com'><child/></root>");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        // Without a default element namespace declared in the static context,
        // unprefixed element names match no namespace. The child element is in
        // namespace http://default.com, so child::child returns empty sequence.
        var result = XPath31Expression.Compile("fn:name(child)").Evaluate(node);
        Assert.Equal("", result.ToString());
    }

    // ------------------------------------------------------------------
    // fn:current-dateTime / fn:current-date / fn:current-time
    // ------------------------------------------------------------------

    [Fact]
    public void CurrentDateTime_ReturnsDateTimeValue()
    {
        var result = Evaluate("fn:current-dateTime()");
        Assert.Equal(XdmValueKind.DateTime, result.Kind);
        var dto = result.DateTimeValue;
        Assert.True(dto <= DateTimeOffset.Now);
        Assert.True(dto > DateTimeOffset.Now.AddMinutes(-1));
    }

    [Fact]
    public void CurrentDate_ReturnsDateValue()
    {
        var result = Evaluate("fn:current-date()");
        Assert.Equal(XdmValueKind.Date, result.Kind);
        var dto = result.DateValue;
        var now = DateTimeOffset.Now;
        Assert.Equal(now.Year, dto.Year);
        Assert.Equal(now.Month, dto.Month);
        Assert.Equal(now.Day, dto.Day);
        Assert.Equal(0, dto.Hour);
        Assert.Equal(0, dto.Minute);
        Assert.Equal(0, dto.Second);
    }

    [Fact]
    public void CurrentTime_ReturnsTimeValue()
    {
        var result = Evaluate("fn:current-time()");
        Assert.Equal(XdmValueKind.Time, result.Kind);
        var dto = result.TimeValue;
        var now = DateTimeOffset.Now;
        Assert.Equal(1, dto.Year);
        Assert.Equal(1, dto.Month);
        // DateTimeOffset cannot represent year 0; with a positive offset the UTC instant may
        // underflow, so the implementation falls back to day 2 while preserving the time.
        Assert.True(dto.Day == 1 || dto.Day == 2, $"Expected day 1 or 2, got {dto.Day}");
        Assert.True(Math.Abs((now.TimeOfDay - dto.TimeOfDay).TotalMinutes) < 1);
    }

    // ------------------------------------------------------------------
    // fn:node-name
    // ------------------------------------------------------------------

    [Fact]
    public void NodeName_ContextItem()
    {
        var doc = new System.Xml.Linq.XElement("{http://example.com}root");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc);
        var result = XPath31Expression.Compile("fn:node-name()").Evaluate(node);
        Assert.Equal(XdmValueKind.QName, result.Kind);
        var qn = result.QNameValue;
        Assert.Equal("root", qn.LocalName);
        Assert.Equal("http://example.com", qn.NamespaceUri);
    }

    [Fact]
    public void NodeName_Argument()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><child/></root>");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("fn:node-name(child)").Evaluate(node);
        Assert.Equal(XdmValueKind.QName, result.Kind);
        var qn = result.QNameValue;
        Assert.Equal("child", qn.LocalName);
        Assert.Equal("", qn.NamespaceUri);
    }

    [Fact]
    public void NodeName_EmptySequence() => Assert.True(Evaluate("fn:node-name(())").IsUndefined);

    [Fact]
    public void NodeName_TextNode()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root>hello</root>");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("fn:node-name(child::text())").Evaluate(node);
        Assert.True(result.IsUndefined);
    }

    // ------------------------------------------------------------------
    // Namespace axis
    // ------------------------------------------------------------------

    [Fact]
    public void NamespaceAxis_InScopePrefixes()
    {
        var doc = System.Xml.Linq.XDocument.Parse(
            "<ns:root xmlns:ns='http://example.com' xmlns:other='http://other.com'><ns:child/></ns:root>");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("namespace::node()").Evaluate(node);
        var uris = new List<string>();
        var prefixes = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
        {
            uris.Add(item.ToString());
            prefixes.Add(item.NodeValue.LocalName);
        }
        Assert.Contains("http://example.com", uris);
        Assert.Contains("http://other.com", uris);
        Assert.Contains("http://www.w3.org/XML/1998/namespace", uris);
        Assert.Contains("ns", prefixes);
        Assert.Contains("other", prefixes);
        Assert.Contains("xml", prefixes);
    }

    [Fact]
    public void NamespaceAxis_DefaultNamespace()
    {
        var doc = System.Xml.Linq.XDocument.Parse(
            "<root xmlns='http://default.com'><child/></root>");
        var child = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!.Element(XName.Get("child", "http://default.com"))!);
        var result = XPath31Expression.Compile("namespace::node()").Evaluate(child);
        var uris = new List<string>();
        var prefixes = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
        {
            uris.Add(item.ToString());
            prefixes.Add(item.NodeValue.LocalName);
        }
        // Default namespace has empty prefix; namespace node string-value is the URI
        Assert.Contains("http://default.com", uris);
        Assert.Contains("", prefixes); // default prefix
    }

    [Fact]
    public void NamespaceAxis_Inherited()
    {
        var doc = System.Xml.Linq.XDocument.Parse(
            "<ns:root xmlns:ns='http://example.com'><ns:child xmlns:ns='http://override.com'/></ns:root>");
        var child = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!.Element(XName.Get("child", "http://override.com"))!);
        var result = XPath31Expression.Compile("namespace::node()").Evaluate(child);
        var uris = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            uris.Add(item.ToString());
        // Inner declaration overrides outer one
        Assert.Contains("http://override.com", uris);
        Assert.DoesNotContain("http://example.com", uris);
    }

    // ------------------------------------------------------------------
    // fn:number
    // ------------------------------------------------------------------

    [Fact]
    public void Number_Integer() => Assert.Equal("42", EvalStr("fn:number(42)"));

    [Fact]
    public void Number_String()
    {
        var result = Evaluate("fn:number('3.14')");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.Equal(3.14, result.DoubleValue);
    }

    [Fact]
    public void Number_InvalidString()
    {
        var result = Evaluate("fn:number('hello')");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.True(double.IsNaN(result.DoubleValue));
    }

    [Fact]
    public void Number_EmptySequence()
    {
        var result = Evaluate("fn:number(())");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.True(double.IsNaN(result.DoubleValue));
    }

    // ------------------------------------------------------------------
    // fn:data
    // ------------------------------------------------------------------

    [Fact]
    public void Data_AtomicPassthrough() => Assert.Equal("42", EvalStr("fn:data(42)"));

    [Fact]
    public void Data_Node()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root>hello</root>");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("fn:data(.)").Evaluate(node);
        Assert.Equal("hello", result.ToString());
    }

    [Fact]
    public void Data_Sequence()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><a>1</a><b>2</b></root>");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("fn:data(child::*)").Evaluate(node);
        Assert.True(result.IsSequence);
        var items = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item.ToString());
        Assert.Equal(new[] { "1", "2" }, items);
    }

    [Fact]
    public void Data_EmptySequence() => Assert.True(Evaluate("fn:data(())").IsUndefined);

    // ------------------------------------------------------------------
    // fn:root
    // ------------------------------------------------------------------

    [Fact]
    public void Root_ContextItem()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><child/></root>");
        var child = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!.Element("child")!);
        var result = XPath31Expression.Compile("fn:root()").Evaluate(child);
        Assert.Equal(XdmValueKind.Node, result.Kind);
        // fn:root() returns the document node, which has empty LocalName
        Assert.Equal(string.Empty, result.NodeValue.LocalName);
        Assert.Equal(XdmNodeKind.Document, result.NodeValue.NodeKind);
    }

    [Fact]
    public void Root_Argument()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><child/></root>");
        var root = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("fn:root(child)").Evaluate(root);
        Assert.Equal(XdmValueKind.Node, result.Kind);
        // fn:root() returns the document node, which has empty LocalName
        Assert.Equal(string.Empty, result.NodeValue.LocalName);
        Assert.Equal(XdmNodeKind.Document, result.NodeValue.NodeKind);
    }

    [Fact]
    public void Root_EmptySequence() => Assert.True(Evaluate("fn:root(())").IsUndefined);

    // ------------------------------------------------------------------
    // Date/time component extractors
    // ------------------------------------------------------------------

    [Fact]
    public void YearFromDateTime()
    {
        var result = Evaluate("fn:year-from-dateTime(fn:current-dateTime())");
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.True(result.IntegerValue >= 2026);
    }

    [Fact]
    public void MonthFromDateTime()
    {
        var result = Evaluate("fn:month-from-dateTime(fn:current-dateTime())");
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.True(result.IntegerValue is >= 1 and <= 12);
    }

    [Fact]
    public void DayFromDateTime()
    {
        var result = Evaluate("fn:day-from-dateTime(fn:current-dateTime())");
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.True(result.IntegerValue is >= 1 and <= 31);
    }

    [Fact]
    public void HoursFromDateTime()
    {
        var result = Evaluate("fn:hours-from-dateTime(fn:current-dateTime())");
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.True(result.IntegerValue is >= 0 and <= 23);
    }

    [Fact]
    public void MinutesFromDateTime()
    {
        var result = Evaluate("fn:minutes-from-dateTime(fn:current-dateTime())");
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.True(result.IntegerValue is >= 0 and <= 59);
    }

    [Fact]
    public void SecondsFromDateTime()
    {
        var result = Evaluate("fn:seconds-from-dateTime(fn:current-dateTime())");
        Assert.Equal(XdmValueKind.Decimal, result.Kind);
        Assert.True(result.DecimalValue is >= 0m and < 60m);
    }

    [Fact]
    public void YearFromDate()
    {
        var result = Evaluate("fn:year-from-date(fn:current-date())");
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.True(result.IntegerValue >= 2026);
    }

    [Fact]
    public void MonthFromDate()
    {
        var result = Evaluate("fn:month-from-date(fn:current-date())");
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.True(result.IntegerValue is >= 1 and <= 12);
    }

    [Fact]
    public void DayFromDate()
    {
        var result = Evaluate("fn:day-from-date(fn:current-date())");
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.True(result.IntegerValue is >= 1 and <= 31);
    }

    [Fact]
    public void HoursFromTime()
    {
        var result = Evaluate("fn:hours-from-time(fn:current-time())");
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.True(result.IntegerValue is >= 0 and <= 23);
    }

    [Fact]
    public void MinutesFromTime()
    {
        var result = Evaluate("fn:minutes-from-time(fn:current-time())");
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.True(result.IntegerValue is >= 0 and <= 59);
    }

    [Fact]
    public void SecondsFromTime()
    {
        var result = Evaluate("fn:seconds-from-time(fn:current-time())");
        Assert.Equal(XdmValueKind.Decimal, result.Kind);
        Assert.True(result.DecimalValue is >= 0m and < 60m);
    }

    // ------------------------------------------------------------------
    // fn:deep-equal
    // ------------------------------------------------------------------

    [Fact]
    public void DeepEqual_Integers() => Assert.Equal("true", EvalStr("fn:deep-equal(42,42)"));

    [Fact]
    public void DeepEqual_IntegersDifferent() => Assert.Equal("false", EvalStr("fn:deep-equal(42,99)"));

    [Fact]
    public void DeepEqual_Strings() => Assert.Equal("true", EvalStr("fn:deep-equal('hello','hello')"));

    [Fact]
    public void DeepEqual_Sequences()
    {
        Assert.Equal("true", EvalStr("fn:deep-equal((1,2,3),(1,2,3))"));
        Assert.Equal("false", EvalStr("fn:deep-equal((1,2,3),(1,2))"));
    }

    [Fact]
    public void DeepEqual_Nodes()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><child/></root>");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc);
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(node), 1, 1);
        FunctionLibrary.Populate(ctx);
        Assert.Equal("true", XPath31Expression.Compile("fn:deep-equal(root,root)").Evaluate(ctx).ToString());
    }

    [Fact]
    public void DeepEqual_Maps()
    {
        Assert.Equal("true", EvalStr("fn:deep-equal(map{'a':1},map{'a':1})"));
        Assert.Equal("false", EvalStr("fn:deep-equal(map{'a':1},map{'a':2})"));
    }

    [Fact]
    public void DeepEqual_Arrays()
    {
        Assert.Equal("true", EvalStr("fn:deep-equal([1,2],[1,2])"));
        Assert.Equal("false", EvalStr("fn:deep-equal([1,2],[1,3])"));
    }

    // ------------------------------------------------------------------
    // fn:generate-id
    // ------------------------------------------------------------------

    [Fact]
    public void GenerateId_ContextItem()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><child/></root>");
        var child = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!.Element("child")!);
        var result = XPath31Expression.Compile("fn:generate-id()").Evaluate(child);
        Assert.False(string.IsNullOrEmpty(result.ToString()));
        Assert.StartsWith("id", result.ToString());
    }

    [Fact]
    public void GenerateId_Argument()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><child/></root>");
        var root = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("fn:generate-id(child)").Evaluate(root);
        Assert.False(string.IsNullOrEmpty(result.ToString()));
        Assert.StartsWith("id", result.ToString());
    }

    [Fact]
    public void GenerateId_EmptySequence()
    {
        Assert.Equal("", EvalStr("fn:generate-id(())"));
    }

    // ------------------------------------------------------------------
    // fn:compare
    // ------------------------------------------------------------------

    [Fact]
    public void Compare_Less() => Assert.Equal("-1", EvalStr("fn:compare('a','b')"));

    [Fact]
    public void Compare_Equal() => Assert.Equal("0", EvalStr("fn:compare('a','a')"));

    [Fact]
    public void Compare_Greater() => Assert.Equal("1", EvalStr("fn:compare('b','a')"));

    // ------------------------------------------------------------------
    // URI encoding functions
    // ------------------------------------------------------------------

    [Fact]
    public void EncodeForUri() => Assert.Equal("hello%20world%2F%C3%A9", EvalStr("fn:encode-for-uri('hello world/é')"));

    [Fact]
    public void IriToUri() => Assert.Equal("hello%20world/%C3%A9?x=1", EvalStr("fn:iri-to-uri('hello world/é?x=1')"));

    [Fact]
    public void EscapeHtmlUri() => Assert.Equal("hello world/%C3%A9?x=1", EvalStr("fn:escape-html-uri('hello world/é?x=1')"));

    // ------------------------------------------------------------------
    // fn:QName / fn:resolve-QName
    // ------------------------------------------------------------------

    [Fact]
    public void Qname()
    {
        var result = Evaluate("fn:QName('http://example.com','ns:local')");
        Assert.Equal(XdmValueKind.QName, result.Kind);
        var qn = result.QNameValue;
        Assert.Equal("local", qn.LocalName);
        Assert.Equal("http://example.com", qn.NamespaceUri);
    }

    [Fact]
    public void Qname_NoPrefix()
    {
        var result = Evaluate("fn:QName('http://example.com','local')");
        Assert.Equal(XdmValueKind.QName, result.Kind);
        var qn = result.QNameValue;
        Assert.Equal("local", qn.LocalName);
        Assert.Equal("http://example.com", qn.NamespaceUri);
    }

    [Fact]
    public void ResolveQName()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<ns:root xmlns:ns='http://example.com'><ns:child/></ns:root>");
        var root = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("fn:resolve-QName('ns:child', .)").Evaluate(root);
        Assert.Equal(XdmValueKind.QName, result.Kind);
        var qn = result.QNameValue;
        Assert.Equal("child", qn.LocalName);
        Assert.Equal("http://example.com", qn.NamespaceUri);
    }

    [Fact]
    public void ResolveQName_NoPrefix()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root xmlns='http://default.com'><child/></root>");
        var root = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("fn:resolve-QName('child', .)").Evaluate(root);
        Assert.Equal(XdmValueKind.QName, result.Kind);
        var qn = result.QNameValue;
        Assert.Equal("child", qn.LocalName);
        Assert.Equal("http://default.com", qn.NamespaceUri);
    }

    // ------------------------------------------------------------------
    // Arrow operator
    // ------------------------------------------------------------------

    [Fact]
    public void Arrow_Single()
    {
        Assert.Equal("HELLO", EvalStr("'hello' => upper-case()"));
    }

    [Fact]
    public void Arrow_Chained()
    {
        Assert.Equal("HELLO WORLD", EvalStr("'  hello world  ' => normalize-space() => upper-case()"));
    }

    [Fact]
    public void Arrow_WithExistingArgument()
    {
        Assert.Equal("HELLO", EvalStr("'hello' => concat(' world') => substring(1, 5) => upper-case()"));
    }

    [Fact]
    public void Arrow_NumericFunction()
    {
        Assert.Equal("3", EvalStr("-3 => abs() => string()"));
    }

    [Fact]
    public void Arrow_VariableTarget()
    {
        Assert.Equal("HELLO", EvalStr("let $f := upper-case#1 return 'hello' => $f()"));
    }

    [Fact]
    public void Arrow_ParenthesizedTarget()
    {
        Assert.Equal("HELLO", EvalStr("let $f := upper-case#1 return 'hello' => ($f)()"));
    }

    [Fact]
    public void Arrow_InlineFunctionTarget()
    {
        Assert.Equal("6", EvalStr("5 => function($x) { $x + 1 }()"));
    }

    [Fact]
    public void Arrow_InlineFunctionTarget_WithArg()
    {
        Assert.Equal("15", EvalStr("10 => function($x, $y) { $x + $y }(5)"));
    }

    // ------------------------------------------------------------------
    // Quantified expressions
    // ------------------------------------------------------------------

    [Fact]
    public void Some_True()
    {
        Assert.Equal("true", EvalStr("some $x in (1, 2, 3) satisfies $x > 2"));
    }

    [Fact]
    public void Some_False()
    {
        Assert.Equal("false", EvalStr("some $x in (1, 2, 3) satisfies $x > 10"));
    }

    [Fact]
    public void Some_Empty()
    {
        Assert.Equal("false", EvalStr("some $x in () satisfies $x > 2"));
    }

    [Fact]
    public void Every_True()
    {
        Assert.Equal("true", EvalStr("every $x in (1, 2, 3) satisfies $x > 0"));
    }

    [Fact]
    public void Every_False()
    {
        Assert.Equal("false", EvalStr("every $x in (1, 2, 3) satisfies $x > 2"));
    }

    [Fact]
    public void Every_Empty()
    {
        Assert.Equal("true", EvalStr("every $x in () satisfies $x > 2"));
    }

    [Fact]
    public void ForExpression_Double()
    {
        var result = EvalSequence("for $x in (1, 2, 3) return $x * 2");
        Assert.Equal(3, result.Length);
        Assert.Equal("2", result[0]);
        Assert.Equal("4", result[1]);
        Assert.Equal("6", result[2]);
    }

    [Fact]
    public void ForExpression_Empty()
    {
        var result = EvalSequence("for $x in () return $x * 2");
        Assert.Empty(result);
    }

    [Fact]
    public void For_VariableDoesNotLeak()
    {
        // $x should not be visible after the for expression
        Assert.Throws<InvalidOperationException>(() => Evaluate("(for $x in (1, 2) return $x), $x"));
    }

    [Fact]
    public void Some_VariableDoesNotLeak()
    {
        // $x should not be visible after the some expression
        Assert.Throws<InvalidOperationException>(() => Evaluate("(some $x in (1, 2) satisfies $x > 0), $x"));
    }

    // ------------------------------------------------------------------
    // Multi-binding FLWOR
    // ------------------------------------------------------------------

    [Fact]
    public void ForExpression_MultiBinding_Cartesian()
    {
        var result = EvalSequence("for $x in (1, 2), $y in (3, 4) return $x + $y");
        Assert.Equal(new[] { "4", "5", "5", "6" }, result);
    }

    [Fact]
    public void ForExpression_MultiBinding_EmptySequence()
    {
        var result = EvalSequence("for $x in (1, 2), $y in () return $x + $y");
        Assert.Empty(result);
    }

    [Fact]
    public void ForExpression_MultiBinding_ThreeBindings()
    {
        var result = EvalSequence("for $x in (1), $y in (2), $z in (3) return $x + $y + $z");
        Assert.Equal(new[] { "6" }, result);
    }

    [Fact]
    public void Some_MultiBinding_True()
    {
        Assert.Equal("true", EvalStr("some $x in (1, 2), $y in (3, 4) satisfies $x < $y"));
    }

    [Fact]
    public void Some_MultiBinding_False()
    {
        Assert.Equal("false", EvalStr("some $x in (5, 6), $y in (1, 2) satisfies $x < $y"));
    }

    [Fact]
    public void Every_MultiBinding_True()
    {
        Assert.Equal("true", EvalStr("every $x in (1, 2), $y in (3, 4) satisfies $x < $y"));
    }

    [Fact]
    public void Every_MultiBinding_False()
    {
        Assert.Equal("false", EvalStr("every $x in (1, 5), $y in (3, 4) satisfies $x < $y"));
    }

    [Fact]
    public void For_MultiBinding_VariableDoesNotLeak()
    {
        Assert.Throws<InvalidOperationException>(() => Evaluate("(for $x in (1), $y in (2) return $x + $y), $x"));
    }

    [Fact]
    public void For_MultiBinding_VariableDoesNotLeak_Y()
    {
        Assert.Throws<InvalidOperationException>(() => Evaluate("(for $x in (1), $y in (2) return $x + $y), $y"));
    }

    // ------------------------------------------------------------------
    // Regex functions
    // ------------------------------------------------------------------

    [Fact]
    public void Matches_BasicTrue() => Assert.Equal("true", EvalStr("matches('hello', 'e')"));

    [Fact]
    public void Matches_BasicFalse() => Assert.Equal("false", EvalStr("matches('hello', 'z')"));

    [Fact]
    public void Matches_Anchor() => Assert.Equal("false", EvalStr("matches('hello', '^e$')"));

    [Fact]
    public void Matches_FlagsCaseInsensitive() => Assert.Equal("true", EvalStr("matches('HELLO', 'hello', 'i')"));

    [Fact]
    public void Matches_FlagsDotAll() => Assert.Equal("true", EvalStr("matches('a\nb', 'a.b', 's')"));

    [Fact]
    public void Matches_FlagsQuoteMode() => Assert.Equal("true", EvalStr("matches('a.b', 'a.b', 'q')"));

    [Fact]
    public void Replace_Basic() => Assert.Equal("aXc", EvalStr("replace('abc', 'b', 'X')"));

    [Fact]
    public void Replace_CaptureGroup() => Assert.Equal("b-a", EvalStr("replace('a-b', '(.+)-(.+)', '$2-$1')"));

    [Fact]
    public void Replace_FlagsQuoteMode() => Assert.Equal("aXb", EvalStr("replace('a.b', '.', 'X', 'q')"));

    // ------------------------------------------------------------------
    // XSD character classes (Unicode 9.0 pinned)
    // ------------------------------------------------------------------

    private const string AdlamCap = "\U0001E900";   // Lu, new in Unicode 9.0
    private const string AdlamDigit = "\U0001E950"; // Nd, new in Unicode 9.0
    private const string Emoji = "\U0001F600";      // So
    private const string Tangut = "\U00017000";     // Lo, new in Unicode 9.0

    [Fact]
    public void Matches_Category_AstralLetter()
        => Assert.Equal("true", EvalStr("matches('" + AdlamCap + "', '\\p{Lu}')"));

    [Fact]
    public void Matches_Category_AstralDigit()
        => Assert.Equal("true", EvalStr("matches('" + AdlamDigit + "', '\\d')"));

    [Fact]
    public void Matches_Category_SymbolIsWordChar()
        => Assert.Equal("true", EvalStr("matches('" + Emoji + "', '\\w')"));

    [Fact]
    public void Matches_Category_ComplementAstral()
    {
        Assert.Equal("false", EvalStr("matches('" + AdlamCap + "', '\\P{L}')"));
        Assert.Equal("true", EvalStr("matches('" + Emoji + "', '\\P{L}')"));
    }

    [Fact]
    public void Matches_Block_Adlam()
    {
        Assert.Equal("true", EvalStr("matches('" + AdlamCap + "', '\\p{IsAdlam}')"));
        Assert.Equal("false", EvalStr("matches('A', '\\p{IsAdlam}')"));
        Assert.Equal("true", EvalStr("matches('A', '\\P{IsAdlam}')"));
    }

    [Fact]
    public void Matches_Block_UnknownThrows()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("matches('A', '\\p{IsFoo}')"));
        Assert.Contains("FORX0002", ex.Message);
    }

    [Fact]
    public void Matches_Category_UnknownThrows()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("matches('A', '\\p{Xx}')"));
        Assert.Contains("FORX0002", ex.Message);
    }

    [Fact]
    public void Matches_ClassSubtraction_Empty()
        => Assert.Equal("false", EvalStr("matches('A', '[\\p{L}-[\\p{L}]]')"));

    [Fact]
    public void Matches_ClassSubtraction_Range()
    {
        Assert.Equal("true", EvalStr("matches('b', '[a-z-[x]]')"));
        Assert.Equal("false", EvalStr("matches('x', '[a-z-[x]]')"));
    }

    [Fact]
    public void Matches_NegatedComplement()
    {
        Assert.Equal("true", EvalStr("matches('a', '[^\\P{Ll}]')"));
        Assert.Equal("false", EvalStr("matches('A', '[^\\P{Ll}]')"));
    }

    [Fact]
    public void Matches_AnchoredAstral()
        => Assert.Equal("true", EvalStr("matches('A" + AdlamCap + "z', '^\\p{L}+$')"));

    [Fact]
    public void Matches_BackreferenceAstral()
        => Assert.Equal("true", EvalStr("matches('" + AdlamCap + AdlamCap + "', '^(\\p{Lu})\\1$')"));

    [Fact]
    public void Replace_AstralClass()
        => Assert.Equal("x#y", EvalStr("replace('x" + AdlamDigit + "y', '\\p{Nd}', '#')"));

    [Fact]
    public void Matches_AdlamBlockCount()
        => Assert.Equal("96", EvalStr("count(((125184 to 125279) ! codepoints-to-string(.))[matches(., '\\p{IsAdlam}')])"));

    // Regression: RegexOptions.NonBacktracking silently failed to match U+000A (LF) on the
    // large translated complement classes; the regex cache now always uses Compiled.
    [Fact]
    public void Matches_ComplementContainsLineFeed()
    {
        Assert.Equal("true", EvalStr("matches('\n', '\\P{Ll}')"));
        Assert.Equal("true", EvalStr("matches('\n', '[\\p{Ll}]|[\\P{Ll}]')"));
        Assert.Equal("**", EvalStr("replace('\n', '\\P{Ll}', '**')"));
    }

    // Regression: fn:concat is variadic; arities above 13 must resolve (unicode-90 uses #16).
    [Fact]
    public void Concat_Arity16()
        => Assert.Equal("abcdefghijklmnop",
            EvalStr("concat('a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p')"));

    // ------------------------------------------------------------------
    // fn:translate code-point semantics
    // ------------------------------------------------------------------

    [Fact]
    public void Translate_AstralSource()
        => Assert.Equal("yx", EvalStr("translate('" + AdlamCap + "x', '" + AdlamCap + "', 'y')"));

    [Fact]
    public void Translate_AstralDelete()
        => Assert.Equal("xy", EvalStr("translate('x" + AdlamCap + "y', '" + AdlamCap + "', '')"));

    // ------------------------------------------------------------------
    // fn:codepoints-to-string XML 1.1 Char validity
    // ------------------------------------------------------------------

    [Fact]
    public void CodepointsToString_AllowsNonCharacters()
    {
        // FDD0-FDEF and astral code points ending in FFFE/FFFF are valid XML 1.1 characters.
        Assert.Equal("64976", EvalStr("string-to-codepoints(codepoints-to-string(64976))[1]"));
        Assert.Equal("131070", EvalStr("string-to-codepoints(codepoints-to-string(131070))[1]"));
        Assert.Equal("1114111", EvalStr("string-to-codepoints(codepoints-to-string(1114111))[1]"));
    }

    [Fact]
    public void CodepointsToString_AllowsXml11C0Controls()
    {
        // Bosak is XML 1.1-capable (Xml11Loader): C0 controls except NUL are valid characters
        // (xml-to-json serializes e.g. backspace/form-feed as JSON escapes).
        Assert.Equal("8", EvalStr("string-to-codepoints(codepoints-to-string(8))[1]"));
        Assert.Equal("12", EvalStr("string-to-codepoints(codepoints-to-string(12))[1]"));
    }

    [Fact]
    public void CodepointsToString_RejectsNonChars()
    {
        Assert.Contains("FOCH0001", Assert.Throws<InvalidOperationException>(() => Evaluate("codepoints-to-string(65534)")).Message);
        Assert.Contains("FOCH0001", Assert.Throws<InvalidOperationException>(() => Evaluate("codepoints-to-string(65535)")).Message);
        Assert.Contains("FOCH0001", Assert.Throws<InvalidOperationException>(() => Evaluate("codepoints-to-string(55296)")).Message);
        Assert.Contains("FOCH0001", Assert.Throws<InvalidOperationException>(() => Evaluate("codepoints-to-string(0)")).Message);
    }


    [Fact]
    public void Tokenize_Basic()
    {
        var result = EvalSequence("tokenize('a,b,c', ',')");
        Assert.Equal(3, result.Length);
        Assert.Equal("a", result[0]);
        Assert.Equal("b", result[1]);
        Assert.Equal("c", result[2]);
    }

    [Fact]
    public void Tokenize_EmptyInput()
    {
        var result = EvalSequence("tokenize('', ',')");
        Assert.Empty(result);
    }

    [Fact]
    public void Tokenize_LeadingTrailingSeparators()
    {
        var result = EvalSequence("tokenize(',a,b,c,', ',')");
        Assert.Equal(5, result.Length);
        Assert.Equal("", result[0]);
        Assert.Equal("a", result[1]);
        Assert.Equal("b", result[2]);
        Assert.Equal("c", result[3]);
        Assert.Equal("", result[4]);
    }

    [Fact]
    public void Tokenize_DoubleSeparators()
    {
        var result = EvalSequence("tokenize('a,,b', ',')");
        Assert.Equal(3, result.Length);
        Assert.Equal("a", result[0]);
        Assert.Equal("", result[1]);
        Assert.Equal("b", result[2]);
    }

    [Fact]
    public void Tokenize_Whitespace()
    {
        var result = EvalSequence("tokenize('  a  b  ', '\\s+')");
        Assert.Equal(4, result.Length);
        Assert.Equal("", result[0]);
        Assert.Equal("a", result[1]);
        Assert.Equal("b", result[2]);
        Assert.Equal("", result[3]);
    }

    // ------------------------------------------------------------------
    // Casting and type system
    // ------------------------------------------------------------------

    [Fact]
    public void CastAs_String() => Assert.Equal("42", EvalStr("42 cast as xs:string"));

    [Fact]
    public void CastAs_Integer() => Assert.Equal("42", EvalStr("'42' cast as xs:integer"));

    [Fact]
    public void CastAs_Decimal()
    {
        var result = Evaluate("'3.14' cast as xs:decimal");
        Assert.Equal(XdmValueKind.Decimal, result.Kind);
        Assert.Equal(3.14m, result.DecimalValue);
    }

    [Fact]
    public void CastAs_Double()
    {
        var result = Evaluate("'3.14' cast as xs:double");
        Assert.Equal(XdmValueKind.Double, result.Kind);
        Assert.Equal(3.14, result.DoubleValue, precision: 2);
    }

    [Fact]
    public void CastAs_Boolean() => Assert.Equal("true", EvalStr("1 cast as xs:boolean"));

    [Fact]
    public void CastAs_DateTime()
    {
        var result = Evaluate("'2024-01-15T10:30:00' cast as xs:dateTime");
        Assert.Equal(XdmValueKind.DateTime, result.Kind);
        var dt = result.DateTimeValue;
        Assert.Equal(2024, dt.Year);
        Assert.Equal(1, dt.Month);
        Assert.Equal(15, dt.Day);
        Assert.Equal(10, dt.Hour);
        Assert.Equal(30, dt.Minute);
    }

    [Fact]
    public void CastAs_Date()
    {
        var result = Evaluate("'2024-01-15' cast as xs:date");
        Assert.Equal(XdmValueKind.Date, result.Kind);
        Assert.Equal(new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero).Date, result.DateValue.Date);
    }

    [Fact]
    public void CastAs_Time()
    {
        var result = Evaluate("'10:30:00' cast as xs:time");
        Assert.Equal(XdmValueKind.Time, result.Kind);
        var dt = result.TimeValue;
        Assert.Equal(10, dt.Hour);
        Assert.Equal(30, dt.Minute);
        Assert.Equal(0, dt.Second);
    }

    [Fact]
    public void CastAs_DateTimeToDate()
    {
        var result = Evaluate("xs:dateTime('2024-01-15T10:30:00') cast as xs:date");
        Assert.Equal(XdmValueKind.Date, result.Kind);
        Assert.Equal(new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero).Date, result.DateValue.Date);
    }

    [Fact]
    public void CastableAs_True() => Assert.Equal("true", EvalStr("'42' castable as xs:integer"));

    [Fact]
    public void CastableAs_False() => Assert.Equal("false", EvalStr("'hello' castable as xs:integer"));

    [Fact]
    public void InstanceOf_Integer() => Assert.Equal("true", EvalStr("42 instance of xs:integer"));

    [Fact]
    public void InstanceOf_String() => Assert.Equal("true", EvalStr("'hello' instance of xs:string"));

    [Fact]
    public void InstanceOf_Boolean() => Assert.Equal("true", EvalStr("true() instance of xs:boolean"));

    [Fact]
    public void Constructor_XsInteger() => Assert.Equal("42", EvalStr("xs:integer('42')"));

    [Fact]
    public void Constructor_XsString() => Assert.Equal("42", EvalStr("xs:string(42)"));

    [Fact]
    public void Constructor_XsBoolean() => Assert.Equal("true", EvalStr("xs:boolean(1)"));

    [Fact]
    public void Constructor_XsDate()
    {
        var result = Evaluate("xs:date('2024-06-01')");
        Assert.Equal(XdmValueKind.Date, result.Kind);
        Assert.Equal(new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero).Date, result.DateValue.Date);
    }

    // ------------------------------------------------------------------
    // Math functions
    // ------------------------------------------------------------------

    [Fact]
    public void Math_Pi()
    {
        var result = Evaluate("math:pi()");
        Assert.Equal(Math.PI, result.DoubleValue, precision: 10);
    }

    [Fact]
    public void Math_Sin() => Assert.Equal("0", EvalStr("math:sin(0)"));

    [Fact]
    public void Math_Cos() => Assert.Equal("1", EvalStr("math:cos(0)"));

    [Fact]
    public void Math_Tan() => Assert.Equal("0", EvalStr("math:tan(0)"));

    [Fact]
    public void Math_Pow() => Assert.Equal("8", EvalStr("math:pow(2, 3)"));

    [Fact]
    public void Math_Sqrt() => Assert.Equal("3", EvalStr("math:sqrt(9)"));

    [Fact]
    public void Math_Exp() => Assert.Equal("1", EvalStr("math:exp(0)"));

    [Fact]
    public void Math_Log() => Assert.Equal("0", EvalStr("math:log(1)"));

    // ------------------------------------------------------------------
    // function-lookup
    // ------------------------------------------------------------------

    [Fact]
    public void FunctionLookup_Found()
    {
        Assert.Equal("7", EvalStr("let $f := function-lookup(QName('http://www.w3.org/2005/xpath-functions', 'abs'), 1) return $f(-7)"));
    }

    [Fact]
    public void FunctionLookup_NotFound()
    {
        var result = Evaluate("function-lookup(QName('http://www.w3.org/2005/xpath-functions', 'nonexistent'), 1)");
        Assert.True(result.IsUndefined);
    }

    // ------------------------------------------------------------------
    // Try/Catch
    // ------------------------------------------------------------------

    [Fact]
    public void TryCatch_Success()
    {
        Assert.Equal("42", EvalStr("try { 42 } catch * { 0 }"));
    }

    [Fact]
    public void TryCatch_CatchesError()
    {
        Assert.Equal("fallback", EvalStr("try { 'hello' cast as xs:integer } catch * { 'fallback' }"));
    }

    [Fact]
    public void TryCatch_AccessErrorCode()
    {
        var result = EvalStr("try { 'x' cast as xs:integer } catch * { $err:code }");
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void TryCatch_AccessErrorDescription()
    {
        var result = EvalStr("try { 'x' cast as xs:integer } catch * { $err:description }");
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void TryCatch_Nested()
    {
        Assert.Equal("outer", EvalStr("try { try { 'x' cast as xs:integer } catch * { 1 div 0 } } catch * { 'outer' }"));
    }

    [Fact]
    public void Error_Caught()
    {
        Assert.Equal("caught", EvalStr("try { fn:error() } catch * { 'caught' }"));
    }

    [Fact]
    public void Error_WithDescription()
    {
        var result = EvalStr("try { fn:error(QName('http://www.w3.org/2005/xqt-errors', 'FOER0000'), 'boom') } catch * { $err:description }");
        Assert.Contains("boom", result);
    }

    // ------------------------------------------------------------------
    // Higher-order functions
    // ------------------------------------------------------------------

    [Fact]
    public void ForEach_Double()
    {
        var result = EvalSequence("for-each((1, 2, 3), function($x) { $x * 2 })");
        Assert.Equal(3, result.Length);
        Assert.Equal("2", result[0]);
        Assert.Equal("4", result[1]);
        Assert.Equal("6", result[2]);
    }

    [Fact]
    public void ForEach_NamedFunction()
    {
        var result = EvalSequence("for-each((-1, -2, -3), abs#1)");
        Assert.Equal(3, result.Length);
        Assert.Equal("1", result[0]);
        Assert.Equal("2", result[1]);
        Assert.Equal("3", result[2]);
    }

    [Fact]
    public void Filter_GreaterThanTwo()
    {
        var result = EvalSequence("filter((1, 2, 3, 4), function($x) { $x > 2 })");
        Assert.Equal(2, result.Length);
        Assert.Equal("3", result[0]);
        Assert.Equal("4", result[1]);
    }

    [Fact]
    public void FoldLeft_Sum()
    {
        var result = Evaluate("fold-left((1, 2, 3, 4), 0, function($a, $b) { $a + $b })");
        Assert.Equal(10, result.IntegerValue);
    }

    [Fact]
    public void FoldRight_StringConcat()
    {
        var result = EvalStr("fold-right((1, 2, 3), '', function($a, $b) { concat($a, '-', $b) })");
        Assert.Equal("1-2-3-", result);
    }

    [Fact]
    public void ForEachPair_SumPairs()
    {
        var result = EvalSequence("for-each-pair((1, 2, 3), (10, 20, 30), function($a, $b) { $a + $b })");
        Assert.Equal(3, result.Length);
        Assert.Equal("11", result[0]);
        Assert.Equal("22", result[1]);
        Assert.Equal("33", result[2]);
    }

    [Fact]
    public void ForEachPair_DifferentLengths()
    {
        var result = EvalSequence("for-each-pair((1, 2, 3), (10, 20), function($a, $b) { $a + $b })");
        Assert.Equal(2, result.Length);
        Assert.Equal("11", result[0]);
        Assert.Equal("22", result[1]);
    }

    // ------------------------------------------------------------------
    // Predicate indexing optimization correctness
    // ------------------------------------------------------------------

    private static string[] MaterializeResult(string xpath)
    {
        var result = Evaluate(xpath);
        if (result.IsUndefined)
            return Array.Empty<string>();
        if (!result.IsSequence)
            return new[] { result.ToString() };
        var list = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            list.Add(item.ToString());
        return list.ToArray();
    }

    [Fact]
    public void PredicateIndexing_Subscript()
    {
        Assert.Equal("10", MaterializeResult("(10, 20, 30)[1]")[0]);
        Assert.Equal("20", MaterializeResult("(10, 20, 30)[2]")[0]);
        Assert.Equal("30", MaterializeResult("(10, 20, 30)[3]")[0]);
        Assert.Empty(MaterializeResult("(10, 20, 30)[4]"));
        Assert.Empty(MaterializeResult("(10, 20, 30)[0]"));
    }

    [Fact]
    public void PredicateIndexing_OnAtomic()
    {
        Assert.Equal("42", MaterializeResult("42[1]")[0]);
        Assert.Empty(MaterializeResult("42[2]"));
    }

    [Fact]
    public void PredicateIndexing_EmptySequence()
    {
        Assert.Empty(MaterializeResult("()[1]"));
        Assert.Empty(MaterializeResult("()[last()]"));
    }

    [Fact]
    public void PredicateIndexing_Last()
    {
        Assert.Equal("30", MaterializeResult("(10, 20, 30)[last()]")[0]);
        Assert.Equal("10", MaterializeResult("(10)[last()]")[0]);
    }

    // ------------------------------------------------------------------
    // fn:analyze-string
    // ------------------------------------------------------------------

    [Fact]
    public void AnalyzeString_BasicMatch()
    {
        var result = Evaluate("analyze-string('The cat sat on the mat', 'c.t')");
        Assert.True(result.IsNode);
        var node = result.NodeValue;
        Assert.Equal("analyze-string-result", node.LocalName);
        Assert.Equal("http://www.w3.org/2005/xpath-functions", node.NamespaceUri);
    }

    [Fact]
    public void AnalyzeString_NoMatch()
    {
        var result = Evaluate("analyze-string('hello world', 'xyz')");
        Assert.True(result.IsNode);
        var children = new List<string>();
        foreach (var child in result.NodeValue.Children())
            children.Add(child.NodeValue.LocalName);
        Assert.Single(children);
        Assert.Equal("non-match", children[0]);
    }

    [Fact]
    public void AnalyzeString_EmptyInput()
    {
        var result = Evaluate("analyze-string('', 'test')");
        Assert.True(result.IsNode);
        var children = new List<string>();
        foreach (var child in result.NodeValue.Children())
            children.Add(child.NodeValue.LocalName);
        Assert.Empty(children);
    }

    [Fact]
    public void AnalyzeString_WithGroups()
    {
        var result = Evaluate("analyze-string('abc123def', '([a-z]+)([0-9]+)')");
        Assert.True(result.IsNode);
        var children = new List<string>();
        foreach (var child in result.NodeValue.Children())
            children.Add(child.NodeValue.LocalName);
        Assert.Equal(2, children.Count); // match (abc123) + non-match (def)
        Assert.Equal("match", children[0]);
        Assert.Equal("non-match", children[1]);
    }

    [Fact]
    public void AnalyzeString_WithFlags()
    {
        var result = Evaluate("analyze-string('HELLO hello', 'hello', 'i')");
        Assert.True(result.IsNode);
        var children = new List<string>();
        foreach (var child in result.NodeValue.Children())
            children.Add(child.NodeValue.LocalName);
        Assert.Equal(3, children.Count); // match (HELLO) + non-match ( ) + match (hello)
        Assert.Equal("match", children[0]);
        Assert.Equal("non-match", children[1]);
        Assert.Equal("match", children[2]);
    }

    // ------------------------------------------------------------------
    // fn:serialize
    // ------------------------------------------------------------------

    [Fact]
    public void Serialize_Element()
    {
        var result = EvalStr("serialize(parse-xml('<root><item>hello</item></root>')/root)");
        Assert.Equal("<root><item>hello</item></root>", result);
    }

    [Fact]
    public void Serialize_Document()
    {
        var result = EvalStr("serialize(parse-xml('<root><item>hello</item></root>'))");
        Assert.Equal("<root><item>hello</item></root>", result);
    }

    [Fact]
    public void Serialize_Atomic()
    {
        Assert.Equal("42", EvalStr("serialize(42)"));
        Assert.Equal("hello", EvalStr("serialize('hello')"));
    }

    [Fact]
    public void Serialize_EmptySequence()
    {
        Assert.Equal("", EvalStr("serialize(())"));
    }

    // ------------------------------------------------------------------
    // fn:serialize — Serialization 3.1 parameters and output methods
    // ------------------------------------------------------------------

    [Fact]
    public void Serialize_FreeStandingAttribute_SENR0001()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("serialize((parse-xml('<a x=\"1\"/>')//@*)[1])"));
        Assert.Contains("SENR0001", ex.Message);
    }

    [Fact]
    public void Serialize_FunctionItem_SENR0001()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("serialize(name#1)"));
        Assert.Contains("SENR0001", ex.Message);
    }

    [Fact]
    public void Serialize_MapConstructorDuplicateKey_XQDY0137()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("map{'indent':true(),'indent':true()}"));
        Assert.Contains("XQDY0137", ex.Message);
    }

    [Fact]
    public void Serialize_XmlDeclaration()
    {
        Assert.Contains("<?xml", EvalStr("serialize(parse-xml('<e/>'), map{'omit-xml-declaration':false()})"));
        Assert.Contains("standalone=\"no\"",
            EvalStr("serialize(parse-xml('<e/>'), map{'omit-xml-declaration':false(),'standalone':false()})"));
        Assert.Contains("standalone=\"yes\"",
            EvalStr("serialize(parse-xml('<e/>'), map{'omit-xml-declaration':false(),'standalone':true()})"));
    }

    [Fact]
    public void Serialize_ItemSeparator()
    {
        Assert.Equal("1|2|3", EvalStr("serialize(1 to 3, map{'method':'xml','item-separator':'|'})"));
        Assert.Equal("<e/>  <f/>",
            EvalStr("serialize(parse-xml('<x><e/><f/></x>')/x/*, map{'item-separator':'  '})"));
    }

    [Fact]
    public void Serialize_CharacterMap()
    {
        Assert.Equal("<e>a£b</e>",
            EvalStr("serialize(parse-xml('<e>a$b</e>'), map{'use-character-maps':map{'$':'£'}})"));
    }

    [Fact]
    public void Serialize_CharacterMapMultiCharKey_SEPM0016()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("serialize(parse-xml('<e/>'), map{'use-character-maps':map{'$$':'£'}})"));
        Assert.Contains("SEPM0016", ex.Message);
    }

    [Fact]
    public void Serialize_CharacterMapWrongTypes_XPTY0004()
    {
        Assert.Throws<InvalidOperationException>(
            () => Evaluate("serialize(parse-xml('<e/>'), map{'use-character-maps':true()})"));
        Assert.Throws<InvalidOperationException>(
            () => Evaluate("serialize(parse-xml('<e/>'), map{'use-character-maps':map{'x':xs:untypedAtomic('j')}})"));
    }

    [Fact]
    public void Serialize_CdataSectionElements()
    {
        Assert.Equal("<doc><b><![CDATA[bold]]></b><i>italic</i></doc>",
            EvalStr("serialize(parse-xml('<doc><b>bold</b><i>italic</i></doc>'), map{'cdata-section-elements':QName('','b')})"));
    }

    [Fact]
    public void Serialize_UntypedAtomicOptionConversion()
    {
        Assert.Equal("<e/>  <f/>",
            EvalStr("serialize(parse-xml('<x><e/><f/></x>')/x/*, map{'indent':xs:untypedAtomic('false'),'item-separator':xs:untypedAtomic('  ')})"));
    }

    [Fact]
    public void Serialize_WrongOptionType_XPTY0004()
    {
        Assert.Throws<InvalidOperationException>(
            () => Evaluate("serialize(parse-xml('<e/>'), map{'indent':'yes'})"));
        Assert.Throws<InvalidOperationException>(
            () => Evaluate("serialize(parse-xml('<e/>'), map{'standalone':' omit '})"));
    }

    [Fact]
    public void Serialize_QNameKeyAndUnknownKeyIgnored()
    {
        // Absent-namespace QName keys and unknown string keys are both ignored.
        Assert.Equal("<e><f/></e>",
            EvalStr("serialize(parse-xml('<e><f/></e>'), map{QName('','indent'):true(),'xindent':true()})"));
    }

    [Fact]
    public void Serialize_EmptySequenceOptionKeepsDefault()
    {
        Assert.DoesNotContain("standalone",
            EvalStr("serialize(parse-xml('<e/>'), map{'omit-xml-declaration':false(),'standalone':()})"));
    }

    [Fact]
    public void Serialize_ElementForm()
    {
        const string paramsDoc = "'<output:serialization-parameters xmlns:output=\"http://www.w3.org/2010/xslt-xquery-serialization\">"
                                 + "<output:method value=\"xml\"/><output:item-separator value=\"|\"/></output:serialization-parameters>'";
        Assert.Equal("1|2|3", EvalStr($"serialize(1 to 3, parse-xml({paramsDoc}))"));
    }

    [Fact]
    public void Serialize_ElementFormValidation()
    {
        // Bad boolean lexical → SEPM0017
        var ex1 = Assert.Throws<InvalidOperationException>(() => Evaluate(
            "serialize(parse-xml('<e/>'), parse-xml('<output:serialization-parameters xmlns:output=\"http://www.w3.org/2010/xslt-xquery-serialization\"><output:indent value=\"maybe\"/></output:serialization-parameters>'))"));
        Assert.Contains("SEPM0017", ex1.Message);
        // Duplicate parameter element → SEPM0019
        var ex2 = Assert.Throws<InvalidOperationException>(() => Evaluate(
            "serialize(parse-xml('<e/>'), parse-xml('<output:serialization-parameters xmlns:output=\"http://www.w3.org/2010/xslt-xquery-serialization\"><output:indent value=\"yes\"/><output:indent value=\"no\"/></output:serialization-parameters>'))"));
        Assert.Contains("SEPM0019", ex2.Message);
        // Duplicate character map entry → SEPM0018
        var ex3 = Assert.Throws<InvalidOperationException>(() => Evaluate(
            "serialize(parse-xml('<e/>'), parse-xml('<output:serialization-parameters xmlns:output=\"http://www.w3.org/2010/xslt-xquery-serialization\"><output:use-character-maps><output:character-map character=\"$\" map-string=\"a\"/><output:character-map character=\"$\" map-string=\"b\"/></output:use-character-maps></output:serialization-parameters>'))"));
        Assert.Contains("SEPM0018", ex3.Message);
        // Vendor-namespace parameter is ignored
        Assert.Equal("<e/>", EvalStr(
            "serialize(parse-xml('<e/>'), parse-xml('<output:serialization-parameters xmlns:output=\"http://www.w3.org/2010/xslt-xquery-serialization\"><v:x value=\"yes\" xmlns:v=\"http://vendor.example.com/\"/></output:serialization-parameters>'))"));
    }

    [Fact]
    public void Serialize_Html5()
    {
        var result = EvalStr(
            "serialize(parse-xml('<html><head/><body><p>Hello World!</p></body></html>'), map{'method':'html','html-version':5})");
        Assert.Contains("<!DOCTYPE HTML>", result);
        Assert.Contains("<meta charset", result);
        // Fragment (no html element) → no DOCTYPE.
        Assert.Equal("<body><p>Hello World!</p></body>",
            EvalStr("serialize(parse-xml('<html><head/><body><p>Hello World!</p></body></html>')//body, map{'method':'html','html-version':5})"));
    }

    [Fact]
    public void Serialize_Json()
    {
        Assert.Equal("null", EvalStr("serialize((), map{'method':'json'})"));
        Assert.Equal("{\"uri\":\"http:\\/\\/www.w3.org\\/\"}",
            EvalStr("serialize(map{'uri':xs:anyURI('http://www.w3.org/')}, map{'method':'json'})"));
        Assert.Equal("[\"a\",\"<a>b<\\/a>\"]",
            EvalStr("serialize(array{'a', parse-xml('<a>b</a>')}, map{'method':'json'})"));
        Assert.Equal("\"\\uD834\\uDD1E\"",
            EvalStr("serialize(codepoints-to-string(119070), map{'method':'json','encoding':'ISO-8859-1'})"));
    }

    [Fact]
    public void Serialize_JsonErrors()
    {
        var ex1 = Assert.Throws<InvalidOperationException>(
            () => Evaluate("serialize(1 to 3, map{'method':'json'})"));
        Assert.Contains("SERE0023", ex1.Message);
        var ex2 = Assert.Throws<InvalidOperationException>(
            () => Evaluate("serialize(map{'abc':(1 to 3)}, map{'method':'json'})"));
        Assert.Contains("SERE0023", ex2.Message);
        var ex3 = Assert.Throws<InvalidOperationException>(
            () => Evaluate("serialize(map{xs:QName('foo'):1,'foo':2}, map{'method':'json'})"));
        Assert.Contains("SERE0022", ex3.Message);
        var ex4 = Assert.Throws<InvalidOperationException>(
            () => Evaluate("serialize([number('NaN')], map{'method':'json'})"));
        Assert.Contains("SERE0020", ex4.Message);
    }

    [Fact]
    public void Serialize_Adaptive()
    {
        Assert.Equal("1;2;3", EvalStr("serialize((1,2,3), map{'method':'adaptive','item-separator':';'})"));
        Assert.Equal("<a/>;<b/>",
            EvalStr("serialize((parse-xml('<a/>'), parse-xml('<b/>')), map{'method':'adaptive','item-separator':';'})"));
        Assert.Equal("x=\"1\";y=\"2\"",
            EvalStr("serialize((parse-xml('<a x=\"1\"/>')/a/@x, parse-xml('<b y=\"2\"/>')/b/@y), map{'method':'adaptive','item-separator':';'})"));
        Assert.Equal("map{1:true(),2:false()};map{8:80,9:90}",
            EvalStr("serialize((map{1:true(),2:false()}, map{8:80,9:90}), map{'method':'adaptive','item-separator':';'})"));
    }

    // ------------------------------------------------------------------
    // Document node path traversal (parse-xml as proxy for doc())
    // ------------------------------------------------------------------

    [Fact]
    public void DocumentNode_ChildPath()
    {
        Assert.Equal("hello", EvalSequence("parse-xml('<root><item>hello</item></root>')/root/item")[0]);
    }

    [Fact]
    public void DocumentNode_DescendantPath()
    {
        Assert.Equal("hello", EvalSequence("parse-xml('<root><item>hello</item></root>')//item")[0]);
    }

    [Fact]
    public void DocumentNode_PredicateSubscript()
    {
        Assert.Equal("b", MaterializeResult("parse-xml('<root><a>a</a><a>b</a></root>')/root/a[2]")[0]);
    }

    [Fact]
    public void DocumentNode_ChainedPath()
    {
        var xml = "<library><book><title>Dune</title></book></library>";
        Assert.Equal("Dune", EvalSequence($"parse-xml('{xml}')/library/book/title")[0]);
    }

    // ------------------------------------------------------------------
    // fn:substring-before / fn:substring-after
    // ------------------------------------------------------------------

    [Fact]
    public void SubstringBefore_Found()
    {
        Assert.Equal("abc", EvalStr("substring-before('abcdef', 'def')"));
    }

    [Fact]
    public void SubstringBefore_NotFound()
    {
        Assert.Equal("", EvalStr("substring-before('abcdef', 'xyz')"));
    }

    [Fact]
    public void SubstringAfter_Found()
    {
        Assert.Equal("def", EvalStr("substring-after('abcdef', 'abc')"));
    }

    [Fact]
    public void SubstringAfter_NotFound()
    {
        Assert.Equal("", EvalStr("substring-after('abcdef', 'xyz')"));
    }

    // ------------------------------------------------------------------
    // fn:string-to-codepoints / fn:codepoints-to-string
    // ------------------------------------------------------------------

    [Fact]
    public void StringToCodepoints()
    {
        var result = EvalSequence("string-to-codepoints('abc')");
        Assert.Equal(new[] { "97", "98", "99" }, result);
    }

    [Fact]
    public void CodepointsToString()
    {
        Assert.Equal("abc", EvalStr("codepoints-to-string((97, 98, 99))"));
    }

    // ------------------------------------------------------------------
    // fn:parse-xml
    // ------------------------------------------------------------------

    [Fact]
    public void ParseXml()
    {
        var result = Evaluate("parse-xml('<root><item>hello</item></root>')");
        Assert.True(result.IsNode);
        Assert.Equal(XdmNodeKind.Document, result.NodeValue.NodeKind);
    }

    // ------------------------------------------------------------------
    // fn:doc / fn:collection
    // ------------------------------------------------------------------

    [Fact]
    public void Doc_LoadsDocument()
    {
        var tempFile = System.IO.Path.GetTempFileName() + ".xml";
        System.IO.File.WriteAllText(tempFile, "<root><item>hello</item></root>");
        try
        {
            var result = Evaluate($"doc('{tempFile.Replace("\\", "/")}')");
            Assert.True(result.IsNode);
            Assert.Equal(XdmNodeKind.Document, result.NodeValue.NodeKind);
        }
        finally
        {
            System.IO.File.Delete(tempFile);
        }
    }

    [Fact]
    public void Doc_SameUriSameIdentity()
    {
        var tempFile = System.IO.Path.GetTempFileName() + ".xml";
        System.IO.File.WriteAllText(tempFile, "<root/>");
        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            var expr = XPath31Expression.Compile($"doc('{tempFile.Replace("\\", "/")}') is doc('{tempFile.Replace("\\", "/")}')");
            var result = expr.Evaluate(ctx);
            Assert.Equal("true", result.ToString());
        }
        finally
        {
            System.IO.File.Delete(tempFile);
        }
    }

    [Fact]
    public void Doc_EmptyStringReturnsEmpty()
    {
        var result = EvalStr("doc('')");
        Assert.Equal("()", result);
    }

    [Fact]
    public void Collection_EmptyArg()
    {
        var result = EvalStr("collection()");
        Assert.Equal("()", result);
    }

    [Fact]
    public void Collection_LoadsDirectory()
    {
        var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
        System.IO.Directory.CreateDirectory(tempDir);
        System.IO.File.WriteAllText(System.IO.Path.Combine(tempDir, "a.xml"), "<a/>");
        System.IO.File.WriteAllText(System.IO.Path.Combine(tempDir, "b.xml"), "<b/>");
        try
        {
            var result = Evaluate($"collection('{tempDir.Replace("\\", "/")}')");
            Assert.True(result.IsSequence);
            int count = 0;
            foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            {
                Assert.True(item.IsNode);
                count++;
            }
            Assert.Equal(2, count);
        }
        finally
        {
            System.IO.Directory.Delete(tempDir, true);
        }
    }

    // ------------------------------------------------------------------
    // fn:trace
    // ------------------------------------------------------------------

    [Fact]
    public void Trace_ReturnsValue()
    {
        Assert.Equal("42", EvalStr("trace(42, 'label')"));
    }

    [Fact]
    public void Trace_Sequence()
    {
        var result = Evaluate("trace((1,2,3), 'seq')");
        Assert.True(result.IsSequence);
    }

    // ------------------------------------------------------------------
    // fn:boolean
    // ------------------------------------------------------------------

    [Fact]
    public void Boolean_True()
    {
        Assert.Equal("true", EvalStr("boolean(1)"));
        Assert.Equal("true", EvalStr("boolean('hello')"));
        Assert.Throws<InvalidOperationException>(() => EvalStr("boolean((1,2))"));
    }

    [Fact]
    public void Boolean_False()
    {
        Assert.Equal("false", EvalStr("boolean(0)"));
        Assert.Equal("false", EvalStr("boolean('')"));
        Assert.Equal("false", EvalStr("boolean(())"));
    }

    // ------------------------------------------------------------------
    // fn:zero-or-one / fn:one-or-more / fn:exactly-one
    // ------------------------------------------------------------------

    [Fact]
    public void ZeroOrOne_Empty() => Assert.True(Evaluate("zero-or-one(())").IsUndefined);

    [Fact]
    public void ZeroOrOne_Singleton() => Assert.Equal("42", EvalStr("zero-or-one(42)"));

    [Fact]
    public void ZeroOrOne_Multi() => Assert.Throws<InvalidOperationException>(() => Evaluate("zero-or-one((1,2))"));

    [Fact]
    public void OneOrMore_Empty() => Assert.Throws<InvalidOperationException>(() => Evaluate("one-or-more(())"));

    [Fact]
    public void OneOrMore_Singleton() => Assert.Equal("42", EvalStr("one-or-more(42)"));

    [Fact]
    public void OneOrMore_Multi()
    {
        var result = Evaluate("one-or-more((1,2))");
        Assert.True(result.IsSequence);
    }

    [Fact]
    public void ExactlyOne_Empty() => Assert.Throws<InvalidOperationException>(() => Evaluate("exactly-one(())"));

    [Fact]
    public void ExactlyOne_Singleton() => Assert.Equal("42", EvalStr("exactly-one(42)"));

    [Fact]
    public void ExactlyOne_Multi() => Assert.Throws<InvalidOperationException>(() => Evaluate("exactly-one((1,2))"));

    // ------------------------------------------------------------------
    // fn:base-uri / fn:document-uri
    // ------------------------------------------------------------------

    [Fact]
    public void BaseUri_ParseXmlDocument()
    {
        // parse-xml creates a dynamic document with no known base URI
        Assert.True(Evaluate("base-uri(parse-xml('<a/>'))").IsUndefined);
    }

    [Fact]
    public void BaseUri_ParseXmlElement()
    {
        Assert.True(Evaluate("base-uri(parse-xml('<a/>')/a[1])").IsUndefined);
    }

    [Fact]
    public void DocumentUri_ParseXmlDocument()
    {
        Assert.True(Evaluate("document-uri(parse-xml('<a/>'))").IsUndefined);
    }

    [Fact]
    public void DocumentUri_ElementReturnsUndefined()
    {
        Assert.True(Evaluate("document-uri(parse-xml('<a/>')/a[1])").IsUndefined);
    }

    // ------------------------------------------------------------------
    // fn:sort tests
    // ------------------------------------------------------------------

    [Fact]
    public void Sort_Mixed_Integer_Decimal()
    {
        var result = Evaluate("sort((3, 1.5, 2))");
        var items = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item);
        Assert.Equal(3, items.Count);
        Assert.Equal(1.5m, items[0].DecimalValue);
        Assert.Equal(2, items[1].IntegerValue);
        Assert.Equal(3, items[2].IntegerValue);
    }

    [Fact]
    public void Sort_Strings()
    {
        var result = Evaluate("sort(('c', 'a', 'b'))");
        var items = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item);
        Assert.Equal(3, items.Count);
        Assert.Equal("a", items[0].StringValue);
        Assert.Equal("b", items[1].StringValue);
        Assert.Equal("c", items[2].StringValue);
    }

    [Fact]
    public void Sort_Mixed_Types_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Evaluate("sort((1, 'a'))"));
    }

    [Fact]
    public void Sort_Booleans()
    {
        var result = Evaluate("sort((true(), false(), true()))");
        var items = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item);
        Assert.Equal(3, items.Count);
        Assert.False(items[0].BooleanValue);
        Assert.True(items[1].BooleanValue);
        Assert.True(items[2].BooleanValue);
    }

    // ------------------------------------------------------------------
    // JSON functions
    // ------------------------------------------------------------------

    [Fact]
    public void ParseJson_EmptyObject()
    {
        var result = Evaluate("parse-json('{}')");
        Assert.True(result.IsMap);
        Assert.Equal(0, result.MapValue.Count);
    }

    [Fact]
    public void ParseJson_ObjectWithString()
    {
        var result = Evaluate("parse-json('{\"name\":\"hello\"}')");
        Assert.True(result.IsMap);
        Assert.Equal(1, result.MapValue.Count);
        Assert.True(result.MapValue.TryGetValue(XdmValue.FromString("name"), out var value));
        Assert.Equal("hello", value.StringValue);
    }

    [Fact]
    public void ParseJson_ObjectWithNumber()
    {
        var result = Evaluate("parse-json('{\"count\":42}')");
        Assert.True(result.IsMap);
        Assert.True(result.MapValue.TryGetValue(XdmValue.FromString("count"), out var value));
        Assert.Equal(XdmValueKind.Double, value.Kind);
        Assert.Equal(42.0, value.DoubleValue);
    }

    [Fact]
    public void ParseJson_ObjectWithBoolean()
    {
        var result = Evaluate("parse-json('{\"active\":true}')");
        Assert.True(result.IsMap);
        Assert.True(result.MapValue.TryGetValue(XdmValue.FromString("active"), out var value));
        Assert.True(value.BooleanValue);
    }

    [Fact]
    public void ParseJson_ObjectWithNull()
    {
        var result = Evaluate("parse-json('{\"value\":null}')");
        Assert.True(result.IsMap);
        Assert.True(result.MapValue.TryGetValue(XdmValue.FromString("value"), out var value));
        Assert.True(value.IsUndefined);
    }

    [Fact]
    public void ParseJson_EmptyArray()
    {
        var result = Evaluate("parse-json('[]')");
        Assert.True(result.IsArray);
        Assert.Equal(0, result.ArrayValue.Count);
    }

    [Fact]
    public void ParseJson_EmptyInput_ReturnsEmpty()
        => Assert.True(Evaluate("parse-json(())").IsUndefined);

    [Fact]
    public void ParseJson_DuplicatesUseLast()
    {
        var result = Evaluate("parse-json('{\"a\":1, \"b\":2, \"a\":3}', map{'duplicates':'use-last'})");
        Assert.True(result.MapValue.TryGetValue(XdmValue.FromString("a"), out var value));
        Assert.Equal(3.0, value.DoubleValue);
    }

    [Fact]
    public void ParseJson_DuplicatesReject_RaisesFOJS0003()
        => Assert.Contains("FOJS0003", Assert.Throws<InvalidOperationException>(() => Evaluate("parse-json('{\"a\":1, \"a\":2}', map{'duplicates':'reject'})")).Message);

    [Fact]
    public void ParseJson_DuplicatesRetain_RaisesFOJS0005()
        => Assert.Contains("FOJS0005", Assert.Throws<InvalidOperationException>(() => Evaluate("parse-json('{\"a\":1}', map{'duplicates':'retain'})")).Message);

    [Fact]
    public void ParseJson_DuplicateKeysAfterUnescape_RaisesFOJS0003()
        => Assert.Contains("FOJS0003", Assert.Throws<InvalidOperationException>(() => Evaluate("parse-json('{\"%\":\"x\", \"\\u0025\":\"y\"}', map{'escape':true(), 'duplicates':'reject'})")).Message);

    [Fact]
    public void ParseJson_ControlEscape_BecomesCharacter()
    {
        var result = Evaluate("parse-json('[\"\\r\"]')");
        Assert.Equal("\r", result.ArrayValue.Get(1).StringValue);
    }

    [Fact]
    public void ParseJson_InvalidXmlCharEscape_DefaultsToReplacementChar()
    {
        var result = Evaluate("parse-json('\"\\uFFFF\"')");
        Assert.Equal("\uFFFD", result.StringValue);
    }

    [Fact]
    public void ParseJson_UnpairedSurrogate_DefaultsToReplacementChar()
    {
        var result = Evaluate("parse-json('\"\\uDEAD\"')");
        Assert.Equal("\uFFFD", result.StringValue);
    }

    [Fact]
    public void ParseJson_EscapeTrue_RetainsNamedEscapes()
    {
        var result = Evaluate("parse-json('[\"\\n\"]', map{'escape':true()})");
        Assert.Equal("\\n", result.ArrayValue.Get(1).StringValue);
    }

    [Fact]
    public void ParseJson_EscapeTrue_ExpandsValidUnicodeEscapes()
    {
        var result = Evaluate("parse-json('[\"\\u0025\"]', map{'escape':true()})");
        Assert.Equal("%", result.ArrayValue.Get(1).StringValue);
    }

    [Fact]
    public void ParseJson_Fallback_ReceivesEscapeSequenceAsWritten()
    {
        var result = Evaluate("parse-json('\"\\uFFFF\"', map{'fallback':lower-case#1})");
        Assert.Equal("\\uffff", result.StringValue);
    }

    [Fact]
    public void ParseJson_FallbackWrongArity_RaisesXPTY0004()
        => Assert.Contains("XPTY0004", Assert.Throws<InvalidOperationException>(() => Evaluate("parse-json('\"\\uFFFF\"', map{'fallback':substring#2})")).Message);

    [Fact]
    public void ParseJson_SurrogatePair_ExpandsToAstralChar()
    {
        var result = Evaluate("parse-json('\"\\uD834\\uDD1E\"')");
        Assert.Equal("\U0001D11E", result.StringValue);
    }

    [Fact]
    public void ParseJson_ArrayWithMixedValues()
    {
        var result = Evaluate("parse-json('[1,\"two\",true,null]')");
        Assert.True(result.IsArray);
        Assert.Equal(4, result.ArrayValue.Count);
        Assert.Equal(1.0, result.ArrayValue.Get(1).DoubleValue);
        Assert.Equal("two", result.ArrayValue.Get(2).StringValue);
        Assert.True(result.ArrayValue.Get(3).BooleanValue);
        Assert.True(result.ArrayValue.Get(4).IsUndefined);
    }

    [Fact]
    public void ParseJson_NestedObject()
    {
        var result = Evaluate("parse-json('{\"a\":{\"b\":1}}')");
        Assert.True(result.IsMap);
        Assert.True(result.MapValue.TryGetValue(XdmValue.FromString("a"), out var inner));
        Assert.True(inner.IsMap);
        Assert.True(inner.MapValue.TryGetValue(XdmValue.FromString("b"), out var b));
        Assert.Equal(1.0, b.DoubleValue);
    }

    [Fact]
    public void JsonToXml_EmptyObject()
    {
        var result = Evaluate("json-to-xml('{}')");
        Assert.True(result.IsNode);
        var doc = result.NodeValue;
        Assert.Equal(XdmNodeKind.Document, doc.NodeKind);
        IXdmNode? root = null;
        foreach (var child in doc.Axis(XdmAxis.Child))
        {
            root = child.NodeValue!;
            break;
        }
        Assert.NotNull(root);
        Assert.Equal("map", root!.LocalName);
        Assert.Equal("http://www.w3.org/2005/xpath-functions", root.NamespaceUri);
    }

    [Fact]
    public void JsonToXml_ObjectWithString()
    {
        var result = Evaluate("json-to-xml('{\"name\":\"hello\"}')");
        Assert.True(result.IsNode);
        IXdmNode? root = null;
        foreach (var child in result.NodeValue.Axis(XdmAxis.Child))
        {
            root = child.NodeValue!;
            break;
        }
        Assert.NotNull(root);
        Assert.Equal("map", root!.LocalName);
        IXdmNode? childNode = null;
        foreach (var child in root.Axis(XdmAxis.Child))
        {
            childNode = child.NodeValue!;
            break;
        }
        Assert.NotNull(childNode);
        Assert.Equal("string", childNode!.LocalName);
        Assert.Equal("hello", childNode.StringValue);
        IXdmNode? keyAttr = null;
        foreach (var attr in childNode.Attributes("key"))
        {
            keyAttr = attr.NodeValue!;
            break;
        }
        Assert.NotNull(keyAttr);
        Assert.Equal("name", keyAttr!.StringValue);
    }

    [Fact]
    public void JsonToXml_ArrayWithNumber()
    {
        var result = Evaluate("json-to-xml('[42]')");
        Assert.True(result.IsNode);
        IXdmNode? root = null;
        foreach (var child in result.NodeValue.Axis(XdmAxis.Child))
        {
            root = child.NodeValue!;
            break;
        }
        Assert.NotNull(root);
        Assert.Equal("array", root!.LocalName);
        IXdmNode? childNode = null;
        foreach (var child in root.Axis(XdmAxis.Child))
        {
            childNode = child.NodeValue!;
            break;
        }
        Assert.NotNull(childNode);
        Assert.Equal("number", childNode!.LocalName);
        Assert.Equal("42", childNode.StringValue);
    }

    [Fact]
    public void JsonToXml_Null()
    {
        var result = Evaluate("json-to-xml('[null]')");
        Assert.True(result.IsNode);
        IXdmNode? root = null;
        foreach (var child in result.NodeValue.Axis(XdmAxis.Child))
        {
            root = child.NodeValue!;
            break;
        }
        Assert.NotNull(root);
        IXdmNode? childNode = null;
        foreach (var child in root.Axis(XdmAxis.Child))
        {
            childNode = child.NodeValue!;
            break;
        }
        Assert.NotNull(childNode);
        Assert.Equal("null", childNode!.LocalName);
    }

    [Fact]
    public void JsonToXml_DuplicatesRetainedByDefault()
    {
        // fn:json-to-xml retains duplicate keys by default (QT3 json-to-xml-018).
        var result = Evaluate("json-to-xml('{\"a\":3, \"b\":4, \"a\":5}')");
        var xml = result.NodeValue.ToXmlString();
        Assert.Contains("<number key=\"a\">3</number>", xml);
        Assert.Contains("<number key=\"a\">5</number>", xml);
    }

    [Fact]
    public void JsonToXml_DuplicatesUseFirst_KeepsFirstOccurrence()
    {
        var result = Evaluate("json-to-xml('{\"a\":3, \"b\":4, \"a\":5}', map{'duplicates':'use-first'})");
        var xml = result.NodeValue.ToXmlString();
        Assert.Contains("<number key=\"a\">3</number>", xml);
        Assert.DoesNotContain("<number key=\"a\">5</number>", xml);
    }

    [Fact]
    public void JsonToXml_EmptySequence_ReturnsEmpty()
    {
        // json-to-xml-028/035: the empty sequence yields the empty sequence.
        var result = Evaluate("json-to-xml(())");
        Assert.True(result.IsUndefined);
        var result2 = Evaluate("json-to-xml((), map{'escape':false()})");
        Assert.True(result2.IsUndefined);
    }

    [Fact]
    public void JsonToXml_EscapeTrue_DecodesQuotesAndRetainsControlEscapes()
    {
        // json-to-xml-049: with escape=true the quotation mark is decoded, the
        // reverse solidus and control characters stay escaped, and the string is
        // marked escaped="true".
        var result = Evaluate("json-to-xml('\"\\\\\\/\\\"\\r\\t\\u0020\"', map{'escape':true()})");
        var xml = result.NodeValue.ToXmlString();
        Assert.Contains("escaped=\"true\"", xml);
        Assert.Contains(">\\\\/\"\\r\\t </string>", xml);
    }

    [Fact]
    public void JsonToXml_EscapeTrue_InvalidXmlChar_RetainedEscaped()
    {
        // json-to-xml-021:  is retained in canonical short form \f.
        var result = Evaluate("json-to-xml('{\"a\":\"\\u000C\"}', map{'escape':true()})");
        var xml = result.NodeValue.ToXmlString();
        Assert.Contains("escaped=\"true\"", xml);
        Assert.Contains(">\\f</string>", xml);
    }

    [Fact]
    public void JsonToXml_UnpairedSurrogate_EscapeFalse_ReplacementChar()
    {
        // json-to-xml-023: the default fallback maps unpaired surrogates to U+FFFD.
        var result = Evaluate("json-to-xml('{\"a\":\"\\uDA00\"}', map{'escape':false()})");
        var xml = result.NodeValue.ToXmlString();
        Assert.Contains("\uFFFD", xml);
    }

    [Fact]
    public void JsonToXml_UnpairedSurrogate_EscapeTrue_RetainedEscaped()
    {
        // json-to-xml-024: with escape=true the escape sequence is retained.
        var result = Evaluate("json-to-xml('{\"a\":\"\\uDA00\"}', map{'escape':true()})");
        var xml = result.NodeValue.ToXmlString();
        Assert.Contains("escaped=\"true\"", xml);
        Assert.Contains(">\\uDA00</string>", xml);
    }

    [Fact]
    public void JsonToXml_FallbackNotFunction_RaisesXPTY0004()
    {
        // json-to-xml-error-026: validated eagerly even when never invoked.
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("json-to-xml('[\"String\"]', map{'fallback':'dummy'})"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void JsonToXml_FallbackWrongArity_RaisesXPTY0004()
    {
        // json-to-xml-error-041: wrong-arity fallback is rejected at option-parse time.
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("json-to-xml('[\"String\"]', map{'fallback':concat#2})"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void JsonToXml_ValidateNotBoolean_RaisesXPTY0004()
    {
        // json-to-xml-error-022: the validate option must be a single xs:boolean.
        var ex = Assert.Throws<InvalidOperationException>(
            () => Evaluate("json-to-xml('[\"String\"]', map{'validate':'EMCA-262'})"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void ParseJson_EscapeTrue_DecodesQuotes()
    {
        // json-doc-012: the quotation mark is decoded even with escape=true.
        var result = Evaluate("parse-json('[\"a\\\"b\"]', map{'escape':true()})?1");
        Assert.Equal("a\"b", result.StringValue);
    }

    [Fact]
    public void XmlToJson_RoundTrip_Object()
    {
        var json = Evaluate("xml-to-json(json-to-xml('{\"a\":1,\"b\":\"two\"}'))");
        Assert.Equal(XdmValueKind.String, json.Kind);
        var parsed = Evaluate($"parse-json('{json.StringValue.Replace("\\", "\\\\").Replace("'", "\\'")}')");
        Assert.True(parsed.IsMap);
        Assert.True(parsed.MapValue.TryGetValue(XdmValue.FromString("a"), out var a));
        Assert.Equal(1.0, a.DoubleValue);
        Assert.True(parsed.MapValue.TryGetValue(XdmValue.FromString("b"), out var b));
        Assert.Equal("two", b.StringValue);
    }

    [Fact]
    public void XmlToJson_RoundTrip_Array()
    {
        var json = Evaluate("xml-to-json(json-to-xml('[1,\"two\",true,null]'))");
        Assert.Equal(XdmValueKind.String, json.Kind);
        var parsed = Evaluate($"parse-json('{json.StringValue.Replace("\\", "\\\\").Replace("'", "\\'")}')");
        Assert.True(parsed.IsArray);
        Assert.Equal(4, parsed.ArrayValue.Count);
    }

    // ------------------------------------------------------------------
    // fn:xml-to-json validation / escaping (F+O §32.2.2) — 2026-07-15
    // ------------------------------------------------------------------

    [Fact]
    public void XmlToJson_Number_ReformatsToCanonicalDouble()
    {
        // j:number content is validated and re-emitted as a canonical xs:double.
        var json = Evaluate("xml-to-json(parse-xml('<number xmlns=\"http://www.w3.org/2005/xpath-functions\">1E6</number>'))");
        Assert.Equal("1.0E6", json.StringValue);
    }

    [Fact]
    public void XmlToJson_Number_SmallNegativeExponent_ExpandsToDecimal()
    {
        // Doubles in [1e-6, 1e6) use decimal notation, not scientific.
        var json = Evaluate("xml-to-json(parse-xml('<number xmlns=\"http://www.w3.org/2005/xpath-functions\">-1E-6</number>'))");
        Assert.Equal("-0.000001", json.StringValue);
    }

    [Fact]
    public void XmlToJson_InvalidNumber_RaisesFOJS0006()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Evaluate("xml-to-json(parse-xml('<number xmlns=\"http://www.w3.org/2005/xpath-functions\">12x</number>'))"));
        Assert.Contains("FOJS0006", ex.Message);
    }

    [Fact]
    public void XmlToJson_MapEntryMissingKey_RaisesFOJS0006()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Evaluate("xml-to-json(parse-xml('<map xmlns=\"http://www.w3.org/2005/xpath-functions\"><string>v</string></map>'))"));
        Assert.Contains("FOJS0006", ex.Message);
    }

    [Fact]
    public void XmlToJson_NonJsonNamespace_RaisesFOJS0006()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Evaluate("xml-to-json(parse-xml('<string xmlns=\"http://example.com/other\">v</string>'))"));
        Assert.Contains("FOJS0006", ex.Message);
    }

    [Fact]
    public void XmlToJson_EscapedString_UnescapesUnicodeEscape()
    {
        // escaped="true" marks JSON-escaped content; \uXXXX must be decoded on output.
        var json = Evaluate("xml-to-json(parse-xml('<string xmlns=\"http://www.w3.org/2005/xpath-functions\" escaped=\"true\">\\u0041bc</string>'))");
        Assert.Equal("\"Abc\"", json.StringValue);
    }

    [Fact]
    public void XmlToJson_Boolean_CanonicalizesOne()
    {
        var json = Evaluate("xml-to-json(parse-xml('<boolean xmlns=\"http://www.w3.org/2005/xpath-functions\">1</boolean>'))");
        Assert.Equal("true", json.StringValue);
    }

    [Fact]
    public void XmlToJson_InvalidBoolean_RaisesFOJS0006()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Evaluate("xml-to-json(parse-xml('<boolean xmlns=\"http://www.w3.org/2005/xpath-functions\">yes</boolean>'))"));
        Assert.Contains("FOJS0006", ex.Message);
    }

    // ------------------------------------------------------------------
    // Resource URI mapping / JSON error codes (2026-07-15)
    // ------------------------------------------------------------------

    [Fact]
    public void ParseJson_InvalidJson_RaisesFOJS0001()
    {
        // Invalid JSON must surface as XPath error FOJS0001, not a raw JsonException.
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("parse-json('{\"a\":}')"));
        Assert.Contains("FOJS0001", ex.Message);
    }

    [Fact]
    public void JsonToXml_InvalidJson_RaisesFOJS0001()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("json-to-xml('[1,]')"));
        Assert.Contains("FOJS0001", ex.Message);
    }

    [Fact]
    public void JsonDoc_ResourceUriMapper_LoadsMappedFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        File.WriteAllText(path, "{" + "\"x\":1}");
        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            ctx.ResourceUriMapper = u => u == "http://example.org/qt3/json/test-json" ? path : null;
            var result = XPath31Expression.Compile("json-doc('http://example.org/qt3/json/test-json')?x").Evaluate(ctx);
            Assert.Equal(1.0, result.DoubleValue);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void UnparsedText_ResourceUriMapper_LoadsMappedFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        File.WriteAllText(path, "hello world");
        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            ctx.ResourceUriMapper = u => u == "http://example.org/text/doc-txt" ? path : null;
            var result = XPath31Expression.Compile("unparsed-text('http://example.org/text/doc-txt')").Evaluate(ctx);
            Assert.Equal("hello world", result.StringValue);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ParseJson_LeadingBom_IsIgnored()
    {
        // json-to-xml-015: a leading U+FEFF must be accepted.
        var result = Evaluate("parse-json(codepoints-to-string(65279) || '[1]')");
        Assert.True(result.IsArray);
        Assert.Equal(1.0, result.ArrayValue.Get(1).DoubleValue);
    }

    [Fact]
    public void ParseJson_UnpairedSurrogate_UsesFallback()
    {
        // json-doc-039 pattern: the fallback receives the raw escape sequence.
        var result = Evaluate("parse-json('{\"s\":\"oh dear \\uDEAD\"}', map{'fallback': function($s){substring($s, 3)}})");
        Assert.True(result.IsMap);
        Assert.True(result.MapValue.TryGetValue(XdmValue.FromString("s"), out var value));
        Assert.Equal("oh dear DEAD", value.StringValue);
    }

    [Fact]
    public void ParseJson_UnpairedSurrogate_NoFallback_YieldsReplacementChar()
    {
        // F+O 3.1: without an explicit fallback option the default fallback returns
        // U+FFFD for escapes denoting invalid characters (fn-parse-json-054/922).
        var result = Evaluate("parse-json('{\"s\":\"\\uDEAD\"}')");
        Assert.True(result.MapValue.TryGetValue(XdmValue.FromString("s"), out var value));
        Assert.Equal("\uFFFD", value.StringValue);
    }

    [Fact]
    public void UnparsedText_InvalidUtf8_InferredEncoding_RaisesFOUT1190()
    {
        // fn-unparsed-text-045 pattern: undecodable bytes with no explicit encoding.
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        File.WriteAllBytes(path, new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F, 0xA0, 0x77, 0x6F, 0x72, 0x6C, 0x64 });
        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            ctx.ResourceUriMapper = u => u == "http://example.org/text/latin1-txt" ? path : null;
            var ex = Assert.Throws<InvalidOperationException>(() =>
                XPath31Expression.Compile("unparsed-text('http://example.org/text/latin1-txt')").Evaluate(ctx));
            Assert.Contains("FOUT1190", ex.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void UnparsedText_UnknownExplicitEncoding_RaisesFOUT1200()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        File.WriteAllText(path, "hello");
        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            ctx.ResourceUriMapper = u => u == "http://example.org/text/plain-txt" ? path : null;
            var ex = Assert.Throws<InvalidOperationException>(() =>
                XPath31Expression.Compile("unparsed-text('http://example.org/text/plain-txt', 'no-such-encoding')").Evaluate(ctx));
            Assert.Contains("FOUT1200", ex.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void UnparsedTextAvailable_InvalidUtf8_ReturnsFalse()
    {
        // fn-unparsed-text-available-037 pattern.
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        File.WriteAllBytes(path, new byte[] { 0x68, 0x69, 0xA0 });
        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            ctx.ResourceUriMapper = u => u == "http://example.org/text/bad-txt" ? path : null;
            Assert.Equal("false", XPath31Expression.Compile("unparsed-text-available('http://example.org/text/bad-txt')").Evaluate(ctx).ToString());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Doc_ResourceUriMapper_LoadsMappedFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        File.WriteAllText(path, "<root><a>1</a></root>");
        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            ctx.ResourceUriMapper = u => u == "http://example.org/docs/test-doc" ? path : null;
            var result = XPath31Expression.Compile("string(doc('http://example.org/docs/test-doc')/root/a)").Evaluate(ctx);
            Assert.Equal("1", result.StringValue);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // ------------------------------------------------------------------
    // QT3 regex quick wins (2026-07-15): dot-vs-CR, \S, x flag, backrefs,
    // empty classes, tokenize captures/NBSP, translate arity types
    // ------------------------------------------------------------------

    [Fact]
    public void Matches_DotExcludesCarriageReturn()
    {
        // XSD '.' matches any character except #xA and #xD (fn-matches-45, fn-tokenize-34).
        Assert.Equal("false", EvalStr("fn:matches(concat('Mary', codepoints-to-string(13), 'Jones'), 'Mary.Jones')"));
        Assert.Equal("true", EvalStr("fn:matches(concat('Mary', codepoints-to-string(13), 'Jones'), 'Mary.Jones', 's')"));
    }

    [Fact]
    public void Matches_WhitespaceClass_ExcludesOnlyXsdWhitespace()
    {
        // \S is the complement of {#x20,#x9,#xA,#xD} only (cbcl-matches-041b).
        Assert.Equal("false", EvalStr("fn:matches(codepoints-to-string((13, 32, 9)), '\\S+')"));
        Assert.Equal("true", EvalStr("fn:matches('a', '\\S')"));
        // NBSP is not XSD whitespace.
        Assert.Equal("true", EvalStr("fn:matches(codepoints-to-string(160), '\\S')"));
    }

    [Fact]
    public void Matches_FlagX_StripsPatternWhitespace()
    {
        // The four examples from the F&O 3.1 spec (5.6.2 Flags).
        Assert.Equal("true", EvalStr("fn:matches('helloworld', 'hello world', 'x')"));
        Assert.Equal("false", EvalStr("fn:matches('helloworld', 'hello[ ]world', 'x')"));
        Assert.Equal("true", EvalStr("fn:matches('hello world', 'hello\\ sworld', 'x')"));
        Assert.Equal("false", EvalStr("fn:matches('hello world', 'hello world', 'x')"));
    }

    [Fact]
    public void Matches_FlagX_StripsInsideCategoryBraces()
    {
        // Whitespace is removed even inside \p{...} names (K2-MatchesFunc-5/6).
        Assert.Equal("true", EvalStr("fn:matches('hello world', '\\p{ IsBasicLatin}+', 'x')"));
        Assert.Equal("true", EvalStr("fn:matches('hello world', '\\p{ I s B a s i c L a t i n }+', 'x')"));
    }

    [Fact]
    public void Matches_BackreferenceToUnclosedGroup_Throws()
    {
        // XSD erratum FO.E24: a back-reference before the group's closing parenthesis
        // is FORX0002 (fn-matches-37..40, fn-matchesErr-4/5).
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:matches('aa', '(a\\1)')"));
        Assert.Contains("FORX0002", ex.Message);
        ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:matches('#abc#1', '^((#)abc\\1)$')"));
        Assert.Contains("FORX0002", ex.Message);
        // A reference to a group that is already closed is fine.
        Assert.Equal("true", EvalStr("fn:matches('abab', '^(ab)\\1$')"));
    }

    [Fact]
    public void Matches_EmptyCharClass_Throws()
    {
        // XSD grammar requires at least one class member (cbcl-matches-001).
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:matches('foo', '[^]')"));
        Assert.Contains("FORX0002", ex.Message);
        ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:matches('foo', '[]')"));
        Assert.Contains("FORX0002", ex.Message);
    }

    [Fact]
    public void Tokenize_ExcludesCapturingGroups()
    {
        // .NET Regex.Split interleaves captures; fn:tokenize must not (fn-tokenize-9).
        Assert.Equal("#r#c#d#r#", EvalStr("string-join(fn:tokenize('abracadabra', '(ab)|(a)'), '#')"));
    }

    [Fact]
    public void Tokenize_OneArg_NbspIsNotASeparator()
    {
        // fn:tokenize/1 splits on XPath whitespace only; NBSP stays (fn-tokenize-51).
        Assert.Equal("1", EvalStr("count(fn:tokenize(codepoints-to-string((97, 98, 99, 160, 100, 101, 102))))"));
    }

    [Fact]
    public void NormalizeSpace_KeepsNonXsdWhitespace()
    {
        Assert.Equal("a b", EvalStr("fn:normalize-space('  a   b  ')"));
        // NBSP is not collapsed or stripped.
        Assert.Equal("1", EvalStr("count(fn:tokenize(fn:normalize-space(codepoints-to-string((160, 97, 160)))) )"));
    }

    [Fact]
    public void Translate_NonStringArgument_Throws()
    {
        // fn-translate3args-5/6/7
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("translate(1, '-', 'x')"));
        Assert.Contains("XPTY0004", ex.Message);
        ex = Assert.Throws<InvalidOperationException>(() => Evaluate("translate('abc', 1, 'x')"));
        Assert.Contains("XPTY0004", ex.Message);
        ex = Assert.Throws<InvalidOperationException>(() => Evaluate("translate('abc', 'x', 1)"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Translate_EmptySequenceForRequiredArg_Throws()
    {
        // K2-TranslateFunc-1/2: $map and $trans are required (xs:string, not xs:string?).
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:translate('arg', (), 'transString')"));
        Assert.Contains("XPTY0004", ex.Message);
        ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:translate('arg', 'mapString', ())"));
        Assert.Contains("XPTY0004", ex.Message);
        // $arg remains optional: the empty sequence translates to "".
        Assert.Equal("", EvalStr("fn:translate((), 'a', 'b')"));
    }

    // ------------------------------------------------------------------
    // XSD regex syntax validation (re00xxx cluster) and anchor/arg fixes
    // ------------------------------------------------------------------

    [Theory]
    [InlineData("{5")]
    [InlineData("{5,")]
    [InlineData("{5,6")]
    [InlineData("a]")]
    [InlineData(@"(?n:(foo)(\s+)(bar))")]
    [InlineData(@"(?i:foo)")]
    [InlineData(@"foo(?#comment)")]
    [InlineData(@"(foo)(\077)")]
    [InlineData(@"(foo)(\7)")]
    [InlineData(@"(foo)(\x2a*)(bar)")]
    [InlineData(@"(\u0034)")]
    [InlineData(@"\A(foo)")]
    [InlineData(@"(foo)\Z")]
    [InlineData(@".*\b(\w+)\b")]
    [InlineData(@"abc(?=XXX)\w+")]
    [InlineData(@"[^-[bc]]")]
    [InlineData(@"[[abcd]-[bc]]+")]
    [InlineData(@"foo\")]
    public void Matches_InvalidXsdSyntax_Throws(string pattern)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate($"fn:matches('qwerty', '{pattern}')"));
        Assert.Contains("FORX0002", ex.Message);
    }

    [Fact]
    public void Matches_ValidSyntax_StillAccepted()
    {
        // Non-capturing groups are part of XSD 1.1 / XPath 3.1.
        Assert.Equal("true", EvalStr(@"fn:matches('abab', '^(?:ab)+$')"));
        // Class subtraction nests one level: [a-d-[b-c]] = {a, d}.
        Assert.Equal("true", EvalStr(@"fn:matches('a', '[a-d-[b-c]]')"));
        Assert.Equal("false", EvalStr(@"fn:matches('c', '[a-d-[b-c]]')"));
        Assert.Equal("false", EvalStr(@"fn:matches('b', '[a-d-[b-c]]')"));
        // Escaped brackets/braces are ordinary members.
        Assert.Equal("true", EvalStr(@"fn:matches(']', '[\[\]]')"));
        Assert.Equal("true", EvalStr(@"fn:matches('x{y}', 'x\{y\}')"));
        // Quantifiers still work.
        Assert.Equal("true", EvalStr(@"fn:matches('aaa', 'a{2,3}')"));
    }

    [Fact]
    public void Matches_Backreference_MultiDigitGobbling()
    {
        // F&O 5.6.1.4: trailing digits join the reference only while the number fits the
        // groups opened so far; (a)(b)\12 is \1 + literal '2'.
        Assert.Equal("true", EvalStr(@"fn:matches('aba2', '^(a)(b)\12$')"));
        // Reference to a group that does not exist yet is rejected.
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate(@"fn:matches('qwerty', '(foo)(\7)')"));
        Assert.Contains("FORX0002", ex.Message);
    }

    [Fact]
    public void Matches_MultilineCaret_ExcludesPositionAfterTrailingNewline()
    {
        // fn-matches-26: '^' must not match the position after a final newline.
        Assert.Equal("false", EvalStr("fn:matches(concat('abcd', codepoints-to-string(10), 'defg', codepoints-to-string(10)), '^$', 'm')"));
        // ...but still matches real line starts (an empty middle line here).
        Assert.Equal("true", EvalStr("fn:matches(concat('abcd', codepoints-to-string((10, 10)), 'defg'), '^$', 'm')"));
        Assert.Equal("true", EvalStr("fn:matches(concat('ab', codepoints-to-string(10), 'cd'), '^cd', 'm')"));
    }

    [Fact]
    public void Matches_EmptyPatternArgument_Throws()
    {
        // K-MatchesFunc-1/3: $pattern and $flags are required.
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:matches('input', ())"));
        Assert.Contains("XPTY0004", ex.Message);
        ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:matches('input', 'pattern', ())"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Tokenize_MultilineCaret_ZeroLengthMatch_Throws()
    {
        // fn-tokenize-36/38: '^' in multiline mode still matches the empty string (at 0),
        // so the zero-length check must fire even with the trailing-newline guard in place.
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Evaluate("fn:tokenize(concat('Mary', codepoints-to-string(10), 'Jones'), '^', 'm')"));
        Assert.Contains("FORX0003", ex.Message);
        ex = Assert.Throws<InvalidOperationException>(() =>
            Evaluate("fn:tokenize(concat('Mary', codepoints-to-string(10), 'Jones'), '^[\\s]*$', 'm')"));
        Assert.Contains("FORX0003", ex.Message);
    }

    // ------------------------------------------------------------------
    // fn:normalize-unicode form handling
    // ------------------------------------------------------------------

    [Fact]
    public void NormalizeUnicode_FormNameIsCaseInsensitiveAndTrimmed()
    {
        Assert.Equal("Nothing to normalize.", EvalStr("normalize-unicode('Nothing to normalize.', 'nFc')"));
        Assert.Equal("ÅÅ", EvalStr("fn:concat(fn:normalize-unicode('Å',' NFC '),fn:normalize-unicode('Å','NFC'))"));
    }

    [Fact]
    public void NormalizeUnicode_EmptyForm_PerformsNoNormalization()
    {
        // U+00C5 (Å) and U+212B (Å) differ unless a normalization form is applied.
        Assert.Equal("false", EvalStr("normalize-unicode('Å', '') eq normalize-unicode('Å', '')"));
        Assert.Equal("f oo", EvalStr("normalize-unicode('f oo', '')"));
    }

    [Fact]
    public void NormalizeUnicode_FullyNormalized()
    {
        // Fully-normalized input passes through unchanged...
        Assert.Equal("blah", EvalStr("normalize-unicode('blah', 'FULLY-NORMALIZED')"));
        Assert.Equal("1", EvalStr("count(normalize-unicode(codepoints-to-string(2494), 'FULLY-NORMALIZED'))"));
        // ...input not in NFC raises FOCH0003.
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Evaluate("normalize-unicode(codepoints-to-string((65, 775)), 'FULLY-NORMALIZED')"));
        Assert.Contains("FOCH0003", ex.Message);
    }

    [Fact]
    public void NormalizeUnicode_NonStringInput_Throws()
    {
        // fn-normalize-unicode1args-7
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("normalize-unicode(12)"));
        Assert.Contains("XPTY0004", ex.Message);
    }
}


public class Tier2iMapArrayTests
{
    private static XdmValue Evaluate(string xpath)
    {
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        return XPath31Expression.Compile(xpath).Evaluate(ctx);
    }

    private static string EvalStr(string xpath) => Evaluate(xpath).ToString();

    // ----- map:merge duplicates option ---------------------------------

    [Fact]
    public void MapMerge_UseFirst_KeepsFirstValue()
        => Assert.Equal("a", EvalStr("map:merge((map{1:'a'},map{1:'b'}), map{'duplicates':'use-first'})?1"));

    [Fact]
    public void MapMerge_UseLast_KeepsLastValue()
        => Assert.Equal("b", EvalStr("map:merge((map{1:'a'},map{1:'b'}), map{'duplicates':'use-last'})?1"));

    [Fact]
    public void MapMerge_Combine_ConcatenatesValues()
    {
        var result = Evaluate("map:merge((map{1:'a'},map{1:'b'},map{1:'c'}), map{'duplicates':'combine'})?1");
        Assert.True(result.IsSequence);
        var items = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item.ToString());
        Assert.Equal(new[] { "a", "b", "c" }, items);
    }

    [Fact]
    public void MapMerge_Reject_RaisesFOJS0003()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("map:merge((map{1:'a'},map{1:'b'}), map{'duplicates':'reject'})"));
        Assert.Contains("FOJS0003", ex.Message);
    }

    [Fact]
    public void MapMerge_EmptyOptions_RaisesXPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("map:merge((map{1:'a'}), ())"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void MapMerge_UseLast_RetainsNewestKeyObject()
        // same-key-001: the surviving key for "abc" must be the xs:anyURI one.
        => Assert.Equal("true", EvalStr(
            "let $m := map:merge((map:entry(xs:untypedAtomic('abc'),1), map:entry(xs:string('abc'),1), map:entry(xs:anyURI('abc'),1)), map{'duplicates':'use-last'}) " +
            "return map:keys($m)[deep-equal(.,'abc')] instance of xs:anyURI"));

    // ----- map:remove / strict singleton keys ---------------------------

    [Fact]
    public void MapRemove_MultipleKeys()
        => Assert.Equal("1", EvalStr("map:size(map:remove(map{'a':1,'b':2,'c':3}, ('a','c')))"));

    [Fact]
    public void MapGet_EmptyKey_RaisesXPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("map:get(map{'a':1}, ())"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void MapGet_MultiItemKey_RaisesXPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("map:get(map{'a':1}, ('a','b'))"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void MapConstructor_EmptyKey_RaisesXPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("map{():'x'}"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void MapCall_MultiItemKey_RaisesXPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("map{'a':1}(('a','b'))"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    // ----- Numeric and duration key equality ----------------------------

    [Fact]
    public void MapKeys_DecimalAndDouble_AreDistinctWhenPrecisionDiffers()
        // map-put-023: decimal 1.0000000000100000000001 and double 1.00000000001 are different keys.
        => Assert.Equal("2", EvalStr("map:size(map:put(map:put(map{}, 1.0000000000100000000001, 1), xs:double('1.00000000001'), 2))"));

    [Fact]
    public void MapKeys_IntegerAndDouble_SameValue_AreSameKey()
        => Assert.Equal("1", EvalStr("map:size(map:put(map:put(map{}, 1, 'a'), xs:double('1'), 'b'))"));

    [Fact]
    public void MapKeys_Duration_NormalizedEquality()
        => Assert.Equal("true", EvalStr("map:contains(map{xs:duration('P1Y'):'x'}, xs:yearMonthDuration('P12M'))"));

    // ----- Array bounds -------------------------------------------------

    [Theory]
    [InlineData("array:get([5,6,7], 0)")]
    [InlineData("array:get([5,6,7], 4)")]
    [InlineData("array:head([])")]
    [InlineData("array:tail([])")]
    [InlineData("array:put([4,5], 0, 'a')")]
    [InlineData("array:put([4,5], 3, 'a')")]
    [InlineData("array:remove([], 1)")]
    [InlineData("array:remove(['a','b'], (2 to 3))")]
    [InlineData("array:insert-before([], 2, ())")]
    [InlineData("array:insert-before([1,2], 0, 'x')")]
    [InlineData("array:subarray([1,2,3], 0)")]
    [InlineData("array:subarray([1,2,3], 2, 3)")]
    [InlineData("array:subarray([1,2,3,4,5], 4294967297, 2)")]
    public void ArrayBounds_RaiseFOAY0001(string expr)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate(expr));
        Assert.Contains("FOAY0001", ex.Message);
    }

    [Fact]
    public void ArraySubarray_NegativeLength_RaisesFOAY0002()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("array:subarray([1,2,3], 2, -1)"));
        Assert.Contains("FOAY0002", ex.Message);
    }

    [Fact]
    public void ArrayInsertBefore_AppendAtCountPlusOne_Succeeds()
        => Assert.Equal("3", EvalStr("array:size(array:insert-before([1,2], 3, 'x'))"));

    // ----- Effective boolean value --------------------------------------

    [Theory]
    [InlineData("if ([1,2]) then 1 else 2")]
    [InlineData("if (map{}) then 1 else 2")]
    [InlineData("not(map{})")]
    public void EffectiveBooleanValue_FunctionItems_RaiseFORG0006(string expr)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate(expr));
        Assert.Contains("FORG0006", ex.Message);
    }

    [Fact]
    public void Exists_IsCountBased_NotEbv()
    {
        Assert.Equal("true", EvalStr("exists((0))"));
        Assert.Equal("true", EvalStr("exists(map{})"));
        Assert.Equal("false", EvalStr("empty(map{})"));
    }

    // ----- Parameterized map/array type tests ---------------------------

    [Theory]
    [InlineData("map{1:'a', 2:'b'} instance of map(xs:integer, xs:string)", "true")]
    [InlineData("map{1:'a', 'x':1} instance of map(xs:integer, xs:string)", "false")]
    [InlineData("map{'a':()} instance of map(xs:string, empty-sequence())", "true")]
    [InlineData("map{'a':(), 'b':5} instance of map(xs:string, empty-sequence())", "false")]
    [InlineData("map{'a':1, 'b':()} instance of map(xs:string, xs:integer+)", "false")]
    [InlineData("[('A','B'),'C'] instance of array(xs:string)", "false")]
    [InlineData("[(),'A'] instance of array(xs:string)", "false")]
    [InlineData("[1,2] instance of array(xs:integer)", "true")]
    [InlineData("map{1:'a'} instance of map(xs:integer)", "ERROR:XPST0003")]
    [InlineData("map{1:'a'} instance of map(xs:string+, xs:integer+)", "ERROR:XPST0003")]
    [InlineData("map{1:'a'} instance of map(integer, string)", "ERROR:XPST0051")]
    public void ParameterizedTypeTests(string expr, string expected)
    {
        if (expected.StartsWith("ERROR:"))
        {
            var ex = Assert.Throws<InvalidOperationException>(() => Evaluate(expr));
            Assert.Contains(expected[6..], ex.Message);
        }
        else
        {
            Assert.Equal(expected, EvalStr(expr));
        }
    }

    // ----- Maps and arrays as function items ----------------------------

    [Theory]
    [InlineData("map{1:'a'} instance of function(*)", "true")]
    [InlineData("[1,2] instance of function(*)", "true")]
    [InlineData("map{1:'A','x':'B'} instance of function(xs:integer) as xs:string?", "true")]
    [InlineData("map{1:'A','x':'B'} instance of function(xs:integer) as xs:string", "false")]
    [InlineData("map{} instance of function(xs:integer) as empty-sequence()", "true")]
    [InlineData("map{12:()} instance of function(xs:decimal) as xs:string*", "true")]
    [InlineData("map{12:'z'} instance of function(xs:decimal) as xs:string", "false")]
    [InlineData("[['A'],['B']] instance of function(xs:integer) as item()*", "true")]
    public void MapsAndArraysAsFunctionItems(string expr, string expected)
        => Assert.Equal(expected, EvalStr(expr));

    // ----- Function type subsumption ------------------------------------

    [Theory]
    [InlineData("function($m as map(*)) as xs:integer {map:size($m)} instance of function(map(xs:integer, xs:string)) as xs:integer", "true")]
    [InlineData("function($m as map(xs:decimal, xs:string+)) as xs:integer {map:size($m)} instance of function(map(xs:integer, xs:string)) as xs:integer", "true")]
    [InlineData("function($m as function(*)) as xs:integer {function-arity($m)} instance of function(map(*)) as xs:integer", "true")]
    [InlineData("function($m as function(xs:anyAtomicType) as item()*) as xs:integer {map:size($m)} instance of function(map(xs:integer, xs:string)) as xs:integer", "true")]
    [InlineData("fn:floor#1 instance of function(xs:numeric) as xs:numeric", "false")]
    [InlineData("fn:floor#1 instance of function(xs:numeric?) as xs:numeric?", "true")]
    public void FunctionTypeSubsumption(string expr, string expected)
        => Assert.Equal(expected, EvalStr(expr));

    // ----- deep-equal ----------------------------------------------------

    [Fact]
    public void DeepEqual_MapKeys_IgnoreCollation()
        => Assert.Equal("false", EvalStr(
            "deep-equal(map{'a':1}, map{'A':1}, 'http://www.w3.org/2013/collation/UCA?strength=secondary')"));
}

// ===========================================================================================================================================================
// Tier-2j: FLWOR completion — 'at $pos' positional variables, 'where' clause, mixed for/let chains,
// plus strict arithmetic / EBV / atomization type checking.
// ===========================================================================================================================================================
public class Tier2jFlworTests
{
    private static XdmValue Evaluate(string xpath)
    {
        var ctx = new EvaluationContext();
        FunctionLibrary.Populate(ctx);
        return XPath31Expression.Compile(xpath).Evaluate(ctx);
    }

    private static string EvalStr(string xpath) => Evaluate(xpath).ToString();

    private static IReadOnlyList<string> EvalItems(string xpath)
    {
        var result = Evaluate(xpath);
        var items = new List<string>();
        if (result.IsSequence && result.SequenceValue is not null)
            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                items.Add(item.ToString());
        else if (!result.IsUndefined)
            items.Add(result.ToString());
        return items;
    }

    // ----- Positional variables (at $pos) -------------------------------

    [Fact]
    public void ForPositional_BindsOneBasedPosition()
        => Assert.Equal(new[] { "1", "2", "3" }, EvalItems("for $i at $p in (10, 20, 30) return $p"));

    [Fact]
    public void ForPositional_PairsWithItem()
        => Assert.Equal(new[] { "10", "1", "20", "2", "30", "3" },
            EvalItems("for $i at $p in (10, 20, 30) return ($i, $p)"));

    [Fact]
    public void ForPositional_EmptyInput_BindsNothing()
        => Assert.Empty(EvalItems("for $i at $p in () return $p"));

    [Fact]
    public void ForPositional_MultipleBindings_EachTracksOwnSequence()
        => Assert.Equal(new[] { "1", "1", "1", "2", "2", "1", "2", "2" },
            EvalItems("for $a at $p1 in (1, 2), $b at $p2 in (1, 2) return ($p1, $p2)"));

    [Fact]
    public void ForPositional_OutOfScopeAfterLoop_RaisesXPST0008()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("for $a at $p in (1, 2) return 1, $p"));
        Assert.Contains("XPST0008", ex.Message);
    }

    [Fact]
    public void ForPositional_MissingDollar_IsParseError()
        => Assert.Throws<Bosak.XPath.Parser.ParseException>(() => Evaluate("for $a at p1 in 1 return 1"));

    [Fact]
    public void ForPositional_RestoresShadowedOuterVariable()
        => Assert.Equal(new[] { "7", "outer" }, EvalItems(
            "let $p := 'outer' return ((for $i at $p in (7, 8) return $i)[1], $p)"));

    // ----- where clause ---------------------------------------------------

    [Fact]
    public void Where_FiltersItems()
        => Assert.Equal(new[] { "3" }, EvalItems("(for $fo in (1, 2, 3) where $fo eq 3 return $fo)"));

    [Fact]
    public void Where_FalseCondition_YieldsEmpty()
        => Assert.Equal("true", EvalStr("empty(for $i in 1 where false() return $i)"));

    [Fact]
    public void Where_OnLet_FiltersSingleTuple()
        => Assert.Equal("true", EvalStr("let $var := (fn:true()) where $var or fn:true() return $var"));

    [Fact]
    public void Where_UsesPositionalVariable()
        => Assert.Equal(new[] { "10", "30" },
            EvalItems("for $file at $offset in (10, 20, 30, 40) where $offset mod 2 = 1 return $file"));

    [Fact]
    public void Where_MultipleClauses_BothApply()
        => Assert.Empty(EvalItems("for $x in (1, 2) where true() where false() return $x"));

    [Fact]
    public void Where_NonBooleanEbvAtomic_RaisesFORG0006()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            Evaluate("count((for $fo in (1, 2, 3) where xs:time('08:08:23Z') return $fo))"));
        Assert.Contains("FORG0006", ex.Message);
    }

    // ----- Mixed for/let chains -------------------------------------------

    [Fact]
    public void Chain_LetLet_BindsSequentially()
        => Assert.Equal("2", EvalStr("let $x := 1 let $z := $x + 1 return $z"));

    [Fact]
    public void Chain_LetLet_UndefinedVariable_RaisesXPST0008()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("let $x := 1 let $z := $x + $y return $x"));
        Assert.Contains("XPST0008", ex.Message);
    }

    [Fact]
    public void Chain_ForFor_InnerSeesOuterBinding()
        => Assert.Equal(new[] { "2", "5", "4", "6", "6", "7" },
            EvalItems("for $x in (1, 2, 3) for $z in ($x, 4) return $x + $z"));

    [Fact]
    public void Chain_ForLet_LetSeesPositionalVariable()
        => Assert.Equal(new[] { "2", "1", "3", "2", "4", "3", "5", "4" },
            EvalItems("for $i at $pos in (3 to 6) let $let := $pos + 1 return ($let, $let - 1)"));

    [Fact]
    public void Chain_VariableNamedWhere_StillWorks()
        => Assert.Equal(new[] { "4", "5" }, EvalItems("for $where in (4, 5) return $where"));

    // ----- Strict arithmetic operands -------------------------------------

    [Theory]
    [InlineData("\"2\" + 1")]
    [InlineData("1 + true()")]
    [InlineData("-(\"2\")")]
    [InlineData("for $i at $p in (1, 2, 3) return $p + \"1\"")]
    public void Arithmetic_NonNumericOperand_RaisesXPTY0004(string expr)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate(expr));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Arithmetic_MultiItemOperand_RaisesXPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("(1, 2, 3) + 1"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Arithmetic_UntypedAtomicOperand_PromotesToDouble()
        => Assert.Equal("3", EvalStr("let $x := xs:untypedAtomic('2') return $x + 1"));

    [Fact]
    public void Arithmetic_DatePlusDuration_StillWorks()
        => Assert.Equal("2020-01-02", EvalStr("xs:date('2020-01-01') + xs:dayTimeDuration('P1D')"));

    // ----- EBV strictness --------------------------------------------------

    [Theory]
    [InlineData("if (xs:date('2020-01-01')) then 1 else 2")]
    [InlineData("if (xs:dayTimeDuration('P1D')) then 1 else 2")]
    public void Ebv_NonBooleanAtomic_RaisesFORG0006(string expr)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate(expr));
        Assert.Contains("FORG0006", ex.Message);
    }

    [Theory]
    [InlineData("if (xs:anyURI('http://x')) then 1 else 2", "1")]
    [InlineData("if (xs:anyURI('')) then 1 else 2", "2")]
    public void Ebv_AnyUri_BehavesLikeString(string expr, string expected)
        => Assert.Equal(expected, EvalStr(expr));

    // ----- Numeric function strictness ------------------------------------

    [Theory]
    [InlineData("fn:abs('a')")]
    [InlineData("fn:floor('2.5')")]
    [InlineData("fn:ceiling('2.5')")]
    [InlineData("fn:round('2.5')")]
    [InlineData("fn:abs(true())")]
    public void NumericFunction_StringOrBooleanArg_RaisesXPTY0004(string expr)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate(expr));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Sum_StringItem_RaisesFORG0006()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:sum(('a', 1))"));
        Assert.Contains("FORG0006", ex.Message);
    }

    [Fact]
    public void Avg_StringItem_RaisesFORG0006()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Evaluate("fn:avg(('a', 1))"));
        Assert.Contains("FORG0006", ex.Message);
    }

    [Fact]
    public void Sum_UntypedAtomicItems_SumsAsDouble()
        => Assert.Equal("3", EvalStr("fn:sum((xs:untypedAtomic('1'), xs:untypedAtomic('2')))"));

    // ----- Regression sanity ----------------------------------------------

    [Fact]
    public void For_SimpleIteration_Unchanged()
        => Assert.Equal(new[] { "2", "4", "6" }, EvalItems("for $x in (1, 2, 3) return $x * 2"));

    [Fact]
    public void QuantifiedExpressions_Unchanged()
    {
        Assert.Equal("true", EvalStr("some $x in (1, 2) satisfies $x > 1"));
        Assert.Equal("true", EvalStr("every $x in (1, 2) satisfies $x > 0"));
    }

    [Fact]
    public void GeneralComparison_StillExistential()
    {
        Assert.Equal("true", EvalStr("\"a\" = (\"a\", \"b\")"));
        Assert.Equal("true", EvalStr("(1, 2) = (2, 3)"));
    }
}
