from pathlib import Path
import json,re,collections,hashlib,subprocess
E=Path(__file__).resolve().parent;R=E.parents[3]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sm(p):
 out={};reason=None
 for l in p.read_text(encoding='utf-8-sig').splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',l)
  if m:reason=m[1]
  elif l.strip() and reason:out[l.strip()]=reason
 return out
rows=[json.loads(l) for l in (E/'normal-affected.jsonl').read_text(encoding='utf-8').splitlines()];names={x['row']['Name'] for x in rows};excluded={x['row']['Name'] for x in rows if not x['row']['xquerySupported']}
a=sm(E/'qt3-normal-skips.txt');b=sm(E/'qt3-prototype-skips.txt');receipts=load(E/'original-receipts.json');failed={x['case'] for x in receipts if x['phase']=='prototype' and x['mode']=='actual' and x['result']['kind']=='Failed'};passed=names-failed
gateFailed=set(re.findall(r'^  FAIL ([^:]+):',(E/'qt3-prototype.log').read_text(encoding='utf-8-sig'),re.M))
assert len(names)==87 and len(excluded)==4 and len(failed)==83 and len(passed)==4
assert set(a)-set(b)==names-excluded and len(gateFailed)==79 and gateFailed==failed-excluded
assert not set(b)-set(a) and all(a[n]==b[n] for n in b)
summary=dict(affectedCases=87,constructionOnly=75,mixed=12,affectedSets=dict(collections.Counter(x['row']['Set'] for x in rows)),dependencyExcluded=sorted(excluded),prototypePassedOriginals=sorted(passed),prototypeFailedOriginals=sorted(failed),gateFailedOriginals=sorted(gateFailed),removedSkips=sorted(set(a)-set(b)),retainedCount=481,retainedReasonsUnchanged=True,normalQt3=[31257,0,564],prototypeQt3=[31261,79,481],normalQt4=[8939,0,1920],prototypeExit=2,admissionQualified=False)
(E/'review-analysis.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
fixtures=set(load(R/'docs/conformance/k04-copy-namespaces-admission/evidence/fixture-identities.json'))
def paths(v):
 if isinstance(v,dict):
  for k,x in v.items():
   if k=='FilePath' and x:
    p=Path(x).resolve();assert p.is_file();fixtures.add(p.relative_to(R).as_posix())
   else:paths(x)
 elif isinstance(v,list):
  for x in v:paths(x)
for x in receipts:
 if x['mode']=='resources':paths(x['result'])
fixtures.add((E/'controls/controls.xml').relative_to(R).as_posix())
(E/'fixture-identities.json').write_text(json.dumps({p:hashlib.sha256((R/p).read_bytes()).hexdigest() for p in sorted(fixtures)},indent=2)+'\n',encoding='utf-8')
sourcefiles=list((R/'tests/Bosak.XPath.Conformance').glob('*.cs'))+list((R/'tests/Bosak.XPath.Conformance40').glob('*.cs'))+list((R/'src/Bosak.XQuery').rglob('*.cs'))+[R/'src/Bosak.XPath.Providers/XDocument/XDocumentProvider.cs',R/'src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs',R/'src/Bosak.XPath.Providers/XDocument/XdmSchemaAnnotator.cs',R/'AGENTS.md',R/'src/Directory.Build.props']
sourcefiles=[p for p in sourcefiles if not {'obj','bin'}&set(p.parts)]
(E/'normal-source-identities.json').write_text(json.dumps({p.relative_to(R).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in sourcefiles},indent=2)+'\n',encoding='utf-8')
binaries={}
for folder in ['tests/Bosak.XPath.Conformance/bin/Release/net10.0','tests/Bosak.XPath.Conformance40/bin/Release/net10.0','.guard-tmp/k04-construction-harness/bin/Release/net10.0','.guard-tmp/k04-construction-driver/bin/Release/net10.0','.guard-tmp/k04-construction-inventory/bin/Release/net10.0','.guard-tmp/k04-construction-typing-probe/bin/Release/net10.0']:
 for p in (R/folder).glob('*.dll'):binaries[p.relative_to(R).as_posix()]=hashlib.sha256(p.read_bytes()).hexdigest()
(E/'binary-identities.json').write_text(json.dumps(binaries,indent=2)+'\n',encoding='utf-8')
print('87 originals reconciled: 79 gated failures, four accepted alternatives, four retained dependencies.')
