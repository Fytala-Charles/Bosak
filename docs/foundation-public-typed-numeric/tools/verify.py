"""Verify TA-09 exact source/binaries, adversarial controls and unchanged public scope."""
from pathlib import Path
import json,hashlib,zipfile,subprocess,xml.etree.ElementTree as ET,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
s=json.loads((P/'STATUS.json').read_text(encoding='utf-8'));pins=json.loads((E/'SOURCE-AND-BINARY-PINS.json').read_text(encoding='utf-8'));assert s['sourceVersion']==pins['sourceVersion']=='1.60.0' and s['unitTestsPassed']==25114
for n,h in pins['sources'].items():assert sha(R/n)==h,n
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip') as z:
 for n,row in pins['engines'].items():assert sha(Path(row['path']))==row['sha256']==hashlib.sha256(z.read(n+'.dll')).hexdigest(),n
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 for n in z.namelist():assert hashlib.sha256(z.read(n)).hexdigest()==pins['sources'][n],n
for n,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/n,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/n,text=True).strip()==row['status']
u=json.loads((E/'UNIT-RESULTS.json').read_text(encoding='utf-8'));assert len(u['projects'])==10 and sum(x['passed'] for x in u['projects'])==25114
actual=[];costs={};getters={}
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in u['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib;assert int(c['passed'])==row['passed'] and c['failed']=='0' and c['notExecuted']=='0'
  actual.extend(n.attrib for n in root.iter() if n.tag.endswith('UnitTestResult'))
  for n in root.iter():
   if n.tag.endswith('StdOut') and n.text:
    costs.update({k:int(v) for k,v in re.findall(r'Typed cache ([\w-]+): (\d+) bytes/read',n.text)})
    getters.update({k:int(v) for k,v in re.findall(r'Typed container getters ([\w-]+): (\d+) reads',n.text)})
 validationCosts={}
 for row in u['projects']:
  for n in ET.fromstring(z.read(row['trx'])).iter():
   if n.tag.endswith('StdOut') and n.text:validationCosts.update({k:int(v) for k,v in re.findall(r'Public typed numeric ([\w-]+): (\d+) bytes/call',n.text)})
 assert validationCosts==json.loads((E/'PUBLIC-NUMERIC-ALLOCATION.json').read_text(encoding='utf-8'))['bytesPerRead'] and set(validationCosts)=={'compute'} and 0<validationCosts['compute']<=32768
 for name in ['units','controls-final','build-final','language-server']:
  receipt=json.loads(z.read('public-numeric-'+name+'.log.json'));assert receipt['exitCode']==0 and receipt['failure'] is None
 for label,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('public-numeric-'+label+'.log').decode('utf-8-sig');assert tuple(int(re.search(k+r':\s+(\d+)',log).group(1)) for k in ['Passed','Failed','Skipped'])==expected
classes={n:[r for r in actual if n+'.' in r['testName']] for n in ['PublicTypedNumericTests','TypedNumericProjectionTests','PublicTypedIdentityTests','TypedIdentityVmTests','TypedIdentityCostTests','SchemaAdvisorAdapterTests','QueryTypedEvidenceTests','QueryTypedScalarCacheTests','QueryTypedContainerTests','QuerySchemaAssociationTests','PublicOrdinarySessionTests']}
a=json.loads((P/'ACCEPTANCE.json').read_text(encoding='utf-8'));assert classes==a['classes'] and a['newControlsPassed']==35 and len(classes['PublicTypedNumericTests'])==35 and len(classes['TypedNumericProjectionTests'])==35 and len(classes['PublicTypedIdentityTests'])==35 and len(classes['TypedIdentityVmTests'])==46 and len(classes['SchemaAdvisorAdapterTests'])==35 and len(classes['QueryTypedEvidenceTests'])==25 and len(classes['QueryTypedScalarCacheTests'])==35 and len(classes['QueryTypedContainerTests'])==94 and all(x['outcome']=='Passed' for rows in classes.values() for x in rows)
assert sum('GetterRevocationStopsFurtherReads' in x['testName'] for x in classes['QueryTypedContainerTests'])==44
cost=json.loads((E/'ALLOCATION-OBSERVATIONS.json').read_text(encoding='utf-8'));assert costs==cost['bytesPerRead'] and len(costs)==3 and all(0<v<=32768 for v in costs.values()) and not cost['budgetsAccepted']
for f in (E/'bindings').rglob('*.json'):
 b=json.loads(f.read_text(encoding='utf-8'));assert not b['errors']
 for x in b['assemblies']:
  if x['name'] in pins['engines']:assert x['sha256']==pins['engines'][x['name']]['sha256']
bindings=json.loads((E/'LOADED-BINDINGS.json').read_text(encoding='utf-8'));assert bindings['allMatchExactCandidate'] and all({'Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime'}<=set(bindings['groups'][g]) for g in ['units','qt3','qt4'])
for label in ['qt3','qt4']:assert (E/(label+'-skips.txt')).read_bytes()==(R/'docs/foundation-bcap-profile/evidence'/(label+'-skips.txt')).read_bytes()
previous=json.loads((R/'docs/foundation-typed-numeric-projection/evidence/SOURCE-AND-BINARY-PINS.json').read_text(encoding='utf-8'))
changed={n for n,h in previous['sources'].items() if n.startswith('src/Bosak.XPath.Api/') and pins['sources'][n]!=h}
added={n for n in pins['sources'] if n.startswith('src/Bosak.XPath.Api/') and n not in previous['sources']}
assert changed=={'src/Bosak.XPath.Api/Schema/ExactArithmeticPlan.cs','src/Bosak.XPath.Api/Schema/SchemaQuerySession.cs'} and added=={'src/Bosak.XPath.Api/Schema/TypedNumericPlan.cs'}
subprocess.run(['python',str(P/'tools/audit.py')],check=True,capture_output=True)
plan=json.loads((P/'PLAN-INPUT.json').read_text(encoding='utf-8'));assert sha(R/plan['path'])==plan['sha256']==hashlib.sha256(subprocess.check_output(['git','show',plan['revision']+':'+plan['path']],cwd=R)).hexdigest()
assert s['internalNumericProjectionQualified'] and s['publicSchemaActivated'] and s['publicTypedIdentityActivated'] and s['publicContractAdapterQualified'] and s['transportEvidenceAttached'] and s['internalImplemented'] and s['internalQualified'] and s['persistentCacheImplemented'] and s['publicMetadataUnchanged']
for k in ['generalVmCaptureActivated','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized']:assert s[k] is False,k
assert s['typedNumericRuntimeActivated'] and s['newPublicProfile']
assert s['computeBytesPerCall']==json.loads((E/'PUBLIC-NUMERIC-ALLOCATION.json').read_text(encoding='utf-8'))['bytesPerRead']['compute'] and s['computedResultsPlain'] and s['originalEvidencePreserved']
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text(encoding='utf-8'));assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for n,h in m.items():assert sha(P/n)==h,n
print('Verified TA-09 public typed numeric execution: 25114 tests, 35 new controls, frozen gates and unchanged public metadata. Existing public identity retained; no native/freeze acceptance.')

