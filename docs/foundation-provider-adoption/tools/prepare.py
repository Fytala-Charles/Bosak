"""Refresh unfilled owner templates to the exact qualified candidate."""
from pathlib import Path
import copy,hashlib,json

P=Path(__file__).resolve().parents[1];R=P.parents[1]
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
pin_path=R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json'
api_path=R/'docs/foundation-captured-identity-production/evidence/public-api-complete.json'
pins=read(pin_path);assert pins['sourceVersion']=='1.81.1'
template=read(R/'docs/foundation-provider-qualification/OWNER-RUN.template.json')
template['candidate']=dict(sourceVersion='1.81.1',pinsSha256=sha(pin_path),apiSha256=sha(api_path),
    engines={name:row['sha256'].lower() for name,row in pins['engines'].items()})
(P/'returns').mkdir(exist_ok=True)
for owner in ['Core','Schema','XmlDb','Braid']:
    row=copy.deepcopy(template);row['ownerRole']=owner
    (P/'returns'/(owner+'.template.json')).write_text(json.dumps(row,indent=2)+'\n',encoding='utf-8')
inputs=[pin_path,api_path,R/'docs/foundation-owner-gap-review/MANIFEST.json',
    R/'docs/foundation-workload-1811/MANIFEST.json',
    R/'docs/foundation-provider-qualification/tools/receipt_preflight.py',
    R/'docs/foundation-provider-qualification/tools/test_receipt_preflight.py',
    R/'docs/foundation-provider-qualification/evidence/OWNER-RETURN-REVIEW.json']
inputs+=sorted((R/'docs/foundation-native-immutable-guard/provider').glob('*.cs'))
external=Path('D:/Development/Bosak.Schema.Public/src/Bosak.Schema/Bosak.Schema.csproj')
(P/'INPUTS.json').write_text(json.dumps(dict(date='2026-11-07',sourceVersion='1.81.1',
    candidateCommit='c0b926de34824ceb2010d5f814a4450b28b31dcc',
    files={p.relative_to(R).as_posix():sha(p) for p in inputs},
    schemaProject=dict(path=str(external),sha256=sha(external)),
    ownerAccepted=False,productionAdopted=False,requirementsWaived=0),indent=2)+'\n',encoding='utf-8')
print('Prepared four unfilled owner templates for exact 1.81.1 engine/API hashes; no acceptance.')
