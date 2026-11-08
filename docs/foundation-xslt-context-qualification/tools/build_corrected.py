"""Preserve exact candidate C# changes while restoring original project metadata."""
from pathlib import Path
import json,hashlib,xml.etree.ElementTree as ET,sys,subprocess
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=ROOT/'.guard-tmp/xslt-context-qualification';dest=W/'Corrected/src/Bosak.Xslt';dest.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
prior=json.loads((ROOT/'docs/foundation-xslt-explicit-context/evidence/INPUTS.json').read_text())
for name,digest in prior['sourceFiles'].items():
 original=ROOT/'src/Bosak.Xslt'/name;assert sha(original)==digest
 source=ROOT/'.guard-tmp/xslt-explicit-context/src/Bosak.Xslt'/name if name in prior['patchedFiles'] else original
 if name in prior['patchedFiles']:assert sha(source)==prior['patchedFiles'][name]
 target=dest/name;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(source.read_bytes())
original=ROOT/'src/Bosak.Xslt/Bosak.Xslt.csproj';tree=ET.parse(original);root=tree.getroot()
for group in root.findall('ItemGroup'):
 for ref in list(group.findall('ProjectReference')):group.remove(ref)
group=ET.SubElement(root,'ItemGroup')
for name,v in prior['qualifiedEngines'].items():
 if name=='Bosak.Xslt.dll':continue
 assert sha(Path(v['path']))==v['sha256']
 ref=ET.SubElement(group,'Reference',Include=Path(name).stem);ET.SubElement(ref,'HintPath').text=v['path']
props=ET.SubElement(root,'PropertyGroup')
for name,value in [('Version','1.36.0'),('GenerateDocumentationFile','true')]:ET.SubElement(props,name).text=value
assert {x.attrib['Include'] for x in root.findall('.//InternalsVisibleTo')}=={'Bosak.Xslt.Tests','Bosak.Xslt.Conformance'}
tree.write(dest/'Bosak.Xslt.csproj',encoding='unicode')
with process_error_mode(),(E/'corrected-build.txt').open('w',encoding='utf-8') as f:r=subprocess.run(['dotnet','build',str(dest/'Bosak.Xslt.csproj'),'-c','Release','--configfile',str(W/'NuGet.Config'),'-p:NuGetAudit=false'],cwd=ROOT,stdout=f,stderr=subprocess.STDOUT,timeout=180)
assert r.returncode==0
candidate=dest/'bin/Release/net10.0/Bosak.Xslt.dll'
(E/'CORRECTION.json').write_text(json.dumps({'originalCandidateSha256':prior['candidateSha256'],'correctedCandidatePath':str(candidate),'correctedCandidateSha256':sha(candidate),'originalProjectSha256':sha(original),'correctedProjectSha256':sha(dest/'Bosak.Xslt.csproj'),'restoredOriginalFriends':['Bosak.Xslt.Tests','Bosak.Xslt.Conformance'],'candidateCSharpUnchanged':{n:sha(dest/n) for n in prior['patchedFiles']},'newFriendGrants':False,'canonicalChanged':False},indent=2)+'\n')
print('Built corrected isolated candidate with original friend declarations; candidate C# changes unchanged.')
