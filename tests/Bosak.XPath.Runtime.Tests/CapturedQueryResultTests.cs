// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Verifies guarded deferred public query continuations, expiration and call tails.
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
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
// ===========================================================================================================================================================
using System.Collections.Immutable;
using Bosak.XPath.Api.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
using Xunit;
namespace Bosak.XPath.Runtime.Tests;

/// <summary>Exercises engine result continuation boundaries with instrumented sequence callbacks.</summary>
public sealed class CapturedQueryResultTests
{
    private sealed class Executable
    {
        internal XdmValue Value;
        internal XdmValue Evaluate(EvaluationContext context)=>Value;
    }
    private class Sequence:IXdmSequence
    {
        internal int Calls,Acquisitions,Advances,Currents;internal Action<string>? Hook;internal bool Known=true;internal XdmValue[] Values=[XdmValue.FromInteger(1),XdmValue.FromInteger(2)];
        internal void Hit(string stage){Calls++;Hook?.Invoke(stage);}
        bool IXdmSequence.TryGetLength(out int n){Hit("length");n=Values.Length;return Known;}
        IXdmSequenceEnumerator IXdmSequence.GetEnumerator(){Hit("enumerator");Acquisitions++;return new Enumerator(this);}
        private sealed class Enumerator(Sequence owner):IXdmSequenceEnumerator
        {
            private int _index=-1;
            XdmValue IXdmSequenceEnumerator.Current{get{owner.Currents++;owner.Hit("current");return owner.Values[_index];}}
            bool IXdmSequenceEnumerator.MoveNext(){owner.Advances++;owner.Hit("move");return ++_index<owner.Values.Length;}
        }
    }
    private sealed class SinglePass:Sequence,ISinglePassSequence;
    private sealed class Fixture:IDisposable
    {
        internal readonly Executable Executable=new();internal readonly CapturedHostBinding Binding;
        internal Bosak.XPath.Api.Schema.SchemaQuerySession Session=>Binding.Session;
        internal Fixture(IXdmSequence source)
        {Executable.Value=XdmValue.FromSequence(XdmSequence.FromSource(source));Binding=CapturedHostBinding.CreatePublicQuery(Executable,new(ImmutableDictionary<string,string>.Empty,"public-query-continuation-controls"),new(4,8,16,128,128),Executable.Evaluate);}
        /// <inheritdoc/>
        public void Dispose()=>Session.Dispose();
    }
    /// <summary>XQ24 all outer callbacks refuse after raw completion without invoking the underlying source.</summary>
    [Fact] public void XQ24_NoExpiredCallbackReplay()
    {var source=new Sequence();using var f=new Fixture(source);IXdmSequence? sequence=null;IXdmSequenceEnumerator? enumerator=null;f.Session.Run([],op=>{sequence=op.Evaluate(new()).SequenceValue;enumerator=sequence!.GetEnumerator();Assert.True(enumerator.MoveNext());Assert.Equal(1,enumerator.Current.IntegerValue);return 0;});int calls=source.Calls;Assert.Throws<ObjectDisposedException>(()=>sequence!.TryGetLength(out _));Assert.Throws<ObjectDisposedException>(()=>sequence!.GetEnumerator());Assert.Throws<ObjectDisposedException>(()=>enumerator!.MoveNext());Assert.Throws<ObjectDisposedException>(()=>enumerator!.Current);Assert.Equal(calls,source.Calls);}
    /// <summary>XQ25 sequence marker and known/unknown length are preserved without materialization.</summary>
    /// <param name="single">Source has the single-pass marker.</param><param name="known">Source supplies a known length.</param>
    [Theory][InlineData(false,false)][InlineData(false,true)][InlineData(true,false)][InlineData(true,true)]
    public void XQ25_LengthAndSinglePass(bool single,bool known)
    {Sequence source=single?new SinglePass():new Sequence();source.Known=known;using var f=new Fixture(source);f.Session.Run([],op=>{var result=op.Evaluate(new());var sequence=XdmSequence.FromSource(result.SequenceValue!);Assert.Equal(single,sequence.IsSinglePass);Assert.Equal(known,sequence.TryGetLength(out int n));Assert.Equal(2,n);Assert.Equal(0,source.Acquisitions);Assert.Equal(0,source.Advances);return 0;});}
    /// <summary>XQ26 original deferred source errors propagate and cannot become a successful item.</summary>
    /// <param name="stage">Failing outer source callback.</param>
    [Theory][InlineData("length")][InlineData("enumerator")][InlineData("move")][InlineData("current")]
    public void XQ26_DeferredErrors(string stage)
    {var source=new Sequence();using var f=new Fixture(source);var expected=new ApplicationException(stage);source.Hook=s=>{if(s==stage)throw expected;};f.Session.Run([],op=>{var sequence=op.Evaluate(new()).SequenceValue!;Exception? error;if(stage=="length")error=Record.Exception(()=>sequence.TryGetLength(out _));else if(stage=="enumerator")error=Record.Exception(()=>sequence.GetEnumerator());else{var e=sequence.GetEnumerator();error=Record.Exception(()=>e.MoveNext());}Assert.Same(expected,error);return 0;});}
    /// <summary>XQ27 caller-supplied evidence markers cannot masquerade as plain public query cells.</summary>
    /// <param name="mode">Empty, scalar or sequence carrier.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)] public void XQ27_MarkedCellsRefuse(int mode)
    {if(mode==0){Assert.Throws<ArgumentException>(()=>XdmValue.SchemaTransportAttach(XdmValue.Undefined,new object()));using var empty=new Fixture(new Sequence());empty.Executable.Value=XdmValue.Undefined;Assert.True(empty.Session.Run([],op=>op.Evaluate(new())).IsUndefined);return;}var value=XdmValue.SchemaTransportAttach(XdmValue.FromInteger(1),new object());var source=new Sequence{Values=[value]};using var f=new Fixture(source);if(mode==1)f.Executable.Value=value;f.Session.Run([],op=>{if(mode==1)Assert.Throws<NotSupportedException>(()=>op.Evaluate(new()));else{var e=op.Evaluate(new()).SequenceValue!.GetEnumerator();Assert.Throws<NotSupportedException>(()=>e.MoveNext());}return 0;});}
    /// <summary>XQ28 continuation overlap/reentry is refused within the exact raw window.</summary>
    [Fact] public void XQ28_ContinuationReentry()
    {var source=new Sequence();using var f=new Fixture(source);f.Session.Run([],op=>{var sequence=op.Evaluate(new()).SequenceValue!;bool seen=false;source.Hook=stage=>{if(stage!="move"||seen)return;seen=true;Assert.Throws<InvalidOperationException>(()=>sequence.TryGetLength(out _));};Assert.True(sequence.GetEnumerator().MoveNext());Assert.True(seen);return 0;});}
    /// <summary>XQ29 raw completion tracks a blocked deferred callback until its actual tail ends.</summary>
    /// <returns>A task completing after the guarded deferred tail and drain.</returns>
    [Fact] public async Task XQ29_DeferredTailDrain()
    {var source=new Sequence();using var f=new Fixture(source);using var entered=new ManualResetEventSlim();using var resume=new ManualResetEventSlim();Task<bool>? advance=null;source.Hook=stage=>{if(stage!="move")return;entered.Set();Assert.True(resume.Wait(TimeSpan.FromSeconds(10)));};var raw=f.Session.RunAsync([],op=>{var e=op.Evaluate(new()).SequenceValue!.GetEnumerator();advance=Task.Run(e.MoveNext);Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));return Task.FromResult(7);});try{Assert.False(raw.IsCompleted);Assert.False(f.Session.DrainAsync().IsCompleted);}finally{resume.Set();}await Assert.ThrowsAsync<ObjectDisposedException>(async()=>await advance!);Assert.Equal(7,await raw);await f.Session.DrainAsync();}
    /// <summary>XQ30 public evaluator must be a method on the exact captured executable.</summary>
    [Fact] public void XQ30_ExactEvaluatorTarget()
    {var first=new Executable();var second=new Executable();Assert.Throws<ArgumentException>(()=>CapturedHostBinding.CreatePublicQuery(first,new(ImmutableDictionary<string,string>.Empty,"same"),new(4,8,16,128,128),second.Evaluate));}
    /// <summary>XQ31 revocation after MoveNext prevents a subsequent foreign Current getter.</summary>
    /// <param name="cancel">Cancel the raw token rather than close the session.</param>
    [Theory][InlineData(false)][InlineData(true)]
    public void XQ31_RevocationBeforeCurrent(bool cancel)
    {var source=new Sequence();using var f=new Fixture(source);using var c=new CancellationTokenSource();source.Hook=stage=>{if(stage!="move")return;if(cancel)c.Cancel();else f.Session.Dispose();};var error=Record.Exception(()=>f.Session.Run([],op=>op.Evaluate(new()).SequenceValue!.GetEnumerator().MoveNext(),c.Token));if(cancel)Assert.IsAssignableFrom<OperationCanceledException>(error);else Assert.IsType<ObjectDisposedException>(error);Assert.Equal(0,source.Currents);}
}
