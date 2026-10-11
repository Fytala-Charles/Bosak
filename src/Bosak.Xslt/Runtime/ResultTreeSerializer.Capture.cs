// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 October 2026
// PURPOSE              : Streams supported capture serialization profiles into a bounded encoding sink.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-10-2026     | Creation: C-F1 streaming capture and explicit unsupported profiles.                      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;

namespace Bosak.Xslt.Runtime;

internal static partial class ResultTreeSerializer
{
    /// <summary>
    /// Produces bounded capture bytes. JSON/adaptive and text normalization require whole-value
    /// serializers today and are explicitly refused when byte limits are requested. Unbounded
    /// capture retains the established serialization behavior, with terminal control observations.
    /// </summary>
    internal static byte[] SerializeCaptureBytes(XdmValue value, Stylesheet.OutputProperties props,
        long? perOutput, long? aggregate, Action? observeControls = null, Action<long>? observeProduction = null)
    {
        observeControls?.Invoke();
        if (!perOutput.HasValue && !aggregate.HasValue)
        {
            var legacyBytes = SerializeToBytes(value, props);
            observeControls?.Invoke();
            return legacyBytes;
        }

        InferMethod(props, value);
        ApplyMethodDefaults(props);
        ValidateOutputProperties(props);
        ValidateResultTree(value, props);
        ValidateSerializableItems(value, props);

        if (props.Method is "json" or "adaptive"
            || (props.Method == "text" && TryGetNormalizationForm(props).HasValue)
            || (props.Method != "text" && props.CharacterMap is { Count: > 0 })
            || (props.Method != "text" && FlattenItems(value).Any(i => i.IsNode && i.NodeValue is not XDocumentNode)))
            throw new XsltRuntimeException("XV0014",
                $"Byte-bounded capture does not support this method/normalization/character-map/provider profile (method='{props.Method}'); no serialization was attempted.", default);

        using var sink = new BoundedSerializationWriter(GetEncodingWithBom(props.Encoding, props.ByteOrderMark),
            props.ByteOrderMark, perOutput, aggregate, observeControls, observeProduction);
        if (!value.IsUndefined)
        {
            switch (props.Method)
            {
                case "text": WriteCaptureText(sink, value, props); break;
                case "html": WriteHtml(sink, value, props); break;
                case "xhtml":
                    var xhtml = new CaptureMarkupWriter(sink, xmlWriterFixes: false);
                    WriteXhtml(xhtml, value, props);
                    xhtml.Complete();
                    break;
                default: WriteCaptureXml(sink, value, props); break;
            }
        }
        var bytes = sink.Complete();
        observeControls?.Invoke();
        return bytes;
    }

    private static void WriteCaptureText(TextWriter writer, XdmValue value, Stylesheet.OutputProperties props)
    {
        WriteByteOrderMark(writer, props);
        bool explicitSeparator = props.ItemSeparatorSpecified && props.ItemSeparator != "#absent";
        bool previousAtomic = false;
        bool wroteItem = false;
        foreach (var item in FlattenItems(value))
        {
            if (item.IsUndefined) continue;
            bool atomic = !item.IsNode;
            if (wroteItem && (explicitSeparator || (previousAtomic && atomic)))
                writer.Write(explicitSeparator ? props.ItemSeparator : " ");
            if (atomic)
                WriteCaptureMappedText(writer, item.ToString(), props);
            else if (item.NodeValue is { } node)
                WriteCaptureTextNode(writer, node, props, topLevel: true);
            previousAtomic = atomic;
            wroteItem = true;
        }
    }

    private static void WriteCaptureTextNode(TextWriter writer, IXdmNode node, Stylesheet.OutputProperties props, bool topLevel)
    {
        if (node.NodeKind is XdmNodeKind.Element or XdmNodeKind.Document)
        {
            foreach (var child in node.Axis(XdmAxis.Child))
                WriteCaptureTextNode(writer, child.NodeValue!, props, topLevel: false);
        }
        else if (node.NodeKind == XdmNodeKind.Text
            || (topLevel && node.NodeKind is not (XdmNodeKind.Comment or XdmNodeKind.ProcessingInstruction)))
            WriteCaptureMappedText(writer, node.StringValue, props);
    }

    private static void WriteCaptureMappedText(TextWriter writer, string text, Stylesheet.OutputProperties props)
    {
        if (props.CharacterMap is not { Count: > 0 } map)
        {
            writer.Write(text);
            return;
        }
        Span<char> chars = stackalloc char[2];
        foreach (var rune in text.EnumerateRunes())
        {
            if (map.TryGetValue(rune.Value, out var replacement)) writer.Write(replacement);
            else
            {
                int count = rune.EncodeToUtf16(chars);
                writer.Write(chars[..count]);
            }
        }
    }

    private static void WriteCaptureXml(BoundedSerializationWriter sink, XdmValue value, Stylesheet.OutputProperties props)
    {
        if (!value.IsNode && !value.IsSequence)
        {
            sink.Write(value.ToString());
            return;
        }

        XNode? node = value.IsNode && value.NodeValue is XDocumentNode xdn ? xdn.UnderlyingObject as XNode : null;
        bool separated = props.ItemSeparatorSpecified && props.ItemSeparator != "#absent";
        bool wrapper = node is XElement e && e.Name == "__xdm_doc__";
        bool sequence = value.IsSequence || (separated && (wrapper || node is XDocument));
        if (node is not null && !sequence)
        {
            bool raw = props.Version == "1.1" || HasRawText(node)
                || (props.Indent && props.SuppressIndentation.Count > 0)
                || (wrapper ? HasUndeclarationAnnotations(node) : props.CharacterMap is { Count: > 0 });
            if (!wrapper && props.Version != "1.1") ValidateXml10(node);
            if (raw)
            {
                WriteCaptureRawXml(sink, node, props);
                return;
            }
            if (TryGetNormalizationForm(props) is { } form) node = NormalizeXNode(node, form);
            node = NormalizeForXmlWriter(WrapCdataSections(node, props), props);
        }

        var lexical = new CaptureMarkupWriter(sink, xmlWriterFixes: true,
            writerEncoding: sequence ? Encoding.Unicode : null, convertHex: !sequence);
        // Preserve XmlWriter's encoding fallback (numeric references for unrepresentable
        // characters), but decode its fixed-size chunks directly into the lexical filter.
        using var bridge = new CaptureDecodingStream(sink.Encoding, lexical);
        var settings = CreateXmlWriterSettings(props, sink.Encoding);
        if (sequence || wrapper || node is not (XDocument or XElement))
        {
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            if (sequence) settings.OmitXmlDeclaration = true;
        }
        if (sequence) WriteByteOrderMark(lexical, props);
        using (var xml = sequence ? XmlWriter.Create(lexical, settings) : XmlWriter.Create(bridge, settings))
        {
            if (sequence)
            {
                bool wrote = false, previousAtomic = false;
                foreach (var item in FlattenItems(value))
                {
                    if (item.IsUndefined) continue;
                    bool atomic = !item.IsNode;
                    if (wrote && (separated || (previousAtomic && atomic)))
                        xml.WriteString(separated ? props.ItemSeparator : " ");
                    if (atomic) xml.WriteString(item.ToString());
                    else if (item.NodeValue is { } itemNode) WriteNode(xml, itemNode);
                    previousAtomic = atomic;
                    wrote = true;
                }
            }
            else if (node is XDocument document || wrapper)
            {
                if (!props.OmitXmlDeclaration)
                    WriteCaptureDeclaration(xml, props);
                var children = node is XDocument doc ? doc.Nodes() : ((XElement)node!).Nodes();
                bool doctypeWritten = false;
                foreach (var child in children)
                {
                    if (!doctypeWritten && child is XElement root)
                    {
                        WriteDoctype(xml, root, props);
                        doctypeWritten = true;
                    }
                    child.WriteTo(xml);
                }
            }
            else if (node is not null)
            {
                WriteDoctype(xml, node as XElement, props);
                node.WriteTo(xml);
            }
            else xml.WriteString(value.ToString());
            xml.Flush();
        }
        bridge.Complete();
        lexical.Complete();
    }

    private static void WriteCaptureDeclaration(XmlWriter writer, Stylesheet.OutputProperties props)
        => writer.WriteProcessingInstruction("xml", $"version=\"{props.Version}\" encoding=\"{props.Encoding}\""
            + (props.Standalone is "yes" or "no" ? $" standalone=\"{props.Standalone}\"" : ""));

    private static void WriteCaptureRawXml(TextWriter sink, XNode node, Stylesheet.OutputProperties props)
    {
        if (TryGetNormalizationForm(props) is { } form) node = NormalizeCommentsAndPis(node, form);
        node = WrapCdataSections(node, props);
        var writer = new CaptureMarkupWriter(sink, xmlWriterFixes: false);
        WriteByteOrderMark(writer, props);
        if (!props.OmitXmlDeclaration && props.Version is "1.0" or "1.1")
            writer.Write($"<?xml version=\"{props.Version}\" encoding=\"{props.Encoding}\""
                + (props.Standalone is "yes" or "no" ? $" standalone=\"{props.Standalone}\"" : "") + "?>");
        var bindings = new Dictionary<string, string> { ["xml"] = "http://www.w3.org/XML/1998/namespace" };
        if (node is XDocument doc)
        {
            WriteDoctype(writer, doc.Root, props);
            foreach (var child in doc.Nodes()) SerializeRawNode(writer, child, props, 0, bindings);
        }
        else if (node is XElement wrapper && wrapper.Name == "__xdm_doc__")
        {
            foreach (var child in wrapper.Nodes()) SerializeRawNode(writer, child, props, 0, bindings);
        }
        else
        {
            WriteDoctype(writer, node as XElement, props);
            SerializeRawNode(writer, node, props, 0, bindings);
        }
        writer.Complete();
    }
}
