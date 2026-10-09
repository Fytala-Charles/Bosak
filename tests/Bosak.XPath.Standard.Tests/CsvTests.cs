// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 oktober 2026
// PURPOSE              : Unit tests verifying correctness of the underlying implementation.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-10-2026     | Creation (REQ-123 parse-csv slice, F&O 4.0 §17.5): parse-csv/csv-to-xml/csv-doc +        |
//                      |                  |       |                | bare {…} map constructor + multi-clause FLWOR                                            |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Standard.Tests;

/// <summary>
/// Tests for fn:parse-csv, fn:csv-to-xml and fn:csv-doc (F&O 4.0 §17.5) and the two parser
/// additions the slice required: the bare {…} map constructor (PR2778) and consecutive
/// for/let clauses in XPath 4.0 FLWOR expressions. Semantics pinned against qt4tests
/// fn/parse-csv.xml, fn/csv-to-xml.xml and fn/csv-doc.xml.
/// </summary>
public class CsvTests
{
    private static XdmValue Eval40(string xpath)
    {
        var expr = XPath31Expression.Compile(xpath, new CompileOptions { Compatibility = XPathCompatibility.XPath40 });
        return expr.Evaluate(new EvaluationContext());
    }

    private static string[] Seq40(string xpath)
    {
        var result = Eval40(xpath);
        if (result.IsUndefined)
            return [];
        if (!result.IsSequence)
            return [result.ToString()!];
        var list = new List<string>();
        foreach (var item in XdmSequence.FromSource(result.SequenceValue!))
            list.Add(item.ToString());
        return list.ToArray();
    }

    private static InvalidOperationException Error40(string xpath)
        => Assert.Throws<InvalidOperationException>(() => Eval40(xpath));

    // ----- basic parsing --------------------------------------------------------

    [Fact]
    public void ParseCsv_BasicRowsAndFields()
    {
        Assert.Equal(["one", "two", "three", "four"],
            Seq40("fn:parse-csv('one,two' || char(10) || 'three,four')?rows?*"));
    }

    [Fact]
    public void ParseCsv_EmptyInput_NoRows()
    {
        Assert.Equal([], Seq40("fn:parse-csv('')?rows"));
        Assert.Equal(["0"], Seq40("fn:count(fn:parse-csv('')?rows)"));
    }

    [Fact]
    public void ParseCsv_EmptySequenceArgument_EmptySequence()
    {
        Assert.Equal([], Seq40("fn:parse-csv(())"));
        Assert.Equal([], Seq40("fn:csv-to-xml(())"));
        Assert.Equal([], Seq40("fn:csv-doc(())"));
    }

    [Fact]
    public void ParseCsv_SingleNewline_OneBlankRow()
    {
        Assert.Equal(["0"], Seq40("fn:parse-csv(char(10))?rows[1] => array:size()"));
    }

    [Fact]
    public void ParseCsv_FinalNewlineProducesNoExtraRow()
    {
        Assert.Equal(["2"], Seq40("fn:count(fn:parse-csv('a,b' || char(10) || 'c,d' || char(10))?rows)"));
    }

    [Theory]
    // parse-csv-033: blank unterminated tail does not exist; 034: newline-terminated does.
    [InlineData("char(10) || ' '", true, "1")]
    [InlineData("char(10) || char(10)", true, "2")]
    [InlineData("'a' || char(10) || ' '", true, "1")]
    // Without trim-whitespace a whitespace-only row is a real field.
    [InlineData("char(10) || ' '", false, "2")]
    public void ParseCsv_BlankRows_TrimWhitespaceRule(string input, bool trim, string expectedRows)
    {
        Assert.Equal([expectedRows],
            Seq40($"fn:count(fn:parse-csv({input}, {{'trim-whitespace': {trim.ToString().ToLowerInvariant()}()}})?rows)"));
    }

    [Fact]
    public void ParseCsv_CrAndCrlfNormalizedToLf()
    {
        Assert.Equal(["a", "b", "c", "d"],
            Seq40("fn:parse-csv('a,b' || char(13) || char(10) || 'c,d' || char(13))?rows?*"));
    }

    [Fact]
    public void ParseCsv_QuotedFieldWithEscapedQuotes()
    {
        // parse-csv-010 shape.
        Assert.Equal(["three,\"four\""],
            Seq40("fn:parse-csv('one,two' || char(10) || '\"three,\"\"four\"\"\",five')?rows[2]?1"));
    }

    [Fact]
    public void ParseCsv_NewlineInsideQuotedField()
    {
        // parse-csv-015 shape.
        Assert.Equal(["[" + "\n" + "]"],
            Seq40("fn:parse-csv('one,\"[' || char(10) || ']\"' || char(10) || '\"\",\"four\"')?rows[1]?2"));
    }

    [Fact]
    public void ParseCsv_QuoteInUnquotedField_IsLiteral()
    {
        // PR2962: quotes in unquoted fields are accepted as literal characters.
        Assert.Equal(["two\"three"],
            Seq40("fn:parse-csv('one,two\"three')?rows[1]?2"));
    }

    [Fact]
    public void ParseCsv_CharacterAfterClosingQuote_FOCV0001()
    {
        var ex = Error40("fn:parse-csv('\"a\"x,b')");
        Assert.Contains("FOCV0001", ex.Message);
    }

    [Fact]
    public void ParseCsv_UnterminatedQuotedField_FOCV0001()
    {
        var ex = Error40("fn:parse-csv('a,\"b')");
        Assert.Contains("FOCV0001", ex.Message);
    }

    [Fact]
    public void ParseCsv_QuoteNotFollowedBySeparator_FOCV0001()
    {
        // parse-csv-940 shape: closing quote must be followed by separator or newline.
        var ex = Error40("fn:parse-csv(\"a;'b;c'd\", { 'separator': ';', 'quote-character': \"'\" })");
        Assert.Contains("FOCV0001", ex.Message);
    }

    // ----- comment rows (PR2778) --------------------------------------------------

    [Fact]
    public void ParseCsv_CommentRowsDiscarded()
    {
        Assert.Equal(["one", "two", "z"],
            Seq40("fn:parse-csv('# this is a comment' || char(10) || 'one,two,\"z\"' || char(10), {'comment-marker': '#'})?rows?*"));
    }

    [Fact]
    public void ParseCsv_SingleCommentOnly_NoRows()
    {
        Assert.Equal([], Seq40("fn:parse-csv('# this is a comment', {'comment-marker': '#'})?rows"));
    }

    [Fact]
    public void ParseCsv_CommentMarkerNotFirstChar_TreatedAsData()
    {
        Assert.Equal(["a#b", "c"],
            Seq40("fn:parse-csv('a#b,c', {'comment-marker': '#'})?rows[1]?*"));
    }

    // ----- delimiters and trim-whitespace ------------------------------------------

    [Fact]
    public void ParseCsv_CustomSeparatorAndQuote()
    {
        Assert.Equal(["b;c"],
            Seq40("fn:parse-csv(\"a;'b;c'\", { 'separator': ';', 'quote-character': \"'\" })?rows[1]?2"));
    }

    [Fact]
    public void ParseCsv_TrimWhitespace_UnquotedFieldsOnly()
    {
        Assert.Equal(["a", "b c", " d "],
            Seq40("fn:parse-csv(' a , b c ,\" d \"', {'trim-whitespace': true()})?rows[1]?*"));
    }

    [Fact]
    public void ParseCsv_NoTrimWhitespace_FieldsIntact()
    {
        Assert.Equal([" a ", " b"],
            Seq40("fn:parse-csv(' a , b')?rows[1]?*"));
    }

    // ----- header handling -----------------------------------------------------------

    [Fact]
    public void ParseCsv_HeaderFalse_EmptyColumnsAndIndex()
    {
        Assert.Equal(["0", "0", "true"],
            Seq40("let $r := fn:parse-csv('a,b') return (count($r?columns), map:size($r?column-index), map:size($r) = 4)"));
    }

    [Fact]
    public void ParseCsv_HeaderTrue_FirstRowBecomesNames()
    {
        Assert.Equal(["a", "b", "2", "1", "one"],
            Seq40("let $r := fn:parse-csv('a,b' || char(10) || 'one,two', {'header': true()}) " +
                  "return ($r?columns, map:size($r?column-index), fn:count($r?rows), $r?get(1, 'a'))"));
    }

    [Fact]
    public void ParseCsv_HeaderNamesAlwaysTrimmed()
    {
        // Quoted or not, header names are trimmed regardless of trim-whitespace.
        Assert.Equal(["a", "b"],
            Seq40("fn:parse-csv('  a  ,\" b \"' || char(10) || '1,2', {'header': true()})?columns"));
    }

    [Fact]
    public void ParseCsv_HeaderStringSequence_SuppliedNames()
    {
        Assert.Equal(["first", "second", "2", "two"],
            Seq40("let $r := fn:parse-csv('one,two', {'header': ('first', 'second')}) " +
                  "return ($r?columns, $r?column-index('second'), $r?get(1, 'second'))"));
    }

    [Fact]
    public void ParseCsv_ColumnIndexSkipsEmptyAndDuplicateNames()
    {
        Assert.Equal(["1", "3"],
            Seq40("let $r := fn:parse-csv('a,,b,a' || char(10) || '1,2,3,4', {'header': true()}) " +
                  "return ($r?column-index('a'), $r?column-index('b'))"));
    }

    // ----- select-columns and trim-rows ----------------------------------------------

    [Fact]
    public void ParseCsv_SelectColumns_ReordersDuplicatesAndPads()
    {
        Assert.Equal(["4", "1", "4", ""],
            Seq40("fn:parse-csv('a,b,c,d' || char(10) || '1,2,3,4', " +
                  "{'header': true(), 'select-columns': (4, 1, 4, 9)})?rows[1]?*"));
    }

    [Fact]
    public void ParseCsv_SelectColumns_AdjustsNamesAndIndex()
    {
        Assert.Equal(["d", "a", "2"],
            Seq40("let $r := fn:parse-csv('a,b,c,d' || char(10) || '1,2,3,4', " +
                  "{'header': true(), 'select-columns': (4, 1)}) " +
                  "return ($r?columns, $r?column-index('a'))"));
    }

    [Fact]
    public void ParseCsv_SelectColumns_NotAppliedToSuppliedNames()
    {
        Assert.Equal(["x", "y"],
            Seq40("fn:parse-csv('1,2,3,4', {'header': ('x', 'y'), 'select-columns': (4, 1)})?columns"));
    }

    [Fact]
    public void ParseCsv_TrimRows_WidthFromHeaderRow()
    {
        // parse-csv-063 shape: the header row fixes the width of every data row.
        Assert.Equal(["1", "2", "3", "4", "11", "12", "13", "14"],
            Seq40("fn:parse-csv('a,b,c,d' || char(10) || '1,2,3,4,5,6,7' || char(10) || '11,12,13,14,15', " +
                  "{'trim-rows': true(), 'header': true()})?rows?*"));
    }

    [Fact]
    public void ParseCsv_TrimRows_PadsShortRows()
    {
        Assert.Equal(["14", "15", "16", "", "", ""],
            Seq40("fn:parse-csv('1,2,3,4,5,6' || char(10) || '14,15,16', {'trim-rows': true()})?rows[2]?*"));
    }

    [Fact]
    public void ParseCsv_SelectColumns_TakesPrecedenceOverTrimRows()
    {
        Assert.Equal(["5", "4"],
            Seq40("fn:parse-csv('1,2,3' || char(10) || '4,5', " +
                  "{'trim-rows': true(), 'select-columns': (2, 1)})?rows[2]?*"));
    }

    // ----- get function ----------------------------------------------------------------

    [Fact]
    public void Get_ByPositionAndName()
    {
        Assert.Equal(["two", "two"],
            Seq40("let $r := fn:parse-csv('a,b' || char(10) || 'one,two', {'header': true()}) " +
                  "return ($r?get(1, 2), $r?get(1, 'b'))"));
    }

    [Fact]
    public void Get_PositiveOutOfRange_ReturnsEmptyString()
    {
        Assert.Equal(["", ""],
            Seq40("let $r := fn:parse-csv('a,b' || char(10) || 'one,two', {'header': true()}) " +
                  "return ($r?get(99, 1), $r?get(1, 99))"));
    }

    [Fact]
    public void Get_UnknownColumnName_FOCV0004()
    {
        var ex = Error40("fn:parse-csv('a,b')?get(1, 'zzz')");
        Assert.Contains("FOCV0004", ex.Message);
    }

    [Theory]
    [InlineData("-1", "1")]  // negative row
    [InlineData("0", "1")]   // zero row
    [InlineData("1", "-1")]  // negative column
    [InlineData("1", "0")]   // zero column
    public void Get_NonPositiveRowOrColumn_FORG0001(string row, string col)
    {
        var ex = Error40($"fn:parse-csv('a,b' || char(10) || 'one,two')?get({row}, {col})");
        Assert.Contains("FORG0001", ex.Message);
    }

    // ----- option validation ------------------------------------------------------------

    [Theory]
    [InlineData("'row-delimiter'")]     // dropped 4.0-draft name
    [InlineData("'field-delimiter'")]   // renamed to 'separator'
    [InlineData("'column-names'")]      // dropped 4.0-draft name
    [InlineData("'bogus'")]
    public void ParseCsv_UnknownOption_XPTY0004(string key)
    {
        var ex = Error40($"fn:parse-csv('a', {{{key}: 1}})");
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Theory]
    [InlineData("'separator'", "('a', 'b')")]   // not a singleton
    [InlineData("'separator'", "'ab'")]         // not a single character
    [InlineData("'quote-character'", "''")]
    [InlineData("'comment-marker'", "('#', '!')")]
    public void ParseCsv_BadDelimiterOption_FOCV0002OrXPTY0004(string key, string value)
    {
        // Either code is acceptable (the qt4tests allow both).
        var ex = Error40($"fn:parse-csv('a', {{{key}: {value}}})");
        Assert.True(ex.Message.Contains("FOCV0002") || ex.Message.Contains("XPTY0004"), ex.Message);
    }

    [Theory]
    [InlineData("'separator'", "';'", "'quote-character'", "';'")]
    [InlineData("'separator'", "'#'", "'comment-marker'", "'#'")]
    public void ParseCsv_DistinctDelimitersRequired_FOCV0002(string key1, string value1, string key2, string value2)
    {
        var ex = Error40($"fn:parse-csv('a', {{{key1}: {value1}, {key2}: {value2}}})");
        Assert.Contains("FOCV0002", ex.Message);
    }

    [Fact]
    public void ParseCsv_NewlineDelimiter_FOCV0002()
    {
        var ex = Error40("fn:parse-csv('a', {'separator': char(10)})");
        Assert.Contains("FOCV0002", ex.Message);
    }

    [Fact]
    public void ParseCsv_NonBooleanOptionValue_XPTY0004()
    {
        var ex = Error40("fn:parse-csv('a', {'trim-whitespace': 'yes'})");
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("'a'")]
    public void ParseCsv_BadSelectColumns_XPTY0004(string value)
    {
        var ex = Error40($"fn:parse-csv('a', {{'select-columns': ({value})}})");
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void ParseCsv_NonMapOptions_XPTY0004()
    {
        var ex = Error40("fn:parse-csv('a', 'options')");
        Assert.Contains("XPTY0004", ex.Message);
    }

    [Fact]
    public void ParseCsv_EmptySequenceOptions_TreatedAsEmptyMap()
    {
        Assert.Equal(["a"],
            Seq40("fn:parse-csv('a', ())?rows[1]?1"));
    }

    [Fact]
    public void ParseCsv_KeywordArguments()
    {
        Assert.Equal(["a"],
            Seq40("fn:parse-csv(value := 'a,b', options := {'select-columns': (1)})?rows[1]?1"));
    }

    // ----- csv-to-xml --------------------------------------------------------------------

    [Fact]
    public void CsvToXml_EmptyInput_Shape()
    {
        Assert.Equal(["<csv xmlns=\"http://www.w3.org/2005/xpath-functions\"><rows/></csv>"],
            Seq40("fn:serialize(fn:csv-to-xml(''))"));
    }

    [Fact]
    public void CsvToXml_HeaderProducesColumnsAndAttributes()
    {
        Assert.Equal(["Bob", "name", "Berlin", "city"],
            Seq40("fn:csv-to-xml('name,city' || char(10) || 'Bob,Berlin', {'header': true()})" +
                  "/*:csv/*:rows/*:row/*:field/(string(), @column/string())"));
    }

    [Fact]
    public void CsvToXml_RaggedRow_FieldWithoutNameHasNoAttribute()
    {
        Assert.Equal(["name", "cake", "not a lie"],
            Seq40("fn:csv-to-xml('name' || char(10) || 'cake,not a lie', {'header': true()})" +
                  "/*:csv/*:rows/*:row/*:field/(@column/string(), string())"));
    }

    [Fact]
    public void CsvToXml_SuppliedNames_EmptyNameBeforeLastIncluded()
    {
        Assert.Equal(["<columns xmlns=\"http://www.w3.org/2005/xpath-functions\"><column>name</column><column/><column>city</column></columns>"],
            Seq40("fn:serialize(fn:csv-to-xml('', {'header': ('name', '', 'city')})/*:csv/*:columns)"));
    }

    [Fact]
    public void CsvToXml_ErrorsMatchParseCsv()
    {
        var ex = Error40("fn:csv-to-xml('a', {'bogus': 1})");
        Assert.Contains("XPTY0004", ex.Message);
    }

    // ----- bare {…} map constructor (PR2778) ----------------------------------------------

    [Fact]
    public void BareBraceMapConstructor_XPath40_Works()
    {
        Assert.Equal(["b"],
            Seq40("fn:parse-csv('a,b', {'select-columns': (2)})?rows[1]?1"));
    }

    [Fact]
    public void BareBraceMapConstructor_XPath31_XPST0003()
    {
        var ex = Assert.Throws<Bosak.XPath.Parser.XPathParseException>(() =>
            XPath31Expression.Compile("fn:count({'a': 1})", new CompileOptions()));
        Assert.Contains("XPST0003", ex.Message);
    }

    [Fact]
    public void KeywordMapConstructor_StillWorks()
    {
        Assert.Equal(["1"], Seq40("fn:count(map {'a': 1, 'b': 2})"));
    }

    // ----- consecutive for/let clauses in XPath 4.0 ----------------------------------------

    [Fact]
    public void MultiClauseFlwor_XPath40_Works()
    {
        // csv-doc-008 shape.
        Assert.Equal(["true"],
            Seq40("let $source := 'one,two' let $options := { 'header': true() } " +
                  "return fn:deep-equal(fn:parse-csv($source, $options)?rows, fn:parse-csv($source, $options)?rows)"));
    }

    [Fact]
    public void MultiClauseFlwor_XPath31_XPST0003()
    {
        var ex = Assert.Throws<Bosak.XPath.Parser.XPathParseException>(() =>
            XPath31Expression.Compile("let $a := 1 let $b := 2 return $a + $b", new CompileOptions()));
        Assert.Contains("XPST0003", ex.Message);
    }
}
