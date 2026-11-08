// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Binds private captured host producers to exact public sessions and serialized raw windows.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2 | 05-11-2026     | Select public ordinary XQuery capture and guarded deferred continuations (1.72.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3 | 06-11-2026     | Retain exact host outcomes and one staged diagnostic through final refusal (1.75.0). |
//                      | Charles Korthout | 0.4 | 06-11-2026     | Select public XSLT profile and actual copied context bounds (1.76.0). |
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Text.Json;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.XPath.Api.Schema;

/// <summary>Engine-only host/executable capability; caller descriptors cannot create public authority.</summary>
internal sealed class CapturedHostBinding
{
    internal const string Profile="bosak-internal-captured-host-v1";
    internal const string Descriptor="bosak-internal-captured-host-descriptor-v1";
    private readonly object _executable;
    internal const string PublicQueryProfile="bosak-xquery-ordinary-v1";
    internal const string PublicQueryDescriptor="bosak-xquery-ordinary-descriptor-v1";
    private readonly int? _bindings,_namespaces;
    private readonly Func<EvaluationContext,XdmValue>? _publicQuery;
    private readonly bool _publicTransform;
    internal const string PublicTransformProfile="bosak-xslt-ordinary-v1";
    internal const string PublicTransformDescriptor="bosak-xslt-ordinary-descriptor-v1";
    internal bool IsPublicQuery=>_publicQuery is not null;
    internal string ProfileName=>IsPublicQuery?PublicQueryProfile:_publicTransform?PublicTransformProfile:Profile;
    internal string DescriptorName=>IsPublicQuery?PublicQueryDescriptor:_publicTransform?PublicTransformDescriptor:Descriptor;
    internal QueryHostLanguage Host {get;}
    internal SchemaQueryEnvironment Environment {get;}
    internal SchemaQuerySession Session {get;}
    internal CapturedHostBinding(object executable,QueryHostLanguage host,SchemaQueryEnvironment captured,SchemaQueryLimits limits,int bindings,int namespaces)
        :this(executable,host,captured,limits,(int?)bindings,namespaces,null) { }
    internal static CapturedHostBinding CreatePublicQuery(object executable,SchemaQueryEnvironment captured,SchemaQueryLimits limits,Func<EvaluationContext,XdmValue> evaluate)
    {
        ArgumentNullException.ThrowIfNull(evaluate);
        if(!ReferenceEquals(evaluate.Target,executable))throw new ArgumentException("Public query evaluator must belong to the exact executable.",nameof(evaluate));
        return new(executable,QueryHostLanguage.XQuery,captured,limits,null,null,evaluate);
    }
    internal static CapturedHostBinding CreatePublicTransform(object executable,SchemaQueryEnvironment captured,SchemaQueryLimits limits)
        =>new(executable,QueryHostLanguage.Xslt,captured,limits,null,null,null,true);
    private CapturedHostBinding(object executable,QueryHostLanguage host,SchemaQueryEnvironment captured,SchemaQueryLimits limits,int? bindings,int? namespaces,Func<EvaluationContext,XdmValue>? evaluate,bool publicTransform=false)
    {
        ArgumentNullException.ThrowIfNull(executable);ArgumentNullException.ThrowIfNull(captured);limits.Validate();
        if(host is not(QueryHostLanguage.XQuery or QueryHostLanguage.Xslt))throw new ArgumentException("An upper host is required.",nameof(host));
        if(evaluate is null&&!publicTransform){ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bindings!.Value);ArgumentOutOfRangeException.ThrowIfNegativeOrZero(namespaces!.Value);}
        _publicTransform=publicTransform;_publicQuery=evaluate;_executable=executable;Host=host;_bindings=bindings;_namespaces=namespaces;
        Environment=new(captured.Namespaces,JsonSerializer.Serialize(new{format=DescriptorName,profile=ProfileName,host=host.ToString(),captured=captured.Fingerprint,bindings,namespaces,contextBounds=IsPublicQuery||_publicTransform?"actual-cardinalities-rechecked":"explicit-internal-ceilings",resultProfile=IsPublicQuery?"flat-atomic-node-v1":_publicTransform?"ordinary-transform-capture-v1":"private-producer",limits.MaxOpenSnapshots,limits.MaxCachedValuesPerSnapshot,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale}));
        Session=SchemaQuerySession.CreateHostCandidate(this,limits);
    }
    // The trusted capture producer stages detached diagnostics, never a new authority capability.
    internal QuerySourceSession.RunCompletion<T> RunOutcome<T>(object executable,
        ImmutableArray<SchemaQuerySnapshot> snapshots, Func<SchemaQueryOperation,Action<T>,T> producer,
        CancellationToken token = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(producer);
        if (!ReferenceEquals(executable,_executable))
            throw new ArgumentException("Foreign captured executable.",nameof(executable));
        var stageGate = new object();
        T? staged = null;
        bool accepting = false;
        var result = Session.RunHostOutcome(this,snapshots,operation =>
        {
            lock (stageGate) accepting = true;
            try
            {
                var value = producer(operation,candidate =>
                {
                    lock (stageGate)
                    {
                        ObjectDisposedException.ThrowIf(!accepting,this);
                        ArgumentNullException.ThrowIfNull(candidate);
                        if (staged is not null) throw new InvalidOperationException("Capture diagnostics have already been staged.");
                        staged = candidate;
                    }
                });
                lock (stageGate)
                {
                    accepting = false;
                    if (staged is not null && !ReferenceEquals(staged,value))
                        throw new InvalidOperationException("Returned capture differs from its staged diagnostics.");
                }
                return value;
            }
            finally { lock (stageGate) accepting = false; }
        },token);
        lock (stageGate)
            return !result.HasResult && result.AdmissionError is null && staged is not null
                ? new(true,staged,null,result.WorkError,result.CleanupErrors) : result;
    }

    internal T Call<T>(SchemaQueryOperation operation,object executable,EvaluationContext? context,SchemaQuerySnapshot? match,
        SchemaQuerySnapshot? global,Func<Frame,T> producer)
    {
        ArgumentNullException.ThrowIfNull(operation);ArgumentNullException.ThrowIfNull(producer);
        var raw=operation.RequireHost(this);
        if(!ReferenceEquals(executable,_executable))throw new ArgumentException("Foreign captured executable.",nameof(executable));
        return raw.CallHost(this,guard=>
        {
            guard();if(match is not null)raw.RequireMember(match.Runtime);if(global is not null)raw.RequireMember(global.Runtime);
            var caller=context??new EvaluationContext();
            var bindingLimit=_bindings??Math.Max(1,caller.SnapshotVariables().Count);
            var namespaceLimit=_namespaces??Math.Max(1,caller.SnapshotNamespaces().Count);
            var inputs=raw.CaptureInputs(caller,bindingLimit,namespaceLimit,_publicTransform);
            var matchRoot=match is null?null:raw.GetRoot(match.Runtime);var globalRoot=global is null?null:raw.GetRoot(global.Runtime);
            if(matchRoot is not null)raw.RequireNodeInput(inputs,matchRoot);if(globalRoot is not null)raw.RequireNodeInput(inputs,globalRoot);
            inputs.ValidateNodeInputs(raw);guard();
            var child=inputs.CreateContext();child.ResourcePolicy=SchemaProviderSessionBinding.ClosedResources();
            var frame=new Frame(child,matchRoot,globalRoot,guard);
            try{frame.Check();var result=producer(frame);frame.Check();return result;}
            finally{frame.Close();}
        });
    }
    internal XdmValue EvaluatePublic(SchemaQueryOperation operation,EvaluationContext context)
    {
        if(_publicQuery is null)throw new NotSupportedException("This private host has no public evaluation route.");
        return Call(operation,_executable,context,null,null,frame=>
        {var value=_publicQuery(frame.Context);frame.Check();return CapturedQueryResult.Bind(this,operation,value);});
    }
    internal T ContinuePublic<T>(SchemaQueryOperation operation,Func<T> work)=>ContinuePublic(operation,_=>work());
    internal T ContinuePublic<T>(SchemaQueryOperation operation,Func<Action,T> work)
    {
        if(_publicQuery is null)throw new NotSupportedException("A public query continuation is required.");
        var raw=operation.RequireHost(this);
        return raw.CallHost(this,guard=>{guard();var result=work(guard);guard();return result;});
    }
    internal sealed class Frame(EvaluationContext context,IXdmNode? match,IXdmNode? global,Action guard)
    {
        private int _closed;
        internal void Check(){ObjectDisposedException.ThrowIf(Volatile.Read(ref _closed)!=0,this);guard();ObjectDisposedException.ThrowIf(Volatile.Read(ref _closed)!=0,this);}
        internal EvaluationContext Context {get{Check();return context;}}
        internal IXdmNode? Match {get{Check();return match;}}
        internal IXdmNode? Global {get{Check();return global;}}
        internal void Close()=>Interlocked.Exchange(ref _closed,1);
    }
}
