"""Verify preserved inputs and that unsigned drafts cannot close R2."""
from pathlib import Path
import hashlib,json,subprocess,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for n in (1,2):
 name=f'BCAP-O{n}-Schema'
 old=P/f'inputs/{name}.previous.response.json'
 d=json.loads((P/f'drafts/{name}.2026-11-03.draft.json').read_text())
 assert d['responseStatus']=='NotSubmitted' and d['decision']=='AcceptWithScope'
 assert d['draftPreparation']['previousBcapResponseSha256']==sha(old)
 for key in ['signer','sourceRevision','deltaApplicability','originalExceptionGraphDisposition']:assert d[key] is None
 for key in ['runtimeQualification','c45Accepted','apiFrozen']:assert d[key] is False
 assert d['acceptedPerformanceBudgets'] is None
 assert d['evidence'][0]['sha256']==sha(P/'inputs/Schema-owner-prose.txt')
 assert json.loads(old.read_text(encoding='utf-8-sig'))['decision']=='Revise'
 result=subprocess.run([sys.executable,str(R/'docs/foundation-bcap-owner-review/tools/returns.py'),'--response',str(P/f'drafts/{name}.2026-11-03.draft.json')],capture_output=True,text=True)
 assert result.returncode==2 and 'Unsubmitted templates are not dispositions' in result.stderr,result
 if (R/f'docs/foundation-bcap-owner-review/returns/{name}.response.json').exists():
  # Later incoming replacements may differ; only sealed historical copies are authoritative here.
  pass
assert 'ff475d712f6ef6aa5033113cc39afb887ae4cbad24705098c5c9cb2097641688' in (P/'inputs/Schema-owner-prose.txt').read_text(encoding='utf-8-sig')
for name,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha(P/name)==h,name
print('Preserved prose/prior decisions and both unsigned drafts verified; validator correctly rejects drafts. No R2 closure inferred.')
