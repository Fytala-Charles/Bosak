"""Compare independent rational expectations and the prior matrix on both pinned engines."""
from pathlib import Path
import json,re,collections
e=Path(__file__).resolve().parent
def read(name):return json.loads((e/name).read_text(encoding='utf-8'))
def lines(name):return [json.loads(x) for x in (e/name).read_text(encoding='utf-8').splitlines()]
summary={}
for matrix in ['oracle','historical-matrix']:
 inputs=read(matrix+'-inputs.json');before=lines(matrix+'-before.jsonl');after=lines(matrix+'-after.jsonl');assert len(before)==len(after)==len(inputs)*3
 stats=collections.Counter();changes=[]
 for k,profile in enumerate(['XPath31','XPath40','XPath40Experimental']):
  for i,row in enumerate(inputs):
   b,a=before[k*len(inputs)+i],after[k*len(inputs)+i]
   assert b['Id']==a['Id']==row['Id'] and b['profile']==a['profile']==profile
   assert b['status']==a['status']=='Observed'
   def correct(x):
    if row.get('error'):return 'FOAR0002' in (x.get('error') or '')
    return x.get('value')=='true' and not x.get('error')
   bc,ac=correct(b),correct(a);stats['rows']+=1;stats['baselineDiscrepancies']+=not bc;stats['afterDiscrepancies']+=not ac;stats['corrected']+=not bc and ac;stats['regressions']+=bc and not ac
   assert ac,(row,a)
   if not bc:changes.append(dict(profile=profile,Id=row['Id'],expression=row['Expression'],before=b,after=a))
 assert stats['regressions']==stats['afterDiscrepancies']==0
 summary[matrix]=dict(stats)
 (e/(matrix+'-corrections.json')).write_text(json.dumps(changes,indent=2)+'\n',encoding='utf-8')
assert summary['historical-matrix']['corrected']==600
(e/'diagnostic-summary.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
print(json.dumps(summary))
