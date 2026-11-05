// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Parses decimal lexical input after checking supplied digit and scale quotas.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-11-2026     | Creation; exact-decimal capacity foundation (1.46.0).                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Numerics;

namespace Bosak.XPath.Core.Xdm;

/// <summary>Bounded decimal lexical ingress for later host and transport integration.</summary>
internal static class ExactDecimalLexical
{
    internal static XdmExactDecimal Parse(ReadOnlySpan<char> text, int maxDigits, int maxScale)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxDigits);
        ArgumentOutOfRangeException.ThrowIfNegative(maxScale);
        while (!text.IsEmpty && XmlWhitespace(text[0])) text = text[1..];
        while (!text.IsEmpty && XmlWhitespace(text[^1])) text = text[..^1];
        bool negative = !text.IsEmpty && text[0] == '-';
        if (!text.IsEmpty && text[0] is '+' or '-') text = text[1..];
        int digits = 0, scale = 0; bool point = false;
        // Validate and enforce supplied work quotas before constructing any coefficient.
        foreach (char c in text)
        {
            if (c == '.' && !point) { point = true; continue; }
            if (c is < '0' or > '9') throw new FormatException("Invalid decimal lexical representation.");
            if (++digits > maxDigits) throw new NotSupportedException("Supplied lexical digits exceed capacity.");
            if (point && ++scale > maxScale) throw new NotSupportedException("Supplied decimal scale exceeds capacity.");
        }
        if (digits == 0) throw new FormatException("A decimal requires at least one ASCII digit.");
        BigInteger coefficient = BigInteger.Zero; int chunk = 0, count = 0;
        foreach (char c in text)
        {
            if (c == '.') continue;
            chunk = chunk * 10 + c - '0';
            if (++count == 9) { coefficient = coefficient * 1_000_000_000 + chunk; chunk = 0; count = 0; }
        }
        if (count != 0)
        {
            int power = 1; for (int i = 0; i < count; i++) power *= 10;
            coefficient = coefficient * power + chunk;
        }
        return new XdmExactDecimal(negative ? -coefficient : coefficient, scale, maxDigits, maxScale);
    }
    private static bool XmlWhitespace(char c) => c is ' ' or '\t' or '\r' or '\n';
}
