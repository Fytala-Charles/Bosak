from pathlib import Path
import json,re,collections,hashlib,subprocess
E=Path(__file__).resolve().parent;R=E.parents[3]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def skipmap(p):
 out={};reason=None
 for line in p.read_text(encoding='utf-8-sig').splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',line)
  if m:reason=m[1]
  elif line.strip() and reason:out[line.strip()]=reason
 return out
rows=[json.loads(l) for l in (E/'normal-affected.jsonl').read_text(encoding='utf-8').splitlines()];index={x['row']['Name']:x for x in rows}
receipts=load(E/'original-receipts.json');actual=[x for x in receipts if x['phase']=='prototype' and x['mode']=='actual'];failed={x['case'] for x in actual if x['result']['kind']=='Failed'};passed={x['case'] for x in actual if x['result']['kind']=='Passed'}
normal=skipmap(E/'qt3-normal-skips.txt');prototype=skipmap(E/'qt3-prototype-skips.txt');removed=set(normal)-set(prototype);excluded={x['row']['Name'] for x in rows if not x['row']['xquerySupported']}
gateFailed=set(re.findall(r'^  FAIL ([^:]+):', (E/'qt3-prototype.log').read_text(encoding='utf-8-sig'),re.M))
assert len(rows)==136 and len(failed)==130 and len(passed)==6
assert removed==set(index)-excluded and len(removed)==132 and len(excluded)==4
assert gateFailed==failed-excluded and len(gateFailed)==126
assert all(normal[n]==prototype[n] for n in prototype) and not set(prototype)-set(normal)
summary=dict(affectedCases=136,affectedSets=dict(collections.Counter(x['row']['Set'] for x in rows)),constructionOnly=75,copyOnly=49,both=12,dependencyExcluded=sorted(excluded),normalDirect={'Passed':5,'Skipped':131},prototypeDirect={'Passed':6,'Failed':130},normalQt3=[31208,0,613],prototypeQt3=[31214,126,481],normalQt4=[8939,0,1920],prototypeGateExit=2,removedSkips=sorted(removed),retainedSkipCount=481,retainedSkipReasonsUnchanged=True,prototypePassedOriginals=sorted(passed),prototypeFailedOriginals=sorted(failed),gateFailedOriginals=sorted(gateFailed),publicControls=dict(collections.Counter(x['result']['kind'] for x in load(E/'control-receipts.json'))),lexicalControls=len(load(E/'lexical-controls.json')),admissionQualified=False)
(E/'review-analysis.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
files=set(load(R/'docs/conformance/k04-context-routing-review/evidence/fixture-identities.json'))
def paths(value):
 if isinstance(value,dict):
  for k,v in value.items():
   if k=='FilePath' and v:
    p=Path(v).resolve();assert p.is_file(),str(p);files.add(p.relative_to(R).as_posix())
   else:paths(v)
 elif isinstance(value,list):
  for item in value:paths(item)
for receipt in receipts:
 if receipt['mode']=='resources':paths(receipt['result'])
files.add((E/'controls/controls.xml').relative_to(R).as_posix())
(E/'fixture-identities.json').write_text(json.dumps({p:hashlib.sha256((R/p).read_bytes()).hexdigest() for p in sorted(files)},indent=2)+'\n',encoding='utf-8')
sourcefiles=list((R/'tests/Bosak.XPath.Conformance').glob('*.cs'))+list((R/'tests/Bosak.XPath.Conformance40').glob('*.cs'))+list((R/'src/Bosak.XQuery').rglob('*.cs'))+[R/'AGENTS.md',R/'src/Directory.Build.props']
sourcefiles=[p for p in sourcefiles if not {'obj','bin'}&set(p.parts)]
(E/'normal-source-identities.json').write_text(json.dumps({p.relative_to(R).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in sourcefiles},indent=2)+'\n',encoding='utf-8')
binaries={}
for folder in ['tests/Bosak.XPath.Conformance/bin/Release/net10.0','tests/Bosak.XPath.Conformance40/bin/Release/net10.0','.guard-tmp/k04-prolog-mode-harness/bin/Release/net10.0','.guard-tmp/k04-prolog-mode-driver/bin/Release/net10.0','.guard-tmp/k04-prolog-mode-inventory/bin/Release/net10.0']:
 for p in (R/folder).glob('*.dll'):binaries[p.relative_to(R).as_posix()]=hashlib.sha256(p.read_bytes()).hexdigest()
(E/'binary-identities.json').write_text(json.dumps(binaries,indent=2)+'\n',encoding='utf-8')
print('All 126 full-gate failures reconcile with direct probes; four independent exclusions retained.')
