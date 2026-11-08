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
//                      | Charles Korthout | 0.2   | 05-11-2026     | Share adjacent metadata guards and avoid authority delegate allocation (1.61.1).         |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 05-11-2026     | Add explicit metadata-only NotRequested observation mode (1.62.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 05-11-2026     | Share adjacent live proofs and call issuer guards directly (1.63.1). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.5   | 05-11-2026     | Stage original typed values into resident scalar output (1.64.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.6   | 07-11-2026     | Adopt qualified callback-free captured issuer identity guards (1.81.1).                   |
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

/// <summary>Distinguishes unrequested typed values from absent, unsupported and supported observations.</summary>
internal enum QueryNodeTypedValueKind { Absent, Unsupported, Supported, NotRequested }

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
    internal QueryTypedContainer TypedValue { get { check(); return value ?? throw new NotSupportedException("Original typed value was not requested, is absent or is unsupported."); } }
}

/// <summary>Stages authenticated original metadata with an optional complete typed container without caching.</summary>
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
    private void CheckBinding()
    {
        // Each post-proof also establishes the next callback's live precondition.
        // No provider callback or cached observation occurs between these boundaries.
        CheckLive();
        var environment=_issuer.Environment;CheckLive();
        if(!ReferenceEquals(environment,_environment))throw new ArgumentException("Foreign facts environment.");
        bool inputOwned=_issuer.OwnsInput(_input,_snapshot.Input);CheckLive();
        if(!inputOwned)throw new ArgumentException("Foreign facts input binding.");
        bool originOwned=_issuer.OwnsOrigin(_origin,_input,_snapshot.Input);CheckLive();
        if(!originOwned)throw new ArgumentException("Foreign original assessment.");
        if(_authority is not null)
        {
            var authority=_issuer.Authority;CheckLive();
            if(!ReferenceEquals(authority,_authority))throw new ArgumentException("Selected authority changed.");
        }
    }
    private void CheckFacts()
    {
        CheckBinding(); _operation.RequireNodeOwner(_snapshot,_node); CheckBinding();
        bool factsOwned=_issuer.OwnsFacts(_facts);CheckLive();
        if(!factsOwned)throw new ArgumentException("Unissued original facts.");
        CheckBinding();
        bool tupleOwned=_issuer.OwnsNode(_node,_facts,_input,_origin);CheckLive();
        if(!tupleOwned)throw new ArgumentException("Foreign node/facts/input/origin tuple.");
        CheckBinding();
    }
    private T Read<T>(Func<T> getter)
    { CheckFacts(); var result=getter(); CheckFacts(); return result; }
    private void OwnType(ISchemaComponentHandle? type)
    {
        CheckFacts();
        CheckOwnedType(type); CheckFacts();
    }
    private void CheckOwnedType(ISchemaComponentHandle? type)
    {
        CheckLive();
        if(type is null)throw new ArgumentException("Unowned original component.");
        bool owned=_authority.Owns(type); CheckLive();
        if(!owned)throw new ArgumentException("Unowned original component.");
    }
    private void CheckStored()
    {
        // The successful post-proof is also the next authority call's precondition.
        CheckFacts(); CheckOwnedType(_type); CheckFacts();
        if(_declaration is not null){CheckOwnedType(_declaration);CheckFacts();}
    }

    internal QueryNodeFacts Read(bool metadataOnly = false)
    {
        CheckFacts();
        var nodeKind=Read(()=>_node.NodeKind);
        _type=Read(()=>_facts.Type); OwnType(_type);
        _declaration=Read(()=>_facts.Declaration); if(_declaration is not null)OwnType(_declaration);
        var origin=Read(()=>_facts.Origin);
        if(!ReferenceEquals(origin,_origin))throw new ArgumentException("Original facts origin differs.");
        bool absent=Read(()=>_facts.HasNoTypedValue), nilled=Read(()=>_facts.IsNilled);
        if(nilled&&(absent||nodeKind!=XdmNodeKind.Element))throw new ArgumentException("Inconsistent nilled facts.");
        QueryTypedContainer? value=null; var kind=metadataOnly ? QueryNodeTypedValueKind.NotRequested : QueryNodeTypedValueKind.Absent;
        if(!metadataOnly&&!absent)
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
    internal XdmValue Atomize(XdmNodeKind expectedKind,int maxCached,int faultAfter=-1)
    {
        var metadata=Read(true);
        var kind=metadata.NodeKind;
        if(kind!=expectedKind||kind is not(XdmNodeKind.Element or XdmNodeKind.Attribute))throw new ArgumentException("Original node kind changed.");
        if(metadata.HasNoTypedValue)
        {
            CheckStored();
            if(kind==XdmNodeKind.Attribute)throw new ArgumentException("An attribute cannot have an absent typed value.");
            throw new InvalidOperationException("FOTY0012: Cannot atomize a node that has no typed value.");
        }
        bool nilled=metadata.IsNilled;
        CheckStored();var outcome=_facts.ReadTypedValue(_token);CheckStored();
        if(outcome is null)throw new ArgumentException("Missing facts typed-value outcome.");
        CheckStored();var resultKind=outcome.Kind;CheckStored();
        if(resultKind==SchemaResultKind.Unsupported)throw new NotSupportedException("Original typed value is unsupported.");
        if(resultKind!=SchemaResultKind.Supported)throw new ArgumentException("Malformed original typed-value outcome.");
        CheckStored();var container=outcome.Value;CheckStored();
        if(container is null)throw new ArgumentException("Supported original typed value is missing.");
        var cache=_snapshot.GetTypedScalars(_issuer,_input,_authority,maxCached);
        var reader=new QueryTypedContainerReader(_operation,_snapshot,_environment,new GuardedIssuer(this),_input,_origin,container,_atoms,_digits,_scale,_token);
        var group=cache.Find(container,_origin);
        if(group is null)
        {
            var captured=reader.Read().Capture();CheckStored();
            group=new QueryTypedScalarCache.Group(cache,container,_origin,captured.Governing,captured.List,captured.Cells);
        }
        reader.ValidateStored(group.Governing,group.Cells);CheckStored();
        if(!ReferenceEquals(group.Governing,_type))throw new ArgumentException("Original governing type differs from typed value.");
        if(nilled&&group.Cells.Length!=0)throw new ArgumentException("Nilled original node has a nonempty typed value.");
        if(group.Cells.Length==0){CheckStored();return XdmValue.Undefined;}
        if(faultAfter==-2)throw new InvalidOperationException("Injected atomization output allocation fault.");
        var sequence=group.Cells.Length>1?new QueryAtomizedSequence(group.Cells.Length):null;
        var result=sequence is null?default:XdmValue.FromSequence(XdmSequence.FromSource(sequence));
        var registration=new QueryTypedScalarRegistration(cache,_operation,_snapshot,reader,group);
        CheckStored();var installed=registration.Commit(faultAfter);CheckStored();
        if(installed.Count!=group.Cells.Length)throw new ArgumentException("Issued container shape changed.");
        if(sequence is null)result=installed.GetTransport(0);
        else for(int index=0;index<group.Cells.Length;index++)sequence.Set(index,installed.GetTransport(index));
        installed.Check();CheckStored();return result;
    }
    private void CheckCapturedIdentity()
    {
        // Captured engine identities are callback-free, not fresh provider ownership proofs.
        _operation.CheckObservationActive();
        _operation.CheckRegistrationActive(_snapshot);
        _token.ThrowIfCancellationRequested();
    }
    private sealed class GuardedIssuer(QueryNodeFactsReader owner) : IQueryTypedContainerIssuer
    {
        SchemaQueryEnvironment IQueryTypedContainerIssuer.Environment { get { owner.CheckCapturedIdentity(); return owner._environment; } }
        ISchemaAuthority IQueryTypedContainerIssuer.Authority { get { owner.CheckCapturedIdentity(); return owner._authority; } }
        void IQueryTypedContainerIssuer.ThrowIfUnavailable(CancellationToken token){token.ThrowIfCancellationRequested();owner.CheckStored();}
        bool IQueryTypedContainerIssuer.OwnsInput(ISchemaAssessmentInput input,IQuerySourceInput current)
        {owner.CheckStored();var result=owner._issuer.OwnsInput(input,current);owner.CheckStored();return result;}
        bool IQueryTypedContainerIssuer.OwnsOrigin(ISchemaAssessmentOrigin origin,ISchemaAssessmentInput input,IQuerySourceInput current)
        {owner.CheckStored();var result=owner._issuer.OwnsOrigin(origin,input,current);owner.CheckStored();return result;}
        bool IQueryTypedContainerIssuer.OwnsContainer(ISchemaTypedValue value,ISchemaAssessmentInput input,IQuerySourceInput current,ISchemaAssessmentOrigin? origin)
        {owner.CheckStored();var result=owner._issuer.OwnsContainer(value,input,current,origin);owner.CheckStored();return result;}
    }
}
