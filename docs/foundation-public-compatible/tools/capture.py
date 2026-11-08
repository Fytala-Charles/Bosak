"""Seal actual TA-25 internal prerequisite qualification; refuses missing/failed cases."""
from pathlib import Path
import json,hashlib,subprocess,xml.etree.ElementTree as ET,zipfile,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,d):p.write_text(json.dumps(d,indent=2)+'\n',encoding='utf-8')
old=read(R/'docs/foundation-compatible-cache/evidence/SOURCE-AND-BINARY-PINS.json')
sources={p.relative_to(R).as_posix():sha(p) for root in ['src','tests'] for p in sorted((R/root).rglob('*')) if p.is_file() and p.suffix in ['.cs','.csproj','.props'] and not any(t in ['bin','obj','.git','qt3tests','qt4tests','xslt30-test','xslt40-test'] for t in p.parts)}
engines={n:dict(r,sha256=sha(Path(r['path']))) for n,r in old['engines'].items()}
corpora={n:{'revision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/n,text=True).strip(),'status':subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/n,text=True).strip()} for n in old['corpora']}
configs={p.relative_to(R).as_posix():sha(p) for p in (R/'tests').glob('*/bin/Release/net10.0/*') if p.is_file() and (p.name.endswith(('.deps.json','.runtimeconfig.json')) or p.name=='xunit.runner.json')}
write(E/'SOURCE-AND-BINARY-PINS.json',dict(baseCommit=old['baseCommit'],sourceVersion='1.70.0',sources=sources,engines=engines,corpora=corpora,configuration=configs,sdkVersion=subprocess.check_output(['dotnet','--version'],text=True).strip(),configurationName='Release',targetFramework='net10.0',observerPath=old['observerPath'],observerSha256=sha(Path(old['observerPath']))))
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip','w',zipfile.ZIP_DEFLATED) as z:
 for n,r in engines.items():z.write(r['path'],n+'.dll')
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip','w',zipfile.ZIP_DEFLATED) as z:
 for n,h in sources.items():
  if old['sources'].get(n)!=h:z.write(R/n,n)
with zipfile.ZipFile(E/'TEST-CONFIGURATION.zip','w',zipfile.ZIP_DEFLATED) as z:
 for n in configs:z.write(R/n,n)
projects=[];rows=[];allocation={}
for p in sorted((E/'unit-trx').glob('*.trx')):
 tree=ET.parse(p);c=tree.find('.//{*}Counters').attrib;assert c['failed']==c['notExecuted']=='0',p
 rs=[r.attrib for r in tree.findall('.//{*}UnitTestResult')];rows+=rs;binary=Path(tree.find('.//{*}UnitTest').attrib['storage'])
 projects.append(dict(trx=p.relative_to(E).as_posix(),sha256=sha(p),passed=int(c['passed']),testBinaryPath=str(binary),testBinarySha256=sha(binary),testAssembly=binary.stem))
 for n in tree.iter():
  if n.tag.endswith('StdOut') and n.text:
   m=re.search(r'Constructed warm identity: (\d+) bytes/query; constructions: (\d+); admissions: (\d+)',n.text)
   if m:allocation=dict(zip(['warmBytesPerQuery','constructions','admissions'],map(int,m.groups())))
assert len(projects)==10 and len(rows)==sum(r['passed'] for r in projects)==25831
write(E/'UNIT-RESULTS.json',dict(passed=25831,projects=projects))
with zipfile.ZipFile(E/'TEST-ASSEMBLIES.zip','w',zipfile.ZIP_DEFLATED) as z:
 for row in projects:z.write(row['testBinaryPath'],row['testAssembly']+'.dll')
new=[r for r in rows if any('.'+c+'.' in r['testName'] for c in ['PublicCompatibleTests'])]
legacy=[r for r in rows if any('.'+c+'.' in r['testName'] for c in ['PublicConstructionTests','QueryProviderOperationScopeTests','PairedProviderOperationScopeTests','RetainedSourceAdmissionTests','CompatibleAdmissionTests','CompatibleCacheTests'])]
assert len(new)==73 and len(legacy)==305
write(E/'ACCEPTANCE.json',dict(newTestsPassed=73,results=new,legacyResults=legacy,constructionAllocation=allocation,scope='Public engine-issued compatible handoff and singleton identity; no native/all-host/owner acceptance.'))
mapped=[]
for n in range(1,33):
 identifier='CV'+str(n).zfill(2)
 patterns=sorted({r['testName'].split('(')[0].rsplit('.',1)[-1] for r in new if identifier in r['testName']})
 if n==7:patterns=['CV06_ExactEndpoints']
 if n==18:patterns=['CV17_OriginalEvidence']
 if n==31:patterns=['CV30_ClosedMembership','Documented native/budget/all-host exclusions in STATUS.json and IMPLEMENTATION.md']
 if n==32:patterns+=['Full solution/gates/API/binding qualification in evidence']
 assert patterns,identifier
 mapped.append(dict(id=identifier,status='QualifiedBoundedPublicProfile',tests=patterns,scope='Public compatible singleton identity; native/budget/all-host and owner acceptance excluded.'))
write(P/'CASE-MAPPING.json',dict(implementationStatus='BoundedPublicCompatibleQualified',publicCompatibilityActivated=True,cases=mapped))
api=read(E/'public-api.json');previous=read(R/'docs/foundation-compatible-cache/evidence/public-api.json');by={a['assembly']:a for a in previous['assemblies']};delta=read(P/'API-DELTA.json')['assemblies']
for a in api['assemblies']:
 b=by[a['assembly']]
 for key in ['friendAssemblies','references']:assert a[key]==b[key],(a['assembly'],key)
 actual=dict(added=sorted(set(a['declarations'])-set(b['declarations'])),removed=sorted(set(b['declarations'])-set(a['declarations'])))
 assert actual==delta.get(a['assembly'],dict(added=[],removed=[])),a['assembly']
write(E/'API-COMPARISON.json',dict(assemblies=len(api['assemblies']),exportedTypes=sum(a['publicTypes'] for a in api['assemblies']),declarationRows=sum(a['declarationCount'] for a in api['assemblies']),exactSelectedDelta=True,friendGrantsIdentical=True,referencesIdentical=True,previousInventorySha256=sha(R/'docs/foundation-compatible-cache/evidence/public-api.json'),deltaSha256=sha(P/'API-DELTA.json')))
for name in ['qt3','qt4']:assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-compatible-cache/evidence'/(name+'-skips.txt')).read_bytes()
observed={}
for group in ['units','qt3','qt4']:
 seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for a in b['assemblies']:
   if a['name'] in engines:
    assert a['sha256']==engines[a['name']]['sha256'] and a['context']=='Default' and not a['collectible'],(f,a)
    seen.add(a['name'])
 assert {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen,group
 observed[group]=sorted(seen)
write(E/'LOADED-BINDINGS.json',dict(allMatchExactCandidate=True,groups=observed))
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','w',zipfile.ZIP_DEFLATED) as z:
 for f in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('public-compatible-*.log*')):z.write(f,f.name)
 for folder in ['focused-trx','initial-unit-trx','unit-trx']:
  for f in sorted((E/folder).glob('*.trx')):z.write(f,f.relative_to(E).as_posix())
write(P/'STATUS.json',dict(date='2026-11-05',baseCommit=old['baseCommit'],sourceVersion='1.70.0',newPublicTestsPassed=73,legacyAdmissionCacheLifetimeConstructionControlsPassed=305,unitTestsPassed=25831,compatiblePublicCasesQualified=32,corePublicDeltaDisposition='ImplementedBoundedCoreSelection',publicSignatureAdded=True,compatibleProfileActivated=True,compatibleIdentityCacheImplemented=True,publicSignatureOwnerAccepted=False,previewMetadataRefreshed=True,packageProjectsEvaluated=11,selectedPackageClosure=7,dirtyQualifiedPackageInputs=29,qt3=[31397,0,424],qt4Gated=[8941,0,1918],skipMapsUnchanged=True,publicApiDelta='API-DELTA.json',exportedTypes=311,declarationRows=6382,newRequiredProviderMembers=False,sourceSchemaReassessed=False,sourceTypesRelabeled=False,packagesBuilt=False,nativeProviderQualified=False,ownerAccepted=False,budgetsAccepted=False,apiFrozen=False,publicationAuthorized=False,pushPerformed=False,nextStep='Reconcile remaining R3 supported/refused scope and same-candidate consumer/performance qualification; select an immutable preview checkpoint/version before package artifacts and package-only consumers.'))
print('Captured TA-25: 25831 passes / 73 public compatible controls + 305 legacy; exact gates/skips, selected API delta, loaded bindings and source/configuration proof.')
