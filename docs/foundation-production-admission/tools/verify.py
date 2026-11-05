from pathlib import Path
import json,hashlib,re,zipfile,sys
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1]
sha=lambda b:hashlib.sha256(b).hexdigest()
for n,h in json.loads((P/'MANIFEST.json').read_text(encoding='utf-8')).items():assert sha((P/n).read_bytes())==h,n
source=json.loads((P/'SOURCE.json').read_text(encoding='utf-8'));assert sha((P/source['archive']).read_bytes())==source['archiveSha256']
with zipfile.ZipFile(P/source['archive']) as z:
 for n,h in source['files'].items():
  assert sha(z.read(n).replace(b'\r\n',b'\n'))==h,n
  if '--check-current' in sys.argv:assert sha((ROOT/n).read_bytes().replace(b'\r\n',b'\n'))==h,n
r=json.loads((P/'RESULTS.json').read_text(encoding='utf-8'));assert r['productVersion']=='1.35.0' and r['unitTests']['passed']==24245
text=(P/'evidence/unit-tests.txt').read_text(encoding='utf-8');rows=re.findall(r'(Passed!|Failed!)\s+- Failed:\s*(\d+), Passed:\s*(\d+), Skipped:\s*(\d+), Total:\s*(\d+).*? - ([^\s]+\.dll)',text)
assert len(rows)==10 and all(v[3]=='0' for v in rows)
assert 'conflicts with an existing registration' in text and 'used by another process' in text
assert [(v[5],int(v[1])) for v in rows if v[0]=='Failed!']==[('Bosak.XPath.Providers.Database.Tests.dll',2)]
serial=(P/'evidence/database-serial.txt').read_text(encoding='utf-8');assert re.search(r'Passed!\s+- Failed:\s*0, Passed:\s*47, Skipped:\s*0',serial)
projects={v[5]:int(v[2]) for v in rows};projects['Bosak.XPath.Providers.Database.Tests.dll']=47
assert projects==r['unitTests']['projects'] and sum(projects.values())==24245
for name,counts in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
 text=(P/f'evidence/{name}.txt').read_text(encoding='utf-8')
 for label,value in zip(['Passed','Failed','Skipped'],counts):assert re.search(rf'{label}:\s+{value}\b',text)
 assert (P/f'evidence/{name}-skips.txt').read_bytes().replace(b'\r\n',b'\n')==(ROOT/f'docs/foundation-package-gates/evidence/{name}-skips.txt').read_bytes().replace(b'\r\n',b'\n')
a=json.loads((P/'evidence/baseline-public-surface.json').read_text(encoding='utf-8'));b=json.loads((P/'evidence/candidate-public-surface.json').read_text(encoding='utf-8'));assert a['surface']==b['surface'] and a['xdmValueSize']==b['xdmValueSize']==48
assert len(b['surface'])==78 and sum(len(t['members']) for t in b['surface'])==1199
baseline=json.loads((ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary.json').read_text(encoding='utf-8'));caller=json.loads((P/'evidence/legacy-caller.json').read_text(encoding='utf-8'))
assert caller['callerSha256']==baseline['callerSha256'] and caller['cases']==baseline['cases'] and caller['passed']==20 and caller['failed']==0
assert {v['name']:v['sha256'] for v in caller['loadedAssemblies']}=={v['name']:v['sha256'] for v in b['assemblies']}
assert all(v['version']=='1.35.0.0' for v in b['assemblies'])
assert not r['atomicVmTransportImplemented'] and not r['publicV5Promoted'] and not r['apiFreezeAccepted']
print('PASS: source hashes, 24,245 unit passes, exact QT3/qt4 gates/skip maps, unchanged 20-case binary caller, unchanged exported metadata and 48-byte XdmValue; no freeze acceptance inferred.')
