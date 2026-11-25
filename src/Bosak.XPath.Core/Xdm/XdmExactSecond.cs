// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Represents bounded exact decimal seconds without lossy calendar conversion.
// SPECIAL NOTES        : Foundation types for the XQuery Data Model; used by all higher layers.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Globalization;
using System.Numerics;

namespace Bosak.XPath.Core.Xdm;

/// <summary>Represents an exact decimal second in [0,60), independently of calendar authority.</summary>
public readonly struct XdmExactSecond : IEquatable<XdmExactSecond>, IComparable<XdmExactSecond>
{
    private readonly XdmExactDecimal _value;
    /// <summary>Creates seconds after checking supplied capacities before normalization.</summary>
    /// <param name="coefficient">Nonnegative decimal coefficient.</param>
    /// <param name="scale">Supplied decimal scale.</param>
    /// <param name="maxDigits">Supplied coefficient capacity, from 1 through 256.</param>
    /// <param name="maxScale">Supplied scale capacity, from 0 through 256.</param>
    /// <exception cref="ArgumentOutOfRangeException">Bounds or second value are invalid.</exception>
    /// <exception cref="NotSupportedException">Supplied value exceeds capacity.</exception>
    public XdmExactSecond(BigInteger coefficient, int scale, int maxDigits, int maxScale)
    {
        if (maxDigits is < 1 or > 256) throw new ArgumentOutOfRangeException(nameof(maxDigits));
        if (maxScale is < 0 or > 256) throw new ArgumentOutOfRangeException(nameof(maxScale));
        _value = new XdmExactDecimal(coefficient, scale, maxDigits, maxScale);
        if (coefficient.Sign < 0 || _value.Coefficient >= 60 * BigInteger.Pow(10, _value.Scale))
            throw new ArgumentOutOfRangeException(nameof(coefficient));
    }
    /// <summary>Gets the normalized coefficient; default is zero.</summary>
    public BigInteger Coefficient => _value.Coefficient;
    /// <summary>Gets the normalized scale.</summary>
    public int Scale => _value.Scale;
    /// <summary>Parses two ASCII whole-second digits and an optional nonempty decimal fraction.</summary>
    /// <param name="text">Second token with no whitespace or sign.</param>
    /// <param name="maxDigits">Coefficient capacity.</param>
    /// <param name="maxScale">Scale capacity.</param>
    /// <returns>The exact seconds.</returns>
    /// <exception cref="FormatException">The lexical token is malformed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The value or bounds are invalid.</exception>
    /// <exception cref="NotSupportedException">Input exceeds capacity.</exception>
    public static XdmExactSecond Parse(string text, int maxDigits, int maxScale)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length > 259) throw new NotSupportedException("Second lexical capacity exceeded.");
        if (text.Length < 2 || (text.Length > 2 && (text[2] != '.' || text.Length == 3))) throw new FormatException("Invalid second token.");
        BigInteger coefficient = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (i == 2) continue;
            if (text[i] is < '0' or > '9') throw new FormatException("ASCII second digits required.");
            coefficient = coefficient * 10 + text[i] - '0';
        }
        return new(coefficient, text.Length > 2 ? text.Length - 3 : 0, maxDigits, maxScale);
    }
    /// <summary>Formats two whole digits and normalized fractional digits within an output bound.</summary>
    /// <param name="maxCharacters">Positive output capacity.</param>
    /// <returns>Invariant second text.</returns>
    /// <exception cref="NotSupportedException">Output exceeds capacity.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Output bound is invalid.</exception>
    public string ToInvariantString(int maxCharacters)
    {
        var text = _value.ToInvariantString(maxCharacters);
        if ((Scale == 0 ? text.Length : text.IndexOf('.')) == 1) text = "0" + text;
        if (text.Length > maxCharacters) throw new NotSupportedException("Second output capacity exceeded.");
        return text;
    }
    /// <summary>Converts exactly to sub-minute units, refusing discarded digits.</summary>
    /// <param name="decimalPlaces">Resolution: 3 for milliseconds or 7 for 100ns ticks.</param>
    /// <returns>Exact units since the start of the minute.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Resolution is neither 3 nor 7.</exception>
    /// <exception cref="NotSupportedException">Conversion would discard fractional digits.</exception>
    public long ToExactUnits(int decimalPlaces)
    {
        if (decimalPlaces is not (3 or 7)) throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
        if (Scale > decimalPlaces) throw new NotSupportedException("Calendar conversion would lose fractional seconds.");
        return checked((long)(Coefficient * BigInteger.Pow(10, decimalPlaces - Scale)));
    }
    /// <summary>Compares seconds numerically.</summary>
    /// <param name="other">Other seconds.</param>
    /// <returns>Negative, zero or positive ordering.</returns>
    public int CompareTo(XdmExactSecond other)
    {
        int scale = Math.Max(Scale, other.Scale);
        return (Coefficient * BigInteger.Pow(10, scale - Scale)).CompareTo(other.Coefficient * BigInteger.Pow(10, scale - other.Scale));
    }
    /// <summary>Tests normalized numeric equality.</summary>
    /// <param name="other">Other seconds.</param>
    /// <returns>Whether the seconds are equal.</returns>
    public bool Equals(XdmExactSecond other) => _value.Equals(other._value);
    /// <summary>Tests normalized numeric equality.</summary>
    /// <param name="obj">Candidate value.</param>
    /// <returns>Whether the candidate is equal.</returns>
    public override bool Equals(object? obj) => obj is XdmExactSecond other && Equals(other);
    /// <summary>Hashes normalized seconds.</summary>
    /// <returns>The numeric hash.</returns>
    public override int GetHashCode() => _value.GetHashCode();
}
