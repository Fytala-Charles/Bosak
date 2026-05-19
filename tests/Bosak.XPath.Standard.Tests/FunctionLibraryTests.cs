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
}
