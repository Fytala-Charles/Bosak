// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 10 October 2026
// PURPOSE              : Per-run capture bookkeeping: byte production under bounds, message channel, result assembly.
// SPECIAL NOTES        : Part of the Bosak XSLT 3.0 processor — controlled validation and preview API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 10-10-2026     | Creation (REQ-125 Slice C)                                                               |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================

using Bosak.XPath.Core.Xdm;
using Bosak.Xslt.Api;
using Bosak.Xslt.Runtime;

namespace Bosak.Xslt.Capture;

/// <summary>
/// Per-run capture state: one instance per <see cref="XsltExecutable.CaptureTransform"/> call. Holds
/// the outputs captured so far, the running byte total, the bounded message channel and the classified
/// failure list, and produces serialization bytes under the configured bounds. The engine observer and
/// the message listener run on the transformation thread; <see cref="BuildResult"/> runs on the
/// calling thread after the transformation thread has joined, so no locking is required.
/// </summary>
internal sealed class XsltCaptureSession
{
    internal const string CountLimitCode = "XV0006";
    internal const string PerOutputLimitCode = "XV0007";
    internal const string AggregateLimitCode = "XV0008";
    internal const string UncodedFailureCode = "XV0009";
    internal const string MessageLimitCode = "XV0010";

    private readonly XsltCaptureOptions _options;
    private readonly List<XsltCapturedOutput> _secondary = new();
    private readonly List<XsltCaptureFailure> _failures = new();
    private readonly List<string> _messages = new();
    private readonly IXsltMessageListener? _hostListener;
    private long _totalBytes;
    private bool _messageLimitRecorded;
    private Exception? _outputLevelFailure;

    internal XsltCaptureSession(XsltCaptureOptions options, IXsltMessageListener? hostListener)
    {
        _options = options;
        _hostListener = hostListener;
        MessageListener = new CaptureMessageListener(this);
    }

    /// <summary>The resolved identity of the principal output: the base output URI (possibly empty).</summary>
    internal string PrincipalIdentity => _options.BaseOutputUri ?? string.Empty;

    /// <summary>Gets the bounded message listener that records xsl:message texts for this run.</summary>
    internal IXsltMessageListener MessageListener { get; }

    // Engine observer: called once per completed secondary xsl:result-document while the engine runs
    // with capture enabled. Serializes the output to bytes immediately (bounded production), enforces
    // the configured bounds, and tells the engine not to retain the XDM value.
    internal bool ObserveResultDocument(string resolvedUri, XdmValue value, Stylesheet.OutputProperties props)
    {
        if (_options.MaxSecondaryOutputCount is { } maxCount && _secondary.Count >= maxCount)
        {
            RecordFailure(new XsltCaptureFailure(
                XsltCaptureFailureKind.OutputCountLimitExceeded,
                CountLimitCode,
                resolvedUri,
                $"Secondary output count bound ({maxCount}) exceeded; output '{resolvedUri}' was not captured and the transformation was terminated."));
            throw new XsltCaptureLimitException(XsltCaptureFailureKind.OutputCountLimitExceeded, resolvedUri);
        }

        byte[] bytes;
        try
        {
            bytes = Runtime.ResultTreeSerializer.SerializeToBytes(value, props);
        }
        catch (Exception ex)
        {
            // A serialization failure (for example SESU0007 for an unsupported encoding) aborts the
            // run with the output's identity attached; the exception is rethrown for the engine to
            // unwind. It is already recorded here, so BuildResult must not classify it a second time.
            _outputLevelFailure = ex;
            RecordFailure(ClassifyTransformFailure(ex, resolvedUri));
            throw;
        }

        if (!AcceptBytes(bytes.Length, resolvedUri, isPrincipal: false))
            throw new XsltCaptureLimitException(LastLimitKind(), resolvedUri);

        _secondary.Add(new XsltCapturedOutput(resolvedUri, isPrincipal: false, bytes, props));
        _totalBytes += bytes.Length;
        return false;
    }

    // Captures the principal output after the transformation completed, applying the same byte
    // bounds as secondary outputs. Principal bound breaches are recorded, not thrown: the run is
    // already over.
    internal void CapturePrincipal(XdmValue result, Stylesheet.OutputProperties effectiveProps)
    {
        var identity = PrincipalIdentity;
        byte[] bytes;
        try
        {
            bytes = Runtime.ResultTreeSerializer.SerializeToBytes(result, effectiveProps);
        }
        catch (Exception ex)
        {
            RecordFailure(ClassifyTransformFailure(ex, identity));
            return;
        }

        if (!AcceptBytes(bytes.Length, identity, isPrincipal: true))
            return;

        _principal = new XsltCapturedOutput(identity, isPrincipal: true, bytes, effectiveProps);
        _totalBytes += bytes.Length;
    }

    private XsltCapturedOutput? _principal;

    private XsltCaptureFailureKind _lastLimitKind;

    private XsltCaptureFailureKind LastLimitKind() => _lastLimitKind;

    // Checks the per-output and aggregate byte bounds. On a breach the failure is recorded and false
    // is returned; the offending bytes are never retained.
    private bool AcceptBytes(long byteCount, string resolvedUri, bool isPrincipal)
    {
        if (_options.MaxBytesPerOutput is { } perOutput && byteCount > perOutput)
        {
            _lastLimitKind = XsltCaptureFailureKind.OutputSizeLimitExceeded;
            RecordFailure(new XsltCaptureFailure(
                XsltCaptureFailureKind.OutputSizeLimitExceeded,
                PerOutputLimitCode,
                resolvedUri,
                $"Output '{resolvedUri}' serialized to {byteCount} bytes, exceeding the per-output bound ({perOutput}); the output was dropped."));
            return false;
        }

        if (_options.MaxTotalBytes is { } total && _totalBytes + byteCount > total)
        {
            _lastLimitKind = XsltCaptureFailureKind.AggregateSizeLimitExceeded;
            RecordFailure(new XsltCaptureFailure(
                XsltCaptureFailureKind.AggregateSizeLimitExceeded,
                AggregateLimitCode,
                resolvedUri,
                $"Capturing output '{resolvedUri}' ({byteCount} bytes) would raise the total captured bytes over the aggregate bound ({total}); the output was dropped."));
            return false;
        }

        return true;
    }

    // Assembles the immutable public result. failure is the exception that ended the run, if any;
    // it is classified here unless it is a bound breach that was already recorded at the breach point.
    internal XsltCaptureResult BuildResult(Exception? failure)
    {
        if (failure is XsltCaptureLimitException)
        {
            // Recorded at the breach point; nothing more to classify. (A stylesheet-level xsl:catch
            // can swallow the exception after recording — the failure entry survives either way.)
        }
        else if (failure is not null && !ReferenceEquals(failure, _outputLevelFailure))
        {
            RecordFailure(ClassifyTransformFailure(failure, resolvedUri: null));
        }

        var outcome = DecideOutcome(failure);
        return new XsltCaptureResult(
            outcome,
            _principal,
            Array.AsReadOnly(_secondary.ToArray()),
            Array.AsReadOnly(_failures.ToArray()),
            Array.AsReadOnly(_messages.ToArray()));
    }

    private XsltCaptureOutcome DecideOutcome(Exception? failure)
    {
        if (failure is null)
            return _failures.Count == 0 ? XsltCaptureOutcome.Success : XsltCaptureOutcome.Partial;
        return _principal is null && _secondary.Count == 0 ? XsltCaptureOutcome.Failed : XsltCaptureOutcome.Partial;
    }

    private void RecordFailure(XsltCaptureFailure failure) => _failures.Add(failure);

    // Classifies an arbitrary engine exception: XsltRuntimeException carries its spec code; other
    // engine errors (XTDE1490, XTDE1500, XTMM9000, ...) carry a leading "CODE: message" token;
    // anything else is an uncoded engine failure. A transformation-scoped duplicate output URI
    // (XTDE1490) gets its own failure kind so collisions stay distinguishable from other aborts.
    private static XsltCaptureFailure ClassifyTransformFailure(Exception exception, string? resolvedUri)
    {
        var code = ExtractCode(exception);
        var kind = code == "XTDE1490" ? XsltCaptureFailureKind.UriCollision : XsltCaptureFailureKind.TransformFailure;
        return new XsltCaptureFailure(kind, code, resolvedUri, exception.Message);
    }

    internal static string ExtractCode(Exception exception)
    {
        if (exception is XsltRuntimeException runtime)
            return runtime.ErrorCode;
        if (exception is XsltGlobalVariableException { OriginalException: { } inner })
            return ExtractCode(inner);
        return ExtractCodeFromMessage(exception.Message) ?? UncodedFailureCode;
    }

    private static string? ExtractCodeFromMessage(string message)
    {
        var colon = message.IndexOf(':');
        if (colon is < 7 or > 12)
            return null;
        var token = message.Substring(0, colon);
        if (!char.IsLetter(token[0]) || !char.IsLetter(token[1]))
            return null;
        for (var i = 2; i < token.Length; i++)
        {
            if (!char.IsLetterOrDigit(token[i]))
                return null;
        }
        return token;
    }

    // Bounded xsl:message channel: records up to the configured bound, discards the overflow and
    // records one classified failure; forwards everything to the host listener so capture composes
    // with the message listener configured at compile time.
    private void OnMessage(string message)
    {
        if (_options.MaxMessages is { } max && _messages.Count >= max)
        {
            if (!_messageLimitRecorded)
            {
                _messageLimitRecorded = true;
                RecordFailure(new XsltCaptureFailure(
                    XsltCaptureFailureKind.MessageCountLimitExceeded,
                    MessageLimitCode,
                    resolvedUri: null,
                    $"Message count bound ({max}) exceeded; further xsl:message texts were discarded."));
            }
        }
        else
        {
            _messages.Add(message);
        }

        _hostListener?.OnMessage(message);
    }

    private sealed class CaptureMessageListener : IXsltMessageListener
    {
        private readonly XsltCaptureSession _session;

        internal CaptureMessageListener(XsltCaptureSession session) => _session = session;

        public void OnMessage(string message) => _session.OnMessage(message);

        public void OnWarning(string message) => _session._hostListener?.OnWarning(message);
    }
}
