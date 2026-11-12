"""Read-only verification of hosted qualification and additive ruleset adoption."""
import hashlib,importlib.util,io,json,zipfile
from pathlib import Path
here=Path(__file__).resolve().parent
root=here.parents[1]
manifest=json.loads((here/'MANIFEST.json').read_text(encoding='utf-8'))
def check(data,expected):
    assert len(data)==expected['bytes'] and hashlib.sha256(data).hexdigest()==expected['sha256']
for name,expected in manifest['files'].items():check((here/name).read_bytes(),expected)
spec=importlib.util.spec_from_file_location('gate',root/'tools/xslt30_gate.py')
gate=importlib.util.module_from_spec(spec);spec.loader.exec_module(gate)
policy,_,inputs,skips=gate.baseline()
with zipfile.ZipFile(here/'EVIDENCE.zip') as archive:
    assert set(archive.namelist())==set(manifest['archiveEntries'])
    for name,expected in manifest['archiveEntries'].items():check(archive.read(name),expected)
    read=lambda name:json.loads(archive.read(name))
    before,after=read('RULESET-BEFORE.json'),read('RULESET-AFTER.json')
    for key in ['name','target','enforcement','conditions','bypass_actors']:assert before[key]==after[key]
    old={r['type']:r for r in before['rules']};new={r['type']:r for r in after['rules']}
    assert all(new[k]==v for k,v in old.items())
    assert set(new)==set(old)|{'required_status_checks'}
    params=new['required_status_checks']['parameters']
    required=[dict(context='Standalone XSLT 3.0 (pinned basic profile)',integration_id=15368)]
    assert params['required_status_checks']==required and not params['strict_required_status_checks_policy']
    assert after['enforcement']=='active'
    live=next(r for r in read('LIVE-MAIN-RULES.json') if r['type']=='required_status_checks')
    assert live['ruleset_id']==22255065 and live['parameters']['required_status_checks']==required
    pr=read('PREMERGE-477.json')
    assert len(pr['statusCheckRollup'])==6 and all(c['conclusion']=='SUCCESS' for c in pr['statusCheckRollup'])
    merge=read('MERGE-477.json')
    assert merge['state']=='MERGED' and merge['mergeCommit']['oid']=='b3e2839cfeb1c6e6e9add98b13d84ea7c0827b0c'
    raw=archive.read('HOSTED-PR477.zip')
    assert 'sha256:'+hashlib.sha256(raw).hexdigest()==read('HOSTED-ARTIFACT.json')['digest']
    workflow=read('HOSTED-WORKFLOW.json')
    assert workflow['conclusion']=='success' and workflow['head_sha']==pr['headRefOid']
    with zipfile.ZipFile(io.BytesIO(raw)) as hosted:
        load=lambda name:json.loads(hosted.read(name))
        result,run,plan=load('GATE.json'),load('RUN.json'),load('PLAN.json')
        assert result['success'] and run['success'] and result['counts']==policy['counts']
        assert not any(result['corpusDifferences'].values()) and result['sourceCorpusUnchanged'] and result['runtimeUnchanged']
        assert load('CORPUS-BEFORE.json')==inputs==load('CORPUS-AFTER.json')
        for filename in ['stdout.log','stderr.log']:
            assert hashlib.sha256(hosted.read(filename)).hexdigest()==run[filename+'Sha256']
        rows=gate.reconcile(plan,run,hosted.read('stdout.log').decode('utf-8-sig'),skips,policy)
        assert len(rows)==14601
        source=read('HOSTED-SOURCE-COMMIT.json');head=read('PR477-HEAD-COMMIT.json')
        assert source['sha']==run['sourceCommit'] and head['sha']==pr['headRefOid']
        assert source['tree']['sha']==head['tree']['sha']
print('Verified PR477 hosted qualification: 10,221 passed / 0 failed / 4,380 excluded, exact case replay and unchanged inputs.')
print('Verified required GitHub Actions check on main; all original rules, conditions and bypass actors preserved.')
