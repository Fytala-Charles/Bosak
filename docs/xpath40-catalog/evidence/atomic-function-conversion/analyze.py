"""Recheck retained observations against independently flattened integer inputs."""
from pathlib import Path
import json,re,collections

here=Path(__file__).resolve().parent
old=here.parent/'binary-empty'/'observations.json'
inputs=json.loads((here.parent/'array-atomization-audit'/'inputs.json').read_text())

def outcome(v):
    if v['error'] is None:return {'value':v['value']}
    match=re.search(r'\b[A-Z]{4}\d{4}\b',v['error'])
    return {'errorCode':match[0] if match else v['error']}

def expected(row):
    a=row['flatIntegers'];n=len(a);c=row['context']
    def seq(values,kind):return {'value':';'.join(str(x).lower()+'|'+kind for x in values)}
    if c=='data':return {'value':','.join(map(str,a))+'|string'}
    if c=='general-eq':return seq([2 in a],'boolean')
    if c=='castable':return seq([n<=1],'boolean')
    if n>1:return {'errorCode':'XPTY0004'}
    if not n:return {'value':''}
    if c=='value-eq':return seq([a[0]==2],'boolean')
    if c=='range':return seq(range(a[0],4),'integer')
    return seq([-a[0] if c=='unary-minus' else abs(a[0]) if c=='function-argument' else a[0]],'integer')

before=json.loads(old.read_text());after=json.loads((here/'array-matrix-final.json').read_text())
assert len(before)==len(after)==1152 and len(inputs)==192
summary=collections.Counter(); changes=[]
for k,profile in enumerate(['XPath31','XPath40','XPath40Experimental']):
    for i,row in enumerate(inputs):
        wanted=expected(row)
        for side in [0,1]:
            j=k*384+i*2+side;b=before[j];a=after[j]
            assert b['expression']==a['expression']==row['expression' if side==0 else 'referenceExpression']
            assert b['profile']==a['profile']==profile
            previous=outcome(b);current=outcome(a);label='array' if side==0 else 'flat'
            summary[label+'BeforeDiscrepancies']+=previous!=wanted
            summary[label+'AfterDiscrepancies']+=current!=wanted
            if previous!=current:
                assert previous!=wanted and current==wanted
                summary[label+'Corrected']+=1
                changes.append(dict(profile=profile,context=row['context'],shape=row['shape'],side=label,before=previous,after=current))
summary['regressions']=0
(here/'matrix-summary.json').write_text(json.dumps(dict(summary),indent=2)+'\n')
(here/'matrix-changes.json').write_text(json.dumps(changes,indent=2)+'\n')
print(json.dumps(dict(summary)))
