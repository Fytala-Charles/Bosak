"""Verify the actual production source/archive/build/test/gate/API cohort."""
from pathlib import Path
import json,hashlib,re,zipfile,xml.etree.ElementTree as ET,subprocess
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];E=P/'evidence';sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
s=json.loads((E/'SOURCE.json').read_text());i=json.loads((E/'INPUTS.json').read_text());r=json.loads((E/'RESULTS.json').read_text())
assert s['version']==i['version']==r['version']=='1.37.0' and i['productionSourceApplied'] and not r['published'] and not r['apiFreeze']
assert sha(E/'source.zip')==s['sourceArchiveSha256']
with zipfile.ZipFile(E/'source.zip') as z:
 assert set(z.namelist())==set(s['files'])
 for name,digest in s['files'].items():assert hashlib.sha256(z.read(name)).hexdigest()==digest and sha(ROOT/name)==digest,name
assert sha(ROOT/'src/Bosak.Xslt/Bosak.Xslt.csproj')==s['projectSha256']==json.loads((ROOT/'docs/foundation-xslt-context-qualification/evidence/CORRECTION.json').read_text())['originalProjectSha256']
assert '0 Error(s)' in (E/'solution-build.txt').read_text() and '5 Warning(s)' in (E/'solution-build.txt').read_text()
warnings=lambda path:set(re.findall(r'^.*warning CS\d+:.*$',path.read_text(),re.M))
assert warnings(E/'solution-build.txt')==warnings(ROOT/'docs/foundation-transport136/evidence/solution-build.txt')
for name,v in i['engines'].items():assert sha(Path(v['path']))==v['sha256']
assert sha(E/'product-binaries.zip')==r['productArchiveSha256']
with zipfile.ZipFile(E/'product-binaries.zip') as z:
 for name,v in i['engines'].items():assert hashlib.sha256(z.read(name)).hexdigest()==v['sha256']
api=json.loads((E/'public-api-0.json').read_text());assert api==json.loads((E/'public-api-1.json').read_text())
baseline=json.loads((ROOT/'docs/foundation-c45-delta136/evidence/public-api.json').read_text());old={a['assembly']:a for a in baseline['assemblies']}
assert len(api['assemblies'])==11 and sum(a['publicTypes'] for a in api['assemblies'])==270 and sum(a['declarationCount'] for a in api['assemblies'])==5770
for a in api['assemblies']:
 assert a['declarations']==old[a['assembly']]['declarations'] and 'Version=1.37.0.0' in a['identity']
 assert sha(ROOT/Path(a['project']).parent/'bin/Release/net10.0'/(a['assembly']+'.dll'))==a['sha256'].lower()
assert next(a for a in api['assemblies'] if a['assembly']=='Bosak.Xslt')['friendAssemblies']==api['baselineXsltFriends']==['Bosak.Xslt.Conformance','Bosak.Xslt.Tests']
units=json.loads((E/'UNITS.json').read_text());assert len(units)==10 and len({x['name'] for x in units})==10
expected=json.loads((ROOT/'docs/foundation-xslt-context-qualification/evidence/RESULTS.json').read_text())['unitTests']['projects'];expected['Bosak.Xslt.Tests']+=17
with zipfile.ZipFile(E/'unit-trx.zip') as z:
 assert len(z.namelist())==10
 for row in units:
  c=row['counters'];assert row['exitCode']==0 and int(c['passed'])==expected[row['name']] and int(c['total'])==int(c['executed'])==int(c['passed'])
  for k in ['failed','error','timeout','notExecuted']:assert int(c[k])==0
  assert ET.fromstring(z.read(row['name']+'.trx')).find('{*}ResultSummary/{*}Counters').attrib==c
assert sum(int(row['counters']['passed']) for row in units)==r['unitTests']['passed']==24284
assert len(r['newRegressionCases'])==17 and all(c['outcome']=='Passed' for c in r['newRegressionCases'])
assert len(r['fnTransformControls'])==8 and all(c['outcome']=='Passed' for c in r['fnTransformControls'])
assert json.loads((E/'GATES.json').read_text())=={'qt3':0,'qt4':0}
for label,counts in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
 for key,count in zip(['Passed','Failed','Skipped'],counts):assert re.search(rf'^{key}:\s+{count}\s*$',(E/(label+'.txt')).read_text(),re.M)
 assert (E/(label+'-skips.txt')).read_bytes().replace(b'\r\n',b'\n')==(ROOT/'docs/foundation-xslt-context-qualification/evidence'/(label+'-skips.txt')).read_bytes().replace(b'\r\n',b'\n')
assert 'Experimental-level (XPath40Experimental) passes: 11' in (E/'qt4.txt').read_text()
after=json.loads((E/'CORPORA-AFTER.json').read_text());assert after==s['corporaBefore']
for name,v in after.items():
 assert not v['status'] and subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT/'tests'/name,text=True).strip()==v['revision']
 assert not subprocess.check_output(['git','status','--porcelain'],cwd=ROOT/'tests'/name,text=True)
for row in i['records']:
 label=row['name'] if row['kind']=='unit' else {'Bosak.XPath.Conformance':'qt3','Bosak.XPath.Conformance40':'qt4'}[row['name']]
 seen=[]
 for f in (E/(label+'-bindings')).glob('*.json'):
  d=json.loads(f.read_text());assert not d['errors'];seen.extend(d['assemblies'])
 assert any(b['name']==row['name'] and b['sha256']==row['assemblies'][row['name']+'.dll'] for b in seen)
 for b in seen:
  name=b['name']+'.dll'
  if name in i['engines']:assert b['sha256']==i['engines'][name]['sha256']
 if label in ['Bosak.Xslt.Tests','qt3','qt4']:assert any(b['name']=='Bosak.Xslt' for b in seen)
 for name,digest in row['assemblies'].items():assert sha(Path(row['directory'])/name)==digest and sha(Path(row['source'])/name)==digest
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified actual 1.37.0 production source/build, 24,284 unit passes, 17 regressions, frozen gates, eleven unchanged declaration surfaces and original XSLT friends.')
