"""Collect exact-case results and retain initial setup failures separately."""
from pathlib import Path
import hashlib,json,re,shutil,xml.etree.ElementTree as ET,zipfile
from compare_api import compare

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
plan=read(P/'PLAN.json');prior=R/'docs/foundation-captured-identity-production/evidence'
old_units=read(prior/'UNIT-RESULTS.json')['projects'];old_extra=read(prior/'EXTRA-RESULTS.json')['lanes']
rows={}
for file in ['units-executions.json','database-executions.json','units-resume-executions.json']:
    for row in read(E/file):
        if row['exitCode']==0:rows[row['key']]=row
assert set(rows)==set(plan['layouts'])-{'qt3','qt4'}
units=[];extras=[]
for key,execution in rows.items():
    layout=plan['layouts'][key]
    expected_binary=next(h for name,h in layout['files'].items() if name.lower()==Path(execution['binary']).name.lower())
    assert sha(Path(execution['binary']))==expected_binary
    folder=Path(execution.get('trxDirectory',str(E/'trx'/key.replace('/','-'))))
    files=list(folder.glob('*.trx'));assert len(files)==1,(key,files)
    trx=files[0];tree=ET.parse(trx);counts=tree.find('.//{*}Counters').attrib
    cases=sorted(r.attrib['testName'] for r in tree.findall('.//{*}UnitTestResult'))
    assert int(counts['passed'])==layout['expected'] and counts['failed']==counts['notExecuted']=='0',(key,counts)
    baseline=next(r for r in old_units if key.endswith('/'+r['project'])) if key.startswith('units/') else old_extra[{'languageServer':'languageServer','native451':'nativeSchema','nativeAdapter':'nativeAdapter','consumer':'publicConsumer'}[key]]
    assert cases==sorted(baseline['caseNames']),key
    row=dict(key=key,passed=int(counts['passed']),failed=0,caseNames=cases,
        trx=trx.relative_to(P).as_posix(),trxSha256=sha(trx),execution=execution)
    (units if key.startswith('units/') else extras).append(row)
assert sum(r['passed'] for r in units)==26342
gates={}
for key,expected in [('qt3',[31397,0,424]),('qt4',[8941,0,1918])]:
    log=R/f'docs/foundation-r2-closure/evidence/execution/preview-qualification-{key}.log';text=log.read_text(encoding='utf-8-sig')
    counts=[int(re.search(r'^'+label+r':\s+(\d+)',text,re.M).group(1)) for label in ['Passed','Failed','Skipped']]
    assert counts==expected and (E/(key+'-skips.txt')).read_bytes()==(prior/(key+'-skips.txt')).read_bytes()
    gates[key]=dict(passed=counts[0],failed=0,skipped=counts[2],logSha256=sha(log),skipSha256=sha(E/(key+'-skips.txt')),exactSkipMapEqual=True)
assert 'Experimental-level (XPath40Experimental) passes: 11' in text
result=dict(previewVersion=plan['previewVersion'],candidateCommit=plan['candidateCommit'],unitPassed=26342,
    units=units,extras=extras,gates=gates,qt4ExperimentalPassed=11,api=compare(),
    ownerAccepted=False,budgetsAccepted=False,apiFrozen=False,publicationAuthorized=False,
    initialRuntimeFixtureFailuresRetained=51,initialApiDependencyFailureRetained=True,
    qualificationScope='Exact nine package binaries in unchanged compiled regression/harness/consumer overlays; owner workload/budget and production-provider acceptance remain separate.')
(E/'RESULTS.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
bindings={}
for key,layout in plan['layouts'].items():
    if key in rows:folder=Path(rows[key].get('bindingsDirectory',str(E/'bindings'/key.replace('/','-'))))
    else:folder=E/'bindings'/key
    reports=list(folder.glob('*.json'));assert reports,key
    packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')['loadedEngines'];seen={}
    for file in reports:
        report=read(file);assert not report['captureErrors'],file
        for row in report['assemblies']:
            if row['name'] in packages:
                assert row['sha256']==packages[row['name']] and row['context']=='Default' and row['collectible']=='False',(key,row)
                assert 'Version=1.81.1.0' in row['identity'] and plan['previewVersion'] in row['informationalVersion']
                seen[row['name']]=row['sha256']
    assert seen and set(seen)<=set(layout['replacedEngines']),key
    bindings[key]=dict(reports=[f.relative_to(P).as_posix() for f in reports],loadedEngines=seen)
(E/'BINDING-SUMMARY.json').write_text(json.dumps(bindings,indent=2)+'\n',encoding='utf-8')
# Original binary layouts remain reconstructible from sealed TA-55 archives plus
# the PV-Q08 package archive. Avoid duplicating hundreds of megabytes of binaries.
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','x',zipfile.ZIP_DEFLATED) as z:
    for log in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('preview-qualification-*.log')):
        receipt=log.with_suffix('.log.json');row=read(receipt)
        assert row['logSha256']==sha(log) and row['failure'] is None
        expected=1 if log.name in ['preview-qualification-api-inventory.log','preview-qualification-units-bosak.xpath.runtime.tests.log'] else 0
        assert row['exitCode']==expected,(log.name,row['exitCode'])
        z.write(log,log.name);z.write(receipt,receipt.name)
with zipfile.ZipFile(E/'TRX-RESULTS.zip','x',zipfile.ZIP_DEFLATED) as z:
    for file in sorted((E/'trx').rglob('*.trx')):z.write(file,file.relative_to(P).as_posix())
api_root=Path(plan['work'])/'api-root'
(E/'API-LAYOUT.json').write_text(json.dumps({f.relative_to(api_root).as_posix():sha(f) for f in sorted(api_root.rglob('*')) if f.is_file()},indent=2)+'\n')
print('Captured exact 26342 solution cases, four extra lanes, frozen gates/skip maps/API and loaded package bindings; original failures remain separate.')
