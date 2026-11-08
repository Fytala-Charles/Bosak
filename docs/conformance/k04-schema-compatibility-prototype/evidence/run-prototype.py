"""Bound each independent fake-graph/mutation process; never build product or consumer projects."""
from pathlib import Path
import concurrent.futures,subprocess,json,time,hashlib,sys
E=Path(__file__).resolve().parent;R=E.parents[3]
dll=R/'.guard-tmp/k04-schema-compatibility-prototype/bin/Release/net10.0/C44R2.Prototype.dll'
D=R/'.guard-tmp/k04-schema-compatibility-prototype'
build=json.loads((E/'build-receipt.json').read_text())
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
assert build['exitCode']==0 and not build['timeout'],'Successful build receipt required'
assert all(sha(D/name)==h for name,h in build['sources'].items()),'Source changed since build'
assert all(sha(dll.parent/name)==h for name,h in build['binaries'].items()),'Binary changed since build'
if '--verify-only' in sys.argv:
 print('Build/source/binary preflight verified.');sys.exit(0)
modes=['none','token-only','qname','fingerprint','profile','schema-version','root-only','storage-id','revalidate','union','rewrite','strip','fallback']
def run(mode):
 command=['dotnet',str(dll),mode];start=time.monotonic();name='prototype-'+mode+'.log'
 with (E/name).open('wb') as output:
  try:
   process=subprocess.run(command,cwd=R,stdout=output,stderr=subprocess.STDOUT,timeout=45)
   code=process.returncode;timed_out=False
  except subprocess.TimeoutExpired:code=None;timed_out=True
 result=json.loads((E/name).read_text(encoding='utf-8-sig')) if code in (0,2) else None
 if result:(E/('prototype-'+mode+'-results.json')).write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
 return dict(mode=mode,command=command,exitCode=code,timeout=timed_out,watchdogSeconds=45,wallSeconds=time.monotonic()-start,log=name,logSha256=hashlib.sha256((E/name).read_bytes()).hexdigest(),passed=result['passed'] if result else None,failed=result['failed'] if result else None,failedControls=[x['name'] for x in result['tests'] if not x['passed']] if result else [])
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:rows=list(pool.map(run,modes))
(E/'prototype-receipts.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8')
for row in rows:print(row['mode'],row['exitCode'],row['passed'],row['failed'])
assert rows[0]['exitCode']==0 and rows[0]['failed']==0
assert all(not row['timeout'] and row['exitCode']==2 and row['failed']>0 for row in rows[1:])
