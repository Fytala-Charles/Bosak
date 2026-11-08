from pathlib import Path
import json,hashlib,re
R=Path(__file__).resolve().parents[3];E=R/'docs/foundation-exact-value-transport/evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
current=json.loads((E/'public-api.json').read_text());previousPath=R/'docs/foundation-exact-decimal/evidence/public-api.json';previous={a['assembly']:a for a in json.loads(previousPath.read_text())['assemblies']};increment=[]
for a in current['assemblies']:
 assert set(previous[a['assembly']]['declarations'])<=set(a['declarations']),a['assembly']
 assert a['friendAssemblies']==previous[a['assembly']]['friendAssemblies'],a['assembly']
 increment.extend(sorted(set(a['declarations'])-set(previous[a['assembly']]['declarations'])))
assert len(increment)==14 and all(r[2:].startswith('Bosak.XPath.Core.Xdm.XdmValue.') for r in increment)
witnessPath=R/'docs/foundation-bcap-owner-review/inputs/proposal/ExactNumeric.cs.txt';source=witnessPath.read_text(encoding='utf-8-sig').split('internal interface IXdmValueNumericMemberWitness')[1]
owner='Bosak.XPath.Core.Xdm.XdmValue';types={'XdmValue':owner,'BigInteger':'System.Numerics.BigInteger','XdmExactDecimal':'Bosak.XPath.Core.Xdm.XdmExactDecimal','string':'System.String'}
expected=[];methods=re.findall(r'static abstract (\w+) (\w+)\(([^)]*)\);',source);props=re.findall(r'(\w+) (\w+)\s*\{\s*get;\s*\}',source);assert len(methods)==4 and len(props)==2
for result,name,args in methods:
 params=[]
 for argument in args.split(','):
  m=re.fullmatch(r'\s*(\w+)(\?)? (\w+)( = null)?\s*',argument);assert m,argument
  kind,nullable,param,optional=m.groups();attrs=''
  if nullable:attrs='[System.Runtime.CompilerServices.NullableAttribute((Byte)2)]'
  if optional:attrs+=(';' if attrs else '')+'[System.Runtime.InteropServices.OptionalAttribute()]'
  params.append(types[kind]+' '+param+(':'+'Optional, HasDefault:null' if optional else ':None:none')+':required=:optional=:attrs='+attrs)
 args=','.join(params);expected.extend(['A '+owner+'.'+name+'('+args+'):', 'M '+owner+'.'+name+':Public, Static, HideBySig:Standard:'+types[result]+' :None:none:required=:optional=:attrs=('+args+'):'])
for result,name in props:
 expected.extend(['A '+owner+'.get_'+name+'():','M '+owner+'.get_'+name+':Public, HideBySig, SpecialName:Standard, HasThis:'+types[result]+' :None:none:required=:optional=:attrs=():','P '+owner+'.'+name+':'+types[result]+'():attrs='])
assert set(expected)==set(increment),(sorted(set(increment)-set(expected)),sorted(set(expected)-set(increment)))
oldFields=sorted(r for r in previous['Bosak.XPath.Core']['declarations'] if r.startswith('L '+owner+'.'));newCore=next(a for a in current['assemblies'] if a['assembly']=='Bosak.XPath.Core');assert oldFields==sorted(r for r in newCore['declarations'] if r.startswith('L '+owner+'.'))
assert sum(a['publicTypes'] for a in current['assemblies'])==310 and sum(a['declarationCount'] for a in current['assemblies'])==6369
record={'assemblies':11,'previousTypesRetained':310,'previousRowsRetained':6355,'previousDeclarationsUnchanged':True,'friendsIdentical':True,'exportedTypes':310,'declarationRows':6369,'newTypes':0,'addedRows':14,'reviewedFactorySignatures':4,'reviewedExactAccessors':2,'witnessSourceSha256':sha(witnessPath),'previousInventorySha256':sha(previousPath),'witnessSignaturesMatch':True,'xdmValueInstanceFieldRowsIdentical':True,'xdmValueInstanceFields':oldFields,'addedRowsDetail':sorted(increment),'qualificationScope':'Exact witness comparison and unchanged previous metadata. No claim of complete host numeric routing, source compatibility of untyped defaults, native ABI/owner acceptance or freeze.'}
assert json.loads((E/'API-COMPARISON.json').read_text())==record
print('Verified previous public rows/fields/friends and all six reviewed exact numeric witness signatures.')
