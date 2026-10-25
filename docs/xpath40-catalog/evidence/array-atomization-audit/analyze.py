from pathlib import Path
import json,re,hashlib,collections,subprocess
r=Path('D:/Development/Bosak');s=r/'.guard-tmp/req126-array-atomization-audit-source';e=r/'.guard-tmp/req126-array-atomization-audit-evidence'
inputs=json.loads((e/'inputs.json').read_text(encoding='utf-8'));observations=json.loads((e/'observations.json').read_text(encoding='utf-8'));assert len(inputs)==192 and len(observations)==1152
def outcome(v):
 if v.get('error') is None:return {'value':v['value']}
 m=re.search(r'\b[A-Z]{4}\d{4}\b',v['error']);return {'errorCode':m[0] if m else v['error'],'exceptionType':v['exceptionType']}
def expected(v):
 flat=v['flatIntegers'];context=v['context'];n=len(flat)
 def seq(items,kind):return {'value':';'.join(str(x).lower()+'|'+kind for x in items)}
 if context=='data':return {'value':','.join(map(str,flat))+'|string'}
 if context=='general-eq':return seq([2 in flat],'boolean')
 if context=='castable':return seq([n<=1],'boolean')
 if n>1:return {'errorCode':'XPTY0004'}
 if n==0:return {'value':''}
 a=flat[0]
 if context=='value-eq':return seq([a==2],'boolean')
 if context=='range':return seq(range(a,4),'integer')
 return seq([-a if context=='unary-minus' else abs(a) if context=='function-argument' else a],'integer')
def conforms(actual,wanted):return all(actual.get(k)==v for k,v in wanted.items())
rows=[];perContext=collections.defaultdict(lambda:dict(pairs=0,matches=0,discrepancies=0,actualMatchesIndependentExpectation=0,actualDiscrepancies=0,referenceDiscrepancies=0));prior={}
for k,profile in enumerate(['XPath31','XPath40','XPath40Experimental']):
 for i,v in enumerate(inputs):
  a,b=observations[k*384+i*2:k*384+i*2+2]
  assert a['profile']==b['profile']==profile and a['expression']==v['expression'] and b['expression']==v['referenceExpression']
  actual=outcome(a);reference=outcome(b);match=actual==reference
  wanted=expected(v);actualCorrect=conforms(actual,wanted);referenceCorrect=conforms(reference,wanted)
  row={**v,'profile':profile,'actual':actual,'reference':reference,'independentExpected':wanted,'matchesFlatReference':match,'actualMatchesIndependentExpectation':actualCorrect,'referenceMatchesIndependentExpectation':referenceCorrect};rows.append(row)
  c=perContext[v['context']];c['pairs']+=1;c['matches' if match else 'discrepancies']+=1
  c['actualMatchesIndependentExpectation' if actualCorrect else 'actualDiscrepancies']+=1
  if not referenceCorrect:c['referenceDiscrepancies']+=1
  if k==0:prior[i]=(actual,reference)
  else:assert prior[i]==(actual,reference),'Profile-dependent outcome requires separate interpretation'
inventory=[];method='';f=s/'src/Bosak.XPath.Runtime/Vm/VmEngine.cs'
for lineNo,line in enumerate(f.read_text(encoding='utf-8').splitlines(),1):
 m=re.search(r'(?:private|internal|public)\s+static\s+\S+\s+(\w+)\(',line)
 if m:method=m[1]
 if re.search(r'\bAtomize\(',line):inventory.append({'line':lineNo,'enclosingMethod':method,'source':line.strip()})
(e/'comparisons.json').write_text(json.dumps(rows,indent=2)+'\n',encoding='utf-8')
(e/'caller-inventory.json').write_text(json.dumps({'path':'src/Bosak.XPath.Runtime/Vm/VmEngine.cs','gitBlob':subprocess.check_output(['git','hash-object','src/Bosak.XPath.Runtime/Vm/VmEngine.cs'],cwd=s,text=True).strip(),'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'references':inventory,'note':'Textual references including declaration/recursive calls; enclosing methods are nearest preceding static declarations, not a whole-program call graph. AtomizeForCast/AtomizeItems and standard-library data/conversion paths are separate.'},indent=2)+'\n',encoding='utf-8')
summary={'shapes':16,'contexts':12,'profiles':3,'pairs':576,'rawObservations':1152,'matches':sum(v['matchesFlatReference'] for v in rows),'discrepancies':sum(not v['matchesFlatReference'] for v in rows),'allProfilesAgree':True,'perContext':dict(perContext),'textualAtomizeReferences':len(inventory),'classification':'Diagnostic comparisons with independently flattened pure-integer/empty shape inputs; not passing native conformance assertions. Equal error codes do not certify all error precedence.','engineChanged':False}
summary.update(actualMatchesIndependentExpectation=sum(v['actualMatchesIndependentExpectation'] for v in rows),actualDiscrepancies=sum(not v['actualMatchesIndependentExpectation'] for v in rows),referenceDiscrepancies=sum(not v['referenceMatchesIndependentExpectation'] for v in rows),independentExpectation='Python recursive flattening plus explicit empty/singleton/multiple and integer/boolean/string result rules; does not trust the processor reference as an oracle.')
(e/'diagnostic-summary.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8');print(json.dumps(summary,indent=2))
