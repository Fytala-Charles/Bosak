from pathlib import Path
import json,hashlib
R=Path(__file__).resolve().parents[3];E=R/'docs/foundation-public-ordinary/evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
current=json.loads((E/'public-api.json').read_text());baseline=R/'docs/foundation-xslt-source-membership/evidence/public-api-0.json';old={a['assembly']:a for a in json.loads(baseline.read_text())['assemblies']};reviewPath=R/'docs/foundation-bcap-owner-review/inputs/proposed-api-0.json';review={r for a in json.loads(reviewPath.read_text())['assemblies'] for r in a['declarations']};added=[]
for a in current['assemblies']:
 assert set(old[a['assembly']]['declarations'])<=set(a['declarations']),a['assembly']
 assert a['friendAssemblies']==old[a['assembly']]['friendAssemblies']
 added.extend(sorted(set(a['declarations'])-set(old[a['assembly']]['declarations'])))
types=sorted(r[2:].split(':')[0] for r in added if r.startswith('T '));reviewTypes=sorted(r[2:].split(':')[0] for r in review if r.startswith('T '));assert len(types)==39 and set(types)<=set(reviewTypes)
def belongs(row):
 return any(row[2:].startswith(t+sep) for t in types for sep in [':','.','('])
selected={r for r in review if belongs(r)}
def normalized(r):
 if r.startswith(('A ','L ')):return None
 r=r.split(':attrs=')[0]
 if r.startswith('T Bosak.XPath.Api.Schema.SchemaQueryLimits:'):r=r.replace(':sizeMetadata=1:',':sizeMetadata=0:')
 return r
actual={normalized(r) for r in added}-{None};expected={normalized(r) for r in selected}-{None};assert actual==expected,(sorted(actual-expected),sorted(expected-actual))
changes={'outsideReviewedRows':sorted(set(added)-review),'reviewedRowsReplaced':sorted(selected-set(added))}
record={'assemblies':11,'originalTypesRetained':270,'originalRowsRetained':5770,'originalDeclarationsUnchanged':True,'friendsIdentical':True,'exportedTypes':309,'declarationRows':6335,'addedTypes':types,'addedRows':565,'reviewedDeclarationInventorySha256':sha(reviewPath),'baselineSha256':sha(baseline),'signatureSubsetMatches':True,'excludedReviewedTypes':sorted(set(reviewTypes)-set(types)),'implementationMetadataDifferences':changes,'differenceExplanation':'The real five-int limits struct replaces the proposal empty struct size marker with five private readonly auto-property fields. Seven implemented getters gain CompilerGenerated attributes. Snapshot nullable metadata moves from property annotation to type context. No unreviewed callable signature, factory, setter, public constructor or friend is added. Actual managed size, ABI freeze and native unchanged-binary compatibility remain R4/R5 qualification work.'}
assert json.loads((E/'API-COMPARISON.json').read_text())==record
print('Verified all original API rows, reviewed callable subset and recorded implementation metadata differences.')
