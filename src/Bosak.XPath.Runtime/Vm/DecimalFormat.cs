// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 22 mei 2026
// PURPOSE              : Holds the properties of an XPath decimal-format declaration.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 22-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Runtime.Vm;

/// <summary>
/// Represents an XPath 3.1 decimal-format declaration (default or named).
/// </summary>
public sealed class DecimalFormat
{
    public string DecimalSeparator { get; set; } = ".";
    public string GroupingSeparator { get; set; } = ",";
    public string Digit { get; set; } = "#";
    public string ZeroDigit { get; set; } = "0";
    public string PatternSeparator { get; set; } = ";";
    public string MinusSign { get; set; } = "-";
    public string Percent { get; set; } = "%";
    public string PerMille { get; set; } = "\u2030";
    public string Infinity { get; set; } = "Infinity";
    public string NaN { get; set; } = "NaN";
    public string ExponentSeparator { get; set; } = "e";
}
