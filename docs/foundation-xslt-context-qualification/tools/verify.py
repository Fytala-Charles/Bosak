"""Verify retained test/gate qualification against the exact isolated candidate cohort."""
from pathlib import Path
import hashlib,json,re,zipfile,xml.etree.ElementTree as ET,subprocess
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
i=json.loads((E/'INPUTS.json').read_text());units=json.loads((E/'UNITS.json').read_text())
assert not i['canonicalChanged'] and not i['productionPromoted']
candidate=json.loads((ROOT/'docs/foundation-xslt-explicit-context/evidence/INPUTS.json').read_text());correction=json.loads((E/'CORRECTION.json').read_text())
assert i['projectMetadataCorrected'] and i['originalCandidateSha256']==candidate['candidateSha256']==correction['originalCandidateSha256']
assert i['candidateSha256']==correction['correctedCandidateSha256']==sha(Path(i['candidatePath']))
assert correction['candidateCSharpUnchanged']==candidate['patchedFiles'] and not correction['newFriendGrants']
assert sha(ROOT/'src/Bosak.Xslt/Bosak.Xslt.csproj')==correction['originalProjectSha256']
assert sha(ROOT/'.guard-tmp/xslt-context-qualification/Corrected/src/Bosak.Xslt/Bosak.Xslt.csproj')==correction['correctedProjectSha256']
artifacts=json.loads((E/'ARTIFACTS.json').read_text())
assert sha(E/'candidate-source.zip')==artifacts['sourceArchiveSha256'] and sha(E/'candidate-binaries.zip')==artifacts['binaryArchiveSha256']
with zipfile.ZipFile(E/'candidate-source.zip') as z:
 assert set(z.namelist())==set(artifacts['sourceFiles'])
 for name,digest in artifacts['sourceFiles'].items():assert hashlib.sha256(z.read(name)).hexdigest()==digest
 for name,digest in correction['candidateCSharpUnchanged'].items():assert hashlib.sha256(z.read('src/Bosak.Xslt/'+name)).hexdigest()==digest
with zipfile.ZipFile(E/'candidate-binaries.zip') as z:
 assert hashlib.sha256(z.read('original/Bosak.Xslt.dll')).hexdigest()==i['originalCandidateSha256']
 assert hashlib.sha256(z.read('corrected/Bosak.Xslt.dll')).hexdigest()==i['candidateSha256']
explicit=json.loads((E/'EXPLICIT-AND-API.json').read_text());assert explicit['candidateSha256']==i['candidateSha256'] and explicit['explicitPassed']==explicit['explicitCases']==22 and explicit['publicDeclarationsIdentical']
assert json.loads((E/'explicit-0.json').read_text())==json.loads((E/'explicit-1.json').read_text())
assert all(c['passed'] for c in json.loads((E/'explicit-0.json').read_text())['cases'])
for b in json.loads((E/'explicit-0.json').read_text())['bindings']:
 assert b['sha256']==(i['candidateSha256'] if b['name']=='Bosak.Xslt' else i['engines'][b['name']+'.dll']['sha256'])
old_api=json.loads((ROOT/'docs/foundation-xslt-explicit-context/evidence/candidate-api.json').read_text())['assemblies'][0]
new_api=json.loads((E/'corrected-api.json').read_text())['assemblies'][0]
assert new_api['declarations']==old_api['declarations'] and new_api['sha256'].lower()==i['candidateSha256']
initial=json.loads((E/'attempt-1/UNITS.json').read_text())
assert next(r for r in initial if r['name']=='Bosak.Xslt.Tests')['counters']['failed']=='200'
assert json.loads((E/'attempt-1/INPUTS.json').read_text())['candidateSha256']==i['originalCandidateSha256']
for v in i['engines'].values():assert sha(Path(v['path']))==v['sha256']
assert sha(Path(i['observer']))==i['observerSha256']
assert sha(ROOT/'docs/foundation-transport-full-gates/tools/StartupHook.cs.txt')==i['observerSourceSha256']
expected=json.loads((ROOT/'docs/foundation-production-admission/RESULTS.json').read_text())['unitTests']['projects'];expected.update({'Bosak.XPath.Core.Tests.dll':162,'Bosak.XPath.Runtime.Tests.dll':281})
assert len(units)==10 and len({r['name'] for r in units})==10
for row in units:
 c=row['counters'];assert row['exitCode']==0 and int(c['passed'])==expected[row['name']+'.dll']
 assert int(c['total'])==int(c['executed'])==int(c['passed'])
 for key in ['failed','error','timeout','notExecuted']:assert int(c[key])==0
assert sum(int(r['counters']['passed']) for r in units)==24267
with zipfile.ZipFile(E/'unit-trx.zip') as z:
 assert len(z.namelist())==10
 for row in units:assert ET.fromstring(z.read(row['name']+'.trx')).find('{*}ResultSummary/{*}Counters').attrib==row['counters']
for row in i['records']:
 for n,digest in row['sourceAssemblies'].items():assert sha(Path(row['source'])/n)==digest
 for n,digest in row['assemblies'].items():
  assert sha(Path(row['directory'])/n)==digest
  if n in i['engines']:assert digest==(i['candidateSha256'] if n=='Bosak.Xslt.dll' else i['engines'][n]['sha256'])
 assert row['assemblies'][row['name']+'.dll']==row['sourceAssemblies'][row['name']+'.dll']
baseline_units={r['name']:r for r in json.loads((ROOT/'docs/foundation-transport136/evidence/UNIT-FINAL.json').read_text())}
for row in i['records']:
 if row['kind']=='unit':assert row['assemblies'][row['name']+'.dll']==baseline_units[row['name']]['assemblies'][row['name']+'.dll']
assert json.loads((E/'GATES.json').read_text())=={'qt3':0,'qt4':0}
for label,counts in [('qt3',(31397,0,424)),('qt4',(8941,0,1918))]:
 text=(E/(label+'.txt')).read_text()
 for key,count in zip(['Passed','Failed','Skipped'],counts):assert re.search(rf'^{key}:\s+{count}\s*$',text,re.M)
 assert (E/(label+'-skips.txt')).read_bytes().replace(b'\r\n',b'\n')==(ROOT/'docs/foundation-production-admission/evidence'/(label+'-skips.txt')).read_bytes().replace(b'\r\n',b'\n')
assert 'Experimental-level (XPath40Experimental) passes: 11' in (E/'qt4.txt').read_text()
after=json.loads((E/'CORPORA-AFTER.json').read_text());assert after==i['corporaBefore']
for name,v in after.items():
 assert not v['status']
 assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT/'tests'/name,text=True).strip()==v['revision']
 assert not subprocess.check_output(['git','status','--porcelain'],cwd=ROOT/'tests'/name,text=True)
bindings={}
for label in [r['name'] for r in units]+['qt3','qt4']:
 seen=[];files=list((E/(label+'-corrected-bindings')).glob('*.json'));assert files,label
 for f in files:
  d=json.loads(f.read_text());assert not d['errors'];seen.extend(d['assemblies'])
 for b in seen:
  n=b['name']+'.dll'
  if n in i['engines']:assert b['sha256']==(i['candidateSha256'] if n=='Bosak.Xslt.dll' else i['engines'][n]['sha256']),label+': '+n
 entry=label if label not in ['qt3','qt4'] else {'qt3':'Bosak.XPath.Conformance','qt4':'Bosak.XPath.Conformance40'}[label]
 row=next(r for r in i['records'] if r['name']==entry)
 assert any(b['name']==entry and b['sha256']==row['assemblies'][entry+'.dll'] for b in seen)
 assert any(b['name']=='Bosak.XPath.Core' for b in seen)
 bindings[label]=sorted({b['name'] for b in seen})
for label in ['Bosak.Xslt.Tests','qt3','qt4']:assert 'Bosak.Xslt' in bindings[label],label
summary=json.loads((E/'RESULTS.json').read_text());assert summary['qualifiedCandidateSha256']==i['candidateSha256'] and not summary['originalCandidateQualified'] and summary['unitTests']['passed']==24267 and summary['unitTests']['failed']==0
assert len(summary['fnTransformControls'])==8 and all(c['outcome']=='Passed' for c in summary['fnTransformControls'])
if (P/'MANIFEST.json').exists():
 m=json.loads((P/'MANIFEST.json').read_text());assert set(m)=={p.relative_to(P).as_posix() for p in P.rglob('*') if p.is_file() and p.name!='MANIFEST.json'}
 for name,digest in m.items():assert sha(P/name)==digest,name
print('Verified 24,267 retained unit passes, exact QT3/qt4 counts and skip maps, candidate bindings, unchanged canonical outputs and clean corpus pins.')
