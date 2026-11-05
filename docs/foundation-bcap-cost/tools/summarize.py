"""Summarize raw observations without assigning acceptance thresholds."""
from pathlib import Path
import json,statistics,math
P=Path(__file__).resolve().parents[1];E=P/'evidence';runs=json.loads((E/'RUNS.json').read_text())['runs']
if (P/'MANIFEST.json').exists():raise RuntimeError('Sealed cohort; use verify.py without rewriting the summary')
def stats(values):
 values=sorted(values);return {'samples':len(values),'min':min(values),'median':statistics.median(values),'p95NearestRank':values[math.ceil(.95*len(values))-1],'max':max(values)}
costs=[]
for profile in ['source-free','shared','distinct-matched','distinct']:
 rows=[json.loads((E/r['receipt']).read_text())['data'] for r in runs if r['profile']==profile]
 timings=[t for row in rows for t in row['warmMilliseconds']]
 costs.append({'profile':profile,'members':rows[0]['members'],'bytesPerCapture':rows[0]['bytesPerCapture'],'freshProcesses':3,'firstCompileMilliseconds':stats([r['compile']['milliseconds'] for r in rows]),'firstCaptureMilliseconds':stats([r['firstCapture']['milliseconds'] for r in rows]),'firstCompileManagedBytes':stats([r['compile']['managedAllocatedBytes'] for r in rows]),'firstCaptureManagedBytes':stats([r['firstCapture']['managedAllocatedBytes'] for r in rows]),'warmMilliseconds':stats(timings),'warmManagedBytesPerCapture':stats([r['warmManagedBytesPerCapture'] for r in rows])})
cancels=[];allRows=[row for r in runs if r['profile']=='cancellation' for row in json.loads((E/r['receipt']).read_text())['data']['rows']]
for channel in ['capture','operation','inherited']:
 rows=[r for r in allRows if r['channel']==channel];assert len(rows)==36 and all(r['releaseBeforeOutcome'] and r['completedSecondaryBytes']==5 for r in rows)
 cancels.append({'channel':channel,'requestToRawOutcomeMilliseconds':stats([r['requestToRawOutcomeMilliseconds'] for r in rows]),'requestToLastReleaseMilliseconds':stats([r['requestToLastReleaseMilliseconds'] for r in rows])})
summary={'date':'2026-11-02','costs':costs,'cancellation':cancels,'coldMeaning':'First compile/first capture in each fresh host, excluding process launch and host workload construction; does not separate JIT from engine work','warmMeaning':'20 successful warmups then 80 captures in each of three independent processes per profile','allocationMeaning':'Precise process-wide managed allocation, includes transform workers and validation; not retained/native/stack/peak memory','cancellationMeaning':'Request while second serialization is paused; includes unpause, unwind, two synthetic release callbacks and caller scheduling','acceptanceBudgets':None,'engineChanges':False,'coreC10BoundedMeasurements':'Recorded; native workload and worst-case checkpoint latency acceptance remain separate'}
(E/'SUMMARY.json').write_text(json.dumps(summary,indent=2)+'\n')
print(json.dumps({'warm':[{ 'profile':r['profile'],'millisecondsMedian':r['warmMilliseconds']['median'],'bytesPerCaptureMedian':r['warmManagedBytesPerCapture']['median']} for r in costs],'cancelOutcomeMaxMs':{r['channel']:r['requestToRawOutcomeMilliseconds']['max'] for r in cancels}},indent=2))
