"""Verify TA-28 actual internal paired lifetimes, retained sources and exact candidate identities."""
from pathlib import Path
import hashlib,json,re,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
s=read(P/'STATUS.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json');adoption=read(P/'ADOPTION.json');old=read(R/adoption['priorSourcePins'])
assert s['sourceVersion']==pins['sourceVersion']==adoption['candidateVersion']=='1.72.0' and adoption['beforeVersion']=='1.71.0'
for name,digest in adoption['beforeHashes'].items():
 if name in old['sources']:assert old['sources'][name]==digest,name
 else:assert sha((R/name).read_bytes())==digest,name
for dossier in ['foundation-host-operation-bridge','foundation-r3-scope','foundation-public-compatible','foundation-compatible-cache','foundation-compatible-admission','foundation-paired-provider-lifetime','foundation-public-construction']:
 base=R/'docs'/dossier
 for name,digest in read(base/'MANIFEST.json').items():assert sha((base/name).read_bytes())==digest,(dossier,name)
for name,digest in pins['sources'].items():assert sha((R/name).read_bytes())==digest,name
changed={n for n,h in pins['sources'].items() if old['sources'].get(n)!=h}
assert changed=={'src/Directory.Build.props','src/Bosak.XPath.Api/Schema/CapturedHostBinding.cs','src/Bosak.XPath.Api/Schema/CapturedQueryResult.cs','src/Bosak.XPath.Api/Schema/SchemaQuerySession.cs','src/Bosak.XQuery/Api/CapturedQueryHost.cs','src/Bosak.XQuery/Api/XQueryExecutable.cs','src/Bosak.XQuery/Api/Schema/SchemaXQueryCompiler.cs','tests/Bosak.XPath.Runtime.Tests/CapturedQueryResultTests.cs','tests/Bosak.XQuery.Tests/PublicQuerySessionTests.cs'},changed
assert '<Version>1.72.0</Version>' in (R/'src/Directory.Build.props').read_text()
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
  assert receipt['exitCode']==(1 if name in {'public-xquery-focused.log.json','public-xquery-build-results.log.json','public-xquery-result-qualified.log.json'} else 0),name
 for name,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('public-xquery-'+name+'.log').decode('utf-8-sig')
  assert tuple(int(re.search(k+r':\s+(\d+)',log).group(1)) for k in ['Passed','Failed','Skipped'])==expected
 assert 'Verified TA-27: 25882 tests / 51 private host bridge controls + 378 retained' in z.read('public-xquery-prior-proof.log').decode('utf-8-sig')
assert len(units['projects'])==10 and len(actual)==sum(r['passed'] for r in units['projects'])==units['passed']==s['unitTestsPassed']==25936
assert all(r['outcome']=='Passed' for r in actual)
with zipfile.ZipFile(E/'TEST-ASSEMBLIES.zip') as z:
 for row in units['projects']:assert sha(z.read(row['testAssembly']+'.dll'))==row['testBinarySha256']
acceptance=read(E/'ACCEPTANCE.json');new=[r for r in actual if any('.'+c+'.' in r['testName'] for c in ['PublicQuerySessionTests','CapturedQueryResultTests'])];legacy=[r for r in actual if any('.'+c+'.' in r['testName'] for c in ['PublicConstructionTests','QueryProviderOperationScopeTests','PairedProviderOperationScopeTests','RetainedSourceAdmissionTests','CompatibleAdmissionTests','CompatibleCacheTests','PublicCompatibleTests','CapturedHostBindingTests','CapturedHostExecutionTests'])]
assert len(new)==s['newPublicQueryControlsPassed']==acceptance['newTestsPassed']==54 and new==acceptance['results']
assert len(legacy)==s['retainedFoundationControlsPassed']==429 and legacy==acceptance['legacyResults']
assert allocation==acceptance['constructionAllocation'] and allocation['constructions']==allocation['admissions']==1
mapping=read(P/'CASE-MAPPING.json');assert mapping['implementationStatus']=='PublicOrdinaryXQueryQualified' and mapping['publicXQueryActivated']
assert [c['id'] for c in mapping['cases']]==['XQ'+str(n).zfill(2) for n in range(1,32)]
for c in mapping['cases']:
 assert c['status']=='QualifiedPublicOrdinaryXQuery' and c['tests']
 for pattern in c['tests']:
  assert any(pattern in r['testName'] and r['outcome']=='Passed' for r in actual),pattern
api=read(E/'public-api.json');prior=read(R/'docs/foundation-host-operation-bridge/evidence/public-api.json');by={a['assembly']:a for a in prior['assemblies']}
delta=read(P/'API-DELTA.json')['assemblies']
assert set(delta)=={'Bosak.XQuery'} and len(delta['Bosak.XQuery']['added'])==4 and not delta['Bosak.XQuery']['removed']
for a in api['assemblies']:
 b=by[a['assembly']]
 for key in ['friendAssemblies','references']:assert a[key]==b[key],(a['assembly'],key)
 actual=dict(added=sorted(set(a['declarations'])-set(b['declarations'])),removed=sorted(set(b['declarations'])-set(a['declarations'])))
 assert actual==delta.get(a['assembly'],dict(added=[],removed=[])),a['assembly']
 assert a['sha256'].lower()==pins['engines'][a['assembly']]['sha256']
assert len(api['assemblies'])==11 and sum(a['publicTypes'] for a in api['assemblies'])==312 and sum(a['declarationCount'] for a in api['assemblies'])==6386
comparison=read(E/'API-COMPARISON.json')
assert comparison['previousInventorySha256']==sha((R/'docs/foundation-host-operation-bridge/evidence/public-api.json').read_bytes()) and comparison['deltaSha256']==sha((P/'API-DELTA.json').read_bytes())
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
for name in ['qt3','qt4']:assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-host-operation-bridge/evidence'/(name+'-skips.txt')).read_bytes()
pre=read(E/'PACKAGE-PREFLIGHT.json');selected={'Bosak.XPath.Core','Bosak.XPath.Parser','Bosak.XPath.Compiler','Bosak.XPath.Runtime','Bosak.XPath.Standard','Bosak.XPath.Providers','Bosak.XPath.Api','Bosak.XQuery'}
assert pre['sourceVersion']=='1.72.0' and len(pre['projects'])==11 and set(pre['selectedPackages'])==selected
assert len(pre['dirtyQualifiedInputs'])==s['dirtyQualifiedPackageInputs']==41 and not pre['candidateCommitted']
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
  assert row['properties']['Version']==row['properties']['PackageVersion']=='1.72.0'
  assert set(a['source'] for a in row['overlayAssets'])==set(a['source'] for a in row['stockAssets'])|{'NOTICE'}
 for asset in row.get('overlayAssets',row['stockAssets']):assert sha((R/asset['source']).read_bytes())==asset['sha256']
closure=set();todo=['Bosak.XQuery']
while todo:
 n=todo.pop()
 if n in closure:continue
 closure.add(n);todo+=pre['projects'][n]['projectReferences']
assert closure==selected
assert s['ah02Implemented'] and s['publicQueryCasesQualified']==31 and s['publicOrdinaryXQueryActivated'] and s['exactExecutableSessionOperationBinding'] and s['guardedOuterDeferredResults'] and s['effectiveOwnedContextPreserved']
assert s['exportedTypes']==312 and s['declarationRows']==6386 and s['selectedPackageClosure']==8
for key in ['publicSchemaAwareXQueryActivated','structuredQueryResultsActivated','publicXsltBcapActivated','typedHostIntegrationActivated','newRequiredProviderMembers','packagesBuilt','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized','pushPerformed']:assert s[key] is False,key
follow=read(P/'SCOPE-FOLLOWUP.json');assert follow['completedWork']==['AH-01','AH-02'] and follow['nextCoreTask']=='AH-03' and not follow['requirementsWaived'] and not follow['r3Complete']
assert follow['publicProposedTypePresence']==dict(present=41,total=47,missing=6)
scope=read(P/'PACKAGE-SCOPE.json');assert set(scope['selectedPackages'])==selected and scope['rootProject']=='Bosak.XQuery' and scope['currentSelectedClosure']==8 and not scope['packagesBuilt']
if (E/'DOCUMENTATION-CHECKS.json').exists():
 for row in read(E/'DOCUMENTATION-CHECKS.json')['pages']:assert sha((R/row['source']).read_bytes())==row['sourceSha256'],row['source']
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,digest in m.items():assert sha((P/name).read_bytes())==digest,name
print('Verified TA-28: 25936 tests / 54 public XQuery controls + 429 retained; exact gates/skips, selected API delta, bindings/configuration and 1.72.0 eight-package metadata. AH-03 next; broader schema/structured/native/owner/freeze/publication pending.')
