"""Bind the exact revised declarations and qualified private capture evidence for owner review."""
from pathlib import Path
import json,hashlib,shutil,subprocess
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';E.mkdir(exist_ok=True);D=P/'inputs';D.mkdir(exist_ok=True);T=P/'templates';T.mkdir(exist_ok=True)
if (P/'PROPOSAL-MANIFEST.json').exists():raise RuntimeError('Existing review binding retained; do not regenerate')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,v):p.write_bytes((json.dumps(v,indent=2)+'\n').encode())
for f in (R/'docs/foundation-bcap-outcomes/declarations/proposal').glob('*.txt'):
 (D/'proposal').mkdir(exist_ok=True);shutil.copyfile(f,D/'proposal'/f.name)
for name,path in [('proposed-api-0.json','docs/foundation-bcap-outcomes/declarations/evidence/api-0.json'),('proposed-api-1.json','docs/foundation-bcap-outcomes/declarations/evidence/api-1.json'),('declaration-inputs.json','docs/foundation-bcap-outcomes/declarations/evidence/INPUTS.json')]:shutil.copyfile(R/path,D/name)
old=json.loads((R/'docs/foundation-r2-ledger/evidence/api-0.json').read_text());new=json.loads((D/'proposed-api-0.json').read_text());assert sum(x['publicTypes'] for x in new['assemblies'])==47
changes=[]
for a,b in zip(old['assemblies'],new['assemblies']):
 assert a['assembly']==b['assembly'] and set(a['declarations'])<=set(b['declarations'])
 changes.extend({'assembly':b['assembly'],'declaration':v} for v in b['declarations'] if v not in a['declarations'])
write(E/'DECLARATION-DELTA.json',{'originalProposalTypes':45,'revisedProposalTypes':47,'originalProposalRows':sum(a['declarationCount'] for a in old['assemblies']),'revisedProposalRows':sum(a['declarationCount'] for a in new['assemblies']),'removedRows':0,'addedRows':changes})
bound={}
for folder in ['foundation-bcap-outcomes','foundation-bcap-profile','foundation-bcap-cost']:
 f=R/'docs'/folder/'MANIFEST.json';bound['docs/'+folder+'/MANIFEST.json']=sha(f)
qualified=json.loads((R/'docs/foundation-bcap-profile/evidence/INPUTS.json').read_text());inventory=json.loads((R/'docs/foundation-bcap-profile/evidence/public-api-0.json').read_text())
proposal={'schemaVersion':1,'date':'2026-11-02','purpose':'Exact additive public declaration review and bounded private feasibility binding; not public SDK implementation or owner acceptance','previousR2ProposalManifestSha256':sha(R/'docs/foundation-r2-ledger/MANIFEST.json'),'publicTypes':47,'publicRows':sum(a['declarationCount'] for a in new['assemblies']),'declarations':{f.relative_to(P).as_posix():sha(f) for f in sorted(D.rglob('*')) if f.is_file()},'qualifiedEvidenceManifests':bound,'qualifiedArchives':qualified['archives'],'qualifiedEngineAssemblies':{a['assembly']:a['sha256'].lower() for a in inventory['assemblies']},'runtimePublicDeclarationsUnchanged':True,'proposedBodiesExecute':False,'nativeConsumerAcceptance':False,'performanceBudgetsAccepted':False}
write(P/'PROPOSAL-MANIFEST.json',proposal);pin=sha(P/'PROPOSAL-MANIFEST.json')
original=json.loads((R/'docs/foundation-r2-owner-review/PACKETS.json').read_text());packets=[]
for v in original['packets']:
 row=dict(v);row['id']='BCAP-'+v['id'];row['originalPacketId']=v['id'];packets.append(row)
pack={'schemaVersion':1,'proposalManifestSha256':pin,'packets':packets,'note':'Original questions retained; role modes and additive capture questions in ROLE-MATRIX.json/REVIEW.md. No old disposition automatically transfers.'};write(P/'PACKETS.json',pack)
modes={'O1-Schema':'MissingFullReturn','O2-Schema':'MissingFullReturn','O3-Braid':'CaptureDeltaReview','O4-Core':'CaptureOutcomeDeltaReview','O4-XmlDb':'DeltaAndExistingProviderRevision','O1-XmlDb':'ExplicitUnchangedScopeCarryForward','O2-Core':'ExplicitUnchangedScopeCarryForward'}
matrix=[]
for v in packets:
 for owner in v['owners']:
  prior=v['originalPacketId']+'-'+owner;id=v['id']+'-'+owner
  response={'schemaVersion':1,'returnId':id,'packet':v['id'],'owner':owner,'proposalManifestSha256':pin,'responseStatus':'NotSubmitted','decision':None,'sourceRevision':None,'signer':None,'date':None,'profile':None,'answers':[{'id':q['id'],'answer':None} for q in v['questions']],'unresolvedRequiredSignatureGaps':None,'evidence':None,'runtimeQualification':False,'c45Accepted':False,'apiFrozen':False,'priorReturnId':prior,'reviewMode':modes[prior],'deltaApplicability':None,'originalExceptionGraphDisposition':None,'acceptedPerformanceBudgets':None}
  write(T/(id+'.template.json'),response);matrix.append({'returnId':id,'priorReturnId':prior,'owner':owner,'mode':modes[prior],'template':'templates/'+id+'.template.json','status':'NotSubmitted'})
write(P/'ROLE-MATRIX.json',{'proposalManifestSha256':pin,'roles':matrix,'ownerAccepted':False})
# Preserve delivered originals byte-for-byte without changing or staging incoming source files.
(D/'prior-returns').mkdir(exist_ok=True)
for id in ['O1-XmlDb','O3-Braid','O4-XmlDb']:
 shutil.copyfile(R/'docs/foundation-r2-owner-review/returns'/(id+'.response.json'),D/'prior-returns'/(id+'.json'))
for id in ['O2-Core','O4-Core']:
 source=R/'docs/foundation-r2-consolidation/inputs'/(id+'.json');shutil.copyfile(source,D/'prior-returns'/(id+'.json'))
comments=json.loads((R/'.guard-tmp/bcap-review-issue-468.json').read_text(encoding='utf-8-sig'));write(D/'issue-468-comments.json',comments)
for n,path in [('original-returns.py','docs/foundation-r2-owner-review/tools/returns.py'),('original-r2-manifest.json','docs/foundation-r2-ledger/MANIFEST.json'),('original-r2-api.json','docs/foundation-r2-ledger/evidence/api-0.json')]:shutil.copyfile(R/path,D/n)
for role in matrix:
 f=P/role['template'];v=json.loads(f.read_text());old=D/'prior-returns'/(role['priorReturnId']+'.json');v['priorResponseSha256']=sha(old) if old.exists() else None;write(f,v)
roots={'Braid':'D:/Development/Bosak.Braid','Schema':'D:/Development/Bosak.Schema.Public','XmlDb':'D:/Development/Bosal.XmlDb'}
status={'date':'2026-11-02','proposalManifestSha256':pin,'coreSourceBase':subprocess.check_output(['git','rev-parse','HEAD'],text=True).strip(),'ownerCheckouts':{k:{'path':v,'head':subprocess.check_output(['git','-C',v,'rev-parse','HEAD'],text=True).strip()} for k,v in roots.items()},'issueCommentIds':[x['id'] for page in comments for x in page],'newSchemaFormalReturnsFound':False,'priorDecisions':{'O1-XmlDb':'AcceptWithScope','O2-Core':'AcceptWithScope','O3-Braid':'Revise','O4-Core':'AcceptWithScope','O4-XmlDb':'Revise'},'revisedReturnsSubmitted':0,'revisedReturnsAccepted':0,'coreFeasibility':'CompleteBoundedCoreFeasibility','bcap01Closed':False,'r2Closed':False,'apiFrozen':False,'productionPromoted':False,'messagesSent':False}
write(E/'REVIEW-STATUS.json',status)
print('Prepared exact '+pin+' review binding and seven unsubmitted role templates; no acceptance or messages.')
