"""Check retained rounding observations against predeclared argument contracts."""
from pathlib import Path
import collections,json,re

here=Path(__file__).resolve().parent
inputs=json.loads((here/'inputs.json').read_text())
before=json.loads((here/'observations-before.json').read_text())
after=json.loads((here/'observations-final.json').read_text())
assert len(inputs)==498 and len(before)==len(after)==1494

def outcome(row):
    if row['error'] is None:return {'value':row['value']}
    match=re.search(r'\b[A-Z]{4}\d{4}\b',row['error'])
    return {'errorCode':match[0] if match else row['error']}

counts=collections.Counter();groups=collections.defaultdict(collections.Counter);changes=[]
for k,profile in enumerate(['XPath31','XPath40','XPath40Experimental']):
    for i,row in enumerate(inputs):
        b,a=before[k*len(inputs)+i],after[k*len(inputs)+i]
        assert a['profile']==b['profile']==profile
        assert a['expression']==b['expression']==row['expression']
        expected={'errorCode' if row['error'] else 'value':row['expected']}
        previous,current=outcome(b),outcome(a)
        for counter in [counts,groups[row['group']]]:
            counter['observations']+=1
            counter['beforeDiscrepancies']+=previous!=expected
            counter['afterDiscrepancies']+=current!=expected
            counter['changed']+=previous!=current
            counter['regressions']+=previous==expected and current!=expected
        assert current==expected,(profile,row,current)
        if previous!=current:
            assert previous!=expected
            changes.append(dict(profile=profile,**row,before=previous,after=current))
summary=dict(**counts,perGroup={k:dict(v) for k,v in groups.items()})
(here/'diagnostic-summary.json').write_text(json.dumps(summary,indent=2)+'\n')
(here/'diagnostic-changes.json').write_text(json.dumps(changes,indent=2)+'\n')
print(json.dumps(summary))
