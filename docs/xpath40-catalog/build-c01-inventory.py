"""Build C01 inventory from immutable specification, metadata and lookup captures."""
from pathlib import Path
import collections, hashlib, json, xml.etree.ElementTree as ET

C=Path(__file__).resolve().parent
E=C/'evidence/c01-inventory'
NS={'f':'http://www.w3.org/xpath-functions/spec/namespace'}
PREFIX={'http://www.w3.org/2005/xpath-functions':'fn','http://www.w3.org/2005/xpath-functions/map':'map','http://www.w3.org/2005/xpath-functions/array':'array','http://www.w3.org/2005/xpath-functions/math':'math','http://www.w3.org/2001/XMLSchema':'xs'}
def load(name):return json.loads((E/name).read_text(encoding='utf-8-sig'))
def declarations(name):
 root=ET.parse(E/name).getroot()
 return {v.get('prefix','fn')+':'+v.get('name'):v for v in root.findall('f:function',NS)}
def describe(node):
 if node is None:return None
 return {'tag':node.tag.rsplit('}',1)[-1],'attributes':dict(node.attrib),'text':' '.join((node.text or '').split()),'children':[describe(c) for c in node]}
def signatures(node):
 result=[]
 if node is None:return result
 for p in node.findall('f:signatures/f:proto',NS):
  args=[dict(v.attrib) for v in p.findall('f:arg',NS)]
  variadic=bool(p.get('is-variadic') or p.get('variadic') or any(v.get('name')=='...' for v in args) or any((v.text or '').strip()=='variadic' for v in node.findall('f:properties/f:property',NS)))
  result.append(dict(attributes=dict(p.attrib),parameters=args,returnType=p.get('return-type'),minArity=sum(v.get('default') is None and v.get('name')!='...' for v in args),maxArity=None if variadic else len(args),variadic=variadic))
 return result
def fingerprint(value):return hashlib.sha256(json.dumps(value,sort_keys=True,ensure_ascii=False).encode()).hexdigest()

live=load('live-signatures.json'); old=load('retained-audit-function-inventory.json')
existing=load('retained-audit-changed31-contracts.json'); previous=load('retained-audit-signature-contracts.json')
draft=declarations('current-function-catalog.xml'); retained=declarations('retained-function-catalog.xml'); baseline=declarations('baseline-function-catalog.xml')
byname=collections.defaultdict(list)
for v in live:byname[PREFIX.get(v['ns'],v['ns'])+':'+v['name']].append(v)
newnames={n for n,rs in byname.items() if any(v['frozen'] or v['experimental'] for v in rs)}
names=newnames|{v['name'] for v in existing['functions']}
oldrows={v['name']:v for v in previous['functions']+existing['functions']}
lookups=[json.loads(v) for v in (E/'lookup-results.jsonl').read_text().splitlines()]
assert len(names)==316 and len(newnames)==119 and len(lookups)==3*len(load('lookup-requests.json'))
assert {v['name'] for v in lookups}==names
assert all(v['error'] is None and v['result'] in ('0','1') for v in lookups)
rows=[]
for name in sorted(names):
 regs=sorted(byname[name],key=lambda v:v['arity']); node=draft.get(name); ds=signatures(node); prior=signatures(retained.get(name)); oldrow=oldrows.get(name,{})
 kw=regs[0]['keywordNames'] if regs else None
 owner='C03' if name.startswith('xs:') else 'C02'
 status='Constructor declaration is outside the F&O function catalog; contextual/coercion review belongs to C03.' if name.startswith('xs:') else 'Absent current F&O function declaration; lifecycle/profile disposition belongs to C07.' if node is None else 'Current raw declaration retained; semantic equivalence requires C02.'
 gaps=[]
 if not regs:gaps.append(dict(kind='No StandardFunctions registration; inspect host registration separately',owner='C06'))
 if name.startswith('xs:'):gaps.append(dict(kind='Constructor lexical/value/context/error boundary',owner='C03'))
 elif node is None:gaps.append(dict(kind='No current function-catalog declaration',owner='C07'))
 else:
  if kw is None:gaps.append(dict(kind='Keyword metadata absent; parser positional fallback and semantics require review',owner='C02'))
  elif not any(kw==[v.get('name') for v in d['parameters']] for d in ds):gaps.append(dict(kind='Keyword labels differ from current declared parameter lists',owner='C02'))
  if any(not any(d['minArity']<=v['arity'] and (d['maxArity'] is None or v['arity']<=d['maxArity']) for d in ds) for v in regs):gaps.append(dict(kind='Registered arity outside current declared ranges',owner='C02'))
  missing=sorted({a for d in ds if d['maxArity'] is not None for a in range(d['minArity'],d['maxArity']+1)}-{v['arity'] for v in regs})
  if missing:gaps.append(dict(kind='Current declared arity lacks exact registration; defaults/variadic dispatch can still supply calls',arities=missing,owner='C02'))
 if any(v['parameterTypeNames'] is None or v['returnTypeName'] is None for v in regs):gaps.append(dict(kind='Precise dispatch sequence-type metadata incomplete; coarse kinds are not full contracts',owner=owner))
 gaps.append(dict(kind='Contract-by-contract omitted/empty/error behavior is not certified by availability or metadata',owner=owner))
 retained_evidence={k:v for k,v in oldrow.items() if any(t in k.lower() for t in ['case','check','behavior','assessment']) and k!='registeredSignatures'}
 rows.append(dict(name=name,scope='4.0-only focus constructor' if name.startswith('xs:') else '4.0-only non-constructor' if name in newnames else 'existing 3.1 name',registeredSignatures=regs,currentDraftSignatures=ds,retainedDraftSignatures=prior,baselineSignatures=signatures(baseline.get(name)),declarationTextChangedSinceRetained=ds!=prior,contractStructureChangedSinceRetained=fingerprint(describe(node))!=fingerprint(describe(retained.get(name))),semanticChangeEstablished=False,keywordAssessment='Absent metadata' if kw is None else 'Names match a current declaration' if any(kw==[p.get('name') for p in d['parameters']] for d in ds) else 'Names differ from current declarations',defaultComparisonPolicy='Raw keyword expressions and draft attributes retained; no textual equality implies semantic acceptance.',metadataChangedSinceRetained=oldrow.get('registeredSignatures')!=regs if oldrow.get('registeredSignatures') is not None else None,currentContractSource=dict(file="evidence/c01-inventory/current-function-catalog.xml",function=name,structureSha256=fingerprint(describe(node))),lookupObservations=[v for v in lookups if v['name']==name],omittedEmptyErrorMapping=dict(owner=owner,status='Review required; retained mappings are leads, not newly accepted semantic evidence',retainedEvidence=retained_evidence,currentOmissionDeclarations=[p for d in ds for p in d['parameters'] if p.get('default') is not None],currentErrorSections=[describe(v) for v in node.findall('f:errors',NS)] if node is not None else [],currentRuleSections=[dict(source='evidence/c01-inventory/current-function-catalog.xml',function=name,section='rules',index=i,structureSha256=fingerprint(describe(v))) for i,v in enumerate(node.findall('f:rules',NS))] if node is not None else []),assessment=status,gaps=gaps))
summary=dict(names=len(names),registered40OnlyNames=len(newnames),nonConstructorNames=sum(not n.startswith('xs:') for n in newnames),focusConstructorNames=sum(n.startswith('xs:') for n in newnames),existingNames=len(existing['functions']),lookupObservations=len(lookups),lookupErrors=sum(v['error'] is not None for v in lookups),added40OnlyNames=sorted(newnames-{v['name'] for v in old['functionNames']}),removed40OnlyNames=sorted({v['name'] for v in old['functionNames']}-newnames),currentDeclarationTextChanges=sum(v['declarationTextChangedSinceRetained'] for v in rows),currentContractStructureChanges=sum(v['contractStructureChangedSinceRetained'] for v in rows),keywordAssessments=dict(collections.Counter(v['keywordAssessment'] for v in rows)),gapKinds=dict(collections.Counter(g['kind'] for v in rows for g in v['gaps'])))
out=dict(reviewDate='2026-10-26',engineVersion='1.26.3',engineSource='08fbdc31ac79baac04cf00f4773ec6d0b2dd247f',documentationBase='7721f5a20641d874c0bdbedf4be1b17978a27c05',currentReviewSpec='b4723ff52fd9d4fabf2b4cd33bd44f21939c512c',retainedReviewSpec='e7ee4ab8cb6c6c0f6f60014e2367f1a90909e69e',historicalAdoptionPolicy='Neither review pin supplies the 28 missing historical adoption records; C08 is unchanged.',scopePolicy='Inventory and owner mapping only; no semantic acceptance, language promotion, gate change or requirement closure.',summary=summary,functions=rows)
(C/'CURRENT-FUNCTION-INVENTORY.json').write_text(json.dumps(out,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
print(json.dumps(summary,indent=2))
