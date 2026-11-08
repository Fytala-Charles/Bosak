"""Retain current Core qualification and exact native execution receipts without rebuilding."""
from pathlib import Path
import json,hashlib,zipfile
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
pins=json.loads((E/'SOURCE-AND-BINARY-PINS.json').read_text(encoding='utf-8'))
for n,h in pins['sources'].items():assert sha(R/n)==h,n
for row in pins['engines'].values():assert sha(Path(row['path']))==row['sha256']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','w',zipfile.ZIP_DEFLATED) as z:
 for receipt in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('native-schema-lane-*.log.json')):
  log=receipt.with_suffix('');z.write(log,log.name);z.write(receipt,receipt.name)
 for group in ['baseline','source','binary']:
  for f in sorted((E/(group+'-trx')).glob('*.trx')):z.write(f,f.relative_to(P).as_posix())
print('Captured native451 baseline/source/unchanged-binary receipts;Core1.81.0 unchanged;counts overlap;no public-provider/budget acceptance.')
