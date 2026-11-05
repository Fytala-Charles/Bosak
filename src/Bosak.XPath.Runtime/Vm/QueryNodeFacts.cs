// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Stages guarded original node facts without conflating absent and empty typed values.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; internal original-node facts reader (1.61.0).                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Trusted original-facts issuance capability, separate from neutral node membership.</summary>
internal interface IQueryNodeFactsIssuer : IQueryTypedContainerIssuer
{
    bool OwnsFacts(ISchemaNodeFacts facts);
    bool OwnsNode(IXdmNode node, ISchemaNodeFacts facts, ISchemaAssessmentInput input, ISchemaAssessmentOrigin origin);
}

/// <summary>Distinguishes an absent typed value from an unsupported read and a supported empty value.</summary>
internal enum QueryNodeTypedValueKind { Absent, Unsupported, Supported }

/// <summary>Uncached original facts whose every observation requires the live raw operation.</summary>
internal sealed class QueryNodeFacts(Action check, ISchemaComponentHandle type, ISchemaComponentHandle? declaration,
    ISchemaAssessmentOrigin origin, bool absent, bool nilled, XdmNodeKind nodeKind, QueryNodeTypedValueKind kind, QueryTypedContainer? value)
{
    internal ISchemaComponentHandle Type { get { check(); return type; } }
    internal ISchemaComponentHandle? Declaration { get { check(); return declaration; } }
    internal ISchemaAssessmentOrigin Origin { get { check(); return origin; } }
    internal bool HasNoTypedValue { get { check(); return absent; } }
    internal bool IsNilled { get { check(); return nilled; } }
    internal XdmNodeKind NodeKind { get { check(); return nodeKind; } }
    internal QueryNodeTypedValueKind TypedValueKind { get { check(); return kind; } }
    internal QueryTypedContainer TypedValue { get { check(); return value ?? throw new NotSupportedException("Original typed value is absent or unsupported."); } }
}

/// <summary>Stages authenticated original metadata and a complete bounded typed container without caching.</summary>
internal sealed class QueryNodeFactsReader
{
    private readonly QuerySourceSession.Operation _operation;
    private readonly QuerySourceSession.Snapshot _snapshot;
    private readonly SchemaQueryEnvironment _environment;
    private readonly IQueryNodeFactsIssuer _issuer;
    private readonly ISchemaAssessmentInput _input;
    private readonly ISchemaAssessmentOrigin _origin;
    private readonly IXdmNode _node;
    private readonly ISchemaNodeFacts _facts;
    private readonly CancellationToken _token;
    private readonly ISchemaAuthority _authority;
    private readonly int _atoms, _digits, _scale;
    private ISchemaComponentHandle? _type, _declaration;

    internal QueryNodeFactsReader(QuerySourceSession.Operation operation, QuerySourceSession.Snapshot snapshot,
        SchemaQueryEnvironment environment, IQueryNodeFactsIssuer issuer, ISchemaAssessmentInput input,
        ISchemaAssessmentOrigin origin, IXdmNode node, ISchemaNodeFacts facts, int atoms, int digits, int scale, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(issuer); ArgumentNullException.ThrowIfNull(input); ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(node); ArgumentNullException.ThrowIfNull(facts);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(atoms); ArgumentOutOfRangeException.ThrowIfNegativeOrZero(digits); ArgumentOutOfRangeException.ThrowIfNegative(scale);
        _operation=operation; _snapshot=snapshot; _environment=environment; _issuer=issuer; _input=input; _origin=origin;
        _node=node; _facts=facts; _token=token; _atoms=atoms; _digits=digits; _scale=scale;
        CheckBinding(); _authority=issuer.Authority ?? throw new ArgumentException("Selected authority is missing."); CheckBinding();
    }
    private void CheckLive()
    {
        _operation.RequireMember(_snapshot); _token.ThrowIfCancellationRequested(); _issuer.ThrowIfUnavailable(_token);
        _operation.Check(); _input.ThrowIfUnavailable(_token); _operation.Check(); _issuer.ThrowIfUnavailable(_token); _token.ThrowIfCancellationRequested();
    }
    private T Call<T>(Func<T> callback)
    { CheckLive(); var result=callback(); CheckLive(); return result; }
    private void CheckBinding()
    {
        CheckLive();
        if(!ReferenceEquals(Call(()=>_issuer.Environment),_environment))throw new ArgumentException("Foreign facts environment.");
        if(!Call(()=>_issuer.OwnsInput(_input,_snapshot.Input)))throw new ArgumentException("Foreign facts input binding.");
        if(!Call(()=>_issuer.OwnsOrigin(_origin,_input,_snapshot.Input)))throw new ArgumentException("Foreign original assessment.");
        if(_authority is not null&&!ReferenceEquals(Call(()=>_issuer.Authority),_authority))throw new ArgumentException("Selected authority changed.");
    }
    private void CheckFacts()
    {
        CheckBinding(); _operation.RequireNodeOwner(_snapshot,_node); CheckBinding();
        if(!Call(()=>_issuer.OwnsFacts(_facts)))throw new ArgumentException("Unissued original facts.");
        CheckBinding();
        if(!Call(()=>_issuer.OwnsNode(_node,_facts,_input,_origin)))throw new ArgumentException("Foreign node/facts/input/origin tuple.");
        CheckBinding();
    }
    private T Read<T>(Func<T> getter)
    { CheckFacts(); var result=getter(); CheckFacts(); return result; }
    private void OwnType(ISchemaComponentHandle? type)
    {
        CheckFacts();
        if(type is null||!Call(()=>_authority.Owns(type)))throw new ArgumentException("Unowned original component.");
        CheckFacts();
    }
    private void CheckStored()
    { CheckFacts(); OwnType(_type); if(_declaration is not null)OwnType(_declaration); CheckFacts(); }

    internal QueryNodeFacts Read()
    {
        CheckFacts();
        var nodeKind=Read(()=>_node.NodeKind);
        _type=Read(()=>_facts.Type); OwnType(_type);
        _declaration=Read(()=>_facts.Declaration); if(_declaration is not null)OwnType(_declaration);
        var origin=Read(()=>_facts.Origin);
        if(!ReferenceEquals(origin,_origin))throw new ArgumentException("Original facts origin differs.");
        bool absent=Read(()=>_facts.HasNoTypedValue), nilled=Read(()=>_facts.IsNilled);
        if(nilled&&(absent||nodeKind!=XdmNodeKind.Element))throw new ArgumentException("Inconsistent nilled facts.");
        QueryTypedContainer? value=null; var kind=QueryNodeTypedValueKind.Absent;
        if(!absent)
        {
            var outcome=Read(()=>_facts.ReadTypedValue(_token)) ?? throw new ArgumentException("Missing facts typed-value outcome.");
            var resultKind=Read(()=>outcome.Kind);
            if(resultKind==SchemaResultKind.Unsupported)kind=QueryNodeTypedValueKind.Unsupported;
            else
            {
                if(resultKind!=SchemaResultKind.Supported)throw new ArgumentException("Malformed original typed-value outcome.");
                var container=Read(()=>outcome.Value) ?? throw new ArgumentException("Supported original typed value is missing.");
                var reader=new QueryTypedContainerReader(_operation,_snapshot,_environment,new GuardedIssuer(this),_input,_origin,container,_atoms,_digits,_scale,_token);
                var captured=reader.Read().Capture(); CheckStored();
                if(!ReferenceEquals(captured.Governing,_type))throw new ArgumentException("Original governing type differs from typed value.");
                if(nilled&&captured.Cells.Length!=0)throw new ArgumentException("Nilled original node has a nonempty typed value.");
                void Guard(){CheckStored();reader.ValidateStored(captured.Governing,captured.Cells);CheckStored();}
                value=new QueryTypedContainer(Guard,captured.Governing,captured.List,captured.Cells); kind=QueryNodeTypedValueKind.Supported;
            }
        }
        CheckStored(); return new QueryNodeFacts(CheckStored,_type!,_declaration,_origin,absent,nilled,nodeKind,kind,value);
    }
    private sealed class GuardedIssuer(QueryNodeFactsReader owner) : IQueryTypedContainerIssuer
    {
        SchemaQueryEnvironment IQueryTypedContainerIssuer.Environment { get { owner.CheckStored(); return owner._environment; } }
        ISchemaAuthority IQueryTypedContainerIssuer.Authority { get { owner.CheckStored(); return owner._authority; } }
        void IQueryTypedContainerIssuer.ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();owner.CheckStored();}
        bool IQueryTypedContainerIssuer.OwnsInput(ISchemaAssessmentInput input,IQuerySourceInput current)
        {owner.CheckStored();var result=owner._issuer.OwnsInput(input,current);owner.CheckStored();return result;}
        bool IQueryTypedContainerIssuer.OwnsOrigin(ISchemaAssessmentOrigin origin,ISchemaAssessmentInput input,IQuerySourceInput current)
        {owner.CheckStored();var result=owner._issuer.OwnsOrigin(origin,input,current);owner.CheckStored();return result;}
        bool IQueryTypedContainerIssuer.OwnsContainer(ISchemaTypedValue value,ISchemaAssessmentInput input,IQuerySourceInput current,ISchemaAssessmentOrigin? origin)
        {owner.CheckStored();var result=owner._issuer.OwnsContainer(value,input,current,origin);owner.CheckStored();return result;}
    }
}
