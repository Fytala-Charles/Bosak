"""Verify portable floor/ceiling evidence; --staged also checks original index bytes."""
from pathlib import Path
import hashlib,json,subprocess,sys,zipfile,xml.etree.ElementTree as ET
e=Path(__file__).resolve().parent;r=e.parents[3]
def sha(b):return hashlib.sha256(b).hexdigest()
m=json.loads((e/'manifest.json').read_text(encoding='utf-8'))
for name,item in m['files'].items():
 p=e/name;assert sha(p.read_bytes())==item['sha256'],name
 if '--staged' in sys.argv:assert sha(subprocess.check_output(['git','show',':'+p.relative_to(r).as_posix()],cwd=r))==item['sha256'],name+' index bytes'
proof=json.loads((r/'docs/xpath40-catalog/FLOOR-CEILING-INTEGER-PRESERVATION.json').read_text(encoding='utf-8'))
assert sha((e/'manifest.json').read_bytes())==proof['evidenceManifestSha256']
for name,item in proof['sources'].items():
 assert subprocess.check_output(['git','hash-object',name],cwd=r).decode().strip()==item['gitBlob'],name
 if '--local-byte-identities' in sys.argv:assert sha((r/name).read_bytes())==item['localSha256'],name
with zipfile.ZipFile(e/'test-receipts.zip') as z:
 rows=json.loads((e/'test-receipts.json').read_text());assert len(rows)==13
 for row in rows:
  data=z.read(row['path']);assert sha(data)==row['sha256']
  assert ET.fromstring(data).find('.//{*}Counters').attrib=={k:str(v) for k,v in row['counters'].items()}
subprocess.run([sys.executable,str(e/'analyze.py')],cwd=r,check=True)
assert json.loads((e/'diagnostic-summary.json').read_text())==proof['diagnostics']
for name,item in m['files'].items():assert sha((e/name).read_bytes())==item['sha256'],name
print('Source/evidence/index hashes, raw test counters, floor/ceiling expectations verified.')
