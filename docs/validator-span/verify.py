from pathlib import Path
import json,hashlib,zipfile,re,importlib.util,subprocess,csv,io
r=Path(__file__).resolve().parents[2];d=Path(__file__).resolve().parent
for n,sha in json.loads((d/'MANIFEST.json').read_text()).items():assert hashlib.sha256((d/n).read_bytes()).hexdigest()==sha,n
result=json.loads((d/'RESULTS.json').read_text());adopt=json.loads((d/'ADOPTION.json').read_text());assert result['success'] and result['solutionPassed']==26440 and result['languageServerPassed']==90 and adopt['beforeImplementation'] and len(adopt['frozenExistingAtomicVocabulary'])==47
spec=importlib.util.spec_from_file_location('gate',r/'tools/xslt30_gate.py');g=importlib.util.module_from_spec(spec);spec.loader.exec_module(g);policy,_,inputs,skips=g.baseline()
with zipfile.ZipFile(d/'EVIDENCE.zip') as z:
 read=lambda n:json.loads(z.read(n));run=read('standalone/RUN.json');plan=read('standalone/PLAN.json');gate=read('standalone/GATE.json')
 assert gate['success'] and not any(gate['corpusDifferences'].values()) and gate['sourceCorpusUnchanged'] and gate['runtimeUnchanged']
 assert run['sourceCommit']==result['qualifiedSourceCommit'] and run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
 assert read('standalone/CORPUS-BEFORE.json')==inputs==read('standalone/CORPUS-AFTER.json')
 for n in ['stdout.log','stderr.log']:assert hashlib.sha256(z.read('standalone/'+n)).hexdigest()==run[n+'Sha256']
 rows=g.reconcile(plan,run,z.read('standalone/stdout.log').decode('utf-8-sig'),skips,policy);assert len(rows)==14601
 retained=list(csv.DictReader(io.StringIO(z.read('standalone/outcomes.csv').decode('utf-8'))));assert [{k:str(v) for k,v in row.items()} for row in rows]==retained
 checks=read('local/CHECKS.json');assert len(checks)==4 and all(c['exitCode']==0 for c in checks)
 text=z.read('local/UNIT.log').decode('utf-8-sig');assert sum(map(int,re.findall(r'Passed:\s+(\d+)',text)))==26440 and not re.search(r'Failed:\s+[1-9]',text)
 assert 'Passed:    90' in z.read('local/LSP.log').decode('utf-8-sig')
 focused=z.read('local/focused.log');assert 'Passed:    16' in focused.decode('utf-16' if focused.startswith(b'\xff\xfe') else 'utf-8-sig')
 for name,sha in result['sourceHashes'].items():
  data=z.read('source/'+name);assert hashlib.sha256(data).hexdigest()==sha;assert data.replace(b'\r\n',b'\n')==(r/name).read_bytes().replace(b'\r\n',b'\n')
  assert subprocess.check_output(['git','show',run['sourceCommit']+':'+name],cwd=r).replace(b'\r\n',b'\n')==data.replace(b'\r\n',b'\n')
 def parse(text):
  found={}
  for count,reason,body in re.findall(r'=== (\d+) skipped: (.*?) ===\r?\n(.*?)(?=^===|\Z)',text,re.M|re.S):
   names=[n for n in body.splitlines() if n.strip()];assert len(names)==int(count)
   for name in names:assert name not in found;found[name]=reason
  return found
 for title,want in [('QT3',(31397,0,424)),('QT4',(8941,0,1918))]:
  text=z.read('local/'+title+'.log').decode('utf-8-sig');assert tuple(int(re.search(r'^'+key+r':\s*(\d+)\s*$',text,re.M).group(1)) for key in ['Passed','Failed','Skipped'])==want
  assert parse(z.read('local/'+title+'-SKIPS.txt').decode('utf-8-sig'))==parse((r/'docs/conformance/evidence'/f'{title.lower()}-skips.txt').read_text(encoding='utf-8-sig'))
 assert hashlib.sha256(z.read('local/OLD-CLASSIFIER.txt').replace(b'\r\n',b'\n')).hexdigest()==adopt['previousHelperSourceSha256']
 cal=read('local/CALIBRATION.json');assert cal['success'] and len({x['workerSha256'] for x in cal['runs']})==1
 for item in cal['runs']:
  assert item['exitCode']==0 and item['watchdogSeconds']==60 and item['managedHeapCapBytes']==268435456
  measured={x['name']:x['bytes'] for x in item['report']['rows']}
  if item['mode']=='matching':assert all(v==(4000000 if item['label']=='baseline' and k.startswith('convert/') else 0) for k,v in measured.items())
  else:
   expected={'xs:integer':40,'xs:string':40,'xs:boolean':40,'xs:decimal':40,'xs:double':40,'xs:int':32,'xs:long':32,'xs:INTEGER':88,'  xs:integer  ':88,'xs:integer?':88,'xs:anyAtomicType':104,'element(a)':40}
   if item['label']=='candidate':expected={k:{'xs:INTEGER':48,'  xs:integer  ':48,'xs:integer?':48,'xs:anyAtomicType':56,'element(a)':40}.get(k,0) for k in expected}
   assert measured=={'validate/'+k:v*100000 for k,v in expected.items()}
 hashes=read('standalone/RUNTIME-HASHES.json');assert all(hashes[n]==sha for n,sha in result['candidateNineHashes'].items())
print('Frozen vocabulary/source, old/new allocations, full tests, every skip and all14,601 standalone identities verified; no .NET launched.')
