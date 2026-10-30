"""Verify pinned readiness inputs and prevent qualification from becoming inferred acceptance."""
import hashlib
import json
from pathlib import Path

HERE=Path(__file__).resolve().parents[1]
ROOT=HERE.parents[1]


def load(path):return json.loads(path.read_bytes())


def main():
    packet=load(HERE/'READINESS.json')
    assert len(packet['evidence'])==13
    for entry in packet['evidence']:
        assert hashlib.sha256((ROOT/entry['path']).read_bytes()).hexdigest()==entry['sha256'],entry['path']
        assert entry['repositoryRevision']==packet['snapshotRevision']
        assert '/blob/'+packet['snapshotRevision']+'/' in entry['immutableUrl']
    packages=load(ROOT/'docs/foundation-package-qualification/evidence/packages.json')
    gates=load(ROOT/'docs/foundation-package-gates/RESULTS.json')
    assert packet['sourceRevision']==packages['sourceRevision']
    assert packet['feed']['sha256']==packages['feedArchive']['sha256']==gates['packageFeedSha256']
    assert len(packet['feed']['packages'])==7
    for package in packet['feed']['packages']:
        original=next(p for p in packages['packages'] if p['id']==package['id'])
        assert package['sha256']==original['sha256'] and package['assemblySha256']==original['assemblySha256']
    assert packet['qualifiedScope']['fullSolutionTests']==gates['unitTests']
    assert packet['qualifiedScope']['otherSourceBuiltProducts']==['Bosak.Xslt','Bosak.XQuery','Bosak.XPath.Providers.Database']
    assert packet['qualifiedScope']['qt3']==[31397,0,424] and packet['qualifiedScope']['qt4']==[8941,0,1918]
    assert len(packet['remainingRequirements'])==8 and all(g['status']=='Pending' for g in packet['remainingRequirements'])
    for key,value in packet['decisions'].items():assert value is None or value is False,key
    template=load(HERE/'OWNER-RESPONSE.template.json')
    assert template['decision'] is None and template['decidedBy'] is None and template['requiredContractGaps'] is None
    assert template['freezeAccepted'] is False and template['productAdoptionAuthorized'] is False and template['publicationAuthorized'] is False
    manifest=HERE/'MANIFEST.json'
    if manifest.exists():
        for entry in load(manifest)['files']:assert hashlib.sha256((HERE/entry['path']).read_bytes()).hexdigest()==entry['sha256'],entry['path']
    print('Verified 13 immutable inputs, seven package identities and eight pending requirements; no owner acceptance inferred.')


if __name__=='__main__':main()
