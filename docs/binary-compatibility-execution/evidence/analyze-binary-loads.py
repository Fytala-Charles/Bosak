from pathlib import Path
import json,hashlib
r=Path.cwd();base=r/'.guard-tmp/binary-results';out=r/'docs/binary-compatibility-execution/evidence'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
summary={}
for directory in sorted(base.iterdir()):
 if not directory.is_dir():continue
 rows=[json.loads(line) for p in (directory/'loads').glob('*.jsonl') for line in p.read_text().splitlines()]
 engines=[row for row in rows if row['fullName'].startswith(('Bosak.XPath.','Bosak.XQuery,','Bosak.Xslt,'))]
 mode='replacement' if '-replacement-' in directory.name else 'baseline';consumer='xmldb' if directory.name.startswith('xmldb') else 'braid'
 root=r/(f'.guard-tmp/binary-identity-{mode}' if directory.name.endswith('-identity') else f'.guard-tmp/binary-{consumer}-{mode}')
 for row in rows:
  p=Path(row['location']);assert p.is_relative_to(root),row
  assert p.exists() and sha(p)==row['sha256'],row['location']
 assert engines,directory.name
 for row in engines:assert f'Version={"1.34.1.0" if mode=="replacement" else "1.26.2.0"}' in row['fullName'],row
 workers={row['processId'] for row in rows if row['processPath'] and Path(row['processPath']).name=='Bosak.Braid.Desktop.exe'}
 summary[directory.name]={'assemblyLoadRows':len(rows),'engineLoadRows':len(engines),'engineAssemblies':sorted({row['fullName'].split(',')[0] for row in engines}),'workerProcessesObserved':len(workers),'allLoadsWithinFrozenLayout':True,'allLoadedHashesMatchLayout':True,'expectedEngineAssemblyVersionsOnly':True}
(out/'assembly-load-verification.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8',newline='\n')
print(json.dumps(summary,indent=2))
