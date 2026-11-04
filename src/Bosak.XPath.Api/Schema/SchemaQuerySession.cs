// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 03 November 2026
// PURPOSE              : Exposes bounded ordinary-source sessions over qualified raw-operation lifetime ownership.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 03-11-2026     | Creation; bounded public ordinary-source facade (1.45.0).                               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 04-11-2026     | Select exact arithmetic with captured numeric limits and guarded raw dispatch (1.48.0).  |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 04-11-2026     | Describe exact scalar profiles and admit checked Boolean/string results (1.49.0).        |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System;
using System.Text.Json;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.XPath.Api.Schema;
/// <summary>Engine-created ordinary-source XPath session with an explicit bounded semantic profile.</summary>
/// <remarks>String/boolean queries and selected exact arithmetic/comparison/string profiles are supported. Schema operations remain unavailable; profiles do not impose a hard time/graph-size budget.</remarks>
public sealed class SchemaQuerySession : IDisposable
{
    private bool _closed;
    private readonly QuerySourceSession _runtime;
    private readonly object _gate = new();
    private readonly Dictionary<ISchemaAssessmentInput, InputAdmission> _inputs = new(ReferenceEqualityComparer.Instance);
    private readonly SchemaQueryLimits _limits;
    private readonly bool _numeric;
    private SchemaQuerySession(QuerySourceSession runtime,SchemaQueryLimits limits,bool numeric=false,string? profile=null)
    { _runtime=runtime;_limits=limits;_numeric=numeric;Environment=new SemanticEnvironment(runtime.Environment,numeric?(profile??ExactArithmeticPlan.Profile):OrdinaryQueryProfile.Name); }
    internal static SchemaResult<T> Unsupported<T>() => new Outcome<T>(default!, false);
    private static SchemaResult<T> Supported<T>(T value) => new Outcome<T>(value, true);
    private sealed class Outcome<T>(T value, bool supported) : SchemaResult<T>
    {
        /// <inheritdoc/>
        public override SchemaResultKind Kind => supported ? SchemaResultKind.Supported : SchemaResultKind.Unsupported;
        /// <inheritdoc/>
        public override T Value => supported ? value : throw new InvalidOperationException("Unsupported outcomes have no value.");
        /// <inheritdoc/>
        public override string? DiagnosticCode => supported ? null : "BOSAK-ORDINARY-PROFILE-UNSUPPORTED";
    }
    /// <summary>Captures the actual immutable environment and optionally binds a compiled assessor.</summary>
    /// <param name="expression">XPath source.</param>
    /// <param name="limits">Explicit finite counts and nonnegative scale.</param>
    /// <param name="options">Existing options copied under the selected capture policy.</param>
    /// <param name="assessor">Optional borrowed schema assessor.</param>
    /// <param name="cancellationToken">Compilation/binding cancellation.</param>
    /// <returns>An ordinary or bounded exact arithmetic/scalar session, or Unsupported for unavailable syntax, options, capacity or assessors.</returns>
    /// <exception cref="ArgumentException">The source or explicit limits are invalid.</exception>
    /// <exception cref="OperationCanceledException">Compilation was canceled.</exception>
    public static SchemaResult<SchemaQuerySession> CompileXPath(string expression, SchemaQueryLimits limits, CompileOptions? options = null, ISchemaAssessor? assessor = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression); limits.Validate(); cancellationToken.ThrowIfCancellationRequested();
        if (assessor is not null) return Unsupported<SchemaQuerySession>();
        if (!SchemaSessionCompilation.TrySnapshotOptions(options ?? CompileOptions.Default,out var frozen,out _))return Unsupported<SchemaQuerySession>();
        QuerySourceSession? runtime;bool numeric=false;string? profile=null;
        if(OrdinaryQueryProfile.Supports(expression,frozen!))
        {
            if(!SchemaSessionCompilation.TryCompileOrdinary(expression,frozen!,limits.MaxOpenSnapshots,out runtime,out _))return Unsupported<SchemaQuerySession>();
        }
        else
        {
            if(!ExactArithmeticPlan.TryCompile(expression,frozen!,limits.MaxNumericDigits,limits.MaxDecimalScale,out var plan,cancellationToken))return Unsupported<SchemaQuerySession>();
            numeric=true;profile=plan!.ProfileName;
            var captured=SchemaSessionCompilation.Capture(frozen!,frozen!.Namespaces,frozen.DefaultElementNamespace);
            var descriptor=plan!.ProfileName==ExactArithmeticPlan.Profile
                ?JsonSerializer.Serialize(new{format=plan.DescriptorName,staticContext=captured.Fingerprint,profile=plan.ProfileName,limits.MaxNumericDigits,limits.MaxDecimalScale})
                :JsonSerializer.Serialize(new{format=plan.DescriptorName,staticContext=captured.Fingerprint,profile=plan.ProfileName,limits.MaxNumericDigits,limits.MaxDecimalScale,serializationCapacity=plan.SerializationCapacity});
            var environment=new SchemaQueryEnvironment(captured.Namespaces,descriptor);
            runtime=new QuerySourceSession(environment,plan!.Evaluate,limits.MaxOpenSnapshots);
        }
        try { cancellationToken.ThrowIfCancellationRequested(); return Supported(new SchemaQuerySession(runtime!,limits,numeric,profile)); }
        catch { runtime!.Dispose(); throw; }
    }
    /// <summary>The exact engine-issued immutable compilation environment.</summary>
    public IQuerySemanticEnvironment Environment { get; }
    /// <summary>The exact bound advisor, or null for the ordinary-XML profile.</summary>
    public ISchemaAssessmentAdvisor? Advisor => null;
    /// <summary>Admits one borrowed input identity and its actual resource lifetime.</summary>
    /// <param name="input">Immutable/version-guarded input.</param>
    /// <param name="lifetime">Admission over already-owned resources.</param>
    /// <param name="originalAssessment">Issued original assessment, absent for ordinary XML.</param>
    /// <param name="compatibilityView">Explicit admitted source/target proof when required.</param>
    /// <param name="cancellationToken">Admission cancellation.</param>
    /// <returns>An ordinary-source admission; original assessment and compatibility return Unsupported in this profile.</returns>
    /// <exception cref="ArgumentException">Input or lifetime is null, duplicated or has invalid bindings.</exception>
    /// <exception cref="InvalidOperationException">The admission quota is full or input binding changes.</exception>
    /// <exception cref="ObjectDisposedException">The session or owner admission is revoked.</exception>
    /// <exception cref="OperationCanceledException">Admission was canceled.</exception>
    public SchemaResult<SchemaQuerySnapshot> OpenSnapshot(ISchemaAssessmentInput input, IQueryOperationLifetime lifetime, ISchemaAssessmentOrigin? originalAssessment = null, ISchemaCompatibilityView? compatibilityView = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input); ArgumentNullException.ThrowIfNull(lifetime);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate) ObjectDisposedException.ThrowIf(_closed, this);
        if (originalAssessment is not null || compatibilityView is not null) return Unsupported<SchemaQuerySnapshot>();
        var adapter = new InputAdmission(this, input, lifetime, cancellationToken);
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_closed, this);
            if (!_inputs.TryAdd(input, adapter)) throw new ArgumentException("Input is already admitted or entering.", nameof(input));
        }
        try
        {
            var snapshot = _runtime.OpenSnapshot(_runtime.Environment, adapter, adapter.Enter, cancellation: cancellationToken);
            return Supported(new SchemaQuerySnapshot(snapshot, adapter.Binding));
        }
        catch { Remove(adapter); throw; }
    }
    /// <summary>Owns admissions through the complete callback consumption/detachment interval.</summary>
    /// <typeparam name="T">Caller result type; returned graphs are not recursively detached.</typeparam>
    /// <param name="snapshots">An immutable set of same-session admissions; empty for source-free work.</param>
    /// <param name="work">Borrowed operation callback.</param>
    /// <param name="cancellationToken">Cooperative raw-work cancellation.</param>
    /// <returns>The callback result after admissions finish.</returns>
    public T Run<T>(ImmutableArray<SchemaQuerySnapshot> snapshots, Func<SchemaQueryOperation, T> work, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        return _runtime.Run(_runtime.Environment, Members(snapshots), op => work(new SchemaQueryOperation(op,_limits,_numeric)), cancellationToken);
    }
    /// <summary>Retains admissions until the raw callback task completes, independent of canceled waits.</summary>
    /// <typeparam name="T">Caller result type.</typeparam>
    /// <param name="snapshots">Same-session snapshot admissions.</param>
    /// <param name="work">Raw asynchronous callback including deferred consumption.</param>
    /// <param name="cancellationToken">Cooperative raw-work cancellation.</param>
    /// <returns>The actual operation task.</returns>
    public Task<T> RunAsync<T>(ImmutableArray<SchemaQuerySnapshot> snapshots, Func<SchemaQueryOperation, Task<T>> work, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        return _runtime.RunAsync(_runtime.Environment, Members(snapshots), op => work(new SchemaQueryOperation(op,_limits,_numeric)), cancellationToken);
    }
    /// <summary>Waits for admitted raw work; canceling this wait does not end that work.</summary>
    /// <param name="cancellationToken">Wait cancellation only.</param>
    /// <returns>A task completing when admitted raw work has finished.</returns>
    public Task DrainAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _runtime.DrainAsync().WaitAsync(cancellationToken);
    }
    /// <summary>Revokes engine admissions; does not dispose borrowed provider resources or force raw work to end.</summary>
    public void Dispose()
    {
        lock (_gate) { if (_closed) return; _closed = true; }
        _runtime.Dispose();
    }
    private ImmutableArray<QuerySourceSession.Snapshot> Members(ImmutableArray<SchemaQuerySnapshot> snapshots)
    {
        if (snapshots.IsDefault) throw new ArgumentException("Explicit snapshot membership is required.", nameof(snapshots));
        var result = ImmutableArray.CreateBuilder<QuerySourceSession.Snapshot>(snapshots.Length);
        foreach (var snapshot in snapshots)
            result.Add((snapshot ?? throw new ArgumentException("Null snapshot member.", nameof(snapshots))).Runtime);
        return result.MoveToImmutable();
    }
    private void Remove(InputAdmission input)
    {
        lock (_gate)
            if (_inputs.TryGetValue(input.Input, out var current) && ReferenceEquals(current, input)) _inputs.Remove(input.Input);
    }
    private sealed class SemanticEnvironment(SchemaQueryEnvironment environment,string profile) : IQuerySemanticEnvironment, IQueryStaticContext
    {
        QueryHostLanguage IQuerySemanticEnvironment.HostLanguage => QueryHostLanguage.XPath;
        string IQuerySemanticEnvironment.HostVersion => "3.1";
        Uri IQuerySemanticEnvironment.XdmSpecificationRevision => new("https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/");
        string IQuerySemanticEnvironment.BehaviorProfile => profile;
        IQueryStaticContext IQuerySemanticEnvironment.StaticContext => this;
        string IQuerySemanticEnvironment.DescriptorFormatVersion => profile==ExactArithmeticPlan.Profile?ExactArithmeticPlan.DescriptorFormat:profile==ExactArithmeticPlan.ScalarProfile?ExactArithmeticPlan.ScalarDescriptorFormat:SchemaQueryEnvironment.DescriptorFormat;
        string IQuerySemanticEnvironment.StaticContextFingerprint => environment.Fingerprint;
        ImmutableDictionary<string,string> IQueryStaticContext.Namespaces => environment.Namespaces;
    }
    private sealed class InputAdmission(SchemaQuerySession session, ISchemaAssessmentInput input,
        IQueryOperationLifetime lifetime, CancellationToken openingCancellation) : IQuerySourceInput, IDisposable
    {
        internal ISchemaAssessmentInput Input => input;
        private IQueryOperationLease? _lease;
        private ISchemaSourceBinding? _source;
        private ISchemaSnapshotBinding? _binding;
        internal ISchemaSnapshotBinding Binding => _binding!;
        internal IDisposable Enter()
        {
            _lease = lifetime.Enter(openingCancellation) ?? throw new InvalidOperationException("Owner returned no admission lease.");
            try
            {
                Check(openingCancellation);
                _source = input.Source ?? throw new ArgumentException("Input has no source binding.");
                _binding = input.Snapshot ?? throw new ArgumentException("Input has no snapshot binding.");
                Check(openingCancellation); return this;
            }
            catch (Exception primary) { SchemaEvidenceKernel.PreserveFailure(primary, Dispose); throw; }
        }
        private void Check(CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            var lease = _lease ?? throw new ObjectDisposedException(nameof(InputAdmission));
            lease.ThrowIfUnavailable(cancellation); input.ThrowIfUnavailable(cancellation);
            if (_source is not null && !ReferenceEquals(input.Source, _source)
                || _binding is not null && !ReferenceEquals(input.Snapshot, _binding))
                throw new InvalidOperationException("Input binding changed during its admission.");
            cancellation.ThrowIfCancellationRequested();
        }
        IXdmNode IQuerySourceInput.Root { get { Check(default); return input.Root; } }
        void IQuerySourceInput.ThrowIfUnavailable() => Check(default);
        void IQuerySourceInput.ThrowIfUnavailable(CancellationToken cancellation) => Check(cancellation);
        bool IQuerySourceInput.OwnsNode(IXdmNode node) { Check(default); return input.OwnsNode(node); }
        /// <summary>Releases only the admission, preserving provider resource ownership.</summary>
        public void Dispose()
        {
            var lease = Interlocked.Exchange(ref _lease, null);
            try { lease?.Dispose(); } finally { session.Remove(this); }
        }
    }
}
/// <summary>Engine-created revocable admission over a caller-owned input snapshot.</summary>
public sealed class SchemaQuerySnapshot : IDisposable
{
    internal QuerySourceSession.Snapshot Runtime { get; }
    internal SchemaQuerySnapshot(QuerySourceSession.Snapshot runtime, ISchemaSnapshotBinding binding)
    { Runtime = runtime; Binding = binding; }
    /// <summary>The descriptive provider snapshot binding; no new authority or lease.</summary>
    public ISchemaSnapshotBinding Binding { get; }
    /// <summary>Revokes this engine admission; actual owner cleanup follows admitted raw work.</summary>
    public void Dispose()
    { Runtime.Dispose(); }
}
/// <summary>Callback-borrowed operation over its complete admitted snapshot set.</summary>
public sealed class SchemaQueryOperation
{
    private readonly QuerySourceSession.Operation _runtime;
    private readonly SchemaQueryLimits _limits;
    private readonly bool _numeric;
    internal SchemaQueryOperation(QuerySourceSession.Operation runtime,SchemaQueryLimits limits,bool numeric)
    { _runtime=runtime;_limits=limits;_numeric=numeric; }
    private void ValidateInput(XdmValue value)
    {
        _runtime.Check();
        if(_numeric){_ = new ExactArithmeticLimits(_limits.MaxNumericDigits,_limits.MaxDecimalScale).Read(value);return;}
        if(value.Kind is not(XdmValueKind.String or XdmValueKind.Boolean or XdmValueKind.Node))
            throw new NotSupportedException("The ordinary string/boolean profile refuses numeric and structured inputs.");
    }
    /// <summary>Projects an admitted root under this operation's availability checks.</summary>
    /// <param name="snapshot">A snapshot in this operation's admitted set.</param>
    /// <returns>The borrowed root; no ownership transfer or automatic detachment.</returns>
    public IXdmNode GetRoot(SchemaQuerySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot); return _runtime.GetRoot(snapshot.Runtime);
    }
    /// <summary>Admits an authority-issued atomic/list/union container before reading its contents.</summary>
    /// <param name="snapshot">The exact source admission in this operation.</param>
    /// <param name="value">The immutable issued provider value.</param>
    /// <returns>Unsupported in the ordinary-source profile; no provider getters are invoked.</returns>
    public SchemaResult<XdmValue> ReadTypedValue(SchemaQuerySnapshot snapshot, ISchemaTypedValue value)
    {
        ArgumentNullException.ThrowIfNull(snapshot); ArgumentNullException.ThrowIfNull(value);
        _runtime.RequireMember(snapshot.Runtime); return SchemaQuerySession.Unsupported<XdmValue>();
    }
    /// <summary>Requests explicit provider-owned simple construction and admits its result.</summary>
    /// <param name="input">Atomic/lexical input under the selected constructor profile.</param>
    /// <param name="targetType">Owned simple target type.</param>
    /// <param name="episode">Captured assessment constants.</param>
    /// <returns>Unsupported in the ordinary-source profile; no provider getters are invoked.</returns>
    public SchemaResult<XdmValue> ConstructSimpleValue(XdmValue input, ISchemaComponentHandle targetType, ISchemaAssessmentEpisode episode)
    {
        _runtime.Check(); ArgumentNullException.ThrowIfNull(targetType); ArgumentNullException.ThrowIfNull(episode);
        return SchemaQuerySession.Unsupported<XdmValue>();
    }
    /// <summary>Evaluates captured XPath or XQuery; XSLT uses its typed transform producer instead.</summary>
    /// <param name="context">Dynamic inputs whose selected policy must be enforced.</param>
    /// <returns>A copied scalar selected by the ordinary, exact arithmetic or exact scalar profile.</returns>
    /// <exception cref="NotSupportedException">Context state, numeric or structured inputs, or the result is outside this profile.</exception>
    /// <exception cref="ArgumentException">Incoming nodes do not have exactly one admitted owner.</exception>
    /// <exception cref="ObjectDisposedException">The operation or a required admission is revoked.</exception>
    /// <exception cref="OperationCanceledException">Raw work was canceled.</exception>
    public XdmValue Evaluate(EvaluationContext context)
    {
        _runtime.Check(); ArgumentNullException.ThrowIfNull(context);
        // These exact cardinalities bound copying, not owner workload defaults. Capture rechecks both counts.
        var variables = context.SnapshotVariables();
        foreach (var value in variables.Values)ValidateInput(value);
        var inputs = _runtime.CaptureInputs(context, Math.Max(1, variables.Count), Math.Max(1, context.SnapshotNamespaces().Count));
        foreach (var pair in inputs.VariableBindings)ValidateInput(pair.Value);
        var result = _runtime.EvaluateAtomic(inputs);
        if(_numeric){if(result.Kind is not(XdmValueKind.Boolean or XdmValueKind.String))_ = new ExactArithmeticLimits(_limits.MaxNumericDigits,_limits.MaxDecimalScale).Read(result);_runtime.Check();}
        else if(result.Kind is not(XdmValueKind.String or XdmValueKind.Boolean))
            throw new NotSupportedException("The ordinary profile returns copied string/boolean scalars only.");
        return result;
    }
}
