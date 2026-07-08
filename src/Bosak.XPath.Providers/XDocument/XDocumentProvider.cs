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
//                      | Charles Korthout | 0.2   | 05-06-2026     | Preserve whitespace in elements; strip document-level whitespace-only text nodes        |
//                      | Charles Korthout | 0.3   | 25-06-2026     | LoadXml sets DocumentUri on returned document node                                      |
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
        var map = ComputeDocumentOrder(document);
        XDocumentNode.RegisterOrderMap(document, map);
        return new XDocumentNode(document);
    }

    /// <summary>
    /// Adapts an <see cref="XElement"/> to <see cref="IXdmNode"/>.
    /// </summary>
    public static IXdmNode ToXdmNode(this XElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        var doc = element.Document;
        if (doc is not null)
        {
            var map = ComputeDocumentOrder(doc);
            XDocumentNode.RegisterOrderMap(doc, map);
        }
        return new XDocumentNode(element);
    }

    /// <summary>
    /// Parses an XML string and returns the root as an <see cref="IXdmNode"/>.
    /// XML 1.1 declarations are accepted by encoding name characters that .NET rejects.
    /// </summary>
    public static IXdmNode ParseXml(string xml)
    {
        var document = Xml11Loader.Parse(xml, LoadOptions.PreserveWhitespace);
        var map = ComputeDocumentOrder(document);
        XDocumentNode.RegisterOrderMap(document, map);
        return new XDocumentNode(document);
    }

    /// <summary>
    /// Parses an XML string that is known to be XML 1.1 and returns the root as an <see cref="IXdmNode"/>.
    /// </summary>
    public static IXdmNode ParseXml11(string xml, string? baseUri = null)
    {
        var document = Xml11Loader.ParseXml11(xml, LoadOptions.PreserveWhitespace, baseUri);
        var map = ComputeDocumentOrder(document);
        XDocumentNode.RegisterOrderMap(document, map);
        return new XDocumentNode(document);
    }

    /// <summary>
    /// Loads an XML file and returns the root as an <see cref="IXdmNode"/>.
    /// The file path is preserved as the document's base URI.
    /// XML 1.1 declarations are accepted by encoding name characters that .NET rejects.
    /// </summary>
    public static IXdmNode LoadXml(string filePath)
    {
        var document = Xml11Loader.Load(filePath, LoadOptions.SetBaseUri | LoadOptions.PreserveWhitespace);
        StripDocumentLevelWhitespace(document);
        var map = ComputeDocumentOrder(document);
        XDocumentNode.RegisterOrderMap(document, map);
        var node = new XDocumentNode(document);
        node.SetDocumentUri(new Uri(filePath).AbsoluteUri);
        return node;
    }

    /// <summary>
    /// Removes whitespace-only text nodes that are direct children of the document node.
    /// XPath/XQuery processors typically preserve whitespace inside elements but strip
    /// insignificant whitespace before/after the root element.
    /// </summary>
    public static void StripDocumentLevelWhitespace(System.Xml.Linq.XDocument doc)
    {
        var toRemove = doc.Nodes()
            .OfType<System.Xml.Linq.XText>()
            .Where(t => string.IsNullOrWhiteSpace(t.Value))
            .ToList();
        foreach (var node in toRemove)
            node.Remove();
    }

    // ------------------------------------------------------------------
    // Document order indexing
    // ------------------------------------------------------------------

    private static Dictionary<XObject, long> ComputeDocumentOrder(System.Xml.Linq.XDocument doc)
    {
        var map = new Dictionary<XObject, long>();
        long index = 0;

        map[doc] = index++;
        Traverse(doc, ref index, map);

        return map;
    }

    private static void Traverse(XContainer container, ref long index, Dictionary<XObject, long> map)
    {
        foreach (var node in container.Nodes())
        {
            map[node] = index++;

            if (node is XElement el)
            {
                foreach (var attr in el.Attributes())
                    map[attr] = index++;

                Traverse(el, ref index, map);
            }
        }
    }
}
