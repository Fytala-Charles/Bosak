from pathlib import Path
import subprocess,json,hashlib,shutil,re
r=Path('D:/Development/Bosak');t=r/'.guard-tmp';s=t/'release1263-source';e=t/'release1263-candidate';rev='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f';oldrev='6a3372fc3b91e733b71a426cf40f26feaaeb3bcb'
assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=s,text=True).strip()==rev
assert not subprocess.check_output(['git','status','--porcelain'],cwd=s,text=True).strip()
assert not e.exists() and not (t/'release1263-packages').exists();e.mkdir();adapt=[]
for kind in ['broad','focused','metadata']:
 dest=e/kind;shutil.copytree(t/'release1262-candidate'/kind,dest,ignore=shutil.ignore_patterns('bin','obj','TestResults'))
 for f in dest.rglob('*'):
  if not f.is_file() or f.suffix not in ['.cs','.csproj']:continue
  raw=f.read_bytes();text=raw.decode('utf-8-sig');header='';body=text
  if f.suffix=='.cs' and 'generated' not in text[:200].lower():
   split=text.find('using ');assert split>0;header=text[:split];body=text[split:]
  changed=body.replace('1.26.2','1.26.3').replace(oldrev,rev)
  changed=re.sub(r'Version\(1,\s*26,\s*2,\s*0\)','Version(1,26,3,0)',changed)
  if header and changed!=body:
   versions=re.findall(r'Charles Korthout\s*\|\s*(\d+(?:\.\d+)+)\s*\|',header);assert versions
   parts=list(max(tuple(map(int,v.split('.'))) for v in versions));parts[-1]+=1;version='.'.join(map(str,parts));newline='\r\n' if '\r\n' in header else '\n'
   header=header.rstrip('\r\n')+newline+f'//                      | Charles Korthout | {version:<5} | 26-10-2026     | Verify exact-source unsigned 1.26.3 package candidate.                                    |'+newline+'// ==========================================================================================================================================================='+newline+newline
  f.write_bytes((header+changed).encode('utf-8'))
  adapt.append(dict(file=f.relative_to(e).as_posix(),fromSha256=hashlib.sha256(raw).hexdigest(),adaptedSha256=hashlib.sha256(f.read_bytes()).hexdigest(),change='Body version/revision/assembly identity only; preserve historical header rows and append bumped row when changed.'))
 if kind!='metadata':(dest/'NuGet.Config').write_text(f'<configuration><packageSources><clear/><add key="candidate" value="{t.as_posix()}/release1263-packages"/><add key="nuget.org" value="https://api.nuget.org/v3/index.json"/></packageSources><packageSourceMapping><clear/><packageSource key="candidate"><package pattern="Bosak.*"/></packageSource><packageSource key="nuget.org"><package pattern="*"/></packageSource></packageSourceMapping></configuration>',encoding='utf-8')
added=[]
for name,count in [('AbsIntegerPreservationTests',1323),('FloorCeilingIntegerPreservationTests',936),('FloorCeilingArgumentContractTests',802)]:
 src=s/'tests/Bosak.XPath.Standard.Tests'/(name+'.cs');dest=e/'focused'/src.name;assert not dest.exists();shutil.copyfile(src,dest)
 added.append(dict(file=src.name,source='tests/Bosak.XPath.Standard.Tests/'+src.name,sha256=hashlib.sha256(src.read_bytes()).hexdigest(),expectedCases=count,copy='Byte-identical pinned source; public APIs only.'))
plan=dict(date='2026-10-26',candidate='1.26.3',publicVersion='1.26.2',immutableSource=rev,sourceTree=subprocess.check_output(['git','rev-parse','HEAD^{tree}'],cwd=s,text=True).strip(),status='PREPARING_UNSIGNED_LOCAL_CANDIDATE',broadChecks=153,retainedFocusedChecks=8114,addedFocusedChecks=3061,totalFocusedChecks=11175,expectedSourceTests=21798,expectedLspTests=84,packageSourceMapping='Bosak.* exclusively local candidate feed; external dependencies nuget.org',freshSeparateCaches=True,addedSourceFiles=added,consumerAdaptations=adapt,publicationPerformed=False,githubCiChecked=False,braid='Previously captured local 1.26.2 automated adoption report; uncommitted/no current-adoption CI or new native acceptance at capture; no 1.26.3 adoption inferred.')
for name in ['source-plan.json','consumer-plan.json']:(e/name).write_text(json.dumps(plan,indent=2)+'\n',encoding='utf-8')
for name in ['build-release1262-candidate.py','test-release1262-packages.ps1','probe-release1262-nuget.py','verify-release1262-gates.py','audit-release1262-candidate.py','preflight-release1262-all.py']:
 text=(t/name).read_text(encoding='utf-8').replace('1262','1263').replace('1.26.2','1.26.3').replace('1.26.1','1.26.2').replace(oldrev,rev).replace('18737','21798').replace('8114','11175').replace('3913 retained public checks +4201 new rounding cases','8114 retained public checks +3061 new integer-preservation/contract cases').replace('public1261Present','public1262Present')
 (t/name.replace('1262','1263')).write_text(text,encoding='utf-8')
print('Pinned 1.26.3 candidate: 21,798 source +84 LSP; 153 broad /11,175 focused checks; 3,061 added source cases.')
