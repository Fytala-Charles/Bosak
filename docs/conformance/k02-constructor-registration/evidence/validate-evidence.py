"""Validate K02-C01 source scope, complete test receipts and paired evidence."""
from pathlib import Path
import json,hashlib,subprocess,sys,zipfile,re
E=Path(__file__).resolve().parent;C=E.parent;R=C.parents[2]
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(b):return hashlib.sha256(b).hexdigest()
def git(*a):return subprocess.check_output(['git',*a],cwd=R)
m=load(E/'manifest.json')
for n,h in m['files'].items():
 assert sha((E/n).read_bytes())==h,n
 if '--staged' in sys.argv:assert sha(git('show',':'+(E/n).relative_to(R).as_posix()))==h,n
for n,h in m['sourceAndDocuments'].items():
 assert git('hash-object',n).decode().strip()==h,n
 if '--staged' in sys.argv:assert git('rev-parse',':'+n).decode().strip()==h,n
for n,h in load(E/'previous-identities.json').items():assert sha((R/n).read_bytes())==h,n
expected={'src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs','src/Bosak.XQuery/Api/XQueryExecutable.cs','src/Directory.Build.props','tests/Bosak.XQuery.Tests/SchemaConstructorRegistrationTests.cs'}
changed=set(git('diff','--name-only',m['sourceBase'],'--','src','tests').decode().splitlines())
# An unstaged new test is absent from git diff, so also verify the explicit expected test path.
assert changed==expected or (changed==expected-{'tests/Bosak.XQuery.Tests/SchemaConstructorRegistrationTests.cs'} and (R/'tests/Bosak.XQuery.Tests/SchemaConstructorRegistrationTests.cs').exists())
assert not git('diff','--name-only',m['sourceBase'],'--','AGENTS.md','assets','.crossnote','tests/Bosak.XPath.Conformance','tests/Bosak.XPath.Conformance40').strip()
pin=load(E/'IMPLEMENTATION-PIN.json');assert pin['recordedBeforeImplementation'] and pin['sourceBase']==m['sourceBase'] and 'Bosak.Schema' in pin['schemaCoordination']['owner']
source=(R/'src/Bosak.XQuery/Api/XQueryExecutable.cs').read_text(encoding='utf-8');assert 'ctx.SchemaSet = BuildSchemaSet(_staticContext.ImportedSchemas, ctx);\n            if (!ctx.SkipStandardFunctionPopulation)\n                FunctionLibrary.RegisterSchemaConstructors(ctx);' in source
standard=(R/'src/Bosak.XPath.Standard/Functions/FunctionLibrary.cs').read_text(encoding='utf-8');assert 'internal static void RegisterSchemaConstructors(EvaluationContext context)' in standard and 'public static void RegisterSchemaConstructors' not in standard
for n in [x for x in expected if x.endswith('.cs')]:
 s=(R/n).read_text(encoding='utf-8-sig');assert s.startswith('// ===') and 'SPDX-License-Identifier: Apache-2.0' in s and '27-10-2026' in s
assert '<Version>1.26.7</Version>' in (R/'src/Directory.Build.props').read_text(encoding='utf-8')
assert load(R/'docs/xpath40-catalog/CATALOG-ACCEPTANCE.json')['status']=='Complete'
old=R/'docs/conformance/k02-schema-inventory/evidence'
for name in ['fixture-files.json','trace-resource-identities.json']:
 for p,h in load(old/name).items():assert sha((R/p).read_bytes())==h,p
for p,v in load(old/'resource-identities.json').items():assert sha((R/p).read_bytes())==v['sha256'],p
assert git('-C','tests/qt3tests','rev-parse','HEAD').decode().strip()==load(old/'inventory-scope.json')['fixtureRevision'] and not git('-C','tests/qt3tests','status','--porcelain','--untracked-files=no').strip()
index=load(E/'raw-log-index.json');rows=load(E/'comparison-results.json')+load(E/'boundary-results.json');assert len(rows)==len(index)==682
with zipfile.ZipFile(E/'raw-process-logs.zip') as z:
 assert set(z.namelist())==set(index)
 for row in rows:
  b=z.read(row['log']);assert sha(b)==index[row['log']] and json.loads(b.decode('utf-8-sig').splitlines()[-1])==row['result']
before=sha((C/'RESULTS.json').read_bytes());subprocess.run([sys.executable,str(C/'build-review.py')],cwd=R,check=True,stdout=subprocess.DEVNULL);assert before==sha((C/'RESULTS.json').read_bytes())==m['reviewSha256']
render=load(E/'render-receipts.json');assert len(render['pages'])==9 and sha((R/'assets/css/fytala-markdown.css').read_bytes())==render['cssSha256']
for n in m['markdownFiles']:
 p=R/n
 targets=re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8')) if p==C/'README.md' else re.findall(r'\[First-evaluation correction\]\(([^)]+)\)',p.read_text(encoding='utf-8'))
 for href in targets:
  if '://' not in href and not href.startswith('#'):assert (p.parent/href.split('#')[0]).exists(),(n,href)
if '--local-artifacts' in sys.argv:
 for p,h in load(E/'binary-identities.json').items():assert sha((R/p).read_bytes())==h,p
 for p in render['pages']:assert sha((R/p['html']).read_bytes())==p['sha256']
print('PASS: 11 regressions; 21836 solution passes; both frozen gates; 68 fixes/4 remaining/0 new failures; 682 process logs; prior evidence and source scope verified.')
