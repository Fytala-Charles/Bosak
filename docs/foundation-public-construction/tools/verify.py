"""Verify bounded public construction, actual lifetime routes and exact retained candidate cohorts."""
from pathlib import Path
import hashlib,json,re,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda b:hashlib.sha256(b).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
s=read(P/'STATUS.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json');adoption=read(P/'ADOPTION.json')
assert s['sourceVersion']==pins['sourceVersion']==adoption['candidateVersion']=='1.66.0'
assert '<Version>1.66.0</Version>' in (R/'src/Directory.Build.props').read_text()
for name,digest in adoption['beforePins'].items():
 assert sha((R/name).read_bytes())==digest,name
 if name.endswith('MANIFEST.json'):
  for f,h in read(R/name).items():assert sha((R/name).parent.joinpath(f).read_bytes())==h,(name,f)
assert sha((R/adoption['priorSourcePins']).read_bytes())==adoption['priorSourcePinsSha256']
old=read(R/adoption['priorSourcePins'])
for name,digest in pins['sources'].items():assert sha((R/name).read_bytes())==digest,name
changed={n for n,h in pins['sources'].items() if old['sources'].get(n)!=h}
assert changed=={'src/Directory.Build.props','src/Bosak.XPath.Api/Schema/ConstructedAtomicPlan.cs','src/Bosak.XPath.Api/Schema/SchemaAdvisorBinding.cs','src/Bosak.XPath.Api/Schema/SchemaQuerySession.cs','src/Bosak.XPath.Runtime/Vm/QuerySourceSession.cs','src/Bosak.XPath.Runtime/Vm/QueryProviderOperationScope.cs','src/Bosak.XPath.Runtime/Vm/EvaluationContext.cs','tests/Bosak.XPath.Runtime.Tests/PublicConstructionTests.cs'},changed
cohorts={'initial':read(E/'INITIAL-PINS.json'),'second':read(E/'SECOND-SOURCE-AND-BINARY-PINS.json'),'qualified':pins}
assert {n for n,h in cohorts['initial']['sources'].items() if cohorts['second']['sources'][n]!=h}=={'src/Bosak.XPath.Api/Schema/SchemaQuerySession.cs'}
assert {n for n,h in cohorts['second']['sources'].items() if pins['sources'][n]!=h}=={'src/Bosak.XPath.Api/Schema/ConstructedAtomicPlan.cs','tests/Bosak.XPath.Runtime.Tests/PublicConstructionTests.cs'}
for label,archive in [('initial','INITIAL-ENGINE-BINARIES.zip'),('second','SECOND-ENGINE-BINARIES.zip'),('qualified','ENGINE-BINARIES.zip')]:
 with zipfile.ZipFile(E/archive) as z:
  for name,row in cohorts[label]['engines'].items():
   assert sha(z.read(name+'.dll'))==row['sha256'],(label,name)
   if label=='qualified':assert sha(Path(row['path']).read_bytes())==row['sha256']
for label,archive in [('initial','INITIAL-SOURCE.zip'),('second','SECOND-CHANGED-SOURCE.zip'),('qualified','CHANGED-SOURCE.zip')]:
 with zipfile.ZipFile(E/archive) as z:
  for name in z.namelist():assert sha(z.read(name))==cohorts[label]['sources'][name]
  assert set(z.namelist())==changed
with zipfile.ZipFile(E/'TEST-CONFIGURATION.zip') as z:
 for name,digest in pins['configuration'].items():assert sha(z.read(name))==sha((R/name).read_bytes())==digest
assert pins['configurationName']=='Release' and pins['targetFramework']=='net10.0'
assert sha(Path(pins['observerPath']).read_bytes())==pins['observerSha256']
for name,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']==''
units=read(E/'UNIT-RESULTS.json');actual=[];allocation={}
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for label,expected in [('initial',25608),('second',25608),('qualified',25610)]:
  count=0
  for row in units[label]['projects']:
   raw=z.read(row['trx']);assert sha(raw)==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib
   assert int(c['passed'])==row['passed'] and c['failed']==c['notExecuted']=='0';count+=int(c['passed'])
   if label=='qualified':
    assert sha(Path(row['testBinaryPath']).read_bytes())==row['testBinarySha256']
    actual.extend(n.attrib for n in root.findall('.//{*}UnitTestResult'))
    for n in root.iter():
     if n.tag.endswith('StdOut') and n.text:
      m=re.search(r'Constructed warm identity: (\d+) bytes/query; constructions: (\d+); admissions: (\d+)',n.text)
      if m:allocation=dict(zip(['warmBytesPerQuery','constructions','admissions'],map(int,m.groups())))
  assert len(units[label]['projects'])==10 and count==units[label]['passed']==expected
 for name in z.namelist():
  if not name.endswith('.log.json'):continue
  receipt=json.loads(z.read(name));assert sha(z.read(name[:-5]))==receipt['logSha256'] and receipt['failure'] is None
  assert receipt['exitCode']==(1 if name=='public-construction-focused-final.log.json' else 0),name
 for prefix in ['','final-','qualified-']:
  for name,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
   log=z.read('public-construction-'+prefix+name+'.log').decode('utf-8-sig')
   assert tuple(int(re.search(k+r':\s+(\d+)',log).group(1)) for k in ['Passed','Failed','Skipped'])==expected
assert len(actual)==s['unitTestsPassed']==25610 and all(r['outcome']=='Passed' for r in actual)
with zipfile.ZipFile(E/'TEST-ASSEMBLIES.zip') as z:
 for row in units['qualified']['projects']:assert sha(z.read(row['testAssembly']+'.dll'))==row['testBinarySha256']
acceptance=read(E/'ACCEPTANCE.json');new=[r for r in actual if '.PublicConstructionTests.' in r['testName']];life=[r for r in actual if '.QueryProviderOperationScopeTests.' in r['testName']]
assert len(new)==s['newPublicTestsPassed']==acceptance['newTestsPassed']==139 and new==acceptance['results']
assert len(life)==18 and life==acceptance['lifetimeResults']
assert allocation==s['allocation']==acceptance['allocation'] and allocation['constructions']==allocation['admissions']==1
for file,prefix,count in [('CASE-MAPPING.json','SC',40),('LIFETIME-MAPPING.json','LC',16)]:
 mapping=read(P/file);assert mapping['implementationStatus']=='BoundedPublicQualified' and not mapping['nativeProviderAccepted']
 assert [r['id'] for r in mapping['cases']]==[prefix+str(n).zfill(2) for n in range(1,count+1)]
 for c in mapping['cases']:
  assert c['status']=='QualifiedBoundedPublicScope' and c['tests']
  for pattern in c['tests']:assert any(pattern in r['testName'] and r['outcome']=='Passed' for r in actual),pattern
api=read(E/'public-api.json');previous=read(R/'docs/foundation-provider-operation-lifetime/evidence/public-api.json');by={a['assembly']:a for a in previous['assemblies']}
for a in api['assemblies']:
 for key in ['declarations','friendAssemblies','references']:assert a[key]==by[a['assembly']][key],(a['assembly'],key)
 assert a['sha256'].lower()==pins['engines'][a['assembly']]['sha256']
assert len(api['assemblies'])==11 and sum(a['publicTypes'] for a in api['assemblies'])==310 and sum(a['declarationCount'] for a in api['assemblies'])==6369
assert read(E/'API-COMPARISON.json')['previousInventorySha256']==sha((R/'docs/foundation-provider-operation-lifetime/evidence/public-api.json').read_bytes())
bindings=read(E/'LOADED-BINDINGS.json');assert bindings['allMatchExactCohorts']
for group,expected in bindings['groups'].items():
 cohort=cohorts['qualified' if group.startswith('qualified-') else 'second' if group.startswith('final-') else 'initial'];seen=set();tests={}
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for a in b['assemblies']:
   if a['name'] in cohort['engines']:
    assert a['sha256']==cohort['engines'][a['name']]['sha256'] and a['context']=='Default' and not a['collectible']
    seen.add(a['name'])
   if a['name'].endswith('.Tests'):tests[a['name']]=a['sha256']
 assert sorted(seen)==expected and {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen
 if group in ['units','final-units','qualified-units']:
  label={'units':'initial','final-units':'second','qualified-units':'qualified'}[group]
  for row in units[label]['projects']:assert tests[row['testAssembly']]==row['testBinarySha256']
for name in ['qt3','qt4']:
 for prefix in ['','final-','qualified-']:assert (E/(prefix+name+'-skips.txt')).read_bytes()==(R/'docs/foundation-provider-operation-lifetime/evidence'/(name+'-skips.txt')).read_bytes()
assert s['constructionProfileActivated'] and s['selectedAcceptanceCasesQualified']==40 and s['lifetimeCasesQualifiedBoundedPublic']==16
for key in ['newPublicSignatures','newRequiredProviderMembers','sourceSnapshotsIntroduced','persistentReceiptRegistry','broaderPersistentSignatureNeedsClosed','nativeProviderQualified','ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized','pushPerformed']:assert s[key] is False,key
assert s['sourceCacheSlotsConsumed']==0
if (E/'DOCUMENTATION-CHECKS.json').exists():
 for row in read(E/'DOCUMENTATION-CHECKS.json')['pages']:assert sha((R/row['source']).read_bytes())==row['sourceSha256'],row['source']
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for name,digest in m.items():assert sha((P/name).read_bytes())==digest,name
print('Verified TA-20: 25610 tests / 139 public controls, SC01-SC40 and LC01-LC16; exact three cohorts/gates/skips/API/configuration/bindings. Native/owner/budget/freeze/publication pending.')
