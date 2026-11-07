"""Verify isolated source build and bounded passing scope without full-suite acceptance."""
from pathlib import Path
import hashlib,json,re,subprocess,xml.etree.ElementTree as ET,zipfile

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=read(P/'PLAN.json');results=read(E/'RESULTS.json');review=read(P/'PROCESS-REVIEW.json')
owner=Path(p['ownerRoot']);source=Path(p['source'])
assert subprocess.check_output(['git','--no-optional-locks','-C',str(owner),'rev-parse','HEAD'],text=True).strip()==p['ownerHead']
assert subprocess.check_output(['git','--no-optional-locks','-C',str(owner),'status','--porcelain'],text=True).strip()==p['ownerStatus']==''
assert sha(Path(p['config']))==p['configSha256'] and sha(Path(p['overlay']))==p['overlaySha256']
for name,h in p['sourceFiles'].items():
    if name in results['dependencyLockChanges']:
        assert Path(name).name=='packages.lock.json' and sha(source/name)==results['dependencyLockChanges'][name]
    else:assert sha(source/name)==h,name
assert results['sourceBuildPassed'] and not results['sourceBodiesChanged'] and results['inProcessPassed']==186
assert not results['fullQualificationComplete'] and results['integrationNotRun']==863 and results['storageCasesWithoutCompletedResult']==3433
assert review['status']=='BlockedByAutomaticApprovalReview' and not review['integrationProbesLaunched'] and not review['explicitUserAuthorizationReceived']
assert review['fullStorageSweepCancelled'] and not review['machineWideErrorSettingsChanged']
assert not p['productionAdapterImplemented'] and not p['ownerAccepted'] and not p['publicationAuthorized']
assert {r['project']:r['passed'] for r in results['projects']}=={'Bosak.XmlDb.Xdm.Tests':47,'Bosak.XmlDb.Storage.Tests':139}
packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')
for row in results['projects']:
    folder=Path(row['execution']['path'])
    for name,h in row['execution']['files'].items():assert sha(folder/name)==h,(row['project'],name)
    trx=P/row['trx'];assert sha(trx)==row['trxSha256']
    tree=ET.parse(trx);counts=tree.find('.//{*}Counters').attrib
    assert int(counts['passed'])==row['passed'] and counts['failed']==counts['notExecuted']=='0'
    names=sorted(r.attrib['testName'] for r in tree.findall('.//{*}UnitTestResult'));assert names==row['caseNames']
    historic=owner/row['historicalTrx']['path'];assert sha(historic)==row['historicalTrx']['sha256']
    assert sorted(r.attrib['testName'] for r in ET.parse(historic).findall('.//{*}UnitTestResult'))==names
    seen={}
    for file in (E/'bindings'/row['execution']['slug']).glob('*.json'):
        b=read(file);assert not b['captureErrors']
        for load in b['assemblies']:
            if load['name'] in packages['loadedEngines']:
                assert load['sha256']==packages['loadedEngines'][load['name']] and load['context']=='Default' and load['collectible']=='False'
                seen[load['name']]=load['sha256']
    assert seen==results['loadedPackageEngines'][row['project']]
    assets=read(source/'tests'/row['project']/'obj/project.assets.json')
    for name in assets['libraries']:
        identity,version=name.split('/',1)
        if identity in packages['loadedEngines']:assert version==p['previewVersion']
initial=next((E/'trx/Bosak.XmlDb.Xdm.Tests').glob('*.trx'));tree=ET.parse(initial)
bad=[r for r in tree.findall('.//{*}UnitTestResult') if r.attrib['outcome']=='Failed'];assert len(bad)==1
assert 'System.IO.IOException' in bad[0].find('.//{*}Message').text and 'denied' in bad[0].find('.//{*}Message').text
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
    for name in z.namelist():
        if name.endswith('.log.json'):
            r=json.loads(z.read(name));assert hashlib.sha256(z.read(name[:-5])).hexdigest()==r['logSha256']
    build=z.read('xmldb-source-build.log').decode('utf-8-sig');assert '0 Warning(s)' in build and '0 Error(s)' in build
for file in [P/'README.md',R/'docs/CHECKPOINT-1.81.1.md']:
    text=re.sub(r'`[^`]*`','',file.read_text(encoding='utf-8'))
    for link in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',text):
        name=link.strip('<>').split('#')[0]
        if name and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',name):assert (file.parent/name).exists()
if (P/'MANIFEST.json').exists():
    files={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
    assert read(P/'MANIFEST.json')==files
print('Verified isolated unchanged-source build and focused186 passing cases; full Storage and Integration remain incomplete, with approval restriction and original native failure preserved.')
