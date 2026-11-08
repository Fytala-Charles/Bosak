"""Verify diagnostic callback attribution without granting semantic/performance acceptance."""
from pathlib import Path
import hashlib,json,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
a=read(P/'ADOPTION.json');s=read(P/'STATUS.json');n=Path(a['nativeRoot'])
assert a['recordedBeforeImplementation'] and s['sourceVersion']=='1.81.0'
for name,h in a['beforeFiles'].items():assert sha(R/name)==h,name
for name,h in a['nativeFiles'].items():assert sha(n/name)==h,name
assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=n,text=True).strip()==a['nativeHead']
assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=n,text=True).strip()==''
for old in [R/'docs/foundation-native-immutable-guard',R/'docs/foundation-owner-workload-protocol']:
 for name,h in read(old/'MANIFEST.json').items():assert sha(old/name)==h,name
subprocess.run(['python',str(P/'tools/verify_compatibility.py')],check=True)
assert s['attributionCompleteWithinScope'] and s['regressionTestsPassed']==59 and s['probePhases']==4
assert s['buildWarnings']==s['buildErrors']==s['requirementsWaived']==0
assert not any(s[k] for k in ['coreChanged','checksElided','productionChanged','publicApiChanged','exclusiveCpuProfileClaimed','ownerAccepted','budgetsAccepted','apiFrozen','packagesBuilt','pushPerformed'])
for name,h in read(E/'SOURCE-PINS.json').items():assert sha(P/name)==h,name
for name,h in read(E/'CALL-PATH.json')['reviewPins'].items():assert sha(R/name)==h,name
old=R/'docs/foundation-native-immutable-guard'
for name in ['NativeStrictAssessment.cs','NativeProviderSession.cs']:assert (P/'provider'/name).read_bytes()==(old/'provider'/name).read_bytes()
for name in ['NativeContainerTests.cs','NativeNodeTests.cs','NativeAssessmentTests.cs']:assert (P/'regression'/name).read_bytes()==(old/'regression'/name).read_bytes()
counts=read(E/'CALLBACK-COUNTS.json');assert not counts['coreChanged'] and not counts['checksElided'] and len(counts['rows'])==4
rows={(r['children'],r['phase']):r for r in counts['rows']};assert set(rows)=={(n,phase) for n in [1,8] for phase in ['firstData','cachedData']}
for (children,phase),row in rows.items():
 c=row['counters'];assert row['sum']==children*1.25 and all(type(v) is int and v>=0 for v in c.values())
 for name in ['Facts.Type','Facts.Declaration','Facts.Origin','Facts.HasNoTypedValue','Facts.IsNilled','Facts.ReadTypedValue']:assert c[name]==children
 assert c['Advisor.Assessor']==c['Advisor.Environment']==c['Assessor.Authority']
assert rows[(1,'firstData')]['counters']['Output.Availability']==2223750
assert rows[(1,'cachedData')]['counters']['Output.Availability']==1573818
assert rows[(1,'firstData')]['counters']['Advisor.Assessor']==242845
refs=read(E/'REFERENCES.json');layouts=read(E/'EXECUTION-LAYOUTS.json');pins=read(R/'docs/foundation-public-primitives/evidence/SOURCE-AND-BINARY-PINS.json')
with zipfile.ZipFile(E/'EXECUTION-LAYOUTS.zip') as z:
 for group,layout in layouts.items():
  output=Path(layout['path']);assert {p.relative_to(output).as_posix() for p in output.rglob('*') if p.is_file()}==set(layout['files'])
  for name,h in layout['files'].items():assert sha(output/name)==hashlib.sha256(z.read(group+'/'+name)).hexdigest()==h,(group,name)
  for name,h in refs['engines'].items():assert layout['files'][name+'.dll']==h==pins['engines'][name]['sha256']
  assert layout['files']['Bosak.Schema.dll']==refs['nativeAssembly']['sha256']
seen=set();probe=False;provider=False
for p in (E/'bindings').glob('*.json'):
 b=read(p);assert not b['errors']
 for row in b['assemblies']:
  name=row['name'];assert row['context']=='Default' and not row['collectible']
  if name in refs['engines']:assert row['sha256']==refs['engines'][name];seen.add(name)
  if name=='Bosak.Schema.PublicData.CallbackProbe':assert row['sha256']==layouts['probe']['files'][name+'.dll'];probe=True
  if name=='Bosak.Schema.ExperimentalProvider':assert row['sha256']==layouts['probe']['files'][name+'.dll'];provider=True
assert probe and provider and {'Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime'}<=seen
t=read(E/'REGRESSION-RESULTS.json');trx=P/t['trx'];assert sha(trx)==t['sha256'];root=ET.parse(trx).getroot();c=root.find('.//{*}Counters').attrib
assert c['passed']=='59' and c['failed']==c['notExecuted']=='0'
assert sorted(x.attrib['testName'] for x in root.findall('.//{*}UnitTestResult'))==t['caseNames'] and len(set(t['caseNames']))==59
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for name in z.namelist():
  if name.endswith('.log.json'):assert json.loads(z.read(name))['exitCode']==0,name
 for name in ['public-data-callback-regression-build.log','public-data-callback-probe-build.log']:
  log=z.read(name).decode('utf-8-sig');assert '0 Warning(s)' in log and '0 Error(s)' in log
assert not read(P/'NEXT-STEP.json')['coreOptimizationSelected']
subprocess.run(['python',str(P/'tools/check_docs.py')],check=True)
if (P/'MANIFEST.json').exists():
 manifest=read(P/'MANIFEST.json');assert set(manifest)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json' and not {'bin','obj','__pycache__'}.intersection(p.relative_to(P).parts)}
 for name,h in manifest.items():assert sha(P/name)==h,name
print('Verified TA-52:59 instrumented regressions/four exact counter phases;2.22m output checks for one first atomization;Core checks unchanged. Bounded policy/full qualification required before engine optimization;next PV-Q04d.')
