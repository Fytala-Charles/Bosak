"""Verify exact review bindings, unchanged qualification inputs and unsubmitted role forms."""
from pathlib import Path
import json,hashlib,copy
from returns import validate
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
proposal=json.loads((P/'PROPOSAL-MANIFEST.json').read_text());pin=sha(P/'PROPOSAL-MANIFEST.json');pack=json.loads((P/'PACKETS.json').read_text());roles=json.loads((P/'ROLE-MATRIX.json').read_text());status=json.loads((P/'evidence/REVIEW-STATUS.json').read_text())
assert pack['proposalManifestSha256']==roles['proposalManifestSha256']==status['proposalManifestSha256']==pin
assert proposal['publicTypes']==47 and not proposal['proposedBodiesExecute'] and not proposal['nativeConsumerAcceptance'] and not proposal['performanceBudgetsAccepted']
for n,h in proposal['declarations'].items():assert sha(P/n)==h,n
for n,h in proposal['qualifiedEvidenceManifests'].items():assert sha(R/n)==h,n
for name,v in proposal['qualifiedArchives'].items():
 portable=R/'artifacts'/name;archive=portable if portable.exists() else Path(v['path']);assert sha(archive)==v['sha256']
a=json.loads((P/'inputs/proposed-api-0.json').read_text());assert a==json.loads((P/'inputs/proposed-api-1.json').read_text()) and sum(x['publicTypes'] for x in a['assemblies'])==47
oldManifest=P/'inputs/original-r2-manifest.json';assert sha(oldManifest)==proposal['previousR2ProposalManifestSha256']
oldApi=P/'inputs/original-r2-api.json';assert sha(oldApi)==json.loads(oldManifest.read_text())['evidence/api-0.json']
for old,new in zip(json.loads(oldApi.read_text())['assemblies'],a['assemblies']):assert old['assembly']==new['assembly'] and set(old['declarations'])<=set(new['declarations'])
assert len(roles['roles'])==7 and status['revisedReturnsSubmitted']==status['revisedReturnsAccepted']==0
checks=[]
for role in roles['roles']:
 template=json.loads((P/role['template']).read_text());assert template['responseStatus']=='NotSubmitted' and template['decision'] is None and template['signer'] is None
 assert template['proposalManifestSha256']==pin
 try:validate(template)
 except ValueError:pass
 else:raise AssertionError('Template must not be a disposition')
 response=copy.deepcopy(template);response.update(responseStatus='Submitted',decision='AcceptWithScope',sourceRevision='0'*40,signer='FORMAT TEST ONLY, not an owner signature',date='2026-11-02',profile='Synthetic format control only',answers=[{'id':v['id'],'answer':'FORMAT TEST ONLY: unchanged or explicitly scoped test disposition'} for v in template['answers']],unresolvedRequiredSignatureGaps=[],evidence=[{'kind':'DesignDisposition','reference':'FORMAT TEST ONLY'}],deltaApplicability='FORMAT TEST ONLY',originalExceptionGraphDisposition='FORMAT TEST ONLY')
 assert validate(response);checks.append({'id':role['returnId']+'-complete-format','passed':True})
 for key,value in [('proposalManifestSha256','0'*64),('priorReturnId','wrong'),('deltaApplicability',None),('originalExceptionGraphDisposition',None),('unresolvedRequiredSignatureGaps',['required gap']),('acceptedPerformanceBudgets',{'milliseconds':1}),('runtimeQualification',True),('priorResponseSha256','0'*64)]:
  bad=copy.deepcopy(response);bad[key]=value
  try:validate(bad)
  except ValueError:checks.append({'id':role['returnId']+'-refuses-'+key,'passed':True})
  else:raise AssertionError(key)
assert not status['bcap01Closed'] and not status['r2Closed'] and not status['apiFrozen'] and not status['messagesSent']
# Format tests are ephemeral in-memory fixtures, never owner returns or accepted responses.
receipt={'checks':len(checks),'formatTestsOnly':True,'ownerDispositionsCreated':0,'checksPassed':checks}
path=P/'evidence/FORMAT-CONTROLS.json'
if (P/'MANIFEST.json').exists():assert json.loads(path.read_text())==receipt
else:path.write_bytes((json.dumps(receipt,indent=2)+'\n').encode())
if (P/'MANIFEST.json').exists():
 manifest=json.loads((P/'MANIFEST.json').read_text())
 expected={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json' and '__pycache__' not in p.parts and not(p.relative_to(P).parts[0]=='returns' and p.name!='README.md')}
 assert set(manifest)==expected
 for n,h in manifest.items():assert sha(P/n)==h,n
print('Verified exact 47-type binding, seven unsubmitted forms and '+str(len(checks))+' format controls; no owner acceptance or production change.')
