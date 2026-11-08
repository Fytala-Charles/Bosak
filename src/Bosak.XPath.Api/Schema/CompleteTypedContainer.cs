// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 06 November 2026
// PURPOSE              : Delivers complete borrowed typed containers with plain cells and guarded metadata.
// SPECIAL NOTES        : Public surface API for compiling and evaluating XPath 3.1 expressions.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 06-11-2026     | Creation; complete ordinary typed-container delivery (1.79.0).                            |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2 | 06-11-2026     | Guard whole compatible container and cursor accesses in paired windows (1.80.0). |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3 | 06-11-2026     | Select eleven public atomic kinds and validate carrier structure (1.81.0). |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections;
using System.Collections.Immutable;
using Bosak.XPath.Core.Schema;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Runtime.Vm;

namespace Bosak.XPath.Api.Schema;

/// <summary>Delivers complete borrowed typed containers with plain cells and guarded metadata.</summary>
internal sealed class CompleteTypedContainer : ISchemaTypedValue, IEnumerable<SchemaAtomicItem>
{
    private readonly QueryTypedContainer _source;
    private readonly CompatibleAdmissionPlan.PublicOperation? _compatible;
    private readonly ISchemaComponentHandle _governing;
    private readonly bool _list;
    private readonly SchemaAtomicItem[] _items;
    private readonly ImmutableArray<QueryTypedContainer.Cell> _cells;
    private CompleteTypedContainer(QueryTypedContainer source,CompatibleAdmissionPlan.PublicOperation? compatible)
    {
        _source=source;_compatible=compatible;var captured=source.Capture();_governing=captured.Governing;_list=captured.List;_cells=captured.Cells;
        _items=new SchemaAtomicItem[captured.Cells.Length];
        for(int i=0;i<_items.Length;i++)_items[i]=new(captured.Cells[i].Value,new Evidence(this,i));
    }
    internal static SchemaResult<ISchemaTypedValue> Create(QueryTypedContainer source,CompatibleAdmissionPlan.PublicOperation? compatible=null)
    {
        var captured=source.Capture();bool supported=true;
        foreach(var cell in captured.Cells)
        {
            var value=cell.Value;
            if(value.Kind==XdmValueKind.String&&value.StringValue is null)throw new ArgumentException("String payload is null.");
            if(value.Kind==XdmValueKind.Duration&&value.DurationValue is null)throw new ArgumentException("Duration payload is null.");
            if(value.Kind==XdmValueKind.QName)
            {
                var name=value.QNameValue;
                if(string.IsNullOrEmpty(name.LocalName)||name.NamespaceUri is null||name.Prefix is null)
                    throw new ArgumentException("QName payload has missing name fields.");
            }
            supported &= value.Kind is XdmValueKind.String or XdmValueKind.Boolean or XdmValueKind.Integer or XdmValueKind.Decimal
                or XdmValueKind.Double or XdmValueKind.Float or XdmValueKind.Date or XdmValueKind.Time
                or XdmValueKind.DateTime or XdmValueKind.Duration or XdmValueKind.QName;
        }
        if(!supported)return SchemaQuerySession.UnsupportedTyped<ISchemaTypedValue>();
        var result=new CompleteTypedContainer(source,compatible);result.Check();return SchemaQuerySession.Supported<ISchemaTypedValue>(result);
    }
    private void Check()=>_ = _source.Capture();
    private T Read<T>(Func<T> select)
    {
        T Core(){Check();return select();}
        return _compatible is null?Core():_compatible.Call(_=>Core());
    }
    ISchemaComponentHandle ISchemaTypedValue.GoverningType=>Read(()=>_governing);
    bool ISchemaTypedValue.IsList=>Read(()=>_list);
    IEnumerable<SchemaAtomicItem> ISchemaTypedValue.AtomicItems=>Read<IEnumerable<SchemaAtomicItem>>(()=>this);
    bool ISchemaTypedValue.TryGetSingleAtomic(out SchemaAtomicItem item)
    {
        var result=Read(()=>_items.Length==1?(true,_items[0]):(false,default(SchemaAtomicItem)));
        item=result.Item2;return result.Item1;
    }
    IEnumerator<SchemaAtomicItem> IEnumerable<SchemaAtomicItem>.GetEnumerator()=>Read<IEnumerator<SchemaAtomicItem>>(()=>new Cursor(this));
    IEnumerator IEnumerable.GetEnumerator()=>((IEnumerable<SchemaAtomicItem>)this).GetEnumerator();
    private sealed class Evidence(CompleteTypedContainer owner,int index):ISchemaAtomicProvenance
    {
        ISchemaComponentHandle ISchemaAtomicProvenance.DeclaredType=>owner.Read(()=>owner._cells[index].Declared);
        ISchemaComponentHandle ISchemaAtomicProvenance.ActualType=>owner.Read(()=>owner._cells[index].Actual);
        ISchemaAssessmentOrigin? ISchemaAtomicProvenance.OriginalAssessment=>owner.Read(()=>owner._cells[index].Origin);
    }
    private sealed class Cursor(CompleteTypedContainer owner):IEnumerator<SchemaAtomicItem>
    {
        private int _index=-1;
        private bool _closed;
        private void Check()=>ObjectDisposedException.ThrowIf(_closed,this);
        /// <inheritdoc/>
        public SchemaAtomicItem Current
        {
            get=>owner.Read(()=>{Check();if((uint)_index>=(uint)owner._items.Length)throw new InvalidOperationException("Cursor is not positioned on an item.");return owner._items[_index];});
        }
        object IEnumerator.Current=>Current;
        /// <inheritdoc/>
        public bool MoveNext()=>owner.Read(()=>{Check();if(_index<owner._items.Length)_index++;return _index<owner._items.Length;});
        /// <inheritdoc/>
        public void Reset()=>owner.Read<int>(()=>{Check();throw new NotSupportedException("Reset is unavailable.");});
        /// <inheritdoc/>
        public void Dispose()=>_closed=true;
    }
}
