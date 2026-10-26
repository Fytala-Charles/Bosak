from pathlib import Path
import subprocess,os,json,hashlib,re,datetime,concurrent.futures
root=Path('D:/Development/Bosak');source=root/'.guard-tmp/release1262-source';out=root/'.guard-tmp/release1262-candidate'
build=(out/'build.log').read_bytes();assert 'Build succeeded.' in build.decode('utf-16' if build.startswith(b'\xff\xfe') else 'utf-8-sig')
runner=source/'tests/Bosak.XPath.Conformance40/bin/Release/net10.0/Bosak.XPath.Conformance40.dll';qt3=source/'tests/Bosak.XPath.Conformance/bin/Release/net10.0/Bosak.XPath.Conformance.dll'
def run(label,args,gateOnly=None,dump=False):
 env=os.environ.copy()
 if gateOnly is not None:env['BOSAK_QT4_GATE_ONLY']=gateOnly
 if dump:env['BOSAK_QT4_DUMP_SKIPS']=str(out/(label+'-skips.txt'))
 start=datetime.datetime.now(datetime.timezone.utc).isoformat()
 with (out/(label+'.log')).open('wb') as f:p=subprocess.run(args,cwd=source,env=env,stdout=f,stderr=subprocess.STDOUT)
 data=(out/(label+'.log')).read_bytes();s=data.decode('utf-8-sig');counts={k:int(re.search('^'+k+r':\s+(\d+)',s,re.M)[1]) for k in ['Total','Passed','Failed','Skipped']}
 return {'label':label,'command':args,'startedAtUtc':start,'completedAtUtc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'exitCode':p.returncode,'counts':counts,'logSha256':hashlib.sha256(data).hexdigest()}
jobs=[('frozen-gate',['dotnet',str(runner),str(root/'tests/qt4tests')],'1',True),('qt3',['dotnet',str(qt3),str(root/'tests/qt3tests')],None,False),('current-native-after',['dotnet',str(runner),str(root/'.upstream/qt4tests'),'fn-unix-dateTime'],'1',False)]
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:results=[f.result() for f in [pool.submit(run,*j) for j in jobs]]
expected=[(0,{'Total':10330,'Passed':8436,'Failed':0,'Skipped':1894}),(0,{'Total':31821,'Passed':31142,'Failed':0,'Skipped':679}),(2,{'Total':24,'Passed':22,'Failed':2,'Skipped':0})]
for result,(code,counts) in zip(results,expected):assert result['exitCode']==code and result['counts']==counts,result
assert 'FAIL unix-DateTime-121:' in (out/'current-native-after.log').read_text(encoding='utf-8-sig')
receipt={'date':'2026-10-26','base':'6a3372fc3b91e733b71a426cf40f26feaaeb3bcb','sourceVersion':'1.26.2','publicVersion':'1.26.1','results':results,'runnerSha256':hashlib.sha256(runner.read_bytes()).hexdigest(),'approvedPolicy':'Consolidated1.26.2 candidate regression verification','frozenFixtureChanged':False,'harnessRetryPolicyChanged':False,'currentNativeNegativeFailuresRemainExplicit':True,'releasePerformed':False}
(out/'gate-verification.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8');print(json.dumps(receipt,indent=2))
