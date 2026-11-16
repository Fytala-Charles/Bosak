from pathlib import Path
import json,hashlib,zipfile
D=Path(__file__).resolve().parent;W=D.parents[1];R=W.parents[1];P=R/'.guard-tmp/release-1.81.17-20261116/extension'
read=lambda p:json.loads(p.read_bytes())
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for f,h in read(D/'MANIFEST.json').items():assert sha(D/f)==h,f
assert sha(D/'release-manifest.json')=='66a9b1e5c1a59fa99e450b60d03637ce37223091453f6f760b8faf468c758cc3'
manifest=read(D/'release-manifest.json');assert manifest['coreVersion']=='1.81.17' and manifest['extensionVersion']=='0.2.7' and manifest['sourceCommit']=='6e183a7563009e1ce52a162354c117da0beb13e2'
for f in ['PREPARATION.json','PUBLISHER.json']:assert read(D/f)['conclusion']=='success'
service=read(D/'MARKETPLACE-VERIFICATION.json');assert service['listed'] and service['byteIdentical']
artifact=P/'artifacts/vscode-bosak-0.2.7.vsix';public=P/'marketplace-vscode-bosak-0.2.7.vsix';assert sha(artifact)==sha(public)==manifest['artifacts'][0]['sha256']==service['sha256']
assert read(D/'LSP-SMOKE-VERIFIED.json')['success'];smoke=read(D/'lsp-smoke.json');assert smoke['engineVersion']=='1.81.17' and len(smoke['packagedStdioChecks'])==3
release=read(D/'GITHUB-RELEASE.json');assert not release['isDraft'] and release['tagName']=='v1.81.17';assert any(a['digest']=='sha256:'+sha(artifact) for a in release['assets'])
with zipfile.ZipFile(D/'EVIDENCE.zip') as z:assert len(z.namelist())==len(set(z.namelist()))
print('Offline extension evidence verified: hosted preparation/publication, exact public VSIX, packaged diagnostics and GitHub asset.')
