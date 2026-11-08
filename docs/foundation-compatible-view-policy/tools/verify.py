"""Verify inactive compatible-view selection, unchanged candidate controls and package preflight."""
from pathlib import Path
import hashlib,json,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
i=read(P/'INPUTS.json');s=read(P/'STATUS.json');prior=R/'docs/foundation-public-construction'
assert i['sourceVersion']==s['sourceVersion']=='1.66.0'
for name,digest in i['reviewInputs'].items():assert sha((R/name).read_bytes())==digest,name
for name,digest in read(prior/'MANIFEST.json').items():assert sha((prior/name).read_bytes())==digest,name
pins=read(prior/'evidence/SOURCE-AND-BINARY-PINS.json')
for name,digest in pins['sources'].items():assert sha((R/name).read_bytes())==digest,name
for row in pins['engines'].values():assert sha(Path(row['path']).read_bytes())==row['sha256']
assert '<Version>1.66.0</Version>' in (R/'src/Directory.Build.props').read_text()
for file,prefix,count in [('PREREQUISITE-CASES.json','MC',16),('ACCEPTANCE-CASES.json','CV',32)]:
 data=read(P/file);assert data['implementationStatus']=='NotStarted'
 assert [c['id'] for c in data['cases']]==[prefix+str(n).zfill(2) for n in range(1,count+1)]
 assert all(c['status']=='NotRun' and c['trigger'] and c['expected'] for c in data['cases'])
assert s['openSignatureNeed']=='CV-ENTRY-01' and s['newPublicSignatureProposed'] and not s['publicSignatureAdopted']
assert 'OpenCompatibleSnapshot' not in (R/'src/Bosak.XPath.Api/Schema/SchemaQuerySession.cs').read_text()
controls=read(E/'CONTROLS.json');total=0
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in controls['projects']:
  raw=z.read(row['trx']);assert sha(raw)==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib
  assert c['failed']==c['notExecuted']=='0' and int(c['passed'])==row['passed'];total+=int(c['passed'])
  assert [r.attrib for r in root.findall('.//{*}UnitTestResult')]==controls['results'][row['group']]
 for name in ['runtime','api','preflight-tests','prior-proof']:
  filename='compatible-policy-'+name+'.log';receipt=json.loads(z.read(filename+'.json'))
  assert sha(z.read(filename))==receipt['logSha256'] and receipt['exitCode']==0 and receipt['failure'] is None
 assert 'Ran 5 tests' in z.read('compatible-policy-preflight-tests.log').decode('utf-8-sig')
 assert 'Verified TA-20: 25610 tests / 139 public controls' in z.read('compatible-policy-prior-proof.log').decode('utf-8-sig')
assert total==controls['existingControlsPassed']==s['existingControlsPassed']==195
assert len(controls['results']['runtime'])==157 and len(controls['results']['api'])==38
for group in ['runtime','api']:
 seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for a in b['assemblies']:
   if a['name'] in pins['engines']:
    assert a['sha256']==pins['engines'][a['name']]['sha256'] and a['context']=='Default' and not a['collectible'];seen.add(a['name'])
 assert {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen
pre=read(E/'PACKAGE-PREFLIGHT.json');assert pre['sourceVersion']=='1.66.0' and len(pre['projects'])==11
selected={'Bosak.XPath.Core','Bosak.XPath.Parser','Bosak.XPath.Compiler','Bosak.XPath.Runtime','Bosak.XPath.Standard','Bosak.XPath.Providers','Bosak.XPath.Api'}
assert set(pre['selectedPackages'])==selected and len(pre['dirtyQualifiedInputs'])==20 and not pre['candidateCommitted']
assert pre['targetPreviewVersion'] is None and pre['releaseDate'] is None and pre['overlayNoticeIncluded']
assert set(pre['stockNoticeMissing'])==selected
assert not pre['packagesBuilt'] and not pre['packageConsumersRun'] and not pre['publicationAuthorized'] and not pre['apiFrozen']
for row in pre['commands']:
 raw=(P/row['evaluation']).read_bytes();assert sha(raw)==row['sha256'];argv=row['argv']
 assert argv[:2]==['dotnet','msbuild'] and any(a.startswith('-getProperty:') for a in argv)
 assert not any(a.startswith(('-target:','-t:')) for a in argv)
for name,row in pre['projects'].items():
 assert sha((R/row['project']).read_bytes())==pins['sources'][row['project']]
 if name in selected:
  base=read(E/'package-evaluations'/(name+'.json'));overlay=read(E/'package-evaluations'/(name+'-overlay.json'))
  assert row['properties']==base['Properties']==overlay['Properties']
  assert row['properties']['Version']==row['properties']['PackageVersion']=='1.66.0'
  assert not row['stockNoticeIncluded'] and any(a['source']=='NOTICE' for a in row['overlayAssets'])
  assert set(a['source'] for a in row['overlayAssets'])==set(a['source'] for a in row['stockAssets'])|{'NOTICE'}
 for asset in row.get('overlayAssets',row['stockAssets']):assert sha((R/asset['source']).read_bytes())==asset['sha256']
assert any(a['source']=='src/Bosak.XPath.Standard/Resources/Unicode17-LICENSE.txt' for a in pre['projects']['Bosak.XPath.Standard']['overlayAssets'])
closure=set();todo=['Bosak.XPath.Api']
while todo:
 n=todo.pop()
 if n in closure:continue
 closure.add(n);todo+=pre['projects'][n]['projectReferences']
assert closure==selected
recipe=read(P/'packaging/COMMANDS.json');assert recipe['status']=='TemplateNotExecuted' and recipe['qualifiedCommit'] is recipe['previewVersion'] is recipe['releaseDate'] is None and not recipe['publicationCommandIncluded']
assert all(any('<' in str(a) for a in cmd) for cmd in recipe['commands'])
subprocess.run(['python','-B','-m','unittest','discover','-s',str(P/'tools'),'-p','test_preflight.py'],check=True,capture_output=True)
for key in ['compatibleProfileActivated','implementationStarted','publicSignatureAdopted','multiAdvisorPrerequisiteImplemented','packagesBuilt','packageConsumersRun','sourceChanged','binariesChanged','fullGatesRerun','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized','pushPerformed']:assert s[key] is False,key
assert s['multiAdvisorCasesNotRun']==16 and s['compatibleCasesNotRun']==32 and s['fullUnitTestsHistorical']==25610
if (E/'DOCUMENTATION-CHECKS.json').exists():
 for row in read(E/'DOCUMENTATION-CHECKS.json')['pages']:assert sha((R/row['source']).read_bytes())==row['sourceSha256'],row['source']
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,digest in m.items():assert sha((P/name).read_bytes())==digest,name
print('Verified TA-21: 195 unchanged-binary controls + 5 preflight controls; 11 projects / seven-package closure / NOTICE overlay. MC16/CV32 NotRun; CV-ENTRY-01 not adopted. No source/version/package/freeze/publication change.')
