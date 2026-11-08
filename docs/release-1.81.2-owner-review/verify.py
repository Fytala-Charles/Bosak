from pathlib import Path
import hashlib
import json
import zipfile

root = Path(__file__).resolve().parent
pins = json.loads((root / "CANDIDATE-PINS.json").read_text(encoding="utf-8"))
for entry in pins["artifacts"]:
    with zipfile.ZipFile(root / entry["archive"]) as archive:
        assert hashlib.sha256(archive.read(entry["manifestPath"])).hexdigest() == entry["manifestSha256"]
assert hashlib.sha256((root / pins["apiInventoryPath"]).read_bytes()).hexdigest() == pins["apiInventorySha256"]
assert hashlib.sha256((root.parent / "release-linux-collection-hosted/RESULTS.json").read_bytes()).hexdigest() == pins["hostedResultsSha256"]
for path in (root / "returns").glob("*.template.json"):
    form = json.loads(path.read_text(encoding="utf-8"))
    assert form["candidate"] == pins, path
    assert form["reviewerIdentity"] is None and form["decision"] is None, path
print("Verified exact hosted review inputs; all four owner forms remain unfilled drafts.")
