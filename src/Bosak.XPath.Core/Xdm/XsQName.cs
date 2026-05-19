// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Represents an xs:QName value in the XQuery Data Model.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// Represents an <c>xs:QName</c> value: a tuple of namespace URI and local name.
/// The prefix is not part of the value space and is not stored.
/// </summary>
public readonly record struct XsQName(string LocalName, string NamespaceUri)
{
    /// <summary>
    /// Returns the lexical representation used by <see cref="XdmValue.ToString"/>.
    /// No-namespace QNames render as the local name; namespaced QNames as <c>Q{uri}local</c>.
    /// </summary>
    public override string ToString()
        => string.IsNullOrEmpty(NamespaceUri) ? LocalName : $"Q{{{NamespaceUri}}}{LocalName}";
}
