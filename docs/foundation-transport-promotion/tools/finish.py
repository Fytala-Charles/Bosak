"""Finish read-only version/compatibility checks without repeating successful tests."""
from pathlib import Path
import sys,json,hashlib,subprocess,zipfile,shutil
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=ROOT/'.guard-tmp/transport-promotion-preflight'
sys.path.insert(0,str(ROOT/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode
sha=lambda b:hashlib.sha256(b).hexdigest()
def run(args,name):
 with process_error_mode(),(E/name).open('w',encoding='utf-8') as f:r=subprocess.run([str(a) for a in args],cwd=W,stdout=f,stderr=subprocess.STDOUT,timeout=120)
 if r.returncode:raise RuntimeError(name+' failed')
def write(n,v):(E/n).write_text(json.dumps(v,indent=2)+'\n',encoding='utf-8')
run(['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File',P/'tools/ReadVersions.ps1','-Directory',W/'lib'],'version-info.json')
assemblies=json.loads((E/'LAYOUT.json').read_text())['seven'];qualification=E/'legacy-qualification.json'
write('legacy-qualification.json',{'assemblies':[{'name':Path(n).stem,'sha256':h} for n,h in assemblies.items()]})
legacy=W/'Legacy';legacy.mkdir(exist_ok=True)
with zipfile.ZipFile(ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary-runtime.zip') as z:
 for n in z.namelist():
  target=(legacy/n).resolve();assert target.is_relative_to(legacy.resolve())
  if not n.endswith('/'):target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(z.read(n))
for n in assemblies:shutil.copyfile(W/'lib'/n,legacy/n)
run([sys.executable,'-B',ROOT/'tools/run-retained-foundation-caller.py','--layout',legacy,'--candidate-evidence',qualification,'--output',E/'legacy-caller.json'],'legacy-log.txt')
write('PREFLIGHT.json',{'sourceArchiveSha256':sha((ROOT/'docs/foundation-transport-full-gates/evidence/source-baseline.zip').read_bytes()),'patchSha256':sha((P/'PRODUCTION.patch').read_bytes()),'appliedOnlyIn':str(W),'livePatchApplied':False,'sourceVersion':'1.36.0'})
print('Patch applied only in preflight; affected tests, versioned metadata and unchanged caller receipts retained.')
