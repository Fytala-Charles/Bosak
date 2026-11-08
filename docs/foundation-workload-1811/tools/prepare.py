"""Prepare a distinct observational cohort; never rebuild provider or production engines."""
from pathlib import Path
import hashlib, json, shutil, xml.etree.ElementTree as ET

P=Path(__file__).resolve().parents[1];R=P.parents[1]
old=R/'docs/foundation-owner-workload-protocol'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
pins=read(R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json')
refs=read(old/'evidence/REFERENCES.json')
assert pins['sourceVersion']=='1.81.1'
for row in pins['engines'].values():assert sha(Path(row['path']))==row['sha256']
for key in ['nativeAssembly','providerAssembly']:assert sha(Path(refs[key]['path']))==refs[key]['sha256']
(P/'workload').mkdir(parents=True,exist_ok=True)
(P/'evidence').mkdir(exist_ok=True)
shutil.copytree(old/'fixtures',P/'fixtures',dirs_exist_ok=True)
protocol=read(old/'PROTOCOL.json');protocol['sourceVersion']='1.81.1'
(P/'PROTOCOL.json').write_text(json.dumps(protocol,indent=2)+'\n',encoding='utf-8')
source=(old/'workload/Program.cs').read_text(encoding='utf-8')
marker='//                      | Charles Korthout | 0.1'
lines=source.splitlines();i=next(i for i,line in enumerate(lines) if line.startswith(marker))
lines.insert(i+1,'//                      | Charles Korthout | 0.2   | 07-11-2026     | Bind isolated measurement metadata to qualified 1.81.1; timed workload unchanged. |')
source='\n'.join(lines)+'\n'
assert source.count('sourceVersion = "1.81.0"')==1
source=source.replace('sourceVersion = "1.81.0"','sourceVersion = "1.81.1"')
(P/'workload/Program.cs').write_text(source,encoding='utf-8')
tree=ET.parse(old/'workload/Workload.csproj')
for reference in tree.findall('.//Reference'):
    name=reference.attrib['Include']
    if name in pins['engines']:path=pins['engines'][name]['path']
    else:path=refs['providerAssembly' if name=='Bosak.Schema.ExperimentalProvider' else 'nativeAssembly']['path']
    reference.find('HintPath').text=path
tree.write(P/'workload/Workload.csproj',encoding='utf-8',xml_declaration=True)
for file in ['compare.py','test_compare.py']:shutil.copyfile(old/'tools'/file,P/'tools'/file)
hook=R/'.guard-tmp/foundation-package-gates-2026-10-30/hook-v2/PackageBindingHook.dll'
adoption=dict(date='2026-11-07',recordedBeforeExecution=True,sourceVersion='1.81.1',
    candidateCommit='c0b926de34824ceb2010d5f814a4450b28b31dcc',
    scope='Observational cohort rebind only; identical timed C# workload and fixture/method fields. No engine/provider semantics, APIs or owner acceptance changed.',
    priorProtocolSha256=sha(old/'PROTOCOL.json'), priorProgramSha256=sha(old/'workload/Program.cs'),
    qualificationPinsSha256=sha(R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json'),
    references={name:dict(path=row['path'],sha256=row['sha256']) for name,row in pins['engines'].items() if name not in ['Bosak.LanguageServer','Bosak.XPath.Providers.Database']},
    nativeAssembly=refs['nativeAssembly'],providerAssembly=refs['providerAssembly'],
    hook=dict(path=str(hook),sha256=sha(hook)),ownerAccepted=False,budgetsAccepted=False)
(P/'COHORT.json').write_text(json.dumps(adoption,indent=2)+'\n',encoding='utf-8')
print('Prepared distinct 1.81.1 cohort with unchanged timed workload and qualified immutable references.')
