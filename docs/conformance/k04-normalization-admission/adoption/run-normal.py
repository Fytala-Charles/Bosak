"""Run complete isolated proposed gates and bounded affected normal-path checks."""
from pathlib import Path
import concurrent.futures
import json
import os
import subprocess
import time

E = Path(__file__).resolve().parent
R = E.parents[3]

def run(job):
    suite, scope = job
    project = 'Bosak.XPath.Conformance' if suite == 'qt3' else 'Bosak.XPath.Conformance40'
    command = ['dotnet', str(R / 'tests' / project / 'bin/Release/net10.0' / (project+'.dll')), 'tests/'+suite+'tests']
    if scope != 'gate':
        command += ['fn-normalize-unicode']
    if scope == 'original':
        command += ['cbcl-fn-normalize-unicode-006']
    name = suite+'-'+scope
    env = dict(os.environ)
    env['BOSAK_QT4_GATE_ONLY'] = '1' if scope == 'gate' else '0'
    env['BOSAK_'+suite.upper()+'_DUMP_SKIPS'] = str(E/(name+'-skips.txt'))
    started = time.monotonic()
    with (E/(name+'.log')).open('wb') as output:
        try:
            p = subprocess.run(command, cwd=R, env=env, stdout=output, stderr=subprocess.STDOUT, timeout=300 if scope == 'gate' else 120)
            code, timeout = p.returncode, False
        except subprocess.TimeoutExpired:
            code, timeout = None, True
    return dict(suite=suite, scope=scope, command=command, exitCode=code, timeout=timeout, watchdogSeconds=300 if scope=='gate' else 120, wallSeconds=time.monotonic()-started, log=name+'.log', skipDump=name+'-skips.txt', mode='Fresh actual normal owner-approved harness')

jobs = [(suite,scope) for scope in ['gate','normalization','original'] for suite in ['qt3','qt4']]
with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
    receipts = list(pool.map(run,jobs))
(E/'normal-receipts.json').write_text(json.dumps(receipts,indent=2)+'\n',encoding='utf-8')
assert all(p['exitCode']==0 and not p['timeout'] for p in receipts)
print('Six complete gate, normalization-set and original-case actual normal runs finished; inspect exact totals and skip identities.')
