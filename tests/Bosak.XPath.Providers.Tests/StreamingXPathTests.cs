// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 September 2026
// PURPOSE              : Unit tests for XPath evaluation over streamed (single-pass) input
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
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 21-09-2026     | fn:copy-of deep-copy guard tests: streamed record/document/attribute copies are        |
//                      |                  |       |                | grounded and independent; non-streaming copy-of path unchanged                           |
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
/// Tests for the VM's single-pass (streamed) evaluation paths: lazy axis maps, filters,
/// path steps, sequence normalization passthrough, and bounded-memory enumeration.
/// </summary>
public class StreamingXPathTests
{
    private const string Doc = """
        <inventory count="3">
        <product id="1"><name>Hammer</name><price>9.99</price></product>
        <product id="2"><name>Saw</name><price>19.99</price></product>
        <product id="3"><name>Drill</name><price>29.99</price></product>
        </inventory>
        """;

    private static XdmValue Eval(string xpath, string xml)
    {
        var doc = XmlStreamingProvider.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
        var expr = XPath31Expression.Compile(xpath);
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);
        return expr.Evaluate(ctx);
    }

    private static List<XdmValue> Items(XdmValue result)
    {
        var list = new List<XdmValue>();
        if (result.IsSequence && result.SequenceValue is not null)
            foreach (var v in XdmSequence.FromSource(result.SequenceValue)) list.Add(v);
        else if (!result.IsUndefined)
            list.Add(result);
        return list;
    }

    [Fact]
    public void RecordPath()
    {
        Assert.Equal(3, Items(Eval("/inventory/product", Doc)).Count);
    }

    [Fact]
    public void DeeperPath()
    {
        var names = Items(Eval("/inventory/product/name", Doc)).Select(v => v.NodeValue!.StringValue);
        Assert.Equal(new[] { "Hammer", "Saw", "Drill" }, names);
    }

    [Fact]
    public void DescendantScan()
    {
        var names = Items(Eval("//name", Doc)).Select(v => v.NodeValue!.StringValue);
        Assert.Equal(new[] { "Hammer", "Saw", "Drill" }, names);
    }

    [Fact]
    public void DescendantScanWithChildStep()
    {
        var names = Items(Eval("//product/name", Doc)).Select(v => v.NodeValue!.StringValue);
        Assert.Equal(new[] { "Hammer", "Saw", "Drill" }, names);
    }

    [Fact]
    public void PredicateFilter()
    {
        var result = Items(Eval("/inventory/product[@id='2']/name", Doc));
        var one = Assert.Single(result);
        Assert.Equal("Saw", one.NodeValue!.StringValue);
    }

    [Fact]
    public void PositionalPredicate()
    {
        var result = Items(Eval("/inventory/product[2]/name", Doc));
        var one = Assert.Single(result);
        Assert.Equal("Saw", one.NodeValue!.StringValue);
    }

    [Fact]
    public void PositionPredicate()
    {
        var result = Items(Eval("/inventory/product[position() = 3]/name", Doc));
        var one = Assert.Single(result);
        Assert.Equal("Drill", one.NodeValue!.StringValue);
    }

    [Fact]
    public void LastSubscriptReturnsFinalRecord()
    {
        // [last()] must drain the stream to find the final item: unbounded but correct.
        var result = Items(Eval("/inventory/product[last()]/name", Doc));
        var one = Assert.Single(result);
        Assert.Equal("Drill", one.NodeValue!.StringValue);
    }

    [Fact]
    public void LastFunctionRaisesStreamingError()
    {
        // fn:last() in a predicate reads the (unknown) context size of a streamed focus.
        var ex = Assert.Throws<InvalidOperationException>(
            () => Items(Eval("/inventory/product[position() = last()]/name", Doc)));
        Assert.Contains("fn:last()", ex.Message);
    }

    [Fact]
    public void CountOverStream()
    {
        // fn:count buffers the stream: unbounded but correct.
        Assert.Equal(3, Eval("count(/inventory/product)", Doc).IntegerValue);
    }

    [Fact]
    public void SimpleMapOperator()
    {
        var values = Items(Eval("/inventory/product/name ! string(.)", Doc)).Select(v => v.StringValue);
        Assert.Equal(new[] { "Hammer", "Saw", "Drill" }, values);
    }

    [Fact]
    public void LastStepAtomics()
    {
        var values = Items(Eval("/inventory/product/name/string()", Doc)).Select(v => v.StringValue);
        Assert.Equal(new[] { "Hammer", "Saw", "Drill" }, values);
    }

    [Fact]
    public void FlorExpression()
    {
        var values = Items(Eval("for $p in /inventory/product return $p/name", Doc))
            .Select(v => v.NodeValue!.StringValue);
        Assert.Equal(new[] { "Hammer", "Saw", "Drill" }, values);
    }

    [Fact]
    public void ReverseAxisInsideRecord()
    {
        // Parent/ancestor navigation within a record stays available over streamed input.
        var roots = Items(Eval("/inventory/product/name/parent::product/@id", Doc))
            .Select(v => v.NodeValue!.StringValue);
        Assert.Equal(new[] { "1", "2", "3" }, roots);
    }

    [Fact]
    public void SecondReadOfStreamRaises()
    {
        var doc = XmlStreamingProvider.Load(new MemoryStream(Encoding.UTF8.GetBytes(Doc)));
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);
        _ = Items(XPath31Expression.Compile("/inventory/product").Evaluate(ctx));

        var ex = Assert.ThrowsAny<Exception>(
            () => Items(XPath31Expression.Compile("/inventory/product").Evaluate(ctx)));
        Assert.True(ex is StreamingException or InvalidOperationException);
        Assert.Contains("single pass", ex.Message);
    }

    [Fact]
    public void BoundedMemoryOverLargeStream()
    {
        const int recordCount = 500_000;
        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write("<records>");
            for (int i = 0; i < recordCount; i++)
                writer.Write($"<record id=\"{i}\"><value>item-{i}</value></record>");
            writer.Write("</records>");
        }
        stream.Position = 0;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long before = GC.GetTotalMemory(true);

        var doc = XmlStreamingProvider.Load(stream);
        var expr = XPath31Expression.Compile("/records/record/value");
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);
        var result = expr.Evaluate(ctx);

        long count = 0;
        foreach (var _ in XdmSequence.FromSource(result.SequenceValue!))
            count++;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long growth = GC.GetTotalMemory(true) - before;

        Assert.Equal(recordCount, count);
        // Bounded input: live growth stays far below the size of the parsed input tree
        // (which would be several hundred MB for 500k records).
        Assert.True(growth < 50_000_000, $"live growth {growth / 1_000_000.0:F1} MB exceeds the bounded-memory budget");
    }

    [Fact]
    public void CopyOfStreamedRecordYieldsGroundedIndependentCopy()
    {
        var doc = XmlStreamingProvider.Load(new MemoryStream(Encoding.UTF8.GetBytes(Doc)));
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);

        var copy = Assert.Single(Items(XPath31Expression.Compile("fn:copy-of(/inventory/product[1])").Evaluate(ctx))).NodeValue!;

        // The copy must be a grounded node, not the live streamed wrapper.
        Assert.IsNotAssignableFrom<IStreamingNode>(copy);
        Assert.Equal(XdmNodeKind.Element, copy.NodeKind);

        // Content parity with the streamed original (evaluated over a fresh stream,
        // since a streamed source is single-pass).
        var original = Assert.Single(Items(Eval("/inventory/product[1]", Doc))).NodeValue!;
        var copyElement = Assert.IsAssignableFrom<System.Xml.Linq.XElement>(
            ((Bosak.XPath.Providers.Xml.XDocumentNode)copy).UnderlyingObject);
        var originalXml = original.ToXmlString();
        Assert.Equal(originalXml, copy.ToXmlString());

        // Mutating the copy's tree must leave the (separately read) original unchanged.
        copyElement.SetAttributeValue("mutated", "yes");
        Assert.Contains("mutated", copy.ToXmlString());
        Assert.Equal(originalXml, original.ToXmlString());
    }

    [Fact]
    public void CopyOfStreamedDocumentDrainsAndGrounds()
    {
        var doc = XmlStreamingProvider.Load(new MemoryStream(Encoding.UTF8.GetBytes(Doc)));
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);

        var copy = Assert.Single(Items(XPath31Expression.Compile("fn:copy-of(/)").Evaluate(ctx))).NodeValue!;
        Assert.IsNotAssignableFrom<IStreamingNode>(copy);
        Assert.Equal(XdmNodeKind.Document, copy.NodeKind);

        // The grounded copy holds the full drained content.
        var copyCtx = CopyContext(copy);
        var names = Items(XPath31Expression.Compile("/inventory/product/name/string()").Evaluate(copyCtx))
            .Select(v => v.StringValue);
        Assert.Equal(new[] { "Hammer", "Saw", "Drill" }, names);
    }

    [Fact]
    public void CopyOfStreamedAttributeIsGrounded()
    {
        var doc = XmlStreamingProvider.Load(new MemoryStream(Encoding.UTF8.GetBytes(Doc)));
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);

        var copy = Assert.Single(Items(XPath31Expression.Compile("fn:copy-of(/inventory/product[1]/@id)").Evaluate(ctx))).NodeValue!;
        Assert.IsNotAssignableFrom<IStreamingNode>(copy);
        Assert.Equal(XdmNodeKind.Attribute, copy.NodeKind);
        Assert.Equal("1", copy.StringValue);
    }

    [Fact]
    public void CopyOfNonStreamingNodeUsesUnchangedFastPath()
    {
        var inMemory = Bosak.XPath.Providers.Xml.XDocumentProvider.ParseXml(Doc);
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(inMemory), 1, 1);

        var original = Assert.Single(Items(XPath31Expression.Compile("/inventory/product[1]").Evaluate(ctx))).NodeValue!;
        var copy = Assert.Single(Items(XPath31Expression.Compile("fn:copy-of(/inventory/product[1])").Evaluate(ctx))).NodeValue!;

        Assert.False(copy.IsSameNode(original));
        Assert.Equal(original.ToXmlString(), copy.ToXmlString());
        Assert.Equal("1", Assert.Single(Items(XPath31Expression.Compile("@id/string()").Evaluate(CopyContext(copy)))).StringValue);
    }

    [Fact]
    public void CopyOfStreamedDocumentPreservesUnparsedEntities()
    {
        // The grounded document copy must carry the DTD unparsed-entity declarations
        // (and the DOCTYPE) exactly like the in-memory deep-copy path (sf-unparsed-entity-07).
        const string xml = """
            <!DOCTYPE r [
            <!NOTATION gif SYSTEM "gifviewer">
            <!ENTITY hatch-pic SYSTEM "grafix/OpenHatch.gif" NDATA gif>
            ]>
            <r><a/></r>
            """;
        var doc = XmlStreamingProvider.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(xml)),
            new StreamingLoadOptions { BaseUri = "http://example.org/base/" });
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(doc), 1, 1);

        var copy = Assert.Single(Items(XPath31Expression.Compile("fn:copy-of(/)").Evaluate(ctx))).NodeValue!;
        Assert.True(copy.HasDocumentType);
        Assert.Equal("r", copy.DocumentTypeName);

        // fn:unparsed-entity-uri is XSLT-only, so assert through the node API.
        Assert.True(copy.TryGetUnparsedEntity("hatch-pic", out var systemId, out _));
        Assert.EndsWith("grafix/OpenHatch.gif", systemId);
    }

    private static EvaluationContext CopyContext(IXdmNode node)
    {
        var ctx = new EvaluationContext();
        ctx.WithFocus(XdmValue.FromNode(node), 1, 1);
        return ctx;
    }
}
