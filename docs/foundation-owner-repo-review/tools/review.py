"""Read-only inspection of owner documentation; no approvals inferred from design returns."""
from pathlib import Path
import hashlib,json,subprocess,sys

P=Path(__file__).resolve().parents[1];R=P.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
roots={'Schema':Path('D:/Development/Bosak.Schema.Public'),'XmlDb':Path('D:/Development/Bosal.XmlDb'),'Braid':Path('D:/Development/Bosak.Braid')}
def inventory():
    rows=[]
    for owner,root in roots.items():
        def git(*args):return subprocess.check_output(['git','--no-optional-locks','-C',str(root),*args],text=True).strip()
        before=git('status','--porcelain');head=git('rev-parse','HEAD')
        files={};exact=[];returns=[]
        for file in sorted((root/'docs').rglob('*')):
            if not file.is_file() or file.suffix.lower() not in ['.md','.json']:continue
            relative=file.relative_to(root).as_posix();files[relative]=sha(file)
            text=file.read_text(encoding='utf-8-sig')
            if '1.81.1' in text or 'preview.c0b926de' in text:exact.append(relative)
            if file.name.endswith('response.json'):
                data=json.loads(text)
                if data.get('owner')==owner:
                    returns.append(dict(path=relative,sha256=files[relative],decision=data.get('decision'),
                        date=data.get('date'),sourceRevision=data.get('sourceRevision'),
                        proposalManifestSha256=data.get('proposalManifestSha256'),
                        runtimeQualification=data.get('runtimeQualification'),c45Accepted=data.get('c45Accepted'),apiFrozen=data.get('apiFrozen')))
        assert git('rev-parse','HEAD')==head and git('status','--porcelain')==before
        rows.append(dict(owner=owner,root=str(root),head=head,status=before,documentationHashes=files,
            exactPreviewMentions=exact,designReturns=returns))
    return dict(date='2026-11-07',readOnly=True,repositories=rows,
        resolvedNames={'requestedXmlDb':'Bosak.XmlDb','actualXmlDb':'Bosal.XmlDb','requestedBraid':'Bosay.Braid','actualBraid':'Bosak.Braid'},
        scope='Current docs/**/*.md and docs/**/*.json only; literal cohort search and review of recorded owner returns/handovers. No source execution, remote fetch, external writes or owner messages.',
        currentPackageAcceptanceFound=False,requirementsWaived=0)
if sys.argv[1:]==['--capture']:
    result=inventory();assert all(not r['exactPreviewMentions'] for r in result['repositories'])
    (P/'REPOSITORIES.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
else:
    expected=json.loads((P/'REPOSITORIES.json').read_text());assert inventory()==expected
    assert not expected['currentPackageAcceptanceFound'] and expected['requirementsWaived']==0
    for row in expected['repositories']:
        assert row['designReturns']
        assert all(r['c45Accepted'] is not True and r['apiFrozen'] is not True for r in row['designReturns'])
print('Reviewed three live owner repositories without changes; recorded replies remain design/current-pin scope, with no exact-preview acceptance found in reviewed documents.')
