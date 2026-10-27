from pathlib import Path
import subprocess,os,time,re,json,sys
E=Path(__file__).resolve().parent;phase=sys.argv[1];log=E/('format-integer-'+phase+'.log');command=['dotnet','tests/Bosak.XPath.Conformance40/bin/Release/net10.0/Bosak.XPath.Conformance40.dll','tests/qt4tests','fn-format-integer'];env=dict(os.environ);env['BOSAK_QT4_GATE_ONLY']='0';env['BOSAK_QT4_DUMP_SKIPS']=str(E/('format-integer-'+phase+'-skips.txt'));start=time.monotonic()
with log.open('wb') as f:
 try:p=subprocess.run(command,env=env,stdout=f,stderr=subprocess.STDOUT,timeout=60);code=p.returncode;timeout=False
 except subprocess.TimeoutExpired:code=None;timeout=True
s=log.read_text(encoding='utf-8-sig');match=re.search(r'Total:\s+(\d+)\s+Passed:\s+(\d+)\s+Failed:\s+(\d+)\s+Skipped:\s+(\d+)',s);totals=list(map(int,match.groups())) if match else None
receipt=dict(command=command,exitCode=code,timeout=timeout,watchdogSeconds=60,wallSeconds=time.monotonic()-start,totals=totals,log=log.name,mode='Normal exploratory set;exit0does not imply zero exploratory failures')
(E/('format-integer-'+phase+'-receipt.json')).write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8');print(json.dumps(receipt))
