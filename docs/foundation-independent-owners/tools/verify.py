"""Verify independent-owner receipts and their unchanged production cohort."""
from pathlib import Path
import hashlib,json,subprocess,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'INPUTS.json').read_text());a=json.loads((E/'probe-0.json').read_text())
assert a==json.loads((E/'probe-1.json').read_text())
assert i['engineChanges'] is False and i['productionVersion']=='1.39.0' and i['sourceRevision']=='f72db224'
assert i['checks']==a['checks']==len(a['rows'])==151 and all(x['passed'] for x in a['rows'])
independent=[x for x in a['rows'] if x['id'].startswith('independent:')]
assert len(independent)==i['independentChecks']==48 and len({x['id'] for x in a['rows']})==151
for host in ['XPath','XQuery','XSLT']:assert len([x for x in independent if x['id'].startswith('independent:'+host+':')])==16
assert sha(P/'tools/Probe.cs.txt')==i['sourceSha256']
assert sha(R/'docs/foundation-real-host-session/tools/TypedNode.cs.txt')==i['nodeSourceSha256']
assert sha(R/'docs/foundation-private-host-factories/tools/Probe.cs.txt')==i['baseProbeSha256']
for b in a['bindings']:assert b['sha256']==(i['probeAssemblySha256'] if b['name']=='Bosak.XPath.Runtime.Tests' else i['engines'][b['name']+'.dll']['sha256'])
base=R/'docs/foundation-private-host-factories'
s=json.loads((base/'evidence/SOURCE.json').read_text())
for name,digest in s['files'].items():assert sha(R/name)==digest,name
for name,v in i['engines'].items():assert sha(Path(v['path']))==v['sha256'],name
assert '0 Error(s)' in (E/'build.txt').read_text() and '0 Warning(s)' in (E/'build.txt').read_text()
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified 151 repeated checks, 48 independent-owner controls, unchanged production source and actual engine loads.')
