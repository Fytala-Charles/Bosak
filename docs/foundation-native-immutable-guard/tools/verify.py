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
old=R/'docs/foundation-native-consumer-handoff'
for name,h in read(old/'MANIFEST.json').items():assert sha(old/name)==h,name
for name in ['NativeNodeProjection.cs','NativeStrictAssessment.cs','NativeProviderSession.cs']:assert (P/'provider'/name).read_bytes()==(old/'provider'/name).read_bytes()
subprocess.run(['python',str(P/'tools/verify_compatibility.py')],check=True)
assert s['consumerTestsPassed']==10 and s['regressionTestsPassed']==59 and s['buildWarnings']==s['buildErrors']==s['requirementsWaived']==0
assert s['immutableOutputOptimizationQualifiedWithinScope'] and s['previousAllocationBlockerResolvedInPrototype'] and s['requestMutationGuardPreserved'] and s['coreChecksUnchanged']
assert not any(s[k] for k in ['productionChanged','publicApiChanged','ownerAccepted','budgetsAccepted','apiFrozen','packagesBuilt','pushPerformed'])
for name,h in read(E/'SOURCE-PINS.json').items():assert sha(P/name)==h,name
consumer=(P/'consumer/PublicConsumerTests.cs').read_text(encoding='utf-8')
assert not re.search(r'NativeContainerAdapter|System\.Reflection|BindingFlags|InternalsVisibleTo|\.Output\b',consumer)
provider='\n'.join(p.read_text(encoding='utf-8') for p in (P/'provider').glob('*.cs'));assert 'InternalsVisibleTo' not in provider
assert 'ProjectReference Include="../provider/Provider.csproj"' in (P/'consumer/Consumer.csproj').read_text(encoding='utf-8')
for folder in ['provider','consumer','workload','phase-probe','regression']:
 for p in (P/folder).glob('*.cs'):
  text=p.read_text(encoding='utf-8');assert text.startswith('// =====') and 'COPYRIGHT            : Fytala' in text and 'SPDX-License-Identifier: Apache-2.0' in text
refs=read(E/'REFERENCES.json');pins=read(R/'docs/foundation-public-primitives/evidence/SOURCE-AND-BINARY-PINS.json');layouts=read(E/'EXECUTION-LAYOUTS.json')
with zipfile.ZipFile(E/'EXECUTION-LAYOUTS.zip') as z:
 for group,layout in layouts.items():
  path=Path(layout['path']);assert {p.relative_to(path).as_posix() for p in path.rglob('*') if p.is_file()}==set(layout['files'])
  for name,h in layout['files'].items():assert sha(path/name)==hashlib.sha256(z.read(group+'/'+name)).hexdigest()==h,(group,name)
  for name,h in refs['engines'].items():assert layout['files'][name+'.dll']==h==pins['engines'][name]['sha256']
  assert layout['files']['Bosak.Schema.dll']==refs['nativeAssembly']['sha256']
assert len({layout['files']['Bosak.Schema.ExperimentalProvider.dll'] for group,layout in layouts.items() if group!='regression'})==1
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
t=read(E/'TEST-RESULTS.json')
for group,count in [('consumer',10),('regression',59)]:
 row=t['lanes'][group];trx=P/row['trx'];assert sha(trx)==row['sha256'];root=ET.parse(trx).getroot();c=root.find('.//{*}Counters').attrib
 assert int(c['passed'])==count and c['failed']==c['notExecuted']=='0'
 assert sorted(x.attrib['testName'] for x in root.findall('.//{*}UnitTestResult'))==row['caseNames'] and len(set(row['caseNames']))==count
after=read(E/'PILOT-METRICS.json');before=read(old/'evidence/PILOT-METRICS.json');delta=read(E/'BEFORE-AFTER.json')
for key in ['iterations','warmup','schemaSha256','instanceSha256','sourceVersion','runtime','architecture']:assert after[key]==before[key]
assert after['iterations']==100 and after['warmup']==10 and not after['ownerAccepted'] and not after['budgetsAccepted']
assert not after['baselineRun'] and after['comparisonRatio'] is None
assert delta['beforeMetricsSha256']==sha(old/'evidence/PILOT-METRICS.json') and delta['afterMetricsSha256']==sha(E/'PILOT-METRICS.json')
assert delta['beforeAllocatedBytesPerCycle']==before['allocatedBytesPerCycle'] and delta['afterAllocatedBytesPerCycle']==after['allocatedBytesPerCycle']
assert after['allocatedBytesPerCycle']<before['allocatedBytesPerCycle']/1000 and delta['sameWorkloadSource']
assert (P/'workload/Program.cs').read_bytes()==(old/'workload/Program.cs').read_bytes()
assert (P/'consumer/PublicConsumerTests.cs').read_bytes()==(old/'consumer/PublicConsumerTests.cs').read_bytes()
for name in ['NativeContainerTests.cs','NativeNodeTests.cs','NativeAssessmentTests.cs']:
 assert (P/'regression'/name).read_bytes()==(R/'docs/foundation-native-assessment-prototype/prototype'/name).read_bytes()
adapter=(P/'provider/NativeContainerAdapter.cs').read_text(encoding='utf-8');assert '_capturedXml' not in adapter
assert 'token.ThrowIfCancellationRequested(); ObjectDisposedException.ThrowIf(!IsAvailable, this);' in adapter
assert 'private readonly XDocument _document;' in adapter
request=(P/'provider/NativeStrictAssessment.cs').read_text(encoding='utf-8');assert '_document.ToString(SaveOptions.DisableFormatting) != _xml' in request
phase=next(x for x in read(E/'PHASE-PROBE.json')['phases'] if x['phase']=='publicDataEvaluate')
previous=next(x for x in read(old/'evidence/PHASE-PROBE.json')['phases'] if x['phase']=='publicDataEvaluate')
assert phase['allocatedBytes']<previous['allocatedBytes']/1000 and 'PV-Q04b' in s['nextSlice']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for name in z.namelist():
  if name.endswith('.log.json'):assert json.loads(z.read(name))['exitCode']==0,name
 for name in ['native-immutable-guard-consumer-build.log','native-immutable-guard-workload-build.log','native-immutable-guard-phase-build.log']:
  log=z.read(name).decode('utf-8-sig');assert '0 Warning(s)' in log and '0 Error(s)' in log
subprocess.run(['python',str(P/'tools/check_docs.py')],check=True)
if (P/'MANIFEST.json').exists():
 manifest=read(P/'MANIFEST.json');assert set(manifest)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json' and not {'bin','obj','__pycache__'}.intersection(p.relative_to(P).parts)}
 for name,h in manifest.items():assert sha(P/name)==h,name
print('Verified TA-50:59 adapter regressions/10 public consumer controls;immutable output guard corrects allocation churn;mutable request guard/Core checks intact. Owner workload/budget/adoption/freeze pending;next PV-Q04b.')
