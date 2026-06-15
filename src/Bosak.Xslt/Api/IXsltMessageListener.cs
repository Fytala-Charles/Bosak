// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 mei 2026
// PURPOSE              : Callback interface for xsl:message output and XSLT warnings.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 31-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 13-06-2026     | Added OnWarning callback                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.Xslt.Api;

/// <summary>
/// Receives text emitted by <c>xsl:message</c> instructions and XSLT warnings during transformation.
/// </summary>
public interface IXsltMessageListener
{
    /// <summary>
    /// Called once for each <c>xsl:message</c> that is evaluated.
    /// </summary>
    /// <param name="message">The atomized string value of the message.</param>
    void OnMessage(string message);

    /// <summary>
    /// Called once for each XSLT warning (for example, a no-matching-template warning).
    /// </summary>
    /// <param name="message">The warning text.</param>
    void OnWarning(string message);
}
