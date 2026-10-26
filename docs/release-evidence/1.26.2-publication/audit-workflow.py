from pathlib import Path
import subprocess,json,re
r=Path('D:/Development/Bosak'); p=r/'.guard-tmp/release1262-publication'; rev=json.loads((p/'implementation-merge.json').read_text(encoding='utf-8'))['mergeCommit']['oid']
def run(*args): return subprocess.check_output(args,cwd=r).decode('utf-8-sig').strip()
runid=json.loads((p/'release-run.json').read_text())['databaseId']; runid=str(runid); data=json.loads(run('gh','run','view',runid,'--json','databaseId,event,headSha,status,conclusion,jobs'))
assert data['event']=='push' and data['headSha']==rev and data['status']=='completed' and data['conclusion']=='success'
runs=json.loads(run('gh','run','list','--workflow','release.yml','--branch','v1.26.2','--limit','10','--json','databaseId,event,headSha')); assert len(runs)==1 and runs[0]['databaseId']==int(runid)
log=run('gh','run','view',runid,'--log'); (p/'workflow-final.json').write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8'); (p/'workflow-final.log').write_text(log+'\n',encoding='utf-8')
counts=re.findall(r'Passed!\s+- Failed:\s*(\d+), Passed:\s*(\d+), Skipped:\s*(\d+)',log)
assert len(counts)==10 and tuple(sum(int(c[i]) for c in counts) for i in range(3))==(0,18737,0),counts
assert log.count('Your package was pushed.')==10
receipt={'workflow':int(runid),'event':'push','revision':rev,'solutionTestsPassed':18737,'failed':0,'skipped':0,'uploadAcknowledgments':10,'releaseRunCountForTag':1,'duplicateDispatchPerformed':False}
(p/'workflow-audit.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8'); print(json.dumps(receipt))
