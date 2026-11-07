"""Verify TA-14 public nilled scope, candidate identities and original qualification."""
from pathlib import Path
import hashlib,json,re,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
s=read(P/'STATUS.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json');i=read(P/'INPUTS.json')
assert s['sourceVersion']==pins['sourceVersion']==i['sourceVersion']=='1.63.0'
assert s['baseCommit']==pins['baseCommit']==i['baseCommit']
assert sha((R/i['prerequisiteManifest']).read_bytes())==i['prerequisiteManifestSha256']
for name,digest in read(R/i['prerequisiteManifest']).items():assert sha((R/i['prerequisiteManifest']).parent.joinpath(name).read_bytes())==digest,name
for name,digest in pins['sources'].items():assert sha((R/name).read_bytes())==digest,name
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip') as z:
 for name,row in pins['engines'].items():assert sha(Path(row['path']).read_bytes())==row['sha256']==sha(z.read(name+'.dll')),name
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 for name in z.namelist():assert sha(z.read(name))==pins['sources'][name]
for name,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']==''
units=read(E/'UNIT-RESULTS.json');acceptance=read(E/'ACCEPTANCE.json');actual=[];observations={}
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in units['projects']:
  raw=z.read(row['trx']);assert sha(raw)==row['sha256'];r=ET.fromstring(raw);c=r.find('.//{*}Counters').attrib
  assert int(c['passed'])==row['passed'] and c['failed']==c['notExecuted']=='0'
  assert sha((R/'tests'/row['project']/'bin/Release/net10.0'/(row['project']+'.dll')).read_bytes())==row['testBinarySha256']
  actual.extend(n.attrib for n in r.findall('.//{*}UnitTestResult'))
  for n in r.iter():
   if n.tag.endswith('StdOut') and n.text:
    m=re.search(r'Public nilled warm: (\d+) bytes/query; provider callbacks: (\d+); metadata reads: (\d+)',n.text)
    if m:observations.update(zip(['warmBytesPerQuery','providerCallbacksPerQuery','metadataReadsPerQuery'],map(int,m.groups())))
    m=re.search(r'Public nilled cold operation: (\d+) bytes/query',n.text)
    if m:observations['coldOperationBytesPerQuery']=int(m.group(1))
 for label in ['build','build-2','build-final','focused','units','language-server','api-audit','qt3','qt4']:
  name='public-nilled-'+label+'.log';receipt=json.loads(z.read(name+'.json'))
  assert sha(z.read(name))==receipt['logSha256'] and receipt['failure'] is None
  assert receipt['exitCode']==(1 if label=='build' else 0)
 for label,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  text=z.read('public-nilled-'+label+'.log').decode('utf-8-sig')
  assert tuple(int(re.search(k+r':\s+(\d+)',text).group(1)) for k in ['Passed','Failed','Skipped'])==expected
assert len(units['projects'])==10 and len({r['project'] for r in units['projects']})==10
assert sum(r['passed'] for r in units['projects'])==units['passed']==s['unitTestsPassed']==25306
assert all(r['outcome']=='Passed' for r in actual)
for name,rows in acceptance['classes'].items():assert rows==[r for r in actual if '.'+name+'.' in r['testName']]
assert len(acceptance['classes']['PublicOriginalNilledTests'])==s['newCasesPassed']==76
assert len(acceptance['classes']['QueryNodeFactsTests'])==116
assert observations==s['allocation']==acceptance['allocation'] and observations['metadataReadsPerQuery']==5
mapping=read(P/'CASE-MAPPING.json');assert mapping['implementationStatus']=='BoundedPublicQualified' and not mapping['nativeProviderAccepted']
assert [row['id'] for row in mapping['cases']]==['NP'+str(n).zfill(2) for n in range(1,25)]
for row in mapping['cases']:
 assert row['status']=='QualifiedBoundedPublicScope' and row['tests']
 for pattern in row['tests']:assert any(pattern in r['testName'] and r['outcome']=='Passed' for r in actual),pattern
bindings=read(E/'LOADED-BINDINGS.json');assert bindings['allMatchExactCandidate']
for group,expected in bindings['groups'].items():
 seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for a in b['assemblies']:
   if a['name'] in pins['engines']:
    assert a['sha256']==pins['engines'][a['name']]['sha256'] and a['context']=='Default' and not a['collectible']
    seen.add(a['name'])
 assert sorted(seen)==expected and {'Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime'}<=seen
previousPath=R/'docs/foundation-node-metadata-reader/evidence/public-api.json'
previous=read(previousPath);current=read(E/'public-api.json');by={a['assembly']:a for a in previous['assemblies']}
for a in current['assemblies']:
 for key in ['declarations','friendAssemblies','references']:assert a[key]==by[a['assembly']][key],(a['assembly'],key)
 assert a['sha256'].lower()==pins['engines'][a['assembly']]['sha256']
assert len(current['assemblies'])==11 and sum(a['publicTypes'] for a in current['assemblies'])==310 and sum(a['declarationCount'] for a in current['assemblies'])==6369
assert read(E/'API-COMPARISON.json')['previousInventorySha256']==sha(previousPath.read_bytes())
for label in ['qt3','qt4']:assert (E/(label+'-skips.txt')).read_bytes()==(R/'docs/foundation-node-metadata-reader/evidence'/(label+'-skips.txt')).read_bytes()
assert s['publicNodeNilledActivated'] and s['skipMapsUnchanged'] and s['publicMetadataUnchanged']
for key in ['newPublicSignatures','factsCacheIntroduced','dataAtomizationActivated','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized','pushPerformed']:assert s[key] is False
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,digest in m.items():assert sha((P/name).read_bytes())==digest,name
print('Verified TA-14: 25306 tests / 76 new, NP01-NP24, exact gates/skips/API/bindings. Guard cost measured; native/owner/freeze/publication pending.')
