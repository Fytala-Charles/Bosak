"""Verify policy-only complete-container selection and unchanged retained qualification."""
from pathlib import Path
import hashlib,json,zipfile,re,subprocess,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';Q=R/'docs/foundation-public-primitives'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
s=read(P/'STATUS.json');a=read(P/'ADOPTION.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json')
assert s['sourceVersion']==a['sourceVersion']==pins['sourceVersion']=='1.81.0'
assert s['dispositionSelected'] and s['decisionCount']==12 and s['pvL01CoreDispositionCompleteWithScope'] and s['existingCharacterizationTestsPassed']==289
assert s['newTestsPassed']==s['requirementsWaived']==0
assert not any(s[k] for k in ['productionChanged','publicApiChanged','freshFullRun','automaticLegacyBridgeImplemented','pv01Complete','ownerAccepted','apiFrozen','publicationAuthorized','packagesBuilt','pushPerformed'])
assert a['policyBeforeImplementation'] and not a['productionChanged']
for n,h in a['beforeHashes'].items():assert sha(R/n)==h,n
for n,h in read(Q/'MANIFEST.json').items():assert sha(Q/n)==h,n
assert (E/'SOURCE-AND-BINARY-PINS.json').read_bytes()==(Q/'evidence/SOURCE-AND-BINARY-PINS.json').read_bytes()
for n,h in pins['sources'].items():assert sha(R/n)==h,n
for n,row in pins['engines'].items():assert sha(Path(row['path']))==row['sha256'],n
for n,h in pins['configuration'].items():assert sha(R/n)==h,n
assert sha(Path(pins['observerPath']))==pins['observerSha256']
for n,row in pins['corpora'].items():
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=R/'tests'/n,text=True).strip()==row['revision']
 assert subprocess.check_output(['git','status','--porcelain'],cwd=R/'tests'/n,text=True).strip()==row['status']==''
disposition=read(P/'DISPOSITION.json');assert len(disposition['decisions'])==12 and disposition['requirementsWaived']==0
assert disposition['status']=='CoreDispositionCompleteWithScope' and not disposition['publicLegacyObservationSelected'] and not disposition['automaticBridgeSelected']
assert [x['id'] for x in disposition['decisions']]==[f'LG{i:02d}' for i in range(1,13)]
checks=read(P/'REVIEW-CHECKLIST.json');assert len(checks['checks'])==12 and checks['additionalPassedTests']==0
assert all(c['status']=='CompletedCoreStaticReview' for c in checks['checks']) and not checks['ownerAccepted']
assert not read(P/'SIGNATURE-DISPOSITION.json')['automaticLegacyBridgeImplemented']
assert read(P/'PLAN.json')['nextSlice']=='PV-Q01'
audit=read(E/'LEGACY-BOUNDARY-AUDIT.json');assert audit['privateTypesNotExported']
for n,h in audit['sourceFiles'].items():assert sha(R/n)==h,n
assert sha(R/audit['apiInventory'])==audit['apiInventorySha256']
for row in audit['receipts']:
 lines=(R/row['path']).read_text(encoding='utf-8').splitlines();text='\n'.join(lines[row['startLine']-1:row['endLine']])+'\n'
 assert text==row['snippet'] and hashlib.sha256(text.encode()).hexdigest()==row['normalizedSnippetSha256']
 assert sha(R/row['path'])==row['sourceSha256']
api=read(R/audit['apiInventory']);declarations=[x for assembly in api['assemblies'] for x in assembly['declarations'] if x.startswith('T ')]
assert not any('SchemaEvidenceKernel' in x or 'QuerySchemaAssociation' in x for x in declarations)
reuse=read(E/'QUALIFICATION-REUSE.json');assert reuse['sourceBinaryConfigurationUnchanged'] and not reuse['priorFullTestsReexecuted']
for n,h in reuse['priorFiles'].items():assert sha(Q/n)==h,n
units=read(Q/'evidence/UNIT-RESULTS.json');assert len(units['projects'])==10 and units['passed']==26338
with zipfile.ZipFile(Q/'evidence/EXECUTION-RECEIPTS.zip') as z:
 total=0
 for row in units['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];root=ET.fromstring(raw);c=root.find('.//{*}Counters').attrib
  assert c['failed']==c['notExecuted']=='0' and int(c['passed'])==row['passed'];total+=row['passed']
  assert sha(Path(row['testBinaryPath']))==row['testBinarySha256']
 assert total==26338
 for name,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('public-primitives-'+name+'.log').decode('utf-8-sig')
  assert tuple(int(re.search(label+r':\s+(\d+)',log).group(1)) for label in ['Passed','Failed','Skipped'])==expected
api=read(Q/'evidence/public-api.json');assert len(api['assemblies'])==11 and sum(x['publicTypes'] for x in api['assemblies'])==318 and sum(x['declarationCount'] for x in api['assemblies'])==6465
c=read(E/'CHARACTERIZATION.json');assert c['existingTestsPassed']==289 and c['newTestsPassed']==0 and len(c['projects'])==2
assert sorted(row['passed'] for row in c['projects'])==[30,259] and len(c['results'])==289
assert all(row['outcome']=='Passed' for row in c['results'])
assert set(c['classes'])=={'SchemaEvidenceKernelTests','SchemaValueTransportTests','SchemaQuerySessionTests','SchemaProviderBindingTests','QuerySchemaAssociationTests','QueryTypedEvidenceTests','SchemaAtomicsTests','PublicTypedIdentityTests'}
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 for row in c['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];tree=ET.fromstring(raw)
  counts=tree.find('.//{*}Counters').attrib;assert counts['failed']==counts['notExecuted']=='0' and int(counts['passed'])==row['passed']
  assert all(x.attrib in c['results'] for x in tree.findall('.//{*}UnitTestResult'))
  assert sha(Path(row['testBinaryPath']))==row['testBinarySha256']
  assert row['testBinarySha256']==next(x for x in units['projects'] if x['testAssembly'].casefold()==Path(row['testBinaryPath']).stem.casefold())['testBinarySha256']
 for n in z.namelist():
  if n.endswith('.log.json'):
   receipt=json.loads(z.read(n));assert receipt['exitCode']==0 and receipt['failure'] is None
   assert hashlib.sha256(z.read(n[:-5])).hexdigest()==receipt['logSha256']
seen=set()
for f in (E/'bindings').glob('*.json'):
 b=read(f);assert not b['errors']
 for x in b['assemblies']:
  if x['name'] in pins['engines']:
   assert x['sha256']==pins['engines'][x['name']]['sha256'] and x['context']=='Default' and not x['collectible'];seen.add(x['name'])
assert {'Bosak.XPath.Core','Bosak.XPath.Runtime','Bosak.XPath.Api'}<=seen
package=read(E/'PACKAGE-PREFLIGHT.json');assert package['sourceVersion']=='1.81.0' and len(package['selectedPackages'])==9 and len(package['projects'])==11
assert not package['packagesBuilt'] and not package['publicationAuthorized'] and package['overlayNoticeIncluded']
for n,h in package['assets'].items():assert sha(R/n)==h,n
for row in package['commands']:assert sha(P/row['evaluation'])==row['sha256']
for row in read(E/'DOCUMENTATION-CHECKS.json')['pages']:assert sha(R/row['source'])==row['sourceSha256']
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for n,h in m.items():assert sha(P/n)==h,n
print('Verified TA-42 Core legacy disposition:12 decisions,fresh30 Core+259 Runtime tests;unchanged1.81.0/retained26338/gates/API;no opaque-field cast or marker promotion. Owner/native/budget/freeze/publication remain open;next PV-Q01.')
