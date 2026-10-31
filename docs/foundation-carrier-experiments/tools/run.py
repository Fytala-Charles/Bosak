"""Reproduce isolated experiments from immutable public Core source; never edits product files."""
from pathlib import Path
import hashlib,io,json,os,subprocess,zipfile
ROOT=Path(__file__).resolve().parents[3]
PACKET=Path(__file__).resolve().parents[1]
WORK=ROOT/'.guard-tmp/carrier-experiments'
REV='c88eea4308c0e49a81e7bfe2f481319f2e4d208c'
WORK.mkdir(parents=True,exist_ok=True)
archive=subprocess.check_output(['git','-C',str(ROOT),'archive','--format=zip',REV,'src/Bosak.XPath.Core'])
(PACKET/'evidence/core-source.zip').write_bytes(archive)
with zipfile.ZipFile(io.BytesIO(archive)) as z:
    sources={str(Path(n).relative_to('src/Bosak.XPath.Core')).replace('\\','/'):z.read(n) for n in z.namelist() if n.endswith('.cs')}
original=sources['Xdm/XdmValue.cs'].decode('utf-8-sig').replace('\r\n','\n')
helper='''
    // Diagnostic marker only: no schema authority or new normative XPath behavior.
    private sealed record PrototypeBox(object? Payload, object Marker);
    public static XdmValue PrototypeAttach(XdmValue value, object marker)
    {
        ArgumentNullException.ThrowIfNull(marker);
        return new(value._kind,value._integer,value._double,new PrototypeBox(value.PrototypePayload,marker),value._schemaTypeName,value._userSchemaTypeName);
    }
    public static object? PrototypeMarker(XdmValue value) => value._reference is PrototypeBox box ? box.Marker : null;
    private object? PrototypePayload => _reference is PrototypeBox box ? box.Payload : _reference;
'''
boundary='}\n\n/// <summary>\n/// Wraps an'
assert boundary in original
inputs=[]
for mode in ['baseline','naive','overlay','wrapper']:
    work=WORK/mode;core=work/'Core';runner=work/'Runner';core.mkdir(parents=True,exist_ok=True);runner.mkdir(exist_ok=True)
    for name,data in sources.items():
        target=core/name;target.parent.mkdir(parents=True,exist_ok=True)
        if name=='Xdm/XdmValue.cs' and mode in ('naive','overlay'):
            text=original
            if mode=='overlay':
                text=text.replace('_reference','PrototypePayload').replace('private readonly object? PrototypePayload;','private readonly object? _reference;').replace('PrototypePayload = reference;','_reference = reference;')
            text=text.replace(boundary,helper+'}\n\n/// <summary>\n/// Wraps an',1)
            if mode=='naive':
                # Constructor must read the raw original payload; unpatched accessors are the subtractive control.
                pass
            data=('// Generated isolated carrier experiment; product source unchanged.\n'+text).encode('utf-8')
            (PACKET/'evidence'/f'{mode}-XdmValue.cs.txt').write_bytes(data)
        target.write_bytes(data)
    (core/'Core.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AssemblyName>Bosak.XPath.Core</AssemblyName><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><AllowUnsafeBlocks>true</AllowUnsafeBlocks><LangVersion>preview</LangVersion></PropertyGroup></Project>',encoding='utf-8')
    (runner/'Program.cs').write_bytes((PACKET/'tools/Probe.cs.txt').read_bytes())
    define='<DefineConstants>PATCHED</DefineConstants>' if mode in ('overlay','naive') else ''
    (runner/'Probe.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>'+define+'</PropertyGroup><ItemGroup><ProjectReference Include="../Core/Core.csproj"/></ItemGroup></Project>',encoding='utf-8')
    (work/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
    cmd=['dotnet','build',str(runner/'Probe.csproj'),'-c','Release','--configfile',str(work/'NuGet.Config'),'-p:NuGetAudit=false']
    build=subprocess.run(cmd,capture_output=True,text=True)
    (PACKET/'evidence'/f'{mode}-build.txt').write_text(build.stdout+build.stderr,encoding='utf-8')
    if build.returncode: raise RuntimeError(build.stdout+build.stderr)
    inputs.append(dict(mode=mode,coreProject=(core/'Core.csproj').read_text(),runnerProject=(runner/'Probe.csproj').read_text(),xdmValueSourceSha256=hashlib.sha256((core/'Xdm/XdmValue.cs').read_bytes()).hexdigest()))
# Two balanced process orders, seven timed samples after three warmups per operation/size.
env=dict(os.environ,DOTNET_TieredCompilation='0',DOTNET_ReadyToRun='0')
for round_index,order in enumerate([['baseline','overlay','wrapper','naive'],['naive','wrapper','overlay','baseline']]):
    for mode in order:
        for size in ([1000] if mode=='naive' else [1000,100000]):
            cmd=['dotnet',str(WORK/mode/'Runner/bin/Release/net10.0/Probe.dll'),mode,str(size)]
            run=subprocess.run(cmd,env=env,capture_output=True,text=True,timeout=120)
            (PACKET/'evidence'/f'round-{round_index}-{mode}-{size}.stderr.txt').write_text(run.stderr,encoding='utf-8')
            if run.returncode: raise RuntimeError(run.stdout+run.stderr)
            json.loads(run.stdout)
            (PACKET/'evidence'/f'round-{round_index}-{mode}-{size}.json').write_text(run.stdout,encoding='utf-8')
(PACKET/'evidence/INPUTS.json').write_text(json.dumps(dict(date='2026-10-31',sourceRevision=REV,coreSourceArchiveSha256=hashlib.sha256(archive).hexdigest(),sourceFiles=len(sources),variants=inputs,environmentOverrides={'DOTNET_TieredCompilation':'0','DOTNET_ReadyToRun':'0'},productionModified=False,scope='Rebuilt cloned Core source only; no VM, real provider, performance budget or API acceptance'),indent=2)+'\n',encoding='utf-8')
print('Built four isolated cohorts and ran fourteen process/size observations; raw results retained.')
