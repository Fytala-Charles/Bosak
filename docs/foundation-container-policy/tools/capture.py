"""Retain unchanged TA-32 qualification and fresh existing-route characterization; no source changes."""
from pathlib import Path
import json,hashlib,zipfile,re,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';Q=R/'docs/foundation-leased-provenance'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,d):p.write_text(json.dumps(d,indent=2)+'\n',encoding='utf-8')
pins=read(E/'SOURCE-AND-BINARY-PINS.json');assert (E/'SOURCE-AND-BINARY-PINS.json').read_bytes()==(Q/'evidence/SOURCE-AND-BINARY-PINS.json').read_bytes()
for n,h in pins['sources'].items():assert sha(R/n)==h,n
for n,row in pins['engines'].items():assert sha(Path(row['path']))==row['sha256'],n
projects=[];rows=[]
for f in sorted((E/'characterization-trx').glob('*.trx')):
 root=ET.parse(f);c=root.find('.//{*}Counters').attrib;assert c['failed']==c['notExecuted']=='0'
 binary=Path(root.find('.//{*}UnitTest').attrib['storage']);rows.extend(n.attrib for n in root.findall('.//{*}UnitTestResult'))
 projects.append(dict(trx=f.relative_to(E).as_posix(),sha256=sha(f),passed=int(c['passed']),testBinaryPath=str(binary),testBinarySha256=sha(binary)))
assert len(projects)==1 and len(rows)==364
classes={}
for row in rows:
 assert row['outcome']=='Passed';name=re.match(r'Bosak\.XPath\.Runtime\.Tests\.([^.]+)\.',row['testName']).group(1);classes[name]=classes.get(name,0)+1
expected={'QueryTypedContainerTests','QueryTypedScalarCacheTests','PublicTypedIdentityTests','CompatibleCacheTests','PublicCompatibleTests','SchemaAtomicsTests'}
assert set(classes)==expected
runtime=next(row for row in read(Q/'evidence/UNIT-RESULTS.json')['projects'] if row['testAssembly'].casefold()=='bosak.xpath.runtime.tests')
assert projects[0]['testBinarySha256']==runtime['testBinarySha256']
write(E/'CHARACTERIZATION.json',dict(existingTestsPassed=364,newTestsPassed=0,classes=classes,projects=projects,results=rows,scope='Existing complete-reader/cache/scalar/compatible routes; not qualification of the planned public container member.'))
seen=set()
for f in (E/'bindings').glob('*.json'):
 b=read(f);assert not b['errors']
 for a in b['assemblies']:
  if a['name'] in pins['engines']:
   assert a['sha256']==pins['engines'][a['name']]['sha256'] and a['context']=='Default' and not a['collectible'];seen.add(a['name'])
assert {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen
write(E/'LOADED-BINDINGS.json',dict(allMatchQualifiedCandidate=True,observed=sorted(seen)))
reuse=['MANIFEST.json','evidence/SOURCE-AND-BINARY-PINS.json','evidence/UNIT-RESULTS.json','evidence/public-api.json','evidence/API-COMPARISON.json','evidence/EXECUTION-RECEIPTS.zip','evidence/PACKAGE-PREFLIGHT.json']
write(E/'QUALIFICATION-REUSE.json',dict(priorDossier='docs/foundation-leased-provenance',priorFiles={n:sha(Q/n) for n in reuse},sourceVersion='1.78.0',sourceBinaryConfigurationUnchanged=True,priorFullTestsReexecuted=False,retainedUnitTestsPassed=26149,retainedQt3=[31397,0,424],retainedQt4Gated=[8941,0,1918],retainedApiTypes=318,retainedApiRows=6463,selectedPackages=9))
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','w',zipfile.ZIP_DEFLATED) as z:
 for receipt in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('container-policy-*.log.json')):
  log=receipt.with_suffix('');z.write(log,log.name);z.write(receipt,receipt.name)
 for f in sorted((E/'characterization-trx').glob('*.trx')):z.write(f,f.relative_to(E).as_posix())
write(P/'STATUS.json',dict(date='2026-11-06',baseCommit=pins['baseCommit'],sourceVersion='1.78.0',productionChanged=False,publicApiChanged=False,policySelected=True,policyDecisionCount=16,plannedCases=32,newTestsPassed=0,existingCharacterizationTestsPassed=364,retainedUnitTestsPassed=26149,freshFullRun=False,containerMemberImplemented=False,pv01Complete=False,ownerAccepted=False,requirementsWaived=0,apiFrozen=False,publicationAuthorized=False,packagesBuilt=False,pushPerformed=False,nextSlice='PV-T02'))
print('Captured TA-36: 16 policy decisions, 32 planned cases, 364 existing controls; unchanged 1.78.0 and retained full/gate/API qualification.')
