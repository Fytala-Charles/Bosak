"""Capture and verify revised responses without modifying their incoming originals."""
from pathlib import Path
import json,hashlib,shutil,subprocess,sys,importlib.util
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];D=P/'inputs';D.mkdir(exist_ok=True);E=P/'evidence';E.mkdir(exist_ok=True)
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
spec=importlib.util.spec_from_file_location('bcap_returns',R/'docs/foundation-bcap-owner-review/tools/returns.py');v=importlib.util.module_from_spec(spec);spec.loader.exec_module(v)
def write(p,value):p.write_bytes((json.dumps(value,indent=2)+'\n').encode())
if (P/'INPUTS.json').exists():raise RuntimeError('Review already captured; preserve it')
records={}
ownerPaths={'BCAP-O3-Braid':Path('D:/Development/Bosak.Braid/docs/reviews/BCAP-O3-Braid.response.json'),'BCAP-O1-XmlDb':Path('D:/Development/Bosal.XmlDb/docs/foundation-requirements/bcap-owner-review/BCAP-O1-XmlDb.response.json'),'BCAP-O4-XmlDb':Path('D:/Development/Bosal.XmlDb/docs/foundation-requirements/bcap-owner-review/BCAP-O4-XmlDb.response.json')}
for id,owner in ownerPaths.items():
 incoming=R/'docs/foundation-bcap-owner-review/returns'/(id+'.response.json');assert sha(incoming)==sha(owner);data=json.loads(incoming.read_text());assert v.validate(data)
 shutil.copyfile(incoming,D/(id+'.json'));records[id]={'sha256':sha(incoming),'decision':data['decision'],'sourceRevision':data['sourceRevision'],'incoming':str(incoming),'ownerCopy':str(owner),'nativeQualified':False}
root=Path('D:/Development/Bosal.XmlDb');ownerInputs=root/'docs/foundation-requirements/bcap-owner-review/INPUTS.json';shutil.copyfile(ownerInputs,D/'XmlDb-INPUTS.json');x=json.loads(ownerInputs.read_text());verified={}
for name,h in x['workingSourceHashes'].items():assert sha(root/name)==h;verified[name]=h
for name,h in x['draftHashes'].items():assert sha(R/'docs/foundation-bcap-owner-review'/name)==h,name
native=[]
for name in ['src/Bosak.XmlDb.Storage/EmbeddedXmlDatabase.cs','src/Bosak.XmlDb.Storage/ImmutableXmlSnapshot.cs','src/Bosak.XmlDb.Storage/GenerationLease.cs']:
 data=subprocess.check_output(['git','-C',str(root),'show',x['sourceRevision']+':'+name]);assert data.replace(b'\r\n',b'\n')==(root/name).read_bytes().replace(b'\r\n',b'\n');native.append(name)
shutil.copyfile(root/'docs/foundation-requirements/membership/results.json',D/'XmlDb-membership-results.json')
braid=Path('D:/Development/Bosak.Braid');mapped={}
for name in ['src/Bosak.Braid.Infrastructure/BosakPreviewExecutor.cs','src/Bosak.Braid.Infrastructure/WorkerProcessPreviewExecutor.cs','src/Bosak.Braid.Infrastructure/PreviewWorkerProtocol.cs']:
 data=subprocess.check_output(['git','-C',str(braid),'show',records['BCAP-O3-Braid']['sourceRevision']+':'+name]);assert data.replace(b'\r\n',b'\n')==(braid/name).read_bytes().replace(b'\r\n',b'\n');mapped[name]=sha(braid/name)
comments=json.loads((R/'.guard-tmp/bcap-response-check-comments.json').read_text(encoding='utf-8-sig'));write(D/'issue-468-comments.json',comments);comment=next(r for page in comments for r in page if r['id']==6570464706);assert 'NotSubmitted' in comment['body'] and 'not accepted' in comment['body'];write(D/'Schema-status-comment.json',comment)
write(P/'INPUTS.json',{'date':'2026-11-02','proposalManifestSha256':sha(R/'docs/foundation-bcap-owner-review/PROPOSAL-MANIFEST.json'),'preparedPacketManifestSha256':sha(R/'docs/foundation-bcap-owner-review/MANIFEST.json'),'responses':records,'xmlDbReceiptSha256':sha(D/'XmlDb-INPUTS.json'),'xmlDbWorkingHashesVerified':verified,'xmlDbNativePathsStillMatchCommit':native,'xmlDbMembershipIsSeparateUncommittedWork':True,'braidSourceMappingsVerified':mapped,'schemaCommentId':6570464706,'sourceCodeTestsRun':False,'productionChanged':False})
write(P/'STATUS.json',{'date':'2026-11-02','proposalManifestSha256':sha(R/'docs/foundation-bcap-owner-review/PROPOSAL-MANIFEST.json'),'requiredReturns':7,'submitted':3,'scopedDesignAccepts':2,'revise':1,'missing':['BCAP-O1-Schema','BCAP-O2-Schema','BCAP-O2-Core','BCAP-O4-Core'],'decisions':{id:r['decision'] for id,r in records.items()},'bcap01Design':'AcceptedByBraidWithinBoundedScope','bcap01Implementation':'Pending public R3 implementation; no native adoption','xdbLife01':'Revise; native cleanup/fault-profile evidence pending','r2Closed':False,'apiFrozen':False,'humanSignaturesAuthenticated':False,'nativeConsumersQualified':False,'productionChanged':False,'nextCoreTask':'Complete BCAP-O2-Core carry-forward and BCAP-O4-Core outcome review; reconcile Schema full returns and XmlDb native revision separately'})
print('Reviewed three exact-bound source/design returns: two scoped accepts, one provider Revise; Schema and revised Core returns still missing.')
