"""Validate K04-C22 scope, reproducible identities and original execution receipts."""
from pathlib import Path
import collections
import hashlib
import json
import re
import subprocess

ROOT = Path(__file__).resolve().parents[4]
FOLDER = ROOT / 'docs/conformance/k04-normalization-options-fix'
E = FOLDER / 'evidence'
PREVIOUS = ROOT / 'docs/conformance/k04-normalization-fix/evidence'

def read(name, folder=E):
    return json.loads((folder / name).read_text(encoding='utf-8-sig'))

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def git(*args):
    return subprocess.check_output(['git', *args], cwd=ROOT).decode().strip()

def text(path):
    raw = path.read_bytes()
    return raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig')

def skip_map(path):
    result, reason = {}, None
    for line in text(path).splitlines():
        match = re.match(r'=== \d+ skipped: (.*) ===', line)
        if match:
            reason = match[1]
        elif line.strip():
            assert reason is not None and line not in result
            result[line] = reason
    return result

pin = read('IMPLEMENTATION-PIN.json')
assert pin['slice'] == 'K04-C22'
assert sha(E / 'adopted-function.xml') == pin['fragmentSha256']
for name, expected in read('previous-identities.json').items():
    assert sha(ROOT / name) == expected, name
for name, expected in read('source-identities.json').items():
    assert git('hash-object', name) == expected, name
for name, expected in read('binary-identities.json').items():
    assert sha(ROOT / name) == expected, name
allowed = set(read('source-identities.json'))
changed = git('diff', '--name-only', pin['sourceBase']).splitlines()
for name in changed:
    if name.startswith(('src/', 'tests/')) or name == 'AGENTS.md':
        assert name in allowed, name
library = (ROOT / 'src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs').read_text()
assert 'switch (NormalizeSpaceString(form).ToUpperInvariant())' in library
assert 'ctx.IsXPath40 && IsEmptySequence(args[1]) ? "NFC" : RequireStringRequired(args[1])' in library
assert '[(Namespaces.Fn, "normalize-unicode")] = new(["value", "form"], [null, "\'NFC\'"])' in library
assert '<Version>1.29.2</Version>' in (ROOT / 'src/Directory.Build.props').read_text()
test = (ROOT / 'tests/Bosak.XPath.Standard.Tests/NormalizationOptionsTests.cs').read_text()
assert test.startswith('// ===') and 'SPDX-License-Identifier: Apache-2.0' in test
assert test.count('/// <param name=') == 5
for file, failed, passed in [('regressions-before.log',80,166), ('regressions-after.log',0,246)]:
    assert re.search(r'Failed:\s+' + str(failed) + r', Passed:\s+' + str(passed) +
                     r',.*Total:\s+246', text(E / file)), file
assert '0 Error(s)' in text(E / 'build.log')
assert read('solution-receipt.json')['exitCode'] == 0
projects = re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+),.*? - ([\w.]+)\.dll', text(E / 'solution-tests.log'))
assert len(projects) == 10 and len({p[1] for p in projects}) == 10
assert sum(int(p[0]) for p in projects) == 23746
assert ('8196', 'Bosak.Xslt.Tests') in projects

current, old = read('probe-receipts.json'), read('probe-receipts.json', PREVIOUS)
assert len(current) == len(old) == 70
by = {p['name']:p for p in old}
changes = []
for p in current:
    assert p['exitCode'] == 0 and not p['timeout']
    assert json.loads(text(E / p['log']).splitlines()[-1]) == p['result']
    if p['result'] != by[p['name']]['result']:
        assert p['kind'] == 'control'
        assert by[p['name']]['result']['kind'] == 'Failed' and p['result']['kind'] == 'Passed'
        changes.append(p['name'])
assert changes == read('probe-deltas.json')['changes'] and len(changes) == 5
assert all(p['result']['kind'] == 'Passed' for p in current if p['kind'] == 'control')
assert collections.Counter(p['result']['kind'] for p in current if p['kind'] == 'original') == {'Passed':4, 'Failed':4}

for suite, count, totals, normal in [('qt3',657,(31164,0,657),(42,0,6)), ('qt4',1914,(8895,0,1914),(44,0,6))]:
    for suffix, expected in [('',totals), ('-normal',normal)]:
        raw = text(E / (suite + suffix + '.log'))
        got = tuple(int(re.search(label + r':\s+(\d+)', raw)[1]) for label in ['Passed','Failed','Skipped'])
        assert got == expected, (suite, suffix, got)
        assert skip_map(E / (suite + suffix + '-skips.txt')) == skip_map(PREVIOUS / (suite + suffix + '-skips.txt'))
    assert len(skip_map(E / (suite + '-skips.txt'))) == count
for name in ['gate-receipts.json','normal-set-receipts.json']:
    assert all(p['exitCode'] == 0 and not p['timeout'] for p in read(name))
v = read('validation.json')
assert v['gatedSets'] == 229 and not any(v[k] for k in ['admissionChanged','baselineChanged','releaseApproved'])
render = read('render-receipt.json')
assert len(render['pages']) == 9
assert sha(ROOT / 'assets/css/fytala-markdown.css') == render['cssSha256']
for page in render['pages']:
    assert sha(ROOT / page['html']) == page['sha256']
manifest = read('manifest.json')
actual = {p.relative_to(FOLDER).as_posix() for p in E.rglob('*') if p.is_file() and p.name != 'manifest.json'} | {'RESULTS.json'}
assert actual == set(manifest['files'])
for name, expected in manifest['files'].items():
    assert sha(FOLDER / name) == expected, name
for name, expected in manifest['documents'].items():
    assert git('hash-object', name) == expected, name
print('K04-C22 validated: 246 regressions, 23,746 solution tests, exact gates/skips, five corrected controls and preserved history.')
