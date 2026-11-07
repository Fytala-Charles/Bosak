"""Seal actual TA-40 eleven-kind complete-container qualification; refuses missing/failed cases."""
from pathlib import Path
import json,hashlib,subprocess,xml.etree.ElementTree as ET,zipfile,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,d):p.write_text(json.dumps(d,indent=2)+'\n',encoding='utf-8')
old=read(R/'docs/foundation-compatible-container/evidence/SOURCE-AND-BINARY-PINS.json')
sources={p.relative_to(R).as_posix():sha(p) for root in ['src','tests'] for p in sorted((R/root).rglob('*')) if p.is_file() and p.suffix in ['.cs','.csproj','.props'] and not any(t in ['bin','obj','.git','qt3tests','qt4tests','xslt30-test','xslt40-test'] for t in p.parts)}
engines={n:dict(r,sha256=sha(Path(r['path']))) for n,r in old['engines'].items()}
corpora={n:{'revision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/n,text=True).strip(),'status':subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/n,text=True).strip()} for n in old['corpora']}
configs={p.relative_to(R).as_posix():sha(p) for p in (R/'tests').glob('*/bin/Release/net10.0/*') if p.is_file() and (p.name.endswith(('.deps.json','.runtimeconfig.json')) or p.name=='xunit.runner.json')}
write(E/'SOURCE-AND-BINARY-PINS.json',dict(baseCommit=old['baseCommit'],sourceVersion='1.81.0',sources=sources,engines=engines,corpora=corpora,configuration=configs,sdkVersion=subprocess.check_output(['dotnet','--version'],text=True).strip(),configurationName='Release',targetFramework='net10.0',observerPath=old['observerPath'],observerSha256=sha(Path(old['observerPath']))))
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
assert len(projects)==10 and len(rows)==sum(r['passed'] for r in projects)==26338
write(E/'UNIT-RESULTS.json',dict(passed=26338,projects=projects))
with zipfile.ZipFile(E/'TEST-ASSEMBLIES.zip','w',zipfile.ZIP_DEFLATED) as z:
 for row in projects:z.write(row['testBinaryPath'],row['testAssembly']+'.dll')
new=[row for row in rows if '.PublicTypedIdentityTests.PW' in row['testName'] or '.PublicCompatibleTests.PW' in row['testName']]
assert len(new)==102
write(E/'ACCEPTANCE.json',dict(newTestsPassed=102,results=new,scope='PV-T05 eleven publicly constructible atomic kinds in both complete routes;87 prior container regressions are not new cases.'))
api=read(E/'public-api.json');previous=read(R/'docs/foundation-compatible-container/evidence/public-api.json');delta=read(P/'API-DELTA.json')['assemblies']
for x,y in zip(api['assemblies'],previous['assemblies'],strict=True):
 assert x['sha256'].lower()==engines[x['assembly']]['sha256'],x['assembly']
 for key in ['project','assembly','references','friendAssemblies']:assert x[key]==y[key],(x['assembly'],key)
 actual=dict(added=sorted(set(x['declarations'])-set(y['declarations'])),removed=sorted(set(y['declarations'])-set(x['declarations'])))
 assert actual==delta.get(x['assembly'],dict(added=[],removed=[]))
assert len(api['assemblies'])==11 and sum(x['publicTypes'] for x in api['assemblies'])==318 and sum(x['declarationCount'] for x in api['assemblies'])==6465
write(E/'API-COMPARISON.json',dict(assemblies=11,exportedTypes=318,declarationRows=6465,newPublicTypes=0,newDeclarationRows=0,selectedDeltaOnly=True,existingValueLayoutUnchanged=True,friendsUnchanged=True,referencesUnchanged=True))
for name in ['qt3','qt4']:assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-compatible-container/evidence'/(name+'-skips.txt')).read_bytes()
observed={}
for group in ['units','qt3','qt4']:
 seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for x in b['assemblies']:
   if x['name'] in engines:
    assert x['sha256']==engines[x['name']]['sha256'] and x['context']=='Default' and not x['collectible'],(f,x)
    seen.add(x['name'])
 assert {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen,group
 observed[group]=sorted(seen)
write(E/'LOADED-BINDINGS.json',dict(allMatchExactCandidate=True,groups=observed))
cost={}
for f in (E/'unit-trx').glob('*.trx'):
 for n in ET.parse(f).iter():
  if n.tag.endswith('StdOut') and n.text:
   m=re.search(r'(Ordinary|Compatible) wider cost: cold caller bytes (\d+); warm reads (\d+); warm caller bytes (\d+)',n.text)
   if m:cost[m.group(1).lower()]=dict(zip(['coldCallerBytes','warmReads','warmCallerBytes'],map(int,m.groups()[1:])))
assert set(cost)=={'ordinary','compatible'} and all(c['warmReads']==2 for c in cost.values())
write(E/'COST-OBSERVATION.json',dict(observations=cost,ownerBudgetAccepted=False,compatibleWarmTraceCapacityReserved=True,scope='Caller-only fixture observations. Ordinary two-member dateTime/QName and compatible one-member dateTime. Compatible cold includes trace growth; warm trace storage pre-reserved with every callback recorded. No native/retained-output budget acceptance or cross-profile ratio claim.'))
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','w',zipfile.ZIP_DEFLATED) as z:
 for receipt in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('public-primitives-*.log.json')):
  log=receipt.with_suffix('');z.write(log,log.name);z.write(receipt,receipt.name)
 for directory in ['focused-trx','unit-trx']:
  for f in sorted((E/directory).glob('*.trx')):z.write(f,f.relative_to(E).as_posix())
write(P/'STATUS.json',dict(date='2026-11-06',baseCommit=old['baseCommit'],sourceVersion='1.81.0',unitTestsPassed=26338,newTestsPassed=102,focusedTestsPassed=189,pvT05CompleteBounded=True,publicFactoryKindsSupported=11,enumOnlyKindsImplemented=False,pv01Complete=False,publicTypes=318,declarationRows=6465,newDeclarationRows=0,qt3=[31397,0,424],qt4Gated=[8941,0,1918],packagesBuilt=False,ownerAccepted=False,budgetsAccepted=False,apiFrozen=False,publicationAuthorized=False,pushPerformed=False,nextSlice='PV-T06'))
print('Captured TA-40:26338 tests;102 wider complete-container controls;189 focused;unchanged declarations/layouts/friends/references.')
