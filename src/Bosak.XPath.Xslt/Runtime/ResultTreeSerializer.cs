// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 mei 2026
// PURPOSE              : Serializes an XDM result tree to an XML string.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : License.txt
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-05-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 24-05-2026     | Added OutputProperties support for xsl:output (method, indent, omit-declaration)       |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Xslt.Runtime;

/// <summary>
/// Serializes XDM result trees to XML strings.
/// </summary>
public static class ResultTreeSerializer
{
    /// <summary>
    /// Serializes an XDM value to an XML string using default settings.
    /// </summary>
    public static string Serialize(XdmValue value)
        => Serialize(value, null);

    /// <summary>
    /// Serializes an XDM value to a string using the provided output properties.
    /// </summary>
    public static string Serialize(XdmValue value, Stylesheet.OutputProperties? output)
    {
        if (value.IsUndefined)
            return string.Empty;

        var props = output ?? new Stylesheet.OutputProperties();

        if (props.Method == "text")
        {
            return SerializeAsText(value);
        }

        // method="xml" (default)
        if (value.IsNode)
        {
            return SerializeNode(value.NodeValue, props);
        }

        if (value.IsSequence && value.SequenceValue != null)
        {
            return SerializeSequence(value.SequenceValue, props);
        }

        // Atomic value
        return value.ToString();
    }

    private static string SerializeAsText(XdmValue value)
    {
        var sb = new System.Text.StringBuilder();
        CollectText(value, sb);
        return sb.ToString();
    }

    private static void CollectText(XdmValue value, System.Text.StringBuilder sb)
    {
        if (value.IsUndefined)
            return;

        if (value.IsNode && value.NodeValue != null)
        {
            CollectTextFromNode(value.NodeValue, sb);
        }
        else if (value.IsSequence && value.SequenceValue != null)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue))
                CollectText(item, sb);
        }
        else
        {
            sb.Append(value.ToString());
        }
    }

    private static void CollectTextFromNode(IXdmNode node, System.Text.StringBuilder sb)
    {
        switch (node.NodeKind)
        {
            case XdmNodeKind.Text:
                sb.Append(node.StringValue);
                break;
            case XdmNodeKind.Element:
                foreach (var child in node.Axis(XdmAxis.Child))
                    CollectTextFromNode(child.NodeValue!, sb);
                break;
            case XdmNodeKind.Document:
                foreach (var child in node.Axis(XdmAxis.Child))
                    CollectTextFromNode(child.NodeValue!, sb);
                break;
            // Attributes, comments, PIs, and namespace nodes are ignored for text output
        }
    }

    private static string SerializeNode(IXdmNode? node, Stylesheet.OutputProperties? output = null)
    {
        if (node == null)
            return string.Empty;

        var props = output ?? new Stylesheet.OutputProperties();

        // For XDocument-backed nodes, use XmlWriter for proper serialization control
        if (node is Providers.Xml.XDocumentNode xdocNode)
        {
            var obj = xdocNode.UnderlyingObject;
            if (obj is XElement elem)
            {
                return SerializeXElement(elem, props);
            }
            if (obj is XDocument doc)
            {
                return SerializeXDocument(doc, props);
            }
            return obj?.ToString() ?? string.Empty;
        }

        // Fallback: build an XML representation
        return node.ToXmlString();
    }

    private static string SerializeXElement(XElement element, Stylesheet.OutputProperties props)
    {
        using var writer = new StringWriter();
        var settings = CreateXmlWriterSettings(props);
        using var xmlWriter = XmlWriter.Create(writer, settings);
        element.WriteTo(xmlWriter);
        xmlWriter.Flush();
        return writer.ToString();
    }

    private static string SerializeXDocument(XDocument document, Stylesheet.OutputProperties props)
    {
        using var writer = new StringWriter();
        var settings = CreateXmlWriterSettings(props);
        using var xmlWriter = XmlWriter.Create(writer, settings);

        if (!props.OmitXmlDeclaration)
        {
            xmlWriter.WriteProcessingInstruction("xml",
                $"version=\"{props.Version}\" encoding=\"{props.Encoding}\"" +
                (props.Standalone != null ? $" standalone=\"{props.Standalone}\"" : ""));
        }

        foreach (var node in document.Nodes())
        {
            node.WriteTo(xmlWriter);
        }

        xmlWriter.Flush();
        return writer.ToString();
    }

    private static string SerializeSequence(IXdmSequence sequence, Stylesheet.OutputProperties props)
    {
        using var writer = new StringWriter();
        var settings = CreateXmlWriterSettings(props);
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        using var xmlWriter = XmlWriter.Create(writer, settings);

        foreach (var item in XdmSequence.FromSource(sequence))
        {
            if (item.IsNode && item.NodeValue != null)
            {
                WriteNode(xmlWriter, item.NodeValue);
            }
            else if (!item.IsUndefined)
            {
                xmlWriter.WriteString(item.ToString());
            }
        }

        xmlWriter.Flush();
        return writer.ToString();
    }

    private static XmlWriterSettings CreateXmlWriterSettings(Stylesheet.OutputProperties props)
    {
        return new XmlWriterSettings
        {
            OmitXmlDeclaration = props.OmitXmlDeclaration,
            Indent = props.Indent,
            Encoding = System.Text.Encoding.UTF8,
            ConformanceLevel = ConformanceLevel.Document
        };
    }

    private static void WriteNode(XmlWriter writer, IXdmNode node)
    {
        if (node is Providers.Xml.XDocumentNode xdocNode)
        {
            var obj = xdocNode.UnderlyingObject;
            switch (obj)
            {
                case XDocument doc:
                    foreach (var child in doc.Nodes())
                        child.WriteTo(writer);
                    break;
                case XElement elem:
                    elem.WriteTo(writer);
                    break;
                case XText text:
                    writer.WriteString(text.Value);
                    break;
                case XComment comment:
                    writer.WriteComment(comment.Value);
                    break;
                case XProcessingInstruction pi:
                    writer.WriteProcessingInstruction(pi.Target, pi.Data);
                    break;
            }
        }
        else
        {
            writer.WriteString(node.ToXmlString());
        }
    }
}
