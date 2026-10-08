// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 08 oktober 2026
// PURPOSE              : Unit tests for REQ-118 4.0-S6b structural record types and the 'but with' operator.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 08-10-2026     | Creation: record type syntax, instance-of, cast, coercion, record lookup checks, 'but with'|
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Api.Tests;

public class RecordTypeTests
{
    private static XdmValue Eval40(string xpath)
    {
        var expr = XPath31Expression.Compile(xpath, new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        return expr.Evaluate(new EvaluationContext());
    }

    // Evaluates in 4.0 mode and stringifies every item of the result sequence.
    private static List<string> Seq40(string xpath)
    {
        var result = Eval40(xpath);
        var items = new List<string>();
        if (result.IsSequence && result.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                items.Add(item.ToString());
        }
        else if (!result.IsUndefined)
        {
            items.Add(result.ToString());
        }
        return items;
    }

    // A handy annotated record value: a map coerced to a record type via cast.
    private const string RecA = "(map{\"a\": 1} cast as record(a))";

    // ------------------------------------------------------------------
    // Record type syntax (XPath 4.0 §3.2.10)
    // ------------------------------------------------------------------

    [Fact]
    public void RecordType_DuplicateFieldNames_XPST0021()
    {
        var ex = Assert.Throws<Parser.XPathParseException>(() => Eval40("1 instance of record(a, a)"));
        Assert.Contains("XPST0021", ex.Message);
    }

    [Fact]
    public void RecordType_MalformedFieldList_XPST0003()
    {
        var ex = Assert.Throws<Parser.XPathParseException>(() => Eval40("1 instance of record(1a)"));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void RecordType_TrailingComma_IsAccepted()
    {
        Assert.Equal("true", Eval40($"{RecA} instance of record(a,)").ToString());
    }

    [Fact]
    public void RecordType_QuotedFieldNames_AreAccepted()
    {
        Assert.Equal("true", Eval40("(map{\"a b\": 1} cast as record(\"a b\"))?\"a b\" instance of xs:integer").ToString());
    }

    // ------------------------------------------------------------------
    // Instance of (§4.15.1, structural matching per §3.2.10)
    // ------------------------------------------------------------------

    [Fact]
    public void InstanceOf_AnnotatedRecord_MatchesOwnType()
    {
        Assert.Equal("true", Eval40($"{RecA} instance of record(a)").ToString());
    }

    [Fact]
    public void InstanceOf_PlainMap_NeverMatchesRecordType()
    {
        // Records are maps with a record annotation; map constructors produce plain maps.
        Assert.Equal("false", Eval40("map{\"a\": 1} instance of record(a)").ToString());
        Assert.Equal("false", Eval40("map{\"a\": 1} instance of record(*)").ToString());
    }

    [Fact]
    public void InstanceOf_Record_MatchesRecordStar()
    {
        Assert.Equal("true", Eval40($"{RecA} instance of record(*)").ToString());
    }

    [Fact]
    public void InstanceOf_EmptyRecord_MatchesOnlyEmptyRecord()
    {
        Assert.Equal("true", Eval40("(map{} cast as record()) instance of record()").ToString());
        Assert.Equal("false", Eval40($"{RecA} instance of record()").ToString());
    }

    [Fact]
    public void InstanceOf_FieldCountMismatch_IsFalse()
    {
        Assert.Equal("false", Eval40($"{RecA} instance of record(a, b)").ToString());
        Assert.Equal("false", Eval40("(map{\"a\": 1, \"b\": 2} cast as record(a, b)) instance of record(a)").ToString());
    }

    [Fact]
    public void InstanceOf_FieldTypeMismatch_IsFalse()
    {
        Assert.Equal("false", Eval40($"{RecA} instance of record(a as xs:string)").ToString());
    }

    [Fact]
    public void InstanceOf_FieldTypeCovariance_IsStructural()
    {
        // record(a as xs:integer) is a subtype of record(a as xs:decimal): an integer
        // field value matches the xs:decimal field test.
        var recInt = "(map{\"a\": 3} cast as record(a as xs:integer))";
        Assert.Equal("true", Eval40($"{recInt} instance of record(a as xs:decimal)").ToString());
        var recDecimal = "(map{\"a\": 3.5} cast as record(a as xs:decimal))";
        Assert.Equal("false", Eval40($"{recDecimal} instance of record(a as xs:integer)").ToString());
    }

    [Fact]
    public void InstanceOf_OptionalField_AllowsEmptyEntryValue()
    {
        // A field whose entry holds the empty sequence still matches when the field
        // type allows empty.
        Assert.Equal("true", Eval40("(map{\"a\": 1} cast as record(a, b as item()?)) instance of record(a, b as item()?)").ToString());
        Assert.Equal("false", Eval40("(map{\"a\": 1} cast as record(a, b as item()?)) instance of record(a, b as xs:integer)").ToString());
    }

    // ------------------------------------------------------------------
    // Cast to a record type (§4.19.2.7)
    // ------------------------------------------------------------------

    [Fact]
    public void Cast_DiscardsSurplusKeys()
    {
        Assert.Equal("1", Eval40("map:size(map{\"a\": 1, \"b\": 2} cast as record(a))").ToString());
        Assert.Equal("false", Eval40("map:contains(map{\"a\": 1, \"b\": 2} cast as record(a), \"b\")").ToString());
    }

    [Fact]
    public void Cast_MissingFieldBecomesEmptyEntry()
    {
        // Spec example: surplus keys discarded, absent fields become ().
        var cast = "(map{\"first\": \"John\", \"middle\": \"Ignatius\", \"last\": \"Smith\"} cast as record(title, first, last))";
        Assert.Equal("3", Eval40($"map:size({cast})").ToString());
        Assert.Equal("John", Eval40($"{cast}?first").ToString());
        Assert.Empty(Seq40($"{cast}?title"));
        Assert.Equal("false", Eval40($"map:contains({cast}, \"middle\")").ToString());
    }

    [Fact]
    public void Cast_MissingNonEmptiableField_XPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("map{\"b\": 2} cast as record(a as xs:integer)"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Cast_CastsNonMatchingFieldValue()
    {
        // "12" does not match xs:integer but is castable to it.
        Assert.Equal("12", Eval40("(map{\"a\": \"12\"} cast as record(a as xs:integer))?a").ToString());
        Assert.Equal("true", Eval40("(map{\"a\": \"12\"} cast as record(a as xs:integer))?a instance of xs:integer").ToString());
    }

    [Fact]
    public void Cast_UncastableFieldValue_FORG0001()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("map{\"a\": \"xx\"} cast as record(a as xs:integer)"));
        Assert.Contains("FORG0001", ex.Message);
    }

    [Fact]
    public void Cast_RecordStar_IsAnAssertion()
    {
        // Already a record: passes through. A plain map is not a record: XPTY0004.
        Assert.Equal("1", Eval40($"({RecA} cast as record(*))?a").ToString());
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("map{\"a\": 1} cast as record(*)"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Castable_RecordTypes()
    {
        Assert.Equal("true", Eval40("map{\"a\": 1} castable as record(a)").ToString());
        Assert.Equal("false", Eval40("map{\"a\": 1} castable as record(*)").ToString());
        Assert.Equal("true", Eval40($"{RecA} castable as record(*)").ToString());
        Assert.Equal("false", Eval40("map{\"b\": 2} castable as record(a as xs:integer)").ToString());
    }

    // ------------------------------------------------------------------
    // Function conversion / coercion (§3.4.2 rule 10)
    // ------------------------------------------------------------------

    [Fact]
    public void Coercion_BuildsAnnotatedRecord()
    {
        Assert.Equal("3", Eval40("function($r as record(a as xs:integer, b as xs:string)) { $r?a + 1 }(map{\"a\": 2, \"b\": \"x\"})").ToString());
        Assert.Equal("true", Eval40("function($r as record(a)) { $r }(map{\"a\": 2}) instance of record(a)").ToString());
    }

    [Fact]
    public void Coercion_MissingFieldBecomesEmptyEntry()
    {
        Assert.Equal("2", Eval40("function($r as record(a, b)) { map:size($r) }(map{\"a\": 1})").ToString());
        Assert.Empty(Seq40("function($r as record(a, b)) { $r?b }(map{\"a\": 1})"));
    }

    [Fact]
    public void Coercion_SurplusKeys_XPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Eval40("function($r as record(a)) { $r }(map{\"a\": 1, \"b\": 2})"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Coercion_MissingNonEmptiableField_XPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Eval40("function($r as record(a as xs:integer)) { $r }(map{\"b\": 2})"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Coercion_PlainMapToRecordStar_XPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Eval40("function($r as record(*)) { $r }(map{\"a\": 1})"));
        Assert.Contains("XPTY0004", ex.Message);
        // An annotated record passes record(*) unchanged.
        Assert.Equal("1", Eval40($"function($r as record(*)) {{ $r?a }}({RecA})").ToString());
    }

    [Fact]
    public void Coercion_PromotesFieldValuesRecursively()
    {
        // Numeric promotion is part of function conversion: the stored value is a double.
        Assert.Equal("true",
            Eval40("function($r as record(a as xs:double)) { $r?a instance of xs:double }(map{\"a\": 2})").ToString());
    }

    [Fact]
    public void Coercion_EntriesAreInDeclarationOrder()
    {
        Assert.Equal(new List<string> { "b", "a" },
            Seq40("map:keys(function($r as record(b, a)) { $r }(map{\"a\": 1, \"b\": 2}))"));
    }

    [Fact]
    public void Coercion_NonMapValue_XPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Eval40("function($r as record(a)) { $r }(42)"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    // ------------------------------------------------------------------
    // Lookup on records (§4.15.3)
    // ------------------------------------------------------------------

    [Fact]
    public void Lookup_DeclaredField_Works()
    {
        Assert.Equal("1", Eval40($"{RecA}?a").ToString());
        Assert.Equal("1", Eval40($"{RecA}(\"a\")").ToString());
    }

    [Fact]
    public void Lookup_UnknownFieldOnRecord_XPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40($"{RecA}?b"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Lookup_RecordAsFunctionUnknownKey_XPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40($"{RecA}(\"b\")"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void Lookup_MultiKeyFormChecksEachKey()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40($"{RecA}?(\"a\", \"b\")"));
        Assert.Contains("XPTY0004", ex.Message);
        // Each key contributes its lookup result (Lookup-107 ordering).
        Assert.Equal(new List<string> { "1", "1" }, Seq40($"{RecA}?(\"a\", \"a\")"));
    }

    [Fact]
    public void Lookup_Wildcard_IsUnaffected()
    {
        Assert.Equal(new List<string> { "1" }, Seq40($"{RecA}?*"));
    }

    [Fact]
    public void Lookup_PlainMapUnknownKey_StillReturnsEmpty()
    {
        Assert.Empty(Seq40("map{\"a\": 1}?b"));
    }

    [Fact]
    public void MapGet_DoesNotFieldCheck()
    {
        // Only the lookup operator and record-as-function calls field-check (§4.15.3);
        // the map:* functions operate on the underlying map.
        Assert.Empty(Seq40($"map:get({RecA}, \"b\")"));
        Assert.Equal("1", Eval40($"map:get({RecA}, \"a\")").ToString());
    }

    // ------------------------------------------------------------------
    // 'but with' (§4.15.4)
    // ------------------------------------------------------------------

    [Fact]
    public void ButWith_ReplacesEntryValues()
    {
        Assert.Equal("2", Eval40($"({RecA} but with map{{\"a\": 2}})?a").ToString());
    }

    [Fact]
    public void ButWith_ResultKeepsRecordAnnotation()
    {
        Assert.Equal("true", Eval40($"({RecA} but with map{{\"a\": 2}}) instance of record(a)").ToString());
        Assert.Equal("3", Eval40($"(({RecA} but with map{{\"a\": 2}}) but with map{{\"a\": 3}})?a").ToString());
    }

    [Fact]
    public void ButWith_UnknownField_XPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40($"{RecA} but with map{{\"b\": 2}}"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void ButWith_PlainMapLhs_XPTY0004()
    {
        // A plain map has no record annotation to name R.
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40("map{\"a\": 1} but with map{\"a\": 2}"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void ButWith_NonMapRhs_XPTY0004()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40($"{RecA} but with 42"));
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void ButWith_CoercesRhsValuesToFieldTypes()
    {
        var recInt = "(map{\"a\": 1} cast as record(a as xs:integer))";
        // 2.5 is not coercible to xs:integer (no reverse numeric promotion).
        var ex = Assert.Throws<InvalidOperationException>(() => Eval40($"{recInt} but with map{{\"a\": 2.5}}"));
        Assert.Contains("XPTY0004", ex.Message);
        // An integer is fine.
        Assert.Equal("5", Eval40($"({recInt} but with map{{\"a\": 5}})?a").ToString());
    }

    [Fact]
    public void ButWith_ResultEntriesFollowDeclarationOrder()
    {
        var rec = "(map{\"a\": 1, \"b\": 2} cast as record(a, b))";
        Assert.Equal(new List<string> { "a", "b" }, Seq40($"map:keys({rec} but with map{{\"b\": 20}})"));
    }
}
