"""Compare corrected, predeclared rounding precision expectations with both engines."""
from pathlib import Path
import json,collections,re
e=Path(__file__).resolve().parent
inputs=json.loads((e/'inputs.json').read_text(encoding='utf-8'))
before=[json.loads(x) for x in (e/'before.jsonl').read_text(encoding='utf-8').splitlines()]
after=[json.loads(x) for x in (e/'after.jsonl').read_text(encoding='utf-8').splitlines()]
assert len(inputs)==1012 and len(before)==len(after)==3036
summary=collections.Counter();groups=collections.defaultdict(collections.Counter);changes=[];remaining=[]
def outcome(row):
 if row.get('error') is None:return dict(value=row.get('value'))
 match=re.search(r'\b[A-Z]{4}\d{4}\b',row['error'])
 return dict(error=match[0] if match else row['error'],exceptionType=row.get('exceptionType'))
for k,profile in enumerate(['XPath31','XPath40','XPath40Experimental']):
 for i,row in enumerate(inputs):
  b,a=before[k*len(inputs)+i],after[k*len(inputs)+i]
  assert b['Id']==a['Id']==row['Id'] and b['profile']==a['profile']==profile and a['status']=='Observed'
  correct=outcome(a)==dict(value='true');observed=b['status']=='Observed'
  for counter in [summary,groups[row['scope']]]:
   counter['rows']+=1;counter['baselineObserved']+=observed;counter['baselineNotExecutedAllocationHazard']+=not observed
   counter['baselineDiscrepancies']+=observed and outcome(b)!=dict(value='true');counter['afterDiscrepancies']+=not correct
   counter['correctedObserved']+=observed and outcome(b)!=dict(value='true') and correct
   counter['regressions']+=observed and outcome(b)==dict(value='true') and not correct
  if row['scope']=='extreme':assert correct,(row,a)
  else:assert observed and outcome(b)==outcome(a),(row,b,a)
  if observed and outcome(b)!=outcome(a):changes.append(dict(profile=profile,**row,before=outcome(b),after=outcome(a)))
  if not correct:remaining.append(dict(profile=profile,**row,outcome=outcome(a)))
result=dict(**summary,perScope={k:dict(v) for k,v in groups.items()})
(e/'diagnostic-summary.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
(e/'corrected-observations.json').write_text(json.dumps(changes,indent=2)+'\n',encoding='utf-8')
(e/'remaining-kernel-findings.json').write_text(json.dumps(remaining,indent=2)+'\n',encoding='utf-8')
print(json.dumps(result))
