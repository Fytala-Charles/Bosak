// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 mei 2026
// PURPOSE              : Holds the properties of an XPath decimal-format declaration.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.11  | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Runtime.Vm;

/// <summary>
/// Represents an XPath 3.1 decimal-format declaration (default or named).
/// </summary>
public sealed class DecimalFormat
{
    /// <summary>The character that separates the integer part from the fractional part (default '.').</summary>
    public string DecimalSeparator { get; set; } = ".";
    /// <summary>The character that separates groups of integer digits (default ',').</summary>
    public string GroupingSeparator { get; set; } = ",";

    /// <summary>The optional-digit sign used in format pictures (default '#').</summary>
    public string Digit { get; set; } = "#";

    /// <summary>The mandatory-digit (zero) sign used in format pictures (default '0').</summary>
    public string ZeroDigit { get; set; } = "0";

    /// <summary>The character that separates the positive and negative sub-pictures (default ';').</summary>
    public string PatternSeparator { get; set; } = ";";

    /// <summary>The sign prefixed to negative numbers (default '-').</summary>
    public string MinusSign { get; set; } = "-";

    /// <summary>The percent sign; its presence in a picture multiplies the value by 100 (default '%').</summary>
    public string Percent { get; set; } = "%";
    /// <summary>The per-mille sign; its presence in a picture multiplies the value by 1000 (default U+2030).</summary>
    public string PerMille { get; set; } = "\u2030";
    /// <summary>The lexical representation used for positive and negative infinity (default "Infinity").</summary>
    public string Infinity { get; set; } = "Infinity";

    /// <summary>The lexical representation used for NaN (default "NaN").</summary>
    public string NaN { get; set; } = "NaN";

    /// <summary>The character that separates mantissa and exponent in scientific notation (default 'e').</summary>
    public string ExponentSeparator { get; set; } = "e";
}
