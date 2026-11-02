"""Retain the fresh source build and exact observed native probe binaries."""
from pathlib import Path
import zipfile,json,hashlib
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];W=R/'.guard-tmp/schema-reconciliation-v2';out=W/'source-and-probe.zip'
if out.exists():raise RuntimeError('Archive retained; refuse overwrite')
with zipfile.ZipFile(out,'w',zipfile.ZIP_DEFLATED) as z:
 for f in sorted(W.rglob('*')):
  if not f.is_file() or f==out or 'obj' in f.relative_to(W).parts:continue
  rel=f.relative_to(W)
  if 'bin' in rel.parts and not str(rel).startswith('Host'):continue
  z.write(f,rel.as_posix())
 for name in ['license.md','NOTICE']:z.write(R/name,'Core-'+name)
 native=R/'.worktrees/Bosak.Schema.Assessment/LICENSE.md'
 if native.exists():z.write(native,'Schema-LICENSE.md')
receipt={'path':str(out),'sha256':hashlib.sha256(out.read_bytes()).hexdigest(),'freshSchemaProjectSha256':hashlib.sha256((W/'Native.csproj').read_bytes()).hexdigest(),'freshProbeProjectSha256':hashlib.sha256((W/'Host/Facts.csproj').read_bytes()).hexdigest(),'scope':'Fresh pinned public Schema sources and observed historical dependency binaries; not BCAP adapter/native acceptance'}
(P/'evidence/ARTIFACTS.json').write_bytes((json.dumps(receipt,indent=2)+'\n').encode());print('Retained isolated source/probe archive and exact build-project hashes.')
