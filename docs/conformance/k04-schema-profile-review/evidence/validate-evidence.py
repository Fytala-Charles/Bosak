"""Verify the bounded C41 schema-profile audit and unchanged implementation/admission."""
from pathlib import Path
import json,hashlib,re,subprocess,collections
E=Path(__file__).resolve().parent;D=E.parent;R=E.parents[3];checks=[]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def lines(p):return [json.loads(x) for x in read(p).splitlines()]
def check(ok,label):
 assert ok,label
 checks.append(label)
def totals(p):return [int(re.search(k+r':\s+(\d+)',read(p))[1]) for k in ['Passed','Failed','Skipped']]
def skips(p):
 out={};reason=None
 for s in read(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',s)
  if m:reason=m[1]
  elif s.strip() and reason:check(s.strip() not in out,'unique skip '+s.strip());out[s.strip()]=reason
 return out
A=R/'docs/conformance/k04-xib-admission/adoption'
for name in ['previous-identities.json','normal-source-identities.json','fixture-identities.json','reviewed-resource-identities.json','binary-identities.json']:
 for p,h in load(E/name).items():check(sha(R/p)==h,'preserved '+p)
check(not subprocess.check_output(['git','diff','f0cba471','--name-only','--','src','tests','AGENTS.md'],cwd=R,text=True).strip(),'no implementation, policy, fixture, baseline or version change')
for p,h in load(A/'binary-identities.json').items():
 if p.startswith('tests/'):check(sha(R/p)==h,'same adopted normal binary '+p)
pin=load(R/'docs/conformance/k04-pragma-review/evidence/REVIEW-PIN.json')['fixtureRevisions']
for suite,revision in pin.items():
 f=R/'tests'/('qt3tests' if suite=='qt3' else 'qt4tests')
 check(subprocess.check_output(['git','-C',str(f),'rev-parse','HEAD'],text=True).strip()==revision,'pinned '+suite)
 check(not subprocess.check_output(['git','-C',str(f),'status','--porcelain'],text=True).strip(),'clean '+suite)
catalog=lines(E/'qt3-catalog.jsonl');old=lines(A/'qt3-adopted-catalog.jsonl');check(len(catalog)==31821 and len({x['Set'] for x in catalog})==428 and catalog==old,'complete adopted catalog/filter observations unchanged')
c=load(E/'cohorts.json');legacy=set(c['legacy']);errors=set(c['schemaPreparationErrors']);companions={x['companion'] for x in c['pairs']};names=legacy|errors|companions
check(len(legacy)==37 and len(errors)==6 and len(companions)==9 and len(names)==52 and set(c['allDiagnostics'])==names,'exact disjoint 37 / six / nine cohorts')
originals=lines(E/'qt3-originals.jsonl');index={x['row']['Name']:x for x in originals};check(len(originals)==52 and set(index)==names,'all 52 original expressions/assertions/resources preserved')
for n in legacy:check(not index[n]['row']['xpathSupported'] and not index[n]['row']['xquerySupported'] and index[n]['row']['routingAllowed'],'independent legacy profile exclusion '+n)
for n in errors|companions:check(index[n]['row']['routingAllowed'] and index[n]['row']['xquerySupported'],'admitted current-profile companion/preparation case '+n)
for p in c['pairs']:check(index[p['legacy']]['Expression']==index[p['companion']]['Expression'] and index[p['legacy']]['row']['ExpressionSha256']==index[p['companion']]['row']['ExpressionSha256']==p['expressionSha256'],'exact same query for opposing profile assertions '+p['legacy'])
diag=load(E/'diagnostic-receipts.json');check(len(diag)==104,'104 original/resource process receipts')
actual={x['case']:x['result'] for x in diag if x['mode']=='actual'}
check(collections.Counter(actual[n]['kind'] for n in legacy)=={'Passed':15,'Failed':22},'legacy assertions 15 pass / 22 fail')
check(all(actual[n]['kind']=='Passed' for n in companions),'all nine current-profile companions pass')
for n in errors:check(actual[n]['kind']=='HarnessException' and actual[n]['exceptionType']=='System.Xml.Schema.XmlSchemaException' and 's:H1 s:H2' in actual[n]['message'],'six preserved multi-affiliation preparation errors '+n)
ledger=load(E/'cohort-ledger.json');check(len(ledger)==52 and {x['Name'] for x in ledger}==names,'complete named disposition ledger')
for x in ledger:check(x['Original']==index[x['Name']] and x['Outcome']==actual[x['Name']],'exact original and outcome witness '+x['Name'])
check(collections.Counter(x['Disposition'] for x in ledger)=={'legacy-intrinsic-counter-profile-mismatch':18,'legacy-direct-pass-retain-profile-exclusion':15,'existing-core-substitution-contract-gap':4,'xsd11-multiple-affiliation-preparation-boundary':6,'current-profile-companion-passes':9},'all disposition counts reconcile')
framework=load(E/'framework-results.json');check(framework['substitutionGroupPropertyType']=='System.Xml.XmlQualifiedName','single-head .NET component property observed')
f={x['name']:x for x in framework['schemas']};check(f['substitution.xsd']['compiled'] and f['substitution.xsd']['globalElementCount']==9,'unchanged XSD 1.0 schema compiles')
check(not f['substitution11.xsd']['compiled'] and f['substitution11.xsd']['stage']=='Add' and 's:H1 s:H2' in f['substitution11.xsd']['message'],'unchanged multiple-affiliation resource fails in framework Add')
matching=load(E/'matching-results.json');nodes=matching['nodeControls'];check(len(nodes)==10 and all(x['schemaValidated'] for x in nodes),'ten independently validated XSD 1.0 node controls')
check({x['name'] for x in nodes if not x['passed']}=={'A','A-own','E','G'} and sum(x['passed'] for x in nodes)==6,'four independent abstract/blocked-membership mismatches / six positive-edge witnesses')
check(all(x['error'] is None for x in nodes) and not matching['functionControl']['passed'] and matching['functionControl']['actual']==False and matching['functionControl']['expected']==True and matching['functionControl']['error'] is None,'function subtype mismatch is a result, not preparation failure')
initial=load(E/'matching-results-initial.json');check([(x['name'],x['expected'],x['actual'],x['passed']) for x in initial['nodeControls']]==[(x['name'],x['expected'],x['actual'],x['passed']) for x in nodes] and initial['functionControl']==matching['functionControl'],'probe refinement changed only metadata serialization')
processes=diag+[load(E/'inventory-receipt.json'),load(E/'framework-receipt.json'),load(E/'matching-receipt.json')]+load(E/'gate-receipts.json');check(len(processes)==109 and all(x['exitCode']==0 and not x['timeout'] for x in processes),'109 final bounded process exits / assertion and preparation outcomes inspected separately')
check(totals(E/'qt3-normal.log')==[31397,0,424] and totals(E/'qt4-normal.log')==[8941,0,1918],'fresh exact normal gates')
a=skips(E/'qt3-normal-skips.txt');q=skips(E/'qt4-normal-skips.txt');check(a==skips(A/'qt3-normal-skips.txt') and q==skips(A/'qt4-normal-skips.txt'),'both complete normal skip maps unchanged')
check(all(a[n]=='Unsupported dependency' for n in legacy) and all(a[n].startswith('Harness error: XmlSchemaException') for n in errors),'cohort reasons remain distinct in gate accounting')
for x in load(E/'schema-context.json')['documents']:check(sha(R/x['snapshot'])==x['sha256'],'exact current sibling documentation snapshot '+x['source'])
check('`Proposed`' in read(E/'schema-context/ADR-006-xsd11-engine-seam-compatibility.md.txt') and 'blocked on owner acceptance' in read(E/'schema-context/XSD11_S4_DESIGN_PREP.md.txt'),'schema seam proposal/owner gate remains pending')
snapshot=read(E/'core-req139-snapshot.txt');check(snapshot in read(R/'docs/FEATURE_REQUESTS.md') and '- [ ] ADR-006 accepted' in snapshot,'REQ139 pending text and acceptance criteria preserved')
check('31,145/0/676' in snapshot and '31,397/0/424' in load(E/'readiness-reconciliation.json')['draftAcceptanceBaseline'],'stale draft baseline explicitly identified without silent rewrite')
for p,x in load(E/'source-inspection.json').items():check(sha(R/p)==x['sha256'],'current core surface/implementation witness '+p)
check('IEnumerable<Stream>' in read(R/'src/Bosak.XPath.Api/Xsd/IXsdValidator.cs') and 'XmlSchemaSet' not in read(R/'src/Bosak.XPath.Api/Xsd/IXsdValidator.cs'),'stream validation facade distinguished from component-model seam')
check('Last updated: 2026-10-29' in read(R/'docs/FEATURE_REQUESTS.md'),'registry current date updated')
check(len(re.findall(r'^  Starting:',read(E/'qt4-normal.log'),re.M))==230 and 'Experimental-level (XPath40Experimental) passes: 11' in read(E/'qt4-normal.log'),'qt4 gate membership unchanged')
render=load(E/'render-receipt.json');check(len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'ten HTML renders / unchanged CSS')
for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'MANIFEST.json').exists():
 m=load(E/'MANIFEST.json')
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw '+p)
 for p,h in m['gitObjects'].items():check(subprocess.check_output(['git','hash-object',p],cwd=R,text=True).strip()==h,'normalized '+p)
print(json.dumps({'status':'passed','checks':len(checks),'legacyCases':37,'schemaPreparationCases':6,'pairedCompanionsPassed':9,'currentProfileControlMismatches':5,'normalBaselineChanged':False}))
