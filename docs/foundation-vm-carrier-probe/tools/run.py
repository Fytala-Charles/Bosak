"""Build/run isolated actual-VM hybrid cohorts; never edits production or sealed prior packets."""
from pathlib import Path
import hashlib,io,json,os,subprocess,zipfile
ROOT=Path(__file__).resolve().parents[3]
PACKET=Path(__file__).resolve().parents[1]
WORK=ROOT/'.guard-tmp/vm-carrier-probe';WORK.mkdir(parents=True,exist_ok=True)
prior=ROOT/'docs/foundation-carrier-experiments/evidence'
feed=ROOT/'docs/foundation-package-qualification/evidence/candidate-feed.zip'
dlls={}
with zipfile.ZipFile(feed) as z:
    for name in z.namelist():
        if not name.endswith('.nupkg'):continue
        with zipfile.ZipFile(io.BytesIO(z.read(name))) as pkg:
            for item in pkg.namelist():
                if item.startswith('lib/net10.0/') and item.endswith('.dll'):dlls[Path(item).name]=pkg.read(item)
assert len(dlls)==7
with zipfile.ZipFile(prior/'core-source.zip') as z:
    sources={str(Path(n).relative_to('src/Bosak.XPath.Core')).replace('\\','/'):z.read(n) for n in z.namelist() if n.endswith('.cs')}
    project=z.read('src/Bosak.XPath.Core/Bosak.XPath.Core.csproj').decode('utf-8-sig')
project=project.replace('<TargetFramework>','<AssemblyName>Bosak.XPath.Core</AssemblyName><AssemblyVersion>1.34.2.0</AssemblyVersion><TargetFramework>',1)
base=(prior/'overlay-XdmValue.cs.txt').read_text(encoding='utf-8')
inputs=[]
for mode in ['baseline','holder','selective']:
    w=WORK/mode;lib=w/'lib';lib.mkdir(parents=True,exist_ok=True)
    for name,data in dlls.items():(lib/name).write_bytes(data)
    if mode!='baseline':
        core=w/'Core';core.mkdir(exist_ok=True)
        text=base
        if mode=='selective':
            old='return new(value._kind,value._integer,value._double,new PrototypeBox(value.PrototypePayload,marker),value._schemaTypeName,value._userSchemaTypeName);'
            new='var stored = value._kind is XdmValueKind.Boolean or XdmValueKind.Integer or XdmValueKind.Double or XdmValueKind.Float ? marker : new PrototypeBox(value.PrototypePayload,marker);\n        return new(value._kind,value._integer,value._double,stored,value._schemaTypeName,value._userSchemaTypeName);'
            assert old in text;text=text.replace(old,new)
            old='public static object? PrototypeMarker(XdmValue value) => value._reference is PrototypeBox box ? box.Marker : null;'
            new='public static object? PrototypeMarker(XdmValue value) => value._kind is XdmValueKind.Boolean or XdmValueKind.Integer or XdmValueKind.Double or XdmValueKind.Float ? value._reference : value._reference is PrototypeBox box ? box.Marker : null;'
            assert old in text;text=text.replace(old,new)
        (PACKET/f'evidence/{mode}-XdmValue.cs.txt').write_text(text,encoding='utf-8')
        for name,data in sources.items():
            target=core/name;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(text.encode('utf-8') if name=='Xdm/XdmValue.cs' else data)
        (core/'Core.csproj').write_text(project,encoding='utf-8')
        (w/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
        build=subprocess.run(['dotnet','build',str(core/'Core.csproj'),'-c','Release','--configfile',str(w/'NuGet.Config'),'-p:NuGetAudit=false'],capture_output=True,text=True,timeout=120)
        (PACKET/f'evidence/{mode}-core-build.txt').write_text(build.stdout+build.stderr,encoding='utf-8')
        if build.returncode:raise RuntimeError(build.stdout+build.stderr)
        (lib/'Bosak.XPath.Core.dll').write_bytes((core/'bin/Release/net10.0/Bosak.XPath.Core.dll').read_bytes())
    runner=w/'Runner';runner.mkdir(exist_ok=True);(runner/'Program.cs').write_bytes((PACKET/'tools/Probe.cs.txt').read_bytes())
    refs=''.join(f'<Reference Include="{Path(n).stem}"><HintPath>../lib/{n}</HintPath><Private>true</Private></Reference>' for n in sorted(dlls))
    define='<DefineConstants>PATCHED</DefineConstants>' if mode!='baseline' else ''
    runner_project='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>'+define+'</PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>'
    (runner/'Probe.csproj').write_text(runner_project,encoding='utf-8')
    (w/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
    build=subprocess.run(['dotnet','build',str(runner/'Probe.csproj'),'-c','Release','--configfile',str(w/'NuGet.Config'),'-p:NuGetAudit=false'],capture_output=True,text=True,timeout=120)
    (PACKET/f'evidence/{mode}-runner-build.txt').write_text(build.stdout+build.stderr,encoding='utf-8')
    if build.returncode:raise RuntimeError(build.stdout+build.stderr)
    inputs.append(dict(mode=mode,runnerProject=runner_project,coreProject=project if mode!='baseline' else None,layout={n:hashlib.sha256((lib/n).read_bytes()).hexdigest() for n in sorted(dlls)}))
env=dict(os.environ,DOTNET_TieredCompilation='0',DOTNET_ReadyToRun='0')
for index,order in enumerate([['baseline','holder','selective'],['selective','holder','baseline']]):
    for mode in order:
        command=['dotnet',str(WORK/mode/'Runner/bin/Release/net10.0/Probe.dll'),mode]
        run=subprocess.run(command,capture_output=True,text=True,env=env,timeout=120)
        (PACKET/f'evidence/round-{index}-{mode}.stderr.txt').write_text(run.stderr,encoding='utf-8')
        if run.returncode:raise RuntimeError(run.stdout+run.stderr)
        json.loads(run.stdout)
        (PACKET/f'evidence/round-{index}-{mode}.json').write_text(run.stdout,encoding='utf-8')
(PACKET/'evidence/INPUTS.json').write_text(json.dumps(dict(date='2026-10-31',sourceRevision='c88eea4308c0e49a81e7bfe2f481319f2e4d208c',sourceArchiveSha256=hashlib.sha256((prior/'core-source.zip').read_bytes()).hexdigest(),priorSourceArchive='docs/foundation-carrier-experiments/evidence/core-source.zip',feedSha256=hashlib.sha256(feed.read_bytes()).hexdigest(),packageDlls={n:hashlib.sha256(data).hexdigest() for n,data in sorted(dlls.items())},variants=inputs,environmentOverrides={'DOTNET_TieredCompilation':'0','DOTNET_ReadyToRun':'0'},scope='Baseline exact seven-DLL package layout; holder/selective replace Core only with source-built diagnostic variant; unqualified hybrids',productionModified=False),indent=2)+'\n',encoding='utf-8')
print('Built baseline and two Core-only hybrid variants; six actual-VM process runs retained.')
