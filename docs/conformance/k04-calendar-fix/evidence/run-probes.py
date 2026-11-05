from pathlib import Path
import subprocess,json,concurrent.futures
P=Path(__file__).resolve().parent;R=P.parents[3]
def run(row):
 name=row['Suite']+'-'+row['Name'];request=P/(name+'-request.json');request.write_text(json.dumps(row)+'\n');command=['dotnet',str(R/'.guard-tmp/k04-calendar-driver/bin/Release/net10.0/Probe.dll'),'execute',str(request)]
 r=subprocess.run(command,cwd=R,capture_output=True,timeout=20);(P/(name+'-output.txt')).write_bytes(r.stdout+r.stderr);assert r.returncode==0
 return dict(name=name,exitCode=r.returncode,timeout=False,watchdogSeconds=20,result=json.loads(r.stdout.decode('utf-8-sig').splitlines()[-1]))
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:receipts=list(pool.map(run,json.loads((R/'docs/conformance/k04-calendar-review/requests.json').read_text())))
(P/'execution.json').write_text(json.dumps(receipts,indent=2)+'\n');print(json.dumps(dict(total=len(receipts),passed=sum(r['result']['kind']=='Passed' for r in receipts),failed=sum(r['result']['kind']=='Failed' for r in receipts))))
