"""Verify the successful normally sandboxed isolated editor-host run."""
from pathlib import Path
import hashlib,json,zipfile
P=Path(__file__).resolve().parents[1];R=P.parents[1]
read=lambda f:json.loads(f.read_text(encoding='utf-8'));sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
s=read(P/'RESULTS.json');plan=read(P/'PLAN.json');e=read(P/'EDITOR.json')
assert s['status']=='EditorExecutionBlockerResolved' and s['checksPassed']==len(e['results'])==5
assert e['extensionVersion']==s['extensionVersion']=='0.2.6' and sha(P/'EDITOR.json')==s['editorReportSha256']
assert [x['name'] for x in e['results']]==['valid','invalid','corrected-live','xpath-invalid-and-live-correction','xquery-invalid-and-live-correction']
assert any('XPST0017' in x for x in e['results'][1]['diagnostics'])
assert all(not row['diagnostics'] for i,row in enumerate(e['results']) if i!=1)
assert sha(Path(plan['harness'])/'test.cjs')==plan['harnessSha256']==sha(P/'tools/editor-test.cjs')
assert 'server.path' in (P/'tools/editor-test.cjs').read_text()
launch=(P/'tools/launch.ps1').read_text();assert '--no-sandbox' not in launch and '-WindowStyle Hidden' in launch
assert all(not s[k] for k in ['machineAclChanged','chromiumSandboxDisabled','activeProfileChanged','publicationPerformed'])
assert 'exited with code: 0' in (P/'editor-stdout.txt').read_text() and 'FATAL:' not in (P/'editor-stderr.txt').read_text()
archive=Path(s['candidateArchive']);assert sha(archive)==s['candidateSha256'];installed=Path(plan['extensions'])/'fytala.vscode-bosak-0.2.6'
with zipfile.ZipFile(archive) as z:
 for name,h in s['installedPayloadVerified'].items():assert sha(installed/name.removeprefix('extension/'))==h==hashlib.sha256(z.read(name)).hexdigest()
 manifest=read(installed/'package.json');manifest.pop('__metadata');assert manifest==json.loads(z.read('extension/package.json'))
assert read(P/'MANIFEST.json')=={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f!=P/'MANIFEST.json' and '__pycache__' not in f.parts}
print('Verified live XPath/XQuery/XSLT diagnostics and correction, installed VSIX content, normal sandbox and host exit0. No ACL or active-profile change.')
