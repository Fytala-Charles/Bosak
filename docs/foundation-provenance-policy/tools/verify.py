"""Verify policy design, exact unchanged candidate and existing-route characterization without new-helper claims."""
from pathlib import Path
import json,hashlib,zipfile,re,subprocess,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';Q=R/'docs/foundation-public-xslt'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
s=read(P/'STATUS.json');a=read(P/'ADOPTION.json');policy=read(P/'POLICY.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json')
assert s['sourceVersion']==a['sourceVersion']==pins['sourceVersion']=='1.76.0'
assert s['policySelected'] and s['policyDecisionCount']==22 and s['plannedCases']==37 and s['existingCharacterizationTestsPassed']==475
assert not any(s[k] for k in ['productionChanged','publicApiChanged','freshFullRun','publicHelperActivated','pv01Complete','schemaReviewAccepted','ownerAccepted','apiFrozen','publicationAuthorized','packagesBuilt','pushPerformed'])
assert s['newTestsPassed']==s['requirementsWaived']==0
assert a['policyBeforeImplementation'] and not a['productionChanged']
for n,h in a['beforeHashes'].items():assert sha(R/n)==h,n
for n,h in read(Q/'MANIFEST.json').items():assert sha(Q/n)==h,n
assert (E/'SOURCE-AND-BINARY-PINS.json').read_bytes()==(Q/'evidence/SOURCE-AND-BINARY-PINS.json').read_bytes()
for n,h in pins['sources'].items():assert sha(R/n)==h,n
assert not any('public static class SchemaAtomics' in (R/n).read_text(encoding='utf-8') for n in pins['sources'] if n.endswith('.cs'))
for n,row in pins['engines'].items():assert sha(Path(row['path']))==row['sha256'],n
for n,h in pins['configuration'].items():assert sha(R/n)==h,n
assert sha(Path(pins['observerPath']))==pins['observerSha256']
for n,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/n,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/n,text=True).strip()==row['status']==''
assert policy['status']=='CoreSelectedBoundedPolicy_ImplementationPending' and len(policy['decisions'])==22
assert [d['id'] for d in policy['decisions']]==['PO'+str(i).zfill(2) for i in range(1,23)]
assert not policy['publicHelperActivated'] and not policy['pv01Complete'] and not policy['ownerAccepted'] and policy['requirementsWaived']==0
cases=read(P/'IMPLEMENTATION-CASES.json');assert cases['status']=='Planned_NotRun' and cases['newTestsPassed']==0 and len(cases['cases'])==37
assert len({c['id'] for c in cases['cases']})==37 and all(c['status']=='NotRun' for c in cases['cases'])
assert len(read(P/'ROUTE-MATRIX.json')['routes'])==9
items=read(P/'OPEN-ITEMS.json');assert items['requirementsWaived']==0 and {x['id'] for x in items['items']}=={'PV-H01','PV-H02','PV-T01','PV-L01','PV-Q01'}
plan=read(P/'PLAN.json');pv=next(t for t in plan['work'] if t['id']=='PV-01');assert pv['status']=='CorePolicySelected_ImplementationPending' and pv['nextSlice']=='PV-H01'
assert plan['noRequirementWaivers'] and not read(P/'SCOPE-FOLLOWUP.json')['pv01Complete']
for f in read(P/'SOURCE-FACTS.json')['facts']:
 assert sha(R/f['path'])==f['sha256']
 for t in f['requiredText']:assert t in (R/f['path']).read_text(encoding='utf-8'),(f['id'],t)
char=read(E/'CHARACTERIZATION.json');assert char['existingTestsPassed']==475 and char['newTestsPassed']==0 and len(char['projects'])==1 and len(char['classes'])==7
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 assert {'provenance-policy-prior-proof.log.json','provenance-policy-characterization.log.json'}<=set(z.namelist())
 actual=[]
 for row in char['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib
  assert c['failed']==c['notExecuted']=='0' and int(c['passed'])==475
  assert sha(Path(row['testBinaryPath']))==row['testBinarySha256']
  actual.extend(n.attrib for n in root.findall('.//{*}UnitTestResult'))
 assert actual==char['results'] and len(actual)==475 and all(n['outcome']=='Passed' for n in actual)
 for name in z.namelist():
  if name.endswith('.log.json'):
   receipt=json.loads(z.read(name));assert receipt['exitCode']==(1 if name=='provenance-policy-capture.log.json' else 0) and receipt['failure'] is None
   assert hashlib.sha256(z.read(name[:-5])).hexdigest()==receipt['logSha256']
prior=read(Q/'evidence/UNIT-RESULTS.json');assert prior['passed']==26054 and len(prior['projects'])==10
assert char['projects'][0]['testBinarySha256']==next(x for x in prior['projects'] if x['testAssembly'].casefold()=='bosak.xpath.runtime.tests')['testBinarySha256']
with zipfile.ZipFile(Q/'evidence/EXECUTION-RECEIPTS.zip') as z:
 total=0
 for row in prior['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib
  assert c['failed']==c['notExecuted']=='0' and int(c['passed'])==row['passed'];total+=row['passed']
  assert sha(Path(row['testBinaryPath']))==row['testBinarySha256']
 assert total==26054
 for name,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('public-xslt-'+name+'-release.log').decode('utf-8-sig')
  assert tuple(int(re.search(label+r':\s+(\d+)',log).group(1)) for label in ['Passed','Failed','Skipped'])==expected
reuse=read(E/'QUALIFICATION-REUSE.json');assert not reuse['priorFullTestsReexecuted'] and reuse['sourceBinaryConfigurationUnchanged']
for n,h in reuse['priorFiles'].items():assert sha(Q/n)==h,n
api=read(Q/'evidence/public-api.json');assert len(api['assemblies'])==11 and sum(a['publicTypes'] for a in api['assemblies'])==317 and sum(a['declarationCount'] for a in api['assemblies'])==6457
for group in [E/'bindings',Q/'evidence/bindings/units-release',Q/'evidence/bindings/qt3-release',Q/'evidence/bindings/qt4-release']:
 seen=set()
 for f in group.glob('*.json'):
  b=read(f);assert not b['errors']
  for x in b['assemblies']:
   if x['name'] in pins['engines']:
    assert x['sha256']==pins['engines'][x['name']]['sha256'] and x['context']=='Default' and not x['collectible'];seen.add(x['name'])
 assert {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen
package=read(Q/'evidence/PACKAGE-PREFLIGHT.json');assert package['sourceVersion']=='1.76.0' and len(package['selectedPackages'])==9 and not package['packagesBuilt']
for n,h in package['assets'].items():assert sha(R/n)==h,n
for row in package['commands']:assert sha(Q/row['evaluation'])==row['sha256']
for row in read(E/'DOCUMENTATION-CHECKS.json')['pages']:assert sha(R/row['source'])==row['sourceSha256']
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for n,h in m.items():assert sha(P/n)==h,n
print('Verified TA-33: 22 policy decisions / 37 planned cases, fresh475 existing controls, unchanged1.76.0/retained26054/gates/API. No helper activation or owner/freeze/publication claim.')
