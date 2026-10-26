from pathlib import Path
import subprocess,json,hashlib,shutil,re
r=Path('D:/Development/Bosak');t=r/'.guard-tmp';s=t/'release1262-source';e=t/'release1262-candidate';rev='6a3372fc3b91e733b71a426cf40f26feaaeb3bcb';oldrev='65417cdb3bff184ca9ff1f624582f55ca4681db9'
assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=s,text=True).strip()==rev
assert not subprocess.check_output(['git','status','--porcelain'],cwd=s,text=True).strip()
assert not e.exists() and not (t/'release1262-packages').exists();e.mkdir()
adapt=[]
for kind in ['broad','focused','metadata']:
 dest=e/kind;shutil.copytree(t/'release1261-candidate'/kind,dest,ignore=shutil.ignore_patterns('bin','obj','TestResults'))
 for f in dest.rglob('*'):
  if f.is_file() and f.suffix in ['.cs','.csproj']:
   raw=f.read_bytes();text=raw.decode('utf-8-sig');changed=text.replace('1.26.1','1.26.2').replace(oldrev,rev)
   changed=re.sub(r'Version\(1,\s*26,\s*1,\s*0\)','Version(1,26,2,0)',changed)
   if changed!=text and f.suffix=='.cs' and 'generated' not in text[:200].lower():
    split=changed.find('using ');assert split>0
    header=changed[:split];versions=re.findall(r'Charles Korthout\s*\|\s*(\d+(?:\.\d+)+)\s*\|',header);assert versions
    parts=versions[-1].split('.');parts[-1]=str(int(parts[-1])+1);version='.'.join(parts)
    pos=header.rfind('// =================================================================');assert pos>0
    newline='\r\n' if '\r\n' in header else '\n'
    changed=changed[:pos]+f'//                      | Charles Korthout | {version:<5} | 26-10-2026     | Verify exact-source unsigned 1.26.2 package candidate.                                    |'+newline+changed[pos:]
   f.write_bytes(changed.encode('utf-8'))
   adapt.append(dict(file=f.relative_to(e).as_posix(),fromSha256=hashlib.sha256(raw).hexdigest(),adaptedSha256=hashlib.sha256(f.read_bytes()).hexdigest(),change='Version/revision/assembly identity only, plus dated header row when changed.'))
 if kind!='metadata':
  (dest/'NuGet.Config').write_text(f'<configuration><packageSources><clear/><add key="candidate" value="{t.as_posix()}/release1262-packages"/><add key="nuget.org" value="https://api.nuget.org/v3/index.json"/></packageSources><packageSourceMapping><clear/><packageSource key="candidate"><package pattern="Bosak.*"/></packageSource><packageSource key="nuget.org"><package pattern="*"/></packageSource></packageSourceMapping></configuration>',encoding='utf-8')
added=[]
for name,count in [('RoundingPrecisionBoundaryTests',1321),('RoundingKernelTests',2880)]:
 src=s/'tests/Bosak.XPath.Standard.Tests'/(name+'.cs');dest=e/'focused'/src.name;assert not dest.exists();shutil.copyfile(src,dest)
 added.append(dict(file=src.name,source='tests/Bosak.XPath.Standard.Tests/'+src.name,sha256=hashlib.sha256(src.read_bytes()).hexdigest(),expectedCases=count,copy='Byte-identical pinned source; public APIs only.'))
plan=dict(date='2026-10-26',candidate='1.26.2',publicVersion='1.26.1',immutableSource=rev,sourceTree=subprocess.check_output(['git','rev-parse','HEAD^{tree}'],cwd=s,text=True).strip(),status='PREPARING_UNSIGNED_LOCAL_CANDIDATE',broadChecks=153,retainedFocusedChecks=3913,addedFocusedChecks=4201,totalFocusedChecks=8114,expectedSourceTests=18737,expectedLspTests=84,packageSourceMapping='Bosak.* exclusively local candidate feed; external dependencies nuget.org',freshSeparateCaches=True,addedSourceFiles=added,consumerAdaptations=adapt,adaptation='Only candidate version/revision/assembly identity retargeting in retained consumer source; new tests copied without edits.',publicationPerformed=False,githubCiChecked=False,braid='Owner reports smoke finished; outcome not supplied')
for name in ['source-plan.json','consumer-plan.json']:(e/name).write_text(json.dumps(plan,indent=2)+'\n',encoding='utf-8')
for name in ['build-release1261-candidate.py','test-release1261-packages.ps1','probe-release1261-nuget.py','verify-release1261-gates.py','audit-release1261-candidate.py','preflight-release1261-all.py']:
 text=(t/name).read_text(encoding='utf-8').replace('1261','1262').replace('1.26.1','1.26.2').replace('1.26.0','1.26.1').replace(oldrev,rev).replace('3913','8114').replace('1493 retained public checks +2420 new atomic/numeric/rounding cases','3913 retained public checks +4201 new rounding cases')
 # Numbered preflight keys describe the public/candidate version pair.
 text=text.replace('public1260Present','public1261Present')
 (t/name.replace('1261','1262')).write_text(text,encoding='utf-8')
print('Pinned candidate: 18,737 source +84 LSP; 153 broad /8,114 focused; 4,201 added package cases.')
