"""Replay compact primitive-comparison evidence without launching .NET."""
from pathlib import Path
import collections,hashlib,importlib.util,io,json,re,zipfile,csv,subprocess
root=Path(__file__).resolve().parents[2];d=Path(__file__).resolve().parent
manifest=json.loads((d/'MANIFEST.json').read_text())
for name,expected in manifest.items():assert hashlib.sha256((d/name).read_bytes()).hexdigest()==expected,name
spec=importlib.util.spec_from_file_location('gate',root/'tools/xslt30_gate.py');gate=importlib.util.module_from_spec(spec);spec.loader.exec_module(gate)
policy,_,inputs,skips=gate.baseline();result=json.loads((d/'RESULTS.json').read_text())
assert result['success'] and result['solutionPassed']==26430 and result['languageServerPassed']==90
with zipfile.ZipFile(d/'EVIDENCE.zip') as z:
 read=lambda name:json.loads(z.read(name))
 run=read('standalone/RUN.json');plan=read('standalone/PLAN.json');qualified=read('standalone/GATE.json')
 assert run['sourceCommit']==result['qualifiedSourceCommit'] and run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
 assert qualified['success'] and qualified['counts']==policy['counts'] and qualified['sourceCorpusUnchanged'] and qualified['runtimeUnchanged']
 assert not any(qualified['corpusDifferences'].values())
 assert read('standalone/CORPUS-BEFORE.json')==inputs==read('standalone/CORPUS-AFTER.json')
 for name in ['stdout.log','stderr.log']:assert hashlib.sha256(z.read('standalone/'+name)).hexdigest()==run[name+'Sha256']
 rows=gate.reconcile(plan,run,z.read('standalone/stdout.log').decode('utf-8-sig'),skips,policy);assert len(rows)==14601
 retained=list(csv.DictReader(io.StringIO(z.read('standalone/outcomes.csv').decode('utf-8'))));assert [{k:str(v) for k,v in row.items()} for row in rows]==retained
 assert len(read('local/CHECKS.json'))==4 and all(c['exitCode']==0 for c in read('local/CHECKS.json'))
 text=z.read('local/UNIT.log').decode('utf-8-sig');assert sum(map(int,re.findall(r'Passed:\s+(\d+)',text)))==26430 and not re.search(r'Failed:\s+[1-9]',text)
 assert 'Passed:    90' in z.read('local/LSP.log').decode('utf-8-sig')
 baseline=read('local/BASELINE-TESTS.json');assert baseline['success'] and baseline['protectivePasses']==3 and baseline['bytesPerCall']==1008
 focused=z.read('local/focused-passed.log')
 assert 'Passed:    10' in focused.decode('utf-16' if focused.startswith(b'\xff\xfe') else 'utf-8-sig')
 baselineText=z.read('local/BASELINE-TESTS.log').decode('utf-8-sig');assert 'IntegerOperators_PreserveResultsWithoutTemporaryAllocations' in baselineText
 assert 'Actual:   100800000' in baselineText and 'Failed:     1' in baselineText and 'Passed:     3' in baselineText
 reference=json.loads((root/'docs/scalar-materialization/REVIEW.json').read_text());assert baseline['baselineNineHashes']==reference['engineHashes']
 for title,expected in [('QT3',(31397,0,424)),('QT4',(8941,0,1918))]:
  text=z.read('local/'+title+'.log').decode('utf-8-sig');assert tuple(int(re.search(r'^'+key+r':\s*(\d+)\s*$',text,re.M).group(1)) for key in ['Passed','Failed','Skipped'])==expected
  def parse(text):
   found={}
   for count,reason,body in re.findall(r'=== (\d+) skipped: (.*?) ===\r?\n(.*?)(?=^===|\Z)',text,re.M|re.S):
    names=[n for n in body.splitlines() if n.strip()];assert len(names)==int(count)
    for name in names:assert name not in found;found[name]=reason
   return found
  assert parse(z.read('local/'+title+'-SKIPS.txt').decode('utf-8-sig'))==parse((root/'docs/conformance/evidence'/f'{title.lower()}-skips.txt').read_text(encoding='utf-8-sig'))
 hashes=read('standalone/RUNTIME-HASHES.json');assert all(hashes[n]==sha for n,sha in result['candidateNineHashes'].items())
 for retained,path in [('VmEngine.cs.txt','src/Bosak.XPath.Runtime/Vm/VmEngine.cs'),('PrimitiveGeneralComparisonTests.cs.txt','tests/Bosak.XPath.Runtime.Tests/PrimitiveGeneralComparisonTests.cs')]:
  assert z.read('source/'+retained)==(root/path).read_bytes()
  assert subprocess.check_output(['git','show',run['sourceCommit']+':'+path],cwd=root).replace(b'\r\n',b'\n')==(root/path).read_bytes().replace(b'\r\n',b'\n')
 assert '<Version>1.81.10</Version>' in (root/'src/Directory.Build.props').read_text()
assert json.loads((d/'ADOPTION.json').read_text())['beforeImplementation']
print('Manifest, source binding, allocation regression, unit counts, all frozen skips and 14,601 standalone identities verified; no .NET launched.')
