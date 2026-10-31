"""Validate the bounded context-declaration review without changing admission or source."""
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

def text(path):
    raw = path.read_bytes()
    return raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')

def load(name):
    return json.loads(text(E/name))

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def git(*args):
    return subprocess.check_output(['git',*args],cwd=R).decode().strip()

pin = load('REVIEW-PIN.json')
assert not git('diff',pin['sourceBase'],'--','src','tests','AGENTS.md')
assert sha(R/'.guard-tmp/k04-context-spec/xquery31.html') == pin['specification']['sha256']
for name, expected in load('previous-identities.json').items():
    assert sha(R/name) == expected, name
for name, expected in load('source-identities.json').items():
    assert git('hash-object',name) == expected, name
for name, expected in load('binary-identities.json').items():
    assert sha(R/name) == expected, name
assert 'exactly **31,165/0/656**' in text(R/'AGENTS.md')
assert '<Version>1.29.2</Version>' in text(R/'src/Directory.Build.props')
inventory = load('inventory.json')
for name, expected in inventory['fixtureHashes'].items():
    assert sha(R/name) == expected
for suite, revision in pin['fixtureRevisions'].items():
    assert git('-C',str(R/'tests'/(suite+'tests')),'rev-parse','HEAD') == revision
    assert not git('-C',str(R/'tests'/(suite+'tests')),'status','--porcelain')
assert collections.Counter(r['Suite'] for r in inventory['cases']) == {'qt3':44,'qt4':53}
selected = [r for r in inventory['cases'] if r['Selected']]
assert selected == load('selected-originals.json') and len(selected) == 31
assert all(r['Suite']=='qt3' and r['MainDeclaration'] and not r['Resources'] and not r['ModuleCount'] for r in selected)
assert load('filter-requests.json') == inventory['cases']
filters = [json.loads(l) for l in text(E/'filter-results.jsonl').splitlines()]
assert len(filters) == 97
by = {(r['Suite'],r['Name']):r for r in filters}
assert len(by) == 97
assert all(by[(r['Suite'],r['Name'])]['xqueryDependenciesSupported'] and not by[(r['Suite'],r['Name'])]['canRouteXquery'] for r in selected)
assert sum(r['canRouteXquery'] for r in filters if r['Suite']=='qt3') == 4
assert all(not r['xpathSupported'] for r in filters)
meta = load('filter-receipt.json')
assert meta['exitCode']==0 and not meta['timeout']

receipts = load('execution-receipts.json')
assert len(receipts) == 80
for row in receipts:
    assert row['exitCode']==0 and not row['timeout']
    assert json.loads(text(E/row['log']).splitlines()[-1]) == row['result']
expected = {('original','actual'):{'Skipped':30,'Passed':1},
            ('original','public-xquery'):{'Failed':30,'Passed':1},
            ('control','public-xquery'):{'Failed':15,'Passed':3}}
for (kind,mode), counts in expected.items():
    assert collections.Counter(r['result']['kind'] for r in receipts if r['kind']==kind and r['mode']==mode) == counts
assert {r['case'] for r in receipts if r['kind']=='original' and r['result']['kind']=='Passed'} == {'contextDecl-023'}
for row in receipts:
    if row['kind']=='original' and row['mode']=='public-xquery':
        assert row['result']['exceptionType']=='XPathParseException' and 'XPST0003' in row['result']['error']
        original = next(r for r in selected if r['Name']==row['case'])
        expected_assert = X.fromstring(original['Assertion'])
        observed_assert = X.fromstring(row['result']['assertion'])
        def structure(n):
            return (n.tag,dict(n.attrib),(n.text or '').strip(),[structure(c) for c in n])
        assert structure(expected_assert) == structure(observed_assert)

library = load('library-receipts.json')
assert len(library)==5 and all(r['exitCode']==0 and not r['timeout'] for r in library)
assert collections.Counter(r['result']['kind'] for r in library)=={'Passed':4,'Failed':1}
duplicate = next(r for r in library if r['name']=='library-duplicate')
assert duplicate['result']['expectedCode']=='XQST0099' and 'XQST0113' in duplicate['result']['error']
for row in library:
    assert json.loads(text(E/row['log']).splitlines()[-1]) == row['result']
initial = load('initial-library-setup-disposition.json')
assert initial['timeout'] and initial['watchdogSeconds']==20 and not initial['engineOrFixtureChanged']
assert 'TypeLoadException' in text(E/'initial-library-setup.log')
# Library probes add a new dispatch branch; original public execution paths remain byte-equivalent.
driver = text(E/'executor.cs.txt')
start = driver.index('if(args[0]=="library-parse"){')
end = driver.index('if(args[0]=="filters"){',start)
assert driver[:start]+driver[end:] == text(E/'original-executor.cs.txt')

normal = load('normal-receipts.json')
assert len(normal)==2 and all(r['exitCode']==0 and not r['timeout'] for r in normal)
for suite, expected in [('qt3',[4,0,40]),('qt4',[0,0,53])]:
    raw = text(E/(suite+'-normal.log'))
    totals = [int(re.search('^'+k+r':\s+(\d+)',raw,re.M)[1]) for k in ['Passed','Failed','Skipped']]
    assert totals == expected
assert len(receipts)+len(library)+len(normal)+1 == 88
v = load('validation.json')
assert not any(v[k] for k in ['engineChanged','admissionChanged','baselineChanged','releaseApproved','freshFullSolutionRun','freshFullGateRun'])
assert v['inheritedSolutionPassed']==23746
render = load('render-receipt.json')
assert len(render['pages'])==10
assert sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for page in render['pages']:
    assert sha(R/page['html'])==page['sha256']
manifest = load('manifest.json')
actual = {p.relative_to(P).as_posix() for p in E.rglob('*') if p.is_file() and p.name!='manifest.json'} | {'RESULTS.json'}
assert actual==set(manifest['files'])
for name, expected in manifest['files'].items():
    assert sha(P/name)==expected, name
for name, expected in manifest['documents'].items():
    assert git('hash-object',name)==expected, name
print('K04-C24 review validated: 97-case inventory, 88 final bounded processes, preserved failures and unchanged source/gates.')
