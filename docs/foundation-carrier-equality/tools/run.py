"""Bounded frozen-caller comparison; production files and older receipts are never edited."""
from pathlib import Path
import hashlib,json,os,subprocess,zipfile,sys
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];W=ROOT/'.guard-tmp/carrier-equality';W.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda b:hashlib.sha256(b).hexdigest()
def run(command,log,timeout=120,env=None):
    with process_error_mode():r=subprocess.run(command,capture_output=True,text=True,timeout=timeout,env=env)
    (P/'evidence'/log).write_text(r.stdout+r.stderr,encoding='utf-8')
    if r.returncode:raise RuntimeError(log+': '+r.stdout[-5000:]+r.stderr[-2000:])
    return r.stdout
with zipfile.ZipFile(P/'evidence/product-dlls.zip') as z:dlls={n:z.read(n) for n in z.namelist() if n.endswith('.dll')}
with zipfile.ZipFile(P/'evidence/core-source.zip') as z:
    sources={str(Path(n).relative_to('src/Bosak.XPath.Core')).replace('\\','/'):z.read(n) for n in z.namelist() if n.endswith('.cs')}
    project=z.read('src/Bosak.XPath.Core/Bosak.XPath.Core.csproj').decode('utf-8-sig')
project=project.replace('<TargetFramework>','<Version>1.35.0</Version><AssemblyVersion>1.35.0.0</AssemblyVersion><AssemblyName>Bosak.XPath.Core</AssemblyName><GenerateDocumentationFile>true</GenerateDocumentationFile><TreatWarningsAsErrors>true</TreatWarningsAsErrors><TargetFramework>',1)
overlay=(ROOT/'docs/foundation-carrier-experiments/evidence/overlay-XdmValue.cs.txt').read_text(encoding='utf-8')
# The unchanged production XdmValue is the same source on which the prior overlay was generated.
assert sources['Xdm/XdmValue.cs'].decode('utf-8-sig')==(ROOT/'src/Bosak.XPath.Core/Xdm/XdmValue.cs').read_text(encoding='utf-8')
overlay=overlay.replace('    public static XdmValue PrototypeAttach','    /// <summary>Diagnostic atomic marker attachment, not an authority factory.</summary>\n    /// <param name="value">The plain atomic value.</param>\n    /// <param name="marker">The diagnostic marker.</param>\n    /// <returns>The marked diagnostic value.</returns>\n    public static XdmValue PrototypeAttach',1).replace('    public static object? PrototypeMarker','    /// <summary>Reads only a diagnostic marker.</summary>\n    /// <param name="value">The diagnostic value.</param>\n    /// <returns>The marker, if present.</returns>\n    public static object? PrototypeMarker',1)
overlay=overlay.replace('        ArgumentNullException.ThrowIfNull(marker);','        ArgumentNullException.ThrowIfNull(marker);\n        if(value._kind is < XdmValueKind.String or > XdmValueKind.Binary) throw new ArgumentException("Atomic values only",nameof(value));',1)
methods="""
    private bool PrototypeHasMarker => PrototypeMarker(this) is not null;
    private XdmValue PrototypePlain => new(_kind,_integer,_double,PrototypePayload,_schemaTypeName,_userSchemaTypeName);
    private bool PrototypeRawEquals(XdmValue other) => base.Equals(other);
    private int PrototypeRawHash() => base.GetHashCode();
    /// <inheritdoc/>
    public override bool Equals(object? other)
    {
        if(other is not XdmValue value) return false;
        if(!PrototypeHasMarker && !value.PrototypeHasMarker) return base.Equals(other);
        return PrototypePlain.PrototypeRawEquals(value.PrototypePlain);
    }
    /// <inheritdoc/>
    public override int GetHashCode() => PrototypeHasMarker ? PrototypePlain.PrototypeRawHash() : base.GetHashCode();
"""
variants=[]
for mode in ['product','source','holder','equal-holder','equal-selective']:
    folder=W/mode;lib=folder/'lib';lib.mkdir(parents=True,exist_ok=True)
    for n,b in dlls.items():(lib/n).write_bytes(b)
    if mode!='product':
        core=folder/'Core';core.mkdir(exist_ok=True);text=sources['Xdm/XdmValue.cs'].decode('utf-8-sig')
        if mode!='source':
            text=overlay
            if mode.startswith('equal-'):text=text.replace('    // Diagnostic marker only:',methods+'\n    // Diagnostic marker only:',1)
            if mode=='equal-selective':
                old='return new(value._kind,value._integer,value._double,new PrototypeBox(value.PrototypePayload,marker),value._schemaTypeName,value._userSchemaTypeName);'
                new='var payload = value._kind is XdmValueKind.Boolean or XdmValueKind.Integer or XdmValueKind.Double or XdmValueKind.Float ? marker : new PrototypeBox(value.PrototypePayload,marker);\n        return new(value._kind,value._integer,value._double,payload,value._schemaTypeName,value._userSchemaTypeName);'
                assert old in text;text=text.replace(old,new)
                text=text.replace('=> value._reference is PrototypeBox box ? box.Marker : null;','=> value._kind is XdmValueKind.Boolean or XdmValueKind.Integer or XdmValueKind.Double or XdmValueKind.Float ? value._reference : value._reference is PrototypeBox box ? box.Marker : null;',1)
                text=text.replace('=> _reference is PrototypeBox box ? box.Payload : _reference;','=> _kind is XdmValueKind.Boolean or XdmValueKind.Integer or XdmValueKind.Double or XdmValueKind.Float ? null : _reference is PrototypeBox box ? box.Payload : _reference;',1)
            text='// Generated isolated carrier qualification source; not a production API.\n'+text
        (P/f'evidence/{mode}-XdmValue.cs.txt').write_text(text,encoding='utf-8')
        for n,b in sources.items():
            target=core/n;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(text.encode('utf-8') if n=='Xdm/XdmValue.cs' else b)
        (core/'Core.csproj').write_text(project,encoding='utf-8');(folder/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
        run(['dotnet','build',str(core/'Core.csproj'),'-c','Release','--configfile',str(folder/'NuGet.Config'),'-p:NuGetAudit=false'],mode+'-build.txt')
        (lib/'Bosak.XPath.Core.dll').write_bytes((core/'bin/Release/net10.0/Bosak.XPath.Core.dll').read_bytes())
    variants.append(dict(mode=mode,layout={n:sha((lib/n).read_bytes()) for n in dlls}))
# Compile once against the existing product, then keep all caller bytes unchanged.
caller=W/'Frozen';caller.mkdir(exist_ok=True);(caller/'Program.cs').write_bytes((P/'tools/Probe.cs.txt').read_bytes())
refs=''.join('<Reference Include="'+Path(n).stem+'"><HintPath>../product/lib/'+n+'</HintPath><Private>true</Private></Reference>' for n in dlls)
callerProject='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>'
(caller/'Probe.csproj').write_text(callerProject,encoding='utf-8');(caller/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
run(['dotnet','build',str(caller/'Probe.csproj'),'-c','Release','--configfile',str(caller/'NuGet.Config'),'-p:NuGetAudit=false'],'caller-build.txt')
frozen={n:(caller/'bin/Release/net10.0'/n).read_bytes() for n in ['Probe.dll','Probe.deps.json','Probe.runtimeconfig.json']}
for mode in [v['mode'] for v in variants]:
    lib=W/mode/'lib'
    for n,b in frozen.items():(lib/n).write_bytes(b)
    # Qualify the separate original 20-case caller through the checked runner.
    legacy=W/mode/'Legacy';legacy.mkdir(exist_ok=True)
    with zipfile.ZipFile(ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary-runtime.zip') as z:
        for n in z.namelist():
            target=(legacy/n).resolve()
            if not target.is_relative_to(legacy.resolve()):raise ValueError('Unsafe archive path')
            if not n.endswith('/'):target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(z.read(n))
    for n in dlls:(legacy/n).write_bytes((lib/n).read_bytes())
    manifest=P/f'evidence/{mode}-qualification.json';manifest.write_text(json.dumps({'assemblies':[{'name':Path(n).stem,'sha256':sha((lib/n).read_bytes())} for n in dlls]},indent=2)+'\n',encoding='utf-8')
    report=P/f'evidence/{mode}-legacy.json'
    # Never overwrite an original receipt; a rerun needs a separate dossier/copy.
    run([sys.executable,'-B',str(ROOT/'tools/run-retained-foundation-caller.py'),'--layout',str(legacy),'--candidate-evidence',str(manifest),'--output',str(report)],mode+'-legacy-log.txt')
env=dict(os.environ,DOTNET_TieredCompilation='0',DOTNET_ReadyToRun='0')
for index,order in enumerate([[v['mode'] for v in variants],list(reversed([v['mode'] for v in variants]))]):
    for mode in order:
        text=run(['dotnet',str(W/mode/'lib/Probe.dll'),mode],f'round-{index}-{mode}.json',timeout=120,env=env);json.loads(text)
(P/'evidence/INPUTS.json').write_text(json.dumps({'sourceRevision':'8080b18ba69959f4fc33feebb6810914da3f817d','sourceSha256':sha((P/'evidence/core-source.zip').read_bytes()),'productSha256':sha((P/'evidence/product-dlls.zip').read_bytes()),'probeSha256':sha((P/'tools/Probe.cs.txt').read_bytes()),'frozenFiles':{n:sha(b) for n,b in frozen.items()},'coreProject':project,'callerProject':callerProject,'variants':variants,'samples':7,'warmups':3,'sizes':[1000,100000],'productionModified':False},indent=2)+'\n',encoding='utf-8')
print('Five cohorts, one unchanged frozen caller, ten bounded VM/equality/allocation runs and five checked 20-case replays retained.')
