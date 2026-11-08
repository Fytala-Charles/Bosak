// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 October 2026
// PURPOSE              : Verifies name decoding preserves serialized XML content and namespace values.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 27-10-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.XPath.Providers.Tests;

/// <summary>Verifies name decoding preserves serialized XML content and namespace values.</summary>
public class XmlNameSerializationTests
{
    private const string Literal = "ǀ0041ǁ";

    /// <summary>Crosses name forms, protected content kinds and document/element serialization.</summary>
    /// <returns>Independent serialization scenarios.</returns>
    public static IEnumerable<object[]> Scenarios()
    {
        foreach (string name in new[] { "r", char.ConvertFromUtf32(119070), Literal })
        foreach (string kind in new[] { "text", "attribute", "comment", "pi", "cdata", "namespace" })
        foreach (bool document in new[] { false, true })
            yield return [name, kind, document];
    }

    /// <summary>Name decoding changes only stored name tokens, preserving protected payloads.</summary>
    /// <param name="name">The original XML element name.</param>
    /// <param name="kind">The content kind to preserve.</param>
    /// <param name="document">Whether the annotation belongs to the document.</param>
    [Theory, MemberData(nameof(Scenarios))]
    public void EncodedNames_PreserveContent(string name, string kind, bool document)
    {
        var root = new XElement(Xml11NameCodec.EncodeName(name));
        string expected;
        switch (kind)
        {
            case "attribute":
                var attribute = new XAttribute("a", Literal + " <&>");
                root.Add(attribute);
                expected = "<" + name + " " + attribute + " />";
                break;
            case "namespace":
                var declaration = new XAttribute(XNamespace.Xmlns + "p", "urn:" + Literal);
                root.Add(declaration);
                expected = "<" + name + " " + declaration + " />";
                break;
            default:
                XNode content = kind switch
                {
                    "text" => new XText(Literal + " <&>"),
                    "comment" => new XComment(Literal + " <r a='" + Literal + "'> [ ]"),
                    "pi" => new XProcessingInstruction(Literal, Literal + " <r> \"quoted\""),
                    "cdata" => new XCData(Literal + " <r a='" + Literal + "'>"),
                    _ => throw new ArgumentException("Unknown scenario.", nameof(kind))
                };
                root.Add(content);
                expected = "<" + name + ">" + content.ToString(SaveOptions.DisableFormatting) + "</" + name + ">";
                break;
        }
        if (document)
        {
            var doc = new XDocument(root);
            doc.AddAnnotation(Xml11Annotation.Instance);
            Assert.Equal(expected, XDocumentNode.Wrap(doc).ToXmlString());
        }
        else
        {
            root.AddAnnotation(Xml11Annotation.Instance);
            Assert.Equal(expected, XDocumentNode.Wrap(root).ToXmlString());
        }
    }

    /// <summary>Attribute names and namespace declaration prefixes decode without changing values.</summary>
    [Fact]
    public void AttributeAndPrefixNames_DecodeIndependently()
    {
        string name = char.ConvertFromUtf32(119070), encoded = Xml11NameCodec.EncodeName(name);
        var root = new XElement("r", new XAttribute(encoded, Literal), new XAttribute(XNamespace.Xmlns + encoded, "urn:" + Literal));
        root.AddAnnotation(Xml11Annotation.Instance);
        Assert.Equal("<r " + name + "=\"" + Literal + "\" xmlns:" + name + "=\"urn:" + Literal + "\" />", XDocumentNode.Wrap(root).ToXmlString());
    }

    /// <summary>Ancestor namespace copying preserves the URI of a serialized standalone child.</summary>
    [Fact]
    public void StandaloneChild_PreservesInheritedNamespaceUri()
    {
        string name = char.ConvertFromUtf32(119070);
        var child = new XElement(Xml11NameCodec.EncodeName(name), Literal);
        var root = new XElement("r", new XAttribute(XNamespace.Xmlns + "p", "urn:" + Literal), child);
        root.AddAnnotation(Xml11Annotation.Instance);
        Assert.Equal("<" + name + " xmlns:p=\"urn:" + Literal + "\">" + Literal + "</" + name + ">", XDocumentNode.Wrap(child).ToXmlString());
    }

    /// <summary>DTD literals, comments and declarations are not mistaken for element name tokens.</summary>
    [Fact]
    public void DocumentType_PreservesInternalSubsetAndPayload()
    {
        string subset = "<!ENTITY e '" + Literal + "'><!----><!-- ] > <r a='" + Literal + "'> -->";
        var doc = new XDocument(new XDocumentType("r", null, null, subset), new XComment(Literal), new XProcessingInstruction("p", Literal), new XElement("r", Literal));
        string expected = doc.ToString(SaveOptions.DisableFormatting);
        doc.AddAnnotation(Xml11Annotation.Instance);
        Assert.Equal(expected, XDocumentNode.Wrap(doc).ToXmlString());
    }

    /// <summary>Unannotated trees retain their literal names as well as their content.</summary>
    [Fact]
    public void UnannotatedTree_IsUnchanged()
    {
        var root = new XElement(Literal, Literal);
        Assert.Equal(root.ToString(SaveOptions.DisableFormatting), XDocumentNode.Wrap(root).ToXmlString());
    }
}
