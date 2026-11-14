"""Verify sealed date-span evidence offline; never launch .NET."""
from pathlib import Path
import json,hashlib,zipfile,csv,io,re,subprocess,importlib.util,xml.etree.ElementTree as ET
D=Path(__file__).resolve().parent;R=D.parents[1]
for name,h in json.loads((D/'MANIFEST.json').read_bytes()).items():assert hashlib.sha256((D/name).read_bytes()).hexdigest()==h,name
result=json.loads((D/'RESULTS.json').read_bytes());assert result['success'] and result['solutionPassed']==26443 and result['languageServerPassed']==90
spec=importlib.util.spec_from_file_location('gate',R/'tools/xslt30_gate.py');g=importlib.util.module_from_spec(spec);spec.loader.exec_module(g);policy,_,inputs,skips=g.baseline()
with zipfile.ZipFile(D/'EVIDENCE.zip') as z:
 read=lambda n:json.loads(z.read(n))
 run=read('standalone/RUN.json');gate=read('standalone/GATE.json');assert gate['success'] and run['success'] and run['sourceCommit']==result['qualifiedSourceCommit'];assert run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
 assert read('standalone/CORPUS-BEFORE.json')==inputs==read('standalone/CORPUS-AFTER.json');assert not any(gate['corpusDifferences'].values()) and gate['sourceCorpusUnchanged'] and gate['runtimeUnchanged']
 for name in ['stdout.log','stderr.log']:assert hashlib.sha256(z.read('standalone/'+name)).hexdigest()==run[name+'Sha256']
 rows=g.reconcile(read('standalone/PLAN.json'),run,z.read('standalone/stdout.log').decode('utf-8-sig'),skips,policy);assert len(rows)==14601
 assert [{k:str(v) for k,v in row.items()} for row in rows]==list(csv.DictReader(io.StringIO(z.read('standalone/outcomes.csv').decode('utf-8'))))
 ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
 tests=[ET.fromstring(z.read(n)).find('.//t:Counters',ns) for n in z.namelist() if n.startswith('local/unrestricted-results/') and n.endswith('.trx')]
 assert len(tests)==10 and all(t.get('failed')=='0' for t in tests) and sum(int(t.get('passed')) for t in tests)==26443
 assert ET.fromstring(z.read('local/lsp.trx')).find('.//t:Counters',ns).get('passed')=='90'
 def parse(text):
  rows={}
  for count,reason,body in re.findall(r'=== (\d+) skipped: (.*?) ===\r?\n(.*?)(?=^===|\Z)',text,re.M|re.S):
   names=[n for n in body.splitlines() if n.strip()];assert len(names)==int(count)
   for name in names:assert name not in rows;rows[name]=reason
  return rows
 for gateName,count in [('QT3',424),('QT4',1918)]:
  actual=parse(z.read('local/'+gateName+'-SKIPS.txt').decode('utf-8-sig'));assert len(actual)==count and actual==parse((R/('docs/conformance/evidence/'+gateName.lower()+'-skips.txt')).read_text(encoding='utf-8-sig'))
 a=read('diagnostic/RESULTS.json');assert a['engine']=='1.81.13.0' and a['parityCases']==3164
 expected={'date':(800,704),'utc':(856,736),'offset':(1072,736),'year-zero':(856,736),'extended':(856,736),'negative-year':(856,736),'wrong-shape':(0,0),'datetime-probe':(0,0),'invalid-month':(800,704),'invalid-day':(800,704),'invalid-zone':(936,736),'leading-plus':(736,704)}
 assert len(a['rows'])==24
 for row in a['rows']:assert row['bytesPerCall']==expected[row['label']][row['variant']=='engine'] and row['allocatedBytes']==row['bytesPerCall']*100000
 hashes=read('standalone/RUNTIME-HASHES.json');assert all(hashes[n]==h for n,h in result['candidateNineHashes'].items())
 for name,h in result['sourceHashes'].items():
  data=z.read('source/'+name);assert hashlib.sha256(data).hexdigest()==h
  assert data.replace(b'\r\n',b'\n')==(R/name).read_bytes().replace(b'\r\n',b'\n')==subprocess.check_output(['git','show',run['sourceCommit']+':'+name],cwd=R).replace(b'\r\n',b'\n')
 assert not read('sandbox-attempt/GATE.json')['success'] and not read('sandbox-attempt/RUN.json')['success']
print('Verified all 14,601 outcomes, frozen skip names/reasons, full tests, date allocations, source identities and retained sandbox failure; no .NET launched.')
