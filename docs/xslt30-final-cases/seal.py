"""Seal the selected-profile qualification and retain rejected cohorts separately."""
import hashlib
import json
import re
from pathlib import Path
import shutil
import sys
import zipfile

here = Path(__file__).resolve().parent
packet = Path(sys.argv[1]).resolve()
diagnostics = packet.parent / 'xslt30-final-cases-20261112'
probe = packet.parent / 'xslt30-final-failures-20261112'
rejected = packet.parent / 'xslt30-final-qualified-20261112'
if (here / 'EVIDENCE.zip').exists():
    raise ValueError('Already sealed; preserve original evidence')
report = json.loads((packet / 'REPORT.json').read_text())
run = json.loads((packet / 'RUN.json').read_text())
audit = json.loads((packet / 'AUDIT.json').read_text())
if not run['success'] or not report['qualificationSuccessful'] or not audit['originalCorpusUnchanged'] or not audit['runtimeLayoutUnchanged']:
    raise ValueError('Qualification or integrity audit failed')
if any(audit['disposableCorpusDifferences'].values()):
    raise ValueError('Corpus was modified')
for name in ['failures.csv', 'skips.csv']:
    shutil.copyfile(packet / name, here / name)
(here / 'RESULTS.json').write_text(json.dumps(dict(report,run=run,integrity=audit,version='1.81.5',published=False,
    qualificationScope='Basic non-schema-aware Legacy/Xslt30 under unchanged exclusions; not full conformance or automatic gate adoption'),indent=2)+'\n')
entries = {}
def record(archive, path, name):
    content = path.read_bytes()
    entries[name] = dict(bytes=len(content),sha256=hashlib.sha256(content).hexdigest())
    archive.writestr(name,content)
names = ['PLAN.json','RUN.json','REPORT.json','AUDIT.json','CORPUS-BEFORE.json','CORPUS-AFTER.json',
    'ORIGINAL-CORPUS-BEFORE.json','RUNTIME-HASHES.json','SOURCE-HASHES.json','SOURCE.zip',
    'stdout.log','stderr.log','outcomes.csv','failures.csv','skips.csv','GENERATED-OUTPUTS.zip',
    'SOLUTION-BUILD.log','RUNNER-BUILD.log','SOLUTION-TEST.log','LANGUAGE-SERVER-TEST.log',
    'OUTPUT.log','TRY.log','NAMESPACE.log','QT3.log','QT4.log','QT3-SKIPS.txt','QT4-SKIPS.txt',
    'FROZEN-SKIP-CHECK.json','BASELINE-IDENTITY.json','PROFILE-CONTROLS.log','WRAPPER-CONTROLS.log',
    'DOTNET-INFO.txt','HOST-AWAKE.json','PARENT-CI.json','PARENT-SCHEMA-CI.json','POSTMERGE-474-RETRY.json',
    'MERGE-474.json','MERGE-475.json']
with zipfile.ZipFile(here/'EVIDENCE.zip','w',compression=zipfile.ZIP_DEFLATED) as archive:
    wrapper_log = (packet/'WRAPPER-CONTROLS.log').read_text(encoding='utf-8-sig')
    for match in re.finditer(r'PASS wrapper-[^\r\n]+ \(([^\r\n]+\.json)\)', wrapper_log):
        receipt = Path(match[1])
        record(archive,receipt,'wrapper-receipts/'+receipt.name)
        data = json.loads(receipt.read_text(encoding='utf-8-sig'))
        for key, value in data.items():
            if key in ('log','stderrLog') and isinstance(value,str) and Path(value).is_file():
                record(archive,Path(value),'wrapper-receipts/'+Path(value).name)
    for name in names: record(archive,packet/name,name)
    for directory in ['controls-nested','baseline-controls-nested','profile-controls','wrapper-controls']:
        for path in (packet/directory).rglob('*'):
            if path.is_file(): record(archive,path,directory+'/'+path.relative_to(packet/directory).as_posix())
    for name in ['FOCUSED-TEST.log','FOCUSED-TEST-2.log','FOCUSED-TEST-3.log','FOCUSED-TEST-4.log',
                 'BASELINE-TEST.log','BASELINE-TEST-2.log','POSTMERGE-474-FAILED.log','baseline-tests/Baseline.csproj']:
        record(archive,diagnostics/name,'diagnostics/'+name)
    for name in ['stdout.log','stderr.log','PROBE-RUN.json','ATTEMPT-1.stdout.log','ATTEMPT-1.stderr.log',
                 'ATTEMPT-1.RUN.json','probe/Program.cs','probe/Probe.csproj']:
        record(archive,probe/name,'probe/'+name)
    for name in ['RUN.json','PLAN.json','stdout.log','stderr.log','SOURCE.zip','SOURCE-HASHES.json',
                 'RUNTIME-HASHES.json','AUDIT.json','WRAPPER-CONTROLS.log','WRAPPER-CONTROLS-2.log']:
        record(archive,rejected/name,'rejected-first-cohort/'+name)
    for name in ['controls-nested','baseline-controls-nested']:
        for path in (rejected/name).glob('*'):
            if path.is_file(): record(archive,path,'rejected-first-cohort/'+name+'/'+path.name)
files = {p.name:dict(bytes=p.stat().st_size,sha256=hashlib.sha256(p.read_bytes()).hexdigest())
         for p in here.iterdir() if p.is_file() and p.name!='MANIFEST.json'}
(here/'MANIFEST.json').write_text(json.dumps(dict(files=files,archiveEntries=entries),indent=2)+'\n')
print('Sealed',len(entries),'evidence entries and',len(files),'dossier files')
