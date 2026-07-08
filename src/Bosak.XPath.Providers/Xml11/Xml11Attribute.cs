// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 July 2026
// PURPOSE              : Helpers for creating XML 1.1-aware XAttributes in the XDocument provider layer.
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-07-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml.Linq;

namespace Bosak.XPath.Providers.Xml;

/// <summary>
/// Helper methods for creating <see cref="XAttribute"/> nodes that may contain
/// XML 1.1-only characters in their values. The values are encoded so that
/// .NET's XML 1.0 validation does not reject them at construction time.
/// </summary>
public static class Xml11Attribute
{
    /// <summary>
    /// Creates an <see cref="XAttribute"/> with the supplied value encoded when necessary.
    /// </summary>
    public static XAttribute Create(XName name, string value)
        => new XAttribute(name, Xml11NameCodec.EncodeValue(value));

    /// <summary>
    /// Sets an attribute value, encoding it when necessary.
    /// </summary>
    public static void SetValue(XElement element, XName name, string value)
        => element.SetAttributeValue(name, Xml11NameCodec.EncodeValue(value));

    /// <summary>
    /// Gets the decoded value of an attribute.
    /// </summary>
    public static string GetValue(XAttribute attribute)
        => Xml11NameCodec.DecodeValue(attribute.Value);
}
