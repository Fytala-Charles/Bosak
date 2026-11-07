// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Admits uncached typed containers under exact raw-operation issuer ownership.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; internal typed container reader (1.53.0)                                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 04-11-2026     | Stage immutable cells and revalidate registered data without getter replay (1.54.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 05-11-2026     | Authorize numeric projection through owned actual type relations (1.59.0).                 |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4 | 05-11-2026     | Separate stable payload scope from current compatible issuer (1.69.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Caller-selected internal adapter attesting immutable container issuance and exact input binding.</summary>
internal interface IQueryTypedContainerIssuer
{
    SchemaQueryEnvironment Environment { get; }
    ISchemaAuthority Authority { get; }
    void ThrowIfUnavailable(CancellationToken cancellation);
    bool OwnsInput(ISchemaAssessmentInput originalInput,IQuerySourceInput input);
    bool OwnsOrigin(ISchemaAssessmentOrigin origin,ISchemaAssessmentInput originalInput,IQuerySourceInput input);
    bool OwnsContainer(ISchemaTypedValue value,ISchemaAssessmentInput originalInput,IQuerySourceInput input,ISchemaAssessmentOrigin? origin);
}

/// <summary>Engine-only current-operation issuer with a callback-free stable payload scope token.</summary>
internal interface IQueryOperationBoundTypedIssuer : IQueryTypedContainerIssuer
{
    object ScopeIdentity { get; }
}

/// <summary>Uncached typed metadata whose later access remains bound to the original raw operation.</summary>
internal sealed class QueryTypedContainer
{
    internal readonly record struct Cell(XdmValue Value,ISchemaComponentHandle Declared,ISchemaComponentHandle Actual,ISchemaAssessmentOrigin? Origin);
    private readonly Action _check;
    private readonly ISchemaComponentHandle _governing;
    private readonly bool _isList;
    private readonly ImmutableArray<Cell> _cells;
    internal QueryTypedContainer(Action check,ISchemaComponentHandle governing,bool isList,ImmutableArray<Cell> cells)
    {_check=check;_governing=governing;_isList=isList;_cells=cells;}
    internal ISchemaComponentHandle GoverningType {get{_check();return _governing;}}
    internal bool IsList {get{_check();return _isList;}}
    internal int Count {get{_check();return _cells.Length;}}
    internal AtomicView GetAtomic(int index){_check();ArgumentOutOfRangeException.ThrowIfNegative(index);if(index>=_cells.Length)throw new ArgumentOutOfRangeException(nameof(index));return new(this,index);}
    internal (ISchemaComponentHandle Governing,bool List,ImmutableArray<Cell> Cells) Capture()
    {_check();return (_governing,_isList,_cells);}
    internal readonly struct AtomicView(QueryTypedContainer owner,int index)
    {
        private Cell Read(){if(owner is null)throw new InvalidOperationException("No admitted atomic view.");owner._check();return owner._cells[index];}
        internal XdmValue Value=>Read().Value;
        internal ISchemaComponentHandle DeclaredType=>Read().Declared;
        internal ISchemaComponentHandle ActualType=>Read().Actual;
        internal ISchemaAssessmentOrigin? OriginalAssessment=>Read().Origin;
    }
}

/// <summary>Validates an entire container before publishing a guarded result, without persistent registration.</summary>
internal sealed class QueryTypedContainerReader
{
    private readonly QuerySourceSession.Operation _operation;
    private readonly QuerySourceSession.Snapshot _snapshot;
    private readonly SchemaQueryEnvironment _environment;
    private readonly IQueryTypedContainerIssuer _issuer;
    private readonly ISchemaAssessmentInput _originalInput;
    private readonly ISchemaAssessmentOrigin? _origin;
    private readonly ISchemaTypedValue _value;
    private readonly CancellationToken _cancellation;
    private readonly ISchemaAuthority _authority;
    private readonly int _maxAtoms,_digits,_scale;
    internal QueryTypedContainerReader(QuerySourceSession.Operation operation,QuerySourceSession.Snapshot snapshot,
        SchemaQueryEnvironment environment,IQueryTypedContainerIssuer issuer,ISchemaAssessmentInput originalInput,
        ISchemaAssessmentOrigin? origin,ISchemaTypedValue value,int maxAtoms,int digits,int scale,CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(issuer);ArgumentNullException.ThrowIfNull(originalInput);ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxAtoms);ArgumentOutOfRangeException.ThrowIfNegativeOrZero(digits);ArgumentOutOfRangeException.ThrowIfNegative(scale);
        _operation=operation;_snapshot=snapshot;_environment=environment;_issuer=issuer;_originalInput=originalInput;_origin=origin;_value=value;
        _maxAtoms=maxAtoms;_digits=digits;_scale=scale;_cancellation=cancellation;
        Check();_authority=issuer.Authority??throw new ArgumentException("A selected authority is required.",nameof(issuer));Check();
    }
    private void Check()
    {
        _operation.RequireMember(_snapshot);_cancellation.ThrowIfCancellationRequested();_issuer.ThrowIfUnavailable(_cancellation);
        if(!ReferenceEquals(_issuer.Environment,_environment)||!_issuer.OwnsInput(_originalInput,_snapshot.Input))throw new ArgumentException("Foreign issuer environment or input binding.");
        _operation.Check();_issuer.ThrowIfUnavailable(_cancellation);_originalInput.ThrowIfUnavailable(_cancellation);
        _operation.Check();_issuer.ThrowIfUnavailable(_cancellation);
        if(_origin is not null&&!_issuer.OwnsOrigin(_origin,_originalInput,_snapshot.Input))throw new ArgumentException("Foreign original assessment.");
        if(_authority is not null&&!ReferenceEquals(_issuer.Authority,_authority))throw new ArgumentException("Selected authority changed.");
        _operation.Check();_issuer.ThrowIfUnavailable(_cancellation);_cancellation.ThrowIfCancellationRequested();
    }
    private void CheckContainer()
    {Check();if(!_issuer.OwnsContainer(_value,_originalInput,_snapshot.Input,_origin))throw new ArgumentException("Unissued typed container.");Check();}
    private TResult Read<TSource,TResult>(TSource source,Func<TSource,TResult> getter)
    {CheckContainer();var result=getter(source);CheckContainer();return result;}
    private void OwnType(ISchemaComponentHandle? type)
    {CheckContainer();if(type is null||!_authority.Owns(type))throw new ArgumentException("Unissued type component.");CheckContainer();}
    private QueryTypedContainer.Cell Validate(SchemaAtomicItem item)
    {
        CheckContainer();var evidence=item.TypeEvidence??throw new ArgumentException("Default atomic cell has no evidence.");
        var declared=Read(evidence,static e=>e.DeclaredType);OwnType(declared);
        var actual=Read(evidence,static e=>e.ActualType);OwnType(actual);
        var origin=Read(evidence,static e=>e.OriginalAssessment);
        if(!ReferenceEquals(origin,_origin))throw new ArgumentException("Atomic evidence has a different original assessment.");
        var value=item.Value;
        ValidatePayload(value);
        CheckContainer();return new(value,declared,actual,origin);
    }
    internal ISchemaAuthority SelectedAuthority {get{CheckContainer();return _authority;}}
    private T Supported<T>(SchemaResult<T>? outcome)
    {
        CheckContainer();
        if(outcome is null)throw new ArgumentException("Authority returned no outcome.");
        if(Read(outcome,static r=>r.Kind)!=SchemaResultKind.Supported)throw new NotSupportedException("Authority cannot authorize numeric projection.");
        return Read(outcome,static r=>r.Value);
    }
    private ISchemaComponentHandle ResolveNumericType(string name)
    {
        var type=Supported(Read(_authority,a=>a.ResolveType(new("http://www.w3.org/2001/XMLSchema",name),_cancellation)));
        OwnType(type);return type;
    }
    private bool NumericRelation(ISchemaComponentHandle actual,ISchemaComponentHandle expected,bool derived)
    {
        OwnType(actual);OwnType(expected);
        bool answer=Supported(Read(_authority,a=>derived?a.IsDerivedFrom(actual,expected,_cancellation):a.IsSameComponent(actual,expected,_cancellation)));
        OwnType(actual);OwnType(expected);return answer;
    }
    internal XdmValue ProjectNumeric(QueryTypedContainer.Cell cell)
    {
        CheckContainer();OwnType(cell.Actual);ValidatePayload(cell.Value);
        var integer=ResolveNumericType("integer");
        ISchemaComponentHandle selected=integer;
        bool isInteger=NumericRelation(cell.Actual,integer,false)||NumericRelation(cell.Actual,integer,true);
        XdmValue result;
        if(isInteger)
        {
            if(cell.Value.Kind!=XdmValueKind.Integer)throw new ArgumentException("Integer type requires an Integer payload.");
            result=XdmValue.FromInteger(cell.Value.BigIntegerValue);
        }
        else
        {
            var decimalType=ResolveNumericType("decimal");
            selected=decimalType;
            if(!(NumericRelation(cell.Actual,decimalType,false)||NumericRelation(cell.Actual,decimalType,true)))throw new NotSupportedException("Actual type is not an authorized numeric family.");
            if(cell.Value.Kind!=XdmValueKind.Decimal)throw new ArgumentException("Decimal type requires a Decimal payload.");
            result=XdmValue.FromDecimal(cell.Value.ExactDecimalValue);
        }
        CheckContainer();OwnType(cell.Actual);OwnType(selected);return result;
    }
    internal void ValidateStored(ISchemaComponentHandle governing,ImmutableArray<QueryTypedContainer.Cell> cells)
    {
        CheckContainer();OwnType(governing);
        if(cells.Length>_maxAtoms)throw new NotSupportedException("Typed result exceeds atomic capacity.");
        foreach(var cell in cells)
        {
            OwnType(cell.Declared);OwnType(cell.Actual);
            if(!ReferenceEquals(cell.Origin,_origin))throw new ArgumentException("Cached original assessment differs.");
            ValidatePayload(cell.Value);CheckContainer();
        }
        CheckContainer();
    }
    private void ValidatePayload(XdmValue value)
    {
        if(XdmValue.SchemaTransportMarker(value) is not null||value.Kind is < XdmValueKind.String or > XdmValueKind.Binary)throw new ArgumentException("Plain atomic payload required.");
        if(value.Kind==XdmValueKind.Integer)_=new XdmExactDecimal(value.BigIntegerValue,0,_digits,_scale);
        else if(value.Kind==XdmValueKind.Decimal){var(c,s)=value.DecimalRepresentation;_=new XdmExactDecimal(c,s,_digits,_scale);}
    }
    internal QueryTypedContainer Read()
    {
        CheckContainer();var governing=Read(_value,static v=>v.GoverningType);OwnType(governing);
        bool isList=Read(_value,static v=>v.IsList);
        CheckContainer();bool single=_value.TryGetSingleAtomic(out var item);CheckContainer();
        ImmutableArray<QueryTypedContainer.Cell> cells;
        if(single)cells=ImmutableArray.Create(Validate(item));
        else
        {
            var enumerable=Read(_value,static v=>v.AtomicItems)??throw new ArgumentException("Null atomic enumerable.");
            IEnumerator<SchemaAtomicItem>? enumerator=null;Exception? primary=null;
            try
            {
                CheckContainer();enumerator=enumerable.GetEnumerator()??throw new ArgumentException("Null atomic enumerator.");CheckContainer();
                var builder=ImmutableArray.CreateBuilder<QueryTypedContainer.Cell>(Math.Min(4,_maxAtoms));
                while(Read(enumerator,static e=>e.MoveNext()))
                {
                    if(builder.Count==_maxAtoms)throw new NotSupportedException("Typed result exceeds atomic capacity.");
                    builder.Add(Validate(Read(enumerator,static e=>e.Current)));
                }
                cells=builder.ToImmutable();
            }
            catch(Exception error){primary=error;throw;}
            finally
            {
                if(enumerator is not null){if(primary is null)enumerator.Dispose();else SchemaEvidenceKernel.PreserveFailure(primary,enumerator.Dispose);}
            }
        }
        CheckContainer();return new QueryTypedContainer(CheckContainer,governing,isList,cells);
    }
}
