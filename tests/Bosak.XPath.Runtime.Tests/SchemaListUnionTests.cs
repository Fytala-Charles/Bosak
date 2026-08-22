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
//                      | Charles Korthout | 0.7   | 21-08-2026     | Added XPST0051 tests for list types and union types containing list members as SequenceType item types |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 21-08-2026     | Added regression tests for casting schema-validated xs:decimal nodes to atomic values    |
//                      | Charles Korthout | 0.9   | 22-08-2026     | Added regression tests for derived string/numeric/union casts and date serialization   |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.10  | 22-08-2026     | Added regression tests for union with named @memberTypes members                        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.11  | 22-08-2026     | Added regression tests for schema-aware attribute kind tests and union function conversion |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.12  | 22-08-2026     | Added regression tests for XPTY0117 on namespace-sensitive atomic function conversion     |
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
    public void ListInstanceOf_RejectsListTypeAsSequenceTypeItemType()
    {
        // A list type is not a valid item type in a SequenceType (XPST0051).
        var ctx = LoadUnionListContext();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            XPath31Expression.Compile("'1 2 3' instance of s:intListType1").Evaluate(ctx));
        Assert.Contains("XPST0051", ex.Message);
    }

    [Fact]
    public void UnionContainingListInstanceOf_ThrowsXpst0051()
    {
        // A union type that contains a list type member is not a valid SequenceType item type.
        var ctx = LoadUnionListContext();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            XPath31Expression.Compile("1 instance of s:impureUnionType").Evaluate(ctx));
        Assert.Contains("XPST0051", ex.Message);
    }

    [Fact]
    public void UnionContainingBuiltInListInstanceOf_ThrowsXpst0051()
    {
        // A union type that transitively contains a built-in list type member is disallowed.
        var ctx = LoadUnionListContext();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            XPath31Expression.Compile("85 instance of s:unionOfListsAndAtomic").Evaluate(ctx));
        Assert.Contains("XPST0051", ex.Message);
    }

    [Fact]
    public void UnionOfAtomicInstanceOf_BracedUriLiteralAcceptsMatchingValue()
    {
        // Positive regression: a braced-URI-literal union of atomic types is a valid item type.
        var ctx = LoadUnionListContext();

        var result = XPath31Expression.Compile(
            "123.12 instance of Q{http://www.w3.org/XQueryTest/unionListDefined}myUnionType1")
            .Evaluate(ctx);
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

    [Fact]
    public void Cast_TypedDecimalElement_ReturnsDecimalAtomicValue()
    {
        // Regression for prod-OrderByClause orderBy26/36/46/56/62/64/65:
        // xs:decimal($x) on a schema-validated xs:decimal element must atomize
        // and return the decimal atomic value, not the original element node.
        var ctx = LoadOrderDataContext();

        var result = XPath31Expression
            .Compile("xs:decimal((/DataValues/NegativeNumbers/orderData)[1])")
            .Evaluate(ctx);

        Assert.Equal(XdmValueKind.Decimal, result.Kind);
        Assert.Equal("-100000000000000000", result.ToString());
    }

    [Fact]
    public void Cast_TypedDecimalElementInForExpression_ReturnsDecimalSequence()
    {
        // Regression for prod-OrderByClause decimal normalization:
        // when xs:decimal($x) is evaluated for every item in a for-expression,
        // the result sequence must contain decimal atomics, not element nodes.
        var ctx = LoadOrderDataContext();

        var result = XPath31Expression
            .Compile("string-join(for $x in /DataValues/NegativeNumbers/orderData return xs:decimal($x), ',')")
            .Evaluate(ctx);

        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.StartsWith("-100000000000000000,-10000000000000000", result.ToString());
        Assert.EndsWith("-1,0", result.ToString());
    }

    private static EvaluationContext LoadOrderDataContext()
    {
        var schemaSet = new XmlSchemaSet();
        string schemaPath = Path.GetFullPath("../../../../qt3tests/prod/OrderByClause/orderData.xsd");
        using (var stream = File.OpenRead(schemaPath))
        using (var reader = XmlReader.Create(stream))
            schemaSet.Add(XmlSchema.Read(reader, null)!);
        schemaSet.Compile();

        string docPath = Path.GetFullPath("../../../../qt3tests/prod/OrderByClause/orderData.xml");
        var docNode = XDocumentProvider.LoadXml(docPath, null, schemaSet);

        var ctx = new EvaluationContext();
        ctx.SchemaSet = schemaSet;
        ctx = ctx.WithNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        ctx.DefaultElementNamespace = "http://www.w3.org/XQueryTestOrderBy";
        ctx = ctx.WithFocus(XdmValue.FromNode(docNode), 1, 1);
        FunctionLibrary.Populate(ctx);
        return ctx;
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

    private static EvaluationContext LoadDerivedContext()
    {
        var schemaSet = new XmlSchemaSet();
        string schemaPath = Path.GetFullPath("../../../../qt3tests/prod/CastExpr/derived.xsd");
        using var stream = File.OpenRead(schemaPath);
        using var reader = XmlReader.Create(stream);
        schemaSet.Add(XmlSchema.Read(reader, null)!);
        schemaSet.Compile();

        var ctx = new EvaluationContext();
        ctx.SchemaSet = schemaSet;
        ctx = ctx.WithNamespace("d", "http://www.w3.org/XQueryTest/derivedTypes");
        ctx = ctx.WithNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        FunctionLibrary.Populate(ctx);
        return ctx;
    }

    [Fact]
    public void Cast_IntegerToNormalizedString_Succeeds()
    {
        // Regression for cbcl-normalizedstring-003: xs:normalizedString(5) must succeed.
        var result = XPath31Expression.Compile("xs:normalizedString(5)").Evaluate(new EvaluationContext());
        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("5", result.StringValue);
        Assert.Equal("normalizedString", result.SchemaTypeName);
    }

    [Fact]
    public void Cast_IntegerToToken_Succeeds()
    {
        // Regression for cbcl-token-003: xs:token(5) must succeed.
        var result = XPath31Expression.Compile("xs:token(5)").Evaluate(new EvaluationContext());
        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("5", result.StringValue);
        Assert.Equal("token", result.SchemaTypeName);
    }

    [Fact]
    public void Castable_IntegerToNormalizedString_IsTrue()
    {
        // Regression for cbcl-normalizedstring-005: 5 castable as xs:normalizedString.
        var result = XPath31Expression.Compile("5 castable as xs:normalizedString").Evaluate(new EvaluationContext());
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Castable_IntegerToToken_IsTrue()
    {
        // Regression for cbcl-token-005: 5 castable as xs:token.
        var result = XPath31Expression.Compile("5 castable as xs:token").Evaluate(new EvaluationContext());
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Castable_IntegerToCanonicalDecimal_IsTrue()
    {
        // Regression for CastableAs653: pattern facets are checked against XSD canonical
        // lexical representation, so 12 is castable as d:canonicalDecimal.
        var ctx = LoadDerivedContext();
        var result = XPath31Expression.Compile("12 castable as d:canonicalDecimal").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Castable_DoubleToCanonicalDouble_IsTrue()
    {
        // Regression for CastableAs655: 93.7 is castable as d:canonicalDouble because the
        // canonical lexical form uses scientific notation.
        var ctx = LoadDerivedContext();
        var result = XPath31Expression.Compile("93.7 castable as d:canonicalDouble").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Castable_ZeroDoubleToCanonicalDouble_IsTrue()
    {
        // Regression for CastableAs657: 0.0e0 is castable as d:canonicalDouble.
        var ctx = LoadDerivedContext();
        var result = XPath31Expression.Compile("0.0e0 castable as d:canonicalDouble").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void Castable_DecimalToImpureUnionType_IsFalse()
    {
        // Regression for cbcl-castable-impure-009: a single xs:decimal cannot be cast to a
        // union whose list member requires a string lexical form.
        var ctx = LoadUnionListContext();
        var result = XPath31Expression.Compile("xs:decimal(\"1\") castable as s:impureUnionType").Evaluate(ctx);
        Assert.False(result.BooleanValue);
    }

    [Fact]
    public void Cast_DerivedDateTimeTypes_PreserveLexicalForm()
    {
        // Regression for cbcl-cast-derived-001: casts to derived gYear/gMonth/gDay/etc.
        // types must serialize in their specific lexical form, not as full xs:dateTime.
        var ctx = LoadDerivedContext();
        var result = XPath31Expression.Compile(
            "string-join((\"---01\" cast as d:gDay, \"--12-25\" cast as d:gMonthDay, " +
            "\"--12\" cast as d:gMonth, \"2004\" cast as d:gYear, \"2004-02\" cast as d:gYearMonth, " +
            "\"P1D\" cast as d:duration), ' ')")
            .Evaluate(ctx);
        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("---01 --12-25 --12 2004 2004-02 P1D", result.StringValue);
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

    private static EvaluationContext LoadUserDefinedTypesContext()
    {
        var schemaSet = new XmlSchemaSet();
        string schemaPath = Path.GetFullPath("../../../../qt3tests/docs/userdefined.xsd");
        using var stream = File.OpenRead(schemaPath);
        using var reader = XmlReader.Create(stream);
        schemaSet.Add(XmlSchema.Read(reader, null)!);
        schemaSet.Compile();

        var ctx = new EvaluationContext();
        ctx.SchemaSet = schemaSet;
        ctx = ctx.WithNamespace("t", "http://www.w3.org/XQueryTest/userDefinedTypes");
        ctx = ctx.WithNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        FunctionLibrary.Populate(ctx);
        return ctx;
    }

    [Fact]
    public void UnionCast_NamedMemberType_SelectsNamedMember()
    {
        // Regression for QT3 op-numeric-add-14/15: a union defined with both a named
        // @memberTypes member (xs:integer) and an inline member must consider the named member.
        var ctx = LoadUserDefinedTypesContext();

        var result = XPath31Expression.Compile("15 cast as t:integer-or-nothing").Evaluate(ctx);
        Assert.Equal(XdmValueKind.Integer, result.Kind);
        Assert.Equal(15L, result.IntegerValue);
    }

    [Fact]
    public void UnionCast_NamedMemberType_EmptyStringSelectsInlineMember()
    {
        var ctx = LoadUserDefinedTypesContext();

        var result = XPath31Expression.Compile("'' cast as t:integer-or-nothing").Evaluate(ctx);
        Assert.Equal(XdmValueKind.String, result.Kind);
        Assert.Equal("", result.StringValue);
    }

    [Fact]
    public void AttributeKindTest_CasePreservedUserDefinedType()
    {
        // Regression for schema-aware attribute kind tests:
        // attribute(*, t:stringBased) must preserve the mixed-case type name so the
        // typed value matches the user-defined enumeration simple type.
        var schemaSet = new XmlSchemaSet();
        string schemaXml = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
            "xmlns:t='urn:test' targetNamespace='urn:test' elementFormDefault='qualified'>" +
            "<xs:simpleType name='stringBased'>" +
            "<xs:restriction base='xs:string'>" +
            "<xs:enumeration value='valid value 1'/>" +
            "</xs:restriction>" +
            "</xs:simpleType>" +
            "<xs:element name='root'>" +
            "<xs:complexType>" +
            "<xs:attribute name='status' type='t:stringBased'/>" +
            "</xs:complexType>" +
            "</xs:element>" +
            "</xs:schema>";
        using (var reader = XmlReader.Create(new StringReader(schemaXml)))
            schemaSet.Add(XmlSchema.Read(reader, null)!);
        schemaSet.Compile();

        var doc = XDocument.Parse("<root status='valid value 1' xmlns='urn:test'/>");
        doc.Validate(schemaSet, null, true);
        var attrNode = new XDocumentNode(doc.Root!.Attribute("status")!);
        var attr = XdmValue.FromNode(attrNode);
        Assert.True(attr.IsNode);
        Assert.Equal(XdmNodeKind.Attribute, attr.NodeValue.NodeKind);
        Assert.Equal("valid value 1", attr.NodeValue.StringValue);
        var annotation = attr.NodeValue.SchemaTypeAnnotation;
        Assert.NotNull(annotation);
        Assert.Equal("urn:test", annotation!.Value.NamespaceUri);
        Assert.Equal("stringBased", annotation.Value.LocalName);

        var ctx = new EvaluationContext();
        ctx.SchemaSet = schemaSet;
        ctx = ctx.WithNamespace("t", "urn:test");

        var result = XPath31Expression.Compile(". instance of attribute(*, t:stringBased)").Evaluate(ctx.WithFocus(attr, 1, 1));
        Assert.True(result.BooleanValue);

        // Direct diagnostic: verify the runtime type-compatibility helper returns true.
        var compat = typeof(VmEngine).GetMethod("IsAttributeTypeCompatible",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(EvaluationContext), typeof(Bosak.XPath.Core.Xdm.IXdmNode) },
            null);
        Assert.NotNull(compat);
        bool compatResult = (bool)compat!.Invoke(null, new object[] { "t:stringBased", ctx, attr.NodeValue })!;
        Assert.True(compatResult);
    }

    [Fact]
    public void UnionFunctionConversion_UntypedAtomicAcceptedByNonNamespaceSensitiveUnion()
    {
        // Regression for ApplyFunctionConversion union-type branch:
        // xs:untypedAtomic that matches a member of a non-namespace-sensitive union is accepted.
        var ctx = LoadUnionListContext();
        var value = XdmValue.FromString("123", "untypedAtomic");

        var result = VmEngine.ApplyFunctionConversion(value, "s:myUnionType1", ctx);
        Assert.True(VmEngine.ValueMatchesType(result, "s:myUnionType1", ctx));
    }

    [Fact]
    public void UnionFunctionConversion_NonMatchingDecimalIsRejected()
    {
        // Regression for ApplyFunctionConversion union-type branch:
        // a value that is not an instance of any member type is rejected with XPTY0004.
        var ctx = LoadUnionListContext();
        var value = XdmValue.FromDecimal(1.5m);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ApplyFunctionConversion(value, "s:sensitiveUnion", ctx));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void UnionFunctionConversion_NamespaceSensitiveUnionRejectsUntypedAtomic()
    {
        // Regression for ApplyFunctionConversion union-type branch:
        // xs:untypedAtomic cannot be cast to a namespace-sensitive union (xs:NCName + xs:QName).
        var ctx = LoadUnionListContext();
        var value = XdmValue.FromString("foo", "untypedAtomic");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ApplyFunctionConversion(value, "s:sensitiveUnion", ctx));
        Assert.Contains("XPTY0117", ex.Message);
    }

    [Fact]
    public void InstanceOf_UnprefixedUserDefinedTypeUsesDefaultElementNamespace()
    {
        // Regression for InstanceOf: unprefixed user-defined schema simple types in the
        // default element namespace are valid atomic item types (ForExprType052/053).
        var ctx = LoadUnionListContext();
        ctx.DefaultElementNamespace = "http://www.w3.org/XQueryTest/unionListDefined";

        var result = XPath31Expression.Compile("123 instance of myUnionType1").Evaluate(ctx);
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void IsSequenceTypeSubtype_ElementSchemaTypeSubtyping()
    {
        // Regression for IsSequenceTypeSubtype: element(*, T1) is a subtype of element(*, T2)
        // when T1 derives from T2 in the schema type hierarchy (FunctionCall-051).
        var ctx = LoadUnionListContext();
        var method = typeof(VmEngine).GetMethod("IsSequenceTypeSubtype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(string), typeof(EvaluationContext) },
            null);
        Assert.NotNull(method);

        bool actual = (bool)method!.Invoke(null, new object[]
        {
            "element(*, s:restrictedUnion)",
            "element(*, s:approximateDate)",
            ctx
        })!;

        Assert.True(actual);
    }

    [Fact]
    public void FunctionConversion_UntypedAtomicToQName_RaisesXpty0117()
    {
        // Regression for CastAsNamespaceSensitiveType-1/2 and CastAs675a:
        // function conversion must not implicitly cast xs:untypedAtomic to xs:QName.
        var ctx = new EvaluationContext().WithNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        var value = XdmValue.FromString("xs:integer", "untypedAtomic");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ApplyFunctionConversion(value, "xs:QName", ctx));
        Assert.Contains("XPTY0117", ex.Message);
    }

    [Fact]
    public void FunctionConversion_UntypedAtomicToNotation_RaisesXpty0117()
    {
        // Regression: xs:untypedAtomic cannot be implicitly cast to xs:NOTATION (also namespace-sensitive).
        var ctx = new EvaluationContext().WithNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        var value = XdmValue.FromString("value1", "untypedAtomic");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ApplyFunctionConversion(value, "xs:NOTATION", ctx));
        Assert.Contains("XPTY0117", ex.Message);
    }

    [Fact]
    public void FunctionConversion_UntypedAtomicToQNameDerivedRestriction_RaisesXpty0117()
    {
        // Regression: xs:untypedAtomic cannot be implicitly cast to a user-defined restriction of xs:QName.
        var ctx = LoadUserDefinedTypesContext();
        var value = XdmValue.FromString("value1", "untypedAtomic");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ApplyFunctionConversion(value, "t:QNameBased", ctx));
        Assert.Contains("XPTY0117", ex.Message);
    }

    [Fact]
    public void FunctionConversion_TypedQNameToQName_Succeeds()
    {
        // Sanity check: a typed xs:QName value still passes function conversion to xs:QName.
        var ctx = new EvaluationContext().WithNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        var value = XdmValue.FromQName(new XsQName("integer", "http://www.w3.org/2001/XMLSchema", "xs"));

        var result = VmEngine.ApplyFunctionConversion(value, "xs:QName", ctx);
        Assert.Equal(XdmValueKind.QName, result.Kind);
    }

    [Fact]
    public void FunctionConversion_ElementNodeToQNameParameter_RaisesXpty0117()
    {
        // Regression for CastAsNamespaceSensitiveType-2: an element node atomizes to xs:untypedAtomic,
        // and function conversion to xs:QName must raise XPTY0117.
        var ctx = new EvaluationContext().WithNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        var elementNode = new XDocumentNode(XDocument.Parse("<tag>xs:integer</tag>").Root!);
        var value = XdmValue.FromNode(elementNode);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            VmEngine.ApplyFunctionConversion(value, "xs:QName", ctx));
        Assert.Contains("XPTY0117", ex.Message);
    }
}
