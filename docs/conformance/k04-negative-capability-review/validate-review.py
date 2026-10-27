from pathlib import Path
import json,hashlib,subprocess,collections
P=Path(__file__).resolve().parent;R=P.parents[2]
m=json.loads((P/'manifest.json').read_text())
for name,digest in m.items():assert hashlib.sha256((R/name).read_bytes()).hexdigest()==digest,name
inventory=json.loads((P/'inventory.json').read_text());assert inventory['scannedCases']=={'qt3':31821,'qt4':46985}
assert len(inventory['negativeDeclarations'])==75
for name,digest in inventory['fixtureHashes'].items():assert hashlib.sha256((R/name).read_bytes()).hexdigest()==digest,name
receipts=json.loads((P/'execution.json').read_text());assert len(receipts)==12
assert collections.Counter(r['result']['kind'] for r in receipts)=={'Passed':4,'Failed':8}
for r in receipts:
 assert r['exitCode']==0 and r['watchdogSeconds']==20
 raw=(P/(r['name']+'-output.txt')).read_text(encoding='utf-8-sig');assert json.loads(raw.splitlines()[-1])==r['result']
filters=[json.loads(line) for line in (P/'filters.jsonl').read_text().splitlines()];assert len(filters)==12
assert all(not r['xpathSupported'] and not r['xqueryDependenciesSupported'] for r in filters)
print('PASS: 78806 cases,75 negative declarations,12 original diagnostics (4 passed/8 failed),all normal admissions retained;identities verified.')
