// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 19 mei 2026
// PURPOSE              : Abstract representation of a node in the XQuery Data Model. Implementations adapt concrete XML AP...
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 19-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 19-05-2026     | Added ToXmlString for fn:serialize                                                     |
//                      | Charles Korthout | 0.3   | 19-05-2026     | Added BaseUri property for fn:base-uri and fn:document-uri                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
namespace Bosak.XPath.Core.Xdm;

/// <summary>
/// Abstract representation of a node in the XQuery Data Model.
/// Implementations adapt concrete XML APIs (XDocument, XmlDocument, streaming, etc.)
/// without copying into a proprietary DOM.
/// </summary>
public interface IXdmNode
{
    /// <summary>Gets the kind of this node.</summary>
    XdmNodeKind NodeKind { get; }

    /// <summary>Gets the local name of this node, or empty string if unnamed.</summary>
    string LocalName { get; }

    /// <summary>Gets the namespace URI of this node, or empty string if none.</summary>
    string NamespaceUri { get; }

    /// <summary>Gets the namespace prefix of this node, or empty string if none.</summary>
    string Prefix { get; }

    /// <summary>Gets the string value of this node per XDM rules.</summary>
    string StringValue { get; }

    /// <summary>Gets the typed value if available, otherwise the string value.</summary>
    XdmValue TypedValue { get; }

    /// <summary>Gets the parent node, or null if this is the root.</summary>
    IXdmNode? Parent { get; }

    /// <summary>Gets the document node containing this node, or null.</summary>
    IXdmNode? Document { get; }

    /// <summary>Returns a lazy sequence of children filtered by node kind.</summary>
    XdmSequence Children(XdmNodeKind kind = XdmNodeKind.All);

    /// <summary>Returns a lazy sequence of attributes, optionally filtered by name.</summary>
    XdmSequence Attributes(string? localName = null, string? namespaceUri = null);

    /// <summary>Returns a lazy sequence along the specified axis.</summary>
    XdmSequence Axis(XdmAxis axis);

    /// <summary>Returns true if this node and <paramref name="other"/> are the same node.</summary>
    bool IsSameNode(IXdmNode other);

    /// <summary>Gets the document order index of this node, or 0 if unknown.</summary>
    long DocumentOrder { get; }

    /// <summary>Gets the base URI of this node, or empty string if none.</summary>
    string BaseUri { get; }

    /// <summary>Returns the XML serialization of this node.</summary>
    string ToXmlString();
}
