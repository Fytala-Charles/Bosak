// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 28 October 2026
// PURPOSE              : Apply construction-mode schema copying on destination-private XDocument nodes.
// SPECIAL NOTES        : Part of the XDocument node provider layer.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 28-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 29-10-2026     | Add bounded native document assessment bridge and original-state copy handling           |
// ===========================================================================================================================================================
using System.Xml.Linq;
using System.Xml.Schema;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Providers.Xml;

/// <summary>Applies the XQuery construction contract without altering source metadata.</summary>
internal static class XQueryConstructionMetadata
{
    internal static void ApplyTree(XObject? source, XNode target, XdmConstructionMode mode, bool preserveNamespaces)
    {
        XElement? from = source as XElement ?? (source as XDocument)?.Root;
        XElement? to = target as XElement ?? (target as XDocument)?.Root;
        if (to is null) return;
        if (mode == XdmConstructionMode.Preserve && from is not null)
        {
            if (!preserveNamespaces)
                foreach (var element in from.DescendantsAndSelf())
                {
                    CheckSensitive(XDocumentNode.Wrap(element));
                    foreach (var attribute in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
                        CheckSensitive(XDocumentNode.Wrap(attribute));
                }
            foreach (var pair in from.DescendantsAndSelf().Zip(to.DescendantsAndSelf()))
            {
                CopyAnnotation(pair.First, pair.Second);
                foreach (var attributes in pair.First.Attributes().Where(a => !a.IsNamespaceDeclaration)
                             .Zip(pair.Second.Attributes().Where(a => !a.IsNamespaceDeclaration)))
                    CopyAnnotation(attributes.First, attributes.Second);
            }
        }
        else if (mode == XdmConstructionMode.Strip)
            foreach (var element in to.DescendantsAndSelf())
            {
                Strip(element);
                foreach (var attribute in element.Attributes().Where(a => !a.IsNamespaceDeclaration)) Strip(attribute);
            }
    }

    internal static void Attribute(IXdmNode? source, XAttribute target, XdmConstructionMode mode)
    {
        if (source is null && target.Name == XNamespace.Xml + "id")
        {
            target.Value = string.Join(' ', target.Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            try { System.Xml.XmlConvert.VerifyNCName(target.Value); }
            catch (Exception ex) when (ex is System.Xml.XmlException or ArgumentException) { throw new InvalidOperationException("XQDY0091: Invalid xml:id value.", ex); }
        }
        if (source is not null && mode == XdmConstructionMode.Preserve)
        {
            // A free attribute's parent is not copied, so its namespace-sensitive typed value is unsafe.
            CheckSensitive(source);
            if (source is XDocumentNode node && node.UnderlyingObject is XAttribute original)
                CopyAnnotation(original, target);
            else if (source.SchemaTypeAnnotation is not null)
                throw new NotSupportedException("Preserving schema metadata from this attribute provider is not supported.");
        }
        else Strip(target); // Fresh attributes always have xs:untypedAtomic, including under preserve.
    }

    internal static void Strip(XObject target)
    {
        if (target is XElement element) XdmElementAssessment.Remove(element);
        target.RemoveAnnotations<IXmlSchemaInfo>();
        target.RemoveAnnotations<XdmIdProperties>();
        target.RemoveAnnotations<ConstructedElementAnnotation>();
        target.RemoveAnnotations<XQueryStrippedTypeAnnotation>();
        target.AddAnnotation(XQueryStrippedTypeAnnotation.Instance);
    }

    private static void CopyAnnotation(XObject source, XObject target)
    {
        if (source is XElement from && target is XElement to) XdmElementAssessment.Copy(from, to);
        target.RemoveAnnotations<IXmlSchemaInfo>();
        target.RemoveAnnotations<XdmIdProperties>();
        target.RemoveAnnotations<ConstructedElementAnnotation>();
        target.RemoveAnnotations<XQueryStrippedTypeAnnotation>();
        IXmlSchemaInfo? info = source switch { XElement e => e.GetSchemaInfo(), XAttribute a => a.GetSchemaInfo(), _ => null };
        if (info is not null)
            target.AddAnnotation(new XmlSchemaInfo
            {
                Validity = info.Validity, IsDefault = info.IsDefault, IsNil = info.IsNil,
                MemberType = info.MemberType, SchemaType = info.SchemaType,
                SchemaElement = info.SchemaElement, SchemaAttribute = info.SchemaAttribute
            });
        if (source.Annotation<ConstructedElementAnnotation>() is not null)
            target.AddAnnotation(ConstructedElementAnnotation.Instance);
        if (source.Annotation<XQueryStrippedTypeAnnotation>() is not null)
            target.AddAnnotation(XQueryStrippedTypeAnnotation.Instance);
        var node = XDocumentNode.Wrap(source);
        if (node.IsId || node.IsIdref)
            target.AddAnnotation(new XdmIdProperties(node.IsId, node.IsIdref));
    }

    private static void CheckSensitive(IXdmNode node)
    {
        if (node.HasNoTypedValue) return;
        var value = node.TypedValue;
        if (value.IsSequence)
        {
            foreach (var item in XdmSequence.FromSource(value.SequenceValue!)) Check(item);
        }
        else Check(value);
        static void Check(XdmValue item)
        {
            if (item.Kind == XdmValueKind.QName || item.SchemaTypeName == "NOTATION")
                throw new InvalidOperationException("XQTY0086: A namespace-sensitive typed value cannot be copied under this construction/namespace policy.");
        }
    }
}

/// <summary>Records XQuery strip semantics without changing XML lexical content.</summary>
internal sealed class XQueryStrippedTypeAnnotation
{
    internal static readonly XQueryStrippedTypeAnnotation Instance = new();
    private XQueryStrippedTypeAnnotation() { }
}
