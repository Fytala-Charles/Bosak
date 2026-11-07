"""Verify actual-provider readiness preparation without promoting historical or synthetic evidence."""
from pathlib import Path
import hashlib,json,re,subprocess,xml.etree.ElementTree as ET,zipfile
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';Q=R/'docs/foundation-public-primitives'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
s=read(P/'STATUS.json');a=read(P/'ADOPTION.json');pins=read(E/'SOURCE-AND-BINARY-PINS.json')
assert s['sourceVersion']==a['sourceVersion']==pins['sourceVersion']=='1.81.0'
assert s['preparationComplete'] and s['checkerControlsPassed']==18 and s['actualProviderWorkloadsRun']==0 and s['newEngineTestsPassed']==0
assert not any(s[k] for k in ['productionChanged','publicApiChanged','freshEngineTestsRun','actualProviderQualified','performanceQualified','budgetsAccepted','pv01Complete','r3Complete','r4Accepted','ownerAccepted','apiFrozen','packagesBuilt','publicationAuthorized','pushPerformed'])
assert s['requirementsWaived']==0 and s['nextSlice']=='PV-Q02'
for n,h in a['beforeHashes'].items():assert sha(R/n)==h,n
for base in [Q,R/'docs/foundation-legacy-disposition']:
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
 total=0
 for row in units['projects']:
  raw=z.read(row['trx']);assert hashlib.sha256(raw).hexdigest()==row['sha256'];root=ET.fromstring(raw);counts=root.find('.//{*}Counters').attrib
  assert counts['failed']==counts['notExecuted']=='0' and int(counts['passed'])==row['passed'];total+=row['passed'];assert sha(Path(row['testBinaryPath']))==row['testBinarySha256']
 assert total==26338
 for name,expected in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
  log=z.read('public-primitives-'+name+'.log').decode('utf-8-sig');assert tuple(int(re.search(label+r':\s+(\d+)',log).group(1)) for label in ['Passed','Failed','Skipped'])==expected
api=read(Q/'evidence/public-api.json');assert len(api['assemblies'])==11 and sum(x['publicTypes'] for x in api['assemblies'])==318 and sum(x['declarationCount'] for x in api['assemblies'])==6465
for row in api['assemblies']:assert row['sha256'].lower()==pins['engines'][row['assembly']]['sha256']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 required={'provider-qualification-'+x+'.log.json' for x in ['prior-verify','controls-qualified','template-preflight-final']};assert required<=set(z.namelist())
 for n in z.namelist():
  if n.endswith('.log.json'):
   receipt=json.loads(z.read(n));expected=2 if n in {'provider-qualification-template-preflight.log.json','provider-qualification-template-preflight-final.log.json'} else 0
   assert receipt['exitCode']==expected and receipt['failure'] is None,n
   assert hashlib.sha256(z.read(n[:-5])).hexdigest()==receipt['logSha256']
 assert 'Ran 18 tests' in z.read('provider-qualification-controls-qualified.log').decode('utf-8-sig')
 result=json.loads(z.read('provider-qualification-template-preflight-final.log').decode('utf-8-sig'));assert not result['structureReady'] and not result['qualificationAccepted'] and not result['ownerAcceptanceVerified'] and result['errors']
 assert result==read(E/'TEMPLATE-PREFLIGHT.json')
scenarios=read(P/'PF-MATRIX.json')['scenarios'];assert [r['id'] for r in scenarios]==[f'PF-{i:02d}' for i in range(1,10)]
assert all(r['ownerBudget'] is None and r['actualCandidateRunReceipt'] is None and not r['accepted'] for r in scenarios)
assert len(read(P/'QUALIFICATION-MATRIX.json')['rows'])==8
snapshot=read(E/'OWNER-SOURCE-SNAPSHOT.json');assert snapshot['readOnly'] and len(snapshot['records'])==4
allowed={'d:/development/bosak/.worktrees/bosak.schema.assessment','d:/development/bosak.schema.public','d:/development/bosak/.worktrees/bosak.xmldb.foundation','d:/development/bosak/.worktrees/bosak.braid.readonly'}
for row in snapshot['records']:
 root=Path(row['root']);assert str(root.resolve()).replace('\\','/').lower() in allowed
 assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=root,text=True).strip()==row['head']
 for n,data in row['files'].items():assert (sha(root/n) if (root/n).is_file() else None)==data['currentSha256']
 for n,h in row['scanSourceHashes'].items():assert sha(root/n)==h,(root,n)
for row in read(E/'OWNER-RETURN-REVIEW.json')['returns']:
 assert sha(Path(row['path']))==row['sha256'] and not row['runtimeQualification'] and not row['c45Accepted'] and not row['apiFrozen']
contract=read(P/'RUN-CONTRACT.json');assert not contract['checkerCanExecuteWorkloads'] and not contract['checkerCanAuthenticateOwnerApproval'] and not contract['checkerCanAcceptQualification']
package=read(E/'PACKAGE-PREFLIGHT.json');scope=read(P/'PACKAGE-SCOPE.json');assert package['sourceVersion']=='1.81.0' and len(package['selectedPackages'])==9 and len(package['projects'])==11
assert scope['rootProject']=='Bosak.Xslt' and scope['currentSelectedClosure']==9 and scope['selectedPackages']==package['selectedPackages']
assert not package['packagesBuilt'] and not package['publicationAuthorized'] and package['overlayNoticeIncluded']
for n,h in package['assets'].items():assert sha(R/n)==h,n
for row in package['commands']:assert sha(P/row['evaluation'])==row['sha256']
for row in read(E/'DOCUMENTATION-CHECKS.json')['pages']:assert sha(R/row['source'])==row['sourceSha256']
if (P/'MANIFEST.json').exists():
 m=read(P/'MANIFEST.json');assert set(m)=={f.relative_to(P).as_posix() for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
 for n,h in m.items():assert sha(P/n)==h,n
print('Verified TA-43 readiness only:18 checker controls/template refusal;four read-only owner snapshots/nine PF gaps;Core1.81.0 unchanged with retained26338/gates/API. Actual/native/budget acceptance0;next PV-Q02.')
