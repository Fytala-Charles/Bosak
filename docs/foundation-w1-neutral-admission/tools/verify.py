"""Verify W1-A source, exact selected receipts and remaining scope without rebuilding."""
from pathlib import Path
import hashlib,json,zipfile,xml.etree.ElementTree as ET,subprocess
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
s=json.loads((P/'STATUS.json').read_text());pins=json.loads((E/'SOURCE-AND-BINARY-PINS.json').read_text());assert s['sourceVersion']==pins['sourceVersion']=='1.43.0' and s['unitTestsPassed']==24414
for name,h in pins['sources'].items():assert sha(R/name)==h,name
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip') as z:
 for name,row in pins['engines'].items():assert sha(Path(row['path']))==row['sha256']==hashlib.sha256(z.read(name+'.dll')).hexdigest(),name
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 for name in z.namelist():assert hashlib.sha256(z.read(name)).hexdigest()==pins['sources'][name],name
for name,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']
u=json.loads((E/'UNIT-RESULTS.json').read_text());assert len(u['projects'])==10 and sum(x['passed'] for x in u['projects'])==24414
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in u['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];c=ET.fromstring(raw).find('.//{*}Counters').attrib;assert int(c['passed'])==row['passed'] and c['failed']=='0'
 assert 'w1-all-units.log' in z.namelist() and 'database-loopback.trx' in ' '.join(z.namelist())
plan=json.loads((P/'ACCEPTANCE.json').read_text());assert len(plan['cases'])==20
required=[c for c in plan['cases'] if c['requiredFor']=='W1-A neutral admission']+[plan['additionalDrainControl']];assert len(required)==18
for row in required:
 assert row['status']=='Passed';receipt=json.loads((P/row['receipt']).read_text());assert receipt['status']=='Passed' and receipt['results'] and all(x['outcome']=='Passed' for x in receipt['results'])
assert {c['id'] for c in plan['cases'] if c['status']=='NotRun'}=={'W1-09','W1-10','W1-18'}
api=json.loads((E/'API-COMPARISON.json').read_text());assert api['assemblies']==11 and api['exportedTypes']==270 and api['declarationRows']==5770 and api['declarationsIdentical'] and api['friendsIdentical']
actual=json.loads((E/'public-api.json').read_text());base=json.loads((R/'docs/foundation-xslt-source-membership/evidence/public-api-0.json').read_text());old={a['assembly']:a for a in base['assemblies']}
for a in actual['assemblies']:assert a['declarations']==old[a['assembly']]['declarations'] and a['friendAssemblies']==old[a['assembly']]['friendAssemblies']
for f in (E/'bindings').rglob('*.json'):
 b=json.loads(f.read_text());assert not b['errors']
 for a in b['assemblies']:
  if a['name'] in pins['engines']:assert a['sha256']==pins['engines'][a['name']]['sha256'],a
for label in ['qt3','qt4']:
 assert (E/(label+'-skips.txt')).read_bytes()==(R/'docs/foundation-bcap-profile/evidence'/(label+'-skips.txt')).read_bytes(),label+' skip map'
for n in range(2):
 probe=json.loads((E/f'bcap-regression-{n}.json').read_text());assert probe['passed'] and len(probe['rows'])==83
for key in ['publicFacadeImplemented','allHostsImplemented','optionalSchemaAdvisorCompositionImplemented','c45Accepted','apiFrozen','publicationAuthorized']:assert s[key] is False
assert s['qt3']==[31397,0,424] and s['qt4Gated']==[8941,0,1918]
if (P/'MANIFEST.json').exists():
 for name,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/name)==h,name
print('Verified W1-A 1.43 source/binary cohort, 24414 unit passes, 18 acceptance prefixes, 83 private regressions twice, exact skip maps and unchanged public metadata. Dependent slices/native/freeze remain pending.')
