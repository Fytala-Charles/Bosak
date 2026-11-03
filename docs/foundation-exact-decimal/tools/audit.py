from pathlib import Path
import json,hashlib
R=Path(__file__).resolve().parents[3];E=R/'docs/foundation-exact-decimal/evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
current=json.loads((E/'public-api.json').read_text());baseline=R/'docs/foundation-xslt-source-membership/evidence/public-api-0.json';old={a['assembly']:a for a in json.loads(baseline.read_text())['assemblies']};previousPath=R/'docs/foundation-public-ordinary/evidence/public-api.json';previous={a['assembly']:a for a in json.loads(previousPath.read_text())['assemblies']};reviewPath=R/'docs/foundation-bcap-owner-review/inputs/proposed-api-0.json';review={r for a in json.loads(reviewPath.read_text())['assemblies'] for r in a['declarations']};added=[];increment=[]
for a in current['assemblies']:
 assert set(previous[a['assembly']]['declarations'])<=set(a['declarations']),a['assembly']
 assert a['friendAssemblies']==old[a['assembly']]['friendAssemblies']==previous[a['assembly']]['friendAssemblies']
 added.extend(sorted(set(a['declarations'])-set(old[a['assembly']]['declarations'])))
 increment.extend(sorted(set(a['declarations'])-set(previous[a['assembly']]['declarations'])))
assert len(increment)==20 and all(r[2:].startswith('Bosak.XPath.Core.Xdm.XdmExactDecimal') for r in increment)
types=sorted(r[2:].split(':')[0] for r in added if r.startswith('T '));reviewTypes=sorted(r[2:].split(':')[0] for r in review if r.startswith('T '));assert len(types)==40 and set(types)<=set(reviewTypes)
def belongs(row):return any(row[2:].startswith(t+sep) for t in types for sep in [':','.','('])
selected={r for r in review if belongs(r)}
def normalized(r):
 if r.startswith(('A ','L ')):return None
 r=r.split(':attrs=')[0]
 if r.startswith(('T Bosak.XPath.Api.Schema.SchemaQueryLimits:','T Bosak.XPath.Core.Xdm.XdmExactDecimal:')):r=r.replace(':sizeMetadata=1:',':sizeMetadata=0:')
 return r
actual={normalized(r) for r in added}-{None};expected={normalized(r) for r in selected}-{None};assert actual==expected,(sorted(actual-expected),sorted(expected-actual))
changes={'outsideReviewedRows':sorted(set(added)-review),'reviewedRowsReplaced':sorted(selected-set(added))}
record={'assemblies':11,'previousTypesRetained':309,'previousRowsRetained':6335,'previousDeclarationsUnchanged':True,'legacyTypesRetained':270,'legacyRowsRetained':5770,'friendsIdentical':True,'exportedTypes':310,'declarationRows':6355,'addedSincePreviousTypes':1,'addedSincePreviousRows':20,'cumulativePromotedTypes':types,'cumulativeAddedRows':585,'newRows':increment,'reviewedDeclarationInventorySha256':sha(reviewPath),'previousInventorySha256':sha(previousPath),'baselineSha256':sha(baseline),'signatureSubsetMatches':True,'excludedReviewedTypes':sorted(set(reviewTypes)-set(types)),'implementationMetadataDifferences':changes,'differenceExplanation':'Retain all 1.45.0 public metadata. The real exact decimal adds private readonly BigInteger coefficient/int scale storage and generated getter annotations, replacing the proposal empty-struct size marker. Earlier limits storage/snapshot nullability differences remain recorded. No unreviewed callable signature or friend is added. Managed size, native ABI/freeze and exact XdmValue transport are not qualified by this metadata audit.'}
assert sum(a['publicTypes'] for a in current['assemblies'])==310 and sum(a['declarationCount'] for a in current['assemblies'])==6355
assert json.loads((E/'API-COMPARISON.json').read_text())==record
print('Verified all 1.45.0 public rows retained and the fortieth reviewed public type promoted; actual storage differences recorded.')
