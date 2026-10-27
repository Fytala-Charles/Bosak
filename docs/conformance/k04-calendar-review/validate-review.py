from pathlib import Path
import json,hashlib,collections
P=Path(__file__).resolve().parent;R=P.parents[2]
for name,digest in json.loads((P/'manifest.json').read_text()).items():assert hashlib.sha256((R/name).read_bytes()).hexdigest()==digest,name
for name,digest in json.loads((P/'previous-identities.json').read_text()).items():assert hashlib.sha256((R/name).read_bytes()).hexdigest()==digest,name
for name,digest in json.loads((P/'fixture-hashes.json').read_text()).items():assert hashlib.sha256((R/name).read_bytes()).hexdigest()==digest,name
rows=json.loads((P/'execution.json').read_text());assert len(rows)==58
assert collections.Counter(r['result']['kind'] for r in rows)=={'Passed':30,'Failed':28}
for row in rows:
 assert row['exitCode']==0 and not row['timeout'] and row['watchdogSeconds']==20
 assert json.loads((P/(row['name']+'-output.txt')).read_text(encoding='utf-8-sig').splitlines()[-1])==row['result']
assert len(json.loads((P/'control-contract.json').read_text()))==27
print('PASS:58 bounded diagnostics,30 passes/28 preserved failures,fixture/evidence identities intact;no engine or admission change.')
