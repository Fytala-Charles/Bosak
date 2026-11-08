from pathlib import Path
import json,hashlib,xml.etree.ElementTree as ET,copy
area=Path('.tools/fx009-02').resolve();results=area/'results'; selection=json.loads((area/'selection.json').read_text(encoding='utf-8')); roles=['source-baseline','source-candidate','binary-control','binary-replacement'];ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
expected_candidate={p['id']:p['assemblySha256'] for p in selection['packages']}
expected_baseline={name:hashlib.sha256((area/'baseline/source/qualification/Probe/bin/Release/net10.0'/(name+'.dll')).read_bytes()).hexdigest() for name in expected_candidate}
def sh(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def text_manifest(p):
 rows={}
 for line in Path(p).read_text(encoding='utf-8').splitlines():
  value,name=line.split('  ',1); assert len(value)==64;rows[name.removeprefix('./')]=value
 return rows
def report(path):
 x=ET.parse(path); counts={k:int(v) for k,v in x.find('.//t:Counters',ns).attrib.items()};assert counts['total']==counts['passed']==27 and counts['failed']==counts['notExecuted']==0,path
 return {'path':str(Path(path).relative_to(area)),'sha256':sh(path),'counts':counts,'testIds':sorted(a.get('testId') for a in x.findall('.//t:UnitTestResult',ns))}
def expected_root(platform,role,kind):
 suffix='tests/Bosak.XmlDb.Xdm.Tests/bin/Release/net10.0' if kind=='tests' else 'qualification/Probe/bin/Release/net10.0'
 if platform=='windows': return (area/(role.removeprefix('source-')+'/source/'+suffix) if role.startswith('source-') else area/'frozen'/role.removeprefix('binary-')/kind).as_posix().lower()
 return '/work/'+(role.removeprefix('source-')+'/source/'+suffix if role.startswith('source-') else 'frozen/'+role.removeprefix('binary-')+'/'+kind)
def bindings(base,platform,role,kind):
 found={};receipts=[]
 for file in sorted((base/(role+'-'+kind+'-bindings')).glob('*.json')):
  x=json.loads(file.read_text(encoding='utf-8'));assert not x['captureErrors']
  observed=[a for a in x['assemblies'] if a['name'].startswith('Bosak.')]
  if not observed:continue
  receipts.append({'path':file.relative_to(area).as_posix(),'sha256':sh(file),'processId':x['processId'],'processName':x['processName']})
  prefix=expected_root(platform,role,kind)
  for a in observed:
   parent=a['path'].replace('\\','/').rsplit('/',1)[0]
   if platform=='windows':parent=parent.lower()
   assert parent==prefix,(platform,role,kind,a)
   assert a['context']=='Default' and a['collectible']=='False',a
   if a['name'] in expected_candidate:
    target=expected_candidate if 'candidate' in role or 'replacement' in role else expected_baseline
    assert a['sha256']==target[a['name']],(platform,role,kind,a)
    found[a['name']]=a['sha256']
 assert found.keys()==expected_candidate.keys(),(platform,role,kind,found)
 return {'engineHashes':found,'defaultContextAndIntendedLayout':True,'receipts':receipts}
all_rows={};same_ids=None;same_observations=None
for platform,base in [('windows',results),('linux',results/'linux')]:
 platform_rows={}
 for role in roles:
  tests=report(base/(role+'-tests/results.trx'))
  if same_ids is None:same_ids=tests['testIds']
  assert tests['testIds']==same_ids,(platform,role)
  path=base/(role+'-observations.json'); observations=json.loads(path.read_text(encoding='utf-8'))['observations'];assert len(observations)==37
  if same_observations is None:same_observations=observations
  assert observations==same_observations,(platform,role,'observations')
  platform_rows[role]={'tests':tests,'probe':{'path':path.relative_to(area).as_posix(),'sha256':sh(path),'observationCount':37},'testBindings':bindings(base,platform,role,'tests'),'probeBindings':bindings(base,platform,role,'probe')}
 if platform=='linux':
  for kind in ['tests','probe']:
   control=text_manifest(base/f'layout-control-{kind}-before.txt');replacement=text_manifest(base/f'layout-replacement-{kind}-before.txt')
   assert control.keys()==replacement.keys()
   assert {name for name in control if control[name]!=replacement[name]}=={name+'.dll' for name in expected_candidate}
   assert control==text_manifest(base/f'layout-control-{kind}-after.txt')
   assert replacement==text_manifest(base/f'layout-replacement-{kind}-after.txt')
  for role in roles:
   assert (base/'store-before.txt').read_bytes()==(base/(role+'-store-after.txt')).read_bytes()
   assert (base/'store-dirs-before.txt').read_bytes()==(base/(role+'-store-dirs-after.txt')).read_bytes()
 all_rows[platform]=platform_rows
# Negative controls mutate detached summaries only, never actual inputs or stores.
claim={'engineHashes':expected_candidate.copy(),'expectedEngineHashes':expected_candidate.copy(),'testTotal':27,'testPassed':27,'observationCount':37,'layoutChanges':sorted(name+'.dll' for name in expected_candidate),'allowedLayoutChanges':sorted(name+'.dll' for name in expected_candidate),'storeBefore':'fixed','storeAfter':'fixed','bindingContext':'Default','bindingLayout':'expected','expectedBindingLayout':'expected'}
def validate(x):
 assert len(x['engineHashes'])==7 and x['engineHashes']==x['expectedEngineHashes']
 assert x['testTotal']==x['testPassed']==27 and x['observationCount']==37
 assert x['layoutChanges']==x['allowedLayoutChanges']
 assert x['storeBefore']==x['storeAfter'] and x['bindingContext']=='Default' and x['bindingLayout']==x['expectedBindingLayout']
validate(claim); controls=[]
for name,mutate in [('wrong-engine-hash',lambda x:x['engineHashes'].__setitem__('Bosak.XPath.Core','0'*64)),('missing-engine',lambda x:x['engineHashes'].pop('Bosak.XPath.Api')),('consumer-rewrite',lambda x:x['layoutChanges'].append('Bosak.XmlDb.Xdm.dll')),('failed-case',lambda x:x.__setitem__('testPassed',26)),('store-mutation',lambda x:x.__setitem__('storeAfter','changed')),('wrong-context',lambda x:x.__setitem__('bindingContext','other')),('wrong-layout',lambda x:x.__setitem__('bindingLayout','other'))]:
 altered=copy.deepcopy(claim);mutate(altered)
 try:validate(altered)
 except AssertionError:controls.append({'name':name,'rejected':True})
 else:raise AssertionError(name)
summary={'scope':'Bounded sequential existing-API XML characterization; no joint acceptance','platforms':all_rows,'distinctExistingTestIds':27,'observationsPerRole':37,'primaryPlatformRoleRuns':8,'testsAcrossRunsOverlap':True,'sameTestIdsAcrossAllRuns':True,'allPrimaryObservationsEqualAcrossPlatformsAndRoles':True,'eachTesthostAndProbeLoadsExactSevenEngineDlls':True,'defaultContextAndIntendedLayout':True,'detachedInvariantSanityControls':controls,'controlsAreNotEndToEndTamperQualification':True,'coreAcceptance':None,'candidateApiFreezeAccepted':False,'newEngineSignatureRequired':False,'concurrentDisposalOrGeneralCancellationQualified':False,'productPinChanged':False}
(results/'primary-verification.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
print('Verified both platforms x four roles: 27 identical IDs, 37 matching observations, actual seven-DLL default-context/layout hashes, unchanged fixtures/layouts; seven negative controls reject invalid claims.')
