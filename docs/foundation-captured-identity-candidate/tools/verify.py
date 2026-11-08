"""Verify isolated captured-identity candidate evidence without promoting or substituting production qualification."""
from pathlib import Path
import hashlib,json,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
a=read(P/'ADOPTION.json');s=read(P/'STATUS.json');n=Path(a['nativeRoot']);i=read(E/'ISOLATION.json');W=Path(i['workspace'])
assert a['recordedBeforeImplementation'] and s['sourceVersion']=='1.81.0' and s['isolatedCandidatePrepared']
for name,h in a['beforeFiles'].items():assert sha(R/name)==h,name
for name,h in a['nativeFiles'].items():assert sha(n/name)==h,name
assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=n,text=True).strip()==a['nativeHead']
assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=n,text=True).strip()==''
old=R/'docs/foundation-public-data-callback-attribution'
for name,h in read(old/'MANIFEST.json').items():assert sha(old/name)==h,name
subprocess.run(['python',str(P/'tools/verify_compatibility.py')],check=True)
for name,h in i['runtimeSources'].items():assert sha(R/name)==h,name
for name,h in i['wrapperFiles'].items():assert sha(W/name)==h,name
for name,h in read(E/'SOURCE-PINS.json').items():assert sha(P/name)==h,name
t=read(E/'TEST-SOURCE-PINS.json')
for name,h in t['originalRuntimeTests'].items():assert sha(R/name)==h,name
assert sha(Path(t['testWrapper']['path']))==t['testWrapper']['sha256']
assert sha(P/'candidate/QueryNodeFacts.cs')==i['candidateSourceSha256']==t['candidateSourceSha256']
assert sha(P/'candidate/CapturedIdentityTests.cs')==t['newTestSourceSha256']
assert sha(P/'candidate/QueryNodeFactsTests.cs')==t['testHostCopySha256']
source=(P/'candidate/QueryNodeFacts.cs').read_text(encoding='utf-8');production=(R/'src/Bosak.XPath.Runtime/Vm/QueryNodeFacts.cs').read_text(encoding='utf-8')
for identity in ['_environment','_authority']:assert f'owner.CheckCapturedIdentity(); return owner.{identity};' in source
assert '_operation.CheckObservationActive();' in source and '_operation.CheckRegistrationActive(_snapshot);' in source
assert 'owner.CheckStored();' in source and 'token.ThrowIfCancellationRequested();owner.CheckStored();' in source
assert source[source.index('        bool IQueryTypedContainerIssuer.OwnsInput'):]==production[production.index('        bool IQueryTypedContainerIssuer.OwnsInput'):]
fixtures=read(E/'TEST-FIXTURES.json')
for name,h in fixtures['files'].items():assert sha(Path(fixtures['origin'])/name)==sha(Path(fixtures['copy'])/name)==h,name
cohort=read(E/'CANDIDATE-COHORT.json');engines=cohort['engines'];assert len(engines)==9 and cohort['changedEngineAssemblies']==['Bosak.XPath.Runtime']
assert not cohort['fullQualificationComplete'] and not cohort['promotionSelected'] and not cohort['foundation20CaseCallerExecuted']
assert sha(Path(cohort['candidateRuntimePath']))==cohort['candidateRuntimeSha256']==engines['Bosak.XPath.Runtime']
for name,h in engines.items():
 assert sha(W/'engine'/f'{name}.dll')==h
 if name!='Bosak.XPath.Runtime':assert h==i['qualifiedEngines'][name]
layouts=read(E/'EXECUTION-LAYOUTS.json')
with zipfile.ZipFile(E/'EXECUTION-LAYOUTS.zip') as z:
 for group,layout in layouts.items():
  path=Path(layout['path']);assert {p.relative_to(path).as_posix() for p in path.rglob('*') if p.is_file()}==set(layout['files'])
  for name,h in layout['files'].items():assert sha(path/name)==hashlib.sha256(z.read(group+'/'+name)).hexdigest()==h,(group,name)
  assert layout['files']['Bosak.XPath.Runtime.dll']==engines['Bosak.XPath.Runtime']
  for name,h in engines.items():
   if name+'.dll' in layout['files']:assert layout['files'][name+'.dll']==h,(group,name)
results=read(E/'TEST-RESULTS.json')
for name,count in [('runtime',1899),('nativeAdapter',59),('native451',451),('consumer',10)]:
 row=results['lanes'][name];trx=P/row['trx'];assert sha(trx)==row['sha256'];root=ET.parse(trx).getroot();c=root.find('.//{*}Counters').attrib
 assert int(c['passed'])==count and c['failed']==c['notExecuted']=='0'
 assert sorted(x.attrib['testName'] for x in root.findall('.//{*}UnitTestResult'))==row['caseNames']
assert sum('CapturedIdentity' in name for name in results['lanes']['runtime']['caseNames'])==4
seen=set()
for p in (E/'bindings').glob('*.json'):
 b=read(p);assert not b['errors']
 for row in b['assemblies']:
  name=row['name']
  if name in engines:assert row['sha256']==engines[name] and row['context']=='Default' and not row['collectible'];seen.add(name)
assert {'Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime'}<=seen
counts=read(E/'CALLBACK-COUNTS.json');raw=E/'CALLBACK-COUNTS.raw.json';assert sha(raw)==counts['rawReceiptSha256']
assert counts['coreChanged'] and counts['experimentalRuntimeOverlay'] and counts['capturedIdentityProofChanged'] and not counts['providerFacingFullProofsChanged']
assert counts['rows']==read(raw)['rows'] and len(counts['rows'])==4
delta=read(E/'COUNTER-DELTA.json');assert delta['beforeSha256']==sha(old/'evidence/CALLBACK-COUNTS.json') and delta['candidateSha256']==sha(E/'CALLBACK-COUNTS.json')
for row in delta['rows']:assert 0<row['candidateOutputChecks']<row['beforeOutputChecks'] and 15<row['reductionPercent']<25
assert not any(s[k] for k in ['productionChanged','publicApiChangeSelected','fullSolutionQualified','qt3CandidateRun','qt4CandidateRun','apiAbiCandidateQualified','candidatePromoted','ownerAccepted','budgetsAccepted','apiFrozen','packagesBuilt','pushPerformed'])
assert s['runtimeBuildWarnings']==s['runtimeBuildErrors']==s['requirementsWaived']==0 and s['testWrapperExistingWarnings']==2 and s['testWrapperErrors']==0
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for name in z.namelist():
  if name.endswith('.log.json'):assert json.loads(z.read(name))['exitCode']==(1 if name in ['captured-identity-tests-build.log.json','captured-identity-runtime-tests.log.json'] else 0),name
 log=z.read('captured-identity-runtime-build.log').decode('utf-8-sig');assert '0 Warning(s)' in log and '0 Error(s)' in log
subprocess.run(['python',str(P/'tools/check_docs.py')],check=True)
if (P/'MANIFEST.json').exists():
 manifest=read(P/'MANIFEST.json');assert set(manifest)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json' and not {'bin','obj','__pycache__'}.intersection(p.relative_to(P).parts)}
 for name,h in manifest.items():assert sha(P/name)==h,name
print('Verified TA-53:isolated Runtime candidate passes1899 Runtime/59 adapter/451 native/10 consumer cases;check counts reduce~18%;production unchanged. Full solution/gates/API-ABI and promotion pending;next PV-Q04e.')
