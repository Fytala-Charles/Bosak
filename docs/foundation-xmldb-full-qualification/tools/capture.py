"""Retain the full Storage result and separately approved in-process Integration scopes."""
from pathlib import Path
import hashlib,json,xml.etree.ElementTree as ET,zipfile

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=read(P/'PLAN.json');owner=Path(p['ownerRoot']);prior=R/'docs/foundation-xmldb-source-candidate'
baseline=read(owner/'docs/foundation-requirements/native-lifetime/results.json')
packaged=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')['loadedEngines']
old=read(prior/'evidence/RESULTS.json');xdm=next(r for r in old['projects'] if r['project']=='Bosak.XmlDb.Xdm.Tests')
assert xdm['passed']==47 and sha(prior/xdm['trx'])==xdm['trxSha256']
rows=[]
for lane,expected,classes in [
    ('storage',3572,None),('integration-inprocess',39,['NativeSnapshotWorkTests']),
    ('integration-pilot',30,['ControlledPilotConcurrencyTests','ControlledPilotDriverTests'])]:
    execution=read(E/(lane+'-execution.json'));assert execution['exitCode']==0
    files=list((E/'trx'/lane).glob('*.trx'));assert len(files)==1
    trx=files[0];tree=ET.parse(trx);counts=tree.find('.//{*}Counters').attrib
    assert int(counts['passed'])==expected and counts['failed']==counts['notExecuted']=='0'
    names=sorted(r.attrib['testName'] for r in tree.findall('.//{*}UnitTestResult'))
    b=next(r for r in baseline['selectedWindows'] if int(r['counts']['passed'])==(3572 if lane=='storage' else 863))
    old_trx=owner/b['path'];assert sha(old_trx)==b['sha256']
    old_names=sorted(r.attrib['testName'] for r in ET.parse(old_trx).findall('.//{*}UnitTestResult'))
    selected=old_names if classes is None else [n for n in old_names if any('.'+c+'.' in n for c in classes)]
    assert names==selected,(lane,len(names),len(selected))
    loaded={};reports=list((E/'bindings'/lane).glob('*.json'));assert reports
    for report_file in reports:
        report=read(report_file);assert not report['captureErrors']
        for load in report['assemblies']:
            if load['name'] in packaged:
                assert load['sha256']==packaged[load['name']] and load['context']=='Default' and load['collectible']=='False'
                loaded[load['name']]=load['sha256']
    rows.append(dict(lane=lane,passed=expected,failed=0,caseNames=names,trx=trx.relative_to(P).as_posix(),
        trxSha256=sha(trx),execution=execution,loadedPackageEngines=loaded,historicalTrx=b))
assert sum(r['passed'] for r in rows)+47==3688
integration_names=[n for row in rows if row['lane'].startswith('integration') for n in row['caseNames']]
assert len(set(integration_names))==69
result=dict(previewVersion=p['previewVersion'],ownerHead=p['ownerHead'],storagePassed=3572,xdmReusedPassed=47,
    xdmReceiptSha256=sha(prior/'evidence/RESULTS.json'),integrationInProcessPassed=69,integrationCasesNotQualified=794,
    distinctCompletedCases=3688,fullSourceQualificationComplete=False,rows=rows,
    explicitUserExecutionAuthorization=True,automaticReviewRejectedFullIntegration=True,
    fullIntegrationLaunched=False,sourceProductionBodiesChanged=False,
    diagnosticEntryGuardOnly=True,ownerAccepted=False,apiFrozen=False,publicationAuthorized=False)
(E/'RESULTS.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','x',zipfile.ZIP_DEFLATED) as z:
    for log in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('xmldb-full-*.log')):
        receipt=log.with_suffix('.log.json');record=read(receipt)
        assert record['exitCode']==0 and record['failure'] is None and record['logSha256']==sha(log)
        z.write(log,log.name);z.write(receipt,receipt.name)
with zipfile.ZipFile(E/'TRX-RESULTS.zip','x',zipfile.ZIP_DEFLATED) as z:
    for file in sorted((E/'trx').rglob('*.trx')):z.write(file,file.relative_to(P).as_posix())
print('Captured full Storage3572, reused XDM47 and in-process Integration69; 794 Integration cases remain unqualified after automatic review rejection.')
