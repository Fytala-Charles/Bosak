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
        Assert.Equal(3.0, result.DoubleValue);
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
        var result = XPath31Expression.Compile("fn:name(ns:child)").Evaluate(node);
        Assert.Equal("ns:child", result.ToString());
    }

    [Fact]
    public void Name_DefaultNamespace()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root xmlns='http://default.com'><child/></root>");
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("fn:name(child)").Evaluate(node);
        Assert.Equal("child", result.ToString());
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
        Assert.Equal(1, dto.Day);
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
        Assert.Equal("root", result.NodeValue.LocalName);
    }

    [Fact]
    public void Root_Argument()
    {
        var doc = System.Xml.Linq.XDocument.Parse("<root><child/></root>");
        var root = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        var result = XPath31Expression.Compile("fn:root(child)").Evaluate(root);
        Assert.Equal(XdmValueKind.Node, result.Kind);
        Assert.Equal("root", result.NodeValue.LocalName);
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
        var node = new Bosak.XPath.Providers.Xml.XDocumentNode(doc.Root!);
        Assert.Equal("true", EvalStr("fn:deep-equal(root,root)"));
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
        Assert.Equal(3, result.Length);
        Assert.Equal("a", result[0]);
        Assert.Equal("b", result[1]);
        Assert.Equal("c", result[2]);
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
        Assert.Equal(2, result.Length);
        Assert.Equal("a", result[0]);
        Assert.Equal("b", result[1]);
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
        var result = Evaluate("'2024-01-15T10:30:00' cast as xs:date");
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

    [Fact]
    public void PredicateIndexing_Subscript()
    {
        Assert.Equal("10", EvalSequence("(10, 20, 30)[1]")[0]);
        Assert.Equal("20", EvalSequence("(10, 20, 30)[2]")[0]);
        Assert.Equal("30", EvalSequence("(10, 20, 30)[3]")[0]);
        Assert.Empty(EvalSequence("(10, 20, 30)[4]"));
        Assert.Empty(EvalSequence("(10, 20, 30)[0]"));
    }

    [Fact]
    public void PredicateIndexing_OnAtomic()
    {
        Assert.Equal("42", EvalSequence("42[1]")[0]);
        Assert.Empty(EvalSequence("42[2]"));
    }

    [Fact]
    public void PredicateIndexing_EmptySequence()
    {
        Assert.Empty(EvalSequence("()[1]"));
        Assert.Empty(EvalSequence("()[last()]"));
    }

    [Fact]
    public void PredicateIndexing_Last()
    {
        Assert.Equal("30", EvalSequence("(10, 20, 30)[last()]")[0]);
        Assert.Equal("10", EvalSequence("(10)[last()]")[0]);
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
}
