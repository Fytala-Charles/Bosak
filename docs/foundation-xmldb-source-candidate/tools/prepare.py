"""Export immutable owner sources; stage offline dependencies without changing its checkout."""
from pathlib import Path
import hashlib,io,json,shutil,subprocess,tarfile,xml.etree.ElementTree as ET

P=Path(__file__).resolve().parents[1];R=P.parents[1];owner=Path('D:/Development/Bosal.XmlDb')
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
git=lambda *a:subprocess.check_output(['git','--no-optional-locks','-C',str(owner),*a],text=True).strip()
assert git('status','--porcelain')=='';head=git('rev-parse','HEAD')
work=R/'.guard-tmp/xmldb-source-candidate';assert not work.exists();source=work/'source';source.mkdir(parents=True)
paths=['src','tests','samples','Directory.Build.props','.editorconfig','global.json','LICENSE.md','Bosak.XmlDb.slnx']
data=subprocess.check_output(['git','-C',str(owner),'archive','--format=tar',head,'--',*paths])
with tarfile.open(fileobj=io.BytesIO(data)) as archive:archive.extractall(source,filter='data')
files={f.relative_to(source).as_posix():sha(f) for f in sorted(source.rglob('*')) if f.is_file()}
feed=work/'external-feed';feed.mkdir();cache=work/'cache';cache.mkdir()
deps={}
for file in source.rglob('packages.lock.json'):
    for target in read(file)['dependencies'].values():
        for name,row in target.items():
            if row.get('type','').lower()=='project' or name.startswith('Bosak.'):continue
            version=row['resolved'];key=(name.lower(),version)
            if key in deps:continue
            path=Path('C:/Users/charl/.nuget/packages')/name.lower()/version/(name.lower()+'.'+version+'.nupkg')
            assert path.is_file(),(name,version)
            shutil.copyfile(path,feed/path.name);deps[key]=dict(id=name,version=version,sha256=sha(path))
package_plan=read(R/'docs/foundation-preview-packages/PLAN.json');version=package_plan['previewVersion']
config=work/'NuGet.Config';config.write_text(f'<configuration><packageSources><clear/><add key="preview" value="{package_plan["feed"]}"/><add key="offline-tools" value="{feed}"/></packageSources><fallbackPackageFolders><clear/></fallbackPackageFolders></configuration>')
overlay=work/'CandidateDependencies.targets'
names=package_plan['selectedPackages']
overlay.write_text('<Project><ItemGroup><PackageReference Update="'+';'.join(names)+'" Version="['+version+']" /></ItemGroup></Project>')
observed=read(owner/'tests/Bosak.XmlDb.IntegrationTests/bin/Debug/net10.0/Bosak.XmlDb.IntegrationTests.deps.json')
identity=next(n.split('/',1)[1] for n,v in observed['libraries'].items() if n.startswith('Bosak.XmlDb.IntegrationTests/') and v['type']=='project')
(P/'evidence').mkdir(parents=True)
plan=dict(date='2026-11-07',ownerRoot=str(owner),ownerHead=head,ownerStatus='',source=str(source),work=str(work),
    archivedPaths=paths,archiveSha256=hashlib.sha256(data).hexdigest(),sourceFiles=files,previewVersion=version,
    config=str(config),configSha256=sha(config),overlay=str(overlay),overlaySha256=sha(overlay),cache=str(cache),
    externalDependencies=list(deps.values()),minVerVersionOverride=identity,
    versionScope='Replay observed integration-test MinVer identity in isolated diagnostic build only; no product version/pin/tag change or parent Core Git inference.',
    productionAdapterImplemented=False,sourceBodiesChanged=False,ownerAccepted=False,publicationAuthorized=False)
(P/'PLAN.json').write_text(json.dumps(plan,indent=2)+'\n',encoding='utf-8')
print('Exported clean XmlDb revision',head[:12],'with',len(files),'files and',len(deps),'offline external packages. MinVer diagnostic override:',identity)
