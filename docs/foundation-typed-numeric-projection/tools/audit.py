from pathlib import Path
import json,hashlib
R=Path(__file__).resolve().parents[3];E=R/'docs/foundation-typed-numeric-projection/evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
current=json.loads((E/'public-api.json').read_text());previousPath=R/'docs/foundation-public-typed-identity/evidence/public-api.json';previous={a['assembly']:a for a in json.loads(previousPath.read_text())['assemblies']}
for a in current['assemblies']:assert a['declarations']==previous[a['assembly']]['declarations'] and a['friendAssemblies']==previous[a['assembly']]['friendAssemblies'],a['assembly']
assert sum(a['publicTypes'] for a in current['assemblies'])==310 and sum(a['declarationCount'] for a in current['assemblies'])==6369
record={'assemblies':11,'exportedTypes':310,'declarationRows':6369,'declarationsIdentical':True,'friendGrantsIdentical':True,'xdmValueFieldLayoutUnchanged':True,'previousInventorySha256':sha(previousPath),'qualifiedInternalSlice':'TA-08-internal-numeric-projection-v1','scope':'Unchanged exported declarations, XdmValue layout, friends and API source. Internal numeric projection qualified; existing public identity retained. No public numeric/native/freeze acceptance.'}
assert json.loads((E/'API-COMPARISON.json').read_text())==record
print('Verified 310 types / 6369 public rows, fields and friend grants unchanged.')
