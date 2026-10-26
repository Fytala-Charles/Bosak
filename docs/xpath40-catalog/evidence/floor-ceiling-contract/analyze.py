"""Check predeclared floor/ceiling public argument-contract expectations."""
from pathlib import Path
import json
e=Path(__file__).resolve().parent
rows=json.loads((e/'inputs.json').read_text(encoding='utf-8'));observed=[json.loads(x) for x in (e/'observations.jsonl').read_text(encoding='utf-8').splitlines()]
assert len(rows)==266 and len(observed)==798
for k,profile in enumerate(['XPath31','XPath40','XPath40Experimental']):
 for i,row in enumerate(rows):
  result=observed[k*len(rows)+i];assert result['Id']==row['Id'] and result['profile']==profile and result['status']=='Observed'
  assert row['Expected'] in (result.get('error') or '') if row['Error'] else result.get('value')==row['Expected'] and not result.get('error'),(row,result)
summary=dict(rows=798,discrepancies=0,implementationChange=False,expectedErrorsAreControls=True)
(e/'diagnostic-summary.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8');print(json.dumps(summary))
