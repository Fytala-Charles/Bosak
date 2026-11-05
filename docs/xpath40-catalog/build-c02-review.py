"""Rebuild bounded C02 contract reconciliation from preserved inputs and explicit decisions."""
from pathlib import Path
import collections,json,re,xml.etree.ElementTree as ET
C=Path(__file__).resolve().parent;E=C/'evidence/c02-function-review'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
decisions=load(C/'C02-REVIEW-DECISIONS.json');sources=load(E/'entry-contract-sources.json');cases=load(E/'catalog-case-inventory.json')
current=load(C/'CURRENT-FUNCTION-INVENTORY.json');byname={v['name']:v for v in current['functions']}
catalog=ET.parse(C/'evidence/c01-inventory/current-function-catalog.xml').getroot()
ns={'f':'http://www.w3.org/xpath-functions/spec/namespace'}
specfunctions={v.get('prefix','fn')+':'+v.get('name'):v for v in catalog.findall('f:function',ns)}
def declared_options(name):
 node=specfunctions[name]
 return [dict(name=v.get('key'),requiredType=' '.join(''.join(v.find('f:type',ns).itertext()).split()) if v.find('f:type',ns) is not None else None,default=' '.join(''.join(v.find('f:default',ns).itertext()).split()) if v.find('f:default',ns) is not None else None,attributes=dict(v.attrib)) for v in node.findall('.//f:option',ns)]
requests=load(E/'probe-requests.json');req={v['id']:v for v in requests};observations=[json.loads(v) for v in (E/'probe-results.jsonl').read_text().splitlines()]
assert len(decisions['entries'])==30 and len(cases)==3148 and len(observations)==195
controls=[]
for v in observations:
 q=req[v['id']]
 if q['expected'] is not None or q['error'] is not None:
  passed=q['error'] in (v['error'] or '') if q['error'] is not None else v['error'] is None and v['result']==q['expected']
  controls.append(dict(id=v['id'],profile=v['profile'],passed=passed))
unmet=[v for v in controls if not v['passed']]
assert len(controls)==120 and {(v['id'],v['profile']) for v in unmet}=={('atomic-annotation-empty','XPath40'),('atomic-annotation-empty','XPath40Experimental')}
functionrows=[]
for name in sorted({n for entry in decisions['entries'].values() for n in entry['functions']}):
 matches=[v['case'] for v in cases if name in v['referencedFunctions']]
 normal=[v['id'] for v in matches if 'expected' in v and v['mode']!='31']
 empty=[v['id'] for v in matches if any(t in v['id'].lower() for t in ['empty','absent','no-input','zero'])]
 errors=[v['id'] for v in matches if 'error' in v]
 pattern=re.compile(r'(?<![\w:.-])'+(r'(?:fn:)?'+re.escape(name[3:]) if name.startswith('fn:') else re.escape(name))+r'\s*(?:\(|#)')
 extra=[v for v in requests if v['classification']=='control' and pattern.search(v['expression'])]
 normal.extend('probe:'+v['id'] for v in extra if v['expected'] is not None)
 empty.extend('probe:'+v['id'] for v in extra if any(t in v['id'] for t in ['empty','absent']))
 errors.extend('probe:'+v['id'] for v in extra if v['error'] is not None and not any(u['id']==v['id'] for u in unmet))
 parents=[p for p,v in decisions['entries'].items() if name in v['functions']]
 inventory=byname[name]
 functionrows.append(dict(name=name,entries=parents,currentDraftSignatures=inventory['currentDraftSignatures'],currentDeclaredOptions=declared_options(name),explicitSharedOptionConventionReference=any(v.tag.rsplit('}',1)[-1]=='termref' and v.get('def')=='option-parameter-conventions' for v in specfunctions[name].iter()),liveSignatures=inventory['registeredSignatures'],keywordAssessment=inventory['keywordAssessment'],normalEvidence=normal,emptyBoundaryEvidence=empty,errorBoundaryEvidence=errors,evidenceMappingPolicy='Lexical case candidates; declared catalog outcomes were freshly executed. These lists do not prove complete semantic coverage, especially type conversion/error precedence.',unsupportedRemainder='Only the linked tested call forms, types, defaults and options are documented consumer contracts. Unrepresented empty/error/coercion/option combinations are not supported by this reference; no whole-signature acceptance is inferred.',followupGroups=sorted({t for p in parents for t in decisions['entries'][p]['tickets']})))
assert len(functionrows)==74 and all(v['normalEvidence'] for v in functionrows),[v['name'] for v in functionrows if not v['normalEvidence']]
entryrows=[]
for name,decision in sorted(decisions['entries'].items()):
 matched=[v for v in functionrows if name in v['entries']]
 entryrows.append(dict(entry=name,functions=decision['functions'],documentedContracts=decision['contracts'],limitations=decision['limitations'],followupGroups=decision['tickets'],evidence=[dict(function=v['name'],normal=v['normalEvidence'],empty=v['emptyBoundaryEvidence'],error=v['errorBoundaryEvidence'],unsupportedRemainder=v['unsupportedRemainder']) for v in matched],sourceGitBlob=sources[name]['gitBlob'],semanticDisposition='Reconciled bounded consumer contracts and explicit limitations; not AC-04 acceptance. C09-C12 must review each entry against applicable-evidence rules.'))
tickets=[]
for id,ticket in sorted(decisions['tickets'].items()):
 probes=[v for v in requests if v['ticket']==id]
 tickets.append(dict(id=id,**ticket,entries=[p for p,v in decisions['entries'].items() if id in v['tickets']],probeIds=[v['id'] for v in probes],observations=[v for v in observations if v['id'] in {p['id'] for p in probes}],scopePolicy='A bounded follow-up group, not a claim that every current draft feature must be implemented for catalog closure. Split unrelated engine corrections before implementation; accurately documented unsupported contracts remain limitations pending owner acceptance.'))
resolved=[
 dict(finding='map:entries/map:items result shape',status='Delivered in 1.1.2; current catalog cases pass',evidence='CONTAINER-CONTRACT-REVIEW.md'),
 dict(finding='index-of explicit-empty collation',status='Delivered in 1.23.2 for 4.0; fresh control passes',probe='index-of-empty-collation',evidence='INDEX-OF-EMPTY-COLLATION.md'),
 dict(finding='index-of-substring absent-pattern behavior',status='Delivered only in Experimental; frozen refusal retained',evidence='INDEX-OF-SUBSTRING-CONTRACT.md'),
 dict(finding='signed Unix epochs',status='Delivered only in Experimental; frozen negative refusal retained',probes=['unix-negative-frozen','unix-negative-experimental'],evidence='UNIX-DATETIME-EXPERIMENTAL.md'),
 dict(finding='Unix wide valid integer calendar range',status='Delivered FODT0001 classification; fresh wide-range control passes',probe='unix-wide-range',evidence='UNIX-DATETIME-WIDE-RANGE.md'),
 dict(finding='abs/floor/ceiling integer identity',status='Delivered in published 1.26.3; fresh controls pass',probes=['abs-wide-integer-identity','floor-wide-integer-identity','ceiling-wide-integer-identity'],evidence='../RELEASE-1.26.3-PUBLICATION.md'),
 dict(finding='extreme rounding precision',status='Delivered in 1.26.2; fresh control passes',probe='round-extreme-precision',evidence='ROUNDING-PRECISION-BOUNDARIES.md'),
]
out=dict(reviewDate='2026-10-26',workpackage='C02',status='Complete: bounded contract reconciliation and limitations, not semantic acceptance',documentationBase='5bdd5ff7',engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',engineVersion='1.26.3',reviewSpec=current['currentReviewSpec'],policy='Preserve frozen/Experimental distinctions. No automatic current-draft adoption, no reconstructed historical pins, no blanket option/error-eagerness rule. Only functions explicitly adopting shared option conventions use those rules. Unknown JSON options are ignored in 3.1; current 4.0 conventions differ.',summary=dict(entries=len(entryrows),functions=len(functionrows),catalogCases=3148,probeExpressions=len(requests),profileObservations=len(observations),assertedControls=len(controls),assertedControlPasses=len(controls)-len(unmet),assertedControlFailures=len(unmet),unmetContractControls=unmet,diagnosticObservations=len(observations)-len(controls),followupGroups=len(tickets),probeClasses=dict(collections.Counter(req[v['id']]['classification'] for v in observations))),resolvedFindings=resolved,entries=entryrows,functions=functionrows,followups=tickets,adjacentScope=[dict(names=['fn:jkey','fn:jtree','fn:jvalue','array:members','array:of-members'],owner='C03/C05',boundary='JNode/model/collection grammar; no generic model adoption by this function/options review'),dict(names=['fn:transform'],owner='C06',boundary='Host registration and declaration profile'),dict(names=['fn:parse-json','fn:json-doc','fn:json-to-xml','fn:xml-to-json','fn:serialize'],owner='F03/C06/C07 and C09-C12 as applicable',boundary='Supplementary option-convention controls supplied for parse-json. No full resource, JSON/XML conversion or Serialization 4.0 parameter acceptance; named deferred serialization record and host review remain distinct.'),dict(owner='C03',boundary='Complete constructor/type/generic coercion behavior, including fixed numeric arithmetic, is not inferred from these function controls.')])
(C/'FUNCTION-OPTIONS-RECONCILIATION.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
print(json.dumps(out['summary']))
