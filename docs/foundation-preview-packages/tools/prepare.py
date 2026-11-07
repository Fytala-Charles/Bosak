"""Archive immutable qualified sources and select a local-only preview identity."""
from pathlib import Path
import hashlib,io,json,subprocess,tarfile

P=Path(__file__).resolve().parents[1];R=P.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
commit='c0b926de34824ceb2010d5f814a4450b28b31dcc'
version='1.81.1-preview.c0b926de'
work=R/'.guard-tmp/preview-packages-1811-v3'
assert not work.exists(),'Preserve earlier preview workspace.'
source=work/'source';source.mkdir(parents=True)
paths=['src','README.md','license.md','NOTICE','assets',
       'docs/foundation-compatible-view-policy/packaging/PreviewAssets.targets']
paths+= [n for n in json.loads((R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json').read_text())['sources'] if not n.startswith('src/')]
data=subprocess.check_output(['git','archive','--format=tar',commit,'--',*paths],cwd=R)
with tarfile.open(fileobj=io.BytesIO(data)) as archive:archive.extractall(source,filter='data')
pins_path=R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json'
pins=json.loads(pins_path.read_text());count=0;materializations={}
def materialize(name,h):
    path=source/name
    if sha(path)==h:return
    original=path.read_bytes();lf=original.replace(b'\r\n',b'\n')
    matches=[value for value in [lf,lf.replace(b'\n',b'\r\n')] if hashlib.sha256(value).hexdigest()==h]
    assert matches,('Source differs beyond permitted checkout newline materialization',name)
    path.write_bytes(matches[0])
    materializations[name]=dict(archiveSha256=hashlib.sha256(original).hexdigest(),qualifiedSha256=h,
        transform='LF/CRLF checkout materialization only; target hash equals sealed qualification')
for name,h in pins['sources'].items():
    materialize(name,h)
    count+=1
preflight=json.loads((R/'docs/foundation-captured-identity-production/evidence/PACKAGE-PREFLIGHT.json').read_text())
for name,h in preflight['assets'].items():materialize(name,h)
feed=work/'feed';feed.mkdir()
(work/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>\n')
plan=dict(date='2026-11-07',candidateCommit=commit,sourceVersion='1.81.1',previewVersion=version,
    publicationAuthorized=False,apiFrozen=False,ownerAccepted=False,
    previewIdentityScope='Local-only preparation; not approved public release SemVer or full package-cohort qualification.',
    work=str(work),source=str(source),feed=str(feed),qualifiedSourceCount=count,
    qualificationPinsSha256=sha(pins_path),archiveSha256=hashlib.sha256(data).hexdigest(),
    archivedFiles={p.relative_to(source).as_posix():sha(p) for p in sorted(source.rglob('*')) if p.is_file()},
    selectedPackages=preflight['selectedPackages'],checkoutMaterializations=materializations,
    overlay='docs/foundation-compatible-view-policy/packaging/PreviewAssets.targets')
(P/'evidence').mkdir(parents=True,exist_ok=True)
(P/'PLAN.json').write_text(json.dumps(plan,indent=2)+'\n',encoding='utf-8')
print('Archived and verified',count,'qualified source files; local preview',version)
