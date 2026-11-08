"""Capture Schema's formal revised returns and reconcile the five external dispositions."""
from pathlib import Path
import json,hashlib,shutil,subprocess,re,importlib.util
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];D=P/'inputs';D.mkdir(exist_ok=True)
if (P/'INPUTS.json').exists():raise RuntimeError('Existing review retained')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
spec=importlib.util.spec_from_file_location('bcap_validator',R/'docs/foundation-bcap-owner-review/tools/returns.py');v=importlib.util.module_from_spec(spec);spec.loader.exec_module(v)
def write(p,x):p.write_bytes((json.dumps(x,indent=2)+'\n').encode())
pages=json.loads((R/'.guard-tmp/schema-response-check.json').read_text(encoding='utf-8-sig'));comment=next(c for page in pages for c in page if c['id']==6573672774)
published={x['returnId']:x for x in [json.loads(b) for b in re.findall(r'```json\s*(\{.*?\})\s*```',comment['body'],re.S)]}
records={}
for id in ['BCAP-O1-Schema','BCAP-O2-Schema','BCAP-O1-XmlDb','BCAP-O3-Braid','BCAP-O4-XmlDb']:
 source=R/'docs/foundation-bcap-owner-review/returns'/(id+'.response.json');data=json.loads(source.read_text());assert v.validate(data)
 if id.endswith('-Schema'):assert published[id]==data and data['decision']=='Revise'
 shutil.copyfile(source,D/(id+'.json'));records[id]={'sha256':sha(source),'decision':data['decision'],'sourceRevision':data['sourceRevision'],'incoming':str(source),'requiredSignatureGaps':data['unresolvedRequiredSignatureGaps']}
write(D/'Schema-submission-comment.json',comment);write(D/'issue-468-comments.json',pages)
work=R/'.worktrees/Bosak.Schema.Assessment';pin=subprocess.check_output(['git','rev-parse','HEAD'],cwd=work,text=True).strip();assert pin=='c3624998eca8c83018e121fa84af611f834a0501'
sources={name:sha(work/name) for name in ['src/Bosak.Schema/Assessment/AssessedDocument.cs','src/Bosak.Schema/Assessment/XsdDocumentAssessor.cs','docs/DOCUMENT-ASSESSMENT.md','docs/S009-02-PROVIDER-FACT-MAPPING.md']}
assert subprocess.check_output(['git','status','--porcelain'],cwd=work,text=True)==''
limits=R/'docs/foundation-bcap-owner-review/inputs/proposal/QueryFacade.cs.txt';text=limits.read_text();assert 'the default struct is invalid' in text and 'no owner workload defaults are selected' in text
write(P/'INPUTS.json',{'date':'2026-11-02','proposalManifestSha256':sha(R/'docs/foundation-bcap-owner-review/PROPOSAL-MANIFEST.json'),'responses':records,'schemaCommentId':6573672774,'schemaFormsSemanticallyMatchPublishedJson':True,'nativeWorktree':str(work),'nativeRevision':pin,'nativeWorktreeClean':True,'nativeSourceHashes':sources,'limitsDeclarationSha256':sha(limits),'ownerResponsesModified':False,'sourceTestsRun':False,'productionChanged':False})
write(P/'STATUS.json',{'date':'2026-11-02','submitted':5,'required':7,'scopedDesignAccepts':2,'revise':3,'missing':['BCAP-O2-Core','BCAP-O4-Core'],'decisions':{k:x['decision'] for k,x in records.items()},'schemaRequiredSignatureGaps':[],'schemaDecision':'Both Revise; not accepted','schemaNativeAccess':'Exact cited candidate exists in clean Core assessment worktree','policyClarification':'Explicit caller finite limits/no defaults is already proposed; implementation and native workload budgets are separate','r2Closed':False,'apiFrozen':False,'nativeConsumersQualified':False,'humanSignaturesAuthenticated':False,'productionChanged':False,'nextCoreTask':'Complete own O2/O4 reviews and clarify explicit-limit policy; Schema owner reviews exact assessment source and identifies precise design blockers versus later implementation obligations'})
print('Verified both Schema Revise returns match published JSON and exact proposal; current total 5/7, with only revised Core O2/O4 unsubmitted.')
