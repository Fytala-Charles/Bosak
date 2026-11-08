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
//                      | Charles Korthout | 0.4   | 04-11-2026     | Identify bounded quotient profile in captured environment (1.50.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 04-11-2026     | Bind exact division policy and algorithm into descriptor (1.51.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 04-11-2026     | Capture numeric constructor and retained division policies (1.52.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.7   | 04-11-2026     | Qualify internal public-contract advisor binding and input adapter (1.56.0).               |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.8   | 04-11-2026     | Capture an internal typed identity profile and candidate evaluation route (1.57.0).         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.9   | 05-11-2026     | Activate bounded public typed identity with exact ownership (1.58.0).                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.10   | 05-11-2026     | Execute bounded public typed numeric operations with live authorization (1.60.0).          |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.11   | 05-11-2026     | Dispatch bounded public original-node nilled with copied results (1.63.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.12   | 05-11-2026     | Dispatch bounded original-node data with ordered atomic results (1.64.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.13 | 05-11-2026     | Guard source-free construction and operation-local capture (1.66.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.14 | 05-11-2026     | Capture original binding for private compatible proof admission (1.68.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.15 | 05-11-2026     | Separate stable payload scope from current compatible issuer (1.69.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.16 | 05-11-2026     | Activate public authenticated compatible handoff and singleton identity (1.70.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.17 | 05-11-2026     | Bind internal captured hosts to exact public session operations (1.71.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.18 | 05-11-2026     | Dispatch public ordinary XQuery through its exact captured host binding (1.72.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.19 | 06-11-2026     | Route exact host-owned structured outcomes and guard post-cleanup delivery (1.75.0). |
//                      | Charles Korthout | 0.20 | 06-11-2026     | Describe ordinary public XSLT captured host (1.76.0). |
//                      | Charles Korthout | 0.21 | 06-11-2026     | Expose additive operation-bound compatible atomic provenance member (1.78.0). |
//                      | Charles Korthout | 0.22 | 06-11-2026     | Deliver complete ordinary typed containers without scalar registration (1.79.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.23 | 06-11-2026     | Enable existing complete-container member in the compatible profile (1.80.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.24 | 06-11-2026     | Document wider complete-container transport without execution expansion (1.81.0). |
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
/// <summary>Engine-created query session with an explicit ordinary, numeric, typed or captured host profile.</summary>
/// <remarks>String/boolean queries and selected exact arithmetic/comparison/string profiles are supported. Assessor-bound typed profiles support single-atomic identity, bounded exact numeric computation and original-node nilled metadata/ordered atomization. A source-free operation-local construction profile is selected by $constructed; bounded compatible singleton identity uses an engine-issued source handoff; profiles do not impose a hard time/graph-size budget.</remarks>
public sealed class SchemaQuerySession : IDisposable
{
    private bool _closed;
    private readonly QuerySourceSession _runtime;
    private readonly object _gate = new();
    private readonly Dictionary<ISchemaAssessmentInput, InputAdmission> _inputs = new(ReferenceEqualityComparer.Instance);
    private readonly SchemaQueryLimits _limits;
    private readonly CapturedHostBinding? _host;
    private readonly bool _numeric;
    private readonly bool _constructed;
    private readonly bool _compatiblePublic;
    private readonly bool _compatibleCandidate;
    private readonly bool _compatibleIdentityCandidate;
    private readonly bool _nilled;
    private readonly bool _data;
    private SchemaAdvisorBinding? _binding;
    private SchemaQuerySession(QuerySourceSession runtime,SchemaQueryLimits limits,bool numeric=false,string? profile=null,CapturedHostBinding? host=null)
    { _host=host;_runtime=runtime;_limits=limits;_numeric=numeric;_compatiblePublic=profile==CompatibleAdmissionPlan.PublicProfile;_compatibleCandidate=profile is CompatibleAdmissionPlan.Profile or CompatibleAdmissionPlan.IdentityProfile;_compatibleIdentityCandidate=profile==CompatibleAdmissionPlan.IdentityProfile;_constructed=profile==ConstructedAtomicPlan.Profile;_nilled=profile==OriginalNodeNilledPlan.Profile;_data=profile==OriginalNodeDataPlan.Profile;Environment=new SemanticEnvironment(runtime.Environment,profile??(numeric?ExactArithmeticPlan.Profile:OrdinaryQueryProfile.Name),host?.Host??QueryHostLanguage.XPath); }
    internal void RequireHostOperation(QuerySourceSession.Operation operation)=>operation.RequireOwner(_runtime);
    internal static SchemaQuerySession CreateHostCandidate(CapturedHostBinding host,SchemaQueryLimits limits)
    {
        ArgumentNullException.ThrowIfNull(host);limits.Validate();
        var runtime=new QuerySourceSession(host.Environment,static _=>throw new NotSupportedException("A captured host requires its exact private producer."),limits.MaxOpenSnapshots);
        return new SchemaQuerySession(runtime,limits,profile:host.ProfileName,host:host);
    }
    internal static SchemaResult<SchemaQuerySession> CompileTypedIdentityCandidate(string expression,SchemaQueryLimits limits,CompileOptions? options=null,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(expression);limits.Validate();token.ThrowIfCancellationRequested();
        if(!TypedIdentityPlan.TryCompile(expression,limits,options??CompileOptions.Default,token,out var runtime))return Unsupported<SchemaQuerySession>();
        try{token.ThrowIfCancellationRequested();return Supported(new SchemaQuerySession(runtime!,limits,profile:TypedIdentityPlan.Profile));}
        catch{runtime!.Dispose();throw;}
    }
    internal static SchemaResult<SchemaQuerySession> CompileCompatibleAdmissionCandidate(string expression,SchemaQueryLimits limits,
        ISchemaAssessor assessor,CompileOptions? options=null,CancellationToken token=default)
        =>CompileCompatibleCandidateCore(expression,limits,assessor,options,token,false);
    internal static SchemaResult<SchemaQuerySession> CompileCompatibleIdentityCandidate(string expression,SchemaQueryLimits limits,
        ISchemaAssessor assessor,CompileOptions? options=null,CancellationToken token=default)
        =>CompileCompatibleCandidateCore(expression,limits,assessor,options,token,true);
    private static SchemaResult<SchemaQuerySession> CompileCompatibleCandidateCore(string expression,SchemaQueryLimits limits,
        ISchemaAssessor assessor,CompileOptions? options,CancellationToken token,bool identity)
    {
        ArgumentNullException.ThrowIfNull(expression);ArgumentNullException.ThrowIfNull(assessor);limits.Validate();token.ThrowIfCancellationRequested();
        if(!CompatibleAdmissionPlan.TryCompile(expression,limits,options??CompileOptions.Default,token,out var runtime,identity))return UnsupportedTyped<SchemaQuerySession>();
        var session=new SchemaQuerySession(runtime!,limits,profile:identity?CompatibleAdmissionPlan.IdentityProfile:CompatibleAdmissionPlan.Profile);
        try
        {
            session._binding=session.BindAdvisorCandidate(assessor,token);
            if(session._binding is null||session._binding.Advisor is not IQueryOperationLifetime||session._binding.Advisor is not ISchemaCompatibilityAuthority)
            {session.Dispose();return UnsupportedTyped<SchemaQuerySession>();}
            session.CheckCandidate(token);return Supported(session);
        }
        catch(Exception primary){SchemaEvidenceKernel.PreserveFailure(primary,session.Dispose);throw;}
    }
    private static SchemaResult<SchemaQuerySession> CompileCompatiblePublic(string expression,SchemaQueryLimits limits,
        ISchemaAssessor assessor,CompileOptions? options,CancellationToken token)
    {
        if(!CompatibleAdmissionPlan.TryCompile(expression,limits,options??CompileOptions.Default,token,out var runtime,true,true))return UnsupportedTyped<SchemaQuerySession>();
        var session=new SchemaQuerySession(runtime!,limits,profile:CompatibleAdmissionPlan.PublicProfile);
        try
        {
            session._binding=session.BindAdvisorCandidate(assessor,token);
            if(session._binding is null||session._binding.Advisor is not IQueryOperationLifetime||session._binding.Advisor is not ISchemaCompatibilityAuthority)
            {session.Dispose();return UnsupportedTyped<SchemaQuerySession>();}
            session.CheckCandidate(token);return Supported(session);
        }
        catch(Exception primary){SchemaEvidenceKernel.PreserveFailure(primary,session.Dispose);throw;}
    }
    /// <summary>Authenticates and retains an original source admission for bounded compatible identity.</summary>
    /// <param name="sourceSnapshot">An engine-created original snapshot with owned source assessment.</param>
    /// <param name="cancellationToken">Cooperative admission cancellation.</param>
    /// <returns>A three-decision outcome; only Compatible owns a new target snapshot.</returns>
    /// <exception cref="ArgumentException">Source metadata or the provider's proof is malformed.</exception>
    /// <exception cref="InvalidOperationException">The target admission quota is exhausted.</exception>
    /// <exception cref="ObjectDisposedException">A required session or source admission is closed.</exception>
    /// <exception cref="OperationCanceledException">Admission was canceled.</exception>
    public SchemaQueryCompatibilityResult OpenCompatibleSnapshot(SchemaQuerySnapshot sourceSnapshot,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(sourceSnapshot);CheckCandidate(cancellationToken);
        if(!_compatiblePublic)return new(SchemaCompatibilityDecision.Unsupported,null);
        var admitted=CompatibleAdmissionPlan.Open(_runtime,Environment,_binding!,sourceSnapshot,cancellationToken,-1);
        if(admitted.Value is null)return new(admitted.Decision,null);
        try
        {
            var snapshot=new SchemaQuerySnapshot(admitted.Value.Snapshot,sourceSnapshot.Binding){CompatibleState=admitted.Value};
            var result=new SchemaQueryCompatibilityResult(admitted.Decision,snapshot);admitted.Value.CheckEngine(cancellationToken);return result;
        }
        catch(Exception primary){SchemaEvidenceKernel.PreserveFailure(primary,admitted.Value.Dispose);throw;}
    }
    internal CompatibleAdmissionPlan.Result OpenCompatibleCandidate(SchemaQuerySnapshot source,CancellationToken token=default,int publicationFault=-1)
    {
        CheckCandidate(token);ArgumentNullException.ThrowIfNull(source);
        if(!_compatibleCandidate)throw new NotSupportedException("This session has no internal compatible admission profile.");
        return CompatibleAdmissionPlan.Open(_runtime,Environment,_binding!,source,token,publicationFault);
    }
    internal T RunCompatibleCandidate<T>(CompatibleAdmissionPlan.Admission admission,Func<Action,T> work,CancellationToken token=default)
    {
        CheckCandidate(token);ArgumentNullException.ThrowIfNull(admission);ArgumentNullException.ThrowIfNull(work);
        if(!_compatibleCandidate)throw new NotSupportedException("This session has no internal compatible admission profile.");
        return admission.Run(_runtime,_binding!,work,token);
    }
    internal T RunCompatibleIdentityCandidate<T>(CompatibleAdmissionPlan.Admission admission,Func<CompatibleAdmissionPlan.TypedOperation,T> work,CancellationToken token=default)
    {
        CheckCandidate(token);ArgumentNullException.ThrowIfNull(admission);ArgumentNullException.ThrowIfNull(work);
        if(!_compatibleIdentityCandidate)throw new NotSupportedException("This session does not select internal compatible identity.");
        return admission.RunTyped(_runtime,_binding!,_limits,work,token);
    }
    internal static SchemaResult<T> Unsupported<T>() => new Outcome<T>(default!, false);
    internal static SchemaResult<T> Supported<T>(T value) => new Outcome<T>(value, true);
    internal static SchemaResult<T> UnsupportedNumeric<T>() => new Outcome<T>(default!,false,"BOSAK-TYPED-NUMERIC-UNSUPPORTED");
    internal static SchemaResult<T> UnsupportedData<T>() => new Outcome<T>(default!,false,"BOSAK-ORIGINAL-DATA-UNSUPPORTED");
    internal static SchemaResult<T> UnsupportedNilled<T>() => new Outcome<T>(default!,false,"BOSAK-ORIGINAL-NILLED-UNSUPPORTED");
    internal static SchemaResult<T> UnsupportedTyped<T>() => new Outcome<T>(default!,false,"BOSAK-TYPED-IDENTITY-UNSUPPORTED");
    internal static SchemaResult<T> UnsupportedConstructed<T>()=>new Outcome<T>(default!,false,"BOSAK-CONSTRUCTED-ATOMIC-UNSUPPORTED");
    internal static SchemaResult<XdmValue> InvalidConstruction()=>new InvalidConstructionOutcome();
    private sealed class InvalidConstructionOutcome:SchemaResult<XdmValue>
    {
        /// <inheritdoc/>
        public override SchemaResultKind Kind=>SchemaResultKind.Invalid;
        /// <inheritdoc/>
        public override XdmValue Value=>throw new InvalidOperationException("Invalid construction has no value.");
        /// <inheritdoc/>
        public override string DiagnosticCode=>"BOSAK-CONSTRUCTION-INVALID";
    }
    private sealed class Outcome<T>(T value, bool supported,string diagnostic="BOSAK-ORDINARY-PROFILE-UNSUPPORTED") : SchemaResult<T>
    {
        /// <inheritdoc/>
        public override SchemaResultKind Kind => supported ? SchemaResultKind.Supported : SchemaResultKind.Unsupported;
        /// <inheritdoc/>
        public override T Value => supported ? value : throw new InvalidOperationException("Unsupported outcomes have no value.");
        /// <inheritdoc/>
        public override string? DiagnosticCode => supported ? null : diagnostic;
    }
    /// <summary>Captures the actual immutable environment and optionally binds a compiled assessor.</summary>
    /// <param name="expression">XPath source.</param>
    /// <param name="limits">Explicit finite counts and nonnegative scale.</param>
    /// <param name="options">Existing options copied under the selected capture policy.</param>
    /// <param name="assessor">Optional borrowed schema assessor.</param>
    /// <param name="cancellationToken">Compilation/binding cancellation.</param>
    /// <returns>An ordinary or bounded exact arithmetic/scalar session, or an assessor-bound typed identity/numeric/original-node nilled/data or operation-local construction/compatible identity session; Unsupported for unavailable syntax, options or provider binding.</returns>
    /// <exception cref="ArgumentException">The source, explicit limits or provider binding are invalid.</exception>
    /// <exception cref="OperationCanceledException">Compilation was canceled.</exception>
    public static SchemaResult<SchemaQuerySession> CompileXPath(string expression, SchemaQueryLimits limits, CompileOptions? options = null, ISchemaAssessor? assessor = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression); limits.Validate(); cancellationToken.ThrowIfCancellationRequested();
        if (assessor is not null)
        {
            if(expression.Trim()=="$compatible")return CompileCompatiblePublic(expression,limits,assessor,options,cancellationToken);
            bool constructed=ConstructedAtomicPlan.TryCompile(expression,limits,options??CompileOptions.Default,cancellationToken,out var typedRuntime);
            bool data=!constructed&&OriginalNodeDataPlan.TryCompile(expression,limits,options??CompileOptions.Default,cancellationToken,out typedRuntime);
            bool nilled=!constructed&&!data&&OriginalNodeNilledPlan.TryCompile(expression,limits,options??CompileOptions.Default,cancellationToken,out typedRuntime);
            bool identity=!constructed&&!data&&!nilled&&TypedIdentityPlan.TryCompilePublic(expression,limits,options??CompileOptions.Default,cancellationToken,out typedRuntime);
            if(!constructed&&!data&&!nilled&&!identity&&!TypedNumericPlan.TryCompile(expression,limits,options??CompileOptions.Default,cancellationToken,out typedRuntime))return UnsupportedTyped<SchemaQuerySession>();
            var typed=new SchemaQuerySession(typedRuntime!,limits,profile:constructed?ConstructedAtomicPlan.Profile:data?OriginalNodeDataPlan.Profile:nilled?OriginalNodeNilledPlan.Profile:identity?TypedIdentityPlan.PublicProfile:TypedNumericPlan.Profile);
            try
            {
                typed._binding=typed.BindAdvisorCandidate(assessor,cancellationToken);
                if(typed._binding is null||constructed&&typed._binding.Advisor is not IQueryOperationLifetime){typed.Dispose();return constructed?UnsupportedConstructed<SchemaQuerySession>():data?UnsupportedData<SchemaQuerySession>():nilled?UnsupportedNilled<SchemaQuerySession>():identity?UnsupportedTyped<SchemaQuerySession>():UnsupportedNumeric<SchemaQuerySession>();}
                cancellationToken.ThrowIfCancellationRequested();return Supported(typed);
            }
            catch(Exception primary){SchemaEvidenceKernel.PreserveFailure(primary,typed.Dispose);throw;}
        }
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
            var descriptor=plan!.ProfileName==ExactArithmeticPlan.ConstructorProfile
                ?JsonSerializer.Serialize(new{format=plan.DescriptorName,staticContext=captured.Fingerprint,profile=plan.ProfileName,limits.MaxNumericDigits,limits.MaxDecimalScale,serializationCapacity=plan.SerializationCapacity,constructorPolicy="primitive-numeric-v1",integerConversion="truncate-toward-zero",divisionPolicy="exact-terminating",divisionAlgorithm="gcd-factor-chunks-v1"})
                :plan!.ProfileName==ExactArithmeticPlan.DivisionProfile
                ?JsonSerializer.Serialize(new{format=plan.DescriptorName,staticContext=captured.Fingerprint,profile=plan.ProfileName,limits.MaxNumericDigits,limits.MaxDecimalScale,serializationCapacity=plan.SerializationCapacity,divisionPolicy="exact-terminating",divisionAlgorithm="gcd-factor-chunks-v1"})
                :plan!.ProfileName==ExactArithmeticPlan.Profile
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
    public ISchemaAssessmentAdvisor? Advisor => _binding?.Advisor;
    private void CheckCandidate(CancellationToken token)
    {token.ThrowIfCancellationRequested();lock(_gate)ObjectDisposedException.ThrowIf(_closed,this);}
    internal SchemaAdvisorBinding? BindAdvisorCandidate(ISchemaAssessor assessor,CancellationToken token=default)
        =>SchemaAdvisorBinding.Bind(_runtime,Environment,assessor,CheckCandidate,token);
    internal IQueryTypedContainerIssuer CreateIssuerCandidate(SchemaAdvisorBinding binding,SchemaQuerySnapshot snapshot,ISchemaAssessmentInput input,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(binding);ArgumentNullException.ThrowIfNull(snapshot);ArgumentNullException.ThrowIfNull(input);CheckCandidate(token);
        lock(_gate)
            if(!ReferenceEquals(snapshot.Runtime.Owner,_runtime)||!_inputs.TryGetValue(input,out var projection)||!ReferenceEquals(projection,snapshot.Runtime.Input))
                throw new ArgumentException("Input is not this exact session admission.");
        snapshot.Runtime.Check(token);
        var issuer=binding.ForInput(_runtime,input,snapshot.Runtime.Input,token);
        snapshot.Runtime.Check(token);CheckCandidate(token);return issuer;
    }
    /// <summary>Admits one borrowed input identity and its actual resource lifetime.</summary>
    /// <param name="input">Immutable/version-guarded input.</param>
    /// <param name="lifetime">Admission over already-owned resources.</param>
    /// <param name="originalAssessment">Issued original assessment, absent for ordinary XML.</param>
    /// <param name="compatibilityView">Explicit admitted source/target proof when required.</param>
    /// <param name="cancellationToken">Admission cancellation.</param>
    /// <returns>An ordinary or typed identity admission; original assessment requires the bound advisor. Raw compatibilityView remains Unsupported; use OpenCompatibleSnapshot in its selected profile.</returns>
    /// <exception cref="ArgumentException">Input or lifetime is null, duplicated or has invalid bindings.</exception>
    /// <exception cref="InvalidOperationException">The admission quota is full or input binding changes.</exception>
    /// <exception cref="ObjectDisposedException">The session or owner admission is revoked.</exception>
    /// <exception cref="OperationCanceledException">Admission was canceled.</exception>
    public SchemaResult<SchemaQuerySnapshot> OpenSnapshot(ISchemaAssessmentInput input, IQueryOperationLifetime lifetime, ISchemaAssessmentOrigin? originalAssessment = null, ISchemaCompatibilityView? compatibilityView = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input); ArgumentNullException.ThrowIfNull(lifetime);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate) ObjectDisposedException.ThrowIf(_closed, this);
        if(_compatibleCandidate||_compatiblePublic)return UnsupportedTyped<SchemaQuerySnapshot>();
        if(_constructed)return UnsupportedConstructed<SchemaQuerySnapshot>();
        if (compatibilityView is not null||originalAssessment is not null&&_binding is null) return _binding is null?Unsupported<SchemaQuerySnapshot>():_data?UnsupportedData<SchemaQuerySnapshot>():_runtime.TypedNumericProfile?UnsupportedNumeric<SchemaQuerySnapshot>():UnsupportedTyped<SchemaQuerySnapshot>();
        lock(_gate)if(_inputs.ContainsKey(input))throw new ArgumentException("Input is already admitted or entering.",nameof(input));
        _binding?.ValidateOrigin(originalAssessment,cancellationToken);
        var adapter = new InputAdmission(this, input, lifetime, cancellationToken);
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_closed, this);
            if (!_inputs.TryAdd(input, adapter)) throw new ArgumentException("Input is already admitted or entering.", nameof(input));
        }
        try
        {
            var snapshot = _runtime.OpenSnapshot(_runtime.Environment, adapter, adapter.Enter, cancellation: cancellationToken);
            try
            {
                var result=new SchemaQuerySnapshot(snapshot,adapter.Binding,_binding);
                if(_binding is not null)
                {
                    _binding.ValidateOrigin(originalAssessment,cancellationToken);
                    result.TypedState=new SchemaQuerySnapshot.TypedAdmission(CreateIssuerCandidate(_binding,result,input,cancellationToken),input,originalAssessment);
                }
                return Supported(result);
            }
            catch(Exception primary){SchemaEvidenceKernel.PreserveFailure(primary,snapshot.Dispose);throw;}
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
        if(_compatibleCandidate)throw new NotSupportedException("Internal compatible admission does not expose public data execution.");
        if(_compatiblePublic)
        {
            var admission=CompatibleMember(snapshots,cancellationToken);
            var result=_runtime.Run(_runtime.Environment,Members(snapshots),op=>
            {var frame=admission.BeginPublic(op,_limits);var result=work(new SchemaQueryOperation(op,_limits,false,false,snapshots:snapshots,compatible:frame));frame.Finish();return result;},cancellationToken);
            admission.CheckEngine(cancellationToken);return result;
        }
        var value = _runtime.Run(_runtime.Environment, Members(snapshots), op => work(new SchemaQueryOperation(op,_limits,_numeric,_binding is not null,_nilled||_data?_binding:null,snapshots,_data,_constructed?_binding:null,host:_host)), cancellationToken);
        if(_host is not null)CheckHostDelivery(snapshots,cancellationToken);return value;
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
        if(_compatibleCandidate)throw new NotSupportedException("Internal compatible admission does not expose public data execution.");
        if(_compatiblePublic)return RunCompatibleAsync(snapshots,work,cancellationToken);
        if(_host is not null)return RunHostAsync(snapshots,work,cancellationToken);
        return _runtime.RunAsync(_runtime.Environment, Members(snapshots), op => work(new SchemaQueryOperation(op,_limits,_numeric,_binding is not null,_nilled||_data?_binding:null,snapshots,_data,_constructed?_binding:null,host:_host)), cancellationToken);
    }
    internal QuerySourceSession.RunCompletion<T> RunHostOutcome<T>(CapturedHostBinding host,
        ImmutableArray<SchemaQuerySnapshot> snapshots, Func<SchemaQueryOperation,T> work, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(host); ArgumentNullException.ThrowIfNull(work);
        if (!ReferenceEquals(_host,host) || !ReferenceEquals(host.Session,this))
            throw new ArgumentException("Outcome routing requires the exact captured host session.",nameof(host));
        ImmutableArray<QuerySourceSession.Snapshot> members;
        try { CheckCandidate(token); members = Members(snapshots); }
        catch (Exception error) { return new(false,default!,error,null,[]); }
        var result = _runtime.RunOutcome(_runtime.Environment,members,
            raw => work(new SchemaQueryOperation(raw,_limits,false,false,snapshots:snapshots,host:host)),token);
        if (result.AdmissionError is null && result.WorkError is null)
        {
            try { CheckHostDelivery(snapshots,token); }
            catch (Exception error)
            { return new(result.HasResult,result.Result,null,error,result.CleanupErrors); }
        }
        return result;
    }

    private void CheckHostDelivery(ImmutableArray<SchemaQuerySnapshot> snapshots,CancellationToken token)
    {CheckCandidate(token);foreach(var snapshot in snapshots)snapshot.Runtime.Lifetime.Check();CheckCandidate(token);}
    private async Task<T> RunHostAsync<T>(ImmutableArray<SchemaQuerySnapshot> snapshots,Func<SchemaQueryOperation,Task<T>> work,CancellationToken token)
    {
        var result=await _runtime.RunAsync(_runtime.Environment,Members(snapshots),op=>work(new SchemaQueryOperation(op,_limits,false,false,snapshots:snapshots,host:_host)),token).ConfigureAwait(false);
        CheckHostDelivery(snapshots,token);return result;
    }
    private async Task<T> RunCompatibleAsync<T>(ImmutableArray<SchemaQuerySnapshot> snapshots,Func<SchemaQueryOperation,Task<T>> work,CancellationToken token)
    {
        var admission=CompatibleMember(snapshots,token);
        var result=await _runtime.RunAsync(_runtime.Environment,Members(snapshots),async op=>
        {var frame=admission.BeginPublic(op,_limits);var result=await work(new SchemaQueryOperation(op,_limits,false,false,snapshots:snapshots,compatible:frame)).ConfigureAwait(false);frame.Finish();return result;},token).ConfigureAwait(false);
        admission.CheckEngine(token);return result;
    }
    private CompatibleAdmissionPlan.Admission CompatibleMember(ImmutableArray<SchemaQuerySnapshot> snapshots,CancellationToken token)
    {
        CheckCandidate(token);
        if(snapshots.IsDefault||snapshots.Length!=1||snapshots[0] is null||!ReferenceEquals(snapshots[0].Runtime.Owner,_runtime)||snapshots[0].CompatibleState is null)
            throw new ArgumentException("Compatible identity requires exactly one current target-compatible snapshot.",nameof(snapshots));
        var admission=snapshots[0].CompatibleState!;admission.CheckEngine(token);return admission;
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
        if(_constructed&&!snapshots.IsEmpty)throw new ArgumentException("Construction requires explicit empty snapshot membership.",nameof(snapshots));
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
    private sealed class SemanticEnvironment(SchemaQueryEnvironment environment,string profile,QueryHostLanguage host=QueryHostLanguage.XPath) : IQuerySemanticEnvironment, IQueryStaticContext
    {
        QueryHostLanguage IQuerySemanticEnvironment.HostLanguage => host;
        string IQuerySemanticEnvironment.HostVersion => host==QueryHostLanguage.Xslt?"3.0":"3.1";
        Uri IQuerySemanticEnvironment.XdmSpecificationRevision => new("https://www.w3.org/TR/2017/REC-xpath-datamodel-31-20170321/");
        string IQuerySemanticEnvironment.BehaviorProfile => profile;
        IQueryStaticContext IQuerySemanticEnvironment.StaticContext => this;
        string IQuerySemanticEnvironment.DescriptorFormatVersion => profile==CapturedHostBinding.PublicTransformProfile?CapturedHostBinding.PublicTransformDescriptor:profile==CapturedHostBinding.PublicQueryProfile?CapturedHostBinding.PublicQueryDescriptor:profile==CapturedHostBinding.Profile?CapturedHostBinding.Descriptor:profile==CompatibleAdmissionPlan.PublicProfile?CompatibleAdmissionPlan.PublicDescriptor:profile==CompatibleAdmissionPlan.IdentityProfile?CompatibleAdmissionPlan.IdentityDescriptor:profile==CompatibleAdmissionPlan.Profile?CompatibleAdmissionPlan.Descriptor:profile==ConstructedAtomicPlan.Profile?ConstructedAtomicPlan.Descriptor:profile==OriginalNodeDataPlan.Profile?OriginalNodeDataPlan.Descriptor:profile==OriginalNodeNilledPlan.Profile?OriginalNodeNilledPlan.Descriptor:profile==TypedNumericPlan.Profile?TypedNumericPlan.Descriptor:profile==TypedIdentityPlan.PublicProfile?TypedIdentityPlan.PublicDescriptor:profile==TypedIdentityPlan.Profile?TypedIdentityPlan.Descriptor:profile==ExactArithmeticPlan.Profile?ExactArithmeticPlan.DescriptorFormat:profile==ExactArithmeticPlan.ScalarProfile?ExactArithmeticPlan.ScalarDescriptorFormat:profile==ExactArithmeticPlan.QuotientProfile?ExactArithmeticPlan.QuotientDescriptorFormat:profile==ExactArithmeticPlan.DivisionProfile?ExactArithmeticPlan.DivisionDescriptorFormat:profile==ExactArithmeticPlan.ConstructorProfile?ExactArithmeticPlan.ConstructorDescriptorFormat:SchemaQueryEnvironment.DescriptorFormat;
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
    internal sealed class TypedAdmission(IQueryTypedContainerIssuer issuer,ISchemaAssessmentInput input,ISchemaAssessmentOrigin? origin)
    {
        internal readonly IQueryTypedContainerIssuer Issuer=issuer;
        internal readonly ISchemaAssessmentInput Input=input;
        internal readonly ISchemaAssessmentOrigin? Origin=origin;
    }
    internal CompatibleAdmissionPlan.Admission? CompatibleState {get;init;}
    internal TypedAdmission? TypedState {get;set;}
    internal QuerySourceSession.Snapshot Runtime { get; }
    internal SchemaAdvisorBinding? OriginalBinding { get; }
    internal SchemaQuerySnapshot(QuerySourceSession.Snapshot runtime, ISchemaSnapshotBinding binding,SchemaAdvisorBinding? originalBinding=null)
    { Runtime = runtime; Binding = binding; OriginalBinding=originalBinding; }
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
    private readonly CapturedHostBinding? _host;
    private readonly SchemaQueryLimits _limits;
    private readonly bool _numeric;
    private readonly bool _publicTyped;
    private readonly SchemaAdvisorBinding? _nodeBinding;
    private readonly ImmutableArray<SchemaQuerySnapshot> _snapshots;
    private readonly bool _data;
    private readonly ConstructedAtomicPlan? _construction;
    private readonly CompatibleAdmissionPlan.PublicOperation? _compatible;
    internal SchemaQueryOperation(QuerySourceSession.Operation runtime,SchemaQueryLimits limits,bool numeric)
        :this(runtime,limits,numeric,false) { }
    internal SchemaQueryOperation(QuerySourceSession.Operation runtime,SchemaQueryLimits limits,bool numeric,bool publicTyped,SchemaAdvisorBinding? nilledBinding=null,ImmutableArray<SchemaQuerySnapshot> snapshots=default,bool data=false,SchemaAdvisorBinding? constructionBinding=null,CompatibleAdmissionPlan.PublicOperation? compatible=null,CapturedHostBinding? host=null)
    { _host=host;_compatible=compatible;_runtime=runtime;_limits=limits;_numeric=numeric;_publicTyped=publicTyped;_nodeBinding=nilledBinding;_snapshots=snapshots;_data=data;_construction=constructionBinding is null?null:new(runtime,constructionBinding,limits); }
    internal QuerySourceSession.Operation RequireHost(CapturedHostBinding host)
    {_runtime.Check();if(!ReferenceEquals(host,_host))throw new ArgumentException("Operation belongs to another captured executable.");host.Session.RequireHostOperation(_runtime);return _runtime;}
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
    /// <exception cref="NotSupportedException">The compatible identity profile does not expose node traversal.</exception>
    public IXdmNode GetRoot(SchemaQuerySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);_runtime.Check();if(_compatible is not null)throw new NotSupportedException("Compatible node traversal is unavailable."); return _runtime.GetRoot(snapshot.Runtime);
    }
    /// <summary>Admits an authority-issued atomic/list/union container before reading its contents.</summary>
    /// <param name="snapshot">The exact source admission in this operation.</param>
    /// <param name="value">The immutable issued provider value.</param>
    /// <returns>A single authenticated atomic value in the typed identity or compatible profile. Compatible identity requires a selected non-list singleton; other profiles and unsupported shapes return Unsupported.</returns>
    public SchemaResult<XdmValue> ReadTypedValue(SchemaQuerySnapshot snapshot, ISchemaTypedValue value)
    {
        ArgumentNullException.ThrowIfNull(snapshot); ArgumentNullException.ThrowIfNull(value);
        _runtime.RequireMember(snapshot.Runtime);
        if(_compatible is not null)return _compatible.Call(op=>op.Read(value));
        if(_nodeBinding is not null)return _data?SchemaQuerySession.UnsupportedData<XdmValue>():SchemaQuerySession.UnsupportedNilled<XdmValue>();
        if(!_publicTyped)return SchemaQuerySession.Unsupported<XdmValue>();
        var typed=snapshot.TypedState??throw new ArgumentException("Snapshot has no bound typed input.",nameof(snapshot));
        var staged=_runtime.PrepareTypedRegistration(snapshot.Runtime,typed.Issuer,typed.Input,typed.Origin,value,_limits.MaxCachedValuesPerSnapshot,_limits.MaxTypedValueAtoms,_limits.MaxNumericDigits,_limits.MaxDecimalScale);
        return staged.TryCommitSingle(out var result)?SchemaQuerySession.Supported(result):_runtime.TypedNumericProfile?SchemaQuerySession.UnsupportedNumeric<XdmValue>():SchemaQuerySession.UnsupportedTyped<XdmValue>();
    }
    /// <summary>Reads a complete typed container while retaining list and selected-member metadata.</summary>
    /// <param name="snapshot">The exact admitted source snapshot in this raw operation.</param>
    /// <param name="value">The immutable provider-issued container; an engine delivery adapter is not new provider issuance.</param>
    /// <returns>A raw-operation borrowed complete adapter for the ordinary or compatible typed-identity profile and eleven public-factory atomic kinds (string, boolean, integer, decimal, double, float, date, time, dateTime, duration and QName); otherwise Unsupported.</returns>
    /// <exception cref="ArgumentException">Membership, issuance, type evidence or payload is invalid.</exception>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="NotSupportedException">Captured atom or numeric capacity is exceeded.</exception>
    /// <exception cref="ObjectDisposedException">The raw operation or required admission is closed.</exception>
    /// <exception cref="OperationCanceledException">Raw work is canceled.</exception>
    /// <remarks>All metadata, evidence and cursor access remains borrowed from this exact operation. Previously copied plain payloads survive expiry. The read preserves supplied payloads and annotations without independent lexical/schema/calendar validation or broader scalar execution. Uri/Binary enum-only representations remain Unsupported. The read creates no scalar registry slots or independent lease. Compatible access uses the exact serialized paired-provider window and originating raw proof.</remarks>
    public SchemaResult<ISchemaTypedValue> ReadTypedContainer(SchemaQuerySnapshot snapshot, ISchemaTypedValue value)
    {
        ArgumentNullException.ThrowIfNull(snapshot);ArgumentNullException.ThrowIfNull(value);
        _runtime.CheckObservationActive();_runtime.RequireMember(snapshot.Runtime);
        if(_compatible is not null)return _compatible.ReadContainer(value);
        if(!_publicTyped||_runtime.TypedNumericProfile||_nodeBinding is not null||_construction is not null)
            return SchemaQuerySession.UnsupportedTyped<ISchemaTypedValue>();
        var typed=snapshot.TypedState??throw new ArgumentException("Snapshot has no bound typed input.",nameof(snapshot));
        var complete=_runtime.ReadTypedContainer(snapshot.Runtime,typed.Issuer,typed.Input,typed.Origin,value,
            _limits.MaxTypedValueAtoms,_limits.MaxNumericDigits,_limits.MaxDecimalScale,validateStored:true);
        return CompleteTypedContainer.Create(complete);
    }

    /// <summary>Observes original atomic type metadata under this compatible operation's leased proof.</summary>
    /// <param name="value">A registered scalar from this exact compatible target snapshot.</param>
    /// <returns>A borrowed guarded provenance view in the selected compatible singleton profile, or Unsupported in other live operation profiles.</returns>
    /// <exception cref="ArgumentException">The selected-profile carrier is missing, foreign or rebound.</exception>
    /// <exception cref="NotSupportedException">Current proof or scalar shape is unsupported.</exception>
    /// <exception cref="ObjectDisposedException">The operation or a required admission is revoked.</exception>
    /// <exception cref="OperationCanceledException">Raw work was canceled.</exception>
    /// <exception cref="InvalidOperationException">Provider work overlaps or reenters the same operation.</exception>
    /// <remarks>Each returned getter validates through this raw operation's serialized source/target provider window. The view expires with the operation and neither acquires an independent lease nor rewrites original type handles into target authority identities. SchemaAtomics remains the context-free descriptive helper and continues refusing compatible carriers.</remarks>
    public SchemaResult<ISchemaAtomicProvenance> ReadAtomicProvenance(XdmValue value)
    {
        _runtime.CheckObservationActive();
        return _compatible is null?SchemaQuerySession.UnsupportedTyped<ISchemaAtomicProvenance>():_compatible.Observe(value);
    }

    internal QueryRegisteredTypedContainer ReadTypedCandidate(SchemaQuerySnapshot snapshot,IQueryTypedContainerIssuer issuer,ISchemaAssessmentInput input,ISchemaAssessmentOrigin? origin,ISchemaTypedValue value)
    {
        ArgumentNullException.ThrowIfNull(snapshot);_runtime.RequireMember(snapshot.Runtime);
        return _runtime.ReadRegisteredTypedContainer(snapshot.Runtime,issuer,input,origin,value,_limits.MaxCachedValuesPerSnapshot,_limits.MaxTypedValueAtoms,_limits.MaxNumericDigits,_limits.MaxDecimalScale);
    }
    internal XdmValue EvaluateTypedIdentityCandidate(SchemaQuerySnapshot snapshot,XdmValue value)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return _runtime.EvaluateTypedIdentity(snapshot.Runtime,value,_limits.MaxTypedValueAtoms,_limits.MaxNumericDigits,_limits.MaxDecimalScale);
    }
    /// <summary>Requests explicit provider-owned simple construction and admits its result.</summary>
    /// <param name="input">Atomic/lexical input under the selected constructor profile.</param>
    /// <param name="targetType">Owned simple target type.</param>
    /// <param name="episode">Captured assessment constants.</param>
    /// <returns>A fresh operation-local atomic, Invalid or Unsupported in the selected construction profile; Unsupported in other profiles.</returns>
    /// <exception cref="ArgumentException">A required argument, owned target, episode or provider evidence is invalid.</exception>
    /// <exception cref="NotSupportedException">The constructed result exceeds the captured atomic or numeric capacity.</exception>
    /// <exception cref="ObjectDisposedException">The raw operation, session or borrowed provider admission is revoked.</exception>
    /// <exception cref="OperationCanceledException">Raw work was canceled.</exception>
    public SchemaResult<XdmValue> ConstructSimpleValue(XdmValue input, ISchemaComponentHandle targetType, ISchemaAssessmentEpisode episode)
    {
        _runtime.Check(); ArgumentNullException.ThrowIfNull(targetType); ArgumentNullException.ThrowIfNull(episode);
        if(_construction is not null)return _construction.Construct(input,targetType,episode);
        if(_data)return SchemaQuerySession.UnsupportedData<XdmValue>();
        return _publicTyped?(_runtime.TypedNumericProfile?SchemaQuerySession.UnsupportedNumeric<XdmValue>():SchemaQuerySession.UnsupportedTyped<XdmValue>()):SchemaQuerySession.Unsupported<XdmValue>();
    }
    /// <summary>Evaluates captured XPath or XQuery; XSLT uses its typed transform producer instead.</summary>
    /// <param name="context">Dynamic inputs whose selected policy must be enforced.</param>
    /// <returns>A scalar selected by the ordinary/numeric profile, the same authenticated value for typed identity, a plain computed result for typed numeric execution, an optional Boolean for original-node nilled, ordered atomic results for original-node data, the same current operation-local constructed value, or a borrowed flat ordinary XQuery result under its selected public host profile.</returns>
    /// <exception cref="NotSupportedException">Context state, numeric or structured inputs, or the result is outside this profile.</exception>
    /// <exception cref="ArgumentException">Inputs do not have exactly one admitted owner or contain invalid typed scalar evidence.</exception>
    /// <exception cref="ObjectDisposedException">The operation or a required admission is revoked.</exception>
    /// <exception cref="OperationCanceledException">Raw work was canceled.</exception>
    public XdmValue Evaluate(EvaluationContext context)
    {
        _runtime.Check(); ArgumentNullException.ThrowIfNull(context);
        if(_host is not null)return _host.EvaluatePublic(this,context);
        if(_compatible is not null)return _compatible.Call(op=>op.Evaluate(context.CaptureCompatibleScalar()));
        if(_construction is not null)return _construction.Evaluate(context);
        if(_nodeBinding is not null)return _data?OriginalNodeDataPlan.Evaluate(_runtime,_nodeBinding,_snapshots,_limits,context):OriginalNodeNilledPlan.Evaluate(_runtime,_nodeBinding,_snapshots,_limits,context);
        if(_publicTyped)
        {
            var value=context.CaptureTypedIdentityScalar();
            return _runtime.TypedNumericProfile?_runtime.EvaluatePublicTypedNumeric(value,_limits.MaxTypedValueAtoms,_limits.MaxNumericDigits,_limits.MaxDecimalScale)
                :_runtime.EvaluatePublicTypedIdentity(value,_limits.MaxTypedValueAtoms,_limits.MaxNumericDigits,_limits.MaxDecimalScale);
        }
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
