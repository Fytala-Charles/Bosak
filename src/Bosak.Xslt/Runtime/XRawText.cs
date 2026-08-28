// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 28 augustus 2026
// PURPOSE              : Represents a text node whose content must be serialized without escaping.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 28-08-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.Xslt.Runtime;

/// <summary>
/// Represents a text node whose content must be written to the output without
/// XML/HTML escaping. Used to implement <c>xsl:text</c> and <c>xsl:value-of</c>
/// <c>disable-output-escaping="yes"</c>.
/// </summary>
internal sealed class XRawText : XText
{
    /// <summary>
    /// Initializes a new raw text node with the specified value.
    /// </summary>
    /// <param name="value">The text value to write unescaped.</param>
    public XRawText(string value) : base(value) { }
}
