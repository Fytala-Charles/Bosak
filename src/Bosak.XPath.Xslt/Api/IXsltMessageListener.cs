// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 31 mei 2026
// PURPOSE              : Callback interface for xsl:message output.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 31-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

namespace Bosak.XPath.Xslt.Api;

/// <summary>
/// Receives text emitted by <c>xsl:message</c> instructions during transformation.
/// </summary>
public interface IXsltMessageListener
{
    /// <summary>
    /// Called once for each <c>xsl:message</c> that is evaluated.
    /// </summary>
    /// <param name="message">The atomized string value of the message.</param>
    void OnMessage(string message);
}
