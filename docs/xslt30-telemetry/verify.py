"""Verify sealed observer evidence offline; never launches .NET."""
import hashlib, importlib.util, io, json, re, zipfile
from pathlib import Path
here=Path(__file__).resolve().parent
root=here.parents[1]
def check(data,expected):
    assert len(data)==expected['bytes'] and hashlib.sha256(data).hexdigest()==expected['sha256']
manifest=json.loads((here/'MANIFEST.json').read_text(encoding='utf-8'))
for name,expected in manifest['files'].items():check((here/name).read_bytes(),expected)
def load(name,path):
    spec=importlib.util.spec_from_file_location(name,path);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module
gate=load('gate',root/'tools/xslt30_gate.py')
summary=load('summary',here/'summarize.py')
policy,_,inputs,skips=gate.baseline()
with zipfile.ZipFile(here/'EVIDENCE.zip') as evidence:
    assert set(evidence.namelist())==set(manifest['archiveEntries'])
    for name,expected in manifest['archiveEntries'].items():check(evidence.read(name),expected)
    report=json.loads(evidence.read('REPORT.json'))
    for label in ['local','hosted']:
        artifact=evidence.read(label.upper()+'.zip')
        metadata=json.loads(evidence.read(label.upper()+'-META.json'))
        digest=hashlib.sha256(artifact).hexdigest()
        assert (metadata['sha256']==digest if label=='local' else metadata['artifact']['digest']=='sha256:'+digest)
        with zipfile.ZipFile(io.BytesIO(artifact)) as capture:
            assert len(capture.namelist())==len(set(capture.namelist())) and capture.testzip() is None
            read=capture.read
            observed=summary.summarize(read)
            assert observed==report[label]
            controls=summary.refusal_controls(read);assert len(controls)==5 and all(controls.values())
            run=json.loads(read('gate/RUN.json')); result=json.loads(read('gate/GATE.json'));plan=json.loads(read('gate/PLAN.json'))
            assert json.loads(read('gate/CORPUS-BEFORE.json'))==inputs==json.loads(read('gate/CORPUS-AFTER.json'))
            assert result['runtimeUnchanged'] and result['sourceCorpusUnchanged'] and not any(result['corpusDifferences'].values())
            for name in ['stdout.log','stderr.log']:
                assert hashlib.sha256(read('gate/'+name)).hexdigest()==run[name+'Sha256']
            if observed['gateSuccess']:
                rows=gate.reconcile(plan,run,read('gate/stdout.log').decode('utf-8-sig'),skips,policy)
                assert len(rows)==14601 and result['counts']==policy['counts']
            else:
                assert not run['success'] and not result['success']
    artifact=evidence.read('REQUIRED.zip')
    metadata=json.loads(evidence.read('REQUIRED-META.json'))
    assert metadata['artifact']['digest']=='sha256:'+hashlib.sha256(artifact).hexdigest()
    with zipfile.ZipFile(io.BytesIO(artifact)) as capture:
        read=capture.read
        run=json.loads(read('RUN.json'));result=json.loads(read('GATE.json'));plan=json.loads(read('PLAN.json'))
        assert run['success'] and result['success'] and result['counts']==policy['counts']
        assert json.loads(read('CORPUS-BEFORE.json'))==inputs==json.loads(read('CORPUS-AFTER.json'))
        assert result['sourceCorpusUnchanged'] and result['runtimeUnchanged'] and not any(result['corpusDifferences'].values())
        rows=gate.reconcile(plan,run,read('stdout.log').decode('utf-8-sig'),skips,policy);assert len(rows)==14601
    mapping=json.loads(evidence.read('SOURCE-TREE-MAP.json'))
    assert len(set(value['tree'] for value in mapping.values()))==1
    boundary=json.loads(evidence.read('SOURCE-BOUNDARY.json'))
    assert not any(name.startswith(('src/','tests/')) for name in boundary['changedFiles'])
    assert not set(boundary['protectedFiles'])&set(boundary['changedFiles'])
    for name,digest in boundary['protectedFiles'].items():
        assert hashlib.sha256(evidence.read('protected/'+name)).hexdigest()==digest
    with zipfile.ZipFile(io.BytesIO(evidence.read('HOSTED.zip'))) as h,zipfile.ZipFile(io.BytesIO(evidence.read('REQUIRED.zip'))) as r,zipfile.ZipFile(io.BytesIO(evidence.read('LOCAL.zip'))) as l:
        hh=json.loads(h.read('gate/RUNTIME-HASHES.json'));rh=json.loads(r.read('RUNTIME-HASHES.json'));lh=json.loads(l.read('gate/RUNTIME-HASHES.json'))
        assert report['comparison']['hostedDiagnosticMatchesRequiredApplication']==(hh==rh)
        assert report['comparison']['localMatchesHostedApplication']==(lh==hh)
        source=evidence.read('observer/tools/xslt30_telemetry.py')
        variants={hashlib.sha256(source).hexdigest(),hashlib.sha256(source.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n')).hexdigest()}
        for capture in [h,l]:assert json.loads(capture.read('OBSERVER.json'))['source']['sha256'] in variants
    assert not any(name.startswith('initial-refusal/gate/') for name in evidence.namelist())
    original=evidence.read('PR484-FAILURE.zip')
    metadata=json.loads(evidence.read('PR484-FAILURE.json'))
    assert metadata['artifact']['digest']=='sha256:'+hashlib.sha256(original).hexdigest()
    with zipfile.ZipFile(io.BytesIO(original)) as capture:
        run=json.loads(capture.read('RUN.json')); result=json.loads(capture.read('GATE.json'))
        assert run['status']=='TimedOut' and not run['success'] and not result['success']
        assert run['elapsedSeconds']==1200.156 and run['timeoutSeconds']==1200
    with zipfile.ZipFile(io.BytesIO(evidence.read('CI-LOGS.zip'))) as logs:
        for part,total in [('/7_Test.txt',26422),('Language-server tests.txt',90)]:
            name=next(n for n in logs.namelist() if part in n)
            text=logs.read(name).decode('utf-8-sig')
            assert not re.search(r'Failed:\s*[1-9]',text)
            assert sum(int(s.replace(',','')) for s in re.findall(r'Passed:\s*([0-9,]+)',text))==total
print('Sealed local/hosted telemetry, original outcomes, successful case ledgers/exclusions, ten corruption refusals and hosted solution/LSP totals verified; no .NET execution.')
