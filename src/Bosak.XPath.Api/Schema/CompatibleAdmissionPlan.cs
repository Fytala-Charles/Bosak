// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Authenticates directional compatible admission with fresh operation-owned provider proofs.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; private compatible proof/admission transaction (1.68.0).                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2 | 05-11-2026     | Separate stable payload scope from current compatible issuer (1.69.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3 | 05-11-2026     | Expose raw-local guarded windows for public compatible identity (1.70.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4 | 06-11-2026     | Add raw-leased compatible provenance with guarded per-getter windows (1.78.0). |
//                      | Charles Korthout | 0.5 | 06-11-2026     | Read complete compatible containers within exact paired windows (1.80.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Text.Json;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.XPath.Api.Schema;

/// <summary>Authenticates directional compatible admission with fresh operation-owned provider proofs.</summary>
internal static class CompatibleAdmissionPlan
{
    internal const string Profile="bosak-internal-compatible-admission-v1";
    internal const string Descriptor="bosak-internal-compatible-admission-descriptor-v1";
    internal const string IdentityProfile="bosak-internal-compatible-atomic-identity-v1";
    internal const string PublicProfile="bosak-compatible-atomic-identity-v1";
    internal const string PublicDescriptor="bosak-compatible-atomic-identity-descriptor-v1";
    internal const string IdentityDescriptor="bosak-internal-compatible-atomic-identity-descriptor-v1";
    internal static bool TryCompile(string expression,SchemaQueryLimits limits,CompileOptions options,CancellationToken token,out QuerySourceSession? runtime,bool identity=false,bool publicIdentity=false)
    {
        runtime=null;token.ThrowIfCancellationRequested();
        if(expression.Trim()!="$compatible"||!SchemaSessionCompilation.TrySnapshotOptions(options,out var frozen,out _))return false;
        var captured=SchemaSessionCompilation.Capture(frozen!,frozen!.Namespaces,frozen.DefaultElementNamespace);
        var descriptor=JsonSerializer.Serialize(new{format=publicIdentity?PublicDescriptor:identity?IdentityDescriptor:Descriptor,profile=publicIdentity?PublicProfile:identity?IdentityProfile:Profile,staticContext=captured.Fingerprint,operation=identity?"internal-operation-bound-identity":"internal-admission-only",proof="fresh-exact-target-authority-call",origin="original-source",dataExecution=identity?"internal-singleton-non-list-selected-families":"unavailable",limits.MaxOpenSnapshots,limits.MaxCachedValuesPerSnapshot,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale});
        token.ThrowIfCancellationRequested();
        runtime=new(new SchemaQueryEnvironment(captured.Namespaces,descriptor),static _=>throw new NotSupportedException("Compatible data execution is not activated."),limits.MaxOpenSnapshots);
        return true;
    }
    private sealed class CurrentIssuer(object scope,QuerySourceSession runtime,QuerySourceSession.Snapshot snapshot,
        SchemaQuerySnapshot source,SchemaAdvisorBinding original,SchemaQuerySnapshot.TypedAdmission state,Action check):IQueryOperationBoundTypedIssuer
    {
        object IQueryOperationBoundTypedIssuer.ScopeIdentity=>scope;
        SchemaQueryEnvironment IQueryTypedContainerIssuer.Environment=>runtime.Environment;
        ISchemaAuthority IQueryTypedContainerIssuer.Authority=>original.Authority;
        void IQueryTypedContainerIssuer.ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();check();token.ThrowIfCancellationRequested();}
        bool IQueryTypedContainerIssuer.OwnsInput(ISchemaAssessmentInput input,IQuerySourceInput projection)
        {check();bool answer=ReferenceEquals(input,state.Input)&&ReferenceEquals(projection,snapshot.Input)&&state.Issuer.OwnsInput(input,source.Runtime.Input);check();return answer;}
        bool IQueryTypedContainerIssuer.OwnsOrigin(ISchemaAssessmentOrigin origin,ISchemaAssessmentInput input,IQuerySourceInput projection)
        {check();bool answer=ReferenceEquals(origin,state.Origin)&&ReferenceEquals(input,state.Input)&&ReferenceEquals(projection,snapshot.Input)&&state.Issuer.OwnsOrigin(origin,input,source.Runtime.Input);check();return answer;}
        bool IQueryTypedContainerIssuer.OwnsContainer(ISchemaTypedValue value,ISchemaAssessmentInput input,IQuerySourceInput projection,ISchemaAssessmentOrigin? origin)
        {check();bool answer=ReferenceEquals(origin,state.Origin)&&ReferenceEquals(input,state.Input)&&ReferenceEquals(projection,snapshot.Input)&&state.Issuer.OwnsContainer(value,input,source.Runtime.Input,origin);check();return answer;}
    }
    internal sealed class TypedOperation(QuerySourceSession.Operation operation,QuerySourceSession.Snapshot snapshot,
        IQueryOperationBoundTypedIssuer issuer,SchemaQuerySnapshot.TypedAdmission state,SchemaQueryLimits limits,Action check)
    {
        internal IQueryOperationBoundTypedIssuer Issuer=>issuer;
        internal void Check()=>check();
        internal SchemaResult<XdmValue> Read(ISchemaTypedValue value,int faultAfter=-1)
        {
            check();ArgumentNullException.ThrowIfNull(value);
            var registration=operation.PrepareTypedRegistration(snapshot,issuer,state.Input,state.Origin,value,limits.MaxCachedValuesPerSnapshot,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale);
            var result=registration.TryCommitCompatibleSingle(out var scalar,faultAfter)?SchemaQuerySession.Supported(scalar):SchemaQuerySession.UnsupportedTyped<XdmValue>();check();return result;
        }
        internal SchemaResult<ISchemaTypedValue> ReadContainer(ISchemaTypedValue value,PublicOperation owner)
        {
            check();ArgumentNullException.ThrowIfNull(value);
            var captured=operation.ReadTypedContainer(snapshot,issuer,state.Input,state.Origin,value,
                limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale).Capture();
            void CheckCaptured()
            {
                operation.CheckObservationActive();check();var authority=issuer.Authority;
                if(!issuer.OwnsInput(state.Input,snapshot.Input))throw new ArgumentException("Compatible complete input changed.");check();
                if(state.Origin is null||!issuer.OwnsOrigin(state.Origin,state.Input,snapshot.Input))throw new ArgumentException("Compatible complete origin is unowned.");check();
                if(!issuer.OwnsContainer(value,state.Input,snapshot.Input,state.Origin))throw new ArgumentException("Compatible complete container is unissued.");check();
                void Own(ISchemaComponentHandle type){check();bool owned=authority.Owns(type);check();if(!owned)throw new ArgumentException("Compatible complete type is unowned.");}
                Own(captured.Governing);
                foreach(var cell in captured.Cells)
                {
                    if(!ReferenceEquals(cell.Origin,state.Origin))throw new ArgumentException("Compatible complete original metadata changed.");
                    Own(cell.Declared);Own(cell.Actual);
                }
                check();operation.CheckObservationActive();
            }
            var complete=new QueryTypedContainer(CheckCaptured,captured.Governing,captured.List,captured.Cells);
            var result=CompleteTypedContainer.Create(complete,owner);check();return result;
        }
        internal XdmValue Evaluate(XdmValue value)
        {
            check();var view=operation.ValidateTypedTransport(snapshot,value,issuer,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale);
            if(view.IsList||view.Count!=1||view.GetAtomic(0).Value.Kind is not(XdmValueKind.String or XdmValueKind.Boolean or XdmValueKind.Integer or XdmValueKind.Decimal))
                throw new NotSupportedException("Compatible identity requires an eligible non-list singleton.");
            check();return value;
        }
        internal QueryRegisteredTypedContainer Validate(XdmValue value)
        {check();var result=operation.ValidateTypedTransport(snapshot,value,issuer,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale);check();return result;}
        internal T ReadProvenance<T>(XdmValue value,Func<QueryTypedContainer.Cell,T> read)
        {
            check();var cache=snapshot.RequireTypedScalars();
            var group=cache.RequireCompatibleObservation(operation,value,issuer,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale);
            if(!ReferenceEquals(group.Origin,state.Origin)||group.Origin is null)throw new ArgumentException("Compatible observation origin changed.");
            var authority=issuer.Authority;check();
            if(!issuer.OwnsInput(state.Input,snapshot.Input))throw new ArgumentException("Compatible observation input changed.");check();
            if(!issuer.OwnsOrigin(group.Origin,state.Input,snapshot.Input))throw new ArgumentException("Compatible observation origin is unowned.");check();
            if(!issuer.OwnsContainer(group.Container,state.Input,snapshot.Input,group.Origin))throw new ArgumentException("Compatible observation container is unissued.");check();
            var cell=group.Cells[0];
            void Own(ISchemaComponentHandle type){check();bool owned=authority.Owns(type);check();if(!owned)throw new ArgumentException("Compatible observation type is unowned.");}
            Own(group.Governing);Own(cell.Declared);Own(cell.Actual);
            var result=read(cell);check();cache.Check(group);return result;
        }
        internal void RequireNoGenericFallback(XdmValue value)
        {check();_ = operation.ValidateTypedTransport(snapshot,value,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale);}
    }
    // The raw frame is ephemeral. Only these serialized windows may invoke its proof/issuer callbacks.
    internal sealed class PublicOperation(QuerySourceSession.Operation operation,IQueryOperationLifetime source,
        IQueryOperationLifetime target,TypedOperation typed,Action engineCheck)
    {
        internal T Call<T>(Func<TypedOperation,T> work)=>operation.CallProviderPair(source,target,guard=>
        {guard();typed.Check();var result=work(typed);typed.Check();guard();return result;},guard=>{guard();engineCheck();guard();});
        internal SchemaResult<ISchemaTypedValue> ReadContainer(ISchemaTypedValue value)
            =>Call(typed=>typed.ReadContainer(value,this));
        internal SchemaResult<ISchemaAtomicProvenance> Observe(XdmValue value)
            =>Call(typed=>typed.ReadProvenance(value,_=>SchemaQuerySession.Supported<ISchemaAtomicProvenance>(new ProvenanceView(this,value))));
        private sealed class ProvenanceView(PublicOperation owner,XdmValue value):ISchemaAtomicProvenance
        {
            ISchemaComponentHandle ISchemaAtomicProvenance.DeclaredType=>owner.Call(typed=>typed.ReadProvenance(value,static atom=>atom.Declared));
            ISchemaComponentHandle ISchemaAtomicProvenance.ActualType=>owner.Call(typed=>typed.ReadProvenance(value,static atom=>atom.Actual));
            ISchemaAssessmentOrigin? ISchemaAtomicProvenance.OriginalAssessment=>owner.Call(typed=>typed.ReadProvenance(value,static atom=>atom.Origin));
        }
        internal void Finish()=>Call(typed=>{typed.Check();return 0;});
    }
    internal sealed class Result(SchemaCompatibilityDecision decision,Admission? admission)
    {
        internal SchemaCompatibilityDecision Decision { get; }=decision;
        internal Admission? Value { get; }=admission;
        internal string? DiagnosticCode=>Decision switch {SchemaCompatibilityDecision.Compatible=>null,SchemaCompatibilityDecision.Incompatible=>"BOSAK-COMPATIBILITY-INCOMPATIBLE",_=>"BOSAK-COMPATIBILITY-UNSUPPORTED"};
    }
    internal sealed class Admission(QuerySourceSession owner,IQuerySemanticEnvironment environment,SchemaAdvisorBinding target,
        SchemaQuerySnapshot source,SchemaAdvisorBinding original,SchemaQuerySnapshot.TypedAdmission state,QuerySourceSession.Snapshot snapshot):IDisposable
    {
        private readonly object _cacheScope=new();
        internal QuerySourceSession.Snapshot Snapshot { get; }=snapshot;
        internal void CheckEngine(CancellationToken token)
        {target.CheckEngineSession(token);original.CheckEngineSession(token);Snapshot.Lifetime.Check();source.Runtime.Lifetime.Check();token.ThrowIfCancellationRequested();}
        internal PublicOperation BeginPublic(QuerySourceSession.Operation operation,SchemaQueryLimits limits)
        {
            if(environment.BehaviorProfile!=PublicProfile)throw new NotSupportedException("Public compatible profile required.");
            operation.RequireMember(Snapshot);CheckEngine(operation.Cancellation);
            return WithProof(operation,environment,target,source,original,state,(decision,check)=>
            {
                if(decision!=SchemaCompatibilityDecision.Compatible)throw new NotSupportedException("Current compatible proof is not positive.");
                check();var issuer=new CurrentIssuer(_cacheScope,owner,Snapshot,source,original,state,check);
                var typed=new TypedOperation(operation,Snapshot,issuer,state,limits,check);
                return new PublicOperation(operation,(IQueryOperationLifetime)original.Advisor,(IQueryOperationLifetime)target.Advisor,typed,()=>CheckEngine(operation.Cancellation));
            });
        }
        internal T RunTyped<T>(QuerySourceSession runtime,SchemaAdvisorBinding binding,SchemaQueryLimits limits,Func<TypedOperation,T> work,CancellationToken token)
        {
            if(environment.BehaviorProfile!=IdentityProfile)throw new NotSupportedException("This admission does not select internal compatible identity.");
            if(!ReferenceEquals(runtime,owner)||!ReferenceEquals(binding,target))throw new ArgumentException("Compatible admission belongs to another consuming session.");
            target.CheckEngineSession(token);original.CheckEngineSession(token);Snapshot.CheckSource(token);
            return owner.Run(owner.Environment,[Snapshot],operation=>WithProof(operation,environment,target,source,original,state,(decision,check)=>
            {
                if(decision!=SchemaCompatibilityDecision.Compatible)throw new NotSupportedException("Current compatible identity proof is not positive.");
                check();var issuer=new CurrentIssuer(_cacheScope,owner,Snapshot,source,original,state,check);
                var typed=new TypedOperation(operation,Snapshot,issuer,state,limits,check);var result=work(typed);check();return result;
            }),token);
        }
        internal T Run<T>(QuerySourceSession runtime,SchemaAdvisorBinding binding,Func<Action,T> work,CancellationToken token)
        {
            if(!ReferenceEquals(runtime,owner)||!ReferenceEquals(binding,target))throw new ArgumentException("Compatible admission belongs to another consuming session.");
            target.CheckEngineSession(token);original.CheckEngineSession(token);Snapshot.CheckSource(token);
            return owner.Run(owner.Environment,[Snapshot],operation=>WithProof(operation,environment,target,source,original,state,(decision,check)=>
            {
                if(decision!=SchemaCompatibilityDecision.Compatible)throw new NotSupportedException(decision==SchemaCompatibilityDecision.Incompatible?"Current proof is Incompatible.":"Current compatibility is Unsupported.");
                check();var result=work(check);check();return result;
            }),token);
        }
        /// <summary>Revokes only this neutral target admission; original host ownership stays with the source.</summary>
        public void Dispose()=>Snapshot.Dispose();
    }
    internal static Result Open(QuerySourceSession runtime,IQuerySemanticEnvironment environment,SchemaAdvisorBinding target,
        SchemaQuerySnapshot source,CancellationToken token,int publicationFault)
    {
        target.RequireRuntime(runtime);target.CheckEngineSession(token);source.Runtime.Lifetime.Check();
        var state=source.TypedState;var original=source.OriginalBinding;
        if(ReferenceEquals(source.Runtime.Owner,runtime)||source.Runtime.RetainedSource is not null||state?.Origin is null||original is null)
            return new(SchemaCompatibilityDecision.Unsupported,null);
        original.RequireRuntime(source.Runtime.Owner);original.CheckEngineSession(token);
        if(original.Advisor is not IQueryOperationLifetime||target.Advisor is not IQueryOperationLifetime||target.Advisor is not ISchemaCompatibilityAuthority)
            return new(SchemaCompatibilityDecision.Unsupported,null);
        QuerySourceSession.Snapshot? retained=runtime.OpenRetainedSourceSnapshot(runtime.Environment,source.Runtime,token);
        try
        {
            var decision=runtime.Run(runtime.Environment,[retained],operation=>WithProof(operation,environment,target,source,original,state,(answer,check)=>{check();return answer;}),token);
            target.CheckEngineSession(token);original.CheckEngineSession(token);retained.CheckSource(token);target.CheckEngineSession(token);original.CheckEngineSession(token);
            if(decision!=SchemaCompatibilityDecision.Compatible)
            {retained.Dispose();retained=null;return new(decision,null);}
            if(publicationFault==0)throw new InvalidOperationException("Injected unpublished compatible admission failure.");
            var admission=new Admission(runtime,environment,target,source,original,state,retained);
            var result=new Result(decision,admission);
            target.CheckEngineSession(token);original.CheckEngineSession(token);retained.CheckSource(token);target.CheckEngineSession(token);original.CheckEngineSession(token);
            retained=null;return result;
        }
        catch(Exception primary)
        {if(retained is not null)SchemaEvidenceKernel.PreserveFailure(primary,retained.Dispose);throw;}
    }
    private static T WithProof<T>(QuerySourceSession.Operation operation,IQuerySemanticEnvironment environment,SchemaAdvisorBinding target,
        SchemaQuerySnapshot source,SchemaAdvisorBinding original,SchemaQuerySnapshot.TypedAdmission state,Func<SchemaCompatibilityDecision,Action,T> work)
    {
        var token=operation.Cancellation;
        var origin=state.Origin??throw new ArgumentException("Missing original source assessment.");
        return operation.CallProviderPair((IQueryOperationLifetime)original.Advisor,(IQueryOperationLifetime)target.Advisor,guard=>
        {
            void Bindings(){guard();original.CheckAvailable(token,guard);target.CheckAvailable(token,guard);guard();}
            void Check()
            {
                guard();
                if(!ReferenceEquals(source.TypedState,state)||!ReferenceEquals(source.OriginalBinding,original))throw new ArgumentException("Original source admission metadata changed.");
                Bindings();
                bool input=state.Issuer.OwnsInput(state.Input,source.Runtime.Input);Bindings();
                if(!input)throw new ArgumentException("Original input is not the exact issued source binding.");
                bool owned=state.Issuer.OwnsOrigin(origin,state.Input,source.Runtime.Input);Bindings();
                if(!owned)throw new ArgumentException("Original assessment is not owned by the captured source advisor.");
            }
            TRead Read<TRead>(Func<TRead> getter){Check();var result=getter();Check();return result;}
            Check();
            var proof=Read(()=>((ISchemaCompatibilityAuthority)target.Advisor).Establish(original.Advisor,target.Advisor,environment,token))
                ??throw new ArgumentException("Compatibility authority returned no result.");
            var decision=Read(()=>proof.Decision);
            if(decision is not(SchemaCompatibilityDecision.Compatible or SchemaCompatibilityDecision.Incompatible or SchemaCompatibilityDecision.Unsupported))
                throw new ArgumentException("Unknown compatibility decision.");
            ISchemaCompatibilityView? view=null;
            void Current()
            {
                Check();
                if(view is null)return;
                var endpoint=Read(()=>view.Source);if(!ReferenceEquals(endpoint,original.Advisor))throw new ArgumentException("Proof source endpoint changed.");
                endpoint=Read(()=>view.Target);if(!ReferenceEquals(endpoint,target.Advisor))throw new ArgumentException("Proof target endpoint changed.");
                var captured=Read(()=>view.ExecutionEnvironment);if(!ReferenceEquals(captured,environment))throw new ArgumentException("Proof execution environment changed.");
                Check();
            }
            if(decision==SchemaCompatibilityDecision.Compatible)
            {view=Read(()=>proof.View)??throw new ArgumentException("Compatible proof has no view.");Current();}
            Current();var result=work(decision,Current);Current();return result;
        },guard=>{guard();original.CheckEngineSession(token);target.CheckEngineSession(token);guard();});
    }
}
