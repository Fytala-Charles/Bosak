// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Verifies public provenance refusal, registry authentication and ordered atomic metadata.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Numerics;
using System.Xml.Linq;
using Bosak.XPath.Providers.Xml;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Fixture=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Fixture;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Tests generic refusal and real registry provenance without provider callback reentry.</summary>
public sealed class SchemaAtomicsTests
{
    private sealed class PoisonProvenance:ISchemaAtomicProvenance
    {
        ISchemaComponentHandle ISchemaAtomicProvenance.DeclaredType=>throw new Exception("declared poison");
        ISchemaComponentHandle ISchemaAtomicProvenance.ActualType=>throw new Exception("actual poison");
        ISchemaAssessmentOrigin? ISchemaAtomicProvenance.OriginalAssessment=>throw new Exception("origin poison");
    }
    private sealed class BrokenObservation(int mode):ISchemaAtomicObservation
    {
        bool ISchemaAtomicObservation.TryObserve(in XdmValue value,out ISchemaAtomicProvenance? provenance)
        {provenance=mode==1?null:new PoisonProvenance();if(mode==2)throw new InvalidOperationException("trusted engine defect");return mode!=0;}
    }
    private static QueryTypedScalarRegistration Prepare(Fixture f,QuerySourceSession.Operation op,QueryTypedContainerTests.Container c,int limit=8)
        =>op.PrepareTypedRegistration(f.Snapshot,f.Issuer,f.Input,f.Origin,c,limit,8,128,128);

    /// <summary>PG01 plain values never infer component evidence from labels or exact payloads.</summary>
    /// <param name="kind">Plain carrier shape.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)]
    [InlineData(6)][InlineData(7)][InlineData(8)][InlineData(9)][InlineData(10)][InlineData(11)][InlineData(12)]
    public void PG01_PlainAndNonAtomic(int kind)
    {
        var value=kind switch {0=>XdmValue.Undefined,1=>XdmValue.FromInteger(1),2=>XdmValue.FromInteger(BigInteger.Pow(10,80)),
            3=>XdmValue.FromDecimal(new XdmExactDecimal(123,2,128,128)),4=>XdmValue.FromString("x","string","Q{urn:x}T"),
            5=>XdmValue.FromMap(new XdmMap()),6=>XdmValue.FromArray(new XdmArray()),7=>XdmValue.FromFunction(new object()),
            8=>XdmValue.FromSequence(MaterializedSequence.FromArray(XdmValue.FromInteger(1),XdmValue.FromInteger(2))),
            9=>XdmValue.FromDouble(double.NaN),10=>XdmValue.FromBoolean(true),12=>XdmValue.FromNode(new XDocumentNode(XDocument.Parse("<n/>"))),_=>XdmValue.FromQName(new("urn:x","n","p"))};
        Assert.False(SchemaAtomics.TryGetProvenance(in value,out var evidence));Assert.Null(evidence);
    }

    /// <summary>PG02 unknown/private-legacy/provider provenance markers are not a public attachment SPI.</summary>
    /// <param name="kind">Unqualified marker family.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)]
    public void PG02_UnknownMarkers(int kind)
    {
        object marker=kind switch{0=>new object(),1=>new PoisonProvenance(),_=>new SchemaEvidenceKernel.Evidence(new object(),XdmValue.FromInteger(1),new object(),new object(),null,null)};
        var value=XdmValue.SchemaTransportAttach(XdmValue.FromInteger(1),marker);
        Assert.False(SchemaAtomics.TryGetProvenance(in value,out var evidence));Assert.Null(evidence);
    }

    /// <summary>PG03 false/null trusted results clear output while unexpected engine defects propagate.</summary>
    /// <param name="mode">Trusted internal bridge failure.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)]
    public void PG03_NoBlanketCatch(int mode)
    {
        var value=XdmValue.SchemaTransportAttach(XdmValue.FromInteger(1),new BrokenObservation(mode));
        if(mode==2)Assert.Throws<InvalidOperationException>(()=>SchemaAtomics.TryGetProvenance(in value,out _));
        else{Assert.False(SchemaAtomics.TryGetProvenance(in value,out var evidence));Assert.Null(evidence);}
    }

    /// <summary>PG04 unregistered entries cannot observe a tuple even when their key matches a resident winner.</summary>
    /// <param name="registered">Whether a distinct genuine winner shares the key.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void PG04_UnregisteredEntry(bool registered)
    {
        using var f=new Fixture();var c=f.Value();
        f.Run(op=>
        {
            var tx=Prepare(f,op,c);if(registered)tx.Commit();
            var cache=f.Snapshot.RequireTypedScalars();
            var group=new QueryTypedScalarCache.Group(cache,c,f.Origin,f.Governing,false,[new(XdmValue.FromInteger(1),f.Governing,f.Actual,f.Origin)]);
            var value=group.Entries[0].Transport;
            Assert.False(SchemaAtomics.TryGetProvenance(in value,out var evidence));Assert.Null(evidence);
            Assert.Throws<ArgumentException>(()=>((ISchemaAtomicProvenance)group.Entries[0]).DeclaredType);
            Assert.Equal(registered?1:0,f.Snapshot.TypedScalarCount);return 0;
        });
    }

    /// <summary>PG05 empty/list/union metadata stays ordered and separate from an atomic observation.</summary>
    /// <param name="count">Number of list members.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)]
    public void PG05_CompleteContainerBoundary(int count)
    {
        using var f=new Fixture();var c=f.Value(count,true,false);var alternate=new QueryTypedContainerTests.Handle();f.Authority.Types.Add(alternate);
        if(count==2)c.Items[1]=new(XdmValue.FromString("selected"),new Provenance(c,f.Governing,alternate,f.Origin));
        var values=f.Run(op=>
        {
            var group=Prepare(f,op,c).Commit();Assert.True(group.IsList);Assert.Equal(count,group.Count);
            var result=new XdmValue[count];for(int i=0;i<count;i++)result[i]=group.GetTransport(i);return result;
        });
        c.OnRead=_=>throw new Exception("container replay");
        for(int i=0;i<values.Length;i++)
        {Assert.True(SchemaAtomics.TryGetProvenance(in values[i],out var p));Assert.Same(f.Governing,p!.DeclaredType);Assert.Same(i==1?alternate:f.Actual,p.ActualType);Assert.Same(f.Origin,p.OriginalAssessment);}
        var empty=XdmValue.Undefined;Assert.False(SchemaAtomics.TryGetProvenance(in empty,out _));Assert.Equal(count,f.Snapshot.TypedScalarCount);
    }

    /// <summary>PG06 failed transactional registration leaves no observable staged prefix.</summary>
    [Fact] public void PG06_RolledBackRegistration()
    {
        using var f=new Fixture();var c=f.Value(2,true,false);
        f.Run(op=>
        {
            var tx=Prepare(f,op,c);var field=typeof(QueryTypedScalarRegistration).GetField("_group",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!;
            var group=(QueryTypedScalarCache.Group)field.GetValue(tx)!;
            Assert.Throws<InvalidOperationException>(()=>tx.Commit(1));
            foreach(var entry in group.Entries){var value=entry.Transport;Assert.False(SchemaAtomics.TryGetProvenance(in value,out _));}
            Assert.Equal(0,f.Snapshot.TypedScalarCount);var current=tx.Commit();var valid=current.GetTransport(0);Assert.True(SchemaAtomics.TryGetProvenance(in valid,out _));return 0;
        });
    }

    /// <summary>PG07 observation cannot fit a prefix of an over-quota container or alter its slot limit.</summary>
    [Fact] public void PG07_WholeBatchQuota()
    {
        using var f=new Fixture();var c=f.Value(2,true,false);
        Assert.Throws<NotSupportedException>(()=>f.Run(op=>Prepare(f,op,c,1).Commit()));Assert.Equal(0,f.Snapshot.TypedScalarCount);
        var empty=XdmValue.Undefined;Assert.False(SchemaAtomics.TryGetProvenance(in empty,out _));
    }
}
