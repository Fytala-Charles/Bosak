"""Read-only verification of the applied 1.36.0 source and exact tested cohort."""
from pathlib import Path
import json,hashlib,zipfile,xml.etree.ElementTree as ET,re,sys,statistics
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];E=P/'evidence';sha=lambda b:hashlib.sha256(b).hexdigest()
for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha((P/n).read_bytes())==h,n
s=json.loads((E/'SOURCE.json').read_text());r=json.loads((P/'RESULTS.json').read_text());layout=json.loads((E/'LAYOUT.json').read_text())
proposal=json.loads((ROOT/'docs/foundation-transport-promotion/PATCH.json').read_text())
assert s['productionModified'] and s['version']=='1.36.0' and r['productionApplied'] and r['version']=='1.36.0'
assert s['promotedPatchSha256']==proposal['patchSha256'] and len(s['patchTargets'])==6
assert sha((E/'source.zip').read_bytes())==s['archiveSha256']
with zipfile.ZipFile(E/'source.zip') as z:
 assert {n for n in z.namelist() if not n.endswith('/')}==set(s['files'])
 for n,h in s['files'].items():
  raw=z.read(n);assert sha(raw)==h,n
  if n in proposal['files']:assert sha(raw.replace(b'\r\n',b'\n'))==proposal['files'][n]['afterSha256']
  if '--check-current' in sys.argv and n!='README.md':assert sha((ROOT/n).read_bytes())==h,n
 for n,h in s['patchTargets'].items():assert sha(z.read(n))==h
 assert '<Version>1.36.0</Version>' in z.read('src/Directory.Build.props').decode('utf-8-sig')
 with zipfile.ZipFile(ROOT/'docs/foundation-transport-full-gates/evidence/source-baseline.zip') as baseline:
  for n in baseline.namelist():
   if not n.endswith('/') and n.startswith(('src/','tests/')) and n not in proposal['files']:
    assert z.read(n).replace(b'\r\n',b'\n')==baseline.read(n).replace(b'\r\n',b'\n'),n
assert len(layout['engines'])==10 and len(layout['seven'])==7
if '--check-current' in sys.argv:
 for n,v in layout['engines'].items():assert sha(Path(v['path']).read_bytes())==v['sha256']
after=json.loads((E/'CORPUS-AFTER.json').read_text())
for n,c in s['corpora'].items():assert after[n]=={'revision':c['revision'],'status':c['statusBefore']} and c['statusBefore']==''
for label,counts in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
 text=(E/(label+'.txt')).read_text()
 for name,expected in zip(['Passed','Failed','Skipped'],counts):assert re.search(rf'^{name}:\s+{expected}\s*$',text,re.M)
 assert (E/(label+'-skips.txt')).read_bytes().replace(b'\r\n',b'\n')==(ROOT/'docs/foundation-production-admission/evidence'/(label+'-skips.txt')).read_bytes().replace(b'\r\n',b'\n')
 assert any((E/(label+'-bindings')).glob('*.json'))
assert json.loads((E/'GATE-RUNS.json').read_text())=={'qt3':0,'qt4':0}
assert 'Experimental-level (XPath40Experimental) passes: 11' in (E/'qt4.txt').read_text()
units=json.loads((E/'UNIT-FINAL.json').read_text());assert units==json.loads((E/'UNIT-RUNS.json').read_text())
expected=json.loads((ROOT/'docs/foundation-production-admission/RESULTS.json').read_text())['unitTests']['projects'];expected.update({'Bosak.XPath.Core.Tests.dll':162,'Bosak.XPath.Runtime.Tests.dll':281})
assert len(units)==10 and all(v['exitCode']==0 for v in units)
counts={};ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
with zipfile.ZipFile(E/'unit-trx.zip') as z:
 assert len(z.namelist())==10
 for v in units:
  xml=ET.fromstring(z.read(v['name']+'.trx'));c=xml.find('t:ResultSummary/t:Counters',ns).attrib
  assert int(c['failed'])==int(c['error'])==int(c['timeout'])==int(c['notExecuted'])==0
  assert int(c['total'])==int(c['executed'])==int(c['passed'])==expected[v['name']+'.dll']
  counts[v['name']+'.dll']=int(c['passed'])
  for n,h in v['assemblies'].items():
   if n in layout['engines']:assert h==layout['engines'][n]['sha256']
  seen=[]
  for f in (E/(v['name']+'-bindings')).glob('*.json'):
   d=json.loads(f.read_text());assert not d['errors'];seen.extend(d['assemblies'])
  assert any(a['name']==v['name'] and a['sha256']==v['assemblies'][v['name']+'.dll'] for a in seen)
  assert any(a['name']=='Bosak.XPath.Core' for a in seen)
  for a in seen:
   if a['name']+'.dll' in layout['engines']:assert a['sha256']==layout['engines'][a['name']+'.dll']['sha256']
assert counts==expected and sum(counts.values())==r['unitTests']['passed']==24267
for label in ['qt3','qt4']:
 seen=[]
 for f in (E/(label+'-bindings')).glob('*.json'):
  d=json.loads(f.read_text());assert not d['errors'];seen.extend(d['assemblies'])
 assert {a['name']+'.dll' for a in seen if a['name']+'.dll' in layout['seven']}==set(layout['seven'])
 for a in seen:
  if a['name']+'.dll' in layout['engines']:assert a['sha256']==layout['engines'][a['name']+'.dll']['sha256']
a=json.loads((E/'baseline-metadata.json').read_text());b=json.loads((E/'candidate-metadata.json').read_text())
assert len(a['surface'])==len(b['surface'])==78 and sum(len(t['members']) for t in a['surface'])==1199 and sum(len(t['members']) for t in b['surface'])==1201
aa={t['name']:t['members'] for t in a['surface']};bb={t['name']:t['members'] for t in b['surface']}
assert set(aa)==set(bb) and [(n,set(bb[n])-set(aa[n]),set(aa[n])-set(bb[n])) for n in bb if aa[n]!=bb[n]]==[('Bosak.XPath.Core.Xdm.XdmValue',{'Boolean Equals(System.Object)','Int32 GetHashCode()'},set())]
assert a['inheritedSurface']==b['inheritedSurface'] and a['xdmValueSize']==b['xdmValueSize']==48
for old,new in zip(a['methods'],b['methods']):
 assert old['declaredOn']=='System.ValueType' and new['declaredOn']=='Bosak.XPath.Core.Xdm.XdmValue'
 assert {k:v for k,v in old.items() if k!='declaredOn'}=={k:v for k,v in new.items() if k!='declaredOn'}
assert {a['name']+'.dll':a['sha256'] for a in b['assemblies']}==layout['seven']
versions=json.loads((E/'version-info.json').read_text(encoding='utf-8-sig'));assert {v['name']+'.dll' for v in versions}==set(layout['seven'])
assert all(v['assemblyVersion']==v['fileVersion']=='1.36.0.0' for v in versions)
old=json.loads((ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary.json').read_text());legacy=json.loads((E/'legacy-caller.json').read_text())
assert legacy['passed']==20 and legacy['failed']==0 and legacy['cases']==old['cases'] and legacy['callerSha256']==old['callerSha256']
assert {a['name']+'.dll':a['sha256'] for a in legacy['loadedAssemblies']}==layout['seven']
for label,total in [('native',121),('atomic',376)]:
 d=json.loads((E/(label+'-candidate.json')).read_text());assert d['checkCount']==len(d['checks'])==total and all(c['passed'] for c in d['checks'])
 assert {a['name']+'.dll':a['sha256'] for a in d['bindings']}==layout['seven']
 if label=='atomic':
  assert d['fixtureCount']==39 and d['kindCount']==11 and d['xdmValueSize']==48 and len(d['costs'])==30
  for g in d['costs']:
   saved=r['freshCohortCosts'][f"{g['kind']}|{g['operation']}|{g['count']}"];assert len(g['samples'])==saved['samples']==7
   assert saved['medianBytesPerOp']==statistics.median(v['allocatedBytes']/g['count'] for v in g['samples'])
   assert saved['medianNsPerOp']==statistics.median(v['nanoseconds']/g['count'] for v in g['samples'])
   wanted={'payload-access':0,'cached-kernel-read':0,'issued-value-read':0 if g['kind']=='integer' else 32,'evidence-read':0,'exact-noop':32 if g['kind']=='integer' else 64}[g['operation']]
   assert saved['medianBytesPerOp']==wanted
assert '5 Warning(s)' in (E/'solution-build.txt').read_text() and '0 Error(s)' in (E/'solution-build.txt').read_text()
assert r['newRegressionCases']==22 and r['pairedOverridesImplemented'] and not r['c45Accepted'] and not r['apiFreezeAccepted'] and not r['published']
print('PASS: applied1.36.0, 24,267 unit passes, exact QT3/qt4/skip maps, observed DLL hashes, legacy20/0, native121/atomic376, versioned metadata and allocation profile; no C45/freeze/publication inferred.')
