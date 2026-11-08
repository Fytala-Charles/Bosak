"""Read-only validation of the URI fix, exact artifact and qualification receipts."""
from pathlib import Path
import hashlib,json,zipfile,xml.etree.ElementTree as ET,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];read=lambda f:json.loads(f.read_text(encoding='utf-8'));sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
s=read(P/'RESULTS.json');plan=read(P/'PLAN.json');contents=read(P/'VSIX-CONTENTS.json');archive=P/contents['archive'];assert sha(archive)==contents['archiveSha256']==s['candidateSha256']
for name,h in s['sourceFiles'].items():assert sha(R/name)==h
assert sha(Path(plan['project']).parent/'ExecuteCommandHandler.cs')==s['sourceFiles']['src/Bosak.LanguageServer/ExecuteCommandHandler.cs']==plan['fixedSourceSha256']
assert sha(Path(plan['project']))==plan['derivedProjectSha256']
assert read(P/'SPECIFICATION-ADOPTION.json')['revision']=='440d8b03d784cf0bd3769b1abb33aacc24b586a6'
with zipfile.ZipFile(archive) as z:
 assert contents['files']=={i.filename:hashlib.sha256(z.read(i)).hexdigest() for i in z.infolist() if not i.is_dir()}
 for name,h in plan['engines'].items():assert contents['files']['extension/server/'+name+'.dll']==h
 assert 'extension/licenses/Unicode17-LICENSE.txt' in contents['files'] and 'extension/NOTICE' in contents['files']
 for label in ['final-fresh','upgrade']:
  root=Path(plan['work'])/label/'extensions/fytala.vscode-bosak-0.2.6'
  for name,h in contents['files'].items():
   if not name.startswith('extension/'):continue
   f=root/name.removeprefix('extension/')
   if name=='extension/package.json':
    installed=read(f);installed.pop('__metadata');assert installed==json.loads(z.read(name))
   else:assert sha(f)==h
rows=read(P/'TEST-COUNTS.json');solution=[]
for row in rows:
 f=P/row['trx'];assert sha(f)==row['sha256'];c=ET.parse(f).find('.//{*}Counters').attrib
 assert all(int(c[k])==row[k] for k in ['passed','failed','notExecuted','total'])
 if row['trx'].startswith('solution-trx/'):solution.append(row)
 elif row['trx'].startswith('database-retry-trx/'):assert row['passed']==47 and row['failed']==0
 else:assert row['passed']==90 and row['failed']==0
assert len(solution)==10 and sum(x['failed'] for x in solution)==45
assert sum(x['passed'] for x in solution)-2+47==s['solutionDistinctPassingCasesAcrossRuns']==26342
assert not s['singleFullSolutionRunGreen']
assert read(P/'solution-build.json')['exitCode']==0 and read(P/'solution-tests.json')['exitCode']==1 and read(P/'database-retry.json')['exitCode']==0
commands=read(P/'COMMANDS.json');assert len(commands['results'])==5 and commands['defaultBundledServer']
assert {x['command']:x['actual'] for x in commands['results']}=={'bosak.evaluateXPath':'6','bosak.runXQuery':'q1|q2|q3','bosak.evaluateXQuery':'42','bosak.transformXslt':'<result>23</result>','bosak.runInitialTemplate':'initial-42'}
assert all(x['passed'] for x in commands['results'])
e=read(P/'EDITOR.json');assert len(e['results'])==5 and any('XPST0017' in d for d in e['results'][1]['diagnostics'])
assert all(not x['diagnostics'] for i,x in enumerate(e['results']) if i!=1)
for phase in ['command','diagnostic']:
 assert 'exited with code: 0' in (P/(phase+'-stdout.txt')).read_text() and 'FATAL:' not in (P/(phase+'-stderr.txt')).read_text()
for name in ['commands-launch.ps1','diagnostics-launch.ps1']:assert '--no-sandbox' not in (P/'tools'/name).read_text()
for name in ['server','compile','tests','package']:
 row=read(P/(name+'-execution.json'));assert row['exitCode']==0 and sha(P/(name+'.txt'))==row['logSha256']
for row in read(P/'INSTALLATION.json'):assert row['exitCode']==0 and sha(P/row['log'])==row['logSha256']
layout=read(P/'SERVER-TEST-LAYOUT.json');root=Path(layout['path']);assert layout['files']=={f.relative_to(root).as_posix():sha(f) for f in root.rglob('*') if f.is_file()}
assert all(not s[k] for k in ['machinePermissionsChanged','activeProfileChanged','chromiumSandboxDisabled','coreProductionVersionChanged','trackedExtensionVersionChanged','publicationPerformed','ownerAcceptanceGranted','apiFreezeGranted'])
assert read(R/'vscode-bosak/package.json')['version']=='0.2.5'
for f in [P/'README.md',R/'docs/RELEASE-READINESS-1.81.1.md',R/'docs/INTEGRATION.md']:
 text=re.sub(r'`[^`]*`','',f.read_text(encoding='utf-8'))
 for target in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',text):
  target=target.strip('<>').split('#')[0]
  if target and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',target):assert (f.parent/target).exists(),(f,target)
assert read(P/'MANIFEST.json')=={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f!=P/'MANIFEST.json' and '__pycache__' not in f.parts}
print('Verified URI fix: solution26342 across environment retry, server90 in two lanes, commands5, live diagnostics5, exact nine engines and installed VSIX. No publication.')
