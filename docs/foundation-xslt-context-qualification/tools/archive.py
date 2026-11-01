"""Preserve exact candidate sources and original/corrected XSLT bytes with licensing."""
from pathlib import Path
import json,hashlib,zipfile
ROOT=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];E=P/'evidence';W=ROOT/'.guard-tmp/xslt-context-qualification';C=json.loads((E/'CORRECTION.json').read_text());old=json.loads((ROOT/'docs/foundation-xslt-explicit-context/evidence/INPUTS.json').read_text())
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
source=W/'Corrected/src/Bosak.Xslt';pins={}
with zipfile.ZipFile(E/'candidate-source.zip','w',compression=zipfile.ZIP_DEFLATED) as z:
 for p in sorted(source.rglob('*')):
  if not p.is_file() or 'bin' in p.relative_to(source).parts or 'obj' in p.relative_to(source).parts:continue
  name='src/Bosak.Xslt/'+p.relative_to(source).as_posix();z.write(p,name);pins[name]=sha(p)
 for name in ['license.md','NOTICE']:z.write(ROOT/name,name);pins[name]=sha(ROOT/name)
with zipfile.ZipFile(E/'candidate-binaries.zip','w',compression=zipfile.ZIP_DEFLATED) as z:
 z.write(old['candidatePath'],'original/Bosak.Xslt.dll');z.write(C['correctedCandidatePath'],'corrected/Bosak.Xslt.dll')
 for name in ['license.md','NOTICE']:z.write(ROOT/name,name)
(E/'ARTIFACTS.json').write_text(json.dumps({'sourceArchiveSha256':sha(E/'candidate-source.zip'),'sourceFiles':pins,'binaryArchiveSha256':sha(E/'candidate-binaries.zip'),'originalCandidateSha256':old['candidateSha256'],'correctedCandidateSha256':C['correctedCandidateSha256'],'portableBitIdenticalRebuildClaimed':False},indent=2)+'\n')
print('Archived exact original/corrected binaries and corrected source inputs with license/NOTICE.')
