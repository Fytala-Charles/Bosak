"""Check measurement-method compatibility; never grant binary qualification or owner acceptance."""
import argparse,hashlib,json,math,statistics
from pathlib import Path

METHOD_KEYS=['format','scenario','schemaSha256','expression','behaviorProfile','limits','measurementScope','repetitions','warmup','iterations','workloads']

def validate(protocol,metrics,protocol_digest):
    if metrics.get('protocolSha256')!=protocol_digest:raise ValueError('Protocol receipt hash mismatch')
    if metrics.get('format')!='bosak-owner-measurement-v1':raise ValueError('Unknown receipt format')
    if metrics.get('ownerAccepted') is not False or metrics.get('budgetsAccepted') is not False:raise ValueError('Owner acceptance is outside this diagnostic tool')
    expected={(w['id'],i):(w['children']) for w in protocol['workloads'] for i in range(protocol['repetitions'])}
    rows=metrics['rows'];seen=set()
    for row in rows:
        key=(row['workload'],row['repetition'])
        if key in seen or key not in expected:raise ValueError('Duplicate or unselected repetition')
        seen.add(key)
        if row['children']!=expected[key] or row['iterations']!=protocol['iterations'] or row['warmup']!=protocol['warmup']:raise ValueError('Work unit or warmup differs')
        samples=row['latencySamplesMs']
        if len(samples)!=row['iterations'] or any(not isinstance(x,(int,float)) or not math.isfinite(x) or x<=0 for x in samples):raise ValueError('Missing/nonfinite/nonpositive latency samples')
        if row['allocatedBytes']<0 or not math.isfinite(row['allocatedBytesPerCycle']) or row['allocatedBytesPerCycle']!=row['allocatedBytes']/row['iterations']:raise ValueError('Invalid allocation observation')
    if seen!=set(expected):raise ValueError('Missing independent repetitions')

def compare(left_protocol,left,left_digest,right_protocol,right,right_digest,mode='baseline'):
    validate(left_protocol,left,left_digest);validate(right_protocol,right,right_digest)
    for key in METHOD_KEYS:
        if left_protocol.get(key)!=right_protocol.get(key):raise ValueError('Method mismatch: '+key)
    for key in ['runtime','architecture','osDescription']:
        if not left.get(key) or left.get(key)!=right.get(key):raise ValueError('Environment mismatch: '+key)
    if mode=='baseline':
        if left.get('role')!='baseline' or right.get('role')!='candidate':raise ValueError('A candidate run cannot masquerade as a selected baseline')
    elif mode=='repeatability':
        if left.get('role')!='candidate' or right.get('role')!='candidate':raise ValueError('Repeatability requires two candidate receipts')
    else:raise ValueError('Unknown comparison mode')
    rows=[]
    for work in right_protocol['workloads']:
        def summary(receipt):
            selected=[r for r in receipt['rows'] if r['workload']==work['id']]
            medians=[statistics.median(r['latencySamplesMs']) for r in selected]
            return {'repetitionMediansMs':medians,'medianOfMediansMs':statistics.median(medians),'allocatedBytesPerCycle':[r['allocatedBytesPerCycle'] for r in selected]}
        rows.append({'workload':work['id'],'left':summary(left),'right':summary(right)})
    return {'status':'MethodComparableDiagnosticOnly','mode':mode,'ownerAccepted':False,'budgetsAccepted':False,'binaryQualificationPerformedByComparator':False,'ratiosAcceptedAsReleaseGate':False,'rows':rows,'scope':'Method checks only;cohort/load evidence and owner workload/budget acceptance remain separate. Three small repetitions do not establish statistical regression acceptance.'}

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    for name in ['left-protocol','left-metrics','right-protocol','right-metrics']:parser.add_argument('--'+name,required=True,type=Path)
    parser.add_argument('--mode',choices=['baseline','repeatability'],default='baseline');args=parser.parse_args()
    read=lambda p:json.loads(p.read_text(encoding='utf-8'))
    digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
    result=compare(read(args.left_protocol),read(args.left_metrics),digest(args.left_protocol),read(args.right_protocol),read(args.right_metrics),digest(args.right_protocol),args.mode)
    print(json.dumps(result,indent=2))

if __name__=='__main__':
    try:main()
    except (ValueError,KeyError,TypeError,OSError) as error:print('Refused: '+str(error));raise SystemExit(2)
