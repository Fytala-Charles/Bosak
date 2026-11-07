"""Compare owner public metadata between observed baseline and rebuilt candidate outputs."""
from pathlib import Path
import hashlib,json,shutil,subprocess,sys

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=read(P/'PLAN.json');work=Path(p['work']);source=Path(p['source']);owner=Path(p['ownerRoot'])
names=['Bosak.XmlDb.Core','Bosak.XmlDb.Storage','Bosak.XmlDb.Xdm'];auditor=R/'.guard-tmp/preview-qualification-1811/api-audit/bin/Release/net10.0/Audit.dll'
assert auditor.is_file()
tool=R/'docs/foundation-xslt-source-membership/tools/Inventory.cs.txt'
assert (auditor.parents[3]/'Program.cs').read_bytes()==tool.read_bytes()
if sys.argv[1:]!=['--compare-only']:
    layouts={}
    for lane,origin in [('baseline',owner/'tests/Bosak.XmlDb.Xdm.Tests/bin/Debug/net10.0'),('candidate',source/'tests/Bosak.XmlDb.Xdm.Tests/bin/Release/net10.0')]:
        root=work/('api-'+lane);root.mkdir()
        for name in names:
            folder=root/'src'/name;binary=folder/'bin/Release/net10.0';binary.mkdir(parents=True)
            shutil.copyfile(owner/'src'/name/(name+'.csproj'),folder/(name+'.csproj'))
            for file in origin.glob('*.dll'):shutil.copyfile(file,binary/file.name)
        layouts[lane]={f.relative_to(root).as_posix():sha(f) for f in root.rglob('*') if f.is_file()}
        subprocess.run([sys.executable,str(R/'docs/foundation-r2-closure/tools/run.py'),'--log',
            str(R/f'docs/foundation-r2-closure/evidence/execution/xmldb-full-api-{lane}.log'),'--timeout-seconds','120','--',
            'dotnet',str(auditor),str(root),str(E/('api-'+lane+'.json')),
            str(R/'src/Bosak.Xslt/bin/Release/net10.0/Bosak.Xslt.dll')],cwd=R,check=True)
old=read(E/'api-baseline.json');new=read(E/'api-candidate.json');deltas=[];debug_rows=[]
if sys.argv[1:]==['--compare-only']:
    layouts={lane:{f.relative_to(work/('api-'+lane)).as_posix():sha(f) for f in (work/('api-'+lane)).rglob('*') if f.is_file()} for lane in ['baseline','candidate']}
for before,after in zip(old['assemblies'],new['assemblies'],strict=True):
    assert before['assembly']==after['assembly']
    marker='[System.Diagnostics.DebuggerBrowsableAttribute((System.Diagnostics.DebuggerBrowsableState)0)];'
    removed=sorted(set(before['declarations'])-set(after['declarations']));added=set(after['declarations'])-set(before['declarations'])
    assert len(removed)==len(added)
    for row in removed:
        assert row.startswith('L ') and 'k__BackingField' in row and marker in row and row.replace(marker,'') in added
        debug_rows.append(dict(assembly=before['assembly'],before=row,after=row.replace(marker,'')))
    assert sorted(row.replace(marker,'') if row.startswith('L ') else row for row in before['declarations'])==sorted(after['declarations'])
    for key in ['friendAssemblies','publicTypes','declarationCount']:
        assert before[key]==after[key],(before['assembly'],key)
    if before['identity']!=after['identity']:deltas.append(dict(assembly=before['assembly'],kind='identity',before=before['identity'],after=after['identity']))
    if before['references']!=after['references']:deltas.append(dict(assembly=before['assembly'],kind='references',before=before['references'],after=after['references']))
assert len(debug_rows)==13
result=dict(publicProtectedRowsUnchanged=True,friendAssembliesUnchanged=True,assemblies=3,privateDebugAttributeDeltas=debug_rows,rawMetadataUnchanged=False,
    publicTypes=sum(a['publicTypes'] for a in new['assemblies']),declarationRows=sum(a['declarationCount'] for a in new['assemblies']),
    identityAndReferenceDeltas=deltas,layouts=layouts,inventoryToolSha256=sha(tool),auditorSha256=sha(auditor),
    scope='Observed baseline Debug versus isolated candidate Release metadata. Public/protected rows, private field shape, friend lists, own assembly identities and reference simple names are compared. Thirteen private DebuggerBrowsable attributes differ explicitly; reference versions and complete ABI are not covered.')
(E/'API-COMPARISON.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
print('Owner public/protected rows and friends unchanged;13 private debug-attribute rows differ;',len(deltas),'identity/reference deltas recorded explicitly.')
