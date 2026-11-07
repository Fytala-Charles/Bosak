"""Compile a caught-failure probe entry point; preserve original source and output bytes."""
from pathlib import Path
import hashlib,json,re,shutil,subprocess,sys,xml.etree.ElementTree as ET

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=read(P/'PLAN.json');old=read(R/'docs/foundation-xmldb-source-candidate/PLAN.json')
source=Path(p['source']);work=Path(p['work']);original=source/'tests/Bosak.XmlDb.IntegrationTests/CrashProbe.cs'
text=original.read_text(encoding='utf-8-sig');signature='    private static int Main(string[] args)'
assert text.count(signature)==1 and 'Environment.FailFast' not in text
guard='''    private static int Main(string[] args)
    {
        try { return MainCore(args); }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static int MainCore(string[] args)'''
guarded=text.replace(signature,guard,1)
versions=re.findall(r'Charles Korthout \| (\d+)\.(\d+)\s*\|',text);major,minor=map(int,versions[-1])
history=f'//                      | Charles Korthout | {major}.{minor+1}   | 07-11-2026     | Isolated diagnostic entry catches managed failures; expected process kills unchanged. |\n'
position=guarded.index('// ===========================================================================================================================================================\n',guarded.index('Change History:'))
guarded=guarded[:position]+history+guarded[position:]
derived=work/'GuardedCrashProbe.cs';derived.write_text(guarded,encoding='utf-8')
overlay=work/'GuardedIntegration.targets';root=ET.Element('Project')
ET.SubElement(root,'Import',dict(Project=old['overlay']))
group=ET.SubElement(root,'ItemGroup',dict(Condition="'$(MSBuildProjectName)' == 'Bosak.XmlDb.IntegrationTests'"))
ET.SubElement(group,'Compile',dict(Remove='CrashProbe.cs'))
ET.SubElement(group,'Compile',dict(Include=str(derived),Link='CrashProbe.cs'))
ET.ElementTree(root).write(overlay,encoding='utf-8',xml_declaration=True)
project=source/'tests/Bosak.XmlDb.IntegrationTests/Bosak.XmlDb.IntegrationTests.csproj'
command=['dotnet','build',str(project),'-c','Release','--no-restore','-p:BuildProjectReferences=false',
    '-p:NuGetAudit=false','-p:CustomAfterMicrosoftCommonTargets='+str(overlay),'-p:MinVerVersionOverride='+old['minVerVersionOverride']]
binary=project.parent/'bin/Release/net10.0';saved=work/'integration-original';target=work/'integration-guarded'
try:
    subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',
        str(R/'docs/foundation-r2-closure/evidence/execution/xmldb-full-guard-build.log'),'--timeout-seconds','180','--',*command],cwd=R,check=True)
    shutil.copytree(binary,target)
finally:
    # Only restore files in the verified, owned output directory; no source or deletion.
    for name,h in p['originalIntegrationFiles'].items():
        assert sha(saved/name)==h
        shutil.copyfile(saved/name,binary/name)
assert sha(original)==old['sourceFiles'][original.relative_to(source).as_posix()]
assert all(sha(binary/name)==h for name,h in p['originalIntegrationFiles'].items())
files={f.relative_to(target).as_posix():sha(f) for f in sorted(target.rglob('*')) if f.is_file()}
assert files['Bosak.XmlDb.IntegrationTests.dll']!=p['originalIntegrationFiles']['Bosak.XmlDb.IntegrationTests.dll']
for name,h in files.items():
    if name not in ['Bosak.XmlDb.IntegrationTests.dll','Bosak.XmlDb.IntegrationTests.pdb','Bosak.XmlDb.IntegrationTests.xml']:
        assert h==p['originalIntegrationFiles'][name],name
receipt=dict(originalSourceSha256=sha(original),derivedSourceSha256=sha(derived),overlaySha256=sha(overlay),
    diagnosticGuardOnly=True,expectedKillBehaviorUnchanged=True,originalOutputsRestored=True,
    layout=dict(path=str(target),files=files),sourceChangeScope='Rename private dispatcher Main to MainCore; wrap entry in caught Exception -> stderr/exit1; append history row. No test assertions or production code changed.')
(E/'GUARDED-INTEGRATION.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8')
print('Guarded Integration diagnostic built; original source and binaries restored. Expected child-process termination remains unchanged.')
