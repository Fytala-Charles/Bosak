"""Rebuild the guarded-loader diagnostic review from immutable process receipts."""
from pathlib import Path
import json,collections
C=Path(__file__).resolve().parent;E=C/'evidence'
def load(n):return json.loads((E/n).read_text(encoding='utf-8-sig'))
prototype=load('results.json');actual=load('implementation-results.json');original=load('actual-results.json');requests=load('implementation-requests.json');v1=load('v1-results.json')
assert len(v1)==56 and len(prototype)==64 and len(actual)==66 and len(original)==10
for rows in [v1,prototype,actual,original]:assert all(v['exitCode']==0 and not v['timeout'] and v['watchdogSeconds']==20 and v['result'] for v in rows)
proto={(v['id'],v['mode']):v['result'] for v in prototype};implemented={(v['id'],v['mode']):v['result'] for v in actual}
assert set(implemented)=={(v['id'],m) for v in requests for m in v['modes']}
for key,p in proto.items():
 if key[1]=='unsafe-control':continue
 a=implemented[key]
 assert all(a[k]==p[k] for k in ['kind','message','exceptionType','deferred','decision','expression','assertion']),key
for v in original:
 p=implemented[(v['id'],'current')];assert p['kind']==v['result']['kind'] and p['message']==(v['result'].get('message') or '')
assert implemented[('validateexpr-28','current')]['kind']=='HarnessException' and implemented[('validateexpr-28','guarded')]['kind']=='Passed'
for i in range(20,26):
 id=f'substitution-0{i}';assert implemented[(id,'current')]['kind']==implemented[(id,'guarded')]['kind']=='HarnessException'
 assert implemented[(id,'guarded')]['decision']=='DeclaredXsdProfileNotReviewed'
for id in ['wrong-assertion','wrong-error']:assert implemented[(id,'guarded')]['kind']=='Failed'
for id in ['no-import-unsafe','partial-import-unsafe']:
 assert proto[(id,'unsafe-control')]['kind']=='Passed' and proto[(id,'guarded')]['kind']=='HarnessException'
for mode in ['current','guarded']:
 assert implemented[('valid-import',mode)]['reuse']==dict(kind='Passed',deferred=False,decision='NoEnvironment')
assert implemented[('prolog-module-guard','guarded')]['decision']=='ModulesNotReviewed'
assert implemented[('module-keyword-string','guarded')]['decision']=='QueryOwnedImportSubset' and implemented[('module-keyword-string','guarded')]['kind']=='Passed'
summary=dict(originalCases=10,prototypeV1Processes=56,prototypeV2Processes=64,actualBaselineProcesses=10,implementedProcesses=66,implementedExecutorCalls=68,allDiagnosticProcesses=196,timeouts=0,implementedRequests=33,unitTestsPassed=21825,qt3=[31144,0,677],qt4=[8436,0,1894],normalSkipNamesReasonsUnchanged=677,sourceCandidate='1.26.6',publishedVersion='1.26.3',defaultMode='Current eager initialization',diagnosticMode='Explicit opt-in, guarded query-owned imports',normalAdmissionChanged=False)
out=dict(date='2026-10-27',sourceBase=load('IMPLEMENTATION-PIN.json')['sourceBase'],status='Guarded diagnostic mode verified; normal admission and schema capability remain unchanged',summary=summary,implemented=actual,prototypeV1=v1,prototypeV2=prototype,productionBaseline=original,decisions={v['id']:dict(current=implemented[(v['id'],'current')],guarded=implemented[(v['id'],'guarded')]) for v in requests},remaining=['Review applicability across the complete eligible schema/import inventory before any default-mode proposal.','Explicit XSD-profile metadata and the six multi-affiliation cases remain unsupported/unreviewed by this mode.','Any normal-runner admission or baseline change requires separate evidence and explicit policy approval.'],policy='No private engine-field reflection in the implemented ownership path, no unsafe bypass in the real harness, no fixture/assertion/filter/routing/retry/baseline change, and no claim of added XSD1.1 validation capability.')
(C/'RESULTS.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');print(json.dumps(summary))
