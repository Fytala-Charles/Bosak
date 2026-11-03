"""Gated command launcher with scoped Windows popup suppression and fresh receipts."""
from pathlib import Path
import argparse,hashlib,json,os,subprocess,sys,time
P=Path(__file__).resolve().parents[1];R=P.parents[1]
sys.path.insert(0,str(R/'docs/foundation-runner-safety/tools'))
from run_caller import process_error_mode

def check_gate():
 g=json.loads((P/'GATE.json').read_text())
 if g['status']!='Ready for implementation':raise ValueError('Blocked pending gate evidence; no command launched')
 refs=[]
 for row in g['requiredReplacements']:
  refs.append((row['path'],row['sha256']))
  if not row['validationReceipt']:raise ValueError('Missing replacement validation receipt')
  subprocess.run([sys.executable,str(R/'docs/foundation-bcap-owner-review/tools/returns.py'),'--response',str(R/row['path'])],check=True,capture_output=True)
 c=g['scopedR2Closure'];refs.append((c['path'],c['sha256']))
 if not c['allSevenRoleValidationReceipt']:raise ValueError('Missing all-seven role validation receipt')
 for name,h in refs:
  if not name or not h:raise ValueError('Missing gate path/hash')
  f=(R/name).resolve()
  if not f.is_relative_to(R) or hashlib.sha256(f.read_bytes()).hexdigest()!=h:raise ValueError('Gate hash mismatch')
 return g

def main():
 a=argparse.ArgumentParser(description=__doc__);a.add_argument('--log',required=True,type=Path);a.add_argument('--timeout-seconds',type=int,default=1800);a.add_argument('--qt4-gate',action='store_true');a.add_argument('command',nargs=argparse.REMAINDER);v=a.parse_args()
 check_gate()
 log=v.log.resolve();base=(P/'evidence/execution').resolve()
 if not log.is_relative_to(base) or log.exists() or log.with_suffix(log.suffix+'.json').exists():raise ValueError('Use a fresh execution receipt path')
 cmd=v.command[1:] if v.command[:1]==['--'] else v.command
 if not cmd or v.timeout_seconds<=0:raise ValueError('Command and positive timeout required')
 log.parent.mkdir(parents=True,exist_ok=True);env=os.environ.copy()
 if v.qt4_gate:env['BOSAK_QT4_GATE_ONLY']='1'
 start=time.monotonic();code=2;failure=None
 try:
  with process_error_mode(),log.open('xb') as out:
   code=subprocess.run(cmd,cwd=R,env=env,stdout=out,stderr=subprocess.STDOUT,timeout=v.timeout_seconds).returncode
 except (OSError,subprocess.TimeoutExpired) as e:failure=type(e).__name__+': '+str(e)
 receipt={'command':cmd,'exitCode':code,'failure':failure,'elapsedSeconds':time.monotonic()-start,'qt4GateOnly':v.qt4_gate,'head':subprocess.check_output(['git','rev-parse','HEAD'],cwd=R,text=True).strip(),'workingTreeStatus':subprocess.check_output(['git','status','--porcelain'],cwd=R,text=True),'logSha256':hashlib.sha256(log.read_bytes()).hexdigest() if log.exists() else None,'qualificationAccepted':False}
 log.with_suffix(log.suffix+'.json').write_text(json.dumps(receipt,indent=2)+'\n')
 return code
if __name__=='__main__':
 try:sys.exit(main())
 except (ValueError,OSError,KeyError,subprocess.CalledProcessError) as e:print(str(e),file=sys.stderr);sys.exit(2)
