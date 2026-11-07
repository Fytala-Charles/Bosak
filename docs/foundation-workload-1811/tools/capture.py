"""Archive completed cohort evidence and descriptive summaries without acceptance."""
from pathlib import Path
import hashlib,json,statistics,zipfile
from compare import validate,METHOD_KEYS

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
read=lambda p:json.loads(p.read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=read(P/'PROTOCOL.json');m=read(E/'MEASUREMENTS.json')
validate(p,m,sha(P/'PROTOCOL.json'))
old=read(R/'docs/foundation-owner-workload-protocol/PROTOCOL.json')
assert all(p[k]==old[k] for k in METHOD_KEYS)
summary=dict(sourceVersion='1.81.1',role='candidate',ownerAccepted=False,budgetsAccepted=False,
    selectedBaseline=None,comparisonRatio=None,methodFieldsUnchanged=True,
    measuredCycles=sum(r['iterations'] for r in m['rows']),
    measuredDecimalReads=sum(r['iterations']*r['children'] for r in m['rows']),workloads=[])
for workload in p['workloads']:
    rows=[r for r in m['rows'] if r['workload']==workload['id']]
    medians=[statistics.median(r['latencySamplesMs']) for r in rows]
    summary['workloads'].append(dict(id=workload['id'],repetitionMediansMs=medians,
        medianOfMediansMs=statistics.median(medians),
        allocatedBytesPerCycle=[r['allocatedBytesPerCycle'] for r in rows],
        outputRootsAliveAfterForcedGc=[r['outputRootsAliveAfterForcedGc'] for r in rows]))
(E/'SUMMARY.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
layout=read(E/'EXECUTION-LAYOUT.json');output=Path(layout['path'])
with zipfile.ZipFile(E/'EXECUTION-LAYOUT.zip','x',zipfile.ZIP_DEFLATED) as archive:
    for name,h in layout['files'].items():
        assert sha(output/name)==h,name
        archive.write(output/name,name)
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','x',zipfile.ZIP_DEFLATED) as archive:
    for name,expected in [('build',0),('measure',0),('method-controls',0),('baseline-refusal',2)]:
        log=R/f'docs/foundation-r2-closure/evidence/execution/workload-1811-{name}.log'
        receipt=log.with_suffix('.log.json');row=read(receipt)
        assert row['exitCode']==expected and row['failure'] is None and row['logSha256']==sha(log)
        archive.write(log,log.name);archive.write(receipt,receipt.name)
sources={f.relative_to(P).as_posix():sha(f) for folder in ['workload','fixtures']
         for f in (P/folder).rglob('*') if f.is_file() and not {'bin','obj'}.intersection(f.parts)}
sources['PROTOCOL.json']=sha(P/'PROTOCOL.json')
(E/'SOURCE-PINS.json').write_text(json.dumps(sources,indent=2)+'\n',encoding='utf-8')
print('Archived layout, scoped receipts, source pins and six descriptive repetition summaries.')
