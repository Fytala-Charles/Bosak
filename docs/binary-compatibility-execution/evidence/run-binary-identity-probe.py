from pathlib import Path
import shutil,json,hashlib,subprocess,time,os,sys
r=Path.cwd();mode=sys.argv[1];dest=r/f'.guard-tmp/binary-identity-{mode}';source=r/'.guard-tmp/binary-identity-probe/bin/Release/net10.0'
if not dest.exists():shutil.copytree(source,dest)
if mode=='replacement':
 controls=json.loads((r/'.guard-tmp/binary-execution-processes.json').read_text());assert all(row.get('exitCode')==0 for row in controls if '-baseline-' in row['tag'])
 candidate=r/'.guard-tmp/braid-candidate/src/Bosak.Braid.Desktop/bin/Release/net10.0'
 for p in dest.glob('Bosak*.dll'):
  if p.name.startswith('Bosak.XPath.') or p.name in ['Bosak.XQuery.dll','Bosak.Xslt.dll']:shutil.copyfile(candidate/p.name,p)
for p in source.glob('*'):
 if p.is_file() and not (p.name.startswith('Bosak.XPath.') or p.name in ['Bosak.XQuery.dll','Bosak.Xslt.dll']):assert hashlib.sha256(p.read_bytes()).digest()==hashlib.sha256((dest/p.name).read_bytes()).digest()
results=r/f'.guard-tmp/binary-results/braid-{mode}-identity';results.mkdir(parents=True,exist_ok=True)
env=os.environ.copy();env['DOTNET_STARTUP_HOOKS']=str(r/'.guard-tmp/binary-load-hook/bin/Release/net10.0/Hook.dll');env['BINARY_QUAL_TRACE_DIR']=str(results/'loads')
start=time.monotonic()
with (results/'process.txt').open('w',encoding='utf-8') as log:
 p=subprocess.run(['dotnet',str(dest/'Probe.dll')],stdout=log,stderr=subprocess.STDOUT,env=env,timeout=90)
(results/'process.json').write_text(json.dumps({'exitCode':p.returncode,'externalTimeoutSeconds':90,'elapsedSeconds':round(time.monotonic()-start,3),'unchangedProbeAndConsumerBytes':True},indent=2)+'\n')
print((results/'process.txt').read_text())
