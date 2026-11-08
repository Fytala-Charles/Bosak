"""Verify package assets/dependencies and fresh-cache/output/loaded-byte provenance."""
from pathlib import Path,PurePosixPath
import base64,hashlib,json,sys,xml.etree.ElementTree as ET,zipfile

P=Path(__file__).resolve().parents[1];R=P.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def inspect():
    p=json.loads((P/'PLAN.json').read_text());w=Path(p['work']);source=Path(p['source'])
    preflight=json.loads((R/'docs/foundation-captured-identity-production/evidence/PACKAGE-PREFLIGHT.json').read_text())
    version=p['previewVersion'];rows=[]
    feed=Path(p['feed']);assert len(list(feed.glob('*.nupkg')))==10
    output=w/'consumer/bin/Release/net10.0';expected={}
    for name in p['selectedPackages']:
        package=feed/f'{name}.{version}.nupkg';contents={}
        with zipfile.ZipFile(package) as z:
            names=z.namelist();assert len(names)==len(set(names))
            assert all(not n.startswith(('/','\\')) and '..' not in PurePosixPath(n).parts and ':' not in n and '\\' not in n for n in names)
            root=ET.fromstring(z.read(name+'.nuspec'))
            element=lambda suffix:next(e for e in root.iter() if e.tag.split('}')[-1]==suffix)
            assert element('id').text==name and element('version').text==version
            assert element('repository').attrib['commit']==p['candidateCommit']
            dependencies={e.attrib['id']:e.attrib['version'] for e in root.iter() if e.tag.endswith('}dependency')}
            project=ET.parse(source/'src'/name/(name+'.csproj'))
            wanted={Path(e.attrib['Include'].replace('\\','/')).stem for e in project.findall('.//ProjectReference')}
            assert set(dependencies)==wanted and all(v==version for v in dependencies.values())
            assert wanted<=set(p['selectedPackages'])
            assets={'NOTICE':'NOTICE'}
            for asset in preflight['projects'][name]['stockAssets']:
                path=asset['packagePath'].replace('\\','/')
                target=path.strip('/')
                if not target or path.endswith('/'):target=(target+'/' if target else '')+Path(asset['source']).name
                assets[target]=asset['source']
            for target,relative in assets.items():
                assert hashlib.sha256(z.read(target)).hexdigest()==sha(source/relative),(name,target)
                contents[target]=sha(source/relative)
            assert {'NOTICE','license.md','README.md','icon.png'}<=set(assets)
            if name=='Bosak.XPath.Standard':
                target='licenses/Unicode17-LICENSE.txt'
                assert hashlib.sha256(z.read(target)).hexdigest()==sha(source/'src/Bosak.XPath.Standard/Resources/Unicode17-LICENSE.txt')
                contents[target]=hashlib.sha256(z.read(target)).hexdigest()
            assert [n for n in names if n.endswith('.dll')]==[f'lib/net10.0/{name}.dll']
            for ext in ['dll','xml']:
                target=f'lib/net10.0/{name}.{ext}';h=hashlib.sha256(z.read(target)).hexdigest()
                assert h==sha(source/'src'/name/'bin/Release/net10.0'/(name+'.'+ext))
                assert h==sha(w/'consumer-cache'/name.lower()/version/'lib/net10.0'/(name+'.'+ext))
                if ext=='dll':assert h==sha(output/(name+'.'+ext))
                contents[target]=h
                if ext=='dll':expected[name]=h
        cache=w/'consumer-cache'/name.lower()/version
        assert sha(cache/f'{name.lower()}.{version}.nupkg')==sha(package)
        digest=base64.b64encode(hashlib.sha512(package.read_bytes()).digest()).decode()
        assert (cache/f'{name.lower()}.{version}.nupkg.sha512').read_text().strip()==digest
        rows.append(dict(id=name,version=version,path=str(package),sha256=sha(package),nugetSha512=digest,
            dependencies=dependencies,verifiedContents=contents))
    assets=json.loads((w/'consumer/obj/project.assets.json').read_text())
    assert set(assets['libraries'])=={n+'/'+version for n in p['selectedPackages']}
    assert {str(Path(n).resolve()) for n in assets['packageFolders']}=={str((w/'consumer-cache').resolve())}
    assert {str(Path(n).resolve()) for n in assets['project']['restore']['sources']}=={str(feed.resolve())}
    reports=list((P/'evidence/bindings').glob('*.json'));assert len(reports)==1
    bindings=json.loads(reports[0].read_text());assert not bindings['captureErrors'];seen={}
    for row in bindings['assemblies']:
        if row['name'] in expected:
            assert row['sha256']==expected[row['name']] and row['context']=='Default' and row['collectible']=='False'
            assert Path(row['path']).resolve()==(output/(row['name']+'.dll')).resolve()
            assert version in row['informationalVersion'] and p['candidateCommit'] in row['informationalVersion']
            seen[row['name']]=row['sha256']
    assert seen==expected
    return dict(previewVersion=version,candidateCommit=p['candidateCommit'],packages=rows,
        loadedEngines=seen,packageOnlyConsumer=True,freshPrivateCache=True,publicationAuthorized=False)
if __name__=='__main__':
    result=inspect();file=P/'evidence/PACKAGES.json'
    if sys.argv[1:]==['--capture']:file.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    else:assert json.loads(file.read_text())==result
    print('Verified 10 candidate packages: dependency cohort, license/NOTICE/README/icon/brand/Unicode/XML assets, cache/output/loaded hashes and local-only restore.')
