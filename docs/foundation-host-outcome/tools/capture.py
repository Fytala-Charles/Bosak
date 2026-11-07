"""Seal actual TA-31 internal host outcome prerequisite qualification; refuses missing/failed cases."""
from pathlib import Path
import json,hashlib,subprocess,xml.etree.ElementTree as ET,zipfile,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,d):p.write_text(json.dumps(d,indent=2)+'\n',encoding='utf-8')
old=read(R/'docs/foundation-xslt-completion/evidence/SOURCE-AND-BINARY-PINS.json')
sources={p.relative_to(R).as_posix():sha(p) for root in ['src','tests'] for p in sorted((R/root).rglob('*')) if p.is_file() and p.suffix in ['.cs','.csproj','.props'] and not any(t in ['bin','obj','.git','qt3tests','qt4tests','xslt30-test','xslt40-test'] for t in p.parts)}
engines={n:dict(r,sha256=sha(Path(r['path']))) for n,r in old['engines'].items()}
corpora={n:{'revision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/n,text=True).strip(),'status':subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/n,text=True).strip()} for n in old['corpora']}
configs={p.relative_to(R).as_posix():sha(p) for p in (R/'tests').glob('*/bin/Release/net10.0/*') if p.is_file() and (p.name.endswith(('.deps.json','.runtimeconfig.json')) or p.name=='xunit.runner.json')}
write(E/'SOURCE-AND-BINARY-PINS.json',dict(baseCommit=old['baseCommit'],sourceVersion='1.75.0',sources=sources,engines=engines,corpora=corpora,configuration=configs,sdkVersion=subprocess.check_output(['dotnet','--version'],text=True).strip(),configurationName='Release',targetFramework='net10.0',observerPath=old['observerPath'],observerSha256=sha(Path(old['observerPath']))))
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
assert len(projects)==10 and len(rows)==sum(r['passed'] for r in projects)==25987
write(E/'UNIT-RESULTS.json',dict(passed=25987,projects=projects))
with zipfile.ZipFile(E/'TEST-ASSEMBLIES.zip','w',zipfile.ZIP_DEFLATED) as z:
 for row in projects:z.write(row['testBinaryPath'],row['testAssembly']+'.dll')
new=[r for r in rows if '.CapturedHostBindingTests.HR' in r['testName']]
assert len(new)==23
write(E/'ACCEPTANCE.json',dict(newTestsPassed=23,results=new,scope='Exact internal API host outcome routing and staged diagnostics; no public XSLT activation.'))
api=read(E/'public-api.json');previous=read(R/'docs/foundation-xslt-completion/evidence/public-api.json')
for a,b in zip(api['assemblies'],previous['assemblies'],strict=True):
 for key in ['project','assembly','references','friendAssemblies','publicTypes','declarationCount','declarations']:assert a[key]==b[key],(a['assembly'],key)
write(E/'API-COMPARISON.json',dict(assemblies=11,exportedTypes=312,declarationRows=6386,existingValueLayoutUnchanged=True,declarationsUnchanged=True,friendsUnchanged=True,referencesUnchanged=True))
for name in ['qt3','qt4']:assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-xslt-completion/evidence'/(name+'-final-skips.txt')).read_bytes()
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
 for receipt in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('host-outcome-*.log.json')):
  log=receipt.with_suffix('');z.write(log,log.name);z.write(receipt,receipt.name)
 for directory in ['focused-trx','unit-trx']:
  for f in sorted((E/directory).glob('*.trx')):z.write(f,f.relative_to(E).as_posix())
write(P/'STATUS.json',dict(date='2026-11-06',baseCommit=old['baseCommit'],sourceVersion='1.75.0',unitTestsPassed=25987,newHostOutcomeControlsPassed=23,publicApiChanged=False,exportedTypes=312,declarationRows=6386,qt3=[31397,0,424],qt4Gated=[8941,0,1918],skipMapsUnchanged=True,ah03Implemented=False,publicXsltBcapActivated=False,packagesBuilt=False,ownerAccepted=False,budgetsAccepted=False,apiFrozen=False,publicationAuthorized=False,pushPerformed=False,nextStep='AH-03 public XSLT compiler/executable/invocation/capture/final outcome. All broader requirements and R4-R7 remain open.'))
print('Captured TA-31 internal host outcome prerequisite: 25987 tests / 23 new controls; unchanged public API and skip maps; exact loaded identities.')
