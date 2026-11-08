"""Read-only verification of adapter receipts, source pins and actual engine bindings."""
from pathlib import Path
import hashlib,json
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
a=json.loads((E/'run-0.json').read_text());assert a==json.loads((E/'run-1.json').read_text())
assert a['checks']==35 and len(a['results'])==35 and all(r['passed'] for r in a['results'])
ids={r['id'] for r in a['results']};assert len(ids)==35
assert {'xslt-distinct-match-global','xslt-global-only-template','xslt-source-free','xslt-public-document-template-focus-A','xslt-public-element-template-focus-absent','xslt-match-without-explicit-global-refused','open-session-drain-completes-1','raw-work-after-close','drain-wait-cancel','callback-failure-releases'}<=ids
i=json.loads((E/'INPUTS.json').read_text());assert not i['engineChanges'] and not i['authenticatedIntegration'] and i['repeatIdentical']
engines={Path(v['path']).stem:v for v in i['qualifiedEngines'].values()}
for v in engines.values():assert sha(Path(v['path']))==v['sha256']
assert len(a['bindings'])>=8
for v in a['bindings']:assert v['sha256']==engines[v['name']]['sha256']
for name,digest in i['sources'].items():assert sha(ROOT/name)==digest
for name,digest in i['adapterSources'].items():assert sha(P/'tools'/name)==digest
assert sha(ROOT/'docs/foundation-c45-domains-hosts/tools/Probe.cs.txt')==i['typedNodeOriginSha256']
assert 'TransformCaptured' in a['bridge'] and 'IXdmNode' in a['bridge']
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified 35 repeated host/model checks, focus correction, exact source pins and qualified loaded bindings.')
