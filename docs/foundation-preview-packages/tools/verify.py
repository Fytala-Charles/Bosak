"""Verify local preview preparation, preserving the distinction from release qualification."""
from pathlib import Path
import hashlib,io,json,re,subprocess,tarfile,xml.etree.ElementTree as ET,zipfile
from packages import inspect

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=read(P/'PLAN.json');assert not any(p[k] for k in ['publicationAuthorized','apiFrozen','ownerAccepted'])
assert p['candidateCommit']=='c0b926de34824ceb2010d5f814a4450b28b31dcc' and p['previewVersion']=='1.81.1-preview.c0b926de'
pins_path=R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json'
assert sha(pins_path)==p['qualificationPinsSha256']
pins=read(pins_path);source=Path(p['source']);assert len(pins['sources'])==p['qualifiedSourceCount']==669
for name,h in pins['sources'].items():assert sha(source/name)==h,name
paths=['src','README.md','license.md','NOTICE','assets',p['overlay']]+[n for n in pins['sources'] if not n.startswith('src/')]
data=subprocess.check_output(['git','archive','--format=tar',p['candidateCommit'],'--',*paths],cwd=R)
assert hashlib.sha256(data).hexdigest()==p['archiveSha256']
with tarfile.open(fileobj=io.BytesIO(data)) as archive:
    files={m.name:archive.extractfile(m).read() for m in archive.getmembers() if m.isfile()}
assert set(files)==set(p['archivedFiles'])
for name,original in files.items():
    actual=(source/name).read_bytes();assert hashlib.sha256(actual).hexdigest()==p['archivedFiles'][name]
    if name in p['checkoutMaterializations']:
        row=p['checkoutMaterializations'][name]
        assert hashlib.sha256(original).hexdigest()==row['archiveSha256'] and sha(source/name)==row['qualifiedSha256']
        assert original.replace(b'\r\n',b'\n')==actual.replace(b'\r\n',b'\n')
    else:assert actual==original,name
with zipfile.ZipFile(E/'SOURCE-BUNDLE.zip') as z:
    assert set(z.namelist())==set(p['archivedFiles'])
    for name,h in p['archivedFiles'].items():assert hashlib.sha256(z.read(name)).hexdigest()==h
result=inspect();assert read(E/'PACKAGES.json')==result
with zipfile.ZipFile(E/'PREVIEW-PACKAGES.zip') as z:
    assert set(z.namelist())=={Path(r['path']).name for r in result['packages']}
    for row in result['packages']:assert hashlib.sha256(z.read(Path(row['path']).name)).hexdigest()==row['sha256']
consumer=read(E/'CONSUMER-RESULTS.json');assert consumer['passed']==6 and consumer['failed']==0 and len(consumer['cases'])==6
assert not consumer['ownerAccepted'] and not consumer['publicationAuthorized']
retained=read(E/'RETAINED-FOUNDATION.json');assert retained['passed']==20 and retained['failed']==0 and len(retained['cases'])==20
candidate=read(E/'FOUNDATION-CANDIDATE.json');assert len(candidate['assemblies'])==7
assert {r['name']:r['sha256'] for r in candidate['assemblies']}=={r['name']:r['sha256'] for r in retained['loadedAssemblies']}
for row in candidate['assemblies']:assert row['sha256']==result['loadedEngines'][row['name']]
project=ET.parse(E/'Consumer.csproj');assert not project.findall('.//ProjectReference') and not project.findall('.//HintPath')
refs=project.findall('.//PackageReference');assert len(refs)==1 and refs[0].attrib==dict(Include='Bosak.Xslt',Version='['+p['previewVersion']+']')
assert (Path(p['work'])/'consumer/Program.cs').read_bytes()==(P/'consumer/Program.cs').read_bytes()
with zipfile.ZipFile(E/'EXECUTION-LAYOUTS.zip') as z:
    for group,layout in read(E/'EXECUTION-LAYOUTS.json').items():
        folder=Path(layout['path'])
        assert layout['files']=={f.relative_to(folder).as_posix():sha(f) for f in folder.rglob('*') if f.is_file()}
        for name,h in layout['files'].items():assert hashlib.sha256(z.read(group+'/'+name)).hexdigest()==h
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
    receipts=[n for n in z.namelist() if n.endswith('.log.json')];assert len(receipts)==14
    for name in receipts:
        row=json.loads(z.read(name));assert row['exitCode']==0 and row['failure'] is None
        assert hashlib.sha256(z.read(name[:-5])).hexdigest()==row['logSha256']
    for name in ['preview-1811-build.log','preview-1811-consumer-build.log']:
        text=z.read(name).decode('utf-8-sig');assert '0 Warning(s)' in text and '0 Error(s)' in text
for file in [P/'README.md',R/'docs/CHECKPOINT-1.81.1.md']:
    text=re.sub(r'`[^`]*`','',file.read_text(encoding='utf-8'))
    for link in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',text):
        name=link.strip('<>').split('#')[0]
        if name and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',name):assert (file.parent/name).exists(),(file,name)
if (P/'MANIFEST.json').exists():
    files={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
    assert read(P/'MANIFEST.json')==files
print('Verified PV-Q08: immutable materialized source, 9 preview packages, fresh-cache six-case package consumer, retained20, assets/dependencies/cache/output/loaded hashes. Full package-backed qualification and owner gates remain open.')
