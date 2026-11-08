// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Verifies composite host admission before evaluation and node access.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 01-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Xml.Linq;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Runtime.Vm;
using Xunit;
namespace Bosak.XPath.Runtime.Tests;
/// <summary>Exercises exact-operation host role admission and failure ordering.</summary>
public sealed class SchemaCompositeAdmissionTests
{
    private sealed class Issuer : ISchemaEvidenceIssuer
    {
        internal readonly object Origin=new();
        bool ISchemaEvidenceIssuer.OwnsAssessment(object value)=>ReferenceEquals(value,Origin);
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value)=>false;
        bool ISchemaEvidenceIssuer.OwnsType(object value)=>false;
        bool ISchemaEvidenceIssuer.IsSameType(object a,object b)=>ReferenceEquals(a,b);
    }
    private sealed class Fixture : IDisposable
    {
        internal readonly Issuer Provider=new();
        internal readonly SchemaQuerySession Session;
        internal readonly SchemaEvidenceKernel.Snapshot A,B;
        internal readonly IXdmNode NA=new XDocumentNode(XDocument.Parse("<n>A</n>")),NB=new XDocumentNode(XDocument.Parse("<n>B</n>"));
        internal int Evaluations,Releases;
        internal Fixture(Action<SchemaQuerySession.Operation,SchemaOperationInputs>? admission)
        {
            Session=new(new SchemaQueryEnvironment(ImmutableDictionary<string,string>.Empty,"admission"),_=>{Evaluations++;return XdmValue.FromInteger(42);},Provider,4,16,null,true,false,admission);
            A=Session.OpenSnapshot(Session.Environment,new object(),Provider.Origin,()=>Releases++);B=Session.OpenSnapshot(Session.Environment,new object(),Provider.Origin,()=>Releases++);
        }
        internal bool Owns(SchemaEvidenceKernel.Snapshot snapshot,IXdmNode node)=>ReferenceEquals(snapshot,A)?ReferenceEquals(node,NA):ReferenceEquals(snapshot,B)&&ReferenceEquals(node,NB);
        /// <summary>Closes the fixture's remaining admissions.</summary>
        public void Dispose()=>Session.Dispose();
    }
    /// <summary>Checks each role using its unique member before dispatch.</summary>
    [Fact] public void DistinctRolesAdmit()
    {Fixture? f=null;using var owned=f=new((op,input)=>{op.RequireNodeInput(input,f!.NA);op.RequireNodeInput(input,f.NB);});f.Session.Run(f.Session.Environment,[f.A,f.B],op=>{var input=op.CaptureInputs(new EvaluationContext(),f.Owns,16,16);op.EvaluateAtomic(input,16).ForEachAtomic(v=>Assert.Equal(42,v.IntegerValue));return 0;});Assert.Equal(1,f.Evaluations);}
    /// <summary>Rejects a live but omitted role before the evaluator runs.</summary>
    [Fact] public void MissingRoleRefusesBeforeEvaluation()
    {Fixture? f=null;using var owned=f=new((op,input)=>op.RequireNodeInput(input,f!.NB));Assert.Throws<ArgumentException>(()=>f.Session.Run(f.Session.Environment,[f.A],op=>op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(),f.Owns,16,16),16)));Assert.Equal(0,f.Evaluations);}
    /// <summary>Rejects ambiguity across two admitted sources before dispatch.</summary>
    [Fact] public void AmbiguousRoleRefuses()
    {Fixture? f=null;using var owned=f=new((op,input)=>op.RequireNodeInput(input,f!.NA));Assert.Throws<ArgumentException>(()=>f.Session.Run(f.Session.Environment,[f.A,f.B],op=>op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(),(_,_)=>true,16,16),16)));Assert.Equal(0,f.Evaluations);}
    /// <summary>Rechecks the predicate at dispatch instead of trusting earlier capture.</summary>
    [Fact] public void OwnershipChangesBeforeDispatch()
    {bool live=true;Fixture? f=null;using var owned=f=new((op,input)=>op.RequireNodeInput(input,f!.NA));Assert.Throws<ArgumentException>(()=>f.Session.Run(f.Session.Environment,[f.A],op=>{var input=op.CaptureInputs(new EvaluationContext(),(s,n)=>live&&f.Owns(s,n),16,16);live=false;return op.EvaluateAtomic(input,16);}));Assert.Equal(0,f.Evaluations);}
    /// <summary>Foreign operation tokens refuse before any host admission callback.</summary>
    [Fact] public void ForeignTokenPrecedesAdmissionCallback()
    {int calls=0;using var f=new Fixture((_,_)=>calls++);var captured=f.Session.Run(f.Session.Environment,[f.A],op=>op.CaptureInputs(new EvaluationContext(),(_,_)=>false,16,16));f.Session.Run(f.Session.Environment,[f.A],op=>{Assert.Throws<ArgumentException>(()=>op.EvaluateAtomic(captured,16));Assert.Throws<ArgumentException>(()=>op.RequireNodeInput(captured,null));return 0;});Assert.Equal(0,calls);Assert.Equal(0,f.Evaluations);}
    /// <summary>Captured role checks cannot be used after raw completion.</summary>
    [Fact] public void RoleAccessEndsWithOperation()
    {using var f=new Fixture((_,_)=>{});SchemaQuerySession.Operation? escaped=null;var input=f.Session.Run(f.Session.Environment,[f.A],op=>{escaped=op;return op.CaptureInputs(new EvaluationContext(),f.Owns,16,16);});Assert.Throws<ObjectDisposedException>(()=>escaped!.RequireNodeInput(input,f.NA));}
    /// <summary>Closure within an admission callback is caught before evaluator invocation.</summary>
    [Fact] public void ClosureDuringAdmissionBlocksDispatch()
    {Fixture? f=null;using var owned=f=new((_,_)=>f!.A.Dispose());Assert.Throws<ObjectDisposedException>(()=>f.Session.Run(f.Session.Environment,[f.A],op=>op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(),f.Owns,16,16),16)));Assert.Equal(0,f.Evaluations);Assert.Equal(1,f.Releases);}
    /// <summary>Missing admission capability still refuses rather than bypassing checks.</summary>
    [Fact] public void NoAdmissionHookStillRefuses()
    {using var f=new Fixture(null);Assert.Throws<NotSupportedException>(()=>f.Session.Run(f.Session.Environment,ImmutableArray<SchemaEvidenceKernel.Snapshot>.Empty,op=>op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(),(_,_)=>false,16,16),16)));Assert.Equal(0,f.Evaluations);}
    /// <summary>Null roles permit source-free work without owner callbacks.</summary>
    [Fact] public void EmptyMembershipWithNullRoles()
    {int ownerCalls=0;using var f=new Fixture((op,input)=>{op.RequireNodeInput(input,null);op.RequireNodeInput(input,null);});f.Session.Run(f.Session.Environment,ImmutableArray<SchemaEvidenceKernel.Snapshot>.Empty,op=>{op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(),(_,_)=>{ownerCalls++;return false;},16,16),16).ForEachAtomic(_=>{});return 0;});Assert.Equal(0,ownerCalls);Assert.Equal(1,f.Evaluations);}
    /// <summary>One admission can supply both roles without extra snapshot leases.</summary>
    [Fact] public void SharedSourceRoles()
    {Fixture? f=null;using var owned=f=new((op,input)=>{op.RequireNodeInput(input,f!.NA);op.RequireNodeInput(input,f.NA);});f.Session.Run(f.Session.Environment,[f.A],op=>{op.EvaluateAtomic(op.CaptureInputs(new EvaluationContext(),f.Owns,16,16),16).ForEachAtomic(_=>{});f.A.Dispose();Assert.Equal(0,f.Releases);return 0;});Assert.Equal(1,f.Releases);}
}
