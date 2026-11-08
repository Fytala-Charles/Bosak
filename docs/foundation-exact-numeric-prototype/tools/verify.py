"""Verify sealed diagnostic receipts and current qualified binary identities."""
from pathlib import Path
import hashlib,json
P=Path(__file__).resolve().parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
a=json.loads((E/'run-0.json').read_text());b=json.loads((E/'run-1.json').read_text())
assert a==b and a['checks']==31 and a['carrierBytes']==a['productBytes']==48
i=json.loads((E/'INPUTS.json').read_text())
assert not i['engineChanges'] and not i['productionIntegration'] and i['repeatIdentical']
assert sha(P/'tools/Prototype.cs.txt')==i['prototypeSha256']
for v in i['qualifiedEngines'].values():assert sha(Path(v['path']))==v['sha256']
core=next(v for v in i['qualifiedEngines'].values() if Path(v['path']).stem=='Bosak.XPath.Core')
assert a['loadedCoreSha256']==core['sha256']
c=a['bytesPerOperation']
assert c['big-read']==c['big-read-evidence']==0
for prefix in ['inline-create','big-create','decimal-create']:assert c[prefix]==c[prefix+'-evidence']
if (P/'MANIFEST.json').exists():
 manifest=json.loads((P/'MANIFEST.json').read_text())
 assert set(manifest)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in manifest.items():assert sha(P/name)==digest,name
print('Verified 31 checks, repeat costs, loaded Core binding, qualified engine pins and sealed files.')
