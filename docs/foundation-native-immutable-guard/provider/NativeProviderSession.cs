// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 07 November 2026
// PURPOSE              : Provides experimental provider-owned assessment and exact public snapshot handoff.
// SPECIAL NOTES        : Isolated provider/consumer qualification; not a production or published API.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 07-11-2026     | Creation; isolated public provider handoff and separate consumer controls.                |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Xml.Linq;
using Bosak.Schema;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;

namespace Bosak.Schema.ContainerPrototype;

/// <summary>Experimental provider-owned binding for bounded native document assessment and querying.</summary>
/// <remarks>Isolated feasibility API only; not exported by a production Bosak package.</remarks>
public sealed class NativeProviderSession : IDisposable
{
    private readonly SchemaQuerySession _engine;
    private readonly NativeContainerAdapter.Advisor _advisor;
    private int _closed;
    /// <summary>Compiles a bounded schema and binds its provider to the selected public query profile.</summary>
    /// <param name="schema">Caller-owned schema, captured by the native assessor.</param>
    /// <param name="expression">Supported public data or typed-identity expression.</param>
    /// <exception cref="ArgumentNullException">The schema is null.</exception>
    /// <exception cref="NotSupportedException">The schema or expression exceeds the prototype profile.</exception>
    public NativeProviderSession(XDocument schema, string expression = "data($node)")
    {
        ArgumentNullException.ThrowIfNull(schema);
        var endpoint = new NativeContainerAdapter(SchemaServices.CreateDocumentAssessor(schema));
        var compiled = SchemaQuerySession.CompileXPath(expression, new(4, 8, 8, 128, 128), assessor: endpoint);
        if (compiled.Kind != SchemaResultKind.Supported) throw new NotSupportedException("Unsupported provider query profile.");
        _engine = compiled.Value;
        _advisor = (NativeContainerAdapter.Advisor)_engine.Advisor!;
    }
    /// <summary>Gets the existing public engine session bound to this provider.</summary>
    /// <exception cref="ObjectDisposedException">This provider session is disposed.</exception>
    public SchemaQuerySession Engine { get { Check(); return _engine; } }
    /// <summary>Assesses a privately captured document and retains ownership of successful output in the result.</summary>
    /// <param name="instance">Caller-owned XML; it is not disposed or retained mutably.</param>
    /// <param name="episode">Fixed assessment episode constants.</param>
    /// <param name="limits">Optional tightened diagnostic budgets.</param>
    /// <param name="cancellationToken">Assessment cancellation.</param>
    /// <returns>A provider-owned result whose successful root may be admitted with OpenSnapshot.</returns>
    /// <exception cref="ArgumentNullException">The instance or episode is null.</exception>
    /// <exception cref="ObjectDisposedException">This session is disposed.</exception>
    /// <exception cref="OperationCanceledException">Assessment is canceled.</exception>
    public NativeProviderResult Assess(XDocument instance, ISchemaAssessmentEpisode episode, SchemaAssessmentLimits? limits = null, CancellationToken cancellationToken = default)
    {
        Check(); ArgumentNullException.ThrowIfNull(instance); ArgumentNullException.ThrowIfNull(episode);
        cancellationToken.ThrowIfCancellationRequested();
        using var request = _advisor.CaptureRequest(instance);
        var result = ((ISchemaAssessmentAdvisor)_advisor).Assess(new(request, SchemaAssessmentMode.Strict, null, null, episode, limits), cancellationToken);
        var output = ((NativeContainerAdapter.AssessmentResult)result).Output;
        try { Check(); cancellationToken.ThrowIfCancellationRequested(); return new NativeProviderResult(this, result, output); }
        catch { output?.Dispose(); throw; }
    }
    internal void Check() => ObjectDisposedException.ThrowIf(Volatile.Read(ref _closed) != 0, this);
    /// <summary>Closes this engine session; separately retained results keep their own input lifetime.</summary>
    public void Dispose() { if (Interlocked.Exchange(ref _closed, 1) == 0) _engine.Dispose(); }
}

/// <summary>Owns a bounded native output and admits only that exact output into its original engine session.</summary>
public sealed class NativeProviderResult : IDisposable
{
    private readonly NativeProviderSession _owner;
    private readonly ISchemaAssessmentResult _result;
    private readonly NativeContainerAdapter.Input? _output;
    private int _closed;
    internal NativeProviderResult(NativeProviderSession owner, ISchemaAssessmentResult result, NativeContainerAdapter.Input? output)
    { _owner = owner; _result = result; _output = output; }
    private void Check() => ObjectDisposedException.ThrowIf(Volatile.Read(ref _closed) != 0, this);
    /// <summary>Gets the existing public assessment result, without exposing provider input internals.</summary>
    /// <exception cref="ObjectDisposedException">This result is disposed.</exception>
    public ISchemaAssessmentResult Assessment { get { Check(); return _result; } }
    /// <summary>Admits this result's exact owned input, origin and resource lifetime into its original session.</summary>
    /// <param name="cancellationToken">Admission cancellation.</param>
    /// <returns>The existing public Core snapshot admission; the caller disposes that admission.</returns>
    /// <exception cref="ObjectDisposedException">The result, output or session is disposed.</exception>
    /// <exception cref="InvalidOperationException">The result has no complete valid output.</exception>
    /// <exception cref="ArgumentException">This output already has a live admission.</exception>
    /// <exception cref="OperationCanceledException">Admission is canceled.</exception>
    public SchemaQuerySnapshot OpenSnapshot(CancellationToken cancellationToken = default)
    {
        Check(); _owner.Check(); cancellationToken.ThrowIfCancellationRequested();
        if (_output is null || _result.Status != SchemaAssessmentStatus.Valid || _result.Evidence?.IsComplete != true)
            throw new InvalidOperationException("Only a complete valid provider result can be admitted.");
        return _owner.Engine.OpenSnapshot(_output, _output, _result.Evidence.Origin, cancellationToken: cancellationToken).Value;
    }
    /// <summary>Revokes this result's provider-owned output without disposing its engine session or caller XML.</summary>
    public void Dispose() { if (Interlocked.Exchange(ref _closed, 1) == 0) _output?.Dispose(); }
}
