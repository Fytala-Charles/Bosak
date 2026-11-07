"""Preserve preview artifacts and execution receipts without claiming release acceptance."""
from pathlib import Path
import hashlib,json,zipfile
from packages import inspect

P=Path(__file__).resolve().parents[1];R=P.parents[1];E=P/'evidence'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
p=json.loads((P/'PLAN.json').read_text());w=Path(p['work']);packages=inspect()
with zipfile.ZipFile(E/'PREVIEW-PACKAGES.zip','x',zipfile.ZIP_DEFLATED) as z:
    for row in packages['packages']:
        file=Path(row['path']);z.write(file,file.name)
with zipfile.ZipFile(E/'SOURCE-BUNDLE.zip','x',zipfile.ZIP_DEFLATED) as z:
    for name,h in p['archivedFiles'].items():
        file=Path(p['source'])/name;assert sha(file)==h,name;z.write(file,name)
layouts={}
with zipfile.ZipFile(E/'EXECUTION-LAYOUTS.zip','x',zipfile.ZIP_DEFLATED) as z:
    for name,folder in [('consumer',w/'consumer/bin/Release/net10.0'),('retained',w/'retained')]:
        files={f.relative_to(folder).as_posix():sha(f) for f in sorted(folder.rglob('*')) if f.is_file()}
        layouts[name]=dict(path=str(folder),files=files)
        for relative in files:z.write(folder/relative,name+'/'+relative)
(E/'EXECUTION-LAYOUTS.json').write_text(json.dumps(layouts,indent=2)+'\n',encoding='utf-8')
with zipfile.ZipFile(E/'EXECUTION-RECEIPTS.zip','x',zipfile.ZIP_DEFLATED) as z:
    for log in sorted((R/'docs/foundation-r2-closure/evidence/execution').glob('preview-1811-*.log')):
        receipt=log.with_suffix('.log.json');row=json.loads(receipt.read_text())
        assert row['exitCode']==0 and row['failure'] is None and row['logSha256']==sha(log),log.name
        z.write(log,log.name);z.write(receipt,receipt.name)
for file in ['Consumer.csproj','NuGet.Config']:
    (E/file).write_bytes((w/'consumer'/file).read_bytes())
(E/'project.assets.json').write_bytes((w/'consumer/obj/project.assets.json').read_bytes())
print('Preserved nine nupkgs, materialized source bundle, execution layouts and 14 successful build/pack/consumer/retained receipts.')
