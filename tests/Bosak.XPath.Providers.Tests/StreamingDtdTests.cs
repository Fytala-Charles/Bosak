// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 september 2026
// PURPOSE              : Unit tests for DTD / unparsed-entity support in the streaming provider.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 17-09-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Xunit;

namespace Bosak.XPath.Providers.Tests.Streaming;

/// <summary>
/// Tests for DOCTYPE handling and unparsed-entity lookup over streamed documents
/// (sf-unparsed-entity-01..08).
/// </summary>
public class StreamingDtdTests
{
    private const string DtdDoc = """
        <?xml version="1.0"?>
        <!DOCTYPE doc [
          <!NOTATION gif SYSTEM "http://www.gif.com" >
          <!ENTITY hatch-pic SYSTEM "../grafix/OpenHatch.gif" NDATA gif >
          <!ENTITY watch-pic PUBLIC "-//Textuality//TEXT standard boilerplate//EN" "../grafix/OpenWatch.gif" NDATA gif >
          <!ELEMENT doc (account+)>
          <!ELEMENT account (#PCDATA)>
        ]>
        <doc>
          <account>one</account>
          <account>two</account>
        </doc>
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
    public void DocumentWithDoctype_Loads()
    {
        var doc = LoadXml(DtdDoc, new StreamingLoadOptions { BaseUri = "http://example.org/strm/a.xml" });

        Assert.True(doc.HasDocumentType);
        Assert.Equal("doc", doc.DocumentTypeName);
    }

    [Fact]
    public void ShellRoot_LooksUpUnparsedEntities()
    {
        var doc = LoadXml(DtdDoc, new StreamingLoadOptions { BaseUri = "http://example.org/strm/a.xml" });
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];

        Assert.True(root.TryGetUnparsedEntity("hatch-pic", out var systemId, out var publicId));
        Assert.Equal("http://example.org/grafix/OpenHatch.gif", systemId);
        Assert.Null(publicId);

        Assert.True(root.TryGetUnparsedEntity("watch-pic", out systemId, out publicId));
        Assert.Equal("http://example.org/grafix/OpenWatch.gif", systemId);
        Assert.Equal("-//Textuality//TEXT standard boilerplate//EN", publicId);
    }

    [Fact]
    public void RecordNode_LooksUpUnparsedEntities()
    {
        var doc = LoadXml(DtdDoc, new StreamingLoadOptions { BaseUri = "http://example.org/strm/a.xml" });
        var root = Nodes(doc.Axis(XdmAxis.Child))[0];
        var record = Nodes(root.Axis(XdmAxis.Child))[0];

        // Record nodes live in detached trees; the lookup must fall back to the
        // streamed document's DTD.
        Assert.True(record.TryGetUnparsedEntity("hatch-pic", out var systemId, out _));
        Assert.Equal("http://example.org/grafix/OpenHatch.gif", systemId);
        Assert.False(record.TryGetUnparsedEntity("missing", out _, out _));
    }
}
