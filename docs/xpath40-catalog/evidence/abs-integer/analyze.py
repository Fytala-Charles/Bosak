"""Check predeclared abs expectations without deriving expected values from engine output."""
from pathlib import Path
import json,collections
e=Path(__file__).resolve().parent
inputs=json.loads((e/'inputs.json').read_text(encoding='utf-8'))
before=[json.loads(x) for x in (e/'before.jsonl').read_text(encoding='utf-8').splitlines()]
after=[json.loads(x) for x in (e/'after.jsonl').read_text(encoding='utf-8').splitlines()]
assert len(inputs)==441 and len(before)==len(after)==1323
stats=collections.Counter();changes=[]
for k,profile in enumerate(['XPath31','XPath40','XPath40Experimental']):
 for i,row in enumerate(inputs):
  b,a=before[k*len(inputs)+i],after[k*len(inputs)+i]
  assert b['Id']==a['Id']==row['Id'] and b['profile']==a['profile']==profile
  assert b['status']==a['status']=='Observed'
  def correct(x):return row['Expected'] in (x.get('error') or '') if row['Error'] else x.get('value')==row['Expected'] and not x.get('error')
  bc,ac=correct(b),correct(a);stats['rows']+=1;stats['baselineDiscrepancies']+=not bc;stats['afterDiscrepancies']+=not ac;stats['corrected']+=not bc and ac;stats['regressions']+=bc and not ac
  assert ac,(row,a)
  if not bc:changes.append(dict(profile=profile,**row,before=b,after=a))
assert stats['corrected']==288 and stats['afterDiscrepancies']==stats['regressions']==0
(e/'diagnostic-summary.json').write_text(json.dumps(dict(stats),indent=2)+'\n',encoding='utf-8')
(e/'corrected-observations.json').write_text(json.dumps(changes,indent=2)+'\n',encoding='utf-8')
print(json.dumps(dict(stats)))
