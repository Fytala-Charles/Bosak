// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 september 2026
// PURPOSE              : Unit tests for lazy record replay enablement (IStreamingDocument.EnableReplay).
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
/// Tests for <see cref="IStreamingDocument.EnableReplay"/>: a not-yet-started stream can
/// be opted into record retention so later enumerations replay the full record stream;
/// a partially consumed stream cannot.
/// </summary>
public class StreamingEnableReplayTests
{
    private const string Doc = """
        <r><a>1</a><a>2</a><b>3</b></r>
        """;

    private static IXdmNode Load(string xml)
        => XmlStreamingProvider.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

    private static List<string> ChildNames(IXdmNode doc)
    {
        var names = new List<string>();
        foreach (var item in doc.Axis(XdmAxis.Child))
        {
            var root = item.NodeValue!;
            foreach (var child in root.Axis(XdmAxis.Child))
                names.Add(child.NodeValue!.StringValue);
        }
        return names;
    }

    [Fact]
    public void EnableReplay_BeforeStart_AllowsSecondEnumeration()
    {
        var doc = Load(Doc);
        var streamingDoc = Assert.IsAssignableFrom<IStreamingDocument>(
            doc.NodeKind == XdmNodeKind.Document ? doc : doc.Document!);

        Assert.True(streamingDoc.EnableReplay());
        Assert.Equal(new[] { "1", "2", "3" }, ChildNames(doc));
        // Idempotent and replayable.
        Assert.True(streamingDoc.EnableReplay());
        Assert.Equal(new[] { "1", "2", "3" }, ChildNames(doc));
    }

    [Fact]
    public void EnableReplay_AfterPartialConsumption_ReturnsFalse()
    {
        var doc = Load(Doc);
        var streamingDoc = (IStreamingDocument)doc;

        // Partially consume the stream.
        var enumerator = doc.Axis(XdmAxis.Child).GetEnumerator();
        Assert.True(enumerator.MoveNext());
        var root = enumerator.Current.NodeValue!;
        var childEnumerator = root.Axis(XdmAxis.Child).GetEnumerator();
        Assert.True(childEnumerator.MoveNext());

        Assert.False(streamingDoc.EnableReplay());
    }
}
