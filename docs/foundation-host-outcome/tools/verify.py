"""Verify the bounded AH-03 API host outcome prerequisite and retained qualification evidence."""
from pathlib import Path
import hashlib,json,zipfile,re,subprocess,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
s=read(P/'STATUS.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json');a=read(P/'ADOPTION.json')
assert s['sourceVersion']==pins['sourceVersion']==a['candidateVersion']=='1.75.0' and a['beforeVersion']=='1.74.0'
assert not any(s[k] for k in ['publicApiChanged','ah03Implemented','publicXsltBcapActivated','packagesBuilt','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized','pushPerformed'])
old=read(R/'docs/foundation-xslt-completion/evidence/SOURCE-AND-BINARY-PINS.json')
for n,h in a['beforeHashes'].items():
 assert h==old['sources'].get(n,sha((R/n).read_bytes())),n
for n,h in pins['sources'].items():assert sha((R/n).read_bytes())==h,n
changed={n for n,h in pins['sources'].items() if old['sources'].get(n)!=h}
assert changed=={'src/Directory.Build.props','src/Bosak.XPath.Api/Schema/SchemaQuerySession.cs','src/Bosak.XPath.Api/Schema/CapturedHostBinding.cs','tests/Bosak.XPath.Runtime.Tests/CapturedHostBindingTests.cs'},changed
for dossier in ['foundation-xslt-completion','foundation-xslt-admission','foundation-public-xquery','foundation-host-operation-bridge','foundation-r3-scope']:
 base=R/'docs'/dossier
 for n,h in read(base/'MANIFEST.json').items():assert sha((base/n).read_bytes())==h,(dossier,n)
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip') as z:
 for n,row in pins['engines'].items():assert sha(z.read(n+'.dll'))==sha(Path(row['path']).read_bytes())==row['sha256']
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 assert set(z.namelist())==changed
 for n in z.namelist():assert sha(z.read(n))==pins['sources'][n]
with zipfile.ZipFile(E/'TEST-CONFIGURATION.zip') as z:
 for n,h in pins['configuration'].items():assert sha(z.read(n))==sha((R/n).read_bytes())==h
assert sha(Path(pins['observerPath']).read_bytes())==pins['observerSha256']
for n,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/n,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/n,text=True).strip()==row['status']==''
plan=read(P/'PLAN.json');assert plan['sourceVersion']=='1.75.0' and plan['nextCoreTask']=='AH-03' and plan['noRequirementWaivers']
assert next(t for t in plan['work'] if t['id']=='AH-03')['status']=='InProgressInternalHostOutcomePrerequisite'
units=read(E/'UNIT-RESULTS.json');rows=[]
assert len(units['projects'])==10 and units['passed']==s['unitTestsPassed']==25987
with zipfile.ZipFile(E/'TEST-ASSEMBLIES.zip') as z:
 for row in units['projects']:assert sha(z.read(row['testAssembly']+'.dll'))==row['testBinarySha256']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 required={'host-outcome-'+name+'.log.json' for name in ['build-qualified','units','qt3','qt4','ls-build','api-audit']}
 assert required<=set(z.namelist()),required-set(z.namelist())
 for row in units['projects']:
  raw=z.read(row['trx']);assert sha(raw)==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib
  assert c['failed']==c['notExecuted']=='0' and int(c['passed'])==row['passed']
  assert sha(Path(row['testBinaryPath']).read_bytes())==row['testBinarySha256']
  rows.extend(n.attrib for n in root.findall('.//{*}UnitTestResult'))
 for name in z.namelist():
  if not name.endswith('.log.json'):continue
  receipt=json.loads(z.read(name));assert sha(z.read(name[:-5]))==receipt['logSha256']
  assert receipt['exitCode']==(1 if name=='host-outcome-build.log.json' else 0) and receipt['failure'] is None,name
 for name,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('host-outcome-'+name+'.log').decode('utf-8-sig')
  assert tuple(int(re.search(label+r':\s+(\d+)',log).group(1)) for label in ['Passed','Failed','Skipped'])==expected
assert len(rows)==25987 and all(row['outcome']=='Passed' for row in rows)
accept=read(E/'ACCEPTANCE.json');new=[row for row in rows if '.CapturedHostBindingTests.HR' in row['testName']]
assert accept['newTestsPassed']==len(new)==23 and accept['results']==new
api=read(E/'public-api.json');previous=read(R/'docs/foundation-xslt-completion/evidence/public-api.json')
assert len(api['assemblies'])==11
for x,y in zip(api['assemblies'],previous['assemblies'],strict=True):
 for key in ['project','assembly','references','friendAssemblies','publicTypes','declarationCount','declarations']:assert x[key]==y[key],(x['assembly'],key)
for name in ['qt3','qt4']:assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-xslt-completion/evidence'/(name+'-final-skips.txt')).read_bytes()
for group in ['units','qt3','qt4']:
 seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for x in b['assemblies']:
   if x['name'] in pins['engines']:
    assert x['sha256']==pins['engines'][x['name']]['sha256'] and x['context']=='Default' and not x['collectible']
    seen.add(x['name'])
 assert {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen
package=read(E/'PACKAGE-PREFLIGHT.json');assert package['sourceVersion']=='1.75.0' and len(package['selectedPackages'])==8 and len(package['projects'])==11
assert not package['packagesBuilt'] and not package['publicationAuthorized'] and package['overlayNoticeIncluded']
for n,h in package['assets'].items():assert sha((R/n).read_bytes())==h,n
for row in package['commands']:assert sha((P/row['evaluation']).read_bytes())==row['sha256']
for row in read(E/'DOCUMENTATION-CHECKS.json')['pages']:assert sha((R/row['source']).read_bytes())==row['sourceSha256']
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for n,h in m.items():assert sha((P/n).read_bytes())==h,n
print('Verified TA-31: 25987/0, 23 host routing controls, exact candidate hashes, unchanged public API/layout/friends/references and skip maps. AH-03/publication remain pending.')
