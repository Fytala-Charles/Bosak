"""Seal the namespace correction evidence without changing either historical cohort."""
import hashlib
import json
from pathlib import Path
import shutil
import sys
import zipfile

here = Path(__file__).resolve().parent
packet = Path(sys.argv[1]).resolve()
diagnostics = packet.parent / 'xslt30-xml-version-20261112'
if (here / 'EVIDENCE.zip').exists():
    raise ValueError('Already sealed; use a new dossier for another qualification')
report = json.loads((packet / 'REPORT.json').read_text())
audit = json.loads((packet / 'AUDIT.json').read_text())
if not report['exactTestSetHashes'] or not audit['originalCorpusUnchanged'] or not audit['runtimeLayoutUnchanged']:
    raise ValueError('Source/corpus/runtime audit failed')
if any(audit['disposableCorpusDifferences'].values()):
    raise ValueError('Candidate wrote to corpus fixtures')
for name in ['failures.csv', 'skips.csv']:
    shutil.copyfile(packet / name, here / name)
result = dict(report, run=json.loads((packet / 'RUN.json').read_text()), integrity=audit,
              version='1.81.4', published=False, parentReview='https://github.com/Fytala-Charles/Bosak/pull/474')
(here / 'RESULTS.json').write_text(json.dumps(result, indent=2) + '\n')
entries = {}
def record(archive, path, name):
    content = path.read_bytes()
    entries[name] = dict(bytes=len(content), sha256=hashlib.sha256(content).hexdigest())
    archive.writestr(name, content)
names = ['PLAN.json', 'RUN.json', 'REPORT.json', 'AUDIT.json', 'CORPUS-BEFORE.json', 'CORPUS-AFTER.json',
         'ORIGINAL-CORPUS-BEFORE.json', 'RUNTIME-HASHES.json', 'SOURCE-HASHES.json', 'SOURCE.zip',
         'stdout.log', 'stderr.log', 'outcomes.csv', 'failures.csv', 'skips.csv', 'GENERATED-OUTPUTS.zip',
         'SOLUTION-BUILD.log', 'RUNNER-BUILD.log', 'SOLUTION-TEST.log', 'LANGUAGE-SERVER-TEST.log',
         'XML-VERSION.log', 'QT3.log', 'QT4.log', 'QT3-SKIPS.txt', 'QT4-SKIPS.txt',
         'QT3-SKIP-VERIFY.log', 'QT4-SKIP-VERIFY.log', 'FROZEN-SKIP-CHECK.json', 'BASELINE-IDENTITY.json',
         'PROFILE-CONTROLS.log', 'DOTNET-INFO.txt', 'HOST-AWAKE.json', 'PARENT-PR.json', 'PARENT-CI.json']
with zipfile.ZipFile(here / 'EVIDENCE.zip', 'w', compression=zipfile.ZIP_DEFLATED) as archive:
    for name in names:
        record(archive, packet / name, name)
    for directory in ['controls', 'profile-controls']:
        for path in (packet / directory).glob('*'):
            if path.is_file():
                record(archive, path, directory + '/' + path.name)
    for name in ['PROBE.json', 'PROBE-RUN.json', 'PROBE-SUMMARY.log', 'PROBE-BUILD.log',
                 'BASELINE-TEST.log', 'FOCUSED-TEST.log', 'FOCUSED-TEST-2.log',
                 'probe/Program.cs', 'probe/Probe.csproj', 'baseline-tests/Baseline.csproj']:
        record(archive, diagnostics / name, 'diagnostics/' + name)
files = {p.name: dict(bytes=p.stat().st_size, sha256=hashlib.sha256(p.read_bytes()).hexdigest())
         for p in here.iterdir() if p.is_file() and p.name != 'MANIFEST.json'}
(here / 'MANIFEST.json').write_text(json.dumps(dict(files=files, archiveEntries=entries), indent=2) + '\n')
print('Sealed', len(entries), 'evidence entries and', len(files), 'dossier files')
