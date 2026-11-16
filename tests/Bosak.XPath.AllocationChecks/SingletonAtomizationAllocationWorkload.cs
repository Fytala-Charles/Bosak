// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 16 November 2026
// PURPOSE              : Measures singleton atomization in an isolated worker
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
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Reflection;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;
namespace Bosak.XPath.AllocationChecks;
internal static class SingletonAtomizationAllocationWorkload
{
    private static XdmValue _sink;
    internal static object Run()
    {
        var atomize=typeof(EvaluationContext).Assembly.GetType("Bosak.XPath.Runtime.Vm.VmEngine",true)!.GetMethod("Atomize",BindingFlags.NonPublic|BindingFlags.Static)!.CreateDelegate<Func<XdmValue,XdmValue>>();
        var rows=new List<object>();
        foreach(bool present in new[]{false,true})
        {
            var input=XdmValue.FromSequence(XdmSequence.FromSource(new Source(present)));
            for(int i=0;i<10000;i++)_sink=atomize(input);
            long start=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<100000;i++)_sink=atomize(input);long bytes=GC.GetAllocatedBytesForCurrentThread()-start;
            rows.Add(new{name=present?"singleton":"empty",bytes,kind=_sink.Kind.ToString(),items=present?_sink.IntegerValue:0L});
        }
        GC.KeepAlive(_sink);
        return new{protocol=1,success=true,workload="atomization",processId=Environment.ProcessId,framework=Environment.Version.ToString(),engine=typeof(EvaluationContext).Assembly.GetName().Version!.ToString(),iterations=100000,warmup=10000,rows};
    }
    private sealed class Source : IXdmSequence
    {
        private readonly bool _present;internal Source(bool present)=>_present=present;
        /// <inheritdoc/>
        public bool TryGetLength(out int length) {length=0;return false;}
        /// <inheritdoc/>
        public IXdmSequenceEnumerator GetEnumerator()=>new Cursor(this);
        private sealed class Cursor : IXdmSequenceEnumerator
        {
            private readonly Source _owner;private int _index=-1;internal Cursor(Source owner)=>_owner=owner;
            /// <inheritdoc/>
            public XdmValue Current=>XdmValue.FromInteger(7);
            /// <inheritdoc/>
            public bool MoveNext()=>++_index==0 && _owner._present;
        }
    }
}
