import importlib.util,json,hashlib,subprocess,time
from pathlib import Path
p=Path(__file__).resolve().parent;r=Path('D:/Development/Bosak/.worktrees/Bosak.ConversionProbe');out=p/'bin/Release/net10.0';review=json.loads((p/'REVIEW.json').read_text())
def hashes():
 result={n:hashlib.sha256((out/n).read_bytes()).hexdigest() for n in review['engineHashes']};assert result==review['engineHashes'];return result
before=hashes();s=importlib.util.spec_from_file_location('bounded',r/'docs/xslt30-qualification/run.py');m=importlib.util.module_from_spec(s);s.loader.exec_module(m)
command=['C:/Program Files/dotnet/dotnet.exe','exec','--fx-version','10.0.11','--roll-forward','Disable',str(out/'Bosak.XPath.Runtime.Tests.dll'),str(p/'RESULT.json')]
assert not (p/'RUN.json').exists()
start=time.monotonic();job=m.BoundedJob(review['memoryLimitBytes']);timeout=False
try:
 with (p/'stdout.log').open('wb') as stdout,(p/'stderr.log').open('wb') as stderr,m.process_error_mode():
  process=subprocess.Popen(command,stdout=stdout,stderr=stderr,creationflags=subprocess.CREATE_NO_WINDOW);job.assign(process)
  try:code=process.wait(timeout=review['watchdogSeconds'])
  except subprocess.TimeoutExpired:timeout=True;job.terminate();code=process.wait()
finally:job.close()
receipt={'command':command,'exitCode':code,'timeout':timeout,'elapsedSeconds':time.monotonic()-start,'memoryLimitBytes':review['memoryLimitBytes'],'watchdogSeconds':review['watchdogSeconds'],'engineHashesBefore':before,'engineHashesAfter':hashes(),'probeSourceSha256':hashlib.sha256((p/'Probe.cs').read_bytes()).hexdigest(),'probeBinarySha256':hashlib.sha256((out/'Bosak.XPath.Runtime.Tests.dll').read_bytes()).hexdigest()}
(p/'RUN.json').write_text(json.dumps(receipt,indent=2));print(json.dumps(receipt,indent=2));print((p/'stderr.log').read_text());assert code==0 and not timeout
