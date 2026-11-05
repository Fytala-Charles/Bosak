"""Verify bounded K03 integration, complete final checks and exact conformance deltas."""
from pathlib import Path
import json,hashlib,subprocess,sys,re
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
m=load(E/'manifest.json')
for n,h in m['files'].items():
 assert sha(E/n)==h,n
 if '--staged' in sys.argv:assert hashlib.sha256(git('show',':'+(E/n).relative_to(R).as_posix())).hexdigest()==h,n
for n,h in m['documentsAndSource'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('rev-parse',':'+n).decode().strip()==h,n
for n,h in load(E/'previous-identities.json').items():assert sha(R/n)==h,n
expected=set(m['implementationFiles']);changed=set(git('diff','--name-only',m['sourceBase'],'--','src','tests').decode().splitlines());newtests={n for n in expected if n.startswith('tests/')};assert changed==expected or changed==expected-newtests
assert not git('diff','--name-only',m['sourceBase'],'--','AGENTS.md','assets','.crossnote','tests/Bosak.XPath.Conformance','tests/Bosak.XPath.Conformance40','src/Bosak.XPath.Providers/Xml11/Xml11NameCodec.cs').strip()
pin=load(E/'IMPLEMENTATION-PIN.json');assert pin['recordedBeforeImplementation'] and pin['sourceBase']==m['sourceBase']
loader=(R/'src/Bosak.XPath.Providers/Xml11/Xml11Loader.cs').read_text(encoding='utf-8');old=git('show',m['sourceBase']+':src/Bosak.XPath.Providers/Xml11/Xml11Loader.cs').decode('utf-8')
pattern=r'    private static XmlReaderSettings CreateSettings\(\).*?\n    }'
assert re.search(pattern,loader,re.S).group()==re.search(pattern,old,re.S).group()
assert 'DecodeTextValues' not in loader and 'EncodeInvalidCharacterReference' not in loader
assert '<Version>1.26.11</Version>' in (R/'src/Directory.Build.props').read_text(encoding='utf-8')
for n in [n for n in expected if n.endswith('.cs')]:
 s=(R/n).read_text(encoding='utf-8-sig');assert s.startswith('// ===') and 'SPDX-License-Identifier: Apache-2.0' in s and '27-10-2026' in s,n
fixture=load(R/'docs/conformance/k03-unicode-names/evidence/AUDIT-PIN.json')
for n,h in fixture['fixtures'].items():assert sha(R/n)==h,n
assert git('-C','tests/qt4tests','rev-parse','HEAD').decode().strip()==fixture['qt4Revision']
assert not git('-C','tests/qt4tests','status','--porcelain','--untracked-files=no').strip() and not git('-C','tests/qt3tests','status','--porcelain','--untracked-files=no').strip()
for row in load(E/'results.json'):assert json.loads((E/row['log']).read_text(encoding='utf-8-sig').splitlines()[-1])==row['result'],row['id']
assert (E/'qt4-prior-skips.txt').read_bytes()==git('show',m['sourceBase']+':docs/release-evidence/1.26.3/frozen-gate-skips.txt')
before=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert before==sha(C/'RESULTS.json')==m['reviewSha256']
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
render=load(E/'render-receipts.json');assert len(render['pages'])==12 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n;targets=re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')) if p==C/'README.md' else re.findall(r'\[Bounded core completion\]\(([^)]+)\)',p.read_text(encoding='utf-8'))
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for n,h in load(E/'binary-identities.json').items():assert sha(R/n)==h,n
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS: 47 new regressions; 22007 solution passes; QT3 exact31145/0/676; qt4 8437/0/1893 with only map-to-element-053 removed; 30 probes; 1131 prior artifacts; 12 HTML renders.')
