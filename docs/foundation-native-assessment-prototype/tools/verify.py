"""Verify the isolated native preassessed-container prototype and retained qualification."""
from pathlib import Path
import hashlib,json,subprocess,xml.etree.ElementTree as ET,zipfile
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
a=read(P/'ADOPTION.json');s=read(P/'STATUS.json');n=Path(a['nativeRoot'])
assert a['recordedBeforeImplementation'] and a['sourceVersion']==s['sourceVersion']=='1.81.0'
assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=n,text=True).strip()==a['nativeHead']
assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=n,text=True).strip()==''
for name,h in a['beforeFiles'].items():assert sha(R/name)==h,name
for name,h in a['nativeFiles'].items():assert sha(n/name)==h,name
prior=R/'docs/foundation-native-node-prototype'
for name,h in read(prior/'MANIFEST.json').items():assert sha(prior/name)==h,name
subprocess.run(['python',str(P/'tools/verify_compatibility.py')],check=True)
assert s['publicDocumentAssessmentImplemented'] and s['publicNodeFactsProjectionImplemented'] and s['nativeContainerPublicTransportQualifiedWithinScope'] and s['testsPassed']==59
assert s['buildWarnings']==s['buildErrors']==s['requirementsWaived']==0
assert not any(s[k] for k in ['productionChanged','publicApiChanged','freshCoreFullSuiteRun','native451SuiteRerun','ownerAccepted','budgetsAccepted','apiFrozen','packagesBuilt','pushPerformed'])
for name,h in read(E/'PROTOTYPE-SOURCE-PINS.json').items():assert sha(P/name)==h,name
refs=read(E/'REFERENCES.json');pins=read(R/'docs/foundation-public-primitives/evidence/SOURCE-AND-BINARY-PINS.json')
assert len(refs['engines'])==9
for name,h in refs['engines'].items():assert h==pins['engines'][name]['sha256']
assert sha(Path(refs['nativeAssembly']['path']))==refs['nativeAssembly']['sha256']
layout=read(E/'EXECUTION-LAYOUT.json');output=Path(layout['path'])
assert {p.relative_to(output).as_posix() for p in output.rglob('*') if p.is_file()}==set(layout['files'])
with zipfile.ZipFile(E/'EXECUTION-LAYOUT.zip') as z:
 assert set(z.namelist())==set(layout['files'])
 for name,h in layout['files'].items():assert sha(output/name)==hashlib.sha256(z.read(name)).hexdigest()==h,name
for name,h in refs['engines'].items():assert layout['files'][name+'.dll']==h
assert layout['files']['Bosak.Schema.dll']==refs['nativeAssembly']['sha256']
t=read(E/'TEST-RESULTS.json');trx=P/t['trx'];assert sha(trx)==t['sha256']
root=ET.parse(trx).getroot();counts=root.find('.//{*}Counters').attrib
assert counts['passed']=='59' and counts['failed']==counts['notExecuted']=='0'
names=sorted(x.attrib['testName'] for x in root.findall('.//{*}UnitTestResult'))
assert names==t['caseNames'] and len(set(names))==59 and t['countsOverlapNotAdditive']
seen={}
consumer_loaded=False
for f in (E/'final-bindings').glob('*.json'):
 b=read(f);assert not b['errors']
 for row in b['assemblies']:
  name=row['name']
  if name=='Bosak.Schema.Assessment.Prototype.Tests':
   assert row['sha256']==layout['files'][name+'.dll'] and row['context']=='Default' and not row['collectible'];consumer_loaded=True
  if name in refs['engines']:
   assert row['sha256']==refs['engines'][name] and row['context']=='Default' and not row['collectible'];seen[name]=row['sha256']
summary=read(E/'BINDING-SUMMARY.json');assert seen==summary['loadedQualifiedEngines'] and len(seen)==summary['loadedCount']
assert {'Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime'}<=set(seen)
assert consumer_loaded
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for name in z.namelist():
  if name.endswith('.log.json'):
   row=json.loads(z.read(name));assert row['exitCode']==(1 if name=='native-assessment-prototype-tests.log.json' else 0),name
 log=z.read('native-assessment-prototype-build-final.log').decode('utf-8-sig');assert '0 Warning(s)' in log and '0 Error(s)' in log
subprocess.run(['python',str(P/'tools/check_docs.py')],check=True)
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');actual={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json' and not {'bin','obj','__pycache__'}.intersection(p.relative_to(P).parts)}
 assert actual==set(m)
 for name,h in m.items():assert sha(P/name)==h,name
print('Verified TA-48:59 isolated provider-owned strict request/container/node controls;exact qualified references/observed loads;Core/native originals unchanged. Generic handoff/owner/budgets/freeze pending;next PV-Q03d.')
