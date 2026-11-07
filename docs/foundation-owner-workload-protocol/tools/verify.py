"""Verify proposed workload methods and candidate observations without accepting owner gates."""
from pathlib import Path
import hashlib,json,subprocess,zipfile
from compare import validate
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
a=read(P/'ADOPTION.json');s=read(P/'STATUS.json');n=Path(a['nativeRoot'])
assert a['recordedBeforeImplementation'] and s['sourceVersion']=='1.81.0'
for name,h in a['beforeFiles'].items():assert sha(R/name)==h,name
for name,h in a['nativeFiles'].items():assert sha(n/name)==h,name
assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=n,text=True).strip()==a['nativeHead']
assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=n,text=True).strip()==''
old=R/'docs/foundation-native-immutable-guard'
for name,h in read(old/'MANIFEST.json').items():assert sha(old/name)==h,name
subprocess.run(['python',str(P/'tools/verify_compatibility.py')],check=True)
assert s['protocolPreparedWithinScope'] and s['comparisonControlsPassed']==14 and s['buildWarnings']==s['buildErrors']==s['requirementsWaived']==0
assert not any(s[k] for k in ['baselineSelected','ownerAccepted','budgetsAccepted','productionChanged','publicApiChanged','apiFrozen','packagesBuilt','pushPerformed'])
for name,h in read(E/'SOURCE-PINS.json').items():assert sha(P/name)==h,name
p=read(P/'PROTOCOL.json');m=read(E/'MEASUREMENTS.json');validate(p,m,sha(P/'PROTOCOL.json'))
assert not p['ownerAccepted'] and p['ownerBudget'] is None and not p['baselineSelected'] and p['comparableBaseline'] is None
assert not m['baselineRun'] and m['comparisonRatio'] is None and m['role']=='candidate'
assert len(m['rows'])==s['independentSessionMeasurements']==6
assert sum(r['iterations'] for r in m['rows'])==s['measuredCycles']==30
assert sum(r['iterations']*r['children'] for r in m['rows'])==s['measuredDecimalReads']==135
assert sha(P/p['schema'])==p['schemaSha256']
for row in p['workloads']:assert sha(P/row['instance'])==row['instanceSha256']
assert read(P/'BASELINE-ELIGIBILITY.json')['selectedBaseline'] is None
budget=read(P/'BUDGET-PROPOSAL.json');assert len(budget['scenarios'])==9 and all(r['ownerBudget'] is None and not r['ownerWorkloadAccepted'] and not r['baselineAccepted'] and not r['candidateAccepted'] for r in budget['scenarios'])
assert all(r['threshold'] is None for r in budget['suggestedBudgetFields'])
refs=read(E/'REFERENCES.json');layout=read(E/'EXECUTION-LAYOUT.json');output=Path(layout['path']);pins=read(R/'docs/foundation-public-primitives/evidence/SOURCE-AND-BINARY-PINS.json')
assert {p.relative_to(output).as_posix() for p in output.rglob('*') if p.is_file()}==set(layout['files'])
with zipfile.ZipFile(E/'EXECUTION-LAYOUT.zip') as z:
 assert set(z.namelist())==set(layout['files'])
 for name,h in layout['files'].items():assert sha(output/name)==hashlib.sha256(z.read(name)).hexdigest()==h,name
for name,h in refs['engines'].items():assert layout['files'][name+'.dll']==h==pins['engines'][name]['sha256']
assert layout['files']['Bosak.Schema.dll']==refs['nativeAssembly']['sha256']
assert sha(Path(refs['providerAssembly']['path']))==layout['files']['Bosak.Schema.ExperimentalProvider.dll']==refs['providerAssembly']['sha256']
seen=set();runner=False
for f in (E/'bindings').glob('*.json'):
 b=read(f);assert not b['errors']
 for row in b['assemblies']:
  name=row['name']
  if name in refs['engines']:
   assert row['sha256']==refs['engines'][name] and row['context']=='Default' and not row['collectible'];seen.add(name)
  if name=='Bosak.Schema.OwnerWorkload.Protocol':assert row['sha256']==layout['files'][name+'.dll'];runner=True
assert runner and {'Bosak.XPath.Api','Bosak.XPath.Core','Bosak.XPath.Runtime'}<=seen
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for name in z.namelist():
  if name.endswith('.log.json'):assert json.loads(z.read(name))['exitCode']==(2 if name=='owner-workload-protocol-baseline-refusal.log.json' else 0),name
 controls=z.read('owner-workload-protocol-controls.log').decode('utf-8-sig');assert 'Ran 14 tests' in controls and 'OK' in controls
 build=z.read('owner-workload-protocol-build.log').decode('utf-8-sig');assert '0 Warning(s)' in build and '0 Error(s)' in build
 refusal=z.read('owner-workload-protocol-baseline-refusal.log').decode('utf-8-sig');assert 'candidate run cannot masquerade' in refusal
subprocess.run(['python',str(P/'tools/check_docs.py')],check=True)
if (P/'MANIFEST.json').exists():
 manifest=read(P/'MANIFEST.json');assert set(manifest)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json' and not {'bin','obj','__pycache__'}.intersection(p.relative_to(P).parts)}
 for name,h in manifest.items():assert sha(P/name)==h,name
print('Verified TA-51:6 candidate sessions/30 cycles/135 reads;14 comparison controls;candidate-as-baseline refusal;exact unchanged provider/Core pins. No selected baseline/accepted budgets/adoption/freeze;next PV-Q04c.')
