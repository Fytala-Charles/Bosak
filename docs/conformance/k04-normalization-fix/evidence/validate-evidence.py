"""Validate K04-C21 preserved receipts, scope and content identities."""
from pathlib import Path
import hashlib
import json
import re
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[4]
FOLDER = ROOT / 'docs/conformance/k04-normalization-fix'
E = FOLDER / 'evidence'

def read(name):
    return json.loads((E / name).read_text(encoding='utf-8-sig'))

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def git(*args):
    return subprocess.check_output(['git', *args], cwd=ROOT).decode().strip()

def log(name):
    raw = (E / name).read_bytes()
    return raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig')

def skip_map(path):
    raw = path.read_bytes()
    text = raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig')
    result, reason = {}, None
    for line in text.splitlines():
        match = re.match(r'=== \d+ skipped: (.*) ===', line)
        if match:
            reason = match[1]
        elif line.strip():
            assert reason is not None
            assert line not in result
            result[line] = reason
    return result

for name, expected in read('previous-identities.json').items():
    assert sha(ROOT / name) == expected, name
for name, expected in read('source-identities.json').items():
    assert git('hash-object', name) == expected, name
for name, expected in read('binary-identities.json').items():
    assert sha(ROOT / name) == expected, name

v = read('validation.json')
assert v['candidateVersion'] == '1.29.1'
assert not any(v[k] for k in ('admissionChanged', 'baselineChanged', 'releaseApproved'))
assert v['normalQt3'] == [31164, 0, 657] and v['normalQt4'] == [8895, 0, 1914]
changed = git('diff', '--name-only', v['sourceBase']).splitlines()
allowed = set(read('source-identities.json'))
allowed.update({'src/Bosak.XPath.Standard/Resources/Unicode17-LICENSE.txt',
                'src/Bosak.XPath.Standard/Resources/UnicodeNormalization17.bin'})
for name in changed:
    if name.startswith(('src/', 'tests/')) or name == 'AGENTS.md':
        assert name in allowed, name
assert 'FullNormalizationEngine.Normalize(input)' in (ROOT / 'src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs').read_text()
for name in ('src/Bosak.XPath.Standard/Functions/FullNormalizationEngine.cs',
             'tests/Bosak.XPath.Standard.Tests/FullNormalizationTests.cs'):
    source = (ROOT / name).read_text()
    assert source.startswith('// ===') and 'SPDX-License-Identifier: Apache-2.0' in source

c = read('corpus-results.json')
for key, expected in {'cases':20034, 'normalizationEquations':200340,
                      'scalarClassifications':1112064, 'identityChecks':1094978,
                      'malformedChecks':4, 'mismatches':0}.items():
    assert c[key] == expected, key
assert not c['failures']
assert sha(ROOT / 'src/Bosak.XPath.Standard/Resources/UnicodeNormalization17.bin') == c['embeddedDataSha256']
assert sha(ROOT / 'src/Bosak.XPath.Standard/Resources/Unicode17-LICENSE.txt') == c['embeddedLicenseSha256']
receipt = read('corpus-receipt.json')
assert receipt['exitCode'] == 0 and not receipt['timeout']
assert re.search(r'Failed:\s+90, Passed:\s+45,.*Total:\s+135', log('regressions-before.log'))
assert re.search(r'Failed:\s+0, Passed:\s+135,.*Total:\s+135', log('regressions-after.log'))
s = read('solution-test-summary.json')
assert len(s['projects']) == 10 and sum(p['passed'] for p in s['projects']) == 23500
for p in s['projects']:
    assert re.search(r'Passed!\s+- Failed:\s+0, Passed:\s+' + str(p['passed']) +
                     r',.*? - ' + re.escape(p['name']) + r'\.dll', log(p['source'])), p['name']
assert 'Failed' in log('initial-solution-tests.log') and '(sequence)' in log('initial-solution-tests.log')
assert len(read('probe-receipts.json')) == 70
for name in ('probe-receipts.json', 'gate-receipts.json', 'normal-set-receipts.json'):
    assert all(not p['timeout'] for p in read(name))
assert all(p['exitCode'] == 0 for p in read('gate-receipts.json'))
delta = read('probe-deltas.json')
assert delta['correctedPositiveOriginals'] == 2 and delta['correctedControls'] == 24
assert len(delta['changes']) == 26
for suite, count in [('qt3',657), ('qt4',1914)]:
    entry = read('skip-verification.json')[suite]
    assert entry['retainedGateNamesAndReasons'] == count and not entry['removed'] and not entry['added']
    current = skip_map(E / (suite + '-skips.txt'))
    assert len(current) == count
    assert current == skip_map(ROOT / 'docs/conformance/k04-date-optional-fix/evidence' / (suite + '-skips.txt'))
    assert skip_map(E / (suite + '-normal-skips.txt')) == skip_map(ROOT / 'docs/conformance/k04-normalization-review/evidence' / (suite + '-normal-skips.txt'))

p = read('package-verification.json')
assert sha(ROOT / p['package']) == p['packageSha256']
with zipfile.ZipFile(ROOT / p['package']) as z:
    assert hashlib.sha256(z.read(p['licenseEntry'])).hexdigest() == c['embeddedLicenseSha256']
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
print('K04-C21 evidence validated: corpus, regressions, 10-project aggregate, gates, scope, licenses and identities.')
