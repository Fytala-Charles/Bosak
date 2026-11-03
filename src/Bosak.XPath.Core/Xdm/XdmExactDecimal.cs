// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Represents an exact finite decimal with explicit supplied and output capacity limits.
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
using System.Globalization;
using System.Numerics;

namespace Bosak.XPath.Core.Xdm;

/// <summary>An exact finite decimal represented by a normalized coefficient and nonnegative scale.</summary>
/// <remarks>Representation only; host arithmetic, schema evidence and XdmValue transport require separate integration.</remarks>
public readonly struct XdmExactDecimal : IEquatable<XdmExactDecimal>
{
    /// <summary>Checks supplied limits before normalization; zero normalizes to scale zero.</summary>
    /// <param name="coefficient">Signed arbitrary precision coefficient already owned by the caller.</param>
    /// <param name="scale">Nonnegative power of ten dividing the coefficient.</param>
    /// <param name="maxCoefficientDigits">Positive bound on supplied coefficient digits, before normalization.</param>
    /// <param name="maxScale">Nonnegative bound on supplied scale.</param>
    /// <exception cref="ArgumentOutOfRangeException">A bound or scale is invalid.</exception>
    /// <exception cref="NotSupportedException">The supplied representation exceeds capacity.</exception>
    public XdmExactDecimal(BigInteger coefficient, int scale, int maxCoefficientDigits, int maxScale)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCoefficientDigits);
        ArgumentOutOfRangeException.ThrowIfNegative(maxScale);
        ArgumentOutOfRangeException.ThrowIfNegative(scale);
        if (scale > maxScale) throw new NotSupportedException("Supplied decimal scale exceeds capacity.");
        if (!ExactNumericCapacity.FitsCoefficient(coefficient, maxCoefficientDigits))
            throw new NotSupportedException("Supplied decimal coefficient exceeds digit capacity.");
        if (coefficient.IsZero) scale = 0;
        // Remove long zero suffixes in blocks; do not divide a large coefficient once per zero.
        while (scale >= 9)
        {
            var quotient = BigInteger.DivRem(coefficient, 1_000_000_000, out var remainder);
            if (!remainder.IsZero) break;
            coefficient = quotient; scale -= 9;
        }
        while (scale > 0)
        {
            var quotient = BigInteger.DivRem(coefficient, 10, out var remainder);
            if (!remainder.IsZero) break;
            coefficient = quotient; scale--;
        }
        Coefficient = coefficient; Scale = scale;
    }
    /// <summary>The normalized coefficient; default represents zero.</summary>
    public BigInteger Coefficient { get; }
    /// <summary>The normalized nonnegative scale.</summary>
    public int Scale { get; }
    /// <summary>Formats normalized invariant numeric text within the output capacity; not an XSD canonical serializer.</summary>
    /// <param name="maxCharacters">Positive output limit including sign and decimal point.</param>
    /// <returns>Invariant normalized numeric text.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The output limit is not positive.</exception>
    /// <exception cref="NotSupportedException">Output exceeds capacity.</exception>
    public string ToInvariantString(int maxCharacters)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCharacters);
        int sign = Coefficient.Sign < 0 ? 1 : 0;
        // Long arithmetic avoids scale+2 overflow. Refuse padding and coefficient conversion before allocation.
        if (Scale > 0 && (long)Scale + 2 + sign > maxCharacters)
            throw new NotSupportedException("Decimal output exceeds character capacity.");
        int digitCapacity = maxCharacters - sign - (Scale > 0 ? 1 : 0);
        if (digitCapacity <= 0 || !ExactNumericCapacity.FitsCoefficient(Coefficient, digitCapacity))
            throw new NotSupportedException("Decimal output exceeds character capacity.");
        var digits = BigInteger.Abs(Coefficient).ToString(CultureInfo.InvariantCulture);
        int length = sign + (Scale == 0 ? digits.Length : Math.Max(digits.Length + 1, Scale + 2));
        return string.Create(length, (digits, Scale, sign), static (output, state) =>
        {
            var (text, scale, prefix) = state;
            if (prefix != 0) output[0] = '-';
            var body = output[prefix..];
            if (scale == 0) text.AsSpan().CopyTo(body);
            else if (text.Length > scale)
            {
                int split = text.Length - scale;
                text.AsSpan(0, split).CopyTo(body); body[split] = '.';
                text.AsSpan(split).CopyTo(body[(split + 1)..]);
            }
            else
            {
                body[0] = '0'; body[1] = '.';
                body.Slice(2, scale - text.Length).Fill('0');
                text.AsSpan().CopyTo(body[(2 + scale - text.Length)..]);
            }
        });
    }
    /// <summary>Compares numeric normalized coefficient and scale.</summary>
    /// <param name="other">Other exact value.</param>
    /// <returns>Whether values are numerically equal.</returns>
    public bool Equals(XdmExactDecimal other) => Coefficient == other.Coefficient && Scale == other.Scale;
    /// <summary>Compares an exact decimal with an object.</summary>
    /// <param name="obj">Other value.</param>
    /// <returns>Whether the other value is an equal exact decimal.</returns>
    public override bool Equals(object? obj) => obj is XdmExactDecimal other && Equals(other);
    /// <summary>Hashes the normalized numeric value.</summary>
    /// <returns>A hash consistent with numeric equality.</returns>
    public override int GetHashCode() => HashCode.Combine(Coefficient, Scale);
}
