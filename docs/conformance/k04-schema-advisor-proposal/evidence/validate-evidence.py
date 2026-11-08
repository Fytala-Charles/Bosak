"""Validate C43's read-only source audit, original cohort mappings and exact no-provider gates."""
from pathlib import Path
import json,hashlib,subprocess,re,collections,xml.etree.ElementTree as X
E=Path(__file__).resolve().parent;D=E.parent;R=E.parents[3];P=R/'docs/conformance/k04-schema-element-fix/evidence';checks=[]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def check(ok,label):
 assert ok,label
 checks.append(label)
def git(*a):return subprocess.check_output(['git',*a],cwd=R,text=True).strip()
def totals(p):return [int(re.search(k+r':\s+(\d+)',read(p))[1]) for k in ['Passed','Failed','Skipped']]
def skips(p):
 out={};reason=None
 for line in read(p).splitlines():
  m=re.match(r'=== \d+ skipped: (.*?) ===',line)
  if m:reason=m[1]
  elif line.strip() and reason:
   assert line.strip() not in out
   out[line.strip()]=reason
 return out
pin=load(E/'REVIEW-PIN.json');check(pin['baseCommit'].startswith('ebfb7c2f') and not pin['implementationAuthorized'] and pin['candidate']=='1.33.1','read-only review pin; no implementation adoption')
check(not git('diff',pin['baseCommit'],'--name-only','--','src','tests','AGENTS.md'),'no product, harness, fixture, version or baseline changes')
for name in ['previous-identities.json','binary-identities.json','xslt-resource-identities.json']:
 for p,h in load(E/name).items():check(sha(R/p)==h,'preserved '+p)
for p,item in load(E/'core-source-identities.json').items():check(sha(R/p)==item['sha256'] and git('hash-object','--',p)==item['gitObject'],'actual core surface identity '+p)
ctx=load(E/'schema-context.json');S=Path(ctx['root'])
check(git('-c','safe.directory=D:/Development/Bosak.Schema','-C',str(S),'rev-parse','HEAD')==ctx['head'],'same reviewed Schema HEAD')
check(git('-c','safe.directory=D:/Development/Bosak.Schema','-C',str(S),'status','--short')==ctx['workingTreeStatus'],'same sibling working-tree status; no mutation')
for x in ctx['documents']:check(sha(S/x['source'])==x['sha256']==sha(R/x['snapshot']),'exact current Schema document '+x['source'])
for p,h in ctx['sourceSha256'].items():check(sha(S/p)==h,'current reviewed commercial source identity '+p)
model=read(S/'src/Bosak.Schema/Model/ElementDeclaration.cs');compiler=read(S/'src/Bosak.Schema/Compilation/SchemaCompiler.cs');check('string? NodePath' in model and all(s not in model for s in ['SubstitutionGroup','IsAbstract','IsNillable','BlockResolved']),'observed element model lacks required member facts')
check('name is null || type is null' in compiler and 'ComputeDigest(document)' in compiler,'explicit-only element capture and root-document digest source witnesses')
check(all('substitutionGroup' not in read(S/p) and 'SubstitutionGroup' not in read(S/p) for p in ctx['sourceSha256'] if p.endswith('.cs')),'no affiliation-list implementation token in reviewed product source')
check('`Proposed`' in read(S/'docs/ADR-006-xsd11-engine-seam-compatibility.md') and 'blocked on owner acceptance' in read(S/'docs/XSD11_S4_DESIGN_PREP.md'),'ADR006/S4c owner gate remains pending')
snapshot=read(R/'docs/conformance/k04-schema-profile-review/evidence/core-req139-snapshot.txt');check(snapshot in read(R/'docs/FEATURE_REQUESTS.md') and '- [ ] ADR-006 accepted' in snapshot,'original REQ139 Pending draft/criteria and historical baseline preserved')
for n,revision in load(E/'fixture-revisions.json').items():
 check(git('-C','tests/'+n,'rev-parse','HEAD')==revision,'pinned fixture revision '+n)
 if n!='xslt30-test':check(not git('-C','tests/'+n,'status','--porcelain'),'clean '+n)
working=load(E/'xslt-working-tree.json');check(git('-C','tests/xslt30-test','status','--porcelain')==working['workingTreeStatus'],'observed unrelated XSLT checkout changes preserved without cleanup')
for path,item in working['reviewedResources'].items():
 rel=Path(path).relative_to('tests/xslt30-test').as_posix();blob=subprocess.check_output(['git','-C','tests/xslt30-test','show','HEAD:'+rel],cwd=R);current=(R/path).read_bytes()
 check(current.replace(b'\r\n',b'\n')==blob.replace(b'\r\n',b'\n') and hashlib.sha256(current.replace(b'\r\n',b'\n')).hexdigest()==item['lfNormalizedSha256'] and sha(R/path)==item['workingRawSha256'],'reviewed XSLT resource matches pinned HEAD content '+path)
rows=load(E/'xslt-originals.json');ledger=load(E/'xslt-ledger.json');receipts=load(E/'xslt-receipts.json');index={x['Name']:x for x in rows};byName={x['name']:x for x in receipts}
cohort={x['Name'] for x in rows if x['cohort']=='documented-30'};check(len(rows)==32 and len(cohort)==30 and set(index)-cohort=={'catalog-005b','validation-1301'},'30 documented targets plus separate applicability/CTA controls')
for n in cohort|{'validation-1301'}:
 matches=[x for x in byName[n]['observations'] if x['name']==n];check(len(matches)==1 and matches[0]['outcome']=='SKIP' and 'XSD 1.1' in matches[0]['detail'],'exact target reaches XSD1.1 guard '+n)
check('stylesheet declares xs:assert/xs:alternative' in byName['validation-1301']['observations'][0]['detail'],'CTA is currently reachable to inline guard, correcting historical prose')
control=read(E/'catalog-005b.log');check(not byName['catalog-005b']['observations'] and 'Selected: 1' in control and 'Selected skipped: 1' in control,'catalog005b independent silent earlier skip, not missing case or pass')
check('satisfied="false"' in index['catalog-005b']['CaseXml'] and 'XML_1.1' in index['catalog-005b']['CaseXml'],'negative XML1.1 dependency preserved')
check(len(ledger)==32 and all(x['Original']==index[x['Name']] for x in ledger),'complete per-case original/capability/disposition ledger')
for x in rows:
 root=X.parse(R/x['file']).getroot();ns={'t':root.tag.split('}')[0][1:]};case=next(c for c in root.findall('t:test-case',ns) if c.get('name')==x['Name']);check(X.tostring(case,encoding='unicode')==x['CaseXml'],'unchanged original assertions '+x['Name'])
selected=sum(int(re.search(r'^Selected: (\d+)',read(E/x['log']),re.M)[1]) for x in receipts);check(selected==42,'42 selected XSLT observations account for substring neighbors across 32 requests')
check(all('FAIL ' not in read(E/x['log']) and 'Selected skipped:' in read(E/x['log']) for x in receipts),'all XSLT selected runs inspected; skip outcome is not inferred from process exit')
six=load(E/'qt3-six-originals.json');sixRun=load(E/'qt3-six-receipts.json');old={x['Name']:x['After'] for x in six};check(len(six)==len(sixRun)==6,'six unchanged QT3 originals mapped independently')
for x in sixRun:check(x['result']==old[x['name']] and x['result']['kind']=='HarnessException' and 's:H1 s:H2' in x['result']['message'],'same original multi-affiliation preparation outcome '+x['name'])
processes=receipts+sixRun+load(E/'gate-receipts.json');check(len(processes)==40 and all(x['exitCode']==0 and not x['timeout'] for x in processes),'40 bounded fresh processes complete; all outcomes separately classified')
check(totals(E/'qt3-normal.log')==[31397,0,424] and totals(E/'qt4-normal.log')==[8941,0,1918],'fresh complete exact normal gates')
for n in ['qt3','qt4']:check(skips(E/(n+'-normal-skips.txt'))==skips(P/(n+'-normal-skips.txt')),'identical complete '+n+' skip map')
check(len(re.findall(r'^  Starting:',read(E/'qt4-normal.log'),re.M))==230 and 'Experimental-level (XPath40Experimental) passes: 11' in read(E/'qt4-normal.log'),'230 sets and eleven experimental passes unchanged')
inherit=load(E/'inherited-qualification.json');check(sha(R/inherit['source'])==inherit['sha256'] and load(R/inherit['source'])['passed']==24203,'source-identical C42 full-solution qualification inherited explicitly')
check(len(load(E/'acceptance-matrix.json'))==14 and len({x['id'] for x in load(E/'acceptance-matrix.json')})==14,'14 distinct acceptance groups; no claimed provider acceptance')
proposal=read(D/'COMPATIBILITY-PROPOSAL.md');check(all(x in proposal for x in ['NotApplicable','Supported(value)','Invalid(diagnostics)','Unsupported(coverage)','Cancelled','atomic','lifetime','No public hook']),'contract covers explicit verdicts, provenance and pending owner gate')
for n in ['README.md','docs/ARCHITECTURE.md','docs/FEATURE_REQUESTS.md','docs/INTEGRATION.md','docs/AGENT_HANDOVER.md','docs/DOCUMENTATION_RENDERER_TEST.md','docs/xpath40-catalog/CONFORMANCE-SKIP-TRIAGE.md','docs/conformance/k04-dependency-audit/README.md']:check('K04-C43 schema compatibility proposal' in read(R/n),'canonical sync '+n)
render=load(E/'render-receipt.json');check(len(render['pages'])==12 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'twelve HTML renders and unchanged Fytala CSS')
for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'MANIFEST.json').exists():
 m=load(E/'MANIFEST.json')
 for p,h in m['rawSha256'].items():check(sha(R/p)==h,'raw '+p)
 for p,h in m['gitObjects'].items():check(git('hash-object','--',p)==h,'normalized '+p)
print(json.dumps(dict(status='passed',checks=len(checks),xsltDocumentedCohort=30,additionalCtaWitness=1,independentApplicabilityControl=1,qt3PreparationErrors=6,boundedProcesses=40,publicApiChanged=False,ownerAcceptance=False)))
