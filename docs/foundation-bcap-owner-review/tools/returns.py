"""Validate a revised review form's binding/completeness, never authenticate or accept its owner."""
from pathlib import Path
import json,sys,hashlib,importlib.util,argparse
P=Path(__file__).resolve().parents[1];R=P.parents[1]
spec=importlib.util.spec_from_file_location('original_review_returns',P/'inputs/original-returns.py');base=importlib.util.module_from_spec(spec);spec.loader.exec_module(base)
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def validate(response):
 packets=json.loads((P/'PACKETS.json').read_text());base.validate(response,packets)
 role=next((x for x in json.loads((P/'ROLE-MATRIX.json').read_text())['roles'] if x['returnId']==response['returnId']),None)
 if role is None:raise ValueError('Unknown revised return role')
 for key in ['reviewMode','priorReturnId']:
  expected=role['mode'] if key=='reviewMode' else role[key]
  if response.get(key)!=expected:raise ValueError('Wrong '+key)
 for key in ['deltaApplicability','originalExceptionGraphDisposition']:
  if not isinstance(response.get(key),str) or not response[key].strip():raise ValueError('Explicit '+key+' disposition required, including reasoned non-applicability')
 old=P/'inputs/prior-returns'/(role['priorReturnId']+'.json');expected=sha(old) if old.exists() else None
 if response.get('priorResponseSha256')!=expected:raise ValueError('Original response hash missing or changed')
 if response.get('acceptedPerformanceBudgets') is not None:raise ValueError('This contract review cannot accept workload budgets')
 if response.get('runtimeQualification') is not False:raise ValueError('This packet is scoped design review; actual public/native qualification is separate')
 return True
if __name__=='__main__':
 parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--response',type=Path,required=True);args=parser.parse_args()
 try:validate(json.loads(args.response.read_text(encoding='utf-8-sig')))
 except (ValueError,KeyError,TypeError,OSError) as e:parser.exit(2,str(e)+'\n')
 print('Revised form binding/completeness valid; not owner authentication, acceptance or native qualification.')
