// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Represents an xs:QName value in the XQuery Data Model.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 22-05-2026     | Added Prefix field for lexical QName serialization                                       |
//                      | Charles Korthout | 0.3   | 25-06-2026     | QName equality ignores prefix; compares namespace URI and local name only              |
//                      | Charles Korthout | 0.4   | 09-09-2026     | XML doc coverage on public API (Beta review)                                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// Represents an <c>xs:QName</c> value: a tuple of namespace URI, local name, and optional prefix.
/// </summary>
/// <param name="LocalName">The local name.</param>
/// <param name="NamespaceUri">The namespace URI, or empty string for no namespace.</param>
/// <param name="Prefix">The lexical prefix, if any.</param>
public readonly record struct XsQName(string LocalName, string NamespaceUri, string Prefix = "")
{
    /// <summary>
    /// Determines whether two <see cref="XsQName"/> values denote the same expanded name.
    /// Only the namespace URI and local name are compared; the prefix is ignored.
    /// </summary>
    public bool Equals(XsQName other)
        => LocalName == other.LocalName && NamespaceUri == other.NamespaceUri;

    /// <summary>
    /// Returns a hash code based on the namespace URI and local name.
    /// </summary>
    public override int GetHashCode()
        => HashCode.Combine(LocalName, NamespaceUri);

    /// <summary>
    /// Returns the lexical representation used by <see cref="XdmValue.ToString"/>.
    /// Prefixed QNames render as <c>prefix:local</c>; no-namespace QNames as the local name;
    /// namespaced QNames without prefix as <c>Q{uri}local</c>.
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Prefix))
            return $"{Prefix}:{LocalName}";
        return LocalName;
    }
}
