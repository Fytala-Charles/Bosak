from pathlib import Path
import hashlib
import json
import xml.etree.ElementTree as ET

base = Path(__file__).resolve().parents[1]
plan = json.loads((base / "PLAN.json").read_text(encoding="utf-8"))
schema = Path(plan["candidate"])
for name, expected in json.loads((base / "BASE-SOURCE-PINS.json").read_text(encoding="utf-8")).items():
    if name.endswith(".cs"):
        assert hashlib.sha256((schema / name).read_bytes()).hexdigest() == expected, name
refs = []
for name in ["src/Bosak.Schema/Bosak.Schema.csproj", "tests/Bosak.Schema.Tests/Bosak.Schema.Tests.csproj"]:
    for entry in ET.parse(schema / name).findall(".//PackageReference"):
        if entry.attrib["Include"].startswith("Bosak."):
            assert entry.attrib["Version"] == "1.81.2"
            refs.append(entry.attrib["Include"])
assert len(refs) == 5
results = json.loads((base / "RESULTS.json").read_text(encoding="utf-8"))
assert results["baselineCases"] == results["candidateSourceCases"] == results["candidateUnchangedBinaryCases"] == 124
assert results["distinctCandidateCases"] == 135
assert len(results["packageOutputComparisons"]) == 27
for name, entry in json.loads((base / "MANIFEST.json").read_text(encoding="utf-8")).items():
    path = (base / name).resolve()
    assert path.is_relative_to(base)
    raw = path.read_bytes()
    assert len(raw) == entry["bytes"] and hashlib.sha256(raw).hexdigest() == entry["sha256"], name
print("Verified isolated five-reference upgrade, unchanged Schema C# source,124 matching source/binary cases and11 extra integration/ownership cases.")

# The local committed checkpoint is a separate fresh qualification, not a relabeling.
import subprocess
checkpoint = json.loads((base / "committed/RESULTS.json").read_text(encoding="utf-8"))
assert subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=schema, text=True).strip() == checkpoint["schemaCommit"]
assert not subprocess.check_output(["git", "status", "--porcelain"], cwd=schema).strip()
assert checkpoint["freshCounts"]["solution"]["passed"] == "124"
assert checkpoint["freshCounts"]["integration"]["passed"] == "11"
assert len(checkpoint["packageOutputComparisons"]) == 18
print("Verified fresh immutable Schema checkpoint; remote delivery remains unperformed.")
