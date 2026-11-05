// ===========================================================================================================================================================
// AUTHOR               : Charles Korthout
// CREATE DATE          : 04 November 2026
// PURPOSE              : Registers typed scalar batches atomically without owning provider resources.
// SPECIAL NOTES        : Part of the register-based virtual machine execution engine.
//
// COPYRIGHT            : Fytala
// LICENSE              : license.md (Apache-2.0)
// SPDX-License-Identifier: Apache-2.0
// ===========================================================================================================================================================
// Change History:      |==================|=======|================|=========================================================================================
//                      |     Author       |Version|  Date          | Notes                                                                                    |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.1   | 04-11-2026     | Creation; transactional typed scalar registry (1.54.0)                                                |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.2   | 04-11-2026     | Attach and validate private resident scalar transport (1.55.0).                           |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.3   | 05-11-2026     | Activate bounded public typed identity with exact ownership (1.58.0).                       |
//                      |==================|=======|================|=========================================================================================
//                      | Charles Korthout | 0.4   | 05-11-2026     | Authorize numeric projection through owned actual type relations (1.59.0).                 |
//                      |==================|=======|================|=========================================================================================
// ===========================================================================================================================================================
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Bosak.XPath.Core.Xdm;
using Bosak.XPath.Core.Schema;

namespace Bosak.XPath.Runtime.Vm;

/// <summary>Snapshot-owned scalar registry; commits run without provider callbacks or eviction.</summary>
internal sealed class QueryTypedScalarCache
{
    private readonly object _gate=new();
    private readonly QuerySourceSession.Snapshot _snapshot;
    private readonly IQueryTypedContainerIssuer _issuer;
    private readonly ISchemaAssessmentInput _input;
    private readonly ISchemaAuthority _authority;
    private readonly int _limit;
    private bool _closed;
    private readonly Dictionary<Key,Entry> _entries=new(KeyComparer.Instance);
    private readonly record struct Key(ISchemaTypedValue Container,ISchemaAssessmentOrigin? Origin,int Ordinal);
    private sealed class KeyComparer : IEqualityComparer<Key>
    {
        internal static readonly KeyComparer Instance=new();
        /// <inheritdoc/>
        public bool Equals(Key x,Key y)=>ReferenceEquals(x.Container,y.Container)&&ReferenceEquals(x.Origin,y.Origin)&&x.Ordinal==y.Ordinal;
        /// <inheritdoc/>
        public int GetHashCode(Key key)=>HashCode.Combine(RuntimeHelpers.GetHashCode(key.Container),key.Origin is null?0:RuntimeHelpers.GetHashCode(key.Origin),key.Ordinal);
    }
    internal sealed class Group
    {
        internal readonly QueryTypedScalarCache Owner;
        internal readonly ISchemaTypedValue Container;
        internal readonly ISchemaAssessmentOrigin? Origin;
        internal readonly ISchemaComponentHandle Governing;
        internal readonly bool List;
        internal readonly ImmutableArray<QueryTypedContainer.Cell> Cells;
        internal readonly Entry[] Entries;
        internal Group(QueryTypedScalarCache owner,ISchemaTypedValue container,ISchemaAssessmentOrigin? origin,ISchemaComponentHandle governing,bool list,ImmutableArray<QueryTypedContainer.Cell> cells)
        {
            Owner=owner;Container=container;Origin=origin;Governing=governing;List=list;Cells=cells;Entries=new Entry[cells.Length];
            for(int i=0;i<Entries.Length;i++)Entries[i]=new Entry(this,i);
        }
    }
    internal sealed class Entry
    {
        internal Group Group {get;}
        internal int Ordinal {get;}
        internal XdmValue Transport {get;}
        internal Entry(Group group,int ordinal)
        {Group=group;Ordinal=ordinal;Transport=XdmValue.SchemaTransportAttach(group.Cells[ordinal].Value,this);}
    }
    internal QueryTypedScalarCache(QuerySourceSession.Snapshot snapshot,IQueryTypedContainerIssuer issuer,ISchemaAssessmentInput input,ISchemaAuthority authority,int limit)
    {ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);_snapshot=snapshot;_issuer=issuer;_input=input;_authority=authority;_limit=limit;}
    internal int Count {get{lock(_gate)return _entries.Count;}}
    internal void RequireScope(IQueryTypedContainerIssuer issuer,ISchemaAssessmentInput input,ISchemaAuthority authority,int limit)
    {
        lock(_gate){ObjectDisposedException.ThrowIf(_closed,this);if(!ReferenceEquals(_issuer,issuer)||!ReferenceEquals(_input,input)||!ReferenceEquals(_authority,authority)||_limit!=limit)throw new ArgumentException("Typed cache scope or scalar ceiling changed.");}
    }
    internal Group? Find(ISchemaTypedValue value,ISchemaAssessmentOrigin? origin)
    {lock(_gate){ObjectDisposedException.ThrowIf(_closed,this);return _entries.TryGetValue(new(value,origin,0),out var entry)?entry.Group:null;}}
    internal Group Install(QuerySourceSession.Operation operation,Group staged,int faultAfter)
    {
        if(!ReferenceEquals(staged.Owner,this))throw new ArgumentException("Foreign registration group.");
        lock(_gate)
        {
            ObjectDisposedException.ThrowIf(_closed,this);operation.CheckRegistrationActive(_snapshot);
            if(_entries.TryGetValue(new(staged.Container,staged.Origin,0),out var winner))return winner.Group;
            if((long)_entries.Count+staged.Entries.Length>_limit)throw new NotSupportedException("Typed scalar evidence capacity exceeded.");
            _entries.EnsureCapacity(checked(_entries.Count+staged.Entries.Length));
            int inserted=0;
            try
            {
                if(faultAfter==0)throw new InvalidOperationException("Injected typed registration fault.");
                for(int i=0;i<staged.Entries.Length;i++)
                {
                    _entries.Add(new(staged.Container,staged.Origin,i),staged.Entries[i]);inserted++;
                    if(faultAfter==inserted)throw new InvalidOperationException("Injected typed registration fault.");
                }
            }
            catch
            {
                for(int i=inserted-1;i>=0;i--)_entries.Remove(new(staged.Container,staged.Origin,i));
                throw;
            }
            return staged;
        }
    }
    internal void Check(Group group)
    {
        lock(_gate)
        {
            ObjectDisposedException.ThrowIf(_closed,this);
            if(!ReferenceEquals(group.Owner,this)||group.Entries.Length!=0&&(!_entries.TryGetValue(new(group.Container,group.Origin,0),out var first)||!ReferenceEquals(first,group.Entries[0])))throw new ArgumentException("Unregistered scalar group.");
        }
    }
    internal QueryRegisteredTypedContainer ValidateTransport(QuerySourceSession.Operation operation,XdmValue value,int maxAtoms,int digits,int scale,CancellationToken cancellation)
    {
        operation.RequireMember(_snapshot);
        if(XdmValue.SchemaTransportMarker(value) is not Entry entry||!ReferenceEquals(entry.Group.Owner,this))throw new ArgumentException("Foreign or missing typed scalar evidence.",nameof(value));
        var group=entry.Group;
        Check(group);
        if((uint)entry.Ordinal>=(uint)group.Entries.Length||!ReferenceEquals(group.Entries[entry.Ordinal],entry)
            ||!value.SchemaTransportMatches(group.Cells[entry.Ordinal].Value))throw new ArgumentException("Typed scalar representation or registration changed.",nameof(value));
        var reader=new QueryTypedContainerReader(operation,_snapshot,_snapshot.Owner.Environment,_issuer,_input,group.Origin,group.Container,maxAtoms,digits,scale,cancellation);
        RequireScope(_issuer,_input,reader.SelectedAuthority,_limit);
        var result=new QueryRegisteredTypedContainer(this,reader,group);result.Check();return result;
    }
    internal void Close(){lock(_gate){_closed=true;_entries.Clear();}}
    internal bool ContainsTransport(XdmValue value)
    {
        if(XdmValue.SchemaTransportMarker(value) is not Entry entry||!ReferenceEquals(entry.Group.Owner,this))return false;
        lock(_gate)return !_closed&&_entries.TryGetValue(new(entry.Group.Container,entry.Group.Origin,entry.Ordinal),out var stored)&&ReferenceEquals(stored,entry);
    }
}

/// <summary>Private staged registration with a fixed insertion-fault test seam and no provider callbacks under commit.</summary>
internal sealed class QueryTypedScalarRegistration
{
    private readonly QueryTypedScalarCache _cache;
    private readonly QuerySourceSession.Operation _operation;
    private readonly QuerySourceSession.Snapshot _snapshot;
    private readonly QueryTypedContainerReader _reader;
    private readonly QueryTypedScalarCache.Group _group;
    internal QueryTypedScalarRegistration(QueryTypedScalarCache cache,QuerySourceSession.Operation operation,QuerySourceSession.Snapshot snapshot,QueryTypedContainerReader reader,QueryTypedScalarCache.Group group)
    {_cache=cache;_operation=operation;_snapshot=snapshot;_reader=reader;_group=group;}
    private void CheckProvider(){_operation.RequireMember(_snapshot);_reader.ValidateStored(_group.Governing,_group.Cells);}
    internal QueryRegisteredTypedContainer Commit(int faultAfter=-1)
    {
        CheckProvider();
        // Allocate the delivery object before cache mutation. The only locked fault seam is a fixed integer, never a callback.
        var result=new QueryRegisteredTypedContainer(_cache,_reader,_group);
        var committed=_cache.Install(_operation,_group,faultAfter);result.Select(committed);
        result.Check();return result;
    }
    internal bool TryCommitSingle(out XdmValue value)
    {
        CheckProvider();value=default;
        if(_group.Cells.Length!=1)return false;
        value=Commit().GetTransport(0);return true;
    }
}

/// <summary>Current-operation view over stable resident scalar identities; payloads are still plain and unattached.</summary>
internal sealed class QueryRegisteredTypedContainer
{
    private readonly QueryTypedScalarCache _cache;
    private readonly QueryTypedContainerReader _reader;
    private QueryTypedScalarCache.Group _group;
    internal QueryRegisteredTypedContainer(QueryTypedScalarCache cache,QueryTypedContainerReader reader,QueryTypedScalarCache.Group group)
    {_cache=cache;_reader=reader;_group=group;}
    internal void Select(QueryTypedScalarCache.Group group)=>_group=group;
    internal void Check(){_cache.Check(_group);_reader.ValidateStored(_group.Governing,_group.Cells);_cache.Check(_group);}
    internal int Count {get{Check();return _group.Cells.Length;}}
    internal bool IsList {get{Check();return _group.List;}}
    internal ISchemaComponentHandle GoverningType {get{Check();return _group.Governing;}}
    internal AtomicView GetAtomic(int index){Check();if((uint)index>=(uint)_group.Cells.Length)throw new ArgumentOutOfRangeException(nameof(index));return new(this,index);}
    internal readonly struct AtomicView(QueryRegisteredTypedContainer owner,int index)
    {
        private QueryTypedContainer.Cell Read(){if(owner is null)throw new InvalidOperationException("No registered atomic view.");owner.Check();return owner._group.Cells[index];}
        internal XdmValue Value=>Read().Value;
        internal ISchemaComponentHandle DeclaredType=>Read().Declared;
        internal ISchemaComponentHandle ActualType=>Read().Actual;
        internal ISchemaAssessmentOrigin? OriginalAssessment=>Read().Origin;
    }
    internal object GetIdentity(int index){Check();return _group.Entries[index];}
    internal XdmValue GetTransport(int index){Check();return _group.Entries[index].Transport;}
    internal XdmValue ProjectNumeric(XdmValue value)
    {
        Check();
        if(XdmValue.SchemaTransportMarker(value) is not QueryTypedScalarCache.Entry entry||!ReferenceEquals(entry.Group,_group))throw new ArgumentException("Foreign numeric evidence.");
        var result=_reader.ProjectNumeric(_group.Cells[entry.Ordinal]);Check();return result;
    }
}
