"""Read-only checks for actual installed-extension command execution."""
from pathlib import Path
import hashlib,json,zipfile,re
P=Path(__file__).resolve().parents[1];R=P.parents[1];read=lambda p:json.loads(p.read_text(encoding='utf-8'));sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
s=read(P/'RESULTS.json');plan=read(P/'PLAN.json');commands=read(P/'COMMANDS-COMPLETE.json');assert len(commands['results'])==plan['expectedCommands']==5
expected={'bosak.evaluateXPath':'6','bosak.runXQuery':'q1|q2|q3','bosak.evaluateXQuery':'42','bosak.transformXslt':'<result>23</result>','bosak.runInitialTemplate':'initial-42'}
assert len([row for row in commands['results'] if row['passed']])==s['passed']==2
assert len([row for row in commands['results'] if not row['passed']])==s['failed']==3
for row in commands['results']:
 assert row['expected']==expected[row['command']]
 if row['passed']:assert row['actual']==row['expected']
 else:assert row['actual'] is None
repro=read(P/'URI-REPRODUCTION.json');assert repro[0]['notification']['error']=='Document is not open.' and repro[1]['notification']['result']=='6'
assert sha(P/'COMMANDS-COMPLETE.json')==s['commandsReceiptSha256']
assert commands['extensionVersion']=='0.2.6' and commands['defaultBundledServer']
assert sha(P/'tools/editor-complete.cjs')==s['completeHarnessSha256']
assert 'exited with code: 0' in (P/'complete-stdout.txt').read_text() and 'FATAL:' not in (P/'complete-stderr.txt').read_text()
assert '--no-sandbox' not in (P/'tools/launch.ps1').read_text()
contents=read(R/plan['candidateManifest']);archive=R/'docs/release-core-vscode-candidate'/contents['archive'];assert sha(archive)==contents['archiveSha256']==s['candidateSha256']
root=Path(commands['extensionPath'])
with zipfile.ZipFile(archive) as z:
 for name,h in contents['files'].items():
  if not name.startswith('extension/'):continue
  f=root/name.removeprefix('extension/')
  if name=='extension/package.json':
   manifest=read(f);manifest.pop('__metadata');assert manifest==json.loads(z.read(name))
  else:assert sha(f)==h
assert not any(s[k] for k in ['activeProfileChanged','machineSettingsChanged','publicationPerformed','productionVersionChanged'])
assert read(P/'MANIFEST.json')=={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f!=P/'MANIFEST.json' and '__pycache__' not in f.parts}
assert not s['fixApplied'] and not s['productionSourceChanged']
print('Verified actual commands: two passes, three URI lookup failures; deterministic URI reproduction and unchanged VSIX. Release remains blocked; no publication.')
