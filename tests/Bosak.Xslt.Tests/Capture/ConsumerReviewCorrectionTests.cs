// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 11 October 2026
// PURPOSE              : Verifies C-F1 bounded byte production and D-F1 terminal execution control.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 11-10-2026     | Creation: REQ-125 consumer C-F1/D-F1 regression evidence.                                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Bosak.Xslt.Capture;
using Bosak.Xslt.Runtime;
using Bosak.Xslt.Stylesheet;
using Xunit;

namespace Bosak.Xslt.Tests.Capture;

/// <summary>Consumer-review regression tests with measured encoded-byte production.</summary>
public class ConsumerReviewCorrectionTests
{
    private static OutputProperties Props(string method = "text", string encoding = "UTF-8", bool bom = false)
        => new()
        {
            Method = method, MethodSpecified = true,
            Encoding = encoding, ByteOrderMark = bom, ByteOrderMarkSpecified = true,
        };

    private static XdmValue Tree(string text) => XdmValue.FromNode(new XDocumentNode(
        new XDocument(new XElement("out", text))));

    /// <summary>CF1 PublicMillionByteProbe IsClassifiedWithoutRetainingOutput.</summary>
    [Fact]
    public void CF1_PublicMillionByteProbe_IsClassifiedWithoutRetainingOutput()
    {
        var executable = new XsltCompiler().Compile("""
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
              <xsl:output method="text"/>
              <xsl:template match="/">
                <xsl:value-of select="string-join(for $i in 1 to 1000000 return 'x', '')"/>
              </xsl:template>
            </xsl:stylesheet>
            """, "file:///review/main.xsl");
        var result = executable.CaptureTransform(new XDocumentNode(new XDocument(new XElement("doc"))),
            new XsltCaptureOptions { MaxBytesPerOutput = 1 });
        Assert.False(result.IsSuccess);
        Assert.Null(result.Principal);
        Assert.Equal("XV0007", Assert.Single(result.Failures).Code);
        Assert.Contains("during production", result.Failures[0].Message);
    }

    /// <summary>CF1 LeadingFeff PreservesLegacyBomAccounting.</summary>
    /// <param name="encoding">The test scenario value.</param>
    /// <param name="bom">The test scenario value.</param>
    [Theory]
    [InlineData("UTF-8", true)]
    [InlineData("UTF-16BE", true)]
    [InlineData("UTF-8", false)]
    public void CF1_LeadingFeff_PreservesLegacyBomAccounting(string encoding, bool bom)
    {
        var value = Tree("\uFEFFtext");
        Assert.Equal(ResultTreeSerializer.SerializeToBytes(value, Props(encoding: encoding, bom: bom)),
            ResultTreeSerializer.SerializeCaptureBytes(value, Props(encoding: encoding, bom: bom), 100, null));
    }

    /// <summary>CF1 TextCharacterMapExpansion IsBoundedDuringMapping.</summary>
    [Fact]
    public void CF1_TextCharacterMapExpansion_IsBoundedDuringMapping()
    {
        var props = Props();
        props.CharacterMap = new Dictionary<int, string> { ['x'] = new string('y', 1_000_000) };
        long produced = 0;
        Assert.Throws<SerializationByteLimitException>(() => ResultTreeSerializer.SerializeCaptureBytes(Tree("x"),
            props, 1, null, observeProduction: n => produced = n));
        Assert.InRange(produced, 2, 16);
    }

    /// <summary>CF1 NonTextCharacterMaps AreRefusedBeforeProduction.</summary>
    /// <param name="method">The test scenario value.</param>
    [Theory]
    [InlineData("xml")]
    [InlineData("html")]
    [InlineData("xhtml")]
    public void CF1_NonTextCharacterMaps_AreRefusedBeforeProduction(string method)
    {
        long produced = 0;
        var props = Props(method);
        props.CharacterMap = new Dictionary<int, string> { ['x'] = "mapped" };
        var ex = Assert.Throws<XsltRuntimeException>(() => ResultTreeSerializer.SerializeCaptureBytes(Tree("x"), props,
            100, null, observeProduction: n => produced = n));
        Assert.Equal("XV0014", ex.ErrorCode);
        Assert.Equal(0, produced);
    }

    /// <summary>CF1 CdataAndRawProfiles PreserveFidelity.</summary>
    /// <param name="method">The test scenario value.</param>
    /// <param name="version">The test scenario value.</param>
    /// <param name="raw">The test scenario value.</param>
    [Theory]
    [InlineData("xml", "1.0", false)]
    [InlineData("xml", "1.1", false)]
    [InlineData("xml", "1.0", true)]
    [InlineData("xhtml", "1.0", false)]
    public void CF1_CdataAndRawProfiles_PreserveFidelity(string method, string version, bool raw)
    {
        var props = Props(method);
        props.Version = version;
        var element = new XElement("out", new XCData("a]]>b\r\n\u00E9\U0001F600"), new XElement("empty"));
        if (raw) element.Add(new XRawText("<raw/>"));
        var value = XdmValue.FromNode(new XDocumentNode(element));
        var expected = ResultTreeSerializer.SerializeToBytes(value, props.Clone());
        Assert.Equal(expected, ResultTreeSerializer.SerializeCaptureBytes(value, props, expected.Length, null));
    }

    /// <summary>CF1 SequenceXml IsByteIdentical.</summary>
    /// <param name="encoding">The test scenario value.</param>
    [Theory]
    [InlineData("UTF-8")]
    [InlineData("US-ASCII")]
    public void CF1_SequenceXml_IsByteIdentical(string encoding)
    {
        var value = Bosak.XPath.Api.XPath31Expression.Compile("('\u00E9\U0001F600', 'two', 'last')").Evaluate(new EvaluationContext());
        Assert.Equal(ResultTreeSerializer.SerializeToBytes(value, Props("xml", encoding)),
            ResultTreeSerializer.SerializeCaptureBytes(value, Props("xml", encoding), 1000, null));
    }

    /// <summary>CF1 MillionByteOutput StopsProductionAtTinyBudget.</summary>
    /// <param name="secondary">The test scenario value.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CF1_MillionByteOutput_StopsProductionAtTinyBudget(bool secondary)
    {
        long produced = 0;
        var session = new XsltCaptureSession(new XsltCaptureOptions { MaxBytesPerOutput = 1 }, null, null,
            n => produced = n);
        var value = Tree(new string('x', 1_000_000));
        Exception? failure = null;
        if (secondary)
            failure = Assert.Throws<XsltCaptureLimitException>(() => session.ObserveResultDocument("file:///large.txt", value, Props()));
        else session.CapturePrincipal(value, Props());
        var result = session.BuildResult(failure);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Principal);
        Assert.Empty(result.SecondaryOutputs);
        Assert.Equal("XV0007", Assert.Single(result.Failures).Code);
        // Actual encoding work, not retained-output length. Lookahead is at most a scalar-sized
        // encoding chunk, irrespective of the million-character result tree already constructed.
        Assert.InRange(produced, 2, 16);
    }

    /// <summary>CF1 RemainingAggregateBudget BoundsNextOutputProduction.</summary>
    [Fact]
    public void CF1_RemainingAggregateBudget_BoundsNextOutputProduction()
    {
        long produced = 0;
        var session = new XsltCaptureSession(new XsltCaptureOptions { MaxTotalBytes = 5 }, null, null,
            n => produced = n);
        session.ObserveResultDocument("file:///first.txt", Tree("abc"), Props());
        var failure = Assert.Throws<XsltCaptureLimitException>(() =>
            session.ObserveResultDocument("file:///next.txt", Tree(new string('x', 1_000_000)), Props()));
        var result = session.BuildResult(failure);
        Assert.Equal("XV0008", Assert.Single(result.Failures).Code);
        Assert.Equal("file:///first.txt", Assert.Single(result.SecondaryOutputs).ResolvedUri);
        Assert.InRange(produced, 3, 16);
    }

    /// <summary>CF1 ExactEncodingAndBomBudget PreservesBytes.</summary>
    /// <param name="encoding">The test scenario value.</param>
    /// <param name="bom">The test scenario value.</param>
    [Theory]
    [InlineData("UTF-8", false)]
    [InlineData("UTF-8", true)]
    [InlineData("UTF-16", false)]
    [InlineData("UTF-16", true)]
    [InlineData("UTF-16BE", true)]
    [InlineData("UTF-32", true)]
    public void CF1_ExactEncodingAndBomBudget_PreservesBytes(string encoding, bool bom)
    {
        var value = Tree("é😀z");
        var expected = ResultTreeSerializer.SerializeToBytes(value, Props(encoding: encoding, bom: bom));
        long produced = 0;
        var actual = ResultTreeSerializer.SerializeCaptureBytes(value, Props(encoding: encoding, bom: bom),
            expected.Length, expected.Length, observeProduction: n => produced = n);
        Assert.Equal(expected, actual);
        Assert.Equal(expected.Length, produced);
        Assert.Throws<SerializationByteLimitException>(() => ResultTreeSerializer.SerializeCaptureBytes(value,
            Props(encoding: encoding, bom: bom), expected.Length - 1, null));
    }

    /// <summary>CF1 MarkupStreaming IsByteIdentical.</summary>
    /// <param name="method">The test scenario value.</param>
    /// <param name="encoding">The test scenario value.</param>
    /// <param name="bom">The test scenario value.</param>
    [Theory]
    [InlineData("xml", "UTF-8", false)]
    [InlineData("xml", "UTF-16", true)]
    [InlineData("xml", "US-ASCII", false)]
    [InlineData("html", "UTF-8", false)]
    [InlineData("xhtml", "UTF-8", false)]
    [InlineData("xhtml", "US-ASCII", false)]
    public void CF1_MarkupStreaming_IsByteIdentical(string method, string encoding, bool bom)
    {
        var value = XdmValue.FromNode(new XDocumentNode(new XDocument(
            new XElement("out", new XAttribute("a", "tab\tline\ncr\r\"é😀"),
                new XElement("empty"), new XText("é😀&<>"), new XComment("a\rb")))));
        var expected = ResultTreeSerializer.SerializeToBytes(value, Props(method, encoding, bom));
        var actual = ResultTreeSerializer.SerializeCaptureBytes(value, Props(method, encoding, bom), expected.Length, null);
        Assert.Equal(Encoding.GetEncoding(encoding).GetString(expected), Encoding.GetEncoding(encoding).GetString(actual));
        Assert.Equal(expected, actual);
    }

    /// <summary>CF1 BomCountsAgainstSmallBudgets.</summary>
    /// <param name="budget">The test scenario value.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CF1_BomCountsAgainstSmallBudgets(int budget)
    {
        var ex = Assert.Throws<SerializationByteLimitException>(() => ResultTreeSerializer.SerializeCaptureBytes(
            Tree("x"), Props(bom: true), budget, null));
        Assert.True(ex.PerOutput);
    }

    /// <summary>CF1 ZeroBudget AllowsTrulyEmptyText.</summary>
    [Fact]
    public void CF1_ZeroBudget_AllowsTrulyEmptyText()
        => Assert.Empty(ResultTreeSerializer.SerializeCaptureBytes(Tree(""), Props(), 0, 0));

    /// <summary>CF1 UnsupportedProfile IsExplicitlyRefusedBeforeProduction.</summary>
    /// <param name="method">The test scenario value.</param>
    /// <param name="normalization">The test scenario value.</param>
    [Theory]
    [InlineData("json", "none")]
    [InlineData("adaptive", "none")]
    [InlineData("text", "NFC")]
    public void CF1_UnsupportedProfile_IsExplicitlyRefusedBeforeProduction(string method, string normalization)
    {
        long produced = 0;
        var props = Props(method);
        props.NormalizationForm = normalization;
        var session = new XsltCaptureSession(new XsltCaptureOptions { MaxBytesPerOutput = 100 }, null, null,
            n => produced = n);
        session.CapturePrincipal(Tree("x"), props);
        var result = session.BuildResult(null);
        Assert.Equal(XsltCaptureFailureKind.UnsupportedSerializationProfile, Assert.Single(result.Failures).Kind);
        Assert.Equal("XV0014", result.Failures[0].Code);
        Assert.Equal(0, produced);
        Assert.Null(result.Principal);
    }

    /// <summary>CF1 SerializationError RetainsEarlierOutputAndIdentity.</summary>
    [Fact]
    public void CF1_SerializationError_RetainsEarlierOutputAndIdentity()
    {
        var session = new XsltCaptureSession(new XsltCaptureOptions { MaxBytesPerOutput = 100 }, null, null);
        session.ObserveResultDocument("file:///first.txt", Tree("ok"), Props());
        var ex = Assert.ThrowsAny<Exception>(() => session.ObserveResultDocument("file:///bad.txt", Tree("x"),
            Props(encoding: "not-an-encoding")));
        var result = session.BuildResult(ex);
        Assert.Equal(XsltCaptureOutcome.Partial, result.Outcome);
        Assert.Single(result.SecondaryOutputs);
        Assert.Equal("SESU0007", Assert.Single(result.Failures).Code);
        Assert.Equal("file:///bad.txt", result.Failures[0].ResolvedUri);
    }

    /// <summary>DF1 ShortPublicRun CancellationFromMessageCannotSucceed.</summary>
    [Fact]
    public void DF1_ShortPublicRun_CancellationFromMessageCannotSucceed()
    {
        using var cts = new CancellationTokenSource();
        var executable = new XsltCompiler { MessageListener = new CancellingListener(cts) }.Compile("""
            <xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0">
              <xsl:template match="/">
                <xsl:result-document href="early.xml"><early/></xsl:result-document>
                <xsl:message>cancel</xsl:message><ok/>
              </xsl:template>
            </xsl:stylesheet>
            """, "file:///review/main.xsl");
        var result = executable.CaptureTransform(new XDocumentNode(new XDocument(new XElement("doc"))),
            new XsltCaptureOptions { CancellationToken = cts.Token, BaseOutputUri = "file:///review/out.xml" });
        Assert.Equal(XsltCaptureOutcome.Cancelled, result.Outcome);
        Assert.Equal("XV0013", Assert.Single(result.Failures).Code);
        Assert.Single(result.SecondaryOutputs);
        Assert.Null(result.Principal);
    }

    /// <summary>DF1 FinalSerialization ControlTriggerCannotPublishSuccess.</summary>
    /// <param name="deadline">The test scenario value.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DF1_FinalSerialization_ControlTriggerCannotPublishSuccess(bool deadline)
    {
        using var cts = new CancellationTokenSource();
        var gate = ExecutionGate.Create(cts.Token, deadline ? TimeSpan.FromMilliseconds(200) : null, null);
        bool triggered = false;
        var session = new XsltCaptureSession(new XsltCaptureOptions { MaxBytesPerOutput = 2_000_000 }, null, gate,
            n =>
            {
                if (n < 1024 || triggered) return;
                triggered = true;
                if (deadline) Thread.Sleep(250);
                else cts.Cancel();
            });
        var ex = Assert.Throws<ExecutionInterruptException>(() => session.CapturePrincipal(Tree(new string('x', 1_000_000)), Props()));
        var result = session.BuildResult(ex);
        Assert.True(triggered);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Principal);
        Assert.Equal(deadline ? "XV0011" : "XV0013", Assert.Single(result.Failures).Code);
        if (!deadline) Assert.Equal(XsltCaptureOutcome.Cancelled, result.Outcome);
    }

    /// <summary>DF1 TerminalChecks DoNotConsumeExactOperationBudget.</summary>
    [Fact]
    public void DF1_TerminalChecks_DoNotConsumeExactOperationBudget()
    {
        var gate = ExecutionGate.Create(default, null, 1);
        gate.Checkpoint();
        for (int i = 0; i < 100; i++) gate.ObserveControls();
        var ex = Assert.Throws<ExecutionInterruptException>(gate.Checkpoint);
        Assert.Equal(ExecutionGateInterruptKind.OperationLimitExceeded, ex.Kind);
    }

    /// <summary>DF1 CompletionBoundary ObservesCancellationAfterLastOutput.</summary>
    [Fact]
    public void DF1_CompletionBoundary_ObservesCancellationAfterLastOutput()
    {
        using var cts = new CancellationTokenSource();
        var gate = ExecutionGate.Create(cts.Token, null, null);
        var session = new XsltCaptureSession(new XsltCaptureOptions(), null, gate);
        session.CapturePrincipal(Tree("ok"), Props());
        cts.Cancel();
        var result = session.BuildResult(null);
        Assert.Equal(XsltCaptureOutcome.Cancelled, result.Outcome);
        Assert.NotNull(result.Principal); // completed output remains partial evidence
        Assert.Equal("XV0013", Assert.Single(result.Failures).Code);
    }

    private sealed class CancellingListener(CancellationTokenSource cts) : IXsltMessageListener
    {
        /// <inheritdoc/>
        public void OnMessage(string message) => cts.Cancel();
        /// <inheritdoc/>
        public void OnWarning(string message) { }
    }
}
