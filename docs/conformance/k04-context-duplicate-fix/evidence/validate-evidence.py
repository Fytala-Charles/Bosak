"""Verify the narrowly scoped duplicate-context error correction and unchanged admission."""
from pathlib import Path
import collections
import hashlib
import json
import re
import subprocess

E = Path(__file__).resolve().parent
P = E.parent
R = E.parents[3]
PREVIOUS = R/'docs/conformance/k04-context-declaration-review/evidence'
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
    return [int(re.search(r'^'+k+r':\s+(\d+)',raw,re.M)[1]) for k in ['Passed','Failed','Skipped']]

pin = load('IMPLEMENTATION-PIN.json')
base = pin['sourceBase']
assert sha(R/'.guard-tmp/k04-context-spec/xquery31.html')==pin['specification']['sha256']
for name, expected in load('previous-identities.json').items():
    assert sha(R/name)==expected, name
for name, expected in load('source-identities.json').items():
    assert git('hash-object',name)==expected, name
for name, expected in load('binary-identities.json').items():
    assert sha(R/name)==expected, name
changed = git('diff',base,'--name-only','--','src','tests','AGENTS.md').splitlines()
assert set(changed) <= set(load('source-identities.json'))
parser = 'src/Bosak.XQuery/Parser/XQueryParser.cs'
before = subprocess.check_output(['git','show',base+':'+parser],cwd=R).decode('utf-8').replace('\r\n','\n')
expected = before.replace('// XQST0113: more than one context item declaration in one module.',
                          '// XQST0099: more than one context item declaration in one module.').replace(
                          '"XQST0113: More than one context item declaration in a module."',
                          '"XQST0099: More than one context item declaration in a module."')
after = text(R/parser)
history = next(l for l in after.splitlines(True) if '| 1.17  | 28-10-2026' in l)
assert after.replace(history,'',1)==expected
props = 'src/Directory.Build.props'
old = subprocess.check_output(['git','show',base+':'+props],cwd=R).decode('utf-8').replace('\r\n','\n')
assert text(R/props)==old.replace('<Version>1.29.2</Version>','<Version>1.29.3</Version>')
for name in [parser,'tests/Bosak.XQuery.Tests/ContextDeclarationErrorTests.cs']:
    source = text(R/name)
    assert source.startswith('// ===') and 'SPDX-License-Identifier: Apache-2.0' in source
assert 'exactly **31,165/0/656**' in text(R/'AGENTS.md')
for file, failed, passed in [('regressions-before.log',8,16),('regressions-after.log',0,24)]:
    assert re.search(r'Failed:\s+'+str(failed)+r', Passed:\s+'+str(passed)+r',.*Total:\s+24',text(E/file))
assert '0 Error(s)' in text(E/'build.log')
assert load('solution-receipt.json')['exitCode']==0
projects = re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+),.*? - ([\w.]+)\.dll',text(E/'solution-tests.log'))
assert len(projects)==len({n for _,n in projects})==10
assert sum(int(n) for n,_ in projects)==23770
assert ('371','Bosak.XQuery.Tests') in projects and ('8196','Bosak.Xslt.Tests') in projects

previous = {r['name']:r for r in load('execution-receipts.json',PREVIOUS)}
rows = load('execution-receipts.json')
assert len(rows)==80 and all(r['exitCode']==0 and not r['timeout'] for r in rows)
for row in rows:
    assert row['result']==previous[row['name']]['result']
    assert json.loads(text(E/row['log']).splitlines()[-1])==row['result']
assert (E/'filter-results.jsonl').read_bytes()==(PREVIOUS/'filter-results.jsonl').read_bytes()
assert len(text(E/'filter-results.jsonl').splitlines())==97
oldlib = {r['name']:r for r in load('library-receipts.json',PREVIOUS)}
newlib = load('library-receipts.json')
assert len(newlib)==5 and all(r['exitCode']==0 and not r['timeout'] and r['result']['kind']=='Passed' for r in newlib)
assert [r['name'] for r in newlib if r['result']!=oldlib[r['name']]['result']]==['library-duplicate']
for row in newlib:
    assert json.loads(text(E/row['log']).splitlines()[-1])==row['result']
    if row['name']=='library-duplicate':
        assert 'XQST0099' in row['result']['error'] and 'XQST0113' not in row['result']['error']
    else:
        assert row['result']==oldlib[row['name']]['result']
for suite, expected, count in [('qt3',[31165,0,656],656),('qt4',[8939,0,1920],1920)]:
    assert totals(E/(suite+'.log'))==expected
    current = skips(E/(suite+'-skips.txt'))
    assert len(current)==count and current==skips(GATE/(suite+'-gate-skips.txt'))
    assert totals(E/(suite+'-normal.log'))==([4,0,40] if suite=='qt3' else [0,0,53])
    assert skips(E/(suite+'-normal-skips.txt'))==skips(PREVIOUS/(suite+'-normal-skips.txt'))
for name in ['gate-receipts.json','normal-receipts.json']:
    assert all(r['exitCode']==0 and not r['timeout'] for r in load(name))
assert load('filter-receipt.json')['exitCode']==0 and not load('filter-receipt.json')['timeout']
v = load('validation.json')
assert v['gatedSets']==230 and not any(v[k] for k in ['admissionChanged','baselineChanged','releaseApproved','mainModuleSupportChanged'])
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
print('K04-C25 validated: exact duplicate error correction, 24 regressions, 23,770 solution passes and unchanged gates/skips.')
