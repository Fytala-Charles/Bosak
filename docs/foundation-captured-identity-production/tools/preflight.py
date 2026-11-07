"""Inspect exact qualified sources, package graph and asset closure; never build, pack, restore or publish."""
from pathlib import Path
import hashlib,json,subprocess
P=Path(__file__).resolve().parents[1];R=P.parents[1]
SELECTED={'Bosak.XPath.Core','Bosak.XPath.Parser','Bosak.XPath.Compiler','Bosak.XPath.Runtime','Bosak.XPath.Standard','Bosak.XPath.Providers','Bosak.XPath.Api','Bosak.XQuery','Bosak.Xslt'}
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def guard_files(root,files):
 for name,digest in files.items():
  p=root/name
  if not p.is_file() or sha(p)!=digest:raise ValueError('Candidate hash mismatch: '+name)
def packed_items(root,evaluation):
 result=[]
 for item in evaluation['Items']['None']:
  if item.get('Pack','').lower()!='true':continue
  p=Path(item['FullPath']).resolve()
  if not p.is_relative_to(root):raise ValueError('Pack input escapes repository: '+str(p))
  if not p.is_file():raise ValueError('Missing pack asset: '+str(p))
  result.append(dict(source=p.relative_to(root).as_posix(),sha256=sha(p),packagePath=item.get('PackagePath','')))
 return sorted(result,key=lambda x:(x['source'],x['packagePath']))
def main():
 e=P/'evidence';out=e/'package-evaluations';out.mkdir(exist_ok=True)
 pins=json.loads((P/'evidence/SOURCE-AND-BINARY-PINS.json').read_text(encoding='utf-8'))
 guard_files(R,pins['sources'])
 for row in pins['engines'].values():
  if sha(Path(row['path']))!=row['sha256']:raise ValueError('Qualified binary changed')
 notice=R/'NOTICE'
 if not notice.is_file():raise ValueError('Candidate NOTICE is required')
 projects={};commands=[]
 def evaluate(project,overlay=False):
  argv=['dotnet','msbuild',str(project),'-getProperty:PackageId,PackageVersion,Version,IsPackable,GenerateDocumentationFile,TargetFramework,PackageLicenseFile,PackageReadmeFile,PackageIcon,IncludeBuildOutput','-getItem:None,ProjectReference,PackageReference']
  if overlay:argv+=['-p:CustomBeforeMicrosoftCommonTargets='+str(R/'docs/foundation-compatible-view-policy/packaging/PreviewAssets.targets'),'-p:CandidateSourceRoot='+str(R)]
  raw=subprocess.check_output(argv,cwd=R);result=json.loads(raw.decode('utf-8-sig'));name=project.stem+('-overlay' if overlay else '')+'.json';(out/name).write_bytes(raw);commands.append(dict(argv=argv,evaluation='evidence/package-evaluations/'+name,sha256=sha(out/name)));return result
 for project in sorted((R/'src').glob('*/*.csproj')):
  base=evaluate(project);name=project.stem;refs=[]
  for item in base['Items'].get('ProjectReference',[]):
   path=Path(item['FullPath']).resolve()
   if not path.is_relative_to(R/'src'):raise ValueError('Project dependency escapes source tree')
   refs.append(path.stem)
  assets=packed_items(R,base)
  row=dict(project=project.relative_to(R).as_posix(),properties=base['Properties'],projectReferences=sorted(refs),packageReferences=[dict(id=i['Identity'],version=i.get('Version')) for i in base['Items'].get('PackageReference',[])],stockAssets=assets,selected=name in SELECTED,stockNoticeIncluded=any(a['source']=='NOTICE' for a in assets))
  if name in SELECTED:
   overlay=evaluate(project,True);row['overlayAssets']=packed_items(R,overlay)
   assert overlay['Properties']==base['Properties']
   assert any(a['source']=='NOTICE' for a in row['overlayAssets'])
   assert all(any(a['source']==required for a in row['overlayAssets']) for required in ['NOTICE','README.md','license.md'])
   assert base['Properties']['Version']==base['Properties']['PackageVersion']=='1.81.1'
   assert base['Properties']['GenerateDocumentationFile']=='true' and base['Properties']['TargetFramework']=='net10.0'
  projects[name]=row
 closure=set();todo=['Bosak.Xslt']
 while todo:
  name=todo.pop()
  if name in closure:continue
  closure.add(name);todo+=projects[name]['projectReferences']
 assert closure==SELECTED,closure
 assets={a['source']:a['sha256'] for row in projects.values() for a in row.get('overlayAssets',row['stockAssets'])}
 relevant=set(pins['sources'])|set(assets)|{(R/'docs/foundation-compatible-view-policy/packaging/PreviewAssets.targets').relative_to(R).as_posix()}
 dirty=set(subprocess.check_output(['git','diff','--name-only','HEAD'],cwd=R,text=True).splitlines())|set(subprocess.check_output(['git','ls-files','--others','--exclude-standard'],cwd=R,text=True).splitlines())
 changed=sorted(relevant&dirty)
 result=dict(date='2026-11-07',sourceVersion='1.81.1',selectedPackages=sorted(SELECTED),projects=projects,assets=assets,commands=commands,dirtyQualifiedInputs=changed,candidateCommitted=not changed,targetPreviewVersion=None,releaseDate=None,packageRecipeEvaluated=True,stockNoticeMissing=[n for n in sorted(SELECTED) if not projects[n]['stockNoticeIncluded']],overlayNoticeIncluded=True,packagesBuilt=False,packageConsumersRun=False,publicationAuthorized=False,apiFrozen=False)
 (e/'PACKAGE-PREFLIGHT.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
 guard_files(R,pins['sources'])
 print(f'Evaluated {len(projects)} projects; exact nine-package XSLT closure; NOTICE overlay verified. {len(changed)} qualified/package inputs differ from HEAD. No build/pack/restore/publication.')
if __name__=='__main__':main()
