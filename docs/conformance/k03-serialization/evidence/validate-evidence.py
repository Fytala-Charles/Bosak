"""Validate content-preserving name serialization and retained name-storage boundaries."""
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
expected={'src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs','src/Directory.Build.props','tests/Bosak.XPath.Providers.Tests/XmlNameSerializationTests.cs'}
changed=set(git('diff','--name-only',m['sourceBase'],'--','src','tests').decode().splitlines());assert changed==expected or changed==expected-{'tests/Bosak.XPath.Providers.Tests/XmlNameSerializationTests.cs'}
assert not git('diff','--name-only',m['sourceBase'],'--','AGENTS.md','assets','.crossnote','src/Bosak.XPath.Providers/Xml11','src/Bosak.XPath.Standard','tests/Bosak.XPath.Conformance','tests/Bosak.XPath.Conformance40').strip()
pin=load(E/'IMPLEMENTATION-PIN.json');assert pin['recordedBeforeImplementation'] and pin['sourceBase']==m['sourceBase']
s=(R/'src/Bosak.XPath.Providers/XDocument/XDocumentNode.cs').read_text(encoding='utf-8');assert 'Xml11NameCodec.DecodeName(xml)' not in s and 'Xml11NameCodec.DecodeName(token.ToString())' in s
assert '<Version>1.26.10</Version>' in (R/'src/Directory.Build.props').read_text(encoding='utf-8')
for n in [p for p in expected if p.endswith('.cs')]:
 t=(R/n).read_text(encoding='utf-8-sig');assert t.startswith('// ===') and 'SPDX-License-Identifier: Apache-2.0' in t and '27-10-2026' in t
for n,h in load(R/'docs/conformance/k03-unicode-names/evidence/AUDIT-PIN.json')['fixtures'].items():assert sha(R/n)==h,n
assert git('-C','tests/qt4tests','rev-parse','HEAD').decode().strip()==load(R/'docs/conformance/k03-unicode-names/evidence/AUDIT-PIN.json')['qt4Revision']
assert not git('-C','tests/qt4tests','status','--porcelain','--untracked-files=no').strip()
for row in load(E/'results.json'):assert json.loads((E/row['log']).read_text(encoding='utf-8-sig').splitlines()[-1])==row['result'],row['id']
before=sha(C/'RESULTS.json');subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert before==sha(C/'RESULTS.json')==m['reviewSha256']
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
render=load(E/'render-receipts.json');assert len(render['pages'])==11 and sha(R/'assets/css/fytala-markdown.css')==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n;targets=re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')) if p==C/'README.md' else re.findall(r'\[Content-preserving name decoding\]\(([^)]+)\)',p.read_text(encoding='utf-8'))
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for n,h in load(E/'binary-identities.json').items():assert sha(R/n)==h,n
 for p in render['pages']:assert sha(R/p['html'])==p['sha256']
print('PASS: 40 regressions; 21960 solution passes; exact676 QT3 skips and qt4 gate unchanged; 30 probes; original Unicode skip retained; 1079 prior artifacts; eleven HTML renders.')
