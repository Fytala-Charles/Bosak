// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : End-to-end tests that compile and evaluate XPath expressions against real XML documents
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Added end-to-end tests for string, sequence, and aggregate functions                   |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Added tests for Intersect, Except, and SimpleMap operators                             |
//                      | Charles Korthout | 0.4   | 19-05-2026     | Added tests for Map, Array, and Lookup                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Core.Tests;

public class EndToEndTests
{
    private const string BooksXml = """
<library xmlns:pub="http://example.com/publisher">
  <book id="b1" genre="fiction">
    <title>The Great Gatsby</title>
    <author>F. Scott Fitzgerald</author>
    <price>10.99</price>
    <pub:publisher>Scribner</pub:publisher>
  </book>
  <book id="b2" genre="sci-fi">
    <title>Dune</title>
    <author>Frank Herbert</author>
    <price>12.50</price>
    <pub:publisher>Ace</pub:publisher>
  </book>
  <book id="b3" genre="fiction">
    <title>1984</title>
    <author>George Orwell</author>
    <price>8.99</price>
    <pub:publisher>Secker</pub:publisher>
  </book>
</library>
""";

    private static IXdmNode LoadDocument() => XDocumentProvider.ParseXml(BooksXml);

    private static XdmValue Evaluate(string xpath, IXdmNode contextItem)
    {
        var expr = XPath31Expression.Compile(xpath);
        return expr.Evaluate(contextItem);
    }

    private static string[] EvaluateStrings(string xpath, IXdmNode contextItem)
    {
        var result = Evaluate(xpath, contextItem);
        var items = Materialize(result);
        return items.Select(i => i.ToString()).ToArray();
    }

    private static long[] EvaluateIntegers(string xpath, IXdmNode contextItem)
    {
        var result = Evaluate(xpath, contextItem);
        var items = Materialize(result);
        return items.Select(i => i.IntegerValue).ToArray();
    }

    private static XdmValue[] Materialize(XdmValue sequence)
    {
        if (sequence.IsUndefined)
            return Array.Empty<XdmValue>();
        if (sequence.IsSequence && sequence.SequenceValue is not null)
        {
            var list = new List<XdmValue>();
            foreach (var item in XdmSequence.FromSource(sequence.SequenceValue))
                list.Add(item);
            return list.ToArray();
        }
        return new[] { sequence };
    }

    // ------------------------------------------------------------------
    // Root & context item
    // ------------------------------------------------------------------

    [Fact]
    public void ContextItem_Root()
    {
        var doc = LoadDocument();
        var result = Evaluate(".", doc);
        Assert.True(result.IsNode);
        Assert.Equal(XdmNodeKind.Document, result.NodeValue.NodeKind);
    }

    [Fact]
    public void RootElement()
    {
        var result = EvaluateStrings("/library/title", LoadDocument());
        Assert.Empty(result); // library has no direct title child
    }

    [Fact]
    public void RootElement_Children()
    {
        var result = EvaluateStrings("/library/book/title", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Contains("The Great Gatsby", result);
        Assert.Contains("Dune", result);
        Assert.Contains("1984", result);
    }

    // ------------------------------------------------------------------
    // Child axis
    // ------------------------------------------------------------------

    [Fact]
    public void ChildAxis_Element()
    {
        var result = EvaluateStrings("/library/book/author", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal("F. Scott Fitzgerald", result[0]);
        Assert.Equal("Frank Herbert", result[1]);
        Assert.Equal("George Orwell", result[2]);
    }

    [Fact]
    public void ChildAxis_Wildcard()
    {
        var result = EvaluateStrings("/library/book/*", LoadDocument());
        Assert.Equal(12, result.Length); // 4 children × 3 books = title, author, price, publisher
    }

    // ------------------------------------------------------------------
    // Descendant axis
    // ------------------------------------------------------------------

    [Fact]
    public void DescendantAxis_DoubleSlash()
    {
        var result = EvaluateStrings("//title", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal("The Great Gatsby", result[0]);
        Assert.Equal("Dune", result[1]);
        Assert.Equal("1984", result[2]);
    }

    [Fact]
    public void DescendantAxis_Explicit()
    {
        var result = EvaluateStrings("/library/descendant::price", LoadDocument());
        Assert.Equal(3, result.Length);
    }

    // ------------------------------------------------------------------
    // Attribute axis
    // ------------------------------------------------------------------

    [Fact]
    public void AttributeAxis_Abbreviated()
    {
        var result = EvaluateStrings("//book/@id", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal("b1", result[0]);
        Assert.Equal("b2", result[1]);
        Assert.Equal("b3", result[2]);
    }

    [Fact]
    public void AttributeAxis_Explicit()
    {
        var result = EvaluateStrings("//book/attribute::genre", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal("fiction", result[0]);
        Assert.Equal("sci-fi", result[1]);
        Assert.Equal("fiction", result[2]);
    }

    // ------------------------------------------------------------------
    // Parent axis
    // ------------------------------------------------------------------

    [Fact]
    public void ParentAxis()
    {
        var result = EvaluateStrings("//title/..", LoadDocument());
        Assert.Equal(3, result.Length);
        // Parent is <book> — StringValue is concatenated text content
        Assert.Contains("The Great Gatsby", result[0]);
    }

    [Fact]
    public void ParentAxis_Abbreviated()
    {
        var result = EvaluateStrings("//title/../@id", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal("b1", result[0]);
    }

    // ------------------------------------------------------------------
    // Self axis
    // ------------------------------------------------------------------

    [Fact]
    public void SelfAxis()
    {
        var result = EvaluateStrings("//title/self::title", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal("The Great Gatsby", result[0]);
    }

    // ------------------------------------------------------------------
    // Predicates
    // ------------------------------------------------------------------

    [Fact]
    public void Predicate_NumericSubscript()
    {
        var result = EvaluateStrings("//book[1]/title", LoadDocument());
        Assert.Single(result);
        Assert.Equal("The Great Gatsby", result[0]);
    }

    [Fact]
    public void Predicate_NumericSubscript_Last()
    {
        var result = EvaluateStrings("//book[last()]/title", LoadDocument());
        Assert.Single(result);
        Assert.Equal("1984", result[0]);
    }

    [Fact]
    public void Predicate_AttributeFilter()
    {
        var result = EvaluateStrings("//book[@genre='fiction']/title", LoadDocument());
        Assert.Equal(2, result.Length);
        Assert.Equal("The Great Gatsby", result[0]);
        Assert.Equal("1984", result[1]);
    }

    [Fact]
    public void Predicate_ChildContentFilter()
    {
        var result = EvaluateStrings("//book[price gt 10]/title", LoadDocument());
        Assert.Equal(2, result.Length);
        Assert.Equal("The Great Gatsby", result[0]);
        Assert.Equal("Dune", result[1]);
    }

    [Fact]
    public void Predicate_PositionFunction()
    {
        var result = EvaluateStrings("//book[position() = 2]/title", LoadDocument());
        Assert.Single(result);
        Assert.Equal("Dune", result[0]);
    }

    // ------------------------------------------------------------------
    // Arithmetic in paths
    // ------------------------------------------------------------------

    [Fact]
    public void Arithmetic_InContext()
    {
        // This tests that the context item is correctly passed through the VM
        var doc = LoadDocument();
        var ctx = new EvaluationContext()
            .WithFocus(XdmValue.FromNode(doc), 1, 1);
        FunctionLibrary.Populate(ctx);

        var expr = XPath31Expression.Compile("count(//book)");
        var result = expr.Evaluate(ctx);
        Assert.Equal(3, result.IntegerValue);
    }

    [Fact]
    public void FunctionCall_CountBooks()
    {
        var result = Evaluate("count(//book)", LoadDocument());
        Assert.Equal(3, result.IntegerValue);
    }

    [Fact]
    public void FunctionCall_Exists()
    {
        var result = Evaluate("exists(//book[@genre='sci-fi'])", LoadDocument());
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void FunctionCall_String()
    {
        var result = Evaluate("string(//book[1]/price)", LoadDocument());
        Assert.Equal("10.99", result.StringValue);
    }

    // ------------------------------------------------------------------
    // Following / Preceding siblings
    // ------------------------------------------------------------------

    [Fact]
    public void FollowingSiblingAxis()
    {
        var result = EvaluateStrings("//book[1]/following-sibling::book/title", LoadDocument());
        Assert.Equal(2, result.Length);
        Assert.Equal("Dune", result[0]);
        Assert.Equal("1984", result[1]);
    }

    [Fact]
    public void PrecedingSiblingAxis()
    {
        var result = EvaluateStrings("//book[3]/preceding-sibling::book/title", LoadDocument());
        Assert.Equal(2, result.Length);
        // Path expression result is always in document order.
        Assert.Equal("The Great Gatsby", result[0]);
        Assert.Equal("Dune", result[1]);
    }

    // ------------------------------------------------------------------
    // Following / Preceding axes
    // ------------------------------------------------------------------

    [Fact]
    public void FollowingAxis()
    {
        var result = EvaluateStrings("//book[1]/title/following::title", LoadDocument());
        Assert.Equal(2, result.Length);
        Assert.Equal("Dune", result[0]);
        Assert.Equal("1984", result[1]);
    }

    [Fact]
    public void PrecedingAxis()
    {
        var result = EvaluateStrings("//book[3]/title/preceding::title", LoadDocument());
        Assert.Equal(2, result.Length);
        // Path expression result is always in document order.
        Assert.Equal("The Great Gatsby", result[0]);
        Assert.Equal("Dune", result[1]);
    }

    // ------------------------------------------------------------------
    // Ancestor axis
    // ------------------------------------------------------------------

    [Fact]
    public void AncestorAxis()
    {
        var result = EvaluateStrings("//title/ancestor::*", LoadDocument());
        // Ancestor axis on each title produces (book, library).
        // After deduplication and document-order sorting: 3 unique books + 1 library = 4.
        Assert.Equal(4, result.Length);
    }

    [Fact]
    public void AncestorOrSelfAxis()
    {
        var result = EvaluateStrings("//title/ancestor-or-self::*", LoadDocument());
        // 3 titles + 3 books + 1 library (deduplicated) = 7.
        Assert.Equal(7, result.Length);
    }

    // ------------------------------------------------------------------
    // Complex expressions
    // ------------------------------------------------------------------

    [Fact]
    public void Complex_MultiplePredicates()
    {
        var result = EvaluateStrings("//book[@genre='fiction'][price lt 10]/title", LoadDocument());
        Assert.Single(result);
        Assert.Equal("1984", result[0]);
    }

    [Fact]
    public void Complex_PathWithCondition()
    {
        var result = EvaluateStrings("//book[author='Frank Herbert']/title", LoadDocument());
        Assert.Single(result);
        Assert.Equal("Dune", result[0]);
    }

    [Fact]
    public void Complex_NestedPath()
    {
        var result = EvaluateStrings("/library/*[2]/*[1]", LoadDocument());
        Assert.Single(result);
        Assert.Equal("Dune", result[0]);
    }

    // ------------------------------------------------------------------
    // Document order & deduplication
    // ------------------------------------------------------------------

    [Fact]
    public void Dedup_AncestorAxis()
    {
        // library is the ancestor of all 3 titles; should appear once after dedup.
        var result = EvaluateStrings("//title/ancestor::*", LoadDocument());
        Assert.Equal(4, result.Length); // 3 books + 1 library
    }

    [Fact]
    public void Dedup_AncestorOrSelfAxis()
    {
        var result = EvaluateStrings("//title/ancestor-or-self::*", LoadDocument());
        Assert.Equal(7, result.Length); // 3 titles + 3 books + 1 library
    }

    [Fact]
    public void Dedup_Union()
    {
        var result = EvaluateStrings("//book[1]/title | //book[3]/title", LoadDocument());
        Assert.Equal(2, result.Length);
        Assert.Equal("The Great Gatsby", result[0]);
        Assert.Equal("1984", result[1]);
    }

    [Fact]
    public void DocumentOrder_ReverseAxisNormalized()
    {
        // preceding-sibling is reverse axis, but path result is document order.
        var result = EvaluateStrings("//book[3]/preceding-sibling::book/title", LoadDocument());
        Assert.Equal(2, result.Length);
        Assert.Equal("The Great Gatsby", result[0]);
        Assert.Equal("Dune", result[1]);
    }

    // ------------------------------------------------------------------
    // String functions
    // ------------------------------------------------------------------

    [Fact]
    public void FunctionCall_ConcatVarargs()
    {
        var result = Evaluate("concat('a', 'b', 'c')", LoadDocument());
        Assert.Equal("abc", result.StringValue);
    }

    [Fact]
    public void FunctionCall_StringLength()
    {
        var result = Evaluate("string-length('hello')", LoadDocument());
        Assert.Equal(5, result.IntegerValue);
    }

    [Fact]
    public void FunctionCall_Substring2()
    {
        var result = Evaluate("substring('hello', 2)", LoadDocument());
        Assert.Equal("ello", result.StringValue);
    }

    [Fact]
    public void FunctionCall_Substring3()
    {
        var result = Evaluate("substring('hello', 2, 2)", LoadDocument());
        Assert.Equal("el", result.StringValue);
    }

    [Fact]
    public void FunctionCall_Contains()
    {
        var result = Evaluate("contains('hello', 'ell')", LoadDocument());
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void FunctionCall_StartsWith()
    {
        var result = Evaluate("starts-with('hello', 'he')", LoadDocument());
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void FunctionCall_EndsWith()
    {
        var result = Evaluate("ends-with('hello', 'lo')", LoadDocument());
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void FunctionCall_NormalizeSpace()
    {
        var result = Evaluate("normalize-space('  hello   world  ')", LoadDocument());
        Assert.Equal("hello world", result.StringValue);
    }

    [Fact]
    public void FunctionCall_Translate()
    {
        var result = Evaluate("translate('hello', 'el', 'xy')", LoadDocument());
        Assert.Equal("hxyyo", result.StringValue);
    }

    [Fact]
    public void FunctionCall_UpperCase()
    {
        var result = Evaluate("upper-case('hello')", LoadDocument());
        Assert.Equal("HELLO", result.StringValue);
    }

    [Fact]
    public void FunctionCall_LowerCase()
    {
        var result = Evaluate("lower-case('HELLO')", LoadDocument());
        Assert.Equal("hello", result.StringValue);
    }

    [Fact]
    public void FunctionCall_Matches()
    {
        var result = Evaluate("matches('hello', 'h.*o')", LoadDocument());
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void FunctionCall_MatchesWithFlags()
    {
        var result = Evaluate("matches('HELLO', 'hello', 'i')", LoadDocument());
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void FunctionCall_Replace()
    {
        var result = Evaluate("replace('hello world', 'world', 'xpath')", LoadDocument());
        Assert.Equal("hello xpath", result.StringValue);
    }

    [Fact]
    public void FunctionCall_Tokenize()
    {
        var result = EvaluateStrings("tokenize('a,b,c', ',')", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal("a", result[0]);
        Assert.Equal("b", result[1]);
        Assert.Equal("c", result[2]);
    }

    // ------------------------------------------------------------------
    // Sequence functions
    // ------------------------------------------------------------------

    [Fact]
    public void FunctionCall_InsertBefore()
    {
        var result = EvaluateStrings("insert-before(('a', 'b', 'c'), 2, 'x')", LoadDocument());
        Assert.Equal(4, result.Length);
        Assert.Equal("a", result[0]);
        Assert.Equal("x", result[1]);
        Assert.Equal("b", result[2]);
        Assert.Equal("c", result[3]);
    }

    [Fact]
    public void FunctionCall_Remove()
    {
        var result = EvaluateStrings("remove(('a', 'b', 'c'), 2)", LoadDocument());
        Assert.Equal(2, result.Length);
        Assert.Equal("a", result[0]);
        Assert.Equal("c", result[1]);
    }

    [Fact]
    public void FunctionCall_Reverse()
    {
        var result = EvaluateStrings("reverse(('a', 'b', 'c'))", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal("c", result[0]);
        Assert.Equal("b", result[1]);
        Assert.Equal("a", result[2]);
    }

    [Fact]
    public void FunctionCall_Subsequence2()
    {
        var result = EvaluateStrings("subsequence(('a', 'b', 'c', 'd'), 2)", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal("b", result[0]);
        Assert.Equal("c", result[1]);
        Assert.Equal("d", result[2]);
    }

    [Fact]
    public void FunctionCall_Subsequence3()
    {
        var result = EvaluateStrings("subsequence(('a', 'b', 'c', 'd'), 2, 2)", LoadDocument());
        Assert.Equal(2, result.Length);
        Assert.Equal("b", result[0]);
        Assert.Equal("c", result[1]);
    }

    [Fact]
    public void FunctionCall_DistinctValues()
    {
        var result = EvaluateStrings("distinct-values(('a', 'b', 'a', 'c'))", LoadDocument());
        Assert.Equal(3, result.Length);
    }

    [Fact]
    public void FunctionCall_IndexOf()
    {
        var result = EvaluateIntegers("index-of(('a', 'b', 'a', 'c'), 'a')", LoadDocument());
        Assert.Equal(2, result.Length);
        Assert.Equal(1, result[0]);
        Assert.Equal(3, result[1]);
    }

    // ------------------------------------------------------------------
    // Aggregate functions
    // ------------------------------------------------------------------

    [Fact]
    public void FunctionCall_Sum()
    {
        var result = Evaluate("sum((1, 2, 3))", LoadDocument());
        Assert.Equal(6m, result.DecimalValue);
    }

    [Fact]
    public void FunctionCall_SumEmpty()
    {
        var result = Evaluate("sum(())", LoadDocument());
        Assert.Equal(0, result.IntegerValue);
    }

    [Fact]
    public void FunctionCall_Avg()
    {
        var result = Evaluate("avg((1, 2, 3))", LoadDocument());
        Assert.Equal(2m, result.DecimalValue);
    }

    [Fact]
    public void FunctionCall_Min()
    {
        var result = Evaluate("min((3, 1, 2))", LoadDocument());
        Assert.Equal(1m, result.DecimalValue);
    }

    [Fact]
    public void FunctionCall_Max()
    {
        var result = Evaluate("max((3, 1, 2))", LoadDocument());
        Assert.Equal(3m, result.DecimalValue);
    }

    [Fact]
    public void FunctionCall_StringJoin()
    {
        var result = Evaluate("string-join(('a', 'b', 'c'), '-')", LoadDocument());
        Assert.Equal("a-b-c", result.StringValue);
    }

    // ------------------------------------------------------------------
    // Set operators
    // ------------------------------------------------------------------

    [Fact]
    public void Intersect_SameNodes()
    {
        var result = EvaluateStrings("//book intersect //book", LoadDocument());
        Assert.Equal(3, result.Length);
    }

    [Fact]
    public void Intersect_Subset()
    {
        var result = EvaluateStrings("//book intersect //book[@genre='fiction']", LoadDocument());
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public void Except_RemovesNodes()
    {
        var result = EvaluateStrings("//book except //book[@genre='sci-fi']", LoadDocument());
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public void SimpleMap_PathToString()
    {
        var result = EvaluateStrings("//book/title ! string(.)", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal("The Great Gatsby", result[0]);
        Assert.Equal("Dune", result[1]);
        Assert.Equal("1984", result[2]);
    }

    [Fact]
    public void SimpleMap_SequenceToString()
    {
        var result = EvaluateStrings("(1, 2, 3) ! string(.)", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal("1", result[0]);
        Assert.Equal("2", result[1]);
        Assert.Equal("3", result[2]);
    }

    // ------------------------------------------------------------------
    // Maps and Arrays
    // ------------------------------------------------------------------

    [Fact]
    public void MapConstructor_Lookup()
    {
        var result = Evaluate("map { 'a': 1, 'b': 2 }?'a'", LoadDocument());
        Assert.Equal(1, result.IntegerValue);
    }

    [Fact]
    public void MapConstructor_LookupMissing()
    {
        var result = Evaluate("map { 'a': 1 }?'z'", LoadDocument());
        Assert.True(result.IsUndefined);
    }

    [Fact]
    public void MapLookupWildcard()
    {
        var result = EvaluateStrings("map { 'a': 1, 'b': 2 }?*", LoadDocument());
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public void MapFunction_Size()
    {
        var result = Evaluate("map:size(map { 'a': 1, 'b': 2 })", LoadDocument());
        Assert.Equal(2, result.IntegerValue);
    }

    [Fact]
    public void MapFunction_Contains()
    {
        var result = Evaluate("map:contains(map { 'a': 1 }, 'a')", LoadDocument());
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void MapFunction_Keys()
    {
        var result = EvaluateStrings("map:keys(map { 'a': 1, 'b': 2 })", LoadDocument());
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public void MapFunction_Merge()
    {
        var result = Evaluate("map:size(map:merge((map { 'a': 1 }, map { 'b': 2 })))", LoadDocument());
        Assert.Equal(2, result.IntegerValue);
    }

    [Fact]
    public void ArrayConstructor_Lookup()
    {
        var result = Evaluate("[10, 20, 30]?2", LoadDocument());
        Assert.Equal(20, result.IntegerValue);
    }

    [Fact]
    public void ArrayConstructor_LookupWildcard()
    {
        var result = EvaluateIntegers("[10, 20, 30]?*", LoadDocument());
        Assert.Equal(3, result.Length);
        Assert.Equal(10, result[0]);
        Assert.Equal(20, result[1]);
        Assert.Equal(30, result[2]);
    }

    [Fact]
    public void ArrayFunction_Size()
    {
        var result = Evaluate("array:size([1, 2, 3])", LoadDocument());
        Assert.Equal(3, result.IntegerValue);
    }

    [Fact]
    public void ArrayFunction_Get()
    {
        var result = Evaluate("array:get([1, 2, 3], 2)", LoadDocument());
        Assert.Equal(2, result.IntegerValue);
    }

    [Fact]
    public void ArrayFunction_Contains()
    {
        var result = Evaluate("array:contains([1, 2, 3], 2)", LoadDocument());
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void ArrayFunction_Head()
    {
        var result = Evaluate("array:head([1, 2, 3])", LoadDocument());
        Assert.Equal(1, result.IntegerValue);
    }

    [Fact]
    public void ArrayFunction_Tail()
    {
        var result = EvaluateIntegers("array:tail([1, 2, 3])?*", LoadDocument());
        Assert.Equal(2, result.Length);
        Assert.Equal(2, result[0]);
        Assert.Equal(3, result[1]);
    }
}
