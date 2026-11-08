// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Constructs and consumes operation-local singleton values without source admissions.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; source-free operation-local construction (1.66.0).                             |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2 | 06-11-2026     | Observe constructed cached types only inside their exact raw lifetime (1.77.0). |
// ===========================================================================================================================================================
using System.Text.Json;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.XPath.Api.Schema;

/// <summary>Constructs and consumes operation-local singleton values without source admissions.</summary>
internal sealed class ConstructedAtomicPlan(QuerySourceSession.Operation operation,SchemaAdvisorBinding binding,SchemaQueryLimits limits)
{
    internal const string Profile="bosak-constructed-atomic-v1";
    internal const string Descriptor="bosak-constructed-atomic-descriptor-v1";
    private Episode? _episode;
    private int _publicationFault=-1; // Caught diagnostic seam; never induces a real allocation failure.
    private sealed record Episode(DateTimeOffset CurrentDateTime,int ImplicitTimezoneOffsetMinutes):ISchemaAssessmentEpisode;
    private sealed record Receipt(ConstructedAtomicPlan Owner,ISchemaTypedValue Witness,ISchemaComponentHandle Target,
        ISchemaComponentHandle Governing,ISchemaComponentHandle Declared,ISchemaComponentHandle Actual,XdmValue Plain)
        :ISchemaAtomicObservation,ISchemaAtomicProvenance
    {
        bool ISchemaAtomicObservation.TryObserve(in XdmValue value,out ISchemaAtomicProvenance? provenance)
        {
            provenance=null;
            try
            {
                Owner.CheckObservation();if(!value.SchemaTransportMatches(Plain))return false;
                Owner.CheckObservation();provenance=this;return true;
            }
            catch(ObjectDisposedException){return false;}
            catch(OperationCanceledException){return false;}
        }
        ISchemaComponentHandle ISchemaAtomicProvenance.DeclaredType{get{Owner.CheckObservation();return Declared;}}
        ISchemaComponentHandle ISchemaAtomicProvenance.ActualType{get{Owner.CheckObservation();return Actual;}}
        ISchemaAssessmentOrigin? ISchemaAtomicProvenance.OriginalAssessment{get{Owner.CheckObservation();return null;}}
    }
    private void CheckObservation(){operation.CheckObservationActive();binding.CheckEngineSession(operation.Cancellation);operation.CheckObservationActive();}
    private sealed class UnavailableCapability:Exception;
    internal static bool TryCompile(string expression,SchemaQueryLimits limits,CompileOptions options,CancellationToken token,out QuerySourceSession? runtime)
    {
        runtime=null;token.ThrowIfCancellationRequested();
        if(expression.Trim()!="$constructed"||!SchemaSessionCompilation.TrySnapshotOptions(options,out var frozen,out _))return false;
        var captured=SchemaSessionCompilation.Capture(frozen!,frozen!.Namespaces,frozen.DefaultElementNamespace);
        var descriptor=JsonSerializer.Serialize(new{format=Descriptor,profile=Profile,staticContext=captured.Fingerprint,variable="constructed",input="unmarked-lexical-string",output="non-list-singleton-string-boolean-integer-decimal",origin="fresh-null",lifetime="exact-advisor-operation-admission",receipt="same-raw-operation",cache="none",limits.MaxOpenSnapshots,limits.MaxCachedValuesPerSnapshot,limits.MaxTypedValueAtoms,limits.MaxNumericDigits,limits.MaxDecimalScale});
        token.ThrowIfCancellationRequested();
        runtime=new(new SchemaQueryEnvironment(captured.Namespaces,descriptor),static _=>throw new NotSupportedException("Constructed values require explicit receipt dispatch."),limits.MaxOpenSnapshots);
        return true;
    }
    private T Call<T>(Func<Action,T> work)=>operation.CallProvider((IQueryOperationLifetime)binding.Advisor,work,
        guard=>binding.CheckAvailable(operation.Cancellation,guard));
    private void Check(Action guard){guard();binding.CheckAvailable(operation.Cancellation,guard);guard();}
    private T Read<T>(Func<T> getter,Action guard)
    {Check(guard);var value=getter();Check(guard);return value;}
    private void Own(ISchemaComponentHandle? handle,Action guard)
    {
        if(handle is null)throw new ArgumentException("Missing constructed type component.");
        bool owns=Read(()=>binding.Authority.Owns(handle),guard);
        if(!owns)throw new ArgumentException("Unowned constructed type component.");
    }
    private void Same(ISchemaComponentHandle actual,ISchemaComponentHandle expected,Action guard)
    {
        Own(actual,guard);Own(expected,guard);
        var result=Read(()=>binding.Authority.IsSameComponent(actual,expected,operation.Cancellation),guard)
            ??throw new ArgumentException("Missing component identity outcome.");
        var kind=Read(()=>result.Kind,guard);
        if(kind==SchemaResultKind.Unsupported)throw new UnavailableCapability();
        if(kind!=SchemaResultKind.Supported)throw new ArgumentException("Malformed component identity outcome.");
        bool same=Read(()=>result.Value,guard);
        if(!same)throw new ArgumentException("Constructed value does not have the requested target type.");
        Own(actual,guard);Own(expected,guard);
    }
    private Episode Freeze(ISchemaAssessmentEpisode caller,Action guard)
    {
        var time=Read(()=>caller.CurrentDateTime,guard);
        int zone=Read(()=>caller.ImplicitTimezoneOffsetMinutes,guard);
        if(zone is < -840 or >840)throw new ArgumentException("Assessment timezone is outside the supported range.",nameof(caller));
        if(_episode is null)_episode=new(time,zone);
        else if(_episode.CurrentDateTime.Ticks!=time.Ticks||_episode.CurrentDateTime.Offset!=time.Offset||_episode.ImplicitTimezoneOffsetMinutes!=zone)
            throw new ArgumentException("Assessment constants changed within the raw operation.",nameof(caller));
        return _episode;
    }
    private bool ValidatePayload(XdmValue value)
    {
        if(XdmValue.SchemaTransportMarker(value) is not null||value.UserSchemaTypeName is not null
            ||value.Kind is < XdmValueKind.String or > XdmValueKind.Binary)throw new ArgumentException("Constructed cell must be a plain atomic value.");
        string? family=value.Kind switch {XdmValueKind.String=>"string",XdmValueKind.Boolean=>"boolean",XdmValueKind.Integer=>"integer",XdmValueKind.Decimal=>"decimal",_=>null};
        if(family is null)return false;
        if(value.Kind==XdmValueKind.String&&value.StringValue is null)throw new ArgumentException("Constructed String payload is null.");
        if(value.SchemaTypeName is not null&&value.SchemaTypeName!=family)throw new ArgumentException("Constructed cell is not a plain primitive carrier.");
        if(value.Kind==XdmValueKind.Integer)_=new XdmExactDecimal(value.BigIntegerValue,0,limits.MaxNumericDigits,limits.MaxDecimalScale);
        if(value.Kind==XdmValueKind.Decimal){var(c,s)=value.DecimalRepresentation;_=new XdmExactDecimal(c,s,limits.MaxNumericDigits,limits.MaxDecimalScale);}
        return true;
    }
    private (XdmValue Value,ISchemaComponentHandle Declared,ISchemaComponentHandle Actual,bool Eligible) Validate(SchemaAtomicItem item,ISchemaComponentHandle target,Action guard)
    {
        var evidence=item.TypeEvidence??throw new ArgumentException("Default constructed cell has no evidence.");
        var declared=Read(()=>evidence.DeclaredType,guard);Own(declared,guard);Same(declared,target,guard);
        var actual=Read(()=>evidence.ActualType,guard);Own(actual,guard);
        if(Read(()=>evidence.OriginalAssessment,guard) is not null)throw new ArgumentException("Constructed evidence cannot retain an original assessment.");
        bool eligible=ValidatePayload(item.Value);Check(guard);return(item.Value,declared,actual,eligible);
    }
    internal SchemaResult<XdmValue> Construct(XdmValue input,ISchemaComponentHandle target,ISchemaAssessmentEpisode caller)
    {
        operation.Check();ArgumentNullException.ThrowIfNull(target);ArgumentNullException.ThrowIfNull(caller);
        if(input.Kind!=XdmValueKind.String||input.UserSchemaTypeName is not null||XdmValue.SchemaTransportMarker(input) is not null)
            return SchemaQuerySession.UnsupportedConstructed<XdmValue>();
        if(input.StringValue is null)throw new ArgumentException("Lexical String payload is null.",nameof(input));
        return Call(guard=>
        {
            try
            {
                Own(target,guard);var episode=Freeze(caller,guard);
                var outcome=Read(()=>binding.Advisor.ConstructSimpleValue(XdmValue.FromString(input.StringValue),target,episode,operation.Cancellation),guard)
                    ??throw new ArgumentException("Missing constructor outcome.");
                var kind=Read(()=>outcome.Kind,guard);
                if(kind==SchemaResultKind.Invalid)return SchemaQuerySession.InvalidConstruction();
                if(kind==SchemaResultKind.Unsupported)return SchemaQuerySession.UnsupportedConstructed<XdmValue>();
                if(kind!=SchemaResultKind.Supported)throw new ArgumentException("Malformed constructor outcome.");
                var witness=Read(()=>outcome.Value,guard)??throw new ArgumentException("Supported constructor has no value.");
                var governing=Read(()=>witness.GoverningType,guard);Own(governing,guard);Same(governing,target,guard);
                bool list=Read(()=>witness.IsList,guard);
                Check(guard);bool fast=witness.TryGetSingleAtomic(out var item);Check(guard);
                int count=0;bool eligible=true;
                (XdmValue Value,ISchemaComponentHandle Declared,ISchemaComponentHandle Actual,bool Eligible) first=default;
                void Add(SchemaAtomicItem cell)
                {var current=Validate(cell,target,guard);if(count++==0)first=current;eligible&=current.Eligible;}
                if(fast)Add(item);
                else
                {
                    var sequence=Read(()=>witness.AtomicItems,guard)??throw new ArgumentException("Null constructor atomic sequence.");
                    IEnumerator<SchemaAtomicItem>? iterator=null;Exception? primary=null;
                    try
                    {
                        Check(guard);iterator=sequence.GetEnumerator()??throw new ArgumentException("Null constructor iterator.");Check(guard);
                        while(Read(()=>iterator.MoveNext(),guard))
                        {
                            if(count==limits.MaxTypedValueAtoms)throw new NotSupportedException("Constructed result exceeds atomic capacity.");
                            Add(Read(()=>iterator.Current,guard));
                        }
                    }
                    catch(Exception error){primary=error;throw;}
                    finally
                    {if(iterator is not null){if(primary is null)iterator.Dispose();else SchemaEvidenceKernel.PreserveFailure(primary,iterator.Dispose);}}
                }
                Check(guard);Own(target,guard);Own(governing,guard);
                if(list||count!=1||!eligible)return SchemaQuerySession.UnsupportedConstructed<XdmValue>();
                var declared=first.Declared??throw new ArgumentException("Missing constructed declaration.");
                var actual=first.Actual??throw new ArgumentException("Missing constructed actual type.");
                Same(governing,target,guard);Same(declared,target,guard);Own(actual,guard);
                if(_publicationFault==0)throw new InvalidOperationException("Injected unpublished construction failure.");
                var receipt=new Receipt(this,witness,target,governing,declared,actual,first.Value);
                var result=XdmValue.SchemaTransportAttach(first.Value,receipt);
                Check(guard);return SchemaQuerySession.Supported(result);
            }
            catch(UnavailableCapability){Check(guard);return SchemaQuerySession.UnsupportedConstructed<XdmValue>();}
        });
    }
    internal XdmValue Evaluate(EvaluationContext context)
    {
        operation.Check();var value=context.CaptureConstructedScalar();operation.Check();
        if(XdmValue.SchemaTransportMarker(value) is not Receipt receipt||!ReferenceEquals(receipt.Owner,this))
            throw new ArgumentException("Value was not constructed by this raw operation.",nameof(context));
        if(!value.SchemaTransportMatches(receipt.Plain))throw new ArgumentException("Constructed receipt representation changed.",nameof(context));
        return Call(guard=>
        {
            try
            {
                Check(guard);Own(receipt.Target,guard);Same(receipt.Governing,receipt.Target,guard);Same(receipt.Declared,receipt.Target,guard);Own(receipt.Actual,guard);
                ValidatePayload(receipt.Plain);Check(guard);return value;
            }
            catch(UnavailableCapability){Check(guard);throw new NotSupportedException("Constructed component identity is unavailable.");}
        });
    }
}
