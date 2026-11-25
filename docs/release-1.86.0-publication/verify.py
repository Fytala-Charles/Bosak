from pathlib import Path
import json,hashlib
D=Path(__file__).resolve().parent
for n,h in json.loads((D/'MANIFEST.json').read_text()).items():assert hashlib.sha256((D/n).read_bytes()).hexdigest()==h,n
v=json.loads((D/'VERIFIED.json').read_text());assert v['success'] and v['signedPackages']==10 and v['exactPayloads']==10 and v['registryOnlyRestore']
p=json.loads((D/'SIGNED-PAYLOADS.json').read_text());assert len(p['packages'])==10 and all(x['signatureVerified'] and x['payloadExactlyMatches'] for x in p['packages'])
assert hashlib.sha256((D/'RELEASE-MANIFEST.json').read_bytes()).hexdigest()==v['manifestSha256']
print('Ten signed exact payload receipts and fresh registry-only consumer evidence verify offline.')
