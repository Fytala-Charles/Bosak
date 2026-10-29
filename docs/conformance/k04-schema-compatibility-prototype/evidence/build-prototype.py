"""Build isolated projects, preserving true exit status and a bounded raw receipt."""
from pathlib import Path
import subprocess,json,time,sys,hashlib
E=Path(__file__).resolve().parent;R=E.parents[3]
D=R/'.guard-tmp/k04-schema-compatibility-prototype'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
sources={name:sha(D/name) for name in ['Contracts.cs','Model.cs','Program.cs','Contracts.csproj','Prototype.csproj']}
command=['dotnet','build',str(D/'Prototype.csproj'),'-c','Release','--nologo'];start=time.monotonic()
with (E/'final-build.log').open('wb') as output:
 try:code=subprocess.run(command,cwd=R,stdout=output,stderr=subprocess.STDOUT,timeout=45).returncode;timeout=False
 except subprocess.TimeoutExpired:code=None;timeout=True
assert all(sha(D/name)==h for name,h in sources.items()),'Source changed during build'
binaries={name:sha(D/'bin/Release/net10.0'/name) for name in ['C44R2.Prototype.dll','C44R2.SchemaContracts.dll','Bosak.XPath.Core.dll']} if code==0 else {}
(E/'build-receipt.json').write_text(json.dumps(dict(command=command,exitCode=code,timeout=timeout,watchdogSeconds=45,wallSeconds=time.monotonic()-start,sources=sources,binaries=binaries),indent=2)+'\n',encoding='utf-8')
print('Isolated build:',code,'timeout:',timeout)
sys.exit(0 if code==0 else 1)
