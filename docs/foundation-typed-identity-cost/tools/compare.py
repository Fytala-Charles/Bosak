"""Verify allocation attribution, descriptor identity and exact diagnostic baseline images."""
from pathlib import Path
import json,hashlib,zipfile,xml.etree.ElementTree as ET,re,sys
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
def observations(raws):
 pairs={};fingerprints=set()
 for raw in raws:
  for n in ET.fromstring(raw).iter():
   if n.tag.endswith('StdOut') and n.text:
    pairs.update({source:{'mutable':int(a),'isolated':int(b)} for source,a,b in re.findall(r'Identity setup (.*?): mutable=(\d+); isolated=(\d+) bytes/call',n.text)})
    fingerprints.update(re.findall(r'Identity descriptor fingerprint: ([0-9A-F]+)',n.text))
 return pairs,fingerprints
u=json.loads((E/'UNIT-RESULTS.json').read_text());old=json.loads((R/'docs/foundation-typed-identity/evidence/IDENTITY-ALLOCATION.json').read_text())['bytesPerRead']['compiled-vm'];new=json.loads((E/'IDENTITY-ALLOCATION.json').read_text())['bytesPerRead']['compiled-vm']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
 current,fp=observations([z.read(row['trx']) for row in u['projects']]);baseline,bfp=observations([z.read(n) for n in z.namelist() if n.startswith('typed-identity-cost-baseline-loaded-trx/') and n.endswith('.trx')])
assert set(current)==set(baseline)=={'$value','upper-case($value)'} and current==baseline and len(fp)==1 and fp==bfp
assert 0<new<old/2 and new<=8192 and old-new==current['$value']['mutable']-current['$value']['isolated']==current['upper-case($value)']['mutable']-current['upper-case($value)']['isolated']
record={'beforeBytesPerCall':old,'afterBytesPerCall':new,'savedBytesPerCall':old-new,'reductionPercent':round(100*(old-new)/old,1),'setupComparisons':current,'descriptorFingerprint':next(iter(fp)),'descriptorUnchanged':True,'budgetsAccepted':False,'scope':'Same held-operation identity fixture; attribution uses the same compiled VM with mutable/isolated fresh contexts. Before is immutable TA-05 qualification; diagnostic baseline source and rebuilt images are separately pinned.'}
if '--record' in sys.argv:(E/'ALLOCATION-COMPARISON.json').write_bytes((json.dumps(record,indent=2)+'\n').encode())
assert json.loads((E/'ALLOCATION-COMPARISON.json').read_text())==record
pins=json.loads((E/'BASELINE-PINS.json').read_text());qualified=json.loads((R/'docs/foundation-typed-identity/evidence/SOURCE-AND-BINARY-PINS.json').read_text())
for n,h in pins['sources'].items():assert qualified['sources'][n]==h,n
with zipfile.ZipFile(E/'BASELINE-ENGINE-BINARIES.zip') as z:
 for n,row in pins['engines'].items():assert hashlib.sha256(z.read(n+'.dll')).hexdigest()==row['sha256']
seen=set()
for f in (E/'baseline-bindings').glob('*.json'):
 b=json.loads(f.read_text());assert not b['errors']
 for a in b['assemblies']:
  if a['name'] in pins['engines']:assert a['sha256']==pins['engines'][a['name']]['sha256'];seen.add(a['name'])
assert {'Bosak.XPath.Api','Bosak.XPath.Core','Bosak.XPath.Runtime'}<=seen
print('Verified registry-copy attribution, unchanged descriptor and local allocation reduction with separately pinned baseline loads.')
