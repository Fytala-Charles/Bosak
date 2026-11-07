"""Verify native Schema source/binary compatibility evidence with exact scope and immutable originals."""
from pathlib import Path
import hashlib,json,re,subprocess,xml.etree.ElementTree as ET,zipfile
R=Path(__file__).resolve().parents[3];P=R/'docs/foundation-native-schema-lane';E=P/'evidence';Q=R/'docs/foundation-public-primitives'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
s=read(P/'STATUS.json');a=read(P/'ADOPTION.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json');d=read(E/'LANE-INPUTS.json');W=Path(d['workspace']);N=Path(d['nativeRoot'])
assert s['sourceVersion']==a['sourceVersion']==pins['sourceVersion']=='1.81.0'
assert s['preflightAndExecutionCompleteWithinScope'] and s['nativeBaselinePassed']==s['nativeSourcePassed']==s['nativeUnchangedBinaryPassed']==s['distinctExistingNativeCases']==451
assert s['countsOverlapNotAdditive'] and s['newEngineTestsPassed']==s['ownerPerformanceWorkloadsRun']==s['requirementsWaived']==0
assert not any(s[k] for k in ['productionChanged','publicApiChanged','freshCoreFullSuiteRun','nativePublicProviderBindingQualified','budgetsAccepted','ownerAccepted','pv01Complete','r3Complete','r4Accepted','apiFrozen','packagesBuilt','publicationAuthorized','pushPerformed'])
assert s['sourceBuildWarnings']==s['sourceBuildErrors']==0 and read(P/'PLAN.json')['nextSlice']=='PV-Q03'
for n,h in a['beforeHashes'].items():assert sha(R/n)==h,n
assert sha(R/a['candidatePins'])==a['candidatePinsSha256']
for base in [Q,R/'docs/foundation-provider-qualification']:
 for n,h in read(base/'MANIFEST.json').items():assert sha(base/n)==h,(base,n)
assert (E/'SOURCE-AND-BINARY-PINS.json').read_bytes()==(Q/'evidence/SOURCE-AND-BINARY-PINS.json').read_bytes()
for n,h in pins['sources'].items():assert sha(R/n)==h,n
for n,row in pins['engines'].items():assert sha(Path(row['path']))==row['sha256'],n
for n,h in pins['configuration'].items():assert sha(R/n)==h,n
for n,row in pins['corpora'].items():
 assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=R/'tests'/n,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=R/'tests'/n,text=True).strip()==row['status']==''
reuse=read(E/'QUALIFICATION-REUSE.json');assert reuse['sourceBinaryConfigurationUnchanged'] and not reuse['priorFullTestsReexecuted']
for n,h in reuse['priorFiles'].items():assert sha(Q/n)==h,n
units=read(Q/'evidence/UNIT-RESULTS.json');assert units['passed']==26338 and len(units['projects'])==10
with zipfile.ZipFile(Q/'evidence/EXECUTION-RECEIPTS.zip') as z:
 for row in units['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];counts=ET.fromstring(raw).find('.//{*}Counters').attrib
  assert counts['failed']==counts['notExecuted']=='0' and int(counts['passed'])==row['passed'];assert sha(Path(row['testBinaryPath']))==row['testBinarySha256']
 for name,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('public-primitives-'+name+'.log').decode('utf-8-sig');assert tuple(int(re.search(label+r':\s+(\d+)',log).group(1)) for label in ['Passed','Failed','Skipped'])==expected
api=read(Q/'evidence/public-api.json');assert len(api['assemblies'])==11 and sum(x['publicTypes'] for x in api['assemblies'])==318 and sum(x['declarationCount'] for x in api['assemblies'])==6465
for row in api['assemblies']:assert row['sha256'].lower()==pins['engines'][row['assembly']]['sha256']
assert N.resolve()==(R/'.worktrees/Bosak.Schema.Assessment').resolve() and W.resolve().is_relative_to(R/'.guard-tmp')
assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=N,text=True).strip()==a['nativeHead']
assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=N,text=True).strip()==a['nativeStatus']==''
for n,h in d['nativeSourceFiles'].items():assert sha(N/n)==h,n
for n,h in d['nativeFixtureFiles'].items():assert sha(N/n)==h,n
for n,h in d['baselineFiles'].items():assert sha(Path(d['originalBaseline'])/n)==h,n
for n,h in d['wrapperFiles'].items():assert sha(W/n)==h,n
layouts=read(E/'EXECUTION-LAYOUTS.json')
with zipfile.ZipFile(E/'EXECUTION-LAYOUTS.zip') as z:
 for group,layout in layouts.items():
  path=Path(layout['path']);actual={f.relative_to(path).as_posix() for f in path.rglob('*') if f.is_file()};assert actual==set(layout['files']),group
  for n,h in layout['files'].items():assert sha(path/n)==hashlib.sha256(z.read(group+'/'+n)).hexdigest()==h,(group,n)
assert layouts['baseline']['files']==d['baselineFiles']
changed={n for n,h in layouts['binary']['files'].items() if d['baselineFiles'].get(n)!=h};assert changed=={n+'.dll' for n in d['candidateEngines']}
assert set(layouts['binary']['files'])==set(d['baselineFiles'])
for n,h in d['unchangedConsumerFiles'].items():assert layouts['binary']['files'][n]==h,n
assert read(E/'BINARY-DELTA.json')['consumerRebuilt'] is False
for group in ['source','binary']:
 for n,h in d['candidateEngines'].items():assert layouts[group]['files'][n+'.dll']==h==pins['engines'][n]['sha256']
sets=[];results=read(E/'NATIVE-RESULTS.json');assert results['countsOverlapNotAdditive'] and results['distinctExistingCases']==451
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 required={'native-schema-lane-'+x+'.log.json' for x in ['prior-verify','source-build','source-build-offline','baseline','source','binary']};assert required<=set(z.namelist())
 for n in z.namelist():
  if n.endswith('.log.json'):
   receipt=json.loads(z.read(n));assert receipt['exitCode']==(1 if n=='native-schema-lane-source-build.log.json' else 0) and receipt['failure'] is None,n
   assert hashlib.sha256(z.read(n[:-5])).hexdigest()==receipt['logSha256']
 for group,row in results['lanes'].items():
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib;assert c['failed']==c['notExecuted']=='0' and c['passed']=='451'
  rows=[x.attrib for x in root.findall('.//{*}UnitTestResult')];assert rows==row['results'] and all(x['outcome']=='Passed' for x in rows);sets.append({x['testName'] for x in rows})
assert len(sets)==3 and sets[0]==sets[1]==sets[2] and len(sets[0])==451
for group in ['baseline','source','binary']:
 seen=set()
 for f in (E/'bindings'/group).glob('*.json'):
  b=read(f);assert not b['errors']
  for row in b['assemblies']:
   n=row['name']
   if n in d['candidateEngines']:
    expected=d['baselineFiles'][n+'.dll'] if group=='baseline' else d['candidateEngines'][n]
    assert row['sha256']==expected and row['context']=='Default' and not row['collectible'];seen.add(n)
 assert seen==set(d['candidateEngines']),group
foundation=read(E/'FOUNDATION-CANDIDATE.json');assert len(foundation['assemblies'])==7
for row in foundation['assemblies']:assert row['sha256']==pins['engines'][row['name']]['sha256']
package=read(E/'PACKAGE-PREFLIGHT.json');assert package['sourceVersion']=='1.81.0' and len(package['selectedPackages'])==9 and len(package['projects'])==11
assert not package['packagesBuilt'] and not package['publicationAuthorized'] and package['overlayNoticeIncluded']
for n,h in package['assets'].items():assert sha(R/n)==h,n
for row in package['commands']:assert sha(P/row['evaluation'])==row['sha256']
# Original documentation source digests are historical; packet manifest below still verifies their immutable receipt.
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for n,h in m.items():assert sha(P/n)==h,n
print('Verified TA-44:native451 baseline/source/unchanged-binary cases agree;exact nine-engine/default loads;only nine DLLs replaced;originals/Core1.81.0 unchanged. Counts overlap;public-provider/budget/owner/freeze/publication pending;next PV-Q03.')
