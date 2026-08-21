// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 21 augustus 2026
// PURPOSE              : Verifies schema-aware XSD list and union simple-type casts and instance-of tests.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 21-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 21-08-2026     | Added regression test for restriction-of-union lowercaseName instance-of                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 21-08-2026     | Added regression tests for schema-aware function coercion and QName-to-union casts         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 21-08-2026     | Added regression tests for namespaced QName castable-to-union and function-lookup coercion |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 21-08-2026     | Added braced-URI-literal instance-of regression for CastAs-UnionType-28/29/30             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 21-08-2026     | Added regression tests for namespace-context dynamic constructor calls and XPST0051 on restriction-of-union |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

public class SchemaListUnionTests
{
    [Fact]
    public void UnionCast_SelectsIntegerMemberAndTruncatesDecimal()
    {
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile("123.12 cast as s:myUnionType1").Evaluate(ctx);
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(123L, result.IntegerValue);
    }

    [Fact]
    public void UnionCast_SelectsDateMember()
    {
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile("xs:date('2000-01-01') cast as s:myUnionType1").Evaluate(ctx);
        Assert.Equal(XdmValueKind.Date, result.Kind);
        Assert.Equal("2000-01-01", result.ToString());
    }

    [Fact]
    public void UnionCast_SelectsPatternRestrictedStringMember()
    {
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile("'IB123' cast as s:myUnionType2").Evaluate(ctx);
        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("IB123", result.StringValue);
    }

    [Fact]
    public void UnionCast_FailsForNonMatchingValue()
    {
        var ctx = LoadUnionListContext();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            XPath31Expression.Compile("'no-match' cast as s:myUnionType1").Evaluate(ctx));
        Assert.Contains("FORG0001", ex.Message);
    }

    [Fact]
    public void ListCast_TokenizesStringToIntegerSequence()
    {
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile("'1 2 3' cast as s:intListType1").Evaluate(ctx);
        Assert.True(result.IsSequence);
        var items = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item);
        Assert.Equal(3, items.Count);
        Assert.Equal(XdmValueKind.Integer, items[0].Kind);
        Assert.Equal(1L, items[0].IntegerValue);
        Assert.Equal(2L, items[1].IntegerValue);
        Assert.Equal(3L, items[2].IntegerValue);
    }

    [Fact]
    public void ListOfUnionsCast_TokenizesAndSelectsMemberType()
    {
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile("'1.0 2.0' cast as s:impureUnionType").Evaluate(ctx);
        Assert.True(result.IsSequence);
        var items = new List<XdmValue>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            items.Add(item);
        Assert.Equal(2, items.Count);
        Assert.Equal(XdmValueKind.Decimal, items[0].Kind);
        Assert.Equal(1.0m, items[0].DecimalValue);
        Assert.Equal(2.0m, items[1].DecimalValue);
    }

    [Fact]
    public void UnionInstanceOf_AcceptsMatchingDecimalValue()
    {
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile("123.12 instance of s:myUnionType1").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void UnionCast_QNameMemberFromString()
    {
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile("namespace-uri-from-QName(\"xs:integer\" cast as s:sensitiveUnion)").Evaluate(ctx);
        Assert.Equal("http://www.w3.org/2001/XMLSchema", result.ToString());
    }

    [Fact]
    public void UnionInstanceOf_AcceptsMatchingStringValue()
    {
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile("'IB123' instance of s:myUnionType2").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void ListInstanceOf_AcceptsMatchingStringValue()
    {
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile("'1 2 3' instance of s:intListType1").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void SchemaValidatedUnionElement_TypedValueUsesSelectedMemberType()
    {
        var schemaSet = LoadUnionListSchema();
        var doc = XDocument.Parse(
            "<elementContainingApproximateDate xmlns='http://www.w3.org/XQueryTest/unionListDefined'>" +
            "<e>2000-01-01</e>" +
            "</elementContainingApproximateDate>");
        doc.Validate(schemaSet, null, true);
        var root = XdmValue.FromNode(new XDocumentNode(doc.Root!));
        var eNode = new XdmValue();
        bool found = false;
        foreach (var child in root.NodeValue.Axis(XdmAxis.Child))
        {
            if (child.NodeValue?.LocalName == "e")
            {
                eNode = child;
                found = true;
                break;
            }
        }
        Assert.True(found);

        var typed = eNode.NodeValue.TypedValue;
        Assert.Equal(XdmValueKind.Date, typed.Kind);
        Assert.Equal("2000-01-01", typed.ToString());
    }

    [Fact]
    public void LowercaseNameInstanceOfSensitiveUnion()
    {
        // Regression for CastAs-UnionType-16:
        // s:lowercaseName is a restriction of s:sensitiveUnion, which is a union
        // of xs:NCName and xs:QName. The value must match the union type.
        var ctx = LoadUnionListContext();
        ctx = ctx.WithNamespace("xs", "http://www.w3.org/2001/XMLSchema");

        var result = XPath31Expression.Compile("s:lowercaseName('xs:integer') instance of s:sensitiveUnion").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    private static EvaluationContext LoadUnionListContext()
    {
        var schemaSet = LoadUnionListSchema();
        var ctx = new EvaluationContext();
        ctx.SchemaSet = schemaSet;
        ctx = ctx.WithNamespace("s", "http://www.w3.org/XQueryTest/unionListDefined");
        FunctionLibrary.Populate(ctx);
        return ctx;
    }

    [Fact]
    public void QNameCastToUnion_PreservesQNameKind()
    {
        // Regression for CastAs-UnionType-20: a QName value cast to a union containing
        // xs:QName must stay a QName, not collapse to the xs:NCName member.
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile("local-name-from-QName(s:sensitiveUnion(xs:QName(\"a\")))").Evaluate(ctx);
        Assert.Equal("a", result.ToString());
    }

    [Fact]
    public void QNameCastableToUnion_WithNamespace()
    {
        // Regression for CastAs-UnionType-25: a QName value (with namespace) is castable
        // as a union type that includes xs:QName as a member.
        var ctx = LoadUnionListContext();
        var doc = XDocument.Parse("<p:a xmlns:p='http://www.example.com'/>");
        ctx = ctx.WithVariable("e", XdmValue.FromNode(new XDocumentNode(doc)));

        var result = XPath31Expression.Compile(
            "node-name($e/*) castable as s:sensitiveUnion").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void UnionFunctionCoercion_ConstructorMatchesTypedFunctionItem()
    {
        // Regression for CastAs-UnionType-18/26: a user-defined union constructor is
        // coercible to a typed function item type.
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile(
            "s:sensitiveUnion#1 instance of function(xs:anyAtomicType?) as s:sensitiveUnion?").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void ListFunctionCoercion_UserDefinedListConstructorMatchesTypedFunctionItem()
    {
        // Regression for CastAs-ListType-26: a user-defined list constructor is coercible
        // to a function returning the item type sequence.
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile(
            "s:myRestrictedList1#1 instance of function(xs:string) as xs:integer*").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void ListFunctionCoercion_UserDefinedListConstructorViaLookupMatchesTypedFunctionItem()
    {
        // Regression for CastAs-ListType-26 via function-lookup: a looked-up user-defined
        // list constructor must match the same typed function item type.
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile(
            "function-lookup(QName('http://www.w3.org/XQueryTest/unionListDefined', 'myRestrictedList1'), 1) " +
            "instance of function(xs:string) as xs:integer*").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void ListFunctionCoercion_BuiltInListConstructorMatchesTypedFunctionItem()
    {
        // Regression for CastAs-ListType-27: xs:NMTOKENS#1 is coercible to a function
        // returning xs:NMTOKEN*.
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile(
            "xs:NMTOKENS#1 instance of function(xs:anyAtomicType) as xs:NMTOKEN*").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void UnionOfListsCast_InstanceOfSensitiveListItemType()
    {
        // Regression for CastAs-UnionType-28/29/30: casting to a union of lists and then
        // checking instance of the selected list's item type must succeed.
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile(
            "(\"a b xs:integer\" cast as s:unionOfLists) instance of s:sensitiveUnion*").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void UnionOfListsCast_InstanceOfSensitiveListItemTypeByBracedUriLiteral()
    {
        // Regression for CastAs-UnionType-28/29/30 (XQuery path): the parser must preserve
        // the namespace URI from the Q{uri}local EQName form in an instance-of sequence type.
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile(
            "(\"a b xs:integer\" cast as s:unionOfLists) instance of Q{http://www.w3.org/XQueryTest/unionListDefined}sensitiveUnion*").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void UnionOfListsFunctionCoercion_MatchesAnyAtomicTypeStarReturn()
    {
        // Regression for CastAs-UnionType-32: the unionOfLists constructor (returning a
        // union of list types) is coercible to function(xs:string?) as xs:anyAtomicType*.
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile(
            "s:unionOfLists#1 instance of function(xs:string?) as xs:anyAtomicType*").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void UnionOfListsFunctionCoercionViaLookup_MatchesAnyAtomicTypeStarReturn()
    {
        // Regression for CastAs-UnionType-32 via function-lookup: a looked-up unionOfLists
        // constructor must match the same typed function item type.
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile(
            "function-lookup(QName('http://www.w3.org/XQueryTest/unionListDefined', 'unionOfLists'), 1) " +
            "instance of function(xs:string?) as xs:anyAtomicType*").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void LowercaseNameInstanceOfItself_ThrowsXpst0051()
    {
        // Regression for CastAs-UnionType-17: a restriction of a union type is not
        // allowed as an item type in a SequenceType.
        var ctx = LoadUnionListContext();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            XPath31Expression.Compile("s:lowercaseName('xs:integer') instance of s:lowercaseName").Evaluate(ctx));
        Assert.Contains("XPST0051", ex.Message);
    }

    [Fact]
    public void SensitiveUnionConstructorDynamicCall_UsesDefiningNamespaceContext()
    {
        // Regression for CastAs-UnionType-13/14/15: a constructor function item for a
        // namespace-sensitive union type must resolve lexical prefixes using the namespace
        // bindings that were in scope where the function item was created, not the call site.
        var definingContext = LoadUnionListContext();

        var funcValue = XPath31Expression.Compile(
            "function-lookup(QName('http://www.w3.org/XQueryTest/unionListDefined', 'sensitiveUnion'), 1)")
            .Evaluate(definingContext);
        Assert.True(funcValue.IsFunction);
        var funcItem = (FunctionItem)funcValue.FunctionValue;

        // Call site binds 'pre' to a namespace, but the defining context does not.
        var callSiteContext = LoadUnionListContext()
            .WithNamespace("pre", "http://example.com/ns");

        var args = new[] { XdmValue.FromString("pre:local") };
        var ex = Assert.Throws<InvalidOperationException>(() =>
            VmEngine.InvokeFunctionItem(funcItem, callSiteContext, args));
        Assert.Contains("FORG0001", ex.Message);
    }

    private static XmlSchemaSet LoadUnionListSchema()
    {
        var schemaSet = new XmlSchemaSet();
        string schemaPath = Path.GetFullPath("../../../../qt3tests/prod/SchemaImport/unionListDefined.xsd");
        using var stream = File.OpenRead(schemaPath);
        using var reader = XmlReader.Create(stream);
        schemaSet.Add(XmlSchema.Read(reader, null)!);
        schemaSet.Compile();
        return schemaSet;
    }
}
