// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 27 October 2026
// PURPOSE              : Verifies Unicode node names and literal payloads survive parsing and copying.
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
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Xunit;
namespace Bosak.XPath.Providers.Tests;
/// <summary>Verifies Unicode node names and literal payloads survive parsing and copying.</summary>
public class UnicodeNodeRoundTripTests
{
    private const string Literal = "ǀ0041ǁ";
    /// <summary>Numeric references use the existing reader behavior without sentinel ambiguity.</summary>
    /// <param name="point">A valid XML1.1 reference value.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(11)]
    [InlineData(133)]
    public void ReferenceAndLiteralValues_AreDistinct(int point)
    {
        var doc = XDocumentProvider.ParseXml11("<r a='"+Literal+"&#x"+point.ToString("X")+";'>"+Literal+"&#x"+point.ToString("X")+";</r>");
        var root = FirstElement(doc);
        Assert.Equal(Literal + char.ConvertFromUtf32(point), root.StringValue);
        var attrs=root.Attributes().GetEnumerator();Assert.True(attrs.MoveNext());Assert.Equal(root.StringValue,attrs.Current.NodeValue.StringValue);
    }
    /// <summary>Entity expansion, CDATA, comments and PIs preserve literal payloads.</summary>
    [Fact]
    public void ParsedPayloads_PreserveLiteralSentinels()
    {
        string xml="<!DOCTYPE r [<!ENTITY e '"+Literal+"'>]><r a='"+Literal+"'>&e;<![CDATA["+Literal+"]]><!--"+Literal+"--><?p "+Literal+"?></r>";
        var root=FirstElement(XDocumentProvider.ParseXml11(xml));
        Assert.Equal(Literal+Literal,root.StringValue);
        Assert.Contains("<!--"+Literal+"-->",root.ToXmlString());
        Assert.Contains("<?p "+Literal+"?>",root.ToXmlString());
    }
    /// <summary>Copied XML1.1 descendants keep logical names after leaving their source document.</summary>
    [Fact]
    public void CopiedDescendant_PreservesNamesAndValues()
    {
        string name=char.ConvertFromUtf32(119070);
        var source=FirstElement(XDocumentProvider.ParseXml11("<r><"+name+" a='"+Literal+"'>"+Literal+"</"+name+"></r>"));
        var child=FirstElement(source);
        var copy=XDocumentProvider.ConstructElement(new XdmElementSpec("out",null,null,Array.Empty<XdmAttributeValue>(),new[]{new XdmContentItem(XdmContentKind.Node,Node:XdmValue.FromNode(child))}));
        Assert.Equal(name,FirstElement(copy).LocalName);
        Assert.Contains("<"+name,copy.ToXmlString());Assert.Contains(Literal,copy.ToXmlString());
    }
    /// <summary>Document copies retain encoded names without decoding literal input names twice.</summary>
    [Fact]
    public void ConstructedDocument_PreservesNamesAndValues()
    {
        string name=char.ConvertFromUtf32(119070);
        var node=XDocumentProvider.ConstructElement(new XdmElementSpec(name,null,null,Array.Empty<XdmAttributeValue>(),new[]{new XdmContentItem(XdmContentKind.Text,Literal)}));
        var doc=XDocumentProvider.ConstructDocument(new[]{new XdmContentItem(XdmContentKind.Node,Node:XdmValue.FromNode(node))});
        Assert.Equal(name,FirstElement(doc).LocalName);Assert.Equal("<"+name+">"+Literal+"</"+name+">",doc.ToXmlString());
        Assert.Equal(name,FirstElement(XDocumentProvider.ParseXml11(doc.ToXmlString())).LocalName);
    }
    /// <summary>Unencoded parsed names are normalized exactly once when copied into encoded trees.</summary>
    [Fact]
    public void PlainLiteralName_CopyPreservesIdentity()
    {
        var source=XDocumentProvider.ParseXml("<"+Literal+">"+Literal+"</"+Literal+">");
        var child=FirstElement(source);
        var copy=XDocumentProvider.ConstructElement(new XdmElementSpec("r",null,null,Array.Empty<XdmAttributeValue>(),new[]{new XdmContentItem(XdmContentKind.Node,Node:XdmValue.FromNode(child))},Xml11Mode:true));
        Assert.Equal(Literal,FirstElement(copy).LocalName);Assert.Equal(Literal,FirstElement(copy).StringValue);
    }
    /// <summary>Free-standing constructed attributes use the same logical name/value representation.</summary>
    [Fact]
    public void ConstructedAttribute_PreservesNameAndValue()
    {
        string name=char.ConvertFromUtf32(119070);
        var attribute=XDocumentProvider.ConstructAttribute(new XdmAttributeValue(name,null,null,Literal));
        Assert.Equal(name,attribute.LocalName);Assert.Equal(Literal,attribute.StringValue);
    }
    private static IXdmNode FirstElement(IXdmNode node){var e=node.Children(XdmNodeKind.Element).GetEnumerator();Assert.True(e.MoveNext());return e.Current.NodeValue;}
}
