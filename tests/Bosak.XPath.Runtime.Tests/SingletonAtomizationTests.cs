// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 November 2026
// PURPOSE              : Verifies full-consumption singleton atomization and fixed allocation budgets
// SPECIAL NOTES        : Unit tests verifying correctness of the underlying implementation.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 16-11-2026     | Creation                                                                                 |
//                      | Charles Korthout | 0.2   | 16-11-2026     | Allow legitimate general-comparison length query in caller fixture.                     |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Reflection;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Api;
using Bosak.XPath.Runtime.Vm;
using Xunit;
namespace Bosak.XPath.Runtime.Tests;
/// <summary>Verifies singleton atomization preserves evaluation order and errors.</summary>
public sealed class SingletonAtomizationTests
{
    private static readonly Func<XdmValue,XdmValue> Atomize=typeof(EvaluationContext).Assembly.GetType("Bosak.XPath.Runtime.Vm.VmEngine",true)!.GetMethod("Atomize",BindingFlags.NonPublic|BindingFlags.Static)!.CreateDelegate<Func<XdmValue,XdmValue>>();
    /// <summary>Checks fixed allocation budgets in a separately bounded worker.</summary>
    [Fact] public void IsolatedAllocation_StaysWithinPreRecordedBudgets() {foreach(var row in AllocationProbeRunner.Run("atomization"))Assert.InRange(row.Value,0L,(row.Key=="empty"?48L:64L)*100000);}
    /// <summary>Checks complete cardinality and source consumption.</summary>
    /// <param name="count">The input item count.</param>
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)][InlineData(5)]
    public void Cardinality_ConsumesFullSource(int count)
    {
        var s=new Source(count);var v=XdmValue.FromSequence(XdmSequence.FromSource(s));
        if(count<2) {var r=Atomize(v);Assert.Equal(count==0?XdmValueKind.Undefined:XdmValueKind.Integer,r.Kind);if(count==1)Assert.Equal(7L,r.IntegerValue);}
        else Assert.Equal("XPTY0004: Atomization requires a singleton or empty sequence, but got a sequence of length "+count,Assert.Throws<InvalidOperationException>(()=>Atomize(v)).Message);
        Assert.Equal(count+1,s.Moves);Assert.Equal(count,s.Currents);Assert.Equal(0,s.Disposals);
    }
    /// <summary>Checks tail failures precede cardinality errors.</summary>
    /// <param name="fault">The tail fault location.</param>
    [Theory][InlineData("move")][InlineData("current")]
    public void TailFailure_PrecedesCardinality(string fault)
    {
        var s=new Source(5,fault);Assert.Equal("tail-"+fault,Assert.Throws<InvalidOperationException>(()=>Atomize(XdmValue.FromSequence(XdmSequence.FromSource(s)))).Message);Assert.Equal(0,s.Disposals);
    }
    /// <summary>Checks replay refusal is owned by the marked source.</summary>
    [Fact]public void MarkedSource_RefusesSecondAcquisition()
    {
        var s=new Marked();var v=XdmValue.FromSequence(XdmSequence.FromSource(s));Assert.Equal(7L,Atomize(v).IntegerValue);Assert.Equal("single-pass",Assert.Throws<InvalidOperationException>(()=>Atomize(v)).Message);
    }
    /// <summary>Checks arithmetic and conversion callers on a genuinely lazy input.</summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="expected">The lexical result.</param>
    [Theory][InlineData("$x+2","9")][InlineData("$x*2","14")][InlineData("-$x","-7")][InlineData("+$x","7")][InlineData("$x=7","true")][InlineData("$x cast as xs:decimal","7")]
    public void Callers_PreserveLazyOperandResults(string expression,string expected)
    {
        var ctx=new EvaluationContext().WithVariable("x",XdmValue.FromSequence(XdmSequence.FromSource(new Source(1,allowLength:true))));Assert.Equal(expected,XPath31Expression.Compile(expression).Evaluate(ctx).ToString());
    }
    private class Source:IXdmSequence
    {
        private readonly int _count;private readonly string? _fault;private readonly bool _allowLength;internal int Moves,Currents,Disposals;
        internal Source(int count,string? fault=null,bool allowLength=false){_count=count;_fault=fault;_allowLength=allowLength;}
        /// <inheritdoc/>
        public bool TryGetLength(out int length){if(!_allowLength)throw new InvalidOperationException("No length shortcut");length=0;return false;}
        /// <inheritdoc/>
        public virtual IXdmSequenceEnumerator GetEnumerator()=>new Cursor(this);
        private sealed class Cursor:IXdmSequenceEnumerator,IDisposable
        {
            private readonly Source _s;private int _i=-1;internal Cursor(Source s)=>_s=s;
            /// <inheritdoc/>
            public XdmValue Current{get{_s.Currents++;if(_s._fault=="current"&&_i==_s._count-1)throw new InvalidOperationException("tail-current");return XdmValue.FromInteger(7);}}
            /// <inheritdoc/>
            public bool MoveNext(){_s.Moves++;_i++;if(_s._fault=="move"&&_i==_s._count)throw new InvalidOperationException("tail-move");return _i<_s._count;}
            /// <inheritdoc/>
            public void Dispose()=>_s.Disposals++;
        }
    }
    private sealed class Marked:Source,ISinglePassSequence
    {
        private int _taken;internal Marked():base(1){}
        /// <inheritdoc/>
        public override IXdmSequenceEnumerator GetEnumerator()=>Interlocked.Exchange(ref _taken,1)==0?base.GetEnumerator():throw new InvalidOperationException("single-pass");
    }
}
