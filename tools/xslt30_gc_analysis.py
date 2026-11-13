"""Keep GC collections, suspension windows and sampled allocation evidence separate."""
import argparse
import collections
import json
import math
from pathlib import Path


def union(intervals):
    result=[]
    for start,end in sorted(intervals):
        if not math.isfinite(start) or not math.isfinite(end) or end<start:raise ValueError('Invalid interval')
        if result and start<=result[-1][1]:result[-1][1]=max(result[-1][1],end)
        else:result.append([start,end])
    return result


def duration(intervals):return sum(end-start for start,end in union(intervals))


def analyze(events):
    collections_pending={};suspensions_pending={};finalizers_pending={}
    collections_done=[];suspensions=[];finalizers=[];unmatched=[];allocation=[];names=collections.Counter();last=-math.inf
    for event in events:
        at=event['milliseconds'];name=event['name'];payload=event['payload']
        if not math.isfinite(at) or at<last:raise ValueError('Event time reversal/nonfinite timestamp')
        last=at;names[name]+=1;scope=(event['processId'],payload.get('ClrInstanceID'))
        if name=='GC/Start':
            key=scope+(payload['Count'],)
            if key in collections_pending:raise ValueError('Duplicate GC start')
            collections_pending[key]=event
        elif name=='GC/Stop':
            key=scope+(payload['Count'],);start=collections_pending.pop(key,None)
            if start is None:unmatched.append(dict(kind='GCStopWithoutStart',event=event));continue
            collections_done.append(dict(start=start['milliseconds'],end=at,count=payload['Count'],scope=list(scope),
                generation=start['payload']['Depth'],type=start['payload']['Type'],reason=start['payload']['Reason']))
        elif name=='GC/SuspendEEStart':
            request=scope+(event['threadId'],)
            if request in suspensions_pending:raise ValueError('Overlapping suspension on one requesting thread')
            suspensions_pending[request]=dict(start=at,reason=payload.get('Reason','Unknown'),scope=list(request))
        elif name in ['GC/SuspendEEStop','GC/RestartEEStart']:
            current=suspensions_pending.get(scope+(event['threadId'],))
            if current is None:unmatched.append(dict(kind=name+'WithoutStart',event=event));continue
            field='fullySuspended' if name=='GC/SuspendEEStop' else 'restartStart'
            if field in current:raise ValueError('Duplicate suspension phase')
            current[field]=at
        elif name=='GC/RestartEEStop':
            current=suspensions_pending.pop(scope+(event['threadId'],),None)
            if current is None:unmatched.append(dict(kind='RestartWithoutSuspend',event=event));continue
            current['end']=at
            if 'fullySuspended' in current and 'restartStart' in current and not current['start']<=current['fullySuspended']<=current['restartStart']<=at:
                raise ValueError('Suspension phase order reversal')
            suspensions.append(current)
        elif name=='GC/FinalizersStart':
            if scope in finalizers_pending:raise ValueError('Duplicate finalizer start')
            finalizers_pending[scope]=at
        elif name=='GC/FinalizersStop':
            start=finalizers_pending.pop(scope,None)
            if start is None:unmatched.append(dict(kind='FinalizersStopWithoutStart',event=event));continue
            count=int(payload['Count'])
            if count<0:raise ValueError('Negative finalizer count')
            finalizers.append(dict(start=start,end=at,count=count,scope=list(scope)))
        elif 'AllocationTick' in name:
            amount=int(payload.get('AllocationAmount64',payload.get('AllocationAmount','0')))
            if amount<0:raise ValueError('Negative allocation sample')
            allocation.append(dict(type=payload.get('TypeName','Unknown'),intervalBytes=amount,
                engineContext=next((f for f in event.get('engineFrames',[]) if f.startswith('Bosak.')),'Unresolved'),milliseconds=at))
    unmatched.extend(dict(kind='GCStartWithoutStop',event=e) for e in collections_pending.values())
    unmatched.extend(dict(kind='SuspendWithoutRestart',event=e) for e in suspensions_pending.values())
    unmatched.extend(dict(kind='FinalizersStartWithoutStop',scope=list(k),start=v) for k,v in finalizers_pending.items())
    gc_pauses=[e for e in suspensions if e['reason']=='SuspendForGC'];prep=[e for e in suspensions if e['reason']=='SuspendForGCPrep']
    gc_windows=union([(e['start'],e['end']) for e in gc_pauses+prep])
    stopped=[(e['fullySuspended'],e['restartStart']) for e in gc_pauses+prep if 'fullySuspended' in e and 'restartStart' in e]
    return dict(collections=len(collections_done),collectionGenerations=dict(collections.Counter(e['generation'] for e in collections_done)),
        collectionTypes=dict(collections.Counter(e['type'] for e in collections_done)),
        collectionElapsedSumMs=sum(e['end']-e['start'] for e in collections_done),collectionElapsedUnionMs=duration([(e['start'],e['end']) for e in collections_done]),
        suspensionReasons=dict(collections.Counter(e['reason'] for e in suspensions)),gcSuspensionWindowUnionMs=duration(gc_windows),
        gcFullySuspendedPhaseUnionMs=duration(stopped),gcSuspensionWindows=gc_windows,
        otherSuspensionWindowUnionMs=duration([(e['start'],e['end']) for e in suspensions if e['reason'] not in ['SuspendForGC','SuspendForGCPrep']]),
        finalizerWindows=len(finalizers),finalizerElapsedUnionMs=duration([(e['start'],e['end']) for e in finalizers]),
        finalizerReportedObjects=sum(e['count'] for e in finalizers),unmatched=unmatched,eventCounts=dict(names),
        allocationTicks=len(allocation),reportedAllocationIntervalBytes=sum(e['intervalBytes'] for e in allocation),
        thresholdCrossingTypes=collections.Counter(e['type'] for e in allocation).most_common(30),
        sampledAllocationEngineContexts=collections.Counter(e['engineContext'] for e in allocation).most_common(30),
        allocationStackCoverage=sum(e['engineContext']!='Unresolved' for e in allocation),
        interpretation='Collection elapsed may overlap background execution; GC suspension windows include rendezvous/resume. Other suspensions are separate. Allocation intervals are approximate; threshold-crossing TypeName does not own all interval bytes. Finalizer event totals do not identify object types or CPU time.')


def correlate(stacks, windows):
    frames=stacks['shared']['frames'];windows=union(windows);totals=collections.Counter();overlaps=collections.Counter()
    for profile in stacks['profiles']:
        if profile['type']!='evented' or profile['unit']!='milliseconds':raise ValueError('Expected original evented millisecond stacks')
        stack=[];previous=profile['startValue']
        for event in profile['events']:
            at=event['at'];names=[frames[i]['name'] for i in stack]
            if at<previous:raise ValueError('Stack time reversal')
            if names and names[-1]=='CPU_TIME' and any(n.startswith(('Bosak.XPath.','Bosak.Xslt!','Bosak.XQuery!')) for n in names):
                for label,token in [('gcPoll','PollGCWorker'),('monitor','Monitor.Enter_Slowpath')]:
                    if any(token in n for n in names):
                        totals[label]+=at-previous
                        overlaps[label]+=sum(max(0,min(at,b)-max(previous,a)) for a,b in windows)
            previous=at
            if event['type']=='O':stack.append(event['frame'])
            elif event['type']=='C':
                if not stack or stack.pop()!=event['frame']:raise ValueError('Unbalanced stack')
            else:raise ValueError('Unknown stack event')
        if stack:raise ValueError('Unclosed stack')
    return dict(sampledWeightsMs=dict(totals),overlapWithGcSuspensionMs=dict(overlaps),
        interpretation='Temporal overlap in one TraceLog timebase, not CPU attribution, exclusive pause responsibility or causation; overlapping threads can sum above wall time.')


if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--events',type=Path,required=True);p.add_argument('--output',type=Path,required=True);p.add_argument('--stacks',type=Path)
    args=p.parse_args()
    if args.output.exists():p.error('Use a fresh analysis output; preserve original evidence')
    report=analyze([json.loads(line) for line in args.events.read_text().splitlines()])
    if args.stacks:report['stackCorrelation']=correlate(json.loads(args.stacks.read_text()),report['gcSuspensionWindows'])
    args.output.write_bytes((json.dumps(report,indent=2)+'\n').encode())
