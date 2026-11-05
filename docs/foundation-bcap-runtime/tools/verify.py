"""Verify recorded real-engine feasibility without promoting it to API acceptance."""
from pathlib import Path
import hashlib,json
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'INPUTS.json').read_text());s=json.loads((E/'STATUS.json').read_text())
assert not i['productionChanged'] and not i['nativeConsumersTested'] and not i['fullSolutionTestsRun'] and not i['frozenGatesRun']
for n,h in i['productionSources'].items():assert sha(R/n)==h,n
for v in i['archives'].values():assert sha(Path(v['path']))==v['sha256']
assert sha(P/'tools/Probe.cs.txt')==i['probeSourceSha256'] and sha(P/'tools/SchemaCaptureCandidate.cs.txt')==i['candidateProducerSourceSha256']
a=json.loads((E/'probe-0.json').read_text());b=json.loads((E/'probe-1.json').read_text())
assert a['passed'] and b['passed'] and a['checks']==b['checks']==26 and a['rows']==b['rows']
pins=json.loads((E/'BINARY-PINS.json').read_text())
for run in [a,b]:
 for binding in run['bindings']:assert pins[binding['name']+'.dll']['sha256']==binding['sha256']
for row in json.loads((E/'REGRESSIONS.json').read_text()):
 assert int(row['counters']['passed']) in [366,265] and row['counters']['failed']=='0'
 for n,h in row['candidateBinaries'].items():
  if n in pins:assert pins[n]['sha256']==h,n
assert s['p03']=='InProgress' and not s['bcap01Closed'] and len(s['cases'])==10
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text())
 for n,h in m.items():assert sha(P/n)==h,n
print('Verified 26 real capture checks twice, 631 existing regressions, exact binary/source pins; production unchanged and P03 still open.')
