"""Validate owner-approved exact patch adoption and fresh actual normal receipts."""
from pathlib import Path
import hashlib
import json
import re
import subprocess

A = Path(__file__).resolve().parent
P = A.parent
R = A.parents[3]
E = P/'evidence'

def text(path):
    raw = path.read_bytes()
    return raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')

def load(name, folder=A):
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

approval = load('approval.json')
assert approval['decisionId'] == 'K04-normalization-adoption-v1'
assert approval['answer'] == 'Approve exact normalization adoption (Recommended)'
assert git('rev-parse',approval['proposalCommit']) == approval['sourceBase']
for name, expected in load('proposal-identities.json').items():
    path = R/name
    if name in ['docs/conformance/k04-normalization-admission/README.md','docs/conformance/k04-normalization-admission/BASELINE-UPDATE-PROPOSAL.md']:
        snapshot = A/('presented-'+path.name+'.txt')
        assert sha(snapshot) == expected
        assert text(path).endswith(text(snapshot))
    else:
        assert sha(path) == expected, name
for name, expected in load('previous-identities.json',E).items():
    assert sha(R/name) == expected, name

# The complete source/policy diff must equal the literal approved patch, including histories.
base = approval['sourceBase']
sections = text(P/'PROPOSED-ADOPTION.patch').split('--- a/')[1:]
names = []
for section in sections:
    name = section.splitlines()[0]
    names.append(name)
    expected = subprocess.check_output(['git','show',base+':'+name],cwd=R).decode('utf-8').replace('\r\n','\n')
    for hunk in re.split(r'^@@ .*?@@.*\n',section,flags=re.M)[1:]:
        lines = hunk.splitlines(True)
        old = ''.join(l[1:] for l in lines if l.startswith((' ','-')))
        new = ''.join(l[1:] for l in lines if l.startswith((' ','+')))
        assert expected.count(old) == 1
        expected = expected.replace(old,new,1)
    assert text(R/name) == expected, name
changed = git('diff',base,'--name-only','--','src','tests','AGENTS.md').splitlines()
assert set(changed) == set(names)
for name, expected in load('adopted-source-identities.json').items():
    assert git('hash-object',name) == expected, name
for name, expected in load('binary-identities.json').items():
    assert sha(R/name) == expected, name
for name, expected in load('production-source-verification.json')['sourceIdentities'].items():
    assert git('hash-object',name) == expected, name
assert load('production-source-verification.json')['currentInformationalVersion'] == '1.29.2+'+base
assert 'exactly **31,165/0/656**' in text(R/'AGENTS.md')
assert '<Version>1.29.2</Version>' in text(R/'src/Directory.Build.props')
runner = text(R/'tests/Bosak.XPath.Conformance40/ConformanceRunner40.cs')
block = runner.split(' GatedSets = ',1)[1].split('};',1)[0]
members = re.findall(r'^\s+"([^"\n]+)",',block,re.M)
assert len(members) == len(set(members)) == 230 and 'fn-normalize-unicode' in members

receipts = load('normal-receipts.json')
assert len(receipts) == 6 and all(p['exitCode']==0 and not p['timeout'] for p in receipts)
expected = {'qt3-gate':[31165,0,656], 'qt4-gate':[8939,0,1920],
            'qt3-normalization':[43,0,5], 'qt4-normalization':[44,0,6],
            'qt3-original':[1,0,1], 'qt4-original':[1,0,1]}
for scope, value in expected.items():
    assert totals(A/(scope+'.log')) == value
    assert totals(A/(scope+'.log')) == totals(E/(scope+'.log'))
    assert skips(A/(scope+'-skips.txt')) == skips(E/(scope+'-skips.txt'))
for p in receipts:
    assert '/tests/' in p['command'][1].replace('\\','/') and '.guard-tmp' not in p['command'][1]
assert 'Experimental-level (XPath40Experimental) passes: 11' in text(A/'qt4-gate.log')
assert '0 Error(s)' in text(A/'build.log')
assert load('solution-receipt.json')['exitCode'] == 0
projects = re.findall(r'Passed!\s+- Failed:\s+0, Passed:\s+(\d+),.*? - ([\w.]+)\.dll',text(A/'solution-tests.log'))
assert len(projects) == len({name for _,name in projects}) == 10
assert sum(int(count) for count,_ in projects) == 23746
assert ('8196','Bosak.Xslt.Tests') in projects
inventory = load('normalization-inventory.json',E)
for name, expected in inventory['fixtureHashes'].items():
    assert sha(R/name) == expected
for suite, revision in load('REVIEW-PIN.json',E)['fixtureRevisions'].items():
    assert git('-C',str(R/'tests'/(suite+'tests')),'rev-parse','HEAD') == revision
    assert not git('-C',str(R/'tests'/(suite+'tests')),'status','--porcelain')
v = load('validation.json')
assert v['adopted'] and not v['releaseApproved'] and v['candidateVersion'] == '1.29.2'
render = load('render-receipt.json')
assert len(render['pages']) == 11
assert sha(R/'assets/css/fytala-markdown.css') == render['cssSha256']
for page in render['pages']:
    assert sha(R/page['html']) == page['sha256']
manifest = load('manifest.json')
actual = {p.relative_to(A).as_posix() for p in A.rglob('*') if p.is_file() and p.name!='manifest.json' and p.name!='BASELINE-ADOPTION.md'}
assert actual == set(manifest['files'])
for name, expected in manifest['files'].items():
    assert sha(A/name) == expected, name
for name, expected in manifest['documents'].items():
    assert git('hash-object',name) == expected, name
print('K04-C23 adoption validated: exact approved patch, fresh complete normal gates, all skip identities and 23,746 solution passes.')
