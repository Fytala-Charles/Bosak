// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 09 October 2026
// PURPOSE              : CSV parsing shared by fn:parse-csv, fn:csv-to-xml and fn:csv-doc (F&O 4.0 §17.5)
// SPECIAL NOTES        : Part of the standard XPath / XQuery function library.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 09-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Functions;

namespace Bosak.XPath.Standard.Functions;

/// <summary>
/// Shared CSV machinery for the F+O 4.0 §17.5 functions fn:parse-csv, fn:csv-to-xml and
/// fn:csv-doc: option validation (FOCV0002/XPTY0004), the RFC 4180 parser with the 4.0
/// extensions (comment rows, literal quotes in unquoted fields, blank-row rules), the
/// parsed-csv-structure-record assembly and the XML representation of fn:csv-to-xml.
/// </summary>
internal static class Csv
{
    private const string FnNamespace = "http://www.w3.org/2005/xpath-functions";

    /// <summary>Validated options of the CSV functions.</summary>
    internal sealed class CsvOptions
    {
        public char Separator = ',';
        public char Quote = '"';
        public char? Comment;
        public bool TrimWhitespace;
        public int HeaderMode; // 0 = false, 1 = true, 2 = names supplied by caller
        public string[]? HeaderNames;
        public long[]? SelectColumns;
        public bool TrimRows;
    }

    /// <summary>The analyzed CSV: adjusted column names and adjusted data rows.</summary>
    internal sealed class CsvData
    {
        public string[]? Names;
        public List<string[]> Rows = [];
    }

    /// <summary>
    /// Validates the <c>$options</c> map of the CSV functions: known keys only (XPTY0004),
    /// single-character delimiters distinct from each other and from U+000A (FOCV0002),
    /// boolean/integer/string option types (XPTY0004).
    /// </summary>
    internal static CsvOptions ParseOptions(XdmValue optionsArg)
    {
        var options = new CsvOptions();
        if (IsEmptySequence(optionsArg))
            return options;
        if (!optionsArg.IsMap)
            throw new InvalidOperationException("XPTY0004: CSV options must be a single map");

        var map = optionsArg.MapValue;
        foreach (var keyValue in map.Keys)
        {
            if (!map.TryGetValue(keyValue, out var value))
                continue;
            string key = StringValue(keyValue);
            switch (key)
            {
                case "separator":
                    options.Separator = CharOption(value, key);
                    break;
                case "quote-character":
                    options.Quote = CharOption(value, key);
                    break;
                case "comment-marker":
                    options.Comment = IsEmptySequence(value) ? null : CharOption(value, key);
                    break;
                case "trim-whitespace":
                    options.TrimWhitespace = BoolOption(value, key);
                    break;
                case "trim-rows":
                    options.TrimRows = BoolOption(value, key);
                    break;
                case "header":
                    HeaderOption(value, options);
                    break;
                case "select-columns":
                    options.SelectColumns = SelectColumnsOption(value);
                    break;
                default:
                    // Includes the dropped 4.0-draft names row-delimiter, field-delimiter,
                    // column-names (parse-csv-021/908/911/920/928/931).
                    throw new InvalidOperationException(
                        $"XPTY0004: fn:parse-csv: unknown option '{key}'.");
            }
        }

        if (options.Separator == '\n' || options.Quote == '\n' ||
            (options.Comment.HasValue && options.Comment.Value == '\n'))
            throw new InvalidOperationException("FOCV0002: CSV delimiters must not be U+000A.");
        if (options.Separator == options.Quote ||
            (options.Comment.HasValue &&
             (options.Comment.Value == options.Separator || options.Comment.Value == options.Quote)))
            throw new InvalidOperationException("FOCV0002: CSV delimiters must be distinct characters.");
        return options;
    }

    /// <summary>
    /// Parses CSV text (after CR/CRLF → LF normalization) into rows of fields. Comment rows
    /// (raw first character equals the comment marker) are discarded before any other row
    /// logic; their extent runs to the next raw newline without quote awareness. Blank rows
    /// are empty arrays: with trim-whitespace a whitespace-only row extent is blank, otherwise
    /// only a zero-length extent is. A blank row exists only when terminated by a newline; a
    /// blank unterminated tail does not exist at all (parse-csv-033/034).
    /// </summary>
    internal static List<string[]> ParseRows(string csv, CsvOptions options)
    {
        var rows = new List<string[]>();
        int n = csv.Length;
        int i = 0;
        if (n == 0)
            return rows; // zero-length input contains no rows

        while (i < n)
        {
            if (options.Comment.HasValue && csv[i] == options.Comment.Value)
            {
                while (i < n && csv[i] != '\n')
                    i++;
                if (i < n)
                    i++; // consume the terminating newline; comment rows emit no row
                continue;
            }

            int rowStart = i;
            var fields = new List<string>();
            while (true)
            {
                if (i < n && csv[i] == options.Quote)
                {
                    // A quote opens a quoted field only as the first character of the field.
                    i++;
                    var sb = new StringBuilder();
                    while (true)
                    {
                        if (i >= n)
                            throw new InvalidOperationException(
                                "FOCV0001: Unterminated quoted field in CSV input.");
                        char c = csv[i];
                        if (c == options.Quote)
                        {
                            if (i + 1 < n && csv[i + 1] == options.Quote)
                            {
                                sb.Append(options.Quote);
                                i += 2;
                            }
                            else
                            {
                                i++;
                                break;
                            }
                        }
                        else
                        {
                            sb.Append(c);
                            i++;
                        }
                    }
                    if (i < n && csv[i] != options.Separator && csv[i] != '\n')
                        throw new InvalidOperationException(
                            "FOCV0001: A closing quote must be followed by a separator or newline.");
                    fields.Add(sb.ToString());
                }
                else
                {
                    int start = i;
                    while (i < n && csv[i] != options.Separator && csv[i] != '\n')
                        i++;
                    string field = csv.Substring(start, i - start);
                    fields.Add(options.TrimWhitespace ? field.Trim() : field);
                }

                if (i < n && csv[i] == options.Separator)
                {
                    i++;
                    continue;
                }
                break;
            }

            bool terminatedByNewline = i < n && csv[i] == '\n';
            int extent = i - rowStart;
            bool blank = options.TrimWhitespace
                ? IsWhitespaceOnly(csv, rowStart, extent)
                : extent == 0;
            if (blank)
            {
                if (terminatedByNewline)
                {
                    rows.Add([]);
                    i++;
                }
                else
                {
                    i = n; // blank unterminated tail: no row exists
                }
            }
            else
            {
                rows.Add([.. fields]);
                if (terminatedByNewline)
                    i++;
                else
                    i = n;
            }
        }
        return rows;
    }

    /// <summary>
    /// Runs the full pipeline: parse, header consumption, select-columns / trim-rows
    /// adjustment, and column-index construction.
    /// </summary>
    internal static CsvData Analyze(string csv, CsvOptions options)
    {
        csv = csv.Replace("\r\n", "\n", StringComparison.Ordinal)
                 .Replace("\r", "\n", StringComparison.Ordinal);
        List<string[]> rawRows = ParseRows(csv, options);

        // trim-rows width: the FIRST ROW OF THE CSV (header row included) fixes the width
        // of every subsequent row (F+O 4.0 §17.5.7), even though the header is consumed.
        int trimWidth = options.TrimRows && rawRows.Count > 0 ? rawRows[0].Length : -1;

        string[]? names = null;
        List<string[]> rows;
        if (options.HeaderMode == 1)
        {
            // Header names are always whitespace-trimmed, regardless of options and quoting.
            if (rawRows.Count > 0)
            {
                names = [.. rawRows[0].Select(f => f.Trim())];
                rows = rawRows.Skip(1).ToList();
            }
            else
            {
                names = [];
                rows = [];
            }
        }
        else if (options.HeaderMode == 2)
        {
            names = options.HeaderNames ?? [];
            rows = rawRows;
        }
        else
        {
            rows = rawRows;
        }

        if (options.SelectColumns is { Length: > 0 } select)
        {
            rows = rows.Select(r => AdjustRow(r, select)).ToList();
            if (options.HeaderMode == 1 && names != null)
                names = AdjustRow(names, select);
            // A supplied name sequence refers to columns in the result, not the input:
            // it is not adjusted (F+O 4.0 §17.5.6).
        }
        else if (trimWidth >= 0)
        {
            rows = rows.Select(r => r.Length == trimWidth ? r : ResizeRow(r, trimWidth)).ToList();
        }

        return new CsvData { Names = names, Rows = rows };
    }

    /// <summary>Assembles the parsed-csv-structure-record map for fn:parse-csv / fn:csv-doc.</summary>
    internal static XdmValue BuildRecord(CsvData data)
    {
        var record = new XdmMap();
        record.Add(XdmValue.FromString("columns"),
            data.Names is { Length: > 0 }
                ? XdmValue.FromSequence(
                    MaterializedSequence.FromList(data.Names.Select(XdmValue.FromString).ToList()))
                : XdmValue.Undefined);
        record.Add(XdmValue.FromString("column-index"), XdmValue.FromMap(BuildColumnIndex(data.Names)));
        record.Add(XdmValue.FromString("rows"), BuildRowsEntry(data.Rows));
        record.Add(XdmValue.FromString("get"), BuildGet(data));
        return XdmValue.FromMap(record);
    }

    /// <summary>Builds the fn:csv-to-xml document for the analyzed CSV data.</summary>
    internal static XDocument BuildXml(CsvData data)
    {
        XNamespace fn = FnNamespace;
        var csvElement = new XElement(fn + "csv");
        string[]? names = data.Names;
        if (names != null)
        {
            int lastCol = -1;
            for (int i = 0; i < names.Length; i++)
                if (names[i].Length != 0)
                    lastCol = i;
            if (lastCol >= 0)
            {
                var columns = new XElement(fn + "columns");
                for (int i = 0; i <= lastCol; i++)
                    columns.Add(new XElement(fn + "column",
                        names[i].Length == 0 ? null : names[i]));
                csvElement.Add(columns);
            }
        }

        var rowsElement = new XElement(fn + "rows");
        foreach (string[] row in data.Rows)
        {
            var rowElement = new XElement(fn + "row");
            for (int c = 0; c < row.Length; c++)
            {
                var field = new XElement(fn + "field", row[c].Length == 0 ? null : row[c]);
                if (names != null && c < names.Length && names[c].Length != 0)
                    field.Add(new XAttribute("column", names[c]));
                rowElement.Add(field);
            }
            rowsElement.Add(rowElement);
        }
        csvElement.Add(rowsElement);
        return new XDocument(csvElement);
    }

    private static XdmMap BuildColumnIndex(string[]? names)
    {
        var index = new XdmMap();
        if (names == null)
            return index;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i].Length != 0 && seen.Add(names[i]))
                index.Add(XdmValue.FromString(names[i]), XdmValue.FromInteger(i + 1));
        }
        return index;
    }

    private static XdmValue BuildRowsEntry(List<string[]> rows)
    {
        if (rows.Count == 0)
            return XdmValue.Undefined;
        var arrays = new List<XdmValue>(rows.Count);
        foreach (string[] row in rows)
            arrays.Add(XdmValue.FromArray(new XdmArray(row.Select(XdmValue.FromString))));
        return XdmValue.FromSequence(MaterializedSequence.FromList(arrays));
    }

    private static XdmValue BuildGet(CsvData data)
    {
        List<string[]> rows = data.Rows;
        var columnIndex = BuildColumnIndex(data.Names);
        return XdmValue.FromFunction(new DelegateFunctionItem(2, (callCtx, callArgs) =>
        {
            long r = IntegerArgument(callArgs[0]);
            XdmValue c = Atomize(callArgs[1]);
            long col;
            if (c.Kind == XdmValueKind.String)
            {
                if (!columnIndex.TryGetValue(XdmValue.FromString(c.ToString()), out var position))
                    throw new InvalidOperationException(
                        $"FOCV0004: CSV column '{c}' does not occur in the column names.");
                col = position.IntegerValue;
            }
            else
            {
                col = IntegerValueOf(c);
            }

            // $r and an integer $c are xs:positiveInteger: zero or negative values fail
            // the conversion (FORG0001, parse-csv-914..917/919); positive out-of-range
            // values return the zero-length string.
            if (r < 1)
                throw new InvalidOperationException(
                    $"FORG0001: Cannot convert {r} to xs:positiveInteger (row number in CSV get function).");
            if (c.Kind != XdmValueKind.String && col < 1)
                throw new InvalidOperationException(
                    $"FORG0001: Cannot convert {col} to xs:positiveInteger (column number in CSV get function).");

            if (r > rows.Count)
                return XdmValue.FromString(string.Empty);
            string[] row = rows[(int)(r - 1)];
            if (col < 1 || col > row.Length)
                return XdmValue.FromString(string.Empty);
            return XdmValue.FromString(row[col - 1]);
        }));
    }

    private static string[] AdjustRow(string[] row, long[] select)
    {
        var result = new string[select.Length];
        for (int i = 0; i < select.Length; i++)
        {
            long index = select[i];
            result[i] = index >= 1 && index <= row.Length ? row[index - 1] : string.Empty;
        }
        return result;
    }

    private static string[] ResizeRow(string[] row, int width)
    {
        if (row.Length == width)
            return row;
        var result = new string[width];
        Array.Fill(result, string.Empty);
        Array.Copy(row, result, Math.Min(row.Length, width));
        return result;
    }

    private static bool IsWhitespaceOnly(string s, int start, int length)
    {
        for (int i = start; i < start + length; i++)
        {
            if (!char.IsWhiteSpace(s[i]))
                return false;
        }
        return true;
    }

    private static char CharOption(XdmValue value, string key)
    {
        var items = AsItems(value).ToList();
        if (items.Count > 1)
            throw new InvalidOperationException(
                $"XPTY0004: The {key} option must be a single character.");
        string s = items.Count == 0 ? string.Empty : StringValue(items[0]);
        if (s.Length != 1)
            throw new InvalidOperationException(
                $"FOCV0002: The {key} option must be a single character.");
        return s[0];
    }

    private static bool BoolOption(XdmValue value, string key)
    {
        XdmValue atom = Atomize(value);
        if (atom.Kind != XdmValueKind.Boolean)
            throw new InvalidOperationException(
                $"XPTY0004: The {key} option must be a single xs:boolean.");
        return atom.BooleanValue;
    }

    private static void HeaderOption(XdmValue value, CsvOptions options)
    {
        var items = new List<XdmValue>();
        if (value.IsUndefined)
        {
            // Empty sequence: a sequence of zero names.
        }
        else if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                items.Add(item);
        }
        else
        {
            items.Add(value);
        }

        if (items.Count == 1 && Atomize(items[0]).Kind == XdmValueKind.Boolean)
        {
            options.HeaderMode = Atomize(items[0]).BooleanValue ? 1 : 0;
            return;
        }
        var names = new string[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            XdmValue atom = Atomize(items[i]);
            if (atom.Kind == XdmValueKind.Map || atom.Kind == XdmValueKind.Array ||
                atom.Kind == XdmValueKind.Function)
                throw new InvalidOperationException(
                    "XPTY0004: The header option must be a boolean or a sequence of strings.");
            names[i] = atom.ToString();
        }
        options.HeaderMode = 2;
        options.HeaderNames = names;
    }

    private static long[] SelectColumnsOption(XdmValue value)
    {
        var result = new List<long>();
        if (value.IsUndefined)
            return [];
        foreach (var item in AsItems(value))
        {
            XdmValue atom = Atomize(item);
            long index = atom.Kind switch
            {
                XdmValueKind.Integer => atom.IntegerValue,
                XdmValueKind.Decimal => (long)atom.DecimalValue,
                XdmValueKind.Double or XdmValueKind.Float => (long)atom.DoubleValue,
                XdmValueKind.String => long.TryParse(atom.ToString(), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var parsed) ? parsed : -1,
                _ => -1,
            };
            if (index < 1)
                throw new InvalidOperationException(
                    "XPTY0004: The select-columns option must be a sequence of xs:positiveInteger.");
            result.Add(index);
        }
        return [.. result];
    }

    private static long IntegerArgument(XdmValue value)
    {
        XdmValue atom = Atomize(value);
        if (atom.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(atom.SequenceValue!))
                return IntegerValueOf(Atomize(item));
            return 0;
        }
        return IntegerValueOf(atom);
    }

    private static long IntegerValueOf(XdmValue atom) => atom.Kind switch
    {
        XdmValueKind.Integer => atom.IntegerValue,
        XdmValueKind.Decimal => (long)atom.DecimalValue,
        XdmValueKind.Double or XdmValueKind.Float => (long)atom.DoubleValue,
        XdmValueKind.String => long.TryParse(atom.ToString(), NumberStyles.Integer,
            CultureInfo.InvariantCulture, out var parsed) ? parsed : 0,
        _ => 0,
    };

    /// <summary>Atomizes a single item: nodes to their string value, rejecting function items.</summary>
    private static XdmValue Atomize(XdmValue value)
    {
        if (value.IsNode)
            return XdmValue.FromString(value.NodeValue.StringValue);
        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                return Atomize(item);
            return XdmValue.Undefined;
        }
        return value;
    }

    private static string StringValue(XdmValue value) => Atomize(value).ToString();

    private static IEnumerable<XdmValue> AsItems(XdmValue value)
    {
        if (value.IsUndefined)
            yield break;
        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                yield return item;
        }
        else
        {
            yield return value;
        }
    }

    private static bool IsEmptySequence(XdmValue value)
    {
        if (value.IsUndefined)
            return true;
        if (value.IsSequence)
        {
            foreach (var _ in XdmSequence.FromSource(value.SequenceValue!))
                return false;
            return true;
        }
        return false;
    }
}
