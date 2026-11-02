"""Verify the versioned XmlDb fix return and capture current four-project status."""
from pathlib import Path
import json,hashlib,subprocess,shutil,xml.etree.ElementTree as ET,importlib.util
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];D=P/'inputs';D.mkdir(exist_ok=True);E=P/'evidence';E.mkdir(exist_ok=True)
if (P/'INPUTS.json').exists():raise RuntimeError('Existing status snapshot retained')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,v):p.write_bytes((json.dumps(v,indent=2)+'\n').encode())
spec=importlib.util.spec_from_file_location('status_return_validator',R/'docs/foundation-bcap-owner-review/tools/returns.py');v=importlib.util.module_from_spec(spec);spec.loader.exec_module(v)
x=Path('D:/Development/Bosal.XmlDb');owner=x/'docs/foundation-requirements/bcap-owner-review/native-lifetime';receipt=json.loads((owner/'INPUTS.json').read_text());fix=receipt['sourceRevision'];assert fix=='0d18e91dc42db4831e50bf9e902f95fae7077d6d'
name='BCAP-O4-XmlDb.native-lifetime-0d18e91.response.json';incoming=R/'docs/foundation-bcap-owner-review/returns'/name;assert sha(incoming)==sha(owner/name)==receipt['response']['sha256'];assert v.validate(json.loads(incoming.read_text()))
shutil.copyfile(incoming,D/name);shutil.copyfile(owner/'INPUTS.json',D/'XmlDb-fix-review-INPUTS.json')
files={}
for n,row in receipt['sourceInputs'].items():
 data=subprocess.check_output(['git','-C',str(x),'show',fix+':'+n]);assert hashlib.sha256(data).hexdigest()==row['committedSha256'],n
 assert data.replace(b'\r\n',b'\n')==(x/n).read_bytes().replace(b'\r\n',b'\n'),n;files[n]=row
for n,h in receipt['draftHashes'].items():assert sha(R/'docs/foundation-bcap-owner-review'/n)==h,n
for n,h in receipt['qualifiedReports'].items():assert sha(x/n)==h,n
results=x/'docs/foundation-requirements/native-lifetime/results.json';shutil.copyfile(results,D/'XmlDb-native-results.json');r=json.loads(results.read_text());assert r['newCases']==190 and r['catalog']['windows']==r['catalog']['linux']==4482
trx=[]
for platform,key in [('Windows','selectedWindows'),('Linux','selectedLinux'),('Focused','selectedFocused')]:
 total=0
 for row in r[key]:
  f=x/row['path'];assert sha(f)==row['sha256'];c=ET.parse(f).find('{*}ResultSummary/{*}Counters').attrib;assert c['passed']==row['counts']['passed'] and c['failed']=='0' and c['notExecuted']=='0'
  total+=int(c['passed']);trx.append({'platform':platform,'path':str(f),'sha256':sha(f),'passed':int(c['passed'])})
 assert total==(190 if platform=='Focused' else 4482)
active={}
for id in ['BCAP-O1-Schema','BCAP-O2-Schema','BCAP-O1-XmlDb','BCAP-O2-Core','BCAP-O3-Braid','BCAP-O4-Core','BCAP-O4-XmlDb']:
 source=incoming if id=='BCAP-O4-XmlDb' else R/'docs/foundation-bcap-owner-review/returns'/(id+'.response.json');data=json.loads(source.read_text());assert v.validate(data)
 dest=D/(id+'.json');shutil.copyfile(source,dest);active[id]={'decision':data['decision'],'sourceRevision':data['sourceRevision'],'sha256':sha(source),'copy':dest.relative_to(P).as_posix()}
old=R/'docs/foundation-bcap-owner-review/returns/BCAP-O4-XmlDb.response.json';assert sha(old)==receipt['priorBcapResponseSha256'];shutil.copyfile(old,D/'BCAP-O4-XmlDb.previous-Revise.json')
roots={'Core':R,'Braid':Path('D:/Development/Bosak.Braid'),'XmlDb':x,'SchemaPublic':Path('D:/Development/Bosak.Schema.Public'),'SchemaAssessment':R/'.worktrees/Bosak.Schema.Assessment'}
checkouts={k:{'path':str(p),'head':subprocess.check_output(['git','rev-parse','HEAD'],cwd=p,text=True).strip()} for k,p in roots.items()}
comments=json.loads((R/'.guard-tmp/ecosystem-status-comments.json').read_text(encoding='utf-8-sig'));write(D/'issue-468-comments.json',comments)
write(P/'INPUTS.json',{'date':'2026-11-02','proposalManifestSha256':sha(R/'docs/foundation-bcap-owner-review/PROPOSAL-MANIFEST.json'),'checkouts':checkouts,'activeResponses':active,'xmlDbFix':fix,'xmlDbFixReviewSha256':sha(D/'XmlDb-fix-review-INPUTS.json'),'xmlDbQualifiedSourceInputs':files,'xmlDbTrxChecked':trx,'xmlDbPriorReviseSha256':sha(old),'productionChanged':False,'providerTestsRerun':False})
write(P/'STATUS.json',{'date':'2026-11-02','submitted':7,'required':7,'scopedDesignAccepts':5,'revise':2,'missing':[],'decisions':{k:v['decision'] for k,v in active.items()},'remainingDesignRevisions':['BCAP-O1-Schema','BCAP-O2-Schema'],'xdbLife01':'ResolvedWithinQualifiedCurrentPinProviderScope','xmlDbFix':fix,'xmlDbProductCore':'1.26.2','xmlDbObservedHead':checkouts['XmlDb']['head'],'nativeBcapCandidateQualified':False,'performanceBudgetsAccepted':False,'r2Closed':False,'apiFrozen':False,'productionChanged':False,'nextStep':'Obtain precise Schema scoped API/policy/source-feasibility dispositions using the reconciliation; Core may prepare R3 neutral-admission scope but public implementation remains gated'})
print('Verified XmlDb fix input blobs and eight selected TRX receipts; active ledger is five scoped accepts/two Schema revisions across seven roles.')
