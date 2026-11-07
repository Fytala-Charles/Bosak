"""Caught local refusal controls; never launches negative .NET diagnostics."""
import hashlib,tempfile,unittest
from pathlib import Path
from preflight import guard_files,packed_items
class PreflightControls(unittest.TestCase):
 def test_matching_hash(self):
  with tempfile.TemporaryDirectory() as d:
   r=Path(d).resolve();(r/'a').write_bytes(b'ok');guard_files(r,{'a':hashlib.sha256(b'ok').hexdigest()})
 def test_changed_hash_refuses(self):
  with tempfile.TemporaryDirectory() as d:
   r=Path(d).resolve();(r/'a').write_bytes(b'changed')
   with self.assertRaises(ValueError):guard_files(r,{'a':hashlib.sha256(b'ok').hexdigest()})
 def test_missing_source_refuses(self):
  with tempfile.TemporaryDirectory() as d:
   with self.assertRaises(ValueError):guard_files(Path(d).resolve(),{'missing':'0'*64})
 def test_pack_escape_refuses(self):
  with tempfile.TemporaryDirectory() as d:
   r=Path(d).resolve();child=r/'child';child.mkdir();(r/'outside').write_bytes(b'x')
   with self.assertRaises(ValueError):packed_items(child,{'Items':{'None':[{'FullPath':str(r/'outside'),'Pack':'true'}]}})
 def test_missing_asset_refuses(self):
  with tempfile.TemporaryDirectory() as d:
   r=Path(d).resolve()
   with self.assertRaises(ValueError):packed_items(r,{'Items':{'None':[{'FullPath':str(r/'NOTICE'),'Pack':'true'}]}})
if __name__=='__main__':unittest.main()
