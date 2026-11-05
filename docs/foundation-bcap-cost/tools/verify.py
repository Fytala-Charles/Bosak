"""Verify raw measurement values, immutable binaries and qualification binding."""
from pathlib import Path
import json,hashlib
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'ENGINE-INPUTS.json').read_text());runs=json.loads((E/'RUNS.json').read_text());summary=json.loads((E/'SUMMARY.json').read_text())
status=json.loads((E/'STATUS.json').read_text());assert status['p03']=='CompleteBoundedCoreFeasibility' and status['nextCoreCoordination']=='P04'
assert not any(status[k] for k in ['productionChanged','engineRebuilt','fullSuiteRerun','bcap01Closed','ownerAccepted','budgetsAccepted','nativeConsumersQualified'])
assert not i['engineRebuild'] and not runs['engineRebuild'] and not runs['budgetsAccepted'] and summary['acceptanceBudgets'] is None
assert i['qualificationManifestSha256']==sha(R/'docs/foundation-bcap-profile/MANIFEST.json') and i['measurementSourceSha256']==sha(P/'tools/MeasurementHost.cs.txt')
assert runs['hostAssemblySha256']==sha(Path(runs['hostAssemblyPath']))
for v in i['engines'].values():assert sha(Path(v['path']))==v['sha256']
assert len(runs['runs'])==15
for r in runs['runs']:
 p=E/r['receipt'];assert sha(p)==r['sha256'];row=json.loads(p.read_text());assert row['passed']
 for v in row['bindings']:assert v['sha256']==(runs['hostAssemblySha256'] if v['name']=='Bosak.XPath.Runtime.Tests' else i['engines'][v['name']]['sha256'])
 if r['profile']!='cancellation':
  d=row['data'];assert len(d['warmMilliseconds'])==80 and d['warmupCaptures']==20 and d['providerReleases']==d['members']
  assert d['warmManagedAllocatedBytes']>0 and d['firstCapture']['managedAllocatedBytes']>0 and all(t>=0 for t in d['warmMilliseconds'])
 else:assert len(row['data']['rows'])==36 and all(v['releaseBeforeOutcome'] and v['completedSecondaryBytes']==5 for v in row['data']['rows'])
if (P/'MANIFEST.json').exists():
 for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/n)==h,n
print('Verified 12 fresh-process cost observations, 960 measured warm captures and 108 controlled cancellation observations on unchanged qualified DLLs; no budgets accepted.')
