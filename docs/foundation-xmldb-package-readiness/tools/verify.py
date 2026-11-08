"""Verify limited XDM binary compatibility while preserving owner-source and adapter gaps."""
from pathlib import Path
import hashlib,json,re,subprocess,xml.etree.ElementTree as ET,zipfile

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=read(P/'PLAN.json');owner=Path(p['ownerRoot']);results=read(E/'RESULTS.json');recipe=read(P/'QUALIFICATION-RECIPE.json')
assert subprocess.check_output(['git','--no-optional-locks','-C',str(owner),'rev-parse','HEAD'],text=True).strip()==p['ownerHead']
assert subprocess.check_output(['git','--no-optional-locks','-C',str(owner),'status','--porcelain'],text=True).strip()==p['ownerStatus']==''
for name,h in p['sourceHashes'].items():assert sha(owner/name)==h,name
assert p['sourceLane']=='PreparedNotRun' and not p['productionAdapterImplemented'] and not p['formatsChanged']
assert not any(p[k] for k in ['ownerAccepted','apiFrozen','publicationAuthorized'])
assert recipe['status']=='PreparedNotExecuted' and recipe['requirementsWaived']==0 and len(recipe['steps'])==6
packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')
assert sha(R/'docs/foundation-preview-packages/MANIFEST.json')==p['packageManifestSha256']
assert packages['previewVersion']==p['previewVersion']=='1.81.1-preview.c0b926de'
baseline=p['layouts']['baseline'];preview=p['layouts']['preview']
assert baseline['files']['Bosak.XmlDb.Xdm.Tests.dll']==preview['files']['Bosak.XmlDb.Xdm.Tests.dll']
assert set(baseline['files'])==set(preview['files'])
for name,h in preview['files'].items():
    if name in {n+'.dll' for n in preview['replacedEngines']}:assert h==packages['loadedEngines'][Path(name).stem]
    else:assert h==baseline['files'][name],name
for lane,layout in p['layouts'].items():
    folder=Path(layout['path'])
    for name,h in layout['files'].items():assert sha(folder/name)==h,(lane,name)
    row=results['lanes'][lane];trx=P/row['trx'];assert sha(trx)==row['sha256']
    tree=ET.parse(trx);counts=tree.find('.//{*}Counters').attrib
    assert counts['passed']=='47' and counts['failed']==counts['notExecuted']=='0'
    assert sorted(x.attrib['testName'] for x in tree.findall('.//{*}UnitTestResult'))==row['caseNames']
    seen={};reports=list((E/'bindings'/lane).glob('*.json'));assert reports
    for file in reports:
        report=read(file);assert not report['captureErrors']
        for loaded in report['assemblies']:
            if loaded['name'] in packages['loadedEngines']:
                assert loaded['sha256']==layout['files'][loaded['name']+'.dll']
                assert loaded['context']=='Default' and loaded['collectible']=='False'
                if lane=='preview':assert loaded['sha256']==packages['loadedEngines'][loaded['name']] and p['previewVersion'] in loaded['informationalVersion']
                seen[loaded['name']]=loaded['sha256']
    assert {'Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime'}<=set(seen)
assert results['lanes']['baseline']['caseNames']==results['lanes']['preview']['caseNames']
assert not results['ownerAccepted'] and not results['sourceLaneRun'] and results['countsOverlapNotAdditive']
assert sha(Path(results['historicalTrx']['path']))==results['historicalTrx']['sha256']
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as z:
    for name in z.namelist():
        if name.endswith('.log.json'):
            row=json.loads(z.read(name));assert row['exitCode']==0 and row['failure'] is None
            assert hashlib.sha256(z.read(name[:-5])).hexdigest()==row['logSha256']
for file in [P/'README.md',R/'docs/CHECKPOINT-1.81.1.md']:
    text=re.sub(r'`[^`]*`','',file.read_text(encoding='utf-8'))
    for link in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',text):
        name=link.strip('<>').split('#')[0]
        if name and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',name):assert (file.parent/name).exists()
if (P/'MANIFEST.json').exists():
    files={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
    assert read(P/'MANIFEST.json')==files
print('Verified XmlDb XDM baseline47/preview47 exact-case existing-binary compatibility, package loads and unchanged owner source. Full source/native/adapter/owner qualification remains separate.')
