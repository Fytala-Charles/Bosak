"""Verify fixture receipts against the unchanged sealed production cohort."""
from pathlib import Path
import hashlib,json
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';B=R/'docs/foundation-xslt-source-membership'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'INPUTS.json').read_text());a=json.loads((E/'probe-0.json').read_text())
assert a==json.loads((E/'probe-1.json').read_text()) and a['checks']==i['checks']==len(a['rows'])==57
assert len({r['id'] for r in a['rows']})==57 and all(r['passed'] for r in a['rows'])
ids={'success','second-acquire-failure','forged-assessment','duplicate-identity','capacity','revoked-before-second-admission','pre-cancel','cancel-after-first-admission','cancel-after-second-acquire','cancel-after-second-admission','work-error','work-and-two-cleanup-errors','admission-and-two-cleanup-errors','successful-work-single-cleanup-error','successful-work-two-cleanup-errors','acquirer-internal-error-and-cleanup','canceled-wait-retains-raw','close-during-raw','cancellation-and-two-cleanup-errors'}
for host in ['XPath','XQuery','XSLT']:assert {r['id'].split(':',1)[1] for r in a['rows'] if r['id'].startswith(host+':')}==ids
assert i['productionVersion']=='1.42.0' and i['productionChanges'] is False and i['setupReflectionOnly'] and a['setupReflectionOnly']
assert sha(P/'tools/Probe.cs.txt')==i['sourceSha256']
assert sha(B/'evidence/SOURCE.json')==i['baselineSourceReceiptSha256'] and sha(B/'evidence/RESULTS.json')==i['baselineResultsSha256'] and sha(B/'evidence/public-api-0.json')==i['baselineApiReceiptSha256']
source=json.loads((B/'evidence/SOURCE.json').read_text())
for name,digest in source['files'].items():assert sha(R/name)==digest,name
for name,v in i['engines'].items():assert sha(Path(v['path']))==v['sha256'],name
for b in a['bindings']:assert b['sha256']==(i['probeAssemblySha256'] if b['name']=='Bosak.XPath.Runtime.Tests' else i['engines'][b['name']+'.dll']['sha256'])
assert '0 Error(s)' in (E/'build.txt').read_text() and '0 Warning(s)' in (E/'build.txt').read_text()
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified 57 repeated all-host acquisition/rollback controls, unchanged production source and exact loaded engines.')
