"""Retain the completed in-process source scope and explicit Integration approval restriction."""
from pathlib import Path
import hashlib,json,xml.etree.ElementTree as ET,zipfile

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
plan=read(P/'PLAN.json');source=Path(plan['source']);owner=Path(plan['ownerRoot'])
baseline=read(owner/'docs/foundation-requirements/native-lifetime/results.json')
rows=[]
executions=[r for r in read(E/'INPROCESS-EXECUTIONS.json') if r['exitCode']==0]+read(E/'FOCUSED-EXECUTIONS.json')
for execution in executions:
    assert execution['exitCode']==0
    files=list((E/'trx'/execution['slug']).glob('*.trx'));assert len(files)==1
    file=files[0];tree=ET.parse(file);c=tree.find('.//{*}Counters').attrib
    assert int(c['passed'])==execution['expected'] and c['failed']==c['notExecuted']=='0'
    cases=sorted(r.attrib['testName'] for r in tree.findall('.//{*}UnitTestResult'))
    prior=next(r for r in baseline['selectedWindows']+baseline['selectedFocused'] if int(r['counts']['passed'])==execution['expected'])
    old=owner/prior['path'];assert sha(old)==prior['sha256']
    assert cases==sorted(r.attrib['testName'] for r in ET.parse(old).findall('.//{*}UnitTestResult'))
    rows.append(dict(project=execution['project'],passed=execution['expected'],failed=0,caseNames=cases,
        trx=file.relative_to(P).as_posix(),trxSha256=sha(file),execution=execution,historicalTrx=prior))
assert sum(r['passed'] for r in rows)==186
changes={name:sha(source/name) for name,h in plan['sourceFiles'].items() if sha(source/name)!=h}
assert all(Path(name).name=='packages.lock.json' for name in changes),changes
packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')
bindings={}
for row in rows:
    seen={}
    for file in (E/'bindings'/row['execution']['slug']).glob('*.json'):
        report=read(file);assert not report['captureErrors']
        for load in report['assemblies']:
            if load['name'] in packages['loadedEngines']:
                assert load['sha256']==packages['loadedEngines'][load['name']] and load['context']=='Default' and load['collectible']=='False'
                seen[load['name']]=load['sha256']
    bindings[row['project']]=seen
assert {'Bosak.XPath.Api','Bosak.XPath.Core','Bosak.XPath.Runtime'}<=set(bindings['Bosak.XmlDb.Xdm.Tests'])
result=dict(sourceBuildPassed=True,sourceBodiesChanged=False,inProcessPassed=186,projects=rows,loadedPackageEngines=bindings,
    dependencyLockChanges=changes,integrationNotRun=863,storageCasesWithoutCompletedResult=3433,fullStorageCancelled=True,fullQualificationComplete=False,
    initialXdmNativeRenameFailureRetained=True,ownerAccepted=False,publicationAuthorized=False)
(E/'RESULTS.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','x',zipfile.ZIP_DEFLATED) as z:
    for log in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('xmldb-source-*.log')):
        receipt=log.with_suffix('.log.json');r=read(receipt)
        failed=log.name in ['xmldb-source-test-Bosak.XmlDb.Xdm.Tests.log','xmldb-source-test-Bosak.XmlDb.Storage.Tests-inprocess.log']
        assert (r['exitCode']!=0 if failed else r['exitCode']==0) and r['failure'] is None and r['logSha256']==sha(log)
        z.write(log,log.name);z.write(receipt,receipt.name)
with zipfile.ZipFile(E/'TRX-RESULTS.zip','x',zipfile.ZIP_DEFLATED) as z:
    for file in sorted((E/'trx').rglob('*.trx')):z.write(file,file.relative_to(P).as_posix())
print('Captured unchanged-source build and 186 passing scoped cases; initial native rename failure, cancelled full Storage sweep and blocked Integration remain separate. Proprietary source/binaries stay in ignored workspace.')
