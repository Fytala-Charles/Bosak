"""Check response completeness and draft binding; never authenticate an owner or accept a gate."""
from pathlib import Path
from datetime import date
import argparse,json,re
P=Path(__file__).resolve().parents[1]
def validate(response,packets=None):
 packets=packets or json.loads((P/'PACKETS.json').read_text())
 def need(condition,message):
  if not condition:raise ValueError(message)
 need(isinstance(response,dict) and response.get('schemaVersion')==1,'Response must be a schemaVersion 1 object')
 packet=next((x for x in packets['packets'] if x['id']==response.get('packet')),None)
 need(packet is not None,'Unknown packet')
 need(response.get('owner') in packet['owners'],'Wrong owner role')
 need(response.get('returnId')==packet['id']+'-'+response['owner'],'Wrong return identity')
 need(response.get('proposalManifestSha256')==packets['proposalManifestSha256'],'Response belongs to another draft')
 need(response.get('responseStatus')=='Submitted','Unsubmitted templates are not dispositions')
 need(response.get('decision') in ['Accept','AcceptWithScope','Revise','Reject'],'Explicit decision required')
 need(bool(re.fullmatch('[0-9a-fA-F]{40}',response.get('sourceRevision') or '')),'Exact source revision required')
 need(isinstance(response.get('signer'),str) and bool(response['signer'].strip()),'Named signer required')
 try:date.fromisoformat(response.get('date') or '')
 except (ValueError,TypeError):raise ValueError('ISO date required')
 need(isinstance(response.get('profile'),str) and bool(response['profile'].strip()),'Explicit profile required')
 answers=response.get('answers');need(isinstance(answers,list),'Answers required')
 need(all(isinstance(x,dict) for x in answers),'Answer objects required')
 ids=[x.get('id') for x in answers];need(len(ids)==len(set(ids)) and set(ids)=={x['id'] for x in packet['questions']},'All question IDs required exactly once')
 need(all(isinstance(x.get('answer'),str) and x['answer'].strip() for x in answers),'Every question needs an answer or reasoned not-applicable disposition')
 gaps=response.get('unresolvedRequiredSignatureGaps');need(isinstance(gaps,list),'Explicit gap assessment required; null is not an empty accepted list')
 if response['decision'] in ['Accept','AcceptWithScope']:need(not gaps,'Acceptance cannot hide required signature gaps')
 evidence=response.get('evidence');need(isinstance(evidence,list) and bool(evidence),'Evidence references required')
 need(all(isinstance(x,dict) and x.get('kind') in ['SourceMapping','DesignDisposition','RuntimeQualification'] and isinstance(x.get('reference'),str) and x['reference'].strip() for x in evidence),'Evidence kind/reference required')
 need(response.get('c45Accepted') is False and response.get('apiFrozen') is False,'R2 response must not claim C45/freeze acceptance')
 return True
if __name__=='__main__':
 parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--response',type=Path,required=True);args=parser.parse_args()
 try:validate(json.loads(args.response.read_text(encoding='utf-8')))
 except (ValueError,OSError,KeyError,TypeError) as error:parser.exit(2,str(error)+'\n')
 print('Response format and draft binding valid; owner authentication, semantics and gate acceptance remain separate review obligations.')
