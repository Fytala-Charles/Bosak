"""Verify the localized decision request without manufacturing an owner disposition."""
from pathlib import Path
import json,hashlib
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
pins=json.loads((P/'SOURCE-PINS.json').read_text());assert pins['readOnlyEvidence'] and not pins['responseCreatedOnBehalfOfSchema']
assert pins['proposalManifestSha256']==sha(R/'docs/foundation-bcap-owner-review/PROPOSAL-MANIFEST.json')
for name,row in pins['files'].items():assert sha(P/name)==row['sha256'],name
for id in ['BCAP-O1-Schema','BCAP-O2-Schema']:
 data=json.loads((P/'inputs/prior-returns'/(id+'.json')).read_text());assert data['decision']=='Revise'
if (P/'MANIFEST.json').exists():
 for name,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/name)==h,name
print('Verified self-contained exact-source Schema request; both owner decisions remain Revise, no answer invented.')
