"""Verify a finite 1.81.1 observational cohort independently of owner acceptance."""
from pathlib import Path
import hashlib,json,re,statistics,zipfile,xml.etree.ElementTree as ET
from compare import validate,METHOD_KEYS
from owner_preflight import check_current

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
c=read(P/'COHORT.json');p=read(P/'PROTOCOL.json');m=read(E/'MEASUREMENTS.json');s=read(E/'SUMMARY.json')
assert c['recordedBeforeExecution'] and c['candidateCommit']=='c0b926de34824ceb2010d5f814a4450b28b31dcc'
assert c['sourceVersion']==p['sourceVersion']==m['sourceVersion']==s['sourceVersion']=='1.81.1'
pins_path=R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json'
assert sha(pins_path)==c['qualificationPinsSha256']
pins=read(P/'CANDIDATE-PINS.json')
packages=read(R/'docs/foundation-preview-packages/evidence/PACKAGES.json')
assert sha(R/'docs/foundation-preview-packages/MANIFEST.json')==c['packageManifestSha256']
assert sha(R/'docs/foundation-preview-qualification/MANIFEST.json')==c['regressionManifestSha256']
assert c['previewVersion']==p['previewVersion']==m['previewVersion']==s['previewVersion']==packages['previewVersion']=='1.81.1-preview.c0b926de'
assert {name:row['sha256'] for name,row in c['references'].items()}==packages['loadedEngines']
original_pins=read(pins_path)
for name,row in pins['engines'].items():
    assert row['sha256']==packages['loadedEngines'].get(name,original_pins['engines'][name]['sha256'])
assets=read(P/'workload/obj/project.assets.json')
assert set(assets['libraries'])=={name+'/'+c['previewVersion'] for name in c['references']}
assert {Path(n).resolve() for n in assets['packageFolders']}=={Path(c['cache']).resolve()}
assert {Path(n).resolve() for n in assets['project']['restore']['sources']}=={Path(c['feed']).resolve()}
project=ET.parse(P/'workload/Workload.csproj');assert not project.findall('.//ProjectReference')
package_refs=project.findall('.//PackageReference')
assert len(package_refs)==1 and package_refs[0].attrib==dict(Include='Bosak.Xslt',Version='['+c['previewVersion']+']')
assert {r.attrib['Include'] for r in project.findall('.//Reference')}=={'Bosak.Schema','Bosak.Schema.ExperimentalProvider'}
for row in packages['packages']:
    file=Path(c['cache'])/row['id'].lower()/c['previewVersion']/(row['id'].lower()+'.'+c['previewVersion']+'.nupkg')
    assert sha(file)==row['sha256']
old=R/'docs/foundation-owner-workload-protocol'
assert sha(old/'PROTOCOL.json')==c['priorProtocolSha256'] and sha(old/'workload/Program.cs')==c['priorProgramSha256']
previous=read(old/'PROTOCOL.json')
for name in ['compare.py','test_compare.py']:assert sha(P/'tools'/name)==sha(old/'tools'/name),name
assert {k:v for k,v in p.items() if k not in ['sourceVersion','previewVersion']}=={k:v for k,v in previous.items() if k not in ['sourceVersion','previewVersion']}
assert all(p[k]==previous[k] for k in METHOD_KEYS)
assert (P/'workload/Program.cs').read_text().split('using System.Diagnostics;',1)[1].replace(', previewVersion = "1.81.1-preview.c0b926de"','').replace('sourceVersion = "1.81.1"','sourceVersion = "1.81.0"')==(old/'workload/Program.cs').read_text().split('using System.Diagnostics;',1)[1]
for name,h in read(E/'SOURCE-PINS.json').items():assert sha(P/name)==h,name
assert sha(P/p['schema'])==p['schemaSha256']
for w in p['workloads']:assert sha(P/w['instance'])==w['instanceSha256']
validate(p,m,sha(P/'PROTOCOL.json'))
assert m['role']=='candidate' and not m['baselineRun'] and m['comparisonRatio'] is None
assert p['ownerBudget'] is p['comparableBaseline'] is None and not p['baselineSelected']
assert len(m['rows'])==6 and s['measuredCycles']==sum(r['iterations'] for r in m['rows'])==30
assert s['measuredDecimalReads']==sum(r['iterations']*r['children'] for r in m['rows'])==135
assert s['selectedBaseline'] is s['comparisonRatio'] is None
for obj in [c,p,m,s]:assert not obj['ownerAccepted']
assert all(not obj['budgetsAccepted'] for obj in [c,m,s])
for workload,summary in zip(p['workloads'],s['workloads'],strict=True):
    rows=[r for r in m['rows'] if r['workload']==workload['id']]
    medians=[statistics.median(r['latencySamplesMs']) for r in rows]
    assert summary['id']==workload['id'] and summary['repetitionMediansMs']==medians
    assert summary['medianOfMediansMs']==statistics.median(medians)
    assert summary['allocatedBytesPerCycle']==[r['allocatedBytesPerCycle'] for r in rows]
layout=read(E/'EXECUTION-LAYOUT.json');output=Path(layout['path'])
assert layout['files']=={f.relative_to(output).as_posix():sha(f) for f in output.rglob('*') if f.is_file()}
with zipfile.ZipFile(E/'EXECUTION-LAYOUT.zip') as archive:
    assert set(archive.namelist())==set(layout['files'])
    for name,h in layout['files'].items():assert hashlib.sha256(archive.read(name)).hexdigest()==h,name
expected={name:row['sha256'] for name,row in c['references'].items()}
assert len(expected)==9
for name,row in c['references'].items():
    assert row=={k:pins['engines'][name][k] for k in ['path','sha256']},name
    assert sha(Path(row['path']))==layout['files'][name+'.dll']==row['sha256']
for name,key in [('Bosak.Schema','nativeAssembly'),('Bosak.Schema.ExperimentalProvider','providerAssembly')]:
    row=c[key];assert sha(Path(row['path']))==layout['files'][name+'.dll']==row['sha256'];expected[name]=row['sha256']
assert sha(Path(c['hook']['path']))==c['hook']['sha256']
expected['Bosak.Schema.OwnerWorkload.Protocol']=layout['files']['Bosak.Schema.OwnerWorkload.Protocol.dll']
bindings=list((E/'bindings').glob('*.json'));assert len(bindings)==1
b=read(bindings[0]);assert not b['captureErrors'];seen=set()
for row in b['assemblies']:
    assert row['name'] in expected and 'fileHashError' not in row,row['name']
    if row['name'] in expected:
        assert row['sha256']==expected[row['name']] and row['context']=='Default' and str(row['collectible']).lower()=='false'
        assert Path(row['path']).resolve()==(output/(row['name']+'.dll')).resolve()
        if row['name'] in c['references']:assert 'Version=1.81.1.0' in row['identity'] and c['previewVersion'] in row['informationalVersion']
        seen.add(row['name'])
assert {'Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime','Bosak.Schema','Bosak.Schema.ExperimentalProvider','Bosak.Schema.OwnerWorkload.Protocol'}<=seen
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as archive:
    for name,code in [('restore',0),('build',0),('measure',0),('method-controls',0),('baseline-refusal',2)]:
        log=f'package-workload-{name}.log';receipt=json.loads(archive.read(log+'.json'))
        assert receipt['exitCode']==code and receipt['failure'] is None
        assert hashlib.sha256(archive.read(log)).hexdigest()==receipt['logSha256']
    assert '0 Warning(s)' in archive.read('package-workload-build.log').decode('utf-8-sig')
    controls=archive.read('package-workload-method-controls.log').decode('utf-8-sig');assert 'Ran 14 tests' in controls and 'OK' in controls
    assert 'candidate run cannot masquerade' in archive.read('package-workload-baseline-refusal.log').decode('utf-8-sig')
context=read(P/'OWNER-CONTEXT.json');review=read(P/'OWNER-REVIEW.json');refusals=read(E/'OWNER-REFUSALS.json')
assert context['candidatePinsSha256']==sha(P/'CANDIDATE-PINS.json') and context['requirementsWaived']==0
for name,h in review['evidence'].items():assert sha(R/name)==h,name
assert len(review['owners'])==4 and len(review['scenarios'])==9 and not review['messagesSent']
assert not any(review[k] for k in ['ownerAccepted','apiFrozen','publicationAuthorized']) and review['requirementsWaived']==0
for owner in review['owners']:
    role=owner['role'];template=read(P/'returns'/(role+'.template.json'))
    assert check_current(template)==refusals['currentUnfilled'][role] and not refusals['currentUnfilled'][role]['structureReady']
    assert check_current(read(R/'docs/foundation-provider-adoption/returns'/(role+'.template.json')))==refusals['priorProductionCohort'][role]
    assert owner['decision'] is owner['ownerIdentity'] is owner['decisionDate'] is None
    assert not any(owner[k] for k in ['workloadApproved','baselineApproved','budgetApproved','productionAdoptionAccepted'])
for row in review['scenarios']:
    assert row['ownerBudget'] is None and not any(row[k] for k in ['ownerWorkloadAccepted','baselineAccepted','candidateAccepted'])
for file in [P/'README.md',P/'OWNER-REVIEW.md',R/'docs/CHECKPOINT-1.81.1.md']:
    source=re.sub(r'`[^`]*`','',file.read_text(encoding='utf-8'))
    for link in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',source):
        target=link.strip('<>').split('#')[0]
        if target and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',target):assert (file.parent/target).exists(),(file,target)
if (P/'MANIFEST.json').exists():
    files={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and not {'bin','obj','__pycache__'}.intersection(f.parts)}
    assert read(P/'MANIFEST.json')==files
print('Verified PV-Q10: exact preview package workload; exact 1.81.1 references/layout/loaded bindings, unchanged timed method, 6 sessions/30 cycles/135 reads, 14 controls and baseline refusal; no owner acceptance.')
