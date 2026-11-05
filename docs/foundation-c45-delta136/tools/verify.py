"""Read-only verification of the full observed inventory and isolated C45 proposal."""
from pathlib import Path
import json,hashlib,sys,collections
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];E=P/'evidence';sha=lambda b:hashlib.sha256(b).hexdigest()
for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha((P/n).read_bytes())==h,n
a=json.loads((E/'public-api-0.json').read_text());assert a==json.loads((E/'public-api-1.json').read_text())==json.loads((E/'public-api.json').read_text())
assert len(a['assemblies'])==11 and sum(v['publicTypes'] for v in a['assemblies'])==270 and sum(v['declarationCount'] for v in a['assemblies'])==5770
assert all('Version=1.36.0.0' in v['identity'] for v in a['assemblies'])
i=json.loads((E/'INPUTS.json').read_text());assert i['engineHashesUnchanged'] and i['inventoryRepeatedIdentically'] and not i['productionSourceChanged'] and not i['runtimeTestsRun']
assert sha((ROOT/'docs/foundation-api-baseline/tools/Program.cs').read_bytes())==i['originalInventorySha256'] and sha((P/'tools/Inventory.cs.txt').read_bytes())==i['guardedInventorySha256']
for v in a['assemblies']:
 if v['assembly']+'.dll' in i['engines']:assert v['sha256']==i['engines'][v['assembly']+'.dll']['sha256'].upper()
if '--check-current' in sys.argv:
 for n,v in i['engines'].items():assert sha(Path(v['path']).read_bytes())==v['sha256']
 for v in a['assemblies']:
  dll=ROOT/Path(v['project']).parent/'bin/Release/net10.0'/(v['assembly']+'.dll');assert sha(dll.read_bytes()).upper()==v['sha256']
def rows(api):return {sha((v['assembly']+'\n'+row).encode()):(v['assembly'],n,row) for v in api['assemblies'] for n,row in enumerate(v['declarations'])}
old=rows(json.loads((ROOT/'docs/foundation-api-baseline/evidence/public-api.json').read_text()));now=rows(a)
assert len(old)==5766 and set(old)<=set(now) and len(set(now)-set(old))==4
cross=json.loads((E/'LEGACY-CROSSWALK.json').read_text());assert not cross['removed'] and cross['preservedIds']==5766 and cross['xmlDbMappedCount']==176
assert {r['id'] for r in cross['added']}==set(now)-set(old) and collections.Counter(r['row'][0] for r in cross['added'])=={'A':2,'M':2}
m=json.loads((E/'member-manifest.json').read_text());entries={v['id']:v for v in m['entries']};assert set(entries)==set(now) and not m['freezeAccepted'] and set(m['ownerDispositions'])=={'Core','Schema','XmlDb','Braid'} and set(m['ownerDispositions'].values())=={'Pending'}
for id,(assembly,index,row) in now.items():assert entries[id]['assembly']==assembly and entries[id]['baselineRow']==index
historical=json.loads((ROOT/'docs/foundation-api-contract-draft/evidence/member-manifest.json').read_text())
for r in historical['entries']:
 assert all(entries[r['id']][k]==r[k] for k in ['reviewGroup','evidenceOnly','interfaceSlot','caseIds','signatureTypes'])
for path,h in m['inputHashes'].items():assert sha((ROOT/path).read_bytes())==h
assert len(cross['xmlDbMappedEntries'])==len({v['id'] for v in cross['xmlDbMappedEntries']})==176 and all(v['id'] in entries for v in cross['xmlDbMappedEntries'])
closure=json.loads((E/'signature-closure.json').read_text());assert all(v['witnessEntryId'] in entries for v in closure['edges'])
cases=json.loads((P/'compatibility-cases.json').read_text());assert len(cases['cases'])==20 and all(v['status']=='NotRun' and v['resultReceipt'] is None for v in cases['cases'])
p=json.loads((E/'proposed-api.json').read_text());delta=json.loads((E/'proposed-member-delta.json').read_text());pi=json.loads((E/'PROPOSAL-INPUTS.json').read_text())
assert len(p['assemblies'])==2 and sum(v['publicTypes'] for v in p['assemblies'])==40 and delta['types']==40 and not delta['accepted'] and not delta['productionSourceChanged'] and delta['existingRequiredInterfaceSlotsAdded']==0 and not delta['productionMemberRemovals']
expected={}
for v in p['assemblies']:
 target='Bosak.XPath.Core' if v['assembly'].startswith('Bosak.XPath.Core.') else 'Bosak.XPath.Api'
 for n,row in enumerate(v['declarations']):expected[sha((target+'\n'+row).encode())]=(target,v['assembly'],n,row)
assert len(expected)==len(delta['entries']) and not set(expected)&set(now)
for v in delta['entries']:
 assert (v['targetAssembly'],v['proposalAssembly'],v['proposalRow'],v['declaration'])==expected[v['id']] and not v['accepted'] and v['implementation']=='NotImplemented' and v['evidenceOnly']==(v['kind'] in ['A','L'])
assert not pi['proposedBodiesExecuted'] and pi['newExportsInProduction']==0
for n,h in pi['sources'].items():assert sha((P/'proposal'/n).read_bytes())==h
assert sha((ROOT/'docs/foundation-boundary-experiment/CORE-CANDIDATE-V5.cs.txt').read_bytes())==pi['v5SourceSha256']
provider=(P/'proposal/Provider.cs.txt').read_text();original=(ROOT/'docs/foundation-boundary-experiment/CORE-CANDIDATE-V5.cs.txt').read_text().replace('namespace C13901.Draft;','namespace Bosak.XPath.Core.Schema;').replace('True if original atomic provenance is attached.','True if current type evidence is available, including construction without an original assessment.')
assert provider==original
mapping=json.loads((E/'REQUIREMENT-MAPPING.json').read_text());assert len(mapping['rows'])==43 and not mapping['accepted']
assert {v['requirementId'] for v in mapping['rows']}=={v['id'] for v in json.loads((ROOT/'docs/foundation-c45-handoff136/MATRIX.json').read_text())['rows']}
assert len(set().union(*(set(v['existingMemberIds']) for v in mapping['rows'])))==176
assert all(set(v['existingMemberIds'])<=set(now) and set(v['proposedMemberIds'])<=set(expected) and v['runtimeCases']=='NotRun' for v in mapping['rows'])
acceptance=json.loads((P/'ACCEPTANCE.json').read_text());assert len(acceptance['cases'])==12 and all(v['status']=='NotRun' and v['receipt'] is None for v in acceptance['cases']) and not acceptance['allPassed']
for name in ['language-server-build.txt','inventory-build.txt','proposal-build.txt']:
 text=(E/name).read_text();assert '0 Warning(s)' in text and '0 Error(s)' in text
r=json.loads((P/'RESULTS.json').read_text());assert r['inventoryEntries']==5770 and r['proposalPublicTypes']==40 and not r['stableManifestAccepted'] and not r['c45Accepted'] and not r['apiFreezeAccepted'] and not r['runtimeTestsRun']
print('PASS:11 assemblies/270 types/5770 entries twice;5766 legacy IDs and176 XmlDb mappings retained; four exact additions;40-type isolated proposal,43 requirement mappings and12 NotRun acceptance cases; no freeze or new product exports.')
