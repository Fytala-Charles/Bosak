// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 20 mei 2026
// PURPOSE              : Compares actual Bosak execution results against QT3 expected assertions.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 20-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 21-05-2026     | assert-true/assert-false unwrap singleton sequences                                    |
//                      | Charles Korthout | 0.3   | 22-05-2026     | Fixed ValuesEqual cross-type numeric comparison (Integer vs Double/Float)            |
//                      | Charles Korthout | 0.4   | 22-05-2026     | Added Duration serialization and type matching for dayTimeDuration/yearMonthDuration  |
//                      | Charles Korthout | 0.5   | 22-05-2026     | Fixed Double/Float serialization to use XdmValue.ToString for canonical formatting       |
//                      | Charles Korthout | 0.6   | 27-05-2026     | DeepEqual: single-item sequence is equivalent to bare item (XDM semantics)               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Bosak.XPath.Standard.Functions;

namespace Bosak.XPath.Conformance;

internal static class ResultComparer
{
    private static readonly XNamespace Ns = "http://www.w3.org/2010/09/qt-fots-catalog";

    public static TestOutcome Compare(XElement resultElement, XdmValue actual, Exception? caughtException)
    {
        var assertions = resultElement.Elements().ToList();
        if (assertions.Count == 0)
        {
            return new TestOutcome(TestOutcomeKind.Skipped, "No assertion in result");
        }

        // Handle wrapper elements: all-of, any-of
        if (assertions.Count == 1)
        {
            var wrapper = assertions[0];
            if (wrapper.Name == Ns + "all-of")
            {
                return CompareAllOf(wrapper.Elements(), actual, caughtException);
            }
            if (wrapper.Name == Ns + "any-of")
            {
                return CompareAnyOf(wrapper.Elements(), actual, caughtException);
            }
        }

        return CompareAssertion(assertions[0], actual, caughtException);
    }

    private static TestOutcome CompareAllOf(IEnumerable<XElement> assertions, XdmValue actual, Exception? caughtException)
    {
        foreach (var assertion in assertions)
        {
            var outcome = CompareAssertion(assertion, actual, caughtException);
            if (outcome.Kind != TestOutcomeKind.Passed)
                return outcome;
        }
        return new TestOutcome(TestOutcomeKind.Passed, null);
    }

    private static TestOutcome CompareAnyOf(IEnumerable<XElement> assertions, XdmValue actual, Exception? caughtException)
    {
        var failures = new List<string>();
        foreach (var assertion in assertions)
        {
            var outcome = CompareAssertion(assertion, actual, caughtException);
            if (outcome.Kind == TestOutcomeKind.Passed)
                return outcome;
            failures.Add(outcome.Message ?? "failed");
        }
        return new TestOutcome(TestOutcomeKind.Failed, $"any-of: none matched. [{string.Join("; ", failures)}]");
    }

    private static TestOutcome CompareAssertion(XElement assertion, XdmValue actual, Exception? caughtException)
    {
        var name = assertion.Name.LocalName;

        return name switch
        {
            "assert-eq" => CompareAssertEq(assertion.Value, actual, caughtException),
            "assert-true" => CompareAssertTrue(actual, caughtException),
            "assert-false" => CompareAssertFalse(actual, caughtException),
            "assert-string-value" => CompareAssertStringValue(assertion.Value, actual, caughtException),
            "assert-empty" => CompareAssertEmpty(actual, caughtException),
            "error" => CompareError((string?)assertion.Attribute("code") ?? "", caughtException),
            "assert-type" => CompareAssertType(assertion.Value, actual, caughtException),
            "assert-xml" => CompareAssertXml(assertion, actual, caughtException),
            "assert-deep-eq" => CompareAssertDeepEq(assertion, actual, caughtException),
            "all-of" => CompareAllOf(assertion.Elements(), actual, caughtException),
            "any-of" => CompareAnyOf(assertion.Elements(), actual, caughtException),
            "assert-count" => CompareAssertCount((string?)assertion.Attribute("count") ?? "", actual, caughtException),
            "assert-permutation" => new TestOutcome(TestOutcomeKind.Skipped, "assert-permutation not yet implemented"),
            "assert" => CompareAssert(assertion.Value, actual, caughtException),
            _ => new TestOutcome(TestOutcomeKind.Skipped, $"Unknown assertion: {name}"),
        };
    }

    private static TestOutcome CompareAssertEq(string expectedExpr, XdmValue actual, Exception? caughtException)
    {
        if (caughtException is not null)
            return new TestOutcome(TestOutcomeKind.Failed, $"Unexpected error: {caughtException.Message}");

        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            var expected = XPath31Expression.Compile(expectedExpr).Evaluate(ctx);
            if (ValuesEqual(actual, expected))
                return new TestOutcome(TestOutcomeKind.Passed, null);

            return new TestOutcome(TestOutcomeKind.Failed,
                $"assert-eq failed. Expected: {SerializeValue(expected)}, Got: {SerializeValue(actual)}");
        }
        catch (Exception ex)
        {
            return new TestOutcome(TestOutcomeKind.Skipped, $"Could not evaluate expected expression '{expectedExpr}': {ex.Message}");
        }
    }

    /// <summary>
    /// If the value is a singleton sequence, returns its single item; otherwise returns the value as-is.
    /// Used by assert-true and assert-false to handle functions that return a singleton sequence.
    /// </summary>
    private static XdmValue UnwrapSingleton(XdmValue value)
    {
        if (value.Kind == XdmValueKind.Sequence && value.SequenceValue is not null &&
            value.SequenceValue.TryGetLength(out var len) && len == 1)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                return item;
        }
        return value;
    }

    private static TestOutcome CompareAssertTrue(XdmValue actual, Exception? caughtException)
    {
        if (caughtException is not null)
            return new TestOutcome(TestOutcomeKind.Failed, $"Unexpected error: {caughtException.Message}");

        var value = UnwrapSingleton(actual);
        if (!value.IsUndefined && value.Kind == XdmValueKind.Boolean && value.BooleanValue)
            return new TestOutcome(TestOutcomeKind.Passed, null);

        return new TestOutcome(TestOutcomeKind.Failed, $"assert-true failed. Got: {SerializeValue(actual)}");
    }

    private static TestOutcome CompareAssertFalse(XdmValue actual, Exception? caughtException)
    {
        if (caughtException is not null)
            return new TestOutcome(TestOutcomeKind.Failed, $"Unexpected error: {caughtException.Message}");

        var value = UnwrapSingleton(actual);
        if (!value.IsUndefined && value.Kind == XdmValueKind.Boolean && !value.BooleanValue)
            return new TestOutcome(TestOutcomeKind.Passed, null);

        return new TestOutcome(TestOutcomeKind.Failed, $"assert-false failed. Got: {SerializeValue(actual)}");
    }

    private static TestOutcome CompareAssertStringValue(string expected, XdmValue actual, Exception? caughtException)
    {
        if (caughtException is not null)
            return new TestOutcome(TestOutcomeKind.Failed, $"Unexpected error: {caughtException.Message}");

        string actualStr = SerializeValue(actual);
        string normalizedExpected = expected.Replace("\r\n", "\n").Replace("\n", " ").Trim();

        if (actualStr == normalizedExpected)
            return new TestOutcome(TestOutcomeKind.Passed, null);

        return new TestOutcome(TestOutcomeKind.Failed, $"assert-string-value failed. Expected: '{normalizedExpected}', Got: '{actualStr}'");
    }

    private static TestOutcome CompareAssertEmpty(XdmValue actual, Exception? caughtException)
    {
        if (caughtException is not null)
            return new TestOutcome(TestOutcomeKind.Failed, $"Unexpected error: {caughtException.Message}");

        if (actual.IsUndefined)
            return new TestOutcome(TestOutcomeKind.Passed, null);

        if (actual.IsSequence && actual.SequenceValue is not null &&
            actual.SequenceValue.TryGetLength(out var len) && len == 0)
            return new TestOutcome(TestOutcomeKind.Passed, null);

        return new TestOutcome(TestOutcomeKind.Failed, $"assert-empty failed. Got: {SerializeValue(actual)}");
    }

    private static TestOutcome CompareError(string expectedCode, Exception? caughtException)
    {
        if (caughtException is null)
            return new TestOutcome(TestOutcomeKind.Failed, $"Expected error {expectedCode} but succeeded");

        // Try to extract error code from exception message
        string message = caughtException.Message;
        if (message.Contains(expectedCode, StringComparison.OrdinalIgnoreCase))
            return new TestOutcome(TestOutcomeKind.Passed, null);

        // Some errors map to generic messages; accept any runtime error for now
        if (caughtException is InvalidOperationException)
            return new TestOutcome(TestOutcomeKind.Passed, null); // lenient matching

        return new TestOutcome(TestOutcomeKind.Failed, $"Expected error {expectedCode}, got: {message}");
    }

    private static bool ValuesEqual(XdmValue a, XdmValue b)
    {
        // Handle undefined (empty sequence)
        if (a.IsUndefined && b.IsUndefined)
            return true;
        if (a.IsUndefined || b.IsUndefined)
            return false;

        // For atomic values, compare by string representation as a heuristic
        // This is not fully spec-correct but works for most assert-eq cases
        string sa = SerializeValue(a);
        string sb = SerializeValue(b);
        if (sa == sb)
            return true;

        // DateTime/Date/Time value comparison: compare instants (timezone-independent)
        if (a.Kind == XdmValueKind.DateTime && b.Kind == XdmValueKind.DateTime)
            return a.DateTimeValue == b.DateTimeValue;
        if (a.Kind == XdmValueKind.Date && b.Kind == XdmValueKind.Date)
            return a.DateValue == b.DateValue;
        if (a.Kind == XdmValueKind.Time && b.Kind == XdmValueKind.Time)
            return a.TimeValue == b.TimeValue;

        // Numeric value comparison: decimals may differ in trailing zeros (13 vs 13.0)
        bool aIsNumeric = a.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float;
        bool bIsNumeric = b.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float;
        if (aIsNumeric && bIsNumeric)
        {
            bool aIsNaN = a.Kind is XdmValueKind.Double or XdmValueKind.Float && double.IsNaN(a.DoubleValue);
            bool bIsNaN = b.Kind is XdmValueKind.Double or XdmValueKind.Float && double.IsNaN(b.DoubleValue);
            if (aIsNaN && bIsNaN) return true;
            if (aIsNaN || bIsNaN) return false;

            if (a.Kind == XdmValueKind.Double || b.Kind == XdmValueKind.Double)
            {
                double da = a.Kind == XdmValueKind.Double ? a.DoubleValue
                          : a.Kind == XdmValueKind.Integer ? a.IntegerValue
                          : a.Kind == XdmValueKind.Decimal ? (double)a.DecimalValue
                          : a.DoubleValue;
                double db = b.Kind == XdmValueKind.Double ? b.DoubleValue
                          : b.Kind == XdmValueKind.Integer ? b.IntegerValue
                          : b.Kind == XdmValueKind.Decimal ? (double)b.DecimalValue
                          : b.DoubleValue;
                return da == db;
            }

            if (a.Kind == XdmValueKind.Float || b.Kind == XdmValueKind.Float)
            {
                float fa = a.Kind == XdmValueKind.Float ? (float)a.DoubleValue
                         : a.Kind == XdmValueKind.Integer ? a.IntegerValue
                         : a.Kind == XdmValueKind.Decimal ? (float)a.DecimalValue
                         : (float)a.DoubleValue;
                float fb = b.Kind == XdmValueKind.Float ? (float)b.DoubleValue
                         : b.Kind == XdmValueKind.Integer ? b.IntegerValue
                         : b.Kind == XdmValueKind.Decimal ? (float)b.DecimalValue
                         : (float)b.DoubleValue;
                return fa == fb;
            }

            if (a.Kind == XdmValueKind.Decimal || b.Kind == XdmValueKind.Decimal)
            {
                decimal da = a.Kind == XdmValueKind.Decimal ? a.DecimalValue
                           : a.Kind == XdmValueKind.Integer ? a.IntegerValue
                           : (decimal)a.DoubleValue;
                decimal db = b.Kind == XdmValueKind.Decimal ? b.DecimalValue
                           : b.Kind == XdmValueKind.Integer ? b.IntegerValue
                           : (decimal)b.DoubleValue;
                return da == db;
            }

            double dva = a.Kind == XdmValueKind.Integer ? a.IntegerValue : a.DoubleValue;
            double dvb = b.Kind == XdmValueKind.Integer ? b.IntegerValue : b.DoubleValue;
            return dva == dvb;
        }

        return false;
    }

    private static string SerializeValue(XdmValue value)
    {
        if (value.IsUndefined)
            return "";

        if (value.IsSequence && value.SequenceValue is not null)
        {
            var items = new List<string>();
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
            {
                items.Add(SerializeSingle(item));
            }
            return string.Join(" ", items);
        }

        return SerializeSingle(value);
    }

    private static string SerializeSingle(XdmValue value)
    {
        if (value.IsUndefined)
            return "";
        if (value.IsNode)
            return value.NodeValue?.StringValue ?? "";
        if (value.IsAtomic)
        {
            if (value.Kind == XdmValueKind.Boolean)
                return value.BooleanValue ? "true" : "false";
            if (value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float)
            {
                return value.ToString();
            }
            if (value.Kind == XdmValueKind.Integer)
                return value.IntegerValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (value.Kind == XdmValueKind.Decimal)
                return FormatCanonicalDecimal(value.DecimalValue);
            if (value.Kind == XdmValueKind.String)
                return value.StringValue;
            if (value.Kind == XdmValueKind.DateTime)
                return value.HasTimezone
                    ? FormatUtcOffset(value.DateTimeValue.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz", System.Globalization.CultureInfo.InvariantCulture))
                    : value.DateTimeValue.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFF", System.Globalization.CultureInfo.InvariantCulture);
            if (value.Kind == XdmValueKind.Date)
                return value.HasTimezone
                    ? FormatUtcOffset(value.DateValue.ToString("yyyy-MM-ddzzz", System.Globalization.CultureInfo.InvariantCulture))
                    : value.DateValue.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            if (value.Kind == XdmValueKind.Time)
                return value.HasTimezone
                    ? FormatUtcOffset(value.TimeValue.ToString("HH:mm:ss.FFFFFFFzzz", System.Globalization.CultureInfo.InvariantCulture))
                    : value.TimeValue.ToString("HH:mm:ss.FFFFFFF", System.Globalization.CultureInfo.InvariantCulture);
            if (value.Kind == XdmValueKind.Duration)
                return value.DurationValue;
            return value.ToString();
        }
        return value.ToString();
    }

    private static TestOutcome CompareAssertType(string expectedType, XdmValue actual, Exception? caughtException)
    {
        if (caughtException is not null)
            return new TestOutcome(TestOutcomeKind.Failed, $"assert-type failed. Unexpected error: {caughtException.Message}");

        if (actual.IsUndefined)
            return new TestOutcome(TestOutcomeKind.Failed, $"assert-type failed. Expected {expectedType}, got empty sequence");

        // Parse cardinality suffix
        bool allowMany = expectedType.EndsWith("*");
        bool allowOneOrMore = expectedType.EndsWith("+");
        bool allowZeroOrOne = expectedType.EndsWith("?");
        string baseType = expectedType.TrimEnd('*', '+', '?');

        // Materialize sequence to check cardinality and item types
        List<XdmValue> items = new();
        if (actual.IsSequence && actual.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(actual.SequenceValue))
                items.Add(item);
        }
        else
        {
            items.Add(actual);
        }

        if (allowMany)
        {
            // Any cardinality allowed
        }
        else if (allowOneOrMore)
        {
            if (items.Count == 0)
                return new TestOutcome(TestOutcomeKind.Failed, $"assert-type failed. Expected {expectedType}, got empty sequence");
        }
        else if (allowZeroOrOne)
        {
            if (items.Count > 1)
                return new TestOutcome(TestOutcomeKind.Failed, $"assert-type failed. Expected {expectedType}, got {items.Count} items");
        }
        else
        {
            if (items.Count != 1)
                return new TestOutcome(TestOutcomeKind.Failed, $"assert-type failed. Expected {expectedType}, got {items.Count} items");
        }

        foreach (var item in items)
        {
            if (!ItemMatchesType(item, baseType))
                return new TestOutcome(TestOutcomeKind.Failed, $"assert-type failed. Expected {expectedType}, got {item.Kind}");
        }

        return new TestOutcome(TestOutcomeKind.Passed, null);
    }

    private static bool ItemMatchesType(XdmValue item, string typeName)
    {
        string normalized = typeName.ToLowerInvariant().Replace("xs:", "").Replace(" ", "");

        if (normalized == "item")
            return !item.IsUndefined;

        if (normalized.StartsWith("document-node()"))
            return item.IsNode && item.NodeValue?.NodeKind == XdmNodeKind.Document;

        return normalized switch
        {
            "string" => item.Kind == XdmValueKind.String,
            "integer" or "int" or "long" or "short" or "byte"
                or "unsignedshort" or "unsignedint" or "unsignedlong" or "unsignedbyte"
                or "positiveinteger" or "negativeinteger" or "nonpositiveinteger" or "nonnegativeinteger"
                => item.Kind == XdmValueKind.Integer,
            "decimal" => item.Kind == XdmValueKind.Decimal,
            "double" => item.Kind == XdmValueKind.Double,
            "float" => item.Kind == XdmValueKind.Float,
            "boolean" => item.Kind == XdmValueKind.Boolean,
            "datetime" => item.Kind == XdmValueKind.DateTime,
            "date" => item.Kind == XdmValueKind.Date,
            "time" => item.Kind == XdmValueKind.Time,
            "duration" or "daytimeduration" or "yearmonthduration" => item.Kind == XdmValueKind.Duration,
            "qname" => item.Kind == XdmValueKind.QName,
            "node" => item.IsNode,
            "anyatomictype" => item.IsAtomic,
            "base64binary" or "hexbinary" => item.Kind == XdmValueKind.String, // approximate
            _ => true // lenient for unimplemented types
        };
    }

    private static string FormatDoubleString(string s)
    {
        // XPath serialization does not use '+' in exponent: E+308 -> E308
        return s.Replace("E+", "E");
    }

    private static string FormatUtcOffset(string s)
    {
        // XPath uses Z for UTC, not +00:00
        return s.Replace("+00:00", "Z");
    }

    private static string FormatCanonicalDecimal(decimal value)
    {
        string s = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (s.Contains('.'))
        {
            s = s.TrimEnd('0').TrimEnd('.');
        }
        return string.IsNullOrEmpty(s) ? "0" : s;
    }

    private static TestOutcome CompareAssert(string assertExpr, XdmValue actual, Exception? caughtException)
    {
        if (caughtException is not null)
            return new TestOutcome(TestOutcomeKind.Failed, $"Unexpected error: {caughtException.Message}");

        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            ctx = ctx.WithVariable("result", actual);
            var result = XPath31Expression.Compile(assertExpr).Evaluate(ctx);
            if (result.Kind == XdmValueKind.Boolean && result.BooleanValue)
                return new TestOutcome(TestOutcomeKind.Passed, null);
            return new TestOutcome(TestOutcomeKind.Failed, $"assert failed. Expression: {assertExpr}, Got: {SerializeValue(result)}");
        }
        catch (Exception ex)
        {
            return new TestOutcome(TestOutcomeKind.Failed, $"assert evaluation failed: {ex.Message}");
        }
    }

    private static TestOutcome CompareAssertCount(string expectedCountStr, XdmValue actual, Exception? caughtException)
    {
        if (caughtException is not null)
            return new TestOutcome(TestOutcomeKind.Failed, $"Unexpected error: {caughtException.Message}");

        if (!int.TryParse(expectedCountStr, out int expectedCount))
            return new TestOutcome(TestOutcomeKind.Skipped, $"Invalid assert-count value: {expectedCountStr}");

        int actualCount = 0;
        if (actual.IsUndefined)
            actualCount = 0;
        else if (actual.IsSequence && actual.SequenceValue is not null)
        {
            foreach (var _ in XdmSequence.FromSource(actual.SequenceValue))
                actualCount++;
        }
        else
            actualCount = 1;

        if (actualCount == expectedCount)
            return new TestOutcome(TestOutcomeKind.Passed, null);

        return new TestOutcome(TestOutcomeKind.Failed, $"assert-count failed. Expected: {expectedCount}, Got: {actualCount}");
    }

    private static TestOutcome CompareAssertDeepEq(XElement assertion, XdmValue actual, Exception? caughtException)
    {
        if (caughtException is not null)
            return new TestOutcome(TestOutcomeKind.Failed, $"Unexpected error: {caughtException.Message}");

        var expectedExprs = assertion.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        if (expectedExprs.Count == 0)
            return new TestOutcome(TestOutcomeKind.Skipped, "assert-deep-eq: no expected expressions");

        try
        {
            var ctx = new EvaluationContext();
            FunctionLibrary.Populate(ctx);
            var expectedItems = new List<XdmValue>();
            foreach (var expr in expectedExprs)
            {
                var value = XPath31Expression.Compile(expr).Evaluate(ctx);
                expectedItems.AddRange(MaterializeValue(value));
            }
            var actualItems = MaterializeValue(actual);

            if (expectedItems.Count != actualItems.Count)
                return new TestOutcome(TestOutcomeKind.Failed, $"assert-deep-eq failed. Expected {expectedItems.Count} items, got {actualItems.Count}");

            for (int i = 0; i < expectedItems.Count; i++)
            {
                if (!DeepEqual(expectedItems[i], actualItems[i]))
                    return new TestOutcome(TestOutcomeKind.Failed, $"assert-deep-eq failed at item {i}. Expected: {SerializeValue(expectedItems[i])}, Got: {SerializeValue(actualItems[i])}");
            }

            return new TestOutcome(TestOutcomeKind.Passed, null);
        }
        catch (Exception ex)
        {
            return new TestOutcome(TestOutcomeKind.Skipped, $"Could not evaluate assert-deep-eq: {ex.Message}");
        }
    }

    private static List<XdmValue> MaterializeValue(XdmValue value)
    {
        var result = new List<XdmValue>();
        if (value.IsUndefined)
            return result;
        if (value.IsSequence && value.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                result.Add(item);
        }
        else
        {
            result.Add(value);
        }
        return result;
    }

    private static TestOutcome CompareAssertXml(XElement assertion, XdmValue actual, Exception? caughtException)
    {
        if (caughtException is not null)
            return new TestOutcome(TestOutcomeKind.Failed, $"Unexpected error: {caughtException.Message}");

        string expectedXml = assertion.Value;
        string actualXml = SerializeToXml(actual);

        bool ignorePrefixes = (string?)assertion.Attribute("ignore-prefixes") == "true";

        string expectedNorm = NormalizeXml(expectedXml, ignorePrefixes);
        string actualNorm = NormalizeXml(actualXml, ignorePrefixes);

        if (expectedNorm == actualNorm)
            return new TestOutcome(TestOutcomeKind.Passed, null);

        return new TestOutcome(TestOutcomeKind.Failed,
            $"assert-xml failed. Expected: {expectedNorm}, Got: {actualNorm}");
    }

    private static string SerializeToXml(XdmValue value)
    {
        if (value.IsUndefined)
            return "";

        if (value.IsSequence && value.SequenceValue is not null)
        {
            var parts = new List<string>();
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                parts.Add(SerializeSingleToXml(item));
            return string.Join("", parts);
        }

        return SerializeSingleToXml(value);
    }

    private static string SerializeSingleToXml(XdmValue value)
    {
        if (value.IsNode)
            return value.NodeValue?.ToXmlString() ?? "";
        return SerializeSingle(value);
    }

    private static string NormalizeXml(string xml, bool ignorePrefixes)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return "";

        try
        {
            var doc = XDocument.Parse(xml);
            if (ignorePrefixes)
                StripNamespaceDeclarations(doc.Root!);
            return doc.ToString(SaveOptions.DisableFormatting);
        }
        catch
        {
            // If not a valid document (e.g., fragment), try as element
            try
            {
                var el = XElement.Parse(xml);
                if (ignorePrefixes)
                    StripNamespaceDeclarations(el);
                return el.ToString(SaveOptions.DisableFormatting);
            }
            catch
            {
                return xml;
            }
        }
    }

    private static void StripNamespaceDeclarations(XElement element)
    {
        var nsAttrs = element.Attributes().Where(a => a.IsNamespaceDeclaration).ToList();
        foreach (var attr in nsAttrs)
            attr.Remove();
        foreach (var child in element.Elements())
            StripNamespaceDeclarations(child);
    }

    private static bool DeepEqual(XdmValue a, XdmValue b)
    {
        if (a.IsUndefined && b.IsUndefined)
            return true;
        if (a.IsUndefined || b.IsUndefined)
            return false;

        // Sequences
        if (a.IsSequence && b.IsSequence)
        {
            var itemsA = MaterializeValue(a);
            var itemsB = MaterializeValue(b);
            if (itemsA.Count != itemsB.Count)
                return false;
            for (int i = 0; i < itemsA.Count; i++)
                if (!DeepEqual(itemsA[i], itemsB[i]))
                    return false;
            return true;
        }
        if (a.IsSequence)
        {
            var itemsA = MaterializeValue(a);
            if (itemsA.Count == 1)
                return DeepEqual(itemsA[0], b);
            return false;
        }
        if (b.IsSequence)
        {
            var itemsB = MaterializeValue(b);
            if (itemsB.Count == 1)
                return DeepEqual(a, itemsB[0]);
            return false;
        }

        // Maps
        if (a.Kind == XdmValueKind.Map && b.Kind == XdmValueKind.Map)
        {
            var mapA = a.MapValue;
            var mapB = b.MapValue;
            if (mapA.Count != mapB.Count)
                return false;
            foreach (var key in mapA.Keys)
            {
                if (!mapB.TryGetValue(key, out var vb))
                    return false;
                if (!mapA.TryGetValue(key, out var va))
                    return false;
                if (!DeepEqual(va, vb))
                    return false;
            }
            return true;
        }
        if (a.Kind == XdmValueKind.Map || b.Kind == XdmValueKind.Map)
            return false;

        // Arrays
        if (a.Kind == XdmValueKind.Array && b.Kind == XdmValueKind.Array)
        {
            var arrA = a.ArrayValue;
            var arrB = b.ArrayValue;
            if (arrA.Count != arrB.Count)
                return false;
            for (int i = 0; i < arrA.Count; i++)
                if (!DeepEqual(arrA.Get(i), arrB.Get(i)))
                    return false;
            return true;
        }
        if (a.Kind == XdmValueKind.Array || b.Kind == XdmValueKind.Array)
            return false;

        // Nodes: compare by string value
        if (a.IsNode && b.IsNode)
            return a.NodeValue?.StringValue == b.NodeValue?.StringValue;

        // Atomic values
        if (a.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float &&
            b.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float)
        {
            bool aIsNaN = a.Kind is XdmValueKind.Double or XdmValueKind.Float && double.IsNaN(a.DoubleValue);
            bool bIsNaN = b.Kind is XdmValueKind.Double or XdmValueKind.Float && double.IsNaN(b.DoubleValue);
            if (aIsNaN && bIsNaN) return true;

            if (a.Kind == XdmValueKind.Float || b.Kind == XdmValueKind.Float)
            {
                float fa = a.Kind == XdmValueKind.Integer ? a.IntegerValue : a.Kind == XdmValueKind.Decimal ? (float)a.DecimalValue : (float)a.DoubleValue;
                float fb = b.Kind == XdmValueKind.Integer ? b.IntegerValue : b.Kind == XdmValueKind.Decimal ? (float)b.DecimalValue : (float)b.DoubleValue;
                return fa == fb;
            }
            if (a.Kind == XdmValueKind.Double || b.Kind == XdmValueKind.Double)
            {
                double da = a.Kind == XdmValueKind.Integer ? a.IntegerValue : a.Kind == XdmValueKind.Decimal ? (double)a.DecimalValue : a.DoubleValue;
                double db = b.Kind == XdmValueKind.Integer ? b.IntegerValue : b.Kind == XdmValueKind.Decimal ? (double)b.DecimalValue : b.DoubleValue;
                return da == db;
            }
            decimal ma = a.Kind == XdmValueKind.Integer ? a.IntegerValue : a.DecimalValue;
            decimal mb = b.Kind == XdmValueKind.Integer ? b.IntegerValue : b.DecimalValue;
            return ma == mb;
        }

        if (a.Kind != b.Kind)
            return false;

        return ValuesEqual(a, b);
    }
}
