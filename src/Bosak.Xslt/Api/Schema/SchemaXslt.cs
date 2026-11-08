// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Captures ordinary XSLT sessions and bounded serialized invocation outcomes.
// SPECIAL NOTES        : Part of the Bosak XPath 3.1 implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.Xslt.Capture;
using Bosak.XPath.Runtime;
using ApiSession = Bosak.XPath.Api.Schema.SchemaQuerySession;
using Bosak.XPath.Runtime.Resources;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.Xslt.Api.Schema;
/// <summary>Captures stylesheet settings and package state into an opt-in executable.</summary>
public static class SchemaXsltCompiler
{
    /// <summary>Compiles and freezes effective static context before binding.</summary>
    /// <param name="stylesheet">Stylesheet source.</param>
    /// <param name="limits">Explicit finite limits.</param>
    /// <param name="compiler">Compiler settings/package closure copied under the selected policy.</param>
    /// <param name="assessor">Optional borrowed assessor.</param>
    /// <param name="baseUri">Explicit stylesheet base URI.</param>
    /// <param name="cancellationToken">Compilation/binding cancellation.</param>
    /// <returns>An executable or explicit unsupported profile result.</returns>
    /// <exception cref="ArgumentException">Source or explicit limits are invalid.</exception>
    /// <exception cref="OperationCanceledException">Compilation was canceled.</exception>
    public static SchemaResult<SchemaXsltExecutable> Compile(string stylesheet, SchemaQueryLimits limits, XsltCompiler? compiler = null, ISchemaAssessor? assessor = null, string? baseUri = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stylesheet);limits.Validate();cancellationToken.ThrowIfCancellationRequested();
        if(assessor is not null)return ApiSession.Unsupported<SchemaXsltExecutable>();
        if(!SchemaHostSessionCompilation.TryCaptureXslt(stylesheet,compiler??new XsltCompiler(),baseUri,out var executable,out var environment,out _))
            return ApiSession.Unsupported<SchemaXsltExecutable>();
        cancellationToken.ThrowIfCancellationRequested();
        var result=new SchemaXsltExecutable(executable!,environment!,limits);
        try { cancellationToken.ThrowIfCancellationRequested();return ApiSession.Supported(result); }
        catch { result.Session.Dispose();throw; }
    }
}
/// <summary>Separates initial-match and global-context document roots for a bounded transform profile.</summary>
public readonly struct SchemaXsltInvocation
{
    /// <summary>Specifies invocation inputs; absence of match input does not supply global context.</summary>
    /// <param name="initialMatchInput">Admitted document root for initial matching, or absent.</param>
    /// <param name="globalContextInput">Separately admitted document root for global context, or absent.</param>
    /// <param name="initialTemplate">Initial expanded template name using the existing host convention.</param>
    /// <param name="initialMode">Initial expanded mode name using the existing host convention.</param>
    /// <param name="rawResult">Whether to preserve the ordinary raw result sequence.</param>
    /// <param name="baseOutputUri">Output base URI under the captured resource policy.</param>
    public SchemaXsltInvocation(SchemaQuerySnapshot? initialMatchInput = null, SchemaQuerySnapshot? globalContextInput = null, string? initialTemplate = null, string? initialMode = null, bool rawResult = false, string? baseOutputUri = null)
    {
        InitialMatchInput=initialMatchInput;GlobalContextInput=globalContextInput;InitialTemplate=initialTemplate;
        InitialMode=initialMode;RawResult=rawResult;BaseOutputUri=baseOutputUri;
    }
    /// <summary>The separately identified initial-match root admission.</summary>
    public SchemaQuerySnapshot? InitialMatchInput { get; }
    /// <summary>The separately identified global-context root admission.</summary>
    public SchemaQuerySnapshot? GlobalContextInput { get; }
    /// <summary>The initial named template.</summary>
    public string? InitialTemplate { get; }
    /// <summary>The initial mode.</summary>
    public string? InitialMode { get; }
    /// <summary>Whether to return the ordinary raw result graph.</summary>
    public bool RawResult { get; }
    /// <summary>The base output URI.</summary>
    public string? BaseOutputUri { get; }
}
/// <summary>A captured executable whose transformations borrow a shared session operation.</summary>
public sealed class SchemaXsltExecutable
{
    /// <summary>Captures under an owned raw operation and returns only after its admission cleanup attempts finish.</summary>
    /// <param name="snapshots">Same-session snapshot admissions; empty for source-free work.</param>
    /// <param name="invocation">Explicit source roles and invocation choices.</param>
    /// <param name="options">Explicit finite capture controls; duplicate context/selection settings refuse.</param>
    /// <param name="context">Sole dynamic input context under the supported captured policy.</param>
    /// <param name="cancellationToken">Raw operation cancellation, distinct from cancelling a caller wait.</param>
    /// <returns>Capture when produced and separate local admission/work/cleanup diagnostics; cleanup failure prevents overall success.</returns>
    /// <exception cref="ArgumentException">Options are missing required caps or duplicate invocation settings.</exception>
    public SchemaXsltRunResult RunCaptureTransform(ImmutableArray<SchemaQuerySnapshot> snapshots, SchemaXsltInvocation invocation, XsltCaptureOptions options, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        var copy=CopyOptions(invocation,options);
        var report=_binding.RunOutcome<SchemaXsltCaptureResult>(_executable,snapshots,
            (operation,stage)=>CaptureCore(operation,invocation,copy,context,stage),cancellationToken);
        return new SchemaXsltRunResult(report.HasResult?report.Result:null,report.AdmissionError,report.WorkError,
            Array.AsReadOnly(report.CleanupErrors.ToArray()));
    }

    /// <summary>Captures bounded serialized outputs under this executable's exact session operation.</summary>
    /// <param name="operation">Live operation from Session; retained through serialization.</param>
    /// <param name="invocation">Sole match/global, template/mode and output-base selection; RawResult must be false.</param>
    /// <param name="options">Copied finite capture controls; Context, InitialTemplate, InitialMode and BaseOutputUri must be absent.</param>
    /// <param name="context">Sole dynamic context; captured authority cannot be replaced.</param>
    /// <returns>Explicit admission failure or detached output and actual per-run receipts.</returns>
    /// <exception cref="ArgumentException">The operation or invocation/options are invalid.</exception>
    /// <exception cref="NotSupportedException">Dynamic state or source roles are outside the ordinary profile.</exception>
    /// <exception cref="ObjectDisposedException">The raw operation or a required admission is revoked.</exception>
    /// <exception cref="OperationCanceledException">Raw work was canceled.</exception>
    public SchemaResult<SchemaXsltCaptureResult> CaptureTransform(SchemaQueryOperation operation, SchemaXsltInvocation invocation, XsltCaptureOptions options, EvaluationContext? context = null)
    {
        var copy=CopyOptions(invocation,options);
        return ApiSession.Supported(CaptureCore(operation,invocation,copy,context));
    }

    private readonly XsltExecutable _executable;
    private readonly CapturedHostBinding _binding;
    private readonly KeyValuePair<(string LocalName,string NamespaceUri),XdmValue>[] _statics;
    private readonly Action<long>? _observeProduction;
    internal SchemaXsltExecutable(XsltExecutable executable,SchemaQueryEnvironment environment,SchemaQueryLimits limits,
        Action<long>? observeProduction=null)
    {
        _executable=executable;_statics=executable.CaptureSchemaStaticValues().ToArray();_observeProduction=observeProduction;
        _binding=CapturedHostBinding.CreatePublicTransform(executable,environment,limits);
    }

    private void Prepare(CapturedHostBinding.Frame frame)
    {
        var match=frame.Match;var global=frame.Global;
        if(match is not null&&match.NodeKind!=XdmNodeKind.Document||global is not null&&global.NodeKind!=XdmNodeKind.Document)
            throw new NotSupportedException("The ordinary public XSLT profile requires document-root invocation inputs.");
        foreach(var pair in _statics)frame.Context.WithVariable(pair.Key.LocalName,pair.Value,pair.Key.NamespaceUri);
    }

    private static XsltCaptureOptions CopyOptions(SchemaXsltInvocation invocation,XsltCaptureOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if(invocation.RawResult||options.Context is not null||options.InitialTemplate is not null||options.InitialMode is not null||options.BaseOutputUri is not null)
            throw new ArgumentException("Invocation is the sole capture context and selection source; capture cannot return raw results.",nameof(options));
        var copy=new XsltCaptureOptions { MaxSecondaryOutputCount=options.MaxSecondaryOutputCount,MaxBytesPerOutput=options.MaxBytesPerOutput,
            MaxTotalBytes=options.MaxTotalBytes,MaxMessages=options.MaxMessages,TimeLimit=options.TimeLimit,
            MaxEngineOperations=options.MaxEngineOperations,CancellationToken=options.CancellationToken,
            InitialTemplate=invocation.InitialTemplate,InitialMode=invocation.InitialMode,BaseOutputUri=invocation.BaseOutputUri };
        if(copy.MaxSecondaryOutputCount is null||copy.MaxBytesPerOutput is null||copy.MaxTotalBytes is null||copy.MaxMessages is null
            ||copy.TimeLimit is null||copy.MaxEngineOperations is null or <=0)
            throw new ArgumentException("All count/byte caps and positive time/engine-operation caps are required.",nameof(options));
        return copy;
    }

    private SchemaXsltCaptureResult CaptureCore(SchemaQueryOperation operation,SchemaXsltInvocation invocation,
        XsltCaptureOptions copy,EvaluationContext? context,Action<SchemaXsltCaptureResult>? stage=null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        var raw=operation.RequireHost(_binding);
        return _binding.Call(operation,_executable,context,invocation.InitialMatchInput,invocation.GlobalContextInput,frame=>
        {
            Prepare(frame);var child=frame.Context;var match=frame.Match;var global=frame.Global;
            using var linked=CancellationTokenSource.CreateLinkedTokenSource(raw.Cancellation,copy.CancellationToken??default);
            child.ExecutionGate=ExecutionGate.Compose(child.ExecutionGate,ExecutionGate.Create(linked.Token,copy.TimeLimit,copy.MaxEngineOperations));
            var errors=new List<Exception>();
            void Observe(Exception error){if(!errors.Any(existing=>ReferenceEquals(existing,error)))errors.Add(error);}
            var capture=_executable.CaptureExplicitContext(match,global,copy,child,Observe,_observeProduction);
            var result=new SchemaXsltCaptureResult(capture,Array.AsReadOnly(child.ResourcePolicy!.Receipts.ToArray()),Array.AsReadOnly(errors.ToArray()));
            stage?.Invoke(result);return result;
        });
    }
    /// <summary>The engine-created session owning admissions and close/drain behavior.</summary>
    public ApiSession Session => _binding.Session;
    /// <summary>Executes inside an operation belonging to this executable's session.</summary>
    /// <param name="operation">Live borrowed operation including every invocation/context node input.</param>
    /// <param name="invocation">Explicit input, template/mode and output selections.</param>
    /// <param name="context">Dynamic inputs subject to admission and captured resource policy.</param>
    /// <returns>A borrowed ordinary result graph or explicit unsupported profile result.</returns>
    /// <exception cref="ArgumentException">The operation or invocation/options are invalid.</exception>
    /// <exception cref="NotSupportedException">Dynamic state or source roles are outside the ordinary profile.</exception>
    /// <exception cref="ObjectDisposedException">The raw operation or a required admission is revoked.</exception>
    /// <exception cref="OperationCanceledException">Raw work was canceled.</exception>
    public SchemaResult<XdmValue> Transform(SchemaQueryOperation operation, SchemaXsltInvocation invocation, EvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(operation);var raw=operation.RequireHost(_binding);
        var result=_binding.Call(operation,_executable,context,invocation.InitialMatchInput,invocation.GlobalContextInput,frame=>
        {
            Prepare(frame);var child=frame.Context;
            child.ExecutionGate=ExecutionGate.Compose(child.ExecutionGate,ExecutionGate.Create(raw.Cancellation,null,null));
            try
            {
                return _executable.TransformCapturedContext(frame.Match,frame.Global,invocation.InitialTemplate,invocation.InitialMode,
                    invocation.RawResult,invocation.BaseOutputUri,child);
            }
            catch(ExecutionInterruptException) { raw.Check();throw; }
        });
        return ApiSession.Supported(result);
    }
}

/// <summary>Serialized capture, immutable receipts and original local execution diagnostics from one invocation.</summary>
public sealed class SchemaXsltCaptureResult
{
    /// <summary>Original exceptions observed during execution/serialization; identity-deduplicated, not flattened or recursively detached, and not an IPC payload.</summary>
    public IReadOnlyList<Exception> ExecutionErrors { get; }

    internal SchemaXsltCaptureResult(XsltCaptureResult capture,IReadOnlyList<ControlledResourceReceipt> receipts,IReadOnlyList<Exception> errors)
    {Capture=capture;ResourceReceipts=receipts;ExecutionErrors=errors;}
    /// <summary>Existing classified capture outputs, properties, messages and failures.</summary>
    public XsltCaptureResult Capture { get; }
    /// <summary>Actual immutable per-run acquisition receipts, excluding compile-time and other runs.</summary>
    public IReadOnlyList<ControlledResourceReceipt> ResourceReceipts { get; }
}

/// <summary>Final local capture-operation outcome after all admission cleanup attempts; never proof of native release when cleanup fails.</summary>
public sealed class SchemaXsltRunResult
{
    internal SchemaXsltRunResult(SchemaXsltCaptureResult? result,Exception? admission,Exception? work,IReadOnlyList<Exception> cleanup)
    {Result=result;AdmissionError=admission;WorkError=work;CleanupErrors=cleanup;}
    /// <summary>Capture and receipts when execution produced them, retained even if cleanup failed.</summary>
    public SchemaXsltCaptureResult? Result { get; }
    /// <summary>Original entry/rollback diagnostic when admission failed; provider aggregates are not flattened.</summary>
    public Exception? AdmissionError { get; }
    /// <summary>Original work diagnostic when capture could not return its envelope.</summary>
    public Exception? WorkError { get; }
    /// <summary>Every original cleanup exception in reverse acquisition attempt order; provider aggregate boundaries remain intact.</summary>
    public IReadOnlyList<Exception> CleanupErrors { get; }
    /// <summary>True only for a successful capture with no execution, admission, work or cleanup error; Partial and Cancelled remain unsuccessful.</summary>
    public bool IsSuccess => Result is not null&&Result.Capture.Outcome==XsltCaptureOutcome.Success&&Result.ExecutionErrors.Count==0
        &&AdmissionError is null&&WorkError is null&&CleanupErrors.Count==0;
}
