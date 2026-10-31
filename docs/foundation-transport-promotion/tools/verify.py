"""Read-only checks of the prepared patch and its isolated preflight receipts."""
from pathlib import Path
import hashlib,json,zipfile,difflib,xml.etree.ElementTree as ET,sys
P=Path(__file__).resolve().parents[1];ROOT=P.parents[1];E=P/'evidence';sha=lambda b:hashlib.sha256(b).hexdigest()
for n,h in json.loads((P/'MANIFEST.json').read_text()).items():assert sha((P/n).read_bytes())==h,n
m=json.loads((P/'PATCH.json').read_text());r=json.loads((P/'RESULTS.json').read_text());d=json.loads((P/'DISPOSITION.json').read_text())
assert len(m['files'])==6 and len(m['qualifiedBodies'])==3 and not m['livePatchApplied']
assert m['proposedVersion']=='1.36.0' and m['liveVersion']=='1.35.0' and sha((P/'PRODUCTION.patch').read_bytes())==m['patchSha256']
significant=lambda text:'\n'.join(line for line in text.splitlines() if line.strip() and not line.lstrip().startswith('//'))
patch=[]
archive=ROOT/'docs/foundation-transport-full-gates/evidence/source-baseline.zip'
with zipfile.ZipFile(archive) as z:
 for n,v in m['files'].items():
  old=z.read(n).replace(b'\r\n',b'\n') if v['beforeSha256'] is not None else b'';new=(P/v['file']).read_bytes()
  assert (sha(old) if v['beforeSha256'] is not None else None)==v['beforeSha256'] and sha(new)==v['afterSha256']
  patch.extend(difflib.unified_diff(old.decode('utf-8-sig').splitlines(True),new.decode('utf-8').splitlines(True),fromfile='a/'+n if v['beforeSha256'] is not None else '/dev/null',tofile='b/'+n))
  if n.endswith('.cs'):
   text=new.decode('utf-8');assert text.startswith('// ===') and 'Generated' not in text[:200] and 'COPYRIGHT            : Fytala' in text and 'SPDX-License-Identifier: Apache-2.0' in text
   purpose=next(line for line in text.splitlines() if line.startswith('// PURPOSE')).split(':',1)[1].strip();assert len(purpose)<100
   assert '31-10-2026' in text.split('namespace',1)[0]
  if n in m['qualifiedBodies']:
   q=m['qualifiedBodies'][n];original=ROOT/'docs/foundation-transport-atomic-cost/candidate'/(Path(n).name+'.txt')
   assert sha(original.read_bytes())==q['qualifiedSourceSha256']
   assert significant(original.read_text(encoding='utf-8'))==significant(new.decode('utf-8'))
   assert sha(significant(new.decode('utf-8')).encode('utf-8'))==q['significantLinesSha256']
assert (P/'PRODUCTION.patch').read_bytes()==''.join(patch).encode('utf-8')
assert '<Version>1.36.0</Version>' in (P/m['files']['src/Directory.Build.props']['file']).read_text()
material=json.loads((E/'MATERIALIZED.json').read_text())
for n,v in material['files'].items():assert v['lfSha256']==m['files'][n]['afterSha256']
if '--check-current' in sys.argv:
 W=ROOT/'.guard-tmp/transport-promotion-preflight'
 for n,v in m['files'].items():
  if v['beforeSha256'] is None:assert not (ROOT/n).exists()
  else:assert sha((ROOT/n).read_bytes().replace(b'\r\n',b'\n'))==v['beforeSha256']
  assert sha((W/n).read_bytes())==material['files'][n]['sha256']
 for n,h in json.loads((E/'LAYOUT.json').read_text())['seven'].items():assert sha((W/'lib'/n).read_bytes())==h
ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
def counts(folder):
 with zipfile.ZipFile(folder/'preflight-trx.zip') as z:
  assert len(z.namelist())==2;out={}
  for n in z.namelist():
   c=ET.fromstring(z.read(n)).find('t:ResultSummary/t:Counters',ns).attrib
   assert int(c['failed'])==0 and int(c['total'])==int(c['executed'])==int(c['passed'])
   out[n.removesuffix('.trx')]=int(c['passed'])
  return out
assert counts(E)==r['preflightTests']['projects']=={'Bosak.XPath.Core.Tests':162,'Bosak.XPath.Runtime.Tests':281} and r['preflightTests']['passed']==443 and r['newRegressionCases']==22
for name in ['Bosak.XPath.Core.Tests-build.txt','Bosak.XPath.Runtime.Tests-build.txt','metadata-build.txt']:
 text=(E/name).read_text();assert '0 Warning(s)' in text and '0 Error(s)' in text
layout=json.loads((E/'LAYOUT.json').read_text())['seven'];units=json.loads((E/'UNIT-RUNS.json').read_text())
assert len(layout)==7 and all(v['exitCode']==0 for v in units)
for v in units:assert {n:h for n,h in v['assemblies'].items() if n in layout}==layout
meta=json.loads((E/'metadata.json').read_text());prior=json.loads((ROOT/'docs/foundation-transport-full-gates/evidence/candidate-metadata.json').read_text())
assert meta['surface']==prior['surface'] and meta['inheritedSurface']==prior['inheritedSurface'] and meta['methods']==prior['methods']
assert len(meta['surface'])==78 and sum(len(t['members']) for t in meta['surface'])==1201 and meta['xdmValueSize']==48
assert {a['name']+'.dll':a['sha256'] for a in meta['assemblies']}==layout
versions=json.loads((E/'version-info.json').read_text(encoding='utf-8-sig'));assert {v['name']+'.dll' for v in versions}==set(layout)
assert all(v['assemblyVersion']==v['fileVersion']=='1.36.0.0' for v in versions)
legacy=json.loads((E/'legacy-caller.json').read_text());old=json.loads((ROOT/'docs/foundation-legacy-compatibility/evidence/unchanged-binary.json').read_text())
assert legacy['passed']==20 and legacy['failed']==0 and legacy['callerSha256']==old['callerSha256'] and legacy['cases']==old['cases']
assert {v['name']+'.dll':v['sha256'] for v in legacy['loadedAssemblies']}==layout
p=json.loads((E/'PREFLIGHT.json').read_text());assert p['sourceArchiveSha256']==sha(archive.read_bytes()) and p['patchSha256']==m['patchSha256'] and not p['livePatchApplied']
for phase in ['initial-preflight','before-explicit-xml']:
 f=E/phase;original=json.loads((f/'PATCH.json').read_text())
 assert sha((f/'PRODUCTION.patch').read_bytes())==original['patchSha256']
 for v in original['files'].values():assert sha((f/v['file']).read_bytes())==v['afterSha256']
 assert counts(f)=={'Bosak.XPath.Core.Tests':162,'Bosak.XPath.Runtime.Tests':281}
 assert json.loads((f/'legacy-caller.json').read_text())['passed']==20
assert 'warning xUnit2017' in (E/'initial-preflight/Bosak.XPath.Core.Tests-build.txt').read_text()
assert 'running scripts is disabled' in ' '.join((E/'before-explicit-xml/version-policy-attempt.txt').read_text().split())
first=json.loads((E/'initial-preflight/ATTEMPT-1.json').read_text());assert all(v['lfSha256']==v['expectedLfSha256'] for v in first['files'].values())
assert not r['productionApplied'] and r['liveApplyCheckExitCode']==0 and all(r[n]=='NotRun' for n in ['fullVersion136SolutionGate','fullVersion136Qt3Gate','fullVersion136Qt4Gate'])
assert not d['productionApplied'] and not d['ownerPublicManifestAccepted'] and not d['c45Accepted'] and not d['apiFreezeAccepted']
print('PASS: six-file 1.36.0 proposal, unchanged implementation bodies, 22 new regressions/443 passes, zero-warning builds, versioned metadata, legacy20/0; live source1.35.0 and final-version full gates pending.')
