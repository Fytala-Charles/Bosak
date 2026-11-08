"""Verify exact package-overlay qualification without granting owner acceptance."""
from pathlib import Path
import hashlib,json,re,subprocess,xml.etree.ElementTree as ET,zipfile
from compare_api import compare

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=read(P/'PLAN.json');results=read(E/'RESULTS.json')
assert p['previewVersion']==results['previewVersion']=='1.81.1-preview.c0b926de'
assert p['candidateCommit']==results['candidateCommit']=='c0b926de34824ceb2010d5f814a4450b28b31dcc'
for obj in [p,results]:assert not any(obj[k] for k in ['ownerAccepted','apiFrozen','publicationAuthorized'])
assert not results['budgetsAccepted']
prior=R/'docs/foundation-captured-identity-production/evidence'
assert sha(prior/'EXECUTION-LAYOUTS.json')==p['baselineLayoutsSha256']
assert sha(R/'docs/foundation-preview-packages/MANIFEST.json')==p['packageManifestSha256']
base=read(prior/'EXECUTION-LAYOUTS.json');packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')
for key,layout in p['layouts'].items():
    root=Path(layout['path']);assert Path(layout['binary']).is_relative_to(root)
    for name,h in layout['files'].items():assert sha(root/name)==h,(key,name)
    assert set(base[key]['files'])-set(layout['files'])==set(layout['omittedOldEngineSymbols'])
    assert set(layout['files'])<=set(base[key]['files'])
    for name,h in layout['files'].items():
        if name in {n+'.'+ext for n in layout['replacedEngines'] for ext in ['dll','xml']}:
            package=next(r for r in packages['packages'] if r['id']==Path(name).stem)
            assert h==package['verifiedContents']['lib/net10.0/'+name]
        else:assert h==base[key]['files'][name],(key,name)
    for name,h in layout['replacedEngines'].items():assert h==packages['loadedEngines'][name]
assert Path(p['fixtureRepair']['corpusPath']).resolve()==(R/'tests/qt3tests').resolve()
pins=read(prior/'SOURCE-AND-BINARY-PINS.json')
for name,row in pins['corpora'].items():
    root=R/'tests'/name
    assert subprocess.check_output(['git','--no-optional-locks','rev-parse','HEAD'],cwd=root,text=True).strip()==row['revision']
    assert subprocess.check_output(['git','--no-optional-locks','status','--porcelain'],cwd=root,text=True).strip()==row['status']==''
old_units=read(prior/'UNIT-RESULTS.json')['projects'];old_extra=read(prior/'EXTRA-RESULTS.json')['lanes']
assert len(results['units'])==10 and sum(r['passed'] for r in results['units'])==results['unitPassed']==26342
assert {r['key']:r['passed'] for r in results['extras']}=={'languageServer':84,'native451':451,'nativeAdapter':59,'consumer':10}
with zipfile.ZipFile(E/'TRX-RESULTS.zip') as archive:
    for row in results['units']+results['extras']:
        trx=P/row['trx'];assert sha(trx)==row['trxSha256']==hashlib.sha256(archive.read(row['trx'])).hexdigest()
        tree=ET.parse(trx);c=tree.find('.//{*}Counters').attrib
        assert int(c['passed'])==row['passed'] and c['failed']==c['notExecuted']=='0'
        names=sorted(x.attrib['testName'] for x in tree.findall('.//{*}UnitTestResult'));assert names==row['caseNames']
        key=row['key'];baseline=next(r for r in old_units if key.endswith('/'+r['project'])) if key.startswith('units/') else old_extra[{'languageServer':'languageServer','native451':'nativeSchema','nativeAdapter':'nativeAdapter','consumer':'publicConsumer'}[key]]
        assert names==sorted(baseline['caseNames'])
    initial=next((E/'trx/units-bosak.xpath.runtime.tests').glob('*.trx'));tree=ET.parse(initial)
    failures=[r for r in tree.findall('.//{*}UnitTestResult') if r.attrib['outcome']=='Failed']
    assert len(failures)==results['initialRuntimeFixtureFailuresRetained']==51
    assert all('System.IO.DirectoryNotFoundException' in r.find('.//{*}Message').text for r in failures)
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as archive:
    for name in archive.namelist():
        if not name.endswith('.log.json'):continue
        row=json.loads(archive.read(name));expected=1 if name in ['preview-qualification-api-inventory.log.json','preview-qualification-units-bosak.xpath.runtime.tests.log.json'] else 0
        assert row['exitCode']==expected and row['failure'] is None
        assert hashlib.sha256(archive.read(name[:-5])).hexdigest()==row['logSha256']
    for key,expected in [('qt3',[31397,0,424]),('qt4',[8941,0,1918])]:
        row=results['gates'][key];text=archive.read('preview-qualification-'+key+'.log').decode('utf-8-sig')
        assert [int(re.search(r'^'+label+r':\s+(\d+)',text,re.M).group(1)) for label in ['Passed','Failed','Skipped']]==expected
        assert (E/(key+'-skips.txt')).read_bytes()==(prior/(key+'-skips.txt')).read_bytes()
        assert sha(E/(key+'-skips.txt'))==row['skipSha256'] and row['exactSkipMapEqual']
    assert 'Experimental-level (XPath40Experimental) passes: 11' in text and results['qt4ExperimentalPassed']==11
assert compare()==results['api']==read(E/'API-COMPARISON.json')
bindings=read(E/'BINDING-SUMMARY.json');assert set(bindings)==set(p['layouts'])
for key,group in bindings.items():
    seen={}
    for name in group['reports']:
        report=read(P/name);assert not report['captureErrors']
        for row in report['assemblies']:
            if row['name'] in packages['loadedEngines']:
                assert row['sha256']==packages['loadedEngines'][row['name']] and row['context']=='Default' and row['collectible']=='False'
                assert p['previewVersion'] in row['informationalVersion']
                seen[row['name']]=row['sha256']
    assert seen==group['loadedEngines'] and set(seen)<=set(p['layouts'][key]['replacedEngines'])
api_root=Path(p['work'])/'api-root'
assert read(E/'API-LAYOUT.json')=={f.relative_to(api_root).as_posix():sha(f) for f in api_root.rglob('*') if f.is_file()}
for file in [P/'README.md',R/'docs/CHECKPOINT-1.81.1.md']:
    text=re.sub(r'`[^`]*`','',file.read_text(encoding='utf-8'))
    for link in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',text):
        name=link.strip('<>').split('#')[0]
        if name and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',name):assert (file.parent/name).exists(),(file,name)
if (P/'MANIFEST.json').exists():
    files={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and '__pycache__' not in f.parts}
    assert read(P/'MANIFEST.json')==files
print('Verified PV-Q09 exact package cohort: solution26342/0, language-server84/native451/adapter59/consumer10, frozen gates/skip maps, API318/6465 and loaded bindings. Setup failures preserved; owner/freeze/publication gates remain open.')
