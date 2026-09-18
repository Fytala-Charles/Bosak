// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 17 September 2026
// PURPOSE              : Unit tests for XPTY0018/XPTY0019 step-result checks and unary operand atomization on final path steps
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
using Bosak.XPath.Api;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Bosak.XPath.Providers.Xml;
using Xunit;

namespace Bosak.XPath.Runtime.Tests;

public class PathStepAtomicResultTests
{
    private const string TransactionsXml =
        "<account>" +
        "<transaction date='2019-01-01' value='1.0'/>" +
        "<transaction date='2021-01-01' value='3.32'/>" +
        "</account>";

    private static IXdmNode PlainNode()
        => new XDocumentNode(new System.Xml.Linq.XDocument(System.Xml.Linq.XElement.Parse(TransactionsXml)));

    private static IXdmNode StreamedNode(bool retainRecords)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(TransactionsXml));
        return XmlStreamingProvider.Load(stream, new StreamingLoadOptions { RetainRecords = retainRecords });
    }

    private static List<string> EvaluateToStrings(string expression, IXdmNode context)
    {
        var result = XPath31Expression.Compile(expression).Evaluate(context);
        var items = new List<string>();
        if (result.IsSequence && result.SequenceValue is not null)
        {
            foreach (var item in XdmSequence.FromSource(result.SequenceValue))
                items.Add(item.IsNode ? $"node:{item.NodeValue!.NodeKind}" : item.ToString() ?? "?");
        }
        else if (!result.IsUndefined)
        {
            items.Add(result.IsNode ? $"node:{result.NodeValue!.NodeKind}" : result.ToString() ?? "?");
        }
        return items;
    }

    [Fact]
    public void LastStepParenthesizedAtomics_YieldsNumbers()
    {
        // sx-gc-*-021 shape: the final step computes atomic values per context item
        // (+@value atomizes the attribute; the other branch computes @value+1).
        var items = EvaluateToStrings(
            "account/transaction/(if (@date = '2019-01-01') then +@value else (@value+1))",
            PlainNode());
        Assert.Equal(new[] { "1", "4.32" }, items);
    }

    [Fact]
    public void LastStepParenthesizedAtomics_OverStreamedSinglePass_YieldsNumbers()
    {
        var items = EvaluateToStrings(
            "account/transaction/(if (@date = '2019-01-01') then +@value else (@value+1))",
            StreamedNode(retainRecords: false));
        Assert.Equal(new[] { "1", "4.32" }, items);
    }

    [Fact]
    public void LastStepParenthesizedAtomics_OverStreamedReplay_YieldsNumbers()
    {
        // D4 record retention replays the record stream as a normal (multi-pass)
        // sequence; the final-step atomics must be allowed on that path too.
        var items = EvaluateToStrings(
            "account/transaction/(if (@date = '2019-01-01') then +@value else (@value+1))",
            StreamedNode(retainRecords: true));
        Assert.Equal(new[] { "1", "4.32" }, items);
    }

    [Fact]
    public void UnaryPlusOnAttribute_AtomizesToNumber()
    {
        var items = EvaluateToStrings("+account/transaction[1]/@value", PlainNode());
        Assert.Equal(new[] { "1" }, items);
    }

    [Fact]
    public void UnaryMinusOnAttribute_AtomizesToNumber()
    {
        var items = EvaluateToStrings("-account/transaction[1]/@value", PlainNode());
        Assert.Equal(new[] { "-1" }, items);
    }

    [Fact]
    public void MixedFinalStep_StillRaisesXpty0018()
    {
        // XPTY0018 is defined for the LAST step mixing nodes and atomic values
        // (XSLT expression-0932/0933): must not be over-suppressed.
        var ex = Assert.Throws<InvalidOperationException>(() =>
            EvaluateToStrings(
                "account/transaction/(if (@date = '2019-01-01') then @value else 2)",
                PlainNode()));
        Assert.Contains("XPTY0018", ex.Message);
    }

    [Fact]
    public void IntermediateStepAtomics_RaiseXpty0019()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            EvaluateToStrings("account/transaction/(@value + 0)/2", PlainNode()));
        Assert.Contains("XPTY0019", ex.Message);
    }

    [Fact]
    public void IntermediateStepAtomics_OverStreamedReplay_RaiseXpty0019()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            EvaluateToStrings("account/transaction/(@value + 0)/2", StreamedNode(retainRecords: true)));
        Assert.Contains("XPTY0019", ex.Message);
    }

    [Fact]
    public void NestedPath_LastStepAtomics_YieldsNumbers()
    {
        var items = EvaluateToStrings(
            "account/(transaction/(if (@date = '2019-01-01') then +@value else (@value+1)))",
            PlainNode());
        Assert.Equal(new[] { "1", "4.32" }, items);
    }

    [Fact]
    public void LastStepPureNodeSequence_RemainsNodes()
    {
        var items = EvaluateToStrings("account/transaction/@value", PlainNode());
        Assert.Equal(new[] { "node:Attribute", "node:Attribute" }, items);
    }
}
