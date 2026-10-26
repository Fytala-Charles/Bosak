from pathlib import Path
import json,zipfile,shutil,hashlib
r=Path('D:/Development/Bosak');e=r/'.guard-tmp/release1261-candidate';d=r/'docs/release-evidence/1.26.1';p=r/'docs/RELEASE-1.26.1-CANDIDATE.json';proof=json.loads(p.read_text())
with zipfile.ZipFile(d/'test-receipts.zip','w',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for lane in ['full','lsp','focused-results']:
  for f in sorted((e/lane).glob('*.trx')):z.writestr(lane+'/'+f.name,f.read_bytes())
shutil.copyfile(e/'documentation-render-receipts.json',d/'documentation-render-receipts.json')
for source,target in [('build-release1261-candidate.py','build-and-test-source.py'),('test-release1261-packages.ps1','run-package-consumers.ps1'),('verify-release1261-gates.py','run-gates.py'),('augment-release1261-proof.py','bundle-receipts.py')]:shutil.copyfile(r/'.guard-tmp'/source,d/target)
proof['receiptArchive']='docs/release-evidence/1.26.1/test-receipts.zip'
proof['validationCommands']=[
 dict(phase='source-build',cwd='.guard-tmp/release1261-source',command=['dotnet','build','Bosak.sln','-c','Release']),
 dict(phase='source-tests',cwd='.guard-tmp/release1261-source',command=['dotnet','test','Bosak.sln','-c','Release','--no-build','--logger','trx','--results-directory',str(e/'full')]),
 dict(phase='language-server',cwd='.guard-tmp/release1261-source',command=['dotnet','test','tests/Bosak.LanguageServer.Tests','-c','Release','--logger','trx','--results-directory',str(e/'lsp')]),
 dict(phase='pack',cwd='.guard-tmp/release1261-source',command=['dotnet','pack','Bosak.sln','-c','Release','--no-build','-o',str(r/'.guard-tmp/release1261-packages')]),
 dict(phase='package-consumers-and-audits',cwd='.',command=['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File','.guard-tmp/test-release1261-packages.ps1']),
 dict(phase='conformance',cwd='.',command=['python','.guard-tmp/verify-release1261-gates.py'],detail='Commands, environments, timestamps, exit codes and raw hashes retained in gate-verification.json and run-gates.py.')]
proof['evidenceHashes']={f.relative_to(d).as_posix():hashlib.sha256(f.read_bytes()).hexdigest() for f in sorted(d.rglob('*')) if f.is_file()}
p.write_text(json.dumps(proof,indent=2)+'\n');(e/'verified-proof.json').write_bytes(p.read_bytes())
print('Retained all 12 raw TRXs in a byte-preserving archive and recorded consumer/source execution scripts.')
