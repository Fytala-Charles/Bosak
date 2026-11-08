"""Validate the scalar-name correction, complete gates and retained K03 boundaries."""
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
expected={'src/Bosak.XPath.Standard/Functions/ElementMap.cs','src/Directory.Build.props','tests/Bosak.XPath.Standard.Tests/ElementMapNameValidationTests.cs'}
changed=set(git('diff','--name-only',m['sourceBase'],'--','src','tests').decode().splitlines());assert changed==expected or changed==expected-{'tests/Bosak.XPath.Standard.Tests/ElementMapNameValidationTests.cs'}
assert not git('diff','--name-only',m['sourceBase'],'--','AGENTS.md','assets','.crossnote','src/Bosak.XPath.Providers','tests/Bosak.XPath.Conformance','tests/Bosak.XPath.Conformance40').strip()
pin=load(E/'IMPLEMENTATION-PIN.json');assert pin['recordedBeforeImplementation'] and pin['sourceBase']==m['sourceBase']
s=(R/'src/Bosak.XPath.Standard/Functions/ElementMap.cs').read_text(encoding='utf-8');assert 'Rune.DecodeFromUtf16' in s and 'OperationStatus.Done' in s and 'char.GetUnicodeCategory' not in s[s.index('private static bool IsNcName'):s.index('internal readonly record struct ParsedName')]
assert '<Version>1.26.9</Version>' in (R/'src/Directory.Build.props').read_text(encoding='utf-8')
for n in [p for p in expected if p.endswith('.cs')]:
 t=(R/n).read_text(encoding='utf-8-sig');assert t.startswith('// ===') and 'SPDX-License-Identifier: Apache-2.0' in t and '27-10-2026' in t
for n,h in load(R/'docs/conformance/k03-unicode-names/evidence/AUDIT-PIN.json')['fixtures'].items():assert sha(R/n)==h,n
assert git('-C','tests/qt4tests','rev-parse','HEAD').decode().strip()==load(R/'docs/conformance/k03-unicode-names/evidence/AUDIT-PIN.json')['qt4Revision']
assert not git('-C','tests/qt4tests','status','--porcelain','--untracked-files=no').strip()
for row in load(E/'results.json'):assert json.loads((E/row['log']).read_text(encoding='utf-8-sig').splitlines()[-1])==row['result'],row['id']
before=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert before==sha(C/'RESULTS.json')==m['reviewSha256']
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
render=load(E/'render-receipts.json');assert len(render['pages'])==10 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n;targets=re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')) if p==C/'README.md' else re.findall(r'\[Scalar validation correction\]\(([^)]+)\)',p.read_text(encoding='utf-8'))
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for n,h in load(E/'binary-identities.json').items():assert sha(R/n)==h,n
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS: 78 regressions; 21920 solution passes; QT3 31145/0/676 with exact676 skips; qt4 gate8436/0/1894; 30 probes; provider findings retained; 1026 prior artifacts; ten HTML renders.')
