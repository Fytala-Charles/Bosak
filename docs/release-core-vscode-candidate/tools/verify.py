"""Read-only verification of the isolated extension candidate evidence."""
from pathlib import Path
import hashlib,json,zipfile,xml.etree.ElementTree as ET,re
P=Path(__file__).resolve().parents[1];R=P.parents[1]
read=lambda p:json.loads(p.read_text(encoding='utf-8'));sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
plan=read(P/'PLAN.json');s=read(P/'RESULTS.json');contents=read(P/'VSIX-CONTENTS-NOTICES.json');archive=P/s['archive'];assert sha(archive)==s['archiveSha256']==contents['archiveSha256']
with zipfile.ZipFile(archive) as z:
 files={i.filename:hashlib.sha256(z.read(i)).hexdigest() for i in z.infolist() if not i.is_dir()};assert files==contents['files']
 for name,h in plan['engines'].items():assert files['extension/server/'+name+'.dll']==h
 assert 'extension/licenses/Unicode17-LICENSE.txt' in files and 'extension/NOTICE' in files
 assert json.loads(z.read('extension/package.json'))['version']=='0.2.6'
 for folder in ['final-fresh','final-upgrade']:
  root=Path(plan['work'])/folder/'extensions/fytala.vscode-bosak-0.2.6'
  for name,h in files.items():
   if not name.startswith('extension/'):continue
   f=root/name.removeprefix('extension/')
   if name=='extension/package.json':
    installed=read(f);installed.pop('__metadata');assert installed==json.loads(z.read(name))
   else:assert sha(f)==h
assert sha(Path(plan['project']))==plan['derivedProjectSha256']
for name,h in plan['sourceFiles'].items():
 if not name.endswith('.csproj'):assert sha(Path(plan['work'])/'source'/name)==h
layout=read(P/'SERVER-TEST-LAYOUT.json');root=Path(layout['path']);assert layout['files']=={f.relative_to(root).as_posix():sha(f) for f in root.rglob('*') if f.is_file()}
trx=list((P/'server-tests-trx').glob('*.trx'));assert len(trx)==1;c=ET.parse(trx[0]).find('.//{*}Counters').attrib;assert c['total']==c['passed']=='84' and c['failed']==c['notExecuted']=='0'
for row in read(P/'INSTALLATION-NOTICES.json'):
 assert row['exitCode']==0 and sha(P/row['log'])==row['logSha256']
for name in ['server','compile','tests','package']:
 row=read(P/(name+'-execution.json'));assert row['exitCode']==0 and sha(P/(name+'.txt'))==row['logSha256']
protocol=read(P/'PROTOCOL-NOTICES.json');assert protocol['archiveSha256']==s['archiveSha256'] and len(protocol['packagedStdioChecks'])==3
assert not s['liveEditorPassed'] and not s['publicationPerformed'] and not s['activeEditorProfileChanged'] and not s['productionVersionChanged']
assert read(R/'vscode-bosak/package.json')['version']=='0.2.5'
for f in [P/'README.md',R/'docs/release-core-vscode/README.md',R/'docs/RELEASE-READINESS-1.81.1.md']:
 text=re.sub(r'`[^`]*`','',f.read_text(encoding='utf-8'))
 for target in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',text):
  target=target.strip('<>').split('#')[0]
  if target and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',target):assert (f.parent/target).exists(),(f,target)
assert read(P/'MANIFEST.json')=={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f!=P/'MANIFEST.json' and '__pycache__' not in f.parts}
print('Verified local VSIX, nine exact engines, server84, protocol3, fresh/upgrade installed content. Live editor-host qualification remains blocked; no publication.')
