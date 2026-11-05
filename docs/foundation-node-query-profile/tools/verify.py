"""Verify source-unchanged node-fact preparation and original execution receipts."""
from pathlib import Path
import hashlib,json,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
i=read(P/'INPUTS.json');s=read(P/'STATUS.json');pins=read(R/i['priorQualification'])
assert sha((R/i['priorQualification']).read_bytes())==i['priorQualificationSha256']
for n,row in i['files'].items():
 b=(R/n).read_bytes();old=subprocess.check_output(['git','show',i['revision']+':'+n],cwd=R)
 assert sha(b)==row['workspaceSha256'] and sha(old)==row['committedSha256'],n
 assert sha(b.replace(b'\r\n',b'\n'))==sha(old.replace(b'\r\n',b'\n'))==row['normalizedSha256'],n
for n,h in pins['sources'].items():assert sha((R/n).read_bytes())==h,n
for row in pins['engines'].values():assert sha(Path(row['path']).read_bytes())==row['sha256']
c=read(E/'CONTROLS.json');actual=[]
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for label,expected in [('runtime',311),('api',38)]:
  receipt=json.loads(z.read('node-profile-'+label+'.log.json'));assert receipt['exitCode']==0 and receipt['failure'] is None
  assert sha(z.read('node-profile-'+label+'.log'))==receipt['logSha256']
  names=[n for n in z.namelist() if n.startswith('node-profile-'+label+'-trx/') and n.endswith('.trx')];assert len(names)==1
  root=ET.fromstring(z.read(names[0]));counter=root.find('.//{*}Counters').attrib
  assert int(counter['passed'])==expected and counter['failed']=='0' and counter['notExecuted']=='0'
  actual.extend(n.attrib for n in root.iter() if n.tag.endswith('UnitTestResult'))
assert actual==c['results'] and c['passed']==349 and c['failed']==0 and all(n['outcome']=='Passed' for n in actual)
for group in ['runtime','api']:
 seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for a in b['assemblies']:
   if a['name'] in pins['engines']:assert a['sha256']==pins['engines'][a['name']]['sha256'];seen.add(a['name'])
 assert sorted(seen)==c['loadedBindings'][group] and {'Bosak.XPath.Api','Bosak.XPath.Core','Bosak.XPath.Runtime'}<=seen
cases=read(P/'ACCEPTANCE-CASES.json');assert cases['implementationStatus']=='NotStarted'
assert [n['id'] for n in cases['cases']]==['NP'+str(n).zfill(2) for n in range(1,25)]
assert all(n['status']=='NotRun' and n['trigger'] and n['expected'] for n in cases['cases'])
profile=read(P/'PROFILE.json');assert profile['status']=='SelectedForImplementation' and not profile['activated']
assert profile['acceptedTrimmedSource']==['nilled($node)','fn:nilled($node)'] and profile['behaviorProfile']=='bosak-original-node-nilled-v1'
assert profile['descriptorFormat']=='bosak-original-node-nilled-descriptor-v1' and profile['requiresInternalMetadataOnlyReader'] and profile['requiresPublicAdvisorFactsBridge']
assert profile['input']['directKinds']==['Node','Undefined'] and not profile['input']['sequenceWrappers'] and not profile['dataAtomizationActivated'] and not profile['publicSignaturesAdded']
assert s['sourceVersion']=='1.61.1' and s['existingControlsPassed']==349 and s['newCasesNotRun']==24
for k in ['implementationStarted','publicNodeProfileActivated','metadataOnlyReadImplemented','fullGatesRerun','sourceChanged','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized']:assert s[k] is False,k
subprocess.run(['python',str(R/'docs/foundation-node-facts-cost/tools/verify.py')],check=True,capture_output=True)
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for n,h in m.items():assert sha((P/n).read_bytes())==h,n
print('Verified selected nilled profile: 349 existing controls; 24 new cases NotRun; source/binaries unchanged; no activation/freeze acceptance.')
