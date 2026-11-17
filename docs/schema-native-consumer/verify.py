"""Verify the optional native Schema consumer pin, execution receipt and source closure offline."""
from pathlib import Path
import hashlib,json,xml.etree.ElementTree as ET,zipfile
here=Path(__file__).resolve().parent;root=here.parents[1]
def sha(b):return hashlib.sha256(b).hexdigest()
def read(name):return json.loads((here/name).read_text(encoding='utf-8-sig'))
pin=read('SOURCE-PIN.json');props=ET.parse(root/'integrations/Schema/Source/Schema.Source.Files.props')
assert props.findtext('.//SchemaSourceRevision')==pin['revision']
items={r.attrib['Include'].split('$(SchemaSourceRoot)/')[1]:r for r in props.findall('.//ItemGroup/*')}
assert set(items)=={r['path'] for r in pin['files']}
for row in pin['files']:
 item=items[row['path']];assert item.tag==row['kind']
 assert item.attrib['ExpectedSha256'].lower()==row['lfSha256'] and item.attrib['AlternateSha256'].lower()==row['crlfSha256']
assert 'ref: '+pin['revision'] in (root/'.github/workflows/schema-integration.yml').read_text(encoding='utf-8')
r=read('RESULTS.json')
assert r['distinctCurrentCoreSchemaCases']==269 and len(r['coreSolution'])==10
assert sum(int(x['counters']['passed']) for x in r['coreSolution'])==r['corePassed']
for lane,expected in [('implementation',260),('publicConsumer',9)]:
 assert sum(int(v['counters']['total']) for v in r[lane])==expected
 assert all(case['outcome']=='Passed' for suite in r[lane] for case in suite['cases'])
assert all(case['outcome']=='Passed' for suite in r['coreSolution'] for case in suite['cases'])
for lane in ['implementation','public']:
 assert r['forms']['lf-'+lane]['cases']==r['forms']['crlf-'+lane]['cases']
assert all(r['forms']['mutated-'+k]['pinMismatchBeforeCompilation'] for k in ['source','fixture','legal'])
for item in read('HISTORICAL-PRESERVATION.json'):
 data=(root/item['path']).read_bytes()
 if item['mode']=='lf':data=data.replace(b'\r\n',b'\n')
 assert sha(data)==item['sha256'],item['path']
a=read('ARCHIVE.json');assert sha((here/'EXECUTION-RECEIPTS.zip').read_bytes())==a['sha256']
with zipfile.ZipFile(here/'EXECUTION-RECEIPTS.zip') as z:
 assert set(z.namelist())=={e['path'] for e in a['entries']}
 for e in a['entries']:
  data=z.read(e['path']);assert sha(data)==e['sha256']
  assert not e['path'].lower().endswith(('.dll','.exe','.nupkg','.pdb'))
  if e['path'].startswith('schema-inputs/'):
   row=next(r for r in pin['files'] if r['path']==e['path'][14:]);assert sha(data)==row['lfSha256']
  if e['path'].startswith('core-inputs/'):
   assert data.replace(b'\r\n',b'\n')==(root/e['path'][12:]).read_bytes().replace(b'\r\n',b'\n')
for e in read('MANIFEST.json')['files']:
 data=(root/e['path']).read_bytes()
 if e['mode']=='lf':data=data.replace(b'\r\n',b'\n')
 assert sha(data)==e['sha256'],e['path']
print('PASS: exact Schema input/CI pin,269 current-Core cases, complete Core regression receipt, LF/CRLF parity, guard refusals and preserved historical dossiers.')
