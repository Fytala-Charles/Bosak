"""Verify the separately retained trigger correction; never launches .NET."""
import hashlib, json, runpy, zipfile
from pathlib import Path
here=Path(__file__).resolve().parent
manifest=json.loads((here/'MANIFEST.json').read_text())
def check(data,value):assert len(data)==value['bytes'] and hashlib.sha256(data).hexdigest()==value['sha256']
for name,value in manifest['files'].items():check((here/name).read_bytes(),value)
with zipfile.ZipFile(here/'EVIDENCE.zip') as z:
    assert set(z.namelist())==set(manifest['archiveEntries'])
    for name,value in manifest['archiveEntries'].items():check(z.read(name),value)
    event=json.loads(z.read('CANCELLED-RUN.json').decode('utf-8-sig'))
    assert event['status']=='completed' and event['conclusion']=='cancelled'
    assert next(s for s in event['jobs'][0]['steps'] if s['name']=='Observe exactly one complete qualification attempt')['conclusion']=='skipped'
    before=z.read('WORKFLOW-BEFORE.yml').decode().replace('\r\n','\n')
    after=z.read('WORKFLOW-AFTER.yml').decode().replace('\r\n','\n')
    assert after.replace('    # One automatic observation on opening; later revisions require explicit dispatch.\n','').replace('    types: [opened]\n','')==before
    assert '  workflow_dispatch:\n' in after and '    types: [opened]\n' in after
runpy.run_path(str(here.parent/'xslt30-telemetry/verify.py'))
print('Cancellation before qualification and event-only guard correction verified; original sealed evidence intact.')
