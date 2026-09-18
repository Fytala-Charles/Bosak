// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Unit tests for the XmlStreamingProvider burst-mode streaming input provider
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-09-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 18-09-2026     | Updated DTD default tests: DTD now parsed by default; Prohibit remains configurable        |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using System.Xml;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Xunit;

namespace Bosak.XPath.Providers.Tests.Streaming;

/// <summary>
/// Tests for <see cref="XmlStreamingProvider"/>: pump mechanics, parent/document chains,
/// node identity, document order, namespace inheritance, single-pass enforcement, and
/// cross-record navigation guards.
/// </summary>
public class XmlStreamingProviderTests
{
    private const string Doc = """
        <?xml version="1.0"?>
        <inventory xmlns:p="urn:products" count="3">
        <product id="1"><name>Hammer</name><p:price>9.99</p:price></product>
        <product id="2"><name>Saw</name><p:price>19.99</p:price></product>
        <text-note>hello</text-note>
        </inventory>
        """;

    private static IXdmNode LoadXml(string xml, StreamingLoadOptions? options = null)
        => XmlStreamingProvider.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)), options);

    private static List<IXdmNode> Nodes(XdmSequence sequence)
    {
        var list = new List<IXdmNode>();
        foreach (var item in sequence)
            if (item.IsNode) list.Add(item.NodeValue!);
        return list;
    }

    [Fact]
    public void DocumentAndRootStructure()
    {
        var doc = LoadXml(Doc, new StreamingLoadOptions { BaseUri = "http://example.org/s.xml" });

        Assert.Equal(XdmNodeKind.Document, doc.NodeKind);
        Assert.Equal("http://example.org/s.xml", doc.BaseUri);
        Assert.Equal("http://example.org/s.xml", doc.DocumentUri);

        var root = Assert.Single(Nodes(doc.Axis(XdmAxis.Child)));
        Assert.Equal("inventory", root.LocalName);
        Assert.Equal("3", Assert.Single(Nodes(root.Attributes("count"))).StringValue);
        Assert.True(root.Parent!.IsSameNode(doc));
        Assert.True(root.Document!.IsSameNode(doc));
    }

    [Fact]
    public void RecordsHaveParentAndDocument()
    {
        var doc = LoadXml(Doc);
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        var records = Nodes(root.Axis(XdmAxis.Child));
        var elements = records.Where(r => r.NodeKind == XdmNodeKind.Element).ToList();

        Assert.Equal(3, elements.Count);
        Assert.Equal(new[] { "product", "product", "text-note" }, elements.Select(e => e.LocalName));
        foreach (var record in elements)
        {
            Assert.True(record.Parent!.IsSameNode(root));
            Assert.True(record.Document!.IsSameNode(doc));
        }
    }

    [Fact]
    public void InRecordNavigationAndIdentity()
    {
        var doc = LoadXml(Doc);
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        var rec1 = Nodes(root.Axis(XdmAxis.Child)).First(r => r.NodeKind == XdmNodeKind.Element);

        var name = Nodes(rec1.Axis(XdmAxis.Child)).First(v => v.LocalName == "name");
        Assert.Equal("Hammer", name.StringValue);
        Assert.True(name.Parent!.IsSameNode(rec1));
        Assert.True(rec1.IsSameNode(name.Parent!));

        var ancestors = Nodes(name.Axis(XdmAxis.Ancestor));
        Assert.Equal(3, ancestors.Count);
        Assert.True(ancestors[0].IsSameNode(rec1));
        Assert.True(ancestors[1].IsSameNode(root));
        Assert.True(ancestors[2].IsSameNode(doc));
    }

    [Fact]
    public void NamespaceDeclaredOnRootIsInheritedIntoRecords()
    {
        var doc = LoadXml(Doc);
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        var rec1 = Nodes(root.Axis(XdmAxis.Child)).First(r => r.NodeKind == XdmNodeKind.Element);

        var price = Nodes(rec1.Axis(XdmAxis.Descendant)).First(v => v.LocalName == "price");
        Assert.Equal("urn:products", price.NamespaceUri);
        Assert.Equal("p", price.Prefix);
    }

    [Fact]
    public void DocumentOrderIncreasesAcrossRecords()
    {
        var doc = LoadXml(Doc);
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        var orders = Nodes(root.Axis(XdmAxis.Child)).Select(r => r.DocumentOrder).ToList();

        for (int i = 1; i < orders.Count; i++)
            Assert.True(orders[i - 1] < orders[i], $"order {i - 1} >= {i}");
        Assert.True(root.DocumentOrder < orders[0]);
        Assert.True(doc.DocumentOrder < root.DocumentOrder);
    }

    [Fact]
    public void SecondEnumerationOfRootChildrenThrows()
    {
        var doc = LoadXml(Doc);
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        _ = Nodes(root.Axis(XdmAxis.Child));

        var ex = Assert.Throws<StreamingException>(() => Nodes(root.Axis(XdmAxis.Child)));
        Assert.Contains("forward-only", ex.Message);
    }

    [Fact]
    public void PrecedingAcrossRecordsThrows()
    {
        var doc = LoadXml(Doc);
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        var elements = Nodes(root.Axis(XdmAxis.Child)).Where(r => r.NodeKind == XdmNodeKind.Element).ToList();

        Assert.Throws<StreamingException>(() => Nodes(elements[1].Axis(XdmAxis.Preceding)));
    }

    [Fact]
    public void FollowingSiblingThrowsExceptForLastRecord()
    {
        var doc = LoadXml(Doc);
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        var records = Nodes(root.Axis(XdmAxis.Child));

        // The pump is done (records were fully enumerated); released records still throw,
        // only the genuinely-last top-level node has an empty (complete) answer.
        Assert.Throws<StreamingException>(() => Nodes(records[0].Axis(XdmAxis.FollowingSibling)));
        Assert.Empty(Nodes(records[^1].Axis(XdmAxis.FollowingSibling)));
    }

    [Fact]
    public void EmptyRootYieldsNoChildren()
    {
        var doc = LoadXml("<empty/>");
        var root = Assert.Single(Nodes(doc.Axis(XdmAxis.Child)));
        Assert.Empty(Nodes(root.Axis(XdmAxis.Child)));
    }

    [Fact]
    public void RootStringValueConsumesTheStream()
    {
        var doc = LoadXml("<r><a>x</a><b>y</b></r>");
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        Assert.Equal("xy", root.StringValue);
    }

    [Fact]
    public void WhitespaceTextRecordsAreSurfacedInOrder()
    {
        var doc = LoadXml("<r>  <a/>  </r>");
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        var kids = Nodes(root.Axis(XdmAxis.Child));

        Assert.Equal(3, kids.Count);
        Assert.Equal(XdmNodeKind.Text, kids[0].NodeKind);
        Assert.Equal(XdmNodeKind.Element, kids[1].NodeKind);
        Assert.Equal(XdmNodeKind.Text, kids[2].NodeKind);
        Assert.True(kids[0].Parent!.IsSameNode(kids[1].Parent!));
        Assert.True(kids[0].DocumentOrder < kids[1].DocumentOrder);
        Assert.True(kids[1].DocumentOrder < kids[2].DocumentOrder);
    }

    [Fact]
    public void MalformedInputThrowsXmlExceptionWithPosition()
    {
        var doc = LoadXml("<r><a></b></r>");
        var ex = Assert.Throws<XmlException>(() => Nodes(doc.Axis(XdmAxis.Descendant)));
        Assert.True(ex.LineNumber >= 1);
    }

    [Fact]
    public void DtdIsParsedByDefault()
    {
        var doc = LoadXml("<!DOCTYPE r [<!ELEMENT r ANY>]><r/>");

        Assert.True(doc.HasDocumentType);
        Assert.Equal("r", doc.DocumentTypeName);
    }

    [Fact]
    public void DtdCanBeProhibitedViaReaderSettings()
    {
        var options = new StreamingLoadOptions
        {
            ReaderSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit },
        };

        Assert.Throws<XmlException>(() => LoadXml("<!DOCTYPE r [<!ELEMENT r ANY>]><r/>", options));
    }

    [Fact]
    public void DtdPropertiesAreCapturedWhenEnabled()
    {
        var options = new StreamingLoadOptions
        {
            ReaderSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse },
        };
        var doc = LoadXml(
            "<!DOCTYPE r PUBLIC \"-//EXAMPLE//DTD//EN\" \"http://example.org/r.dtd\" [<!ELEMENT r ANY>]><r/>",
            options);

        Assert.True(doc.HasDocumentType);
        Assert.Equal("r", doc.DocumentTypeName);
        Assert.Equal("-//EXAMPLE//DTD//EN", doc.PublicId);
        Assert.Equal("http://example.org/r.dtd", doc.SystemId);
        Assert.Equal("<!ELEMENT r ANY>", doc.InternalSubset.Trim());
    }

    [Fact]
    public void RootAttributesAndNamespacesAreAvailableBeforeStreaming()
    {
        var doc = LoadXml("<r xmlns=\"urn:default\" a=\"1\" xmlns:p=\"urn:p\" p:b=\"2\"><x/></r>");
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];

        Assert.Equal("urn:default", root.NamespaceUri);
        var attrs = Nodes(root.Axis(XdmAxis.Attribute));
        Assert.Equal(2, attrs.Count);
        Assert.Equal("1", attrs.First(a => a.LocalName == "a").StringValue);
        Assert.Equal("2", attrs.First(a => a.LocalName == "b").StringValue);
        Assert.Equal("urn:p", attrs.First(a => a.LocalName == "b").NamespaceUri);
    }

    [Fact]
    public void BaseUriAndDocumentUriComeFromOptions()
    {
        var doc = LoadXml("<r><x/></r>", new StreamingLoadOptions
        {
            BaseUri = "http://example.org/base/",
            DocumentUri = "http://example.org/doc.xml",
        });

        Assert.Equal("http://example.org/base/", doc.BaseUri);
        Assert.Equal("http://example.org/doc.xml", doc.DocumentUri);

        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        Assert.Equal("http://example.org/doc.xml", root.DocumentUri);
        var record = Assert.Single(Nodes(root.Axis(XdmAxis.Child)));
        Assert.Equal("http://example.org/doc.xml", record.DocumentUri);
    }

    [Fact]
    public void CommentsAndProcessingInstructionsAreStreamedAsRecords()
    {
        var doc = LoadXml("<r><!--note--><?pi data?><a/></r>");
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        var kids = Nodes(root.Axis(XdmAxis.Child));

        Assert.Equal(new[] { XdmNodeKind.Comment, XdmNodeKind.ProcessingInstruction, XdmNodeKind.Element },
            kids.Select(k => k.NodeKind));
        Assert.Equal("note", kids[0].StringValue);
        Assert.Equal("pi", kids[1].LocalName);
    }
}
