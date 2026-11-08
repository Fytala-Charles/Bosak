"""Verify the corrected immutable stock release cohort without publication."""
from pathlib import Path
import json,hashlib,zipfile,subprocess,xml.etree.ElementTree as ET,re
from packages import inspect
P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence';read=lambda f:json.loads(f.read_text(encoding='utf-8'));sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
p=read(P/'PLAN.json');s=read(E/'RESULTS.json');source=Path(p['source'])
assert p['candidateCommit']=='1d69d059c14888c1c43264efa8668909292af858' and s['singleFullSolutionInvocationGreen']
assert subprocess.check_output(['git','-C',str(source),'rev-parse','HEAD'],text=True).strip()==p['candidateCommit']
assert not subprocess.check_output(['git','-C',str(source),'ls-files','--','vscode-bosak/server'],text=True).strip()
for name,h in p['sourceTrees'].items():assert subprocess.check_output(['git','-C',str(source),'rev-parse','HEAD:'+name],text=True).strip()==h
assert inspect()==read(E/'PACKAGES.json')
for row in read(E/'SEAL-VERIFICATION.json'):
 folder=Path(p['feed']) if row['kind']=='nuget' else E/'vsix-out'
 assert sha(folder/'release-manifest.json')==row['manifestSha256']
 out=subprocess.run(['python',str(source/'.github/scripts/release_artifacts.py'),'verify',row['kind'],str(folder),'--expected-sha',row['manifestSha256']],capture_output=True,text=True);assert out.returncode==0,out.stderr
results=read(E/'TEST-RESULTS.json');assert len(results['solution'])==10 and sum(x['passed'] for x in results['solution'])==26342
assert sorted(x['passed'] for x in results['extras'])==[10,59,90,451]
for row in results['solution']+results['extras']:
 f=P/row['trx'];assert sha(f)==row['trxSha256'];root=ET.parse(f);c=root.find('.//{*}Counters').attrib;assert int(c['passed'])==int(c['total'])==row['passed'] and c['failed']==c['notExecuted']=='0'
 assert sorted(x.attrib['testName'] for x in root.findall('.//{*}UnitTestResult'))==row['caseNames']
assert read(E/'RETAINED-FOUNDATION.json')['passed']==20 and read(E/'CONSUMER-RESULTS.json')['passed']==7
for name,expected in [('qt3',[31397,0,424]),('qt4',[8941,0,1918])]:
 text=(E/(name+'.txt')).read_text(encoding='utf-8-sig');counts=[int(re.search('^'+label+r':\s+(\d+)',text,re.M).group(1)) for label in ['Passed','Failed','Skipped']];assert counts==expected
 assert (E/(name+'-skips.txt')).read_bytes()==(R/'docs/foundation-preview-qualification/evidence'/(name+'-skips.txt')).read_bytes()
api=read(E/'API-COMPARISON.json');assert api['publicTypes']==318 and api['declarationRows']==6465 and api['packaged']==10
contents=read(E/'VSIX-CONTENTS.json');archive=P/contents['archive'];assert sha(archive)==contents['archiveSha256']
with zipfile.ZipFile(archive) as z:
 assert contents['files']=={i.filename:hashlib.sha256(z.read(i)).hexdigest() for i in z.infolist() if not i.is_dir()}
 for folder in ['final-fresh','upgrade']:
  root=Path(p['work'])/folder/'extensions/fytala.vscode-bosak-0.2.6'
  for name,h in contents['files'].items():
   if not name.startswith('extension/'):continue
   f=root/name.removeprefix('extension/')
   if name=='extension/package.json':
    m=read(f);m.pop('__metadata');assert m==json.loads(z.read(name))
   else:assert sha(f)==h
commands=read(E/'COMMANDS.json');assert len(commands['results'])==5 and all(x['passed'] and x['actual']==x['expected'] for x in commands['results']) and commands['defaultBundledServer']
editor=read(E/'EDITOR.json');assert len(editor['results'])==5 and any('XPST0017' in x for x in editor['results'][1]['diagnostics']) and all(not x['diagnostics'] for i,x in enumerate(editor['results']) if i!=1)
assert 'exited with code: 0' in (E/'editor-stdout.txt').read_text() and 'FATAL:' not in (E/'editor-stderr.txt').read_text()
packages=read(E/'PACKAGES.json')['loadedEngines'];seen={}
for f in (E/'bindings').rglob('*.json'):
 report=read(f);assert not report['captureErrors']
 for a in report['assemblies']:
  if a['name'] in packages:assert a['sha256']==packages[a['name']] and a['context']=='Default' and a['collectible']=='False';seen[a['name']]=a['sha256']
assert seen==packages
assert all(not s[k] for k in ['ownerAccepted','budgetsAccepted','apiFrozen','publicationAuthorized','publicationPerformed','hostedWorkflowExecuted','publisherAuthenticationTested','machinePermissionsChanged','activeProfileChanged'])
for f in [P/'README.md',R/'docs/RELEASING.md',R/'docs/release-1.81.2-stock/README.md']:
 text=re.sub(r'`[^`]*`','',f.read_text(encoding='utf-8'))
 for target in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',text):
  target=target.strip('<>').split('#')[0]
  if target and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',target):assert (f.parent/target).exists(),(f,target)
assert read(P/'MANIFEST.json')=={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f!=P/'MANIFEST.json' and '__pycache__' not in f.parts}
print('Verified corrected-source stock ten-package/VSIX seals, single-run26342/server90, retained20, native451/59/10, exact gates/API, and installed commands/live correction. No hosted publication or owner acceptance.')
