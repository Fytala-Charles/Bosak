"""Read-only verification of ten approved local1.81.2 packages and extension0.2.6."""
from pathlib import Path
import json,hashlib,zipfile,xml.etree.ElementTree as ET,re,sys
from packages import inspect
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';read=lambda f:json.loads(f.read_text(encoding='utf-8'));sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
p=read(P/'PLAN.json');s=read(E/'RESULTS.json');assert p['version']=='1.81.2' and p['extensionVersion']=='0.2.6' and len(p['selectedPackages'])==10
assert inspect()==read(E/'PACKAGES.json')
for name,h in p['archivedFiles'].items():assert sha(Path(p['source'])/name)==h,name
fix=read(P/'FIXTURE-CORRECTION.json');assert fix['sourceCommit']==p['candidateCommit']
for name,h in fix['files'].items():assert sha(Path(p['source'])/name)==h
assert all(not s[k] for k in ['ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized','publicationPerformed'])
rows=read(E/'TEST-RESULTS.json');assert sum(x['passed'] for x in rows['solution'])==26342 and len(rows['solution'])==10
for row in rows['solution']+rows['extras']:
 trx=P/row['trx'];assert sha(trx)==row['trxSha256'];root=ET.parse(trx);c=root.find('.//{*}Counters').attrib;assert int(c['passed'])==int(c['total'])==row['passed'] and c['failed']==c['notExecuted']=='0'
 assert sorted(x.attrib['testName'] for x in root.findall('.//{*}UnitTestResult'))==row['caseNames']
assert sorted(x['passed'] for x in rows['extras'])==[10,59,90,451]
assert read(E/'RETAINED-FOUNDATION.json')['passed']==20 and read(E/'CONSUMER-RESULTS.json')['passed']==7
for name,expected in [('qt3',[31397,0,424]),('qt4',[8941,0,1918])]:
 log=(E/(name+'.txt')).read_text(encoding='utf-8-sig');counts=[int(re.search('^'+label+r':\s+(\d+)',log,re.M).group(1)) for label in ['Passed','Failed','Skipped']];assert counts==expected
 assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-preview-qualification/evidence'/(name+'-skips.txt')).read_bytes()
api=read(E/'API-COMPARISON.json');assert api['products']==11 and api['packaged']==10 and api['publicTypes']==318 and api['declarationRows']==6465
contents=read(E/'VSIX-CONTENTS.json');archive=P/contents['archive'];assert sha(archive)==contents['archiveSha256']
with zipfile.ZipFile(archive) as z:
 assert contents['files']=={i.filename:hashlib.sha256(z.read(i)).hexdigest() for i in z.infolist() if not i.is_dir()}
 for folder in ['final-fresh','upgrade']:
  root=Path(p['work'])/folder/'extensions/fytala.vscode-bosak-0.2.6'
  for name,h in contents['files'].items():
   if not name.startswith('extension/'):continue
   f=root/name.removeprefix('extension/')
   if name=='extension/package.json':
    manifest=read(f);manifest.pop('__metadata');assert manifest==json.loads(z.read(name))
   else:assert sha(f)==h
 assert 'extension/licenses/Unicode17-LICENSE.txt' in contents['files'] and 'extension/NOTICE' in contents['files']
commands=read(E/'COMMANDS.json');assert len(commands['results'])==5 and all(x['passed'] and x['actual']==x['expected'] for x in commands['results']);assert commands['defaultBundledServer']
editor=read(E/'EDITOR.json');assert len(editor['results'])==5 and any('XPST0017' in x for x in editor['results'][1]['diagnostics']) and all(not x['diagnostics'] for i,x in enumerate(editor['results']) if i!=1)
assert 'exited with code: 0' in (E/'editor-stdout.txt').read_text() and 'FATAL:' not in (E/'editor-stderr.txt').read_text()
assert '--no-sandbox' not in (P/'tools/editor-launch.ps1').read_text()
packages=read(E/'PACKAGES.json')['loadedEngines'];seen={}
for f in (E/'bindings').rglob('*.json'):
 report=read(f);assert not report['captureErrors']
 for a in report['assemblies']:
  if a['name'] in packages:
   assert a['sha256']==packages[a['name']] and a['context']=='Default' and a['collectible']=='False',(f,a['name']);seen[a['name']]=a['sha256']
assert seen==packages
assert read(P/'MANIFEST.json')=={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f!=P/'MANIFEST.json' and '__pycache__' not in f.parts}
print('Verified ten1.81.2 packages, consumer7/retained20, solution26342/server90/native451/adapter59/consumer10, exact frozen gates/API, and installed0.2.6 editor commands/diagnostics. No owner acceptance or publication.')
