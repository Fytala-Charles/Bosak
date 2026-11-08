"""Verify the bounded TA-40 eleven-kind complete-container checkpoint."""
from pathlib import Path
import hashlib,json,zipfile,re,subprocess,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
s=read(P/'STATUS.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json');a=read(P/'ADOPTION.json')
assert s['sourceVersion']==pins['sourceVersion']==a['candidateVersion']=='1.81.0' and a['beforeVersion']=='1.80.0'
assert not any(s[k] for k in ['pv01Complete','packagesBuilt','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized','pushPerformed'])
old=read(R/'docs/foundation-compatible-container/evidence/SOURCE-AND-BINARY-PINS.json')
for n,h in a['beforeHashes'].items():
 assert h==old['sources'].get(n,sha((R/n).read_bytes())),n
for n,h in pins['sources'].items():assert sha((R/n).read_bytes())==h,n
changed={n for n,h in pins['sources'].items() if old['sources'].get(n)!=h}
assert changed=={'src/Directory.Build.props','src/Bosak.XPath.Api/Schema/SchemaQuerySession.cs','src/Bosak.XPath.Api/Schema/CompleteTypedContainer.cs','tests/Bosak.XPath.Runtime.Tests/PublicTypedIdentityTests.cs','tests/Bosak.XPath.Runtime.Tests/PublicCompatibleTests.cs'},changed
for dossier in ['foundation-primitive-policy','foundation-compatible-container','foundation-public-container','foundation-container-policy','foundation-leased-provenance','foundation-provenance-policy','foundation-public-xslt','foundation-host-outcome','foundation-xslt-completion','foundation-xslt-admission','foundation-public-xquery','foundation-host-operation-bridge','foundation-r3-scope']:
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
plan=read(P/'PLAN.json');assert plan['sourceVersion']=='1.81.0' and plan['nextCoreTask']=='PV-01' and plan['noRequirementWaivers']
assert plan['nextSlice']=='PV-T06'
units=read(E/'UNIT-RESULTS.json');rows=[];costs=[]
assert len(units['projects'])==10 and units['passed']==s['unitTestsPassed']==26338
with zipfile.ZipFile(E/'TEST-ASSEMBLIES.zip') as z:
 for row in units['projects']:assert sha(z.read(row['testAssembly']+'.dll'))==row['testBinarySha256']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 required={'public-primitives-'+name+'.log.json' for name in ['build','units','qt3','qt4','ls-build','api-audit']}
 assert required<=set(z.namelist()),required-set(z.namelist())
 for row in units['projects']:
  raw=z.read(row['trx']);assert sha(raw)==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib
  assert c['failed']==c['notExecuted']=='0' and int(c['passed'])==row['passed']
  assert sha(Path(row['testBinaryPath']).read_bytes())==row['testBinarySha256']
  rows.extend(n.attrib for n in root.findall('.//{*}UnitTestResult'))
  for n in root.iter():
   if n.tag.endswith('StdOut') and n.text:
    m=re.search(r'Provenance warm observation: (\d+) caller bytes / 1000 observations with three getters',n.text)
    if m:costs.append(int(m.group(1)))
 for name in z.namelist():
  if not name.endswith('.log.json'):continue
  receipt=json.loads(z.read(name));assert sha(z.read(name[:-5]))==receipt['logSha256']
  assert receipt['exitCode']==0 and receipt['failure'] is None,name
 for name,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('public-primitives-'+name+'.log').decode('utf-8-sig')
  assert tuple(int(re.search(label+r':\s+(\d+)',log).group(1)) for label in ['Passed','Failed','Skipped'])==expected
assert len(rows)==26338 and all(row['outcome']=='Passed' for row in rows)
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 focused=[n for n in z.namelist() if n.startswith('focused-trx/') and n.endswith('.trx')];assert len(focused)==1
 root=ET.fromstring(z.read(focused[0]));counts=root.find('.//{*}Counters').attrib
 assert int(counts['passed'])==189 and counts['failed']==counts['notExecuted']=='0'
 assert {x.attrib['testName'] for x in root.findall('.//{*}UnitTestResult')}<={r['testName'] for r in rows}
accept=read(E/'ACCEPTANCE.json');new=[row for row in rows if '.PublicTypedIdentityTests.PW' in row['testName'] or '.PublicCompatibleTests.PW' in row['testName']]
assert accept['newTestsPassed']==len(new)==102 and accept['results']==new
cost=read(E/'COST-OBSERVATION.json');assert set(cost['observations'])=={'ordinary','compatible'} and not cost['ownerBudgetAccepted'] and cost['compatibleWarmTraceCapacityReserved']
assert all(c['warmReads']==2 for c in cost['observations'].values())
mapping=read(P/'CASE-MAPPING.json');assert mapping['total']==sum(c['count'] for c in mapping['cases'])==102
assert {n for c in mapping['cases'] for n in c['tests']}=={r['testName'] for r in new}
assert read(P/'SCOPE-FOLLOWUP.json')['nextSlice']=='PV-T06'
assert read(P/'SIGNATURE-DISPOSITION.json')['ownerAccepted'] is False
selection=read(E/'SELECTION-AUDIT.json');assert selection['selectedKinds']==['Boolean','Date','DateTime','Decimal','Double','Duration','Float','Integer','QName','String','Time']
for n,h in selection['sourceFiles'].items():assert sha((R/n).read_bytes())==h,n
body=(R/'src/Bosak.XPath.Api/Schema/CompleteTypedContainer.cs').read_text(encoding='utf-8')
whitelist=re.search(r'supported &= value.Kind is (.*?);',body,re.S).group(1)
assert sorted(set(re.findall(r'XdmValueKind\.(\w+)',whitelist)))==selection['selectedKinds']
api=read(E/'public-api.json');previous=read(R/'docs/foundation-compatible-container/evidence/public-api.json')
assert len(api['assemblies'])==11
for assembly in api['assemblies']:assert assembly['sha256'].lower()==pins['engines'][assembly['assembly']]['sha256'],assembly['assembly']
delta=read(P/'API-DELTA.json')['assemblies'];assert delta=={}
for x,y in zip(api['assemblies'],previous['assemblies'],strict=True):
 for key in ['project','assembly','references','friendAssemblies']:assert x[key]==y[key],(x['assembly'],key)
 actual=dict(added=sorted(set(x['declarations'])-set(y['declarations'])),removed=sorted(set(y['declarations'])-set(x['declarations'])))
 assert actual==delta.get(x['assembly'],dict(added=[],removed=[]))
assert sum(x['publicTypes'] for x in api['assemblies'])==318 and sum(x['declarationCount'] for x in api['assemblies'])==6465
assert s['pvT05CompleteBounded'] and s['publicFactoryKindsSupported']==11 and not s['enumOnlyKindsImplemented']
for name in ['qt3','qt4']:assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-compatible-container/evidence'/(name+'-skips.txt')).read_bytes()
for group in ['units','qt3','qt4']:
 seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for x in b['assemblies']:
   if x['name'] in pins['engines']:
    assert x['sha256']==pins['engines'][x['name']]['sha256'] and x['context']=='Default' and not x['collectible']
    seen.add(x['name'])
 assert {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen
package=read(E/'PACKAGE-PREFLIGHT.json');assert package['sourceVersion']=='1.81.0' and len(package['selectedPackages'])==9 and len(package['projects'])==11
assert not package['packagesBuilt'] and not package['publicationAuthorized'] and package['overlayNoticeIncluded']
for n,h in package['assets'].items():assert sha((R/n).read_bytes())==h,n
for row in package['commands']:assert sha((P/row['evaluation']).read_bytes())==row['sha256']
for row in read(E/'DOCUMENTATION-CHECKS.json')['pages']:assert sha((R/row['source']).read_bytes())==row['sourceSha256']
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for n,h in m.items():assert sha((P/n).read_bytes())==h,n
print('Verified TA-40:26338/0;102 wider controls/189 focused;eleven public factory kinds;exact binaries/bindings;unchanged declarations/layouts/friends/references/skip maps. Enum-only/legacy/owner/budget/freeze/publication remain open.')
