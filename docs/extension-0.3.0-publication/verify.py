"""Verify the extension delivery receipts without launching VS Code or publishing."""
from pathlib import Path
import json,hashlib,subprocess
D=Path(__file__).resolve().parent;W=D.parents[1];R=W.parents[1];P=R/'.guard-tmp/extension-snippets-release-20261116'
read=lambda p:json.loads(p.read_bytes())
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for f,h in read(D/'MANIFEST.json').items():assert sha(D/f)==h,f
assert sha(D/'RELEASE-MANIFEST.json')=='32dcfde6954ad34048aec0f1a40166fd2e48a20ac4052cf2d56eb879ada0807d'
manifest=read(D/'RELEASE-MANIFEST.json');assert manifest['sourceCommit']=='1dbfbc34d4dd3c7844d399ac67aaf6a706c4301e' and manifest['extensionVersion']=='0.3.0' and manifest['coreVersion']=='1.81.17'
for f in ['PREPARATION.json','PUBLISHER.json','PR-CI.json','PR-SCHEMA.json','PR-STANDALONE.json']:assert read(D/f)['conclusion']=='success'
for phase in ['normal','offline']:
 result=read(D/('EDITOR-'+phase+'.json'));assert result['success'] and len(result['cases'])==5 and result['vscodeVersion']=='1.146.0' and result['extensionVersion']=='0.3.0'
assert read(D/'EDITOR-LAUNCH-normal-path-fixed.json')['success'] and read(D/'EDITOR-LAUNCH-offline.json')['success']
assert read(D/'INSTALLED-PAYLOAD-VERIFICATION.json')['filesVerified']==295
assert read(D/'PR-MERGED.json')['state']=='MERGED'
service=read(D/'MARKETPLACE-VERIFICATION.json');assert service['listed'] and service['byteIdentical']
expected=manifest['artifacts'][0]['sha256'];assert sha(P/'artifacts/vscode-bosak-0.3.0.vsix')==sha(P/'marketplace-vscode-bosak-0.3.0.vsix')==expected==service['sha256']
release=read(D/'GITHUB-RELEASE.json');assert not release['isDraft'] and release['tagName']=='vscode-v0.3.0';assert any(x['digest']=='sha256:'+expected for x in release['assets'])
assert not subprocess.check_output(['git','diff',manifest['sourceCommit'],'--name-only','--','src','tests','vscode-bosak/snippets','vscode-bosak/package.json','vscode-bosak/package-lock.json','vscode-bosak/test'],cwd=W,text=True)
print('Extension 0.3.0 delivery verified: PR checks/merge, exact artifacts, real editor normal/offline cases and public Marketplace/GitHub delivery.')
