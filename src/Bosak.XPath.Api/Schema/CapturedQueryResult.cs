// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 05 November 2026
// PURPOSE              : Guards flat deferred public query results with the exact raw host operation.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
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
using Bosak.XPath.Core.Xdm;
namespace Bosak.XPath.Api.Schema;

/// <summary>Outer sequence guard; cached scalar values and node graphs are not recursively wrapped.</summary>
internal static class CapturedQueryResult
{
    internal static XdmValue Bind(CapturedHostBinding binding,SchemaQueryOperation operation,XdmValue value)
    {
        if(!value.IsSequence){RequireCell(value,true);return value;}
        var source=value.SequenceValue??throw new ArgumentException("Missing query sequence.");
        IXdmSequence guarded=source is ISinglePassSequence?new SinglePass(binding,operation,source):new Sequence(binding,operation,source);
        return XdmValue.FromSequence(XdmSequence.FromSource(guarded));
    }
    private static void RequireCell(XdmValue value,bool allowEmpty=false)
    {
        if(XdmValue.SchemaTransportMarker(value) is not null)throw new NotSupportedException("Ordinary query results cannot carry authenticated transport markers.");
        if(allowEmpty&&value.IsUndefined)return;
        if(value.Kind is not(>=XdmValueKind.String and <=XdmValueKind.Binary or XdmValueKind.Node))
            throw new NotSupportedException("The ordinary query profile returns only flat atomic and node items.");
    }
    private class Sequence(CapturedHostBinding binding,SchemaQueryOperation operation,IXdmSequence source):IXdmSequence
    {
        /// <inheritdoc/>
        public bool TryGetLength(out int length)
        {
            var result=binding.ContinuePublic(operation,()=>{var known=source.TryGetLength(out int count);if(known&&count<0)throw new InvalidOperationException("Negative query sequence length.");return(known,count);});
            length=result.count;return result.known;
        }
        /// <inheritdoc/>
        public IXdmSequenceEnumerator GetEnumerator()=>binding.ContinuePublic(operation,()=>new Enumerator(binding,operation,source.GetEnumerator()??throw new InvalidOperationException("Missing query enumerator.")));
    }
    private sealed class SinglePass(CapturedHostBinding binding,SchemaQueryOperation operation,IXdmSequence source):Sequence(binding,operation,source),ISinglePassSequence;
    private sealed class Enumerator(CapturedHostBinding binding,SchemaQueryOperation operation,IXdmSequenceEnumerator inner):IXdmSequenceEnumerator
    {
        private XdmValue _current;
        /// <inheritdoc/>
        public XdmValue Current=>binding.ContinuePublic(operation,()=>_current);
        /// <inheritdoc/>
        public bool MoveNext()=>binding.ContinuePublic(operation,guard=>
        {
            if(!inner.MoveNext())return false;
            guard();var value=inner.Current;guard();RequireCell(value);_current=value;return true;
        });
    }
}
