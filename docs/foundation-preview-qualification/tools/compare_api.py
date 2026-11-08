"""Compare current package metadata with the sealed production API inventory."""
from pathlib import Path
import hashlib,json
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
def compare():
    packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')
    pins=read(R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json')
    before=read(R/'docs/foundation-captured-identity-production/evidence/public-api-complete.json');after=read(E/'public-api.json')
    for old,new in zip(before['assemblies'],after['assemblies'],strict=True):
        for key in ['project','assembly','identity','references','friendAssemblies','publicTypes','declarationCount','declarations']:
            assert old[key]==new[key],(old['assembly'],key)
        name=new['assembly'];expected=packages['loadedEngines'].get(name,pins['engines'][name]['sha256'])
        assert new['sha256'].lower()==expected,name
    assert len(after['assemblies'])==11 and sum(r['publicTypes'] for r in after['assemblies'])==318 and sum(r['declarationCount'] for r in after['assemblies'])==6465
    return dict(publicTypes=318,declarationRows=6465,products=11,packaged=9,
        unchangedNonpackaged=['Bosak.LanguageServer','Bosak.XPath.Providers.Database'],observedMetadataDelta=False,
        inventoryToolSha256=hashlib.sha256((R/'docs/foundation-xslt-source-membership/tools/Inventory.cs.txt').read_bytes()).hexdigest(),
        scope='Public/protected declarations, defaults, enum constants, attributes/value layouts, references/friends and identities; not behavioral or owner/API-freeze acceptance.')
if __name__=='__main__':
    result=compare();file=E/'API-COMPARISON.json'
    if file.exists():assert read(file)==result
    else:file.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    print('API unchanged: 318 types/6465 rows, identities/references/friends; nine package products and two pinned exclusions.')
