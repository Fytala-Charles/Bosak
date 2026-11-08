from pathlib import Path
import xml.etree.ElementTree as X,json,hashlib,subprocess,collections
P=Path(__file__).resolve().parent;R=P.parents[2];ns={'t':'http://www.w3.org/2010/09/qt-fots-catalog'}
def save(n,v):(P/n).write_text(json.dumps(v,indent=2,ensure_ascii=True)+'\n',encoding='utf-8')
rows=[];hashes={};counts={};negative=[]
for suite in ['qt3','qt4']:
 root=R/'tests'/(suite+'tests');total=0
 for ref in X.parse(root/'catalog.xml').getroot().findall('t:test-set',ns):
  path=root/ref.get('file');hashes[path.relative_to(R).as_posix()]=hashlib.sha256(path.read_bytes()).hexdigest();doc=X.parse(path).getroot()
  for case in doc.findall('t:test-case',ns):
   total+=1;deps=doc.findall('t:dependency',ns)+case.findall('t:dependency',ns)
   neg=[d for d in deps if d.get('satisfied') in ['false','0']]
   for d in neg:negative.append(dict(suite=suite,name=case.get('name'),type=d.get('type'),value=d.get('value')))
   if any(d.get('type') in ['language','calendar','unicode-normalization-form'] for d in neg):
    rows.append(dict(Suite=suite,SuiteRoot=root.relative_to(R).as_posix(),SetFile=path.relative_to(R).as_posix(),Name=case.get('name'),Expression=case.find('t:test',ns).text,assertion=X.tostring(case.find('t:result',ns),encoding='unicode'),Dependencies=[dict(Type=d.get('type'),Value=d.get('value'),Satisfied=d.get('satisfied','true') not in ['false','0']) for d in deps]))
 counts[suite]=total
save('inventory.json',dict(scannedCases=counts,negativeDeclarations=negative,fixtureHashes=hashes));save('requests.json',rows)
driver=R/'.guard-tmp/k04-parse-fix-driver/bin/Release/net10.0/Probe.dll';assert driver.exists()
receipts=[]
for row in rows:
 name=row['Suite']+'-'+row['Name'];request=P/(name+'-request.json');save(request.name,row)
 command=['dotnet',str(driver),'execute',str(request)]
 result=subprocess.run(command,cwd=R,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=20)
 (P/(name+'-output.txt')).write_bytes(result.stdout);assert result.returncode==0
 outcome=json.loads(result.stdout.decode('utf-8-sig').splitlines()[-1]);receipts.append(dict(name=name,command=command,exitCode=result.returncode,watchdogSeconds=20,result=outcome))
result=subprocess.run(['dotnet',str(driver),'filters',str(P/'requests.json'),str(P/'filters.jsonl')],cwd=R,capture_output=True,timeout=20);assert result.returncode==0,result.stderr
save('execution.json',receipts)
identities={}
for path in [driver,*list((R/'tests/Bosak.XPath.Conformance/bin/Release/net10.0').glob('Bosak*.dll')),*list((R/'tests/Bosak.XPath.Conformance40/bin/Release/net10.0').glob('Bosak*.dll'))]:identities[path.relative_to(R).as_posix()]=hashlib.sha256(path.read_bytes()).hexdigest()
save('binary-identities.json',identities)
save('RESULTS.json',dict(date='2026-10-27',slice='K04-C13',sourceBase=subprocess.check_output(['git','rev-parse','HEAD'],cwd=R).decode().strip(),cases=len(rows),outcomes=dict(collections.Counter(r['result']['kind'] for r in receipts)),engineChanged=False,baselineChanged=False,validationInherited=dict(solutionPassed=22198,qt3=[31162,0,659],qt4=[8587,0,1892])))
print(json.dumps([r['result'] for r in receipts],indent=2))
