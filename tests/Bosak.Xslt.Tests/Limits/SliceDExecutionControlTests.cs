// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Contract tests for REQ-125 Slice D: cooperative cancellation, execution limits, nested inheritance and run isolation (AC-08, AC-09).
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice D)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Bosak.Xslt.Api;
using Bosak.Xslt.Capture;
using Bosak.Xslt.Validation;
using Xunit;

namespace Bosak.Xslt.Tests.Limits;

/// <summary>
/// AC-08/AC-09 contract tests for cooperative execution control:
/// AC-08 — "Cancellation and declared execution limits stop representative loops/recursion and
/// serialization as documented; limitations are demonstrated and documented."
/// AC-09 — "Existing default APIs remain compatible; controlled opt-in survives nested contexts;
/// parallel requests cannot share mutable policy or result state."
/// </summary>
public class SliceDExecutionControlTests
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false);
    private static readonly Uri ValidationMainUri = new("file:///limits/val-main.xsl");

    private static XsltExecutable Compile(string xsl)
        => new XsltCompiler().Compile(xsl, "file:///limits/main.xsl");

    private static XDocumentNode Source()
        => new(new XDocument(new XElement("doc")));

    private static string LoopStylesheet(int iterations)
        => $@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:for-each select='1 to {iterations}'>
      <xsl:result-document href='o{{position()}}.xml'><o><xsl:value-of select='.'/></o></xsl:result-document>
    </xsl:for-each>
    <done/>
  </xsl:template>
</xsl:stylesheet>";

    private static bool HasFailure(XsltCaptureResult result, XsltCaptureFailureKind kind, string code)
        => result.Failures.Any(f => f.Kind == kind && f.Code == code);

    private sealed class CancelOnFirstMessageListener : IXsltMessageListener
    {
        private readonly CancellationTokenSource _cts;
        private bool _cancelled;

        internal CancelOnFirstMessageListener(CancellationTokenSource cts) => _cts = cts;

        public void OnMessage(string message)
        {
            if (!_cancelled)
            {
                _cancelled = true;
                _cts.Cancel();
            }
        }

        public void OnWarning(string message)
        {
        }
    }

    // ------------------------------------------------------------------
    // AC-08: cancellation (token) — before start and mid-run
    // ------------------------------------------------------------------

    [Fact]
    public void D_CancelBeforeStart_ProducesCancelledWithoutOutputsOrThrow()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='early.xml'><early/></xsl:result-document>
    <out>hi</out>
  </xsl:template>
</xsl:stylesheet>");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
            CancellationToken = cts.Token,
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(XsltCaptureOutcome.Cancelled, result.Outcome);
        Assert.Null(result.Principal);
        Assert.Empty(result.SecondaryOutputs);
        Assert.Empty(result.Messages);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(XsltCaptureFailureKind.Cancelled, failure.Kind);
        Assert.Equal("XV0013", failure.Code);
    }

    [Fact]
    public void D_CancelMidRunViaMessageListener_CancelledRetainsEarlierOutputs()
    {
        // The host message listener cancels the token from inside the run; the run must
        // terminate cooperatively at a later checkpoint with the outputs already captured
        // retained on a Cancelled result.
        using var cts = new CancellationTokenSource();
        var listener = new CancelOnFirstMessageListener(cts);
        var executable = new XsltCompiler { MessageListener = listener }.Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:for-each select='1 to 1000'>
      <xsl:result-document href='o{position()}.xml'><o><xsl:value-of select='.'/></o></xsl:result-document>
      <xsl:message select='.'/>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
            CancellationToken = cts.Token,
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(XsltCaptureOutcome.Cancelled, result.Outcome);
        Assert.True(result.SecondaryOutputs.Count >= 1, "at least the pre-cancellation outputs are retained");
        Assert.True(result.SecondaryOutputs.Count < 1000, "the loop was terminated before completion");
        Assert.True(result.Messages.Count >= 1, "the message that triggered cancellation was recorded");
        Assert.True(HasFailure(result, XsltCaptureFailureKind.Cancelled, "XV0013"));
    }

    // ------------------------------------------------------------------
    // AC-08: declared limits — wall-clock and operation budget
    // ------------------------------------------------------------------

    [Fact]
    public void D_TimeLimitExceeded_TerminatesLoopWithClassifiedFailure()
    {
        var executable = Compile(LoopStylesheet(1_000_000));

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
            TimeLimit = TimeSpan.FromMilliseconds(10),
        });

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Outcome, new[] { XsltCaptureOutcome.Partial, XsltCaptureOutcome.Failed });
        Assert.True(HasFailure(result, XsltCaptureFailureKind.TimeLimitExceeded, "XV0011"));
        Assert.True(result.SecondaryOutputs.Count < 1_000_000, "the loop was terminated before completion");
    }

    [Fact]
    public void D_OperationLimitExceeded_TerminatesLoopDeterministically()
    {
        var executable = Compile(LoopStylesheet(500));

        XsltCaptureResult Run() => executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
            MaxEngineOperations = 100,
        });

        var first = Run();
        var second = Run();

        foreach (var result in new[] { first, second })
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(XsltCaptureOutcome.Partial, result.Outcome);
            Assert.True(HasFailure(result, XsltCaptureFailureKind.OperationLimitExceeded, "XV0012"));
            Assert.True(result.SecondaryOutputs.Count > 0, "outputs produced before the budget ran out are retained");
            Assert.True(result.SecondaryOutputs.Count < 500, "the loop was terminated before completion");
        }

        // The budget counts checkpoints exactly, so the abort point — and therefore the
        // retained output set — is identical across runs of the same stylesheet and input.
        Assert.Equal(
            first.SecondaryOutputs.Select(o => o.ResolvedUri),
            second.SecondaryOutputs.Select(o => o.ResolvedUri));
    }

    [Fact]
    public void D_OperationLimitExceeded_StopsTemplateRecursion()
    {
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template name='count'>
    <xsl:param name='n' select='0'/>
    <xsl:if test='$n &lt; 5000'>
      <xsl:result-document href='r{$n}.xml'><r/></xsl:result-document>
      <xsl:call-template name='count'><xsl:with-param name='n' select='$n + 1'/></xsl:call-template>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>");

        var result = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
            InitialTemplate = "count",
            MaxEngineOperations = 200,
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(XsltCaptureOutcome.Partial, result.Outcome);
        Assert.True(HasFailure(result, XsltCaptureFailureKind.OperationLimitExceeded, "XV0012"));
        Assert.True(result.SecondaryOutputs.Count > 0, "outputs produced before the budget ran out are retained");
        Assert.True(result.SecondaryOutputs.Count < 5000, "the recursion was terminated before completion");
    }

    // ------------------------------------------------------------------
    // AC-09: default path bit-identical; controls opt-in only
    // ------------------------------------------------------------------

    [Fact]
    public void D_GenerousLimits_SuccessAndBytesIdenticalToUnlimitedRun()
    {
        var xsl = @"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:result-document href='a.xml'><a>1</a></xsl:result-document>
    <xsl:result-document href='b.xml'><b>2</b></xsl:result-document>
    <out>hi</out>
  </xsl:template>
</xsl:stylesheet>";
        var executable = Compile(xsl);

        var unlimited = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
        });
        using var cts = new CancellationTokenSource();
        var limited = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
            CancellationToken = cts.Token,
            TimeLimit = TimeSpan.FromSeconds(30),
            MaxEngineOperations = 1_000_000,
        });

        Assert.True(unlimited.IsSuccess, unlimited.ToString());
        Assert.True(limited.IsSuccess, limited.ToString());
        Assert.NotNull(unlimited.Principal);
        Assert.NotNull(limited.Principal);
        Assert.True(unlimited.Principal!.Bytes.SequenceEqual(limited.Principal!.Bytes), "principal bytes are bit-identical");
        Assert.Equal(
            unlimited.SecondaryOutputs.Select(o => o.ResolvedUri),
            limited.SecondaryOutputs.Select(o => o.ResolvedUri));
        foreach (var (unlimitedOutput, limitedOutput) in unlimited.SecondaryOutputs.Zip(limited.SecondaryOutputs))
        {
            Assert.True(unlimitedOutput.Bytes.SequenceEqual(limitedOutput.Bytes),
                $"bytes for {unlimitedOutput.ResolvedUri} are bit-identical");
        }
    }

    [Fact]
    public void D_OptionsRejectNonPositiveLimits()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new XsltCaptureOptions { TimeLimit = TimeSpan.Zero });
        Assert.Throws<ArgumentOutOfRangeException>(() => new XsltCaptureOptions { TimeLimit = TimeSpan.FromMilliseconds(-1) });
        Assert.Throws<ArgumentOutOfRangeException>(() => new XsltCaptureOptions { MaxEngineOperations = -1 });
    }

    // ------------------------------------------------------------------
    // AC-09: nested fn:transform inherits one shared gate
    // ------------------------------------------------------------------

    [Fact]
    public void D_NestedTransformInheritsSharedOperationBudget()
    {
        // Outer budget is far above what the outer engine needs for its own instructions but
        // far below what the nested transform's loop needs: without inheritance the nested
        // loop runs to completion and the outer run succeeds; with the shared gate the
        // budget is exhausted inside the nested run and the outer run classifies the
        // interruption. The inner stylesheet travels as stylesheet-text (a literal
        // xsl:stylesheet inside xsl:variable would be XTSE0010).
        var executable = Compile(@"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:fn='http://www.w3.org/2005/xpath-functions' version='3.0'>
  <xsl:variable name='inner' select='""&lt;xsl:stylesheet xmlns:xsl=&apos;http://www.w3.org/1999/XSL/Transform&apos; version=&apos;3.0&apos;>&lt;xsl:template name=&apos;main&apos;>&lt;xsl:for-each select=&apos;1 to 20000&apos;>&lt;xsl:result-document href=&apos;n{position()}.xml&apos;>&lt;n/>&lt;/xsl:result-document>&lt;/xsl:for-each>&lt;/xsl:template>&lt;/xsl:stylesheet>""'/>
  <xsl:template match='/'>
    <xsl:variable name='nested' select=""fn:transform(map{'stylesheet-text': $inner,
                                               'initial-template': fn:QName('', 'main'),
                                               'delivery-format': 'raw'})""/>
    <out>done</out>
  </xsl:template>
</xsl:stylesheet>");

        var controlled = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
            MaxEngineOperations = 5_000,
        });

        Assert.False(controlled.IsSuccess);
        Assert.True(
            HasFailure(controlled, XsltCaptureFailureKind.OperationLimitExceeded, "XV0012"),
            $"expected an XV0012 operation-limit failure, got: {string.Join("; ", controlled.Failures.Select(f => f.Code))}");

        // Contrast: without controls the same nested transform pair runs to completion.
        var uncontrolled = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
        });
        Assert.True(uncontrolled.IsSuccess,
            string.Join("; ", uncontrolled.Failures.Select(f => $"{f.Code}: {f.Message}")));
    }

    // ------------------------------------------------------------------
    // AC-09: run isolation — sequential and concurrent
    // ------------------------------------------------------------------

    [Fact]
    public void D_SequentialRuns_ClearControlsBetweenRuns()
    {
        var executable = Compile(LoopStylesheet(50));
        var sharedContext = new EvaluationContext();

        var limited = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            Context = sharedContext,
            BaseOutputUri = "file:///out/principal.xml",
            MaxEngineOperations = 5,
        });
        Assert.False(limited.IsSuccess);

        // A nearly-exhausted gate must not leak into the next run on the same context: a
        // clean run succeeds with the complete output set.
        var clean = executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            Context = sharedContext,
            BaseOutputUri = "file:///out/principal.xml",
        });
        Assert.True(clean.IsSuccess, clean.ToString());
        Assert.Equal(50, clean.SecondaryOutputs.Count);
    }

    [Fact]
    public async Task D_ConcurrentRuns_DoNotShareControls()
    {
        var executable = Compile(LoopStylesheet(200));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var cancelledTask = Task.Run(() => executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
            CancellationToken = cts.Token,
        }));
        var cleanTask = Task.Run(() => executable.CaptureTransform(Source(), new XsltCaptureOptions
        {
            BaseOutputUri = "file:///out/principal.xml",
        }));

        await Task.WhenAll(cancelledTask, cleanTask);

        Assert.Equal(XsltCaptureOutcome.Cancelled, cancelledTask.Result.Outcome);
        Assert.True(cleanTask.Result.IsSuccess, cleanTask.Result.ToString());
        Assert.Equal(200, cleanTask.Result.SecondaryOutputs.Count);
    }

    // ------------------------------------------------------------------
    // AC-08: validation cancellation
    // ------------------------------------------------------------------

    [Fact]
    public void D_ValidationPreCancelled_ProducesCancelledNotInvalid()
    {
        // Even a stylesheet with an invalid expression classifies as Cancelled (not Invalid)
        // when the token is already triggered: cancellation wins over the accumulated outcome.
        var broken = @"
<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>
  <xsl:template match='/'>
    <xsl:value-of select='1 +'/>
  </xsl:template>
</xsl:stylesheet>";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = XsltValidation.Validate(
            Utf8.GetBytes(broken),
            ValidationMainUri,
            options: new XsltValidationOptions { CancellationToken = cts.Token });

        Assert.Equal(XsltValidationOutcome.Cancelled, result.Outcome);
        Assert.NotEqual(XsltValidationOutcome.Invalid, result.Outcome);
        Assert.NotEqual(XsltValidationOutcome.Valid, result.Outcome);
        var failure = Assert.Single(result.Diagnostics);
        Assert.Equal("XV0013", failure.Code);
        Assert.Equal(ValidationMainUri, failure.ModuleUri);
    }

    [Fact]
    public void D_ValidationCancelMidWalk_CancelledDespiteValidStylesheet()
    {
        // A large stylesheet keeps the walk busy long enough for a CancelAfter token to fire
        // mid-walk; cancellation classifies the whole run as Cancelled with the XV0013
        // diagnostic, never as Valid and never as a throw. The same stylesheet without a
        // token validates clean, proving the outcome is driven by cancellation.
        var builder = new StringBuilder();
        builder.Append("<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>");
        for (var i = 0; i < 3000; i++)
        {
            builder.Append($"<xsl:template name='t{i}'><xsl:value-of select='{i} + 1'/></xsl:template>");
        }

        builder.Append("</xsl:stylesheet>");
        var stylesheet = builder.ToString();
        var bytes = Utf8.GetBytes(stylesheet);

        var uncancelled = XsltValidation.Validate(bytes, ValidationMainUri);
        Assert.Equal(XsltValidationOutcome.Valid, uncancelled.Outcome);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(1));
        var cancelled = XsltValidation.Validate(
            bytes,
            ValidationMainUri,
            options: new XsltValidationOptions { CancellationToken = cts.Token });

        Assert.Equal(XsltValidationOutcome.Cancelled, cancelled.Outcome);
        Assert.NotEqual(XsltValidationOutcome.Valid, cancelled.Outcome);
        Assert.Contains(cancelled.Diagnostics, d => d.Code == "XV0013");
    }
}
