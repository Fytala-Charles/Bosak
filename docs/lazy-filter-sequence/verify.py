"""Replay sealed qualification receipts offline without launching .NET."""
from pathlib import Path
import json,hashlib,zipfile,subprocess,csv,io,re,importlib.util,xml.etree.ElementTree as ET
D=Path(__file__).resolve().parent;R=D.parents[1]
for n,h in json.loads((D/'MANIFEST.json').read_bytes()).items():assert hashlib.sha256((D/n).read_bytes()).hexdigest()==h,n
result=json.loads((D/'RESULTS.json').read_bytes());adopt=json.loads((D/'ADOPTION.json').read_bytes());assert result['success'] and result['solutionPassed']==26453 and result['languageServerPassed']==90 and result['focusedPassed']==10 and adopt['recordedBeforeImplementation']
spec=importlib.util.spec_from_file_location('gate',R/'tools/xslt30_gate.py');g=importlib.util.module_from_spec(spec);spec.loader.exec_module(g);policy,_,inputs,skips=g.baseline()
with zipfile.ZipFile(D/'EVIDENCE.zip') as z:
 read=lambda n:json.loads(z.read(n));run=read('standalone/RUN.json');gate=read('standalone/GATE.json');plan=read('standalone/PLAN.json')
 assert gate['success'] and run['success'] and run['sourceCommit']==result['qualifiedSourceCommit'];assert run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
 assert read('standalone/CORPUS-BEFORE.json')==inputs==read('standalone/CORPUS-AFTER.json');assert not any(gate['corpusDifferences'].values()) and gate['sourceCorpusUnchanged'] and gate['runtimeUnchanged']
 for n in ['stdout.log','stderr.log']:assert hashlib.sha256(z.read('standalone/'+n)).hexdigest()==run[n+'Sha256']
 rows=g.reconcile(plan,run,z.read('standalone/stdout.log').decode('utf-8-sig'),skips,policy);assert len(rows)==14601
 assert [{k:str(v) for k,v in r.items()} for r in rows]==list(csv.DictReader(io.StringIO(z.read('standalone/outcomes.csv').decode('utf-8'))))
 ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
 tests=[ET.fromstring(z.read(n)).find('.//t:Counters',ns) for n in z.namelist() if n.startswith('local/solution-results/') and n.endswith('.trx')]
 assert len(tests)==10 and all(t.get('failed')=='0' for t in tests) and sum(int(t.get('passed')) for t in tests)==26453
 assert ET.fromstring(z.read('local/focused.trx')).find('.//t:Counters',ns).get('passed')=='10'
 assert ET.fromstring(z.read('preparation/zero-match.trx')).find('.//t:Counters',ns).get('total')=='0'
 assert re.search(r'Passed:\s+90\b',z.read('local/LSP.log').decode('utf-8-sig'))
 checks=read('local/CHECKS.json');assert len(checks)==4 and all(c['exitCode']==0 for c in checks)
 def parse(text):
  result={}
  for count,reason,body in re.findall(r'=== (\d+) skipped: (.*?) ===\r?\n(.*?)(?=^===|\Z)',text,re.M|re.S):
   names=[n for n in body.splitlines() if n.strip()];assert len(names)==int(count)
   for name in names:assert name not in result;result[name]=reason
  return result
 for label,count in [('QT3',424),('QT4',1918)]:
  actual=parse(z.read('local/'+label+'-SKIPS.txt').decode('utf-8-sig'));assert len(actual)==count and actual==parse((R/('docs/conformance/evidence/'+label.lower()+'-skips.txt')).read_text(encoding='utf-8-sig'))
 cal=read('local/CALIBRATION.json');review=read('local/CALIBRATION-REVIEW.json');budgets=adopt['allocationRegressionBudgetsBytesPerCall'];assert review['budgets']==budgets and cal['success'] and cal['sameCompatibleWorker']
 assert len({r['workerSha256'] for r in cal['runs']})==1
 for item in cal['runs']:
  report=item['report'];assert report['success'] and report['iterations']==100000 and report['warmup']==10000 and len(report['rows'])==5
  rows={r['name']:r['bytes'] for r in report['rows']};assert set(rows)==set(budgets)
  verdict={n:amount<=budgets[n]*100000 for n,amount in rows.items()};assert verdict==item['budgetVerdicts'];assert all(verdict.values()) if item['label']=='candidate' else not any(verdict.values())
 for name,h in review['sourceHashes'].items():assert hashlib.sha256(z.read('compatible-worker/'+name)).hexdigest()==h
 sem=read('semantic/RESULTS.json');assert sem['success'] and sem['sameCompatibleWorker'] and not sem['allocationLoopsExecuted'] and len({r['workerSha256'] for r in sem['runs']})==1
 for item in sem['runs']:assert item['exitCode']==0 and item['result']['success'] and item['result']['comparisonCases']==139 and len(item['result']['checks'])==14 and not item['result']['rows']
 assert hashlib.sha256(z.read('semantic/Prototype.cs')).hexdigest()==read('semantic/REVIEW.json')['baselinePrototypeSourceSha256']
 hashes=read('standalone/RUNTIME-HASHES.json');assert all(hashes[n]==h for n,h in result['candidateNineHashes'].items())
 for n,h in result['sourceHashes'].items():
  data=z.read('source/'+n);assert hashlib.sha256(data).hexdigest()==h
  assert data.replace(b'\r\n',b'\n')==(R/n).read_bytes().replace(b'\r\n',b'\n')==subprocess.check_output(['git','show',run['sourceCommit']+':'+n],cwd=R).replace(b'\r\n',b'\n')
print('Verified all 14,601 outcomes, exact frozen exclusions, full/focused tests, compatible allocation/semantic controls and qualified source hashes; no .NET launched.')
