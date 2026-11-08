"""Create an immutable local transport archive with declared sources and qualified artifacts."""
from pathlib import Path
import json,hashlib,zipfile
R=Path(__file__).resolve().parents[3];P=Path(__file__).resolve().parents[1];W=R/'.guard-tmp/bcap-owner-review';W.mkdir(exist_ok=True);out=W/'review-inputs.zip'
if out.exists():raise RuntimeError('Transport already exists; preserve it')
proposal=json.loads((P/'PROPOSAL-MANIFEST.json').read_text())
with zipfile.ZipFile(out,'w',zipfile.ZIP_DEFLATED) as z:
 for folder in ['foundation-bcap-outcomes','foundation-bcap-profile','foundation-bcap-cost','foundation-bcap-owner-review']:
  root=R/'docs'/folder
  for f in sorted(root.rglob('*')):
   if not f.is_file() or '__pycache__' in f.parts or (folder=='foundation-bcap-owner-review' and f.relative_to(root).parts[0]=='returns' and f.name!='README.md'):continue
   z.write(f,f.relative_to(R).as_posix())
 for name,v in proposal['qualifiedArchives'].items():
  f=Path(v['path']);assert hashlib.sha256(f.read_bytes()).hexdigest()==v['sha256'];z.write(f,'artifacts/'+name)
 for name in ['license.md','NOTICE']:z.write(R/name,name)
receipt={'path':str(out),'sha256':hashlib.sha256(out.read_bytes()).hexdigest(),'bytes':out.stat().st_size,'proposalManifestSha256':hashlib.sha256((P/'PROPOSAL-MANIFEST.json').read_bytes()).hexdigest(),'deliveryConfirmed':False,'ownerAcceptance':False,'scope':'Local transport artifact; archive paths are descriptive, not provider authority. No message/publication/upload is performed.'}
(W/'TRANSPORT.json').write_text(json.dumps(receipt,indent=2)+'\n')
print(json.dumps(receipt,indent=2))
