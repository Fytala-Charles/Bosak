"""Validate bounded main-context implementation, original assertions and unchanged admission."""
from pathlib import Path
import collections
import hashlib
import json
import re
import subprocess
import xml.etree.ElementTree as X

E = Path(__file__).resolve().parent
P = E.parent
R = E.parents[3]
PREVIOUS = R/'docs/conformance/k04-context-duplicate-fix/evidence'
C24 = R/'docs/conformance/k04-context-declaration-review/evidence'
GATE = R/'docs/conformance/k04-normalization-admission/adoption'

def text(path):
    raw = path.read_bytes()
    return raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')

def load(name, folder=E):
    return json.loads(text(folder/name))

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def git(*args):
    return subprocess.check_output(['git',*args],cwd=R).decode().strip()

def skips(path):
    result, reason = {}, None
    for line in text(path).splitlines():
        match = re.match(r'=== \d+ skipped: (.*) ===',line)
        if match:
            reason = match[1]
        elif line.strip():
            assert reason is not None and line not in result
            result[line] = reason
    return result

def totals(path):
    raw = text(path)
    return [int(re.search(r'^'+label+r':\s+(\d+)',raw,re.M)[1]) for label in ['Passed','Failed','Skipped']]

pin = load('IMPLEMENTATION-PIN.json')
base = pin['sourceBase']
assert sha(R/'.guard-tmp/k04-context-spec/xquery31.html') == pin['specification']['sha256']
for name, expected in load('previous-identities.json').items():
    assert sha(R/name)==expected, name
for name, expected in load('source-identities.json').items():
    assert git('hash-object',name)==expected, name
for name, expected in load('binary-identities.json').items():
    assert sha(R/name)==expected, name
allowed = set(load('source-identities.json'))
changed = set(git('diff',base,'--name-only','--','src','tests','AGENTS.md').splitlines())
assert changed <= allowed
for name in allowed:
    if name.endswith('.cs'):
        source = text(R/name)
        assert source.startswith('// ===') and 'SPDX-License-Identifier: Apache-2.0' in source
assert not git('diff',base,'--','AGENTS.md','tests/Bosak.XPath.Conformance','tests/Bosak.XPath.Conformance40')
assert 'exactly **31,165/0/656**' in text(R/'AGENTS.md')
assert '<Version>1.30.0</Version>' in text(R/'src/Directory.Build.props')
for name, failed, passed, total in [('regressions-before.log',30,6,36),
                                   ('regressions-after-initial.log',0,36,36),
                                   ('supplemental-assertion-initial.log',1,47,48),
                                   ('regressions-after.log',0,52,52)]:
    assert re.search(r'Failed:\s+'+str(failed)+r', Passed:\s+'+str(passed)+r',.*Total:\s+'+str(total),text(E/name)),name
assert load('supplemental-assertion-disposition.json')['expectedError']=='FOER0000'
assert not load('supplemental-assertion-disposition.json')['productionChanged']
assert '0 Error(s)' in text(E/'build.log')
assert load('solution-receipt.json')['exitCode']==0
projects = re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+),.*? - ([\w.]+)\.dll',text(E/'solution-tests.log'))
assert len(projects)==len({n for _,n in projects})==10
assert sum(int(n) for n,_ in projects)==23822
assert ('423','Bosak.XQuery.Tests') in projects and ('8196','Bosak.Xslt.Tests') in projects
initial = load('initial-verification-disposition.json')
assert initial['solutionPassed']==23806 and initial['solutionExitCode']==0
for name, expected in initial['files'].items():
    assert sha(E/'initial-verification'/name)==expected

grammar_phase = load('itemtype-review-disposition.json')
assert grammar_phase['solutionPassed']==23818 and grammar_phase['solutionExitCode']==0
assert grammar_phase['newRegressionRows']==4
for name, expected in grammar_phase['files'].items():
    assert sha(E/'pre-itemtype-review'/name)==expected

before = {row['name']:row for row in load('execution-receipts.json',PREVIOUS)}
rows = load('execution-receipts.json')
assert len(rows)==80 and all(r['exitCode']==0 and not r['timeout'] for r in rows)
changes, details = [], []
for row in rows:
    old, new = before[row['name']]['result'], row['result']
    assert json.loads(text(E/row['log']).splitlines()[-1])==new
    if old['kind']!=new['kind']:
        assert old['kind']=='Failed' and new['kind']=='Passed'
        changes.append(row['name'])
    elif old!=new:
        details.append(row['name'])
    if row['mode']=='public-xquery':
        request = next(r for r in load('selected-originals.json')+load('control-requests.json') if r['Name']==row['case'])
        source = X.parse(request['SetFile']).getroot()
        ns = source.tag.split('}')[0]+'}'
        case = next(c for c in source.findall(ns+'test-case') if c.get('name')==row['case'])
        def structure(n):
            return (n.tag,dict(n.attrib),(n.text or '').strip(),[structure(c) for c in n])
        assert structure(case.find(ns+'result'))==structure(X.fromstring(new['assertion']))
assert len(changes)==45 and changes==load('review-deltas.json')['changedOutcomes']
assert details==['original-qt3-contextDecl-023-public-xquery']
assert all(r['result']['kind']=='Passed' for r in rows if r['mode']=='public-xquery')
assert collections.Counter(r['result']['kind'] for r in rows if r['mode']=='actual')=={'Passed':1,'Skipped':30}
assert (E/'filter-results.jsonl').read_bytes()==(PREVIOUS/'filter-results.jsonl').read_bytes()
assert len(text(E/'filter-results.jsonl').splitlines())==97
oldlib = {r['name']:r for r in load('library-receipts.json',PREVIOUS)}
library = load('library-receipts.json')
assert len(library)==5 and all(r['exitCode']==0 and not r['timeout'] and r['result']==oldlib[r['name']]['result'] for r in library)
for suite, expected, count in [('qt3',[31165,0,656],656),('qt4',[8939,0,1920],1920)]:
    assert totals(E/(suite+'.log'))==expected
    current = skips(E/(suite+'-skips.txt'))
    assert len(current)==count and current==skips(GATE/(suite+'-gate-skips.txt'))
    assert totals(E/(suite+'-normal.log'))==([4,0,40] if suite=='qt3' else [0,0,53])
    assert skips(E/(suite+'-normal-skips.txt'))==skips(C24/(suite+'-normal-skips.txt'))
for name in ['gate-receipts.json','normal-receipts.json']:
    assert all(r['exitCode']==0 and not r['timeout'] for r in load(name))
assert load('filter-receipt.json')['exitCode']==0 and not load('filter-receipt.json')['timeout']
inventory = load('inventory.json',C24)
for name, expected in inventory['fixtureHashes'].items():
    assert sha(R/name)==expected
for suite, revision in load('REVIEW-PIN.json',C24)['fixtureRevisions'].items():
    assert git('-C',str(R/'tests'/(suite+'tests')),'rev-parse','HEAD')==revision
    assert not git('-C',str(R/'tests'/(suite+'tests')),'status','--porcelain')
v = load('validation.json')
assert v['gatedSets']==230 and not any(v[k] for k in ['admissionChanged','baselineChanged','releaseApproved'])
assert v['originalPublicPassed']==31 and v['independentControlsPassed']==18
render = load('render-receipt.json')
assert len(render['pages'])==9 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for page in render['pages']:
    assert sha(R/page['html'])==page['sha256']
manifest = load('manifest.json')
actual = {p.relative_to(P).as_posix() for p in E.rglob('*') if p.is_file() and p.name!='manifest.json'} | {'RESULTS.json'}
assert actual==set(manifest['files'])
for name, expected in manifest['files'].items():
    assert sha(P/name)==expected, name
for name, expected in manifest['documents'].items():
    assert git('hash-object',name)==expected, name
print('K04-C26 validated: 52 regressions, 31 original and18 control passes, 23,822 solution passes and unchanged admission/gates.')
