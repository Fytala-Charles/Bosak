"""Replay the unchanged foundation binary through its mandatory safe launcher."""
from pathlib import Path
import hashlib,json,subprocess,sys,zipfile
from packages import inspect

P=Path(__file__).resolve().parents[1];R=P.parents[1]
p=json.loads((P/'PLAN.json').read_text());packages=inspect();layout=Path(p['work'])/'retained'
layout.mkdir()
with zipfile.ZipFile(R/'docs/foundation-legacy-compatibility/evidence/unchanged-binary-runtime.zip') as z:
    for name in ['LegacyCaller.dll','LegacyCaller.deps.json','LegacyCaller.runtimeconfig.json']:
        (layout/name).write_bytes(z.read(name))
names=json.loads((R/'docs/foundation-captured-identity-production/evidence/FOUNDATION-CANDIDATE.json').read_text())['assemblies']
assemblies=[]
for row in names:
    name=row['name']
    package=next(x for x in packages['packages'] if x['id']==name)
    with zipfile.ZipFile(package['path']) as z:(layout/(name+'.dll')).write_bytes(z.read('lib/net10.0/'+name+'.dll'))
    assemblies.append(dict(name=name,sha256=hashlib.sha256((layout/(name+'.dll')).read_bytes()).hexdigest()))
candidate=P/'evidence/FOUNDATION-CANDIDATE.json'
candidate.write_text(json.dumps(dict(previewVersion=p['previewVersion'],assemblies=assemblies),indent=2)+'\n')
subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',
    str(R/'docs/foundation-r2-closure/evidence/execution/preview-1811-retained.log'),'--timeout-seconds','120','--',
    sys.executable,str(R/'tools/run-retained-foundation-caller.py'),'--layout',str(layout),
    '--output',str(P/'evidence/RETAINED-FOUNDATION.json'),'--candidate-evidence',str(candidate)],cwd=R,check=True)
print('Retained unchanged caller completed through hash-qualified scoped launcher.')
