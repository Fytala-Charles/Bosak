"""Verify proposal inputs, current candidate templates and expected refusal evidence."""
from pathlib import Path
import copy,hashlib,json,re,xml.etree.ElementTree as ET
from preflight import check_current

P=Path(__file__).resolve().parents[1];R=P.parents[1]
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=read(P/'INPUTS.json')
assert i['sourceVersion']=='1.81.1' and i['requirementsWaived']==0
assert not i['ownerAccepted'] and not i['productionAdopted']
for name,h in i['files'].items():assert sha(R/name)==h,name
external=Path(i['schemaProject']['path']);assert sha(external)==i['schemaProject']['sha256']
refs=ET.parse(external).findall('.//PackageReference')
assert len(refs)==4 and all(r.attrib['Version']=='1.26.3' for r in refs)
pins_path=R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json'
api_path=R/'docs/foundation-captured-identity-production/evidence/public-api-complete.json'
pins=read(pins_path);refusals=read(P/'evidence/TEMPLATE-REFUSALS.json')
assert set(refusals)=={'Core','Schema','XmlDb','Braid'}
for owner in refusals:
    template=read(P/'returns'/(owner+'.template.json'))
    assert template['ownerRole']==owner and template['scenario'] is None
    assert template['candidate']==dict(sourceVersion='1.81.1',pinsSha256=sha(pins_path),apiSha256=sha(api_path),
        engines={name:row['sha256'].lower() for name,row in pins['engines'].items()})
    result=check_current(template);assert result==refusals[owner]
    assert not result['structureReady'] and not result['qualificationAccepted'] and not result['ownerAcceptanceVerified']
    assert result['errors'] and not any(e.startswith('candidate:') for e in result['errors'])
    stale=copy.deepcopy(template);stale['candidate']['sourceVersion']='1.81.0'
    assert 'candidate: source version mismatch' in check_current(stale)['errors']
    assert template['budget']['thresholds'] is None and not template['loadedEngines']
log=R/'docs/foundation-r2-closure/evidence/execution/provider-adoption-receipt-controls.log'
receipt=read(log.with_suffix('.log.json'))
assert receipt['exitCode']==0 and receipt['failure'] is None and receipt['logSha256']==sha(log)
assert 'Ran 18 tests' in log.read_text(encoding='utf-8-sig') and 'OK' in log.read_text(encoding='utf-8-sig')
assert sha(P/'evidence/receipt-controls.log')==sha(log)
assert sha(P/'evidence/receipt-controls.log.json')==sha(log.with_suffix('.log.json'))
for file in [P/'README.md',P/'SCHEMA-ADOPTION.md',R/'docs/CHECKPOINT-1.81.1.md']:
    source=re.sub(r'`[^`]*`','',file.read_text(encoding='utf-8'))
    for link in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',source):
        target=link.strip('<>').split('#')[0]
        if target and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',target):assert (file.parent/target).exists(),(file,target)
if (P/'MANIFEST.json').exists():
    files={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
    assert read(P/'MANIFEST.json')==files
print('PV-Q07 verified: adoption proposal, four exact-1.81.1 unfilled templates/refusals, stale-version rejection, 18 structural controls. Production adoption and owner acceptance remain open.')
