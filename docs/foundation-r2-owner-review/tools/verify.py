"""Verify prepared packets and unanswered returns without inferring acceptance."""
from pathlib import Path
import copy,hashlib,json,subprocess
from returns import validate
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
pack=json.loads((P/'PACKETS.json').read_text());B=R/'docs/foundation-r2-ledger'
assert not pack['ownerAcceptance'] and not pack['deliveryConfirmed'] and pack['proposalManifestSha256']==sha(B/'MANIFEST.json') and pack['ledgerSha256']==sha(B/'LEDGER.json')
assert [x['id'] for x in pack['packets']]==['O1','O2','O3','O4']
templates=list((P/'templates').glob('*.json'));assert len(templates)==7
for packet in pack['packets']:
 assert packet['prepared'] and not packet['deliveryConfirmed'] and packet['acceptance']=='Pending' and len(packet['questions'])==4
 for owner in packet['owners']:
  template=json.loads((P/'templates'/(packet['id']+'-'+owner+'.template.json')).read_text())
  assert template['owner']==owner and template['proposalManifestSha256']==pack['proposalManifestSha256'] and template['responseStatus']=='NotSubmitted'
  assert template['decision'] is None and template['signer'] is None and template['unresolvedRequiredSignatureGaps'] is None and template['evidence'] is None
  assert all(x['answer'] is None for x in template['answers']) and {x['id'] for x in template['answers']}=={x['id'] for x in packet['questions']}
checks=[]
def rejects(name,response):
 try:validate(response,pack)
 except ValueError:checks.append({'id':name,'passed':True});return
 raise AssertionError(name)
t=json.loads(templates[0].read_text());rejects('unsubmitted-is-not-acceptance',t)
good=copy.deepcopy(t);good.update(responseStatus='Submitted',decision='AcceptWithScope',sourceRevision='1'*40,signer='Synthetic format fixture',date='2026-11-01',profile='Synthetic format-only example',unresolvedRequiredSignatureGaps=[],evidence=[{'kind':'DesignDisposition','reference':'Synthetic fixture; no actual owner response'}])
for answer in good['answers']:answer['answer']='Synthetic completeness check, not owner acceptance'
assert validate(good,pack);checks.append({'id':'complete-synthetic-format','passed':True})
for name,key,value in [('foreign-draft','proposalManifestSha256','0'*64),('wrong-owner','owner','Unknown'),('missing-signer','signer',None),('missing-answer','answers',good['answers'][:-1]),('unresolved-signature','unresolvedRequiredSignatureGaps',['Missing required member']),('unknown-gap-assessment','unresolvedRequiredSignatureGaps',None),('missing-evidence','evidence',[]),('premature-freeze','apiFrozen',True)]:
 test=copy.deepcopy(good);test[key]=value;rejects(name,test)
observations=json.loads((P/'LOCAL-RETURN-CHECK.json').read_text());assert not observations['explicitRevisedDraftReturnFound']
for repo in observations['repositories']:
 assert repo['observedStatus']=='clean' and not repo['matchingLocalDocuments']
 assert subprocess.check_output(['git','cat-file','-t',repo['revision']],cwd=repo['root']).decode().strip()=='commit'
source=json.loads((R/'docs/foundation-xslt-source-membership/evidence/SOURCE.json').read_text())
for name,digest in source['files'].items():assert sha(R/name)==digest,name
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
if '--record-format-checks' in __import__('sys').argv:
 (P/'evidence/FORMAT-CHECKS.json').write_text(json.dumps({'scope':'Synthetic response-format controls only; no actual owner return, authentication, runtime or gate acceptance','checks':checks},indent=2)+'\n',encoding='utf-8')
print('Verified four prepared packets, seven unanswered role templates, ten format controls and unchanged production; delivery/acceptance not claimed.')
