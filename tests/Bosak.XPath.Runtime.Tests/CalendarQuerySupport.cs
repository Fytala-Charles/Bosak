// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 25 November 2026
// PURPOSE              : Provides public-contract authority fixtures for guarded calendar queries.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 25-11-2026     | Creation                                                                                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Api;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using S = Bosak.XPath.Runtime.Tests.CalendarTransportSupport;
using QuerySession = Bosak.XPath.Api.Schema.SchemaQuerySession;

namespace Bosak.XPath.Runtime.Tests;

// Public-contract provider fixtures; the package consumer recompiles these without engine friend access.
internal static class CalendarQuerySupport
{
    private sealed class Result<T>(T value, bool supported = true) : SchemaResult<T>
    {
        /// <inheritdoc/>
        public override SchemaResultKind Kind => supported ? SchemaResultKind.Supported : SchemaResultKind.Unsupported;
        /// <inheritdoc/>
        public override T Value => supported ? value : throw new Exception("Unsupported result value poison");
        /// <inheritdoc/>
        public override string? DiagnosticCode => throw new Exception("Diagnostic poison");
    }
    internal sealed class Authority : ISchemaAuthority
    {
        internal readonly Dictionary<string,S.Handle> Builtins = new(StringComparer.Ordinal)
        { ["dateTime"] = new(), ["time"] = new(), ["dateTimeStamp"] = new(), ["string"] = new() };
        internal readonly HashSet<ISchemaComponentHandle> Types = new(ReferenceEqualityComparer.Instance);
        internal readonly Dictionary<ISchemaComponentHandle,ISchemaComponentHandle> Parents = new(ReferenceEqualityComparer.Instance);
        internal Action<string>? Hook;
        internal bool UnsupportedRelation, ForeignResolution;
        internal int Relations, Resolutions;
        internal Authority()
        {
            Types.UnionWith(Builtins.Values);Parents.Add(Builtins["dateTimeStamp"],Builtins["dateTime"]);
        }
        internal S.Handle Derived(string family)
        {
            var handle = new S.Handle();Types.Add(handle);Parents.Add(handle,Builtins[family]);return handle;
        }
        bool ISchemaAuthority.Owns(ISchemaComponentHandle type) { Hook?.Invoke("owns");return Types.Contains(type); }
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveType(SchemaExpandedName name,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();Hook?.Invoke("resolve:"+name.LocalName);Resolutions++;
            if(ForeignResolution)return new Result<ISchemaComponentHandle>(new S.Handle());
            return name.NamespaceUri=="http://www.w3.org/2001/XMLSchema"&&Builtins.TryGetValue(name.LocalName,out var value)
                ?new Result<ISchemaComponentHandle>(value):new Result<ISchemaComponentHandle>(null!,false);
        }
        SchemaResult<bool> ISchemaAuthority.IsSameComponent(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken token)
        {token.ThrowIfCancellationRequested();Hook?.Invoke("same");Relations++;return new Result<bool>(ReferenceEquals(a,b),!UnsupportedRelation);}
        SchemaResult<bool> ISchemaAuthority.IsDerivedFrom(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();Hook?.Invoke("derived");Relations++;var seen=new HashSet<ISchemaComponentHandle>(ReferenceEqualityComparer.Instance);bool answer=false;
            while(seen.Add(a)&&Parents.TryGetValue(a,out var parent)){if(ReferenceEquals(parent,b)){answer=true;break;}a=parent;}
            return new Result<bool>(answer,!UnsupportedRelation);
        }
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveElementDeclaration(SchemaExpandedName n,CancellationToken t)=>throw new Exception("Element lookup poison");
        SchemaResult<ISchemaComponentHandle> ISchemaAuthority.ResolveAttributeDeclaration(SchemaExpandedName n,CancellationToken t)=>throw new Exception("Attribute lookup poison");
        SchemaResult<bool> ISchemaAuthority.IsAffiliatedWith(ISchemaComponentHandle a,ISchemaComponentHandle b,CancellationToken t)=>throw new Exception("Affiliation poison");
        SchemaResult<ImmutableArray<ISchemaComponentHandle>> ISchemaAuthority.GetComponentClosure(ISchemaComponentHandle a,CancellationToken t)=>throw new Exception("Closure poison");
    }
    internal sealed class Fixture : IDisposable
    {
        internal readonly S.Input Input = new();
        internal readonly S.Assessor Assessor = new();
        internal readonly S.Advisor Advisor;
        internal readonly Authority Authority = new();
        internal readonly S.Handle Governing = new();
        internal readonly S.Origin Origin = new();
        internal readonly QuerySession Session;
        internal readonly SchemaQuerySnapshot Snapshot;
        internal Fixture(string expression="$left eq $right",int timezone=0,int digits=256,int scale=256,CompileOptions? options=null)
        {
            Assessor.Selected=Authority;Advisor=new(Assessor);Assessor.Advisor=Advisor;
            Authority.Types.Add(Governing);Advisor.Origins.Add(Origin);
            Session=QuerySession.CompileCalendarComparison(expression,new SchemaQueryLimits(3,32,8,digits,scale),Assessor,timezone,options).Value;
            Snapshot=Session.OpenSnapshot(Input,Input,Origin).Value;
        }
        internal S.Container Value(XdmValue value,ISchemaComponentHandle? actual=null,int count=1)
        {
            actual??=Authority.Builtins[value.Kind==XdmValueKind.Time?"time":value.SchemaTypeName=="dateTimeStamp"?"dateTimeStamp":"dateTime"];
            var c=new S.Container { Governing=Governing,Fast=count==1,List=count!=1 };
            c.Items=Enumerable.Range(0,count).Select(_=>new SchemaAtomicItem(value,new S.Provenance(c,Governing,actual,Origin))).ToArray();
            Advisor.Values.Add(c,(Input,Origin));return c;
        }
        internal T Run<T>(Func<SchemaQueryOperation,T> work,CancellationToken token=default)=>Session.Run([Snapshot],work,token);
        internal (XdmValue Left,XdmValue Right) Admit(S.Container left,S.Container right)
            =>Run(op=>(op.ReadTypedValue(Snapshot,left).Value,op.ReadTypedValue(Snapshot,right).Value));
        internal bool Compare(XdmValue left,XdmValue right)=>Run(op=>op.Evaluate(Context(left,right)).BooleanValue);
        internal static Bosak.XPath.Runtime.Vm.EvaluationContext Context(XdmValue left,XdmValue right)
            =>new Bosak.XPath.Runtime.Vm.EvaluationContext().WithVariable("left",left).WithVariable("right",right);
        /// <inheritdoc/>
        public void Dispose()=>Session.Dispose();
    }
    internal static XdmValue D(string text="2000-01-02T03:04:05.123456789Z",bool stamp=false)
        =>XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse(text,stamp?XdmExactCalendarKind.DateTimeStamp:XdmExactCalendarKind.DateTime,256,256));
    internal static XdmValue T(string text="03:04:05.123456789Z")
        =>XdmValue.FromExactCalendar(XdmExactCalendarValue.Parse(text,XdmExactCalendarKind.Time,256,256));
}
