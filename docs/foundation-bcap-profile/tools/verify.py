"""Verify recorded real-engine feasibility without promoting it to API acceptance."""
from pathlib import Path
import hashlib,json
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'INPUTS.json').read_text());s=json.loads((E/'STATUS.json').read_text())
source=json.loads((E/'SOURCE-INPUTS.json').read_text())
assert i['candidateSources']==source['candidateSources']
for name,digest in source['tests'].items():assert sha(R/name)==digest,name
assert not i['productionChanged'] and not i['nativeConsumersTested'] and i['fullSolutionTestsRun'] and i['frozenGatesRun']
for n,h in i['productionSources'].items():assert sha(R/n)==h,n
for v in i['archives'].values():assert sha(Path(v['path']))==v['sha256']
assert sha(P/'tools/Probe.cs.txt')==i['probeSourceSha256'] and sha(P/'tools/SchemaCaptureCandidate.cs.txt')==i['candidateProducerSourceSha256']
a=json.loads((E/'probe-0.json').read_text());b=json.loads((E/'probe-1.json').read_text())
assert a['passed'] and b['passed'] and a['checks']==b['checks']==83 and a['rows']==b['rows']
pins=json.loads((E/'BINARY-PINS.json').read_text())
for run in [a,b]:
 for binding in run['bindings']:assert pins[binding['name']+'.dll']['sha256']==binding['sha256']
q=json.loads((E/'QUALIFICATION-INPUTS.json').read_text())
for name,v in q['engines'].items():
 if name+'.dll' in pins:assert pins[name+'.dll']['sha256']==v['sha256'],name
full=json.loads((E/'FULL-QUALIFICATION.json').read_text());assert full['unitsPassed']==24390 and full['loadedHashesVerified'] and full['skipMapsByteIdentical']
assert s['p03']=='InProgress' and not s['bcap01Closed'] and len(s['cases'])==10
old=json.loads((R/'docs/foundation-bcap-candidate/evidence/api-0.json').read_text())
new=json.loads((R/'docs/foundation-bcap-outcomes/declarations/evidence/api-0.json').read_text())
assert new==json.loads((R/'docs/foundation-bcap-outcomes/declarations/evidence/api-1.json').read_text())
assert i['reviewDeclarationManifestSha256']==sha(R/'docs/foundation-bcap-outcomes/MANIFEST.json')
assert sum(x['publicTypes'] for x in new['assemblies'])==47
for base,updated in zip(old['assemblies'],new['assemblies']):
 assert base['assembly']==updated['assembly']
 assert set(base['declarations'])<=set(updated['declarations']),base['assembly']
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text())
 for n,h in m.items():assert sha(P/n)==h,n
print('Verified 83 controls twice, 24,390 tests and exact frozen gates on one isolated candidate; public API unchanged and P03/owner acceptance open.')
