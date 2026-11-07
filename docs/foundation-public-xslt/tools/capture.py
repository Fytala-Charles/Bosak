"""Seal actual TA-32 public XSLT qualification; refuses missing/failed cases."""
from pathlib import Path
import json,hashlib,subprocess,xml.etree.ElementTree as ET,zipfile,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,d):p.write_text(json.dumps(d,indent=2)+'\n',encoding='utf-8')
old=read(R/'docs/foundation-host-outcome/evidence/SOURCE-AND-BINARY-PINS.json')
sources={p.relative_to(R).as_posix():sha(p) for root in ['src','tests'] for p in sorted((R/root).rglob('*')) if p.is_file() and p.suffix in ['.cs','.csproj','.props'] and not any(t in ['bin','obj','.git','qt3tests','qt4tests','xslt30-test','xslt40-test'] for t in p.parts)}
engines={n:dict(r,sha256=sha(Path(r['path']))) for n,r in old['engines'].items()}
corpora={n:{'revision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/n,text=True).strip(),'status':subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/n,text=True).strip()} for n in old['corpora']}
configs={p.relative_to(R).as_posix():sha(p) for p in (R/'tests').glob('*/bin/Release/net10.0/*') if p.is_file() and (p.name.endswith(('.deps.json','.runtimeconfig.json')) or p.name=='xunit.runner.json')}
write(E/'SOURCE-AND-BINARY-PINS.json',dict(baseCommit=old['baseCommit'],sourceVersion='1.76.0',sources=sources,engines=engines,corpora=corpora,configuration=configs,sdkVersion=subprocess.check_output(['dotnet','--version'],text=True).strip(),configurationName='Release',targetFramework='net10.0',observerPath=old['observerPath'],observerSha256=sha(Path(old['observerPath']))))
with zipfile.ZipFile(E/'ENGINE-BINARIES.zip','w',zipfile.ZIP_DEFLATED) as z:
 for n,r in engines.items():z.write(r['path'],n+'.dll')
with zipfile.ZipFile(E/'CHANGED-SOURCE.zip','w',zipfile.ZIP_DEFLATED) as z:
 for n,h in sources.items():
  if old['sources'].get(n)!=h:z.write(R/n,n)
with zipfile.ZipFile(E/'TEST-CONFIGURATION.zip','w',zipfile.ZIP_DEFLATED) as z:
 for n in configs:z.write(R/n,n)
projects=[];rows=[];allocation={}
for p in sorted((E/'unit-release-trx').glob('*.trx')):
 tree=ET.parse(p);c=tree.find('.//{*}Counters').attrib;assert c['failed']==c['notExecuted']=='0',p
 rs=[r.attrib for r in tree.findall('.//{*}UnitTestResult')];rows+=rs;binary=Path(tree.find('.//{*}UnitTest').attrib['storage'])
 projects.append(dict(trx=p.relative_to(E).as_posix(),sha256=sha(p),passed=int(c['passed']),testBinaryPath=str(binary),testBinarySha256=sha(binary),testAssembly=binary.stem))
 for n in tree.iter():
  if n.tag.endswith('StdOut') and n.text:
   m=re.search(r'Constructed warm identity: (\d+) bytes/query; constructions: (\d+); admissions: (\d+)',n.text)
   if m:allocation=dict(zip(['warmBytesPerQuery','constructions','admissions'],map(int,m.groups())))
assert len(projects)==10 and len(rows)==sum(r['passed'] for r in projects)==26054
write(E/'UNIT-RESULTS.json',dict(passed=26054,projects=projects))
with zipfile.ZipFile(E/'TEST-ASSEMBLIES.zip','w',zipfile.ZIP_DEFLATED) as z:
 for row in projects:z.write(row['testBinaryPath'],row['testAssembly']+'.dll')
new=[row for row in rows if '.PublicXsltSessionTests.' in row['testName']]
assert len(new)==67
write(E/'ACCEPTANCE.json',dict(newTestsPassed=67,results=new,scope='Bounded public ordinary XSLT invocation and capture; native/owner/schema-aware/budget qualification remains pending.'))
api=read(E/'public-api.json');previous=read(R/'docs/foundation-host-outcome/evidence/public-api.json');delta=read(P/'API-DELTA.json')['assemblies']
for x,y in zip(api['assemblies'],previous['assemblies'],strict=True):
 for key in ['project','assembly','references','friendAssemblies']:assert x[key]==y[key],(x['assembly'],key)
 actual=dict(added=sorted(set(x['declarations'])-set(y['declarations'])),removed=sorted(set(y['declarations'])-set(x['declarations'])))
 assert actual==delta.get(x['assembly'],dict(added=[],removed=[])),x['assembly']
assert len(api['assemblies'])==11 and sum(x['publicTypes'] for x in api['assemblies'])==317 and sum(x['declarationCount'] for x in api['assemblies'])==6457
write(E/'API-COMPARISON.json',dict(assemblies=11,exportedTypes=317,declarationRows=6457,newPublicTypes=5,newDeclarationRows=71,selectedDeltaOnly=True,existingValueLayoutUnchanged=True,friendsUnchanged=True,referencesUnchanged=True))
for name in ['qt3','qt4']:assert (E/(name+'-release-skips.txt')).read_bytes()==(R/'docs/foundation-host-outcome/evidence'/(name+'-skips.txt')).read_bytes()
observed={}
for group in ['units-release','qt3-release','qt4-release']:
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
caseMapping=[]
for n in range(1,32):
 identifier='PX'+str(n).zfill(2);cases=[row['testName'] for row in new if identifier+'_' in row['testName']];assert cases,identifier
 caseMapping.append(dict(id=identifier,status='QualifiedBoundedPublicProfile',tests=cases))
write(P/'CASE-MAPPING.json',dict(cases=caseMapping,additionalPassedTestCount=False))
costs=[]
for f in (E/'unit-release-trx').glob('*.trx'):
 for n in ET.parse(f).iter():
  if n.tag.endswith('StdOut') and n.text:
   m=re.search(r'Public XSLT cost: cold caller bytes (\d+); cold ticks (\d+); warm runs (\d+); warm caller bytes (\d+); warm ticks (\d+); frequency (\d+)',n.text)
   if m:costs.append(dict(zip(['coldCallerBytes','coldTicks','warmRuns','warmCallerBytes','warmTicks','stopwatchFrequency'],map(int,m.groups()))))
assert len(costs)==1 and costs[0]['warmRuns']==8
write(E/'COST-OBSERVATION.json',dict(measurement=costs[0],allocationScope='Calling thread only; excludes transform/serializer worker allocations.',latencyScope='Cold compile plus first capture; eight warm captures including joined worker time.',fixture='PX31 source-free Hello text, mandatory caps under Options(), no source provider.',ownerBudgetAccepted=False,representativeNativeWorkload=False))
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','w',zipfile.ZIP_DEFLATED) as z:
 for receipt in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('public-xslt-*.log.json')):
  log=receipt.with_suffix('');z.write(log,log.name);z.write(receipt,receipt.name)
 for directory in ['focused-trx','focused-final-trx','focused-qualified-trx','focused-acceptance-trx','focused-release-trx','unit-trx','unit-final-trx','unit-release-trx']:
  for f in sorted((E/directory).glob('*.trx')):z.write(f,f.relative_to(E).as_posix())
write(P/'STATUS.json',dict(date='2026-11-06',baseCommit=old['baseCommit'],sourceVersion='1.76.0',unitTestsPassed=26054,newPublicXsltControlsPassed=67,publicApiChanged=True,newPublicTypes=5,newDeclarationRows=71,exportedTypes=317,declarationRows=6457,qt3=[31397,0,424],qt4Gated=[8941,0,1918],skipMapsUnchanged=True,ah03Implemented=True,publicXsltBcapActivated=True,schemaAwareXsltActivated=False,proposedPublicTypesPresent=46,proposedPublicTypesTotal=47,remainingPublicType='Bosak.XPath.Core.Schema.SchemaAtomics',previewSelectedPackages=9,packagesBuilt=False,ownerAccepted=False,budgetsAccepted=False,apiFrozen=False,publicationAuthorized=False,pushPerformed=False,nextStep='PV-01 immutable policy for public atomic provenance observation and remaining typed-container coverage; NU-01/SC-01 and actual R4-R7 remain unwaived.'))
print('Captured TA-32: 26054 tests / 67 public XSLT controls, five selected types / 71 rows, exact gates/skips and actual loaded identities. No native/freeze/publication acceptance.')
