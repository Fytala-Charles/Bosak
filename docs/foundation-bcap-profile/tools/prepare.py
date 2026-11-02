"""Create a full-build source layout from the immutable isolated outcome candidate."""
from pathlib import Path
import shutil,subprocess,json,hashlib,os
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);W=R/'.guard-tmp/bcap-profile';OLD=R/'.guard-tmp/bcap-outcomes-v3'
if W.exists():raise RuntimeError('Existing candidate retained; refuse overwrite')
i=json.loads((R/'docs/foundation-bcap-outcomes/evidence/INPUTS.json').read_text())
for n,h in i['candidateSources'].items():assert hashlib.sha256((OLD/n).read_bytes()).hexdigest()==h,n
shutil.copytree(OLD/'src',W/'src',ignore=shutil.ignore_patterns('bin','obj'))
for n in ['license.md','NOTICE','README.md','NuGet.Config','global.json','Bosak.sln']:
 if (R/n).exists():shutil.copyfile(R/n,W/n)
tests={}
for n in subprocess.check_output(['git','ls-files','tests'],text=True).splitlines():
 f=R/n
 if not f.is_file():continue
 dest=W/n;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(f,dest);tests[n]=hashlib.sha256(f.read_bytes()).hexdigest()
for link,target in [(W/'tests/qt3tests',R/'tests/qt3tests'),(W/'tests/qt4tests',R/'tests/qt4tests'),(W/'docs',R/'docs')]:
 env=os.environ.copy();env['BOSAK_FIXTURE_LINK']=str(link);env['BOSAK_FIXTURE_TARGET']=str(target)
 subprocess.run(['powershell','-NoProfile','-Command','New-Item -ItemType Junction -Path $env:BOSAK_FIXTURE_LINK -Target $env:BOSAK_FIXTURE_TARGET | Out-Null'],env=env,stdout=subprocess.PIPE,stderr=subprocess.PIPE,check=True)
(E/'SOURCE-INPUTS.json').write_text(json.dumps({'sourceBase':i['sourceRevision'],'candidateSources':i['candidateSources'],'tests':tests,'productionChanged':False},indent=2)+'\n')
for n in ['run.py','regressions.py','seal-inputs.py','verify.py']:
 if not (P/'tools'/n).exists():(P/'tools'/n).write_bytes((R/'docs/foundation-bcap-outcomes/tools'/n).read_text().replace('bcap-outcomes-v3','bcap-profile').encode())
if not (P/'tools/Probe.cs.txt').exists():(P/'tools/Probe.cs.txt').write_bytes((R/'docs/foundation-bcap-outcomes/tools/Probe.cs.txt').read_bytes())
if not (P/'tools/SchemaCaptureCandidate.cs.txt').exists():(P/'tools/SchemaCaptureCandidate.cs.txt').write_bytes((R/'docs/foundation-bcap-outcomes/tools/SchemaCaptureCandidate.cs.txt').read_bytes())
print('Prepared full solution candidate with immutable engine sources and pinned test sources.')
