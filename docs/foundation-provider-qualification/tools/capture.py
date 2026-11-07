"""Retain current Core qualification and completed readiness-tool receipts; no native acceptance."""
from pathlib import Path
import json,hashlib,zipfile
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';Q=R/'docs/foundation-public-primitives'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8'))
pins=read(E/'SOURCE-AND-BINARY-PINS.json');assert (E/'SOURCE-AND-BINARY-PINS.json').read_bytes()==(Q/'evidence/SOURCE-AND-BINARY-PINS.json').read_bytes()
for n,h in pins['sources'].items():assert sha(R/n)==h,n
for row in pins['engines'].values():assert sha(Path(row['path']))==row['sha256']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','w',zipfile.ZIP_DEFLATED) as z:
 for receipt in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('provider-qualification-*.log.json')):
  log=receipt.with_suffix('');z.write(log,log.name);z.write(receipt,receipt.name)
print('Captured readiness controls only;Core1.81.0 unchanged;actual provider workloads0;budgets not accepted.')
