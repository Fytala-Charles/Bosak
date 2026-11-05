"""Finite reference model for TA-02 accounting/commit policy; not runtime admission code."""
from dataclasses import dataclass
from threading import RLock
from random import Random

class Refusal(Exception):pass
class Cancelled(Exception):pass

@dataclass(eq=False,frozen=True)
class Container:
    label:str
    count:int
    is_list:bool=True

@dataclass(eq=False)
class Entry:
    scope:object
    container:Container
    ordinal:int
    origin:object
    # Synthetic result of an already-authorized actual-type relation, not an incoming type-name claim.
    family:str='integer'
    value:int=1

@dataclass(eq=False)
class Transaction:
    scope:object
    container:Container
    origin:object
    cells:tuple

class Operation:
    def __init__(self,scope):self.scope=scope;self.open=True

class Cache:
    def __init__(self,limit,atoms,scope=None):
        if limit<=0 or atoms<=0:raise ValueError('positive limits')
        self.limit=limit;self.atoms=atoms;self.scope=scope or object();self.open=True
        self.entries={};self.owned={};self.lock=RLock();self.payload_reads=0
        # owned is the test issuer witness table, not a proposed resident Core container index.
    def issue(self,container,origin):self.owned[(id(container),id(origin))]=(container,origin)
    def _owns(self,container,origin):
        pair=self.owned.get((id(container),id(origin)))
        return pair is not None and pair[0] is container and pair[1] is origin
    def stage(self,container,origin,family='integer',value=1):
        if not self.open or not self._owns(container,origin):raise Refusal('ownership')
        if container.count<0 or container.count>self.atoms:raise Refusal('atom-capacity')
        # Entries retain container/origin references, so id-based reference keys cannot be recycled while resident.
        return Transaction(self.scope,container,origin,tuple(Entry(self.scope,container,i,origin,family,value) for i in range(container.count)))
    def commit(self,tx,fail_after=None,cancelled=False):
        # Models a locked, callback-free commit after actual provider/operation validation outside this lock.
        if not self._owns(tx.container,tx.origin):raise Refusal('ownership before commit')
        with self.lock:
            if not self.open or tx.scope is not self.scope:raise Refusal('scope')
            if cancelled:raise Cancelled('before commit')
            keys=[(id(tx.container),i,id(tx.origin)) for i in range(len(tx.cells))]
            missing=[(key,cell) for key,cell in zip(keys,tx.cells) if key not in self.entries]
            if len(self.entries)+len(missing)>self.limit:raise Refusal('scalar-capacity')
            undo=[key for key,_ in missing];attempted=0
            try:
                if fail_after==0:raise MemoryError('staged allocation fault')
                for key,cell in missing:
                    attempted+=1;self.entries[key]=cell
                    if fail_after==attempted:raise MemoryError('internal commit fault')
            except BaseException:
                for index in range(attempted-1,-1,-1):self.entries.pop(undo[index],None)
                raise
            return tuple(self.entries[key] for key in keys)
    def read(self,entry,op):
        if not self.open or not op.open or op.scope is not self.scope or entry.scope is not self.scope:raise Refusal('lifetime/scope')
        if not self._owns(entry.container,entry.origin):raise Refusal('ownership')
        key=(id(entry.container),entry.ordinal,id(entry.origin))
        if self.entries.get(key) is not entry:raise Refusal('unregistered')
        return entry
    def close(self):
        with self.lock:self.open=False;self.entries.clear();self.owned.clear()

def project(cache,entry,op,mode='identity',plain_profile=False):
    if plain_profile:raise Refusal('plain profile rejects markers')
    admitted=cache.read(entry,op)
    if mode=='identity':return admitted
    if mode not in ['add-zero','compare-one']:raise Refusal('typed constructor/cast route not selected')
    if admitted.family not in ['integer','decimal']:raise Refusal('unsupported actual numeric relation')
    cache.payload_reads+=1
    return {'value':admitted.value if mode=='add-zero' else admitted.value==1,'kind':admitted.family if mode=='add-zero' else 'boolean','evidence':None,'origin':None}

def expect(error,action):
    try:action()
    except error:return
    raise AssertionError('expected '+error.__name__)

def controls():
    # Empty/singleton/multiple lists count scalar cells, never independent container registrations.
    accounting=0
    for count in [0,1,2,3]:
        for limit in [1,2,3]:
            c=Cache(limit,3);v=Container('value',count);origin=object();c.issue(v,origin);tx=c.stage(v,origin)
            if count>limit:expect(Refusal,lambda:c.commit(tx));assert not c.entries
            else:
                first=c.commit(tx);again=c.commit(tx);assert len(c.entries)==count and all(a is b for a,b in zip(first,again))
            accounting+=1
    c=Cache(1,1)
    for _ in range(1000):
        v=Container('empty',0);c.issue(v,None);assert c.commit(c.stage(v,None))==();del c.owned[(id(v),id(None))]
    assert not c.entries
    # Same labels and payload values are not cache identity; distinct origins/scopes do not alias.
    c=Cache(4,4);origin=object();a=Container('same',1);b=Container('same',1)
    c.issue(a,origin);c.issue(b,origin);ea=c.commit(c.stage(a,origin))[0];eb=c.commit(c.stage(b,origin))[0];assert ea is not eb
    expect(Refusal,lambda:c.stage(Container('same',1),origin))
    c.issue(a,None);en=c.commit(c.stage(a,None))[0];assert en is not ea
    other=Cache(4,4);other.issue(a,origin);eo=other.commit(other.stage(a,origin))[0];expect(Refusal,lambda:other.read(ea,Operation(other.scope)));assert eo is not ea
    # Faults roll back only newly inserted entries, preserving prior evidence and all quota capacity.
    faults=0
    for point in [0,1,2,3]:
        c=Cache(4,4);old=Container('old',1);new=Container('new',3);c.issue(old,None);c.issue(new,None);prior=c.commit(c.stage(old,None))[0];tx=c.stage(new,None)
        expect(MemoryError,lambda:c.commit(tx,fail_after=point));assert list(c.entries.values())==[prior]
        assert len(c.commit(tx))==3 and len(c.entries)==4;faults+=1
    # Interleaved preparations recheck capacity at commit and reuse a same-container winner.
    c=Cache(3,3);a=Container('a',2);b=Container('b',2);c.issue(a,None);c.issue(b,None);ta=c.stage(a,None);tb=c.stage(b,None);c.commit(ta)
    expect(Refusal,lambda:c.commit(tb));assert len(c.entries)==2
    c=Cache(2,2);c.issue(a,None);ta=c.stage(a,None);tb=c.stage(a,None);first=c.commit(ta);second=c.commit(tb);assert all(a is b for a,b in zip(first,second)) and len(c.entries)==2
    # Cancel before commit changes nothing; cancellation after commit/during delivery leaves a complete batch.
    c=Cache(2,2);v=Container('v',2);c.issue(v,None);tx=c.stage(v,None);expect(Cancelled,lambda:c.commit(tx,cancelled=True));assert not c.entries
    batch=c.commit(tx);expect(Cancelled,lambda:(_ for _ in ()).throw(Cancelled('delivery')));assert len(c.entries)==2 and all(a is b for a,b in zip(batch,c.commit(tx)))
    c.close();expect(Refusal,lambda:c.commit(tx));assert not c.entries
    c=Cache(2,2);v=Container('revoked',2);c.issue(v,None);tx=c.stage(v,None);del c.owned[(id(v),id(None))]
    expect(Refusal,lambda:c.commit(tx));assert not c.entries
    # Authorized identity retains its marker; computations do not manufacture provenance or assessment.
    projection=0
    for family in ['integer','decimal','other']:
        c=Cache(2,2);v=Container('v',1,False);origin=object();c.issue(v,origin);entry=c.commit(c.stage(v,origin,family))[0];op=Operation(c.scope)
        assert project(c,entry,op) is entry
        if family=='other':expect(Refusal,lambda:project(c,entry,op,'add-zero'));assert c.payload_reads==0
        else:
            for mode in ['add-zero','compare-one']:
                value=project(c,entry,op,mode);assert value['evidence'] is None and value['origin'] is None;projection+=1
        for mode in ['constructor','cast']:
            reads=c.payload_reads;expect(Refusal,lambda:project(c,entry,op,mode));assert reads==c.payload_reads
        reads=c.payload_reads;expect(Refusal,lambda:project(c,entry,op,plain_profile=True));assert reads==c.payload_reads
        op.open=False;expect(Refusal,lambda:project(c,entry,op));op=Operation(c.scope);del c.owned[(id(v),id(origin))];expect(Refusal,lambda:project(c,entry,op));c.close()
    # Seeded mixed operations validate bounded occupancy under successful and failed admissions.
    random=Random(15302);scenarios=1000
    for _ in range(scenarios):
        c=Cache(random.randrange(1,9),8);pool=[]
        for _ in range(30):
            if pool and random.randrange(3)==0:v=random.choice(pool)
            else:v=Container('equal-label',random.randrange(9));pool.append(v);c.issue(v,None)
            tx=c.stage(v,None);before=dict(c.entries)
            try:c.commit(tx,fail_after=0 if random.randrange(10)==0 else None,cancelled=random.randrange(10)==0)
            except (Refusal,Cancelled,MemoryError):assert c.entries==before
            assert len(c.entries)<=c.limit
    return {'accountingCases':accounting,'emptyContainerIterations':1000,'rollbackFaultPoints':faults,'interleavingCases':2,'cancelCommitDeliveryCases':3,'precommitRevocationCases':1,'numericComputedCases':projection,'numericFamiliesChecked':3,'seededScenarios':scenarios,'operationsPerScenario':30,'runtimeConcurrencyTested':False,'productionImplemented':False,'budgetsAccepted':False}
