// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Stores a private materialized ordered sequence of registered atomic transports.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 05-11-2026     | Creation; materialized original-node atomic output (1.64.0).                             |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using Bosak.XPath.Core.Xdm;
namespace Bosak.XPath.Runtime.Vm;
/// <summary>Stores a private materialized ordered sequence of registered atomic transports.</summary>
internal sealed class QueryAtomizedSequence(int count):IXdmSequence
{
    private readonly XdmValue[] _values=new XdmValue[count];
    internal void Set(int index,XdmValue value)=>_values[index]=value;
    bool IXdmSequence.TryGetLength(out int length){length=_values.Length;return true;}
    IXdmSequenceEnumerator IXdmSequence.GetEnumerator()=>new Cursor(_values);
    private sealed class Cursor(XdmValue[] values):IXdmSequenceEnumerator
    {
        private int _index=-1;
        XdmValue IXdmSequenceEnumerator.Current=>(uint)_index<(uint)values.Length?values[_index]:throw new InvalidOperationException("No current atomic value.");
        bool IXdmSequenceEnumerator.MoveNext(){if(_index<values.Length)_index++;return _index<values.Length;}
    }
}
