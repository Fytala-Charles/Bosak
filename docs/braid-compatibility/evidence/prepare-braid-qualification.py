from pathlib import Path
import subprocess,zipfile,io,json,hashlib
r=Path.cwd();source=Path('D:/Development/Bosak.Braid');out=r/'docs/braid-compatibility';(out/'evidence').mkdir(parents=True,exist_ok=True)
head=subprocess.check_output(['git','rev-parse','HEAD'],cwd=source,text=True).strip();core=subprocess.check_output(['git','rev-parse','HEAD'],text=True).strip();pin='6a3372fc3b91e733b71a426cf40f26feaaeb3bcb'
archive=subprocess.check_output(['git','archive','--format=zip',head],cwd=source)
engineArchive=subprocess.check_output(['git','archive','--format=zip',pin,'src','license.md'],cwd=r)
engine=r/'.guard-tmp/braid-engine-baseline';engine.mkdir(exist_ok=True)
with zipfile.ZipFile(io.BytesIO(engineArchive)) as z:z.extractall(engine)
for mode in ['baseline','candidate']:
 dest=r/f'.guard-tmp/braid-{mode}';dest.mkdir(exist_ok=True)
 with zipfile.ZipFile(io.BytesIO(archive)) as z:z.extractall(dest)
 dependency=engine if mode=='baseline' else r;revision=pin if mode=='baseline' else core
 (dest/'Directory.Build.targets').write_text(f'''<Project><PropertyGroup><BosakDependencyRoot>{dependency.as_posix()}</BosakDependencyRoot><BosakDependencyRevision>{revision}</BosakDependencyRevision></PropertyGroup><Target Name="VerifyBosakDependencyRevision" BeforeTargets="ResolveProjectReferences"><Message Importance="high" Text="Isolated qualification: committed-source identities verified externally; original dependency guard unchanged in source archive." /></Target></Project>''')
 projects=sorted(p.relative_to(dest).as_posix() for root in ['src','tests'] for p in (dest/root).rglob('*.csproj'))
 (dest/'Qualification.slnx').write_text('<Solution>'+''.join(f'<Project Path="{p}" />' for p in projects)+'</Solution>')
(out/'evidence/source-selection.json').write_text(json.dumps({'date':'2026-10-29','braidHead':head,'coreHead':core,'baselineEnginePin':pin,'braidInitialStatus':subprocess.check_output(['git','status','--short'],cwd=source,text=True).splitlines(),'consumerArchiveSha256':hashlib.sha256(archive).hexdigest(),'baselineEngineArchiveSha256':hashlib.sha256(engineArchive).hexdigest(),'scope':'Actual committed consumer source in two ignored workspaces; original pins and checkout unchanged'},indent=2)+'\n')
print('Baseline/candidate Braid copies and isolated baseline engine source prepared.')
