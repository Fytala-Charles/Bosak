"""Verify the typed-admission audit, exact source pins and existing-control receipts."""
from pathlib import Path
import hashlib,json,subprocess,sys,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((P/'INPUTS.json').read_text());assert i['sourceVersion']=='1.52.0' and i['revision'].startswith('2981d395')
for n,h in i['files'].items():
 raw=(R/n).read_bytes();committed=subprocess.check_output(['git','show',i['revision']+':'+n],cwd=R)
 assert sha(R/n)==h and hashlib.sha256(committed).hexdigest()==i['committedFiles'][n],n
 assert hashlib.sha256(raw.replace(b'\r\n',b'\n')).hexdigest()==i['normalizedFiles'][n]==hashlib.sha256(committed.replace(b'\r\n',b'\n')).hexdigest(),n
counts={};rows=[]
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 log=json.loads(z.read('typed-admission-plan-controls.log.json'));assert log['exitCode']==0 and log['failure'] is None
 for n in z.namelist():
  if n.endswith('.trx'):
   for x in ET.fromstring(z.read(n)).iter():
    if x.tag.endswith('UnitTestResult'):
     key=next((k for k in ['SchemaValueTransportTests','SchemaEvidenceKernelTests','QuerySchemaAssociationTests','PublicOrdinarySessionTests'] if k+'.' in x.attrib['testName']),None)
     assert key and x.attrib['outcome']=='Passed';counts[key]=counts.get(key,0)+1;rows.append(x.attrib)
c=json.loads((E/'CONTROLS.json').read_text());assert c['passed']==104 and c['failed']==0 and counts==c['classes'] and sorted(rows,key=lambda x:x['testId'])==sorted(c['results'],key=lambda x:x['testId'])
pins=json.loads((R/'docs/foundation-exact-constructors/evidence/SOURCE-AND-BINARY-PINS.json').read_text());seen={}
for f in (E/'bindings').glob('*.json'):
 b=json.loads(f.read_text());assert not b['errors']
 for a in b['assemblies']:
  if a['name'] in pins['engines']:assert a['sha256']==pins['engines'][a['name']]['sha256'];seen[a['name']]=a['sha256']
assert seen==json.loads((E/'LOADED-BINDINGS.json').read_text())['loadedEngines']
s=json.loads((P/'STATUS.json').read_text());assert s['auditComplete'] and s['nextInternalSlicePrepared'] and s['existingControlsPassed']==104
for k in ['productionChanged','newTypedContainerImplemented','publicSchemaActivated','newPublicSignatureRequiredByAudit','absenceOfFutureSignatureGapProven','fullRuntimeGatesRerun','nativeQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized']:assert s[k] is False,k
matrix=json.loads((P/'MATRIX.json').read_text());assert len(matrix['rows'])==6 and len({x['id'] for x in matrix['rows']})==6
subprocess.run([sys.executable,str(R/'docs/foundation-exact-constructors/tools/verify.py')],check=True,capture_output=True)
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for n,h in m.items():assert sha(P/n)==h,n
print('Verified typed-admission preparation and 104 existing controls; production 1.52.0 unchanged. No new typed-container/public/native/freeze support inferred.')
