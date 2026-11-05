"""Verify bounded integration observations and the unchanged actual production cohort."""
from pathlib import Path
import hashlib,json
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
a=json.loads((E/'run-0.json').read_text());assert a==json.loads((E/'run-1.json').read_text())
assert a['checks']==len(a['rows'])==75 and len({r['id'] for r in a['rows']})==75 and all(r['passed'] for r in a['rows'])
for host in ['XPath','XQuery','XSLT']:
 rows={r['id'].split(':',1)[1] for r in a['rows'] if r['id'].startswith(host+':')};assert len(rows)==25
 assert {'actual-host-and-captured-inputs','caller-preserved-success','caller-preserved-error','caller-preserved-async','foreign-environment','foreign-snapshot','deferred-after-callback-refused','close-rejects-existing-operation-access','raw-completion-releases-once','callback-and-cleanup-retained','mutable-decimal-format-refused','sequence-input-refused','structured-result-refused'}<=rows
assert len(a['fieldPolicy'])==69 and sum(f['policy']=='Copied bounded input' for f in a['fieldPolicy'])==9
assert sum(f['policy']=='Must match fresh-context default' for f in a['fieldPolicy'])==60
assert a['bytesPerOperation']=={'fixture-reflective-input-capture':33461,'fresh-evaluation-context':2920,'xquery-context-wrapper-copy':3744} and a['allocationIterations']==1000
i=json.loads((E/'INPUTS.json').read_text());assert not i['productionChanges'] and not i['newFriendGrants'] and not i['productionFactoriesComplete']
prior=json.loads((ROOT/'docs/foundation-xslt-context-promotion/evidence/INPUTS.json').read_text());assert i['qualifiedEngines']==prior['engines']
engines={Path(v['path']).stem:v for v in i['qualifiedEngines'].values()}
for v in engines.values():assert sha(Path(v['path']))==v['sha256']
for b in a['bindings']:assert b['sha256']==(i['fixtureAssemblySha256'] if b['name']=='Bosak.XPath.Runtime.Tests' else engines[b['name']]['sha256'])
assert {b['name'] for b in a['bindings']} >= {'Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime','Bosak.XQuery','Bosak.Xslt'}
for name,digest in i['sources'].items():assert sha(ROOT/name)==digest
for name,digest in i['fixtureSources'].items():assert sha(P/'tools'/name)==digest
assert sha(ROOT/'docs/foundation-c45-domains-hosts/tools/Probe.cs.txt')==i['typedNodeOriginSha256']
assert sha(ROOT/'.guard-tmp/real-host-session/bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll')==i['fixtureAssemblySha256']
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified 75 real-session host checks twice, 69-field bounded policy, repeated context costs and unchanged 1.37.0 source/binary pins.')
