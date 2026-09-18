// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 September 2026
// PURPOSE              : Unit tests for opt-in record retention (tee/replay) over streamed input
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 17-09-2026     | Creation (Phase D4)                                                                      |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text;
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Runtime.Vm;
using Xunit;

namespace Bosak.XPath.Providers.Tests.Streaming;

/// <summary>
/// Tests for <see cref="StreamingLoadOptions.RetainRecords"/>: the pump memoizes every
/// materialized record so that crawling shapes (unions, <c>except</c>/<c>intersect</c>,
/// fork branches, multi-entry map constructors) can each enumerate the full record
/// stream. Without retention the stream stays forward-only (covered by
/// <see cref="XmlStreamingProviderTests"/> and <see cref="StreamingXPathTests"/>).
/// </summary>
public class StreamingRetentionTests
{
    private const string Doc = """
        <r><a>1</a><a>2</a><b>3</b><a>4</a><c>5</c></r>
        """;

    private static IXdmNode LoadRetained(string xml)
        => XmlStreamingProvider.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(xml)),
            new StreamingLoadOptions { RetainRecords = true });

    private static IXdmNode Root(IXdmNode doc)
    {
        foreach (var item in doc.Axis(XdmAxis.Child))
            return item.NodeValue!;
        throw new InvalidOperationException("no root");
    }

    private static List<string> LocalNames(XdmSequence sequence)
    {
        var list = new List<string>();
        foreach (var item in sequence)
            list.Add(item.NodeValue!.LocalName);
        return list;
    }

    [Fact]
    public void TwoEnumerationsBothSeeAllRecordsInOrder()
    {
        var doc = LoadRetained(Doc);
        var root = Root(doc);

        var first = LocalNames(root.Axis(XdmAxis.Child));
        var second = LocalNames(root.Axis(XdmAxis.Child));

        Assert.Equal(new[] { "a", "a", "b", "a", "c" }, first);
        Assert.Equal(first, second);
    }

    [Fact]
    public void InterleavedEnumerationYieldsConsistentViews()
    {
        var doc = LoadRetained(Doc);
        var root = Root(doc);
        var sequence = root.Axis(XdmAxis.Child);

        // Pass A starts and reads two records.
        var passA = sequence.GetEnumerator();
        Assert.True(passA.MoveNext());
        var a0 = passA.Current.NodeValue!.LocalName;
        Assert.True(passA.MoveNext());
        var a1 = passA.Current.NodeValue!.LocalName;

        // Pass B runs to completion while A is parked at index 2 (B drives the pump).
        var passB = LocalNames(sequence);
        Assert.Equal(new[] { "a", "a", "b", "a", "c" }, passB);

        // Pass A resumes: it must see the records B pumped, in order.
        var rest = new List<string>();
        while (passA.MoveNext())
            rest.Add(passA.Current.NodeValue!.LocalName);
        Assert.Equal(new[] { "a", "a" }, new[] { a0, a1 });
        Assert.Equal(new[] { "b", "a", "c" }, rest);
    }

    [Fact]
    public void DescendantAxisReplaysWithRetention()
    {
        var doc = LoadRetained(Doc);
        var expected = LocalNames(doc.Axis(XdmAxis.Descendant));
        var replay = LocalNames(doc.Axis(XdmAxis.Descendant));
        Assert.Equal(expected, replay);
        // Descendants include the root, every record, and each record's text children.
        Assert.Equal(new[] { "r", "a", "a", "b", "a", "c" },
            expected.Where(n => n.Length > 0).ToList());
        Assert.Equal(11, expected.Count); // r + 5 records + 5 text nodes
    }

    [Fact]
    public void KindFilteredReplaySeesAllRecords()
    {
        // Inter-record whitespace and a comment are records too; with retention a
        // kind-filtered second enumeration still sees every element record.
        var xml = "<r>\n  <a>1</a>\n  <!--note-->\n  <b>2</b>\n  <a>3</a>\n</r>";
        var doc = LoadRetained(xml);
        var root = Root(doc);

        var all = new List<IXdmNode>();
        foreach (var item in root.Children())
            all.Add(item.NodeValue!);
        Assert.Contains(all, n => n.NodeKind == XdmNodeKind.Text);
        Assert.Contains(all, n => n.NodeKind == XdmNodeKind.Comment);

        var elements = LocalNames(root.Children(XdmNodeKind.Element));
        Assert.Equal(new[] { "a", "b", "a" }, elements);
    }

    [Fact]
    public void StringValueReplaysRetainedRecords()
    {
        var doc = LoadRetained(Doc);
        var root = Root(doc);
        _ = LocalNames(root.Axis(XdmAxis.Child)); // pump to completion
        Assert.Equal("12345", root.StringValue);
    }

    [Fact]
    public void DrainAppendsAllRecordsToTheMemo()
    {
        var doc = LoadRetained(Doc);
        var streaming = (IStreamingDocument)doc;
        streaming.Drain();

        var root = Root(doc);
        Assert.Equal(new[] { "a", "a", "b", "a", "c" }, LocalNames(root.Axis(XdmAxis.Child)));
    }

    [Fact]
    public void RetainedReplayDoesNotGrowBeyondDocumentSize()
    {
        const int recordCount = 50_000;
        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write("<records>");
            for (int i = 0; i < recordCount; i++)
                writer.Write($"<record id=\"{i}\"><value>item-{i}</value></record>");
            writer.Write("</records>");
        }
        stream.Position = 0;

        var doc = XmlStreamingProvider.Load(stream, new StreamingLoadOptions { RetainRecords = true });
        var expr = XPath31Expression.Compile("/records/record/value");
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);

        long Count()
        {
            var result = expr.Evaluate(ctx);
            long count = 0;
            foreach (var _ in XdmSequence.FromSource(result.SequenceValue!))
                count++;
            return count;
        }

        Assert.Equal(recordCount, Count());
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long before = GC.GetTotalMemory(true);

        // The second pass replays the memo: growth stays far below re-parsing the
        // input, confirming retention holds each record once (document-size semantics)
        // rather than leaking per enumeration.
        Assert.Equal(recordCount, Count());
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long growth = GC.GetTotalMemory(true) - before;

        Assert.True(growth < 10_000_000, $"replay growth {growth / 1_000_000.0:F1} MB exceeds the retained-replay budget");
    }

    [Fact]
    public void UnionOverRetainedStreamedDocument()
    {
        Assert.Equal(4, Eval("count(//a | //b)"));
    }

    [Fact]
    public void UnionResultIsInDocumentOrder()
    {
        // Document order of (//a | //b) over <r><a>1</a><a>2</a><b>3</b><a>4</a>...
        Assert.Equal("a,a,b,a", EvalString("string-join((//a | //b) ! name(), ',')"));
    }

    [Fact]
    public void ExceptOverRetainedStreamedDocument()
    {
        // 6 elements (r, a, a, b, a, c) minus the single b.
        Assert.Equal(5, Eval("count(//* except //b)"));
    }

    [Fact]
    public void IntersectOverRetainedStreamedDocument()
    {
        Assert.Equal(3, Eval("count((//a | //b) intersect //a)"));
    }

    private static long Eval(string xpath)
    {
        var doc = LoadRetained(Doc);
        var expr = XPath31Expression.Compile(xpath);
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);
        return expr.Evaluate(ctx).IntegerValue;
    }

    private static string EvalString(string xpath)
    {
        var doc = LoadRetained(Doc);
        var expr = XPath31Expression.Compile(xpath);
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);
        return expr.Evaluate(ctx).StringValue;
    }
}
