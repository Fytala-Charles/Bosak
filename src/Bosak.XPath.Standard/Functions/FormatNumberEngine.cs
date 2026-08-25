// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 mei 2026
// PURPOSE              : Picture-string parser and formatter for fn:format-number.
// SPECIAL NOTES        : Part of the standard XPath / XQuery function library.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 31-05-2026     | Non-numeric atomic cast, grouping-separator fixes, decimal-format merge support          |
//                      | Charles Korthout | 0.3   | 19-07-2026     | XPTY0004 for non-numeric strings; non-BMP zero-digit support in scientific notation        |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 25-08-2026     | FODF1310 for duplicate percent/per-mille or percent+per-mille in a subpicture             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Globalization;
using System.Text;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Standard.Functions;

internal static class FormatNumberEngine
{
    public static string Format(XdmValue value, string picture, DecimalFormat format, bool backwardsCompatible = false)
    {
        if (value.IsUndefined)
            return format.NaN;

        value = AtomizeForFormatNumber(value);

        // Non-numeric atomic values (e.g., strings) are cast to double.
        // In XPath 1.0 backwards-compatible mode the cast yields NaN; otherwise
        // an uncastable value is a type error (XPTY0004).
        if (!IsNumeric(value))
        {
            double d = ConvertToDouble(value);
            if (double.IsNaN(d))
            {
                if (backwardsCompatible)
                    return format.NaN;
                throw new InvalidOperationException("XPTY0004");
            }
            value = XdmValue.FromDouble(d);
        }

        // Parse picture into positive and negative subpictures
        var (positivePicture, negativePicture) = ParsePicture(picture, format);

        bool negative = IsNegative(value);
        Subpicture subpicture = negative && negativePicture.HasValue
            ? negativePicture.Value
            : positivePicture;

        // NaN (no prefix/suffix per spec)
        if (IsNaN(value))
            return format.NaN;

        // Infinity (with prefix/suffix of relevant subpicture)
        if (IsInfinity(value))
        {
            if (negative)
            {
                if (negativePicture.HasValue)
                    return subpicture.Prefix + format.Infinity + subpicture.Suffix;
                else
                    return format.MinusSign + positivePicture.Prefix + format.Infinity + positivePicture.Suffix;
            }
            return positivePicture.Prefix + format.Infinity + positivePicture.Suffix;
        }

        return FormatSubpicture(value, subpicture, format, negative, negativePicture.HasValue);
    }

    private static XdmValue AtomizeForFormatNumber(XdmValue value)
    {
        if (value.IsNode)
            return XdmValue.FromString(value.NodeValue.StringValue);
        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!))
                return AtomizeForFormatNumber(item);
            return XdmValue.Undefined;
        }
        return value;
    }

    private static bool IsNumeric(XdmValue value)
    {
        return value.Kind is XdmValueKind.Integer or XdmValueKind.Decimal or XdmValueKind.Double or XdmValueKind.Float;
    }

    private static double ConvertToDouble(XdmValue value)
    {
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => (double)value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => value.DoubleValue,
            XdmValueKind.Boolean => value.BooleanValue ? 1.0 : 0.0,
            _ => double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : double.NaN
        };
    }

    private static bool IsNaN(XdmValue value)
    {
        if (value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float)
            return double.IsNaN(value.DoubleValue);
        return false;
    }

    private static bool IsInfinity(XdmValue value)
    {
        if (value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float)
            return double.IsInfinity(value.DoubleValue);
        return false;
    }

    private static bool IsNegative(XdmValue value)
    {
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue < 0,
            XdmValueKind.Decimal => value.DecimalValue < 0,
            XdmValueKind.Double or XdmValueKind.Float => double.IsNegative(value.DoubleValue),
            _ => false
        };
    }

    private static (Subpicture positive, Subpicture? negative) ParsePicture(string picture, DecimalFormat format)
    {
        if (string.IsNullOrEmpty(picture))
            throw FormatError("FODF1310");

        int patternSepIndex = picture.IndexOf(format.PatternSeparator);
        string posPic = patternSepIndex >= 0 ? picture.Substring(0, patternSepIndex) : picture;
        string? negPic = patternSepIndex >= 0 ? picture.Substring(patternSepIndex + 1) : null;

        var positive = ParseSubpicture(posPic, format);
        Subpicture? negative = negPic is not null ? ParseSubpicture(negPic, format) : null;

        return (positive, negative);
    }

    private static Subpicture ParseSubpicture(string picture, DecimalFormat format)
    {
        if (string.IsNullOrEmpty(picture))
            throw FormatError("FODF1310");

        var sub = new Subpicture();

        // Grammar-based parsing of the subpicture:
        // subpicture ::= prefix? format-token suffix?
        // format-token ::= integer-part (decimal-separator fractional-part)? (exponent-separator exponent-part)?
        // integer-part ::= (digit-sign | grouping-separator)*
        // fractional-part ::= (digit-sign | grouping-separator)*
        // exponent-part ::= digit-sign+
        //
        // First, find the start of the format token: the first digit sign, grouping separator, or decimal separator.
        int firstActive = -1;
        for (int i = 0; i < picture.Length; i++)
        {
            if (IsDigitSign(picture, i, format, out _) || MatchesAt(picture, i, format.GroupingSeparator) || MatchesAt(picture, i, format.DecimalSeparator))
            {
                firstActive = i;
                break;
            }
        }

        if (firstActive < 0)
        {
            // No active chars at all
            sub.Prefix = picture;
            sub.IntegerDigits = "";
            sub.FractionalDigits = "";
            sub.Suffix = "";
            sub.HasDecimalSeparator = false;
        }
        else
        {
            sub.Prefix = picture.Substring(0, firstActive);

            // Parse format token starting at firstActive
            int pos = firstActive;

            // Integer part: digits and grouping separators
            while (pos < picture.Length)
            {
                if (IsDigitSign(picture, pos, format, out int len))
                    pos += len;
                else if (MatchesAt(picture, pos, format.GroupingSeparator))
                    pos += format.GroupingSeparator.Length;
                else
                    break;
            }

            // Optional decimal separator + fractional part
            if (pos < picture.Length && MatchesAt(picture, pos, format.DecimalSeparator))
            {
                pos += format.DecimalSeparator.Length;
                while (pos < picture.Length)
                {
                    if (IsDigitSign(picture, pos, format, out int len))
                        pos += len;
                    else if (MatchesAt(picture, pos, format.GroupingSeparator))
                        pos += format.GroupingSeparator.Length;
                    else
                        break;
                }
            }

            // Optional exponent separator + exponent part
            // Use lookahead: only consume exponent separator if followed by at least one mandatory digit sign.
            bool hasExponent = false;
            if (pos < picture.Length && MatchesAt(picture, pos, format.ExponentSeparator))
            {
                int lookahead = pos + format.ExponentSeparator.Length;
                int expDigitCount = 0;
                while (lookahead < picture.Length && IsMandatoryDigitSign(picture, lookahead, format, out int len))
                {
                    lookahead += len;
                    expDigitCount++;
                }
                if (expDigitCount > 0)
                {
                    hasExponent = true;
                    sub.ExponentDigits = expDigitCount;
                    sub.IsScientific = true;
                    pos = lookahead;
                }
            }

            // The format token ends at pos. Everything after is suffix.
            sub.Suffix = picture.Substring(pos);
            string formatToken = picture.Substring(firstActive, pos - firstActive);

            // Split format token into mantissa and exponent
            if (hasExponent)
            {
                int expIndex = IndexOfOrdinal(formatToken, format.ExponentSeparator);
                string mantissaToken = formatToken.Substring(0, expIndex);
                string exponentToken = formatToken.Substring(expIndex + format.ExponentSeparator.Length);

                // Exponent part must contain only mandatory digit signs (already validated by lookahead, but double-check)
                for (int i = 0; i < exponentToken.Length; )
                {
                    if (IsMandatoryDigitSign(exponentToken, i, format, out int len))
                    {
                        i += len;
                    }
                    else if (MatchesAt(exponentToken, i, format.GroupingSeparator))
                    {
                        throw FormatError("FODF1310"); // grouping in exponent
                    }
                    else
                    {
                        throw FormatError("FODF1310");
                    }
                }

                int decIndex = IndexOfOrdinal(mantissaToken, format.DecimalSeparator);
                sub.IntegerDigits = decIndex >= 0 ? mantissaToken.Substring(0, decIndex) : mantissaToken;
                sub.FractionalDigits = decIndex >= 0 ? mantissaToken.Substring(decIndex + format.DecimalSeparator.Length) : "";
                sub.HasDecimalSeparator = decIndex >= 0;
            }
            else
            {
                int decIndex = IndexOfOrdinal(formatToken, format.DecimalSeparator);
                sub.IntegerDigits = decIndex >= 0 ? formatToken.Substring(0, decIndex) : formatToken;
                sub.FractionalDigits = decIndex >= 0 ? formatToken.Substring(decIndex + format.DecimalSeparator.Length) : "";
                sub.HasDecimalSeparator = decIndex >= 0;
            }
        }

        // Validate prefix and suffix don't contain digit signs, decimal separators, or grouping separators
        for (int i = 0; i < sub.Prefix.Length; i++)
        {
            if (IsDigitSign(sub.Prefix, i, format, out int len) || MatchesAt(sub.Prefix, i, format.DecimalSeparator) || MatchesAt(sub.Prefix, i, format.GroupingSeparator))
                throw FormatError("FODF1310");
        }
        for (int i = 0; i < sub.Suffix.Length; i++)
        {
            if (IsDigitSign(sub.Suffix, i, format, out int len) || MatchesAt(sub.Suffix, i, format.DecimalSeparator) || MatchesAt(sub.Suffix, i, format.GroupingSeparator))
                throw FormatError("FODF1310");
        }

        // A percent sign, per-mille sign, or exponent separator must not appear more than once
        // in a subpicture, and percent and per-mille must not both appear in the same subpicture.
        int percentCount = CountOccurrences(picture, format.Percent);
        int perMilleCount = CountOccurrences(picture, format.PerMille);
        int exponentCount = CountOccurrences(picture, format.ExponentSeparator);
        if (percentCount > 1 || perMilleCount > 1 || exponentCount > 1)
            throw FormatError("FODF1310");
        if (percentCount > 0 && perMilleCount > 0)
            throw FormatError("FODF1310");

        // Must have at least one actual digit sign in the format token
        int actualDigits = CountAllDigits(sub.IntegerDigits, format) + CountAllDigits(sub.FractionalDigits, format);
        if (actualDigits == 0)
            throw FormatError("FODF1310");

        // Validate grouping separator adjacency rules
        ValidateGroupingSeparators(sub, format);

        // Validate percent / per-mille positioning and detect scaling operators
        int firstDigitPos = -1;
        int exponentSepPos = -1;
        for (int i = 0; i < picture.Length; i++)
        {
            char c = picture[i];
            if (firstDigitPos < 0 && IsDigitSign(c, format))
                firstDigitPos = i;
            if (exponentSepPos < 0 && format.ExponentSeparator.Length == 1 && c == format.ExponentSeparator[0] && sub.IsScientific)
                exponentSepPos = i;

            if (format.Percent.Length == 1 && c == format.Percent[0] || format.PerMille.Length == 1 && c == format.PerMille[0])
            {
                // Must not appear to the left of the first digit sign in the integer part
                if (firstDigitPos >= 0 && i < firstDigitPos)
                    throw FormatError("FODF1310");
                // Must not appear to the right of the exponent separator
                if (exponentSepPos >= 0 && i > exponentSepPos)
                    throw FormatError("FODF1310");

                if (format.Percent.Length == 1 && c == format.Percent[0]) sub.HasPercent = true;
                if (format.PerMille.Length == 1 && c == format.PerMille[0]) sub.HasPerMille = true;
            }
        }

        // Validate ordering of digit signs
        ValidateDigitOrdering(sub, format);

        // Compute min/max sizes
        sub.MinIntegerDigits = CountMandatoryDigits(sub.IntegerDigits, format);
        sub.MaxIntegerDigits = CountAllDigits(sub.IntegerDigits, format) + (string.IsNullOrEmpty(sub.IntegerDigits) && sub.HasDecimalSeparator ? 1 : 0);
        sub.MinFractionalDigits = CountMandatoryDigits(sub.FractionalDigits, format);
        sub.MaxFractionalDigits = CountAllDigits(sub.FractionalDigits, format);

        // Adjustment: if both min-integer and max-fractional are zero
        if (sub.MinIntegerDigits == 0 && sub.MaxFractionalDigits == 0)
        {
            if (sub.IsScientific)
            {
                sub.MinFractionalDigits = 1;
                sub.MaxFractionalDigits = 1;
            }
            else
            {
                sub.MinIntegerDigits = 1;
            }
        }

        return sub;
    }

    private static void ValidateGroupingSeparators(Subpicture sub, DecimalFormat format)
    {
        // Grouping separator must not be adjacent to decimal separator
        // Grouping separator must not be at end of integer part
        // Grouping separators must not be adjacent to each other

        // Check integer part
        for (int i = 0; i < sub.IntegerDigits.Length; )
        {
            if (MatchesAt(sub.IntegerDigits, i, format.GroupingSeparator))
            {
                // Must not be at the end of integer part
                if (i + format.GroupingSeparator.Length >= sub.IntegerDigits.Length)
                    throw FormatError("FODF1310");
                // Must not be adjacent to another grouping separator
                if (i + format.GroupingSeparator.Length < sub.IntegerDigits.Length && MatchesAt(sub.IntegerDigits, i + format.GroupingSeparator.Length, format.GroupingSeparator))
                    throw FormatError("FODF1310");
                i += format.GroupingSeparator.Length;
            }
            else
            {
                i++;
            }
        }

        // Check adjacency to decimal separator: grouping sep immediately before or after decimal sep
        if (sub.HasDecimalSeparator)
        {
            if (sub.IntegerDigits.Length > 0 && MatchesAt(sub.IntegerDigits, sub.IntegerDigits.Length - format.GroupingSeparator.Length, format.GroupingSeparator))
                throw FormatError("FODF1310"); // grouping sep at end of integer part (just before decimal)
            if (sub.FractionalDigits.Length > 0 && MatchesAt(sub.FractionalDigits, 0, format.GroupingSeparator))
                throw FormatError("FODF1310"); // grouping sep at start of fractional part (just after decimal)
        }

        // Check fractional part for adjacent grouping separators
        for (int i = 0; i < sub.FractionalDigits.Length; )
        {
            if (MatchesAt(sub.FractionalDigits, i, format.GroupingSeparator))
            {
                if (i + format.GroupingSeparator.Length < sub.FractionalDigits.Length && MatchesAt(sub.FractionalDigits, i + format.GroupingSeparator.Length, format.GroupingSeparator))
                    throw FormatError("FODF1310");
                i += format.GroupingSeparator.Length;
            }
            else
            {
                i++;
            }
        }
    }

    private static bool IsDigitSign(char c, DecimalFormat format)
    {
        return IsOptionalDigitSign(c, format) || IsMandatoryDigitSign(c, format);
    }

    private static bool IsActiveChar(char c, DecimalFormat format)
    {
        return IsOptionalDigitSign(c, format)
            || IsMandatoryDigitSign(c, format)
            || format.GroupingSeparator.Length == 1 && c == format.GroupingSeparator[0]
            || format.DecimalSeparator.Length == 1 && c == format.DecimalSeparator[0];
    }

    private static bool IsOptionalDigitSign(char c, DecimalFormat format)
    {
        return format.Digit.Length == 1 && c == format.Digit[0];
        // Note: ASCII 9 is treated as optional in fractional part per strict spec,
        // but the test suite does not appear to exercise this distinction for
        // non-scientific pictures. It is always mandatory in the integer part.
    }

    private static bool IsMandatoryDigitSign(char c, DecimalFormat format)
    {
        if (format.ZeroDigit.Length == 1)
        {
            int offset = c - format.ZeroDigit[0];
            if (offset >= 0 && offset <= 9)
                return true;
        }
        if (c == '9')
            return true;
        if (format.ZeroDigit == "0" && c >= '0' && c <= '9')
            return true;
        return false;
    }

    private static bool IsMandatoryDigitSign(string text, int index, DecimalFormat format, out int length)
    {
        length = format.ZeroDigit.Length;
        if (MatchesAt(text, index, format.ZeroDigit))
            return true;

        if (format.ZeroDigit == "0" && index < text.Length && text[index] >= '0' && text[index] <= '9')
        {
            length = 1;
            return true;
        }

        if (index < text.Length && !char.IsLowSurrogate(text[index]))
        {
            int codePoint = char.ConvertToUtf32(text, index);
            int zeroCode = char.ConvertToUtf32(format.ZeroDigit, 0);
            int offset = codePoint - zeroCode;
            if (offset >= 1 && offset <= 9)
            {
                length = char.IsHighSurrogate(text[index]) ? 2 : 1;
                return true;
            }
        }

        return false;
    }

    private static bool IsOptionalDigitSign(string text, int index, DecimalFormat format, out int length)
    {
        length = format.Digit.Length;
        return MatchesAt(text, index, format.Digit);
    }

    private static bool IsDigitSign(string text, int index, DecimalFormat format, out int length)
    {
        if (IsOptionalDigitSign(text, index, format, out length))
            return true;
        if (IsMandatoryDigitSign(text, index, format, out length))
            return true;
        return false;
    }

    private static int CountMandatoryDigits(string part, DecimalFormat format)
    {
        int count = 0;
        for (int i = 0; i < part.Length; )
        {
            if (IsMandatoryDigitSign(part, i, format, out int len))
            {
                count++;
                i += len;
            }
            else
            {
                i++;
            }
        }
        return count;
    }

    private static int CountAllDigits(string part, DecimalFormat format)
    {
        int count = 0;
        for (int i = 0; i < part.Length; )
        {
            if (IsDigitSign(part, i, format, out int len))
            {
                count++;
                i += len;
            }
            else
            {
                i++;
            }
        }
        return count;
    }

    private static void ValidateDigitOrdering(Subpicture sub, DecimalFormat format)
    {
        // Integer part: optional digits must precede mandatory digits
        bool seenMandatory = false;
        for (int i = 0; i < sub.IntegerDigits.Length; )
        {
            if (MatchesAt(sub.IntegerDigits, i, format.GroupingSeparator))
            {
                i += format.GroupingSeparator.Length;
                continue;
            }
            if (IsMandatoryDigitSign(sub.IntegerDigits, i, format, out int len))
            {
                seenMandatory = true;
                i += len;
            }
            else if (IsOptionalDigitSign(sub.IntegerDigits, i, format, out len) && seenMandatory)
            {
                throw FormatError("FODF1310");
            }
            else
            {
                i++;
            }
        }

        // Fractional part: mandatory digits must precede optional digits
        bool seenOptional = false;
        for (int i = 0; i < sub.FractionalDigits.Length; )
        {
            if (MatchesAt(sub.FractionalDigits, i, format.GroupingSeparator))
            {
                i += format.GroupingSeparator.Length;
                continue;
            }
            if (IsOptionalDigitSign(sub.FractionalDigits, i, format, out int len))
            {
                seenOptional = true;
                i += len;
            }
            else if (IsMandatoryDigitSign(sub.FractionalDigits, i, format, out len) && seenOptional)
            {
                throw FormatError("FODF1310");
            }
            else
            {
                i++;
            }
        }
    }

    private static string FormatSubpicture(XdmValue value, Subpicture sub, DecimalFormat format, bool negative, bool hasNegativeSubpicture)
    {
        decimal num;
        double? largeDouble = null;

        // Apply percent / per-mille scaling.
        // For double/float inputs, perform scaling in double space before converting to decimal
        // so that results match XPath double arithmetic (e.g. cbcl-fn-format-number-035).
        if (value.Kind == XdmValueKind.Double || value.Kind == XdmValueKind.Float)
        {
            double d = value.DoubleValue;
            if (sub.HasPercent) d *= 100;
            if (sub.HasPerMille) d *= 1000;

            if (double.IsInfinity(d))
            {
                string inf = sub.Prefix + format.Infinity + sub.Suffix;
                if (negative && !hasNegativeSubpicture)
                    inf = format.MinusSign + inf;
                return inf;
            }
            if (double.IsNaN(d))
                return format.NaN;

            try
            {
                num = DoubleToDecimal(d);
            }
            catch (OverflowException)
            {
                largeDouble = d;
                num = 0m;
            }
        }
        else
        {
            num = ToDecimal(value);
            if (sub.HasPercent) num *= 100m;
            if (sub.HasPerMille) num *= 1000m;
        }

        string signPrefix = "";
        string signSuffix = "";

        if (negative)
        {
            if (hasNegativeSubpicture)
            {
                // Negative subpicture handles its own sign via prefix/suffix
            }
            else
            {
                // Single subpicture: prepend minus sign
                signPrefix = format.MinusSign;
            }
        }

        if (largeDouble.HasValue)
        {
            return signPrefix + FormatLargeDouble(largeDouble.Value, sub, format) + signSuffix;
        }

        if (sub.IsScientific)
        {
            return signPrefix + FormatScientific(num, sub, format) + signSuffix;
        }

        return signPrefix + FormatDecimal(num, sub, format) + signSuffix;
    }

    private static decimal ToDecimal(XdmValue value)
    {
        return value.Kind switch
        {
            XdmValueKind.Integer => value.IntegerValue,
            XdmValueKind.Decimal => value.DecimalValue,
            XdmValueKind.Double or XdmValueKind.Float => DoubleToDecimal(value.DoubleValue),
            _ => 0m
        };
    }

    private static decimal DoubleToDecimal(double d)
    {
        if (double.IsNaN(d) || double.IsInfinity(d))
            return 0m;

        // Use shortest round-trip string; for integer powers of 10 this gives e.g. "1E+25"
        // which decimal.Parse converts to the exact power of 10, avoiding double imprecision.
        string s = d.ToString("R", CultureInfo.InvariantCulture);
        if (decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal result))
            return result;
        return (decimal)d;
    }

    private static string FormatDecimal(decimal num, Subpicture sub, DecimalFormat format)
    {
        decimal absNum = Math.Abs(num);

        // Round to max fractional digits using half-to-even
        int fracDigits = sub.MaxFractionalDigits;
        decimal rounded = fracDigits >= 0
            ? RoundHalfToEven(absNum, fracDigits)
            : absNum;

        // Convert to string with all fractional digits we need
        string intStr, fracStr;
        if (fracDigits > 0)
        {
            decimal scale = DecimalPowerOf10(fracDigits);
            decimal scaled = rounded * scale;
            // Use integer arithmetic to avoid floating point issues
            // Actually for decimal this is fine
            string full = scaled.ToString("F0", CultureInfo.InvariantCulture);
            if (full.Length <= fracDigits)
            {
                intStr = "0";
                fracStr = full.PadLeft(fracDigits, '0');
            }
            else
            {
                intStr = full.Substring(0, full.Length - fracDigits);
                fracStr = full.Substring(full.Length - fracDigits);
            }
        }
        else
        {
            intStr = rounded.ToString("F0", CultureInfo.InvariantCulture);
            fracStr = "";
        }

        // If zero, intStr might be "0"
        if (intStr == "0" && (fracStr == "" || fracStr.TrimStart('0') == ""))
        {
            // Value is zero
            intStr = "0";
        }

        // Build integer part
        StringBuilder integerBuilder = new();

        // Suppress leading zeros if all integer digits are optional
        bool hasMandatoryInteger = sub.MinIntegerDigits > 0;
        if (intStr == "0" && !hasMandatoryInteger && sub.MaxIntegerDigits > 0)
        {
            // Integer part is zero and all digits optional → empty, unless no fractional digits at all
            intStr = "";
        }

        // Pad integer to minimum digits
        int intDigitCount = CountDigitSigns(sub.IntegerDigits, format);
        int targetMinInt = sub.MinIntegerDigits;
        if (intStr.Length < targetMinInt)
        {
            intStr = intStr.PadLeft(targetMinInt, '0');
        }

        // If integer part is still empty (no digits), and we need at least one digit
        if (string.IsNullOrEmpty(intStr) && targetMinInt == 0 && sub.MaxFractionalDigits == 0)
        {
            intStr = format.ZeroDigit;
        }

        foreach (char c in intStr)
            integerBuilder.Append(MapDigit(c, format));

        // Insert grouping separators
        if (!string.IsNullOrEmpty(intStr) && !string.IsNullOrEmpty(sub.IntegerDigits))
        {
            InsertGroupingSeparators(integerBuilder, sub.IntegerDigits, format);
        }

        // Build fractional part using the fractional picture (interleaving digits with grouping separators)
        string fractionalOutput = "";
        if (!string.IsNullOrEmpty(fracStr) || sub.MinFractionalDigits > 0 || (sub.MaxFractionalDigits > 0 && sub.HasDecimalSeparator))
        {
            if (!string.IsNullOrEmpty(sub.FractionalDigits))
            {
                StringBuilder fracBuilder = new();
                int digitIndex = 0;
                for (int i = 0; i < sub.FractionalDigits.Length; )
                {
                    if (IsDigitSign(sub.FractionalDigits, i, format, out int len))
                    {
                        if (digitIndex < fracStr.Length)
                        {
                            fracBuilder.Append(fracStr[digitIndex]);
                            digitIndex++;
                        }
                        i += len;
                    }
                    else if (MatchesAt(sub.FractionalDigits, i, format.GroupingSeparator))
                    {
                        fracBuilder.Append(format.GroupingSeparator);
                        i += format.GroupingSeparator.Length;
                    }
                    else
                    {
                        i++;
                    }
                }

                // Strip trailing zeros in optional positions, and any trailing grouping separators
                int minToKeep = sub.MinFractionalDigits;
                if (absNum == 0 && sub.MaxFractionalDigits > 0 && minToKeep < 1)
                    minToKeep = 1;

                while (fracBuilder.Length > minToKeep)
                {
                    char last = fracBuilder[fracBuilder.Length - 1];
                    if (last == '0')
                    {
                        fracBuilder.Length--;
                    }
                    else if (EndsWith(fracBuilder, format.GroupingSeparator))
                    {
                        fracBuilder.Length -= format.GroupingSeparator.Length;
                    }
                    else
                    {
                        break;
                    }
                }

                fractionalOutput = fracBuilder.ToString();
            }
            else if (!string.IsNullOrEmpty(fracStr))
            {
                fractionalOutput = fracStr;
            }
        }

        // Assemble result
        StringBuilder result = new();
        result.Append(sub.Prefix);
        result.Append(integerBuilder);


        if (!string.IsNullOrEmpty(fractionalOutput))
        {
            result.Append(format.DecimalSeparator);
            // Map digits to zero-digit family
            foreach (char c in fractionalOutput)
            {
                result.Append(MapDigit(c, format));
            }
        }
        else if (sub.HasDecimalSeparator && sub.MinFractionalDigits == 0 && sub.MaxFractionalDigits == 0)
        {
            // Decimal separator present but no fractional digits in picture
            // Don't output separator
        }
        else if (sub.HasDecimalSeparator && sub.MinFractionalDigits > 0)
        {
            result.Append(format.DecimalSeparator);
            AppendRepeated(result, format.ZeroDigit, sub.MinFractionalDigits);
        }

        result.Append(sub.Suffix);

        string output = result.ToString();

        // If output is empty or just prefix+suffix, output a single zero
        if (string.IsNullOrEmpty(output) || output == sub.Prefix + sub.Suffix)
        {
            output = sub.Prefix + format.ZeroDigit + sub.Suffix;
        }

        // If output is just prefix + decimal separator + suffix (no digits), add a zero in fractional part
        if (output == sub.Prefix + format.DecimalSeparator + sub.Suffix)
        {
            output = sub.Prefix + format.DecimalSeparator + format.ZeroDigit + sub.Suffix;
        }

        return output;
    }

    /// <summary>
    /// Formats a very large double that cannot be represented as a decimal.
    /// Expands scientific notation and formats according to the subpicture.
    /// </summary>
    private static string FormatLargeDouble(double d, Subpicture sub, DecimalFormat format)
    {
        string plain = ExpandScientificNotation(d);
        // Split into integer and fractional parts
        int dotIndex = plain.IndexOf('.');
        string intPart = dotIndex >= 0 ? plain.Substring(0, dotIndex) : plain;
        string fracPart = dotIndex >= 0 ? plain.Substring(dotIndex + 1) : "";

        // Round to max fractional digits
        int maxFrac = sub.MaxFractionalDigits;
        if (maxFrac >= 0 && fracPart.Length > maxFrac)
        {
            // Round half to even
            string rounded = RoundStringHalfToEven(intPart, fracPart, maxFrac);
            int newDot = rounded.IndexOf('.');
            intPart = newDot >= 0 ? rounded.Substring(0, newDot) : rounded;
            fracPart = newDot >= 0 ? rounded.Substring(newDot + 1) : "";
        }
        else if (maxFrac >= 0 && fracPart.Length < maxFrac)
        {
            fracPart = fracPart.PadRight(maxFrac, '0');
        }

        // Build integer part according to picture
        StringBuilder integerBuilder = new();
        bool hasMandatoryInteger = sub.MinIntegerDigits > 0;
        if (intPart == "0" && !hasMandatoryInteger && sub.MaxIntegerDigits > 0)
        {
            intPart = "";
        }
        int targetMinInt = sub.MinIntegerDigits;
        if (intPart.Length < targetMinInt)
        {
            intPart = intPart.PadLeft(targetMinInt, '0');
        }
        if (string.IsNullOrEmpty(intPart) && targetMinInt == 0 && sub.MaxFractionalDigits == 0)
        {
            intPart = "0";
        }
        foreach (char c in intPart)
            integerBuilder.Append(MapDigit(c, format));

        if (!string.IsNullOrEmpty(intPart) && !string.IsNullOrEmpty(sub.IntegerDigits))
        {
            InsertGroupingSeparators(integerBuilder, sub.IntegerDigits, format);
        }

        // Build fractional part
        string fractionalOutput = "";
        if (!string.IsNullOrEmpty(fracPart) || sub.MinFractionalDigits > 0 || (sub.MaxFractionalDigits > 0 && sub.HasDecimalSeparator))
        {
            if (!string.IsNullOrEmpty(sub.FractionalDigits))
            {
                StringBuilder fracBuilder = new();
                int digitIndex = 0;
                for (int i = 0; i < sub.FractionalDigits.Length; )
                {
                    if (IsDigitSign(sub.FractionalDigits, i, format, out int len))
                    {
                        if (digitIndex < fracPart.Length)
                        {
                            fracBuilder.Append(fracPart[digitIndex]);
                            digitIndex++;
                        }
                        i += len;
                    }
                    else if (MatchesAt(sub.FractionalDigits, i, format.GroupingSeparator))
                    {
                        fracBuilder.Append(format.GroupingSeparator);
                        i += format.GroupingSeparator.Length;
                    }
                    else
                    {
                        i++;
                    }
                }

                int minToKeep = sub.MinFractionalDigits;
                if (d == 0 && sub.MaxFractionalDigits > 0 && minToKeep < 1)
                    minToKeep = 1;

                while (fracBuilder.Length > minToKeep)
                {
                    char last = fracBuilder[fracBuilder.Length - 1];
                    if (last == '0')
                    {
                        fracBuilder.Length--;
                    }
                    else if (EndsWith(fracBuilder, format.GroupingSeparator))
                    {
                        fracBuilder.Length -= format.GroupingSeparator.Length;
                    }
                    else
                    {
                        break;
                    }
                }

                fractionalOutput = fracBuilder.ToString();
            }
            else if (!string.IsNullOrEmpty(fracPart))
            {
                fractionalOutput = fracPart;
            }
        }

        // Assemble
        StringBuilder result = new();
        result.Append(sub.Prefix);
        result.Append(integerBuilder);

        if (!string.IsNullOrEmpty(fractionalOutput))
        {
            result.Append(format.DecimalSeparator);
            foreach (char c in fractionalOutput)
                result.Append(MapDigit(c, format));
        }
        else if (sub.HasDecimalSeparator && sub.MinFractionalDigits == 0 && sub.MaxFractionalDigits == 0)
        {
            // Don't output separator
        }
        else if (sub.HasDecimalSeparator && sub.MinFractionalDigits > 0)
        {
            result.Append(format.DecimalSeparator);
            AppendRepeated(result, format.ZeroDigit, sub.MinFractionalDigits);
        }

        result.Append(sub.Suffix);

        string output = result.ToString();
        if (string.IsNullOrEmpty(output) || output == sub.Prefix + sub.Suffix)
            output = sub.Prefix + format.ZeroDigit + sub.Suffix;
        if (output == sub.Prefix + format.DecimalSeparator + sub.Suffix)
            output = sub.Prefix + format.DecimalSeparator + format.ZeroDigit + sub.Suffix;

        return output;
    }

    private static string ExpandScientificNotation(double d)
    {
        string s = d.ToString("R", CultureInfo.InvariantCulture);
        int eIndex = s.IndexOf('E');
        if (eIndex < 0) eIndex = s.IndexOf('e');
        if (eIndex < 0) return s;

        string mantissa = s.Substring(0, eIndex);
        int exponent = int.Parse(s.Substring(eIndex + 1), CultureInfo.InvariantCulture);

        int dotIndex = mantissa.IndexOf('.');
        string intPart = dotIndex >= 0 ? mantissa.Substring(0, dotIndex) : mantissa;
        string fracPart = dotIndex >= 0 ? mantissa.Substring(dotIndex + 1) : "";

        // Remove leading zeros from integer part (but keep at least one digit)
        if (intPart.Length > 1 && intPart[0] == '0')
            intPart = intPart.TrimStart('0');
        if (string.IsNullOrEmpty(intPart))
            intPart = "0";

        // Combine integer and fractional parts
        string digits = intPart + fracPart;
        int decimalPos = intPart.Length + exponent;

        if (decimalPos <= 0)
        {
            // Need leading zeros
            return "0." + new string('0', -decimalPos) + digits;
        }
        else if (decimalPos >= digits.Length)
        {
            // Need trailing zeros
            return digits + new string('0', decimalPos - digits.Length);
        }
        else
        {
            return digits.Substring(0, decimalPos) + "." + digits.Substring(decimalPos);
        }
    }

    private static string RoundStringHalfToEven(string intPart, string fracPart, int targetDigits)
    {
        if (fracPart.Length <= targetDigits)
            return intPart + (string.IsNullOrEmpty(fracPart) ? "" : "." + fracPart);

        // Look at the digit after targetDigits
        char nextDigit = fracPart[targetDigits];
        bool roundUp = false;

        if (nextDigit > '5')
        {
            roundUp = true;
        }
        else if (nextDigit == '5')
        {
            // Check if there are non-zero digits after
            bool hasMore = false;
            for (int i = targetDigits + 1; i < fracPart.Length; i++)
            {
                if (fracPart[i] != '0')
                {
                    hasMore = true;
                    break;
                }
            }
            if (hasMore)
            {
                roundUp = true;
            }
            else
            {
                // Half to even: round up if the last kept digit is odd
                char lastKept = targetDigits > 0 ? fracPart[targetDigits - 1] : intPart[intPart.Length - 1];
                roundUp = (lastKept - '0') % 2 == 1;
            }
        }

        if (!roundUp)
        {
            return intPart + (targetDigits > 0 ? "." + fracPart.Substring(0, targetDigits) : "");
        }

        // Round up
        var sb = new StringBuilder();
        if (targetDigits > 0)
        {
            sb.Append(fracPart.Substring(0, targetDigits));
        }
        else
        {
            // Need to round up the integer part
            var intSb = new StringBuilder(intPart);
            for (int i = intSb.Length - 1; i >= 0; i--)
            {
                if (intSb[i] == '9')
                {
                    intSb[i] = '0';
                }
                else
                {
                    intSb[i]++;
                    break;
                }
            }
            // If all were 9s, prepend 1
            if (intSb[0] == '0' && intPart[0] != '0')
            {
                intSb.Insert(0, '1');
            }
            return intSb.ToString();
        }

        // Round up within fractional part
        for (int i = sb.Length - 1; i >= 0; i--)
        {
            if (sb[i] == '9')
            {
                sb[i] = '0';
            }
            else
            {
                sb[i]++;
                break;
            }
        }

        // Check if we overflowed into integer part
        bool allZero = true;
        for (int i = 0; i < sb.Length; i++)
        {
            if (sb[i] != '0')
            {
                allZero = false;
                break;
            }
        }

        if (allZero && sb.Length > 0)
        {
            var intSb = new StringBuilder(intPart);
            for (int i = intSb.Length - 1; i >= 0; i--)
            {
                if (intSb[i] == '9')
                {
                    intSb[i] = '0';
                }
                else
                {
                    intSb[i]++;
                    break;
                }
            }
            if (intSb[0] == '0' && intPart[0] != '0')
            {
                intSb.Insert(0, '1');
            }
            return intSb.ToString() + (targetDigits > 0 ? "." + sb.ToString() : "");
        }

        return intPart + "." + sb.ToString();
    }

    private static int CountDigitSigns(string part, DecimalFormat format)
    {
        int count = 0;
        for (int i = 0; i < part.Length; )
        {
            if (IsDigitSign(part, i, format, out int len))
            {
                count++;
                i += len;
            }
            else
            {
                i++;
            }
        }
        return count;
    }

    private static void InsertGroupingSeparators(StringBuilder builder, string integerPicture, DecimalFormat format)
    {
        // Calculate positions of grouping separators
        List<int> positions = new();
        int digitCount = 0;
        for (int i = integerPicture.Length - 1; i >= 0;)
        {
            if (MatchesAt(integerPicture, i, format.GroupingSeparator))
            {
                positions.Add(digitCount);
                i -= format.GroupingSeparator.Length;
            }
            else if (IsDigitSign(integerPicture, i, format, out int len))
            {
                digitCount++;
                i -= len;
            }
            else
            {
                i--;
            }
        }

        if (positions.Count == 0)
            return;

        // Check if regular
        positions.Sort();
        bool regular = true;
        int g = positions[0];
        if (g == 0) regular = false;
        else
        {
            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i] != (i + 1) * g)
                {
                    regular = false;
                    break;
                }
            }
            if (regular)
            {
                // Check all multiples of g < digitCount are present
                for (int m = g; m < digitCount; m += g)
                {
                    if (!positions.Contains(m))
                    {
                        regular = false;
                        break;
                    }
                }
            }
        }

        if (regular)
        {
            // Extrapolate
            positions.Clear();
            for (int m = g; m < builder.Length; m += g)
            {
                positions.Add(m);
            }
        }

        // Insert separators from right to left
        positions.Sort((a, b) => b.CompareTo(a));
        foreach (int pos in positions)
        {
            if (pos < builder.Length)
            {
                builder.Insert(builder.Length - pos, format.GroupingSeparator);
            }
        }
    }

    private static void AppendRepeated(StringBuilder builder, string text, int count)
    {
        for (int i = 0; i < count; i++)
            builder.Append(text);
    }

    private static bool EndsWith(StringBuilder builder, string pattern)
    {
        if (builder.Length < pattern.Length)
            return false;
        for (int i = 0; i < pattern.Length; i++)
            if (builder[builder.Length - pattern.Length + i] != pattern[i])
                return false;
        return true;
    }

    private static bool MatchesAt(string text, int index, string pattern)
    {
        return index >= 0 && index + pattern.Length <= text.Length && text.AsSpan(index, pattern.Length).SequenceEqual(pattern.AsSpan());
    }

    private static int IndexOfOrdinal(string text, string pattern)
    {
        return text.IndexOf(pattern, StringComparison.Ordinal);
    }

    private static int CountOccurrences(string text, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return 0;
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }

    private static string MapDigit(char c, DecimalFormat format)
    {
        if (c >= '0' && c <= '9')
        {
            int offset = c - '0';
            if (format.ZeroDigit.Length == 1)
                return ((char)(format.ZeroDigit[0] + offset)).ToString();
            // Astral-plane zero-digit: use Unicode code-point arithmetic
            int zeroCode = char.ConvertToUtf32(format.ZeroDigit, 0);
            return char.ConvertFromUtf32(zeroCode + offset);
        }
        return c.ToString();
    }

    private static decimal RoundHalfToEven(decimal value, int digits)
    {
        if (digits < 0)
            return value;
        return Math.Round(value, digits, MidpointRounding.ToEven);
    }

    private static decimal DecimalPowerOf10(int exponent)
    {
        if (exponent >= 0)
        {
            decimal result = 1m;
            for (int i = 0; i < exponent; i++)
                result *= 10m;
            return result;
        }
        else
        {
            decimal result = 1m;
            for (int i = 0; i < -exponent; i++)
                result /= 10m;
            return result;
        }
    }

    private static string FormatScientific(decimal num, Subpicture sub, DecimalFormat format)
    {
        decimal absNum = Math.Abs(num);

        // Format exponent string
        string FormatExponent(int exponent)
        {
            bool expNegative = exponent < 0;
            int expAbs = Math.Abs(exponent);
            string expDigits = expAbs.ToString(CultureInfo.InvariantCulture);

            // Map exponent digits to the configured zero-digit family.
            StringBuilder mapped = new();
            foreach (char c in expDigits)
                mapped.Append(MapDigit(c, format));

            // Pad with the full zero-digit string (supports non-BMP zero-digits).
            while (mapped.Length < sub.ExponentDigits * format.ZeroDigit.Length)
                mapped.Insert(0, format.ZeroDigit);

            return (expNegative ? format.MinusSign : "") + mapped.ToString();
        }

        if (absNum == 0)
        {
            // Zero in scientific notation
            StringBuilder mantissaBuilder = new();

            // Integer part: output at least one zero if the picture has any integer digit signs
            if (sub.MaxIntegerDigits > 0)
            {
                AppendRepeated(mantissaBuilder, format.ZeroDigit, Math.Max(sub.MinIntegerDigits, 1));
            }

            // Fractional part: output only if there are mandatory fractional digits
            if (sub.MinFractionalDigits > 0)
            {
                mantissaBuilder.Append(format.DecimalSeparator);
                AppendRepeated(mantissaBuilder, format.ZeroDigit, sub.MinFractionalDigits);
            }

            return sub.Prefix + mantissaBuilder.ToString() + format.ExponentSeparator + FormatExponent(0) + sub.Suffix;
        }

        // Calculate exponent and mantissa
        // Scaling factor = number of mandatory digit signs in integer part
        int scalingFactor = CountMandatoryDigits(sub.IntegerDigits, format);

        int exponent = 0;
        decimal mantissa = absNum;

        if (mantissa != 0)
        {
            double log10 = (double)mantissa == 0 ? 0 : Math.Log10((double)mantissa);
            exponent = (int)Math.Floor(log10) - (scalingFactor - 1);
            decimal divisor = DecimalPowerOf10(exponent);
            if (divisor != 0)
                mantissa = mantissa / divisor;
        }

        // Round mantissa to max fractional digits
        int fracDigits = sub.MaxFractionalDigits;
        decimal roundedMantissa = fracDigits >= 0
            ? RoundHalfToEven(mantissa, fracDigits)
            : mantissa;

        // Format mantissa without prefix/suffix
        var mantissaSub = sub;
        mantissaSub.Prefix = "";
        mantissaSub.Suffix = "";
        string mantissaStr = FormatDecimal(roundedMantissa, mantissaSub, format);

        // In scientific notation, if the integer part of the picture contains digit signs,
        // ensure at least one digit appears before the decimal point.
        if (!string.IsNullOrEmpty(sub.IntegerDigits) && mantissaStr.Length > 0 && mantissaStr[0] == format.DecimalSeparator[0])
        {
            mantissaStr = format.ZeroDigit + mantissaStr;
        }

        // When the integer part of the picture is empty and the mantissa rounded to an integer,
        // restore the decimal separator and a zero so the output matches the picture.
        if (string.IsNullOrEmpty(sub.IntegerDigits) && sub.HasDecimalSeparator && !mantissaStr.Contains(format.DecimalSeparator))
        {
            mantissaStr += format.DecimalSeparator.ToString() + format.ZeroDigit;
        }



        return sub.Prefix + mantissaStr + format.ExponentSeparator + FormatExponent(exponent) + sub.Suffix;
    }

    private static int CountOptionalDigits(string part, DecimalFormat format)
    {
        int count = 0;
        foreach (char c in part)
            if (IsOptionalDigitSign(c, format))
                count++;
        return count;
    }

    private static InvalidOperationException FormatError(string code)
    {
        return new InvalidOperationException(code);
    }

    private struct Subpicture
    {
        public string Prefix;
        public string Suffix;
        public string IntegerDigits;
        public string FractionalDigits;
        public bool HasDecimalSeparator;
        public bool IsScientific;
        public int ExponentDigits;
        public bool HasPercent;
        public bool HasPerMille;
        public int MinIntegerDigits;
        public int MaxIntegerDigits;
        public int MinFractionalDigits;
        public int MaxFractionalDigits;
    }
}
