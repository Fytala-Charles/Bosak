"""Native-source integration against production private kernel and isolated carrier; no production edits."""
from pathlib import Path
import hashlib,json,subprocess,zipfile,os,sys,xml.etree.ElementTree as ET
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];W=ROOT/'.guard-tmp/private-transport';W.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda b:hashlib.sha256(b).hexdigest()
def run(command,log,timeout=120,env=None):
 with process_error_mode():r=subprocess.run(command,capture_output=True,text=True,timeout=timeout,env=env)
 (P/'evidence'/log).write_text(r.stdout+r.stderr,encoding='utf-8')
 if r.returncode:raise RuntimeError(log+': '+r.stdout[-4500:]+r.stderr[-2000:])
 return r.stdout
with zipfile.ZipFile(ROOT/'docs/foundation-carrier-equality/evidence/product-dlls.zip') as z:dlls={n:z.read(n) for n in z.namelist() if n.endswith('.dll')}
with zipfile.ZipFile(ROOT/'docs/foundation-carrier-equality/evidence/core-source.zip') as z:
 coreSources={str(Path(n).relative_to('src/Bosak.XPath.Core')).replace('\\','/'):z.read(n) for n in z.namelist() if n.endswith('.cs')}
 coreProject=z.read('src/Bosak.XPath.Core/Bosak.XPath.Core.csproj').decode('utf-8-sig')
coreProject=coreProject.replace('<TargetFramework>','<Version>1.35.0</Version><AssemblyVersion>1.35.0.0</AssemblyVersion><AssemblyName>Bosak.XPath.Core</AssemblyName><GenerateDocumentationFile>true</GenerateDocumentationFile><TreatWarningsAsErrors>true</TreatWarningsAsErrors><TargetFramework>',1)
variants=[]
for mode in ['control','carrier']:
 folder=W/mode;lib=folder/'lib';lib.mkdir(parents=True,exist_ok=True)
 for n,b in dlls.items():(lib/n).write_bytes(b)
 config=folder/'NuGet.Config';config.write_text('<configuration><packageSources><clear /></packageSources></configuration>',encoding='utf-8')
 if mode=='carrier':
  core=folder/'Core';core.mkdir(exist_ok=True)
  text=(P/'evidence/XdmValue.cs.txt').read_text(encoding='utf-8')
  for n,b in coreSources.items():
   target=core/n;target.parent.mkdir(parents=True,exist_ok=True)
   if n=='Xdm/XdmValue.cs':b=text.encode('utf-8')
   if n=='Xdm/SchemaEvidenceKernel.cs':b=(P/'evidence/SchemaEvidenceKernel.cs.txt').read_bytes()
   target.write_bytes(b)
  (core/'Core.csproj').write_text(coreProject,encoding='utf-8')
  run(['dotnet','build',str(core/'Core.csproj'),'-c','Release','--configfile',str(config),'-p:NuGetAudit=false'],'carrier-core-build.txt')
  (lib/'Bosak.XPath.Core.dll').write_bytes((core/'bin/Release/net10.0/Bosak.XPath.Core.dll').read_bytes())
 runtime=folder/'Runtime';runtime.mkdir(exist_ok=True)
 with zipfile.ZipFile(ROOT/'docs/foundation-native-kernel-carrier/evidence/runtime-source.zip') as z:
  for n in z.namelist():
   if not n.endswith('.cs'):continue
   raw=z.read(n)
   if n.endswith('/SchemaQuerySession.cs'):
    if mode=='carrier':raw=(P/'evidence/SchemaQuerySession.cs.txt').read_bytes()
    else:raw=(ROOT/'docs/foundation-native-kernel-carrier/evidence/SchemaQuerySession-test-accessors.cs.txt').read_bytes()
   target=runtime/Path(n).relative_to('src/Bosak.XPath.Runtime');target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(raw)
  project=ET.fromstring(z.read('src/Bosak.XPath.Runtime/Bosak.XPath.Runtime.csproj'))
 for group in project.findall('ItemGroup'):
  for ref in list(group):
   if ref.tag=='ProjectReference':group.remove(ref)
 props=project.find('PropertyGroup')
 for name,value in [('Version','1.35.0'),('AssemblyVersion','1.35.0.0'),('AssemblyName','Bosak.XPath.Runtime'),('TreatWarningsAsErrors','true')]:ET.SubElement(props,name).text=value
 group=ET.SubElement(project,'ItemGroup')
 for n in ['Bosak.XPath.Core.dll','Bosak.XPath.Compiler.dll','Bosak.XPath.Parser.dll','Bosak.XPath.Providers.dll']:
  ref=ET.SubElement(group,'Reference',{'Include':Path(n).stem});ET.SubElement(ref,'HintPath').text='../lib/'+n;ET.SubElement(ref,'Private').text='true'
 runtimeProject=ET.tostring(project,encoding='unicode');(runtime/'Runtime.csproj').write_text(runtimeProject,encoding='utf-8')
 run(['dotnet','build',str(runtime/'Runtime.csproj'),'-c','Release','--configfile',str(config),'-p:NuGetAudit=false'],mode+'-runtime-build.txt')
 (lib/'Bosak.XPath.Runtime.dll').write_bytes((runtime/'bin/Release/net10.0/Bosak.XPath.Runtime.dll').read_bytes())
 host=folder/'Host';host.mkdir(exist_ok=True);(host/'Program.cs').write_bytes((P/'tools/Probe.cs.txt' if mode=='carrier' else ROOT/'docs/foundation-native-kernel-carrier/tools/Probe.cs.txt').read_bytes())
 with zipfile.ZipFile(ROOT/'docs/foundation-native-adapter/evidence/native-source.zip') as z:
  for n in z.namelist():
   if n.endswith('.cs'):
    target=host/'Native'/Path(n).name;target.parent.mkdir(exist_ok=True);target.write_bytes(z.read(n))
 refs=''.join('<Reference Include="'+Path(n).stem+'"><HintPath>../lib/'+n+'</HintPath><Private>true</Private></Reference>' for n in dlls)
 define='<DefineConstants>CARRIER</DefineConstants>' if mode=='carrier' else ''
 hostProject='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><AssemblyName>Bosak.XPath.Runtime.Tests</AssemblyName><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors>'+define+'</PropertyGroup><ItemGroup>'+refs+'</ItemGroup></Project>'
 (host/'Host.csproj').write_text(hostProject,encoding='utf-8')
 run(['dotnet','build',str(host/'Host.csproj'),'-c','Release','--configfile',str(config),'-p:NuGetAudit=false'],mode+'-host-build.txt')
 variants.append({'mode':mode,'layout':{n:sha((lib/n).read_bytes()) for n in dlls},'runtimeProject':runtimeProject,'hostProject':hostProject})
 legacy=folder/'Legacy';legacy.mkdir(exist_ok=True)
 with zipfile.ZipFile(ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary-runtime.zip') as z:
  for n in z.namelist():
   target=(legacy/n).resolve()
   if not target.is_relative_to(legacy.resolve()):raise ValueError('Unsafe archive path')
   if not n.endswith('/'):target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(z.read(n))
 for n in dlls:(legacy/n).write_bytes((lib/n).read_bytes())
 qualification=P/f'evidence/{mode}-qualification.json';qualification.write_text(json.dumps({'assemblies':[{'name':Path(n).stem,'sha256':sha((lib/n).read_bytes())} for n in dlls]},indent=2)+'\n',encoding='utf-8')
 run([sys.executable,'-B',str(ROOT/'tools/run-retained-foundation-caller.py'),'--layout',str(legacy),'--candidate-evidence',str(qualification),'--output',str(P/f'evidence/{mode}-legacy.json')],mode+'-legacy-log.txt')
for index,order in enumerate([['control','carrier'],['carrier','control']]):
 for mode in order:
  output=run(['dotnet',str(W/mode/'Host/bin/Release/net10.0/Bosak.XPath.Runtime.Tests.dll'),mode],f'round-{index}-{mode}.json',timeout=60);json.loads(output)
inputs={'productionCoreRevision':'8080b18ba69959f4fc33feebb6810914da3f817d','runtimeArchiveSha256':sha((ROOT/'docs/foundation-native-kernel-carrier/evidence/runtime-source.zip').read_bytes()),'nativeArchiveSha256':sha((ROOT/'docs/foundation-native-adapter/evidence/native-source.zip').read_bytes()),'coreArchiveSha256':sha((ROOT/'docs/foundation-carrier-equality/evidence/core-source.zip').read_bytes()),'productArchiveSha256':sha((ROOT/'docs/foundation-carrier-equality/evidence/product-dlls.zip').read_bytes()),'parentCarrierSha256':sha((ROOT/'docs/foundation-carrier-field-equality/evidence/field-selective-XdmValue.cs.txt').read_bytes()),'probeSha256':sha((P/'tools/Probe.cs.txt').read_bytes()),'coreProject':coreProject,'friendHostIdentity':'Bosak.XPath.Runtime.Tests','candidatePrivateMethods':['SchemaQuerySession.ReadEvidence','Operation.ReadValue','Operation.AdmitConversionValue','Operation.ValidateAccess'],'variants':variants,'productionModified':False}
inputs['candidateSources']={n:sha((P/'evidence'/n).read_bytes()) for n in ['XdmValue.cs.txt','SchemaEvidenceKernel.cs.txt','SchemaQuerySession.cs.txt']}
inputs['controlProbeSha256']=sha((ROOT/'docs/foundation-native-kernel-carrier/tools/Probe.cs.txt').read_bytes())
inputs['controlRuntimeSha256']=sha((ROOT/'docs/foundation-native-kernel-carrier/evidence/SchemaQuerySession-test-accessors.cs.txt').read_bytes())
inputs['checkedRunnerSha256']=sha((ROOT/'tools/run-retained-foundation-caller.py').read_bytes())
(P/'evidence/INPUTS.json').write_text(json.dumps(inputs,indent=2)+'\n',encoding='utf-8')
print('Direct private transport: four bounded runs and two checked legacy replays retained.')
