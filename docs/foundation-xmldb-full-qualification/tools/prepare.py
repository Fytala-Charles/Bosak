"""Record explicit execution authorization and pin the isolated full-suite candidate."""
from pathlib import Path
import hashlib,json,shutil,subprocess

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
old=R/'docs/foundation-xmldb-source-candidate';p=read(old/'PLAN.json');results=read(old/'evidence/RESULTS.json')
owner=Path(p['ownerRoot']);source=Path(p['source'])
assert subprocess.check_output(['git','--no-optional-locks','-C',str(owner),'rev-parse','HEAD'],text=True).strip()==p['ownerHead']
assert subprocess.check_output(['git','--no-optional-locks','-C',str(owner),'status','--porcelain'],text=True).strip()==''
for name,h in p['sourceFiles'].items():assert sha(source/name)==results['dependencyLockChanges'].get(name,h),name
for row in results['projects']:
    for name,h in row['execution']['files'].items():assert sha(Path(row['execution']['path'])/name)==h
work=R/'.guard-tmp/xmldb-full-qualification';assert not work.exists();work.mkdir();E.mkdir(parents=True)
auth=dict(date='2026-11-07',explicitUserAuthorizationReceived=True,
    userInstruction='Yes, allowed to do the storage sweep and integration intentional child-process kill/fault probes',
    scope='Isolated full Storage sweep and Integration intentional child-process termination/fault probes only; not owner release/API/budget acceptance.',
    unexpectedManagedFailuresMustBeCaught=True,machineWideErrorSettingsAllowed=False,
    priorRestrictionReceiptSha256=sha(old/'PROCESS-REVIEW.json'))
(P/'AUTHORIZATION.json').write_text(json.dumps(auth,indent=2)+'\n',encoding='utf-8')
integration=source/'tests/Bosak.XmlDb.IntegrationTests/bin/Release/net10.0'
shutil.copytree(integration,work/'integration-original')
plan=dict(date='2026-11-07',source=str(source),work=str(work),ownerRoot=str(owner),ownerHead=p['ownerHead'],
    priorSourcePlanSha256=sha(old/'PLAN.json'),priorSourceManifestSha256=sha(old/'MANIFEST.json'),
    authorizationSha256=sha(P/'AUTHORIZATION.json'),previewVersion=p['previewVersion'],
    packageManifestSha256=sha(R/'docs/foundation-preview-packages/MANIFEST.json'),
    originalIntegrationFiles={f.relative_to(integration).as_posix():sha(f) for f in sorted(integration.rglob('*')) if f.is_file()},
    ownerAccepted=False,apiFrozen=False,publicationAuthorized=False)
storage=source/'tests/Bosak.XmlDb.Storage.Tests/bin/Release/net10.0'
plan['storageLayout']=dict(path=str(storage),files={f.relative_to(storage).as_posix():sha(f) for f in sorted(storage.rglob('*')) if f.is_file()})
(P/'PLAN.json').write_text(json.dumps(plan,indent=2)+'\n',encoding='utf-8')
print('Recorded explicit authorization; pinned Storage and preserved original Integration outputs. Earlier restriction packet unchanged.')
