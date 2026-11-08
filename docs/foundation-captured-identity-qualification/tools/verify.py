"""Verify exact candidate solution/gate/metadata/binary qualification without promoting production."""
from pathlib import Path
import hashlib,json,re,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
a=read(P/'ADOPTION.json');s=read(P/'STATUS.json');i=read(E/'INPUTS.json');q=read(P/'QUALIFICATION.json');n=Path(a['nativeRoot']);W=Path(i['workspace'])
assert a['recordedBeforeImplementation'] and s['sourceVersion']=='1.81.0' and s['qualificationCompleteWithinScope']
for name,h in a['beforeFiles'].items():assert sha(R/name)==h,name
for name,h in a['nativeFiles'].items():assert sha(n/name)==h,name
assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=n,text=True).strip()==a['nativeHead']
assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=n,text=True).strip()==''
old=R/'docs/foundation-captured-identity-candidate'
for name,h in read(old/'MANIFEST.json').items():assert sha(old/name)==h,name
subprocess.run(['python',str(P/'tools/verify_compatibility.py')],check=True)
assert sha(Path(i['metadataAudit']['path']))==i['metadataAudit']['sha256']
for name,h in read(E/'SOURCE-PINS.json').items():assert sha(P/name)==h,name
expected=i['candidateCohort']['engines'];assert len(expected)==9 and q['engineHashes']==expected
assert sha(Path(i['candidateCohort']['candidateRuntimePath']))==q['exactCandidateRuntimeSha256']==expected['Bosak.XPath.Runtime']
units=read(E/'UNIT-RESULTS.json');assert units['passed']==s['unitTestsPassed']==q['unitTestsPassed']==26342 and len(units['projects'])==10
for row in units['projects']:
 assert row['exitCode']==0 and sha(Path(row['launchedBinary']))==row['launchedTestSha256']==row['testSha256']
 trx=P/row['trx'];assert sha(trx)==row['trxSha256'];root=ET.parse(trx).getroot();c=root.find('.//{*}Counters').attrib
 assert int(c['passed'])==row['expected']==row['passed'] and c['failed']==c['notExecuted']=='0'
 assert sorted(x.attrib['testName'] for x in root.findall('.//{*}UnitTestResult'))==row['caseNames']
 if row['project']!='bosak.xpath.runtime.tests':assert row['testSha256']==row['baselineTestSha256']
assert sum(r['passed'] for r in units['projects'])==26342
assert len(read(E/'UNIT-EXECUTIONS.initial.json'))==8 and read(E/'UNIT-EXECUTIONS.initial.json')[-1]['exitCode']==1
assert next(r for r in units['projects'] if 'database.tests' in r['project'])['localHttpAuthorization']
api=read(E/'public-api.json');base=read(R/i['baselineApi']);comparison=read(E/'API-COMPARISON.json')
assert sha(R/i['baselineApi'])==i['baselineApiSha256']==comparison['baselineSha256'] and sha(E/'public-api.json')==comparison['candidateSha256']
assert len(api['assemblies'])==11 and sum(r['publicTypes'] for r in api['assemblies'])==318 and sum(r['declarationCount'] for r in api['assemblies'])==6465
for x,y in zip(api['assemblies'],base['assemblies'],strict=True):
 for key in ['project','assembly','identity','references','friendAssemblies','publicTypes','declarationCount','declarations']:assert x[key]==y[key],(x['assembly'],key)
 path=W/x['project'];binary=path.parent/'bin/Release/net10.0'/(x['assembly']+'.dll');assert sha(binary)==x['sha256'].lower()
 if x['assembly']=='Bosak.XPath.Runtime':assert sha(binary)==expected[x['assembly']]
assert comparison['changedAssemblyBytes']==['Bosak.XPath.Runtime'] and comparison['valueFieldAndLayoutRowsEqual']
for name,counts in [('qt3',[31397,0,424]),('qt4',[8941,0,1918])]:
 gate=read(E/'GATE-RESULTS.json')['gates'][name];log=R/f'docs/foundation-r2-closure/evidence/execution/captured-qualification-{name}.log';text=log.read_text(encoding='utf-8-sig')
 assert [int(re.search(r'^'+label+r':\s+(\d+)',text,re.M).group(1)) for label in ['Passed','Failed','Skipped']]==counts
 assert sha(log)==gate['logSha256'] and sha(E/(name+'-skips.txt'))==gate['skipSha256']==gate['baselineSkipSha256']
 assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-public-primitives/evidence'/(name+'-skips.txt')).read_bytes()
assert read(E/'GATE-RESULTS.json')['qt4ExperimentalPassed']==11
review=read(E/'SOURCE-REVIEW.json');assert review['candidateSourceSha256']==sha(old/'candidate/QueryNodeFacts.cs') and review['productionSourceSha256']==sha(R/'src/Bosak.XPath.Runtime/Vm/QueryNodeFacts.cs')
for name,h in review['reviewedGuards'].items():assert sha(R/name)==h,name
retained=read(E/'RETAINED-FOUNDATION.json');manifest=read(E/'FOUNDATION-CANDIDATE.json');assert retained['passed']==20 and retained['failed']==0 and len(retained['cases'])==20 and len(manifest['assemblies'])==7
for row in retained['loadedAssemblies']:assert row['sha256']==expected[row['name']]
assert {r['name']:r['sha256'] for r in manifest['assemblies']}=={r['name']:r['sha256'] for r in retained['loadedAssemblies']}
layouts=read(E/'EXECUTION-LAYOUTS.json')
with zipfile.ZipFile(E/'EXECUTION-LAYOUTS.zip') as z:
 for group,layout in layouts.items():
  path=Path(layout['path']);assert {p.relative_to(path).as_posix() for p in path.rglob('*') if p.is_file()}==set(layout['files'])
  for name,h in layout['files'].items():assert sha(path/name)==hashlib.sha256(z.read(group+'/'+name)).hexdigest()==h,(group,name)
  for name,h in expected.items():
   if name+'.dll' in layout['files']:assert layout['files'][name+'.dll']==h,(group,name)
  assert layout['files']['Bosak.XPath.Runtime.dll']==expected['Bosak.XPath.Runtime']
assert layouts['retained']['files']['LegacyCaller.dll']==retained['callerSha256']
seen=set()
for group in ['unit-bindings','database-bindings','qt3-bindings','qt4-bindings']:
 current={}
 for path in (E/group).glob('*.json'):
  b=read(path);assert not b['errors']
  for row in b['assemblies']:
   name=row['name']
   if name in expected:assert row['sha256']==expected[name] and row['context']=='Default' and not row['collectible'];current[name]=row['sha256'];seen.add(name)
 assert current==read(E/'BINDING-SUMMARY.json')['groups'][group] and 'Bosak.XPath.Runtime' in current
assert seen==set(expected)
reuse=read(E/'QUALIFICATION-REUSE.json');assert reuse['previousManifestSha256']==sha(old/'MANIFEST.json') and reuse['sameCandidateRuntimeSha256']==expected['Bosak.XPath.Runtime'] and not reuse['reexecutedThisPacket']
assert not any(s[k] for k in ['productionChanged','candidatePromoted','ownerAccepted','budgetsAccepted','apiFrozen','packagesBuilt','pushPerformed']) and s['requirementsWaived']==0
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for name in z.namelist():
  if name.endswith('.log.json'):assert json.loads(z.read(name))['exitCode']==(1 if name in ['captured-qualification-unit-bosak.xpath.providers.database.tests.log.json','captured-qualification-retained-foundation.log.json'] else 0),name
subprocess.run(['python',str(P/'tools/check_docs.py')],check=True)
if (P/'MANIFEST.json').exists():
 manifest=read(P/'MANIFEST.json');assert set(manifest)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json' and not {'bin','obj','__pycache__'}.intersection(p.relative_to(P).parts)}
 for name,h in manifest.items():assert sha(P/name)==h,name
print('Verified TA-54:26342 candidate tests;exact31397/0/424 and8941/0/1918+11 gates/skip maps;318types/6465rows unchanged;20 retained cases. Production unchanged;owner budgets/freeze/promotion pending;next PV-Q04f.')
