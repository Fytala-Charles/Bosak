"""Read-only checks for the numeric/host diagnostic checkpoint."""
from pathlib import Path
import hashlib,json
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
a=json.loads((E/'probe-0.json').read_text());b=json.loads((E/'probe-1.json').read_text())
assert a==b and a['fixtures']==14 and len(a['observations'])==42
assert len(a['controls'])==5 and all(x['passed'] for x in a['controls'])
assert all(x['exact'] for x in a['observations'] if x['withinProfile'])
for host in ['XPath','XQuery','XSLT']:
 rows={x['id']:x for x in a['observations'] if x['host']==host}
 assert len(rows)==14 and rows['integer-difference']['canonical']=='0'
 assert rows['integer-40-constructor']['error']=='FORG0001'
 assert rows['invalid-decimal']['error']=='FORG0001'
 assert rows['long-plus-one']['integer'] and rows['long-plus-one']['exact']
i=json.loads((E/'INPUTS.json').read_text())
assert i['enginesUnchanged'] and not i['sourceChanged']
for record in i['qualifiedEngineHashes'].values():assert sha(Path(record['path']))==record['sha256']
for name,digest in i['sourceFiles'].items():assert sha(ROOT/name)==digest
assert sha(P/'tools/Probe.cs.txt')==i['probeSha256']
engines={Path(v['path']).stem:v for v in i['qualifiedEngineHashes'].values()}
for binding in a['bindings']:
 assert binding['sha256']==engines[binding['name']]['sha256']
manifest=P/'MANIFEST.json'
if manifest.exists():
 for name,digest in json.loads(manifest.read_text()).items():assert sha(P/name)==digest,name
print('Verified repeat observations, five controls, profile assertions, loaded bindings and unchanged source/binary pins.')
