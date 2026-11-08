"""Verify a finite 1.81.1 observational cohort independently of owner acceptance."""
from pathlib import Path
import hashlib,json,re,statistics,zipfile
from compare import validate,METHOD_KEYS

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
c=read(P/'COHORT.json');p=read(P/'PROTOCOL.json');m=read(E/'MEASUREMENTS.json');s=read(E/'SUMMARY.json')
assert c['recordedBeforeExecution'] and c['candidateCommit']=='c0b926de34824ceb2010d5f814a4450b28b31dcc'
assert c['sourceVersion']==p['sourceVersion']==m['sourceVersion']==s['sourceVersion']=='1.81.1'
pins_path=R/'docs/foundation-captured-identity-production/evidence/SOURCE-AND-BINARY-PINS.json'
assert sha(pins_path)==c['qualificationPinsSha256']
pins=read(pins_path)
old=R/'docs/foundation-owner-workload-protocol'
assert sha(old/'PROTOCOL.json')==c['priorProtocolSha256'] and sha(old/'workload/Program.cs')==c['priorProgramSha256']
previous=read(old/'PROTOCOL.json')
for name in ['compare.py','test_compare.py']:assert sha(P/'tools'/name)==sha(old/'tools'/name),name
assert {k:v for k,v in p.items() if k!='sourceVersion'}=={k:v for k,v in previous.items() if k!='sourceVersion'}
assert all(p[k]==previous[k] for k in METHOD_KEYS)
assert (P/'workload/Program.cs').read_text().split('using System.Diagnostics;',1)[1].replace('sourceVersion = "1.81.1"','sourceVersion = "1.81.0"')==(old/'workload/Program.cs').read_text().split('using System.Diagnostics;',1)[1]
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
        if row['name'] in c['references']:assert 'Version=1.81.1.0' in row['identity']
        seen.add(row['name'])
assert {'Bosak.XPath.Core','Bosak.XPath.Api','Bosak.XPath.Runtime','Bosak.Schema','Bosak.Schema.ExperimentalProvider','Bosak.Schema.OwnerWorkload.Protocol'}<=seen
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip') as archive:
    for name,code in [('build',0),('measure',0),('method-controls',0),('baseline-refusal',2)]:
        log=f'workload-1811-{name}.log';receipt=json.loads(archive.read(log+'.json'))
        assert receipt['exitCode']==code and receipt['failure'] is None
        assert hashlib.sha256(archive.read(log)).hexdigest()==receipt['logSha256']
    assert '0 Warning(s)' in archive.read('workload-1811-build.log').decode('utf-8-sig')
    controls=archive.read('workload-1811-method-controls.log').decode('utf-8-sig');assert 'Ran 14 tests' in controls and 'OK' in controls
    assert 'candidate run cannot masquerade' in archive.read('workload-1811-baseline-refusal.log').decode('utf-8-sig')
for file in [P/'README.md',R/'docs/CHECKPOINT-1.81.1.md']:
    source=re.sub(r'`[^`]*`','',file.read_text(encoding='utf-8'))
    for link in re.findall(r'\[[^\[\]\n]*\]\(([^)\n]+)\)',source):
        target=link.strip('<>').split('#')[0]
        if target and not re.match(r'[A-Za-z][A-Za-z0-9+.-]*:',target):assert (file.parent/target).exists(),(file,target)
if (P/'MANIFEST.json').exists():
    files={f.relative_to(P).as_posix():sha(f) for f in P.rglob('*') if f.is_file() and f.name!='MANIFEST.json' and not {'bin','obj','__pycache__'}.intersection(f.parts)}
    assert read(P/'MANIFEST.json')==files
print('Verified PV-Q06: exact 1.81.1 references/layout/loaded bindings, unchanged timed method, 6 sessions/30 cycles/135 reads, 14 controls and baseline refusal; no owner acceptance.')
