"""Verify the fresh versioned production patch; earlier source cohorts remain historical evidence."""
from pathlib import Path
import hashlib,json,re,subprocess,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
a=read(P/'ADOPTION.json');s=read(P/'STATUS.json');q=read(P/'QUALIFICATION.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json');n=Path(a['nativeRoot'])
assert a['recordedBeforeImplementation'] and a['previousSourceVersion']=='1.81.0' and a['sourceVersion']==s['sourceVersion']==pins['sourceVersion']=='1.81.1'
assert s['productionPatchAdopted'] and s['freshQualificationCompleteWithinScope'] and s['unitTestsPassed']==q['unitPassed']==26342 and s['languageServerTestsPassed']==84
assert s['buildErrors']==s['newSourceWarnings']==s['requirementsWaived']==0 and s['existingBuildWarnings']==8
assert not any(s[k] for k in ['ownerAccepted','budgetsAccepted','apiFrozen','packagesBuilt','pushPerformed'])
# Archive validation is independent of mutable current-source assertions in old verifiers.
old=R/'docs/foundation-captured-identity-qualification'
for name,h in read(old/'MANIFEST.json').items():assert sha(old/name)==h,name
for name,h in a['beforeFiles'].items():
 if name.startswith('docs/'):assert sha(R/name)==h,name
with zipfile.ZipFile(E/'PRODUCTION-BEFORE.zip') as z:
 for name in z.namelist():assert hashlib.sha256(z.read(name)).hexdigest()==a['beforeFiles'][name],name
for name,h in a['nativeFiles'].items():assert sha(n/name)==h,name
assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=n,text=True).strip()==a['nativeHead']
assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=n,text=True).strip()==''
for name,h in pins['sources'].items():assert sha(R/name)==h,name
for name,h in pins['configuration'].items():assert sha(R/name)==h,name
for name,row in pins['engines'].items():assert sha(Path(row['path']))==row['sha256'],name
for name,row in pins['corpora'].items():
 assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=R/'tests'/name,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=R/'tests'/name,text=True).strip()==row['status']==''
before=read(R/'docs/foundation-public-primitives/evidence/SOURCE-AND-BINARY-PINS.json')
assert {name for name,h in pins['sources'].items() if before['sources'].get(name)!=h}==set(pins['changedSourceFiles'])
assert len(pins['changedSourceFiles'])==4 and '<Version>1.81.1</Version>' in (R/'src/Directory.Build.props').read_text(encoding='utf-8')
candidate=R/'docs/foundation-captured-identity-candidate/candidate/QueryNodeFacts.cs';adopted=R/'src/Bosak.XPath.Runtime/Vm/QueryNodeFacts.cs'
assert candidate.read_text(encoding='utf-8').split('using Bosak.XPath.Core.Schema;',1)[1]==adopted.read_text(encoding='utf-8').split('using Bosak.XPath.Core.Schema;',1)[1]
for name,h in read(E/'ADOPTED-SOURCE.json')['changedFiles'].items():assert sha(R/name)==h,name
for name,h in read(E/'SOURCE-PINS.json').items():assert sha(P/name)==h,name
units=read(E/'UNIT-RESULTS.json');assert units['passed']==26342 and len(units['projects'])==10
for row in units['projects']:
 assert row['exitCode']==0 and sha(Path(row['launchedBinary']))==row['testSha256']
 trx=P/row['trx'];assert sha(trx)==row['trxSha256'];root=ET.parse(trx).getroot();c=root.find('.//{*}Counters').attrib
 assert int(c['passed'])==row['expected']==row['passed'] and c['failed']==c['notExecuted']=='0'
 assert sorted(x.attrib['testName'] for x in root.findall('.//{*}UnitTestResult'))==row['caseNames']
assert sum(row['passed'] for row in units['projects'])==26342
assert next(row for row in units['projects'] if 'database.tests' in row['project'])['localHttpAuthorization']
for name,count in [('languageServer',84),('nativeSchema',451),('nativeAdapter',59),('publicConsumer',10)]:
 row=read(E/'EXTRA-RESULTS.json')['lanes'][name];trx=P/row['trx'];assert sha(trx)==row['sha256'];root=ET.parse(trx).getroot();c=root.find('.//{*}Counters').attrib
 assert int(c['passed'])==count and c['failed']==c['notExecuted']=='0'
 assert sorted(x.attrib['testName'] for x in root.findall('.//{*}UnitTestResult'))==row['caseNames']
api=read(E/'public-api-complete.json');baseline=read(old/'evidence/public-api.json');comparison=read(E/'API-COMPARISON.json')
assert sha(E/'public-api-complete.json')==comparison['candidateSha256'] and sha(old/'evidence/public-api.json')==comparison['baselineSha256']
assert len(api['assemblies'])==11 and sum(x['publicTypes'] for x in api['assemblies'])==318 and sum(x['declarationCount'] for x in api['assemblies'])==6465
for x,y in zip(api['assemblies'],baseline['assemblies'],strict=True):
 for key in ['project','assembly','references','friendAssemblies','publicTypes','declarationCount','declarations']:assert x[key]==y[key],(x['assembly'],key)
 assert x['identity']==y['identity'].replace('Version=1.81.0.0','Version=1.81.1.0')
 assert x['sha256'].lower()==pins['engines'][x['assembly']]['sha256']
gates=read(E/'GATE-RESULTS.json')
for name,expected in [('qt3',[31397,0,424]),('qt4',[8941,0,1918])]:
 log=R/f'docs/foundation-r2-closure/evidence/execution/captured-production-{name}.log';text=log.read_text(encoding='utf-8-sig');row=gates['gates'][name]
 assert [int(re.search(r'^'+key+r':\s+(\d+)',text,re.M).group(1)) for key in ['Passed','Failed','Skipped']]==expected
 assert sha(log)==row['logSha256'] and sha(E/(name+'-skips.txt'))==row['skipSha256']==row['baselineSkipSha256']
 assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-public-primitives/evidence'/(name+'-skips.txt')).read_bytes()
assert gates['qt4ExperimentalPassed']==11
retained=read(E/'RETAINED-FOUNDATION.json');manifest=read(E/'FOUNDATION-CANDIDATE.json');assert retained['passed']==20 and retained['failed']==0 and len(retained['cases'])==20 and len(manifest['assemblies'])==7
assert {r['name']:r['sha256'] for r in manifest['assemblies']}=={r['name']:r['sha256'] for r in retained['loadedAssemblies']}
for row in retained['loadedAssemblies']:assert row['sha256']==pins['engines'][row['name']]['sha256']
layouts=read(E/'EXECUTION-LAYOUTS.json')
with zipfile.ZipFile(E/'EXECUTION-LAYOUTS.zip') as z:
 for group,layout in layouts.items():
  path=Path(layout['path']);assert {p.relative_to(path).as_posix() for p in path.rglob('*') if p.is_file()}==set(layout['files'])
  for name,h in layout['files'].items():assert sha(path/name)==hashlib.sha256(z.read(group+'/'+name)).hexdigest()==h,(group,name)
  for name,row in pins['engines'].items():
   if name+'.dll' in layout['files']:assert layout['files'][name+'.dll']==row['sha256'],(group,name)
assert layouts['retained-foundation']['files']['LegacyCaller.dll']==retained['callerSha256']
for group,expected in read(E/'BINDING-SUMMARY.json')['groups'].items():
 observed={}
 for file in (E/group).glob('*.json'):
  b=read(file);assert not b['errors']
  for row in b['assemblies']:
   if row['name'] in pins['engines']:assert row['sha256']==pins['engines'][row['name']]['sha256'] and row['context']=='Default' and not row['collectible'];observed[row['name']]=row['sha256']
 assert observed==expected and 'Bosak.XPath.Runtime' in observed
package=read(E/'PACKAGE-PREFLIGHT.json');assert package['sourceVersion']=='1.81.1' and package['date']=='2026-11-07' and len(package['selectedPackages'])==9 and len(package['projects'])==11 and not package['packagesBuilt'] and package['overlayNoticeIncluded']
for name,h in package['assets'].items():assert sha(R/name)==h,name
for row in package['commands']:assert sha(P/row['evaluation'])==row['sha256']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for name in z.namelist():
  if name.endswith('.log.json'):assert json.loads(z.read(name))['exitCode']==0,name
 build=z.read('captured-production-build.log').decode('utf-8-sig');assert '8 Warning(s)' in build and '0 Error(s)' in build
 for line in build.splitlines():
  if ': warning ' in line:assert 'QueryNodeFacts.cs' not in line and 'CapturedIdentityTests.cs' not in line
subprocess.run(['python',str(P/'tools/check_docs.py')],check=True)
if (P/'MANIFEST.json').exists():
 manifest=read(P/'MANIFEST.json');assert set(manifest)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json' and not {'bin','obj','__pycache__'}.intersection(p.relative_to(P).parts)}
 for name,h in manifest.items():assert sha(P/name)==h,name
print('Verified TA-55:fresh1.81.1 production patch;26342 solution+84 language-server,451 native/59 adapter/10 consumer/20 retained,exact frozen gates and version-only API delta. Owner budget/adoption/freeze/publication pending;next PV-Q05.')
