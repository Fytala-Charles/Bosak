"""Read-only comparison of retained hosted attempts; no execution or gate-policy changes."""
import hashlib
import importlib.util
import io
import json
import re
import statistics
import zipfile


def digest(data):
    return hashlib.sha256(data).hexdigest()


def fingerprint(value):
    return digest(json.dumps(value, sort_keys=True, separators=(',', ':')).encode())


def payload(root, source):
    path = (root / source['source']).resolve()
    assert path.is_relative_to(root.resolve())
    data = path.read_bytes()
    if source['nested']:
        with zipfile.ZipFile(io.BytesIO(data)) as archive:
            data = archive.read(source['nested'])
    assert digest(data) == source['archiveSha256']
    assert source['artifactDigest'] == 'sha256:' + digest(data)
    return data


def comparison(left, right):
    if left['checkoutCommit'] == right['checkoutCommit'] and left['sourceTree'] != right['sourceTree']:
        raise ValueError('One checkout commit cannot identify two source trees')
    policy = ['corpusFingerprint', 'caseInventoryFingerprint', 'profile', 'timeoutSeconds', 'memoryLimitBytes']
    if any(left[key] != right[key] for key in policy):
        return 'not-comparable-policy-or-inputs'
    if left['checkoutCommit'] == right['checkoutCommit']:
        return ('same-checkout-and-payload' if left['applicationFingerprint'] == right['applicationFingerprint']
                else 'same-checkout-different-payload')
    if left['sourceTree'] == right['sourceTree']:
        return ('equivalent-tree-and-payload' if left['applicationFingerprint'] == right['applicationFingerprint']
                else 'equivalent-tree-different-payload')
    return 'different-source-cohorts'


def progress(text):
    samples = [dict(seconds=int(t), test=name.strip()) for t, name in
               re.findall(r'Elapsed (\d+)s; current test: ([^\r\n]+)', text)]
    assert samples and samples[0]['seconds'] == 0
    assert all(a['seconds'] < b['seconds'] for a, b in zip(samples, samples[1:]))
    milestones = {}
    for prefix in ['catalog-', 'sf-', 'si-', 'sx-', 'date-020']:
        milestones[prefix] = next((row for row in samples if row['test'].startswith(prefix)), None)
    return samples, milestones


def platform(text):
    result = {}
    group = ''
    for line in text.splitlines():
        line = line.split('\t')[-1].lstrip('\ufeff')
        line = re.sub(r'^\d{4}-\d\d-\d\dT\S+Z\s*', '', line)
        if line.startswith('##[group]'):
            group = line[len('##[group]'):]
        elif line.startswith('##[endgroup]'):
            group = ''
        elif group == 'Runner Image' and line.startswith('Image: '):
            result['image'] = line[7:]
        elif group == 'Runner Image' and line.startswith('Version: '):
            result['imageVersion'] = line[9:]
        elif group == 'Runner Image Provisioner' and line.startswith('Region: '):
            result['region'] = line[8:]
    assert set(result) == {'image', 'imageVersion', 'region'}
    return result


def analyze(root, sources, read):
    spec = importlib.util.spec_from_file_location('gate', root / 'tools/xslt30_gate.py')
    gate = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(gate)
    policy, baseline_plan, inputs, skips = gate.baseline()
    records = []
    for source in sources:
        with zipfile.ZipFile(io.BytesIO(payload(root, source))) as archive:
            get = lambda name: json.loads(archive.read(name))
            run, result, plan, hashes = [get(n) for n in ['RUN.json', 'GATE.json', 'PLAN.json', 'RUNTIME-HASHES.json']]
            assert get('CORPUS-BEFORE.json') == inputs == get('CORPUS-AFTER.json')
            assert result['sourceCorpusUnchanged'] and result['runtimeUnchanged']
            assert not any(result['corpusDifferences'].values())
            assert plan['sets'] == baseline_plan['sets'] and plan['totalCases'] == 14601
            assert plan['corpusCommit'] == run['corpusCommit'] == policy['corpusCommit']
            assert run['timeoutSeconds'] == plan['wallTimeoutSeconds'] == 1200
            assert run['memoryLimitBytes'] == plan['memoryLimitBytes'] == 8589934592
            assert run['processorProfile'] == 'Legacy' and run['catalogProfile'] == 'Xslt30' and not run['schemaAware']
            assert len(run['command']) == 5 and run['command'][1].endswith('Bosak.Xslt.Conformance.dll')
            assert run['command'][2:4] == ['--processor-profile', 'legacy'] and run['command'][-1].endswith('catalog.xml')
            for name in ['stdout.log', 'stderr.log']:
                assert digest(archive.read(name)) == run[name + 'Sha256']
            stdout = archive.read('stdout.log').decode('utf-8-sig')
            assert not re.findall(r'^\s*FAIL\s', stdout, re.M)
            if run['status'] == 'Completed':
                assert run['success'] and result['success']
                assert len(gate.reconcile(plan, run, stdout, skips, policy)) == 14601
                counts = policy['counts']
            else:
                assert run['status'] == 'TimedOut' and not run['success'] and not result['success']
                counts = None  # A timeout has no complete passing aggregate.
            workflow = json.loads(read('metadata/' + source['id'] + '-workflow.json'))
            assert workflow['id'] == int(source['run'])
            assert workflow['run_attempt'] == source['attempt'] and workflow['status'] == 'completed'
            assert workflow['conclusion'] == ('success' if counts else 'failure')
            checkout = json.loads(read('metadata/commit-' + run['sourceCommit'] + '.json'))
            head = json.loads(read('metadata/commit-' + workflow['head_sha'] + '.json'))
            assert checkout['sha'] == run['sourceCommit'] and head['sha'] == workflow['head_sha']
            assert checkout['tree']['sha'] == head['tree']['sha']
            log = read('logs/' + source['id'] + '.log').decode('utf-8-sig')
            samples, milestones = progress(log)
            engine = {n: value for n, value in hashes.items() if n.startswith('Bosak.') and n.endswith('.dll')
                      and n != 'Bosak.Xslt.Conformance.dll'}
            assert len(hashes) == 32 and len(engine) == 9
            last = next((name for name in archive.namelist() if name.endswith('last_test.txt')), None)
            records.append(dict(id=source['id'], runId=source['run'], attempt=source['attempt'],
                workflowEvent=workflow['event'], workflowHead=workflow['head_sha'], checkoutCommit=run['sourceCommit'],
                sourceTree=checkout['tree']['sha'], status=run['status'], elapsedSeconds=run['elapsedSeconds'],
                rightCensored=not bool(counts), completeCounts=counts,
                lastCase=archive.read(last).decode('utf-8-sig') if last else None,
                applicationFiles=len(hashes), engineAssemblies=len(engine), applicationFingerprint=fingerprint(hashes),
                engineFingerprint=fingerprint(engine), corpusFingerprint=fingerprint(inputs),
                caseInventoryFingerprint=fingerprint(plan['sets']), profile='Legacy/Xslt30/basic/non-schema-aware',
                timeoutSeconds=1200, memoryLimitBytes=8589934592, samples=samples,
                sampleMedianGapSeconds=statistics.median(b['seconds'] - a['seconds'] for a, b in zip(samples, samples[1:])),
                firstObservedMilestones=milestones, platform=platform(log)))
    by_id = {row['id']: row for row in records}
    pairs = []
    for left, right in [('df16-timeout', 'df16-success'), ('pr481-attempt1', 'pr481-attempt2'), ('pr482', 'main482-timeout')]:
        a, b = by_id[left], by_id[right]
        pairs.append(dict(left=left, right=right, comparison=comparison(a, b),
                          applicationFilesIdentical=a['applicationFingerprint'] == b['applicationFingerprint']))
    assert [p['comparison'] for p in pairs] == ['same-checkout-and-payload', 'same-checkout-and-payload', 'equivalent-tree-different-payload']
    assert sum(r['rightCensored'] for r in records) == 3
    return dict(date='2026-11-13',attempts=records,comparisons=pairs,newDiagnosticExecutions=0,policyChanged=False,
                completeRuns=4,timeouts=3,rootCauseEstablished=False,hostRuntimeBinariesCaptured=False,
                cpuGcIoTelemetryCaptured=False,perCaseDurationsCaptured=False,
                samplingLimit='First observed family/test at approximately 20-second intervals; not exact start or duration.',
                payloadLimit='32 application/configuration files only; not the dotnet host, shared runtime/JIT, OS or machine state.')
