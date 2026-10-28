"""Verify the exact unadopted normalization admission patch and isolated gate receipts."""
from pathlib import Path
import hashlib
import json
import re
import subprocess

E = Path(__file__).resolve().parent
P = E.parent
R = E.parents[3]
PREVIOUS = R / 'docs/conformance/k04-normalization-options-fix/evidence'

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

pin = load('REVIEW-PIN.json')
base = pin['sourceBase']
assert not git('diff',base,'--','src','tests','AGENTS.md')
for name, expected in load('previous-identities.json').items():
    assert sha(R/name) == expected, name
for name, expected in load('normal-source-identities.json').items():
    assert git('hash-object',name) == expected, name
for name, expected in load('binary-identities.json').items():
    assert sha(R/name) == expected, name
assert 'exactly **31,164/0/657**' in text(R/'AGENTS.md')
assert '<Version>1.29.2</Version>' in text(R/'src/Directory.Build.props')

# Reconstruct every proposed file from normal source and the literal reviewable patch.
sections = text(P/'PROPOSED-ADOPTION.patch').split('--- a/')[1:]
reconstructed = {}
for section in sections:
    name = section.splitlines()[0]
    expected = subprocess.check_output(['git','show',base+':'+name],cwd=R).decode('utf-8').replace('\r\n','\n')
    for hunk in re.split(r'^@@ .*?@@.*\n',section,flags=re.M)[1:]:
        lines = hunk.splitlines(True)
        old = ''.join(l[1:] for l in lines if l.startswith((' ','-')))
        new = ''.join(l[1:] for l in lines if l.startswith((' ','+')))
        assert expected.count(old) == 1
        expected = expected.replace(old,new,1)
    reconstructed[name] = expected
assert set(reconstructed) == set(pin['normalSourceIdentities'])
assert 'exactly **31,165/0/656**' in reconstructed['AGENTS.md']
for suite, path in [('qt3','tests/Bosak.XPath.Conformance/ConformanceRunner.cs'),('qt4','tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs')]:
    assert text(E/(suite+'-prototype-runner.cs.txt')) == reconstructed[path]
subprocess.run(['git','apply','--check',str(P/'PROPOSED-ADOPTION.patch')],cwd=R,check=True)

def members(source, field):
    block = source.split(' '+field+' = ',1)[1].split('};',1)[0]
    return re.findall(r'^\s+"([^"\n]+)",',block,re.M)

old40 = text(R/'tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs')
oldsets = members(old40,'GatedSets')
newsets = members(reconstructed['tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs'],'GatedSets')
assert len(oldsets) == len(set(oldsets)) == 229
assert len(newsets) == len(set(newsets)) == 230
assert set(newsets)-set(oldsets) == {'fn-normalize-unicode'} and not set(oldsets)-set(newsets)
for name, row in load('copied-source-identities.json').items():
    assert sha(R/name) == row['sha256']
    if row['proposed']:
        assert text(R/name) == reconstructed[row['normalSource']]
    else:
        assert (R/name).read_bytes() == (R/row['normalSource']).read_bytes()

inventory = load('normalization-inventory.json')
for name, expected in inventory['fixtureHashes'].items():
    assert sha(R/name) == expected
for suite, count in [('qt3',48),('qt4',50)]:
    assert len(inventory['cases'][suite]) == count
    assert git('-C',str(R/'tests'/(suite+'tests')),'rev-parse','HEAD') == pin['fixtureRevisions'][suite]
    assert not git('-C',str(R/'tests'/(suite+'tests')),'status','--porcelain')

assert totals(E/'qt3-gate.log') == [31165,0,656]
assert totals(E/'qt4-gate.log') == [8939,0,1920]
assert totals(E/'qt3-normalization.log') == [43,0,5]
assert totals(E/'qt4-normalization.log') == [44,0,6]
# The substring case filter also selects the related negative ...006a, which remains skipped.
for suite in ['qt3','qt4']:
    assert totals(E/(suite+'-original.log')) == [1,0,1]
    assert set(skips(E/(suite+'-original-skips.txt'))) == {'cbcl-fn-normalize-unicode-006a'}
    assert 'cbcl-fn-normalize-unicode-006' not in skips(E/(suite+'-normalization-skips.txt'))
old3, new3 = skips(PREVIOUS/'qt3-skips.txt'), skips(E/'qt3-gate-skips.txt')
assert set(old3)-set(new3) == {'cbcl-fn-normalize-unicode-006'} and not set(new3)-set(old3)
assert all(old3[n] == reason for n,reason in new3.items()) and len(new3) == 656
old4, new4 = skips(PREVIOUS/'qt4-skips.txt'), skips(E/'qt4-gate-skips.txt')
qualified = skips(PREVIOUS/'qt4-normal-skips.txt')
assert set(new4)-set(old4) == set(qualified) and not set(old4)-set(new4)
assert all(new4[n] == reason for n,reason in (old4|qualified).items()) and len(new4) == 1920
normal3 = skips(PREVIOUS/'qt3-normal-skips.txt')
normal3.pop('cbcl-fn-normalize-unicode-006')
assert skips(E/'qt3-normalization-skips.txt') == normal3
assert skips(E/'qt4-normalization-skips.txt') == qualified
receipts = load('rehearsal-receipts.json')
assert len(receipts) == 6 and all(r['exitCode']==0 and not r['timeout'] for r in receipts)
for suite in ['qt3','qt4']:
    assert '0 Error(s)' in text(E/(suite+'-build.log'))
assert 'Experimental-level (XPath40Experimental) passes: 11' in text(E/'qt4-gate.log')
v = load('validation.json')
assert not v['adopted'] and not v['releaseApproved'] and v['inheritedSolutionPassed'] == 23746
render = load('render-receipt.json')
assert len(render['pages']) == 10
assert sha(R/'assets/css/fytala-markdown.css') == render['cssSha256']
for page in render['pages']:
    assert sha(R/page['html']) == page['sha256']
manifest = load('manifest.json')
actual = {p.relative_to(P).as_posix() for p in E.rglob('*') if p.is_file() and p.name!='manifest.json'} | {'RESULTS.json','PROPOSED-ADOPTION.patch'}
assert actual == set(manifest['files'])
for name, expected in manifest['files'].items():
    assert sha(P/name) == expected, name
for name, expected in manifest['documents'].items():
    assert git('hash-object',name) == expected, name
print('K04-C23 proposal validated: exact three-file patch, isolated gates, skip deltas and unchanged normal policy.')
