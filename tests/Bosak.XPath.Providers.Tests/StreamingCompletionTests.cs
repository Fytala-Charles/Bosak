// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 30 October 2026
// PURPOSE              : Verifies exception-safe completion and reader ownership for streaming input.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 30-10-2026     | Creation; completion cleanup and failure precedence tests                                   |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using System.Text;
using System.Xml;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Streaming;
using Xunit;

namespace Bosak.XPath.Providers.Tests.Streaming;

/// <summary>Verifies owned-reader cleanup without changing supplied-reader ownership.</summary>
public sealed class StreamingCompletionTests
{
    private static StreamingLoadOptions Options(bool closeInput = true, bool retain = false) => new()
    {
        RetainRecords = retain,
        ReaderSettings = new XmlReaderSettings { CloseInput = closeInput, DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }
    };

    /// <summary>Completion runs before closing input and is not repeated by Drain.</summary>
    /// <param name="closeInput">Whether disposing the owned reader also closes its input.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SuccessfulCompletion_RespectsCloseInputAndRunsOnce(bool closeInput)
    {
        using var input = new TrackingStream();
        var doc = (IStreamingDocument)XmlStreamingProvider.Load(input, Options(closeInput));
        var callbacks = 0;
        doc.StreamCompleted += () => { Assert.True(input.CanRead); callbacks++; };
        doc.Drain();
        doc.Drain();
        Assert.Equal(1, callbacks);
        Assert.Equal(closeInput ? 1 : 0, input.DisposeCalls);
        Assert.Equal(!closeInput, input.CanRead);
    }

    /// <summary>A callback fault keeps its identity and still closes an owned reader.</summary>
    /// <param name="closeInput">Whether disposing the owned reader also closes its input.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CallbackFailure_AttemptsCleanupAndDoesNotRepeatCompletion(bool closeInput)
    {
        using var input = new TrackingStream();
        var doc = (IStreamingDocument)XmlStreamingProvider.Load(input, Options(closeInput));
        var failure = new InvalidOperationException("completion failure");
        var callbacks = 0;
        doc.StreamCompleted += () => { callbacks++; ThrowCompletion(failure); };
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => doc.Drain()));
        Assert.Contains(nameof(ThrowCompletion), failure.StackTrace);
        Assert.True(doc.CanDrain);
        doc.Drain();
        Assert.Equal(1, callbacks);
        Assert.Equal(closeInput ? 1 : 0, input.DisposeCalls);
        Assert.Equal(!closeInput, input.CanRead);
    }

    /// <summary>Supplied readers stay caller-owned even if their completion callback fails.</summary>
    [Fact]
    public void SuppliedReader_CallbackFailureLeavesReaderAndInputOpen()
    {
        using var input = new TrackingStream();
        using var reader = XmlReader.Create(input, Options().ReaderSettings!);
        var doc = (IStreamingDocument)XmlStreamingProvider.Load(reader);
        var failure = new InvalidOperationException("caller-owned completion");
        doc.StreamCompleted += () => throw failure;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => doc.Drain()));
        Assert.NotEqual(ReadState.Closed, reader.ReadState);
        Assert.True(input.CanRead);
        Assert.Equal(0, input.DisposeCalls);
        doc.Drain();
        Assert.Equal(0, input.DisposeCalls);
    }

    /// <summary>A cleanup-only failure propagates unchanged and completion is not retried.</summary>
    [Fact]
    public void DisposalFailure_PropagatesOriginalWhenCallbackSucceeds()
    {
        var input = new TrackingStream();
        try
        {
            var failure = new IOException("dispose failure");
            input.Failure = failure;
            var doc = (IStreamingDocument)XmlStreamingProvider.Load(input, Options());
            var callbacks = 0;
            doc.StreamCompleted += () => callbacks++;
            Assert.Same(failure, Assert.Throws<IOException>(() => doc.Drain()));
            doc.Drain();
            Assert.Equal(1, input.DisposeCalls);
            Assert.Equal(1, callbacks);
        }
        finally { input.Failure = null; input.Dispose(); }
    }

    /// <summary>Simultaneous callback and cleanup faults retain both original exceptions in order.</summary>
    [Fact]
    public void DualFailure_AggregatesCallbackFirstAndCleanupSecond()
    {
        var input = new TrackingStream();
        try
        {
            var callback = new InvalidOperationException("callback failure");
            var cleanup = new IOException("cleanup failure");
            input.Failure = cleanup;
            var doc = (IStreamingDocument)XmlStreamingProvider.Load(input, Options());
            doc.StreamCompleted += () => ThrowCompletion(callback);
            var aggregate = Assert.Throws<AggregateException>(() => doc.Drain());
            Assert.Equal(2, aggregate.InnerExceptions.Count);
            Assert.Same(callback, aggregate.InnerExceptions[0]);
            Assert.Same(cleanup, aggregate.InnerExceptions[1]);
            Assert.Contains(nameof(ThrowCompletion), callback.StackTrace);
            doc.Drain();
            Assert.Equal(1, input.DisposeCalls);
        }
        finally { input.Failure = null; input.Dispose(); }
    }

    /// <summary>Cancellation keeps the original token while owned-reader cleanup still runs.</summary>
    [Fact]
    public void CallbackCancellation_PreservesTokenAndClosesOwnedInput()
    {
        using var input = new TrackingStream();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var failure = new OperationCanceledException(cancellation.Token);
        var doc = (IStreamingDocument)XmlStreamingProvider.Load(input, Options());
        doc.StreamCompleted += () => throw failure;
        var actual = Assert.Throws<OperationCanceledException>(() => doc.Drain());
        Assert.Same(failure, actual);
        Assert.Equal(cancellation.Token, actual.CancellationToken);
        Assert.Equal(1, input.DisposeCalls);
    }

    /// <summary>Empty roots take the same completion cleanup path.</summary>
    [Fact]
    public void EmptyRoot_CallbackFailureClosesOwnedInput()
    {
        using var input = new TrackingStream("<root/>");
        var doc = (IStreamingDocument)XmlStreamingProvider.Load(input, Options());
        doc.StreamCompleted += () => throw new InvalidOperationException("empty callback");
        Assert.Throws<InvalidOperationException>(() => doc.Drain());
        Assert.False(input.CanRead);
        Assert.Equal(1, input.DisposeCalls);
    }

    /// <summary>Retained replay does not repeat a failed completion callback or cleanup.</summary>
    [Fact]
    public void RetainedRecords_CallbackFailureAllowsReplayWithoutRepeatingCompletion()
    {
        using var input = new TrackingStream();
        var node = XmlStreamingProvider.Load(input, Options(retain: true));
        var doc = (IStreamingDocument)node;
        var callbacks = 0;
        doc.StreamCompleted += () => { callbacks++; throw new InvalidOperationException("retained callback"); };
        Assert.Throws<InvalidOperationException>(() => doc.Drain());
        doc.Drain();
        var root = node.Children(XdmNodeKind.Element).GetEnumerator();
        Assert.True(root.MoveNext());
        var count = 0;
        foreach (var value in root.Current.NodeValue.Children(XdmNodeKind.Element)) count++;
        Assert.Equal(2, count);
        Assert.Equal(1, callbacks);
        Assert.Equal(1, input.DisposeCalls);
    }

    /// <summary>Normal multicast short-circuiting remains in force when a callback fails.</summary>
    [Fact]
    public void ThrowingFirstHandler_SkipsLaterHandlerButStillCleansUp()
    {
        using var input = new TrackingStream();
        var doc = (IStreamingDocument)XmlStreamingProvider.Load(input, Options());
        var later = false;
        doc.StreamCompleted += () => throw new InvalidOperationException("first handler");
        doc.StreamCompleted += () => later = true;
        Assert.Throws<InvalidOperationException>(() => doc.Drain());
        Assert.False(later);
        Assert.Equal(1, input.DisposeCalls);
    }

    private static void ThrowCompletion(Exception failure) => throw failure;

    private sealed class TrackingStream(string xml = "<root><item>1</item><item>2</item></root>")
        : MemoryStream(Encoding.UTF8.GetBytes(xml))
    {
        internal int DisposeCalls;
        internal Exception? Failure;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing) DisposeCalls++;
            base.Dispose(disposing);
            if (disposing && Failure is not null) throw Failure;
        }
    }
}
