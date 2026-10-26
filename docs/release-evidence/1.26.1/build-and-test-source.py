from pathlib import Path
import subprocess,os,json
r=Path('D:/Development/Bosak');s=r/'.guard-tmp/release1261-source';e=r/'.guard-tmp/release1261-candidate';links=[]
try:
 for name in ['qt3tests','qt4tests']:
  p=s/'tests'/name;target=r/'tests'/name
  assert p.resolve().is_relative_to(s.resolve()) and not p.is_junction() and not any(p.iterdir())
  p.rmdir();subprocess.run(['powershell','-NoProfile','-Command',f"New-Item -ItemType Junction -Path '{p}' -Target '{target}' | Out-Null"],check=True)
  assert p.is_junction() and p.resolve()==target.resolve();links.append((p,target))
 jobs=[('build',['dotnet','build','Bosak.sln','-c','Release']),('source-tests',['dotnet','test','Bosak.sln','-c','Release','--no-build','--logger','trx','--results-directory',str(e/'full')]),('lsp-tests',['dotnet','test','tests/Bosak.LanguageServer.Tests','-c','Release','--logger','trx','--results-directory',str(e/'lsp')])]
 results=[]
 for label,args in jobs:
  print('Running '+label,flush=True)
  with (e/(label+'.log')).open('wb') as f:proc=subprocess.run(args,cwd=s,stdout=f,stderr=subprocess.STDOUT)
  results.append(dict(label=label,exitCode=proc.returncode));(e/'validation-exits.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
  assert proc.returncode==0,(label,proc.returncode)
finally:
 for p,target in links:
  assert p.is_junction() and p.resolve()==target.resolve() and p.parent.resolve()==(s/'tests').resolve()
  os.rmdir(p);p.mkdir()
print('All solution/LSP tests passed; fixture junctions removed.',flush=True)
with (e/'pack.log').open('wb') as f: result=subprocess.run(['dotnet','pack','Bosak.sln','-c','Release','--no-build','-o',str(r/'.guard-tmp/release1261-packages')],cwd=s,stdout=f,stderr=subprocess.STDOUT)
assert result.returncode==0
print('Candidate packages packed.',flush=True)
