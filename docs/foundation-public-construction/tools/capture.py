"""Capture actual TA-20 final and historical cohorts; refuses missing/failed qualification."""
from pathlib import Path
import hashlib,json,subprocess,zipfile,xml.etree.ElementTree as ET,re
P=Path(__file__).resolve().parents[1];E=P/'evidence';R=P.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,indent=2)+'\n',encoding='utf-8')
old=read(R/'docs/foundation-provider-operation-lifetime/evidence/SOURCE-AND-BINARY-PINS.json')
sources={p.relative_to(R).as_posix():sha(p) for root in ['src','tests'] for p in sorted((R/root).rglob('*')) if p.is_file() and p.suffix in ['.cs','.csproj','.props'] and not any(t in ['bin','obj','.git','qt3tests','qt4tests','xslt30-test','xslt40-test'] for t in p.parts)}
engines={n:dict(r,sha256=sha(Path(r['path']))) for n,r in old['engines'].items()}
corpora={n:{'revision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/n,text=True).strip(),'status':subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/n,text=True).strip()} for n in old['corpora']}
configs={p.relative_to(R).as_posix():sha(p) for p in (R/'tests').glob('*/bin/Release/net10.0/*') if p.is_file() and (p.name.endswith(('.deps.json','.runtimeconfig.json')) or p.name=='xunit.runner.json')}
write(E/'SOURCE-AND-BINARY-PINS.json',dict(baseCommit=old['baseCommit'],sourceVersion='1.66.0',sources=sources,engines=engines,corpora=corpora,configuration=configs,sdkVersion=subprocess.check_output(['dotnet','--version'],text=True).strip(),configurationName='Release',targetFramework='net10.0',observerPath='D:/Development/Bosak/.guard-tmp/bcap-profile/Observer/bin/Release/net10.0/Observer.dll',observerSha256=sha(R/'.guard-tmp/bcap-profile/Observer/bin/Release/net10.0/Observer.dll')))
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip','w',zipfile.ZIP_DEFLATED) as z:
 for n,r in engines.items():z.write(r['path'],n+'.dll')
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip','w',zipfile.ZIP_DEFLATED) as z:
 for n,h in sources.items():
  if old['sources'].get(n)!=h:z.write(R/n,n)
with zipfile.ZipFile(E/'TEST-CONFIGURATION.zip','w',zipfile.ZIP_DEFLATED) as z:
 for n in configs:z.write(R/n,n)
cohorts={};rows=[];allocation={}
for label,folder,group,total in [('initial','unit-trx','units',25608),('second','final-unit-trx','final-units',25608),('qualified','qualified-unit-trx','qualified-units',25610)]:
 observed={}
 for f in (E/'bindings'/group).glob('*.json'):
  for a in read(f)['assemblies']:
   if a['name'].endswith('.Tests'):observed[a['name'].lower()]=a
 projects=[];actual=[]
 for p in sorted((E/folder).glob('*.trx')):
  tree=ET.parse(p);c=tree.find('.//{*}Counters').attrib;assert c['failed']==c['notExecuted']=='0',p
  rs=[r.attrib for r in tree.findall('.//{*}UnitTestResult')];actual+=rs
  storage=Path(tree.find('.//{*}UnitTest').attrib['storage']);name=storage.stem.lower();a=observed[name]
  projects.append(dict(trx=p.relative_to(E).as_posix(),sha256=sha(p),passed=int(c['passed']),testBinaryPath=a['path'],testBinarySha256=a['sha256'],testAssembly=a['name']))
  if label=='qualified':
   assert sha(Path(a['path']))==a['sha256']
   for n in tree.iter():
    if n.tag.endswith('StdOut') and n.text:
     m=re.search(r'Constructed warm identity: (\d+) bytes/query; constructions: (\d+); admissions: (\d+)',n.text)
     if m:allocation=dict(zip(['warmBytesPerQuery','constructions','admissions'],map(int,m.groups())))
 assert len(projects)==10 and len(actual)==sum(p['passed'] for p in projects)==total
 cohorts[label]=dict(passed=total,projects=projects)
 if label=='qualified':rows=actual
write(E/'UNIT-RESULTS.json',cohorts)
with zipfile.ZipFile(E/'TEST-ASSEMBLIES.zip','w',zipfile.ZIP_DEFLATED) as z:
 for r in cohorts['qualified']['projects']:z.write(r['testBinaryPath'],r['testAssembly']+'.dll')
new=[r for r in rows if '.PublicConstructionTests.' in r['testName']];life=[r for r in rows if '.QueryProviderOperationScopeTests.' in r['testName']]
assert len(new)==139 and len(life)==18 and allocation['constructions']==allocation['admissions']==1
write(E/'ACCEPTANCE.json',dict(newTestsPassed=139,results=new,lifetimeResults=life,allocation=allocation,scope='Bounded public operation-local profile; not native/all-host/persistent acceptance.'))
for source,target,prefix in [('ACCEPTANCE-CASES.json','CASE-MAPPING.json','SC'),('LIFETIME-CASES.json','LIFETIME-MAPPING.json','LC')]:
 cases=read(R/'docs/foundation-sourcefree-construction-policy'/source)['cases'];mapped=[]
 special={'SC06':'SC06_SC07_SC11_ActualProviderTail','SC07':'SC06_SC07_SC11_ActualProviderTail','SC11':'SC06_SC07_SC11_ActualProviderTail'}
 lc={'LC01':['SC05_SC29_RepeatedNoCache'],'LC02':['SC03_UnsupportedBinding'],'LC03':['SC08_AcquisitionFailure'],'LC04':['SC06_SC07_SC11_ActualProviderTail'],'LC05':['SC06_SC07_SC11_ActualProviderTail'],'LC06':['SC06_SC07_SC11_ActualProviderTail'],'LC07':['SC06_SC07_SC11_ActualProviderTail'],'LC08':['SC06_SC07_SC11_ActualProviderTail'],'LC09':['SC06_SC07_SC11_ActualProviderTail'],'LC10':['SC09_CallbackRevocation'],'LC11':['SC10_SerializedCallsAndIndependentOperations'],'LC12':['SC10_SerializedCallsAndIndependentOperations'],'LC13':['SC12_ConstructorAndCleanupFailures','LC13_OriginalAggregateBoundaries'],'LC14':['LC14_RepeatedCompletionAndForeignAdmission','LC13_LC14_DrainIncludesBlockedCleanupFailure','SC08_AcquisitionFailure'],'LC15':['SC34_SC35_NoImport'],'LC16':['SC06_SC07_SC11_ActualProviderTail']}
 for c in cases:
  patterns=lc[c['id']] if prefix=='LC' else [special[c['id']]] if c['id'] in special else sorted({r['testName'].split('(')[0].rsplit('.',1)[-1] for r in new if c['id'] in r['testName']})
  assert patterns,c['id']
  for pat in patterns:assert any(pat in r['testName'] and r['outcome']=='Passed' for r in rows),pat
  mapped.append(dict(c,status='QualifiedBoundedPublicScope',tests=patterns))
 write(P/target,dict(implementationStatus='BoundedPublicQualified',nativeProviderAccepted=False,cases=mapped))
api=read(E/'public-api.json');prev=read(R/'docs/foundation-provider-operation-lifetime/evidence/public-api.json');by={a['assembly']:a for a in prev['assemblies']}
for a in api['assemblies']:
 for k in ['declarations','friendAssemblies','references']:assert a[k]==by[a['assembly']][k],(a['assembly'],k)
write(E/'API-COMPARISON.json',dict(assemblies=len(api['assemblies']),exportedTypes=sum(a['publicTypes'] for a in api['assemblies']),declarationRows=sum(a['declarationCount'] for a in api['assemblies']),declarationsIdentical=True,friendGrantsIdentical=True,referencesIdentical=True,previousInventorySha256=sha(R/'docs/foundation-provider-operation-lifetime/evidence/public-api.json')))
for name in ['qt3','qt4']:
 for f in [name+'-skips.txt','final-'+name+'-skips.txt','qualified-'+name+'-skips.txt']:assert (E/f).read_bytes()==(R/'docs/foundation-provider-operation-lifetime/evidence'/(name+'-skips.txt')).read_bytes()
observed={}
for group in ['units','qt3','qt4','final-units','final-qt3','final-qt4','qualified-units','qualified-qt3','qualified-qt4']:
 cohort=engines if group.startswith('qualified-') else read(E/('SECOND-SOURCE-AND-BINARY-PINS.json' if group.startswith('final-') else 'INITIAL-PINS.json'))['engines'];seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors'],f
  for a in b['assemblies']:
   if a['name'] in cohort:
    assert a['sha256']==cohort[a['name']]['sha256'] and a['context']=='Default' and not a['collectible'],(f,a)
    seen.add(a['name'])
 assert {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen,group
 observed[group]=sorted(seen)
write(E/'LOADED-BINDINGS.json',dict(allMatchExactCohorts=True,groups=observed))
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','w',zipfile.ZIP_DEFLATED) as z:
 for f in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('public-construction-*.log*')):z.write(f,f.name)
 for folder in ['initial-focused-trx','expanded-focused-trx','final-focused-trx','reviewed-focused-trx','null-focused-trx','unit-trx','final-unit-trx','qualified-unit-trx']:
  for f in sorted((E/folder).glob('*.trx')):z.write(f,f.relative_to(E).as_posix())
write(P/'STATUS.json',dict(date='2026-11-05',baseCommit=old['baseCommit'],sourceVersion='1.66.0',unitTestsPassed=25610,newPublicTestsPassed=139,constructionProfileActivated=True,selectedAcceptanceCasesQualified=40,lifetimeCasesQualifiedBoundedPublic=16,qt3=[31397,0,424],qt4Gated=[8941,0,1918],skipMapsUnchanged=True,publicMetadataUnchanged=True,newPublicSignatures=False,newRequiredProviderMembers=False,allocation=allocation,sourceSnapshotsIntroduced=False,sourceCacheSlotsConsumed=0,persistentReceiptRegistry=False,broaderPersistentSignatureNeedsClosed=False,nativeProviderQualified=False,ownerAccepted=False,budgetsAccepted=False,apiFrozen=False,publicationAuthorized=False,pushPerformed=False,nextStep='Select finite compatible-view scope and prepare preview-package recipe/migration inventory; wider constructor/parse-budget/all-host and actual consumer/R4-R7 qualification remain pending.'))
print('Captured TA-20: 25610 final unit passes / 139 public controls; SC40/LC16, exact three cohorts and original receipts.')
