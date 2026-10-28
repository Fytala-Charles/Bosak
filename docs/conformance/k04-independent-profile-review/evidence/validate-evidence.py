"""Validate C39's complete exclusion ledger, bounded observations and unchanged policy."""
from pathlib import Path
import json,hashlib,re,subprocess,collections
E=Path(__file__).resolve().parent;D=E.parent;R=E.parents[3];checks=[]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def check(ok,label):
 assert ok,label
 checks.append(label)
def lines(p):return [json.loads(x) for x in read(p).splitlines()]
def totals(p):return [int(re.search(k+r':\s+(\d+)',read(p))[1]) for k in ['Passed','Failed','Skipped']]
def skips(p):
 out={};reason=None
 for s in read(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',s)
  if m:reason=m[1]
  elif s.strip() and reason:check(s.strip() not in out,'unique skip '+s.strip());out[s.strip()]=reason
 return out
A=R/'docs/conformance/k04-pragma-admission/adoption'
for name in ['previous-identities.json','normal-source-identities.json','fixture-identities.json','diagnostic-fixture-identities.json','binary-identities.json']:
 for p,h in load(E/name).items():check(sha(R/p)==h,'preserved '+p)
check(not subprocess.check_output(['git','diff','2806ddac','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).strip(),'no production, policy, fixture or baseline change')
for p,h in load(A/'binary-identities.json').items():check(sha(R/p)==h,'same delivered C38 binary '+p)
fixturepin=load(R/'docs/conformance/k04-pragma-review/evidence/REVIEW-PIN.json')['fixtureRevisions']
for suite,revision in fixturepin.items():
 f=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(f),'rev-parse','HEAD'],text=True).strip()==revision,'pinned '+suite)
 check(not subprocess.check_output(['git','-C',str(f),'status','--porcelain'],text=True).strip(),'clean '+suite)
qt3=lines(E/'qt3-catalog.jsonl');qt4=lines(E/'qt4-catalog.jsonl')
check(len(qt3)==31821 and len({x['Set'] for x in qt3})==428 and len(qt4)==46985 and len({x['Set'] for x in qt4})==738,'78806 cases / 1166 sets inventoried')
old={x['Name']:x for x in lines(A/'adopted-catalog.jsonl')}
for x in qt3:
 y=old[x['Name']]
 check(all(x[k]==y[k] for k in ['Name','Set','File','ExpressionSha256','xpathSupported','xquerySupported','Dependencies']) and x['routingAllowed']==y['actual'],'actual QT3 inventory equals adopted input '+x['Name'])
 for profile in ['xpathSupported','xquerySupported']:check(x[profile]==all(d[profile] for d in x['Dependencies']),'singleton dependency reconciliation '+profile+' '+x['Name'])
for x in qt4:check(x['xpathSupported']==all(d['xpathSupported'] for d in x['Dependencies']) and not x['xquerySupported'] and not x['routingAllowed'],'actual XPath-only qt4 dependency conjunction '+x['Name'])
check(totals(E/'qt3-normal.log')==[31395,0,426] and totals(E/'qt4-normal.log')==[8939,0,1920],'fresh exact normal gates')
a=skips(E/'qt3-normal-skips.txt');q=skips(E/'qt4-normal-skips.txt')
check(a==skips(A/'qt3-normal-skips.txt')==load(E/'approved-skip-map.json') and q==skips(A/'qt4-normal-skips.txt'),'both complete approved skip maps unchanged')
ledger=load(E/'skip-ledger.json');check(len(ledger)==426 and {x['Name']:x['Reason'] for x in ledger}==a,'complete 426 named skip ledger')
index={x['Name']:x for x in qt3};cases=[x for x in ledger if x['Reason']=='Unsupported dependency']
check(len(cases)==370 and all(x['row']==index[x['Name']] and x['row']['routingAllowed'] and not x['row']['xpathSupported'] and not x['row']['xquerySupported'] for x in cases),'370 independently rejected cases / routing allowed')
expected={'earlier-spec-applicability':165,'declared-unsupported-optional-feature':93,'negative-on-frozen-admitted-feature':26,'xml-version-profile':39,'legacy-xsd-profile-review':37,'unreported-unicode-version':5,'negative-on-implemented-normalization':2,'different-default-language':1,'known-xib-polarity-candidate':2}
check(collections.Counter(x['Category'] for x in cases)==expected and load(E/'classification-summary.json')['categories']==expected,'all nine disposition categories reconcile exactly')
for x in cases:check(x['rejectedDependencies']==[d for d in x['row']['Dependencies'] if not d['xquerySupported']],'exact rejected singleton witnesses '+x['Name'])
check(sum(len(x['rejectedDependencies']) for x in cases)==371 and [x['Name'] for x in cases if len(x['rejectedDependencies'])>1]==['fn-transform-902'],'371 blocker occurrences / one overlapping case')
check(sum(x['Reason'].startswith('Harness error: XmlSchemaException') for x in ledger)==6,'six schema errors kept outside dependency categories')
selected=lines(E/'qt3-originals.jsonl');check(len(selected)==370 and {x['row']['Name'] for x in selected}=={x['Name'] for x in cases},'all 370 original queries/assertions/resources preserved')
for suite,rows in [('qt3',qt3),('qt4',qt4)]:
 hits=[x for x in rows if any(d['Type']=='language' and d['Value']=='xib' for d in x['Dependencies'])]
 check(len(hits)==2 and {x['Name'] for x in hits}=={'format-date-en151','format-dateTime-en151'} and all(x['gated'] and not x['xpathSupported'] and x['Name'] in (a if suite=='qt3' else q) for x in hits),'exact two already-gated xib originals '+suite)
 polarity=lines(E/(suite+'-polarity.jsonl'));check(len(polarity)==32,'32 actual polarity observations '+suite)
 for satisfied in [True,False]:
  x=next(x for x in polarity if (x['Type'],x['Value'],x['Satisfied'])==('language','xib',satisfied))
  check(x['xpathSupported']==satisfied and (x['xquerySupported']==satisfied if suite=='qt3' else not x['xquerySupported']),'current xib polarity '+suite+' '+str(satisfied))
diag=load(E/'diagnostic-receipts.json');actual=[x for x in diag if x['mode']=='actual'];controls=[x for x in diag if x['mode']=='public-xquery']
check(len(actual)==9 and collections.Counter(x['result']['kind'] for x in actual)=={'Passed':4,'Failed':5},'nine direct original assertion outcomes preserved')
check(all(x['result']['kind']=='Passed' for x in actual if x['case'].startswith('format-')),'all four unchanged xib assertions pass')
check(all(x['result']['kind']=='Failed' for x in actual if not x['case'].startswith('format-')),'normalization/default-language exclusions remain inapplicable')
check(len(controls)==12 and all(x['result']['kind']=='Passed' for x in controls),'twelve independent public language controls pass')
check(len([x for x in diag if x['mode']=='resources'])==9,'original resources loaded independently')
processes=diag+load(E/'gate-receipts.json')+load(E/'inventory-receipts.json');check(len(processes)==34 and all(x['exitCode']==0 and not x['timeout'] for x in processes),'34 externally bounded successful process exits / assertions inspected separately')
for x in load(E/'schema-context.json')['documents']:check(sha(R/x['snapshot'])==x['sha256'],'exact sibling documentation snapshot '+x['source'])
check('`Proposed`' in read(E/'schema-context/ADR-006-xsd11-engine-seam-compatibility.md.txt') and 'blocked on owner acceptance' in read(E/'schema-context/XSD11_S4_DESIGN_PREP.md.txt'),'schema callback remains proposed / S4c acceptance boundary preserved')
check(read(E/'core-req139-snapshot.txt') in read(R/'docs/FEATURE_REQUESTS.md') and '- [ ] ADR-006 accepted' in read(E/'core-req139-snapshot.txt'),'core REQ139 section unchanged and unaccepted')
for p,x in load(E/'source-inspection.json').items():check(sha(R/p)==x['sha256'],'source capability/policy witness '+p)
check(load(E/'candidate-assessment.json')['adoptionQualified']==False,'candidate is not an admission proposal')
check(len(re.findall(r'^  Starting:',read(E/'qt4-normal.log'),re.M))==230 and 'Experimental-level (XPath40Experimental) passes: 11' in read(E/'qt4-normal.log'),'qt4 gate membership unchanged')
render=load(E/'render-receipt.json');check(len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'ten HTML renders / unchanged CSS')
for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'MANIFEST.json').exists():
 m=load(E/'MANIFEST.json')
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw '+p)
 for p,h in m['gitObjects'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized '+p)
print(json.dumps({'status':'passed','checks':len(checks),'dependencyCases':370,'blockerOccurrences':371,'candidateOriginalsPassed':4,'publicControlsPassed':12,'normalBaselineChanged':False}))
