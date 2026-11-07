"""Verify TA-23 actual internal paired lifetimes, retained sources and exact candidate identities."""
from pathlib import Path
import hashlib,json,re,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
s=read(P/'STATUS.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json');adoption=read(P/'ADOPTION.json');old=read(R/adoption['priorSourcePins'])
assert s['sourceVersion']==pins['sourceVersion']==adoption['candidateVersion']=='1.68.0' and adoption['beforeVersion']=='1.67.0'
for name,digest in adoption['beforeHashes'].items():
 if name in old['sources']:assert old['sources'][name]==digest,name
 else:assert sha((R/name).read_bytes())==digest,name
for dossier in ['foundation-paired-provider-lifetime','foundation-public-construction']:
 base=R/'docs'/dossier
 for name,digest in read(base/'MANIFEST.json').items():assert sha((base/name).read_bytes())==digest,(dossier,name)
for name,digest in pins['sources'].items():assert sha((R/name).read_bytes())==digest,name
changed={n for n,h in pins['sources'].items() if old['sources'].get(n)!=h}
assert changed=={'src/Directory.Build.props','src/Bosak.XPath.Api/Schema/SchemaQuerySession.cs','src/Bosak.XPath.Api/Schema/SchemaAdvisorBinding.cs','src/Bosak.XPath.Api/Schema/CompatibleAdmissionPlan.cs','tests/Bosak.XPath.Runtime.Tests/CompatibleAdmissionTests.cs'},changed
assert '<Version>1.68.0</Version>' in (R/'src/Directory.Build.props').read_text()
assert 'OpenCompatibleSnapshot' not in (R/'src/Bosak.XPath.Api/Schema/SchemaQuerySession.cs').read_text()
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip') as z:
 for name,row in pins['engines'].items():assert sha(z.read(name+'.dll'))==sha(Path(row['path']).read_bytes())==row['sha256']
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip') as z:
 assert set(z.namelist())==changed
 for name in z.namelist():assert sha(z.read(name))==pins['sources'][name]
with zipfile.ZipFile(E/'TEST-CONFIGURATION.zip') as z:
 for name,digest in pins['configuration'].items():assert sha(z.read(name))==sha((R/name).read_bytes())==digest
assert pins['configurationName']=='Release' and pins['targetFramework']=='net10.0'
assert sha(Path(pins['observerPath']).read_bytes())==pins['observerSha256']
for name,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']==''
units=read(E/'UNIT-RESULTS.json');actual=[];allocation={}
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in units['projects']:
  raw=z.read(row['trx']);assert sha(raw)==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib
  assert c['failed']==c['notExecuted']=='0' and int(c['passed'])==row['passed']
  assert sha(Path(row['testBinaryPath']).read_bytes())==row['testBinarySha256']
  actual.extend(n.attrib for n in root.findall('.//{*}UnitTestResult'))
  for n in root.iter():
   if n.tag.endswith('StdOut') and n.text:
    m=re.search(r'Constructed warm identity: (\d+) bytes/query; constructions: (\d+); admissions: (\d+)',n.text)
    if m:allocation=dict(zip(['warmBytesPerQuery','constructions','admissions'],map(int,m.groups())))
 for name in z.namelist():
  if not name.endswith('.log.json'):continue
  receipt=json.loads(z.read(name));assert sha(z.read(name[:-5]))==receipt['logSha256'] and receipt['failure'] is None
  assert receipt['exitCode']==(1 if name=='compatible-admission-build-tests.log.json' else 0),name
 focus=[n for n in z.namelist() if n.startswith('final-focused-trx/') and n.endswith('.trx')];assert len(focus)==1
 root=ET.fromstring(z.read(focus[0]));c=root.find('.//{*}Counters').attrib;assert c['passed']==c['total']=='67' and c['failed']==c['notExecuted']=='0'
 for name,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('compatible-admission-'+name+'.log').decode('utf-8-sig')
  assert tuple(int(re.search(k+r':\s+(\d+)',log).group(1)) for k in ['Passed','Failed','Skipped'])==expected
 assert 'Verified TA-22: 25650 tests / 40 internal controls + 157 legacy' in z.read('compatible-admission-prior-proof.log').decode('utf-8-sig')
assert len(units['projects'])==10 and len(actual)==sum(r['passed'] for r in units['projects'])==units['passed']==s['unitTestsPassed']==25717
assert all(r['outcome']=='Passed' for r in actual)
with zipfile.ZipFile(E/'TEST-ASSEMBLIES.zip') as z:
 for row in units['projects']:assert sha(z.read(row['testAssembly']+'.dll'))==row['testBinarySha256']
acceptance=read(E/'ACCEPTANCE.json');new=[r for r in actual if any('.'+c+'.' in r['testName'] for c in ['CompatibleAdmissionTests'])];legacy=[r for r in actual if any('.'+c+'.' in r['testName'] for c in ['PublicConstructionTests','QueryProviderOperationScopeTests','PairedProviderOperationScopeTests','RetainedSourceAdmissionTests'])]
assert len(new)==s['newInternalTestsPassed']==acceptance['newTestsPassed']==67 and new==acceptance['results']
assert len(legacy)==s['legacyLifetimeConstructionControlsPassed']==197 and legacy==acceptance['legacyResults']
assert allocation==acceptance['constructionAllocation'] and allocation['constructions']==allocation['admissions']==1
mapping=read(P/'CASE-MAPPING.json');assert mapping['implementationStatus']=='InternalAdmissionQualified' and not mapping['publicCompatibilityActivated']
assert [c['id'] for c in mapping['cases']]==['AC'+str(n).zfill(2) for n in range(1,29)]
for c in mapping['cases']:
 assert c['status']=='QualifiedInternalAdmission' and c['tests']
 for pattern in c['tests']:assert any(pattern in r['testName'] and r['outcome']=='Passed' for r in actual),pattern
api=read(E/'public-api.json');prior=read(R/'docs/foundation-paired-provider-lifetime/evidence/public-api.json');by={a['assembly']:a for a in prior['assemblies']}
for a in api['assemblies']:
 for key in ['declarations','friendAssemblies','references']:assert a[key]==by[a['assembly']][key],(a['assembly'],key)
 assert a['sha256'].lower()==pins['engines'][a['assembly']]['sha256']
assert len(api['assemblies'])==11 and sum(a['publicTypes'] for a in api['assemblies'])==310 and sum(a['declarationCount'] for a in api['assemblies'])==6369
assert read(E/'API-COMPARISON.json')['previousInventorySha256']==sha((R/'docs/foundation-paired-provider-lifetime/evidence/public-api.json').read_bytes())
bindings=read(E/'LOADED-BINDINGS.json');assert bindings['allMatchExactCandidate']
for group,expected in bindings['groups'].items():
 seen=set();tests={}
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for a in b['assemblies']:
   if a['name'] in pins['engines']:
    assert a['sha256']==pins['engines'][a['name']]['sha256'] and a['context']=='Default' and not a['collectible'];seen.add(a['name'])
   if a['name'].endswith('.Tests'):tests[a['name'].lower()]=a['sha256']
 assert sorted(seen)==expected and {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen
 if group=='units':
  for row in units['projects']:assert tests[row['testAssembly'].lower()]==row['testBinarySha256']
for name in ['qt3','qt4']:assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-paired-provider-lifetime/evidence'/(name+'-skips.txt')).read_bytes()
pre=read(E/'PACKAGE-PREFLIGHT.json');selected={'Bosak.XPath.Core','Bosak.XPath.Parser','Bosak.XPath.Compiler','Bosak.XPath.Runtime','Bosak.XPath.Standard','Bosak.XPath.Providers','Bosak.XPath.Api'}
assert pre['sourceVersion']=='1.68.0' and len(pre['projects'])==11 and set(pre['selectedPackages'])==selected
assert len(pre['dirtyQualifiedInputs'])==s['dirtyQualifiedPackageInputs']==24 and not pre['candidateCommitted']
assert pre['targetPreviewVersion'] is None and pre['releaseDate'] is None and pre['overlayNoticeIncluded'] and set(pre['stockNoticeMissing'])==selected
assert not pre['packagesBuilt'] and not pre['packageConsumersRun'] and not pre['publicationAuthorized'] and not pre['apiFrozen']
for row in pre['commands']:
 assert sha((P/row['evaluation']).read_bytes())==row['sha256']
 assert row['argv'][:2]==['dotnet','msbuild'] and any(a.startswith('-getProperty:') for a in row['argv']) and not any(a.startswith(('-target:','-t:')) for a in row['argv'])
for name,row in pre['projects'].items():
 assert sha((R/row['project']).read_bytes())==pins['sources'][row['project']]
 if name in selected:
  base=read(E/'package-evaluations'/(name+'.json'));overlay=read(E/'package-evaluations'/(name+'-overlay.json'))
  assert row['properties']==base['Properties']==overlay['Properties']
  assert row['properties']['Version']==row['properties']['PackageVersion']=='1.68.0'
  assert set(a['source'] for a in row['overlayAssets'])==set(a['source'] for a in row['stockAssets'])|{'NOTICE'}
 for asset in row.get('overlayAssets',row['stockAssets']):assert sha((R/asset['source']).read_bytes())==asset['sha256']
closure=set();todo=['Bosak.XPath.Api']
while todo:
 n=todo.pop()
 if n in closure:continue
 closure.add(n);todo+=pre['projects'][n]['projectReferences']
assert closure==selected
assert s['authenticatedPrivateAdmissionImplemented'] and s['freshPrivateRawReproofImplemented'] and s['internalAdmissionCasesQualified']==28
assert s['openSignatureNeed']=='CV-ENTRY-01' and s['nextInternalRequirement']=='CV-CACHE-01' and s['compatiblePublicCasesNotRun']==32
core=read(P/'DISPOSITION.json');assert core['coreDisposition']==s['corePublicDeltaDisposition']=='SelectedForBoundedImplementation'
assert not core['ownerAcceptance'] and not core['c45Acceptance'] and not core['publicDeclarationsAdded'] and not core['apiFrozen']
cache=read(P/'CACHE-CASES.json');assert cache['implementationStatus']=='NotStarted' and [c['id'] for c in cache['cases']]==['CI'+str(n).zfill(2) for n in range(1,17)] and all(c['status']=='NotRun' for c in cache['cases'])
for key in ['publicSignatureAdded','publicSignatureAdopted','compatibleProfileActivated','compatibleIdentityCacheImplemented','newPublicSignatures','newRequiredProviderMembers','sourceSchemaReassessed','typedValueConsumptionActivated','packagesBuilt','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized','pushPerformed']:assert s[key] is False,key
if (E/'DOCUMENTATION-CHECKS.json').exists():
 for row in read(E/'DOCUMENTATION-CHECKS.json')['pages']:assert sha((R/row['source']).read_bytes())==row['sourceSha256'],row['source']
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,digest in m.items():assert sha((P/name).read_bytes())==digest,name
print('Verified TA-23: 25717 tests / 67 private proof-admission controls + 197 legacy; exact gates/skips/API/bindings/configuration and 1.68.0 metadata. Core shape selected; cache/public/native/owner/freeze/publication pending.')
