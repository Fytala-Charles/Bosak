from pathlib import Path
import shutil,json,hashlib
E=Path(__file__).resolve().parent;R=E.parents[3];T=R/'.guard-tmp/k04-copy-namespaces-baseline';(T/'lib').mkdir(parents=True,exist_ok=True)
for p,h in json.loads((E/'baseline-binary-identities.json').read_text(encoding='utf-8')).items():
 old=R/'.guard-tmp/k04-prolog-mode-harness/bin/Release/net10.0'/Path(p).name
 assert hashlib.sha256(old.read_bytes()).hexdigest()==h
 shutil.copyfile(old,R/p)
shutil.copyfile(E/'qualified-test-source.cs.txt',T/'CopyNamespacesTests.cs')
shutil.copyfile(E/'baseline.csproj.txt',T/'Baseline.csproj')
