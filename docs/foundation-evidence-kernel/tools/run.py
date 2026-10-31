"""Build/run isolated actual-VM hybrid cohorts; never edits production or sealed prior packets."""
from pathlib import Path
import hashlib,io,json,os,subprocess,zipfile
ROOT=Path(__file__).resolve().parents[3]
PACKET=Path(__file__).resolve().parents[1]
WORK=ROOT/'.guard-tmp/evidence-kernel';WORK.mkdir(parents=True,exist_ok=True)
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
for mode in ['baseline','holder','kernel']:
    w=WORK/mode;lib=w/'lib';lib.mkdir(parents=True,exist_ok=True)
    for name,data in dlls.items():(lib/name).write_bytes(data)
    if mode!='baseline':
        core=w/'Core';core.mkdir(exist_ok=True)
        text=base
        if mode=='kernel':
            key='    public static object? PrototypeMarker(XdmValue value)'
            at=text.index(key)
            text='// Generated isolated Core kernel diagnostic; not production adoption.\n'+text[:at]+'    /// <summary>Diagnostic-only post-cast composition hook; not a product API.</summary>\n    public static Func<XdmValue,string,XdmValue,XdmValue>? PrototypeAfterCast { get; set; }\n    /// <summary>Removes the diagnostic holder while preserving ordinary payload/type fields.</summary>\n    /// <param name="value">The diagnostic value.</param>\n    /// <returns>The ordinary payload without the holder.</returns>\n    public static XdmValue PrototypeStrip(XdmValue value) => new(value._kind,value._integer,value._double,value.PrototypePayload,value._schemaTypeName,value._userSchemaTypeName);\n'+text[at:]
        (PACKET/f'evidence/{mode}-XdmValue.cs.txt').write_text(text,encoding='utf-8')
        for name,data in sources.items():
            target=core/name;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(text.encode('utf-8') if name=='Xdm/XdmValue.cs' else data)
        (core/'Core.csproj').write_text(project,encoding='utf-8')
        (w/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
        build=subprocess.run(['dotnet','build',str(core/'Core.csproj'),'-c','Release','--configfile',str(w/'NuGet.Config'),'-p:NuGetAudit=false'],capture_output=True,text=True,timeout=120)
        (PACKET/f'evidence/{mode}-core-build.txt').write_text(build.stdout+build.stderr,encoding='utf-8')
        if build.returncode:raise RuntimeError(build.stdout+build.stderr)
        (lib/'Bosak.XPath.Core.dll').write_bytes((core/'bin/Release/net10.0/Bosak.XPath.Core.dll').read_bytes())
    runtimeProject=None
    if mode=='kernel':
        import xml.etree.ElementTree as ET
        runtime=w/'Runtime';runtime.mkdir(exist_ok=True)
        with zipfile.ZipFile(PACKET/'evidence/runtime-source.zip') as archive:
            for name in archive.namelist():
                if not name.endswith('.cs'):continue
                data=archive.read(name)
                if name.endswith('/Vm/VmEngine.cs'):
                    text=data.decode('utf-8-sig')
                    signature='    private static bool TryCast(XdmValue value, string typeName, EvaluationContext? context, out XdmValue result, out CastFailureKind failureKind)'
                    assert text.count(signature)==1
                    wrapper=signature+"\n    {\n        bool ok = TryCastKernelBody(value,typeName,context,out result,out failureKind);\n        if(ok && XdmValue.PrototypeAfterCast is { } hook) result=hook(value,typeName,result);\n        return ok;\n    }\n\n"
                    text=text.replace(signature,wrapper+signature.replace(' TryCast(', ' TryCastKernelBody('),1)
                    data=('// Generated isolated Runtime kernel diagnostic; not production adoption.\n'+text).encode('utf-8');(PACKET/'evidence/kernel-VmEngine.cs.txt').write_bytes(data)
                target=runtime/Path(name).relative_to('src/Bosak.XPath.Runtime');target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(data)
            tree=ET.fromstring(archive.read('src/Bosak.XPath.Runtime/Bosak.XPath.Runtime.csproj'))
        for group in tree.findall('ItemGroup'):
            for ref in list(group):
                if ref.tag=='ProjectReference':group.remove(ref)
        props=tree.find('PropertyGroup')
        ET.SubElement(props,'AssemblyName').text='Bosak.XPath.Runtime';ET.SubElement(props,'AssemblyVersion').text='1.34.2.0'
        group=ET.SubElement(tree,'ItemGroup')
        for n in ['Bosak.XPath.Core.dll','Bosak.XPath.Compiler.dll','Bosak.XPath.Parser.dll','Bosak.XPath.Providers.dll']:
            ref=ET.SubElement(group,'Reference',{'Include':Path(n).stem});ET.SubElement(ref,'HintPath').text='../lib/'+n;ET.SubElement(ref,'Private').text='true'
        runtimeProject=ET.tostring(tree,encoding='unicode');(runtime/'Runtime.csproj').write_text(runtimeProject,encoding='utf-8')
        build=subprocess.run(['dotnet','build',str(runtime/'Runtime.csproj'),'-c','Release','--configfile',str(w/'NuGet.Config'),'-p:NuGetAudit=false'],capture_output=True,text=True,timeout=120)
        (PACKET/'evidence/kernel-runtime-build.txt').write_text(build.stdout+build.stderr,encoding='utf-8')
        if build.returncode:raise RuntimeError(build.stdout+build.stderr)
        (lib/'Bosak.XPath.Runtime.dll').write_bytes((runtime/'bin/Release/net10.0/Bosak.XPath.Runtime.dll').read_bytes())
    runner=w/'Runner';runner.mkdir(exist_ok=True);(runner/'Program.cs').write_bytes((PACKET/'tools/Probe.cs.txt').read_bytes());(runner/'Kernel.cs').write_bytes((PACKET/'tools/Kernel.cs.txt').read_bytes())
    (runner/'Contracts.cs').write_bytes((ROOT/'docs/foundation-boundary-experiment/CORE-CANDIDATE-V5.cs.txt').read_bytes())
    with zipfile.ZipFile(ROOT/'docs/foundation-native-adapter/evidence/native-source.zip') as native:
        for item in native.namelist():
            if item.endswith('.cs'):
                target=runner/'Native'/Path(item).name;target.parent.mkdir(exist_ok=True);target.write_bytes(native.read(item))
    refs=''.join(f'<Reference Include="{Path(n).stem}"><HintPath>../lib/{n}</HintPath><Private>true</Private></Reference>' for n in sorted(dlls))
    define='<DefineConstants>PATCHED;GUARDED</DefineConstants>' if mode=='kernel' else '<DefineConstants>PATCHED</DefineConstants>' if mode=='holder' else ''
    runner_project='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors>'+define+'</PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>'
    (runner/'Probe.csproj').write_text(runner_project,encoding='utf-8')
    (w/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
    build=subprocess.run(['dotnet','build',str(runner/'Probe.csproj'),'-c','Release','--configfile',str(w/'NuGet.Config'),'-p:NuGetAudit=false'],capture_output=True,text=True,timeout=120)
    (PACKET/f'evidence/{mode}-runner-build.txt').write_text(build.stdout+build.stderr,encoding='utf-8')
    if build.returncode:raise RuntimeError(build.stdout+build.stderr)
    inputs.append(dict(mode=mode,runnerProject=runner_project,coreProject=project if mode!='baseline' else None,runtimeProject=runtimeProject,layout={n:hashlib.sha256((lib/n).read_bytes()).hexdigest() for n in sorted(dlls)}))
env=dict(os.environ,DOTNET_TieredCompilation='0',DOTNET_ReadyToRun='0')
for index,order in enumerate([['baseline','holder','kernel'],['kernel','holder','baseline']]):
    for mode in order:
        command=['dotnet',str(WORK/mode/'Runner/bin/Release/net10.0/Probe.dll'),mode]
        run=subprocess.run(command,capture_output=True,text=True,env=env,timeout=120)
        (PACKET/f'evidence/round-{index}-{mode}.stderr.txt').write_text(run.stderr,encoding='utf-8')
        if run.returncode:raise RuntimeError(run.stdout+run.stderr)
        json.loads(run.stdout)
        (PACKET/f'evidence/round-{index}-{mode}.json').write_text(run.stdout,encoding='utf-8')
(PACKET/'evidence/INPUTS.json').write_text(json.dumps(dict(date='2026-10-31',sourceRevision='c88eea4308c0e49a81e7bfe2f481319f2e4d208c',sourceArchiveSha256=hashlib.sha256((prior/'core-source.zip').read_bytes()).hexdigest(),priorSourceArchive='docs/foundation-carrier-experiments/evidence/core-source.zip',feedSha256=hashlib.sha256(feed.read_bytes()).hexdigest(),packageDlls={n:hashlib.sha256(data).hexdigest() for n,data in sorted(dlls.items())},variants=inputs,environmentOverrides={'DOTNET_TieredCompilation':'0','DOTNET_ReadyToRun':'0'},scope='Baseline exact seven-DLL package layout; holder replaces Core only; kernel replaces Core and Runtime with diagnostic hooks; unqualified hybrids',nativeSourceArchiveSha256=hashlib.sha256((ROOT/'docs/foundation-native-adapter/evidence/native-source.zip').read_bytes()).hexdigest(),draftPath='docs/foundation-boundary-experiment/CORE-CANDIDATE-V5.cs.txt',draftSha256=hashlib.sha256((ROOT/'docs/foundation-boundary-experiment/CORE-CANDIDATE-V5.cs.txt').read_bytes()).hexdigest(),probeSha256=hashlib.sha256((PACKET/'tools/Probe.cs.txt').read_bytes()).hexdigest(),nativeSourceRevision=json.loads((ROOT/'docs/foundation-native-adapter/evidence/SOURCE.json').read_text(encoding='utf-8'))['revision'],runtimeSourceSha256=hashlib.sha256((PACKET/'evidence/runtime-source.zip').read_bytes()).hexdigest(),kernelSourceSha256=hashlib.sha256((PACKET/'tools/Kernel.cs.txt').read_bytes()).hexdigest(),productionModified=False),indent=2)+'\n',encoding='utf-8')
print('Built baseline, holder and guarded Core/Runtime hybrid; six bounded process runs retained.')
