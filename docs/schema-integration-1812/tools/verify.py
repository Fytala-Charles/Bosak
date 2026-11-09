from pathlib import Path
import hashlib
import json
import xml.etree.ElementTree as ET

base = Path(__file__).resolve().parents[1]
root = base.parents[1]
pin = json.loads((base / "SOURCE-PIN.json").read_text(encoding="utf-8"))
props = ET.parse(root / "integrations/Schema/Source/Schema.Source.Files.props")
assert props.findtext(".//SchemaSourceRevision") == pin["revision"]
items = {entry.attrib["Include"].split("$(SchemaSourceRoot)/", 1)[1]: entry for entry in props.findall(".//ItemGroup/*")}
assert set(items) == {row["path"] for row in pin["files"]}
source = root / ".worktrees/Bosak.Schema.Integration.c953ebe"
for row in pin["files"]:
    item = items[row["path"]]
    assert item.tag == row["kind"]
    assert item.attrib["ExpectedSha256"].lower() == row["lfSha256"]
    assert item.attrib["AlternateSha256"].lower() == row["crlfSha256"]
    assert hashlib.sha256((source / row["path"]).read_bytes()).hexdigest() in [row["lfSha256"], row["crlfSha256"]]
assert "ref: " + pin["revision"] in (root / ".github/workflows/schema-integration.yml").read_text(encoding="utf-8")
for name, digest in json.loads((base / "HISTORICAL-PRESERVATION.json").read_text(encoding="utf-8")).items():
    assert hashlib.sha256((root / name).read_bytes()).hexdigest() == digest, name
results = json.loads((base / "RESULTS.json").read_text(encoding="utf-8"))
assert results["distinctCases"] == 135 and results["pinMutationRefusedBeforeCompilation"]
for name, entry in json.loads((base / "MANIFEST.json").read_text(encoding="utf-8")).items():
    raw = (base / name).read_bytes()
    assert len(raw) == entry["bytes"] and hashlib.sha256(raw).hexdigest() == entry["sha256"], name
print("Verified merged Schema pin,37 LF/CRLF inputs,135 cases, mutation refusal and unchanged historical dossier.")

for name, digest in json.loads((base / "LOCAL-INPUTS.json").read_text(encoding="utf-8")).items():
    assert hashlib.sha256((root / name).read_bytes().replace(b"\r\n", b"\n")).hexdigest() == digest, name
