from pathlib import Path
import subprocess,json,time,os
area=Path('.tools/fx009-02').resolve(); results=area/'results'; cache=area/'candidate-cache'; feed=area/'feed'; shared=Path('C:/Users/charl/.nuget/packages')
commands=[('candidate-restore',['dotnet','restore',str(area/'candidate/source/tests/Bosak.XmlDb.Xdm.Tests/Bosak.XmlDb.Xdm.Tests.csproj'),'--source',str(feed),'--source',str(shared),'--packages',str(cache),'--ignore-failed-sources','--force-evaluate','-p:NuGetAudit=false','-v','quiet']),('candidate-probe-restore',['dotnet','restore',str(area/'candidate/source/qualification/Probe/Fx009Probe.csproj'),'--source',str(feed),'--source',str(shared),'--packages',str(cache),'--ignore-failed-sources','--force-evaluate','-p:NuGetAudit=false','-v','quiet']),('baseline-test-build',['dotnet','build',str(area/'baseline/source/tests/Bosak.XmlDb.Xdm.Tests/Bosak.XmlDb.Xdm.Tests.csproj'),'-c','Release','-v','quiet']),('candidate-test-build',['dotnet','build',str(area/'candidate/source/tests/Bosak.XmlDb.Xdm.Tests/Bosak.XmlDb.Xdm.Tests.csproj'),'-c','Release','--no-restore','-v','quiet']),('candidate-probe-build',['dotnet','build',str(area/'candidate/source/qualification/Probe/Fx009Probe.csproj'),'-c','Release','--no-restore','-v','quiet'])]
for label,cmd in commands:
 start=time.monotonic();p=subprocess.Popen(cmd,stdout=subprocess.PIPE,stderr=subprocess.PIPE,text=True)
 try: out,err=p.communicate(timeout=240)
 except subprocess.TimeoutExpired:
  subprocess.run(['taskkill','/PID',str(p.pid),'/T','/F'],capture_output=True);out,err=p.communicate();raise
 (results/(label+'.txt')).write_text(out+'\nSTDERR:\n'+err,encoding='utf-8')
 (results/(label+'-command.json')).write_text(json.dumps({'command':cmd,'exitCode':p.returncode,'seconds':time.monotonic()-start,'timeoutSeconds':240,'isolatedNuGetAuditDisabled':label.startswith('candidate-') and 'restore' in label},indent=2)+'\n',encoding='utf-8')
 print(label,p.returncode,out[-700:],err[-400:],flush=True)
 assert p.returncode==0,label
