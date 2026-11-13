"""Read retained telemetry without launching qualification or inferring a root cause."""
import collections, json, math


def summarize(read):
    receipt=json.loads(read('TELEMETRY.json'))
    samples=[json.loads(line) for line in read('samples.jsonl').decode('utf-8-sig').splitlines() if line]
    if not samples:
        return dict(status=receipt['status'],gateSuccess=receipt['gateSuccess'],telemetryCaptured=receipt['telemetryCaptured'],samples=0)
    child=json.loads(read('CHILD.json'))
    native=json.loads(read('NATIVE-MODULES.json'))
    run=json.loads(read('gate/RUN.json'))
    gate=json.loads(read('gate/GATE.json'))
    assert len(samples)==receipt['samples']
    assert all(row['creationFileTime']==child['creationFileTime'] for row in samples)
    for before,after in zip(samples,samples[1:]):
        assert after['observerElapsedSeconds']>before['observerElapsedSeconds']
        for key in ['kernelCpuSeconds','userCpuSeconds','pageFaults']:
            assert after[key]>=before[key]
        assert all(after['io'][key]>=before['io'][key] for key in before['io'])
    assert all(math.isfinite(row[key]) and row[key]>=0 for row in samples for key in ['kernelCpuSeconds','userCpuSeconds','collectionSeconds','workingSetBytes','privateCommitBytes'])
    assert receipt['applicationMatchesGate'] and receipt['sourceApplicationUnchanged']
    assert json.loads(read('APPLICATION-BEFORE.json'))==json.loads(read('gate/RUNTIME-HASHES.json'))
    assert all(native[key]['observedLoaded'] for key in ['dotnet.exe','coreclr.dll','clrjit.dll'])
    assert not native.get('system.private.corelib.dll',{}).get('observedLoaded',False)
    assert run['timeoutSeconds']==1200 and run['memoryLimitBytes']==8589934592
    qualified=bool(receipt['status']=='Completed' and receipt['gateControllerExitCode']==0 and run['success'] and gate['success'])
    assert receipt['gateSuccess']==qualified
    final=json.loads(read('FINAL-TIMES.json'))
    cpu=final['kernelCpuSeconds']+final['userCpuSeconds']
    gaps=[b['observerElapsedSeconds']-a['observerElapsedSeconds'] for a,b in zip(samples,samples[1:])]
    return dict(sourceCommit=run['sourceCommit'],status=run['status'],gateSuccess=qualified,telemetryCaptured=receipt['telemetryCaptured'],samples=len(samples),
        qualificationElapsedSeconds=run['elapsedSeconds'],finalProcessCpuSeconds=cpu,
        cpuSecondsPerWallSecond=cpu/run['elapsedSeconds'],affinityCpuCount=child['affinityCpuCount'],
        maximumObservedWorkingSetBytes=max(row['workingSetBytes'] for row in samples),
        maximumObservedPrivateCommitBytes=max(row['privateCommitBytes'] for row in samples),
        maximumReportedPeakWorkingSetBytes=max(row['reportedPeakWorkingSetBytes'] for row in samples),
        maximumReportedPeakCommitBytes=max(row['reportedPeakCommitBytes'] for row in samples),
        lastSampleIo=samples[-1]['io'],maximumSampleGapSeconds=max(gaps,default=None),
        maximumCounterCollectionSeconds=max(row['collectionSeconds'] for row in samples),
        sampleErrors=receipt['errors'],mostObservedMarkers=[dict(case=case,samples=count) for case,count in collections.Counter(row['lastRecordedCase'] for row in samples).most_common(8)],nativeHashes={key:value['sha256'] for key,value in native.items()},
        interpretation='CPU/wall is process core-equivalents, not percent host utilization or VM entitlement. Sample maxima and I/O counters are not allocation, GC or latency measures. Counter collection excludes module hashing and other observer overhead.')


def refusal_controls(read):
    original=[json.loads(line) for line in read('samples.jsonl').decode('utf-8-sig').splitlines() if line]
    controls={}
    def rejects(name,changes):
        try:
            summarize(lambda key:changes[key] if key in changes else read(key))
        except AssertionError:
            controls[name]=True
        else:
            raise AssertionError('Corrupted evidence accepted: '+name)
    rows=[dict(row) for row in original]
    rows[1]['userCpuSeconds']=-1
    rejects('decreasing CPU',{'samples.jsonl':('\n'.join(json.dumps(row) for row in rows)+'\n').encode()})
    rows=[dict(row) for row in original]
    rows[0]['creationFileTime']+=1
    rejects('wrong child identity',{'samples.jsonl':('\n'.join(json.dumps(row) for row in rows)+'\n').encode()})
    receipt=json.loads(read('TELEMETRY.json'));receipt['gateSuccess']=not receipt['gateSuccess']
    rejects('qualification flag mismatch',{'TELEMETRY.json':json.dumps(receipt).encode()})
    app=json.loads(read('APPLICATION-BEFORE.json'));app['changed']='not original'
    rejects('application hash drift',{'APPLICATION-BEFORE.json':json.dumps(app).encode()})
    native=json.loads(read('NATIVE-MODULES.json'));native['system.private.corelib.dll']['observedLoaded']=True
    rejects('sibling falsely marked loaded',{'NATIVE-MODULES.json':json.dumps(native).encode()})
    return controls
