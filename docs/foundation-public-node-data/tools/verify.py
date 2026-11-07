"""Verify TA-17 bounded original-node data, exact candidates and original receipts."""
from pathlib import Path
import hashlib,json,re,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
i=read(P/'INPUTS.json');s=read(P/'STATUS.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json');initial=read(E/'INITIAL-PINS.json')
assert s['sourceVersion']==pins['sourceVersion']==i['sourceVersion']=='1.64.0'
assert s['baseCommit']==pins['baseCommit']==i['baseCommit'] and i['priorSourceVersion']=='1.63.1'
assert sha((R/i['priorManifest']).read_bytes())==i['priorManifestSha256']
for name,digest in read(R/i['priorManifest']).items():assert sha((R/i['priorManifest']).parent.joinpath(name).read_bytes())==digest,name
for name,digest in pins['sources'].items():assert sha((R/name).read_bytes())==digest,name
for label,cohort in [('',pins),('INITIAL-',initial)]:
 with zipfile.ZipFile(E/(label+'ENGINE-BINARIES.zip')) as z:
  for name,row in cohort['engines'].items():
   assert sha(z.read(name+'.dll'))==row['sha256'],name
   if not label:assert sha(Path(row['path']).read_bytes())==row['sha256'],name
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 for name in z.namelist():assert sha(z.read(name))==pins['sources'][name]
with zipfile.ZipFile(E/'INITIAL-SOURCE.zip') as z:
 for name in z.namelist():assert sha(z.read(name))==initial['sources'][name]
assert {name for name,h in initial['sources'].items() if pins['sources'][name]!=h}=={'src/Bosak.XPath.Runtime/Vm/QueryNodeFacts.cs','tests/Bosak.XPath.Runtime.Tests/PublicOriginalDataTests.cs'}
for name,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']==''
units=read(E/'UNIT-RESULTS.json');acceptance=read(E/'ACCEPTANCE.json');actual=[];cost={}
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in units['projects']:
  raw=z.read(row['trx']);assert sha(raw)==row['sha256'];r=ET.fromstring(raw);c=r.find('.//{*}Counters').attrib
  assert int(c['passed'])==row['passed'] and c['failed']==c['notExecuted']=='0'
  assert sha((R/'tests'/row['project']/'bin/Release/net10.0'/(row['project']+'.dll')).read_bytes())==row['testBinarySha256']
  actual.extend(n.attrib for n in r.findall('.//{*}UnitTestResult'))
  for node in r.iter():
   if node.tag.endswith('StdOut') and node.text:
    m=re.search(r'Public data warm three: (\d+) bytes/query; slots: (\d+); typed outcomes: (\d+)',node.text)
    if m:cost.update(zip(['warmThreeBytesPerQuery','slots','typedOutcomesInTenQueries'],map(int,m.groups())))
    m=re.search(r'Public data cold session three: (\d+) bytes',node.text)
    if m:cost['coldThreeSessionBytes']=int(m.group(1))
 for label in ['build-initial','build-tests','build-2','build-3','build-final','focused','focused-final','units','qt3','qt4','language-server','api-audit','final-build','final-build-reviewed','final-units','final-qt3','final-qt4','final-language-server','final-api-audit']:
  name='public-data-'+label+'.log';receipt=json.loads(z.read(name+'.json'))
  assert sha(z.read(name))==receipt['logSha256'] and receipt['failure'] is None
  assert receipt['exitCode']==(1 if label=='build-tests' else 0),label
 for label,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  text=z.read('public-data-final-'+label+'.log').decode('utf-8-sig')
  assert tuple(int(re.search(k+r':\s+(\d+)',text).group(1)) for k in ['Passed','Failed','Skipped'])==expected
 initialRows=[]
 for name in z.namelist():
  if name.startswith('public-data-unit-trx/') and name.endswith('.trx'):
   root=ET.fromstring(z.read(name));c=root.find('.//{*}Counters').attrib;assert c['failed']==c['notExecuted']=='0'
   initialRows.extend(n.attrib for n in root.findall('.//{*}UnitTestResult'))
 assert len(initialRows)==25452 and all(row['outcome']=='Passed' for row in initialRows)
assert len(units['projects'])==10 and sum(row['passed'] for row in units['projects'])==units['passed']==s['unitTestsPassed']==25453
assert all(row['outcome']=='Passed' for row in actual)
for name,rows in acceptance['classes'].items():assert rows==[r for r in actual if '.'+name+'.' in r['testName']]
assert len(acceptance['classes']['PublicOriginalDataTests'])==s['newCasesPassed']==117
assert len(acceptance['classes']['PublicOriginalNilledTests'])==106 and len(acceptance['classes']['QueryNodeFactsTests'])==116
assert cost==s['allocation']==acceptance['allocation'] and cost['slots']==3 and cost['typedOutcomesInTenQueries']==10
mapping=read(P/'CASE-MAPPING.json');assert mapping['implementationStatus']=='BoundedPublicQualified' and not mapping['nativeProviderAccepted']
assert [row['id'] for row in mapping['cases']]==['AD'+str(n).zfill(2) for n in range(1,41)]
for row in mapping['cases']:
 assert row['status']=='QualifiedBoundedPublicScope' and row['tests']
 for pattern in row['tests']:assert any(pattern in r['testName'] and r['outcome']=='Passed' for r in actual),pattern
assert any('AD25_TransactionFault(point: -2)' in row['testName'] for row in actual)
bindings=read(E/'LOADED-BINDINGS.json');assert bindings['allMatchExactCandidate']
for group,expected in bindings['groups'].items():
 observed=pins if group.startswith('final-') else initial;seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for a in b['assemblies']:
   if a['name'] in observed['engines']:
    assert a['sha256']==observed['engines'][a['name']]['sha256'] and a['context']=='Default' and not a['collectible']
    seen.add(a['name'])
 assert sorted(seen)==expected and {'Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime'}<=seen
previousPath=R/'docs/foundation-public-nilled-cost/evidence/public-api.json';previous=read(previousPath);current=read(E/'public-api.json');by={a['assembly']:a for a in previous['assemblies']}
for a in current['assemblies']:
 for key in ['declarations','friendAssemblies','references']:assert a[key]==by[a['assembly']][key],(a['assembly'],key)
 assert a['sha256'].lower()==pins['engines'][a['assembly']]['sha256']
assert len(current['assemblies'])==11 and sum(a['publicTypes'] for a in current['assemblies'])==310 and sum(a['declarationCount'] for a in current['assemblies'])==6369
assert read(E/'API-COMPARISON.json')['previousInventorySha256']==sha(previousPath.read_bytes())
for label in ['qt3','qt4']:assert (E/(label+'-skips.txt')).read_bytes()==(R/'docs/foundation-public-nilled-cost/evidence'/(label+'-skips.txt')).read_bytes()
assert s['dataProfileActivated'] and s['selectedAcceptanceCasesQualified']==40 and s['skipMapsUnchanged'] and s['publicMetadataUnchanged']
for key in ['newPublicSignatures','factsCacheIntroduced','persistentEmptyIndexIntroduced','generalImplicitAtomizationActivated','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized','pushPerformed']:assert s[key] is False,key
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,digest in m.items():assert sha((P/name).read_bytes())==digest,name
print('Verified TA-17: 25453 tests / 117 new, AD01-AD40, exact gates/skips/API/bindings. Native/owner/freeze/publication pending.')
