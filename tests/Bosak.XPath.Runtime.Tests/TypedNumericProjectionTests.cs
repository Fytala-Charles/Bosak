// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies authority-owned typed numeric projection without public arithmetic activation.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; authorized numeric projection controls (1.59.0)                                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 05-11-2026     | Retain scope controls and reuse numeric authority fixture (1.60.0).                        |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Numerics;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
using Xunit.Abstractions;
using Fixture=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Fixture;
using Handle=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Handle;
using Provenance=Bosak.XPath.Runtime.Tests.QueryTypedContainerTests.Provenance;
namespace Bosak.XPath.Runtime.Tests;
/// <summary>Qualifies exact current ownership and positive type relations before primitive projection.</summary>
/// <param name="output">Records local authorized projection costs.</param>
public sealed class TypedNumericProjectionTests(ITestOutputHelper output)
{
    private sealed class Answer<T>(SchemaResultKind kind,T value,NumericAuthority owner):SchemaResult<T>
    {
        /// <inheritdoc/>
        public override SchemaResultKind Kind{get{owner.Visit("kind");return kind;}}
        /// <inheritdoc/>
        public override T Value{get{owner.Visit("value");if(kind!=SchemaResultKind.Supported){owner.UnsupportedValueReads++;throw new Exception("unsupported value poison");}return value;}}
        /// <inheritdoc/>
        public override string? DiagnosticCode=>throw new Exception("diagnostic poison");
    }
    internal sealed class NumericAuthority(Handle actual,Handle governing):ISchemaAuthority
    {
        internal readonly Handle Integer=new(),Decimal=new();internal readonly HashSet<ISchemaComponentHandle> Owned=new(ReferenceEqualityComparer.Instance){actual,governing};
        internal string Family="integer",ForceStage="";internal SchemaResultKind ForceKind=SchemaResultKind.Supported;internal bool Same,NullOutcome,NullBuiltin,ForeignBuiltin,Record=true;internal int UnsupportedValueReads;internal Action<string>? Hook;internal Action<ISchemaComponentHandle>? OwnHook;internal readonly List<string> Calls=[];
        internal void Visit(string stage){if(Record)Calls.Add(stage);Hook?.Invoke(stage);}
        bool ISchemaAuthority.Owns(ISchemaComponentHandle value){OwnHook?.Invoke(value);return Owned.Contains(value)||!ForeignBuiltin&&(ReferenceEquals(value,Integer)||ReferenceEquals(value,Decimal));}
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveType(SchemaExpandedName name,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();Assert.Equal("http://www.w3.org/2001/XMLSchema",name.NamespaceUri);string stage="resolve-"+name.LocalName;Visit(stage);if(ForceStage==stage&&NullOutcome)return null!;return new Answer<ISchemaComponentHandle>(ForceStage==stage?ForceKind:SchemaResultKind.Supported,NullBuiltin?null!:name.LocalName=="integer"?Integer:Decimal,this);
        }
        private SchemaResult<bool> Relation(ISchemaComponentHandle value,ISchemaComponentHandle expected,bool derived,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();string family=ReferenceEquals(expected,Integer)?"integer":"decimal";string stage=(derived?"derive-":"same-")+family;Visit(stage);if(ForceStage==stage&&NullOutcome)return null!;
            return new Answer<bool>(ForceStage==stage?ForceKind:SchemaResultKind.Supported,ReferenceEquals(value,actual)&&Family==family&&(derived||Same),this);
        }
        SchemaResult<bool> ISchemaAuthority.IsSameComponent(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>Relation(a,b,false,t);
        SchemaResult<bool> ISchemaAuthority.IsDerivedFrom(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>Relation(a,b,true,t);
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveElementDeclaration(SchemaExpandedName n,CancellationToken t)=>throw new Exception("element poison");
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveAttributeDeclaration(SchemaExpandedName n,CancellationToken t)=>throw new Exception("attribute poison");
        SchemaResult<bool> ISchemaAuthority.IsAffiliatedWith(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>throw new Exception("affiliation poison");
        SchemaResult<ImmutableArray<ISchemaComponentHandle>> ISchemaAuthority.GetComponentClosure(ISchemaComponentHandle a,CancellationToken t)=>throw new Exception("closure poison");
    }
    private static XdmValue Read(Fixture f,QuerySourceSession.Operation op,QueryTypedContainerTests.Container c)=>op.ReadRegisteredTypedContainer(f.Snapshot,f.Issuer,f.Input,f.Origin,c,8,8,2048,2048).GetTransport(0);
    private static XdmValue Project(Fixture f,QuerySourceSession.Operation op,XdmValue value,int digits=2048)=>op.ProjectTypedNumeric(f.Snapshot,value,8,digits,2048);
    /// <summary>Authoritative sameness or derivation admits selected numeric types independently from governing list type.</summary>
    /// <param name="integer">Selects the integer family.</param>
    /// <param name="same">Selects authoritative sameness instead of derivation.</param>
    [Theory][InlineData(true,true)][InlineData(true,false)][InlineData(false,true)][InlineData(false,false)]
    public void PositiveFamiliesAndPlainComputation(bool integer,bool same)
    {
        using var f=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing){Family=integer?"integer":"decimal",Same=same};f.Issuer.SelectedAuthority=authority;var c=f.Value(1,true);var plain=integer?XdmValue.FromInteger(BigInteger.Pow(10,1000),"integer","derived"):XdmValue.FromDecimal(new XdmExactDecimal(12300,4,2048,2048),"decimal","derived");c.Items=[new(plain,new Provenance(c,f.Governing,f.Actual,f.Origin))];
        f.Run(op=>{var marked=Read(f,op,c);var marker=XdmValue.SchemaTransportMarker(marked);var projected=Project(f,op,marked);Assert.Null(XdmValue.SchemaTransportMarker(projected));Assert.Null(projected.UserSchemaTypeName);var limits=new ExactArithmeticLimits(2048,2048);var sum=limits.ToValue(limits.Add(limits.Read(projected),limits.Read(XdmValue.FromInteger(0)),op.Check));Assert.Null(XdmValue.SchemaTransportMarker(sum));Assert.True(sum.SchemaTransportMatches(projected));Assert.Same(marker,XdmValue.SchemaTransportMarker(marked));Assert.Throws<NotSupportedException>(()=>limits.Read(marked));return 0;});
        Assert.Equal("resolve-integer",authority.Calls[0]);if(integer)Assert.DoesNotContain("resolve-decimal",authority.Calls);else Assert.Contains("resolve-decimal",authority.Calls);
    }
    /// <summary>Numeric-looking data and labels do not authorize a nonnumeric actual type.</summary>
    [Fact] public void NoNameOrKindAuthority()
    {
        using var f=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing){Family="string"};f.Issuer.SelectedAuthority=authority;var c=f.Value();c.Items=[new(XdmValue.FromInteger(1,"integer","xs:integer"),new Provenance(c,f.Governing,f.Actual,f.Origin))];f.Run(op=>{var marked=Read(f,op,c);Assert.Throws<NotSupportedException>(()=>Project(f,op,marked));return 0;});
    }
    /// <summary>Authorized type families do not excuse incompatible primitive representations.</summary>
    /// <param name="family">Authorized family.</param>
    /// <param name="kind">Payload selector.</param>
    [Theory][InlineData("integer",0)][InlineData("integer",1)][InlineData("integer",2)][InlineData("decimal",1)][InlineData("decimal",3)]
    public void InconsistentPayloadRefuses(string family,int kind)
    {
        using var f=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing){Family=family};f.Issuer.SelectedAuthority=authority;var c=f.Value();var value=kind switch{0=>XdmValue.FromDecimal(1m),1=>XdmValue.FromString("1"),2=>XdmValue.FromDouble(1),_=>XdmValue.FromInteger(1)};c.Items=[new(value,new Provenance(c,f.Governing,f.Actual,f.Origin))];f.Run(op=>{var marked=Read(f,op,c);Assert.Throws<ArgumentException>(()=>Project(f,op,marked));return 0;});
    }
    /// <summary>Unsupported/NotFound/Invalid answers refuse without reading unavailable Value.</summary>
    /// <param name="stage">Authority operation.</param>
    /// <param name="kind">Unsupported category.</param>
    [Theory][InlineData("resolve-integer",SchemaResultKind.Unsupported)][InlineData("resolve-integer",SchemaResultKind.NotFound)][InlineData("resolve-integer",SchemaResultKind.Invalid)][InlineData("same-integer",SchemaResultKind.Unsupported)][InlineData("derive-integer",SchemaResultKind.Unsupported)][InlineData("resolve-decimal",SchemaResultKind.Unsupported)][InlineData("same-decimal",SchemaResultKind.Unsupported)][InlineData("derive-decimal",SchemaResultKind.Unsupported)]
    public void UnsupportedOutcomeRefuses(string stage,SchemaResultKind kind)
    {
        using var f=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing){ForceStage=stage,ForceKind=kind,Family=stage.Contains("decimal")?"decimal":"integer"};f.Issuer.SelectedAuthority=authority;var c=f.Value();if(authority.Family=="decimal")c.Items=[new(XdmValue.FromDecimal(1m),new Provenance(c,f.Governing,f.Actual,f.Origin))];f.Run(op=>{var marked=Read(f,op,c);Assert.Throws<NotSupportedException>(()=>Project(f,op,marked));return 0;});Assert.Equal(0,authority.UnsupportedValueReads);
    }
    /// <summary>Null responses, null handles and unowned resolved builtins cannot establish authority.</summary>
    /// <param name="mode">Malformed response selector.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)] public void MalformedAuthorityRefuses(int mode)
    {
        using var f=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing){ForceStage=mode==1?"same-integer":"resolve-integer",NullOutcome=mode<2,NullBuiltin=mode==2,ForeignBuiltin=mode==3};f.Issuer.SelectedAuthority=authority;var c=f.Value();f.Run(op=>{var marked=Read(f,op,c);Assert.Throws<ArgumentException>(()=>Project(f,op,marked));return 0;});
    }
    /// <summary>Revocation at each authority/result stage prevents projection before later getters.</summary>
    /// <param name="stage">Stage revoking the input.</param>
    [Theory][InlineData("resolve-integer")][InlineData("kind")][InlineData("value")][InlineData("same-integer")][InlineData("derive-integer")]
    public void RevocationDuringAuthorityCall(string stage)
    {
        using var f=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing);f.Issuer.SelectedAuthority=authority;var c=f.Value();f.Run(op=>{var marked=Read(f,op,c);authority.Hook=s=>{if(s==stage)f.Input.Available=false;};Assert.Throws<ObjectDisposedException>(()=>Project(f,op,marked));return 0;});
    }
    /// <summary>Current type membership is checked after relation answers, even a positive answer.</summary>
    [Fact] public void ActualTypeRevocationAfterAnswer()
    {
        using var f=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing){Same=true};f.Issuer.SelectedAuthority=authority;var c=f.Value();f.Run(op=>{var marked=Read(f,op,c);authority.Hook=s=>{if(s=="same-integer")authority.Owned.Remove(f.Actual);};Assert.Throws<ArgumentException>(()=>Project(f,op,marked));return 0;});
    }
    /// <summary>Provider failure does not mutate evidence or prevent a later valid projection.</summary>
    [Fact] public void FailureAndRetry()
    {
        using var f=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing);f.Issuer.SelectedAuthority=authority;var c=f.Value();f.Run(op=>{var marked=Read(f,op,c);var marker=XdmValue.SchemaTransportMarker(marked);authority.Hook=s=>{if(s=="derive-integer")throw new InvalidOperationException("provider");};Assert.Equal("provider",Assert.Throws<InvalidOperationException>(()=>Project(f,op,marked)).Message);authority.Hook=null;Assert.Equal(1,Project(f,op,marked).IntegerValue);Assert.Same(marker,XdmValue.SchemaTransportMarker(marked));return 0;});
    }
    /// <summary>Raw expiry and foreign operation membership refuse before authority lookups.</summary>
    [Fact] public void RawAndForeignScopeRefuse()
    {
        using var f=new Fixture();using var g=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing);f.Issuer.SelectedAuthority=authority;QuerySourceSession.Operation? escaped=null;var marked=f.Run(op=>{escaped=op;return Read(f,op,f.Value());});authority.Hook=_=>throw new Exception("foreign authority poison");Assert.Throws<ObjectDisposedException>(()=>Project(f,escaped!,marked));g.Run(op=>{Assert.Throws<ArgumentException>(()=>op.ProjectTypedNumeric(f.Snapshot,marked,8,2048,2048));return 0;});
    }
    /// <summary>Cancellation and close during native-style authority callbacks retain raw ownership until completion.</summary>
    /// <param name="close">Whether the session closes rather than canceling.</param>
    [Theory][InlineData(false)][InlineData(true)] public void InterruptionDuringRelation(bool close)
    {
        using var f=new Fixture();using var cancel=new CancellationTokenSource();var authority=new NumericAuthority(f.Actual,f.Governing);f.Issuer.SelectedAuthority=authority;var c=f.Value();f.Run(op=>{var marked=Read(f,op,c);authority.Hook=s=>{if(s=="same-integer"){if(close)f.Session.Dispose();else cancel.Cancel();}};if(close)Assert.Throws<ObjectDisposedException>(()=>Project(f,op,marked));else Assert.Throws<OperationCanceledException>(()=>Project(f,op,marked));Assert.Equal(0,f.Input.Releases);return 0;},cancel.Token);
    }
    /// <summary>Current supplied numeric capacity is enforced before authority calls and no huge formatting is required.</summary>
    [Fact] public void WarmQuotaBeforeAuthority()
    {
        using var f=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing);f.Issuer.SelectedAuthority=authority;var c=f.Value();c.Items=[new(XdmValue.FromInteger(BigInteger.Pow(10,1000)),new Provenance(c,f.Governing,f.Actual,f.Origin))];f.Run(op=>{var marked=Read(f,op,c);authority.Hook=_=>throw new Exception("quota authority poison");Assert.Throws<NotSupportedException>(()=>Project(f,op,marked,10));return 0;});
    }
    /// <summary>Measures current authority-authorized projection separately from public identity and arithmetic setup.</summary>
    [Fact] public void ProjectionAllocationObservation()
    {
        using var f=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing){Record=false};f.Issuer.SelectedAuthority=authority;var c=f.Value();var marked=f.Run(op=>Read(f,op,c));int reads=c.Reads.Count;f.Run(op=>{for(int i=0;i<100;i++)Project(f,op,marked);long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)Project(f,op,marked);long bytes=(GC.GetAllocatedBytesForCurrentThread()-before)/1000;output.WriteLine($"Typed numeric projection authorize: {bytes} bytes/call");Assert.InRange(bytes,1,16384);return 0;});Assert.Equal(reads,c.Reads.Count);
    }

    /// <summary>The selected builtin remains owned through projection delivery, not only the relation query.</summary>
    [Fact] public void BuiltinRevocationAtDeliveryRefuses()
    {
        using var f=new Fixture();var authority=new NumericAuthority(f.Actual,f.Governing);f.Issuer.SelectedAuthority=authority;var c=f.Value();f.Run(op=>{var marked=Read(f,op,c);bool relation=false;int actualChecks=0;authority.Hook=s=>{if(s=="derive-integer")relation=true;};authority.OwnHook=type=>{if(relation&&ReferenceEquals(type,f.Actual)&&++actualChecks==2)authority.ForeignBuiltin=true;};Assert.Throws<ArgumentException>(()=>Project(f,op,marked));return 0;});
    }
}
