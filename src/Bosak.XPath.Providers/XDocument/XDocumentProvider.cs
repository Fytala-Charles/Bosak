// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Extension methods to convert LINQ to XML objects into IXdmNode adapters
// SPECIAL NOTES        : Part of the XDocument node provider layer.
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

using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Providers.Xml;


/// <summary>
/// Provides extension methods to adapt LINQ to XML types to the XDM <see cref="IXdmNode"/> interface.
/// </summary>
public static class XDocumentProvider
{
    /// <summary>
    /// Adapts an <see cref="XDocument"/> to <see cref="IXdmNode"/>.
    /// </summary>
    public static IXdmNode ToXdmNode(this System.Xml.Linq.XDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return new XDocumentNode(document);
    }

    /// <summary>
    /// Adapts an <see cref="XElement"/> to <see cref="IXdmNode"/>.
    /// </summary>
    public static IXdmNode ToXdmNode(this XElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return new XDocumentNode(element);
    }

    /// <summary>
    /// Parses an XML string and returns the root as an <see cref="IXdmNode"/>.
    /// </summary>
    public static IXdmNode ParseXml(string xml)
    {
        var document = System.Xml.Linq.XDocument.Parse(xml);
        return new XDocumentNode(document);
    }
}
