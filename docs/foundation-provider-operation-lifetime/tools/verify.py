"""Verify TA-19 actual internal operation lifetime qualification and candidate identities."""
from pathlib import Path
import hashlib,json,re,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
s=read(P/'STATUS.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json');adoption=read(P/'ADOPTION.json')
assert s['sourceVersion']==pins['sourceVersion']==adoption['candidateVersion']=='1.65.0'
assert '<Version>1.65.0</Version>' in (R/'src/Directory.Build.props').read_text()
for dossier in ['foundation-public-node-data','foundation-sourcefree-construction-policy']:
 old=R/'docs'/dossier
 for name,digest in read(old/'MANIFEST.json').items():assert sha((old/name).read_bytes())==digest,(dossier,name)
assert sha((R/'docs/foundation-sourcefree-construction-policy/MANIFEST.json').read_bytes())==adoption['beforeHashes']['docs/foundation-sourcefree-construction-policy/MANIFEST.json']
previous=read(R/'docs/foundation-public-node-data/evidence/SOURCE-AND-BINARY-PINS.json')
for name,digest in adoption['beforeHashes'].items():
 if name in previous['sources']:assert digest==previous['sources'][name],name
for name,digest in pins['sources'].items():assert sha((R/name).read_bytes())==digest,name
changed={n for n,h in pins['sources'].items() if previous['sources'].get(n)!=h}
assert changed=={'src/Directory.Build.props','src/Bosak.XPath.Runtime/Vm/QuerySourceSession.cs','src/Bosak.XPath.Runtime/Vm/SchemaOperationLeaseSet.cs','src/Bosak.XPath.Runtime/Vm/QueryProviderOperationScope.cs','tests/Bosak.XPath.Runtime.Tests/QueryProviderOperationScopeTests.cs'},changed
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip') as z:
 for name,row in pins['engines'].items():assert sha(z.read(name+'.dll'))==sha(Path(row['path']).read_bytes())==row['sha256']
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 assert set(z.namelist())==changed
 for name in z.namelist():assert sha(z.read(name))==pins['sources'][name]
for name,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']==''
units=read(E/'UNIT-RESULTS.json');actual=[]
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in units['projects']:
  raw=z.read(row['trx']);assert sha(raw)==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib
  assert int(c['passed'])==row['passed'] and c['failed']==c['notExecuted']=='0'
  assert sha(Path(row['testBinaryPath']).read_bytes())==row['testBinarySha256']
  actual.extend(n.attrib for n in root.findall('.//{*}UnitTestResult'))
 for name in z.namelist():
  if not name.endswith('.log.json'):continue
  receipt=json.loads(z.read(name));assert sha(z.read(name[:-5]))==receipt['logSha256'] and receipt['failure'] is None
  assert receipt['exitCode']==(1 if name=='provider-lifetime-units.log.json' else 0),name
 for name,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('provider-lifetime-'+name+'-final.log').decode('utf-8-sig')
  assert tuple(int(re.search(k+r':\s+(\d+)',log).group(1)) for k in ['Passed','Failed','Skipped'])==expected
assert len(units['projects'])==10 and len(actual)==sum(r['passed'] for r in units['projects'])==s['unitTestsPassed']==25471
assert all(r['outcome']=='Passed' for r in actual)
new=[r for r in actual if '.QueryProviderOperationScopeTests.' in r['testName']]
assert len(new)==s['newInternalLifetimeTestsPassed']==18 and new==read(E/'QUALIFICATION.json')['results']
for n in [1,*range(3,17)]:
 pattern='LC04_LC09_LC16_ActualCallTail' if n in range(4,10) else 'LC'+str(n).zfill(2)
 assert any(pattern in r['testName'] for r in new),n
assert s['publicCapabilityCaseLC02']==s['actualConstructorEvidenceIntegration']=='NotRun' and s['constructionCasesNotRun']==40
api=read(E/'public-api.json');old=read(R/'docs/foundation-public-node-data/evidence/public-api.json');by={a['assembly']:a for a in old['assemblies']}
for a in api['assemblies']:
 for key in ['declarations','friendAssemblies','references']:assert a[key]==by[a['assembly']][key],(a['assembly'],key)
 assert a['sha256'].lower()==pins['engines'][a['assembly']]['sha256']
assert len(api['assemblies'])==11 and sum(a['publicTypes'] for a in api['assemblies'])==310 and sum(a['declarationCount'] for a in api['assemblies'])==6369
bindings=read(E/'LOADED-BINDINGS.json');assert bindings['allMatchExactCandidate']
for group,expected in bindings['groups'].items():
 seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for a in b['assemblies']:
   if a['name'] in pins['engines']:
    assert a['sha256']==pins['engines'][a['name']]['sha256'] and a['context']=='Default' and not a['collectible']
    seen.add(a['name'])
 assert sorted(seen)==expected and {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen
for name in ['qt3','qt4']:assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-public-node-data/evidence'/(name+'-skips.txt')).read_bytes()
for key in ['publicConstructorActivated','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized','pushPerformed']:assert s[key] is False,key
if (E/'DOCUMENTATION-CHECKS.json').exists():
 docs=read(E/'DOCUMENTATION-CHECKS.json')
 for row in docs['pages']:assert sha((R/row['source']).read_bytes())==row['sourceSha256'],row['source']
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,digest in m.items():assert sha((P/name).read_bytes())==digest,name
print('Verified TA-19: 25471 tests / 18 internal lifetime controls; exact gates/skips/API/bindings. Public construction, actual constructor-tail integration, native/freeze/publication pending.')
