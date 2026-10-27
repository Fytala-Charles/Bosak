from pathlib import Path
import json,collections
C=Path(__file__).resolve().parent; E=C/'evidence'
def load(n): return json.loads((E/n).read_text(encoding='utf-8-sig'))
scope=load('inventory-scope.json'); meta=load('metadata-results.json'); comparisons=load('comparison-results.json'); boundaries=load('boundary-results.json'); cases={x['id']:x for x in load('schema-cases.json')}
assert len(meta)==902 and len(comparisons)==648 and len(boundaries)==34
for row in meta+comparisons+boundaries:
 assert row['exitCode']==0 and not row['timeout'] and row['watchdogSeconds']==20 and row['result'] and not row.get('budgetExceeded',False)
eligible={x['id']:x['result'] for x in meta if x['result']['deferred']}; assert len(eligible)==324
pairs=collections.defaultdict(dict)
for row in comparisons: pairs[row['id']][row['mode']]=row['result']
assert set(pairs)==set(eligible)
for name,pair in pairs.items():
 assert set(pair)=={'current','guarded'}
 assert pair['guarded']['deferred'] and pair['guarded']['decision']==eligible[name]['decision']
 assert pair['current']['expression']==pair['guarded']['expression']==eligible[name]['expression']
 assert pair['current']['assertion']==pair['guarded']['assertion']==eligible[name]['assertion']
transitions=collections.Counter(p['current']['kind']+' -> '+p['guarded']['kind'] for p in pairs.values())
assert transitions=={'Passed -> Passed':251,'Passed -> Failed':72,'HarnessException -> Passed':1}
bp=collections.defaultdict(dict)
for row in boundaries: bp[row['id']][row['mode']]=row['result']
md={x['id']:x['result'] for x in meta}
for name,p in bp.items():
 assert all(p['current'][k]==p['guarded'][k] for k in ['kind','message','exceptionType','expression','assertion'])
 assert p['guarded']['decision']==md[name]['decision'] and not p['guarded']['deferred']
regressions=[]
for name,pair in pairs.items():
 if pair['guarded']['kind']!='Failed':continue
 group='K02-C02' if name in ['qischema032','qischema032a','qischema032b','qischema032c'] else 'K02-C01'
 regressions.append(dict(id=name,followUp=group,request=cases[name],current=pair['current'],guarded=pair['guarded']))
assert collections.Counter(x['followUp'] for x in regressions)=={'K02-C01':68,'K02-C02':4}
traces=load('mechanism-traces.json'); assert len(traces)==4
for t in traces[:2]:assert t['typeAfterFirst'] and not t['functionAfterFirst'] and t['functionAfterSecond'] and t['error1'].startswith('XPST0051:') and t['error2'] is None
assert not traces[2]['compiled'] and traces[3]['compiled'] and traces[3]['events']==[]
summary=dict(schemaCases=1010,baselinePassing=895,baselineSkipped=115,inspected=902,excluded=108,eligible=324,metadataProcesses=902,pairedProcesses=648,boundaryProcesses=34,totalWatchdogProcesses=1584,timeouts=0,transitions=dict(transitions),guardReasons=dict(collections.Counter(x['result']['decision'] for x in meta)),regressionGroups={'K02-C01':68,'K02-C02':4})
result=dict(date='2026-10-27',sourceBase=scope['sourceBase'],status='Normal adoption blocked by 72 guarded-mode regressions',summary=summary,scope=scope,regressions=regressions,boundaries=boundaries,mechanismTraces=traces,followUps=[dict(id='K02-C01',status='Open',next='Register imported schema constructors after schema compilation without repopulating all standard functions; preserve host bindings, collision errors and population policy. Cover direct calls, named references, function-lookup, list/union types and fresh-context evaluation. Re-run all 324 pairs.'),dict(id='K02-C02',status='Open',next='Preserve schema source/base URI for relative imports, starting with file-backed resolver streams. Specify non-file stream identity separately. Re-run all 324 pairs before proposing normal adoption.')],validation=dict(fresh='1584 isolated metadata/comparison/boundary processes with 20-second external watchdogs, plus four mechanism controls',inherited='Source-identical ff1fd574: 21825 unit passes; QT3 31144/0/677; qt4 8436/0/1894. Full suites were not rerun for this documentation audit.'),policy='Default-off guarded diagnostics only. No source, fixture, assertion, routing, dependency, skip, baseline, version, release or REQ-126 acceptance change. Paired results compare assertion outcomes, not byte-for-byte result equivalence or full XSD capability.')
(C/'RESULTS.json').write_text(json.dumps(result,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
print(json.dumps(summary))
