// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 01 November 2026
// PURPOSE              : Verifies actual multi-snapshot admission, typed ownership and raw cleanup.
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
/// <summary>Checks composite operations over real kernel snapshots and evidence.</summary>
public sealed class SchemaMultiSnapshotTests
{
    private sealed class Scalar(object origin, object type) : ISchemaIssuedScalar
    {
        internal int Reads;
        XdmValue ISchemaIssuedScalar.Value { get { Reads++; return XdmValue.FromInteger(7); } }
        object ISchemaIssuedScalar.DeclaredType => type;
        object ISchemaIssuedScalar.ActualType => type;
        object ISchemaIssuedScalar.OriginalAssessment => origin;
    }
    private sealed class Issuer : ISchemaEvidenceIssuer
    {
        internal readonly object A=new(), B=new(), Type=new();
        bool ISchemaEvidenceIssuer.OwnsAssessment(object value)=>ReferenceEquals(value,A)||ReferenceEquals(value,B);
        bool ISchemaEvidenceIssuer.OwnsType(object value)=>ReferenceEquals(value,Type);
        bool ISchemaEvidenceIssuer.OwnsValue(ISchemaIssuedScalar value)=>value is Scalar;
        bool ISchemaEvidenceIssuer.IsSameType(object a,object b)=>ReferenceEquals(a,b);
    }
    private sealed class Fixture : IDisposable
    {
        internal readonly Issuer Issuer=new();
        internal readonly SchemaQuerySession Session;
        internal readonly SchemaEvidenceKernel.Snapshot A,B;
        internal readonly List<string> Released=[];
        internal Fixture()
        {
            Session=new(new SchemaQueryEnvironment(ImmutableDictionary<string,string>.Empty,"multi"),c=>c.SnapshotVariables()[("v","")],Issuer,8,16);
            A=Session.OpenSnapshot(Session.Environment,new object(),Issuer.A,()=>Released.Add("A"));
            B=Session.OpenSnapshot(Session.Environment,new object(),Issuer.B,()=>Released.Add("B"));
        }
        /// <summary>Closes remaining admissions.</summary>
        public void Dispose()=>Session.Dispose();
    }
    private static long Evaluate(SchemaQuerySession.Operation op, long value=42)
    {
        var inputs=op.CaptureInputs(new EvaluationContext().WithVariable("v",XdmValue.FromInteger(value)),(_,_)=>false,16,16);long result=0;
        op.EvaluateAtomic(inputs,16).ForEachAtomic(v=>result=v.IntegerValue);return result;
    }
    /// <summary>Uses two real snapshots and retains each scalar's original assessment.</summary>
    [Fact] public void KeepsOriginalAssessments()
    {
        using var f=new Fixture();f.Session.Run(f.Session.Environment,[f.A,f.B],op=>
        {
            var a=op.ReadValue(f.A,new Scalar(f.Issuer.A,f.Issuer.Type));var b=op.ReadValue(f.B,new Scalar(f.Issuer.B,f.Issuer.Type));
            Assert.Same(f.A,f.Session.ReadEvidence(a)!.Snapshot);Assert.Same(f.B,f.Session.ReadEvidence(b)!.Snapshot);Assert.Equal(42,Evaluate(op));return 0;
        });Assert.Empty(f.Released);
    }
    /// <summary>Rejects a purported scalar from the wrong original assessment.</summary>
    [Fact] public void RejectsWrongAssessment()
    {using var f=new Fixture();f.Session.Run(f.Session.Environment,[f.A,f.B],op=>{Assert.Throws<ArgumentException>(()=>op.ReadValue(f.A,new Scalar(f.Issuer.B,f.Issuer.Type)));return 0;});}
    /// <summary>Rejects a nonmember before reading scalar payload.</summary>
    [Fact] public void RejectsNonmemberBeforeScalarGetter()
    {using var f=new Fixture();var scalar=new Scalar(f.Issuer.B,f.Issuer.Type);f.Session.Run(f.Session.Environment,[f.A],op=>{Assert.Throws<ArgumentException>(()=>op.ReadValue(f.B,scalar));Assert.Equal(0,scalar.Reads);return 0;});}
    /// <summary>Rejects invalid membership before callback and leaves admissions owned by the host.</summary>
    /// <param name="kind">Invalid membership shape.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(3)]
    public void RejectsInvalidMembership(int kind)
    {
        using var f=new Fixture();using var other=new Fixture();bool called=false;
        ImmutableArray<SchemaEvidenceKernel.Snapshot> members=kind switch{0=>default,1=>[null!],2=>[f.A,f.A],_=>[f.A,other.B]};
        Assert.ThrowsAny<ArgumentException>(()=>f.Session.Run(f.Session.Environment,members,_=>{called=true;return 0;}));Assert.False(called);Assert.Empty(f.Released);
    }
    /// <summary>Explicit source-free operations work and their results remain callback-borrowed.</summary>
    [Fact] public void EmptyOperation()
    {using var f=new Fixture();SchemaAtomicResult escaped=default;Assert.Equal(42,f.Session.Run(f.Session.Environment,ImmutableArray<SchemaEvidenceKernel.Snapshot>.Empty,op=>{var input=op.CaptureInputs(new EvaluationContext().WithVariable("v",XdmValue.FromInteger(42)),(_,_)=>false,16,16);escaped=op.EvaluateAtomic(input,16);return Evaluate(op);}));Assert.Throws<ObjectDisposedException>(()=>escaped.ForEachAtomic(_=>{}));}
    /// <summary>Same-membership requests cannot transfer to another composite operation.</summary>
    [Fact] public void InputsRequireExactOperation()
    {using var f=new Fixture();var input=f.Session.Run(f.Session.Environment,[f.A,f.B],op=>op.CaptureInputs(new EvaluationContext().WithVariable("v",XdmValue.FromInteger(1)),(_,_)=>false,16,16));f.Session.Run(f.Session.Environment,[f.A,f.B],op=>{Assert.Throws<ArgumentException>(()=>op.EvaluateAtomic(input,16));return 0;});}
    /// <summary>Single-snapshot captures also cannot bypass composite membership tokens.</summary>
    [Fact] public void SingleInputsCannotTransfer()
    {using var f=new Fixture();var input=f.Session.Run(f.Session.Environment,f.A,op=>op.CaptureInputs(new EvaluationContext().WithVariable("v",XdmValue.FromInteger(1)),_=>false,16,16));f.Session.Run(f.Session.Environment,[f.A],op=>{Assert.Throws<ArgumentException>(()=>op.EvaluateAtomic(input,16));return 0;});}
    /// <summary>Requires exactly one owner for each input node before host execution.</summary>
    /// <param name="ambiguous">Whether both admissions falsely claim the same node.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void RejectsUnownedAndAmbiguousNodes(bool ambiguous)
    {using var f=new Fixture();var node=new XDocumentNode(XElement.Parse("<n/>"));f.Session.Run(f.Session.Environment,[f.A,f.B],op=>{Assert.Throws<NotSupportedException>(()=>op.CaptureInputs(new EvaluationContext().WithFocus(XdmValue.FromNode(node),1,1),(_,_)=>ambiguous,16,16));return 0;});}
    /// <summary>Rejects evidence from a live same-session snapshot absent from the group.</summary>
    [Fact] public void RejectsNonmemberEvidenceInInputs()
    {using var f=new Fixture();var value=f.Session.Run(f.Session.Environment,f.B,op=>op.ReadValue(new Scalar(f.Issuer.B,f.Issuer.Type)));f.Session.Run(f.Session.Environment,[f.A],op=>{Assert.Throws<ArgumentException>(()=>op.CaptureInputs(new EvaluationContext().WithVariable("v",value),(_,_)=>false,16,16));return 0;});}
    /// <summary>Revoking one source invalidates the entire group without releasing active leases.</summary>
    [Fact] public void RevocationIsGroupWide()
    {using var f=new Fixture();f.Session.Run(f.Session.Environment,[f.A,f.B],op=>{f.A.Dispose();Assert.Throws<ObjectDisposedException>(()=>Evaluate(op));Assert.Empty(f.Released);return 0;});Assert.Equal(new[]{"A"},f.Released);Assert.Equal(42,f.Session.Run(f.Session.Environment,[f.B],op=>Evaluate(op)));}
    /// <summary>Work and both deferred cleanup failures survive in reverse member order.</summary>
    [Fact] public void PreservesWorkAndEveryCleanupFailure()
    {
        using var f=new Fixture();var ea=new Exception("A");var eb=new Exception("B");var primary=new ApplicationException("work");
        var a=f.Session.OpenSnapshot(f.Session.Environment,new object(),f.Issuer.A,()=>throw ea);var b=f.Session.OpenSnapshot(f.Session.Environment,new object(),f.Issuer.B,()=>throw eb);
        var error=Assert.Throws<AggregateException>(()=>f.Session.Run<int>(f.Session.Environment,[a,b],op=>{a.Dispose();b.Dispose();throw primary;}));Assert.Equal(new Exception[]{primary,eb,ea},error.InnerExceptions);a.Dispose();b.Dispose();
    }
    /// <summary>Nested operations retain independent raw leases.</summary>
    [Fact] public void NestedOperations()
    {using var f=new Fixture();f.Session.Run(f.Session.Environment,[f.A,f.B],outer=>{Assert.Equal(42,f.Session.Run(f.Session.Environment,[f.A,f.B],inner=>Evaluate(inner)));Assert.Equal(42,Evaluate(outer));return 0;});}
    /// <summary>Canceled waits and session close retain both sources until raw work ends.</summary>
    /// <returns>The completed raw callback checks.</returns>
    [Fact] public async Task CanceledWaitAndCloseRetainRawSources()
    {
        using var f=new Fixture();var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var resume=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var raw=f.Session.RunAsync(f.Session.Environment,[f.A,f.B],async op=>{Assert.Equal(42,Evaluate(op));entered.SetResult();await resume.Task;Assert.Throws<ObjectDisposedException>(()=>Evaluate(op));return 0;});
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));using var cancel=new CancellationTokenSource();cancel.Cancel();await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>raw.WaitAsync(cancel.Token));f.Session.Dispose();Assert.Empty(f.Released);resume.SetResult();await raw.WaitAsync(TimeSpan.FromSeconds(10));Assert.Equal(new[]{"B","A"},f.Released);
    }
    /// <summary>Raw and implicit-source paths cannot bypass composite input admission.</summary>
    [Fact] public void RejectsImplicitSingleSourceMethods()
    {using var f=new Fixture();f.Session.Run(f.Session.Environment,[f.A,f.B],op=>{Assert.Throws<NotSupportedException>(()=>op.Evaluate(new EvaluationContext()));Assert.Throws<NotSupportedException>(()=>op.ReadValue(new Scalar(f.Issuer.A,f.Issuer.Type)));Assert.Throws<NotSupportedException>(()=>op.CaptureInputs(new EvaluationContext(),_=>true,16,16));return 0;});}
}
