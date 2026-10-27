from pathlib import Path
import subprocess,json,time,os,re,xml.etree.ElementTree as X
A=Path(__file__).resolve().parent;E=A.parent/'evidence';R=A.parents[3];receipts=[]
def run(command,name,env=None):
 start=time.monotonic();log=A/(name+'.log')
 with log.open('wb') as f:
  try:p=subprocess.run(command,cwd=R,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=30);code=p.returncode;timeout=False
  except subprocess.TimeoutExpired:code=None;timeout=True
 row=dict(command=command,log=log.name,exitCode=code,timeout=timeout,watchdogSeconds=30,wallSeconds=time.monotonic()-start);receipts.append(row);assert code==0 and not timeout;return row,log.read_text(encoding='utf-8-sig')
for suite in ['qt3','qt4']:
 for kind in ['filter','projection']:
  source=E/('filter-controls.json' if kind=='filter' else 'projected-controls.json');output=A/(suite+'-'+kind+'.jsonl');command=['dotnet',str(R/'.guard-tmp/k04-calendar-filter-driver/bin/Release/net10.0/Probe.dll'),suite,str(source),str(output),'synthetic','current'];run(command,suite+'-'+kind)
  actual=[json.loads(s) for s in output.read_text(encoding='utf-8-sig').splitlines()];expected=[json.loads(s) for s in (E/(suite+('-prototype.jsonl' if kind=='filter' else '-projected.jsonl'))).read_text(encoding='utf-8-sig').splitlines()];assert actual==expected
for row in json.loads((E/'requests.json').read_text()):
 suite=row['Suite'];project='Bosak.XPath.Conformance' if suite=='qt3' else 'Bosak.XPath.Conformance40';setname=X.parse(R/row['SetFile']).getroot().get('name');env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='0';env.pop('BOSAK_QT3_DUMP_SKIPS',None);env.pop('BOSAK_QT4_DUMP_SKIPS',None);receipt,text=run(['dotnet',str(R/'tests'/project/'bin/Release/net10.0'/(project+'.dll')),row['SuiteRoot'],setname,row['Name']],suite+'-'+row['Name'],env);match=re.search(r'Total:\s+(\d+)\s+Passed:\s+(\d+)\s+Failed:\s+(\d+)\s+Skipped:\s+(\d+)',text);assert list(map(int,match.groups()))==[1,1,0,0]
for scope,counts in [('fn-format-date',[232,208,6,18]),('fn-format-dateTime',[94,77,2,15])]:
 env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='0';env['BOSAK_QT4_DUMP_SKIPS']=str(A/(scope+'-skips.txt'));receipt,text=run(['dotnet',str(R/'tests/Bosak.XPath.Conformance40/bin/Release/net10.0/Bosak.XPath.Conformance40.dll'),'tests/qt4tests',scope],scope,env);match=re.search(r'Total:\s+(\d+)\s+Passed:\s+(\d+)\s+Failed:\s+(\d+)\s+Skipped:\s+(\d+)',text);assert list(map(int,match.groups()))==counts
(A/'check-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n');print('Normal364controls/40projections,four originals and both exploratory scopes match approval.')
