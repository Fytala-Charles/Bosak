"""Validate portable evidence; --staged additionally checks exact Git index bytes."""
from pathlib import Path
import hashlib,json,subprocess,sys,zipfile,xml.etree.ElementTree as ET
e=Path(__file__).resolve().parent;r=e.parents[3]
def sha(b):return hashlib.sha256(b).hexdigest()
m=json.loads((e/'manifest.json').read_text(encoding='utf-8'))
for name,item in m['files'].items():
 p=e/name;assert sha(p.read_bytes())==item['sha256'],name
 if '--staged' in sys.argv:
  data=subprocess.check_output(['git','show',':'+p.relative_to(r).as_posix()],cwd=r)
  assert sha(data)==item['sha256'],name+' index bytes'
proof=json.loads((r/'docs/xpath40-catalog/ROUNDING-PRECISION-BOUNDARIES.json').read_text())
assert sha((e/'manifest.json').read_bytes())==proof['evidenceManifestSha256']
for name,item in proof['sources'].items():
 assert subprocess.check_output(['git','hash-object',name],cwd=r).decode().strip()==item['gitBlob'],name
 if '--local-byte-identities' in sys.argv:assert sha((r/name).read_bytes())==item['sha256'],name
for label,item in json.loads((e/'focused-test-receipts.json').read_text()).items():
 assert ET.parse(e/item['path']).find('.//{*}Counters').attrib=={k:str(v) for k,v in item['counters'].items()},label
with zipfile.ZipFile(e/'full-test-receipts.zip') as z:
 for item in json.loads((e/'full-test-receipts.json').read_text()):
  data=z.read(item['path']);assert sha(data)==item['sha256']
  assert ET.fromstring(data).find('.//{*}Counters').attrib=={k:str(v) for k,v in item['counters'].items()}
subprocess.run([sys.executable,str(e/'analyze.py')],check=True,cwd=r)
assert json.loads((e/'diagnostic-summary.json').read_text())==proof['diagnostics']
before=[json.loads(x) for x in (e/'retained-kernel-examples-before.jsonl').read_text().splitlines()]
after=[json.loads(x) for x in (e/'retained-kernel-examples-after.jsonl').read_text().splitlines()]
assert len(before)==len(after)==21
assert before==after
for name,item in m['files'].items():assert sha((e/name).read_bytes())==item['sha256'],name
print('Evidence/source/index hashes, raw test counters and independent diagnostic expectations verified.')
