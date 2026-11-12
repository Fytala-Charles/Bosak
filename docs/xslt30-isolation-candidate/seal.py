"""Seal original candidate evidence and unsuccessful preliminary attempts without rewriting them."""
import hashlib
import json
from pathlib import Path
import shutil
import sys
import zipfile


here = Path(__file__).resolve().parent
packet = Path(sys.argv[1]).resolve()
if (here / 'EVIDENCE.zip').exists():
    raise ValueError('Evidence already sealed; retain it and use a separate dossier for a new attempt')
report = json.loads((packet / 'REPORT.json').read_text(encoding='utf-8'))
audit = json.loads((packet / 'AUDIT.json').read_text(encoding='utf-8'))
if not report['exactTestSetHashes'] or not audit['originalCorpusUnchanged'] or not audit['runtimeLayoutUnchanged']:
    raise ValueError('Integrity audit refused sealing')
if any(audit['disposableCorpusDifferences'].values()):
    raise ValueError('Output-isolation candidate wrote to the disposable corpus')
receipt = json.loads((packet / 'RUN.json').read_text(encoding='utf-8'))
result = dict(report, run=receipt, integrity=audit, version='1.81.3', published=False,
              originalReferenceCounts=dict(Passed=10199, Failed=22, Skipped=4380),
              acceptance='Local regression candidate only; no frozen-gate adoption or full-conformance claim')
(here / 'RESULTS.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
for name in ['failures.csv', 'skips.csv']:
    shutil.copyfile(packet / name, here / name)
names = ['PLAN.json', 'RUN.json', 'REPORT.json', 'AUDIT.json', 'CORPUS-BEFORE.json', 'CORPUS-AFTER.json',
         'ORIGINAL-CORPUS-BEFORE.json', 'RUNTIME-HASHES.json', 'SOURCE-HASHES.json', 'SOURCE.zip',
         'stdout.log', 'stderr.log', 'outcomes.csv', 'failures.csv', 'skips.csv', 'GENERATED-OUTPUTS.zip',
         'QT3.log', 'QT4.log', 'PROFILE-CONTROLS.log', 'LANGUAGE-SERVER-TEST.log', 'DOTNET-INFO.txt',
         'VERIFICATION-REUSE.json', 'HOST-AWAKE.json', 'hold-awake.py']
entries = {}
def record(archive, path, name):
    content = path.read_bytes()
    entries[name] = dict(bytes=len(content), sha256=hashlib.sha256(content).hexdigest())
    archive.writestr(name, content)
with zipfile.ZipFile(here / 'EVIDENCE.zip', 'w', compression=zipfile.ZIP_DEFLATED) as archive:
    for name in names:
        record(archive, packet / name, name)
    for path in (packet / 'controls').glob('*'):
        if path.is_file():
            record(archive, path, 'controls/' + path.name)
    for path in (packet / 'controls-strict').glob('*'):
        if path.is_file():
            record(archive, path, 'controls-strict/' + path.name)
    for path in (packet / 'baseline-controls').glob('*'):
        if path.is_file():
            record(archive, path, 'baseline-controls/' + path.name)
    record(archive, packet.parent / 'xslt30-qualification-20261112/RUNTIME-HASHES.json', 'baseline-controls/RUNTIME-HASHES.json')
    for path in (packet / 'profile-controls').glob('*.log'):
        record(archive, path, 'profile-controls/' + path.name)
    prefix = packet.parent / 'xslt30-isolation-lf-20261112'
    for name in ['SOLUTION-BUILD.log', 'SOLUTION-BUILD-2.log', 'SOLUTION-TEST.log']:
        record(archive, prefix / name, 'verification/' + name)
    partial = packet.parent / 'xslt30-isolation-20261112'
    for name in ['RUN.json', 'PLAN.json', 'stdout.log', 'stderr.log', 'UNIT.log', 'UNIT-2.log',
                 'RESULT-DOCUMENT.log', 'RESULT-DOCUMENT-2.log', 'SOLUTION-TEST.log']:
        record(archive, partial / name, 'preliminary/' + name)
    for name in ['controls/CONTROLS.json', 'controls-2/CONTROLS.json']:
        record(archive, partial / name, 'preliminary/' + name)
    paused = packet.parent / 'xslt30-isolation-final-20261112'
    for name in ['RUN.json', 'PLAN.json', 'stdout.log', 'stderr.log', 'AUDIT.json', 'SOURCE-HASHES.json', 'RUNTIME-HASHES.json']:
        record(archive, paused / name, 'host-paused-attempt/' + name)
files = {}
for path in here.iterdir():
    if path.is_file() and path.name != 'MANIFEST.json':
        content = path.read_bytes()
        files[path.name] = dict(bytes=len(content), sha256=hashlib.sha256(content).hexdigest())
(here / 'MANIFEST.json').write_text(json.dumps(dict(files=files, archiveEntries=entries), indent=2) + '\n', encoding='utf-8')
print('Sealed', len(entries), 'evidence entries and', len(files), 'dossier files')
