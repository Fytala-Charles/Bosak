"""Pin live XmlDb sources and compare an existing XDM binary suite under old/new engines."""
from pathlib import Path
import hashlib,json,os,shutil,subprocess,sys,xml.etree.ElementTree as ET,zipfile

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';owner=Path('D:/Development/Bosal.XmlDb')
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git','--no-optional-locks','-C',str(owner),*args],text=True).strip()
status=git('status','--porcelain');assert status=='';head=git('rev-parse','HEAD')
work=R/'.guard-tmp/xmldb-package-readiness';assert not work.exists();work.mkdir()
E.mkdir(parents=True,exist_ok=True)
packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')
origin=owner/'tests/Bosak.XmlDb.Xdm.Tests/bin/Debug/net10.0'
assert (origin/'Bosak.XmlDb.Xdm.Tests.dll').is_file()
source_files={f.relative_to(owner).as_posix():sha(f) for f in sorted((owner/'src').rglob('*'))
    if f.is_file() and f.suffix in ['.cs','.csproj'] and not {'bin','obj'}.intersection(f.relative_to(owner).parts)}
plan=dict(date='2026-11-07',ownerRoot=str(owner),ownerHead=head,ownerStatus=status,sourceHashes=source_files,
    previewVersion=packages['previewVersion'],packageManifestSha256=sha(R/'docs/foundation-preview-packages/MANIFEST.json'),
    sourceLane='PreparedNotRun',productionAdapterImplemented=False,ownerAccepted=False,
    apiFrozen=False,publicationAuthorized=False,formatsChanged=False,layouts={})
for lane in ['baseline','preview']:
    target=work/lane;shutil.copytree(origin,target)
    replacements={}
    if lane=='preview':
        for row in packages['packages']:
            name=row['id']
            if not (target/(name+'.dll')).exists():continue
            with zipfile.ZipFile(row['path']) as archive:
                (target/(name+'.dll')).write_bytes(archive.read('lib/net10.0/'+name+'.dll'))
            replacements[name]=sha(target/(name+'.dll'))
    files={f.relative_to(target).as_posix():sha(f) for f in sorted(target.rglob('*')) if f.is_file()}
    plan['layouts'][lane]=dict(path=str(target),files=files,replacedEngines=replacements)
(P/'PLAN.json').write_text(json.dumps(plan,indent=2)+'\n',encoding='utf-8')
cohort=read(R/'docs/foundation-package-workload/COHORT.json');hook=Path(cohort['hook']['path'])
assert sha(hook)==cohort['hook']['sha256']
for lane,layout in plan['layouts'].items():
    target=Path(layout['path']);assert layout['files']=={f.relative_to(target).as_posix():sha(f) for f in target.rglob('*') if f.is_file()}
    env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=str(hook);env['BOSAK_PACKAGE_BINDING_REPORT_DIRECTORY']=str(E/'bindings'/lane)
    subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',
        str(R/f'docs/foundation-r2-closure/evidence/execution/xmldb-package-{lane}.log'),'--timeout-seconds','180','--',
        'dotnet','vstest',str(target/'Bosak.XmlDb.Xdm.Tests.dll'),'--logger:trx','--ResultsDirectory:'+str(E/'trx'/lane)],
        cwd=R,env=env,check=True)
assert git('rev-parse','HEAD')==head and git('status','--porcelain')==status
print('Completed isolated existing-binary XDM baseline and package-overlay lanes; live owner checkout unchanged.')
