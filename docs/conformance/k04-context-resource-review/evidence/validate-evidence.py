"""Verify the resource/module audit, retained failures and unchanged production policy."""
from pathlib import Path
import collections
import hashlib
import json
import re
import subprocess
import xml.etree.ElementTree as X

E=Path(__file__).resolve().parent
P=E.parent
R=E.parents[3]
C24=R/'docs/conformance/k04-context-declaration-review/evidence'
C26=R/'docs/conformance/k04-main-context/evidence'

def text(path):
    raw=path.read_bytes()
    return raw.decode('utf-16' if raw.startswith(b'\xff\xfe') else 'utf-8-sig').replace('\r\n','\n')

def load(name,folder=E):return json.loads(text(folder/name))
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git',*args],cwd=R).decode().strip()

def skips(path):
    result,reason={},None
    for line in text(path).splitlines():
        m=re.match(r'=== \d+ skipped: (.*) ===',line)
        if m:reason=m[1]
        elif line.strip():
            assert reason is not None and line not in result
            result[line]=reason
    return result

def totals(path):
    raw=text(path)
    return [int(re.search(r'^'+k+r':\s+(\d+)',raw,re.M)[1]) for k in ['Passed','Failed','Skipped']]

pin=load('REVIEW-PIN.json')
base=pin['sourceBase']
assert not git('diff',base,'--','src','tests','AGENTS.md')
assert sha(R/'.guard-tmp/k04-context-spec/xquery31.html')==pin['specification']['sha256']
for name,expected in load('previous-identities.json').items():assert sha(R/name)==expected,name
for name,expected in load('normal-source-identities.json').items():assert git('hash-object',name)==expected,name
for name,expected in load('binary-identities.json').items():assert sha(R/name)==expected,name
for name,expected in load('resource-identities.json').items():assert sha(R/name)==expected,name
assert 'exactly **31,165/0/656**' in text(R/'AGENTS.md')
assert '<Version>1.30.0</Version>' in text(R/'src/Directory.Build.props')
for suite,revision in pin['fixtureRevisions'].items():
    assert git('-C',str(R/'tests'/(suite+'tests')),'rev-parse','HEAD')==revision
    assert not git('-C',str(R/'tests'/(suite+'tests')),'status','--porcelain')

normal=text(R/'tests/Bosak.XPath.Conformance/TestExecutor.cs')
prototype=text(E/'prototype-executor.cs.txt')
history=next(l for l in prototype.splitlines(True) if '| 0.29 ' in l and '28-10-2026' in l)
assert prototype.replace(history,'',1)==normal.replace(r'\bdeclare\s+(copy-namespaces|context|construction)\b',r'\bdeclare\s+(copy-namespaces|construction)\b',1)
for name,row in load('copied-source-identities.json').items():
    assert sha(R/name)==row['sha256']
    if row['changed']:assert text(R/name)==prototype
    else:assert (R/name).read_bytes()==(R/row['source']).read_bytes()
subprocess.run(['git','apply','--check',str(P/'PROTOTYPE-ROUTING.patch')],cwd=R,check=True)

selected=load('selected-originals.json')
old=load('inventory.json',C24)['cases']
assert selected==[r for r in old if r['Suite']=='qt3' and not r['Selected']] and len(selected)==13
assert sum(r['ModuleCount']>0 for r in selected)==10
receipts=load('original-receipts.json')
assert len(receipts)==39 and all(r['exitCode']==0 and not r['timeout'] for r in receipts)
for row in receipts:assert json.loads(text(E/row['log']).splitlines()[-1])==row['result']
for phase,counts in [('normal',{'Passed':4,'Skipped':9}),('prototype',{'Passed':11,'Failed':2})]:
    actual=[r for r in receipts if r['mode']=='actual' and r['phase']==phase]
    assert len(actual)==13 and collections.Counter(r['result']['kind'] for r in actual)==counts
failures={r['case']:r['result']['message'] for r in receipts if r['mode']=='actual' and r['phase']=='prototype' and r['result']['kind']=='Failed'}
assert set(failures)=={'contextDecl-056','contextDecl-057'}
assert all("XPST0081: Prefix 'u' is not declared" in s for s in failures.values())
metadata={r['case']:r['result'] for r in receipts if r['mode']=='resources'}
assert len(metadata)==13
for row in metadata.values():
    for item in row['Sources']+row['Schemas']+row['Modules']:
        path=Path(item['FilePath']).resolve()
        assert path.is_relative_to(R) and path.is_file()
        assert path.relative_to(R).as_posix() in load('resource-identities.json')

controls=load('control-receipts.json')
assert len(controls)==12 and all(r['exitCode']==0 and not r['timeout'] for r in controls)
for row in controls:assert json.loads(text(E/row['log']).splitlines()[-1])==row['result']
assert collections.Counter(r['result']['kind'] for r in controls)=={'Passed':8,'Failed':4}
by={r['name']:r['result'] for r in controls}
assert set(n for n,r in by.items() if r['kind']=='Failed')=={'C27-library-alias-only','C27-matching-importer-alias','C27-conflicting-importer-alias','C27-colliding-union-type-contexts'}
assert by['C27-colliding-union-type-contexts']['message']=='Expected error XPTY0004 but succeeded'
assert by['C27-distinct-union-type-contexts']['kind']==by['C27-union-left-positive']['kind']=='Passed'
for number in ['056','057']:assert by['C27-original-'+number+'-importer-u-bound']['kind']=='Passed'
# Seeded controls retain the exact original expressions/assertions; only diagnostic bindings/path rebasing differ.
ns='{http://www.w3.org/2010/09/qt-fots-catalog}'
original=X.parse(R/'tests/qt3tests/prod/ContextItemDecl.xml').getroot()
control=X.parse(E/'control-fixtures.xml').getroot()
for number in ['056','057']:
    o=next(c for c in original.findall(ns+'test-case') if c.get('name')=='contextDecl-'+number)
    c=next(c for c in control.findall(ns+'test-case') if c.get('name')=='C27-original-'+number+'-importer-u-bound')
    assert o.findtext(ns+'test')==c.findtext(ns+'test')
    assert X.tostring(o.find(ns+'result'))==X.tostring(c.find(ns+'result'))
assert len(load('control-receipts.json',E/'initial-control-run'))==9
assert len(load('initial-union-control-receipts.json'))==11

sets=load('set-receipts.json')
assert len(sets)==3 and [r['exitCode'] for r in sets]==[0,2,0] and not any(r['timeout'] for r in sets)
assert totals(E/'qt3-set-normal.log')==[4,0,40]
assert totals(E/'qt3-set-prototype.log')==[42,2,0]
assert totals(E/'qt4-set-normal.log')==[0,0,53]
assert skips(E/'qt3-set-normal-skips.txt')==skips(C26/'qt3-normal-skips.txt')
assert skips(E/'qt4-set-normal-skips.txt')==skips(C26/'qt4-normal-skips.txt')
assert not skips(E/'qt3-set-prototype-skips.txt')
for case,message in failures.items():assert 'FAIL '+case+': '+message in text(E/'qt3-set-prototype.log')
routing=load('routing-receipts.json')
assert len(routing)==2 and all(r['exitCode']==0 and not r['timeout'] for r in routing)
phases={p:{(r['Suite'],r['Name']):r for r in [json.loads(l) for l in text(E/('routing-'+p+'.jsonl')).splitlines()]} for p in ['normal','prototype']}
assert all(len(r)==111 for r in phases.values())
changed=[]
for key,a in phases['normal'].items():
    b=phases['prototype'][key]
    assert {k:v for k,v in a.items() if k!='canRouteXquery'}=={k:v for k,v in b.items() if k!='canRouteXquery'}
    if a['canRouteXquery']!=b['canRouteXquery']:changed.append(key)
for row in load('routing-expectations.json'):
    for phase in phases:assert phases[phase][('qt3',row['name'])]['canRouteXquery']==row[phase]
assert len(changed)==43 and sum(not name.startswith('routing-') for _,name in changed)==40
assert len(receipts)+len(controls)+len(sets)+len(routing)==56
v=load('validation.json')
assert not any(v[k] for k in ['engineChanged','admissionChanged','baselineChanged','releaseApproved','admissionReady','freshFullSolutionRun','freshFullGateRun'])
assert v['inheritedSolutionPassed']==23822
render=load('render-receipt.json')
assert len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for page in render['pages']:assert sha(R/page['html'])==page['sha256']
manifest=load('manifest.json')
actual={p.relative_to(P).as_posix() for p in E.rglob('*') if p.is_file() and p.name!='manifest.json'}|{'RESULTS.json','PROTOTYPE-ROUTING.patch'}
assert actual==set(manifest['files'])
for name,expected in manifest['files'].items():assert sha(P/name)==expected,name
for name,expected in manifest['documents'].items():assert git('hash-object',name)==expected,name
print('C27 validated: all13 originals audited, two original failures and four control failures retained; normal source/policy unchanged.')
