"""Verify TA-01 exact source/binaries, adversarial controls and unchanged public scope."""
from pathlib import Path
import json,hashlib,zipfile,subprocess,xml.etree.ElementTree as ET,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
s=json.loads((P/'STATUS.json').read_text());pins=json.loads((E/'SOURCE-AND-BINARY-PINS.json').read_text());assert s['sourceVersion']==pins['sourceVersion']=='1.53.0' and s['unitTestsPassed']==24865
for n,h in pins['sources'].items():assert sha(R/n)==h,n
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip') as z:
 for n,row in pins['engines'].items():assert sha(Path(row['path']))==row['sha256']==hashlib.sha256(z.read(n+'.dll')).hexdigest(),n
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 for n in z.namelist():assert hashlib.sha256(z.read(n)).hexdigest()==pins['sources'][n],n
for n,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/n,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/n,text=True).strip()==row['status']
u=json.loads((E/'UNIT-RESULTS.json').read_text());assert len(u['projects'])==10 and sum(x['passed'] for x in u['projects'])==24865
actual=[];costs={};getters={}
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in u['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib;assert int(c['passed'])==row['passed'] and c['failed']=='0' and c['notExecuted']=='0'
  actual.extend(n.attrib for n in root.iter() if n.tag.endswith('UnitTestResult'))
  for n in root.iter():
   if n.tag.endswith('StdOut') and n.text:
    costs.update({k:int(v) for k,v in re.findall(r'Typed container cold ([\w-]+): (\d+) bytes/read',n.text)})
    getters.update({k:int(v) for k,v in re.findall(r'Typed container getters ([\w-]+): (\d+) reads',n.text)})
 for name in ['units-final','controls-0','controls-1','controls-2']:
  receipt=json.loads(z.read('typed-container-'+name+'.log.json'));assert receipt['exitCode']==0 and receipt['failure'] is None
 for label,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('typed-container-'+label+'-final.log').decode('utf-8-sig');assert tuple(int(re.search(k+r':\s+(\d+)',log).group(1)) for k in ['Passed','Failed','Skipped'])==expected
classes={n:[r for r in actual if n+'.' in r['testName']] for n in ['QueryTypedContainerTests','QuerySchemaAssociationTests','PublicOrdinarySessionTests']}
a=json.loads((P/'ACCEPTANCE.json').read_text());assert classes==a['classes'] and a['newControlsPassed']==94 and len(classes['QueryTypedContainerTests'])==94 and all(x['outcome']=='Passed' for rows in classes.values() for x in rows)
assert sum('GetterRevocationStopsFurtherReads' in x['testName'] for x in classes['QueryTypedContainerTests'])==44
cost=json.loads((E/'ALLOCATION-OBSERVATIONS.json').read_text());assert costs==cost['bytesPerRead'] and getters==cost['providerReadsForFirstAdmission']=={'scalar':6,'list-three':22} and len(costs)==3 and all(0<costs[k]<=v for k,v in cost['regressionCeilingsBytes'].items()) and not cost['budgetsAccepted']
for f in (E/'bindings').rglob('*.json'):
 b=json.loads(f.read_text());assert not b['errors']
 for x in b['assemblies']:
  if x['name'] in pins['engines']:assert x['sha256']==pins['engines'][x['name']]['sha256']
for label in ['qt3','qt4']:assert (E/(label+'-skips.txt')).read_bytes()==(R/'docs/foundation-bcap-profile/evidence'/(label+'-skips.txt')).read_bytes()
previous=json.loads((R/'docs/foundation-exact-constructors/evidence/SOURCE-AND-BINARY-PINS.json').read_text())
for n,h in previous['sources'].items():
 if n.startswith('src/Bosak.XPath.Api/'):assert pins['sources'][n]==h,n
subprocess.run(['python',str(P/'tools/audit.py')],check=True,capture_output=True)
plan=json.loads((P/'PLAN-INPUT.json').read_text());assert sha(R/plan['path'])==plan['sha256']==hashlib.sha256(subprocess.check_output(['git','show',plan['revision']+':'+plan['path']],cwd=R)).hexdigest()
assert s['internalImplemented'] and s['internalQualified'] and s['publicMetadataUnchanged']
for k in ['publicSchemaActivated','transportEvidenceAttached','persistentCacheImplemented','newPublicProfile','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized']:assert s[k] is False,k
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for n,h in m.items():assert sha(P/n)==h,n
print('Verified TA-01 internal reader: 24865 tests, 94 new controls, frozen gates and unchanged public metadata/API source. No public/native/cache/freeze activation.')

prior=E/'pre-membership-allocation';oldPins=json.loads((prior/'SOURCE-AND-BINARY-PINS.json').read_text())
with zipfile.ZipFile(prior/'ENGINE-BINARIES.zip') as z:
 for n,row in oldPins['engines'].items():assert hashlib.sha256(z.read(n+'.dll')).hexdigest()==row['sha256']
for f in (prior/'bindings').rglob('*.json'):
 b=json.loads(f.read_text());assert not b['errors']
 for x in b['assemblies']:
  if x['name'] in oldPins['engines']:assert x['sha256']==oldPins['engines'][x['name']]['sha256']
with zipfile.ZipFile(prior/'EXECUTION-RECEIPTS.zip') as z:
 assert json.loads(z.read('typed-container-units.log.json'))['exitCode']==0
oldCost=json.loads((prior/'ALLOCATION-OBSERVATIONS.json').read_text());comparison=json.loads((E/'ALLOCATION-COMPARISON.json').read_text())
assert comparison['before']==oldCost['bytesPerRead'] and comparison['after']==costs and comparison['getterCountsUnchanged'] and all(costs[n]<oldCost['bytesPerRead'][n] for n in costs)
