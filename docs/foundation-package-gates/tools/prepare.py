"""Stage solution test/gate binaries with the exact seven package DLLs in isolated runners."""
import argparse
import hashlib
import io
import json
from pathlib import Path
import re
import shutil
import subprocess
import zipfile

HERE=Path(__file__).resolve().parents[1]
ROOT=HERE.parents[1]
WORK=ROOT/'.guard-tmp/foundation-package-gates-2026-10-30'


def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    global WORK
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--workspace',default=str(WORK));args=parser.parse_args()
    WORK=Path(args.workspace).resolve();assert WORK.is_relative_to((ROOT/'.guard-tmp').resolve())
    assert not WORK.exists() or not any(WORK.iterdir()), 'Use a fresh staging directory'
    WORK.mkdir(parents=True,exist_ok=True)
    packages=json.loads((ROOT/'docs/foundation-package-qualification/evidence/packages.json').read_bytes())
    qualified={a['assembly']:a for a in json.loads((ROOT/'docs/streaming-completion-cleanup/evidence/api-comparison.json').read_bytes())['assemblies']}
    dlls={}
    with zipfile.ZipFile(ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip') as feed:
        for package in packages['packages']:
            raw=feed.read(package['filename']);assert hashlib.sha256(raw).hexdigest()==package['sha256']
            with zipfile.ZipFile(io.BytesIO(raw)) as z:
                dlls[package['id']]=z.read('lib/net10.0/'+package['id']+'.dll')
                assert hashlib.sha256(dlls[package['id']]).hexdigest()==package['assemblySha256']
    projects=re.findall(r'^Project\([^\n]+?= "([^"]+)", "([^"]+\.csproj)"', (ROOT/'Bosak.sln').read_text(encoding='utf-8-sig'),re.M)
    selected=[(name,ROOT/Path(path.replace('\\','/'))) for name,path in projects if name.endswith('.Tests')]
    assert len(selected)==10
    selected += [(name,ROOT/'tests'/name/(name+'.csproj')) for name in ['Bosak.XPath.Conformance','Bosak.XPath.Conformance40']]
    runners=[]
    for name,project in selected:
        source=project.parent/'bin/Release/net10.0';target=WORK/'runners'/name
        assert (source/(name+'.dll')).exists(), name
        if name=='Bosak.XPath.Runtime.Tests':
            target=target/'bin/Release/net10.0'
        shutil.copytree(source,target)
        replacements=[]
        for package,raw in dlls.items():
            path=target/(package+'.dll')
            if path.exists():
                old=sha(path);path.write_bytes(raw)
                replacements.append({'assembly':package,'originalSha256':old,'packageSha256':sha(path)})
        extras=[]
        for path in target.glob('Bosak*.dll'):
            if path.stem in qualified and path.stem not in dlls:
                assert sha(path)==qualified[path.stem]['candidateSha256'].lower(), path
                extras.append({'assembly':path.stem,'sha256':sha(path),'scope':'Original qualified source-built product; not package-derived'})
        bindings=WORK/'bindings'/name;bindings.mkdir(parents=True)
        runners.append({'name':name,'kind':'unit' if name.endswith('.Tests') else 'gate','directory':str(target),'entryAssembly':name+'.dll','entrySha256':sha(target/(name+'.dll')),'project':project.relative_to(ROOT).as_posix(),'projectSha256':sha(project),'replacements':replacements,'otherProductAssemblies':extras,'bindingDirectory':str(bindings)})
    # Runtime tests use a fixed four-parent relative corpus path. The alias is owned by this fresh cohort.
    link=WORK/'runners/qt3tests'; fixture=(ROOT/'tests/qt3tests').resolve()
    assert fixture.is_relative_to((ROOT/'tests').resolve()) and not link.exists()
    subprocess.run(['cmd','/c','mklink','/J',str(link),str(fixture)],check=True,stdout=subprocess.PIPE)
    with (WORK/'observer-build.txt').open('wb') as output:
        subprocess.run(['dotnet','build',str(HERE/'tools/BindingHook.csproj'),'-c','Release','--nologo'],cwd=ROOT,stdout=output,stderr=subprocess.STDOUT,check=True)
    hook=HERE/'tools/bin/Release/net10.0/PackageBindingHook.dll';assert hook.exists()
    shutil.copyfile(hook,WORK/'PackageBindingHook.dll')
    shutil.copyfile(HERE/'tools/StartupHook.cs',WORK/'observer-source.cs')
    (WORK/'runners.json').write_bytes((json.dumps({'packageCohortSha256':sha(ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip'),'hookSha256':sha(hook),'hookPath':str(WORK/'PackageBindingHook.dll'),'runners':runners},indent=2)+'\n').encode())
    print('Staged ten unit-test and two gate runners; original bin directories unchanged.')


if __name__=='__main__': main()
