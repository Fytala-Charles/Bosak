from pathlib import Path
import json,re,hashlib
r=Path('D:/Development/Bosak/.guard-tmp');e=r/'release1262-candidate';plan=json.loads((e/'consumer-plan.json').read_text());rev=plan['immutableSource'];adapt=[]
for kind in ['broad','focused','metadata']:
 for original in (r/'release1261-candidate'/kind).rglob('*'):
  if not original.is_file() or original.suffix not in ['.cs','.csproj'] or any(x in ['bin','obj','TestResults'] for x in original.relative_to(r/'release1261-candidate'/kind).parts):continue
  raw=original.read_bytes();text=raw.decode('utf-8-sig');header='';body=text
  if original.suffix=='.cs' and 'generated' not in text[:200].lower():
   split=text.find('using ');assert split>0;header=text[:split];body=text[split:]
  changed=body.replace('1.26.1','1.26.2').replace('65417cdb3bff184ca9ff1f624582f55ca4681db9',rev)
  changed=re.sub(r'Version\(1,\s*26,\s*1,\s*0\)','Version(1,26,2,0)',changed)
  if header and changed!=body:
   versions=re.findall(r'Charles Korthout\s*\|\s*(\d+(?:\.\d+)+)\s*\|',header);assert versions
   parts=list(max((tuple(map(int,v.split('.'))) for v in versions)));parts[-1]+=1;version='.'.join(map(str,parts))
   newline='\r\n' if '\r\n' in header else '\n'
   header=header.rstrip('\r\n')+newline+f'//                      | Charles Korthout | {version:<5} | 26-10-2026     | Verify exact-source unsigned 1.26.2 package candidate.                                    |'+newline+'// ==========================================================================================================================================================='+newline+newline
  dest=e/kind/original.relative_to(r/'release1261-candidate'/kind);dest.write_bytes((header+changed).encode('utf-8'))
  adapt.append(dict(file=dest.relative_to(e).as_posix(),fromSha256=hashlib.sha256(raw).hexdigest(),adaptedSha256=hashlib.sha256(dest.read_bytes()).hexdigest(),change='Body version/revision/assembly identity only; preserve prior history and append bumped row when body changes.'))
plan['consumerAdaptations']=adapt
for f in ['source-plan.json','consumer-plan.json']:(e/f).write_text(json.dumps(plan,indent=2)+'\n',encoding='utf-8')
print('Retained consumer history preserved; bumped dated history rows appended.')
