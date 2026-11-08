"""Verify exact function-sharing source/receipts and allocation result without rebuilding."""
from pathlib import Path
import hashlib,json,zipfile,xml.etree.ElementTree as ET,subprocess,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
s=json.loads((P/'STATUS.json').read_text());pins=json.loads((E/'SOURCE-AND-BINARY-PINS.json').read_text());assert s['sourceVersion']==pins['sourceVersion']=='1.44.1' and s['unitTestsPassed']==24467
for name,h in pins['sources'].items():assert sha(R/name)==h,name
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip') as z:
 for name,row in pins['engines'].items():assert sha(Path(row['path']))==row['sha256']==hashlib.sha256(z.read(name+'.dll')).hexdigest(),name
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 for name in z.namelist():assert hashlib.sha256(z.read(name)).hexdigest()==pins['sources'][name],name
for name,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']
u=json.loads((E/'UNIT-RESULTS.json').read_text());assert len(u['projects'])==10 and sum(x['passed'] for x in u['projects'])==24467
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in u['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];c=ET.fromstring(raw).find('.//{*}Counters').attrib;assert int(c['passed'])==row['passed'] and c['failed']=='0' and c['notExecuted']=='0'
 log=json.loads(z.read('function-sharing-all-units.log.json'));assert log['exitCode']==0 and log['failure'] is None
 controls=z.read('function-sharing-controls-trx/function-sharing-controls.trx');a=json.loads((P/'ACCEPTANCE.json').read_text());assert hashlib.sha256(controls).hexdigest()==a['trxSha256']
 assert len(a['registryResults'])==20 and len(a['associationResults'])==33 and len(a['ordinaryResults'])==24 and all(x['outcome']=='Passed' for x in a['registryResults']+a['associationResults']+a['ordinaryResults'])
 for label,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('function-sharing-'+label+'.log').decode('utf-8-sig');assert tuple(int(re.search(k+r':\s+(\d+)',log).group(1)) for k in ['Passed','Failed','Skipped'])==expected
for f in (E/'bindings').rglob('*.json'):
 b=json.loads(f.read_text());assert not b['errors']
 for a in b['assemblies']:
  if a['name'] in pins['engines']:assert a['sha256']==pins['engines'][a['name']]['sha256'],a
api=json.loads((E/'public-api.json').read_text());base=json.loads((R/'docs/foundation-xslt-source-membership/evidence/public-api-0.json').read_text());old={x['assembly']:x for x in base['assemblies']}
assert len(api['assemblies'])==11 and sum(x['publicTypes'] for x in api['assemblies'])==270 and sum(x['declarationCount'] for x in api['assemblies'])==5770
for a in api['assemblies']:assert a['declarations']==old[a['assembly']]['declarations'] and a['friendAssemblies']==old[a['assembly']]['friendAssemblies']
for label in ['qt3','qt4']:assert (E/(label+'-skips.txt')).read_bytes()==(R/'docs/foundation-bcap-profile/evidence'/(label+'-skips.txt')).read_bytes()
for n in range(2):
 d=json.loads((E/f'bcap-regression-{n}.json').read_text());assert d['passed'] and len(d['rows'])==83
cost=json.loads((E/'COST-OBSERVATIONS.json').read_text());assert not cost['budgetsAccepted'] and cost['warmBytesSavedPerCapturedEvaluation']==19256
oldpins=json.loads((R/'docs/foundation-w1-schema-association/evidence/SOURCE-AND-BINARY-PINS.json').read_text())['engines']
for lane,records in cost['rows'].items():
 assert len(records)==3
 expected=oldpins if lane=='baseline' else pins['engines']
 for record in records:
  assert record['passed']
  for a in record['bindings']:
   if a['name'] in expected:assert a['sha256']==expected[a['name']]['sha256']
for n in range(3):
 before={r['name']:r for r in cost['rows']['baseline'][n]['rows']};after={r['name']:r for r in cost['rows']['candidate'][n]['rows']}
 for name,row in before.items():assert row['allocatedBytes']-after[name]['allocatedBytes']==(19256*4000 if name in ['ordinary-warm-operation','assessed-warm-identity'] else 0),name
assert cost['publicContextBytes']==[2640,2640] and cost['publicPopulatedContextBytes']==[21896,21896] and cost['isolatedPopulatedContextBytes']==[21896,2640]
for key in ['poolingUsed','publicFacadeImplemented','nativeProviderQualified','performanceBudgetsAccepted','c45Accepted','apiFrozen','publicationAuthorized']:assert s[key] is False
if (P/'MANIFEST.json').exists():
 for name,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/name)==h,name
print('Verified 1.44.1 allocation fix: 24467 unit passes, 20 registry/33 association/24 ordinary controls, exact gates/skip maps/API and 19256-byte captured-operation savings. Public/native/freeze work remains pending.')
