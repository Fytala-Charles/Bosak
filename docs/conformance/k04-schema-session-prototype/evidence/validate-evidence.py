"""Validate C44's isolated prototype, ABI experiment, proposed surface and unchanged product baseline."""
from pathlib import Path
import json,hashlib,subprocess,re
E=Path(__file__).resolve().parent;D=E.parent;R=E.parents[3];checks=[]
def read(p):
 b=p.read_bytes();return b.decode('utf-16' if b.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')
def load(p):return json.loads(read(p))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def check(ok,label):
 assert ok,label
 checks.append(label)
def git(*args):return subprocess.check_output(['git',*args],cwd=R,text=True).strip()
pin=load(E/'PROTOTYPE-ADOPTION.json');check(pin['baseCommit'].startswith('029be319') and len(pin['sources'])==2 and 'before source generation' in pin['kind'],'prototype-only immutable semantic adoption before implementation')
check(not git('diff',pin['baseCommit'],'--name-only','--','src','tests','AGENTS.md') and not git('ls-files','--others','--exclude-standard','--','src','tests'),'no product, API, test, fixture, baseline or version source changes')
for name in ['previous-identities.json','baseline-binary-identities.json','prototype-binary-identities.json','abi-binary-identities.json']:
 for path,h in load(E/name).items():check(sha(R/path)==h,'preserved '+path)
for path,item in load(E/'core-source-identities.json').items():check(sha(R/path)==item['sha256'] and git('hash-object','--',path)==item['gitObject'],'same baseline core source '+path)
for path,item in load(E/'value-flow-audit.json').items():check(sha(R/path)==item['sha256'] and git('hash-object','--',path)==item['gitObject'] and item['observations'],'current audited payload/host path '+path)
for name,item in load(E/'executed-source-identities.json').items():check(sha(E/name)==sha(R/item['path'])==item['sha256'],'exact executed isolated source '+name)
receipts=load(E/'prototype-receipts.json');check(len(receipts)==5 and {x['mode'] for x in receipts}=={'none','fallback','qname','partial','unwrap'},'five final control/mutant runs')
normal=load(E/'prototype-none-results.json');check(normal['passed']==110 and normal['failed']==0 and len(normal['tests'])==110 and all(x['passed'] for x in normal['tests']),'110 final contract controls pass')
names={x['name'] for x in normal['tests']};check(len(names)==110,'110 distinct named controls')
for term in ['owned NotApplicable never downgrades','supported false is data','provider diagnostic details retained','mid-assessment cancellation does not downgrade','scoped local versus global declaration','import bytes affect closure','static context fingerprint affects closure','owned assessment cannot mix annotation universes','complete P members include extension witness','head affiliation does not prove subset','nilled still checks candidate type','typed atomization keeps authority','foreign binding rejects equal QName','annotated atom cannot silently enter legacy route','strip copies remove descendants and attributes','escaped atom keeps independent session alive','parallel immutable session and annotation reads','actual engine returns legacy typed data only']:check(term in names,'explicit contract witness '+term)
expected={'fallback':5,'qname':3,'partial':3,'unwrap':6};ledger=load(E/'mutation-ledger.json')
for x in receipts:
 result=load(E/('prototype-'+x['mode']+'-results.json'));check(not x['timeout'] and x['watchdogSeconds']==45,'bounded prototype '+x['mode'])
 if x['mode']=='none':check(x['exitCode']==0,'successful positive process exit')
 else:
  failed=[t['name'] for t in result['tests'] if not t['passed']];check(x['exitCode']==2 and result['failed']==expected[x['mode']] and result['passed']+result['failed']==110 and failed==ledger[x['mode']] and {t['name'] for t in result['tests']}==names,'same 110 controls reject mutant '+x['mode'])
layout=normal['layout'];check([layout[k] for k in ['actualXdmValueBytes','payloadEnvelopeCarrierBytes','extraFieldCarrierBytes']]==[48,48,56],'measured 48-byte current/envelope versus 56-byte extra-reference carriers')
check([x['Name'] for x in layout['actualPrivateFields']]==['_kind','_integer','_double','_reference','_schemaTypeName','_userSchemaTypeName'],'actual current six-field categories pinned')
perf={x['label']:x for x in normal['performance']['samples']};check(len(perf)==3 and all(x['iterations']==200000 for x in perf.values()) and normal['performance']['checksum']==7440000,'bounded prototype-only performance loops consume results')
extra=perf['annotated envelope create']['allocatedBytes']-perf['plain carrier read']['allocatedBytes'];check(extra//200000==32,'observed 32 bytes per annotated integer envelope, not a product threshold')
check(normal['actualEngineBoundary']=={'value':'12','UserSchemaTypeName':'Q{urn:types}Restricted','sessionAvailableThroughExistingValueApi':False},'real public XPath data boundary records lexical type without invented session API')
abi=load(E/'abi-receipts.json');check(len(abi)==4 and all(x['exitCode']==0 and not x['timeout'] for x in abi),'four bounded synthetic ABI build/run processes')
before=load(E/'abi-v1-results.json');after=load(E/'abi-v2-results.json');check(before['passed']==25 and after['passed']==55 and before['failed']==after['failed']==0 and before['size']==after['size']==48 and not before['hasAddition'] and after['hasAddition'],'25 legacy /55 replacement-library consumer controls, same synthetic layout')
old=R/'.guard-tmp/k04-schema-session-abi/consumer/bin/Release/net10.0';new=R/'.guard-tmp/k04-schema-session-abi/upgraded';check(sha(old/'OldConsumer.dll')==sha(new/'OldConsumer.dll') and sha(old/'CarrierAbi.dll')!=sha(new/'CarrierAbi.dll'),'identical old consumer bytes run against distinct same-identity synthetic libraries')
api=load(E/'api-surface.json');check(len(api['exportedTypes'])==24 and set(api['references'])=={'Bosak.XPath.Core','System.Runtime','System.Collections','System.Collections.Immutable'},'24 compiled proposed types; only Core and BCL dependencies')
check(all(x['type'].startswith('C44.Proposed.') for x in api['exportedTypes']) and not any('SchemaAnnotation' in x for x in api['actualXdmValuePublicMembers']),'proposed surface remains isolated; current XdmValue public API unchanged')
inspector=load(E/'api-inspector-receipt.json');check(inspector['exitCode']==0 and not inspector['timeout'] and len(receipts+abi+[inspector])==10,'ten final bounded qualification processes')
for name in ['prototype-build.log','proposed-contracts-build.log','inspector-build.log','abi-consumer-build.log','abi-v2-build.log']:check('Build succeeded.' in read(E/name) and '0 Warning(s)' in read(E/name) and '0 Error(s)' in read(E/name),'successful final isolated build '+name)
initial=load(E/'initial-prototype-none-results.json');check(initial['failed']==1 and [x['name'] for x in initial['tests'] if not x['passed']]==['actual engine returns legacy typed data only'],'initial missing-node-annotation setup error is preserved separately')
history=load(E/'execution-history.json');check(history['final']['controlCount']==110 and [x['controls'] for x in history['intermediate']]==[91,97,103,107],'calibration/intermediate executions separated from final qualification')
inherit=load(E/'inherited-qualification.json');check(sha(R/inherit['solutionReceipt'])==inherit['solutionReceiptSha256'] and load(R/inherit['solutionReceipt'])['passed']==24203 and inherit['regressionPasses']==39,'source-identical 24,203 solution /39 regression passes inherited explicitly')
P=R/inherit['normalGatesSource'];check(inherit['qt3']==[31397,0,424] and inherit['qt4']==[8941,0,1918] and all(x['exitCode']==0 and not x['timeout'] for x in load(P/'gate-receipts.json')),'C43 complete exact normal gates/maps inherited unchanged')
snapshot=read(R/'docs/conformance/k04-schema-profile-review/evidence/core-req139-snapshot.txt');check(snapshot in read(R/'docs/FEATURE_REQUESTS.md') and '- [ ] ADR-006 accepted' in snapshot,'REQ139 original pending acceptance criteria/history preserved')
check('**Pending.**' in read(D/'OWNER-DECISION.md') and 'before the S4 design freeze' in read(D/'OWNER-DECISION.md'),'owner acceptance requested, not assumed')
for path in ['README.md','docs/ARCHITECTURE.md','docs/FEATURE_REQUESTS.md','docs/INTEGRATION.md','docs/AGENT_HANDOVER.md','docs/DOCUMENTATION_RENDERER_TEST.md','docs/xpath40-catalog/CONFORMANCE-SKIP-TRIAGE.md','docs/conformance/k04-dependency-audit/README.md']:check('K04-C44 isolated schema prototype' in read(R/path),'canonical sync '+path)
render=load(E/'render-receipt.json');check(len(render['pages'])==12 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256'],'twelve HTML renders with unchanged CSS')
for x in render['pages']:check(sha(R/x['html'])==x['sha256'],'render '+x['source'])
if (E/'MANIFEST.json').exists():
 m=load(E/'MANIFEST.json')
 for path,h in m['rawSha256'].items():check(sha(R/path)==h,'raw '+path)
 for path,h in m['gitObjects'].items():check(git('hash-object','--',path)==h,'normalized '+path)
print(json.dumps(dict(status='passed',checks=len(checks),contractControls=110,mutantsRejected=4,syntheticConsumerBefore=25,syntheticConsumerAfter=55,proposedTypes=24,productSourceChanged=False,ownerAcceptance=False)))
