"""Seal an uncommitted candidate's exact sources and runtime separately from its base commit."""
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import subprocess
import sys


root = Path(__file__).resolve().parents[2]
packet = Path(sys.argv[1]).resolve()
original = Path('D:/Development/Bosak/tests/xslt30-test')
reference = Path('D:/Development/Bosak/.guard-tmp/xslt30-qualification-20261112')
if (packet / 'PLAN.json').exists():
    raise ValueError('Never overwrite a prepared candidate packet')
spec = importlib.util.spec_from_file_location('audit', root / 'docs/xslt30-qualification/audit.py')
audit = importlib.util.module_from_spec(spec)
spec.loader.exec_module(audit)
plan = json.loads((reference / 'PLAN.json').read_text(encoding='utf-8'))
for item in plan['sets']:
    if audit.digest(packet / 'corpus' / item['file']) != item['sha256']:
        raise ValueError('Corpus checkout bytes differ from the pinned LF test set: ' + item['file'])
corpus_before = audit.inventory(packet / 'corpus')
if corpus_before != json.loads((reference / 'CORPUS-BEFORE.json').read_text(encoding='utf-8')):
    raise ValueError('Corpus input bytes differ from the complete original reference inventory')
plan.update(sourceCommit=None, sourceBaseCommit='46b3ba0421d99fa2107997229732ff925fc331b5',
            sourceIdentity='Uncommitted candidate; exact changed sources in SOURCE-HASHES.json and SOURCE.zip',
            standardsPolicy='Bounded adoption recorded in docs/xslt30-xml-version/ADOPTION.json')
source_names = subprocess.check_output(['git', '-C', str(root), 'diff', '--ignore-submodules=all', '--name-only', '--', 'src', 'tests'], text=True).splitlines()
source_names += ['tests/Bosak.Xslt.Tests/NamespaceUndeclarationTests.cs']
runtime = packet / 'runtime'
shutil.copytree(root / 'tests/Bosak.Xslt.Conformance/bin/Release/net10.0', runtime)
manifest = {p.relative_to(runtime).as_posix(): audit.digest(p) for p in runtime.rglob('*') if p.is_file()}
import zipfile
with zipfile.ZipFile(packet / 'SOURCE.zip', 'w', compression=zipfile.ZIP_DEFLATED) as archive:
    for name in source_names:
        archive.write(root / name, name)
sources = {name: audit.digest(root / name) for name in source_names}
for name, value in [('PLAN.json', plan), ('RUNTIME-HASHES.json', manifest), ('SOURCE-HASHES.json', sources),
                    ('CORPUS-BEFORE.json', corpus_before),
                    ('ORIGINAL-CORPUS-BEFORE.json', audit.inventory(original))]:
    (packet / name).write_text(json.dumps(value, indent=2) + '\n', encoding='utf-8')
print('Prepared', len(manifest), 'runtime files and', len(sources), 'changed source files')
