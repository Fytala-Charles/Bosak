"""Verify TA-13 candidate identities, original receipts and bounded qualification."""
from pathlib import Path
import hashlib,json,re,subprocess,zipfile,xml.etree.ElementTree as ET

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))

s=read(P/'STATUS.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json');inputs=read(P/'INPUTS.json')
assert s['sourceVersion']==pins['sourceVersion']==inputs['sourceVersion']=='1.62.0'
assert s['baseCommit']==pins['baseCommit']==inputs['baseCommit']
assert sha((R/inputs['priorManifest']).read_bytes())==inputs['priorManifestSha256']
for name,digest in read(R/inputs['priorManifest']).items():
 assert sha((R/inputs['priorManifest']).parent.joinpath(name).read_bytes())==digest,name
for name,digest in pins['sources'].items():assert sha((R/name).read_bytes())==digest,name
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip') as z:
 for name,row in pins['engines'].items():assert sha(Path(row['path']).read_bytes())==row['sha256']==sha(z.read(name+'.dll')),name
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 for name in z.namelist():assert sha(z.read(name))==pins['sources'][name]
initial=read(E/'INITIAL-QUALIFICATION-PINS.json')
assert initial['engines']==pins['engines']
with zipfile.ZipFile(E/'INITIAL-QUALIFICATION-SOURCE.zip') as z:
 for name in z.namelist():
  raw=z.read(name);assert sha(raw)==initial['sources'][name]
  current=(R/name).read_bytes()
  if name.endswith('QueryNodeFactsTests.cs'):
   current=current.replace(b'    /// <returns>A task completing after the interrupted operation has drained.</returns>\r\n',b'')
  assert current==raw,name
for name,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']==''
units=read(E/'UNIT-RESULTS.json');acceptance=read(E/'ACCEPTANCE.json');actual=[];cost=None
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in units['projects']:
  raw=z.read(row['trx']);assert sha(raw)==row['sha256'];r=ET.fromstring(raw);c=r.find('.//{*}Counters').attrib
  assert int(c['passed'])==row['passed'] and c['failed']==c['notExecuted']=='0'
  assert sha((R/'tests'/row['project']/'bin/Release/net10.0'/(row['project']+'.dll')).read_bytes())==row['testBinarySha256']
  actual.extend(n.attrib for n in r.findall('.//{*}UnitTestResult'))
  for n in r.iter():
   if n.tag.endswith('StdOut') and n.text:
    m=re.search(r'Metadata facts cold: (\d+) bytes/read; warm: (\d+) bytes/read; callbacks: (\d+) per observation',n.text)
    if m:cost=dict(zip(['cold','warm','callbacksPerObservation'],map(int,m.groups())))
 for label in ['build','build-final','build-docs-final','runtime-final','language-server','api-audit','database-rerun','qt3','qt4','units','focused']:
  name='node-metadata-'+label+'.log';receipt=json.loads(z.read(name+'.json'))
  assert sha(z.read(name))==receipt['logSha256'] and receipt['failure'] is None
  assert receipt['exitCode']==(1 if label in ['units','focused'] else 0)
 for label,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  text=z.read('node-metadata-'+label+'.log').decode('utf-8-sig')
  assert tuple(int(re.search(k+r':\s+(\d+)',text).group(1)) for k in ['Passed','Failed','Skipped'])==expected
 failed=[]
 for name in z.namelist():
  if name.startswith('node-metadata-unit-trx/') and name.endswith('.trx'):
   failed.extend(n for n in ET.fromstring(z.read(name)).findall('.//{*}UnitTestResult') if n.attrib['outcome']!='Passed')
 assert len(failed)==45 and all('.Providers.Database.Tests.' in n.attrib['testName'] for n in failed)
 assert all('HttpListenerException' in ''.join(n.itertext()) and 'handle is invalid' in ''.join(n.itertext()) for n in failed)
assert len(units['projects'])==10 and sum(r['passed'] for r in units['projects'])==units['passed']==s['unitTestsPassed']==25230
new=[r for r in actual if '.QueryNodeFactsTests.MetadataOnly' in r['testName']]
old=[r for r in actual if '.QueryNodeFactsTests.' in r['testName'] and r not in new]
assert new==acceptance['newCases'] and old==acceptance['existingReaderCases']
assert len(new)==s['newCasesPassed']==37 and len(old)==s['existingReaderCasesPassed']==79
assert all(r['outcome']=='Passed' for r in actual)
assert cost==s['allocation']==acceptance['allocation'] and cost['warm']==0 and cost['callbacksPerObservation']==54
assert 0<cost['cold']<2000000
bindings=read(E/'LOADED-BINDINGS.json')
for group,expected in bindings['groups'].items():
 seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for a in b['assemblies']:
   if a['name'] in pins['engines']:
    assert a['sha256']==pins['engines'][a['name']]['sha256'] and a['context']=='Default' and not a['collectible']
    seen.add(a['name'])
 assert sorted(seen)==expected
 assert {'Bosak.XPath.Core','Bosak.XPath.Runtime'}<=seen
 if group!='database-rerun':assert 'Bosak.XPath.Api' in seen
previous=read(R/'docs/foundation-node-facts-cost/evidence/public-api.json');current=read(E/'public-api.json')
by={a['assembly']:a for a in previous['assemblies']}
for a in current['assemblies']:
 for key in ['declarations','friendAssemblies','references']:assert a[key]==by[a['assembly']][key],(a['assembly'],key)
 assert a['sha256'].lower()==pins['engines'][a['assembly']]['sha256']
assert len(current['assemblies'])==11 and sum(a['publicTypes'] for a in current['assemblies'])==310 and sum(a['declarationCount'] for a in current['assemblies'])==6369
for label in ['qt3','qt4']:assert (E/(label+'-skips.txt')).read_bytes()==(R/'docs/foundation-node-facts-cost/evidence'/(label+'-skips.txt')).read_bytes()
for key in ['internalMetadataOnlyImplemented','internalMetadataOnlyQualified','skipMapsUnchanged','publicMetadataUnchanged']:assert s[key]
for key in ['publicNodeProfileActivated','newPublicSignatures','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized']:assert s[key] is False
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,digest in m.items():assert sha((P/name).read_bytes())==digest,name
print('Verified TA-13: 37 new/79 existing reader cases; combined 25230 tests; exact gates/skips/API/bindings. Public nilled/native/freeze pending.')
