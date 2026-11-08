"""Read-only checks for the complete rebuilt candidate, tests, gates and API review."""
from pathlib import Path
import json,hashlib,zipfile,xml.etree.ElementTree as ET,re,sys,statistics
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];E=P/'evidence';sha=lambda b:hashlib.sha256(b).hexdigest()
for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha((P/n).read_bytes())==h,n
source=json.loads((E/'SOURCE.json').read_text());layout=json.loads((E/'LAYOUT.json').read_text());r=json.loads((P/'RESULTS.json').read_text())
assert sha((E/'source-baseline.zip').read_bytes())==source['archiveSha256'] and not source['productionModified']
assert len(source['overlays'])==3 and len(layout['engines'])==10 and len(layout['seven'])==7
materialized=json.loads((E/'SOURCE-MATERIALIZATION.json').read_text())
assert materialized['sourceRevision']==source['revision'] and materialized['overlays']==source['overlays'] and materialized['unmodifiedSourceAndTestsSinceSemanticBaseline']
with zipfile.ZipFile(E/'source-baseline.zip') as z:
 assert set(materialized['files'])=={n for n in z.namelist() if not n.endswith('/')}
 for n,h in materialized['files'].items():assert h==(source['overlays'].get(n) or sha(z.read(n)))
for path,h in source['overlays'].items():assert sha((ROOT/'docs/foundation-transport-atomic-cost/candidate'/(Path(path).name+'.txt')).read_bytes())==h
if '--check-current' in sys.argv:
 with zipfile.ZipFile(E/'source-baseline.zip') as z:
  W=ROOT/'.guard-tmp/transport-full-gates'
  for n in z.namelist():
   if n.endswith('/'):continue
   assert sha((W/n).read_bytes())==(source['overlays'].get(n) or sha(z.read(n))),n
after=json.loads((E/'CORPUS-AFTER.json').read_text())
for n,c in source['corpora'].items():assert after[n]=={'revision':c['revision'],'status':c['statusBefore']} and c['statusBefore']==''
for label,counts in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
 text=(E/(label+'.txt')).read_text()
 for field,wanted in zip(['Passed','Failed','Skipped'],counts):assert re.search(rf'^{field}:\s+{wanted}\s*$',text,re.M)
 assert (E/(label+'-skips.txt')).read_bytes().replace(b'\r\n',b'\n')==(ROOT/'docs/foundation-production-admission/evidence'/(label+'-skips.txt')).read_bytes().replace(b'\r\n',b'\n')
assert 'Experimental-level (XPath40Experimental) passes: 11' in (E/'qt4.txt').read_text()
assert json.loads((E/'GATE-RUNS.json').read_text())=={'qt3':0,'qt4':0}
initial=json.loads((E/'UNIT-RUNS.json').read_text());units=json.loads((E/'UNIT-FINAL.json').read_text());expected=json.loads((ROOT/'docs/foundation-production-admission/RESULTS.json').read_text())['unitTests']['projects']
assert len(units)==10 and all(v['exitCode']==0 for v in units)
assert [v['name'] for v in initial if v['exitCode']!=0]==['Bosak.XPath.Providers.Database.Tests']
assert [v for v in initial if v['name']!='Bosak.XPath.Providers.Database.Tests']==[v for v in units if v['name']!='Bosak.XPath.Providers.Database.Tests']
counts={};ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
with zipfile.ZipFile(E/'unit-trx.zip') as z:
 xml=ET.fromstring(z.read('Bosak.XPath.Providers.Database.Tests.trx'));c=xml.find('t:ResultSummary/t:Counters',ns).attrib
 assert int(c['passed'])==2 and int(c['failed'])==45
 failed=[v for v in xml.findall('t:Results/t:UnitTestResult',ns) if v.attrib['outcome']=='Failed'];assert len(failed)==45
 assert all('HttpListenerException' in v.find('t:Output/t:ErrorInfo/t:Message',ns).text for v in failed)
with zipfile.ZipFile(E/'unit-trx.zip') as z:
 assert len(z.namelist())==10
 for v in units:
  if v['name']=='Bosak.XPath.Providers.Database.Tests':
   with zipfile.ZipFile(E/'database-rerun-trx.zip') as retry:xml=ET.fromstring(retry.read(v['name']+'.trx'))
  else:xml=ET.fromstring(z.read(v['name']+'.trx'))
  c=xml.find('t:ResultSummary/t:Counters',ns).attrib
  assert int(c['failed'])==int(c['error'])==int(c['timeout'])==int(c['notExecuted'])==0
  assert int(c['total'])==int(c['executed'])==int(c['passed'])==expected[v['name']+'.dll']
  counts[v['name']+'.dll']=int(c['passed'])
  for n,h in v['assemblies'].items():
   if n in layout['engines']:assert h==layout['engines'][n]['sha256']
  seen=[]
  folder=E/('database-rerun-bindings' if v['name']=='Bosak.XPath.Providers.Database.Tests' else v['name']+'-bindings')
  for f in folder.glob('*.json'):
   d=json.loads(f.read_text());assert not d['errors'];seen.extend(d['assemblies'])
  assert any(a['name']==v['name'] and a['sha256']==v['assemblies'][v['name']+'.dll'] for a in seen)
  assert any(a['name']=='Bosak.XPath.Core' for a in seen)
  for a in seen:
   n=a['name']+'.dll'
   if n in layout['engines']:assert a['sha256']==layout['engines'][n]['sha256']
assert counts==expected and sum(counts.values())==r['unitTests']['passed']==24245
for label in ['qt3','qt4']:
 seen=[]
 for f in (E/(label+'-bindings')).glob('*.json'):
  d=json.loads(f.read_text());assert not d['errors'];seen.extend(d['assemblies'])
 assert {a['name']+'.dll' for a in seen if a['name']+'.dll' in layout['seven']}==set(layout['seven'])
 for a in seen:
  if a['name']+'.dll' in layout['engines']:assert a['sha256']==layout['engines'][a['name']+'.dll']['sha256']
a=json.loads((E/'baseline-metadata.json').read_text());b=json.loads((E/'candidate-metadata.json').read_text())
assert a['surface']==json.loads((ROOT/'docs/foundation-production-admission/evidence/candidate-public-surface.json').read_text())['surface']
assert len(a['surface'])==len(b['surface'])==78 and sum(len(t['members']) for t in a['surface'])==1199 and sum(len(t['members']) for t in b['surface'])==1201
aa={t['name']:t['members'] for t in a['surface']};bb={t['name']:t['members'] for t in b['surface']};assert set(aa)==set(bb)
assert [(n,set(bb[n])-set(aa[n]),set(aa[n])-set(bb[n])) for n in bb if aa[n]!=bb[n]]==[('Bosak.XPath.Core.Xdm.XdmValue',{'Boolean Equals(System.Object)','Int32 GetHashCode()'},set())]
assert a['inheritedSurface']==b['inheritedSurface'] and a['xdmValueSize']==b['xdmValueSize']==48
for old,new in zip(a['methods'],b['methods']):
 assert old['declaredOn']=='System.ValueType' and new['declaredOn']=='Bosak.XPath.Core.Xdm.XdmValue'
 assert {k:v for k,v in old.items() if k!='declaredOn'}=={k:v for k,v in new.items() if k!='declaredOn'}
assert {a['name']+'.dll':a['sha256'] for a in b['assemblies']}==layout['seven']
old=json.loads((ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary.json').read_text());legacy=json.loads((E/'legacy-caller.json').read_text())
assert legacy['passed']==20 and legacy['failed']==0 and legacy['cases']==old['cases'] and legacy['callerSha256']==old['callerSha256']
assert {a['name']+'.dll':a['sha256'] for a in legacy['loadedAssemblies']}==layout['seven']
for label,total in [('native',121),('atomic',376)]:
 d=json.loads((E/(label+'-candidate.json')).read_text());assert d['checkCount']==len(d['checks'])==total and all(c['passed'] for c in d['checks'])
 assert {a['name']+'.dll':a['sha256'] for a in d['bindings']}==layout['seven']
 if label=='atomic':
  assert d['fixtureCount']==39 and d['kindCount']==11 and d['xdmValueSize']==48 and len(d['costs'])==30
  for g in d['costs']:
   key=f"{g['kind']}|{g['operation']}|{g['count']}";saved=r['freshCohortCosts'][key]
   assert len(g['samples'])==saved['samples']==7
   assert saved['medianBytesPerOp']==statistics.median(v['allocatedBytes']/g['count'] for v in g['samples'])
   assert saved['medianNsPerOp']==statistics.median(v['nanoseconds']/g['count'] for v in g['samples'])
   want={'payload-access':0,'cached-kernel-read':0,'issued-value-read':0 if g['kind']=='integer' else 32,'evidence-read':0,'exact-noop':32 if g['kind']=='integer' else 64}[g['operation']]
   assert saved['medianBytesPerOp']==want
assert '5 Warning(s)' in (E/'solution-build.txt').read_text() and '0 Error(s)' in (E/'solution-build.txt').read_text()
assert not r['productionModified'] and not r['publicManifestAccepted'] and not r['apiFreezeAccepted']
print('PASS: full rebuilt cohort, 24,245 unit passes, exact QT3/qt4 gates and skip maps, actual loaded hashes, legacy20/0, native121/atomic376; only two declared equality overrides, no freeze acceptance.')
