"""Refresh named review identities using the original policy and content-derived IDs."""
from pathlib import Path
import importlib.util,json,hashlib,shutil,collections
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';sha=lambda b:hashlib.sha256(b).hexdigest()
shutil.copyfile(E/'public-api-0.json',E/'public-api.json')
for n in ['scope-rules.json','compatibility-cases.json']:shutil.copyfile(ROOT/'docs/foundation-api-contract-draft'/n,P/n)
(E/'baseline.json').write_text(json.dumps({'sourceRevision':json.loads((E/'INPUTS.json').read_text())['sourceRevision'],'version':'1.36.0','purpose':'Current observed review baseline, not accepted stable manifest.'},indent=2)+'\n',encoding='utf-8')
path=ROOT/'docs/foundation-api-contract-draft/tools/compile_manifest.py';spec=importlib.util.spec_from_file_location('prior_manifest',path);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
module.BASE=E;module.HERE=P
m,closure=module.generate();m['ownerDispositions']['Braid']='Pending';m['status']='Observed1.36 inventory with carried-forward proposed review classifications; no stable/legacy/experimental acceptance'
(E/'member-manifest.json').write_bytes(module.encoded(m));(E/'signature-closure.json').write_bytes(module.encoded(closure))
oldApi=json.loads((ROOT/'docs/foundation-api-baseline/evidence/public-api.json').read_text());api=json.loads((E/'public-api.json').read_text())
def rowmap(a):return {sha((assembly['assembly']+'\n'+row).encode()):{'assembly':assembly['assembly'],'row':row} for assembly in a['assemblies'] for row in assembly['declarations']}
before=rowmap(oldApi);after=rowmap(api)
added=[{'id':k,**after[k]} for k in after.keys()-before.keys()];removed=[{'id':k,**before[k]} for k in before.keys()-after.keys()]
oldManifest=json.loads((ROOT/'docs/foundation-api-contract-draft/evidence/member-manifest.json').read_text());oldEntries={e['id']:e for e in oldManifest['entries']};newEntries={e['id']:e for e in m['entries']}
assert len(before)==5766 and len(after)==5770 and len(added)==4 and not removed
assert all(oldEntries[k]['reviewGroup']==newEntries[k]['reviewGroup'] and oldEntries[k]['caseIds']==newEntries[k]['caseIds'] and oldEntries[k]['evidenceOnly']==newEntries[k]['evidenceOnly'] for k in before)
owner=json.loads((ROOT/'docs/foundation-c45-handoff136/inputs/xmldb-matrix.json').read_text())
def ids(v):
 if isinstance(v,str):return {v} if v in before else set()
 if isinstance(v,list):return set().union(*(ids(i) for i in v)) if v else set()
 if isinstance(v,dict):return set().union(*(ids(i) for i in v.values())) if v else set()
 return set()
mapped=ids(owner['matrix']);assert len(mapped)==176 and mapped<=newEntries.keys()
crosswalk={'scope':'Legacy exact signature IDs retained; row positions may shift. No owner acceptance.','oldEntries':5766,'currentEntries':5770,'preservedIds':len(before.keys()&after.keys()),'added':sorted(added,key=lambda r:r['row']),'removed':removed,'xmlDbMappedCount':176,'xmlDbMappedEntries':[{'id':k,'oldRow':oldEntries[k]['baselineRow'],'currentRow':newEntries[k]['baselineRow'],'assembly':newEntries[k]['assembly'],'reviewGroup':newEntries[k]['reviewGroup'],'evidenceOnly':newEntries[k]['evidenceOnly']} for k in sorted(mapped)],'policySha256':sha(path.read_bytes()),'classificationAccepted':False}
(E/'LEGACY-CROSSWALK.json').write_text(json.dumps(crosswalk,indent=2)+'\n',encoding='utf-8')
proposed=json.loads((E/'proposed-api.json').read_text());proposal=[]
for a in proposed['assemblies']:
 target='Bosak.XPath.Core' if a['assembly'].startswith('Bosak.XPath.Core.') else 'Bosak.XPath.Api'
 for i,row in enumerate(a['declarations']):proposal.append({'id':sha((target+'\n'+row).encode()),'targetAssembly':target,'proposalAssembly':a['assembly'],'proposalRow':i,'kind':row[0],'declaration':row,'status':'ProposedAdd','evidenceOnly':row[0] in ['A','L'],'accepted':False,'implementation':'NotImplemented'})
assert len({r['id'] for r in proposal})==len(proposal) and not ({r['id'] for r in proposal}&after.keys())
types=[r['declaration'][2:].split(':',1)[0] for r in proposal if r['kind']=='T']
for row in proposal:
 body=row['declaration'][2:];row['type']=next(t for t in sorted(types,key=lambda t:-len(t)) if body.startswith(t+':') or body.startswith(t+'.'))
(E/'proposed-member-delta.json').write_text(json.dumps({'scope':'Exact isolated declaration proposal mapped to intended target assemblies; not production exports or accepted layout.','productionSourceChanged':False,'productionMemberRemovals':[],'existingRequiredInterfaceSlotsAdded':0,'types':40,'targetNamespaces':['Bosak.XPath.Core.Schema','Bosak.XPath.Api.Schema'],'entries':proposal,'accepted':False,'ownerDispositions':{n:'Pending' for n in ['Core','Schema','XmlDb','Braid']}},indent=2)+'\n',encoding='utf-8')
print('Preserved5766 IDs and all176 XmlDb mappings; added two method rows and two attribute-evidence rows; generated40-type proposed delta.')
