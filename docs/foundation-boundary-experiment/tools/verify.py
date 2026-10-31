from pathlib import Path
import hashlib,json
P=Path(__file__).resolve().parents[1]
sha=lambda b:hashlib.sha256(b).hexdigest()
manifest=json.loads((P/'MANIFEST.json').read_text(encoding='utf-8'))
for name,digest in manifest.items():assert sha((P/name).read_bytes())==digest,name
r=json.loads((P/'evidence/run.json').read_text(encoding='utf-8'));i=json.loads((P/'evidence/INPUTS.json').read_text(encoding='utf-8'))
assert r['checkCount']==len(r['checks'])==75 and all(x['passed'] for x in r['checks'])
assert len({x['id'] for x in r['checks']})==75
assert sum(x['id'].startswith('B') for x in r['checks'])==21
assert len(r['surface'])==34
assert r['coreSha256']==i['coreSha256']=='bcd14bcf8f11b23a48fd7f4a0db9786dc75100748fed67e488b5eec60221c2fa'
assert sha((P/'CORE-CANDIDATE-V5.cs.txt').read_bytes())==i['draftSha256']
assert (P/'evidence/run.stderr.txt').read_bytes()==b''
print('PASS: sealed V5 inputs, 34 draft types, 54 prior + 21 boundary controls; no production acceptance inferred.')
