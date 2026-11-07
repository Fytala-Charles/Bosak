"""Verify public consumer handoff, candidate-only observations and the unresolved allocation blocker."""
from pathlib import Path
import hashlib,json,re,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
a=read(P/'ADOPTION.json');s=read(P/'STATUS.json');n=Path(a['nativeRoot'])
assert a['recordedBeforeImplementation'] and a['date']==s['date']=='2026-11-07' and s['sourceVersion']=='1.81.0'
for name,h in a['beforeFiles'].items():assert sha(R/name)==h,name
for name,h in a['nativeFiles'].items():assert sha(n/name)==h,name
assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=n,text=True).strip()==a['nativeHead']
assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=n,text=True).strip()==''
old=R/'docs/foundation-native-assessment-prototype'
for name,h in read(old/'MANIFEST.json').items():assert sha(old/name)==h,name
for name in ['NativeContainerAdapter.cs','NativeNodeProjection.cs','NativeStrictAssessment.cs']:assert (P/'provider'/name).read_bytes()==(old/'prototype'/name).read_bytes()
subprocess.run(['python',str(P/'tools/verify_compatibility.py')],check=True)
assert s['testsPassed']==10 and s['buildWarnings']==s['buildErrors']==s['requirementsWaived']==0
assert s['separateConsumerPublicHandoffQualifiedWithinScope'] and s['allocationBlockerObserved']
assert not any(s[k] for k in ['comparableBaselineRun','productionChanged','publicApiChanged','ownerAccepted','budgetsAccepted','apiFrozen','packagesBuilt','pushPerformed'])
for name,h in read(E/'SOURCE-PINS.json').items():assert sha(P/name)==h,name
consumer=(P/'consumer/PublicConsumerTests.cs').read_text(encoding='utf-8')
assert not re.search(r'NativeContainerAdapter|System\.Reflection|BindingFlags|InternalsVisibleTo|\.Output\b',consumer)
provider='\n'.join(p.read_text(encoding='utf-8') for p in (P/'provider').glob('*.cs'));assert 'InternalsVisibleTo' not in provider
assert 'ProjectReference Include="../provider/Provider.csproj"' in (P/'consumer/Consumer.csproj').read_text(encoding='utf-8')
for folder in ['provider','consumer','workload','phase-probe']:
 for p in (P/folder).glob('*.cs'):
  text=p.read_text(encoding='utf-8');assert text.startswith('// =====') and 'COPYRIGHT            : Fytala' in text and 'SPDX-License-Identifier: Apache-2.0' in text
refs=read(E/'REFERENCES.json');pins=read(R/'docs/foundation-public-primitives/evidence/SOURCE-AND-BINARY-PINS.json');layouts=read(E/'EXECUTION-LAYOUTS.json')
with zipfile.ZipFile(E/'EXECUTION-LAYOUTS.zip') as z:
 for group,layout in layouts.items():
  path=Path(layout['path']);assert {p.relative_to(path).as_posix() for p in path.rglob('*') if p.is_file()}==set(layout['files'])
  for name,h in layout['files'].items():assert sha(path/name)==hashlib.sha256(z.read(group+'/'+name)).hexdigest()==h,(group,name)
  for name,h in refs['engines'].items():assert layout['files'][name+'.dll']==h==pins['engines'][name]['sha256']
  assert layout['files']['Bosak.Schema.dll']==refs['nativeAssembly']['sha256']
assert len({layout['files']['Bosak.Schema.ExperimentalProvider.dll'] for layout in layouts.values()})==1
summary=read(E/'BINDING-SUMMARY.json')
for group,folder,assembly in [('consumer','bindings','Bosak.Schema.PublicConsumer.Tests'),('workload','workload-bindings','Bosak.Schema.PublicConsumer.Workload')]:
 seen={};found=False
 for p in (E/folder).glob('*.json'):
  b=read(p);assert not b['errors']
  for row in b['assemblies']:
   name=row['name']
   if name in refs['engines']:
    assert row['sha256']==refs['engines'][name] and row['context']=='Default' and not row['collectible'];seen[name]=row['sha256']
   if name==assembly:
    assert row['sha256']==layouts[group]['files'][name+'.dll'];found=True
 assert found and seen==summary['groups'][group] and {'Bosak.XPath.Api','Bosak.XPath.Core','Bosak.XPath.Runtime'}<=set(seen)
t=read(E/'TEST-RESULTS.json');trx=P/t['trx'];assert sha(trx)==t['sha256'];root=ET.parse(trx).getroot();c=root.find('.//{*}Counters').attrib
assert c['passed']=='10' and c['failed']==c['notExecuted']=='0'
assert sorted(x.attrib['testName'] for x in root.findall('.//{*}UnitTestResult'))==t['caseNames'] and len(set(t['caseNames']))==10
m=read(E/'PILOT-METRICS.json');assert m['iterations']==s['pilotMeasuredCycles']==100 and m['warmup']==10
assert not m['ownerAccepted'] and not m['budgetsAccepted'] and not m['baselineRun'] and m['comparisonRatio'] is None
assert m['allocatedBytesPerCycle']==s['pilotAllocatedBytesPerCycle'] and m['allocatedBytes']/100==m['allocatedBytesPerCycle']
assert m['latencyMs']['min']<=m['latencyMs']['median']<=m['latencyMs']['p95']<=m['latencyMs']['max']
phases=read(E/'PHASE-PROBE.json')['phases'];hot=max(phases,key=lambda x:x['allocatedBytes']);assert hot['phase']==s['allocationHotBoundary']=='publicDataEvaluate'
assert hot['allocatedBytes']>30_000_000_000 and 'PV-Q04a' in s['nextSlice']
w=read(P/'WORKLOAD-READINESS.json');assert len(w['rows'])==9 and w['acceptedBudgets']==w['acceptedScenarios']==0
assert all(not x['accepted'] and x['ownerBudget'] is None for x in w['rows']) and w['allocationBlockerObserved']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for name in z.namelist():
  if name.endswith('.log.json'):assert json.loads(z.read(name))['exitCode']==0,name
 for name in ['native-consumer-handoff-build.log','native-consumer-handoff-workload-build.log','native-consumer-handoff-phase-build.log']:
  log=z.read(name).decode('utf-8-sig');assert '0 Warning(s)' in log and '0 Error(s)' in log
subprocess.run(['python',str(P/'tools/check_docs.py')],check=True)
if (P/'MANIFEST.json').exists():
 manifest=read(P/'MANIFEST.json');assert set(manifest)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json' and not {'bin','obj','__pycache__'}.intersection(p.relative_to(P).parts)}
 for name,h in manifest.items():assert sha(P/name)==h,name
print('Verified TA-49:10 public separate-consumer controls;100-cycle pilot and phase attribution;exact qualified references;31.5GB/cycle allocation blocker remains open. No owner/budget/freeze/publication acceptance;next PV-Q04a.')
