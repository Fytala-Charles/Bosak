"""Check authored Core design returns without converting them into runtime/freeze acceptance."""
from pathlib import Path
import hashlib,json,sys,subprocess
P=Path(__file__).resolve().parents[1];R=P.parents[1];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest();B=R/'docs/foundation-r2-owner-review'
sys.path.insert(0,str(B/'tools'))
from returns import validate
i=json.loads((P/'INDEX.json').read_text());assert i['kind']=='ScopedCoreDesignReturns' and i['productionVersion']=='1.42.0'
assert not i['r2Complete'] and not i['runtimeQualified'] and not i['c45Accepted'] and not i['apiFrozen']
assert i['proposalManifestSha256']==sha(R/'docs/foundation-r2-ledger/MANIFEST.json') and i['ownerPacketIndexSha256']==sha(B/'PACKETS.json')
assert {x['returnId'] for x in i['returns']}=={'O2-Core','O4-Core'} and len(i['externalReturnsPending'])==5
records=[]
for row in i['returns']:
 response=json.loads((P/row['file']).read_text());assert validate(response)
 assert response['returnId']==row['returnId'] and response['owner']=='Core' and response['decision']=='AcceptWithScope' and response['signer']=='Codex — Core engineering design review'
 assert not response['runtimeQualification'] and not response['c45Accepted'] and not response['apiFrozen'] and response['unresolvedRequiredSignatureGaps']==[]
 assert subprocess.check_output(['git','cat-file','-t',response['sourceRevision']],cwd=R).decode().strip()=='commit'
 for e in response['evidence']:assert sha(R/e['reference'])==e['sha256'],e['reference']
 records.append({'returnId':response['returnId'],'formatAndDraftBindingValid':True,'scope':'Core design disposition; not runtime/C45/freeze acceptance'})
source=json.loads((R/'docs/foundation-xslt-source-membership/evidence/SOURCE.json').read_text())
for name,digest in source['files'].items():assert sha(R/name)==digest,name
if '--record' in sys.argv:(P/'evidence/VALIDATION.json').write_text(json.dumps({'records':records,'productionChanged':False,'r2Complete':False,'externalRoleReturnsPending':5},indent=2)+'\n',encoding='utf-8')
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified two scoped authored Core design returns, five external returns pending, exact evidence/draft binding and unchanged production; R2 remains open.')
