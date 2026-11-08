"""Verify the fresh Integration attempt without launching any tests."""
from pathlib import Path
import hashlib,json,subprocess,xml.etree.ElementTree as ET
R=Path(__file__).resolve().parents[3];P=R/'docs/foundation-xmldb-manual-approval';F=R/'docs/foundation-xmldb-full-qualification'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
s=read(P/'RESULTS.json');e=read(P/'fresh-review-execution.json');assert e['exitCode']==1
log=R/e['log']; receipt=read(log.with_suffix(log.suffix+'.json'))
assert receipt['exitCode']==1 and receipt['failure'] is None and sha(log)==receipt['logSha256']
assert not receipt['qualificationAccepted']
assert receipt['command'][0:2]==['dotnet','vstest']
assert Path(receipt['command'][2]).name=='Bosak.XmlDb.IntegrationTests.dll'
trx=P/s['trx'];assert sha(trx)==s['trxSha256'];tree=ET.parse(trx)
c=tree.find('.//{*}Counters').attrib;assert c['total']=='863' and c['passed']=='862' and c['failed']=='1' and c['notExecuted']=='0'
assert tree.find('.//{*}ResultSummary').attrib['outcome']=='Failed'
names=sorted(x.attrib['testName'] for x in tree.findall('.//{*}UnitTestResult'))
assert len(names)==len(set(names))==863 and names==s['caseNames']
prior=read(F/'evidence/RESULTS.json');hist=next(x['historicalTrx'] for x in prior['rows'] if x['lane']=='integration-inprocess');plan=read(F/'PLAN.json');owner=Path(plan['ownerRoot']);baseline=owner/hist['path'];assert sha(baseline)==hist['sha256']
assert names==sorted(x.attrib['testName'] for x in ET.parse(baseline).findall('.//{*}UnitTestResult'))
assert subprocess.check_output(['git','--no-optional-locks','-C',str(owner),'rev-parse','HEAD'],text=True).strip()==plan['ownerHead']
assert not subprocess.check_output(['git','--no-optional-locks','-C',str(owner),'status','--porcelain'],text=True).strip()
layout=e['layout'];folder=Path(layout['path']);assert layout==read(F/'evidence/GUARDED-INTEGRATION.json')['layout']
assert layout['files']=={f.relative_to(folder).as_posix():sha(f) for f in folder.rglob('*') if f.is_file()}
packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json');seen={}
for file in (P/'bindings-fresh-review').glob('*.json'):
 report=read(file);assert not report['captureErrors']
 for load in report['assemblies']:
  if load['name'] in packages['loadedEngines']:
   assert load['sha256']==packages['loadedEngines'][load['name']] and load['context']=='Default' and load['collectible']=='False'
   seen[load['name']]=load['sha256']
assert seen==s['loadedPackageEngines'] and len(seen)==7
reopen=P/s['reopenTrx'];assert sha(reopen)==s['reopenTrxSha256'];rt=ET.parse(reopen);rc=rt.find('.//{*}Counters').attrib;assert rc['total']==rc['passed']=='1' and rc['failed']=='0'
failed=[x.attrib['testName'] for x in tree.findall('.//{*}UnitTestResult') if x.attrib['outcome']!='Passed']
assert failed==[s['fullRunFailure']]==[x.attrib['testName'] for x in rt.findall('.//{*}UnitTestResult')]
rl=read(P/'REOPEN-LAYOUT.json');rr=Path(rl['root']);assert rl['files']=={f.relative_to(rr).as_posix():sha(f) for f in rr.rglob('*') if f.is_file()}
assert rl['testPayloadUnchanged'] and rl['sampleOutputUnchanged']
reopenLog=P/'xmldb-reopen-layout-20261108.log';reopenReceipt=read(reopenLog.with_suffix('.log.json'));assert reopenReceipt['exitCode']==0 and reopenReceipt['failure'] is None and sha(reopenLog)==reopenReceipt['logSha256']
reopenSeen={}
for file in (P/'bindings-reopen').glob('*.json'):
 report=read(file);assert not report['captureErrors']
 for load in report['assemblies']:
  if load['name'] in packages['loadedEngines']:
   assert load['sha256']==packages['loadedEngines'][load['name']] and load['context']=='Default' and load['collectible']=='False'
   reopenSeen[load['name']]=load['sha256']
assert reopenSeen==rl['sampleEngines'] and len(reopenSeen)==7
assert s['splitRunCoverageComplete'] and not s['singleFullRunGreen']
assert s['distinctCompletedCases']==3572+47+863==4482 and s['integrationCasesNotQualified']==0
assert not any(s[k] for k in ['ownerAccepted','apiFrozen','publicationAuthorized'])
assert sha(P/'CONSUMED.json')==s['historicalConsumedApprovalSha256']
assert read(P/'MANIFEST.json')=={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f!=P/'MANIFEST.json' and '__pycache__' not in f.parts}
print('Verified 862 full-run passes plus one layout-corrected reopen pass; exact 863-case catalog and pinned package loads. Aggregate 4482 distinct passes across runs; original full-run failure preserved.')
