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
        Assert.Equal("Dune", result[0]);
        Assert.Equal("The Great Gatsby", result[1]);
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
        Assert.Equal("Dune", result[0]);
        Assert.Equal("The Great Gatsby", result[1]);
    }

    // ------------------------------------------------------------------
    // Ancestor axis
    // ------------------------------------------------------------------

    [Fact]
    public void AncestorAxis()
    {
        var result = EvaluateStrings("//title/ancestor::*", LoadDocument());
        Assert.Equal(6, result.Length); // library + book (×3 titles = 3 books + library for each?)
        // Actually: for each title, ancestors are book and library
        // title[1]: book[1], library
        // title[2]: book[2], library
        // title[3]: book[3], library
        // Total: 6
    }

    [Fact]
    public void AncestorOrSelfAxis()
    {
        var result = EvaluateStrings("//title/ancestor-or-self::*", LoadDocument());
        Assert.Equal(9, result.Length); // 6 ancestors + 3 self
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
}
